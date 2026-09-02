using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventBroker
{
    public static event Action<UIScreenType> OnScreenChangeRequested;


    public static void RequestScreen(UIScreenType Screen_Type)
    {
        OnScreenChangeRequested?.Invoke(Screen_Type);
    }



}


/// <summary>
/// Shared runtime transitions for every UI screen and popup. A single
/// always-active runner keeps close animations alive even when gameplay is
/// paused and prevents competing coroutines during rapid button presses.
/// </summary>
public static class UITransition
{
    /*
     * Show jaan boojh kar Hide se lamba hai: EaseOutBack ka overshoot
     * tab hi mehsoos hota hai jab usay saans lene ki jagah mile. Close
     * chhota rakha hai taake popup band karna sust na lage.
     */
    private const float ShowDuration = 0.34f;
    private const float HideDuration = 0.16f;

    /*
     * Panel itni chhoti scale se "pop" karta hai. 1 ke jitna qareeb,
     * effect utna halka. 0.94 par animation itni halki thi ke Level
     * Complete par nazar hi nahi aati thi.
     */
    private const float HiddenScale = 0.80f;

    private const float HiddenVerticalOffset = -46f;

    /*
     * Alpha show ke sirf itne hisse mein poora ho jata hai. Jab tak
     * scale settle hoti hai panel already solid dikh raha hota hai -
     * is se full-screen panels ke kinare chhoti scale par nazar nahi
     * aate, aur popup dhundhla bhi mehsoos nahi hota.
     */
    private const float ShowFadePortion = 0.45f;

    private static UITransitionRunner runner;


    public static void Show(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        GetRunner().SetVisible(
            panel,
            true,
            false
        );
    }


    public static void Hide(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        GetRunner().SetVisible(
            panel,
            false,
            false
        );
    }


    public static void HideImmediate(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        GetRunner().SetVisible(
            panel,
            false,
            true
        );
    }


    private static UITransitionRunner GetRunner()
    {
        if (runner != null)
        {
            return runner;
        }

        GameObject runnerObject =
            new GameObject("UI Transition Runner");

        UnityEngine.Object.DontDestroyOnLoad(runnerObject);

        runner =
            runnerObject.AddComponent<UITransitionRunner>();

        return runner;
    }


    private sealed class UITransitionRunner : MonoBehaviour
    {
        private sealed class TransitionState
        {
            public GameObject Panel;
            public RectTransform RectTransform;
            public CanvasGroup CanvasGroup;
            public Vector3 ShownScale;
            public Vector2 ShownPosition;
            public float ShownAlpha;
            public Coroutine Routine;
            public bool IntendedVisible;
        }


        private readonly Dictionary<int, TransitionState> states =
            new Dictionary<int, TransitionState>();


        public void SetVisible(
            GameObject panel,
            bool visible,
            bool immediate)
        {
            TransitionState state = GetOrCreateState(panel);

            if (state == null)
            {
                return;
            }

            if (!immediate &&
                state.IntendedVisible == visible &&
                (
                    (visible && panel.activeSelf) ||
                    (!visible &&
                     (!panel.activeSelf || state.Routine != null))
                ))
            {
                return;
            }

            state.IntendedVisible = visible;

            if (state.Routine != null)
            {
                StopCoroutine(state.Routine);
                state.Routine = null;
            }

            if (immediate)
            {
                ApplyImmediateState(state, visible);
                return;
            }

            if (visible)
            {
                bool wasInactive = !panel.activeSelf;

                panel.SetActive(true);

                if (wasInactive)
                {
                    ApplyHiddenVisuals(state);
                }
            }
            else if (!panel.activeSelf)
            {
                ApplyImmediateState(state, false);
                return;
            }

            /*
             * Animation ke dauran panel KHUD raycasts block karta hai,
             * taake tap uske neeche wali screen tak na pohanche. Home ke
             * popups (Settings / Daily Reward) khulte waqt Main Menu jaan
             * boojh kar active rehta hai (UIBaseScreen.IsHomePopup), is
             * liye pehle in 0.22s mein tap seedha Play button par lag
             * jata tha.
             *
             * Steady state mein bhi shown panel blocksRaycasts = true
             * hi rehta hai, is liye ye sirf transition window ko usi
             * state ke sath match karta hai - koi naya behaviour nahi.
             *
             * interactable OFF rehta hai, is liye panel ke apne buttons
             * animation mukammal hone se pehle press nahi ho sakte.
             */
            state.CanvasGroup.blocksRaycasts = true;
            state.CanvasGroup.interactable = false;

            state.Routine = StartCoroutine(
                AnimateState(state, visible)
            );
        }


        private TransitionState GetOrCreateState(GameObject panel)
        {
            int instanceId = panel.GetInstanceID();

            if (states.TryGetValue(
                    instanceId,
                    out TransitionState existing) &&
                existing != null &&
                existing.Panel != null)
            {
                return existing;
            }

            CanvasGroup canvasGroup =
                panel.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }

            RectTransform rectTransform =
                panel.transform as RectTransform;

            TransitionState state =
                new TransitionState
                {
                    Panel = panel,
                    RectTransform = rectTransform,
                    CanvasGroup = canvasGroup,
                    ShownScale =
                        SanitizeShownScale(panel.transform.localScale),
                    ShownPosition =
                        rectTransform != null
                            ? rectTransform.anchoredPosition
                            : Vector2.zero,
                    ShownAlpha = Mathf.Approximately(canvasGroup.alpha, 0f)
                        ? 1f
                        : canvasGroup.alpha,
                    IntendedVisible = panel.activeSelf
                };

