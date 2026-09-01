using UnityEngine;

/// <summary>
/// See-saw beam. Ek fulcrum par jhukta hai aur apna tilt angle report
/// karta hai.
///
/// Blocks platforms par apne wazan ke sath baithte hain, is liye jis side
/// zyada wazan bachta hai wo side neeche chala jata hai — tilt asli physics
/// se aata hai, koi fake animation nahi.
/// </summary>
[DisallowMultipleComponent]
public sealed class BalanceBeam : MonoBehaviour
{
    [Header("Beam")]

    [Tooltip(
        "Jhukne wala bar. Is par NON-kinematic Rigidbody hona chahiye."
    )]
    [SerializeField]
    private Rigidbody beamBody;

    [Tooltip(
        "Left tower yahan spawn hoga. Beam ka child hona chahiye taake " +
        "beam ke sath jhuke."
    )]
    [SerializeField]
    private Transform leftPlatform;

    [Tooltip("Right tower yahan spawn hoga. Beam ka child hona chahiye.")]
    [SerializeField]
    private Transform rightPlatform;


    [Header("Hinge")]

    [Tooltip(
        "ON: Awake par HingeJoint khud configure ho jayega. Agar aap ne " +
        "prefab par khud hinge set kiya hai to ise OFF kar dein."
    )]
    [SerializeField]
    private bool configureHingeOnAwake = true;

    [Tooltip(
        "Jhukne ka axis. Camera samne se dekh raha ho to (0,0,1) sahi hai."
    )]
    [SerializeField]
    private Vector3 hingeAxis = Vector3.forward;

    [Tooltip(
        "Beam is angle se zyada physically nahi jhuk sakta. Ise game ke " +
        "fail angle se thora ZYADA rakhein, warna beam limit par ruk " +
        "jayega aur fail trigger hi nahi hoga."
    )]
    [SerializeField, Range(5f, 89f)]
    private float hingeLimit = 35f;

    [Tooltip(
        "Halki si spring jo beam ko level par wapas laane ki koshish " +
        "karti hai. 0 = bilkul free see-saw (zyada mushkil)."
    )]
    [SerializeField, Min(0f)]
    private float restoringSpring = 0f;

    [SerializeField, Min(0f)]
    private float restoringDamper = 2f;


    private Quaternion initialBeamRotation;
    private HingeJoint hinge;
    private bool hasInitialRotation;


    public Transform LeftPlatform => leftPlatform;

    public Transform RightPlatform => rightPlatform;

    public Rigidbody BeamBody => beamBody;


    /// <summary>
    /// Beam ka jhukav degrees mein.
    /// Musbat (+) = RIGHT side neeche, manfi (-) = LEFT side neeche.
    /// </summary>
    public float TiltAngle
    {
        get
        {
            Transform beamTransform =
                beamBody != null
                    ? beamBody.transform
                    : transform;

            /*
             * Beam ke right axis ki verticality hi tilt hai. Asin se
             * seedha signed angle mil jata hai aur ye ±90 se aage
             * wrap nahi hota.
             */
            float verticality =
                Mathf.Clamp(
                    beamTransform.right.y,
                    -1f,
                    1f
                );

            return -Mathf.Asin(verticality) * Mathf.Rad2Deg;
        }
    }


    public float AbsoluteTiltAngle =>
        Mathf.Abs(TiltAngle);


    private void Awake()
    {
        if (beamBody == null)
        {
            beamBody = GetComponentInChildren<Rigidbody>();
        }

        if (beamBody != null)
        {
            initialBeamRotation = beamBody.transform.localRotation;
            hasInitialRotation = true;
        }

        if (configureHingeOnAwake)
        {
            ConfigureHinge();
        }
    }


    /// <summary>
    /// Beam par HingeJoint set karta hai. connectedBody null rakhte hain,
    /// yani beam duniya ke saath apni jagah par hinge hota hai — is se
    /// alag se kinematic fulcrum rigidbody ki zaroorat nahi rehti.
    /// </summary>
    public void ConfigureHinge()
    {
        if (beamBody == null)
        {
            Debug.LogError(
                "BalanceBeam: beamBody assign nahi hai.",
                this
            );

            return;
        }

        hinge = beamBody.GetComponent<HingeJoint>();

        if (hinge == null)
        {
            hinge = beamBody.gameObject.AddComponent<HingeJoint>();
        }

        hinge.autoConfigureConnectedAnchor = false;
        hinge.anchor = Vector3.zero;
        hinge.connectedAnchor = beamBody.transform.position;

        hinge.axis =
            hingeAxis.sqrMagnitude > 0.0001f
                ? hingeAxis.normalized
                : Vector3.forward;

        hinge.useLimits = true;

        hinge.limits = new JointLimits
        {
            min = -hingeLimit,
            max = hingeLimit,
            bounciness = 0f,
            bounceMinVelocity = 0f
        };

        if (restoringSpring > 0f)
        {
            hinge.useSpring = true;

            hinge.spring = new JointSpring
            {
                spring = restoringSpring,
                damper = restoringDamper,
                targetPosition = 0f
            };
        }
        else
        {
            hinge.useSpring = false;
        }
    }


    /// <summary>
    /// Beam ko wapas level par le aata hai aur uski saari motion rok
    /// deta hai. Level restart par use hota hai.
    /// </summary>
    public void ResetBeam()
    {
        if (beamBody == null)
        {
            return;
        }

        /*
         * hasInitialRotation sirf tab true hota hai jab Awake chal chuka
         * ho. Editor-time tooling (jaise level preview) Awake ke baghair
         * kaam karti hai, is liye wahan authored pose par gir jate hain —
         * beam prefab hamesha LEVEL (identity local rotation) authored
         * hona chahiye.
         */
        beamBody.transform.localRotation =
            hasInitialRotation
                ? initialBeamRotation
                : Quaternion.identity;

#if UNITY_6000_0_OR_NEWER
        beamBody.linearVelocity = Vector3.zero;
#else
        beamBody.velocity = Vector3.zero;
#endif

        beamBody.angularVelocity = Vector3.zero;
        beamBody.WakeUp();
    }


    private void OnDrawGizmosSelected()
    {
        if (leftPlatform != null)
        {
            Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.9f);
            Gizmos.DrawWireCube(
                leftPlatform.position,
                Vector3.one * 0.2f
            );
        }

        if (rightPlatform != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.9f);
            Gizmos.DrawWireCube(
                rightPlatform.position,
                Vector3.one * 0.2f
            );
        }
    }
}
