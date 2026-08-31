// using UnityEngine;
// using UnityEngine.UI;

// public class MainScreenController : MonoBehaviour
// {
//     [Header("Home Screen Buttons")]
//     [SerializeField] private Button rewardButton;
//     [SerializeField] private Button settingButton;
//     [SerializeField] private Button playButton;


//     private void Start()
//     {
//         // Start game on Home Screen
//         UIEventBroker.RequestScreen(UIScreenType.MainMenu);


//         if (rewardButton != null)
//         {
//             rewardButton.onClick.AddListener(OpenReward);
//         }


//         if (settingButton != null)
//         {
//             settingButton.onClick.AddListener(OpenSettings);
//         }


//         if (playButton != null)
//         {
//             playButton.onClick.AddListener(OpenGameplay);
//         }
//     }


//     private void OnDestroy()
//     {
//         if (rewardButton != null)
//         {
//             rewardButton.onClick.RemoveListener(OpenReward);
//         }


//         if (settingButton != null)
//         {
//             settingButton.onClick.RemoveListener(OpenSettings);
//         }


//         if (playButton != null)
//         {
//             playButton.onClick.RemoveListener(OpenGameplay);
//         }
//     }


//     private void OpenReward()
//     {
//         UIEventBroker.RequestScreen(UIScreenType.RewardScreen);
//     }


//     private void OpenSettings()
//     {
//         UIEventBroker.RequestScreen(UIScreenType.SettingScreen);
//     }


//     private void OpenGameplay()
//     {
//         UIEventBroker.RequestScreen(UIScreenType.GamePlayScreen);
//     }
// }











// using UnityEngine;
// using UnityEngine.UI;

// public sealed class MainScreenController : MonoBehaviour
// {
//     [Header("Home Screen Buttons")]
//     [SerializeField] private Button rewardButton;
//     [SerializeField] private Button settingButton;


//     private void Start()
//     {
//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );


//         if (rewardButton != null)
//         {
//             rewardButton.onClick.RemoveListener(
//                 OpenReward
//             );

//             rewardButton.onClick.AddListener(
//                 OpenReward
//             );
//         }


//         if (settingButton != null)
//         {
//             settingButton.onClick.RemoveListener(
//                 OpenSettings
//             );

//             settingButton.onClick.AddListener(
//                 OpenSettings
//             );
//         }
//     }


//     private void OnDestroy()
//     {
//         if (rewardButton != null)
//         {
//             rewardButton.onClick.RemoveListener(
//                 OpenReward
//             );
//         }


//         if (settingButton != null)
//         {
//             settingButton.onClick.RemoveListener(
//                 OpenSettings
//             );
//         }
//     }


//     private void OpenReward()
//     {
//         UIEventBroker.RequestScreen(
//             UIScreenType.RewardScreen
//         );
//     }


//     private void OpenSettings()
//     {
//         UIEventBroker.RequestScreen(
//             UIScreenType.SettingScreen
//         );
//     }
// }



// using UnityEngine;
// using UnityEngine.UI;

// public sealed class MainScreenController : MonoBehaviour
// {
//     [Header("Home Screen Buttons")]
//     [SerializeField] private Button rewardButton;
//     [SerializeField] private Button settingButton;


//     [Header("Profile")]
//     [SerializeField] private PlayerProfile playerProfile;
//     [SerializeField] private Image profileAvatarImage;


//     private void Start()
//     {
//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );


//         if (rewardButton != null)
//         {
//             rewardButton.onClick.RemoveListener(
//                 OpenReward
//             );

//             rewardButton.onClick.AddListener(
//                 OpenReward
//             );
//         }


//         if (settingButton != null)
//         {
//             settingButton.onClick.RemoveListener(
//                 OpenSettings
//             );

//             settingButton.onClick.AddListener(
//                 OpenSettings
//             );
//         }


//         RefreshProfileAvatar();
//     }


//     private void OnEnable()
//     {
//         // Important:
//         // Agar screen switcher is GameObject ko SetActive(true/false)
//         // karke show/hide karta hai (settings se wapas main menu par
//         // aane par), to avatar yahan se refresh ho jayega automatically.
//         RefreshProfileAvatar();
//     }


//     private void OnDestroy()
//     {
//         if (rewardButton != null)
//         {
//             rewardButton.onClick.RemoveListener(
//                 OpenReward
//             );
//         }


//         if (settingButton != null)
//         {
//             settingButton.onClick.RemoveListener(
//                 OpenSettings
//             );
//         }
//     }


//     private void OpenReward()
//     {
//         UIEventBroker.RequestScreen(
//             UIScreenType.RewardScreen
//         );
//     }


//     private void OpenSettings()
//     {
//         UIEventBroker.RequestScreen(
//             UIScreenType.SettingScreen
//         );
//     }


