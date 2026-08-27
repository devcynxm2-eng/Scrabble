// using System;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public sealed class DailyRewardUIController : MonoBehaviour
// {
//     [Serializable]
//     public sealed class RewardDayUI
//     {
//         [Header("Root")]
//         public GameObject dayRoot;

//         [Header("Texts")]
//         public TMP_Text dayText;
//         public TMP_Text topStatusText;   // "New Text" / timer / Ready / Claimed
//         public TMP_Text rewardText;

//         [Header("Icons / States")]
//         public GameObject lockIcon;
//         public GameObject tickIcon;
//         public GameObject availableHighlight;
//     }


//     [Header("Daily Reward Panel")]
//     [SerializeField] private GameObject dailyRewardPanel;
//     [SerializeField] private Button closeButton;
//     [SerializeField] private Button claimButton;


//     [Header("Panel Timer")]
//     [SerializeField] private TMP_Text countdownText;
//     [SerializeField] private string defaultCountdownText = "23:59:59";


//     [Header("Claim Button Text")]
//     [SerializeField] private TMP_Text claimButtonText;


//     [Header("7 Day Reward UI")]
//     [SerializeField]
//     private List<RewardDayUI> rewardDays =
//         new List<RewardDayUI>(7);


//     public event Action ClaimPressed;


//     private int currentAvailableDayIndex = -1;
//     private bool canClaimCurrentDay;


//     private void Awake()
//     {
//         if (dailyRewardPanel != null)
//         {
//             dailyRewardPanel.SetActive(false);
//         }

//         ApplyDefaultUI();
//     }


//     private void OnEnable()
//     {
//         UIEventBroker.OnScreenChangeRequested +=
//             HandleScreenChange;

//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseDailyReward
//             );

//             closeButton.onClick.AddListener(
//                 CloseDailyReward
//             );
//         }

//         if (claimButton != null)
//         {
//             claimButton.onClick.RemoveListener(
//                 HandleClaimPressed
//             );

//             claimButton.onClick.AddListener(
//                 HandleClaimPressed
//             );
//         }
//     }


//     private void OnDisable()
//     {
//         UIEventBroker.OnScreenChangeRequested -=
//             HandleScreenChange;

//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseDailyReward
//             );
//         }

//         if (claimButton != null)
//         {
//             claimButton.onClick.RemoveListener(
//                 HandleClaimPressed
//             );
//         }
//     }


//     private void HandleScreenChange(
//         UIScreenType targetScreen)
//     {
//         if (targetScreen == UIScreenType.RewardScreen)
//         {
//             OpenDailyReward();
//             return;
//         }

//         if (targetScreen == UIScreenType.MainMenu)
//         {
//             HideDailyReward();
//         }
//     }


//     public void OpenDailyReward()
//     {
//         if (dailyRewardPanel != null)
//         {
//             dailyRewardPanel.SetActive(true);
//         }
//     }


//     public void CloseDailyReward()
//     {
//         HideDailyReward();

//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );
//     }


//     public void HideDailyReward()
//     {
//         if (dailyRewardPanel != null)
//         {
//             dailyRewardPanel.SetActive(false);
//         }
//     }


//     private void HandleClaimPressed()
//     {
//         if (!canClaimCurrentDay)
//         {
//             return;
//         }

//         ClaimPressed?.Invoke();
//     }


//     private void ApplyDefaultUI()
//     {
//         SetCountdownText(defaultCountdownText);

//         for (int i = 0; i < rewardDays.Count; i++)
//         {
//             RewardDayUI day = rewardDays[i];

//             if (day == null)
//             {
//                 continue;
//             }

//             if (day.dayText != null)
//             {
//                 day.dayText.text = $"Day {i + 1}";
//             }

//             if (day.rewardText != null &&
//                 string.IsNullOrWhiteSpace(day.rewardText.text))
//             {
//                 day.rewardText.text = "x1";
//             }

//             SetDayLocked(i, "Locked");
//         }

//         if (rewardDays.Count > 0)
//         {
//             SetDayAvailable(0, "Ready");
//         }
//         else
//         {
//             RefreshClaimButton(false);
//         }
//     }


