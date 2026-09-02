// using System.Collections;
// using System;
// using TMPro;
// using UnityEngine;


// public sealed class LevelCompleteSequenceController : MonoBehaviour
// {
//     [Header("Gameplay")]
//     [SerializeField]
//     private PopupGameplayVisibilityController gameplayVisibilityController;



//     [Header("Particle Parent")]
//     [SerializeField]
//     private GameObject particleParent;



//     [Header("You Win Text")]
//     [Tooltip(
//         "Optional TMP text. Khali ho to controller particle screen ke " +
//         "center mein YOU WIN text khud create karega."
//     )]
//     [SerializeField]
//     private TMP_Text youWinText;

//     [SerializeField]
//     private string youWinMessage = "YOU WIN";

//     [SerializeField, Min(1f)]
//     private float youWinFontSize = 112f;

//     [SerializeField]
//     private Color youWinColor = new Color(1f, 0.82f, 0.12f, 1f);

//     [SerializeField]
//     private Vector2 youWinAnchoredPosition = new Vector2(0f, 100f);



//     [Header("First Particle Group")]
//     [SerializeField]
//     private ParticleSystem particleOne;

//     [SerializeField]
//     private ParticleSystem particleTwo;



//     [Header("Second Particle Group")]
//     [SerializeField]
//     private ParticleSystem particleThree;

//     [SerializeField]
//     private ParticleSystem particleFour;








//     [Header("First Group Slide In")]
//     [Tooltip(
//         "ON: pehle group ke confetti cannons left/right se slide ho kar " +
//         "aate hain, aur apni jagah pohanchne ke BAAD particles play hote hain."
//     )]
//     [SerializeField]
//     private bool animateFirstGroupSlideIn = true;

//     [Tooltip(
//         "Optional. Khali chhod dein to particleOne/particleTwo ka apna " +
//         "parent (jis image ke wo child hain) khud use ho jayega."
//     )]
//     [SerializeField]
//     private RectTransform particleOneRoot;

//     [SerializeField]
//     private RectTransform particleTwoRoot;

//     [Tooltip(
//         "Apni jagah se kitna door (off-screen) se slide shuru hoga."
//     )]
//     [SerializeField, Min(0f)]
//     private float firstGroupSlideDistance = 700f;

//     [SerializeField, Min(0.01f)]
//     private float firstGroupSlideDuration = 0.45f;


//     [Header("Cannon Shoot Animation")]
//     [Tooltip(
//         "ON: doosre group ke particles play hote waqt un ki cannon image " +
//         "peechay ko recoil karti hai, taake lage ke shot cannon se nikla hai."
//     )]
//     [SerializeField]
//     private bool animateCannonShoot = true;

//     [Tooltip(
//         "Optional. Khali chhod dein to particleThree/particleFour ka apna " +
//         "parent (cannon image) khud use ho jayega."
//     )]
//     [SerializeField]
//     private RectTransform particleThreeRoot;

//     [SerializeField]
//     private RectTransform particleFourRoot;

//     [Tooltip(
//         "Cannon apne barrel ke ulat kitna peechay hatega."
//     )]
//     [SerializeField, Min(0f)]
//     private float cannonRecoilDistance = 34f;

//     [Tooltip("Peechay hatne ka time — tez rakhein taake kick lage.")]
//     [SerializeField, Min(0.01f)]
//     private float cannonRecoilBackDuration = 0.07f;

//     [Tooltip("Wapas apni jagah aane ka time.")]
//     [SerializeField, Min(0.01f)]
//     private float cannonRecoilReturnDuration = 0.22f;

//     [SerializeField, Range(1f, 1.5f)]
//     private float cannonShootPunchScale = 1.12f;


//     [Header("Timing")]
//     [SerializeField, Min(0f)]
//     private float celebrationDuration = 1.5f;


//     [SerializeField]
//     private float popupDelay = 0.2f;



//     [Header("Level Complete Popup")]
//     [SerializeField]
//     private GameObject levelCompletePopup;



//     private Coroutine sequenceRoutine;
//     private Action sequenceCompleted;

//     /*
//      * Har particle ka "root" wo image hai jiska wo child hai.
//      * Ye ek hi baar resolve aur capture hote hain, taake sequence
//      * dobara chalne par base pose se hi shuru ho.
//      */
//     private RectTransform resolvedRootOne;
//     private RectTransform resolvedRootTwo;
//     private RectTransform resolvedRootThree;
//     private RectTransform resolvedRootFour;

//     private Vector2 baseAnchoredOne;
//     private Vector2 baseAnchoredTwo;
//     private Vector2 baseAnchoredThree;
//     private Vector2 baseAnchoredFour;

//     private Vector3 baseScaleThree;
//     private Vector3 baseScaleFour;

//     private bool rootsResolved;



//     private void Awake()
//     {
//         ResolveConfettiParticles();
//         EnsureYouWinText();
//         HideYouWinText();

//         if(levelCompletePopup != null)
//         {
//             /*
//              * UITransition ke through hide karte hain, raw SetActive se
//              * nahi - warna is panel ki base pose (scale / anchored
//              * position / alpha) UITransition ke paas kabhi register hi
//              * nahi hoti. Neeche LevelCompleteRoutine aur
//              * LevelCompleteUIController dono isay UITransition se hi
//              * show/hide karte hain.
//              */
//             UITransition.HideImmediate(levelCompletePopup);
//         }


//         if(particleParent != null)
//         {
//             particleParent.SetActive(false);
//         }
//     }



//     public void PlayLevelCompleteSequence(
//         Action onSequenceCompleted = null)
//     {
//         StopSequence();

