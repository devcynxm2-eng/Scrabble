// using UnityEngine;
// using UnityEngine.UI;

// public sealed class PauseUIController : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private GameObject pausePanel;

//     [SerializeField] private Button pauseButton;
//     [SerializeField] private Button resumeButton;
//     [SerializeField] private Button restartButton;
//     [SerializeField] private Button homeButton;


//     [Header("Gameplay References")]
//     [SerializeField] private LevelRuntimeController levelRuntimeController;


//     private bool isPaused;


//     private void Awake()
//     {
//         if (pausePanel != null)
//         {
//             pausePanel.SetActive(false);
//         }
//     }


//     private void OnEnable()
//     {
//         ResolveReferences();

//         if (pauseButton != null)
//         {
//             pauseButton.onClick.RemoveListener(OpenPause);
//             pauseButton.onClick.AddListener(OpenPause);
//         }

//         if (resumeButton != null)
//         {
//             resumeButton.onClick.RemoveListener(ResumeGame);
//             resumeButton.onClick.AddListener(ResumeGame);
//         }

//         if (restartButton != null)
//         {
//             restartButton.onClick.RemoveListener(RestartLevel);
//             restartButton.onClick.AddListener(RestartLevel);
//         }

//         if (homeButton != null)
//         {
//             homeButton.onClick.RemoveListener(GoToHome);
//             homeButton.onClick.AddListener(GoToHome);
//         }
//     }


//     private void OnDisable()
//     {
//         if (pauseButton != null)
//         {
//             pauseButton.onClick.RemoveListener(OpenPause);
//         }

//         if (resumeButton != null)
//         {
//             resumeButton.onClick.RemoveListener(ResumeGame);
//         }

//         if (restartButton != null)
//         {
//             restartButton.onClick.RemoveListener(RestartLevel);
//         }

//         if (homeButton != null)
//         {
//             homeButton.onClick.RemoveListener(GoToHome);
//         }
//     }


//     private void ResolveReferences()
//     {
//         if (levelRuntimeController == null)
//         {
//             levelRuntimeController =
//                 FindFirstObjectByType<LevelRuntimeController>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     public void OpenPause()
//     {
//         if (isPaused)
//         {
//             return;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return;
//         }

//         isPaused = true;

//         if (pausePanel != null)
//         {
//             pausePanel.SetActive(true);
//         }

//         Time.timeScale = 0f;
//     }


//     public void ResumeGame()
//     {
//         isPaused = false;

//         if (pausePanel != null)
//         {
//             pausePanel.SetActive(false);
//         }

//         Time.timeScale = 1f;
//     }


//     public void RestartLevel()
//     {
//         isPaused = false;

//         if (pausePanel != null)
//         {
//             pausePanel.SetActive(false);
//         }

//         Time.timeScale = 1f;

//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.RestartCurrentLevel();
//         }
//         else
//         {
//             Debug.LogWarning(
//                 "PauseUIController: LevelRuntimeController missing hai.",
//                 this
//             );
//         }
//     }


//     public void GoToHome()
//     {
//         isPaused = false;

//         if (pausePanel != null)
//         {
//             pausePanel.SetActive(false);
//         }

//         Time.timeScale = 1f;

//         UIEventBroker.RequestScreen(
//             UIScreenType.MainMenu
//         );

//         if (levelRuntimeController != null)
//         {
//             levelRuntimeController.ShowMainMenu();
//         }
//         else
//         {
//             Debug.LogWarning(
//                 "PauseUIController: LevelRuntimeController missing hai.",
//                 this
//             );
//         }
//     }


//     public void ForceClose()
//     {
//         isPaused = false;

//         if (pausePanel != null)
//         {
//             pausePanel.SetActive(false);
//         }

//         Time.timeScale = 1f;
//     }
// }





using UnityEngine;
using UnityEngine.UI;

public sealed class PauseUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button homeButton;


    [Header("Gameplay References")]
    [SerializeField] private LevelRuntimeController levelRuntimeController;

    [SerializeField]
    private PopupGameplayVisibilityController popupGameplayVisibility;


    private bool isPaused;


    private void Awake()
    {
        if (pausePanel != null)
        {
            UITransition.HideImmediate(pausePanel);
        }
    }


    private void OnEnable()
    {
        ResolveReferences();

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OpenPause);
            pauseButton.onClick.AddListener(OpenPause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartLevel);
            restartButton.onClick.AddListener(RestartLevel);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(GoToHome);
            homeButton.onClick.AddListener(GoToHome);
        }
    }


    private void OnDisable()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OpenPause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartLevel);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(GoToHome);
        }
    }


    private void ResolveReferences()
    {
        if (levelRuntimeController == null)
        {
            levelRuntimeController =
                FindFirstObjectByType<LevelRuntimeController>(
                    FindObjectsInactive.Include
                );
        }

        if (popupGameplayVisibility == null)
        {
            popupGameplayVisibility =
                FindFirstObjectByType<PopupGameplayVisibilityController>(
                    FindObjectsInactive.Include
                );
        }
    }


    public void OpenPause()
    {
        if (isPaused)
        {
            return;
        }

        if (levelRuntimeController != null &&
            !levelRuntimeController.IsLevelGenerated)
        {
            return;
        }

        isPaused = true;

        popupGameplayVisibility?.HideGameplay();

        if (pausePanel != null)
        {
            UITransition.Show(pausePanel);
        }

        Time.timeScale = 0f;
    }


    public void ResumeGame()
    {
        isPaused = false;

        /*
         * Yahan animated Hide use nahi karte.
         *
         * Pause panel ka Canvas Screen Space - Camera hai aur uska plane
         * camera se 100 units door hai, jabke poora gameplay world sirf
         * 2-7 units par baitha hai. Is liye jis lamhe dono ek sath screen
         * par hon, tower UI ke ooper draw hota hai.
         *
         * Animated Hide panel ko 0.22s tak visible rakhta hai jabke neeche
         * ShowGameplay() tower ko foran wapas le aata hai — usi window mein
         * UI tower ke peechay chali jati hai. HideImmediate se overlap
         * bilkul khatam ho jata hai.
         */
        if (pausePanel != null)
        {
            UITransition.HideImmediate(pausePanel);
        }

        popupGameplayVisibility?.ShowGameplay();

        Time.timeScale = 1f;
    }


    public void RestartLevel()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            UITransition.Hide(pausePanel);
        }

        popupGameplayVisibility?.ShowGameplay();

        Time.timeScale = 1f;

        if (levelRuntimeController != null)
        {
            levelRuntimeController.RestartCurrentLevel();
        }
        else
        {
            Debug.LogWarning(
                "PauseUIController: LevelRuntimeController missing hai.",
                this
            );
        }
    }


    public void GoToHome()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            UITransition.Hide(pausePanel);
        }

        Time.timeScale = 1f;

        UIEventBroker.RequestScreen(
            UIScreenType.MainMenu
        );

        if (levelRuntimeController != null)
        {
            levelRuntimeController.ShowMainMenu();
        }
        else
        {
            Debug.LogWarning(
                "PauseUIController: LevelRuntimeController missing hai.",
                this
            );
        }
    }


    public void ForceClose()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            UITransition.Hide(pausePanel);
        }

        popupGameplayVisibility?.ShowGameplay();

        Time.timeScale = 1f;
    }
}
