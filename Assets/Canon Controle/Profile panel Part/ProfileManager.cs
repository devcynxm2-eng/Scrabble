using UnityEngine;

/// <summary>
/// ProfileManager
///
/// Central point that AvatarSelectionPanel, NameEntryPanel and
/// AgeScrollerPanel report back to after the user confirms an edit.
/// No onboarding wizard involved anywhere — panels are opened directly
/// from ProfilePanel's Edit buttons, and each panel hides itself before
/// calling back here.
///
/// SETUP:
///   1. Put this on a persistent GameObject in your Profile scene
///      (e.g. the same object as ProfilePanel, or a dedicated
///      "ProfileManager" object).
///   2. Assign PlayerProfile.asset and the ProfilePanel reference.
/// </summary>
public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }

    [Header("Profile")]
    [SerializeField] private PlayerProfile playerProfile;

    [Header("Profile Panel")]
    [SerializeField] private ProfilePanel profilePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnAvatarSelected(Sprite avatar, int index)
    {
        if (playerProfile == null)
        {
            Debug.LogError("ProfileManager: PlayerProfile not assigned.");
            return;
        }

        playerProfile.AvatarSprite = avatar;
        playerProfile.AvatarIndex = index;

        FinishEdit();
    }

    public void OnNameConfirmed(string playerName)
    {
        if (playerProfile == null)
        {
            Debug.LogError("ProfileManager: PlayerProfile not assigned.");
            return;
        }

        playerProfile.PlayerName = playerName;

        FinishEdit();
    }

    public void OnAgeConfirmed(int age)
    {
        if (playerProfile == null)
        {
            Debug.LogError("ProfileManager: PlayerProfile not assigned.");
            return;
        }

        playerProfile.Age = age;

        FinishEdit();
    }

    private void FinishEdit()
    {
        playerProfile.Save();

        if (profilePanel != null)
        {
            profilePanel.ShowProfile();
            profilePanel.RefreshDisplay();
        }
        else
        {
            Debug.LogError("ProfileManager: ProfilePanel reference missing.");
        }
    }
}
