using TMPro;
using UnityEngine;

public sealed class LifeUIController : MonoBehaviour
{
    [Header("Life UI")]
    [SerializeField] private TMP_Text livesText;

    [Tooltip(
        "Optional. Example output: 5/20. " +
        "Agar sirf current lives dikhani hain to Show Max Lives OFF kar dein."
    )]
    [SerializeField] private bool showMaxLives = true;


    [Header("Recovery Timer UI")]
    [SerializeField] private TMP_Text recoveryTimerText;

    [Tooltip(
        "Optional parent/container. Lives full hon to timer UI hide ho jayegi."
    )]
    [SerializeField] private GameObject recoveryTimerRoot;

    [SerializeField] private string fullLivesText = "FULL";


    [Header("Reference")]
    [SerializeField] private LifeManager lifeManager;


    private void OnEnable()
    {
        ResolveLifeManager();
        Subscribe();
        RefreshAll();
    }


    private void OnDisable()
    {
        Unsubscribe();
    }


    private void ResolveLifeManager()
    {
        if (lifeManager != null)
        {
            return;
        }

        if (LifeManager.Instance != null)
        {
            lifeManager =
                LifeManager.Instance;

            return;
        }

        lifeManager =
            FindFirstObjectByType<LifeManager>(
                FindObjectsInactive.Include
            );
    }


    private void Subscribe()
    {
        if (lifeManager == null)
        {
            return;
        }

        lifeManager.LivesChanged -=
            HandleLivesChanged;

        lifeManager.LivesChanged +=
            HandleLivesChanged;

        lifeManager.RecoveryTimerChanged -=
            HandleRecoveryTimerChanged;

        lifeManager.RecoveryTimerChanged +=
            HandleRecoveryTimerChanged;
    }


    private void Unsubscribe()
    {
        if (lifeManager == null)
        {
            return;
        }

        lifeManager.LivesChanged -=
            HandleLivesChanged;

        lifeManager.RecoveryTimerChanged -=
            HandleRecoveryTimerChanged;
    }


    private void RefreshAll()
    {
        if (lifeManager == null)
        {
            SetLivesText(
                0,
                0
            );

            SetRecoveryTimer(
                0f,
                true
            );

            return;
        }

        SetLivesText(
            lifeManager.CurrentLives,
            lifeManager.MaxLives
        );

        SetRecoveryTimer(
            lifeManager.GetRemainingRecoverySeconds(),
            lifeManager.IsFull
        );
    }


    private void HandleLivesChanged(
        int currentLives,
        int maxLives)
    {
        SetLivesText(
            currentLives,
            maxLives
        );

        if (lifeManager != null)
        {
            SetRecoveryTimer(
                lifeManager.GetRemainingRecoverySeconds(),
                lifeManager.IsFull
            );
        }
    }


    private void HandleRecoveryTimerChanged(
        float remainingSeconds)
    {
        bool isFull =
            lifeManager != null &&
            lifeManager.IsFull;

        SetRecoveryTimer(
            remainingSeconds,
            isFull
        );
    }


    private void SetLivesText(
        int currentLives,
        int maxLives)
    {
        if (livesText == null)
        {
            return;
        }

        if (showMaxLives)
        {
            livesText.text =
                $"{currentLives}/{maxLives}";
        }
        else
        {
            livesText.text =
                currentLives.ToString();
        }
    }


    private void SetRecoveryTimer(
        float remainingSeconds,
        bool isFull)
    {
        if (recoveryTimerRoot != null)
        {
            recoveryTimerRoot.SetActive(
                !isFull
            );
        }

        if (recoveryTimerText == null)
        {
            return;
        }

        if (isFull)
        {
            recoveryTimerText.text =
                fullLivesText;

            return;
        }

        int totalSeconds =
            Mathf.Max(
                0,
                Mathf.CeilToInt(
                    remainingSeconds
                )
            );

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        recoveryTimerText.text =
            $"{minutes:00}:{seconds:00}";
    }
}
