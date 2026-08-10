//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;

//public sealed class LevelRuntimeController : MonoBehaviour
//{
//    [Header("Level")]

//    [SerializeField]
//    private LevelData levelData;

//    [Header("References")]

//    [SerializeField]
//    private PhysicsObjectPool objectPool;

//    [Tooltip(
//        "Complete level ka spawn origin. " +
//        "Empty ho to controller ka Transform use hoga."
//    )]
//    [SerializeField]
//    private Transform levelOrigin;

//    [Tooltip(
//        "Active spawned physics objects is root ke andar rahenge."
//    )]
//    [SerializeField]
//    private Transform runtimeObjectsRoot;

//    [Header("Events")]

//    [SerializeField]
//    private UnityEvent onLevelGenerated;

//    [SerializeField]
//    private UnityEvent onLevelComplete;

//    private readonly List<PhysicsTowerObject>
//        activeObjects =
//            new List<PhysicsTowerObject>();

//    private LevelTable cachedTable;

//    private LevelTable cachedTablePrefab;

//    private LevelTable currentTable;

//    private int remainingTargets;

//    private bool levelGenerated;

//    public LevelData CurrentLevelData =>
//        levelData;

//    public LevelTable CurrentTable =>
//        currentTable;

//    public int RemainingTargets =>
//        remainingTargets;

//    public bool IsLevelGenerated =>
//        levelGenerated;

//    private void Start()
//    {
//        GenerateLevel();
//    }

//    public void LoadLevel(
//        LevelData newLevelData)
//    {
//        if (newLevelData == null)
//        {
//            Debug.LogError(
//                "LoadLevel: LevelData null hai.",
//                this
//            );

//            return;
//        }

//        levelData =
//            newLevelData;

//        GenerateLevel();
//    }

//    [ContextMenu("Generate Level")]
//    public void GenerateLevel()
//    {
//        if (!ValidateReferences())
//        {
//            return;
//        }

//        ClearCurrentLevel();

//        objectPool.PrepareForLevel(
//            levelData
//        );

//        if (!PrepareTable())
//        {
//            return;
//        }

//        if (!currentTable.TryGetTowerSurface(
//                out Vector3 towerSurfacePosition,
//                out Quaternion towerSurfaceRotation))
//        {
//            Debug.LogError(
//                "Table ki top surface calculate nahi ho saki.",
//                currentTable
//            );

//            return;
//        }

//        remainingTargets =
//            0;

//        GenerateTower(
//            towerSurfacePosition,
//            towerSurfaceRotation
//        );

//        Physics.SyncTransforms();

//        levelGenerated =
//            true;

//        Debug.Log(
//            $"Level {levelData.LevelNumber} generated. " +
//            $"Objects: {activeObjects.Count}, " +
//            $"Targets: {remainingTargets}",
//            this
//        );

//        onLevelGenerated?.Invoke();

//        if (remainingTargets <= 0)
//        {
//            Debug.LogWarning(
//                $"Level {levelData.LevelNumber} mein " +
//                "koi target generate nahi hua.",
//                this
//            );
//        }
//    }

//    private bool PrepareTable()
//    {
//        LevelTable requiredPrefab =
//            levelData.TablePrefab;

//        if (requiredPrefab == null)
//        {
//            Debug.LogError(
//                $"Level {levelData.LevelNumber}: " +
//                "Table Prefab missing hai.",
//                levelData
//            );

//            return false;
//        }

//        /*
//         * Same table prefab ho to existing cached
//         * instance reuse hogi.
//         */
//        if (cachedTable == null ||
//            cachedTablePrefab != requiredPrefab)
//        {
//            if (cachedTable != null)
//            {
//                Destroy(
//                    cachedTable.gameObject
//                );

//                cachedTable =
//                    null;
//            }

//            Transform origin =
//                GetLevelOrigin();

//            cachedTable =
//                Instantiate(
//                    requiredPrefab,
//                    origin
//                );

