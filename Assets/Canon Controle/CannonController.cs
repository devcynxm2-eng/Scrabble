//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.EventSystems;

///*
// * IMPORTANT SETUP NOTE:
// * Ye script ab kisi UI Image/Canvas element par attach karne ki zaroorat
// * NAHI hai. Isay kisi bhi plain GameObject (jaise "GameManagers" ya khud
// * cannon ke root) par attach karein. Ye ab UI Event System (IPointerDownHandler
// * waghera) use nahi karti � is liye Canvas kabhi bhi aim/shoot ko intercept
// * nahi kar sakta.
// *
// * Requires: Project Settings > Player > Active Input Handling =
// * "Input System Package (New)" ya "Both", aur com.unity.inputsystem package
// * installed hona chahiye.
// */
//public sealed class CannonController : MonoBehaviour
//{
//    [Header("Cannon References")]
//    [SerializeField] private Transform yawPivot;
//    [SerializeField] private Transform pitchPivot;
//    [SerializeField] private Transform muzzle;

//    [Tooltip("Complete cannon root. Ball cannon colliders ko ignore karegi.")]
//    [SerializeField] private Transform cannonRoot;

//    [Header("Camera And Exact Targeting")]
//    [SerializeField] private Camera aimCamera;

//    [Tooltip(
//        "TowerCan layer select karein. Block par click hone par isi mask ko " +
//        "sab se pehle check kiya jayega."
//    )]
//    [SerializeField] private LayerMask towerTargetMask;

//    [Tooltip(
//        "Table, Ground aur Environment layers. TowerCan ko bhi include kar sakte hain. " +
//        "UI layer yahan KABHI include na karein."
//    )]
//    [SerializeField] private LayerMask fallbackAimMask = ~0;

//    [SerializeField, Min(1f)]
//    private float maxAimDistance = 500f;

//    [Tooltip(
//        "Ray kisi collider ko hit na kare to tower ki depth par point calculate hoga. " +
//        "Tower center par empty Transform bana kar assign karein."
//    )]
//    [SerializeField] private Transform fallbackAimPlaneReference;

//    [SerializeField, Min(1f)]
//    private float fallbackAimDistance = 100f;

//    [Header("UI Interaction Guard")]
//    [Tooltip(
//        "ON hone par agar pointer kisi UI element (button, panel, HUD) ke " +
//        "upar ho to cannon aim/shoot bilkul trigger nahi hoga. Isay ON rakhein " +
//        "taake HUD buttons dabane par galti se shot na chal jaye."
//    )]
//    [SerializeField] private bool ignoreInputOverUI = true;

//    [Header("Forward Only")]
//    [SerializeField] private bool preventBackwardAim = true;

//    [Header("Aim Limits")]
//    [SerializeField] private float minimumPitch = -10f;
//    [SerializeField] private float maximumPitch = 70f;

//    [Header("Shooting")]
//    [SerializeField] private GameObject cannonBallPrefab;
//    [SerializeField, Min(0f)] private float launchSpeed = 30f;

//    [Tooltip("Ball muzzle se itna aage spawn hogi.")]
//    [SerializeField, Min(0f)]
//    private float spawnForwardOffset = 0.20f;

//    [SerializeField, Min(0f)]
//    private float cannonBallLifetime = 5f;

//    [SerializeField, Min(0f)]
//    private float minimumShotInterval = 0f;

//    [Header("Trajectory")]
//    [Tooltip(
//        "ON: gravity off, ball seedhi line mein exact clicked point tak jayegi. " +
//        "OFF: gravity on rahegi aur script khud sahi arc/angle calculate karke " +
//        "wahi exact clicked point hit karegi (agar us speed se pohonchna possible ho)."
//    )]
//    [SerializeField] private bool exactStraightShot = true;

//    [Tooltip(
//        "Arc mode mein 2 possible trajectories hoti hain (low arc / high arc). " +
//        "True = neechi seedhi trajectory (fast, cannon jaisi). False = oonchi lobbed trajectory."
//    )]
//    [SerializeField] private bool preferLowArcTrajectory = true;

//    [Tooltip(
//        "Arc mode mein agar target current Launch Speed se physically reach hi na ho " +
//        "(bohat door ya bohat oopar), to fallback ke taur par seedhi line mein shot chalegi " +
//        "taake ball kabhi bhi target miss na kare."
//    )]
//    [SerializeField] private bool fallbackToStraightIfUnreachable = true;

//    [Header("Bottom Dead Space")]
//    [SerializeField, Min(0f)]
//    private float bottomDeadSpaceHeight = 250f;

//    [Header("Debug")]
//    [SerializeField] private bool showDebugLogs = true;
//    [SerializeField] private bool showDebugLines = true;

//    private Vector3 currentAimPoint;
//    private bool hasAimPoint;
//    private bool pointerStartedInDeadSpace;
//    private bool pointerStartedOverUI;
//    private bool isPointerHeld;
//    private float nextAllowedShotTime;

//    private Collider[] cannonColliders;

//    private void Awake()
//    {
//        if (aimCamera == null)
//        {
//            aimCamera = Camera.main;
//        }

//        if (cannonRoot == null)
//        {
//            cannonRoot = transform;
//        }

//        CacheCannonColliders();


//        QualitySettings.vSyncCount = 0;
//        Application.targetFrameRate = 60;


//    }

//    private void CacheCannonColliders()
//    {
//        cannonColliders =
//            cannonRoot != null
//                ? cannonRoot.GetComponentsInChildren<Collider>(true)
//                : new Collider[0];
//    }

//    private void Update()
//    {
//        Pointer activePointer =
//            Pointer.current;

//        if (activePointer == null)
//        {
//            return;
//        }

//        Vector2 screenPosition =
//            activePointer.position.ReadValue();

//        if (activePointer.press.wasPressedThisFrame)
//        {
//            HandlePointerDown(screenPosition);
//        }
//        else if (activePointer.press.isPressed &&
//                 isPointerHeld)
//        {
//            HandlePointerDrag(screenPosition);
//        }

//        if (activePointer.press.wasReleasedThisFrame &&
//            isPointerHeld)
//        {
//            HandlePointerUp(screenPosition);
//        }
//    }

//    private void HandlePointerDown(Vector2 screenPosition)
//    {
//        isPointerHeld = true;

//        pointerStartedOverUI =
//            ignoreInputOverUI &&
//            IsPointerOverUI();

//        pointerStartedInDeadSpace =
//            IsInsideDeadSpace(screenPosition);

//        if (pointerStartedOverUI ||
//            pointerStartedInDeadSpace)
//        {
//            hasAimPoint = false;
//            return;
//        }

//        UpdateAim(screenPosition);
//    }

//    private void HandlePointerDrag(Vector2 screenPosition)
//    {
//        if (pointerStartedOverUI ||
//            pointerStartedInDeadSpace ||
//            IsInsideDeadSpace(screenPosition))
//        {
//            return;
//        }

//        UpdateAim(screenPosition);
//    }

//    private void HandlePointerUp(Vector2 screenPosition)
//    {
//        isPointerHeld = false;

//        bool blocked =
//            pointerStartedOverUI ||
//            pointerStartedInDeadSpace ||
//            IsInsideDeadSpace(screenPosition);

//        pointerStartedOverUI = false;
//        pointerStartedInDeadSpace = false;

//        if (blocked)
//        {
//            hasAimPoint = false;
//            return;
//        }

//        /*
//         * Release position se exact ray dobara banegi.
//         */
//        if (UpdateAim(screenPosition))
//        {
//            Shoot();
//        }
//    }

//    /// <summary>
//    /// Pointer kisi UI element (Canvas Graphic jispar Raycast Target on ho)
//    /// ke upar hai ya nahi � is check ki wajah se HUD/buttons ke upar
//    /// tap karne par cannon galti se fire nahi karega, aur UI se koi
//    /// interference bhi nahi hogi kyunke ye sirf ek guard hai, 3D aim
//    /// calculation UI raycast par depend nahi karti.
//    /// </summary>
//    private bool IsPointerOverUI()
//    {
//        if (EventSystem.current == null)
//        {
//            return false;
//        }

//        return EventSystem.current.IsPointerOverGameObject();
//    }

//    private bool IsInsideDeadSpace(Vector2 screenPosition)
//    {
//        return screenPosition.y <= bottomDeadSpaceHeight;
//    }

//    private bool UpdateAim(Vector2 screenPosition)
//    {
//        if (!ValidateReferences())
//        {
//            hasAimPoint = false;
//            return false;
//        }

//        Ray screenRay =
//            aimCamera.ScreenPointToRay(screenPosition);

//        /*
//         * Sab se pehle sirf TowerCan layer check hoti hai.
//         * Is se table, backdrop ya kisi large collider ka hit block ke hit ko
//         * replace nahi karega.
//         */
//        if (!TryGetClosestValidHit(
//                screenRay,
//                towerTargetMask,
//                true,
//                out RaycastHit chosenHit))
//        {
//            /*
//             * Tower hit na ho to normal environment hit use karo.
//             */
//            if (!TryGetClosestValidHit(
//                    screenRay,
//                    fallbackAimMask,
//                    false,
//                    out chosenHit))
//            {
//                currentAimPoint =
//                    GetFallbackAimPoint(screenRay);
//            }
//            else
//            {
//                currentAimPoint = chosenHit.point;
//            }
//        }
//        else
//        {
//            currentAimPoint = chosenHit.point;
//        }

//        if (preventBackwardAim &&
//            IsPointBehindCannon(currentAimPoint))
//        {
//            hasAimPoint = false;

//            if (showDebugLogs)
//            {
//                Debug.Log(
//                    "Rear-side click ignored. Cannon forward side mein hi aim karega.",
//                    this
//                );
//            }

//            return false;
//        }

//        /*
//         * Original turning formulas unchanged.
//         */
//        RotateCannonTowards(currentAimPoint);

//        Physics.SyncTransforms();

//        Vector3 finalDirection =
//            currentAimPoint - muzzle.position;

//        if (finalDirection.sqrMagnitude < 0.0001f)
//        {
//            hasAimPoint = false;
//            return false;
//        }

//        hasAimPoint = true;

//        if (showDebugLines)
//        {
//            Debug.DrawLine(
//                muzzle.position,
//                currentAimPoint,
//                Color.red,
//                0.25f
//            );
//        }

//        if (showDebugLogs)
//        {
//            string hitName =
//                chosenHit.collider != null
//                    ? chosenHit.collider.name
//                    : "Fallback point";

//            Debug.Log(
//                $"Aim target: {hitName} | Point: {currentAimPoint}",
//                this
//            );
//        }

//        return true;
//    }

//    private bool TryGetClosestValidHit(
//        Ray ray,
//        LayerMask mask,
//        bool requireTowerTarget,
//        out RaycastHit closestHit)
//    {
//        closestHit = default;

//        if (mask.value == 0)
//        {
//            return false;
//        }

//        RaycastHit[] hits =
//            Physics.RaycastAll(
//                ray,
//                maxAimDistance,
//                mask,
//                QueryTriggerInteraction.Ignore
//            );

//        bool found = false;
//        float closestDistance = float.MaxValue;

//        foreach (RaycastHit hit in hits)
//        {
//            if (hit.collider == null ||
//                IsCannonCollider(hit.collider))
//            {
//                continue;
//            }

//            if (requireTowerTarget &&
//                !IsTowerTarget(hit.collider))
//            {
//                continue;
//            }

//            if (hit.distance >= closestDistance)
//            {
//                continue;
//            }

//            closestDistance = hit.distance;
//            closestHit = hit;
//            found = true;
//        }

//        return found;
//    }

//    private bool IsTowerTarget(Collider hitCollider)
//    {
//        if (hitCollider == null)
//        {
//            return false;
//        }

//        /*
//         * Generated tower root par ye receiver automatically add hota hai.
//         * Is check ki wajah se child collider hit bhi correct can target banega.
//         */
//        if (hitCollider.GetComponentInParent<
//                GeneratedTowerCanHitReceiver>() != null)
//        {
//            return true;
//        }

//        int layerBit = 1 << hitCollider.gameObject.layer;

//        return (towerTargetMask.value & layerBit) != 0;
//    }

//    private bool IsCannonCollider(Collider candidate)
//    {
//        if (candidate == null ||
//            cannonRoot == null)
//        {
//            return false;
//        }

//        Transform candidateTransform =
//            candidate.transform;

//        return
//            candidateTransform == cannonRoot ||
//            candidateTransform.IsChildOf(cannonRoot);
//    }

//    private Vector3 GetFallbackAimPoint(Ray screenRay)
//    {
//        if (fallbackAimPlaneReference != null)
//        {
//            /*
//             * Camera-facing plane tower ki depth par banta hai.
//             */
//            Plane aimPlane =
//                new Plane(
//                    -aimCamera.transform.forward,
//                    fallbackAimPlaneReference.position
//                );

//            if (aimPlane.Raycast(
//                    screenRay,
//                    out float enterDistance) &&
//                enterDistance > 0f)
//            {
//                return screenRay.GetPoint(enterDistance);
//            }
//        }

//        return screenRay.GetPoint(fallbackAimDistance);
//    }

//    private bool IsPointBehindCannon(Vector3 targetPoint)
//    {
//        Transform forwardSpace =
//            yawPivot.parent != null
//                ? yawPivot.parent
//                : transform;

//        Vector3 flatDirection =
//            targetPoint - yawPivot.position;

//        flatDirection.y = 0f;

//        Vector3 flatForward =
//            forwardSpace.forward;

//        flatForward.y = 0f;

//        if (flatDirection.sqrMagnitude < 0.0001f ||
//            flatForward.sqrMagnitude < 0.0001f)
//        {
//            return false;
//        }

//        return Vector3.Dot(
//            flatForward.normalized,
//            flatDirection.normalized
//        ) <= 0f;
//    }

//    private void RotateCannonTowards(Vector3 targetPoint)
//    {
//        if (yawPivot == null ||
//            pitchPivot == null)
//        {
//            return;
//        }

//        RotateYawTowards(targetPoint);
//        RotatePitchTowards(targetPoint);
//    }

//    /*
//     * User ki original left/right turning logic unchanged.
//     */
//    private void RotateYawTowards(Vector3 targetPoint)
//    {
//        Vector3 worldDirection =
//            targetPoint - yawPivot.position;

//        worldDirection.y = 0f;

//        if (worldDirection.sqrMagnitude < 0.001f)
//        {
//            return;
//        }

//        Vector3 localDirection;

//        if (yawPivot.parent != null)
//        {
//            localDirection =
//                yawPivot.parent.InverseTransformDirection(
//                    worldDirection
//                );
//        }
//        else
//        {
//            localDirection = worldDirection;
//        }

//        float yawAngle =
//            Mathf.Atan2(
//                localDirection.x,
//                localDirection.z
//            ) * Mathf.Rad2Deg;

//        yawPivot.localRotation =
//            Quaternion.Euler(
//                0f,
//                yawAngle,
//                0f
//            );
//    }

//    /*
//     * User ki original up/down turning logic unchanged.
//     * Note: ye sirf VISUAL barrel angle set karti hai. Actual launch velocity
//     * (seedhi line ya ballistic arc) Shoot() mein alag se calculate hoti hai,
//     * is liye visual pitch clamp se exact-hit trajectory affect nahi hoti.
//     */
//    private void RotatePitchTowards(Vector3 targetPoint)
//    {
//        Vector3 worldDirection =
//            targetPoint - pitchPivot.position;

//        Vector3 localDirection =
//            yawPivot.InverseTransformDirection(
//                worldDirection
//            );

//        float horizontalDistance =
//            new Vector2(
//                localDirection.x,
//                localDirection.z
//            ).magnitude;

//        float pitchAngle =
//            Mathf.Atan2(
//                localDirection.y,
//                horizontalDistance
//            ) * Mathf.Rad2Deg;

//        pitchAngle =
//            Mathf.Clamp(
//                pitchAngle,
//                minimumPitch,
//                maximumPitch
//            );

//        pitchPivot.localRotation =
//            Quaternion.Euler(
//                -pitchAngle,
//                0f,
//                0f
//            );
//    }

//    public void Shoot()
//    {
//        if (!hasAimPoint ||
//            Time.time < nextAllowedShotTime)
//        {
//            return;
//        }

//        if (cannonBallPrefab == null ||
//            muzzle == null)
//        {
//            return;
//        }

//        Physics.SyncTransforms();

//        Vector3 spawnOrigin =
//            muzzle.position;

//        bool useGravityForShot =
//            !exactStraightShot;

//        Vector3 shotVelocity;

//        if (useGravityForShot)
//        {
//            /*
//             * Gravity ON: fixed launchSpeed ke sath exact clicked point tak
//             * pohonchne wala angle/direction calculate karo.
//             */
//            bool solved =
//                TryCalculateBallisticVelocity(
//                    spawnOrigin,
//                    currentAimPoint,
//                    launchSpeed,
//                    out shotVelocity
//                );

//            if (!solved)
//            {
//                if (!fallbackToStraightIfUnreachable)
//                {
//                    if (showDebugLogs)
//                    {
//                        Debug.LogWarning(
//                            "Target is current Launch Speed se reach nahi ho sakta " +
//                            "(bohat door/oopar hai). Shot cancel.",
//                            this
//                        );
//                    }

//                    return;
//                }

//                /*
//                 * Reach na ho to seedhi line shot fallback (guaranteed hit).
//                 */
//                useGravityForShot = false;

//                Vector3 straightDirection =
//                    currentAimPoint - spawnOrigin;

//                if (straightDirection.sqrMagnitude < 0.0001f)
//                {
//                    return;
//                }

//                shotVelocity =
//                    straightDirection.normalized *
//                    launchSpeed;

//                if (showDebugLogs)
//                {
//                    Debug.LogWarning(
//                        "Target unreachable with current arc/speed. " +
//                        "Straight-line fallback shot fired instead.",
//                        this
//                    );
//                }
//            }
//        }
//        else
//        {
//            Vector3 straightDirection =
//                currentAimPoint - spawnOrigin;

//            if (straightDirection.sqrMagnitude < 0.0001f)
//            {
//                return;
//            }

//            shotVelocity =
//                straightDirection.normalized *
//                launchSpeed;
//        }

//        Vector3 shotDirection =
//            shotVelocity.sqrMagnitude > 0.0001f
//                ? shotVelocity.normalized
//                : (currentAimPoint - spawnOrigin).normalized;

//        if (preventBackwardAim &&
//            IsDirectionBehindCannon(shotDirection))
//        {
//            return;
//        }

//        nextAllowedShotTime =
//            Time.time + minimumShotInterval;

//        Vector3 spawnPosition =
//            spawnOrigin +
//            shotDirection * spawnForwardOffset;

//        Quaternion spawnRotation =
//            Quaternion.LookRotation(
//                shotDirection,
//                GetSafeUpDirection(shotDirection)
//            );

//        GameObject cannonBall =
//            Instantiate(
//                cannonBallPrefab,
//                spawnPosition,
//                spawnRotation
//            );

//        Rigidbody ballRigidbody =
//            cannonBall.GetComponent<Rigidbody>();

//        if (ballRigidbody == null)
//        {
//            Debug.LogError(
//                "Cannonball prefab par Rigidbody missing hai.",
//                cannonBall
//            );

//            Destroy(cannonBall);
//            return;
//        }

//        ballRigidbody.isKinematic = false;
//        ballRigidbody.useGravity = useGravityForShot;
//        ballRigidbody.angularVelocity = Vector3.zero;
//        ballRigidbody.collisionDetectionMode =
//            CollisionDetectionMode.ContinuousDynamic;

//        SetBodyVelocity(ballRigidbody, Vector3.zero);

//        IgnoreBallCollisionWithCannon(cannonBall);

//        SetBodyVelocity(
//            ballRigidbody,
//            shotVelocity
//        );

//        if (showDebugLines)
//        {
//            Debug.DrawLine(
//                spawnPosition,
//                currentAimPoint,
//                Color.green,
//                2f
//            );
//        }

//        Destroy(
//            cannonBall,
//            cannonBallLifetime
//        );
//    }

//    /// <summary>
//    /// Fixed launch speed par, gravity ke sath, exact target tak pohonchne wali
//    /// launch velocity calculate karta hai (standard projectile motion formula).
//    /// Do solutions hoti hain (low arc / high arc); preferLowArcTrajectory se
//    /// select hoti hai. Agar target current speed se reach hi na ho (discriminant
//    /// negative) to false return hoti hai.
//    /// </summary>
//    private bool TryCalculateBallisticVelocity(
//        Vector3 startPosition,
//        Vector3 targetPosition,
//        float speed,
//        out Vector3 launchVelocity)
//    {
//        launchVelocity = Vector3.zero;

//        if (speed <= 0.01f)
//        {
//            return false;
//        }

//        float gravity =
//            Mathf.Abs(Physics.gravity.y);

//        if (gravity <= 0.001f)
//        {
//            return false;
//        }

//        Vector3 toTarget =
//            targetPosition - startPosition;

//        Vector3 horizontalToTarget =
//            new Vector3(
//                toTarget.x,
//                0f,
//                toTarget.z
//            );

//        float horizontalDistance =
//            horizontalToTarget.magnitude;

//        float verticalDistance =
//            toTarget.y;

//        if (horizontalDistance < 0.0001f)
//        {
//            /*
//             * Target seedha upar/neeche hai � horizontal solve nahi hota.
//             * Straight vertical shot bhej do.
//             */
//            launchVelocity =
//                new Vector3(
//                    0f,
//                    Mathf.Sign(verticalDistance == 0f ? 1f : verticalDistance) * speed,
//                    0f
//                );

//            return true;
//        }

//        float speedSquared =
//            speed * speed;

//        float discriminant =
//            (speedSquared * speedSquared) -
//            gravity *
//            (gravity * horizontalDistance * horizontalDistance +
//             2f * verticalDistance * speedSquared);

//        if (discriminant < 0f)
//        {
//            /*
//             * Is speed se target physically reach nahi ho sakta.
//             */
//            return false;
//        }

//        float sqrtDiscriminant =
//            Mathf.Sqrt(discriminant);

//        float lowArcAngle =
//            Mathf.Atan2(
//                speedSquared - sqrtDiscriminant,
//                gravity * horizontalDistance
//            );

//        float highArcAngle =
//            Mathf.Atan2(
//                speedSquared + sqrtDiscriminant,
//                gravity * horizontalDistance
//            );

//        float chosenAngle =
//            preferLowArcTrajectory
//                ? lowArcAngle
//                : highArcAngle;

//        Vector3 horizontalDirection =
//            horizontalToTarget.normalized;

//        float horizontalSpeed =
//            speed * Mathf.Cos(chosenAngle);

//        float verticalSpeed =
//            speed * Mathf.Sin(chosenAngle);

//        launchVelocity =
//            horizontalDirection * horizontalSpeed +
//            Vector3.up * verticalSpeed;

//        return true;
//    }

//    private bool IsDirectionBehindCannon(Vector3 direction)
//    {
//        Transform forwardSpace =
//            yawPivot.parent != null
//                ? yawPivot.parent
//                : transform;

//        Vector3 flatDirection = direction;
//        flatDirection.y = 0f;

//        Vector3 flatForward = forwardSpace.forward;
//        flatForward.y = 0f;

//        if (flatDirection.sqrMagnitude < 0.0001f ||
//            flatForward.sqrMagnitude < 0.0001f)
//        {
//            return false;
//        }

//        return Vector3.Dot(
//            flatForward.normalized,
//            flatDirection.normalized
//        ) <= 0f;
//    }

//    private Vector3 GetSafeUpDirection(Vector3 shotDirection)
//    {
//        Vector3 upDirection =
//            muzzle != null
//                ? muzzle.up
//                : Vector3.up;

//        if (Mathf.Abs(
//                Vector3.Dot(
//                    shotDirection,
//                    upDirection.normalized
//                )) > 0.999f)
//        {
//            upDirection = Vector3.up;
//        }

//        return upDirection;
//    }

//    private void IgnoreBallCollisionWithCannon(GameObject cannonBall)
//    {
//        if (cannonBall == null)
//        {
//            return;
//        }

//        if (cannonColliders == null ||
//            cannonColliders.Length == 0)
//        {
//            CacheCannonColliders();
//        }

//        Collider[] ballColliders =
//            cannonBall.GetComponentsInChildren<Collider>(true);

//        foreach (Collider ballCollider in ballColliders)
//        {
//            if (ballCollider == null)
//            {
//                continue;
//            }

//            foreach (Collider cannonCollider in cannonColliders)
//            {
//                if (cannonCollider == null)
//                {
//                    continue;
//                }

//                Physics.IgnoreCollision(
//                    ballCollider,
//                    cannonCollider,
//                    true
//                );
//            }
//        }
//    }

//    private bool ValidateReferences()
//    {
//        if (aimCamera == null)
//        {
//            Debug.LogError("Aim Camera missing hai.", this);
//            return false;
//        }

