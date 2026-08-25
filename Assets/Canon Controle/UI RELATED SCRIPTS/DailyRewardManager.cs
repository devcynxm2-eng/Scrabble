// using System;
// using System.Collections.Generic;
// using UnityEngine;

// public enum DailyRewardType
// {
//     Coins,
//     Lives,
//     Custom
// }


// [Serializable]
// public sealed class DailyRewardDefinition
// {
//     [Header("Reward")]
//     public DailyRewardType rewardType = DailyRewardType.Coins;

//     [Min(1)]
//     public int amount = 1;

//     [Tooltip(
//         "Custom reward ke liye unique ID. " +
//         "Example: BeachBall, Rocket, Special."
//     )]
//     public string customRewardId = "";

//     [Tooltip(
//         "UI par reward ke neeche jo text show karna hai. " +
//         "Example: 500, x1, Special!"
//     )]
//     public string rewardDisplayText = "x1";
// }


// public sealed class DailyRewardManager : MonoBehaviour
// {
//     private const string InitializedKey =
//         "RoyalSmash.DailyReward.Initialized";

//     private const string CurrentDayKey =
//         "RoyalSmash.DailyReward.CurrentDay";

//     private const string NextClaimUtcTicksKey =
//         "RoyalSmash.DailyReward.NextClaimUtcTicks";

//     private const string CustomRewardPrefix =
//         "RoyalSmash.DailyReward.Custom.";


//     public static DailyRewardManager Instance { get; private set; }


//     [Header("7 Day Rewards")]
//     [SerializeField]
//     private List<DailyRewardDefinition> rewards =
//         new List<DailyRewardDefinition>();


//     [Header("Timing")]
//     [Tooltip(
//         "Default 86400 seconds = 24 hours. " +
//         "Testing ke liye temporary chhota value rakh sakte hain."
//     )]
//     [SerializeField, Min(1f)]
//     private float rewardIntervalSeconds = 86400f;

//     [Tooltip(
//         "Day 7 claim hone ke baad isi interval ke baad naya 7-day cycle start hoga."
//     )]
//     [SerializeField]
//     private bool restartCycleAfterDay7 = true;


//     [Header("References")]
//     [SerializeField] private ScoreManager scoreManager;
//     [SerializeField] private LifeManager lifeManager;


//     [Header("Behaviour")]
//     [SerializeField] private bool dontDestroyOnLoad = true;

//     [SerializeField, Min(0.1f)]
//     private float timerEventRefreshInterval = 0.25f;


//     /*
//      * 0 = Day 1 next/current reward
//      * 1 = Day 2 next/current reward
//      * ...
//      * 6 = Day 7 next/current reward
//      * 7 = Current 7-day cycle complete
//      */
//     private int currentDayIndex;

//     /*
//      * 0 = current reward immediately available.
//      * Otherwise UTC time jab current reward available hogi.
//      */
//     private long nextClaimUtcTicks;

//     private float nextTimerEventAt;


//     public int CurrentDayIndex =>
//         currentDayIndex;

//     public int RewardCount =>
//         rewards.Count;

//     public bool IsCycleComplete =>
//         currentDayIndex >= rewards.Count;

//     public bool CanClaimCurrentReward =>
//         !IsCycleComplete &&
//         GetSecondsUntilCurrentReward() <= 0f;


//     public event Action StateChanged;

//     public event Action<float> TimerChanged;

//     public event Action<
//         int,
//         DailyRewardDefinition
//     > RewardClaimed;


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

//         EnsureDefaultRewards();
//         ResolveReferences();
//         LoadOrCreateState();
//         ProcessExpiredCycleIfNeeded();
//         SaveState();
//     }


//     private void Start()
//     {
//         BroadcastAll();
//     }


//     private void Update()
//     {
//         ProcessExpiredCycleIfNeeded();

//         if (Time.unscaledTime <
//             nextTimerEventAt)
//         {
//             return;
//         }

//         nextTimerEventAt =
//             Time.unscaledTime +
//             timerEventRefreshInterval;

//         TimerChanged?.Invoke(
//             GetSecondsUntilCurrentReward()
//         );
//     }


//     private void OnApplicationPause(
//         bool paused)
//     {
//         if (paused)
//         {
//             SaveState();
//             return;
//         }

//         ProcessExpiredCycleIfNeeded();
//         SaveState();
//         BroadcastAll();
//     }


//     private void OnApplicationFocus(
//         bool hasFocus)
//     {
//         if (!hasFocus)
//         {
//             SaveState();
//             return;
//         }

//         ProcessExpiredCycleIfNeeded();
//         SaveState();
//         BroadcastAll();
//     }


//     private void OnApplicationQuit()
//     {
//         SaveState();
//     }


//     private void ResolveReferences()
//     {
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

//         if (lifeManager == null)
//         {
//             if (LifeManager.Instance != null)
//             {
//                 lifeManager =
//                     LifeManager.Instance;
//             }
//             else
//             {
//                 lifeManager =
//                     FindFirstObjectByType<LifeManager>(
//                         FindObjectsInactive.Include
//                     );
//             }
//         }
//     }


//     private void EnsureDefaultRewards()
//     {
//         /*
//          * Inspector mein custom list already bani hui ho to usko touch nahi karna.
//          */
//         if (rewards != null &&
//             rewards.Count > 0)
//         {
//             return;
//         }

//         rewards =
//             new List<DailyRewardDefinition>
//             {
//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Coins,
//                     amount = 500,
//                     rewardDisplayText = "500"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Lives,
//                     amount = 1,
//                     rewardDisplayText = "x1"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Custom,
//                     amount = 1,
//                     customRewardId = "BeachBall",
//                     rewardDisplayText = "x1"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Custom,
//                     amount = 1,
//                     customRewardId = "Rocket",
//                     rewardDisplayText = "x1"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Coins,
//                     amount = 1000,
//                     rewardDisplayText = "1000"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Lives,
//                     amount = 2,
//                     rewardDisplayText = "x2"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Custom,
//                     amount = 1,
//                     customRewardId = "Special",
//                     rewardDisplayText = "Special!"
//                 }
//             };
//     }


//     private void LoadOrCreateState()
//     {
//         bool initialized =
//             PlayerPrefs.GetInt(
//                 InitializedKey,
//                 0
//             ) == 1;

//         if (!initialized)
//         {
//             /*
//              * First install / first launch:
//              * Day 1 immediately available.
//              */
//             currentDayIndex = 0;
//             nextClaimUtcTicks = 0;

//             PlayerPrefs.SetInt(
//                 InitializedKey,
//                 1
//             );

//             SaveState();
//             return;
//         }

//         currentDayIndex =
//             Mathf.Clamp(
//                 PlayerPrefs.GetInt(
//                     CurrentDayKey,
//                     0
//                 ),
//                 0,
//                 rewards.Count
//             );

//         string storedTicks =
//             PlayerPrefs.GetString(
//                 NextClaimUtcTicksKey,
//                 "0"
//             );

//         if (!long.TryParse(
//                 storedTicks,
//                 out nextClaimUtcTicks))
//         {
//             nextClaimUtcTicks = 0;
//         }
//     }


