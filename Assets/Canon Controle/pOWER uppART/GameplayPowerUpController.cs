// using System;
// using System.Collections;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public enum GameplayPowerUpType
// {
//     InfiniteBalls,
//     PowerCannon
// }


// public sealed class GameplayPowerUpController : MonoBehaviour
// {
//     [Header("Buttons")]
//     [SerializeField] private Button infiniteBallsButton;
//     [SerializeField] private Button powerCannonButton;


//     [Header("Single Top Timer UI")]
//     [Tooltip(
//         "Screen ke top par single TMP Text. " +
//         "Jo power-up active hoga usi ka timer yahan show hoga."
//     )]
//     [SerializeField] private TMP_Text powerUpTimerText;


//     [Header("Infinite Balls")]
//     [Tooltip("Infinite Balls kitne seconds active rahega.")]
//     [SerializeField, Min(0.1f)]
//     private float infiniteBallsDuration = 15f;


//     [Header("Power Cannon")]
//     [Tooltip("Power Cannon kitne seconds active rahega.")]
//     [SerializeField, Min(0.1f)]
//     private float powerCannonDuration = 15f;

//     [SerializeField, Min(1f)]
//     private float cannonSizeMultiplier = 1.25f;

//     [SerializeField, Min(1f)]
//     private float cannonBallSizeMultiplier = 1.5f;

//     [Tooltip("Cannon ball ki launch speed/force ka multiplier.")]
//     [SerializeField, Min(1f)]
//     private float launchForceMultiplier = 1.5f;


//     [Header("References")]
//     [SerializeField] private CannonController cannonController;
//     [SerializeField] private LevelRuntimeController levelRuntimeController;

//     [Tooltip(
//         "Complete visible cannon root. Isi Transform ka size Power Cannon par increase hoga."
//     )]
//     [SerializeField] private Transform cannonVisualRoot;


//     private Vector3 originalCannonScale;
//     private bool originalScaleCached;

//     private bool infiniteBallsUsedThisLevel;
//     private bool powerCannonUsedThisLevel;

//     private bool infiniteBallsActive;
//     private bool powerCannonActive;

//     private float infiniteBallsRemainingTime;
//     private float powerCannonRemainingTime;

//     private GameplayPowerUpType lastActivatedPowerUp;

//     private Coroutine infiniteBallsRoutine;
//     private Coroutine powerCannonRoutine;


//     public bool InfiniteBallsUsedThisLevel =>
//         infiniteBallsUsedThisLevel;

//     public bool PowerCannonUsedThisLevel =>
//         powerCannonUsedThisLevel;

//     public bool AnyPowerUpUsedThisLevel =>
//         infiniteBallsUsedThisLevel ||
//         powerCannonUsedThisLevel;

//     public bool InfiniteBallsActive =>
//         infiniteBallsActive;

//     public bool PowerCannonActive =>
//         powerCannonActive;


//     public event Action<GameplayPowerUpType> PowerUpActivated;


//     private void Awake()
//     {
//         ResolveReferences();
//         CacheOriginalCannonScale();
//         HideTimer();
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         CacheOriginalCannonScale();

//         if (infiniteBallsButton != null)
//         {
//             infiniteBallsButton.onClick.RemoveListener(
//                 ActivateInfiniteBalls
//             );

//             infiniteBallsButton.onClick.AddListener(
//                 ActivateInfiniteBalls
//             );
//         }

//         if (powerCannonButton != null)
//         {
//             powerCannonButton.onClick.RemoveListener(
//                 ActivatePowerCannon
//             );

//             powerCannonButton.onClick.AddListener(
//                 ActivatePowerCannon
//             );
//         }

//         SubscribeToLevel();
//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     private void OnDisable()
//     {
//         if (infiniteBallsButton != null)
//         {
//             infiniteBallsButton.onClick.RemoveListener(
//                 ActivateInfiniteBalls
//             );
//         }

//         if (powerCannonButton != null)
//         {
//             powerCannonButton.onClick.RemoveListener(
//                 ActivatePowerCannon
//             );
//         }

//         UnsubscribeFromLevel();
//     }


//     private void Update()
//     {
//         RefreshTopTimer();
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

//         if (cannonVisualRoot == null &&
//             cannonController != null)
//         {
//             cannonVisualRoot =
//                 cannonController.transform;
//         }
//     }


//     private void CacheOriginalCannonScale()
//     {
//         if (originalScaleCached ||
//             cannonVisualRoot == null)
//         {
//             return;
//         }

//         originalCannonScale =
//             cannonVisualRoot.localScale;

//         originalScaleCached = true;
//     }


//     private void SubscribeToLevel()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelGenerated -=
//             HandleLevelGenerated;

//         levelRuntimeController.LevelGenerated +=
//             HandleLevelGenerated;

//         levelRuntimeController.LevelCompleted -=
//             HandleLevelCompleted;

//         levelRuntimeController.LevelCompleted +=
//             HandleLevelCompleted;
//     }


//     private void UnsubscribeFromLevel()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelGenerated -=
//             HandleLevelGenerated;

//         levelRuntimeController.LevelCompleted -=
//             HandleLevelCompleted;
//     }


//     public void ActivateInfiniteBalls()
//     {
//         ResolveReferences();

//         if (!CanUsePowerUp() ||
//             infiniteBallsUsedThisLevel ||
//             infiniteBallsActive ||
//             cannonController == null)
//         {
//             return;
//         }

//         infiniteBallsUsedThisLevel = true;
//         infiniteBallsActive = true;

//         infiniteBallsRemainingTime =
//             Mathf.Max(
//                 0.1f,
//                 infiniteBallsDuration
//             );

//         lastActivatedPowerUp =
//             GameplayPowerUpType.InfiniteBalls;

//         cannonController.SetInfiniteBallsEnabled(
//             true
//         );

//         if (infiniteBallsRoutine != null)
//         {
//             StopCoroutine(
//                 infiniteBallsRoutine
//             );
//         }

//         infiniteBallsRoutine =
//             StartCoroutine(
//                 InfiniteBallsTimerRoutine()
//             );

//         PowerUpActivated?.Invoke(
//             GameplayPowerUpType.InfiniteBalls
//         );

//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     public void ActivatePowerCannon()
//     {
//         ResolveReferences();
//         CacheOriginalCannonScale();

//         if (!CanUsePowerUp() ||
//             powerCannonUsedThisLevel ||
//             powerCannonActive ||
//             cannonController == null)
//         {
//             return;
//         }