//        if (yawPivot == null ||
//            pitchPivot == null ||
//            muzzle == null)
//        {
//            Debug.LogError(
//                "Yaw Pivot, Pitch Pivot ya Muzzle missing hai.",
//                this
//            );

//            return false;
//        }

//        return true;
//    }

//    private static void SetBodyVelocity(
//        Rigidbody body,
//        Vector3 velocity)
//    {
//#if UNITY_6000_0_OR_NEWER
//        body.linearVelocity = velocity;
//#else
//        body.velocity = velocity;
//#endif
//    }

//#if UNITY_EDITOR
//    private void OnDrawGizmosSelected()
//    {
//        if (!showDebugLines ||
//            !hasAimPoint ||
//            muzzle == null)
//        {
//            return;
//        }

//        Gizmos.DrawLine(
//            muzzle.position,
//            currentAimPoint
//        );

//        Gizmos.DrawSphere(
//            currentAimPoint,
//            0.08f
//        );
//    }
//#endif
//}






// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using TMPro;

// public sealed class CannonController : MonoBehaviour
// {
//     [Header("Cannon References")]
//     [SerializeField] private Transform yawPivot;
//     [SerializeField] private Transform pitchPivot;
//     [SerializeField] private Transform muzzle;

//     [Tooltip(
//         "Complete cannon root. Ball cannon colliders ko ignore karegi."
//     )]
//     [SerializeField] private Transform cannonRoot;


//     [Header("Camera And Exact Targeting")]
//     [SerializeField] private Camera aimCamera;

//     [Tooltip(
//         "TowerTarget layer select karein. " +
//         "Block par click hone par isi mask ko sab se pehle check kiya jayega."
//     )]
//     [SerializeField] private LayerMask towerTargetMask;

//     [Tooltip(
//         "Table, Ground aur Environment layers. " +
//         "UI layer yahan include na karein."
//     )]
//     [SerializeField] private LayerMask fallbackAimMask = ~0;

//     [SerializeField, Min(1f)]
//     private float maxAimDistance = 500f;

//     [Tooltip(
//         "Ray kisi collider ko hit na kare to tower ki depth par " +
//         "point calculate hoga."
//     )]
//     [SerializeField]
//     private Transform fallbackAimPlaneReference;

//     [SerializeField, Min(1f)]
//     private float fallbackAimDistance = 100f;


//     [Header("UI Interaction Guard")]

//     [Tooltip(
//         "Pointer UI ke upar ho to cannon aim/shoot trigger nahi hoga."
//     )]
//     [SerializeField]
//     private bool ignoreInputOverUI = true;


//     [Header("Forward Only")]
//     [SerializeField]
//     private bool preventBackwardAim = true;


//     [Header("Aim Limits")]
//     [SerializeField] private float minimumPitch = -10f;
//     [SerializeField] private float maximumPitch = 70f;


//     [Header("Shooting")]
//     [SerializeField] private GameObject cannonBallPrefab;

//     [SerializeField, Min(0f)]
//     private float launchSpeed = 30f;

//     [Tooltip("Ball muzzle se itna aage spawn hogi.")]
//     [SerializeField, Min(0f)]
//     private float spawnForwardOffset = 0.20f;

//     [SerializeField, Min(0f)]
//     private float cannonBallLifetime = 5f;

//     [SerializeField, Min(0f)]
//     private float minimumShotInterval = 0f;


//     [Header("Ball Limit And HUD")]

//     [SerializeField]
//     private LevelRuntimeController levelRuntimeController;

//     [Tooltip(
//         "Optional TMP text. Empty ho to top-left HUD automatically create hoga."
//     )]
//     [SerializeField]
//     private TMP_Text ballsRemainingText;

//     [SerializeField]
//     private bool autoCreateBallsHud = true;

//     [SerializeField, Min(1)]
//     private int fallbackBallLimit = 30;


//     [Header("Trajectory")]

//     [Tooltip(
//         "ON: gravity off aur straight exact shot. " +
//         "OFF: gravity ke sath ballistic trajectory."
//     )]
//     [SerializeField]
//     private bool exactStraightShot = true;

//     [Tooltip(
//         "True = low arc. False = high arc."
//     )]
//     [SerializeField]
//     private bool preferLowArcTrajectory = true;

//     [Tooltip(
//         "Arc se target unreachable ho to straight shot fallback."
//     )]
//     [SerializeField]
//     private bool fallbackToStraightIfUnreachable = true;


//     [Header("Bottom Dead Space")]

//     [Tooltip(
//         "Screen ke bottom se pixels. " +
//         "Is area mein aim/shoot nahi chalega. " +
//         "DeadZoneVisual automatically isi value ke sath sync hogi."
//     )]
//     [SerializeField, Min(0f)]
//     private float bottomDeadSpaceHeight = 250f;


//     [Header("Debug")]
//     [SerializeField] private bool showDebugLogs = true;
//     [SerializeField] private bool showDebugLines = true;


//     /*
//      * DeadZoneVisual isi property ko read karegi.
//      *
//      * Isliye dead-zone height sirf CannonController
//      * mein maintain karni hai.
//      */
//     public float BottomDeadSpaceHeight =>
//         bottomDeadSpaceHeight;


//     private Vector3 currentAimPoint;

//     private bool hasAimPoint;
//     private bool pointerStartedInDeadSpace;
//     private bool pointerStartedOverUI;
//     private bool isPointerHeld;

//     private float nextAllowedShotTime;

//     private Collider[] cannonColliders;

//     private LevelRuntimeController subscribedLevelController;
//     private GridLevelData activeBallLevel;
//     private GameObject autoCreatedBallsHud;
//     private int totalBalls;
//     private int remainingBalls;
//     private bool ballLimitInitialized;

//     public int RemainingBalls => remainingBalls;
//     public int TotalBalls => totalBalls;


//     public void SetGameplayActive(bool active)
//     {
//         if (!active)
//         {
//             isPointerHeld = false;
//             hasAimPoint = false;
//         }

//         if (cannonRoot != null)
//         {
//             cannonRoot.gameObject.SetActive(active);
//         }

//         if (ballsRemainingText != null)
//         {
//             ballsRemainingText.gameObject.SetActive(active);
//         }

//         enabled = active;
//     }


//     private void Awake()
//     {
//         if (aimCamera == null)
//         {
//             aimCamera = Camera.main;
//         }

//         if (cannonRoot == null)
//         {
//             cannonRoot = transform;
//         }

//         CacheCannonColliders();
//     }


//     private void OnEnable()
//     {
//         ConnectLevelController();
//     }


//     private void Start()
//     {
//         ConnectLevelController();
//         EnsureBallsHud();

//         ResetBallLimit(
//             levelRuntimeController != null
//                 ? levelRuntimeController.CurrentLevelData
//                 : null
//         );
//     }


//     private void OnDisable()
//     {
//         DisconnectLevelController();
//     }


//     private void ConnectLevelController()
//     {
//         if (levelRuntimeController == null)
//         {
//             levelRuntimeController =
//                 FindFirstObjectByType<LevelRuntimeController>();
//         }

//         if (subscribedLevelController ==
//             levelRuntimeController)
//         {
//             return;
//         }

//         DisconnectLevelController();

//         subscribedLevelController =
//             levelRuntimeController;

//         if (subscribedLevelController != null)
//         {
//             subscribedLevelController.LevelGenerated +=
//                 HandleLevelGenerated;
//         }
//     }


//     private void DisconnectLevelController()
//     {
//         if (subscribedLevelController != null)
//         {
//             subscribedLevelController.LevelGenerated -=
//                 HandleLevelGenerated;
//         }

//         subscribedLevelController = null;
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         ResetBallLimit(generatedLevel);
//     }


//     private void EnsureBallLimitInitialized()
//     {
//         GridLevelData currentLevel =
//             levelRuntimeController != null
//                 ? levelRuntimeController.CurrentLevelData
//                 : null;

//         if (!ballLimitInitialized ||
//             currentLevel != activeBallLevel)
//         {
//             ResetBallLimit(currentLevel);
//         }
//     }


//     private void ResetBallLimit(
//         GridLevelData ballLevel)
//     {
//         activeBallLevel = ballLevel;

//         totalBalls = Mathf.Max(
//             1,
//             ballLevel != null
//                 ? ballLevel.AvailableBalls
//                 : fallbackBallLimit
//         );

//         remainingBalls = totalBalls;
//         ballLimitInitialized = true;

//         EnsureBallsHud();
//         UpdateBallsHud();
//     }


//     private void ConsumeBall()
//     {
//         remainingBalls = Mathf.Max(
//             0,
//             remainingBalls - 1
//         );

//         UpdateBallsHud();
//     }


//     private void EnsureBallsHud()
//     {
//         if (ballsRemainingText != null ||
//             !autoCreateBallsHud)
//         {
//             return;
//         }

//         Canvas targetCanvas = null;

//         Canvas[] canvases =
//             FindObjectsByType<Canvas>(
//                 FindObjectsInactive.Exclude,
//                 FindObjectsSortMode.None
//             );

//         foreach (Canvas candidate in canvases)
//         {
//             if (candidate != null &&
//                 candidate.renderMode !=
//                 RenderMode.WorldSpace)
//             {
//                 targetCanvas = candidate;
//                 break;
//             }
//         }

//         if (targetCanvas == null)
//         {
//             GameObject canvasObject =
//                 new GameObject(
//                     "Cannon HUD Canvas",
//                     typeof(RectTransform),
//                     typeof(Canvas),
//                     typeof(CanvasScaler),
//                     typeof(GraphicRaycaster)
//                 );

//             int uiLayer =
//                 LayerMask.NameToLayer("UI");

//             if (uiLayer >= 0)
//             {
//                 canvasObject.layer = uiLayer;
//             }

//             targetCanvas =
//                 canvasObject.GetComponent<Canvas>();

//             targetCanvas.renderMode =
//                 RenderMode.ScreenSpaceOverlay;

//             targetCanvas.sortingOrder = 100;

//             CanvasScaler scaler =
//                 canvasObject.GetComponent<CanvasScaler>();

//             scaler.uiScaleMode =
//                 CanvasScaler.ScaleMode.ScaleWithScreenSize;

//             scaler.referenceResolution =
//                 new Vector2(1080f, 1920f);
//         }

//         autoCreatedBallsHud =
//             new GameObject(
//                 "Balls Remaining Text",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         autoCreatedBallsHud.layer =
//             targetCanvas.gameObject.layer;

//         RectTransform textRect =
//             autoCreatedBallsHud.GetComponent<RectTransform>();

//         textRect.SetParent(
//             targetCanvas.transform,
//             false
//         );

//         textRect.anchorMin =
//             new Vector2(0f, 1f);

//         textRect.anchorMax =
//             new Vector2(0f, 1f);

//         textRect.pivot =
//             new Vector2(0f, 1f);

//         textRect.anchoredPosition =
//             new Vector2(32f, -32f);

//         textRect.sizeDelta =
//             new Vector2(420f, 80f);

//         TextMeshProUGUI hudText =
//             autoCreatedBallsHud.GetComponent<TextMeshProUGUI>();

//         hudText.fontSize = 44f;
//         hudText.fontStyle = FontStyles.Bold;
//         hudText.alignment = TextAlignmentOptions.TopLeft;
//         hudText.color = Color.white;
//         hudText.raycastTarget = false;

//         ballsRemainingText = hudText;
//     }


//     private void UpdateBallsHud()
//     {
//         if (ballsRemainingText == null)
//         {
//             return;
//         }

//         ballsRemainingText.text =
//             $"{remainingBalls}";

//         ballsRemainingText.color =
//             remainingBalls > 0
//                 ? Color.white
//                 : new Color(1f, 0.25f, 0.25f, 1f);
//     }


//     private void CacheCannonColliders()
//     {
//         cannonColliders =
//             cannonRoot != null
//                 ? cannonRoot.GetComponentsInChildren<Collider>(true)
//                 : new Collider[0];
//     }


//     private void Update()
//     {
//         Pointer activePointer =
//             Pointer.current;

//         if (activePointer == null)
//         {
//             return;
//         }

//         Vector2 screenPosition =
//             activePointer.position.ReadValue();

//         if (activePointer.press.wasPressedThisFrame)
//         {
//             HandlePointerDown(
//                 screenPosition
//             );
//         }
//         else if (
//             activePointer.press.isPressed &&
//             isPointerHeld)
//         {
//             HandlePointerDrag(
//                 screenPosition
//             );
//         }

//         if (
//             activePointer.press.wasReleasedThisFrame &&
//             isPointerHeld)
//         {
//             HandlePointerUp(
//                 screenPosition
//             );
//         }
//     }


//     private void HandlePointerDown(
//         Vector2 screenPosition)
//     {
//         isPointerHeld =
//             true;

//         pointerStartedOverUI =
//             ignoreInputOverUI &&
//             IsPointerOverUI();

//         pointerStartedInDeadSpace =
//             IsInsideDeadSpace(
//                 screenPosition
//             );

//         if (
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace)
//         {
//             hasAimPoint =
//                 false;

//             return;
//         }

//         UpdateAim(
//             screenPosition
//         );
//     }


//     private void HandlePointerDrag(
//         Vector2 screenPosition)
//     {
//         if (
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace ||
//             IsInsideDeadSpace(screenPosition))
//         {
//             return;
//         }

//         UpdateAim(
//             screenPosition
//         );
//     }


//     private void HandlePointerUp(
//         Vector2 screenPosition)
//     {
//         isPointerHeld =
//             false;

//         bool blocked =
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace ||
//             IsInsideDeadSpace(
//                 screenPosition
//             );

//         pointerStartedOverUI =
//             false;

//         pointerStartedInDeadSpace =
//             false;

//         if (blocked)
//         {
//             hasAimPoint =
//                 false;

//             return;
//         }

//         /*
//          * Release position se exact aim
//          * dobara calculate.
//          */
//         if (UpdateAim(screenPosition))
//         {
//             Shoot();
//         }
//     }


//     private bool IsPointerOverUI()
//     {
//         if (EventSystem.current == null)
//         {
//             return false;
//         }

//         return
//             EventSystem.current
//                 .IsPointerOverGameObject();
//     }


//     private bool IsInsideDeadSpace(
//         Vector2 screenPosition)
//     {
//         return
//             screenPosition.y <=
//             bottomDeadSpaceHeight;
//     }


//     private bool UpdateAim(
//         Vector2 screenPosition)
//     {
//         if (!ValidateReferences())
//         {
//             hasAimPoint =
//                 false;

//             return false;
//         }

//         Ray screenRay =
//             aimCamera.ScreenPointToRay(
//                 screenPosition
//             );

//         RaycastHit chosenHit;

//         /*
//          * First preference:
//          * tower target.
//          */
//         if (!TryGetClosestValidHit(
//                 screenRay,
//                 towerTargetMask,
//                 true,
//                 out chosenHit))
//         {
//             /*
//              * Tower hit na ho to
//              * environment fallback.
//              */
//             if (!TryGetClosestValidHit(
//                     screenRay,
//                     fallbackAimMask,
//                     false,
//                     out chosenHit))
//             {
//                 currentAimPoint =
//                     GetFallbackAimPoint(
//                         screenRay
//                     );
//             }
//             else
//             {
//                 currentAimPoint =
//                     chosenHit.point;
//             }
//         }
//         else
//         {
//             currentAimPoint =
//                 chosenHit.point;
//         }


//         if (
//             preventBackwardAim &&
//             IsPointBehindCannon(
//                 currentAimPoint
//             ))
//         {
//             hasAimPoint =
//                 false;

//             if (showDebugLogs)
//             {
//                 Debug.Log(
//                     "Rear-side click ignored.",
//                     this
//                 );
//             }

//             return false;
//         }


//         RotateCannonTowards(
//             currentAimPoint
//         );

//         Physics.SyncTransforms();


//         Vector3 finalDirection =
//             currentAimPoint -
//             muzzle.position;

//         if (
//             finalDirection.sqrMagnitude <
//             0.0001f)
//         {
//             hasAimPoint =
//                 false;

//             return false;
//         }

//         hasAimPoint =
//             true;


//         if (showDebugLines)
//         {
//             Debug.DrawLine(
//                 muzzle.position,
//                 currentAimPoint,
//                 Color.red,
//                 0.25f
//             );
//         }


//         if (showDebugLogs)
//         {
//             string hitName =
//                 chosenHit.collider != null
//                     ? chosenHit.collider.name
//                     : "Fallback point";

//             Debug.Log(
//                 $"Aim target: {hitName} | " +
//                 $"Point: {currentAimPoint}",
//                 this
//             );
//         }

//         return true;
//     }


//     private bool TryGetClosestValidHit(
//         Ray ray,
//         LayerMask mask,
//         bool requireTowerTarget,
//         out RaycastHit closestHit)
//     {
//         closestHit =
//             default;

//         if (mask.value == 0)
//         {
//             return false;
//         }


//         RaycastHit[] hits =
//             Physics.RaycastAll(
//                 ray,
//                 maxAimDistance,
//                 mask,
//                 QueryTriggerInteraction.Ignore
//             );


//         bool found =
//             false;

//         float closestDistance =
//             float.MaxValue;


//         foreach (RaycastHit hit in hits)
//         {
//             if (
//                 hit.collider == null ||
//                 IsCannonCollider(
//                     hit.collider
//                 ))
//             {
//                 continue;
//             }


//             if (
//                 requireTowerTarget &&
//                 !IsTowerTarget(
//                     hit.collider
//                 ))
//             {
//                 continue;
//             }


//             if (
//                 hit.distance >=
//                 closestDistance)
//             {
//                 continue;
//             }


//             closestDistance =
//                 hit.distance;

//             closestHit =
//                 hit;

//             found =
//                 true;
//         }

//         return found;
//     }


//     private bool IsTowerTarget(
//         Collider hitCollider)
//     {
//         if (hitCollider == null)
//         {
//             return false;
//         }


//         /*
//          * New generic tower system mein
//          * PhysicsTowerObject check.
//          */
//         if (hitCollider.GetComponentInParent<
//                 PhysicsTowerObject>() != null)
//         {
//             return true;
//         }


//         int layerBit =
//             1 <<
//             hitCollider.gameObject.layer;


//         return
//             (towerTargetMask.value &
//              layerBit) != 0;
//     }


//     private bool IsCannonCollider(
//         Collider candidate)
//     {
//         if (
//             candidate == null ||
//             cannonRoot == null)
//         {
//             return false;
//         }


//         Transform candidateTransform =
//             candidate.transform;


//         return
//             candidateTransform ==
//                 cannonRoot ||
//             candidateTransform.IsChildOf(
//                 cannonRoot
//             );
//     }


//     private Vector3 GetFallbackAimPoint(
//         Ray screenRay)
//     {
//         if (
//             fallbackAimPlaneReference !=
//             null)
//         {
//             Plane aimPlane =
//                 new Plane(
//                     -aimCamera.transform.forward,
//                     fallbackAimPlaneReference.position
//                 );


//             if (
//                 aimPlane.Raycast(
//                     screenRay,
//                     out float enterDistance) &&
//                 enterDistance > 0f)
//             {
//                 return
//                     screenRay.GetPoint(
//                         enterDistance
//                     );
//             }
//         }


//         return
//             screenRay.GetPoint(
//                 fallbackAimDistance
//             );
//     }


//     private bool IsPointBehindCannon(
//         Vector3 targetPoint)
//     {
//         Transform forwardSpace =
//             yawPivot.parent != null
//                 ? yawPivot.parent
//                 : transform;


//         Vector3 flatDirection =
//             targetPoint -
//             yawPivot.position;

//         flatDirection.y =
//             0f;


//         Vector3 flatForward =
//             forwardSpace.forward;

//         flatForward.y =
//             0f;


//         if (
//             flatDirection.sqrMagnitude <
//                 0.0001f ||
//             flatForward.sqrMagnitude <
//                 0.0001f)
//         {
//             return false;
//         }


//         return
//             Vector3.Dot(
//                 flatForward.normalized,
//                 flatDirection.normalized
//             ) <= 0f;
//     }


//     private void RotateCannonTowards(
//         Vector3 targetPoint)
//     {
//         if (
//             yawPivot == null ||
//             pitchPivot == null)
//         {
//             return;
//         }

//         RotateYawTowards(
//             targetPoint
//         );

//         RotatePitchTowards(
//             targetPoint
//         );
//     }


//     private void RotateYawTowards(
//         Vector3 targetPoint)
//     {
//         Vector3 worldDirection =
//             targetPoint -
//             yawPivot.position;

//         worldDirection.y =
//             0f;


//         if (
//             worldDirection.sqrMagnitude <
//             0.001f)
//         {
//             return;
//         }


//         Vector3 localDirection;


//         if (yawPivot.parent != null)
//         {
//             localDirection =
//                 yawPivot.parent
//                     .InverseTransformDirection(
//                         worldDirection
//                     );
//         }
//         else
//         {
//             localDirection =
//                 worldDirection;
//         }


//         float yawAngle =
//             Mathf.Atan2(
//                 localDirection.x,
//                 localDirection.z
//             ) *
//             Mathf.Rad2Deg;


//         yawPivot.localRotation =
//             Quaternion.Euler(
//                 0f,
//                 yawAngle,
//                 0f
//             );
//     }


//     private void RotatePitchTowards(
//         Vector3 targetPoint)
//     {
//         Vector3 worldDirection =
//             targetPoint -
//             pitchPivot.position;


//         Vector3 localDirection =
//             yawPivot
//                 .InverseTransformDirection(
//                     worldDirection
//                 );


//         float horizontalDistance =
//             new Vector2(
//                 localDirection.x,
//                 localDirection.z
//             ).magnitude;


//         float pitchAngle =
//             Mathf.Atan2(
//                 localDirection.y,
//                 horizontalDistance
//             ) *
//             Mathf.Rad2Deg;


//         pitchAngle =
//             Mathf.Clamp(
//                 pitchAngle,
//                 minimumPitch,
//                 maximumPitch
//             );


//         pitchPivot.localRotation =
//             Quaternion.Euler(
//                 -pitchAngle,
//                 0f,
//                 0f
//             );
//     }


//     public void Shoot()
//     {
//         EnsureBallLimitInitialized();

//         if (
//             remainingBalls <= 0 ||
//             (levelRuntimeController != null &&
//              !levelRuntimeController.IsLevelGenerated) ||
//             !hasAimPoint ||
//             Time.time <
//             nextAllowedShotTime)
//         {
//             return;
//         }


//         if (
//             cannonBallPrefab == null ||
//             muzzle == null)
//         {
//             return;
//         }


//         Physics.SyncTransforms();


//         Vector3 spawnOrigin =
//             muzzle.position;


//         bool useGravityForShot =
//             !exactStraightShot;


//         Vector3 shotVelocity;


//         if (useGravityForShot)
//         {
//             bool solved =
//                 TryCalculateBallisticVelocity(
//                     spawnOrigin,
//                     currentAimPoint,
//                     launchSpeed,
//                     out shotVelocity
//                 );


//             if (!solved)
//             {
//                 if (
//                     !fallbackToStraightIfUnreachable)
//                 {
//                     if (showDebugLogs)
//                     {
//                         Debug.LogWarning(
//                             "Target current Launch Speed " +
//                             "se reach nahi ho sakta.",
//                             this
//                         );
//                     }

//                     return;
//                 }


//                 useGravityForShot =
//                     false;


//                 Vector3 straightDirection =
//                     currentAimPoint -
//                     spawnOrigin;


//                 if (
//                     straightDirection.sqrMagnitude <
//                     0.0001f)
//                 {
//                     return;
//                 }


//                 shotVelocity =
//                     straightDirection.normalized *
//                     launchSpeed;


//                 if (showDebugLogs)
//                 {
//                     Debug.LogWarning(
//                         "Target unreachable. " +
//                         "Straight fallback shot.",
//                         this
//                     );
//                 }
//             }
//         }
//         else
//         {
//             Vector3 straightDirection =
//                 currentAimPoint -
//                 spawnOrigin;


//             if (
//                 straightDirection.sqrMagnitude <
//                 0.0001f)
//             {
//                 return;
//             }


//             shotVelocity =
//                 straightDirection.normalized *
//                 launchSpeed;
//         }


//         Vector3 shotDirection =
//             shotVelocity.sqrMagnitude >
//             0.0001f
//                 ? shotVelocity.normalized
//                 : (
//                     currentAimPoint -
//                     spawnOrigin
//                 ).normalized;


//         if (
//             preventBackwardAim &&
//             IsDirectionBehindCannon(
//                 shotDirection
//             ))
//         {
//             return;
//         }


//         nextAllowedShotTime =
//             Time.time +
//             minimumShotInterval;


//         Vector3 spawnPosition =
//             spawnOrigin +
//             shotDirection *
//             spawnForwardOffset;


