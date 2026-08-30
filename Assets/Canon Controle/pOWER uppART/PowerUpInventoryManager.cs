// using System;
// using TMPro;
// using UnityEngine;

// public sealed class PowerUpInventoryManager : MonoBehaviour
// {
//     private const string InfiniteBallsCountKey =
//         "RoyalSmash.PowerUps.InfiniteBalls.Count";

//     private const string PowerCannonCountKey =
//         "RoyalSmash.PowerUps.PowerCannon.Count";

//     private const string InfiniteBallsStarterGrantedKey =
//         "RoyalSmash.PowerUps.InfiniteBalls.StarterGranted";

//     private const string PowerCannonStarterGrantedKey =
//         "RoyalSmash.PowerUps.PowerCannon.StarterGranted";


//     public static PowerUpInventoryManager Instance
//     {
//         get;
//         private set;
//     }


//     [Header("Unlock Levels")]

//     [SerializeField, Min(1)]
//     private int infiniteBallsUnlockLevel = 5;

//     [SerializeField, Min(1)]
//     private int powerCannonUnlockLevel = 8;


//     [Header("Starter Amount")]

//     [SerializeField, Min(1)]
//     private int infiniteBallsStarterAmount = 2;

//     [SerializeField, Min(1)]
//     private int powerCannonStarterAmount = 2;


//     [Header("Gameplay Option Roots")]

//     [Tooltip(
//         "Infinite Balls ka complete gameplay option/button root. " +
//         "Level 5 se pehle hidden rahega."
//     )]
//     [SerializeField]
//     private GameObject infiniteBallsOptionRoot;

//     [Tooltip(
//         "Power Cannon ka complete gameplay option/button root. " +
//         "Level 8 se pehle hidden rahega."
//     )]
//     [SerializeField]
//     private GameObject powerCannonOptionRoot;


//     [Header("Optional Count Texts")]

//     [SerializeField]
//     private TMP_Text infiniteBallsCountText;

//     [SerializeField]
//     private TMP_Text powerCannonCountText;


//     [Header("References")]

//     [SerializeField]
//     private LevelRuntimeController levelRuntimeController;


//     [Header("Behaviour")]

//     [SerializeField]
//     private bool dontDestroyOnLoad = true;


//     private int currentLevelNumber = 1;


//     public int InfiniteBallsCount =>
//         PlayerPrefs.GetInt(
//             InfiniteBallsCountKey,
//             0
//         );

//     public int PowerCannonCount =>
//         PlayerPrefs.GetInt(
//             PowerCannonCountKey,
//             0
//         );

//     public bool InfiniteBallsUnlocked =>
//         currentLevelNumber >=
//         infiniteBallsUnlockLevel;

//     public bool PowerCannonUnlocked =>
//         currentLevelNumber >=
//         powerCannonUnlockLevel;


//     public event Action<int> InfiniteBallsCountChanged;
//     public event Action<int> PowerCannonCountChanged;

//     public event Action InfiniteBallsUnlockedEvent;
//     public event Action PowerCannonUnlockedEvent;


//     private void Awake()
//     {
//         if (Instance != null &&
//             Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         Instance = this;

//         if (dontDestroyOnLoad)
//         {
//             DontDestroyOnLoad(
//                 gameObject
//             );
//         }

//         ResolveReferences();
//         RefreshUI();
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         SubscribeToLevelEvents();

//         TryApplyCurrentLoadedLevel();
//     }


//     private void OnDisable()
//     {
//         UnsubscribeFromLevelEvents();
//     }


//     private void ResolveReferences()
//     {
//         if (levelRuntimeController == null)
//         {
//             levelRuntimeController =
//                 FindFirstObjectByType<LevelRuntimeController>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     private void SubscribeToLevelEvents()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelGenerated -=
//             HandleLevelGenerated;

//         levelRuntimeController.LevelGenerated +=
//             HandleLevelGenerated;
//     }


//     private void UnsubscribeFromLevelEvents()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelGenerated -=
//             HandleLevelGenerated;
//     }


//     private void TryApplyCurrentLoadedLevel()
//     {
//         if (levelRuntimeController == null ||
//             !levelRuntimeController.IsLevelGenerated ||
//             levelRuntimeController.CurrentLevelData == null)
//         {
//             SetOptionVisibility();
//             RefreshUI();
//             return;
//         }

