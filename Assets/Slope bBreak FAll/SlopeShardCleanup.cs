using System.Collections;
using UnityEngine;

/// <summary>
/// Gire hue kanch ke tukre kuch second baad zameen se ghayab kar deta
/// hai — magar sirf tab jab unke ooper kuch khara na ho.
///
/// Jar tootne ke baad tukre asli rigidbody ban kar zameen par gir jate
/// hain aur wahin hamesha ke liye pare rehte hain. Gameplay mein unka
/// koi kaam nahi, magar har tukra ek rigidbody aur ek convex collider
/// hai. Ek tower mein darjnon jars hain, yani sainkron tukre; mobile par
/// ye khamakha ka physics kharcha hai aur screen bhi bhar jati hai.
///
/// Lekin ek tukra kabhi kabhi asal mein bojh utha raha hota hai: koi jar
/// us par tik kar khari ho sakti hai. Aisa tukra hata dena tower ko bina
/// wajah gira dega. BreakableGlass ka apna support map to gire hue tukron
/// ko ginta hi nahi (HasPhysicalSupport released pieces ko chhor deta
/// hai), magar PhysX ki takkar asli hai — jar sach much us par khari
/// rehti hai.
///
/// Is liye ghayab hone se pehle ek dafa ooper dekha jata hai. Ooper kuch
/// ho to tukra wahin rehta hai aur thori der baad phir dekhta hai. Dher
/// isi tarah ooper se neeche khud-ba-khud khali hota hai: sabse ooper
/// wala tukra pehle jata hai, phir uske neeche wala azad ho jata hai.
///
/// Kharcha: koi Update nahi. Component tab tak bilkul khamosh rehta hai
/// jab tak tukra jar se alag nahi hota — BreakableGlassPiece.Release()
/// usay scene root par bhejta hai, aur Unity ka OnTransformParentChanged
/// message wahin sab kuch shuru kar deta hai. BreakableGlass.cs ko is
/// mein haath nahi lagta.
/// </summary>
[DisallowMultipleComponent]
public sealed class SlopeShardCleanup : MonoBehaviour
{
    [Tooltip(
        "Tukre ke tikne ka intezar iss se zyada nahi kiya jata. Agar " +
        "tukra dhalan par lurhakta hi rahe to bhi ginti shuru ho jaye."
    )]
    [SerializeField, Min(0.5f)]
    private float settleTimeout = 5f;


    [Tooltip("Zameen par tik jane ke baad kitni der nazar aata rahe.")]
    [SerializeField, Min(0f)]
    private float secondsOnGround = 2.5f;


    [Tooltip("Ghayab hone mein lagne wala waqt (chhota hote hote).")]
    [SerializeField, Min(0f)]
    private float shrinkDuration = 0.4f;


    [Tooltip(
        "Ooper kuch khara ho to itni der baad dobara dekha jata hai."
    )]
    [SerializeField, Min(0.25f)]
    private float occupiedRecheckInterval = 1f;


    [Tooltip(
        "Tukre ke ooper kitni oonchai tak dekha jaye ke koi tika to nahi."
    )]
    [SerializeField, Min(0.01f)]
    private float topProbeHeight = 0.06f;


    /*
     * Tikne ka faisla. Squared speed rakha hai taake har check par sqrt
     * na lagana pare, aur check har frame ke bajaye is waqfe se hota
     * hai — girte waqt 5 check per second kaafi hain.
     */
    private const float SettledSpeedSquared = 0.01f;
    private const float SettleCheckInterval = 0.2f;


    /*
     * Footprint thora simat kar liya jata hai, warna barabar mein khara
     * padosi tukra bhi "ooper rakha hua" gina jata hai.
     */
    private const float FootprintShrink = 0.9f;


    // Sab shards ek hi buffer istemal karte hain — koi garbage nahi.
    private static readonly Collider[] TopHits = new Collider[16];


    private bool cleaning;