//     public DailyRewardDefinition GetReward(
//         int dayIndex)
//     {
//         if (dayIndex < 0 ||
//             dayIndex >= rewards.Count)
//         {
//             return null;
//         }

//         return rewards[dayIndex];
//     }


//     public bool IsDayClaimed(
//         int dayIndex)
//     {
//         if (dayIndex < 0 ||
//             dayIndex >= rewards.Count)
//         {
//             return false;
//         }

//         return dayIndex <
//             currentDayIndex;
//     }


//     public bool IsCurrentDay(
//         int dayIndex)
//     {
//         return !IsCycleComplete &&
//             dayIndex == currentDayIndex;
//     }


//     public float GetSecondsUntilCurrentReward()
//     {
//         if (nextClaimUtcTicks <= 0)
//         {
//             return 0f;
//         }

//         long remainingTicks =
//             nextClaimUtcTicks -
//             DateTime.UtcNow.Ticks;

//         if (remainingTicks <= 0)
//         {
//             return 0f;
//         }

//         return (float)TimeSpan
//             .FromTicks(remainingTicks)
//             .TotalSeconds;
//     }


//     public float GetSecondsUntilDayUnlock(
//         int dayIndex)
//     {
//         if (dayIndex < 0 ||
//             dayIndex >= rewards.Count)
//         {
//             return 0f;
//         }

//         if (IsDayClaimed(dayIndex))
//         {
//             return 0f;
//         }

//         if (IsCycleComplete)
//         {
//             /*
//              * Current cycle complete hai.
//              * Sab current cycle days claimed hain.
//              */
//             return 0f;
//         }

//         if (dayIndex <
//             currentDayIndex)
//         {
//             return 0f;
//         }

//         float currentRemaining =
//             GetSecondsUntilCurrentReward();

//         int dayDifference =
//             dayIndex -
//             currentDayIndex;

//         if (dayDifference <= 0)
//         {
//             return currentRemaining;
//         }

//         /*
//          * Future cards par estimated sequential countdown:
//          *
//          * Current day available ho:
//          * Day+1 = 24h
//          * Day+2 = 48h
//          *
//          * Current day 5h baad available ho:
//          * Day+1 = 29h
//          */
//         return currentRemaining +
//             dayDifference *
//             rewardIntervalSeconds;
//     }


//     public bool TryClaimCurrentReward()
//     {
//         ProcessExpiredCycleIfNeeded();

//         if (!CanClaimCurrentReward)
//         {
//             BroadcastAll();
//             return false;
//         }

//         ResolveReferences();

//         int claimedDayIndex =
//             currentDayIndex;

//         DailyRewardDefinition reward =
//             GetReward(
//                 claimedDayIndex
//             );

//         if (reward == null)
//         {
//             return false;
//         }

//         GrantReward(
//             reward
//         );

//         currentDayIndex++;

//         if (IsCycleComplete)
//         {
//             if (restartCycleAfterDay7)
//             {
//                 nextClaimUtcTicks =
//                     DateTime.UtcNow.Ticks +
//                     SecondsToTicks(
//                         rewardIntervalSeconds
//                     );
//             }
//             else
//             {
//                 nextClaimUtcTicks = 0;
//             }
//         }
//         else
//         {
//             /*
//              * Next day ka reward claim ke 24h baad unlock hoga.
//              */
//             nextClaimUtcTicks =
//                 DateTime.UtcNow.Ticks +
//                 SecondsToTicks(
//                     rewardIntervalSeconds
//                 );
//         }

//         SaveState();

//         RewardClaimed?.Invoke(
//             claimedDayIndex,
//             reward
//         );

//         BroadcastAll();

//         return true;
//     }


//     private void GrantReward(
//         DailyRewardDefinition reward)
//     {
//         int safeAmount =
//             Mathf.Max(
//                 1,
//                 reward.amount
//             );

//         switch (reward.rewardType)
//         {
//             case DailyRewardType.Coins:
//             {
//                 if (scoreManager != null)
//                 {
//                     scoreManager.AddScore(
//                         safeAmount
//                     );
//                 }
//                 else
//                 {
//                     Debug.LogWarning(
//                         "DailyRewardManager: ScoreManager missing hai. Coin reward grant nahi hui.",
//                         this
//                     );
//                 }

//                 break;
//             }

//             case DailyRewardType.Lives:
//             {
//                 if (lifeManager != null)
//                 {
//                     lifeManager.AddLives(
//                         safeAmount
//                     );
//                 }
//                 else
//                 {
//                     Debug.LogWarning(
//                         "DailyRewardManager: LifeManager missing hai. Life reward grant nahi hui.",
//                         this
//                     );
//                 }

//                 break;
//             }

//             case DailyRewardType.Custom:
//             {
//                 AddCustomReward(
//                     reward.customRewardId,
//                     safeAmount
//                 );

//                 break;
//             }
//         }
//     }


//     private void AddCustomReward(
//         string rewardId,
//         int amount)
//     {
//         if (string.IsNullOrWhiteSpace(
//                 rewardId) ||
//             amount <= 0)
//         {
//             Debug.LogWarning(
//                 "DailyRewardManager: Custom reward ID missing hai.",
//                 this
//             );

//             return;
//         }

//         string key =
//             GetCustomRewardKey(
//                 rewardId
//             );

//         int currentAmount =
//             PlayerPrefs.GetInt(
//                 key,
//                 0
//             );

//         PlayerPrefs.SetInt(
//             key,
//             currentAmount + amount
//         );

//         PlayerPrefs.Save();
//     }


//     public int GetCustomRewardCount(
//         string rewardId)
//     {
//         if (string.IsNullOrWhiteSpace(
//                 rewardId))
//         {
//             return 0;
//         }

//         return PlayerPrefs.GetInt(
//             GetCustomRewardKey(
//                 rewardId
//             ),
//             0
//         );
//     }


//     public bool TryConsumeCustomReward(
//         string rewardId,
//         int amount = 1)
//     {
//         if (string.IsNullOrWhiteSpace(
//                 rewardId) ||
//             amount <= 0)
//         {
//             return false;
//         }

//         string key =
//             GetCustomRewardKey(
//                 rewardId
//             );

//         int currentAmount =
//             PlayerPrefs.GetInt(
//                 key,
//                 0
//             );

//         if (currentAmount <
//             amount)
//         {
//             return false;
//         }

//         PlayerPrefs.SetInt(
//             key,
//             currentAmount - amount
//         );

//         PlayerPrefs.Save();

//         return true;
//     }


//     private string GetCustomRewardKey(
//         string rewardId)
//     {
//         return CustomRewardPrefix +
//             rewardId.Trim();
//     }


//     private void ProcessExpiredCycleIfNeeded()
//     {
//         if (!IsCycleComplete ||
//             !restartCycleAfterDay7)
//         {
//             return;
//         }

//         if (nextClaimUtcTicks <= 0)
//         {
//             return;
//         }

//         if (DateTime.UtcNow.Ticks <
//             nextClaimUtcTicks)
//         {
//             return;
//         }

//         /*
//          * 7-day cycle complete + cooldown complete:
//          * Day 1 dobara immediately available.
//          */
//         currentDayIndex = 0;
//         nextClaimUtcTicks = 0;