//         Quaternion spawnRotation =
//             Quaternion.LookRotation(
//                 shotDirection,
//                 GetSafeUpDirection(
//                     shotDirection
//                 )
//             );


//         GameObject cannonBall =
//             Instantiate(
//                 cannonBallPrefab,
//                 spawnPosition,
//                 spawnRotation
//             );


//         Rigidbody ballRigidbody =
//             cannonBall
//                 .GetComponent<Rigidbody>();


//         if (ballRigidbody == null)
//         {
//             Debug.LogError(
//                 "Cannonball prefab par Rigidbody missing hai.",
//                 cannonBall
//             );

//             Destroy(
//                 cannonBall
//             );

//             return;
//         }

//         LowerGroundDisappearEffect groundDisappearEffect =
//             cannonBall.GetComponent<LowerGroundDisappearEffect>();

//         if (groundDisappearEffect == null)
//         {
//             groundDisappearEffect =
//                 cannonBall.AddComponent<LowerGroundDisappearEffect>();
//         }

//         groundDisappearEffect.SetDestroyOnComplete(true);
//         groundDisappearEffect.SetAcceptUnmarkedStaticGround(true);
//         groundDisappearEffect.ResetEffect();


//         ballRigidbody.isKinematic =
//             false;

//         ballRigidbody.useGravity =
//             useGravityForShot;

//         ballRigidbody.angularVelocity =
//             Vector3.zero;

//         ballRigidbody.collisionDetectionMode =
//             CollisionDetectionMode
//                 .ContinuousDynamic;


//         SetBodyVelocity(
//             ballRigidbody,
//             Vector3.zero
//         );


//         IgnoreBallCollisionWithCannon(
//             cannonBall
//         );


//         SetBodyVelocity(
//             ballRigidbody,
//             shotVelocity
//         );

//         ConsumeBall();


//         if (showDebugLines)
//         {
//             Debug.DrawLine(
//                 spawnPosition,
//                 currentAimPoint,
//                 Color.green,
//                 2f
//             );
//         }


//         groundDisappearEffect.ScheduleSmoothFallback(
//             cannonBallLifetime
//         );
//     }


//     private bool TryCalculateBallisticVelocity(
//         Vector3 startPosition,
//         Vector3 targetPosition,
//         float speed,
//         out Vector3 launchVelocity)
//     {
//         launchVelocity =
//             Vector3.zero;


//         if (speed <= 0.01f)
//         {
//             return false;
//         }


//         float gravity =
//             Mathf.Abs(
//                 Physics.gravity.y
//             );


//         if (gravity <= 0.001f)
//         {
//             return false;
//         }


//         Vector3 toTarget =
//             targetPosition -
//             startPosition;


//         Vector3 horizontalToTarget =
//             new Vector3(
//                 toTarget.x,
//                 0f,
//                 toTarget.z
//             );


//         float horizontalDistance =
//             horizontalToTarget.magnitude;


//         float verticalDistance =
//             toTarget.y;


//         if (
//             horizontalDistance <
//             0.0001f)
//         {
//             launchVelocity =
//                 new Vector3(
//                     0f,
//                     Mathf.Sign(
//                         verticalDistance == 0f
//                             ? 1f
//                             : verticalDistance
//                     ) * speed,
//                     0f
//                 );

//             return true;
//         }


//         float speedSquared =
//             speed *
//             speed;


//         float discriminant =
//             (
//                 speedSquared *
//                 speedSquared
//             )
//             -
//             gravity *
//             (
//                 gravity *
//                 horizontalDistance *
//                 horizontalDistance
//                 +
//                 2f *
//                 verticalDistance *
//                 speedSquared
//             );


//         if (discriminant < 0f)
//         {
//             return false;
//         }


//         float sqrtDiscriminant =
//             Mathf.Sqrt(
//                 discriminant
//             );


//         float lowArcAngle =
//             Mathf.Atan2(
//                 speedSquared -
//                 sqrtDiscriminant,

//                 gravity *
//                 horizontalDistance
//             );


//         float highArcAngle =
//             Mathf.Atan2(
//                 speedSquared +
//                 sqrtDiscriminant,

//                 gravity *
//                 horizontalDistance
//             );


//         float chosenAngle =
//             preferLowArcTrajectory
//                 ? lowArcAngle
//                 : highArcAngle;


//         Vector3 horizontalDirection =
//             horizontalToTarget.normalized;


//         float horizontalSpeed =
//             speed *
//             Mathf.Cos(
//                 chosenAngle
//             );


//         float verticalSpeed =
//             speed *
//             Mathf.Sin(
//                 chosenAngle
//             );


//         launchVelocity =
//             horizontalDirection *
//             horizontalSpeed
//             +
//             Vector3.up *
//             verticalSpeed;


//         return true;
//     }


//     private bool IsDirectionBehindCannon(
//         Vector3 direction)
//     {
//         Transform forwardSpace =
//             yawPivot.parent != null
//                 ? yawPivot.parent
//                 : transform;


//         Vector3 flatDirection =
//             direction;

//         flatDirection.y =
//             0f;


//         Vector3 flatForward =
//             forwardSpace.forward;

//         flatForward.y =
//             0f;


//         if (
//             flatDirection.sqrMagnitude <
//                 0.0001f ||
//             flatForward.sqrMagnitude <
//                 0.0001f)
//         {
//             return false;
//         }


//         return
//             Vector3.Dot(
//                 flatForward.normalized,
//                 flatDirection.normalized
//             ) <= 0f;
//     }


//     private Vector3 GetSafeUpDirection(
//         Vector3 shotDirection)
//     {
//         Vector3 upDirection =
//             muzzle != null
//                 ? muzzle.up
//                 : Vector3.up;


//         if (
//             Mathf.Abs(
//                 Vector3.Dot(
//                     shotDirection,
//                     upDirection.normalized
//                 )
//             ) > 0.999f)
//         {
//             upDirection =
//                 Vector3.up;
//         }


//         return upDirection;
//     }


//     private void IgnoreBallCollisionWithCannon(
//         GameObject cannonBall)
//     {
//         if (cannonBall == null)
//         {
//             return;
//         }


//         if (
//             cannonColliders == null ||
//             cannonColliders.Length == 0)
//         {
//             CacheCannonColliders();
//         }


//         Collider[] ballColliders =
//             cannonBall
//                 .GetComponentsInChildren<
//                     Collider>(true);


//         foreach (
//             Collider ballCollider
//             in ballColliders)
//         {
//             if (ballCollider == null)
//             {
//                 continue;
//             }


//             foreach (
//                 Collider cannonCollider
//                 in cannonColliders)
//             {
//                 if (
//                     cannonCollider == null)
//                 {
//                     continue;
//                 }


//                 Physics.IgnoreCollision(
//                     ballCollider,
//                     cannonCollider,
//                     true
//                 );
//             }
//         }
//     }


//     private bool ValidateReferences()
//     {
//         if (aimCamera == null)
//         {
//             Debug.LogError(
//                 "Aim Camera missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (
//             yawPivot == null ||
//             pitchPivot == null ||
//             muzzle == null)
//         {
//             Debug.LogError(
//                 "Yaw Pivot, Pitch Pivot ya Muzzle missing hai.",
//                 this
//             );

//             return false;
//         }


//         return true;
//     }


//     private static void SetBodyVelocity(
//         Rigidbody body,
//         Vector3 velocity)
//     {
//         if (body == null)
//         {
//             return;
//         }

// #if UNITY_6000_0_OR_NEWER
//         body.linearVelocity =
//             velocity;
// #else
//         body.velocity =
//             velocity;
// #endif
//     }


// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         if (
//             !showDebugLines ||
//             !hasAimPoint ||
//             muzzle == null)
//         {
//             return;
//         }


//         Gizmos.DrawLine(
//             muzzle.position,
//             currentAimPoint
//         );


//         Gizmos.DrawSphere(
//             currentAimPoint,
//             0.08f
//         );
//     }
// #endif
// }










// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using TMPro;

// public sealed class CannonController : MonoBehaviour
// {
//     [Header("Cannon References")]
//     [SerializeField] private Transform yawPivot;
//     [SerializeField] private Transform pitchPivot;
//     [SerializeField] private Transform muzzle;

//     [Tooltip(
//         "Complete cannon root. Ball cannon colliders ko ignore karegi."
//     )]
//     [SerializeField] private Transform cannonRoot;


//     [Header("Camera And Exact Targeting")]
//     [SerializeField] private Camera aimCamera;

//     [Tooltip(
//         "TowerTarget layer select karein. " +
//         "Block par click hone par isi mask ko sab se pehle check kiya jayega."
//     )]
//     [SerializeField] private LayerMask towerTargetMask;

//     [Tooltip(
//         "Table, Ground aur Environment layers. " +
//         "UI layer yahan include na karein."
//     )]
//     [SerializeField] private LayerMask fallbackAimMask = ~0;

//     [SerializeField, Min(1f)]
//     private float maxAimDistance = 500f;

//     [Tooltip(
//         "Ray kisi collider ko hit na kare to tower ki depth par " +
//         "point calculate hoga."
//     )]
//     [SerializeField]
//     private Transform fallbackAimPlaneReference;

//     [SerializeField, Min(1f)]
//     private float fallbackAimDistance = 100f;


//     [Header("UI Interaction Guard")]

//     [Tooltip(
//         "Pointer UI ke upar ho to cannon aim/shoot trigger nahi hoga."
//     )]
//     [SerializeField]
//     private bool ignoreInputOverUI = true;


//     [Header("Forward Only")]
//     [SerializeField]
//     private bool preventBackwardAim = true;


//     [Header("Aim Limits")]
//     [SerializeField] private float minimumPitch = -10f;
//     [SerializeField] private float maximumPitch = 70f;


//     [Header("Shooting")]
//     [SerializeField] private GameObject cannonBallPrefab;

//     [SerializeField, Min(0f)]
//     private float launchSpeed = 30f;

//     [Tooltip("Ball muzzle se itna aage spawn hogi.")]
//     [SerializeField, Min(0f)]
//     private float spawnForwardOffset = 0.20f;

//     [SerializeField, Min(0f)]
//     private float cannonBallLifetime = 5f;

//     [SerializeField, Min(0f)]
//     private float minimumShotInterval = 0f;


//     [Header("Ball Limit And HUD")]

//     [SerializeField]
//     private LevelRuntimeController levelRuntimeController;

//     [Tooltip(
//         "Last ball fire hone ke baad itna short realtime wait hoga. " +
//         "Is delay mein last shot ko Level Complete trigger karne ka chance milta hai."
//     )]
//     [SerializeField, Min(0f)]
//     private float outOfMovesResultDelay = 0.75f;

//     [Tooltip(
//         "Optional TMP text. Empty ho to top-left HUD automatically create hoga."
//     )]
//     [SerializeField]
//     private TMP_Text ballsRemainingText;

//     [SerializeField]
//     private bool autoCreateBallsHud = true;

//     [SerializeField, Min(1)]
//     private int fallbackBallLimit = 30;


//     [Header("Trajectory")]

//     [Tooltip(
//         "ON: gravity off aur straight exact shot. " +
//         "OFF: gravity ke sath ballistic trajectory."
//     )]
//     [SerializeField]
//     private bool exactStraightShot = true;

//     [Tooltip(
//         "True = low arc. False = high arc."
//     )]
//     [SerializeField]
//     private bool preferLowArcTrajectory = true;

//     [Tooltip(
//         "Arc se target unreachable ho to straight shot fallback."
//     )]
//     [SerializeField]
//     private bool fallbackToStraightIfUnreachable = true;


//     [Header("Bottom Dead Space")]

//     [Tooltip(
//         "Screen ke bottom se pixels. " +
//         "Is area mein aim/shoot nahi chalega. " +
//         "DeadZoneVisual automatically isi value ke sath sync hogi."
//     )]
//     [SerializeField, Min(0f)]
//     private float bottomDeadSpaceHeight = 250f;


//     [Header("Debug")]
//     [SerializeField] private bool showDebugLogs = true;
//     [SerializeField] private bool showDebugLines = true;


//     /*
//      * DeadZoneVisual isi property ko read karegi.
//      *
//      * Isliye dead-zone height sirf CannonController
//      * mein maintain karni hai.
//      */
//     public float BottomDeadSpaceHeight =>
//         bottomDeadSpaceHeight;


//     private Vector3 currentAimPoint;

//     private bool hasAimPoint;
//     private bool pointerStartedInDeadSpace;
//     private bool pointerStartedOverUI;
//     private bool isPointerHeld;

//     private float nextAllowedShotTime;

//     private Collider[] cannonColliders;

//     private LevelRuntimeController subscribedLevelController;
//     private GridLevelData activeBallLevel;
//     private GameObject autoCreatedBallsHud;
//     private int totalBalls;
//     private int remainingBalls;
//     private bool ballLimitInitialized;

//     private readonly List<GameObject> activeCannonBalls =
//         new List<GameObject>();

//     private Coroutine outOfMovesRoutine;

//     public int RemainingBalls => remainingBalls;
//     public int TotalBalls => totalBalls;

//     public event Action OutOfMoves;


//     public void SetGameplayActive(bool active)
//     {
//         if (!active)
//         {
//             isPointerHeld = false;
//             hasAimPoint = false;
//         }

//         if (cannonRoot != null)
//         {
//             cannonRoot.gameObject.SetActive(active);
//         }

//         if (ballsRemainingText != null)
//         {
//             ballsRemainingText.gameObject.SetActive(active);
//         }

//         enabled = active;
//     }


//     private void Awake()
//     {
//         if (aimCamera == null)
//         {
//             aimCamera = Camera.main;
//         }

//         if (cannonRoot == null)
//         {
//             cannonRoot = transform;
//         }

//         CacheCannonColliders();
//     }


//     private void OnEnable()
//     {
//         ConnectLevelController();
//     }


//     private void Start()
//     {
//         ConnectLevelController();
//         EnsureBallsHud();

//         ResetBallLimit(
//             levelRuntimeController != null
//                 ? levelRuntimeController.CurrentLevelData
//                 : null
//         );
//     }


//     private void OnDisable()
//     {
//         DisconnectLevelController();

//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//             outOfMovesRoutine = null;
//         }
//     }


//     private void ConnectLevelController()
//     {
//         if (levelRuntimeController == null)
//         {
//             levelRuntimeController =
//                 FindFirstObjectByType<LevelRuntimeController>();
//         }

//         if (subscribedLevelController ==
//             levelRuntimeController)
//         {
//             return;
//         }

//         DisconnectLevelController();

//         subscribedLevelController =
//             levelRuntimeController;

//         if (subscribedLevelController != null)
//         {
//             subscribedLevelController.LevelGenerated +=
//                 HandleLevelGenerated;
//         }
//     }


//     private void DisconnectLevelController()
//     {
//         if (subscribedLevelController != null)
//         {
//             subscribedLevelController.LevelGenerated -=
//                 HandleLevelGenerated;
//         }

//         subscribedLevelController = null;
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         ResetBallLimit(generatedLevel);
//     }


//     private void EnsureBallLimitInitialized()
//     {
//         GridLevelData currentLevel =
//             levelRuntimeController != null
//                 ? levelRuntimeController.CurrentLevelData
//                 : null;

//         if (!ballLimitInitialized ||
//             currentLevel != activeBallLevel)
//         {
//             ResetBallLimit(currentLevel);
//         }
//     }


//     private void ResetBallLimit(
//         GridLevelData ballLevel)
//     {
//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//             outOfMovesRoutine = null;
//         }

//         activeCannonBalls.Clear();

//         activeBallLevel = ballLevel;

//         totalBalls = Mathf.Max(
//             1,
//             ballLevel != null
//                 ? ballLevel.AvailableBalls
//                 : fallbackBallLimit
//         );

//         remainingBalls = totalBalls;
//         ballLimitInitialized = true;

//         EnsureBallsHud();
//         UpdateBallsHud();
//     }


//     private void ConsumeBall()
//     {
//         remainingBalls = Mathf.Max(
//             0,
//             remainingBalls - 1
//         );

//         UpdateBallsHud();
//     }


//     private void EnsureBallsHud()
//     {
//         if (ballsRemainingText != null ||
//             !autoCreateBallsHud)
//         {
//             return;
//         }

//         Canvas targetCanvas = null;

//         Canvas[] canvases =
//             FindObjectsByType<Canvas>(
//                 FindObjectsInactive.Exclude,
//                 FindObjectsSortMode.None
//             );

//         foreach (Canvas candidate in canvases)
//         {
//             if (candidate != null &&
//                 candidate.renderMode !=
//                 RenderMode.WorldSpace)
//             {
//                 targetCanvas = candidate;
//                 break;
//             }
//         }

//         if (targetCanvas == null)
//         {
//             GameObject canvasObject =
//                 new GameObject(
//                     "Cannon HUD Canvas",
//                     typeof(RectTransform),
//                     typeof(Canvas),
//                     typeof(CanvasScaler),
//                     typeof(GraphicRaycaster)
//                 );

//             int uiLayer =
//                 LayerMask.NameToLayer("UI");

//             if (uiLayer >= 0)
//             {
//                 canvasObject.layer = uiLayer;
//             }

//             targetCanvas =
//                 canvasObject.GetComponent<Canvas>();

//             targetCanvas.renderMode =
//                 RenderMode.ScreenSpaceOverlay;

//             targetCanvas.sortingOrder = 100;

//             CanvasScaler scaler =
//                 canvasObject.GetComponent<CanvasScaler>();

//             scaler.uiScaleMode =
//                 CanvasScaler.ScaleMode.ScaleWithScreenSize;

//             scaler.referenceResolution =
//                 new Vector2(1080f, 1920f);
//         }

//         autoCreatedBallsHud =
//             new GameObject(
//                 "Balls Remaining Text",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         autoCreatedBallsHud.layer =
//             targetCanvas.gameObject.layer;

//         RectTransform textRect =
//             autoCreatedBallsHud.GetComponent<RectTransform>();

//         textRect.SetParent(
//             targetCanvas.transform,
//             false
//         );

//         textRect.anchorMin =
//             new Vector2(0f, 1f);

//         textRect.anchorMax =
//             new Vector2(0f, 1f);

//         textRect.pivot =
//             new Vector2(0f, 1f);

//         textRect.anchoredPosition =
//             new Vector2(32f, -32f);

//         textRect.sizeDelta =
//             new Vector2(420f, 80f);

//         TextMeshProUGUI hudText =
//             autoCreatedBallsHud.GetComponent<TextMeshProUGUI>();

//         hudText.fontSize = 44f;
//         hudText.fontStyle = FontStyles.Bold;
//         hudText.alignment = TextAlignmentOptions.TopLeft;
//         hudText.color = Color.white;
//         hudText.raycastTarget = false;

//         ballsRemainingText = hudText;
//     }


//     private void UpdateBallsHud()
//     {
//         if (ballsRemainingText == null)
//         {
//             return;
//         }

//         ballsRemainingText.text =
//             $"{remainingBalls}";

//         ballsRemainingText.color =
//             remainingBalls > 0
//                 ? Color.white
//                 : new Color(1f, 0.25f, 0.25f, 1f);
//     }


//     private void CacheCannonColliders()
//     {
//         cannonColliders =
//             cannonRoot != null
//                 ? cannonRoot.GetComponentsInChildren<Collider>(true)
//                 : new Collider[0];
//     }


//     private void Update()
//     {
//         Pointer activePointer =
//             Pointer.current;

//         if (activePointer == null)
//         {
//             return;
//         }

//         Vector2 screenPosition =
//             activePointer.position.ReadValue();

//         if (activePointer.press.wasPressedThisFrame)
//         {
//             HandlePointerDown(
//                 screenPosition
//             );
//         }
//         else if (
//             activePointer.press.isPressed &&
//             isPointerHeld)
//         {
//             HandlePointerDrag(
//                 screenPosition
//             );
//         }

//         if (
//             activePointer.press.wasReleasedThisFrame &&
//             isPointerHeld)
//         {
//             HandlePointerUp(
//                 screenPosition
//             );
//         }
//     }


//     private void HandlePointerDown(
//         Vector2 screenPosition)
//     {
//         isPointerHeld =
//             true;

//         pointerStartedOverUI =
//             ignoreInputOverUI &&
//             IsPointerOverUI();

//         pointerStartedInDeadSpace =
//             IsInsideDeadSpace(
//                 screenPosition
//             );

//         if (
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace)
//         {
//             hasAimPoint =
//                 false;

//             return;
//         }

//         UpdateAim(
//             screenPosition
//         );
//     }


//     private void HandlePointerDrag(
//         Vector2 screenPosition)
//     {
//         if (
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace ||
//             IsInsideDeadSpace(screenPosition))
//         {
//             return;
//         }

//         UpdateAim(
//             screenPosition
//         );
//     }


//     private void HandlePointerUp(
//         Vector2 screenPosition)
//     {
//         isPointerHeld =
//             false;

//         bool blocked =
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace ||
//             IsInsideDeadSpace(
//                 screenPosition
//             );

//         pointerStartedOverUI =
//             false;

//         pointerStartedInDeadSpace =
//             false;

//         if (blocked)
//         {
//             hasAimPoint =
//                 false;

//             return;
//         }

//         /*
//          * Release position se exact aim
//          * dobara calculate.
//          */
//         if (UpdateAim(screenPosition))
//         {
//             Shoot();
//         }
//     }


//     private bool IsPointerOverUI()
//     {
//         if (EventSystem.current == null)
//         {
//             return false;
//         }

//         return
//             EventSystem.current
//                 .IsPointerOverGameObject();
//     }


//     private bool IsInsideDeadSpace(
//         Vector2 screenPosition)
//     {
//         return
//             screenPosition.y <=
//             bottomDeadSpaceHeight;
//     }


//     private bool UpdateAim(
//         Vector2 screenPosition)
//     {
//         if (!ValidateReferences())
//         {
//             hasAimPoint =
//                 false;

//             return false;
//         }

//         Ray screenRay =
//             aimCamera.ScreenPointToRay(
//                 screenPosition
//             );

//         RaycastHit chosenHit;

//         /*
//          * First preference:
//          * tower target.
//          */
//         if (!TryGetClosestValidHit(
//                 screenRay,
//                 towerTargetMask,
//                 true,
//                 out chosenHit))
//         {
//             /*
//              * Tower hit na ho to
//              * environment fallback.
//              */
//             if (!TryGetClosestValidHit(
//                     screenRay,
//                     fallbackAimMask,
//                     false,
//                     out chosenHit))
//             {
//                 currentAimPoint =
//                     GetFallbackAimPoint(
//                         screenRay
//                     );
//             }
//             else
//             {
//                 currentAimPoint =
//                     chosenHit.point;
//             }
//         }
//         else
//         {
//             currentAimPoint =
//                 chosenHit.point;
//         }


//         if (
//             preventBackwardAim &&
//             IsPointBehindCannon(
//                 currentAimPoint
//             ))
//         {
//             hasAimPoint =
//                 false;

//             if (showDebugLogs)
//             {
//                 Debug.Log(
//                     "Rear-side click ignored.",
//                     this
//                 );
//             }

//             return false;
//         }


//         RotateCannonTowards(
//             currentAimPoint
//         );

//         Physics.SyncTransforms();


//         Vector3 finalDirection =
//             currentAimPoint -
//             muzzle.position;

//         if (
//             finalDirection.sqrMagnitude <
//             0.0001f)
//         {
//             hasAimPoint =
//                 false;

//             return false;
//         }

//         hasAimPoint =
//             true;


//         if (showDebugLines)
//         {
//             Debug.DrawLine(
//                 muzzle.position,
//                 currentAimPoint,
//                 Color.red,
//                 0.25f
//             );
//         }


//         if (showDebugLogs)
//         {
//             string hitName =
//                 chosenHit.collider != null
//                     ? chosenHit.collider.name
//                     : "Fallback point";

//             Debug.Log(
//                 $"Aim target: {hitName} | " +
//                 $"Point: {currentAimPoint}",
//                 this
//             );
//         }

//         return true;
//     }


//     private bool TryGetClosestValidHit(
//         Ray ray,
//         LayerMask mask,
//         bool requireTowerTarget,
//         out RaycastHit closestHit)
//     {
//         closestHit =
//             default;

//         if (mask.value == 0)
//         {
//             return false;
//         }


//         RaycastHit[] hits =
//             Physics.RaycastAll(
//                 ray,
//                 maxAimDistance,
//                 mask,
//                 QueryTriggerInteraction.Ignore
//             );


//         bool found =
//             false;

//         float closestDistance =
//             float.MaxValue;


//         foreach (RaycastHit hit in hits)
//         {
//             if (
//                 hit.collider == null ||
//                 IsCannonCollider(
//                     hit.collider
//                 ))
//             {
//                 continue;
//             }


//             if (
//                 requireTowerTarget &&
//                 !IsTowerTarget(
//                     hit.collider
//                 ))
//             {
//                 continue;
//             }