//         ApplyLevel(
//             levelRuntimeController
//                 .CurrentLevelData
//                 .LevelNumber
//         );
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         if (generatedLevel == null)
//         {
//             return;
//         }

//         ApplyLevel(
//             generatedLevel.LevelNumber
//         );
//     }


//     private void ApplyLevel(
//         int levelNumber)
//     {
//         currentLevelNumber =
//             Mathf.Max(
//                 1,
//                 levelNumber
//             );

//         bool infiniteWasGranted =
//             HasInfiniteStarterBeenGranted();

//         bool cannonWasGranted =
//             HasPowerCannonStarterBeenGranted();


//         if (InfiniteBallsUnlocked &&
//             !infiniteWasGranted)
//         {
//             MarkInfiniteStarterGranted();

//             AddInfiniteBalls(
//                 infiniteBallsStarterAmount
//             );

//             InfiniteBallsUnlockedEvent?.Invoke();
//         }


//         if (PowerCannonUnlocked &&
//             !cannonWasGranted)
//         {
//             MarkPowerCannonStarterGranted();

//             AddPowerCannon(
//                 powerCannonStarterAmount
//             );

//             PowerCannonUnlockedEvent?.Invoke();
//         }


//         SetOptionVisibility();
//         RefreshUI();
//     }


//     private void SetOptionVisibility()
//     {
//         if (infiniteBallsOptionRoot != null)
//         {
//             infiniteBallsOptionRoot.SetActive(
//                 InfiniteBallsUnlocked
//             );
//         }

//         if (powerCannonOptionRoot != null)
//         {
//             powerCannonOptionRoot.SetActive(
//                 PowerCannonUnlocked
//             );
//         }
//     }


//     private void RefreshUI()
//     {
//         if (infiniteBallsCountText != null)
//         {
//             infiniteBallsCountText.text =
//                 $"x{InfiniteBallsCount}";
//         }

//         if (powerCannonCountText != null)
//         {
//             powerCannonCountText.text =
//                 $"x{PowerCannonCount}";
//         }
//     }


//     public void AddInfiniteBalls(
//         int amount = 1)
//     {
//         if (amount <= 0)
//         {
//             return;
//         }

//         int newCount =
//             InfiniteBallsCount +
//             amount;

//         PlayerPrefs.SetInt(
//             InfiniteBallsCountKey,
//             newCount
//         );

//         PlayerPrefs.Save();

//         RefreshUI();

//         InfiniteBallsCountChanged?.Invoke(
//             newCount
//         );
//     }


//     public void AddPowerCannon(
//         int amount = 1)
//     {
//         if (amount <= 0)
//         {
//             return;
//         }

//         int newCount =
//             PowerCannonCount +
//             amount;

//         PlayerPrefs.SetInt(
//             PowerCannonCountKey,
//             newCount
//         );

//         PlayerPrefs.Save();

//         RefreshUI();

//         PowerCannonCountChanged?.Invoke(
//             newCount
//         );
//     }


//     public bool TryConsumeInfiniteBalls(
//         int amount = 1)
//     {
//         if (amount <= 0)
//         {
//             return true;
//         }

//         int currentCount =
//             InfiniteBallsCount;

//         if (currentCount <
//             amount)
//         {
//             return false;
//         }

//         int newCount =
//             currentCount -
//             amount;

//         PlayerPrefs.SetInt(
//             InfiniteBallsCountKey,
//             newCount
//         );

//         PlayerPrefs.Save();

//         RefreshUI();

//         InfiniteBallsCountChanged?.Invoke(
//             newCount
//         );

//         return true;
//     }


//     public bool TryConsumePowerCannon(
//         int amount = 1)
//     {
//         if (amount <= 0)
//         {
//             return true;
//         }

//         int currentCount =
//             PowerCannonCount;

//         if (currentCount <
//             amount)
//         {
//             return false;
//         }

//         int newCount =
//             currentCount -
//             amount;

//         PlayerPrefs.SetInt(
//             PowerCannonCountKey,
//             newCount
//         );

//         PlayerPrefs.Save();