    private void OnTransformParentChanged()
    {
        /*
         * Release() pehle IsFalling true karta hai, phir tukre ko scene
         * root par bhejta hai. Yani parent ka null hona = tukra gir
         * chuka hai. Kisi aur wajah se parent badle to kuch nahi hota.
         */
        if (cleaning || transform.parent != null)
        {
            return;
        }

        BreakableGlassPiece piece = GetComponent<BreakableGlassPiece>();

        if (piece != null && !piece.IsFalling)
        {
            return;
        }

        cleaning = true;

        StartCoroutine(RemoveAfterLanding());
    }


    private IEnumerator RemoveAfterLanding()
    {
        Rigidbody body = GetComponent<Rigidbody>();

        float waited = 0f;

        while (waited < settleTimeout)
        {
            if (body == null ||
                body.IsSleeping() ||
                body.linearVelocity.sqrMagnitude < SettledSpeedSquared)
            {
                break;
            }

            waited += SettleCheckInterval;

            yield return new WaitForSeconds(SettleCheckInterval);
        }


        yield return new WaitForSeconds(secondsOnGround);


        /*
         * Jab tak koi cheez is tukre par tiki hui hai, tukra wahin rehta
         * hai. Ye loop kabhi khatam na bhi ho to theek hai — matlab tukra
         * sach much bojh utha raha hai, aur wohi maqsood hai.
         */
        while (IsCarryingSomething())
        {
            yield return new WaitForSeconds(occupiedRecheckInterval);
        }


        /*
         * Ghayab hote tukre ka collider band, warna simatte hue collider
         * ke aas paas ki cheezein hilti hain.
         */
        Collider ownCollider = GetComponent<Collider>();

        if (ownCollider != null)
        {
            ownCollider.enabled = false;
        }

        if (body != null)
        {
            body.isKinematic = true;
        }


        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;

            transform.localScale = Vector3.Lerp(
                startScale,
                Vector3.zero,
                shrinkDuration > 0f
                    ? Mathf.Clamp01(elapsed / shrinkDuration)
                    : 1f
            );

            yield return null;
        }


        Destroy(gameObject);
    }


    /// <summary>
    /// Tukre ke bilkul ooper ek patli tehh mein dekhta hai. Koi khari
    /// jar ya kisi aur jar ka sabit tukra mila, ya koi doosra gira hua
    /// tukra jo is par rakha hai — to ye tukra abhi nahi hat sakta.
    ///
    /// Zameen aur ball ginti mein nahi aate: floor tukre par "rakha" nahi
    /// hota, aur ball guzarti rehti hai.
    /// </summary>
    private bool IsCarryingSomething()
    {
        Collider ownCollider = GetComponent<Collider>();

        if (ownCollider == null)
        {
            return false;
        }


        Bounds bounds = ownCollider.bounds;

        Vector3 centre = new Vector3(
            bounds.center.x,
            bounds.max.y + topProbeHeight * 0.5f,
            bounds.center.z
        );

        Vector3 halfExtents = new Vector3(
            bounds.extents.x * FootprintShrink,
            topProbeHeight * 0.5f,
            bounds.extents.z * FootprintShrink
        );


        int count = Physics.OverlapBoxNonAlloc(
            centre,
            halfExtents,
            TopHits,
            Quaternion.identity,
            ~0,
            QueryTriggerInteraction.Ignore
        );


        for (int i = 0; i < count; i++)
        {
            Collider hit = TopHits[i];

            if (hit == null ||
                hit.transform == transform ||
                hit.transform.IsChildOf(transform) ||
                BreakableGlass.IsBall(hit))
            {
                continue;
            }


            // Doosra tukra — chahe gira hua ho ya kisi sabit jar ka hissa
            if (hit.GetComponentInParent<BreakableGlassPiece>() != null)
            {
                return true;
            }


            // Khari jar
            if (hit.GetComponentInParent<BreakableGlass>() != null)
            {
                return true;
            }
        }


        return false;
    }
}