//         sequenceCompleted = onSequenceCompleted;


//         sequenceRoutine =
//             StartCoroutine(
//                 LevelCompleteRoutine()
//             );
//     }




//     private IEnumerator LevelCompleteRoutine()
//     {
//         ResolveConfettiParticles();

//         if(levelCompletePopup != null)
//         {
//             UITransition.HideImmediate(
//                 levelCompletePopup
//             );
//         }

//         // Hide gameplay
//         if(gameplayVisibilityController != null)
//         {
//             gameplayVisibilityController.HideGameplay();
//         }



//         // Enable particle screen
//         if(particleParent != null)
//         {
//             particleParent.SetActive(true);
//         }



//         ShowYouWinText();

//         ResolveRoots();


//         // Pehle group ke cannons left/right se slide ho kar aate hain,
//         // aur apni jagah pohanchne ke BAAD hi particles play hote hain.
//         // Dono confetti cannons ek hi waqt sirf ek burst chalati hain.



//         // Second 2 particles — cannon recoil particles ke sath hi
//         // chalta hai, taake shot cannon se nikalta hua lage.
//         PlayCannonShoot(
//             resolvedRootThree,
//             baseAnchoredThree,
//             baseScaleThree
//         );

//         PlayCannonShoot(
//             resolvedRootFour,
//             baseAnchoredFour,
//             baseScaleFour
//         );

//         PlayParticle(
//             particleThree
//         );

//         PlayParticle(
//             particleFour
//         );


//         yield return WaitForUnscaledSeconds(
//             celebrationDuration
//         );


//         StopParticle(particleOne);
//         StopParticle(particleTwo);
//         StopParticle(particleThree);
//         StopParticle(particleFour);



//         HideYouWinText();



//         // Disable particle screen
//         if(particleParent != null)
//         {
//             particleParent.SetActive(false);
//         }



//         yield return WaitForUnscaledSeconds(
//             popupDelay
//         );


//         Action completedCallback =
//             sequenceCompleted;

//         sequenceCompleted = null;
//         sequenceRoutine = null;


//         // LevelCompleteUIController ka proper popup setup/pause use karo.
//         if(completedCallback != null)
//         {
//             completedCallback.Invoke();
//         }
//         else if(levelCompletePopup != null)
//         {
//             UITransition.Show(
//                 levelCompletePopup
//             );
//         }
//     }





//     /// <summary>
//     /// Har particle ka root wo image hai jiska wo child hai. Inspector
//     /// mein override diya ho to wo, warna particle ka apna parent.
//     /// Base pose sirf ek baar capture hoti hai.
//     /// </summary>
//     private void ResolveRoots()
//     {
//         if (rootsResolved)
//         {
//             return;
//         }

//         resolvedRootOne = ResolveRoot(particleOneRoot, particleOne);
//         resolvedRootTwo = ResolveRoot(particleTwoRoot, particleTwo);
//         resolvedRootThree = ResolveRoot(particleThreeRoot, particleThree);
//         resolvedRootFour = ResolveRoot(particleFourRoot, particleFour);

//         if (resolvedRootOne != null)
//         {
//             baseAnchoredOne = resolvedRootOne.anchoredPosition;
//         }

//         if (resolvedRootTwo != null)
//         {
//             baseAnchoredTwo = resolvedRootTwo.anchoredPosition;
//         }

//         if (resolvedRootThree != null)
//         {
//             baseAnchoredThree = resolvedRootThree.anchoredPosition;
//             baseScaleThree = resolvedRootThree.localScale;
//         }

//         if (resolvedRootFour != null)
//         {
//             baseAnchoredFour = resolvedRootFour.anchoredPosition;
//             baseScaleFour = resolvedRootFour.localScale;
//         }

//         rootsResolved = true;
//     }


//     /// <summary>
//     /// Scene mein confetti references manually missing hon to particle
//     /// overlay ke andar se dono existing ParticleSystems khud pick karo.
//     /// </summary>
//     private void ResolveConfettiParticles()
//     {
//         if (particleParent == null ||
//             (particleThree != null && particleFour != null))
//         {
//             return;
//         }

//         ParticleSystem[] particles =
//             particleParent.GetComponentsInChildren<ParticleSystem>(true);

//         bool referencesChanged = false;

//         foreach (ParticleSystem candidate in particles)
//         {
//             if (candidate == null)
//             {
//                 continue;
//             }

//             if (particleThree == null &&
//                 candidate != particleFour)
//             {
//                 particleThree = candidate;
//                 referencesChanged = true;
//                 continue;
//             }

//             if (particleFour == null &&
//                 candidate != particleThree)
//             {
//                 particleFour = candidate;
//                 referencesChanged = true;
//             }

//             if (particleThree != null &&
//                 particleFour != null)
//             {
//                 break;
//             }
//         }

//         if (referencesChanged)
//         {
//             rootsResolved = false;
//         }
//     }


//     private static RectTransform ResolveRoot(
//         RectTransform explicitRoot,
//         ParticleSystem particle)
//     {
//         if (explicitRoot != null)
//         {
//             return explicitRoot;
//         }

//         if (particle == null)
//         {
//             return null;
//         }

//         return particle.transform.parent as RectTransform;
//     }


//     /// <summary>
//     /// Pehle group ke dono roots ko off-screen se apni jagah tak slide
//     /// karta hai. Jo root screen ke right half mein hai wo right se aata
//     /// hai, left wala left se.
//     /// </summary>
//     private IEnumerator SlideInFirstGroupRoutine()
//     {
//         if (!animateFirstGroupSlideIn ||
//             (resolvedRootOne == null && resolvedRootTwo == null))
//         {
//             yield break;
//         }