//     public void SetCountdownText(string value)
//     {
//         if (countdownText != null)
//         {
//             countdownText.text = value;
//         }
//     }


//     public void SetClaimButtonText(string value)
//     {
//         if (claimButtonText != null)
//         {
//             claimButtonText.text = value;
//         }
//     }


//     public void SetDayRewardText(
//         int dayIndex,
//         string rewardValue)
//     {
//         if (!TryGetDay(dayIndex, out RewardDayUI day))
//         {
//             return;
//         }

//         if (day.rewardText != null)
//         {
//             day.rewardText.text = rewardValue;
//         }
//     }


//     public void SetDayTopText(
//         int dayIndex,
//         string value)
//     {
//         if (!TryGetDay(dayIndex, out RewardDayUI day))
//         {
//             return;
//         }

//         if (day.topStatusText != null)
//         {
//             day.topStatusText.text = value;
//         }
//     }


//     public void SetDayLocked(
//         int dayIndex,
//         string timerOrStatusText)
//     {
//         if (!TryGetDay(dayIndex, out RewardDayUI day))
//         {
//             return;
//         }

//         if (day.topStatusText != null)
//         {
//             day.topStatusText.text = timerOrStatusText;
//         }

//         if (day.lockIcon != null)
//         {
//             day.lockIcon.SetActive(true);
//         }

//         if (day.tickIcon != null)
//         {
//             day.tickIcon.SetActive(false);
//         }

//         if (day.availableHighlight != null)
//         {
//             day.availableHighlight.SetActive(false);
//         }

//         if (currentAvailableDayIndex == dayIndex)
//         {
//             currentAvailableDayIndex = -1;
//             canClaimCurrentDay = false;
//             RefreshClaimButton(false);
//         }
//     }


//     public void SetDayAvailable(
//         int dayIndex,
//         string statusText = "Ready")
//     {
//         if (!TryGetDay(dayIndex, out RewardDayUI day))
//         {
//             return;
//         }

//         currentAvailableDayIndex = dayIndex;
//         canClaimCurrentDay = true;

//         if (day.topStatusText != null)
//         {
//             day.topStatusText.text = statusText;
//         }

//         if (day.lockIcon != null)
//         {
//             day.lockIcon.SetActive(false);
//         }

//         if (day.tickIcon != null)
//         {
//             day.tickIcon.SetActive(false);
//         }

//         if (day.availableHighlight != null)
//         {
//             day.availableHighlight.SetActive(true);
//         }

//         RefreshClaimButton(true);
//     }


//     public void SetDayClaimed(
//         int dayIndex,
//         string statusText = "Claimed")
//     {
//         if (!TryGetDay(dayIndex, out RewardDayUI day))
//         {
//             return;
//         }

//         if (day.topStatusText != null)
//         {
//             day.topStatusText.text = statusText;
//         }

//         if (day.lockIcon != null)
//         {
//             day.lockIcon.SetActive(false);
//         }

//         if (day.tickIcon != null)
//         {
//             day.tickIcon.SetActive(true);
//         }

//         if (day.availableHighlight != null)
//         {
//             day.availableHighlight.SetActive(false);
//         }

//         if (currentAvailableDayIndex == dayIndex)
//         {
//             currentAvailableDayIndex = -1;
//             canClaimCurrentDay = false;
//             RefreshClaimButton(false);
//         }
//     }


//     private void RefreshClaimButton(
//         bool isInteractable)
//     {
//         if (claimButton != null)
//         {
//             claimButton.interactable = isInteractable;
//         }

//         if (claimButtonText != null)
//         {
//             claimButtonText.text =
//                 isInteractable
//                     ? "CLAIM"
//                     : "COME BACK LATER";
//         }
//     }


//     private bool TryGetDay(
//         int dayIndex,
//         out RewardDayUI day)
//     {
//         day = null;

//         if (dayIndex < 0 ||
//             dayIndex >= rewardDays.Count)
//         {
//             return false;
//         }

//         day = rewardDays[dayIndex];
//         return day != null;
//     }


//     [ContextMenu("UI TEST / Day 1 Available")]
//     private void DebugDay1Available()
//     {
//         SetDayAvailable(0, "Ready");
//     }