//         SaveState();
//         StateChanged?.Invoke();
//     }


//     private static long SecondsToTicks(
//         float seconds)
//     {
//         return TimeSpan
//             .FromSeconds(
//                 Math.Max(
//                     1d,
//                     seconds
//                 )
//             )
//             .Ticks;
//     }


//     private void BroadcastAll()
//     {
//         StateChanged?.Invoke();

//         TimerChanged?.Invoke(
//             GetSecondsUntilCurrentReward()
//         );
//     }


//     private void SaveState()
//     {
//         PlayerPrefs.SetInt(
//             CurrentDayKey,
//             currentDayIndex
//         );

//         PlayerPrefs.SetString(
//             NextClaimUtcTicksKey,
//             nextClaimUtcTicks.ToString()
//         );

//         PlayerPrefs.SetInt(
//             InitializedKey,
//             1
//         );

//         PlayerPrefs.Save();
//     }


//     [ContextMenu("DEBUG / Claim Current Reward")]
//     private void DebugClaimCurrentReward()
//     {
//         TryClaimCurrentReward();
//     }


//     [ContextMenu("DEBUG / Make Current Reward Ready")]
//     private void DebugMakeCurrentRewardReady()
//     {
//         if (IsCycleComplete)
//         {
//             currentDayIndex = 0;
//         }

//         nextClaimUtcTicks = 0;

//         SaveState();
//         BroadcastAll();
//     }


//     [ContextMenu("DEBUG / Reset Daily Reward Progress")]
//     private void DebugResetDailyRewardProgress()
//     {
//         currentDayIndex = 0;
//         nextClaimUtcTicks = 0;

//         PlayerPrefs.SetInt(
//             InitializedKey,
//             1
//         );

//         SaveState();
//         BroadcastAll();
//     }
// }











// using System;
// using System.Collections.Generic;
// using UnityEngine;

// public enum DailyRewardType
// {
//     Coins,
//     Lives,
//     InfiniteBalls,
//     PowerCannon,
//     Custom
// }


// [Serializable]
// public sealed class DailyRewardDefinition
// {
//     [Header("Reward")]
//     public DailyRewardType rewardType = DailyRewardType.Coins;

//     [Min(1)]
//     public int amount = 1;

//     [Tooltip(
//         "Custom reward ke liye unique ID. " +
//         "Example: BeachBall, Rocket, Special."
//     )]
//     public string customRewardId = "";

//     [Header("Custom Bundle")]
//     [Tooltip(
//         "Sirf Reward Type = Custom par use hoga. " +
//         "Custom reward ke sath coins bhi de sakte hain."
//     )]
//     [Min(0)]
//     public int customCoins = 0;

//     [Tooltip(
//         "Sirf Reward Type = Custom par use hoga. " +
//         "Custom reward ke sath lives bhi de sakte hain."
//     )]
//     [Min(0)]
//     public int customLives = 0;

//     [Tooltip(
//         "Sirf Reward Type = Custom par use hoga. " +
//         "Custom reward ke sath Infinite Balls power-ups bhi de sakte hain."
//     )]
//     [Min(0)]
//     public int customInfiniteBalls = 0;

//     [Tooltip(
//         "Sirf Reward Type = Custom par use hoga. " +
//         "Custom reward ke sath Power Cannon power-ups bhi de sakte hain."
//     )]
//     [Min(0)]
//     public int customPowerCannon = 0;

//     [Tooltip(
//         "UI par reward ke neeche jo text show karna hai. " +
//         "Example: 500, x1, Special!, 500 + x1"
//     )]
//     public string rewardDisplayText = "x1";
// }


// public sealed class DailyRewardManager : MonoBehaviour
// {
//     private const string InitializedKey =
//         "RoyalSmash.DailyReward.Initialized";

//     private const string CurrentDayKey =
//         "RoyalSmash.DailyReward.CurrentDay";

//     private const string NextClaimUtcTicksKey =
//         "RoyalSmash.DailyReward.NextClaimUtcTicks";

//     private const string CustomRewardPrefix =
//         "RoyalSmash.DailyReward.Custom.";


//     public static DailyRewardManager Instance { get; private set; }


//     [Header("7 Day Rewards")]
//     [SerializeField]
//     private List<DailyRewardDefinition> rewards =
//         new List<DailyRewardDefinition>();


//     [Header("Timing")]
//     [Tooltip(
//         "Default 86400 seconds = 24 hours. " +
//         "Testing ke liye temporary chhota value rakh sakte hain."
//     )]
//     [SerializeField, Min(1f)]
//     private float rewardIntervalSeconds = 86400f;

//     [Tooltip(
//         "Day 7 claim hone ke baad isi interval ke baad naya 7-day cycle start hoga."
//     )]
//     [SerializeField]
//     private bool restartCycleAfterDay7 = true;


//     [Header("References")]
//     [SerializeField] private ScoreManager scoreManager;
//     [SerializeField] private LifeManager lifeManager;
//     [SerializeField] private PowerUpInventoryManager powerUpInventoryManager;


//     [Header("Behaviour")]
//     [SerializeField] private bool dontDestroyOnLoad = true;

//     [SerializeField, Min(0.1f)]
//     private float timerEventRefreshInterval = 0.25f;


//     /*
//      * 0 = Day 1 next/current reward
//      * 1 = Day 2 next/current reward
//      * ...
//      * 6 = Day 7 next/current reward
//      * 7 = Current 7-day cycle complete
//      */
//     private int currentDayIndex;

//     /*
//      * 0 = current reward immediately available.
//      * Otherwise UTC time jab current reward available hogi.
//      */
//     private long nextClaimUtcTicks;

//     private float nextTimerEventAt;


//     public int CurrentDayIndex =>
//         currentDayIndex;

//     public int RewardCount =>
//         rewards.Count;

//     public bool IsCycleComplete =>
//         currentDayIndex >= rewards.Count;

//     public bool CanClaimCurrentReward =>
//         !IsCycleComplete &&
//         GetSecondsUntilCurrentReward() <= 0f;


//     public event Action StateChanged;

//     public event Action<float> TimerChanged;

//     public event Action<
//         int,
//         DailyRewardDefinition
//     > RewardClaimed;


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

//         EnsureDefaultRewards();
//         ResolveReferences();
//         LoadOrCreateState();
//         ProcessExpiredCycleIfNeeded();
//         SaveState();
//     }


//     private void Start()
//     {
//         BroadcastAll();
//     }


//     private void Update()
//     {
//         ProcessExpiredCycleIfNeeded();

//         if (Time.unscaledTime <
//             nextTimerEventAt)
//         {
//             return;
//         }

//         nextTimerEventAt =
//             Time.unscaledTime +
//             timerEventRefreshInterval;

//         TimerChanged?.Invoke(
//             GetSecondsUntilCurrentReward()
//         );
//     }


//     private void OnApplicationPause(
//         bool paused)
//     {
//         if (paused)
//         {
//             SaveState();
//             return;
//         }

//         ProcessExpiredCycleIfNeeded();
//         SaveState();
//         BroadcastAll();
//     }


