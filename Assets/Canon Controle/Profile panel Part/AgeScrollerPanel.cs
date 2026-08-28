using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// AgeScrollerPanel — vertical wheel-style age picker.
///
/// SETUP:
///   ScrollRect + Content, an UpButton/DownButton, a SelectedAgeLabel,
///   and a ConfirmButton. Items are generated at runtime in BuildList().
/// </summary>
public class AgeScrollerPanel : OnboardingPanelBase
{
    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private TextMeshProUGUI selectedAgeLabel;
    [SerializeField] private Button confirmButton;

    [Header("Arrow Buttons")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    [Header("Age Range")]
    [SerializeField] private int minAge = 5;
    [SerializeField] private int maxAge = 99;
    [SerializeField] private int defaultAge = 5;

    [Header("Layout")]
    [SerializeField] private float itemHeight = 58f;
    [SerializeField] private float itemSpacing = 0f;
    [SerializeField] private int fontSize = 46;

    [Header("Center Highlight")]
    [SerializeField] private GameObject centerHighlight;

    [Header("Visuals")]
    [SerializeField] private float centerScale = 1.15f;
    [SerializeField] private float nearScale = 0.85f;
    [SerializeField] private float farScale = 0.65f;

    [SerializeField] private float centerAlpha = 1f;
    [SerializeField] private float nearAlpha = 0.55f;
    [SerializeField] private float farAlpha = 0.18f;

    [SerializeField] private Color centerColor = new Color(1f, 0.45f, 0f);
    [SerializeField] private Color nearColor = new Color(0.30f, 0.12f, 0.05f);
    [SerializeField] private Color farColor = new Color(0.65f, 0.65f, 0.65f);

    [Header("Snap")]
    [SerializeField] private float snapDuration = 0.20f;
    [SerializeField] private Ease snapEase = Ease.OutCubic;

    private readonly List<RectTransform> items = new List<RectTransform>();
    private readonly List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();
    private readonly List<CanvasGroup> groups = new List<CanvasGroup>();

    private int selectedIndex;
    private bool isSnapping;

    private void OnValidate()
    {
        ValidateAgeSettings();
    }

    protected override void OnPanelShown()
    {
        ValidateAgeSettings();

        if (scrollRect != null)
        {
            scrollRect.vertical = false;
            scrollRect.horizontal = false;
            scrollRect.inertia = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
        }

        BuildList();
        JumpToAge(defaultAge);

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        if (upButton != null)
        {
            upButton.onClick.RemoveAllListeners();
            upButton.onClick.AddListener(OnTopButtonClicked);
        }

        if (downButton != null)
        {
            downButton.onClick.RemoveAllListeners();
            downButton.onClick.AddListener(OnBottomButtonClicked);
        }

        RefreshButtonStates();

        if (centerHighlight != null)
            centerHighlight.SetActive(true);
    }

    private void ValidateAgeSettings()
    {
        if (minAge < 0)
            minAge = 0;

        if (maxAge < minAge)
            maxAge = minAge;

        defaultAge = Mathf.Clamp(defaultAge, minAge, maxAge);

        if (itemHeight < 1f)
            itemHeight = 1f;

        if (itemSpacing < 0f)
            itemSpacing = 0f;

        if (fontSize < 1)
            fontSize = 1;
    }

    private float GetItemStep()
    {
        return itemHeight + itemSpacing;
    }

    private void BuildList()
    {
        if (content == null)
        {
            Debug.LogError("AgeScrollerPanel: Content is not assigned.");
            return;
        }

        foreach (Transform child in content)
            Destroy(child.gameObject);

        items.Clear();
        labels.Clear();
        groups.Clear();

        int count = Mathf.Max(1, maxAge - minAge + 1);

        content.anchorMin = new Vector2(0.5f, 1f);
        content.anchorMax = new Vector2(0.5f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;

        float contentWidth = content.rect.width;

        if (contentWidth <= 0)
            contentWidth = 250f;

        float itemStep = GetItemStep();
        float contentHeight =
            count * itemHeight +
            Mathf.Max(0, count - 1) * itemSpacing;

        content.sizeDelta = new Vector2(contentWidth, contentHeight);

        for (int i = 0; i < count; i++)
        {
            int age = minAge + i;

            GameObject itemGO = new GameObject($"Age_{age}");
            itemGO.transform.SetParent(content, false);

            RectTransform itemRT = itemGO.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0.5f, 1f);
            itemRT.anchorMax = new Vector2(0.5f, 1f);
            itemRT.pivot = new Vector2(0.5f, 0.5f);
            itemRT.sizeDelta = new Vector2(contentWidth, itemHeight);

            itemRT.anchoredPosition = new Vector2(
                0f,
                -(i * itemStep + itemHeight * 0.5f)
            );

            CanvasGroup canvasGroup = itemGO.AddComponent<CanvasGroup>();

            GameObject textGO = new GameObject("Label");
            textGO.transform.SetParent(itemGO.transform, false);

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = age.ToString();
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = farColor;
            text.fontStyle = FontStyles.Bold;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;

            items.Add(itemRT);
            labels.Add(text);
            groups.Add(canvasGroup);
        }
    }

    private void OnTopButtonClicked()
    {
        if (isSnapping || items.Count == 0)
            return;

        if (!CanGoUp())
        {
            RefreshButtonStates();
            return;
        }

        MoveToPreviousAge();
    }

    private void OnBottomButtonClicked()
    {
        if (isSnapping || items.Count == 0)
            return;

        if (!CanGoDown())
        {
            RefreshButtonStates();
            return;
        }

        MoveToNextAge();
    }

    private bool CanGoUp()
    {
        return selectedIndex > 0;
    }

    private bool CanGoDown()
    {
        return selectedIndex < items.Count - 1;
    }

    private void MoveToPreviousAge()
    {
        if (isSnapping || items.Count == 0)
            return;

        if (!CanGoUp())
        {
            RefreshButtonStates();
            return;
        }

        SnapToIndex(selectedIndex - 1);
    }

    private void MoveToNextAge()
    {
        if (isSnapping || items.Count == 0)
            return;

        if (!CanGoDown())
        {
            RefreshButtonStates();
            return;
        }

        SnapToIndex(selectedIndex + 1);
    }

    private void JumpToAge(int age)
    {
        if (items.Count == 0)
            return;

        selectedIndex = Mathf.Clamp(age - minAge, 0, items.Count - 1);

        float targetY = GetTargetY(selectedIndex);
        content.anchoredPosition = new Vector2(0f, targetY);

        UpdateLabel();
        RefreshVisuals();
        RefreshButtonStates();
    }

    private void SnapToIndex(int index)
    {
        if (items.Count == 0)
            return;

        int clampedIndex = Mathf.Clamp(index, 0, items.Count - 1);

        if (clampedIndex == selectedIndex)
        {
            RefreshButtonStates();
            return;
        }

        selectedIndex = clampedIndex;

        float targetY = GetTargetY(selectedIndex);

        isSnapping = true;

        content.DOKill();

        content.DOAnchorPosY(targetY, snapDuration)
            .SetEase(snapEase)
            .OnUpdate(RefreshVisuals)
            .OnComplete(() =>
            {
                isSnapping = false;
                RefreshVisuals();
                UpdateLabel();
                RefreshButtonStates();
            });
    }

    private float GetTargetY(int index)
    {
        RectTransform viewport = scrollRect != null ? scrollRect.viewport : null;

        if (viewport == null)
        {
            Debug.LogWarning("AgeScrollerPanel: ScrollRect viewport missing.");
            return index * GetItemStep();
        }

        float viewportHalf = viewport.rect.height * 0.5f;
        float itemStep = GetItemStep();

        float selectedItemCenterY = index * itemStep + itemHeight * 0.5f;
        float targetY = selectedItemCenterY - viewportHalf;

        float maxY = Mathf.Max(0f, content.rect.height - viewport.rect.height);

        return Mathf.Clamp(targetY, 0f, maxY);
    }

    private void RefreshVisuals()
    {
        RectTransform viewport = scrollRect != null ? scrollRect.viewport : null;

        if (viewport == null || items.Count == 0)
            return;

        float viewportHalf = viewport.rect.height * 0.5f;
        float itemStep = GetItemStep();

        float centerIndex =
            (content.anchoredPosition.y + viewportHalf - itemHeight * 0.5f) / itemStep;

        int roundedCenterIndex = Mathf.RoundToInt(centerIndex);

        for (int i = 0; i < items.Count; i++)
        {
            float dist = Mathf.Abs(i - centerIndex);

            if (dist > 2.5f)
            {
                groups[i].alpha = 0f;
                items[i].localScale = Vector3.one * farScale;
                labels[i].color = farColor;
                continue;
            }

            bool isCenter = roundedCenterIndex == i;

            if (isCenter)
            {
                groups[i].alpha = centerAlpha;
                items[i].localScale = Vector3.one * centerScale;
                labels[i].color = centerColor;
                labels[i].fontStyle = FontStyles.Bold;
            }
            else if (dist <= 1.5f)
            {
                groups[i].alpha = nearAlpha;
                items[i].localScale = Vector3.one * nearScale;
                labels[i].color = nearColor;
                labels[i].fontStyle = FontStyles.Bold;
            }
            else
            {
                groups[i].alpha = farAlpha;
                items[i].localScale = Vector3.one * farScale;
                labels[i].color = farColor;
                labels[i].fontStyle = FontStyles.Bold;
            }
        }
    }

    private void UpdateLabel()
    {
        int selectedAge = minAge + selectedIndex;

        if (selectedAgeLabel != null)
            selectedAgeLabel.text = selectedAge.ToString();
    }

    private void RefreshButtonStates()
    {
        SetButtonState(upButton, CanGoUp());
        SetButtonState(downButton, CanGoDown());
    }

    private void SetButtonState(Button button, bool active)
    {
        if (button == null)
            return;

        button.interactable = active;

        CanvasGroup group = button.GetComponent<CanvasGroup>();

        if (group == null)
            group = button.gameObject.AddComponent<CanvasGroup>();

        group.interactable = active;
        group.blocksRaycasts = active;
        group.alpha = active ? 1f : 0.45f;
    }

    private void OnConfirmClicked()
    {
        if (isSnapping)
            return;

        int confirmedAge = minAge + selectedIndex;

        Debug.Log($"Confirmed Age: {confirmedAge}");

        Hide(() =>
        {
            if (ProfileManager.Instance != null)
                ProfileManager.Instance.OnAgeConfirmed(confirmedAge);
            else
                Debug.LogError("ProfileManager.Instance missing.");
        });
    }

    private void OnDisable()
    {
        if (content != null)
            content.DOKill();
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmClicked);

        if (upButton != null)
            upButton.onClick.RemoveListener(OnTopButtonClicked);

        if (downButton != null)
            downButton.onClick.RemoveListener(OnBottomButtonClicked);
    }
}