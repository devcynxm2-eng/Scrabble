//using System;
//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public sealed class PhysicsTowerObject : MonoBehaviour
//{
//    public event Action<PhysicsTowerObject> Cleared;

//    [Header("Level")]
//    [SerializeField]
//    private bool countsAsTarget = true;

//    [Header("Physics")]
//    [SerializeField]
//    private Rigidbody body;

//    [SerializeField, Min(1)]
//    private int solverIterations = 8;

//    [SerializeField, Min(1)]
//    private int solverVelocityIterations = 2;

//    private bool isCleared;

//    public bool CountsAsTarget => countsAsTarget;
//    public bool IsCleared => isCleared;

//    private void Awake()
//    {
//        if (body == null)
//        {
//            body = GetComponent<Rigidbody>();
//        }
//    }

//    public void PrepareForSpawn()
//    {
//        isCleared = false;

//        if (body == null)
//            return;

//        body.isKinematic = false;
//        body.useGravity = true;
//        body.detectCollisions = true;

//        body.solverIterations =
//            solverIterations;

//        body.solverVelocityIterations =
//            solverVelocityIterations;

//        SetVelocity(Vector3.zero);

//        body.angularVelocity =
//            Vector3.zero;

//        body.WakeUp();
//    }

//    public void PrepareForPool()
//    {
//        isCleared = true;

//        if (body == null)
//            return;

//        SetVelocity(Vector3.zero);

//        body.angularVelocity =
//            Vector3.zero;

//        body.useGravity = false;
//        body.isKinematic = true;

//        body.Sleep();
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (isCleared || other == null)
//            return;

//        LevelClearZone clearZone =
//            other.GetComponentInParent<LevelClearZone>();

//        if (clearZone == null)
//            return;

//        isCleared = true;

//        Cleared?.Invoke(this);
//    }

//    private void SetVelocity(Vector3 velocity)
//    {
//#if UNITY_6000_0_OR_NEWER
//        body.linearVelocity = velocity;
//#else
//        body.velocity = velocity;
//#endif
//    }
//}





