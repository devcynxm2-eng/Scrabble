using UnityEngine;

/// <summary>
/// Poore tower ka liquid ek hi ParticleSystem se nikalta hai.
///
/// Har jar ko apna particle system dena mobile par mehnga hai — ek
/// tower mein 16+ jars ho sakti hain, yani 16 systems. Yahan sirf ek
/// system hai aur jars us se Emit karwati hain, apna rang saath dekar.
/// Is tarah saara spill ek hi draw call batch mein jata hai.
/// </summary>
[DisallowMultipleComponent]
public sealed class SlopeLiquidSpiller : MonoBehaviour
{
    private static SlopeLiquidSpiller instance;


    [Header("Particles")]

    [Tooltip("Jar se phoot kar girne wala chheenta.")]
    [SerializeField]
    private ParticleSystem dropletSystem;


    [Tooltip(
        "Jar se zameen tak girti hui dhaar. Ek juri hui satah, " +
        "alag alag qatre nahi."
    )]
    [SerializeField]
    private ParticleSystem fallSystem;


    [Tooltip(
        "Zameen par behta hua paani. Har tooti jar par iska ek hi " +
        "particle banta hai, har qatre ka alag nahi."
    )]
    [SerializeField]
    private ParticleSystem puddleSystem;


    [Header("Falling Stream")]

    [Tooltip("Dhaar kitni chaudi ho.")]
    [SerializeField]
    private Vector2 streamWidthRange = new Vector2(0.30f, 0.42f);


    [Tooltip("Jar se zameen tak pohanchne mein kitna waqt lage.")]
    [SerializeField, Min(0.15f)]
    private float fallDuration = 0.75f;


    [Header("Ground Puddle")]

    [Tooltip("Zameen dhoondne ke liye layer.")]
    [SerializeField]
    private LayerMask groundMask = ~0;


    [Tooltip("Poori jar ka paani zameen par kitna chaura phaile.")]
    [SerializeField]
    private Vector2 puddleSizeRange = new Vector2(0.75f, 1.05f);


    [Tooltip("Paani zameen par kitni der rehta hai.")]
    [SerializeField]
    private Vector2 puddleLifetimeRange = new Vector2(2.6f, 3.4f);


    [Header("Amount")]

    [Tooltip(
        "Poori jar tootne par kitne droplets. Ek hi piece nikle to " +
        "isi ka hissa nikalta hai."
    )]
    [SerializeField, Range(1, 40)]
    private int dropletsPerJar = 8;


    [Tooltip("Droplet kis raftaar se bahar uchhalta hai.")]
    [SerializeField]
    private Vector2 speedRange = new Vector2(1.1f, 3.0f);


    [Tooltip(
        "Droplet ka size, world units mein. Jar khud lagbhag 0.8 " +
        "lambi hai, is liye 0.1 se neeche kuch bhi nazar nahi aata."
    )]
    [SerializeField]
    private Vector2 sizeRange = new Vector2(0.10f, 0.20f);


    [Tooltip(
        "Droplet kitni der zinda rehta hai. Itna hona chahiye ke wo " +
        "zameen tak pohanch kar behta hua nazar aaye."
    )]
    [SerializeField]
    private Vector2 lifetimeRange = new Vector2(2.5f, 3.5f);


