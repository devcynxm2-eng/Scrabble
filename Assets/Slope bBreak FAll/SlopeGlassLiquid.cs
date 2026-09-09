using UnityEngine;

/// <summary>
/// Jar ke andar ka liquid.
///
/// Ye BreakableGlass ko bilkul haath nahi lagata. Pehle spill ka hook
/// BreakableGlass ke andar lagaya gaya tha, magar wo file game ki asal
/// mechanics chalati hai — usay chhoona theek nahi. Ab ye component
/// khud jar ki public haalat dekhta hai aur usi par paani nikalta hai.
///
/// Rang bhi yahan pehle se bhara hota hai: jars ka rang texture se aata
/// hai, material ke tint se nahi, aur runtime par texture parhna mobile
/// par faltu kharcha hai. SlopeGlassPrefabBuilder prefab banate waqt
/// wahi ausat rang likh deta hai jo level editor ke grid mein bhi
/// dikhta hai.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(BreakableGlass))]
public sealed class SlopeGlassLiquid : MonoBehaviour
{
    [Tooltip(
        "Jar ke andar ke liquid ka rang. Prefab banate waqt jar ke " +
        "apne rang se bhara jata hai."
    )]
    [SerializeField]
    private Color liquidColor = Color.white;


    [Tooltip("Is jar se kitna liquid nikle (1 = normal).")]
    [SerializeField, Range(0.25f, 2f)]
    private float liquidAmount = 1f;


    private BreakableGlass glass;
    private int lastRemainingHits = -1;
    private bool spilledOnBreak;


    public Color LiquidColor => liquidColor;

    public float LiquidAmount => liquidAmount;


    private void Awake()
    {
        glass = GetComponent<BreakableGlass>();

        if (glass != null)
        {
            lastRemainingHits = glass.RemainingHits;
        }
    }


    private void Update()
    {
        if (glass == null)
        {
            enabled = false;

            return;
        }


        /*
         * Chot lagi magar jar poori nahi tooti — thora sa paani nikalta
         * hai. RemainingHits public hai, is liye iske liye BreakableGlass
         * mein kuch add karne ki zaroorat nahi.
         */
        int remaining = glass.RemainingHits;

        if (remaining < lastRemainingHits)
        {
            Spill(transform.position, 0.35f);
        }

        lastRemainingHits = remaining;


        if (!glass.IsBroken || spilledOnBreak)
        {
            return;
        }


        spilledOnBreak = true;

        Spill(transform.position, 1f);

        /*
         * Jar toot chuki, ab har frame poochne ki koi wajah nahi. Ek
         * tower mein darjnon jars hoti hain, is liye ye band karna
         * mobile par mayne rakhta hai.
         */
        enabled = false;
    }


    /// <summary>
    /// <paramref name="share"/> batata hai ke jar ka kitna hissa toota.
    /// Chhoti chot par thora paani, poori toot par bharpoor.
    /// </summary>
    public void Spill(
        Vector3 breakPoint,
        float share)
    {
        /*
         * Break point kabhi kabhi jar ke kinare par hota hai. Paani jar
         * ke andar se nikalna chahiye, is liye thora markaz ki taraf.
         */
        Vector3 origin = Vector3.Lerp(
            breakPoint,
            transform.position,
            0.3f
        );


        SlopeLiquidSpiller.SpillAt(
            origin,
            liquidColor,
            liquidAmount * Mathf.Clamp01(share),
            transform.lossyScale.y
        );
    }
}
