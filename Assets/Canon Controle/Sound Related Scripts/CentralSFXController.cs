// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.InputSystem;
// using UnityEngine.UI;

// public sealed class CentralSFXController : MonoBehaviour
// {
//     [System.Serializable]
//     public sealed class ButtonSFXGroup
//     {
//         public string groupName = "UI Buttons";

//         public AudioClip sfx;

//         [Range(0f, 1f)]
//         public float volume = 1f;

//         public List<Button> buttons = new List<Button>();
//     }


//     [System.Serializable]
//     public sealed class ClickSFXGroup
//     {
//         public string groupName = "Gameplay Click";

//         public AudioClip sfx;

//         [Range(0f, 1f)]
//         public float volume = 1f;

//         public List<GameObject> activeTargets =
//             new List<GameObject>();

//         public bool ignoreWhenPointerOverUI = true;

//         [Min(0f)]
//         public float minimumInterval = 0f;

//         [HideInInspector]
//         public float lastPlayedTime = -999f;
//     }


//     [Header("Reference")]
//     [SerializeField] private AudioManager audioManager;


//     [Header("UI Button SFX Groups")]
//     [SerializeField]
//     private List<ButtonSFXGroup> buttonSFXGroups =
//         new List<ButtonSFXGroup>();


//     [Header("Gameplay Click / Touch SFX Groups")]
//     [SerializeField]
//     private List<ClickSFXGroup> clickSFXGroups =
//         new List<ClickSFXGroup>();


//     private readonly Dictionary<Button, UnityEngine.Events.UnityAction>
//         buttonListeners =
//             new Dictionary<Button, UnityEngine.Events.UnityAction>();


//     private readonly List<RaycastResult>
//         uiRaycastResults =
//             new List<RaycastResult>();


//     private void Awake()
//     {
//         ResolveAudioManager();
//     }


//     private void OnEnable()
//     {
//         ResolveAudioManager();
//         RegisterButtonSFX();
//     }


//     private void OnDisable()
//     {
//         UnregisterButtonSFX();
//     }


//     private void Update()
//     {
//         if (!WasPrimaryPointerPressedThisFrame())
//         {
//             return;
//         }

//         HandleClickSFXGroups();
//     }


//     private void ResolveAudioManager()
//     {
//         if (audioManager != null)
//         {
//             return;
//         }

//         if (AudioManager.Instance != null)
//         {
//             audioManager = AudioManager.Instance;
//             return;
//         }

//         audioManager =
//             FindFirstObjectByType<AudioManager>(
//                 FindObjectsInactive.Include
//             );
//     }


//     private void RegisterButtonSFX()
//     {
//         UnregisterButtonSFX();

//         for (int groupIndex = 0;
//              groupIndex < buttonSFXGroups.Count;
//              groupIndex++)
//         {
//             ButtonSFXGroup group =
//                 buttonSFXGroups[groupIndex];

//             if (group == null ||
//                 group.sfx == null ||
//                 group.buttons == null)
//             {
//                 continue;
//             }

//             for (int buttonIndex = 0;
//                  buttonIndex < group.buttons.Count;
//                  buttonIndex++)
//             {
//                 Button button =
//                     group.buttons[buttonIndex];

//                 if (button == null ||
//                     buttonListeners.ContainsKey(button))
//                 {
//                     continue;
//                 }

//                 AudioClip clip = group.sfx;
//                 float volume = group.volume;

//                 UnityEngine.Events.UnityAction listener =
//                     () => PlaySFX(
//                         clip,
//                         volume
//                     );

//                 button.onClick.AddListener(listener);

//                 buttonListeners.Add(
//                     button,
//                     listener
//                 );
//             }
//         }
//     }


//     private void UnregisterButtonSFX()
//     {
//         foreach (
//             KeyValuePair<
//                 Button,
//                 UnityEngine.Events.UnityAction
//             > entry in buttonListeners)
//         {
//             if (entry.Key != null)
//             {
//                 entry.Key.onClick.RemoveListener(
//                     entry.Value
//                 );
//             }
//         }