//         RefreshUI();

//         PowerCannonCountChanged?.Invoke(
//             newCount
//         );

//         return true;
//     }


//     private static bool HasInfiniteStarterBeenGranted()
//     {
//         return PlayerPrefs.GetInt(
//             InfiniteBallsStarterGrantedKey,
//             0
//         ) == 1;
//     }


//     private static bool HasPowerCannonStarterBeenGranted()
//     {
//         return PlayerPrefs.GetInt(
//             PowerCannonStarterGrantedKey,
//             0
//         ) == 1;
//     }


//     private static void MarkInfiniteStarterGranted()
//     {
//         PlayerPrefs.SetInt(
//             InfiniteBallsStarterGrantedKey,
//             1
//         );

//         PlayerPrefs.Save();
//     }


//     private static void MarkPowerCannonStarterGranted()
//     {
//         PlayerPrefs.SetInt(
//             PowerCannonStarterGrantedKey,
//             1
//         );

//         PlayerPrefs.Save();
//     }


//     [ContextMenu("DEBUG / Set Current Level 5")]
//     private void DebugSetLevel5()
//     {
//         ApplyLevel(5);
//     }


//     [ContextMenu("DEBUG / Set Current Level 8")]
//     private void DebugSetLevel8()
//     {
//         ApplyLevel(8);
//     }


//     [ContextMenu("DEBUG / Reset Power Up Unlock Data")]
//     private void DebugResetPowerUpUnlockData()
//     {
//         PlayerPrefs.DeleteKey(
//             InfiniteBallsCountKey
//         );

//         PlayerPrefs.DeleteKey(
//             PowerCannonCountKey
//         );

//         PlayerPrefs.DeleteKey(
//             InfiniteBallsStarterGrantedKey
//         );

//         PlayerPrefs.DeleteKey(
//             PowerCannonStarterGrantedKey
//         );

//         PlayerPrefs.Save();

//         currentLevelNumber = 1;

//         SetOptionVisibility();
//         RefreshUI();

//         InfiniteBallsCountChanged?.Invoke(0);
//         PowerCannonCountChanged?.Invoke(0);
//     }
// }








using System;
using TMPro;
using UnityEngine;

public sealed class PowerUpInventoryManager : MonoBehaviour
{
    private const string InfiniteBallsCountKey =
        "RoyalSmash.PowerUps.InfiniteBalls.Count";

    private const string PowerCannonCountKey =
        "RoyalSmash.PowerUps.PowerCannon.Count";

    private const string InfiniteBallsStarterGrantedKey =
        "RoyalSmash.PowerUps.InfiniteBalls.StarterGranted";

    private const string PowerCannonStarterGrantedKey =
        "RoyalSmash.PowerUps.PowerCannon.StarterGranted";


    public static PowerUpInventoryManager Instance
    {
        get;
        private set;
    }


    [Header("Unlock Levels")]

    /*
     * TESTING: filhal dono power-ups Level 1 se hi unlocked hain.
     *
     * Final progression ke liye ye wapas 5 (Infinite Balls) aur
     * 8 (Power Cannon) karne hain — scene ke PowerUpInventoryManager
     * par bhi wahi values set karni hongi, warna serialized value
     * code default ko override kar deti hai.
     */
    [SerializeField, Min(1)]
    private int infiniteBallsUnlockLevel = 1;

    [SerializeField, Min(1)]
    private int powerCannonUnlockLevel = 1;


    [Header("Starter Amount")]

    [SerializeField, Min(1)]
    private int infiniteBallsStarterAmount = 2;

    [SerializeField, Min(1)]
    private int powerCannonStarterAmount = 2;


    [Header("Gameplay Option Roots")]

    [Tooltip(
        "Infinite Balls ka complete gameplay option/button root. " +
        "Ye hamesha visible rahega, lekin apne Unlock Level se " +
        "pehle use nahi hoga."
    )]
    [SerializeField]
    private GameObject infiniteBallsOptionRoot;

    [Tooltip(
        "Power Cannon ka complete gameplay option/button root. " +
        "Ye hamesha visible rahega, lekin apne Unlock Level se " +
        "pehle use nahi hoga."
    )]
    [SerializeField]
    private GameObject powerCannonOptionRoot;


