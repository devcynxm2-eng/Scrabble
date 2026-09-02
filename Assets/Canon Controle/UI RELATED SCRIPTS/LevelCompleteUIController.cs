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





// using System.Collections;
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
//         new Color(0.32f, 0.32f, 0.32f, 0.7f);

//     [Header("Star Reveal Animation")]

//     [SerializeField]
//     private bool animateStarReveal = true;

//     [SerializeField, Min(0f)]
//     private float starRevealInitialDelay = 0.12f;

//     [SerializeField, Min(0.01f)]
//     private float starPopDuration = 0.24f;

//     [SerializeField, Min(0f)]
//     private float starRevealStagger = 0.08f;

//     [SerializeField, Range(0.05f, 1f)]
//     private float starStartScale = 0.2f;

//     [SerializeField, Range(1f, 1.6f)]
//     private float starPopScale = 1.22f;


//     [Header("Gameplay References")]
//     [SerializeField] private LevelRuntimeController levelRuntimeController;
//     [SerializeField] private ScoreManager scoreManager;
//     [SerializeField] private StarRatingManager starRatingManager;

//     [SerializeField]
//     private PopupGameplayVisibilityController popupGameplayVisibility;


//     [Header("Behaviour")]
//     [SerializeField] private bool pauseGameWhenOpened = true;


//     private bool isOpen;
//     private bool isContinuing;
//     private bool returnToMainMenuAfterContinue;

//     private Coroutine starRevealRoutine;
//     private Vector3 star1BaseScale;
//     private Vector3 star2BaseScale;
//     private Vector3 star3BaseScale;
//     private bool starBaseScalesCaptured;
//     private int starRevealTarget = -1;


//     private void Awake()
//     {
//         CaptureStarBaseScales();

//         if (levelCompletePanel != null)
//         {
//             UITransition.HideImmediate(levelCompletePanel);
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
//         StopStarRevealAnimation(true);

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

//         if (popupGameplayVisibility == null)
//         {
//             popupGameplayVisibility =
//                 FindFirstObjectByType<PopupGameplayVisibilityController>(
//                     FindObjectsInactive.Include
//                 );
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

//         int completedLevelNumber =
//             completedLevel != null
//                 ? completedLevel.LevelNumber
//                 : 0;

//         returnToMainMenuAfterContinue =
//             scoreManager != null
//                 ? scoreManager.IsMilestoneLevel(
//                     completedLevelNumber
//                 )
//                 : completedLevelNumber > 0 &&
//                   completedLevelNumber % 10 == 0;

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
//          * Normal level par ScoreManager isi event par score add karta hai.
//          * Har 10th level ka score main-menu coin animation tak pending rehta hai.
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
//                 ? starRatingManager.CalculateStars()
//                 : 0;

//         SetStarVisuals(
//             0
//         );

//         if (continueButton != null)
//         {
//             continueButton.gameObject.SetActive(
//                 hasNextLevel ||
//                 returnToMainMenuAfterContinue
//             );

//             continueButton.interactable =
//                 hasNextLevel ||
//                 returnToMainMenuAfterContinue;
//         }

//         popupGameplayVisibility?.HideGameplay();

//         if (levelCompletePanel != null)
//         {
//             UITransition.Show(levelCompletePanel);
//         }

//         if (pauseGameWhenOpened)
//         {
//             Time.timeScale = 0f;
//         }

//         PlayStarRevealAnimation(currentStars);
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

//         PlayStarRevealAnimation(stars);
//     }


//     private void CaptureStarBaseScales()
//     {
//         if (starBaseScalesCaptured)
//         {
//             return;
//         }

//         star1BaseScale =
//             star1 != null
//                 ? star1.transform.localScale
//                 : Vector3.one;

//         star2BaseScale =
//             star2 != null
//                 ? star2.transform.localScale
//                 : Vector3.one;

//         star3BaseScale =
//             star3 != null
//                 ? star3.transform.localScale
//                 : Vector3.one;

//         starBaseScalesCaptured = true;
//     }


//     private void PlayStarRevealAnimation(int stars)
//     {
//         int safeStars = Mathf.Clamp(stars, 0, 3);

//         if (starRevealRoutine != null &&
//             starRevealTarget == safeStars)
//         {
//             return;
//         }

//         StopStarRevealAnimation(true);
//         SetStarVisuals(0);
//         starRevealTarget = safeStars;

//         if (!animateStarReveal ||
//             !Application.isPlaying ||
//             safeStars <= 0)
//         {
//             SetStarVisuals(safeStars);
//             return;
//         }