//            cachedTable.name =
//                requiredPrefab.name +
//                "_Runtime";

//            cachedTablePrefab =
//                requiredPrefab;
//        }

//        currentTable =
//            cachedTable;

//        Transform levelRoot =
//            GetLevelOrigin();

//        Transform tableTransform =
//            currentTable.transform;

//        if (tableTransform.parent !=
//            levelRoot)
//        {
//            tableTransform.SetParent(
//                levelRoot,
//                false
//            );
//        }

//        /*
//         * Table placement tower placement se
//         * completely separate hai.
//         */
//        tableTransform.localPosition =
//            levelData.TablePositionOffset;

//        tableTransform.localRotation =
//            Quaternion.Euler(
//                levelData.TableRotationEuler
//            );

//        tableTransform.localScale =
//            requiredPrefab.transform.localScale;

//        currentTable.gameObject.SetActive(
//            true
//        );

//        if (currentTable.TowerSurfaceCollider ==
//            null)
//        {
//            Debug.LogError(
//                "LevelTable par Tower Surface Collider missing hai.",
//                currentTable
//            );

//            return false;
//        }

//        return true;
//    }

//    /// <summary>
//    /// Actual prefab collider dimensions ke according
//    /// tower generate karta hai.
//    ///
//    /// Placement Size manually enter karne ki
//    /// zaroorat nahi.
//    /// </summary>
//    private void GenerateTower(
//        Vector3 towerSurfacePosition,
//        Quaternion towerSurfaceRotation)
//    {
//        IReadOnlyList<LevelRowData> rows =
//            levelData.Rows;

//        if (rows == null ||
//            rows.Count == 0)
//        {
//            Debug.LogWarning(
//                $"Level {levelData.LevelNumber}: Rows empty hain.",
//                this
//            );

//            return;
//        }

//        /*
//         * First row ka bottom table top = 0.
//         */
//        float currentRowBottom =
//            levelData.TowerOffset.y;

//        for (int rowIndex = 0;
//             rowIndex < rows.Count;
//             rowIndex++)
//        {
//            LevelRowData row =
//                rows[rowIndex];

//            if (row == null)
//            {
//                continue;
//            }

//            PhysicsObjectDefinition definition =
//                row.ObjectDefinition;

//            if (definition == null ||
//                definition.Prefab == null)
//            {
//                Debug.LogWarning(
//                    $"Level {levelData.LevelNumber}, " +
//                    $"Row {rowIndex}: Object definition/prefab missing.",
//                    this
//                );

//                continue;
//            }

//            int objectCount =
//                Mathf.Max(
//                    1,
//                    row.Count
//                );

//            Quaternion rowRotation =
//                towerSurfaceRotation *
//                Quaternion.Euler(
//                    row.RotationEuler
//                );

//            /*
//             * First object temporary surface position
//             * par spawn karo.
//             *
//             * Physics next FixedUpdate se pehle run nahi hogi,
//             * isliye measurement safe hai.
//             */
//            PhysicsTowerObject firstObject =
//                objectPool.Get(
//                    definition,
//                    towerSurfacePosition,
//                    rowRotation,
//                    runtimeObjectsRoot
//                );

//            if (firstObject == null)
//            {
//                Debug.LogWarning(
//                    $"Row {rowIndex}: First pooled object spawn failed.",
//                    this
//                );

//                continue;
//            }

//            /*
//             * Transform changes collider bounds mein
//             * immediately reflect karwane ke liye.
//             *
//             * Sirf level generation ke waqt.
//             */
//            Physics.SyncTransforms();

//            if (!firstObject.TryGetPhysicsBounds(
//                    out Bounds firstBounds))
//            {
//                Debug.LogError(
//                    $"{definition.name}: " +
//                    "Prefab par valid enabled non-trigger Collider nahi mila.",
//                    firstObject
//                );

//                objectPool.Release(
//                    firstObject
//                );

//                continue;
//            }

