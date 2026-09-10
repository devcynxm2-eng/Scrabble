using UnityEngine;

// Emits once at the actual fracture, before the intact visual can be disabled.
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


    private bool spilledOnBreak;

    public Color LiquidColor => liquidColor;
    public float LiquidAmount => liquidAmount;

    public void NotifyBreak(Vector3 impactPoint)
    {
        if (spilledOnBreak)
            return;

        spilledOnBreak = true;
        Spill(impactPoint, 1f);
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
