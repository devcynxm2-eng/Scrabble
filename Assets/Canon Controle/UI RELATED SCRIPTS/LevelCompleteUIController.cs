// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public sealed class LevelCompleteUIController : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private GameObject levelCompletePanel;
//     [SerializeField] private Button continueButton;

//     [Tooltip(
//         "Optional TMP text. Assign karen agar popup par current completed " +
//         "level number automatically show karwana ho."
//     )]
//     [SerializeField] private TMP_Text levelCompleteText;


//     [Header("Gameplay Reference")]
//     [SerializeField] private LevelRuntimeController levelRuntimeController;


//     [Header("Behaviour")]
//     [SerializeField] private bool pauseGameWhenOpened = true;


//     private bool isOpen;
//     private bool isContinuing;


//     private void Awake()
//     {
//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         Subscribe();

//         if (continueButton != null)
//         {
//             continueButton.onClick.RemoveListener(ContinueToNextLevel);
//             continueButton.onClick.AddListener(ContinueToNextLevel);
//         }
//     }


//     private void OnDisable()
//     {
//         Unsubscribe();

//         if (continueButton != null)
//         {
//             continueButton.onClick.RemoveListener(ContinueToNextLevel);
//         }
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


//     private void Subscribe()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelCompleted -= HandleLevelCompleted;
//         levelRuntimeController.LevelCompleted += HandleLevelCompleted;

//         levelRuntimeController.LevelGenerated -= HandleLevelGenerated;
//         levelRuntimeController.LevelGenerated += HandleLevelGenerated;
//     }


//     private void Unsubscribe()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelCompleted -= HandleLevelCompleted;
//         levelRuntimeController.LevelGenerated -= HandleLevelGenerated;
//     }


//     private void HandleLevelCompleted(
//         GridLevelData completedLevel)
//     {
//         if (isOpen)
//         {
//             return;
//         }

//         isOpen = true;
//         isContinuing = false;

//         bool hasNextLevel =
//             levelRuntimeController != null &&
//             levelRuntimeController.HasNextLevel;

//         if (levelCompleteText != null)
//         {
//             if (completedLevel != null)
//             {
//                 levelCompleteText.text =
//                     hasNextLevel
//                         ? $"LEVEL {completedLevel.LevelNumber} COMPLETE"
//                         : "ALL LEVELS COMPLETE";
//             }
//             else
//             {
//                 levelCompleteText.text =
//                     hasNextLevel
//                         ? "LEVEL COMPLETE"
//                         : "ALL LEVELS COMPLETE";
//             }
//         }

//         if (continueButton != null)
//         {
//             continueButton.gameObject.SetActive(hasNextLevel);
//             continueButton.interactable = hasNextLevel;
//         }

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(true);
//         }

//         if (pauseGameWhenOpened)
//         {
//             Time.timeScale = 0f;
//         }
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         isOpen = false;
//         isContinuing = false;

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }
//     }


//     public void ContinueToNextLevel()
//     {
//         if (isContinuing ||
//             levelRuntimeController == null ||
//             !levelRuntimeController.HasNextLevel)
//         {
//             return;
//         }

//         isContinuing = true;

//         if (continueButton != null)
//         {
//             continueButton.interactable = false;
//         }

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }

//         isOpen = false;

//         Time.timeScale = 1f;

//         levelRuntimeController.LoadNextLevel();
//     }


//     public void Hide()
//     {
//         isOpen = false;
//         isContinuing = false;

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }
//     }
// }



// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public sealed class LevelCompleteUIController : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private GameObject levelCompletePanel;
//     [SerializeField] private Button continueButton;

//     [Tooltip(
//         "Optional TMP text. Completed level number show karne ke liye."
//     )]
//     [SerializeField] private TMP_Text levelCompleteText;

//     [Tooltip(
//         "Level complete par is level mein earn kiye coins/score show karega. " +
//         "Example: +100"
//     )]
//     [SerializeField] private TMP_Text earnedCoinsText;

//     [SerializeField] private string earnedCoinsPrefix = "+";


//     [Header("Gameplay References")]
//     [SerializeField] private LevelRuntimeController levelRuntimeController;
//     [SerializeField] private ScoreManager scoreManager;


