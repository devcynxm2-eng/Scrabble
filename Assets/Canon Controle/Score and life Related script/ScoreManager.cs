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


    public static ScoreManager Instance { get; private set; }


    [Header("Score / Coins")]
    [SerializeField, Min(0)]
    private int pointsPerLevel = 100;

    [SerializeField]
    private bool saveScore = true;


    [Header("References")]
    [SerializeField]
    private LevelRuntimeController levelRuntimeController;


    [Header("Behaviour")]
    [SerializeField]
    private bool dontDestroyOnLoad = true;


    private int currentScore;


    public int CurrentScore =>
        currentScore;

    public int PointsPerLevel =>
        pointsPerLevel;


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
        AddScore(
            pointsPerLevel
        );
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

        currentScore += amount;

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

        if (saveScore)
        {
            PlayerPrefs.DeleteKey(
                ScoreKey
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





