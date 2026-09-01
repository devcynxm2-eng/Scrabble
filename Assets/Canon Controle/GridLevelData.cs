using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class LevelTablePlacement
{
    [SerializeField]
    private LevelTable prefab;

    [SerializeField]
    private Vector3 positionOffset = Vector3.zero;

    [SerializeField]
    private Vector3 rotationEuler = Vector3.zero;

    [Tooltip(
        "ON: gameplay ke duran table aur us par locked blocks continuously rotate honge."
    )]
    [SerializeField]
    private bool enableRuntimeRotation;

    [Tooltip(
        "Table ke local-space axis par runtime rotation hogi. Normal turntable ke liye (0, 1, 0)."
    )]
    [SerializeField]
    private Vector3 runtimeRotationAxis = Vector3.up;

    [Tooltip(
        "Runtime rotation speed degrees per second. Negative value opposite direction mein ghumati hai."
    )]
    [SerializeField]
    private float runtimeRotationSpeed = 20f;

    [Tooltip(
        "ON: gameplay ke duran table aur us par locked blocks left/right smoothly move honge."
    )]
    [SerializeField]
    private bool enableRuntimeHorizontalMovement;

    [Tooltip(
        "Level ke local-space mein movement direction. Left/right ke liye (1, 0, 0)."
    )]
    [SerializeField]
    private Vector3 runtimeMovementAxis = Vector3.right;

    [Tooltip(
        "Center position se ek side tak maximum movement distance."
    )]
    [SerializeField, Min(0f)]
    private float runtimeMovementDistance = 0.75f;

    [Tooltip(
        "Left/right movement cycles per second. 0.2 ka matlab ek complete cycle 5 seconds mein."
    )]
    [SerializeField, Min(0f)]
    private float runtimeMovementSpeed = 0.2f;

    [Tooltip(
        "ON: configured left/right cycles complete hone ke baad locked blocks dynamic physics mein release honge."
    )]
    [SerializeField]
    private bool releaseBlocksAfterMovementCycles = true;

    [Tooltip(
        "Physics release se pehle minimum complete left/right cycles."
    )]
    [SerializeField, Min(1)]
    private int minimumMovementCyclesBeforeRelease = 3;

    [Tooltip(
        "Physics release se pehle maximum complete left/right cycles. Har level run mein min/max ke beech random value pick hogi."
    )]
    [SerializeField, Min(1)]
    private int maximumMovementCyclesBeforeRelease = 5;

    [Tooltip(
        "Release par table ki movement velocity ka kitna hissa blocks ko mile."
    )]
    [SerializeField, Range(0f, 2f)]
    private float movementReleaseVelocityMultiplier = 1f;

    [Tooltip(
        "Prefab ke original scale ka multiplier. (1, 1, 1) original size hai."
    )]
    [SerializeField]
    private Vector3 sizeMultiplier = Vector3.one;


    public LevelTable Prefab => prefab;

    public Vector3 PositionOffset => positionOffset;

    public Vector3 RotationEuler => rotationEuler;

    public bool RuntimeRotationEnabled => enableRuntimeRotation;

    public Vector3 RuntimeRotationAxis => runtimeRotationAxis;

    public float RuntimeRotationSpeed => runtimeRotationSpeed;

    public bool RuntimeHorizontalMovementEnabled =>
        enableRuntimeHorizontalMovement;

    public Vector3 RuntimeMovementAxis => runtimeMovementAxis;

    public float RuntimeMovementDistance => runtimeMovementDistance;

    public float RuntimeMovementSpeed => runtimeMovementSpeed;

    public bool ReleaseBlocksAfterMovementCycles =>
        releaseBlocksAfterMovementCycles;

    public int MinimumMovementCyclesBeforeRelease =>
        minimumMovementCyclesBeforeRelease;

    public int MaximumMovementCyclesBeforeRelease =>
        maximumMovementCyclesBeforeRelease;

    public float MovementReleaseVelocityMultiplier =>
        movementReleaseVelocityMultiplier;

    public Vector3 SizeMultiplier => sizeMultiplier;


    public LevelTablePlacement()
    {
    }


    public LevelTablePlacement(
        LevelTable tablePrefab,
        Vector3 tablePositionOffset,
        Vector3 tableRotationEuler)
    {
        prefab = tablePrefab;
        positionOffset = tablePositionOffset;
        rotationEuler = tableRotationEuler;
        sizeMultiplier = Vector3.one;
    }


    public void EnsureValidSize()
    {
        sizeMultiplier.x = Mathf.Max(0.01f, sizeMultiplier.x);
        sizeMultiplier.y = Mathf.Max(0.01f, sizeMultiplier.y);
        sizeMultiplier.z = Mathf.Max(0.01f, sizeMultiplier.z);

        if (runtimeRotationAxis.sqrMagnitude < 0.0001f)
        {
            runtimeRotationAxis = Vector3.up;
        }

        if (runtimeMovementAxis.sqrMagnitude < 0.0001f)
        {
            runtimeMovementAxis = Vector3.right;
        }

        runtimeMovementDistance =
            Mathf.Max(0f, runtimeMovementDistance);

        runtimeMovementSpeed =
            Mathf.Max(0f, runtimeMovementSpeed);

        minimumMovementCyclesBeforeRelease =
            Mathf.Max(1, minimumMovementCyclesBeforeRelease);

        maximumMovementCyclesBeforeRelease =
            Mathf.Max(
                minimumMovementCyclesBeforeRelease,
                maximumMovementCyclesBeforeRelease
            );

        movementReleaseVelocityMultiplier =
            Mathf.Clamp(
                movementReleaseVelocityMultiplier,
                0f,
                2f
            );
    }
}

[Serializable]
public sealed class LevelTrapPlacement
{
    /*
     * Trap = level mein rakha gaya obstacle (stick, bar, blade) jo
     * apni jagah par ghoom sakta hai aur/ya aage-peeche chal sakta hai.
     *
     * Ye tables se alag hai: trap par grid spawn nahi hota, ye sirf ball
     * aur girte hue blocks ke raaste mein aata hai.
     */

    [Tooltip(
        "Trap ka prefab — stick, bar, blade wagera. Behtar hai ke is par " +
        "collider aur KINEMATIC Rigidbody ho, taake ye ball ko theek se " +
        "dhakel sake."
    )]
    [SerializeField]
    private GameObject prefab;

    [Tooltip("Editor list mein pehchan ke liye. Khali ho to prefab ka naam.")]
    [SerializeField]
    private string displayName = string.Empty;

    [Tooltip("Level origin ke relative position.")]
    [SerializeField]
    private Vector3 positionOffset = Vector3.zero;

    [SerializeField]
    private Vector3 rotationEuler = Vector3.zero;

    [Tooltip("Prefab ki original scale ka multiplier.")]
    [SerializeField]
    private Vector3 scaleMultiplier = Vector3.one;


    [Header("Rotation")]

    [Tooltip("ON: trap apne axis par lagatar ghoomta rahega.")]
    [SerializeField]
    private bool enableRotation = false;

    [Tooltip(
        "Ghoomne ka axis. (0,0,1) = screen ke rukh par pinwheel ki " +
        "tarah, (0,1,0) = upar se dekhne par."
    )]
    [SerializeField]
    private Vector3 rotationAxis = Vector3.forward;

    [Tooltip("Degrees per second. Manfi value ulti simt ghumati hai.")]
    [SerializeField]
    private float rotationSpeed = 90f;


    [Header("Movement")]

    [Tooltip(
        "ON: trap apne axis par aage-peeche (ping-pong) chalega."
    )]
    [SerializeField]
    private bool enableMovement = false;

