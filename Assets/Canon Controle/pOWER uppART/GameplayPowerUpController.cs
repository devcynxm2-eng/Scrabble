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
using System.Collections.Generic;
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


    [Header("Simple Rocket")]
    [Tooltip("Rocket fire hote waqt cannon kitna peeche jayega.")]
    [SerializeField, Min(0f)]
    private float cannonRecoilDistance = 0.25f;

    [SerializeField, Min(0.01f)]
    private float cannonRecoilDuration = 0.12f;

    [Tooltip("Rocket ko impact point tak pohanchne ka time. Kam value = fast rocket.")]
    [SerializeField, Min(0.1f)]
    private float rocketFlightDuration = 0.55f;

    [SerializeField, Min(1f)]
    private float rocketMaxDistance = 30f;

    [SerializeField, Min(0.1f)]
    private float rocketExplosionRadius = 2.25f;

    [SerializeField, Min(0.1f)]
    private float rocketExplosionImpulse = 8f;


    [Header("References")]
    [SerializeField] private CannonController cannonController;
    [SerializeField] private LevelRuntimeController levelRuntimeController;
    [SerializeField] private PowerUpInventoryManager powerUpInventoryManager;
    [SerializeField] private GoogleAdsManager googleAdsManager;

    [Tooltip("Cannon ke left/right launcher placement ka anchor.")]
    [SerializeField] private Transform cannonVisualRoot;


    private RuntimeRocketBarrage rocketBarrage;

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
        HideTimer();
    }


    private void OnEnable()
    {
        ResolveReferences();

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

        if (rocketBarrage == null)
        {
            rocketBarrage =
                GetComponent<RuntimeRocketBarrage>();

            if (rocketBarrage == null)
            {
                rocketBarrage =
                    gameObject.AddComponent<
                        RuntimeRocketBarrage>();
            }
        }

        rocketBarrage.Configure(
            cannonRecoilDistance,
            cannonRecoilDuration,
            rocketFlightDuration,
            rocketMaxDistance,
            rocketExplosionRadius,
            rocketExplosionImpulse
        );
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
        ResolveReferences();

        powerCannonUsedThisLevel = true;
        powerCannonActive = true;

        powerCannonRemainingTime =
            rocketBarrage != null
                ? rocketBarrage.EstimatedDuration
                : 0.1f;

        lastActivatedPowerUp =
            GameplayPowerUpType.PowerCannon;

        cannonController
            .SetRuntimeBallSizeMultiplier(
                1f
            );

        cannonController
            .SetRuntimeLaunchForceMultiplier(
                1f
            );

        if (rocketBarrage != null)
        {
            rocketBarrage.PlayBarrage(
                cannonController.RocketLaunchPoint,
                cannonVisualRoot,
                cannonController.RocketShootDirection
            );
        }

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
        while (powerCannonActive &&
               rocketBarrage != null &&
               rocketBarrage.IsRunning)
        {
            yield return null;

            powerCannonRemainingTime -=
                Time.deltaTime;

            powerCannonRemainingTime = Mathf.Max(
                0f,
                powerCannonRemainingTime
            );
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

        if (rocketBarrage != null)
        {
            rocketBarrage.StopBarrage();
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

        if (rocketBarrage != null)
        {
            rocketBarrage.StopBarrage();
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

        if (rocketBarrage != null)
        {
            rocketBarrage.StopBarrage();
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


internal sealed class RuntimeRocketBarrage : MonoBehaviour
{
    // Legacy launcher helpers are kept below for scene compatibility,
    // but the simplified flow always fires one rocket from the cannon.
    private const int launcherCount = 1;
    private const float appearDuration = 0.1f;

    private sealed class LauncherSlot
    {
        public Transform Root;
        public Vector3 HiddenPosition;
        public Vector3 ShownPosition;
    }


    private float recoilDistance = 0.25f;
    private float recoilDuration = 0.12f;
    private float flightDuration = 0.55f;
    private float maxFlightDistance = 30f;
    private float explosionRadius = 2.25f;
    private float explosionImpulse = 8f;

    private Coroutine barrageRoutine;
    private readonly List<Coroutine> rocketRoutines =
        new List<Coroutine>();
    private readonly List<GameObject> spawnedObjects =
        new List<GameObject>();

    private int activeRocketCount;
    private Material launcherMaterial;
    private Material launcherAccentMaterial;
    private Material rocketMaterial;
    private Material rocketAccentMaterial;
    private Material trailMaterial;
    private Transform activeRecoilRoot;
    private Vector3 recoilRestPosition;
    private bool hasRecoilRestPosition;


    public bool IsRunning =>
        barrageRoutine != null;

    public float EstimatedDuration =>
        flightDuration +
        recoilDuration * 2f +
        0.1f;


    public void Configure(
        float requestedRecoilDistance,
        float requestedRecoilDuration,
        float requestedFlightDuration,
        float requestedMaxFlightDistance,
        float requestedExplosionRadius,
        float requestedExplosionImpulse)
    {
        recoilDistance = Mathf.Max(
            0f,
            requestedRecoilDistance
        );

        recoilDuration = Mathf.Max(
            0.01f,
            requestedRecoilDuration
        );

        flightDuration = Mathf.Max(
            0.1f,
            requestedFlightDuration
        );

        maxFlightDistance = Mathf.Max(
            1f,
            requestedMaxFlightDistance
        );

        explosionRadius = Mathf.Max(
            0.1f,
            requestedExplosionRadius
        );

        explosionImpulse = Mathf.Max(
            0.1f,
            requestedExplosionImpulse
        );
    }


    public void PlayBarrage(
        Transform launchPoint,
        Transform recoilRoot,
        Vector3 launchDirection)
    {
        StopBarrage();

        if (launchPoint == null)
        {
            return;
        }

        EnsureMaterials();

        activeRecoilRoot = recoilRoot;

        if (activeRecoilRoot != null)
        {
            recoilRestPosition =
                activeRecoilRoot.position;
            hasRecoilRestPosition = true;
        }

        barrageRoutine = StartCoroutine(
            BarrageRoutine(
                launchPoint,
                recoilRoot,
                launchDirection
            )
        );
    }


    public void StopBarrage()
    {
        if (barrageRoutine != null)
        {
            StopCoroutine(barrageRoutine);
            barrageRoutine = null;
        }

        for (int i = 0; i < rocketRoutines.Count; i++)
        {
            Coroutine routine = rocketRoutines[i];

            if (routine != null)
            {
                StopCoroutine(routine);
            }
        }

        rocketRoutines.Clear();
        activeRocketCount = 0;

        RestoreCannonPosition();
        ClearSpawnedObjects();
    }


    private IEnumerator BarrageRoutine(
        Transform launchPoint,
        Transform recoilRoot,
        Vector3 launchDirection)
    {
        GameObject barrageRoot =
            new GameObject("Runtime Simple Rocket");

        spawnedObjects.Add(barrageRoot);

        if (launchDirection.sqrMagnitude < 0.0001f)
        {
            ClearSpawnedObjects();
            RestoreCannonPosition();
            barrageRoutine = null;
            yield break;
        }

        launchDirection.Normalize();

        yield return AnimateCannonRecoil(
            recoilRoot,
            launchPoint,
            true
        );

        Vector3 launchPosition =
            launchPoint.position;

        FindDirectionalImpact(
            launchPosition,
            launchDirection,
            recoilRoot,
            out Vector3 impactPoint,
            out PhysicsTowerObject target
        );

        activeRocketCount = 1;

        Coroutine rocketRoutine = StartCoroutine(
            FlyRocket(
                barrageRoot.transform,
                launchPosition,
                impactPoint,
                target
            )
        );

        rocketRoutines.Add(rocketRoutine);

        yield return AnimateCannonRecoil(
            recoilRoot,
            launchPoint,
            false
        );

        while (activeRocketCount > 0)
        {
            yield return null;
        }

        rocketRoutines.Clear();

        ClearSpawnedObjects();
        RestoreCannonPosition();
        barrageRoutine = null;
    }


    private IEnumerator AnimateCannonRecoil(
        Transform recoilRoot,
        Transform launchPoint,
        bool movingBack)
    {
        if (recoilRoot == null ||
            launchPoint == null ||
            recoilDistance <= 0f)
        {
            yield break;
        }

        Vector3 restPosition = recoilRestPosition;
        Vector3 backPosition =
            restPosition -
            launchPoint.forward * recoilDistance;

        Vector3 startPosition = movingBack
            ? restPosition
            : recoilRoot.position;

        Vector3 endPosition = movingBack
            ? backPosition
            : restPosition;

        float elapsed = 0f;

        while (elapsed < recoilDuration &&
               recoilRoot != null)
        {
            float progress = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(
                    elapsed / recoilDuration
                )
            );

            recoilRoot.position = Vector3.Lerp(
                startPosition,
                endPosition,
                progress
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (recoilRoot != null)
        {
            recoilRoot.position = endPosition;
        }
    }


    private void RestoreCannonPosition()
    {
        if (activeRecoilRoot != null &&
            hasRecoilRestPosition)
        {
            activeRecoilRoot.position =
                recoilRestPosition;
        }

        activeRecoilRoot = null;
        hasRecoilRestPosition = false;
    }


    private List<LauncherSlot> CreateLaunchers(
        Transform parent,
        Transform anchor)
    {
        List<LauncherSlot> result =
            new List<LauncherSlot>();

        for (int i = 0; i < launcherCount; i++)
        {
            bool leftSide =
                i < Mathf.CeilToInt(launcherCount * 0.5f);

            int sideIndex = leftSide
                ? i
                : i - Mathf.CeilToInt(launcherCount * 0.5f);

            float side = leftSide ? -1f : 1f;
            float distance = 1.15f + sideIndex * 0.85f;

            Vector3 shownPosition =
                anchor.position +
                Vector3.right * side * distance +
                Vector3.up * 0.2f +
                Vector3.forward * (0.15f + sideIndex * 0.18f);

            Vector3 hiddenPosition =
                shownPosition + Vector3.down * 1.45f;

            GameObject launcherObject =
                new GameObject(
                    leftSide
                        ? "Left Rocket Launcher"
                        : "Right Rocket Launcher"
                );

            launcherObject.transform.SetParent(
                parent,
                true
            );

            launcherObject.transform.position =
                hiddenPosition;
            launcherObject.transform.localScale =
                Vector3.one * 0.65f;

            CreatePrimitivePart(
                PrimitiveType.Cylinder,
                launcherObject.transform,
                "Launcher Base",
                Vector3.zero,
                new Vector3(0.3f, 0.16f, 0.3f),
                Quaternion.identity,
                launcherAccentMaterial
            );

            CreatePrimitivePart(
                PrimitiveType.Cylinder,
                launcherObject.transform,
                "Launcher Tube",
                Vector3.up * 0.52f,
                new Vector3(0.15f, 0.55f, 0.15f),
                Quaternion.identity,
                launcherMaterial
            );

            CreatePrimitivePart(
                PrimitiveType.Cylinder,
                launcherObject.transform,
                "Launcher Ring",
                Vector3.up * 1.02f,
                new Vector3(0.21f, 0.08f, 0.21f),
                Quaternion.identity,
                launcherAccentMaterial
            );

            result.Add(
                new LauncherSlot
                {
                    Root = launcherObject.transform,
                    HiddenPosition = hiddenPosition,
                    ShownPosition = shownPosition
                }
            );
        }

        return result;
    }


    private IEnumerator AnimateLaunchers(
        List<LauncherSlot> launchers,
        bool appearing)
    {
        float elapsed = 0f;

        while (elapsed < appearDuration)
        {
            float normalized = Mathf.Clamp01(
                elapsed / appearDuration
            );

            float progress = appearing
                ? 1f - Mathf.Pow(1f - normalized, 3f)
                : normalized * normalized * normalized;

            for (int i = 0; i < launchers.Count; i++)
            {
                LauncherSlot launcher = launchers[i];

                if (launcher.Root == null)
                {
                    continue;
                }

                launcher.Root.position = Vector3.Lerp(
                    appearing
                        ? launcher.HiddenPosition
                        : launcher.ShownPosition,
                    appearing
                        ? launcher.ShownPosition
                        : launcher.HiddenPosition,
                    progress
                );

                launcher.Root.localScale = Vector3.Lerp(
                    appearing
                        ? Vector3.one * 0.65f
                        : Vector3.one,
                    appearing
                        ? Vector3.one
                        : Vector3.one * 0.65f,
                    progress
                );
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < launchers.Count; i++)
        {
            LauncherSlot launcher = launchers[i];

            if (launcher.Root == null)
            {
                continue;
            }

            launcher.Root.position = appearing
                ? launcher.ShownPosition
                : launcher.HiddenPosition;

            launcher.Root.localScale = appearing
                ? Vector3.one
                : Vector3.one * 0.65f;
        }
    }


    private static IEnumerator AimLauncher(
        Transform launcher,
        Vector3 targetPoint,
        float duration)
    {
        if (launcher == null)
        {
            yield break;
        }

        Vector3 direction =
            targetPoint - launcher.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            yield break;
        }

        Quaternion startRotation = launcher.rotation;
        Quaternion targetRotation =
            Quaternion.FromToRotation(
                Vector3.up,
                direction.normalized
            );

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            launcher.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                progress
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        launcher.rotation = targetRotation;
    }


    private void FindDirectionalImpact(
        Vector3 origin,
        Vector3 direction,
        Transform ignoredRoot,
        out Vector3 impactPoint,
        out PhysicsTowerObject target)
    {
        direction.Normalize();
        impactPoint =
            origin + direction * maxFlightDistance;
        target = null;

        Physics.SyncTransforms();

        RaycastHit[] hits = Physics.RaycastAll(
            origin + direction * 0.05f,
            direction,
            maxFlightDistance,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.collider == null ||
                hit.distance >= closestDistance)
            {
                continue;
            }

            Transform hitTransform =
                hit.collider.transform;

            if (ignoredRoot != null &&
                (hitTransform == ignoredRoot ||
                 hitTransform.IsChildOf(ignoredRoot)))
            {
                continue;
            }

            closestDistance = hit.distance;
            impactPoint = hit.point;
            target = hit.collider.GetComponentInParent<
                PhysicsTowerObject>();
        }
    }


    private IEnumerator FlyRocket(
        Transform parent,
        Vector3 startPosition,
        Vector3 impactPoint,
        PhysicsTowerObject target)
    {
        GameObject rocket = CreateRocket(
            parent,
            startPosition
        );

        spawnedObjects.Add(rocket);

        Vector3 previousPosition = startPosition;
        float elapsed = 0f;

        while (elapsed < flightDuration &&
               rocket != null)
        {
            float progress = Mathf.Clamp01(
                elapsed / flightDuration
            );

            float easedProgress = Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    impactPoint,
                    easedProgress
                );

            rocket.transform.position = position;

            Vector3 movement =
                position - previousPosition;

            if (movement.sqrMagnitude > 0.00001f)
            {
                rocket.transform.rotation =
                    Quaternion.LookRotation(
                        movement.normalized,
                        Vector3.up
                    );
            }

            previousPosition = position;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rocket != null)
        {
            rocket.transform.position = impactPoint;
        }

        CreateExplosion(impactPoint);
        ApplyExplosion(
            impactPoint,
            target
        );

        spawnedObjects.Remove(rocket);

        if (rocket != null)
        {
            Destroy(rocket);
        }

        activeRocketCount = Mathf.Max(
            0,
            activeRocketCount - 1
        );
    }


    private GameObject CreateRocket(
        Transform parent,
        Vector3 position)
    {
        GameObject rocket =
            new GameObject("Runtime Homing Rocket");

        rocket.transform.SetParent(parent, true);
        rocket.transform.position = position;

        CreatePrimitivePart(
            PrimitiveType.Cube,
            rocket.transform,
            "Red Square Rocket",
            Vector3.zero,
            Vector3.one * 0.32f,
            Quaternion.identity,
            rocketMaterial
        );

        return rocket;
    }


    private void ApplyExplosion(
        Vector3 impactPoint,
        PhysicsTowerObject primaryTarget)
    {
        HashSet<PhysicsTowerObject> affected =
            new HashSet<PhysicsTowerObject>();

        if (primaryTarget != null &&
            !primaryTarget.IsCleared)
        {
            affected.Add(primaryTarget);

            ApplyExplosionToTower(
                primaryTarget,
                impactPoint,
                true
            );
        }

        Collider[] overlaps = Physics.OverlapSphere(
            impactPoint,
            explosionRadius,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider overlap = overlaps[i];

            if (overlap == null)
            {
                continue;
            }

            PhysicsTowerObject tower =
                overlap.GetComponentInParent<
                    PhysicsTowerObject>();

            if (tower == null ||
                tower.IsCleared ||
                !affected.Add(tower))
            {
                continue;
            }

            ApplyExplosionToTower(
                tower,
                impactPoint,
                false
            );
        }
    }


    private void ApplyExplosionToTower(
        PhysicsTowerObject tower,
        Vector3 impactPoint,
        bool directHit)
    {
        if (tower == null || tower.IsCleared)
        {
            return;
        }

        Rigidbody body = tower.Body;
        Vector3 center = body != null
            ? body.worldCenterOfMass
            : tower.transform.position;

        Vector3 offset = center - impactPoint;
        float distance = offset.magnitude;

        Vector3 direction = distance > 0.001f
            ? offset / distance
            : Vector3.up;

        float falloff = 1f - Mathf.Clamp01(
            distance / explosionRadius
        );

        float force = Mathf.Lerp(
            explosionImpulse * 0.35f,
            explosionImpulse,
            falloff
        );

        Vector3 impulse =
            (direction + Vector3.up * 0.3f).normalized *
            force;

        Vector3 torque =
            UnityEngine.Random.onUnitSphere *
            force * 0.22f;

        tower.ApplyExternalImpact(
            impulse,
            torque,
            directHit,
            impactPoint
        );
    }


    private void CreateExplosion(Vector3 position)
    {
        GameObject explosion =
            new GameObject("Runtime Rocket Explosion");

        explosion.transform.position = position;

        ParticleSystem particles =
            explosion.AddComponent<ParticleSystem>();

        particles.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        ParticleSystem.MainModule main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.45f;
        main.startLifetime =
            new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        main.startSpeed =
            new ParticleSystem.MinMaxCurve(3f, 7f);
        main.startSize =
            new ParticleSystem.MinMaxCurve(0.12f, 0.45f);
        main.startColor =
            new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.9f, 0.2f, 1f),
                new Color(1f, 0.18f, 0.02f, 1f)
            );
        main.maxParticles = 48;
        main.simulationSpace =
            ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission =
            particles.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType =
            ParticleSystemShapeType.Sphere;
        shape.radius = 0.16f;

        ParticleSystemRenderer particleRenderer =
            explosion.GetComponent<
                ParticleSystemRenderer>();

        if (trailMaterial != null)
        {
            particleRenderer.sharedMaterial =
                trailMaterial;
        }

        particles.Emit(38);
        Destroy(explosion, 1.5f);
    }


    private static List<PhysicsTowerObject> CollectTargets()
    {
        PhysicsTowerObject[] found =
            FindObjectsByType<PhysicsTowerObject>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        List<PhysicsTowerObject> targets =
            new List<PhysicsTowerObject>();

        for (int i = 0; i < found.Length; i++)
        {
            PhysicsTowerObject target = found[i];

            if (target == null ||
                target.IsCleared ||
                !target.CountsAsTarget)
            {
                continue;
            }

            targets.Add(target);
        }

        targets.Sort(
            (first, second) =>
                second.transform.position.y.CompareTo(
                    first.transform.position.y
                )
        );

        if (targets.Count > 4)
        {
            int topTargetCount = Mathf.Max(
                4,
                Mathf.CeilToInt(targets.Count * 0.6f)
            );

            targets.RemoveRange(
                topTargetCount,
                targets.Count - topTargetCount
            );
        }

        for (int i = 0; i < targets.Count; i++)
        {
            int swapIndex = UnityEngine.Random.Range(
                i,
                targets.Count
            );

            PhysicsTowerObject temporary = targets[i];
            targets[i] = targets[swapIndex];
            targets[swapIndex] = temporary;
        }

        return targets;
    }


    private static Vector3 GetTargetPoint(
        PhysicsTowerObject target)
    {
        if (target == null)
        {
            return Vector3.zero;
        }

        if (target.TryGetPhysicsBounds(out Bounds bounds))
        {
            return bounds.center;
        }

        return target.transform.position;
    }


    private GameObject CreatePrimitivePart(
        PrimitiveType primitiveType,
        Transform parent,
        string objectName,
        Vector3 localPosition,
        Vector3 localScale,
        Quaternion localRotation,
        Material material)
    {
        GameObject part =
            GameObject.CreatePrimitive(primitiveType);

        part.name = objectName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = localScale;

        Collider partCollider =
            part.GetComponent<Collider>();

        if (partCollider != null)
        {
            partCollider.enabled = false;
            Destroy(partCollider);
        }

        Renderer partRenderer =
            part.GetComponent<Renderer>();

        if (partRenderer != null && material != null)
        {
            partRenderer.sharedMaterial = material;
        }

        return part;
    }


    private void EnsureMaterials()
    {
        if (rocketMaterial != null)
        {
            return;
        }

        launcherMaterial = CreateMaterial(
            new Color(0.12f, 0.18f, 0.26f, 1f),
            false
        );

        launcherAccentMaterial = CreateMaterial(
            new Color(1f, 0.35f, 0.04f, 1f),
            false
        );

        rocketMaterial = CreateMaterial(
            new Color(0.9f, 0.03f, 0.03f, 1f),
            false
        );

        rocketAccentMaterial = CreateMaterial(
            new Color(0.85f, 0.08f, 0.05f, 1f),
            false
        );

        trailMaterial = CreateMaterial(
            Color.white,
            true
        );
    }


    private static Material CreateMaterial(
        Color color,
        bool unlit)
    {
        Shader shader = null;

        if (unlit)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            shader = Shader.Find(
                "Universal Render Pipeline/Lit"
            );
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            return null;
        }

        Material material = new Material(shader);
        material.color = color;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        return material;
    }


    private void ClearSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1;
             i >= 0;
             i--)
        {
            GameObject spawned = spawnedObjects[i];

            if (spawned != null)
            {
                Destroy(spawned);
            }
        }

        spawnedObjects.Clear();
    }


    private void OnDestroy()
    {
        StopBarrage();
        DestroyMaterial(launcherMaterial);
        DestroyMaterial(launcherAccentMaterial);
        DestroyMaterial(rocketMaterial);
        DestroyMaterial(rocketAccentMaterial);
        DestroyMaterial(trailMaterial);
    }


    private static void DestroyMaterial(
        Material material)
    {
        if (material != null)
        {
            Destroy(material);
        }
    }
}