    [Header("Optional Count Texts")]

    [SerializeField]
    private TMP_Text infiniteBallsCountText;

    [SerializeField]
    private TMP_Text powerCannonCountText;


    [Header("References")]

    [SerializeField]
    private LevelRuntimeController levelRuntimeController;


    [Header("Behaviour")]

    [SerializeField]
    private bool dontDestroyOnLoad = true;


    private int currentLevelNumber = 1;


    public int InfiniteBallsCount =>
        PlayerPrefs.GetInt(
            InfiniteBallsCountKey,
            0
        );

    public int PowerCannonCount =>
        PlayerPrefs.GetInt(
            PowerCannonCountKey,
            0
        );

    public bool InfiniteBallsUnlocked =>
        currentLevelNumber >=
        infiniteBallsUnlockLevel;

    public bool PowerCannonUnlocked =>
        currentLevelNumber >=
        powerCannonUnlockLevel;


    public event Action<int> InfiniteBallsCountChanged;
    public event Action<int> PowerCannonCountChanged;

    public event Action InfiniteBallsUnlockedEvent;
    public event Action PowerCannonUnlockedEvent;


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
            DontDestroyOnLoad(
                gameObject
            );
        }

        ResolveReferences();
        RefreshUI();
    }


    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToLevelEvents();

        TryApplyCurrentLoadedLevel();
    }


    private void OnDisable()
    {
        UnsubscribeFromLevelEvents();
    }


    private void ResolveReferences()
    {
        if (levelRuntimeController == null)
        {
            levelRuntimeController =
                FindFirstObjectByType<LevelRuntimeController>(
                    FindObjectsInactive.Include
                );
        }
    }


    private void SubscribeToLevelEvents()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        levelRuntimeController.LevelGenerated -=
            HandleLevelGenerated;

        levelRuntimeController.LevelGenerated +=
            HandleLevelGenerated;
    }


    private void UnsubscribeFromLevelEvents()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        levelRuntimeController.LevelGenerated -=
            HandleLevelGenerated;
    }


    private void TryApplyCurrentLoadedLevel()
    {
        if (levelRuntimeController == null ||
            !levelRuntimeController.IsLevelGenerated ||
            levelRuntimeController.CurrentLevelData == null)
        {
            SetOptionVisibility();
            RefreshUI();
            return;
        }

        ApplyLevel(
            levelRuntimeController
                .CurrentLevelData
                .LevelNumber
        );
    }


    private void HandleLevelGenerated(
        GridLevelData generatedLevel)
    {
        if (generatedLevel == null)
        {
            return;
        }

        ApplyLevel(
            generatedLevel.LevelNumber
        );
    }


    private void ApplyLevel(
        int levelNumber)
    {
        currentLevelNumber =
            Mathf.Max(
                1,
                levelNumber
            );

        bool infiniteWasGranted =
            HasInfiniteStarterBeenGranted();

        bool cannonWasGranted =
            HasPowerCannonStarterBeenGranted();


        if (InfiniteBallsUnlocked &&
            !infiniteWasGranted)
        {
            MarkInfiniteStarterGranted();

            AddInfiniteBalls(
                infiniteBallsStarterAmount
            );

            InfiniteBallsUnlockedEvent?.Invoke();
        }


        if (PowerCannonUnlocked &&
            !cannonWasGranted)
        {
            MarkPowerCannonStarterGranted();

            AddPowerCannon(
                powerCannonStarterAmount
            );

            PowerCannonUnlockedEvent?.Invoke();
        }


        SetOptionVisibility();
        RefreshUI();
    }


    private void SetOptionVisibility()
    {
        /*
         * Dono power-up options gameplay UI par hamesha visible rahenge.
         *
         * Unlock se pehle:
         * - option visible rahega
         * - inventory/count visible reh sakta hai
         * - GameplayPowerUpController button ko use nahi karne dega
         *
         * Actual use har power-up ke apne Unlock Level se shuru hota
         * hai (filhal testing ke liye dono Level 1 par hain).
         *
         * Daily Reward se pehle milne wali quantity save/show ho sakti hai,
         * lekin unlock level se pehle use nahi hogi.
         */

        if (infiniteBallsOptionRoot != null)
        {
            infiniteBallsOptionRoot.SetActive(
                true
            );
        }

        if (powerCannonOptionRoot != null)
        {
            powerCannonOptionRoot.SetActive(
                true
            );
        }
    }


    private void RefreshUI()
    {
        if (infiniteBallsCountText != null)
        {
            infiniteBallsCountText.text =
                $"x{InfiniteBallsCount}";
        }

        if (powerCannonCountText != null)
        {
            powerCannonCountText.text =
                $"x{PowerCannonCount}";
        }
    }


    public void AddInfiniteBalls(
        int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        int newCount =
            InfiniteBallsCount +
            amount;

        PlayerPrefs.SetInt(
            InfiniteBallsCountKey,
            newCount
        );

        PlayerPrefs.Save();

        RefreshUI();

        InfiniteBallsCountChanged?.Invoke(
            newCount
        );
    }


    public void AddPowerCannon(
        int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        int newCount =
            PowerCannonCount +
            amount;

        PlayerPrefs.SetInt(
            PowerCannonCountKey,
            newCount
        );

        PlayerPrefs.Save();

        RefreshUI();

        PowerCannonCountChanged?.Invoke(
            newCount
        );
    }


    public bool TryConsumeInfiniteBalls(
        int amount = 1)
    {
        if (amount <= 0)
        {
            return true;
        }

        int currentCount =
            InfiniteBallsCount;

        if (currentCount <
            amount)
        {
            return false;
        }

        int newCount =
            currentCount -
            amount;

        PlayerPrefs.SetInt(
            InfiniteBallsCountKey,
            newCount
        );

        PlayerPrefs.Save();

        RefreshUI();

        InfiniteBallsCountChanged?.Invoke(
            newCount
        );

        return true;
    }


    public bool TryConsumePowerCannon(
        int amount = 1)
    {
        if (amount <= 0)
        {
            return true;
        }

        int currentCount =
            PowerCannonCount;

        if (currentCount <
            amount)
        {
            return false;
        }

        int newCount =
            currentCount -
            amount;

        PlayerPrefs.SetInt(
            PowerCannonCountKey,
            newCount
        );

        PlayerPrefs.Save();

        RefreshUI();

        PowerCannonCountChanged?.Invoke(
            newCount
        );

        return true;
    }


    private static bool HasInfiniteStarterBeenGranted()
    {
        return PlayerPrefs.GetInt(
            InfiniteBallsStarterGrantedKey,
            0
        ) == 1;
    }


    private static bool HasPowerCannonStarterBeenGranted()
    {
        return PlayerPrefs.GetInt(
            PowerCannonStarterGrantedKey,
            0
        ) == 1;
    }


    private static void MarkInfiniteStarterGranted()
    {
        PlayerPrefs.SetInt(
            InfiniteBallsStarterGrantedKey,
            1
        );

        PlayerPrefs.Save();
    }


    private static void MarkPowerCannonStarterGranted()
    {
        PlayerPrefs.SetInt(
            PowerCannonStarterGrantedKey,
            1
        );

        PlayerPrefs.Save();
    }


    [ContextMenu("DEBUG / Set Current Level 5")]
    private void DebugSetLevel5()
    {
        ApplyLevel(5);
    }


    [ContextMenu("DEBUG / Set Current Level 8")]
    private void DebugSetLevel8()
    {
        ApplyLevel(8);
    }


    [ContextMenu("DEBUG / Reset Power Up Unlock Data")]
    private void DebugResetPowerUpUnlockData()
    {
        PlayerPrefs.DeleteKey(
            InfiniteBallsCountKey
        );

        PlayerPrefs.DeleteKey(
            PowerCannonCountKey
        );

        PlayerPrefs.DeleteKey(
            InfiniteBallsStarterGrantedKey
        );

        PlayerPrefs.DeleteKey(
            PowerCannonStarterGrantedKey
        );

        PlayerPrefs.Save();

        currentLevelNumber = 1;

        SetOptionVisibility();
        RefreshUI();

        InfiniteBallsCountChanged?.Invoke(0);
        PowerCannonCountChanged?.Invoke(0);
    }
}










