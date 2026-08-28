








using UnityEngine;
using DG.Tweening;

public abstract class OnboardingPanelBase : MonoBehaviour
{
    [Header("Panel Animation")]
    [SerializeField] protected float slideInDuration = 0.45f;
    [SerializeField] protected float slideOutDuration = 0.35f;
    [SerializeField] protected Ease slideInEase = Ease.OutCubic;
    [SerializeField] protected Ease slideOutEase = Ease.InCubic;
    [SerializeField] protected float offscreenOffsetX = 1200f;

    protected RectTransform rectTransform;
    protected CanvasGroup canvasGroup;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (!canvasGroup)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetOffscreenRight();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        SetOffscreenRight();

        // IMPORTANT:
        // Data/image/text update panel show hone se pehle
        OnBeforeShow();

        AnimateIn();
    }

    public void Hide(System.Action onComplete = null)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        AnimateOut(onComplete);
    }

    protected virtual void AnimateIn()
    {
        rectTransform.DOKill();
        canvasGroup.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Join(rectTransform.DOAnchorPosX(0f, slideInDuration).SetEase(slideInEase));
        seq.Join(canvasGroup.DOFade(1f, slideInDuration * 0.6f));

        seq.OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            OnPanelShown();
        });
    }

    protected virtual void AnimateOut(System.Action onComplete)
    {
        rectTransform.DOKill();
        canvasGroup.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Join(rectTransform.DOAnchorPosX(-offscreenOffsetX, slideOutDuration).SetEase(slideOutEase));
        seq.Join(canvasGroup.DOFade(0f, slideOutDuration * 0.6f));

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    // Show/animation se pehle call hota hai
    protected virtual void OnBeforeShow() { }

    // Animation complete ke baad call hota hai
    protected virtual void OnPanelShown() { }

    private void SetOffscreenRight()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition =
                new Vector2(offscreenOffsetX, rectTransform.anchoredPosition.y);
        }
    }
}