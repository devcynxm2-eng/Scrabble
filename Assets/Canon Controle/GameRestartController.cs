using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameRestartController : MonoBehaviour
{
    public void RestartGame()
    {
        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }
}