// using UnityEngine;
// using UnityEngine.UI;

// public sealed class OutOfMovesUIController : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private GameObject outOfMovesPanel;
//     [SerializeField] private Button tryAgainButton;

//     [Header("Gameplay References")]
//     [SerializeField] private CannonController cannonController;
//     [SerializeField] private LevelRuntimeController levelRuntimeController;

//     [Header("Behaviour")]
//     [SerializeField] private bool pauseGameWhenOpened = true;


//     private bool isOpen;


//     private void Awake()
//     {
//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         Subscribe();

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.onClick.RemoveListener(TryAgain);
//             tryAgainButton.onClick.AddListener(TryAgain);
//         }
//     }


//     private void OnDisable()
//     {
//         Unsubscribe();

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.onClick.RemoveListener(TryAgain);
//         }
//     }


//     private void ResolveReferences()
//     {
//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

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
//         if (cannonController != null)
//         {
//             cannonController.OutOfMoves -= HandleOutOfMoves;
//             cannonController.OutOfMoves += HandleOutOfMoves;
//         }
//     }


//     private void Unsubscribe()
//     {
//         if (cannonController != null)
//         {
//             cannonController.OutOfMoves -= HandleOutOfMoves;
//         }
//     }


//     private void HandleOutOfMoves()
//     {
//         if (isOpen)
//         {
//             return;
//         }

//         // Agar level complete ho chuka hai
//         // to Out Of Moves popup nahi khulega.
//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         isOpen = true;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(true);
//         }

//         if (pauseGameWhenOpened)
//         {
//             Time.timeScale = 0f;
//         }
//     }


//     public void TryAgain()
//     {
//         Time.timeScale = 1f;

//         isOpen = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }

//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.RestartCurrentLevel();
//         }
//         else
//         {
//             Debug.LogWarning(
//                 "OutOfMovesUIController: LevelRuntimeController missing hai.",
//                 this
//             );
//         }
//     }


//     public void Hide()
//     {
//         isOpen = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }
//     }
// }







// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public sealed class OutOfMovesUIController : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private GameObject outOfMovesPanel;
//     [SerializeField] private Button tryAgainButton;

//     [Tooltip(
//         "200 coins spend karke extra shots lene wala button."
//     )]
//     [SerializeField] private Button useCoinsButton;

//     [Tooltip(
//         "Optional text. Example: 200"
//     )]
//     [SerializeField] private TMP_Text continueCostText;

//     [Tooltip(
//         "Optional text. Current available coins/score show karega."
//     )]
//     [SerializeField] private TMP_Text currentCoinsText;


//     [Header("Continue With Coins")]
//     [SerializeField, Min(0)]
//     private int continueCost = 200;

//     [SerializeField, Min(1)]
//     private int extraShots = 5;


//     [Header("Gameplay References")]
//     [SerializeField] private CannonController cannonController;
//     [SerializeField] private LevelRuntimeController levelRuntimeController;
//     [SerializeField] private ScoreManager scoreManager;


//     [Header("Behaviour")]
//     [SerializeField] private bool pauseGameWhenOpened = true;


//     private bool isOpen;


