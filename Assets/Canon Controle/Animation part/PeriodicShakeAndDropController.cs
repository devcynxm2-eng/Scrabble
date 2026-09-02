// using System.Collections;
// using DG.Tweening;
// using UnityEngine;

// /// <summary>
// /// Target image ko periodically shake karta hai aur us specified
// /// spawn point se random coins/items neeche drop karta hai.
// /// </summary>
// public sealed class PeriodicShakeAndDropController : MonoBehaviour
// {
//     [Header("Target")]
//     [SerializeField]
//     private RectTransform imageToShake;


//     [Header("Item Spawn Point")]
//     [Tooltip(
//         "Coins/items isi exact UI position se spawn hongi. " +
//         "Inspector mein manually ek RectTransform assign karein."
//     )]
//     [SerializeField]
//     private RectTransform itemSpawnPoint;


//     [Tooltip(
//         "Agar Item Spawn Point assign nahi hai to imageToShake ki position use hogi."
//     )]
//     [SerializeField]
//     private bool fallbackToTargetPosition = true;


//     [Header("Timing")]
//     [SerializeField, Min(0.1f)]
//     private float minIntervalSeconds = 3f;

//     [SerializeField, Min(0.1f)]
//     private float maxIntervalSeconds = 6f;

//     [SerializeField]
//     private bool autoStart = true;


//     [Header("Shake Settings")]
//     [SerializeField, Min(0.01f)]
//     private float shakeDuration = 0.4f;

//     [SerializeField]
//     private Vector3 shakeStrength =
//         new Vector3(18f, 18f, 0f);

//     [SerializeField, Min(1)]
//     private int shakeVibrato = 12;

//     [SerializeField, Range(0f, 90f)]
//     private float shakeRandomness = 90f;

//     [SerializeField]
//     private bool shakeFadeOut = true;


//     [Header("Falling Items")]
//     [Tooltip(
//         "Random coin/gem/item prefabs."
//     )]
//     [SerializeField]
//     private RectTransform[] itemPrefabs;

//     [Tooltip(
//         "Items ka parent. Khali ho to imageToShake ka parent use hoga."
//     )]
//     [SerializeField]
//     private RectTransform spawnParent;

//     [SerializeField, Min(0)]
//     private int minItemCount = 3;

//     [SerializeField, Min(0)]
//     private int maxItemCount = 6;


//     [Header("Spawn Variation")]
//     [Tooltip(
//         "Spawn point ke around horizontal random spread."
//     )]
//     [SerializeField, Min(0f)]
//     private float spawnSpreadX = 40f;


//     [Header("Fall")]
//     [SerializeField, Min(1f)]
//     private float fallDistance = 260f;

//     [SerializeField, Min(0f)]
//     private float fallHorizontalDrift = 60f;

//     [SerializeField, Min(0.05f)]
//     private float fallDurationMin = 0.5f;

//     [SerializeField, Min(0.05f)]
//     private float fallDurationMax = 0.9f;

//     [SerializeField]
//     private Ease fallEase = Ease.InQuad;


//     [Header("Rotation")]
//     [SerializeField]
//     private bool rotateWhileFalling = true;

//     [SerializeField]
//     private Vector2 rotationRange =
//         new Vector2(-220f, 220f);


//     [Header("Pop In")]
//     [SerializeField, Min(0.01f)]
//     private float popInDuration = 0.15f;


//     [Header("Fade")]
//     [SerializeField]
//     private bool fadeOutAtEnd = true;

//     [SerializeField, Range(0f, 1f)]
//     private float fadeOutStartProgress = 0.7f;


//     private Coroutine loopRoutine;


//     private void Awake()
//     {
//         if (spawnParent == null &&
//             imageToShake != null)
//         {
//             spawnParent =
//                 imageToShake.parent as RectTransform;
//         }

//         ValidateSpawnPoint();
//     }


//     private void OnEnable()
//     {
//         if (autoStart)
//         {
//             StartLoop();
//         }
//     }


//     private void OnDisable()
//     {
//         StopLoop();

//         if (imageToShake != null)
//         {
//             imageToShake.DOKill();
//         }
//     }


//     public void StartLoop()
//     {
//         StopLoop();

//         loopRoutine =
//             StartCoroutine(
//                 ShakeAndDropLoopRoutine()
//             );
//     }


//     public void StopLoop()
//     {
//         if (loopRoutine != null)
//         {
//             StopCoroutine(loopRoutine);
//             loopRoutine = null;
//         }
//     }


//     public void TriggerOnce()
//     {
//         StartCoroutine(
//             ShakeAndDropOnceRoutine()
//         );
//     }


//     private IEnumerator ShakeAndDropLoopRoutine()
//     {
//         while (true)
//         {
//             float wait =
//                 Random.Range(
//                     minIntervalSeconds,
//                     maxIntervalSeconds
//                 );

//             yield return new WaitForSeconds(wait);

//             yield return ShakeAndDropOnceRoutine();
//         }
//     }


//     private IEnumerator ShakeAndDropOnceRoutine()
//     {
//         if (imageToShake == null)
//         {
//             yield break;
//         }

//         Tween shakeTween =
//             imageToShake.DOShakeAnchorPos(
//                 shakeDuration,
//                 shakeStrength,
//                 shakeVibrato,
//                 shakeRandomness,
//                 false,
//                 shakeFadeOut
//             );


//         yield return new WaitForSeconds(
//             shakeDuration * 0.5f
//         );


//         SpawnFallingItems();


//         yield return shakeTween.WaitForCompletion();
//     }


//     private void SpawnFallingItems()
//     {
//         if (itemPrefabs == null ||
//             itemPrefabs.Length == 0)
//         {
//             return;
//         }

//         int count =
//             Random.Range(
//                 minItemCount,
//                 maxItemCount + 1
//             );


