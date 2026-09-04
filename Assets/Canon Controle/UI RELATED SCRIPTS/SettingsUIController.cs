// using UnityEngine;
// using UnityEngine.UI;

// public sealed class SettingsUIController : MonoBehaviour
// {
//     [Header("Settings Popup")]
//     [SerializeField] private GameObject settingsPanel;
//     [SerializeField] private Button closeButton;


//     [Header("Setting Toggles")]
//     [SerializeField] private Toggle soundToggle;
//     [SerializeField] private Toggle musicToggle;
//     [SerializeField] private Toggle vibrationToggle;


//     [Header("Managers")]
//     [SerializeField] private AudioManager audioManager;
//     [SerializeField] private VibrationManager vibrationManager;


//     private void Awake()
//     {
//         if (settingsPanel != null)
//         {
//             settingsPanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();

//         UIEventBroker.OnScreenChangeRequested +=
//             HandleScreenChange;

//         AddListeners();
//         RefreshToggleStates();
//     }


//     private void OnDisable()
//     {
//         UIEventBroker.OnScreenChangeRequested -=
//             HandleScreenChange;

//         RemoveListeners();
//     }


//     private void ResolveReferences()
//     {
//         if (audioManager == null)
//         {
//             audioManager =
//                 FindFirstObjectByType<AudioManager>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (vibrationManager == null)
//         {
//             vibrationManager =
//                 FindFirstObjectByType<VibrationManager>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     private void AddListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );

//             closeButton.onClick.AddListener(
//                 CloseSettings
//             );
//         }

//         if (soundToggle != null)
//         {
//             soundToggle.onValueChanged.RemoveListener(
//                 HandleSoundToggleChanged
//             );

//             soundToggle.onValueChanged.AddListener(
//                 HandleSoundToggleChanged
//             );
//         }

//         if (musicToggle != null)
//         {
//             musicToggle.onValueChanged.RemoveListener(
//                 HandleMusicToggleChanged
//             );

//             musicToggle.onValueChanged.AddListener(
//                 HandleMusicToggleChanged
//             );
//         }

//         if (vibrationToggle != null)
//         {
//             vibrationToggle.onValueChanged.RemoveListener(
//                 HandleVibrationToggleChanged
//             );

//             vibrationToggle.onValueChanged.AddListener(
//                 HandleVibrationToggleChanged
//             );
//         }
//     }


//     private void RemoveListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );
//         }

//         if (soundToggle != null)
//         {
//             soundToggle.onValueChanged.RemoveListener(
//                 HandleSoundToggleChanged
//             );
//         }

//         if (musicToggle != null)
//         {
//             musicToggle.onValueChanged.RemoveListener(
//                 HandleMusicToggleChanged
//             );
//         }

//         if (vibrationToggle != null)
//         {
//             vibrationToggle.onValueChanged.RemoveListener(
//                 HandleVibrationToggleChanged
//             );
//         }
//     }


//     private void HandleScreenChange(
//         UIScreenType targetScreen)
//     {
//         if (targetScreen ==
//             UIScreenType.SettingScreen)
//         {
//             OpenSettings();
//             return;
//         }

//         if (targetScreen ==
//             UIScreenType.MainMenu)
//         {
//             HideSettings();
//         }
//     }


//     public void OpenSettings()
//     {
//         ResolveReferences();
//         RefreshToggleStates();

//         if (settingsPanel != null)
//         {
//             settingsPanel.SetActive(true);
//         }
//     }


//     public void CloseSettings()
//     {
//         HideSettings();

//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );
//     }


//     public void HideSettings()
//     {
//         if (settingsPanel != null)
//         {
//             settingsPanel.SetActive(false);
//         }
//     }


//     private void HandleSoundToggleChanged(
//         bool isOn)
//     {
//         if (audioManager != null)
//         {
//             audioManager.SetSoundEnabled(
//                 isOn
//             );
//         }
//         else
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );
//         }
//     }


//     private void HandleMusicToggleChanged(
//         bool isOn)
//     {
//         if (audioManager != null)
//         {
//             audioManager.SetMusicEnabled(
//                 isOn
//             );
//         }
//         else
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );
//         }
//     }


//     private void HandleVibrationToggleChanged(
//         bool isOn)
//     {
//         if (vibrationManager != null)
//         {
//             vibrationManager.SetVibrationEnabled(
//                 isOn
//             );
//         }
//         else
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: VibrationManager missing hai.",
//                 this
//             );
//         }
//     }


//     private void RefreshToggleStates()
//     {
//         if (audioManager != null)
//         {
//             if (soundToggle != null)
//             {
//                 soundToggle.SetIsOnWithoutNotify(
//                     audioManager.IsSoundEnabled
//                 );
//             }

//             if (musicToggle != null)
//             {
//                 musicToggle.SetIsOnWithoutNotify(
//                     audioManager.IsMusicEnabled
//                 );
//             }
//         }

//         if (vibrationManager != null &&
//             vibrationToggle != null)
//         {
//             vibrationToggle.SetIsOnWithoutNotify(
//                 vibrationManager.IsVibrationEnabled
//             );
//         }
//     }
// }










// using UnityEngine;
// using UnityEngine.UI;

// public sealed class SettingsUIController : MonoBehaviour
// {
//     [Header("Settings Popup")]
//     [SerializeField] private GameObject settingsPanel;
//     [SerializeField] private Button closeButton;


//     [Header("Sound Button")]
//     [SerializeField] private Button soundButton;
//     [SerializeField] private GameObject soundOnUI;
//     [SerializeField] private GameObject soundOffUI;


//     [Header("Music Button")]
//     [SerializeField] private Button musicButton;
//     [SerializeField] private GameObject musicOnUI;
//     [SerializeField] private GameObject musicOffUI;


//     [Header("Vibration Button")]
//     [SerializeField] private Button vibrationButton;
//     [SerializeField] private GameObject vibrationOnUI;
//     [SerializeField] private GameObject vibrationOffUI;


//     [Header("Managers")]
//     [SerializeField] private AudioManager audioManager;
//     [SerializeField] private VibrationManager vibrationManager;


//     private void Awake()
//     {
//         if (settingsPanel != null)
//         {
//             UITransition.HideImmediate(settingsPanel);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();

//         UIEventBroker.OnScreenChangeRequested +=
//             HandleScreenChange;

//         AddListeners();
//         RefreshButtonStates();
//     }


//     private void OnDisable()
//     {
//         UIEventBroker.OnScreenChangeRequested -=
//             HandleScreenChange;

//         RemoveListeners();
//     }


//     private void ResolveReferences()
//     {
//         if (audioManager == null)
//         {
//             audioManager =
//                 FindFirstObjectByType<AudioManager>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (vibrationManager == null)
//         {
//             vibrationManager =
//                 FindFirstObjectByType<VibrationManager>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     private void AddListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );

//             closeButton.onClick.AddListener(
//                 CloseSettings
//             );
//         }


//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );

//             soundButton.onClick.AddListener(
//                 ToggleSound
//             );
//         }


//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );

//             musicButton.onClick.AddListener(
//                 ToggleMusic
//             );
//         }


//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );

//             vibrationButton.onClick.AddListener(
//                 ToggleVibration
//             );
//         }
//     }


//     private void RemoveListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );
//         }


//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );
//         }


//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );
//         }


//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );
//         }
//     }


//     private void HandleScreenChange(
//         UIScreenType targetScreen)
//     {
//         if (targetScreen ==
//             UIScreenType.SettingScreen)
//         {
//             OpenSettings();
//             return;
//         }

//         if (targetScreen ==
//             UIScreenType.MainMenu)
//         {
//             HideSettings();
//         }
//     }


//     public void OpenSettings()
//     {
//         ResolveReferences();
//         RefreshButtonStates();

//         if (settingsPanel != null)
//         {
//             UITransition.Show(settingsPanel);
//         }
//     }


//     public void CloseSettings()
//     {
//         HideSettings();

//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );
//     }


//     public void HideSettings()
//     {
//         if (settingsPanel != null)
//         {
//             UITransition.Hide(settingsPanel);
//         }
//     }


//     private void ToggleSound()
//     {
//         ResolveReferences();

//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }

//         bool newState =
//             !audioManager.IsSoundEnabled;

//         audioManager.SetSoundEnabled(
//             newState
//         );

//         RefreshSoundUI();
//     }


//     private void ToggleMusic()
//     {
//         ResolveReferences();

//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }

//         bool newState =
//             !audioManager.IsMusicEnabled;

//         audioManager.SetMusicEnabled(
//             newState
//         );

//         RefreshMusicUI();
//     }


//     private void ToggleVibration()
//     {
//         ResolveReferences();

//         if (vibrationManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: VibrationManager missing hai.",
//                 this
//             );

//             return;
//         }

//         bool newState =
//             !vibrationManager.IsVibrationEnabled;

//         vibrationManager.SetVibrationEnabled(
//             newState
//         );

//         RefreshVibrationUI();
//     }


//     private void RefreshButtonStates()
//     {
//         RefreshSoundUI();
//         RefreshMusicUI();
//         RefreshVibrationUI();
//     }


//     private void RefreshSoundUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsSoundEnabled;

//         SetStateUI(
//             soundOnUI,
//             soundOffUI,
//             isOn
//         );
//     }


//     private void RefreshMusicUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsMusicEnabled;

//         SetStateUI(
//             musicOnUI,
//             musicOffUI,
//             isOn
//         );
//     }


//     private void RefreshVibrationUI()
//     {
//         bool isOn =
//             vibrationManager != null &&
//             vibrationManager.IsVibrationEnabled;

//         SetStateUI(
//             vibrationOnUI,
//             vibrationOffUI,
//             isOn
//         );
//     }


//     private static void SetStateUI(
//         GameObject onUI,
//         GameObject offUI,
//         bool isOn)
//     {
//         if (onUI != null)
//         {
//             onUI.SetActive(
//                 isOn
//             );
//         }

//         if (offUI != null)
//         {
//             offUI.SetActive(
//                 !isOn
//             );
//         }
//     }
// }










// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;

// public sealed class SettingsUIController : MonoBehaviour
// {
//     [Header("Settings Popup")]
//     [SerializeField] private GameObject settingsPanel;
//     [SerializeField] private Button closeButton;


//     // =========================================================
//     // SOUND
//     // =========================================================

//     [Header("Sound Button")]
//     [SerializeField] private Button soundButton;
//     [SerializeField] private GameObject soundOnUI;
//     [SerializeField] private GameObject soundOffUI;


//     [Header("Sound Animation")]
//     [Tooltip("Speaker icon jo scale animation karega.")]
//     [SerializeField] private RectTransform speakerIcon;

//     [Tooltip("Pehli sound bar jo right side move karegi.")]
//     [SerializeField] private RectTransform soundBar1;

//     [Tooltip("Dusri sound bar jo right side move karegi.")]
//     [SerializeField] private RectTransform soundBar2;

//     [Tooltip("Bars kitne pixels right side move karengi.")]
//     [SerializeField, Min(0f)]
//     private float soundBarMoveDistance = 12f;

//     [Tooltip("Bars ke movement ka duration.")]
//     [SerializeField, Min(0.01f)]
//     private float soundBarMoveDuration = 0.18f;

//     [Tooltip("Speaker icon kitna scale hoga.")]
//     [SerializeField, Min(1f)]
//     private float speakerScaleMultiplier = 1.12f;

//     [Tooltip("Speaker scale animation duration.")]
//     [SerializeField, Min(0.01f)]
//     private float speakerScaleDuration = 0.18f;

//     [Tooltip("Sound animation kitni baar repeat hogi.")]
//     [SerializeField, Range(1, 5)]
//     private int soundAnimationLoops = 2;

//     [Tooltip("Sound animation repetitions ke darmiyan delay.")]
//     [SerializeField, Min(0f)]
//     private float soundAnimationLoopDelay = 0.04f;

//     [SerializeField]
//     private Ease soundBarEase = Ease.OutQuad;

//     [SerializeField]
//     private Ease speakerEase = Ease.OutBack;


//     // =========================================================
//     // MUSIC
//     // =========================================================

//     [Header("Music Button")]
//     [SerializeField] private Button musicButton;
//     [SerializeField] private GameObject musicOnUI;
//     [SerializeField] private GameObject musicOffUI;


//     [Header("Music Animation")]
//     [Tooltip(
//         "Main music icon jo scale/bounce animation karega."
//     )]
//     [SerializeField] private RectTransform musicIcon;

//     [Tooltip(
//         "5 ya 6 music notes/icons assign karein. " +
//         "Ye notes icon ke around se upward move karengi."
//     )]
//     [SerializeField] private RectTransform[] musicNotes;

//     [Tooltip(
//         "Music notes kitne pixels upward move karengi."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteMoveDistance = 70f;

//     [Tooltip(
//         "Music notes ko left/right kitna random movement milega."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteHorizontalDrift = 25f;

//     [Tooltip(
//         "Music note movement duration."
//     )]
//     [SerializeField, Min(0.05f)]
//     private float musicNoteMoveDuration = 0.65f;

//     [Tooltip(
//         "Music notes fade out hongi ya nahi."
//     )]
//     [SerializeField]
//     private bool musicNotesFadeOut = true;

//     [Tooltip(
//         "Music notes kitni der baad fade hona shuru hongi."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float musicNoteFadeStart = 0.45f;

//     [Tooltip(
//         "Music icon kitna scale hoga."
//     )]
//     [SerializeField, Min(1f)]
//     private float musicIconScaleMultiplier = 1.12f;

//     [Tooltip(
//         "Music icon scale animation duration."
//     )]
//     [SerializeField, Min(0.01f)]
//     private float musicIconScaleDuration = 0.18f;

//     [Tooltip(
//         "Music animation kitni baar repeat hogi."
//     )]
//     [SerializeField, Range(1, 5)]
//     private int musicAnimationLoops = 2;

//     [Tooltip(
//         "Music animation repetitions ke darmiyan delay."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicAnimationLoopDelay = 0.08f;

//     [SerializeField]
//     private Ease musicNoteEase = Ease.OutQuad;

//     [SerializeField]
//     private Ease musicIconEase = Ease.OutBack;


//     // =========================================================
//     // VIBRATION
//     // =========================================================

//     [Header("Vibration Button")]
//     [SerializeField] private Button vibrationButton;
//     [SerializeField] private GameObject vibrationOnUI;
//     [SerializeField] private GameObject vibrationOffUI;


//     // =========================================================
//     // MANAGERS
//     // =========================================================

//     [Header("Managers")]
//     [SerializeField] private AudioManager audioManager;
//     [SerializeField] private VibrationManager vibrationManager;


//     // =========================================================
//     // INTERNAL ANIMATION DATA
//     // =========================================================

//     private Vector2 soundBar1OriginalPosition;
//     private Vector2 soundBar2OriginalPosition;
//     private Vector3 speakerOriginalScale;

//     private Vector3 musicIconOriginalScale;

//     private Vector2[] musicNoteOriginalPositions;
//     private Vector3[] musicNoteOriginalScales;

//     private CanvasGroup[] musicNoteCanvasGroups;

//     private bool soundAnimationInitialized;
//     private bool musicAnimationInitialized;


//     // =========================================================
//     // AWAKE
//     // =========================================================

//     private void Awake()
//     {
//         if (settingsPanel != null)
//         {
//             UITransition.HideImmediate(
//                 settingsPanel
//             );
//         }

//         CacheSoundAnimationPositions();
//         CacheMusicAnimationData();
//     }


//     // =========================================================
//     // ENABLE / DISABLE
//     // =========================================================

//     private void OnEnable()
//     {
//         ResolveReferences();

//         UIEventBroker.OnScreenChangeRequested +=
//             HandleScreenChange;

//         AddListeners();

//         RefreshButtonStates();
//     }


//     private void OnDisable()
//     {
//         UIEventBroker.OnScreenChangeRequested -=
//             HandleScreenChange;

//         RemoveListeners();

//         KillSoundAnimation();
//         KillMusicAnimation();
//     }


//     // =========================================================
//     // REFERENCES
//     // =========================================================

//     private void ResolveReferences()
//     {
//         if (audioManager == null)
//         {
//             audioManager =
//                 FindFirstObjectByType<AudioManager>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (vibrationManager == null)
//         {
//             vibrationManager =
//                 FindFirstObjectByType<VibrationManager>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     // =========================================================
//     // LISTENERS
//     // =========================================================

//     private void AddListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );

//             closeButton.onClick.AddListener(
//                 CloseSettings
//             );
//         }

//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );

//             soundButton.onClick.AddListener(
//                 ToggleSound
//             );
//         }

//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );

//             musicButton.onClick.AddListener(
//                 ToggleMusic
//             );
//         }

//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );

//             vibrationButton.onClick.AddListener(
//                 ToggleVibration
//             );
//         }
//     }


//     private void RemoveListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );
//         }

//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );
//         }

//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );
//         }

//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );
//         }
//     }


//     // =========================================================
//     // SCREEN CHANGE
//     // =========================================================

//     private void HandleScreenChange(
//         UIScreenType targetScreen
//     )
//     {
//         if (targetScreen ==
//             UIScreenType.SettingScreen)
//         {
//             OpenSettings();
//             return;
//         }

//         if (targetScreen ==
//             UIScreenType.MainMenu)
//         {
//             HideSettings();
//         }
//     }


//     // =========================================================
//     // OPEN SETTINGS
//     // =========================================================

//     public void OpenSettings()
//     {
//         ResolveReferences();

//         RefreshButtonStates();

//         if (settingsPanel != null)
//         {
//             UITransition.Show(
//                 settingsPanel
//             );
//         }

//         /*
//          * Settings open hote hi dono animations.
//          */
//         PlaySoundAnimation();
//         PlayMusicAnimation();
//     }


//     // =========================================================
//     // CLOSE SETTINGS
//     // =========================================================

//     public void CloseSettings()
//     {
//         KillSoundAnimation();
//         KillMusicAnimation();

//         HideSettings();

//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );
//     }


//     public void HideSettings()
//     {
//         KillSoundAnimation();
//         KillMusicAnimation();

//         if (settingsPanel != null)
//         {
//             UITransition.Hide(
//                 settingsPanel
//             );
//         }
//     }


//     // =========================================================
//     // SOUND TOGGLE
//     // =========================================================

//     private void ToggleSound()
//     {
//         ResolveReferences();

//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }

//         bool newState =
//             !audioManager.IsSoundEnabled;

//         audioManager.SetSoundEnabled(
//             newState
//         );

//         RefreshSoundUI();

//         PlaySoundAnimation();
//     }


//     // =========================================================
//     // MUSIC TOGGLE
//     // =========================================================

//     private void ToggleMusic()
//     {
//         ResolveReferences();

//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }

//         bool newState =
//             !audioManager.IsMusicEnabled;

//         audioManager.SetMusicEnabled(
//             newState
//         );

//         RefreshMusicUI();

//         /*
//          * Music toggle press karne par music animation.
//          */
//         PlayMusicAnimation();
//     }


//     // =========================================================
//     // VIBRATION TOGGLE
//     // =========================================================

//     private void ToggleVibration()
//     {
//         ResolveReferences();

//         if (vibrationManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: VibrationManager missing hai.",
//                 this
//             );

//             return;
//         }

//         bool newState =
//             !vibrationManager.IsVibrationEnabled;

//         vibrationManager.SetVibrationEnabled(
//             newState
//         );

//         RefreshVibrationUI();
//     }


//     // =========================================================
//     // REFRESH STATES
//     // =========================================================

//     private void RefreshButtonStates()
//     {
//         RefreshSoundUI();
//         RefreshMusicUI();
//         RefreshVibrationUI();
//     }


//     private void RefreshSoundUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsSoundEnabled;

//         SetStateUI(
//             soundOnUI,
//             soundOffUI,
//             isOn
//         );
//     }


//     private void RefreshMusicUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsMusicEnabled;

//         SetStateUI(
//             musicOnUI,
//             musicOffUI,
//             isOn
//         );
//     }


//     private void RefreshVibrationUI()
//     {
//         bool isOn =
//             vibrationManager != null &&
//             vibrationManager.IsVibrationEnabled;

//         SetStateUI(
//             vibrationOnUI,
//             vibrationOffUI,
//             isOn
//         );
//     }


//     private static void SetStateUI(
//         GameObject onUI,
//         GameObject offUI,
//         bool isOn
//     )
//     {
//         if (onUI != null)
//         {
//             onUI.SetActive(
//                 isOn
//             );
//         }

//         if (offUI != null)
//         {
//             offUI.SetActive(
//                 !isOn
//             );
//         }
//     }


//     // =========================================================
//     // SOUND ANIMATION
//     // =========================================================

//     private void CacheSoundAnimationPositions()
//     {
//         if (soundBar1 != null)
//         {
//             soundBar1OriginalPosition =
//                 soundBar1.anchoredPosition;
//         }