//         Vector2 startOne =
//             GetSlideStartPosition(baseAnchoredOne);

//         Vector2 startTwo =
//             GetSlideStartPosition(baseAnchoredTwo);

//         if (resolvedRootOne != null)
//         {
//             resolvedRootOne.anchoredPosition = startOne;
//         }

//         if (resolvedRootTwo != null)
//         {
//             resolvedRootTwo.anchoredPosition = startTwo;
//         }

//         float duration =
//             Mathf.Max(0.01f, firstGroupSlideDuration);

//         float elapsed = 0f;

//         while (elapsed < duration)
//         {
//             float progress =
//                 Mathf.Clamp01(elapsed / duration);

//             // Ease-out: tez aata hai, phir narmi se apni jagah rukta hai.
//             float eased =
//                 1f - Mathf.Pow(1f - progress, 3f);

//             if (resolvedRootOne != null)
//             {
//                 resolvedRootOne.anchoredPosition =
//                     Vector2.LerpUnclamped(
//                         startOne,
//                         baseAnchoredOne,
//                         eased
//                     );
//             }

//             if (resolvedRootTwo != null)
//             {
//                 resolvedRootTwo.anchoredPosition =
//                     Vector2.LerpUnclamped(
//                         startTwo,
//                         baseAnchoredTwo,
//                         eased
//                     );
//             }

//             elapsed += Time.unscaledDeltaTime;

//             yield return null;
//         }

//         if (resolvedRootOne != null)
//         {
//             resolvedRootOne.anchoredPosition = baseAnchoredOne;
//         }

//         if (resolvedRootTwo != null)
//         {
//             resolvedRootTwo.anchoredPosition = baseAnchoredTwo;
//         }
//     }


//     private Vector2 GetSlideStartPosition(
//         Vector2 basePosition)
//     {
//         float direction =
//             basePosition.x >= 0f ? 1f : -1f;

//         return basePosition +
//                new Vector2(
//                    direction * firstGroupSlideDistance,
//                    0f
//                );
//     }


//     private void PlayCannonShoot(
//         RectTransform cannonRoot,
//         Vector2 basePosition,
//         Vector3 baseScale)
//     {
//         if (!animateCannonShoot ||
//             cannonRoot == null)
//         {
//             return;
//         }

//         StartCoroutine(
//             CannonShootRoutine(
//                 cannonRoot,
//                 basePosition,
//                 baseScale
//             )
//         );
//     }


//     /// <summary>
//     /// Cannon ka recoil: barrel ke ULAT taraf tez kick, phir narmi se
//     /// wapas apni jagah, sath mein halka scale punch.
//     ///
//     /// Recoil direction cannon ke apne rotated "up" ka ulta hai, is liye
//     /// dono tilted cannons (+24.78 aur -24.78) khud-ba-khud mirror ho
//     /// jate hain — koi alag setting ki zaroorat nahi.
//     /// </summary>
//     private IEnumerator CannonShootRoutine(
//         RectTransform cannonRoot,
//         Vector2 basePosition,
//         Vector3 baseScale)
//     {
//         Vector2 recoilDirection =
//             -(Vector2)cannonRoot.up;

//         if (recoilDirection.sqrMagnitude < 0.0001f)
//         {
//             recoilDirection = Vector2.down;
//         }

//         Vector2 recoilPosition =
//             basePosition +
//             recoilDirection.normalized * cannonRecoilDistance;

//         Vector3 punchScale =
//             baseScale * cannonShootPunchScale;

//         // Kick back
//         float backDuration =
//             Mathf.Max(0.01f, cannonRecoilBackDuration);

//         float elapsed = 0f;

//         while (elapsed < backDuration &&
//                cannonRoot != null)
//         {
//             float progress =
//                 Mathf.Clamp01(elapsed / backDuration);

//             cannonRoot.anchoredPosition =
//                 Vector2.LerpUnclamped(
//                     basePosition,
//                     recoilPosition,
//                     progress
//                 );

//             cannonRoot.localScale =
//                 Vector3.LerpUnclamped(
//                     baseScale,
//                     punchScale,
//                     progress
//                 );

//             elapsed += Time.unscaledDeltaTime;

//             yield return null;
//         }

//         // Wapas apni jagah
//         float returnDuration =
//             Mathf.Max(0.01f, cannonRecoilReturnDuration);

//         elapsed = 0f;

//         while (elapsed < returnDuration &&
//                cannonRoot != null)
//         {
//             float progress =
//                 Mathf.Clamp01(elapsed / returnDuration);

//             float eased =
//                 1f - Mathf.Pow(1f - progress, 3f);

//             cannonRoot.anchoredPosition =
//                 Vector2.LerpUnclamped(
//                     recoilPosition,
//                     basePosition,
//                     eased
//                 );

//             cannonRoot.localScale =
//                 Vector3.LerpUnclamped(
//                     punchScale,
//                     baseScale,
//                     eased
//                 );

//             elapsed += Time.unscaledDeltaTime;

//             yield return null;
//         }

//         if (cannonRoot != null)
//         {
//             cannonRoot.anchoredPosition = basePosition;
//             cannonRoot.localScale = baseScale;
//         }
//     }


//     /// <summary>
//     /// Slide aur recoil ke baad sab kuch apni original jagah par.
//     /// </summary>
//     private void RestoreRootPoses()
//     {
//         if (!rootsResolved)
//         {
//             return;
//         }

//         if (resolvedRootOne != null)
//         {
//             resolvedRootOne.anchoredPosition = baseAnchoredOne;
//         }