using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class PhysicsTowerObject : MonoBehaviour
{
    public event Action<PhysicsTowerObject> Cleared;

    /// <summary>
    /// Fires the moment a locked block turns dynamic (direct hit or
    /// lost support) — available for VFX/SFX/analytics hooks. The
    /// support-chain check itself doesn't need this: it reads
    /// IsLocked + movement-from-spawn each FixedUpdate directly.
    /// </summary>
    public event Action<PhysicsTowerObject, Vector3> Activated;


    [Header("Level")]

    [SerializeField]
    private bool countsAsTarget = true;


    [Header("Spawn Locking")]

    [Tooltip(
        "ON: object spawn par locked/kinematic rahega (khada rahega, " +
        "khud se nahi girega). Sirf cannonball se direct hit hone par " +
        "ya support khone par dynamic/gravity-affected banega. " +
        "Royal Smash jaisi 'smash the tower' gameplay ke liye ON rakhein."
    )]
    [SerializeField]
    private bool startLockedOnSpawn = true;


    [Header("Hit Impact")]

    [Tooltip("Cannonball ki velocity ka kitna impulse is object ko transfer hoga.")]
    [SerializeField, Range(0f, 2f)]
    private float impactTransferMultiplier = 0.35f;

    [SerializeField, Min(0f)]
    private float maximumImpactImpulse = 14f;


    [Header("Physics")]

    [SerializeField]
    private Rigidbody body;

    [SerializeField, Min(1)]
    private int solverIterations = 8;

    [SerializeField, Min(1)]
    private int solverVelocityIterations = 2;


    [Header("Visual Color")]

    [Tooltip(
        "Empty ho to script automatically child Renderers find karegi. " +
        "Agar sirf specific mesh ko color karna ho to Renderer manually assign karein."
    )]
    [SerializeField]
    private Renderer[] colorRenderers;


    private Collider[] physicsColliders;

    private MaterialPropertyBlock
        colorPropertyBlock;

    private LowerGroundDisappearEffect
        lowerGroundDisappearEffect;

    private bool isCleared;


    private static readonly int
        BaseColorProperty =
            Shader.PropertyToID(
                "_BaseColor"
            );

    private static readonly int
        ColorProperty =
            Shader.PropertyToID(
                "_Color"
            );


    public bool CountsAsTarget =>
        countsAsTarget;

    public bool IsCleared =>
        isCleared;

    public Rigidbody Body =>
        body;

    /// <summary>
    /// True while the object is kinematic/no-gravity (standing solid,
    /// still supporting whatever is stacked above it).
    /// </summary>
    public bool IsLocked { get; private set; }

    /// <summary>
    /// Grid cell this instance was spawned into. Set by
    /// LevelRuntimeController right after spawn; used for support-chain
    /// lookups (is the cell directly below still standing?).
    /// </summary>
    public Vector3Int GridCoordinate { get; private set; }

    /// <summary>
    /// Grid mein is piece ka complete occupied footprint. Support check
    /// anchor ke sirf ek cell ki bajaye is poore bottom footprint ko use
    /// karta hai.
    /// </summary>
    public Vector3Int GridSpan { get; private set; } = Vector3Int.one;

    /// <summary>
    /// Designer ka cell-unit offset, including Center On Seam 0.5
    /// offsets. Runtime support calculation actual shifted footprint ko
    /// isi value se resolve karta hai.
    /// </summary>
    public Vector3 GridLocalOffset { get; private set; }


    private void Awake()
    {
        ResolveReferences();
    }


    private void ResolveReferences()
    {
        if (body == null)
        {
            body =
                GetComponent<Rigidbody>();
        }

        physicsColliders =
            GetComponentsInChildren<Collider>(
                true
            );

        if (colorRenderers == null ||
            colorRenderers.Length == 0)
        {
            colorRenderers =
                GetComponentsInChildren<Renderer>(
                    true
                );
        }

        if (colorPropertyBlock == null)
        {
            colorPropertyBlock =
                new MaterialPropertyBlock();
        }

        if (lowerGroundDisappearEffect == null)
        {
            lowerGroundDisappearEffect =
                GetComponent<LowerGroundDisappearEffect>();

            if (lowerGroundDisappearEffect == null)
            {
                lowerGroundDisappearEffect =
                    gameObject.AddComponent<LowerGroundDisappearEffect>();
            }
        }

        lowerGroundDisappearEffect.SetDestroyOnComplete(false);
        lowerGroundDisappearEffect.SetAcceptUnmarkedStaticGround(true);
    }


    public void SetGridFootprint(
        Vector3Int coordinate,
        Vector3Int span,
        Vector3 localOffset)
    {
        GridCoordinate = coordinate;
        GridSpan = new Vector3Int(
            Mathf.Max(1, span.x),
            Mathf.Max(1, span.y),
            Mathf.Max(1, span.z)
        );
        GridLocalOffset = localOffset;
    }


    public bool TryGetPhysicsBounds(
        out Bounds combinedBounds)
    {
        combinedBounds =
            default;

        if (physicsColliders == null ||
            physicsColliders.Length == 0)
        {
            ResolveReferences();
        }

        bool hasBounds =
            false;

        foreach (Collider targetCollider
                 in physicsColliders)
        {
            if (targetCollider == null ||
                !targetCollider.enabled ||
                targetCollider.isTrigger)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds =
                    targetCollider.bounds;

                hasBounds =
                    true;
            }
            else
            {
                combinedBounds.Encapsulate(
                    targetCollider.bounds
                );
            }
        }

        return hasBounds;
    }


    /// <summary>
    /// Optimized color assignment.
    ///
    /// Renderer.material use nahi hota,
    /// isliye material instances create nahi hongi.
    /// </summary>
    public void SetVisualColor(
        Color color)
    {
        if (colorRenderers == null ||
            colorRenderers.Length == 0 ||
            colorPropertyBlock == null)
        {
            ResolveReferences();
        }

        if (colorRenderers == null)
        {
            return;
        }

        foreach (Renderer targetRenderer
                 in colorRenderers)
        {
            if (targetRenderer == null)
            {
                continue;
            }

            Material sharedMaterial =
                targetRenderer.sharedMaterial;

            if (sharedMaterial == null)
            {
                continue;
            }

            colorPropertyBlock.Clear();

            targetRenderer.GetPropertyBlock(
                colorPropertyBlock
            );

            /*
             * Unity URP Lit / Simple Lit
             */
            if (sharedMaterial.HasProperty(
                    BaseColorProperty))
            {
                colorPropertyBlock.SetColor(
                    BaseColorProperty,
                    color
                );
            }

            /*
             * Built-in Standard ya kuch
             * custom shaders.
             */
            if (sharedMaterial.HasProperty(
                    ColorProperty))
            {
                colorPropertyBlock.SetColor(
                    ColorProperty,
                    color
                );
            }

            targetRenderer.SetPropertyBlock(
                colorPropertyBlock
            );
        }
    }


    /// <summary>
    /// Clears runtime visual overrides so the Renderer uses the material
    /// and texture authored directly on the prefab.
    /// </summary>
    public void RestorePrefabVisual()
    {
        if (colorRenderers == null ||
            colorRenderers.Length == 0 ||
            colorPropertyBlock == null)
        {
            ResolveReferences();
        }

        if (colorRenderers == null ||
            colorPropertyBlock == null)
        {
            return;
        }

        foreach (Renderer targetRenderer
                 in colorRenderers)
        {
            if (targetRenderer == null)
            {
                continue;
            }

            colorPropertyBlock.Clear();

            targetRenderer.SetPropertyBlock(
                colorPropertyBlock
            );
        }
    }


    public void PrepareForSpawn()
    {
        isCleared =
            false;

        if (body == null)
        {
            ResolveReferences();
        }

        if (body == null)
        {
            return;
        }

        lowerGroundDisappearEffect.Completed -=
            HandleLowerGroundDisappearCompleted;

        lowerGroundDisappearEffect.Completed +=
            HandleLowerGroundDisappearCompleted;

        lowerGroundDisappearEffect.ResetEffect();

        body.detectCollisions =
            true;

        body.solverIterations =
            solverIterations;

        body.solverVelocityIterations =
            solverVelocityIterations;

        /*
         * Locked spawn: naya spawned block khud se nahi girega.
         * Sirf direct hit (OnCollisionEnter) ya support-chain
         * (LevelRuntimeController) se ActivatePhysics() call hone
         * par gravity/physics enable hogi.
         */
        IsLocked =
            startLockedOnSpawn;

        body.isKinematic =
            IsLocked;

        body.useGravity =
            !IsLocked;

        /*
         * Kinematic body par velocity set karna Unity console warning
         * deta hai (unsupported, harmless) — isliye sirf dynamic
         * (unlocked) case mein reset karte hain.
         */
        if (!IsLocked)
        {
            SetVelocity(
                Vector3.zero
            );

            body.angularVelocity =
                Vector3.zero;
        }

        body.WakeUp();
    }


    /// <summary>
    /// Locked block ko dynamic banata hai — direct cannonball hit se,
    /// ya LevelRuntimeController ke support-chain check se (jab neeche
    /// wala block hat jaye).
    /// </summary>
    public void ActivatePhysics(
        Vector3 impulse = default,
        Vector3 torqueImpulse = default)
    {
        if (!IsLocked ||
            isCleared)
        {
            return;
        }

        if (body == null)
        {
            ResolveReferences();
        }

        if (body == null)
        {
            return;
        }

        IsLocked =
            false;

        body.isKinematic =
            false;

        body.useGravity =
            true;

        body.WakeUp();

        if (impulse.sqrMagnitude >
            0.0001f)
        {
            body.AddForce(
                impulse,
                ForceMode.Impulse
            );
        }

        if (torqueImpulse.sqrMagnitude >
            0.0001f)
        {
            body.AddTorque(
                torqueImpulse,
                ForceMode.Impulse
            );
        }

        Activated?.Invoke(
            this,
            impulse
        );
    }


    public void PrepareForPool()
    {
        if (lowerGroundDisappearEffect != null)
        {
            lowerGroundDisappearEffect.Completed -=
                HandleLowerGroundDisappearCompleted;

            lowerGroundDisappearEffect.ResetEffect();
        }

        isCleared =
            true;

        IsLocked =
            true;

        GridCoordinate =
            default;

        GridSpan =
            Vector3Int.one;

        GridLocalOffset =
            Vector3.zero;

        /*
         * Old level ke event references
         * pooled object ke sath nahi rahenge.
         */
        Cleared =
            null;

        Activated =
            null;

        if (body == null)
        {
            ResolveReferences();
        }

        if (body == null)
        {
            return;
        }

        /*
         * Sirf tab velocity reset karo jab body abhi dynamic thi —
         * pehle se kinematic body par set karna sirf console
         * warning deta hai, koi asar nahi hota.
         */
        if (!body.isKinematic)
        {
            SetVelocity(
                Vector3.zero
            );

            body.angularVelocity =
                Vector3.zero;
        }

        body.useGravity =
            false;

        body.isKinematic =
            true;

        body.Sleep();
    }


    private void OnCollisionEnter(
        Collision collision)
    {
        if (!IsLocked ||
            isCleared ||
            collision == null ||
            collision.collider == null)
        {
            return;
        }

        /*
         * Sirf CannonBallMarker wale colliders is block ko activate
         * karte hain — random physics touches (dusre locked blocks
         * ke beech chhoti overlaps wagera) ko ignore kiya jata hai.
         */
        CannonBallMarker cannonBall =
            collision.collider.GetComponentInParent<
                CannonBallMarker>();

        if (cannonBall == null)
        {
            return;
        }

        Vector3 incomingVelocity =
            collision.relativeVelocity;

        float incomingMass =
            collision.rigidbody != null
                ? Mathf.Max(0.01f, collision.rigidbody.mass)
                : 1f;

        Vector3 impulse =
            Vector3.ClampMagnitude(
                incomingVelocity *
                incomingMass *
                impactTransferMultiplier,
                maximumImpactImpulse
            );

        ActivatePhysics(
            impulse
        );
    }


    private void OnTriggerEnter(
        Collider other)
    {
        if (isCleared ||
            other == null)
        {
            return;
        }

        LevelClearZone clearZone =
            other.GetComponentInParent<
                LevelClearZone>();

        if (clearZone == null)
        {
            return;
        }

        if (lowerGroundDisappearEffect != null &&
            lowerGroundDisappearEffect.TryBeginFromTrigger(
                Vector3.up
            ))
        {
            return;
        }

        MarkCleared();
    }


    private void HandleLowerGroundDisappearCompleted(
        LowerGroundDisappearEffect completedEffect)
    {
        MarkCleared();
    }


    private void MarkCleared()
    {
        if (isCleared)
        {
            return;
        }

        isCleared =
            true;

        Cleared?.Invoke(
            this
        );
    }


    private void SetVelocity(
        Vector3 velocity)
    {
        if (body == null)
        {
            return;
        }

#if UNITY_6000_0_OR_NEWER
        body.linearVelocity =
            velocity;
#else
        body.velocity =
            velocity;
#endif
    }