//     [ContextMenu("UI TEST / Day 1 Claimed")]
//     private void DebugDay1Claimed()
//     {
//         SetDayClaimed(0, "Claimed");
//     }


//     [ContextMenu("UI TEST / Day 2 Locked With Timer")]
//     private void DebugDay2Locked()
//     {
//         SetDayLocked(1, "04:59:59");
//     }
// }











// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public sealed class DailyRewardUIController : MonoBehaviour
// {
//     [System.Serializable]
//     public sealed class RewardDayUI
//     {
//         [Header("Root")]
//         public GameObject dayRoot;

//         [Header("Texts")]
//         public TMP_Text dayText;

//         [Tooltip(
//             "Reference image mein 'New Text' wali field. " +
//             "Isi par countdown / READY / CLAIMED show hoga."
//         )]
//         public TMP_Text topStatusText;

//         public TMP_Text rewardText;

//         [Header("Icons / States")]
//         public GameObject lockIcon;

//         [Tooltip(
//             "Day unlock/claim state mein lock ki jagah ye tick icon show hoga."
//         )]
//         public GameObject tickIcon;

//         [Tooltip(
//             "Optional highlight for currently claimable day."
//         )]
//         public GameObject availableHighlight;
//     }


//     [Header("Daily Reward Panel")]
//     [SerializeField] private GameObject dailyRewardPanel;
//     [SerializeField] private Button closeButton;
//     [SerializeField] private Button claimButton;
//     [SerializeField] private TMP_Text claimButtonText;


//     [Header("Optional Main Countdown")]
//     [SerializeField] private TMP_Text countdownText;


//     [Header("7 Day Reward UI")]
//     [SerializeField]
//     private List<RewardDayUI> rewardDays =
//         new List<RewardDayUI>(7);


//     [Header("Reference")]
//     [SerializeField]
//     private DailyRewardManager dailyRewardManager;


//     private void Awake()
//     {
//         if (dailyRewardPanel != null)
//         {
//             dailyRewardPanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveManager();

//         UIEventBroker.OnScreenChangeRequested +=
//             HandleScreenChange;

//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseDailyReward
//             );

//             closeButton.onClick.AddListener(
//                 CloseDailyReward
//             );
//         }

//         if (claimButton != null)
//         {
//             claimButton.onClick.RemoveListener(
//                 ClaimCurrentReward
//             );

//             claimButton.onClick.AddListener(
//                 ClaimCurrentReward
//             );
//         }

//         SubscribeToManager();
//         RefreshAll();
//     }


//     private void OnDisable()
//     {
//         UIEventBroker.OnScreenChangeRequested -=
//             HandleScreenChange;

//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseDailyReward
//             );
//         }

//         if (claimButton != null)
//         {
//             claimButton.onClick.RemoveListener(
//                 ClaimCurrentReward
//             );
//         }

//         UnsubscribeFromManager();
//     }


//     private void ResolveManager()
//     {
//         if (dailyRewardManager != null)
//         {
//             return;
//         }

//         if (DailyRewardManager.Instance != null)
//         {
//             dailyRewardManager =
//                 DailyRewardManager.Instance;

//             return;
//         }

//         dailyRewardManager =
//             FindFirstObjectByType<DailyRewardManager>(
//                 FindObjectsInactive.Include
//             );
//     }


//     private void SubscribeToManager()
//     {
//         if (dailyRewardManager == null)
//         {
//             return;
//         }

//         dailyRewardManager.StateChanged -=
//             HandleStateChanged;

//         dailyRewardManager.StateChanged +=
//             HandleStateChanged;

//         dailyRewardManager.TimerChanged -=
//             HandleTimerChanged;

//         dailyRewardManager.TimerChanged +=
//             HandleTimerChanged;
//     }


//     private void UnsubscribeFromManager()
//     {
//         if (dailyRewardManager == null)
//         {
//             return;
//         }

//         dailyRewardManager.StateChanged -=
//             HandleStateChanged;

//         dailyRewardManager.TimerChanged -=
//             HandleTimerChanged;
//     }


//     private void HandleScreenChange(
//         UIScreenType targetScreen)
//     {
//         if (targetScreen ==
//             UIScreenType.RewardScreen)
//         {
//             OpenDailyReward();
//             return;
//         }

