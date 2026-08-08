//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Serialization;

//public sealed class TowerLevelGenerator : MonoBehaviour
//{
//    [Header("Level Prefabs")]
//    [SerializeField] private GameObject tablePrefab;

//    [FormerlySerializedAs("blockPrefab")]
//    [SerializeField] private GameObject canPrefab;

//    [Header("Level Position")]
//    [Tooltip("Empty ho to generator ka apna Transform use hoga.")]
//    [SerializeField] private Transform levelOrigin;

//    [Tooltip("Tower ko table ke top-center se offset dene ke liye.")]
//    [SerializeField] private Vector3 towerOffset;

//    [Header("Can Pyramid Layout")]
//    [Tooltip("Bottom row mein kitni cans hongi.")]
//    [FormerlySerializedAs("blocksPerRow")]
//    [SerializeField, Min(1)] private int bottomRowCanCount = 5;

//    [Tooltip(
//        "Total rows. Ye Bottom Row Can Count se zyada nahi ho sakti. " +
//        "5 rows aur 5 bottom cans se pattern 5,4,3,2,1 banega."
//    )]
//    [SerializeField, Min(1)] private int numberOfRows = 5;

//    [Tooltip("Har can ka exact world diameter.")]
//    [SerializeField, Min(0.05f)] private float canDiameter = 0.75f;

//    [Tooltip("Har can ki exact world height.")]
//    [SerializeField, Min(0.05f)] private float canHeight = 1.05f;

//    [Tooltip("Same row ki cans ke darmiyan horizontal gap.")]
//    [SerializeField, Min(0f)] private float horizontalGap = 0.03f;

//    [Tooltip("Rows ke darmiyan vertical gap. Recommended 0.")]
//    [SerializeField, Min(0f)] private float verticalGap = 0f;

//    [Tooltip(
//        "Can visual prefab ki existing size aur orientation ignore karke " +
//        "script usay upright aur exact size mein fit karegi."
//    )]
//    [SerializeField] private bool automaticallyFitCanVisual = true;

//    [Header("Generated Can Physics")]
//    [Tooltip(
//        "Generated can root par BoxCollider use hota hai taake stack stable rahe. " +
//        "Visual cylinder hi rahega."
//    )]
//    [SerializeField, Range(0.5f, 1f)]
//    private float colliderDiameterMultiplier = 0.92f;

//    [SerializeField, Min(0.01f)] private float canMass = 0.8f;

//    [Tooltip("Generated can root kis layer par hoga.")]
//    [SerializeField] private int generatedCanLayer = 0;

//    [Header("Generation")]
//    [SerializeField] private bool generateOnStart = true;

//    [Tooltip(
//        "Level start par tamam cans locked rahengi. " +
//        "Sirf hit can aur unsupported upper cans dynamic hongi."
//    )]
//    [SerializeField] private bool lockCansOnSpawn = true;

//    [Header("Cannonball Detection")]
//    [Tooltip("Sirf CannonBall ya Projectile layer select karein.")]
//    [SerializeField] private LayerMask cannonBallLayers;

//    [Header("Hit Response")]
//    [Tooltip("Cannonball velocity ka kitna impulse hit can ko transfer hoga.")]
//    [SerializeField, Range(0f, 2f)]
//    private float impactTransferMultiplier = 0.30f;

//    [SerializeField, Min(0f)]
//    private float maximumImpactImpulse = 12f;

//    [Header("Pyramid Support Chain")]
//    [Tooltip(
//        "Upper can ke 2 lower supports hote hain. " +
//        "2 rakhne par ek support hatte hi upper can girna start karegi. " +
//        "1 rakhne par dono supports hatne ke baad giregi."
//    )]
//    [SerializeField, Range(1, 2)]
//    private int minimumValidSupports = 2;

//    [Tooltip(
//        "Lower support itna move kare to upper can usay lost support samjhegi."
//    )]
//    [SerializeField, Min(0.01f)]
//    private float supportMovementTolerance = 0.14f;

//    [Tooltip(
//        "Lower support itna tilt kare to upper can usay lost support samjhegi."
//    )]
//    [SerializeField, Range(1f, 90f)]
//    private float supportTiltTolerance = 15f;

//    [Tooltip(
//        "Support lose hone ke kitne physics frames baad upper can activate hogi."
//    )]
//    [SerializeField, Range(1, 10)]
//    private int unsupportedFramesRequired = 2;

//    [Header("Fallen Can Cleanup")]
//    [SerializeField] private bool destroyFallenCans = true;

//    [Tooltip("Can fall detect hone ke kitne seconds baad destroy hogi.")]
//    [SerializeField, Min(0f)]
//    private float fallenCanDestroyDelay = 3f;

//    [Tooltip("Itna tilt hone par can fallen consider hogi.")]
//    [SerializeField, Range(1f, 180f)]
//    private float fallenTiltAngle = 50f;

//    [Tooltip("Tilt ke saath minimum movement.")]
//    [SerializeField, Min(0f)]
//    private float minimumMovementBeforeFall = 0.12f;

//    [Tooltip("Table top se itna neeche jane par can fallen hogi.")]
//    [SerializeField, Min(0f)]
//    private float fallenBelowTableDistance = 0.25f;

//    [Tooltip(
//        "Can initial stack position se itni horizontal distance move kar jaye " +
//        "to usay fallen/out-of-stack consider kiya jayega."
//    )]
//    [SerializeField, Min(0f)]
//    private float fallenHorizontalDistance = 0.65f;

//    private sealed class CanRuntimeData
//    {
//        public GameObject Root;
//        public Rigidbody Body;
//        public Collider[] Colliders;

//        public int Row;
//        public int Index;

//        public Vector3 InitialPosition;
//        public Vector3 InitialUp;

//        public bool IsDynamic;
//        public bool DestroyScheduled;
//        public int UnsupportedFrames;

//        public readonly List<CanRuntimeData> Supports =
//            new List<CanRuntimeData>(2);
//    }

//    private Transform generatedLevelRoot;
//    private Coroutine generationCoroutine;

//    private readonly List<CanRuntimeData> spawnedCans =
//        new List<CanRuntimeData>();

//    private readonly List<List<CanRuntimeData>> cansByRow =
//        new List<List<CanRuntimeData>>();

//    private readonly Dictionary<Rigidbody, CanRuntimeData> cansByRigidbody =
//        new Dictionary<Rigidbody, CanRuntimeData>();

//    private float tableTopY;
//    private bool hasActiveCans;

//    private void Start()
//    {
//        if (generateOnStart)
//        {
//            GenerateLevel();
//        }
//    }

//    private void FixedUpdate()
//    {
//        if (!hasActiveCans)
//        {
//            return;
//        }

//        UpdateCanSupportChain();
//        UpdateFallenCanCleanup();
//        RemoveDestroyedCanReferences();
//    }

//    [ContextMenu("Generate Level")]
//    public void GenerateLevel()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning(
//                "Tower generation ko Play Mode mein test karein.",
//                this
//            );

//            return;
//        }

//        if (generationCoroutine != null)
//        {
//            StopCoroutine(generationCoroutine);
//        }

//        generationCoroutine =
//            StartCoroutine(GenerateLevelRoutine());
//    }

//    private IEnumerator GenerateLevelRoutine()
//    {
//        ClearGeneratedLevel();

//        if (!ValidateReferences())
//        {
//            generationCoroutine = null;
//            yield break;
//        }

//        Transform origin =
//            levelOrigin != null
//                ? levelOrigin
//                : transform;

//        generatedLevelRoot =
//            new GameObject("GeneratedCanPyramid").transform;

//        generatedLevelRoot.SetPositionAndRotation(
//            origin.position,
//            origin.rotation
//        );

//        generatedLevelRoot.localScale = Vector3.one;

//        GameObject generatedTable = Instantiate(
//            tablePrefab,
//            origin.position,
//            origin.rotation,
//            generatedLevelRoot
//        );

//        generatedTable.name = "GeneratedTable";

//        if (!TryGetCombinedBounds(
//                generatedTable,
//                out Bounds tableBounds))
//        {
//            Debug.LogError(
//                "Table prefab par Collider ya Renderer nahi mila.",
//                generatedTable
//            );

//            generationCoroutine = null;
//            yield break;
//        }

//        tableTopY = tableBounds.max.y;

//        Vector3 tableTopCenter = new Vector3(
//            tableBounds.center.x,
//            tableTopY,
//            tableBounds.center.z
//        );

//        Vector3 rotatedOffset =
//            origin.rotation *
//            new Vector3(
//                towerOffset.x,
//                0f,
//                towerOffset.z
//            );

//        Vector3 pyramidBaseCenter =
//            tableTopCenter +
//            rotatedOffset +
//            Vector3.up * towerOffset.y;

//        SpawnCanPyramid(
//            pyramidBaseCenter,
//            origin.rotation
//        );

//        BuildSupportLinks();

//        Physics.SyncTransforms();

//        // Generation ke baad ek physics frame tak cans locked rahengi.
//        yield return new WaitForFixedUpdate();

