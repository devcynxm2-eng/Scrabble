using UnityEngine;
using UnityEngine.UI;

public sealed class LifePlayController : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private Button playButton;


    [Header("References")]
    [SerializeField] private LifeManager lifeManager;
    [SerializeField] private LevelRuntimeController levelRuntimeController;


    private void OnEnable()
    {
        ResolveReferences();

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                HandlePlayPressed
            );

            playButton.onClick.AddListener(
                HandlePlayPressed
            );
        }
    }


    private void OnDisable()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                HandlePlayPressed
            );
        }
    }


    private void ResolveReferences()
    {
        if (lifeManager == null)
        {
            if (LifeManager.Instance != null)
            {
                lifeManager =
                    LifeManager.Instance;
            }
            else
            {
                lifeManager =
                    FindFirstObjectByType<LifeManager>(
                        FindObjectsInactive.Include
                    );
            }
        }

        if (levelRuntimeController == null)
        {
            levelRuntimeController =
                FindFirstObjectByType<LevelRuntimeController>(
                    FindObjectsInactive.Include
                );
        }
    }


    private void HandlePlayPressed()
    {
        ResolveReferences();

        if (lifeManager == null)
        {
            Debug.LogWarning(
                "LifePlayController: LifeManager missing hai.",
                this
            );

            return;
        }

        if (levelRuntimeController == null)
        {
            Debug.LogWarning(
                "LifePlayController: LevelRuntimeController missing hai.",
                this
            );

            return;
        }

        /*
         * Main Menu se PLAY = new gameplay run.
         * Sirf yahan 1 life consume hogi.
         *
         * Continue / next levels LifePlayController ko call nahi karte,
         * isliye same run mein additional life consume nahi hogi.
         */
        if (!lifeManager.TryConsumeLife())
        {
            Debug.Log(
                "PLAY blocked: player ke paas life available nahi hai.",
                this
            );

            return;
        }

        levelRuntimeController.PlayFromMainMenu();
    }
}
