// using System;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public sealed class ScoreManager : MonoBehaviour
// {
//     private const string ScoreKey =
//         "RoyalSmash.TotalScore";


//     public static ScoreManager Instance { get; private set; }


//     [Header("Score")]
//     [SerializeField, Min(0)]
//     private int pointsPerLevel = 100;

//     [SerializeField]
//     private bool saveScore = true;


//     [Header("References")]
//     [SerializeField]
//     private LevelRuntimeController levelRuntimeController;


//     [Header("Behaviour")]
//     [SerializeField]
//     private bool dontDestroyOnLoad = true;


//     private int currentScore;


//     public int CurrentScore =>
//         currentScore;

//     public int PointsPerLevel =>
//         pointsPerLevel;


//     public event Action<int> ScoreChanged;

//     public event Action<int, int> ScoreAdded;


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
//             DontDestroyOnLoad(gameObject);
//         }

//         LoadScore();
//     }


//     private void OnEnable()
//     {
//         SceneManager.sceneLoaded +=
//             HandleSceneLoaded;

//         ResolveLevelRuntimeController();
//         SubscribeToLevelEvents();
//     }


//     private void OnDisable()
//     {
//         SceneManager.sceneLoaded -=
//             HandleSceneLoaded;

//         UnsubscribeFromLevelEvents();
//     }


//     private void HandleSceneLoaded(
//         Scene scene,
//         LoadSceneMode mode)
//     {
//         UnsubscribeFromLevelEvents();

//         levelRuntimeController = null;

//         ResolveLevelRuntimeController();
//         SubscribeToLevelEvents();

//         ScoreChanged?.Invoke(
//             currentScore
//         );
//     }


//     private void ResolveLevelRuntimeController()
//     {
//         if (levelRuntimeController != null)
//         {
//             return;
//         }

//         levelRuntimeController =
//             FindFirstObjectByType<LevelRuntimeController>(
//                 FindObjectsInactive.Include
//             );
//     }


//     private void SubscribeToLevelEvents()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelCompleted -=
//             HandleLevelCompleted;

//         levelRuntimeController.LevelCompleted +=
//             HandleLevelCompleted;
//     }


//     private void UnsubscribeFromLevelEvents()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         levelRuntimeController.LevelCompleted -=
//             HandleLevelCompleted;
//     }


//     private void HandleLevelCompleted(
//         GridLevelData completedLevel)
//     {
//         AddScore(
//             pointsPerLevel
//         );
//     }


//     public void AddScore(
//         int amount)
//     {
//         if (amount <= 0)
//         {
//             return;
//         }

//         currentScore += amount;

//         SaveScore();

//         ScoreAdded?.Invoke(
//             amount,
//             currentScore
//         );

//         ScoreChanged?.Invoke(
//             currentScore
//         );
//     }


//     public void SetScore(
//         int value)
//     {
//         currentScore =
//             Mathf.Max(
//                 0,
//                 value
//             );

//         SaveScore();

//         ScoreChanged?.Invoke(
//             currentScore
//         );
//     }


//     public void ResetScore()
//     {
//         currentScore = 0;

//         if (saveScore)
//         {
//             PlayerPrefs.DeleteKey(
//                 ScoreKey
//             );

//             PlayerPrefs.Save();
//         }

//         ScoreChanged?.Invoke(
//             currentScore
//         );
//     }


//     private void LoadScore()
//     {
//         currentScore =
//             saveScore
//                 ? PlayerPrefs.GetInt(
//                     ScoreKey,
//                     0
//                 )
//                 : 0;
//     }


//     private void SaveScore()
//     {
//         if (!saveScore)
//         {
//             return;
//         }

//         PlayerPrefs.SetInt(
//             ScoreKey,
//             currentScore
//         );

//         PlayerPrefs.Save();
//     }
// }