//            /*
//             * ACTUAL PHYSICS DIMENSIONS.
//             *
//             * Ab manually PlacementSize nahi.
//             */
//            float objectWidth =
//                firstBounds.size.x;

//            float objectHeight =
//                firstBounds.size.y;

//            if (objectWidth <= 0.0001f ||
//                objectHeight <= 0.0001f)
//            {
//                Debug.LogError(
//                    $"{definition.name}: Collider bounds invalid hain.",
//                    firstObject
//                );

//                objectPool.Release(
//                    firstObject
//                );

//                continue;
//            }

//            /*
//             * Prefab pivot collider center par hona
//             * zaroori nahi.
//             *
//             * Ye offset us problem ko automatically
//             * compensate karta hai.
//             */
//            Vector3 boundsCenterOffset =
//                firstBounds.center -
//                firstObject.transform.position;

//            float horizontalStep =
//                objectWidth +
//                levelData.HorizontalGap;

//            float rowWidth =
//                (objectCount - 1) *
//                horizontalStep;

//            float startX =
//                -(rowWidth * 0.5f) +
//                levelData.TowerOffset.x +
//                row.CenterOffsetX;

//            /*
//             * Bottom row:
//             *
//             * table surface + half actual collider height
//             */
//            float centerY =
//                currentRowBottom +
//                objectHeight * 0.5f;

//            float localZ =
//                levelData.TowerOffset.z +
//                row.DepthOffset;

//            for (int objectIndex = 0;
//                 objectIndex < objectCount;
//                 objectIndex++)
//            {
//                PhysicsTowerObject instance;

//                if (objectIndex == 0)
//                {
//                    instance =
//                        firstObject;
//                }
//                else
//                {
//                    instance =
//                        objectPool.Get(
//                            definition,
//                            towerSurfacePosition,
//                            rowRotation,
//                            runtimeObjectsRoot
//                        );
//                }

//                if (instance == null)
//                {
//                    continue;
//                }

//                float localX =
//                    startX +
//                    objectIndex *
//                    horizontalStep;

//                /*
//                 * Desired collider BOUNDS center.
//                 */
//                Vector3 localBoundsCenter =
//                    new Vector3(
//                        localX,
//                        centerY,
//                        localZ
//                    );

//                Vector3 desiredBoundsCenter =
//                    towerSurfacePosition +
//                    towerSurfaceRotation *
//                    localBoundsCenter;

//                /*
//                 * Actual prefab pivot may not be centered.
//                 * Bounds-center offset automatically compensate.
//                 */
//                Vector3 desiredTransformPosition =
//                    desiredBoundsCenter -
//                    boundsCenterOffset;

//                instance.transform.SetPositionAndRotation(
//                    desiredTransformPosition,
//                    rowRotation
//                );

//                instance.Cleared +=
//                    HandleObjectCleared;

//                activeObjects.Add(
//                    instance
//                );

//                if (instance.CountsAsTarget)
//                {
//                    remainingTargets++;
//                }
//            }

//            /*
//             * Poore row ke transform updates
//             * physics engine ko sync.
//             */
//            Physics.SyncTransforms();

//            /*
//             * Next row actual collider height
//             * ke exactly upar.
//             */
//            currentRowBottom +=
//                objectHeight +
//                levelData.VerticalGap;
//        }
//    }

//    private void HandleObjectCleared(
//        PhysicsTowerObject target)
//    {
//        if (target == null)
//        {
//            return;
//        }

//        target.Cleared -=
//            HandleObjectCleared;

//        activeObjects.Remove(
//            target
//        );

//        if (target.CountsAsTarget)
//        {
//            remainingTargets =
//                Mathf.Max(
//                    0,
//                    remainingTargets - 1
//                );
//        }

//        objectPool.Release(
//            target
//        );

//        Debug.Log(
//            $"Target cleared. Remaining: {remainingTargets}",
//            this
//        );

//        if (levelGenerated &&
//            remainingTargets == 0)
//        {
//            CompleteLevel();
//        }
//    }

