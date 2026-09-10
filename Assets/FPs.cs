using UnityEngine;

public class FPs : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
private void Awake()
{
    Application.targetFrameRate = 60;
    QualitySettings.vSyncCount = 0;
}


}