using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ScoreManager : MonoBehaviour
{
    private const string ScoreKey =
        "RoyalSmash.TotalScore";

    private const string PendingMilestoneScoreKey =
        "RoyalSmash.PendingMilestoneScore";

    private const string PendingMilestoneLevelKey =
        "RoyalSmash.PendingMilestoneLevel";

    private const string LastClaimedMilestoneLevelKey =
        "RoyalSmash.LastClaimedMilestoneLevel";


    public static ScoreManager Instance { get; private set; }


    [Header("Score / Coins")]
    [SerializeField, Min(0)]
    private int pointsPerLevel = 100;

    [Tooltip(
        "Har itne levels ke baad score coin animation complete hone tak pending rahega."
    )]
    [SerializeField, Min(1)]
    private int milestoneRewardInterval = 10;

    [SerializeField]
    private bool saveScore = true;


    [Header("References")]
    [SerializeField]
    private LevelRuntimeController levelRuntimeController;


    [Header("Behaviour")]
    [SerializeField]
    private bool dontDestroyOnLoad = true;


    private int currentScore;
    private int pendingMilestoneScore;
    private int pendingMilestoneLevel;
    private int lastClaimedMilestoneLevel;


    public int CurrentScore =>
        currentScore;

    public int PointsPerLevel =>
        pointsPerLevel;

    public int PendingMilestoneScore =>
        pendingMilestoneScore;

    public bool HasPendingMilestoneScore =>
        pendingMilestoneScore > 0;


    public event Action<int> ScoreChanged;

    public event Action<int, int> ScoreAdded;

    public event Action<int, int> ScoreSpent;


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

        LoadScore();
        LoadPendingMilestoneScore();
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded +=
            HandleSceneLoaded;

        ResolveLevelRuntimeController();
        SubscribeToLevelEvents();
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -=
            HandleSceneLoaded;

        UnsubscribeFromLevelEvents();
    }


    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        UnsubscribeFromLevelEvents();

        levelRuntimeController = null;

        ResolveLevelRuntimeController();
        SubscribeToLevelEvents();

        ScoreChanged?.Invoke(
            currentScore
        );
    }


    private void ResolveLevelRuntimeController()
    {
        if (levelRuntimeController != null)
        {
            return;
        }

        levelRuntimeController =
            FindFirstObjectByType<LevelRuntimeController>(
                FindObjectsInactive.Include
            );
    }


    private void SubscribeToLevelEvents()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        levelRuntimeController.LevelCompleted -=
            HandleLevelCompleted;

        levelRuntimeController.LevelCompleted +=
            HandleLevelCompleted;
    }


    private void UnsubscribeFromLevelEvents()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        levelRuntimeController.LevelCompleted -=
            HandleLevelCompleted;
    }


    private void HandleLevelCompleted(
        GridLevelData completedLevel)
    {
        int completedLevelNumber =
            completedLevel != null
                ? completedLevel.LevelNumber
                : 0;

        if (IsMilestoneLevel(completedLevelNumber))
        {
            QueueMilestoneScore(
                pointsPerLevel,
                completedLevelNumber
            );

            return;
        }

        AddScore(
            pointsPerLevel
        );
    }


    public bool IsMilestoneLevel(int levelNumber)
    {
        // return levelNumber > 0 &&
        //     milestoneRewardInterval > 0 &&
        //     levelNumber % milestoneRewardInterval == 0;


return levelNumber >= milestoneRewardInterval;


    }


    private void QueueMilestoneScore(
        int amount,
        int completedLevelNumber)
    {
        if (amount <= 0 ||
            completedLevelNumber <= 0 ||
            pendingMilestoneLevel == completedLevelNumber ||
            completedLevelNumber <= lastClaimedMilestoneLevel)
        {
            return;
        }

        pendingMilestoneScore = AddWithoutOverflow(
            pendingMilestoneScore,
            amount
        );

        pendingMilestoneLevel = completedLevelNumber;
        SavePendingMilestoneScore();
    }


    public int ClaimPendingMilestoneScore()
    {
        int amount = pendingMilestoneScore;
        int claimedLevel = pendingMilestoneLevel;

        if (amount <= 0)
        {
            return 0;
        }

        pendingMilestoneScore = 0;
        pendingMilestoneLevel = 0;
        lastClaimedMilestoneLevel = Mathf.Max(
            lastClaimedMilestoneLevel,
            claimedLevel
        );
        SavePendingMilestoneScore();
        AddScore(amount);

        return amount;
    }


    public bool CanAfford(
        int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return currentScore >= amount;
    }


    public bool TrySpendScore(
        int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (!CanAfford(amount))
        {
            return false;
        }

        currentScore -= amount;

        SaveScore();

        ScoreSpent?.Invoke(
            amount,
            currentScore
        );

        ScoreChanged?.Invoke(
            currentScore
        );

        return true;
    }


    public void AddScore(
        int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentScore = AddWithoutOverflow(
            currentScore,
            amount
        );

        SaveScore();

        ScoreAdded?.Invoke(
            amount,
            currentScore
        );

        ScoreChanged?.Invoke(
            currentScore
        );
    }


    public void SetScore(
        int value)
    {
        currentScore =
            Mathf.Max(
                0,
                value
            );

        SaveScore();

        ScoreChanged?.Invoke(
            currentScore
        );
    }


    public void ResetScore()
    {
        currentScore = 0;
        pendingMilestoneScore = 0;
        pendingMilestoneLevel = 0;
        lastClaimedMilestoneLevel = 0;

        if (saveScore)
        {
            PlayerPrefs.DeleteKey(
                ScoreKey
            );

            PlayerPrefs.DeleteKey(
                PendingMilestoneScoreKey
            );

            PlayerPrefs.DeleteKey(
                PendingMilestoneLevelKey
            );

            PlayerPrefs.DeleteKey(
                LastClaimedMilestoneLevelKey
            );

            PlayerPrefs.Save();
        }

        ScoreChanged?.Invoke(
            currentScore
        );
    }


    private void LoadScore()
    {
        currentScore =
            saveScore
                ? PlayerPrefs.GetInt(
                    ScoreKey,
                    0
                )
                : 0;
    }


    private void LoadPendingMilestoneScore()
    {
        pendingMilestoneScore = Mathf.Max(
            0,
            PlayerPrefs.GetInt(
                PendingMilestoneScoreKey,
                0
            )
        );

        pendingMilestoneLevel = Mathf.Max(
            0,
            PlayerPrefs.GetInt(
                PendingMilestoneLevelKey,
                0
            )
        );

        lastClaimedMilestoneLevel = Mathf.Max(
            0,
            PlayerPrefs.GetInt(
                LastClaimedMilestoneLevelKey,
                0
            )
        );
    }


    private void SavePendingMilestoneScore()
    {
        PlayerPrefs.SetInt(
            PendingMilestoneScoreKey,
            pendingMilestoneScore
        );

        PlayerPrefs.SetInt(
            PendingMilestoneLevelKey,
            pendingMilestoneLevel
        );

        PlayerPrefs.SetInt(
            LastClaimedMilestoneLevelKey,
            lastClaimedMilestoneLevel
        );

        PlayerPrefs.Save();
    }


    private static int AddWithoutOverflow(
        int currentValue,
        int amount)
    {
        long total =
            (long)currentValue + amount;

        return (int)Math.Min(
            int.MaxValue,
            total
        );
    }


    private void SaveScore()
    {
        if (!saveScore)
        {
            return;
        }

        PlayerPrefs.SetInt(
            ScoreKey,
            currentScore
        );

        PlayerPrefs.Save();
    }
}