//         powerCannonUsedThisLevel = true;
//         powerCannonActive = true;

//         powerCannonRemainingTime =
//             Mathf.Max(
//                 0.1f,
//                 powerCannonDuration
//             );

//         lastActivatedPowerUp =
//             GameplayPowerUpType.PowerCannon;

//         if (cannonVisualRoot != null &&
//             originalScaleCached)
//         {
//             cannonVisualRoot.localScale =
//                 originalCannonScale *
//                 cannonSizeMultiplier;
//         }

//         cannonController
//             .SetRuntimeBallSizeMultiplier(
//                 cannonBallSizeMultiplier
//             );

//         cannonController
//             .SetRuntimeLaunchForceMultiplier(
//                 launchForceMultiplier
//             );

//         if (powerCannonRoutine != null)
//         {
//             StopCoroutine(
//                 powerCannonRoutine
//             );
//         }

//         powerCannonRoutine =
//             StartCoroutine(
//                 PowerCannonTimerRoutine()
//             );

//         PowerUpActivated?.Invoke(
//             GameplayPowerUpType.PowerCannon
//         );

//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     private IEnumerator InfiniteBallsTimerRoutine()
//     {
//         while (infiniteBallsRemainingTime > 0f &&
//                infiniteBallsActive)
//         {
//             yield return null;

//             infiniteBallsRemainingTime -=
//                 Time.deltaTime;
//         }

//         infiniteBallsRemainingTime = 0f;

//         EndInfiniteBalls();
//     }


//     private IEnumerator PowerCannonTimerRoutine()
//     {
//         while (powerCannonRemainingTime > 0f &&
//                powerCannonActive)
//         {
//             yield return null;

//             powerCannonRemainingTime -=
//                 Time.deltaTime;
//         }

//         powerCannonRemainingTime = 0f;

//         EndPowerCannon();
//     }


//     private void EndInfiniteBalls()
//     {
//         infiniteBallsActive = false;
//         infiniteBallsRemainingTime = 0f;

//         if (cannonController != null)
//         {
//             cannonController.SetInfiniteBallsEnabled(
//                 false
//             );
//         }

//         infiniteBallsRoutine = null;

//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     private void EndPowerCannon()
//     {
//         powerCannonActive = false;
//         powerCannonRemainingTime = 0f;

//         if (cannonVisualRoot != null &&
//             originalScaleCached)
//         {
//             cannonVisualRoot.localScale =
//                 originalCannonScale;
//         }

//         if (cannonController != null)
//         {
//             cannonController
//                 .SetRuntimeBallSizeMultiplier(
//                     1f
//                 );

//             cannonController
//                 .SetRuntimeLaunchForceMultiplier(
//                     1f
//                 );
//         }

//         powerCannonRoutine = null;

//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     private void RefreshTopTimer()
//     {
//         if (powerUpTimerText == null)
//         {
//             return;
//         }

//         bool showInfinite =
//             infiniteBallsActive;

//         bool showPowerCannon =
//             powerCannonActive;

//         if (!showInfinite &&
//             !showPowerCannon)
//         {
//             HideTimer();
//             return;
//         }

//         float timeToShow;

//         if (showInfinite &&
//             showPowerCannon)
//         {
//             timeToShow =
//                 lastActivatedPowerUp ==
//                 GameplayPowerUpType.InfiniteBalls
//                     ? infiniteBallsRemainingTime
//                     : powerCannonRemainingTime;
//         }
//         else if (showInfinite)
//         {
//             timeToShow =
//                 infiniteBallsRemainingTime;
//         }
//         else
//         {
//             timeToShow =
//                 powerCannonRemainingTime;
//         }

//         ShowTimer();
//         UpdateTimerText(
//             timeToShow
//         );
//     }


//     private void UpdateTimerText(
//         float remainingSeconds)
//     {
//         if (powerUpTimerText == null)
//         {
//             return;
//         }

//         int totalSeconds =
//             Mathf.Max(
//                 0,
//                 Mathf.CeilToInt(
//                     remainingSeconds
//                 )
//             );

//         int minutes =
//             totalSeconds / 60;

//         int seconds =
//             totalSeconds % 60;

//         powerUpTimerText.text =
//             $"{minutes:00}:{seconds:00}";
//     }


//     private void ShowTimer()
//     {
//         if (powerUpTimerText != null &&
//             !powerUpTimerText.gameObject.activeSelf)
//         {
//             powerUpTimerText.gameObject.SetActive(
//                 true
//             );
//         }
//     }


//     private void HideTimer()
//     {
//         if (powerUpTimerText == null)
//         {
//             return;
//         }

//         powerUpTimerText.text = "";

//         if (powerUpTimerText.gameObject.activeSelf)
//         {
//             powerUpTimerText.gameObject.SetActive(
//                 false
//             );
//         }
//     }


//     private bool CanUsePowerUp()
//     {
//         return levelRuntimeController == null ||
//             levelRuntimeController.IsLevelGenerated;
//     }


//     private void HandleLevelCompleted(
//         GridLevelData completedLevel)
//     {
//         StopActivePowerUpsForLevelComplete();
//     }


//     private void StopActivePowerUpsForLevelComplete()
//     {
//         /*
//          * Level complete hote hi dono active effects aur timer stop karte hain.
//          *
//          * IMPORTANT:
//          * infiniteBallsUsedThisLevel / powerCannonUsedThisLevel ko yahan
//          * reset NAHI karte, taake StarRatingManager ko correctly pata rahe
//          * ke is level mein power-up use hua tha aur max 1 star rule apply ho.
//          */

//         if (infiniteBallsRoutine != null)
//         {
//             StopCoroutine(
//                 infiniteBallsRoutine
//             );

//             infiniteBallsRoutine = null;
//         }

//         if (powerCannonRoutine != null)
//         {
//             StopCoroutine(
//                 powerCannonRoutine
//             );

//             powerCannonRoutine = null;
//         }

//         infiniteBallsActive = false;
//         powerCannonActive = false;

//         infiniteBallsRemainingTime = 0f;
//         powerCannonRemainingTime = 0f;

//         if (cannonController != null)
//         {
//             cannonController.SetInfiniteBallsEnabled(
//                 false
//             );

//             cannonController
//                 .SetRuntimeBallSizeMultiplier(
//                     1f
//                 );

//             cannonController
//                 .SetRuntimeLaunchForceMultiplier(
//                     1f
//                 );
//         }

//         if (cannonVisualRoot != null &&
//             originalScaleCached)
//         {
//             cannonVisualRoot.localScale =
//                 originalCannonScale;
//         }