//     private void OnApplicationFocus(
//         bool hasFocus)
//     {
//         if (!hasFocus)
//         {
//             SaveState();
//             return;
//         }

//         ProcessExpiredCycleIfNeeded();
//         SaveState();
//         BroadcastAll();
//     }


//     private void OnApplicationQuit()
//     {
//         SaveState();
//     }


//     private void ResolveReferences()
//     {
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

//         if (lifeManager == null)
//         {
//             if (LifeManager.Instance != null)
//             {
//                 lifeManager =
//                     LifeManager.Instance;
//             }
//             else
//             {
//                 lifeManager =
//                     FindFirstObjectByType<LifeManager>(
//                         FindObjectsInactive.Include
//                     );
//             }
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
//     }


//     private void EnsureDefaultRewards()
//     {
//         /*
//          * Inspector mein custom list already bani hui ho to usko touch nahi karna.
//          */
//         if (rewards != null &&
//             rewards.Count > 0)
//         {
//             return;
//         }

//         rewards =
//             new List<DailyRewardDefinition>
//             {
//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Coins,
//                     amount = 500,
//                     rewardDisplayText = "500"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Lives,
//                     amount = 1,
//                     rewardDisplayText = "x1"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Custom,
//                     amount = 1,
//                     customRewardId = "BeachBall",
//                     rewardDisplayText = "x1"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Custom,
//                     amount = 1,
//                     customRewardId = "Rocket",
//                     rewardDisplayText = "x1"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Coins,
//                     amount = 1000,
//                     rewardDisplayText = "1000"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Lives,
//                     amount = 2,
//                     rewardDisplayText = "x2"
//                 },

//                 new DailyRewardDefinition
//                 {
//                     rewardType = DailyRewardType.Custom,
//                     amount = 1,
//                     customRewardId = "Special",
//                     rewardDisplayText = "Special!"
//                 }
//             };
//     }


//     private void LoadOrCreateState()
//     {
//         bool initialized =
//             PlayerPrefs.GetInt(
//                 InitializedKey,
//                 0
//             ) == 1;

//         if (!initialized)
//         {
//             /*
//              * First install / first launch:
//              * Day 1 immediately available.
//              */
//             currentDayIndex = 0;
//             nextClaimUtcTicks = 0;

//             PlayerPrefs.SetInt(
//                 InitializedKey,
//                 1
//             );

//             SaveState();
//             return;
//         }

//         currentDayIndex =
//             Mathf.Clamp(
//                 PlayerPrefs.GetInt(
//                     CurrentDayKey,
//                     0
//                 ),
//                 0,
//                 rewards.Count
//             );

//         string storedTicks =
//             PlayerPrefs.GetString(
//                 NextClaimUtcTicksKey,
//                 "0"
//             );

//         if (!long.TryParse(
//                 storedTicks,
//                 out nextClaimUtcTicks))
//         {
//             nextClaimUtcTicks = 0;
//         }
//     }


//     public DailyRewardDefinition GetReward(
//         int dayIndex)
//     {
//         if (dayIndex < 0 ||
//             dayIndex >= rewards.Count)
//         {
//             return null;
//         }

//         return rewards[dayIndex];
//     }


//     public bool IsDayClaimed(
//         int dayIndex)
//     {
//         if (dayIndex < 0 ||
//             dayIndex >= rewards.Count)
//         {
//             return false;
//         }

//         return dayIndex <
//             currentDayIndex;
//     }


//     public bool IsCurrentDay(
//         int dayIndex)
//     {
//         return !IsCycleComplete &&
//             dayIndex == currentDayIndex;
//     }


//     public float GetSecondsUntilCurrentReward()
//     {
//         if (nextClaimUtcTicks <= 0)
//         {
//             return 0f;
//         }

//         long remainingTicks =
//             nextClaimUtcTicks -
//             DateTime.UtcNow.Ticks;

//         if (remainingTicks <= 0)
//         {
//             return 0f;
//         }

//         return (float)TimeSpan
//             .FromTicks(remainingTicks)
//             .TotalSeconds;
//     }


//     public float GetSecondsUntilDayUnlock(
//         int dayIndex)
//     {
//         if (dayIndex < 0 ||
//             dayIndex >= rewards.Count)
//         {
//             return 0f;
//         }

//         if (IsDayClaimed(dayIndex))
//         {
//             return 0f;
//         }

//         if (IsCycleComplete)
//         {
//             /*
//              * Current cycle complete hai.
//              * Sab current cycle days claimed hain.
//              */
//             return 0f;
//         }

//         if (dayIndex <
//             currentDayIndex)
//         {
//             return 0f;
//         }

//         float currentRemaining =
//             GetSecondsUntilCurrentReward();

//         int dayDifference =
//             dayIndex -
//             currentDayIndex;

//         if (dayDifference <= 0)
//         {
//             return currentRemaining;
//         }

//         /*
//          * Future cards par estimated sequential countdown:
//          *
//          * Current day available ho:
//          * Day+1 = 24h
//          * Day+2 = 48h
//          *
//          * Current day 5h baad available ho:
//          * Day+1 = 29h
//          */
//         return currentRemaining +
//             dayDifference *
//             rewardIntervalSeconds;
//     }


//     public bool TryClaimCurrentReward()
//     {
//         ProcessExpiredCycleIfNeeded();

//         if (!CanClaimCurrentReward)
//         {
//             BroadcastAll();
//             return false;
//         }

//         ResolveReferences();

//         int claimedDayIndex =
//             currentDayIndex;

//         DailyRewardDefinition reward =
//             GetReward(
//                 claimedDayIndex
//             );

//         if (reward == null)
//         {
//             return false;
//         }

//         GrantReward(
//             reward
//         );

//         currentDayIndex++;

//         if (IsCycleComplete)
//         {
//             if (restartCycleAfterDay7)
//             {
//                 nextClaimUtcTicks =
//                     DateTime.UtcNow.Ticks +
//                     SecondsToTicks(
//                         rewardIntervalSeconds
//                     );
//             }
//             else
//             {
//                 nextClaimUtcTicks = 0;
//             }
//         }
//         else
//         {
//             /*
//              * Next day ka reward claim ke 24h baad unlock hoga.
//              */
//             nextClaimUtcTicks =
//                 DateTime.UtcNow.Ticks +
//                 SecondsToTicks(
//                     rewardIntervalSeconds
//                 );
//         }

//         SaveState();

//         RewardClaimed?.Invoke(
//             claimedDayIndex,
//             reward
//         );

//         BroadcastAll();

//         return true;
//     }


//     private void GrantReward(
//         DailyRewardDefinition reward)
//     {
//         int safeAmount =
//             Mathf.Max(
//                 1,
//                 reward.amount
//             );

//         switch (reward.rewardType)
//         {
//             case DailyRewardType.Coins:
//             {
//                 if (scoreManager != null)
//                 {
//                     scoreManager.AddScore(
//                         safeAmount
//                     );
//                 }
//                 else
//                 {
//                     Debug.LogWarning(
//                         "DailyRewardManager: ScoreManager missing hai. Coin reward grant nahi hui.",
//                         this
//                     );
//                 }