//             if (
//                 hit.distance >=
//                 closestDistance)
//             {
//                 continue;
//             }


//             closestDistance =
//                 hit.distance;

//             closestHit =
//                 hit;

//             found =
//                 true;
//         }

//         return found;
//     }


//     private bool IsTowerTarget(
//         Collider hitCollider)
//     {
//         if (hitCollider == null)
//         {
//             return false;
//         }


//         /*
//          * New generic tower system mein
//          * PhysicsTowerObject check.
//          */
//         if (hitCollider.GetComponentInParent<
//                 PhysicsTowerObject>() != null)
//         {
//             return true;
//         }


//         int layerBit =
//             1 <<
//             hitCollider.gameObject.layer;


//         return
//             (towerTargetMask.value &
//              layerBit) != 0;
//     }


//     private bool IsCannonCollider(
//         Collider candidate)
//     {
//         if (
//             candidate == null ||
//             cannonRoot == null)
//         {
//             return false;
//         }


//         Transform candidateTransform =
//             candidate.transform;


//         return
//             candidateTransform ==
//                 cannonRoot ||
//             candidateTransform.IsChildOf(
//                 cannonRoot
//             );
//     }


//     private Vector3 GetFallbackAimPoint(
//         Ray screenRay)
//     {
//         if (
//             fallbackAimPlaneReference !=
//             null)
//         {
//             Plane aimPlane =
//                 new Plane(
//                     -aimCamera.transform.forward,
//                     fallbackAimPlaneReference.position
//                 );


//             if (
//                 aimPlane.Raycast(
//                     screenRay,
//                     out float enterDistance) &&
//                 enterDistance > 0f)
//             {
//                 return
//                     screenRay.GetPoint(
//                         enterDistance
//                     );
//             }
//         }


//         return
//             screenRay.GetPoint(
//                 fallbackAimDistance
//             );
//     }


//     private bool IsPointBehindCannon(
//         Vector3 targetPoint)
//     {
//         Transform forwardSpace =
//             yawPivot.parent != null
//                 ? yawPivot.parent
//                 : transform;


//         Vector3 flatDirection =
//             targetPoint -
//             yawPivot.position;

//         flatDirection.y =
//             0f;


//         Vector3 flatForward =
//             forwardSpace.forward;

//         flatForward.y =
//             0f;


//         if (
//             flatDirection.sqrMagnitude <
//                 0.0001f ||
//             flatForward.sqrMagnitude <
//                 0.0001f)
//         {
//             return false;
//         }


//         return
//             Vector3.Dot(
//                 flatForward.normalized,
//                 flatDirection.normalized
//             ) <= 0f;
//     }


//     private void RotateCannonTowards(
//         Vector3 targetPoint)
//     {
//         if (
//             yawPivot == null ||
//             pitchPivot == null)
//         {
//             return;
//         }

//         RotateYawTowards(
//             targetPoint
//         );

//         RotatePitchTowards(
//             targetPoint
//         );
//     }


//     private void RotateYawTowards(
//         Vector3 targetPoint)
//     {
//         Vector3 worldDirection =
//             targetPoint -
//             yawPivot.position;

//         worldDirection.y =
//             0f;


//         if (
//             worldDirection.sqrMagnitude <
//             0.001f)
//         {
//             return;
//         }


//         Vector3 localDirection;


//         if (yawPivot.parent != null)
//         {
//             localDirection =
//                 yawPivot.parent
//                     .InverseTransformDirection(
//                         worldDirection
//                     );
//         }
//         else
//         {
//             localDirection =
//                 worldDirection;
//         }


//         float yawAngle =
//             Mathf.Atan2(
//                 localDirection.x,
//                 localDirection.z
//             ) *
//             Mathf.Rad2Deg;


//         yawPivot.localRotation =
//             Quaternion.Euler(
//                 0f,
//                 yawAngle,
//                 0f
//             );
//     }


//     private void RotatePitchTowards(
//         Vector3 targetPoint)
//     {
//         Vector3 worldDirection =
//             targetPoint -
//             pitchPivot.position;


//         Vector3 localDirection =
//             yawPivot
//                 .InverseTransformDirection(
//                     worldDirection
//                 );


//         float horizontalDistance =
//             new Vector2(
//                 localDirection.x,
//                 localDirection.z
//             ).magnitude;


//         float pitchAngle =
//             Mathf.Atan2(
//                 localDirection.y,
//                 horizontalDistance
//             ) *
//             Mathf.Rad2Deg;


//         pitchAngle =
//             Mathf.Clamp(
//                 pitchAngle,
//                 minimumPitch,
//                 maximumPitch
//             );


//         pitchPivot.localRotation =
//             Quaternion.Euler(
//                 -pitchAngle,
//                 0f,
//                 0f
//             );
//     }


//     public void Shoot()
//     {
//         EnsureBallLimitInitialized();

//         if (
//             remainingBalls <= 0 ||
//             (levelRuntimeController != null &&
//              !levelRuntimeController.IsLevelGenerated) ||
//             !hasAimPoint ||
//             Time.time <
//             nextAllowedShotTime)
//         {
//             return;
//         }


//         if (
//             cannonBallPrefab == null ||
//             muzzle == null)
//         {
//             return;
//         }


//         Physics.SyncTransforms();


//         Vector3 spawnOrigin =
//             muzzle.position;


//         bool useGravityForShot =
//             !exactStraightShot;


//         Vector3 shotVelocity;


//         if (useGravityForShot)
//         {
//             bool solved =
//                 TryCalculateBallisticVelocity(
//                     spawnOrigin,
//                     currentAimPoint,
//                     launchSpeed,
//                     out shotVelocity
//                 );


//             if (!solved)
//             {
//                 if (
//                     !fallbackToStraightIfUnreachable)
//                 {
//                     if (showDebugLogs)
//                     {
//                         Debug.LogWarning(
//                             "Target current Launch Speed " +
//                             "se reach nahi ho sakta.",
//                             this
//                         );
//                     }

//                     return;
//                 }


//                 useGravityForShot =
//                     false;


//                 Vector3 straightDirection =
//                     currentAimPoint -
//                     spawnOrigin;


//                 if (
//                     straightDirection.sqrMagnitude <
//                     0.0001f)
//                 {
//                     return;
//                 }


//                 shotVelocity =
//                     straightDirection.normalized *
//                     launchSpeed;


//                 if (showDebugLogs)
//                 {
//                     Debug.LogWarning(
//                         "Target unreachable. " +
//                         "Straight fallback shot.",
//                         this
//                     );
//                 }
//             }
//         }
//         else
//         {
//             Vector3 straightDirection =
//                 currentAimPoint -
//                 spawnOrigin;


//             if (
//                 straightDirection.sqrMagnitude <
//                 0.0001f)
//             {
//                 return;
//             }


//             shotVelocity =
//                 straightDirection.normalized *
//                 launchSpeed;
//         }


//         Vector3 shotDirection =
//             shotVelocity.sqrMagnitude >
//             0.0001f
//                 ? shotVelocity.normalized
//                 : (
//                     currentAimPoint -
//                     spawnOrigin
//                 ).normalized;


//         if (
//             preventBackwardAim &&
//             IsDirectionBehindCannon(
//                 shotDirection
//             ))
//         {
//             return;
//         }


//         nextAllowedShotTime =
//             Time.time +
//             minimumShotInterval;


//         Vector3 spawnPosition =
//             spawnOrigin +
//             shotDirection *
//             spawnForwardOffset;


//         Quaternion spawnRotation =
//             Quaternion.LookRotation(
//                 shotDirection,
//                 GetSafeUpDirection(
//                     shotDirection
//                 )
//             );


//         GameObject cannonBall =
//             Instantiate(
//                 cannonBallPrefab,
//                 spawnPosition,
//                 spawnRotation
//             );


//         Rigidbody ballRigidbody =
//             cannonBall
//                 .GetComponent<Rigidbody>();


//         if (ballRigidbody == null)
//         {
//             Debug.LogError(
//                 "Cannonball prefab par Rigidbody missing hai.",
//                 cannonBall
//             );

//             Destroy(
//                 cannonBall
//             );

//             return;
//         }

//         activeCannonBalls.Add(cannonBall);

//         LowerGroundDisappearEffect groundDisappearEffect =
//             cannonBall.GetComponent<LowerGroundDisappearEffect>();

//         if (groundDisappearEffect == null)
//         {
//             groundDisappearEffect =
//                 cannonBall.AddComponent<LowerGroundDisappearEffect>();
//         }

//         groundDisappearEffect.SetDestroyOnComplete(true);
//         groundDisappearEffect.SetAcceptUnmarkedStaticGround(true);
//         groundDisappearEffect.ResetEffect();


//         ballRigidbody.isKinematic =
//             false;

//         ballRigidbody.useGravity =
//             useGravityForShot;

//         ballRigidbody.angularVelocity =
//             Vector3.zero;

//         ballRigidbody.collisionDetectionMode =
//             CollisionDetectionMode
//                 .ContinuousDynamic;


//         SetBodyVelocity(
//             ballRigidbody,
//             Vector3.zero
//         );


//         IgnoreBallCollisionWithCannon(
//             cannonBall
//         );


//         SetBodyVelocity(
//             ballRigidbody,
//             shotVelocity
//         );

//         ConsumeBall();


//         if (showDebugLines)
//         {
//             Debug.DrawLine(
//                 spawnPosition,
//                 currentAimPoint,
//                 Color.green,
//                 2f
//             );
//         }


//         groundDisappearEffect.ScheduleSmoothFallback(
//             cannonBallLifetime
//         );

//         if (remainingBalls <= 0)
//         {
//             StartOutOfMovesCheck();
//         }
//     }


//     private void StartOutOfMovesCheck()
//     {
//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//         }

//         outOfMovesRoutine =
//             StartCoroutine(
//                 WaitForBallsToResolveThenShowOutOfMoves()
//             );
//     }


//     private IEnumerator WaitForBallsToResolveThenShowOutOfMoves()
//     {
//         /*
//          * Purana version cannonball destroy hone ka wait karta tha.
//          * CannonBallLifetime 5 seconds ho sakti hai, isliye popup late aata tha.
//          *
//          * Ab sirf short configurable delay wait hota hai.
//          * Is delay mein last shot agar level complete kar de to
//          * Level Complete popup priority lega.
//          */
//         if (outOfMovesResultDelay > 0f)
//         {
//             yield return new WaitForSecondsRealtime(
//                 outOfMovesResultDelay
//             );
//         }

//         outOfMovesRoutine = null;

//         if (remainingBalls > 0)
//         {
//             yield break;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             // Level complete ho chuka hai.
//             yield break;
//         }

//         if (showDebugLogs)
//         {
//             Debug.Log(
//                 "Balls finished. OutOfMoves event invoke.",
//                 this
//             );
//         }

//         OutOfMoves?.Invoke();
//     }


//     private bool TryCalculateBallisticVelocity(
//         Vector3 startPosition,
//         Vector3 targetPosition,
//         float speed,
//         out Vector3 launchVelocity)
//     {
//         launchVelocity =
//             Vector3.zero;


//         if (speed <= 0.01f)
//         {
//             return false;
//         }


//         float gravity =
//             Mathf.Abs(
//                 Physics.gravity.y
//             );


//         if (gravity <= 0.001f)
//         {
//             return false;
//         }


//         Vector3 toTarget =
//             targetPosition -
//             startPosition;


//         Vector3 horizontalToTarget =
//             new Vector3(
//                 toTarget.x,
//                 0f,
//                 toTarget.z
//             );


//         float horizontalDistance =
//             horizontalToTarget.magnitude;


//         float verticalDistance =
//             toTarget.y;


//         if (
//             horizontalDistance <
//             0.0001f)
//         {
//             launchVelocity =
//                 new Vector3(
//                     0f,
//                     Mathf.Sign(
//                         verticalDistance == 0f
//                             ? 1f
//                             : verticalDistance
//                     ) * speed,
//                     0f
//                 );

//             return true;
//         }


//         float speedSquared =
//             speed *
//             speed;


//         float discriminant =
//             (
//                 speedSquared *
//                 speedSquared
//             )
//             -
//             gravity *
//             (
//                 gravity *
//                 horizontalDistance *
//                 horizontalDistance
//                 +
//                 2f *
//                 verticalDistance *
//                 speedSquared
//             );


//         if (discriminant < 0f)
//         {
//             return false;
//         }


//         float sqrtDiscriminant =
//             Mathf.Sqrt(
//                 discriminant
//             );


//         float lowArcAngle =
//             Mathf.Atan2(
//                 speedSquared -
//                 sqrtDiscriminant,

//                 gravity *
//                 horizontalDistance
//             );


//         float highArcAngle =
//             Mathf.Atan2(
//                 speedSquared +
//                 sqrtDiscriminant,

//                 gravity *
//                 horizontalDistance
//             );


//         float chosenAngle =
//             preferLowArcTrajectory
//                 ? lowArcAngle
//                 : highArcAngle;


//         Vector3 horizontalDirection =
//             horizontalToTarget.normalized;


//         float horizontalSpeed =
//             speed *
//             Mathf.Cos(
//                 chosenAngle
//             );


//         float verticalSpeed =
//             speed *
//             Mathf.Sin(
//                 chosenAngle
//             );


//         launchVelocity =
//             horizontalDirection *
//             horizontalSpeed
//             +
//             Vector3.up *
//             verticalSpeed;


//         return true;
//     }


//     private bool IsDirectionBehindCannon(
//         Vector3 direction)
//     {
//         Transform forwardSpace =
//             yawPivot.parent != null
//                 ? yawPivot.parent
//                 : transform;


//         Vector3 flatDirection =
//             direction;

//         flatDirection.y =
//             0f;


//         Vector3 flatForward =
//             forwardSpace.forward;

//         flatForward.y =
//             0f;


//         if (
//             flatDirection.sqrMagnitude <
//                 0.0001f ||
//             flatForward.sqrMagnitude <
//                 0.0001f)
//         {
//             return false;
//         }


//         return
//             Vector3.Dot(
//                 flatForward.normalized,
//                 flatDirection.normalized
//             ) <= 0f;
//     }


//     private Vector3 GetSafeUpDirection(
//         Vector3 shotDirection)
//     {
//         Vector3 upDirection =
//             muzzle != null
//                 ? muzzle.up
//                 : Vector3.up;


//         if (
//             Mathf.Abs(
//                 Vector3.Dot(
//                     shotDirection,
//                     upDirection.normalized
//                 )
//             ) > 0.999f)
//         {
//             upDirection =
//                 Vector3.up;
//         }


//         return upDirection;
//     }


//     private void IgnoreBallCollisionWithCannon(
//         GameObject cannonBall)
//     {
//         if (cannonBall == null)
//         {
//             return;
//         }


//         if (
//             cannonColliders == null ||
//             cannonColliders.Length == 0)
//         {
//             CacheCannonColliders();
//         }


//         Collider[] ballColliders =
//             cannonBall
//                 .GetComponentsInChildren<
//                     Collider>(true);


//         foreach (
//             Collider ballCollider
//             in ballColliders)
//         {
//             if (ballCollider == null)
//             {
//                 continue;
//             }


//             foreach (
//                 Collider cannonCollider
//                 in cannonColliders)
//             {
//                 if (
//                     cannonCollider == null)
//                 {
//                     continue;
//                 }


//                 Physics.IgnoreCollision(
//                     ballCollider,
//                     cannonCollider,
//                     true
//                 );
//             }
//         }
//     }


//     private bool ValidateReferences()
//     {
//         if (aimCamera == null)
//         {
//             Debug.LogError(
//                 "Aim Camera missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (
//             yawPivot == null ||
//             pitchPivot == null ||
//             muzzle == null)
//         {
//             Debug.LogError(
//                 "Yaw Pivot, Pitch Pivot ya Muzzle missing hai.",
//                 this
//             );

//             return false;
//         }


//         return true;
//     }


//     private static void SetBodyVelocity(
//         Rigidbody body,
//         Vector3 velocity)
//     {
//         if (body == null)
//         {
//             return;
//         }

// #if UNITY_6000_0_OR_NEWER
//         body.linearVelocity =
//             velocity;
// #else
//         body.velocity =
//             velocity;
// #endif
//     }


// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         if (
//             !showDebugLines ||
//             !hasAimPoint ||
//             muzzle == null)
//         {
//             return;
//         }


//         Gizmos.DrawLine(
//             muzzle.position,
//             currentAimPoint
//         );


//         Gizmos.DrawSphere(
//             currentAimPoint,
//             0.08f
//         );
//     }
// #endif
// }



// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using TMPro;

// public sealed class CannonController : MonoBehaviour
// {
//     [Header("Cannon References")]
//     [SerializeField] private Transform yawPivot;
//     [SerializeField] private Transform pitchPivot;
//     [SerializeField] private Transform muzzle;

//     [Tooltip(
//         "Complete cannon root. Ball cannon colliders ko ignore karegi."
//     )]
//     [SerializeField] private Transform cannonRoot;


//     [Header("Camera And Exact Targeting")]
//     [SerializeField] private Camera aimCamera;

//     [Tooltip(
//         "TowerTarget layer select karein. " +
//         "Block par click hone par isi mask ko sab se pehle check kiya jayega."
//     )]
//     [SerializeField] private LayerMask towerTargetMask;

//     [Tooltip(
//         "Table, Ground aur Environment layers. " +
//         "UI layer yahan include na karein."
//     )]
//     [SerializeField] private LayerMask fallbackAimMask = ~0;

//     [SerializeField, Min(1f)]
//     private float maxAimDistance = 500f;

//     [Tooltip(
//         "Ray kisi collider ko hit na kare to tower ki depth par " +
//         "point calculate hoga."
//     )]
//     [SerializeField]
//     private Transform fallbackAimPlaneReference;

//     [SerializeField, Min(1f)]
//     private float fallbackAimDistance = 100f;


//     [Header("UI Interaction Guard")]

//     [Tooltip(
//         "Pointer UI ke upar ho to cannon aim/shoot trigger nahi hoga."
//     )]
//     [SerializeField]
//     private bool ignoreInputOverUI = true;


//     [Header("Forward Only")]
//     [SerializeField]
//     private bool preventBackwardAim = true;


//     [Header("Aim Limits")]
//     [SerializeField] private float minimumPitch = -10f;
//     [SerializeField] private float maximumPitch = 70f;


//     [Header("Shooting")]
//     [SerializeField] private GameObject cannonBallPrefab;

//     [SerializeField, Min(0f)]
//     private float launchSpeed = 30f;

//     [Tooltip("Ball muzzle se itna aage spawn hogi.")]
//     [SerializeField, Min(0f)]
//     private float spawnForwardOffset = 0.20f;

//     [SerializeField, Min(0f)]
//     private float cannonBallLifetime = 5f;

//     [SerializeField, Min(0f)]
//     private float minimumShotInterval = 0f;


//     [Header("Ball Limit And HUD")]

//     [SerializeField]
//     private LevelRuntimeController levelRuntimeController;

//     [Tooltip(
//         "Last ball fire hone ke baad itna short realtime wait hoga. " +
//         "Is delay mein last shot ko Level Complete trigger karne ka chance milta hai."
//     )]
//     [SerializeField, Min(0f)]
//     private float outOfMovesResultDelay = 0.75f;

//     [Tooltip(
//         "Optional TMP text. Empty ho to top-left HUD automatically create hoga."
//     )]
//     [SerializeField]
//     private TMP_Text ballsRemainingText;

//     [SerializeField]
//     private bool autoCreateBallsHud = true;

//     [SerializeField, Min(1)]
//     private int fallbackBallLimit = 30;


//     [Header("Trajectory")]

//     [Tooltip(
//         "ON: gravity off aur straight exact shot. " +
//         "OFF: gravity ke sath ballistic trajectory."
//     )]
//     [SerializeField]
//     private bool exactStraightShot = true;

//     [Tooltip(
//         "True = low arc. False = high arc."
//     )]
//     [SerializeField]
//     private bool preferLowArcTrajectory = true;

//     [Tooltip(
//         "Arc se target unreachable ho to straight shot fallback."
//     )]
//     [SerializeField]
//     private bool fallbackToStraightIfUnreachable = true;


//     [Header("Bottom Dead Space")]

//     [Tooltip(
//         "Screen ke bottom se pixels. " +
//         "Is area mein aim/shoot nahi chalega. " +
//         "DeadZoneVisual automatically isi value ke sath sync hogi."
//     )]
//     [SerializeField, Min(0f)]
//     private float bottomDeadSpaceHeight = 250f;


//     [Header("Debug")]
//     [SerializeField] private bool showDebugLogs = true;
//     [SerializeField] private bool showDebugLines = true;


//     /*
//      * DeadZoneVisual isi property ko read karegi.
//      *
//      * Isliye dead-zone height sirf CannonController
//      * mein maintain karni hai.
//      */
//     public float BottomDeadSpaceHeight =>
//         bottomDeadSpaceHeight;


//     private Vector3 currentAimPoint;

//     private bool hasAimPoint;
//     private bool pointerStartedInDeadSpace;
//     private bool pointerStartedOverUI;
//     private bool isPointerHeld;

//     private float nextAllowedShotTime;

//     private Collider[] cannonColliders;

//     private LevelRuntimeController subscribedLevelController;
//     private GridLevelData activeBallLevel;
//     private GameObject autoCreatedBallsHud;
//     private int totalBalls;
//     private int remainingBalls;
//     private bool ballLimitInitialized;

//     private readonly List<GameObject> activeCannonBalls =
//         new List<GameObject>();

//     private Coroutine outOfMovesRoutine;

//     public int RemainingBalls => remainingBalls;
//     public int TotalBalls => totalBalls;

//     public event Action OutOfMoves;


//     public void SetGameplayActive(bool active)
//     {
//         if (!active)
//         {
//             isPointerHeld = false;
//             hasAimPoint = false;
//         }

//         if (cannonRoot != null)
//         {
//             cannonRoot.gameObject.SetActive(active);
//         }

//         if (ballsRemainingText != null)
//         {
//             ballsRemainingText.gameObject.SetActive(active);
//         }

//         enabled = active;
//     }


//     private void Awake()
//     {
//         if (aimCamera == null)
//         {
//             aimCamera = Camera.main;
//         }

//         if (cannonRoot == null)
//         {
//             cannonRoot = transform;
//         }

//         CacheCannonColliders();
//     }


//     private void OnEnable()
//     {
//         ConnectLevelController();
//     }


//     private void Start()
//     {
//         ConnectLevelController();
//         EnsureBallsHud();

//         ResetBallLimit(
//             levelRuntimeController != null
//                 ? levelRuntimeController.CurrentLevelData
//                 : null
//         );
//     }


//     private void OnDisable()
//     {
//         DisconnectLevelController();

//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//             outOfMovesRoutine = null;
//         }
//     }


//     private void ConnectLevelController()
//     {
//         if (levelRuntimeController == null)
//         {
//             levelRuntimeController =
//                 FindFirstObjectByType<LevelRuntimeController>();
//         }

//         if (subscribedLevelController ==
//             levelRuntimeController)
//         {
//             return;
//         }

//         DisconnectLevelController();

//         subscribedLevelController =
//             levelRuntimeController;

//         if (subscribedLevelController != null)
//         {
//             subscribedLevelController.LevelGenerated +=
//                 HandleLevelGenerated;
//         }
//     }


//     private void DisconnectLevelController()
//     {
//         if (subscribedLevelController != null)
//         {
//             subscribedLevelController.LevelGenerated -=
//                 HandleLevelGenerated;
//         }

//         subscribedLevelController = null;
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         ResetBallLimit(generatedLevel);
//     }


//     private void EnsureBallLimitInitialized()
//     {
//         GridLevelData currentLevel =
//             levelRuntimeController != null
//                 ? levelRuntimeController.CurrentLevelData
//                 : null;

//         if (!ballLimitInitialized ||
//             currentLevel != activeBallLevel)
//         {
//             ResetBallLimit(currentLevel);
//         }
//     }


//     private void ResetBallLimit(
//         GridLevelData ballLevel)
//     {
//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//             outOfMovesRoutine = null;
//         }

//         activeCannonBalls.Clear();

//         activeBallLevel = ballLevel;

//         totalBalls = Mathf.Max(
//             1,
//             ballLevel != null
//                 ? ballLevel.AvailableBalls
//                 : fallbackBallLimit
//         );

//         remainingBalls = totalBalls;
//         ballLimitInitialized = true;

//         EnsureBallsHud();
//         UpdateBallsHud();
//     }


//     private void ConsumeBall()
//     {
//         remainingBalls = Mathf.Max(
//             0,
//             remainingBalls - 1
//         );

//         UpdateBallsHud();
//     }


//     public bool AddExtraShots(
//         int amount)
//     {
//         if (amount <= 0)
//         {
//             return false;
//         }

//         EnsureBallLimitInitialized();

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return false;
//         }

