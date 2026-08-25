// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public sealed class CoinRewardAnimator : MonoBehaviour
// {
//     [Header("Coin Setup")]
//     [SerializeField] private GameObject coinPrefab;

//     [SerializeField] private RectTransform spawnParent;

//     [SerializeField] private RectTransform coinTarget;


//     [Header("Animation")]
//     [SerializeField, Min(1)]
//     private int visualCoinCount = 12;


//     [SerializeField, Min(10f)]
//     private float spawnRadius = 150f;


//     [SerializeField, Min(0.1f)]
//     private float moveDuration = 0.8f;


//     [SerializeField, Min(0f)]
//     private float spawnDelay = 0.04f;


//     [Header("Behaviour")]
//     [SerializeField]
//     private bool destroyAfterComplete = true;


//     public event Action AnimationCompleted;


//     private readonly List<GameObject> activeCoins =
//         new List<GameObject>();


//     public void PlayRewardAnimation(
//         int rewardAmount)
//     {
//         if (coinPrefab == null ||
//             spawnParent == null ||
//             coinTarget == null)
//         {
//             Debug.LogWarning(
//                 "CoinRewardAnimator: References missing.",
//                 this
//             );

//             AnimationCompleted?.Invoke();
//             return;
//         }


//         StopAllCoroutines();

//         ClearCoins();


//         StartCoroutine(
//             SpawnCoinsRoutine()
//         );
//     }



//     private IEnumerator SpawnCoinsRoutine()
//     {
//         for (int i = 0;
//              i < visualCoinCount;
//              i++)
//         {
//             GameObject coin =
//                 Instantiate(
//                     coinPrefab,
//                     spawnParent
//                 );


//             RectTransform rect =
//                 coin.GetComponent<RectTransform>();


//             if (rect != null)
//             {
//                 rect.localScale =
//                     Vector3.one;


//                 rect.anchorMin =
//                     new Vector2(
//                         0.5f,
//                         0.5f
//                     );


//                 rect.anchorMax =
//                     new Vector2(
//                         0.5f,
//                         0.5f
//                     );


//                 rect.pivot =
//                     new Vector2(
//                         0.5f,
//                         0.5f
//                     );


//                 rect.anchoredPosition =
//                     UnityEngine.Random.insideUnitCircle *
//                     spawnRadius;
//             }


//             activeCoins.Add(
//                 coin
//             );


//             StartCoroutine(
//                 AnimateCoin(
//                     coin
//                 )
//             );


//             yield return new WaitForSeconds(
//                 spawnDelay
//             );
//         }


//         yield return new WaitForSeconds(
//             moveDuration
//         );


//         AnimationCompleted?.Invoke();
//     }



//     private IEnumerator AnimateCoin(
//         GameObject coin)
//     {
//         if (coin == null)
//         {
//             yield break;
//         }


//         RectTransform rect =
//             coin.GetComponent<RectTransform>();


//         if (rect == null)
//         {
//             yield break;
//         }


//         Vector3 startPosition =
//             rect.position;


//         float elapsed = 0f;


//         while (elapsed < moveDuration)
//         {
//             if (coin == null)
//             {
//                 yield break;
//             }


//             elapsed +=
//                 Time.unscaledDeltaTime;


//             float progress =
//                 Mathf.Clamp01(
//                     elapsed /
//                     moveDuration
//                 );


//             progress =
//                 Mathf.SmoothStep(
//                     0f,
//                     1f,
//                     progress
//                 );


//             rect.position =
//                 Vector3.Lerp(
//                     startPosition,
//                     coinTarget.position,
//                     progress
//                 );


//             yield return null;
//         }


//         if (destroyAfterComplete &&
//             coin != null)
//         {
//             Destroy(
//                 coin
//             );
//         }
//     }



//     private void ClearCoins()
//     {
//         foreach (GameObject coin in activeCoins)
//         {
//             if (coin != null)
//             {
//                 Destroy(
//                     coin
//                 );
//             }
//         }


//         activeCoins.Clear();
//     }
// }




// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public sealed class CoinRewardAnimator : MonoBehaviour
// {
//     [Header("Coin Setup")]
//     [SerializeField] private GameObject coinPrefab;
//     [SerializeField] private RectTransform spawnParent;
//     [SerializeField] private RectTransform coinTarget;


//     [Header("Scatter")]
//     [SerializeField, Min(1)]
//     private int visualCoinCount = 12;

//     [SerializeField, Min(10f)]
//     private float scatterRadius = 220f;


//     [Header("Collect Animation")]
//     [SerializeField, Min(0.1f)]
//     private float collectDuration = 0.8f;

//     [SerializeField, Min(0f)]
//     private float spawnDelay = 0.03f;


//     [Header("Behaviour")]
//     [SerializeField]
//     private bool destroyAfterCollect = true;


//     public event Action CoinsCollected;


//     private readonly List<GameObject> activeCoins =
//         new List<GameObject>();


//     /// <summary>
//     /// Call when reward popup opens.
//     /// Coins will appear scattered and stay there.
//     /// </summary>
//     public void ShowCoins()
//     {
//         ClearCoins();

//         StartCoroutine(
//             SpawnCoinsRoutine()
//         );
//     }



//     private IEnumerator SpawnCoinsRoutine()
//     {
//         for (int i = 0;
//              i < visualCoinCount;
//              i++)
//         {
//             GameObject coin =
//                 Instantiate(
//                     coinPrefab,
//                     spawnParent
//                 );


//             coin.transform.SetAsLastSibling();


//             RectTransform rect =
//                 coin.GetComponent<RectTransform>();


//             if (rect != null)
//             {
//                 rect.localScale =
//                     Vector3.one;


//                 rect.anchorMin =
//                     new Vector2(
//                         0.5f,
//                         0.5f
//                     );


//                 rect.anchorMax =
//                     new Vector2(
//                         0.5f,
//                         0.5f
//                     );


//                 rect.pivot =
//                     new Vector2(
//                         0.5f,
//                         0.5f
//                     );


//                 rect.anchoredPosition =
//                     UnityEngine.Random.insideUnitCircle *
//                     scatterRadius;
//             }


//             activeCoins.Add(
//                 coin
//             );


//             yield return new WaitForSeconds(
//                 spawnDelay
//             );
//         }
//     }



//     /// <summary>
//     /// Call from CLAIM button.
//     /// Coins will fly to coin counter.
//     /// </summary>
//     public void CollectCoins()
//     {
//         StartCoroutine(
//             CollectCoinsRoutine()
//         );
//     }



//     private IEnumerator CollectCoinsRoutine()
//     {
//         List<GameObject> coinsCopy =
//             new List<GameObject>(
//                 activeCoins
//             );


//         foreach (GameObject coin in coinsCopy)
//         {
//             if (coin != null)
//             {
//                 StartCoroutine(
//                     MoveCoin(
//                         coin
//                     )
//                 );


//                 yield return new WaitForSeconds(
//                     spawnDelay
//                 );
//             }
//         }


//         yield return new WaitForSeconds(
//             collectDuration
//         );


//         CoinsCollected?.Invoke();
//     }



//     private IEnumerator MoveCoin(
//         GameObject coin)
//     {
//         if (coin == null)
//         {
//             yield break;
//         }


//         RectTransform rect =
//             coin.GetComponent<RectTransform>();


//         if (rect == null)
//         {
//             yield break;
//         }


//         Vector3 startPosition =
//             rect.position;


//         float elapsed = 0f;


//         while (elapsed < collectDuration)
//         {
//             if (coin == null)
//             {
//                 yield break;
//             }


//             elapsed +=
//                 Time.unscaledDeltaTime;


//             float progress =
//                 Mathf.Clamp01(
//                     elapsed /
//                     collectDuration
//                 );


//             progress =
//                 Mathf.SmoothStep(
//                     0f,
//                     1f,
//                     progress
//                 );


//             rect.position =
//                 Vector3.Lerp(
//                     startPosition,
//                     coinTarget.position,
//                     progress
//                 );


//             yield return null;
//         }


//         if (destroyAfterCollect &&
//             coin != null)
//         {
//             Destroy(
//                 coin
//             );
//         }
//     }