//    private void CompleteLevel()
//    {
//        if (!levelGenerated)
//        {
//            return;
//        }

//        levelGenerated =
//            false;

//        Debug.Log(
//            $"LEVEL {levelData.LevelNumber} COMPLETE",
//            this
//        );

//        onLevelComplete?.Invoke();
//    }

//    public void ClearCurrentLevel()
//    {
//        levelGenerated =
//            false;

//        for (int i =
//                 activeObjects.Count - 1;
//             i >= 0;
//             i--)
//        {
//            PhysicsTowerObject instance =
//                activeObjects[i];

//            if (instance == null)
//            {
//                continue;
//            }

//            instance.Cleared -=
//                HandleObjectCleared;

//            if (objectPool != null)
//            {
//                objectPool.Release(
//                    instance
//                );
//            }
//        }

//        activeObjects.Clear();

//        remainingTargets =
//            0;

//        if (currentTable != null)
//        {
//            currentTable.gameObject.SetActive(
//                false
//            );
//        }

//        currentTable =
//            null;
//    }

//    private Transform GetLevelOrigin()
//    {
//        return levelOrigin != null
//            ? levelOrigin
//            : transform;
//    }

//    private bool ValidateReferences()
//    {
//        if (levelData == null)
//        {
//            Debug.LogError(
//                "Level Data missing hai.",
//                this
//            );

//            return false;
//        }

//        if (objectPool == null)
//        {
//            Debug.LogError(
//                "Physics Object Pool missing hai.",
//                this
//            );

//            return false;
//        }

//        if (runtimeObjectsRoot == null)
//        {
//            Debug.LogError(
//                "Runtime Objects Root missing hai.",
//                this
//            );

//            return false;
//        }

//        if (levelData.TablePrefab == null)
//        {
//            Debug.LogError(
//                "LevelData mein Table Prefab missing hai.",
//                levelData
//            );

//            return false;
//        }

//        /*
//         * Runtime root scale physics dimensions ko
//         * unexpectedly distort na kare.
//         */
//        Vector3 scale =
//            runtimeObjectsRoot.lossyScale;

//        if (Mathf.Abs(scale.x - 1f) > 0.01f ||
//            Mathf.Abs(scale.y - 1f) > 0.01f ||
//            Mathf.Abs(scale.z - 1f) > 0.01f)
//        {
//            Debug.LogWarning(
//                "RuntimeObjects Root ki world scale 1,1,1 honi chahiye.",
//                runtimeObjectsRoot
//            );
//        }

//        return true;
//    }

//    private void OnDestroy()
//    {
//        for (int i = 0;
//             i < activeObjects.Count;
//             i++)
//        {
//            PhysicsTowerObject instance =
//                activeObjects[i];

//            if (instance != null)
//            {
//                instance.Cleared -=
//                    HandleObjectCleared;
//            }
//        }
//    }
//}











