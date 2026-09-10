// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;
// using UnityEngine.Serialization;

// public class BallDropController : MonoBehaviour
// {
//     [Header("Control Object")]
//     public Transform controlObject;


//     [Header("Ball")]
//     public GameObject ballPrefab;
//     public Transform ballSpawnPoint;

//     [Tooltip("Height above the clicked surface where the ball starts falling.")]
//     [Min(0.1f)]
//     public float clickDropHeight = 0.5f;

//     [Tooltip("Seconds after release before the spawned ball is destroyed. Zero keeps it forever.")]
//     [Min(0f)]
//     public float ballLifetime = 4f;

// private Vector3 releasePosition;
//     [Header("Precise Left / Right Movement")]
//     public float minXPosition = -5f;
//     public float maxXPosition = 5f;


//     [Header("Power Meter")]
//     public Slider powerSlider;

//     public float powerChargeSpeed = 0.5f;

//     public float minimumPower = 0.2f;
//     public float maximumPower = 1f;


//     [Header("Ball Fall")]
//     public float minimumFallSpeed = 2f;
//     public float maximumFallSpeed = 10f;

//     [FormerlySerializedAs("limitRollingSpeedFromSlider")]
//     [Tooltip("Use slider speed as the ball's minimum/base speed. Gravity can still accelerate it on slopes.")]
//     public bool maintainBaseSpeedFromSlider = true;

//     [Tooltip("Extra downward acceleration prevents the rolling ball from floating above slopes.")]
//     [Min(0f)]
//     public float extraDownwardAcceleration = 12f;


//     [Header("Input Zone")]
//     public RectTransform inputZone;


//     [Header("Camera")]
//     public Camera gameplayCamera;
//     private float cameraFollowOffsetX;
//     private bool cameraFollowReady;
//     private Vector3 dragCameraOrigin;





//     private void LateUpdate()
//     {
//         // if (gameplayCamera == null || controlObject == null) return;
//         // if (!cameraFollowReady)
//         // {
//         //     cameraFollowOffsetX = gameplayCamera.transform.position.x - controlObject.position.x;
//         //     cameraFollowReady = true;
//         // }
//         // var position = gameplayCamera.transform.position;
//         // position.x = controlObject.position.x + cameraFollowOffsetX;
//         // gameplayCamera.transform.position = position;
//     }


//     [Header("Power Charge")]
//     public float stopDelay = 0.15f;
//     public float movementThreshold = 0.5f;


//     [Header("Aim Dotted Trail")]
//     public Transform aimOrigin;

//     public Transform releaseDirection;

//     public float minimumAimLength = 2f;
//     public float maximumAimLength = 8f;

//     public float dotSpacing = 0.35f;
//     public float dotSize = 0.08f;


//     [Header("Aim Dot Appearance")]
//     public Material aimDotMaterial;


//     [Header("Aim Physics")]
//     public LayerMask aimCollisionMask = ~0;

//     public int maximumAimReflections = 3;

//     public float aimBallRadius = 0.1f;

//     public float reflectionSurfaceOffset = 0.02f;


//     [Header("Slope Detection")]
//     public float slopeDetectionDistance = 20f;

//     public float slopeSurfaceOffset = 0.03f;



//     private bool controlling;
//     private readonly List<GameObject> releasedBalls = new List<GameObject>();

//     public void ClearReleasedBalls()
//     {
//         foreach (var ball in releasedBalls)
//         {
//             if (ball == null) continue;
//             ball.SetActive(false);
//             Destroy(ball);
//         }
//         releasedBalls.Clear();
//         controlling = false;
//         charging = false;
//         ResetPower();
//     }
//     private bool charging;

//     private Vector3 targetPosition;

//     private float xOffset;

//     private float stopTimer;

//     private Plane movementPlane;


//     private List<GameObject> aimDots =
//         new List<GameObject>();


//     private List<Vector3> aimPathPoints =
//         new List<Vector3>();


//     private readonly RaycastHit[] aimHitBuffer =
//         new RaycastHit[64];



//     void Start()
//     {
//         if (gameplayCamera == null)
//         {
//             gameplayCamera = Camera.main;
//         }


//         if (controlObject != null)
//         {
//             targetPosition =
//                 controlObject.position;


//             movementPlane =
//                 new Plane(
//                     Vector3.up,
//                     controlObject.position
//                 );
//         }


//         if (powerSlider != null)
//         {
//             powerSlider.minValue = 0f;
//             powerSlider.maxValue = 1f;
//             powerSlider.value = 0f;
//         }


//         HideAimGuide();
//     }



//     void Update()
//     {
//         HandleInput();

//         SmoothControlObject();

//         HandlePowerMeter();

//         UpdateAimGuide();
//     }



//     // =========================================================
//     // INPUT
//     // =========================================================

//     // void HandleInput()
//     // {
//     //     // MOUSE DOWN
//     //     if (Input.GetMouseButtonDown(0))
//     //     {
//     //         Vector2 mousePosition =
//     //             Input.mousePosition;


//     //         if (IsInsideZone(mousePosition))
//     //         {
//     //             StartControl(mousePosition);
//     //         }
//     //     }


//     //     // MOUSE DRAG
//     //     if (Input.GetMouseButton(0) && controlling)
//     //     {
//     //         UpdateControlPosition(
//     //             Input.mousePosition
//     //         );
//     //     }


//     //     // MOUSE RELEASE
//     //     if (Input.GetMouseButtonUp(0))
//     //     {
//     //         if (controlling)
//     //         {
//     //             ReleaseBall();
//     //         }
//     //     }


//     //     // TOUCH
//     //     if (Input.touchCount > 0)
//     //     {
//     //         Touch touch =
//     //             Input.GetTouch(0);