//         if (targetScreen ==
//             UIScreenType.MainMenu)
//         {
//             HideDailyReward();
//         }
//     }


//     public void OpenDailyReward()
//     {
//         ResolveManager();

//         if (dailyRewardPanel != null)
//         {
//             dailyRewardPanel.SetActive(true);
//         }

//         RefreshAll();
//     }


//     public void CloseDailyReward()
//     {
//         HideDailyReward();

//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );
//     }


//     public void HideDailyReward()
//     {
//         if (dailyRewardPanel != null)
//         {
//             dailyRewardPanel.SetActive(false);
//         }
//     }


//     private void ClaimCurrentReward()
//     {
//         ResolveManager();

//         if (dailyRewardManager == null)
//         {
//             Debug.LogWarning(
//                 "DailyRewardUIController: DailyRewardManager missing hai.",
//                 this
//             );

//             return;
//         }

//         dailyRewardManager
//             .TryClaimCurrentReward();

//         RefreshAll();
//     }


//     private void HandleStateChanged()
//     {
//         RefreshAll();
//     }


//     private void HandleTimerChanged(
//         float remainingSeconds)
//     {
//         RefreshTimerTexts();
//         RefreshClaimButton();
//     }


//     private void RefreshAll()
//     {
//         if (dailyRewardManager == null)
//         {
//             return;
//         }

//         RefreshRewardTexts();
//         RefreshDayStates();
//         RefreshTimerTexts();
//         RefreshClaimButton();
//     }


//     private void RefreshRewardTexts()
//     {
//         int count =
//             Mathf.Min(
//                 rewardDays.Count,
//                 dailyRewardManager.RewardCount
//             );

//         for (int i = 0;
//              i < count;
//              i++)
//         {
//             RewardDayUI dayUI =
//                 rewardDays[i];

//             DailyRewardDefinition reward =
//                 dailyRewardManager.GetReward(i);

//             if (dayUI == null ||
//                 reward == null)
//             {
//                 continue;
//             }

//             if (dayUI.dayText != null)
//             {
//                 dayUI.dayText.text =
//                     $"Day {i + 1}";
//             }

//             if (dayUI.rewardText != null)
//             {
//                 dayUI.rewardText.text =
//                     reward.rewardDisplayText;
//             }
//         }
//     }


//     private void RefreshDayStates()
//     {
//         int count =
//             Mathf.Min(
//                 rewardDays.Count,
//                 dailyRewardManager.RewardCount
//             );

//         for (int i = 0;
//              i < count;
//              i++)
//         {
//             RewardDayUI dayUI =
//                 rewardDays[i];

//             if (dayUI == null)
//             {
//                 continue;
//             }

//             bool isClaimed =
//                 dailyRewardManager
//                     .IsDayClaimed(i);

//             bool isCurrent =
//                 dailyRewardManager
//                     .IsCurrentDay(i);

//             bool isAvailable =
//                 isCurrent &&
//                 dailyRewardManager
//                     .CanClaimCurrentReward;

//             /*
//              * User requirement:
//              * Unlock hone par lock icon ki jagah tick icon.
//              *
//              * Claimed day:
//              *   Lock OFF
//              *   Tick ON
//              *
//              * Current available day:
//              *   Lock OFF
//              *   Tick ON
//              *   Optional highlight ON
//              *
//              * Locked future day:
//              *   Lock ON
//              *   Tick OFF
//              */
//             if (dayUI.lockIcon != null)
//             {
//                 dayUI.lockIcon.SetActive(
//                     !isClaimed &&
//                     !isAvailable
//                 );
//             }

//             if (dayUI.tickIcon != null)
//             {
//                 dayUI.tickIcon.SetActive(
//                     isClaimed ||
//                     isAvailable
//                 );
//             }

//             if (dayUI.availableHighlight != null)
//             {
//                 dayUI.availableHighlight.SetActive(
//                     isAvailable
//                 );
//             }
//         }
//     }


//     private void RefreshTimerTexts()
//     {
//         if (dailyRewardManager == null)
//         {
//             return;
//         }

//         int count =
//             Mathf.Min(
//                 rewardDays.Count,
//                 dailyRewardManager.RewardCount
//             );