//         if (soundBar2 != null)
//         {
//             soundBar2OriginalPosition =
//                 soundBar2.anchoredPosition;
//         }

//         if (speakerIcon != null)
//         {
//             speakerOriginalScale =
//                 speakerIcon.localScale;
//         }

//         soundAnimationInitialized = true;
//     }


//     public void PlaySoundAnimation()
//     {
//         if (!soundAnimationInitialized)
//         {
//             CacheSoundAnimationPositions();
//         }

//         KillSoundAnimation();

//         Sequence animation =
//             DOTween.Sequence();


//         /*
//          * BAR 1
//          */

//         if (soundBar1 != null)
//         {
//             soundBar1.anchoredPosition =
//                 soundBar1OriginalPosition;

//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 Vector2 rightPosition =
//                     soundBar1OriginalPosition +
//                     Vector2.right *
//                     soundBarMoveDistance;

//                 animation.Append(
//                     soundBar1
//                         .DOAnchorPos(
//                             rightPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );

//                 animation.Append(
//                     soundBar1
//                         .DOAnchorPos(
//                             soundBar1OriginalPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );

//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     animation.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }
//         }


//         /*
//          * BAR 2
//          */

//         if (soundBar2 != null)
//         {
//             soundBar2.anchoredPosition =
//                 soundBar2OriginalPosition;

//             Sequence bar2Sequence =
//                 DOTween.Sequence();

//             bar2Sequence.AppendInterval(
//                 soundBarMoveDuration * 0.35f
//             );

//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 Vector2 rightPosition =
//                     soundBar2OriginalPosition +
//                     Vector2.right *
//                     soundBarMoveDistance;

//                 bar2Sequence.Append(
//                     soundBar2
//                         .DOAnchorPos(
//                             rightPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );

//                 bar2Sequence.Append(
//                     soundBar2
//                         .DOAnchorPos(
//                             soundBar2OriginalPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );

//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     bar2Sequence.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }

//             animation.Insert(
//                 0f,
//                 bar2Sequence
//             );
//         }


//         /*
//          * SPEAKER ICON
//          */

//         if (speakerIcon != null)
//         {
//             speakerIcon.localScale =
//                 speakerOriginalScale;

//             Vector3 enlargedScale =
//                 speakerOriginalScale *
//                 speakerScaleMultiplier;

//             Sequence speakerSequence =
//                 DOTween.Sequence();

//             speakerSequence.Append(
//                 speakerIcon
//                     .DOScale(
//                         enlargedScale,
//                         speakerScaleDuration
//                     )
//                     .SetEase(
//                         speakerEase
//                     )
//             );

//             speakerSequence.Append(
//                 speakerIcon
//                     .DOScale(
//                         speakerOriginalScale,
//                         speakerScaleDuration
//                     )
//                     .SetEase(
//                         Ease.OutQuad
//                     )
//             );

//             speakerSequence.SetLoops(
//                 soundAnimationLoops,
//                 LoopType.Restart
//             );

//             animation.Insert(
//                 0f,
//                 speakerSequence
//             );
//         }

//         animation.Play();
//     }


//     // =========================================================
//     // MUSIC ANIMATION DATA
//     // =========================================================

//     private void CacheMusicAnimationData()
//     {
//         if (musicIcon != null)
//         {
//             musicIconOriginalScale =
//                 musicIcon.localScale;
//         }

//         if (musicNotes == null ||
//             musicNotes.Length == 0)
//         {
//             musicAnimationInitialized = true;
//             return;
//         }

//         musicNoteOriginalPositions =
//             new Vector2[musicNotes.Length];

//         musicNoteOriginalScales =
//             new Vector3[musicNotes.Length];

//         musicNoteCanvasGroups =
//             new CanvasGroup[musicNotes.Length];


//         for (int i = 0;
//              i < musicNotes.Length;
//              i++)
//         {
//             RectTransform note =
//                 musicNotes[i];

//             if (note == null)
//             {
//                 continue;
//             }

//             musicNoteOriginalPositions[i] =
//                 note.anchoredPosition;

//             musicNoteOriginalScales[i] =
//                 note.localScale;


//             CanvasGroup canvasGroup =
//                 note.GetComponent<CanvasGroup>();

//             if (canvasGroup == null)
//             {
//                 canvasGroup =
//                     note.gameObject.AddComponent<CanvasGroup>();
//             }

//             musicNoteCanvasGroups[i] =
//                 canvasGroup;
//         }

//         musicAnimationInitialized = true;
//     }


//     // =========================================================
//     // MUSIC ANIMATION
//     // =========================================================

//     /// <summary>
//     /// Music animation:
//     ///
//     /// 5-6 notes upward move karengi.
//     /// Har note ko slightly different horizontal drift milega.
//     /// Notes fade out hongi.
//     /// Main music icon scale/bounce karega.
//     /// </summary>
//     public void PlayMusicAnimation()
//     {
//         if (!musicAnimationInitialized)
//         {
//             CacheMusicAnimationData();
//         }

//         KillMusicAnimation();


//         Sequence musicSequence =
//             DOTween.Sequence();


//         /*
//          * ---------------------------------------------------------
//          * MUSIC NOTES
//          * ---------------------------------------------------------
//          */

//         if (musicNotes != null &&
//             musicNotes.Length > 0)
//         {
//             for (int i = 0;
//                  i < musicNotes.Length;
//                  i++)
//             {
//                 RectTransform note =
//                     musicNotes[i];

//                 if (note == null)
//                 {
//                     continue;
//                 }


//                 /*
//                  * Original state par reset.
//                  */

//                 note.anchoredPosition =
//                     musicNoteOriginalPositions[i];

//                 note.localScale =
//                     musicNoteOriginalScales[i];


//                 CanvasGroup canvasGroup =
//                     musicNoteCanvasGroups[i];


//                 if (canvasGroup != null)
//                 {
//                     canvasGroup.alpha = 1f;
//                 }


//                 /*
//                  * Har note ko different horizontal direction.
//                  */

//                 float normalizedIndex =
//                     musicNotes.Length > 1
//                         ? (float)i /
//                           (musicNotes.Length - 1)
//                         : 0.5f;


//                 float randomDrift =
//                     Random.Range(
//                         -musicNoteHorizontalDrift,
//                         musicNoteHorizontalDrift
//                     );


//                 /*
//                  * Thora spread based movement.
//                  */

//                 float spreadDirection =
//                     Mathf.Lerp(
//                         -1f,
//                         1f,
//                         normalizedIndex
//                     );


//                 randomDrift +=
//                     spreadDirection *
//                     musicNoteHorizontalDrift *
//                     0.35f;


//                 Vector2 targetPosition =
//                     musicNoteOriginalPositions[i] +
//                     new Vector2(
//                         randomDrift,
//                         musicNoteMoveDistance
//                     );


//                 /*
//                  * Har note ko slight different delay.
//                  */

//                 float noteDelay =
//                     i * 0.035f;


//                 /*
//                  * Movement tween.
//                  */

//                 Tween moveTween =
//                     note
//                         .DOAnchorPos(
//                             targetPosition,
//                             musicNoteMoveDuration
//                         )
//                         .SetEase(
//                             musicNoteEase
//                         );


//                 musicSequence.Insert(
//                     noteDelay,
//                     moveTween
//                 );


//                 /*
//                  * Fade out.
//                  */

//                 if (musicNotesFadeOut &&
//                     canvasGroup != null)
//                 {
//                     float fadeDelay =
//                         noteDelay +
//                         musicNoteMoveDuration *
//                         musicNoteFadeStart;


//                     float fadeDuration =
//                         musicNoteMoveDuration *
//                         (1f -
//                          musicNoteFadeStart);


//                     Tween fadeTween =
//                         canvasGroup
//                             .DOFade(
//                                 0f,
//                                 Mathf.Max(
//                                     0.05f,
//                                     fadeDuration
//                                 )
//                             );


//                     musicSequence.Insert(
//                         fadeDelay,
//                         fadeTween
//                     );
//                 }
//             }
//         }


//         /*
//          * ---------------------------------------------------------
//          * MUSIC ICON
//          * ---------------------------------------------------------
//          */

//         if (musicIcon != null)
//         {
//             musicIcon.localScale =
//                 musicIconOriginalScale;


//             Vector3 enlargedScale =
//                 musicIconOriginalScale *
//                 musicIconScaleMultiplier;


//             Sequence iconSequence =
//                 DOTween.Sequence();


//             for (int i = 0;
//                  i < musicAnimationLoops;
//                  i++)
//             {
//                 iconSequence.Append(
//                     musicIcon
//                         .DOScale(
//                             enlargedScale,
//                             musicIconScaleDuration
//                         )
//                         .SetEase(
//                             musicIconEase
//                         )
//                 );


//                 iconSequence.Append(
//                     musicIcon
//                         .DOScale(
//                             musicIconOriginalScale,
//                             musicIconScaleDuration
//                         )
//                         .SetEase(
//                             Ease.OutQuad
//                         )
//                 );


//                 if (musicAnimationLoopDelay > 0f)
//                 {
//                     iconSequence.AppendInterval(
//                         musicAnimationLoopDelay
//                     );
//                 }
//             }


//             musicSequence.Insert(
//                 0f,
//                 iconSequence
//             );
//         }


//         /*
//          * ---------------------------------------------------------
//          * COMPLETE
//          * ---------------------------------------------------------
//          */

//         musicSequence.OnComplete(
//             ResetMusicAnimationObjects
//         );


//         musicSequence.Play();
//     }


//     // =========================================================
//     // RESET MUSIC ANIMATION
//     // =========================================================

//     private void ResetMusicAnimationObjects()
//     {
//         if (musicIcon != null)
//         {
//             musicIcon.localScale =
//                 musicIconOriginalScale;
//         }


//         if (musicNotes == null)
//         {
//             return;
//         }


//         for (int i = 0;
//              i < musicNotes.Length;
//              i++)
//         {
//             RectTransform note =
//                 musicNotes[i];

//             if (note == null)
//             {
//                 continue;
//             }


//             note.anchoredPosition =
//                 musicNoteOriginalPositions[i];

//             note.localScale =
//                 musicNoteOriginalScales[i];


//             CanvasGroup canvasGroup =
//                 musicNoteCanvasGroups[i];

//             if (canvasGroup != null)
//             {
//                 canvasGroup.alpha = 1f;
//             }
//         }
//     }


//     // =========================================================
//     // KILL SOUND ANIMATION
//     // =========================================================

//     private void KillSoundAnimation()
//     {
//         if (soundBar1 != null)
//         {
//             soundBar1.DOKill();

//             if (soundAnimationInitialized)
//             {
//                 soundBar1.anchoredPosition =
//                     soundBar1OriginalPosition;
//             }
//         }


//         if (soundBar2 != null)
//         {
//             soundBar2.DOKill();

//             if (soundAnimationInitialized)
//             {
//                 soundBar2.anchoredPosition =
//                     soundBar2OriginalPosition;
//             }
//         }


//         if (speakerIcon != null)
//         {
//             speakerIcon.DOKill();

//             if (soundAnimationInitialized)
//             {
//                 speakerIcon.localScale =
//                     speakerOriginalScale;
//             }
//         }
//     }


//     // =========================================================
//     // KILL MUSIC ANIMATION
//     // =========================================================

//     private void KillMusicAnimation()
//     {
//         if (musicIcon != null)
//         {
//             musicIcon.DOKill();
//         }


//         if (musicNotes != null)
//         {
//             for (int i = 0;
//                  i < musicNotes.Length;
//                  i++)
//             {
//                 if (musicNotes[i] != null)
//                 {
//                     musicNotes[i].DOKill();
//                 }


//                 if (musicNoteCanvasGroups != null &&
//                     i < musicNoteCanvasGroups.Length &&
//                     musicNoteCanvasGroups[i] != null)
//                 {
//                     musicNoteCanvasGroups[i].DOKill();
//                 }
//             }
//         }


//         if (musicAnimationInitialized)
//         {
//             ResetMusicAnimationObjects();
//         }
//     }
// }













// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;

// public sealed class SettingsUIController : MonoBehaviour
// {
//     [Header("Settings Popup")]
//     [SerializeField] private GameObject settingsPanel;
//     [SerializeField] private Button closeButton;


//     // =========================================================
//     // SOUND
//     // =========================================================

//     [Header("Sound Button")]
//     [SerializeField] private Button soundButton;
//     [SerializeField] private GameObject soundOnUI;
//     [SerializeField] private GameObject soundOffUI;


//     [Header("Sound Animation")]
//     [Tooltip("Speaker icon jo scale animation karega.")]
//     [SerializeField] private RectTransform speakerIcon;

//     [Tooltip("Pehli sound bar jo right side move karegi.")]
//     [SerializeField] private RectTransform soundBar1;

//     [Tooltip("Dusri sound bar jo right side move karegi.")]
//     [SerializeField] private RectTransform soundBar2;

//     [SerializeField, Min(0f)]
//     private float soundBarMoveDistance = 12f;

//     [SerializeField, Min(0.01f)]
//     private float soundBarMoveDuration = 0.18f;

//     [SerializeField, Min(1f)]
//     private float speakerScaleMultiplier = 1.12f;

//     [SerializeField, Min(0.01f)]
//     private float speakerScaleDuration = 0.18f;

//     [SerializeField, Range(1, 5)]
//     private int soundAnimationLoops = 2;

//     [SerializeField, Min(0f)]
//     private float soundAnimationLoopDelay = 0.04f;

//     [SerializeField]
//     private Ease soundBarEase = Ease.OutQuad;

//     [SerializeField]
//     private Ease speakerEase = Ease.OutBack;


//     // =========================================================
//     // MUSIC
//     // =========================================================

//     [Header("Music Button")]
//     [SerializeField] private Button musicButton;
//     [SerializeField] private GameObject musicOnUI;
//     [SerializeField] private GameObject musicOffUI;


//     [Header("Music Animation")]
//     [Tooltip(
//         "Main music icon. Ye scale/bounce animation karega."
//     )]
//     [SerializeField] private RectTransform musicIcon;


//     [Tooltip(
//         "5 ya 6 small music icons/notes assign karein. " +
//         "Ye sab main music icon ki position se start hongi."
//     )]
//     [SerializeField] private RectTransform[] musicNotes;


//     [Tooltip(
//         "Small music icons kitne pixels upar jayengi."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteMoveDistance = 70f;


//     [Tooltip(
//         "Small music icons ko left/right kitna spread milega."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteHorizontalDrift = 25f;


//     [Tooltip(
//         "Small music icon ko upar jane mein kitna time lagega."
//     )]
//     [SerializeField, Min(0.05f)]
//     private float musicNoteMoveDuration = 0.65f;


//     [Tooltip(
//         "Har note ke beech chhota stagger delay."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteStagger = 0.035f;


//     [Tooltip(
//         "Small music icons fade out hongi."
//     )]
//     [SerializeField]
//     private bool musicNotesFadeOut = true;


//     [Tooltip(
//         "Movement ke kitne percent par fade start hogi."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float musicNoteFadeStart = 0.45f;


//     [Tooltip(
//         "Main music icon kitna scale hoga."
//     )]
//     [SerializeField, Min(1f)]
//     private float musicIconScaleMultiplier = 1.12f;


//     [Tooltip(
//         "Main music icon scale animation duration."
//     )]
//     [SerializeField, Min(0.01f)]
//     private float musicIconScaleDuration = 0.18f;


//     [Tooltip(
//         "Music animation kitni baar repeat hogi."
//     )]
//     [SerializeField, Range(1, 5)]
//     private int musicAnimationLoops = 2;


//     [Tooltip(
//         "Ek complete music animation ke baad doosri repetition " +
//         "se pehle interval."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicAnimationLoopDelay = 0.08f;


//     [SerializeField]
//     private Ease musicNoteEase = Ease.OutQuad;


//     [SerializeField]
//     private Ease musicIconEase = Ease.OutBack;


//     // =========================================================
//     // VIBRATION
//     // =========================================================

//     [Header("Vibration Button")]
//     [SerializeField] private Button vibrationButton;
//     [SerializeField] private GameObject vibrationOnUI;
//     [SerializeField] private GameObject vibrationOffUI;


//     // =========================================================
//     // MANAGERS
//     // =========================================================

//     [Header("Managers")]
//     [SerializeField] private AudioManager audioManager;
//     [SerializeField] private VibrationManager vibrationManager;


//     // =========================================================
//     // INTERNAL ANIMATION DATA
//     // =========================================================

//     private Vector2 soundBar1OriginalPosition;
//     private Vector2 soundBar2OriginalPosition;

//     private Vector3 speakerOriginalScale;

//     private Vector3 musicIconOriginalScale;

//     /*
//      * Notes ki authored/original Inspector positions.
//      * Animation complete hone par yahin restore hongi.
//      */
//     private Vector2[] musicNoteOriginalPositions;

//     private Vector3[] musicNoteOriginalScales;


//     /*
//      * Runtime start position.
//      *
//      * Har small music note animation ke waqt
//      * main music icon ki position se start karegi.
//      */
//     private Vector2[] musicNoteStartPositions;


//     private CanvasGroup[] musicNoteCanvasGroups;

//     private bool soundAnimationInitialized;
//     private bool musicAnimationInitialized;


//     // =========================================================
//     // AWAKE
//     // =========================================================

//     private void Awake()
//     {
//         if (settingsPanel != null)
//         {
//             UITransition.HideImmediate(settingsPanel);
//         }

//         CacheSoundAnimationPositions();

//         CacheMusicAnimationData();
//     }


//     // =========================================================
//     // ENABLE / DISABLE
//     // =========================================================

//     private void OnEnable()
//     {
//         ResolveReferences();

//         UIEventBroker.OnScreenChangeRequested +=
//             HandleScreenChange;

//         AddListeners();

//         RefreshButtonStates();
//     }


//     private void OnDisable()
//     {
//         UIEventBroker.OnScreenChangeRequested -=
//             HandleScreenChange;

//         RemoveListeners();

//         KillSoundAnimation();

//         KillMusicAnimation();
//     }


//     // =========================================================
//     // REFERENCES
//     // =========================================================

//     private void ResolveReferences()
//     {
//         if (audioManager == null)
//         {
//             audioManager =
//                 FindFirstObjectByType<AudioManager>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (vibrationManager == null)
//         {
//             vibrationManager =
//                 FindFirstObjectByType<VibrationManager>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     // =========================================================
//     // LISTENERS
//     // =========================================================

//     private void AddListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );

//             closeButton.onClick.AddListener(
//                 CloseSettings
//             );
//         }


//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );

//             soundButton.onClick.AddListener(
//                 ToggleSound
//             );
//         }


//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );

//             musicButton.onClick.AddListener(
//                 ToggleMusic
//             );
//         }


//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );

//             vibrationButton.onClick.AddListener(
//                 ToggleVibration
//             );
//         }
//     }


//     private void RemoveListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );
//         }


//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );
//         }


//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );
//         }


//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );
//         }
//     }


//     // =========================================================
//     // SCREEN CHANGE
//     // =========================================================

//     private void HandleScreenChange(
//         UIScreenType targetScreen
//     )
//     {
//         if (targetScreen ==
//             UIScreenType.SettingScreen)
//         {
//             OpenSettings();

//             return;
//         }


//         if (targetScreen ==
//             UIScreenType.MainMenu)
//         {
//             HideSettings();
//         }
//     }


//     // =========================================================
//     // OPEN SETTINGS
//     // =========================================================

//     public void OpenSettings()
//     {
//         ResolveReferences();

//         RefreshButtonStates();


//         if (settingsPanel != null)
//         {
//             UITransition.Show(
//                 settingsPanel
//             );
//         }


//         /*
//          * Settings open hote hi animations.
//          */
//         PlaySoundAnimation();

//         PlayMusicAnimation();
//     }


//     // =========================================================
//     // CLOSE SETTINGS
//     // =========================================================

//     public void CloseSettings()
//     {
//         KillSoundAnimation();

//         KillMusicAnimation();

//         HideSettings();


//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );
//     }


//     public void HideSettings()
//     {
//         KillSoundAnimation();

//         KillMusicAnimation();


//         if (settingsPanel != null)
//         {
//             UITransition.Hide(
//                 settingsPanel
//             );
//         }
//     }


//     // =========================================================
//     // SOUND TOGGLE
//     // =========================================================

//     private void ToggleSound()
//     {
//         ResolveReferences();


//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }


//         bool newState =
//             !audioManager.IsSoundEnabled;


//         audioManager.SetSoundEnabled(
//             newState
//         );


//         RefreshSoundUI();

//         PlaySoundAnimation();
//     }


//     // =========================================================
//     // MUSIC TOGGLE
//     // =========================================================

//     private void ToggleMusic()
//     {
//         ResolveReferences();


//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }


//         bool newState =
//             !audioManager.IsMusicEnabled;


//         audioManager.SetMusicEnabled(
//             newState
//         );


