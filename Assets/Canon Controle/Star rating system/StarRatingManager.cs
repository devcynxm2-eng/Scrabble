// using System;
// using System.Collections;
// using UnityEngine;

// public sealed class StarRatingManager : MonoBehaviour
// {
//     public static StarRatingManager Instance { get; private set; }


//     [Header("References")]
//     [SerializeField] private LevelRuntimeController levelRuntimeController;
//     [SerializeField] private CannonController cannonController;


//     [Header("Star Rules")]
//     [Tooltip(
//         "3 Stars ke liye original balls ka minimum remaining percentage. " +
//         "Default 0.5 = 50%."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float threeStarRemainingFraction = 0.5f;

//     [Tooltip(
//         "Agar Out Of Moves ke baad extra shots liye gaye hon to maximum " +
//         "kitne stars mil sakte hain."
//     )]
//     [SerializeField, Range(1, 3)]
//     private int maxStarsAfterExtraShots = 1;


//     private int originalBallCount;
//     private int extraShotsAddedThisLevel;
//     private int lastAwardedStars;

//     private Coroutine captureOriginalBallsRoutine;


//     public int OriginalBallCount =>
//         originalBallCount;

//     public int ExtraShotsAddedThisLevel =>
//         extraShotsAddedThisLevel;

//     public bool UsedExtraShotsThisLevel =>
//         extraShotsAddedThisLevel > 0;

//     public int LastAwardedStars =>
//         lastAwardedStars;


//     public event Action<int> StarsAwarded;


//     private void Awake()
//     {
//         if (Instance != null &&
//             Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         Instance = this;
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();
//         Subscribe();
//     }


//     private void OnDisable()
//     {
//         Unsubscribe();

//         if (captureOriginalBallsRoutine != null)
//         {
//             StopCoroutine(
//                 captureOriginalBallsRoutine
//             );

//             captureOriginalBallsRoutine = null;
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

//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     private void Subscribe()
//     {
//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.LevelGenerated -=
//                 HandleLevelGenerated;

//             levelRuntimeController.LevelGenerated +=
//                 HandleLevelGenerated;

//             levelRuntimeController.LevelCompleted -=
//                 HandleLevelCompleted;

//             levelRuntimeController.LevelCompleted +=
//                 HandleLevelCompleted;
//         }

//         if (cannonController != null)
//         {
//             cannonController.ExtraShotsAdded -=
//                 HandleExtraShotsAdded;

//             cannonController.ExtraShotsAdded +=
//                 HandleExtraShotsAdded;
//         }
//     }


//     private void Unsubscribe()
//     {
//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.LevelGenerated -=
//                 HandleLevelGenerated;

//             levelRuntimeController.LevelCompleted -=
//                 HandleLevelCompleted;
//         }

//         if (cannonController != null)
//         {
//             cannonController.ExtraShotsAdded -=
//                 HandleExtraShotsAdded;
//         }
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         originalBallCount = 0;
//         extraShotsAddedThisLevel = 0;
//         lastAwardedStars = 0;

//         if (captureOriginalBallsRoutine != null)
//         {
//             StopCoroutine(
//                 captureOriginalBallsRoutine
//             );
//         }

//         /*
//          * CannonController bhi LevelGenerated event par apna ball limit reset
//          * karta hai. Subscription order par depend na karne ke liye ek frame
//          * wait karke original TotalBalls snapshot lete hain.
//          */
//         captureOriginalBallsRoutine =
//             StartCoroutine(
//                 CaptureOriginalBallsNextFrame()
//             );
//     }


//     private IEnumerator CaptureOriginalBallsNextFrame()
//     {
//         yield return null;

//         captureOriginalBallsRoutine = null;

//         if (cannonController == null)
//         {
//             ResolveReferences();
//         }

//         if (cannonController != null)
//         {
//             originalBallCount =
//                 Mathf.Max(
//                     0,
//                     cannonController.TotalBalls
//                 );
//         }
//     }


//     private void HandleExtraShotsAdded(
//         int amount)
//     {
//         if (amount <= 0)
//         {
//             return;
//         }

//         extraShotsAddedThisLevel +=
//             amount;
//     }


//     private void HandleLevelCompleted(
//         GridLevelData completedLevel)
//     {
//         lastAwardedStars =
//             CalculateStars();