    private void Awake()
    {
        instance = this;
    }


    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }


    /// <summary>
    /// Jar tootne par isay call kiya jata hai. Spiller na mile to
    /// kuch nahi hota — spill sirf ek visual hai, gameplay uspar
    /// depend nahi karta.
    /// </summary>
    public static void SpillAt(
        Vector3 position,
        Color liquidColor,
        float amount,
        float sizeScale)
    {
        if (instance == null)
        {
            return;
        }


        instance.Spill(
            position,
            liquidColor,
            amount,
            sizeScale
        );
    }


    public void Spill(
        Vector3 position,
        Color liquidColor,
        float amount,
        float sizeScale)
    {
        if (dropletSystem == null)
        {
            return;
        }


        int count = Mathf.Max(
            1,
            Mathf.RoundToInt(dropletsPerJar * Mathf.Clamp(amount, 0.05f, 2f))
        );

        /*
         * Jar ka scale sirf 0.41 hai. Pehle droplet size ko usi se
         * multiply karta tha, to droplets 0.02-0.04 ke reh jate thay —
         * yani jar ka teen fisad. Bilkul nazar nahi aate thay. Ab size
         * jar ke scale se chhota nahi hota, sirf bara ho sakta hai.
         */
        float dropletScale = Mathf.Clamp(sizeScale, 1f, 2f);


        for (int i = 0; i < count; i++)
        {
            ParticleSystem.EmitParams emitParams =
                new ParticleSystem.EmitParams();

            emitParams.position = position + Random.insideUnitSphere * 0.09f;

            /*
             * Upar ki taraf jhukaao diya hai taake liquid jar se
             * phoot kar nikalta hua lage, seedha neeche na tapke.
             */
            Vector3 direction =
                (Random.insideUnitSphere + Vector3.up * 0.8f).normalized;

            emitParams.velocity =
                direction * Random.Range(speedRange.x, speedRange.y);

            /*
             * Yehi rang shader mein vertex colour ban kar jata hai,
             * is liye spill bilkul usi jar ka rang hota hai jo tooti.
             */
            emitParams.startColor = liquidColor;

            emitParams.startSize =
                Random.Range(sizeRange.x, sizeRange.y) * dropletScale;

            emitParams.startLifetime =
                Random.Range(lifetimeRange.x, lifetimeRange.y);

            emitParams.applyShapeToPosition = false;

            dropletSystem.Emit(emitParams, 1);
        }


        /*
         * Tarteeb ahem hai. Pehle dhaar chalti hai (jar se zameen tak),
         * chheenta sirf uske sath ka bikhraao hai, aur zameen wala paani
         * tab banta hai jab dhaar wahan pohanch chuki ho — warna paani
         * zameen par pehle se maujood lagta hai.
         */
        float travel = SpillStream(position, liquidColor, amount);

        StartCoroutine(
            SpreadOnGroundAfterFall(position, liquidColor, amount, travel)
        );
    }


    /// <summary>
    /// Jar se zameen tak girti hui dhaar. Ek hi particle, jiski lambai
    /// zameen tak ka faasla hai. Neeche girne ka amal shader khud
    /// particle ki umar se chalata hai.
    /// </summary>
    /// <returns>Zameen tak pohanchne mein lagne wala waqt.</returns>
    private float SpillStream(
        Vector3 breakPoint,
        Color liquidColor,
        float amount)
    {
        if (fallSystem == null ||
            !TryFindGround(breakPoint, out RaycastHit hit))
        {
            return fallDuration;
        }


        float distance = Mathf.Max(0.05f, breakPoint.y - hit.point.y);

        ParticleSystem.EmitParams emitParams =
            new ParticleSystem.EmitParams();

        // Quad jar aur zameen ke darmiyan hai, is liye markaz mein
        emitParams.position = new Vector3(
            breakPoint.x,
            (breakPoint.y + hit.point.y) * 0.5f,
            breakPoint.z
        );

        emitParams.velocity = Vector3.zero;
        emitParams.startColor = liquidColor;

        emitParams.startSize3D = new Vector3(
            Random.Range(streamWidthRange.x, streamWidthRange.y) *
                Mathf.Lerp(0.6f, 1.3f, Mathf.Clamp01(amount)),
            distance,
            1f
        );

        emitParams.startLifetime = fallDuration;
        emitParams.applyShapeToPosition = false;

        fallSystem.Emit(emitParams, 1);

        return fallDuration;
    }


    private System.Collections.IEnumerator SpreadOnGroundAfterFall(
        Vector3 breakPoint,
        Color liquidColor,
        float amount,
        float travel)
    {
        // Dhaar ko zameen tak pohanchne do
        yield return new WaitForSeconds(travel * 0.75f);

        SpillOnGround(breakPoint, liquidColor, amount);
    }


    private bool TryFindGround(
        Vector3 from,
        out RaycastHit hit)
    {
        return Physics.Raycast(
            from + Vector3.up * 0.2f,
            Vector3.down,
            out hit,
            12f,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }



    /// <summary>
    /// Zameen par ek behti hui paani ki satah banata hai.
    ///
    /// Pehle har qatra apna chhota dhabba banata tha, is liye zameen par
    /// paani ki jagah bikhre hue nuqte nazar aate thay. Ab poori jar ka
    /// paani ek hi juri hui satah hai — chaudai us par munhasir hai ke
    /// kitna toota.
    /// </summary>
    private void SpillOnGround(
        Vector3 breakPoint,
        Color liquidColor,
        float amount)
    {
        if (puddleSystem == null)
        {
            return;
        }


        /*
         * Paani wahin girta hai jahan zameen hai. Raycast na lagey to
         * puddle banane ka koi matlab nahi — hawa mein paani ajeeb lagta.
         */
        if (!TryFindGround(breakPoint, out RaycastHit hit))
        {
            return;
        }


        float spread = Mathf.Clamp(amount, 0.05f, 2f);

        ParticleSystem.EmitParams emitParams =
            new ParticleSystem.EmitParams();

        // Zameen se zara upar, warna surface ke sath z-fight karta hai
        emitParams.position = hit.point + Vector3.up * 0.035f;
        emitParams.velocity = Vector3.zero;
        emitParams.startColor = liquidColor;

        emitParams.startSize =
            Random.Range(puddleSizeRange.x, puddleSizeRange.y) *
            Mathf.Lerp(0.45f, 1f, spread);

        emitParams.startLifetime = Random.Range(
            puddleLifetimeRange.x,
            puddleLifetimeRange.y
        );

        emitParams.applyShapeToPosition = false;

        puddleSystem.Emit(emitParams, 1);
    }
}
