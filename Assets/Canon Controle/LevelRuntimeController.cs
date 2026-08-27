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










// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.Events;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.UI;

// public sealed class LevelRuntimeController : MonoBehaviour
// {
//     private const string SavedLevelNumberKey =
//         "RoyalSmash.CurrentLevelNumber";

//     public event System.Action<GridLevelData> LevelGenerated;






// // [Header("Level Progress")]
// // [SerializeField]
// // private LevelMilestoneProgressBar milestoneProgressBar;









//     [Header("Level")]

//     [SerializeField]
//     private GridLevelData levelData;

//     [Tooltip(
//         "Level Creator ke banaye tamam levels ki central database. " +
//         "Runtime mein individual levels manually assign nahi karne."
//     )]
//     [SerializeField]
//     private GridLevelDatabase levelDatabase;

//     [SerializeField]
//     private bool startFromLevelOne = true;


//     [Header("Next Level UI")]

//     [SerializeField]
//     private Button nextLevelButton;

//     [SerializeField]
//     private TMP_Text levelCompleteText;

//     [Tooltip(
//         "References missing hon to runtime par complete popup aur " +
//         "NEXT LEVEL button khud create karega."
//     )]
//     [SerializeField]
//     private bool autoCreateNextLevelUI = true;

//     [Tooltip(
//         "Level complete hote hi database ka agla saved level " +
//         "automatically load kare."
//     )]
//     [SerializeField]
//     private bool autoLoadNextLevel = true;

//     [Tooltip(
//         "Completion message dikhane ke baad next level load hone ka delay."
//     )]
//     [SerializeField, Min(0f)]
//     private float autoLoadNextLevelDelay = 1.25f;


//     [Header("Main Menu")]

//     [SerializeField]
//     private bool showMainMenuOnStartup = true;

//     [SerializeField]
//     private GameObject mainMenuPanel;

//     [SerializeField]
//     private Button playButton;

//     [SerializeField]
//     private Button resetProgressButton;

//     [SerializeField]
//     private TMP_Text currentLevelText;


//     [Header("References")]

//     [SerializeField]
//     private PhysicsObjectPool objectPool;

//     [SerializeField]
//     private Transform levelOrigin;

//     [SerializeField]
//     private Transform runtimeObjectsRoot;


//     [Header("Support Chain")]

//     [Tooltip(
//         "Support khone ke kitne FixedUpdate frames baad upar wala " +
//         "block khud bhi dynamic ho jayega."
//     )]
//     [SerializeField, Range(1, 10)]
//     private int unsupportedFramesRequired = 2;

//     [Tooltip(
//         "Half-supported piece ko missing-support side par tip karne wala " +
//         "small sideways impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipImpulse = 0.18f;

//     [Tooltip(
//         "Exact edge-balance ko break karne wala natural rotation impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipTorque = 0.28f;


//     [Header("Mirrored Depth Physics")]

//     [Tooltip(
//         "Kisi bhi 2+ depth-layer level mein hit block ka impact same X/Y " +
//         "par maujood baqi depth-layer blocks ko bhi unlock aur push karega. " +
//         "Ye gameplay link Mirror Paint authoring option se independent hai."
//     )]
//     [SerializeField]
//     private bool linkMirroredDepthLayerPhysics = true;

//     [Tooltip(
//         "Front block ke impulse ka kitna hissa mirrored layers ko mile."
//     )]
//     [SerializeField, Range(0f, 1.5f)]
//     private float mirroredLayerImpulseMultiplier = 0.45f;

//     [Tooltip(
//         "Mirrored depth layers exact same frame par na giren. Har linked " +
//         "block ko is range mein chhota natural delay milta hai."
//     )]
//     [SerializeField]
//     private Vector2 mirroredActivationDelayRange =
//         new Vector2(0.09f, 0.22f);

//     [Tooltip(
//         "Mirrored impulse ki strength mein per-block random variation."
//     )]
//     [SerializeField]
//     private Vector2 mirroredImpulseVariation =
//         new Vector2(0.78f, 0.96f);

//     [Tooltip(
//         "Mirrored force direction mein maximum random angle variation."
//     )]
//     [SerializeField, Range(0f, 25f)]
//     private float mirroredDirectionVariationDegrees = 4f;

//     [Tooltip(
//         "Linked blocks ko alag natural spin dene ke liye random torque."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float mirroredRandomTorqueMultiplier = 0.035f;


//     [Header("Events")]

//     [SerializeField]
//     private UnityEvent onLevelGenerated;

//     [SerializeField]
//     private UnityEvent onLevelComplete;


//     private sealed class DefinitionFitData
//     {
//         public Vector3 Scale;
//         public Vector3 BoundsCenterOffset;
//     }


//     private sealed class GridTrackingEntry
//     {
//         public Vector3 InitialPosition;
//         public int UnsupportedFrames;
//         public int TableIndex;
//         public Quaternion SurfaceRotation;
//     }


//     private readonly List<PhysicsTowerObject>
//         activeObjects =
//             new List<PhysicsTowerObject>();

//     /// <summary>
//     /// Keyed by (definition, span) since a bigger footprint (e.g. a
//     /// 2-cell-tall piece) needs its own fit scale, distinct from the
//     /// same definition used at 1x1x1.
//     /// </summary>
//     private readonly Dictionary<(PhysicsObjectDefinition, int spanKey), DefinitionFitData>
//         fitCache =
//             new Dictionary<(PhysicsObjectDefinition, int), DefinitionFitData>();

//     /// <summary>
//     /// What currently occupies each grid cell — used by the support
//     /// chain to answer "is the cell below me still standing?".
//     /// </summary>
//     private readonly Dictionary<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject>
//         occupancyByCoordinate =
//             new Dictionary<(int, Vector3Int), PhysicsTowerObject>();

//     private readonly Dictionary<PhysicsTowerObject, GridTrackingEntry>
//         trackingByInstance =
//             new Dictionary<PhysicsTowerObject, GridTrackingEntry>();


//     private readonly List<LevelTable> cachedTables =
//         new List<LevelTable>();

//     private readonly List<LevelTable> cachedTablePrefabs =
//         new List<LevelTable>();

//     private readonly List<int> tablePhysicsReleaseCycleTargets =
//         new List<int>();

//     private readonly HashSet<int> releasedTablePhysicsIndices =
//         new HashSet<int>();

//     private LevelTable currentTable;


//     private int remainingTargets;

//     private bool levelGenerated;

//     private bool isPropagatingMirroredActivation;

//     private int levelGenerationRevision;

//     private float runtimeTableAnimationTime;

//     private int currentLevelIndex;

//     private AsyncOperationHandle<GridLevelData>
//         activeLevelHandle;

//     private bool hasActiveLevelHandle;

//     private AsyncOperationHandle<GridLevelData>
//         pendingLevelHandle;

//     private bool hasPendingLevelHandle;

//     private bool isLoadingLevel;

//     private GameObject autoCreatedCompletePanel;

//     private CannonController cannonController;

//     private GameObject gameplayRestartButton;

//     /// <summary>
//     /// Lowest occupied grid row for the current level — the "floor"
//     /// row that always counts as supported by the table, even if the
//     /// designer didn't start painting at y=0.
//     /// </summary>
//     private readonly Dictionary<int, int> gridBaseRowByTable =
//         new Dictionary<int, int>();


//     public GridLevelData CurrentLevelData =>
//         levelData;

//     public LevelTable CurrentTable =>
//         currentTable;

//     public IReadOnlyList<LevelTable> CurrentTables =>
//         cachedTables;

//     public int RemainingTargets =>
//         remainingTargets;

//     public bool IsLevelGenerated =>
//         levelGenerated;

//     public int CurrentLevelIndex =>
//         currentLevelIndex;

//     public bool HasNextLevel =>
//         FindNextLevelIndex(currentLevelIndex + 1) >= 0;


//     private void Start()
//     {
//         if (ShouldStartLiveWorkingLevelPreview())
//         {
//             EnsureNextLevelUI();
//             SetLevelCompleteUIVisible(false);
//             currentLevelIndex = 0;
//             GenerateLevel();
//             return;
//         }

//         SelectStartingLevel();

//         if (FindNextLevelIndex(currentLevelIndex) < 0)
//         {
//             Debug.LogError(
//                 "Addressable level catalog empty hai. Level Creator se " +
//                 "kam az kam ek level SAVE karein.",
//                 this
//             );
//             RefreshNextLevelButton();
//             return;
//         }

//         if (showMainMenuOnStartup)
//         {
//             ShowMainMenu();
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ShowMainMenu()
//     {
//         EnsureMainMenuUI();
//         SetGameplayWorldVisible(false);
//         UpdateCurrentLevelText();

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(true);
//         }
//     }


//     public void PlayFromMainMenu()
//     {
//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int playableIndex =
//             FindNextLevelIndex(currentLevelIndex);

//         if (playableIndex < 0)
//         {
//             Debug.LogError(
//                 "Play: koi saved Addressable level configured nahi hai.",
//                 this
//             );
//             return;
//         }

//         currentLevelIndex = playableIndex;

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(false);
//         }

//         SetGameplayWorldVisible(true);

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ResetProgressFromMainMenu()
//     {
//         PlayerPrefs.DeleteKey(SavedLevelNumberKey);
//         PlayerPrefs.Save();

//         int firstLevelIndex =
//             FindNextLevelIndex(0);

//         currentLevelIndex =
//             firstLevelIndex >= 0
//                 ? firstLevelIndex
//                 : 0;

//         UpdateCurrentLevelText();
//     }


//     public void LoadLevel(
//         GridLevelData newLevelData)
//     {
//         if (newLevelData == null)
//         {
//             Debug.LogError(
//                 "LoadLevel: GridLevelData null hai.",
//                 this
//             );

//             return;
//         }


//         if (hasActiveLevelHandle)
//         {
//             PrepareForLevelAssetRelease();
//             ReleaseActiveLevelHandle();
//         }

//         levelData = newLevelData;

//         int sequenceIndex =
//             levelDatabase != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     newLevelData.LevelNumber
//                 )
//                 : -1;

//         if (sequenceIndex >= 0)
//         {
//             currentLevelIndex = sequenceIndex;
//         }

//         SaveCurrentLevelProgress(
//             newLevelData.LevelNumber
//         );


//         GenerateLevel();
//     }


//     [ContextMenu("Generate Level")]
//     public void GenerateLevel()
//     {
//         EnsureNextLevelUI();
//         SetLevelCompleteUIVisible(false);
//         RefreshNextLevelButton();

//         if (!ValidateReferences())
//         {
//             return;
//         }


//         ClearCurrentLevel();

//         runtimeTableAnimationTime = 0f;


//         objectPool.PrepareForLevel(
//             levelData
//         );


//         if (!PrepareTable())
//         {
//             return;
//         }

//         PrepareTablePhysicsReleaseTargets();


//         remainingTargets =
//             0;


//         SpawnGrid();


//         Physics.SyncTransforms();


//         levelGenerated =
//             true;


//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} GENERATED\n" +
//             $"Boxes: {activeObjects.Count}\n" +
//             $"Targets: {remainingTargets}",
//             this
//         );


//         LevelGenerated?.Invoke(levelData);

//         onLevelGenerated?.Invoke();


//         if (remainingTargets <= 0)
//         {
//             Debug.LogWarning(
//                 "Generated level mein koi target nahi mila.",
//                 this
//             );
//         }
//     }


//     /// <summary>
//     /// Measures a definition's prefab once (identity rotation, prefab's
//     /// authored scale) and derives the localScale + pivot-to-collider
//     /// offset needed so its collider bounds exactly fill levelData's
//     /// CellSize. Cached per-definition so mixed shapes (cube, cylinder,
//     /// ...) all land cleanly on the same uniform grid without measuring
//     /// on every single cell spawn.
//     /// </summary>
//     private bool TryGetFitData(
//         PhysicsObjectDefinition definition,
//         int spanX,
//         int spanY,
//         int spanZ,
//         PieceOrientation orientation,
//         out DefinitionFitData fitData)
//     {
//         int spanKey =
//             spanX * 10000 +
//             spanY * 100 +
//             spanZ +
//             (int)orientation * 1000000;

//         (PhysicsObjectDefinition, int) cacheKey =
//             (definition, spanKey);

//         if (fitCache.TryGetValue(
//                 cacheKey,
//                 out fitData))
//         {
//             return true;
//         }

//         fitData = null;

//         PhysicsTowerObject measurementObject =
//             objectPool.Get(
//                 definition,
//                 Vector3.zero,
//                 Quaternion.identity,
//                 runtimeObjectsRoot
//             );

//         if (measurementObject == null)
//         {
//             Debug.LogError(
//                 "Grid object measure karne ke liye pool object nahi mila.",
//                 this
//             );

//             return false;
//         }

//         Physics.SyncTransforms();

//         if (!measurementObject.TryGetPhysicsBounds(
//                 out Bounds bounds))
//         {
//             Debug.LogError(
//                 "Physics object par valid non-trigger Collider nahi mila.",
//                 measurementObject
//             );

//             objectPool.Release(measurementObject);

//             return false;
//         }

//         Vector3 measuredSize =
//             bounds.size;

//         Vector3 measuredCenterOffset =
//             bounds.center -
//             measurementObject.transform.position;

//         objectPool.Release(measurementObject);

//         if (measuredSize.x <= 0.0001f ||
//             measuredSize.y <= 0.0001f ||
//             measuredSize.z <= 0.0001f)
//         {
//             Debug.LogError(
//                 $"{definition.DisplayName}: collider size invalid hai.",
//                 this
//             );

//             return false;
//         }

//         Vector3 authoredScale =
//             definition.Prefab.transform.localScale;

//         Vector3 finalScale;

//         Vector3 finalOffset;

//         if (definition.AutoFitToCell)
//         {
//             /*
//              * Span > 1 par target size sirf spanCount * cellSize nahi —
//              * internal gaps bhi fill hote hain (spanX*cellSize.x +
//              * (spanX-1)*gap) taake merged piece apne poore footprint
//              * ke outer edges tak seamless dikhe, jaisa cells alag alag
//              * (gap ke sath) hote to unka combined outer span hota.
//              */
//             Vector3 gridAlignedTargetSize =
//                 new Vector3(
//                     spanX * levelData.CellSize.x +
//                     (spanX - 1) * levelData.HorizontalGap,

//                     spanY * levelData.CellSize.y +
//                     (spanY - 1) * levelData.VerticalGap,

//                     spanZ * levelData.CellSize.z +
//                     (spanZ - 1) * levelData.DepthGap
//                 );

//             /*
//              * gridAlignedTargetSize world/grid axes ke liye hai.
//              * Lekin fit-scale prefab ke apne UNROTATED local axes par
//              * compute hoti hai — agar orientation tip kar rahi hai
//              * (Lying X/Z) to local axes rotate hone ke baad world axes
//              * se match nahi karte, is liye target size ko yahan
//              * local axes par re-map karte hain (spawn time par
//              * PieceOrientationUtility.GetRotation() isi mapping ko
//              * wapas world space mein rotate kar degi).
//              */
//             Vector3 targetSize =
//                 PieceOrientationUtility.GetLocalAxisTargetSize(
//                     gridAlignedTargetSize,
//                     orientation
//                 );

//             /*
//              * boundsAtScale1 = measuredSize / authoredScale
//              * finalScale     = targetSize / boundsAtScale1
//              *                = targetSize * authoredScale / measuredSize
//              *
//              * Isi tarah offset bhi authored scale se scale-1 basis par
//              * normalize karke, phir finalScale par re-apply hota hai —
//              * taake off-center collider pivot bhi correct rahe.
//              */
//             finalScale =
//                 new Vector3(
//                     targetSize.x * authoredScale.x / measuredSize.x,
//                     targetSize.y * authoredScale.y / measuredSize.y,
//                     targetSize.z * authoredScale.z / measuredSize.z
//                 );

//             Vector3 offsetAtScale1 =
//                 new Vector3(
//                     measuredCenterOffset.x / authoredScale.x,
//                     measuredCenterOffset.y / authoredScale.y,
//                     measuredCenterOffset.z / authoredScale.z
//                 );

//             finalOffset =
//                 Vector3.Scale(
//                     offsetAtScale1,
//                     finalScale
//                 );
//         }
//         else
//         {
//             finalScale =
//                 authoredScale;

//             finalOffset =
//                 measuredCenterOffset;
//         }

//         finalScale =
//             Vector3.Scale(
//                 finalScale,
//                 definition.ManualScaleMultiplier
//             );

//         fitData =
//             new DefinitionFitData
//             {
//                 Scale = finalScale,
//                 BoundsCenterOffset = finalOffset
//             };

//         fitCache[cacheKey] =
//             fitData;

//         return true;
//     }


//     private void SpawnGrid()
//     {
//         Vector3Int occupiedMin =
//             new Vector3Int(
//                 int.MaxValue,
//                 int.MaxValue,
//                 int.MaxValue
//             );

//         Vector3Int occupiedMax =
//             new Vector3Int(
//                 int.MinValue,
//                 int.MinValue,
//                 int.MinValue
//             );

//         if (currentTable == null ||
//             !currentTable.TryGetTowerSurface(
//                 out Vector3 primarySurfacePosition,
//                 out Quaternion primarySurfaceRotation))
//         {
//             Debug.LogError(
//                 "PRIMARY table ki tower surface calculate nahi ho saki.",
//                 currentTable
//             );

//             return;
//         }

//         int tableCount =
//             Mathf.Max(1, cachedTables.Count);

//         Vector3[] surfacePositions =
//             new Vector3[tableCount];

//         Quaternion[] surfaceRotations =
//             new Quaternion[tableCount];

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             LevelTable table =
//                 tableIndex < cachedTables.Count
//                     ? cachedTables[tableIndex]
//                     : null;

//             if (table != null &&
//                 table.TryGetTowerSurface(
//                     out Vector3 tableSurfacePosition,
//                     out Quaternion tableSurfaceRotation))
//             {
//                 surfacePositions[tableIndex] = tableSurfacePosition;
//                 surfaceRotations[tableIndex] = tableSurfaceRotation;
//             }
//             else
//             {
//                 surfacePositions[tableIndex] = primarySurfacePosition;
//                 surfaceRotations[tableIndex] = primarySurfaceRotation;
//             }
//         }

//         Dictionary<int, Vector3Int> occupiedMinByTable =
//             new Dictionary<int, Vector3Int>();

//         Dictionary<int, Vector3Int> occupiedMaxByTable =
//             new Dictionary<int, Vector3Int>();

//         bool foundAnyOccupiedCell = false;

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             if (!levelData.TryGetOccupiedBounds(
//                     tableIndex,
//                     out Vector3Int tableMinimum,
//                     out Vector3Int tableMaximum))
//             {
//                 continue;
//             }

//             foundAnyOccupiedCell = true;
//             occupiedMinByTable[tableIndex] = tableMinimum;
//             occupiedMaxByTable[tableIndex] = tableMaximum;
//             occupiedMin = Vector3Int.Min(occupiedMin, tableMinimum);
//             occupiedMax = Vector3Int.Max(occupiedMax, tableMaximum);
//         }

//         if (!foundAnyOccupiedCell)
//         {
//             Debug.LogError(
//                 "Kisi bhi table ke manual grid mein occupied cells nahi hain.",
//                 levelData
//             );

//             return;
//         }

//         gridBaseRowByTable.Clear();

//         foreach (KeyValuePair<int, Vector3Int> entry
//                  in occupiedMinByTable)
//         {
//             gridBaseRowByTable[entry.Key] =
//                 entry.Value.y;
//         }


//         Vector3 cellSize =
//             levelData.CellSize;


//         float stepX =
//             cellSize.x +
//             levelData.HorizontalGap;


//         float stepY =
//             cellSize.y +
//             levelData.VerticalGap;


//         float stepZ =
//             cellSize.z +
//             levelData.DepthGap;


//         /*
//          * Important:
//          *
//          * Y outer loop hai.
//          * Isliye logical spawn order bottom → top rahega.
//          */
//         for (int y = occupiedMin.y;
//              y <= occupiedMax.y;
//              y++)
//         {
//             for (int z = occupiedMin.z;
//                  z <= occupiedMax.z;
//                  z++)
//             {
//                 for (int x = occupiedMin.x;
//                      x <= occupiedMax.x;
//                      x++)
//                 {
//                     for (int authoredTableIndex = 0;
//                          authoredTableIndex < tableCount;
//                          authoredTableIndex++)
//                     {
//                         if (!occupiedMinByTable.ContainsKey(
//                                 authoredTableIndex))
//                         {
//                             continue;
//                         }

//                         GridCellData cell =
//                             levelData.GetCell(
//                                 x,
//                                 y,
//                                 z,
//                                 authoredTableIndex
//                             );


//                     if (cell == null ||
//                         !cell.Occupied)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Covered cells (bare footprint ka hissa) khud
//                      * spawn nahi hoti — unka anchor cell hi spawn
//                      * karta hai (neeche is loop mein aage aake).
//                      */
//                     if (cell.IsCovered)
//                     {
//                         continue;
//                     }


//                     int blockTableIndex =
//                         authoredTableIndex;

//                     Vector3Int tableOccupiedMin =
//                         occupiedMinByTable[blockTableIndex];

//                     Vector3Int tableOccupiedMax =
//                         occupiedMaxByTable[blockTableIndex];

//                     float occupiedCenterX =
//                         (
//                             tableOccupiedMin.x +
//                             tableOccupiedMax.x
//                         ) *
//                         0.5f;

//                     float occupiedCenterZ =
//                         (
//                             tableOccupiedMin.z +
//                             tableOccupiedMax.z
//                         ) *
//                         0.5f;

//                     Vector3 surfacePosition =
//                         surfacePositions[blockTableIndex];

//                     Quaternion surfaceRotation =
//                         surfaceRotations[blockTableIndex];


//                     int spanX =
//                         Mathf.Max(1, cell.SpanX);

//                     int spanY =
//                         Mathf.Max(1, cell.SpanY);

//                     int spanZ =
//                         Mathf.Max(1, cell.SpanZ);


//                     /*
//                      * X:
//                      * Shape automatically center.
//                      *
//                      * Span > 1 ho to poore footprint (x..x+spanX-1)
//                      * ka center use hota hai, sirf anchor cell ka nahi.
//                      */
//                     float localX =
//                         (
//                             x -
//                             occupiedCenterX
//                         ) *
//                         stepX +
//                         (spanX - 1) *
//                         stepX *
//                         0.5f +
//                         cell.LocalOffset.x *
//                         stepX;


//                     /*
//                      * Y:
//                      * Lowest occupied manual-grid row
//                      * directly table top par start hogi.
//                      *
//                      * Transparent bottom margin
//                      * automatically ignore hota hai.
//                      */
//                     float localY =
//                         cellSize.y *
//                         0.5f +
//                         (
//                             y -
//                             tableOccupiedMin.y
//                         ) *
//                         stepY +
//                         (spanY - 1) *
//                         stepY *
//                         0.5f +
//                         cell.LocalOffset.y *
//                         stepY;


//                     /*
//                      * Z:
//                      * Complete depth centered.
//                      */
//                     float localZ =
//                         (
//                             z -
//                             occupiedCenterZ
//                         ) *
//                         stepZ +
//                         (spanZ - 1) *
//                         stepZ *
//                         0.5f +
//                         cell.LocalOffset.z *
//                         stepZ;


//                     Vector3 localCellCenter =
//                         new Vector3(
//                             localX,
//                             localY,
//                             localZ
//                         )
//                         +
//                         levelData.GridOffset;


//                     PhysicsObjectDefinition definition =
//                         levelData.GetPaletteEntry(
//                             cell.DefinitionIndex
//                         );

//                     if (definition == null ||
//                         definition.Prefab == null)
//                     {
//                         Debug.LogWarning(
//                             $"Cell ({x},{y},{z}): palette entry " +
//                             $"{cell.DefinitionIndex} invalid hai, skip.",
//                             levelData
//                         );

//                         continue;
//                     }

//                     PieceOrientation orientation =
//                         cell.Orientation;

//                     if (!TryGetFitData(
//                             definition,
//                             spanX,
//                             spanY,
//                             spanZ,
//                             orientation,
//                             out DefinitionFitData fitData))
//                     {
//                         continue;
//                     }


//                     /*
//                      * Orientation ka extra tip (Lying X/Z) surface
//                      * rotation ke UPAR apply hoti hai — piece ko table
//                      * ke sath sath khud bhi rotate karta hai.
//                      */
//                     Quaternion pieceRotation =
//                         surfaceRotation *
//                         PieceOrientationUtility.GetRotation(
//                             orientation,
//                             cell.CustomZRotation
//                         ) *
//                         Quaternion.Euler(
//                             cell.RotationEulerOffset
//                         );


//                     Vector3 pieceScale =
//                         Vector3.Scale(
//                             fitData.Scale,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 desiredBoundsCenter =
//                         surfacePosition +
//                         surfaceRotation *
//                         localCellCenter;


//                     /*
//                      * Prefab pivot center par na bhi ho
//                      * to collider bottom proper align hoga.
//                      */
//                     Vector3 rotatedBoundsOffset =
//                         pieceRotation *
//                         Vector3.Scale(
//                             fitData.BoundsCenterOffset,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 spawnPosition =
//                         desiredBoundsCenter -
//                         rotatedBoundsOffset;


//                     PhysicsTowerObject instance =
//                         objectPool.Get(
//                             definition,
//                             spawnPosition,
//                             pieceRotation,
//                             runtimeObjectsRoot
//                         );


//                     if (instance == null)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Pool.Get() prefab ki authored scale laga deta hai —
//                      * yahan usay cell-fit scale se override karte hain
//                      * taake mixed shapes (cube/cylinder) uniform grid
//                      * mein clean fit hon.
//                      */
//                     instance.transform.localScale =
//                         pieceScale;


//                     /*
//                      * Pool se purani property-block state clear karo.
//                      * Default visual hamesha prefab ka apna material aur
//                      * texture hai; paint tint definition par optional hai.
//                      */
//                     instance.RestorePrefabVisual();

//                     if (definition.TintWithPaintColor)
//                     {
//                         instance.SetVisualColor(
//                             cell.Color
//                         );
//                     }


//                     Vector3Int anchorCoordinate =
//                         new Vector3Int(x, y, z);

//                     instance.SetGridFootprint(
//                         anchorCoordinate,
//                         new Vector3Int(
//                             spanX,
//                             spanY,
//                             spanZ
//                         ),
//                         cell.LocalOffset
//                     );

//                     instance.ConfigureBreakable(
//                         cell.Breakable,
//                         cell.HitsToBreak
//                     );

//                     /*
//                      * Bare footprint ke SAARE cells is instance ko
//                      * point karte hain — is se agar upar kuch spawn ho
//                      * to woh footprint ke kisi bhi column par "supported"
//                      * sahi detect hoga.
//                      */
//                     for (int fz = 0; fz < spanZ; fz++)
//                     {
//                         for (int fy = 0; fy < spanY; fy++)
//                         {
//                             for (int fx = 0; fx < spanX; fx++)
//                             {
//                                 occupancyByCoordinate[
//                                     (
//                                         blockTableIndex,
//                                         new Vector3Int(
//                                             x + fx,
//                                             y + fy,
//                                             z + fz
//                                         )
//                                     )
//                                 ] = instance;
//                             }
//                         }
//                     }

//                     trackingByInstance[instance] =
//                         new GridTrackingEntry
//                         {
//                             InitialPosition = spawnPosition,
//                             UnsupportedFrames = 0,
//                             TableIndex = blockTableIndex,
//                             SurfaceRotation = surfaceRotation
//                         };


//                     instance.Cleared +=
//                         HandleObjectCleared;

//                     instance.Activated +=
//                         HandleObjectActivated;


//                     activeObjects.Add(
//                         instance
//                     );


//                     if (instance.CountsAsTarget)
//                     {
//                         remainingTargets++;
//                     }
//                     }
//                 }
//             }
//         }
//     }


//     private int ResolveBlockTableIndex(int requestedIndex)
//     {
//         if (requestedIndex >= 0 &&
//             requestedIndex < cachedTables.Count &&
//             cachedTables[requestedIndex] != null)
//         {
//             return requestedIndex;
//         }

//         return 0;
//     }


//     private void FixedUpdate()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }

//         AnimateEnabledTables();

//         if (activeObjects.Count == 0)
//         {
//             return;
//         }

//         /*
//          * Snapshot: activeObjects FixedUpdate ke dauran
//          * HandleObjectCleared se modify ho sakti hai
//          * (ActivatePhysics() khud kuch remove nahi karta,
//          * lekin safety ke liye copy le lete hain).
//          */
//         for (int i = activeObjects.Count - 1; i >= 0; i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];

//             if (instance == null || !instance.IsLocked)
//             {
//                 continue;
//             }

//             if (!trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry))
//             {
//                 continue;
//             }

//             if (IsSupported(
//                     instance,
//                     entry,
//                     out Vector3 unsupportedLocalDirection))
//             {
//                 entry.UnsupportedFrames = 0;
//                 continue;
//             }

//             entry.UnsupportedFrames++;

//             if (entry.UnsupportedFrames >=
//                 unsupportedFramesRequired)
//             {
//                 Vector3 worldFallDirection =
//                     entry.SurfaceRotation *
//                     unsupportedLocalDirection.normalized;

//                 Vector3 worldUp =
//                     entry.SurfaceRotation * Vector3.up;

//                 Vector3 tippingTorque =
//                     Vector3.Cross(
//                         worldUp,
//                         worldFallDirection
//                     ).normalized *
//                     unsupportedTipTorque *
//                     Random.Range(0.9f, 1.1f);

//                 instance.ActivatePhysics(
//                     worldFallDirection *
//                     unsupportedTipImpulse *
//                     Random.Range(0.9f, 1.1f),
//                     tippingTorque
//                 );
//             }
//         }
//     }


//     /// <summary>
//     /// Bottom row table par khadi hoti hai. Baqi pieces ke poore bottom
//     /// footprint mein kam az kam ek LOCKED support hona chahiye. Hit hua
//     /// dynamic block foran support count hona band ho jata hai, chahe woh
//     /// abhi apni spawn position se zyada move na bhi hua ho.
//     ///
//     /// Multi-cell footprint (span > 1) wale pieces ke liye sirf
//     /// "kahin bhi neeche support hai" (OR) kaafi nahi — agar ek extreme
//     /// edge (min ya max column) ka support hat jaye to piece ko us taraf
//     /// tip hona chahiye, chahe doosra extreme abhi bhi supported ho.
//     /// Ye seam-offset pieces (0.5 Center-on-Seam) aur plain grid-aligned
//     /// multi-span pieces, dono par apply hota hai.
//     /// </summary>
//     private bool IsSupported(
//         PhysicsTowerObject instance,
//         GridTrackingEntry trackingEntry,
//         out Vector3 unsupportedLocalDirection)
//     {
//         unsupportedLocalDirection = Vector3.zero;

//         if (instance == null)
//         {
//             unsupportedLocalDirection = Vector3.right;
//             return false;
//         }

//         Vector3Int coordinate =
//             instance.GridCoordinate;

//         int baseRow =
//             gridBaseRowByTable.TryGetValue(
//                 trackingEntry.TableIndex,
//                 out int tableBaseRow)
//                 ? tableBaseRow
//                 : coordinate.y;

//         if (coordinate.y <= baseRow)
//         {
//             return true;
//         }

//         Vector3Int span =
//             instance.GridSpan;

//         Vector3 localOffset =
//             instance.GridLocalOffset;

//         int minimumX =
//             coordinate.x +
//             Mathf.FloorToInt(localOffset.x);

//         int maximumX =
//             coordinate.x +
//             Mathf.Max(1, span.x) - 1 +
//             Mathf.CeilToInt(localOffset.x);

//         int minimumZ =
//             coordinate.z +
//             Mathf.FloorToInt(localOffset.z);

//         int maximumZ =
//             coordinate.z +
//             Mathf.Max(1, span.z) - 1 +
//             Mathf.CeilToInt(localOffset.z);

//         bool seamOnX =
//             !Mathf.Approximately(
//                 localOffset.x,
//                 Mathf.Round(localOffset.x)
//             );

//         bool seamOnZ =
//             !Mathf.Approximately(
//                 localOffset.z,
//                 Mathf.Round(localOffset.z)
//             );

//         /*
//          * Seam offset ke bina bhi agar piece ek se zyada grid column
//          * span kar raha hai (e.g. horizontal 2-cell beam), to sirf
//          * "kahin bhi support hai" kaafi nahi — dono extremes (min aur
//          * max column) ka support alag alag check hona chahiye, warna
//          * ek pillar hat jane par doosre pillar ka sahara poore beam
//          * ko galat tarah "supported" dikha deta hai.
//          */
//         bool spansMultipleColumnsX =
//             maximumX > minimumX;

//         bool spansMultipleColumnsZ =
//             maximumZ > minimumZ;

//         bool hasAnySupport = false;
//         bool minimumXSupported = false;
//         bool maximumXSupported = false;
//         bool minimumZSupported = false;
//         bool maximumZSupported = false;

//         for (int z = minimumZ; z <= maximumZ; z++)
//         {
//             for (int x = minimumX; x <= maximumX; x++)
//             {
//                 Vector3Int below =
//                     new Vector3Int(
//                         x,
//                         coordinate.y - 1,
//                         z
//                     );

//                 if (occupancyByCoordinate.TryGetValue(
//                         (trackingEntry.TableIndex, below),
//                         out PhysicsTowerObject supportInstance) &&
//                     supportInstance != null &&
//                     supportInstance != instance &&
//                     supportInstance.IsLocked)
//                 {
//                     hasAnySupport = true;
//                     minimumXSupported |= x == minimumX;
//                     maximumXSupported |= x == maximumX;
//                     minimumZSupported |= z == minimumZ;
//                     maximumZSupported |= z == maximumZ;
//                 }
//             }
//         }

//         if (!hasAnySupport)
//         {
//             unsupportedLocalDirection =
//                 GetNaturalUnsupportedDirection(
//                     coordinate,
//                     localOffset
//                 );

//             return false;
//         }

//         if ((seamOnX || spansMultipleColumnsX) &&
//             (!minimumXSupported || !maximumXSupported))
//         {
//             if (!minimumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.left;
//             }

//             if (!maximumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.right;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.x >= 0f
//                         ? Vector3.right
//                         : Vector3.left;
//             }

//             return false;
//         }

//         if ((seamOnZ || spansMultipleColumnsZ) &&
//             (!minimumZSupported || !maximumZSupported))
//         {
//             if (!minimumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.back;
//             }

//             if (!maximumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.forward;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.z >= 0f
//                         ? Vector3.forward
//                         : Vector3.back;
//             }

//             return false;
//         }

//         return true;
//     }


//     private static Vector3 GetNaturalUnsupportedDirection(
//         Vector3Int coordinate,
//         Vector3 localOffset)
//     {
//         Vector3 direction =
//             new Vector3(
//                 localOffset.x,
//                 0f,
//                 localOffset.z
//             );

//         if (direction.sqrMagnitude > 0.001f)
//         {
//             return direction.normalized;
//         }

//         int hash =
//             coordinate.x * 73856093 ^
//             coordinate.y * 19349663 ^
//             coordinate.z * 83492791;

//         float x =
//             (hash & 1) == 0 ? -1f : 1f;

//         float z =
//             (hash & 2) == 0 ? -0.45f : 0.45f;

//         return new Vector3(x, 0f, z).normalized;
//     }


//     private bool PrepareTable()
//     {
//         int requiredTableCount =
//             levelData.TableCount;

//         if (requiredTableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "GridLevelData mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }

//         while (cachedTables.Count > requiredTableCount)
//         {
//             DestroyCachedTableAt(
//                 cachedTables.Count - 1
//             );

//             cachedTables.RemoveAt(
//                 cachedTables.Count - 1
//             );

//             cachedTablePrefabs.RemoveAt(
//                 cachedTablePrefabs.Count - 1
//             );
//         }

//         Transform levelRoot =
//             GetLevelOrigin();

//         while (cachedTables.Count < requiredTableCount)
//         {
//             cachedTables.Add(null);
//             cachedTablePrefabs.Add(null);
//         }

//         for (int index = 0;
//              index < requiredTableCount;
//              index++)
//         {
//             LevelTable requiredPrefab =
//                 levelData.GetTablePrefab(index);

//             if (requiredPrefab == null)
//             {
//                 DestroyCachedTableAt(index);

//                 Debug.LogWarning(
//                     $"Table Placements element {index} ka Prefab missing hai; " +
//                     "is extra table ko skip kiya gaya.",
//                     levelData
//                 );

//                 continue;
//             }

//             if (cachedTables[index] == null ||
//                 cachedTablePrefabs[index] != requiredPrefab)
//             {
//                 DestroyCachedTableAt(index);

//                 LevelTable instance =
//                     Instantiate(
//                         requiredPrefab,
//                         levelRoot
//                     );

//                 instance.name =
//                     requiredPrefab.name +
//                     $"_Runtime_{index + 1}";

//                 cachedTables[index] = instance;
//                 cachedTablePrefabs[index] = requiredPrefab;
//             }

//             LevelTable table =
//                 cachedTables[index];

//             Transform tableTransform =
//                 table.transform;

//             if (tableTransform.parent != levelRoot)
//             {
//                 tableTransform.SetParent(
//                     levelRoot,
//                     false
//                 );
//             }

//             tableTransform.localPosition =
//                 levelData.GetTablePositionOffset(index);

//             tableTransform.localRotation =
//                 Quaternion.Euler(
//                     levelData.GetTableRotationEuler(index)
//                 );

//             tableTransform.localScale =
//                 Vector3.Scale(
//                     requiredPrefab.transform.localScale,
//                     levelData.GetTableSizeMultiplier(index)
//                 );

//             table.gameObject.SetActive(true);

//             if (table.TowerSurfaceCollider == null)
//             {
//                 Debug.LogError(
//                     $"Table Placements element {index} ke prefab par " +
//                     "Tower Surface Collider missing hai.",
//                     table
//                 );

//                 return false;
//             }
//         }

//         currentTable = cachedTables[0];

//         return true;
//     }


//     private void DestroyCachedTableAt(int index)
//     {
//         if (index < 0 ||
//             index >= cachedTables.Count)
//         {
//             return;
//         }

//         LevelTable table =
//             cachedTables[index];

//         if (table != null)
//         {
//             if (Application.isPlaying)
//             {
//                 Destroy(table.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(table.gameObject);
//             }
//         }

//         cachedTables[index] = null;

//         if (index < cachedTablePrefabs.Count)
//         {
//             cachedTablePrefabs[index] = null;
//         }
//     }


//     private void AnimateEnabledTables()
//     {
//         if (levelData == null ||
//             cachedTables.Count == 0)
//         {
//             return;
//         }

//         runtimeTableAnimationTime +=
//             Time.fixedDeltaTime;

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             LevelTable table = cachedTables[tableIndex];
//             bool rotationEnabled =
//                 levelData.IsTableRuntimeRotationEnabled(
//                     tableIndex
//                 );
//             bool movementEnabled =
//                 levelData.IsTableRuntimeHorizontalMovementEnabled(
//                     tableIndex
//                 );

//             if (table == null ||
//                 (!rotationEnabled && !movementEnabled))
//             {
//                 continue;
//             }

//             Vector3 rotationAxis =
//                 levelData.GetTableRuntimeRotationAxis(tableIndex);
//             float rotationSpeed =
//                 levelData.GetTableRuntimeRotationSpeed(tableIndex);
//             Vector3 movementAxis =
//                 levelData.GetTableRuntimeMovementAxis(tableIndex);
//             float movementDistance =
//                 levelData.GetTableRuntimeMovementDistance(tableIndex);
//             float movementSpeed =
//                 levelData.GetTableRuntimeMovementSpeed(tableIndex);

//             bool canRotate =
//                 rotationEnabled &&
//                 rotationAxis.sqrMagnitude >= 0.0001f &&
//                 !Mathf.Approximately(rotationSpeed, 0f);

//             bool canMove =
//                 movementEnabled &&
//                 movementAxis.sqrMagnitude >= 0.0001f &&
//                 movementDistance > 0f &&
//                 movementSpeed > 0f;

//             if (!canRotate && !canMove)
//             {
//                 continue;
//             }

//             Transform tableTransform = table.transform;
//             Vector3 oldWorldPosition = tableTransform.position;
//             Quaternion oldWorldRotation = tableTransform.rotation;

//             if (canMove)
//             {
//                 float movementPhase =
//                     runtimeTableAnimationTime *
//                     movementSpeed *
//                     Mathf.PI * 2f;

//                 tableTransform.localPosition =
//                     levelData.GetTablePositionOffset(tableIndex) +
//                     movementAxis.normalized *
//                     Mathf.Sin(movementPhase) *
//                     movementDistance;
//             }

//             if (canRotate)
//             {
//                 tableTransform.localRotation =
//                     tableTransform.localRotation *
//                     Quaternion.AngleAxis(
//                         rotationSpeed * Time.fixedDeltaTime,
//                         rotationAxis.normalized
//                     );
//             }

//             Vector3 newWorldPosition = tableTransform.position;
//             Vector3 tableVelocity =
//                 (newWorldPosition - oldWorldPosition) /
//                 Mathf.Max(0.0001f, Time.fixedDeltaTime);
//             Quaternion worldRotationDelta =
//                 tableTransform.rotation *
//                 Quaternion.Inverse(oldWorldRotation);

//             for (int objectIndex = 0;
//                  objectIndex < activeObjects.Count;
//                  objectIndex++)
//             {
//                 PhysicsTowerObject instance =
//                     activeObjects[objectIndex];

//                 if (instance == null ||
//                     !instance.IsLocked ||
//                     !trackingByInstance.TryGetValue(
//                         instance,
//                         out GridTrackingEntry entry) ||
//                     entry.TableIndex != tableIndex)
//                 {
//                     continue;
//                 }

//                 Rigidbody objectBody = instance.Body;
//                 Vector3 objectPosition =
//                     objectBody != null
//                         ? objectBody.position
//                         : instance.transform.position;
//                 Quaternion objectRotation =
//                     objectBody != null
//                         ? objectBody.rotation
//                         : instance.transform.rotation;
//                 Vector3 animatedPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (objectPosition - oldWorldPosition);

//                 instance.MoveLockedWithTable(
//                     animatedPosition,
//                     worldRotationDelta * objectRotation
//                 );

//                 entry.InitialPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (entry.InitialPosition - oldWorldPosition);

//                 entry.SurfaceRotation =
//                     worldRotationDelta * entry.SurfaceRotation;
//             }

//             TryReleaseTableBlocksAfterMovementCycles(
//                 tableIndex,
//                 canMove ? movementSpeed : 0f,
//                 tableVelocity
//             );
//         }
//     }


//     private void PrepareTablePhysicsReleaseTargets()
//     {
//         tablePhysicsReleaseCycleTargets.Clear();
//         releasedTablePhysicsIndices.Clear();

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             if (!levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                     tableIndex))
//             {
//                 tablePhysicsReleaseCycleTargets.Add(int.MaxValue);
//                 continue;
//             }

//             int minimumCycles =
//                 Mathf.Max(
//                     1,
//                     levelData.GetTableMinimumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );
//             int maximumCycles =
//                 Mathf.Max(
//                     minimumCycles,
//                     levelData.GetTableMaximumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );

//             tablePhysicsReleaseCycleTargets.Add(
//                 Random.Range(
//                     minimumCycles,
//                     maximumCycles + 1
//                 )
//             );
//         }
//     }


//     private void TryReleaseTableBlocksAfterMovementCycles(
//         int tableIndex,
//         float movementSpeed,
//         Vector3 tableVelocity)
//     {
//         if (tableIndex < 0 ||
//             tableIndex >= tablePhysicsReleaseCycleTargets.Count ||
//             releasedTablePhysicsIndices.Contains(tableIndex) ||
//             !levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                 tableIndex) ||
//             movementSpeed <= 0f)
//         {
//             return;
//         }

//         float completedCycles =
//             runtimeTableAnimationTime * movementSpeed;

//         if (completedCycles <
//             tablePhysicsReleaseCycleTargets[tableIndex])
//         {
//             return;
//         }

//         releasedTablePhysicsIndices.Add(tableIndex);

//         float velocityMultiplier =
//             levelData.GetTableMovementReleaseVelocityMultiplier(
//                 tableIndex
//             );

//         bool wasPropagatingMirroredActivation =
//             isPropagatingMirroredActivation;

//         isPropagatingMirroredActivation = true;

//         for (int objectIndex = activeObjects.Count - 1;
//              objectIndex >= 0;
//              objectIndex--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[objectIndex];

//             if (instance == null ||
//                 !instance.IsLocked ||
//                 !trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry) ||
//                 entry.TableIndex != tableIndex)
//             {
//                 continue;
//             }

//             Rigidbody body = instance.Body;
//             float bodyMass =
//                 body != null
//                     ? Mathf.Max(0.01f, body.mass)
//                     : 1f;

//             Vector3 naturalVariation =
//                 new Vector3(
//                     Random.Range(-0.08f, 0.08f),
//                     Random.Range(0.015f, 0.07f),
//                     Random.Range(-0.08f, 0.08f)
//                 );

//             Vector3 inheritedImpulse =
//                 (tableVelocity * velocityMultiplier +
//                  naturalVariation) *
//                 bodyMass;

//             Vector3 naturalTorque =
//                 Random.onUnitSphere *
//                 Random.Range(0.035f, 0.12f) *
//                 bodyMass;

//             instance.ActivatePhysics(
//                 inheritedImpulse,
//                 naturalTorque
//             );
//         }

//         isPropagatingMirroredActivation =
//             wasPropagatingMirroredActivation;
//     }


//     private void HandleObjectActivated(
//         PhysicsTowerObject source,
//         Vector3 sourceImpulse)
//     {
//         if (source == null ||
//             levelData == null ||
//             !linkMirroredDepthLayerPhysics ||
//             levelData.GridDepth <= 1 ||
//             isPropagatingMirroredActivation)
//         {
//             return;
//         }

//         Vector3Int sourceCoordinate =
//             source.GridCoordinate;

//         if (!trackingByInstance.TryGetValue(
//                 source,
//                 out GridTrackingEntry sourceTracking))
//         {
//             return;
//         }

//         int sourceTableIndex =
//             sourceTracking.TableIndex;

//         int activationRevision =
//             levelGenerationRevision;

//         for (int z = 0;
//              z < levelData.GridDepth;
//              z++)
//         {
//             if (z == sourceCoordinate.z ||
//                 !TryFindDepthLinkedObject(
//                     source,
//                     sourceCoordinate,
//                     sourceTableIndex,
//                     z,
//                     out PhysicsTowerObject mirroredObject,
//                     out Vector3Int mirroredCoordinate))
//             {
//                 continue;
//             }

//             StartCoroutine(
//                 ActivateMirroredObjectNaturally(
//                     mirroredObject,
//                     mirroredCoordinate,
//                     sourceTableIndex,
//                     sourceImpulse,
//                     activationRevision
//                 )
//             );
//         }
//     }


//     private bool TryFindDepthLinkedObject(
//         PhysicsTowerObject source,
//         Vector3Int sourceCoordinate,
//         int tableIndex,
//         int targetDepth,
//         out PhysicsTowerObject linkedObject,
//         out Vector3Int linkedCoordinate)
//     {
//         linkedObject = null;
//         linkedCoordinate = new Vector3Int(
//             sourceCoordinate.x,
//             sourceCoordinate.y,
//             targetDepth
//         );

//         if (TryGetLockedDepthObject(
//                 linkedCoordinate,
//                 tableIndex,
//                 source,
//                 out linkedObject))
//         {
//             return true;
//         }

//         /*
//          * Manually-authored layers can have different silhouettes. In that
//          * case there may be no block at the exact X/Y coordinate behind the
//          * hit. Find the nearest locked block in a small projected area so the
//          * back structure still receives a believable local impact rather
//          * than remaining completely static.
//          */
//         const int searchRadius = 2;
//         float bestScore = float.PositiveInfinity;

//         for (int yOffset = -searchRadius;
//              yOffset <= searchRadius;
//              yOffset++)
//         {
//             for (int xOffset = -searchRadius;
//                  xOffset <= searchRadius;
//                  xOffset++)
//             {
//                 if (xOffset == 0 && yOffset == 0)
//                 {
//                     continue;
//                 }

//                 Vector3Int candidateCoordinate =
//                     new Vector3Int(
//                         sourceCoordinate.x + xOffset,
//                         sourceCoordinate.y + yOffset,
//                         targetDepth
//                     );

//                 if (!TryGetLockedDepthObject(
//                         candidateCoordinate,
//                         tableIndex,
//                         source,
//                         out PhysicsTowerObject candidate))
//                 {
//                     continue;
//                 }

//                 float score =
//                     xOffset * xOffset +
//                     yOffset * yOffset * 1.5f;

//                 if (score >= bestScore)
//                 {
//                     continue;
//                 }

//                 bestScore = score;
//                 linkedObject = candidate;
//                 linkedCoordinate = candidateCoordinate;
//             }
//         }

//         return linkedObject != null;
//     }


//     private bool TryGetLockedDepthObject(
//         Vector3Int coordinate,
//         int tableIndex,
//         PhysicsTowerObject source,
//         out PhysicsTowerObject candidate)
//     {
//         return occupancyByCoordinate.TryGetValue(
//                    (tableIndex, coordinate),
//                    out candidate) &&
//                candidate != null &&
//                candidate != source &&
//                candidate.IsLocked;
//     }


//     private IEnumerator ActivateMirroredObjectNaturally(
//         PhysicsTowerObject mirroredObject,
//         Vector3Int mirroredCoordinate,
//         int tableIndex,
//         Vector3 sourceImpulse,
//         int activationRevision)
//     {
//         float minimumDelay =
//             Mathf.Max(0f, mirroredActivationDelayRange.x);

//         float maximumDelay =
//             Mathf.Max(minimumDelay, mirroredActivationDelayRange.y);

//         float delay =
//             Random.Range(minimumDelay, maximumDelay);

//         if (delay > 0f)
//         {
//             yield return new WaitForSeconds(delay);
//         }

//         if (activationRevision != levelGenerationRevision ||
//             mirroredObject == null ||
//             !mirroredObject.IsLocked ||
//             !occupancyByCoordinate.TryGetValue(
//                 (tableIndex, mirroredCoordinate),
//                 out PhysicsTowerObject currentObject) ||
//             currentObject != mirroredObject)
//         {
//             yield break;
//         }

//         float minimumStrength =
//             Mathf.Max(0f, mirroredImpulseVariation.x);

//         float maximumStrength =
//             Mathf.Max(minimumStrength, mirroredImpulseVariation.y);

//         float randomStrength =
//             Random.Range(minimumStrength, maximumStrength);

//         Vector3 randomAngles =
//             new Vector3(
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 )
//             );

//         Vector3 variedImpulse =
//             Quaternion.Euler(randomAngles) *
//             sourceImpulse *
//             mirroredLayerImpulseMultiplier *
//             randomStrength;

//         Vector3 randomTorque =
//             sourceImpulse.sqrMagnitude > 0.0001f
//                 ? Random.onUnitSphere *
//                   sourceImpulse.magnitude *
//                   mirroredRandomTorqueMultiplier *
//                   Random.Range(0.65f, 1.25f)
//                 : Vector3.zero;

//         isPropagatingMirroredActivation = true;

//         try
//         {
//             mirroredObject.ActivatePhysics(
//                 variedImpulse,
//                 randomTorque
//             );
//         }
//         finally
//         {
//             isPropagatingMirroredActivation = false;
//         }
//     }


//     private void HandleObjectCleared(
//         PhysicsTowerObject target)
//     {
//         if (target == null)
//         {
//             return;
//         }


//         target.Cleared -=
//             HandleObjectCleared;

//         target.Activated -=
//             HandleObjectActivated;


//         activeObjects.Remove(
//             target
//         );

//         trackingByInstance.Remove(
//             target
//         );

//         /*
//          * Multi-cell footprint wale piece ke liye occupancyByCoordinate
//          * mein uske SAARE covered cells target ko point karte hain
//          * (sirf anchor nahi) — is liye scan karke sab remove karte hain,
//          * warna stale entries agle level mein reused instance ko
//          * galat tarah "still standing" dikha sakti hain.
//          */
//         List<(int tableIndex, Vector3Int coordinate)> coordinatesToRemove = null;

//         foreach (KeyValuePair<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject> entry
//                  in occupancyByCoordinate)
//         {
//             if (entry.Value != target)
//             {
//                 continue;
//             }

//             if (coordinatesToRemove == null)
//             {
//                 coordinatesToRemove =
//                     new List<(int, Vector3Int)>();
//             }

//             coordinatesToRemove.Add(entry.Key);
//         }

//         if (coordinatesToRemove != null)
//         {
//             foreach ((int tableIndex, Vector3Int coordinate) key
//                      in coordinatesToRemove)
//             {
//                 occupancyByCoordinate.Remove(key);
//             }
//         }


//         if (target.CountsAsTarget)
//         {
//             remainingTargets =
//                 Mathf.Max(
//                     0,
//                     remainingTargets - 1
//                 );
//         }


//         objectPool.Release(
//             target
//         );


//         if (levelGenerated &&
//             remainingTargets == 0)
//         {
//             CompleteLevel();
//         }
//     }


//     private void CompleteLevel()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }


//         levelGenerated =
//             false;


//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} COMPLETE",
//             this
//         );

// //  if (milestoneProgressBar != null)
// //     {
// //         milestoneProgressBar.OnLevelCompleted(
// //             levelData.LevelNumber
// //         );
// //     }


//         onLevelComplete?.Invoke();

//         ShowLevelCompleteUI();

//         if (autoLoadNextLevel &&
//             HasNextLevel &&
//             !IsLiveWorkingLevelPreviewEnabled())
//         {
//             StartCoroutine(
//                 LoadNextLevelAfterCompletion(
//                     currentLevelIndex
//                 )
//             );
//         }
//     }


//     private IEnumerator LoadNextLevelAfterCompletion(
//         int completedLevelIndex)
//     {
//         if (autoLoadNextLevelDelay > 0f)
//         {
//             yield return new WaitForSecondsRealtime(
//                 autoLoadNextLevelDelay
//             );
//         }

//         /*
//          * Player ne delay ke duran restart/next button use kar liya ho to
//          * purani completion routine naye level ko dobara skip na kare.
//          */
//         if (currentLevelIndex != completedLevelIndex ||
//             levelGenerated ||
//             isLoadingLevel)
//         {
//             yield break;
//         }

//         LoadNextLevel();
//     }


//     public void LoadNextLevel()
//     {
//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int nextIndex =
//             FindNextLevelIndex(
//                 currentLevelIndex + 1
//             );

//         if (nextIndex < 0)
//         {
//             Debug.LogWarning(
//                 "Next Addressable level configured nahi hai. Level " +
//                 "Creator se next level SAVE karein.",
//                 this
//             );

//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(nextIndex)
//         );
//     }

//     public void RestartCurrentLevel()
//     {
//         if (isLoadingLevel ||
//             levelData == null)
//         {
//             return;
//         }

//         DestroyActiveCannonBalls();
//         GenerateLevel();
//     }


//     private IEnumerator LoadAddressableLevelRoutine(
//         int levelIndex)
//     {
//         if (isLoadingLevel ||
//             levelDatabase == null)
//         {
//             yield break;
//         }

//         string address =
//             levelDatabase.GetAddress(levelIndex);

//         if (string.IsNullOrWhiteSpace(address))
//         {
//             Debug.LogError(
//                 $"Level index {levelIndex} ka Addressable address missing hai.",
//                 this
//             );
//             yield break;
//         }

//         isLoadingLevel = true;
//         RefreshNextLevelButton();

//         AsyncOperationHandle<GridLevelData> newHandle =
//             Addressables.LoadAssetAsync<GridLevelData>(address);

//         pendingLevelHandle = newHandle;
//         hasPendingLevelHandle = true;

//         yield return newHandle;

//         if (newHandle.Status != AsyncOperationStatus.Succeeded ||
//             newHandle.Result == null)
//         {
//             Debug.LogError(
//                 $"Addressable level load failed: {address}",
//                 this
//             );

//             if (newHandle.IsValid())
//             {
//                 Addressables.Release(newHandle);
//             }

//             hasPendingLevelHandle = false;
//             isLoadingLevel = false;
//             RefreshNextLevelButton();
//             yield break;
//         }

//         /*
//          * Pehle purane instantiated objects destroy hote hain. Ek frame
//          * baad purana Addressables handle release hota hai, isliye bundle
//          * unload ke waqt koi live prefab/material instance nahi rehta.
//          */
//         PrepareForLevelAssetRelease();
//         yield return null;
//         ReleaseActiveLevelHandle();

//         activeLevelHandle = newHandle;
//         hasActiveLevelHandle = true;
//         hasPendingLevelHandle = false;
//         currentLevelIndex = levelIndex;
//         levelData = newHandle.Result;
//         isLoadingLevel = false;

//         SaveCurrentLevelProgress(
//             levelData.LevelNumber
//         );

//         GenerateLevel();
//     }


//     public void LoadLevelByNumber(int levelNumber)
//     {
//         if (levelDatabase == null)
//         {
//             Debug.LogError(
//                 "LoadLevelByNumber: GridLevelDatabase missing hai.",
//                 this
//             );
//             return;
//         }

//         if (levelNumber >
//             levelDatabase.MaximumPlayableLevelNumber)
//         {
//             Debug.LogWarning(
//                 $"Level {levelNumber} playable range se bahar hai. " +
//                 $"Last playable level " +
//                 $"{levelDatabase.MaximumPlayableLevelNumber} hai.",
//                 this
//             );
//             return;
//         }

//         int levelIndex =
//             levelDatabase.FindIndexByLevelNumber(levelNumber);

//         if (levelIndex < 0)
//         {
//             Debug.LogWarning(
//                 $"Addressable Level {levelNumber} catalog mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(levelIndex)
//         );
//     }


//     private void SelectStartingLevel()
//     {
//         if (levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             currentLevelIndex = 0;
//             return;
//         }

//         if (PlayerPrefs.HasKey(SavedLevelNumberKey))
//         {
//             int savedLevelNumber =
//                 PlayerPrefs.GetInt(
//                     SavedLevelNumberKey,
//                     1
//                 );

//             int savedLevelIndex =
//                 levelDatabase.FindIndexByLevelNumber(
//                     savedLevelNumber
//                 );

//             if (savedLevelIndex >= 0 &&
//                 FindNextLevelIndex(savedLevelIndex) ==
//                 savedLevelIndex)
//             {
//                 currentLevelIndex = savedLevelIndex;
//                 return;
//             }
//         }

//         if (startFromLevelOne)
//         {
//             int firstLevelIndex =
//                 FindNextLevelIndex(0);

//             if (firstLevelIndex >= 0)
//             {
//                 currentLevelIndex = firstLevelIndex;
//             }

//             return;
//         }

//         int configuredIndex =
//             levelData != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     levelData.LevelNumber
//                 )
//                 : -1;

//         currentLevelIndex =
//             configuredIndex >= 0
//                 ? configuredIndex
//                 : Mathf.Max(0, FindNextLevelIndex(0));

//     }


//     private static bool IsLiveWorkingLevelPreviewEnabled()
//     {
// #if UNITY_EDITOR
//         return UnityEditor.SessionState.GetBool(
//             "RoyalSmash.LiveWorkingLevelPreview",
//             false
//         );
// #else
//         return false;
// #endif
//     }


//     private static bool ShouldStartLiveWorkingLevelPreview()
//     {
// #if UNITY_EDITOR
//         const string previewSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreview";

//         const string previewLaunchSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreviewLaunch";

//         bool previewEnabled =
//             UnityEditor.SessionState.GetBool(
//                 previewSessionKey,
//                 false
//             );

//         bool launchRequested =
//             UnityEditor.SessionState.GetBool(
//                 previewLaunchSessionKey,
//                 false
//             );

//         UnityEditor.SessionState.SetBool(
//             previewLaunchSessionKey,
//             false
//         );

//         if (!launchRequested)
//         {
//             /*
//              * Toolbar se Play karne par pichli editor preview session
//              * normal game startup ko bypass nahi kar sakti.
//              */
//             UnityEditor.SessionState.SetBool(
//                 previewSessionKey,
//                 false
//             );
//         }

//         return previewEnabled && launchRequested;
// #else
//         return false;
// #endif
//     }


//     private int FindNextLevelIndex(
//         int startingIndex)
//     {
//         if (levelDatabase == null)
//         {
//             return -1;
//         }

//         for (int i = Mathf.Max(0, startingIndex);
//              i < levelDatabase.Count;
//              i++)
//         {
//             int levelNumber =
//                 levelDatabase.GetLevelNumber(i);

//             if (levelNumber >
//                 levelDatabase.MaximumPlayableLevelNumber)
//             {
//                 break;
//             }

//             if (levelNumber > 0 &&
//                 !string.IsNullOrWhiteSpace(
//                     levelDatabase.GetAddress(i)))
//             {
//                 return i;
//             }
//         }

//         return -1;
//     }


//     private void EnsureMainMenuUI()
//     {
//         if (mainMenuPanel == null)
//         {
//             RectTransform[] sceneRects =
//                 FindObjectsByType<RectTransform>(
//                     FindObjectsInactive.Include,
//                     FindObjectsSortMode.None
//                 );

//             foreach (RectTransform sceneRect in sceneRects)
//             {
//                 if (sceneRect != null &&
//                     sceneRect.name.Equals(
//                         "Main menu Panel",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ))
//                 {
//                     mainMenuPanel = sceneRect.gameObject;
//                     break;
//                 }
//             }
//         }

//         if (mainMenuPanel == null)
//         {
//             Debug.LogWarning(
//                 "Main menu Panel scene mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         Button[] menuButtons =
//             mainMenuPanel.GetComponentsInChildren<Button>(true);

//         foreach (Button menuButton in menuButtons)
//         {
//             if (menuButton == null)
//             {
//                 continue;
//             }

//             string buttonName =
//                 menuButton.name.ToLowerInvariant();

//             if (playButton == null &&
//                 buttonName.Contains("play"))
//             {
//                 playButton = menuButton;
//             }
//             else if (resetProgressButton == null &&
//                      buttonName.Contains("reset"))
//             {
//                 resetProgressButton = menuButton;
//             }
//         }

//         if (currentLevelText == null)
//         {
//             TMP_Text[] menuTexts =
//                 mainMenuPanel.GetComponentsInChildren<TMP_Text>(true);

//             foreach (TMP_Text menuText in menuTexts)
//             {
//                 if (menuText != null &&
//                     menuText.text.IndexOf(
//                         "level number",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ) >= 0)
//                 {
//                     currentLevelText = menuText;
//                     break;
//                 }
//             }
//         }

//         if (playButton != null)
//         {
//             playButton.onClick.RemoveListener(
//                 PlayFromMainMenu
//             );
//             playButton.onClick.AddListener(
//                 PlayFromMainMenu
//             );
//         }

//         if (resetProgressButton != null)
//         {
//             resetProgressButton.onClick.RemoveListener(
//                 ResetProgressFromMainMenu
//             );
//             resetProgressButton.onClick.AddListener(
//                 ResetProgressFromMainMenu
//             );
//         }
//     }


//     private void SetGameplayWorldVisible(bool visible)
//     {
//         if (levelOrigin != null &&
//             levelOrigin != transform)
//         {
//             levelOrigin.gameObject.SetActive(visible);
//         }

//         if (runtimeObjectsRoot != null)
//         {
//             runtimeObjectsRoot.gameObject.SetActive(visible);
//         }

//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (cannonController != null)
//         {
//             cannonController.SetGameplayActive(visible);
//         }

//         if (gameplayRestartButton == null)
//         {
//             GameRestartController restartController =
//                 FindFirstObjectByType<GameRestartController>(
//                     FindObjectsInactive.Include
//                 );

//             if (restartController != null)
//             {
//                 gameplayRestartButton =
//                     restartController.gameObject;
//             }
//         }

//         if (gameplayRestartButton != null)
//         {
//             gameplayRestartButton.SetActive(visible);
//         }

//         if (!visible)
//         {
//             foreach (LevelTable table in cachedTables)
//             {
//                 if (table != null)
//                 {
//                     table.gameObject.SetActive(false);
//                 }
//             }

//             if (nextLevelButton != null)
//             {
//                 nextLevelButton.gameObject.SetActive(false);
//             }
//         }
//     }


//     private void SaveCurrentLevelProgress(int levelNumber)
//     {
//         if (levelNumber <= 0 ||
//             IsLiveWorkingLevelPreviewEnabled())
//         {
//             return;
//         }

//         PlayerPrefs.SetInt(
//             SavedLevelNumberKey,
//             levelNumber
//         );
//         PlayerPrefs.Save();
//         UpdateCurrentLevelText();
//     }


//     private void UpdateCurrentLevelText()
//     {
//         if (currentLevelText == null ||
//             levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             return;
//         }

//         int safeIndex =
//             Mathf.Clamp(
//                 currentLevelIndex,
//                 0,
//                 levelDatabase.Count - 1
//             );

//         int levelNumber =
//             levelDatabase.GetLevelNumber(safeIndex);

//         currentLevelText.text =
//             $"LEVEL {Mathf.Max(1, levelNumber)}";
//     }


//     private void EnsureNextLevelUI()
//     {
//         if (nextLevelButton != null)
//         {
//             nextLevelButton.onClick.RemoveListener(
//                 LoadNextLevel
//             );

//             nextLevelButton.onClick.AddListener(
//                 LoadNextLevel
//             );
//         }

//         if ((nextLevelButton != null &&
//              levelCompleteText != null) ||
//             !autoCreateNextLevelUI)
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
//                 candidate.renderMode != RenderMode.WorldSpace)
//             {
//                 targetCanvas = candidate;
//                 break;
//             }
//         }

//         if (targetCanvas == null)
//         {
//             GameObject canvasObject =
//                 new GameObject(
//                     "Level Progress Canvas",
//                     typeof(RectTransform),
//                     typeof(Canvas),
//                     typeof(CanvasScaler),
//                     typeof(GraphicRaycaster)
//                 );

//             int uiLayer = LayerMask.NameToLayer("UI");

//             if (uiLayer >= 0)
//             {
//                 canvasObject.layer = uiLayer;
//             }

//             targetCanvas = canvasObject.GetComponent<Canvas>();
//             targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             targetCanvas.sortingOrder = 110;

//             CanvasScaler scaler =
//                 canvasObject.GetComponent<CanvasScaler>();

//             scaler.uiScaleMode =
//                 CanvasScaler.ScaleMode.ScaleWithScreenSize;

//             scaler.referenceResolution =
//                 new Vector2(1080f, 1920f);
//         }

//         int panelLayer = targetCanvas.gameObject.layer;

//         autoCreatedCompletePanel =
//             new GameObject(
//                 "Level Complete Panel",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(Image)
//             );

//         autoCreatedCompletePanel.layer = panelLayer;

//         RectTransform panelRect =
//             autoCreatedCompletePanel.GetComponent<RectTransform>();

//         panelRect.SetParent(targetCanvas.transform, false);
//         panelRect.anchorMin = new Vector2(0.5f, 0.5f);
//         panelRect.anchorMax = new Vector2(0.5f, 0.5f);
//         panelRect.pivot = new Vector2(0.5f, 0.5f);
//         panelRect.anchoredPosition = Vector2.zero;
//         panelRect.sizeDelta = new Vector2(620f, 180f);

//         Image panelImage =
//             autoCreatedCompletePanel.GetComponent<Image>();

//         panelImage.color = new Color(0.03f, 0.04f, 0.07f, 0.92f);

//         GameObject titleObject =
//             new GameObject(
//                 "Level Complete Text",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         titleObject.layer = panelLayer;

//         RectTransform titleRect =
//             titleObject.GetComponent<RectTransform>();

//         titleRect.SetParent(panelRect, false);
//         titleRect.anchorMin = new Vector2(0f, 1f);
//         titleRect.anchorMax = new Vector2(1f, 1f);
//         titleRect.pivot = new Vector2(0.5f, 1f);
//         titleRect.anchoredPosition = new Vector2(0f, -38f);
//         titleRect.sizeDelta = new Vector2(-40f, 100f);

//         TextMeshProUGUI title =
//             titleObject.GetComponent<TextMeshProUGUI>();

//         title.fontSize = 48f;
//         title.fontStyle = FontStyles.Bold;
//         title.alignment = TextAlignmentOptions.Center;
//         title.color = Color.white;
//         title.raycastTarget = false;

//         levelCompleteText = title;

//         GameObject buttonObject =
//             new GameObject(
//                 "Next Level Button",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(Image),
//                 typeof(Button)
//             );

//         buttonObject.layer = panelLayer;

//         RectTransform buttonRect =
//             buttonObject.GetComponent<RectTransform>();

//         /*
//          * NEXT LEVEL gameplay ke duran bhi visible rehna chahiye,
//          * isliye button complete panel ka child nahi. Panel hide hone
//          * par sirf completion title hide hota hai, button nahi.
//          */
//         buttonRect.SetParent(targetCanvas.transform, false);
//         buttonRect.anchorMin = new Vector2(1f, 1f);
//         buttonRect.anchorMax = new Vector2(1f, 1f);
//         buttonRect.pivot = new Vector2(1f, 1f);
//         buttonRect.anchoredPosition = new Vector2(-32f, -32f);
//         buttonRect.sizeDelta = new Vector2(390f, 88f);

//         Image buttonImage = buttonObject.GetComponent<Image>();
//         buttonImage.color = new Color(0.15f, 0.62f, 1f, 1f);

//         nextLevelButton = buttonObject.GetComponent<Button>();
//         nextLevelButton.targetGraphic = buttonImage;
//         nextLevelButton.onClick.AddListener(LoadNextLevel);

//         GameObject labelObject =
//             new GameObject(
//                 "Label",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         labelObject.layer = panelLayer;

//         RectTransform labelRect =
//             labelObject.GetComponent<RectTransform>();

//         labelRect.SetParent(buttonRect, false);
//         labelRect.anchorMin = Vector2.zero;
//         labelRect.anchorMax = Vector2.one;
//         labelRect.offsetMin = Vector2.zero;
//         labelRect.offsetMax = Vector2.zero;

//         TextMeshProUGUI label =
//             labelObject.GetComponent<TextMeshProUGUI>();

//         label.text = "NEXT LEVEL";
//         label.fontSize = 38f;
//         label.fontStyle = FontStyles.Bold;
//         label.alignment = TextAlignmentOptions.Center;
//         label.color = Color.white;
//         label.raycastTarget = false;

//         RefreshNextLevelButton();
//     }


//     private void ShowLevelCompleteUI()
//     {
//         EnsureNextLevelUI();

//         bool hasNext = HasNextLevel;

//         if (levelCompleteText != null)
//         {
//             levelCompleteText.text =
//                 hasNext
//                     ? $"LEVEL {levelData.LevelNumber} COMPLETE"
//                     : "ALL LEVELS COMPLETE";
//         }

//         if (nextLevelButton != null)
//         {
//             RefreshNextLevelButton();
//         }

//         SetLevelCompleteUIVisible(true);
//     }


//     private void SetLevelCompleteUIVisible(
//         bool visible)
//     {
//         if (autoCreatedCompletePanel != null)
//         {
//             autoCreatedCompletePanel.SetActive(visible);
//         }
//         else if (levelCompleteText != null)
//         {
//             levelCompleteText.gameObject.SetActive(visible);
//         }

//         if (nextLevelButton != null)
//         {
//             nextLevelButton.gameObject.SetActive(true);
//         }
//     }


//     private void RefreshNextLevelButton()
//     {
//         if (nextLevelButton == null)
//         {
//             return;
//         }

//         int nextIndex =
//             FindNextLevelIndex(
//                 currentLevelIndex + 1
//             );

//         bool hasNext = nextIndex >= 0;
//         nextLevelButton.gameObject.SetActive(true);
//         nextLevelButton.interactable =
//             hasNext && !isLoadingLevel;

//         TMP_Text buttonLabel =
//             nextLevelButton.GetComponentInChildren<TMP_Text>(true);

//         if (buttonLabel == null)
//         {
//             return;
//         }

//         if (!hasNext)
//         {
//             buttonLabel.text = "NO NEXT LEVEL";
//             return;
//         }

//         buttonLabel.text =
//             isLoadingLevel
//                 ? "LOADING..."
//                 : $"NEXT LEVEL " +
//                   $"{levelDatabase.GetLevelNumber(nextIndex)}";
//     }


//     private void PrepareForLevelAssetRelease()
//     {
//         ClearCurrentLevel();
//         DestroyActiveCannonBalls();

//         if (objectPool != null)
//         {
//             objectPool.ClearAll();
//         }

//         for (int index = cachedTables.Count - 1;
//              index >= 0;
//              index--)
//         {
//             DestroyCachedTableAt(index);
//         }

//         cachedTables.Clear();
//         cachedTablePrefabs.Clear();
//         currentTable = null;
//     }


//     private static void DestroyActiveCannonBalls()
//     {
//         CannonBallMarker[] cannonBalls =
//             FindObjectsByType<CannonBallMarker>(
//                 FindObjectsInactive.Include,
//                 FindObjectsSortMode.None
//             );

//         foreach (CannonBallMarker cannonBall in cannonBalls)
//         {
//             if (cannonBall == null)
//             {
//                 continue;
//             }

//             if (Application.isPlaying)
//             {
//                 Destroy(cannonBall.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(cannonBall.gameObject);
//             }
//         }
//     }


//     private void ReleaseActiveLevelHandle()
//     {
//         if (!hasActiveLevelHandle)
//         {
//             return;
//         }

//         if (activeLevelHandle.IsValid())
//         {
//             Addressables.Release(activeLevelHandle);
//         }

//         hasActiveLevelHandle = false;
//     }


//     public void ClearCurrentLevel()
//     {
//         levelGenerationRevision++;

//         levelGenerated =
//             false;


//         for (int i =
//                  activeObjects.Count - 1;
//              i >= 0;
//              i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance == null)
//             {
//                 continue;
//             }


//             instance.Cleared -=
//                 HandleObjectCleared;

//             instance.Activated -=
//                 HandleObjectActivated;


//             if (objectPool != null)
//             {
//                 objectPool.Release(
//                     instance
//                 );
//             }
//         }


//         activeObjects.Clear();

//         occupancyByCoordinate.Clear();

//         trackingByInstance.Clear();

//         gridBaseRowByTable.Clear();

//         /*
//          * Fit cache CellSize-specific hai — agar LoadLevel() se
//          * different GridLevelData aa jaye (alag CellSize) to purani
//          * cached scale ghalat hogi.
//          */
//         fitCache.Clear();


//         remainingTargets =
//             0;


//         foreach (LevelTable table in cachedTables)
//         {
//             if (table != null)
//             {
//                 table.gameObject.SetActive(false);
//             }
//         }


//         currentTable =
//             null;
//     }


//     private Transform GetLevelOrigin()
//     {
//         return
//             levelOrigin != null
//                 ? levelOrigin
//                 : transform;
//     }


//     private bool ValidateReferences()
//     {
//         if (levelData == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (!levelData.HasAnyValidTableGrid)
//         {
//             Debug.LogError(
//                 "Manual GridLevelData mein koi painted cell nahi hai. " +
//                 "Level Creator mein grid allocate karke shapes paint karein.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.BlockPalette == null ||
//             levelData.BlockPalette.Count == 0)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein Block Palette empty hai.",
//                 levelData
//             );

//             return false;
//         }

//         bool hasValidPaletteEntry =
//             false;

//         foreach (PhysicsObjectDefinition entry
//                  in levelData.BlockPalette)
//         {
//             if (entry != null &&
//                 entry.Prefab != null)
//             {
//                 hasValidPaletteEntry =
//                     true;

//                 break;
//             }
//         }

//         if (!hasValidPaletteEntry)
//         {
//             Debug.LogError(
//                 "Grid Level Data ki Block Palette mein koi valid " +
//                 "Prefab wali entry nahi hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.TableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (objectPool == null)
//         {
//             Debug.LogError(
//                 "Physics Object Pool missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (runtimeObjectsRoot == null)
//         {
//             Debug.LogError(
//                 "Runtime Objects Root missing hai.",
//                 this
//             );

//             return false;
//         }


//         Vector3 runtimeScale =
//             runtimeObjectsRoot.lossyScale;


//         if (Mathf.Abs(runtimeScale.x - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.y - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.z - 1f) > 0.01f)
//         {
//             Debug.LogWarning(
//                 "RuntimeObjects ki world Scale ideally 1,1,1 honi chahiye.",
//                 runtimeObjectsRoot
//             );
//         }


//         return true;
//     }


//     private void OnDestroy()
//     {
//         for (int i = 0;
//              i < activeObjects.Count;
//              i++)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance != null)
//             {
//                 instance.Cleared -=
//                     HandleObjectCleared;

//                 instance.Activated -=
//                     HandleObjectActivated;
//             }
//         }

//         ReleaseActiveLevelHandle();

//         if (hasPendingLevelHandle &&
//             pendingLevelHandle.IsValid())
//         {
//             Addressables.Release(pendingLevelHandle);
//         }

//         hasPendingLevelHandle = false;
//     }
// }











// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.Events;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.UI;

// public sealed class LevelRuntimeController : MonoBehaviour
// {
//     private const string SavedLevelNumberKey =
//         "RoyalSmash.CurrentLevelNumber";

//     public event System.Action<GridLevelData> LevelGenerated;






// // [Header("Level Progress")]
// // [SerializeField]
// // private LevelMilestoneProgressBar milestoneProgressBar;









//     [Header("Level")]

//     [SerializeField]
//     private GridLevelData levelData;

//     [Tooltip(
//         "Level Creator ke banaye tamam levels ki central database. " +
//         "Runtime mein individual levels manually assign nahi karne."
//     )]
//     [SerializeField]
//     private GridLevelDatabase levelDatabase;

//     [SerializeField]
//     private bool startFromLevelOne = true;


//     [Header("Next Level UI")]

//     [SerializeField]
//     private Button nextLevelButton;

//     [SerializeField]
//     private TMP_Text levelCompleteText;

//     [Tooltip(
//         "References missing hon to runtime par complete popup aur " +
//         "NEXT LEVEL button khud create karega."
//     )]
//     [SerializeField]
//     private bool autoCreateNextLevelUI = true;

//     [Tooltip(
//         "Level complete hote hi database ka agla saved level " +
//         "automatically load kare."
//     )]
//     [SerializeField]
//     private bool autoLoadNextLevel = true;

//     [Tooltip(
//         "Completion message dikhane ke baad next level load hone ka delay."
//     )]
//     [SerializeField, Min(0f)]
//     private float autoLoadNextLevelDelay = 1.25f;


//     [Header("Main Menu")]

//     [SerializeField]
//     private bool showMainMenuOnStartup = true;

//     [SerializeField]
//     private GameObject mainMenuPanel;

//     [Tooltip(
//         "Gameplay UI panel. Main Menu par hidden rahega aur Play press karte hi " +
//         "gameplay world ke sath active ho jayega."
//     )]
//     [SerializeField]
//     private GameObject gameplayPanel;

//     [SerializeField]
//     private Button playButton;

//     [SerializeField]
//     private Button resetProgressButton;

//     [SerializeField]
//     private TMP_Text currentLevelText;


//     [Header("References")]

//     [SerializeField]
//     private PhysicsObjectPool objectPool;

//     [SerializeField]
//     private Transform levelOrigin;

//     [SerializeField]
//     private Transform runtimeObjectsRoot;


//     [Header("Support Chain")]

//     [Tooltip(
//         "Support khone ke kitne FixedUpdate frames baad upar wala " +
//         "block khud bhi dynamic ho jayega."
//     )]
//     [SerializeField, Range(1, 10)]
//     private int unsupportedFramesRequired = 2;

//     [Tooltip(
//         "Half-supported piece ko missing-support side par tip karne wala " +
//         "small sideways impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipImpulse = 0.18f;

//     [Tooltip(
//         "Exact edge-balance ko break karne wala natural rotation impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipTorque = 0.28f;


//     [Header("Mirrored Depth Physics")]

//     [Tooltip(
//         "Kisi bhi 2+ depth-layer level mein hit block ka impact same X/Y " +
//         "par maujood baqi depth-layer blocks ko bhi unlock aur push karega. " +
//         "Ye gameplay link Mirror Paint authoring option se independent hai."
//     )]
//     [SerializeField]
//     private bool linkMirroredDepthLayerPhysics = true;

//     [Tooltip(
//         "Front block ke impulse ka kitna hissa mirrored layers ko mile."
//     )]
//     [SerializeField, Range(0f, 1.5f)]
//     private float mirroredLayerImpulseMultiplier = 0.45f;

//     [Tooltip(
//         "Mirrored depth layers exact same frame par na giren. Har linked " +
//         "block ko is range mein chhota natural delay milta hai."
//     )]
//     [SerializeField]
//     private Vector2 mirroredActivationDelayRange =
//         new Vector2(0.09f, 0.22f);

//     [Tooltip(
//         "Mirrored impulse ki strength mein per-block random variation."
//     )]
//     [SerializeField]
//     private Vector2 mirroredImpulseVariation =
//         new Vector2(0.78f, 0.96f);

//     [Tooltip(
//         "Mirrored force direction mein maximum random angle variation."
//     )]
//     [SerializeField, Range(0f, 25f)]
//     private float mirroredDirectionVariationDegrees = 4f;

//     [Tooltip(
//         "Linked blocks ko alag natural spin dene ke liye random torque."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float mirroredRandomTorqueMultiplier = 0.035f;


//     [Header("Events")]

//     [SerializeField]
//     private UnityEvent onLevelGenerated;

//     [SerializeField]
//     private UnityEvent onLevelComplete;


//     private sealed class DefinitionFitData
//     {
//         public Vector3 Scale;
//         public Vector3 BoundsCenterOffset;
//     }


//     private sealed class GridTrackingEntry
//     {
//         public Vector3 InitialPosition;
//         public int UnsupportedFrames;
//         public int TableIndex;
//         public Quaternion SurfaceRotation;
//     }


//     private readonly List<PhysicsTowerObject>
//         activeObjects =
//             new List<PhysicsTowerObject>();

//     /// <summary>
//     /// Keyed by (definition, span) since a bigger footprint (e.g. a
//     /// 2-cell-tall piece) needs its own fit scale, distinct from the
//     /// same definition used at 1x1x1.
//     /// </summary>
//     private readonly Dictionary<(PhysicsObjectDefinition, int spanKey), DefinitionFitData>
//         fitCache =
//             new Dictionary<(PhysicsObjectDefinition, int), DefinitionFitData>();

//     /// <summary>
//     /// What currently occupies each grid cell — used by the support
//     /// chain to answer "is the cell below me still standing?".
//     /// </summary>
//     private readonly Dictionary<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject>
//         occupancyByCoordinate =
//             new Dictionary<(int, Vector3Int), PhysicsTowerObject>();

//     private readonly Dictionary<PhysicsTowerObject, GridTrackingEntry>
//         trackingByInstance =
//             new Dictionary<PhysicsTowerObject, GridTrackingEntry>();


//     private readonly List<LevelTable> cachedTables =
//         new List<LevelTable>();

//     private readonly List<LevelTable> cachedTablePrefabs =
//         new List<LevelTable>();

//     private readonly List<int> tablePhysicsReleaseCycleTargets =
//         new List<int>();

//     private readonly HashSet<int> releasedTablePhysicsIndices =
//         new HashSet<int>();

//     private LevelTable currentTable;


//     private int remainingTargets;

//     private bool levelGenerated;

//     private bool isPropagatingMirroredActivation;

//     private int levelGenerationRevision;

//     private float runtimeTableAnimationTime;

//     private int currentLevelIndex;

//     private AsyncOperationHandle<GridLevelData>
//         activeLevelHandle;

//     private bool hasActiveLevelHandle;

//     private AsyncOperationHandle<GridLevelData>
//         pendingLevelHandle;

//     private bool hasPendingLevelHandle;

//     private bool isLoadingLevel;

//     private GameObject autoCreatedCompletePanel;

//     private CannonController cannonController;

//     private GameObject gameplayRestartButton;

//     /// <summary>
//     /// Lowest occupied grid row for the current level — the "floor"
//     /// row that always counts as supported by the table, even if the
//     /// designer didn't start painting at y=0.
//     /// </summary>
//     private readonly Dictionary<int, int> gridBaseRowByTable =
//         new Dictionary<int, int>();


//     public GridLevelData CurrentLevelData =>
//         levelData;

//     public LevelTable CurrentTable =>
//         currentTable;

//     public IReadOnlyList<LevelTable> CurrentTables =>
//         cachedTables;

//     public int RemainingTargets =>
//         remainingTargets;

//     public bool IsLevelGenerated =>
//         levelGenerated;

//     public int CurrentLevelIndex =>
//         currentLevelIndex;

//     public bool HasNextLevel =>
//         FindNextLevelIndex(currentLevelIndex + 1) >= 0;


//     private void Start()
//     {
//         if (ShouldStartLiveWorkingLevelPreview())
//         {
//             EnsureNextLevelUI();
//             SetLevelCompleteUIVisible(false);
//             currentLevelIndex = 0;
//             GenerateLevel();
//             return;
//         }

//         SelectStartingLevel();

//         if (FindNextLevelIndex(currentLevelIndex) < 0)
//         {
//             Debug.LogError(
//                 "Addressable level catalog empty hai. Level Creator se " +
//                 "kam az kam ek level SAVE karein.",
//                 this
//             );
//             RefreshNextLevelButton();
//             return;
//         }

//         if (showMainMenuOnStartup)
//         {
//             ShowMainMenu();
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ShowMainMenu()
//     {
//         EnsureMainMenuUI();
//         SetGameplayWorldVisible(false);
//         UpdateCurrentLevelText();

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(true);
//         }
//     }


//     public void PlayFromMainMenu()
//     {
//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int playableIndex =
//             FindNextLevelIndex(currentLevelIndex);

//         if (playableIndex < 0)
//         {
//             Debug.LogError(
//                 "Play: koi saved Addressable level configured nahi hai.",
//                 this
//             );
//             return;
//         }

//         currentLevelIndex = playableIndex;

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(false);
//         }

//         SetGameplayWorldVisible(true);

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ResetProgressFromMainMenu()
//     {
//         PlayerPrefs.DeleteKey(SavedLevelNumberKey);
//         PlayerPrefs.Save();

//         int firstLevelIndex =
//             FindNextLevelIndex(0);

//         currentLevelIndex =
//             firstLevelIndex >= 0
//                 ? firstLevelIndex
//                 : 0;

//         UpdateCurrentLevelText();
//     }


//     public void LoadLevel(
//         GridLevelData newLevelData)
//     {
//         if (newLevelData == null)
//         {
//             Debug.LogError(
//                 "LoadLevel: GridLevelData null hai.",
//                 this
//             );

//             return;
//         }


//         if (hasActiveLevelHandle)
//         {
//             PrepareForLevelAssetRelease();
//             ReleaseActiveLevelHandle();
//         }

//         levelData = newLevelData;

//         int sequenceIndex =
//             levelDatabase != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     newLevelData.LevelNumber
//                 )
//                 : -1;

//         if (sequenceIndex >= 0)
//         {
//             currentLevelIndex = sequenceIndex;
//         }

//         SaveCurrentLevelProgress(
//             newLevelData.LevelNumber
//         );


//         GenerateLevel();
//     }


//     [ContextMenu("Generate Level")]
//     public void GenerateLevel()
//     {
//         EnsureNextLevelUI();
//         SetLevelCompleteUIVisible(false);
//         RefreshNextLevelButton();

//         if (!ValidateReferences())
//         {
//             return;
//         }


//         ClearCurrentLevel();

//         runtimeTableAnimationTime = 0f;


//         objectPool.PrepareForLevel(
//             levelData
//         );


//         if (!PrepareTable())
//         {
//             return;
//         }

//         PrepareTablePhysicsReleaseTargets();


//         remainingTargets =
//             0;


//         SpawnGrid();


//         Physics.SyncTransforms();


//         levelGenerated =
//             true;


//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} GENERATED\n" +
//             $"Boxes: {activeObjects.Count}\n" +
//             $"Targets: {remainingTargets}",
//             this
//         );


//         LevelGenerated?.Invoke(levelData);

//         onLevelGenerated?.Invoke();


//         if (remainingTargets <= 0)
//         {
//             Debug.LogWarning(
//                 "Generated level mein koi target nahi mila.",
//                 this
//             );
//         }
//     }


//     /// <summary>
//     /// Measures a definition's prefab once (identity rotation, prefab's
//     /// authored scale) and derives the localScale + pivot-to-collider
//     /// offset needed so its collider bounds exactly fill levelData's
//     /// CellSize. Cached per-definition so mixed shapes (cube, cylinder,
//     /// ...) all land cleanly on the same uniform grid without measuring
//     /// on every single cell spawn.
//     /// </summary>
//     private bool TryGetFitData(
//         PhysicsObjectDefinition definition,
//         int spanX,
//         int spanY,
//         int spanZ,
//         PieceOrientation orientation,
//         out DefinitionFitData fitData)
//     {
//         int spanKey =
//             spanX * 10000 +
//             spanY * 100 +
//             spanZ +
//             (int)orientation * 1000000;

//         (PhysicsObjectDefinition, int) cacheKey =
//             (definition, spanKey);

//         if (fitCache.TryGetValue(
//                 cacheKey,
//                 out fitData))
//         {
//             return true;
//         }

//         fitData = null;

//         PhysicsTowerObject measurementObject =
//             objectPool.Get(
//                 definition,
//                 Vector3.zero,
//                 Quaternion.identity,
//                 runtimeObjectsRoot
//             );

//         if (measurementObject == null)
//         {
//             Debug.LogError(
//                 "Grid object measure karne ke liye pool object nahi mila.",
//                 this
//             );

//             return false;
//         }

//         Physics.SyncTransforms();

//         if (!measurementObject.TryGetPhysicsBounds(
//                 out Bounds bounds))
//         {
//             Debug.LogError(
//                 "Physics object par valid non-trigger Collider nahi mila.",
//                 measurementObject
//             );

//             objectPool.Release(measurementObject);

//             return false;
//         }

//         Vector3 measuredSize =
//             bounds.size;

//         Vector3 measuredCenterOffset =
//             bounds.center -
//             measurementObject.transform.position;

//         objectPool.Release(measurementObject);

//         if (measuredSize.x <= 0.0001f ||
//             measuredSize.y <= 0.0001f ||
//             measuredSize.z <= 0.0001f)
//         {
//             Debug.LogError(
//                 $"{definition.DisplayName}: collider size invalid hai.",
//                 this
//             );

//             return false;
//         }

//         Vector3 authoredScale =
//             definition.Prefab.transform.localScale;

//         Vector3 finalScale;

//         Vector3 finalOffset;

//         if (definition.AutoFitToCell)
//         {
//             /*
//              * Span > 1 par target size sirf spanCount * cellSize nahi —
//              * internal gaps bhi fill hote hain (spanX*cellSize.x +
//              * (spanX-1)*gap) taake merged piece apne poore footprint
//              * ke outer edges tak seamless dikhe, jaisa cells alag alag
//              * (gap ke sath) hote to unka combined outer span hota.
//              */
//             Vector3 gridAlignedTargetSize =
//                 new Vector3(
//                     spanX * levelData.CellSize.x +
//                     (spanX - 1) * levelData.HorizontalGap,

//                     spanY * levelData.CellSize.y +
//                     (spanY - 1) * levelData.VerticalGap,

//                     spanZ * levelData.CellSize.z +
//                     (spanZ - 1) * levelData.DepthGap
//                 );

//             /*
//              * gridAlignedTargetSize world/grid axes ke liye hai.
//              * Lekin fit-scale prefab ke apne UNROTATED local axes par
//              * compute hoti hai — agar orientation tip kar rahi hai
//              * (Lying X/Z) to local axes rotate hone ke baad world axes
//              * se match nahi karte, is liye target size ko yahan
//              * local axes par re-map karte hain (spawn time par
//              * PieceOrientationUtility.GetRotation() isi mapping ko
//              * wapas world space mein rotate kar degi).
//              */
//             Vector3 targetSize =
//                 PieceOrientationUtility.GetLocalAxisTargetSize(
//                     gridAlignedTargetSize,
//                     orientation
//                 );

//             /*
//              * boundsAtScale1 = measuredSize / authoredScale
//              * finalScale     = targetSize / boundsAtScale1
//              *                = targetSize * authoredScale / measuredSize
//              *
//              * Isi tarah offset bhi authored scale se scale-1 basis par
//              * normalize karke, phir finalScale par re-apply hota hai —
//              * taake off-center collider pivot bhi correct rahe.
//              */
//             finalScale =
//                 new Vector3(
//                     targetSize.x * authoredScale.x / measuredSize.x,
//                     targetSize.y * authoredScale.y / measuredSize.y,
//                     targetSize.z * authoredScale.z / measuredSize.z
//                 );

//             Vector3 offsetAtScale1 =
//                 new Vector3(
//                     measuredCenterOffset.x / authoredScale.x,
//                     measuredCenterOffset.y / authoredScale.y,
//                     measuredCenterOffset.z / authoredScale.z
//                 );

//             finalOffset =
//                 Vector3.Scale(
//                     offsetAtScale1,
//                     finalScale
//                 );
//         }
//         else
//         {
//             finalScale =
//                 authoredScale;

//             finalOffset =
//                 measuredCenterOffset;
//         }

//         finalScale =
//             Vector3.Scale(
//                 finalScale,
//                 definition.ManualScaleMultiplier
//             );

//         fitData =
//             new DefinitionFitData
//             {
//                 Scale = finalScale,
//                 BoundsCenterOffset = finalOffset
//             };

//         fitCache[cacheKey] =
//             fitData;

//         return true;
//     }


//     private void SpawnGrid()
//     {
//         Vector3Int occupiedMin =
//             new Vector3Int(
//                 int.MaxValue,
//                 int.MaxValue,
//                 int.MaxValue
//             );

//         Vector3Int occupiedMax =
//             new Vector3Int(
//                 int.MinValue,
//                 int.MinValue,
//                 int.MinValue
//             );

//         if (currentTable == null ||
//             !currentTable.TryGetTowerSurface(
//                 out Vector3 primarySurfacePosition,
//                 out Quaternion primarySurfaceRotation))
//         {
//             Debug.LogError(
//                 "PRIMARY table ki tower surface calculate nahi ho saki.",
//                 currentTable
//             );

//             return;
//         }

//         int tableCount =
//             Mathf.Max(1, cachedTables.Count);

//         Vector3[] surfacePositions =
//             new Vector3[tableCount];

//         Quaternion[] surfaceRotations =
//             new Quaternion[tableCount];

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             LevelTable table =
//                 tableIndex < cachedTables.Count
//                     ? cachedTables[tableIndex]
//                     : null;

//             if (table != null &&
//                 table.TryGetTowerSurface(
//                     out Vector3 tableSurfacePosition,
//                     out Quaternion tableSurfaceRotation))
//             {
//                 surfacePositions[tableIndex] = tableSurfacePosition;
//                 surfaceRotations[tableIndex] = tableSurfaceRotation;
//             }
//             else
//             {
//                 surfacePositions[tableIndex] = primarySurfacePosition;
//                 surfaceRotations[tableIndex] = primarySurfaceRotation;
//             }
//         }

//         Dictionary<int, Vector3Int> occupiedMinByTable =
//             new Dictionary<int, Vector3Int>();

//         Dictionary<int, Vector3Int> occupiedMaxByTable =
//             new Dictionary<int, Vector3Int>();

//         bool foundAnyOccupiedCell = false;

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             if (!levelData.TryGetOccupiedBounds(
//                     tableIndex,
//                     out Vector3Int tableMinimum,
//                     out Vector3Int tableMaximum))
//             {
//                 continue;
//             }

//             foundAnyOccupiedCell = true;
//             occupiedMinByTable[tableIndex] = tableMinimum;
//             occupiedMaxByTable[tableIndex] = tableMaximum;
//             occupiedMin = Vector3Int.Min(occupiedMin, tableMinimum);
//             occupiedMax = Vector3Int.Max(occupiedMax, tableMaximum);
//         }

//         if (!foundAnyOccupiedCell)
//         {
//             Debug.LogError(
//                 "Kisi bhi table ke manual grid mein occupied cells nahi hain.",
//                 levelData
//             );

//             return;
//         }

//         gridBaseRowByTable.Clear();

//         foreach (KeyValuePair<int, Vector3Int> entry
//                  in occupiedMinByTable)
//         {
//             gridBaseRowByTable[entry.Key] =
//                 entry.Value.y;
//         }


//         Vector3 cellSize =
//             levelData.CellSize;


//         float stepX =
//             cellSize.x +
//             levelData.HorizontalGap;


//         float stepY =
//             cellSize.y +
//             levelData.VerticalGap;


//         float stepZ =
//             cellSize.z +
//             levelData.DepthGap;


//         /*
//          * Important:
//          *
//          * Y outer loop hai.
//          * Isliye logical spawn order bottom → top rahega.
//          */
//         for (int y = occupiedMin.y;
//              y <= occupiedMax.y;
//              y++)
//         {
//             for (int z = occupiedMin.z;
//                  z <= occupiedMax.z;
//                  z++)
//             {
//                 for (int x = occupiedMin.x;
//                      x <= occupiedMax.x;
//                      x++)
//                 {
//                     for (int authoredTableIndex = 0;
//                          authoredTableIndex < tableCount;
//                          authoredTableIndex++)
//                     {
//                         if (!occupiedMinByTable.ContainsKey(
//                                 authoredTableIndex))
//                         {
//                             continue;
//                         }

//                         GridCellData cell =
//                             levelData.GetCell(
//                                 x,
//                                 y,
//                                 z,
//                                 authoredTableIndex
//                             );


//                     if (cell == null ||
//                         !cell.Occupied)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Covered cells (bare footprint ka hissa) khud
//                      * spawn nahi hoti — unka anchor cell hi spawn
//                      * karta hai (neeche is loop mein aage aake).
//                      */
//                     if (cell.IsCovered)
//                     {
//                         continue;
//                     }


//                     int blockTableIndex =
//                         authoredTableIndex;

//                     Vector3Int tableOccupiedMin =
//                         occupiedMinByTable[blockTableIndex];

//                     Vector3Int tableOccupiedMax =
//                         occupiedMaxByTable[blockTableIndex];

//                     float occupiedCenterX =
//                         (
//                             tableOccupiedMin.x +
//                             tableOccupiedMax.x
//                         ) *
//                         0.5f;

//                     float occupiedCenterZ =
//                         (
//                             tableOccupiedMin.z +
//                             tableOccupiedMax.z
//                         ) *
//                         0.5f;

//                     Vector3 surfacePosition =
//                         surfacePositions[blockTableIndex];

//                     Quaternion surfaceRotation =
//                         surfaceRotations[blockTableIndex];


//                     int spanX =
//                         Mathf.Max(1, cell.SpanX);

//                     int spanY =
//                         Mathf.Max(1, cell.SpanY);

//                     int spanZ =
//                         Mathf.Max(1, cell.SpanZ);


//                     /*
//                      * X:
//                      * Shape automatically center.
//                      *
//                      * Span > 1 ho to poore footprint (x..x+spanX-1)
//                      * ka center use hota hai, sirf anchor cell ka nahi.
//                      */
//                     float localX =
//                         (
//                             x -
//                             occupiedCenterX
//                         ) *
//                         stepX +
//                         (spanX - 1) *
//                         stepX *
//                         0.5f +
//                         cell.LocalOffset.x *
//                         stepX;


//                     /*
//                      * Y:
//                      * Lowest occupied manual-grid row
//                      * directly table top par start hogi.
//                      *
//                      * Transparent bottom margin
//                      * automatically ignore hota hai.
//                      */
//                     float localY =
//                         cellSize.y *
//                         0.5f +
//                         (
//                             y -
//                             tableOccupiedMin.y
//                         ) *
//                         stepY +
//                         (spanY - 1) *
//                         stepY *
//                         0.5f +
//                         cell.LocalOffset.y *
//                         stepY;


//                     /*
//                      * Z:
//                      * Complete depth centered.
//                      */
//                     float localZ =
//                         (
//                             z -
//                             occupiedCenterZ
//                         ) *
//                         stepZ +
//                         (spanZ - 1) *
//                         stepZ *
//                         0.5f +
//                         cell.LocalOffset.z *
//                         stepZ;


//                     Vector3 localCellCenter =
//                         new Vector3(
//                             localX,
//                             localY,
//                             localZ
//                         )
//                         +
//                         levelData.GridOffset;


//                     PhysicsObjectDefinition definition =
//                         levelData.GetPaletteEntry(
//                             cell.DefinitionIndex
//                         );

//                     if (definition == null ||
//                         definition.Prefab == null)
//                     {
//                         Debug.LogWarning(
//                             $"Cell ({x},{y},{z}): palette entry " +
//                             $"{cell.DefinitionIndex} invalid hai, skip.",
//                             levelData
//                         );

//                         continue;
//                     }

//                     PieceOrientation orientation =
//                         cell.Orientation;

//                     if (!TryGetFitData(
//                             definition,
//                             spanX,
//                             spanY,
//                             spanZ,
//                             orientation,
//                             out DefinitionFitData fitData))
//                     {
//                         continue;
//                     }


//                     /*
//                      * Orientation ka extra tip (Lying X/Z) surface
//                      * rotation ke UPAR apply hoti hai — piece ko table
//                      * ke sath sath khud bhi rotate karta hai.
//                      */
//                     Quaternion pieceRotation =
//                         surfaceRotation *
//                         PieceOrientationUtility.GetRotation(
//                             orientation,
//                             cell.CustomZRotation
//                         ) *
//                         Quaternion.Euler(
//                             cell.RotationEulerOffset
//                         );


//                     Vector3 pieceScale =
//                         Vector3.Scale(
//                             fitData.Scale,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 desiredBoundsCenter =
//                         surfacePosition +
//                         surfaceRotation *
//                         localCellCenter;


//                     /*
//                      * Prefab pivot center par na bhi ho
//                      * to collider bottom proper align hoga.
//                      */
//                     Vector3 rotatedBoundsOffset =
//                         pieceRotation *
//                         Vector3.Scale(
//                             fitData.BoundsCenterOffset,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 spawnPosition =
//                         desiredBoundsCenter -
//                         rotatedBoundsOffset;


//                     PhysicsTowerObject instance =
//                         objectPool.Get(
//                             definition,
//                             spawnPosition,
//                             pieceRotation,
//                             runtimeObjectsRoot
//                         );


//                     if (instance == null)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Pool.Get() prefab ki authored scale laga deta hai —
//                      * yahan usay cell-fit scale se override karte hain
//                      * taake mixed shapes (cube/cylinder) uniform grid
//                      * mein clean fit hon.
//                      */
//                     instance.transform.localScale =
//                         pieceScale;


//                     /*
//                      * Pool se purani property-block state clear karo.
//                      * Default visual hamesha prefab ka apna material aur
//                      * texture hai; paint tint definition par optional hai.
//                      */
//                     instance.RestorePrefabVisual();

//                     if (definition.TintWithPaintColor)
//                     {
//                         instance.SetVisualColor(
//                             cell.Color
//                         );
//                     }


//                     Vector3Int anchorCoordinate =
//                         new Vector3Int(x, y, z);

//                     instance.SetGridFootprint(
//                         anchorCoordinate,
//                         new Vector3Int(
//                             spanX,
//                             spanY,
//                             spanZ
//                         ),
//                         cell.LocalOffset
//                     );

//                     instance.ConfigureBreakable(
//                         cell.Breakable,
//                         cell.HitsToBreak
//                     );

//                     /*
//                      * Bare footprint ke SAARE cells is instance ko
//                      * point karte hain — is se agar upar kuch spawn ho
//                      * to woh footprint ke kisi bhi column par "supported"
//                      * sahi detect hoga.
//                      */
//                     for (int fz = 0; fz < spanZ; fz++)
//                     {
//                         for (int fy = 0; fy < spanY; fy++)
//                         {
//                             for (int fx = 0; fx < spanX; fx++)
//                             {
//                                 occupancyByCoordinate[
//                                     (
//                                         blockTableIndex,
//                                         new Vector3Int(
//                                             x + fx,
//                                             y + fy,
//                                             z + fz
//                                         )
//                                     )
//                                 ] = instance;
//                             }
//                         }
//                     }

//                     trackingByInstance[instance] =
//                         new GridTrackingEntry
//                         {
//                             InitialPosition = spawnPosition,
//                             UnsupportedFrames = 0,
//                             TableIndex = blockTableIndex,
//                             SurfaceRotation = surfaceRotation
//                         };


//                     instance.Cleared +=
//                         HandleObjectCleared;

//                     instance.Activated +=
//                         HandleObjectActivated;


//                     activeObjects.Add(
//                         instance
//                     );


//                     if (instance.CountsAsTarget)
//                     {
//                         remainingTargets++;
//                     }
//                     }
//                 }
//             }
//         }
//     }


//     private int ResolveBlockTableIndex(int requestedIndex)
//     {
//         if (requestedIndex >= 0 &&
//             requestedIndex < cachedTables.Count &&
//             cachedTables[requestedIndex] != null)
//         {
//             return requestedIndex;
//         }

//         return 0;
//     }


//     private void FixedUpdate()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }

//         AnimateEnabledTables();

//         if (activeObjects.Count == 0)
//         {
//             return;
//         }

//         /*
//          * Snapshot: activeObjects FixedUpdate ke dauran
//          * HandleObjectCleared se modify ho sakti hai
//          * (ActivatePhysics() khud kuch remove nahi karta,
//          * lekin safety ke liye copy le lete hain).
//          */
//         for (int i = activeObjects.Count - 1; i >= 0; i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];

//             if (instance == null || !instance.IsLocked)
//             {
//                 continue;
//             }

//             if (!trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry))
//             {
//                 continue;
//             }

//             if (IsSupported(
//                     instance,
//                     entry,
//                     out Vector3 unsupportedLocalDirection))
//             {
//                 entry.UnsupportedFrames = 0;
//                 continue;
//             }

//             entry.UnsupportedFrames++;

//             if (entry.UnsupportedFrames >=
//                 unsupportedFramesRequired)
//             {
//                 Vector3 worldFallDirection =
//                     entry.SurfaceRotation *
//                     unsupportedLocalDirection.normalized;

//                 Vector3 worldUp =
//                     entry.SurfaceRotation * Vector3.up;

//                 Vector3 tippingTorque =
//                     Vector3.Cross(
//                         worldUp,
//                         worldFallDirection
//                     ).normalized *
//                     unsupportedTipTorque *
//                     Random.Range(0.9f, 1.1f);

//                 instance.ActivatePhysics(
//                     worldFallDirection *
//                     unsupportedTipImpulse *
//                     Random.Range(0.9f, 1.1f),
//                     tippingTorque
//                 );
//             }
//         }
//     }


//     /// <summary>
//     /// Bottom row table par khadi hoti hai. Baqi pieces ke poore bottom
//     /// footprint mein kam az kam ek LOCKED support hona chahiye. Hit hua
//     /// dynamic block foran support count hona band ho jata hai, chahe woh
//     /// abhi apni spawn position se zyada move na bhi hua ho.
//     ///
//     /// Multi-cell footprint (span > 1) wale pieces ke liye sirf
//     /// "kahin bhi neeche support hai" (OR) kaafi nahi — agar ek extreme
//     /// edge (min ya max column) ka support hat jaye to piece ko us taraf
//     /// tip hona chahiye, chahe doosra extreme abhi bhi supported ho.
//     /// Ye seam-offset pieces (0.5 Center-on-Seam) aur plain grid-aligned
//     /// multi-span pieces, dono par apply hota hai.
//     /// </summary>
//     private bool IsSupported(
//         PhysicsTowerObject instance,
//         GridTrackingEntry trackingEntry,
//         out Vector3 unsupportedLocalDirection)
//     {
//         unsupportedLocalDirection = Vector3.zero;

//         if (instance == null)
//         {
//             unsupportedLocalDirection = Vector3.right;
//             return false;
//         }

//         Vector3Int coordinate =
//             instance.GridCoordinate;

//         int baseRow =
//             gridBaseRowByTable.TryGetValue(
//                 trackingEntry.TableIndex,
//                 out int tableBaseRow)
//                 ? tableBaseRow
//                 : coordinate.y;

//         if (coordinate.y <= baseRow)
//         {
//             return true;
//         }

//         Vector3Int span =
//             instance.GridSpan;

//         Vector3 localOffset =
//             instance.GridLocalOffset;

//         int minimumX =
//             coordinate.x +
//             Mathf.FloorToInt(localOffset.x);

//         int maximumX =
//             coordinate.x +
//             Mathf.Max(1, span.x) - 1 +
//             Mathf.CeilToInt(localOffset.x);

//         int minimumZ =
//             coordinate.z +
//             Mathf.FloorToInt(localOffset.z);

//         int maximumZ =
//             coordinate.z +
//             Mathf.Max(1, span.z) - 1 +
//             Mathf.CeilToInt(localOffset.z);

//         bool seamOnX =
//             !Mathf.Approximately(
//                 localOffset.x,
//                 Mathf.Round(localOffset.x)
//             );

//         bool seamOnZ =
//             !Mathf.Approximately(
//                 localOffset.z,
//                 Mathf.Round(localOffset.z)
//             );

//         /*
//          * Seam offset ke bina bhi agar piece ek se zyada grid column
//          * span kar raha hai (e.g. horizontal 2-cell beam), to sirf
//          * "kahin bhi support hai" kaafi nahi — dono extremes (min aur
//          * max column) ka support alag alag check hona chahiye, warna
//          * ek pillar hat jane par doosre pillar ka sahara poore beam
//          * ko galat tarah "supported" dikha deta hai.
//          */
//         bool spansMultipleColumnsX =
//             maximumX > minimumX;

//         bool spansMultipleColumnsZ =
//             maximumZ > minimumZ;

//         bool hasAnySupport = false;
//         bool minimumXSupported = false;
//         bool maximumXSupported = false;
//         bool minimumZSupported = false;
//         bool maximumZSupported = false;

//         for (int z = minimumZ; z <= maximumZ; z++)
//         {
//             for (int x = minimumX; x <= maximumX; x++)
//             {
//                 Vector3Int below =
//                     new Vector3Int(
//                         x,
//                         coordinate.y - 1,
//                         z
//                     );

//                 if (occupancyByCoordinate.TryGetValue(
//                         (trackingEntry.TableIndex, below),
//                         out PhysicsTowerObject supportInstance) &&
//                     supportInstance != null &&
//                     supportInstance != instance &&
//                     supportInstance.IsLocked)
//                 {
//                     hasAnySupport = true;
//                     minimumXSupported |= x == minimumX;
//                     maximumXSupported |= x == maximumX;
//                     minimumZSupported |= z == minimumZ;
//                     maximumZSupported |= z == maximumZ;
//                 }
//             }
//         }

//         if (!hasAnySupport)
//         {
//             unsupportedLocalDirection =
//                 GetNaturalUnsupportedDirection(
//                     coordinate,
//                     localOffset
//                 );

//             return false;
//         }

//         if ((seamOnX || spansMultipleColumnsX) &&
//             (!minimumXSupported || !maximumXSupported))
//         {
//             if (!minimumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.left;
//             }

//             if (!maximumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.right;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.x >= 0f
//                         ? Vector3.right
//                         : Vector3.left;
//             }

//             return false;
//         }

//         if ((seamOnZ || spansMultipleColumnsZ) &&
//             (!minimumZSupported || !maximumZSupported))
//         {
//             if (!minimumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.back;
//             }

//             if (!maximumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.forward;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.z >= 0f
//                         ? Vector3.forward
//                         : Vector3.back;
//             }

//             return false;
//         }

//         return true;
//     }


//     private static Vector3 GetNaturalUnsupportedDirection(
//         Vector3Int coordinate,
//         Vector3 localOffset)
//     {
//         Vector3 direction =
//             new Vector3(
//                 localOffset.x,
//                 0f,
//                 localOffset.z
//             );

//         if (direction.sqrMagnitude > 0.001f)
//         {
//             return direction.normalized;
//         }

//         int hash =
//             coordinate.x * 73856093 ^
//             coordinate.y * 19349663 ^
//             coordinate.z * 83492791;

//         float x =
//             (hash & 1) == 0 ? -1f : 1f;

//         float z =
//             (hash & 2) == 0 ? -0.45f : 0.45f;

//         return new Vector3(x, 0f, z).normalized;
//     }


//     private bool PrepareTable()
//     {
//         int requiredTableCount =
//             levelData.TableCount;

//         if (requiredTableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "GridLevelData mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }

//         while (cachedTables.Count > requiredTableCount)
//         {
//             DestroyCachedTableAt(
//                 cachedTables.Count - 1
//             );

//             cachedTables.RemoveAt(
//                 cachedTables.Count - 1
//             );

//             cachedTablePrefabs.RemoveAt(
//                 cachedTablePrefabs.Count - 1
//             );
//         }

//         Transform levelRoot =
//             GetLevelOrigin();

//         while (cachedTables.Count < requiredTableCount)
//         {
//             cachedTables.Add(null);
//             cachedTablePrefabs.Add(null);
//         }

//         for (int index = 0;
//              index < requiredTableCount;
//              index++)
//         {
//             LevelTable requiredPrefab =
//                 levelData.GetTablePrefab(index);

//             if (requiredPrefab == null)
//             {
//                 DestroyCachedTableAt(index);

//                 Debug.LogWarning(
//                     $"Table Placements element {index} ka Prefab missing hai; " +
//                     "is extra table ko skip kiya gaya.",
//                     levelData
//                 );

//                 continue;
//             }

//             if (cachedTables[index] == null ||
//                 cachedTablePrefabs[index] != requiredPrefab)
//             {
//                 DestroyCachedTableAt(index);

//                 LevelTable instance =
//                     Instantiate(
//                         requiredPrefab,
//                         levelRoot
//                     );

//                 instance.name =
//                     requiredPrefab.name +
//                     $"_Runtime_{index + 1}";

//                 cachedTables[index] = instance;
//                 cachedTablePrefabs[index] = requiredPrefab;
//             }

//             LevelTable table =
//                 cachedTables[index];

//             Transform tableTransform =
//                 table.transform;

//             if (tableTransform.parent != levelRoot)
//             {
//                 tableTransform.SetParent(
//                     levelRoot,
//                     false
//                 );
//             }

//             tableTransform.localPosition =
//                 levelData.GetTablePositionOffset(index);

//             tableTransform.localRotation =
//                 Quaternion.Euler(
//                     levelData.GetTableRotationEuler(index)
//                 );

//             tableTransform.localScale =
//                 Vector3.Scale(
//                     requiredPrefab.transform.localScale,
//                     levelData.GetTableSizeMultiplier(index)
//                 );

//             table.gameObject.SetActive(true);

//             if (table.TowerSurfaceCollider == null)
//             {
//                 Debug.LogError(
//                     $"Table Placements element {index} ke prefab par " +
//                     "Tower Surface Collider missing hai.",
//                     table
//                 );

//                 return false;
//             }
//         }

//         currentTable = cachedTables[0];

//         return true;
//     }


//     private void DestroyCachedTableAt(int index)
//     {
//         if (index < 0 ||
//             index >= cachedTables.Count)
//         {
//             return;
//         }

//         LevelTable table =
//             cachedTables[index];

//         if (table != null)
//         {
//             if (Application.isPlaying)
//             {
//                 Destroy(table.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(table.gameObject);
//             }
//         }

//         cachedTables[index] = null;

//         if (index < cachedTablePrefabs.Count)
//         {
//             cachedTablePrefabs[index] = null;
//         }
//     }


//     private void AnimateEnabledTables()
//     {
//         if (levelData == null ||
//             cachedTables.Count == 0)
//         {
//             return;
//         }

//         runtimeTableAnimationTime +=
//             Time.fixedDeltaTime;

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             LevelTable table = cachedTables[tableIndex];
//             bool rotationEnabled =
//                 levelData.IsTableRuntimeRotationEnabled(
//                     tableIndex
//                 );
//             bool movementEnabled =
//                 levelData.IsTableRuntimeHorizontalMovementEnabled(
//                     tableIndex
//                 );

//             if (table == null ||
//                 (!rotationEnabled && !movementEnabled))
//             {
//                 continue;
//             }

//             Vector3 rotationAxis =
//                 levelData.GetTableRuntimeRotationAxis(tableIndex);
//             float rotationSpeed =
//                 levelData.GetTableRuntimeRotationSpeed(tableIndex);
//             Vector3 movementAxis =
//                 levelData.GetTableRuntimeMovementAxis(tableIndex);
//             float movementDistance =
//                 levelData.GetTableRuntimeMovementDistance(tableIndex);
//             float movementSpeed =
//                 levelData.GetTableRuntimeMovementSpeed(tableIndex);

//             bool canRotate =
//                 rotationEnabled &&
//                 rotationAxis.sqrMagnitude >= 0.0001f &&
//                 !Mathf.Approximately(rotationSpeed, 0f);

//             bool canMove =
//                 movementEnabled &&
//                 movementAxis.sqrMagnitude >= 0.0001f &&
//                 movementDistance > 0f &&
//                 movementSpeed > 0f;

//             if (!canRotate && !canMove)
//             {
//                 continue;
//             }

//             Transform tableTransform = table.transform;
//             Vector3 oldWorldPosition = tableTransform.position;
//             Quaternion oldWorldRotation = tableTransform.rotation;

//             if (canMove)
//             {
//                 float movementPhase =
//                     runtimeTableAnimationTime *
//                     movementSpeed *
//                     Mathf.PI * 2f;

//                 tableTransform.localPosition =
//                     levelData.GetTablePositionOffset(tableIndex) +
//                     movementAxis.normalized *
//                     Mathf.Sin(movementPhase) *
//                     movementDistance;
//             }

//             if (canRotate)
//             {
//                 tableTransform.localRotation =
//                     tableTransform.localRotation *
//                     Quaternion.AngleAxis(
//                         rotationSpeed * Time.fixedDeltaTime,
//                         rotationAxis.normalized
//                     );
//             }

//             Vector3 newWorldPosition = tableTransform.position;
//             Vector3 tableVelocity =
//                 (newWorldPosition - oldWorldPosition) /
//                 Mathf.Max(0.0001f, Time.fixedDeltaTime);
//             Quaternion worldRotationDelta =
//                 tableTransform.rotation *
//                 Quaternion.Inverse(oldWorldRotation);

//             for (int objectIndex = 0;
//                  objectIndex < activeObjects.Count;
//                  objectIndex++)
//             {
//                 PhysicsTowerObject instance =
//                     activeObjects[objectIndex];

//                 if (instance == null ||
//                     !instance.IsLocked ||
//                     !trackingByInstance.TryGetValue(
//                         instance,
//                         out GridTrackingEntry entry) ||
//                     entry.TableIndex != tableIndex)
//                 {
//                     continue;
//                 }

//                 Rigidbody objectBody = instance.Body;
//                 Vector3 objectPosition =
//                     objectBody != null
//                         ? objectBody.position
//                         : instance.transform.position;
//                 Quaternion objectRotation =
//                     objectBody != null
//                         ? objectBody.rotation
//                         : instance.transform.rotation;
//                 Vector3 animatedPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (objectPosition - oldWorldPosition);

//                 instance.MoveLockedWithTable(
//                     animatedPosition,
//                     worldRotationDelta * objectRotation
//                 );

//                 entry.InitialPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (entry.InitialPosition - oldWorldPosition);

//                 entry.SurfaceRotation =
//                     worldRotationDelta * entry.SurfaceRotation;
//             }

//             TryReleaseTableBlocksAfterMovementCycles(
//                 tableIndex,
//                 canMove ? movementSpeed : 0f,
//                 tableVelocity
//             );
//         }
//     }


//     private void PrepareTablePhysicsReleaseTargets()
//     {
//         tablePhysicsReleaseCycleTargets.Clear();
//         releasedTablePhysicsIndices.Clear();

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             if (!levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                     tableIndex))
//             {
//                 tablePhysicsReleaseCycleTargets.Add(int.MaxValue);
//                 continue;
//             }

//             int minimumCycles =
//                 Mathf.Max(
//                     1,
//                     levelData.GetTableMinimumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );
//             int maximumCycles =
//                 Mathf.Max(
//                     minimumCycles,
//                     levelData.GetTableMaximumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );

//             tablePhysicsReleaseCycleTargets.Add(
//                 Random.Range(
//                     minimumCycles,
//                     maximumCycles + 1
//                 )
//             );
//         }
//     }


//     private void TryReleaseTableBlocksAfterMovementCycles(
//         int tableIndex,
//         float movementSpeed,
//         Vector3 tableVelocity)
//     {
//         if (tableIndex < 0 ||
//             tableIndex >= tablePhysicsReleaseCycleTargets.Count ||
//             releasedTablePhysicsIndices.Contains(tableIndex) ||
//             !levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                 tableIndex) ||
//             movementSpeed <= 0f)
//         {
//             return;
//         }

//         float completedCycles =
//             runtimeTableAnimationTime * movementSpeed;

//         if (completedCycles <
//             tablePhysicsReleaseCycleTargets[tableIndex])
//         {
//             return;
//         }

//         releasedTablePhysicsIndices.Add(tableIndex);

//         float velocityMultiplier =
//             levelData.GetTableMovementReleaseVelocityMultiplier(
//                 tableIndex
//             );

//         bool wasPropagatingMirroredActivation =
//             isPropagatingMirroredActivation;

//         isPropagatingMirroredActivation = true;

//         for (int objectIndex = activeObjects.Count - 1;
//              objectIndex >= 0;
//              objectIndex--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[objectIndex];

//             if (instance == null ||
//                 !instance.IsLocked ||
//                 !trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry) ||
//                 entry.TableIndex != tableIndex)
//             {
//                 continue;
//             }

//             Rigidbody body = instance.Body;
//             float bodyMass =
//                 body != null
//                     ? Mathf.Max(0.01f, body.mass)
//                     : 1f;

//             Vector3 naturalVariation =
//                 new Vector3(
//                     Random.Range(-0.08f, 0.08f),
//                     Random.Range(0.015f, 0.07f),
//                     Random.Range(-0.08f, 0.08f)
//                 );

//             Vector3 inheritedImpulse =
//                 (tableVelocity * velocityMultiplier +
//                  naturalVariation) *
//                 bodyMass;

//             Vector3 naturalTorque =
//                 Random.onUnitSphere *
//                 Random.Range(0.035f, 0.12f) *
//                 bodyMass;

//             instance.ActivatePhysics(
//                 inheritedImpulse,
//                 naturalTorque
//             );
//         }

//         isPropagatingMirroredActivation =
//             wasPropagatingMirroredActivation;
//     }


//     private void HandleObjectActivated(
//         PhysicsTowerObject source,
//         Vector3 sourceImpulse)
//     {
//         if (source == null ||
//             levelData == null ||
//             !linkMirroredDepthLayerPhysics ||
//             levelData.GridDepth <= 1 ||
//             isPropagatingMirroredActivation)
//         {
//             return;
//         }

//         Vector3Int sourceCoordinate =
//             source.GridCoordinate;

//         if (!trackingByInstance.TryGetValue(
//                 source,
//                 out GridTrackingEntry sourceTracking))
//         {
//             return;
//         }

//         int sourceTableIndex =
//             sourceTracking.TableIndex;

//         int activationRevision =
//             levelGenerationRevision;

//         for (int z = 0;
//              z < levelData.GridDepth;
//              z++)
//         {
//             if (z == sourceCoordinate.z ||
//                 !TryFindDepthLinkedObject(
//                     source,
//                     sourceCoordinate,
//                     sourceTableIndex,
//                     z,
//                     out PhysicsTowerObject mirroredObject,
//                     out Vector3Int mirroredCoordinate))
//             {
//                 continue;
//             }

//             StartCoroutine(
//                 ActivateMirroredObjectNaturally(
//                     mirroredObject,
//                     mirroredCoordinate,
//                     sourceTableIndex,
//                     sourceImpulse,
//                     activationRevision
//                 )
//             );
//         }
//     }


//     private bool TryFindDepthLinkedObject(
//         PhysicsTowerObject source,
//         Vector3Int sourceCoordinate,
//         int tableIndex,
//         int targetDepth,
//         out PhysicsTowerObject linkedObject,
//         out Vector3Int linkedCoordinate)
//     {
//         linkedObject = null;
//         linkedCoordinate = new Vector3Int(
//             sourceCoordinate.x,
//             sourceCoordinate.y,
//             targetDepth
//         );

//         if (TryGetLockedDepthObject(
//                 linkedCoordinate,
//                 tableIndex,
//                 source,
//                 out linkedObject))
//         {
//             return true;
//         }

//         /*
//          * Manually-authored layers can have different silhouettes. In that
//          * case there may be no block at the exact X/Y coordinate behind the
//          * hit. Find the nearest locked block in a small projected area so the
//          * back structure still receives a believable local impact rather
//          * than remaining completely static.
//          */
//         const int searchRadius = 2;
//         float bestScore = float.PositiveInfinity;

//         for (int yOffset = -searchRadius;
//              yOffset <= searchRadius;
//              yOffset++)
//         {
//             for (int xOffset = -searchRadius;
//                  xOffset <= searchRadius;
//                  xOffset++)
//             {
//                 if (xOffset == 0 && yOffset == 0)
//                 {
//                     continue;
//                 }

//                 Vector3Int candidateCoordinate =
//                     new Vector3Int(
//                         sourceCoordinate.x + xOffset,
//                         sourceCoordinate.y + yOffset,
//                         targetDepth
//                     );

//                 if (!TryGetLockedDepthObject(
//                         candidateCoordinate,
//                         tableIndex,
//                         source,
//                         out PhysicsTowerObject candidate))
//                 {
//                     continue;
//                 }

//                 float score =
//                     xOffset * xOffset +
//                     yOffset * yOffset * 1.5f;

//                 if (score >= bestScore)
//                 {
//                     continue;
//                 }

//                 bestScore = score;
//                 linkedObject = candidate;
//                 linkedCoordinate = candidateCoordinate;
//             }
//         }

//         return linkedObject != null;
//     }


//     private bool TryGetLockedDepthObject(
//         Vector3Int coordinate,
//         int tableIndex,
//         PhysicsTowerObject source,
//         out PhysicsTowerObject candidate)
//     {
//         return occupancyByCoordinate.TryGetValue(
//                    (tableIndex, coordinate),
//                    out candidate) &&
//                candidate != null &&
//                candidate != source &&
//                candidate.IsLocked;
//     }


//     private IEnumerator ActivateMirroredObjectNaturally(
//         PhysicsTowerObject mirroredObject,
//         Vector3Int mirroredCoordinate,
//         int tableIndex,
//         Vector3 sourceImpulse,
//         int activationRevision)
//     {
//         float minimumDelay =
//             Mathf.Max(0f, mirroredActivationDelayRange.x);

//         float maximumDelay =
//             Mathf.Max(minimumDelay, mirroredActivationDelayRange.y);

//         float delay =
//             Random.Range(minimumDelay, maximumDelay);

//         if (delay > 0f)
//         {
//             yield return new WaitForSeconds(delay);
//         }

//         if (activationRevision != levelGenerationRevision ||
//             mirroredObject == null ||
//             !mirroredObject.IsLocked ||
//             !occupancyByCoordinate.TryGetValue(
//                 (tableIndex, mirroredCoordinate),
//                 out PhysicsTowerObject currentObject) ||
//             currentObject != mirroredObject)
//         {
//             yield break;
//         }

//         float minimumStrength =
//             Mathf.Max(0f, mirroredImpulseVariation.x);

//         float maximumStrength =
//             Mathf.Max(minimumStrength, mirroredImpulseVariation.y);

//         float randomStrength =
//             Random.Range(minimumStrength, maximumStrength);

//         Vector3 randomAngles =
//             new Vector3(
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 )
//             );

//         Vector3 variedImpulse =
//             Quaternion.Euler(randomAngles) *
//             sourceImpulse *
//             mirroredLayerImpulseMultiplier *
//             randomStrength;

//         Vector3 randomTorque =
//             sourceImpulse.sqrMagnitude > 0.0001f
//                 ? Random.onUnitSphere *
//                   sourceImpulse.magnitude *
//                   mirroredRandomTorqueMultiplier *
//                   Random.Range(0.65f, 1.25f)
//                 : Vector3.zero;

//         isPropagatingMirroredActivation = true;

//         try
//         {
//             mirroredObject.ActivatePhysics(
//                 variedImpulse,
//                 randomTorque
//             );
//         }
//         finally
//         {
//             isPropagatingMirroredActivation = false;
//         }
//     }


//     private void HandleObjectCleared(
//         PhysicsTowerObject target)
//     {
//         if (target == null)
//         {
//             return;
//         }


//         target.Cleared -=
//             HandleObjectCleared;

//         target.Activated -=
//             HandleObjectActivated;


//         activeObjects.Remove(
//             target
//         );

//         trackingByInstance.Remove(
//             target
//         );

//         /*
//          * Multi-cell footprint wale piece ke liye occupancyByCoordinate
//          * mein uske SAARE covered cells target ko point karte hain
//          * (sirf anchor nahi) — is liye scan karke sab remove karte hain,
//          * warna stale entries agle level mein reused instance ko
//          * galat tarah "still standing" dikha sakti hain.
//          */
//         List<(int tableIndex, Vector3Int coordinate)> coordinatesToRemove = null;

//         foreach (KeyValuePair<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject> entry
//                  in occupancyByCoordinate)
//         {
//             if (entry.Value != target)
//             {
//                 continue;
//             }

//             if (coordinatesToRemove == null)
//             {
//                 coordinatesToRemove =
//                     new List<(int, Vector3Int)>();
//             }

//             coordinatesToRemove.Add(entry.Key);
//         }

//         if (coordinatesToRemove != null)
//         {
//             foreach ((int tableIndex, Vector3Int coordinate) key
//                      in coordinatesToRemove)
//             {
//                 occupancyByCoordinate.Remove(key);
//             }
//         }


//         if (target.CountsAsTarget)
//         {
//             remainingTargets =
//                 Mathf.Max(
//                     0,
//                     remainingTargets - 1
//                 );
//         }


//         objectPool.Release(
//             target
//         );


//         if (levelGenerated &&
//             remainingTargets == 0)
//         {
//             CompleteLevel();
//         }
//     }


//     private void CompleteLevel()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }


//         levelGenerated =
//             false;


//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} COMPLETE",
//             this
//         );

// //  if (milestoneProgressBar != null)
// //     {
// //         milestoneProgressBar.OnLevelCompleted(
// //             levelData.LevelNumber
// //         );
// //     }


//         onLevelComplete?.Invoke();

//         ShowLevelCompleteUI();

//         if (autoLoadNextLevel &&
//             HasNextLevel &&
//             !IsLiveWorkingLevelPreviewEnabled())
//         {
//             StartCoroutine(
//                 LoadNextLevelAfterCompletion(
//                     currentLevelIndex
//                 )
//             );
//         }
//     }


//     private IEnumerator LoadNextLevelAfterCompletion(
//         int completedLevelIndex)
//     {
//         if (autoLoadNextLevelDelay > 0f)
//         {
//             yield return new WaitForSecondsRealtime(
//                 autoLoadNextLevelDelay
//             );
//         }

//         /*
//          * Player ne delay ke duran restart/next button use kar liya ho to
//          * purani completion routine naye level ko dobara skip na kare.
//          */
//         if (currentLevelIndex != completedLevelIndex ||
//             levelGenerated ||
//             isLoadingLevel)
//         {
//             yield break;
//         }

//         LoadNextLevel();
//     }


//     public void LoadNextLevel()
//     {
//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int nextIndex =
//             FindNextLevelIndex(
//                 currentLevelIndex + 1
//             );

//         if (nextIndex < 0)
//         {
//             Debug.LogWarning(
//                 "Next Addressable level configured nahi hai. Level " +
//                 "Creator se next level SAVE karein.",
//                 this
//             );

//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(nextIndex)
//         );
//     }

//     public void RestartCurrentLevel()
//     {
//         if (isLoadingLevel ||
//             levelData == null)
//         {
//             return;
//         }

//         DestroyActiveCannonBalls();
//         GenerateLevel();
//     }


//     private IEnumerator LoadAddressableLevelRoutine(
//         int levelIndex)
//     {
//         if (isLoadingLevel ||
//             levelDatabase == null)
//         {
//             yield break;
//         }

//         string address =
//             levelDatabase.GetAddress(levelIndex);

//         if (string.IsNullOrWhiteSpace(address))
//         {
//             Debug.LogError(
//                 $"Level index {levelIndex} ka Addressable address missing hai.",
//                 this
//             );
//             yield break;
//         }

//         isLoadingLevel = true;
//         RefreshNextLevelButton();

//         AsyncOperationHandle<GridLevelData> newHandle =
//             Addressables.LoadAssetAsync<GridLevelData>(address);

//         pendingLevelHandle = newHandle;
//         hasPendingLevelHandle = true;

//         yield return newHandle;

//         if (newHandle.Status != AsyncOperationStatus.Succeeded ||
//             newHandle.Result == null)
//         {
//             Debug.LogError(
//                 $"Addressable level load failed: {address}",
//                 this
//             );

//             if (newHandle.IsValid())
//             {
//                 Addressables.Release(newHandle);
//             }

//             hasPendingLevelHandle = false;
//             isLoadingLevel = false;
//             RefreshNextLevelButton();
//             yield break;
//         }

//         /*
//          * Pehle purane instantiated objects destroy hote hain. Ek frame
//          * baad purana Addressables handle release hota hai, isliye bundle
//          * unload ke waqt koi live prefab/material instance nahi rehta.
//          */
//         PrepareForLevelAssetRelease();
//         yield return null;
//         ReleaseActiveLevelHandle();

//         activeLevelHandle = newHandle;
//         hasActiveLevelHandle = true;
//         hasPendingLevelHandle = false;
//         currentLevelIndex = levelIndex;
//         levelData = newHandle.Result;
//         isLoadingLevel = false;

//         SaveCurrentLevelProgress(
//             levelData.LevelNumber
//         );

//         GenerateLevel();
//     }


//     public void LoadLevelByNumber(int levelNumber)
//     {
//         if (levelDatabase == null)
//         {
//             Debug.LogError(
//                 "LoadLevelByNumber: GridLevelDatabase missing hai.",
//                 this
//             );
//             return;
//         }

//         if (levelNumber >
//             levelDatabase.MaximumPlayableLevelNumber)
//         {
//             Debug.LogWarning(
//                 $"Level {levelNumber} playable range se bahar hai. " +
//                 $"Last playable level " +
//                 $"{levelDatabase.MaximumPlayableLevelNumber} hai.",
//                 this
//             );
//             return;
//         }

//         int levelIndex =
//             levelDatabase.FindIndexByLevelNumber(levelNumber);

//         if (levelIndex < 0)
//         {
//             Debug.LogWarning(
//                 $"Addressable Level {levelNumber} catalog mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(levelIndex)
//         );
//     }


//     private void SelectStartingLevel()
//     {
//         if (levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             currentLevelIndex = 0;
//             return;
//         }

//         if (PlayerPrefs.HasKey(SavedLevelNumberKey))
//         {
//             int savedLevelNumber =
//                 PlayerPrefs.GetInt(
//                     SavedLevelNumberKey,
//                     1
//                 );

//             int savedLevelIndex =
//                 levelDatabase.FindIndexByLevelNumber(
//                     savedLevelNumber
//                 );

//             if (savedLevelIndex >= 0 &&
//                 FindNextLevelIndex(savedLevelIndex) ==
//                 savedLevelIndex)
//             {
//                 currentLevelIndex = savedLevelIndex;
//                 return;
//             }
//         }

//         if (startFromLevelOne)
//         {
//             int firstLevelIndex =
//                 FindNextLevelIndex(0);

//             if (firstLevelIndex >= 0)
//             {
//                 currentLevelIndex = firstLevelIndex;
//             }

//             return;
//         }

//         int configuredIndex =
//             levelData != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     levelData.LevelNumber
//                 )
//                 : -1;

//         currentLevelIndex =
//             configuredIndex >= 0
//                 ? configuredIndex
//                 : Mathf.Max(0, FindNextLevelIndex(0));

//     }


//     private static bool IsLiveWorkingLevelPreviewEnabled()
//     {
// #if UNITY_EDITOR
//         return UnityEditor.SessionState.GetBool(
//             "RoyalSmash.LiveWorkingLevelPreview",
//             false
//         );
// #else
//         return false;
// #endif
//     }


//     private static bool ShouldStartLiveWorkingLevelPreview()
//     {
// #if UNITY_EDITOR
//         const string previewSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreview";

//         const string previewLaunchSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreviewLaunch";

//         bool previewEnabled =
//             UnityEditor.SessionState.GetBool(
//                 previewSessionKey,
//                 false
//             );

//         bool launchRequested =
//             UnityEditor.SessionState.GetBool(
//                 previewLaunchSessionKey,
//                 false
//             );

//         UnityEditor.SessionState.SetBool(
//             previewLaunchSessionKey,
//             false
//         );

//         if (!launchRequested)
//         {
//             /*
//              * Toolbar se Play karne par pichli editor preview session
//              * normal game startup ko bypass nahi kar sakti.
//              */
//             UnityEditor.SessionState.SetBool(
//                 previewSessionKey,
//                 false
//             );
//         }

//         return previewEnabled && launchRequested;
// #else
//         return false;
// #endif
//     }


//     private int FindNextLevelIndex(
//         int startingIndex)
//     {
//         if (levelDatabase == null)
//         {
//             return -1;
//         }

//         for (int i = Mathf.Max(0, startingIndex);
//              i < levelDatabase.Count;
//              i++)
//         {
//             int levelNumber =
//                 levelDatabase.GetLevelNumber(i);

//             if (levelNumber >
//                 levelDatabase.MaximumPlayableLevelNumber)
//             {
//                 break;
//             }

//             if (levelNumber > 0 &&
//                 !string.IsNullOrWhiteSpace(
//                     levelDatabase.GetAddress(i)))
//             {
//                 return i;
//             }
//         }

//         return -1;
//     }


//     private void EnsureMainMenuUI()
//     {
//         if (mainMenuPanel == null)
//         {
//             RectTransform[] sceneRects =
//                 FindObjectsByType<RectTransform>(
//                     FindObjectsInactive.Include,
//                     FindObjectsSortMode.None
//                 );

//             foreach (RectTransform sceneRect in sceneRects)
//             {
//                 if (sceneRect != null &&
//                     sceneRect.name.Equals(
//                         "Main menu Panel",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ))
//                 {
//                     mainMenuPanel = sceneRect.gameObject;
//                     break;
//                 }
//             }
//         }

//         if (mainMenuPanel == null)
//         {
//             Debug.LogWarning(
//                 "Main menu Panel scene mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         Button[] menuButtons =
//             mainMenuPanel.GetComponentsInChildren<Button>(true);

//         foreach (Button menuButton in menuButtons)
//         {
//             if (menuButton == null)
//             {
//                 continue;
//             }

//             string buttonName =
//                 menuButton.name.ToLowerInvariant();

//             if (playButton == null &&
//                 buttonName.Contains("play"))
//             {
//                 playButton = menuButton;
//             }
//             else if (resetProgressButton == null &&
//                      buttonName.Contains("reset"))
//             {
//                 resetProgressButton = menuButton;
//             }
//         }

//         if (currentLevelText == null)
//         {
//             TMP_Text[] menuTexts =
//                 mainMenuPanel.GetComponentsInChildren<TMP_Text>(true);

//             foreach (TMP_Text menuText in menuTexts)
//             {
//                 if (menuText != null &&
//                     menuText.text.IndexOf(
//                         "level number",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ) >= 0)
//                 {
//                     currentLevelText = menuText;
//                     break;
//                 }
//             }
//         }

//         if (playButton != null)
//         {
//             playButton.onClick.RemoveListener(
//                 PlayFromMainMenu
//             );
//             playButton.onClick.AddListener(
//                 PlayFromMainMenu
//             );
//         }

//         if (resetProgressButton != null)
//         {
//             resetProgressButton.onClick.RemoveListener(
//                 ResetProgressFromMainMenu
//             );
//             resetProgressButton.onClick.AddListener(
//                 ResetProgressFromMainMenu
//             );
//         }
//     }


//     private void SetGameplayWorldVisible(bool visible)
//     {
//         if (gameplayPanel != null)
//         {
//             gameplayPanel.SetActive(visible);
//         }

//         if (levelOrigin != null &&
//             levelOrigin != transform)
//         {
//             levelOrigin.gameObject.SetActive(visible);
//         }

//         if (runtimeObjectsRoot != null)
//         {
//             runtimeObjectsRoot.gameObject.SetActive(visible);
//         }

//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (cannonController != null)
//         {
//             cannonController.SetGameplayActive(visible);
//         }

//         if (gameplayRestartButton == null)
//         {
//             GameRestartController restartController =
//                 FindFirstObjectByType<GameRestartController>(
//                     FindObjectsInactive.Include
//                 );

//             if (restartController != null)
//             {
//                 gameplayRestartButton =
//                     restartController.gameObject;
//             }
//         }

//         if (gameplayRestartButton != null)
//         {
//             gameplayRestartButton.SetActive(visible);
//         }

//         if (!visible)
//         {
//             foreach (LevelTable table in cachedTables)
//             {
//                 if (table != null)
//                 {
//                     table.gameObject.SetActive(false);
//                 }
//             }

//             if (nextLevelButton != null)
//             {
//                 nextLevelButton.gameObject.SetActive(false);
//             }
//         }
//     }


//     private void SaveCurrentLevelProgress(int levelNumber)
//     {
//         if (levelNumber <= 0 ||
//             IsLiveWorkingLevelPreviewEnabled())
//         {
//             return;
//         }

//         PlayerPrefs.SetInt(
//             SavedLevelNumberKey,
//             levelNumber
//         );
//         PlayerPrefs.Save();
//         UpdateCurrentLevelText();
//     }


//     private void UpdateCurrentLevelText()
//     {
//         if (currentLevelText == null ||
//             levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             return;
//         }

//         int safeIndex =
//             Mathf.Clamp(
//                 currentLevelIndex,
//                 0,
//                 levelDatabase.Count - 1
//             );

//         int levelNumber =
//             levelDatabase.GetLevelNumber(safeIndex);

//         currentLevelText.text =
//             $"{Mathf.Max(1, levelNumber)}";
//     }


//     private void EnsureNextLevelUI()
//     {
//         if (nextLevelButton != null)
//         {
//             nextLevelButton.onClick.RemoveListener(
//                 LoadNextLevel
//             );

//             nextLevelButton.onClick.AddListener(
//                 LoadNextLevel
//             );
//         }

//         if ((nextLevelButton != null &&
//              levelCompleteText != null) ||
//             !autoCreateNextLevelUI)
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
//                 candidate.renderMode != RenderMode.WorldSpace)
//             {
//                 targetCanvas = candidate;
//                 break;
//             }
//         }

//         if (targetCanvas == null)
//         {
//             GameObject canvasObject =
//                 new GameObject(
//                     "Level Progress Canvas",
//                     typeof(RectTransform),
//                     typeof(Canvas),
//                     typeof(CanvasScaler),
//                     typeof(GraphicRaycaster)
//                 );

//             int uiLayer = LayerMask.NameToLayer("UI");

//             if (uiLayer >= 0)
//             {
//                 canvasObject.layer = uiLayer;
//             }

//             targetCanvas = canvasObject.GetComponent<Canvas>();
//             targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             targetCanvas.sortingOrder = 110;

//             CanvasScaler scaler =
//                 canvasObject.GetComponent<CanvasScaler>();

//             scaler.uiScaleMode =
//                 CanvasScaler.ScaleMode.ScaleWithScreenSize;

//             scaler.referenceResolution =
//                 new Vector2(1080f, 1920f);
//         }

//         int panelLayer = targetCanvas.gameObject.layer;

//         autoCreatedCompletePanel =
//             new GameObject(
//                 "Level Complete Panel",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(Image)
//             );

//         autoCreatedCompletePanel.layer = panelLayer;

//         RectTransform panelRect =
//             autoCreatedCompletePanel.GetComponent<RectTransform>();

//         panelRect.SetParent(targetCanvas.transform, false);
//         panelRect.anchorMin = new Vector2(0.5f, 0.5f);
//         panelRect.anchorMax = new Vector2(0.5f, 0.5f);
//         panelRect.pivot = new Vector2(0.5f, 0.5f);
//         panelRect.anchoredPosition = Vector2.zero;
//         panelRect.sizeDelta = new Vector2(620f, 180f);

//         Image panelImage =
//             autoCreatedCompletePanel.GetComponent<Image>();

//         panelImage.color = new Color(0.03f, 0.04f, 0.07f, 0.92f);

//         GameObject titleObject =
//             new GameObject(
//                 "Level Complete Text",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         titleObject.layer = panelLayer;

//         RectTransform titleRect =
//             titleObject.GetComponent<RectTransform>();

//         titleRect.SetParent(panelRect, false);
//         titleRect.anchorMin = new Vector2(0f, 1f);
//         titleRect.anchorMax = new Vector2(1f, 1f);
//         titleRect.pivot = new Vector2(0.5f, 1f);
//         titleRect.anchoredPosition = new Vector2(0f, -38f);
//         titleRect.sizeDelta = new Vector2(-40f, 100f);

//         TextMeshProUGUI title =
//             titleObject.GetComponent<TextMeshProUGUI>();

//         title.fontSize = 48f;
//         title.fontStyle = FontStyles.Bold;
//         title.alignment = TextAlignmentOptions.Center;
//         title.color = Color.white;
//         title.raycastTarget = false;

//         levelCompleteText = title;

//         GameObject buttonObject =
//             new GameObject(
//                 "Next Level Button",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(Image),
//                 typeof(Button)
//             );

//         buttonObject.layer = panelLayer;

//         RectTransform buttonRect =
//             buttonObject.GetComponent<RectTransform>();

//         /*
//          * NEXT LEVEL gameplay ke duran bhi visible rehna chahiye,
//          * isliye button complete panel ka child nahi. Panel hide hone
//          * par sirf completion title hide hota hai, button nahi.
//          */
//         buttonRect.SetParent(targetCanvas.transform, false);
//         buttonRect.anchorMin = new Vector2(1f, 1f);
//         buttonRect.anchorMax = new Vector2(1f, 1f);
//         buttonRect.pivot = new Vector2(1f, 1f);
//         buttonRect.anchoredPosition = new Vector2(-32f, -32f);
//         buttonRect.sizeDelta = new Vector2(390f, 88f);

//         Image buttonImage = buttonObject.GetComponent<Image>();
//         buttonImage.color = new Color(0.15f, 0.62f, 1f, 1f);

//         nextLevelButton = buttonObject.GetComponent<Button>();
//         nextLevelButton.targetGraphic = buttonImage;
//         nextLevelButton.onClick.AddListener(LoadNextLevel);

//         GameObject labelObject =
//             new GameObject(
//                 "Label",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         labelObject.layer = panelLayer;

//         RectTransform labelRect =
//             labelObject.GetComponent<RectTransform>();

//         labelRect.SetParent(buttonRect, false);
//         labelRect.anchorMin = Vector2.zero;
//         labelRect.anchorMax = Vector2.one;
//         labelRect.offsetMin = Vector2.zero;
//         labelRect.offsetMax = Vector2.zero;

//         TextMeshProUGUI label =
//             labelObject.GetComponent<TextMeshProUGUI>();

//         label.text = "NEXT LEVEL";
//         label.fontSize = 38f;
//         label.fontStyle = FontStyles.Bold;
//         label.alignment = TextAlignmentOptions.Center;
//         label.color = Color.white;
//         label.raycastTarget = false;

//         RefreshNextLevelButton();
//     }


//     private void ShowLevelCompleteUI()
//     {
//         EnsureNextLevelUI();

//         bool hasNext = HasNextLevel;

//         if (levelCompleteText != null)
//         {
//             levelCompleteText.text =
//                 hasNext
//                     ? $"LEVEL {levelData.LevelNumber} COMPLETE"
//                     : "ALL LEVELS COMPLETE";
//         }

//         if (nextLevelButton != null)
//         {
//             RefreshNextLevelButton();
//         }

//         SetLevelCompleteUIVisible(true);
//     }


//     private void SetLevelCompleteUIVisible(
//         bool visible)
//     {
//         if (autoCreatedCompletePanel != null)
//         {
//             autoCreatedCompletePanel.SetActive(visible);
//         }
//         else if (levelCompleteText != null)
//         {
//             levelCompleteText.gameObject.SetActive(visible);
//         }

//         if (nextLevelButton != null)
//         {
//             nextLevelButton.gameObject.SetActive(true);
//         }
//     }


//     private void RefreshNextLevelButton()
//     {
//         if (nextLevelButton == null)
//         {
//             return;
//         }

//         int nextIndex =
//             FindNextLevelIndex(
//                 currentLevelIndex + 1
//             );

//         bool hasNext = nextIndex >= 0;
//         nextLevelButton.gameObject.SetActive(true);
//         nextLevelButton.interactable =
//             hasNext && !isLoadingLevel;

//         TMP_Text buttonLabel =
//             nextLevelButton.GetComponentInChildren<TMP_Text>(true);

//         if (buttonLabel == null)
//         {
//             return;
//         }

//         if (!hasNext)
//         {
//             buttonLabel.text = "NO NEXT LEVEL";
//             return;
//         }

//         buttonLabel.text =
//             isLoadingLevel
//                 ? "LOADING..."
//                 : $"NEXT LEVEL " +
//                   $"{levelDatabase.GetLevelNumber(nextIndex)}";
//     }


//     private void PrepareForLevelAssetRelease()
//     {
//         ClearCurrentLevel();
//         DestroyActiveCannonBalls();

//         if (objectPool != null)
//         {
//             objectPool.ClearAll();
//         }

//         for (int index = cachedTables.Count - 1;
//              index >= 0;
//              index--)
//         {
//             DestroyCachedTableAt(index);
//         }

//         cachedTables.Clear();
//         cachedTablePrefabs.Clear();
//         currentTable = null;
//     }


//     private static void DestroyActiveCannonBalls()
//     {
//         CannonBallMarker[] cannonBalls =
//             FindObjectsByType<CannonBallMarker>(
//                 FindObjectsInactive.Include,
//                 FindObjectsSortMode.None
//             );

//         foreach (CannonBallMarker cannonBall in cannonBalls)
//         {
//             if (cannonBall == null)
//             {
//                 continue;
//             }

//             if (Application.isPlaying)
//             {
//                 Destroy(cannonBall.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(cannonBall.gameObject);
//             }
//         }
//     }


//     private void ReleaseActiveLevelHandle()
//     {
//         if (!hasActiveLevelHandle)
//         {
//             return;
//         }

//         if (activeLevelHandle.IsValid())
//         {
//             Addressables.Release(activeLevelHandle);
//         }

//         hasActiveLevelHandle = false;
//     }


//     public void ClearCurrentLevel()
//     {
//         levelGenerationRevision++;

//         levelGenerated =
//             false;


//         for (int i =
//                  activeObjects.Count - 1;
//              i >= 0;
//              i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance == null)
//             {
//                 continue;
//             }


//             instance.Cleared -=
//                 HandleObjectCleared;

//             instance.Activated -=
//                 HandleObjectActivated;


//             if (objectPool != null)
//             {
//                 objectPool.Release(
//                     instance
//                 );
//             }
//         }


//         activeObjects.Clear();

//         occupancyByCoordinate.Clear();

//         trackingByInstance.Clear();

//         gridBaseRowByTable.Clear();

//         /*
//          * Fit cache CellSize-specific hai — agar LoadLevel() se
//          * different GridLevelData aa jaye (alag CellSize) to purani
//          * cached scale ghalat hogi.
//          */
//         fitCache.Clear();


//         remainingTargets =
//             0;


//         foreach (LevelTable table in cachedTables)
//         {
//             if (table != null)
//             {
//                 table.gameObject.SetActive(false);
//             }
//         }


//         currentTable =
//             null;
//     }


//     private Transform GetLevelOrigin()
//     {
//         return
//             levelOrigin != null
//                 ? levelOrigin
//                 : transform;
//     }


//     private bool ValidateReferences()
//     {
//         if (levelData == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (!levelData.HasAnyValidTableGrid)
//         {
//             Debug.LogError(
//                 "Manual GridLevelData mein koi painted cell nahi hai. " +
//                 "Level Creator mein grid allocate karke shapes paint karein.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.BlockPalette == null ||
//             levelData.BlockPalette.Count == 0)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein Block Palette empty hai.",
//                 levelData
//             );

//             return false;
//         }

//         bool hasValidPaletteEntry =
//             false;

//         foreach (PhysicsObjectDefinition entry
//                  in levelData.BlockPalette)
//         {
//             if (entry != null &&
//                 entry.Prefab != null)
//             {
//                 hasValidPaletteEntry =
//                     true;

//                 break;
//             }
//         }

//         if (!hasValidPaletteEntry)
//         {
//             Debug.LogError(
//                 "Grid Level Data ki Block Palette mein koi valid " +
//                 "Prefab wali entry nahi hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.TableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (objectPool == null)
//         {
//             Debug.LogError(
//                 "Physics Object Pool missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (runtimeObjectsRoot == null)
//         {
//             Debug.LogError(
//                 "Runtime Objects Root missing hai.",
//                 this
//             );

//             return false;
//         }


//         Vector3 runtimeScale =
//             runtimeObjectsRoot.lossyScale;


//         if (Mathf.Abs(runtimeScale.x - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.y - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.z - 1f) > 0.01f)
//         {
//             Debug.LogWarning(
//                 "RuntimeObjects ki world Scale ideally 1,1,1 honi chahiye.",
//                 runtimeObjectsRoot
//             );
//         }


//         return true;
//     }


//     private void OnDestroy()
//     {
//         for (int i = 0;
//              i < activeObjects.Count;
//              i++)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance != null)
//             {
//                 instance.Cleared -=
//                     HandleObjectCleared;

//                 instance.Activated -=
//                     HandleObjectActivated;
//             }
//         }

//         ReleaseActiveLevelHandle();

//         if (hasPendingLevelHandle &&
//             pendingLevelHandle.IsValid())
//         {
//             Addressables.Release(pendingLevelHandle);
//         }

//         hasPendingLevelHandle = false;
//     }
// }









// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.Events;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.Serialization;
// using UnityEngine.UI;

// public sealed class LevelRuntimeController : MonoBehaviour
// {
//     private const string SavedLevelNumberKey =
//         "RoyalSmash.CurrentLevelNumber";

//     public event System.Action<GridLevelData> LevelGenerated;






// // [Header("Level Progress")]
// // [SerializeField]
// // private LevelMilestoneProgressBar milestoneProgressBar;









//     [Header("Level")]

//     [SerializeField]
//     private GridLevelData levelData;

//     [Tooltip(
//         "Level Creator ke banaye tamam levels ki central database. " +
//         "Runtime mein individual levels manually assign nahi karne."
//     )]
//     [SerializeField]
//     private GridLevelDatabase levelDatabase;

//     [SerializeField]
//     private bool startFromLevelOne = true;


//     [Header("Level Complete UI")]

//     [Tooltip(
//         "Apna designed Level Complete popup/panel yahan assign karein. " +
//         "Level complete hote hi ye panel active hoga."
//     )]
//     [SerializeField]
//     private GameObject levelCompletePanel;

//     [Tooltip(
//         "Level Complete popup ka Continue button. Press karne par next " +
//         "Addressable level load hoga."
//     )]
//     [FormerlySerializedAs("nextLevelButton")]
//     [SerializeField]
//     private Button continueButton;

//     [Tooltip(
//         "Optional level complete title/text. Example: LEVEL 5 COMPLETE."
//     )]
//     [SerializeField]
//     private TMP_Text levelCompleteText;

//     [Tooltip(
//         "Agar custom Level Complete Panel / Continue Button assign nahi kiya " +
//         "to fallback runtime UI automatically create kare."
//     )]
//     [SerializeField]
//     private bool autoCreateNextLevelUI = true;

//     [Tooltip(
//         "Level Complete popup visible ho to gameplay physics ko pause kare. " +
//         "Continue press karne par Time Scale automatically 1 ho jayega."
//     )]
//     [SerializeField]
//     private bool pauseGameplayOnLevelComplete = true;


//     [Header("Main Menu")]

//     [SerializeField]
//     private bool showMainMenuOnStartup = true;

//     [SerializeField]
//     private GameObject mainMenuPanel;

//     [Tooltip(
//         "Gameplay UI panel. Main Menu par hidden rahega aur Play press karte hi " +
//         "gameplay world ke sath active ho jayega."
//     )]
//     [SerializeField]
//     private GameObject gameplayPanel;

//     [SerializeField]
//     private Button playButton;

//     [SerializeField]
//     private Button resetProgressButton;

//     [SerializeField]
//     private TMP_Text currentLevelText;


//     [Header("References")]

//     [SerializeField]
//     private PhysicsObjectPool objectPool;

//     [SerializeField]
//     private Transform levelOrigin;

//     [SerializeField]
//     private Transform runtimeObjectsRoot;


//     [Header("Support Chain")]

//     [Tooltip(
//         "Support khone ke kitne FixedUpdate frames baad upar wala " +
//         "block khud bhi dynamic ho jayega."
//     )]
//     [SerializeField, Range(1, 10)]
//     private int unsupportedFramesRequired = 2;

//     [Tooltip(
//         "Half-supported piece ko missing-support side par tip karne wala " +
//         "small sideways impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipImpulse = 0.18f;

//     [Tooltip(
//         "Exact edge-balance ko break karne wala natural rotation impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipTorque = 0.28f;


//     [Header("Mirrored Depth Physics")]

//     [Tooltip(
//         "Kisi bhi 2+ depth-layer level mein hit block ka impact same X/Y " +
//         "par maujood baqi depth-layer blocks ko bhi unlock aur push karega. " +
//         "Ye gameplay link Mirror Paint authoring option se independent hai."
//     )]
//     [SerializeField]
//     private bool linkMirroredDepthLayerPhysics = true;

//     [Tooltip(
//         "Front block ke impulse ka kitna hissa mirrored layers ko mile."
//     )]
//     [SerializeField, Range(0f, 1.5f)]
//     private float mirroredLayerImpulseMultiplier = 0.45f;

//     [Tooltip(
//         "Mirrored depth layers exact same frame par na giren. Har linked " +
//         "block ko is range mein chhota natural delay milta hai."
//     )]
//     [SerializeField]
//     private Vector2 mirroredActivationDelayRange =
//         new Vector2(0.09f, 0.22f);

//     [Tooltip(
//         "Mirrored impulse ki strength mein per-block random variation."
//     )]
//     [SerializeField]
//     private Vector2 mirroredImpulseVariation =
//         new Vector2(0.78f, 0.96f);

//     [Tooltip(
//         "Mirrored force direction mein maximum random angle variation."
//     )]
//     [SerializeField, Range(0f, 25f)]
//     private float mirroredDirectionVariationDegrees = 4f;

//     [Tooltip(
//         "Linked blocks ko alag natural spin dene ke liye random torque."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float mirroredRandomTorqueMultiplier = 0.035f;


//     [Header("Events")]

//     [SerializeField]
//     private UnityEvent onLevelGenerated;

//     [SerializeField]
//     private UnityEvent onLevelComplete;


//     private sealed class DefinitionFitData
//     {
//         public Vector3 Scale;
//         public Vector3 BoundsCenterOffset;
//     }


//     private sealed class GridTrackingEntry
//     {
//         public Vector3 InitialPosition;
//         public int UnsupportedFrames;
//         public int TableIndex;
//         public Quaternion SurfaceRotation;
//     }


//     private readonly List<PhysicsTowerObject>
//         activeObjects =
//             new List<PhysicsTowerObject>();

//     /// <summary>
//     /// Keyed by (definition, span) since a bigger footprint (e.g. a
//     /// 2-cell-tall piece) needs its own fit scale, distinct from the
//     /// same definition used at 1x1x1.
//     /// </summary>
//     private readonly Dictionary<(PhysicsObjectDefinition, int spanKey), DefinitionFitData>
//         fitCache =
//             new Dictionary<(PhysicsObjectDefinition, int), DefinitionFitData>();

//     /// <summary>
//     /// What currently occupies each grid cell — used by the support
//     /// chain to answer "is the cell below me still standing?".
//     /// </summary>
//     private readonly Dictionary<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject>
//         occupancyByCoordinate =
//             new Dictionary<(int, Vector3Int), PhysicsTowerObject>();

//     private readonly Dictionary<PhysicsTowerObject, GridTrackingEntry>
//         trackingByInstance =
//             new Dictionary<PhysicsTowerObject, GridTrackingEntry>();


//     private readonly List<LevelTable> cachedTables =
//         new List<LevelTable>();

//     private readonly List<LevelTable> cachedTablePrefabs =
//         new List<LevelTable>();

//     private readonly List<int> tablePhysicsReleaseCycleTargets =
//         new List<int>();

//     private readonly HashSet<int> releasedTablePhysicsIndices =
//         new HashSet<int>();

//     private LevelTable currentTable;


//     private int remainingTargets;

//     private bool levelGenerated;

//     private bool isPropagatingMirroredActivation;

//     private int levelGenerationRevision;

//     private float runtimeTableAnimationTime;

//     private int currentLevelIndex;

//     private AsyncOperationHandle<GridLevelData>
//         activeLevelHandle;

//     private bool hasActiveLevelHandle;

//     private AsyncOperationHandle<GridLevelData>
//         pendingLevelHandle;

//     private bool hasPendingLevelHandle;

//     private bool isLoadingLevel;

//     private GameObject autoCreatedCompletePanel;

//     private CannonController cannonController;

//     private GameObject gameplayRestartButton;

//     /// <summary>
//     /// Lowest occupied grid row for the current level — the "floor"
//     /// row that always counts as supported by the table, even if the
//     /// designer didn't start painting at y=0.
//     /// </summary>
//     private readonly Dictionary<int, int> gridBaseRowByTable =
//         new Dictionary<int, int>();


//     public GridLevelData CurrentLevelData =>
//         levelData;

//     public LevelTable CurrentTable =>
//         currentTable;

//     public IReadOnlyList<LevelTable> CurrentTables =>
//         cachedTables;

//     public int RemainingTargets =>
//         remainingTargets;

//     public bool IsLevelGenerated =>
//         levelGenerated;

//     public int CurrentLevelIndex =>
//         currentLevelIndex;

//     public bool HasNextLevel =>
//         FindNextLevelIndex(currentLevelIndex + 1) >= 0;


//     private void Start()
//     {
//         Time.timeScale = 1f;

//         EnsureNextLevelUI();
//         SetLevelCompleteUIVisible(false);

//         if (ShouldStartLiveWorkingLevelPreview())
//         {
//             SetLevelCompleteUIVisible(false);
//             currentLevelIndex = 0;
//             GenerateLevel();
//             return;
//         }

//         SelectStartingLevel();

//         if (FindNextLevelIndex(currentLevelIndex) < 0)
//         {
//             Debug.LogError(
//                 "Addressable level catalog empty hai. Level Creator se " +
//                 "kam az kam ek level SAVE karein.",
//                 this
//             );
//             RefreshContinueButton();
//             return;
//         }

//         if (showMainMenuOnStartup)
//         {
//             ShowMainMenu();
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ShowMainMenu()
//     {
//         Time.timeScale = 1f;
//         SetLevelCompleteUIVisible(false);

//         EnsureMainMenuUI();
//         SetGameplayWorldVisible(false);
//         UpdateCurrentLevelText();

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(true);
//         }
//     }


//     public void PlayFromMainMenu()
//     {
//         Time.timeScale = 1f;
//         SetLevelCompleteUIVisible(false);

//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int playableIndex =
//             FindNextLevelIndex(currentLevelIndex);

//         if (playableIndex < 0)
//         {
//             Debug.LogError(
//                 "Play: koi saved Addressable level configured nahi hai.",
//                 this
//             );
//             return;
//         }

//         currentLevelIndex = playableIndex;

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(false);
//         }

//         SetGameplayWorldVisible(true);

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ResetProgressFromMainMenu()
//     {
//         PlayerPrefs.DeleteKey(SavedLevelNumberKey);
//         PlayerPrefs.Save();

//         int firstLevelIndex =
//             FindNextLevelIndex(0);

//         currentLevelIndex =
//             firstLevelIndex >= 0
//                 ? firstLevelIndex
//                 : 0;

//         UpdateCurrentLevelText();
//     }


//     public void LoadLevel(
//         GridLevelData newLevelData)
//     {
//         if (newLevelData == null)
//         {
//             Debug.LogError(
//                 "LoadLevel: GridLevelData null hai.",
//                 this
//             );

//             return;
//         }


//         if (hasActiveLevelHandle)
//         {
//             PrepareForLevelAssetRelease();
//             ReleaseActiveLevelHandle();
//         }

//         levelData = newLevelData;

//         int sequenceIndex =
//             levelDatabase != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     newLevelData.LevelNumber
//                 )
//                 : -1;

//         if (sequenceIndex >= 0)
//         {
//             currentLevelIndex = sequenceIndex;
//         }

//         SaveCurrentLevelProgress(
//             newLevelData.LevelNumber
//         );


//         GenerateLevel();
//     }


//     [ContextMenu("Generate Level")]
//     public void GenerateLevel()
//     {
//         EnsureNextLevelUI();
//         SetLevelCompleteUIVisible(false);
//         RefreshContinueButton();

//         if (!ValidateReferences())
//         {
//             return;
//         }


//         ClearCurrentLevel();

//         runtimeTableAnimationTime = 0f;


//         objectPool.PrepareForLevel(
//             levelData
//         );


//         if (!PrepareTable())
//         {
//             return;
//         }

//         PrepareTablePhysicsReleaseTargets();


//         remainingTargets =
//             0;


//         SpawnGrid();


//         Physics.SyncTransforms();


//         levelGenerated =
//             true;


//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} GENERATED\n" +
//             $"Boxes: {activeObjects.Count}\n" +
//             $"Targets: {remainingTargets}",
//             this
//         );


//         LevelGenerated?.Invoke(levelData);

//         onLevelGenerated?.Invoke();


//         if (remainingTargets <= 0)
//         {
//             Debug.LogWarning(
//                 "Generated level mein koi target nahi mila.",
//                 this
//             );
//         }
//     }


//     /// <summary>
//     /// Measures a definition's prefab once (identity rotation, prefab's
//     /// authored scale) and derives the localScale + pivot-to-collider
//     /// offset needed so its collider bounds exactly fill levelData's
//     /// CellSize. Cached per-definition so mixed shapes (cube, cylinder,
//     /// ...) all land cleanly on the same uniform grid without measuring
//     /// on every single cell spawn.
//     /// </summary>
//     private bool TryGetFitData(
//         PhysicsObjectDefinition definition,
//         int spanX,
//         int spanY,
//         int spanZ,
//         PieceOrientation orientation,
//         out DefinitionFitData fitData)
//     {
//         int spanKey =
//             spanX * 10000 +
//             spanY * 100 +
//             spanZ +
//             (int)orientation * 1000000;

//         (PhysicsObjectDefinition, int) cacheKey =
//             (definition, spanKey);

//         if (fitCache.TryGetValue(
//                 cacheKey,
//                 out fitData))
//         {
//             return true;
//         }

//         fitData = null;

//         PhysicsTowerObject measurementObject =
//             objectPool.Get(
//                 definition,
//                 Vector3.zero,
//                 Quaternion.identity,
//                 runtimeObjectsRoot
//             );

//         if (measurementObject == null)
//         {
//             Debug.LogError(
//                 "Grid object measure karne ke liye pool object nahi mila.",
//                 this
//             );

//             return false;
//         }

//         Physics.SyncTransforms();

//         if (!measurementObject.TryGetPhysicsBounds(
//                 out Bounds bounds))
//         {
//             Debug.LogError(
//                 "Physics object par valid non-trigger Collider nahi mila.",
//                 measurementObject
//             );

//             objectPool.Release(measurementObject);

//             return false;
//         }

//         Vector3 measuredSize =
//             bounds.size;

//         Vector3 measuredCenterOffset =
//             bounds.center -
//             measurementObject.transform.position;

//         objectPool.Release(measurementObject);

//         if (measuredSize.x <= 0.0001f ||
//             measuredSize.y <= 0.0001f ||
//             measuredSize.z <= 0.0001f)
//         {
//             Debug.LogError(
//                 $"{definition.DisplayName}: collider size invalid hai.",
//                 this
//             );

//             return false;
//         }

//         Vector3 authoredScale =
//             definition.Prefab.transform.localScale;

//         Vector3 finalScale;

//         Vector3 finalOffset;

//         if (definition.AutoFitToCell)
//         {
//             /*
//              * Span > 1 par target size sirf spanCount * cellSize nahi —
//              * internal gaps bhi fill hote hain (spanX*cellSize.x +
//              * (spanX-1)*gap) taake merged piece apne poore footprint
//              * ke outer edges tak seamless dikhe, jaisa cells alag alag
//              * (gap ke sath) hote to unka combined outer span hota.
//              */
//             Vector3 gridAlignedTargetSize =
//                 new Vector3(
//                     spanX * levelData.CellSize.x +
//                     (spanX - 1) * levelData.HorizontalGap,

//                     spanY * levelData.CellSize.y +
//                     (spanY - 1) * levelData.VerticalGap,

//                     spanZ * levelData.CellSize.z +
//                     (spanZ - 1) * levelData.DepthGap
//                 );

//             /*
//              * gridAlignedTargetSize world/grid axes ke liye hai.
//              * Lekin fit-scale prefab ke apne UNROTATED local axes par
//              * compute hoti hai — agar orientation tip kar rahi hai
//              * (Lying X/Z) to local axes rotate hone ke baad world axes
//              * se match nahi karte, is liye target size ko yahan
//              * local axes par re-map karte hain (spawn time par
//              * PieceOrientationUtility.GetRotation() isi mapping ko
//              * wapas world space mein rotate kar degi).
//              */
//             Vector3 targetSize =
//                 PieceOrientationUtility.GetLocalAxisTargetSize(
//                     gridAlignedTargetSize,
//                     orientation
//                 );

//             /*
//              * boundsAtScale1 = measuredSize / authoredScale
//              * finalScale     = targetSize / boundsAtScale1
//              *                = targetSize * authoredScale / measuredSize
//              *
//              * Isi tarah offset bhi authored scale se scale-1 basis par
//              * normalize karke, phir finalScale par re-apply hota hai —
//              * taake off-center collider pivot bhi correct rahe.
//              */
//             finalScale =
//                 new Vector3(
//                     targetSize.x * authoredScale.x / measuredSize.x,
//                     targetSize.y * authoredScale.y / measuredSize.y,
//                     targetSize.z * authoredScale.z / measuredSize.z
//                 );

//             Vector3 offsetAtScale1 =
//                 new Vector3(
//                     measuredCenterOffset.x / authoredScale.x,
//                     measuredCenterOffset.y / authoredScale.y,
//                     measuredCenterOffset.z / authoredScale.z
//                 );

//             finalOffset =
//                 Vector3.Scale(
//                     offsetAtScale1,
//                     finalScale
//                 );
//         }
//         else
//         {
//             finalScale =
//                 authoredScale;

//             finalOffset =
//                 measuredCenterOffset;
//         }

//         finalScale =
//             Vector3.Scale(
//                 finalScale,
//                 definition.ManualScaleMultiplier
//             );

//         fitData =
//             new DefinitionFitData
//             {
//                 Scale = finalScale,
//                 BoundsCenterOffset = finalOffset
//             };

//         fitCache[cacheKey] =
//             fitData;

//         return true;
//     }


//     private void SpawnGrid()
//     {
//         Vector3Int occupiedMin =
//             new Vector3Int(
//                 int.MaxValue,
//                 int.MaxValue,
//                 int.MaxValue
//             );

//         Vector3Int occupiedMax =
//             new Vector3Int(
//                 int.MinValue,
//                 int.MinValue,
//                 int.MinValue
//             );

//         if (currentTable == null ||
//             !currentTable.TryGetTowerSurface(
//                 out Vector3 primarySurfacePosition,
//                 out Quaternion primarySurfaceRotation))
//         {
//             Debug.LogError(
//                 "PRIMARY table ki tower surface calculate nahi ho saki.",
//                 currentTable
//             );

//             return;
//         }

//         int tableCount =
//             Mathf.Max(1, cachedTables.Count);

//         Vector3[] surfacePositions =
//             new Vector3[tableCount];

//         Quaternion[] surfaceRotations =
//             new Quaternion[tableCount];

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             LevelTable table =
//                 tableIndex < cachedTables.Count
//                     ? cachedTables[tableIndex]
//                     : null;

//             if (table != null &&
//                 table.TryGetTowerSurface(
//                     out Vector3 tableSurfacePosition,
//                     out Quaternion tableSurfaceRotation))
//             {
//                 surfacePositions[tableIndex] = tableSurfacePosition;
//                 surfaceRotations[tableIndex] = tableSurfaceRotation;
//             }
//             else
//             {
//                 surfacePositions[tableIndex] = primarySurfacePosition;
//                 surfaceRotations[tableIndex] = primarySurfaceRotation;
//             }
//         }

//         Dictionary<int, Vector3Int> occupiedMinByTable =
//             new Dictionary<int, Vector3Int>();

//         Dictionary<int, Vector3Int> occupiedMaxByTable =
//             new Dictionary<int, Vector3Int>();

//         bool foundAnyOccupiedCell = false;

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             if (!levelData.TryGetOccupiedBounds(
//                     tableIndex,
//                     out Vector3Int tableMinimum,
//                     out Vector3Int tableMaximum))
//             {
//                 continue;
//             }

//             foundAnyOccupiedCell = true;
//             occupiedMinByTable[tableIndex] = tableMinimum;
//             occupiedMaxByTable[tableIndex] = tableMaximum;
//             occupiedMin = Vector3Int.Min(occupiedMin, tableMinimum);
//             occupiedMax = Vector3Int.Max(occupiedMax, tableMaximum);
//         }

//         if (!foundAnyOccupiedCell)
//         {
//             Debug.LogError(
//                 "Kisi bhi table ke manual grid mein occupied cells nahi hain.",
//                 levelData
//             );

//             return;
//         }

//         gridBaseRowByTable.Clear();

//         foreach (KeyValuePair<int, Vector3Int> entry
//                  in occupiedMinByTable)
//         {
//             gridBaseRowByTable[entry.Key] =
//                 entry.Value.y;
//         }


//         Vector3 cellSize =
//             levelData.CellSize;


//         float stepX =
//             cellSize.x +
//             levelData.HorizontalGap;


//         float stepY =
//             cellSize.y +
//             levelData.VerticalGap;


//         float stepZ =
//             cellSize.z +
//             levelData.DepthGap;


//         /*
//          * Important:
//          *
//          * Y outer loop hai.
//          * Isliye logical spawn order bottom → top rahega.
//          */
//         for (int y = occupiedMin.y;
//              y <= occupiedMax.y;
//              y++)
//         {
//             for (int z = occupiedMin.z;
//                  z <= occupiedMax.z;
//                  z++)
//             {
//                 for (int x = occupiedMin.x;
//                      x <= occupiedMax.x;
//                      x++)
//                 {
//                     for (int authoredTableIndex = 0;
//                          authoredTableIndex < tableCount;
//                          authoredTableIndex++)
//                     {
//                         if (!occupiedMinByTable.ContainsKey(
//                                 authoredTableIndex))
//                         {
//                             continue;
//                         }

//                         GridCellData cell =
//                             levelData.GetCell(
//                                 x,
//                                 y,
//                                 z,
//                                 authoredTableIndex
//                             );


//                     if (cell == null ||
//                         !cell.Occupied)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Covered cells (bare footprint ka hissa) khud
//                      * spawn nahi hoti — unka anchor cell hi spawn
//                      * karta hai (neeche is loop mein aage aake).
//                      */
//                     if (cell.IsCovered)
//                     {
//                         continue;
//                     }


//                     int blockTableIndex =
//                         authoredTableIndex;

//                     Vector3Int tableOccupiedMin =
//                         occupiedMinByTable[blockTableIndex];

//                     Vector3Int tableOccupiedMax =
//                         occupiedMaxByTable[blockTableIndex];

//                     float occupiedCenterX =
//                         (
//                             tableOccupiedMin.x +
//                             tableOccupiedMax.x
//                         ) *
//                         0.5f;

//                     float occupiedCenterZ =
//                         (
//                             tableOccupiedMin.z +
//                             tableOccupiedMax.z
//                         ) *
//                         0.5f;

//                     Vector3 surfacePosition =
//                         surfacePositions[blockTableIndex];

//                     Quaternion surfaceRotation =
//                         surfaceRotations[blockTableIndex];


//                     int spanX =
//                         Mathf.Max(1, cell.SpanX);

//                     int spanY =
//                         Mathf.Max(1, cell.SpanY);

//                     int spanZ =
//                         Mathf.Max(1, cell.SpanZ);


//                     /*
//                      * X:
//                      * Shape automatically center.
//                      *
//                      * Span > 1 ho to poore footprint (x..x+spanX-1)
//                      * ka center use hota hai, sirf anchor cell ka nahi.
//                      */
//                     float localX =
//                         (
//                             x -
//                             occupiedCenterX
//                         ) *
//                         stepX +
//                         (spanX - 1) *
//                         stepX *
//                         0.5f +
//                         cell.LocalOffset.x *
//                         stepX;


//                     /*
//                      * Y:
//                      * Lowest occupied manual-grid row
//                      * directly table top par start hogi.
//                      *
//                      * Transparent bottom margin
//                      * automatically ignore hota hai.
//                      */
//                     float localY =
//                         cellSize.y *
//                         0.5f +
//                         (
//                             y -
//                             tableOccupiedMin.y
//                         ) *
//                         stepY +
//                         (spanY - 1) *
//                         stepY *
//                         0.5f +
//                         cell.LocalOffset.y *
//                         stepY;


//                     /*
//                      * Z:
//                      * Complete depth centered.
//                      */
//                     float localZ =
//                         (
//                             z -
//                             occupiedCenterZ
//                         ) *
//                         stepZ +
//                         (spanZ - 1) *
//                         stepZ *
//                         0.5f +
//                         cell.LocalOffset.z *
//                         stepZ;


//                     Vector3 localCellCenter =
//                         new Vector3(
//                             localX,
//                             localY,
//                             localZ
//                         )
//                         +
//                         levelData.GridOffset;


//                     PhysicsObjectDefinition definition =
//                         levelData.GetPaletteEntry(
//                             cell.DefinitionIndex
//                         );

//                     if (definition == null ||
//                         definition.Prefab == null)
//                     {
//                         Debug.LogWarning(
//                             $"Cell ({x},{y},{z}): palette entry " +
//                             $"{cell.DefinitionIndex} invalid hai, skip.",
//                             levelData
//                         );

//                         continue;
//                     }

//                     PieceOrientation orientation =
//                         cell.Orientation;

//                     if (!TryGetFitData(
//                             definition,
//                             spanX,
//                             spanY,
//                             spanZ,
//                             orientation,
//                             out DefinitionFitData fitData))
//                     {
//                         continue;
//                     }


//                     /*
//                      * Orientation ka extra tip (Lying X/Z) surface
//                      * rotation ke UPAR apply hoti hai — piece ko table
//                      * ke sath sath khud bhi rotate karta hai.
//                      */
//                     Quaternion pieceRotation =
//                         surfaceRotation *
//                         PieceOrientationUtility.GetRotation(
//                             orientation,
//                             cell.CustomZRotation
//                         ) *
//                         Quaternion.Euler(
//                             cell.RotationEulerOffset
//                         );


//                     Vector3 pieceScale =
//                         Vector3.Scale(
//                             fitData.Scale,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 desiredBoundsCenter =
//                         surfacePosition +
//                         surfaceRotation *
//                         localCellCenter;


//                     /*
//                      * Prefab pivot center par na bhi ho
//                      * to collider bottom proper align hoga.
//                      */
//                     Vector3 rotatedBoundsOffset =
//                         pieceRotation *
//                         Vector3.Scale(
//                             fitData.BoundsCenterOffset,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 spawnPosition =
//                         desiredBoundsCenter -
//                         rotatedBoundsOffset;


//                     PhysicsTowerObject instance =
//                         objectPool.Get(
//                             definition,
//                             spawnPosition,
//                             pieceRotation,
//                             runtimeObjectsRoot
//                         );


//                     if (instance == null)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Pool.Get() prefab ki authored scale laga deta hai —
//                      * yahan usay cell-fit scale se override karte hain
//                      * taake mixed shapes (cube/cylinder) uniform grid
//                      * mein clean fit hon.
//                      */
//                     instance.transform.localScale =
//                         pieceScale;


//                     /*
//                      * Pool se purani property-block state clear karo.
//                      * Default visual hamesha prefab ka apna material aur
//                      * texture hai; paint tint definition par optional hai.
//                      */
//                     instance.RestorePrefabVisual();

//                     if (definition.TintWithPaintColor)
//                     {
//                         instance.SetVisualColor(
//                             cell.Color
//                         );
//                     }


//                     Vector3Int anchorCoordinate =
//                         new Vector3Int(x, y, z);

//                     instance.SetGridFootprint(
//                         anchorCoordinate,
//                         new Vector3Int(
//                             spanX,
//                             spanY,
//                             spanZ
//                         ),
//                         cell.LocalOffset
//                     );

//                     instance.ConfigureBreakable(
//                         cell.Breakable,
//                         cell.HitsToBreak
//                     );

//                     /*
//                      * Bare footprint ke SAARE cells is instance ko
//                      * point karte hain — is se agar upar kuch spawn ho
//                      * to woh footprint ke kisi bhi column par "supported"
//                      * sahi detect hoga.
//                      */
//                     for (int fz = 0; fz < spanZ; fz++)
//                     {
//                         for (int fy = 0; fy < spanY; fy++)
//                         {
//                             for (int fx = 0; fx < spanX; fx++)
//                             {
//                                 occupancyByCoordinate[
//                                     (
//                                         blockTableIndex,
//                                         new Vector3Int(
//                                             x + fx,
//                                             y + fy,
//                                             z + fz
//                                         )
//                                     )
//                                 ] = instance;
//                             }
//                         }
//                     }

//                     trackingByInstance[instance] =
//                         new GridTrackingEntry
//                         {
//                             InitialPosition = spawnPosition,
//                             UnsupportedFrames = 0,
//                             TableIndex = blockTableIndex,
//                             SurfaceRotation = surfaceRotation
//                         };


//                     instance.Cleared +=
//                         HandleObjectCleared;

//                     instance.Activated +=
//                         HandleObjectActivated;


//                     activeObjects.Add(
//                         instance
//                     );


//                     if (instance.CountsAsTarget)
//                     {
//                         remainingTargets++;
//                     }
//                     }
//                 }
//             }
//         }
//     }


//     private int ResolveBlockTableIndex(int requestedIndex)
//     {
//         if (requestedIndex >= 0 &&
//             requestedIndex < cachedTables.Count &&
//             cachedTables[requestedIndex] != null)
//         {
//             return requestedIndex;
//         }

//         return 0;
//     }


//     private void FixedUpdate()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }

//         AnimateEnabledTables();

//         if (activeObjects.Count == 0)
//         {
//             return;
//         }

//         /*
//          * Snapshot: activeObjects FixedUpdate ke dauran
//          * HandleObjectCleared se modify ho sakti hai
//          * (ActivatePhysics() khud kuch remove nahi karta,
//          * lekin safety ke liye copy le lete hain).
//          */
//         for (int i = activeObjects.Count - 1; i >= 0; i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];

//             if (instance == null || !instance.IsLocked)
//             {
//                 continue;
//             }

//             if (!trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry))
//             {
//                 continue;
//             }

//             if (IsSupported(
//                     instance,
//                     entry,
//                     out Vector3 unsupportedLocalDirection))
//             {
//                 entry.UnsupportedFrames = 0;
//                 continue;
//             }

//             entry.UnsupportedFrames++;

//             if (entry.UnsupportedFrames >=
//                 unsupportedFramesRequired)
//             {
//                 Vector3 worldFallDirection =
//                     entry.SurfaceRotation *
//                     unsupportedLocalDirection.normalized;

//                 Vector3 worldUp =
//                     entry.SurfaceRotation * Vector3.up;

//                 Vector3 tippingTorque =
//                     Vector3.Cross(
//                         worldUp,
//                         worldFallDirection
//                     ).normalized *
//                     unsupportedTipTorque *
//                     Random.Range(0.9f, 1.1f);

//                 instance.ActivatePhysics(
//                     worldFallDirection *
//                     unsupportedTipImpulse *
//                     Random.Range(0.9f, 1.1f),
//                     tippingTorque
//                 );
//             }
//         }
//     }


//     /// <summary>
//     /// Bottom row table par khadi hoti hai. Baqi pieces ke poore bottom
//     /// footprint mein kam az kam ek LOCKED support hona chahiye. Hit hua
//     /// dynamic block foran support count hona band ho jata hai, chahe woh
//     /// abhi apni spawn position se zyada move na bhi hua ho.
//     ///
//     /// Multi-cell footprint (span > 1) wale pieces ke liye sirf
//     /// "kahin bhi neeche support hai" (OR) kaafi nahi — agar ek extreme
//     /// edge (min ya max column) ka support hat jaye to piece ko us taraf
//     /// tip hona chahiye, chahe doosra extreme abhi bhi supported ho.
//     /// Ye seam-offset pieces (0.5 Center-on-Seam) aur plain grid-aligned
//     /// multi-span pieces, dono par apply hota hai.
//     /// </summary>
//     private bool IsSupported(
//         PhysicsTowerObject instance,
//         GridTrackingEntry trackingEntry,
//         out Vector3 unsupportedLocalDirection)
//     {
//         unsupportedLocalDirection = Vector3.zero;

//         if (instance == null)
//         {
//             unsupportedLocalDirection = Vector3.right;
//             return false;
//         }

//         Vector3Int coordinate =
//             instance.GridCoordinate;

//         int baseRow =
//             gridBaseRowByTable.TryGetValue(
//                 trackingEntry.TableIndex,
//                 out int tableBaseRow)
//                 ? tableBaseRow
//                 : coordinate.y;

//         if (coordinate.y <= baseRow)
//         {
//             return true;
//         }

//         Vector3Int span =
//             instance.GridSpan;

//         Vector3 localOffset =
//             instance.GridLocalOffset;

//         int minimumX =
//             coordinate.x +
//             Mathf.FloorToInt(localOffset.x);

//         int maximumX =
//             coordinate.x +
//             Mathf.Max(1, span.x) - 1 +
//             Mathf.CeilToInt(localOffset.x);

//         int minimumZ =
//             coordinate.z +
//             Mathf.FloorToInt(localOffset.z);

//         int maximumZ =
//             coordinate.z +
//             Mathf.Max(1, span.z) - 1 +
//             Mathf.CeilToInt(localOffset.z);

//         bool seamOnX =
//             !Mathf.Approximately(
//                 localOffset.x,
//                 Mathf.Round(localOffset.x)
//             );

//         bool seamOnZ =
//             !Mathf.Approximately(
//                 localOffset.z,
//                 Mathf.Round(localOffset.z)
//             );

//         /*
//          * Seam offset ke bina bhi agar piece ek se zyada grid column
//          * span kar raha hai (e.g. horizontal 2-cell beam), to sirf
//          * "kahin bhi support hai" kaafi nahi — dono extremes (min aur
//          * max column) ka support alag alag check hona chahiye, warna
//          * ek pillar hat jane par doosre pillar ka sahara poore beam
//          * ko galat tarah "supported" dikha deta hai.
//          */
//         bool spansMultipleColumnsX =
//             maximumX > minimumX;

//         bool spansMultipleColumnsZ =
//             maximumZ > minimumZ;

//         bool hasAnySupport = false;
//         bool minimumXSupported = false;
//         bool maximumXSupported = false;
//         bool minimumZSupported = false;
//         bool maximumZSupported = false;

//         for (int z = minimumZ; z <= maximumZ; z++)
//         {
//             for (int x = minimumX; x <= maximumX; x++)
//             {
//                 Vector3Int below =
//                     new Vector3Int(
//                         x,
//                         coordinate.y - 1,
//                         z
//                     );

//                 if (occupancyByCoordinate.TryGetValue(
//                         (trackingEntry.TableIndex, below),
//                         out PhysicsTowerObject supportInstance) &&
//                     supportInstance != null &&
//                     supportInstance != instance &&
//                     supportInstance.IsLocked)
//                 {
//                     hasAnySupport = true;
//                     minimumXSupported |= x == minimumX;
//                     maximumXSupported |= x == maximumX;
//                     minimumZSupported |= z == minimumZ;
//                     maximumZSupported |= z == maximumZ;
//                 }
//             }
//         }

//         if (!hasAnySupport)
//         {
//             unsupportedLocalDirection =
//                 GetNaturalUnsupportedDirection(
//                     coordinate,
//                     localOffset
//                 );

//             return false;
//         }

//         if ((seamOnX || spansMultipleColumnsX) &&
//             (!minimumXSupported || !maximumXSupported))
//         {
//             if (!minimumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.left;
//             }

//             if (!maximumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.right;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.x >= 0f
//                         ? Vector3.right
//                         : Vector3.left;
//             }

//             return false;
//         }

//         if ((seamOnZ || spansMultipleColumnsZ) &&
//             (!minimumZSupported || !maximumZSupported))
//         {
//             if (!minimumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.back;
//             }

//             if (!maximumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.forward;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.z >= 0f
//                         ? Vector3.forward
//                         : Vector3.back;
//             }

//             return false;
//         }

//         return true;
//     }


//     private static Vector3 GetNaturalUnsupportedDirection(
//         Vector3Int coordinate,
//         Vector3 localOffset)
//     {
//         Vector3 direction =
//             new Vector3(
//                 localOffset.x,
//                 0f,
//                 localOffset.z
//             );

//         if (direction.sqrMagnitude > 0.001f)
//         {
//             return direction.normalized;
//         }

//         int hash =
//             coordinate.x * 73856093 ^
//             coordinate.y * 19349663 ^
//             coordinate.z * 83492791;

//         float x =
//             (hash & 1) == 0 ? -1f : 1f;

//         float z =
//             (hash & 2) == 0 ? -0.45f : 0.45f;

//         return new Vector3(x, 0f, z).normalized;
//     }


//     private bool PrepareTable()
//     {
//         int requiredTableCount =
//             levelData.TableCount;

//         if (requiredTableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "GridLevelData mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }

//         while (cachedTables.Count > requiredTableCount)
//         {
//             DestroyCachedTableAt(
//                 cachedTables.Count - 1
//             );

//             cachedTables.RemoveAt(
//                 cachedTables.Count - 1
//             );

//             cachedTablePrefabs.RemoveAt(
//                 cachedTablePrefabs.Count - 1
//             );
//         }

//         Transform levelRoot =
//             GetLevelOrigin();

//         while (cachedTables.Count < requiredTableCount)
//         {
//             cachedTables.Add(null);
//             cachedTablePrefabs.Add(null);
//         }

//         for (int index = 0;
//              index < requiredTableCount;
//              index++)
//         {
//             LevelTable requiredPrefab =
//                 levelData.GetTablePrefab(index);

//             if (requiredPrefab == null)
//             {
//                 DestroyCachedTableAt(index);

//                 Debug.LogWarning(
//                     $"Table Placements element {index} ka Prefab missing hai; " +
//                     "is extra table ko skip kiya gaya.",
//                     levelData
//                 );

//                 continue;
//             }

//             if (cachedTables[index] == null ||
//                 cachedTablePrefabs[index] != requiredPrefab)
//             {
//                 DestroyCachedTableAt(index);

//                 LevelTable instance =
//                     Instantiate(
//                         requiredPrefab,
//                         levelRoot
//                     );

//                 instance.name =
//                     requiredPrefab.name +
//                     $"_Runtime_{index + 1}";

//                 cachedTables[index] = instance;
//                 cachedTablePrefabs[index] = requiredPrefab;
//             }

//             LevelTable table =
//                 cachedTables[index];

//             Transform tableTransform =
//                 table.transform;

//             if (tableTransform.parent != levelRoot)
//             {
//                 tableTransform.SetParent(
//                     levelRoot,
//                     false
//                 );
//             }

//             tableTransform.localPosition =
//                 levelData.GetTablePositionOffset(index);

//             tableTransform.localRotation =
//                 Quaternion.Euler(
//                     levelData.GetTableRotationEuler(index)
//                 );

//             tableTransform.localScale =
//                 Vector3.Scale(
//                     requiredPrefab.transform.localScale,
//                     levelData.GetTableSizeMultiplier(index)
//                 );

//             table.gameObject.SetActive(true);

//             if (table.TowerSurfaceCollider == null)
//             {
//                 Debug.LogError(
//                     $"Table Placements element {index} ke prefab par " +
//                     "Tower Surface Collider missing hai.",
//                     table
//                 );

//                 return false;
//             }
//         }

//         currentTable = cachedTables[0];

//         return true;
//     }


//     private void DestroyCachedTableAt(int index)
//     {
//         if (index < 0 ||
//             index >= cachedTables.Count)
//         {
//             return;
//         }

//         LevelTable table =
//             cachedTables[index];

//         if (table != null)
//         {
//             if (Application.isPlaying)
//             {
//                 Destroy(table.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(table.gameObject);
//             }
//         }

//         cachedTables[index] = null;

//         if (index < cachedTablePrefabs.Count)
//         {
//             cachedTablePrefabs[index] = null;
//         }
//     }


//     private void AnimateEnabledTables()
//     {
//         if (levelData == null ||
//             cachedTables.Count == 0)
//         {
//             return;
//         }

//         runtimeTableAnimationTime +=
//             Time.fixedDeltaTime;

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             LevelTable table = cachedTables[tableIndex];
//             bool rotationEnabled =
//                 levelData.IsTableRuntimeRotationEnabled(
//                     tableIndex
//                 );
//             bool movementEnabled =
//                 levelData.IsTableRuntimeHorizontalMovementEnabled(
//                     tableIndex
//                 );

//             if (table == null ||
//                 (!rotationEnabled && !movementEnabled))
//             {
//                 continue;
//             }

//             Vector3 rotationAxis =
//                 levelData.GetTableRuntimeRotationAxis(tableIndex);
//             float rotationSpeed =
//                 levelData.GetTableRuntimeRotationSpeed(tableIndex);
//             Vector3 movementAxis =
//                 levelData.GetTableRuntimeMovementAxis(tableIndex);
//             float movementDistance =
//                 levelData.GetTableRuntimeMovementDistance(tableIndex);
//             float movementSpeed =
//                 levelData.GetTableRuntimeMovementSpeed(tableIndex);

//             bool canRotate =
//                 rotationEnabled &&
//                 rotationAxis.sqrMagnitude >= 0.0001f &&
//                 !Mathf.Approximately(rotationSpeed, 0f);

//             bool canMove =
//                 movementEnabled &&
//                 movementAxis.sqrMagnitude >= 0.0001f &&
//                 movementDistance > 0f &&
//                 movementSpeed > 0f;

//             if (!canRotate && !canMove)
//             {
//                 continue;
//             }

//             Transform tableTransform = table.transform;
//             Vector3 oldWorldPosition = tableTransform.position;
//             Quaternion oldWorldRotation = tableTransform.rotation;

//             if (canMove)
//             {
//                 float movementPhase =
//                     runtimeTableAnimationTime *
//                     movementSpeed *
//                     Mathf.PI * 2f;

//                 tableTransform.localPosition =
//                     levelData.GetTablePositionOffset(tableIndex) +
//                     movementAxis.normalized *
//                     Mathf.Sin(movementPhase) *
//                     movementDistance;
//             }

//             if (canRotate)
//             {
//                 tableTransform.localRotation =
//                     tableTransform.localRotation *
//                     Quaternion.AngleAxis(
//                         rotationSpeed * Time.fixedDeltaTime,
//                         rotationAxis.normalized
//                     );
//             }

//             Vector3 newWorldPosition = tableTransform.position;
//             Vector3 tableVelocity =
//                 (newWorldPosition - oldWorldPosition) /
//                 Mathf.Max(0.0001f, Time.fixedDeltaTime);
//             Quaternion worldRotationDelta =
//                 tableTransform.rotation *
//                 Quaternion.Inverse(oldWorldRotation);

//             for (int objectIndex = 0;
//                  objectIndex < activeObjects.Count;
//                  objectIndex++)
//             {
//                 PhysicsTowerObject instance =
//                     activeObjects[objectIndex];

//                 if (instance == null ||
//                     !instance.IsLocked ||
//                     !trackingByInstance.TryGetValue(
//                         instance,
//                         out GridTrackingEntry entry) ||
//                     entry.TableIndex != tableIndex)
//                 {
//                     continue;
//                 }

//                 Rigidbody objectBody = instance.Body;
//                 Vector3 objectPosition =
//                     objectBody != null
//                         ? objectBody.position
//                         : instance.transform.position;
//                 Quaternion objectRotation =
//                     objectBody != null
//                         ? objectBody.rotation
//                         : instance.transform.rotation;
//                 Vector3 animatedPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (objectPosition - oldWorldPosition);

//                 instance.MoveLockedWithTable(
//                     animatedPosition,
//                     worldRotationDelta * objectRotation
//                 );

//                 entry.InitialPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (entry.InitialPosition - oldWorldPosition);

//                 entry.SurfaceRotation =
//                     worldRotationDelta * entry.SurfaceRotation;
//             }

//             TryReleaseTableBlocksAfterMovementCycles(
//                 tableIndex,
//                 canMove ? movementSpeed : 0f,
//                 tableVelocity
//             );
//         }
//     }


//     private void PrepareTablePhysicsReleaseTargets()
//     {
//         tablePhysicsReleaseCycleTargets.Clear();
//         releasedTablePhysicsIndices.Clear();

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             if (!levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                     tableIndex))
//             {
//                 tablePhysicsReleaseCycleTargets.Add(int.MaxValue);
//                 continue;
//             }

//             int minimumCycles =
//                 Mathf.Max(
//                     1,
//                     levelData.GetTableMinimumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );
//             int maximumCycles =
//                 Mathf.Max(
//                     minimumCycles,
//                     levelData.GetTableMaximumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );

//             tablePhysicsReleaseCycleTargets.Add(
//                 Random.Range(
//                     minimumCycles,
//                     maximumCycles + 1
//                 )
//             );
//         }
//     }


//     private void TryReleaseTableBlocksAfterMovementCycles(
//         int tableIndex,
//         float movementSpeed,
//         Vector3 tableVelocity)
//     {
//         if (tableIndex < 0 ||
//             tableIndex >= tablePhysicsReleaseCycleTargets.Count ||
//             releasedTablePhysicsIndices.Contains(tableIndex) ||
//             !levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                 tableIndex) ||
//             movementSpeed <= 0f)
//         {
//             return;
//         }

//         float completedCycles =
//             runtimeTableAnimationTime * movementSpeed;

//         if (completedCycles <
//             tablePhysicsReleaseCycleTargets[tableIndex])
//         {
//             return;
//         }

//         releasedTablePhysicsIndices.Add(tableIndex);

//         float velocityMultiplier =
//             levelData.GetTableMovementReleaseVelocityMultiplier(
//                 tableIndex
//             );

//         bool wasPropagatingMirroredActivation =
//             isPropagatingMirroredActivation;

//         isPropagatingMirroredActivation = true;

//         for (int objectIndex = activeObjects.Count - 1;
//              objectIndex >= 0;
//              objectIndex--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[objectIndex];

//             if (instance == null ||
//                 !instance.IsLocked ||
//                 !trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry) ||
//                 entry.TableIndex != tableIndex)
//             {
//                 continue;
//             }

//             Rigidbody body = instance.Body;
//             float bodyMass =
//                 body != null
//                     ? Mathf.Max(0.01f, body.mass)
//                     : 1f;

//             Vector3 naturalVariation =
//                 new Vector3(
//                     Random.Range(-0.08f, 0.08f),
//                     Random.Range(0.015f, 0.07f),
//                     Random.Range(-0.08f, 0.08f)
//                 );

//             Vector3 inheritedImpulse =
//                 (tableVelocity * velocityMultiplier +
//                  naturalVariation) *
//                 bodyMass;

//             Vector3 naturalTorque =
//                 Random.onUnitSphere *
//                 Random.Range(0.035f, 0.12f) *
//                 bodyMass;

//             instance.ActivatePhysics(
//                 inheritedImpulse,
//                 naturalTorque
//             );
//         }

//         isPropagatingMirroredActivation =
//             wasPropagatingMirroredActivation;
//     }


//     private void HandleObjectActivated(
//         PhysicsTowerObject source,
//         Vector3 sourceImpulse)
//     {
//         if (source == null ||
//             levelData == null ||
//             !linkMirroredDepthLayerPhysics ||
//             levelData.GridDepth <= 1 ||
//             isPropagatingMirroredActivation)
//         {
//             return;
//         }

//         Vector3Int sourceCoordinate =
//             source.GridCoordinate;

//         if (!trackingByInstance.TryGetValue(
//                 source,
//                 out GridTrackingEntry sourceTracking))
//         {
//             return;
//         }

//         int sourceTableIndex =
//             sourceTracking.TableIndex;

//         int activationRevision =
//             levelGenerationRevision;

//         for (int z = 0;
//              z < levelData.GridDepth;
//              z++)
//         {
//             if (z == sourceCoordinate.z ||
//                 !TryFindDepthLinkedObject(
//                     source,
//                     sourceCoordinate,
//                     sourceTableIndex,
//                     z,
//                     out PhysicsTowerObject mirroredObject,
//                     out Vector3Int mirroredCoordinate))
//             {
//                 continue;
//             }

//             StartCoroutine(
//                 ActivateMirroredObjectNaturally(
//                     mirroredObject,
//                     mirroredCoordinate,
//                     sourceTableIndex,
//                     sourceImpulse,
//                     activationRevision
//                 )
//             );
//         }
//     }


//     private bool TryFindDepthLinkedObject(
//         PhysicsTowerObject source,
//         Vector3Int sourceCoordinate,
//         int tableIndex,
//         int targetDepth,
//         out PhysicsTowerObject linkedObject,
//         out Vector3Int linkedCoordinate)
//     {
//         linkedObject = null;
//         linkedCoordinate = new Vector3Int(
//             sourceCoordinate.x,
//             sourceCoordinate.y,
//             targetDepth
//         );

//         if (TryGetLockedDepthObject(
//                 linkedCoordinate,
//                 tableIndex,
//                 source,
//                 out linkedObject))
//         {
//             return true;
//         }

//         /*
//          * Manually-authored layers can have different silhouettes. In that
//          * case there may be no block at the exact X/Y coordinate behind the
//          * hit. Find the nearest locked block in a small projected area so the
//          * back structure still receives a believable local impact rather
//          * than remaining completely static.
//          */
//         const int searchRadius = 2;
//         float bestScore = float.PositiveInfinity;

//         for (int yOffset = -searchRadius;
//              yOffset <= searchRadius;
//              yOffset++)
//         {
//             for (int xOffset = -searchRadius;
//                  xOffset <= searchRadius;
//                  xOffset++)
//             {
//                 if (xOffset == 0 && yOffset == 0)
//                 {
//                     continue;
//                 }

//                 Vector3Int candidateCoordinate =
//                     new Vector3Int(
//                         sourceCoordinate.x + xOffset,
//                         sourceCoordinate.y + yOffset,
//                         targetDepth
//                     );

//                 if (!TryGetLockedDepthObject(
//                         candidateCoordinate,
//                         tableIndex,
//                         source,
//                         out PhysicsTowerObject candidate))
//                 {
//                     continue;
//                 }

//                 float score =
//                     xOffset * xOffset +
//                     yOffset * yOffset * 1.5f;

//                 if (score >= bestScore)
//                 {
//                     continue;
//                 }

//                 bestScore = score;
//                 linkedObject = candidate;
//                 linkedCoordinate = candidateCoordinate;
//             }
//         }

//         return linkedObject != null;
//     }


//     private bool TryGetLockedDepthObject(
//         Vector3Int coordinate,
//         int tableIndex,
//         PhysicsTowerObject source,
//         out PhysicsTowerObject candidate)
//     {
//         return occupancyByCoordinate.TryGetValue(
//                    (tableIndex, coordinate),
//                    out candidate) &&
//                candidate != null &&
//                candidate != source &&
//                candidate.IsLocked;
//     }


//     private IEnumerator ActivateMirroredObjectNaturally(
//         PhysicsTowerObject mirroredObject,
//         Vector3Int mirroredCoordinate,
//         int tableIndex,
//         Vector3 sourceImpulse,
//         int activationRevision)
//     {
//         float minimumDelay =
//             Mathf.Max(0f, mirroredActivationDelayRange.x);

//         float maximumDelay =
//             Mathf.Max(minimumDelay, mirroredActivationDelayRange.y);

//         float delay =
//             Random.Range(minimumDelay, maximumDelay);

//         if (delay > 0f)
//         {
//             yield return new WaitForSeconds(delay);
//         }

//         if (activationRevision != levelGenerationRevision ||
//             mirroredObject == null ||
//             !mirroredObject.IsLocked ||
//             !occupancyByCoordinate.TryGetValue(
//                 (tableIndex, mirroredCoordinate),
//                 out PhysicsTowerObject currentObject) ||
//             currentObject != mirroredObject)
//         {
//             yield break;
//         }

//         float minimumStrength =
//             Mathf.Max(0f, mirroredImpulseVariation.x);

//         float maximumStrength =
//             Mathf.Max(minimumStrength, mirroredImpulseVariation.y);

//         float randomStrength =
//             Random.Range(minimumStrength, maximumStrength);

//         Vector3 randomAngles =
//             new Vector3(
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 )
//             );

//         Vector3 variedImpulse =
//             Quaternion.Euler(randomAngles) *
//             sourceImpulse *
//             mirroredLayerImpulseMultiplier *
//             randomStrength;

//         Vector3 randomTorque =
//             sourceImpulse.sqrMagnitude > 0.0001f
//                 ? Random.onUnitSphere *
//                   sourceImpulse.magnitude *
//                   mirroredRandomTorqueMultiplier *
//                   Random.Range(0.65f, 1.25f)
//                 : Vector3.zero;

//         isPropagatingMirroredActivation = true;

//         try
//         {
//             mirroredObject.ActivatePhysics(
//                 variedImpulse,
//                 randomTorque
//             );
//         }
//         finally
//         {
//             isPropagatingMirroredActivation = false;
//         }
//     }


//     private void HandleObjectCleared(
//         PhysicsTowerObject target)
//     {
//         if (target == null)
//         {
//             return;
//         }


//         target.Cleared -=
//             HandleObjectCleared;

//         target.Activated -=
//             HandleObjectActivated;


//         activeObjects.Remove(
//             target
//         );

//         trackingByInstance.Remove(
//             target
//         );

//         /*
//          * Multi-cell footprint wale piece ke liye occupancyByCoordinate
//          * mein uske SAARE covered cells target ko point karte hain
//          * (sirf anchor nahi) — is liye scan karke sab remove karte hain,
//          * warna stale entries agle level mein reused instance ko
//          * galat tarah "still standing" dikha sakti hain.
//          */
//         List<(int tableIndex, Vector3Int coordinate)> coordinatesToRemove = null;

//         foreach (KeyValuePair<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject> entry
//                  in occupancyByCoordinate)
//         {
//             if (entry.Value != target)
//             {
//                 continue;
//             }

//             if (coordinatesToRemove == null)
//             {
//                 coordinatesToRemove =
//                     new List<(int, Vector3Int)>();
//             }

//             coordinatesToRemove.Add(entry.Key);
//         }

//         if (coordinatesToRemove != null)
//         {
//             foreach ((int tableIndex, Vector3Int coordinate) key
//                      in coordinatesToRemove)
//             {
//                 occupancyByCoordinate.Remove(key);
//             }
//         }


//         if (target.CountsAsTarget)
//         {
//             remainingTargets =
//                 Mathf.Max(
//                     0,
//                     remainingTargets - 1
//                 );
//         }


//         objectPool.Release(
//             target
//         );


//         if (levelGenerated &&
//             remainingTargets == 0)
//         {
//             CompleteLevel();
//         }
//     }


//     private void CompleteLevel()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }


//         levelGenerated =
//             false;


//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} COMPLETE",
//             this
//         );

// //  if (milestoneProgressBar != null)
// //     {
// //         milestoneProgressBar.OnLevelCompleted(
// //             levelData.LevelNumber
// //         );
// //     }


//         onLevelComplete?.Invoke();

//         ShowLevelCompleteUI();

//         if (pauseGameplayOnLevelComplete)
//         {
//             Time.timeScale = 0f;
//         }
//     }




//     public void ContinueToNextLevel()
//     {
//         if (isLoadingLevel)
//         {
//             return;
//         }

//         if (!HasNextLevel)
//         {
//             Debug.LogWarning(
//                 "Continue: next Addressable level configured nahi hai.",
//                 this
//             );
//             return;
//         }

//         Time.timeScale = 1f;
//         SetLevelCompleteUIVisible(false);

//         LoadNextLevel();
//     }


//     public void LoadNextLevel()
//     {
//         Time.timeScale = 1f;
//         SetLevelCompleteUIVisible(false);

//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int nextIndex =
//             FindNextLevelIndex(
//                 currentLevelIndex + 1
//             );

//         if (nextIndex < 0)
//         {
//             Debug.LogWarning(
//                 "Next Addressable level configured nahi hai. Level " +
//                 "Creator se next level SAVE karein.",
//                 this
//             );

//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(nextIndex)
//         );
//     }

//     public void RestartCurrentLevel()
//     {
//         if (isLoadingLevel ||
//             levelData == null)
//         {
//             return;
//         }

//         DestroyActiveCannonBalls();
//         GenerateLevel();
//     }


//     private IEnumerator LoadAddressableLevelRoutine(
//         int levelIndex)
//     {
//         if (isLoadingLevel ||
//             levelDatabase == null)
//         {
//             yield break;
//         }

//         string address =
//             levelDatabase.GetAddress(levelIndex);

//         if (string.IsNullOrWhiteSpace(address))
//         {
//             Debug.LogError(
//                 $"Level index {levelIndex} ka Addressable address missing hai.",
//                 this
//             );
//             yield break;
//         }

//         isLoadingLevel = true;
//         RefreshContinueButton();

//         AsyncOperationHandle<GridLevelData> newHandle =
//             Addressables.LoadAssetAsync<GridLevelData>(address);

//         pendingLevelHandle = newHandle;
//         hasPendingLevelHandle = true;

//         yield return newHandle;

//         if (newHandle.Status != AsyncOperationStatus.Succeeded ||
//             newHandle.Result == null)
//         {
//             Debug.LogError(
//                 $"Addressable level load failed: {address}",
//                 this
//             );

//             if (newHandle.IsValid())
//             {
//                 Addressables.Release(newHandle);
//             }

//             hasPendingLevelHandle = false;
//             isLoadingLevel = false;
//             RefreshContinueButton();
//             yield break;
//         }

//         /*
//          * Pehle purane instantiated objects destroy hote hain. Ek frame
//          * baad purana Addressables handle release hota hai, isliye bundle
//          * unload ke waqt koi live prefab/material instance nahi rehta.
//          */
//         PrepareForLevelAssetRelease();
//         yield return null;
//         ReleaseActiveLevelHandle();

//         activeLevelHandle = newHandle;
//         hasActiveLevelHandle = true;
//         hasPendingLevelHandle = false;
//         currentLevelIndex = levelIndex;
//         levelData = newHandle.Result;
//         isLoadingLevel = false;

//         SaveCurrentLevelProgress(
//             levelData.LevelNumber
//         );

//         GenerateLevel();
//     }


//     public void LoadLevelByNumber(int levelNumber)
//     {
//         if (levelDatabase == null)
//         {
//             Debug.LogError(
//                 "LoadLevelByNumber: GridLevelDatabase missing hai.",
//                 this
//             );
//             return;
//         }

//         if (levelNumber >
//             levelDatabase.MaximumPlayableLevelNumber)
//         {
//             Debug.LogWarning(
//                 $"Level {levelNumber} playable range se bahar hai. " +
//                 $"Last playable level " +
//                 $"{levelDatabase.MaximumPlayableLevelNumber} hai.",
//                 this
//             );
//             return;
//         }

//         int levelIndex =
//             levelDatabase.FindIndexByLevelNumber(levelNumber);

//         if (levelIndex < 0)
//         {
//             Debug.LogWarning(
//                 $"Addressable Level {levelNumber} catalog mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(levelIndex)
//         );
//     }


//     private void SelectStartingLevel()
//     {
//         if (levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             currentLevelIndex = 0;
//             return;
//         }

//         if (PlayerPrefs.HasKey(SavedLevelNumberKey))
//         {
//             int savedLevelNumber =
//                 PlayerPrefs.GetInt(
//                     SavedLevelNumberKey,
//                     1
//                 );

//             int savedLevelIndex =
//                 levelDatabase.FindIndexByLevelNumber(
//                     savedLevelNumber
//                 );

//             if (savedLevelIndex >= 0 &&
//                 FindNextLevelIndex(savedLevelIndex) ==
//                 savedLevelIndex)
//             {
//                 currentLevelIndex = savedLevelIndex;
//                 return;
//             }
//         }

//         if (startFromLevelOne)
//         {
//             int firstLevelIndex =
//                 FindNextLevelIndex(0);

//             if (firstLevelIndex >= 0)
//             {
//                 currentLevelIndex = firstLevelIndex;
//             }

//             return;
//         }

//         int configuredIndex =
//             levelData != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     levelData.LevelNumber
//                 )
//                 : -1;

//         currentLevelIndex =
//             configuredIndex >= 0
//                 ? configuredIndex
//                 : Mathf.Max(0, FindNextLevelIndex(0));

//     }


//     private static bool IsLiveWorkingLevelPreviewEnabled()
//     {
// #if UNITY_EDITOR
//         return UnityEditor.SessionState.GetBool(
//             "RoyalSmash.LiveWorkingLevelPreview",
//             false
//         );
// #else
//         return false;
// #endif
//     }


//     private static bool ShouldStartLiveWorkingLevelPreview()
//     {
// #if UNITY_EDITOR
//         const string previewSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreview";

//         const string previewLaunchSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreviewLaunch";

//         bool previewEnabled =
//             UnityEditor.SessionState.GetBool(
//                 previewSessionKey,
//                 false
//             );

//         bool launchRequested =
//             UnityEditor.SessionState.GetBool(
//                 previewLaunchSessionKey,
//                 false
//             );

//         UnityEditor.SessionState.SetBool(
//             previewLaunchSessionKey,
//             false
//         );

//         if (!launchRequested)
//         {
//             /*
//              * Toolbar se Play karne par pichli editor preview session
//              * normal game startup ko bypass nahi kar sakti.
//              */
//             UnityEditor.SessionState.SetBool(
//                 previewSessionKey,
//                 false
//             );
//         }

//         return previewEnabled && launchRequested;
// #else
//         return false;
// #endif
//     }


//     private int FindNextLevelIndex(
//         int startingIndex)
//     {
//         if (levelDatabase == null)
//         {
//             return -1;
//         }

//         for (int i = Mathf.Max(0, startingIndex);
//              i < levelDatabase.Count;
//              i++)
//         {
//             int levelNumber =
//                 levelDatabase.GetLevelNumber(i);

//             if (levelNumber >
//                 levelDatabase.MaximumPlayableLevelNumber)
//             {
//                 break;
//             }

//             if (levelNumber > 0 &&
//                 !string.IsNullOrWhiteSpace(
//                     levelDatabase.GetAddress(i)))
//             {
//                 return i;
//             }
//         }

//         return -1;
//     }


//     private void EnsureMainMenuUI()
//     {
//         if (mainMenuPanel == null)
//         {
//             RectTransform[] sceneRects =
//                 FindObjectsByType<RectTransform>(
//                     FindObjectsInactive.Include,
//                     FindObjectsSortMode.None
//                 );

//             foreach (RectTransform sceneRect in sceneRects)
//             {
//                 if (sceneRect != null &&
//                     sceneRect.name.Equals(
//                         "Main menu Panel",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ))
//                 {
//                     mainMenuPanel = sceneRect.gameObject;
//                     break;
//                 }
//             }
//         }

//         if (mainMenuPanel == null)
//         {
//             Debug.LogWarning(
//                 "Main menu Panel scene mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         Button[] menuButtons =
//             mainMenuPanel.GetComponentsInChildren<Button>(true);

//         foreach (Button menuButton in menuButtons)
//         {
//             if (menuButton == null)
//             {
//                 continue;
//             }

//             string buttonName =
//                 menuButton.name.ToLowerInvariant();

//             if (playButton == null &&
//                 buttonName.Contains("play"))
//             {
//                 playButton = menuButton;
//             }
//             else if (resetProgressButton == null &&
//                      buttonName.Contains("reset"))
//             {
//                 resetProgressButton = menuButton;
//             }
//         }

//         if (currentLevelText == null)
//         {
//             TMP_Text[] menuTexts =
//                 mainMenuPanel.GetComponentsInChildren<TMP_Text>(true);

//             foreach (TMP_Text menuText in menuTexts)
//             {
//                 if (menuText != null &&
//                     menuText.text.IndexOf(
//                         "level number",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ) >= 0)
//                 {
//                     currentLevelText = menuText;
//                     break;
//                 }
//             }
//         }

//         if (playButton != null)
//         {
//             playButton.onClick.RemoveListener(
//                 PlayFromMainMenu
//             );
//             playButton.onClick.AddListener(
//                 PlayFromMainMenu
//             );
//         }

//         if (resetProgressButton != null)
//         {
//             resetProgressButton.onClick.RemoveListener(
//                 ResetProgressFromMainMenu
//             );
//             resetProgressButton.onClick.AddListener(
//                 ResetProgressFromMainMenu
//             );
//         }
//     }


//     private void SetGameplayWorldVisible(bool visible)
//     {
//         if (gameplayPanel != null)
//         {
//             gameplayPanel.SetActive(visible);
//         }

//         if (levelOrigin != null &&
//             levelOrigin != transform)
//         {
//             levelOrigin.gameObject.SetActive(visible);
//         }

//         if (runtimeObjectsRoot != null)
//         {
//             runtimeObjectsRoot.gameObject.SetActive(visible);
//         }

//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (cannonController != null)
//         {
//             cannonController.SetGameplayActive(visible);
//         }

//         if (gameplayRestartButton == null)
//         {
//             GameRestartController restartController =
//                 FindFirstObjectByType<GameRestartController>(
//                     FindObjectsInactive.Include
//                 );

//             if (restartController != null)
//             {
//                 gameplayRestartButton =
//                     restartController.gameObject;
//             }
//         }

//         if (gameplayRestartButton != null)
//         {
//             gameplayRestartButton.SetActive(visible);
//         }

//         if (!visible)
//         {
//             foreach (LevelTable table in cachedTables)
//             {
//                 if (table != null)
//                 {
//                     table.gameObject.SetActive(false);
//                 }
//             }

//             SetLevelCompleteUIVisible(false);
//         }
//     }


//     private void SaveCurrentLevelProgress(int levelNumber)
//     {
//         if (levelNumber <= 0 ||
//             IsLiveWorkingLevelPreviewEnabled())
//         {
//             return;
//         }

//         PlayerPrefs.SetInt(
//             SavedLevelNumberKey,
//             levelNumber
//         );
//         PlayerPrefs.Save();
//         UpdateCurrentLevelText();
//     }


//     private void UpdateCurrentLevelText()
//     {
//         if (currentLevelText == null ||
//             levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             return;
//         }

//         int safeIndex =
//             Mathf.Clamp(
//                 currentLevelIndex,
//                 0,
//                 levelDatabase.Count - 1
//             );

//         int levelNumber =
//             levelDatabase.GetLevelNumber(safeIndex);

//         currentLevelText.text =
//             $"{Mathf.Max(1, levelNumber)}";
//     }


//     private void EnsureNextLevelUI()
//     {
//         if (continueButton != null)
//         {
//             continueButton.onClick.RemoveListener(
//                 ContinueToNextLevel
//             );

//             continueButton.onClick.AddListener(
//                 ContinueToNextLevel
//             );
//         }

//         if ((levelCompletePanel != null &&
//              continueButton != null) ||
//             !autoCreateNextLevelUI)
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
//                 candidate.renderMode != RenderMode.WorldSpace)
//             {
//                 targetCanvas = candidate;
//                 break;
//             }
//         }

//         if (targetCanvas == null)
//         {
//             GameObject canvasObject =
//                 new GameObject(
//                     "Level Progress Canvas",
//                     typeof(RectTransform),
//                     typeof(Canvas),
//                     typeof(CanvasScaler),
//                     typeof(GraphicRaycaster)
//                 );

//             int uiLayer = LayerMask.NameToLayer("UI");

//             if (uiLayer >= 0)
//             {
//                 canvasObject.layer = uiLayer;
//             }

//             targetCanvas = canvasObject.GetComponent<Canvas>();
//             targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             targetCanvas.sortingOrder = 110;

//             CanvasScaler scaler =
//                 canvasObject.GetComponent<CanvasScaler>();

//             scaler.uiScaleMode =
//                 CanvasScaler.ScaleMode.ScaleWithScreenSize;

//             scaler.referenceResolution =
//                 new Vector2(1080f, 1920f);
//         }

//         int panelLayer = targetCanvas.gameObject.layer;

//         autoCreatedCompletePanel =
//             new GameObject(
//                 "Level Complete Panel",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(Image)
//             );

//         autoCreatedCompletePanel.layer = panelLayer;
//         levelCompletePanel = autoCreatedCompletePanel;

//         RectTransform panelRect =
//             autoCreatedCompletePanel.GetComponent<RectTransform>();

//         panelRect.SetParent(targetCanvas.transform, false);
//         panelRect.anchorMin = new Vector2(0.5f, 0.5f);
//         panelRect.anchorMax = new Vector2(0.5f, 0.5f);
//         panelRect.pivot = new Vector2(0.5f, 0.5f);
//         panelRect.anchoredPosition = Vector2.zero;
//         panelRect.sizeDelta = new Vector2(620f, 180f);

//         Image panelImage =
//             autoCreatedCompletePanel.GetComponent<Image>();

//         panelImage.color = new Color(0.03f, 0.04f, 0.07f, 0.92f);

//         GameObject titleObject =
//             new GameObject(
//                 "Level Complete Text",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         titleObject.layer = panelLayer;

//         RectTransform titleRect =
//             titleObject.GetComponent<RectTransform>();

//         titleRect.SetParent(panelRect, false);
//         titleRect.anchorMin = new Vector2(0f, 1f);
//         titleRect.anchorMax = new Vector2(1f, 1f);
//         titleRect.pivot = new Vector2(0.5f, 1f);
//         titleRect.anchoredPosition = new Vector2(0f, -38f);
//         titleRect.sizeDelta = new Vector2(-40f, 100f);

//         TextMeshProUGUI title =
//             titleObject.GetComponent<TextMeshProUGUI>();

//         title.fontSize = 48f;
//         title.fontStyle = FontStyles.Bold;
//         title.alignment = TextAlignmentOptions.Center;
//         title.color = Color.white;
//         title.raycastTarget = false;

//         levelCompleteText = title;

//         GameObject buttonObject =
//             new GameObject(
//                 "Next Level Button",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(Image),
//                 typeof(Button)
//             );

//         buttonObject.layer = panelLayer;

//         RectTransform buttonRect =
//             buttonObject.GetComponent<RectTransform>();

//         buttonRect.SetParent(panelRect, false);
//         buttonRect.anchorMin = new Vector2(0.5f, 0f);
//         buttonRect.anchorMax = new Vector2(0.5f, 0f);
//         buttonRect.pivot = new Vector2(0.5f, 0f);
//         buttonRect.anchoredPosition = new Vector2(0f, 18f);
//         buttonRect.sizeDelta = new Vector2(390f, 88f);

//         Image buttonImage = buttonObject.GetComponent<Image>();
//         buttonImage.color = new Color(0.15f, 0.62f, 1f, 1f);

//         continueButton = buttonObject.GetComponent<Button>();
//         continueButton.targetGraphic = buttonImage;
//         continueButton.onClick.AddListener(ContinueToNextLevel);

//         GameObject labelObject =
//             new GameObject(
//                 "Label",
//                 typeof(RectTransform),
//                 typeof(CanvasRenderer),
//                 typeof(TextMeshProUGUI)
//             );

//         labelObject.layer = panelLayer;

//         RectTransform labelRect =
//             labelObject.GetComponent<RectTransform>();

//         labelRect.SetParent(buttonRect, false);
//         labelRect.anchorMin = Vector2.zero;
//         labelRect.anchorMax = Vector2.one;
//         labelRect.offsetMin = Vector2.zero;
//         labelRect.offsetMax = Vector2.zero;

//         TextMeshProUGUI label =
//             labelObject.GetComponent<TextMeshProUGUI>();

//         label.text = "CONTINUE";
//         label.fontSize = 38f;
//         label.fontStyle = FontStyles.Bold;
//         label.alignment = TextAlignmentOptions.Center;
//         label.color = Color.white;
//         label.raycastTarget = false;

//         RefreshContinueButton();
//     }


//     private void ShowLevelCompleteUI()
//     {
//         EnsureNextLevelUI();

//         bool hasNext = HasNextLevel;

//         if (levelCompleteText != null)
//         {
//             levelCompleteText.text =
//                 hasNext
//                     ? $"LEVEL {levelData.LevelNumber} COMPLETE"
//                     : "ALL LEVELS COMPLETE";
//         }

//         RefreshContinueButton();
//         SetLevelCompleteUIVisible(true);
//     }


//     private void SetLevelCompleteUIVisible(
//         bool visible)
//     {
//         if (levelCompletePanel != null)
//         {
//             levelCompletePanel.SetActive(visible);
//         }
//         else if (autoCreatedCompletePanel != null)
//         {
//             autoCreatedCompletePanel.SetActive(visible);
//         }
//         else if (levelCompleteText != null)
//         {
//             levelCompleteText.gameObject.SetActive(visible);
//         }

//         if (continueButton != null)
//         {
//             continueButton.gameObject.SetActive(
//                 visible && HasNextLevel
//             );
//         }
//     }


//     private void RefreshContinueButton()
//     {
//         if (continueButton == null)
//         {
//             return;
//         }

//         int nextIndex =
//             FindNextLevelIndex(
//                 currentLevelIndex + 1
//             );

//         bool hasNext = nextIndex >= 0;

//         continueButton.interactable =
//             hasNext && !isLoadingLevel;

//         TMP_Text buttonLabel =
//             continueButton.GetComponentInChildren<TMP_Text>(true);

//         if (buttonLabel == null)
//         {
//             return;
//         }

//         if (!hasNext)
//         {
//             buttonLabel.text = "COMPLETE";
//             return;
//         }

//         buttonLabel.text =
//             isLoadingLevel
//                 ? "LOADING..."
//                 : "CONTINUE";
//     }


//     private void PrepareForLevelAssetRelease()
//     {
//         ClearCurrentLevel();
//         DestroyActiveCannonBalls();

//         if (objectPool != null)
//         {
//             objectPool.ClearAll();
//         }

//         for (int index = cachedTables.Count - 1;
//              index >= 0;
//              index--)
//         {
//             DestroyCachedTableAt(index);
//         }

//         cachedTables.Clear();
//         cachedTablePrefabs.Clear();
//         currentTable = null;
//     }


//     private static void DestroyActiveCannonBalls()
//     {
//         CannonBallMarker[] cannonBalls =
//             FindObjectsByType<CannonBallMarker>(
//                 FindObjectsInactive.Include,
//                 FindObjectsSortMode.None
//             );

//         foreach (CannonBallMarker cannonBall in cannonBalls)
//         {
//             if (cannonBall == null)
//             {
//                 continue;
//             }

//             if (Application.isPlaying)
//             {
//                 Destroy(cannonBall.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(cannonBall.gameObject);
//             }
//         }
//     }


//     private void ReleaseActiveLevelHandle()
//     {
//         if (!hasActiveLevelHandle)
//         {
//             return;
//         }

//         if (activeLevelHandle.IsValid())
//         {
//             Addressables.Release(activeLevelHandle);
//         }

//         hasActiveLevelHandle = false;
//     }


//     public void ClearCurrentLevel()
//     {
//         levelGenerationRevision++;

//         levelGenerated =
//             false;


//         for (int i =
//                  activeObjects.Count - 1;
//              i >= 0;
//              i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance == null)
//             {
//                 continue;
//             }


//             instance.Cleared -=
//                 HandleObjectCleared;

//             instance.Activated -=
//                 HandleObjectActivated;


//             if (objectPool != null)
//             {
//                 objectPool.Release(
//                     instance
//                 );
//             }
//         }


//         activeObjects.Clear();

//         occupancyByCoordinate.Clear();

//         trackingByInstance.Clear();

//         gridBaseRowByTable.Clear();

//         /*
//          * Fit cache CellSize-specific hai — agar LoadLevel() se
//          * different GridLevelData aa jaye (alag CellSize) to purani
//          * cached scale ghalat hogi.
//          */
//         fitCache.Clear();


//         remainingTargets =
//             0;


//         foreach (LevelTable table in cachedTables)
//         {
//             if (table != null)
//             {
//                 table.gameObject.SetActive(false);
//             }
//         }


//         currentTable =
//             null;
//     }


//     private Transform GetLevelOrigin()
//     {
//         return
//             levelOrigin != null
//                 ? levelOrigin
//                 : transform;
//     }


//     private bool ValidateReferences()
//     {
//         if (levelData == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (!levelData.HasAnyValidTableGrid)
//         {
//             Debug.LogError(
//                 "Manual GridLevelData mein koi painted cell nahi hai. " +
//                 "Level Creator mein grid allocate karke shapes paint karein.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.BlockPalette == null ||
//             levelData.BlockPalette.Count == 0)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein Block Palette empty hai.",
//                 levelData
//             );

//             return false;
//         }

//         bool hasValidPaletteEntry =
//             false;

//         foreach (PhysicsObjectDefinition entry
//                  in levelData.BlockPalette)
//         {
//             if (entry != null &&
//                 entry.Prefab != null)
//             {
//                 hasValidPaletteEntry =
//                     true;

//                 break;
//             }
//         }

//         if (!hasValidPaletteEntry)
//         {
//             Debug.LogError(
//                 "Grid Level Data ki Block Palette mein koi valid " +
//                 "Prefab wali entry nahi hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.TableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (objectPool == null)
//         {
//             Debug.LogError(
//                 "Physics Object Pool missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (runtimeObjectsRoot == null)
//         {
//             Debug.LogError(
//                 "Runtime Objects Root missing hai.",
//                 this
//             );

//             return false;
//         }


//         Vector3 runtimeScale =
//             runtimeObjectsRoot.lossyScale;


//         if (Mathf.Abs(runtimeScale.x - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.y - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.z - 1f) > 0.01f)
//         {
//             Debug.LogWarning(
//                 "RuntimeObjects ki world Scale ideally 1,1,1 honi chahiye.",
//                 runtimeObjectsRoot
//             );
//         }


//         return true;
//     }


//     private void OnDestroy()
//     {
//         Time.timeScale = 1f;

//         for (int i = 0;
//              i < activeObjects.Count;
//              i++)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance != null)
//             {
//                 instance.Cleared -=
//                     HandleObjectCleared;

//                 instance.Activated -=
//                     HandleObjectActivated;
//             }
//         }

//         ReleaseActiveLevelHandle();

//         if (hasPendingLevelHandle &&
//             pendingLevelHandle.IsValid())
//         {
//             Addressables.Release(pendingLevelHandle);
//         }

//         hasPendingLevelHandle = false;
//     }
// }


// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.Events;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.UI;

// public sealed class LevelRuntimeController : MonoBehaviour
// {
//     private const string SavedLevelNumberKey =
//         "RoyalSmash.CurrentLevelNumber";

//     public event System.Action<GridLevelData> LevelGenerated;
//     public event System.Action<GridLevelData> LevelCompleted;






// // [Header("Level Progress")]
// // [SerializeField]
// // private LevelMilestoneProgressBar milestoneProgressBar;









//     [Header("Level")]

//     [SerializeField]
//     private GridLevelData levelData;

//     [Tooltip(
//         "Level Creator ke banaye tamam levels ki central database. " +
//         "Runtime mein individual levels manually assign nahi karne."
//     )]
//     [SerializeField]
//     private GridLevelDatabase levelDatabase;

//     [SerializeField]
//     private bool startFromLevelOne = true;


//     [Header("Main Menu")]

//     [SerializeField]
//     private bool showMainMenuOnStartup = true;

//     [SerializeField]
//     private GameObject mainMenuPanel;

//     [Tooltip(
//         "Gameplay UI panel. Main Menu par hidden rahega aur Play press karte hi " +
//         "gameplay world ke sath active ho jayega."
//     )]
//     [SerializeField]
//     private GameObject gameplayPanel;

//     [SerializeField]
//     private Button playButton;

//     [SerializeField]
//     private Button resetProgressButton;

//     [SerializeField]
//     private TMP_Text currentLevelText;


//     [Header("References")]

//     [SerializeField]
//     private PhysicsObjectPool objectPool;

//     [SerializeField]
//     private Transform levelOrigin;

//     [SerializeField]
//     private Transform runtimeObjectsRoot;


//     [Header("Support Chain")]

//     [Tooltip(
//         "Support khone ke kitne FixedUpdate frames baad upar wala " +
//         "block khud bhi dynamic ho jayega."
//     )]
//     [SerializeField, Range(1, 10)]
//     private int unsupportedFramesRequired = 2;

//     [Tooltip(
//         "Half-supported piece ko missing-support side par tip karne wala " +
//         "small sideways impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipImpulse = 0.18f;

//     [Tooltip(
//         "Exact edge-balance ko break karne wala natural rotation impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipTorque = 0.28f;


//     [Header("Mirrored Depth Physics")]

//     [Tooltip(
//         "Kisi bhi 2+ depth-layer level mein hit block ka impact same X/Y " +
//         "par maujood baqi depth-layer blocks ko bhi unlock aur push karega. " +
//         "Ye gameplay link Mirror Paint authoring option se independent hai."
//     )]
//     [SerializeField]
//     private bool linkMirroredDepthLayerPhysics = true;

//     [Tooltip(
//         "Front block ke impulse ka kitna hissa mirrored layers ko mile."
//     )]
//     [SerializeField, Range(0f, 1.5f)]
//     private float mirroredLayerImpulseMultiplier = 0.45f;

//     [Tooltip(
//         "Mirrored depth layers exact same frame par na giren. Har linked " +
//         "block ko is range mein chhota natural delay milta hai."
//     )]
//     [SerializeField]
//     private Vector2 mirroredActivationDelayRange =
//         new Vector2(0.09f, 0.22f);

//     [Tooltip(
//         "Mirrored impulse ki strength mein per-block random variation."
//     )]
//     [SerializeField]
//     private Vector2 mirroredImpulseVariation =
//         new Vector2(0.78f, 0.96f);

//     [Tooltip(
//         "Mirrored force direction mein maximum random angle variation."
//     )]
//     [SerializeField, Range(0f, 25f)]
//     private float mirroredDirectionVariationDegrees = 4f;

//     [Tooltip(
//         "Linked blocks ko alag natural spin dene ke liye random torque."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float mirroredRandomTorqueMultiplier = 0.035f;


//     [Header("Events")]

//     [SerializeField]
//     private UnityEvent onLevelGenerated;

//     [SerializeField]
//     private UnityEvent onLevelComplete;


//     private sealed class DefinitionFitData
//     {
//         public Vector3 Scale;
//         public Vector3 BoundsCenterOffset;
//     }


//     private sealed class GridTrackingEntry
//     {
//         public Vector3 InitialPosition;
//         public int UnsupportedFrames;
//         public int TableIndex;
//         public Quaternion SurfaceRotation;
//     }


//     private readonly List<PhysicsTowerObject>
//         activeObjects =
//             new List<PhysicsTowerObject>();

//     /// <summary>
//     /// Keyed by (definition, span) since a bigger footprint (e.g. a
//     /// 2-cell-tall piece) needs its own fit scale, distinct from the
//     /// same definition used at 1x1x1.
//     /// </summary>
//     private readonly Dictionary<(PhysicsObjectDefinition, int spanKey), DefinitionFitData>
//         fitCache =
//             new Dictionary<(PhysicsObjectDefinition, int), DefinitionFitData>();

//     /// <summary>
//     /// What currently occupies each grid cell — used by the support
//     /// chain to answer "is the cell below me still standing?".
//     /// </summary>
//     private readonly Dictionary<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject>
//         occupancyByCoordinate =
//             new Dictionary<(int, Vector3Int), PhysicsTowerObject>();

//     private readonly Dictionary<PhysicsTowerObject, GridTrackingEntry>
//         trackingByInstance =
//             new Dictionary<PhysicsTowerObject, GridTrackingEntry>();


//     private readonly List<LevelTable> cachedTables =
//         new List<LevelTable>();

//     private readonly List<LevelTable> cachedTablePrefabs =
//         new List<LevelTable>();

//     private readonly List<int> tablePhysicsReleaseCycleTargets =
//         new List<int>();

//     private readonly HashSet<int> releasedTablePhysicsIndices =
//         new HashSet<int>();

//     private LevelTable currentTable;


//     private int remainingTargets;

//     private bool levelGenerated;

//     private bool isPropagatingMirroredActivation;

//     private int levelGenerationRevision;

//     private float runtimeTableAnimationTime;

//     private int currentLevelIndex;

//     private AsyncOperationHandle<GridLevelData>
//         activeLevelHandle;

//     private bool hasActiveLevelHandle;

//     private AsyncOperationHandle<GridLevelData>
//         pendingLevelHandle;

//     private bool hasPendingLevelHandle;

//     private bool isLoadingLevel;

//     private CannonController cannonController;

//     private GameObject gameplayRestartButton;

//     /// <summary>
//     /// Lowest occupied grid row for the current level — the "floor"
//     /// row that always counts as supported by the table, even if the
//     /// designer didn't start painting at y=0.
//     /// </summary>
//     private readonly Dictionary<int, int> gridBaseRowByTable =
//         new Dictionary<int, int>();


//     public GridLevelData CurrentLevelData =>
//         levelData;

//     public LevelTable CurrentTable =>
//         currentTable;

//     public IReadOnlyList<LevelTable> CurrentTables =>
//         cachedTables;

//     public int RemainingTargets =>
//         remainingTargets;

//     public bool IsLevelGenerated =>
//         levelGenerated;

//     public int CurrentLevelIndex =>
//         currentLevelIndex;

//     public bool HasNextLevel =>
//         FindNextLevelIndex(currentLevelIndex + 1) >= 0;


//     private void Start()
//     {
//         if (ShouldStartLiveWorkingLevelPreview())
//         {
//             currentLevelIndex = 0;
//             GenerateLevel();
//             return;
//         }

//         SelectStartingLevel();

//         if (FindNextLevelIndex(currentLevelIndex) < 0)
//         {
//             Debug.LogError(
//                 "Addressable level catalog empty hai. Level Creator se " +
//                 "kam az kam ek level SAVE karein.",
//                 this
//             );
//             return;
//         }

//         if (showMainMenuOnStartup)
//         {
//             ShowMainMenu();
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ShowMainMenu()
//     {
//         EnsureMainMenuUI();
//         SetGameplayWorldVisible(false);
//         UpdateCurrentLevelText();

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(true);
//         }
//     }


//     public void PlayFromMainMenu()
//     {
//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int playableIndex =
//             FindNextLevelIndex(currentLevelIndex);

//         if (playableIndex < 0)
//         {
//             Debug.LogError(
//                 "Play: koi saved Addressable level configured nahi hai.",
//                 this
//             );
//             return;
//         }

//         currentLevelIndex = playableIndex;

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(false);
//         }

//         SetGameplayWorldVisible(true);

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ResetProgressFromMainMenu()
//     {
//         PlayerPrefs.DeleteKey(SavedLevelNumberKey);
//         PlayerPrefs.Save();

//         int firstLevelIndex =
//             FindNextLevelIndex(0);

//         currentLevelIndex =
//             firstLevelIndex >= 0
//                 ? firstLevelIndex
//                 : 0;

//         UpdateCurrentLevelText();
//     }


//     public void LoadLevel(
//         GridLevelData newLevelData)
//     {
//         if (newLevelData == null)
//         {
//             Debug.LogError(
//                 "LoadLevel: GridLevelData null hai.",
//                 this
//             );

//             return;
//         }


//         if (hasActiveLevelHandle)
//         {
//             PrepareForLevelAssetRelease();
//             ReleaseActiveLevelHandle();
//         }

//         levelData = newLevelData;

//         int sequenceIndex =
//             levelDatabase != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     newLevelData.LevelNumber
//                 )
//                 : -1;

//         if (sequenceIndex >= 0)
//         {
//             currentLevelIndex = sequenceIndex;
//         }

//         SaveCurrentLevelProgress(
//             newLevelData.LevelNumber
//         );


//         GenerateLevel();
//     }


//     [ContextMenu("Generate Level")]
//     public void GenerateLevel()
//     {
//         if (!ValidateReferences())
//         {
//             return;
//         }


//         ClearCurrentLevel();

//         runtimeTableAnimationTime = 0f;


//         objectPool.PrepareForLevel(
//             levelData
//         );


//         if (!PrepareTable())
//         {
//             return;
//         }

//         PrepareTablePhysicsReleaseTargets();


//         remainingTargets =
//             0;


//         SpawnGrid();


//         Physics.SyncTransforms();


//         levelGenerated =
//             true;


//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} GENERATED\n" +
//             $"Boxes: {activeObjects.Count}\n" +
//             $"Targets: {remainingTargets}",
//             this
//         );


//         LevelGenerated?.Invoke(levelData);

//         onLevelGenerated?.Invoke();


//         if (remainingTargets <= 0)
//         {
//             Debug.LogWarning(
//                 "Generated level mein koi target nahi mila.",
//                 this
//             );
//         }
//     }


//     /// <summary>
//     /// Measures a definition's prefab once (identity rotation, prefab's
//     /// authored scale) and derives the localScale + pivot-to-collider
//     /// offset needed so its collider bounds exactly fill levelData's
//     /// CellSize. Cached per-definition so mixed shapes (cube, cylinder,
//     /// ...) all land cleanly on the same uniform grid without measuring
//     /// on every single cell spawn.
//     /// </summary>
//     private bool TryGetFitData(
//         PhysicsObjectDefinition definition,
//         int spanX,
//         int spanY,
//         int spanZ,
//         PieceOrientation orientation,
//         out DefinitionFitData fitData)
//     {
//         int spanKey =
//             spanX * 10000 +
//             spanY * 100 +
//             spanZ +
//             (int)orientation * 1000000;

//         (PhysicsObjectDefinition, int) cacheKey =
//             (definition, spanKey);

//         if (fitCache.TryGetValue(
//                 cacheKey,
//                 out fitData))
//         {
//             return true;
//         }

//         fitData = null;

//         PhysicsTowerObject measurementObject =
//             objectPool.Get(
//                 definition,
//                 Vector3.zero,
//                 Quaternion.identity,
//                 runtimeObjectsRoot
//             );

//         if (measurementObject == null)
//         {
//             Debug.LogError(
//                 "Grid object measure karne ke liye pool object nahi mila.",
//                 this
//             );

//             return false;
//         }

//         Physics.SyncTransforms();

//         if (!measurementObject.TryGetPhysicsBounds(
//                 out Bounds bounds))
//         {
//             Debug.LogError(
//                 "Physics object par valid non-trigger Collider nahi mila.",
//                 measurementObject
//             );

//             objectPool.Release(measurementObject);

//             return false;
//         }

//         Vector3 measuredSize =
//             bounds.size;

//         Vector3 measuredCenterOffset =
//             bounds.center -
//             measurementObject.transform.position;

//         objectPool.Release(measurementObject);

//         if (measuredSize.x <= 0.0001f ||
//             measuredSize.y <= 0.0001f ||
//             measuredSize.z <= 0.0001f)
//         {
//             Debug.LogError(
//                 $"{definition.DisplayName}: collider size invalid hai.",
//                 this
//             );

//             return false;
//         }

//         Vector3 authoredScale =
//             definition.Prefab.transform.localScale;

//         Vector3 finalScale;

//         Vector3 finalOffset;

//         if (definition.AutoFitToCell)
//         {
//             /*
//              * Span > 1 par target size sirf spanCount * cellSize nahi —
//              * internal gaps bhi fill hote hain (spanX*cellSize.x +
//              * (spanX-1)*gap) taake merged piece apne poore footprint
//              * ke outer edges tak seamless dikhe, jaisa cells alag alag
//              * (gap ke sath) hote to unka combined outer span hota.
//              */
//             Vector3 gridAlignedTargetSize =
//                 new Vector3(
//                     spanX * levelData.CellSize.x +
//                     (spanX - 1) * levelData.HorizontalGap,

//                     spanY * levelData.CellSize.y +
//                     (spanY - 1) * levelData.VerticalGap,

//                     spanZ * levelData.CellSize.z +
//                     (spanZ - 1) * levelData.DepthGap
//                 );

//             /*
//              * gridAlignedTargetSize world/grid axes ke liye hai.
//              * Lekin fit-scale prefab ke apne UNROTATED local axes par
//              * compute hoti hai — agar orientation tip kar rahi hai
//              * (Lying X/Z) to local axes rotate hone ke baad world axes
//              * se match nahi karte, is liye target size ko yahan
//              * local axes par re-map karte hain (spawn time par
//              * PieceOrientationUtility.GetRotation() isi mapping ko
//              * wapas world space mein rotate kar degi).
//              */
//             Vector3 targetSize =
//                 PieceOrientationUtility.GetLocalAxisTargetSize(
//                     gridAlignedTargetSize,
//                     orientation
//                 );

//             /*
//              * boundsAtScale1 = measuredSize / authoredScale
//              * finalScale     = targetSize / boundsAtScale1
//              *                = targetSize * authoredScale / measuredSize
//              *
//              * Isi tarah offset bhi authored scale se scale-1 basis par
//              * normalize karke, phir finalScale par re-apply hota hai —
//              * taake off-center collider pivot bhi correct rahe.
//              */
//             finalScale =
//                 new Vector3(
//                     targetSize.x * authoredScale.x / measuredSize.x,
//                     targetSize.y * authoredScale.y / measuredSize.y,
//                     targetSize.z * authoredScale.z / measuredSize.z
//                 );

//             Vector3 offsetAtScale1 =
//                 new Vector3(
//                     measuredCenterOffset.x / authoredScale.x,
//                     measuredCenterOffset.y / authoredScale.y,
//                     measuredCenterOffset.z / authoredScale.z
//                 );

//             finalOffset =
//                 Vector3.Scale(
//                     offsetAtScale1,
//                     finalScale
//                 );
//         }
//         else
//         {
//             finalScale =
//                 authoredScale;

//             finalOffset =
//                 measuredCenterOffset;
//         }

//         finalScale =
//             Vector3.Scale(
//                 finalScale,
//                 definition.ManualScaleMultiplier
//             );

//         fitData =
//             new DefinitionFitData
//             {
//                 Scale = finalScale,
//                 BoundsCenterOffset = finalOffset
//             };

//         fitCache[cacheKey] =
//             fitData;

//         return true;
//     }


//     private void SpawnGrid()
//     {
//         Vector3Int occupiedMin =
//             new Vector3Int(
//                 int.MaxValue,
//                 int.MaxValue,
//                 int.MaxValue
//             );

//         Vector3Int occupiedMax =
//             new Vector3Int(
//                 int.MinValue,
//                 int.MinValue,
//                 int.MinValue
//             );

//         if (currentTable == null ||
//             !currentTable.TryGetTowerSurface(
//                 out Vector3 primarySurfacePosition,
//                 out Quaternion primarySurfaceRotation))
//         {
//             Debug.LogError(
//                 "PRIMARY table ki tower surface calculate nahi ho saki.",
//                 currentTable
//             );

//             return;
//         }

//         int tableCount =
//             Mathf.Max(1, cachedTables.Count);

//         Vector3[] surfacePositions =
//             new Vector3[tableCount];

//         Quaternion[] surfaceRotations =
//             new Quaternion[tableCount];

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             LevelTable table =
//                 tableIndex < cachedTables.Count
//                     ? cachedTables[tableIndex]
//                     : null;

//             if (table != null &&
//                 table.TryGetTowerSurface(
//                     out Vector3 tableSurfacePosition,
//                     out Quaternion tableSurfaceRotation))
//             {
//                 surfacePositions[tableIndex] = tableSurfacePosition;
//                 surfaceRotations[tableIndex] = tableSurfaceRotation;
//             }
//             else
//             {
//                 surfacePositions[tableIndex] = primarySurfacePosition;
//                 surfaceRotations[tableIndex] = primarySurfaceRotation;
//             }
//         }

//         Dictionary<int, Vector3Int> occupiedMinByTable =
//             new Dictionary<int, Vector3Int>();

//         Dictionary<int, Vector3Int> occupiedMaxByTable =
//             new Dictionary<int, Vector3Int>();

//         bool foundAnyOccupiedCell = false;

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             if (!levelData.TryGetOccupiedBounds(
//                     tableIndex,
//                     out Vector3Int tableMinimum,
//                     out Vector3Int tableMaximum))
//             {
//                 continue;
//             }

//             foundAnyOccupiedCell = true;
//             occupiedMinByTable[tableIndex] = tableMinimum;
//             occupiedMaxByTable[tableIndex] = tableMaximum;
//             occupiedMin = Vector3Int.Min(occupiedMin, tableMinimum);
//             occupiedMax = Vector3Int.Max(occupiedMax, tableMaximum);
//         }

//         if (!foundAnyOccupiedCell)
//         {
//             Debug.LogError(
//                 "Kisi bhi table ke manual grid mein occupied cells nahi hain.",
//                 levelData
//             );

//             return;
//         }

//         gridBaseRowByTable.Clear();

//         foreach (KeyValuePair<int, Vector3Int> entry
//                  in occupiedMinByTable)
//         {
//             gridBaseRowByTable[entry.Key] =
//                 entry.Value.y;
//         }


//         Vector3 cellSize =
//             levelData.CellSize;


//         float stepX =
//             cellSize.x +
//             levelData.HorizontalGap;


//         float stepY =
//             cellSize.y +
//             levelData.VerticalGap;


//         float stepZ =
//             cellSize.z +
//             levelData.DepthGap;


//         /*
//          * Important:
//          *
//          * Y outer loop hai.
//          * Isliye logical spawn order bottom → top rahega.
//          */
//         for (int y = occupiedMin.y;
//              y <= occupiedMax.y;
//              y++)
//         {
//             for (int z = occupiedMin.z;
//                  z <= occupiedMax.z;
//                  z++)
//             {
//                 for (int x = occupiedMin.x;
//                      x <= occupiedMax.x;
//                      x++)
//                 {
//                     for (int authoredTableIndex = 0;
//                          authoredTableIndex < tableCount;
//                          authoredTableIndex++)
//                     {
//                         if (!occupiedMinByTable.ContainsKey(
//                                 authoredTableIndex))
//                         {
//                             continue;
//                         }

//                         GridCellData cell =
//                             levelData.GetCell(
//                                 x,
//                                 y,
//                                 z,
//                                 authoredTableIndex
//                             );


//                     if (cell == null ||
//                         !cell.Occupied)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Covered cells (bare footprint ka hissa) khud
//                      * spawn nahi hoti — unka anchor cell hi spawn
//                      * karta hai (neeche is loop mein aage aake).
//                      */
//                     if (cell.IsCovered)
//                     {
//                         continue;
//                     }


//                     int blockTableIndex =
//                         authoredTableIndex;

//                     Vector3Int tableOccupiedMin =
//                         occupiedMinByTable[blockTableIndex];

//                     Vector3Int tableOccupiedMax =
//                         occupiedMaxByTable[blockTableIndex];

//                     float occupiedCenterX =
//                         (
//                             tableOccupiedMin.x +
//                             tableOccupiedMax.x
//                         ) *
//                         0.5f;

//                     float occupiedCenterZ =
//                         (
//                             tableOccupiedMin.z +
//                             tableOccupiedMax.z
//                         ) *
//                         0.5f;

//                     Vector3 surfacePosition =
//                         surfacePositions[blockTableIndex];

//                     Quaternion surfaceRotation =
//                         surfaceRotations[blockTableIndex];


//                     int spanX =
//                         Mathf.Max(1, cell.SpanX);

//                     int spanY =
//                         Mathf.Max(1, cell.SpanY);

//                     int spanZ =
//                         Mathf.Max(1, cell.SpanZ);


//                     /*
//                      * X:
//                      * Shape automatically center.
//                      *
//                      * Span > 1 ho to poore footprint (x..x+spanX-1)
//                      * ka center use hota hai, sirf anchor cell ka nahi.
//                      */
//                     float localX =
//                         (
//                             x -
//                             occupiedCenterX
//                         ) *
//                         stepX +
//                         (spanX - 1) *
//                         stepX *
//                         0.5f +
//                         cell.LocalOffset.x *
//                         stepX;


//                     /*
//                      * Y:
//                      * Lowest occupied manual-grid row
//                      * directly table top par start hogi.
//                      *
//                      * Transparent bottom margin
//                      * automatically ignore hota hai.
//                      */
//                     float localY =
//                         cellSize.y *
//                         0.5f +
//                         (
//                             y -
//                             tableOccupiedMin.y
//                         ) *
//                         stepY +
//                         (spanY - 1) *
//                         stepY *
//                         0.5f +
//                         cell.LocalOffset.y *
//                         stepY;


//                     /*
//                      * Z:
//                      * Complete depth centered.
//                      */
//                     float localZ =
//                         (
//                             z -
//                             occupiedCenterZ
//                         ) *
//                         stepZ +
//                         (spanZ - 1) *
//                         stepZ *
//                         0.5f +
//                         cell.LocalOffset.z *
//                         stepZ;


//                     Vector3 localCellCenter =
//                         new Vector3(
//                             localX,
//                             localY,
//                             localZ
//                         )
//                         +
//                         levelData.GridOffset;


//                     PhysicsObjectDefinition definition =
//                         levelData.GetPaletteEntry(
//                             cell.DefinitionIndex
//                         );

//                     if (definition == null ||
//                         definition.Prefab == null)
//                     {
//                         Debug.LogWarning(
//                             $"Cell ({x},{y},{z}): palette entry " +
//                             $"{cell.DefinitionIndex} invalid hai, skip.",
//                             levelData
//                         );

//                         continue;
//                     }

//                     PieceOrientation orientation =
//                         cell.Orientation;

//                     if (!TryGetFitData(
//                             definition,
//                             spanX,
//                             spanY,
//                             spanZ,
//                             orientation,
//                             out DefinitionFitData fitData))
//                     {
//                         continue;
//                     }


//                     /*
//                      * Orientation ka extra tip (Lying X/Z) surface
//                      * rotation ke UPAR apply hoti hai — piece ko table
//                      * ke sath sath khud bhi rotate karta hai.
//                      */
//                     Quaternion pieceRotation =
//                         surfaceRotation *
//                         PieceOrientationUtility.GetRotation(
//                             orientation,
//                             cell.CustomZRotation
//                         ) *
//                         Quaternion.Euler(
//                             cell.RotationEulerOffset
//                         );


//                     Vector3 pieceScale =
//                         Vector3.Scale(
//                             fitData.Scale,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 desiredBoundsCenter =
//                         surfacePosition +
//                         surfaceRotation *
//                         localCellCenter;


//                     /*
//                      * Prefab pivot center par na bhi ho
//                      * to collider bottom proper align hoga.
//                      */
//                     Vector3 rotatedBoundsOffset =
//                         pieceRotation *
//                         Vector3.Scale(
//                             fitData.BoundsCenterOffset,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 spawnPosition =
//                         desiredBoundsCenter -
//                         rotatedBoundsOffset;


//                     PhysicsTowerObject instance =
//                         objectPool.Get(
//                             definition,
//                             spawnPosition,
//                             pieceRotation,
//                             runtimeObjectsRoot
//                         );


//                     if (instance == null)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Pool.Get() prefab ki authored scale laga deta hai —
//                      * yahan usay cell-fit scale se override karte hain
//                      * taake mixed shapes (cube/cylinder) uniform grid
//                      * mein clean fit hon.
//                      */
//                     instance.transform.localScale =
//                         pieceScale;


//                     /*
//                      * Pool se purani property-block state clear karo.
//                      * Default visual hamesha prefab ka apna material aur
//                      * texture hai; paint tint definition par optional hai.
//                      */
//                     instance.RestorePrefabVisual();

//                     if (definition.TintWithPaintColor)
//                     {
//                         instance.SetVisualColor(
//                             cell.Color
//                         );
//                     }


//                     Vector3Int anchorCoordinate =
//                         new Vector3Int(x, y, z);

//                     instance.SetGridFootprint(
//                         anchorCoordinate,
//                         new Vector3Int(
//                             spanX,
//                             spanY,
//                             spanZ
//                         ),
//                         cell.LocalOffset
//                     );

//                     instance.ConfigureBreakable(
//                         cell.Breakable,
//                         cell.HitsToBreak
//                     );

//                     /*
//                      * Bare footprint ke SAARE cells is instance ko
//                      * point karte hain — is se agar upar kuch spawn ho
//                      * to woh footprint ke kisi bhi column par "supported"
//                      * sahi detect hoga.
//                      */
//                     for (int fz = 0; fz < spanZ; fz++)
//                     {
//                         for (int fy = 0; fy < spanY; fy++)
//                         {
//                             for (int fx = 0; fx < spanX; fx++)
//                             {
//                                 occupancyByCoordinate[
//                                     (
//                                         blockTableIndex,
//                                         new Vector3Int(
//                                             x + fx,
//                                             y + fy,
//                                             z + fz
//                                         )
//                                     )
//                                 ] = instance;
//                             }
//                         }
//                     }

//                     trackingByInstance[instance] =
//                         new GridTrackingEntry
//                         {
//                             InitialPosition = spawnPosition,
//                             UnsupportedFrames = 0,
//                             TableIndex = blockTableIndex,
//                             SurfaceRotation = surfaceRotation
//                         };


//                     instance.Cleared +=
//                         HandleObjectCleared;

//                     instance.Activated +=
//                         HandleObjectActivated;


//                     activeObjects.Add(
//                         instance
//                     );


//                     if (instance.CountsAsTarget)
//                     {
//                         remainingTargets++;
//                     }
//                     }
//                 }
//             }
//         }
//     }


//     private int ResolveBlockTableIndex(int requestedIndex)
//     {
//         if (requestedIndex >= 0 &&
//             requestedIndex < cachedTables.Count &&
//             cachedTables[requestedIndex] != null)
//         {
//             return requestedIndex;
//         }

//         return 0;
//     }


//     private void FixedUpdate()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }

//         AnimateEnabledTables();

//         if (activeObjects.Count == 0)
//         {
//             return;
//         }

//         /*
//          * Snapshot: activeObjects FixedUpdate ke dauran
//          * HandleObjectCleared se modify ho sakti hai
//          * (ActivatePhysics() khud kuch remove nahi karta,
//          * lekin safety ke liye copy le lete hain).
//          */
//         for (int i = activeObjects.Count - 1; i >= 0; i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];

//             if (instance == null || !instance.IsLocked)
//             {
//                 continue;
//             }

//             if (!trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry))
//             {
//                 continue;
//             }

//             if (IsSupported(
//                     instance,
//                     entry,
//                     out Vector3 unsupportedLocalDirection))
//             {
//                 entry.UnsupportedFrames = 0;
//                 continue;
//             }

//             entry.UnsupportedFrames++;

//             if (entry.UnsupportedFrames >=
//                 unsupportedFramesRequired)
//             {
//                 Vector3 worldFallDirection =
//                     entry.SurfaceRotation *
//                     unsupportedLocalDirection.normalized;

//                 Vector3 worldUp =
//                     entry.SurfaceRotation * Vector3.up;

//                 Vector3 tippingTorque =
//                     Vector3.Cross(
//                         worldUp,
//                         worldFallDirection
//                     ).normalized *
//                     unsupportedTipTorque *
//                     Random.Range(0.9f, 1.1f);

//                 instance.ActivatePhysics(
//                     worldFallDirection *
//                     unsupportedTipImpulse *
//                     Random.Range(0.9f, 1.1f),
//                     tippingTorque
//                 );
//             }
//         }
//     }


//     /// <summary>
//     /// Bottom row table par khadi hoti hai. Baqi pieces ke poore bottom
//     /// footprint mein kam az kam ek LOCKED support hona chahiye. Hit hua
//     /// dynamic block foran support count hona band ho jata hai, chahe woh
//     /// abhi apni spawn position se zyada move na bhi hua ho.
//     ///
//     /// Multi-cell footprint (span > 1) wale pieces ke liye sirf
//     /// "kahin bhi neeche support hai" (OR) kaafi nahi — agar ek extreme
//     /// edge (min ya max column) ka support hat jaye to piece ko us taraf
//     /// tip hona chahiye, chahe doosra extreme abhi bhi supported ho.
//     /// Ye seam-offset pieces (0.5 Center-on-Seam) aur plain grid-aligned
//     /// multi-span pieces, dono par apply hota hai.
//     /// </summary>
//     private bool IsSupported(
//         PhysicsTowerObject instance,
//         GridTrackingEntry trackingEntry,
//         out Vector3 unsupportedLocalDirection)
//     {
//         unsupportedLocalDirection = Vector3.zero;

//         if (instance == null)
//         {
//             unsupportedLocalDirection = Vector3.right;
//             return false;
//         }

//         Vector3Int coordinate =
//             instance.GridCoordinate;

//         int baseRow =
//             gridBaseRowByTable.TryGetValue(
//                 trackingEntry.TableIndex,
//                 out int tableBaseRow)
//                 ? tableBaseRow
//                 : coordinate.y;

//         if (coordinate.y <= baseRow)
//         {
//             return true;
//         }

//         Vector3Int span =
//             instance.GridSpan;

//         Vector3 localOffset =
//             instance.GridLocalOffset;

//         int minimumX =
//             coordinate.x +
//             Mathf.FloorToInt(localOffset.x);

//         int maximumX =
//             coordinate.x +
//             Mathf.Max(1, span.x) - 1 +
//             Mathf.CeilToInt(localOffset.x);

//         int minimumZ =
//             coordinate.z +
//             Mathf.FloorToInt(localOffset.z);

//         int maximumZ =
//             coordinate.z +
//             Mathf.Max(1, span.z) - 1 +
//             Mathf.CeilToInt(localOffset.z);

//         bool seamOnX =
//             !Mathf.Approximately(
//                 localOffset.x,
//                 Mathf.Round(localOffset.x)
//             );

//         bool seamOnZ =
//             !Mathf.Approximately(
//                 localOffset.z,
//                 Mathf.Round(localOffset.z)
//             );

//         /*
//          * Seam offset ke bina bhi agar piece ek se zyada grid column
//          * span kar raha hai (e.g. horizontal 2-cell beam), to sirf
//          * "kahin bhi support hai" kaafi nahi — dono extremes (min aur
//          * max column) ka support alag alag check hona chahiye, warna
//          * ek pillar hat jane par doosre pillar ka sahara poore beam
//          * ko galat tarah "supported" dikha deta hai.
//          */
//         bool spansMultipleColumnsX =
//             maximumX > minimumX;

//         bool spansMultipleColumnsZ =
//             maximumZ > minimumZ;

//         bool hasAnySupport = false;
//         bool minimumXSupported = false;
//         bool maximumXSupported = false;
//         bool minimumZSupported = false;
//         bool maximumZSupported = false;

//         for (int z = minimumZ; z <= maximumZ; z++)
//         {
//             for (int x = minimumX; x <= maximumX; x++)
//             {
//                 Vector3Int below =
//                     new Vector3Int(
//                         x,
//                         coordinate.y - 1,
//                         z
//                     );

//                 if (occupancyByCoordinate.TryGetValue(
//                         (trackingEntry.TableIndex, below),
//                         out PhysicsTowerObject supportInstance) &&
//                     supportInstance != null &&
//                     supportInstance != instance &&
//                     supportInstance.IsLocked)
//                 {
//                     hasAnySupport = true;
//                     minimumXSupported |= x == minimumX;
//                     maximumXSupported |= x == maximumX;
//                     minimumZSupported |= z == minimumZ;
//                     maximumZSupported |= z == maximumZ;
//                 }
//             }
//         }

//         if (!hasAnySupport)
//         {
//             unsupportedLocalDirection =
//                 GetNaturalUnsupportedDirection(
//                     coordinate,
//                     localOffset
//                 );

//             return false;
//         }

//         if ((seamOnX || spansMultipleColumnsX) &&
//             (!minimumXSupported || !maximumXSupported))
//         {
//             if (!minimumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.left;
//             }

//             if (!maximumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.right;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.x >= 0f
//                         ? Vector3.right
//                         : Vector3.left;
//             }

//             return false;
//         }

//         if ((seamOnZ || spansMultipleColumnsZ) &&
//             (!minimumZSupported || !maximumZSupported))
//         {
//             if (!minimumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.back;
//             }

//             if (!maximumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.forward;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.z >= 0f
//                         ? Vector3.forward
//                         : Vector3.back;
//             }

//             return false;
//         }

//         return true;
//     }


//     private static Vector3 GetNaturalUnsupportedDirection(
//         Vector3Int coordinate,
//         Vector3 localOffset)
//     {
//         Vector3 direction =
//             new Vector3(
//                 localOffset.x,
//                 0f,
//                 localOffset.z
//             );

//         if (direction.sqrMagnitude > 0.001f)
//         {
//             return direction.normalized;
//         }

//         int hash =
//             coordinate.x * 73856093 ^
//             coordinate.y * 19349663 ^
//             coordinate.z * 83492791;

//         float x =
//             (hash & 1) == 0 ? -1f : 1f;

//         float z =
//             (hash & 2) == 0 ? -0.45f : 0.45f;

//         return new Vector3(x, 0f, z).normalized;
//     }


//     private bool PrepareTable()
//     {
//         int requiredTableCount =
//             levelData.TableCount;

//         if (requiredTableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "GridLevelData mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }

//         while (cachedTables.Count > requiredTableCount)
//         {
//             DestroyCachedTableAt(
//                 cachedTables.Count - 1
//             );

//             cachedTables.RemoveAt(
//                 cachedTables.Count - 1
//             );

//             cachedTablePrefabs.RemoveAt(
//                 cachedTablePrefabs.Count - 1
//             );
//         }

//         Transform levelRoot =
//             GetLevelOrigin();

//         while (cachedTables.Count < requiredTableCount)
//         {
//             cachedTables.Add(null);
//             cachedTablePrefabs.Add(null);
//         }

//         for (int index = 0;
//              index < requiredTableCount;
//              index++)
//         {
//             LevelTable requiredPrefab =
//                 levelData.GetTablePrefab(index);

//             if (requiredPrefab == null)
//             {
//                 DestroyCachedTableAt(index);

//                 Debug.LogWarning(
//                     $"Table Placements element {index} ka Prefab missing hai; " +
//                     "is extra table ko skip kiya gaya.",
//                     levelData
//                 );

//                 continue;
//             }

//             if (cachedTables[index] == null ||
//                 cachedTablePrefabs[index] != requiredPrefab)
//             {
//                 DestroyCachedTableAt(index);

//                 LevelTable instance =
//                     Instantiate(
//                         requiredPrefab,
//                         levelRoot
//                     );

//                 instance.name =
//                     requiredPrefab.name +
//                     $"_Runtime_{index + 1}";

//                 cachedTables[index] = instance;
//                 cachedTablePrefabs[index] = requiredPrefab;
//             }

//             LevelTable table =
//                 cachedTables[index];

//             Transform tableTransform =
//                 table.transform;

//             if (tableTransform.parent != levelRoot)
//             {
//                 tableTransform.SetParent(
//                     levelRoot,
//                     false
//                 );
//             }

//             tableTransform.localPosition =
//                 levelData.GetTablePositionOffset(index);

//             tableTransform.localRotation =
//                 Quaternion.Euler(
//                     levelData.GetTableRotationEuler(index)
//                 );

//             tableTransform.localScale =
//                 Vector3.Scale(
//                     requiredPrefab.transform.localScale,
//                     levelData.GetTableSizeMultiplier(index)
//                 );

//             table.gameObject.SetActive(true);

//             if (table.TowerSurfaceCollider == null)
//             {
//                 Debug.LogError(
//                     $"Table Placements element {index} ke prefab par " +
//                     "Tower Surface Collider missing hai.",
//                     table
//                 );

//                 return false;
//             }
//         }

//         currentTable = cachedTables[0];

//         return true;
//     }


//     private void DestroyCachedTableAt(int index)
//     {
//         if (index < 0 ||
//             index >= cachedTables.Count)
//         {
//             return;
//         }

//         LevelTable table =
//             cachedTables[index];

//         if (table != null)
//         {
//             if (Application.isPlaying)
//             {
//                 Destroy(table.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(table.gameObject);
//             }
//         }

//         cachedTables[index] = null;

//         if (index < cachedTablePrefabs.Count)
//         {
//             cachedTablePrefabs[index] = null;
//         }
//     }


//     private void AnimateEnabledTables()
//     {
//         if (levelData == null ||
//             cachedTables.Count == 0)
//         {
//             return;
//         }

//         runtimeTableAnimationTime +=
//             Time.fixedDeltaTime;

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             LevelTable table = cachedTables[tableIndex];
//             bool rotationEnabled =
//                 levelData.IsTableRuntimeRotationEnabled(
//                     tableIndex
//                 );
//             bool movementEnabled =
//                 levelData.IsTableRuntimeHorizontalMovementEnabled(
//                     tableIndex
//                 );

//             if (table == null ||
//                 (!rotationEnabled && !movementEnabled))
//             {
//                 continue;
//             }

//             Vector3 rotationAxis =
//                 levelData.GetTableRuntimeRotationAxis(tableIndex);
//             float rotationSpeed =
//                 levelData.GetTableRuntimeRotationSpeed(tableIndex);
//             Vector3 movementAxis =
//                 levelData.GetTableRuntimeMovementAxis(tableIndex);
//             float movementDistance =
//                 levelData.GetTableRuntimeMovementDistance(tableIndex);
//             float movementSpeed =
//                 levelData.GetTableRuntimeMovementSpeed(tableIndex);

//             bool canRotate =
//                 rotationEnabled &&
//                 rotationAxis.sqrMagnitude >= 0.0001f &&
//                 !Mathf.Approximately(rotationSpeed, 0f);

//             bool canMove =
//                 movementEnabled &&
//                 movementAxis.sqrMagnitude >= 0.0001f &&
//                 movementDistance > 0f &&
//                 movementSpeed > 0f;

//             if (!canRotate && !canMove)
//             {
//                 continue;
//             }

//             Transform tableTransform = table.transform;
//             Vector3 oldWorldPosition = tableTransform.position;
//             Quaternion oldWorldRotation = tableTransform.rotation;

//             if (canMove)
//             {
//                 float movementPhase =
//                     runtimeTableAnimationTime *
//                     movementSpeed *
//                     Mathf.PI * 2f;

//                 tableTransform.localPosition =
//                     levelData.GetTablePositionOffset(tableIndex) +
//                     movementAxis.normalized *
//                     Mathf.Sin(movementPhase) *
//                     movementDistance;
//             }

//             if (canRotate)
//             {
//                 tableTransform.localRotation =
//                     tableTransform.localRotation *
//                     Quaternion.AngleAxis(
//                         rotationSpeed * Time.fixedDeltaTime,
//                         rotationAxis.normalized
//                     );
//             }

//             Vector3 newWorldPosition = tableTransform.position;
//             Vector3 tableVelocity =
//                 (newWorldPosition - oldWorldPosition) /
//                 Mathf.Max(0.0001f, Time.fixedDeltaTime);
//             Quaternion worldRotationDelta =
//                 tableTransform.rotation *
//                 Quaternion.Inverse(oldWorldRotation);

//             for (int objectIndex = 0;
//                  objectIndex < activeObjects.Count;
//                  objectIndex++)
//             {
//                 PhysicsTowerObject instance =
//                     activeObjects[objectIndex];

//                 if (instance == null ||
//                     !instance.IsLocked ||
//                     !trackingByInstance.TryGetValue(
//                         instance,
//                         out GridTrackingEntry entry) ||
//                     entry.TableIndex != tableIndex)
//                 {
//                     continue;
//                 }

//                 Rigidbody objectBody = instance.Body;
//                 Vector3 objectPosition =
//                     objectBody != null
//                         ? objectBody.position
//                         : instance.transform.position;
//                 Quaternion objectRotation =
//                     objectBody != null
//                         ? objectBody.rotation
//                         : instance.transform.rotation;
//                 Vector3 animatedPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (objectPosition - oldWorldPosition);

//                 instance.MoveLockedWithTable(
//                     animatedPosition,
//                     worldRotationDelta * objectRotation
//                 );

//                 entry.InitialPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (entry.InitialPosition - oldWorldPosition);

//                 entry.SurfaceRotation =
//                     worldRotationDelta * entry.SurfaceRotation;
//             }

//             TryReleaseTableBlocksAfterMovementCycles(
//                 tableIndex,
//                 canMove ? movementSpeed : 0f,
//                 tableVelocity
//             );
//         }
//     }


//     private void PrepareTablePhysicsReleaseTargets()
//     {
//         tablePhysicsReleaseCycleTargets.Clear();
//         releasedTablePhysicsIndices.Clear();

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             if (!levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                     tableIndex))
//             {
//                 tablePhysicsReleaseCycleTargets.Add(int.MaxValue);
//                 continue;
//             }

//             int minimumCycles =
//                 Mathf.Max(
//                     1,
//                     levelData.GetTableMinimumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );
//             int maximumCycles =
//                 Mathf.Max(
//                     minimumCycles,
//                     levelData.GetTableMaximumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );

//             tablePhysicsReleaseCycleTargets.Add(
//                 Random.Range(
//                     minimumCycles,
//                     maximumCycles + 1
//                 )
//             );
//         }
//     }


//     private void TryReleaseTableBlocksAfterMovementCycles(
//         int tableIndex,
//         float movementSpeed,
//         Vector3 tableVelocity)
//     {
//         if (tableIndex < 0 ||
//             tableIndex >= tablePhysicsReleaseCycleTargets.Count ||
//             releasedTablePhysicsIndices.Contains(tableIndex) ||
//             !levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                 tableIndex) ||
//             movementSpeed <= 0f)
//         {
//             return;
//         }

//         float completedCycles =
//             runtimeTableAnimationTime * movementSpeed;

//         if (completedCycles <
//             tablePhysicsReleaseCycleTargets[tableIndex])
//         {
//             return;
//         }

//         releasedTablePhysicsIndices.Add(tableIndex);

//         float velocityMultiplier =
//             levelData.GetTableMovementReleaseVelocityMultiplier(
//                 tableIndex
//             );

//         bool wasPropagatingMirroredActivation =
//             isPropagatingMirroredActivation;

//         isPropagatingMirroredActivation = true;

//         for (int objectIndex = activeObjects.Count - 1;
//              objectIndex >= 0;
//              objectIndex--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[objectIndex];

//             if (instance == null ||
//                 !instance.IsLocked ||
//                 !trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry) ||
//                 entry.TableIndex != tableIndex)
//             {
//                 continue;
//             }

//             Rigidbody body = instance.Body;
//             float bodyMass =
//                 body != null
//                     ? Mathf.Max(0.01f, body.mass)
//                     : 1f;

//             Vector3 naturalVariation =
//                 new Vector3(
//                     Random.Range(-0.08f, 0.08f),
//                     Random.Range(0.015f, 0.07f),
//                     Random.Range(-0.08f, 0.08f)
//                 );

//             Vector3 inheritedImpulse =
//                 (tableVelocity * velocityMultiplier +
//                  naturalVariation) *
//                 bodyMass;

//             Vector3 naturalTorque =
//                 Random.onUnitSphere *
//                 Random.Range(0.035f, 0.12f) *
//                 bodyMass;

//             instance.ActivatePhysics(
//                 inheritedImpulse,
//                 naturalTorque
//             );
//         }

//         isPropagatingMirroredActivation =
//             wasPropagatingMirroredActivation;
//     }


//     private void HandleObjectActivated(
//         PhysicsTowerObject source,
//         Vector3 sourceImpulse)
//     {
//         if (source == null ||
//             levelData == null ||
//             !linkMirroredDepthLayerPhysics ||
//             levelData.GridDepth <= 1 ||
//             isPropagatingMirroredActivation)
//         {
//             return;
//         }

//         Vector3Int sourceCoordinate =
//             source.GridCoordinate;

//         if (!trackingByInstance.TryGetValue(
//                 source,
//                 out GridTrackingEntry sourceTracking))
//         {
//             return;
//         }

//         int sourceTableIndex =
//             sourceTracking.TableIndex;

//         int activationRevision =
//             levelGenerationRevision;

//         for (int z = 0;
//              z < levelData.GridDepth;
//              z++)
//         {
//             if (z == sourceCoordinate.z ||
//                 !TryFindDepthLinkedObject(
//                     source,
//                     sourceCoordinate,
//                     sourceTableIndex,
//                     z,
//                     out PhysicsTowerObject mirroredObject,
//                     out Vector3Int mirroredCoordinate))
//             {
//                 continue;
//             }

//             StartCoroutine(
//                 ActivateMirroredObjectNaturally(
//                     mirroredObject,
//                     mirroredCoordinate,
//                     sourceTableIndex,
//                     sourceImpulse,
//                     activationRevision
//                 )
//             );
//         }
//     }


//     private bool TryFindDepthLinkedObject(
//         PhysicsTowerObject source,
//         Vector3Int sourceCoordinate,
//         int tableIndex,
//         int targetDepth,
//         out PhysicsTowerObject linkedObject,
//         out Vector3Int linkedCoordinate)
//     {
//         linkedObject = null;
//         linkedCoordinate = new Vector3Int(
//             sourceCoordinate.x,
//             sourceCoordinate.y,
//             targetDepth
//         );

//         if (TryGetLockedDepthObject(
//                 linkedCoordinate,
//                 tableIndex,
//                 source,
//                 out linkedObject))
//         {
//             return true;
//         }

//         /*
//          * Manually-authored layers can have different silhouettes. In that
//          * case there may be no block at the exact X/Y coordinate behind the
//          * hit. Find the nearest locked block in a small projected area so the
//          * back structure still receives a believable local impact rather
//          * than remaining completely static.
//          */
//         const int searchRadius = 2;
//         float bestScore = float.PositiveInfinity;

//         for (int yOffset = -searchRadius;
//              yOffset <= searchRadius;
//              yOffset++)
//         {
//             for (int xOffset = -searchRadius;
//                  xOffset <= searchRadius;
//                  xOffset++)
//             {
//                 if (xOffset == 0 && yOffset == 0)
//                 {
//                     continue;
//                 }

//                 Vector3Int candidateCoordinate =
//                     new Vector3Int(
//                         sourceCoordinate.x + xOffset,
//                         sourceCoordinate.y + yOffset,
//                         targetDepth
//                     );

//                 if (!TryGetLockedDepthObject(
//                         candidateCoordinate,
//                         tableIndex,
//                         source,
//                         out PhysicsTowerObject candidate))
//                 {
//                     continue;
//                 }

//                 float score =
//                     xOffset * xOffset +
//                     yOffset * yOffset * 1.5f;

//                 if (score >= bestScore)
//                 {
//                     continue;
//                 }

//                 bestScore = score;
//                 linkedObject = candidate;
//                 linkedCoordinate = candidateCoordinate;
//             }
//         }

//         return linkedObject != null;
//     }


//     private bool TryGetLockedDepthObject(
//         Vector3Int coordinate,
//         int tableIndex,
//         PhysicsTowerObject source,
//         out PhysicsTowerObject candidate)
//     {
//         return occupancyByCoordinate.TryGetValue(
//                    (tableIndex, coordinate),
//                    out candidate) &&
//                candidate != null &&
//                candidate != source &&
//                candidate.IsLocked;
//     }


//     private IEnumerator ActivateMirroredObjectNaturally(
//         PhysicsTowerObject mirroredObject,
//         Vector3Int mirroredCoordinate,
//         int tableIndex,
//         Vector3 sourceImpulse,
//         int activationRevision)
//     {
//         float minimumDelay =
//             Mathf.Max(0f, mirroredActivationDelayRange.x);

//         float maximumDelay =
//             Mathf.Max(minimumDelay, mirroredActivationDelayRange.y);

//         float delay =
//             Random.Range(minimumDelay, maximumDelay);

//         if (delay > 0f)
//         {
//             yield return new WaitForSeconds(delay);
//         }

//         if (activationRevision != levelGenerationRevision ||
//             mirroredObject == null ||
//             !mirroredObject.IsLocked ||
//             !occupancyByCoordinate.TryGetValue(
//                 (tableIndex, mirroredCoordinate),
//                 out PhysicsTowerObject currentObject) ||
//             currentObject != mirroredObject)
//         {
//             yield break;
//         }

//         float minimumStrength =
//             Mathf.Max(0f, mirroredImpulseVariation.x);

//         float maximumStrength =
//             Mathf.Max(minimumStrength, mirroredImpulseVariation.y);

//         float randomStrength =
//             Random.Range(minimumStrength, maximumStrength);

//         Vector3 randomAngles =
//             new Vector3(
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 )
//             );

//         Vector3 variedImpulse =
//             Quaternion.Euler(randomAngles) *
//             sourceImpulse *
//             mirroredLayerImpulseMultiplier *
//             randomStrength;

//         Vector3 randomTorque =
//             sourceImpulse.sqrMagnitude > 0.0001f
//                 ? Random.onUnitSphere *
//                   sourceImpulse.magnitude *
//                   mirroredRandomTorqueMultiplier *
//                   Random.Range(0.65f, 1.25f)
//                 : Vector3.zero;

//         isPropagatingMirroredActivation = true;

//         try
//         {
//             mirroredObject.ActivatePhysics(
//                 variedImpulse,
//                 randomTorque
//             );
//         }
//         finally
//         {
//             isPropagatingMirroredActivation = false;
//         }
//     }


//     private void HandleObjectCleared(
//         PhysicsTowerObject target)
//     {
//         if (target == null)
//         {
//             return;
//         }


//         target.Cleared -=
//             HandleObjectCleared;

//         target.Activated -=
//             HandleObjectActivated;


//         activeObjects.Remove(
//             target
//         );

//         trackingByInstance.Remove(
//             target
//         );

//         /*
//          * Multi-cell footprint wale piece ke liye occupancyByCoordinate
//          * mein uske SAARE covered cells target ko point karte hain
//          * (sirf anchor nahi) — is liye scan karke sab remove karte hain,
//          * warna stale entries agle level mein reused instance ko
//          * galat tarah "still standing" dikha sakti hain.
//          */
//         List<(int tableIndex, Vector3Int coordinate)> coordinatesToRemove = null;

//         foreach (KeyValuePair<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject> entry
//                  in occupancyByCoordinate)
//         {
//             if (entry.Value != target)
//             {
//                 continue;
//             }

//             if (coordinatesToRemove == null)
//             {
//                 coordinatesToRemove =
//                     new List<(int, Vector3Int)>();
//             }

//             coordinatesToRemove.Add(entry.Key);
//         }

//         if (coordinatesToRemove != null)
//         {
//             foreach ((int tableIndex, Vector3Int coordinate) key
//                      in coordinatesToRemove)
//             {
//                 occupancyByCoordinate.Remove(key);
//             }
//         }


//         if (target.CountsAsTarget)
//         {
//             remainingTargets =
//                 Mathf.Max(
//                     0,
//                     remainingTargets - 1
//                 );
//         }


//         objectPool.Release(
//             target
//         );


//         if (levelGenerated &&
//             remainingTargets == 0)
//         {
//             CompleteLevel();
//         }
//     }


//     private void CompleteLevel()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }

//         levelGenerated = false;

//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} COMPLETE",
//             this
//         );

//         // Existing UnityEvent remains available for any old inspector hooks.
//         onLevelComplete?.Invoke();

//         // Dedicated UI controllers listen to this event.
//         LevelCompleted?.Invoke(levelData);
//     }


//     public void LoadNextLevel()
//     {
//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int nextIndex =
//             FindNextLevelIndex(
//                 currentLevelIndex + 1
//             );

//         if (nextIndex < 0)
//         {
//             Debug.LogWarning(
//                 "Next Addressable level configured nahi hai. Level " +
//                 "Creator se next level SAVE karein.",
//                 this
//             );

//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(nextIndex)
//         );
//     }

//     public void RestartCurrentLevel()
//     {
//         if (isLoadingLevel ||
//             levelData == null)
//         {
//             return;
//         }

//         DestroyActiveCannonBalls();
//         GenerateLevel();
//     }


//     private IEnumerator LoadAddressableLevelRoutine(
//         int levelIndex)
//     {
//         if (isLoadingLevel ||
//             levelDatabase == null)
//         {
//             yield break;
//         }

//         string address =
//             levelDatabase.GetAddress(levelIndex);

//         if (string.IsNullOrWhiteSpace(address))
//         {
//             Debug.LogError(
//                 $"Level index {levelIndex} ka Addressable address missing hai.",
//                 this
//             );
//             yield break;
//         }

//         isLoadingLevel = true;

//         AsyncOperationHandle<GridLevelData> newHandle =
//             Addressables.LoadAssetAsync<GridLevelData>(address);

//         pendingLevelHandle = newHandle;
//         hasPendingLevelHandle = true;

//         yield return newHandle;

//         if (newHandle.Status != AsyncOperationStatus.Succeeded ||
//             newHandle.Result == null)
//         {
//             Debug.LogError(
//                 $"Addressable level load failed: {address}",
//                 this
//             );

//             if (newHandle.IsValid())
//             {
//                 Addressables.Release(newHandle);
//             }

//             hasPendingLevelHandle = false;
//             isLoadingLevel = false;
//             yield break;
//         }

//         /*
//          * Pehle purane instantiated objects destroy hote hain. Ek frame
//          * baad purana Addressables handle release hota hai, isliye bundle
//          * unload ke waqt koi live prefab/material instance nahi rehta.
//          */
//         PrepareForLevelAssetRelease();
//         yield return null;
//         ReleaseActiveLevelHandle();

//         activeLevelHandle = newHandle;
//         hasActiveLevelHandle = true;
//         hasPendingLevelHandle = false;
//         currentLevelIndex = levelIndex;
//         levelData = newHandle.Result;
//         isLoadingLevel = false;

//         SaveCurrentLevelProgress(
//             levelData.LevelNumber
//         );

//         GenerateLevel();
//     }


//     public void LoadLevelByNumber(int levelNumber)
//     {
//         if (levelDatabase == null)
//         {
//             Debug.LogError(
//                 "LoadLevelByNumber: GridLevelDatabase missing hai.",
//                 this
//             );
//             return;
//         }

//         if (levelNumber >
//             levelDatabase.MaximumPlayableLevelNumber)
//         {
//             Debug.LogWarning(
//                 $"Level {levelNumber} playable range se bahar hai. " +
//                 $"Last playable level " +
//                 $"{levelDatabase.MaximumPlayableLevelNumber} hai.",
//                 this
//             );
//             return;
//         }

//         int levelIndex =
//             levelDatabase.FindIndexByLevelNumber(levelNumber);

//         if (levelIndex < 0)
//         {
//             Debug.LogWarning(
//                 $"Addressable Level {levelNumber} catalog mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(levelIndex)
//         );
//     }


//     private void SelectStartingLevel()
//     {
//         if (levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             currentLevelIndex = 0;
//             return;
//         }

//         if (PlayerPrefs.HasKey(SavedLevelNumberKey))
//         {
//             int savedLevelNumber =
//                 PlayerPrefs.GetInt(
//                     SavedLevelNumberKey,
//                     1
//                 );

//             int savedLevelIndex =
//                 levelDatabase.FindIndexByLevelNumber(
//                     savedLevelNumber
//                 );

//             if (savedLevelIndex >= 0 &&
//                 FindNextLevelIndex(savedLevelIndex) ==
//                 savedLevelIndex)
//             {
//                 currentLevelIndex = savedLevelIndex;
//                 return;
//             }
//         }

//         if (startFromLevelOne)
//         {
//             int firstLevelIndex =
//                 FindNextLevelIndex(0);

//             if (firstLevelIndex >= 0)
//             {
//                 currentLevelIndex = firstLevelIndex;
//             }

//             return;
//         }

//         int configuredIndex =
//             levelData != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     levelData.LevelNumber
//                 )
//                 : -1;

//         currentLevelIndex =
//             configuredIndex >= 0
//                 ? configuredIndex
//                 : Mathf.Max(0, FindNextLevelIndex(0));

//     }


//     private static bool IsLiveWorkingLevelPreviewEnabled()
//     {
// #if UNITY_EDITOR
//         return UnityEditor.SessionState.GetBool(
//             "RoyalSmash.LiveWorkingLevelPreview",
//             false
//         );
// #else
//         return false;
// #endif
//     }


//     private static bool ShouldStartLiveWorkingLevelPreview()
//     {
// #if UNITY_EDITOR
//         const string previewSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreview";

//         const string previewLaunchSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreviewLaunch";

//         bool previewEnabled =
//             UnityEditor.SessionState.GetBool(
//                 previewSessionKey,
//                 false
//             );

//         bool launchRequested =
//             UnityEditor.SessionState.GetBool(
//                 previewLaunchSessionKey,
//                 false
//             );

//         UnityEditor.SessionState.SetBool(
//             previewLaunchSessionKey,
//             false
//         );

//         if (!launchRequested)
//         {
//             /*
//              * Toolbar se Play karne par pichli editor preview session
//              * normal game startup ko bypass nahi kar sakti.
//              */
//             UnityEditor.SessionState.SetBool(
//                 previewSessionKey,
//                 false
//             );
//         }

//         return previewEnabled && launchRequested;
// #else
//         return false;
// #endif
//     }


//     private int FindNextLevelIndex(
//         int startingIndex)
//     {
//         if (levelDatabase == null)
//         {
//             return -1;
//         }

//         for (int i = Mathf.Max(0, startingIndex);
//              i < levelDatabase.Count;
//              i++)
//         {
//             int levelNumber =
//                 levelDatabase.GetLevelNumber(i);

//             if (levelNumber >
//                 levelDatabase.MaximumPlayableLevelNumber)
//             {
//                 break;
//             }

//             if (levelNumber > 0 &&
//                 !string.IsNullOrWhiteSpace(
//                     levelDatabase.GetAddress(i)))
//             {
//                 return i;
//             }
//         }

//         return -1;
//     }


//     private void EnsureMainMenuUI()
//     {
//         if (mainMenuPanel == null)
//         {
//             RectTransform[] sceneRects =
//                 FindObjectsByType<RectTransform>(
//                     FindObjectsInactive.Include,
//                     FindObjectsSortMode.None
//                 );

//             foreach (RectTransform sceneRect in sceneRects)
//             {
//                 if (sceneRect != null &&
//                     sceneRect.name.Equals(
//                         "Main menu Panel",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ))
//                 {
//                     mainMenuPanel = sceneRect.gameObject;
//                     break;
//                 }
//             }
//         }

//         if (mainMenuPanel == null)
//         {
//             Debug.LogWarning(
//                 "Main menu Panel scene mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         Button[] menuButtons =
//             mainMenuPanel.GetComponentsInChildren<Button>(true);

//         foreach (Button menuButton in menuButtons)
//         {
//             if (menuButton == null)
//             {
//                 continue;
//             }

//             string buttonName =
//                 menuButton.name.ToLowerInvariant();

//             if (playButton == null &&
//                 buttonName.Contains("play"))
//             {
//                 playButton = menuButton;
//             }
//             else if (resetProgressButton == null &&
//                      buttonName.Contains("reset"))
//             {
//                 resetProgressButton = menuButton;
//             }
//         }

//         if (currentLevelText == null)
//         {
//             TMP_Text[] menuTexts =
//                 mainMenuPanel.GetComponentsInChildren<TMP_Text>(true);

//             foreach (TMP_Text menuText in menuTexts)
//             {
//                 if (menuText != null &&
//                     menuText.text.IndexOf(
//                         "level number",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ) >= 0)
//                 {
//                     currentLevelText = menuText;
//                     break;
//                 }
//             }
//         }

//         if (playButton != null)
//         {
//             playButton.onClick.RemoveListener(
//                 PlayFromMainMenu
//             );
//             playButton.onClick.AddListener(
//                 PlayFromMainMenu
//             );
//         }

//         if (resetProgressButton != null)
//         {
//             resetProgressButton.onClick.RemoveListener(
//                 ResetProgressFromMainMenu
//             );
//             resetProgressButton.onClick.AddListener(
//                 ResetProgressFromMainMenu
//             );
//         }
//     }


//     private void SetGameplayWorldVisible(bool visible)
//     {
//         if (gameplayPanel != null)
//         {
//             gameplayPanel.SetActive(visible);
//         }

//         if (levelOrigin != null &&
//             levelOrigin != transform)
//         {
//             levelOrigin.gameObject.SetActive(visible);
//         }

//         if (runtimeObjectsRoot != null)
//         {
//             runtimeObjectsRoot.gameObject.SetActive(visible);
//         }

//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (cannonController != null)
//         {
//             cannonController.SetGameplayActive(visible);
//         }

//         if (gameplayRestartButton == null)
//         {
//             GameRestartController restartController =
//                 FindFirstObjectByType<GameRestartController>(
//                     FindObjectsInactive.Include
//                 );

//             if (restartController != null)
//             {
//                 gameplayRestartButton =
//                     restartController.gameObject;
//             }
//         }

//         if (gameplayRestartButton != null)
//         {
//             gameplayRestartButton.SetActive(visible);
//         }

//         if (!visible)
//         {
//             foreach (LevelTable table in cachedTables)
//             {
//                 if (table != null)
//                 {
//                     table.gameObject.SetActive(false);
//                 }
//             }

//             // Level Complete UI is now owned by LevelCompleteUIController.
//         }
//     }


//     private void SaveCurrentLevelProgress(int levelNumber)
//     {
//         if (levelNumber <= 0 ||
//             IsLiveWorkingLevelPreviewEnabled())
//         {
//             return;
//         }

//         PlayerPrefs.SetInt(
//             SavedLevelNumberKey,
//             levelNumber
//         );
//         PlayerPrefs.Save();
//         UpdateCurrentLevelText();
//     }


//     private void UpdateCurrentLevelText()
//     {
//         if (currentLevelText == null ||
//             levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             return;
//         }

//         int safeIndex =
//             Mathf.Clamp(
//                 currentLevelIndex,
//                 0,
//                 levelDatabase.Count - 1
//             );

//         int levelNumber =
//             levelDatabase.GetLevelNumber(safeIndex);

//         currentLevelText.text =
//             $"{Mathf.Max(1, levelNumber)}";
//     }


//     private void PrepareForLevelAssetRelease()
//     {
//         ClearCurrentLevel();
//         DestroyActiveCannonBalls();

//         if (objectPool != null)
//         {
//             objectPool.ClearAll();
//         }

//         for (int index = cachedTables.Count - 1;
//              index >= 0;
//              index--)
//         {
//             DestroyCachedTableAt(index);
//         }

//         cachedTables.Clear();
//         cachedTablePrefabs.Clear();
//         currentTable = null;
//     }


//     private static void DestroyActiveCannonBalls()
//     {
//         CannonBallMarker[] cannonBalls =
//             FindObjectsByType<CannonBallMarker>(
//                 FindObjectsInactive.Include,
//                 FindObjectsSortMode.None
//             );

//         foreach (CannonBallMarker cannonBall in cannonBalls)
//         {
//             if (cannonBall == null)
//             {
//                 continue;
//             }

//             if (Application.isPlaying)
//             {
//                 Destroy(cannonBall.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(cannonBall.gameObject);
//             }
//         }
//     }


//     private void ReleaseActiveLevelHandle()
//     {
//         if (!hasActiveLevelHandle)
//         {
//             return;
//         }

//         if (activeLevelHandle.IsValid())
//         {
//             Addressables.Release(activeLevelHandle);
//         }

//         hasActiveLevelHandle = false;
//     }


//     public void ClearCurrentLevel()
//     {
//         levelGenerationRevision++;

//         levelGenerated =
//             false;


//         for (int i =
//                  activeObjects.Count - 1;
//              i >= 0;
//              i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance == null)
//             {
//                 continue;
//             }


//             instance.Cleared -=
//                 HandleObjectCleared;

//             instance.Activated -=
//                 HandleObjectActivated;


//             if (objectPool != null)
//             {
//                 objectPool.Release(
//                     instance
//                 );
//             }
//         }


//         activeObjects.Clear();

//         occupancyByCoordinate.Clear();

//         trackingByInstance.Clear();

//         gridBaseRowByTable.Clear();

//         /*
//          * Fit cache CellSize-specific hai — agar LoadLevel() se
//          * different GridLevelData aa jaye (alag CellSize) to purani
//          * cached scale ghalat hogi.
//          */
//         fitCache.Clear();


//         remainingTargets =
//             0;


//         foreach (LevelTable table in cachedTables)
//         {
//             if (table != null)
//             {
//                 table.gameObject.SetActive(false);
//             }
//         }


//         currentTable =
//             null;
//     }


//     private Transform GetLevelOrigin()
//     {
//         return
//             levelOrigin != null
//                 ? levelOrigin
//                 : transform;
//     }


//     private bool ValidateReferences()
//     {
//         if (levelData == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (!levelData.HasAnyValidTableGrid)
//         {
//             Debug.LogError(
//                 "Manual GridLevelData mein koi painted cell nahi hai. " +
//                 "Level Creator mein grid allocate karke shapes paint karein.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.BlockPalette == null ||
//             levelData.BlockPalette.Count == 0)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein Block Palette empty hai.",
//                 levelData
//             );

//             return false;
//         }

//         bool hasValidPaletteEntry =
//             false;

//         foreach (PhysicsObjectDefinition entry
//                  in levelData.BlockPalette)
//         {
//             if (entry != null &&
//                 entry.Prefab != null)
//             {
//                 hasValidPaletteEntry =
//                     true;

//                 break;
//             }
//         }

//         if (!hasValidPaletteEntry)
//         {
//             Debug.LogError(
//                 "Grid Level Data ki Block Palette mein koi valid " +
//                 "Prefab wali entry nahi hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.TableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (objectPool == null)
//         {
//             Debug.LogError(
//                 "Physics Object Pool missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (runtimeObjectsRoot == null)
//         {
//             Debug.LogError(
//                 "Runtime Objects Root missing hai.",
//                 this
//             );

//             return false;
//         }


//         Vector3 runtimeScale =
//             runtimeObjectsRoot.lossyScale;


//         if (Mathf.Abs(runtimeScale.x - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.y - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.z - 1f) > 0.01f)
//         {
//             Debug.LogWarning(
//                 "RuntimeObjects ki world Scale ideally 1,1,1 honi chahiye.",
//                 runtimeObjectsRoot
//             );
//         }


//         return true;
//     }


//     private void OnDestroy()
//     {
//         for (int i = 0;
//              i < activeObjects.Count;
//              i++)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance != null)
//             {
//                 instance.Cleared -=
//                     HandleObjectCleared;

//                 instance.Activated -=
//                     HandleObjectActivated;
//             }
//         }

//         ReleaseActiveLevelHandle();

//         if (hasPendingLevelHandle &&
//             pendingLevelHandle.IsValid())
//         {
//             Addressables.Release(pendingLevelHandle);
//         }

//         hasPendingLevelHandle = false;
//     }
// }







// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.Events;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.UI;

// public sealed class LevelRuntimeController : MonoBehaviour
// {
//     private const string SavedLevelNumberKey =
//         "RoyalSmash.CurrentLevelNumber";

//     public event System.Action<GridLevelData> LevelGenerated;
//     public event System.Action<GridLevelData> LevelCompleted;






// // [Header("Level Progress")]
// // [SerializeField]
// // private LevelMilestoneProgressBar milestoneProgressBar;









//     [Header("Level")]

//     [SerializeField]
//     private GridLevelData levelData;

//     [Tooltip(
//         "Level Creator ke banaye tamam levels ki central database. " +
//         "Runtime mein individual levels manually assign nahi karne."
//     )]
//     [SerializeField]
//     private GridLevelDatabase levelDatabase;

//     [SerializeField]
//     private bool startFromLevelOne = true;


//     [Header("Main Menu")]

//     [SerializeField]
//     private bool showMainMenuOnStartup = true;

//     [SerializeField]
//     private GameObject mainMenuPanel;

//     [Tooltip(
//         "Gameplay UI panel. Main Menu par hidden rahega aur Play press karte hi " +
//         "gameplay world ke sath active ho jayega."
//     )]
//     [SerializeField]
//     private GameObject gameplayPanel;

//     [SerializeField]
//     private Button resetProgressButton;

//     [SerializeField]
//     private TMP_Text currentLevelText;


//     [Header("References")]

//     [SerializeField]
//     private PhysicsObjectPool objectPool;

//     [SerializeField]
//     private Transform levelOrigin;

//     [SerializeField]
//     private Transform runtimeObjectsRoot;


//     [Header("Support Chain")]

//     [Tooltip(
//         "Support khone ke kitne FixedUpdate frames baad upar wala " +
//         "block khud bhi dynamic ho jayega."
//     )]
//     [SerializeField, Range(1, 10)]
//     private int unsupportedFramesRequired = 2;

//     [Tooltip(
//         "Half-supported piece ko missing-support side par tip karne wala " +
//         "small sideways impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipImpulse = 0.18f;

//     [Tooltip(
//         "Exact edge-balance ko break karne wala natural rotation impulse."
//     )]
//     [SerializeField, Min(0f)]
//     private float unsupportedTipTorque = 0.28f;


//     [Header("Mirrored Depth Physics")]

//     [Tooltip(
//         "Kisi bhi 2+ depth-layer level mein hit block ka impact same X/Y " +
//         "par maujood baqi depth-layer blocks ko bhi unlock aur push karega. " +
//         "Ye gameplay link Mirror Paint authoring option se independent hai."
//     )]
//     [SerializeField]
//     private bool linkMirroredDepthLayerPhysics = true;

//     [Tooltip(
//         "Front block ke impulse ka kitna hissa mirrored layers ko mile."
//     )]
//     [SerializeField, Range(0f, 1.5f)]
//     private float mirroredLayerImpulseMultiplier = 0.45f;

//     [Tooltip(
//         "Mirrored depth layers exact same frame par na giren. Har linked " +
//         "block ko is range mein chhota natural delay milta hai."
//     )]
//     [SerializeField]
//     private Vector2 mirroredActivationDelayRange =
//         new Vector2(0.09f, 0.22f);

//     [Tooltip(
//         "Mirrored impulse ki strength mein per-block random variation."
//     )]
//     [SerializeField]
//     private Vector2 mirroredImpulseVariation =
//         new Vector2(0.78f, 0.96f);

//     [Tooltip(
//         "Mirrored force direction mein maximum random angle variation."
//     )]
//     [SerializeField, Range(0f, 25f)]
//     private float mirroredDirectionVariationDegrees = 4f;

//     [Tooltip(
//         "Linked blocks ko alag natural spin dene ke liye random torque."
//     )]
//     [SerializeField, Range(0f, 1f)]
//     private float mirroredRandomTorqueMultiplier = 0.035f;


//     [Header("Events")]

//     [SerializeField]
//     private UnityEvent onLevelGenerated;

//     [SerializeField]
//     private UnityEvent onLevelComplete;


//     private sealed class DefinitionFitData
//     {
//         public Vector3 Scale;
//         public Vector3 BoundsCenterOffset;
//     }


//     private sealed class GridTrackingEntry
//     {
//         public Vector3 InitialPosition;
//         public int UnsupportedFrames;
//         public int TableIndex;
//         public Quaternion SurfaceRotation;
//     }


//     private readonly List<PhysicsTowerObject>
//         activeObjects =
//             new List<PhysicsTowerObject>();

//     /// <summary>
//     /// Keyed by (definition, span) since a bigger footprint (e.g. a
//     /// 2-cell-tall piece) needs its own fit scale, distinct from the
//     /// same definition used at 1x1x1.
//     /// </summary>
//     private readonly Dictionary<(PhysicsObjectDefinition, int spanKey), DefinitionFitData>
//         fitCache =
//             new Dictionary<(PhysicsObjectDefinition, int), DefinitionFitData>();

//     /// <summary>
//     /// What currently occupies each grid cell — used by the support
//     /// chain to answer "is the cell below me still standing?".
//     /// </summary>
//     private readonly Dictionary<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject>
//         occupancyByCoordinate =
//             new Dictionary<(int, Vector3Int), PhysicsTowerObject>();

//     private readonly Dictionary<PhysicsTowerObject, GridTrackingEntry>
//         trackingByInstance =
//             new Dictionary<PhysicsTowerObject, GridTrackingEntry>();


//     private readonly List<LevelTable> cachedTables =
//         new List<LevelTable>();

//     private readonly List<LevelTable> cachedTablePrefabs =
//         new List<LevelTable>();

//     private readonly List<int> tablePhysicsReleaseCycleTargets =
//         new List<int>();

//     private readonly HashSet<int> releasedTablePhysicsIndices =
//         new HashSet<int>();

//     private LevelTable currentTable;


//     private int remainingTargets;

//     private bool levelGenerated;

//     private bool isPropagatingMirroredActivation;

//     private int levelGenerationRevision;

//     private float runtimeTableAnimationTime;

//     private int currentLevelIndex;

//     private AsyncOperationHandle<GridLevelData>
//         activeLevelHandle;

//     private bool hasActiveLevelHandle;

//     private AsyncOperationHandle<GridLevelData>
//         pendingLevelHandle;

//     private bool hasPendingLevelHandle;

//     private bool isLoadingLevel;

//     private CannonController cannonController;

//     private GameObject gameplayRestartButton;

//     /// <summary>
//     /// Lowest occupied grid row for the current level — the "floor"
//     /// row that always counts as supported by the table, even if the
//     /// designer didn't start painting at y=0.
//     /// </summary>
//     private readonly Dictionary<int, int> gridBaseRowByTable =
//         new Dictionary<int, int>();


//     public GridLevelData CurrentLevelData =>
//         levelData;

//     public LevelTable CurrentTable =>
//         currentTable;

//     public IReadOnlyList<LevelTable> CurrentTables =>
//         cachedTables;

//     public int RemainingTargets =>
//         remainingTargets;

//     public bool IsLevelGenerated =>
//         levelGenerated;

//     public int CurrentLevelIndex =>
//         currentLevelIndex;

//     public bool HasNextLevel =>
//         FindNextLevelIndex(currentLevelIndex + 1) >= 0;


//     private void Start()
//     {
//         if (ShouldStartLiveWorkingLevelPreview())
//         {
//             currentLevelIndex = 0;
//             GenerateLevel();
//             return;
//         }

//         SelectStartingLevel();

//         if (FindNextLevelIndex(currentLevelIndex) < 0)
//         {
//             Debug.LogError(
//                 "Addressable level catalog empty hai. Level Creator se " +
//                 "kam az kam ek level SAVE karein.",
//                 this
//             );
//             return;
//         }

//         if (showMainMenuOnStartup)
//         {
//             ShowMainMenu();
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ShowMainMenu()
//     {
//         EnsureMainMenuUI();
//         SetGameplayWorldVisible(false);
//         UpdateCurrentLevelText();

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(true);
//         }
//     }


//     public void PlayFromMainMenu()
//     {
//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int playableIndex =
//             FindNextLevelIndex(currentLevelIndex);

//         if (playableIndex < 0)
//         {
//             Debug.LogError(
//                 "Play: koi saved Addressable level configured nahi hai.",
//                 this
//             );
//             return;
//         }

//         currentLevelIndex = playableIndex;

//         if (mainMenuPanel != null)
//         {
//             mainMenuPanel.SetActive(false);
//         }

//         SetGameplayWorldVisible(true);

//         StartCoroutine(
//             LoadAddressableLevelRoutine(currentLevelIndex)
//         );
//     }


//     public void ResetProgressFromMainMenu()
//     {
//         PlayerPrefs.DeleteKey(SavedLevelNumberKey);
//         PlayerPrefs.Save();

//         int firstLevelIndex =
//             FindNextLevelIndex(0);

//         currentLevelIndex =
//             firstLevelIndex >= 0
//                 ? firstLevelIndex
//                 : 0;

//         UpdateCurrentLevelText();
//     }


//     public void LoadLevel(
//         GridLevelData newLevelData)
//     {
//         if (newLevelData == null)
//         {
//             Debug.LogError(
//                 "LoadLevel: GridLevelData null hai.",
//                 this
//             );

//             return;
//         }


//         if (hasActiveLevelHandle)
//         {
//             PrepareForLevelAssetRelease();
//             ReleaseActiveLevelHandle();
//         }

//         levelData = newLevelData;

//         int sequenceIndex =
//             levelDatabase != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     newLevelData.LevelNumber
//                 )
//                 : -1;

//         if (sequenceIndex >= 0)
//         {
//             currentLevelIndex = sequenceIndex;
//         }

//         SaveCurrentLevelProgress(
//             newLevelData.LevelNumber
//         );


//         GenerateLevel();
//     }


//     [ContextMenu("Generate Level")]
//     public void GenerateLevel()
//     {
//         if (!ValidateReferences())
//         {
//             return;
//         }


//         ClearCurrentLevel();

//         runtimeTableAnimationTime = 0f;


//         objectPool.PrepareForLevel(
//             levelData
//         );


//         if (!PrepareTable())
//         {
//             return;
//         }

//         PrepareTablePhysicsReleaseTargets();


//         remainingTargets =
//             0;


//         SpawnGrid();


//         Physics.SyncTransforms();


//         levelGenerated =
//             true;


//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} GENERATED\n" +
//             $"Boxes: {activeObjects.Count}\n" +
//             $"Targets: {remainingTargets}",
//             this
//         );


//         LevelGenerated?.Invoke(levelData);

//         onLevelGenerated?.Invoke();


//         if (remainingTargets <= 0)
//         {
//             Debug.LogWarning(
//                 "Generated level mein koi target nahi mila.",
//                 this
//             );
//         }
//     }


//     /// <summary>
//     /// Measures a definition's prefab once (identity rotation, prefab's
//     /// authored scale) and derives the localScale + pivot-to-collider
//     /// offset needed so its collider bounds exactly fill levelData's
//     /// CellSize. Cached per-definition so mixed shapes (cube, cylinder,
//     /// ...) all land cleanly on the same uniform grid without measuring
//     /// on every single cell spawn.
//     /// </summary>
//     private bool TryGetFitData(
//         PhysicsObjectDefinition definition,
//         int spanX,
//         int spanY,
//         int spanZ,
//         PieceOrientation orientation,
//         out DefinitionFitData fitData)
//     {
//         int spanKey =
//             spanX * 10000 +
//             spanY * 100 +
//             spanZ +
//             (int)orientation * 1000000;

//         (PhysicsObjectDefinition, int) cacheKey =
//             (definition, spanKey);

//         if (fitCache.TryGetValue(
//                 cacheKey,
//                 out fitData))
//         {
//             return true;
//         }

//         fitData = null;

//         PhysicsTowerObject measurementObject =
//             objectPool.Get(
//                 definition,
//                 Vector3.zero,
//                 Quaternion.identity,
//                 runtimeObjectsRoot
//             );

//         if (measurementObject == null)
//         {
//             Debug.LogError(
//                 "Grid object measure karne ke liye pool object nahi mila.",
//                 this
//             );

//             return false;
//         }

//         Physics.SyncTransforms();

//         if (!measurementObject.TryGetPhysicsBounds(
//                 out Bounds bounds))
//         {
//             Debug.LogError(
//                 "Physics object par valid non-trigger Collider nahi mila.",
//                 measurementObject
//             );

//             objectPool.Release(measurementObject);

//             return false;
//         }

//         Vector3 measuredSize =
//             bounds.size;

//         Vector3 measuredCenterOffset =
//             bounds.center -
//             measurementObject.transform.position;

//         objectPool.Release(measurementObject);

//         if (measuredSize.x <= 0.0001f ||
//             measuredSize.y <= 0.0001f ||
//             measuredSize.z <= 0.0001f)
//         {
//             Debug.LogError(
//                 $"{definition.DisplayName}: collider size invalid hai.",
//                 this
//             );

//             return false;
//         }

//         Vector3 authoredScale =
//             definition.Prefab.transform.localScale;

//         Vector3 finalScale;

//         Vector3 finalOffset;

//         if (definition.AutoFitToCell)
//         {
//             /*
//              * Span > 1 par target size sirf spanCount * cellSize nahi —
//              * internal gaps bhi fill hote hain (spanX*cellSize.x +
//              * (spanX-1)*gap) taake merged piece apne poore footprint
//              * ke outer edges tak seamless dikhe, jaisa cells alag alag
//              * (gap ke sath) hote to unka combined outer span hota.
//              */
//             Vector3 gridAlignedTargetSize =
//                 new Vector3(
//                     spanX * levelData.CellSize.x +
//                     (spanX - 1) * levelData.HorizontalGap,

//                     spanY * levelData.CellSize.y +
//                     (spanY - 1) * levelData.VerticalGap,

//                     spanZ * levelData.CellSize.z +
//                     (spanZ - 1) * levelData.DepthGap
//                 );

//             /*
//              * gridAlignedTargetSize world/grid axes ke liye hai.
//              * Lekin fit-scale prefab ke apne UNROTATED local axes par
//              * compute hoti hai — agar orientation tip kar rahi hai
//              * (Lying X/Z) to local axes rotate hone ke baad world axes
//              * se match nahi karte, is liye target size ko yahan
//              * local axes par re-map karte hain (spawn time par
//              * PieceOrientationUtility.GetRotation() isi mapping ko
//              * wapas world space mein rotate kar degi).
//              */
//             Vector3 targetSize =
//                 PieceOrientationUtility.GetLocalAxisTargetSize(
//                     gridAlignedTargetSize,
//                     orientation
//                 );

//             /*
//              * boundsAtScale1 = measuredSize / authoredScale
//              * finalScale     = targetSize / boundsAtScale1
//              *                = targetSize * authoredScale / measuredSize
//              *
//              * Isi tarah offset bhi authored scale se scale-1 basis par
//              * normalize karke, phir finalScale par re-apply hota hai —
//              * taake off-center collider pivot bhi correct rahe.
//              */
//             finalScale =
//                 new Vector3(
//                     targetSize.x * authoredScale.x / measuredSize.x,
//                     targetSize.y * authoredScale.y / measuredSize.y,
//                     targetSize.z * authoredScale.z / measuredSize.z
//                 );

//             Vector3 offsetAtScale1 =
//                 new Vector3(
//                     measuredCenterOffset.x / authoredScale.x,
//                     measuredCenterOffset.y / authoredScale.y,
//                     measuredCenterOffset.z / authoredScale.z
//                 );

//             finalOffset =
//                 Vector3.Scale(
//                     offsetAtScale1,
//                     finalScale
//                 );
//         }
//         else
//         {
//             finalScale =
//                 authoredScale;

//             finalOffset =
//                 measuredCenterOffset;
//         }

//         finalScale =
//             Vector3.Scale(
//                 finalScale,
//                 definition.ManualScaleMultiplier
//             );

//         fitData =
//             new DefinitionFitData
//             {
//                 Scale = finalScale,
//                 BoundsCenterOffset = finalOffset
//             };

//         fitCache[cacheKey] =
//             fitData;

//         return true;
//     }


//     private void SpawnGrid()
//     {
//         Vector3Int occupiedMin =
//             new Vector3Int(
//                 int.MaxValue,
//                 int.MaxValue,
//                 int.MaxValue
//             );

//         Vector3Int occupiedMax =
//             new Vector3Int(
//                 int.MinValue,
//                 int.MinValue,
//                 int.MinValue
//             );

//         if (currentTable == null ||
//             !currentTable.TryGetTowerSurface(
//                 out Vector3 primarySurfacePosition,
//                 out Quaternion primarySurfaceRotation))
//         {
//             Debug.LogError(
//                 "PRIMARY table ki tower surface calculate nahi ho saki.",
//                 currentTable
//             );

//             return;
//         }

//         int tableCount =
//             Mathf.Max(1, cachedTables.Count);

//         Vector3[] surfacePositions =
//             new Vector3[tableCount];

//         Quaternion[] surfaceRotations =
//             new Quaternion[tableCount];

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             LevelTable table =
//                 tableIndex < cachedTables.Count
//                     ? cachedTables[tableIndex]
//                     : null;

//             if (table != null &&
//                 table.TryGetTowerSurface(
//                     out Vector3 tableSurfacePosition,
//                     out Quaternion tableSurfaceRotation))
//             {
//                 surfacePositions[tableIndex] = tableSurfacePosition;
//                 surfaceRotations[tableIndex] = tableSurfaceRotation;
//             }
//             else
//             {
//                 surfacePositions[tableIndex] = primarySurfacePosition;
//                 surfaceRotations[tableIndex] = primarySurfaceRotation;
//             }
//         }

//         Dictionary<int, Vector3Int> occupiedMinByTable =
//             new Dictionary<int, Vector3Int>();

//         Dictionary<int, Vector3Int> occupiedMaxByTable =
//             new Dictionary<int, Vector3Int>();

//         bool foundAnyOccupiedCell = false;

//         for (int tableIndex = 0;
//              tableIndex < tableCount;
//              tableIndex++)
//         {
//             if (!levelData.TryGetOccupiedBounds(
//                     tableIndex,
//                     out Vector3Int tableMinimum,
//                     out Vector3Int tableMaximum))
//             {
//                 continue;
//             }

//             foundAnyOccupiedCell = true;
//             occupiedMinByTable[tableIndex] = tableMinimum;
//             occupiedMaxByTable[tableIndex] = tableMaximum;
//             occupiedMin = Vector3Int.Min(occupiedMin, tableMinimum);
//             occupiedMax = Vector3Int.Max(occupiedMax, tableMaximum);
//         }

//         if (!foundAnyOccupiedCell)
//         {
//             Debug.LogError(
//                 "Kisi bhi table ke manual grid mein occupied cells nahi hain.",
//                 levelData
//             );

//             return;
//         }

//         gridBaseRowByTable.Clear();

//         foreach (KeyValuePair<int, Vector3Int> entry
//                  in occupiedMinByTable)
//         {
//             gridBaseRowByTable[entry.Key] =
//                 entry.Value.y;
//         }


//         Vector3 cellSize =
//             levelData.CellSize;


//         float stepX =
//             cellSize.x +
//             levelData.HorizontalGap;


//         float stepY =
//             cellSize.y +
//             levelData.VerticalGap;


//         float stepZ =
//             cellSize.z +
//             levelData.DepthGap;


//         /*
//          * Important:
//          *
//          * Y outer loop hai.
//          * Isliye logical spawn order bottom → top rahega.
//          */
//         for (int y = occupiedMin.y;
//              y <= occupiedMax.y;
//              y++)
//         {
//             for (int z = occupiedMin.z;
//                  z <= occupiedMax.z;
//                  z++)
//             {
//                 for (int x = occupiedMin.x;
//                      x <= occupiedMax.x;
//                      x++)
//                 {
//                     for (int authoredTableIndex = 0;
//                          authoredTableIndex < tableCount;
//                          authoredTableIndex++)
//                     {
//                         if (!occupiedMinByTable.ContainsKey(
//                                 authoredTableIndex))
//                         {
//                             continue;
//                         }

//                         GridCellData cell =
//                             levelData.GetCell(
//                                 x,
//                                 y,
//                                 z,
//                                 authoredTableIndex
//                             );


//                     if (cell == null ||
//                         !cell.Occupied)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Covered cells (bare footprint ka hissa) khud
//                      * spawn nahi hoti — unka anchor cell hi spawn
//                      * karta hai (neeche is loop mein aage aake).
//                      */
//                     if (cell.IsCovered)
//                     {
//                         continue;
//                     }


//                     int blockTableIndex =
//                         authoredTableIndex;

//                     Vector3Int tableOccupiedMin =
//                         occupiedMinByTable[blockTableIndex];

//                     Vector3Int tableOccupiedMax =
//                         occupiedMaxByTable[blockTableIndex];

//                     float occupiedCenterX =
//                         (
//                             tableOccupiedMin.x +
//                             tableOccupiedMax.x
//                         ) *
//                         0.5f;

//                     float occupiedCenterZ =
//                         (
//                             tableOccupiedMin.z +
//                             tableOccupiedMax.z
//                         ) *
//                         0.5f;

//                     Vector3 surfacePosition =
//                         surfacePositions[blockTableIndex];

//                     Quaternion surfaceRotation =
//                         surfaceRotations[blockTableIndex];


//                     int spanX =
//                         Mathf.Max(1, cell.SpanX);

//                     int spanY =
//                         Mathf.Max(1, cell.SpanY);

//                     int spanZ =
//                         Mathf.Max(1, cell.SpanZ);


//                     /*
//                      * X:
//                      * Shape automatically center.
//                      *
//                      * Span > 1 ho to poore footprint (x..x+spanX-1)
//                      * ka center use hota hai, sirf anchor cell ka nahi.
//                      */
//                     float localX =
//                         (
//                             x -
//                             occupiedCenterX
//                         ) *
//                         stepX +
//                         (spanX - 1) *
//                         stepX *
//                         0.5f +
//                         cell.LocalOffset.x *
//                         stepX;


//                     /*
//                      * Y:
//                      * Lowest occupied manual-grid row
//                      * directly table top par start hogi.
//                      *
//                      * Transparent bottom margin
//                      * automatically ignore hota hai.
//                      */
//                     float localY =
//                         cellSize.y *
//                         0.5f +
//                         (
//                             y -
//                             tableOccupiedMin.y
//                         ) *
//                         stepY +
//                         (spanY - 1) *
//                         stepY *
//                         0.5f +
//                         cell.LocalOffset.y *
//                         stepY;


//                     /*
//                      * Z:
//                      * Complete depth centered.
//                      */
//                     float localZ =
//                         (
//                             z -
//                             occupiedCenterZ
//                         ) *
//                         stepZ +
//                         (spanZ - 1) *
//                         stepZ *
//                         0.5f +
//                         cell.LocalOffset.z *
//                         stepZ;


//                     Vector3 localCellCenter =
//                         new Vector3(
//                             localX,
//                             localY,
//                             localZ
//                         )
//                         +
//                         levelData.GridOffset;


//                     PhysicsObjectDefinition definition =
//                         levelData.GetPaletteEntry(
//                             cell.DefinitionIndex
//                         );

//                     if (definition == null ||
//                         definition.Prefab == null)
//                     {
//                         Debug.LogWarning(
//                             $"Cell ({x},{y},{z}): palette entry " +
//                             $"{cell.DefinitionIndex} invalid hai, skip.",
//                             levelData
//                         );

//                         continue;
//                     }

//                     PieceOrientation orientation =
//                         cell.Orientation;

//                     if (!TryGetFitData(
//                             definition,
//                             spanX,
//                             spanY,
//                             spanZ,
//                             orientation,
//                             out DefinitionFitData fitData))
//                     {
//                         continue;
//                     }


//                     /*
//                      * Orientation ka extra tip (Lying X/Z) surface
//                      * rotation ke UPAR apply hoti hai — piece ko table
//                      * ke sath sath khud bhi rotate karta hai.
//                      */
//                     Quaternion pieceRotation =
//                         surfaceRotation *
//                         PieceOrientationUtility.GetRotation(
//                             orientation,
//                             cell.CustomZRotation
//                         ) *
//                         Quaternion.Euler(
//                             cell.RotationEulerOffset
//                         );


//                     Vector3 pieceScale =
//                         Vector3.Scale(
//                             fitData.Scale,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 desiredBoundsCenter =
//                         surfacePosition +
//                         surfaceRotation *
//                         localCellCenter;


//                     /*
//                      * Prefab pivot center par na bhi ho
//                      * to collider bottom proper align hoga.
//                      */
//                     Vector3 rotatedBoundsOffset =
//                         pieceRotation *
//                         Vector3.Scale(
//                             fitData.BoundsCenterOffset,
//                             cell.ScaleMultiplier
//                         );


//                     Vector3 spawnPosition =
//                         desiredBoundsCenter -
//                         rotatedBoundsOffset;


//                     PhysicsTowerObject instance =
//                         objectPool.Get(
//                             definition,
//                             spawnPosition,
//                             pieceRotation,
//                             runtimeObjectsRoot
//                         );


//                     if (instance == null)
//                     {
//                         continue;
//                     }


//                     /*
//                      * Pool.Get() prefab ki authored scale laga deta hai —
//                      * yahan usay cell-fit scale se override karte hain
//                      * taake mixed shapes (cube/cylinder) uniform grid
//                      * mein clean fit hon.
//                      */
//                     instance.transform.localScale =
//                         pieceScale;


//                     /*
//                      * Pool se purani property-block state clear karo.
//                      * Default visual hamesha prefab ka apna material aur
//                      * texture hai; paint tint definition par optional hai.
//                      */
//                     instance.RestorePrefabVisual();

//                     if (definition.TintWithPaintColor)
//                     {
//                         instance.SetVisualColor(
//                             cell.Color
//                         );
//                     }


//                     Vector3Int anchorCoordinate =
//                         new Vector3Int(x, y, z);

//                     instance.SetGridFootprint(
//                         anchorCoordinate,
//                         new Vector3Int(
//                             spanX,
//                             spanY,
//                             spanZ
//                         ),
//                         cell.LocalOffset
//                     );

//                     instance.ConfigureBreakable(
//                         cell.Breakable,
//                         cell.HitsToBreak
//                     );

//                     /*
//                      * Bare footprint ke SAARE cells is instance ko
//                      * point karte hain — is se agar upar kuch spawn ho
//                      * to woh footprint ke kisi bhi column par "supported"
//                      * sahi detect hoga.
//                      */
//                     for (int fz = 0; fz < spanZ; fz++)
//                     {
//                         for (int fy = 0; fy < spanY; fy++)
//                         {
//                             for (int fx = 0; fx < spanX; fx++)
//                             {
//                                 occupancyByCoordinate[
//                                     (
//                                         blockTableIndex,
//                                         new Vector3Int(
//                                             x + fx,
//                                             y + fy,
//                                             z + fz
//                                         )
//                                     )
//                                 ] = instance;
//                             }
//                         }
//                     }

//                     trackingByInstance[instance] =
//                         new GridTrackingEntry
//                         {
//                             InitialPosition = spawnPosition,
//                             UnsupportedFrames = 0,
//                             TableIndex = blockTableIndex,
//                             SurfaceRotation = surfaceRotation
//                         };


//                     instance.Cleared +=
//                         HandleObjectCleared;

//                     instance.Activated +=
//                         HandleObjectActivated;


//                     activeObjects.Add(
//                         instance
//                     );


//                     if (instance.CountsAsTarget)
//                     {
//                         remainingTargets++;
//                     }
//                     }
//                 }
//             }
//         }
//     }


//     private int ResolveBlockTableIndex(int requestedIndex)
//     {
//         if (requestedIndex >= 0 &&
//             requestedIndex < cachedTables.Count &&
//             cachedTables[requestedIndex] != null)
//         {
//             return requestedIndex;
//         }

//         return 0;
//     }


//     private void FixedUpdate()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }

//         AnimateEnabledTables();

//         if (activeObjects.Count == 0)
//         {
//             return;
//         }

//         /*
//          * Snapshot: activeObjects FixedUpdate ke dauran
//          * HandleObjectCleared se modify ho sakti hai
//          * (ActivatePhysics() khud kuch remove nahi karta,
//          * lekin safety ke liye copy le lete hain).
//          */
//         for (int i = activeObjects.Count - 1; i >= 0; i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];

//             if (instance == null || !instance.IsLocked)
//             {
//                 continue;
//             }

//             if (!trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry))
//             {
//                 continue;
//             }

//             if (IsSupported(
//                     instance,
//                     entry,
//                     out Vector3 unsupportedLocalDirection))
//             {
//                 entry.UnsupportedFrames = 0;
//                 continue;
//             }

//             entry.UnsupportedFrames++;

//             if (entry.UnsupportedFrames >=
//                 unsupportedFramesRequired)
//             {
//                 Vector3 worldFallDirection =
//                     entry.SurfaceRotation *
//                     unsupportedLocalDirection.normalized;

//                 Vector3 worldUp =
//                     entry.SurfaceRotation * Vector3.up;

//                 Vector3 tippingTorque =
//                     Vector3.Cross(
//                         worldUp,
//                         worldFallDirection
//                     ).normalized *
//                     unsupportedTipTorque *
//                     Random.Range(0.9f, 1.1f);

//                 instance.ActivatePhysics(
//                     worldFallDirection *
//                     unsupportedTipImpulse *
//                     Random.Range(0.9f, 1.1f),
//                     tippingTorque
//                 );
//             }
//         }
//     }


//     /// <summary>
//     /// Bottom row table par khadi hoti hai. Baqi pieces ke poore bottom
//     /// footprint mein kam az kam ek LOCKED support hona chahiye. Hit hua
//     /// dynamic block foran support count hona band ho jata hai, chahe woh
//     /// abhi apni spawn position se zyada move na bhi hua ho.
//     ///
//     /// Multi-cell footprint (span > 1) wale pieces ke liye sirf
//     /// "kahin bhi neeche support hai" (OR) kaafi nahi — agar ek extreme
//     /// edge (min ya max column) ka support hat jaye to piece ko us taraf
//     /// tip hona chahiye, chahe doosra extreme abhi bhi supported ho.
//     /// Ye seam-offset pieces (0.5 Center-on-Seam) aur plain grid-aligned
//     /// multi-span pieces, dono par apply hota hai.
//     /// </summary>
//     private bool IsSupported(
//         PhysicsTowerObject instance,
//         GridTrackingEntry trackingEntry,
//         out Vector3 unsupportedLocalDirection)
//     {
//         unsupportedLocalDirection = Vector3.zero;

//         if (instance == null)
//         {
//             unsupportedLocalDirection = Vector3.right;
//             return false;
//         }

//         Vector3Int coordinate =
//             instance.GridCoordinate;

//         int baseRow =
//             gridBaseRowByTable.TryGetValue(
//                 trackingEntry.TableIndex,
//                 out int tableBaseRow)
//                 ? tableBaseRow
//                 : coordinate.y;

//         if (coordinate.y <= baseRow)
//         {
//             return true;
//         }

//         Vector3Int span =
//             instance.GridSpan;

//         Vector3 localOffset =
//             instance.GridLocalOffset;

//         int minimumX =
//             coordinate.x +
//             Mathf.FloorToInt(localOffset.x);

//         int maximumX =
//             coordinate.x +
//             Mathf.Max(1, span.x) - 1 +
//             Mathf.CeilToInt(localOffset.x);

//         int minimumZ =
//             coordinate.z +
//             Mathf.FloorToInt(localOffset.z);

//         int maximumZ =
//             coordinate.z +
//             Mathf.Max(1, span.z) - 1 +
//             Mathf.CeilToInt(localOffset.z);

//         bool seamOnX =
//             !Mathf.Approximately(
//                 localOffset.x,
//                 Mathf.Round(localOffset.x)
//             );

//         bool seamOnZ =
//             !Mathf.Approximately(
//                 localOffset.z,
//                 Mathf.Round(localOffset.z)
//             );

//         /*
//          * Seam offset ke bina bhi agar piece ek se zyada grid column
//          * span kar raha hai (e.g. horizontal 2-cell beam), to sirf
//          * "kahin bhi support hai" kaafi nahi — dono extremes (min aur
//          * max column) ka support alag alag check hona chahiye, warna
//          * ek pillar hat jane par doosre pillar ka sahara poore beam
//          * ko galat tarah "supported" dikha deta hai.
//          */
//         bool spansMultipleColumnsX =
//             maximumX > minimumX;

//         bool spansMultipleColumnsZ =
//             maximumZ > minimumZ;

//         bool hasAnySupport = false;
//         bool minimumXSupported = false;
//         bool maximumXSupported = false;
//         bool minimumZSupported = false;
//         bool maximumZSupported = false;

//         for (int z = minimumZ; z <= maximumZ; z++)
//         {
//             for (int x = minimumX; x <= maximumX; x++)
//             {
//                 Vector3Int below =
//                     new Vector3Int(
//                         x,
//                         coordinate.y - 1,
//                         z
//                     );

//                 if (occupancyByCoordinate.TryGetValue(
//                         (trackingEntry.TableIndex, below),
//                         out PhysicsTowerObject supportInstance) &&
//                     supportInstance != null &&
//                     supportInstance != instance &&
//                     supportInstance.IsLocked)
//                 {
//                     hasAnySupport = true;
//                     minimumXSupported |= x == minimumX;
//                     maximumXSupported |= x == maximumX;
//                     minimumZSupported |= z == minimumZ;
//                     maximumZSupported |= z == maximumZ;
//                 }
//             }
//         }

//         if (!hasAnySupport)
//         {
//             unsupportedLocalDirection =
//                 GetNaturalUnsupportedDirection(
//                     coordinate,
//                     localOffset
//                 );

//             return false;
//         }

//         if ((seamOnX || spansMultipleColumnsX) &&
//             (!minimumXSupported || !maximumXSupported))
//         {
//             if (!minimumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.left;
//             }

//             if (!maximumXSupported)
//             {
//                 unsupportedLocalDirection += Vector3.right;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.x >= 0f
//                         ? Vector3.right
//                         : Vector3.left;
//             }

//             return false;
//         }

//         if ((seamOnZ || spansMultipleColumnsZ) &&
//             (!minimumZSupported || !maximumZSupported))
//         {
//             if (!minimumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.back;
//             }

//             if (!maximumZSupported)
//             {
//                 unsupportedLocalDirection += Vector3.forward;
//             }

//             if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
//             {
//                 unsupportedLocalDirection =
//                     localOffset.z >= 0f
//                         ? Vector3.forward
//                         : Vector3.back;
//             }

//             return false;
//         }

//         return true;
//     }


//     private static Vector3 GetNaturalUnsupportedDirection(
//         Vector3Int coordinate,
//         Vector3 localOffset)
//     {
//         Vector3 direction =
//             new Vector3(
//                 localOffset.x,
//                 0f,
//                 localOffset.z
//             );

//         if (direction.sqrMagnitude > 0.001f)
//         {
//             return direction.normalized;
//         }

//         int hash =
//             coordinate.x * 73856093 ^
//             coordinate.y * 19349663 ^
//             coordinate.z * 83492791;

//         float x =
//             (hash & 1) == 0 ? -1f : 1f;

//         float z =
//             (hash & 2) == 0 ? -0.45f : 0.45f;

//         return new Vector3(x, 0f, z).normalized;
//     }


//     private bool PrepareTable()
//     {
//         int requiredTableCount =
//             levelData.TableCount;

//         if (requiredTableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "GridLevelData mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }

//         while (cachedTables.Count > requiredTableCount)
//         {
//             DestroyCachedTableAt(
//                 cachedTables.Count - 1
//             );

//             cachedTables.RemoveAt(
//                 cachedTables.Count - 1
//             );

//             cachedTablePrefabs.RemoveAt(
//                 cachedTablePrefabs.Count - 1
//             );
//         }

//         Transform levelRoot =
//             GetLevelOrigin();

//         while (cachedTables.Count < requiredTableCount)
//         {
//             cachedTables.Add(null);
//             cachedTablePrefabs.Add(null);
//         }

//         for (int index = 0;
//              index < requiredTableCount;
//              index++)
//         {
//             LevelTable requiredPrefab =
//                 levelData.GetTablePrefab(index);

//             if (requiredPrefab == null)
//             {
//                 DestroyCachedTableAt(index);

//                 Debug.LogWarning(
//                     $"Table Placements element {index} ka Prefab missing hai; " +
//                     "is extra table ko skip kiya gaya.",
//                     levelData
//                 );

//                 continue;
//             }

//             if (cachedTables[index] == null ||
//                 cachedTablePrefabs[index] != requiredPrefab)
//             {
//                 DestroyCachedTableAt(index);

//                 LevelTable instance =
//                     Instantiate(
//                         requiredPrefab,
//                         levelRoot
//                     );

//                 instance.name =
//                     requiredPrefab.name +
//                     $"_Runtime_{index + 1}";

//                 cachedTables[index] = instance;
//                 cachedTablePrefabs[index] = requiredPrefab;
//             }

//             LevelTable table =
//                 cachedTables[index];

//             Transform tableTransform =
//                 table.transform;

//             if (tableTransform.parent != levelRoot)
//             {
//                 tableTransform.SetParent(
//                     levelRoot,
//                     false
//                 );
//             }

//             tableTransform.localPosition =
//                 levelData.GetTablePositionOffset(index);

//             tableTransform.localRotation =
//                 Quaternion.Euler(
//                     levelData.GetTableRotationEuler(index)
//                 );

//             tableTransform.localScale =
//                 Vector3.Scale(
//                     requiredPrefab.transform.localScale,
//                     levelData.GetTableSizeMultiplier(index)
//                 );

//             table.gameObject.SetActive(true);

//             if (table.TowerSurfaceCollider == null)
//             {
//                 Debug.LogError(
//                     $"Table Placements element {index} ke prefab par " +
//                     "Tower Surface Collider missing hai.",
//                     table
//                 );

//                 return false;
//             }
//         }

//         currentTable = cachedTables[0];

//         return true;
//     }


//     private void DestroyCachedTableAt(int index)
//     {
//         if (index < 0 ||
//             index >= cachedTables.Count)
//         {
//             return;
//         }

//         LevelTable table =
//             cachedTables[index];

//         if (table != null)
//         {
//             if (Application.isPlaying)
//             {
//                 Destroy(table.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(table.gameObject);
//             }
//         }

//         cachedTables[index] = null;

//         if (index < cachedTablePrefabs.Count)
//         {
//             cachedTablePrefabs[index] = null;
//         }
//     }


//     private void AnimateEnabledTables()
//     {
//         if (levelData == null ||
//             cachedTables.Count == 0)
//         {
//             return;
//         }

//         runtimeTableAnimationTime +=
//             Time.fixedDeltaTime;

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             LevelTable table = cachedTables[tableIndex];
//             bool rotationEnabled =
//                 levelData.IsTableRuntimeRotationEnabled(
//                     tableIndex
//                 );
//             bool movementEnabled =
//                 levelData.IsTableRuntimeHorizontalMovementEnabled(
//                     tableIndex
//                 );

//             if (table == null ||
//                 (!rotationEnabled && !movementEnabled))
//             {
//                 continue;
//             }

//             Vector3 rotationAxis =
//                 levelData.GetTableRuntimeRotationAxis(tableIndex);
//             float rotationSpeed =
//                 levelData.GetTableRuntimeRotationSpeed(tableIndex);
//             Vector3 movementAxis =
//                 levelData.GetTableRuntimeMovementAxis(tableIndex);
//             float movementDistance =
//                 levelData.GetTableRuntimeMovementDistance(tableIndex);
//             float movementSpeed =
//                 levelData.GetTableRuntimeMovementSpeed(tableIndex);

//             bool canRotate =
//                 rotationEnabled &&
//                 rotationAxis.sqrMagnitude >= 0.0001f &&
//                 !Mathf.Approximately(rotationSpeed, 0f);

//             bool canMove =
//                 movementEnabled &&
//                 movementAxis.sqrMagnitude >= 0.0001f &&
//                 movementDistance > 0f &&
//                 movementSpeed > 0f;

//             if (!canRotate && !canMove)
//             {
//                 continue;
//             }

//             Transform tableTransform = table.transform;
//             Vector3 oldWorldPosition = tableTransform.position;
//             Quaternion oldWorldRotation = tableTransform.rotation;

//             if (canMove)
//             {
//                 float movementPhase =
//                     runtimeTableAnimationTime *
//                     movementSpeed *
//                     Mathf.PI * 2f;

//                 tableTransform.localPosition =
//                     levelData.GetTablePositionOffset(tableIndex) +
//                     movementAxis.normalized *
//                     Mathf.Sin(movementPhase) *
//                     movementDistance;
//             }

//             if (canRotate)
//             {
//                 tableTransform.localRotation =
//                     tableTransform.localRotation *
//                     Quaternion.AngleAxis(
//                         rotationSpeed * Time.fixedDeltaTime,
//                         rotationAxis.normalized
//                     );
//             }

//             Vector3 newWorldPosition = tableTransform.position;
//             Vector3 tableVelocity =
//                 (newWorldPosition - oldWorldPosition) /
//                 Mathf.Max(0.0001f, Time.fixedDeltaTime);
//             Quaternion worldRotationDelta =
//                 tableTransform.rotation *
//                 Quaternion.Inverse(oldWorldRotation);

//             for (int objectIndex = 0;
//                  objectIndex < activeObjects.Count;
//                  objectIndex++)
//             {
//                 PhysicsTowerObject instance =
//                     activeObjects[objectIndex];

//                 if (instance == null ||
//                     !instance.IsLocked ||
//                     !trackingByInstance.TryGetValue(
//                         instance,
//                         out GridTrackingEntry entry) ||
//                     entry.TableIndex != tableIndex)
//                 {
//                     continue;
//                 }

//                 Rigidbody objectBody = instance.Body;
//                 Vector3 objectPosition =
//                     objectBody != null
//                         ? objectBody.position
//                         : instance.transform.position;
//                 Quaternion objectRotation =
//                     objectBody != null
//                         ? objectBody.rotation
//                         : instance.transform.rotation;
//                 Vector3 animatedPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (objectPosition - oldWorldPosition);

//                 instance.MoveLockedWithTable(
//                     animatedPosition,
//                     worldRotationDelta * objectRotation
//                 );

//                 entry.InitialPosition =
//                     newWorldPosition +
//                     worldRotationDelta *
//                     (entry.InitialPosition - oldWorldPosition);

//                 entry.SurfaceRotation =
//                     worldRotationDelta * entry.SurfaceRotation;
//             }

//             TryReleaseTableBlocksAfterMovementCycles(
//                 tableIndex,
//                 canMove ? movementSpeed : 0f,
//                 tableVelocity
//             );
//         }
//     }


//     private void PrepareTablePhysicsReleaseTargets()
//     {
//         tablePhysicsReleaseCycleTargets.Clear();
//         releasedTablePhysicsIndices.Clear();

//         for (int tableIndex = 0;
//              tableIndex < cachedTables.Count;
//              tableIndex++)
//         {
//             if (!levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                     tableIndex))
//             {
//                 tablePhysicsReleaseCycleTargets.Add(int.MaxValue);
//                 continue;
//             }

//             int minimumCycles =
//                 Mathf.Max(
//                     1,
//                     levelData.GetTableMinimumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );
//             int maximumCycles =
//                 Mathf.Max(
//                     minimumCycles,
//                     levelData.GetTableMaximumMovementCyclesBeforeRelease(
//                         tableIndex
//                     )
//                 );

//             tablePhysicsReleaseCycleTargets.Add(
//                 Random.Range(
//                     minimumCycles,
//                     maximumCycles + 1
//                 )
//             );
//         }
//     }


//     private void TryReleaseTableBlocksAfterMovementCycles(
//         int tableIndex,
//         float movementSpeed,
//         Vector3 tableVelocity)
//     {
//         if (tableIndex < 0 ||
//             tableIndex >= tablePhysicsReleaseCycleTargets.Count ||
//             releasedTablePhysicsIndices.Contains(tableIndex) ||
//             !levelData.ShouldReleaseTableBlocksAfterMovementCycles(
//                 tableIndex) ||
//             movementSpeed <= 0f)
//         {
//             return;
//         }

//         float completedCycles =
//             runtimeTableAnimationTime * movementSpeed;

//         if (completedCycles <
//             tablePhysicsReleaseCycleTargets[tableIndex])
//         {
//             return;
//         }

//         releasedTablePhysicsIndices.Add(tableIndex);

//         float velocityMultiplier =
//             levelData.GetTableMovementReleaseVelocityMultiplier(
//                 tableIndex
//             );

//         bool wasPropagatingMirroredActivation =
//             isPropagatingMirroredActivation;

//         isPropagatingMirroredActivation = true;

//         for (int objectIndex = activeObjects.Count - 1;
//              objectIndex >= 0;
//              objectIndex--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[objectIndex];

//             if (instance == null ||
//                 !instance.IsLocked ||
//                 !trackingByInstance.TryGetValue(
//                     instance,
//                     out GridTrackingEntry entry) ||
//                 entry.TableIndex != tableIndex)
//             {
//                 continue;
//             }

//             Rigidbody body = instance.Body;
//             float bodyMass =
//                 body != null
//                     ? Mathf.Max(0.01f, body.mass)
//                     : 1f;

//             Vector3 naturalVariation =
//                 new Vector3(
//                     Random.Range(-0.08f, 0.08f),
//                     Random.Range(0.015f, 0.07f),
//                     Random.Range(-0.08f, 0.08f)
//                 );

//             Vector3 inheritedImpulse =
//                 (tableVelocity * velocityMultiplier +
//                  naturalVariation) *
//                 bodyMass;

//             Vector3 naturalTorque =
//                 Random.onUnitSphere *
//                 Random.Range(0.035f, 0.12f) *
//                 bodyMass;

//             instance.ActivatePhysics(
//                 inheritedImpulse,
//                 naturalTorque
//             );
//         }

//         isPropagatingMirroredActivation =
//             wasPropagatingMirroredActivation;
//     }


//     private void HandleObjectActivated(
//         PhysicsTowerObject source,
//         Vector3 sourceImpulse)
//     {
//         if (source == null ||
//             levelData == null ||
//             !linkMirroredDepthLayerPhysics ||
//             levelData.GridDepth <= 1 ||
//             isPropagatingMirroredActivation)
//         {
//             return;
//         }

//         Vector3Int sourceCoordinate =
//             source.GridCoordinate;

//         if (!trackingByInstance.TryGetValue(
//                 source,
//                 out GridTrackingEntry sourceTracking))
//         {
//             return;
//         }

//         int sourceTableIndex =
//             sourceTracking.TableIndex;

//         int activationRevision =
//             levelGenerationRevision;

//         for (int z = 0;
//              z < levelData.GridDepth;
//              z++)
//         {
//             if (z == sourceCoordinate.z ||
//                 !TryFindDepthLinkedObject(
//                     source,
//                     sourceCoordinate,
//                     sourceTableIndex,
//                     z,
//                     out PhysicsTowerObject mirroredObject,
//                     out Vector3Int mirroredCoordinate))
//             {
//                 continue;
//             }

//             StartCoroutine(
//                 ActivateMirroredObjectNaturally(
//                     mirroredObject,
//                     mirroredCoordinate,
//                     sourceTableIndex,
//                     sourceImpulse,
//                     activationRevision
//                 )
//             );
//         }
//     }


//     private bool TryFindDepthLinkedObject(
//         PhysicsTowerObject source,
//         Vector3Int sourceCoordinate,
//         int tableIndex,
//         int targetDepth,
//         out PhysicsTowerObject linkedObject,
//         out Vector3Int linkedCoordinate)
//     {
//         linkedObject = null;
//         linkedCoordinate = new Vector3Int(
//             sourceCoordinate.x,
//             sourceCoordinate.y,
//             targetDepth
//         );

//         if (TryGetLockedDepthObject(
//                 linkedCoordinate,
//                 tableIndex,
//                 source,
//                 out linkedObject))
//         {
//             return true;
//         }

//         /*
//          * Manually-authored layers can have different silhouettes. In that
//          * case there may be no block at the exact X/Y coordinate behind the
//          * hit. Find the nearest locked block in a small projected area so the
//          * back structure still receives a believable local impact rather
//          * than remaining completely static.
//          */
//         const int searchRadius = 2;
//         float bestScore = float.PositiveInfinity;

//         for (int yOffset = -searchRadius;
//              yOffset <= searchRadius;
//              yOffset++)
//         {
//             for (int xOffset = -searchRadius;
//                  xOffset <= searchRadius;
//                  xOffset++)
//             {
//                 if (xOffset == 0 && yOffset == 0)
//                 {
//                     continue;
//                 }

//                 Vector3Int candidateCoordinate =
//                     new Vector3Int(
//                         sourceCoordinate.x + xOffset,
//                         sourceCoordinate.y + yOffset,
//                         targetDepth
//                     );

//                 if (!TryGetLockedDepthObject(
//                         candidateCoordinate,
//                         tableIndex,
//                         source,
//                         out PhysicsTowerObject candidate))
//                 {
//                     continue;
//                 }

//                 float score =
//                     xOffset * xOffset +
//                     yOffset * yOffset * 1.5f;

//                 if (score >= bestScore)
//                 {
//                     continue;
//                 }

//                 bestScore = score;
//                 linkedObject = candidate;
//                 linkedCoordinate = candidateCoordinate;
//             }
//         }

//         return linkedObject != null;
//     }


//     private bool TryGetLockedDepthObject(
//         Vector3Int coordinate,
//         int tableIndex,
//         PhysicsTowerObject source,
//         out PhysicsTowerObject candidate)
//     {
//         return occupancyByCoordinate.TryGetValue(
//                    (tableIndex, coordinate),
//                    out candidate) &&
//                candidate != null &&
//                candidate != source &&
//                candidate.IsLocked;
//     }


//     private IEnumerator ActivateMirroredObjectNaturally(
//         PhysicsTowerObject mirroredObject,
//         Vector3Int mirroredCoordinate,
//         int tableIndex,
//         Vector3 sourceImpulse,
//         int activationRevision)
//     {
//         float minimumDelay =
//             Mathf.Max(0f, mirroredActivationDelayRange.x);

//         float maximumDelay =
//             Mathf.Max(minimumDelay, mirroredActivationDelayRange.y);

//         float delay =
//             Random.Range(minimumDelay, maximumDelay);

//         if (delay > 0f)
//         {
//             yield return new WaitForSeconds(delay);
//         }

//         if (activationRevision != levelGenerationRevision ||
//             mirroredObject == null ||
//             !mirroredObject.IsLocked ||
//             !occupancyByCoordinate.TryGetValue(
//                 (tableIndex, mirroredCoordinate),
//                 out PhysicsTowerObject currentObject) ||
//             currentObject != mirroredObject)
//         {
//             yield break;
//         }

//         float minimumStrength =
//             Mathf.Max(0f, mirroredImpulseVariation.x);

//         float maximumStrength =
//             Mathf.Max(minimumStrength, mirroredImpulseVariation.y);

//         float randomStrength =
//             Random.Range(minimumStrength, maximumStrength);

//         Vector3 randomAngles =
//             new Vector3(
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 ),
//                 Random.Range(
//                     -mirroredDirectionVariationDegrees,
//                     mirroredDirectionVariationDegrees
//                 )
//             );

//         Vector3 variedImpulse =
//             Quaternion.Euler(randomAngles) *
//             sourceImpulse *
//             mirroredLayerImpulseMultiplier *
//             randomStrength;

//         Vector3 randomTorque =
//             sourceImpulse.sqrMagnitude > 0.0001f
//                 ? Random.onUnitSphere *
//                   sourceImpulse.magnitude *
//                   mirroredRandomTorqueMultiplier *
//                   Random.Range(0.65f, 1.25f)
//                 : Vector3.zero;

//         isPropagatingMirroredActivation = true;

//         try
//         {
//             mirroredObject.ActivatePhysics(
//                 variedImpulse,
//                 randomTorque
//             );
//         }
//         finally
//         {
//             isPropagatingMirroredActivation = false;
//         }
//     }


//     private void HandleObjectCleared(
//         PhysicsTowerObject target)
//     {
//         if (target == null)
//         {
//             return;
//         }


//         target.Cleared -=
//             HandleObjectCleared;

//         target.Activated -=
//             HandleObjectActivated;


//         activeObjects.Remove(
//             target
//         );

//         trackingByInstance.Remove(
//             target
//         );

//         /*
//          * Multi-cell footprint wale piece ke liye occupancyByCoordinate
//          * mein uske SAARE covered cells target ko point karte hain
//          * (sirf anchor nahi) — is liye scan karke sab remove karte hain,
//          * warna stale entries agle level mein reused instance ko
//          * galat tarah "still standing" dikha sakti hain.
//          */
//         List<(int tableIndex, Vector3Int coordinate)> coordinatesToRemove = null;

//         foreach (KeyValuePair<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject> entry
//                  in occupancyByCoordinate)
//         {
//             if (entry.Value != target)
//             {
//                 continue;
//             }

//             if (coordinatesToRemove == null)
//             {
//                 coordinatesToRemove =
//                     new List<(int, Vector3Int)>();
//             }

//             coordinatesToRemove.Add(entry.Key);
//         }

//         if (coordinatesToRemove != null)
//         {
//             foreach ((int tableIndex, Vector3Int coordinate) key
//                      in coordinatesToRemove)
//             {
//                 occupancyByCoordinate.Remove(key);
//             }
//         }


//         if (target.CountsAsTarget)
//         {
//             remainingTargets =
//                 Mathf.Max(
//                     0,
//                     remainingTargets - 1
//                 );
//         }


//         objectPool.Release(
//             target
//         );


//         if (levelGenerated &&
//             remainingTargets == 0)
//         {
//             CompleteLevel();
//         }
//     }


//     private void CompleteLevel()
//     {
//         if (!levelGenerated)
//         {
//             return;
//         }

//         levelGenerated = false;

//         Debug.Log(
//             $"GRID LEVEL {levelData.LevelNumber} COMPLETE",
//             this
//         );

//         // Existing UnityEvent remains available for any old inspector hooks.
//         onLevelComplete?.Invoke();

//         // Dedicated UI controllers listen to this event.
//         LevelCompleted?.Invoke(levelData);
//     }


//     public void LoadNextLevel()
//     {
//         if (isLoadingLevel)
//         {
//             return;
//         }

//         int nextIndex =
//             FindNextLevelIndex(
//                 currentLevelIndex + 1
//             );

//         if (nextIndex < 0)
//         {
//             Debug.LogWarning(
//                 "Next Addressable level configured nahi hai. Level " +
//                 "Creator se next level SAVE karein.",
//                 this
//             );

//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(nextIndex)
//         );
//     }

//     public void RestartCurrentLevel()
//     {
//         if (isLoadingLevel ||
//             levelData == null)
//         {
//             return;
//         }

//         DestroyActiveCannonBalls();
//         GenerateLevel();
//     }


//     private IEnumerator LoadAddressableLevelRoutine(
//         int levelIndex)
//     {
//         if (isLoadingLevel ||
//             levelDatabase == null)
//         {
//             yield break;
//         }

//         string address =
//             levelDatabase.GetAddress(levelIndex);

//         if (string.IsNullOrWhiteSpace(address))
//         {
//             Debug.LogError(
//                 $"Level index {levelIndex} ka Addressable address missing hai.",
//                 this
//             );
//             yield break;
//         }

//         isLoadingLevel = true;

//         AsyncOperationHandle<GridLevelData> newHandle =
//             Addressables.LoadAssetAsync<GridLevelData>(address);

//         pendingLevelHandle = newHandle;
//         hasPendingLevelHandle = true;

//         yield return newHandle;

//         if (newHandle.Status != AsyncOperationStatus.Succeeded ||
//             newHandle.Result == null)
//         {
//             Debug.LogError(
//                 $"Addressable level load failed: {address}",
//                 this
//             );

//             if (newHandle.IsValid())
//             {
//                 Addressables.Release(newHandle);
//             }

//             hasPendingLevelHandle = false;
//             isLoadingLevel = false;
//             yield break;
//         }

//         /*
//          * Pehle purane instantiated objects destroy hote hain. Ek frame
//          * baad purana Addressables handle release hota hai, isliye bundle
//          * unload ke waqt koi live prefab/material instance nahi rehta.
//          */
//         PrepareForLevelAssetRelease();
//         yield return null;
//         ReleaseActiveLevelHandle();

//         activeLevelHandle = newHandle;
//         hasActiveLevelHandle = true;
//         hasPendingLevelHandle = false;
//         currentLevelIndex = levelIndex;
//         levelData = newHandle.Result;
//         isLoadingLevel = false;

//         SaveCurrentLevelProgress(
//             levelData.LevelNumber
//         );

//         GenerateLevel();
//     }


//     public void LoadLevelByNumber(int levelNumber)
//     {
//         if (levelDatabase == null)
//         {
//             Debug.LogError(
//                 "LoadLevelByNumber: GridLevelDatabase missing hai.",
//                 this
//             );
//             return;
//         }

//         if (levelNumber >
//             levelDatabase.MaximumPlayableLevelNumber)
//         {
//             Debug.LogWarning(
//                 $"Level {levelNumber} playable range se bahar hai. " +
//                 $"Last playable level " +
//                 $"{levelDatabase.MaximumPlayableLevelNumber} hai.",
//                 this
//             );
//             return;
//         }

//         int levelIndex =
//             levelDatabase.FindIndexByLevelNumber(levelNumber);

//         if (levelIndex < 0)
//         {
//             Debug.LogWarning(
//                 $"Addressable Level {levelNumber} catalog mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         StartCoroutine(
//             LoadAddressableLevelRoutine(levelIndex)
//         );
//     }


//     private void SelectStartingLevel()
//     {
//         if (levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             currentLevelIndex = 0;
//             return;
//         }

//         if (PlayerPrefs.HasKey(SavedLevelNumberKey))
//         {
//             int savedLevelNumber =
//                 PlayerPrefs.GetInt(
//                     SavedLevelNumberKey,
//                     1
//                 );

//             int savedLevelIndex =
//                 levelDatabase.FindIndexByLevelNumber(
//                     savedLevelNumber
//                 );

//             if (savedLevelIndex >= 0 &&
//                 FindNextLevelIndex(savedLevelIndex) ==
//                 savedLevelIndex)
//             {
//                 currentLevelIndex = savedLevelIndex;
//                 return;
//             }
//         }

//         if (startFromLevelOne)
//         {
//             int firstLevelIndex =
//                 FindNextLevelIndex(0);

//             if (firstLevelIndex >= 0)
//             {
//                 currentLevelIndex = firstLevelIndex;
//             }

//             return;
//         }

//         int configuredIndex =
//             levelData != null
//                 ? levelDatabase.FindIndexByLevelNumber(
//                     levelData.LevelNumber
//                 )
//                 : -1;

//         currentLevelIndex =
//             configuredIndex >= 0
//                 ? configuredIndex
//                 : Mathf.Max(0, FindNextLevelIndex(0));

//     }


//     private static bool IsLiveWorkingLevelPreviewEnabled()
//     {
// #if UNITY_EDITOR
//         return UnityEditor.SessionState.GetBool(
//             "RoyalSmash.LiveWorkingLevelPreview",
//             false
//         );
// #else
//         return false;
// #endif
//     }


//     private static bool ShouldStartLiveWorkingLevelPreview()
//     {
// #if UNITY_EDITOR
//         const string previewSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreview";

//         const string previewLaunchSessionKey =
//             "RoyalSmash.LiveWorkingLevelPreviewLaunch";

//         bool previewEnabled =
//             UnityEditor.SessionState.GetBool(
//                 previewSessionKey,
//                 false
//             );

//         bool launchRequested =
//             UnityEditor.SessionState.GetBool(
//                 previewLaunchSessionKey,
//                 false
//             );

//         UnityEditor.SessionState.SetBool(
//             previewLaunchSessionKey,
//             false
//         );

//         if (!launchRequested)
//         {
//             /*
//              * Toolbar se Play karne par pichli editor preview session
//              * normal game startup ko bypass nahi kar sakti.
//              */
//             UnityEditor.SessionState.SetBool(
//                 previewSessionKey,
//                 false
//             );
//         }

//         return previewEnabled && launchRequested;
// #else
//         return false;
// #endif
//     }


//     private int FindNextLevelIndex(
//         int startingIndex)
//     {
//         if (levelDatabase == null)
//         {
//             return -1;
//         }

//         for (int i = Mathf.Max(0, startingIndex);
//              i < levelDatabase.Count;
//              i++)
//         {
//             int levelNumber =
//                 levelDatabase.GetLevelNumber(i);

//             if (levelNumber >
//                 levelDatabase.MaximumPlayableLevelNumber)
//             {
//                 break;
//             }

//             if (levelNumber > 0 &&
//                 !string.IsNullOrWhiteSpace(
//                     levelDatabase.GetAddress(i)))
//             {
//                 return i;
//             }
//         }

//         return -1;
//     }


//     private void EnsureMainMenuUI()
//     {
//         if (mainMenuPanel == null)
//         {
//             RectTransform[] sceneRects =
//                 FindObjectsByType<RectTransform>(
//                     FindObjectsInactive.Include,
//                     FindObjectsSortMode.None
//                 );

//             foreach (RectTransform sceneRect in sceneRects)
//             {
//                 if (sceneRect != null &&
//                     sceneRect.name.Equals(
//                         "Main menu Panel",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ))
//                 {
//                     mainMenuPanel = sceneRect.gameObject;
//                     break;
//                 }
//             }
//         }

//         if (mainMenuPanel == null)
//         {
//             Debug.LogWarning(
//                 "Main menu Panel scene mein nahi mila.",
//                 this
//             );
//             return;
//         }

//         Button[] menuButtons =
//             mainMenuPanel.GetComponentsInChildren<Button>(true);

//         foreach (Button menuButton in menuButtons)
//         {
//             if (menuButton == null)
//             {
//                 continue;
//             }

//             string buttonName =
//                 menuButton.name.ToLowerInvariant();

//             if (resetProgressButton == null &&
//                 buttonName.Contains("reset"))
//             {
//                 resetProgressButton = menuButton;
//             }
//         }

//         if (currentLevelText == null)
//         {
//             TMP_Text[] menuTexts =
//                 mainMenuPanel.GetComponentsInChildren<TMP_Text>(true);

//             foreach (TMP_Text menuText in menuTexts)
//             {
//                 if (menuText != null &&
//                     menuText.text.IndexOf(
//                         "level number",
//                         System.StringComparison.OrdinalIgnoreCase
//                     ) >= 0)
//                 {
//                     currentLevelText = menuText;
//                     break;
//                 }
//             }
//         }

//         if (resetProgressButton != null)
//         {
//             resetProgressButton.onClick.RemoveListener(
//                 ResetProgressFromMainMenu
//             );
//             resetProgressButton.onClick.AddListener(
//                 ResetProgressFromMainMenu
//             );
//         }
//     }


//     private void SetGameplayWorldVisible(bool visible)
//     {
//         if (gameplayPanel != null)
//         {
//             gameplayPanel.SetActive(visible);
//         }

//         if (levelOrigin != null &&
//             levelOrigin != transform)
//         {
//             levelOrigin.gameObject.SetActive(visible);
//         }

//         if (runtimeObjectsRoot != null)
//         {
//             runtimeObjectsRoot.gameObject.SetActive(visible);
//         }

//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (cannonController != null)
//         {
//             cannonController.SetGameplayActive(visible);
//         }

//         if (gameplayRestartButton == null)
//         {
//             GameRestartController restartController =
//                 FindFirstObjectByType<GameRestartController>(
//                     FindObjectsInactive.Include
//                 );

//             if (restartController != null)
//             {
//                 gameplayRestartButton =
//                     restartController.gameObject;
//             }
//         }

//         if (gameplayRestartButton != null)
//         {
//             gameplayRestartButton.SetActive(visible);
//         }

//         if (!visible)
//         {
//             foreach (LevelTable table in cachedTables)
//             {
//                 if (table != null)
//                 {
//                     table.gameObject.SetActive(false);
//                 }
//             }

//             // Level Complete UI is now owned by LevelCompleteUIController.
//         }
//     }


//     private void SaveCurrentLevelProgress(int levelNumber)
//     {
//         if (levelNumber <= 0 ||
//             IsLiveWorkingLevelPreviewEnabled())
//         {
//             return;
//         }

//         PlayerPrefs.SetInt(
//             SavedLevelNumberKey,
//             levelNumber
//         );
//         PlayerPrefs.Save();
//         UpdateCurrentLevelText();
//     }


//     private void UpdateCurrentLevelText()
//     {
//         if (currentLevelText == null ||
//             levelDatabase == null ||
//             levelDatabase.Count == 0)
//         {
//             return;
//         }

//         int safeIndex =
//             Mathf.Clamp(
//                 currentLevelIndex,
//                 0,
//                 levelDatabase.Count - 1
//             );

//         int levelNumber =
//             levelDatabase.GetLevelNumber(safeIndex);

//         currentLevelText.text =
//             $"{Mathf.Max(1, levelNumber)}";
//     }


//     private void PrepareForLevelAssetRelease()
//     {
//         ClearCurrentLevel();
//         DestroyActiveCannonBalls();

//         if (objectPool != null)
//         {
//             objectPool.ClearAll();
//         }

//         for (int index = cachedTables.Count - 1;
//              index >= 0;
//              index--)
//         {
//             DestroyCachedTableAt(index);
//         }

//         cachedTables.Clear();
//         cachedTablePrefabs.Clear();
//         currentTable = null;
//     }


//     private static void DestroyActiveCannonBalls()
//     {
//         CannonBallMarker[] cannonBalls =
//             FindObjectsByType<CannonBallMarker>(
//                 FindObjectsInactive.Include,
//                 FindObjectsSortMode.None
//             );

//         foreach (CannonBallMarker cannonBall in cannonBalls)
//         {
//             if (cannonBall == null)
//             {
//                 continue;
//             }

//             if (Application.isPlaying)
//             {
//                 Destroy(cannonBall.gameObject);
//             }
//             else
//             {
//                 DestroyImmediate(cannonBall.gameObject);
//             }
//         }
//     }


//     private void ReleaseActiveLevelHandle()
//     {
//         if (!hasActiveLevelHandle)
//         {
//             return;
//         }

//         if (activeLevelHandle.IsValid())
//         {
//             Addressables.Release(activeLevelHandle);
//         }

//         hasActiveLevelHandle = false;
//     }


//     public void ClearCurrentLevel()
//     {
//         levelGenerationRevision++;

//         levelGenerated =
//             false;


//         for (int i =
//                  activeObjects.Count - 1;
//              i >= 0;
//              i--)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance == null)
//             {
//                 continue;
//             }


//             instance.Cleared -=
//                 HandleObjectCleared;

//             instance.Activated -=
//                 HandleObjectActivated;


//             if (objectPool != null)
//             {
//                 objectPool.Release(
//                     instance
//                 );
//             }
//         }


//         activeObjects.Clear();

//         occupancyByCoordinate.Clear();

//         trackingByInstance.Clear();

//         gridBaseRowByTable.Clear();

//         /*
//          * Fit cache CellSize-specific hai — agar LoadLevel() se
//          * different GridLevelData aa jaye (alag CellSize) to purani
//          * cached scale ghalat hogi.
//          */
//         fitCache.Clear();


//         remainingTargets =
//             0;


//         foreach (LevelTable table in cachedTables)
//         {
//             if (table != null)
//             {
//                 table.gameObject.SetActive(false);
//             }
//         }


//         currentTable =
//             null;
//     }


//     private Transform GetLevelOrigin()
//     {
//         return
//             levelOrigin != null
//                 ? levelOrigin
//                 : transform;
//     }


//     private bool ValidateReferences()
//     {
//         if (levelData == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (!levelData.HasAnyValidTableGrid)
//         {
//             Debug.LogError(
//                 "Manual GridLevelData mein koi painted cell nahi hai. " +
//                 "Level Creator mein grid allocate karke shapes paint karein.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.BlockPalette == null ||
//             levelData.BlockPalette.Count == 0)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein Block Palette empty hai.",
//                 levelData
//             );

//             return false;
//         }

//         bool hasValidPaletteEntry =
//             false;

//         foreach (PhysicsObjectDefinition entry
//                  in levelData.BlockPalette)
//         {
//             if (entry != null &&
//                 entry.Prefab != null)
//             {
//                 hasValidPaletteEntry =
//                     true;

//                 break;
//             }
//         }

//         if (!hasValidPaletteEntry)
//         {
//             Debug.LogError(
//                 "Grid Level Data ki Block Palette mein koi valid " +
//                 "Prefab wali entry nahi hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (levelData.TableCount < 1 ||
//             levelData.GetTablePrefab(0) == null)
//         {
//             Debug.LogError(
//                 "Grid Level Data mein PRIMARY Table Prefab missing hai.",
//                 levelData
//             );

//             return false;
//         }


//         if (objectPool == null)
//         {
//             Debug.LogError(
//                 "Physics Object Pool missing hai.",
//                 this
//             );

//             return false;
//         }


//         if (runtimeObjectsRoot == null)
//         {
//             Debug.LogError(
//                 "Runtime Objects Root missing hai.",
//                 this
//             );

//             return false;
//         }


//         Vector3 runtimeScale =
//             runtimeObjectsRoot.lossyScale;


//         if (Mathf.Abs(runtimeScale.x - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.y - 1f) > 0.01f ||
//             Mathf.Abs(runtimeScale.z - 1f) > 0.01f)
//         {
//             Debug.LogWarning(
//                 "RuntimeObjects ki world Scale ideally 1,1,1 honi chahiye.",
//                 runtimeObjectsRoot
//             );
//         }


//         return true;
//     }


//     private void OnDestroy()
//     {
//         for (int i = 0;
//              i < activeObjects.Count;
//              i++)
//         {
//             PhysicsTowerObject instance =
//                 activeObjects[i];


//             if (instance != null)
//             {
//                 instance.Cleared -=
//                     HandleObjectCleared;

//                 instance.Activated -=
//                     HandleObjectActivated;
//             }
//         }

//         ReleaseActiveLevelHandle();

//         if (hasPendingLevelHandle &&
//             pendingLevelHandle.IsValid())
//         {
//             Addressables.Release(pendingLevelHandle);
//         }

//         hasPendingLevelHandle = false;
//     }
// }



using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public sealed class LevelRuntimeController : MonoBehaviour
{
    private const string SavedLevelNumberKey =
        "RoyalSmash.CurrentLevelNumber";

    public event System.Action<GridLevelData> LevelGenerated;
    public event System.Action<GridLevelData> LevelCompleted;
    public event System.Action LevelLoadingStarted;






// [Header("Level Progress")]
// [SerializeField]
// private LevelMilestoneProgressBar milestoneProgressBar;









    [Header("Level")]

    [SerializeField]
    private GridLevelData levelData;

    [Tooltip(
        "Level Creator ke banaye tamam levels ki central database. " +
        "Runtime mein individual levels manually assign nahi karne."
    )]
    [SerializeField]
    private GridLevelDatabase levelDatabase;

    [SerializeField]
    private bool startFromLevelOne = true;


    [Header("Main Menu")]

    [SerializeField]
    private bool showMainMenuOnStartup = true;

    [SerializeField]
    private GameObject mainMenuPanel;

    [Tooltip(
        "Gameplay UI panel. Main Menu par hidden rahega aur Play press karte hi " +
        "gameplay world ke sath active ho jayega."
    )]
    [SerializeField]
    private GameObject gameplayPanel;

    [SerializeField]
    private Button resetProgressButton;

    [SerializeField]
    private TMP_Text currentLevelText;

    [SerializeField]
    private TMP_Text mainMenuLevelText;


    [Header("Level Entrance Animation")]

    [Tooltip(
        "Enabled ho to blocks instant appear hone ke bajaye bottom se " +
        "staggered rise-and-pop animation ke sath reveal honge."
    )]
    [SerializeField]
    private bool animateLevelEntrance = true;

    [SerializeField, Min(0f)]
    private float levelEntranceInitialDelay = 0.02f;

    [SerializeField, Min(0.01f)]
    private float blockEntranceDuration = 0.14f;

    [SerializeField, Min(0f)]
    private float blockEntranceStagger = 0.007f;

    [Tooltip("Block apni final jagah se kitna neeche se rise karega.")]
    [SerializeField, Min(0f)]
    private float blockEntranceRiseDistance = 0.45f;

    [SerializeField, Range(0f, 1f)]
    private float blockEntranceStartScale = 0.05f;


    [Header("References")]

    [SerializeField]
    private PhysicsObjectPool objectPool;

    [SerializeField]
    private Transform levelOrigin;

    [SerializeField]
    private Transform runtimeObjectsRoot;


    [Header("Support Chain")]

    [Tooltip(
        "Support khone ke kitne FixedUpdate frames baad upar wala " +
        "block khud bhi dynamic ho jayega."
    )]
    [SerializeField, Range(1, 10)]
    private int unsupportedFramesRequired = 2;

    [Tooltip(
        "Half-supported piece ko missing-support side par tip karne wala " +
        "small sideways impulse."
    )]
    [SerializeField, Min(0f)]
    private float unsupportedTipImpulse = 0.18f;

    [Tooltip(
        "Exact edge-balance ko break karne wala natural rotation impulse."
    )]
    [SerializeField, Min(0f)]
    private float unsupportedTipTorque = 0.28f;


    [Header("Mirrored Depth Physics")]

    [Tooltip(
        "Kisi bhi 2+ depth-layer level mein hit block ka impact same X/Y " +
        "par maujood baqi depth-layer blocks ko bhi unlock aur push karega. " +
        "Ye gameplay link Mirror Paint authoring option se independent hai."
    )]
    [SerializeField]
    private bool linkMirroredDepthLayerPhysics = true;

    [Tooltip(
        "Front block ke impulse ka kitna hissa mirrored layers ko mile."
    )]
    [SerializeField, Range(0f, 1.5f)]
    private float mirroredLayerImpulseMultiplier = 0.45f;

    [Tooltip(
        "Mirrored depth layers exact same frame par na giren. Har linked " +
        "block ko is range mein chhota natural delay milta hai."
    )]
    [SerializeField]
    private Vector2 mirroredActivationDelayRange =
        new Vector2(0.09f, 0.22f);

    [Tooltip(
        "Mirrored impulse ki strength mein per-block random variation."
    )]
    [SerializeField]
    private Vector2 mirroredImpulseVariation =
        new Vector2(0.78f, 0.96f);

    [Tooltip(
        "Mirrored force direction mein maximum random angle variation."
    )]
    [SerializeField, Range(0f, 25f)]
    private float mirroredDirectionVariationDegrees = 4f;

    [Tooltip(
        "Linked blocks ko alag natural spin dene ke liye random torque."
    )]
    [SerializeField, Range(0f, 1f)]
    private float mirroredRandomTorqueMultiplier = 0.035f;


    [Header("Events")]

    [SerializeField]
    private UnityEvent onLevelGenerated;

    [SerializeField]
    private UnityEvent onLevelComplete;


    private sealed class DefinitionFitData
    {
        public Vector3 Scale;
        public Vector3 BoundsCenterOffset;
    }


    private sealed class LevelEntranceAnimationEntry
    {
        public Transform Target;
        public Vector3 StartPosition;
        public Vector3 FinalPosition;
        public Vector3 StartScale;
        public Vector3 FinalScale;
    }


    private sealed class GridTrackingEntry
    {
        public Vector3 InitialPosition;
        public int UnsupportedFrames;
        public int TableIndex;
        public Quaternion SurfaceRotation;
    }


    private readonly List<PhysicsTowerObject>
        activeObjects =
            new List<PhysicsTowerObject>();

    private readonly List<LevelEntranceAnimationEntry>
        levelEntranceAnimationEntries =
            new List<LevelEntranceAnimationEntry>();

    /// <summary>
    /// Keyed by (definition, span) since a bigger footprint (e.g. a
    /// 2-cell-tall piece) needs its own fit scale, distinct from the
    /// same definition used at 1x1x1.
    /// </summary>
    private readonly Dictionary<(PhysicsObjectDefinition, int spanKey), DefinitionFitData>
        fitCache =
            new Dictionary<(PhysicsObjectDefinition, int), DefinitionFitData>();

    /// <summary>
    /// What currently occupies each grid cell — used by the support
    /// chain to answer "is the cell below me still standing?".
    /// </summary>
    private readonly Dictionary<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject>
        occupancyByCoordinate =
            new Dictionary<(int, Vector3Int), PhysicsTowerObject>();

    private readonly Dictionary<PhysicsTowerObject, GridTrackingEntry>
        trackingByInstance =
            new Dictionary<PhysicsTowerObject, GridTrackingEntry>();


    private readonly List<LevelTable> cachedTables =
        new List<LevelTable>();

    private readonly List<LevelTable> cachedTablePrefabs =
        new List<LevelTable>();

    private readonly List<int> tablePhysicsReleaseCycleTargets =
        new List<int>();

    private readonly HashSet<int> releasedTablePhysicsIndices =
        new HashSet<int>();

    private LevelTable currentTable;


    private int remainingTargets;

    private bool levelGenerated;

    private bool isPropagatingMirroredActivation;

    private int levelGenerationRevision;

    private float runtimeTableAnimationTime;

    private int currentLevelIndex;

    private AsyncOperationHandle<GridLevelData>
        activeLevelHandle;

    private bool hasActiveLevelHandle;

    private AsyncOperationHandle<GridLevelData>
        pendingLevelHandle;

    private bool hasPendingLevelHandle;

    private bool isLoadingLevel;

    private Coroutine levelEntranceRoutine;

    private CannonController cannonController;

    private GameObject gameplayRestartButton;

    /// <summary>
    /// Lowest occupied grid row for the current level — the "floor"
    /// row that always counts as supported by the table, even if the
    /// designer didn't start painting at y=0.
    /// </summary>
    private readonly Dictionary<int, int> gridBaseRowByTable =
        new Dictionary<int, int>();


    public GridLevelData CurrentLevelData =>
        levelData;

    public LevelTable CurrentTable =>
        currentTable;

    public IReadOnlyList<LevelTable> CurrentTables =>
        cachedTables;

    public int RemainingTargets =>
        remainingTargets;

    public bool IsLevelGenerated =>
        levelGenerated;

    public int CurrentLevelIndex =>
        currentLevelIndex;

    public bool HasNextLevel =>
        FindNextLevelIndex(currentLevelIndex + 1) >= 0;


    private void Start()
    {
        if (ShouldStartLiveWorkingLevelPreview())
        {
            currentLevelIndex = 0;
            GenerateLevel();
            return;
        }

        SelectStartingLevel();

        if (FindNextLevelIndex(currentLevelIndex) < 0)
        {
            Debug.LogError(
                "Addressable level catalog empty hai. Level Creator se " +
                "kam az kam ek level SAVE karein.",
                this
            );
            return;
        }

        if (showMainMenuOnStartup)
        {
            ShowMainMenu();
            return;
        }

        StartCoroutine(
            LoadAddressableLevelRoutine(currentLevelIndex)
        );
    }


    public void ShowMainMenu()
    {
        EnsureMainMenuUI();
        SetGameplayWorldVisible(false);
        UpdateCurrentLevelText();

        if (mainMenuPanel != null)
        {
            UITransition.Show(mainMenuPanel);
        }
    }


    public void PlayFromMainMenu()
    {
        if (isLoadingLevel)
        {
            return;
        }

        int playableIndex =
            FindNextLevelIndex(currentLevelIndex);

        if (playableIndex < 0)
        {
            Debug.LogError(
                "Play: koi saved Addressable level configured nahi hai.",
                this
            );
            return;
        }

        currentLevelIndex = playableIndex;

        if (mainMenuPanel != null)
        {
            UITransition.Hide(mainMenuPanel);
        }

        SetGameplayWorldVisible(true);

        StartCoroutine(
            LoadAddressableLevelRoutine(currentLevelIndex)
        );
    }


    public void ResetProgressFromMainMenu()
    {
        PlayerPrefs.DeleteKey(SavedLevelNumberKey);
        PlayerPrefs.Save();

        int firstLevelIndex =
            FindNextLevelIndex(0);

        currentLevelIndex =
            firstLevelIndex >= 0
                ? firstLevelIndex
                : 0;

        UpdateCurrentLevelText();
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


        if (hasActiveLevelHandle)
        {
            PrepareForLevelAssetRelease();
            ReleaseActiveLevelHandle();
        }

        levelData = newLevelData;

        int sequenceIndex =
            levelDatabase != null
                ? levelDatabase.FindIndexByLevelNumber(
                    newLevelData.LevelNumber
                )
                : -1;

        if (sequenceIndex >= 0)
        {
            currentLevelIndex = sequenceIndex;
        }

        SaveCurrentLevelProgress(
            newLevelData.LevelNumber
        );


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

        LevelLoadingStarted?.Invoke();

        runtimeTableAnimationTime = 0f;


        objectPool.PrepareForLevel(
            levelData
        );


        if (!PrepareTable())
        {
            return;
        }

        PrepareTablePhysicsReleaseTargets();


        remainingTargets =
            0;


        SpawnGrid();


        Physics.SyncTransforms();


        if (ShouldAnimateLevelEntrance())
        {
            int animationRevision =
                levelGenerationRevision;

            levelEntranceRoutine = StartCoroutine(
                AnimateLevelEntranceRoutine(animationRevision)
            );

            return;
        }

        FinishLevelGeneration();
    }


    private bool ShouldAnimateLevelEntrance()
    {
        return
            Application.isPlaying &&
            animateLevelEntrance &&
            levelEntranceAnimationEntries.Count > 0;
    }


    private IEnumerator AnimateLevelEntranceRoutine(
        int animationRevision)
    {
        if (levelEntranceInitialDelay > 0f)
        {
            float delayElapsed = 0f;

            while (delayElapsed < levelEntranceInitialDelay)
            {
                if (animationRevision != levelGenerationRevision)
                {
                    yield break;
                }

                delayElapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        float duration =
            Mathf.Max(0.01f, blockEntranceDuration);

        float totalDuration =
            duration +
            blockEntranceStagger *
            Mathf.Max(0, levelEntranceAnimationEntries.Count - 1);

        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            if (animationRevision != levelGenerationRevision)
            {
                yield break;
            }

            for (int i = 0;
                 i < levelEntranceAnimationEntries.Count;
                 i++)
            {
                LevelEntranceAnimationEntry entry =
                    levelEntranceAnimationEntries[i];

                if (entry == null || entry.Target == null)
                {
                    continue;
                }

                float normalizedTime =
                    Mathf.Clamp01(
                        (
                            elapsed -
                            i * blockEntranceStagger
                        ) /
                        duration
                    );

                float positionProgress =
                    normalizedTime *
                    normalizedTime *
                    (3f - 2f * normalizedTime);

                float scaleProgress =
                    EaseOutBack(normalizedTime);

                entry.Target.position =
                    Vector3.LerpUnclamped(
                        entry.StartPosition,
                        entry.FinalPosition,
                        positionProgress
                    );

                entry.Target.localScale =
                    Vector3.LerpUnclamped(
                        entry.StartScale,
                        entry.FinalScale,
                        scaleProgress
                    );
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        foreach (LevelEntranceAnimationEntry entry
                 in levelEntranceAnimationEntries)
        {
            if (entry == null || entry.Target == null)
            {
                continue;
            }

            entry.Target.position = entry.FinalPosition;
            entry.Target.localScale = entry.FinalScale;
        }

        Physics.SyncTransforms();

        levelEntranceRoutine = null;
        FinishLevelGeneration();
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


    private void FinishLevelGeneration()
    {
        levelGenerated =
            true;


        Debug.Log(
            $"GRID LEVEL {levelData.LevelNumber} GENERATED\n" +
            $"Boxes: {activeObjects.Count}\n" +
            $"Targets: {remainingTargets}",
            this
        );


        LevelGenerated?.Invoke(levelData);

        onLevelGenerated?.Invoke();


        if (remainingTargets <= 0)
        {
            Debug.LogWarning(
                "Generated level mein koi target nahi mila.",
                this
            );
        }
    }


    /// <summary>
    /// Measures a definition's prefab once (identity rotation, prefab's
    /// authored scale) and derives the localScale + pivot-to-collider
    /// offset needed so its collider bounds exactly fill levelData's
    /// CellSize. Cached per-definition so mixed shapes (cube, cylinder,
    /// ...) all land cleanly on the same uniform grid without measuring
    /// on every single cell spawn.
    /// </summary>
    private bool TryGetFitData(
        PhysicsObjectDefinition definition,
        int spanX,
        int spanY,
        int spanZ,
        PieceOrientation orientation,
        out DefinitionFitData fitData)
    {
        int spanKey =
            spanX * 10000 +
            spanY * 100 +
            spanZ +
            (int)orientation * 1000000;

        (PhysicsObjectDefinition, int) cacheKey =
            (definition, spanKey);

        if (fitCache.TryGetValue(
                cacheKey,
                out fitData))
        {
            return true;
        }

        fitData = null;

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

            objectPool.Release(measurementObject);

            return false;
        }

        Vector3 measuredSize =
            bounds.size;

        Vector3 measuredCenterOffset =
            bounds.center -
            measurementObject.transform.position;

        objectPool.Release(measurementObject);

        if (measuredSize.x <= 0.0001f ||
            measuredSize.y <= 0.0001f ||
            measuredSize.z <= 0.0001f)
        {
            Debug.LogError(
                $"{definition.DisplayName}: collider size invalid hai.",
                this
            );

            return false;
        }

        Vector3 authoredScale =
            definition.Prefab.transform.localScale;

        Vector3 finalScale;

        Vector3 finalOffset;

        if (definition.AutoFitToCell)
        {
            /*
             * Span > 1 par target size sirf spanCount * cellSize nahi —
             * internal gaps bhi fill hote hain (spanX*cellSize.x +
             * (spanX-1)*gap) taake merged piece apne poore footprint
             * ke outer edges tak seamless dikhe, jaisa cells alag alag
             * (gap ke sath) hote to unka combined outer span hota.
             */
            Vector3 gridAlignedTargetSize =
                new Vector3(
                    spanX * levelData.CellSize.x +
                    (spanX - 1) * levelData.HorizontalGap,

                    spanY * levelData.CellSize.y +
                    (spanY - 1) * levelData.VerticalGap,

                    spanZ * levelData.CellSize.z +
                    (spanZ - 1) * levelData.DepthGap
                );

            /*
             * gridAlignedTargetSize world/grid axes ke liye hai.
             * Lekin fit-scale prefab ke apne UNROTATED local axes par
             * compute hoti hai — agar orientation tip kar rahi hai
             * (Lying X/Z) to local axes rotate hone ke baad world axes
             * se match nahi karte, is liye target size ko yahan
             * local axes par re-map karte hain (spawn time par
             * PieceOrientationUtility.GetRotation() isi mapping ko
             * wapas world space mein rotate kar degi).
             */
            Vector3 targetSize =
                PieceOrientationUtility.GetLocalAxisTargetSize(
                    gridAlignedTargetSize,
                    orientation
                );

            /*
             * boundsAtScale1 = measuredSize / authoredScale
             * finalScale     = targetSize / boundsAtScale1
             *                = targetSize * authoredScale / measuredSize
             *
             * Isi tarah offset bhi authored scale se scale-1 basis par
             * normalize karke, phir finalScale par re-apply hota hai —
             * taake off-center collider pivot bhi correct rahe.
             */
            finalScale =
                new Vector3(
                    targetSize.x * authoredScale.x / measuredSize.x,
                    targetSize.y * authoredScale.y / measuredSize.y,
                    targetSize.z * authoredScale.z / measuredSize.z
                );

            Vector3 offsetAtScale1 =
                new Vector3(
                    measuredCenterOffset.x / authoredScale.x,
                    measuredCenterOffset.y / authoredScale.y,
                    measuredCenterOffset.z / authoredScale.z
                );

            finalOffset =
                Vector3.Scale(
                    offsetAtScale1,
                    finalScale
                );
        }
        else
        {
            finalScale =
                authoredScale;

            finalOffset =
                measuredCenterOffset;
        }

        finalScale =
            Vector3.Scale(
                finalScale,
                definition.ManualScaleMultiplier
            );

        fitData =
            new DefinitionFitData
            {
                Scale = finalScale,
                BoundsCenterOffset = finalOffset
            };

        fitCache[cacheKey] =
            fitData;

        return true;
    }


    private void SpawnGrid()
    {
        Vector3Int occupiedMin =
            new Vector3Int(
                int.MaxValue,
                int.MaxValue,
                int.MaxValue
            );

        Vector3Int occupiedMax =
            new Vector3Int(
                int.MinValue,
                int.MinValue,
                int.MinValue
            );

        if (currentTable == null ||
            !currentTable.TryGetTowerSurface(
                out Vector3 primarySurfacePosition,
                out Quaternion primarySurfaceRotation))
        {
            Debug.LogError(
                "PRIMARY table ki tower surface calculate nahi ho saki.",
                currentTable
            );

            return;
        }

        int tableCount =
            Mathf.Max(1, cachedTables.Count);

        Vector3[] surfacePositions =
            new Vector3[tableCount];

        Quaternion[] surfaceRotations =
            new Quaternion[tableCount];

        for (int tableIndex = 0;
             tableIndex < tableCount;
             tableIndex++)
        {
            LevelTable table =
                tableIndex < cachedTables.Count
                    ? cachedTables[tableIndex]
                    : null;

            if (table != null &&
                table.TryGetTowerSurface(
                    out Vector3 tableSurfacePosition,
                    out Quaternion tableSurfaceRotation))
            {
                surfacePositions[tableIndex] = tableSurfacePosition;
                surfaceRotations[tableIndex] = tableSurfaceRotation;
            }
            else
            {
                surfacePositions[tableIndex] = primarySurfacePosition;
                surfaceRotations[tableIndex] = primarySurfaceRotation;
            }
        }

        Dictionary<int, Vector3Int> occupiedMinByTable =
            new Dictionary<int, Vector3Int>();

        Dictionary<int, Vector3Int> occupiedMaxByTable =
            new Dictionary<int, Vector3Int>();

        bool foundAnyOccupiedCell = false;

        for (int tableIndex = 0;
             tableIndex < tableCount;
             tableIndex++)
        {
            if (!levelData.TryGetOccupiedBounds(
                    tableIndex,
                    out Vector3Int tableMinimum,
                    out Vector3Int tableMaximum))
            {
                continue;
            }

            foundAnyOccupiedCell = true;
            occupiedMinByTable[tableIndex] = tableMinimum;
            occupiedMaxByTable[tableIndex] = tableMaximum;
            occupiedMin = Vector3Int.Min(occupiedMin, tableMinimum);
            occupiedMax = Vector3Int.Max(occupiedMax, tableMaximum);
        }

        if (!foundAnyOccupiedCell)
        {
            Debug.LogError(
                "Kisi bhi table ke manual grid mein occupied cells nahi hain.",
                levelData
            );

            return;
        }

        gridBaseRowByTable.Clear();

        foreach (KeyValuePair<int, Vector3Int> entry
                 in occupiedMinByTable)
        {
            gridBaseRowByTable[entry.Key] =
                entry.Value.y;
        }


        Vector3 cellSize =
            levelData.CellSize;


        float stepX =
            cellSize.x +
            levelData.HorizontalGap;


        float stepY =
            cellSize.y +
            levelData.VerticalGap;


        float stepZ =
            cellSize.z +
            levelData.DepthGap;


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
                    for (int authoredTableIndex = 0;
                         authoredTableIndex < tableCount;
                         authoredTableIndex++)
                    {
                        if (!occupiedMinByTable.ContainsKey(
                                authoredTableIndex))
                        {
                            continue;
                        }

                        GridCellData cell =
                            levelData.GetCell(
                                x,
                                y,
                                z,
                                authoredTableIndex
                            );


                    if (cell == null ||
                        !cell.Occupied)
                    {
                        continue;
                    }


                    /*
                     * Covered cells (bare footprint ka hissa) khud
                     * spawn nahi hoti — unka anchor cell hi spawn
                     * karta hai (neeche is loop mein aage aake).
                     */
                    if (cell.IsCovered)
                    {
                        continue;
                    }


                    int blockTableIndex =
                        authoredTableIndex;

                    Vector3Int tableOccupiedMin =
                        occupiedMinByTable[blockTableIndex];

                    Vector3Int tableOccupiedMax =
                        occupiedMaxByTable[blockTableIndex];

                    float occupiedCenterX =
                        (
                            tableOccupiedMin.x +
                            tableOccupiedMax.x
                        ) *
                        0.5f;

                    float occupiedCenterZ =
                        (
                            tableOccupiedMin.z +
                            tableOccupiedMax.z
                        ) *
                        0.5f;

                    Vector3 surfacePosition =
                        surfacePositions[blockTableIndex];

                    Quaternion surfaceRotation =
                        surfaceRotations[blockTableIndex];


                    int spanX =
                        Mathf.Max(1, cell.SpanX);

                    int spanY =
                        Mathf.Max(1, cell.SpanY);

                    int spanZ =
                        Mathf.Max(1, cell.SpanZ);


                    /*
                     * X:
                     * Shape automatically center.
                     *
                     * Span > 1 ho to poore footprint (x..x+spanX-1)
                     * ka center use hota hai, sirf anchor cell ka nahi.
                     */
                    float localX =
                        (
                            x -
                            occupiedCenterX
                        ) *
                        stepX +
                        (spanX - 1) *
                        stepX *
                        0.5f +
                        cell.LocalOffset.x *
                        stepX;


                    /*
                     * Y:
                     * Lowest occupied manual-grid row
                     * directly table top par start hogi.
                     *
                     * Transparent bottom margin
                     * automatically ignore hota hai.
                     */
                    float localY =
                        cellSize.y *
                        0.5f +
                        (
                            y -
                            tableOccupiedMin.y
                        ) *
                        stepY +
                        (spanY - 1) *
                        stepY *
                        0.5f +
                        cell.LocalOffset.y *
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
                        stepZ +
                        (spanZ - 1) *
                        stepZ *
                        0.5f +
                        cell.LocalOffset.z *
                        stepZ;


                    Vector3 localCellCenter =
                        new Vector3(
                            localX,
                            localY,
                            localZ
                        )
                        +
                        levelData.GridOffset;


                    PhysicsObjectDefinition definition =
                        levelData.GetPaletteEntry(
                            cell.DefinitionIndex
                        );

                    if (definition == null ||
                        definition.Prefab == null)
                    {
                        Debug.LogWarning(
                            $"Cell ({x},{y},{z}): palette entry " +
                            $"{cell.DefinitionIndex} invalid hai, skip.",
                            levelData
                        );

                        continue;
                    }

                    PieceOrientation orientation =
                        cell.Orientation;

                    if (!TryGetFitData(
                            definition,
                            spanX,
                            spanY,
                            spanZ,
                            orientation,
                            out DefinitionFitData fitData))
                    {
                        continue;
                    }


                    /*
                     * Orientation ka extra tip (Lying X/Z) surface
                     * rotation ke UPAR apply hoti hai — piece ko table
                     * ke sath sath khud bhi rotate karta hai.
                     */
                    Quaternion pieceRotation =
                        surfaceRotation *
                        PieceOrientationUtility.GetRotation(
                            orientation,
                            cell.CustomZRotation
                        ) *
                        Quaternion.Euler(
                            cell.RotationEulerOffset
                        );


                    Vector3 pieceScale =
                        Vector3.Scale(
                            fitData.Scale,
                            cell.ScaleMultiplier
                        );


                    Vector3 desiredBoundsCenter =
                        surfacePosition +
                        surfaceRotation *
                        localCellCenter;


                    /*
                     * Prefab pivot center par na bhi ho
                     * to collider bottom proper align hoga.
                     */
                    Vector3 rotatedBoundsOffset =
                        pieceRotation *
                        Vector3.Scale(
                            fitData.BoundsCenterOffset,
                            cell.ScaleMultiplier
                        );


                    Vector3 spawnPosition =
                        desiredBoundsCenter -
                        rotatedBoundsOffset;


                    PhysicsTowerObject instance =
                        objectPool.Get(
                            definition,
                            spawnPosition,
                            pieceRotation,
                            runtimeObjectsRoot
                        );


                    if (instance == null)
                    {
                        continue;
                    }


                    /*
                     * Pool.Get() prefab ki authored scale laga deta hai —
                     * yahan usay cell-fit scale se override karte hain
                     * taake mixed shapes (cube/cylinder) uniform grid
                     * mein clean fit hon.
                     */
                    instance.transform.localScale =
                        pieceScale;

                    if (Application.isPlaying &&
                        animateLevelEntrance)
                    {
                        Vector3 entranceUp =
                            surfaceRotation * Vector3.up;

                        Vector3 entranceStartPosition =
                            spawnPosition -
                            entranceUp * blockEntranceRiseDistance;

                        Vector3 entranceStartScale =
                            pieceScale * blockEntranceStartScale;

                        instance.transform.position =
                            entranceStartPosition;

                        instance.transform.localScale =
                            entranceStartScale;

                        levelEntranceAnimationEntries.Add(
                            new LevelEntranceAnimationEntry
                            {
                                Target = instance.transform,
                                StartPosition = entranceStartPosition,
                                FinalPosition = spawnPosition,
                                StartScale = entranceStartScale,
                                FinalScale = pieceScale
                            }
                        );
                    }


                    /*
                     * Pool se purani property-block state clear karo.
                     * Default visual hamesha prefab ka apna material aur
                     * texture hai; paint tint definition par optional hai.
                     */
                    instance.RestorePrefabVisual();

                    if (definition.TintWithPaintColor)
                    {
                        instance.SetVisualColor(
                            cell.Color
                        );
                    }


                    Vector3Int anchorCoordinate =
                        new Vector3Int(x, y, z);

                    instance.SetGridFootprint(
                        anchorCoordinate,
                        new Vector3Int(
                            spanX,
                            spanY,
                            spanZ
                        ),
                        cell.LocalOffset
                    );

                    instance.ConfigureBreakable(
                        cell.Breakable,
                        cell.HitsToBreak
                    );

                    /*
                     * Bare footprint ke SAARE cells is instance ko
                     * point karte hain — is se agar upar kuch spawn ho
                     * to woh footprint ke kisi bhi column par "supported"
                     * sahi detect hoga.
                     */
                    for (int fz = 0; fz < spanZ; fz++)
                    {
                        for (int fy = 0; fy < spanY; fy++)
                        {
                            for (int fx = 0; fx < spanX; fx++)
                            {
                                occupancyByCoordinate[
                                    (
                                        blockTableIndex,
                                        new Vector3Int(
                                            x + fx,
                                            y + fy,
                                            z + fz
                                        )
                                    )
                                ] = instance;
                            }
                        }
                    }

                    trackingByInstance[instance] =
                        new GridTrackingEntry
                        {
                            InitialPosition = spawnPosition,
                            UnsupportedFrames = 0,
                            TableIndex = blockTableIndex,
                            SurfaceRotation = surfaceRotation
                        };


                    instance.Cleared +=
                        HandleObjectCleared;

                    instance.Activated +=
                        HandleObjectActivated;


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
    }


    private int ResolveBlockTableIndex(int requestedIndex)
    {
        if (requestedIndex >= 0 &&
            requestedIndex < cachedTables.Count &&
            cachedTables[requestedIndex] != null)
        {
            return requestedIndex;
        }

        return 0;
    }


    private void FixedUpdate()
    {
        if (!levelGenerated)
        {
            return;
        }

        AnimateEnabledTables();

        if (activeObjects.Count == 0)
        {
            return;
        }

        /*
         * Snapshot: activeObjects FixedUpdate ke dauran
         * HandleObjectCleared se modify ho sakti hai
         * (ActivatePhysics() khud kuch remove nahi karta,
         * lekin safety ke liye copy le lete hain).
         */
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            PhysicsTowerObject instance =
                activeObjects[i];

            if (instance == null || !instance.IsLocked)
            {
                continue;
            }

            if (!trackingByInstance.TryGetValue(
                    instance,
                    out GridTrackingEntry entry))
            {
                continue;
            }

            if (IsSupported(
                    instance,
                    entry,
                    out Vector3 unsupportedLocalDirection))
            {
                entry.UnsupportedFrames = 0;
                continue;
            }

            entry.UnsupportedFrames++;

            if (entry.UnsupportedFrames >=
                unsupportedFramesRequired)
            {
                Vector3 worldFallDirection =
                    entry.SurfaceRotation *
                    unsupportedLocalDirection.normalized;

                Vector3 worldUp =
                    entry.SurfaceRotation * Vector3.up;

                Vector3 tippingTorque =
                    Vector3.Cross(
                        worldUp,
                        worldFallDirection
                    ).normalized *
                    unsupportedTipTorque *
                    Random.Range(0.9f, 1.1f);

                instance.ActivatePhysics(
                    worldFallDirection *
                    unsupportedTipImpulse *
                    Random.Range(0.9f, 1.1f),
                    tippingTorque
                );
            }
        }
    }


    /// <summary>
    /// Bottom row table par khadi hoti hai. Baqi pieces ke poore bottom
    /// footprint mein kam az kam ek LOCKED support hona chahiye. Hit hua
    /// dynamic block foran support count hona band ho jata hai, chahe woh
    /// abhi apni spawn position se zyada move na bhi hua ho.
    ///
    /// Multi-cell footprint (span > 1) wale pieces ke liye sirf
    /// "kahin bhi neeche support hai" (OR) kaafi nahi — agar ek extreme
    /// edge (min ya max column) ka support hat jaye to piece ko us taraf
    /// tip hona chahiye, chahe doosra extreme abhi bhi supported ho.
    /// Ye seam-offset pieces (0.5 Center-on-Seam) aur plain grid-aligned
    /// multi-span pieces, dono par apply hota hai.
    /// </summary>
    private bool IsSupported(
        PhysicsTowerObject instance,
        GridTrackingEntry trackingEntry,
        out Vector3 unsupportedLocalDirection)
    {
        unsupportedLocalDirection = Vector3.zero;

        if (instance == null)
        {
            unsupportedLocalDirection = Vector3.right;
            return false;
        }

        Vector3Int coordinate =
            instance.GridCoordinate;

        int baseRow =
            gridBaseRowByTable.TryGetValue(
                trackingEntry.TableIndex,
                out int tableBaseRow)
                ? tableBaseRow
                : coordinate.y;

        if (coordinate.y <= baseRow)
        {
            return true;
        }

        Vector3Int span =
            instance.GridSpan;

        Vector3 localOffset =
            instance.GridLocalOffset;

        int minimumX =
            coordinate.x +
            Mathf.FloorToInt(localOffset.x);

        int maximumX =
            coordinate.x +
            Mathf.Max(1, span.x) - 1 +
            Mathf.CeilToInt(localOffset.x);

        int minimumZ =
            coordinate.z +
            Mathf.FloorToInt(localOffset.z);

        int maximumZ =
            coordinate.z +
            Mathf.Max(1, span.z) - 1 +
            Mathf.CeilToInt(localOffset.z);

        bool seamOnX =
            !Mathf.Approximately(
                localOffset.x,
                Mathf.Round(localOffset.x)
            );

        bool seamOnZ =
            !Mathf.Approximately(
                localOffset.z,
                Mathf.Round(localOffset.z)
            );

        /*
         * Seam offset ke bina bhi agar piece ek se zyada grid column
         * span kar raha hai (e.g. horizontal 2-cell beam), to sirf
         * "kahin bhi support hai" kaafi nahi — dono extremes (min aur
         * max column) ka support alag alag check hona chahiye, warna
         * ek pillar hat jane par doosre pillar ka sahara poore beam
         * ko galat tarah "supported" dikha deta hai.
         */
        bool spansMultipleColumnsX =
            maximumX > minimumX;

        bool spansMultipleColumnsZ =
            maximumZ > minimumZ;

        bool hasAnySupport = false;
        bool minimumXSupported = false;
        bool maximumXSupported = false;
        bool minimumZSupported = false;
        bool maximumZSupported = false;

        for (int z = minimumZ; z <= maximumZ; z++)
        {
            for (int x = minimumX; x <= maximumX; x++)
            {
                Vector3Int below =
                    new Vector3Int(
                        x,
                        coordinate.y - 1,
                        z
                    );

                if (occupancyByCoordinate.TryGetValue(
                        (trackingEntry.TableIndex, below),
                        out PhysicsTowerObject supportInstance) &&
                    supportInstance != null &&
                    supportInstance != instance &&
                    supportInstance.IsLocked)
                {
                    hasAnySupport = true;
                    minimumXSupported |= x == minimumX;
                    maximumXSupported |= x == maximumX;
                    minimumZSupported |= z == minimumZ;
                    maximumZSupported |= z == maximumZ;
                }
            }
        }

        if (!hasAnySupport)
        {
            unsupportedLocalDirection =
                GetNaturalUnsupportedDirection(
                    coordinate,
                    localOffset
                );

            return false;
        }

        if ((seamOnX || spansMultipleColumnsX) &&
            (!minimumXSupported || !maximumXSupported))
        {
            if (!minimumXSupported)
            {
                unsupportedLocalDirection += Vector3.left;
            }

            if (!maximumXSupported)
            {
                unsupportedLocalDirection += Vector3.right;
            }

            if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
            {
                unsupportedLocalDirection =
                    localOffset.x >= 0f
                        ? Vector3.right
                        : Vector3.left;
            }

            return false;
        }

        if ((seamOnZ || spansMultipleColumnsZ) &&
            (!minimumZSupported || !maximumZSupported))
        {
            if (!minimumZSupported)
            {
                unsupportedLocalDirection += Vector3.back;
            }

            if (!maximumZSupported)
            {
                unsupportedLocalDirection += Vector3.forward;
            }

            if (unsupportedLocalDirection.sqrMagnitude < 0.001f)
            {
                unsupportedLocalDirection =
                    localOffset.z >= 0f
                        ? Vector3.forward
                        : Vector3.back;
            }

            return false;
        }

        return true;
    }


    private static Vector3 GetNaturalUnsupportedDirection(
        Vector3Int coordinate,
        Vector3 localOffset)
    {
        Vector3 direction =
            new Vector3(
                localOffset.x,
                0f,
                localOffset.z
            );

        if (direction.sqrMagnitude > 0.001f)
        {
            return direction.normalized;
        }

        int hash =
            coordinate.x * 73856093 ^
            coordinate.y * 19349663 ^
            coordinate.z * 83492791;

        float x =
            (hash & 1) == 0 ? -1f : 1f;

        float z =
            (hash & 2) == 0 ? -0.45f : 0.45f;

        return new Vector3(x, 0f, z).normalized;
    }


    private bool PrepareTable()
    {
        int requiredTableCount =
            levelData.TableCount;

        if (requiredTableCount < 1 ||
            levelData.GetTablePrefab(0) == null)
        {
            Debug.LogError(
                "GridLevelData mein PRIMARY Table Prefab missing hai.",
                levelData
            );

            return false;
        }

        while (cachedTables.Count > requiredTableCount)
        {
            DestroyCachedTableAt(
                cachedTables.Count - 1
            );

            cachedTables.RemoveAt(
                cachedTables.Count - 1
            );

            cachedTablePrefabs.RemoveAt(
                cachedTablePrefabs.Count - 1
            );
        }

        Transform levelRoot =
            GetLevelOrigin();

        while (cachedTables.Count < requiredTableCount)
        {
            cachedTables.Add(null);
            cachedTablePrefabs.Add(null);
        }

        for (int index = 0;
             index < requiredTableCount;
             index++)
        {
            LevelTable requiredPrefab =
                levelData.GetTablePrefab(index);

            if (requiredPrefab == null)
            {
                DestroyCachedTableAt(index);

                Debug.LogWarning(
                    $"Table Placements element {index} ka Prefab missing hai; " +
                    "is extra table ko skip kiya gaya.",
                    levelData
                );

                continue;
            }

            if (cachedTables[index] == null ||
                cachedTablePrefabs[index] != requiredPrefab)
            {
                DestroyCachedTableAt(index);

                LevelTable instance =
                    Instantiate(
                        requiredPrefab,
                        levelRoot
                    );

                instance.name =
                    requiredPrefab.name +
                    $"_Runtime_{index + 1}";

                cachedTables[index] = instance;
                cachedTablePrefabs[index] = requiredPrefab;
            }

            LevelTable table =
                cachedTables[index];

            Transform tableTransform =
                table.transform;

            if (tableTransform.parent != levelRoot)
            {
                tableTransform.SetParent(
                    levelRoot,
                    false
                );
            }

            tableTransform.localPosition =
                levelData.GetTablePositionOffset(index);

            tableTransform.localRotation =
                Quaternion.Euler(
                    levelData.GetTableRotationEuler(index)
                );

            tableTransform.localScale =
                Vector3.Scale(
                    requiredPrefab.transform.localScale,
                    levelData.GetTableSizeMultiplier(index)
                );

            table.gameObject.SetActive(true);

            if (table.TowerSurfaceCollider == null)
            {
                Debug.LogError(
                    $"Table Placements element {index} ke prefab par " +
                    "Tower Surface Collider missing hai.",
                    table
                );

                return false;
            }
        }

        currentTable = cachedTables[0];

        return true;
    }


    private void DestroyCachedTableAt(int index)
    {
        if (index < 0 ||
            index >= cachedTables.Count)
        {
            return;
        }

        LevelTable table =
            cachedTables[index];

        if (table != null)
        {
            if (Application.isPlaying)
            {
                Destroy(table.gameObject);
            }
            else
            {
                DestroyImmediate(table.gameObject);
            }
        }

        cachedTables[index] = null;

        if (index < cachedTablePrefabs.Count)
        {
            cachedTablePrefabs[index] = null;
        }
    }


    private void AnimateEnabledTables()
    {
        if (levelData == null ||
            cachedTables.Count == 0)
        {
            return;
        }

        runtimeTableAnimationTime +=
            Time.fixedDeltaTime;

        for (int tableIndex = 0;
             tableIndex < cachedTables.Count;
             tableIndex++)
        {
            LevelTable table = cachedTables[tableIndex];
            bool rotationEnabled =
                levelData.IsTableRuntimeRotationEnabled(
                    tableIndex
                );
            bool movementEnabled =
                levelData.IsTableRuntimeHorizontalMovementEnabled(
                    tableIndex
                );

            if (table == null ||
                (!rotationEnabled && !movementEnabled))
            {
                continue;
            }

            Vector3 rotationAxis =
                levelData.GetTableRuntimeRotationAxis(tableIndex);
            float rotationSpeed =
                levelData.GetTableRuntimeRotationSpeed(tableIndex);
            Vector3 movementAxis =
                levelData.GetTableRuntimeMovementAxis(tableIndex);
            float movementDistance =
                levelData.GetTableRuntimeMovementDistance(tableIndex);
            float movementSpeed =
                levelData.GetTableRuntimeMovementSpeed(tableIndex);

            bool canRotate =
                rotationEnabled &&
                rotationAxis.sqrMagnitude >= 0.0001f &&
                !Mathf.Approximately(rotationSpeed, 0f);

            bool canMove =
                movementEnabled &&
                movementAxis.sqrMagnitude >= 0.0001f &&
                movementDistance > 0f &&
                movementSpeed > 0f;

            if (!canRotate && !canMove)
            {
                continue;
            }

            Transform tableTransform = table.transform;
            Vector3 oldWorldPosition = tableTransform.position;
            Quaternion oldWorldRotation = tableTransform.rotation;

            if (canMove)
            {
                float movementPhase =
                    runtimeTableAnimationTime *
                    movementSpeed *
                    Mathf.PI * 2f;

                tableTransform.localPosition =
                    levelData.GetTablePositionOffset(tableIndex) +
                    movementAxis.normalized *
                    Mathf.Sin(movementPhase) *
                    movementDistance;
            }

            if (canRotate)
            {
                tableTransform.localRotation =
                    tableTransform.localRotation *
                    Quaternion.AngleAxis(
                        rotationSpeed * Time.fixedDeltaTime,
                        rotationAxis.normalized
                    );
            }

            Vector3 newWorldPosition = tableTransform.position;
            Vector3 tableVelocity =
                (newWorldPosition - oldWorldPosition) /
                Mathf.Max(0.0001f, Time.fixedDeltaTime);
            Quaternion worldRotationDelta =
                tableTransform.rotation *
                Quaternion.Inverse(oldWorldRotation);

            for (int objectIndex = 0;
                 objectIndex < activeObjects.Count;
                 objectIndex++)
            {
                PhysicsTowerObject instance =
                    activeObjects[objectIndex];

                if (instance == null ||
                    !instance.IsLocked ||
                    !trackingByInstance.TryGetValue(
                        instance,
                        out GridTrackingEntry entry) ||
                    entry.TableIndex != tableIndex)
                {
                    continue;
                }

                Rigidbody objectBody = instance.Body;
                Vector3 objectPosition =
                    objectBody != null
                        ? objectBody.position
                        : instance.transform.position;
                Quaternion objectRotation =
                    objectBody != null
                        ? objectBody.rotation
                        : instance.transform.rotation;
                Vector3 animatedPosition =
                    newWorldPosition +
                    worldRotationDelta *
                    (objectPosition - oldWorldPosition);

                instance.MoveLockedWithTable(
                    animatedPosition,
                    worldRotationDelta * objectRotation
                );

                entry.InitialPosition =
                    newWorldPosition +
                    worldRotationDelta *
                    (entry.InitialPosition - oldWorldPosition);

                entry.SurfaceRotation =
                    worldRotationDelta * entry.SurfaceRotation;
            }

            TryReleaseTableBlocksAfterMovementCycles(
                tableIndex,
                canMove ? movementSpeed : 0f,
                tableVelocity
            );
        }
    }


    private void PrepareTablePhysicsReleaseTargets()
    {
        tablePhysicsReleaseCycleTargets.Clear();
        releasedTablePhysicsIndices.Clear();

        for (int tableIndex = 0;
             tableIndex < cachedTables.Count;
             tableIndex++)
        {
            if (!levelData.ShouldReleaseTableBlocksAfterMovementCycles(
                    tableIndex))
            {
                tablePhysicsReleaseCycleTargets.Add(int.MaxValue);
                continue;
            }

            int minimumCycles =
                Mathf.Max(
                    1,
                    levelData.GetTableMinimumMovementCyclesBeforeRelease(
                        tableIndex
                    )
                );
            int maximumCycles =
                Mathf.Max(
                    minimumCycles,
                    levelData.GetTableMaximumMovementCyclesBeforeRelease(
                        tableIndex
                    )
                );

            tablePhysicsReleaseCycleTargets.Add(
                Random.Range(
                    minimumCycles,
                    maximumCycles + 1
                )
            );
        }
    }


    private void TryReleaseTableBlocksAfterMovementCycles(
        int tableIndex,
        float movementSpeed,
        Vector3 tableVelocity)
    {
        if (tableIndex < 0 ||
            tableIndex >= tablePhysicsReleaseCycleTargets.Count ||
            releasedTablePhysicsIndices.Contains(tableIndex) ||
            !levelData.ShouldReleaseTableBlocksAfterMovementCycles(
                tableIndex) ||
            movementSpeed <= 0f)
        {
            return;
        }

        float completedCycles =
            runtimeTableAnimationTime * movementSpeed;

        if (completedCycles <
            tablePhysicsReleaseCycleTargets[tableIndex])
        {
            return;
        }

        releasedTablePhysicsIndices.Add(tableIndex);

        float velocityMultiplier =
            levelData.GetTableMovementReleaseVelocityMultiplier(
                tableIndex
            );

        bool wasPropagatingMirroredActivation =
            isPropagatingMirroredActivation;

        isPropagatingMirroredActivation = true;

        for (int objectIndex = activeObjects.Count - 1;
             objectIndex >= 0;
             objectIndex--)
        {
            PhysicsTowerObject instance =
                activeObjects[objectIndex];

            if (instance == null ||
                !instance.IsLocked ||
                !trackingByInstance.TryGetValue(
                    instance,
                    out GridTrackingEntry entry) ||
                entry.TableIndex != tableIndex)
            {
                continue;
            }

            Rigidbody body = instance.Body;
            float bodyMass =
                body != null
                    ? Mathf.Max(0.01f, body.mass)
                    : 1f;

            Vector3 naturalVariation =
                new Vector3(
                    Random.Range(-0.08f, 0.08f),
                    Random.Range(0.015f, 0.07f),
                    Random.Range(-0.08f, 0.08f)
                );

            Vector3 inheritedImpulse =
                (tableVelocity * velocityMultiplier +
                 naturalVariation) *
                bodyMass;

            Vector3 naturalTorque =
                Random.onUnitSphere *
                Random.Range(0.035f, 0.12f) *
                bodyMass;

            instance.ActivatePhysics(
                inheritedImpulse,
                naturalTorque
            );
        }

        isPropagatingMirroredActivation =
            wasPropagatingMirroredActivation;
    }


    private void HandleObjectActivated(
        PhysicsTowerObject source,
        Vector3 sourceImpulse)
    {
        if (source == null ||
            levelData == null ||
            !linkMirroredDepthLayerPhysics ||
            levelData.GridDepth <= 1 ||
            isPropagatingMirroredActivation)
        {
            return;
        }

        Vector3Int sourceCoordinate =
            source.GridCoordinate;

        if (!trackingByInstance.TryGetValue(
                source,
                out GridTrackingEntry sourceTracking))
        {
            return;
        }

        int sourceTableIndex =
            sourceTracking.TableIndex;

        int activationRevision =
            levelGenerationRevision;

        for (int z = 0;
             z < levelData.GridDepth;
             z++)
        {
            if (z == sourceCoordinate.z ||
                !TryFindDepthLinkedObject(
                    source,
                    sourceCoordinate,
                    sourceTableIndex,
                    z,
                    out PhysicsTowerObject mirroredObject,
                    out Vector3Int mirroredCoordinate))
            {
                continue;
            }

            StartCoroutine(
                ActivateMirroredObjectNaturally(
                    mirroredObject,
                    mirroredCoordinate,
                    sourceTableIndex,
                    sourceImpulse,
                    activationRevision
                )
            );
        }
    }


    private bool TryFindDepthLinkedObject(
        PhysicsTowerObject source,
        Vector3Int sourceCoordinate,
        int tableIndex,
        int targetDepth,
        out PhysicsTowerObject linkedObject,
        out Vector3Int linkedCoordinate)
    {
        linkedObject = null;
        linkedCoordinate = new Vector3Int(
            sourceCoordinate.x,
            sourceCoordinate.y,
            targetDepth
        );

        if (TryGetLockedDepthObject(
                linkedCoordinate,
                tableIndex,
                source,
                out linkedObject))
        {
            return true;
        }

        /*
         * Manually-authored layers can have different silhouettes. In that
         * case there may be no block at the exact X/Y coordinate behind the
         * hit. Find the nearest locked block in a small projected area so the
         * back structure still receives a believable local impact rather
         * than remaining completely static.
         */
        const int searchRadius = 2;
        float bestScore = float.PositiveInfinity;

        for (int yOffset = -searchRadius;
             yOffset <= searchRadius;
             yOffset++)
        {
            for (int xOffset = -searchRadius;
                 xOffset <= searchRadius;
                 xOffset++)
            {
                if (xOffset == 0 && yOffset == 0)
                {
                    continue;
                }

                Vector3Int candidateCoordinate =
                    new Vector3Int(
                        sourceCoordinate.x + xOffset,
                        sourceCoordinate.y + yOffset,
                        targetDepth
                    );

                if (!TryGetLockedDepthObject(
                        candidateCoordinate,
                        tableIndex,
                        source,
                        out PhysicsTowerObject candidate))
                {
                    continue;
                }

                float score =
                    xOffset * xOffset +
                    yOffset * yOffset * 1.5f;

                if (score >= bestScore)
                {
                    continue;
                }

                bestScore = score;
                linkedObject = candidate;
                linkedCoordinate = candidateCoordinate;
            }
        }

        return linkedObject != null;
    }


    private bool TryGetLockedDepthObject(
        Vector3Int coordinate,
        int tableIndex,
        PhysicsTowerObject source,
        out PhysicsTowerObject candidate)
    {
        return occupancyByCoordinate.TryGetValue(
                   (tableIndex, coordinate),
                   out candidate) &&
               candidate != null &&
               candidate != source &&
               candidate.IsLocked;
    }


    private IEnumerator ActivateMirroredObjectNaturally(
        PhysicsTowerObject mirroredObject,
        Vector3Int mirroredCoordinate,
        int tableIndex,
        Vector3 sourceImpulse,
        int activationRevision)
    {
        float minimumDelay =
            Mathf.Max(0f, mirroredActivationDelayRange.x);

        float maximumDelay =
            Mathf.Max(minimumDelay, mirroredActivationDelayRange.y);

        float delay =
            Random.Range(minimumDelay, maximumDelay);

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (activationRevision != levelGenerationRevision ||
            mirroredObject == null ||
            !mirroredObject.IsLocked ||
            !occupancyByCoordinate.TryGetValue(
                (tableIndex, mirroredCoordinate),
                out PhysicsTowerObject currentObject) ||
            currentObject != mirroredObject)
        {
            yield break;
        }

        float minimumStrength =
            Mathf.Max(0f, mirroredImpulseVariation.x);

        float maximumStrength =
            Mathf.Max(minimumStrength, mirroredImpulseVariation.y);

        float randomStrength =
            Random.Range(minimumStrength, maximumStrength);

        Vector3 randomAngles =
            new Vector3(
                Random.Range(
                    -mirroredDirectionVariationDegrees,
                    mirroredDirectionVariationDegrees
                ),
                Random.Range(
                    -mirroredDirectionVariationDegrees,
                    mirroredDirectionVariationDegrees
                ),
                Random.Range(
                    -mirroredDirectionVariationDegrees,
                    mirroredDirectionVariationDegrees
                )
            );

        Vector3 variedImpulse =
            Quaternion.Euler(randomAngles) *
            sourceImpulse *
            mirroredLayerImpulseMultiplier *
            randomStrength;

        Vector3 randomTorque =
            sourceImpulse.sqrMagnitude > 0.0001f
                ? Random.onUnitSphere *
                  sourceImpulse.magnitude *
                  mirroredRandomTorqueMultiplier *
                  Random.Range(0.65f, 1.25f)
                : Vector3.zero;

        isPropagatingMirroredActivation = true;

        try
        {
            mirroredObject.ActivatePhysics(
                variedImpulse,
                randomTorque
            );
        }
        finally
        {
            isPropagatingMirroredActivation = false;
        }
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

        target.Activated -=
            HandleObjectActivated;


        activeObjects.Remove(
            target
        );

        trackingByInstance.Remove(
            target
        );

        /*
         * Multi-cell footprint wale piece ke liye occupancyByCoordinate
         * mein uske SAARE covered cells target ko point karte hain
         * (sirf anchor nahi) — is liye scan karke sab remove karte hain,
         * warna stale entries agle level mein reused instance ko
         * galat tarah "still standing" dikha sakti hain.
         */
        List<(int tableIndex, Vector3Int coordinate)> coordinatesToRemove = null;

        foreach (KeyValuePair<(int tableIndex, Vector3Int coordinate), PhysicsTowerObject> entry
                 in occupancyByCoordinate)
        {
            if (entry.Value != target)
            {
                continue;
            }

            if (coordinatesToRemove == null)
            {
                coordinatesToRemove =
                    new List<(int, Vector3Int)>();
            }

            coordinatesToRemove.Add(entry.Key);
        }

        if (coordinatesToRemove != null)
        {
            foreach ((int tableIndex, Vector3Int coordinate) key
                     in coordinatesToRemove)
            {
                occupancyByCoordinate.Remove(key);
            }
        }


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

        levelGenerated = false;

        Debug.Log(
            $"GRID LEVEL {levelData.LevelNumber} COMPLETE",
            this
        );

        // Existing UnityEvent remains available for any old inspector hooks.
        onLevelComplete?.Invoke();

        // Dedicated UI controllers listen to this event.
        LevelCompleted?.Invoke(levelData);
    }


    public void LoadNextLevel()
    {
        if (isLoadingLevel)
        {
            return;
        }

        int nextIndex =
            FindNextLevelIndex(
                currentLevelIndex + 1
            );

        if (nextIndex < 0)
        {
            Debug.LogWarning(
                "Next Addressable level configured nahi hai. Level " +
                "Creator se next level SAVE karein.",
                this
            );

            return;
        }

        StartCoroutine(
            LoadAddressableLevelRoutine(nextIndex)
        );
    }

    public void RestartCurrentLevel()
    {
        if (isLoadingLevel ||
            levelData == null)
        {
            return;
        }

        DestroyActiveCannonBalls();
        GenerateLevel();
    }


    private IEnumerator LoadAddressableLevelRoutine(
        int levelIndex)
    {
        if (isLoadingLevel ||
            levelDatabase == null)
        {
            yield break;
        }

        string address =
            levelDatabase.GetAddress(levelIndex);

        if (string.IsNullOrWhiteSpace(address))
        {
            Debug.LogError(
                $"Level index {levelIndex} ka Addressable address missing hai.",
                this
            );
            yield break;
        }

        isLoadingLevel = true;

        LevelLoadingStarted?.Invoke();

        AsyncOperationHandle<GridLevelData> newHandle =
            Addressables.LoadAssetAsync<GridLevelData>(address);

        pendingLevelHandle = newHandle;
        hasPendingLevelHandle = true;

        yield return newHandle;

        if (newHandle.Status != AsyncOperationStatus.Succeeded ||
            newHandle.Result == null)
        {
            Debug.LogError(
                $"Addressable level load failed: {address}",
                this
            );

            if (newHandle.IsValid())
            {
                Addressables.Release(newHandle);
            }

            hasPendingLevelHandle = false;
            isLoadingLevel = false;
            yield break;
        }

        /*
         * Pehle purane instantiated objects destroy hote hain. Ek frame
         * baad purana Addressables handle release hota hai, isliye bundle
         * unload ke waqt koi live prefab/material instance nahi rehta.
         */
        PrepareForLevelAssetRelease();
        yield return null;
        ReleaseActiveLevelHandle();

        activeLevelHandle = newHandle;
        hasActiveLevelHandle = true;
        hasPendingLevelHandle = false;
        currentLevelIndex = levelIndex;
        levelData = newHandle.Result;
        isLoadingLevel = false;

        SaveCurrentLevelProgress(
            levelData.LevelNumber
        );

        GenerateLevel();
    }


    public void LoadLevelByNumber(int levelNumber)
    {
        if (levelDatabase == null)
        {
            Debug.LogError(
                "LoadLevelByNumber: GridLevelDatabase missing hai.",
                this
            );
            return;
        }

        if (levelNumber >
            levelDatabase.MaximumPlayableLevelNumber)
        {
            Debug.LogWarning(
                $"Level {levelNumber} playable range se bahar hai. " +
                $"Last playable level " +
                $"{levelDatabase.MaximumPlayableLevelNumber} hai.",
                this
            );
            return;
        }

        int levelIndex =
            levelDatabase.FindIndexByLevelNumber(levelNumber);

        if (levelIndex < 0)
        {
            Debug.LogWarning(
                $"Addressable Level {levelNumber} catalog mein nahi mila.",
                this
            );
            return;
        }

        StartCoroutine(
            LoadAddressableLevelRoutine(levelIndex)
        );
    }


    private void SelectStartingLevel()
    {
        if (levelDatabase == null ||
            levelDatabase.Count == 0)
        {
            currentLevelIndex = 0;
            return;
        }

        if (PlayerPrefs.HasKey(SavedLevelNumberKey))
        {
            int savedLevelNumber =
                PlayerPrefs.GetInt(
                    SavedLevelNumberKey,
                    1
                );

            int savedLevelIndex =
                levelDatabase.FindIndexByLevelNumber(
                    savedLevelNumber
                );

            if (savedLevelIndex >= 0 &&
                FindNextLevelIndex(savedLevelIndex) ==
                savedLevelIndex)
            {
                currentLevelIndex = savedLevelIndex;
                return;
            }
        }

        if (startFromLevelOne)
        {
            int firstLevelIndex =
                FindNextLevelIndex(0);

            if (firstLevelIndex >= 0)
            {
                currentLevelIndex = firstLevelIndex;
            }

            return;
        }

        int configuredIndex =
            levelData != null
                ? levelDatabase.FindIndexByLevelNumber(
                    levelData.LevelNumber
                )
                : -1;

        currentLevelIndex =
            configuredIndex >= 0
                ? configuredIndex
                : Mathf.Max(0, FindNextLevelIndex(0));

    }


    private static bool IsLiveWorkingLevelPreviewEnabled()
    {
#if UNITY_EDITOR
        return UnityEditor.SessionState.GetBool(
            "RoyalSmash.LiveWorkingLevelPreview",
            false
        );
#else
        return false;
#endif
    }


    private static bool ShouldStartLiveWorkingLevelPreview()
    {
#if UNITY_EDITOR
        const string previewSessionKey =
            "RoyalSmash.LiveWorkingLevelPreview";

        const string previewLaunchSessionKey =
            "RoyalSmash.LiveWorkingLevelPreviewLaunch";

        bool previewEnabled =
            UnityEditor.SessionState.GetBool(
                previewSessionKey,
                false
            );

        bool launchRequested =
            UnityEditor.SessionState.GetBool(
                previewLaunchSessionKey,
                false
            );

        UnityEditor.SessionState.SetBool(
            previewLaunchSessionKey,
            false
        );

        if (!launchRequested)
        {
            /*
             * Toolbar se Play karne par pichli editor preview session
             * normal game startup ko bypass nahi kar sakti.
             */
            UnityEditor.SessionState.SetBool(
                previewSessionKey,
                false
            );
        }

        return previewEnabled && launchRequested;
#else
        return false;
#endif
    }


    private int FindNextLevelIndex(
        int startingIndex)
    {
        if (levelDatabase == null)
        {
            return -1;
        }

        for (int i = Mathf.Max(0, startingIndex);
             i < levelDatabase.Count;
             i++)
        {
            int levelNumber =
                levelDatabase.GetLevelNumber(i);

            if (levelNumber >
                levelDatabase.MaximumPlayableLevelNumber)
            {
                break;
            }

            if (levelNumber > 0 &&
                !string.IsNullOrWhiteSpace(
                    levelDatabase.GetAddress(i)))
            {
                return i;
            }
        }

        return -1;
    }


    private void EnsureMainMenuUI()
    {
        if (mainMenuPanel == null)
        {
            RectTransform[] sceneRects =
                FindObjectsByType<RectTransform>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (RectTransform sceneRect in sceneRects)
            {
                if (sceneRect != null &&
                    sceneRect.name.Equals(
                        "Main menu Panel",
                        System.StringComparison.OrdinalIgnoreCase
                    ))
                {
                    mainMenuPanel = sceneRect.gameObject;
                    break;
                }
            }
        }

        if (mainMenuPanel == null)
        {
            Debug.LogWarning(
                "Main menu Panel scene mein nahi mila.",
                this
            );
            return;
        }

        Button[] menuButtons =
            mainMenuPanel.GetComponentsInChildren<Button>(true);

        foreach (Button menuButton in menuButtons)
        {
            if (menuButton == null)
            {
                continue;
            }

            string buttonName =
                menuButton.name.ToLowerInvariant();

            if (resetProgressButton == null &&
                buttonName.Contains("reset"))
            {
                resetProgressButton = menuButton;
            }
        }

        if (currentLevelText == null)
        {
            TMP_Text[] menuTexts =
                mainMenuPanel.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text menuText in menuTexts)
            {
                if (menuText != null &&
                    menuText.text.IndexOf(
                        "level number",
                        System.StringComparison.OrdinalIgnoreCase
                    ) >= 0)
                {
                    currentLevelText = menuText;
                    break;
                }
            }
        }

        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.RemoveListener(
                ResetProgressFromMainMenu
            );
            resetProgressButton.onClick.AddListener(
                ResetProgressFromMainMenu
            );
        }
    }


    private void SetGameplayWorldVisible(bool visible)
    {
        if (gameplayPanel != null)
        {
            if (visible)
            {
                UITransition.Show(gameplayPanel);
            }
            else
            {
                UITransition.Hide(gameplayPanel);
            }
        }

        if (levelOrigin != null &&
            levelOrigin != transform)
        {
            levelOrigin.gameObject.SetActive(visible);
        }

        if (runtimeObjectsRoot != null)
        {
            runtimeObjectsRoot.gameObject.SetActive(visible);
        }

        if (cannonController == null)
        {
            cannonController =
                FindFirstObjectByType<CannonController>(
                    FindObjectsInactive.Include
                );
        }

        if (cannonController != null)
        {
            cannonController.SetGameplayActive(visible);
        }

        if (gameplayRestartButton == null)
        {
            GameRestartController restartController =
                FindFirstObjectByType<GameRestartController>(
                    FindObjectsInactive.Include
                );

            if (restartController != null)
            {
                gameplayRestartButton =
                    restartController.gameObject;
            }
        }

        if (gameplayRestartButton != null)
        {
            gameplayRestartButton.SetActive(visible);
        }

        if (!visible)
        {
            foreach (LevelTable table in cachedTables)
            {
                if (table != null)
                {
                    table.gameObject.SetActive(false);
                }
            }

            // Level Complete UI is now owned by LevelCompleteUIController.
        }
    }


    private void SaveCurrentLevelProgress(int levelNumber)
    {
        if (levelNumber <= 0 ||
            IsLiveWorkingLevelPreviewEnabled())
        {
            return;
        }

        PlayerPrefs.SetInt(
            SavedLevelNumberKey,
            levelNumber
        );
        PlayerPrefs.Save();
        UpdateCurrentLevelText();
    }


    private void UpdateCurrentLevelText()
    {
        if ((currentLevelText == null &&
             mainMenuLevelText == null) ||
            levelDatabase == null ||
            levelDatabase.Count == 0)
        {
            return;
        }

        int safeIndex =
            Mathf.Clamp(
                currentLevelIndex,
                0,
                levelDatabase.Count - 1
            );

        int levelNumber =
            levelDatabase.GetLevelNumber(safeIndex);

        string levelNumberText =
            $"{Mathf.Max(1, levelNumber)}";

        if (currentLevelText != null)
        {
            currentLevelText.text =
                levelNumberText;
        }

        if (mainMenuLevelText != null)
        {
            mainMenuLevelText.text =
                levelNumberText;
        }
    }


    private void PrepareForLevelAssetRelease()
    {
        ClearCurrentLevel();
        DestroyActiveCannonBalls();

        if (objectPool != null)
        {
            objectPool.ClearAll();
        }

        for (int index = cachedTables.Count - 1;
             index >= 0;
             index--)
        {
            DestroyCachedTableAt(index);
        }

        cachedTables.Clear();
        cachedTablePrefabs.Clear();
        currentTable = null;
    }


    private static void DestroyActiveCannonBalls()
    {
        CannonBallMarker[] cannonBalls =
            FindObjectsByType<CannonBallMarker>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (CannonBallMarker cannonBall in cannonBalls)
        {
            if (cannonBall == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(cannonBall.gameObject);
            }
            else
            {
                DestroyImmediate(cannonBall.gameObject);
            }
        }
    }


    private void ReleaseActiveLevelHandle()
    {
        if (!hasActiveLevelHandle)
        {
            return;
        }

        if (activeLevelHandle.IsValid())
        {
            Addressables.Release(activeLevelHandle);
        }

        hasActiveLevelHandle = false;
    }


    public void ClearCurrentLevel()
    {
        levelGenerationRevision++;

        if (levelEntranceRoutine != null)
        {
            StopCoroutine(levelEntranceRoutine);
            levelEntranceRoutine = null;
        }

        levelEntranceAnimationEntries.Clear();

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

            instance.Activated -=
                HandleObjectActivated;


            if (objectPool != null)
            {
                objectPool.Release(
                    instance
                );
            }
        }


        activeObjects.Clear();

        occupancyByCoordinate.Clear();

        trackingByInstance.Clear();

        gridBaseRowByTable.Clear();

        /*
         * Fit cache CellSize-specific hai — agar LoadLevel() se
         * different GridLevelData aa jaye (alag CellSize) to purani
         * cached scale ghalat hogi.
         */
        fitCache.Clear();


        remainingTargets =
            0;


        foreach (LevelTable table in cachedTables)
        {
            if (table != null)
            {
                table.gameObject.SetActive(false);
            }
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


        if (!levelData.HasAnyValidTableGrid)
        {
            Debug.LogError(
                "Manual GridLevelData mein koi painted cell nahi hai. " +
                "Level Creator mein grid allocate karke shapes paint karein.",
                levelData
            );

            return false;
        }


        if (levelData.BlockPalette == null ||
            levelData.BlockPalette.Count == 0)
        {
            Debug.LogError(
                "Grid Level Data mein Block Palette empty hai.",
                levelData
            );

            return false;
        }

        bool hasValidPaletteEntry =
            false;

        foreach (PhysicsObjectDefinition entry
                 in levelData.BlockPalette)
        {
            if (entry != null &&
                entry.Prefab != null)
            {
                hasValidPaletteEntry =
                    true;

                break;
            }
        }

        if (!hasValidPaletteEntry)
        {
            Debug.LogError(
                "Grid Level Data ki Block Palette mein koi valid " +
                "Prefab wali entry nahi hai.",
                levelData
            );

            return false;
        }


        if (levelData.TableCount < 1 ||
            levelData.GetTablePrefab(0) == null)
        {
            Debug.LogError(
                "Grid Level Data mein PRIMARY Table Prefab missing hai.",
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

                instance.Activated -=
                    HandleObjectActivated;
            }
        }

        ReleaseActiveLevelHandle();

        if (hasPendingLevelHandle &&
            pendingLevelHandle.IsValid())
        {
            Addressables.Release(pendingLevelHandle);
        }

        hasPendingLevelHandle = false;
    }
}