//     [Header("Behaviour")]
//     [SerializeField] private bool pauseGameWhenOpened = true;


//     private bool isOpen;
//     private bool isContinuing;


//     private void Awake()
//     {
//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         Subscribe();

//         if (continueButton != null)
//         {
//             continueButton.onClick.RemoveListener(
//                 ContinueToNextLevel
//             );

//             continueButton.onClick.AddListener(
//                 ContinueToNextLevel
//             );
//         }
//     }


//     private void OnDisable()
//     {
//         Unsubscribe();

//         if (continueButton != null)
//         {
//             continueButton.onClick.RemoveListener(
//                 ContinueToNextLevel
//             );
//         }
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

//         if (scoreManager == null)
//         {
//             if (ScoreManager.Instance != null)
//             {
//                 scoreManager =
//                     ScoreManager.Instance;
//             }
//             else
//             {
//                 scoreManager =
//                     FindFirstObjectByType<ScoreManager>(
//                         FindObjectsInactive.Include
//                     );
//             }
//         }
//     }


//     private void Subscribe()
//     {
//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.LevelCompleted -=
//                 HandleLevelCompleted;

//             levelRuntimeController.LevelCompleted +=
//                 HandleLevelCompleted;

//             levelRuntimeController.LevelGenerated -=
//                 HandleLevelGenerated;

//             levelRuntimeController.LevelGenerated +=
//                 HandleLevelGenerated;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreAdded -=
//                 HandleScoreAdded;

//             scoreManager.ScoreAdded +=
//                 HandleScoreAdded;
//         }
//     }


//     private void Unsubscribe()
//     {
//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.LevelCompleted -=
//                 HandleLevelCompleted;

//             levelRuntimeController.LevelGenerated -=
//                 HandleLevelGenerated;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreAdded -=
//                 HandleScoreAdded;
//         }
//     }


//     private void HandleLevelCompleted(
//         GridLevelData completedLevel)
//     {
//         if (isOpen)
//         {
//             return;
//         }

//         isOpen = true;
//         isContinuing = false;

//         bool hasNextLevel =
//             levelRuntimeController != null &&
//             levelRuntimeController.HasNextLevel;

//         if (levelCompleteText != null)
//         {
//             if (completedLevel != null)
//             {
//                 levelCompleteText.text =
//                     hasNextLevel
//                         ? $"LEVEL {completedLevel.LevelNumber} COMPLETE"
//                         : "ALL LEVELS COMPLETE";
//             }
//             else
//             {
//                 levelCompleteText.text =
//                     hasNextLevel
//                         ? "LEVEL COMPLETE"
//                         : "ALL LEVELS COMPLETE";
//             }
//         }

//         if (earnedCoinsText != null)
//         {
//             int fallbackEarned =
//                 scoreManager != null
//                     ? scoreManager.PointsPerLevel
//                     : 0;

//             SetEarnedCoinsText(
//                 fallbackEarned
//             );
//         }

//         if (continueButton != null)
//         {
//             continueButton.gameObject.SetActive(
//                 hasNextLevel
//             );

//             continueButton.interactable =
//                 hasNextLevel;
//         }

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(true);
//         }

//         if (pauseGameWhenOpened)
//         {
//             Time.timeScale = 0f;
//         }
//     }


//     private void HandleScoreAdded(
//         int amountAdded,
//         int newTotalScore)
//     {
//         if (!isOpen)
//         {
//             return;
//         }

//         SetEarnedCoinsText(
//             amountAdded
//         );
//     }


//     private void SetEarnedCoinsText(
//         int amount)
//     {
//         if (earnedCoinsText == null)
//         {
//             return;
//         }

//         earnedCoinsText.text =
//             $"{earnedCoinsPrefix}{amount}";
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         isOpen = false;
//         isContinuing = false;

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }
//     }


//     public void ContinueToNextLevel()
//     {
//         if (isContinuing ||
//             levelRuntimeController == null ||
//             !levelRuntimeController.HasNextLevel)
//         {
//             return;
//         }

//         isContinuing = true;

//         if (continueButton != null)
//         {
//             continueButton.interactable = false;
//         }

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }

//         isOpen = false;

//         Time.timeScale = 1f;

//         levelRuntimeController.LoadNextLevel();
//     }


