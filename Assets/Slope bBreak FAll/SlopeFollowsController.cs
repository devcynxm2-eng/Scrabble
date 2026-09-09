using UnityEngine;

/// <summary>
/// Ramp ko control object ke sath left/right khiskata hai.
///
/// Player jab aim karne ke liye drag karta hai to BallDropController sirf
/// apne controlObject ko X par le jata hai — ramp apni jagah khari rehti
/// hai. Is component ke sath ramp bhi utna hi khiskti hai, is liye ball
/// hamesha ramp ke beech se neeche aati hai, chahe aim kahin bhi ho.
///
/// BallDropController ko chhua nahi jata: uska controlObject pehle se
/// public hai, to yahan sirf uski position parhi jati hai. Isi wajah se
/// smoothing bhi muft mil jati hai — controller khud apne object ko lerp
/// karta hai, aur ramp us lerp ki hui position ko follow karti hai.
///
/// Colliders ke bare mein ek zaroori baat: ramp par MeshCollider hai aur
/// koi Rigidbody nahi, yani PhysX ke liye wo STATIC hai. Static collider
/// ko har frame hilane par PhysX apna poora static tree dobara banata hai
/// — mobile par ye mehnga sauda hai. Is liye shuru mein hi in par ek
/// kinematic Rigidbody laga di jati hai, jo hilne wale platforms ka
/// mamool ka tareeqa hai.
/// </summary>
[DisallowMultipleComponent]
public sealed class SlopeFollowsController : MonoBehaviour
{
    [Tooltip(
        "Slope ball ka controller. Khali chhorne par khud dhoond leta " +
        "hai (wohi jiska Control Object assign ho)."
    )]
    [SerializeField]
    private BallDropController controller;


    [Tooltip(
        "Jo cheezein controller ke sath khiskengi — aam tor par sirf " +
        "ramp (Plane (1))."
    )]
    [SerializeField]
    private Transform[] targets = new Transform[0];


    [Tooltip(
        "Controller ki harkat ka kitna hissa ramp par lage. 1 = bilkul " +
        "barabar. Ramp ke kinare ground se bahar nikalte lagen to isay " +
        "kam kar dein."
    )]
    [SerializeField, Range(0f, 1f)]
    private float followAmount = 1f;


    [Tooltip(
        "Hilne wale colliders par kinematic Rigidbody laga deta hai, " +
        "warna PhysX unhen static samajh kar har frame apna tree dobara " +
        "banata hai."
    )]
    [SerializeField]
    private bool addKinematicBodies = true;


    private Transform control;
    private float controlStartX;
    private float[] targetStartX;


    private void Awake()
    {
        if (controller == null)
        {
            controller = FindController();
        }

        if (controller == null || controller.controlObject == null)
        {
            Debug.LogWarning(
                "SlopeFollowsController: Control Object nahi mila, ramp " +
                "nahi hilegi.",
                this
            );

            enabled = false;

            return;
        }


        control = controller.controlObject;
        controlStartX = control.localPosition.x;

        targetStartX = new float[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
            {
                continue;
            }

            targetStartX[i] = targets[i].localPosition.x;

            if (addKinematicBodies)
            {
                MakeMovable(targets[i]);
            }
        }
    }


    /// <summary>
    /// Scene ka wohi controller jiska Control Object assign hai. Scene
    /// mein purana khali controller bhi para ho sakta hai, is liye sirf
    /// naam se dhoondna kaafi nahi.
    /// </summary>
    private BallDropController FindController()
    {
        BallDropController[] all =
            FindObjectsByType<BallDropController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].controlObject != null)
            {
                return all[i];
            }
        }

        return null;
    }


    private void MakeMovable(Transform target)
    {
        if (target.GetComponent<Collider>() == null ||
            target.GetComponent<Rigidbody>() != null)
        {
            return;
        }

        Rigidbody body = target.gameObject.AddComponent<Rigidbody>();

        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }


    /*
     * LateUpdate isi liye ke controller apne object ko Update mein lerp
     * karta hai — us ke baad parhne se ramp ek frame peechay nahi rehti.
     */
    private void LateUpdate()
    {
        if (control == null)
        {
            return;
        }


        float shift =
            (control.localPosition.x - controlStartX) * followAmount;

        for (int i = 0; i < targets.Length; i++)
        {
            Transform target = targets[i];

            if (target == null)
            {
                continue;
            }

            Vector3 local = target.localPosition;

            local.x = targetStartX[i] + shift;

            target.localPosition = local;
        }
    }
}
