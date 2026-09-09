using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BreakableGlass : MonoBehaviour
{
    [Header("Glass Objects")]
    [Tooltip("The visible, solid version of this glass.")]
    public GameObject intactObject;

    [Tooltip("A child/root containing the pre-broken rigidbody pieces.")]
    public GameObject brokenParts;

    [Header("Damage")]
    [Tooltip("Hits needed before the first glass piece is released.")]
    [Min(1)]
    public int hitsToFracture = 1;

    [Tooltip("Ignore very soft contacts. Set to zero to accept every ball hit.")]
    [Min(0f)]
    public float minimumBallImpactSpeed = 0f;

    [Tooltip("Stops one ball contact from releasing several touching pieces at once.")]
    [Min(0f)]
    public float pieceHitCooldown = 0.08f;

    [Tooltip("Time used to shrink a directly-hit tower glass before destroying it.")]
    [Min(0f)]
    public float destroyScaleDuration = 0.12f;

    [Header("Support Detection")]
    [Tooltip("Extra horizontal distance allowed when finding a supporting piece below.")]
    [Min(0f)]
    public float supportHorizontalTolerance = 0.08f;

    [Tooltip("Pieces close to the highest support level are treated as joint supports.")]
    [Min(0f)]
    public float supportLevelTolerance = 0.12f;

    [Tooltip("Ignore collisions between pieces of this glass to prevent explosive separation.")]
    public bool ignorePieceToPieceCollisions = true;

    [Header("Whole Glass Fall")]
    [Tooltip("Mass used when an unhit glass loses all support and falls as one structure.")]
    [Min(0.01f)]
    public float wholeGlassMass = 1f;

    [Header("Natural Glass Impact")]
    [Tooltip("Minimum relative speed before a falling glass makes another glass react.")]
    [Min(0f)]
    public float minimumGlassImpactSpeed = 0.5f;

    [Tooltip("Controls the natural push/tilt transferred between falling glasses.")]
    [Min(0f)]
    public float glassImpactStrength = 0.16f;

    [Tooltip("Safety clamp that prevents a glass impact from looking like a blast.")]
    [Min(0f)]
    public float maximumGlassImpactImpulse = 1.25f;

    [Tooltip("A whole falling glass breaks when its landing impact reaches this speed.")]
    [Min(0f)]
    public float minimumFallBreakSpeed = 2f;

    private readonly List<BreakableGlassPiece> pieces =
        new List<BreakableGlassPiece>();

    private readonly Dictionary<BreakableGlassPiece, List<BreakableGlassPiece>> supports =
        new Dictionary<BreakableGlassPiece, List<BreakableGlassPiece>>();

    private readonly RaycastHit[] physicalSupportHits = new RaycastHit[32];

    private GlassTowerController tower;
    private int towerRow = -1;
    private int towerColumn = -1;
    private int hitCount;
    private int releasedPieceCount;
    private float nextPieceHitTime;
    private bool fractured;
    private bool broken;
    private bool fallingAsWhole;
    private bool structuralSupportLost;
    private Rigidbody wholeGlassRigidbody;

    public bool IsBroken => broken;
    public bool IsFallingAsWhole => fallingAsWhole;
    public bool IsSupporting =>
        !broken && !fallingAsWhole && !structuralSupportLost;
    public int TowerRow => towerRow;
    public int TowerColumn => towerColumn;
    public int RemainingHits => Mathf.Max(0, hitsToFracture - hitCount);

    private void Start()
    {
        SetInitialState();
    }

    private void SetInitialState()
    {
        fractured = false;
        broken = false;
        fallingAsWhole = false;
        structuralSupportLost = false;
        hitCount = 0;
        releasedPieceCount = 0;
        nextPieceHitTime = 0f;

        if (intactObject != null)
        {
            intactObject.SetActive(true);
        }

        CollectPieces();

        if (brokenParts != null && brokenParts != intactObject)
        {
            brokenParts.SetActive(false);
        }

        if (tower != null)
        {
            SetWholeGlassGravity(false);
        }
    }

    private void CollectPieces()
    {
        pieces.Clear();
        supports.Clear();

        if (brokenParts == null)
        {
            return;
        }

        BreakableGlassPiece[] foundPieces =
            brokenParts.GetComponentsInChildren<BreakableGlassPiece>(true);

        pieces.AddRange(foundPieces);

        // Mixed prefabs can have scripts on only some shards. Register every
        // shard body so none is left kinematic when the glass is released.
        {
            Rigidbody[] bodies =
                brokenParts.GetComponentsInChildren<Rigidbody>(true);

            for (int i = 0; i < bodies.Length; i++)
            {
                BreakableGlassPiece piece =
                    bodies[i].GetComponent<BreakableGlassPiece>();

                if (piece == null)
                {
                    piece = bodies[i].gameObject.AddComponent<BreakableGlassPiece>();
                }

                if (!pieces.Contains(piece))
                    pieces.Add(piece);
            }
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            pieces[i].Configure(this);
            pieces[i].ResetPiece();
        }
    }

    public void ConfigureTower(
        GlassTowerController owner,
        int row,
        int column)
    {
        tower = owner;
        towerRow = row;
        towerColumn = column;
        SetWholeGlassGravity(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
        {
            return;
        }

        HandleWholeGlassImpact(collision);

        if (fallingAsWhole &&
            IsGroundSurface(collision) &&
            collision.relativeVelocity.magnitude >= minimumFallBreakSpeed)
        {
            Shatter(GetImpactPoint(collision));
            return;
        }

        if (broken || fractured || !IsBall(collision.collider))
        {
            return;
        }

        if (collision.relativeVelocity.magnitude < minimumBallImpactSpeed)
        {
            return;
        }

        ApplyBallHit(GetImpactPoint(collision));
    }

    private bool IsGroundSurface(Collision collision)
    {
        if (collision == null || collision.collider == null ||
            IsBall(collision.collider))
        {
            return false;
        }

        BreakableGlass hitGlass =
            collision.collider.GetComponentInParent<BreakableGlass>();
        BreakableGlassPiece hitPiece =
            collision.collider.GetComponentInParent<BreakableGlassPiece>();

        // A glass landing on another whole glass or loose glass piece should
        // only cause a natural physics reaction; it must not shatter here.
        if (hitGlass != null || hitPiece != null)
        {
            return false;
        }

        // Only an upward-facing floor/platform contact counts as ground.
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (Vector3.Dot(collision.GetContact(i).normal, Vector3.up) >= 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleWholeGlassImpact(Collision collision)
    {
        BreakableGlass otherGlass =
            collision.collider.GetComponentInParent<BreakableGlass>();

        if (otherGlass == null || otherGlass == this)
        {
            return;
        }

        Vector3 contactPoint = GetImpactPoint(collision);
        float impactSpeed = collision.relativeVelocity.magnitude;

        if (fallingAsWhole && otherGlass.IsSupporting)
        {
            otherGlass.ReactToGlassImpact(impactSpeed, contactPoint);
        }
        else if (IsSupporting && otherGlass.IsFallingAsWhole)
        {
            ReactToGlassImpact(impactSpeed, contactPoint);
        }
    }

    internal void ReactToGlassImpact(float impactSpeed, Vector3 contactPoint)
    {
        if (!IsSupporting || impactSpeed < minimumGlassImpactSpeed)
        {
            return;
        }

        if (fractured)
        {
            Shatter(contactPoint);
            return;
        }

        fallingAsWhole = true;
        structuralSupportLost = true;
        transform.SetParent(null, true);
        SetWholeGlassGravity(true);

        float impulse = Mathf.Min(
            maximumGlassImpactImpulse,
            impactSpeed * glassImpactStrength);

        if (impulse > 0f && wholeGlassRigidbody != null)
        {
            Vector3 tiltDirection = Vector3.ProjectOnPlane(
                transform.position - contactPoint,
                Vector3.up);

            if (tiltDirection.sqrMagnitude < 0.001f)
            {
                tiltDirection = transform.right;
            }

            tiltDirection.Normalize();
            Vector3 pushDirection =
                (tiltDirection + Vector3.down * 0.2f).normalized;

            wholeGlassRigidbody.AddForceAtPosition(
                pushDirection * impulse,
                contactPoint,
                ForceMode.Impulse);
        }

        if (tower != null)
        {
            tower.NotifyGlassShattered(this);
        }
    }

    internal static bool IsBall(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.CompareTag("Ball"))
        {
            return true;
        }

        Rigidbody attachedBody = other.attachedRigidbody;
        return attachedBody != null && attachedBody.CompareTag("Ball");
    }

    internal static Vector3 GetImpactPoint(Collision collision)
    {
        return collision != null && collision.contactCount > 0
            ? collision.GetContact(0).point
            : Vector3.zero;
    }

    public void ApplyBallHit(Vector3 impactPoint, int damage = 1)
    {
        if (broken || fractured)
        {
            return;
        }

        hitCount += Mathf.Max(1, damage);

        if (hitCount >= Mathf.Max(1, hitsToFracture))
        {
            BeginFracture(impactPoint);
        }
    }

    private void BeginFracture(Vector3 impactPoint)
    {
        // A falling glass has no fixed base: all shards must keep falling.
        // The partial-fracture path deliberately pins supported pieces.
        if (fallingAsWhole || structuralSupportLost)
        {
            Shatter(impactPoint);
            return;
        }

        fractured = true;
        fallingAsWhole = false;
        nextPieceHitTime = Time.time + pieceHitCooldown;
        SetWholeGlassGravity(false);

        if (brokenParts == null)
        {
            CompleteShatter();
            HideIntactObject();
            return;
        }

        brokenParts.SetActive(true);

        // Scene templates can replace the external broken object after Start.
        if (pieces.Count == 0)
        {
            CollectPieces();
        }

        BuildSupportMap();
        HideIntactObject();

        BreakableGlassPiece closestPiece = FindClosestPiece(impactPoint);

        if (closestPiece != null)
        {
            ReleasePieceAndUnsupported(closestPiece, impactPoint);
        }
        else
        {
            CompleteShatter();
        }
    }

    public void RemovePiece(BreakableGlassPiece piece, Vector3 impactPoint)
    {
        if (!fractured || piece == null || piece.IsRemoved)
        {
            return;
        }

        if (!piece.IsFalling && Time.time < nextPieceHitTime)
        {
            return;
        }

        nextPieceHitTime = Time.time + pieceHitCooldown;
        ReleasePieceAndUnsupported(piece, impactPoint);
    }

    private void ReleasePieceAndUnsupported(
        BreakableGlassPiece firstPiece,
        Vector3 impactPoint)
    {
        RemoveHitPiece(firstPiece);

        if (!HasIntactBaseSupport())
        {
            DropAllRemainingPieces(impactPoint);
        }

        bool releasedUnsupportedPiece;

        do
        {
            releasedUnsupportedPiece = false;

            for (int i = 0; i < pieces.Count; i++)
            {
                BreakableGlassPiece piece = pieces[i];

                if (piece == null || piece.IsReleased || IsBasePiece(piece))
                {
                    continue;
                }

                List<BreakableGlassPiece> pieceSupports = supports[piece];
                bool hasSupport = false;

                for (int supportIndex = 0;
                     supportIndex < pieceSupports.Count;
                     supportIndex++)
                {
                    BreakableGlassPiece support = pieceSupports[supportIndex];

                    if (support != null && !support.IsReleased)
                    {
                        hasSupport = true;
                        break;
                    }
                }

                if (!hasSupport)
                {
                    ReleasePiece(piece, impactPoint, 0f);
                    releasedUnsupportedPiece = true;
                }
            }
        }
        while (releasedUnsupportedPiece);

        UpdateStructuralSupportState();
        UpdateParentColliderBounds();

        if (releasedPieceCount >= pieces.Count)
        {
            CompleteShatter();
        }
    }

    private void RemoveHitPiece(BreakableGlassPiece piece)
    {
        if (piece == null || piece.IsRemoved)
        {
            return;
        }

        bool wasAlreadyReleased = piece.IsReleased;
        piece.RemoveByScaling(destroyScaleDuration);

        if (!wasAlreadyReleased)
        {
            releasedPieceCount++;
        }
    }

    private bool HasIntactBaseSupport()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            BreakableGlassPiece piece = pieces[i];

            if (piece != null && IsBasePiece(piece) && !piece.IsReleased)
            {
                return true;
            }
        }

        return false;
    }

    private void DropAllRemainingPieces(Vector3 impactPoint)
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            BreakableGlassPiece piece = pieces[i];

            if (piece != null && !piece.IsReleased)
            {
                ReleasePiece(piece, impactPoint, 0f);
            }
        }
    }

    private void ReleasePiece(
        BreakableGlassPiece piece,
        Vector3 impactPoint,
        float force)
    {
        if (piece == null || piece.IsReleased)
        {
            return;
        }

        piece.Release(impactPoint, force);
        releasedPieceCount++;
    }

    private bool IsBasePiece(BreakableGlassPiece piece)
    {
        return !supports.TryGetValue(piece, out List<BreakableGlassPiece> pieceSupports) ||
               pieceSupports.Count == 0;
    }

    private void UpdateStructuralSupportState()
    {
        if (structuralSupportLost || tower == null || supports.Count == 0)
        {
            return;
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            BreakableGlassPiece piece = pieces[i];

            if (piece != null && IsBasePiece(piece) && !piece.IsReleased)
            {
                return;
            }
        }

        structuralSupportLost = true;
        tower.NotifyGlassShattered(this);
    }

    private void UpdateParentColliderBounds()
    {
        GameObject colliderRoot = intactObject != null
            ? intactObject
            : gameObject;
        BoxCollider[] parentColliders =
            colliderRoot.GetComponentsInChildren<BoxCollider>(true);

        for (int colliderIndex = 0;
             colliderIndex < parentColliders.Length;
             colliderIndex++)
        {
            BoxCollider parentCollider = parentColliders[colliderIndex];

            if (parentCollider == null ||
                IsPartOfBrokenObject(parentCollider.transform))
            {
                continue;
            }

            bool hasRemainingPiece = false;
            Bounds localBounds = new Bounds();

            for (int pieceIndex = 0; pieceIndex < pieces.Count; pieceIndex++)
            {
                BreakableGlassPiece piece = pieces[pieceIndex];

                if (piece == null || piece.IsReleased)
                {
                    continue;
                }

                Bounds worldBounds = piece.GetBounds();
                EncapsulateWorldBounds(
                    ref localBounds,
                    ref hasRemainingPiece,
                    parentCollider.transform,
                    worldBounds);
            }

            if (hasRemainingPiece)
            {
                parentCollider.center = localBounds.center;
                parentCollider.size = localBounds.size;

                // Exact collision is handled by each remaining piece collider.
                // Keep this fitted parent collider disabled so it cannot bridge
                // holes left by destroyed pieces.
                parentCollider.enabled = false;
            }
            else
            {
                parentCollider.center = Vector3.zero;
                parentCollider.size = Vector3.zero;
                parentCollider.enabled = false;
            }
        }
    }

    private static void EncapsulateWorldBounds(
        ref Bounds localBounds,
        ref bool initialized,
        Transform targetSpace,
        Bounds worldBounds)
    {
        Vector3 min = worldBounds.min;
        Vector3 max = worldBounds.max;

        for (int x = 0; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                for (int z = 0; z <= 1; z++)
                {
                    Vector3 worldCorner = new Vector3(
                        x == 0 ? min.x : max.x,
                        y == 0 ? min.y : max.y,
                        z == 0 ? min.z : max.z);
                    Vector3 localCorner =
                        targetSpace.InverseTransformPoint(worldCorner);

                    if (!initialized)
                    {
                        localBounds = new Bounds(localCorner, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(localCorner);
                    }
                }
            }
        }
    }

    private BreakableGlassPiece FindClosestPiece(Vector3 impactPoint)
    {
        BreakableGlassPiece closest = null;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < pieces.Count; i++)
        {
            BreakableGlassPiece piece = pieces[i];

            if (piece == null || piece.IsReleased)
            {
                continue;
            }

            Vector3 closestPoint = piece.GetClosestPoint(impactPoint);
            float distance = (closestPoint - impactPoint).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = piece;
            }
        }

        return closest;
    }

    private void BuildSupportMap()
    {
        supports.Clear();
        IgnoreInternalPieceCollisions();

        for (int i = 0; i < pieces.Count; i++)
        {
            BreakableGlassPiece piece = pieces[i];
            List<BreakableGlassPiece> pieceSupports =
                new List<BreakableGlassPiece>();
            supports[piece] = pieceSupports;

            Bounds pieceBounds = piece.GetBounds();
            float highestSupportY = float.NegativeInfinity;

            for (int candidateIndex = 0;
                 candidateIndex < pieces.Count;
                 candidateIndex++)
            {
                BreakableGlassPiece candidate = pieces[candidateIndex];

                if (candidate == piece || candidate == null)
                {
                    continue;
                }

                Bounds candidateBounds = candidate.GetBounds();

                if (candidateBounds.center.y >= pieceBounds.center.y - 0.001f ||
                    !OverlapsHorizontally(pieceBounds, candidateBounds))
                {
                    continue;
                }

                highestSupportY = Mathf.Max(
                    highestSupportY,
                    candidateBounds.center.y);
            }

            if (float.IsNegativeInfinity(highestSupportY))
            {
                continue;
            }

            for (int candidateIndex = 0;
                 candidateIndex < pieces.Count;
                 candidateIndex++)
            {
                BreakableGlassPiece candidate = pieces[candidateIndex];

                if (candidate == piece || candidate == null)
                {
                    continue;
                }

                Bounds candidateBounds = candidate.GetBounds();

                if (Mathf.Abs(candidateBounds.center.y - highestSupportY) <=
                        supportLevelTolerance &&
                    candidateBounds.center.y < pieceBounds.center.y - 0.001f &&
                    OverlapsHorizontally(pieceBounds, candidateBounds))
                {
                    pieceSupports.Add(candidate);
                }
            }
        }
    }

    private void IgnoreInternalPieceCollisions()
    {
        if (!ignorePieceToPieceCollisions)
        {
            return;
        }

        for (int firstIndex = 0; firstIndex < pieces.Count; firstIndex++)
        {
            Collider firstCollider = pieces[firstIndex] != null
                ? pieces[firstIndex].PieceCollider
                : null;

            if (firstCollider == null)
            {
                continue;
            }

            for (int secondIndex = firstIndex + 1;
                 secondIndex < pieces.Count;
                 secondIndex++)
            {
                Collider secondCollider = pieces[secondIndex] != null
                    ? pieces[secondIndex].PieceCollider
                    : null;

                if (secondCollider != null)
                {
                    Physics.IgnoreCollision(firstCollider, secondCollider, true);
                }
            }
        }
    }

    private bool OverlapsHorizontally(Bounds first, Bounds second)
    {
        bool overlapsX =
            first.min.x - supportHorizontalTolerance <= second.max.x &&
            first.max.x + supportHorizontalTolerance >= second.min.x;
        bool overlapsZ =
            first.min.z - supportHorizontalTolerance <= second.max.z &&
            first.max.z + supportHorizontalTolerance >= second.min.z;

        return overlapsX && overlapsZ;
    }

    public bool HasPhysicalSupport(
        float extraDistance,
        float probeRadius,
        LayerMask collisionMask)
    {
        if (!IsSupporting || !TryGetActiveColliderBounds(out Bounds glassBounds))
        {
            return false;
        }

        Vector3 origin = glassBounds.center + Vector3.up * 0.01f;
        float castDistance = glassBounds.extents.y + Mathf.Max(0.01f, extraDistance);
        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            Mathf.Max(0.005f, probeRadius),
            Vector3.down,
            physicalSupportHits,
            castDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = physicalSupportHits[i].collider;

            if (hitCollider == null ||
                hitCollider.transform == transform ||
                hitCollider.transform.IsChildOf(transform) ||
                IsBall(hitCollider))
            {
                continue;
            }

            BreakableGlassPiece supportPiece =
                hitCollider.GetComponentInParent<BreakableGlassPiece>();

            if (supportPiece != null)
            {
                if (!supportPiece.IsReleased && supportPiece.Owner != this)
                {
                    return true;
                }

                continue;
            }

            BreakableGlass supportGlass =
                hitCollider.GetComponentInParent<BreakableGlass>();

            if (supportGlass != null)
            {
                if (supportGlass != this && supportGlass.IsSupporting)
                {
                    return true;
                }

                continue;
            }

            // A normal level collider (floor/platform) is valid support.
            return true;
        }

        return false;
    }

    private bool TryGetActiveColliderBounds(out Bounds combinedBounds)
    {
        combinedBounds = default;
        bool initialized = false;
        Collider[] glassColliders = GetComponentsInChildren<Collider>(false);

        for (int i = 0; i < glassColliders.Length; i++)
        {
            Collider glassCollider = glassColliders[i];

            if (glassCollider == null ||
                !glassCollider.enabled ||
                !glassCollider.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!initialized)
            {
                combinedBounds = glassCollider.bounds;
                initialized = true;
            }
            else
            {
                combinedBounds.Encapsulate(glassCollider.bounds);
            }
        }

        return initialized;
    }

    public void ShatterFromLostSupport()
    {
        DropFromLostSupport();
    }

    public void DropFromLostSupport()
    {
        if (broken || fallingAsWhole)
        {
            return;
        }

        if (fractured)
        {
            // A previously hit glass no longer has one solid shell. Let its
            // remaining pieces fall naturally, still without any added force.
            Shatter(transform.position);
            return;
        }

        fallingAsWhole = true;
        structuralSupportLost = true;
        transform.SetParent(null, true);
        SetWholeGlassGravity(true);

        if (tower != null)
        {
            tower.NotifyGlassShattered(this);
        }
    }

    public void Shatter(Vector3 impactPoint)
    {
        if (broken)
        {
            return;
        }

        fractured = true;
        fallingAsWhole = false;
        SetWholeGlassGravity(false);

        if (brokenParts != null)
        {
            brokenParts.SetActive(true);

            if (pieces.Count == 0)
            {
                CollectPieces();
            }

            IgnoreInternalPieceCollisions();
            HideIntactObject();

            for (int i = 0; i < pieces.Count; i++)
            {
                ReleasePiece(pieces[i], impactPoint, 0f);
            }
        }
        else
        {
            HideIntactObject();
        }

        CompleteShatter();
    }

    public void DestroyWithPieces()
    {
        CompleteShatter();
        foreach (var piece in pieces)
        {
            if (piece == null)
                continue;
            piece.gameObject.SetActive(false);
            Destroy(piece.gameObject);
        }
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void CompleteShatter()
    {
        if (broken)
        {
            return;
        }

        broken = true;
        structuralSupportLost = true;

        if (tower != null)
        {
            tower.NotifyGlassShattered(this);
        }
    }

    internal static void StabilizeFallingBody(Rigidbody body)
    {
        // Convex glass shapes and freshly released shards can overlap. Limit
        // the solver's separation speed so contacts cannot launch the pile.
        body.maxDepenetrationVelocity = 1f;
        body.constraints = RigidbodyConstraints.None;
        body.solverIterations = Mathf.Max(body.solverIterations, 12);
        body.solverVelocityIterations = Mathf.Max(body.solverVelocityIterations, 4);
        body.linearDamping = Mathf.Max(body.linearDamping, 0.15f);
        body.angularDamping = Mathf.Max(body.angularDamping, 0.8f);
        body.maxAngularVelocity = Mathf.Min(body.maxAngularVelocity, 6f);
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void SetWholeGlassGravity(bool enabled)
    {
        // Rigidbody and support colliders must move on the same root. Keeping the
        // body on a visual child can leave an invisible parent collider behind.
        GameObject physicsObject = gameObject;

        if (wholeGlassRigidbody == null)
        {
            wholeGlassRigidbody = physicsObject.GetComponent<Rigidbody>();

            if (wholeGlassRigidbody == null)
            {
                wholeGlassRigidbody = physicsObject.AddComponent<Rigidbody>();
            }
        }

        if (enabled)
        {
            MeshCollider[] meshColliders =
                physicsObject.GetComponentsInChildren<MeshCollider>(true);

            for (int i = 0; i < meshColliders.Length; i++)
            {
                if (!IsPartOfBrokenObject(meshColliders[i].transform))
                {
                    meshColliders[i].convex = true;
                }
            }
        }

        wholeGlassRigidbody.mass = Mathf.Max(0.01f, wholeGlassMass);
        wholeGlassRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        wholeGlassRigidbody.isKinematic = !enabled;
        wholeGlassRigidbody.useGravity = enabled;

        if (enabled)
        {
            StabilizeFallingBody(wholeGlassRigidbody);
            wholeGlassRigidbody.WakeUp();
        }
        else
        {
            wholeGlassRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }

    private void HideIntactObject()
    {
        if (intactObject == null)
        {
            return;
        }

        bool brokenPartsAreInsideIntact =
            brokenParts != null &&
            (brokenParts == intactObject ||
             brokenParts.transform.IsChildOf(intactObject.transform));

        if (!brokenPartsAreInsideIntact)
        {
            intactObject.SetActive(false);
            return;
        }

        Renderer[] renderers =
            intactObject.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (!IsPartOfBrokenObject(renderers[i].transform))
            {
                renderers[i].enabled = false;
            }
        }

        Collider[] colliders =
            intactObject.GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (!IsPartOfBrokenObject(colliders[i].transform))
            {
                colliders[i].enabled = false;
            }
        }
    }

    private bool IsPartOfBrokenObject(Transform candidate)
    {
        return brokenParts != null &&
               (candidate == brokenParts.transform ||
                candidate.IsChildOf(brokenParts.transform));
    }
}