//         RefreshMusicUI();

//         PlayMusicAnimation();
//     }


//     // =========================================================
//     // VIBRATION TOGGLE
//     // =========================================================

//     private void ToggleVibration()
//     {
//         ResolveReferences();


//         if (vibrationManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: VibrationManager missing hai.",
//                 this
//             );

//             return;
//         }


//         bool newState =
//             !vibrationManager.IsVibrationEnabled;


//         vibrationManager.SetVibrationEnabled(
//             newState
//         );


//         RefreshVibrationUI();
//     }


//     // =========================================================
//     // REFRESH STATES
//     // =========================================================

//     private void RefreshButtonStates()
//     {
//         RefreshSoundUI();

//         RefreshMusicUI();

//         RefreshVibrationUI();
//     }


//     private void RefreshSoundUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsSoundEnabled;


//         SetStateUI(
//             soundOnUI,
//             soundOffUI,
//             isOn
//         );
//     }


//     private void RefreshMusicUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsMusicEnabled;


//         SetStateUI(
//             musicOnUI,
//             musicOffUI,
//             isOn
//         );
//     }


//     private void RefreshVibrationUI()
//     {
//         bool isOn =
//             vibrationManager != null &&
//             vibrationManager.IsVibrationEnabled;


//         SetStateUI(
//             vibrationOnUI,
//             vibrationOffUI,
//             isOn
//         );
//     }


//     private static void SetStateUI(
//         GameObject onUI,
//         GameObject offUI,
//         bool isOn
//     )
//     {
//         if (onUI != null)
//         {
//             onUI.SetActive(
//                 isOn
//             );
//         }


//         if (offUI != null)
//         {
//             offUI.SetActive(
//                 !isOn
//             );
//         }
//     }


//     // =========================================================
//     // SOUND ANIMATION
//     // =========================================================

//     private void CacheSoundAnimationPositions()
//     {
//         if (soundBar1 != null)
//         {
//             soundBar1OriginalPosition =
//                 soundBar1.anchoredPosition;
//         }


//         if (soundBar2 != null)
//         {
//             soundBar2OriginalPosition =
//                 soundBar2.anchoredPosition;
//         }


//         if (speakerIcon != null)
//         {
//             speakerOriginalScale =
//                 speakerIcon.localScale;
//         }


//         soundAnimationInitialized = true;
//     }


//     public void PlaySoundAnimation()
//     {
//         if (!soundAnimationInitialized)
//         {
//             CacheSoundAnimationPositions();
//         }


//         KillSoundAnimation();


//         Sequence animation =
//             DOTween.Sequence();


//         // ---------------------------------------------------------
//         // BAR 1
//         // ---------------------------------------------------------

//         if (soundBar1 != null)
//         {
//             soundBar1.anchoredPosition =
//                 soundBar1OriginalPosition;


//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 Vector2 rightPosition =
//                     soundBar1OriginalPosition +
//                     Vector2.right *
//                     soundBarMoveDistance;


//                 animation.Append(
//                     soundBar1
//                         .DOAnchorPos(
//                             rightPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 animation.Append(
//                     soundBar1
//                         .DOAnchorPos(
//                             soundBar1OriginalPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     animation.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }
//         }


//         // ---------------------------------------------------------
//         // BAR 2
//         // ---------------------------------------------------------

//         if (soundBar2 != null)
//         {
//             soundBar2.anchoredPosition =
//                 soundBar2OriginalPosition;


//             Sequence bar2Sequence =
//                 DOTween.Sequence();


//             bar2Sequence.AppendInterval(
//                 soundBarMoveDuration *
//                 0.35f
//             );


//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 Vector2 rightPosition =
//                     soundBar2OriginalPosition +
//                     Vector2.right *
//                     soundBarMoveDistance;


//                 bar2Sequence.Append(
//                     soundBar2
//                         .DOAnchorPos(
//                             rightPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 bar2Sequence.Append(
//                     soundBar2
//                         .DOAnchorPos(
//                             soundBar2OriginalPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     bar2Sequence.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }


//             animation.Insert(
//                 0f,
//                 bar2Sequence
//             );
//         }


//         // ---------------------------------------------------------
//         // SPEAKER
//         // ---------------------------------------------------------

//         if (speakerIcon != null)
//         {
//             speakerIcon.localScale =
//                 speakerOriginalScale;


//             Vector3 enlargedScale =
//                 speakerOriginalScale *
//                 speakerScaleMultiplier;


//             Sequence speakerSequence =
//                 DOTween.Sequence();


//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 speakerSequence.Append(
//                     speakerIcon
//                         .DOScale(
//                             enlargedScale,
//                             speakerScaleDuration
//                         )
//                         .SetEase(
//                             speakerEase
//                         )
//                 );


//                 speakerSequence.Append(
//                     speakerIcon
//                         .DOScale(
//                             speakerOriginalScale,
//                             speakerScaleDuration
//                         )
//                         .SetEase(
//                             Ease.OutQuad
//                         )
//                 );


//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     speakerSequence.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }


//             animation.Insert(
//                 0f,
//                 speakerSequence
//             );
//         }


//         animation.Play();
//     }


//     // =========================================================
//     // MUSIC ANIMATION DATA
//     // =========================================================

//     private void CacheMusicAnimationData()
//     {
//         if (musicIcon != null)
//         {
//             musicIconOriginalScale =
//                 musicIcon.localScale;
//         }


//         if (musicNotes == null ||
//             musicNotes.Length == 0)
//         {
//             musicAnimationInitialized = true;

//             return;
//         }


//         musicNoteOriginalPositions =
//             new Vector2[
//                 musicNotes.Length
//             ];


//         musicNoteOriginalScales =
//             new Vector3[
//                 musicNotes.Length
//             ];


//         musicNoteStartPositions =
//             new Vector2[
//                 musicNotes.Length
//             ];


//         musicNoteCanvasGroups =
//             new CanvasGroup[
//                 musicNotes.Length
//             ];


//         for (int i = 0;
//              i < musicNotes.Length;
//              i++)
//         {
//             RectTransform note =
//                 musicNotes[i];


//             if (note == null)
//             {
//                 continue;
//             }


//             /*
//              * Save original Inspector position.
//              */
//             musicNoteOriginalPositions[i] =
//                 note.anchoredPosition;


//             musicNoteOriginalScales[i] =
//                 note.localScale;


//             /*
//              * Calculate main music icon position
//              * relative to each note's parent.
//              */
//             musicNoteStartPositions[i] =
//                 GetAnchoredPositionRelativeToParent(
//                     musicIcon,
//                     note.parent as RectTransform
//                 );


//             CanvasGroup canvasGroup =
//                 note.GetComponent<CanvasGroup>();


//             if (canvasGroup == null)
//             {
//                 canvasGroup =
//                     note.gameObject.AddComponent<CanvasGroup>();
//             }


//             musicNoteCanvasGroups[i] =
//                 canvasGroup;
//         }


//         musicAnimationInitialized = true;
//     }


//     // =========================================================
//     // MUSIC ANIMATION
//     // =========================================================

//     public void PlayMusicAnimation()
//     {
//         if (!musicAnimationInitialized)
//         {
//             CacheMusicAnimationData();
//         }


//         KillMusicAnimation();


//         Sequence musicSequence =
//             DOTween.Sequence();


//         // ---------------------------------------------------------
//         // SMALL MUSIC ICONS / NOTES
//         // ---------------------------------------------------------

//         if (musicNotes != null &&
//             musicNotes.Length > 0)
//         {
//             for (int i = 0;
//                  i < musicNotes.Length;
//                  i++)
//             {
//                 RectTransform note =
//                     musicNotes[i];


//                 if (note == null)
//                 {
//                     continue;
//                 }


//                 /*
//                  * IMPORTANT:
//                  *
//                  * Small note main music icon ki
//                  * exact position se start karegi.
//                  */
//                 note.anchoredPosition =
//                     musicNoteStartPositions[i];


//                 note.localScale =
//                     musicNoteOriginalScales[i];


//                 CanvasGroup canvasGroup =
//                     musicNoteCanvasGroups[i];


//                 if (canvasGroup != null)
//                 {
//                     canvasGroup.alpha = 1f;
//                 }


//                 /*
//                  * Left/right spread.
//                  *
//                  * 0 = left side
//                  * middle = center
//                  * last = right side
//                  */
//                 float normalizedIndex =
//                     musicNotes.Length > 1
//                         ? (float)i /
//                           (musicNotes.Length - 1)
//                         : 0.5f;


//                 float spreadDirection =
//                     Mathf.Lerp(
//                         -1f,
//                         1f,
//                         normalizedIndex
//                     );


//                 float randomDrift =
//                     Random.Range(
//                         -musicNoteHorizontalDrift,
//                         musicNoteHorizontalDrift
//                     );


//                 randomDrift +=
//                     spreadDirection *
//                     musicNoteHorizontalDrift *
//                     0.35f;


//                 Vector2 targetPosition =
//                     musicNoteStartPositions[i] +
//                     new Vector2(
//                         randomDrift,
//                         musicNoteMoveDistance
//                     );


//                 /*
//                  * Har note slightly different time par start hogi.
//                  */
//                 float noteDelay =
//                     i *
//                     musicNoteStagger;


//                 Tween moveTween =
//                     note
//                         .DOAnchorPos(
//                             targetPosition,
//                             musicNoteMoveDuration
//                         )
//                         .SetEase(
//                             musicNoteEase
//                         );


//                 musicSequence.Insert(
//                     noteDelay,
//                     moveTween
//                 );


//                 // -------------------------------------------------
//                 // FADE
//                 // -------------------------------------------------

//                 if (musicNotesFadeOut &&
//                     canvasGroup != null)
//                 {
//                     float fadeDelay =
//                         noteDelay +
//                         musicNoteMoveDuration *
//                         musicNoteFadeStart;


//                     float fadeDuration =
//                         musicNoteMoveDuration *
//                         (1f -
//                          musicNoteFadeStart);


//                     Tween fadeTween =
//                         canvasGroup
//                             .DOFade(
//                                 0f,
//                                 Mathf.Max(
//                                     0.05f,
//                                     fadeDuration
//                                 )
//                             );


//                     musicSequence.Insert(
//                         fadeDelay,
//                         fadeTween
//                     );
//                 }
//             }
//         }


//         // ---------------------------------------------------------
//         // MAIN MUSIC ICON
//         // ---------------------------------------------------------

//         if (musicIcon != null)
//         {
//             musicIcon.localScale =
//                 musicIconOriginalScale;


//             Vector3 enlargedScale =
//                 musicIconOriginalScale *
//                 musicIconScaleMultiplier;


//             Sequence iconSequence =
//                 DOTween.Sequence();


//             for (int i = 0;
//                  i < musicAnimationLoops;
//                  i++)
//             {
//                 iconSequence.Append(
//                     musicIcon
//                         .DOScale(
//                             enlargedScale,
//                             musicIconScaleDuration
//                         )
//                         .SetEase(
//                             musicIconEase
//                         )
//                 );


//                 iconSequence.Append(
//                     musicIcon
//                         .DOScale(
//                             musicIconOriginalScale,
//                             musicIconScaleDuration
//                         )
//                         .SetEase(
//                             Ease.OutQuad
//                         )
//                 );


//                 /*
//                  * Important:
//                  *
//                  * Ek animation complete hone ke baad
//                  * next animation se pehle interval.
//                  */
//                 if (musicAnimationLoopDelay > 0f)
//                 {
//                     iconSequence.AppendInterval(
//                         musicAnimationLoopDelay
//                     );
//                 }
//             }


//             musicSequence.Insert(
//                 0f,
//                 iconSequence
//             );
//         }


//         musicSequence.OnComplete(
//             ResetMusicAnimationObjects
//         );


//         musicSequence.Play();
//     }


//     // =========================================================
//     // RESET MUSIC ANIMATION
//     // =========================================================

//     private void ResetMusicAnimationObjects()
//     {
//         if (musicIcon != null)
//         {
//             musicIcon.localScale =
//                 musicIconOriginalScale;
//         }


//         if (musicNotes == null)
//         {
//             return;
//         }


//         for (int i = 0;
//              i < musicNotes.Length;
//              i++)
//         {
//             RectTransform note =
//                 musicNotes[i];


//             if (note == null)
//             {
//                 continue;
//             }


//             /*
//              * Animation complete hone ke baad
//              * notes apni Inspector positions par wapas.
//              */
//             note.anchoredPosition =
//                 musicNoteOriginalPositions[i];


//             note.localScale =
//                 musicNoteOriginalScales[i];


//             CanvasGroup canvasGroup =
//                 musicNoteCanvasGroups[i];


//             if (canvasGroup != null)
//             {
//                 canvasGroup.alpha = 1f;
//             }
//         }
//     }


//     // =========================================================
//     // KILL SOUND ANIMATION
//     // =========================================================

//     private void KillSoundAnimation()
//     {
//         if (soundBar1 != null)
//         {
//             soundBar1.DOKill();


//             if (soundAnimationInitialized)
//             {
//                 soundBar1.anchoredPosition =
//                     soundBar1OriginalPosition;
//             }
//         }


//         if (soundBar2 != null)
//         {
//             soundBar2.DOKill();


//             if (soundAnimationInitialized)
//             {
//                 soundBar2.anchoredPosition =
//                     soundBar2OriginalPosition;
//             }
//         }


//         if (speakerIcon != null)
//         {
//             speakerIcon.DOKill();


//             if (soundAnimationInitialized)
//             {
//                 speakerIcon.localScale =
//                     speakerOriginalScale;
//             }
//         }
//     }


//     // =========================================================
//     // KILL MUSIC ANIMATION
//     // =========================================================

//     private void KillMusicAnimation()
//     {
//         if (musicIcon != null)
//         {
//             musicIcon.DOKill();
//         }


//         if (musicNotes != null)
//         {
//             for (int i = 0;
//                  i < musicNotes.Length;
//                  i++)
//             {
//                 if (musicNotes[i] != null)
//                 {
//                     musicNotes[i].DOKill();
//                 }


//                 if (musicNoteCanvasGroups != null &&
//                     i < musicNoteCanvasGroups.Length &&
//                     musicNoteCanvasGroups[i] != null)
//                 {
//                     musicNoteCanvasGroups[i].DOKill();
//                 }
//             }
//         }


//         if (musicAnimationInitialized)
//         {
//             ResetMusicAnimationObjects();
//         }
//     }


//     // =========================================================
//     // UI POSITION HELPER
//     // =========================================================

//     private static Vector2 GetAnchoredPositionRelativeToParent(
//         RectTransform target,
//         RectTransform targetParent
//     )
//     {
//         if (target == null ||
//             targetParent == null)
//         {
//             return Vector2.zero;
//         }


//         Vector3 worldPosition =
//             target.position;


//         Vector3 localPosition =
//             targetParent.InverseTransformPoint(
//                 worldPosition
//             );


//         return new Vector2(
//             localPosition.x,
//             localPosition.y
//         );
//     }
// }







// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;

// public sealed class SettingsUIController : MonoBehaviour
// {
//     [Header("Settings Popup")]
//     [SerializeField] private GameObject settingsPanel;
//     [SerializeField] private Button closeButton;


//     // =========================================================
//     // SOUND
//     // =========================================================

//     [Header("Sound Button")]
//     [SerializeField] private Button soundButton;
//     [SerializeField] private GameObject soundOnUI;
//     [SerializeField] private GameObject soundOffUI;


//     [Header("Sound Animation")]
//     [Tooltip("Speaker icon jo scale animation karega.")]
//     [SerializeField] private RectTransform speakerIcon;

//     [Tooltip("Pehli sound bar jo right side move karegi.")]
//     [SerializeField] private RectTransform soundBar1;

//     [Tooltip("Dusri sound bar jo right side move karegi.")]
//     [SerializeField] private RectTransform soundBar2;

//     [SerializeField, Min(0f)]
//     private float soundBarMoveDistance = 12f;

//     [SerializeField, Min(0.01f)]
//     private float soundBarMoveDuration = 0.18f;

//     [SerializeField, Min(1f)]
//     private float speakerScaleMultiplier = 1.12f;

//     [SerializeField, Min(0.01f)]
//     private float speakerScaleDuration = 0.18f;

//     [SerializeField, Range(1, 5)]
//     private int soundAnimationLoops = 2;

//     [SerializeField, Min(0f)]
//     private float soundAnimationLoopDelay = 0.04f;

//     [SerializeField]
//     private Ease soundBarEase = Ease.OutQuad;

//     [SerializeField]
//     private Ease speakerEase = Ease.OutBack;


//     // =========================================================
//     // MUSIC
//     // =========================================================

//     [Header("Music Button")]
//     [SerializeField] private Button musicButton;
//     [SerializeField] private GameObject musicOnUI;
//     [SerializeField] private GameObject musicOffUI;


//     [Header("Music Animation")]
//     [Tooltip(
//         "Main music icon. Ye scale/bounce animation karega."
//     )]
//     [SerializeField] private RectTransform musicIcon;


//     [Tooltip(
//         "5 ya 6 small music icons/notes assign karein. " +
//         "Ye sab main music icon ki position se start hongi."
//     )]
//     [SerializeField] private RectTransform[] musicNotes;


//     [Tooltip(
//         "Small music icons kitne pixels upar jayengi."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteMoveDistance = 70f;


//     [Tooltip(
//         "Small music icons ko left/right kitna spread milega."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteHorizontalDrift = 25f;


//     [Tooltip(
//         "Small music icon ko upar jane mein kitna time lagega."
//     )]
//     [SerializeField, Min(0.05f)]
//     private float musicNoteMoveDuration = 0.65f;


//     [Tooltip(
//         "Har note ke beech chhota stagger delay."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteStagger = 0.035f;


//     [Tooltip(
//         "Small music icons fade out hongi."
//     )]
//     [SerializeField]
//     private bool musicNotesFadeOut = true;


//     [Tooltip(
//         "Movement ke kitne percent par fade start hogi."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float musicNoteFadeStart = 0.45f;


//     [Tooltip(
//         "Main music icon kitna scale hoga."
//     )]
//     [SerializeField, Min(1f)]
//     private float musicIconScaleMultiplier = 1.12f;


//     [Tooltip(
//         "Main music icon scale animation duration."
//     )]
//     [SerializeField, Min(0.01f)]
//     private float musicIconScaleDuration = 0.18f;


//     [Tooltip(
//         "Music animation kitni baar repeat hogi."
//     )]
//     [SerializeField, Range(1, 5)]
//     private int musicAnimationLoops = 2;


//     [Tooltip(
//         "Ek complete music animation ke baad doosri repetition " +
//         "se pehle interval."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicAnimationLoopDelay = 0.08f;


//     [SerializeField]
//     private Ease musicNoteEase = Ease.OutQuad;


//     [SerializeField]
//     private Ease musicIconEase = Ease.OutBack;


//     // =========================================================
//     // VIBRATION
//     // =========================================================

//     [Header("Vibration Button")]
//     [SerializeField] private Button vibrationButton;
//     [SerializeField] private GameObject vibrationOnUI;
//     [SerializeField] private GameObject vibrationOffUI;


//     [Header("Vibration Animation")]
//     [Tooltip("Vibration ka top icon. Animation mein upar se neeche ayega.")]
//     [SerializeField] private RectTransform vibrationTopIcon;

//     [Tooltip("Vibration ka bottom icon. Animation mein neeche se upar ayega.")]
//     [SerializeField] private RectTransform vibrationBottomIcon;

//     [Tooltip("Center ka main phone/vibration icon.")]
//     [SerializeField] private RectTransform vibrationPhoneIcon;

//     [SerializeField, Min(0f)]
//     private float vibrationEntryDistance = 45f;

//     [SerializeField, Min(0.01f)]
//     private float vibrationEntryDuration = 0.25f;

//     [SerializeField, Min(0.01f)]
//     private float vibrationPhoneFadeDuration = 0.25f;

//     [SerializeField, Min(0.01f)]
//     private float vibrationShakeDuration = 0.45f;

//     [SerializeField, Min(0.01f)]
//     private float vibrationShakeStrength = 5f;

//     [SerializeField, Min(1)]
//     private int vibrationShakeVibrato = 12;

//     [SerializeField, Min(0f)]
//     private float vibrationAnimationDelay = 0.05f;

//     [SerializeField]
//     private Ease vibrationEntryEase = Ease.OutBack;

//     [SerializeField]
//     private Ease vibrationPhoneFadeEase = Ease.OutQuad;


//     // =========================================================
//     // MANAGERS
//     // =========================================================

//     [Header("Managers")]
//     [SerializeField] private AudioManager audioManager;
//     [SerializeField] private VibrationManager vibrationManager;


//     // =========================================================
//     // INTERNAL ANIMATION DATA
//     // =========================================================

//     private Vector2 soundBar1OriginalPosition;
//     private Vector2 soundBar2OriginalPosition;