//        LockAllCans();

//        generationCoroutine = null;
//    }

//    private void SpawnCanPyramid(
//        Vector3 pyramidBaseCenter,
//        Quaternion levelRotation)
//    {
//        spawnedCans.Clear();
//        cansByRow.Clear();
//        cansByRigidbody.Clear();

//        int actualRows =
//            Mathf.Min(
//                numberOfRows,
//                bottomRowCanCount
//            );

//        float horizontalStep =
//            canDiameter + horizontalGap;

//        float verticalStep =
//            canHeight + verticalGap;

//        Vector3 levelRight =
//            levelRotation * Vector3.right;

//        for (int row = 0; row < actualRows; row++)
//        {
//            int cansInRow =
//                bottomRowCanCount - row;

//            List<CanRuntimeData> currentRow =
//                new List<CanRuntimeData>(cansInRow);

//            cansByRow.Add(currentRow);

//            float rowCenterOffset =
//                (cansInRow - 1) * 0.5f;

//            for (int index = 0;
//                 index < cansInRow;
//                 index++)
//            {
//                float horizontalOffset =
//                    (index - rowCenterOffset) *
//                    horizontalStep;

//                float verticalOffset =
//                    canHeight * 0.5f +
//                    row * verticalStep;

//                Vector3 canCenter =
//                    pyramidBaseCenter +
//                    levelRight * horizontalOffset +
//                    Vector3.up * verticalOffset;

//                CanRuntimeData canData =
//                    SpawnCan(
//                        canCenter,
//                        levelRotation,
//                        row,
//                        index
//                    );

//                if (canData != null)
//                {
//                    currentRow.Add(canData);
//                }
//            }
//        }
//    }

//    private CanRuntimeData SpawnCan(
//        Vector3 requiredCenter,
//        Quaternion levelRotation,
//        int row,
//        int index)
//    {
//        GameObject canRoot =
//            new GameObject(
//                $"TowerCan_Row_{row + 1}_Can_{index + 1}"
//            );

//        canRoot.transform.SetParent(
//            generatedLevelRoot,
//            false
//        );

//        canRoot.transform.SetPositionAndRotation(
//            requiredCenter,
//            levelRotation
//        );

//        canRoot.layer =
//            Mathf.Clamp(
//                generatedCanLayer,
//                0,
//                31
//            );

//        // Exact stable physics root.
//        BoxCollider boxCollider =
//            canRoot.AddComponent<BoxCollider>();

//        boxCollider.center = Vector3.zero;
//        boxCollider.size = new Vector3(
//            canDiameter * colliderDiameterMultiplier,
//            canHeight,
//            canDiameter * colliderDiameterMultiplier
//        );

//        Rigidbody canBody =
//            canRoot.AddComponent<Rigidbody>();

//        canBody.mass = canMass;
//        canBody.useGravity = false;
//        canBody.isKinematic = true;
//        canBody.detectCollisions = true;
//        canBody.interpolation = RigidbodyInterpolation.Interpolate;
//        canBody.collisionDetectionMode =
//            CollisionDetectionMode.ContinuousSpeculative;

//        SetBodyVelocity(
//            canBody,
//            Vector3.zero
//        );

//        canBody.angularVelocity =
//            Vector3.zero;

//        // Visual ko separate child ke taur par spawn karo.
//        GameObject visualContainer =
//            new GameObject("Visual");

//        visualContainer.transform.SetParent(
//            canRoot.transform,
//            false
//        );

//        visualContainer.transform.localPosition =
//            Vector3.zero;

//        visualContainer.transform.localRotation =
//            Quaternion.identity;

//        visualContainer.transform.localScale =
//            Vector3.one;

//        GameObject canVisual =
//            Instantiate(
//                canPrefab,
//                visualContainer.transform
//            );

//        canVisual.name = "CanVisual";

//        PrepareVisualPrefab(canVisual);

//        if (automaticallyFitCanVisual)
//        {
//            if (!FitVisualInsideCanRoot(
//                    canVisual,
//                    visualContainer.transform))
//            {
//                Debug.LogWarning(
//                    "Can visual automatically fit nahi ho saka. " +
//                    "Prefab par enabled Renderer hona chahiye.",
//                    canVisual
//                );
//            }
//        }
//        else
//        {
//            canVisual.transform.localPosition =
//                Vector3.zero;

//            canVisual.transform.localRotation =
//                Quaternion.identity;

//            canVisual.transform.localScale =
//                Vector3.one;
//        }

//        Collider[] generatedColliders =
//            canRoot.GetComponentsInChildren<Collider>(true);

//        CanRuntimeData canData =
//            new CanRuntimeData
//            {
//                Root = canRoot,
//                Body = canBody,
//                Colliders = generatedColliders,

//                Row = row,
//                Index = index,

//                InitialPosition = canBody.position,
//                InitialUp = canBody.transform.up,

//                IsDynamic = false,
//                DestroyScheduled = false,
//                UnsupportedFrames = 0
//            };

//        spawnedCans.Add(canData);
//        cansByRigidbody[canBody] = canData;

//        GeneratedTowerCanHitReceiver hitReceiver =
//            canRoot.AddComponent<
//                GeneratedTowerCanHitReceiver>();

//        hitReceiver.Initialize(
//            this,
//            canBody
//        );

//        return canData;
//    }

//    private void PrepareVisualPrefab(
//        GameObject visualInstance)
//    {
//        // Visual prefab ke colliders disable kar do.
//        Collider[] visualColliders =
//            visualInstance.GetComponentsInChildren<
//                Collider>(true);

//        foreach (Collider visualCollider
//                 in visualColliders)
//        {
//            if (visualCollider != null)
//            {
//                visualCollider.enabled = false;
//            }
//        }

//        // Nested rigidbodies physics root ke saath conflict na karein.
//        Rigidbody[] visualBodies =
//            visualInstance.GetComponentsInChildren<
//                Rigidbody>(true);

//        foreach (Rigidbody visualBody
//                 in visualBodies)
//        {
//            if (visualBody == null)
//            {
//                continue;
//            }

//            visualBody.detectCollisions = false;
//            visualBody.isKinematic = true;

//            Destroy(visualBody);
//        }
//    }

//    private bool FitVisualInsideCanRoot(
//        GameObject visualInstance,
//        Transform visualContainer)
//    {
//        Transform visualTransform =
//            visualInstance.transform;

//        visualTransform.localPosition =
//            Vector3.zero;

//        visualTransform.localRotation =
//            Quaternion.identity;

//        visualTransform.localScale =
//            Vector3.one;

//        visualContainer.localPosition =
//            Vector3.zero;

//        visualContainer.localRotation =
//            Quaternion.identity;

//        visualContainer.localScale =
//            Vector3.one;

//        Physics.SyncTransforms();

//        if (!TryGetRendererBounds(
//                visualInstance,
//                out Bounds originalBounds))
//        {
//            return false;
//        }

//        Vector3 originalSize =
//            originalBounds.size;

//        /*
//         * Longest visual axis ko Y-axis par lao.
//         * Is se horizontal imported cylinder bhi upright ho jayega.
//         */
//        Quaternion orientationCorrection =
//            Quaternion.identity;

//        if (originalSize.x >
//            Mathf.Max(originalSize.y, originalSize.z))
//        {
//            orientationCorrection =
//                Quaternion.Euler(0f, 0f, 90f);
//        }
//        else if (originalSize.z >
//                 Mathf.Max(originalSize.x, originalSize.y))
//        {
//            orientationCorrection =
//                Quaternion.Euler(90f, 0f, 0f);
//        }

//        visualTransform.localRotation =
//            orientationCorrection;

//        Physics.SyncTransforms();

//        if (!TryGetRendererBounds(
//                visualInstance,
//                out Bounds uprightBounds))
//        {
//            return false;
//        }

//        Vector3 uprightSize =
//            uprightBounds.size;

//        if (uprightSize.x <= 0.0001f ||
//            uprightSize.y <= 0.0001f ||
//            uprightSize.z <= 0.0001f)
//        {
//            return false;
//        }

//        /*
//         * VisualContainer root axes ke saath aligned hai,
//         * is liye exact world X/Y/Z size apply ho sakti hai.
//         */
//        visualContainer.localScale =
//            new Vector3(
//                canDiameter / uprightSize.x,
//                canHeight / uprightSize.y,
//                canDiameter / uprightSize.z
//            );

//        Physics.SyncTransforms();

//        if (!TryGetRendererBounds(
//                visualInstance,
//                out Bounds resizedBounds))
//        {
//            return false;
//        }

//        /*
//         * Imported pivot center par na ho tab bhi
//         * visual ko physics collider ke exact center mein rakho.
//         */
//        Vector3 centerCorrection =
//            visualContainer.position -
//            resizedBounds.center;

//        visualContainer.position +=
//            centerCorrection;

//        Physics.SyncTransforms();

//        return true;
//    }