#if UNITY_EDITOR
    private void Reset()
    {
        body =
            GetComponent<Rigidbody>();

        physicsColliders =
            GetComponentsInChildren<Collider>(
                true
            );

        colorRenderers =
            GetComponentsInChildren<Renderer>(
                true
            );
    }
#endif
}







//using System;
//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public sealed class PhysicsTowerObject : MonoBehaviour
//{
//    public event Action<PhysicsTowerObject> Cleared;

//    [Header("Level")]

//    [SerializeField]
//    private bool countsAsTarget = true;

//    [Header("Physics")]

//    [SerializeField]
//    private Rigidbody body;

//    [SerializeField, Min(1)]
//    private int solverIterations = 8;

//    [SerializeField, Min(1)]
//    private int solverVelocityIterations = 2;

//    private Collider[] physicsColliders;

//    private bool isCleared;

//    public bool CountsAsTarget =>
//        countsAsTarget;

//    public bool IsCleared =>
//        isCleared;

//    public Rigidbody Body =>
//        body;

//    private void Awake()
//    {
//        ResolveReferences();
//    }

//    private void ResolveReferences()
//    {
//        if (body == null)
//        {
//            body =
//                GetComponent<Rigidbody>();
//        }

//        physicsColliders =
//            GetComponentsInChildren<Collider>(
//                true
//            );
//    }

