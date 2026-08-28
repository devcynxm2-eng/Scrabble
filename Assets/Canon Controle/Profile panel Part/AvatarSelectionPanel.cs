using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AvatarSelectionPanel : OnboardingPanelBase
{
    [Header("Confirm")]
    [SerializeField] private Button confirmButton;

    [Header("Highlight Settings")]
    [SerializeField] private string highlightObjectName = "Highlight";
    [SerializeField] private float selectedScale = 1.08f;

    private Button[] avatarButtons;

    private int selectedIndex = -1;
    private Sprite selectedSprite;
    private Button selectedButton;

    protected override void Awake()
    {
        base.Awake();

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = false;
        }
    }

    protected override void OnPanelShown()
    {
        avatarButtons = GetComponentsInChildren<Button>(includeInactive: true);

        int avatarIndex = 0;

        for (int i = 0; i < avatarButtons.Length; i++)
        {
            Button btn = avatarButtons[i];

            if (btn == null || btn == confirmButton)
                continue;

            int capturedIndex = avatarIndex;
            Button capturedBtn = btn;

            Image img = capturedBtn.GetComponent<Image>();

            if (img == null || img.sprite == null)
                img = capturedBtn.GetComponentInChildren<Image>();

            Sprite capturedSprite = img != null ? img.sprite : null;

            capturedBtn.onClick.RemoveAllListeners();
            capturedBtn.onClick.AddListener(() =>
                SelectAvatar(capturedIndex, capturedSprite, capturedBtn));

            capturedBtn.transform.localScale = Vector3.one;

            avatarIndex++;
        }

        ClearSelection();
        DisableAllHighlights();
    }

    private void SelectAvatar(int index, Sprite sprite, Button btn)
    {
        selectedIndex = index;
        selectedSprite = sprite;
        selectedButton = btn;

        if (confirmButton != null)
            confirmButton.interactable = true;

        DisableAllHighlights();
        EnableHighlight(btn);

        ResetButtonScales();

        btn.transform.DOKill();
        btn.transform.DOScale(selectedScale, 0.15f);
        btn.transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 5, 0.5f);

        Debug.Log($"Avatar selected only. Index: {selectedIndex}");
    }

    private void OnConfirmClicked()
    {
        if (selectedIndex < 0 || selectedSprite == null)
        {
            Debug.LogWarning("No avatar selected.");
            return;
        }

        Debug.Log($"Avatar confirmed. Index: {selectedIndex}");

        // Capture before Hide(), since Hide() deactivates the panel
        // and any pending selection state should already be locked in.
        int confirmedIndex = selectedIndex;
        Sprite confirmedSprite = selectedSprite;

        Hide(() =>
        {
            if (ProfileManager.Instance != null)
                ProfileManager.Instance.OnAvatarSelected(confirmedSprite, confirmedIndex);
            else
                Debug.LogError("ProfileManager.Instance missing.");
        });
    }

    private void ClearSelection()
    {
        selectedIndex = -1;
        selectedSprite = null;
        selectedButton = null;

        if (confirmButton != null)
            confirmButton.interactable = false;

        ResetButtonScales();
        DisableAllHighlights();
    }

    private void DisableAllHighlights()
    {
        if (avatarButtons == null)
            return;

        foreach (Button btn in avatarButtons)
        {
            if (btn == null || btn == confirmButton)
                continue;

            Transform highlight = btn.transform.Find(highlightObjectName);

            if (highlight != null)
                highlight.gameObject.SetActive(false);
        }
    }

    private void EnableHighlight(Button btn)
    {
        if (btn == null)
            return;

        Transform highlight = btn.transform.Find(highlightObjectName);

        if (highlight != null)
        {
            highlight.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Highlight object not found inside {btn.name}. Expected child name: {highlightObjectName}");
        }
    }

    private void ResetButtonScales()
    {
        if (avatarButtons == null)
            return;

        foreach (Button btn in avatarButtons)
        {
            if (btn == null || btn == confirmButton)
                continue;

            btn.transform.DOKill();
            btn.transform.DOScale(1f, 0.15f);
        }
    }

    private void OnDisable()
    {
        if (avatarButtons == null)
            return;

        foreach (Button btn in avatarButtons)
        {
            if (btn != null)
                btn.transform.DOKill();
        }
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmClicked);

        if (avatarButtons == null)
            return;

        foreach (Button btn in avatarButtons)
        {
            if (btn != null && btn != confirmButton)
                btn.onClick.RemoveAllListeners();
        }
    }
}