    [Tooltip(
        "Chalne ka axis:\n" +
        "(0,1,0) = upar/neeche\n" +
        "(0,0,1) = aage/peeche\n" +
        "(1,0,0) = left/right"
    )]
    [SerializeField]
    private Vector3 movementAxis = Vector3.up;

    [Tooltip("Markaz se kitna door tak jayega.")]
    [SerializeField, Min(0f)]
    private float movementDistance = 0.5f;

    [Tooltip("Movement ki raftaar (cycles per second).")]
    [SerializeField, Min(0f)]
    private float movementSpeed = 0.3f;


    public GameObject Prefab => prefab;

    public string DisplayName =>
        !string.IsNullOrEmpty(displayName)
            ? displayName
            : prefab != null
                ? prefab.name
                : "Trap";

    public Vector3 PositionOffset => positionOffset;

    public Vector3 RotationEuler => rotationEuler;

    public Vector3 ScaleMultiplier => scaleMultiplier;

    public bool RotationEnabled => enableRotation;

    public Vector3 RotationAxis => rotationAxis;

    public float RotationSpeed => rotationSpeed;

    public bool MovementEnabled => enableMovement;

    public Vector3 MovementAxis => movementAxis;

    public float MovementDistance => movementDistance;

    public float MovementSpeed => movementSpeed;
}


public sealed class GridLevelData : ScriptableObject
{
    [Header("Level")]

    [SerializeField, Min(1)]
    private int levelNumber = 1;

    [SerializeField, Min(1)]
    private int availableBalls = 30;


    [SerializeField, HideInInspector]
    private LevelTable tablePrefab;

    [SerializeField, HideInInspector]
    private Vector3 tablePositionOffset =
        Vector3.zero;

    [SerializeField, HideInInspector]
    private Vector3 tableRotationEuler =
        Vector3.zero;

    [Header("Tables")]
    [Tooltip(
        "Element 0 PRIMARY table hai aur grid isi ki top surface par spawn hota hai. " +
        "Additional elements level mein extra tables spawn karte hain."
    )]
    [SerializeField]
    private List<LevelTablePlacement> tablePlacements =
        new List<LevelTablePlacement>();


    [Header("Traps")]

    /*
     * Optional. Khali list ka matlab hai "is level mein koi trap nahi",
     * aur purane 100 levels deserialize hote waqt yahi khali list paate
     * hain — is liye unka behaviour bilkul nahi badalta.
     */
    [Tooltip(
        "Level ke moving / rotating obstacles. Khali chhorna bilkul " +
        "theek hai."
    )]
    [SerializeField]
    private List<LevelTrapPlacement> trapPlacements =
        new List<LevelTrapPlacement>();


    [Header("Grid Blocks")]

    [Tooltip(
        "Is level mein use hone wale shapes (cube, cylinder, wagera). " +
        "Har grid cell inmein se ek index reference karta hai, isliye " +
        "ek hi level mein multiple shapes mix ho sakti hain."
    )]
    [SerializeField]
    private List<PhysicsObjectDefinition> blockPalette =
        new List<PhysicsObjectDefinition>();

    [Tooltip(
        "Ek grid cell ka uniform world-space footprint. " +
        "Palette ki har shape auto-fit ke through isi size mein " +
        "scale hoti hai, is liye cube/cylinder jaisi different shapes " +
        "bhi same grid mein clean tarah se align hoti hain."
    )]
    [SerializeField]
    private Vector3 cellSize =
        new Vector3(0.4f, 0.4f, 0.4f);


    [Header("3D Grid Resolution")]

    [SerializeField, Min(1)]
    private int gridWidth = 12;

    [SerializeField, Min(1)]
    private int gridHeight = 12;

    [SerializeField, Min(1), HideInInspector]
    private int gridDepth = 2;

    [SerializeField, HideInInspector]
    private bool mirrorPaintAcrossLayers = true;


    [Header("Grid Position")]

    [SerializeField]
    private Vector3 gridOffset =
        Vector3.zero;

    [SerializeField, Min(0f)]
    private float horizontalGap = 0f;

    [SerializeField, Min(0f)]
    private float verticalGap = 0f;

    [SerializeField, Min(0f), HideInInspector]
    private float depthGap = 0f;


    [Header("Paint Colors")]

    [SerializeField]
    private List<Color> colorPalette =
        new List<Color>
        {
            new Color(1.00f, 0.20f, 0.30f, 1f),
            new Color(0.10f, 0.58f, 1.00f, 1f),
            new Color(1.00f, 0.78f, 0.10f, 1f),
            new Color(0.55f, 0.25f, 0.95f, 1f),
            new Color(0.05f, 0.80f, 0.52f, 1f),
            new Color(1.00f, 0.42f, 0.10f, 1f)
        };

    [SerializeField, HideInInspector]
    private List<GridDepthLayerData> layers =
        new List<GridDepthLayerData>();

    [SerializeField, HideInInspector]
    private List<TableGridData> additionalTableGrids =
        new List<TableGridData>();


    [FormerlySerializedAs("bakedOccupiedCellCount")]
    [SerializeField, HideInInspector]
    private int occupiedCellCount;

    [SerializeField, HideInInspector]
    private Vector3Int occupiedMin =
        Vector3Int.zero;

    [SerializeField, HideInInspector]
    private Vector3Int occupiedMax =
        Vector3Int.zero;


    public int LevelNumber => levelNumber;

    public int AvailableBalls => availableBalls;

    public int TableCount =>
        tablePlacements != null && tablePlacements.Count > 0
            ? tablePlacements.Count
            : tablePrefab != null ? 1 : 0;

    public LevelTable TablePrefab =>
        GetTablePrefab(0);

    public Vector3 TablePositionOffset =>
        GetTablePositionOffset(0);

    public Vector3 TableRotationEuler =>
        GetTableRotationEuler(0);

    public Vector3 TableSizeMultiplier =>
        GetTableSizeMultiplier(0);


    public IReadOnlyList<LevelTrapPlacement> TrapPlacements =>
        trapPlacements;

    public int TrapCount =>
        trapPlacements != null
            ? trapPlacements.Count
            : 0;

    public LevelTrapPlacement GetTrap(int index)
    {
        if (trapPlacements == null ||
            index < 0 ||
            index >= trapPlacements.Count)
        {
            return null;
        }

        return trapPlacements[index];
    }


    public LevelTable GetTablePrefab(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.Prefab
            : index == 0 && UsesLegacyTable()
                ? tablePrefab
                : null;
    }


    public Vector3 GetTablePositionOffset(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.PositionOffset
            : index == 0 && UsesLegacyTable()
                ? tablePositionOffset
                : Vector3.zero;
    }


    public Vector3 GetTableRotationEuler(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.RotationEuler
            : index == 0 && UsesLegacyTable()
                ? tableRotationEuler
                : Vector3.zero;
    }


    public Vector3 GetTableSizeMultiplier(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.SizeMultiplier
            : Vector3.one;
    }


    public bool IsTableRuntimeRotationEnabled(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null &&
               placement.RuntimeRotationEnabled;
    }


    public Vector3 GetTableRuntimeRotationAxis(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.RuntimeRotationAxis
            : Vector3.up;
    }


    public float GetTableRuntimeRotationSpeed(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.RuntimeRotationSpeed
            : 0f;
    }


    public bool IsTableRuntimeHorizontalMovementEnabled(
        int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null &&
               placement.RuntimeHorizontalMovementEnabled;
    }


    public Vector3 GetTableRuntimeMovementAxis(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.RuntimeMovementAxis
            : Vector3.right;
    }


    public float GetTableRuntimeMovementDistance(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.RuntimeMovementDistance
            : 0f;
    }


    public float GetTableRuntimeMovementSpeed(int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.RuntimeMovementSpeed
            : 0f;
    }


