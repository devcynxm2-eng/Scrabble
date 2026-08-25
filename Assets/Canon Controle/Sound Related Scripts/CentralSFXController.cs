using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class CentralSFXController : MonoBehaviour
{
    [System.Serializable]
    public sealed class ButtonSFXGroup
    {
        public string groupName = "UI Buttons";

        public AudioClip sfx;

        [Range(0f, 1f)]
        public float volume = 1f;

        public List<Button> buttons = new List<Button>();
    }


    [System.Serializable]
    public sealed class ClickSFXGroup
    {
        public string groupName = "Gameplay Click";

        public AudioClip sfx;

        [Range(0f, 1f)]
        public float volume = 1f;

        public List<GameObject> activeTargets =
            new List<GameObject>();

        public bool ignoreWhenPointerOverUI = true;

        [Min(0f)]
        public float minimumInterval = 0f;

        [HideInInspector]
        public float lastPlayedTime = -999f;
    }


    [Header("Reference")]
    [SerializeField] private AudioManager audioManager;


    [Header("UI Button SFX Groups")]
    [SerializeField]
    private List<ButtonSFXGroup> buttonSFXGroups =
        new List<ButtonSFXGroup>();


    [Header("Gameplay Click / Touch SFX Groups")]
    [SerializeField]
    private List<ClickSFXGroup> clickSFXGroups =
        new List<ClickSFXGroup>();


    private readonly Dictionary<Button, UnityEngine.Events.UnityAction>
        buttonListeners =
            new Dictionary<Button, UnityEngine.Events.UnityAction>();


    private readonly List<RaycastResult>
        uiRaycastResults =
            new List<RaycastResult>();


    private void Awake()
    {
        ResolveAudioManager();
    }


    private void OnEnable()
    {
        ResolveAudioManager();
        RegisterButtonSFX();
    }


    private void OnDisable()
    {
        UnregisterButtonSFX();
    }


    private void Update()
    {
        if (!WasPrimaryPointerPressedThisFrame())
        {
            return;
        }

        HandleClickSFXGroups();
    }


    private void ResolveAudioManager()
    {
        if (audioManager != null)
        {
            return;
        }

        if (AudioManager.Instance != null)
        {
            audioManager = AudioManager.Instance;
            return;
        }

        audioManager =
            FindFirstObjectByType<AudioManager>(
                FindObjectsInactive.Include
            );
    }


    private void RegisterButtonSFX()
    {
        UnregisterButtonSFX();

        for (int groupIndex = 0;
             groupIndex < buttonSFXGroups.Count;
             groupIndex++)
        {
            ButtonSFXGroup group =
                buttonSFXGroups[groupIndex];

            if (group == null ||
                group.sfx == null ||
                group.buttons == null)
            {
                continue;
            }

            for (int buttonIndex = 0;
                 buttonIndex < group.buttons.Count;
                 buttonIndex++)
            {
                Button button =
                    group.buttons[buttonIndex];

                if (button == null ||
                    buttonListeners.ContainsKey(button))
                {
                    continue;
                }

                AudioClip clip = group.sfx;
                float volume = group.volume;

                UnityEngine.Events.UnityAction listener =
                    () => PlaySFX(
                        clip,
                        volume
                    );

                button.onClick.AddListener(listener);

                buttonListeners.Add(
                    button,
                    listener
                );
            }
        }
    }


    private void UnregisterButtonSFX()
    {
        foreach (
            KeyValuePair<
                Button,
                UnityEngine.Events.UnityAction
            > entry in buttonListeners)
        {
            if (entry.Key != null)
            {
                entry.Key.onClick.RemoveListener(
                    entry.Value
                );
            }
        }

        buttonListeners.Clear();
    }


    private void HandleClickSFXGroups()
    {
        for (int i = 0;
             i < clickSFXGroups.Count;
             i++)
        {
            ClickSFXGroup group =
                clickSFXGroups[i];

            if (group == null ||
                group.sfx == null)
            {
                continue;
            }

            if (!HasAnyActiveTarget(
                    group.activeTargets))
            {
                continue;
            }

            if (group.ignoreWhenPointerOverUI &&
                IsPointerOverUI())
            {
                continue;
            }

            if (Time.unscaledTime -
                group.lastPlayedTime <
                group.minimumInterval)
            {
                continue;
            }

            group.lastPlayedTime =
                Time.unscaledTime;

            PlaySFX(
                group.sfx,
                group.volume
            );
        }
    }


    private static bool HasAnyActiveTarget(
        List<GameObject> targets)
    {
        if (targets == null ||
            targets.Count == 0)
        {
            return false;
        }

        for (int i = 0;
             i < targets.Count;
             i++)
        {
            GameObject target =
                targets[i];

            if (target != null &&
                target.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }


    private static bool WasPrimaryPointerPressedThisFrame()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }


    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (!TryGetCurrentPointerPosition(
                out Vector2 pointerPosition))
        {
            return false;
        }

        PointerEventData pointerEventData =
            new PointerEventData(
                EventSystem.current
            )
            {
                position = pointerPosition
            };

        uiRaycastResults.Clear();

        EventSystem.current.RaycastAll(
            pointerEventData,
            uiRaycastResults
        );

        return uiRaycastResults.Count > 0;
    }


    private static bool TryGetCurrentPointerPosition(
        out Vector2 pointerPosition)
    {
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();

            return true;
        }

        if (Mouse.current != null)
        {
            pointerPosition =
                Mouse.current.position.ReadValue();

            return true;
        }

        pointerPosition = Vector2.zero;
        return false;
    }


    private void PlaySFX(
        AudioClip clip,
        float volume)
    {
        ResolveAudioManager();

        if (audioManager == null)
        {
            Debug.LogWarning(
                "CentralSFXController: AudioManager missing hai.",
                this
            );

            return;
        }

        audioManager.PlaySFX(
            clip,
            volume
        );
    }


#if UNITY_EDITOR
    [ContextMenu("Refresh Button SFX Listeners")]
    private void RefreshButtonSFXListeners()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        RegisterButtonSFX();
    }
#endif
}