using UnityEngine;
using UnityEngine.UI;


public class MainScreenController : MonoBehaviour
{
    



    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button playButton;



    private void Start()
    {
        settingButton.onClick.AddListener(() => UIEventBroker.RequestScreen(UIScreenType.SettingScreen));
        playButton.onClick.AddListener(() => UIEventBroker.RequestScreen(UIScreenType.GamePlayScreen));
    }






}
