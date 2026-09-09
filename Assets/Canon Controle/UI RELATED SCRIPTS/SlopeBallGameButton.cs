using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slope / Ball Fall Break game ko usi Canon_Controller scene ke
/// andar on aur off karta hai. Koi doosri scene load nahi hoti.
///
/// Ye component kisi aise object par hona chahiye jo hamesha active
/// rahe (jaise Managers), kyunke Canon ka Canvas band hone par
/// button khud inactive ho jata hai.
/// </summary>
public sealed class SlopeBallGameButton : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playSlopeBallButton;

    [Tooltip("Slope game ke andar wala back button. Optional hai.")]
    [SerializeField] private Button backToMenuButton;


    [Header("Slope Ball Game")]
    [Tooltip(
        "Slope ball game ka root object. Ye default par band rehta " +
        "hai aur button dabane par on hota hai."
    )]
    [SerializeField] private GameObject slopeBallRoot;
    [SerializeField] private GameObject homeScreen;
    private GameObject slopeHud;
    private bool[] canonActiveBeforeOpen;


    [Header("Canon Side")]
    [Tooltip(
        "Slope game chalne ke doran ye Canon objects band ho jayenge " +
        "aur wapis aane par dobara on ho jayenge."
    )]
    [SerializeField] private GameObject[] canonObjects;


    private bool isSlopeBallOpen;


    public bool IsSlopeBallOpen =>
        isSlopeBallOpen;


    private void Start()
    {
        CreateBackButtonIfNeeded();
        if (playSlopeBallButton != null)
        {
            playSlopeBallButton.onClick.RemoveListener(
                OpenSlopeBallGame
            );

            playSlopeBallButton.onClick.AddListener(
                OpenSlopeBallGame
            );
        }


        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveListener(
                CloseSlopeBallGame
            );

            backToMenuButton.onClick.AddListener(
                CloseSlopeBallGame
            );
        }


        /*
         * Scene start par slope game hamesha band hona chahiye,
         * chahe kisi ne editor mein galti se on chhod diya ho.
         */
        if (slopeBallRoot != null)
        {
            slopeBallRoot.SetActive(false);
        }
    }


    private void OnDestroy()
    {
        if (slopeHud != null) Destroy(slopeHud);
        if (playSlopeBallButton != null)
        {
            playSlopeBallButton.onClick.RemoveListener(
                OpenSlopeBallGame
            );
        }


        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveListener(
                CloseSlopeBallGame
            );
        }
    }

    public void CreateBackButtonIfNeeded()
    {
        if (backToMenuButton != null || slopeBallRoot == null)
            return;

        var hud = new GameObject("Slope HUD", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        slopeHud = hud;
        hud.transform.SetParent(slopeBallRoot.transform, false);
        var canvas = hud.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        var scaler = hud.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var buttonObject = new GameObject("Back to Main Menu", typeof(RectTransform),
            typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(hud.transform, false);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchorMin = rect.anchorMax = new Vector2(
            Screen.safeArea.xMin / Screen.width, Screen.safeArea.yMax / Screen.height);
        rect.anchoredPosition = new Vector2(24, -24);
        rect.sizeDelta = new Vector2(230, 88);
        buttonObject.GetComponent<Image>().color = new Color(0.04f, 0.15f, 0.22f, 0.94f);
        backToMenuButton = buttonObject.GetComponent<Button>();
        backToMenuButton.targetGraphic = buttonObject.GetComponent<Image>();

        var label = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        label.transform.SetParent(buttonObject.transform, false);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        var text = label.GetComponent<TMPro.TextMeshProUGUI>();
        text.text = "< BACK";
        text.font = TMPro.TMP_Settings.defaultFontAsset;
        text.color = Color.white;
        text.fontSize = 34;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.raycastTarget = false;
        backToMenuButton.onClick.AddListener(CloseSlopeBallGame);
        hud.SetActive(true);
    }

    private void LateUpdate()
    {
        // The HUD belongs to the slope root, so it hides with that screen.
    }


    /// <summary>
    /// Canon ka setup chhupa kar slope ball game on karta hai.
    /// </summary>
    public void OpenSlopeBallGame()
    {
        if (slopeBallRoot == null)
        {
            Debug.LogWarning(
                "SlopeBallGameButton: slopeBallRoot not assigned."
            );

            return;
        }


        if (isSlopeBallOpen)
        {
            return;
        }


        /*
         * Canon pause ke doran timeScale 0 kar deta hai, is liye
         * slope game shuru karne se pehle normal speed wapis.
         */
        Time.timeScale = 1f;


        /*
         * Pehle Canon ka camera band hota hai, phir slope root on.
         * Is tarah BallDropController ke Start() mein Camera.main
         * slope wala camera hi pick karega.
         */
        canonActiveBeforeOpen = new bool[canonObjects == null ? 0 : canonObjects.Length];
        for (int i = 0; i < canonActiveBeforeOpen.Length; i++)
            canonActiveBeforeOpen[i] = canonObjects[i] != null && canonObjects[i].activeSelf;
        SetCanonObjectsActive(false);
        if (homeScreen != null) homeScreen.SetActive(false);


        slopeBallRoot.SetActive(true);
        CreateBackButtonIfNeeded();
        if (slopeHud != null) slopeHud.SetActive(true);


        isSlopeBallOpen = true;
    }


    /// <summary>
    /// Slope ball game band kar ke Canon ka main menu wapis laata hai.
    /// </summary>
    public void CloseSlopeBallGame()
    {
        if (slopeBallRoot == null)
        {
            return;
        }


        foreach (var ball in slopeBallRoot.GetComponentsInChildren<BallDropController>(true))
            ball.ClearReleasedBalls();
        Time.timeScale = 1f;
        slopeBallRoot.SetActive(false);


        SetCanonObjectsActive(true);
        if (canonActiveBeforeOpen != null)
            for (int i = 0; i < canonActiveBeforeOpen.Length; i++)
                if (canonObjects[i] != null) canonObjects[i].SetActive(canonActiveBeforeOpen[i]);
        if (homeScreen != null) homeScreen.SetActive(true);
        if (slopeHud != null) slopeHud.SetActive(false);


        isSlopeBallOpen = false;
    }


    private void SetCanonObjectsActive(
        bool isActive)
    {
        if (canonObjects == null)
        {
            return;
        }


        for (int i = 0; i < canonObjects.Length; i++)
        {
            if (canonObjects[i] == null)
            {
                continue;
            }


            canonObjects[i].SetActive(isActive);
        }
    }
}