//         starRevealRoutine = StartCoroutine(
//             AnimateStarsRoutine(safeStars)
//         );
//     }


//     private IEnumerator AnimateStarsRoutine(int earnedStars)
//     {
//         if (starRevealInitialDelay > 0f)
//         {
//             yield return WaitForUnscaledSeconds(
//                 starRevealInitialDelay
//             );
//         }

//         Image[] stars =
//             { star1, star2, star3 };

//         Vector3[] baseScales =
//             { star1BaseScale, star2BaseScale, star3BaseScale };

//         for (int i = 0; i < earnedStars; i++)
//         {
//             Image starImage = stars[i];

//             if (starImage != null)
//             {
//                 starImage.color = earnedStarColor;

//                 yield return AnimateSingleStarRoutine(
//                     starImage.transform,
//                     baseScales[i]
//                 );
//             }

//             if (i < earnedStars - 1 &&
//                 starRevealStagger > 0f)
//             {
//                 yield return WaitForUnscaledSeconds(
//                     starRevealStagger
//                 );
//             }
//         }

//         starRevealRoutine = null;
//     }


//     private IEnumerator AnimateSingleStarRoutine(
//         Transform starTransform,
//         Vector3 baseScale)
//     {
//         float duration = Mathf.Max(0.01f, starPopDuration);
//         Vector3 smallScale = baseScale * starStartScale;
//         Vector3 largeScale = baseScale * starPopScale;
//         float elapsed = 0f;

//         starTransform.localScale = smallScale;

//         while (elapsed < duration)
//         {
//             float normalizedTime =
//                 Mathf.Clamp01(elapsed / duration);

//             if (normalizedTime < 0.65f)
//             {
//                 float growProgress =
//                     SmoothStep(normalizedTime / 0.65f);

//                 starTransform.localScale =
//                     Vector3.LerpUnclamped(
//                         smallScale,
//                         largeScale,
//                         growProgress
//                     );
//             }
//             else
//             {
//                 float settleProgress =
//                     SmoothStep(
//                         (normalizedTime - 0.65f) / 0.35f
//                     );

//                 starTransform.localScale =
//                     Vector3.LerpUnclamped(
//                         largeScale,
//                         baseScale,
//                         settleProgress
//                     );
//             }

//             elapsed += Time.unscaledDeltaTime;
//             yield return null;
//         }

//         starTransform.localScale = baseScale;
//     }


//     private static IEnumerator WaitForUnscaledSeconds(float duration)
//     {
//         float elapsed = 0f;

//         while (elapsed < duration)
//         {
//             elapsed += Time.unscaledDeltaTime;
//             yield return null;
//         }
//     }


//     private static float SmoothStep(float value)
//     {
//         float clampedValue = Mathf.Clamp01(value);

//         return
//             clampedValue *
//             clampedValue *
//             (3f - 2f * clampedValue);
//     }


//     private void StopStarRevealAnimation(bool restoreBaseScales)
//     {
//         if (starRevealRoutine != null)
//         {
//             StopCoroutine(starRevealRoutine);
//             starRevealRoutine = null;
//         }

//         if (restoreBaseScales)
//         {
//             CaptureStarBaseScales();

//             if (star1 != null)
//             {
//                 star1.transform.localScale = star1BaseScale;
//             }

//             if (star2 != null)
//             {
//                 star2.transform.localScale = star2BaseScale;
//             }

//             if (star3 != null)
//             {
//                 star3.transform.localScale = star3BaseScale;
//             }
//         }

//         starRevealTarget = -1;
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
//         returnToMainMenuAfterContinue = false;

//         StopStarRevealAnimation(true);

//         SetStarVisuals(0);

//         if (levelCompletePanel != null)
//         {
//             UITransition.Hide(levelCompletePanel);
//         }

//         popupGameplayVisibility?.ShowGameplay();
//     }


//     public void ContinueToNextLevel()
//     {
//         if (isContinuing ||
//             levelRuntimeController == null ||
//             (!returnToMainMenuAfterContinue &&
//              !levelRuntimeController.HasNextLevel))
//         {
//             return;
//         }

//         isContinuing = true;

//         StopStarRevealAnimation(true);

//         if (continueButton != null)
//         {
//             continueButton.interactable = false;
//         }

//         if (levelCompletePanel != null)
//         {
//             UITransition.Hide(levelCompletePanel);
//         }

//         isOpen = false;

//         Time.timeScale = 1f;

//         if (returnToMainMenuAfterContinue)
//         {
//             popupGameplayVisibility?.HideGameplay();

//             bool menuShown =
//                 levelRuntimeController
//                     .AdvanceToNextLevelAndShowMainMenu();