//         for (int i = 0;
//              i < count;
//              i++)
//         {
//             RewardDayUI dayUI =
//                 rewardDays[i];

//             if (dayUI == null ||
//                 dayUI.topStatusText == null)
//             {
//                 continue;
//             }

//             if (dailyRewardManager
//                 .IsDayClaimed(i))
//             {
//                 dayUI.topStatusText.text =
//                     "CLAIMED";

//                 continue;
//             }

//             bool isCurrent =
//                 dailyRewardManager
//                     .IsCurrentDay(i);

//             if (isCurrent &&
//                 dailyRewardManager
//                     .CanClaimCurrentReward)
//             {
//                 dayUI.topStatusText.text =
//                     "READY";

//                 continue;
//             }

//             float remaining =
//                 dailyRewardManager
//                     .GetSecondsUntilDayUnlock(i);

//             dayUI.topStatusText.text =
//                 FormatTime(
//                     remaining
//                 );
//         }


//         if (countdownText == null)
//         {
//             return;
//         }

//         if (dailyRewardManager
//             .IsCycleComplete)
//         {
//             countdownText.text =
//                 "7 DAYS COMPLETE";

//             return;
//         }

//         if (dailyRewardManager
//             .CanClaimCurrentReward)
//         {
//             countdownText.text =
//                 "READY";

//             return;
//         }

//         countdownText.text =
//             FormatTime(
//                 dailyRewardManager
//                     .GetSecondsUntilCurrentReward()
//             );
//     }


//     private void RefreshClaimButton()
//     {
//         if (dailyRewardManager == null)
//         {
//             return;
//         }

//         bool canClaim =
//             dailyRewardManager
//                 .CanClaimCurrentReward;

//         if (claimButton != null)
//         {
//             claimButton.interactable =
//                 canClaim;
//         }

//         if (claimButtonText == null)
//         {
//             return;
//         }

//         if (dailyRewardManager
//             .IsCycleComplete)
//         {
//             claimButtonText.text =
//                 "COMPLETE";
//         }
//         else if (canClaim)
//         {
//             claimButtonText.text =
//                 "CLAIM";
//         }
//         else
//         {
//             claimButtonText.text =
//                 "COME BACK LATER";
//         }
//     }


//     private static string FormatTime(
//         float seconds)
//     {
//         int totalSeconds =
//             Mathf.Max(
//                 0,
//                 Mathf.CeilToInt(
//                     seconds
//                 )
//             );

//         int hours =
//             totalSeconds / 3600;

//         int minutes =
//             (totalSeconds % 3600) / 60;

//         int secs =
//             totalSeconds % 60;

//         /*
//          * 24h se zyada ho to hours 24 se upar continue karein:
//          * 47:59:59
//          * 71:59:59
//          *
//          * Is se har card ki "New Text" field actual countdown rahegi.
//          */
//         return
//             $"{hours:00}:{minutes:00}:{secs:00}";
//     }
// }