//         if (resolvedRootTwo != null)
//         {
//             resolvedRootTwo.anchoredPosition = baseAnchoredTwo;
//         }

//         if (resolvedRootThree != null)
//         {
//             resolvedRootThree.anchoredPosition = baseAnchoredThree;
//             resolvedRootThree.localScale = baseScaleThree;
//         }

//         if (resolvedRootFour != null)
//         {
//             resolvedRootFour.anchoredPosition = baseAnchoredFour;
//             resolvedRootFour.localScale = baseScaleFour;
//         }
//     }


//     private void PlayParticle(
//         ParticleSystem particle)
//     {
//         if(particle == null)
//         {
//             return;
//         }


//         particle.Stop(
//             true,
//             ParticleSystemStopBehavior.StopEmittingAndClear
//         );


//         ParticleSystem.MainModule main =
//             particle.main;

//         main.loop = false;


//         particle.Play();
//     }


//     private void EnsureYouWinText()
//     {
//         if (youWinText != null ||
//             particleParent == null)
//         {
//             return;
//         }

//         GameObject textObject =
//             new GameObject(
//                 "You Win Text",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         textObject.layer = particleParent.layer;

//         RectTransform textRect =
//             textObject.GetComponent<RectTransform>();

//         textRect.SetParent(
//             particleParent.transform,
//             false
//         );

//         textRect.anchorMin = new Vector2(0.5f, 0.5f);
//         textRect.anchorMax = new Vector2(0.5f, 0.5f);
//         textRect.pivot = new Vector2(0.5f, 0.5f);
//         textRect.anchoredPosition = youWinAnchoredPosition;
//         textRect.sizeDelta = new Vector2(900f, 200f);
//         textRect.SetAsLastSibling();

//         youWinText =
//             textObject.GetComponent<TextMeshProUGUI>();

//         youWinText.alignment =
//             TextAlignmentOptions.Center;

//         youWinText.fontStyle = FontStyles.Bold;
//         youWinText.fontSize = youWinFontSize;
//         youWinText.color = youWinColor;
//         youWinText.raycastTarget = false;
//         youWinText.textWrappingMode = TextWrappingModes.NoWrap;
//     }


//     private void ShowYouWinText()
//     {
//         EnsureYouWinText();

//         if (youWinText == null)
//         {
//             return;
//         }

//         youWinText.text =
//             string.IsNullOrWhiteSpace(youWinMessage)
//                 ? "YOU WIN"
//                 : youWinMessage;

//         youWinText.fontSize = youWinFontSize;
//         youWinText.color = youWinColor;
//         youWinText.rectTransform.anchoredPosition =
//             youWinAnchoredPosition;

//         youWinText.gameObject.SetActive(true);
//         youWinText.rectTransform.SetAsLastSibling();
//     }


//     private void HideYouWinText()
//     {
//         if (youWinText != null)
//         {
//             youWinText.gameObject.SetActive(false);
//         }
//     }


//     private static void StopParticle(
//         ParticleSystem particle)
//     {
//         if(particle == null)
//         {
//             return;
//         }

//         particle.Stop(
//             true,
//             ParticleSystemStopBehavior.StopEmittingAndClear
//         );
//     }


//     private static IEnumerator WaitForUnscaledSeconds(
//         float duration)
//     {
//         float elapsed = 0f;

//         while(elapsed < Mathf.Max(0f, duration))
//         {
//             elapsed += Time.unscaledDeltaTime;
//             yield return null;
//         }
//     }


//     public void StopSequence()
//     {
//         // A managed reference to a destroyed Unity object can still reach this
//         // method. Avoid calling native MonoBehaviour APIs in that state.
//         if (this == null)
//         {
//             return;
//         }

//         /*
//          * Cannon recoil apni alag coroutines mein chalta hai, is liye
//          * sirf sequenceRoutine rokna kaafi nahi.
//          */
//         StopAllCoroutines();

//         sequenceRoutine = null;
//         sequenceCompleted = null;

//         RestoreRootPoses();

//         StopParticle(particleOne);
//         StopParticle(particleTwo);
//         StopParticle(particleThree);
//         StopParticle(particleFour);

//         HideYouWinText();

//         if(particleParent != null)
//         {
//             particleParent.SetActive(false);
//         }
//     }


//     private void OnDisable()
//     {
//         StopSequence();
//     }
// }














using System.Collections;
using System;
using TMPro;
using UnityEngine;


