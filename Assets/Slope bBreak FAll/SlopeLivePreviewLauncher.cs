#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Live Preview ka wo hissa jo Play Mode mein jaate hi chalta hai.
///
/// START LIVE GAME PREVIEW dabane par editor sirf Play Mode start
/// karta hai. Play Mode ka domain reload editor ke saare non-static
/// state ko udaa deta hai, is liye pehla refresh koi trigger nahi
/// karta tha aur user ko Canon ka main menu hi nazar aata reh jata.
///
/// Ye class SessionState (jo domain reload survive karti hai) se
/// yaad rakhti hai ke kaunsa level preview karna tha, aur Play Mode
/// shuru hote hi slope game khol kar tower bana deti hai.
/// </summary>
[InitializeOnLoad]
public static class SlopeLivePreviewLauncher
{
    public const string EnabledKey =
        "SlopeBall.LivePreviewEnabled";

    public const string LaunchKey =
        "SlopeBall.LivePreviewLaunch";

    public const string LevelGuidKey =
        "SlopeBall.LivePreviewLevelGuid";


    static SlopeLivePreviewLauncher()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }


    /// <summary>
    /// Preview shuru karne se pehle yaad rakhta hai ke kaunsa level
    /// dikhana hai, taake domain reload ke baad wapis mil jaye.
    /// </summary>
    public static void RememberLevel(
        SlopeLevelData level)
    {
        if (level == null)
        {
            return;
        }


        string path = AssetDatabase.GetAssetPath(level);

        SessionState.SetString(
            LevelGuidKey,
            AssetDatabase.AssetPathToGUID(path)
        );
    }


    public static SlopeLevelData GetRememberedLevel()
    {
        string guid =
            SessionState.GetString(LevelGuidKey, string.Empty);

        if (string.IsNullOrEmpty(guid))
        {
            return null;
        }


        string path = AssetDatabase.GUIDToAssetPath(guid);

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }


        return AssetDatabase.LoadAssetAtPath<SlopeLevelData>(path);
    }


    private static void OnPlayModeChanged(
        PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode)
        {
            return;
        }


        if (!SessionState.GetBool(LaunchKey, false))
        {
            return;
        }


        /*
         * Launch flag sirf ek baar chalna chahiye. Enabled flag
         * chalta rehta hai taake baad ke design changes bhi
         * Game View refresh karte rahein.
         */
        SessionState.SetBool(LaunchKey, false);


        /*
         * Ek frame ka intezar zaroori hai. EnteredPlayMode par
         * scene ke Awake/Start abhi chale nahi hote, aur tower ko
         * un ke baad hi banana hai.
         */
        EditorApplication.delayCall += () =>
        {
            EditorApplication.delayCall += ApplyRememberedLevel;
        };
    }


    private static void ApplyRememberedLevel()
    {
        if (!Application.isPlaying ||
            !SessionState.GetBool(EnabledKey, false))
        {
            return;
        }


        SlopeLevelData level = GetRememberedLevel();

        if (level == null)
        {
            Debug.LogWarning(
                "Slope live preview: level asset nahi mila."
            );

            return;
        }


        RefreshNow(level);
    }


    /// <summary>
    /// Slope game khol kar diye gaye level ka tower bana deta hai.
    /// Editor ka REFRESH GAME VIEW bhi yahi call karta hai.
    /// </summary>
    public static void RefreshNow(
        SlopeLevelData level)
    {
        if (!Application.isPlaying ||
            level == null)
        {
            return;
        }


        /*
         * Slope game band ho to pehle usay kholte hain, warna tower
         * inactive root ke andar banega aur kuch nazar nahi aayega.
         */
        SlopeBallGameButton switcher =
            Object.FindFirstObjectByType<SlopeBallGameButton>(
                FindObjectsInactive.Include
            );

        if (switcher != null &&
            !switcher.IsSlopeBallOpen)
        {
            switcher.OpenSlopeBallGame();
        }


        SlopeLevelLoader loader =
            Object.FindFirstObjectByType<SlopeLevelLoader>(
                FindObjectsInactive.Include
            );

        if (loader == null)
        {
            Debug.LogWarning(
                "Slope live preview: scene mein SlopeLevelLoader " +
                "nahi mila."
            );

            return;
        }


        loader.RebuildWithLevel(level);
    }
}
#endif