using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DailyRewardUIController : MonoBehaviour
{
    [System.Serializable]
    public sealed class RewardDayUI
    {
        [Header("Root")]
        public GameObject dayRoot;

        [Header("Texts")]
        public TMP_Text dayText;

        [Tooltip(
            "Reference image mein 'New Text' wali field. " +
            "Sirf NEXT unlock hone wale day par countdown show hoga."
        )]
        public TMP_Text topStatusText;

        public TMP_Text rewardText;

        [Header("Icons / States")]
        public GameObject lockIcon;

        [Tooltip(
            "Unlocked / claimed state mein lock ki jagah ye tick icon show hoga."
        )]
        public GameObject tickIcon;

        [Tooltip(
            "Optional highlight for currently claimable day."
        )]
        public GameObject availableHighlight;
    }


    [Header("Daily Reward Panel")]
    [SerializeField] private GameObject dailyRewardPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text claimButtonText;


    [Header("Optional Main Countdown")]
    [SerializeField] private TMP_Text countdownText;


    [Header("7 Day Reward UI")]
    [SerializeField]
    private List<RewardDayUI> rewardDays =
        new List<RewardDayUI>(7);


    [Header("Reference")]
    [SerializeField]
    private DailyRewardManager dailyRewardManager;

    [SerializeField]
    private CoinRewardAnimator rewardAnimator;


    private void Awake()
    {
        if (dailyRewardPanel != null)
        {
            UITransition.HideImmediate(dailyRewardPanel);
        }
    }


    private void OnEnable()
    {
        ResolveManager();

        UIEventBroker.OnScreenChangeRequested +=
            HandleScreenChange;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                CloseDailyReward
            );

            closeButton.onClick.AddListener(
                CloseDailyReward
            );
        }

        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(
                ClaimCurrentReward
            );

            claimButton.onClick.AddListener(
                ClaimCurrentReward
            );
        }

        SubscribeToManager();
        RefreshAll();
    }


    private void OnDisable()
    {
        UIEventBroker.OnScreenChangeRequested -=
            HandleScreenChange;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                CloseDailyReward
            );
        }

        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(
                ClaimCurrentReward
            );
        }

        UnsubscribeFromManager();
    }


    private void ResolveManager()
    {
        if (rewardAnimator == null)
        {
            rewardAnimator =
                FindFirstObjectByType<CoinRewardAnimator>(
                    FindObjectsInactive.Include
                );
        }

        if (dailyRewardManager != null)
        {
            return;
        }

        if (DailyRewardManager.Instance != null)
        {
            dailyRewardManager =
                DailyRewardManager.Instance;

            return;
        }

        dailyRewardManager =
            FindFirstObjectByType<DailyRewardManager>(
                FindObjectsInactive.Include
            );
    }


    private void SubscribeToManager()
    {
        if (dailyRewardManager == null)
        {
            return;
        }

        dailyRewardManager.StateChanged -=
            HandleStateChanged;

        dailyRewardManager.StateChanged +=
            HandleStateChanged;

        dailyRewardManager.TimerChanged -=
            HandleTimerChanged;

        dailyRewardManager.TimerChanged +=
            HandleTimerChanged;

        dailyRewardManager.RewardClaimed -=
            HandleRewardClaimed;

        dailyRewardManager.RewardClaimed +=
            HandleRewardClaimed;
    }


    private void UnsubscribeFromManager()
    {
        if (dailyRewardManager == null)
        {
            return;
        }

        dailyRewardManager.StateChanged -=
            HandleStateChanged;

        dailyRewardManager.TimerChanged -=
            HandleTimerChanged;

        dailyRewardManager.RewardClaimed -=
            HandleRewardClaimed;
    }


    private void HandleScreenChange(
        UIScreenType targetScreen)
    {
        if (targetScreen ==
            UIScreenType.RewardScreen)
        {
            OpenDailyReward();
            return;
        }

        if (targetScreen ==
            UIScreenType.MainMenu)
        {
            HideDailyReward();
        }
    }


    public void OpenDailyReward()
    {
        ResolveManager();

        if (dailyRewardPanel != null)
        {
            UITransition.Show(dailyRewardPanel);
        }

        RefreshAll();
    }


    public void CloseDailyReward()
    {
        HideDailyReward();

        UIEventBroker.RequestScreen(
            UIScreenType.MainMenu
        );
    }


    public void HideDailyReward()
    {
        if (dailyRewardPanel != null)
        {
            UITransition.Hide(dailyRewardPanel);
        }
    }


    private void ClaimCurrentReward()
    {
        ResolveManager();

        if (dailyRewardManager == null)
        {
            Debug.LogWarning(
                "DailyRewardUIController: DailyRewardManager missing hai.",
                this
            );

            return;
        }

        dailyRewardManager
            .TryClaimCurrentReward();

        RefreshAll();
    }


    private void HandleRewardClaimed(
        int claimedDayIndex,
        DailyRewardDefinition reward)
    {
        if (reward == null)
        {
            return;
        }

        ResolveManager();

        if (rewardAnimator == null)
        {
            Debug.LogWarning(
                "DailyRewardUIController: CoinRewardAnimator missing hai.",
                this
            );

            return;
        }

        List<RewardVisualType> visualTypes =
            new List<RewardVisualType>();

        switch (reward.rewardType)
        {
            case DailyRewardType.Coins:
                AddVisualType(
                    visualTypes,
                    RewardVisualType.Coin
                );
                break;

            case DailyRewardType.Lives:
                AddVisualType(
                    visualTypes,
                    RewardVisualType.Life
                );
                break;

            case DailyRewardType.InfiniteBalls:
                AddVisualType(
                    visualTypes,
                    RewardVisualType.CanonBall
                );
                break;

            case DailyRewardType.PowerCannon:
                AddVisualType(
                    visualTypes,
                    RewardVisualType.Rocket
                );
                break;

            case DailyRewardType.Custom:
                AddCustomRewardVisuals(
                    visualTypes,
                    reward
                );
                break;
        }

        if (visualTypes.Count > 0)
        {
            if (reward.rewardType == DailyRewardType.Custom)
            {
                rewardAnimator.PlayRewardGroup(
                    visualTypes.ToArray()
                );
            }
            else
            {
                rewardAnimator.PlayRewardSequence(
                    visualTypes.ToArray()
                );
            }
        }
    }


    private static void AddCustomRewardVisuals(
        List<RewardVisualType> visualTypes,
        DailyRewardDefinition reward)
    {
        if (reward.customCoins > 0)
        {
            AddVisualType(
                visualTypes,
                RewardVisualType.Coin
            );
        }

        if (reward.customLives > 0)
        {
            AddVisualType(
                visualTypes,
                RewardVisualType.Life
            );
        }

        if (reward.customInfiniteBalls > 0)
        {
            AddVisualType(
                visualTypes,
                RewardVisualType.CanonBall
            );
        }

        if (reward.customPowerCannon > 0)
        {
            AddVisualType(
                visualTypes,
                RewardVisualType.Rocket
            );
        }

        if (visualTypes.Count > 0)
        {
            return;
        }

        string customId =
            reward.customRewardId == null
                ? string.Empty
                : reward.customRewardId
                    .Trim()
                    .ToLowerInvariant();

        if (customId.Contains("coin"))
        {
            AddVisualType(
                visualTypes,
                RewardVisualType.Coin
            );
        }
        else if (customId.Contains("life"))
        {
            AddVisualType(
                visualTypes,
                RewardVisualType.Life
            );
        }
        else if (customId.Contains("rocket"))
        {
            AddVisualType(
                visualTypes,
                RewardVisualType.Rocket
            );
        }
        else if (customId.Contains("canon") ||
                 customId.Contains("cannon") ||
                 customId.Contains("ball"))
        {
            AddVisualType(
                visualTypes,
                RewardVisualType.CanonBall
            );
        }
    }


    private static void AddVisualType(
        List<RewardVisualType> visualTypes,
        RewardVisualType visualType)
    {
        if (!visualTypes.Contains(visualType))
        {
            visualTypes.Add(visualType);
        }
    }


    private void HandleStateChanged()
    {
        RefreshAll();
    }


    private void HandleTimerChanged(
        float remainingSeconds)
    {
        RefreshTimerTexts();
        RefreshClaimButton();
    }


    private void RefreshAll()
    {
        if (dailyRewardManager == null)
        {
            return;
        }

        RefreshRewardTexts();
        RefreshDayStates();
        RefreshTimerTexts();
        RefreshClaimButton();
    }


    private void RefreshRewardTexts()
    {
        int count =
            Mathf.Min(
                rewardDays.Count,
                dailyRewardManager.RewardCount
            );

        for (int i = 0;
             i < count;
             i++)
        {
            RewardDayUI dayUI =
                rewardDays[i];

            DailyRewardDefinition reward =
                dailyRewardManager.GetReward(i);

            if (dayUI == null ||
                reward == null)
            {
                continue;
            }

            if (dayUI.dayText != null)
            {
                dayUI.dayText.text =
                    $"Day {i + 1}";
            }

            if (dayUI.rewardText != null)
            {
                dayUI.rewardText.text =
                    reward.rewardDisplayText;
            }
        }
    }


    private void RefreshDayStates()
    {
        int count =
            Mathf.Min(
                rewardDays.Count,
                dailyRewardManager.RewardCount
            );

        for (int i = 0;
             i < count;
             i++)
        {
            RewardDayUI dayUI =
                rewardDays[i];

            if (dayUI == null)
            {
                continue;
            }

            bool isClaimed =
                dailyRewardManager
                    .IsDayClaimed(i);

            bool isCurrent =
                dailyRewardManager
                    .IsCurrentDay(i);

            bool isAvailable =
                isCurrent &&
                dailyRewardManager
                    .CanClaimCurrentReward;

            if (dayUI.lockIcon != null)
            {
                dayUI.lockIcon.SetActive(
                    !isClaimed &&
                    !isAvailable
                );
            }

            if (dayUI.tickIcon != null)
            {
                dayUI.tickIcon.SetActive(
                    isClaimed ||
                    isAvailable
                );
            }

            if (dayUI.availableHighlight != null)
            {
                dayUI.availableHighlight.SetActive(
                    isAvailable
                );
            }
        }
    }


    private void RefreshTimerTexts()
    {
        if (dailyRewardManager == null)
        {
            return;
        }

        int count =
            Mathf.Min(
                rewardDays.Count,
                dailyRewardManager.RewardCount
            );

        int nextUnlockDayIndex = -1;

        /*
         * Sirf current day timer show karega jab woh abhi locked ho.
         * Agar current day already READY hai to kisi card par countdown nahi hoga.
         */
        if (!dailyRewardManager.IsCycleComplete &&
            !dailyRewardManager.CanClaimCurrentReward)
        {
            nextUnlockDayIndex =
                dailyRewardManager.CurrentDayIndex;
        }

        for (int i = 0;
             i < count;
             i++)
        {
            RewardDayUI dayUI =
                rewardDays[i];

            if (dayUI == null ||
                dayUI.topStatusText == null)
            {
                continue;
            }

            bool isClaimed =
                dailyRewardManager
                    .IsDayClaimed(i);

            bool isCurrent =
                dailyRewardManager
                    .IsCurrentDay(i);

            bool isAvailable =
                isCurrent &&
                dailyRewardManager
                    .CanClaimCurrentReward;

            if (isClaimed)
            {
                dayUI.topStatusText.gameObject.SetActive(true);
                dayUI.topStatusText.text = "CLAIMED";
                continue;
            }

            if (isAvailable)
            {
                dayUI.topStatusText.gameObject.SetActive(true);
                dayUI.topStatusText.text = "READY";
                continue;
            }

            if (i == nextUnlockDayIndex)
            {
                dayUI.topStatusText.gameObject.SetActive(true);

                dayUI.topStatusText.text =
                    FormatTime(
                        dailyRewardManager
                            .GetSecondsUntilCurrentReward()
                    );

                continue;
            }

            /*
             * Future locked rewards:
             * timer/status text completely hidden.
             */
            dayUI.topStatusText.text = "";
            dayUI.topStatusText.gameObject.SetActive(false);
        }


        if (countdownText == null)
        {
            return;
        }

        if (dailyRewardManager
            .IsCycleComplete)
        {
            countdownText.text =
                "7 DAYS COMPLETE";

            return;
        }

        if (dailyRewardManager
            .CanClaimCurrentReward)
        {
            countdownText.text =
                "READY";

            return;
        }

        countdownText.text =
            FormatTime(
                dailyRewardManager
                    .GetSecondsUntilCurrentReward()
            );
    }


    private void RefreshClaimButton()
    {
        if (dailyRewardManager == null)
        {
            return;
        }

        bool canClaim =
            dailyRewardManager
                .CanClaimCurrentReward;

        if (claimButton != null)
        {
            claimButton.interactable =
                canClaim;
        }

        if (claimButtonText == null)
        {
            return;
        }

        if (dailyRewardManager
            .IsCycleComplete)
        {
            claimButtonText.text =
                "COMPLETE";
        }
        else if (canClaim)
        {
            claimButtonText.text =
                "CLAIM";
        }
        else
        {
            claimButtonText.text =
                "COME BACK LATER";
        }
    }


    private static string FormatTime(
        float seconds)
    {
        int totalSeconds =
            Mathf.Max(
                0,
                Mathf.CeilToInt(
                    seconds
                )
            );

        int hours =
            totalSeconds / 3600;

        int minutes =
            (totalSeconds % 3600) / 60;

        int secs =
            totalSeconds % 60;

        return
            $"{hours:00}:{minutes:00}:{secs:00}";
    }
}