//     private Vector3 speakerOriginalScale;

//     private Vector3 musicIconOriginalScale;

//     /*
//      * Notes ki authored/original Inspector positions.
//      * Animation complete hone par yahin restore hongi.
//      */
//     private Vector2[] musicNoteOriginalPositions;

//     private Vector3[] musicNoteOriginalScales;


//     /*
//      * Runtime start position.
//      *
//      * Har small music note animation ke waqt
//      * main music icon ki position se start karegi.
//      */
//     private Vector2[] musicNoteStartPositions;


//     private CanvasGroup[] musicNoteCanvasGroups;

//     private bool soundAnimationInitialized;
//     private bool musicAnimationInitialized;

//     private Vector2 vibrationTopOriginalPosition;
//     private Vector2 vibrationBottomOriginalPosition;
//     private Vector3 vibrationPhoneOriginalScale;
//     private CanvasGroup vibrationPhoneCanvasGroup;
//     private bool vibrationAnimationInitialized;


//     // =========================================================
//     // AWAKE
//     // =========================================================

//     private void Awake()
//     {
//         if (settingsPanel != null)
//         {
//             UITransition.HideImmediate(settingsPanel);
//         }

//         CacheSoundAnimationPositions();

//         CacheMusicAnimationData();
//         HideMusicNotesImmediate();

//         CacheVibrationAnimationData();
//         HideVibrationAnimationImmediate();
//     }


//     // =========================================================
//     // ENABLE / DISABLE
//     // =========================================================

//     private void OnEnable()
//     {
//         ResolveReferences();

//         UIEventBroker.OnScreenChangeRequested +=
//             HandleScreenChange;

//         AddListeners();

//         RefreshButtonStates();
//     }


//     private void OnDisable()
//     {
//         UIEventBroker.OnScreenChangeRequested -=
//             HandleScreenChange;

//         RemoveListeners();

//         KillSoundAnimation();

//         KillMusicAnimation();

//         KillVibrationAnimation();
//     }


//     // =========================================================
//     // REFERENCES
//     // =========================================================

//     private void ResolveReferences()
//     {
//         if (audioManager == null)
//         {
//             audioManager =
//                 FindFirstObjectByType<AudioManager>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (vibrationManager == null)
//         {
//             vibrationManager =
//                 FindFirstObjectByType<VibrationManager>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     // =========================================================
//     // LISTENERS
//     // =========================================================

//     private void AddListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );

//             closeButton.onClick.AddListener(
//                 CloseSettings
//             );
//         }


//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );

//             soundButton.onClick.AddListener(
//                 ToggleSound
//             );
//         }


//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );

//             musicButton.onClick.AddListener(
//                 ToggleMusic
//             );
//         }


//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );

//             vibrationButton.onClick.AddListener(
//                 ToggleVibration
//             );
//         }
//     }


//     private void RemoveListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );
//         }


//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );
//         }


//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );
//         }


//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );
//         }
//     }


//     // =========================================================
//     // SCREEN CHANGE
//     // =========================================================

//     private void HandleScreenChange(
//         UIScreenType targetScreen
//     )
//     {
//         if (targetScreen ==
//             UIScreenType.SettingScreen)
//         {
//             OpenSettings();

//             return;
//         }


//         if (targetScreen ==
//             UIScreenType.MainMenu)
//         {
//             HideSettings();
//         }
//     }


//     // =========================================================
//     // OPEN SETTINGS
//     // =========================================================

//     public void OpenSettings()
//     {
//         ResolveReferences();

//         RefreshButtonStates();


//         if (settingsPanel != null)
//         {
//             UITransition.Show(
//                 settingsPanel
//             );
//         }


//         /*
//          * Settings open hote hi animations.
//          */
//         PlaySoundAnimation();

//         PlayMusicAnimation();

//         PlayVibrationAnimation();
//     }


//     // =========================================================
//     // CLOSE SETTINGS
//     // =========================================================

//     public void CloseSettings()
//     {
//         KillSoundAnimation();

//         KillMusicAnimation();

//         KillVibrationAnimation();

//         HideSettings();


//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );
//     }


//     public void HideSettings()
//     {
//         KillSoundAnimation();

//         KillMusicAnimation();

//         KillVibrationAnimation();


//         if (settingsPanel != null)
//         {
//             UITransition.Hide(
//                 settingsPanel
//             );
//         }
//     }


//     // =========================================================
//     // SOUND TOGGLE
//     // =========================================================

//     private void ToggleSound()
//     {
//         ResolveReferences();


//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }


//         bool newState =
//             !audioManager.IsSoundEnabled;


//         audioManager.SetSoundEnabled(
//             newState
//         );


//         RefreshSoundUI();

//         PlaySoundAnimation();
//     }


//     // =========================================================
//     // MUSIC TOGGLE
//     // =========================================================

//     private void ToggleMusic()
//     {
//         ResolveReferences();


//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }


//         bool newState =
//             !audioManager.IsMusicEnabled;


//         audioManager.SetMusicEnabled(
//             newState
//         );


//         RefreshMusicUI();

//         PlayMusicAnimation();
//     }


//     // =========================================================
//     // VIBRATION TOGGLE
//     // =========================================================

//     private void ToggleVibration()
//     {
//         ResolveReferences();


//         if (vibrationManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: VibrationManager missing hai.",
//                 this
//             );

//             return;
//         }


//         bool newState =
//             !vibrationManager.IsVibrationEnabled;


//         vibrationManager.SetVibrationEnabled(
//             newState
//         );


//         RefreshVibrationUI();

//         PlayVibrationAnimation();
//     }


//     // =========================================================
//     // REFRESH STATES
//     // =========================================================

//     private void RefreshButtonStates()
//     {
//         RefreshSoundUI();

//         RefreshMusicUI();

//         RefreshVibrationUI();
//     }


//     private void RefreshSoundUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsSoundEnabled;


//         SetStateUI(
//             soundOnUI,
//             soundOffUI,
//             isOn
//         );
//     }


//     private void RefreshMusicUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsMusicEnabled;


//         SetStateUI(
//             musicOnUI,
//             musicOffUI,
//             isOn
//         );
//     }


//     private void RefreshVibrationUI()
//     {
//         bool isOn =
//             vibrationManager != null &&
//             vibrationManager.IsVibrationEnabled;


//         SetStateUI(
//             vibrationOnUI,
//             vibrationOffUI,
//             isOn
//         );
//     }


//     private static void SetStateUI(
//         GameObject onUI,
//         GameObject offUI,
//         bool isOn
//     )
//     {
//         if (onUI != null)
//         {
//             onUI.SetActive(
//                 isOn
//             );
//         }


//         if (offUI != null)
//         {
//             offUI.SetActive(
//                 !isOn
//             );
//         }
//     }


//     // =========================================================
//     // SOUND ANIMATION
//     // =========================================================

//     private void CacheSoundAnimationPositions()
//     {
//         if (soundBar1 != null)
//         {
//             soundBar1OriginalPosition =
//                 soundBar1.anchoredPosition;
//         }


//         if (soundBar2 != null)
//         {
//             soundBar2OriginalPosition =
//                 soundBar2.anchoredPosition;
//         }


//         if (speakerIcon != null)
//         {
//             speakerOriginalScale =
//                 speakerIcon.localScale;
//         }


//         soundAnimationInitialized = true;
//     }


//     public void PlaySoundAnimation()
//     {
//         if (!soundAnimationInitialized)
//         {
//             CacheSoundAnimationPositions();
//         }


//         KillSoundAnimation();


//         Sequence animation =
//             DOTween.Sequence();


//         // ---------------------------------------------------------
//         // BAR 1
//         // ---------------------------------------------------------

//         if (soundBar1 != null)
//         {
//             soundBar1.anchoredPosition =
//                 soundBar1OriginalPosition;


//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 Vector2 rightPosition =
//                     soundBar1OriginalPosition +
//                     Vector2.right *
//                     soundBarMoveDistance;


//                 animation.Append(
//                     soundBar1
//                         .DOAnchorPos(
//                             rightPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 animation.Append(
//                     soundBar1
//                         .DOAnchorPos(
//                             soundBar1OriginalPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     animation.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }
//         }


//         // ---------------------------------------------------------
//         // BAR 2
//         // ---------------------------------------------------------

//         if (soundBar2 != null)
//         {
//             soundBar2.anchoredPosition =
//                 soundBar2OriginalPosition;


//             Sequence bar2Sequence =
//                 DOTween.Sequence();


//             bar2Sequence.AppendInterval(
//                 soundBarMoveDuration *
//                 0.35f
//             );


//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 Vector2 rightPosition =
//                     soundBar2OriginalPosition +
//                     Vector2.right *
//                     soundBarMoveDistance;


//                 bar2Sequence.Append(
//                     soundBar2
//                         .DOAnchorPos(
//                             rightPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 bar2Sequence.Append(
//                     soundBar2
//                         .DOAnchorPos(
//                             soundBar2OriginalPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     bar2Sequence.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }


//             animation.Insert(
//                 0f,
//                 bar2Sequence
//             );
//         }


//         // ---------------------------------------------------------
//         // SPEAKER
//         // ---------------------------------------------------------

//         if (speakerIcon != null)
//         {
//             speakerIcon.localScale =
//                 speakerOriginalScale;


//             Vector3 enlargedScale =
//                 speakerOriginalScale *
//                 speakerScaleMultiplier;


//             Sequence speakerSequence =
//                 DOTween.Sequence();


//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 speakerSequence.Append(
//                     speakerIcon
//                         .DOScale(
//                             enlargedScale,
//                             speakerScaleDuration
//                         )
//                         .SetEase(
//                             speakerEase
//                         )
//                 );


//                 speakerSequence.Append(
//                     speakerIcon
//                         .DOScale(
//                             speakerOriginalScale,
//                             speakerScaleDuration
//                         )
//                         .SetEase(
//                             Ease.OutQuad
//                         )
//                 );


//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     speakerSequence.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }


//             animation.Insert(
//                 0f,
//                 speakerSequence
//             );
//         }


//         animation.Play();
//     }


//     // =========================================================
//     // MUSIC ANIMATION DATA
//     // =========================================================

//     private void CacheMusicAnimationData()
//     {
//         if (musicIcon != null)
//         {
//             musicIconOriginalScale =
//                 musicIcon.localScale;
//         }


//         if (musicNotes == null ||
//             musicNotes.Length == 0)
//         {
//             musicAnimationInitialized = true;

//             return;
//         }


//         musicNoteOriginalPositions =
//             new Vector2[
//                 musicNotes.Length
//             ];


//         musicNoteOriginalScales =
//             new Vector3[
//                 musicNotes.Length
//             ];


//         musicNoteStartPositions =
//             new Vector2[
//                 musicNotes.Length
//             ];


//         musicNoteCanvasGroups =
//             new CanvasGroup[
//                 musicNotes.Length
//             ];


//         for (int i = 0;
//              i < musicNotes.Length;
//              i++)
//         {
//             RectTransform note =
//                 musicNotes[i];


//             if (note == null)
//             {
//                 continue;
//             }


//             /*
//              * Save original Inspector position.
//              */
//             musicNoteOriginalPositions[i] =
//                 note.anchoredPosition;


//             musicNoteOriginalScales[i] =
//                 note.localScale;


//             /*
//              * Calculate main music icon position
//              * relative to each note's parent.
//              */
//             musicNoteStartPositions[i] =
//                 GetAnchoredPositionRelativeToParent(
//                     musicIcon,
//                     note.parent as RectTransform
//                 );


//             CanvasGroup canvasGroup =
//                 note.GetComponent<CanvasGroup>();


//             if (canvasGroup == null)
//             {
//                 canvasGroup =
//                     note.gameObject.AddComponent<CanvasGroup>();
//             }


//             musicNoteCanvasGroups[i] =
//                 canvasGroup;
//         }


//         musicAnimationInitialized = true;
//     }


//     // =========================================================
//     // MUSIC ANIMATION
//     // =========================================================

//     public void PlayMusicAnimation()
//     {
//         if (!musicAnimationInitialized)
//         {
//             CacheMusicAnimationData();
//         }


//         KillMusicAnimation();


//         Sequence musicSequence =
//             DOTween.Sequence();


//         // ---------------------------------------------------------
//         // SMALL MUSIC ICONS / NOTES
//         // ---------------------------------------------------------

//         if (musicNotes != null &&
//             musicNotes.Length > 0)
//         {
//             for (int i = 0;
//                  i < musicNotes.Length;
//                  i++)
//             {
//                 RectTransform note =
//                     musicNotes[i];


//                 if (note == null)
//                 {
//                     continue;
//                 }

//                 // Small music notes normally hidden rehti hain.
//                 // Animation start hote hi enable hongi.
//                 note.gameObject.SetActive(true);


//                 /*
//                  * IMPORTANT:
//                  *
//                  * Small note main music icon ki
//                  * exact position se start karegi.
//                  */
//                 note.anchoredPosition =
//                     musicNoteStartPositions[i];


//                 note.localScale =
//                     musicNoteOriginalScales[i];


//                 CanvasGroup canvasGroup =
//                     musicNoteCanvasGroups[i];


//                 if (canvasGroup != null)
//                 {
//                     canvasGroup.alpha = 1f;
//                 }


//                 /*
//                  * Left/right spread.
//                  *
//                  * 0 = left side
//                  * middle = center
//                  * last = right side
//                  */
//                 float normalizedIndex =
//                     musicNotes.Length > 1
//                         ? (float)i /
//                           (musicNotes.Length - 1)
//                         : 0.5f;


//                 float spreadDirection =
//                     Mathf.Lerp(
//                         -1f,
//                         1f,
//                         normalizedIndex
//                     );


//                 float randomDrift =
//                     Random.Range(
//                         -musicNoteHorizontalDrift,
//                         musicNoteHorizontalDrift
//                     );


//                 randomDrift +=
//                     spreadDirection *
//                     musicNoteHorizontalDrift *
//                     0.35f;


//                 Vector2 targetPosition =
//                     musicNoteStartPositions[i] +
//                     new Vector2(
//                         randomDrift,
//                         musicNoteMoveDistance
//                     );


//                 /*
//                  * Har note slightly different time par start hogi.
//                  */
//                 float noteDelay =
//                     i *
//                     musicNoteStagger;


//                 Tween moveTween =
//                     note
//                         .DOAnchorPos(
//                             targetPosition,
//                             musicNoteMoveDuration
//                         )
//                         .SetEase(
//                             musicNoteEase
//                         );


//                 musicSequence.Insert(
//                     noteDelay,
//                     moveTween
//                 );


//                 // -------------------------------------------------
//                 // FADE
//                 // -------------------------------------------------

//                 if (musicNotesFadeOut &&
//                     canvasGroup != null)
//                 {
//                     float fadeDelay =
//                         noteDelay +
//                         musicNoteMoveDuration *
//                         musicNoteFadeStart;


//                     float fadeDuration =
//                         musicNoteMoveDuration *
//                         (1f -
//                          musicNoteFadeStart);


//                     Tween fadeTween =
//                         canvasGroup
//                             .DOFade(
//                                 0f,
//                                 Mathf.Max(
//                                     0.05f,
//                                     fadeDuration
//                                 )
//                             );


//                     musicSequence.Insert(
//                         fadeDelay,
//                         fadeTween
//                     );
//                 }
//             }
//         }


//         // ---------------------------------------------------------
//         // MAIN MUSIC ICON
//         // ---------------------------------------------------------

//         if (musicIcon != null)
//         {
//             musicIcon.localScale =
//                 musicIconOriginalScale;


//             Vector3 enlargedScale =
//                 musicIconOriginalScale *
//                 musicIconScaleMultiplier;


//             Sequence iconSequence =
//                 DOTween.Sequence();


//             for (int i = 0;
//                  i < musicAnimationLoops;
//                  i++)
//             {
//                 iconSequence.Append(
//                     musicIcon
//                         .DOScale(
//                             enlargedScale,
//                             musicIconScaleDuration
//                         )
//                         .SetEase(
//                             musicIconEase
//                         )
//                 );


//                 iconSequence.Append(
//                     musicIcon
//                         .DOScale(
//                             musicIconOriginalScale,
//                             musicIconScaleDuration
//                         )
//                         .SetEase(
//                             Ease.OutQuad
//                         )
//                 );


//                 /*
//                  * Important:
//                  *
//                  * Ek animation complete hone ke baad
//                  * next animation se pehle interval.
//                  */
//                 if (musicAnimationLoopDelay > 0f)
//                 {
//                     iconSequence.AppendInterval(
//                         musicAnimationLoopDelay
//                     );
//                 }
//             }


//             musicSequence.Insert(
//                 0f,
//                 iconSequence
//             );
//         }


//         musicSequence.OnComplete(
//             ResetMusicAnimationObjects
//         );


//         musicSequence.Play();
//     }


//     // =========================================================
//     // RESET MUSIC ANIMATION
//     // =========================================================

//     private void ResetMusicAnimationObjects()
//     {
//         if (musicIcon != null)
//         {
//             musicIcon.localScale =
//                 musicIconOriginalScale;
//         }


//         if (musicNotes == null)
//         {
//             return;
//         }


//         for (int i = 0;
//              i < musicNotes.Length;
//              i++)
//         {
//             RectTransform note =
//                 musicNotes[i];


//             if (note == null)
//             {
//                 continue;
//             }


//             /*
//              * Animation complete hone ke baad
//              * notes apni Inspector positions par wapas.
//              */
//             note.anchoredPosition =
//                 musicNoteOriginalPositions[i];


//             note.localScale =
//                 musicNoteOriginalScales[i];


//             CanvasGroup canvasGroup =
//                 musicNoteCanvasGroups[i];


//             if (canvasGroup != null)
//             {
//                 canvasGroup.alpha = 1f;
//             }

//             // Animation complete hone ke baad note dobara hidden.
//             note.gameObject.SetActive(false);
//         }
//     }


//     // =========================================================
//     // KILL SOUND ANIMATION
//     // =========================================================

//     private void KillSoundAnimation()
//     {
//         if (soundBar1 != null)
//         {
//             soundBar1.DOKill();


//             if (soundAnimationInitialized)
//             {
//                 soundBar1.anchoredPosition =
//                     soundBar1OriginalPosition;
//             }
//         }


//         if (soundBar2 != null)
//         {
//             soundBar2.DOKill();


//             if (soundAnimationInitialized)
//             {
//                 soundBar2.anchoredPosition =
//                     soundBar2OriginalPosition;
//             }
//         }


//         if (speakerIcon != null)
//         {
//             speakerIcon.DOKill();


//             if (soundAnimationInitialized)
//             {
//                 speakerIcon.localScale =
//                     speakerOriginalScale;
//             }
//         }
//     }


//     // =========================================================
//     // HIDE MUSIC NOTES
//     // =========================================================

//     private void HideMusicNotesImmediate()
//     {
//         if (musicNotes == null)
//         {
//             return;
//         }

//         for (int i = 0; i < musicNotes.Length; i++)
//         {
//             if (musicNotes[i] != null)
//             {
//                 musicNotes[i].gameObject.SetActive(false);
//             }
//         }
//     }


//     // =========================================================
//     // KILL MUSIC ANIMATION
//     // =========================================================

//     private void KillMusicAnimation()
//     {
//         if (musicIcon != null)
//         {
//             musicIcon.DOKill();
//         }


//         if (musicNotes != null)
//         {
//             for (int i = 0;
//                  i < musicNotes.Length;
//                  i++)
//             {
//                 if (musicNotes[i] != null)
//                 {
//                     musicNotes[i].DOKill();
//                 }


//                 if (musicNoteCanvasGroups != null &&
//                     i < musicNoteCanvasGroups.Length &&
//                     musicNoteCanvasGroups[i] != null)
//                 {
//                     musicNoteCanvasGroups[i].DOKill();
//                 }
//             }
//         }


//         if (musicAnimationInitialized)
//         {
//             ResetMusicAnimationObjects();
//         }
//         else
//         {
//             HideMusicNotesImmediate();
//         }
//     }


//     // =========================================================
//     // VIBRATION ANIMATION
//     // =========================================================

//     private void CacheVibrationAnimationData()
//     {
//         if (vibrationTopIcon != null)
//         {
//             vibrationTopOriginalPosition =
//                 vibrationTopIcon.anchoredPosition;
//         }

//         if (vibrationBottomIcon != null)
//         {
//             vibrationBottomOriginalPosition =
//                 vibrationBottomIcon.anchoredPosition;
//         }

//         if (vibrationPhoneIcon != null)
//         {
//             vibrationPhoneOriginalScale =
//                 vibrationPhoneIcon.localScale;

//             vibrationPhoneCanvasGroup =
//                 vibrationPhoneIcon.GetComponent<CanvasGroup>();