//                 break;
//             }

//             case DailyRewardType.Lives:
//             {
//                 if (lifeManager != null)
//                 {
//                     lifeManager.AddLives(
//                         safeAmount
//                     );
//                 }
//                 else
//                 {
//                     Debug.LogWarning(
//                         "DailyRewardManager: LifeManager missing hai. Life reward grant nahi hui.",
//                         this
//                     );
//                 }

//                 break;
//             }

//             case DailyRewardType.InfiniteBalls:
//             {
//                 if (powerUpInventoryManager != null)
//                 {
//                     powerUpInventoryManager.AddInfiniteBalls(
//                         safeAmount
//                     );
//                 }
//                 else
//                 {
//                     Debug.LogWarning(
//                         "DailyRewardManager: PowerUpInventoryManager missing hai. Infinite Balls reward grant nahi hui.",
//                         this
//                     );
//                 }

//                 break;
//             }

//             case DailyRewardType.PowerCannon:
//             {
//                 if (powerUpInventoryManager != null)
//                 {
//                     powerUpInventoryManager.AddPowerCannon(
//                         safeAmount
//                     );
//                 }
//                 else
//                 {
//                     Debug.LogWarning(
//                         "DailyRewardManager: PowerUpInventoryManager missing hai. Power Cannon reward grant nahi hui.",
//                         this
//                     );
//                 }

//                 break;
//             }

//             case DailyRewardType.Custom:
//             {
//                 GrantCustomBundle(
//                     reward,
//                     safeAmount
//                 );

//                 break;
//             }
//         }
//     }


//     private void GrantCustomBundle(
//         DailyRewardDefinition reward,
//         int safeCustomItemAmount)
//     {
//         if (reward == null)
//         {
//             return;
//         }

//         /*
//          * CUSTOM reward ab bundle ho sakta hai:
//          *
//          * - Coins
//          * - Lives
//          * - Infinite Balls
//          * - Power Cannon
//          * - Existing custom item ID
//          *
//          * In sab ko ek hi daily reward card se
//          * individually ya combination mein grant kar sakte hain.
//          */

//         if (reward.customCoins > 0)
//         {
//             if (scoreManager != null)
//             {
//                 scoreManager.AddScore(
//                     reward.customCoins
//                 );
//             }
//             else
//             {
//                 Debug.LogWarning(
//                     "DailyRewardManager: ScoreManager missing hai. Custom coins grant nahi huay.",
//                     this
//                 );
//             }
//         }

//         if (reward.customLives > 0)
//         {
//             if (lifeManager != null)
//             {
//                 lifeManager.AddLives(
//                     reward.customLives
//                 );
//             }
//             else
//             {
//                 Debug.LogWarning(
//                     "DailyRewardManager: LifeManager missing hai. Custom lives grant nahi huin.",
//                     this
//                 );
//             }
//         }

//         if (reward.customInfiniteBalls > 0)
//         {
//             if (powerUpInventoryManager != null)
//             {
//                 powerUpInventoryManager.AddInfiniteBalls(
//                     reward.customInfiniteBalls
//                 );
//             }
//             else
//             {
//                 Debug.LogWarning(
//                     "DailyRewardManager: PowerUpInventoryManager missing hai. Custom Infinite Balls grant nahi huay.",
//                     this
//                 );
//             }
//         }

//         if (reward.customPowerCannon > 0)
//         {
//             if (powerUpInventoryManager != null)
//             {
//                 powerUpInventoryManager.AddPowerCannon(
//                     reward.customPowerCannon
//                 );
//             }
//             else
//             {
//                 Debug.LogWarning(
//                     "DailyRewardManager: PowerUpInventoryManager missing hai. Custom Power Cannon grant nahi huay.",
//                     this
//                 );
//             }
//         }

//         /*
//          * Existing customRewardId support bhi same rakha hai.
//          * Agar ID blank hai to koi custom item grant nahi hoga.
//          *
//          * Existing 'amount' field Custom item ID ki quantity ke liye
//          * use hota rahega.
//          */
//         if (!string.IsNullOrWhiteSpace(
//                 reward.customRewardId))
//         {
//             AddCustomReward(
//                 reward.customRewardId,
//                 safeCustomItemAmount
//             );
//         }

//         bool nothingConfigured =
//             reward.customCoins <= 0 &&
//             reward.customLives <= 0 &&
//             reward.customInfiniteBalls <= 0 &&
//             reward.customPowerCannon <= 0 &&
//             string.IsNullOrWhiteSpace(
//                 reward.customRewardId);

//         if (nothingConfigured)
//         {
//             Debug.LogWarning(
//                 "DailyRewardManager: Custom reward selected hai lekin bundle mein koi reward configure nahi kiya.",
//                 this
//             );
//         }
//     }


//     private void AddCustomReward(
//         string rewardId,
//         int amount)
//     {
//         if (string.IsNullOrWhiteSpace(
//                 rewardId) ||
//             amount <= 0)
//         {
//             Debug.LogWarning(
//                 "DailyRewardManager: Custom reward ID missing hai.",
//                 this
//             );

//             return;
//         }

//         string key =
//             GetCustomRewardKey(
//                 rewardId
//             );

//         int currentAmount =
//             PlayerPrefs.GetInt(
//                 key,
//                 0
//             );

//         PlayerPrefs.SetInt(
//             key,
//             currentAmount + amount
//         );

//         PlayerPrefs.Save();
//     }


//     public int GetCustomRewardCount(
//         string rewardId)
//     {
//         if (string.IsNullOrWhiteSpace(
//                 rewardId))
//         {
//             return 0;
//         }

//         return PlayerPrefs.GetInt(
//             GetCustomRewardKey(
//                 rewardId
//             ),
//             0
//         );
//     }


//     public bool TryConsumeCustomReward(
//         string rewardId,
//         int amount = 1)
//     {
//         if (string.IsNullOrWhiteSpace(
//                 rewardId) ||
//             amount <= 0)
//         {
//             return false;
//         }

//         string key =
//             GetCustomRewardKey(
//                 rewardId
//             );

//         int currentAmount =
//             PlayerPrefs.GetInt(
//                 key,
//                 0
//             );

//         if (currentAmount <
//             amount)
//         {
//             return false;
//         }

//         PlayerPrefs.SetInt(
//             key,
//             currentAmount - amount
//         );

//         PlayerPrefs.Save();

//         return true;
//     }


//     private string GetCustomRewardKey(
//         string rewardId)
//     {
//         return CustomRewardPrefix +
//             rewardId.Trim();
//     }


//     private void ProcessExpiredCycleIfNeeded()
//     {
//         if (!IsCycleComplete ||
//             !restartCycleAfterDay7)
//         {
//             return;
//         }

//         if (nextClaimUtcTicks <= 0)
//         {
//             return;
//         }

//         if (DateTime.UtcNow.Ticks <
//             nextClaimUtcTicks)
//         {
//             return;
//         }

//         /*
//          * 7-day cycle complete + cooldown complete:
//          * Day 1 dobara immediately available.
//          */
//         currentDayIndex = 0;
//         nextClaimUtcTicks = 0;

//         SaveState();
//         StateChanged?.Invoke();
//     }