    public bool ShouldReleaseTableBlocksAfterMovementCycles(
        int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null &&
               placement.ReleaseBlocksAfterMovementCycles;
    }


    public int GetTableMinimumMovementCyclesBeforeRelease(
        int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.MinimumMovementCyclesBeforeRelease
            : 3;
    }


    public int GetTableMaximumMovementCyclesBeforeRelease(
        int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.MaximumMovementCyclesBeforeRelease
            : 5;
    }


    public float GetTableMovementReleaseVelocityMultiplier(
        int index)
    {
        LevelTablePlacement placement =
            GetTablePlacement(index);

        return placement != null
            ? placement.MovementReleaseVelocityMultiplier
            : 1f;
    }


    private LevelTablePlacement GetTablePlacement(int index)
    {
        if (tablePlacements == null ||
            index < 0 ||
            index >= tablePlacements.Count)
        {
            return null;
        }

        return tablePlacements[index];
    }


    private bool UsesLegacyTable()
    {
        return tablePlacements == null ||
               tablePlacements.Count == 0;
    }

    public IReadOnlyList<PhysicsObjectDefinition> BlockPalette =>
        blockPalette;

    public Vector3 CellSize =>
        cellSize;

    /// <summary>
    /// Palette entry for a cell's DefinitionIndex, clamped so an
    /// out-of-range/stale index (e.g. after removing a palette entry)
    /// falls back to entry 0 instead of throwing.
    /// </summary>
    public PhysicsObjectDefinition GetPaletteEntry(
        int definitionIndex)
    {
        if (blockPalette == null ||
            blockPalette.Count == 0)
        {
            return null;
        }

        int clampedIndex =
            Mathf.Clamp(
                definitionIndex,
                0,
                blockPalette.Count - 1
            );

        return blockPalette[clampedIndex];
    }

    public int GridWidth =>
        gridWidth;

    public int GridHeight =>
        gridHeight;

    public int GridDepth =>
        gridDepth;

    public bool MirrorPaintAcrossLayers =>
        mirrorPaintAcrossLayers;

    public Vector3 GridOffset =>
        gridOffset;

    public float HorizontalGap =>
        horizontalGap;

    public float VerticalGap =>
        verticalGap;

    public float DepthGap =>
        depthGap;

    public int OccupiedCellCount =>
        occupiedCellCount;

    public IReadOnlyList<GridDepthLayerData> Layers =>
        layers;


    public bool HasValidGrid
    {
        get
        {
            return
                IsGridAllocated &&
                occupiedCellCount > 0;
        }
    }


    /// <summary>
    /// True once layers/rows/cells match gridWidth/gridHeight/gridDepth,
    /// regardless of whether any cell is occupied yet. Used by the manual
    /// grid painter, which needs an allocated (but possibly still empty) grid.
    /// </summary>
    public bool IsGridAllocated
    {
        get
        {
            return IsGridAllocatedForTable(0);
        }
    }


    public bool HasAnyValidTableGrid
    {
        get
        {
            int gridCount = Mathf.Max(1, TableCount);

            for (int tableIndex = 0;
                 tableIndex < gridCount;
                 tableIndex++)
            {
                if (IsGridAllocatedForTable(tableIndex) &&
                    TryGetOccupiedBounds(
                        tableIndex,
                        out _,
                        out _))
                {
                    return true;
                }
            }

            return false;
        }
    }


    public bool IsGridAllocatedForTable(int tableIndex)
    {
        List<GridDepthLayerData> tableLayers =
            GetLayersForTable(tableIndex);

        if (tableLayers == null ||
            tableLayers.Count != gridDepth)
        {
            return false;
        }

        for (int z = 0;
             z < tableLayers.Count;
             z++)
        {
            GridDepthLayerData layer =
                tableLayers[z];

            if (layer == null ||
                layer.Rows == null ||
                layer.Rows.Count != gridHeight)
            {
                return false;
            }

            for (int y = 0;
                 y < layer.Rows.Count;
                 y++)
            {
                GridRowData row =
                    layer.Rows[y];

                if (row == null ||
                    row.Cells == null ||
                    row.Cells.Count != gridWidth)
                {
                    return false;
                }
            }
        }

        return true;
    }


    public GridCellData GetCell(
        int x,
        int y,
        int z)
    {
        return GetCell(x, y, z, 0);
    }


    public GridCellData GetCell(
        int x,
        int y,
        int z,
        int tableIndex)
    {
        if (x < 0 ||
            x >= gridWidth ||
            y < 0 ||
            y >= gridHeight ||
            z < 0 ||
            z >= gridDepth)
        {
            return null;
        }

        List<GridDepthLayerData> tableLayers =
            GetLayersForTable(tableIndex);

        if (tableLayers == null ||
            z >= tableLayers.Count)
        {
            return null;
        }

        GridDepthLayerData layer =
            tableLayers[z];

        if (layer == null ||
            layer.Rows == null ||
            y >= layer.Rows.Count)
        {
            return null;
        }

        GridRowData row =
            layer.Rows[y];

        if (row == null ||
            row.Cells == null ||
            x >= row.Cells.Count)
        {
            return null;
        }

        return row.Cells[x];
    }


    public bool TryGetOccupiedBounds(
        out Vector3Int minimum,
        out Vector3Int maximum)
    {
        minimum =
            occupiedMin;

        maximum =
            occupiedMax;

        return
            occupiedCellCount > 0;
    }


    public bool TryGetOccupiedBounds(
        int tableIndex,
        out Vector3Int minimum,
        out Vector3Int maximum)
    {
        minimum =
            new Vector3Int(
                int.MaxValue,
                int.MaxValue,
                int.MaxValue
            );

        maximum =
            new Vector3Int(
                int.MinValue,
                int.MinValue,
                int.MinValue
            );

        bool foundAny = false;
        List<GridDepthLayerData> tableLayers =
            GetLayersForTable(tableIndex);

        if (tableLayers == null)
        {
            minimum = Vector3Int.zero;
            maximum = Vector3Int.zero;
            return false;
        }

        for (int z = 0; z < tableLayers.Count; z++)
        {
            GridDepthLayerData layer = tableLayers[z];

            if (layer?.Rows == null)
            {
                continue;
            }

            for (int y = 0; y < layer.Rows.Count; y++)
            {
                GridRowData row = layer.Rows[y];

                if (row?.Cells == null)
                {
                    continue;
                }

                for (int x = 0; x < row.Cells.Count; x++)
                {
                    GridCellData cell = row.Cells[x];

                    if (cell == null || !cell.Occupied)
                    {
                        continue;
                    }

                    foundAny = true;
                    Vector3Int coordinate = new Vector3Int(x, y, z);
                    minimum = Vector3Int.Min(minimum, coordinate);
                    maximum = Vector3Int.Max(maximum, coordinate);
                }
            }
        }

        if (!foundAny)
        {
            minimum = Vector3Int.zero;
            maximum = Vector3Int.zero;
        }

        return foundAny;
    }


    private List<GridDepthLayerData> GetLayersForTable(int tableIndex)
    {
        if (tableIndex <= 0)
        {
            return layers;
        }

        int additionalIndex = tableIndex - 1;

        if (additionalTableGrids == null ||
            additionalIndex >= additionalTableGrids.Count ||
            additionalTableGrids[additionalIndex] == null)
        {
            return null;
        }

        return additionalTableGrids[additionalIndex].Layers;
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        gridWidth = Mathf.Max(1, gridWidth);
        gridHeight = Mathf.Max(1, gridHeight);
        gridDepth = Mathf.Max(1, gridDepth);

        if (tablePlacements != null)
        {
            foreach (LevelTablePlacement placement in tablePlacements)
            {
                placement?.EnsureValidSize();
            }
        }

        /*
         * When an existing one-layer level is changed to two layers,
         * immediately copy its authored front layer into the new back
         * layer. This keeps the level playable without requiring a
         * separate allocation click after changing Grid Depth.
         */
        if (layers != null &&
            layers.Count > 0 &&
            layers.Count < gridDepth)
        {
            EditorEnsureGridAllocated(true);
        }
    }