//         for (int i = 0; i < count; i++)
//         {
//             SpawnSingleItem();
//         }
//     }


//     private void SpawnSingleItem()
//     {
//         RectTransform prefab =
//             itemPrefabs[
//                 Random.Range(
//                     0,
//                     itemPrefabs.Length
//                 )
//             ];


//         if (prefab == null)
//         {
//             return;
//         }


//         RectTransform parent =
//             spawnParent != null
//                 ? spawnParent
//                 : imageToShake.parent as RectTransform;


//         if (parent == null)
//         {
//             return;
//         }


//         RectTransform item =
//             Instantiate(
//                 prefab,
//                 parent
//             );


//         item.gameObject.SetActive(true);


//         /*
//          * IMPORTANT:
//          *
//          * Ab item imageToShake se nahi balkay
//          * manually assigned itemSpawnPoint se spawn hogi.
//          */
//         Vector2 spawnPosition =
//             GetSpawnPosition();


//         item.anchoredPosition =
//             spawnPosition;


//         Vector3 targetScale =
//             item.localScale;


//         item.localScale =
//             Vector3.zero;


//         CanvasGroup canvasGroup =
//             item.GetComponent<CanvasGroup>();


//         if (canvasGroup == null)
//         {
//             canvasGroup =
//                 item.gameObject.AddComponent<CanvasGroup>();
//         }


//         canvasGroup.alpha = 1f;


//         /*
//          * Pop in
//          */
//         item.DOScale(
//             targetScale,
//             popInDuration
//         )
//         .SetEase(
//             Ease.OutBack
//         );


//         float fallDuration =
//             Random.Range(
//                 fallDurationMin,
//                 fallDurationMax
//             );


//         float horizontalDrift =
//             Random.Range(
//                 -fallHorizontalDrift,
//                 fallHorizontalDrift
//             );


//         Vector2 fallTarget =
//             spawnPosition +
//             new Vector2(
//                 horizontalDrift,
//                 -fallDistance
//             );


//         Sequence fallSequence =
//             DOTween.Sequence();


//         fallSequence.Append(
//             item.DOAnchorPos(
//                 fallTarget,
//                 fallDuration
//             )
//             .SetEase(
//                 fallEase
//             )
//         );


//         if (rotateWhileFalling)
//         {
//             float randomRotation =
//                 Random.Range(
//                     rotationRange.x,
//                     rotationRange.y
//                 );


//             item.DORotate(
//                 new Vector3(
//                     0f,
//                     0f,
//                     randomRotation
//                 ),
//                 fallDuration,
//                 RotateMode.FastBeyond360
//             );
//         }


//         if (fadeOutAtEnd)
//         {
//             float fadeDelay =
//                 fallDuration *
//                 fadeOutStartProgress;


//             float fadeDuration =
//                 fallDuration *
//                 (1f - fadeOutStartProgress);


//             canvasGroup
//                 .DOFade(
//                     0f,
//                     Mathf.Max(
//                         0.05f,
//                         fadeDuration
//                     )
//                 )
//                 .SetDelay(
//                     fadeDelay
//                 );
//         }


//         fallSequence.OnComplete(
//             () =>
//             {
//                 if (item != null)
//                 {
//                     Destroy(
//                         item.gameObject
//                     );
//                 }
//             }
//         );
//     }


//     private Vector2 GetSpawnPosition()
//     {
//         /*
//          * Manual spawn point assigned hai
//          * to uski exact position use karo.
//          */
//         if (itemSpawnPoint != null)
//         {
//             Vector2 position =
//                 GetAnchoredPositionRelativeToParent(
//                     itemSpawnPoint,
//                     spawnParent
//                 );


//             position.x +=
//                 Random.Range(
//                     -spawnSpreadX,
//                     spawnSpreadX
//                 );


//             return position;
//         }


//         /*
//          * Optional fallback.
//          */
//         if (fallbackToTargetPosition &&
//             imageToShake != null)
//         {
//             Vector2 position =
//                 GetAnchoredPositionRelativeToParent(
//                     imageToShake,
//                     spawnParent
//                 );


//             position.x +=
//                 Random.Range(
//                     -spawnSpreadX,
//                     spawnSpreadX
//                 );


//             return position;
//         }


//         return Vector2.zero;
//     }


//     private static Vector2 GetAnchoredPositionRelativeToParent(
//         RectTransform target,
//         RectTransform targetParent)
//     {
//         if (target == null ||
//             targetParent == null)
//         {
//             return Vector2.zero;
//         }


//         Vector3 worldPosition =
//             target.position;


//         Vector3 localPosition =
//             targetParent.InverseTransformPoint(
//                 worldPosition
//             );


//         return new Vector2(
//             localPosition.x,
//             localPosition.y
//         );
//     }


//     private void ValidateSpawnPoint()
//     {
//         if (itemSpawnPoint == null)
//         {
//             Debug.LogWarning(
//                 "PeriodicShakeAndDropController: " +
//                 "Item Spawn Point assign nahi hai. " +
//                 "Fallback enabled ho to target image ki position use hogi.",
//                 this
//             );
//         }
//     }
// }













// using System.Collections;
// using DG.Tweening;
// using UnityEngine;

// /// <summary>
// /// Target image ko periodically shake karta hai aur specified
// /// spawn point se random coins/items neeche drop karta hai.
// ///
// /// Spawned items:
// /// - Manually assigned spawn point se appear hotay hain.
// /// - Inspector se starting size control hota hai.
// /// - Inspector se final size control hota hai.
// /// - Optional random final size available hai.
// /// - Start size se final size tak smooth scale animation hoti hai.
// /// - Fall ke waqt rotation aur fade optional hai.
// /// </summary>
// public sealed class PeriodicShakeAndDropController : MonoBehaviour
// {
//     [Header("Target")]
//     [SerializeField]
//     private RectTransform imageToShake;


