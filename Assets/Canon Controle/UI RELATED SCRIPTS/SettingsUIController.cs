using UnityEngine;
using UnityEngine.UI;

public sealed class SettingsUIController : MonoBehaviour
{
    [Header("Settings Popup")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;


    [Header("Setting Toggles")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle vibrationToggle;


    [Header("Managers")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private VibrationManager vibrationManager;


    private void Awake()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }


    private void OnEnable()
    {
        ResolveReferences();

        UIEventBroker.OnScreenChangeRequested +=
            HandleScreenChange;

        AddListeners();
        RefreshToggleStates();
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

        if (soundToggle != null)
        {
            soundToggle.onValueChanged.RemoveListener(
                HandleSoundToggleChanged
            );

            soundToggle.onValueChanged.AddListener(
                HandleSoundToggleChanged
            );
        }

        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(
                HandleMusicToggleChanged
            );

            musicToggle.onValueChanged.AddListener(
                HandleMusicToggleChanged
            );
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.RemoveListener(
                HandleVibrationToggleChanged
            );

            vibrationToggle.onValueChanged.AddListener(
                HandleVibrationToggleChanged
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

        if (soundToggle != null)
        {
            soundToggle.onValueChanged.RemoveListener(
                HandleSoundToggleChanged
            );
        }

        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(
                HandleMusicToggleChanged
            );
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.RemoveListener(
                HandleVibrationToggleChanged
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
        RefreshToggleStates();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
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
            settingsPanel.SetActive(false);
        }
    }


    private void HandleSoundToggleChanged(
        bool isOn)
    {
        if (audioManager != null)
        {
            audioManager.SetSoundEnabled(
                isOn
            );
        }
        else
        {
            Debug.LogWarning(
                "SettingsUIController: AudioManager missing hai.",
                this
            );
        }
    }


    private void HandleMusicToggleChanged(
        bool isOn)
    {
        if (audioManager != null)
        {
            audioManager.SetMusicEnabled(
                isOn
            );
        }
        else
        {
            Debug.LogWarning(
                "SettingsUIController: AudioManager missing hai.",
                this
            );
        }
    }


    private void HandleVibrationToggleChanged(
        bool isOn)
    {
        if (vibrationManager != null)
        {
            vibrationManager.SetVibrationEnabled(
                isOn
            );
        }
        else
        {
            Debug.LogWarning(
                "SettingsUIController: VibrationManager missing hai.",
                this
            );
        }
    }


    private void RefreshToggleStates()
    {
        if (audioManager != null)
        {
            if (soundToggle != null)
            {
                soundToggle.SetIsOnWithoutNotify(
                    audioManager.IsSoundEnabled
                );
            }

            if (musicToggle != null)
            {
                musicToggle.SetIsOnWithoutNotify(
                    audioManager.IsMusicEnabled
                );
            }
        }

        if (vibrationManager != null &&
            vibrationToggle != null)
        {
            vibrationToggle.SetIsOnWithoutNotify(
                vibrationManager.IsVibrationEnabled
            );
        }
    }
}