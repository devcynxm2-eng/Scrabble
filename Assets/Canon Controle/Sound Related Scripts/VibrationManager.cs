// using UnityEngine;

// public sealed class VibrationManager : MonoBehaviour
// {
//     private const string VibrationEnabledKey =
//         "RoyalSmash.VibrationEnabled";


//     public static VibrationManager Instance { get; private set; }


//     [Header("Behaviour")]
//     [SerializeField] private bool dontDestroyOnLoad = true;

//     [SerializeField] private bool debugLogInEditor = false;


//     private bool vibrationEnabled = true;


//     public bool IsVibrationEnabled =>
//         vibrationEnabled;


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

//         LoadSetting();
//     }


//     private void LoadSetting()
//     {
//         vibrationEnabled =
//             PlayerPrefs.GetInt(
//                 VibrationEnabledKey,
//                 1
//             ) == 1;
//     }


//     public void SetVibrationEnabled(
//         bool isEnabled)
//     {
//         vibrationEnabled =
//             isEnabled;

//         PlayerPrefs.SetInt(
//             VibrationEnabledKey,
//             vibrationEnabled ? 1 : 0
//         );

//         PlayerPrefs.Save();
//     }


//     public void Vibrate()
//     {
//         if (!vibrationEnabled)
//         {
//             return;
//         }

// #if UNITY_ANDROID || UNITY_IOS
//         Handheld.Vibrate();
// #else
//         if (debugLogInEditor)
//         {
//             Debug.Log(
//                 "VibrationManager: Vibrate() called. " +
//                 "Actual vibration mobile device par chalegi.",
//                 this
//             );
//         }
// #endif
//     }
// }














using UnityEngine;

public sealed class VibrationManager : MonoBehaviour
{
    private const string VibrationEnabledKey =
        "RoyalSmash.VibrationEnabled";


    public static VibrationManager Instance { get; private set; }


    [Header("Behaviour")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [SerializeField] private bool debugLogInEditor = false;


    private bool vibrationEnabled = true;


    public bool IsVibrationEnabled =>
        vibrationEnabled;


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

        LoadSetting();
    }


    private void LoadSetting()
    {
        vibrationEnabled =
            PlayerPrefs.GetInt(
                VibrationEnabledKey,
                1
            ) == 1;
    }


    public void SetVibrationEnabled(
        bool isEnabled)
    {
        vibrationEnabled =
            isEnabled;

        PlayerPrefs.SetInt(
            VibrationEnabledKey,
            vibrationEnabled ? 1 : 0
        );

        PlayerPrefs.Save();
    }


    /// <summary>
    /// Generic vibration call.
    /// </summary>
    public void Vibrate()
    {
        if (!vibrationEnabled)
        {
            return;
        }

#if UNITY_ANDROID || UNITY_IOS

        Handheld.Vibrate();

#else

        if (debugLogInEditor)
        {
            Debug.Log(
                "VibrationManager: Vibrate() called.",
                this
            );
        }

#endif
    }


    /// <summary>
    /// Ball hits block / tower object.
    /// </summary>
    public void PlayImpactVibration()
    {
        if (!vibrationEnabled)
        {
            return;
        }

#if UNITY_ANDROID || UNITY_IOS

        Handheld.Vibrate();

#else

        if (debugLogInEditor)
        {
            Debug.Log(
                "VibrationManager: Impact vibration called.",
                this
            );
        }

#endif
    }


    /// <summary>
    /// Ball reaches lower ground and disappears.
    /// </summary>
    public void PlayFallVibration()
    {
        if (!vibrationEnabled)
        {
            return;
        }

#if UNITY_ANDROID || UNITY_IOS

        Handheld.Vibrate();

#else

        if (debugLogInEditor)
        {
            Debug.Log(
                "VibrationManager: Fall vibration called.",
                this
            );
        }

#endif
    }
}