//     [Header("Item Spawn Point")]
//     [Tooltip(
//         "Coins/items isi exact UI position se spawn hongi. " +
//         "Inspector mein manually ek RectTransform assign karein."
//     )]
//     [SerializeField]
//     private RectTransform itemSpawnPoint;


//     [Tooltip(
//         "Agar Item Spawn Point assign nahi hai to imageToShake ki position use hogi."
//     )]
//     [SerializeField]
//     private bool fallbackToTargetPosition = true;


//     [Header("Timing")]
//     [SerializeField, Min(0.1f)]
//     private float minIntervalSeconds = 3f;

//     [SerializeField, Min(0.1f)]
//     private float maxIntervalSeconds = 6f;

//     [SerializeField]
//     private bool autoStart = true;


//     [Header("Shake Settings")]
//     [SerializeField, Min(0.01f)]
//     private float shakeDuration = 0.4f;

//     [SerializeField]
//     private Vector3 shakeStrength =
//         new Vector3(18f, 18f, 0f);

//     [SerializeField, Min(1)]
//     private int shakeVibrato = 12;

//     [SerializeField, Range(0f, 90f)]
//     private float shakeRandomness = 90f;

//     [SerializeField]
//     private bool shakeFadeOut = true;


//     [Header("Falling Items")]
//     [Tooltip(
//         "Random coin/gem/item prefabs."
//     )]
//     [SerializeField]
//     private RectTransform[] itemPrefabs;

//     [Tooltip(
//         "Items ka parent. Khali ho to imageToShake ka parent use hoga."
//     )]
//     [SerializeField]
//     private RectTransform spawnParent;

//     [SerializeField, Min(0)]
//     private int minItemCount = 3;

//     [SerializeField, Min(0)]
//     private int maxItemCount = 6;


//     [Header("Spawn Variation")]
//     [Tooltip(
//         "Spawn point ke around horizontal random spread."
//     )]
//     [SerializeField, Min(0f)]
//     private float spawnSpreadX = 40f;


//     [Header("Item Scale")]
//     [Tooltip(
//         "Item spawn hote waqt prefab ke original scale ka kitna " +
//         "percentage hoga. 0.3 = 30%, 1 = 100%."
//     )]
//     [SerializeField, Min(0.01f)]
//     private float spawnStartScale = 0.3f;


//     [Tooltip(
//         "Item ka final scale prefab ke original scale ke comparison " +
//         "mein. 1 = original prefab size, 2 = double size."
//     )]
//     [SerializeField, Min(0.01f)]
//     private float finalScaleMultiplier = 1f;


//     [Tooltip(
//         "Spawn start size se final size tak kitne seconds mein scale hoga."
//     )]
//     [SerializeField, Min(0f)]
//     private float scaleUpDuration = 0.25f;


//     [Tooltip(
//         "Scale animation ki easing."
//     )]
//     [SerializeField]
//     private Ease scaleEase = Ease.OutBack;


//     [Tooltip(
//         "ON: har spawned item ka final size random hoga."
//     )]
//     [SerializeField]
//     private bool useRandomFinalScale = false;


//     [Tooltip(
//         "Random final scale ka minimum aur maximum multiplier."
//     )]
//     [SerializeField]
//     private Vector2 randomFinalScaleRange =
//         new Vector2(0.85f, 1.15f);


//     [Header("Fall")]
//     [SerializeField, Min(1f)]
//     private float fallDistance = 260f;

//     [SerializeField, Min(0f)]
//     private float fallHorizontalDrift = 60f;

//     [SerializeField, Min(0.05f)]
//     private float fallDurationMin = 0.5f;

//     [SerializeField, Min(0.05f)]
//     private float fallDurationMax = 0.9f;

//     [SerializeField]
//     private Ease fallEase = Ease.InQuad;


//     [Header("Rotation")]
//     [SerializeField]
//     private bool rotateWhileFalling = true;

//     [SerializeField]
//     private Vector2 rotationRange =
//         new Vector2(-220f, 220f);


//     [Header("Fade")]
//     [SerializeField]
//     private bool fadeOutAtEnd = true;

//     [SerializeField, Range(0f, 1f)]
//     private float fadeOutStartProgress = 0.7f;


//     private Coroutine loopRoutine;


//     private void Awake()
//     {
//         if (spawnParent == null &&
//             imageToShake != null)
//         {
//             spawnParent =
//                 imageToShake.parent as RectTransform;
//         }

//         ValidateSpawnPoint();
//         ValidateSettings();
//     }


//     private void OnEnable()
//     {
//         if (autoStart)
//         {
//             StartLoop();
//         }
//     }


//     private void OnDisable()
//     {
//         StopLoop();

//         if (imageToShake != null)
//         {
//             imageToShake.DOKill();
//         }
//     }


//     public void StartLoop()
//     {
//         StopLoop();

//         loopRoutine =
//             StartCoroutine(
//                 ShakeAndDropLoopRoutine()
//             );
//     }


//     public void StopLoop()
//     {
//         if (loopRoutine != null)
//         {
//             StopCoroutine(loopRoutine);
//             loopRoutine = null;
//         }
//     }


//     /// <summary>
//     /// Manually ek baar shake + drop trigger karta hai.
//     /// Button ya UnityEvent se bhi call kar sakte hain.
//     /// </summary>
//     public void TriggerOnce()
//     {
//         StartCoroutine(
//             ShakeAndDropOnceRoutine()
//         );
//     }


//     private IEnumerator ShakeAndDropLoopRoutine()
//     {
//         while (true)
//         {
//             float wait =
//                 Random.Range(
//                     minIntervalSeconds,
//                     maxIntervalSeconds
//                 );

//             yield return new WaitForSeconds(wait);

