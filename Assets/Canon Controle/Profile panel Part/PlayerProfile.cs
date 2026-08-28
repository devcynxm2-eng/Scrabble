using UnityEngine;

/// <summary>
/// PlayerProfile — ScriptableObject that stores Name / Age / Avatar.
///
/// AVATAR PERSISTENCE:
///   Sprites cannot be saved to PlayerPrefs. Instead we save the avatar INDEX.
///   The actual sprites live in the 'avatarSprites' array on this asset.
///   On Load(), AvatarSprite is restored from avatarSprites[AvatarIndex].
///
/// SETUP (one time only):
///   1. Select PlayerProfile.asset in your Project window
///   2. In the Inspector you will see "Avatar Sprites" array
///   3. Set the Size to however many avatars you have
///   4. Drag each avatar Sprite into the slots IN THE SAME ORDER
///      as they appear in your AvatarSelectionPanel
///   5. Never reorder this array after launch or saved indexes will be wrong
///
/// READING DATA ANYWHERE:
///   Drag PlayerProfile.asset into any script Inspector field:
///   [SerializeField] private PlayerProfile playerProfile;
///
///   playerProfile.PlayerName    → string
///   playerProfile.Age           → int
///   playerProfile.AvatarSprite  → Sprite (restored from index on Load)
///   playerProfile.AvatarIndex   → int
/// </summary>
[CreateAssetMenu(menuName = "Profile/PlayerProfile", fileName = "PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    // ─── Avatar sprite list (assign in Inspector, never reorder) ──────────────
    [Header("Avatar Sprites — assign once, never reorder")]
    public Sprite[] avatarSprites;

    // ─── Runtime data (not serialized to asset, lives in memory) ─────────────
    [HideInInspector] public Sprite AvatarSprite;
    [HideInInspector] public int    AvatarIndex = -1;
    [HideInInspector] public string PlayerName  = "";
    [HideInInspector] public int    Age         = 0;

    // ─── PlayerPrefs keys ─────────────────────────────────────────────────────
    private const string KEY_NAME   = "profile_name";
    private const string KEY_AGE    = "profile_age";
    private const string KEY_AVATAR = "profile_avatar_index";

    // ─── Save ─────────────────────────────────────────────────────────────────
    public void Save()
    {
        PlayerPrefs.SetString(KEY_NAME,   PlayerName);
        PlayerPrefs.SetInt   (KEY_AGE,    Age);
        PlayerPrefs.SetInt   (KEY_AVATAR, AvatarIndex);
        PlayerPrefs.Save();
    }

    // ─── Load ─────────────────────────────────────────────────────────────────
    public void Load()
    {
        PlayerName  = PlayerPrefs.GetString(KEY_NAME,   "");
        Age         = PlayerPrefs.GetInt   (KEY_AGE,    0);
        AvatarIndex = PlayerPrefs.GetInt   (KEY_AVATAR, -1);

        RestoreAvatarSprite();
    }

    // ─── Restore sprite from saved index ─────────────────────────────────────
    private void RestoreAvatarSprite()
    {
        if (AvatarIndex < 0 || avatarSprites == null || avatarSprites.Length == 0)
        {
            AvatarSprite = null;
            return;
        }

        if (AvatarIndex < avatarSprites.Length)
        {
            AvatarSprite = avatarSprites[AvatarIndex];
        }
        else
        {
            Debug.LogWarning($"[PlayerProfile] AvatarIndex {AvatarIndex} out of range. " +
                              $"Array has {avatarSprites.Length} sprites.");
            AvatarSprite = null;
        }
    }

    // ─── Reset (e.g. "Delete Profile" / "Log out" button) ─────────────────────
    public void ResetProfile()
    {
        PlayerPrefs.DeleteKey(KEY_NAME);
        PlayerPrefs.DeleteKey(KEY_AGE);
        PlayerPrefs.DeleteKey(KEY_AVATAR);
        PlayerPrefs.Save();

        AvatarSprite = null;
        AvatarIndex  = -1;
        PlayerName   = "";
        Age          = 0;
    }
}