//     //         if (touch.phase ==
//     //             TouchPhase.Began)
//     //         {
//     //             if (IsInsideZone(
//     //                 touch.position))
//     //             {
//     //                 StartControl(
//     //                     touch.position
//     //                 );
//     //             }
//     //         }


//     //         if (touch.phase ==
//     //             TouchPhase.Moved &&
//     //             controlling)
//     //         {
//     //             UpdateControlPosition(
//     //                 touch.position
//     //             );
//     //         }


//     //         if (touch.phase ==
//     //                 TouchPhase.Ended ||
//     //             touch.phase ==
//     //                 TouchPhase.Canceled)
//     //         {
//     //             if (controlling)
//     //             {
//     //                 ReleaseBall();
//     //             }
//     //         }
//     //     }
//     // }


// void HandleInput()
// {
//     if (Input.touchCount == 0 && Input.GetMouseButtonDown(0))
//     {
//         Vector2 mousePosition = Input.mousePosition;

//         if (IsInsideZone(mousePosition) && StartControl(mousePosition))
//         {
//             ReleaseBall();
//         }
//     }


//     if (Input.touchCount > 0)
//     {
//         Touch touch = Input.GetTouch(0);

//         if (touch.phase == TouchPhase.Began)
//         {
//             if (IsInsideZone(touch.position) && StartControl(touch.position))
//             {
//                 ReleaseBall();
//             }
//         }
//     }
// }



//     // =========================================================
//     // START CONTROL
//     // =========================================================

//     // void StartControl(
//     //     Vector2 screenPosition)
//     // {
//     //     if (controlObject == null)
//     //         return;


//     //     controlling = true;
//     //     if (gameplayCamera != null) dragCameraOrigin = gameplayCamera.transform.position;

//     //     charging = false;

//     //     stopTimer = 0f;


//     //     movementPlane =
//     //         new Plane(
//     //             Vector3.up,
//     //             controlObject.position
//     //         );


//     //     Vector3 worldPosition;


//     //     if (TryGetWorldPosition(
//     //         screenPosition,
//     //         out worldPosition))
//     //     {
//     //         xOffset =
//     //             controlObject.position.x -
//     //             worldPosition.x;
//     //     }


//     //     targetPosition =
//     //         controlObject.position;


//     //     ResetPower();

//     //     ShowAimGuide();
//     // }


// bool StartControl(Vector2 screenPosition)
// {
//     if (controlObject == null || gameplayCamera == null)
//         return false;


//     controlling = true;


//     if (gameplayCamera != null)
//         dragCameraOrigin = gameplayCamera.transform.position;


//     charging = false;
//     stopTimer = 0f;


//     // Project onto the visible play area so both screen X and Y affect the drop.
//     // A horizontal plane fixes world height and makes clicks appear on one line.
//     movementPlane = new Plane(
//         gameplayCamera.transform.forward,
//         ballSpawnPoint != null ? ballSpawnPoint.position : controlObject.position
//     );


//     Vector3 worldPosition;


//     if (TryGetDropPosition(
//         screenPosition,
//         out worldPosition))
//     {
//         // Spawn at the pointer, then let the existing downward velocity/gravity act.
//         releasePosition = worldPosition;


//         // Keep old movement calculation
//         xOffset =
//             controlObject.position.x -
//             worldPosition.x;
//     }


//     else
//     {
//         controlling = false;
//         return false;
//     }

//     targetPosition =
//         controlObject.position;


//     ResetPower();

//     ShowAimGuide();
//     return true;
// }
   
   
   
   
   
   
   
   
   
   
   
//     // =========================================================
//     // PRECISE MOVEMENT
//     // =========================================================

//     void UpdateControlPosition(
//         Vector2 screenPosition)
//     {
//         if (controlObject == null)
//             return;


//         Vector3 worldPosition;


//         if (!TryGetWorldPosition(
//             screenPosition,
//             out worldPosition))
//         {
//             return;
//         }


//         float targetX =
//             worldPosition.x + xOffset;


//         targetX =
//             Mathf.Clamp(
//                 targetX,
//                 minXPosition,
//                 maxXPosition
//             );


//         float oldX =
//             targetPosition.x;


//         targetPosition =
//             controlObject.position;


//         targetPosition.x =
//             targetX;


//         float movement =
//             Mathf.Abs(
//                 targetX - oldX
//             );


//         if (movement > 0.001f)
//         {
//             stopTimer = 0f;

//             charging = false;

//             ResetPower();
//         }
//         else
//         {
//             stopTimer +=
//                 Time.deltaTime;
//         }
//     }



//     // =========================================================
//     // SMOOTH CONTROL
//     // =========================================================

//     void SmoothControlObject()
//     {
//         if (!controlling)
//             return;


//         if (controlObject == null)
//             return;


//         controlObject.position =
//             Vector3.Lerp(
//                 controlObject.position,
//                 targetPosition,
//                 25f *
//                 Time.deltaTime
//             );
//     }



//     // =========================================================
//     // POWER
//     // =========================================================

//     void HandlePowerMeter()
//     {
//         if (!controlling)
//             return;


//         if (stopTimer >= stopDelay)
//         {
//             charging = true;
//         }


//         if (charging &&
//             powerSlider != null)
//         {
//             powerSlider.value +=
//                 powerChargeSpeed *
//                 Time.deltaTime;


//             powerSlider.value =
//                 Mathf.Clamp(
//                     powerSlider.value,
//                     0f,
//                     maximumPower
//                 );
//         }
//     }



//     // =========================================================
//     // AIM GUIDE
//     // =========================================================

//     void UpdateAimGuide()
//     {
//         if (!controlling)
//         {
//             HideAimGuide();
//             return;
//         }


//         if (aimOrigin == null)
//             return;


//         float power = 0f;


//         if (powerSlider != null)
//         {
//             power =
//                 powerSlider.value;
//         }


//         float aimLength =
//             Mathf.Lerp(
//                 minimumAimLength,
//                 maximumAimLength,
//                 power
//             );


