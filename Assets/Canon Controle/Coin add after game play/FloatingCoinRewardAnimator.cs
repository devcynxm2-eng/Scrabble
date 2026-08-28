using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public sealed class FloatingCoinRewardAnimator : MonoBehaviour
{
    [Header("Coin Setup")]
    [SerializeField]
    private GameObject coinPrefab;


    [SerializeField]
    private RectTransform spawnParent;


    [SerializeField]
    private RectTransform coinDestination;



    [Header("Spawn")]
    [SerializeField, Min(1)]
    private int minCoins = 8;


    [SerializeField, Min(1)]
    private int maxCoins = 9;


    [SerializeField]
    private float spawnRadius = 250f;



    [Header("Rotation")]
    [SerializeField]
    private float rotationSpeed = 360f;



    [Header("Floating")]
    [SerializeField]
    private float floatHeight = 20f;


    [SerializeField]
    private float floatDuration = 1f;



    [Header("Collect")]
    [SerializeField]
    private float collectDelay = 0.8f;


    [SerializeField]
    private float moveDuration = 0.8f;



    [Header("Behaviour")]
    [SerializeField]
    private bool destroyAfterCollect = true;


    [Header("Reward References")]
    [SerializeField]
    private Button playButton;

    [SerializeField]
    private ScoreManager scoreManager;

    [SerializeField]
    private LevelRuntimeController levelRuntimeController;



    private readonly List<GameObject> activeCoins =
        new List<GameObject>();

    private Coroutine spawnRoutine;
    private bool isAnimating;
    private int coinsWaitingForDestination;


    public event System.Action CoinsCollected;



    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
        StartCoroutine(
            TryPlayPendingRewardNextFrame()
        );
    }



    private void OnDisable()
    {
        Unsubscribe();

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        isAnimating = false;
        coinsWaitingForDestination = 0;

        if (playButton != null)
        {
            playButton.interactable = true;
        }

        ClearCoins();
    }



    public void PlayFloatingCoins()
    {
        TryPlayPendingReward();
    }



    private void ResolveReferences()
    {
        if (scoreManager == null)
        {
            scoreManager = ScoreManager.Instance != null
                ? ScoreManager.Instance
                : FindFirstObjectByType<ScoreManager>(
                    FindObjectsInactive.Include
                );
        }

        if (levelRuntimeController == null)
        {
            levelRuntimeController =
                FindFirstObjectByType<LevelRuntimeController>(
                    FindObjectsInactive.Include
                );
        }
    }



    private void Subscribe()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        levelRuntimeController.MainMenuShown -=
            HandleMainMenuShown;

        levelRuntimeController.MainMenuShown +=
            HandleMainMenuShown;
    }



    private void Unsubscribe()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        levelRuntimeController.MainMenuShown -=
            HandleMainMenuShown;
    }



    private void HandleMainMenuShown()
    {
        TryPlayPendingReward();
    }



    private IEnumerator TryPlayPendingRewardNextFrame()
    {
        yield return null;
        ResolveReferences();
        Subscribe();
        TryPlayPendingReward();
    }



    private void TryPlayPendingReward()
    {
        ResolveReferences();

        if (isAnimating ||
            scoreManager == null ||
            !scoreManager.HasPendingMilestoneScore ||
            levelRuntimeController == null ||
            !levelRuntimeController.IsMainMenuVisible)
        {
            return;
        }

        ClearCoins();

        isAnimating = true;

        if (playButton != null)
        {
            playButton.interactable = false;
        }

        spawnRoutine = StartCoroutine(
            SpawnCoinsRoutine()
        );
    }



    private IEnumerator SpawnCoinsRoutine()
    {
        int amount =
            Random.Range(
                minCoins,
                maxCoins + 1
            );


        for(int i = 0; i < amount; i++)
        {
            SpawnCoin();

            yield return new WaitForSecondsRealtime(
                0.05f
            );
        }


        yield return new WaitForSecondsRealtime(
            collectDelay
        );


        spawnRoutine = null;
        CollectCoins();
    }




    private void SpawnCoin()
    {
        if(coinPrefab == null ||
           spawnParent == null)
        {
            return;
        }


        GameObject coin =
            Instantiate(
                coinPrefab,
                spawnParent
            );


        RectTransform rect =
            coin.GetComponent<RectTransform>();


        if(rect == null)
        {
            Destroy(coin);
            return;
        }



        rect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );


        rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        rect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        rect.anchoredPosition =
            Vector2.zero;



        Vector2 randomPosition =
            Random.insideUnitCircle *
            spawnRadius;



        activeCoins.Add(
            coin
        );



        // Spread animation
        rect.DOAnchorPos(
                randomPosition,
                0.45f
            )
            .SetEase(
                Ease.OutBack
            )
            .SetUpdate(true);



        // Rotation
        rect.DORotate(
                new Vector3(
                    0f,
                    rotationSpeed,
                    0f
                ),
                1f,
                RotateMode.FastBeyond360
            )
            .SetLoops(
                -1,
                LoopType.Restart
            )
            .SetEase(
                Ease.Linear
            )
            .SetUpdate(true);



        // Floating
        rect.DOAnchorPosY(
                randomPosition.y + floatHeight,
                floatDuration
            )
            .SetLoops(
                -1,
                LoopType.Yoyo
            )
            .SetEase(
                Ease.InOutSine
            )
            .SetUpdate(true);
    }





    private void CollectCoins()
    {
        List<GameObject> coinsToCollect =
            new List<GameObject>(activeCoins);

        coinsWaitingForDestination = 0;

        foreach(GameObject coin in coinsToCollect)
        {
            if(coin != null)
            {
                coinsWaitingForDestination++;

                MoveCoinToTarget(
                    coin
                );
            }
        }

        if (coinsWaitingForDestination <= 0)
        {
            CompletePendingReward();
        }
    }





    private void MoveCoinToTarget(
        GameObject coin)
    {
        if(coin == null ||
           coinDestination == null)
        {
            HandleCoinReachedDestination();
            return;
        }


        RectTransform rect =
            coin.GetComponent<RectTransform>();


        if(rect == null)
        {
            HandleCoinReachedDestination();
            return;
        }



        rect.DOKill();



        rect.DOMove(
                coinDestination.position,
                moveDuration
            )
            .SetEase(
                Ease.InBack
            )
            .SetUpdate(true)
            .OnComplete(
                () =>
                {
                    if(destroyAfterCollect &&
                       coin != null)
                    {
                        activeCoins.Remove(
                            coin
                        );

                        Destroy(
                            coin
                        );
                    }

                    HandleCoinReachedDestination();
                }
            );
    }



    private void HandleCoinReachedDestination()
    {
        coinsWaitingForDestination = Mathf.Max(
            0,
            coinsWaitingForDestination - 1
        );

        if (coinsWaitingForDestination == 0)
        {
            CompletePendingReward();
        }
    }



    private void CompletePendingReward()
    {
        if (!isAnimating)
        {
            return;
        }

        isAnimating = false;
        coinsWaitingForDestination = 0;

        if (playButton != null)
        {
            playButton.interactable = true;
        }

        if (scoreManager != null)
        {
            scoreManager.ClaimPendingMilestoneScore();
        }

        CoinsCollected?.Invoke();
    }





    private void ClearCoins()
    {
        foreach(GameObject coin in activeCoins)
        {
            if(coin != null)
            {
                coin.transform.DOKill();

                Destroy(
                    coin
                );
            }
        }


        activeCoins.Clear();
    }




    private void OnDestroy()
    {
        ClearCoins();
    }
}
