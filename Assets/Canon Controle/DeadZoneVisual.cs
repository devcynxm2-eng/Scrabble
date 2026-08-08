using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public sealed class DeadZoneVisual : MonoBehaviour
{
    [Header("Reference")]

    [Tooltip(
        "Wohi CannonController assign karein " +
        "jiski Bottom Dead Space Height use karni hai."
    )]
    [SerializeField]
    private CannonController cannonController;


    [Header("Visual")]

    [Tooltip(
        "Dead zone ka transparent overlay color."
    )]
    [SerializeField]
    private Color deadZoneColor =
        new Color(
            1f,
            0.20f,
            0.10f,
            0.22f
        );


    private RectTransform rectTransform;
    private Image targetImage;
    private Canvas parentCanvas;

    private float lastHeightPixels =
        -1f;

    private float lastCanvasScaleFactor =
        -1f;


    private void Awake()
    {
        CacheReferences();

        SetupVisual();

        RefreshSize(
            true
        );
    }


    private void OnEnable()
    {
        CacheReferences();

        SetupVisual();

        RefreshSize(
            true
        );
    }


    private void LateUpdate()
    {
        /*
         * Cannon dead-zone height runtime par
         * change ho ya Canvas scale/resolution change ho,
         * UI automatically update hogi.
         */
        RefreshSize(
            false
        );
    }


    private void CacheReferences()
    {
        if (rectTransform == null)
        {
            rectTransform =
                GetComponent<RectTransform>();
        }


        if (targetImage == null)
        {
            targetImage =
                GetComponent<Image>();
        }


        if (parentCanvas == null)
        {
            parentCanvas =
                GetComponentInParent<Canvas>();
        }
    }


    private void SetupVisual()
    {
        if (rectTransform != null)
        {
            /*
             * Bottom stretch.
             *
             * Left/right automatically
             * complete screen width.
             */
            rectTransform.anchorMin =
                new Vector2(
                    0f,
                    0f
                );

            rectTransform.anchorMax =
                new Vector2(
                    1f,
                    0f
                );

            rectTransform.pivot =
                new Vector2(
                    0.5f,
                    0f
                );

            rectTransform.anchoredPosition =
                Vector2.zero;

            /*
             * Horizontal stretch mein
             * X size 0 hona chahiye.
             */
            Vector2 sizeDelta =
                rectTransform.sizeDelta;

            sizeDelta.x =
                0f;

            rectTransform.sizeDelta =
                sizeDelta;
        }


        if (targetImage != null)
        {
            targetImage.color =
                deadZoneColor;

            /*
             * VERY IMPORTANT:
             *
             * Visual input ko block nahi karegi.
             */
            targetImage.raycastTarget =
                false;
        }
    }


    private void RefreshSize(
        bool force)
    {
        if (
            cannonController == null ||
            rectTransform == null)
        {
            return;
        }


        float deadZonePixels =
            Mathf.Max(
                0f,
                cannonController
                    .BottomDeadSpaceHeight
            );


        /*
         * CannonController screen pixels use karta hai.
         *
         * CanvasScaler ke sath RectTransform units
         * different ho sakti hain.
         *
         * Canvas scale factor se convert karne ki wajah se
         * visual dead-zone actual gameplay dead-zone
         * ke sath align rahegi.
         */
        float canvasScaleFactor =
            1f;


        if (parentCanvas != null)
        {
            canvasScaleFactor =
                Mathf.Max(
                    0.0001f,
                    parentCanvas.scaleFactor
                );
        }


        if (
            !force &&
            Mathf.Approximately(
                deadZonePixels,
                lastHeightPixels
            ) &&
            Mathf.Approximately(
                canvasScaleFactor,
                lastCanvasScaleFactor
            ))
        {
            return;
        }


        lastHeightPixels =
            deadZonePixels;

        lastCanvasScaleFactor =
            canvasScaleFactor;


        float uiHeight =
            deadZonePixels /
            canvasScaleFactor;


        Vector2 sizeDelta =
            rectTransform.sizeDelta;

        sizeDelta.x =
            0f;

        sizeDelta.y =
            uiHeight;

        rectTransform.sizeDelta =
            sizeDelta;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targetImage == null)
        {
            targetImage =
                GetComponent<Image>();
        }


        if (targetImage != null)
        {
            targetImage.color =
                deadZoneColor;

            targetImage.raycastTarget =
                false;
        }
    }
#endif
}