//             if (!menuShown)
//             {
//                 isOpen = true;
//                 isContinuing = false;

//                 if (continueButton != null)
//                 {
//                     continueButton.interactable = true;
//                 }
//             }

//             return;
//         }

//         popupGameplayVisibility?.ShowGameplay();

//         levelRuntimeController.LoadNextLevel();
//     }


//     public void Hide()
//     {
//         isOpen = false;
//         isContinuing = false;
//         returnToMainMenuAfterContinue = false;

//         StopStarRevealAnimation(true);

//         if (levelCompletePanel != null)
//         {
//             UITransition.Hide(levelCompletePanel);
//         }

//         popupGameplayVisibility?.ShowGameplay();
//     }
// }





using System.Collections;
using System.Collections.Generic;
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


    [Header("Score And Life Shot Impact")]
    [Tooltip("Level Complete screen ka life count text.")]
    [SerializeField] private TMP_Text lifeText;

    [SerializeField, Min(1f)]
    private float statsImpactStartScale = 2.3f;

    [SerializeField, Min(0f)]
    private float statsImpactStartYOffset = 90f;

    [SerializeField, Min(0.01f)]
    private float statsImpactHitDuration = 0.2f;

    [SerializeField, Min(0.01f)]
    private float statsImpactBounceDuration = 0.16f;


    [Header("Stars")]
    [SerializeField] private Image star1;
    [SerializeField] private Image star2;
    [SerializeField] private Image star3;

    [Tooltip("Second star ke peechay wali gold ray image (start 2 back Image).")]
    [SerializeField] private Image star2BackImage;

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

    [Tooltip("Star reveal start mein apni jagah se kitna upar float kare.")]
    [SerializeField, Min(0f)]
    private float starFloatHeight = 110f;

    [Tooltip("Second star reveal par gold ray ka starting rotation.")]
    [SerializeField]
    private float star2BackStartRotation = -28f;

    [Header("Small Star Burst")]

    [Tooltip(
        "ON: har earned star ke PEECHAY se bohat saare chhote stars " +
        "nikal kar chaaron taraf phailenge, khud bhi spin aur scale " +
        "karte hue."
    )]
    [SerializeField]
    private bool playSmallStarBurst = true;

    [Tooltip(
        "Chhote star ka sprite. Khali chhod dein to main star ka apna " +
        "sprite use hoga."
    )]
    [SerializeField]
    private Sprite smallStarSprite;

    [SerializeField, Range(1, 40)]
    private int smallStarCount = 14;

    [Tooltip(
        "Star ke center se kitni door tak phailenge (min, max)."
    )]
    [SerializeField]
    private Vector2 smallStarDistanceRange =
        new Vector2(70f, 165f);

    [Tooltip(
        "Main star ke size ke muqable chhote star ka scale (min, max)."
    )]
    [SerializeField]
    private Vector2 smallStarScaleRange =
        new Vector2(0.14f, 0.30f);

    [Tooltip(
        "Ek chhote star ke poore burst ka time (min, max) seconds."
    )]
    [SerializeField]
    private Vector2 smallStarDurationRange =
        new Vector2(0.45f, 0.75f);

    [Tooltip(
        "Spin speed degrees per second (min, max). Direction random hoti hai."
    )]
    [SerializeField]
    private Vector2 smallStarSpinSpeedRange =
        new Vector2(90f, 320f);

    [SerializeField]
    private Color smallStarColor =
        new Color(1f, 0.86f, 0.25f, 1f);


    [Header("Gold Ray Continuous Animation")]
    [SerializeField, Min(0f)]
    private float star2BackRotationSpeed = 24f;

    [SerializeField, Range(0f, 0.2f)]
    private float star2BackPulseAmount = 0.04f;

    [SerializeField, Min(0.01f)]
    private float star2BackPulseSpeed = 2f;


    [Header("Gameplay References")]
    [SerializeField] private LevelRuntimeController levelRuntimeController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private StarRatingManager starRatingManager;

    [SerializeField]
    private PopupGameplayVisibilityController popupGameplayVisibility;


    [Header("Level Complete Sequence")]
    [SerializeField]
    private LevelCompleteSequenceController levelCompleteSequenceController;


    [Header("Behaviour")]
    [SerializeField] private bool pauseGameWhenOpened = true;


    private bool isOpen;
    private bool isContinuing;
    private bool returnToMainMenuAfterContinue;

    /// <summary>
    /// Runtime mein banaye gaye chhote star objects. Reveal dobara start
    /// ya popup band hone par inhein saaf karna zaroori hai.
    /// </summary>
    private readonly List<GameObject> smallStarBurstObjects =
        new List<GameObject>();

    private Coroutine starRevealRoutine;
    private Vector3 star1BaseScale;
    private Vector3 star2BaseScale;
    private Vector3 star3BaseScale;
    private Vector2 star1BasePosition;
    private Vector2 star2BasePosition;
    private Vector2 star3BasePosition;
    private Vector3 star2BackBaseScale;
    private Quaternion star2BackBaseRotation;
    private bool starBaseScalesCaptured;
    private int starRevealTarget = -1;
    private int pendingStars;
    private Coroutine star2BackLoopRoutine;

    private Coroutine statsImpactRoutine;
    private Vector2 scoreTextBasePosition;
    private Vector2 lifeTextBasePosition;
    private Vector3 scoreTextBaseScale;
    private Vector3 lifeTextBaseScale;
    private Quaternion scoreTextBaseRotation;
    private Quaternion lifeTextBaseRotation;
    private bool statsImpactBasePoseCaptured;


    private void Awake()
    {
        CaptureStarBaseScales();
        CaptureStatsImpactBasePose();

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

        if (levelCompleteSequenceController != null)
        {
            levelCompleteSequenceController.StopSequence();
        }

        StopStatsImpactAnimation(true);
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

        if (levelCompleteSequenceController == null)
        {
            levelCompleteSequenceController =
                FindFirstObjectByType<LevelCompleteSequenceController>(
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

        int completedLevelNumber =
            completedLevel != null
                ? completedLevel.LevelNumber
                : 0;

        returnToMainMenuAfterContinue =
            scoreManager != null
                ? scoreManager.IsMilestoneLevel(
                    completedLevelNumber
                )
                : completedLevelNumber > 0 &&
                  completedLevelNumber % 10 == 0;

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
         * Normal level par ScoreManager isi event par score add karta hai.
         * Har 10th level ka score main-menu coin animation tak pending rehta hai.
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

        pendingStars = Mathf.Clamp(
            currentStars,
            0,
            3
        );

        SetStarVisuals(
            0
        );

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(
                hasNextLevel ||
                returnToMainMenuAfterContinue
            );

            continueButton.interactable =
                hasNextLevel ||
                returnToMainMenuAfterContinue;
        }

        if (levelCompleteSequenceController != null)
        {
            levelCompleteSequenceController
                .PlayLevelCompleteSequence(
                    ShowLevelCompletePopup
                );
        }
        else
        {
            ShowLevelCompletePopup();
        }
    }


    private void ShowLevelCompletePopup()
    {
        if (!isOpen)
        {
            return;
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

        PlayStarRevealAnimation(
            pendingStars
        );

        PlayStatsImpactAnimation();
    }


    private void CaptureStatsImpactBasePose()
    {
        if (statsImpactBasePoseCaptured)
        {
            return;
        }

        RectTransform scoreRect =
            earnedCoinsText != null
                ? earnedCoinsText.rectTransform
                : null;

        RectTransform lifeRect =
            lifeText != null
                ? lifeText.rectTransform
                : null;

        if (scoreRect == null && lifeRect == null)
        {
            return;
        }

        if (scoreRect != null)
        {
            scoreTextBasePosition =
                scoreRect.anchoredPosition;
            scoreTextBaseScale =
                scoreRect.localScale;
            scoreTextBaseRotation =
                scoreRect.localRotation;
        }

        if (lifeRect != null)
        {
            lifeTextBasePosition =
                lifeRect.anchoredPosition;
            lifeTextBaseScale =
                lifeRect.localScale;
            lifeTextBaseRotation =
                lifeRect.localRotation;
        }

        statsImpactBasePoseCaptured = true;
    }


    private void PlayStatsImpactAnimation()
    {
        CaptureStatsImpactBasePose();

        if (!statsImpactBasePoseCaptured)
        {
            return;
        }

        StopStatsImpactAnimation(true);

        statsImpactRoutine = StartCoroutine(
            StatsImpactRoutine()
        );
    }


    private IEnumerator StatsImpactRoutine()
    {
        SetStatsImpactPose(
            statsImpactStartScale,
            statsImpactStartYOffset,
            8f
        );

        float elapsed = 0f;

        while (elapsed < statsImpactHitDuration)
        {
            float normalized = Mathf.Clamp01(
                elapsed / statsImpactHitDuration
            );

            float hitProgress =
                normalized * normalized;

            SetStatsImpactPose(
                Mathf.Lerp(
                    statsImpactStartScale,
                    0.82f,
                    hitProgress
                ),
                Mathf.Lerp(
                    statsImpactStartYOffset,
                    0f,
                    hitProgress
                ),
                Mathf.Lerp(
                    8f,
                    0f,
                    hitProgress
                )
            );

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        SetStatsImpactPose(0.82f, 0f, 0f);

        yield return AnimateStatsImpactScale(
            0.82f,
            1.14f,
            statsImpactBounceDuration * 0.45f
        );

        yield return AnimateStatsImpactScale(
            1.14f,
            1f,
            statsImpactBounceDuration * 0.55f
        );

        RestoreStatsImpactPose();
        statsImpactRoutine = null;
    }


    private IEnumerator AnimateStatsImpactScale(
        float fromScale,
        float toScale,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            SetStatsImpactPose(
                Mathf.Lerp(
                    fromScale,
                    toScale,
                    progress
                ),
                0f,
                0f
            );

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        SetStatsImpactPose(toScale, 0f, 0f);
    }


    private void SetStatsImpactPose(
        float scaleMultiplier,
        float yOffset,
        float rotationAmount)
    {
        RectTransform scoreRect =
            earnedCoinsText != null
                ? earnedCoinsText.rectTransform
                : null;

        RectTransform lifeRect =
            lifeText != null
                ? lifeText.rectTransform
                : null;

        if (scoreRect != null)
        {
            scoreRect.anchoredPosition =
                scoreTextBasePosition +
                Vector2.up * yOffset;
            scoreRect.localScale =
                scoreTextBaseScale * scaleMultiplier;
            scoreRect.localRotation =
                scoreTextBaseRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    -rotationAmount
                );
        }

        if (lifeRect != null)
        {
            lifeRect.anchoredPosition =
                lifeTextBasePosition +
                Vector2.up * yOffset;
            lifeRect.localScale =
                lifeTextBaseScale * scaleMultiplier;
            lifeRect.localRotation =
                lifeTextBaseRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    rotationAmount
                );
        }
    }


    private void StopStatsImpactAnimation(
        bool restorePose)
    {
        if (statsImpactRoutine != null)
        {
            StopCoroutine(statsImpactRoutine);
            statsImpactRoutine = null;
        }

        if (restorePose)
        {
            RestoreStatsImpactPose();
        }
    }


    private void RestoreStatsImpactPose()
    {
        if (!statsImpactBasePoseCaptured)
        {
            return;
        }

        RectTransform scoreRect =
            earnedCoinsText != null
                ? earnedCoinsText.rectTransform
                : null;

        RectTransform lifeRect =
            lifeText != null
                ? lifeText.rectTransform
                : null;

        if (scoreRect != null)
        {
            scoreRect.anchoredPosition =
                scoreTextBasePosition;
            scoreRect.localScale =
                scoreTextBaseScale;
            scoreRect.localRotation =
                scoreTextBaseRotation;
        }

        if (lifeRect != null)
        {
            lifeRect.anchoredPosition =
                lifeTextBasePosition;
            lifeRect.localScale =
                lifeTextBaseScale;
            lifeRect.localRotation =
                lifeTextBaseRotation;
        }
    }


    private void HandleLevelCompleted(
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

        pendingStars = Mathf.Clamp(
            stars,
            0,
            3
        );

        if (levelCompletePanel != null &&
            levelCompletePanel.activeSelf)
        {
            PlayStarRevealAnimation(
                pendingStars
            );
        }
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

        star1BasePosition = GetStarAnchoredPosition(star1);
        star2BasePosition = GetStarAnchoredPosition(star2);
        star3BasePosition = GetStarAnchoredPosition(star3);

        star2BackBaseScale =
            star2BackImage != null
                ? star2BackImage.transform.localScale
                : Vector3.one;

        star2BackBaseRotation =
            star2BackImage != null
                ? star2BackImage.transform.localRotation
                : Quaternion.identity;

        starBaseScalesCaptured = true;
    }


    private static Vector2 GetStarAnchoredPosition(Image starImage)
    {
        RectTransform rectTransform =
            starImage != null
                ? starImage.rectTransform
                : null;

        return
            rectTransform != null
                ? rectTransform.anchoredPosition
                : Vector2.zero;
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

        Vector2[] basePositions =
            { star1BasePosition, star2BasePosition, star3BasePosition };

        for (int i = 0; i < earnedStars; i++)
        {
            Image starImage = stars[i];

            if (starImage != null)
            {
                starImage.color = earnedStarColor;

                bool animateStar2Back =
                    i == 1 &&
                    star2BackImage != null;

                if (animateStar2Back)
                {
                    star2BackImage.color = earnedStarColor;
                }

                /*
                 * Chhote stars ka burst star ke pop ke sath hi shuru hota
                 * hai (blocking nahi hai), taake dono ek hi celebration
                 * lagen.
                 */
                PlaySmallStarBurst(
                    starImage,
                    basePositions[i]
                );

                yield return AnimateSingleStarRoutine(
                    starImage.rectTransform,
                    baseScales[i],
                    basePositions[i],
                    animateStar2Back
                );

                if (animateStar2Back)
                {
                    StartStar2BackLoop();
                }
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
        RectTransform starTransform,
        Vector3 baseScale,
        Vector2 basePosition,
        bool animateStar2Back)
    {
        float duration = Mathf.Max(0.01f, starPopDuration);
        Vector3 smallScale = baseScale * starStartScale;
        Vector3 largeScale = baseScale * starPopScale;
        Vector2 floatStartPosition =
            basePosition + Vector2.up * starFloatHeight;
        float elapsed = 0f;

        starTransform.localScale = smallScale;
        starTransform.anchoredPosition = floatStartPosition;

        if (animateStar2Back)
        {
            star2BackImage.transform.localScale =
                star2BackBaseScale * starStartScale;

            star2BackImage.transform.localRotation =
                star2BackBaseRotation *
                Quaternion.Euler(0f, 0f, star2BackStartRotation);
        }

        while (elapsed < duration)
        {
            float normalizedTime =
                Mathf.Clamp01(elapsed / duration);

            float floatProgress = SmoothStep(normalizedTime);
            starTransform.anchoredPosition =
                Vector2.LerpUnclamped(
                    floatStartPosition,
                    basePosition,
                    floatProgress
                );

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

                if (animateStar2Back)
                {
                    star2BackImage.transform.localScale =
                        Vector3.LerpUnclamped(
                            star2BackBaseScale * starStartScale,
                            star2BackBaseScale * 1.08f,
                            growProgress
                        );
                }
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

                if (animateStar2Back)
                {
                    star2BackImage.transform.localScale =
                        Vector3.LerpUnclamped(
                            star2BackBaseScale * 1.08f,
                            star2BackBaseScale,
                            settleProgress
                        );
                }
            }

            if (animateStar2Back)
            {
                star2BackImage.transform.localRotation =
                    Quaternion.SlerpUnclamped(
                        star2BackBaseRotation *
                            Quaternion.Euler(
                                0f,
                                0f,
                                star2BackStartRotation
                            ),
                        star2BackBaseRotation,
                        floatProgress
                    );
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        starTransform.localScale = baseScale;
        starTransform.anchoredPosition = basePosition;

        if (animateStar2Back)
        {
            star2BackImage.transform.localScale = star2BackBaseScale;
            star2BackImage.transform.localRotation = star2BackBaseRotation;
        }
    }


    /// <summary>
    /// Star ke peechay se chhote stars ka burst. Har chhota star apni
    /// random direction, distance, size, spin aur duration leta hai,
    /// taake burst mechanical na lage.
    ///
    /// Ye blocking nahi hai — main star ka pop isi dauran chalta rehta hai.
    /// </summary>
    private void PlaySmallStarBurst(
        Image starImage,
        Vector2 starBasePosition)
    {
        if (!playSmallStarBurst ||
            starImage == null ||
            !Application.isPlaying ||
            !isActiveAndEnabled)
        {
            return;
        }

        RectTransform parentRect =
            starImage.rectTransform.parent as RectTransform;

        if (parentRect == null)
        {
            return;
        }

        Sprite sprite =
            smallStarSprite != null
                ? smallStarSprite
                : starImage.sprite;

        if (sprite == null)
        {
            return;
        }

        int count =
            Mathf.Max(1, smallStarCount);

        Vector2 starSize =
            starImage.rectTransform.rect.size;

        if (starSize.x <= 1f ||
            starSize.y <= 1f)
        {
            starSize = new Vector2(100f, 100f);
        }

        /*
         * Angles ko equally divide kar ke har slice mein thoda random
         * offset dete hain — full circle cover hota hai lekin spacing
         * natural rehti hai.
         */
        float angleStep = 360f / count;
        float angleOffset = Random.Range(0f, angleStep);

        for (int i = 0; i < count; i++)
        {
            GameObject smallStar =
                new GameObject(
                    "SmallStarBurst",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

            RectTransform smallRect =
                smallStar.GetComponent<RectTransform>();

            smallRect.SetParent(parentRect, false);

            /*
             * Main star ke theek peechay insert karte hain, taake chhote
             * stars uske "back" se nikalte hue nazar aayen.
             */
            smallRect.SetSiblingIndex(
                starImage.rectTransform.GetSiblingIndex()
            );

            smallRect.anchorMin =
                starImage.rectTransform.anchorMin;

            smallRect.anchorMax =
                starImage.rectTransform.anchorMax;

            smallRect.pivot =
                starImage.rectTransform.pivot;

            smallRect.sizeDelta = starSize;
            smallRect.anchoredPosition = starBasePosition;
            smallRect.localScale = Vector3.zero;

            Image smallImage =
                smallStar.GetComponent<Image>();

            smallImage.sprite = sprite;
            smallImage.color = smallStarColor;
            smallImage.raycastTarget = false;
            smallImage.preserveAspect = true;

            smallStarBurstObjects.Add(smallStar);

            float angle =
                angleOffset +
                angleStep * i +
                Random.Range(-angleStep * 0.3f, angleStep * 0.3f);

            float radians = angle * Mathf.Deg2Rad;

            Vector2 direction =
                new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                );

            Vector2 targetPosition =
                starBasePosition +
                direction *
                Random.Range(
                    Mathf.Min(
                        smallStarDistanceRange.x,
                        smallStarDistanceRange.y
                    ),
                    Mathf.Max(
                        smallStarDistanceRange.x,
                        smallStarDistanceRange.y
                    )
                );

            float peakScale =
                Random.Range(
                    Mathf.Min(
                        smallStarScaleRange.x,
                        smallStarScaleRange.y
                    ),
                    Mathf.Max(
                        smallStarScaleRange.x,
                        smallStarScaleRange.y
                    )
                );

            float duration =
                Random.Range(
                    Mathf.Min(
                        smallStarDurationRange.x,
                        smallStarDurationRange.y
                    ),
                    Mathf.Max(
                        smallStarDurationRange.x,
                        smallStarDurationRange.y
                    )
                );

            float spinSpeed =
                Random.Range(
                    Mathf.Min(
                        smallStarSpinSpeedRange.x,
                        smallStarSpinSpeedRange.y
                    ),
                    Mathf.Max(
                        smallStarSpinSpeedRange.x,
                        smallStarSpinSpeedRange.y
                    )
                ) *
                (Random.value < 0.5f ? -1f : 1f);

            StartCoroutine(
                AnimateSmallStarRoutine(
                    smallStar,
                    smallRect,
                    smallImage,
                    starBasePosition,
                    targetPosition,
                    peakScale,
                    spinSpeed,
                    duration,
                    Random.Range(0f, 0.08f)
                )
            );
        }
    }


    private IEnumerator AnimateSmallStarRoutine(
        GameObject smallStar,
        RectTransform smallRect,
        Image smallImage,
        Vector2 startPosition,
        Vector2 targetPosition,
        float peakScale,
        float spinSpeed,
        float duration,
        float startDelay)
    {
        if (startDelay > 0f)
        {
            yield return WaitForUnscaledSeconds(startDelay);
        }

        if (smallStar == null)
        {
            yield break;
        }

        float safeDuration = Mathf.Max(0.05f, duration);
        float elapsed = 0f;
        float rotation = 0f;

        Color baseColor = smallImage.color;

        while (elapsed < safeDuration &&
               smallStar != null)
        {
            float normalizedTime =
                Mathf.Clamp01(elapsed / safeDuration);

            // Bahar ki taraf tez nikal kar dheere dheere ruk jata hai.
            float outwardProgress =
                1f - Mathf.Pow(1f - normalizedTime, 3f);

            smallRect.anchoredPosition =
                Vector2.LerpUnclamped(
                    startPosition,
                    targetPosition,
                    outwardProgress
                );

            /*
             * Pehle 30% mein pop hota hai, phir aakhir tak simat kar
             * gayab ho jata hai.
             */
            float scale =
                normalizedTime < 0.3f
                    ? Mathf.Lerp(
                        0f,
                        peakScale,
                        SmoothStep(normalizedTime / 0.3f)
                    )
                    : Mathf.Lerp(
                        peakScale,
                        peakScale * 0.25f,
                        SmoothStep(
                            (normalizedTime - 0.3f) / 0.7f
                        )
                    );

            smallRect.localScale = Vector3.one * scale;

            rotation += spinSpeed * Time.unscaledDeltaTime;

            smallRect.localRotation =
                Quaternion.Euler(0f, 0f, rotation);

            // Aakhri 45% mein fade out.
            float alpha =
                normalizedTime < 0.55f
                    ? 1f
                    : 1f - SmoothStep(
                        (normalizedTime - 0.55f) / 0.45f
                    );

            smallImage.color =
                new Color(
                    baseColor.r,
                    baseColor.g,
                    baseColor.b,
                    baseColor.a * alpha
                );

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        smallStarBurstObjects.Remove(smallStar);

        if (smallStar != null)
        {
            Destroy(smallStar);
        }
    }


    private void ClearSmallStarBursts()
    {
        for (int i = smallStarBurstObjects.Count - 1;
             i >= 0;
             i--)
        {
            GameObject smallStar =
                smallStarBurstObjects[i];

            if (smallStar != null)
            {
                Destroy(smallStar);
            }
        }

        smallStarBurstObjects.Clear();
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


    private void StartStar2BackLoop()
    {
        if (star2BackImage == null ||
            !Application.isPlaying ||
            !isActiveAndEnabled ||
            star2BackLoopRoutine != null)
        {
            return;
        }

        star2BackLoopRoutine = StartCoroutine(
            AnimateStar2BackLoopRoutine()
        );
    }


    private IEnumerator AnimateStar2BackLoopRoutine()
    {
        float rotation = 0f;
        float pulseTime = 0f;

        while (star2BackImage != null)
        {
            float deltaTime = Time.unscaledDeltaTime;
            rotation =
                Mathf.Repeat(
                    rotation + star2BackRotationSpeed * deltaTime,
                    360f
                );

            pulseTime += deltaTime * star2BackPulseSpeed;

            float pulseScale =
                1f + Mathf.Sin(pulseTime * Mathf.PI * 2f) *
                star2BackPulseAmount;

            star2BackImage.transform.localRotation =
                star2BackBaseRotation *
                Quaternion.Euler(0f, 0f, rotation);

            star2BackImage.transform.localScale =
                star2BackBaseScale * pulseScale;

            yield return null;
        }

        star2BackLoopRoutine = null;
    }


    private void StopStar2BackLoop(bool restoreBasePose)
    {
        if (star2BackLoopRoutine != null)
        {
            StopCoroutine(star2BackLoopRoutine);
            star2BackLoopRoutine = null;
        }

        if (restoreBasePose && star2BackImage != null)
        {
            star2BackImage.transform.localScale = star2BackBaseScale;
            star2BackImage.transform.localRotation =
                star2BackBaseRotation;
        }
    }


    private void StopStarRevealAnimation(bool restoreBaseScales)
    {
        StopStar2BackLoop(restoreBaseScales);

        ClearSmallStarBursts();

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
                star1.rectTransform.anchoredPosition = star1BasePosition;
            }

            if (star2 != null)
            {
                star2.transform.localScale = star2BaseScale;
                star2.rectTransform.anchoredPosition = star2BasePosition;
            }

            if (star3 != null)
            {
                star3.transform.localScale = star3BaseScale;
                star3.rectTransform.anchoredPosition = star3BasePosition;
            }

            if (star2BackImage != null)
            {
                star2BackImage.transform.localScale = star2BackBaseScale;
                star2BackImage.transform.localRotation =
                    star2BackBaseRotation;
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

        SetSingleStarVisual(
            star2BackImage,
            safeStars >= 2
        );

        if (safeStars >= 2)
        {
            StartStar2BackLoop();
        }
        else
        {
            StopStar2BackLoop(true);
        }
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
        returnToMainMenuAfterContinue = false;
        pendingStars = 0;

        if (levelCompleteSequenceController != null)
        {
            levelCompleteSequenceController.StopSequence();
        }

        StopStatsImpactAnimation(true);
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
            (!returnToMainMenuAfterContinue &&
             !levelRuntimeController.HasNextLevel))
        {
            return;
        }

        isContinuing = true;

        StopStatsImpactAnimation(true);
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

        Time.timeScale = 1f;

        if (returnToMainMenuAfterContinue)
        {
            popupGameplayVisibility?.HideGameplay();

            bool menuShown =
                levelRuntimeController
                    .AdvanceToNextLevelAndShowMainMenu();

            if (!menuShown)
            {
                isOpen = true;
                isContinuing = false;

                if (continueButton != null)
                {
                    continueButton.interactable = true;
                }
            }

            return;
        }

        popupGameplayVisibility?.ShowGameplay();

        levelRuntimeController.LoadNextLevel();
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

    public void Hide()
    {
        isOpen = false;
        isContinuing = false;
        returnToMainMenuAfterContinue = false;
        pendingStars = 0;

        if (levelCompleteSequenceController != null)
        {
            levelCompleteSequenceController.StopSequence();
        }

        StopStatsImpactAnimation(true);
        StopStarRevealAnimation(true);

        if (levelCompletePanel != null)
        {
            UITransition.Hide(levelCompletePanel);
        }

        popupGameplayVisibility?.ShowGameplay();
    }
}