            states[instanceId] = state;
            return state;
        }


        /// <summary>
        /// Panel ki authored "shown" scale. Jis tarah designers panel ko
        /// alpha 0 se hide karte hain, usi tarah scale 0 se bhi karte
        /// hain - aur us soorat mein 0 ko shown pose maan lena panel ko
        /// hamesha ke liye invisible kar deta. Ye ShownAlpha wale guard
        /// ka exact mirror hai.
        /// </summary>
        private static Vector3 SanitizeShownScale(
            Vector3 scale)
        {
            return new Vector3(
                Mathf.Approximately(scale.x, 0f) ? 1f : scale.x,
                Mathf.Approximately(scale.y, 0f) ? 1f : scale.y,
                Mathf.Approximately(scale.z, 0f) ? 1f : scale.z
            );
        }


        private static void ApplyImmediateState(
            TransitionState state,
            bool visible)
        {
            if (visible)
            {
                state.Panel.SetActive(true);
                state.CanvasGroup.alpha = state.ShownAlpha;
                state.Panel.transform.localScale = state.ShownScale;

                if (state.RectTransform != null)
                {
                    state.RectTransform.anchoredPosition =
                        state.ShownPosition;
                }

                state.CanvasGroup.blocksRaycasts = true;
                state.CanvasGroup.interactable = true;
                return;
            }

            ApplyHiddenVisuals(state);
            state.Panel.SetActive(false);
        }


        private static void ApplyHiddenVisuals(
            TransitionState state)
        {
            state.CanvasGroup.alpha = 0f;
            state.CanvasGroup.blocksRaycasts = false;
            state.CanvasGroup.interactable = false;
            state.Panel.transform.localScale =
                state.ShownScale * HiddenScale;

            if (state.RectTransform != null)
            {
                state.RectTransform.anchoredPosition =
                    state.ShownPosition +
                    Vector2.up * HiddenVerticalOffset;
            }
        }


        /// <summary>
        /// Apni jagah se thora aage nikal kar wapas settle hone wali
        /// curve - yehi "pop" ka ehsaas deti hai. Wahi formula jo
        /// LevelRuntimeController blocks ki entrance par use karta hai,
        /// taake UI aur gameplay ka feel ek jaisa rahe.
        /// </summary>
        private static float EaseOutBack(float value)
        {
            const float overshoot = 1.70158f;

            float shiftedValue = value - 1f;

            return
                1f +
                (overshoot + 1f) *
                shiftedValue * shiftedValue * shiftedValue +
                overshoot * shiftedValue * shiftedValue;
        }


        private static float EaseOutCubic(float value)
        {
            return 1f - Mathf.Pow(1f - value, 3f);
        }


        private IEnumerator AnimateState(
            TransitionState state,
            bool show)
        {
            float duration =
                show
                    ? ShowDuration
                    : HideDuration;

            float startAlpha = state.CanvasGroup.alpha;
            float targetAlpha = show ? state.ShownAlpha : 0f;
            Vector3 startScale = state.Panel.transform.localScale;
            Vector3 targetScale =
                show
                    ? state.ShownScale
                    : state.ShownScale * HiddenScale;

            Vector2 startPosition =
                state.RectTransform != null
                    ? state.RectTransform.anchoredPosition
                    : Vector2.zero;

            Vector2 targetPosition =
                show
                    ? state.ShownPosition
                    : state.ShownPosition +
                      Vector2.up * HiddenVerticalOffset;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (state.Panel == null ||
                    state.IntendedVisible != show)
                {
                    yield break;
                }

                float normalizedTime =
                    Mathf.Clamp01(elapsed / duration);

                /*
                 * Show: teen alag curves, ek sath chalti hui.
                 *   scale    - EaseOutBack, thora aage ja kar settle (pop)
                 *   position - cubic ease-out, seedha upar
                 *   alpha    - sab se pehle poora, taake panel kabhi
                 *              dhundhla na lage
                 *
                 * Hide: teenon ek sath cubic ease-in - tez aur saaf.
                 */
                float scaleProgress;
                float positionProgress;
                float alphaProgress;

                if (show)
                {
                    scaleProgress =
                        EaseOutBack(normalizedTime);

                    positionProgress =
                        EaseOutCubic(normalizedTime);

                    alphaProgress =
                        EaseOutCubic(
                            Mathf.Clamp01(
                                normalizedTime / ShowFadePortion
                            )
                        );
                }
                else
                {
                    float easeIn =
                        normalizedTime *
                        normalizedTime *
                        normalizedTime;

                    scaleProgress = easeIn;
                    positionProgress = easeIn;
                    alphaProgress = easeIn;
                }

                state.CanvasGroup.alpha =
                    Mathf.LerpUnclamped(
                        startAlpha,
                        targetAlpha,
                        alphaProgress
                    );

                state.Panel.transform.localScale =
                    Vector3.LerpUnclamped(
                        startScale,
                        targetScale,
                        scaleProgress
                    );

                if (state.RectTransform != null)
                {
                    state.RectTransform.anchoredPosition =
                        Vector2.LerpUnclamped(
                            startPosition,
                            targetPosition,
                            positionProgress
                        );
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            state.CanvasGroup.alpha = targetAlpha;
            state.Panel.transform.localScale = targetScale;

            if (state.RectTransform != null)
            {
                state.RectTransform.anchoredPosition = targetPosition;
            }

            state.Routine = null;

            if (show)
            {
                state.CanvasGroup.blocksRaycasts = true;
                state.CanvasGroup.interactable = true;
            }
            else
            {
                state.Panel.SetActive(false);
            }
        }
    }
}