//         HideTimer();

//         RefreshButtons();
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         ResetPowerUpsForNewLevel();
//     }


//     public void ResetPowerUpsForNewLevel()
//     {
//         if (infiniteBallsRoutine != null)
//         {
//             StopCoroutine(
//                 infiniteBallsRoutine
//             );

//             infiniteBallsRoutine = null;
//         }

//         if (powerCannonRoutine != null)
//         {
//             StopCoroutine(
//                 powerCannonRoutine
//             );

//             powerCannonRoutine = null;
//         }

//         infiniteBallsUsedThisLevel = false;
//         powerCannonUsedThisLevel = false;

//         infiniteBallsActive = false;
//         powerCannonActive = false;

//         infiniteBallsRemainingTime = 0f;
//         powerCannonRemainingTime = 0f;

//         if (cannonController != null)
//         {
//             cannonController.ResetRuntimePowerUps();
//         }

//         if (cannonVisualRoot != null &&
//             originalScaleCached)
//         {
//             cannonVisualRoot.localScale =
//                 originalCannonScale;
//         }

//         HideTimer();

//         RefreshButtons();
//     }


//     private void RefreshButtons()
//     {
//         bool levelReady =
//             levelRuntimeController == null ||
//             levelRuntimeController.IsLevelGenerated;

//         if (infiniteBallsButton != null)
//         {
//             infiniteBallsButton.interactable =
//                 levelReady &&
//                 !infiniteBallsUsedThisLevel;
//         }

//         if (powerCannonButton != null)
//         {
//             powerCannonButton.interactable =
//                 levelReady &&
//                 !powerCannonUsedThisLevel;
//         }
//     }
// }









// using System;
// using System.Collections;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public enum GameplayPowerUpType
// {
//     InfiniteBalls,
//     PowerCannon
// }


// public sealed class GameplayPowerUpController : MonoBehaviour
// {
//     [Header("Buttons")]
//     [SerializeField] private Button infiniteBallsButton;
//     [SerializeField] private Button powerCannonButton;


//     [Header("Single Top Timer UI")]
//     [Tooltip(
//         "Screen ke top par single TMP Text. " +
//         "Jo power-up active hoga usi ka timer yahan show hoga."
//     )]
//     [SerializeField] private TMP_Text powerUpTimerText;


//     [Header("Infinite Balls")]
//     [Tooltip("Infinite Balls kitne seconds active rahega.")]
//     [SerializeField, Min(0.1f)]
//     private float infiniteBallsDuration = 15f;


//     [Header("Power Cannon")]
//     [Tooltip("Power Cannon kitne seconds active rahega.")]
//     [SerializeField, Min(0.1f)]
//     private float powerCannonDuration = 15f;

//     [SerializeField, Min(1f)]
//     private float cannonSizeMultiplier = 1.25f;

//     [SerializeField, Min(1f)]
//     private float cannonBallSizeMultiplier = 1.5f;

//     [Tooltip("Cannon ball ki launch speed/force ka multiplier.")]
//     [SerializeField, Min(1f)]
//     private float launchForceMultiplier = 1.5f;


//     [Header("References")]
//     [SerializeField] private CannonController cannonController;
//     [SerializeField] private LevelRuntimeController levelRuntimeController;
//     [SerializeField] private PowerUpInventoryManager powerUpInventoryManager;

//     [Tooltip(
//         "Complete visible cannon root. Isi Transform ka size Power Cannon par increase hoga."
//     )]
//     [SerializeField] private Transform cannonVisualRoot;


//     private Vector3 originalCannonScale;
//     private bool originalScaleCached;

//     private bool infiniteBallsUsedThisLevel;
//     private bool powerCannonUsedThisLevel;

//     private bool infiniteBallsActive;
//     private bool powerCannonActive;

//     private float infiniteBallsRemainingTime;
//     private float powerCannonRemainingTime;

//     private GameplayPowerUpType lastActivatedPowerUp;

//     private Coroutine infiniteBallsRoutine;
//     private Coroutine powerCannonRoutine;


//     public bool InfiniteBallsUsedThisLevel =>
//         infiniteBallsUsedThisLevel;

//     public bool PowerCannonUsedThisLevel =>
//         powerCannonUsedThisLevel;

//     public bool AnyPowerUpUsedThisLevel =>
//         infiniteBallsUsedThisLevel ||
//         powerCannonUsedThisLevel;

//     public bool InfiniteBallsActive =>
//         infiniteBallsActive;

//     public bool PowerCannonActive =>
//         powerCannonActive;


//     public event Action<GameplayPowerUpType> PowerUpActivated;


//     private void Awake()
//     {
//         ResolveReferences();
//         CacheOriginalCannonScale();
//         HideTimer();
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         CacheOriginalCannonScale();

//         if (infiniteBallsButton != null)
//         {
//             infiniteBallsButton.onClick.RemoveListener(
//                 ActivateInfiniteBalls
//             );

//             infiniteBallsButton.onClick.AddListener(
//                 ActivateInfiniteBalls
//             );
//         }

//         if (powerCannonButton != null)
//         {
//             powerCannonButton.onClick.RemoveListener(
//                 ActivatePowerCannon
//             );

//             powerCannonButton.onClick.AddListener(
//                 ActivatePowerCannon
//             );
//         }

//         SubscribeToLevel();
//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     private void OnDisable()
//     {
//         if (infiniteBallsButton != null)
//         {
//             infiniteBallsButton.onClick.RemoveListener(
//                 ActivateInfiniteBalls
//             );
//         }

//         if (powerCannonButton != null)
//         {
//             powerCannonButton.onClick.RemoveListener(
//                 ActivatePowerCannon
//             );
//         }

//         UnsubscribeFromLevel();
//     }


//     private void Update()
//     {
//         RefreshTopTimer();
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

//         if (powerUpInventoryManager == null)
//         {
//             if (PowerUpInventoryManager.Instance != null)
//             {
//                 powerUpInventoryManager =
//                     PowerUpInventoryManager.Instance;
//             }
//             else
//             {
//                 powerUpInventoryManager =
//                     FindFirstObjectByType<PowerUpInventoryManager>(
//                         FindObjectsInactive.Include
//                     );
//             }
//         }

//         if (cannonVisualRoot == null &&
//             cannonController != null)
//         {
//             cannonVisualRoot =
//                 cannonController.transform;
//         }
//     }


//     private void CacheOriginalCannonScale()
//     {
//         if (originalScaleCached ||
//             cannonVisualRoot == null)
//         {
//             return;
//         }

