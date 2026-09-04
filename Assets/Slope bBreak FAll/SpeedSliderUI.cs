using UnityEngine;
using UnityEngine.UI;

public class SpeedSliderUI : MonoBehaviour
{
    [Header("References")]
    public Rigidbody ballRigidbody;
    public Slider speedSlider;

    [Header("Speed Range")]
    public float maxSpeed = 20f;   // is speed pe slider full ho jayega

    void Update()
    {
        if (ballRigidbody == null || speedSlider == null) return;

        // Ball ki total velocity (magnitude) — downhill speed
        float currentSpeed = ballRigidbody.linearVelocity.magnitude;

        // Slider 0 to 1 range me normalize karo
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);

        speedSlider.value = normalizedSpeed;
    }
}