//     private void ClearCoins()
//     {
//         foreach (GameObject coin in activeCoins)
//         {
//             if (coin != null)
//             {
//                 Destroy(
//                     coin
//                 );
//             }
//         }


//         activeCoins.Clear();
//     }
// }



// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;


// public sealed class CoinRewardAnimator : MonoBehaviour
// {
//     [Header("Coin Setup")]
//     [SerializeField] private GameObject coinPrefab;
//     [SerializeField] private RectTransform spawnParent;
//     [SerializeField] private RectTransform coinTarget;


//     [Header("Burst Effect")]
//     [SerializeField, Min(1)]
//     private int visualCoinCount = 12;

//     [SerializeField, Min(50f)]
//     private float burstDistance = 180f;

//     [SerializeField]
//     private float burstDuration = 0.35f;


//     [Header("Collect Animation")]
//     [SerializeField]
//     private float collectDuration = 0.7f;

//     [SerializeField]
//     private float visibleDelayBeforeCollect = 0.25f;


//     [Header("Spawn")]
//     [SerializeField]
//     private float spawnDelay = 0.03f;


//     [Header("Coin Motion")]
//     [SerializeField]
//     private float rotationSpeed = 720f;

//     [SerializeField]
//     private float bounceHeight = 35f;


//     [Header("Behaviour")]
//     [SerializeField]
//     private bool destroyAfterCollect = true;


//     public event Action CoinsCollected;


//     private readonly List<GameObject> activeCoins =
//         new List<GameObject>();


//     private readonly Dictionary<GameObject, Vector3> coinStartPositions =
//         new Dictionary<GameObject, Vector3>();


//     public void PlayRewardCollection()
//     {
//         ClearCoins();

//         StartCoroutine(
//             SpawnThenCollectRoutine()
//         );
//     }



//     private IEnumerator SpawnThenCollectRoutine()
//     {
//         yield return StartCoroutine(
//             SpawnCoinsRoutine()
//         );


//         yield return new WaitForSeconds(
//             visibleDelayBeforeCollect
//         );


//         yield return StartCoroutine(
//             CollectCoinsRoutine()
//         );
//     }



//     private IEnumerator SpawnCoinsRoutine()
//     {
//         for (int i = 0; i < visualCoinCount; i++)
//         {
//             GameObject coin =
//                 Instantiate(
//                     coinPrefab,
//                     spawnParent
//                 );


//             coin.transform.SetAsLastSibling();


//             RectTransform rect =
//                 coin.GetComponent<RectTransform>();


//             if (rect != null)
//             {
//                 rect.localScale =
//                     Vector3.one;


//                 rect.anchorMin =
//                     new Vector2(0.5f, 0.5f);

//                 rect.anchorMax =
//                     new Vector2(0.5f, 0.5f);

//                 rect.pivot =
//                     new Vector2(0.5f, 0.5f);


//                 rect.anchoredPosition =
//                     Vector2.zero;
//             }


//             activeCoins.Add(
//                 coin
//             );


//             StartCoroutine(
//                 BurstCoin(
//                     coin
//                 )
//             );


//             yield return new WaitForSeconds(
//                 spawnDelay
//             );
//         }
//     }



//     private IEnumerator BurstCoin(
//         GameObject coin)
//     {
//         RectTransform rect =
//             coin.GetComponent<RectTransform>();


//         if (rect == null)
//         {
//             yield break;
//         }


//         Vector2 direction =
//             UnityEngine.Random.insideUnitCircle
//             .normalized;


//         Vector2 targetPosition =
//             direction *
//             UnityEngine.Random.Range(
//                 burstDistance * 0.6f,
//                 burstDistance
//             );


//         Vector2 startPosition =
//             Vector2.zero;


//         float elapsed = 0f;


//         while (elapsed < burstDuration)
//         {
//             elapsed +=
//                 Time.unscaledDeltaTime;


//             float progress =
//                 Mathf.Clamp01(
//                     elapsed /
//                     burstDuration
//                 );


//             progress =
//                 Mathf.SmoothStep(
//                     0f,
//                     1f,
//                     progress
//                 );