//    private void BuildSupportLinks()
//    {
//        for (int row = 0;
//             row < cansByRow.Count;
//             row++)
//        {
//            List<CanRuntimeData> currentRow =
//                cansByRow[row];

//            for (int index = 0;
//                 index < currentRow.Count;
//                 index++)
//            {
//                CanRuntimeData canData =
//                    currentRow[index];

//                canData.Supports.Clear();

//                // Bottom row ko table support karti hai.
//                if (row == 0)
//                {
//                    continue;
//                }

//                List<CanRuntimeData> lowerRow =
//                    cansByRow[row - 1];

//                /*
//                 * Pyramid placement:
//                 * Upper can index i ko lower row ki
//                 * i aur i+1 cans support karti hain.
//                 */
//                if (index < lowerRow.Count)
//                {
//                    canData.Supports.Add(
//                        lowerRow[index]
//                    );
//                }

//                if (index + 1 < lowerRow.Count)
//                {
//                    canData.Supports.Add(
//                        lowerRow[index + 1]
//                    );
//                }
//            }
//        }
//    }

//    private void LockAllCans()
//    {
//        hasActiveCans = false;

//        foreach (CanRuntimeData canData
//                 in spawnedCans)
//        {
//            if (canData == null ||
//                canData.Body == null)
//            {
//                continue;
//            }

//            Rigidbody body =
//                canData.Body;

//            SetBodyVelocity(
//                body,
//                Vector3.zero
//            );

//            body.angularVelocity =
//                Vector3.zero;

//            body.useGravity =
//                !lockCansOnSpawn;

//            body.isKinematic =
//                lockCansOnSpawn;

//            canData.IsDynamic =
//                !lockCansOnSpawn;

//            canData.UnsupportedFrames = 0;
//        }

//        Physics.SyncTransforms();

//        if (!lockCansOnSpawn)
//        {
//            hasActiveCans = true;
//        }
//    }

//    public bool IsCannonBallCollision(
//        Collision collision)
//    {
//        if (collision == null ||
//            collision.collider == null)
//        {
//            return false;
//        }

//        int colliderLayer =
//            collision.collider.gameObject.layer;

//        if (IsLayerInsideMask(
//                colliderLayer,
//                cannonBallLayers))
//        {
//            return true;
//        }

//        if (collision.rigidbody != null)
//        {
//            int bodyLayer =
//                collision.rigidbody.gameObject.layer;

//            return IsLayerInsideMask(
//                bodyLayer,
//                cannonBallLayers
//            );
//        }

//        return false;
//    }

//    public void HandleCanHit(
//        Rigidbody canBody,
//        Collision collision)
//    {
//        if (canBody == null ||
//            collision == null)
//        {
//            return;
//        }

//        if (!cansByRigidbody.TryGetValue(
//                canBody,
//                out CanRuntimeData canData))
//        {
//            return;
//        }

//        /*
//         * Dynamic can ko normal Unity physics hit handle karegi.
//         */
//        if (canData.IsDynamic)
//        {
//            return;
//        }

//        Vector3 incomingVelocity =
//            collision.relativeVelocity;

//        float incomingMass = 1f;

//        if (collision.rigidbody != null)
//        {
//            incomingVelocity =
//                GetBodyVelocity(
//                    collision.rigidbody
//                );

//            incomingMass =
//                Mathf.Max(
//                    0.01f,
//                    collision.rigidbody.mass
//                );
//        }

//        Vector3 impactPoint =
//            canBody.worldCenterOfMass;

//        if (collision.contactCount > 0)
//        {
//            impactPoint =
//                collision.GetContact(0).point;
//        }

//        /*
//         * Sirf hit can dynamic hogi.
//         */
//        ActivateCan(
//            canData,
//            incomingVelocity,
//            incomingMass,
//            impactPoint,
//            true
//        );
//    }

//    private void ActivateCan(
//        CanRuntimeData canData,
//        Vector3 incomingVelocity,
//        float incomingMass,
//        Vector3 impactPoint,
//        bool applyImpact)
//    {
//        if (canData == null ||
//            canData.Body == null ||
//            canData.IsDynamic)
//        {
//            return;
//        }

//        Rigidbody body =
//            canData.Body;

//        SetBodyVelocity(
//            body,
//            Vector3.zero
//        );

//        body.angularVelocity =
//            Vector3.zero;

//        body.isKinematic = false;
//        body.useGravity = true;
//        body.WakeUp();

//        canData.IsDynamic = true;
//        canData.UnsupportedFrames = 0;

//        hasActiveCans = true;

//        if (!applyImpact ||
//            incomingVelocity.sqrMagnitude < 0.001f)
//        {
//            return;
//        }

//        Vector3 impulse =
//            incomingVelocity *
//            incomingMass *
//            impactTransferMultiplier;

//        impulse =
//            Vector3.ClampMagnitude(
//                impulse,
//                maximumImpactImpulse
//            );

//        body.AddForceAtPosition(
//            impulse,
//            impactPoint,
//            ForceMode.Impulse
//        );
//    }

//    private void UpdateCanSupportChain()
//    {
//        foreach (CanRuntimeData canData
//                 in spawnedCans)
//        {
//            if (canData == null ||
//                canData.Root == null ||
//                canData.Body == null ||
//                canData.IsDynamic)
//            {
//                continue;
//            }

//            // Bottom row ko table support karti hai.
//            if (canData.Row == 0)
//            {
//                continue;
//            }

//            int validSupportCount = 0;

//            foreach (CanRuntimeData support
//                     in canData.Supports)
//            {
//                if (IsSupportValid(support))
//                {
//                    validSupportCount++;
//                }
//            }

//            if (validSupportCount >=
//                minimumValidSupports)
//            {
//                canData.UnsupportedFrames = 0;
//                continue;
//            }

//            canData.UnsupportedFrames++;

//            if (canData.UnsupportedFrames <
//                unsupportedFramesRequired)
//            {
//                continue;
//            }

//            /*
//             * Lower support move/fall ho gayi.
//             * Sirf ye upper can ab dynamic hogi.
//             */
//            ActivateCan(
//                canData,
//                Vector3.zero,
//                1f,
//                canData.Body.worldCenterOfMass,
//                false
//            );
//        }
//    }

//    private bool IsSupportValid(
//        CanRuntimeData support)
//    {
//        if (support == null ||
//            support.Root == null ||
//            support.Body == null ||
//            support.DestroyScheduled)
//        {
//            return false;
//        }

//        /*
//         * Locked support hamesha valid hai.
//         */
//        if (!support.IsDynamic)
//        {
//            return true;
//        }

//        float movement =
//            Vector3.Distance(
//                support.Body.position,
//                support.InitialPosition
//            );

//        float tilt =
//            Vector3.Angle(
//                support.Body.transform.up,
//                support.InitialUp
//            );

//        return
//            movement <= supportMovementTolerance &&
//            tilt <= supportTiltTolerance;
//    }

//    private void UpdateFallenCanCleanup()
//    {
//        if (!destroyFallenCans)
//        {
//            return;
//        }

//        foreach (CanRuntimeData canData
//                 in spawnedCans)
//        {
//            if (canData == null ||
//                canData.Root == null ||
//                canData.Body == null ||
//                !canData.IsDynamic ||
//                canData.DestroyScheduled)
//            {
//                continue;
//            }

//            if (!IsCanFallen(canData))
//            {
//                continue;
//            }

//            canData.DestroyScheduled = true;

//            Destroy(
//                canData.Root,
//                fallenCanDestroyDelay
//            );
//        }
//    }

//    private bool IsCanFallen(
//        CanRuntimeData canData)
//    {
//        if (!TryGetCanBounds(
//                canData,
//                out Bounds bounds))
//        {
//            return false;
//        }

//        bool belowTable =
//            bounds.center.y <=
//            tableTopY -
//            fallenBelowTableDistance;

//        Vector3 movement =
//            canData.Body.position -
//            canData.InitialPosition;

//        float movementDistance =
//            movement.magnitude;

//        float horizontalMovement =
//            new Vector2(
//                movement.x,
//                movement.z
//            ).magnitude;

//        float tiltAngle =
//            Vector3.Angle(
//                canData.Body.transform.up,
//                canData.InitialUp
//            );

//        bool tippedOver =
//            movementDistance >=
//            minimumMovementBeforeFall &&
//            tiltAngle >=
//            fallenTiltAngle;

//        bool movedOutOfStack =
//            fallenHorizontalDistance > 0f &&
//            horizontalMovement >=
//            fallenHorizontalDistance;

//        return
//            belowTable ||
//            tippedOver ||
//            movedOutOfStack;
//    }

//    private void RemoveDestroyedCanReferences()
//    {
//        for (int i = spawnedCans.Count - 1;
//             i >= 0;
//             i--)
//        {
//            CanRuntimeData canData =
//                spawnedCans[i];

//            if (canData != null &&
//                canData.Root != null &&
//                canData.Body != null)
//            {
//                continue;
//            }

//            if (canData != null &&
//                canData.Body != null)
//            {
//                cansByRigidbody.Remove(
//                    canData.Body
//                );
//            }

