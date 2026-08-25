using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public sealed class GoogleAdsManager : MonoBehaviour
{
    public static GoogleAdsManager Instance { get; private set; }


    [Header("General")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Tooltip(
        "Unity objects ko ad callbacks se safely update karne ke liye."
    )]
    [SerializeField] private bool raiseAdEventsOnUnityMainThread = true;


    [Header("Startup Behaviour")]
    [SerializeField] private bool loadBannerOnStart = true;
    [SerializeField] private bool showBannerOnStart = true;

    [SerializeField] private bool loadInterstitialOnStart = true;
    [SerializeField] private bool loadRewardedOnStart = true;
    [SerializeField] private bool loadAppOpenOnStart = true;

    [Tooltip(
        "Testing ke liye ON rakh sakte hain. Production mein app-open placement " +
        "ko user experience ke mutabiq configure karein."
    )]
    [SerializeField] private bool showAppOpenAfterFirstLoad = true;

    [SerializeField] private bool showAppOpenWhenReturningToForeground = true;


    [Header("Banner")]
    [SerializeField] private AdPosition bannerPosition = AdPosition.Bottom;


    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private AppOpenAd appOpenAd;

    private DateTime appOpenLoadTimeUtc;

    private bool isInitialized;
    private bool isInitializing;
    private bool isFullScreenAdShowing;

    private Action pendingInterstitialClosedCallback;
    private Action pendingRewardedEarnedCallback;
    private Action pendingRewardedClosedCallback;


    public bool IsInitialized =>
        isInitialized;

    public bool IsInterstitialReady =>
        interstitialAd != null &&
        interstitialAd.CanShowAd();

    public bool IsRewardedReady =>
        rewardedAd != null &&
        rewardedAd.CanShowAd();

    public bool IsAppOpenReady =>
        appOpenAd != null &&
        appOpenAd.CanShowAd() &&
        DateTime.UtcNow -
        appOpenLoadTimeUtc <
        TimeSpan.FromHours(4);


    public event Action AdsInitialized;

    public event Action BannerLoaded;
    public event Action<string> BannerFailedToLoad;

    public event Action InterstitialLoaded;
    public event Action<string> InterstitialFailedToLoad;

    public event Action RewardedLoaded;
    public event Action<string> RewardedFailedToLoad;

    public event Action AppOpenLoaded;
    public event Action<string> AppOpenFailedToLoad;


#if UNITY_ANDROID
    private const string BannerAdUnitId =
        "ca-app-pub-3940256099942544/6300978111";

    private const string InterstitialAdUnitId =
        "ca-app-pub-3940256099942544/1033173712";

    private const string RewardedAdUnitId =
        "ca-app-pub-3940256099942544/5224354917";

    private const string AppOpenAdUnitId =
        "ca-app-pub-3940256099942544/9257395921";

#elif UNITY_IOS
    private const string BannerAdUnitId =
        "ca-app-pub-3940256099942544/2934735716";

    private const string InterstitialAdUnitId =
        "ca-app-pub-3940256099942544/4411468910";

    private const string RewardedAdUnitId =
        "ca-app-pub-3940256099942544/1712485313";

    private const string AppOpenAdUnitId =
        "ca-app-pub-3940256099942544/5575463023";

#else
    private const string BannerAdUnitId = "unused";
    private const string InterstitialAdUnitId = "unused";
    private const string RewardedAdUnitId = "unused";
    private const string AppOpenAdUnitId = "unused";
#endif


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

        AppStateEventNotifier.AppStateChanged +=
            HandleAppStateChanged;
    }


    private void Start()
    {
        Initialize();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            AppStateEventNotifier.AppStateChanged -=
                HandleAppStateChanged;

            DestroyBanner();
            DestroyInterstitial();
            DestroyRewarded();
            DestroyAppOpen();

            Instance = null;
        }
    }


    public void Initialize()
    {
        if (isInitialized ||
            isInitializing)
        {
            return;
        }

#if !UNITY_ANDROID && !UNITY_IOS
        Debug.Log(
            "GoogleAdsManager: Ads Android/iOS build par run hongi.",
            this
        );
        return;
#else
        isInitializing = true;

        MobileAds.RaiseAdEventsOnUnityMainThread =
            raiseAdEventsOnUnityMainThread;

        MobileAds.Initialize(
            initializationStatus =>
            {
                isInitializing = false;
                isInitialized = true;

                Debug.Log(
                    "GoogleAdsManager: Google Mobile Ads initialized.",
                    this
                );

                AdsInitialized?.Invoke();

                LoadStartupAds();
            }
        );
#endif
    }


    private void LoadStartupAds()
    {
        if (loadBannerOnStart)
        {
            LoadBanner(
                showBannerOnStart
            );
        }

        if (loadInterstitialOnStart)
        {
            LoadInterstitial();
        }

        if (loadRewardedOnStart)
        {
            LoadRewarded();
        }

        if (loadAppOpenOnStart)
        {
            LoadAppOpen(
                showAppOpenAfterFirstLoad
            );
        }
    }


    // =========================================================
    // BANNER
    // =========================================================

    public void LoadBanner(
        bool showWhenLoaded = true)
    {
        if (!EnsureInitialized())
        {
            return;
        }

        DestroyBanner();

        bannerView =
            new BannerView(
                BannerAdUnitId,
                AdSize.Banner,
                bannerPosition
            );

        bannerView.OnBannerAdLoaded +=
            () =>
            {
                Debug.Log(
                    "GoogleAdsManager: Banner loaded.",
                    this
                );

                BannerLoaded?.Invoke();

                if (showWhenLoaded)
                {
                    bannerView?.Show();
                }
                else
                {
                    bannerView?.Hide();
                }
            };

        bannerView.OnBannerAdLoadFailed +=
            error =>
            {
                string message =
                    error != null
                        ? error.ToString()
                        : "Unknown banner load error.";

                Debug.LogWarning(
                    "GoogleAdsManager: Banner failed to load: " +
                    message,
                    this
                );

                BannerFailedToLoad?.Invoke(
                    message
                );
            };

        bannerView.LoadAd(
            new AdRequest()
        );
    }


    public void ShowBanner()
    {
        if (bannerView == null)
        {
            LoadBanner(true);
            return;
        }

        bannerView.Show();
    }


    public void HideBanner()
    {
        bannerView?.Hide();
    }


    public void DestroyBanner()
    {
        if (bannerView == null)
        {
            return;
        }

        bannerView.Destroy();
        bannerView = null;
    }


    // =========================================================
    // INTERSTITIAL
    // =========================================================

    public void LoadInterstitial()
    {
        if (!EnsureInitialized())
        {
            return;
        }

        DestroyInterstitial();

        InterstitialAd.Load(
            InterstitialAdUnitId,
            new AdRequest(),
            (
                InterstitialAd ad,
                LoadAdError error
            ) =>
            {
                if (error != null ||
                    ad == null)
                {
                    string message =
                        error != null
                            ? error.ToString()
                            : "Interstitial ad returned null.";

                    Debug.LogWarning(
                        "GoogleAdsManager: Interstitial failed to load: " +
                        message,
                        this
                    );

                    InterstitialFailedToLoad?.Invoke(
                        message
                    );

                    return;
                }

                interstitialAd = ad;

                RegisterInterstitialEvents(
                    interstitialAd
                );

                Debug.Log(
                    "GoogleAdsManager: Interstitial loaded.",
                    this
                );

                InterstitialLoaded?.Invoke();
            }
        );
    }


    public bool ShowInterstitial(
        Action onClosed = null)
    {
        if (isFullScreenAdShowing ||
            !IsInterstitialReady)
        {
            Debug.Log(
                "GoogleAdsManager: Interstitial not ready.",
                this
            );

            return false;
        }

        pendingInterstitialClosedCallback =
            onClosed;

        isFullScreenAdShowing = true;

        interstitialAd.Show();

        return true;
    }


    private void RegisterInterstitialEvents(
        InterstitialAd ad)
    {
        ad.OnAdFullScreenContentOpened +=
            () =>
            {
                isFullScreenAdShowing = true;
            };

        ad.OnAdFullScreenContentClosed +=
            () =>
            {
                isFullScreenAdShowing = false;

                Action callback =
                    pendingInterstitialClosedCallback;

                pendingInterstitialClosedCallback =
                    null;

                DestroyInterstitial();
                LoadInterstitial();

                callback?.Invoke();
            };

        ad.OnAdFullScreenContentFailed +=
            error =>
            {
                isFullScreenAdShowing = false;

                Action callback =
                    pendingInterstitialClosedCallback;

                pendingInterstitialClosedCallback =
                    null;

                Debug.LogWarning(
                    "GoogleAdsManager: Interstitial failed to show: " +
                    error,
                    this
                );

                DestroyInterstitial();
                LoadInterstitial();

                callback?.Invoke();
            };
    }


    private void DestroyInterstitial()
    {
        if (interstitialAd == null)
        {
            return;
        }

        interstitialAd.Destroy();
        interstitialAd = null;
    }


    // =========================================================
    // REWARDED
    // =========================================================

    public void LoadRewarded()
    {
        if (!EnsureInitialized())
        {
            return;
        }

        DestroyRewarded();

        RewardedAd.Load(
            RewardedAdUnitId,
            new AdRequest(),
            (
                RewardedAd ad,
                LoadAdError error
            ) =>
            {
                if (error != null ||
                    ad == null)
                {
                    string message =
                        error != null
                            ? error.ToString()
                            : "Rewarded ad returned null.";

                    Debug.LogWarning(
                        "GoogleAdsManager: Rewarded failed to load: " +
                        message,
                        this
                    );

                    RewardedFailedToLoad?.Invoke(
                        message
                    );

                    return;
                }

                rewardedAd = ad;

                RegisterRewardedEvents(
                    rewardedAd
                );

                Debug.Log(
                    "GoogleAdsManager: Rewarded loaded.",
                    this
                );

                RewardedLoaded?.Invoke();
            }
        );
    }


    public bool ShowRewarded(
        Action onRewardEarned,
        Action onClosed = null)
    {
        if (isFullScreenAdShowing ||
            !IsRewardedReady)
        {
            Debug.Log(
                "GoogleAdsManager: Rewarded ad not ready.",
                this
            );

            return false;
        }

        pendingRewardedEarnedCallback =
            onRewardEarned;

        pendingRewardedClosedCallback =
            onClosed;

        isFullScreenAdShowing = true;

        rewardedAd.Show(
            reward =>
            {
                /*
                 * Reward sirf Google ke reward callback par grant hogi.
                 * Ad close hone par automatically reward nahi milegi.
                 */
                Action rewardCallback =
                    pendingRewardedEarnedCallback;

                pendingRewardedEarnedCallback =
                    null;

                rewardCallback?.Invoke();
            }
        );

        return true;
    }


    private void RegisterRewardedEvents(
        RewardedAd ad)
    {
        ad.OnAdFullScreenContentOpened +=
            () =>
            {
                isFullScreenAdShowing = true;
            };

        ad.OnAdFullScreenContentClosed +=
            () =>
            {
                isFullScreenAdShowing = false;

                pendingRewardedEarnedCallback =
                    null;

                Action closedCallback =
                    pendingRewardedClosedCallback;

                pendingRewardedClosedCallback =
                    null;

                DestroyRewarded();
                LoadRewarded();

                closedCallback?.Invoke();
            };

        ad.OnAdFullScreenContentFailed +=
            error =>
            {
                isFullScreenAdShowing = false;

                pendingRewardedEarnedCallback =
                    null;

                Action closedCallback =
                    pendingRewardedClosedCallback;

                pendingRewardedClosedCallback =
                    null;

                Debug.LogWarning(
                    "GoogleAdsManager: Rewarded failed to show: " +
                    error,
                    this
                );

                DestroyRewarded();
                LoadRewarded();

                closedCallback?.Invoke();
            };
    }


    private void DestroyRewarded()
    {
        if (rewardedAd == null)
        {
            return;
        }

        rewardedAd.Destroy();
        rewardedAd = null;
    }


    // =========================================================
    // APP OPEN
    // =========================================================

    public void LoadAppOpen(
        bool showAfterLoad = false)
    {
        if (!EnsureInitialized())
        {
            return;
        }

        DestroyAppOpen();

        AppOpenAd.Load(
            AppOpenAdUnitId,
            new AdRequest(),
            (
                AppOpenAd ad,
                LoadAdError error
            ) =>
            {
                if (error != null ||
                    ad == null)
                {
                    string message =
                        error != null
                            ? error.ToString()
                            : "App open ad returned null.";

                    Debug.LogWarning(
                        "GoogleAdsManager: App Open failed to load: " +
                        message,
                        this
                    );

                    AppOpenFailedToLoad?.Invoke(
                        message
                    );

                    return;
                }

                appOpenAd = ad;
                appOpenLoadTimeUtc =
                    DateTime.UtcNow;

                RegisterAppOpenEvents(
                    appOpenAd
                );

                Debug.Log(
                    "GoogleAdsManager: App Open loaded.",
                    this
                );

                AppOpenLoaded?.Invoke();

                if (showAfterLoad)
                {
                    ShowAppOpenIfAvailable();
                }
            }
        );
    }


    public bool ShowAppOpenIfAvailable()
    {
        if (isFullScreenAdShowing ||
            !IsAppOpenReady)
        {
            return false;
        }

        isFullScreenAdShowing = true;

        appOpenAd.Show();

        return true;
    }


    private void RegisterAppOpenEvents(
        AppOpenAd ad)
    {
        ad.OnAdFullScreenContentOpened +=
            () =>
            {
                isFullScreenAdShowing = true;
            };

        ad.OnAdFullScreenContentClosed +=
            () =>
            {
                isFullScreenAdShowing = false;

                DestroyAppOpen();
                LoadAppOpen(false);
            };

        ad.OnAdFullScreenContentFailed +=
            error =>
            {
                isFullScreenAdShowing = false;

                Debug.LogWarning(
                    "GoogleAdsManager: App Open failed to show: " +
                    error,
                    this
                );

                DestroyAppOpen();
                LoadAppOpen(false);
            };
    }


    private void DestroyAppOpen()
    {
        if (appOpenAd == null)
        {
            return;
        }

        appOpenAd.Destroy();
        appOpenAd = null;
    }


    private void HandleAppStateChanged(
        AppState state)
    {
        if (!showAppOpenWhenReturningToForeground)
        {
            return;
        }

        if (state !=
            AppState.Foreground)
        {
            return;
        }

        if (isFullScreenAdShowing)
        {
            return;
        }

        ShowAppOpenIfAvailable();
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private bool EnsureInitialized()
    {
        if (isInitialized)
        {
            return true;
        }

        Initialize();

        Debug.Log(
            "GoogleAdsManager: SDK abhi initialize ho rahi hai.",
            this
        );

        return false;
    }


    [ContextMenu("TEST / Show Banner")]
    private void DebugShowBanner()
    {
        ShowBanner();
    }


    [ContextMenu("TEST / Hide Banner")]
    private void DebugHideBanner()
    {
        HideBanner();
    }


    [ContextMenu("TEST / Show Interstitial")]
    private void DebugShowInterstitial()
    {
        ShowInterstitial();
    }


    [ContextMenu("TEST / Show Rewarded")]
    private void DebugShowRewarded()
    {
        ShowRewarded(
            () =>
            {
                Debug.Log(
                    "GoogleAdsManager: TEST Reward earned.",
                    this
                );
            }
        );
    }


    [ContextMenu("TEST / Show App Open")]
    private void DebugShowAppOpen()
    {
        ShowAppOpenIfAvailable();
    }
}