//             yield return ShakeAndDropOnceRoutine();
//         }
//     }


//     private IEnumerator ShakeAndDropOnceRoutine()
//     {
//         if (imageToShake == null)
//         {
//             yield break;
//         }


//         Tween shakeTween =
//             imageToShake.DOShakeAnchorPos(
//                 shakeDuration,
//                 shakeStrength,
//                 shakeVibrato,
//                 shakeRandomness,
//                 false,
//                 shakeFadeOut
//             );


//         /*
//          * Shake ke aadhe raste mein items spawn hongi.
//          */
//         yield return new WaitForSeconds(
//             shakeDuration * 0.5f
//         );


//         SpawnFallingItems();


//         yield return shakeTween.WaitForCompletion();
//     }


//     private void SpawnFallingItems()
//     {
//         if (itemPrefabs == null ||
//             itemPrefabs.Length == 0)
//         {
//             return;
//         }


//         int count =
//             Random.Range(
//                 minItemCount,
//                 maxItemCount + 1
//             );


//         for (int i = 0; i < count; i++)
//         {
//             SpawnSingleItem();
//         }
//     }


//     private void SpawnSingleItem()
//     {
//         RectTransform prefab =
//             itemPrefabs[
//                 Random.Range(
//                     0,
//                     itemPrefabs.Length
//                 )
//             ];


//         if (prefab == null)
//         {
//             return;
//         }


//         RectTransform parent =
//             spawnParent != null
//                 ? spawnParent
//                 : imageToShake.parent as RectTransform;


//         if (parent == null)
//         {
//             return;
//         }


//         RectTransform item =
//             Instantiate(
//                 prefab,
//                 parent
//             );


//         item.gameObject.SetActive(true);


//         /*
//          * Exact manually assigned spawn point.
//          */
//         Vector2 spawnPosition =
//             GetSpawnPosition();


//         item.anchoredPosition =
//             spawnPosition;


//         /*
//          * ------------------------------------------------------------
//          * SCALE SETUP
//          * ------------------------------------------------------------
//          *
//          * Prefab ka original localScale preserve kiya ja raha hai.
//          *
//          * Example:
//          *
//          * Prefab scale = 2
//          * Spawn Start Scale = 0.3
//          *
//          * Actual spawn scale = 2 * 0.3 = 0.6
//          *
//          * Final Scale Multiplier = 1
//          *
//          * Actual final scale = 2 * 1 = 2
//          * ------------------------------------------------------------
//          */

//         Vector3 prefabScale =
//             item.localScale;


//         float finalMultiplier =
//             finalScaleMultiplier;


//         /*
//          * Optional random final size.
//          */
//         if (useRandomFinalScale)
//         {
//             float minimum =
//                 Mathf.Max(
//                     0.01f,
//                     randomFinalScaleRange.x
//                 );


//             float maximum =
//                 Mathf.Max(
//                     minimum,
//                     randomFinalScaleRange.y
//                 );


//             finalMultiplier =
//                 Random.Range(
//                     minimum,
//                     maximum
//                 );
//         }


//         Vector3 startScale =
//             prefabScale *
//             spawnStartScale;


//         Vector3 finalScale =
//             prefabScale *
//             finalMultiplier;


//         /*
//          * Start size immediately apply.
//          */
//         item.localScale =
//             startScale;


//         CanvasGroup canvasGroup =
//             item.GetComponent<CanvasGroup>();


//         if (canvasGroup == null)
//         {
//             canvasGroup =
//                 item.gameObject.AddComponent<CanvasGroup>();
//         }


//         canvasGroup.alpha = 1f;


//         /*
//          * ------------------------------------------------------------
//          * SCALE-UP ANIMATION
//          * ------------------------------------------------------------
//          *
//          * 0.3 -> 1.0 for example.
//          */
//         if (scaleUpDuration > 0f)
//         {
//             item.DOScale(
//                 finalScale,
//                 scaleUpDuration
//             )
//             .SetEase(
//                 scaleEase
//             );
//         }
//         else
//         {
//             item.localScale =
//                 finalScale;
//         }


//         /*
//          * Fall duration.
//          */
//         float fallDuration =
//             Random.Range(
//                 fallDurationMin,
//                 fallDurationMax
//             );


//         /*
//          * Horizontal random movement.
//          */
//         float horizontalDrift =
//             Random.Range(
//                 -fallHorizontalDrift,
//                 fallHorizontalDrift
//             );


//         Vector2 fallTarget =
//             spawnPosition +
//             new Vector2(
//                 horizontalDrift,
//                 -fallDistance
//             );


//         Sequence fallSequence =
//             DOTween.Sequence();


//         /*
//          * Fall animation.
//          */
//         fallSequence.Append(
//             item.DOAnchorPos(
//                 fallTarget,
//                 fallDuration
//             )
//             .SetEase(
//                 fallEase
//             )
//         );


//         /*
//          * Rotation.
//          */
//         if (rotateWhileFalling)
//         {
//             float randomRotation =
//                 Random.Range(
//                     rotationRange.x,
//                     rotationRange.y
//                 );


//             item.DORotate(
//                 new Vector3(
//                     0f,
//                     0f,
//                     randomRotation
//                 ),
//                 fallDuration,
//                 RotateMode.FastBeyond360
//             );
//         }


//         /*
//          * Fade out near the end.
//          */
//         if (fadeOutAtEnd)
//         {
//             float safeFadeStart =
//                 Mathf.Clamp01(
//                     fadeOutStartProgress
//                 );


//             float fadeDelay =
//                 fallDuration *
//                 safeFadeStart;


//             float fadeDuration =
//                 fallDuration *
//                 (1f - safeFadeStart);