//         buttonListeners.Clear();
//     }


//     private void HandleClickSFXGroups()
//     {
//         for (int i = 0;
//              i < clickSFXGroups.Count;
//              i++)
//         {
//             ClickSFXGroup group =
//                 clickSFXGroups[i];

//             if (group == null ||
//                 group.sfx == null)
//             {
//                 continue;
//             }

//             if (!HasAnyActiveTarget(
//                     group.activeTargets))
//             {
//                 continue;
//             }

//             if (group.ignoreWhenPointerOverUI &&
//                 IsPointerOverUI())
//             {
//                 continue;
//             }

//             if (Time.unscaledTime -
//                 group.lastPlayedTime <
//                 group.minimumInterval)
//             {
//                 continue;
//             }

//             group.lastPlayedTime =
//                 Time.unscaledTime;

//             PlaySFX(
//                 group.sfx,
//                 group.volume
//             );
//         }
//     }


//     private static bool HasAnyActiveTarget(
//         List<GameObject> targets)
//     {
//         if (targets == null ||
//             targets.Count == 0)
//         {
//             return false;
//         }

//         for (int i = 0;
//              i < targets.Count;
//              i++)
//         {
//             GameObject target =
//                 targets[i];

//             if (target != null &&
//                 target.activeInHierarchy)
//             {
//                 return true;
//             }
//         }

//         return false;
//     }


//     private static bool WasPrimaryPointerPressedThisFrame()
//     {
//         if (Mouse.current != null &&
//             Mouse.current.leftButton.wasPressedThisFrame)
//         {
//             return true;
//         }

//         if (Touchscreen.current != null &&
//             Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
//         {
//             return true;
//         }

//         return false;
//     }


//     private bool IsPointerOverUI()
//     {
//         if (EventSystem.current == null)
//         {
//             return false;
//         }

//         if (!TryGetCurrentPointerPosition(
//                 out Vector2 pointerPosition))
//         {
//             return false;
//         }

//         PointerEventData pointerEventData =
//             new PointerEventData(
//                 EventSystem.current
//             )
//             {
//                 position = pointerPosition
//             };

//         uiRaycastResults.Clear();

//         EventSystem.current.RaycastAll(
//             pointerEventData,
//             uiRaycastResults
//         );

//         return uiRaycastResults.Count > 0;
//     }


//     private static bool TryGetCurrentPointerPosition(
//         out Vector2 pointerPosition)
//     {
//         if (Touchscreen.current != null &&
//             Touchscreen.current.primaryTouch.press.isPressed)
//         {
//             pointerPosition =
//                 Touchscreen.current.primaryTouch.position.ReadValue();

//             return true;
//         }

//         if (Mouse.current != null)
//         {
//             pointerPosition =
//                 Mouse.current.position.ReadValue();

//             return true;
//         }

//         pointerPosition = Vector2.zero;
//         return false;
//     }


//     private void PlaySFX(
//         AudioClip clip,
//         float volume)
//     {
//         ResolveAudioManager();

//         if (audioManager == null)
//         {
//             Debug.LogWarning(
//                 "CentralSFXController: AudioManager missing hai.",
//                 this
//             );

//             return;
//         }

//         audioManager.PlaySFX(
//             clip,
//             volume
//         );
//     }


// #if UNITY_EDITOR
//     [ContextMenu("Refresh Button SFX Listeners")]
//     private void RefreshButtonSFXListeners()
//     {
//         if (!Application.isPlaying)
//         {
//             return;
//         }