//     public void Hide()
//     {
//         isOpen = false;
//         isContinuing = false;

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }
//     }
// }



// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public sealed class LevelCompleteUIController : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private GameObject levelCompletePanel;
//     [SerializeField] private Button continueButton;

//     [Tooltip(
//         "Optional TMP text. Completed level number show karne ke liye."
//     )]
//     [SerializeField] private TMP_Text levelCompleteText;

//     [Tooltip(
//         "Level complete par is level mein earn kiye coins/score show karega. " +
//         "Example: +100"
//     )]
//     [SerializeField] private TMP_Text earnedCoinsText;

//     [SerializeField] private string earnedCoinsPrefix = "+";


//     [Header("Stars")]
//     [SerializeField] private Image star1;
//     [SerializeField] private Image star2;
//     [SerializeField] private Image star3;

//     [Tooltip("Earned star ka Image color.")]
//     [SerializeField] private Color earnedStarColor = Color.white;

//     [Tooltip("Unearned star ka Image color / alpha.")]
//     [SerializeField] private Color unearnedStarColor =
//         new Color(1f, 1f, 1f, 0.25f);


//     [Header("Gameplay References")]
//     [SerializeField] private LevelRuntimeController levelRuntimeController;
//     [SerializeField] private ScoreManager scoreManager;
//     [SerializeField] private StarRatingManager starRatingManager;


//     [Header("Behaviour")]
//     [SerializeField] private bool pauseGameWhenOpened = true;


//     private bool isOpen;
//     private bool isContinuing;


//     private void Awake()
//     {
//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         Subscribe();

//         if (continueButton != null)
//         {
//             continueButton.onClick.RemoveListener(
//                 ContinueToNextLevel
//             );

//             continueButton.onClick.AddListener(
//                 ContinueToNextLevel
//             );
//         }
//     }


//     private void OnDisable()
//     {
//         Unsubscribe();

//         if (continueButton != null)
//         {
//             continueButton.onClick.RemoveListener(
//                 ContinueToNextLevel
//             );
//         }
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

//         if (scoreManager == null)
//         {
//             if (ScoreManager.Instance != null)
//             {
//                 scoreManager =
//                     ScoreManager.Instance;
//             }
//             else
//             {
//                 scoreManager =
//                     FindFirstObjectByType<ScoreManager>(
//                         FindObjectsInactive.Include
//                     );
//             }
//         }

//         if (starRatingManager == null)
//         {
//             if (StarRatingManager.Instance != null)
//             {
//                 starRatingManager =
//                     StarRatingManager.Instance;
//             }
//             else
//             {
//                 starRatingManager =
//                     FindFirstObjectByType<StarRatingManager>(
//                         FindObjectsInactive.Include
//                     );
//             }
//         }
//     }


//     private void Subscribe()
//     {
//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.LevelCompleted -=
//                 HandleLevelCompleted;

//             levelRuntimeController.LevelCompleted +=
//                 HandleLevelCompleted;

//             levelRuntimeController.LevelGenerated -=
//                 HandleLevelGenerated;

//             levelRuntimeController.LevelGenerated +=
//                 HandleLevelGenerated;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreAdded -=
//                 HandleScoreAdded;

//             scoreManager.ScoreAdded +=
//                 HandleScoreAdded;
//         }

//         if (starRatingManager != null)
//         {
//             starRatingManager.StarsAwarded -=
//                 HandleStarsAwarded;

//             starRatingManager.StarsAwarded +=
//                 HandleStarsAwarded;
//         }
//     }


//     private void Unsubscribe()
//     {
//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.LevelCompleted -=
//                 HandleLevelCompleted;

//             levelRuntimeController.LevelGenerated -=
//                 HandleLevelGenerated;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreAdded -=
//                 HandleScoreAdded;
//         }

//         if (starRatingManager != null)
//         {
//             starRatingManager.StarsAwarded -=
//                 HandleStarsAwarded;
//         }
//     }


//     private void HandleLevelCompleted(
//         GridLevelData completedLevel)
//     {
//         if (isOpen)
//         {
//             return;
//         }

//         isOpen = true;
//         isContinuing = false;

//         bool hasNextLevel =
//             levelRuntimeController != null &&
//             levelRuntimeController.HasNextLevel;