//             if (vibrationPhoneCanvasGroup == null)
//             {
//                 vibrationPhoneCanvasGroup =
//                     vibrationPhoneIcon.gameObject.AddComponent<CanvasGroup>();
//             }
//         }

//         vibrationAnimationInitialized = true;
//     }


//     public void PlayVibrationAnimation()
//     {
//         if (!vibrationAnimationInitialized)
//         {
//             CacheVibrationAnimationData();
//         }

//         KillVibrationAnimation();

//         Sequence sequence = DOTween.Sequence();

//         // TOP: upar se neeche.
//         if (vibrationTopIcon != null)
//         {
//             vibrationTopIcon.gameObject.SetActive(true);

//             vibrationTopIcon.anchoredPosition =
//                 vibrationTopOriginalPosition +
//                 Vector2.up * vibrationEntryDistance;

//             sequence.Insert(
//                 0f,
//                 vibrationTopIcon
//                     .DOAnchorPos(
//                         vibrationTopOriginalPosition,
//                         vibrationEntryDuration
//                     )
//                     .SetEase(vibrationEntryEase)
//             );
//         }

//         // BOTTOM: neeche se upar.
//         if (vibrationBottomIcon != null)
//         {
//             vibrationBottomIcon.gameObject.SetActive(true);

//             vibrationBottomIcon.anchoredPosition =
//                 vibrationBottomOriginalPosition +
//                 Vector2.down * vibrationEntryDistance;

//             sequence.Insert(
//                 0f,
//                 vibrationBottomIcon
//                     .DOAnchorPos(
//                         vibrationBottomOriginalPosition,
//                         vibrationEntryDuration
//                     )
//                     .SetEase(vibrationEntryEase)
//             );
//         }

//         // CENTER PHONE: dono icons ke arrive hone ke baad fade in.
//         if (vibrationPhoneIcon != null)
//         {
//             vibrationPhoneIcon.gameObject.SetActive(true);

//             if (vibrationPhoneCanvasGroup == null)
//             {
//                 vibrationPhoneCanvasGroup =
//                     vibrationPhoneIcon.GetComponent<CanvasGroup>();
//             }

//             if (vibrationPhoneCanvasGroup != null)
//             {
//                 vibrationPhoneCanvasGroup.alpha = 0f;

//                 sequence.Insert(
//                     vibrationEntryDuration +
//                     vibrationAnimationDelay,
//                     vibrationPhoneCanvasGroup
//                         .DOFade(
//                             1f,
//                             vibrationPhoneFadeDuration
//                         )
//                         .SetEase(vibrationPhoneFadeEase)
//                 );
//             }

//             // Fade complete hone ke baad phone vibrate karega.
//             sequence.Insert(
//                 vibrationEntryDuration +
//                 vibrationAnimationDelay +
//                 vibrationPhoneFadeDuration,
//                 vibrationPhoneIcon
//                     .DOShakeAnchorPos(
//                         vibrationShakeDuration,
//                         vibrationShakeStrength,
//                         vibrationShakeVibrato,
//                         90f,
//                         false,
//                         true
//                     )
//             );
//         }

//         // Complete hone ke baad teeno objects hidden.
//         // sequence.OnComplete(
//         //     HideVibrationAnimationImmediate
//         // );

//         sequence.Play();
//     }


//     private void HideVibrationAnimationImmediate()
//     {
//         if (vibrationTopIcon != null)
//         {
//             vibrationTopIcon.DOKill();

//             if (vibrationAnimationInitialized)
//             {
//                 vibrationTopIcon.anchoredPosition =
//                     vibrationTopOriginalPosition;
//             }

//             vibrationTopIcon.gameObject.SetActive(false);
//         }

//         if (vibrationBottomIcon != null)
//         {
//             vibrationBottomIcon.DOKill();

//             if (vibrationAnimationInitialized)
//             {
//                 vibrationBottomIcon.anchoredPosition =
//                     vibrationBottomOriginalPosition;
//             }

//             vibrationBottomIcon.gameObject.SetActive(false);
//         }

//         if (vibrationPhoneIcon != null)
//         {
//             vibrationPhoneIcon.DOKill();

//             if (vibrationAnimationInitialized)
//             {
//                 vibrationPhoneIcon.localScale =
//                     vibrationPhoneOriginalScale;
//             }

//             if (vibrationPhoneCanvasGroup != null)
//             {
//                 vibrationPhoneCanvasGroup.DOKill();
//                 vibrationPhoneCanvasGroup.alpha = 0f;
//             }

//             vibrationPhoneIcon.gameObject.SetActive(false);
//         }
//     }


//     private void KillVibrationAnimation()
//     {
//         if (vibrationTopIcon != null)
//         {
//             vibrationTopIcon.DOKill();
//         }

//         if (vibrationBottomIcon != null)
//         {
//             vibrationBottomIcon.DOKill();
//         }

//         if (vibrationPhoneIcon != null)
//         {
//             vibrationPhoneIcon.DOKill();
//         }

//         if (vibrationPhoneCanvasGroup != null)
//         {
//             vibrationPhoneCanvasGroup.DOKill();
//         }

//         if (vibrationAnimationInitialized)
//         {
//             HideVibrationAnimationImmediate();
//         }
//     }


//     // =========================================================
//     // UI POSITION HELPER
//     // =========================================================

//     private static Vector2 GetAnchoredPositionRelativeToParent(
//         RectTransform target,
//         RectTransform targetParent
//     )
//     {
//         if (target == null ||
//             targetParent == null)
//         {
//             return Vector2.zero;
//         }


//         Vector3 worldPosition =
//             target.position;


//         Vector3 localPosition =
//             targetParent.InverseTransformPoint(
//                 worldPosition
//             );


//         return new Vector2(
//             localPosition.x,
//             localPosition.y
//         );
//     }
// }




// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;

// public sealed class SettingsUIController : MonoBehaviour
// {
//     [Header("Settings Popup")]
//     [SerializeField] private GameObject settingsPanel;
//     [SerializeField] private Button closeButton;


//     // =========================================================
//     // SOUND
//     // =========================================================

//     [Header("Sound Button")]
//     [SerializeField] private Button soundButton;
//     [SerializeField] private GameObject soundOnUI;
//     [SerializeField] private GameObject soundOffUI;


//     [Header("Sound Animation")]
//     [Tooltip("Speaker icon jo scale animation karega.")]
//     [SerializeField] private RectTransform speakerIcon;

//     [Tooltip("Pehli sound bar jo right side move karegi.")]
//     [SerializeField] private RectTransform soundBar1;

//     [Tooltip("Dusri sound bar jo right side move karegi.")]
//     [SerializeField] private RectTransform soundBar2;

//     [SerializeField, Min(0f)]
//     private float soundBarMoveDistance = 12f;

//     [SerializeField, Min(0.01f)]
//     private float soundBarMoveDuration = 0.18f;

//     [SerializeField, Min(1f)]
//     private float speakerScaleMultiplier = 1.12f;

//     [SerializeField, Min(0.01f)]
//     private float speakerScaleDuration = 0.18f;

//     [SerializeField, Range(1, 5)]
//     private int soundAnimationLoops = 2;

//     [SerializeField, Min(0f)]
//     private float soundAnimationLoopDelay = 0.04f;

//     [SerializeField]
//     private Ease soundBarEase = Ease.OutQuad;

//     [SerializeField]
//     private Ease speakerEase = Ease.OutBack;


//     // =========================================================
//     // MUSIC
//     // =========================================================

//     [Header("Music Button")]
//     [SerializeField] private Button musicButton;
//     [SerializeField] private GameObject musicOnUI;
//     [SerializeField] private GameObject musicOffUI;


//     [Header("Music Animation")]
//     [Tooltip(
//         "Main music icon. Ye scale/bounce animation karega."
//     )]
//     [SerializeField] private RectTransform musicIcon;


//     [Tooltip(
//         "5 ya 6 small music icons/notes assign karein. " +
//         "Ye sab main music icon ki position se start hongi."
//     )]
//     [SerializeField] private RectTransform[] musicNotes;


//     [Tooltip(
//         "Small music icons kitne pixels upar jayengi."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteMoveDistance = 70f;


//     [Tooltip(
//         "Small music icons ko left/right kitna spread milega."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteHorizontalDrift = 25f;


//     [Tooltip(
//         "Small music icon ko upar jane mein kitna time lagega."
//     )]
//     [SerializeField, Min(0.05f)]
//     private float musicNoteMoveDuration = 0.65f;


//     [Tooltip(
//         "Har note ke beech chhota stagger delay."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicNoteStagger = 0.035f;


//     [Tooltip(
//         "Small music icons fade out hongi."
//     )]
//     [SerializeField]
//     private bool musicNotesFadeOut = true;


//     [Tooltip(
//         "Movement ke kitne percent par fade start hogi."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float musicNoteFadeStart = 0.45f;


//     [Tooltip(
//         "Main music icon kitna scale hoga."
//     )]
//     [SerializeField, Min(1f)]
//     private float musicIconScaleMultiplier = 1.12f;


//     [Tooltip(
//         "Main music icon scale animation duration."
//     )]
//     [SerializeField, Min(0.01f)]
//     private float musicIconScaleDuration = 0.18f;


//     [Tooltip(
//         "Music animation kitni baar repeat hogi."
//     )]
//     [SerializeField, Range(1, 5)]
//     private int musicAnimationLoops = 2;


//     [Tooltip(
//         "Ek complete music animation ke baad doosri repetition " +
//         "se pehle interval."
//     )]
//     [SerializeField, Min(0f)]
//     private float musicAnimationLoopDelay = 0.08f;


//     [SerializeField]
//     private Ease musicNoteEase = Ease.OutQuad;


//     [SerializeField]
//     private Ease musicIconEase = Ease.OutBack;



//     // =========================================================
//     // BELL / NOTIFICATION ANIMATION
//     // =========================================================

//     [Header("Bell Animation")]
//     [Tooltip(
//         "Main bell icon. Ye left/right move karega."
//     )]
//     [SerializeField]
//     private RectTransform bellIcon;

//     [Tooltip(
//         "Bell ke andar wala small clapper icon. " +
//         "Bell right jaye to ye left jayega aur bell left jaye to ye right jayega."
//     )]
//     [SerializeField]
//     private RectTransform bellClapperIcon;

//     [Tooltip(
//         "Bell aur clapper kitne pixels left/right move karenge."
//     )]
//     [SerializeField, Min(0f)]
//     private float bellMoveDistance = 8f;

//     [Tooltip(
//         "Bell ke ek side move hone ka duration."
//     )]
//     [SerializeField, Min(0.01f)]
//     private float bellMoveDuration = 0.12f;

//     [Tooltip(
//         "Bell animation kitni complete shakes karegi."
//     )]
//     [SerializeField, Range(1, 6)]
//     private int bellAnimationLoops = 3;

//     [Tooltip(
//         "Ek complete left/right shake ke baad next shake se pehle interval."
//     )]
//     [SerializeField, Min(0f)]
//     private float bellAnimationLoopDelay = 0.04f;

//     [Tooltip(
//         "Clapper bell ke opposite direction mein kitna move kare."
//     )]
//     [SerializeField, Range(0f, 1.5f)]
//     private float bellClapperOppositeMultiplier = 1f;

//     [SerializeField]
//     private Ease bellMoveEase = Ease.InOutSine;


//     // =========================================================
//     // VIBRATION
//     // =========================================================

//     [Header("Vibration Button")]
//     [SerializeField] private Button vibrationButton;
//     [SerializeField] private GameObject vibrationOnUI;
//     [SerializeField] private GameObject vibrationOffUI;


//     [Header("Vibration Animation")]
//     [Tooltip("Vibration ka top icon. Animation mein upar se neeche ayega.")]
//     [SerializeField] private RectTransform vibrationTopIcon;

//     [Tooltip("Vibration ka bottom icon. Animation mein neeche se upar ayega.")]
//     [SerializeField] private RectTransform vibrationBottomIcon;

//     [Tooltip("Center ka main phone/vibration icon.")]
//     [SerializeField] private RectTransform vibrationPhoneIcon;

//     [SerializeField, Min(0f)]
//     private float vibrationEntryDistance = 45f;

//     [SerializeField, Min(0.01f)]
//     private float vibrationEntryDuration = 0.25f;

//     [SerializeField, Min(0.01f)]
//     private float vibrationPhoneFadeDuration = 0.25f;

//     [SerializeField, Min(0.01f)]
//     private float vibrationShakeDuration = 0.45f;

//     [SerializeField, Min(0.01f)]
//     private float vibrationShakeStrength = 5f;

//     [SerializeField, Min(1)]
//     private int vibrationShakeVibrato = 12;

//     [SerializeField, Min(0f)]
//     private float vibrationAnimationDelay = 0.05f;

//     [SerializeField]
//     private Ease vibrationEntryEase = Ease.OutBack;

//     [SerializeField]
//     private Ease vibrationPhoneFadeEase = Ease.OutQuad;


//     // =========================================================
//     // MANAGERS
//     // =========================================================

//     [Header("Managers")]
//     [SerializeField] private AudioManager audioManager;
//     [SerializeField] private VibrationManager vibrationManager;


//     // =========================================================
//     // INTERNAL ANIMATION DATA
//     // =========================================================

//     private Vector2 soundBar1OriginalPosition;
//     private Vector2 soundBar2OriginalPosition;

//     private Vector3 speakerOriginalScale;

//     private Vector3 musicIconOriginalScale;

//     /*
//      * Notes ki authored/original Inspector positions.
//      * Animation complete hone par yahin restore hongi.
//      */
//     private Vector2[] musicNoteOriginalPositions;

//     private Vector3[] musicNoteOriginalScales;


//     /*
//      * Runtime start position.
//      *
//      * Har small music note animation ke waqt
//      * main music icon ki position se start karegi.
//      */
//     private Vector2[] musicNoteStartPositions;


//     private CanvasGroup[] musicNoteCanvasGroups;

//     private bool soundAnimationInitialized;
//     private bool musicAnimationInitialized;

//     private Vector2 bellOriginalPosition;
//     private Vector2 bellClapperOriginalPosition;
//     private bool bellAnimationInitialized;

//     private Vector2 vibrationTopOriginalPosition;
//     private Vector2 vibrationBottomOriginalPosition;
//     private Vector3 vibrationPhoneOriginalScale;
//     private CanvasGroup vibrationPhoneCanvasGroup;
//     private bool vibrationAnimationInitialized;


//     // =========================================================
//     // AWAKE
//     // =========================================================

//     private void Awake()
//     {
//         if (settingsPanel != null)
//         {
//             UITransition.HideImmediate(settingsPanel);
//         }

//         CacheSoundAnimationPositions();

//         CacheMusicAnimationData();
//         HideMusicNotesImmediate();

//         CacheVibrationAnimationData();
//         HideVibrationAnimationImmediate();

//         CacheBellAnimationData();
//     }


//     // =========================================================
//     // ENABLE / DISABLE
//     // =========================================================

//     private void OnEnable()
//     {
//         ResolveReferences();

//         UIEventBroker.OnScreenChangeRequested +=
//             HandleScreenChange;

//         AddListeners();

//         RefreshButtonStates();
//     }


//     private void OnDisable()
//     {
//         UIEventBroker.OnScreenChangeRequested -=
//             HandleScreenChange;

//         RemoveListeners();

//         KillSoundAnimation();

//         KillMusicAnimation();

//         KillVibrationAnimation();

//         KillBellAnimation();
//     }


//     // =========================================================
//     // REFERENCES
//     // =========================================================

//     private void ResolveReferences()
//     {
//         if (audioManager == null)
//         {
//             audioManager =
//                 FindFirstObjectByType<AudioManager>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (vibrationManager == null)
//         {
//             vibrationManager =
//                 FindFirstObjectByType<VibrationManager>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     // =========================================================
//     // LISTENERS
//     // =========================================================

//     private void AddListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );

//             closeButton.onClick.AddListener(
//                 CloseSettings
//             );
//         }


//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );

//             soundButton.onClick.AddListener(
//                 ToggleSound
//             );
//         }


//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );

//             musicButton.onClick.AddListener(
//                 ToggleMusic
//             );
//         }


//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );

//             vibrationButton.onClick.AddListener(
//                 ToggleVibration
//             );
//         }
//     }


//     private void RemoveListeners()
//     {
//         if (closeButton != null)
//         {
//             closeButton.onClick.RemoveListener(
//                 CloseSettings
//             );
//         }


//         if (soundButton != null)
//         {
//             soundButton.onClick.RemoveListener(
//                 ToggleSound
//             );
//         }


//         if (musicButton != null)
//         {
//             musicButton.onClick.RemoveListener(
//                 ToggleMusic
//             );
//         }


//         if (vibrationButton != null)
//         {
//             vibrationButton.onClick.RemoveListener(
//                 ToggleVibration
//             );
//         }
//     }


//     // =========================================================
//     // SCREEN CHANGE
//     // =========================================================

//     private void HandleScreenChange(
//         UIScreenType targetScreen
//     )
//     {
//         if (targetScreen ==
//             UIScreenType.SettingScreen)
//         {
//             OpenSettings();

//             return;
//         }


//         if (targetScreen ==
//             UIScreenType.MainMenu)
//         {
//             HideSettings();
//         }
//     }


//     // =========================================================
//     // OPEN SETTINGS
//     // =========================================================

//     public void OpenSettings()
//     {
//         ResolveReferences();

//         RefreshButtonStates();


//         if (settingsPanel != null)
//         {
//             UITransition.Show(
//                 settingsPanel
//             );
//         }


//         /*
//          * Settings open hote hi animations.
//          */
//         PlaySoundAnimation();

//         PlayMusicAnimation();

//         PlayVibrationAnimation();

//         PlayBellAnimation();
//     }


//     // =========================================================
//     // CLOSE SETTINGS
//     // =========================================================

//     public void CloseSettings()
//     {
//         KillSoundAnimation();

//         KillMusicAnimation();

//         KillVibrationAnimation();

//         KillBellAnimation();

//         HideSettings();


//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );
//     }


//     public void HideSettings()
//     {
//         KillSoundAnimation();

//         KillMusicAnimation();

//         KillVibrationAnimation();

//         KillBellAnimation();


//         if (settingsPanel != null)
//         {
//             UITransition.Hide(
//                 settingsPanel
//             );
//         }
//     }


//     // =========================================================
//     // SOUND TOGGLE
//     // =========================================================

//     private void ToggleSound()
//     {
//         ResolveReferences();


//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }


//         bool newState =
//             !audioManager.IsSoundEnabled;


//         audioManager.SetSoundEnabled(
//             newState
//         );


//         RefreshSoundUI();

//         PlaySoundAnimation();
//     }


//     // =========================================================
//     // MUSIC TOGGLE
//     // =========================================================

//     private void ToggleMusic()
//     {
//         ResolveReferences();


//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }


//         bool newState =
//             !audioManager.IsMusicEnabled;


//         audioManager.SetMusicEnabled(
//             newState
//         );


//         RefreshMusicUI();

//         PlayMusicAnimation();
//     }


//     // =========================================================
//     // VIBRATION TOGGLE
//     // =========================================================

//     private void ToggleVibration()
//     {
//         ResolveReferences();


//         if (vibrationManager == null)
//         {
//             Debug.LogWarning(
//                 "SettingsUIController: VibrationManager missing hai.",
//                 this
//             );

//             return;
//         }


//         bool newState =
//             !vibrationManager.IsVibrationEnabled;


//         vibrationManager.SetVibrationEnabled(
//             newState
//         );


//         RefreshVibrationUI();

//         PlayVibrationAnimation();
//     }


//     // =========================================================
//     // REFRESH STATES
//     // =========================================================

//     private void RefreshButtonStates()
//     {
//         RefreshSoundUI();

//         RefreshMusicUI();

//         RefreshVibrationUI();
//     }


//     private void RefreshSoundUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsSoundEnabled;


//         SetStateUI(
//             soundOnUI,
//             soundOffUI,
//             isOn
//         );
//     }


//     private void RefreshMusicUI()
//     {
//         bool isOn =
//             audioManager != null &&
//             audioManager.IsMusicEnabled;


//         SetStateUI(
//             musicOnUI,
//             musicOffUI,
//             isOn
//         );
//     }


//     private void RefreshVibrationUI()
//     {
//         bool isOn =
//             vibrationManager != null &&
//             vibrationManager.IsVibrationEnabled;


//         SetStateUI(
//             vibrationOnUI,
//             vibrationOffUI,
//             isOn
//         );
//     }


//     private static void SetStateUI(
//         GameObject onUI,
//         GameObject offUI,
//         bool isOn
//     )
//     {
//         if (onUI != null)
//         {
//             onUI.SetActive(
//                 isOn
//             );
//         }


//         if (offUI != null)
//         {
//             offUI.SetActive(
//                 !isOn
//             );
//         }
//     }


//     // =========================================================
//     // SOUND ANIMATION
//     // =========================================================