//         /*
//          * IMPORTANT:
//          *
//          * Ball does NOT use releaseDirection
//          * for its initial movement.
//          *
//          * Ball first FALLS DOWN.
//          *
//          * After hitting the slope:
//          *
//          * Gravity is projected onto the slope.
//          *
//          * This gives the actual downhill
//          * direction the ball will naturally take.
//          */


//         Vector3 origin =
//             aimOrigin.position;


//         Vector3 fallDirection =
//             Vector3.down;


//         BuildPhysicsAimPath(
//             origin,
//             fallDirection,
//             aimLength
//         );


//         int dotCount =
//             Mathf.FloorToInt(
//                 aimLength /
//                 Mathf.Max(
//                     0.01f,
//                     dotSpacing
//                 )
//             );


//         CreateDots(dotCount);


//         int visibleDots =
//             PlaceDotsAlongAimPath(
//                 dotCount,
//                 Mathf.Max(
//                     0.01f,
//                     dotSpacing
//                 )
//             );


//         for (int i = visibleDots;
//              i < aimDots.Count;
//              i++)
//         {
//             aimDots[i].SetActive(false);
//         }
//     }



//     // =========================================================
//     // BUILD ACTUAL PHYSICS PATH
//     // =========================================================

//     void BuildPhysicsAimPath(
//         Vector3 origin,
//         Vector3 direction,
//         float totalLength)
//     {
//         aimPathPoints.Clear();

//         aimPathPoints.Add(origin);


//         float remainingLength =
//             Mathf.Max(
//                 0f,
//                 totalLength
//             );


//         int reflectionCount = 0;


//         Vector3 currentOrigin =
//             origin;


//         Vector3 currentDirection =
//             direction.normalized;


//         bool hasHitSlope = false;



//         while (remainingLength > 0.001f)
//         {
//             RaycastHit hit;


//             bool hitSomething =
//                 TryGetClosestAimHit(
//                     currentOrigin,
//                     currentDirection,
//                     Mathf.Max(
//                         0.001f,
//                         aimBallRadius
//                     ),
//                     Mathf.Min(
//                         slopeDetectionDistance,
//                         remainingLength
//                     ),
//                     out hit
//                 );


//             // =================================================
//             // NOTHING HIT
//             // =================================================

//             if (!hitSomething)
//             {
//                 aimPathPoints.Add(
//                     currentOrigin +
//                     currentDirection *
//                     remainingLength
//                 );

//                 break;
//             }



//             // =================================================
//             // HIT POINT
//             // =================================================

//             float distance =
//                 Mathf.Max(
//                     0f,
//                     hit.distance
//                 );


//             Vector3 hitPoint =
//                 currentOrigin +
//                 currentDirection *
//                 distance;


//             aimPathPoints.Add(
//                 hitPoint
//             );


//             remainingLength -=
//                 distance;



//             // =================================================
//             // FIRST SURFACE = SLOPE
//             // =================================================

//             if (!hasHitSlope)
//             {
//                 hasHitSlope = true;


//                 /*
//                  * Gravity projected onto the
//                  * surface gives the direction
//                  * the ball naturally rolls/falls.
//                  */

//                 // Vector3 downhillDirection =
//                 //     Vector3.ProjectOnPlane(
//                 //         Physics.gravity,
//                 //         hit.normal
//                 //     );


// Vector3 downhillDirection = Vector3.down;



//                 if (downhillDirection.sqrMagnitude >
//                     0.0001f)
//                 {
//                     downhillDirection.Normalize();


//                     currentDirection =
//                         downhillDirection;


//                     currentOrigin =
//                         hitPoint +
//                         currentDirection *
//                         slopeSurfaceOffset;


//                     remainingLength -=
//                         slopeSurfaceOffset;


//                     continue;
//                 }


//                 break;
//             }



//             // =================================================
//             // AFTER SLOPE: PHYSICAL COLLISION
//             // =================================================

//             if (reflectionCount >=
//                 maximumAimReflections)
//             {
//                 break;
//             }


//             Vector3 reflectedDirection =
//                 Vector3.Reflect(
//                     currentDirection,
//                     hit.normal
//                 ).normalized;


//             if (reflectedDirection.sqrMagnitude <
//                 0.0001f)
//             {
//                 break;
//             }


//             currentDirection =
//                 reflectedDirection;


//             currentOrigin =
//                 hitPoint +
//                 currentDirection *
//                 reflectionSurfaceOffset;


//             remainingLength -=
//                 reflectionSurfaceOffset;


//             reflectionCount++;
//         }
//     }



//     // =========================================================
//     // SPHERE CAST
//     // =========================================================

//     bool TryGetClosestAimHit(
//         Vector3 origin,
//         Vector3 direction,
//         float radius,
//         float distance,
//         out RaycastHit closestHit)
//     {
//         int hitCount =
//             Physics.SphereCastNonAlloc(
//                 origin,
//                 radius,
//                 direction,
//                 aimHitBuffer,
//                 distance,
//                 aimCollisionMask,
//                 QueryTriggerInteraction.Ignore
//             );


//         closestHit = default;


//         float closestDistance =
//             float.PositiveInfinity;


//         for (int i = 0;
//              i < hitCount;
//              i++)
//         {
//             RaycastHit candidate =
//                 aimHitBuffer[i];


//             if (candidate.collider == null)
//                 continue;


//             if (ShouldIgnoreAimCollider(
//                 candidate.collider))
//             {
//                 continue;
//             }


//             if (candidate.distance >=
//                 closestDistance)
//             {
//                 continue;
//             }


//             closestHit =
//                 candidate;


//             closestDistance =
//                 candidate.distance;
//         }


//         return closestDistance <
//                float.PositiveInfinity;
//     }



//     // =========================================================
//     // IGNORE OWN OBJECTS
//     // =========================================================