//             Vector2 position =
//                 Vector2.Lerp(
//                     startPosition,
//                     targetPosition,
//                     progress
//                 );


//             position.y +=
//                 Mathf.Sin(
//                     progress *
//                     Mathf.PI
//                 ) *
//                 bounceHeight;


//             rect.anchoredPosition =
//                 position;


//             rect.Rotate(
//                 0f,
//                 0f,
//                 rotationSpeed *
//                 Time.unscaledDeltaTime
//             );


//             yield return null;
//         }
//     }



//     private IEnumerator CollectCoinsRoutine()
//     {
//         List<GameObject> coins =
//             new List<GameObject>(
//                 activeCoins
//             );


//         foreach (GameObject coin in coins)
//         {
//             if (coin != null)
//             {
//                 StartCoroutine(
//                     MoveCoinToTarget(
//                         coin
//                     )
//                 );


//                 yield return new WaitForSeconds(
//                     spawnDelay
//                 );
//             }
//         }


//         yield return new WaitForSeconds(
//             collectDuration + 0.15f
//         );


//         CoinsCollected?.Invoke();
//     }



//     private IEnumerator MoveCoinToTarget(
//         GameObject coin)
//     {
//         if (coin == null)
//         {
//             yield break;
//         }


//         RectTransform rect =
//             coin.GetComponent<RectTransform>();


//         if (rect == null)
//         {
//             yield break;
//         }


//         Vector3 start =
//             rect.position;


//         float elapsed = 0f;


//         while (elapsed < collectDuration)
//         {
//             if (coin == null)
//             {
//                 yield break;
//             }


//             elapsed +=
//                 Time.unscaledDeltaTime;


//             float progress =
//                 Mathf.Clamp01(
//                     elapsed /
//                     collectDuration
//                 );


//             progress =
//                 Mathf.SmoothStep(
//                     0f,
//                     1f,
//                     progress
//                 );


//             rect.position =
//                 Vector3.Lerp(
//                     start,
//                     coinTarget.position,
//                     progress
//                 );


//             rect.Rotate(
//                 0f,
//                 0f,
//                 rotationSpeed *
//                 Time.unscaledDeltaTime
//             );


//             yield return null;
//         }


//         if (destroyAfterCollect &&
//             coin != null)
//         {
//             Destroy(
//                 coin
//             );
//         }
//     }



//     private void ClearCoins()
//     {
//         foreach (GameObject coin in activeCoins)
//         {
//             if (coin != null)
//             {
//                 Destroy(
//                     coin
//                 );
//             }
//         }


//         activeCoins.Clear();
//     }
// }




// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;


// public enum RewardVisualType
// {
//     Coin,
//     Life,
//     PowerUp,
//     Gem,
//     Star,
//     Custom
// }



// [Serializable]
// public sealed class RewardVisualDefinition
// {
//     public RewardVisualType rewardType;

//     [Header("Visual")]
//     public GameObject rewardPrefab;

//     [Header("Destination")]
//     public RectTransform target;


//     [Header("Amount")]
//     [Min(1)]
//     public int visualCount = 10;
// }




// public sealed class CoinRewardAnimator : MonoBehaviour
// {
//     [Header("Reward Visual Data")]
//     [SerializeField]
//     private List<RewardVisualDefinition> rewards =
//         new List<RewardVisualDefinition>();



//     [Header("Spawn")]
//     [SerializeField]
//     private RectTransform spawnParent;


//     [SerializeField]
//     private float spawnDelay = 0.03f;



//     [Header("Burst")]
//     [SerializeField]
//     private float burstDistance = 180f;


//     [SerializeField]
//     private float burstDuration = 0.35f;



//     [Header("Collect")]
//     [SerializeField]
//     private float visibleDelayBeforeCollect = 0.25f;


//     [SerializeField]
//     private float collectDuration = 0.7f;



//     [Header("Motion")]
//     [SerializeField]
//     private float rotationSpeed = 720f;


//     [SerializeField]
//     private float bounceHeight = 35f;



//     [Header("Behaviour")]
//     [SerializeField]
//     private bool destroyAfterCollect = true;



//     public event Action<RewardVisualType> RewardCollected;