//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//             outOfMovesRoutine = null;
//         }

//         remainingBalls += amount;
//         totalBalls += amount;

//         UpdateBallsHud();

//         if (showDebugLogs)
//         {
//             Debug.Log(
//                 $"Extra shots granted: +{amount}. Remaining: {remainingBalls}",
//                 this
//             );
//         }

//         return true;
//     }


//     private void EnsureBallsHud()
//     {
//         if (ballsRemainingText != null ||
//             !autoCreateBallsHud)
//         {
//             return;
//         }

//         Canvas targetCanvas = null;

//         Canvas[] canvases =
//             FindObjectsByType<Canvas>(
//                 FindObjectsInactive.Exclude,
//                 FindObjectsSortMode.None
//             );

//         foreach (Canvas candidate in canvases)
//         {
//             if (candidate != null &&
//                 candidate.renderMode !=
//                 RenderMode.WorldSpace)
//             {
//                 targetCanvas = candidate;
//                 break;
//             }
//         }

//         if (targetCanvas == null)
//         {
//             GameObject canvasObject =
//                 new GameObject(
//                     "Cannon HUD Canvas",
//                     typeof(RectTransform),
//                     typeof(Canvas),
//                     typeof(CanvasScaler),
//                     typeof(GraphicRaycaster)
//                 );

//             int uiLayer =
//                 LayerMask.NameToLayer("UI");

//             if (uiLayer >= 0)
//             {
//                 canvasObject.layer = uiLayer;
//             }

//             targetCanvas =
//                 canvasObject.GetComponent<Canvas>();

//             targetCanvas.renderMode =
//                 RenderMode.ScreenSpaceOverlay;

//             targetCanvas.sortingOrder = 100;

//             CanvasScaler scaler =
//                 canvasObject.GetComponent<CanvasScaler>();

//             scaler.uiScaleMode =
//                 CanvasScaler.ScaleMode.ScaleWithScreenSize;

//             scaler.referenceResolution =
//                 new Vector2(1080f, 1920f);
//         }

//         autoCreatedBallsHud =
//             new GameObject(
//                 "Balls Remaining Text",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         autoCreatedBallsHud.layer =
//             targetCanvas.gameObject.layer;

//         RectTransform textRect =
//             autoCreatedBallsHud.GetComponent<RectTransform>();

//         textRect.SetParent(
//             targetCanvas.transform,
//             false
//         );

//         textRect.anchorMin =
//             new Vector2(0f, 1f);

//         textRect.anchorMax =
//             new Vector2(0f, 1f);

//         textRect.pivot =
//             new Vector2(0f, 1f);

//         textRect.anchoredPosition =
//             new Vector2(32f, -32f);

//         textRect.sizeDelta =
//             new Vector2(420f, 80f);

//         TextMeshProUGUI hudText =
//             autoCreatedBallsHud.GetComponent<TextMeshProUGUI>();

//         hudText.fontSize = 44f;
//         hudText.fontStyle = FontStyles.Bold;
//         hudText.alignment = TextAlignmentOptions.TopLeft;
//         hudText.color = Color.white;
//         hudText.raycastTarget = false;

//         ballsRemainingText = hudText;
//     }


//     private void UpdateBallsHud()
//     {
//         if (ballsRemainingText == null)
//         {
//             return;
//         }

//         ballsRemainingText.text =
//             $"{remainingBalls}";

//         ballsRemainingText.color =
//             remainingBalls > 0
//                 ? Color.white
//                 : new Color(1f, 0.25f, 0.25f, 1f);
//     }


//     private void CacheCannonColliders()
//     {
//         cannonColliders =
//             cannonRoot != null
//                 ? cannonRoot.GetComponentsInChildren<Collider>(true)
//                 : new Collider[0];
//     }


//     private void Update()
//     {
//         Pointer activePointer =
//             Pointer.current;

//         if (activePointer == null)
//         {
//             return;
//         }

//         Vector2 screenPosition =
//             activePointer.position.ReadValue();

//         if (activePointer.press.wasPressedThisFrame)
//         {
//             HandlePointerDown(
//                 screenPosition
//             );
//         }
//         else if (
//             activePointer.press.isPressed &&
//             isPointerHeld)
//         {
//             HandlePointerDrag(
//                 screenPosition
//             );
//         }

//         if (
//             activePointer.press.wasReleasedThisFrame &&
//             isPointerHeld)
//         {
//             HandlePointerUp(
//                 screenPosition
//             );
//         }
//     }


//     private void HandlePointerDown(
//         Vector2 screenPosition)
//     {
//         isPointerHeld =
//             true;

//         pointerStartedOverUI =
//             ignoreInputOverUI &&
//             IsPointerOverUI();

//         pointerStartedInDeadSpace =
//             IsInsideDeadSpace(
//                 screenPosition
//             );

//         if (
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace)
//         {
//             hasAimPoint =
//                 false;

//             return;
//         }

//         UpdateAim(
//             screenPosition
//         );
//     }


//     private void HandlePointerDrag(
//         Vector2 screenPosition)
//     {
//         if (
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace ||
//             IsInsideDeadSpace(screenPosition))
//         {
//             return;
//         }

//         UpdateAim(
//             screenPosition
//         );
//     }


//     private void HandlePointerUp(
//         Vector2 screenPosition)
//     {
//         isPointerHeld =
//             false;

//         bool blocked =
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace ||
//             IsInsideDeadSpace(
//                 screenPosition
//             );

//         pointerStartedOverUI =
//             false;

//         pointerStartedInDeadSpace =
//             false;

//         if (blocked)
//         {
//             hasAimPoint =
//                 false;

//             return;
//         }

//         /*
//          * Release position se exact aim
//          * dobara calculate.
//          */
//         if (UpdateAim(screenPosition))
//         {
//             Shoot();
//         }
//     }


//     private bool IsPointerOverUI()
//     {
//         if (EventSystem.current == null)
//         {
//             return false;
//         }

//         return
//             EventSystem.current
//                 .IsPointerOverGameObject();
//     }


//     private bool IsInsideDeadSpace(
//         Vector2 screenPosition)
//     {
//         return
//             screenPosition.y <=
//             bottomDeadSpaceHeight;
//     }


//     private bool UpdateAim(
//         Vector2 screenPosition)
//     {
//         if (!ValidateReferences())
//         {
//             hasAimPoint =
//                 false;

//             return false;
//         }

//         Ray screenRay =
//             aimCamera.ScreenPointToRay(
//                 screenPosition
//             );

//         RaycastHit chosenHit;

//         /*
//          * First preference:
//          * tower target.
//          */
//         if (!TryGetClosestValidHit(
//                 screenRay,
//                 towerTargetMask,
//                 true,
//                 out chosenHit))
//         {
//             /*
//              * Tower hit na ho to
//              * environment fallback.
//              */
//             if (!TryGetClosestValidHit(
//                     screenRay,
//                     fallbackAimMask,
//                     false,
//                     out chosenHit))
//             {
//                 currentAimPoint =
//                     GetFallbackAimPoint(
//                         screenRay
//                     );
//             }
//             else
//             {
//                 currentAimPoint =
//                     chosenHit.point;
//             }
//         }
//         else
//         {
//             currentAimPoint =
//                 chosenHit.point;
//         }


//         if (
//             preventBackwardAim &&
//             IsPointBehindCannon(
//                 currentAimPoint
//             ))
//         {
//             hasAimPoint =
//                 false;

//             if (showDebugLogs)
//             {
//                 Debug.Log(
//                     "Rear-side click ignored.",
//                     this
//                 );
//             }

//             return false;
//         }


//         RotateCannonTowards(
//             currentAimPoint
//         );

//         Physics.SyncTransforms();


//         Vector3 finalDirection =
//             currentAimPoint -
//             muzzle.position;

//         if (
//             finalDirection.sqrMagnitude <
//             0.0001f)
//         {
//             hasAimPoint =
//                 false;

//             return false;
//         }

//         hasAimPoint =
//             true;


//         if (showDebugLines)
//         {
//             Debug.DrawLine(
//                 muzzle.position,
//                 currentAimPoint,
//                 Color.red,
//                 0.25f
//             );
//         }


//         if (showDebugLogs)
//         {
//             string hitName =
//                 chosenHit.collider != null
//                     ? chosenHit.collider.name
//                     : "Fallback point";

//             Debug.Log(
//                 $"Aim target: {hitName} | " +
//                 $"Point: {currentAimPoint}",
//                 this
//             );
//         }

//         return true;
//     }


//     private bool TryGetClosestValidHit(
//         Ray ray,
//         LayerMask mask,
//         bool requireTowerTarget,
//         out RaycastHit closestHit)
//     {
//         closestHit =
//             default;

//         if (mask.value == 0)
//         {
//             return false;
//         }


//         RaycastHit[] hits =
//             Physics.RaycastAll(
//                 ray,
//                 maxAimDistance,
//                 mask,
//                 QueryTriggerInteraction.Ignore
//             );


//         bool found =
//             false;

//         float closestDistance =
//             float.MaxValue;


//         foreach (RaycastHit hit in hits)
//         {
//             if (
//                 hit.collider == null ||
//                 IsCannonCollider(
//                     hit.collider
//                 ))
//             {
//                 continue;
//             }


//             if (
//                 requireTowerTarget &&
//                 !IsTowerTarget(
//                     hit.collider
//                 ))
//             {
//                 continue;
//             }


//             if (
//                 hit.distance >=
//                 closestDistance)
//             {
//                 continue;
//             }


//             closestDistance =
//                 hit.distance;

//             closestHit =
//                 hit;

//             found =
//                 true;
//         }

//         return found;
//     }


//     private bool IsTowerTarget(
//         Collider hitCollider)
//     {
//         if (hitCollider == null)
//         {
//             return false;
//         }


//         /*
//          * New generic tower system mein
//          * PhysicsTowerObject check.
//          */
//         if (hitCollider.GetComponentInParent<
//                 PhysicsTowerObject>() != null)
//         {
//             return true;
//         }


//         int layerBit =
//             1 <<
//             hitCollider.gameObject.layer;


//         return
//             (towerTargetMask.value &
//              layerBit) != 0;
//     }


//     private bool IsCannonCollider(
//         Collider candidate)
//     {
//         if (
//             candidate == null ||
//             cannonRoot == null)
//         {
//             return false;
//         }


//         Transform candidateTransform =
//             candidate.transform;


//         return
//             candidateTransform ==
//                 cannonRoot ||
//             candidateTransform.IsChildOf(
//                 cannonRoot
//             );
//     }


//     private Vector3 GetFallbackAimPoint(
//         Ray screenRay)
//     {
//         if (
//             fallbackAimPlaneReference !=
//             null)
//         {
//             Plane aimPlane =
//                 new Plane(
//                     -aimCamera.transform.forward,
//                     fallbackAimPlaneReference.position
//                 );


//             if (
//                 aimPlane.Raycast(
//                     screenRay,
//                     out float enterDistance) &&
//                 enterDistance > 0f)
//             {
//                 return
//                     screenRay.GetPoint(
//                         enterDistance
//                     );
//             }
//         }


//         return
//             screenRay.GetPoint(
//                 fallbackAimDistance
//             );
//     }


//     private bool IsPointBehindCannon(
//         Vector3 targetPoint)
//     {
//         Transform forwardSpace =
//             yawPivot.parent != null
//                 ? yawPivot.parent
//                 : transform;


//         Vector3 flatDirection =
//             targetPoint -
//             yawPivot.position;

//         flatDirection.y =
//             0f;


//         Vector3 flatForward =
//             forwardSpace.forward;

//         flatForward.y =
//             0f;


//         if (
//             flatDirection.sqrMagnitude <
//                 0.0001f ||
//             flatForward.sqrMagnitude <
//                 0.0001f)
//         {
//             return false;
//         }


//         return
//             Vector3.Dot(
//                 flatForward.normalized,
//                 flatDirection.normalized
//             ) <= 0f;
//     }


//     private void RotateCannonTowards(
//         Vector3 targetPoint)
//     {
//         if (
//             yawPivot == null ||
//             pitchPivot == null)
//         {
//             return;
//         }

//         RotateYawTowards(
//             targetPoint
//         );

//         RotatePitchTowards(
//             targetPoint
//         );
//     }


//     private void RotateYawTowards(
//         Vector3 targetPoint)
//     {
//         Vector3 worldDirection =
//             targetPoint -
//             yawPivot.position;

//         worldDirection.y =
//             0f;


//         if (
//             worldDirection.sqrMagnitude <
//             0.001f)
//         {
//             return;
//         }


//         Vector3 localDirection;


//         if (yawPivot.parent != null)
//         {
//             localDirection =
//                 yawPivot.parent
//                     .InverseTransformDirection(
//                         worldDirection
//                     );
//         }
//         else
//         {
//             localDirection =
//                 worldDirection;
//         }


//         float yawAngle =
//             Mathf.Atan2(
//                 localDirection.x,
//                 localDirection.z
//             ) *
//             Mathf.Rad2Deg;


//         yawPivot.localRotation =
//             Quaternion.Euler(
//                 0f,
//                 yawAngle,
//                 0f
//             );
//     }


//     private void RotatePitchTowards(
//         Vector3 targetPoint)
//     {
//         Vector3 worldDirection =
//             targetPoint -
//             pitchPivot.position;


//         Vector3 localDirection =
//             yawPivot
//                 .InverseTransformDirection(
//                     worldDirection
//                 );


//         float horizontalDistance =
//             new Vector2(
//                 localDirection.x,
//                 localDirection.z
//             ).magnitude;


//         float pitchAngle =
//             Mathf.Atan2(
//                 localDirection.y,
//                 horizontalDistance
//             ) *
//             Mathf.Rad2Deg;


//         pitchAngle =
//             Mathf.Clamp(
//                 pitchAngle,
//                 minimumPitch,
//                 maximumPitch
//             );


//         pitchPivot.localRotation =
//             Quaternion.Euler(
//                 -pitchAngle,
//                 0f,
//                 0f
//             );
//     }


//     public void Shoot()
//     {
//         EnsureBallLimitInitialized();

//         if (
//             remainingBalls <= 0 ||
//             (levelRuntimeController != null &&
//              !levelRuntimeController.IsLevelGenerated) ||
//             !hasAimPoint ||
//             Time.time <
//             nextAllowedShotTime)
//         {
//             return;
//         }


//         if (
//             cannonBallPrefab == null ||
//             muzzle == null)
//         {
//             return;
//         }


//         Physics.SyncTransforms();


//         Vector3 spawnOrigin =
//             muzzle.position;


//         bool useGravityForShot =
//             !exactStraightShot;


//         Vector3 shotVelocity;


//         if (useGravityForShot)
//         {
//             bool solved =
//                 TryCalculateBallisticVelocity(
//                     spawnOrigin,
//                     currentAimPoint,
//                     launchSpeed,
//                     out shotVelocity
//                 );


//             if (!solved)
//             {
//                 if (
//                     !fallbackToStraightIfUnreachable)
//                 {
//                     if (showDebugLogs)
//                     {
//                         Debug.LogWarning(
//                             "Target current Launch Speed " +
//                             "se reach nahi ho sakta.",
//                             this
//                         );
//                     }

//                     return;
//                 }


//                 useGravityForShot =
//                     false;


//                 Vector3 straightDirection =
//                     currentAimPoint -
//                     spawnOrigin;


//                 if (
//                     straightDirection.sqrMagnitude <
//                     0.0001f)
//                 {
//                     return;
//                 }


//                 shotVelocity =
//                     straightDirection.normalized *
//                     launchSpeed;


//                 if (showDebugLogs)
//                 {
//                     Debug.LogWarning(
//                         "Target unreachable. " +
//                         "Straight fallback shot.",
//                         this
//                     );
//                 }
//             }
//         }
//         else
//         {
//             Vector3 straightDirection =
//                 currentAimPoint -
//                 spawnOrigin;


//             if (
//                 straightDirection.sqrMagnitude <
//                 0.0001f)
//             {
//                 return;
//             }


//             shotVelocity =
//                 straightDirection.normalized *
//                 launchSpeed;
//         }


//         Vector3 shotDirection =
//             shotVelocity.sqrMagnitude >
//             0.0001f
//                 ? shotVelocity.normalized
//                 : (
//                     currentAimPoint -
//                     spawnOrigin
//                 ).normalized;


//         if (
//             preventBackwardAim &&
//             IsDirectionBehindCannon(
//                 shotDirection
//             ))
//         {
//             return;
//         }


//         nextAllowedShotTime =
//             Time.time +
//             minimumShotInterval;


//         Vector3 spawnPosition =
//             spawnOrigin +
//             shotDirection *
//             spawnForwardOffset;


//         Quaternion spawnRotation =
//             Quaternion.LookRotation(
//                 shotDirection,
//                 GetSafeUpDirection(
//                     shotDirection
//                 )
//             );


//         GameObject cannonBall =
//             Instantiate(
//                 cannonBallPrefab,
//                 spawnPosition,
//                 spawnRotation
//             );


//         Rigidbody ballRigidbody =
//             cannonBall
//                 .GetComponent<Rigidbody>();


//         if (ballRigidbody == null)
//         {
//             Debug.LogError(
//                 "Cannonball prefab par Rigidbody missing hai.",
//                 cannonBall
//             );

//             Destroy(
//                 cannonBall
//             );

//             return;
//         }

//         activeCannonBalls.Add(cannonBall);

//         LowerGroundDisappearEffect groundDisappearEffect =
//             cannonBall.GetComponent<LowerGroundDisappearEffect>();

//         if (groundDisappearEffect == null)
//         {
//             groundDisappearEffect =
//                 cannonBall.AddComponent<LowerGroundDisappearEffect>();
//         }

//         groundDisappearEffect.SetDestroyOnComplete(true);
//         groundDisappearEffect.SetAcceptUnmarkedStaticGround(true);
//         groundDisappearEffect.ResetEffect();


//         ballRigidbody.isKinematic =
//             false;

//         ballRigidbody.useGravity =
//             useGravityForShot;

//         ballRigidbody.angularVelocity =
//             Vector3.zero;

//         ballRigidbody.collisionDetectionMode =
//             CollisionDetectionMode
//                 .ContinuousDynamic;


//         SetBodyVelocity(
//             ballRigidbody,
//             Vector3.zero
//         );


//         IgnoreBallCollisionWithCannon(
//             cannonBall
//         );


//         SetBodyVelocity(
//             ballRigidbody,
//             shotVelocity
//         );

//         ConsumeBall();


//         if (showDebugLines)
//         {
//             Debug.DrawLine(
//                 spawnPosition,
//                 currentAimPoint,
//                 Color.green,
//                 2f
//             );
//         }


//         groundDisappearEffect.ScheduleSmoothFallback(
//             cannonBallLifetime
//         );

//         if (remainingBalls <= 0)
//         {
//             StartOutOfMovesCheck();
//         }
//     }


//     private void StartOutOfMovesCheck()
//     {
//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//         }

//         outOfMovesRoutine =
//             StartCoroutine(
//                 WaitForBallsToResolveThenShowOutOfMoves()
//             );
//     }


//     private IEnumerator WaitForBallsToResolveThenShowOutOfMoves()
//     {
//         /*
//          * Purana version cannonball destroy hone ka wait karta tha.
//          * CannonBallLifetime 5 seconds ho sakti hai, isliye popup late aata tha.
//          *
//          * Ab sirf short configurable delay wait hota hai.
//          * Is delay mein last shot agar level complete kar de to
//          * Level Complete popup priority lega.
//          */
//         if (outOfMovesResultDelay > 0f)
//         {
//             yield return new WaitForSecondsRealtime(
//                 outOfMovesResultDelay
//             );
//         }

//         outOfMovesRoutine = null;

//         if (remainingBalls > 0)
//         {
//             yield break;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             // Level complete ho chuka hai.
//             yield break;
//         }

//         if (showDebugLogs)
//         {
//             Debug.Log(
//                 "Balls finished. OutOfMoves event invoke.",
//                 this
//             );
//         }

//         OutOfMoves?.Invoke();
//     }


//     private bool TryCalculateBallisticVelocity(
//         Vector3 startPosition,
//         Vector3 targetPosition,
//         float speed,
//         out Vector3 launchVelocity)
//     {
//         launchVelocity =
//             Vector3.zero;


//         if (speed <= 0.01f)
//         {
//             return false;
//         }


//         float gravity =
//             Mathf.Abs(
//                 Physics.gravity.y
//             );


//         if (gravity <= 0.001f)
//         {
//             return false;
//         }


//         Vector3 toTarget =
//             targetPosition -
//             startPosition;


//         Vector3 horizontalToTarget =
//             new Vector3(
//                 toTarget.x,
//                 0f,
//                 toTarget.z
//             );


//         float horizontalDistance =
//             horizontalToTarget.magnitude;


//         float verticalDistance =
//             toTarget.y;


//         if (
//             horizontalDistance <
//             0.0001f)
//         {
//             launchVelocity =
//                 new Vector3(
//                     0f,
//                     Mathf.Sign(
//                         verticalDistance == 0f
//                             ? 1f
//                             : verticalDistance
//                     ) * speed,
//                     0f
//                 );

//             return true;
//         }


//         float speedSquared =
//             speed *
//             speed;


//         float discriminant =
//             (
//                 speedSquared *
//                 speedSquared
//             )
//             -
//             gravity *
//             (
//                 gravity *
//                 horizontalDistance *
//                 horizontalDistance
//                 +
//                 2f *
//                 verticalDistance *
//                 speedSquared
//             );


//         if (discriminant < 0f)
//         {
//             return false;
//         }


//         float sqrtDiscriminant =
//             Mathf.Sqrt(
//                 discriminant
//             );


//         float lowArcAngle =
//             Mathf.Atan2(
//                 speedSquared -
//                 sqrtDiscriminant,

//                 gravity *
//                 horizontalDistance
//             );


//         float highArcAngle =
//             Mathf.Atan2(
//                 speedSquared +
//                 sqrtDiscriminant,

//                 gravity *
//                 horizontalDistance
//             );


//         float chosenAngle =
//             preferLowArcTrajectory
//                 ? lowArcAngle
//                 : highArcAngle;


//         Vector3 horizontalDirection =
//             horizontalToTarget.normalized;


//         float horizontalSpeed =
//             speed *
//             Mathf.Cos(
//                 chosenAngle
//             );


//         float verticalSpeed =
//             speed *
//             Mathf.Sin(
//                 chosenAngle
//             );


//         launchVelocity =
//             horizontalDirection *
//             horizontalSpeed
//             +
//             Vector3.up *
//             verticalSpeed;


//         return true;
//     }


//     private bool IsDirectionBehindCannon(
//         Vector3 direction)
//     {
//         Transform forwardSpace =
//             yawPivot.parent != null
//                 ? yawPivot.parent
//                 : transform;


//         Vector3 flatDirection =
//             direction;

//         flatDirection.y =
//             0f;


//         Vector3 flatForward =
//             forwardSpace.forward;

//         flatForward.y =
//             0f;


//         if (
//             flatDirection.sqrMagnitude <
//                 0.0001f ||
//             flatForward.sqrMagnitude <
//                 0.0001f)
//         {
//             return false;
//         }


//         return
//             Vector3.Dot(
//                 flatForward.normalized,
//                 flatDirection.normalized
//             ) <= 0f;
//     }


//     private Vector3 GetSafeUpDirection(
//         Vector3 shotDirection)
//     {
//         Vector3 upDirection =
//             muzzle != null
//                 ? muzzle.up
//                 : Vector3.up;


//         if (
//             Mathf.Abs(
//                 Vector3.Dot(
//                     shotDirection,
//                     upDirection.normalized
//                 )
//             ) > 0.999f)
//         {
//             upDirection =
//                 Vector3.up;
//         }


//         return upDirection;
//     }


//     private void IgnoreBallCollisionWithCannon(
//         GameObject cannonBall)
//     {
//         if (cannonBall == null)
//         {
//             return;
//         }


//         if (
//             cannonColliders == null ||
//             cannonColliders.Length == 0)
//         {
//             CacheCannonColliders();
//         }


//         Collider[] ballColliders =
//             cannonBall
//                 .GetComponentsInChildren<
//                     Collider>(true);


//         foreach (
//             Collider ballCollider
//             in ballColliders)
//         {
//             if (ballCollider == null)
//             {
//                 continue;
//             }


//             foreach (
//                 Collider cannonCollider
//                 in cannonColliders)
//             {
//                 if (
//                     cannonCollider == null)
//                 {
//                     continue;
//                 }


//                 Physics.IgnoreCollision(
//                     ballCollider,
//                     cannonCollider,
//                     true
//                 );
//             }
//         }
//     }


//     private bool ValidateReferences()
//     {
//         if (aimCamera == null)
//         {
//             Debug.LogError(
//                 "Aim Camera missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (
//             yawPivot == null ||
//             pitchPivot == null ||
//             muzzle == null)
//         {
//             Debug.LogError(
//                 "Yaw Pivot, Pitch Pivot ya Muzzle missing hai.",
//                 this
//             );

