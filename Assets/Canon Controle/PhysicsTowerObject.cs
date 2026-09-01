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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

    [Tooltip(
        "Dynamic tower block ki meaningful collision locked block ko " +
        "naturally unlock kar sakti hai. Chhoti settling contacts ignore hongi."
    )]
    [SerializeField, Min(0f)]
    private float minimumBlockImpactSpeed = 0.45f;

    [Tooltip(
        "Moving front-layer block ka physical collision impulse back/adjacent " +
        "locked block ko kitna transfer hoga."
    )]
    [SerializeField, Range(0f, 1f)]
    private float blockImpactTransferMultiplier = 0.3f;

    [Tooltip(
        "Off-centre block collision se naturally milne wali rotation strength."
    )]
    [SerializeField, Range(0f, 1f)]
    private float blockImpactTorqueMultiplier = 0.35f;


    [Header("Break Effect")]

    [Tooltip(
        "Optional authored broken/fractured prefab. Empty ho to runtime " +
        "small material-matched shards generate karega."
    )]
    [SerializeField]
    private GameObject brokenVisualPrefab;

    [SerializeField, Range(3, 16)]
    private int fallbackShardCount = 7;

    [SerializeField, Min(0f)]
    private float breakShardImpulse = 1.4f;

    [SerializeField, Min(0.1f)]
    private float breakShardLifetime = 2.5f;


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

    private bool breakableForSpawn;

    /*
     * Special "chain reaction" block ka runtime state. Ye values spawn
     * ke waqt ConfigureChainReaction() palette definition se bharta hai.
     * Aam blocks par chainReactionEnabled false rehta hai.
     */
    private bool chainReactionEnabled;
    private float chainReactionRadius;
    private int chainReactionMaxBlocks = 1;
    private float chainReactionImpulse;
    private float chainReactionUpwardBias;
    private GameObject chainReactionVfxPrefab;
    private float chainReactionVfxLifetime = 2f;
    private bool chainReactionPropagates;
    private float chainReactionPropagationDelay;

    /* Block detonate hone ka intezaar kar raha hai (chain aa rahi hai). */
    private bool chainReactionScheduled;

    /* Har block sirf ek baar phat sakta hai — is se blast loop nahi banta. */
    private bool chainReactionTriggered;

    private int remainingBreakHits = 1;


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


    public void ConfigureBreakable(
        bool isBreakable,
        int hitsToBreak)
    {
        breakableForSpawn = isBreakable;
        remainingBreakHits = Mathf.Max(1, hitsToBreak);
    }


    /// <summary>
    /// Spawn ke waqt palette definition se special-block settings copy
    /// karta hai. Definition null ho ya chain reaction OFF ho to ye block
    /// bilkul aam block ki tarah behave karta hai — is liye purane levels
    /// par koi asar nahi parta.
    /// </summary>
    public void ConfigureChainReaction(
        PhysicsObjectDefinition definition)
    {
        chainReactionTriggered = false;

        if (definition == null ||
            !definition.ChainReactionEnabled)
        {
            chainReactionEnabled = false;
            return;
        }

        chainReactionEnabled = true;
        chainReactionRadius = definition.ChainReactionRadius;
        chainReactionMaxBlocks = definition.ChainReactionMaxBlocks;
        chainReactionImpulse = definition.ChainReactionImpulse;
        chainReactionUpwardBias = definition.ChainReactionUpwardBias;
        chainReactionVfxPrefab = definition.ChainReactionVfxPrefab;
        chainReactionVfxLifetime = definition.ChainReactionVfxLifetime;
        chainReactionPropagates = definition.ChainReactionPropagates;
        chainReactionPropagationDelay =
            definition.ChainReactionPropagationDelay;
        chainReactionScheduled = false;
    }


    /// <summary>
    /// Special block ka blast. Radius ke andar maujood blocks ko distance
    /// ke hisaab se sort kar ke sirf sab se qareeb wale
    /// chainReactionMaxBlocks ko urata hai — isi cap ki wajah se poora
    /// tower nahi girta.
    /// </summary>
    public void TriggerChainReaction(
        Vector3 hitPoint)
    {
        if (!chainReactionEnabled ||
            chainReactionTriggered ||
            isCleared)
        {
            return;
        }

        chainReactionTriggered = true;
        chainReactionScheduled = false;

        Vector3 blastCenter =
            body != null
                ? body.worldCenterOfMass
                : transform.position;

        SpawnChainReactionVfx(blastCenter);

        List<PhysicsTowerObject> neighbours =
            CollectChainReactionNeighbours(blastCenter);

        for (int i = 0; i < neighbours.Count; i++)
        {
            PhysicsTowerObject neighbour = neighbours[i];

            if (neighbour == null ||
                neighbour.IsCleared)
            {
                continue;
            }

            Vector3 neighbourCenter =
                neighbour.Body != null
                    ? neighbour.Body.worldCenterOfMass
                    : neighbour.transform.position;

            Vector3 offset =
                neighbourCenter - blastCenter;

            float distance = offset.magnitude;

            Vector3 direction =
                distance > 0.001f
                    ? offset / distance
                    : Vector3.up;

            /*
             * Blast ko halka upar ki taraf jhukate hain, warna blocks
             * sirf side mein khisakte hain aur explosion feel nahi hota.
             */
            direction =
                (direction +
                 Vector3.up * chainReactionUpwardBias).normalized;

            /* Qareeb wale blocks ko zyada zor milta hai. */
            float falloff =
                chainReactionRadius > 0.001f
                    ? 1f - Mathf.Clamp01(distance / chainReactionRadius)
                    : 1f;

            float force =
                chainReactionImpulse *
                Mathf.Lerp(0.45f, 1f, falloff);

            Vector3 torqueImpulse =
                UnityEngine.Random.insideUnitSphere *
                (force * 0.08f);

            /*
             * Agar parosi KHUD bhi special block hai aur propagation ON
             * hai, to usay abhi torte nahi — usay thori der baad khud
             * phatne ke liye schedule karte hain.
             *
             * Yehi is feature ki jaan hai: chain NAZAR aani chahiye,
             * block-dar-block safar karti hui. Pehle ye seedha recursive
             * call tha, jis se poori chain EK hi frame mein chal jati thi
             * aur chain reaction ke bajaye ek bara dhamaka lagta tha.
             */
            if (chainReactionPropagates &&
                neighbour.CanChainDetonate)
            {
                neighbour.ScheduleChainDetonation(
                    chainReactionPropagationDelay,
                    blastCenter
                );

                continue;
            }

            /*
             * Aam parosi: countsAsDirectHit = true, taake breakable
             * blocks blast se waqai tootein, sirf dhakka na khayein.
             */
            neighbour.ApplyExternalImpact(
                direction * force,
                torqueImpulse,
                true,
                blastCenter
            );
        }

        /*
         * Special block khud bhi phat kar khatam ho jata hai.
         */
        MarkCleared();
    }


    /// <summary>
    /// Ye block khud phat sakta hai ya nahi — yani special hai, abhi tak
    /// phata nahi, aur pehle se qatar mein bhi nahi.
    /// </summary>
    public bool CanChainDetonate =>
        chainReactionEnabled &&
        !chainReactionTriggered &&
        !chainReactionScheduled &&
        !isCleared;


    /// <summary>
    /// Is block ko thori der baad phatne ke liye schedule karta hai,
    /// taake chain visibly safar kare.
    /// </summary>
    public void ScheduleChainDetonation(
        float delay,
        Vector3 sourcePoint)
    {
        if (!CanChainDetonate)
        {
            return;
        }

        /*
         * Delay 0 ho, ya block inactive ho (coroutine chal hi nahi
         * sakti), to foran phaad dete hain — behaviour purane recursive
         * version jaisa, magar sirf usi soorat mein.
         */
        if (delay <= 0f ||
            !isActiveAndEnabled)
        {
            TriggerChainReaction(sourcePoint);
            return;
        }

        chainReactionScheduled = true;

        StartCoroutine(
            ChainDetonationRoutine(delay, sourcePoint)
        );
    }


    private IEnumerator ChainDetonationRoutine(
        float delay,
        Vector3 sourcePoint)
    {
        yield return new WaitForSeconds(delay);

        chainReactionScheduled = false;

        TriggerChainReaction(sourcePoint);
    }


    private List<PhysicsTowerObject> CollectChainReactionNeighbours(
        Vector3 blastCenter)
    {
        List<PhysicsTowerObject> found =
            new List<PhysicsTowerObject>();

        Collider[] overlaps = Physics.OverlapSphere(
            blastCenter,
            chainReactionRadius,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider overlap = overlaps[i];

            if (overlap == null)
            {
                continue;
            }

            PhysicsTowerObject tower =
                overlap.GetComponentInParent<
                    PhysicsTowerObject>();

            if (tower == null ||
                tower == this ||
                tower.IsCleared ||
                found.Contains(tower))
            {
                continue;
            }

            found.Add(tower);
        }

        /*
         * Sab se qareeb blocks pehle — cap lagne par wahi bachte hain
         * jo blast ke sab se nazdeek thay.
         */
        found.Sort(
            (first, second) =>
            {
                float firstDistance =
                    (first.transform.position -
                     blastCenter).sqrMagnitude;

                float secondDistance =
                    (second.transform.position -
                     blastCenter).sqrMagnitude;

                return firstDistance.CompareTo(secondDistance);
            }
        );

        if (found.Count > chainReactionMaxBlocks)
        {
            found.RemoveRange(
                chainReactionMaxBlocks,
                found.Count - chainReactionMaxBlocks
            );
        }

        return found;
    }


    private void SpawnChainReactionVfx(
        Vector3 blastCenter)
    {
        if (chainReactionVfxPrefab == null)
        {
            return;
        }

        GameObject vfx = Instantiate(
            chainReactionVfxPrefab,
            blastCenter,
            Quaternion.identity
        );

        Destroy(
            vfx,
            Mathf.Max(0.1f, chainReactionVfxLifetime)
        );
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

        breakableForSpawn = false;
        remainingBreakHits = 1;
        chainReactionEnabled = false;
        chainReactionTriggered = false;
        chainReactionScheduled = false;

        /*
         * lowerGroundDisappearEffect ko bhi check karte hain, sirf body ko
         * nahi. body ek [SerializeField] hai — prefab par pehle se assigned
         * hota hai — jabke effect Awake ki ResolveReferences() se milta hai.
         * Agar koi caller Awake chale baghair spawn kare (jaise editor-time
         * level preview), to purani condition ResolveReferences() ko skip
         * kar deti thi aur agli line null reference throw karti thi.
         */
        if (body == null ||
            lowerGroundDisappearEffect == null)
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


    public void ApplyExternalImpact(
        Vector3 impulse,
        Vector3 torqueImpulse,
        bool countsAsDirectHit,
        Vector3 hitPoint)
    {
        if (isCleared)
        {
            return;
        }

        if (countsAsDirectHit &&
            TryHandleBreakableHit(
                impulse,
                hitPoint
            ))
        {
            return;
        }

        if (IsLocked)
        {
            ActivatePhysics(
                impulse,
                torqueImpulse
            );

            return;
        }

        if (body == null)
        {
            ResolveReferences();
        }

        if (body == null || body.isKinematic)
        {
            return;
        }

        body.WakeUp();
        body.AddForce(
            impulse,
            ForceMode.Impulse
        );

        if (torqueImpulse.sqrMagnitude > 0.0001f)
        {
            body.AddTorque(
                torqueImpulse,
                ForceMode.Impulse
            );
        }
    }


    public void MoveLockedWithTable(
        Vector3 worldPosition,
        Quaternion worldRotation)
    {
        if (!IsLocked || isCleared)
        {
            return;
        }

        if (body == null)
        {
            ResolveReferences();
        }

        if (body != null && body.isKinematic)
        {
            body.MovePosition(worldPosition);
            body.MoveRotation(worldRotation);
            return;
        }

        transform.SetPositionAndRotation(
            worldPosition,
            worldRotation
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

        /* Cannonball direct hit gets the authored primary transfer. */
        CannonBallMarker cannonBall =
            collision.collider.GetComponentInParent<
                CannonBallMarker>();

        if (cannonBall != null)
        {
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

            Vector3 cannonBallHitPoint =
                collision.contactCount > 0
                    ? collision.GetContact(0).point
                    : transform.position;

            /*
             * Special block: direct hit par pehle blast. Aam blocks ke
             * liye ye branch skip ho jati hai.
             */
            if (chainReactionEnabled &&
                !chainReactionTriggered)
            {
                TriggerChainReaction(cannonBallHitPoint);
                return;
            }

            if (TryHandleBreakableHit(
                    impulse,
                    cannonBallHitPoint))
            {
                return;
            }

            ActivatePhysics(impulse);
            return;
        }

        PhysicsTowerObject movingBlock =
            collision.collider.GetComponentInParent<
                PhysicsTowerObject>();

        Vector3 relativeVelocity =
            collision.relativeVelocity;

        if (movingBlock == null ||
            movingBlock == this ||
            movingBlock.IsLocked ||
            relativeVelocity.magnitude < minimumBlockImpactSpeed)
        {
            return;
        }

        Vector3 collisionImpulse =
            collision.impulse.sqrMagnitude > 0.0001f
                ? collision.impulse
                : relativeVelocity *
                  Mathf.Max(
                      0.01f,
                      collision.rigidbody != null
                          ? collision.rigidbody.mass
                          : 1f
                  );

        Vector3 transferredImpulse =
            Vector3.ClampMagnitude(
                collisionImpulse *
                blockImpactTransferMultiplier,
                maximumImpactImpulse
            );

        Vector3 torqueImpulse = Vector3.zero;

        if (collision.contactCount > 0 &&
            body != null)
        {
            Vector3 contactOffset =
                collision.GetContact(0).point -
                body.worldCenterOfMass;

            torqueImpulse =
                Vector3.Cross(
                    contactOffset,
                    transferredImpulse
                ) *
                blockImpactTorqueMultiplier;
        }

        ActivatePhysics(
            transferredImpulse,
            torqueImpulse
        );
    }


    private bool TryHandleBreakableHit(
        Vector3 impulse,
        Vector3 hitPoint)
    {
        if (!breakableForSpawn ||
            isCleared)
        {
            return false;
        }

        remainingBreakHits =
            Mathf.Max(0, remainingBreakHits - 1);

        if (remainingBreakHits > 0)
        {
            return true;
        }

        SpawnBreakEffect(impulse, hitPoint);

        isCleared = true;

        Action<PhysicsTowerObject> clearedHandler =
            Cleared;

        if (clearedHandler != null)
        {
            clearedHandler(this);
        }
        else
        {
            gameObject.SetActive(false);
        }

        return true;
    }


    private void SpawnBreakEffect(
        Vector3 impulse,
        Vector3 hitPoint)
    {
        if (brokenVisualPrefab != null)
        {
            GameObject brokenVisual =
                Instantiate(
                    brokenVisualPrefab,
                    transform.position,
                    transform.rotation,
                    transform.parent
                );

            brokenVisual.transform.localScale =
                transform.localScale;

            Rigidbody[] fragmentBodies =
                brokenVisual.GetComponentsInChildren<Rigidbody>(true);

            foreach (Rigidbody fragmentBody in fragmentBodies)
            {
                if (fragmentBody == null)
                {
                    continue;
                }

                fragmentBody.isKinematic = false;
                fragmentBody.useGravity = true;
                fragmentBody.AddForce(
                    impulse /
                    Mathf.Max(1, fragmentBodies.Length),
                    ForceMode.Impulse
                );
                fragmentBody.AddExplosionForce(
                    breakShardImpulse,
                    hitPoint,
                    1.5f,
                    0.05f,
                    ForceMode.Impulse
                );
            }

            Destroy(
                brokenVisual,
                breakShardLifetime
            );
            return;
        }

        SpawnFallbackShards(impulse, hitPoint);
    }


    private void SpawnFallbackShards(
        Vector3 impulse,
        Vector3 hitPoint)
    {
        if (!TryGetPhysicsBounds(out Bounds bounds))
        {
            return;
        }

        Material shardMaterial = null;

        if (colorRenderers == null ||
            colorRenderers.Length == 0)
        {
            ResolveReferences();
        }

        if (colorRenderers != null)
        {
            foreach (Renderer sourceRenderer in colorRenderers)
            {
                if (sourceRenderer != null &&
                    sourceRenderer.sharedMaterial != null)
                {
                    shardMaterial = sourceRenderer.sharedMaterial;
                    break;
                }
            }
        }

        int shardCount =
            Mathf.Clamp(fallbackShardCount, 3, 16);

        float shardDivisor =
            Mathf.Max(1.6f, Mathf.Pow(shardCount, 1f / 3f));

        Vector3 baseShardSize =
            new Vector3(
                Mathf.Max(0.025f, bounds.size.x / shardDivisor),
                Mathf.Max(0.025f, bounds.size.y / shardDivisor),
                Mathf.Max(0.025f, bounds.size.z / shardDivisor)
            );

        for (int index = 0;
             index < shardCount;
             index++)
        {
            GameObject shard =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            shard.name = "Break Shard";
            shard.layer = gameObject.layer;
            shard.transform.position =
                new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    Random.Range(bounds.min.z, bounds.max.z)
                );
            shard.transform.rotation =
                Random.rotation;
            shard.transform.localScale =
                Vector3.Scale(
                    baseShardSize,
                    new Vector3(
                        Random.Range(0.55f, 1.05f),
                        Random.Range(0.55f, 1.05f),
                        Random.Range(0.55f, 1.05f)
                    )
                );

            Renderer shardRenderer =
                shard.GetComponent<Renderer>();

            if (shardRenderer != null &&
                shardMaterial != null)
            {
                shardRenderer.sharedMaterial = shardMaterial;
            }

            Rigidbody shardBody =
                shard.AddComponent<Rigidbody>();

            shardBody.mass =
                body != null
                    ? Mathf.Max(0.01f, body.mass / shardCount)
                    : 0.08f;
            shardBody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;
            shardBody.AddForce(
                impulse / shardCount,
                ForceMode.Impulse
            );
            shardBody.AddExplosionForce(
                breakShardImpulse *
                Random.Range(0.75f, 1.2f),
                hitPoint,
                Mathf.Max(0.5f, bounds.extents.magnitude * 2f),
                0.04f,
                ForceMode.Impulse
            );
            shardBody.AddTorque(
                Random.onUnitSphere *
                breakShardImpulse *
                0.2f,
                ForceMode.Impulse
            );

            Destroy(shard, breakShardLifetime);
        }
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



