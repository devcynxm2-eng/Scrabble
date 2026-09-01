using UnityEngine;

public sealed class LevelTable : MonoBehaviour
{
    [Header("Tower Surface")]

    [Tooltip(
        "Sirf table ki TOP playing surface ka BoxCollider assign karein. " +
        "Tower isi collider ki upper surface se spawn hoga."
    )]
    [SerializeField]
    private BoxCollider towerSurfaceCollider;

    [Header("Runtime Rotation")]

    [Tooltip(
        "Optional. Table ka sirf UPAR wala hissa (top surface + uske " +
        "visuals) jise runtime rotation par ghoomna chahiye. Assign " +
        "karne par neeche wali stick/leg khari rehti hai aur sirf top " +
        "apne tower ke sath ghoomta hai.\n\n" +
        "Khali chhorne par poora table ghoomta hai — yani purana behaviour."
    )]
    [SerializeField]
    private Transform rotatingRoot;


    private Quaternion initialRotatingRootRotation =
        Quaternion.identity;

    private bool hasCapturedRotatingRoot;


    public BoxCollider TowerSurfaceCollider =>
        towerSurfaceCollider;

    public Transform RotatingRoot =>
        rotatingRoot;


    /// <summary>
    /// Runtime rotation kis transform par apply hogi.
    ///
    /// Rotating Root assign ho to sirf wohi ghoomta hai (neeche wali
    /// stick khari rehti hai). Warna poora table — yani woh behaviour
    /// jo saare purane table prefabs ka hai.
    /// </summary>
    public Transform RotationPivot =>
        rotatingRoot != null
            ? rotatingRoot
            : transform;


    private void Awake()
    {
        CaptureRotatingRootRotation();
    }


    private void CaptureRotatingRootRotation()
    {
        if (hasCapturedRotatingRoot ||
            rotatingRoot == null)
        {
            return;
        }

        initialRotatingRootRotation =
            rotatingRoot.localRotation;

        hasCapturedRotatingRoot = true;
    }


    /// <summary>
    /// Top hisse ki rotation wapas authored pose par le aata hai.
    ///
    /// Tables pool ki tarah reuse hote hain, is liye ye zaroori hai —
    /// warna pichhle level ka jama shuda spin agle level mein bhi
    /// nazar aata.
    /// </summary>
    public void ResetRotatingRoot()
    {
        if (rotatingRoot == null)
        {
            return;
        }

        CaptureRotatingRootRotation();

        rotatingRoot.localRotation =
            initialRotatingRootRotation;
    }

    /// <summary>
    /// Table ki actual upper surface ka world-space center return karta hai.
    ///
    /// Table prefab ka pivot/origin kahin bhi ho sakta hai.
    /// Tower placement us pivot par depend nahi karegi.
    /// </summary>
    public bool TryGetTowerSurface(
        out Vector3 surfacePosition,
        out Quaternion surfaceRotation)
    {
        surfacePosition = transform.position;
        surfaceRotation = transform.rotation;

        if (towerSurfaceCollider == null)
        {
            Debug.LogError(
                "LevelTable: Tower Surface Collider assign nahi hai.",
                this
            );

            return false;
        }

        Transform colliderTransform =
            towerSurfaceCollider.transform;

        /*
         * BoxCollider ki local-space top-center position.
         */
        Vector3 localTopCenter =
            towerSurfaceCollider.center +
            Vector3.up *
            (towerSurfaceCollider.size.y * 0.5f);

        /*
         * Actual world-space table top.
         * TransformPoint collider ki scale/rotation bhi handle karta hai.
         */
        surfacePosition =
            colliderTransform.TransformPoint(
                localTopCenter
            );

        surfaceRotation =
            colliderTransform.rotation;

        return true;
    }

    private void Reset()
    {
        /*
         * Convenience only.
         *
         * Agar root/children mein BoxCollider ho
         * to automatically first collider pick karega.
         *
         * Inspector mein afterwards correct TABLE TOP
         * collider verify zaroor karein.
         */
        if (towerSurfaceCollider == null)
        {
            towerSurfaceCollider =
                GetComponentInChildren<BoxCollider>();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (towerSurfaceCollider == null)
        {
            return;
        }

        Transform colliderTransform =
            towerSurfaceCollider.transform;

        Vector3 localTopCenter =
            towerSurfaceCollider.center +
            Vector3.up *
            (towerSurfaceCollider.size.y * 0.5f);

        Vector3 worldTopCenter =
            colliderTransform.TransformPoint(
                localTopCenter
            );

        Gizmos.DrawSphere(
            worldTopCenter,
            0.08f
        );

        Gizmos.DrawLine(
            worldTopCenter,
            worldTopCenter +
            colliderTransform.up * 0.5f
        );
    }
#endif
}