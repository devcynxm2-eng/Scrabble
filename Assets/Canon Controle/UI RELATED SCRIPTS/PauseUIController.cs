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


    private bool isPaused;


    private void Awake()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
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

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }


    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }


    public void RestartLevel()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

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
            pausePanel.SetActive(false);
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
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }
}