//         RegisterButtonSFX();
//     }
// #endif
// }






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


    [System.Serializable]
    public sealed class ScreenSFXGroup
    {
        public string groupName = "Screen SFX";

        [Tooltip(
            "Screen ka root GameObject yahan assign karein."
        )]
        public GameObject screen;

        [Header("Open / Appear")]
        public AudioClip openSFX;

        [Range(0f, 1f)]
        public float openVolume = 1f;

        [Header("Close / Disappear")]
        public AudioClip closeSFX;

        [Range(0f, 1f)]
        public float closeVolume = 1f;

        [HideInInspector]
        public bool lastActiveState;
    }


    [System.Serializable]
    public sealed class ParticleSFXGroup
    {
        public string groupName = "Particle SFX";

        [Tooltip(
            "Jis ParticleSystem ke Play hone par sound chalana hai."
        )]
        public ParticleSystem particle;

        [Tooltip(
            "Particle Play hone par ye sound chalega."
        )]
        public AudioClip sfx;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip(
            "Agar ON ho to particle already playing ho aur dobara PlayParticleSFX " +
            "call ho to sound dobara nahi chalega."
        )]
        public bool preventDuplicateWhilePlaying = false;

        [HideInInspector]
        public bool wasPlaying;
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


    [Header("Screen Open / Close SFX Groups")]
    [SerializeField]
    private List<ScreenSFXGroup> screenSFXGroups =
        new List<ScreenSFXGroup>();


    [Header("Particle SFX Groups")]
    [SerializeField]
    private List<ParticleSFXGroup> particleSFXGroups =
        new List<ParticleSFXGroup>();


    private readonly Dictionary<Button, UnityEngine.Events.UnityAction>
        buttonListeners =
            new Dictionary<Button, UnityEngine.Events.UnityAction>();


    private readonly List<RaycastResult>
        uiRaycastResults =
            new List<RaycastResult>();


    private void Awake()
    {
        ResolveAudioManager();
        InitializeScreenStates();
        InitializeParticleStates();
    }


    private void OnEnable()
    {
        ResolveAudioManager();
        RegisterButtonSFX();

        InitializeScreenStates();
        InitializeParticleStates();
    }


    private void OnDisable()
    {
        UnregisterButtonSFX();
    }


    private void Update()
    {
        if (WasPrimaryPointerPressedThisFrame())
        {
            HandleClickSFXGroups();
        }

        HandleScreenSFXGroups();
        HandleParticleSFXGroups();
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


    private void InitializeScreenStates()
    {
        if (screenSFXGroups == null)
        {
            return;
        }

        for (int i = 0;
             i < screenSFXGroups.Count;
             i++)
        {
            ScreenSFXGroup group =
                screenSFXGroups[i];

            if (group == null ||
                group.screen == null)
            {
                continue;
            }

            group.lastActiveState =
                group.screen.activeInHierarchy;
        }
    }


    private void HandleScreenSFXGroups()
    {
        if (screenSFXGroups == null ||
            screenSFXGroups.Count == 0)
        {
            return;
        }

        for (int i = 0;
             i < screenSFXGroups.Count;
             i++)
        {
            ScreenSFXGroup group =
                screenSFXGroups[i];

            if (group == null ||
                group.screen == null)
            {
                continue;
            }

            bool currentActiveState =
                group.screen.activeInHierarchy;


            if (!group.lastActiveState &&
                currentActiveState)
            {
                if (group.openSFX != null)
                {
                    PlaySFX(
                        group.openSFX,
                        group.openVolume
                    );
                }
            }
            else if (group.lastActiveState &&
                     !currentActiveState)
            {
                if (group.closeSFX != null)
                {
                    PlaySFX(
                        group.closeSFX,
                        group.closeVolume
                    );
                }
            }

            group.lastActiveState =
                currentActiveState;
        }
    }


    private void InitializeParticleStates()
    {
        if (particleSFXGroups == null)
        {
            return;
        }

        for (int i = 0;
             i < particleSFXGroups.Count;
             i++)
        {
            ParticleSFXGroup group =
                particleSFXGroups[i];

            if (group == null ||
                group.particle == null)
            {
                continue;
            }

            group.wasPlaying =
                group.particle.isPlaying;
        }
    }


    private void HandleParticleSFXGroups()
    {
        if (particleSFXGroups == null ||
            particleSFXGroups.Count == 0)
        {
            return;
        }

        for (int i = 0;
             i < particleSFXGroups.Count;
             i++)
        {
            ParticleSFXGroup group =
                particleSFXGroups[i];

            if (group == null ||
                group.particle == null ||
                group.sfx == null)
            {
                continue;
            }

            bool isPlaying =
                group.particle.isPlaying;

            /*
             * Particle stopped/not playing -> playing:
             * automatic particle SFX.
             */
            if (!group.wasPlaying &&
                isPlaying)
            {
                PlaySFX(
                    group.sfx,
                    group.volume
                );
            }

            group.wasPlaying =
                isPlaying;
        }
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


    /*
     * Call this method from any script immediately after particle.Play().
     *
     * Example:
     *
     * particle.Play();
     * centralSFXController.PlayParticleSFX(particle);
     *
     * Ye method exact particle ke corresponding SFX ko immediately play
     * karta hai. Isliye animation/event ke exact moment par sound sync
     * kiya ja sakta hai.
     */
    public void PlayParticleSFX(
        ParticleSystem particle)
    {
        if (particle == null ||
            particleSFXGroups == null)
        {
            return;
        }

        for (int i = 0;
             i < particleSFXGroups.Count;
             i++)
        {
            ParticleSFXGroup group =
                particleSFXGroups[i];

            if (group == null ||
                group.particle != particle ||
                group.sfx == null)
            {
                continue;
            }

            if (group.preventDuplicateWhilePlaying &&
                group.wasPlaying)
            {
                return;
            }

            PlaySFX(
                group.sfx,
                group.volume
            );

            group.wasPlaying = true;
            return;
        }
    }


    /*
     * Optional direct method:
     * Particle reference ke baghair group index se SFX play kar sakte ho.
     */
    public void PlayParticleSFXByGroupIndex(
        int groupIndex)
    {
        if (particleSFXGroups == null ||
            groupIndex < 0 ||
            groupIndex >= particleSFXGroups.Count)
        {
            return;
        }

        ParticleSFXGroup group =
            particleSFXGroups[groupIndex];

        if (group == null ||
            group.sfx == null)
        {
            return;
        }

        PlaySFX(
            group.sfx,
            group.volume
        );
    }


    /*
     * Convenience method:
     * Particle Play + SFX ek hi call mein.
     */
    public void PlayParticleWithSFX(
        ParticleSystem particle)
    {
        if (particle == null)
        {
            return;
        }

        particle.Play();

        PlayParticleSFX(
            particle
        );
    }


    public void PlayScreenOpenSFX(
        int groupIndex)
    {
        if (!IsValidScreenGroup(groupIndex))
        {
            return;
        }

        ScreenSFXGroup group =
            screenSFXGroups[groupIndex];

        if (group.openSFX != null)
        {
            PlaySFX(
                group.openSFX,
                group.openVolume
            );
        }
    }


    public void PlayScreenCloseSFX(
        int groupIndex)
    {
        if (!IsValidScreenGroup(groupIndex))
        {
            return;
        }

        ScreenSFXGroup group =
            screenSFXGroups[groupIndex];

        if (group.closeSFX != null)
        {
            PlaySFX(
                group.closeSFX,
                group.closeVolume
            );
        }
    }


    private bool IsValidScreenGroup(
        int index)
    {
        return
            screenSFXGroups != null &&
            index >= 0 &&
            index < screenSFXGroups.Count &&
            screenSFXGroups[index] != null;
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

        if (clip == null)
        {
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


    [ContextMenu("Refresh Screen SFX States")]
    private void RefreshScreenSFXStates()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        InitializeScreenStates();
    }


    [ContextMenu("Refresh Particle SFX States")]
    private void RefreshParticleSFXStates()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        InitializeParticleStates();
    }
#endif
}
