//    /// <summary>
//    /// Actual enabled non-trigger physics colliders
//    /// ki combined world-space bounds return karta hai.
//    ///
//    /// Generator isi se actual block width/height
//    /// automatically calculate karega.
//    /// </summary>
//    public bool TryGetPhysicsBounds(
//        out Bounds combinedBounds)
//    {
//        combinedBounds =
//            default;

//        if (physicsColliders == null ||
//            physicsColliders.Length == 0)
//        {
//            ResolveReferences();
//        }

//        bool hasBounds =
//            false;

//        foreach (Collider targetCollider
//                 in physicsColliders)
//        {
//            if (targetCollider == null ||
//                !targetCollider.enabled ||
//                targetCollider.isTrigger)
//            {
//                continue;
//            }

//            if (!hasBounds)
//            {
//                combinedBounds =
//                    targetCollider.bounds;

//                hasBounds =
//                    true;
//            }
//            else
//            {
//                combinedBounds.Encapsulate(
//                    targetCollider.bounds
//                );
//            }
//        }

//        return hasBounds;
//    }

//    /// <summary>
//    /// Pool se gameplay mein aate waqt
//    /// real physics enable hoti hai.
//    /// </summary>
//    public void PrepareForSpawn()
//    {
//        isCleared =
//            false;

