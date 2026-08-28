using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class NameEntryPanel : OnboardingPanelBase
{
    [Header("Profile")]
    [SerializeField] private PlayerProfile playerProfile;

    [Header("UI References")]
    [SerializeField] private Image avatarPreviewImage;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI charCountText;

    [Header("Config")]
    [SerializeField] private int maxNameLength = 20;
    [SerializeField] private string defaultPlayerName = "Player";

    protected override void OnBeforeShow()
    {
        ShowSelectedAvatarInstant();
        PrepareInputField();
        PrepareConfirmButton();
    }

    protected override void OnPanelShown()
    {
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.RemoveListener(OnNameChanged);
            nameInputField.onValueChanged.AddListener(OnNameChanged);

            nameInputField.ActivateInputField();
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
            confirmButton.onClick.AddListener(OnConfirmClicked);

            confirmButton.transform.DOKill();
            confirmButton.transform.localScale = Vector3.zero;

            confirmButton.transform
                .DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.1f);
        }
    }

    private void PrepareInputField()
    {
        if (nameInputField == null)
            return;

        // Pre-fill with existing name so editing shows current value
        nameInputField.text = playerProfile != null ? playerProfile.PlayerName : "";
        nameInputField.characterLimit = maxNameLength;
        nameInputField.lineType = TMP_InputField.LineType.SingleLine;

        if (nameInputField.placeholder != null)
        {
            TextMeshProUGUI placeholderText =
                nameInputField.placeholder.GetComponent<TextMeshProUGUI>();

            if (placeholderText != null)
                placeholderText.text = defaultPlayerName;
        }

        UpdateCharCount(nameInputField.text.Length);
        SetConfirmInteractable(true);
    }

    private void PrepareConfirmButton()
    {
        if (confirmButton == null)
            return;

        confirmButton.onClick.RemoveListener(OnConfirmClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);

        SetConfirmInteractable(true);
    }

    private void ShowSelectedAvatarInstant()
    {
        if (avatarPreviewImage == null)
            return;

        if (playerProfile == null)
        {
            Debug.LogWarning("PlayerProfile is not assigned in NameEntryPanel.");
            avatarPreviewImage.gameObject.SetActive(false);
            return;
        }

        if (playerProfile.AvatarSprite != null)
        {
            avatarPreviewImage.sprite = playerProfile.AvatarSprite;
            avatarPreviewImage.gameObject.SetActive(true);

            avatarPreviewImage.transform.DOKill();
            avatarPreviewImage.transform.localScale = Vector3.one;
        }
        else
        {
            avatarPreviewImage.gameObject.SetActive(false);
        }
    }

    private void OnNameChanged(string value)
    {
        UpdateCharCount(value.Length);
        SetConfirmInteractable(true);
    }

    private void OnConfirmClicked()
    {
        if (nameInputField == null)
            return;

        string name = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(name))
            name = defaultPlayerName;

        if (confirmButton != null)
            confirmButton.transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 5, 0.5f);

        nameInputField.DeactivateInputField();

        Hide(() =>
        {
            if (ProfileManager.Instance != null)
                ProfileManager.Instance.OnNameConfirmed(name);
            else
                Debug.LogError("ProfileManager.Instance missing.");
        });
    }

    private void UpdateCharCount(int len)
    {
        if (charCountText != null)
            charCountText.text = $"{len} / {maxNameLength}";
    }

    private void SetConfirmInteractable(bool interactable)
    {
        if (confirmButton == null)
            return;

        confirmButton.interactable = interactable;

        CanvasGroup btnGroup = confirmButton.GetComponent<CanvasGroup>();

        if (btnGroup == null)
            btnGroup = confirmButton.gameObject.AddComponent<CanvasGroup>();

        btnGroup.DOKill();
        btnGroup.alpha = interactable ? 1f : 0.4f;
    }

    private void OnDisable()
    {
        if (nameInputField != null)
            nameInputField.onValueChanged.RemoveListener(OnNameChanged);

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
            confirmButton.transform.DOKill();
        }

        if (avatarPreviewImage != null)
            avatarPreviewImage.transform.DOKill();
    }

    private void OnDestroy()
    {
        if (nameInputField != null)
            nameInputField.onValueChanged.RemoveListener(OnNameChanged);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
    }
}