//         if (levelCompleteText != null)
//         {
//             if (completedLevel != null)
//             {
//                 levelCompleteText.text =
//                     hasNextLevel
//                         ? $"LEVEL {completedLevel.LevelNumber} COMPLETE"
//                         : "ALL LEVELS COMPLETE";
//             }
//             else
//             {
//                 levelCompleteText.text =
//                     hasNextLevel
//                         ? "LEVEL COMPLETE"
//                         : "ALL LEVELS COMPLETE";
//             }
//         }

//         /*
//          * ScoreManager isi LevelCompleted event par score add karta hai.
//          * ScoreAdded event thori der mein exact earned amount bhej dega.
//          *
//          * Fallback ke liye current PointsPerLevel pehle hi show kar dete hain.
//          */
//         if (earnedCoinsText != null)
//         {
//             int fallbackEarned =
//                 scoreManager != null
//                     ? scoreManager.PointsPerLevel
//                     : 0;

//             SetEarnedCoinsText(
//                 fallbackEarned
//             );
//         }

//         /*
//          * Event subscription order ki wajah se StarRatingManager ne same
//          * LevelCompleted event par rating pehle ya baad mein calculate ki ho
//          * sakti hai. Current value show karte hain; StarsAwarded event exact
//          * result aate hi visuals refresh kar dega.
//          */
//         int currentStars =
//             starRatingManager != null
//                 ? starRatingManager.LastAwardedStars
//                 : 0;

//         SetStarVisuals(
//             currentStars
//         );

//         if (continueButton != null)
//         {
//             continueButton.gameObject.SetActive(
//                 hasNextLevel
//             );

//             continueButton.interactable =
//                 hasNextLevel;
//         }

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(true);
//         }

//         if (pauseGameWhenOpened)
//         {
//             Time.timeScale = 0f;
//         }
//     }


//     private void HandleScoreAdded(
//         int amountAdded,
//         int newTotalScore)
//     {
//         if (!isOpen)
//         {
//             return;
//         }

//         SetEarnedCoinsText(
//             amountAdded
//         );
//     }


//     private void SetEarnedCoinsText(
//         int amount)
//     {
//         if (earnedCoinsText == null)
//         {
//             return;
//         }

//         earnedCoinsText.text =
//             $"{earnedCoinsPrefix}{amount}";
//     }


//     private void HandleStarsAwarded(
//         int stars)
//     {
//         if (!isOpen)
//         {
//             return;
//         }

//         SetStarVisuals(
//             stars
//         );
//     }


//     private void SetStarVisuals(
//         int stars)
//     {
//         int safeStars =
//             Mathf.Clamp(
//                 stars,
//                 0,
//                 3
//             );

//         SetSingleStarVisual(
//             star1,
//             safeStars >= 1
//         );

//         SetSingleStarVisual(
//             star2,
//             safeStars >= 2
//         );

//         SetSingleStarVisual(
//             star3,
//             safeStars >= 3
//         );
//     }


//     private void SetSingleStarVisual(
//         Image starImage,
//         bool earned)
//     {
//         if (starImage == null)
//         {
//             return;
//         }

//         starImage.color =
//             earned
//                 ? earnedStarColor
//                 : unearnedStarColor;
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         isOpen = false;
//         isContinuing = false;

//         SetStarVisuals(0);

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }
//     }


//     public void ContinueToNextLevel()
//     {
//         if (isContinuing ||
//             levelRuntimeController == null ||
//             !levelRuntimeController.HasNextLevel)
//         {
//             return;
//         }

//         isContinuing = true;

//         if (continueButton != null)
//         {
//             continueButton.interactable = false;
//         }

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }

//         isOpen = false;

//         Time.timeScale = 1f;

//         levelRuntimeController.LoadNextLevel();
//     }


//     public void Hide()
//     {
//         isOpen = false;
//         isContinuing = false;

//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(false);
//         }
//     }
// }





