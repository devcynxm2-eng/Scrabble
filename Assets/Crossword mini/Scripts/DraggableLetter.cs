using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DraggableLetter : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("References")]
    [SerializeField] private TMP_Text letterText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Placed Tile")]
    [Range(0.5f, 1f)]
    [SerializeField] private float placedSizePercent = 0.84f;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private RectTransform dragLayer;

    private Transform homeParent;
    private int homeSiblingIndex;

    private char letter;
    private bool isPlaced;
    private bool isReturning;

    public char Letter => letter;
    public bool IsPlaced => isPlaced;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (letterText == null)
        {
            letterText =
                GetComponentInChildren<TMP_Text>(true);
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        rootCanvas =
            GetComponentInParent<Canvas>()?.rootCanvas;
    }

    public void Initialize(
        char value,
        RectTransform dragLayerOverride)
    {
        letter = char.ToUpperInvariant(value);

        if (letterText != null)
        {
            letterText.text = letter.ToString();
            letterText.raycastTarget = false;
        }

        homeParent = transform.parent;
        homeSiblingIndex = transform.GetSiblingIndex();

        rootCanvas =
            GetComponentInParent<Canvas>()?.rootCanvas;

        if (dragLayerOverride != null)
        {
            dragLayer = dragLayerOverride;
        }
        else if (rootCanvas != null)
        {
            dragLayer =
                rootCanvas.transform as RectTransform;
        }

        isPlaced = false;
        isReturning = false;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (isPlaced || isReturning)
        {
            return;
        }

        homeParent = transform.parent;
        homeSiblingIndex = transform.GetSiblingIndex();

        if (rootCanvas == null)
        {
            rootCanvas =
                GetComponentInParent<Canvas>()?.rootCanvas;
        }

        if (dragLayer == null && rootCanvas != null)
        {
            dragLayer =
                rootCanvas.transform as RectTransform;
        }

        if (dragLayer == null)
        {
            Debug.LogError(
                "Drag Layer could not be found.",
                this
            );
            return;
        }

        transform.SetParent(
            dragLayer,
            true
        );

        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        UpdateDragPosition(eventData);
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (isPlaced ||
            isReturning ||
            dragLayer == null)
        {
            return;
        }

        UpdateDragPosition(eventData);
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (isPlaced)
        {
            return;
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        ReturnToHome();
    }

    private void UpdateDragPosition(
        PointerEventData eventData)
    {
        Camera eventCamera = null;

        if (rootCanvas != null &&
            rootCanvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = rootCanvas.worldCamera;
        }

        if (RectTransformUtility
            .ScreenPointToWorldPointInRectangle(
                dragLayer,
                eventData.position,
                eventCamera,
                out Vector3 worldPoint))
        {
            rectTransform.position = worldPoint;
        }
    }

    public void SettleInto(
        RectTransform targetCell)
    {
        if (targetCell == null)
        {
            ReturnToHome();
            return;
        }

        isPlaced = true;
        isReturning = false;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.interactable = false;
        }

        transform.SetParent(
            targetCell,
            false
        );

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            Vector2.zero;

        rectTransform.localRotation =
            Quaternion.identity;

        rectTransform.localScale =
            Vector3.one;

        rectTransform.sizeDelta =
            new Vector2(
                targetCell.rect.width *
                placedSizePercent,

                targetCell.rect.height *
                placedSizePercent
            );
    }

    public void ReturnToHome()
    {
        if (isPlaced ||
            isReturning ||
            homeParent == null)
        {
            return;
        }

        isReturning = true;

        transform.SetParent(
            homeParent,
            false
        );

        int safeSiblingIndex =
            Mathf.Clamp(
                homeSiblingIndex,
                0,
                homeParent.childCount - 1
            );

        transform.SetSiblingIndex(
            safeSiblingIndex
        );

        rectTransform.localRotation =
            Quaternion.identity;

        rectTransform.localScale =
            Vector3.one;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (homeParent is RectTransform homeRect)
        {
            Canvas.ForceUpdateCanvases();

            LayoutRebuilder
                .ForceRebuildLayoutImmediate(
                    homeRect
                );
        }

        isReturning = false;
    }
}