//     private readonly List<GameObject> activeRewards =
//         new List<GameObject>();


//     private Coroutine currentRoutine;



//     // -----------------------------
//     // PUBLIC BUTTON FUNCTIONS
//     // -----------------------------


//     public void PlayCoinReward()
//     {
//         PlayReward(
//             RewardVisualType.Coin
//         );
//     }



//     public void PlayLifeReward()
//     {
//         PlayReward(
//             RewardVisualType.Life
//         );
//     }



//     public void PlayPowerUpReward()
//     {
//         PlayReward(
//             RewardVisualType.PowerUp
//         );
//     }



//     public void PlayReward(
//         RewardVisualType type)
//     {
//         StopCurrentAnimation();


//         RewardVisualDefinition definition =
//             GetDefinition(type);


//         if (definition == null)
//         {
//             Debug.LogWarning(
//                 "Reward setup missing: " + type
//             );

//             return;
//         }


//         currentRoutine =
//             StartCoroutine(
//                 RewardRoutine(
//                     definition
//                 )
//             );
//     }




//     private IEnumerator RewardRoutine(
//         RewardVisualDefinition definition)
//     {
//         yield return StartCoroutine(
//             SpawnRewards(
//                 definition
//             )
//         );


//         yield return new WaitForSeconds(
//             visibleDelayBeforeCollect
//         );


//         yield return StartCoroutine(
//             CollectRewards(
//                 definition
//             )
//         );


//         RewardCollected?.Invoke(
//             definition.rewardType
//         );
//     }




//     private IEnumerator SpawnRewards(
//         RewardVisualDefinition definition)
//     {
//         for(int i = 0;
//             i < definition.visualCount;
//             i++)
//         {
//             if(definition.rewardPrefab == null ||
//                spawnParent == null)
//             {
//                 yield break;
//             }


//             GameObject reward =
//                 Instantiate(
//                     definition.rewardPrefab,
//                     spawnParent
//                 );


//             reward.transform.SetAsLastSibling();


//             RectTransform rect =
//                 reward.GetComponent<RectTransform>();


//             if(rect != null)
//             {
//                 rect.localScale =
//                     Vector3.one;


//                 rect.anchorMin =
//                     new Vector2(
//                         0.5f,
//                         0.5f
//                     );

//                 rect.anchorMax =
//                     new Vector2(
//                         0.5f,
//                         0.5f
//                     );

//                 rect.pivot =
//                     new Vector2(
//                         0.5f,
//                         0.5f
//                     );


//                 rect.anchoredPosition =
//                     Vector2.zero;
//             }


//             activeRewards.Add(
//                 reward
//             );


//             StartCoroutine(
//                 BurstReward(
//                     reward
//                 )
//             );


//             yield return new WaitForSeconds(
//                 spawnDelay
//             );
//         }
//     }




//     private IEnumerator BurstReward(
//         GameObject reward)
//     {
//         if(reward == null)
//             yield break;


//         RectTransform rect =
//             reward.GetComponent<RectTransform>();


//         if(rect == null)
//             yield break;



//         Vector2 direction =
//             UnityEngine.Random
//             .insideUnitCircle
//             .normalized;



//         Vector2 targetPosition =
//             direction *
//             UnityEngine.Random.Range(
//                 burstDistance * 0.6f,
//                 burstDistance
//             );



//         float timer = 0f;



//         while(timer < burstDuration)
//         {
//             if(rect == null)
//                 yield break;


//             timer +=
//                 Time.unscaledDeltaTime;



//             float progress =
//                 Mathf.Clamp01(
//                     timer /
//                     burstDuration
//                 );



//             progress =
//                 Mathf.SmoothStep(
//                     0f,
//                     1f,
//                     progress
//                 );



//             Vector2 pos =
//                 Vector2.Lerp(
//                     Vector2.zero,
//                     targetPosition,
//                     progress
//                 );



//             pos.y +=
//                 Mathf.Sin(
//                     progress *
//                     Mathf.PI
//                 )
//                 *
//                 bounceHeight;



//             rect.anchoredPosition =
//                 pos;