//             canvasGroup
//                 .DOFade(
//                     0f,
//                     Mathf.Max(
//                         0.05f,
//                         fadeDuration
//                     )
//                 )
//                 .SetDelay(
//                     fadeDelay
//                 );
//         }


//         /*
//          * Destroy after fall.
//          */
//         fallSequence.OnComplete(
//             () =>
//             {
//                 if (item != null)
//                 {
//                     Destroy(
//                         item.gameObject
//                     );
//                 }
//             }
//         );
//     }


//     private Vector2 GetSpawnPosition()
//     {
//         /*
//          * Manual spawn point assigned hai
//          * to uski exact position use karo.
//          */
//         if (itemSpawnPoint != null)
//         {
//             Vector2 position =
//                 GetAnchoredPositionRelativeToParent(
//                     itemSpawnPoint,
//                     spawnParent
//                 );


//             position.x +=
//                 Random.Range(
//                     -spawnSpreadX,
//                     spawnSpreadX
//                 );


//             return position;
//         }


//         /*
//          * Optional fallback.
//          */
//         if (fallbackToTargetPosition &&
//             imageToShake != null)
//         {
//             Vector2 position =
//                 GetAnchoredPositionRelativeToParent(
//                     imageToShake,
//                     spawnParent
//                 );


//             position.x +=
//                 Random.Range(
//                     -spawnSpreadX,
//                     spawnSpreadX
//                 );


//             return position;
//         }


//         return Vector2.zero;
//     }


//     private static Vector2 GetAnchoredPositionRelativeToParent(
//         RectTransform target,
//         RectTransform targetParent)
//     {
//         if (target == null ||
//             targetParent == null)
//         {
//             return Vector2.zero;
//         }


//         Vector3 worldPosition =
//             target.position;


//         Vector3 localPosition =
//             targetParent.InverseTransformPoint(
//                 worldPosition
//             );


//         return new Vector2(
//             localPosition.x,
//             localPosition.y
//         );
//     }


//     private void ValidateSpawnPoint()
//     {
//         if (itemSpawnPoint == null)
//         {
//             Debug.LogWarning(
//                 "PeriodicShakeAndDropController: " +
//                 "Item Spawn Point assign nahi hai. " +
//                 "Fallback enabled ho to target image ki position use hogi.",
//                 this
//             );
//         }
//     }


//     private void ValidateSettings()
//     {
//         if (maxIntervalSeconds < minIntervalSeconds)
//         {
//             Debug.LogWarning(
//                 "PeriodicShakeAndDropController: " +
//                 "Max Interval ko Min Interval se kam rakha gaya hai. " +
//                 "Runtime par Unity Random.Range safe range use karega.",
//                 this
//             );
//         }


//         if (maxItemCount < minItemCount)
//         {
//             Debug.LogWarning(
//                 "PeriodicShakeAndDropController: " +
//                 "Max Item Count ko Min Item Count se kam rakha gaya hai.",
//                 this
//             );
//         }


//         if (fallDurationMax < fallDurationMin)
//         {
//             Debug.LogWarning(
//                 "PeriodicShakeAndDropController: " +
//                 "Fall Duration Max ko Min se kam rakha gaya hai.",
//                 this
//             );
//         }


//         if (randomFinalScaleRange.y <
//             randomFinalScaleRange.x)
//         {
//             Debug.LogWarning(
//                 "PeriodicShakeAndDropController: " +
//                 "Random Final Scale Max ko Min se kam rakha gaya hai.",
//                 this
//             );
//         }
//     }
// }