//     private void CacheSoundAnimationPositions()
//     {
//         if (soundBar1 != null)
//         {
//             soundBar1OriginalPosition =
//                 soundBar1.anchoredPosition;
//         }


//         if (soundBar2 != null)
//         {
//             soundBar2OriginalPosition =
//                 soundBar2.anchoredPosition;
//         }


//         if (speakerIcon != null)
//         {
//             speakerOriginalScale =
//                 speakerIcon.localScale;
//         }


//         soundAnimationInitialized = true;
//     }


//     public void PlaySoundAnimation()
//     {
//         if (!soundAnimationInitialized)
//         {
//             CacheSoundAnimationPositions();
//         }


//         KillSoundAnimation();


//         Sequence animation =
//             DOTween.Sequence();


//         // ---------------------------------------------------------
//         // BAR 1
//         // ---------------------------------------------------------

//         if (soundBar1 != null)
//         {
//             soundBar1.anchoredPosition =
//                 soundBar1OriginalPosition;


//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 Vector2 rightPosition =
//                     soundBar1OriginalPosition +
//                     Vector2.right *
//                     soundBarMoveDistance;


//                 animation.Append(
//                     soundBar1
//                         .DOAnchorPos(
//                             rightPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 animation.Append(
//                     soundBar1
//                         .DOAnchorPos(
//                             soundBar1OriginalPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     animation.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }
//         }


//         // ---------------------------------------------------------
//         // BAR 2
//         // ---------------------------------------------------------

//         if (soundBar2 != null)
//         {
//             soundBar2.anchoredPosition =
//                 soundBar2OriginalPosition;


//             Sequence bar2Sequence =
//                 DOTween.Sequence();


//             bar2Sequence.AppendInterval(
//                 soundBarMoveDuration *
//                 0.35f
//             );


//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 Vector2 rightPosition =
//                     soundBar2OriginalPosition +
//                     Vector2.right *
//                     soundBarMoveDistance;


//                 bar2Sequence.Append(
//                     soundBar2
//                         .DOAnchorPos(
//                             rightPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 bar2Sequence.Append(
//                     soundBar2
//                         .DOAnchorPos(
//                             soundBar2OriginalPosition,
//                             soundBarMoveDuration
//                         )
//                         .SetEase(
//                             soundBarEase
//                         )
//                 );


//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     bar2Sequence.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }


//             animation.Insert(
//                 0f,
//                 bar2Sequence
//             );
//         }


//         // ---------------------------------------------------------
//         // SPEAKER
//         // ---------------------------------------------------------

//         if (speakerIcon != null)
//         {
//             speakerIcon.localScale =
//                 speakerOriginalScale;


//             Vector3 enlargedScale =
//                 speakerOriginalScale *
//                 speakerScaleMultiplier;


//             Sequence speakerSequence =
//                 DOTween.Sequence();


//             for (int i = 0;
//                  i < soundAnimationLoops;
//                  i++)
//             {
//                 speakerSequence.Append(
//                     speakerIcon
//                         .DOScale(
//                             enlargedScale,
//                             speakerScaleDuration
//                         )
//                         .SetEase(
//                             speakerEase
//                         )
//                 );


//                 speakerSequence.Append(
//                     speakerIcon
//                         .DOScale(
//                             speakerOriginalScale,
//                             speakerScaleDuration
//                         )
//                         .SetEase(
//                             Ease.OutQuad
//                         )
//                 );


//                 if (soundAnimationLoopDelay > 0f)
//                 {
//                     speakerSequence.AppendInterval(
//                         soundAnimationLoopDelay
//                     );
//                 }
//             }


//             animation.Insert(
//                 0f,
//                 speakerSequence
//             );
//         }


//         animation.Play();
//     }


//     // =========================================================
//     // MUSIC ANIMATION DATA
//     // =========================================================

//     private void CacheMusicAnimationData()
//     {
//         if (musicIcon != null)
//         {
//             musicIconOriginalScale =
//                 musicIcon.localScale;
//         }


//         if (musicNotes == null ||
//             musicNotes.Length == 0)
//         {
//             musicAnimationInitialized = true;

//             return;
//         }


//         musicNoteOriginalPositions =
//             new Vector2[
//                 musicNotes.Length
//             ];


//         musicNoteOriginalScales =
//             new Vector3[
//                 musicNotes.Length
//             ];


//         musicNoteStartPositions =
//             new Vector2[
//                 musicNotes.Length
//             ];


//         musicNoteCanvasGroups =
//             new CanvasGroup[
//                 musicNotes.Length
//             ];


//         for (int i = 0;
//              i < musicNotes.Length;
//              i++)
//         {
//             RectTransform note =
//                 musicNotes[i];


//             if (note == null)
//             {
//                 continue;
//             }


//             /*
//              * Save original Inspector position.
//              */
//             musicNoteOriginalPositions[i] =
//                 note.anchoredPosition;


//             musicNoteOriginalScales[i] =
//                 note.localScale;


//             /*
//              * Calculate main music icon position
//              * relative to each note's parent.
//              */
//             musicNoteStartPositions[i] =
//                 GetAnchoredPositionRelativeToParent(
//                     musicIcon,
//                     note.parent as RectTransform
//                 );


//             CanvasGroup canvasGroup =
//                 note.GetComponent<CanvasGroup>();


//             if (canvasGroup == null)
//             {
//                 canvasGroup =
//                     note.gameObject.AddComponent<CanvasGroup>();
//             }


//             musicNoteCanvasGroups[i] =
//                 canvasGroup;
//         }


//         musicAnimationInitialized = true;
//     }


//     // =========================================================
//     // MUSIC ANIMATION
//     // =========================================================

//     public void PlayMusicAnimation()
//     {
//         if (!musicAnimationInitialized)
//         {
//             CacheMusicAnimationData();
//         }


//         KillMusicAnimation();


//         Sequence musicSequence =
//             DOTween.Sequence();


//         // ---------------------------------------------------------
//         // SMALL MUSIC ICONS / NOTES
//         // ---------------------------------------------------------

//         if (musicNotes != null &&
//             musicNotes.Length > 0)
//         {
//             for (int i = 0;
//                  i < musicNotes.Length;
//                  i++)
//             {
//                 RectTransform note =
//                     musicNotes[i];


//                 if (note == null)
//                 {
//                     continue;
//                 }

//                 // Small music notes normally hidden rehti hain.
//                 // Animation start hote hi enable hongi.
//                 note.gameObject.SetActive(true);


//                 /*
//                  * IMPORTANT:
//                  *
//                  * Small note main music icon ki
//                  * exact position se start karegi.
//                  */
//                 note.anchoredPosition =
//                     musicNoteStartPositions[i];


//                 note.localScale =
//                     musicNoteOriginalScales[i];


//                 CanvasGroup canvasGroup =
//                     musicNoteCanvasGroups[i];


//                 if (canvasGroup != null)
//                 {
//                     canvasGroup.alpha = 1f;
//                 }


//                 /*
//                  * Left/right spread.
//                  *
//                  * 0 = left side
//                  * middle = center
//                  * last = right side
//                  */
//                 float normalizedIndex =
//                     musicNotes.Length > 1
//                         ? (float)i /
//                           (musicNotes.Length - 1)
//                         : 0.5f;


//                 float spreadDirection =
//                     Mathf.Lerp(
//                         -1f,
//                         1f,
//                         normalizedIndex
//                     );


//                 float randomDrift =
//                     Random.Range(
//                         -musicNoteHorizontalDrift,
//                         musicNoteHorizontalDrift
//                     );


//                 randomDrift +=
//                     spreadDirection *
//                     musicNoteHorizontalDrift *
//                     0.35f;


//                 Vector2 targetPosition =
//                     musicNoteStartPositions[i] +
//                     new Vector2(
//                         randomDrift,
//                         musicNoteMoveDistance
//                     );


//                 /*
//                  * Har note slightly different time par start hogi.
//                  */
//                 float noteDelay =
//                     i *
//                     musicNoteStagger;


//                 Tween moveTween =
//                     note
//                         .DOAnchorPos(
//                             targetPosition,
//                             musicNoteMoveDuration
//                         )
//                         .SetEase(
//                             musicNoteEase
//                         );


//                 musicSequence.Insert(
//                     noteDelay,
//                     moveTween
//                 );


//                 // -------------------------------------------------
//                 // FADE
//                 // -------------------------------------------------

//                 if (musicNotesFadeOut &&
//                     canvasGroup != null)
//                 {
//                     float fadeDelay =
//                         noteDelay +
//                         musicNoteMoveDuration *
//                         musicNoteFadeStart;


//                     float fadeDuration =
//                         musicNoteMoveDuration *
//                         (1f -
//                          musicNoteFadeStart);


//                     Tween fadeTween =
//                         canvasGroup
//                             .DOFade(
//                                 0f,
//                                 Mathf.Max(
//                                     0.05f,
//                                     fadeDuration
//                                 )
//                             );


//                     musicSequence.Insert(
//                         fadeDelay,
//                         fadeTween
//                     );
//                 }
//             }
//         }


//         // ---------------------------------------------------------
//         // MAIN MUSIC ICON
//         // ---------------------------------------------------------

//         if (musicIcon != null)
//         {
//             musicIcon.localScale =
//                 musicIconOriginalScale;


//             Vector3 enlargedScale =
//                 musicIconOriginalScale *
//                 musicIconScaleMultiplier;


//             Sequence iconSequence =
//                 DOTween.Sequence();


//             for (int i = 0;
//                  i < musicAnimationLoops;
//                  i++)
//             {
//                 iconSequence.Append(
//                     musicIcon
//                         .DOScale(
//                             enlargedScale,
//                             musicIconScaleDuration
//                         )
//                         .SetEase(
//                             musicIconEase
//                         )
//                 );


//                 iconSequence.Append(
//                     musicIcon
//                         .DOScale(
//                             musicIconOriginalScale,
//                             musicIconScaleDuration
//                         )
//                         .SetEase(
//                             Ease.OutQuad
//                         )
//                 );


//                 /*
//                  * Important:
//                  *
//                  * Ek animation complete hone ke baad
//                  * next animation se pehle interval.
//                  */
//                 if (musicAnimationLoopDelay > 0f)
//                 {
//                     iconSequence.AppendInterval(
//                         musicAnimationLoopDelay
//                     );
//                 }
//             }


//             musicSequence.Insert(
//                 0f,
//                 iconSequence
//             );
//         }


//         musicSequence.OnComplete(
//             ResetMusicAnimationObjects
//         );


//         musicSequence.Play();
//     }


//     // =========================================================
//     // RESET MUSIC ANIMATION
//     // =========================================================

//     private void ResetMusicAnimationObjects()
//     {
//         if (musicIcon != null)
//         {
//             musicIcon.localScale =
//                 musicIconOriginalScale;
//         }


//         if (musicNotes == null)
//         {
//             return;
//         }


//         for (int i = 0;
//              i < musicNotes.Length;
//              i++)
//         {
//             RectTransform note =
//                 musicNotes[i];


//             if (note == null)
//             {
//                 continue;
//             }


//             /*
//              * Animation complete hone ke baad
//              * notes apni Inspector positions par wapas.
//              */
//             note.anchoredPosition =
//                 musicNoteOriginalPositions[i];


//             note.localScale =
//                 musicNoteOriginalScales[i];


//             CanvasGroup canvasGroup =
//                 musicNoteCanvasGroups[i];


//             if (canvasGroup != null)
//             {
//                 canvasGroup.alpha = 1f;
//             }

//             // Animation complete hone ke baad note dobara hidden.
//             note.gameObject.SetActive(false);
//         }
//     }


//     // =========================================================
//     // KILL SOUND ANIMATION
//     // =========================================================

//     private void KillSoundAnimation()
//     {
//         if (soundBar1 != null)
//         {
//             soundBar1.DOKill();


//             if (soundAnimationInitialized)
//             {
//                 soundBar1.anchoredPosition =
//                     soundBar1OriginalPosition;
//             }
//         }


//         if (soundBar2 != null)
//         {
//             soundBar2.DOKill();


//             if (soundAnimationInitialized)
//             {
//                 soundBar2.anchoredPosition =
//                     soundBar2OriginalPosition;
//             }
//         }


//         if (speakerIcon != null)
//         {
//             speakerIcon.DOKill();


//             if (soundAnimationInitialized)
//             {
//                 speakerIcon.localScale =
//                     speakerOriginalScale;
//             }
//         }
//     }


//     // =========================================================
//     // HIDE MUSIC NOTES
//     // =========================================================

//     private void HideMusicNotesImmediate()
//     {
//         if (musicNotes == null)
//         {
//             return;
//         }

//         for (int i = 0; i < musicNotes.Length; i++)
//         {
//             if (musicNotes[i] != null)
//             {
//                 musicNotes[i].gameObject.SetActive(false);
//             }
//         }
//     }


//     // =========================================================
//     // KILL MUSIC ANIMATION
//     // =========================================================

//     private void KillMusicAnimation()
//     {
//         if (musicIcon != null)
//         {
//             musicIcon.DOKill();
//         }


//         if (musicNotes != null)
//         {
//             for (int i = 0;
//                  i < musicNotes.Length;
//                  i++)
//             {
//                 if (musicNotes[i] != null)
//                 {
//                     musicNotes[i].DOKill();
//                 }


//                 if (musicNoteCanvasGroups != null &&
//                     i < musicNoteCanvasGroups.Length &&
//                     musicNoteCanvasGroups[i] != null)
//                 {
//                     musicNoteCanvasGroups[i].DOKill();
//                 }
//             }
//         }


//         if (musicAnimationInitialized)
//         {
//             ResetMusicAnimationObjects();
//         }
//         else
//         {
//             HideMusicNotesImmediate();
//         }
//     }


//     // =========================================================
//     // BELL ANIMATION
//     // =========================================================

//     private void CacheBellAnimationData()
//     {
//         if (bellIcon != null)
//         {
//             bellOriginalPosition =
//                 bellIcon.anchoredPosition;
//         }

//         if (bellClapperIcon != null)
//         {
//             bellClapperOriginalPosition =
//                 bellClapperIcon.anchoredPosition;
//         }

//         bellAnimationInitialized = true;
//     }


//     public void PlayBellAnimation()
//     {
//         if (!bellAnimationInitialized)
//         {
//             CacheBellAnimationData();
//         }

//         KillBellAnimation();

//         if (bellIcon == null &&
//             bellClapperIcon == null)
//         {
//             return;
//         }

//         if (bellIcon != null)
//         {
//             bellIcon.anchoredPosition =
//                 bellOriginalPosition;
//         }

//         if (bellClapperIcon != null)
//         {
//             bellClapperIcon.anchoredPosition =
//                 bellClapperOriginalPosition;
//         }

//         Sequence bellSequence =
//             DOTween.Sequence();

//         for (int i = 0;
//              i < bellAnimationLoops;
//              i++)
//         {
//             Vector2 bellRightPosition =
//                 bellOriginalPosition +
//                 Vector2.right * bellMoveDistance;

//             Vector2 clapperLeftPosition =
//                 bellClapperOriginalPosition -
//                 Vector2.right *
//                 (
//                     bellMoveDistance *
//                     bellClapperOppositeMultiplier
//                 );

//             if (bellIcon != null)
//             {
//                 bellSequence.Append(
//                     bellIcon
//                         .DOAnchorPos(
//                             bellRightPosition,
//                             bellMoveDuration
//                         )
//                         .SetEase(bellMoveEase)
//                 );
//             }

//             if (bellClapperIcon != null)
//             {
//                 bellSequence.Join(
//                     bellClapperIcon
//                         .DOAnchorPos(
//                             clapperLeftPosition,
//                             bellMoveDuration
//                         )
//                         .SetEase(bellMoveEase)
//                 );
//             }


//             Vector2 bellLeftPosition =
//                 bellOriginalPosition -
//                 Vector2.right * bellMoveDistance;

//             Vector2 clapperRightPosition =
//                 bellClapperOriginalPosition +
//                 Vector2.right *
//                 (
//                     bellMoveDistance *
//                     bellClapperOppositeMultiplier
//                 );

//             if (bellIcon != null)
//             {
//                 bellSequence.Append(
//                     bellIcon
//                         .DOAnchorPos(
//                             bellLeftPosition,
//                             bellMoveDuration
//                         )
//                         .SetEase(bellMoveEase)
//                 );
//             }

//             if (bellClapperIcon != null)
//             {
//                 bellSequence.Join(
//                     bellClapperIcon
//                         .DOAnchorPos(
//                             clapperRightPosition,
//                             bellMoveDuration
//                         )
//                         .SetEase(bellMoveEase)
//                 );
//             }


//             if (bellIcon != null)
//             {
//                 bellSequence.Append(
//                     bellIcon
//                         .DOAnchorPos(
//                             bellOriginalPosition,
//                             bellMoveDuration
//                         )
//                         .SetEase(bellMoveEase)
//                 );
//             }

//             if (bellClapperIcon != null)
//             {
//                 bellSequence.Join(
//                     bellClapperIcon
//                         .DOAnchorPos(
//                             bellClapperOriginalPosition,
//                             bellMoveDuration
//                         )
//                         .SetEase(bellMoveEase)
//                 );
//             }


//             if (bellAnimationLoopDelay > 0f)
//             {
//                 bellSequence.AppendInterval(
//                     bellAnimationLoopDelay
//                 );
//             }
//         }


//         // Animation complete hone ke baad dono icons visible
//         // aur apni original Inspector positions par rahenge.
//         bellSequence.OnComplete(() =>
//         {
//             if (bellIcon != null)
//             {
//                 bellIcon.anchoredPosition =
//                     bellOriginalPosition;
//             }

//             if (bellClapperIcon != null)
//             {
//                 bellClapperIcon.anchoredPosition =
//                     bellClapperOriginalPosition;
//             }
//         });

//         bellSequence.Play();
//     }


//     private void KillBellAnimation()
//     {
//         if (bellIcon != null)
//         {
//             bellIcon.DOKill();

//             if (bellAnimationInitialized)
//             {
//                 bellIcon.anchoredPosition =
//                     bellOriginalPosition;
//             }
//         }

//         if (bellClapperIcon != null)
//         {
//             bellClapperIcon.DOKill();

//             if (bellAnimationInitialized)
//             {
//                 bellClapperIcon.anchoredPosition =
//                     bellClapperOriginalPosition;
//             }
//         }
//     }


//     // =========================================================
//     // VIBRATION ANIMATION
//     // =========================================================

//     private void CacheVibrationAnimationData()
//     {
//         if (vibrationTopIcon != null)
//         {
//             vibrationTopOriginalPosition =
//                 vibrationTopIcon.anchoredPosition;
//         }

//         if (vibrationBottomIcon != null)
//         {
//             vibrationBottomOriginalPosition =
//                 vibrationBottomIcon.anchoredPosition;
//         }

//         if (vibrationPhoneIcon != null)
//         {
//             vibrationPhoneOriginalScale =
//                 vibrationPhoneIcon.localScale;

//             vibrationPhoneCanvasGroup =
//                 vibrationPhoneIcon.GetComponent<CanvasGroup>();

//             if (vibrationPhoneCanvasGroup == null)
//             {
//                 vibrationPhoneCanvasGroup =
//                     vibrationPhoneIcon.gameObject.AddComponent<CanvasGroup>();
//             }
//         }

//         vibrationAnimationInitialized = true;
//     }


//     public void PlayVibrationAnimation()
//     {
//         if (!vibrationAnimationInitialized)
//         {
//             CacheVibrationAnimationData();
//         }

//         KillVibrationAnimation();

//         Sequence sequence = DOTween.Sequence();

//         // TOP: upar se neeche.
//         if (vibrationTopIcon != null)
//         {
//             vibrationTopIcon.gameObject.SetActive(true);

//             vibrationTopIcon.anchoredPosition =
//                 vibrationTopOriginalPosition +
//                 Vector2.up * vibrationEntryDistance;

//             sequence.Insert(
//                 0f,
//                 vibrationTopIcon
//                     .DOAnchorPos(
//                         vibrationTopOriginalPosition,
//                         vibrationEntryDuration
//                     )
//                     .SetEase(vibrationEntryEase)
//             );
//         }

//         // BOTTOM: neeche se upar.
//         if (vibrationBottomIcon != null)
//         {
//             vibrationBottomIcon.gameObject.SetActive(true);

//             vibrationBottomIcon.anchoredPosition =
//                 vibrationBottomOriginalPosition +
//                 Vector2.down * vibrationEntryDistance;

//             sequence.Insert(
//                 0f,
//                 vibrationBottomIcon
//                     .DOAnchorPos(
//                         vibrationBottomOriginalPosition,
//                         vibrationEntryDuration
//                     )
//                     .SetEase(vibrationEntryEase)
//             );
//         }