    /// <summary>
    /// Hand-authored level design API used by GridLevelDataEditor's
    /// manual grid painter and consumed directly by LevelRuntimeController.
    /// </summary>

    public void EditorAddPaletteEntry(
        PhysicsObjectDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        if (blockPalette == null)
        {
            blockPalette =
                new List<PhysicsObjectDefinition>();
        }

        if (!blockPalette.Contains(definition))
        {
            blockPalette.Add(definition);
        }
    }


    public void EditorSetLevelNumber(
        int newLevelNumber)
    {
        levelNumber = Mathf.Max(1, newLevelNumber);
    }


    public bool EditorMigrateLegacyTable()
    {
        if (tablePlacements == null)
        {
            tablePlacements =
                new List<LevelTablePlacement>();
        }

        if (tablePlacements.Count > 0 ||
            tablePrefab == null)
        {
            return false;
        }

        tablePlacements.Add(
            new LevelTablePlacement(
                tablePrefab,
                tablePositionOffset,
                tableRotationEuler
            )
        );

        return true;
    }


    public bool EditorMigrateSharedGridTableAssignments()
    {
        if (layers == null ||
            TableCount <= 1)
        {
            return false;
        }

        List<(Vector3Int coordinate, int tableIndex)> migrations =
            new List<(Vector3Int, int)>();

        for (int z = 0; z < gridDepth; z++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    GridCellData cell = GetCell(x, y, z, 0);

                    if (cell == null ||
                        !cell.Occupied ||
                        cell.IsCovered ||
                        cell.TableIndex <= 0)
                    {
                        continue;
                    }

                    migrations.Add(
                        (
                            new Vector3Int(x, y, z),
                            Mathf.Clamp(
                                cell.TableIndex,
                                0,
                                TableCount - 1
                            )
                        )
                    );
                }
            }
        }

        bool changed = false;

        foreach ((Vector3Int coordinate, int tableIndex) migration
                 in migrations)
        {
            if (EditorTryTransferBlockToTable(
                    0,
                    migration.coordinate,
                    migration.tableIndex,
                    out _))
            {
                changed = true;
                continue;
            }

            EditorSetSpanGroupTable(
                migration.coordinate,
                0
            );

            changed = true;
        }

        return changed;
    }

    public void EditorEnsureGridAllocated(
        bool duplicateLastDepthLayer = false)
    {
        EditorEnsureGridAllocated(
            0,
            duplicateLastDepthLayer
        );
    }


    public void EditorEnsureGridAllocated(
        int tableIndex,
        bool duplicateLastDepthLayer = false)
    {
        gridWidth =
            Mathf.Max(1, gridWidth);

        gridHeight =
            Mathf.Max(1, gridHeight);

        gridDepth =
            Mathf.Max(1, gridDepth);

        List<GridDepthLayerData> oldLayers =
            GetLayersForTable(tableIndex);

        List<GridDepthLayerData> resizedLayers =
            new List<GridDepthLayerData>(gridDepth);

        for (int z = 0; z < gridDepth; z++)
        {
            GridDepthLayerData layer =
                new GridDepthLayerData();

            for (int y = 0; y < gridHeight; y++)
            {
                GridRowData row =
                    new GridRowData();

                for (int x = 0; x < gridWidth; x++)
                {
                    bool isNewDepthLayer =
                        oldLayers != null &&
                        oldLayers.Count > 0 &&
                        z >= oldLayers.Count;

                    int sourceZ =
                        duplicateLastDepthLayer &&
                        isNewDepthLayer
                            ? oldLayers.Count - 1
                            : z;

                    GridCellData oldCell =
                        FindExistingCell(
                            oldLayers,
                            x,
                            y,
                            sourceZ
                        );

                    GridCellData newCell =
                        oldCell != null
                            ? new GridCellData(
                                oldCell.Occupied,
                                oldCell.Color,
                                oldCell.DefinitionIndex)
                            : new GridCellData(
                                false,
                                Color.white);

                    if (oldCell != null)
                    {
                        newCell.SetSpan(
                            oldCell.SpanX,
                            oldCell.SpanY,
                            isNewDepthLayer
                                ? 1
                                : oldCell.SpanZ
                        );

                        newCell.SetOrientation(
                            oldCell.Orientation
                        );

                        newCell.SetCustomZRotation(
                            oldCell.CustomZRotation
                        );

                        newCell.SetLocalOffset(
                            oldCell.LocalOffset
                        );

                        newCell.SetRotationEulerOffset(
                            oldCell.RotationEulerOffset
                        );

                        newCell.SetScaleMultiplier(
                            oldCell.ScaleMultiplier
                        );

                        newCell.SetBreakable(
                            oldCell.Breakable,
                            oldCell.HitsToBreak
                        );

                        newCell.SetTableIndex(
                            oldCell.TableIndex
                        );

                        if (oldCell.IsCovered)
                        {
                            Vector3Int anchorCoordinate =
                                oldCell.AnchorCoordinate;

                            if (isNewDepthLayer)
                            {
                                anchorCoordinate.z +=
                                    z - sourceZ;
                            }

                            newCell.SetCoveredBy(
                                anchorCoordinate
                            );
                        }
                    }

                    row.Cells.Add(
                        newCell
                    );
                }

                layer.Rows.Add(row);
            }

            resizedLayers.Add(layer);
        }

        if (tableIndex <= 0)
        {
            layers = resizedLayers;
            RecalculateGridMetadata();
            return;
        }

        if (additionalTableGrids == null)
        {
            additionalTableGrids =
                new List<TableGridData>();
        }

        int additionalIndex = tableIndex - 1;

        while (additionalTableGrids.Count <= additionalIndex)
        {
            additionalTableGrids.Add(
                new TableGridData()
            );
        }

        if (additionalTableGrids[additionalIndex] == null)
        {
            additionalTableGrids[additionalIndex] =
                new TableGridData();
        }

        additionalTableGrids[additionalIndex].Layers =
            resizedLayers;
    }


    public void EditorSetDepthLayerCount(
        int newLayerCount)
    {
        gridDepth =
            Mathf.Clamp(newLayerCount, 1, 16);

        int gridCount =
            Mathf.Max(
                TableCount,
                additionalTableGrids != null
                    ? additionalTableGrids.Count + 1
                    : 1
            );

        for (int tableIndex = 0;
             tableIndex < gridCount;
             tableIndex++)
        {
            if (tableIndex == 0 ||
                GetLayersForTable(tableIndex) != null)
            {
                EditorEnsureGridAllocated(
                    tableIndex,
                    true
                );
            }
        }
    }


    public void EditorSetMirrorPaintAcrossLayers(
        bool enabled)
    {
        mirrorPaintAcrossLayers = enabled;
    }


    public void EditorSetDepthGap(
        float gap)
    {
        depthGap = Mathf.Max(0f, gap);
    }


    public void EditorCopyDepthLayerToAll(
        int sourceLayerIndex,
        int tableIndex = 0)
    {
        if (!IsGridAllocatedForTable(tableIndex) ||
            gridDepth <= 1)
        {
            return;
        }

        sourceLayerIndex =
            Mathf.Clamp(
                sourceLayerIndex,
                0,
                gridDepth - 1
            );

        List<GridDepthLayerData> tableLayers =
            GetLayersForTable(tableIndex);

        GridDepthLayerData sourceLayer =
            tableLayers[sourceLayerIndex];

        for (int z = 0; z < gridDepth; z++)
        {
            if (z == sourceLayerIndex)
            {
                continue;
            }

            tableLayers[z] = CloneDepthLayer(
                sourceLayer,
                sourceLayerIndex,
                z
            );
        }

        RecalculateGridMetadata();
    }


    private static GridDepthLayerData CloneDepthLayer(
        GridDepthLayerData sourceLayer,
        int sourceLayerIndex,
        int targetLayerIndex)
    {
        GridDepthLayerData cloneLayer =
            new GridDepthLayerData();

        if (sourceLayer?.Rows == null)
        {
            return cloneLayer;
        }

        foreach (GridRowData sourceRow in sourceLayer.Rows)
        {
            GridRowData cloneRow =
                new GridRowData();

            if (sourceRow?.Cells != null)
            {
                foreach (GridCellData sourceCell in sourceRow.Cells)
                {
                    GridCellData cloneCell =
                        new GridCellData(
                            sourceCell != null && sourceCell.Occupied,
                            sourceCell != null
                                ? sourceCell.Color
                                : Color.white,
                            sourceCell != null
                                ? sourceCell.DefinitionIndex
                                : 0
                        );

                    if (sourceCell != null)
                    {
                        cloneCell.SetSpan(
                            sourceCell.SpanX,
                            sourceCell.SpanY,
                            1
                        );

                        cloneCell.SetOrientation(
                            sourceCell.Orientation
                        );

                        cloneCell.SetCustomZRotation(
                            sourceCell.CustomZRotation
                        );

                        cloneCell.SetLocalOffset(
                            sourceCell.LocalOffset
                        );

                        cloneCell.SetRotationEulerOffset(
                            sourceCell.RotationEulerOffset
                        );

                        cloneCell.SetScaleMultiplier(
                            sourceCell.ScaleMultiplier
                        );

                        cloneCell.SetBreakable(
                            sourceCell.Breakable,
                            sourceCell.HitsToBreak
                        );

                        cloneCell.SetTableIndex(
                            sourceCell.TableIndex
                        );

                        if (sourceCell.IsCovered)
                        {
                            Vector3Int anchorCoordinate =
                                sourceCell.AnchorCoordinate;

                            anchorCoordinate.z +=
                                targetLayerIndex - sourceLayerIndex;

                            cloneCell.SetCoveredBy(
                                anchorCoordinate
                            );
                        }
                    }

                    cloneRow.Cells.Add(cloneCell);
                }
            }

            cloneLayer.Rows.Add(cloneRow);
        }

        return cloneLayer;
    }


    private static GridCellData FindExistingCell(
        List<GridDepthLayerData> sourceLayers,
        int x,
        int y,
        int z)
    {
        if (sourceLayers == null ||
            z >= sourceLayers.Count)
        {
            return null;
        }

        GridDepthLayerData layer =
            sourceLayers[z];

        if (layer?.Rows == null ||
            y >= layer.Rows.Count)
        {
            return null;
        }

        GridRowData row =
            layer.Rows[y];

        if (row?.Cells == null ||
            x >= row.Cells.Count)
        {
            return null;
        }

        return row.Cells[x];
    }


    /// <summary>
    /// Paints/erases a single, independent (1x1x1) cell. If that cell
    /// was previously an anchor or part of a bigger footprint, the
    /// whole old span group is cleared first so no stale anchor/covered
    /// data is left behind.
    /// </summary>
    public void EditorSetCell(
        int x,
        int y,
        int z,
        bool occupied,
        Color color,
        int definitionIndex = 0,
        int tableIndex = 0)
    {
        EditorClearSpanGroupAt(x, y, z, tableIndex);

        GridCellData cell =
            GetCell(x, y, z, tableIndex);

        if (cell == null)
        {
            return;
        }

        cell.SetOccupied(occupied);
        cell.SetColor(color);
        cell.SetDefinitionIndex(Mathf.Max(0, definitionIndex));
        cell.SetTableIndex(tableIndex);
        cell.ClearSpanState();
        cell.SetTableIndex(tableIndex);
    }


    /// <summary>
    /// Paints a multi-cell shape: (x,y,z) becomes the anchor (the cell
    /// that actually spawns something at runtime), and every other cell
    /// in the spanX/spanY/spanZ footprint is marked as covered by it
    /// (occupied for stability/bounds purposes, but spawns nothing of
    /// its own). Any span group already touching the footprint is
    /// cleared first so footprints never silently overlap.
    /// </summary>
    public void EditorPaintSpan(
        int x,
        int y,
        int z,
        int spanX,
        int spanY,
        int spanZ,
        Color color,
        int definitionIndex = 0,
        PieceOrientation orientation = PieceOrientation.UprightY,
        Vector3 localOffset = default,
        float customZRotation = 0f,
        int tableIndex = 0)
    {
        spanX = Mathf.Max(1, spanX);
        spanY = Mathf.Max(1, spanY);
        spanZ = Mathf.Max(1, spanZ);

        for (int dz = 0; dz < spanZ; dz++)
        {
            for (int dy = 0; dy < spanY; dy++)
            {
                for (int dx = 0; dx < spanX; dx++)
                {
                    EditorClearSpanGroupAt(
                        x + dx,
                        y + dy,
                        z + dz,
                        tableIndex
                    );
                }
            }
        }

        Vector3Int anchorCoordinate =
            new Vector3Int(x, y, z);

        for (int dz = 0; dz < spanZ; dz++)
        {
            for (int dy = 0; dy < spanY; dy++)
            {
                for (int dx = 0; dx < spanX; dx++)
                {
                    GridCellData cell =
                        GetCell(
                            x + dx,
                            y + dy,
                            z + dz,
                            tableIndex
                        );

                    if (cell == null)
                    {
                        continue;
                    }

                    cell.SetOccupied(true);
                    cell.SetColor(color);
                    cell.SetDefinitionIndex(Mathf.Max(0, definitionIndex));

                    bool isAnchor =
                        dx == 0 && dy == 0 && dz == 0;

                    if (isAnchor)
                    {
                        cell.ClearSpanState();

                        cell.SetSpan(spanX, spanY, spanZ);

                        cell.SetOrientation(orientation);

                        cell.SetCustomZRotation(
                            customZRotation
                        );

                        cell.SetLocalOffset(localOffset);
                    }
                    else
                    {
                        cell.SetSpan(1, 1, 1);

                        cell.SetCoveredBy(anchorCoordinate);
                    }

                    cell.SetTableIndex(tableIndex);
                }
            }
        }

        RecalculateGridMetadata();
    }


    /// <summary>
    /// Erases the whole span group (anchor + all covered cells) that
    /// the given coordinate belongs to. If the cell is a plain 1x1x1
    /// cell, only that cell is cleared.
    /// </summary>
    public void EditorClearSpanGroupAt(
        int x,
        int y,
        int z,
        int tableIndex = 0)
    {
        GridCellData cell =
            GetCell(x, y, z, tableIndex);

        if (cell == null)
        {
            return;
        }

        Vector3Int anchorCoordinate =
            cell.IsCovered
                ? cell.AnchorCoordinate
                : new Vector3Int(x, y, z);

        GridCellData anchorCell =
            GetCell(
                anchorCoordinate.x,
                anchorCoordinate.y,
                anchorCoordinate.z,
                tableIndex
            );

        if (anchorCell == null)
        {
            /*
             * Anchor reference stale/out of bounds — is cell ko
             * khud independently clear kar dete hain.
             */
            cell.SetOccupied(false);
            cell.ClearSpanState();

            return;
        }

        /*
         * Safety clamp: a group can never legitimately be bigger than
         * the grid itself. Without this, any corrupted/stale span value
         * on an anchor (e.g. from bad data) could wipe far more of the
         * grid than intended instead of failing safely.
         */
        int groupSpanX =
            Mathf.Clamp(anchorCell.SpanX, 1, gridWidth);

        int groupSpanY =
            Mathf.Clamp(anchorCell.SpanY, 1, gridHeight);

        int groupSpanZ =
            Mathf.Clamp(anchorCell.SpanZ, 1, gridDepth);

        for (int dz = 0; dz < groupSpanZ; dz++)
        {
            for (int dy = 0; dy < groupSpanY; dy++)
            {
                for (int dx = 0; dx < groupSpanX; dx++)
                {
                    GridCellData groupCell =
                        GetCell(
                            anchorCoordinate.x + dx,
                            anchorCoordinate.y + dy,
                            anchorCoordinate.z + dz,
                            tableIndex
                        );

                    if (groupCell == null)
                    {
                        continue;
                    }

                    groupCell.SetOccupied(false);
                    groupCell.ClearSpanState();
                }
            }
        }

        RecalculateGridMetadata();
    }


    public bool EditorTryMoveBlock(
        Vector3Int sourceCoordinate,
        Vector3Int destinationCoordinate,
        out string failureReason,
        int tableIndex = 0)
    {
        failureReason = string.Empty;

        GridCellData sourceCell =
            GetCell(
                sourceCoordinate.x,
                sourceCoordinate.y,
                sourceCoordinate.z,
                tableIndex
            );

        if (sourceCell == null ||
            !sourceCell.Occupied)
        {
            failureReason = "Selected block ab grid mein maujood nahi hai.";
            return false;
        }

        Vector3Int sourceAnchor =
            sourceCell.IsCovered
                ? sourceCell.AnchorCoordinate
                : sourceCoordinate;

        GridCellData anchorCell =
            GetCell(
                sourceAnchor.x,
                sourceAnchor.y,
                sourceAnchor.z,
                tableIndex
            );

        if (anchorCell == null ||
            !anchorCell.Occupied ||
            anchorCell.IsCovered)
        {
            failureReason = "Selected block ka anchor invalid hai.";
            return false;
        }

        int moveSpanX = Mathf.Max(1, anchorCell.SpanX);
        int moveSpanY = Mathf.Max(1, anchorCell.SpanY);
        int moveSpanZ = Mathf.Max(1, anchorCell.SpanZ);

        if (destinationCoordinate.x < 0 ||
            destinationCoordinate.y < 0 ||
            destinationCoordinate.z < 0 ||
            destinationCoordinate.x + moveSpanX > gridWidth ||
            destinationCoordinate.y + moveSpanY > gridHeight ||
            destinationCoordinate.z + moveSpanZ > gridDepth)
        {
            failureReason = "Block ka footprint grid boundary se bahar ja raha hai.";
            return false;
        }

        for (int dz = 0; dz < moveSpanZ; dz++)
        {
            for (int dy = 0; dy < moveSpanY; dy++)
            {
                for (int dx = 0; dx < moveSpanX; dx++)
                {
                    Vector3Int targetCoordinate =
                        destinationCoordinate +
                        new Vector3Int(dx, dy, dz);

                    GridCellData targetCell =
                        GetCell(
                            targetCoordinate.x,
                            targetCoordinate.y,
                            targetCoordinate.z,
                            tableIndex
                        );

                    if (targetCell == null ||
                        !targetCell.Occupied)
                    {
                        continue;
                    }

                    Vector3Int targetAnchor =
                        targetCell.IsCovered
                            ? targetCell.AnchorCoordinate
                            : targetCoordinate;

                    if (targetAnchor != sourceAnchor)
                    {
                        failureReason =
                            "Destination par doosra block maujood hai.";

                        return false;
                    }
                }
            }
        }

        if (destinationCoordinate == sourceAnchor)
        {
            return true;
        }

        Color moveColor = anchorCell.Color;
        int moveDefinitionIndex = anchorCell.DefinitionIndex;
        PieceOrientation moveOrientation = anchorCell.Orientation;
        float moveCustomZ = anchorCell.CustomZRotation;
        Vector3 moveLocalOffset = anchorCell.LocalOffset;
        Vector3 moveRotationOffset = anchorCell.RotationEulerOffset;
        Vector3 moveScale = anchorCell.ScaleMultiplier;
        bool moveBreakable = anchorCell.Breakable;
        int moveHitsToBreak = anchorCell.HitsToBreak;
        EditorClearSpanGroupAt(
            sourceAnchor.x,
            sourceAnchor.y,
            sourceAnchor.z,
            tableIndex
        );

        EditorPaintSpan(
            destinationCoordinate.x,
            destinationCoordinate.y,
            destinationCoordinate.z,
            moveSpanX,
            moveSpanY,
            moveSpanZ,
            moveColor,
            moveDefinitionIndex,
            moveOrientation,
            moveLocalOffset,
            moveCustomZ,
            tableIndex
        );

        GridCellData movedAnchor =
            GetCell(
                destinationCoordinate.x,
                destinationCoordinate.y,
                destinationCoordinate.z,
                tableIndex
            );

        if (movedAnchor != null)
        {
            movedAnchor.SetRotationEulerOffset(
                moveRotationOffset
            );

            movedAnchor.SetScaleMultiplier(
                moveScale
            );

            movedAnchor.SetBreakable(
                moveBreakable,
                moveHitsToBreak
            );
        }

        return true;
    }


    public void EditorSetSpanGroupTable(
        Vector3Int coordinate,
        int tableIndex)
    {
        GridCellData cell =
            GetCell(
                coordinate.x,
                coordinate.y,
                coordinate.z
            );

        if (cell == null ||
            !cell.Occupied)
        {
            return;
        }

        Vector3Int anchorCoordinate =
            cell.IsCovered
                ? cell.AnchorCoordinate
                : coordinate;

        GridCellData anchorCell =
            GetCell(
                anchorCoordinate.x,
                anchorCoordinate.y,
                anchorCoordinate.z,
                tableIndex
            );

        if (anchorCell == null)
        {
            return;
        }

        int safeTableIndex = Mathf.Max(0, tableIndex);
        int groupSpanX = Mathf.Max(1, anchorCell.SpanX);
        int groupSpanY = Mathf.Max(1, anchorCell.SpanY);
        int groupSpanZ = Mathf.Max(1, anchorCell.SpanZ);

        for (int dz = 0; dz < groupSpanZ; dz++)
        {
            for (int dy = 0; dy < groupSpanY; dy++)
            {
                for (int dx = 0; dx < groupSpanX; dx++)
                {
                    GridCellData groupCell =
                        GetCell(
                            anchorCoordinate.x + dx,
                            anchorCoordinate.y + dy,
                            anchorCoordinate.z + dz,
                            tableIndex
                        );

                    groupCell?.SetTableIndex(safeTableIndex);
                }
            }
        }
    }


    public bool EditorTryTransferBlockToTable(
        int sourceTableIndex,
        Vector3Int sourceCoordinate,
        int destinationTableIndex,
        out string failureReason)
    {
        failureReason = string.Empty;

        if (sourceTableIndex == destinationTableIndex)
        {
            return true;
        }

        GridCellData sourceCell =
            GetCell(
                sourceCoordinate.x,
                sourceCoordinate.y,
                sourceCoordinate.z,
                sourceTableIndex
            );

        if (sourceCell == null || !sourceCell.Occupied)
        {
            failureReason = "Selected block source table par nahi mila.";
            return false;
        }

        Vector3Int anchorCoordinate =
            sourceCell.IsCovered
                ? sourceCell.AnchorCoordinate
                : sourceCoordinate;

        GridCellData anchorCell =
            GetCell(
                anchorCoordinate.x,
                anchorCoordinate.y,
                anchorCoordinate.z,
                sourceTableIndex
            );

        if (anchorCell == null || anchorCell.IsCovered)
        {
            failureReason = "Selected block ka anchor invalid hai.";
            return false;
        }

        EditorEnsureGridAllocated(
            destinationTableIndex,
            false
        );

        int spanX = Mathf.Max(1, anchorCell.SpanX);
        int spanY = Mathf.Max(1, anchorCell.SpanY);
        int spanZ = Mathf.Max(1, anchorCell.SpanZ);

        for (int dz = 0; dz < spanZ; dz++)
        {
            for (int dy = 0; dy < spanY; dy++)
            {
                for (int dx = 0; dx < spanX; dx++)
                {
                    GridCellData targetCell =
                        GetCell(
                            anchorCoordinate.x + dx,
                            anchorCoordinate.y + dy,
                            anchorCoordinate.z + dz,
                            destinationTableIndex
                        );

                    if (targetCell != null && targetCell.Occupied)
                    {
                        failureReason =
                            "Target table ke same grid cells par doosra block maujood hai.";

                        return false;
                    }
                }
            }
        }

        Color color = anchorCell.Color;
        int definitionIndex = anchorCell.DefinitionIndex;
        PieceOrientation orientation = anchorCell.Orientation;
        Vector3 localOffset = anchorCell.LocalOffset;
        float customZ = anchorCell.CustomZRotation;
        Vector3 rotationOffset = anchorCell.RotationEulerOffset;
        Vector3 scale = anchorCell.ScaleMultiplier;
        bool breakable = anchorCell.Breakable;
        int hitsToBreak = anchorCell.HitsToBreak;

        EditorClearSpanGroupAt(
            anchorCoordinate.x,
            anchorCoordinate.y,
            anchorCoordinate.z,
            sourceTableIndex
        );

        EditorPaintSpan(
            anchorCoordinate.x,
            anchorCoordinate.y,
            anchorCoordinate.z,
            spanX,
            spanY,
            spanZ,
            color,
            definitionIndex,
            orientation,
            localOffset,
            customZ,
            destinationTableIndex
        );

        GridCellData transferredCell =
            GetCell(
                anchorCoordinate.x,
                anchorCoordinate.y,
                anchorCoordinate.z,
                destinationTableIndex
            );

        if (transferredCell != null)
        {
            transferredCell.SetRotationEulerOffset(rotationOffset);
            transferredCell.SetScaleMultiplier(scale);
            transferredCell.SetBreakable(
                breakable,
                hitsToBreak
            );
        }

        return true;
    }


    public void EditorClearAllCells(int tableIndex = 0)
    {
        List<GridDepthLayerData> tableLayers =
            GetLayersForTable(tableIndex);

        if (tableLayers == null)
        {
            return;
        }

        foreach (GridDepthLayerData layer in tableLayers)
        {
            if (layer?.Rows == null)
            {
                continue;
            }

            foreach (GridRowData row in layer.Rows)
            {
                if (row?.Cells == null)
                {
                    continue;
                }

                foreach (GridCellData cell in row.Cells)
                {
                    if (cell == null)
                    {
                        continue;
                    }

                    cell.SetOccupied(false);
                    cell.ClearSpanState();
                }
            }
        }

        RecalculateGridMetadata();
    }


    public void EditorFillRectangle(
        int z,
        int minX,
        int minY,
        int maxX,
        int maxY,
        Color color,
        int definitionIndex = 0,
        bool occupied = true,
        int tableIndex = 0)
    {
        for (int y = Mathf.Max(0, minY);
             y <= Mathf.Min(gridHeight - 1, maxY);
             y++)
        {
            for (int x = Mathf.Max(0, minX);
                 x <= Mathf.Min(gridWidth - 1, maxX);
                 x++)
            {
                EditorSetCell(
                    x,
                    y,
                    z,
                    occupied,
                    color,
                    definitionIndex,
                    tableIndex
                );
            }
        }

        RecalculateGridMetadata();
    }


    /// <summary>
    /// Classic bottom-heavy pyramid: widest row at y=0,
    /// each row above one cell narrower, horizontally centered.
    /// </summary>
    public void EditorApplyPyramidPreset(
        int z,
        int bottomRowCount,
        int rowCount,
        IReadOnlyList<Color> palette,
        int definitionIndex = 0,
        int tableIndex = 0)
    {
        bottomRowCount =
            Mathf.Clamp(bottomRowCount, 1, gridWidth);

        rowCount =
            Mathf.Clamp(rowCount, 1, bottomRowCount);

        for (int y = 0; y < rowCount; y++)
        {
            int countInRow =
                Mathf.Max(1, bottomRowCount - y);

            int startX =
                Mathf.Max(
                    0,
                    (gridWidth - countInRow) / 2
                );

            Color rowColor =
                palette != null && palette.Count > 0
                    ? palette[y % palette.Count]
                    : Color.white;

            for (int x = startX;
                 x < startX + countInRow && x < gridWidth;
                 x++)
            {
                EditorSetCell(
                    x,
                    y,
                    z,
                    true,
                    rowColor,
                    definitionIndex,
                    tableIndex
                );
            }
        }

        RecalculateGridMetadata();
    }


    /// <summary>
    /// Fills a horizontal circular disc across X/Z at a single Y height.
    /// Used to build round "cake tier" stacks (shrinking radius per Y).
    /// </summary>
    public void EditorApplyRingLayer(
        int y,
        float radius,
        Color color,
        int definitionIndex = 0,
        bool hollow = false,
        float ringThickness = 1f,
        int tableIndex = 0)
    {
        if (y < 0 || y >= gridHeight)
        {
            return;
        }

        float centerX =
            (gridWidth - 1) * 0.5f;

        float centerZ =
            (gridDepth - 1) * 0.5f;

        for (int z = 0; z < gridDepth; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                float distance =
                    Vector2.Distance(
                        new Vector2(x, z),
                        new Vector2(centerX, centerZ)
                    );

                bool inside =
                    distance <= radius;

                if (inside && hollow)
                {
                    inside =
                        distance >=
                        radius - ringThickness;
                }

                if (inside)
                {
                    EditorSetCell(
                        x,
                        y,
                        z,
                        true,
                        color,
                        definitionIndex,
                        tableIndex
                    );
                }
            }
        }

        RecalculateGridMetadata();
    }


    public void RecalculateGridMetadata()
    {
        occupiedCellCount = 0;

        Vector3Int minOccupied =
            new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

        Vector3Int maxOccupied =
            new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        bool foundAny = false;

        if (layers != null)
        {
            for (int z = 0; z < layers.Count; z++)
            {
                GridDepthLayerData layer = layers[z];

                if (layer?.Rows == null)
                {
                    continue;
                }

                for (int y = 0; y < layer.Rows.Count; y++)
                {
                    GridRowData row = layer.Rows[y];

                    if (row?.Cells == null)
                    {
                        continue;
                    }

                    for (int x = 0; x < row.Cells.Count; x++)
                    {
                        GridCellData cell = row.Cells[x];

                        if (cell == null || !cell.Occupied)
                        {
                            continue;
                        }

                        occupiedCellCount++;
                        foundAny = true;

                        minOccupied.x = Mathf.Min(minOccupied.x, x);
                        minOccupied.y = Mathf.Min(minOccupied.y, y);
                        minOccupied.z = Mathf.Min(minOccupied.z, z);

                        maxOccupied.x = Mathf.Max(maxOccupied.x, x);
                        maxOccupied.y = Mathf.Max(maxOccupied.y, y);
                        maxOccupied.z = Mathf.Max(maxOccupied.z, z);
                    }
                }
            }
        }

        occupiedMin = foundAny ? minOccupied : Vector3Int.zero;
        occupiedMax = foundAny ? maxOccupied : Vector3Int.zero;
    }