//     private static long SecondsToTicks(
//         float seconds)
//     {
//         return TimeSpan
//             .FromSeconds(
//                 Math.Max(
//                     1d,
//                     seconds
//                 )
//             )
//             .Ticks;
//     }


//     private void BroadcastAll()
//     {
//         StateChanged?.Invoke();

//         TimerChanged?.Invoke(
//             GetSecondsUntilCurrentReward()
//         );
//     }


//     private void SaveState()
//     {
//         PlayerPrefs.SetInt(
//             CurrentDayKey,
//             currentDayIndex
//         );

//         PlayerPrefs.SetString(
//             NextClaimUtcTicksKey,
//             nextClaimUtcTicks.ToString()
//         );

//         PlayerPrefs.SetInt(
//             InitializedKey,
//             1
//         );

//         PlayerPrefs.Save();
//     }


//     [ContextMenu("DEBUG / Claim Current Reward")]
//     private void DebugClaimCurrentReward()
//     {
//         TryClaimCurrentReward();
//     }


//     [ContextMenu("DEBUG / Make Current Reward Ready")]
//     private void DebugMakeCurrentRewardReady()
//     {
//         if (IsCycleComplete)
//         {
//             currentDayIndex = 0;
//         }

//         nextClaimUtcTicks = 0;

//         SaveState();
//         BroadcastAll();
//     }


//     [ContextMenu("DEBUG / Reset Daily Reward Progress")]
//     private void DebugResetDailyRewardProgress()
//     {
//         currentDayIndex = 0;
//         nextClaimUtcTicks = 0;

//         PlayerPrefs.SetInt(
//             InitializedKey,
//             1
//         );

//         SaveState();
//         BroadcastAll();
//     }
// }








using System;
using System.Collections.Generic;
using UnityEngine;

public enum DailyRewardType
{
    Coins,
    Lives,
    InfiniteBalls,
    PowerCannon,
    Custom
}


[Serializable]
public sealed class DailyRewardDefinition
{
    [Header("Reward")]
    public DailyRewardType rewardType = DailyRewardType.Coins;

    [Min(1)]
    public int amount = 1;

    [Tooltip(
        "Custom reward ke liye unique ID. " +
        "Example: BeachBall, Rocket, Special."
    )]
    public string customRewardId = "";

    [Header("Custom Bundle")]
    [Tooltip(
        "Sirf Reward Type = Custom par use hoga. " +
        "Custom reward ke sath coins bhi de sakte hain."
    )]
    [Min(0)]
    public int customCoins = 0;

    [Tooltip(
        "Sirf Reward Type = Custom par use hoga. " +
        "Custom reward ke sath lives bhi de sakte hain."
    )]
    [Min(0)]
    public int customLives = 0;

    [Tooltip(
        "Sirf Reward Type = Custom par use hoga. " +
        "Custom reward ke sath Infinite Balls power-ups bhi de sakte hain."
    )]
    [Min(0)]
    public int customInfiniteBalls = 0;

    [Tooltip(
        "Sirf Reward Type = Custom par use hoga. " +
        "Custom reward ke sath Power Cannon power-ups bhi de sakte hain."
    )]
    [Min(0)]
    public int customPowerCannon = 0;

    [Tooltip(
        "UI par reward ke neeche jo text show karna hai. " +
        "Example: 500, x1, Special!, 500 + x1"
    )]
    public string rewardDisplayText = "x1";
}


public sealed class DailyRewardManager : MonoBehaviour
{
    private const string InitializedKey =
        "RoyalSmash.DailyReward.Initialized";

    private const string CurrentDayKey =
        "RoyalSmash.DailyReward.CurrentDay";

    private const string NextClaimUtcTicksKey =
        "RoyalSmash.DailyReward.NextClaimUtcTicks";

    private const string CustomRewardPrefix =
        "RoyalSmash.DailyReward.Custom.";


    public static DailyRewardManager Instance { get; private set; }


    [Header("7 Day Rewards")]
    [SerializeField]
    private List<DailyRewardDefinition> rewards =
        new List<DailyRewardDefinition>();


    [Header("Timing")]
    [Tooltip(
        "Default 86400 seconds = 24 hours. " +
        "Testing ke liye temporary chhota value rakh sakte hain."
    )]
    [SerializeField, Min(1f)]
    private float rewardIntervalSeconds = 86400f;

    [Tooltip(
        "ON ho to Day 7 claim ke baad cycle Day 1 par reset ho jayega. " +
        "Naya Day 1 normal reward interval ke baad claimable hoga."
    )]
    [SerializeField]
    private bool restartCycleAfterDay7 = true;


    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LifeManager lifeManager;
    [SerializeField] private PowerUpInventoryManager powerUpInventoryManager;


    [Header("Behaviour")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [SerializeField, Min(0.1f)]
    private float timerEventRefreshInterval = 0.25f;


    /*
     * 0 = Day 1 next/current reward
     * 1 = Day 2 next/current reward
     * ...
     * 6 = Day 7 next/current reward
     * 7 = Current 7-day cycle complete
     */
    private int currentDayIndex;

    /*
     * 0 = current reward immediately available.
     * Otherwise UTC time jab current reward available hogi.
     */
    private long nextClaimUtcTicks;

    private float nextTimerEventAt;


    public int CurrentDayIndex =>
        currentDayIndex;

    public int RewardCount =>
        rewards.Count;

    public bool IsCycleComplete =>
        currentDayIndex >= rewards.Count;

    public bool CanClaimCurrentReward =>
        !IsCycleComplete &&
        GetSecondsUntilCurrentReward() <= 0f;


    public event Action StateChanged;

    public event Action<float> TimerChanged;

    public event Action<
        int,
        DailyRewardDefinition
    > RewardClaimed;


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

        EnsureDefaultRewards();
        ResolveReferences();
        LoadOrCreateState();
        ProcessExpiredCycleIfNeeded();
        SaveState();
    }


    private void Start()
    {
        BroadcastAll();
    }


    private void Update()
    {
        ProcessExpiredCycleIfNeeded();

        if (Time.unscaledTime <
            nextTimerEventAt)
        {
            return;
        }

        nextTimerEventAt =
            Time.unscaledTime +
            timerEventRefreshInterval;

        TimerChanged?.Invoke(
            GetSecondsUntilCurrentReward()
        );
    }


    private void OnApplicationPause(
        bool paused)
    {
        if (paused)
        {
            SaveState();
            return;
        }

        ProcessExpiredCycleIfNeeded();
        SaveState();
        BroadcastAll();
    }


    private void OnApplicationFocus(
        bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveState();
            return;
        }

