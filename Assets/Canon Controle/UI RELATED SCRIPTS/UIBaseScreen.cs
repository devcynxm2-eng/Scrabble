using UnityEngine;

public class UIBaseScreen : MonoBehaviour
{
    

[SerializeField] protected UIScreenType screenType;
[SerializeField] protected GameObject Panel_View;

protected virtual void OnEnable(){
        
        UIEventBroker.OnScreenChangeRequested += HandleScreenChange;
    
    
}


protected virtual void OnDisable()
    {
        


UIEventBroker.OnScreenChangeRequested -= HandleScreenChange;



    }




private void HandleScreenChange(UIScreenType targetscreen)
    {

        bool shouldbeactive = (screenType == targetscreen);


        if(IsOverlayScreen(targetscreen)){

            if(screenType ==UIScreenType.GamePlayScreen) shouldbeactive = true;

        }

Panel_View.SetActive(shouldbeactive);



    }


private bool IsOverlayScreen(UIScreenType target)
    {
        
        return target == UIScreenType.PauseScreen ||
                target == UIScreenType.SettingScreen ||
                target == UIScreenType.LevelCompleteScreen;

    }



}