//     bool ShouldIgnoreAimCollider(
//         Collider collider)
//     {
//         Transform t =
//             collider.transform;


//         if (controlObject != null)
//         {
//             if (t == controlObject ||
//                 t.IsChildOf(controlObject))
//             {
//                 return true;
//             }
//         }


//         if (transform == t ||
//             t.IsChildOf(transform))
//         {
//             return true;
//         }


//         return false;
//     }



//     // =========================================================
//     // PLACE DOTS
//     // =========================================================

//     int PlaceDotsAlongAimPath(
//         int maximumDotCount,
//         float spacing)
//     {
//         if (maximumDotCount <= 0)
//             return 0;


//         if (aimPathPoints.Count < 2)
//             return 0;


//         int dotIndex = 0;


//         float distanceToNextDot =
//             spacing;


//         for (int segmentIndex = 0;
//              segmentIndex <
//                  aimPathPoints.Count - 1 &&
//              dotIndex <
//                  maximumDotCount;
//              segmentIndex++)
//         {
//             Vector3 start =
//                 aimPathPoints[
//                     segmentIndex
//                 ];


//             Vector3 segment =
//                 aimPathPoints[
//                     segmentIndex + 1
//                 ] - start;


//             float segmentLength =
//                 segment.magnitude;


//             if (segmentLength <= 0.001f)
//                 continue;


//             Vector3 segmentDirection =
//                 segment /
//                 segmentLength;


//             while (
//                 distanceToNextDot <=
//                 segmentLength +
//                 0.0001f &&
//                 dotIndex <
//                 maximumDotCount)
//             {
//                 GameObject dot =
//                     aimDots[dotIndex];


//                 dot.transform.position =
//                     start +
//                     segmentDirection *
//                     distanceToNextDot;


//                 dot.SetActive(true);


//                 dotIndex++;


//                 distanceToNextDot +=
//                     spacing;
//             }


//             distanceToNextDot -=
//                 segmentLength;
//         }


//         return dotIndex;
//     }



//     // =========================================================
//     // CREATE DOTS
//     // =========================================================

//     void CreateDots(int count)
//     {
//         while (aimDots.Count < count)
//         {
//             GameObject dot =
//                 GameObject.CreatePrimitive(
//                     PrimitiveType.Sphere
//                 );


//             dot.name =
//                 "AimDot";


//             dot.transform.SetParent(
//                 transform
//             );


//             dot.transform.localScale =
//                 Vector3.one *
//                 dotSize;


//             Collider collider =
//                 dot.GetComponent<Collider>();


//             if (collider != null)
//             {
//                 Destroy(collider);
//             }


//             Renderer renderer =
//                 dot.GetComponent<Renderer>();


//             if (renderer != null)
//             {
//                 if (aimDotMaterial != null)
//                 {
//                     renderer.material =
//                         aimDotMaterial;
//                 }
//                 else
//                 {
//                     Material material =
//                         new Material(
//                             Shader.Find(
//                                 "Universal Render Pipeline/Lit"
//                             )
//                         );


//                     material.color =
//                         Color.white;


//                     renderer.material =
//                         material;
//                 }
//             }


//             dot.SetActive(false);


//             aimDots.Add(dot);
//         }
//     }



//     // =========================================================
//     // SHOW AIM
//     // =========================================================

//     void ShowAimGuide()
//     {
//         UpdateAimGuide();
//     }



//     // =========================================================
//     // HIDE AIM
//     // =========================================================

//     void HideAimGuide()
//     {
//         for (int i = 0;
//              i < aimDots.Count;
//              i++)
//         {
//             if (aimDots[i] != null)
//             {
//                 aimDots[i].SetActive(false);
//             }
//         }
//     }



//     // =========================================================
//     // RELEASE BALL
//     // =========================================================

//     void ReleaseBall()
//     {
//         controlling = false;

//         charging = false;


//         HideAimGuide();


//         if (ballPrefab == null)
//         {
//             Debug.LogWarning(
//                 "Ball Prefab is not assigned."
//             );

//             return;
//         }


//         float power =
//             minimumPower;


//         if (powerSlider != null)
//         {
//             power =
//                 powerSlider.value;
//         }


//         power =
//             Mathf.Clamp(
//                 power,
//                 minimumPower,
//                 maximumPower
//             );


//         // Vector3 spawnPosition;


//         // if (ballSpawnPoint != null)
//         // {
//         //     spawnPosition =
//         //         ballSpawnPoint.position;
//         // }
//         // else if (controlObject != null)
//         // {
//         //     spawnPosition =
//         //         controlObject.position;
//         // }
//         // else
//         // {
//         //     spawnPosition =
//         //         transform.position;
//         // }

// Vector3 spawnPosition = releasePosition;
//         GameObject ball =
//             Instantiate(
//                 ballPrefab,
//                 spawnPosition,
//                 Quaternion.identity
//             );


//         releasedBalls.RemoveAll(releasedBall => releasedBall == null);
//         releasedBalls.Add(ball);

//         if (ballLifetime > 0f)
//         {
//             Destroy(ball, ballLifetime);
//         }


//         Rigidbody ballRb =
//             ball.GetComponent<Rigidbody>();


//         if (ballRb == null)
//         {
//             Debug.LogWarning(
//                 "Ball Prefab needs Rigidbody."
//             );

//             ResetPower();

//             return;
//         }


//         ballRb.isKinematic = false;

//         ballRb.useGravity = true;


//         ballRb.linearVelocity =
//             Vector3.zero;


//         ballRb.angularVelocity =
//             Vector3.zero;


//         /*
//          * IMPORTANT:
//          *
//          * Ball only gets DOWNWARD velocity.
//          *
//          * No forward launch.
//          * No horizontal launch.
//          *
//          * After hitting the slope,
//          * Unity physics takes over.
//          */

