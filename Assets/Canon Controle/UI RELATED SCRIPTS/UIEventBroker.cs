using System;
public class UIEventBroker
{
    public static event Action<UIScreenType> OnScreenChangeRequested;


    public static void RequestScreen(UIScreenType Screen_Type)
    {
        OnScreenChangeRequested?.Invoke(Screen_Type);
    }



}