using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Target image ko periodically shake karta hai aur specified
/// spawn point se random coins/items neeche drop karta hai.
///
/// Daily Reward integration:
/// - Reward unlocked/claimable ho to animation automatically ON.
/// - Reward locked ho to animation OFF.
/// - Reward Screen open hona zaroori nahi.
/// - Reward claim hone ke baad animation automatically stop.
/// - Next reward unlock hone par animation dobara start.
///
/// Item features:
/// - Manual spawn point
/// - Spawn spread
/// - Starting scale
/// - Final scale
/// - Optional random final scale
/// - Scale-up animation
/// - Fall distance
/// - Fall speed
/// - Horizontal drift
/// - Rotation
/// - Fade
/// </summary>
public sealed class PeriodicShakeAndDropController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private RectTransform imageToShake;


    // ================================================================
    // ITEM SPAWN POINT
    // ================================================================

    [Header("Item Spawn Point")]
    [Tooltip(
        "Coins/items isi exact UI position se spawn hongi. " +
        "Inspector mein manually ek RectTransform assign karein."
    )]
    [SerializeField]
    private RectTransform itemSpawnPoint;

    [Tooltip(
        "Agar Item Spawn Point assign nahi hai to imageToShake ki " +
        "position use hogi."
    )]
    [SerializeField]
    private bool fallbackToTargetPosition = true;


    // ================================================================
    // DAILY REWARD
    // ================================================================

    [Header("Daily Reward Activation")]
    [Tooltip(
        "ON karne par shake/drop animation sirf tab chalegi jab " +
        "DailyRewardManager ke mutabiq current reward claimable/unlocked ho."
    )]
    [SerializeField]
    private bool onlyRunWhenRewardUnlocked = true;

    [Tooltip(
        "Reward status ko kitni der baad dobara check karna hai. " +
        "0.25 - 0.5 recommended hai."
    )]
    [SerializeField, Min(0.1f)]
    private float rewardStatusCheckInterval = 0.5f;


    // ================================================================
    // TIMING
    // ================================================================

    [Header("Timing")]
    [SerializeField, Min(0.1f)]
    private float minIntervalSeconds = 3f;

    [SerializeField, Min(0.1f)]
    private float maxIntervalSeconds = 6f;

    [Tooltip(
        "Agar Daily Reward restriction OFF hai to animation automatically start hogi."
    )]
    [SerializeField]
    private bool autoStart = true;


    // ================================================================
    // SHAKE
    // ================================================================

    [Header("Shake Settings")]
    [SerializeField, Min(0.01f)]
    private float shakeDuration = 0.4f;

    [SerializeField]
    private Vector3 shakeStrength =
        new Vector3(18f, 18f, 0f);

    [SerializeField, Min(1)]
    private int shakeVibrato = 12;

    [SerializeField, Range(0f, 90f)]
    private float shakeRandomness = 90f;

    [SerializeField]
    private bool shakeFadeOut = true;


    // ================================================================
    // FALLING ITEMS
    // ================================================================

    [Header("Falling Items")]
    [Tooltip(
        "Random coin/gem/item prefabs."
    )]
    [SerializeField]
    private RectTransform[] itemPrefabs;

    [Tooltip(
        "Items ka parent. Khali ho to imageToShake ka parent use hoga."
    )]
    [SerializeField]
    private RectTransform spawnParent;

    [SerializeField, Min(0)]
    private int minItemCount = 3;

    [SerializeField, Min(0)]
    private int maxItemCount = 6;


    // ================================================================
    // SPAWN VARIATION
    // ================================================================

    [Header("Spawn Variation")]
    [Tooltip(
        "Spawn point ke around horizontal random spread."
    )]
    [SerializeField, Min(0f)]
    private float spawnSpreadX = 40f;


    // ================================================================
    // SCALE
    // ================================================================

    [Header("Item Scale")]
    [Tooltip(
        "Item spawn hote waqt prefab ke original scale ka kitna " +
        "percentage hoga. 0.3 = 30%, 1 = 100%."
    )]
    [SerializeField, Min(0.01f)]
    private float spawnStartScale = 0.3f;

    [Tooltip(
        "Item ka final scale prefab ke original scale ke comparison " +
        "mein. 1 = original size, 2 = double size."
    )]
    [SerializeField, Min(0.01f)]
    private float finalScaleMultiplier = 1f;

    [Tooltip(
        "Spawn start size se final size tak kitne seconds mein scale hoga."
    )]
    [SerializeField, Min(0f)]
    private float scaleUpDuration = 0.25f;

    [Tooltip(
        "Scale animation ki easing."
    )]
    [SerializeField]
    private Ease scaleEase = Ease.OutBack;

    [Tooltip(
        "ON: har spawned item ka final size random hoga."
    )]
    [SerializeField]
    private bool useRandomFinalScale = false;

    [Tooltip(
        "Random final scale ka minimum aur maximum multiplier."
    )]
    [SerializeField]
    private Vector2 randomFinalScaleRange =
        new Vector2(0.85f, 1.15f);


    // ================================================================
    // FALL
    // ================================================================

    [Header("Fall")]
    [SerializeField, Min(1f)]
    private float fallDistance = 260f;

    [SerializeField, Min(0f)]
    private float fallHorizontalDrift = 60f;

    [Tooltip(
        "Minimum fall duration. Zyada value = slower fall."
    )]
    [SerializeField, Min(0.05f)]
    private float fallDurationMin = 0.5f;

    [Tooltip(
        "Maximum fall duration. Zyada value = slower fall."
    )]
    [SerializeField, Min(0.05f)]
    private float fallDurationMax = 0.9f;

    [SerializeField]
    private Ease fallEase = Ease.InQuad;


    // ================================================================
    // ROTATION
    // ================================================================

    [Header("Rotation")]
    [SerializeField]
    private bool rotateWhileFalling = true;

    [SerializeField]
    private Vector2 rotationRange =
        new Vector2(-220f, 220f);


    // ================================================================
    // FADE
    // ================================================================

    [Header("Fade")]
    [SerializeField]
    private bool fadeOutAtEnd = true;

    [SerializeField, Range(0f, 1f)]
    private float fadeOutStartProgress = 0.7f;


    // ================================================================
    // RUNTIME
    // ================================================================

    private Coroutine loopRoutine;

    private Coroutine rewardMonitorRoutine;

    private bool animationAllowed;

    private bool rewardStateKnown;


    // ================================================================
    // UNITY
    // ================================================================

    private void Awake()
    {
        if (spawnParent == null &&
            imageToShake != null)
        {
            spawnParent =
                imageToShake.parent as RectTransform;
        }

        ValidateSpawnPoint();
        ValidateSettings();
    }


    private void OnEnable()
    {
        /*
         * Agar reward restriction enabled hai to
         * background mein reward status check hota rahega.
         */
        if (onlyRunWhenRewardUnlocked)
        {
            StartRewardMonitoring();
        }
        else if (autoStart)
        {
            animationAllowed = true;
            StartLoop();
        }
    }


    private void OnDisable()
    {
        StopLoop();

        StopRewardMonitoring();

        KillTargetShake();

        animationAllowed = false;
    }


    // ================================================================
    // REWARD MONITORING
    // ================================================================

    private void StartRewardMonitoring()
    {
        StopRewardMonitoring();

        rewardMonitorRoutine =
            StartCoroutine(
                RewardStatusMonitorRoutine()
            );
    }


    private void StopRewardMonitoring()
    {
        if (rewardMonitorRoutine != null)
        {
            StopCoroutine(
                rewardMonitorRoutine
            );

            rewardMonitorRoutine = null;
        }
    }


    private IEnumerator RewardStatusMonitorRoutine()
    {
        /*
         * Initial check foran.
         */
        UpdateRewardAnimationState();


        while (true)
        {
            /*
             * Realtime use kar rahe hain taake Time.timeScale = 0
             * hone par bhi reward state monitor ho.
             */
            yield return new WaitForSecondsRealtime(
                Mathf.Max(
                    0.1f,
                    rewardStatusCheckInterval
                )
            );


            UpdateRewardAnimationState();
        }
    }


    private void UpdateRewardAnimationState()
    {
        bool rewardUnlocked =
            IsCurrentRewardUnlocked();


        /*
         * State same hai to kuch nahi karna.
         */
        if (rewardStateKnown &&
            rewardUnlocked == animationAllowed)
        {
            return;
        }


        rewardStateKnown = true;


        if (rewardUnlocked)
        {
            EnableRewardAnimation();
        }
        else
        {
            DisableRewardAnimation();
        }
    }


    private bool IsCurrentRewardUnlocked()
    {
        /*
         * DailyRewardManager ka current reward claimable hai
         * to animation allow hogi.
         */
        if (DailyRewardManager.Instance == null)
        {
            /*
             * Manager abhi initialize nahi hua.
             *
             * Important:
             * Is situation mein animation start nahi karenge,
             * taake locked reward par accidentally animation na chale.
             */
            return false;
        }


        return
            DailyRewardManager.Instance.CanClaimCurrentReward;
    }


    private void EnableRewardAnimation()
    {
        animationAllowed = true;

        /*
         * Agar component active hai aur loop already running nahi
         * hai to foran start karo.
         */
        if (isActiveAndEnabled)
        {
            StartLoop();
        }
    }


    private void DisableRewardAnimation()
    {
        animationAllowed = false;

        StopLoop();

        KillTargetShake();
    }


    // ================================================================
    // LOOP
    // ================================================================

    public void StartLoop()
    {
        /*
         * Reward locked hai to manually bhi loop start nahi hoga
         * jab restriction enabled hai.
         */
        if (onlyRunWhenRewardUnlocked &&
            !IsCurrentRewardUnlocked())
        {
            animationAllowed = false;
            return;
        }


        animationAllowed = true;


        StopLoop();


        loopRoutine =
            StartCoroutine(
                ShakeAndDropLoopRoutine()
            );
    }


    public void StopLoop()
    {
        if (loopRoutine != null)
        {
            StopCoroutine(
                loopRoutine
            );

            loopRoutine = null;
        }
    }


    // ================================================================
    // MANUAL TRIGGER
    // ================================================================

    public void TriggerOnce()
    {
        /*
         * Manual trigger bhi reward restriction respect karega.
         */
        if (onlyRunWhenRewardUnlocked &&
            !IsCurrentRewardUnlocked())
        {
            return;
        }


        StartCoroutine(
            ShakeAndDropOnceRoutine()
        );
    }


    // ================================================================
    // MAIN LOOP
    // ================================================================

    private IEnumerator ShakeAndDropLoopRoutine()
    {
        while (true)
        {
            /*
             * Reward state agar beech mein locked ho jaye to
             * loop foran stop.
             */
            if (onlyRunWhenRewardUnlocked &&
                !IsCurrentRewardUnlocked())
            {
                animationAllowed = false;

                loopRoutine = null;

                yield break;
            }


            float wait =
                Random.Range(
                    minIntervalSeconds,
                    maxIntervalSeconds
                );


            /*
             * Realtime wait.
             *
             * Is se menu/pause mein bhi timer predictable rahega.
             */
            yield return new WaitForSecondsRealtime(
                wait
            );


            /*
             * Reward claim ho gaya ho to shake se pehle
             * dobara check.
             */
            if (onlyRunWhenRewardUnlocked &&
                !IsCurrentRewardUnlocked())
            {
                animationAllowed = false;

                loopRoutine = null;

                yield break;
            }


            yield return ShakeAndDropOnceRoutine();
        }


    }


    // ================================================================
    // SHAKE + DROP
    // ================================================================

    private IEnumerator ShakeAndDropOnceRoutine()
    {
        if (!animationAllowed &&
            onlyRunWhenRewardUnlocked)
        {
            yield break;
        }


        if (imageToShake == null)
        {
            yield break;
        }


        Tween shakeTween =
            imageToShake.DOShakeAnchorPos(
                shakeDuration,
                shakeStrength,
                shakeVibrato,
                shakeRandomness,
                false,
                shakeFadeOut
            );


        /*
         * Shake ke aadhe raste mein items spawn hongi.
         */
        yield return new WaitForSecondsRealtime(
            shakeDuration * 0.5f
        );


        /*
         * Reward claim hone ke case mein items spawn na hon.
         */
        if (onlyRunWhenRewardUnlocked &&
            !IsCurrentRewardUnlocked())
        {
            shakeTween.Kill();

            yield break;
        }


        SpawnFallingItems();


        yield return shakeTween.WaitForCompletion();
    }


    // ================================================================
    // SPAWN ITEMS
    // ================================================================

    private void SpawnFallingItems()
    {
        if (itemPrefabs == null ||
            itemPrefabs.Length == 0)
        {
            return;
        }


        int count =
            Random.Range(
                minItemCount,
                maxItemCount + 1
            );


        for (int i = 0;
             i < count;
             i++)
        {
            SpawnSingleItem();
        }
    }


    private void SpawnSingleItem()
    {
        RectTransform prefab =
            itemPrefabs[
                Random.Range(
                    0,
                    itemPrefabs.Length
                )
            ];


        if (prefab == null)
        {
            return;
        }


        RectTransform parent =
            spawnParent != null
                ? spawnParent
                : imageToShake.parent as RectTransform;


        if (parent == null)
        {
            return;
        }


        RectTransform item =
            Instantiate(
                prefab,
                parent
            );


        item.gameObject.SetActive(true);


        /*
         * Exact manually assigned spawn point.
         */
        Vector2 spawnPosition =
            GetSpawnPosition();


        item.anchoredPosition =
            spawnPosition;


        // ============================================================
        // SCALE
        // ============================================================

        Vector3 prefabScale =
            item.localScale;


        float finalMultiplier =
            finalScaleMultiplier;


        if (useRandomFinalScale)
        {
            float minimum =
                Mathf.Max(
                    0.01f,
                    randomFinalScaleRange.x
                );


            float maximum =
                Mathf.Max(
                    minimum,
                    randomFinalScaleRange.y
                );


            finalMultiplier =
                Random.Range(
                    minimum,
                    maximum
                );
        }


        Vector3 startScale =
            prefabScale *
            spawnStartScale;


        Vector3 finalScale =
            prefabScale *
            finalMultiplier;


        item.localScale =
            startScale;


        // ============================================================
        // CANVAS GROUP
        // ============================================================

        CanvasGroup canvasGroup =
            item.GetComponent<CanvasGroup>();


        if (canvasGroup == null)
        {
            canvasGroup =
                item.gameObject.AddComponent<CanvasGroup>();
        }


        canvasGroup.alpha = 1f;


        // ============================================================
        // SCALE-UP
        // ============================================================

        if (scaleUpDuration > 0f)
        {
            item.DOScale(
                finalScale,
                scaleUpDuration
            )
            .SetEase(
                scaleEase
            );
        }
        else
        {
            item.localScale =
                finalScale;
        }


        // ============================================================
        // FALL
        // ============================================================

        float fallDuration =
            Random.Range(
                fallDurationMin,
                fallDurationMax
            );


        float horizontalDrift =
            Random.Range(
                -fallHorizontalDrift,
                fallHorizontalDrift
            );


        Vector2 fallTarget =
            spawnPosition +
            new Vector2(
                horizontalDrift,
                -fallDistance
            );


        Sequence fallSequence =
            DOTween.Sequence();


        fallSequence.Append(
            item.DOAnchorPos(
                fallTarget,
                fallDuration
            )
            .SetEase(
                fallEase
            )
        );


        // ============================================================
        // ROTATION
        // ============================================================

        if (rotateWhileFalling)
        {
            float randomRotation =
                Random.Range(
                    rotationRange.x,
                    rotationRange.y
                );


            item.DORotate(
                new Vector3(
                    0f,
                    0f,
                    randomRotation
                ),
                fallDuration,
                RotateMode.FastBeyond360
            );
        }


        // ============================================================
        // FADE
        // ============================================================

        if (fadeOutAtEnd)
        {
            float safeFadeStart =
                Mathf.Clamp01(
                    fadeOutStartProgress
                );


            float fadeDelay =
                fallDuration *
                safeFadeStart;


            float fadeDuration =
                fallDuration *
                (1f - safeFadeStart);


            canvasGroup
                .DOFade(
                    0f,
                    Mathf.Max(
                        0.05f,
                        fadeDuration
                    )
                )
                .SetDelay(
                    fadeDelay
                );
        }


        // ============================================================
        // DESTROY
        // ============================================================

        fallSequence.OnComplete(
            () =>
            {
                if (item != null)
                {
                    Destroy(
                        item.gameObject
                    );
                }
            }
        );
    }


    // ================================================================
    // SPAWN POSITION
    // ================================================================

    private Vector2 GetSpawnPosition()
    {
        /*
         * Manual spawn point.
         */
        if (itemSpawnPoint != null)
        {
            Vector2 position =
                GetAnchoredPositionRelativeToParent(
                    itemSpawnPoint,
                    spawnParent
                );


            position.x +=
                Random.Range(
                    -spawnSpreadX,
                    spawnSpreadX
                );


            return position;
        }


        /*
         * Optional fallback.
         */
        if (fallbackToTargetPosition &&
            imageToShake != null)
        {
            Vector2 position =
                GetAnchoredPositionRelativeToParent(
                    imageToShake,
                    spawnParent
                );


            position.x +=
                Random.Range(
                    -spawnSpreadX,
                    spawnSpreadX
                );


            return position;
        }


        return Vector2.zero;
    }


    private static Vector2 GetAnchoredPositionRelativeToParent(
        RectTransform target,
        RectTransform targetParent)
    {
        if (target == null ||
            targetParent == null)
        {
            return Vector2.zero;
        }


        Vector3 worldPosition =
            target.position;


        Vector3 localPosition =
            targetParent.InverseTransformPoint(
                worldPosition
            );


        return new Vector2(
            localPosition.x,
            localPosition.y
        );
    }


    // ================================================================
    // CLEANUP
    // ================================================================

    private void KillTargetShake()
    {
        if (imageToShake != null)
        {
            imageToShake.DOKill();
        }
    }


    // ================================================================
    // VALIDATION
    // ================================================================

    private void ValidateSpawnPoint()
    {
        if (itemSpawnPoint == null)
        {
            Debug.LogWarning(
                "PeriodicShakeAndDropController: " +
                "Item Spawn Point assign nahi hai. " +
                "Fallback enabled ho to target image ki position use hogi.",
                this
            );
        }
    }


    private void ValidateSettings()
    {
        if (maxIntervalSeconds < minIntervalSeconds)
        {
            Debug.LogWarning(
                "PeriodicShakeAndDropController: " +
                "Max Interval ko Min Interval se kam rakha gaya hai.",
                this
            );
        }


        if (maxItemCount < minItemCount)
        {
            Debug.LogWarning(
                "PeriodicShakeAndDropController: " +
                "Max Item Count ko Min Item Count se kam rakha gaya hai.",
                this
            );
        }


        if (fallDurationMax < fallDurationMin)
        {
            Debug.LogWarning(
                "PeriodicShakeAndDropController: " +
                "Fall Duration Max ko Min se kam rakha gaya hai.",
                this
            );
        }


        if (randomFinalScaleRange.y <
            randomFinalScaleRange.x)
        {
            Debug.LogWarning(
                "PeriodicShakeAndDropController: " +
                "Random Final Scale Max ko Min se kam rakha gaya hai.",
                this
            );
        }
    }
}