//             rect.Rotate(
//                 0f,
//                 0f,
//                 rotationSpeed *
//                 Time.unscaledDeltaTime
//             );


//             yield return null;
//         }
//     }




//     private IEnumerator CollectRewards(
//         RewardVisualDefinition definition)
//     {
//         List<GameObject> copy =
//             new List<GameObject>(
//                 activeRewards
//             );



//         foreach(GameObject reward in copy)
//         {
//             if(reward != null)
//             {
//                 StartCoroutine(
//                     MoveReward(
//                         reward,
//                         definition.target
//                     )
//                 );


//                 yield return new WaitForSeconds(
//                     spawnDelay
//                 );
//             }
//         }



//         yield return new WaitForSeconds(
//             collectDuration + 0.15f
//         );
//     }




//     private IEnumerator MoveReward(
//         GameObject reward,
//         RectTransform target)
//     {
//         if(reward == null ||
//            target == null)
//         {
//             yield break;
//         }



//         RectTransform rect =
//             reward.GetComponent<RectTransform>();


//         if(rect == null)
//             yield break;



//         Vector3 start =
//             rect.position;



//         float timer = 0f;



//         while(timer < collectDuration)
//         {
//             if(rect == null ||
//                target == null)
//             {
//                 yield break;
//             }



//             timer +=
//                 Time.unscaledDeltaTime;



//             float progress =
//                 Mathf.Clamp01(
//                     timer /
//                     collectDuration
//                 );



//             progress =
//                 Mathf.SmoothStep(
//                     0f,
//                     1f,
//                     progress
//                 );



//             rect.position =
//                 Vector3.Lerp(
//                     start,
//                     target.position,
//                     progress
//                 );



//             rect.Rotate(
//                 0f,
//                 0f,
//                 rotationSpeed *
//                 Time.unscaledDeltaTime
//             );


//             yield return null;
//         }



//         if(destroyAfterCollect &&
//            reward != null)
//         {
//             activeRewards.Remove(
//                 reward
//             );


//             Destroy(
//                 reward
//             );
//         }
//     }




//     private RewardVisualDefinition GetDefinition(
//         RewardVisualType type)
//     {
//         foreach(RewardVisualDefinition item in rewards)
//         {
//             if(item.rewardType == type)
//                 return item;
//         }


//         return null;
//     }





//     private void StopCurrentAnimation()
//     {
//         if(currentRoutine != null)
//         {
//             StopCoroutine(
//                 currentRoutine
//             );

//             currentRoutine = null;
//         }


//         ClearRewards();
//     }




//     private void ClearRewards()
//     {
//         for(int i = activeRewards.Count - 1;
//             i >= 0;
//             i--)
//         {
//             GameObject reward =
//                 activeRewards[i];


//             if(reward != null)
//             {
//                 Destroy(
//                     reward
//                 );
//             }
//         }


//         activeRewards.Clear();
//     }
// }







using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum RewardVisualType
{
    Coin,
    Life,
    CanonBall,
    Rocket,
    PowerUp,
    Gem,
    Star,
    Custom
}


[Serializable]
public sealed class RewardVisualDefinition
{
    public RewardVisualType rewardType;

    [Header("Visual")]
    public GameObject rewardPrefab;

    [Header("Destination")]
    public RectTransform target;

    [Header("Amount")]
    [Min(1)]
    public int visualCount = 1;

    [Header("Reward Text")]
    public bool showAmountText = true;
    public GameObject amountTextPrefab;

    [Header("Text Only Reward")]
    public bool textOnlyReward = false;
}

public sealed class CoinRewardAnimator : MonoBehaviour
{
    [Header("Reward Visual Data")]
    [SerializeField]
    private List<RewardVisualDefinition> rewards = new List<RewardVisualDefinition>();

    [Header("Spawn")]
    [SerializeField] private RectTransform spawnParent;
    [SerializeField] private float spawnDelay = 0.03f;

    [Header("Burst")]
    [SerializeField] private float burstDistance = 180f;
    [SerializeField] private float burstDuration = 0.35f;

