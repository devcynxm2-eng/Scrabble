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

    public BoxCollider TowerSurfaceCollider =>
        towerSurfaceCollider;

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


    /// <summary>
    /// Tower top collider ka actual world-space X/Z usable size.
    /// Generator is se ensure karta hai ke structure table ke bahar na ho.
    /// </summary>
    public bool TryGetTowerSurfaceSize(
        out Vector2 surfaceSize)
    {
        surfaceSize = Vector2.zero;

        if (towerSurfaceCollider == null)
        {
            return false;
        }

        Vector3 scale =
            towerSurfaceCollider.transform.lossyScale;

        surfaceSize = new Vector2(
            Mathf.Abs(
                towerSurfaceCollider.size.x * scale.x
            ),
            Mathf.Abs(
                towerSurfaceCollider.size.z * scale.z
            )
        );

        return surfaceSize.x > 0.001f &&
               surfaceSize.y > 0.001f;
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