//            spawnedCans.RemoveAt(i);
//        }
//    }

//    private bool TryGetCanBounds(
//        CanRuntimeData canData,
//        out Bounds combinedBounds)
//    {
//        combinedBounds = default;

//        if (canData == null ||
//            canData.Colliders == null)
//        {
//            return false;
//        }

//        bool hasBounds = false;

//        foreach (Collider canCollider
//                 in canData.Colliders)
//        {
//            if (canCollider == null ||
//                !canCollider.enabled ||
//                canCollider.isTrigger)
//            {
//                continue;
//            }

//            if (!hasBounds)
//            {
//                combinedBounds =
//                    canCollider.bounds;

//                hasBounds = true;
//            }
//            else
//            {
//                combinedBounds.Encapsulate(
//                    canCollider.bounds
//                );
//            }
//        }

//        return hasBounds;
//    }

//    private bool TryGetRendererBounds(
//        GameObject target,
//        out Bounds combinedBounds)
//    {
//        combinedBounds = default;

//        Renderer[] renderers =
//            target.GetComponentsInChildren<
//                Renderer>(true);

//        bool hasBounds = false;

//        foreach (Renderer targetRenderer
//                 in renderers)
//        {
//            if (targetRenderer == null ||
//                !targetRenderer.enabled)
//            {
//                continue;
//            }

//            if (!hasBounds)
//            {
//                combinedBounds =
//                    targetRenderer.bounds;

//                hasBounds = true;
//            }
//            else
//            {
//                combinedBounds.Encapsulate(
//                    targetRenderer.bounds
//                );
//            }
//        }

//        return hasBounds;
//    }

//    private bool TryGetCombinedBounds(
//        GameObject target,
//        out Bounds combinedBounds)
//    {
//        combinedBounds = default;

//        Collider[] colliders =
//            target.GetComponentsInChildren<
//                Collider>(true);

//        bool hasBounds = false;

//        foreach (Collider targetCollider
//                 in colliders)
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

//                hasBounds = true;
//            }
//            else
//            {
//                combinedBounds.Encapsulate(
//                    targetCollider.bounds
//                );
//            }
//        }

//        if (hasBounds)
//        {
//            return true;
//        }

//        return TryGetRendererBounds(
//            target,
//            out combinedBounds
//        );
//    }

//    private static bool IsLayerInsideMask(
//        int layer,
//        LayerMask layerMask)
//    {
//        return (
//            layerMask.value &
//            (1 << layer)
//        ) != 0;
//    }

//    private static Vector3 GetBodyVelocity(
//        Rigidbody body)
//    {
//        if (body == null)
//        {
//            return Vector3.zero;
//        }

//#if UNITY_6000_0_OR_NEWER
//        return body.linearVelocity;
//#else
//        return body.velocity;
//#endif
//    }

//    private static void SetBodyVelocity(
//        Rigidbody body,
//        Vector3 velocity)
//    {
//        if (body == null)
//        {
//            return;
//        }

//#if UNITY_6000_0_OR_NEWER
//        body.linearVelocity = velocity;
//#else
//        body.velocity = velocity;
//#endif
//    }

//    [ContextMenu("Clear Generated Level")]
//    public void ClearGeneratedLevel()
//    {
//        hasActiveCans = false;

//        spawnedCans.Clear();
//        cansByRow.Clear();
//        cansByRigidbody.Clear();

//        if (generatedLevelRoot == null)
//        {
//            return;
//        }

//        if (Application.isPlaying)
//        {
//            Destroy(
//                generatedLevelRoot.gameObject
//            );
//        }
//        else
//        {
//            DestroyImmediate(
//                generatedLevelRoot.gameObject
//            );
//        }

//        generatedLevelRoot = null;
//    }

//    private bool ValidateReferences()
//    {
//        if (tablePrefab == null)
//        {
//            Debug.LogError(
//                "Table Prefab assign nahi hai.",
//                this
//            );

//            return false;
//        }

//        if (canPrefab == null)
//        {
//            Debug.LogError(
//                "Can Prefab assign nahi hai.",
//                this
//            );

//            return false;
//        }

//        if (bottomRowCanCount < 1)
//        {
//            Debug.LogError(
//                "Bottom Row Can Count kam az kam 1 honi chahiye.",
//                this
//            );

//            return false;
//        }

//        if (numberOfRows < 1)
//        {
//            Debug.LogError(
//                "Number Of Rows kam az kam 1 honi chahiye.",
//                this
//            );

//            return false;
//        }

//        if (canDiameter <= 0f ||
//            canHeight <= 0f)
//        {
//            Debug.LogError(
//                "Can Diameter aur Can Height 0 se bari honi chahiye.",
//                this
//            );

//            return false;
//        }

//        if (cannonBallLayers.value == 0)
//        {
//            Debug.LogError(
//                "Cannon Ball Layers mein CannonBall/Projectile layer select karein.",
//                this
//            );

//            return false;
//        }

//        return true;
//    }
//}

///*
// * Isi TowerLevelGenerator.cs file mein rehne dein.
// * Can prefab par manually attach karne ki zaroorat nahi.
// */
//public sealed class GeneratedTowerCanHitReceiver :
//    MonoBehaviour
//{
//    private TowerLevelGenerator generator;
//    private Rigidbody canBody;

//    public void Initialize(
//        TowerLevelGenerator owner,
//        Rigidbody body)
//    {
//        generator = owner;
//        canBody = body;
//    }

//    private void OnCollisionEnter(
//        Collision collision)
//    {
//        if (generator == null ||
//            canBody == null ||
//            collision == null)
//        {
//            return;
//        }

//        if (!generator.IsCannonBallCollision(
//                collision))
//        {
//            return;
//        }

