using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Gives dynamic objects a small final bounce when they reach the lower
/// part of a LevelTable, then smoothly shrinks them away.
/// </summary>
[DisallowMultipleComponent]
public sealed class LowerGroundDisappearEffect : MonoBehaviour
{
    public event Action<LowerGroundDisappearEffect> Completed;

    [Header("Bounce Then Disappear")]

    [SerializeField, Min(0f)]
    private float bounceSpeed = 1.6f;

    [SerializeField, Range(0f, 1f)]
    private float sidewaysVelocityRetention = 0.45f;

    [SerializeField, Min(0f)]
    private float bounceVisibleDuration = 0.18f;

    [SerializeField, Min(0.05f)]
    private float shrinkDuration = 0.75f;

    [Tooltip(
        "Shrink ke duran sideways movement aur spin kitni naturally " +
        "settle hon. Vertical bounce/gravity active rehti hai."
    )]
    [SerializeField, Min(0f)]
    private float settleDamping = 3f;

    [SerializeField]
    private bool destroyOnComplete = true;

    [Tooltip(
        "ON: LevelTable marker na ho tab bhi static upward-facing collider " +
        "ko lower ground treat karega."
    )]
    [SerializeField]
    private bool acceptUnmarkedStaticGround = true;

    private Rigidbody body;
    private Coroutine disappearRoutine;
    private Coroutine fallbackRoutine;
    private Vector3 scaleBeforeDisappear;
    private bool isDisappearing;

