using TMPro;
using UnityEngine;

public sealed class ScoreUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text scoreText;

    [SerializeField]
    private string prefix = "";


    [Header("Reference")]
    [SerializeField]
    private ScoreManager scoreManager;


    private void OnEnable()
    {
        ResolveScoreManager();
        Subscribe();
        Refresh();
    }


    private void OnDisable()
    {
        Unsubscribe();
    }


    private void ResolveScoreManager()
    {
        if (scoreManager != null)
        {
            return;
        }

        if (ScoreManager.Instance != null)
        {
            scoreManager =
                ScoreManager.Instance;

            return;
        }

        scoreManager =
            FindFirstObjectByType<ScoreManager>(
                FindObjectsInactive.Include
            );
    }


    private void Subscribe()
    {
        if (scoreManager == null)
        {
            return;
        }

        scoreManager.ScoreChanged -=
            HandleScoreChanged;

        scoreManager.ScoreChanged +=
            HandleScoreChanged;
    }


    private void Unsubscribe()
    {
        if (scoreManager == null)
        {
            return;
        }

        scoreManager.ScoreChanged -=
            HandleScoreChanged;
    }


    private void Refresh()
    {
        if (scoreManager == null)
        {
            SetText(0);
            return;
        }

        SetText(
            scoreManager.CurrentScore
        );
    }


    private void HandleScoreChanged(
        int newScore)
    {
        SetText(
            newScore
        );
    }


    private void SetText(
        int score)
    {
        if (scoreText == null)
        {
            return;
        }

        scoreText.text =
            $"{prefix}{score}";
    }
}