    [Header("Early Collect (NEW)")]
    [Tooltip("Jitni items spawn hone ke baad collection shuru ho jaye (baaki spawn hote rehte hain saath saath)")]
    [SerializeField] private int collectStartThreshold = 4;

    [Tooltip("Collector queue se agla item nikalne ka gap")]
    [SerializeField] private float collectStagger = 0.05f;

    [Header("Collect")]
    [SerializeField] private float collectDuration = 0.7f;

    [Header("Motion")]
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float bounceHeight = 35f;

    [Header("Text Animation")]
    [SerializeField] private float textMoveHeight = 80f;
    [SerializeField] private float textFadeDuration = 0.8f;

    [Header("Behaviour")]
    [SerializeField] private bool destroyAfterCollect = true;

    public event Action<RewardVisualType> RewardCollected;

    private readonly List<GameObject> activeRewards = new List<GameObject>();
    private readonly Queue<GameObject> pendingCollectQueue = new Queue<GameObject>();
    private readonly Dictionary<GameObject, Coroutine> burstCoroutines = new Dictionary<GameObject, Coroutine>();

    private Coroutine currentRoutine;
    private Coroutine collectorRoutine;

    private bool spawningDone;
    private int spawnedCount;
    private int collectedCount;
    private int totalToCollect;

    // ==========================
    // BUTTON FUNCTIONS
    // ==========================

    public void PlayCoinReward() => PlayReward(RewardVisualType.Coin);
    public void PlayLifeReward() => PlayReward(RewardVisualType.Life);
    public void PlayCanonBallReward() => PlayReward(RewardVisualType.CanonBall);
    public void PlayRocketReward() => PlayReward(RewardVisualType.Rocket);
    public void PlayPowerUpReward() => PlayReward(RewardVisualType.PowerUp);

    public void PlayReward(RewardVisualType type)
    {
        StopCurrentAnimation();

        RewardVisualDefinition definition = GetDefinition(type);

        if (definition == null)
        {
            Debug.LogWarning("Reward setup missing: " + type);
            return;
        }

        currentRoutine = StartCoroutine(RewardRoutine(definition));
    }

    // ==========================
    // MAIN ROUTINE (spawn + collect run in parallel now)
    // ==========================

    private IEnumerator RewardRoutine(RewardVisualDefinition definition)
    {
        // Text-only rewards (Canon Ball / Rocket) untouched
        if (definition.textOnlyReward)
        {
            yield return StartCoroutine(SpawnTextOnly(definition));
            RewardCollected?.Invoke(definition.rewardType);
            yield break;
        }

        spawningDone = false;
        spawnedCount = 0;
        collectedCount = 0;
        totalToCollect = definition.visualCount;
        pendingCollectQueue.Clear();

        // Collector runs alongside spawner from the start.
        // It just waits internally until threshold is met.
        collectorRoutine = StartCoroutine(CollectorLoop(definition));

        yield return StartCoroutine(SpawnRewards(definition));

        spawningDone = true;

        // Wait for collector to finish draining the queue
        yield return collectorRoutine;

        RewardCollected?.Invoke(definition.rewardType);
    }

    private IEnumerator SpawnTextOnly(RewardVisualDefinition definition)
    {
        if (definition.showAmountText && definition.amountTextPrefab != null)
        {
            GameObject text = Instantiate(definition.amountTextPrefab, spawnParent);
            StartCoroutine(FadeAmountText(text));
        }
        yield break;
    }

