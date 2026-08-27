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










using UnityEngine;
using UnityEngine.UI;

public sealed class SettingsUIController : MonoBehaviour
{
    [Header("Settings Popup")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;


    [Header("Sound Button")]
    [SerializeField] private Button soundButton;
    [SerializeField] private GameObject soundOnUI;
    [SerializeField] private GameObject soundOffUI;


    [Header("Music Button")]
    [SerializeField] private Button musicButton;
    [SerializeField] private GameObject musicOnUI;
    [SerializeField] private GameObject musicOffUI;


    [Header("Vibration Button")]
    [SerializeField] private Button vibrationButton;
    [SerializeField] private GameObject vibrationOnUI;
    [SerializeField] private GameObject vibrationOffUI;


    [Header("Managers")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private VibrationManager vibrationManager;


    private void Awake()
    {
        if (settingsPanel != null)
        {
            UITransition.HideImmediate(settingsPanel);
        }
    }


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
    }


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


    private void HandleScreenChange(
        UIScreenType targetScreen)
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


    public void OpenSettings()
    {
        ResolveReferences();
        RefreshButtonStates();

        if (settingsPanel != null)
        {
            UITransition.Show(settingsPanel);
        }
    }


    public void CloseSettings()
    {
        HideSettings();

        UIEventBroker.RequestScreen(
            UIScreenType.MainMenu
        );
    }


    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            UITransition.Hide(settingsPanel);
        }
    }


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
    }


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
    }


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
    }


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
        bool isOn)
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
}