//         StarsAwarded?.Invoke(
//             lastAwardedStars
//         );
//     }


//     public int CalculateStars()
//     {
//         if (cannonController == null)
//         {
//             ResolveReferences();
//         }

//         if (cannonController == null)
//         {
//             return 1;
//         }

//         /*
//          * Extra shots use huay:
//          * Level complete ho sakta hai, lekin rating maximum configured value
//          * (default 1 star) tak capped rahegi.
//          */
//         if (UsedExtraShotsThisLevel)
//         {
//             return Mathf.Clamp(
//                 maxStarsAfterExtraShots,
//                 1,
//                 3
//             );
//         }

//         int totalOriginalBalls =
//             originalBallCount;

//         /*
//          * Safety fallback:
//          * Agar snapshot kisi unusual execution order ki wajah se 0 raha,
//          * current TotalBalls se calculate kar lete hain.
//          */
//         if (totalOriginalBalls <= 0)
//         {
//             totalOriginalBalls =
//                 Mathf.Max(
//                     1,
//                     cannonController.TotalBalls -
//                     extraShotsAddedThisLevel
//                 );
//         }

//         int remainingBalls =
//             Mathf.Max(
//                 0,
//                 cannonController.RemainingBalls
//             );

//         int threeStarMinimumRemaining =
//             Mathf.CeilToInt(
//                 totalOriginalBalls *
//                 threeStarRemainingFraction
//             );

//         threeStarMinimumRemaining =
//             Mathf.Clamp(
//                 threeStarMinimumRemaining,
//                 1,
//                 totalOriginalBalls
//             );

//         if (remainingBalls >=
//             threeStarMinimumRemaining)
//         {
//             return 3;
//         }

//         if (remainingBalls >= 1)
//         {
//             return 2;
//         }

//         return 1;
//     }
// }










using System;
using System.Collections;
using UnityEngine;

public sealed class StarRatingManager : MonoBehaviour
{
    public static StarRatingManager Instance { get; private set; }


    [Header("References")]
    [SerializeField] private LevelRuntimeController levelRuntimeController;
    [SerializeField] private CannonController cannonController;
    [SerializeField] private GameplayPowerUpController gameplayPowerUpController;


    [Header("Star Rules")]
    [Tooltip(
        "3 Stars ke liye original balls ka minimum remaining percentage. " +
        "Default 0.5 = 50%."
    )]
    [SerializeField, Range(0f, 1f)]
    private float threeStarRemainingFraction = 0.5f;

    [Tooltip(
        "Agar Out Of Moves ke baad extra shots liye gaye hon to maximum " +
        "kitne stars mil sakte hain."
    )]
    [SerializeField, Range(1, 3)]
    private int maxStarsAfterExtraShots = 1;


    private int originalBallCount;
    private int extraShotsAddedThisLevel;
    private bool powerUpUsedThisLevel;
    private int lastAwardedStars;

    private Coroutine captureOriginalBallsRoutine;


    public int OriginalBallCount =>
        originalBallCount;

    public int ExtraShotsAddedThisLevel =>
        extraShotsAddedThisLevel;

    public bool UsedExtraShotsThisLevel =>
        extraShotsAddedThisLevel > 0;

    public bool PowerUpUsedThisLevel =>
        powerUpUsedThisLevel;

    public int LastAwardedStars =>
        lastAwardedStars;