#endif
}


[Serializable]
public sealed class TableGridData
{
    [SerializeField]
    private List<GridDepthLayerData> layers =
        new List<GridDepthLayerData>();

    public List<GridDepthLayerData> Layers
    {
        get => layers;
        set => layers = value ?? new List<GridDepthLayerData>();
    }
}


[Serializable]
public sealed class GridDepthLayerData
{
    [SerializeField]
    private List<GridRowData> rows =
        new List<GridRowData>();


    public List<GridRowData> Rows =>
        rows;
}


[Serializable]
public sealed class GridRowData
{
    [SerializeField]
    private List<GridCellData> cells =
        new List<GridCellData>();


    public List<GridCellData> Cells =>
        cells;
}


[Serializable]
public sealed class GridCellData
{
    [SerializeField]
    private bool occupied;

    [SerializeField]
    private Color color =
        Color.white;

    [Tooltip(
        "GridLevelData.BlockPalette mein index — is cell ke liye " +
        "kaunsi shape (cube, cylinder, wagera) spawn hogi."
    )]
    [SerializeField]
    private int definitionIndex;

    [Tooltip(
        "GridLevelData.TablePlacements ka zero-based index. Block isi table ki surface par spawn hoga."
    )]
    [SerializeField, Min(0)]
    private int tableIndex;