//        generator.HandleCanHit(
//            canBody,
//            collision
//        );
//    }
//}




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class TowerLevelGenerator : MonoBehaviour
{
    [Header("Level Prefabs")]
    [SerializeField] private GameObject tablePrefab;

    [FormerlySerializedAs("blockPrefab")]
    [SerializeField] private GameObject canPrefab;

    [Header("Level Position")]
    [Tooltip("Empty ho to generator ka apna Transform use hoga.")]
    [SerializeField] private Transform levelOrigin;

    [Tooltip("Tower ko table ke top-center se offset dene ke liye.")]
    [SerializeField] private Vector3 towerOffset;

    [Header("Can Size")]
    [SerializeField, Min(0.05f)]
    private float canDiameter = 0.38f;

    [SerializeField, Min(0.05f)]
    private float canHeight = 0.55f;

    [SerializeField, Min(0f)]
    private float horizontalGap = 0.015f;

    [SerializeField, Min(0f)]
    private float verticalGap = 0f;

    [Tooltip(
        "Can visual prefab ki existing size aur orientation ignore karke " +
        "script usay upright aur exact size mein fit karegi."
    )]
    [SerializeField]
    private bool automaticallyFitCanVisual = true;

    [Header("Simple Tower Pattern")]
    [Tooltip(
        "On hone par har new level mein simple centered tower pattern change hoga."
    )]
    [SerializeField]
    private bool randomizePatternEachLevel = true;

    [Tooltip("Randomization Off ho to bottom row mein itni cans hongi.")]
    [FormerlySerializedAs("blocksPerRow")]
    [SerializeField, Min(2)]
    private int bottomRowCanCount = 5;

    [Tooltip("Randomization Off ho to total rows.")]
    [SerializeField, Min(1)]
    private int numberOfRows = 5;

    [SerializeField, Min(2)]
    private int minimumBottomRowCans = 4;

    [SerializeField, Min(2)]
    private int maximumBottomRowCans = 6;

    [SerializeField, Min(2)]
    private int minimumTowerRows = 3;

    [SerializeField, Min(2)]
    private int maximumTowerRows = 5;

    [Tooltip("Same pattern do levels continuously repeat nahi hoga.")]
    [SerializeField]
    private bool avoidImmediatePatternRepeat = true;

    [Header("Generated Can Physics")]
    [Tooltip(
        "Generated can root par BoxCollider use hota hai taake stack stable rahe. " +
        "Visual cylinder hi rahega."
    )]
    [SerializeField, Range(0.5f, 1f)]
    private float colliderDiameterMultiplier = 0.92f;

    [SerializeField, Min(0.01f)]
    private float canMass = 0.45f;

    [Tooltip("Generated can root kis layer par hoga.")]
    [SerializeField, Range(0, 31)]
    private int generatedCanLayer = 0;

    [Header("Generation")]
    [SerializeField]
    private bool generateOnStart = true;

    [Tooltip(
        "Level start par tamam cans locked rahengi. " +
        "Sirf hit can aur unsupported upper cans dynamic hongi."
    )]
    [SerializeField]
    private bool lockCansOnSpawn = true;

    [Header("Cannonball Detection")]
    [Tooltip(
        "Sirf CannonBall ya Projectile layer select karein. " +
        "Everything select na karein."
    )]
    [SerializeField]
    private LayerMask cannonBallLayers;

    [Header("Hit Response")]
    [Tooltip("Cannonball velocity ka kitna impulse hit can ko transfer hoga.")]
    [SerializeField, Range(0f, 2f)]
    private float impactTransferMultiplier = 0.30f;

    [SerializeField, Min(0f)]
    private float maximumImpactImpulse = 12f;

    [Header("Pyramid Support Chain")]
    [Tooltip(
        "2 rakhne par do supports mein se ek bhi remove ho to upper can giray gi. " +
        "1 rakhne par dono supports remove hone ke baad upper can giray gi."
    )]
    [SerializeField, Range(1, 2)]
    private int minimumValidSupports = 2;

    [Tooltip(
        "Lower support itna move kare to upper can usay lost support samjhegi."
    )]
    [SerializeField, Min(0.01f)]
    private float supportMovementTolerance = 0.10f;

    [Tooltip(
        "Lower support itna tilt kare to upper can usay lost support samjhegi."
    )]
    [SerializeField, Range(1f, 90f)]
    private float supportTiltTolerance = 12f;

    [Tooltip(
        "Support lose hone ke kitne physics frames baad upper can activate hogi."
    )]
    [SerializeField, Range(1, 10)]
    private int unsupportedFramesRequired = 2;

    [Header("Fallen Can Cleanup")]
    [SerializeField]
    private bool destroyFallenCans = true;

    [Tooltip("Can fall detect hone ke kitne seconds baad destroy hogi.")]
    [SerializeField, Min(0f)]
    private float fallenCanDestroyDelay = 3f;

    [Tooltip("Itna tilt hone par can fallen consider hogi.")]
    [SerializeField, Range(1f, 180f)]
    private float fallenTiltAngle = 35f;

    [Tooltip("Tilt ke saath minimum movement.")]
    [SerializeField, Min(0f)]
    private float minimumMovementBeforeFall = 0.05f;

    [Tooltip("Table top se itna neeche jane par can fallen hogi.")]
    [SerializeField, Min(0f)]
    private float fallenBelowTableDistance = 0.10f;

    [Tooltip(
        "Can initial stack position se itni horizontal distance move kar jaye " +
        "to usay fallen/out-of-stack consider kiya jayega."
    )]
    [SerializeField, Min(0f)]
    private float fallenHorizontalDistance = 0.25f;

    [Tooltip(
        "Safety cleanup: Dynamic can itne seconds tak destroy na ho to " +
        "automatically fallen treat hogi. Zero se disable."
    )]
    [SerializeField, Min(0f)]
    private float maximumDynamicLifetime = 10f;

    [Header("Automatic Level Progression")]
    [Tooltip(
        "Tamam generated cans destroy hone ke baad next tower automatically load hoga."
    )]
    [SerializeField]
    private bool autoLoadNextLevel = true;

    [Tooltip("Last can remove hone ke baad next tower generate hone ka delay.")]
    [SerializeField, Min(0f)]
    private float nextLevelDelay = 1.25f;

    [SerializeField, Min(1)]
    private int startingLevel = 1;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs = true;

    public int CurrentLevel { get; private set; }
    public int RemainingCanCount => spawnedCans.Count;

    private enum SimpleTowerPattern
    {
        ClassicPyramid,
        DoubleBase,
        PairedRows,
        FlatTop
    }

    private sealed class CanRuntimeData
    {
        public GameObject Root;
        public Rigidbody Body;
        public Collider[] Colliders;

        public int GenerationId;
        public int Row;
        public int Index;

        public Vector3 InitialPosition;
        public Vector3 InitialUp;

        public bool IsDynamic;
        public bool DestroyScheduled;

        public int UnsupportedFrames;
        public float DynamicStartTime;

        public readonly List<CanRuntimeData> Supports =
            new List<CanRuntimeData>(2);
    }

    private Transform generatedLevelRoot;

    private Coroutine generationCoroutine;
    private Coroutine nextLevelCoroutine;

    private readonly List<CanRuntimeData> spawnedCans =
        new List<CanRuntimeData>();

    private readonly List<List<CanRuntimeData>> cansByRow =
        new List<List<CanRuntimeData>>();

    private readonly Dictionary<Rigidbody, CanRuntimeData> cansByRigidbody =
        new Dictionary<Rigidbody, CanRuntimeData>();

    private readonly List<int> activeRowPattern =
        new List<int>();

    private SimpleTowerPattern lastPattern =
        (SimpleTowerPattern)(-1);

    private int currentGenerationId;

    private float tableTopY;

    private bool hasActiveCans;
    private bool levelWasGenerated;
    private bool nextLevelQueued;

    private void Awake()
    {
        CurrentLevel =
            Mathf.Max(
                1,
                startingLevel
            );
    }

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateLevel();
        }
    }

    private void FixedUpdate()
    {
        if (hasActiveCans)
        {
            UpdateCanSupportChain();
            UpdateFallenCanCleanup();
        }

        /*
         * Ye checks hasActiveCans ke bahar rahenge.
         * Last can destroy hone ke baad bhi level completion detect hogi.
         */
        RemoveDestroyedCanReferences();
        CheckForLevelCompletion();
    }

    [ContextMenu("Generate Current Level")]
    public void GenerateLevel()
    {
        CancelPendingNextLevel();

        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Tower generation ko Play Mode mein test karein.",
                this
            );

            return;
        }

        if (generationCoroutine != null)
        {
            StopCoroutine(
                generationCoroutine
            );
        }

        generationCoroutine =
            StartCoroutine(
                GenerateLevelRoutine()
            );
    }

    [ContextMenu("Generate Next Level")]
    public void GenerateNextLevel()
    {
        CancelPendingNextLevel();

        CurrentLevel++;
        GenerateLevel();
    }

    [ContextMenu("Force Complete Current Level")]
    public void ForceCompleteCurrentLevel()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        List<CanRuntimeData> copy =
            new List<CanRuntimeData>(
                spawnedCans
            );

        foreach (CanRuntimeData canData
                 in copy)
        {
            if (canData == null)
            {
                continue;
            }

            if (canData.Root != null)
            {
                Destroy(
                    canData.Root
                );
            }

            UnregisterCan(
                canData
            );
        }

        CheckForLevelCompletion();
    }

    private IEnumerator GenerateLevelRoutine()
    {
        ClearGeneratedLevel();

        /*
         * Runtime Destroy end-of-frame par hota hai.
         * Ek frame wait karne se old tower completely remove ho jata hai.
         */
        yield return null;

        nextLevelQueued = false;
        levelWasGenerated = false;

        if (!ValidateReferences())
        {
            generationCoroutine = null;
            yield break;
        }
        currentGenerationId++;

        Transform origin =
            levelOrigin != null
                ? levelOrigin
                : transform;

        generatedLevelRoot =
            new GameObject(
                $"GeneratedCanPyramid_Level_{CurrentLevel}"
            ).transform;

        generatedLevelRoot.SetPositionAndRotation(
            origin.position,
            origin.rotation
        );

        generatedLevelRoot.localScale =
            Vector3.one;

        GameObject generatedTable = Instantiate(
            tablePrefab,
            origin.position,
            origin.rotation,
            generatedLevelRoot
        );

        generatedTable.name =
            "GeneratedTable";

        if (!TryGetCombinedBounds(
                generatedTable,
                out Bounds tableBounds))
        {
            Debug.LogError(
                "Table prefab par enabled Collider ya Renderer nahi mila.",
                generatedTable
            );

            generationCoroutine = null;
            yield break;
        }

        tableTopY =
            tableBounds.max.y;

        Vector3 tableTopCenter =
            new Vector3(
                tableBounds.center.x,
                tableTopY,
                tableBounds.center.z
            );

        Vector3 rotatedOffset =
            origin.rotation *
            new Vector3(
                towerOffset.x,
                0f,
                towerOffset.z
            );

        Vector3 pyramidBaseCenter =
            tableTopCenter +
            rotatedOffset +
            Vector3.up * towerOffset.y;

        BuildActiveRowPattern();

        SpawnCanPyramid(
            pyramidBaseCenter,
            origin.rotation
        );

        BuildSupportLinks();

        Physics.SyncTransforms();

        yield return
            new WaitForFixedUpdate();

        LockAllCans();

        levelWasGenerated =
            spawnedCans.Count > 0;

        if (showDebugLogs)
        {
            Debug.Log(
                $"Level {CurrentLevel} generated. " +
                $"Pattern: {GetActivePatternText()}. " +
                $"Total cans: {spawnedCans.Count}.",
                this
            );
        }

        generationCoroutine = null;
    }

    private void BuildActiveRowPattern()
    {
        activeRowPattern.Clear();

        int bottomCount;
        int requestedRows;

        if (randomizePatternEachLevel)
        {
            int minimumBottom =
                Mathf.Min(
                    minimumBottomRowCans,
                    maximumBottomRowCans
                );

            int maximumBottom =
                Mathf.Max(
                    minimumBottomRowCans,
                    maximumBottomRowCans
                );

            bottomCount =
                Random.Range(
                    minimumBottom,
                    maximumBottom + 1
                );

            int minimumRows =
                Mathf.Min(
                    minimumTowerRows,
                    maximumTowerRows
                );

            int maximumRows =
                Mathf.Max(
                    minimumTowerRows,
                    maximumTowerRows
                );

            requestedRows =
                Random.Range(
                    minimumRows,
                    maximumRows + 1
                );
        }
        else
        {
            bottomCount =
                bottomRowCanCount;

            requestedRows =
                numberOfRows;
        }

        bottomCount =
            Mathf.Max(
                2,
                bottomCount
            );

        int rowCount =
            Mathf.Clamp(
                requestedRows,
                1,
                bottomCount
            );

        SimpleTowerPattern selectedPattern =
            SimpleTowerPattern.ClassicPyramid;

        if (randomizePatternEachLevel)
        {
            int patternCount =
                System.Enum.GetValues(
                    typeof(SimpleTowerPattern)
                ).Length;

            selectedPattern =
                (SimpleTowerPattern)Random.Range(
                    0,
                    patternCount
                );

            if (avoidImmediatePatternRepeat &&
                patternCount > 1 &&
                selectedPattern == lastPattern)
            {
                selectedPattern =
                    (SimpleTowerPattern)(
                        ((int)selectedPattern + 1) %
                        patternCount
                    );
            }
        }

        lastPattern =
            selectedPattern;

        for (int row = 0;
             row < rowCount;
             row++)
        {
            int cansInRow;

            switch (selectedPattern)
            {
                case SimpleTowerPattern.DoubleBase:
                    cansInRow =
                        row <= 1
                            ? bottomCount
                            : bottomCount - row + 1;
                    break;

                case SimpleTowerPattern.PairedRows:
                    cansInRow =
                        bottomCount -
                        (row / 2);
                    break;

                case SimpleTowerPattern.FlatTop:
                    cansInRow =
                        bottomCount -
                        row;

                    if (row ==
                            rowCount - 1 &&
                        activeRowPattern.Count > 0)
                    {
                        cansInRow =
                            activeRowPattern[
                                activeRowPattern.Count - 1
                            ];
                    }
                    break;

                default:
                    cansInRow =
                        bottomCount -
                        row;
                    break;
            }

            cansInRow =
                Mathf.Max(
                    1,
                    cansInRow
                );

            /*
             * Upper row lower row se bari nahi hogi.
             */
            if (activeRowPattern.Count > 0)
            {
                cansInRow =
                    Mathf.Min(
                        cansInRow,
                        activeRowPattern[
                            activeRowPattern.Count - 1
                        ]
                    );
            }

            activeRowPattern.Add(
                cansInRow
            );
        }
    }

    private string GetActivePatternText()
    {
        if (activeRowPattern.Count == 0)
        {
            return "None";
        }

        return string.Join(
            "-",
            activeRowPattern.ConvertAll(
                value => value.ToString()
            ).ToArray()
        );
    }

    private void SpawnCanPyramid(
        Vector3 pyramidBaseCenter,
        Quaternion levelRotation)
    {
        spawnedCans.Clear();
        cansByRow.Clear();
        cansByRigidbody.Clear();

        float horizontalStep =
            canDiameter +
            horizontalGap;

        float verticalStep =
            canHeight +
            verticalGap;

        Vector3 levelRight =
            levelRotation *
            Vector3.right;

        for (int row = 0;
             row < activeRowPattern.Count;
             row++)
        {
            int cansInRow =
                activeRowPattern[row];

            List<CanRuntimeData> currentRow =
                new List<CanRuntimeData>(
                    cansInRow
                );

            cansByRow.Add(
                currentRow
            );

            float rowCenterOffset =
                (cansInRow - 1) *
                0.5f;

            for (int index = 0;
                 index < cansInRow;
                 index++)
            {
                float horizontalOffset =
                    (index - rowCenterOffset) *
                    horizontalStep;

                float verticalOffset =
                    canHeight * 0.5f +
                    row * verticalStep;

                Vector3 canCenter =
                    pyramidBaseCenter +
                    levelRight * horizontalOffset +
                    Vector3.up * verticalOffset;

                CanRuntimeData canData =
                    SpawnCan(
                        canCenter,
                        levelRotation,
                        row,
                        index
                    );

                if (canData != null)
                {
                    currentRow.Add(
                        canData
                    );
                }
            }
        }
    }

    private CanRuntimeData SpawnCan(
        Vector3 requiredCenter,
        Quaternion levelRotation,
        int row,
        int index)
    {
        GameObject canRoot =
            new GameObject(
                $"TowerCan_Row_{row + 1}_Can_{index + 1}"
            );

        canRoot.transform.SetParent(
            generatedLevelRoot,
            false
        );

        canRoot.transform.SetPositionAndRotation(
            requiredCenter,
            levelRotation
        );

        canRoot.layer =
            Mathf.Clamp(
                generatedCanLayer,
                0,
                31
            );

        BoxCollider boxCollider =
            canRoot.AddComponent<
                BoxCollider>();

        boxCollider.center =
            Vector3.zero;

        boxCollider.size =
            new Vector3(
                canDiameter *
                colliderDiameterMultiplier,
                canHeight,
                canDiameter *
                colliderDiameterMultiplier
            );

        Rigidbody canBody =
            canRoot.AddComponent<
                Rigidbody>();

        canBody.mass =
            canMass;

        canBody.useGravity =
            false;

        canBody.isKinematic =
            true;

        canBody.detectCollisions =
            true;

        canBody.interpolation =
            RigidbodyInterpolation.Interpolate;

        canBody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;

        SetBodyVelocity(
            canBody,
            Vector3.zero
        );

        canBody.angularVelocity =
            Vector3.zero;

        GameObject visualContainer =
            new GameObject(
                "Visual"
            );

        visualContainer.transform.SetParent(
            canRoot.transform,
            false
        );

        visualContainer.transform.localPosition =
            Vector3.zero;

        visualContainer.transform.localRotation =
            Quaternion.identity;

        visualContainer.transform.localScale =
            Vector3.one;

        GameObject canVisual =
            Instantiate(
                canPrefab,
                visualContainer.transform
            );

        canVisual.name =
            "CanVisual";

        PrepareVisualPrefab(
            canVisual
        );

        if (automaticallyFitCanVisual)
        {
            if (!FitVisualInsideCanRoot(
                    canVisual,
                    visualContainer.transform))
            {
                Debug.LogWarning(
                    "Can visual automatically fit nahi ho saka. " +
                    "Can prefab par enabled Renderer hona chahiye.",
                    canVisual
                );
            }
        }
        else
        {
            canVisual.transform.localPosition =
                Vector3.zero;

            canVisual.transform.localRotation =
                Quaternion.identity;

            canVisual.transform.localScale =
                Vector3.one;
        }

        Collider[] generatedColliders =
            canRoot.GetComponentsInChildren<
                Collider>(true);

        CanRuntimeData canData =
            new CanRuntimeData
            {
                Root = canRoot,
                Body = canBody,
                Colliders = generatedColliders,

                GenerationId =
                    currentGenerationId,

                Row = row,
                Index = index,

                InitialPosition =
                    canBody.position,

                InitialUp =
                    canBody.transform.up,

                IsDynamic = false,
                DestroyScheduled = false,

                UnsupportedFrames = 0,
                DynamicStartTime = -1f
            };

        spawnedCans.Add(
            canData
        );

        cansByRigidbody[canBody] =
            canData;

        GeneratedTowerCanHitReceiver hitReceiver =
            canRoot.AddComponent<
                GeneratedTowerCanHitReceiver>();

        hitReceiver.Initialize(
            this,
            canBody
        );

        return canData;
    }

    private void PrepareVisualPrefab(
        GameObject visualInstance)
    {
        Collider[] visualColliders =
            visualInstance.GetComponentsInChildren<
                Collider>(true);

        foreach (Collider visualCollider
                 in visualColliders)
        {
            if (visualCollider != null)
            {
                visualCollider.enabled =
                    false;
            }
        }

        Rigidbody[] visualBodies =
            visualInstance.GetComponentsInChildren<
                Rigidbody>(true);

        foreach (Rigidbody visualBody
                 in visualBodies)
        {
            if (visualBody == null)
            {
                continue;
            }

            visualBody.detectCollisions =
                false;

            visualBody.isKinematic =
                true;

            Destroy(
                visualBody
            );
        }
    }

    private bool FitVisualInsideCanRoot(
        GameObject visualInstance,
        Transform visualContainer)
    {
        Transform visualTransform =
            visualInstance.transform;

        visualTransform.localPosition =
            Vector3.zero;

        visualTransform.localRotation =
            Quaternion.identity;

        visualTransform.localScale =
            Vector3.one;

        visualContainer.localPosition =
            Vector3.zero;

        visualContainer.localRotation =
            Quaternion.identity;

        visualContainer.localScale =
            Vector3.one;

        Physics.SyncTransforms();

        if (!TryGetRendererBounds(
                visualInstance,
                out Bounds originalBounds))
        {
            return false;
        }

        Vector3 originalSize =
            originalBounds.size;

        Quaternion orientationCorrection =
            Quaternion.identity;

        /*
         * Visual ka longest axis Y par rotate karo.
         */
        if (originalSize.x >
            Mathf.Max(
                originalSize.y,
                originalSize.z
            ))
        {
            orientationCorrection =
                Quaternion.Euler(
                    0f,
                    0f,
                    90f
                );
        }
        else if (originalSize.z >
                 Mathf.Max(
                     originalSize.x,
                     originalSize.y
                 ))
        {
            orientationCorrection =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f
                );
        }

        visualTransform.localRotation =
            orientationCorrection;

        Physics.SyncTransforms();

        if (!TryGetRendererBounds(
                visualInstance,
                out Bounds uprightBounds))
        {
            return false;
        }

        Vector3 uprightSize =
            uprightBounds.size;

        if (uprightSize.x <= 0.0001f ||
            uprightSize.y <= 0.0001f ||
            uprightSize.z <= 0.0001f)
        {
            return false;
        }

        visualContainer.localScale =
            new Vector3(
                canDiameter /
                uprightSize.x,

                canHeight /
                uprightSize.y,

                canDiameter /
                uprightSize.z
            );

        Physics.SyncTransforms();

        if (!TryGetRendererBounds(
                visualInstance,
                out Bounds resizedBounds))
        {
            return false;
        }

        /*
         * Imported pivot center par na ho to visual ko center karo.
         */
        Vector3 centerCorrection =
            visualContainer.position -
            resizedBounds.center;

        visualContainer.position +=
            centerCorrection;

        Physics.SyncTransforms();

        return true;
    }

    private void BuildSupportLinks()
    {
        for (int row = 0;
             row < cansByRow.Count;
             row++)
        {
            List<CanRuntimeData> currentRow =
                cansByRow[row];

            for (int index = 0;
                 index < currentRow.Count;
                 index++)
            {
                CanRuntimeData canData =
                    currentRow[index];

                canData.Supports.Clear();

                /*
                 * Bottom row ko table support karti hai.
                 */
                if (row == 0)
                {
                    continue;
                }

                List<CanRuntimeData> lowerRow =
                    cansByRow[row - 1];

                CanRuntimeData nearestSupport =
                    null;

                CanRuntimeData secondSupport =
                    null;

                float nearestDistance =
                    float.MaxValue;

                float secondDistance =
                    float.MaxValue;

                foreach (CanRuntimeData lowerCan
                         in lowerRow)
                {
                    if (lowerCan == null ||
                        lowerCan.Body == null)
                    {
                        continue;
                    }

                    Vector3 difference =
                        lowerCan.InitialPosition -
                        canData.InitialPosition;

                    difference.y =
                        0f;

                    float distance =
                        difference.sqrMagnitude;

                    if (distance <
                        nearestDistance)
                    {
                        secondDistance =
                            nearestDistance;

                        secondSupport =
                            nearestSupport;

                        nearestDistance =
                            distance;

                        nearestSupport =
                            lowerCan;
                    }
                    else if (distance <
                             secondDistance)
                    {
                        secondDistance =
                            distance;

                        secondSupport =
                            lowerCan;
                    }
                }

                if (nearestSupport != null)
                {
                    canData.Supports.Add(
                        nearestSupport
                    );
                }

                if (secondSupport != null &&
                    secondSupport != nearestSupport)
                {
                    canData.Supports.Add(
                        secondSupport
                    );
                }
            }
        }
    }

    private void LockAllCans()
    {
        hasActiveCans =
            false;

        foreach (CanRuntimeData canData
                 in spawnedCans)
        {
            if (canData == null ||
                canData.Body == null)
            {
                continue;
            }

            Rigidbody body =
                canData.Body;

            SetBodyVelocity(
                body,
                Vector3.zero
            );

            body.angularVelocity =
                Vector3.zero;

            body.useGravity =
                !lockCansOnSpawn;

            body.isKinematic =
                lockCansOnSpawn;

            canData.IsDynamic =
                !lockCansOnSpawn;

            canData.UnsupportedFrames =
                0;

            canData.DynamicStartTime =
                canData.IsDynamic
                    ? Time.unscaledTime
                    : -1f;
        }

        Physics.SyncTransforms();

        if (!lockCansOnSpawn)
        {
            hasActiveCans =
                true;
        }
    }

    public bool IsCannonBallCollision(
        Collision collision)
    {
        if (collision == null ||
            collision.collider == null)
        {
            return false;
        }

        int colliderLayer =
            collision.collider.gameObject.layer;

        if (IsLayerInsideMask(
                colliderLayer,
                cannonBallLayers))
        {
            return true;
        }

        if (collision.rigidbody != null)
        {
            int bodyLayer =
                collision.rigidbody.gameObject.layer;

            return IsLayerInsideMask(
                bodyLayer,
                cannonBallLayers
            );
        }

        return false;
    }

    public void HandleCanHit(
        Rigidbody canBody,
        Collision collision)
    {
        if (canBody == null ||
            collision == null)
        {
            return;
        }

        if (!cansByRigidbody.TryGetValue(
                canBody,
                out CanRuntimeData canData))
        {
            return;
        }

        /*
         * Dynamic can ko normal physics handle karegi.
         */
        if (canData.IsDynamic)
        {
            return;
        }

        Vector3 incomingVelocity =
            collision.relativeVelocity;

        float incomingMass =
            1f;

        if (collision.rigidbody != null)
        {
            incomingVelocity =
                GetBodyVelocity(
                    collision.rigidbody
                );

            incomingMass =
                Mathf.Max(
                    0.01f,
                    collision.rigidbody.mass
                );
        }

        Vector3 impactPoint =
            canBody.worldCenterOfMass;

        if (collision.contactCount > 0)
        {
            impactPoint =
                collision.GetContact(0).point;
        }

        ActivateCan(
            canData,
            incomingVelocity,
            incomingMass,
            impactPoint,
            true
        );
    }

    private void ActivateCan(
        CanRuntimeData canData,
        Vector3 incomingVelocity,
        float incomingMass,
        Vector3 impactPoint,
        bool applyImpact)
    {
        if (canData == null ||
            canData.Body == null ||
            canData.IsDynamic)
        {
            return;
        }

        Rigidbody body =
            canData.Body;

        SetBodyVelocity(
            body,
            Vector3.zero
        );

        body.angularVelocity =
            Vector3.zero;

        body.isKinematic =
            false;

        body.useGravity =
            true;

        body.WakeUp();

        canData.IsDynamic =
            true;

        canData.UnsupportedFrames =
            0;

        canData.DynamicStartTime =
            Time.unscaledTime;

        hasActiveCans =
            true;

        if (!applyImpact ||
            incomingVelocity.sqrMagnitude <
            0.001f)
        {
            return;
        }

        Vector3 impulse =
            incomingVelocity *
            incomingMass *
            impactTransferMultiplier;

        impulse =
            Vector3.ClampMagnitude(
                impulse,
                maximumImpactImpulse
            );

        body.AddForceAtPosition(
            impulse,
            impactPoint,
            ForceMode.Impulse
        );
    }

    private void UpdateCanSupportChain()
    {
        foreach (CanRuntimeData canData
                 in spawnedCans)
        {
            if (canData == null ||
                canData.Root == null ||
                canData.Body == null ||
                canData.IsDynamic ||
                canData.Row == 0)
            {
                continue;
            }

            int requiredSupportCount =
                Mathf.Min(
                    minimumValidSupports,
                    canData.Supports.Count
                );

            if (requiredSupportCount <= 0)
            {
                continue;
            }

            int validSupportCount =
                0;

            foreach (CanRuntimeData support
                     in canData.Supports)
            {
                if (IsSupportValid(
                        support))
                {
                    validSupportCount++;
                }
            }

            if (validSupportCount >=
                requiredSupportCount)
            {
                canData.UnsupportedFrames =
                    0;

                continue;
            }

            canData.UnsupportedFrames++;

            if (canData.UnsupportedFrames <
                unsupportedFramesRequired)
            {
                continue;
            }

            ActivateCan(
                canData,
                Vector3.zero,
                1f,
                canData.Body.worldCenterOfMass,
                false
            );
        }
    }

    private bool IsSupportValid(
        CanRuntimeData support)
    {
        if (support == null ||
            support.Root == null ||
            support.Body == null ||
            support.DestroyScheduled)
        {
            return false;
        }

        /*
         * Locked support valid hai.
         */
        if (!support.IsDynamic)
        {
            return true;
        }

        float movement =
            Vector3.Distance(
                support.Body.position,
                support.InitialPosition
            );

        float tilt =
            Vector3.Angle(
                support.Body.transform.up,
                support.InitialUp
            );

        return
            movement <=
            supportMovementTolerance &&
            tilt <=
            supportTiltTolerance;
    }

    private void UpdateFallenCanCleanup()
    {
        if (!destroyFallenCans)
        {
            return;
        }

        foreach (CanRuntimeData canData
                 in spawnedCans)
        {
            if (canData == null ||
                canData.Root == null ||
                canData.Body == null ||
                !canData.IsDynamic ||
                canData.DestroyScheduled)
            {
                continue;
            }

            if (!IsCanFallen(
                    canData))
            {
                continue;
            }

            ScheduleCanDestruction(
                canData
            );
        }
    }

    private bool IsCanFallen(
        CanRuntimeData canData)
    {
        if (!TryGetCanBounds(
                canData,
                out Bounds bounds))
        {
            return false;
        }

        bool belowTable =
            bounds.center.y <=
            tableTopY -
            fallenBelowTableDistance;

        Vector3 movement =
            canData.Body.position -
            canData.InitialPosition;

        float movementDistance =
            movement.magnitude;

        float horizontalMovement =
            new Vector2(
                movement.x,
                movement.z
            ).magnitude;

        float tiltAngle =
            Vector3.Angle(
                canData.Body.transform.up,
                canData.InitialUp
            );

        bool tippedOver =
            movementDistance >=
            minimumMovementBeforeFall &&
            tiltAngle >=
            fallenTiltAngle;

        bool movedOutOfStack =
            fallenHorizontalDistance > 0f &&
            horizontalMovement >=
            fallenHorizontalDistance;

        bool exceededDynamicLifetime =
            maximumDynamicLifetime > 0f &&
            canData.DynamicStartTime >= 0f &&
            Time.unscaledTime -
            canData.DynamicStartTime >=
            maximumDynamicLifetime;

        return
            belowTable ||
            tippedOver ||
            movedOutOfStack ||
            exceededDynamicLifetime;
    }

    private void ScheduleCanDestruction(
        CanRuntimeData canData)
    {
        if (canData == null ||
            canData.DestroyScheduled)
        {
            return;
        }

        canData.DestroyScheduled =
            true;

        StartCoroutine(
            DestroyCanAfterDelayRoutine(
                canData,
                fallenCanDestroyDelay
            )
        );
    }

    private IEnumerator DestroyCanAfterDelayRoutine(
        CanRuntimeData canData,
        float delay)
    {
        if (delay > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    delay
                );
        }

        /*
         * Old level ki delayed coroutine new level ko affect nahi karegi.
         */
        if (canData == null ||
            canData.GenerationId !=
            currentGenerationId)
        {
            if (canData != null &&
                canData.Root != null)
            {
                Destroy(
                    canData.Root
                );
            }

            yield break;
        }

        if (canData.Root != null)
        {
            Destroy(
                canData.Root
            );
        }

        UnregisterCan(
            canData
        );

        CheckForLevelCompletion();
    }

    private void UnregisterCan(
        CanRuntimeData canData)
    {
        if (canData == null)
        {
            return;
        }

        if (canData.Body != null)
        {
            cansByRigidbody.Remove(
                canData.Body
            );
        }

        spawnedCans.Remove(
            canData
        );
    }

    private void RemoveDestroyedCanReferences()
    {
        for (int i =
                 spawnedCans.Count - 1;
             i >= 0;
             i--)
        {
            CanRuntimeData canData =
                spawnedCans[i];

            if (canData != null &&
                canData.Root != null &&
                canData.Body != null)
            {
                continue;
            }

            if (canData != null &&
                canData.Body != null)
            {
                cansByRigidbody.Remove(
                    canData.Body
                );
            }

            spawnedCans.RemoveAt(
                i
            );
        }
    }

    private void CheckForLevelCompletion()
    {
        if (!autoLoadNextLevel ||
            !levelWasGenerated ||
            nextLevelQueued ||
            generationCoroutine != null)
        {
            return;
        }

        if (spawnedCans.Count > 0)
        {
            return;
        }

        nextLevelQueued =
            true;

        if (showDebugLogs)
        {
            Debug.Log(
                $"Level {CurrentLevel} complete. " +
                $"Next level {nextLevelDelay:0.00}s mein load hoga.",
                this
            );
        }

        nextLevelCoroutine =
            StartCoroutine(
                LoadNextLevelRoutine()
            );
    }

    private IEnumerator LoadNextLevelRoutine()
    {
        if (nextLevelDelay > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    nextLevelDelay
                );
        }

        CurrentLevel++;

        /*
         * GenerateLevel ke andar cancellation se
         * current coroutine ko stop hone se bachao.
         */
        nextLevelCoroutine =
            null;

        GenerateLevel();
    }

    private void CancelPendingNextLevel()
    {
        if (nextLevelCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            nextLevelCoroutine
        );

        nextLevelCoroutine =
            null;

        nextLevelQueued =
            false;
    }

    private bool TryGetCanBounds(
        CanRuntimeData canData,
        out Bounds combinedBounds)
    {
        combinedBounds =
            default;

        if (canData == null ||
            canData.Colliders == null)
        {
            return false;
        }

        bool hasBounds =
            false;

        foreach (Collider canCollider
                 in canData.Colliders)
        {
            if (canCollider == null ||
                !canCollider.enabled ||
                canCollider.isTrigger)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds =
                    canCollider.bounds;

                hasBounds =
                    true;
            }
            else
            {
                combinedBounds.Encapsulate(
                    canCollider.bounds
                );
            }
        }

        return hasBounds;
    }

    private bool TryGetRendererBounds(
        GameObject target,
        out Bounds combinedBounds)
    {
        combinedBounds =
            default;

        Renderer[] renderers =
            target.GetComponentsInChildren<
                Renderer>(true);

        bool hasBounds =
            false;

        foreach (Renderer targetRenderer
                 in renderers)
        {
            if (targetRenderer == null ||
                !targetRenderer.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds =
                    targetRenderer.bounds;

                hasBounds =
                    true;
            }
            else
            {
                combinedBounds.Encapsulate(
                    targetRenderer.bounds
                );
            }
        }

        return hasBounds;
    }

    private bool TryGetCombinedBounds(
        GameObject target,
        out Bounds combinedBounds)
    {
        combinedBounds =
            default;

        Collider[] colliders =
            target.GetComponentsInChildren<
                Collider>(true);

        bool hasBounds =
            false;

        foreach (Collider targetCollider
                 in colliders)
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

        if (hasBounds)
        {
            return true;
        }

        return TryGetRendererBounds(
            target,
            out combinedBounds
        );
    }

    private static bool IsLayerInsideMask(
        int layer,
        LayerMask layerMask)
    {
        return (
            layerMask.value &
            (1 << layer)
        ) != 0;
    }

    private static Vector3 GetBodyVelocity(
        Rigidbody body)
    {
        if (body == null)
        {
            return Vector3.zero;
        }

#if UNITY_6000_0_OR_NEWER
        return body.linearVelocity;
#else
        return body.velocity;
#endif
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

    [ContextMenu("Clear Generated Level")]
    public void ClearGeneratedLevel()
    {
        hasActiveCans =
            false;

        levelWasGenerated =
            false;

        spawnedCans.Clear();
        cansByRow.Clear();
        cansByRigidbody.Clear();
        activeRowPattern.Clear();

        if (generatedLevelRoot == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(
                generatedLevelRoot.gameObject
            );
        }
        else
        {
            DestroyImmediate(
                generatedLevelRoot.gameObject
            );
        }

        generatedLevelRoot =
            null;
    }

    private bool ValidateReferences()
    {
        if (tablePrefab == null)
        {
            Debug.LogError(
                "Table Prefab assign nahi hai.",
                this
            );

            return false;
        }

        if (canPrefab == null)
        {
            Debug.LogError(
                "Can Prefab assign nahi hai.",
                this
            );

            return false;
        }

        if (canDiameter <= 0f ||
            canHeight <= 0f)
        {
            Debug.LogError(
                "Can Diameter aur Can Height 0 se bari honi chahiye.",
                this
            );

            return false;
        }

        if (cannonBallLayers.value == 0)
        {
            Debug.LogError(
                "Cannon Ball Layers mein CannonBall/Projectile layer select karein.",
                this
            );

            return false;
        }

        if (cannonBallLayers.value == ~0)
        {
            Debug.LogWarning(
                "Cannon Ball Layers Everything par hai. " +
                "Sirf CannonBall layer select karein.",
                this
            );
        }

        return true;
    }
}

/*
 * Isi TowerLevelGenerator.cs file mein rehne dein.
 * Can prefab par manually attach karne ki zaroorat nahi.
 */
public sealed class GeneratedTowerCanHitReceiver :
    MonoBehaviour
{
    private TowerLevelGenerator generator;
    private Rigidbody canBody;

    public void Initialize(
        TowerLevelGenerator owner,
        Rigidbody body)
    {
        generator =
            owner;

        canBody =
            body;
    }

    private void OnCollisionEnter(
        Collision collision)
    {
        if (generator == null ||
            canBody == null ||
            collision == null)
        {
            return;
        }

        if (!generator.IsCannonBallCollision(
                collision))
        {
            return;
        }

        generator.HandleCanHit(
            canBody,
            collision
        );
    }
}