using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class LevelRuntimeController : MonoBehaviour
{
    [Header("Level")]

    [SerializeField]
    private GridLevelData levelData;


    [Header("References")]

    [SerializeField]
    private PhysicsObjectPool objectPool;

    [SerializeField]
    private Transform levelOrigin;

    [SerializeField]
    private Transform runtimeObjectsRoot;


    [Header("Events")]

    [SerializeField]
    private UnityEvent onLevelGenerated;

    [SerializeField]
    private UnityEvent onLevelComplete;


    private readonly List<PhysicsTowerObject>
        activeObjects =
            new List<PhysicsTowerObject>();


    private LevelTable cachedTable;

    private LevelTable cachedTablePrefab;

    private LevelTable currentTable;


    private int remainingTargets;

    private bool levelGenerated;


    public GridLevelData CurrentLevelData =>
        levelData;

    public LevelTable CurrentTable =>
        currentTable;

    public int RemainingTargets =>
        remainingTargets;

    public bool IsLevelGenerated =>
        levelGenerated;


    private void Start()
    {
        GenerateLevel();
    }


    public void LoadLevel(
        GridLevelData newLevelData)
    {
        if (newLevelData == null)
        {
            Debug.LogError(
                "LoadLevel: GridLevelData null hai.",
                this
            );

            return;
        }


        levelData =
            newLevelData;


        GenerateLevel();
    }


    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        if (!ValidateReferences())
        {
            return;
        }


        ClearCurrentLevel();


        objectPool.PrepareForLevel(
            levelData
        );


        if (!PrepareTable())
        {
            return;
        }


        if (!currentTable.TryGetTowerSurface(
                out Vector3 surfacePosition,
                out Quaternion surfaceRotation))
        {
            Debug.LogError(
                "Table ki tower surface calculate nahi ho saki.",
                currentTable
            );

            return;
        }


        if (!MeasureGridObject(
                out Vector3 objectSize,
                out Vector3 localBoundsCenterOffset))
        {
            return;
        }


        remainingTargets =
            0;


        SpawnGrid(
            surfacePosition,
            surfaceRotation,
            objectSize,
            localBoundsCenterOffset
        );


        Physics.SyncTransforms();


        levelGenerated =
            true;


        Debug.Log(
            $"GRID LEVEL {levelData.LevelNumber} GENERATED\n" +
            $"Boxes: {activeObjects.Count}\n" +
            $"Targets: {remainingTargets}",
            this
        );


        onLevelGenerated?.Invoke();


        if (remainingTargets <= 0)
        {
            Debug.LogWarning(
                "Generated level mein koi target nahi mila.",
                this
            );
        }
    }


    private bool MeasureGridObject(
        out Vector3 objectSize,
        out Vector3 localBoundsCenterOffset)
    {
        objectSize =
            Vector3.zero;


        localBoundsCenterOffset =
            Vector3.zero;


        PhysicsObjectDefinition definition =
            levelData.ObjectDefinition;


        /*
         * Temporary measurement object.
         *
         * Identity rotation par measure karte hain
         * taake X/Y/Z collider dimensions proper milen.
         */
        PhysicsTowerObject measurementObject =
            objectPool.Get(
                definition,
                Vector3.zero,
                Quaternion.identity,
                runtimeObjectsRoot
            );


        if (measurementObject == null)
        {
            Debug.LogError(
                "Grid object measure karne ke liye pool object nahi mila.",
                this
            );

            return false;
        }


        Physics.SyncTransforms();


        if (!measurementObject.TryGetPhysicsBounds(
                out Bounds bounds))
        {
            Debug.LogError(
                "Physics object par valid non-trigger Collider nahi mila.",
                measurementObject
            );


            objectPool.Release(
                measurementObject
            );


            return false;
        }


        objectSize =
            bounds.size;


        localBoundsCenterOffset =
            bounds.center -
            measurementObject.transform.position;


        objectPool.Release(
            measurementObject
        );


        if (objectSize.x <= 0.0001f ||
            objectSize.y <= 0.0001f ||
            objectSize.z <= 0.0001f)
        {
            Debug.LogError(
                "Physics object's collider size invalid hai.",
                this
            );

            return false;
        }


        return true;
    }


    private void SpawnGrid(
        Vector3 surfacePosition,
        Quaternion surfaceRotation,
        Vector3 objectSize,
        Vector3 localBoundsCenterOffset)
    {
        if (!levelData.TryGetOccupiedBounds(
                out Vector3Int occupiedMin,
                out Vector3Int occupiedMax))
        {
            Debug.LogError(
                "Baked grid mein occupied cells nahi hain.",
                levelData
            );

            return;
        }


        float stepX =
            objectSize.x +
            levelData.HorizontalGap;


        float stepY =
            objectSize.y +
            levelData.VerticalGap;


        float stepZ =
            objectSize.z +
            levelData.DepthGap;


        /*
         * Image ke transparent margins ignore hote hain.
         *
         * Occupied shape ka actual center calculate hota hai.
         */
        float occupiedCenterX =
            (
                occupiedMin.x +
                occupiedMax.x
            ) *
            0.5f;


        float occupiedCenterZ =
            (
                occupiedMin.z +
                occupiedMax.z
            ) *
            0.5f;


        PhysicsObjectDefinition definition =
            levelData.ObjectDefinition;


        /*
         * Important:
         *
         * Y outer loop hai.
         * Isliye logical spawn order bottom → top rahega.
         */
        for (int y = occupiedMin.y;
             y <= occupiedMax.y;
             y++)
        {
            for (int z = occupiedMin.z;
                 z <= occupiedMax.z;
                 z++)
            {
                for (int x = occupiedMin.x;
                     x <= occupiedMax.x;
                     x++)
                {
                    GridCellData cell =
                        levelData.GetCell(
                            x,
                            y,
                            z
                        );


                    if (cell == null ||
                        !cell.Occupied)
                    {
                        continue;
                    }


                    /*
                     * X:
                     * Shape automatically center.
                     */
                    float localX =
                        (
                            x -
                            occupiedCenterX
                        ) *
                        stepX;


                    /*
                     * Y:
                     * Lowest occupied image row
                     * directly table top par start hogi.
                     *
                     * Transparent bottom margin
                     * automatically ignore hota hai.
                     */
                    float localY =
                        objectSize.y *
                        0.5f +
                        (
                            y -
                            occupiedMin.y
                        ) *
                        stepY;


                    /*
                     * Z:
                     * Complete depth centered.
                     */
                    float localZ =
                        (
                            z -
                            occupiedCenterZ
                        ) *
                        stepZ;


                    Vector3 localCellCenter =
                        new Vector3(
                            localX,
                            localY,
                            localZ
                        )
                        +
                        levelData.GridOffset;


                    Vector3 desiredBoundsCenter =
                        surfacePosition +
                        surfaceRotation *
                        localCellCenter;


                    /*
                     * Prefab pivot center par na bhi ho
                     * to collider bottom proper align hoga.
                     */
                    Vector3 rotatedBoundsOffset =
                        surfaceRotation *
                        localBoundsCenterOffset;


                    Vector3 spawnPosition =
                        desiredBoundsCenter -
                        rotatedBoundsOffset;


                    PhysicsTowerObject instance =
                        objectPool.Get(
                            definition,
                            spawnPosition,
                            surfaceRotation,
                            runtimeObjectsRoot
                        );


                    if (instance == null)
                    {
                        continue;
                    }


                    /*
                     * Color baked data se.
                     */
                    instance.SetVisualColor(
                        cell.Color
                    );


                    instance.Cleared +=
                        HandleObjectCleared;


                    activeObjects.Add(
                        instance
                    );


                    if (instance.CountsAsTarget)
                    {
                        remainingTargets++;
                    }
                }
            }
        }
    }


    private bool PrepareTable()
    {
        LevelTable requiredPrefab =
            levelData.TablePrefab;


        if (requiredPrefab == null)
        {
            Debug.LogError(
                "GridLevelData mein Table Prefab missing hai.",
                levelData
            );

            return false;
        }


        if (cachedTable == null ||
            cachedTablePrefab != requiredPrefab)
        {
            if (cachedTable != null)
            {
                Destroy(
                    cachedTable.gameObject
                );
            }


            cachedTable =
                Instantiate(
                    requiredPrefab,
                    GetLevelOrigin()
                );


            cachedTable.name =
                requiredPrefab.name +
                "_Runtime";


            cachedTablePrefab =
                requiredPrefab;
        }


        currentTable =
            cachedTable;


        Transform tableTransform =
            currentTable.transform;


        Transform levelRoot =
            GetLevelOrigin();


        if (tableTransform.parent !=
            levelRoot)
        {
            tableTransform.SetParent(
                levelRoot,
                false
            );
        }


        tableTransform.localPosition =
            levelData.TablePositionOffset;


        tableTransform.localRotation =
            Quaternion.Euler(
                levelData.TableRotationEuler
            );


        tableTransform.localScale =
            requiredPrefab
                .transform
                .localScale;


        currentTable.gameObject.SetActive(
            true
        );


        if (currentTable.TowerSurfaceCollider ==
            null)
        {
            Debug.LogError(
                "Table prefab par Tower Surface Collider missing hai.",
                currentTable
            );

            return false;
        }


        return true;
    }


    private void HandleObjectCleared(
        PhysicsTowerObject target)
    {
        if (target == null)
        {
            return;
        }


        target.Cleared -=
            HandleObjectCleared;


        activeObjects.Remove(
            target
        );


        if (target.CountsAsTarget)
        {
            remainingTargets =
                Mathf.Max(
                    0,
                    remainingTargets - 1
                );
        }


        objectPool.Release(
            target
        );


        if (levelGenerated &&
            remainingTargets == 0)
        {
            CompleteLevel();
        }
    }


    private void CompleteLevel()
    {
        if (!levelGenerated)
        {
            return;
        }


        levelGenerated =
            false;


        Debug.Log(
            $"GRID LEVEL {levelData.LevelNumber} COMPLETE",
            this
        );


        onLevelComplete?.Invoke();
    }


    public void ClearCurrentLevel()
    {
        levelGenerated =
            false;


        for (int i =
                 activeObjects.Count - 1;
             i >= 0;
             i--)
        {
            PhysicsTowerObject instance =
                activeObjects[i];


            if (instance == null)
            {
                continue;
            }


            instance.Cleared -=
                HandleObjectCleared;


            if (objectPool != null)
            {
                objectPool.Release(
                    instance
                );
            }
        }


        activeObjects.Clear();


        remainingTargets =
            0;


        if (currentTable != null)
        {
            currentTable.gameObject.SetActive(
                false
            );
        }


        currentTable =
            null;
    }


    private Transform GetLevelOrigin()
    {
        return
            levelOrigin != null
                ? levelOrigin
                : transform;
    }


    private bool ValidateReferences()
    {
        if (levelData == null)
        {
            Debug.LogError(
                "Grid Level Data missing hai.",
                this
            );

            return false;
        }


        if (!levelData.HasValidBakedGrid)
        {
            Debug.LogError(
                "GridLevelData bake nahi hui. " +
                "GridLevelData Inspector mein " +
                "'BAKE 3D GRID FROM IMAGE' press karein.",
                levelData
            );

            return false;
        }


        if (levelData.ObjectDefinition == null ||
            levelData.ObjectDefinition.Prefab == null)
        {
            Debug.LogError(
                "Grid Level Data mein Object Definition/Prefab missing hai.",
                levelData
            );

            return false;
        }


        if (levelData.TablePrefab == null)
        {
            Debug.LogError(
                "Grid Level Data mein Table Prefab missing hai.",
                levelData
            );

            return false;
        }


        if (objectPool == null)
        {
            Debug.LogError(
                "Physics Object Pool missing hai.",
                this
            );

            return false;
        }


        if (runtimeObjectsRoot == null)
        {
            Debug.LogError(
                "Runtime Objects Root missing hai.",
                this
            );

            return false;
        }


        Vector3 runtimeScale =
            runtimeObjectsRoot.lossyScale;


        if (Mathf.Abs(runtimeScale.x - 1f) > 0.01f ||
            Mathf.Abs(runtimeScale.y - 1f) > 0.01f ||
            Mathf.Abs(runtimeScale.z - 1f) > 0.01f)
        {
            Debug.LogWarning(
                "RuntimeObjects ki world Scale ideally 1,1,1 honi chahiye.",
                runtimeObjectsRoot
            );
        }


        return true;
    }


    private void OnDestroy()
    {
        for (int i = 0;
             i < activeObjects.Count;
             i++)
        {
            PhysicsTowerObject instance =
                activeObjects[i];


            if (instance != null)
            {
                instance.Cleared -=
                    HandleObjectCleared;
            }
        }
    }
}










