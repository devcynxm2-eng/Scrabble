using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class BallSliderSpeed : MonoBehaviour
{
    private Rigidbody ballRigidbody;
    private float baseSpeed;
    private float downwardAcceleration;
    private float groundedUntil;
    private Vector3 groundNormal = Vector3.up;
    private bool configured;

    public float BaseSpeed => baseSpeed;

    public void Configure(
        Rigidbody targetRigidbody,
        float selectedSpeed,
        float extraDownwardAcceleration)
    {
        ballRigidbody = targetRigidbody;
        baseSpeed = Mathf.Max(0.01f, selectedSpeed);
        downwardAcceleration = Mathf.Max(0f, extraDownwardAcceleration);
        ballRigidbody.useGravity = true;
        ballRigidbody.linearDamping = 0f;
        ballRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        ballRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        configured = true;
        enabled = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision == null)
        {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            if (Vector3.Dot(normal, Vector3.up) > 0.15f)
            {
                groundNormal = normal.normalized;
                groundedUntil = Time.time + 0.1f;
                return;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!configured || ballRigidbody == null)
        {
            return;
        }

        // Stronger downward pull removes the hovering/floaty feeling. Gravity is
        // still enabled, so steeper slopes naturally produce more acceleration.
        ballRigidbody.AddForce(
            Vector3.down * downwardAcceleration,
            ForceMode.Acceleration);

        if (Time.time > groundedUntil)
        {
            return;
        }

        Vector3 velocity = ballRigidbody.linearVelocity;
        Vector3 surfaceVelocity =
            Vector3.ProjectOnPlane(velocity, groundNormal);
        float surfaceSpeed = surfaceVelocity.magnitude;

        if (surfaceSpeed >= baseSpeed || surfaceSpeed <= 0.01f)
        {
            return;
        }

        Vector3 normalVelocity = velocity - surfaceVelocity;
        float outwardSpeed = Vector3.Dot(normalVelocity, groundNormal);

        if (outwardSpeed > 0f)
        {
            normalVelocity -= groundNormal * outwardSpeed;
        }

        // Restore base speed only along the contacted surface. Airborne velocity
        // is never amplified, so the ball cannot be held up unnaturally.
        ballRigidbody.linearVelocity =
            surfaceVelocity.normalized * baseSpeed + normalVelocity;
    }
}
