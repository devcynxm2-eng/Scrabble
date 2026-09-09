using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class BreakableGlassPiece : MonoBehaviour
{
    private BreakableGlass owner;
    private Rigidbody cachedRigidbody;
    private Collider cachedCollider;

    public bool IsRemoved { get; private set; }
    public bool IsFalling { get; private set; }
    public bool IsReleased => IsRemoved || IsFalling;
    internal BreakableGlass Owner => owner;

    internal Collider PieceCollider
    {
        get
        {
            CacheComponents();
            return cachedCollider;
        }
    }

    private void Awake()
    {
        CacheComponents();
    }

    internal void Configure(BreakableGlass parentGlass)
    {
        owner = parentGlass;
        CacheComponents();
    }

    internal void ResetPiece()
    {
        CacheComponents();
        IsRemoved = false;
        IsFalling = false;

        if (cachedRigidbody == null)
        {
            return;
        }

        cachedRigidbody.isKinematic = true;
        cachedRigidbody.useGravity = false;
        cachedRigidbody.linearVelocity = Vector3.zero;
        cachedRigidbody.angularVelocity = Vector3.zero;
    }

    private void CacheComponents()
    {
        if (cachedRigidbody == null)
        {
            cachedRigidbody = GetComponent<Rigidbody>();
        }

        if (cachedCollider == null)
        {
            cachedCollider = GetComponent<Collider>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (owner == null || IsRemoved || collision == null)
        {
            return;
        }

        if (BreakableGlass.IsBall(collision.collider))
        {
            owner.RemovePiece(this, BreakableGlass.GetImpactPoint(collision));
            return;
        }

        if (!IsFalling)
        {
            return;
        }

        BreakableGlass hitGlass =
            collision.collider.GetComponentInParent<BreakableGlass>();

        if (hitGlass != null && hitGlass != owner)
        {
            hitGlass.ReactToGlassImpact(
                collision.relativeVelocity.magnitude,
                BreakableGlass.GetImpactPoint(collision));
        }
    }

    internal Vector3 GetClosestPoint(Vector3 position)
    {
        CacheComponents();
        return cachedCollider != null
            ? cachedCollider.ClosestPoint(position)
            : transform.position;
    }

    internal Bounds GetBounds()
    {
        CacheComponents();

        if (cachedCollider != null)
        {
            return cachedCollider.bounds;
        }

        return new Bounds(transform.position, Vector3.one * 0.01f);
    }

    public void RemoveByScaling(float duration)
    {
        if (IsRemoved)
        {
            return;
        }

        CacheComponents();
        IsRemoved = true;
        IsFalling = false;

        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = true;
            cachedRigidbody.useGravity = false;
            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }

        Collider[] pieceColliders =
            GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < pieceColliders.Length; i++)
        {
            pieceColliders[i].enabled = false;
        }

        StartCoroutine(ScaleDownAndDestroy(Mathf.Max(0f, duration)));
    }

    private IEnumerator ScaleDownAndDestroy(float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;
            transform.localScale =
                Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }

    public void Release(Vector3 impactPoint, float force)
    {
        if (IsReleased)
        {
            return;
        }

        CacheComponents();
        IsFalling = true;

        if (cachedRigidbody == null)
        {
            return;
        }

        // Detach the complete piece (mesh, collider and Rigidbody together) so a
        // kinematic glass/root transform cannot leave its collider behind.
        transform.SetParent(null, true);
        cachedRigidbody.isKinematic = false;
        cachedRigidbody.useGravity = true;
        cachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        BreakableGlass.StabilizeFallingBody(cachedRigidbody);
        cachedRigidbody.WakeUp();

        if (force <= 0f)
        {
            return;
        }

        Vector3 direction =
            cachedRigidbody.worldCenterOfMass - impactPoint;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector3.up;
        }

        cachedRigidbody.AddForce(
            direction.normalized * force,
            ForceMode.Impulse);
    }
}