//         originalCannonScale =
//             cannonVisualRoot.localScale;

//         originalScaleCached = true;
//     }


//     private void SubscribeToLevel()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelGenerated -=
//             HandleLevelGenerated;

//         levelRuntimeController.LevelGenerated +=
//             HandleLevelGenerated;

//         levelRuntimeController.LevelCompleted -=
//             HandleLevelCompleted;

//         levelRuntimeController.LevelCompleted +=
//             HandleLevelCompleted;

//         if (powerUpInventoryManager != null)
//         {
//             powerUpInventoryManager.InfiniteBallsCountChanged -=
//                 HandleInfiniteBallsCountChanged;

//             powerUpInventoryManager.InfiniteBallsCountChanged +=
//                 HandleInfiniteBallsCountChanged;

//             powerUpInventoryManager.PowerCannonCountChanged -=
//                 HandlePowerCannonCountChanged;

//             powerUpInventoryManager.PowerCannonCountChanged +=
//                 HandlePowerCannonCountChanged;
//         }
//     }


//     private void UnsubscribeFromLevel()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelGenerated -=
//             HandleLevelGenerated;

//         levelRuntimeController.LevelCompleted -=
//             HandleLevelCompleted;

//         if (powerUpInventoryManager != null)
//         {
//             powerUpInventoryManager.InfiniteBallsCountChanged -=
//                 HandleInfiniteBallsCountChanged;

//             powerUpInventoryManager.PowerCannonCountChanged -=
//                 HandlePowerCannonCountChanged;
//         }
//     }


//     public void ActivateInfiniteBalls()
//     {
//         ResolveReferences();

//         if (!CanUsePowerUp() ||
//             infiniteBallsActive ||
//             cannonController == null ||
//             powerUpInventoryManager == null ||
//             !powerUpInventoryManager.InfiniteBallsUnlocked)
//         {
//             return;
//         }

//         if (!powerUpInventoryManager.TryConsumeInfiniteBalls(1))
//         {
//             RefreshButtons();
//             return;
//         }

//         infiniteBallsUsedThisLevel = true;
//         infiniteBallsActive = true;

//         infiniteBallsRemainingTime =
//             Mathf.Max(
//                 0.1f,
//                 infiniteBallsDuration
//             );

//         lastActivatedPowerUp =
//             GameplayPowerUpType.InfiniteBalls;

//         cannonController.SetInfiniteBallsEnabled(
//             true
//         );

//         if (infiniteBallsRoutine != null)
//         {
//             StopCoroutine(
//                 infiniteBallsRoutine
//             );
//         }

//         infiniteBallsRoutine =
//             StartCoroutine(
//                 InfiniteBallsTimerRoutine()
//             );

//         PowerUpActivated?.Invoke(
//             GameplayPowerUpType.InfiniteBalls
//         );

//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     public void ActivatePowerCannon()
//     {
//         ResolveReferences();
//         CacheOriginalCannonScale();

//         if (!CanUsePowerUp() ||
//             powerCannonActive ||
//             cannonController == null ||
//             powerUpInventoryManager == null ||
//             !powerUpInventoryManager.PowerCannonUnlocked)
//         {
//             return;
//         }

//         if (!powerUpInventoryManager.TryConsumePowerCannon(1))
//         {
//             RefreshButtons();
//             return;
//         }

//         powerCannonUsedThisLevel = true;
//         powerCannonActive = true;

//         powerCannonRemainingTime =
//             Mathf.Max(
//                 0.1f,
//                 powerCannonDuration
//             );

//         lastActivatedPowerUp =
//             GameplayPowerUpType.PowerCannon;

//         if (cannonVisualRoot != null &&
//             originalScaleCached)
//         {
//             cannonVisualRoot.localScale =
//                 originalCannonScale *
//                 cannonSizeMultiplier;
//         }

//         cannonController
//             .SetRuntimeBallSizeMultiplier(
//                 cannonBallSizeMultiplier
//             );

//         cannonController
//             .SetRuntimeLaunchForceMultiplier(
//                 launchForceMultiplier
//             );

//         if (powerCannonRoutine != null)
//         {
//             StopCoroutine(
//                 powerCannonRoutine
//             );
//         }

//         powerCannonRoutine =
//             StartCoroutine(
//                 PowerCannonTimerRoutine()
//             );

//         PowerUpActivated?.Invoke(
//             GameplayPowerUpType.PowerCannon
//         );

//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     private IEnumerator InfiniteBallsTimerRoutine()
//     {
//         while (infiniteBallsRemainingTime > 0f &&
//                infiniteBallsActive)
//         {
//             yield return null;

//             infiniteBallsRemainingTime -=
//                 Time.deltaTime;
//         }

//         infiniteBallsRemainingTime = 0f;

//         EndInfiniteBalls();
//     }


//     private IEnumerator PowerCannonTimerRoutine()
//     {
//         while (powerCannonRemainingTime > 0f &&
//                powerCannonActive)
//         {
//             yield return null;

//             powerCannonRemainingTime -=
//                 Time.deltaTime;
//         }

//         powerCannonRemainingTime = 0f;

//         EndPowerCannon();
//     }


//     private void EndInfiniteBalls()
//     {
//         infiniteBallsActive = false;
//         infiniteBallsRemainingTime = 0f;

//         if (cannonController != null)
//         {
//             cannonController.SetInfiniteBallsEnabled(
//                 false
//             );
//         }

//         infiniteBallsRoutine = null;

//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     private void EndPowerCannon()
//     {
//         powerCannonActive = false;
//         powerCannonRemainingTime = 0f;

//         if (cannonVisualRoot != null &&
//             originalScaleCached)
//         {
//             cannonVisualRoot.localScale =
//                 originalCannonScale;
//         }

//         if (cannonController != null)
//         {
//             cannonController
//                 .SetRuntimeBallSizeMultiplier(
//                     1f
//                 );

//             cannonController
//                 .SetRuntimeLaunchForceMultiplier(
//                     1f
//                 );
//         }

//         powerCannonRoutine = null;

//         RefreshButtons();
//         RefreshTopTimer();
//     }


//     private void RefreshTopTimer()
//     {
//         if (powerUpTimerText == null)
//         {
//             return;
//         }

//         bool showInfinite =
//             infiniteBallsActive;

//         bool showPowerCannon =
//             powerCannonActive;

//         if (!showInfinite &&
//             !showPowerCannon)
//         {
//             HideTimer();
//             return;
//         }