    private IEnumerator SpawnRewards(RewardVisualDefinition definition)
    {
        if (spawnParent == null) yield break;

        for (int i = 0; i < definition.visualCount; i++)
        {
            if (definition.rewardPrefab == null) yield break;

            GameObject reward = Instantiate(definition.rewardPrefab, spawnParent);
            reward.transform.SetAsLastSibling();

            RectTransform rect = reward.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }

            activeRewards.Add(reward);

            if (definition.showAmountText && definition.amountTextPrefab != null)
            {
                GameObject text = Instantiate(definition.amountTextPrefab, spawnParent);
                text.transform.position = reward.transform.position;
                StartCoroutine(FadeAmountText(text));
            }

            // track burst coroutine so we can cancel it early if collector grabs this reward mid-burst
            Coroutine burst = StartCoroutine(BurstReward(reward));
            burstCoroutines[reward] = burst;

            // reward is now available for the collector
            pendingCollectQueue.Enqueue(reward);
            spawnedCount++;

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    // Watches the queue; once threshold reached (or spawning finished),
    // starts sending items to the destination while spawning may still continue.
    private IEnumerator CollectorLoop(RewardVisualDefinition definition)
    {
        bool started = false;

        while (collectedCount < totalToCollect)
        {
            if (!started)
            {
                // wait until we hit the threshold, or spawning already finished (fewer items than threshold)
                if (pendingCollectQueue.Count >= collectStartThreshold || spawningDone)
                {
                    started = true;
                }
                else
                {
                    yield return null;
                    continue;
                }
            }

            if (pendingCollectQueue.Count > 0)
            {
                GameObject reward = pendingCollectQueue.Dequeue();

                if (reward != null)
                {
                    // stop the burst animation on this reward before sending it off
                    if (burstCoroutines.TryGetValue(reward, out Coroutine burstCo))
                    {
                        if (burstCo != null) StopCoroutine(burstCo);
                        burstCoroutines.Remove(reward);
                    }

                    StartCoroutine(MoveReward(reward, definition.target));
                }

                collectedCount++;
                yield return new WaitForSeconds(collectStagger);
            }
            else
            {
                // queue temporarily empty but more items still being spawned - wait
                if (spawningDone) yield break; // safety, shouldn't hit if counts are right
                yield return null;
            }
        }
    }

    private IEnumerator BurstReward(GameObject reward)
    {
        if (reward == null) yield break;

        RectTransform rect = reward.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
        Vector2 targetPosition = direction * UnityEngine.Random.Range(burstDistance * 0.6f, burstDistance);

        float timer = 0f;

        while (timer < burstDuration)
        {
            if (rect == null) yield break;

            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / burstDuration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            Vector2 pos = Vector2.Lerp(Vector2.zero, targetPosition, progress);
            pos.y += Mathf.Sin(progress * Mathf.PI) * bounceHeight;

            rect.anchoredPosition = pos;
            rect.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);

            yield return null;
        }
    }

    private IEnumerator MoveReward(GameObject reward, RectTransform target)
    {
        if (reward == null || target == null) yield break;

        RectTransform rect = reward.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector3 start = rect.position;
        float timer = 0f;

        while (timer < collectDuration)
        {
            if (rect == null || target == null) yield break;

            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / collectDuration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            rect.position = Vector3.Lerp(start, target.position, progress);
            rect.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        if (destroyAfterCollect && reward != null)
        {
            activeRewards.Remove(reward);
            Destroy(reward);
        }
    }

    private IEnumerator FadeAmountText(GameObject textObject)
    {
        if (textObject == null) yield break;

        CanvasGroup group = textObject.GetComponent<CanvasGroup>();
        if (group == null) group = textObject.AddComponent<CanvasGroup>();

        group.alpha = 1f;

        Vector3 start = textObject.transform.position;
        Vector3 end = start + Vector3.up * textMoveHeight;

        float timer = 0f;

        while (timer < textFadeDuration)
        {
            if (textObject == null) yield break;

            timer += Time.deltaTime;
            float progress = timer / textFadeDuration;

            textObject.transform.position = Vector3.Lerp(start, end, progress);
            group.alpha = Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        Destroy(textObject);
    }

    private RewardVisualDefinition GetDefinition(RewardVisualType type)
    {
        foreach (RewardVisualDefinition item in rewards)
        {
            if (item.rewardType == type) return item;
        }
        return null;
    }

    private void StopCurrentAnimation()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        if (collectorRoutine != null)
        {
            StopCoroutine(collectorRoutine);
            collectorRoutine = null;
        }

        pendingCollectQueue.Clear();
        burstCoroutines.Clear();

        ClearRewards();
    }

    private void ClearRewards()
    {
        for (int i = activeRewards.Count - 1; i >= 0; i--)
        {
            GameObject reward = activeRewards[i];
            if (reward != null) Destroy(reward);
        }
        activeRewards.Clear();
    }
}