public sealed class LevelCompleteSequenceController : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField]
    private PopupGameplayVisibilityController gameplayVisibilityController;


    [Header("Particle Parent")]
    [SerializeField]
    private GameObject particleParent;


    [Header("You Win Text")]
    [Tooltip(
        "Optional TMP text. Khali ho to controller particle screen ke " +
        "center mein YOU WIN text khud create karega."
    )]
    [SerializeField]
    private TMP_Text youWinText;

    [SerializeField]
    private string youWinMessage = "YOU WIN";

    [SerializeField, Min(1f)]
    private float youWinFontSize = 112f;

    [SerializeField]
    private Color youWinColor = new Color(1f, 0.82f, 0.12f, 1f);

    [SerializeField]
    private Vector2 youWinAnchoredPosition = new Vector2(0f, 100f);

    [Tooltip(
        "Screen upar slide hone ke BAAD YOU WIN text kitne time mein " +
        "fade in hoga."
    )]
    [SerializeField, Min(0.01f)]
    private float youWinFadeDuration = 0.35f;


    [Header("First Particle Group")]
    [SerializeField]
    private ParticleSystem particleOne;

    [SerializeField]
    private ParticleSystem particleTwo;


    [Header("Second Particle Group")]
    [SerializeField]
    private ParticleSystem particleThree;

    [SerializeField]
    private ParticleSystem particleFour;


    [Header("First Group Slide In")]
    [Tooltip(
        "ON: pehle group ke confetti cannons left/right se slide ho kar " +
        "aate hain, aur apni jagah pohanchne ke BAAD particles play hote hain."
    )]
    [SerializeField]
    private bool animateFirstGroupSlideIn = true;

    [Tooltip(
        "Optional. Khali chhod dein to particleOne/particleTwo ka apna " +
        "parent (jis image ke wo child hain) khud use ho jayega."
    )]
    [SerializeField]
    private RectTransform particleOneRoot;

    [SerializeField]
    private RectTransform particleTwoRoot;

    [Tooltip(
        "Apni jagah se kitna door (off-screen) se slide shuru hoga."
    )]
    [SerializeField, Min(0f)]
    private float firstGroupSlideDistance = 700f;

    [SerializeField, Min(0.01f)]
    private float firstGroupSlideDuration = 0.45f;


    [Header("Cannon Shoot Animation")]
    [Tooltip(
        "ON: doosre group ke particles play hote waqt un ki cannon image " +
        "peechay ko recoil karti hai, taake lage ke shot cannon se nikla hai."
    )]
    [SerializeField]
    private bool animateCannonShoot = true;

    [Tooltip(
        "Optional. Khali chhod dein to particleThree/particleFour ka apna " +
        "parent (cannon image) khud use ho jayega."
    )]
    [SerializeField]
    private RectTransform particleThreeRoot;

    [SerializeField]
    private RectTransform particleFourRoot;

    [Tooltip(
        "Cannon apne barrel ke ulat kitna peechay hatega."
    )]
    [SerializeField, Min(0f)]
    private float cannonRecoilDistance = 34f;

    [Tooltip("Peechay hatne ka time — tez rakhein taake kick lage.")]
    [SerializeField, Min(0.01f)]
    private float cannonRecoilBackDuration = 0.07f;

    [Tooltip("Wapas apni jagah aane ka time.")]
    [SerializeField, Min(0.01f)]
    private float cannonRecoilReturnDuration = 0.22f;

    [SerializeField, Range(1f, 1.5f)]
    private float cannonShootPunchScale = 1.12f;


    [Header("Screen Slide In (Bottom -> Top)")]
    [Tooltip(
        "ON: pura confetti screen (YOU WIN text + dono confetti groups) " +
        "bottom se slide ho kar apni final position (top) par aata hai. " +
        "Ye particleParent ke RectTransform ko move karta hai, is liye " +
        "text aur confetti dono sath sath move karte hain."
    )]
    [SerializeField]
    private bool animateScreenSlideIn = true;

    [Tooltip(
        "Optional. Khali chhod dein to particleParent ka apna RectTransform " +
        "khud use ho jayega."
    )]
    [SerializeField]
    private RectTransform particleScreenRoot;

    [Tooltip(
        "Screen apni final position se kitna neechay (off-screen) se slide " +
        "shuru karegi."
    )]
    [SerializeField, Min(0f)]
    private float screenSlideDistance = 1000f;

    [SerializeField, Min(0.01f)]
    private float screenSlideDuration = 0.5f;


    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float celebrationDuration = 1.5f;

    [SerializeField]
    private float popupDelay = 0.2f;


    [Header("Level Complete Popup")]
    [SerializeField]
    private GameObject levelCompletePopup;


    private Coroutine sequenceRoutine;
    private Action sequenceCompleted;

    /*
     * Har particle ka "root" wo image hai jiska wo child hai.
     * Ye ek hi baar resolve aur capture hote hain, taake sequence
     * dobara chalne par base pose se hi shuru ho.
     */
    private RectTransform resolvedRootOne;
    private RectTransform resolvedRootTwo;
    private RectTransform resolvedRootThree;
    private RectTransform resolvedRootFour;

    private Vector2 baseAnchoredOne;
    private Vector2 baseAnchoredTwo;
    private Vector2 baseAnchoredThree;
    private Vector2 baseAnchoredFour;

    private Vector3 baseScaleThree;
    private Vector3 baseScaleFour;

    private bool rootsResolved;

    // Poori confetti screen ki base (final) anchored position.
    private Vector2 baseScreenAnchoredPosition;
    private bool screenRootResolved;


    private void Awake()
    {
        ResolveConfettiParticles();
        EnsureYouWinText();
        HideYouWinText();
        ResolveScreenRoot();

        if(levelCompletePopup != null)
        {
            /*
             * UITransition ke through hide karte hain, raw SetActive se
             * nahi - warna is panel ki base pose (scale / anchored
             * position / alpha) UITransition ke paas kabhi register hi
             * nahi hoti. Neeche LevelCompleteRoutine aur
             * LevelCompleteUIController dono isay UITransition se hi
             * show/hide karte hain.
             */
            UITransition.HideImmediate(levelCompletePopup);
        }

        if(particleParent != null)
        {
            particleParent.SetActive(false);
        }
    }


    public void PlayLevelCompleteSequence(
        Action onSequenceCompleted = null)
    {
        StopSequence();

        sequenceCompleted = onSequenceCompleted;

        sequenceRoutine =
            StartCoroutine(
                LevelCompleteRoutine()
            );
    }


    private IEnumerator LevelCompleteRoutine()
    {
        ResolveConfettiParticles();

        if(levelCompletePopup != null)
        {
            UITransition.HideImmediate(
                levelCompletePopup
            );
        }

        // Hide gameplay
        if(gameplayVisibilityController != null)
        {
            gameplayVisibilityController.HideGameplay();
        }

        // Enable particle screen
        if(particleParent != null)
        {
            particleParent.SetActive(true);
        }

        ResolveScreenRoot();
        ResolveRoots();

        // Text abhi gameObject active kar dete hain (taake wo screen ke
        // sath slide ho), lekin alpha 0 rakhte hain — is liye slide ke
        // dauran khud text nazar nahi aata, sirf confetti/cannons.
        ShowYouWinText();
        SetYouWinAlpha(0f);

        // Poori screen (text + confetti) ko bottom (off-screen) par
        // teleport karo, phir apni final position (top) tak slide karo.
        // Confetti aur cannon roots particleScreenRoot ke children hain,
        // is liye wo sab is ke sath hi move hote hain.
        yield return SlideScreenInRoutine();

        // Screen apni final jagah par pohanchne ke BAAD YOU WIN text
        // ussi jagah par fade in hota hai.
        yield return FadeInYouWinTextRoutine();

        // Pehle group ke cannons left/right se slide ho kar aate hain,
        // aur apni jagah pohanchne ke BAAD hi particles play hote hain.
        // Dono confetti cannons ek hi waqt sirf ek burst chalati hain.

        // Second 2 particles — cannon recoil particles ke sath hi
        // chalta hai, taake shot cannon se nikalta hua lage.
        PlayCannonShoot(
            resolvedRootThree,
            baseAnchoredThree,
            baseScaleThree
        );

        PlayCannonShoot(
            resolvedRootFour,
            baseAnchoredFour,
            baseScaleFour
        );

        PlayParticle(
            particleThree
        );

        PlayParticle(
            particleFour
        );

        yield return WaitForUnscaledSeconds(
            celebrationDuration
        );

        StopParticle(particleOne);
        StopParticle(particleTwo);
        StopParticle(particleThree);
        StopParticle(particleFour);

        HideYouWinText();

        // Disable particle screen
        if(particleParent != null)
        {
            particleParent.SetActive(false);
        }

        // Agli baar sequence chalne par screen wapas bottom se slide ho,
        // is liye base position par reset kar dete hain.
        RestoreScreenPose();

        yield return WaitForUnscaledSeconds(
            popupDelay
        );

        Action completedCallback =
            sequenceCompleted;

        sequenceCompleted = null;
        sequenceRoutine = null;

        // LevelCompleteUIController ka proper popup setup/pause use karo.
        if(completedCallback != null)
        {
            completedCallback.Invoke();
        }
        else if(levelCompletePopup != null)
        {
            UITransition.Show(
                levelCompletePopup
            );
        }
    }


    /// <summary>
    /// particleParent ka apna RectTransform resolve karta hai (ya
    /// inspector override), aur is ki final (base) anchored position
    /// ek hi baar capture karta hai.
    /// </summary>
    private void ResolveScreenRoot()
    {
        if (screenRootResolved)
        {
            return;
        }

        if (particleScreenRoot == null &&
            particleParent != null)
        {
            particleScreenRoot =
                particleParent.GetComponent<RectTransform>();
        }

        if (particleScreenRoot != null)
        {
            baseScreenAnchoredPosition =
                particleScreenRoot.anchoredPosition;

            screenRootResolved = true;
        }
    }


    /// <summary>
    /// Poori confetti screen ko apni final position se neechay
    /// (off-screen) par teleport karta hai, phir ease-out ke sath
    /// upar apni final position tak slide karta hai.
    /// </summary>
    private IEnumerator SlideScreenInRoutine()
    {
        if (!animateScreenSlideIn ||
            particleScreenRoot == null)
        {
            yield break;
        }

        Vector2 startPosition =
            baseScreenAnchoredPosition +
            new Vector2(0f, -screenSlideDistance);

        particleScreenRoot.anchoredPosition = startPosition;

        float duration =
            Mathf.Max(0.01f, screenSlideDuration);

        float elapsed = 0f;

        while (elapsed < duration &&
               particleScreenRoot != null)
        {
            float progress =
                Mathf.Clamp01(elapsed / duration);

            // Ease-out: tez upar aata hai, phir narmi se apni jagah rukta hai.
            float eased =
                1f - Mathf.Pow(1f - progress, 3f);

            particleScreenRoot.anchoredPosition =
                Vector2.LerpUnclamped(
                    startPosition,
                    baseScreenAnchoredPosition,
                    eased
                );

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        if (particleScreenRoot != null)
        {
            particleScreenRoot.anchoredPosition =
                baseScreenAnchoredPosition;
        }
    }


    /// <summary>
    /// Screen ko wapas apni final (base) position par le aata hai,
    /// bina animation ke — sequence khatam hone ya stop hone par.
    /// </summary>
    private void RestoreScreenPose()
    {
        if (!screenRootResolved ||
            particleScreenRoot == null)
        {
            return;
        }

        particleScreenRoot.anchoredPosition =
            baseScreenAnchoredPosition;
    }


    /// <summary>
    /// Har particle ka root wo image hai jiska wo child hai. Inspector
    /// mein override diya ho to wo, warna particle ka apna parent.
    /// Base pose sirf ek baar capture hoti hai.
    /// </summary>
    private void ResolveRoots()
    {
        if (rootsResolved)
        {
            return;
        }

        resolvedRootOne = ResolveRoot(particleOneRoot, particleOne);
        resolvedRootTwo = ResolveRoot(particleTwoRoot, particleTwo);
        resolvedRootThree = ResolveRoot(particleThreeRoot, particleThree);
        resolvedRootFour = ResolveRoot(particleFourRoot, particleFour);

        if (resolvedRootOne != null)
        {
            baseAnchoredOne = resolvedRootOne.anchoredPosition;
        }

        if (resolvedRootTwo != null)
        {
            baseAnchoredTwo = resolvedRootTwo.anchoredPosition;
        }

        if (resolvedRootThree != null)
        {
            baseAnchoredThree = resolvedRootThree.anchoredPosition;
            baseScaleThree = resolvedRootThree.localScale;
        }

        if (resolvedRootFour != null)
        {
            baseAnchoredFour = resolvedRootFour.anchoredPosition;
            baseScaleFour = resolvedRootFour.localScale;
        }

        rootsResolved = true;
    }


    /// <summary>
    /// Scene mein confetti references manually missing hon to particle
    /// overlay ke andar se dono existing ParticleSystems khud pick karo.
    /// </summary>
    private void ResolveConfettiParticles()
    {
        if (particleParent == null ||
            (particleThree != null && particleFour != null))
        {
            return;
        }

        ParticleSystem[] particles =
            particleParent.GetComponentsInChildren<ParticleSystem>(true);

        bool referencesChanged = false;

        foreach (ParticleSystem candidate in particles)
        {
            if (candidate == null)
            {
                continue;
            }

            if (particleThree == null &&
                candidate != particleFour)
            {
                particleThree = candidate;
                referencesChanged = true;
                continue;
            }

            if (particleFour == null &&
                candidate != particleThree)
            {
                particleFour = candidate;
                referencesChanged = true;
            }

            if (particleThree != null &&
                particleFour != null)
            {
                break;
            }
        }

        if (referencesChanged)
        {
            rootsResolved = false;
        }
    }


    private static RectTransform ResolveRoot(
        RectTransform explicitRoot,
        ParticleSystem particle)
    {
        if (explicitRoot != null)
        {
            return explicitRoot;
        }

        if (particle == null)
        {
            return null;
        }

        return particle.transform.parent as RectTransform;
    }


    /// <summary>
    /// Pehle group ke dono roots ko off-screen se apni jagah tak slide
    /// karta hai. Jo root screen ke right half mein hai wo right se aata
    /// hai, left wala left se.
    /// </summary>
    private IEnumerator SlideInFirstGroupRoutine()
    {
        if (!animateFirstGroupSlideIn ||
            (resolvedRootOne == null && resolvedRootTwo == null))
        {
            yield break;
        }

        Vector2 startOne =
            GetSlideStartPosition(baseAnchoredOne);

        Vector2 startTwo =
            GetSlideStartPosition(baseAnchoredTwo);

        if (resolvedRootOne != null)
        {
            resolvedRootOne.anchoredPosition = startOne;
        }

        if (resolvedRootTwo != null)
        {
            resolvedRootTwo.anchoredPosition = startTwo;
        }

        float duration =
            Mathf.Max(0.01f, firstGroupSlideDuration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress =
                Mathf.Clamp01(elapsed / duration);

            // Ease-out: tez aata hai, phir narmi se apni jagah rukta hai.
            float eased =
                1f - Mathf.Pow(1f - progress, 3f);

            if (resolvedRootOne != null)
            {
                resolvedRootOne.anchoredPosition =
                    Vector2.LerpUnclamped(
                        startOne,
                        baseAnchoredOne,
                        eased
                    );
            }

            if (resolvedRootTwo != null)
            {
                resolvedRootTwo.anchoredPosition =
                    Vector2.LerpUnclamped(
                        startTwo,
                        baseAnchoredTwo,
                        eased
                    );
            }

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        if (resolvedRootOne != null)
        {
            resolvedRootOne.anchoredPosition = baseAnchoredOne;
        }

        if (resolvedRootTwo != null)
        {
            resolvedRootTwo.anchoredPosition = baseAnchoredTwo;
        }
    }


    private Vector2 GetSlideStartPosition(
        Vector2 basePosition)
    {
        float direction =
            basePosition.x >= 0f ? 1f : -1f;

        return basePosition +
               new Vector2(
                   direction * firstGroupSlideDistance,
                   0f
               );
    }


    private void PlayCannonShoot(
        RectTransform cannonRoot,
        Vector2 basePosition,
        Vector3 baseScale)
    {
        if (!animateCannonShoot ||
            cannonRoot == null)
        {
            return;
        }

        StartCoroutine(
            CannonShootRoutine(
                cannonRoot,
                basePosition,
                baseScale
            )
        );
    }


    /// <summary>
    /// Cannon ka recoil: barrel ke ULAT taraf tez kick, phir narmi se
    /// wapas apni jagah, sath mein halka scale punch.
    ///
    /// Recoil direction cannon ke apne rotated "up" ka ulta hai, is liye
    /// dono tilted cannons (+24.78 aur -24.78) khud-ba-khud mirror ho
    /// jate hain — koi alag setting ki zaroorat nahi.
    /// </summary>
    private IEnumerator CannonShootRoutine(
        RectTransform cannonRoot,
        Vector2 basePosition,
        Vector3 baseScale)
    {
        Vector2 recoilDirection =
            -(Vector2)cannonRoot.up;

        if (recoilDirection.sqrMagnitude < 0.0001f)
        {
            recoilDirection = Vector2.down;
        }

        Vector2 recoilPosition =
            basePosition +
            recoilDirection.normalized * cannonRecoilDistance;

        Vector3 punchScale =
            baseScale * cannonShootPunchScale;

        // Kick back
        float backDuration =
            Mathf.Max(0.01f, cannonRecoilBackDuration);

        float elapsed = 0f;

        while (elapsed < backDuration &&
               cannonRoot != null)
        {
            float progress =
                Mathf.Clamp01(elapsed / backDuration);

            cannonRoot.anchoredPosition =
                Vector2.LerpUnclamped(
                    basePosition,
                    recoilPosition,
                    progress
                );

            cannonRoot.localScale =
                Vector3.LerpUnclamped(
                    baseScale,
                    punchScale,
                    progress
                );

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        // Wapas apni jagah
        float returnDuration =
            Mathf.Max(0.01f, cannonRecoilReturnDuration);

        elapsed = 0f;

        while (elapsed < returnDuration &&
               cannonRoot != null)
        {
            float progress =
                Mathf.Clamp01(elapsed / returnDuration);

            float eased =
                1f - Mathf.Pow(1f - progress, 3f);

            cannonRoot.anchoredPosition =
                Vector2.LerpUnclamped(
                    recoilPosition,
                    basePosition,
                    eased
                );

            cannonRoot.localScale =
                Vector3.LerpUnclamped(
                    punchScale,
                    baseScale,
                    eased
                );

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        if (cannonRoot != null)
        {
            cannonRoot.anchoredPosition = basePosition;
            cannonRoot.localScale = baseScale;
        }
    }


    /// <summary>
    /// Slide aur recoil ke baad sab kuch apni original jagah par.
    /// </summary>
    private void RestoreRootPoses()
    {
        if (!rootsResolved)
        {
            return;
        }

        if (resolvedRootOne != null)
        {
            resolvedRootOne.anchoredPosition = baseAnchoredOne;
        }

        if (resolvedRootTwo != null)
        {
            resolvedRootTwo.anchoredPosition = baseAnchoredTwo;
        }

        if (resolvedRootThree != null)
        {
            resolvedRootThree.anchoredPosition = baseAnchoredThree;
            resolvedRootThree.localScale = baseScaleThree;
        }

        if (resolvedRootFour != null)
        {
            resolvedRootFour.anchoredPosition = baseAnchoredFour;
            resolvedRootFour.localScale = baseScaleFour;
        }
    }


    private void PlayParticle(
        ParticleSystem particle)
    {
        if(particle == null)
        {
            return;
        }

        particle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        ParticleSystem.MainModule main =
            particle.main;

        main.loop = false;

        particle.Play();
    }


    private void EnsureYouWinText()
    {
        if (youWinText != null ||
            particleParent == null)
        {
            return;
        }

        GameObject textObject =
            new GameObject(
                "You Win Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );

        textObject.layer = particleParent.layer;

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.SetParent(
            particleParent.transform,
            false
        );

        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = youWinAnchoredPosition;
        textRect.sizeDelta = new Vector2(900f, 200f);
        textRect.SetAsLastSibling();

        youWinText =
            textObject.GetComponent<TextMeshProUGUI>();

        youWinText.alignment =
            TextAlignmentOptions.Center;

        youWinText.fontStyle = FontStyles.Bold;
        youWinText.fontSize = youWinFontSize;
        youWinText.color = youWinColor;
        youWinText.raycastTarget = false;
        youWinText.textWrappingMode = TextWrappingModes.NoWrap;
    }


    private void ShowYouWinText()
    {
        EnsureYouWinText();

        if (youWinText == null)
        {
            return;
        }

        youWinText.text =
            string.IsNullOrWhiteSpace(youWinMessage)
                ? "YOU WIN"
                : youWinMessage;

        youWinText.fontSize = youWinFontSize;
        youWinText.color = youWinColor;
        youWinText.rectTransform.anchoredPosition =
            youWinAnchoredPosition;

        youWinText.gameObject.SetActive(true);
        youWinText.rectTransform.SetAsLastSibling();
    }


    private void HideYouWinText()
    {
        if (youWinText != null)
        {
            youWinText.gameObject.SetActive(false);
        }
    }


    private void SetYouWinAlpha(
        float alpha)
    {
        if (youWinText == null)
        {
            return;
        }

        youWinText.alpha = Mathf.Clamp01(alpha);
    }


    /// <summary>
    /// YOU WIN text ko apni final jagah par (koi movement nahi, sirf
    /// alpha) 0 se 1 tak fade karta hai.
    /// </summary>
    private IEnumerator FadeInYouWinTextRoutine()
    {
        if (youWinText == null)
        {
            yield break;
        }

        float duration =
            Mathf.Max(0.01f, youWinFadeDuration);

        float elapsed = 0f;

        while (elapsed < duration &&
               youWinText != null)
        {
            float progress =
                Mathf.Clamp01(elapsed / duration);

            SetYouWinAlpha(progress);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        SetYouWinAlpha(1f);
    }


    private static void StopParticle(
        ParticleSystem particle)
    {
        if(particle == null)
        {
            return;
        }

        particle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }


    private static IEnumerator WaitForUnscaledSeconds(
        float duration)
    {
        float elapsed = 0f;

        while(elapsed < Mathf.Max(0f, duration))
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }


    public void StopSequence()
    {
        // A managed reference to a destroyed Unity object can still reach this
        // method. Avoid calling native MonoBehaviour APIs in that state.
        if (this == null)
        {
            return;
        }

        /*
         * Cannon recoil apni alag coroutines mein chalta hai, is liye
         * sirf sequenceRoutine rokna kaafi nahi.
         */
        StopAllCoroutines();

        sequenceRoutine = null;
        sequenceCompleted = null;

        RestoreRootPoses();
        RestoreScreenPose();

        StopParticle(particleOne);
        StopParticle(particleTwo);
        StopParticle(particleThree);
        StopParticle(particleFour);

        HideYouWinText();

        if(particleParent != null)
        {
            particleParent.SetActive(false);
        }
    }


    private void OnDisable()
    {
        StopSequence();
    }
}