    [Tooltip(
        "Sirf anchor cell par meaningful hai (1 se bara). Ye cell " +
        "X/Y/Z directions mein kitne grid cells cover karti hai — " +
        "jaise ek bara cylinder jo 2 cells vertically le."
    )]
    [SerializeField, Min(1)]
    private int spanX = 1;

    [SerializeField, Min(1)]
    private int spanY = 1;

    [SerializeField, Min(1)]
    private int spanZ = 1;

    [Tooltip(
        "ON: ye cell kisi bare (multi-cell) piece ke footprint ka " +
        "hissa hai lekin khud spawn nahi hogi — uska anchor cell " +
        "(AnchorX/Y/Z) hi spawn karta hai."
    )]
    [SerializeField]
    private bool isCovered;

    [SerializeField]
    private int anchorX = -1;

    [SerializeField]
    private int anchorY = -1;

    [SerializeField]
    private int anchorZ = -1;

    [Tooltip(
        "Sirf anchor cell par meaningful hai. Piece ko upright (jaisa " +
        "authored hai) rakhna hai ya 90 degree tip karke lying rakhna hai."
    )]
    [SerializeField]
    private PieceOrientation orientation =
        PieceOrientation.UprightY;

    [Tooltip(
        "Designer ka manually entered extra local Z-axis rotation angle."
    )]
    [SerializeField]
    private float customZRotation;

