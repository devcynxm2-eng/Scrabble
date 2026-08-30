using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public sealed class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }


    [Header("Loading Screen")]
    [SerializeField]
    private GameObject loadingPanel;


    [SerializeField]
    private Image backgroundImage;


    [SerializeField]
    private TMP_Text loadingMessageText;



    [Header("Random Content")]
    [SerializeField]
    private Sprite[] loadingBackgrounds;


    [TextArea]
    [SerializeField]
    private string[] loadingMessages;



    [Header("Timing")]
    [Tooltip(
        "Loading screen kam az kam itni der visible rahega, chahe level " +
        "isse pehle hi load ho jaye. Warna local Addressable load itna " +
        "fast hota hai ke screen sirf ek flash ban kar reh jati hai."
    )]
    [SerializeField, Min(0f)]
    private float minimumLoadingTime = 1.5f;


    private float loadingShownTime;
    private bool isLoadingVisible;


    public bool IsLoadingVisible =>
        isLoadingVisible;



    private void Awake()
    {
        if(Instance != null &&
           Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;


        DontDestroyOnLoad(
            gameObject
        );


        HideLoading();
    }



    public void LoadLevel(
        string sceneName)
    {
        StartCoroutine(
            LoadLevelRoutine(
                sceneName
            )
        );
    }





    private IEnumerator LoadLevelRoutine(
        string sceneName)
    {
        ShowLoading();


        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                sceneName
            );


        operation.allowSceneActivation = false;


        float timer = 0f;


        while(!operation.isDone)
        {
            timer += Time.deltaTime;


            if(operation.progress >= 0.9f &&
               timer >= minimumLoadingTime)
            {
                operation.allowSceneActivation = true;
            }


            yield return null;
        }


        HideLoading();
    }





    public void ShowLoading()
    {
        RandomizeLoadingContent();


        loadingShownTime =
            Time.unscaledTime;

        isLoadingVisible = true;


        if(loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
    }





    /// <summary>
    /// Loading screen ko minimum display time pura hone ke baad hide
    /// karta hai. Caller isay `yield return` kar ke wait kar sakta hai.
    ///
    /// Unscaled time use hota hai taake pause/timeScale se farq na pare.
    /// </summary>
    public IEnumerator HideAfterMinimumTimeRoutine()
    {
        if(isLoadingVisible)
        {
            float elapsed =
                Time.unscaledTime -
                loadingShownTime;


            while(elapsed < minimumLoadingTime)
            {
                yield return null;

                elapsed +=
                    Time.unscaledDeltaTime;
            }
        }


        HideLoading();
    }





    public void HideLoading()
    {
        isLoadingVisible = false;


        if(loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }





    private void RandomizeLoadingContent()
    {
        if(backgroundImage != null &&
           loadingBackgrounds.Length > 0)
        {
            int index =
                Random.Range(
                    0,
                    loadingBackgrounds.Length
                );


            backgroundImage.sprite =
                loadingBackgrounds[index];
        }



        if(loadingMessageText != null &&
           loadingMessages.Length > 0)
        {
            int index =
                Random.Range(
                    0,
                    loadingMessages.Length
                );


            loadingMessageText.text =
                loadingMessages[index];
        }
    }
}