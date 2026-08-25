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
        new Color(1f, 1f, 1f, 0.25f);


    [Header("Gameplay References")]
    [SerializeField] private LevelRuntimeController levelRuntimeController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private StarRatingManager starRatingManager;


    [Header("Behaviour")]
    [SerializeField] private bool pauseGameWhenOpened = true;


    private bool isOpen;
    private bool isContinuing;


    private void Awake()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
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
                ? starRatingManager.LastAwardedStars
                : 0;

        SetStarVisuals(
            currentStars
        );

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(
                hasNextLevel
            );

            continueButton.interactable =
                hasNextLevel;
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        if (pauseGameWhenOpened)
        {
            Time.timeScale = 0f;
        }
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

        SetStarVisuals(
            stars
        );
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

        SetStarVisuals(0);

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
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

        if (continueButton != null)
        {
            continueButton.interactable = false;
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        isOpen = false;

        Time.timeScale = 1f;

        levelRuntimeController.LoadNextLevel();
    }


    public void Hide()
    {
        isOpen = false;
        isContinuing = false;

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }
}

