//         float timeToShow;

//         if (showInfinite &&
//             showPowerCannon)
//         {
//             timeToShow =
//                 lastActivatedPowerUp ==
//                 GameplayPowerUpType.InfiniteBalls
//                     ? infiniteBallsRemainingTime
//                     : powerCannonRemainingTime;
//         }
//         else if (showInfinite)
//         {
//             timeToShow =
//                 infiniteBallsRemainingTime;
//         }
//         else
//         {
//             timeToShow =
//                 powerCannonRemainingTime;
//         }

//         ShowTimer();
//         UpdateTimerText(
//             timeToShow
//         );
//     }


//     private void UpdateTimerText(
//         float remainingSeconds)
//     {
//         if (powerUpTimerText == null)
//         {
//             return;
//         }

//         int totalSeconds =
//             Mathf.Max(
//                 0,
//                 Mathf.CeilToInt(
//                     remainingSeconds
//                 )
//             );

//         int minutes =
//             totalSeconds / 60;

//         int seconds =
//             totalSeconds % 60;

//         powerUpTimerText.text =
//             $"{minutes:00}:{seconds:00}";
//     }


//     private void ShowTimer()
//     {
//         if (powerUpTimerText != null &&
//             !powerUpTimerText.gameObject.activeSelf)
//         {
//             powerUpTimerText.gameObject.SetActive(
//                 true
//             );
//         }
//     }


//     private void HideTimer()
//     {
//         if (powerUpTimerText == null)
//         {
//             return;
//         }

//         powerUpTimerText.text = "";

//         if (powerUpTimerText.gameObject.activeSelf)
//         {
//             powerUpTimerText.gameObject.SetActive(
//                 false
//             );
//         }
//     }


//     private bool CanUsePowerUp()
//     {
//         return levelRuntimeController == null ||
//             levelRuntimeController.IsLevelGenerated;
//     }


//     private void HandleLevelCompleted(
//         GridLevelData completedLevel)
//     {
//         StopActivePowerUpsForLevelComplete();
//     }


//     private void StopActivePowerUpsForLevelComplete()
//     {
//         /*
//          * Level complete hote hi dono active effects aur timer stop karte hain.
//          *
//          * IMPORTANT:
//          * infiniteBallsUsedThisLevel / powerCannonUsedThisLevel ko yahan
//          * reset NAHI karte, taake StarRatingManager ko correctly pata rahe
//          * ke is level mein power-up use hua tha aur max 1 star rule apply ho.
//          */

//         if (infiniteBallsRoutine != null)
//         {
//             StopCoroutine(
//                 infiniteBallsRoutine
//             );

//             infiniteBallsRoutine = null;
//         }

//         if (powerCannonRoutine != null)
//         {
//             StopCoroutine(
//                 powerCannonRoutine
//             );

//             powerCannonRoutine = null;
//         }

//         infiniteBallsActive = false;
//         powerCannonActive = false;

//         infiniteBallsRemainingTime = 0f;
//         powerCannonRemainingTime = 0f;

//         if (cannonController != null)
//         {
//             cannonController.SetInfiniteBallsEnabled(
//                 false
//             );

//             cannonController
//                 .SetRuntimeBallSizeMultiplier(
//                     1f
//                 );

//             cannonController
//                 .SetRuntimeLaunchForceMultiplier(
//                     1f
//                 );
//         }

//         if (cannonVisualRoot != null &&
//             originalScaleCached)
//         {
//             cannonVisualRoot.localScale =
//                 originalCannonScale;
//         }

//         HideTimer();

//         RefreshButtons();
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         ResetPowerUpsForNewLevel();
//     }


//     public void ResetPowerUpsForNewLevel()
//     {
//         if (infiniteBallsRoutine != null)
//         {
//             StopCoroutine(
//                 infiniteBallsRoutine
//             );

//             infiniteBallsRoutine = null;
//         }

//         if (powerCannonRoutine != null)
//         {
//             StopCoroutine(
//                 powerCannonRoutine
//             );

//             powerCannonRoutine = null;
//         }

//         infiniteBallsUsedThisLevel = false;
//         powerCannonUsedThisLevel = false;

//         infiniteBallsActive = false;
//         powerCannonActive = false;

//         infiniteBallsRemainingTime = 0f;
//         powerCannonRemainingTime = 0f;

//         if (cannonController != null)
//         {
//             cannonController.ResetRuntimePowerUps();
//         }

//         if (cannonVisualRoot != null &&
//             originalScaleCached)
//         {
//             cannonVisualRoot.localScale =
//                 originalCannonScale;
//         }

//         HideTimer();

//         RefreshButtons();
//     }


//     private void HandleInfiniteBallsCountChanged(
//         int newCount)
//     {
//         RefreshButtons();
//     }


//     private void HandlePowerCannonCountChanged(
//         int newCount)
//     {
//         RefreshButtons();
//     }


//     private void RefreshButtons()
//     {
//         ResolveReferences();

//         bool levelReady =
//             levelRuntimeController == null ||
//             levelRuntimeController.IsLevelGenerated;

//         bool infiniteAvailable =
//             powerUpInventoryManager != null &&
//             powerUpInventoryManager.InfiniteBallsUnlocked &&
//             powerUpInventoryManager.InfiniteBallsCount > 0;

//         bool powerCannonAvailable =
//             powerUpInventoryManager != null &&
//             powerUpInventoryManager.PowerCannonUnlocked &&
//             powerUpInventoryManager.PowerCannonCount > 0;

//         if (infiniteBallsButton != null)
//         {
//             infiniteBallsButton.interactable =
//                 levelReady &&
//                 infiniteAvailable &&
//                 !infiniteBallsActive;
//         }

//         if (powerCannonButton != null)
//         {
//             powerCannonButton.interactable =
//                 levelReady &&
//                 powerCannonAvailable &&
//                 !powerCannonActive;
//         }
//     }
// }




using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameplayPowerUpType
{
    InfiniteBalls,
    PowerCannon
}