//         // CENTER PHONE: dono icons ke arrive hone ke baad fade in.
//         if (vibrationPhoneIcon != null)
//         {
//             vibrationPhoneIcon.gameObject.SetActive(true);

//             if (vibrationPhoneCanvasGroup == null)
//             {
//                 vibrationPhoneCanvasGroup =
//                     vibrationPhoneIcon.GetComponent<CanvasGroup>();
//             }

//             if (vibrationPhoneCanvasGroup != null)
//             {
//                 vibrationPhoneCanvasGroup.alpha = 0f;

//                 sequence.Insert(
//                     vibrationEntryDuration +
//                     vibrationAnimationDelay,
//                     vibrationPhoneCanvasGroup
//                         .DOFade(
//                             1f,
//                             vibrationPhoneFadeDuration
//                         )
//                         .SetEase(vibrationPhoneFadeEase)
//                 );
//             }

//             // Fade complete hone ke baad phone vibrate karega.
//             sequence.Insert(
//                 vibrationEntryDuration +
//                 vibrationAnimationDelay +
//                 vibrationPhoneFadeDuration,
//                 vibrationPhoneIcon
//                     .DOShakeAnchorPos(
//                         vibrationShakeDuration,
//                         vibrationShakeStrength,
//                         vibrationShakeVibrato,
//                         90f,
//                         false,
//                         true
//                     )
//             );
//         }

//         // Complete hone ke baad teeno objects hidden.
//         // sequence.OnComplete(
//         //     HideVibrationAnimationImmediate
//         // );

//         sequence.Play();
//     }


//     private void HideVibrationAnimationImmediate()
//     {
//         if (vibrationTopIcon != null)
//         {
//             vibrationTopIcon.DOKill();

//             if (vibrationAnimationInitialized)
//             {
//                 vibrationTopIcon.anchoredPosition =
//                     vibrationTopOriginalPosition;
//             }

//             vibrationTopIcon.gameObject.SetActive(false);
//         }

//         if (vibrationBottomIcon != null)
//         {
//             vibrationBottomIcon.DOKill();

//             if (vibrationAnimationInitialized)
//             {
//                 vibrationBottomIcon.anchoredPosition =
//                     vibrationBottomOriginalPosition;
//             }

//             vibrationBottomIcon.gameObject.SetActive(false);
//         }

//         if (vibrationPhoneIcon != null)
//         {
//             vibrationPhoneIcon.DOKill();

//             if (vibrationAnimationInitialized)
//             {
//                 vibrationPhoneIcon.localScale =
//                     vibrationPhoneOriginalScale;
//             }

//             if (vibrationPhoneCanvasGroup != null)
//             {
//                 vibrationPhoneCanvasGroup.DOKill();
//                 vibrationPhoneCanvasGroup.alpha = 0f;
//             }

//             vibrationPhoneIcon.gameObject.SetActive(false);
//         }
//     }


//     private void KillVibrationAnimation()
//     {
//         if (vibrationTopIcon != null)
//         {
//             vibrationTopIcon.DOKill();
//         }

//         if (vibrationBottomIcon != null)
//         {
//             vibrationBottomIcon.DOKill();
//         }

//         if (vibrationPhoneIcon != null)
//         {
//             vibrationPhoneIcon.DOKill();
//         }

//         if (vibrationPhoneCanvasGroup != null)
//         {
//             vibrationPhoneCanvasGroup.DOKill();
//         }

//         if (vibrationAnimationInitialized)
//         {
//             HideVibrationAnimationImmediate();
//         }
//     }


//     // =========================================================
//     // UI POSITION HELPER
//     // =========================================================

//     private static Vector2 GetAnchoredPositionRelativeToParent(
//         RectTransform target,
//         RectTransform targetParent
//     )
//     {
//         if (target == null ||
//             targetParent == null)
//         {
//             return Vector2.zero;
//         }


//         Vector3 worldPosition =
//             target.position;


//         Vector3 localPosition =
//             targetParent.InverseTransformPoint(
//                 worldPosition
//             );


//         return new Vector2(
//             localPosition.x,
//             localPosition.y
//         );
//     }
// }