//        if (body == null)
//        {
//            ResolveReferences();
//        }

//        if (body == null)
//        {
//            return;
//        }

//        SetVelocity(
//            Vector3.zero
//        );

//        body.angularVelocity =
//            Vector3.zero;

//        body.detectCollisions =
//            true;

//        body.isKinematic =
//            false;

//        body.useGravity =
//            true;

//        body.solverIterations =
//            solverIterations;

//        body.solverVelocityIterations =
//            solverVelocityIterations;

//        body.WakeUp();
//    }

//    /// <summary>
//    /// Object destroy nahi hota.
//    /// Pool mein return hota hai.
//    /// </summary>
//    public void PrepareForPool()
//    {
//        isCleared =
//            true;

//        Cleared =
//            null;

//        if (body == null)
//        {
//            ResolveReferences();
//        }

//        if (body == null)
//        {
//            return;
//        }

//        SetVelocity(
//            Vector3.zero
//        );

//        body.angularVelocity =
//            Vector3.zero;

//        body.useGravity =
//            false;

//        body.isKinematic =
//            true;

//        body.Sleep();
//    }

//    private void OnTriggerEnter(
//        Collider other)
//    {
//        if (isCleared ||
//            other == null)
//        {
//            return;
//        }

//        LevelClearZone clearZone =
//            other.GetComponentInParent<
//                LevelClearZone>();

//        if (clearZone == null)
//        {
//            return;
//        }

//        MarkCleared();
//    }

//    private void MarkCleared()
//    {
//        if (isCleared)
//        {
//            return;
//        }

//        isCleared =
//            true;

//        Cleared?.Invoke(
//            this
//        );
//    }

//    private void SetVelocity(
//        Vector3 velocity)
//    {
//        if (body == null)
//        {
//            return;
//        }

//#if UNITY_6000_0_OR_NEWER
//        body.linearVelocity =
//            velocity;
//#else
//        body.velocity =
//            velocity;
//#endif
//    }

//#if UNITY_EDITOR
//    private void Reset()
//    {
//        body =
//            GetComponent<Rigidbody>();

//        physicsColliders =
//            GetComponentsInChildren<Collider>(
//                true
//            );
//    }
//#endif
//}



