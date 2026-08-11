using UnityEngine;

public sealed class GameRestartController : MonoBehaviour
{
    [SerializeField]
    private LevelRuntimeController levelRuntimeController;

    public void RestartGame()
    {
        if (levelRuntimeController == null)
        {
            levelRuntimeController =
                FindFirstObjectByType<LevelRuntimeController>();
        }

        if (levelRuntimeController == null)
        {
            Debug.LogError(
                "Restart ke liye LevelRuntimeController nahi mila.",
                this
            );
            return;
        }

        levelRuntimeController.RestartCurrentLevel();
    }
}
