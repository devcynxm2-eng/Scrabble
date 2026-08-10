using System;
using System.Collections.Generic;
using UnityEngine;

public enum GridImageMaskMode
{
    Alpha,
    Luminance
}

public enum GridCellColorMode
{
    SourceImage,
    Palette,
    SingleColor
}

[CreateAssetMenu(
    fileName = "GridLevel_001",
    menuName = "Royal Smash/Grid Level Data")]
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


    [Header("Grid Object")]

    [SerializeField]
    private PhysicsObjectDefinition objectDefinition;


    [Header("Source Image")]

    [SerializeField]
    private Texture2D sourceImage;


    [Header("3D Grid Resolution")]

    [SerializeField, Min(1)]
    private int gridWidth = 12;

    [SerializeField, Min(1)]
    private int gridHeight = 12;

    [SerializeField, Min(1)]
    private int gridDepth = 1;


    [Header("Image Mask")]

    [SerializeField]
    private GridImageMaskMode maskMode =
        GridImageMaskMode.Alpha;

    [SerializeField, Range(0f, 1f)]
    private float alphaThreshold = 0.10f;

    [SerializeField, Range(0f, 1f)]
    private float luminanceThreshold = 0.50f;

    [SerializeField]
    private bool occupiedWhenDark = true;

    [SerializeField, Range(0.01f, 1f)]
    private float cellCoverageThreshold = 0.20f;


    [Header("Structural Stability")]

    [Tooltip(
        "ON recommended. Image ko physically self-supporting " +
        "block structure mein convert karega."
    )]
    [SerializeField]
    private bool enableStructuralStability = true;

    [Tooltip(
        "Heart jaisi pointed shape ke narrow bottom rows " +
        "remove karke stable/wider foundation find karega."
    )]
    [SerializeField]
    private bool autoFindStableBase = true;

    [Tooltip(
        "Stable base mein kam az kam itne consecutive boxes hon."
    )]
    [SerializeField, Min(1)]
    private int minimumBaseCells = 4;

    [Tooltip(
        "Image ki maximum row width ka kitna percentage " +
        "foundation ke liye required ho. " +
        "0.60 - 0.75 recommended."
    )]
    [SerializeField, Range(0.1f, 1f)]
    private float minimumBaseWidthRatio = 0.65f;

    [Tooltip(
        "Selected stable base ke neeche image ka pointed/weak " +
        "portion remove hoga."
    )]
    [SerializeField]
    private bool cropBelowStableBase = true;

    [Tooltip(
        "Har upper block ke directly neeche support block " +
        "required hoga. Is se structure apne aap nahi girta."
    )]
    [SerializeField]
    private bool removeUnsupportedCells = true;

    [Tooltip(
        "Foundation row mein sirf largest connected block-run rakhein. " +
        "Disconnected tiny pieces remove ho jayenge."
    )]
    [SerializeField]
    private bool keepLargestBaseRunOnly = true;


    [Header("Grid Position")]

    [SerializeField]
    private Vector3 gridOffset =
        Vector3.zero;

    [SerializeField, Min(0f)]
    private float horizontalGap = 0f;

    [SerializeField, Min(0f)]
    private float verticalGap = 0f;

    [SerializeField, Min(0f)]
    private float depthGap = 0f;


    [Header("Colors")]

    [SerializeField]
    private GridCellColorMode colorMode =
        GridCellColorMode.Palette;

    [SerializeField]
    private Color singleColor =
        new Color(
            1f,
            0.25f,
            0.35f,
            1f
        );

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

    [SerializeField]
    private int colorSeed = 12345;


    [SerializeField, HideInInspector]
    private List<GridDepthLayerData> layers =
        new List<GridDepthLayerData>();


    [SerializeField, HideInInspector]
    private int bakedOccupiedCellCount;

    [SerializeField, HideInInspector]
    private int bakedOriginalOccupiedCellCount;

    [SerializeField, HideInInspector]
    private int bakedRemovedForStability;

    [SerializeField, HideInInspector]
    private int bakedStableBaseRow;

    [SerializeField, HideInInspector]
    private int bakedSourceWidth;

    [SerializeField, HideInInspector]
    private int bakedSourceHeight;

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

    public PhysicsObjectDefinition ObjectDefinition =>
        objectDefinition;

    public Texture2D SourceImage =>
        sourceImage;

    public int GridWidth =>
        gridWidth;

    public int GridHeight =>
        gridHeight;

    public int GridDepth =>
        gridDepth;

    public Vector3 GridOffset =>
        gridOffset;

    public float HorizontalGap =>
        horizontalGap;

    public float VerticalGap =>
        verticalGap;

    public float DepthGap =>
        depthGap;

    public int BakedOccupiedCellCount =>
        bakedOccupiedCellCount;

    public int BakedOriginalOccupiedCellCount =>
        bakedOriginalOccupiedCellCount;

    public int BakedRemovedForStability =>
        bakedRemovedForStability;

    public int BakedStableBaseRow =>
        bakedStableBaseRow;

    public int BakedSourceWidth =>
        bakedSourceWidth;

    public int BakedSourceHeight =>
        bakedSourceHeight;

    public IReadOnlyList<GridDepthLayerData> Layers =>
        layers;


    public bool HasValidBakedGrid
    {
        get
        {
            if (layers == null ||
                layers.Count != gridDepth)
            {
                return false;
            }

            if (bakedOccupiedCellCount <= 0)
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
            bakedOccupiedCellCount > 0;
    }


#if UNITY_EDITOR

    private struct SampledCell
    {
        public bool Occupied;
        public Color Color;
    }


    public void EditorBakeFromReadableSourceImage()
    {
        if (sourceImage == null)
        {
            Debug.LogError(
                "GridLevelData: Source Image missing hai.",
                this
            );

            return;
        }


        gridWidth =
            Mathf.Max(
                1,
                gridWidth
            );

        gridHeight =
            Mathf.Max(
                1,
                gridHeight
            );

        gridDepth =
            Mathf.Max(
                1,
                gridDepth
            );


        Color32[] pixels =
            sourceImage.GetPixels32();

        int textureWidth =
            sourceImage.width;

        int textureHeight =
            sourceImage.height;


        /*
         * STEP 1
         *
         * Image ko pehle RAW 2D occupancy mein sample karte hain.
         */
        SampledCell[,] sampled =
            new SampledCell[
                gridHeight,
                gridWidth
            ];


        bakedOriginalOccupiedCellCount =
            0;


        for (int y = 0;
             y < gridHeight;
             y++)
        {
            for (int x = 0;
                 x < gridWidth;
                 x++)
            {
                SampleImageCell(
                    pixels,
                    textureWidth,
                    textureHeight,
                    x,
                    y,
                    out bool occupied,
                    out Color sourceColor
                );


                sampled[y, x] =
                    new SampledCell
                    {
                        Occupied =
                            occupied,

                        Color =
                            GetBakedColor(
                                x,
                                y,
                                0,
                                sourceColor
                            )
                    };


                if (occupied)
                {
                    bakedOriginalOccupiedCellCount++;
                }
            }
        }


        /*
         * STEP 2
         *
         * Raw image mask ko stable physical structure
         * mein convert karte hain.
         */
        bool[,] stableMask =
            CreateStableMask(
                sampled
            );


        /*
         * STEP 3
         *
         * Final stable 2D mask ko Z depth mein repeat.
         */
        layers =
            new List<GridDepthLayerData>(
                gridDepth
            );


        bakedOccupiedCellCount =
            0;


        bool foundAnyOccupiedCell =
            false;


        Vector3Int minOccupied =
            new Vector3Int(
                int.MaxValue,
                int.MaxValue,
                int.MaxValue
            );


        Vector3Int maxOccupied =
            new Vector3Int(
                int.MinValue,
                int.MinValue,
                int.MinValue
            );


        for (int z = 0;
             z < gridDepth;
             z++)
        {
            GridDepthLayerData layer =
                new GridDepthLayerData();


            for (int y = 0;
                 y < gridHeight;
                 y++)
            {
                GridRowData row =
                    new GridRowData();


                for (int x = 0;
                     x < gridWidth;
                     x++)
                {
                    bool occupied =
                        stableMask[y, x];


                    Color color =
                        GetBakedColor(
                            x,
                            y,
                            z,
                            sampled[y, x].Color
                        );


                    /*
                     * Source image color mode ke case mein
                     * sampled color preserve karte hain.
                     */
                    if (colorMode ==
                        GridCellColorMode.SourceImage)
                    {
                        color =
                            sampled[y, x].Color;
                    }


                    GridCellData cell =
                        new GridCellData(
                            occupied,
                            color
                        );


                    row.Cells.Add(
                        cell
                    );


                    if (!occupied)
                    {
                        continue;
                    }


                    bakedOccupiedCellCount++;


                    foundAnyOccupiedCell =
                        true;


                    minOccupied.x =
                        Mathf.Min(
                            minOccupied.x,
                            x
                        );

                    minOccupied.y =
                        Mathf.Min(
                            minOccupied.y,
                            y
                        );

                    minOccupied.z =
                        Mathf.Min(
                            minOccupied.z,
                            z
                        );


                    maxOccupied.x =
                        Mathf.Max(
                            maxOccupied.x,
                            x
                        );

                    maxOccupied.y =
                        Mathf.Max(
                            maxOccupied.y,
                            y
                        );

                    maxOccupied.z =
                        Mathf.Max(
                            maxOccupied.z,
                            z
                        );
                }


                layer.Rows.Add(
                    row
                );
            }


            layers.Add(
                layer
            );
        }


        if (foundAnyOccupiedCell)
        {
            occupiedMin =
                minOccupied;

            occupiedMax =
                maxOccupied;
        }
        else
        {
            occupiedMin =
                Vector3Int.zero;

            occupiedMax =
                Vector3Int.zero;
        }


        /*
         * Original count 2D hai.
         *
         * Final count depth ke sath multiply hua hai.
         */
        int final2DCount =
            gridDepth > 0
                ? bakedOccupiedCellCount /
                  gridDepth
                : 0;


        bakedRemovedForStability =
            Mathf.Max(
                0,
                bakedOriginalOccupiedCellCount -
                final2DCount
            );


        bakedSourceWidth =
            textureWidth;

        bakedSourceHeight =
            textureHeight;


        Debug.Log(
            $"Grid Level {levelNumber} baked.\n" +
            $"Resolution: " +
            $"{gridWidth}x{gridHeight}x{gridDepth}\n" +
            $"Raw Image Cells: " +
            $"{bakedOriginalOccupiedCellCount}\n" +
            $"Stable Cells Per Layer: " +
            $"{final2DCount}\n" +
            $"Removed For Stability: " +
            $"{bakedRemovedForStability}\n" +
            $"Stable Base Row: " +
            $"{bakedStableBaseRow}",
            this
        );
    }


    private bool[,] CreateStableMask(
        SampledCell[,] sampled)
    {
        bool[,] mask =
            new bool[
                gridHeight,
                gridWidth
            ];


        /*
         * Stability OFF:
         * raw mask exactly copy.
         */
        if (!enableStructuralStability)
        {
            for (int y = 0;
                 y < gridHeight;
                 y++)
            {
                for (int x = 0;
                     x < gridWidth;
                     x++)
                {
                    mask[y, x] =
                        sampled[y, x].Occupied;
                }
            }


            bakedStableBaseRow =
                FindFirstOccupiedRow(
                    sampled
                );


            return mask;
        }


        int firstOccupiedRow =
            FindFirstOccupiedRow(
                sampled
            );


        if (firstOccupiedRow < 0)
        {
            bakedStableBaseRow =
                0;

            return mask;
        }


        /*
         * Pehle image ki widest connected row
         * calculate karte hain.
         */
        int maximumRun =
            1;


        for (int y = 0;
             y < gridHeight;
             y++)
        {
            maximumRun =
                Mathf.Max(
                    maximumRun,
                    GetLargestOccupiedRun(
                        sampled,
                        y,
                        out _,
                        out _
                    )
                );
        }


        int requiredBaseWidth =
            Mathf.Max(
                minimumBaseCells,
                Mathf.CeilToInt(
                    maximumRun *
                    minimumBaseWidthRatio
                )
            );


        requiredBaseWidth =
            Mathf.Clamp(
                requiredBaseWidth,
                1,
                gridWidth
            );


        int baseRow =
            firstOccupiedRow;


        if (autoFindStableBase)
        {
            bool foundBase =
                false;


            for (int y = firstOccupiedRow;
                 y < gridHeight;
                 y++)
            {
                int run =
                    GetLargestOccupiedRun(
                        sampled,
                        y,
                        out _,
                        out _
                    );


                if (run <
                    requiredBaseWidth)
                {
                    continue;
                }


                baseRow =
                    y;

                foundBase =
                    true;

                break;
            }


            if (!foundBase)
            {
                /*
                 * Required width na mile to
                 * widest available row ko foundation bana do.
                 */
                int bestRun =
                    -1;


                for (int y = firstOccupiedRow;
                     y < gridHeight;
                     y++)
                {
                    int run =
                        GetLargestOccupiedRun(
                            sampled,
                            y,
                            out _,
                            out _
                        );


                    if (run <= bestRun)
                    {
                        continue;
                    }


                    bestRun =
                        run;

                    baseRow =
                        y;
                }
            }
        }


        bakedStableBaseRow =
            baseRow;


        /*
         * FOUNDATION ROW
         */
        if (keepLargestBaseRunOnly)
        {
            GetLargestOccupiedRun(
                sampled,
                baseRow,
                out int baseStart,
                out int baseEnd
            );


            if (baseStart >= 0)
            {
                for (int x = baseStart;
                     x <= baseEnd;
                     x++)
                {
                    mask[baseRow, x] =
                        sampled[
                            baseRow,
                            x
                        ].Occupied;
                }
            }
        }
        else
        {
            for (int x = 0;
                 x < gridWidth;
                 x++)
            {
                mask[baseRow, x] =
                    sampled[
                        baseRow,
                        x
                    ].Occupied;
            }
        }


        /*
         * Agar cropping OFF ho,
         * raw lower cells retain kar sakte hain.
         *
         * Lekin stable standing ke liye
         * cropping recommended hai.
         */
        if (!cropBelowStableBase)
        {
            for (int y = 0;
                 y < baseRow;
                 y++)
            {
                for (int x = 0;
                     x < gridWidth;
                     x++)
                {
                    mask[y, x] =
                        sampled[
                            y,
                            x
                        ].Occupied;
                }
            }
        }


        /*
         * BOTTOM → TOP STRUCTURAL PASS
         *
         * Har upper box ko DIRECTLY BELOW
         * ek retained box chahiye.
         *
         * Is wajah se floating/unsupported
         * image pixels remove ho jayenge.
         */
        for (int y = baseRow + 1;
             y < gridHeight;
             y++)
        {
            for (int x = 0;
                 x < gridWidth;
                 x++)
            {
                if (!sampled[y, x].Occupied)
                {
                    mask[y, x] =
                        false;

                    continue;
                }


                if (!removeUnsupportedCells)
                {
                    mask[y, x] =
                        true;

                    continue;
                }


                bool hasDirectSupport =
                    mask[
                        y - 1,
                        x
                    ];


                mask[y, x] =
                    hasDirectSupport;
            }
        }


        return mask;
    }


    private int FindFirstOccupiedRow(
        SampledCell[,] sampled)
    {
        for (int y = 0;
             y < gridHeight;
             y++)
        {
            for (int x = 0;
                 x < gridWidth;
                 x++)
            {
                if (sampled[y, x].Occupied)
                {
                    return y;
                }
            }
        }


        return -1;
    }


    private int GetLargestOccupiedRun(
        SampledCell[,] sampled,
        int row,
        out int bestStart,
        out int bestEnd)
    {
        bestStart =
            -1;

        bestEnd =
            -1;


        int currentStart =
            -1;

        int bestLength =
            0;


        for (int x = 0;
             x < gridWidth;
             x++)
        {
            bool occupied =
                sampled[
                    row,
                    x
                ].Occupied;


            if (occupied)
            {
                if (currentStart < 0)
                {
                    currentStart =
                        x;
                }
            }


            bool runEnded =
                !occupied ||
                x == gridWidth - 1;


            if (!runEnded ||
                currentStart < 0)
            {
                continue;
            }


            int currentEnd =
                occupied &&
                x == gridWidth - 1
                    ? x
                    : x - 1;


            int length =
                currentEnd -
                currentStart +
                1;


            if (length > bestLength)
            {
                bestLength =
                    length;

                bestStart =
                    currentStart;

                bestEnd =
                    currentEnd;
            }


            currentStart =
                -1;
        }


        return bestLength;
    }


    private void SampleImageCell(
        Color32[] pixels,
        int textureWidth,
        int textureHeight,
        int gridX,
        int gridY,
        out bool occupied,
        out Color averageColor)
    {
        float normalizedStartX =
            gridX /
            (float)gridWidth;

        float normalizedEndX =
            (gridX + 1) /
            (float)gridWidth;


        float normalizedStartY =
            gridY /
            (float)gridHeight;

        float normalizedEndY =
            (gridY + 1) /
            (float)gridHeight;


        int startPixelX =
            Mathf.Clamp(
                Mathf.FloorToInt(
                    normalizedStartX *
                    textureWidth
                ),
                0,
                textureWidth - 1
            );


        int endPixelX =
            Mathf.Clamp(
                Mathf.CeilToInt(
                    normalizedEndX *
                    textureWidth
                ) - 1,
                startPixelX,
                textureWidth - 1
            );


        int startPixelY =
            Mathf.Clamp(
                Mathf.FloorToInt(
                    normalizedStartY *
                    textureHeight
                ),
                0,
                textureHeight - 1
            );


        int endPixelY =
            Mathf.Clamp(
                Mathf.CeilToInt(
                    normalizedEndY *
                    textureHeight
                ) - 1,
                startPixelY,
                textureHeight - 1
            );


        int sampleCount =
            0;

        int occupiedSampleCount =
            0;


        Vector4 colorAccumulator =
            Vector4.zero;


        for (int pixelY = startPixelY;
             pixelY <= endPixelY;
             pixelY++)
        {
            for (int pixelX = startPixelX;
                 pixelX <= endPixelX;
                 pixelX++)
            {
                int index =
                    pixelY *
                    textureWidth +
                    pixelX;


                Color32 sample =
                    pixels[index];


                sampleCount++;


                if (!IsPixelOccupied(sample))
                {
                    continue;
                }


                occupiedSampleCount++;


                colorAccumulator +=
                    new Vector4(
                        sample.r / 255f,
                        sample.g / 255f,
                        sample.b / 255f,
                        sample.a / 255f
                    );
            }
        }


        float coverage =
            sampleCount > 0
                ? occupiedSampleCount /
                  (float)sampleCount
                : 0f;


        occupied =
            coverage >=
            cellCoverageThreshold;


        if (occupiedSampleCount > 0)
        {
            Vector4 average =
                colorAccumulator /
                occupiedSampleCount;


            averageColor =
                new Color(
                    average.x,
                    average.y,
                    average.z,
                    1f
                );
        }
        else
        {
            averageColor =
                Color.white;
        }
    }


    private bool IsPixelOccupied(
        Color32 pixel)
    {
        switch (maskMode)
        {
            case GridImageMaskMode.Luminance:
            {
                float luminance =
                    (
                        0.2126f * pixel.r +
                        0.7152f * pixel.g +
                        0.0722f * pixel.b
                    ) /
                    255f;


                if (occupiedWhenDark)
                {
                    return
                        luminance <=
                        luminanceThreshold;
                }


                return
                    luminance >=
                    luminanceThreshold;
            }


            default:
            {
                float alpha =
                    pixel.a /
                    255f;


                return
                    alpha >=
                    alphaThreshold;
            }
        }
    }


    private Color GetBakedColor(
        int x,
        int y,
        int z,
        Color sourceColor)
    {
        switch (colorMode)
        {
            case GridCellColorMode.SourceImage:
            {
                sourceColor.a =
                    1f;

                return
                    sourceColor;
            }


            case GridCellColorMode.SingleColor:
            {
                Color result =
                    singleColor;

                result.a =
                    1f;

                return result;
            }


            default:
            {
                return
                    GetPaletteColor(
                        x,
                        y,
                        z
                    );
            }
        }
    }


    private Color GetPaletteColor(
        int x,
        int y,
        int z)
    {
        if (colorPalette == null ||
            colorPalette.Count == 0)
        {
            return Color.white;
        }


        unchecked
        {
            int hash =
                colorSeed;


            hash =
                hash * 397 ^
                x;

            hash =
                hash * 397 ^
                y;

            hash =
                hash * 397 ^
                z;


            if (hash == int.MinValue)
            {
                hash = 0;
            }


            hash =
                Mathf.Abs(
                    hash
                );


            int index =
                hash %
                colorPalette.Count;


            Color color =
                colorPalette[index];

            color.a =
                1f;


            return color;
        }
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


    public bool Occupied =>
        occupied;

    public Color Color =>
        color;


    public GridCellData()
    {
    }


    public GridCellData(
        bool occupied,
        Color color)
    {
        this.occupied =
            occupied;

        this.color =
            color;
    }
}