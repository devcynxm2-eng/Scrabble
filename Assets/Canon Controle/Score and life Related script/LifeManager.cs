using System;
using UnityEngine;

public sealed class LifeManager : MonoBehaviour
{
    private const string LivesKey =
        "RoyalSmash.CurrentLives";

    private const string NextRecoveryUtcTicksKey =
        "RoyalSmash.NextLifeRecoveryUtcTicks";

    private const string InitializedKey =
        "RoyalSmash.LifeSystemInitialized";


    public static LifeManager Instance { get; private set; }


    [Header("Life Settings")]
    [SerializeField, Min(0)]
    private int startingLives = 5;

    [SerializeField, Min(1)]
    private int maxLives = 20;

    [SerializeField, Min(1)]
    private int recoveryAmount = 5;

    [Tooltip("Default 300 seconds = 5 minutes.")]
    [SerializeField, Min(1f)]
    private float recoveryIntervalSeconds = 300f;


    [Header("Behaviour")]
    [SerializeField]
    private bool dontDestroyOnLoad = true;

    [Tooltip(
        "Timer UI ko har frame update karne ki zarurat nahi. " +
        "Default 0.25 second par event refresh hota hai."
    )]
    [SerializeField, Min(0.05f)]
    private float timerEventRefreshInterval = 0.25f;


    private int currentLives;

    /*
     * UTC ticks use kar rahe hain taake:
     * - Game close hone par timer continue rahe.
     * - Scene change se timer reset na ho.
     * - Time.timeScale = 0 par bhi recovery chale.
     */
    private long nextRecoveryUtcTicks;

    private float nextTimerEventRefreshAt;


    public int CurrentLives =>
        currentLives;

    public int MaxLives =>
        maxLives;

    public int RecoveryAmount =>
        recoveryAmount;

    public float RecoveryIntervalSeconds =>
        recoveryIntervalSeconds;

    public bool HasLives =>
        currentLives > 0;

    public bool IsFull =>
        currentLives >= maxLives;

    public bool IsRecoveryRunning =>
        currentLives < maxLives &&
        nextRecoveryUtcTicks > 0;


    public event Action<int, int> LivesChanged;

    /*
     * remainingSeconds:
     * - > 0  = next recovery tak remaining time
     * - 0    = full lives ya timer inactive
     */
    public event Action<float> RecoveryTimerChanged;


    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        SanitizeInspectorValues();
        LoadOrCreateState();

        /*
         * Game start hote hi offline elapsed time apply karo.
         */
        ApplyElapsedRecovery();