//             return false;
//         }


//         return true;
//     }


//     private static void SetBodyVelocity(
//         Rigidbody body,
//         Vector3 velocity)
//     {
//         if (body == null)
//         {
//             return;
//         }

// #if UNITY_6000_0_OR_NEWER
//         body.linearVelocity =
//             velocity;
// #else
//         body.velocity =
//             velocity;
// #endif
//     }


// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         if (
//             !showDebugLines ||
//             !hasAimPoint ||
//             muzzle == null)
//         {
//             return;
//         }


//         Gizmos.DrawLine(
//             muzzle.position,
//             currentAimPoint
//         );


//         Gizmos.DrawSphere(
//             currentAimPoint,
//             0.08f
//         );
//     }
// #endif
// }







// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using TMPro;

// public sealed class CannonController : MonoBehaviour
// {
//     [Header("Cannon References")]
//     [SerializeField] private Transform yawPivot;
//     [SerializeField] private Transform pitchPivot;
//     [SerializeField] private Transform muzzle;

//     [Tooltip(
//         "Complete cannon root. Ball cannon colliders ko ignore karegi."
//     )]
//     [SerializeField] private Transform cannonRoot;


//     [Header("Camera And Exact Targeting")]
//     [SerializeField] private Camera aimCamera;

//     [Tooltip(
//         "TowerTarget layer select karein. " +
//         "Block par click hone par isi mask ko sab se pehle check kiya jayega."
//     )]
//     [SerializeField] private LayerMask towerTargetMask;

//     [Tooltip(
//         "Table, Ground aur Environment layers. " +
//         "UI layer yahan include na karein."
//     )]
//     [SerializeField] private LayerMask fallbackAimMask = ~0;

//     [SerializeField, Min(1f)]
//     private float maxAimDistance = 500f;

//     [Tooltip(
//         "Ray kisi collider ko hit na kare to tower ki depth par " +
//         "point calculate hoga."
//     )]
//     [SerializeField]
//     private Transform fallbackAimPlaneReference;

//     [SerializeField, Min(1f)]
//     private float fallbackAimDistance = 100f;


//     [Header("UI Interaction Guard")]

//     [Tooltip(
//         "Pointer UI ke upar ho to cannon aim/shoot trigger nahi hoga."
//     )]
//     [SerializeField]
//     private bool ignoreInputOverUI = true;


//     [Header("Forward Only")]
//     [SerializeField]
//     private bool preventBackwardAim = true;


//     [Header("Aim Limits")]
//     [SerializeField] private float minimumPitch = -10f;
//     [SerializeField] private float maximumPitch = 70f;


//     [Header("Shooting")]
//     [SerializeField] private GameObject cannonBallPrefab;

//     [SerializeField, Min(0f)]
//     private float launchSpeed = 30f;

//     [Tooltip("Ball muzzle se itna aage spawn hogi.")]
//     [SerializeField, Min(0f)]
//     private float spawnForwardOffset = 0.20f;

//     [SerializeField, Min(0f)]
//     private float cannonBallLifetime = 5f;

//     [SerializeField, Min(0f)]
//     private float minimumShotInterval = 0f;


//     [Header("Ball Limit And HUD")]

//     [SerializeField]
//     private LevelRuntimeController levelRuntimeController;

//     [Tooltip(
//         "Last ball fire hone ke baad itna short realtime wait hoga. " +
//         "Is delay mein last shot ko Level Complete trigger karne ka chance milta hai."
//     )]
//     [SerializeField, Min(0f)]
//     private float outOfMovesResultDelay = 0.75f;

//     [Tooltip(
//         "Optional TMP text. Empty ho to top-left HUD automatically create hoga."
//     )]
//     [SerializeField]
//     private TMP_Text ballsRemainingText;

//     [SerializeField]
//     private bool autoCreateBallsHud = true;

//     [SerializeField, Min(1)]
//     private int fallbackBallLimit = 30;


//     [Header("Trajectory")]

//     [Tooltip(
//         "ON: gravity off aur straight exact shot. " +
//         "OFF: gravity ke sath ballistic trajectory."
//     )]
//     [SerializeField]
//     private bool exactStraightShot = true;

//     [Tooltip(
//         "True = low arc. False = high arc."
//     )]
//     [SerializeField]
//     private bool preferLowArcTrajectory = true;

//     [Tooltip(
//         "Arc se target unreachable ho to straight shot fallback."
//     )]
//     [SerializeField]
//     private bool fallbackToStraightIfUnreachable = true;


//     [Header("Bottom Dead Space")]

//     [Tooltip(
//         "Screen ke bottom se pixels. " +
//         "Is area mein aim/shoot nahi chalega. " +
//         "DeadZoneVisual automatically isi value ke sath sync hogi."
//     )]
//     [SerializeField, Min(0f)]
//     private float bottomDeadSpaceHeight = 250f;


//     [Header("Debug")]
//     [SerializeField] private bool showDebugLogs = true;
//     [SerializeField] private bool showDebugLines = true;


//     /*
//      * DeadZoneVisual isi property ko read karegi.
//      *
//      * Isliye dead-zone height sirf CannonController
//      * mein maintain karni hai.
//      */
//     public float BottomDeadSpaceHeight =>
//         bottomDeadSpaceHeight;


//     private Vector3 currentAimPoint;

//     private bool hasAimPoint;
//     private bool pointerStartedInDeadSpace;
//     private bool pointerStartedOverUI;
//     private bool isPointerHeld;

//     private float nextAllowedShotTime;

//     private Collider[] cannonColliders;

//     private LevelRuntimeController subscribedLevelController;
//     private GridLevelData activeBallLevel;
//     private GameObject autoCreatedBallsHud;
//     private int totalBalls;
//     private int remainingBalls;
//     private bool ballLimitInitialized;

//     private readonly List<GameObject> activeCannonBalls =
//         new List<GameObject>();

//     private Coroutine outOfMovesRoutine;

//     public int RemainingBalls => remainingBalls;
//     public int TotalBalls => totalBalls;

//     public event Action OutOfMoves;
//     public event Action<int> ExtraShotsAdded;


//     public void SetGameplayActive(bool active)
//     {
//         if (!active)
//         {
//             isPointerHeld = false;
//             hasAimPoint = false;
//         }

//         if (cannonRoot != null)
//         {
//             cannonRoot.gameObject.SetActive(active);
//         }

//         if (ballsRemainingText != null)
//         {
//             ballsRemainingText.gameObject.SetActive(active);
//         }

//         enabled = active;
//     }


//     private void Awake()
//     {
//         if (aimCamera == null)
//         {
//             aimCamera = Camera.main;
//         }

//         if (cannonRoot == null)
//         {
//             cannonRoot = transform;
//         }

//         CacheCannonColliders();
//     }


//     private void OnEnable()
//     {
//         ConnectLevelController();
//     }


//     private void Start()
//     {
//         ConnectLevelController();
//         EnsureBallsHud();

//         ResetBallLimit(
//             levelRuntimeController != null
//                 ? levelRuntimeController.CurrentLevelData
//                 : null
//         );
//     }


//     private void OnDisable()
//     {
//         DisconnectLevelController();

//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//             outOfMovesRoutine = null;
//         }
//     }


//     private void ConnectLevelController()
//     {
//         if (levelRuntimeController == null)
//         {
//             levelRuntimeController =
//                 FindFirstObjectByType<LevelRuntimeController>();
//         }

//         if (subscribedLevelController ==
//             levelRuntimeController)
//         {
//             return;
//         }

//         DisconnectLevelController();

//         subscribedLevelController =
//             levelRuntimeController;

//         if (subscribedLevelController != null)
//         {
//             subscribedLevelController.LevelGenerated +=
//                 HandleLevelGenerated;
//         }
//     }


//     private void DisconnectLevelController()
//     {
//         if (subscribedLevelController != null)
//         {
//             subscribedLevelController.LevelGenerated -=
//                 HandleLevelGenerated;
//         }

//         subscribedLevelController = null;
//     }


//     private void HandleLevelGenerated(
//         GridLevelData generatedLevel)
//     {
//         ResetBallLimit(generatedLevel);
//     }


//     private void EnsureBallLimitInitialized()
//     {
//         GridLevelData currentLevel =
//             levelRuntimeController != null
//                 ? levelRuntimeController.CurrentLevelData
//                 : null;

//         if (!ballLimitInitialized ||
//             currentLevel != activeBallLevel)
//         {
//             ResetBallLimit(currentLevel);
//         }
//     }


//     private void ResetBallLimit(
//         GridLevelData ballLevel)
//     {
//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//             outOfMovesRoutine = null;
//         }

//         activeCannonBalls.Clear();

//         activeBallLevel = ballLevel;

//         totalBalls = Mathf.Max(
//             1,
//             ballLevel != null
//                 ? ballLevel.AvailableBalls
//                 : fallbackBallLimit
//         );

//         remainingBalls = totalBalls;
//         ballLimitInitialized = true;

//         EnsureBallsHud();
//         UpdateBallsHud();
//     }


//     private void ConsumeBall()
//     {
//         remainingBalls = Mathf.Max(
//             0,
//             remainingBalls - 1
//         );

//         UpdateBallsHud();
//     }


//     public bool AddExtraShots(
//         int amount)
//     {
//         if (amount <= 0)
//         {
//             return false;
//         }

//         EnsureBallLimitInitialized();

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             return false;
//         }

//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//             outOfMovesRoutine = null;
//         }

//         remainingBalls += amount;
//         totalBalls += amount;

//         UpdateBallsHud();

//         ExtraShotsAdded?.Invoke(amount);

//         if (showDebugLogs)
//         {
//             Debug.Log(
//                 $"Extra shots granted: +{amount}. Remaining: {remainingBalls}",
//                 this
//             );
//         }

//         return true;
//     }


//     private void EnsureBallsHud()
//     {
//         if (ballsRemainingText != null ||
//             !autoCreateBallsHud)
//         {
//             return;
//         }

//         Canvas targetCanvas = null;

//         Canvas[] canvases =
//             FindObjectsByType<Canvas>(
//                 FindObjectsInactive.Exclude,
//                 FindObjectsSortMode.None
//             );

//         foreach (Canvas candidate in canvases)
//         {
//             if (candidate != null &&
//                 candidate.renderMode !=
//                 RenderMode.WorldSpace)
//             {
//                 targetCanvas = candidate;
//                 break;
//             }
//         }

//         if (targetCanvas == null)
//         {
//             GameObject canvasObject =
//                 new GameObject(
//                     "Cannon HUD Canvas",
//                     typeof(RectTransform),
//                     typeof(Canvas),
//                     typeof(CanvasScaler),
//                     typeof(GraphicRaycaster)
//                 );

//             int uiLayer =
//                 LayerMask.NameToLayer("UI");

//             if (uiLayer >= 0)
//             {
//                 canvasObject.layer = uiLayer;
//             }

//             targetCanvas =
//                 canvasObject.GetComponent<Canvas>();

//             targetCanvas.renderMode =
//                 RenderMode.ScreenSpaceOverlay;

//             targetCanvas.sortingOrder = 100;

//             CanvasScaler scaler =
//                 canvasObject.GetComponent<CanvasScaler>();

//             scaler.uiScaleMode =
//                 CanvasScaler.ScaleMode.ScaleWithScreenSize;

//             scaler.referenceResolution =
//                 new Vector2(1080f, 1920f);
//         }

//         autoCreatedBallsHud =
//             new GameObject(
//                 "Balls Remaining Text",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         autoCreatedBallsHud.layer =
//             targetCanvas.gameObject.layer;

//         RectTransform textRect =
//             autoCreatedBallsHud.GetComponent<RectTransform>();

//         textRect.SetParent(
//             targetCanvas.transform,
//             false
//         );

//         textRect.anchorMin =
//             new Vector2(0f, 1f);

//         textRect.anchorMax =
//             new Vector2(0f, 1f);

//         textRect.pivot =
//             new Vector2(0f, 1f);

//         textRect.anchoredPosition =
//             new Vector2(32f, -32f);

//         textRect.sizeDelta =
//             new Vector2(420f, 80f);

//         TextMeshProUGUI hudText =
//             autoCreatedBallsHud.GetComponent<TextMeshProUGUI>();

//         hudText.fontSize = 44f;
//         hudText.fontStyle = FontStyles.Bold;
//         hudText.alignment = TextAlignmentOptions.TopLeft;
//         hudText.color = Color.white;
//         hudText.raycastTarget = false;

//         ballsRemainingText = hudText;
//     }


//     private void UpdateBallsHud()
//     {
//         if (ballsRemainingText == null)
//         {
//             return;
//         }

//         ballsRemainingText.text =
//             $"{remainingBalls}";

//         ballsRemainingText.color =
//             remainingBalls > 0
//                 ? Color.white
//                 : new Color(1f, 0.25f, 0.25f, 1f);
//     }


//     private void CacheCannonColliders()
//     {
//         cannonColliders =
//             cannonRoot != null
//                 ? cannonRoot.GetComponentsInChildren<Collider>(true)
//                 : new Collider[0];
//     }


//     private void Update()
//     {
//         Pointer activePointer =
//             Pointer.current;

//         if (activePointer == null)
//         {
//             return;
//         }

//         Vector2 screenPosition =
//             activePointer.position.ReadValue();

//         if (activePointer.press.wasPressedThisFrame)
//         {
//             HandlePointerDown(
//                 screenPosition
//             );
//         }
//         else if (
//             activePointer.press.isPressed &&
//             isPointerHeld)
//         {
//             HandlePointerDrag(
//                 screenPosition
//             );
//         }

//         if (
//             activePointer.press.wasReleasedThisFrame &&
//             isPointerHeld)
//         {
//             HandlePointerUp(
//                 screenPosition
//             );
//         }
//     }


//     private void HandlePointerDown(
//         Vector2 screenPosition)
//     {
//         isPointerHeld =
//             true;

//         pointerStartedOverUI =
//             ignoreInputOverUI &&
//             IsPointerOverUI();

//         pointerStartedInDeadSpace =
//             IsInsideDeadSpace(
//                 screenPosition
//             );

//         if (
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace)
//         {
//             hasAimPoint =
//                 false;

//             return;
//         }

//         UpdateAim(
//             screenPosition
//         );
//     }


//     private void HandlePointerDrag(
//         Vector2 screenPosition)
//     {
//         if (
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace ||
//             IsInsideDeadSpace(screenPosition))
//         {
//             return;
//         }

//         UpdateAim(
//             screenPosition
//         );
//     }


//     private void HandlePointerUp(
//         Vector2 screenPosition)
//     {
//         isPointerHeld =
//             false;

//         bool blocked =
//             pointerStartedOverUI ||
//             pointerStartedInDeadSpace ||
//             IsInsideDeadSpace(
//                 screenPosition
//             );

//         pointerStartedOverUI =
//             false;

//         pointerStartedInDeadSpace =
//             false;

//         if (blocked)
//         {
//             hasAimPoint =
//                 false;

//             return;
//         }

//         /*
//          * Release position se exact aim
//          * dobara calculate.
//          */
//         if (UpdateAim(screenPosition))
//         {
//             Shoot();
//         }
//     }


//     private bool IsPointerOverUI()
//     {
//         if (EventSystem.current == null)
//         {
//             return false;
//         }

//         return
//             EventSystem.current
//                 .IsPointerOverGameObject();
//     }


//     private bool IsInsideDeadSpace(
//         Vector2 screenPosition)
//     {
//         return
//             screenPosition.y <=
//             bottomDeadSpaceHeight;
//     }


//     private bool UpdateAim(
//         Vector2 screenPosition)
//     {
//         if (!ValidateReferences())
//         {
//             hasAimPoint =
//                 false;

//             return false;
//         }

//         Ray screenRay =
//             aimCamera.ScreenPointToRay(
//                 screenPosition
//             );

//         RaycastHit chosenHit;

//         /*
//          * First preference:
//          * tower target.
//          */
//         if (!TryGetClosestValidHit(
//                 screenRay,
//                 towerTargetMask,
//                 true,
//                 out chosenHit))
//         {
//             /*
//              * Tower hit na ho to
//              * environment fallback.
//              */
//             if (!TryGetClosestValidHit(
//                     screenRay,
//                     fallbackAimMask,
//                     false,
//                     out chosenHit))
//             {
//                 currentAimPoint =
//                     GetFallbackAimPoint(
//                         screenRay
//                     );
//             }
//             else
//             {
//                 currentAimPoint =
//                     chosenHit.point;
//             }
//         }
//         else
//         {
//             currentAimPoint =
//                 chosenHit.point;
//         }


//         if (
//             preventBackwardAim &&
//             IsPointBehindCannon(
//                 currentAimPoint
//             ))
//         {
//             hasAimPoint =
//                 false;

//             if (showDebugLogs)
//             {
//                 Debug.Log(
//                     "Rear-side click ignored.",
//                     this
//                 );
//             }

//             return false;
//         }


//         RotateCannonTowards(
//             currentAimPoint
//         );

//         Physics.SyncTransforms();


//         Vector3 finalDirection =
//             currentAimPoint -
//             muzzle.position;

//         if (
//             finalDirection.sqrMagnitude <
//             0.0001f)
//         {
//             hasAimPoint =
//                 false;

//             return false;
//         }

//         hasAimPoint =
//             true;


//         if (showDebugLines)
//         {
//             Debug.DrawLine(
//                 muzzle.position,
//                 currentAimPoint,
//                 Color.red,
//                 0.25f
//             );
//         }


//         if (showDebugLogs)
//         {
//             string hitName =
//                 chosenHit.collider != null
//                     ? chosenHit.collider.name
//                     : "Fallback point";

//             Debug.Log(
//                 $"Aim target: {hitName} | " +
//                 $"Point: {currentAimPoint}",
//                 this
//             );
//         }

//         return true;
//     }


//     private bool TryGetClosestValidHit(
//         Ray ray,
//         LayerMask mask,
//         bool requireTowerTarget,
//         out RaycastHit closestHit)
//     {
//         closestHit =
//             default;

//         if (mask.value == 0)
//         {
//             return false;
//         }


//         RaycastHit[] hits =
//             Physics.RaycastAll(
//                 ray,
//                 maxAimDistance,
//                 mask,
//                 QueryTriggerInteraction.Ignore
//             );


//         bool found =
//             false;

//         float closestDistance =
//             float.MaxValue;


//         foreach (RaycastHit hit in hits)
//         {
//             if (
//                 hit.collider == null ||
//                 IsCannonCollider(
//                     hit.collider
//                 ))
//             {
//                 continue;
//             }


//             if (
//                 requireTowerTarget &&
//                 !IsTowerTarget(
//                     hit.collider
//                 ))
//             {
//                 continue;
//             }


//             if (
//                 hit.distance >=
//                 closestDistance)
//             {
//                 continue;
//             }


//             closestDistance =
//                 hit.distance;

//             closestHit =
//                 hit;

//             found =
//                 true;
//         }

//         return found;
//     }


//     private bool IsTowerTarget(
//         Collider hitCollider)
//     {
//         if (hitCollider == null)
//         {
//             return false;
//         }


//         /*
//          * New generic tower system mein
//          * PhysicsTowerObject check.
//          */
//         if (hitCollider.GetComponentInParent<
//                 PhysicsTowerObject>() != null)
//         {
//             return true;
//         }


//         int layerBit =
//             1 <<
//             hitCollider.gameObject.layer;


//         return
//             (towerTargetMask.value &
//              layerBit) != 0;
//     }


//     private bool IsCannonCollider(
//         Collider candidate)
//     {
//         if (
//             candidate == null ||
//             cannonRoot == null)
//         {
//             return false;
//         }


//         Transform candidateTransform =
//             candidate.transform;


//         return
//             candidateTransform ==
//                 cannonRoot ||
//             candidateTransform.IsChildOf(
//                 cannonRoot
//             );
//     }


//     private Vector3 GetFallbackAimPoint(
//         Ray screenRay)
//     {
//         if (
//             fallbackAimPlaneReference !=
//             null)
//         {
//             Plane aimPlane =
//                 new Plane(
//                     -aimCamera.transform.forward,
//                     fallbackAimPlaneReference.position
//                 );


//             if (
//                 aimPlane.Raycast(
//                     screenRay,
//                     out float enterDistance) &&
//                 enterDistance > 0f)
//             {
//                 return
//                     screenRay.GetPoint(
//                         enterDistance
//                     );
//             }
//         }


//         return
//             screenRay.GetPoint(
//                 fallbackAimDistance
//             );
//     }


//     private bool IsPointBehindCannon(
//         Vector3 targetPoint)
//     {
//         Transform forwardSpace =
//             yawPivot.parent != null
//                 ? yawPivot.parent
//                 : transform;


//         Vector3 flatDirection =
//             targetPoint -
//             yawPivot.position;

//         flatDirection.y =
//             0f;


//         Vector3 flatForward =
//             forwardSpace.forward;

//         flatForward.y =
//             0f;


//         if (
//             flatDirection.sqrMagnitude <
//                 0.0001f ||
//             flatForward.sqrMagnitude <
//                 0.0001f)
//         {
//             return false;
//         }


//         return
//             Vector3.Dot(
//                 flatForward.normalized,
//                 flatDirection.normalized
//             ) <= 0f;
//     }


//     private void RotateCannonTowards(
//         Vector3 targetPoint)
//     {
//         if (
//             yawPivot == null ||
//             pitchPivot == null)
//         {
//             return;
//         }

//         RotateYawTowards(
//             targetPoint
//         );

//         RotatePitchTowards(
//             targetPoint
//         );
//     }


//     private void RotateYawTowards(
//         Vector3 targetPoint)
//     {
//         Vector3 worldDirection =
//             targetPoint -
//             yawPivot.position;

//         worldDirection.y =
//             0f;


//         if (
//             worldDirection.sqrMagnitude <
//             0.001f)
//         {
//             return;
//         }


//         Vector3 localDirection;


//         if (yawPivot.parent != null)
//         {
//             localDirection =
//                 yawPivot.parent
//                     .InverseTransformDirection(
//                         worldDirection
//                     );
//         }
//         else
//         {
//             localDirection =
//                 worldDirection;
//         }


//         float yawAngle =
//             Mathf.Atan2(
//                 localDirection.x,
//                 localDirection.z
//             ) *
//             Mathf.Rad2Deg;


//         yawPivot.localRotation =
//             Quaternion.Euler(
//                 0f,
//                 yawAngle,
//                 0f
//             );
//     }


//     private void RotatePitchTowards(
//         Vector3 targetPoint)
//     {
//         Vector3 worldDirection =
//             targetPoint -
//             pitchPivot.position;


//         Vector3 localDirection =
//             yawPivot
//                 .InverseTransformDirection(
//                     worldDirection
//                 );


//         float horizontalDistance =
//             new Vector2(
//                 localDirection.x,
//                 localDirection.z
//             ).magnitude;


//         float pitchAngle =
//             Mathf.Atan2(
//                 localDirection.y,
//                 horizontalDistance
//             ) *
//             Mathf.Rad2Deg;


//         pitchAngle =
//             Mathf.Clamp(
//                 pitchAngle,
//                 minimumPitch,
//                 maximumPitch
//             );


//         pitchPivot.localRotation =
//             Quaternion.Euler(
//                 -pitchAngle,
//                 0f,
//                 0f
//             );
//     }


//     public void Shoot()
//     {
//         EnsureBallLimitInitialized();

//         if (
//             remainingBalls <= 0 ||
//             (levelRuntimeController != null &&
//              !levelRuntimeController.IsLevelGenerated) ||
//             !hasAimPoint ||
//             Time.time <
//             nextAllowedShotTime)
//         {
//             return;
//         }


//         if (
//             cannonBallPrefab == null ||
//             muzzle == null)
//         {
//             return;
//         }


//         Physics.SyncTransforms();


//         Vector3 spawnOrigin =
//             muzzle.position;


//         bool useGravityForShot =
//             !exactStraightShot;


//         Vector3 shotVelocity;


//         if (useGravityForShot)
//         {
//             bool solved =
//                 TryCalculateBallisticVelocity(
//                     spawnOrigin,
//                     currentAimPoint,
//                     launchSpeed,
//                     out shotVelocity
//                 );


//             if (!solved)
//             {
//                 if (
//                     !fallbackToStraightIfUnreachable)
//                 {
//                     if (showDebugLogs)
//                     {
//                         Debug.LogWarning(
//                             "Target current Launch Speed " +
//                             "se reach nahi ho sakta.",
//                             this
//                         );
//                     }

//                     return;
//                 }


//                 useGravityForShot =
//                     false;


//                 Vector3 straightDirection =
//                     currentAimPoint -
//                     spawnOrigin;