    public event Action<int> StarsAwarded;


    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }


    private void OnDisable()
    {
        Unsubscribe();

        if (captureOriginalBallsRoutine != null)
        {
            StopCoroutine(
                captureOriginalBallsRoutine
            );

            captureOriginalBallsRoutine = null;
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

        if (cannonController == null)
        {
            cannonController =
                FindFirstObjectByType<CannonController>(
                    FindObjectsInactive.Include
                );
        }

        if (gameplayPowerUpController == null)
        {
            gameplayPowerUpController =
                FindFirstObjectByType<GameplayPowerUpController>(
                    FindObjectsInactive.Include
                );
        }
    }


    private void Subscribe()
    {
        if (levelRuntimeController != null)
        {
            levelRuntimeController.LevelGenerated -=
                HandleLevelGenerated;

            levelRuntimeController.LevelGenerated +=
                HandleLevelGenerated;

            levelRuntimeController.LevelCompleted -=
                HandleLevelCompleted;

            levelRuntimeController.LevelCompleted +=
                HandleLevelCompleted;
        }

        if (cannonController != null)
        {
            cannonController.ExtraShotsAdded -=
                HandleExtraShotsAdded;

            cannonController.ExtraShotsAdded +=
                HandleExtraShotsAdded;
        }

        if (gameplayPowerUpController != null)
        {
            gameplayPowerUpController.PowerUpActivated -=
                HandlePowerUpActivated;

            gameplayPowerUpController.PowerUpActivated +=
                HandlePowerUpActivated;
        }
    }


    private void Unsubscribe()
    {
        if (levelRuntimeController != null)
        {
            levelRuntimeController.LevelGenerated -=
                HandleLevelGenerated;

            levelRuntimeController.LevelCompleted -=
                HandleLevelCompleted;
        }

        if (cannonController != null)
        {
            cannonController.ExtraShotsAdded -=
                HandleExtraShotsAdded;
        }

        if (gameplayPowerUpController != null)
        {
            gameplayPowerUpController.PowerUpActivated -=
                HandlePowerUpActivated;
        }
    }


    private void HandleLevelGenerated(
        GridLevelData generatedLevel)
    {
        originalBallCount = 0;
        extraShotsAddedThisLevel = 0;
        powerUpUsedThisLevel = false;
        lastAwardedStars = 0;

        if (captureOriginalBallsRoutine != null)
        {
            StopCoroutine(
                captureOriginalBallsRoutine
            );
        }

        /*
         * CannonController bhi LevelGenerated event par apna ball limit reset
         * karta hai. Subscription order par depend na karne ke liye ek frame
         * wait karke original TotalBalls snapshot lete hain.
         */
        captureOriginalBallsRoutine =
            StartCoroutine(
                CaptureOriginalBallsNextFrame()
            );
    }


    private IEnumerator CaptureOriginalBallsNextFrame()
    {
        yield return null;

        captureOriginalBallsRoutine = null;

        if (cannonController == null)
        {
            ResolveReferences();
        }

        if (cannonController != null)
        {
            originalBallCount =
                Mathf.Max(
                    0,
                    cannonController.TotalBalls
                );
        }
    }


    private void HandleExtraShotsAdded(
        int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        extraShotsAddedThisLevel +=
            amount;
    }


    private void HandlePowerUpActivated(
        GameplayPowerUpType powerUpType)
    {
        powerUpUsedThisLevel = true;
    }


    private void HandleLevelCompleted(
        GridLevelData completedLevel)
    {
        lastAwardedStars =
            CalculateStars();

        StarsAwarded?.Invoke(
            lastAwardedStars
        );
    }


    public int CalculateStars()
    {
        if (cannonController == null)
        {
            ResolveReferences();
        }

        if (cannonController == null)
        {
            return 1;
        }

        /*
         * Extra shots use huay:
         * Level complete ho sakta hai, lekin rating maximum configured value
         * (default 1 star) tak capped rahegi.
         */
        if (UsedExtraShotsThisLevel)
        {
            return Mathf.Clamp(
                maxStarsAfterExtraShots,
                1,
                3
            );
        }

        int totalOriginalBalls =
            originalBallCount;

        /*
         * Safety fallback:
         * Agar snapshot kisi unusual execution order ki wajah se 0 raha,
         * current TotalBalls se calculate kar lete hain.
         */
        if (totalOriginalBalls <= 0)
        {
            totalOriginalBalls =
                Mathf.Max(
                    1,
                    cannonController.TotalBalls -
                    extraShotsAddedThisLevel
                );
        }

        int remainingBalls =
            Mathf.Max(
                0,
                cannonController.RemainingBalls
            );

        int threeStarMinimumRemaining =
            Mathf.CeilToInt(
                totalOriginalBalls *
                threeStarRemainingFraction
            );

        threeStarMinimumRemaining =
            Mathf.Clamp(
                threeStarMinimumRemaining,
                1,
                totalOriginalBalls
            );

        if (remainingBalls >=
            threeStarMinimumRemaining)
        {
            return 3;
        }

        if (remainingBalls >= 1)
        {
            return 2;
        }

        return 1;
    }
}