        EnsureRecoveryTimerState();
        SaveState();
    }


    private void Start()
    {
        BroadcastAll();
    }


    private void Update()
    {
        if (IsFull)
        {
            if (nextRecoveryUtcTicks != 0)
            {
                StopRecoveryTimer();
                SaveState();
            }

            RefreshTimerEventIfNeeded();
            return;
        }

        if (nextRecoveryUtcTicks <= 0)
        {
            StartRecoveryTimerFromNow();
            SaveState();
        }

        /*
         * Realtime / UTC based check.
         * Isliye Pause screen par Time.timeScale = 0 ho tab bhi chalega.
         */
        if (DateTime.UtcNow.Ticks >=
            nextRecoveryUtcTicks)
        {
            ApplyElapsedRecovery();
        }

        RefreshTimerEventIfNeeded();
    }


    private void OnApplicationPause(
        bool paused)
    {
        if (paused)
        {
            SaveState();
            return;
        }

        ApplyElapsedRecovery();
        EnsureRecoveryTimerState();
        SaveState();
        BroadcastAll();
    }


    private void OnApplicationFocus(
        bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveState();
            return;
        }

        ApplyElapsedRecovery();
        EnsureRecoveryTimerState();
        SaveState();
        BroadcastAll();
    }


    private void OnApplicationQuit()
    {
        SaveState();
    }


    private void SanitizeInspectorValues()
    {
        maxLives =
            Mathf.Max(
                1,
                maxLives
            );

        startingLives =
            Mathf.Clamp(
                startingLives,
                0,
                maxLives
            );

        recoveryAmount =
            Mathf.Max(
                1,
                recoveryAmount
            );

        recoveryIntervalSeconds =
            Mathf.Max(
                1f,
                recoveryIntervalSeconds
            );

        timerEventRefreshInterval =
            Mathf.Max(
                0.05f,
                timerEventRefreshInterval
            );
    }


    private void LoadOrCreateState()
    {
        bool initialized =
            PlayerPrefs.GetInt(
                InitializedKey,
                0
            ) == 1;

        if (!initialized)
        {
            currentLives =
                Mathf.Clamp(
                    startingLives,
                    0,
                    maxLives
                );

            nextRecoveryUtcTicks = 0;

            if (currentLives < maxLives)
            {
                StartRecoveryTimerFromNow();
            }

            PlayerPrefs.SetInt(
                InitializedKey,
                1
            );

            SaveState();
            return;
        }

        currentLives =
            Mathf.Clamp(
                PlayerPrefs.GetInt(
                    LivesKey,
                    startingLives
                ),
                0,
                maxLives
            );

        string storedTicks =
            PlayerPrefs.GetString(
                NextRecoveryUtcTicksKey,
                "0"
            );

        if (!long.TryParse(
                storedTicks,
                out nextRecoveryUtcTicks))
        {
            nextRecoveryUtcTicks = 0;
        }
    }


    public bool CanConsumeLives(
        int amount = 1)
    {
        if (amount <= 0)
        {
            return true;
        }

        return currentLives >= amount;
    }


    public bool TryConsumeLife()
    {
        return TryConsumeLives(1);
    }


    public bool TryConsumeLives(
        int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        /*
         * Consume se pehle offline/realtime recovery apply kar do.
         * Is se exact boundary par player ko recover hui life mil sakti hai.
         */
        ApplyElapsedRecovery();

        if (!CanConsumeLives(amount))
        {
            BroadcastAll();
            return false;
        }

        bool wasFull =
            IsFull;

        currentLives -= amount;

        /*
         * Important:
         * Agar 20/20 se 19/20 hua hai to naya 5 minute timer ab start hoga.
         *
         * Agar pehle se 10/20 tha aur timer already chal raha tha,
         * consume karne par existing timer reset nahi hoga.
         */
        if (wasFull ||
            nextRecoveryUtcTicks <= 0)
        {
            StartRecoveryTimerFromNow();
        }

        SaveState();
        BroadcastAll();

        return true;
    }


    public void AddLives(
        int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        ApplyElapsedRecovery();

        int previousLives =
            currentLives;

        currentLives =
            Mathf.Min(
                maxLives,
                currentLives + amount
            );

        if (currentLives >= maxLives)
        {
            StopRecoveryTimer();
        }
        else if (nextRecoveryUtcTicks <= 0)
        {
            StartRecoveryTimerFromNow();
        }

        if (currentLives != previousLives)
        {
            SaveState();
            BroadcastAll();
        }
    }


    public void SetLives(
        int value)
    {
        currentLives =
            Mathf.Clamp(
                value,
                0,
                maxLives
            );

        if (IsFull)
        {
            StopRecoveryTimer();
        }
        else
        {
            StartRecoveryTimerFromNow();
        }

        SaveState();
        BroadcastAll();
    }


    public float GetRemainingRecoverySeconds()
    {
        if (IsFull ||
            nextRecoveryUtcTicks <= 0)
        {
            return 0f;
        }

        long remainingTicks =
            nextRecoveryUtcTicks -
            DateTime.UtcNow.Ticks;

        if (remainingTicks <= 0)
        {
            return 0f;
        }

        return (float)TimeSpan
            .FromTicks(remainingTicks)
            .TotalSeconds;
    }


    private void ApplyElapsedRecovery()
    {
        if (IsFull)
        {
            StopRecoveryTimer();
            return;
        }

        if (nextRecoveryUtcTicks <= 0)
        {
            StartRecoveryTimerFromNow();
            return;
        }

        long nowTicks =
            DateTime.UtcNow.Ticks;

        if (nowTicks <
            nextRecoveryUtcTicks)
        {
            return;
        }

        long intervalTicks =
            SecondsToTicks(
                recoveryIntervalSeconds
            );

        if (intervalTicks <= 0)
        {
            intervalTicks =
                TimeSpan.TicksPerSecond;
        }

        /*
         * Example:
         * nextRecovery = 10:05
         * now          = 10:16
         *
         * Due cycles:
         * 10:05
         * 10:10
         * 10:15
         * = 3 cycles
         */
        long overdueTicks =
            nowTicks -
            nextRecoveryUtcTicks;

        long additionalCycles =
            overdueTicks /
            intervalTicks;

        long dueCycles =
            1 +
            additionalCycles;

        int missingLives =
            maxLives -
            currentLives;

        int maximumUsefulCycles =
            Mathf.CeilToInt(
                missingLives /
                (float)recoveryAmount
            );

        int cyclesToApply =
            (int)Math.Min(
                dueCycles,
                maximumUsefulCycles
            );

        if (cyclesToApply <= 0)
        {
            return;
        }

        int previousLives =
            currentLives;

        currentLives =
            Mathf.Min(
                maxLives,
                currentLives +
                cyclesToApply *
                recoveryAmount
            );

        if (IsFull)
        {
            StopRecoveryTimer();
        }
        else
        {
            /*
             * Original timer phase preserve hoti hai.
             * Offline return par har recovery cycle ka proper elapsed time count hoga.
             */
            nextRecoveryUtcTicks +=
                cyclesToApply *
                intervalTicks;
        }

        SaveState();

        if (currentLives != previousLives)
        {
            BroadcastAll();
        }
    }


    private void EnsureRecoveryTimerState()
    {
        if (IsFull)
        {
            StopRecoveryTimer();
            return;
        }

        if (nextRecoveryUtcTicks <= 0)
        {
            StartRecoveryTimerFromNow();
        }
    }


    private void StartRecoveryTimerFromNow()
    {
        if (IsFull)
        {
            StopRecoveryTimer();
            return;
        }

        nextRecoveryUtcTicks =
            DateTime.UtcNow.Ticks +
            SecondsToTicks(
                recoveryIntervalSeconds
            );
    }


    private void StopRecoveryTimer()
    {
        nextRecoveryUtcTicks = 0;
    }


    private static long SecondsToTicks(
        float seconds)
    {
        double safeSeconds =
            Math.Max(
                1d,
                seconds
            );

        return TimeSpan
            .FromSeconds(safeSeconds)
            .Ticks;
    }


    private void RefreshTimerEventIfNeeded()
    {
        if (Time.unscaledTime <
            nextTimerEventRefreshAt)
        {
            return;
        }

        nextTimerEventRefreshAt =
            Time.unscaledTime +
            timerEventRefreshInterval;

        RecoveryTimerChanged?.Invoke(
            GetRemainingRecoverySeconds()
        );
    }


    private void BroadcastAll()
    {
        LivesChanged?.Invoke(
            currentLives,
            maxLives
        );

        RecoveryTimerChanged?.Invoke(
            GetRemainingRecoverySeconds()
        );
    }


    private void SaveState()
    {
        PlayerPrefs.SetInt(
            LivesKey,
            currentLives
        );

        PlayerPrefs.SetString(
            NextRecoveryUtcTicksKey,
            nextRecoveryUtcTicks.ToString()
        );

        PlayerPrefs.SetInt(
            InitializedKey,
            1
        );

        PlayerPrefs.Save();
    }


    [ContextMenu("DEBUG / Add 5 Lives")]
    private void DebugAddFiveLives()
    {
        AddLives(5);
    }


    [ContextMenu("DEBUG / Consume 1 Life")]
    private void DebugConsumeOneLife()
    {
        TryConsumeLife();
    }


    [ContextMenu("DEBUG / Reset To Starting Lives")]
    private void DebugResetToStartingLives()
    {
        currentLives =
            Mathf.Clamp(
                startingLives,
                0,
                maxLives
            );

        if (IsFull)
        {
            StopRecoveryTimer();
        }
        else
        {
            StartRecoveryTimerFromNow();
        }

        SaveState();
        BroadcastAll();
    }
}