    public bool IsDisappearing => isDisappearing;


    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }


    public void SetDestroyOnComplete(bool shouldDestroy)
    {
        destroyOnComplete = shouldDestroy;
    }


    public void SetAcceptUnmarkedStaticGround(bool accept)
    {
        acceptUnmarkedStaticGround = accept;
    }


    public void ScheduleSmoothFallback(
        float delay)
    {
        if (fallbackRoutine != null)
        {
            StopCoroutine(fallbackRoutine);
        }

        fallbackRoutine =
            StartCoroutine(
                SmoothFallbackRoutine(
                    Mathf.Max(0f, delay)
                )
            );
    }


    public void ResetEffect()
    {
        if (disappearRoutine != null)
        {
            StopCoroutine(disappearRoutine);
            disappearRoutine = null;
        }

        if (fallbackRoutine != null)
        {
            StopCoroutine(fallbackRoutine);
            fallbackRoutine = null;
        }

        isDisappearing = false;

        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }

        if (body != null)
        {
            body.detectCollisions = true;
        }
    }


    public bool TryBeginFromTrigger(
        Vector3 bounceNormal)
    {
        return TryBeginDisappear(bounceNormal);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null ||
            collision.collider == null)
        {
            return;
        }

        Vector3 bounceNormal = Vector3.up;

        if (collision.contactCount > 0)
        {
            bounceNormal =
                collision.GetContact(0).normal;
        }

        bool isMarkedLowerGround =
            IsMarkedLowerGround(
                collision.collider
            );

        bool isUnmarkedStaticGround =
            acceptUnmarkedStaticGround &&
            IsUnmarkedStaticGround(
                collision,
                bounceNormal
            );

        if (!isMarkedLowerGround &&
            !isUnmarkedStaticGround)
        {
            return;
        }

        TryBeginDisappear(bounceNormal);
    }


    private bool TryBeginDisappear(
        Vector3 bounceNormal)
    {
        if (isDisappearing)
        {
            return false;
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }

        if (body == null ||
            body.isKinematic)
        {
            return false;
        }

        isDisappearing = true;
        scaleBeforeDisappear = transform.localScale;

        if (fallbackRoutine != null)
        {
            StopCoroutine(fallbackRoutine);
            fallbackRoutine = null;
        }

        bounceNormal =
            bounceNormal.sqrMagnitude > 0.0001f
                ? bounceNormal.normalized
                : Vector3.up;

        Vector3 currentVelocity =
            GetBodyVelocity(body);

        Vector3 sidewaysVelocity =
            Vector3.ProjectOnPlane(
                currentVelocity,
                bounceNormal
            ) * sidewaysVelocityRetention;

        SetBodyVelocity(
            body,
            sidewaysVelocity +
            bounceNormal * bounceSpeed
        );

        body.WakeUp();

        disappearRoutine =
            StartCoroutine(DisappearRoutine());

        return true;
    }


    private IEnumerator DisappearRoutine()
    {
        if (bounceVisibleDuration > 0f)
        {
            yield return new WaitForSeconds(
                bounceVisibleDuration
            );
        }

        float elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            float frameTime = Time.deltaTime;
            elapsed += frameTime;

            /*
             * Rigidbody ko yahan freeze nahi karte. Ball/block apna
             * bounce aur gravity naturally complete karta rehta hai,
             * jabke sideways slide aur spin dheere dheere settle hote
             * hain. Is se shrink start hote hi visible hard-stop nahi
             * aata.
             */
            if (body != null &&
                !body.isKinematic)
            {
                float damping = Mathf.Exp(
                    -settleDamping * frameTime
                );

                Vector3 velocity =
                    GetBodyVelocity(body);

                float verticalVelocity =
                    Vector3.Dot(
                        velocity,
                        Vector3.up
                    );

                Vector3 sidewaysVelocity =
                    velocity -
                    Vector3.up * verticalVelocity;

                SetBodyVelocity(
                    body,
                    sidewaysVelocity * damping +
                    Vector3.up * verticalVelocity
                );

                body.angularVelocity *= damping;
            }

            float progress = Mathf.Clamp01(
                elapsed / shrinkDuration
            );

            float remainingScale =
                1f - Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            transform.localScale =
                scaleBeforeDisappear * remainingScale;

            yield return null;
        }

        transform.localScale = Vector3.zero;

        /*
         * Physics sirf us waqt stop hoti hai jab object visually zero
         * scale ho chuka hota hai, isliye player ko snap nazar nahi aata.
         */
        if (body != null)
        {
            SetBodyVelocity(body, Vector3.zero);
            body.angularVelocity = Vector3.zero;
            body.detectCollisions = false;
            body.useGravity = false;
            body.isKinematic = true;
        }

        disappearRoutine = null;

        Completed?.Invoke(this);

        if (destroyOnComplete &&
            this != null &&
            gameObject != null)
        {
            Destroy(gameObject);
        }
    }


    private IEnumerator SmoothFallbackRoutine(
        float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        fallbackRoutine = null;

        if (!isDisappearing)
        {
            TryBeginDisappear(Vector3.up);
        }
    }


    private static bool IsMarkedLowerGround(
        Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return false;
        }

        if (targetCollider.GetComponentInParent<LevelClearZone>() != null)
        {
            return true;
        }

        LevelTable levelTable =
            targetCollider.GetComponentInParent<LevelTable>();

        return
            levelTable != null &&
            targetCollider != levelTable.TowerSurfaceCollider;
    }


    private static bool IsUnmarkedStaticGround(
        Collision collision,
        Vector3 contactNormal)
    {
        if (collision == null ||
            collision.collider == null ||
            (collision.rigidbody != null &&
             !collision.rigidbody.isKinematic) ||
            contactNormal.y < 0.35f)
        {
            return false;
        }

        if (collision.collider.GetComponentInParent<
                PhysicsTowerObject>() != null)
        {
            return false;
        }

        /*
         * A LevelTable top is deliberately not an exit surface. Its
         * lower/base colliders were already accepted above.
         */
        LevelTable levelTable =
            collision.collider.GetComponentInParent<LevelTable>();

        return
            levelTable == null ||
            collision.collider != levelTable.TowerSurfaceCollider;
    }


    private static Vector3 GetBodyVelocity(
        Rigidbody targetBody)
    {
#if UNITY_6000_0_OR_NEWER
        return targetBody.linearVelocity;
#else
        return targetBody.velocity;
#endif
    }


    private static void SetBodyVelocity(
        Rigidbody targetBody,
        Vector3 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        targetBody.linearVelocity = velocity;
#else
        targetBody.velocity = velocity;
#endif
    }
}