    [Tooltip(
        "Sirf anchor cell par meaningful hai. Piece ko apne normal grid " +
        "slot se cell-units mein hata deta hai — jaise brick-offset " +
        "stacking, jahan upar wala block do niche wale blocks ke beech " +
        "seam par (0.5 offset) khada hota hai, kisi ek cell ke exact " +
        "center par nahi."
    )]
    [SerializeField]
    private Vector3 localOffset =
        Vector3.zero;

    [Tooltip(
        "Orientation preset ke upar apply hone wali free XYZ rotation (degrees)."
    )]
    [SerializeField]
    private Vector3 rotationEulerOffset =
        Vector3.zero;

    [Tooltip(
        "Auto-fit size ka per-block multiplier. (1, 1, 1) normal size hai."
    )]
    [SerializeField]
    private Vector3 scaleMultiplier =
        Vector3.one;

    [Tooltip(
        "ON: cannon hit par ye placed block toot sakta hai."
    )]
    [SerializeField]
    private bool breakable;

    [Tooltip(
        "Block tootne se pehle kitne direct cannon hits chahiye."
    )]
    [SerializeField, Min(1)]
    private int hitsToBreak = 1;


    public bool Occupied =>
        occupied;

    public Color Color =>
        color;

    public int DefinitionIndex =>
        definitionIndex;