//                 if (
//                     straightDirection.sqrMagnitude <
//                     0.0001f)
//                 {
//                     return;
//                 }


//                 shotVelocity =
//                     straightDirection.normalized *
//                     launchSpeed;


//                 if (showDebugLogs)
//                 {
//                     Debug.LogWarning(
//                         "Target unreachable. " +
//                         "Straight fallback shot.",
//                         this
//                     );
//                 }
//             }
//         }
//         else
//         {
//             Vector3 straightDirection =
//                 currentAimPoint -
//                 spawnOrigin;


//             if (
//                 straightDirection.sqrMagnitude <
//                 0.0001f)
//             {
//                 return;
//             }


//             shotVelocity =
//                 straightDirection.normalized *
//                 launchSpeed;
//         }


//         Vector3 shotDirection =
//             shotVelocity.sqrMagnitude >
//             0.0001f
//                 ? shotVelocity.normalized
//                 : (
//                     currentAimPoint -
//                     spawnOrigin
//                 ).normalized;


//         if (
//             preventBackwardAim &&
//             IsDirectionBehindCannon(
//                 shotDirection
//             ))
//         {
//             return;
//         }


//         nextAllowedShotTime =
//             Time.time +
//             minimumShotInterval;


//         Vector3 spawnPosition =
//             spawnOrigin +
//             shotDirection *
//             spawnForwardOffset;


//         Quaternion spawnRotation =
//             Quaternion.LookRotation(
//                 shotDirection,
//                 GetSafeUpDirection(
//                     shotDirection
//                 )
//             );


//         GameObject cannonBall =
//             Instantiate(
//                 cannonBallPrefab,
//                 spawnPosition,
//                 spawnRotation
//             );


//         Rigidbody ballRigidbody =
//             cannonBall
//                 .GetComponent<Rigidbody>();


//         if (ballRigidbody == null)
//         {
//             Debug.LogError(
//                 "Cannonball prefab par Rigidbody missing hai.",
//                 cannonBall
//             );

//             Destroy(
//                 cannonBall
//             );

//             return;
//         }

//         activeCannonBalls.Add(cannonBall);

//         LowerGroundDisappearEffect groundDisappearEffect =
//             cannonBall.GetComponent<LowerGroundDisappearEffect>();

//         if (groundDisappearEffect == null)
//         {
//             groundDisappearEffect =
//                 cannonBall.AddComponent<LowerGroundDisappearEffect>();
//         }

//         groundDisappearEffect.SetDestroyOnComplete(true);
//         groundDisappearEffect.SetAcceptUnmarkedStaticGround(true);
//         groundDisappearEffect.ResetEffect();


//         ballRigidbody.isKinematic =
//             false;

//         ballRigidbody.useGravity =
//             useGravityForShot;

//         ballRigidbody.angularVelocity =
//             Vector3.zero;

//         ballRigidbody.collisionDetectionMode =
//             CollisionDetectionMode
//                 .ContinuousDynamic;


//         SetBodyVelocity(
//             ballRigidbody,
//             Vector3.zero
//         );


//         IgnoreBallCollisionWithCannon(
//             cannonBall
//         );


//         SetBodyVelocity(
//             ballRigidbody,
//             shotVelocity
//         );

//         ConsumeBall();


//         if (showDebugLines)
//         {
//             Debug.DrawLine(
//                 spawnPosition,
//                 currentAimPoint,
//                 Color.green,
//                 2f
//             );
//         }


//         groundDisappearEffect.ScheduleSmoothFallback(
//             cannonBallLifetime
//         );

//         if (remainingBalls <= 0)
//         {
//             StartOutOfMovesCheck();
//         }
//     }


//     private void StartOutOfMovesCheck()
//     {
//         if (outOfMovesRoutine != null)
//         {
//             StopCoroutine(outOfMovesRoutine);
//         }

//         outOfMovesRoutine =
//             StartCoroutine(
//                 WaitForBallsToResolveThenShowOutOfMoves()
//             );
//     }


//     private IEnumerator WaitForBallsToResolveThenShowOutOfMoves()
//     {
//         /*
//          * Purana version cannonball destroy hone ka wait karta tha.
//          * CannonBallLifetime 5 seconds ho sakti hai, isliye popup late aata tha.
//          *
//          * Ab sirf short configurable delay wait hota hai.
//          * Is delay mein last shot agar level complete kar de to
//          * Level Complete popup priority lega.
//          */
//         if (outOfMovesResultDelay > 0f)
//         {
//             yield return new WaitForSecondsRealtime(
//                 outOfMovesResultDelay
//             );
//         }

//         outOfMovesRoutine = null;

//         if (remainingBalls > 0)
//         {
//             yield break;
//         }

//         if (levelRuntimeController != null &&
//             !levelRuntimeController.IsLevelGenerated)
//         {
//             // Level complete ho chuka hai.
//             yield break;
//         }

//         if (showDebugLogs)
//         {
//             Debug.Log(
//                 "Balls finished. OutOfMoves event invoke.",
//                 this
//             );
//         }

//         OutOfMoves?.Invoke();
//     }


//     private bool TryCalculateBallisticVelocity(
//         Vector3 startPosition,
//         Vector3 targetPosition,
//         float speed,
//         out Vector3 launchVelocity)
//     {
//         launchVelocity =
//             Vector3.zero;


//         if (speed <= 0.01f)
//         {
//             return false;
//         }


//         float gravity =
//             Mathf.Abs(
//                 Physics.gravity.y
//             );


//         if (gravity <= 0.001f)
//         {
//             return false;
//         }


//         Vector3 toTarget =
//             targetPosition -
//             startPosition;


//         Vector3 horizontalToTarget =
//             new Vector3(
//                 toTarget.x,
//                 0f,
//                 toTarget.z
//             );


//         float horizontalDistance =
//             horizontalToTarget.magnitude;


//         float verticalDistance =
//             toTarget.y;


//         if (
//             horizontalDistance <
//             0.0001f)
//         {
//             launchVelocity =
//                 new Vector3(
//                     0f,
//                     Mathf.Sign(
//                         verticalDistance == 0f
//                             ? 1f
//                             : verticalDistance
//                     ) * speed,
//                     0f
//                 );

//             return true;
//         }


//         float speedSquared =
//             speed *
//             speed;


//         float discriminant =
//             (
//                 speedSquared *
//                 speedSquared
//             )
//             -
//             gravity *
//             (
//                 gravity *
//                 horizontalDistance *
//                 horizontalDistance
//                 +
//                 2f *
//                 verticalDistance *
//                 speedSquared
//             );


//         if (discriminant < 0f)
//         {
//             return false;
//         }


//         float sqrtDiscriminant =
//             Mathf.Sqrt(
//                 discriminant
//             );


//         float lowArcAngle =
//             Mathf.Atan2(
//                 speedSquared -
//                 sqrtDiscriminant,

//                 gravity *
//                 horizontalDistance
//             );


//         float highArcAngle =
//             Mathf.Atan2(
//                 speedSquared +
//                 sqrtDiscriminant,

//                 gravity *
//                 horizontalDistance
//             );


//         float chosenAngle =
//             preferLowArcTrajectory
//                 ? lowArcAngle
//                 : highArcAngle;


//         Vector3 horizontalDirection =
//             horizontalToTarget.normalized;


//         float horizontalSpeed =
//             speed *
//             Mathf.Cos(
//                 chosenAngle
//             );


//         float verticalSpeed =
//             speed *
//             Mathf.Sin(
//                 chosenAngle
//             );


//         launchVelocity =
//             horizontalDirection *
//             horizontalSpeed
//             +
//             Vector3.up *
//             verticalSpeed;


//         return true;
//     }


//     private bool IsDirectionBehindCannon(
//         Vector3 direction)
//     {
//         Transform forwardSpace =
//             yawPivot.parent != null
//                 ? yawPivot.parent
//                 : transform;


//         Vector3 flatDirection =
//             direction;

//         flatDirection.y =
//             0f;


//         Vector3 flatForward =
//             forwardSpace.forward;

//         flatForward.y =
//             0f;


//         if (
//             flatDirection.sqrMagnitude <
//                 0.0001f ||
//             flatForward.sqrMagnitude <
//                 0.0001f)
//         {
//             return false;
//         }


//         return
//             Vector3.Dot(
//                 flatForward.normalized,
//                 flatDirection.normalized
//             ) <= 0f;
//     }


//     private Vector3 GetSafeUpDirection(
//         Vector3 shotDirection)
//     {
//         Vector3 upDirection =
//             muzzle != null
//                 ? muzzle.up
//                 : Vector3.up;


//         if (
//             Mathf.Abs(
//                 Vector3.Dot(
//                     shotDirection,
//                     upDirection.normalized
//                 )
//             ) > 0.999f)
//         {
//             upDirection =
//                 Vector3.up;
//         }


//         return upDirection;
//     }


//     private void IgnoreBallCollisionWithCannon(
//         GameObject cannonBall)
//     {
//         if (cannonBall == null)
//         {
//             return;
//         }


//         if (
//             cannonColliders == null ||
//             cannonColliders.Length == 0)
//         {
//             CacheCannonColliders();
//         }


//         Collider[] ballColliders =
//             cannonBall
//                 .GetComponentsInChildren<
//                     Collider>(true);


//         foreach (
//             Collider ballCollider
//             in ballColliders)
//         {
//             if (ballCollider == null)
//             {
//                 continue;
//             }


//             foreach (
//                 Collider cannonCollider
//                 in cannonColliders)
//             {
//                 if (
//                     cannonCollider == null)
//                 {
//                     continue;
//                 }


//                 Physics.IgnoreCollision(
//                     ballCollider,
//                     cannonCollider,
//                     true
//                 );
//             }
//         }
//     }


//     private bool ValidateReferences()
//     {
//         if (aimCamera == null)
//         {
//             Debug.LogError(
//                 "Aim Camera missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (
//             yawPivot == null ||
//             pitchPivot == null ||
//             muzzle == null)
//         {
//             Debug.LogError(
//                 "Yaw Pivot, Pitch Pivot ya Muzzle missing hai.",
//                 this
//             );

//             return false;
//         }


//         return true;
//     }


//     private static void SetBodyVelocity(
//         Rigidbody body,
//         Vector3 velocity)
//     {
//         if (body == null)
//         {
//             return;
//         }

// #if UNITY_6000_0_OR_NEWER
//         body.linearVelocity =
//             velocity;
// #else
//         body.velocity =
//             velocity;
// #endif
//     }


// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         if (
//             !showDebugLines ||
//             !hasAimPoint ||
//             muzzle == null)
//         {
//             return;
//         }


//         Gizmos.DrawLine(
//             muzzle.position,
//             currentAimPoint
//         );


//         Gizmos.DrawSphere(
//             currentAimPoint,
//             0.08f
//         );
//     }
// #endif
// }