public sealed class GameplayPowerUpController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button infiniteBallsButton;
    [SerializeField] private Button powerCannonButton;


    [Header("Rewarded Ad Icons")]
    [Tooltip(
        "Infinite Balls stock x0 ho to ye ad icon show hoga."
    )]
    [SerializeField] private GameObject infiniteBallsRewardedAdIcon;

    [Tooltip(
        "Power Cannon stock x0 ho to ye ad icon show hoga."
    )]
    [SerializeField] private GameObject powerCannonRewardedAdIcon;


    [Header("Single Top Timer UI")]
    [Tooltip(
        "Screen ke top par single TMP Text. " +
        "Jo power-up active hoga usi ka timer yahan show hoga."
    )]
    [SerializeField] private TMP_Text powerUpTimerText;


    [Header("Infinite Balls")]
    [Tooltip("Infinite Balls kitne seconds active rahega.")]
    [SerializeField, Min(0.1f)]
    private float infiniteBallsDuration = 15f;


    [Header("Power Cannon")]
    [Tooltip("Power Cannon kitne seconds active rahega.")]
    [SerializeField, Min(0.1f)]
    private float powerCannonDuration = 15f;

    [SerializeField, Min(1f)]
    private float cannonSizeMultiplier = 1.25f;

    [SerializeField, Min(1f)]
    private float cannonBallSizeMultiplier = 1.5f;

    [Tooltip("Cannon ball ki launch speed/force ka multiplier.")]
    [SerializeField, Min(1f)]
    private float launchForceMultiplier = 1.5f;


    [Header("References")]
    [SerializeField] private CannonController cannonController;
    [SerializeField] private LevelRuntimeController levelRuntimeController;
    [SerializeField] private PowerUpInventoryManager powerUpInventoryManager;
    [SerializeField] private GoogleAdsManager googleAdsManager;

    [Tooltip(
        "Complete visible cannon root. Isi Transform ka size Power Cannon par increase hoga."
    )]
    [SerializeField] private Transform cannonVisualRoot;


    private Vector3 originalCannonScale;
    private bool originalScaleCached;

    private bool infiniteBallsUsedThisLevel;
    private bool powerCannonUsedThisLevel;

    private bool infiniteBallsActive;
    private bool powerCannonActive;

    private float infiniteBallsRemainingTime;
    private float powerCannonRemainingTime;

    private GameplayPowerUpType lastActivatedPowerUp;

    private Coroutine infiniteBallsRoutine;
    private Coroutine powerCannonRoutine;

    private bool rewardedAdInProgress;


    public bool InfiniteBallsUsedThisLevel =>
        infiniteBallsUsedThisLevel;

    public bool PowerCannonUsedThisLevel =>
        powerCannonUsedThisLevel;

    public bool AnyPowerUpUsedThisLevel =>
        infiniteBallsUsedThisLevel ||
        powerCannonUsedThisLevel;

    public bool InfiniteBallsActive =>
        infiniteBallsActive;

    public bool PowerCannonActive =>
        powerCannonActive;


    public event Action<GameplayPowerUpType> PowerUpActivated;


    private void Awake()
    {
        ResolveReferences();
        CacheOriginalCannonScale();
        HideTimer();
    }


    private void OnEnable()
    {
        ResolveReferences();
        CacheOriginalCannonScale();

        if (infiniteBallsButton != null)
        {
            infiniteBallsButton.onClick.RemoveListener(
                ActivateInfiniteBalls
            );

            infiniteBallsButton.onClick.AddListener(
                ActivateInfiniteBalls
            );
        }

        if (powerCannonButton != null)
        {
            powerCannonButton.onClick.RemoveListener(
                ActivatePowerCannon
            );

            powerCannonButton.onClick.AddListener(
                ActivatePowerCannon
            );
        }

        SubscribeToLevel();
        RefreshButtons();
        RefreshTopTimer();
    }


    private void OnDisable()
    {
        if (infiniteBallsButton != null)
        {
            infiniteBallsButton.onClick.RemoveListener(
                ActivateInfiniteBalls
            );
        }

        if (powerCannonButton != null)
        {
            powerCannonButton.onClick.RemoveListener(
                ActivatePowerCannon
            );
        }

        UnsubscribeFromLevel();
    }


    private void Update()
    {
        RefreshTopTimer();
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

        if (powerUpInventoryManager == null)
        {
            if (PowerUpInventoryManager.Instance != null)
            {
                powerUpInventoryManager =
                    PowerUpInventoryManager.Instance;
            }
            else
            {
                powerUpInventoryManager =
                    FindFirstObjectByType<PowerUpInventoryManager>(
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

        if (cannonVisualRoot == null &&
            cannonController != null)
        {
            cannonVisualRoot =
                cannonController.transform;
        }
    }


    private void CacheOriginalCannonScale()
    {
        if (originalScaleCached ||
            cannonVisualRoot == null)
        {
            return;
        }

        originalCannonScale =
            cannonVisualRoot.localScale;

        originalScaleCached = true;
    }


    private void SubscribeToLevel()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        levelRuntimeController.LevelGenerated -=
            HandleLevelGenerated;

        levelRuntimeController.LevelGenerated +=
            HandleLevelGenerated;

        levelRuntimeController.LevelCompleted -=
            HandleLevelCompleted;

        levelRuntimeController.LevelCompleted +=
            HandleLevelCompleted;

        if (powerUpInventoryManager != null)
        {
            powerUpInventoryManager.InfiniteBallsCountChanged -=
                HandleInfiniteBallsCountChanged;

            powerUpInventoryManager.InfiniteBallsCountChanged +=
                HandleInfiniteBallsCountChanged;

            powerUpInventoryManager.PowerCannonCountChanged -=
                HandlePowerCannonCountChanged;

            powerUpInventoryManager.PowerCannonCountChanged +=
                HandlePowerCannonCountChanged;
        }

        if (googleAdsManager != null)
        {
            googleAdsManager.RewardedLoaded -=
                HandleRewardedLoaded;

            googleAdsManager.RewardedLoaded +=
                HandleRewardedLoaded;
        }
    }


    private void UnsubscribeFromLevel()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        levelRuntimeController.LevelGenerated -=
            HandleLevelGenerated;

        levelRuntimeController.LevelCompleted -=
            HandleLevelCompleted;

        if (powerUpInventoryManager != null)
        {
            powerUpInventoryManager.InfiniteBallsCountChanged -=
                HandleInfiniteBallsCountChanged;

            powerUpInventoryManager.PowerCannonCountChanged -=
                HandlePowerCannonCountChanged;
        }

        if (googleAdsManager != null)
        {
            googleAdsManager.RewardedLoaded -=
                HandleRewardedLoaded;
        }
    }


    public void ActivateInfiniteBalls()
    {
        ResolveReferences();

        if (!CanUsePowerUp() ||
            infiniteBallsActive ||
            rewardedAdInProgress ||
            cannonController == null ||
            powerUpInventoryManager == null ||
            !powerUpInventoryManager.InfiniteBallsUnlocked)
        {
            return;
        }

        if (powerUpInventoryManager.InfiniteBallsCount > 0)
        {
            if (powerUpInventoryManager.TryConsumeInfiniteBalls(1))
            {
                StartInfiniteBallsEffect();
            }

            return;
        }

        ShowRewardedForInfiniteBalls();
    }


    private void StartInfiniteBallsEffect()
    {
        infiniteBallsUsedThisLevel = true;
        infiniteBallsActive = true;

        infiniteBallsRemainingTime =
            Mathf.Max(
                0.1f,
                infiniteBallsDuration
            );

        lastActivatedPowerUp =
            GameplayPowerUpType.InfiniteBalls;

        cannonController.SetInfiniteBallsEnabled(
            true
        );

        if (infiniteBallsRoutine != null)
        {
            StopCoroutine(
                infiniteBallsRoutine
            );
        }

        infiniteBallsRoutine =
            StartCoroutine(
                InfiniteBallsTimerRoutine()
            );

        PowerUpActivated?.Invoke(
            GameplayPowerUpType.InfiniteBalls
        );

        RefreshButtons();
        RefreshTopTimer();
    }


    private void ShowRewardedForInfiniteBalls()
    {
        if (googleAdsManager == null ||
            !googleAdsManager.IsRewardedReady)
        {
            RefreshButtons();
            return;
        }

        rewardedAdInProgress = true;
        RefreshButtons();

        bool shown =
            googleAdsManager.ShowRewarded(
                () =>
                {
                    if (powerUpInventoryManager != null)
                    {
                        /*
                         * Successful rewarded ad = sirf x1 earn.
                         * Power-up automatically activate NAHI hoga.
                         * User next button press par is x1 ko use karega.
                         */
                        powerUpInventoryManager.AddInfiniteBalls(1);
                    }
                },
                () =>
                {
                    rewardedAdInProgress = false;
                    RefreshButtons();
                }
            );

        if (!shown)
        {
            rewardedAdInProgress = false;
            RefreshButtons();
        }
    }


    public void ActivatePowerCannon()
    {
        ResolveReferences();
        CacheOriginalCannonScale();

        if (!CanUsePowerUp() ||
            powerCannonActive ||
            rewardedAdInProgress ||
            cannonController == null ||
            powerUpInventoryManager == null ||
            !powerUpInventoryManager.PowerCannonUnlocked)
        {
            return;
        }

        if (powerUpInventoryManager.PowerCannonCount > 0)
        {
            if (powerUpInventoryManager.TryConsumePowerCannon(1))
            {
                StartPowerCannonEffect();
            }

            return;
        }

        ShowRewardedForPowerCannon();
    }


    private void StartPowerCannonEffect()
    {
        CacheOriginalCannonScale();

        powerCannonUsedThisLevel = true;
        powerCannonActive = true;

        powerCannonRemainingTime =
            Mathf.Max(
                0.1f,
                powerCannonDuration
            );

        lastActivatedPowerUp =
            GameplayPowerUpType.PowerCannon;

        if (cannonVisualRoot != null &&
            originalScaleCached)
        {
            cannonVisualRoot.localScale =
                originalCannonScale *
                cannonSizeMultiplier;
        }

        cannonController
            .SetRuntimeBallSizeMultiplier(
                cannonBallSizeMultiplier
            );

        cannonController
            .SetRuntimeLaunchForceMultiplier(
                launchForceMultiplier
            );

        if (powerCannonRoutine != null)
        {
            StopCoroutine(
                powerCannonRoutine
            );
        }

        powerCannonRoutine =
            StartCoroutine(
                PowerCannonTimerRoutine()
            );

        PowerUpActivated?.Invoke(
            GameplayPowerUpType.PowerCannon
        );

        RefreshButtons();
        RefreshTopTimer();
    }


    private void ShowRewardedForPowerCannon()
    {
        if (googleAdsManager == null ||
            !googleAdsManager.IsRewardedReady)
        {
            RefreshButtons();
            return;
        }

        rewardedAdInProgress = true;
        RefreshButtons();

        bool shown =
            googleAdsManager.ShowRewarded(
                () =>
                {
                    if (powerUpInventoryManager != null)
                    {
                        /*
                         * Successful rewarded ad = sirf x1 earn.
                         * Power-up automatically activate NAHI hoga.
                         * User next button press par is x1 ko use karega.
                         */
                        powerUpInventoryManager.AddPowerCannon(1);
                    }
                },
                () =>
                {
                    rewardedAdInProgress = false;
                    RefreshButtons();
                }
            );

        if (!shown)
        {
            rewardedAdInProgress = false;
            RefreshButtons();
        }
    }


    private IEnumerator InfiniteBallsTimerRoutine()
    {
        while (infiniteBallsRemainingTime > 0f &&
               infiniteBallsActive)
        {
            yield return null;

            infiniteBallsRemainingTime -=
                Time.deltaTime;
        }

        infiniteBallsRemainingTime = 0f;

        EndInfiniteBalls();
    }


    private IEnumerator PowerCannonTimerRoutine()
    {
        while (powerCannonRemainingTime > 0f &&
               powerCannonActive)
        {
            yield return null;

            powerCannonRemainingTime -=
                Time.deltaTime;
        }

        powerCannonRemainingTime = 0f;

        EndPowerCannon();
    }


    private void EndInfiniteBalls()
    {
        infiniteBallsActive = false;
        infiniteBallsRemainingTime = 0f;

        if (cannonController != null)
        {
            cannonController.SetInfiniteBallsEnabled(
                false
            );
        }

        infiniteBallsRoutine = null;

        RefreshButtons();
        RefreshTopTimer();
    }


    private void EndPowerCannon()
    {
        powerCannonActive = false;
        powerCannonRemainingTime = 0f;

        if (cannonVisualRoot != null &&
            originalScaleCached)
        {
            cannonVisualRoot.localScale =
                originalCannonScale;
        }

        if (cannonController != null)
        {
            cannonController
                .SetRuntimeBallSizeMultiplier(
                    1f
                );

            cannonController
                .SetRuntimeLaunchForceMultiplier(
                    1f
                );
        }

        powerCannonRoutine = null;

        RefreshButtons();
        RefreshTopTimer();
    }


    private void RefreshTopTimer()
    {
        if (powerUpTimerText == null)
        {
            return;
        }

        bool showInfinite =
            infiniteBallsActive;

        bool showPowerCannon =
            powerCannonActive;

        if (!showInfinite &&
            !showPowerCannon)
        {
            HideTimer();
            return;
        }

        float timeToShow;

        if (showInfinite &&
            showPowerCannon)
        {
            timeToShow =
                lastActivatedPowerUp ==
                GameplayPowerUpType.InfiniteBalls
                    ? infiniteBallsRemainingTime
                    : powerCannonRemainingTime;
        }
        else if (showInfinite)
        {
            timeToShow =
                infiniteBallsRemainingTime;
        }
        else
        {
            timeToShow =
                powerCannonRemainingTime;
        }

        ShowTimer();
        UpdateTimerText(
            timeToShow
        );
    }


    private void UpdateTimerText(
        float remainingSeconds)
    {
        if (powerUpTimerText == null)
        {
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

        powerUpTimerText.text =
            $"{minutes:00}:{seconds:00}";
    }


    private void ShowTimer()
    {
        if (powerUpTimerText != null &&
            !powerUpTimerText.gameObject.activeSelf)
        {
            powerUpTimerText.gameObject.SetActive(
                true
            );
        }
    }


    private void HideTimer()
    {
        if (powerUpTimerText == null)
        {
            return;
        }

        powerUpTimerText.text = "";

        if (powerUpTimerText.gameObject.activeSelf)
        {
            powerUpTimerText.gameObject.SetActive(
                false
            );
        }
    }


    private bool CanUsePowerUp()
    {
        return levelRuntimeController == null ||
            levelRuntimeController.IsLevelGenerated;
    }


    private void HandleLevelCompleted(
        GridLevelData completedLevel)
    {
        StopActivePowerUpsForLevelComplete();
    }


    private void StopActivePowerUpsForLevelComplete()
    {
        /*
         * Level complete hote hi dono active effects aur timer stop karte hain.
         *
         * IMPORTANT:
         * infiniteBallsUsedThisLevel / powerCannonUsedThisLevel ko yahan
         * reset NAHI karte, taake StarRatingManager ko correctly pata rahe
         * ke is level mein power-up use hua tha aur max 1 star rule apply ho.
         */

        if (infiniteBallsRoutine != null)
        {
            StopCoroutine(
                infiniteBallsRoutine
            );

            infiniteBallsRoutine = null;
        }

        if (powerCannonRoutine != null)
        {
            StopCoroutine(
                powerCannonRoutine
            );

            powerCannonRoutine = null;
        }

        infiniteBallsActive = false;
        powerCannonActive = false;

        infiniteBallsRemainingTime = 0f;
        powerCannonRemainingTime = 0f;

        if (cannonController != null)
        {
            cannonController.SetInfiniteBallsEnabled(
                false
            );

            cannonController
                .SetRuntimeBallSizeMultiplier(
                    1f
                );

            cannonController
                .SetRuntimeLaunchForceMultiplier(
                    1f
                );
        }

        if (cannonVisualRoot != null &&
            originalScaleCached)
        {
            cannonVisualRoot.localScale =
                originalCannonScale;
        }

        HideTimer();

        RefreshButtons();
    }


    private void HandleLevelGenerated(
        GridLevelData generatedLevel)
    {
        ResetPowerUpsForNewLevel();
    }


    public void ResetPowerUpsForNewLevel()
    {
        if (infiniteBallsRoutine != null)
        {
            StopCoroutine(
                infiniteBallsRoutine
            );

            infiniteBallsRoutine = null;
        }

        if (powerCannonRoutine != null)
        {
            StopCoroutine(
                powerCannonRoutine
            );

            powerCannonRoutine = null;
        }

        infiniteBallsUsedThisLevel = false;
        powerCannonUsedThisLevel = false;

        infiniteBallsActive = false;
        powerCannonActive = false;

        infiniteBallsRemainingTime = 0f;
        powerCannonRemainingTime = 0f;

        rewardedAdInProgress = false;

        if (cannonController != null)
        {
            cannonController.ResetRuntimePowerUps();
        }

        if (cannonVisualRoot != null &&
            originalScaleCached)
        {
            cannonVisualRoot.localScale =
                originalCannonScale;
        }

        HideTimer();

        RefreshButtons();
    }


    private void HandleRewardedLoaded()
    {
        RefreshButtons();
    }


    private void HandleInfiniteBallsCountChanged(
        int newCount)
    {
        RefreshButtons();
    }


    private void HandlePowerCannonCountChanged(
        int newCount)
    {
        RefreshButtons();
    }


    private void RefreshButtons()
    {
        ResolveReferences();

        bool levelReady =
            levelRuntimeController == null ||
            levelRuntimeController.IsLevelGenerated;

        bool infiniteUnlocked =
            powerUpInventoryManager != null &&
            powerUpInventoryManager.InfiniteBallsUnlocked;

        bool powerCannonUnlocked =
            powerUpInventoryManager != null &&
            powerUpInventoryManager.PowerCannonUnlocked;

        int infiniteCount =
            powerUpInventoryManager != null
                ? powerUpInventoryManager.InfiniteBallsCount
                : 0;

        int powerCannonCount =
            powerUpInventoryManager != null
                ? powerUpInventoryManager.PowerCannonCount
                : 0;

        bool rewardedReady =
            googleAdsManager != null &&
            googleAdsManager.IsRewardedReady;

        bool infiniteNeedsRewardedAd =
            infiniteUnlocked &&
            infiniteCount <= 0;

        bool powerCannonNeedsRewardedAd =
            powerCannonUnlocked &&
            powerCannonCount <= 0;


        /*
         * Stock x0 = ad icon show.
         * Stock x1+ = ad icon hide.
         */
        if (infiniteBallsRewardedAdIcon != null)
        {
            infiniteBallsRewardedAdIcon.SetActive(
                infiniteNeedsRewardedAd &&
                !rewardedAdInProgress
            );
        }

        if (powerCannonRewardedAdIcon != null)
        {
            powerCannonRewardedAdIcon.SetActive(
                powerCannonNeedsRewardedAd &&
                !rewardedAdInProgress
            );
        }


        if (infiniteBallsButton != null)
        {
            infiniteBallsButton.interactable =
                levelReady &&
                infiniteUnlocked &&
                !infiniteBallsActive &&
                !rewardedAdInProgress &&
                (
                    infiniteCount > 0 ||
                    rewardedReady
                );
        }

        if (powerCannonButton != null)
        {
            powerCannonButton.interactable =
                levelReady &&
                powerCannonUnlocked &&
                !powerCannonActive &&
                !rewardedAdInProgress &&
                (
                    powerCannonCount > 0 ||
                    rewardedReady
                );
        }
    }

}