    public int TableIndex =>
        Mathf.Max(0, tableIndex);

    public int SpanX =>
        spanX;

    public int SpanY =>
        spanY;

    public int SpanZ =>
        spanZ;

    public bool IsCovered =>
        isCovered;

    public Vector3Int AnchorCoordinate =>
        new Vector3Int(anchorX, anchorY, anchorZ);

    public PieceOrientation Orientation =>
        orientation;

    public float CustomZRotation =>
        customZRotation;

    public Vector3 LocalOffset =>
        localOffset;

    public Vector3 RotationEulerOffset =>
        rotationEulerOffset;

    public Vector3 ScaleMultiplier =>
        scaleMultiplier.sqrMagnitude <= 0.000001f
            ? Vector3.one
            : new Vector3(
                Mathf.Max(0.01f, scaleMultiplier.x),
                Mathf.Max(0.01f, scaleMultiplier.y),
                Mathf.Max(0.01f, scaleMultiplier.z)
            );

    public bool Breakable =>
        breakable;

    public int HitsToBreak =>
        Mathf.Max(1, hitsToBreak);


    public GridCellData()
    {
    }


    public GridCellData(
        bool occupied,
        Color color,
        int definitionIndex = 0)
    {
        this.occupied =
            occupied;

        this.color =
            color;

        this.definitionIndex =
            definitionIndex;
    }


    public void SetOccupied(bool value)
    {
        occupied = value;
    }


    public void SetColor(Color value)
    {
        color = value;
    }


    public void SetDefinitionIndex(int value)
    {
        definitionIndex = value;
    }


    public void SetTableIndex(int value)
    {
        tableIndex = Mathf.Max(0, value);
    }


    public void SetSpan(
        int newSpanX,
        int newSpanY,
        int newSpanZ)
    {
        spanX = Mathf.Max(1, newSpanX);
        spanY = Mathf.Max(1, newSpanY);
        spanZ = Mathf.Max(1, newSpanZ);
    }


    public void SetCoveredBy(
        Vector3Int anchorCoordinate)
    {
        isCovered = true;

        anchorX = anchorCoordinate.x;
        anchorY = anchorCoordinate.y;
        anchorZ = anchorCoordinate.z;
    }


    public void SetOrientation(
        PieceOrientation value)
    {
        orientation = value;
    }


    public void SetCustomZRotation(
        float value)
    {
        customZRotation = value;
    }


    public void SetLocalOffset(
        Vector3 value)
    {
        localOffset = value;
    }


    public void SetRotationEulerOffset(
        Vector3 value)
    {
        rotationEulerOffset = value;
    }


    public void SetScaleMultiplier(
        Vector3 value)
    {
        scaleMultiplier =
            new Vector3(
                Mathf.Max(0.01f, value.x),
                Mathf.Max(0.01f, value.y),
                Mathf.Max(0.01f, value.z)
            );
    }


    public void SetBreakable(
        bool value,
        int requiredHits = 1)
    {
        breakable = value;
        hitsToBreak = Mathf.Max(1, requiredHits);
    }


    /// <summary>
    /// Resets a cell back to a plain, independent (non-spanning,
    /// non-covered) state — used whenever the painter overwrites/erases
    /// a cell so it doesn't keep stale span/anchor data around.
    /// </summary>
    public void ClearSpanState()
    {
        tableIndex = 0;

        spanX = 1;
        spanY = 1;
        spanZ = 1;

        isCovered = false;

        anchorX = -1;
        anchorY = -1;
        anchorZ = -1;

        orientation = PieceOrientation.UprightY;

        customZRotation = 0f;

        localOffset = Vector3.zero;

        rotationEulerOffset = Vector3.zero;

        scaleMultiplier = Vector3.one;

        breakable = false;
        hitsToBreak = 1;
    }
}