using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public sealed class CannonController : MonoBehaviour
{
    [Header("Cannon References")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchPivot;
    [SerializeField] private Transform muzzle;

    [Tooltip(
        "Complete cannon root. Ball cannon colliders ko ignore karegi."
    )]
    [SerializeField] private Transform cannonRoot;


    [Header("Level Entrance Animation")]

    [Tooltip(
        "Blocks load hone ke baad cannon bottom se apni rest position par " +
        "animate hogi."
    )]
    [SerializeField]
    private bool animateCannonEntrance = true;

    [SerializeField, Min(0f)]
    private float cannonEntranceInitialDelay = 0.03f;

    [SerializeField, Min(0.01f)]
    private float cannonEntranceDuration = 0.32f;

    [SerializeField, Min(0f)]
    private float cannonEntranceBottomOffset = 2.4f;

    [SerializeField, Range(0.1f, 1f)]
    private float cannonEntranceStartScale = 0.82f;


    [Header("Camera And Exact Targeting")]
    [SerializeField] private Camera aimCamera;

    [Tooltip(
        "TowerTarget layer select karein. " +
        "Block par click hone par isi mask ko sab se pehle check kiya jayega."
    )]
    [SerializeField] private LayerMask towerTargetMask;

    [Tooltip(
        "Table, Ground aur Environment layers. " +
        "UI layer yahan include na karein."
    )]
    [SerializeField] private LayerMask fallbackAimMask = ~0;

    [SerializeField, Min(1f)]
    private float maxAimDistance = 500f;

    [Tooltip(
        "Ray kisi collider ko hit na kare to tower ki depth par " +
        "point calculate hoga."
    )]
    [SerializeField]
    private Transform fallbackAimPlaneReference;

    [SerializeField, Min(1f)]
    private float fallbackAimDistance = 100f;


    [Header("UI Interaction Guard")]

    [Tooltip(
        "Pointer UI ke upar ho to cannon aim/shoot trigger nahi hoga."
    )]
    [SerializeField]
    private bool ignoreInputOverUI = true;


    [Header("Forward Only")]
    [SerializeField]
    private bool preventBackwardAim = true;


    [Header("Aim Limits")]
    [SerializeField] private float minimumPitch = -10f;
    [SerializeField] private float maximumPitch = 70f;


    [Header("Shooting")]
    [SerializeField] private GameObject cannonBallPrefab;

    [SerializeField, Min(0f)]
    private float launchSpeed = 30f;

    [Tooltip("Ball muzzle se itna aage spawn hogi.")]
    [SerializeField, Min(0f)]
    private float spawnForwardOffset = 0.20f;

    [SerializeField, Min(0f)]
    private float cannonBallLifetime = 5f;

    [SerializeField, Min(0f)]
    private float minimumShotInterval = 0f;


    [Header("Runtime Power Up State")]

    [Tooltip(
        "Runtime value. GameplayPowerUpController isko current level ke liye control karega."
    )]
    [SerializeField]
    private bool infiniteBallsEnabled = false;

    [Tooltip(
        "Runtime ball size multiplier. Normal = 1."
    )]
    [SerializeField, Min(0.01f)]
    private float runtimeBallSizeMultiplier = 1f;

    [Tooltip(
        "Runtime launch speed/force multiplier. Normal = 1."
    )]
    [SerializeField, Min(0.01f)]
    private float runtimeLaunchForceMultiplier = 1f;


    [Header("Ball Limit And HUD")]

    [SerializeField]
    private LevelRuntimeController levelRuntimeController;

    [Tooltip(
        "Tamam fired balls destroy hone ke baad itna realtime settle wait hoga. " +
        "Is delay mein girte blocks ko Level Complete trigger karne ka chance milta hai."
    )]
    [SerializeField, Min(0f)]
    private float outOfMovesResultDelay = 0.75f;

    [Tooltip(
        "Optional TMP text. Empty ho to top-left HUD automatically create hoga."
    )]
    [SerializeField]
    private TMP_Text ballsRemainingText;

    [SerializeField]
    private bool autoCreateBallsHud = true;

    [SerializeField, Min(1)]
    private int fallbackBallLimit = 30;


    [Header("Trajectory")]

    [Tooltip(
        "ON: gravity off aur straight exact shot. " +
        "OFF: gravity ke sath ballistic trajectory."
    )]
    [SerializeField]
    private bool exactStraightShot = true;

    [Tooltip(
        "True = low arc. False = high arc."
    )]
    [SerializeField]
    private bool preferLowArcTrajectory = true;

    [Tooltip(
        "Arc se target unreachable ho to straight shot fallback."
    )]
    [SerializeField]
    private bool fallbackToStraightIfUnreachable = true;


    [Header("Bottom Dead Space")]

    [Tooltip(
        "Screen ke bottom se pixels. " +
        "Is area mein aim/shoot nahi chalega. " +
        "DeadZoneVisual automatically isi value ke sath sync hogi."
    )]
    [SerializeField, Min(0f)]
    private float bottomDeadSpaceHeight = 250f;


    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showDebugLines = true;


    /*
     * DeadZoneVisual isi property ko read karegi.
     *
     * Isliye dead-zone height sirf CannonController
     * mein maintain karni hai.
     */
    public float BottomDeadSpaceHeight =>
        bottomDeadSpaceHeight;

    public Transform RocketLaunchPoint =>
        muzzle != null
            ? muzzle
            : transform;

    public Vector3 RocketShootDirection
    {
        get
        {
            if (muzzle == null)
            {
                return transform.forward;
            }

            Vector3 direction = hasAimPoint
                ? currentAimPoint - muzzle.position
                : muzzle.forward;

            return direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : muzzle.forward;
        }
    }


    private Vector3 currentAimPoint;

    private bool hasAimPoint;
    private bool pointerStartedInDeadSpace;
    private bool pointerStartedOverUI;
    private bool isPointerHeld;

    private float nextAllowedShotTime;

    private Collider[] cannonColliders;

    private LevelRuntimeController subscribedLevelController;
    private GridLevelData activeBallLevel;
    private GameObject autoCreatedBallsHud;
    private int totalBalls;
    private int remainingBalls;
    private bool ballLimitInitialized;

    private readonly List<GameObject> activeCannonBalls =
        new List<GameObject>();

    private Coroutine outOfMovesRoutine;

    private Coroutine cannonEntranceRoutine;
    private Vector3 cannonRestLocalPosition;
    private Vector3 cannonRestLocalScale;
    private bool cannonRestPoseCaptured;
    private bool isCannonEntranceAnimating;

    public int RemainingBalls => remainingBalls;
    public int TotalBalls => totalBalls;

    public bool InfiniteBallsEnabled =>
        infiniteBallsEnabled;

    public float RuntimeBallSizeMultiplier =>
        runtimeBallSizeMultiplier;

    public float RuntimeLaunchForceMultiplier =>
        runtimeLaunchForceMultiplier;

    public event Action OutOfMoves;
    public event Action<int> ExtraShotsAdded;


    public void SetGameplayActive(bool active)
    {
        if (!active)
        {
            isPointerHeld = false;
            hasAimPoint = false;
        }

        if (cannonRoot != null)
        {
            cannonRoot.gameObject.SetActive(active);
        }

       if (ballsRemainingText != null)
{
    ballsRemainingText.gameObject.SetActive(true);
}

        enabled = active;
    }


    private void Awake()
    {
        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        if (cannonRoot == null)
        {
            cannonRoot = transform;
        }

        CaptureCannonRestPose();
        CacheCannonColliders();
    }


    private void OnEnable()
    {
        ConnectLevelController();
    }


    private void Start()
    {
        ConnectLevelController();
        EnsureBallsHud();

        ResetBallLimit(
            levelRuntimeController != null
                ? levelRuntimeController.CurrentLevelData
                : null
        );
    }


    private void OnDisable()
    {
        DisconnectLevelController();

        if (outOfMovesRoutine != null)
        {
            StopCoroutine(outOfMovesRoutine);
            outOfMovesRoutine = null;
        }

        StopCannonEntranceAnimation(true);
    }


    private void ConnectLevelController()
    {
        if (levelRuntimeController == null)
        {
            levelRuntimeController =
                FindFirstObjectByType<LevelRuntimeController>();
        }

        if (subscribedLevelController ==
            levelRuntimeController)
        {
            return;
        }

        DisconnectLevelController();

        subscribedLevelController =
            levelRuntimeController;

        if (subscribedLevelController != null)
        {
            subscribedLevelController.LevelLoadingStarted +=
                HandleLevelLoadingStarted;

            subscribedLevelController.LevelGenerated +=
                HandleLevelGenerated;

            subscribedLevelController.LevelCompleted +=
                HandleLevelCompleted;
        }
    }


    private void DisconnectLevelController()
    {
        if (subscribedLevelController != null)
        {
            subscribedLevelController.LevelLoadingStarted -=
                HandleLevelLoadingStarted;

            subscribedLevelController.LevelGenerated -=
                HandleLevelGenerated;

            subscribedLevelController.LevelCompleted -=
                HandleLevelCompleted;
        }

        subscribedLevelController = null;
    }


    private void HandleLevelLoadingStarted()
    {
        PrepareCannonEntrance();
    }


    private void HandleLevelGenerated(
        GridLevelData generatedLevel)
    {
        ResetBallLimit(generatedLevel);

        if (!animateCannonEntrance || cannonRoot == null)
        {
            StopCannonEntranceAnimation(true);
            return;
        }

        if (!isCannonEntranceAnimating)
        {
            PrepareCannonEntrance();
        }

        cannonEntranceRoutine = StartCoroutine(
            AnimateCannonEntranceRoutine()
        );
    }


    private void HandleLevelCompleted(
        GridLevelData completedLevel)
    {
        if (outOfMovesRoutine == null)
        {
            return;
        }

        StopCoroutine(outOfMovesRoutine);
        outOfMovesRoutine = null;
    }


    private void CaptureCannonRestPose()
    {
        if (cannonRestPoseCaptured || cannonRoot == null)
        {
            return;
        }

        cannonRestLocalPosition = cannonRoot.localPosition;
        cannonRestLocalScale = cannonRoot.localScale;
        cannonRestPoseCaptured = true;
    }


    private void PrepareCannonEntrance()
    {
        if (!animateCannonEntrance || cannonRoot == null)
        {
            isCannonEntranceAnimating = false;
            return;
        }

        CaptureCannonRestPose();
        StopCannonEntranceAnimation(false);

        cannonRoot.localPosition =
            cannonRestLocalPosition -
            Vector3.up * cannonEntranceBottomOffset;

        cannonRoot.localScale =
            cannonRestLocalScale * cannonEntranceStartScale;

        isPointerHeld = false;
        hasAimPoint = false;
        isCannonEntranceAnimating = true;
    }


    private IEnumerator AnimateCannonEntranceRoutine()
    {
        if (cannonEntranceInitialDelay > 0f)
        {
            float delayElapsed = 0f;

            while (delayElapsed < cannonEntranceInitialDelay)
            {
                delayElapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        Vector3 startPosition = cannonRoot.localPosition;
        Vector3 startScale = cannonRoot.localScale;
        float duration = Mathf.Max(0.01f, cannonEntranceDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (cannonRoot == null)
            {
                cannonEntranceRoutine = null;
                isCannonEntranceAnimating = false;
                yield break;
            }

            float normalizedTime =
                Mathf.Clamp01(elapsed / duration);

            float smoothProgress =
                normalizedTime *
                normalizedTime *
                (3f - 2f * normalizedTime);

            float popProgress =
                EaseOutBack(normalizedTime);

            cannonRoot.localPosition =
                Vector3.LerpUnclamped(
                    startPosition,
                    cannonRestLocalPosition,
                    smoothProgress
                );

            cannonRoot.localScale =
                Vector3.LerpUnclamped(
                    startScale,
                    cannonRestLocalScale,
                    popProgress
                );

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        cannonRoot.localPosition = cannonRestLocalPosition;
        cannonRoot.localScale = cannonRestLocalScale;
        cannonEntranceRoutine = null;
        isCannonEntranceAnimating = false;
    }


    private void StopCannonEntranceAnimation(bool restoreRestPose)
    {
        if (cannonEntranceRoutine != null)
        {
            StopCoroutine(cannonEntranceRoutine);
            cannonEntranceRoutine = null;
        }

        if (restoreRestPose &&
            cannonRestPoseCaptured &&
            cannonRoot != null)
        {
            cannonRoot.localPosition = cannonRestLocalPosition;
            cannonRoot.localScale = cannonRestLocalScale;
        }

        isCannonEntranceAnimating = false;
    }


    private static float EaseOutBack(float value)
    {
        const float overshoot = 1.70158f;
        float shiftedValue = value - 1f;

        return
            1f +
            (overshoot + 1f) *
            shiftedValue * shiftedValue * shiftedValue +
            overshoot * shiftedValue * shiftedValue;
    }


    private void EnsureBallLimitInitialized()
    {
        GridLevelData currentLevel =
            levelRuntimeController != null
                ? levelRuntimeController.CurrentLevelData
                : null;

        if (!ballLimitInitialized ||
            currentLevel != activeBallLevel)
        {
            ResetBallLimit(currentLevel);
        }
    }


    private void ResetBallLimit(
        GridLevelData ballLevel)
    {
        if (outOfMovesRoutine != null)
        {
            StopCoroutine(outOfMovesRoutine);
            outOfMovesRoutine = null;
        }

        activeCannonBalls.Clear();

        activeBallLevel = ballLevel;

        totalBalls = Mathf.Max(
            1,
            ballLevel != null
                ? ballLevel.AvailableBalls
                : fallbackBallLimit
        );

        remainingBalls = totalBalls;
        ballLimitInitialized = true;

        EnsureBallsHud();
        UpdateBallsHud();
    }


    private void ConsumeBall()
    {
        if (infiniteBallsEnabled)
        {
            UpdateBallsHud();
            return;
        }

        remainingBalls = Mathf.Max(
            0,
            remainingBalls - 1
        );

        UpdateBallsHud();
    }


    public bool AddExtraShots(
        int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        EnsureBallLimitInitialized();

        if (levelRuntimeController != null &&
            !levelRuntimeController.IsLevelGenerated)
        {
            return false;
        }

        if (outOfMovesRoutine != null)
        {
            StopCoroutine(outOfMovesRoutine);
            outOfMovesRoutine = null;
        }

        remainingBalls += amount;
        totalBalls += amount;

        UpdateBallsHud();

        ExtraShotsAdded?.Invoke(amount);

        if (showDebugLogs)
        {
            Debug.Log(
                $"Extra shots granted: +{amount}. Remaining: {remainingBalls}",
                this
            );
        }

        return true;
    }


    public void SetInfiniteBallsEnabled(
        bool enabledValue)
    {
        infiniteBallsEnabled =
            enabledValue;

        if (infiniteBallsEnabled &&
            outOfMovesRoutine != null)
        {
            StopCoroutine(
                outOfMovesRoutine
            );

            outOfMovesRoutine = null;
        }

        UpdateBallsHud();
    }


    public void SetRuntimeBallSizeMultiplier(
        float multiplier)
    {
        runtimeBallSizeMultiplier =
            Mathf.Max(
                0.01f,
                multiplier
            );
    }


    public void SetRuntimeLaunchForceMultiplier(
        float multiplier)
    {
        runtimeLaunchForceMultiplier =
            Mathf.Max(
                0.01f,
                multiplier
            );
    }


    public void ResetRuntimePowerUps()
    {
        infiniteBallsEnabled = false;
        runtimeBallSizeMultiplier = 1f;
        runtimeLaunchForceMultiplier = 1f;

        UpdateBallsHud();
    }


    private void EnsureBallsHud()
    {
        if (ballsRemainingText != null ||
            !autoCreateBallsHud)
        {
            return;
        }

        Canvas targetCanvas = null;

        Canvas[] canvases =
            FindObjectsByType<Canvas>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (Canvas candidate in canvases)
        {
            if (candidate != null &&
                candidate.renderMode !=
                RenderMode.WorldSpace)
            {
                targetCanvas = candidate;
                break;
            }
        }

        if (targetCanvas == null)
        {
            GameObject canvasObject =
                new GameObject(
                    "Cannon HUD Canvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster)
                );

            int uiLayer =
                LayerMask.NameToLayer("UI");

            if (uiLayer >= 0)
            {
                canvasObject.layer = uiLayer;
            }

            targetCanvas =
                canvasObject.GetComponent<Canvas>();

            targetCanvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            targetCanvas.sortingOrder = 100;

            CanvasScaler scaler =
                canvasObject.GetComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1080f, 1920f);
        }

        autoCreatedBallsHud =
            new GameObject(
                "Balls Remaining Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );

        autoCreatedBallsHud.layer =
            targetCanvas.gameObject.layer;

        RectTransform textRect =
            autoCreatedBallsHud.GetComponent<RectTransform>();

        textRect.SetParent(
            targetCanvas.transform,
            false
        );

        textRect.anchorMin =
            new Vector2(0f, 1f);

        textRect.anchorMax =
            new Vector2(0f, 1f);

        textRect.pivot =
            new Vector2(0f, 1f);

        textRect.anchoredPosition =
            new Vector2(32f, -32f);

        textRect.sizeDelta =
            new Vector2(420f, 80f);

        TextMeshProUGUI hudText =
            autoCreatedBallsHud.GetComponent<TextMeshProUGUI>();

        hudText.fontSize = 44f;
        hudText.fontStyle = FontStyles.Bold;
        hudText.alignment = TextAlignmentOptions.TopLeft;
        hudText.color = Color.white;
        hudText.raycastTarget = false;

        ballsRemainingText = hudText;
    }


    private void UpdateBallsHud()
    {
        if (ballsRemainingText == null)
        {
            return;
        }

        ballsRemainingText.text =
            infiniteBallsEnabled
                ? "∞"
                : $"{remainingBalls}";

        ballsRemainingText.color =
            infiniteBallsEnabled ||
            remainingBalls > 0
                ? Color.white
                : new Color(1f, 0.25f, 0.25f, 1f);
    }


    private void CacheCannonColliders()
    {
        cannonColliders =
            cannonRoot != null
                ? cannonRoot.GetComponentsInChildren<Collider>(true)
                : new Collider[0];
    }


    private void Update()
    {
        if (isCannonEntranceAnimating)
        {
            isPointerHeld = false;
            hasAimPoint = false;
            return;
        }

        Pointer activePointer =
            Pointer.current;

        if (activePointer == null)
        {
            return;
        }

        Vector2 screenPosition =
            activePointer.position.ReadValue();

        if (activePointer.press.wasPressedThisFrame)
        {
            HandlePointerDown(
                screenPosition
            );
        }
        else if (
            activePointer.press.isPressed &&
            isPointerHeld)
        {
            HandlePointerDrag(
                screenPosition
            );
        }

        if (
            activePointer.press.wasReleasedThisFrame &&
            isPointerHeld)
        {
            HandlePointerUp(
                screenPosition
            );
        }
    }


    private void HandlePointerDown(
        Vector2 screenPosition)
    {
        isPointerHeld =
            true;

        pointerStartedOverUI =
            ignoreInputOverUI &&
            IsPointerOverUI();

        pointerStartedInDeadSpace =
            IsInsideDeadSpace(
                screenPosition
            );

        if (
            pointerStartedOverUI ||
            pointerStartedInDeadSpace)
        {
            hasAimPoint =
                false;

            return;
        }

        UpdateAim(
            screenPosition
        );
    }


    private void HandlePointerDrag(
        Vector2 screenPosition)
    {
        if (
            pointerStartedOverUI ||
            pointerStartedInDeadSpace ||
            IsInsideDeadSpace(screenPosition))
        {
            return;
        }

        UpdateAim(
            screenPosition
        );
    }


    private void HandlePointerUp(
        Vector2 screenPosition)
    {
        isPointerHeld =
            false;

        bool blocked =
            pointerStartedOverUI ||
            pointerStartedInDeadSpace ||
            IsInsideDeadSpace(
                screenPosition
            );

        pointerStartedOverUI =
            false;

        pointerStartedInDeadSpace =
            false;

        if (blocked)
        {
            hasAimPoint =
                false;

            return;
        }

        /*
         * Release position se exact aim
         * dobara calculate.
         */
        if (UpdateAim(screenPosition))
        {
            Shoot();
        }
    }


    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        return
            EventSystem.current
                .IsPointerOverGameObject();
    }


    private bool IsInsideDeadSpace(
        Vector2 screenPosition)
    {
        return
            screenPosition.y <=
            bottomDeadSpaceHeight;
    }


    private bool UpdateAim(
        Vector2 screenPosition)
    {
        if (!ValidateReferences())
        {
            hasAimPoint =
                false;

            return false;
        }

        Ray screenRay =
            aimCamera.ScreenPointToRay(
                screenPosition
            );

        RaycastHit chosenHit;

        /*
         * First preference:
         * tower target.
         */
        if (!TryGetClosestValidHit(
                screenRay,
                towerTargetMask,
                true,
                out chosenHit))
        {
            /*
             * Tower hit na ho to
             * environment fallback.
             */
            if (!TryGetClosestValidHit(
                    screenRay,
                    fallbackAimMask,
                    false,
                    out chosenHit))
            {
                currentAimPoint =
                    GetFallbackAimPoint(
                        screenRay
                    );
            }
            else
            {
                currentAimPoint =
                    chosenHit.point;
            }
        }
        else
        {
            currentAimPoint =
                chosenHit.point;
        }


        if (
            preventBackwardAim &&
            IsPointBehindCannon(
                currentAimPoint
            ))
        {
            hasAimPoint =
                false;

            if (showDebugLogs)
            {
                Debug.Log(
                    "Rear-side click ignored.",
                    this
                );
            }

            return false;
        }


        RotateCannonTowards(
            currentAimPoint
        );

        Physics.SyncTransforms();


        Vector3 finalDirection =
            currentAimPoint -
            muzzle.position;

        if (
            finalDirection.sqrMagnitude <
            0.0001f)
        {
            hasAimPoint =
                false;

            return false;
        }

        hasAimPoint =
            true;


        if (showDebugLines)
        {
            Debug.DrawLine(
                muzzle.position,
                currentAimPoint,
                Color.red,
                0.25f
            );
        }


        if (showDebugLogs)
        {
            string hitName =
                chosenHit.collider != null
                    ? chosenHit.collider.name
                    : "Fallback point";

            Debug.Log(
                $"Aim target: {hitName} | " +
                $"Point: {currentAimPoint}",
                this
            );
        }

        return true;
    }


    private bool TryGetClosestValidHit(
        Ray ray,
        LayerMask mask,
        bool requireTowerTarget,
        out RaycastHit closestHit)
    {
        closestHit =
            default;

        if (mask.value == 0)
        {
            return false;
        }


        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                maxAimDistance,
                mask,
                QueryTriggerInteraction.Ignore
            );


        bool found =
            false;

        float closestDistance =
            float.MaxValue;


        foreach (RaycastHit hit in hits)
        {
            if (
                hit.collider == null ||
                IsCannonCollider(
                    hit.collider
                ))
            {
                continue;
            }


            if (
                requireTowerTarget &&
                !IsTowerTarget(
                    hit.collider
                ))
            {
                continue;
            }


            if (
                hit.distance >=
                closestDistance)
            {
                continue;
            }


            closestDistance =
                hit.distance;

            closestHit =
                hit;

            found =
                true;
        }

        return found;
    }


    private bool IsTowerTarget(
        Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return false;
        }


        /*
         * New generic tower system mein
         * PhysicsTowerObject check.
         */
        if (hitCollider.GetComponentInParent<
                PhysicsTowerObject>() != null)
        {
            return true;
        }


        int layerBit =
            1 <<
            hitCollider.gameObject.layer;


        return
            (towerTargetMask.value &
             layerBit) != 0;
    }


    private bool IsCannonCollider(
        Collider candidate)
    {
        if (
            candidate == null ||
            cannonRoot == null)
        {
            return false;
        }


        Transform candidateTransform =
            candidate.transform;


        return
            candidateTransform ==
                cannonRoot ||
            candidateTransform.IsChildOf(
                cannonRoot
            );
    }


    private Vector3 GetFallbackAimPoint(
        Ray screenRay)
    {
        if (
            fallbackAimPlaneReference !=
            null)
        {
            Plane aimPlane =
                new Plane(
                    -aimCamera.transform.forward,
                    fallbackAimPlaneReference.position
                );


            if (
                aimPlane.Raycast(
                    screenRay,
                    out float enterDistance) &&
                enterDistance > 0f)
            {
                return
                    screenRay.GetPoint(
                        enterDistance
                    );
            }
        }


        return
            screenRay.GetPoint(
                fallbackAimDistance
            );
    }


    private bool IsPointBehindCannon(
        Vector3 targetPoint)
    {
        Transform forwardSpace =
            yawPivot.parent != null
                ? yawPivot.parent
                : transform;


        Vector3 flatDirection =
            targetPoint -
            yawPivot.position;

        flatDirection.y =
            0f;


        Vector3 flatForward =
            forwardSpace.forward;

        flatForward.y =
            0f;


        if (
            flatDirection.sqrMagnitude <
                0.0001f ||
            flatForward.sqrMagnitude <
                0.0001f)
        {
            return false;
        }


        return
            Vector3.Dot(
                flatForward.normalized,
                flatDirection.normalized
            ) <= 0f;
    }


    private void RotateCannonTowards(
        Vector3 targetPoint)
    {
        if (
            yawPivot == null ||
            pitchPivot == null)
        {
            return;
        }

        RotateYawTowards(
            targetPoint
        );

        RotatePitchTowards(
            targetPoint
        );
    }


    private void RotateYawTowards(
        Vector3 targetPoint)
    {
        Vector3 worldDirection =
            targetPoint -
            yawPivot.position;

        worldDirection.y =
            0f;


        if (
            worldDirection.sqrMagnitude <
            0.001f)
        {
            return;
        }


        Vector3 localDirection;


        if (yawPivot.parent != null)
        {
            localDirection =
                yawPivot.parent
                    .InverseTransformDirection(
                        worldDirection
                    );
        }
        else
        {
            localDirection =
                worldDirection;
        }


        float yawAngle =
            Mathf.Atan2(
                localDirection.x,
                localDirection.z
            ) *
            Mathf.Rad2Deg;


        yawPivot.localRotation =
            Quaternion.Euler(
                0f,
                yawAngle,
                0f
            );
    }


    private void RotatePitchTowards(
        Vector3 targetPoint)
    {
        Vector3 worldDirection =
            targetPoint -
            pitchPivot.position;


        Vector3 localDirection =
            yawPivot
                .InverseTransformDirection(
                    worldDirection
                );


        float horizontalDistance =
            new Vector2(
                localDirection.x,
                localDirection.z
            ).magnitude;


        float pitchAngle =
            Mathf.Atan2(
                localDirection.y,
                horizontalDistance
            ) *
            Mathf.Rad2Deg;


        pitchAngle =
            Mathf.Clamp(
                pitchAngle,
                minimumPitch,
                maximumPitch
            );


        pitchPivot.localRotation =
            Quaternion.Euler(
                -pitchAngle,
                0f,
                0f
            );
    }


    public void Shoot()
    {
        EnsureBallLimitInitialized();

        if (
            isCannonEntranceAnimating ||
            (!infiniteBallsEnabled &&
             remainingBalls <= 0) ||
            (levelRuntimeController != null &&
             !levelRuntimeController.IsLevelGenerated) ||
            !hasAimPoint ||
            Time.time <
            nextAllowedShotTime)
        {
            return;
        }


        if (
            cannonBallPrefab == null ||
            muzzle == null)
        {
            return;
        }


        Physics.SyncTransforms();


        Vector3 spawnOrigin =
            muzzle.position;


        float effectiveLaunchSpeed =
            launchSpeed *
            runtimeLaunchForceMultiplier;


        bool useGravityForShot =
            !exactStraightShot;


        Vector3 shotVelocity;


        if (useGravityForShot)
        {
            bool solved =
                TryCalculateBallisticVelocity(
                    spawnOrigin,
                    currentAimPoint,
                    effectiveLaunchSpeed,
                    out shotVelocity
                );


            if (!solved)
            {
                if (
                    !fallbackToStraightIfUnreachable)
                {
                    if (showDebugLogs)
                    {
                        Debug.LogWarning(
                            "Target current Launch Speed " +
                            "se reach nahi ho sakta.",
                            this
                        );
                    }

                    return;
                }


                useGravityForShot =
                    false;


                Vector3 straightDirection =
                    currentAimPoint -
                    spawnOrigin;


                if (
                    straightDirection.sqrMagnitude <
                    0.0001f)
                {
                    return;
                }


                shotVelocity =
                    straightDirection.normalized *
                    effectiveLaunchSpeed;


                if (showDebugLogs)
                {
                    Debug.LogWarning(
                        "Target unreachable. " +
                        "Straight fallback shot.",
                        this
                    );
                }
            }
        }
        else
        {
            Vector3 straightDirection =
                currentAimPoint -
                spawnOrigin;


            if (
                straightDirection.sqrMagnitude <
                0.0001f)
            {
                return;
            }


            shotVelocity =
                straightDirection.normalized *
                launchSpeed;
        }


        Vector3 shotDirection =
            shotVelocity.sqrMagnitude >
            0.0001f
                ? shotVelocity.normalized
                : (
                    currentAimPoint -
                    spawnOrigin
                ).normalized;


        if (
            preventBackwardAim &&
            IsDirectionBehindCannon(
                shotDirection
            ))
        {
            return;
        }


        nextAllowedShotTime =
            Time.time +
            minimumShotInterval;


        Vector3 spawnPosition =
            spawnOrigin +
            shotDirection *
            spawnForwardOffset;


        Quaternion spawnRotation =
            Quaternion.LookRotation(
                shotDirection,
                GetSafeUpDirection(
                    shotDirection
                )
            );


        GameObject cannonBall =
            Instantiate(
                cannonBallPrefab,
                spawnPosition,
                spawnRotation
            );


        if (!Mathf.Approximately(
                runtimeBallSizeMultiplier,
                1f))
        {
            cannonBall.transform.localScale *=
                runtimeBallSizeMultiplier;
        }


        Rigidbody ballRigidbody =
            cannonBall
                .GetComponent<Rigidbody>();


        if (ballRigidbody == null)
        {
            Debug.LogError(
                "Cannonball prefab par Rigidbody missing hai.",
                cannonBall
            );

            Destroy(
                cannonBall
            );

            return;
        }

        activeCannonBalls.Add(cannonBall);

        LowerGroundDisappearEffect groundDisappearEffect =
            cannonBall.GetComponent<LowerGroundDisappearEffect>();

        if (groundDisappearEffect == null)
        {
            groundDisappearEffect =
                cannonBall.AddComponent<LowerGroundDisappearEffect>();
        }

        groundDisappearEffect.SetDestroyOnComplete(true);
        groundDisappearEffect.SetAcceptUnmarkedStaticGround(true);
        groundDisappearEffect.ResetEffect();


        ballRigidbody.isKinematic =
            false;

        ballRigidbody.useGravity =
            useGravityForShot;

        ballRigidbody.angularVelocity =
            Vector3.zero;

        ballRigidbody.collisionDetectionMode =
            CollisionDetectionMode
                .ContinuousDynamic;


        SetBodyVelocity(
            ballRigidbody,
            Vector3.zero
        );


        IgnoreBallCollisionWithCannon(
            cannonBall
        );


        SetBodyVelocity(
            ballRigidbody,
            shotVelocity
        );

        ConsumeBall();


        if (showDebugLines)
        {
            Debug.DrawLine(
                spawnPosition,
                currentAimPoint,
                Color.green,
                2f
            );
        }


        groundDisappearEffect.ScheduleSmoothFallback(
            cannonBallLifetime
        );

        if (!infiniteBallsEnabled &&
            remainingBalls <= 0)
        {
            StartOutOfMovesCheck();
        }
    }


    private void StartOutOfMovesCheck()
    {
        if (outOfMovesRoutine != null)
        {
            StopCoroutine(outOfMovesRoutine);
        }

        outOfMovesRoutine =
            StartCoroutine(
                WaitForBallsToResolveThenShowOutOfMoves()
            );
    }


    private IEnumerator WaitForBallsToResolveThenShowOutOfMoves()
    {
        /*
         * Ball count zero hona final result nahi hai. Last fired ball aur
         * pehle se active balls ko hit, bounce, shrink aur destroy hone do.
         * Har frame level complete check hoti rahegi taake Win popup ko
         * hamesha Out Of Moves par priority mile.
         */
        while (HasActiveCannonBalls())
        {
            if (ShouldCancelOutOfMovesCheck())
            {
                outOfMovesRoutine = null;
                yield break;
            }

            yield return null;
        }

        /*
         * Ball destroy hone ke baad girte huay tower blocks ko ek short
         * settle window milti hai. Ye realtime par chalta hai taake kisi
         * accidental Time.timeScale change se result check freeze na ho.
         */
        float settleElapsed = 0f;

        while (settleElapsed < outOfMovesResultDelay)
        {
            if (ShouldCancelOutOfMovesCheck())
            {
                outOfMovesRoutine = null;
                yield break;
            }

            settleElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        outOfMovesRoutine = null;

        if (ShouldCancelOutOfMovesCheck())
        {
            yield break;
        }

        if (showDebugLogs)
        {
            Debug.Log(
                "Balls finished. OutOfMoves event invoke.",
                this
            );
        }

        OutOfMoves?.Invoke();
    }


    private bool HasActiveCannonBalls()
    {
        for (int i = activeCannonBalls.Count - 1;
             i >= 0;
             i--)
        {
            if (activeCannonBalls[i] == null)
            {
                activeCannonBalls.RemoveAt(i);
            }
        }

        return activeCannonBalls.Count > 0;
    }


    private bool ShouldCancelOutOfMovesCheck()
    {
        if (infiniteBallsEnabled ||
            remainingBalls > 0)
        {
            return true;
        }

        if (levelRuntimeController == null)
        {
            return false;
        }

        return
            !levelRuntimeController.IsLevelGenerated ||
            levelRuntimeController.RemainingTargets <= 0;
    }


    private bool TryCalculateBallisticVelocity(
        Vector3 startPosition,
        Vector3 targetPosition,
        float speed,
        out Vector3 launchVelocity)
    {
        launchVelocity =
            Vector3.zero;


        if (speed <= 0.01f)
        {
            return false;
        }


        float gravity =
            Mathf.Abs(
                Physics.gravity.y
            );


        if (gravity <= 0.001f)
        {
            return false;
        }


        Vector3 toTarget =
            targetPosition -
            startPosition;


        Vector3 horizontalToTarget =
            new Vector3(
                toTarget.x,
                0f,
                toTarget.z
            );


        float horizontalDistance =
            horizontalToTarget.magnitude;


        float verticalDistance =
            toTarget.y;


        if (
            horizontalDistance <
            0.0001f)
        {
            launchVelocity =
                new Vector3(
                    0f,
                    Mathf.Sign(
                        verticalDistance == 0f
                            ? 1f
                            : verticalDistance
                    ) * speed,
                    0f
                );

            return true;
        }


        float speedSquared =
            speed *
            speed;


        float discriminant =
            (
                speedSquared *
                speedSquared
            )
            -
            gravity *
            (
                gravity *
                horizontalDistance *
                horizontalDistance
                +
                2f *
                verticalDistance *
                speedSquared
            );


        if (discriminant < 0f)
        {
            return false;
        }


        float sqrtDiscriminant =
            Mathf.Sqrt(
                discriminant
            );


        float lowArcAngle =
            Mathf.Atan2(
                speedSquared -
                sqrtDiscriminant,

                gravity *
                horizontalDistance
            );


        float highArcAngle =
            Mathf.Atan2(
                speedSquared +
                sqrtDiscriminant,

                gravity *
                horizontalDistance
            );


        float chosenAngle =
            preferLowArcTrajectory
                ? lowArcAngle
                : highArcAngle;


        Vector3 horizontalDirection =
            horizontalToTarget.normalized;


        float horizontalSpeed =
            speed *
            Mathf.Cos(
                chosenAngle
            );


        float verticalSpeed =
            speed *
            Mathf.Sin(
                chosenAngle
            );


        launchVelocity =
            horizontalDirection *
            horizontalSpeed
            +
            Vector3.up *
            verticalSpeed;


        return true;
    }


    private bool IsDirectionBehindCannon(
        Vector3 direction)
    {
        Transform forwardSpace =
            yawPivot.parent != null
                ? yawPivot.parent
                : transform;


        Vector3 flatDirection =
            direction;

        flatDirection.y =
            0f;


        Vector3 flatForward =
            forwardSpace.forward;

        flatForward.y =
            0f;


        if (
            flatDirection.sqrMagnitude <
                0.0001f ||
            flatForward.sqrMagnitude <
                0.0001f)
        {
            return false;
        }


        return
            Vector3.Dot(
                flatForward.normalized,
                flatDirection.normalized
            ) <= 0f;
    }


    private Vector3 GetSafeUpDirection(
        Vector3 shotDirection)
    {
        Vector3 upDirection =
            muzzle != null
                ? muzzle.up
                : Vector3.up;


        if (
            Mathf.Abs(
                Vector3.Dot(
                    shotDirection,
                    upDirection.normalized
                )
            ) > 0.999f)
        {
            upDirection =
                Vector3.up;
        }


        return upDirection;
    }


    private void IgnoreBallCollisionWithCannon(
        GameObject cannonBall)
    {
        if (cannonBall == null)
        {
            return;
        }


        if (
            cannonColliders == null ||
            cannonColliders.Length == 0)
        {
            CacheCannonColliders();
        }


        Collider[] ballColliders =
            cannonBall
                .GetComponentsInChildren<
                    Collider>(true);


        foreach (
            Collider ballCollider
            in ballColliders)
        {
            if (ballCollider == null)
            {
                continue;
            }


            foreach (
                Collider cannonCollider
                in cannonColliders)
            {
                if (
                    cannonCollider == null)
                {
                    continue;
                }


                Physics.IgnoreCollision(
                    ballCollider,
                    cannonCollider,
                    true
                );
            }
        }
    }


    private bool ValidateReferences()
    {
        if (aimCamera == null)
        {
            Debug.LogError(
                "Aim Camera missing hai.",
                this
            );

            return false;
        }


        if (
            yawPivot == null ||
            pitchPivot == null ||
            muzzle == null)
        {
            Debug.LogError(
                "Yaw Pivot, Pitch Pivot ya Muzzle missing hai.",
                this
            );

            return false;
        }


        return true;
    }


    private static void SetBodyVelocity(
        Rigidbody body,
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
    private void OnDrawGizmosSelected()
    {
        if (
            !showDebugLines ||
            !hasAimPoint ||
            muzzle == null)
        {
            return;
        }


        Gizmos.DrawLine(
            muzzle.position,
            currentAimPoint
        );


        Gizmos.DrawSphere(
            currentAimPoint,
            0.08f
        );
    }
#endif
}