//     /// <summary>
//     /// Public is liye hai ke profile update hote hi ProfileManager isay
//     /// turant call kar sake. Main Panel overlay ke peechay active hi
//     /// rehta hai, is liye OnEnable dobara fire nahi hota aur avatar
//     /// khud se refresh nahi hota tha.
//     /// </summary>
//     public void RefreshProfileAvatar()
//     {
//         if (playerProfile == null)
//         {
//             Debug.LogWarning(
//                 "MainScreenController: PlayerProfile not assigned."
//             );

//             return;
//         }

//         if (profileAvatarImage == null)
//             return;

//         playerProfile.Load();

//         /*
//          * Saved avatar ho to wo laga dete hain. Na ho to scene mein jo
//          * sprite pehle se assign hai wahi rehne dete hain.
//          *
//          * Image ko kisi bhi soorat mein disable NAHI karna — pehle
//          * avatar select na hone par ye off ho jati thi aur main menu se
//          * Profile Image gayab ho jati thi.
//          */
//         if (playerProfile.AvatarSprite != null)
//         {
//             profileAvatarImage.sprite = playerProfile.AvatarSprite;
//         }

//         profileAvatarImage.enabled = true;

//         profileAvatarImage.preserveAspect = true;
//         profileAvatarImage.type = Image.Type.Simple;
//     }
// }










using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class MainScreenController : MonoBehaviour
{
    [Header("Home Screen Buttons")]
    [SerializeField] private Button rewardButton;
    [SerializeField] private Button settingButton;


    [Header("Profile")]
    [SerializeField] private PlayerProfile playerProfile;

    [SerializeField] private Image profileAvatarImage;

    [SerializeField] private TMP_Text profileNameText;


    private void Start()
    {
        UIEventBroker.RequestScreen(
            UIScreenType.MainMenu
        );


        if (rewardButton != null)
        {
            rewardButton.onClick.RemoveListener(
                OpenReward
            );

            rewardButton.onClick.AddListener(
                OpenReward
            );
        }


        if (settingButton != null)
        {
            settingButton.onClick.RemoveListener(
                OpenSettings
            );

            settingButton.onClick.AddListener(
                OpenSettings
            );
        }


        RefreshProfile();
    }


    private void OnEnable()
    {
        /*
         * Main menu dobara active hone par profile image
         * aur name dono refresh honge.
         */
        RefreshProfile();
    }


    private void OnDestroy()
    {
        if (rewardButton != null)
        {
            rewardButton.onClick.RemoveListener(
                OpenReward
            );
        }


        if (settingButton != null)
        {
            settingButton.onClick.RemoveListener(
                OpenSettings
            );
        }
    }


    private void OpenReward()
    {
        UIEventBroker.RequestScreen(
            UIScreenType.RewardScreen
        );
    }


    private void OpenSettings()
    {
        UIEventBroker.RequestScreen(
            UIScreenType.SettingScreen
        );
    }


    /// <summary>
    /// Profile image + player name dono refresh karta hai.
    /// ProfileManager edit complete hone ke baad isay call karega.
    /// </summary>
    public void RefreshProfile()
    {
        if (playerProfile == null)
        {
            Debug.LogWarning(
                "MainScreenController: PlayerProfile not assigned."
            );

            return;
        }


        playerProfile.Load();


        // ==========================
        // PROFILE IMAGE
        // ==========================

        if (profileAvatarImage != null)
        {
            /*
             * Saved avatar ho to wo show karo.
             * Agar saved avatar nahi hai to existing
             * Inspector sprite ko change nahi karte.
             */
            if (playerProfile.AvatarSprite != null)
            {
                profileAvatarImage.sprite =
                    playerProfile.AvatarSprite;
            }

            profileAvatarImage.enabled = true;
            profileAvatarImage.preserveAspect = true;
            profileAvatarImage.type =
                Image.Type.Simple;
        }


        // ==========================
        // PROFILE NAME
        // ==========================

        if (profileNameText != null)
        {
            string playerName =
                playerProfile.PlayerName;

            /*
             * Agar name empty hai to default text.
             * Isay apni requirement ke mutabiq change kar sakte ho.
             */
            if (string.IsNullOrWhiteSpace(playerName))
            {
                profileNameText.text = "Player";
            }
            else
            {
                profileNameText.text = playerName;
            }
        }
    }


    /// <summary>
    /// Backward compatibility ke liye purana method bhi rakha hai.
    /// Agar kisi existing script/Button se RefreshProfileAvatar()
    /// call ho raha hai to wo bhi image + name update karega.
    /// </summary>
    public void RefreshProfileAvatar()
    {
        RefreshProfile();
    }
}