//     private void Awake()
//     {
//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         Subscribe();

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.onClick.RemoveListener(
//                 TryAgain
//             );

//             tryAgainButton.onClick.AddListener(
//                 TryAgain
//             );
//         }

//         if (useCoinsButton != null)
//         {
//             useCoinsButton.onClick.RemoveListener(
//                 UseCoinsForExtraShots
//             );

//             useCoinsButton.onClick.AddListener(
//                 UseCoinsForExtraShots
//             );
//         }

//         RefreshPurchaseUI();
//     }


//     private void OnDisable()
//     {
//         Unsubscribe();

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.onClick.RemoveListener(
//                 TryAgain
//             );
//         }

//         if (useCoinsButton != null)
//         {
//             useCoinsButton.onClick.RemoveListener(
//                 UseCoinsForExtraShots
//             );
//         }
//     }


//     private void ResolveReferences()
//     {
//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

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
//         if (cannonController != null)
//         {
//             cannonController.OutOfMoves -=
//                 HandleOutOfMoves;

//             cannonController.OutOfMoves +=
//                 HandleOutOfMoves;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreChanged -=
//                 HandleScoreChanged;

//             scoreManager.ScoreChanged +=
//                 HandleScoreChanged;
//         }
//     }


//     private void Unsubscribe()
//     {
//         if (cannonController != null)
//         {
//             cannonController.OutOfMoves -=
//                 HandleOutOfMoves;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreChanged -=
//                 HandleScoreChanged;
//         }
//     }


//     private void HandleOutOfMoves()
//     {
//         if (isOpen)
//         {
//             return;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         isOpen = true;

//         RefreshPurchaseUI();

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(true);
//         }

//         if (pauseGameWhenOpened)
//         {
//             Time.timeScale = 0f;
//         }
//     }


//     private void HandleScoreChanged(
//         int newScore)
//     {
//         RefreshPurchaseUI();
//     }


//     private void RefreshPurchaseUI()
//     {
//         if (continueCostText != null)
//         {
//             continueCostText.text =
//                 continueCost.ToString();
//         }

//         if (currentCoinsText != null)
//         {
//             currentCoinsText.text =
//                 scoreManager != null
//                     ? scoreManager.CurrentScore.ToString()
//                     : "0";
//         }

//         if (useCoinsButton != null)
//         {
//             useCoinsButton.interactable =
//                 scoreManager != null &&
//                 scoreManager.CanAfford(
//                     continueCost
//                 );
//         }
//     }


//     public void UseCoinsForExtraShots()
//     {
//         ResolveReferences();

//         if (!isOpen ||
//             scoreManager == null ||
//             cannonController == null)
//         {
//             return;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         if (!scoreManager.TrySpendScore(
//                 continueCost))
//         {
//             RefreshPurchaseUI();
//             return;
//         }

//         bool shotsGranted =
//             cannonController.AddExtraShots(
//                 extraShots
//             );

//         if (!shotsGranted)
//         {
//             scoreManager.AddScore(
//                 continueCost
//             );

//             RefreshPurchaseUI();

//             Debug.LogWarning(
//                 "OutOfMovesUIController: Extra shots grant nahi huay, coins refund kar diye gaye.",
//                 this
//             );

//             return;
//         }

//         isOpen = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }

//         Time.timeScale = 1f;
//     }


//     public void TryAgain()
//     {
//         Time.timeScale = 1f;

//         isOpen = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }

//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.RestartCurrentLevel();
//         }
//     }


//     public void Hide()
//     {
//         isOpen = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }
//     }
// }



// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public sealed class OutOfMovesUIController : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private GameObject outOfMovesPanel;
//     [SerializeField] private Button tryAgainButton;

//     [Tooltip(
//         "Coins spend karke extra shots lene wala button."
//     )]
//     [SerializeField] private Button useCoinsButton;

//     [Tooltip(
//         "Rewarded ad watch karke extra shots lene wala button."
//     )]
//     [SerializeField] private Button watchAdButton;

//     [Tooltip(
//         "Optional text. Example: 200"
//     )]
//     [SerializeField] private TMP_Text continueCostText;

//     [Tooltip(
//         "Optional text. Current available coins/score show karega."
//     )]
//     [SerializeField] private TMP_Text currentCoinsText;


//     [Header("Continue With Coins")]
//     [SerializeField, Min(0)]
//     private int continueCost = 200;

//     [SerializeField, Min(1)]
//     private int coinExtraShots = 5;


//     [Header("Continue With Rewarded Ad")]
//     [SerializeField, Min(1)]
//     private int rewardedExtraShots = 5;

//     [Tooltip(
//         "ON = Rewarded button tab show hoga jab player coins option afford " +
//         "na kar sake. OFF = Rewarded button hamesha show hoga."
//     )]
//     [SerializeField]
//     private bool showRewardedOnlyWhenCoinsInsufficient = true;


//     [Header("Gameplay References")]
//     [SerializeField] private CannonController cannonController;
//     [SerializeField] private LevelRuntimeController levelRuntimeController;
//     [SerializeField] private ScoreManager scoreManager;
//     [SerializeField] private GoogleAdsManager googleAdsManager;


//     [Header("Behaviour")]
//     [SerializeField] private bool pauseGameWhenOpened = true;


//     private bool isOpen;
//     private bool rewardedAdInProgress;
//     private bool rewardedAdEarned;


//     private void Awake()
//     {
//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         Subscribe();

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.onClick.RemoveListener(
//                 TryAgain
//             );

//             tryAgainButton.onClick.AddListener(
//                 TryAgain
//             );
//         }

//         if (useCoinsButton != null)
//         {
//             useCoinsButton.onClick.RemoveListener(
//                 UseCoinsForExtraShots
//             );

//             useCoinsButton.onClick.AddListener(
//                 UseCoinsForExtraShots
//             );
//         }

//         if (watchAdButton != null)
//         {
//             watchAdButton.onClick.RemoveListener(
//                 WatchRewardedAdForExtraShots
//             );

//             watchAdButton.onClick.AddListener(
//                 WatchRewardedAdForExtraShots
//             );
//         }

//         RefreshContinueUI();
//     }


//     private void OnDisable()
//     {
//         Unsubscribe();

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.onClick.RemoveListener(
//                 TryAgain
//             );
//         }

//         if (useCoinsButton != null)
//         {
//             useCoinsButton.onClick.RemoveListener(
//                 UseCoinsForExtraShots
//             );
//         }

//         if (watchAdButton != null)
//         {
//             watchAdButton.onClick.RemoveListener(
//                 WatchRewardedAdForExtraShots
//             );
//         }
//     }


//     private void ResolveReferences()
//     {
//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

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

//         if (googleAdsManager == null)
//         {
//             if (GoogleAdsManager.Instance != null)
//             {
//                 googleAdsManager =
//                     GoogleAdsManager.Instance;
//             }
//             else
//             {
//                 googleAdsManager =
//                     FindFirstObjectByType<GoogleAdsManager>(
//                         FindObjectsInactive.Include
//                     );
//             }
//         }
//     }


//     private void Subscribe()
//     {
//         if (cannonController != null)
//         {
//             cannonController.OutOfMoves -=
//                 HandleOutOfMoves;

//             cannonController.OutOfMoves +=
//                 HandleOutOfMoves;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreChanged -=
//                 HandleScoreChanged;

//             scoreManager.ScoreChanged +=
//                 HandleScoreChanged;
//         }

//         if (googleAdsManager != null)
//         {
//             googleAdsManager.RewardedLoaded -=
//                 HandleRewardedLoaded;

//             googleAdsManager.RewardedLoaded +=
//                 HandleRewardedLoaded;

//             googleAdsManager.RewardedFailedToLoad -=
//                 HandleRewardedFailedToLoad;

//             googleAdsManager.RewardedFailedToLoad +=
//                 HandleRewardedFailedToLoad;
//         }
//     }


//     private void Unsubscribe()
//     {
//         if (cannonController != null)
//         {
//             cannonController.OutOfMoves -=
//                 HandleOutOfMoves;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreChanged -=
//                 HandleScoreChanged;
//         }

//         if (googleAdsManager != null)
//         {
//             googleAdsManager.RewardedLoaded -=
//                 HandleRewardedLoaded;

//             googleAdsManager.RewardedFailedToLoad -=
//                 HandleRewardedFailedToLoad;
//         }
//     }


//     private void HandleOutOfMoves()
//     {
//         if (isOpen)
//         {
//             return;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         isOpen = true;
//         rewardedAdInProgress = false;
//         rewardedAdEarned = false;

//         ResolveReferences();
//         RefreshContinueUI();

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(true);
//         }

//         if (pauseGameWhenOpened)
//         {
//             Time.timeScale = 0f;
//         }
//     }


//     private void HandleScoreChanged(
//         int newScore)
//     {
//         RefreshContinueUI();
//     }


//     private void HandleRewardedLoaded()
//     {
//         RefreshContinueUI();
//     }


//     private void HandleRewardedFailedToLoad(
//         string error)
//     {
//         RefreshContinueUI();
//     }


//     private void RefreshContinueUI()
//     {
//         bool canAffordCoins =
//             scoreManager != null &&
//             scoreManager.CanAfford(
//                 continueCost
//             );

//         if (continueCostText != null)
//         {
//             continueCostText.text =
//                 continueCost.ToString();
//         }

//         if (currentCoinsText != null)
//         {
//             currentCoinsText.text =
//                 scoreManager != null
//                     ? scoreManager.CurrentScore.ToString()
//                     : "0";
//         }

//         if (useCoinsButton != null)
//         {
//             useCoinsButton.interactable =
//                 !rewardedAdInProgress &&
//                 canAffordCoins;
//         }

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.interactable =
//                 !rewardedAdInProgress;
//         }

//         if (watchAdButton != null)
//         {
//             bool shouldShowRewardedButton =
//                 !showRewardedOnlyWhenCoinsInsufficient ||
//                 !canAffordCoins;

//             watchAdButton.gameObject.SetActive(
//                 shouldShowRewardedButton
//             );

//             watchAdButton.interactable =
//                 shouldShowRewardedButton &&
//                 !rewardedAdInProgress &&
//                 googleAdsManager != null &&
//                 googleAdsManager.IsRewardedReady;
//         }
//     }


//     public void UseCoinsForExtraShots()
//     {
//         ResolveReferences();

//         if (!isOpen ||
//             rewardedAdInProgress ||
//             scoreManager == null ||
//             cannonController == null)
//         {
//             return;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         if (!scoreManager.TrySpendScore(
//                 continueCost))
//         {
//             RefreshContinueUI();
//             return;
//         }

//         bool shotsGranted =
//             cannonController.AddExtraShots(
//                 coinExtraShots
//             );

//         if (!shotsGranted)
//         {
//             scoreManager.AddScore(
//                 continueCost
//             );

//             RefreshContinueUI();

//             Debug.LogWarning(
//                 "OutOfMovesUIController: Coin extra shots grant nahi huay, coins refund kar diye gaye.",
//                 this
//             );

//             return;
//         }

//         ResumeGameplayAfterExtraShots();
//     }


//     public void WatchRewardedAdForExtraShots()
//     {
//         ResolveReferences();

//         if (!isOpen ||
//             rewardedAdInProgress ||
//             cannonController == null ||
//             googleAdsManager == null)
//         {
//             return;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         if (!googleAdsManager.IsRewardedReady)
//         {
//             RefreshContinueUI();

//             Debug.Log(
//                 "OutOfMovesUIController: Rewarded ad abhi ready nahi hai.",
//                 this
//             );

//             return;
//         }

//         rewardedAdInProgress = true;
//         rewardedAdEarned = false;

//         RefreshContinueUI();

//         bool adStarted =
//             googleAdsManager.ShowRewarded(
//                 HandleRewardEarned,
//                 HandleRewardedAdClosed
//             );

//         if (!adStarted)
//         {
//             rewardedAdInProgress = false;
//             rewardedAdEarned = false;

//             RefreshContinueUI();
//         }
//     }


//     private void HandleRewardEarned()
//     {
//         rewardedAdEarned = true;
//     }


//     private void HandleRewardedAdClosed()
//     {
//         rewardedAdInProgress = false;

//         if (!isOpen)
//         {
//             rewardedAdEarned = false;
//             return;
//         }

//         if (!rewardedAdEarned)
//         {
//             RefreshContinueUI();
//             return;
//         }

//         rewardedAdEarned = false;

//         bool shotsGranted =
//             cannonController != null &&
//             cannonController.AddExtraShots(
//                 rewardedExtraShots
//             );

//         if (!shotsGranted)
//         {
//             Debug.LogWarning(
//                 "OutOfMovesUIController: Reward earned hua lekin extra shots grant nahi ho sake.",
//                 this
//             );

//             RefreshContinueUI();
//             return;
//         }

//         ResumeGameplayAfterExtraShots();
//     }


//     private void ResumeGameplayAfterExtraShots()
//     {
//         isOpen = false;
//         rewardedAdInProgress = false;
//         rewardedAdEarned = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }

//         Time.timeScale = 1f;
//     }


//     public void TryAgain()
//     {
//         if (rewardedAdInProgress)
//         {
//             return;
//         }

//         Time.timeScale = 1f;

//         isOpen = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }

//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.RestartCurrentLevel();
//         }
//         else
//         {
//             Debug.LogWarning(
//                 "OutOfMovesUIController: LevelRuntimeController missing hai.",
//                 this
//             );
//         }
//     }


//     public void Hide()
//     {
//         isOpen = false;
//         rewardedAdInProgress = false;
//         rewardedAdEarned = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }
//     }
// }




// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public sealed class OutOfMovesUIController : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private GameObject outOfMovesPanel;
//     [SerializeField] private Button tryAgainButton;

//     [Tooltip(
//         "Coins spend karke extra shots lene wala button."
//     )]
//     [SerializeField] private Button useCoinsButton;

//     [Tooltip(
//         "Rewarded ad watch karke extra shots lene wala button."
//     )]
//     [SerializeField] private Button watchAdButton;

//     [Tooltip(
//         "Optional text. Example: 200"
//     )]
//     [SerializeField] private TMP_Text continueCostText;

//     [Tooltip(
//         "Optional text. Current available coins/score show karega."
//     )]
//     [SerializeField] private TMP_Text currentCoinsText;


//     [Header("Continue With Coins")]
//     [SerializeField, Min(0)]
//     private int continueCost = 200;

//     [SerializeField, Min(1)]
//     private int coinExtraShots = 5;


//     [Header("Continue With Rewarded Ad")]
//     [SerializeField, Min(1)]
//     private int rewardedExtraShots = 5;

//     [Tooltip(
//         "ON = Rewarded button tab show hoga jab player coins option afford " +
//         "na kar sake. OFF = Rewarded button hamesha show hoga."
//     )]
//     [SerializeField]
//     private bool showRewardedOnlyWhenCoinsInsufficient = true;


//     [Header("Gameplay References")]
//     [SerializeField] private CannonController cannonController;
//     [SerializeField] private LevelRuntimeController levelRuntimeController;
//     [SerializeField] private ScoreManager scoreManager;
//     [SerializeField] private GoogleAdsManager googleAdsManager;

//     [SerializeField]
//     private PopupGameplayVisibilityController popupGameplayVisibility;


//     [Header("Behaviour")]
//     [SerializeField] private bool pauseGameWhenOpened = true;


//     private bool isOpen;
//     private bool rewardedAdInProgress;
//     private bool rewardedAdEarned;


//     private void Awake()
//     {
//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         Subscribe();

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.onClick.RemoveListener(
//                 TryAgain
//             );

//             tryAgainButton.onClick.AddListener(
//                 TryAgain
//             );
//         }

//         if (useCoinsButton != null)
//         {
//             useCoinsButton.onClick.RemoveListener(
//                 UseCoinsForExtraShots
//             );

//             useCoinsButton.onClick.AddListener(
//                 UseCoinsForExtraShots
//             );
//         }

//         if (watchAdButton != null)
//         {
//             watchAdButton.onClick.RemoveListener(
//                 WatchRewardedAdForExtraShots
//             );

//             watchAdButton.onClick.AddListener(
//                 WatchRewardedAdForExtraShots
//             );
//         }

//         RefreshContinueUI();
//     }


//     private void OnDisable()
//     {
//         Unsubscribe();

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.onClick.RemoveListener(
//                 TryAgain
//             );
//         }

//         if (useCoinsButton != null)
//         {
//             useCoinsButton.onClick.RemoveListener(
//                 UseCoinsForExtraShots
//             );
//         }

//         if (watchAdButton != null)
//         {
//             watchAdButton.onClick.RemoveListener(
//                 WatchRewardedAdForExtraShots
//             );
//         }
//     }


//     private void ResolveReferences()
//     {
//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

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

//         if (googleAdsManager == null)
//         {
//             if (GoogleAdsManager.Instance != null)
//             {
//                 googleAdsManager =
//                     GoogleAdsManager.Instance;
//             }
//             else
//             {
//                 googleAdsManager =
//                     FindFirstObjectByType<GoogleAdsManager>(
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
//         if (cannonController != null)
//         {
//             cannonController.OutOfMoves -=
//                 HandleOutOfMoves;

//             cannonController.OutOfMoves +=
//                 HandleOutOfMoves;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreChanged -=
//                 HandleScoreChanged;

//             scoreManager.ScoreChanged +=
//                 HandleScoreChanged;
//         }

//         if (googleAdsManager != null)
//         {
//             googleAdsManager.RewardedLoaded -=
//                 HandleRewardedLoaded;

//             googleAdsManager.RewardedLoaded +=
//                 HandleRewardedLoaded;

//             googleAdsManager.RewardedFailedToLoad -=
//                 HandleRewardedFailedToLoad;

//             googleAdsManager.RewardedFailedToLoad +=
//                 HandleRewardedFailedToLoad;
//         }
//     }


//     private void Unsubscribe()
//     {
//         if (cannonController != null)
//         {
//             cannonController.OutOfMoves -=
//                 HandleOutOfMoves;
//         }

//         if (scoreManager != null)
//         {
//             scoreManager.ScoreChanged -=
//                 HandleScoreChanged;
//         }

//         if (googleAdsManager != null)
//         {
//             googleAdsManager.RewardedLoaded -=
//                 HandleRewardedLoaded;

//             googleAdsManager.RewardedFailedToLoad -=
//                 HandleRewardedFailedToLoad;
//         }
//     }


//     private void HandleOutOfMoves()
//     {
//         if (isOpen)
//         {
//             return;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         isOpen = true;
//         rewardedAdInProgress = false;
//         rewardedAdEarned = false;

//         ResolveReferences();
//         RefreshContinueUI();

//         popupGameplayVisibility?.HideGameplay();

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(true);
//         }

//         if (pauseGameWhenOpened)
//         {
//             Time.timeScale = 0f;
//         }
//     }


//     private void HandleScoreChanged(
//         int newScore)
//     {
//         RefreshContinueUI();
//     }


//     private void HandleRewardedLoaded()
//     {
//         RefreshContinueUI();
//     }


//     private void HandleRewardedFailedToLoad(
//         string error)
//     {
//         RefreshContinueUI();
//     }


//     private void RefreshContinueUI()
//     {
//         bool canAffordCoins =
//             scoreManager != null &&
//             scoreManager.CanAfford(
//                 continueCost
//             );

//         if (continueCostText != null)
//         {
//             continueCostText.text =
//                 continueCost.ToString();
//         }

//         if (currentCoinsText != null)
//         {
//             currentCoinsText.text =
//                 scoreManager != null
//                     ? scoreManager.CurrentScore.ToString()
//                     : "0";
//         }

//         if (useCoinsButton != null)
//         {
//             useCoinsButton.interactable =
//                 !rewardedAdInProgress &&
//                 canAffordCoins;
//         }

//         if (tryAgainButton != null)
//         {
//             tryAgainButton.interactable =
//                 !rewardedAdInProgress;
//         }

//         if (watchAdButton != null)
//         {
//             bool shouldShowRewardedButton =
//                 !showRewardedOnlyWhenCoinsInsufficient ||
//                 !canAffordCoins;

//             watchAdButton.gameObject.SetActive(
//                 shouldShowRewardedButton
//             );

//             watchAdButton.interactable =
//                 shouldShowRewardedButton &&
//                 !rewardedAdInProgress &&
//                 googleAdsManager != null &&
//                 googleAdsManager.IsRewardedReady;
//         }
//     }


//     public void UseCoinsForExtraShots()
//     {
//         ResolveReferences();

//         if (!isOpen ||
//             rewardedAdInProgress ||
//             scoreManager == null ||
//             cannonController == null)
//         {
//             return;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         if (!scoreManager.TrySpendScore(
//                 continueCost))
//         {
//             RefreshContinueUI();
//             return;
//         }

//         bool shotsGranted =
//             cannonController.AddExtraShots(
//                 coinExtraShots
//             );

//         if (!shotsGranted)
//         {
//             /*
//              * Safety refund:
//              * Agar kisi reason se shots grant na hon to coins wapas.
//              */
//             scoreManager.AddScore(
//                 continueCost
//             );

//             RefreshContinueUI();

//             Debug.LogWarning(
//                 "OutOfMovesUIController: Coin extra shots grant nahi huay, coins refund kar diye gaye.",
//                 this
//             );

//             return;
//         }

//         ResumeGameplayAfterExtraShots();
//     }


//     public void WatchRewardedAdForExtraShots()
//     {
//         ResolveReferences();

//         if (!isOpen ||
//             rewardedAdInProgress ||
//             cannonController == null ||
//             googleAdsManager == null)
//         {
//             return;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         if (!googleAdsManager.IsRewardedReady)
//         {
//             RefreshContinueUI();

//             Debug.Log(
//                 "OutOfMovesUIController: Rewarded ad abhi ready nahi hai.",
//                 this
//             );

//             return;
//         }

//         rewardedAdInProgress = true;
//         rewardedAdEarned = false;

//         RefreshContinueUI();

//         bool adStarted =
//             googleAdsManager.ShowRewarded(
//                 HandleRewardEarned,
//                 HandleRewardedAdClosed
//             );

//         if (!adStarted)
//         {
//             rewardedAdInProgress = false;
//             rewardedAdEarned = false;

//             RefreshContinueUI();
//         }
//     }


//     private void HandleRewardEarned()
//     {
//         /*
//          * Google reward callback receive hua.
//          * Shots ad close hone ke baad grant karenge taake gameplay
//          * full-screen ad ke peeche resume na ho.
//          */
//         rewardedAdEarned = true;
//     }


//     private void HandleRewardedAdClosed()
//     {
//         rewardedAdInProgress = false;

//         if (!isOpen)
//         {
//             rewardedAdEarned = false;
//             return;
//         }

//         if (!rewardedAdEarned)
//         {
//             /*
//              * User ne reward earn nahi kiya ya ad show fail hui.
//              * Popup open hi rahega, koi balls grant nahi hongi.
//              */
//             RefreshContinueUI();
//             return;
//         }

//         rewardedAdEarned = false;

//         bool shotsGranted =
//             cannonController != null &&
//             cannonController.AddExtraShots(
//                 rewardedExtraShots
//             );

//         if (!shotsGranted)
//         {
//             Debug.LogWarning(
//                 "OutOfMovesUIController: Reward earned hua lekin extra shots grant nahi ho sake.",
//                 this
//             );

//             RefreshContinueUI();
//             return;
//         }

//         ResumeGameplayAfterExtraShots();
//     }


//     private void ResumeGameplayAfterExtraShots()
//     {
//         isOpen = false;
//         rewardedAdInProgress = false;
//         rewardedAdEarned = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }

//         popupGameplayVisibility?.ShowGameplay();

//         Time.timeScale = 1f;
//     }


//     public void TryAgain()
//     {
//         if (rewardedAdInProgress)
//         {
//             return;
//         }

//         Time.timeScale = 1f;

//         isOpen = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }

//         popupGameplayVisibility?.ShowGameplay();

//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.RestartCurrentLevel();
//         }
//         else
//         {
//             Debug.LogWarning(
//                 "OutOfMovesUIController: LevelRuntimeController missing hai.",
//                 this
//             );
//         }
//     }


//     public void Hide()
//     {
//         isOpen = false;
//         rewardedAdInProgress = false;
//         rewardedAdEarned = false;

//         if (outOfMovesPanel != null)
//         {
//             outOfMovesPanel.SetActive(false);
//         }

//         popupGameplayVisibility?.ShowGameplay();
//     }
// }




using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class OutOfMovesUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject outOfMovesPanel;
    [SerializeField] private Button tryAgainButton;

    [Tooltip(
        "Coins spend karke extra shots lene wala button."
    )]
    [SerializeField] private Button useCoinsButton;

    [Tooltip(
        "Rewarded ad watch karke extra shots lene wala button."
    )]
    [SerializeField] private Button watchAdButton;

    [Tooltip(
        "Optional text. Example: 200"
    )]
    [SerializeField] private TMP_Text continueCostText;

    [Tooltip(
        "Optional text. Current available coins/score show karega."
    )]
    [SerializeField] private TMP_Text currentCoinsText;


    [Header("Continue With Coins")]
    [SerializeField, Min(0)]
    private int continueCost = 200;

    [SerializeField, Min(1)]
    private int coinExtraShots = 5;


    [Header("Continue With Rewarded Ad")]
    [SerializeField, Min(1)]
    private int rewardedExtraShots = 5;

    [Tooltip(
        "Legacy field only. Is value ko ignore kiya jata hai. " +
        "Rewarded button hamesha visible rahega; ad ready na ho to sirf disabled hoga."
    )]
    [SerializeField]
    private bool showRewardedOnlyWhenCoinsInsufficient = false;


    [Header("Gameplay References")]
    [SerializeField] private CannonController cannonController;
    [SerializeField] private LevelRuntimeController levelRuntimeController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GoogleAdsManager googleAdsManager;

    [SerializeField]
    private PopupGameplayVisibilityController popupGameplayVisibility;


    [Header("Behaviour")]
    [SerializeField] private bool pauseGameWhenOpened = true;


    private bool isOpen;
    private bool rewardedAdInProgress;
    private bool rewardedAdEarned;


    private void Awake()
    {
        if (outOfMovesPanel != null)
        {
            outOfMovesPanel.SetActive(false);
        }
    }


    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();

        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.RemoveListener(
                TryAgain
            );

            tryAgainButton.onClick.AddListener(
                TryAgain
            );
        }

        if (useCoinsButton != null)
        {
            useCoinsButton.onClick.RemoveListener(
                UseCoinsForExtraShots
            );

            useCoinsButton.onClick.AddListener(
                UseCoinsForExtraShots
            );
        }

        if (watchAdButton != null)
        {
            watchAdButton.onClick.RemoveListener(
                WatchRewardedAdForExtraShots
            );

            watchAdButton.onClick.AddListener(
                WatchRewardedAdForExtraShots
            );
        }

        RefreshContinueUI();
    }


    private void OnDisable()
    {
        Unsubscribe();

        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.RemoveListener(
                TryAgain
            );
        }

        if (useCoinsButton != null)
        {
            useCoinsButton.onClick.RemoveListener(
                UseCoinsForExtraShots
            );
        }

        if (watchAdButton != null)
        {
            watchAdButton.onClick.RemoveListener(
                WatchRewardedAdForExtraShots
            );
        }
    }


    private void ResolveReferences()
    {
        if (cannonController == null)
        {
            cannonController =
                FindFirstObjectByType<CannonController>(
                    FindObjectsInactive.Include
                );
        }

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

        if (googleAdsManager == null)
        {
            if (GoogleAdsManager.Instance != null)
            {
                googleAdsManager =
                    GoogleAdsManager.Instance;
            }
            else
            {
                googleAdsManager =
                    FindFirstObjectByType<GoogleAdsManager>(
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
        if (cannonController != null)
        {
            cannonController.OutOfMoves -=
                HandleOutOfMoves;

            cannonController.OutOfMoves +=
                HandleOutOfMoves;
        }

        if (scoreManager != null)
        {
            scoreManager.ScoreChanged -=
                HandleScoreChanged;

            scoreManager.ScoreChanged +=
                HandleScoreChanged;
        }

        if (googleAdsManager != null)
        {
            googleAdsManager.RewardedLoaded -=
                HandleRewardedLoaded;

            googleAdsManager.RewardedLoaded +=
                HandleRewardedLoaded;

            googleAdsManager.RewardedFailedToLoad -=
                HandleRewardedFailedToLoad;

            googleAdsManager.RewardedFailedToLoad +=
                HandleRewardedFailedToLoad;
        }
    }


    private void Unsubscribe()
    {
        if (cannonController != null)
        {
            cannonController.OutOfMoves -=
                HandleOutOfMoves;
        }

        if (scoreManager != null)
        {
            scoreManager.ScoreChanged -=
                HandleScoreChanged;
        }

        if (googleAdsManager != null)
        {
            googleAdsManager.RewardedLoaded -=
                HandleRewardedLoaded;

            googleAdsManager.RewardedFailedToLoad -=
                HandleRewardedFailedToLoad;
        }
    }


    private void HandleOutOfMoves()
    {
        if (isOpen)
        {
            return;
        }

        if (levelRuntimeController != null &&
            !levelRuntimeController.IsLevelGenerated)
        {
            return;
        }

        isOpen = true;
        rewardedAdInProgress = false;
        rewardedAdEarned = false;

        ResolveReferences();
        RefreshContinueUI();

        /*
         * Out Of Moves popup open hote hi:
         * - cannon hide
         * - table hide
         * - tower/level blocks hide
         * - fired balls destroy
         *
         * Ye PopupGameplayVisibilityController handle karta hai.
         */
        popupGameplayVisibility?.HideGameplay();

        if (outOfMovesPanel != null)
        {
            outOfMovesPanel.SetActive(true);
        }

        /*
         * Panel active hone ke baad ek final refresh.
         * Rewarded button ko kabhi hide nahi karna:
         * ad ready = enabled
         * ad not ready = visible but disabled
         */
        RefreshContinueUI();

        if (pauseGameWhenOpened)
        {
            Time.timeScale = 0f;
        }
    }


    private void HandleScoreChanged(
        int newScore)
    {
        RefreshContinueUI();
    }


    private void HandleRewardedLoaded()
    {
        RefreshContinueUI();
    }


    private void HandleRewardedFailedToLoad(
        string error)
    {
        RefreshContinueUI();
    }


    private void RefreshContinueUI()
    {
        bool canAffordCoins =
            scoreManager != null &&
            scoreManager.CanAfford(
                continueCost
            );

        if (continueCostText != null)
        {
            continueCostText.text =
                continueCost.ToString();
        }

        if (currentCoinsText != null)
        {
            currentCoinsText.text =
                scoreManager != null
                    ? scoreManager.CurrentScore.ToString()
                    : "0";
        }

        if (useCoinsButton != null)
        {
            useCoinsButton.interactable =
                !rewardedAdInProgress &&
                canAffordCoins;
        }

        if (tryAgainButton != null)
        {
            tryAgainButton.interactable =
                !rewardedAdInProgress;
        }

        if (watchAdButton != null)
        {
            /*
             * Rewarded Ad button hamesha visible rahega.
             *
             * Ad ready ho:
             * -> button enabled
             *
             * Ad ready na ho / ad in progress ho:
             * -> button visible rahega
             * -> sirf disabled hoga
             *
             * Coins afford kar sakta ho ya nahi,
             * is se Rewarded button ki visibility affect nahi hogi.
             */
            watchAdButton.gameObject.SetActive(
                true
            );

            watchAdButton.interactable =
                !rewardedAdInProgress &&
                googleAdsManager != null &&
                googleAdsManager.IsRewardedReady;
        }
    }


    public void UseCoinsForExtraShots()
    {
        ResolveReferences();

        if (!isOpen ||
            rewardedAdInProgress ||
            scoreManager == null ||
            cannonController == null)
        {
            return;
        }

        if (levelRuntimeController != null &&
            !levelRuntimeController.IsLevelGenerated)
        {
            return;
        }

        if (!scoreManager.TrySpendScore(
                continueCost))
        {
            RefreshContinueUI();
            return;
        }

        bool shotsGranted =
            cannonController.AddExtraShots(
                coinExtraShots
            );

        if (!shotsGranted)
        {
            /*
             * Safety refund:
             * Agar kisi reason se shots grant na hon to coins wapas.
             */
            scoreManager.AddScore(
                continueCost
            );

            RefreshContinueUI();

            Debug.LogWarning(
                "OutOfMovesUIController: Coin extra shots grant nahi huay, coins refund kar diye gaye.",
                this
            );

            return;
        }

        ResumeGameplayAfterExtraShots();
    }


    public void WatchRewardedAdForExtraShots()
    {
        ResolveReferences();

        if (!isOpen ||
            rewardedAdInProgress ||
            cannonController == null ||
            googleAdsManager == null)
        {
            return;
        }

        if (levelRuntimeController != null &&
            !levelRuntimeController.IsLevelGenerated)
        {
            return;
        }

        if (!googleAdsManager.IsRewardedReady)
        {
            RefreshContinueUI();

            Debug.Log(
                "OutOfMovesUIController: Rewarded ad abhi ready nahi hai.",
                this
            );

            return;
        }

        rewardedAdInProgress = true;
        rewardedAdEarned = false;

        RefreshContinueUI();

        bool adStarted =
            googleAdsManager.ShowRewarded(
                HandleRewardEarned,
                HandleRewardedAdClosed
            );

        if (!adStarted)
        {
            rewardedAdInProgress = false;
            rewardedAdEarned = false;

            RefreshContinueUI();
        }
    }


    private void HandleRewardEarned()
    {
        /*
         * Google reward callback receive hua.
         * Shots ad close hone ke baad grant karenge taake gameplay
         * full-screen ad ke peeche resume na ho.
         */
        rewardedAdEarned = true;
    }


    private void HandleRewardedAdClosed()
    {
        rewardedAdInProgress = false;

        if (!isOpen)
        {
            rewardedAdEarned = false;
            return;
        }

        if (!rewardedAdEarned)
        {
            /*
             * User ne reward earn nahi kiya ya ad show fail hui.
             * Popup open hi rahega, koi balls grant nahi hongi.
             */
            RefreshContinueUI();
            return;
        }

        rewardedAdEarned = false;

        bool shotsGranted =
            cannonController != null &&
            cannonController.AddExtraShots(
                rewardedExtraShots
            );

        if (!shotsGranted)
        {
            Debug.LogWarning(
                "OutOfMovesUIController: Reward earned hua lekin extra shots grant nahi ho sake.",
                this
            );

            RefreshContinueUI();
            return;
        }

        ResumeGameplayAfterExtraShots();
    }


    private void ResumeGameplayAfterExtraShots()
    {
        isOpen = false;
        rewardedAdInProgress = false;
        rewardedAdEarned = false;

        if (outOfMovesPanel != null)
        {
            outOfMovesPanel.SetActive(false);
        }

        popupGameplayVisibility?.ShowGameplay();

        Time.timeScale = 1f;
    }


    public void TryAgain()
    {
        if (rewardedAdInProgress)
        {
            return;
        }

        Time.timeScale = 1f;

        isOpen = false;

        if (outOfMovesPanel != null)
        {
            outOfMovesPanel.SetActive(false);
        }

        popupGameplayVisibility?.ShowGameplay();

        if (levelRuntimeController != null)
        {
            levelRuntimeController.RestartCurrentLevel();
        }
        else
        {
            Debug.LogWarning(
                "OutOfMovesUIController: LevelRuntimeController missing hai.",
                this
            );
        }
    }


    public void Hide()
    {
        isOpen = false;
        rewardedAdInProgress = false;
        rewardedAdEarned = false;

        if (outOfMovesPanel != null)
        {
            outOfMovesPanel.SetActive(false);
        }

        popupGameplayVisibility?.ShowGameplay();
    }
}