        ProcessExpiredCycleIfNeeded();
        SaveState();
        BroadcastAll();
    }


    private void OnApplicationQuit()
    {
        SaveState();
    }


    private void ResolveReferences()
    {
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

        if (lifeManager == null)
        {
            if (LifeManager.Instance != null)
            {
                lifeManager =
                    LifeManager.Instance;
            }
            else
            {
                lifeManager =
                    FindFirstObjectByType<LifeManager>(
                        FindObjectsInactive.Include
                    );
            }
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
    }


    private void EnsureDefaultRewards()
    {
        /*
         * Inspector mein custom list already bani hui ho to usko touch nahi karna.
         */
        if (rewards != null &&
            rewards.Count > 0)
        {
            return;
        }

        rewards =
            new List<DailyRewardDefinition>
            {
                new DailyRewardDefinition
                {
                    rewardType = DailyRewardType.Coins,
                    amount = 500,
                    rewardDisplayText = "500"
                },

                new DailyRewardDefinition
                {
                    rewardType = DailyRewardType.Lives,
                    amount = 1,
                    rewardDisplayText = "x1"
                },

                new DailyRewardDefinition
                {
                    rewardType = DailyRewardType.Custom,
                    amount = 1,
                    customRewardId = "BeachBall",
                    rewardDisplayText = "x1"
                },

                new DailyRewardDefinition
                {
                    rewardType = DailyRewardType.Custom,
                    amount = 1,
                    customRewardId = "Rocket",
                    rewardDisplayText = "x1"
                },

                new DailyRewardDefinition
                {
                    rewardType = DailyRewardType.Coins,
                    amount = 1000,
                    rewardDisplayText = "1000"
                },

                new DailyRewardDefinition
                {
                    rewardType = DailyRewardType.Lives,
                    amount = 2,
                    rewardDisplayText = "x2"
                },

                new DailyRewardDefinition
                {
                    rewardType = DailyRewardType.Custom,
                    amount = 1,
                    customRewardId = "Special",
                    rewardDisplayText = "Special!"
                }
            };
    }


    private void LoadOrCreateState()
    {
        bool initialized =
            PlayerPrefs.GetInt(
                InitializedKey,
                0
            ) == 1;

        if (!initialized)
        {
            /*
             * First install / first launch:
             * Day 1 immediately available.
             */
            currentDayIndex = 0;
            nextClaimUtcTicks = 0;

            PlayerPrefs.SetInt(
                InitializedKey,
                1
            );

            SaveState();
            return;
        }

        currentDayIndex =
            Mathf.Clamp(
                PlayerPrefs.GetInt(
                    CurrentDayKey,
                    0
                ),
                0,
                rewards.Count
            );

        string storedTicks =
            PlayerPrefs.GetString(
                NextClaimUtcTicksKey,
                "0"
            );

        if (!long.TryParse(
                storedTicks,
                out nextClaimUtcTicks))
        {
            nextClaimUtcTicks = 0;
        }
    }


    public DailyRewardDefinition GetReward(
        int dayIndex)
    {
        if (dayIndex < 0 ||
            dayIndex >= rewards.Count)
        {
            return null;
        }

        return rewards[dayIndex];
    }


    public bool IsDayClaimed(
        int dayIndex)
    {
        if (dayIndex < 0 ||
            dayIndex >= rewards.Count)
        {
            return false;
        }

        return dayIndex <
            currentDayIndex;
    }


    public bool IsCurrentDay(
        int dayIndex)
    {
        return !IsCycleComplete &&
            dayIndex == currentDayIndex;
    }


    public float GetSecondsUntilCurrentReward()
    {
        if (nextClaimUtcTicks <= 0)
        {
            return 0f;
        }

        long remainingTicks =
            nextClaimUtcTicks -
            DateTime.UtcNow.Ticks;

        if (remainingTicks <= 0)
        {
            return 0f;
        }

        return (float)TimeSpan
            .FromTicks(remainingTicks)
            .TotalSeconds;
    }


    public float GetSecondsUntilDayUnlock(
        int dayIndex)
    {
        if (dayIndex < 0 ||
            dayIndex >= rewards.Count)
        {
            return 0f;
        }

        if (IsDayClaimed(dayIndex))
        {
            return 0f;
        }

        if (IsCycleComplete)
        {
            /*
             * Current cycle complete hai.
             * Sab current cycle days claimed hain.
             */
            return 0f;
        }

        if (dayIndex <
            currentDayIndex)
        {
            return 0f;
        }

        float currentRemaining =
            GetSecondsUntilCurrentReward();

        int dayDifference =
            dayIndex -
            currentDayIndex;

        if (dayDifference <= 0)
        {
            return currentRemaining;
        }

        /*
         * Future cards par estimated sequential countdown:
         *
         * Current day available ho:
         * Day+1 = 24h
         * Day+2 = 48h
         *
         * Current day 5h baad available ho:
         * Day+1 = 29h
         */
        return currentRemaining +
            dayDifference *
            rewardIntervalSeconds;
    }


    public bool TryClaimCurrentReward()
    {
        ProcessExpiredCycleIfNeeded();

        if (!CanClaimCurrentReward)
        {
            BroadcastAll();
            return false;
        }

        ResolveReferences();

        int claimedDayIndex =
            currentDayIndex;

        DailyRewardDefinition reward =
            GetReward(
                claimedDayIndex
            );

        if (reward == null)
        {
            return false;
        }

        GrantReward(
            reward
        );

        currentDayIndex++;

        if (currentDayIndex >= rewards.Count)
        {
            if (restartCycleAfterDay7)
            {
                /*
                 * Day 7 claim hote hi next cycle ko Day 1 par
                 * reset kar dete hain.
                 *
                 * Day 1 immediately claimable NAHI hoga.
                 * Wo normal rewardIntervalSeconds ke baad unlock hoga.
                 *
                 * Example:
                 * Day 7 claim
                 * -> new cycle Day 1
                 * -> 24 hours countdown
                 * -> Day 1 claim available
                 */
                currentDayIndex = 0;

                nextClaimUtcTicks =
                    DateTime.UtcNow.Ticks +
                    SecondsToTicks(
                        rewardIntervalSeconds
                    );
            }
            else
            {
                /*
                 * Repeat disabled ho to cycle complete state mein rahega.
                 */
                currentDayIndex =
                    rewards.Count;

                nextClaimUtcTicks = 0;
            }
        }
        else
        {
            /*
             * Next day ka reward claim ke normal interval ke baad unlock hoga.
             */
            nextClaimUtcTicks =
                DateTime.UtcNow.Ticks +
                SecondsToTicks(
                    rewardIntervalSeconds
                );
        }

        SaveState();

        RewardClaimed?.Invoke(
            claimedDayIndex,
            reward
        );

        BroadcastAll();

        return true;
    }


    private void GrantReward(
        DailyRewardDefinition reward)
    {
        int safeAmount =
            Mathf.Max(
                1,
                reward.amount
            );

        switch (reward.rewardType)
        {
            case DailyRewardType.Coins:
            {
                if (scoreManager != null)
                {
                    scoreManager.AddScore(
                        safeAmount
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "DailyRewardManager: ScoreManager missing hai. Coin reward grant nahi hui.",
                        this
                    );
                }

                break;
            }

            case DailyRewardType.Lives:
            {
                if (lifeManager != null)
                {
                    lifeManager.AddLives(
                        safeAmount
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "DailyRewardManager: LifeManager missing hai. Life reward grant nahi hui.",
                        this
                    );
                }

                break;
            }

            case DailyRewardType.InfiniteBalls:
            {
                if (powerUpInventoryManager != null)
                {
                    powerUpInventoryManager.AddInfiniteBalls(
                        safeAmount
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "DailyRewardManager: PowerUpInventoryManager missing hai. Infinite Balls reward grant nahi hui.",
                        this
                    );
                }

                break;
            }

            case DailyRewardType.PowerCannon:
            {
                if (powerUpInventoryManager != null)
                {
                    powerUpInventoryManager.AddPowerCannon(
                        safeAmount
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "DailyRewardManager: PowerUpInventoryManager missing hai. Power Cannon reward grant nahi hui.",
                        this
                    );
                }

                break;
            }

            case DailyRewardType.Custom:
            {
                GrantCustomBundle(
                    reward,
                    safeAmount
                );

                break;
            }
        }
    }


    private void GrantCustomBundle(
        DailyRewardDefinition reward,
        int safeCustomItemAmount)
    {
        if (reward == null)
        {
            return;
        }

        /*
         * CUSTOM reward ab bundle ho sakta hai:
         *
         * - Coins
         * - Lives
         * - Infinite Balls
         * - Power Cannon
         * - Existing custom item ID
         *
         * In sab ko ek hi daily reward card se
         * individually ya combination mein grant kar sakte hain.
         */

        if (reward.customCoins > 0)
        {
            if (scoreManager != null)
            {
                scoreManager.AddScore(
                    reward.customCoins
                );
            }
            else
            {
                Debug.LogWarning(
                    "DailyRewardManager: ScoreManager missing hai. Custom coins grant nahi huay.",
                    this
                );
            }
        }

        if (reward.customLives > 0)
        {
            if (lifeManager != null)
            {
                lifeManager.AddLives(
                    reward.customLives
                );
            }
            else
            {
                Debug.LogWarning(
                    "DailyRewardManager: LifeManager missing hai. Custom lives grant nahi huin.",
                    this
                );
            }
        }

        if (reward.customInfiniteBalls > 0)
        {
            if (powerUpInventoryManager != null)
            {
                powerUpInventoryManager.AddInfiniteBalls(
                    reward.customInfiniteBalls
                );
            }
            else
            {
                Debug.LogWarning(
                    "DailyRewardManager: PowerUpInventoryManager missing hai. Custom Infinite Balls grant nahi huay.",
                    this
                );
            }
        }

        if (reward.customPowerCannon > 0)
        {
            if (powerUpInventoryManager != null)
            {
                powerUpInventoryManager.AddPowerCannon(
                    reward.customPowerCannon
                );
            }
            else
            {
                Debug.LogWarning(
                    "DailyRewardManager: PowerUpInventoryManager missing hai. Custom Power Cannon grant nahi huay.",
                    this
                );
            }
        }

        /*
         * Existing customRewardId support bhi same rakha hai.
         * Agar ID blank hai to koi custom item grant nahi hoga.
         *
         * Existing 'amount' field Custom item ID ki quantity ke liye
         * use hota rahega.
         */
        if (!string.IsNullOrWhiteSpace(
                reward.customRewardId))
        {
            AddCustomReward(
                reward.customRewardId,
                safeCustomItemAmount
            );
        }

        bool nothingConfigured =
            reward.customCoins <= 0 &&
            reward.customLives <= 0 &&
            reward.customInfiniteBalls <= 0 &&
            reward.customPowerCannon <= 0 &&
            string.IsNullOrWhiteSpace(
                reward.customRewardId);

        if (nothingConfigured)
        {
            Debug.LogWarning(
                "DailyRewardManager: Custom reward selected hai lekin bundle mein koi reward configure nahi kiya.",
                this
            );
        }
    }


    private void AddCustomReward(
        string rewardId,
        int amount)
    {
        if (string.IsNullOrWhiteSpace(
                rewardId) ||
            amount <= 0)
        {
            Debug.LogWarning(
                "DailyRewardManager: Custom reward ID missing hai.",
                this
            );

            return;
        }

        string key =
            GetCustomRewardKey(
                rewardId
            );

        int currentAmount =
            PlayerPrefs.GetInt(
                key,
                0
            );

        PlayerPrefs.SetInt(
            key,
            currentAmount + amount
        );

        PlayerPrefs.Save();
    }


    public int GetCustomRewardCount(
        string rewardId)
    {
        if (string.IsNullOrWhiteSpace(
                rewardId))
        {
            return 0;
        }

        return PlayerPrefs.GetInt(
            GetCustomRewardKey(
                rewardId
            ),
            0
        );
    }


    public bool TryConsumeCustomReward(
        string rewardId,
        int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(
                rewardId) ||
            amount <= 0)
        {
            return false;
        }

        string key =
            GetCustomRewardKey(
                rewardId
            );

        int currentAmount =
            PlayerPrefs.GetInt(
                key,
                0
            );

        if (currentAmount <
            amount)
        {
            return false;
        }

        PlayerPrefs.SetInt(
            key,
            currentAmount - amount
        );

        PlayerPrefs.Save();

        return true;
    }


    private string GetCustomRewardKey(
        string rewardId)
    {
        return CustomRewardPrefix +
            rewardId.Trim();
    }


    private void ProcessExpiredCycleIfNeeded()
    {
        if (!IsCycleComplete ||
            !restartCycleAfterDay7)
        {
            return;
        }

        if (nextClaimUtcTicks <= 0)
        {
            return;
        }

        if (DateTime.UtcNow.Ticks <
            nextClaimUtcTicks)
        {
            return;
        }

        /*
         * 7-day cycle complete + cooldown complete:
         * Day 1 dobara immediately available.
         */
        currentDayIndex = 0;
        nextClaimUtcTicks = 0;

        SaveState();
        StateChanged?.Invoke();
    }


    private static long SecondsToTicks(
        float seconds)
    {
        return TimeSpan
            .FromSeconds(
                Math.Max(
                    1d,
                    seconds
                )
            )
            .Ticks;
    }


    private void BroadcastAll()
    {
        StateChanged?.Invoke();

        TimerChanged?.Invoke(
            GetSecondsUntilCurrentReward()
        );
    }


    private void SaveState()
    {
        PlayerPrefs.SetInt(
            CurrentDayKey,
            currentDayIndex
        );

        PlayerPrefs.SetString(
            NextClaimUtcTicksKey,
            nextClaimUtcTicks.ToString()
        );

        PlayerPrefs.SetInt(
            InitializedKey,
            1
        );

        PlayerPrefs.Save();
    }


    [ContextMenu("DEBUG / Claim Current Reward")]
    private void DebugClaimCurrentReward()
    {
        TryClaimCurrentReward();
    }


    [ContextMenu("DEBUG / Make Current Reward Ready")]
    private void DebugMakeCurrentRewardReady()
    {
        if (IsCycleComplete)
        {
            currentDayIndex = 0;
        }

        nextClaimUtcTicks = 0;

        SaveState();
        BroadcastAll();
    }


    [ContextMenu("DEBUG / Reset Daily Reward Progress")]
    private void DebugResetDailyRewardProgress()
    {
        currentDayIndex = 0;
        nextClaimUtcTicks = 0;

        PlayerPrefs.SetInt(
            InitializedKey,
            1
        );

        SaveState();
        BroadcastAll();
    }
}














