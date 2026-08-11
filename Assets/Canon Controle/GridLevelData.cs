using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class GridLevelData : ScriptableObject
{
    [Header("Level")]

    [SerializeField, Min(1)]
    private int levelNumber = 1;

    [SerializeField, Min(1)]
    private int availableBalls = 10;


    [Header("Table")]

    [SerializeField]
    private LevelTable tablePrefab;

    [SerializeField]
    private Vector3 tablePositionOffset =
        Vector3.zero;

    [SerializeField]
    private Vector3 tableRotationEuler =
        Vector3.zero;


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

    public LevelTable TablePrefab => tablePrefab;

    public Vector3 TablePositionOffset =>
        tablePositionOffset;

    public Vector3 TableRotationEuler =>
        tableRotationEuler;

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
            if (layers == null ||
                layers.Count != gridDepth)
            {
                return false;
            }

            for (int z = 0;
                 z < layers.Count;
                 z++)
            {
                GridDepthLayerData layer =
                    layers[z];

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
    }


    public GridCellData GetCell(
        int x,
        int y,
        int z)
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

        if (layers == null ||
            z >= layers.Count)
        {
            return null;
        }

        GridDepthLayerData layer =
            layers[z];

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


#if UNITY_EDITOR

    private void OnValidate()
    {
        gridWidth = Mathf.Max(1, gridWidth);
        gridHeight = Mathf.Max(1, gridHeight);
        gridDepth = Mathf.Max(1, gridDepth);

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

    public void EditorEnsureGridAllocated(
        bool duplicateLastDepthLayer = false)
    {
        gridWidth =
            Mathf.Max(1, gridWidth);

        gridHeight =
            Mathf.Max(1, gridHeight);

        gridDepth =
            Mathf.Max(1, gridDepth);

        List<GridDepthLayerData> oldLayers =
            layers;

        layers =
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

            layers.Add(layer);
        }

        RecalculateGridMetadata();
    }


    public void EditorSetDepthLayerCount(
        int newLayerCount)
    {
        gridDepth =
            Mathf.Clamp(newLayerCount, 1, 16);

        EditorEnsureGridAllocated(true);
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
        int sourceLayerIndex)
    {
        if (!IsGridAllocated ||
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

        GridDepthLayerData sourceLayer =
            layers[sourceLayerIndex];

        for (int z = 0; z < gridDepth; z++)
        {
            if (z == sourceLayerIndex)
            {
                continue;
            }

            layers[z] = CloneDepthLayer(
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
        int definitionIndex = 0)
    {
        EditorClearSpanGroupAt(x, y, z);

        GridCellData cell =
            GetCell(x, y, z);

        if (cell == null)
        {
            return;
        }

        cell.SetOccupied(occupied);
        cell.SetColor(color);
        cell.SetDefinitionIndex(Mathf.Max(0, definitionIndex));
        cell.ClearSpanState();
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
        float customZRotation = 0f)
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
                        z + dz
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
                        GetCell(x + dx, y + dy, z + dz);

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
        int z)
    {
        GridCellData cell =
            GetCell(x, y, z);

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
                anchorCoordinate.z
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
                            anchorCoordinate.z + dz
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


    public void EditorClearAllCells()
    {
        if (layers == null)
        {
            return;
        }

        foreach (GridDepthLayerData layer in layers)
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
        bool occupied = true)
    {
        for (int y = Mathf.Max(0, minY);
             y <= Mathf.Min(gridHeight - 1, maxY);
             y++)
        {
            for (int x = Mathf.Max(0, minX);
                 x <= Mathf.Min(gridWidth - 1, maxX);
                 x++)
            {
                EditorSetCell(x, y, z, occupied, color, definitionIndex);
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
        int definitionIndex = 0)
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
                EditorSetCell(x, y, z, true, rowColor, definitionIndex);
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
        float ringThickness = 1f)
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
                    EditorSetCell(x, y, z, true, color, definitionIndex);
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


    public bool Occupied =>
        occupied;

    public Color Color =>
        color;

    public int DefinitionIndex =>
        definitionIndex;

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


    /// <summary>
    /// Resets a cell back to a plain, independent (non-spanning,
    /// non-covered) state — used whenever the painter overwrites/erases
    /// a cell so it doesn't keep stale span/anchor data around.
    /// </summary>
    public void ClearSpanState()
    {
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
    }
}
