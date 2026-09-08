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
        SetCanonObjectsActive(false);


        slopeBallRoot.SetActive(true);


        isSlopeBallOpen = true;
    }


    /// <summary>
    /// Slope ball game band kar ke Canon ka main menu wapis laata hai.
    /// </summary>
    public void CloseSlopeBallGame()
    {
        if (slopeBallRoot == null ||
            !isSlopeBallOpen)
        {
            return;
        }


        slopeBallRoot.SetActive(false);


        SetCanonObjectsActive(true);


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
