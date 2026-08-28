using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfilePanel : MonoBehaviour
{
    [Header("Player Profile")]
    [SerializeField] private PlayerProfile playerProfile;

    [Header("Display Fields")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI ageText;

    [Header("Edit Buttons")]
    [SerializeField] private Button editAvatarButton;
    [SerializeField] private Button editNameButton;
    [SerializeField] private Button editAgeButton;

    [Header("Edit Panels")]
    [SerializeField] private AvatarSelectionPanel avatarPanel;
    [SerializeField] private NameEntryPanel namePanel;
    [SerializeField] private AgeScrollerPanel agePanel;

    [Header("Parent Panels")]
    [Tooltip("Parent/root object that contains AvatarPanel, NamePanel, AgePanel")]
    [SerializeField] private GameObject editPanelsRoot;

    [Tooltip("Profile panel root. If empty, this GameObject will be used.")]
    [SerializeField] private GameObject profilePanelRoot;

    [Header("Navigation (optional)")]
    [Tooltip("Optional. 'Save' / 'Done' button that closes the profile screen. Note: each field (Name/Avatar/Age) already saves the instant its own Confirm button is pressed — this button just leaves the screen once the user is done editing. Leave unassigned if this screen never needs to be closed (e.g. it's a permanent tab).")]
    [SerializeField] private Button saveAndCloseButton;

    [Tooltip("Optional. GameObject to activate when Save/Close is pressed (e.g. MainMenu, Settings root). Leave empty to just deactivate the profile screen.")]
    [SerializeField] private GameObject nextScreen;

    private void Awake()
    {
        if (profilePanelRoot == null)
            profilePanelRoot = gameObject;
    }

    private void OnEnable()
    {
        if (playerProfile != null)
            playerProfile.Load();

        RefreshDisplay();
        WireButtons();
    }

    /// <summary>Shows the profile view and hides any open edit panel.</summary>
    public void ShowProfile()
    {
        if (editPanelsRoot != null)
            editPanelsRoot.SetActive(false);

        if (profilePanelRoot != null)
            profilePanelRoot.SetActive(true);

        if (playerProfile != null)
            playerProfile.Load();

        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (playerProfile == null)
        {
            Debug.LogError("PlayerProfile missing in ProfilePanel.");
            return;
        }

        if (avatarImage != null)
        {
            avatarImage.sprite = playerProfile.AvatarSprite;
            avatarImage.enabled = playerProfile.AvatarSprite != null;
            avatarImage.preserveAspect = true;
            avatarImage.type = Image.Type.Simple;
        }

        if (nameText != null)
        {
            nameText.text = string.IsNullOrEmpty(playerProfile.PlayerName)
                ? "—"
                : playerProfile.PlayerName;
        }

        if (ageText != null)
        {
            ageText.text = playerProfile.Age > 0
                ? playerProfile.Age.ToString()
                : "—";
        }
    }

    private void WireButtons()
    {
        if (editAvatarButton != null)
        {
            editAvatarButton.onClick.RemoveAllListeners();
            editAvatarButton.onClick.AddListener(EditAvatar);
        }

        if (editNameButton != null)
        {
            editNameButton.onClick.RemoveAllListeners();
            editNameButton.onClick.AddListener(EditName);
        }

        if (editAgeButton != null)
        {
            editAgeButton.onClick.RemoveAllListeners();
            editAgeButton.onClick.AddListener(EditAge);
        }

        if (saveAndCloseButton != null)
        {
            saveAndCloseButton.onClick.RemoveAllListeners();
            saveAndCloseButton.onClick.AddListener(OnSaveAndCloseClicked);
        }
    }

    private void EditAvatar()
    {
        if (avatarPanel == null)
        {
            Debug.LogError("AvatarPanel reference missing.");
            return;
        }

        OpenEditRoot();
        avatarPanel.Show();
    }

    private void EditName()
    {
        if (namePanel == null)
        {
            Debug.LogError("NamePanel reference missing.");
            return;
        }

        OpenEditRoot();
        namePanel.Show();
    }

    private void EditAge()
    {
        if (agePanel == null)
        {
            Debug.LogError("AgePanel reference missing.");
            return;
        }

        OpenEditRoot();
        agePanel.Show();
    }

    private void OpenEditRoot()
    {
        // Important: parent active hona zaroori hai
        if (editPanelsRoot != null)
            editPanelsRoot.SetActive(true);
        else
            Debug.LogError("Edit Panels Root missing. Assign parent of Avatar/Name/Age panels.");

        if (profilePanelRoot != null)
            profilePanelRoot.SetActive(false);
    }

    private void OnSaveAndCloseClicked()
    {
        if (profilePanelRoot != null)
            profilePanelRoot.SetActive(false);

        if (editPanelsRoot != null)
            editPanelsRoot.SetActive(false);

        if (nextScreen != null)
            nextScreen.SetActive(true);
    }
}