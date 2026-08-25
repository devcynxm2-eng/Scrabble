using UnityEngine;
using UnityEngine.UI;

public class UIBaseScreen : MonoBehaviour
{
    [Header("Screen Setup")]
    [SerializeField] protected UIScreenType screenType;
    [SerializeField] protected GameObject Panel_View;

    [Header("Optional Close Button")]
    [SerializeField] private Button closeButton;

    [Tooltip("Close button press hone par kis screen par wapas jana hai.")]
    [SerializeField] private UIScreenType closeTargetScreen =
        UIScreenType.MainMenu;


    protected virtual void OnEnable()
    {
        UIEventBroker.OnScreenChangeRequested += HandleScreenChange;

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseScreen);
        }
    }


    protected virtual void OnDisable()
    {
        UIEventBroker.OnScreenChangeRequested -= HandleScreenChange;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseScreen);
        }
    }


    private void HandleScreenChange(UIScreenType targetScreen)
    {
        bool shouldBeActive =
            screenType == targetScreen;


        // Home screen visible rahe jab Home ke popups open hon
        if (IsHomePopup(targetScreen))
        {
            if (screenType == UIScreenType.MainMenu)
            {
                shouldBeActive = true;
            }
        }


        if (Panel_View != null)
        {
            Panel_View.SetActive(shouldBeActive);
        }


        // Actual game pause
        if (screenType == UIScreenType.PauseScreen &&
            targetScreen == UIScreenType.PauseScreen)
        {
            Time.timeScale = 0f;
        }
    }


    private bool IsHomePopup(UIScreenType targetScreen)
    {
        return targetScreen == UIScreenType.SettingScreen ||
               targetScreen == UIScreenType.RewardScreen;
    }


    private void CloseScreen()
    {
        // Pause popup close ho to game resume
        if (screenType == UIScreenType.PauseScreen)
        {
            Time.timeScale = 1f;
        }


        UIEventBroker.RequestScreen(
            closeTargetScreen
        );
    }
}