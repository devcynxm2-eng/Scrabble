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
    private const float TransitionDuration = 0.22f;
    private const float HiddenScale = 0.94f;
    private const float HiddenVerticalOffset = -24f;

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

            state.CanvasGroup.blocksRaycasts = false;
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
                    ShownScale = panel.transform.localScale,
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


        private IEnumerator AnimateState(
            TransitionState state,
            bool show)
        {
            float duration = TransitionDuration;

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
                 * Show ka cubic ease-out aur hide ka cubic ease-in
                 * ek dusre ka exact visual reverse hain.
                 */
                float progress = show
                    ? 1f - Mathf.Pow(1f - normalizedTime, 3f)
                    : normalizedTime *
                      normalizedTime *
                      normalizedTime;

                state.CanvasGroup.alpha =
                    Mathf.LerpUnclamped(
                        startAlpha,
                        targetAlpha,
                        progress
                    );

                state.Panel.transform.localScale =
                    Vector3.LerpUnclamped(
                        startScale,
                        targetScale,
                        progress
                    );

                if (state.RectTransform != null)
                {
                    state.RectTransform.anchoredPosition =
                        Vector2.LerpUnclamped(
                            startPosition,
                            targetPosition,
                            progress
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
