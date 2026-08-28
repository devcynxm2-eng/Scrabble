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
    [SerializeField]
    private float minimumLoadingTime = 1.5f;



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


        if(loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
    }





    public void HideLoading()
    {
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