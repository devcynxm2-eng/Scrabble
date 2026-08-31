using TMPro;
using UnityEngine;

public sealed class GlobalFontController : MonoBehaviour
{
    [Header("Global Font")]
    [Tooltip("Yahan apna TextMeshPro Font Asset drag & drop karein.")]
    [SerializeField]
    private TMP_FontAsset globalFont;


    [Header("Options")]
    [Tooltip("Scene ke inactive TMP objects ko bhi update karega.")]
    [SerializeField]
    private bool includeInactive = true;


    [Tooltip("Scene load hone ke baad automatically font apply kare.")]
    [SerializeField]
    private bool applyOnStart = true;


    [Tooltip("Har frame naye TMP objects ko check kare. " +
             "Sirf tab enable karein jab runtime mein dynamically TMP create hote hon.")]
    [SerializeField]
    private bool monitorNewTexts = false;


    private void Start()
    {
        if (applyOnStart)
        {
            ApplyGlobalFont();
        }
    }


    private void Update()
    {
        if (!monitorNewTexts)
        {
            return;
        }

        ApplyGlobalFont();
    }


    /// <summary>
    /// Scene ke tamam TMP_Text components par selected font apply karta hai.
    /// </summary>
    [ContextMenu("Apply Global Font")]
    public void ApplyGlobalFont()
    {
        if (globalFont == null)
        {
            Debug.LogWarning(
                "GlobalFontController: Global Font Asset assign nahi hai.",
                this
            );

            return;
        }


        TMP_Text[] texts =
            FindObjectsByType<TMP_Text>(
                includeInactive
                    ? FindObjectsInactive.Include
                    : FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );


        int changedCount = 0;


        for (int i = 0;
             i < texts.Length;
             i++)
        {
            TMP_Text text =
                texts[i];


            if (text == null)
            {
                continue;
            }


            if (text.font == globalFont)
            {
                continue;
            }


            text.font =
                globalFont;


            changedCount++;
        }


        Debug.Log(
            $"GlobalFontController: {changedCount} TMP texts par font apply ho gaya.",
            this
        );
    }


    /// <summary>
    /// Kisi ek specific TMP text par global font apply karta hai.
    /// Dynamic UI ke liye useful hai.
    /// </summary>
    public void ApplyFontToText(
        TMP_Text text)
    {
        if (globalFont == null ||
            text == null)
        {
            return;
        }


        text.font =
            globalFont;
    }
}