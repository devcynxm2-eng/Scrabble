using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Level browsing ke liye "NEXT >" debug button.
///
/// Level complete kiye baghair seedha agla level load karta hai, taake
/// saare levels tez tezi se dekhe ja sakein. Apna canvas aur button khud
/// runtime par banata hai, is liye scene mein kuch wire karne ki zarurat
/// nahi.
///
/// Default se ye SIRF editor mein nazar aata hai - Show In Build OFF hai,
/// taake ye ghalti se shipped build mein na chala jaye.
/// </summary>
public sealed class LevelSkipDebugButton : MonoBehaviour
{
    [Header("References")]

    [Tooltip(
        "Khali chhod dein to scene se khud dhoond liya jayega."
    )]
    [SerializeField]
    private LevelRuntimeController levelRuntimeController;


    [Header("Behaviour")]

    [Tooltip(
        "OFF (default): button sirf Unity editor mein banta hai. " +
        "ON karne par shipped build mein bhi nazar aayega - ye sirf " +
        "internal testing builds ke liye hai."
    )]
    [SerializeField]
    private bool showInBuild = false;

    [Tooltip(
        "Main Menu par button chhup jayega, taake menu ke ooper na aaye."
    )]
    [SerializeField]
    private bool hideOnMainMenu = true;


    [Header("Appearance")]

    [SerializeField]
    private string buttonLabel = "NEXT >";

    [Tooltip(
        "Screen ke top-right corner se offset. Balls HUD top-left par " +
        "banta hai, is liye default top-right rakha hai."
    )]
    [SerializeField]
    private Vector2 anchoredPosition = new Vector2(-26f, -26f);

    [SerializeField]
    private Vector2 buttonSize = new Vector2(170f, 78f);


    private GameObject buttonRoot;
    private Button skipButton;


    private void Start()
    {
        if (!Application.isEditor &&
            !showInBuild)
        {
            enabled = false;
            return;
        }

        ResolveController();
        BuildButton();
        RefreshVisibility();
    }


    private void OnDestroy()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipToNextLevel);
        }
    }


    private void Update()
    {
        RefreshVisibility();
    }


    private void ResolveController()
    {
        if (levelRuntimeController != null)
        {
            return;
        }

        levelRuntimeController =
            FindFirstObjectByType<LevelRuntimeController>(
                FindObjectsInactive.Include
            );
    }


    /// <summary>
    /// Main Menu par button chhupa deta hai. Gameplay ke dauran (aur
    /// popups ke ooper bhi, kyunke iska canvas sab se ooper hai) nazar
    /// aata rehta hai.
    /// </summary>
    private void RefreshVisibility()
    {
        if (buttonRoot == null)
        {
            return;
        }

        bool shouldShow = true;

        if (hideOnMainMenu &&
            levelRuntimeController != null &&
            levelRuntimeController.IsMainMenuVisible)
        {
            shouldShow = false;
        }

        if (buttonRoot.activeSelf != shouldShow)
        {
            buttonRoot.SetActive(shouldShow);
        }
    }


    /// <summary>
    /// Agla level load karta hai, chahe mojooda level complete hua ho ya
    /// nahi.
    ///
    /// Skip se pehle gameplay ko wapas normal state mein laana zaroori
    /// hai: agar koi popup khula tha to usne timeScale 0 kar rakha hoga
    /// aur PopupGameplayVisibilityController ne tower/cannon chupa rakhe
    /// honge - warna naya level load to ho jata, magar screen par kuch
    /// nazar na aata.
    /// </summary>
    public void SkipToNextLevel()
    {
        ResolveController();

        if (levelRuntimeController == null)
        {
            Debug.LogWarning(
                "LevelSkipDebugButton: LevelRuntimeController nahi mila.",
                this
            );

            return;
        }

        LevelCompleteUIController levelComplete =
            FindFirstObjectByType<LevelCompleteUIController>(
                FindObjectsInactive.Include
            );

        if (levelComplete != null)
        {
            levelComplete.Hide();
        }

        OutOfMovesUIController outOfMoves =
            FindFirstObjectByType<OutOfMovesUIController>(
                FindObjectsInactive.Include
            );

        if (outOfMoves != null)
        {
            outOfMoves.Hide();
        }

        PopupGameplayVisibilityController visibility =
            FindFirstObjectByType<PopupGameplayVisibilityController>(
                FindObjectsInactive.Include
            );

        if (visibility != null)
        {
            visibility.ShowGameplay();
        }

        Time.timeScale = 1f;

        levelRuntimeController.LoadNextLevel();
    }


    private void BuildButton()
    {
        GameObject canvasObject =
            new GameObject(
                "Level Skip Debug Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

        int uiLayer =
            LayerMask.NameToLayer("UI");

        if (uiLayer >= 0)
        {
            canvasObject.layer = uiLayer;
        }

        Canvas canvas =
            canvasObject.GetComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        /*
         * Sab se ooper, taake Level Complete / Pause popups ke dauran bhi
         * button dabaya ja sake.
         */
        canvas.sortingOrder = 32000;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        buttonRoot = canvasObject;


        GameObject buttonObject =
            new GameObject(
                "Next Level Button",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );

        if (uiLayer >= 0)
        {
            buttonObject.layer = uiLayer;
        }

        RectTransform buttonRect =
            buttonObject.GetComponent<RectTransform>();

        buttonRect.SetParent(
            canvasObject.transform,
            false
        );

        buttonRect.anchorMin = Vector2.one;
        buttonRect.anchorMax = Vector2.one;
        buttonRect.pivot = Vector2.one;
        buttonRect.sizeDelta = buttonSize;
        buttonRect.anchoredPosition = anchoredPosition;

        Image background =
            buttonObject.GetComponent<Image>();

        background.color =
            new Color(0.09f, 0.10f, 0.13f, 0.86f);


        GameObject labelObject =
            new GameObject(
                "Label",
                typeof(RectTransform)
            );

        if (uiLayer >= 0)
        {
            labelObject.layer = uiLayer;
        }

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.SetParent(
            buttonRect,
            false
        );

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TMP_Text label =
            labelObject.AddComponent<TextMeshProUGUI>();

        label.text = buttonLabel;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 18f;
        label.fontSizeMax = 40f;
        label.color = Color.white;

        skipButton =
            buttonObject.GetComponent<Button>();

        skipButton.targetGraphic = background;

        skipButton.onClick.AddListener(
            SkipToNextLevel
        );
    }
}