//         float normalizedPower =
//             Mathf.InverseLerp(
//                 minimumPower,
//                 maximumPower,
//                 power
//             );


//         float fallSpeed =
//             Mathf.Lerp(
//                 minimumFallSpeed,
//                 maximumFallSpeed,
//                 normalizedPower
//             );


//         ballRb.linearVelocity =
//             Vector3.down *
//             fallSpeed;


//         BallSliderSpeed ballSpeed =
//             ball.GetComponent<BallSliderSpeed>();


//         if (maintainBaseSpeedFromSlider)
//         {
//             if (ballSpeed == null)
//             {
//                 ballSpeed =
//                     ball.AddComponent<BallSliderSpeed>();
//             }


//             ballSpeed.Configure(
//                 ballRb,
//                 fallSpeed,
//                 extraDownwardAcceleration
//             );
//         }
//         else if (ballSpeed != null)
//         {
//             ballSpeed.enabled = false;
//         }


//         ResetPower();
//     }



//     // =========================================================
//     // SCREEN TO WORLD
//     // =========================================================

//     bool TryGetDropPosition(Vector2 screenPosition, out Vector3 worldPosition)
//     {
//         Ray ray = gameplayCamera.ScreenPointToRay(screenPosition);
//         RaycastHit[] hits = Physics.RaycastAll(
//             ray, Mathf.Infinity, aimCollisionMask, QueryTriggerInteraction.Ignore);
//         float nearestDistance = float.PositiveInfinity;
//         worldPosition = Vector3.zero;

//         foreach (RaycastHit hit in hits)
//         {
//             // Existing balls must not move the next drop point off the slope.
//             bool isReleasedBall = false;
//             foreach (GameObject ball in releasedBalls)
//             {
//                 if (ball != null && hit.transform.IsChildOf(ball.transform))
//                 {
//                     isReleasedBall = true;
//                     break;
//                 }
//             }

//             if (isReleasedBall || hit.normal.y <= 0.01f || hit.distance >= nearestDistance)
//                 continue;

//             nearestDistance = hit.distance;
//             worldPosition = hit.point + Vector3.up * Mathf.Max(0.1f, clickDropHeight);
//         }

//         if (!float.IsPositiveInfinity(nearestDistance))
//             return true;

//         // Empty space in the input area still supports the existing free drop.
//         return TryGetWorldPosition(screenPosition, out worldPosition);
//     }

//     bool TryGetWorldPosition(
//         Vector2 screenPosition,
//         out Vector3 worldPosition)
//     {
//         worldPosition =
//             Vector3.zero;


//         if (gameplayCamera == null)
//             return false;


//         Ray ray =
//             gameplayCamera.ScreenPointToRay(
//                 screenPosition
//             );


//         float distance;
//         // Freeze the drag's projection origin while the view follows the ramp;
//         // otherwise camera movement feeds back into the pointer's world position.
//         if (controlling)
//             ray.origin += dragCameraOrigin - gameplayCamera.transform.position;


//         if (movementPlane.Raycast(
//             ray,
//             out distance))
//         {
//             worldPosition =
//                 ray.GetPoint(distance);


//             return true;
//         }


//         return false;
//     }



//     // =========================================================
//     // INPUT ZONE
//     // =========================================================

//     bool IsInsideZone(
//         Vector2 position)
//     {
//         if (inputZone == null)
//             return true;


//         return
//             RectTransformUtility
//             .RectangleContainsScreenPoint(
//                 inputZone,
//                 position,
//                 null
//             );
//     }



//     // =========================================================
//     // RESET POWER
//     // =========================================================

//     void ResetPower()
//     {
//         if (powerSlider != null)
//         {
//             powerSlider.value = 0f;
//         }
//     }
// }













using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class BallDropController : MonoBehaviour
{
    [Header("Control Object")]
    public Transform controlObject;


    [Header("Ball")]
    public GameObject ballPrefab;
    public Transform ballSpawnPoint;

    [Tooltip("Height above the clicked surface where the ball starts falling.")]
    [Min(0.1f)]
    public float clickDropHeight = 0.5f;

    [Tooltip("Seconds after release before the spawned ball is destroyed. Zero keeps it forever.")]
    [Min(0f)]
    public float ballLifetime = 4f;

private Vector3 releasePosition;
    [Header("Precise Left / Right Movement")]
    public float minXPosition = -5f;
    public float maxXPosition = 5f;


    [Header("Power Meter")]
    public Slider powerSlider;

    public float powerChargeSpeed = 0.5f;

    public float minimumPower = 0.2f;
    public float maximumPower = 1f;


    [Header("Ball Fall")]
    public float minimumFallSpeed = 2f;
    public float maximumFallSpeed = 10f;

    [Tooltip("Physics material applied to the spawned ball's collider (controls friction/bounciness for realistic slope sliding). Leave null to keep the prefab's own material.")]
    public PhysicsMaterial ballPhysicsMaterial;


    [Header("Input Zone")]
    public RectTransform inputZone;


    [Header("Camera")]
    public Camera gameplayCamera;
    private float cameraFollowOffsetX;
    private bool cameraFollowReady;
    private Vector3 dragCameraOrigin;





    private void LateUpdate()
    {
        // if (gameplayCamera == null || controlObject == null) return;
        // if (!cameraFollowReady)
        // {
        //     cameraFollowOffsetX = gameplayCamera.transform.position.x - controlObject.position.x;
        //     cameraFollowReady = true;
        // }
        // var position = gameplayCamera.transform.position;
        // position.x = controlObject.position.x + cameraFollowOffsetX;
        // gameplayCamera.transform.position = position;
    }


    [Header("Power Charge")]
    public float stopDelay = 0.15f;
    public float movementThreshold = 0.5f;


    [Header("Aim Dotted Trail")]
    public Transform aimOrigin;

    public Transform releaseDirection;

    public float minimumAimLength = 2f;
    public float maximumAimLength = 8f;

    public float dotSpacing = 0.35f;
    public float dotSize = 0.08f;


    [Header("Aim Dot Appearance")]
    public Material aimDotMaterial;


    [Header("Aim Physics")]
    public LayerMask aimCollisionMask = ~0;

    public int maximumAimReflections = 3;

    public float aimBallRadius = 0.1f;

    public float reflectionSurfaceOffset = 0.02f;


    [Header("Slope Detection")]
    public float slopeDetectionDistance = 20f;

    public float slopeSurfaceOffset = 0.03f;



    private bool controlling;
    private readonly List<GameObject> releasedBalls = new List<GameObject>();

    public void ClearReleasedBalls()
    {
        foreach (var ball in releasedBalls)
        {
            if (ball == null) continue;
            ball.SetActive(false);
            Destroy(ball);
        }
        releasedBalls.Clear();
        controlling = false;
        charging = false;
        ResetPower();
    }
    private bool charging;

    private Vector3 targetPosition;

    private float xOffset;

    private float stopTimer;

    private Plane movementPlane;


    private List<GameObject> aimDots =
        new List<GameObject>();


    private List<Vector3> aimPathPoints =
        new List<Vector3>();


    private readonly RaycastHit[] aimHitBuffer =
        new RaycastHit[64];



    void Start()
    {
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }


        if (controlObject != null)
        {
            targetPosition =
                controlObject.position;


            movementPlane =
                new Plane(
                    Vector3.up,
                    controlObject.position
                );
        }


        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0f;
        }


        HideAimGuide();
    }



    void Update()
    {
        HandleInput();

        SmoothControlObject();

        HandlePowerMeter();

        UpdateAimGuide();
    }



    // =========================================================
    // INPUT
    // =========================================================

