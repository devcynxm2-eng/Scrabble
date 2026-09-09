#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Kisi breakable jar ka namaindah rang nikalta hai.
///
/// Jar materials ka tint (_BaseColor) safaid hai — rang texture se
/// aata hai. Is liye seedha material se rang lena har jar ko safaid
/// dikhata hai. Yahan material ke preview thumbnail ka ausat rang
/// liya jata hai.
///
/// Do jagah is ki zaroorat hai: level editor ke grid mein cell ka
/// rang, aur prefab mein bake hone wala liquid ka rang. Dono ek hi
/// jagah se aate hain taake grid ka rang aur girne wale paani ka rang
/// hamesha match karein.
/// </summary>
public static class SlopeGlassColorUtility
{
    private static readonly Color[] FallbackColors =
    {
        new Color(0.35f, 0.75f, 0.95f, 1f),
        new Color(1.00f, 0.78f, 0.10f, 1f),
        new Color(0.55f, 0.85f, 0.35f, 1f),
        new Color(0.85f, 0.45f, 0.85f, 1f),
        new Color(1.00f, 0.42f, 0.35f, 1f),
        new Color(0.45f, 0.95f, 0.85f, 1f)
    };


    public static Color FallbackColor(
        int index)
    {
        return FallbackColors[
            Mathf.Abs(index) % FallbackColors.Length
        ];
    }


    /// <summary>
    /// Object ka rang. <paramref name="ready"/> false ho to preview
    /// abhi ban raha hai aur baad mein dobara poochna chahiye.
    /// </summary>
    public static Color GetObjectColor(
        BreakableGlass prefab,
        int fallbackIndex,
        out bool ready)
    {
        ready = true;

        if (prefab == null)
        {
            return FallbackColor(fallbackIndex);
        }


        MeshRenderer renderer =
            prefab.GetComponent<MeshRenderer>();

        Material material =
            renderer != null ? renderer.sharedMaterial : null;

        if (material == null)
        {
            return FallbackColor(fallbackIndex);
        }


        /*
         * Project URP par hai, jahan tint _BaseColor hota hai.
         * Purane shaders _Color use karte hain.
         */
        Color tint = Color.white;

        if (material.HasProperty("_BaseColor"))
        {
            tint = material.GetColor("_BaseColor");
        }
        else if (material.HasProperty("_Color"))
        {
            tint = material.GetColor("_Color");
        }


        if (!IsNearWhite(tint))
        {
            return Opaque(tint);
        }


        Texture2D preview =
            AssetPreview.GetAssetPreview(material);

        if (preview == null)
        {
            ready = !AssetPreview.IsLoadingAssetPreviews();

            return FallbackColor(fallbackIndex);
        }


        if (TryAverageColor(preview, out Color average))
        {
            return average;
        }


        return FallbackColor(fallbackIndex);
    }


    private static bool IsNearWhite(
        Color color)
    {
        return color.r > 0.92f &&
               color.g > 0.92f &&
               color.b > 0.92f;
    }


    /// <summary>
    /// Preview thumbnail ka ausat rang. Transparent aur be-rang (grey)
    /// pixels chhod dete hain taake sirf object ka rang aaye.
    /// </summary>
    private static bool TryAverageColor(
        Texture2D preview,
        out Color average)
    {
        average = Color.white;

        Color[] pixels;

        try
        {
            pixels = preview.GetPixels();
        }
        catch (UnityException)
        {
            return false;
        }


        float r = 0f;
        float g = 0f;
        float b = 0f;
        int counted = 0;

        for (int i = 0; i < pixels.Length; i += 4)
        {
            Color pixel = pixels[i];

            if (pixel.a < 0.5f)
            {
                continue;
            }


            float max = Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b));
            float min = Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));

            if (max - min < 0.08f)
            {
                continue;
            }


            r += pixel.r;
            g += pixel.g;
            b += pixel.b;
            counted++;
        }


        if (counted == 0)
        {
            return false;
        }


        average = Brighten(
            new Color(
                r / counted,
                g / counted,
                b / counted,
                1f
            )
        );

        return true;
    }


    /// <summary>
    /// Preview lighting ki wajah se ausat rang gehra aata hai. Hue
    /// wahi rakhte hue usay chamka dete hain.
    /// </summary>
    private static Color Brighten(
        Color color)
    {
        float max = Mathf.Max(
            color.r,
            Mathf.Max(color.g, color.b)
        );

        if (max < 0.01f)
        {
            return color;
        }


        float scale = 0.85f / max;

        return new Color(
            Mathf.Clamp01(color.r * scale),
            Mathf.Clamp01(color.g * scale),
            Mathf.Clamp01(color.b * scale),
            1f
        );
    }


    private static Color Opaque(
        Color color)
    {
        color.a = 1f;

        return color;
    }
}
#endif