using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LevelCompleteUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Button continueButton;

    [Tooltip(
        "Optional TMP text. Completed level number show karne ke liye."
    )]
    [SerializeField] private TMP_Text levelCompleteText;

    [Tooltip(
        "Level complete par is level mein earn kiye coins/score show karega. " +
        "Example: +100"
    )]
    [SerializeField] private TMP_Text earnedCoinsText;

    [SerializeField] private string earnedCoinsPrefix = "+";


    [Header("Stars")]
    [SerializeField] private Image star1;
    [SerializeField] private Image star2;
    [SerializeField] private Image star3;

    [Tooltip("Earned star ka Image color.")]
    [SerializeField] private Color earnedStarColor = Color.white;

    [Tooltip("Unearned star ka Image color / alpha.")]
    [SerializeField] private Color unearnedStarColor =
        new Color(0.32f, 0.32f, 0.32f, 0.7f);

    [Header("Star Reveal Animation")]

    [SerializeField]
    private bool animateStarReveal = true;

    [SerializeField, Min(0f)]
    private float starRevealInitialDelay = 0.12f;

    [SerializeField, Min(0.01f)]
    private float starPopDuration = 0.24f;

    [SerializeField, Min(0f)]
    private float starRevealStagger = 0.08f;

    [SerializeField, Range(0.05f, 1f)]
    private float starStartScale = 0.2f;

    [SerializeField, Range(1f, 1.6f)]
    private float starPopScale = 1.22f;


    [Header("Gameplay References")]
    [SerializeField] private LevelRuntimeController levelRuntimeController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private StarRatingManager starRatingManager;

    [SerializeField]
    private PopupGameplayVisibilityController popupGameplayVisibility;


    [Header("Behaviour")]
    [SerializeField] private bool pauseGameWhenOpened = true;


    private bool isOpen;
    private bool isContinuing;

    private Coroutine starRevealRoutine;
    private Vector3 star1BaseScale;
    private Vector3 star2BaseScale;
    private Vector3 star3BaseScale;
    private bool starBaseScalesCaptured;
    private int starRevealTarget = -1;


    private void Awake()
    {
        CaptureStarBaseScales();

        if (levelCompletePanel != null)
        {
            UITransition.HideImmediate(levelCompletePanel);
        }
    }


    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                ContinueToNextLevel
            );

            continueButton.onClick.AddListener(
                ContinueToNextLevel
            );
        }
    }


    private void OnDisable()
    {
        Unsubscribe();
        StopStarRevealAnimation(true);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                ContinueToNextLevel
            );
        }
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

        if (scoreManager == null)
        {
            if (ScoreManager.Instance != null)
            {
                scoreManager =
                    ScoreManager.Instance;
            }
            else
            {
                scoreManager =
                    FindFirstObjectByType<ScoreManager>(
                        FindObjectsInactive.Include
                    );
            }
        }

        if (starRatingManager == null)
        {
            if (StarRatingManager.Instance != null)
            {
                starRatingManager =
                    StarRatingManager.Instance;
            }
            else
            {
                starRatingManager =
                    FindFirstObjectByType<StarRatingManager>(
                        FindObjectsInactive.Include
                    );
            }
        }

        if (popupGameplayVisibility == null)
        {
            popupGameplayVisibility =
                FindFirstObjectByType<PopupGameplayVisibilityController>(
                    FindObjectsInactive.Include
                );
        }
    }


    private void Subscribe()
    {
        if (levelRuntimeController != null)
        {
            levelRuntimeController.LevelCompleted -=
                HandleLevelCompleted;

            levelRuntimeController.LevelCompleted +=
                HandleLevelCompleted;

            levelRuntimeController.LevelGenerated -=
                HandleLevelGenerated;

            levelRuntimeController.LevelGenerated +=
                HandleLevelGenerated;
        }

        if (scoreManager != null)
        {
            scoreManager.ScoreAdded -=
                HandleScoreAdded;

            scoreManager.ScoreAdded +=
                HandleScoreAdded;
        }

        if (starRatingManager != null)
        {
            starRatingManager.StarsAwarded -=
                HandleStarsAwarded;

            starRatingManager.StarsAwarded +=
                HandleStarsAwarded;
        }
    }


    private void Unsubscribe()
    {
        if (levelRuntimeController != null)
        {
            levelRuntimeController.LevelCompleted -=
                HandleLevelCompleted;

            levelRuntimeController.LevelGenerated -=
                HandleLevelGenerated;
        }

        if (scoreManager != null)
        {
            scoreManager.ScoreAdded -=
                HandleScoreAdded;
        }

        if (starRatingManager != null)
        {
            starRatingManager.StarsAwarded -=
                HandleStarsAwarded;
        }
    }


    private void HandleLevelCompleted(
        GridLevelData completedLevel)
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        isContinuing = false;

        bool hasNextLevel =
            levelRuntimeController != null &&
            levelRuntimeController.HasNextLevel;

        if (levelCompleteText != null)
        {
            if (completedLevel != null)
            {
                levelCompleteText.text =
                    hasNextLevel
                        ? $"LEVEL {completedLevel.LevelNumber} COMPLETE"
                        : "ALL LEVELS COMPLETE";
            }
            else
            {
                levelCompleteText.text =
                    hasNextLevel
                        ? "LEVEL COMPLETE"
                        : "ALL LEVELS COMPLETE";
            }
        }

        /*
         * ScoreManager isi LevelCompleted event par score add karta hai.
         * ScoreAdded event thori der mein exact earned amount bhej dega.
         *
         * Fallback ke liye current PointsPerLevel pehle hi show kar dete hain.
         */
        if (earnedCoinsText != null)
        {
            int fallbackEarned =
                scoreManager != null
                    ? scoreManager.PointsPerLevel
                    : 0;

            SetEarnedCoinsText(
                fallbackEarned
            );
        }

        /*
         * Event subscription order ki wajah se StarRatingManager ne same
         * LevelCompleted event par rating pehle ya baad mein calculate ki ho
         * sakti hai. Current value show karte hain; StarsAwarded event exact
         * result aate hi visuals refresh kar dega.
         */
        int currentStars =
            starRatingManager != null
                ? starRatingManager.CalculateStars()
                : 0;

        SetStarVisuals(
            0
        );

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(
                hasNextLevel
            );

            continueButton.interactable =
                hasNextLevel;
        }

        popupGameplayVisibility?.HideGameplay();

        if (levelCompletePanel != null)
        {
            UITransition.Show(levelCompletePanel);
        }

        if (pauseGameWhenOpened)
        {
            Time.timeScale = 0f;
        }

        PlayStarRevealAnimation(currentStars);
    }


    private void HandleScoreAdded(
        int amountAdded,
        int newTotalScore)
    {
        if (!isOpen)
        {
            return;
        }

        SetEarnedCoinsText(
            amountAdded
        );
    }


    private void SetEarnedCoinsText(
        int amount)
    {
        if (earnedCoinsText == null)
        {
            return;
        }

        earnedCoinsText.text =
            $"{earnedCoinsPrefix}{amount}";
    }


    private void HandleStarsAwarded(
        int stars)
    {
        if (!isOpen)
        {
            return;
        }

        PlayStarRevealAnimation(stars);
    }


    private void CaptureStarBaseScales()
    {
        if (starBaseScalesCaptured)
        {
            return;
        }

        star1BaseScale =
            star1 != null
                ? star1.transform.localScale
                : Vector3.one;

        star2BaseScale =
            star2 != null
                ? star2.transform.localScale
                : Vector3.one;

        star3BaseScale =
            star3 != null
                ? star3.transform.localScale
                : Vector3.one;

        starBaseScalesCaptured = true;
    }


    private void PlayStarRevealAnimation(int stars)
    {
        int safeStars = Mathf.Clamp(stars, 0, 3);

        if (starRevealRoutine != null &&
            starRevealTarget == safeStars)
        {
            return;
        }

        StopStarRevealAnimation(true);
        SetStarVisuals(0);
        starRevealTarget = safeStars;

        if (!animateStarReveal ||
            !Application.isPlaying ||
            safeStars <= 0)
        {
            SetStarVisuals(safeStars);
            return;
        }

        starRevealRoutine = StartCoroutine(
            AnimateStarsRoutine(safeStars)
        );
    }


    private IEnumerator AnimateStarsRoutine(int earnedStars)
    {
        if (starRevealInitialDelay > 0f)
        {
            yield return WaitForUnscaledSeconds(
                starRevealInitialDelay
            );
        }

        Image[] stars =
            { star1, star2, star3 };

        Vector3[] baseScales =
            { star1BaseScale, star2BaseScale, star3BaseScale };

        for (int i = 0; i < earnedStars; i++)
        {
            Image starImage = stars[i];

            if (starImage != null)
            {
                starImage.color = earnedStarColor;

                yield return AnimateSingleStarRoutine(
                    starImage.transform,
                    baseScales[i]
                );
            }

            if (i < earnedStars - 1 &&
                starRevealStagger > 0f)
            {
                yield return WaitForUnscaledSeconds(
                    starRevealStagger
                );
            }
        }

        starRevealRoutine = null;
    }


    private IEnumerator AnimateSingleStarRoutine(
        Transform starTransform,
        Vector3 baseScale)
    {
        float duration = Mathf.Max(0.01f, starPopDuration);
        Vector3 smallScale = baseScale * starStartScale;
        Vector3 largeScale = baseScale * starPopScale;
        float elapsed = 0f;

        starTransform.localScale = smallScale;

        while (elapsed < duration)
        {
            float normalizedTime =
                Mathf.Clamp01(elapsed / duration);

            if (normalizedTime < 0.65f)
            {
                float growProgress =
                    SmoothStep(normalizedTime / 0.65f);

                starTransform.localScale =
                    Vector3.LerpUnclamped(
                        smallScale,
                        largeScale,
                        growProgress
                    );
            }
            else
            {
                float settleProgress =
                    SmoothStep(
                        (normalizedTime - 0.65f) / 0.35f
                    );

                starTransform.localScale =
                    Vector3.LerpUnclamped(
                        largeScale,
                        baseScale,
                        settleProgress
                    );
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        starTransform.localScale = baseScale;
    }


    private static IEnumerator WaitForUnscaledSeconds(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }


    private static float SmoothStep(float value)
    {
        float clampedValue = Mathf.Clamp01(value);

        return
            clampedValue *
            clampedValue *
            (3f - 2f * clampedValue);
    }


    private void StopStarRevealAnimation(bool restoreBaseScales)
    {
        if (starRevealRoutine != null)
        {
            StopCoroutine(starRevealRoutine);
            starRevealRoutine = null;
        }

        if (restoreBaseScales)
        {
            CaptureStarBaseScales();

            if (star1 != null)
            {
                star1.transform.localScale = star1BaseScale;
            }

            if (star2 != null)
            {
                star2.transform.localScale = star2BaseScale;
            }

            if (star3 != null)
            {
                star3.transform.localScale = star3BaseScale;
            }
        }

        starRevealTarget = -1;
    }


    private void SetStarVisuals(
        int stars)
    {
        int safeStars =
            Mathf.Clamp(
                stars,
                0,
                3
            );

        SetSingleStarVisual(
            star1,
            safeStars >= 1
        );

        SetSingleStarVisual(
            star2,
            safeStars >= 2
        );

        SetSingleStarVisual(
            star3,
            safeStars >= 3
        );
    }


    private void SetSingleStarVisual(
        Image starImage,
        bool earned)
    {
        if (starImage == null)
        {
            return;
        }

        starImage.color =
            earned
                ? earnedStarColor
                : unearnedStarColor;
    }


    private void HandleLevelGenerated(
        GridLevelData generatedLevel)
    {
        isOpen = false;
        isContinuing = false;

        StopStarRevealAnimation(true);

        SetStarVisuals(0);

        if (levelCompletePanel != null)
        {
            UITransition.Hide(levelCompletePanel);
        }

        popupGameplayVisibility?.ShowGameplay();
    }


    public void ContinueToNextLevel()
    {
        if (isContinuing ||
            levelRuntimeController == null ||
            !levelRuntimeController.HasNextLevel)
        {
            return;
        }

        isContinuing = true;

        StopStarRevealAnimation(true);

        if (continueButton != null)
        {
            continueButton.interactable = false;
        }

        if (levelCompletePanel != null)
        {
            UITransition.Hide(levelCompletePanel);
        }

        isOpen = false;

        popupGameplayVisibility?.ShowGameplay();

        Time.timeScale = 1f;

        levelRuntimeController.LoadNextLevel();
    }


    public void Hide()
    {
        isOpen = false;
        isContinuing = false;

        StopStarRevealAnimation(true);

        if (levelCompletePanel != null)
        {
            UITransition.Hide(levelCompletePanel);
        }

        popupGameplayVisibility?.ShowGameplay();
    }
}