void HandleInput()
{
    if (Input.touchCount == 0 && Input.GetMouseButtonDown(0))
    {
        Vector2 mousePosition = Input.mousePosition;

        if (IsInsideZone(mousePosition) && StartControl(mousePosition))
        {
            ReleaseBall();
        }
    }


    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            if (IsInsideZone(touch.position) && StartControl(touch.position))
            {
                ReleaseBall();
            }
        }
    }
}



    // =========================================================
    // START CONTROL
    // =========================================================

bool StartControl(Vector2 screenPosition)
{
    if (controlObject == null || gameplayCamera == null)
        return false;


    controlling = true;


    if (gameplayCamera != null)
        dragCameraOrigin = gameplayCamera.transform.position;


    charging = false;
    stopTimer = 0f;


    // Project onto the visible play area so both screen X and Y affect the drop.
    // A horizontal plane fixes world height and makes clicks appear on one line.
    movementPlane = new Plane(
        gameplayCamera.transform.forward,
        ballSpawnPoint != null ? ballSpawnPoint.position : controlObject.position
    );


    Vector3 worldPosition;


    if (TryGetDropPosition(
        screenPosition,
        out worldPosition))
    {
        // Spawn at the pointer, then let the existing downward velocity/gravity act.
        releasePosition = worldPosition;


        // Keep old movement calculation
        xOffset =
            controlObject.position.x -
            worldPosition.x;
    }


    else
    {
        controlling = false;
        return false;
    }

    targetPosition =
        controlObject.position;


    ResetPower();

    ShowAimGuide();
    return true;
}
   
   
   
   
   
   
   
   
   
   
   
    // =========================================================
    // PRECISE MOVEMENT
    // =========================================================

    void UpdateControlPosition(
        Vector2 screenPosition)
    {
        if (controlObject == null)
            return;


        Vector3 worldPosition;


        if (!TryGetWorldPosition(
            screenPosition,
            out worldPosition))
        {
            return;
        }


        float targetX =
            worldPosition.x + xOffset;


        targetX =
            Mathf.Clamp(
                targetX,
                minXPosition,
                maxXPosition
            );


        float oldX =
            targetPosition.x;


        targetPosition =
            controlObject.position;


        targetPosition.x =
            targetX;


        float movement =
            Mathf.Abs(
                targetX - oldX
            );


        if (movement > 0.001f)
        {
            stopTimer = 0f;

            charging = false;

            ResetPower();
        }
        else
        {
            stopTimer +=
                Time.deltaTime;
        }
    }



    // =========================================================
    // SMOOTH CONTROL
    // =========================================================

    void SmoothControlObject()
    {
        if (!controlling)
            return;


        if (controlObject == null)
            return;


        controlObject.position =
            Vector3.Lerp(
                controlObject.position,
                targetPosition,
                25f *
                Time.deltaTime
            );
    }



    // =========================================================
    // POWER
    // =========================================================

    void HandlePowerMeter()
    {
        if (!controlling)
            return;


        if (stopTimer >= stopDelay)
        {
            charging = true;
        }


        if (charging &&
            powerSlider != null)
        {
            powerSlider.value +=
                powerChargeSpeed *
                Time.deltaTime;


            powerSlider.value =
                Mathf.Clamp(
                    powerSlider.value,
                    0f,
                    maximumPower
                );
        }
    }



    // =========================================================
    // AIM GUIDE
    // =========================================================

    void UpdateAimGuide()
    {
        if (!controlling)
        {
            HideAimGuide();
            return;
        }


        if (aimOrigin == null)
            return;


        float power = 0f;


        if (powerSlider != null)
        {
            power =
                powerSlider.value;
        }


        float aimLength =
            Mathf.Lerp(
                minimumAimLength,
                maximumAimLength,
                power
            );


        Vector3 origin =
            aimOrigin.position;


        Vector3 fallDirection =
            Vector3.down;


        BuildPhysicsAimPath(
            origin,
            fallDirection,
            aimLength
        );


        int dotCount =
            Mathf.FloorToInt(
                aimLength /
                Mathf.Max(
                    0.01f,
                    dotSpacing
                )
            );


        CreateDots(dotCount);


        int visibleDots =
            PlaceDotsAlongAimPath(
                dotCount,
                Mathf.Max(
                    0.01f,
                    dotSpacing
                )
            );


        for (int i = visibleDots;
             i < aimDots.Count;
             i++)
        {
            aimDots[i].SetActive(false);
        }
    }



    // =========================================================
    // BUILD ACTUAL PHYSICS PATH
    // =========================================================

    void BuildPhysicsAimPath(
        Vector3 origin,
        Vector3 direction,
        float totalLength)
    {
        aimPathPoints.Clear();

        aimPathPoints.Add(origin);


        float remainingLength =
            Mathf.Max(
                0f,
                totalLength
            );


        int reflectionCount = 0;


        Vector3 currentOrigin =
            origin;


        Vector3 currentDirection =
            direction.normalized;


        bool hasHitSlope = false;



        while (remainingLength > 0.001f)
        {
            RaycastHit hit;


            bool hitSomething =
                TryGetClosestAimHit(
                    currentOrigin,
                    currentDirection,
                    Mathf.Max(
                        0.001f,
                        aimBallRadius
                    ),
                    Mathf.Min(
                        slopeDetectionDistance,
                        remainingLength
                    ),
                    out hit
                );


            if (!hitSomething)
            {
                aimPathPoints.Add(
                    currentOrigin +
                    currentDirection *
                    remainingLength
                );

                break;
            }



            float distance =
                Mathf.Max(
                    0f,
                    hit.distance
                );


            Vector3 hitPoint =
                currentOrigin +
                currentDirection *
                distance;


            aimPathPoints.Add(
                hitPoint
            );


            remainingLength -=
                distance;



            if (!hasHitSlope)
            {
                hasHitSlope = true;

Vector3 downhillDirection = Vector3.down;



                if (downhillDirection.sqrMagnitude >
                    0.0001f)
                {
                    downhillDirection.Normalize();


                    currentDirection =
                        downhillDirection;


                    currentOrigin =
                        hitPoint +
                        currentDirection *
                        slopeSurfaceOffset;


                    remainingLength -=
                        slopeSurfaceOffset;


                    continue;
                }


                break;
            }



            if (reflectionCount >=
                maximumAimReflections)
            {
                break;
            }


            Vector3 reflectedDirection =
                Vector3.Reflect(
                    currentDirection,
                    hit.normal
                ).normalized;


            if (reflectedDirection.sqrMagnitude <
                0.0001f)
            {
                break;
            }


            currentDirection =
                reflectedDirection;


            currentOrigin =
                hitPoint +
                currentDirection *
                reflectionSurfaceOffset;


            remainingLength -=
                reflectionSurfaceOffset;


            reflectionCount++;
        }
    }



    // =========================================================
    // SPHERE CAST
    // =========================================================

    bool TryGetClosestAimHit(
        Vector3 origin,
        Vector3 direction,
        float radius,
        float distance,
        out RaycastHit closestHit)
    {
        int hitCount =
            Physics.SphereCastNonAlloc(
                origin,
                radius,
                direction,
                aimHitBuffer,
                distance,
                aimCollisionMask,
                QueryTriggerInteraction.Ignore
            );


        closestHit = default;


        float closestDistance =
            float.PositiveInfinity;


        for (int i = 0;
             i < hitCount;
             i++)
        {
            RaycastHit candidate =
                aimHitBuffer[i];


            if (candidate.collider == null)
                continue;


            if (ShouldIgnoreAimCollider(
                candidate.collider))
            {
                continue;
            }


            if (candidate.distance >=
                closestDistance)
            {
                continue;
            }


            closestHit =
                candidate;


            closestDistance =
                candidate.distance;
        }


        return closestDistance <
               float.PositiveInfinity;
    }



    // =========================================================
    // IGNORE OWN OBJECTS
    // =========================================================

    bool ShouldIgnoreAimCollider(
        Collider collider)
    {
        Transform t =
            collider.transform;


        if (controlObject != null)
        {
            if (t == controlObject ||
                t.IsChildOf(controlObject))
            {
                return true;
            }
        }


        if (transform == t ||
            t.IsChildOf(transform))
        {
            return true;
        }


        return false;
    }



    // =========================================================
    // PLACE DOTS
    // =========================================================

    int PlaceDotsAlongAimPath(
        int maximumDotCount,
        float spacing)
    {
        if (maximumDotCount <= 0)
            return 0;


        if (aimPathPoints.Count < 2)
            return 0;


        int dotIndex = 0;


        float distanceToNextDot =
            spacing;


        for (int segmentIndex = 0;
             segmentIndex <
                 aimPathPoints.Count - 1 &&
             dotIndex <
                 maximumDotCount;
             segmentIndex++)
        {
            Vector3 start =
                aimPathPoints[
                    segmentIndex
                ];


            Vector3 segment =
                aimPathPoints[
                    segmentIndex + 1
                ] - start;


            float segmentLength =
                segment.magnitude;


            if (segmentLength <= 0.001f)
                continue;


            Vector3 segmentDirection =
                segment /
                segmentLength;


            while (
                distanceToNextDot <=
                segmentLength +
                0.0001f &&
                dotIndex <
                maximumDotCount)
            {
                GameObject dot =
                    aimDots[dotIndex];


                dot.transform.position =
                    start +
                    segmentDirection *
                    distanceToNextDot;


                dot.SetActive(true);


                dotIndex++;


                distanceToNextDot +=
                    spacing;
            }


            distanceToNextDot -=
                segmentLength;
        }


        return dotIndex;
    }



    // =========================================================
    // CREATE DOTS
    // =========================================================

    void CreateDots(int count)
    {
        while (aimDots.Count < count)
        {
            GameObject dot =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );


            dot.name =
                "AimDot";


            dot.transform.SetParent(
                transform
            );


            dot.transform.localScale =
                Vector3.one *
                dotSize;


            Collider collider =
                dot.GetComponent<Collider>();


            if (collider != null)
            {
                Destroy(collider);
            }


            Renderer renderer =
                dot.GetComponent<Renderer>();


            if (renderer != null)
            {
                if (aimDotMaterial != null)
                {
                    renderer.material =
                        aimDotMaterial;
                }
                else
                {
                    Material material =
                        new Material(
                            Shader.Find(
                                "Universal Render Pipeline/Lit"
                            )
                        );


                    material.color =
                        Color.white;


                    renderer.material =
                        material;
                }
            }


            dot.SetActive(false);


            aimDots.Add(dot);
        }
    }



    // =========================================================
    // SHOW AIM
    // =========================================================

    void ShowAimGuide()
    {
        UpdateAimGuide();
    }



    // =========================================================
    // HIDE AIM
    // =========================================================

    void HideAimGuide()
    {
        for (int i = 0;
             i < aimDots.Count;
             i++)
        {
            if (aimDots[i] != null)
            {
                aimDots[i].SetActive(false);
            }
        }
    }



    // =========================================================
    // RELEASE BALL
    // =========================================================

    void ReleaseBall()
    {
        controlling = false;

        charging = false;


        HideAimGuide();


        if (ballPrefab == null)
        {
            Debug.LogWarning(
                "Ball Prefab is not assigned."
            );

            return;
        }


        float power =
            minimumPower;


        if (powerSlider != null)
        {
            power =
                powerSlider.value;
        }


        power =
            Mathf.Clamp(
                power,
                minimumPower,
                maximumPower
            );


        Vector3 spawnPosition = releasePosition;

        GameObject ball =
            Instantiate(
                ballPrefab,
                spawnPosition,
                Quaternion.identity
            );


        releasedBalls.RemoveAll(releasedBall => releasedBall == null);
        releasedBalls.Add(ball);

        if (ballLifetime > 0f)
        {
            Destroy(ball, ballLifetime);
        }


        Rigidbody ballRb =
            ball.GetComponent<Rigidbody>();


        if (ballRb == null)
        {
            Debug.LogWarning(
                "Ball Prefab needs Rigidbody."
            );

            ResetPower();

            return;
        }


        ballRb.isKinematic = false;

        ballRb.useGravity = true;


        ballRb.linearVelocity =
            Vector3.zero;


        ballRb.angularVelocity =
            Vector3.zero;


        // Real-physics approach:
        // Power only decides the ONE-TIME launch speed (like the strength
        // behind a throw). After this, Unity's own gravity + Rigidbody
        // collision response take over completely - no per-frame velocity
        // overrides, no artificial "extra downward acceleration", and no
        // forced minimum speed. If the ball rolls slower on a shallow
        // slope or faster on a steep one, that is real gravity + friction,
        // not a script pretending to be physics.
        float normalizedPower =
            Mathf.InverseLerp(
                minimumPower,
                maximumPower,
                power
            );


        float fallSpeed =
            Mathf.Lerp(
                minimumFallSpeed,
                maximumFallSpeed,
                normalizedPower
            );


        ballRb.linearVelocity =
            Vector3.down *
            fallSpeed;


        // Realistic slope sliding/rolling now comes from a real PhysicsMaterial
        // (friction + bounciness) instead of a script forcing a speed floor.
        if (ballPhysicsMaterial != null)
        {
            Collider ballCollider =
                ball.GetComponent<Collider>();

            if (ballCollider != null)
            {
                ballCollider.material = ballPhysicsMaterial;
            }
        }


        ResetPower();
    }



    // =========================================================
    // SCREEN TO WORLD
    // =========================================================

    bool TryGetDropPosition(Vector2 screenPosition, out Vector3 worldPosition)
    {
        Ray ray = gameplayCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(
            ray, Mathf.Infinity, aimCollisionMask, QueryTriggerInteraction.Ignore);
        float nearestDistance = float.PositiveInfinity;
        worldPosition = Vector3.zero;

        foreach (RaycastHit hit in hits)
        {
            bool isReleasedBall = false;
            foreach (GameObject ball in releasedBalls)
            {
                if (ball != null && hit.transform.IsChildOf(ball.transform))
                {
                    isReleasedBall = true;
                    break;
                }
            }

            if (isReleasedBall || hit.normal.y <= 0.01f || hit.distance >= nearestDistance)
                continue;

            nearestDistance = hit.distance;
            worldPosition = hit.point + Vector3.up * Mathf.Max(0.1f, clickDropHeight);
        }

        if (!float.IsPositiveInfinity(nearestDistance))
            return true;

        return TryGetWorldPosition(screenPosition, out worldPosition);
    }

    bool TryGetWorldPosition(
        Vector2 screenPosition,
        out Vector3 worldPosition)
    {
        worldPosition =
            Vector3.zero;


        if (gameplayCamera == null)
            return false;


        Ray ray =
            gameplayCamera.ScreenPointToRay(
                screenPosition
            );


        float distance;
        if (controlling)
            ray.origin += dragCameraOrigin - gameplayCamera.transform.position;


        if (movementPlane.Raycast(
            ray,
            out distance))
        {
            worldPosition =
                ray.GetPoint(distance);


            return true;
        }


        return false;
    }



    // =========================================================
    // INPUT ZONE
    // =========================================================

    bool IsInsideZone(
        Vector2 position)
    {
        if (inputZone == null)
            return true;


        return
            RectTransformUtility
            .RectangleContainsScreenPoint(
                inputZone,
                position,
                null
            );
    }



    // =========================================================
    // RESET POWER
    // =========================================================

    void ResetPower()
    {
        if (powerSlider != null)
        {
            powerSlider.value = 0f;
        }
    }
}