using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public sealed class SettingsUIController : MonoBehaviour
{
    [Header("Settings Popup")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;


    // =========================================================
    // SOUND
    // =========================================================

    [Header("Sound Button")]
    [SerializeField] private Button soundButton;
    [SerializeField] private GameObject soundOnUI;
    [SerializeField] private GameObject soundOffUI;


    [Header("Sound Animation")]
    [Tooltip("Speaker icon jo scale animation karega.")]
    [SerializeField] private RectTransform speakerIcon;

    [Tooltip("Pehli sound bar jo right side move karegi.")]
    [SerializeField] private RectTransform soundBar1;

    [Tooltip("Dusri sound bar jo right side move karegi.")]
    [SerializeField] private RectTransform soundBar2;

    [SerializeField, Min(0f)]
    private float soundBarMoveDistance = 12f;

    [SerializeField, Min(0.01f)]
    private float soundBarMoveDuration = 0.18f;

    [SerializeField, Min(1f)]
    private float speakerScaleMultiplier = 1.12f;

    [SerializeField, Min(0.01f)]
    private float speakerScaleDuration = 0.18f;

    [SerializeField, Range(1, 5)]
    private int soundAnimationLoops = 2;

    [SerializeField, Min(0f)]
    private float soundAnimationLoopDelay = 0.04f;

    [SerializeField]
    private Ease soundBarEase = Ease.OutQuad;

    [SerializeField]
    private Ease speakerEase = Ease.OutBack;


    // =========================================================
    // MUSIC
    // =========================================================

    [Header("Music Button")]
    [SerializeField] private Button musicButton;
    [SerializeField] private GameObject musicOnUI;
    [SerializeField] private GameObject musicOffUI;


    [Header("Music Animation")]
    [Tooltip(
        "Main music icon. Ye scale/bounce animation karega."
    )]
    [SerializeField] private RectTransform musicIcon;


    [Tooltip(
        "5 ya 6 small music icons/notes assign karein. " +
        "Ye sab main music icon ki position se start hongi."
    )]
    [SerializeField] private RectTransform[] musicNotes;


    [Tooltip(
        "Small music icons kitne pixels upar jayengi."
    )]
    [SerializeField, Min(0f)]
    private float musicNoteMoveDistance = 70f;


    [Tooltip(
        "Small music icons ko left/right kitna spread milega."
    )]
    [SerializeField, Min(0f)]
    private float musicNoteHorizontalDrift = 25f;


    [Tooltip(
        "Small music icon ko upar jane mein kitna time lagega."
    )]
    [SerializeField, Min(0.05f)]
    private float musicNoteMoveDuration = 0.65f;


    [Tooltip(
        "Har note ke beech chhota stagger delay."
    )]
    [SerializeField, Min(0f)]
    private float musicNoteStagger = 0.035f;


    [Tooltip(
        "Small music icons fade out hongi."
    )]
    [SerializeField]
    private bool musicNotesFadeOut = true;


    [Tooltip(
        "Movement ke kitne percent par fade start hogi."
    )]
    [SerializeField, Range(0f, 1f)]
    private float musicNoteFadeStart = 0.45f;


    [Tooltip(
        "Main music icon kitna scale hoga."
    )]
    [SerializeField, Min(1f)]
    private float musicIconScaleMultiplier = 1.12f;


    [Tooltip(
        "Main music icon scale animation duration."
    )]
    [SerializeField, Min(0.01f)]
    private float musicIconScaleDuration = 0.18f;


    [Tooltip(
        "Music animation kitni baar repeat hogi."
    )]
    [SerializeField, Range(1, 5)]
    private int musicAnimationLoops = 2;


    [Tooltip(
        "Ek complete music animation ke baad doosri repetition " +
        "se pehle interval."
    )]
    [SerializeField, Min(0f)]
    private float musicAnimationLoopDelay = 0.08f;


    [SerializeField]
    private Ease musicNoteEase = Ease.OutQuad;


    [SerializeField]
    private Ease musicIconEase = Ease.OutBack;



    // =========================================================
    // BELL / NOTIFICATION ANIMATION
    // =========================================================

    [Header("Bell Animation")]
    [Tooltip(
        "Main bell icon. Ye left/right move karega."
    )]
    [SerializeField]
    private RectTransform bellIcon;

    [Tooltip(
        "Bell ke andar wala small clapper icon. " +
        "Bell right jaye to ye left jayega aur bell left jaye to ye right jayega."
    )]
    [SerializeField]
    private RectTransform bellClapperIcon;

    [Tooltip(
        "Bell aur clapper kitne pixels left/right move karenge."
    )]
    [SerializeField, Min(0f)]
    private float bellMoveDistance = 8f;

    [Tooltip(
        "Bell ke ek side move hone ka duration."
    )]
    [SerializeField, Min(0.01f)]
    private float bellMoveDuration = 0.12f;

    [Tooltip(
        "Bell animation kitni complete shakes karegi."
    )]
    [SerializeField, Range(1, 6)]
    private int bellAnimationLoops = 3;

    [Tooltip(
        "Ek complete left/right shake ke baad next shake se pehle interval."
    )]
    [SerializeField, Min(0f)]
    private float bellAnimationLoopDelay = 0.04f;

    [Tooltip(
        "Clapper bell ke opposite direction mein kitna move kare."
    )]
    [SerializeField, Range(0f, 1.5f)]
    private float bellClapperOppositeMultiplier = 1f;

    [SerializeField]
    private Ease bellMoveEase = Ease.InOutSine;


    // =========================================================
    // VIBRATION
    // =========================================================

    [Header("Vibration Button")]
    [SerializeField] private Button vibrationButton;
    [SerializeField] private GameObject vibrationOnUI;
    [SerializeField] private GameObject vibrationOffUI;


    [Header("Vibration Animation")]
    [Tooltip("Vibration ka top icon. Animation mein upar se neeche ayega.")]
    [SerializeField] private RectTransform vibrationTopIcon;

    [Tooltip("Vibration ka bottom icon. Animation mein neeche se upar ayega.")]
    [SerializeField] private RectTransform vibrationBottomIcon;

    [Tooltip("Center ka main phone/vibration icon.")]
    [SerializeField] private RectTransform vibrationPhoneIcon;

    [SerializeField, Min(0f)]
    private float vibrationEntryDistance = 45f;

    [SerializeField, Min(0.01f)]
    private float vibrationEntryDuration = 0.25f;

    [SerializeField, Min(0.01f)]
    private float vibrationPhoneFadeDuration = 0.25f;

    [SerializeField, Min(0.01f)]
    private float vibrationShakeDuration = 0.45f;

    [SerializeField, Min(0.01f)]
    private float vibrationShakeStrength = 5f;

    [SerializeField, Min(1)]
    private int vibrationShakeVibrato = 12;

    [SerializeField, Min(0f)]
    private float vibrationAnimationDelay = 0.05f;

    [SerializeField]
    private Ease vibrationEntryEase = Ease.OutBack;

    [SerializeField]
    private Ease vibrationPhoneFadeEase = Ease.OutQuad;


    // =========================================================
    // MANAGERS
    // =========================================================

    [Header("Managers")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private VibrationManager vibrationManager;


    // =========================================================
    // INTERNAL ANIMATION DATA
    // =========================================================

    private Vector2 soundBar1OriginalPosition;
    private Vector2 soundBar2OriginalPosition;

    private Vector3 speakerOriginalScale;

    private Vector3 musicIconOriginalScale;

    /*
     * Notes ki authored/original Inspector positions.
     * Animation complete hone par yahin restore hongi.
     */
    private Vector2[] musicNoteOriginalPositions;

    private Vector3[] musicNoteOriginalScales;


    /*
     * Runtime start position.
     *
     * Har small music note animation ke waqt
     * main music icon ki position se start karegi.
     */
    private Vector2[] musicNoteStartPositions;


    private CanvasGroup[] musicNoteCanvasGroups;

    private bool soundAnimationInitialized;
    private bool musicAnimationInitialized;

    private Vector2 bellOriginalPosition;
    private Vector2 bellClapperOriginalPosition;
    private bool bellAnimationInitialized;

    private Vector2 vibrationTopOriginalPosition;
    private Vector2 vibrationBottomOriginalPosition;
    private Vector3 vibrationPhoneOriginalScale;
    private CanvasGroup vibrationPhoneCanvasGroup;
    private bool vibrationAnimationInitialized;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (settingsPanel != null)
        {
            UITransition.HideImmediate(settingsPanel);
        }

        CacheSoundAnimationPositions();

        CacheMusicAnimationData();
        HideMusicNotesImmediate();

        CacheVibrationAnimationData();
        HideVibrationAnimationImmediate();

        CacheBellAnimationData();
    }


    // =========================================================
    // ENABLE / DISABLE
    // =========================================================

    private void OnEnable()
    {
        ResolveReferences();

        UIEventBroker.OnScreenChangeRequested +=
            HandleScreenChange;

        AddListeners();

        RefreshButtonStates();
    }


    private void OnDisable()
    {
        UIEventBroker.OnScreenChangeRequested -=
            HandleScreenChange;

        RemoveListeners();

        KillSoundAnimation();

        KillMusicAnimation();

        KillVibrationAnimation();

        KillBellAnimation();
    }


    // =========================================================
    // REFERENCES
    // =========================================================

    private void ResolveReferences()
    {
        if (audioManager == null)
        {
            audioManager =
                FindFirstObjectByType<AudioManager>(
                    FindObjectsInactive.Include
                );
        }

        if (vibrationManager == null)
        {
            vibrationManager =
                FindFirstObjectByType<VibrationManager>(
                    FindObjectsInactive.Include
                );
        }
    }


    // =========================================================
    // LISTENERS
    // =========================================================

    private void AddListeners()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                CloseSettings
            );

            closeButton.onClick.AddListener(
                CloseSettings
            );
        }


        if (soundButton != null)
        {
            soundButton.onClick.RemoveListener(
                ToggleSound
            );

            soundButton.onClick.AddListener(
                ToggleSound
            );
        }


        if (musicButton != null)
        {
            musicButton.onClick.RemoveListener(
                ToggleMusic
            );

            musicButton.onClick.AddListener(
                ToggleMusic
            );
        }


        if (vibrationButton != null)
        {
            vibrationButton.onClick.RemoveListener(
                ToggleVibration
            );

            vibrationButton.onClick.AddListener(
                ToggleVibration
            );
        }
    }


    private void RemoveListeners()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                CloseSettings
            );
        }


        if (soundButton != null)
        {
            soundButton.onClick.RemoveListener(
                ToggleSound
            );
        }


        if (musicButton != null)
        {
            musicButton.onClick.RemoveListener(
                ToggleMusic
            );
        }


        if (vibrationButton != null)
        {
            vibrationButton.onClick.RemoveListener(
                ToggleVibration
            );
        }
    }


    // =========================================================
    // SCREEN CHANGE
    // =========================================================

    private void HandleScreenChange(
        UIScreenType targetScreen
    )
    {
        if (targetScreen ==
            UIScreenType.SettingScreen)
        {
            OpenSettings();

            return;
        }


        if (targetScreen ==
            UIScreenType.MainMenu)
        {
            HideSettings();
        }
    }


    // =========================================================
    // OPEN SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        ResolveReferences();

        RefreshButtonStates();


        if (settingsPanel != null)
        {
            UITransition.Show(
                settingsPanel
            );
        }


        /*
         * Settings open hote hi animations.
         */
        PlaySoundAnimation();

        PlayMusicAnimation();

        PlayVibrationAnimation();

        PlayBellAnimation();
    }


    // =========================================================
    // CLOSE SETTINGS
    // =========================================================

    public void CloseSettings()
    {
        KillSoundAnimation();

        KillMusicAnimation();

        KillVibrationAnimation();

        KillBellAnimation();

        HideSettings();


        UIEventBroker.RequestScreen(
            UIScreenType.MainMenu
        );
    }


    public void HideSettings()
    {
        KillSoundAnimation();

        KillMusicAnimation();

        KillVibrationAnimation();

        KillBellAnimation();


        if (settingsPanel != null)
        {
            UITransition.Hide(
                settingsPanel
            );
        }
    }


    // =========================================================
    // SOUND TOGGLE
    // =========================================================

    private void ToggleSound()
    {
        ResolveReferences();


        if (audioManager == null)
        {
            Debug.LogWarning(
                "SettingsUIController: AudioManager missing hai.",
                this
            );

            return;
        }


        bool newState =
            !audioManager.IsSoundEnabled;


        audioManager.SetSoundEnabled(
            newState
        );


        RefreshSoundUI();

        // Animation sirf ON karne par chalegi, OFF par nahi.
        if (newState)
        {
            PlaySoundAnimation();
        }
    }


    // =========================================================
    // MUSIC TOGGLE
    // =========================================================

    private void ToggleMusic()
    {
        ResolveReferences();


        if (audioManager == null)
        {
            Debug.LogWarning(
                "SettingsUIController: AudioManager missing hai.",
                this
            );

            return;
        }


        bool newState =
            !audioManager.IsMusicEnabled;


        audioManager.SetMusicEnabled(
            newState
        );


        RefreshMusicUI();

        // Animation sirf ON karne par chalegi, OFF par nahi.
        if (newState)
        {
            PlayMusicAnimation();
        }
    }


    // =========================================================
    // VIBRATION TOGGLE
    // =========================================================

    private void ToggleVibration()
    {
        ResolveReferences();


        if (vibrationManager == null)
        {
            Debug.LogWarning(
                "SettingsUIController: VibrationManager missing hai.",
                this
            );

            return;
        }


        bool newState =
            !vibrationManager.IsVibrationEnabled;


        vibrationManager.SetVibrationEnabled(
            newState
        );


        RefreshVibrationUI();

        // Animation sirf ON karne par chalegi, OFF par nahi.
        if (newState)
        {
            PlayVibrationAnimation();
        }
    }


    // =========================================================
    // REFRESH STATES
    // =========================================================

    private void RefreshButtonStates()
    {
        RefreshSoundUI();

        RefreshMusicUI();

        RefreshVibrationUI();
    }


    private void RefreshSoundUI()
    {
        bool isOn =
            audioManager != null &&
            audioManager.IsSoundEnabled;


        SetStateUI(
            soundOnUI,
            soundOffUI,
            isOn
        );
    }


    private void RefreshMusicUI()
    {
        bool isOn =
            audioManager != null &&
            audioManager.IsMusicEnabled;


        SetStateUI(
            musicOnUI,
            musicOffUI,
            isOn
        );
    }


    private void RefreshVibrationUI()
    {
        bool isOn =
            vibrationManager != null &&
            vibrationManager.IsVibrationEnabled;


        SetStateUI(
            vibrationOnUI,
            vibrationOffUI,
            isOn
        );
    }


    private static void SetStateUI(
        GameObject onUI,
        GameObject offUI,
        bool isOn
    )
    {
        if (onUI != null)
        {
            onUI.SetActive(
                isOn
            );
        }


        if (offUI != null)
        {
            offUI.SetActive(
                !isOn
            );
        }
    }


    // =========================================================
    // SOUND ANIMATION
    // =========================================================

    private void CacheSoundAnimationPositions()
    {
        if (soundBar1 != null)
        {
            soundBar1OriginalPosition =
                soundBar1.anchoredPosition;
        }


        if (soundBar2 != null)
        {
            soundBar2OriginalPosition =
                soundBar2.anchoredPosition;
        }


        if (speakerIcon != null)
        {
            speakerOriginalScale =
                speakerIcon.localScale;
        }


        soundAnimationInitialized = true;
    }


    public void PlaySoundAnimation()
    {
        if (!soundAnimationInitialized)
        {
            CacheSoundAnimationPositions();
        }


        KillSoundAnimation();


        Sequence animation =
            DOTween.Sequence();


        // ---------------------------------------------------------
        // BAR 1
        // ---------------------------------------------------------

        if (soundBar1 != null)
        {
            soundBar1.anchoredPosition =
                soundBar1OriginalPosition;


            for (int i = 0;
                 i < soundAnimationLoops;
                 i++)
            {
                Vector2 rightPosition =
                    soundBar1OriginalPosition +
                    Vector2.right *
                    soundBarMoveDistance;


                animation.Append(
                    soundBar1
                        .DOAnchorPos(
                            rightPosition,
                            soundBarMoveDuration
                        )
                        .SetEase(
                            soundBarEase
                        )
                );


                animation.Append(
                    soundBar1
                        .DOAnchorPos(
                            soundBar1OriginalPosition,
                            soundBarMoveDuration
                        )
                        .SetEase(
                            soundBarEase
                        )
                );


                if (soundAnimationLoopDelay > 0f)
                {
                    animation.AppendInterval(
                        soundAnimationLoopDelay
                    );
                }
            }
        }


        // ---------------------------------------------------------
        // BAR 2
        // ---------------------------------------------------------

        if (soundBar2 != null)
        {
            soundBar2.anchoredPosition =
                soundBar2OriginalPosition;


            Sequence bar2Sequence =
                DOTween.Sequence();


            bar2Sequence.AppendInterval(
                soundBarMoveDuration *
                0.35f
            );


            for (int i = 0;
                 i < soundAnimationLoops;
                 i++)
            {
                Vector2 rightPosition =
                    soundBar2OriginalPosition +
                    Vector2.right *
                    soundBarMoveDistance;


                bar2Sequence.Append(
                    soundBar2
                        .DOAnchorPos(
                            rightPosition,
                            soundBarMoveDuration
                        )
                        .SetEase(
                            soundBarEase
                        )
                );


                bar2Sequence.Append(
                    soundBar2
                        .DOAnchorPos(
                            soundBar2OriginalPosition,
                            soundBarMoveDuration
                        )
                        .SetEase(
                            soundBarEase
                        )
                );


                if (soundAnimationLoopDelay > 0f)
                {
                    bar2Sequence.AppendInterval(
                        soundAnimationLoopDelay
                    );
                }
            }


            animation.Insert(
                0f,
                bar2Sequence
            );
        }


        // ---------------------------------------------------------
        // SPEAKER
        // ---------------------------------------------------------

        if (speakerIcon != null)
        {
            speakerIcon.localScale =
                speakerOriginalScale;


            Vector3 enlargedScale =
                speakerOriginalScale *
                speakerScaleMultiplier;


            Sequence speakerSequence =
                DOTween.Sequence();


            for (int i = 0;
                 i < soundAnimationLoops;
                 i++)
            {
                speakerSequence.Append(
                    speakerIcon
                        .DOScale(
                            enlargedScale,
                            speakerScaleDuration
                        )
                        .SetEase(
                            speakerEase
                        )
                );


                speakerSequence.Append(
                    speakerIcon
                        .DOScale(
                            speakerOriginalScale,
                            speakerScaleDuration
                        )
                        .SetEase(
                            Ease.OutQuad
                        )
                );


                if (soundAnimationLoopDelay > 0f)
                {
                    speakerSequence.AppendInterval(
                        soundAnimationLoopDelay
                    );
                }
            }


            animation.Insert(
                0f,
                speakerSequence
            );
        }


        animation.Play();
    }


    // =========================================================
    // MUSIC ANIMATION DATA
    // =========================================================

    private void CacheMusicAnimationData()
    {
        if (musicIcon != null)
        {
            musicIconOriginalScale =
                musicIcon.localScale;
        }


        if (musicNotes == null ||
            musicNotes.Length == 0)
        {
            musicAnimationInitialized = true;

            return;
        }


        musicNoteOriginalPositions =
            new Vector2[
                musicNotes.Length
            ];


        musicNoteOriginalScales =
            new Vector3[
                musicNotes.Length
            ];


        musicNoteStartPositions =
            new Vector2[
                musicNotes.Length
            ];


        musicNoteCanvasGroups =
            new CanvasGroup[
                musicNotes.Length
            ];


        for (int i = 0;
             i < musicNotes.Length;
             i++)
        {
            RectTransform note =
                musicNotes[i];


            if (note == null)
            {
                continue;
            }


            /*
             * Save original Inspector position.
             */
            musicNoteOriginalPositions[i] =
                note.anchoredPosition;


            musicNoteOriginalScales[i] =
                note.localScale;


            /*
             * Calculate main music icon position
             * relative to each note's parent.
             */
            musicNoteStartPositions[i] =
                GetAnchoredPositionRelativeToParent(
                    musicIcon,
                    note.parent as RectTransform
                );


            CanvasGroup canvasGroup =
                note.GetComponent<CanvasGroup>();


            if (canvasGroup == null)
            {
                canvasGroup =
                    note.gameObject.AddComponent<CanvasGroup>();
            }


            musicNoteCanvasGroups[i] =
                canvasGroup;
        }


        musicAnimationInitialized = true;
    }


    // =========================================================
    // MUSIC ANIMATION
    // =========================================================

    public void PlayMusicAnimation()
    {
        if (!musicAnimationInitialized)
        {
            CacheMusicAnimationData();
        }


        KillMusicAnimation();


        Sequence musicSequence =
            DOTween.Sequence();


        // ---------------------------------------------------------
        // SMALL MUSIC ICONS / NOTES
        // ---------------------------------------------------------

        if (musicNotes != null &&
            musicNotes.Length > 0)
        {
            for (int i = 0;
                 i < musicNotes.Length;
                 i++)
            {
                RectTransform note =
                    musicNotes[i];


                if (note == null)
                {
                    continue;
                }

                // Small music notes normally hidden rehti hain.
                // Animation start hote hi enable hongi.
                note.gameObject.SetActive(true);


                /*
                 * IMPORTANT:
                 *
                 * Small note main music icon ki
                 * exact position se start karegi.
                 */
                note.anchoredPosition =
                    musicNoteStartPositions[i];


                note.localScale =
                    musicNoteOriginalScales[i];


                CanvasGroup canvasGroup =
                    musicNoteCanvasGroups[i];


                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }


                /*
                 * Left/right spread.
                 *
                 * 0 = left side
                 * middle = center
                 * last = right side
                 */
                float normalizedIndex =
                    musicNotes.Length > 1
                        ? (float)i /
                          (musicNotes.Length - 1)
                        : 0.5f;


                float spreadDirection =
                    Mathf.Lerp(
                        -1f,
                        1f,
                        normalizedIndex
                    );


                float randomDrift =
                    Random.Range(
                        -musicNoteHorizontalDrift,
                        musicNoteHorizontalDrift
                    );


                randomDrift +=
                    spreadDirection *
                    musicNoteHorizontalDrift *
                    0.35f;


                Vector2 targetPosition =
                    musicNoteStartPositions[i] +
                    new Vector2(
                        randomDrift,
                        musicNoteMoveDistance
                    );


                /*
                 * Har note slightly different time par start hogi.
                 */
                float noteDelay =
                    i *
                    musicNoteStagger;


                Tween moveTween =
                    note
                        .DOAnchorPos(
                            targetPosition,
                            musicNoteMoveDuration
                        )
                        .SetEase(
                            musicNoteEase
                        );


                musicSequence.Insert(
                    noteDelay,
                    moveTween
                );


                // -------------------------------------------------
                // FADE
                // -------------------------------------------------

                if (musicNotesFadeOut &&
                    canvasGroup != null)
                {
                    float fadeDelay =
                        noteDelay +
                        musicNoteMoveDuration *
                        musicNoteFadeStart;


                    float fadeDuration =
                        musicNoteMoveDuration *
                        (1f -
                         musicNoteFadeStart);


                    Tween fadeTween =
                        canvasGroup
                            .DOFade(
                                0f,
                                Mathf.Max(
                                    0.05f,
                                    fadeDuration
                                )
                            );


                    musicSequence.Insert(
                        fadeDelay,
                        fadeTween
                    );
                }
            }
        }


        // ---------------------------------------------------------
        // MAIN MUSIC ICON
        // ---------------------------------------------------------

        if (musicIcon != null)
        {
            musicIcon.localScale =
                musicIconOriginalScale;


            Vector3 enlargedScale =
                musicIconOriginalScale *
                musicIconScaleMultiplier;


            Sequence iconSequence =
                DOTween.Sequence();


            for (int i = 0;
                 i < musicAnimationLoops;
                 i++)
            {
                iconSequence.Append(
                    musicIcon
                        .DOScale(
                            enlargedScale,
                            musicIconScaleDuration
                        )
                        .SetEase(
                            musicIconEase
                        )
                );


                iconSequence.Append(
                    musicIcon
                        .DOScale(
                            musicIconOriginalScale,
                            musicIconScaleDuration
                        )
                        .SetEase(
                            Ease.OutQuad
                        )
                );


                /*
                 * Important:
                 *
                 * Ek animation complete hone ke baad
                 * next animation se pehle interval.
                 */
                if (musicAnimationLoopDelay > 0f)
                {
                    iconSequence.AppendInterval(
                        musicAnimationLoopDelay
                    );
                }
            }


            musicSequence.Insert(
                0f,
                iconSequence
            );
        }


        musicSequence.OnComplete(
            ResetMusicAnimationObjects
        );


        musicSequence.Play();
    }


    // =========================================================
    // RESET MUSIC ANIMATION
    // =========================================================

    private void ResetMusicAnimationObjects()
    {
        if (musicIcon != null)
        {
            musicIcon.localScale =
                musicIconOriginalScale;
        }


        if (musicNotes == null)
        {
            return;
        }


        for (int i = 0;
             i < musicNotes.Length;
             i++)
        {
            RectTransform note =
                musicNotes[i];


            if (note == null)
            {
                continue;
            }


            /*
             * Animation complete hone ke baad
             * notes apni Inspector positions par wapas.
             */
            note.anchoredPosition =
                musicNoteOriginalPositions[i];


            note.localScale =
                musicNoteOriginalScales[i];


            CanvasGroup canvasGroup =
                musicNoteCanvasGroups[i];


            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            // Animation complete hone ke baad note dobara hidden.
            note.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // KILL SOUND ANIMATION
    // =========================================================

    private void KillSoundAnimation()
    {
        if (soundBar1 != null)
        {
            soundBar1.DOKill();


            if (soundAnimationInitialized)
            {
                soundBar1.anchoredPosition =
                    soundBar1OriginalPosition;
            }
        }


        if (soundBar2 != null)
        {
            soundBar2.DOKill();


            if (soundAnimationInitialized)
            {
                soundBar2.anchoredPosition =
                    soundBar2OriginalPosition;
            }
        }


        if (speakerIcon != null)
        {
            speakerIcon.DOKill();


            if (soundAnimationInitialized)
            {
                speakerIcon.localScale =
                    speakerOriginalScale;
            }
        }
    }


    // =========================================================
    // HIDE MUSIC NOTES
    // =========================================================

    private void HideMusicNotesImmediate()
    {
        if (musicNotes == null)
        {
            return;
        }

        for (int i = 0; i < musicNotes.Length; i++)
        {
            if (musicNotes[i] != null)
            {
                musicNotes[i].gameObject.SetActive(false);
            }
        }
    }


    // =========================================================
    // KILL MUSIC ANIMATION
    // =========================================================

    private void KillMusicAnimation()
    {
        if (musicIcon != null)
        {
            musicIcon.DOKill();
        }


        if (musicNotes != null)
        {
            for (int i = 0;
                 i < musicNotes.Length;
                 i++)
            {
                if (musicNotes[i] != null)
                {
                    musicNotes[i].DOKill();
                }


                if (musicNoteCanvasGroups != null &&
                    i < musicNoteCanvasGroups.Length &&
                    musicNoteCanvasGroups[i] != null)
                {
                    musicNoteCanvasGroups[i].DOKill();
                }
            }
        }


        if (musicAnimationInitialized)
        {
            ResetMusicAnimationObjects();
        }
        else
        {
            HideMusicNotesImmediate();
        }
    }


    // =========================================================
    // BELL ANIMATION
    // =========================================================

    private void CacheBellAnimationData()
    {
        if (bellIcon != null)
        {
            bellOriginalPosition =
                bellIcon.anchoredPosition;
        }

        if (bellClapperIcon != null)
        {
            bellClapperOriginalPosition =
                bellClapperIcon.anchoredPosition;
        }

        bellAnimationInitialized = true;
    }


    public void PlayBellAnimation()
    {
        if (!bellAnimationInitialized)
        {
            CacheBellAnimationData();
        }

        KillBellAnimation();

        if (bellIcon == null &&
            bellClapperIcon == null)
        {
            return;
        }

        if (bellIcon != null)
        {
            bellIcon.anchoredPosition =
                bellOriginalPosition;
        }

        if (bellClapperIcon != null)
        {
            bellClapperIcon.anchoredPosition =
                bellClapperOriginalPosition;
        }

        Sequence bellSequence =
            DOTween.Sequence();

        for (int i = 0;
             i < bellAnimationLoops;
             i++)
        {
            Vector2 bellRightPosition =
                bellOriginalPosition +
                Vector2.right * bellMoveDistance;

            Vector2 clapperLeftPosition =
                bellClapperOriginalPosition -
                Vector2.right *
                (
                    bellMoveDistance *
                    bellClapperOppositeMultiplier
                );

            if (bellIcon != null)
            {
                bellSequence.Append(
                    bellIcon
                        .DOAnchorPos(
                            bellRightPosition,
                            bellMoveDuration
                        )
                        .SetEase(bellMoveEase)
                );
            }

            if (bellClapperIcon != null)
            {
                bellSequence.Join(
                    bellClapperIcon
                        .DOAnchorPos(
                            clapperLeftPosition,
                            bellMoveDuration
                        )
                        .SetEase(bellMoveEase)
                );
            }


            Vector2 bellLeftPosition =
                bellOriginalPosition -
                Vector2.right * bellMoveDistance;

            Vector2 clapperRightPosition =
                bellClapperOriginalPosition +
                Vector2.right *
                (
                    bellMoveDistance *
                    bellClapperOppositeMultiplier
                );

            if (bellIcon != null)
            {
                bellSequence.Append(
                    bellIcon
                        .DOAnchorPos(
                            bellLeftPosition,
                            bellMoveDuration
                        )
                        .SetEase(bellMoveEase)
                );
            }

            if (bellClapperIcon != null)
            {
                bellSequence.Join(
                    bellClapperIcon
                        .DOAnchorPos(
                            clapperRightPosition,
                            bellMoveDuration
                        )
                        .SetEase(bellMoveEase)
                );
            }


            if (bellIcon != null)
            {
                bellSequence.Append(
                    bellIcon
                        .DOAnchorPos(
                            bellOriginalPosition,
                            bellMoveDuration
                        )
                        .SetEase(bellMoveEase)
                );
            }

            if (bellClapperIcon != null)
            {
                bellSequence.Join(
                    bellClapperIcon
                        .DOAnchorPos(
                            bellClapperOriginalPosition,
                            bellMoveDuration
                        )
                        .SetEase(bellMoveEase)
                );
            }


            if (bellAnimationLoopDelay > 0f)
            {
                bellSequence.AppendInterval(
                    bellAnimationLoopDelay
                );
            }
        }


        // Animation complete hone ke baad dono icons visible
        // aur apni original Inspector positions par rahenge.
        bellSequence.OnComplete(() =>
        {
            if (bellIcon != null)
            {
                bellIcon.anchoredPosition =
                    bellOriginalPosition;
            }

            if (bellClapperIcon != null)
            {
                bellClapperIcon.anchoredPosition =
                    bellClapperOriginalPosition;
            }
        });

        bellSequence.Play();
    }


    private void KillBellAnimation()
    {
        if (bellIcon != null)
        {
            bellIcon.DOKill();

            if (bellAnimationInitialized)
            {
                bellIcon.anchoredPosition =
                    bellOriginalPosition;
            }
        }

        if (bellClapperIcon != null)
        {
            bellClapperIcon.DOKill();

            if (bellAnimationInitialized)
            {
                bellClapperIcon.anchoredPosition =
                    bellClapperOriginalPosition;
            }
        }
    }


    // =========================================================
    // VIBRATION ANIMATION
    // =========================================================

    private void CacheVibrationAnimationData()
    {
        if (vibrationTopIcon != null)
        {
            vibrationTopOriginalPosition =
                vibrationTopIcon.anchoredPosition;
        }

        if (vibrationBottomIcon != null)
        {
            vibrationBottomOriginalPosition =
                vibrationBottomIcon.anchoredPosition;
        }

        if (vibrationPhoneIcon != null)
        {
            vibrationPhoneOriginalScale =
                vibrationPhoneIcon.localScale;

            vibrationPhoneCanvasGroup =
                vibrationPhoneIcon.GetComponent<CanvasGroup>();

            if (vibrationPhoneCanvasGroup == null)
            {
                vibrationPhoneCanvasGroup =
                    vibrationPhoneIcon.gameObject.AddComponent<CanvasGroup>();
            }
        }

        vibrationAnimationInitialized = true;
    }


    public void PlayVibrationAnimation()
    {
        if (!vibrationAnimationInitialized)
        {
            CacheVibrationAnimationData();
        }

        KillVibrationAnimation();

        Sequence sequence = DOTween.Sequence();

        // TOP: upar se neeche.
        if (vibrationTopIcon != null)
        {
            vibrationTopIcon.gameObject.SetActive(true);

            vibrationTopIcon.anchoredPosition =
                vibrationTopOriginalPosition +
                Vector2.up * vibrationEntryDistance;

            sequence.Insert(
                0f,
                vibrationTopIcon
                    .DOAnchorPos(
                        vibrationTopOriginalPosition,
                        vibrationEntryDuration
                    )
                    .SetEase(vibrationEntryEase)
            );
        }

        // BOTTOM: neeche se upar.
        if (vibrationBottomIcon != null)
        {
            vibrationBottomIcon.gameObject.SetActive(true);

            vibrationBottomIcon.anchoredPosition =
                vibrationBottomOriginalPosition +
                Vector2.down * vibrationEntryDistance;

            sequence.Insert(
                0f,
                vibrationBottomIcon
                    .DOAnchorPos(
                        vibrationBottomOriginalPosition,
                        vibrationEntryDuration
                    )
                    .SetEase(vibrationEntryEase)
            );
        }

        // CENTER PHONE: dono icons ke arrive hone ke baad fade in.
        if (vibrationPhoneIcon != null)
        {
            vibrationPhoneIcon.gameObject.SetActive(true);

            if (vibrationPhoneCanvasGroup == null)
            {
                vibrationPhoneCanvasGroup =
                    vibrationPhoneIcon.GetComponent<CanvasGroup>();
            }

            if (vibrationPhoneCanvasGroup != null)
            {
                vibrationPhoneCanvasGroup.alpha = 0f;

                sequence.Insert(
                    vibrationEntryDuration +
                    vibrationAnimationDelay,
                    vibrationPhoneCanvasGroup
                        .DOFade(
                            1f,
                            vibrationPhoneFadeDuration
                        )
                        .SetEase(vibrationPhoneFadeEase)
                );
            }

            // Fade complete hone ke baad phone vibrate karega.
            sequence.Insert(
                vibrationEntryDuration +
                vibrationAnimationDelay +
                vibrationPhoneFadeDuration,
                vibrationPhoneIcon
                    .DOShakeAnchorPos(
                        vibrationShakeDuration,
                        vibrationShakeStrength,
                        vibrationShakeVibrato,
                        90f,
                        false,
                        true
                    )
            );
        }

        // Complete hone ke baad teeno objects hidden.
        // sequence.OnComplete(
        //     HideVibrationAnimationImmediate
        // );

        sequence.Play();
    }


    private void HideVibrationAnimationImmediate()
    {
        if (vibrationTopIcon != null)
        {
            vibrationTopIcon.DOKill();

            if (vibrationAnimationInitialized)
            {
                vibrationTopIcon.anchoredPosition =
                    vibrationTopOriginalPosition;
            }

            vibrationTopIcon.gameObject.SetActive(false);
        }

        if (vibrationBottomIcon != null)
        {
            vibrationBottomIcon.DOKill();

            if (vibrationAnimationInitialized)
            {
                vibrationBottomIcon.anchoredPosition =
                    vibrationBottomOriginalPosition;
            }

            vibrationBottomIcon.gameObject.SetActive(false);
        }

        if (vibrationPhoneIcon != null)
        {
            vibrationPhoneIcon.DOKill();

            if (vibrationAnimationInitialized)
            {
                vibrationPhoneIcon.localScale =
                    vibrationPhoneOriginalScale;
            }

            if (vibrationPhoneCanvasGroup != null)
            {
                vibrationPhoneCanvasGroup.DOKill();
                vibrationPhoneCanvasGroup.alpha = 0f;
            }

            vibrationPhoneIcon.gameObject.SetActive(false);
        }
    }


    private void KillVibrationAnimation()
    {
        if (vibrationTopIcon != null)
        {
            vibrationTopIcon.DOKill();
        }

        if (vibrationBottomIcon != null)
        {
            vibrationBottomIcon.DOKill();
        }

        if (vibrationPhoneIcon != null)
        {
            vibrationPhoneIcon.DOKill();
        }

        if (vibrationPhoneCanvasGroup != null)
        {
            vibrationPhoneCanvasGroup.DOKill();
        }

        if (vibrationAnimationInitialized)
        {
            HideVibrationAnimationImmediate();
        }
    }


    // =========================================================
    // UI POSITION HELPER
    // =========================================================

    private static Vector2 GetAnchoredPositionRelativeToParent(
        RectTransform target,
        RectTransform targetParent
    )
    {
        if (target == null ||
            targetParent == null)
        {
            return Vector2.zero;
        }


        Vector3 worldPosition =
            target.position;


        Vector3 localPosition =
            targetParent.InverseTransformPoint(
                worldPosition
            );


        return new Vector2(
            localPosition.x,
            localPosition.y
        );
    }
}










