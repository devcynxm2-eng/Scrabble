using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slope Ball ke ek level ki poori definition.
///
/// Tower ek painted grid hai: columns x rows x layers. Har cell mein
/// ya to kuch nahi hai (-1) ya palette ka koi object index hai, is liye
/// ek hi tower mein alag alag objects mix ho sakte hain.
///
/// Row 0 sab se neeche hai aur Layer 0 sab se aage (camera ke qareeb),
/// bilkul waise jaise runtime par tower banta hai.
///
/// Ye Canon wale GridLevelData se bilkul alag hai aur usse koi cheez
/// share nahi karta, taake Canon ka level system affect na ho.
/// </summary>
[CreateAssetMenu(
    fileName = "SlopeLevel_001",
    menuName = "Slope Ball/Slope Level",
    order = 0
)]
public sealed class SlopeLevelData : ScriptableObject
{
    public const int EmptyCell = -1;


    [Header("Level")]

    [SerializeField, Min(1)]
    private int levelNumber = 1;


    [Header("Grid")]

    [SerializeField, Min(1)]
    private int gridColumns = 5;


    [SerializeField, Min(1)]
    private int gridRows = 5;


    [Tooltip(
        "Tower ki depth. 2 rakhne par peeche ek aur layer ban jati " +
        "hai, yani tower do glass motta ho jata hai."
    )]
    [SerializeField, Range(1, 4)]
    private int gridLayers = 1;


    [Tooltip("Do layers ke darmiyan Z distance.")]
    [SerializeField]
    private float layerSpacing = 0.6f;


    /*
     * Har cell mein palette ka index hai, ya EmptyCell (-1).
     * Index formula: (layer * gridRows + row) * gridColumns + column
     */
    [SerializeField, HideInInspector]
    private List<int> cells = new List<int>();


    [Header("Objects")]

    [Tooltip(
        "Is level mein use hone wale objects. Painter mein inhi mein " +
        "se koi ek select karke cells par lagaya jata hai."
    )]
    [SerializeField]
    private List<BreakableGlass> objectPalette =
        new List<BreakableGlass>();


    [Header("Glass")]

    [Tooltip("Har generated object par lagne wala uniform scale.")]
    [SerializeField, Range(0.1f, 2f)]
    private float glassScale = 0.65f;


    [Tooltip("Do object centres ke darmiyan local distance.")]
    [SerializeField]
    private Vector2 spacing =
        new Vector2(0.635f, 1.285f);


    [Tooltip("Tower Root se tower base ka offset.")]
    [SerializeField]
    private Vector3 localOrigin =
        Vector3.zero;


    [Header("Collapse")]

    [Tooltip(
        "Support khatam hone aur agli unsupported row ke girne ke " +
        "darmiyan ka delay."
    )]
    [SerializeField, Min(0f)]
    private float collapseStepDelay = 0.08f;


    [Tooltip("Har upar wale glass ke neeche asli collider check karo.")]
    [SerializeField]
    private bool usePhysicalSupportCheck = true;


    [SerializeField, Min(0.01f)]
    private float physicalSupportDistance = 0.18f;


    [SerializeField, Min(0.005f)]
    private float physicalSupportProbeRadius = 0.04f;


    [Header("Ball")]

    [Tooltip("Ball kitni der baad khud khatam ho jayegi.")]
    [SerializeField, Min(0.1f)]
    private float ballLifetime = 4f;


    [SerializeField]
    private float minXPosition = -5f;


    [SerializeField]
    private float maxXPosition = 5f;


    [SerializeField, Min(0.01f)]
    private float minimumFallSpeed = 2f;


    [SerializeField, Min(0.01f)]
    private float maximumFallSpeed = 10f;


    [Tooltip(
        "Gravity ke oopar extra neeche ki taraf acceleration. Isse " +
        "ball floaty mehsoos nahi hoti."
    )]
    [SerializeField, Min(0f)]
    private float extraDownwardAcceleration = 12f;


    [Header("Power Meter")]

    [SerializeField, Min(0.01f)]
    private float powerChargeSpeed = 0.5f;


    [SerializeField, Range(0f, 1f)]
    private float minimumPower = 0.2f;


    [SerializeField, Range(0f, 1f)]
    private float maximumPower = 1f;


    public int LevelNumber => levelNumber;

    public int GridColumns => Mathf.Max(1, gridColumns);

    public int GridRows => Mathf.Max(1, gridRows);

    public int GridLayers => Mathf.Clamp(gridLayers, 1, 4);

    public float LayerSpacing => layerSpacing;

    public IReadOnlyList<int> Cells => cells;

    public IReadOnlyList<BreakableGlass> ObjectPalette => objectPalette;

    public float GlassScale => glassScale;

    public Vector2 Spacing => spacing;

    public Vector3 LocalOrigin => localOrigin;

    public float CollapseStepDelay => collapseStepDelay;

    public bool UsePhysicalSupportCheck => usePhysicalSupportCheck;

    public float PhysicalSupportDistance => physicalSupportDistance;

    public float PhysicalSupportProbeRadius => physicalSupportProbeRadius;

    public float BallLifetime => ballLifetime;

    public float MinXPosition => minXPosition;

    public float MaxXPosition => maxXPosition;

    public float MinimumFallSpeed => minimumFallSpeed;

    public float MaximumFallSpeed => maximumFallSpeed;

    public float ExtraDownwardAcceleration => extraDownwardAcceleration;

    public float PowerChargeSpeed => powerChargeSpeed;

    public float MinimumPower => minimumPower;

    public float MaximumPower => maximumPower;


    public int GetCellIndex(
        int layer,
        int row,
        int column)
    {
        return (layer * GridRows + row) * GridColumns + column;
    }


    /// <summary>
    /// Cell list ko grid ke current size ke mutabiq resize karta hai.
    /// Purani painting jitni fit ho sakti hai preserve hoti hai.
    /// </summary>
    public void EnsureCellCapacity()
    {
        int required =
            GridColumns * GridRows * GridLayers;

        if (cells.Count == required &&
            lastColumns == GridColumns &&
            lastRows == GridRows &&
            lastLayers == GridLayers)
        {
            return;
        }


        List<int> resized = new List<int>(required);

        for (int i = 0; i < required; i++)
        {
            resized.Add(EmptyCell);
        }


        /*
         * Purani dimensions se cell-by-cell copy karte hain, warna
         * resize ke baad poora design shift ho jata.
         */
        if (cells.Count > 0 &&
            lastColumns > 0 &&
            lastRows > 0 &&
            lastLayers > 0)
        {
            for (int layer = 0; layer < lastLayers; layer++)
            {
                for (int row = 0; row < lastRows; row++)
                {
                    for (int column = 0; column < lastColumns; column++)
                    {
                        if (layer >= GridLayers ||
                            row >= GridRows ||
                            column >= GridColumns)
                        {
                            continue;
                        }


                        int oldIndex =
                            (layer * lastRows + row) * lastColumns + column;

                        if (oldIndex < 0 ||
                            oldIndex >= cells.Count)
                        {
                            continue;
                        }


                        resized[GetCellIndex(layer, row, column)] =
                            cells[oldIndex];
                    }
                }
            }
        }


        cells = resized;

        lastColumns = GridColumns;
        lastRows = GridRows;
        lastLayers = GridLayers;
    }


    /*
     * Resize ke waqt purani grid ki dimensions chahiye hoti hain,
     * is liye unhein asset ke sath save karte hain.
     */
    [SerializeField, HideInInspector]
    private int lastColumns;

    [SerializeField, HideInInspector]
    private int lastRows;

    [SerializeField, HideInInspector]
    private int lastLayers;


    /// <summary>
    /// Cell par kaunsa palette object hai. Khali cell par EmptyCell.
    /// </summary>
    public int GetCell(
        int layer,
        int row,
        int column)
    {
        if (layer < 0 || row < 0 || column < 0 ||
            layer >= GridLayers ||
            row >= GridRows ||
            column >= GridColumns)
        {
            return EmptyCell;
        }


        int index = GetCellIndex(layer, row, column);

        return index < cells.Count ? cells[index] : EmptyCell;
    }


    public bool IsOccupied(
        int layer,
        int row,
        int column)
    {
        return GetCell(layer, row, column) != EmptyCell;
    }


    public void SetCell(
        int layer,
        int row,
        int column,
        int definitionIndex)
    {
        EnsureCellCapacity();


        if (layer < 0 || row < 0 || column < 0 ||
            layer >= GridLayers ||
            row >= GridRows ||
            column >= GridColumns)
        {
            return;
        }


        cells[GetCellIndex(layer, row, column)] =
            definitionIndex;
    }


    public int GetTotalGlassCount()
    {
        int total = 0;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i] != EmptyCell)
            {
                total++;
            }
        }

        return total;
    }


    /// <summary>
    /// Kisi layer ki kisi row mein kitne objects hain.
    /// </summary>
    public int GetRowCount(
        int layer,
        int row)
    {
        int total = 0;

        for (int column = 0; column < GridColumns; column++)
        {
            if (IsOccupied(layer, row, column))
            {
                total++;
            }
        }

        return total;
    }


    /// <summary>
    /// Cell ka palette object. Index galat ho ya palette khali ho
    /// to null, aur us soorat mein tower controller apna default
    /// glassPrefab use karta hai.
    /// </summary>
    public BreakableGlass GetPaletteObject(
        int definitionIndex)
    {
        if (definitionIndex < 0 ||
            definitionIndex >= objectPalette.Count)
        {
            return null;
        }


        return objectPalette[definitionIndex];
    }


    // ==========================
    // PRESETS
    // ==========================

    public void ClearAll()
    {
        EnsureCellCapacity();

        for (int i = 0; i < cells.Count; i++)
        {
            cells[i] = EmptyCell;
        }
    }


    public void ClearLayer(
        int layer)
    {
        EnsureCellCapacity();

        for (int row = 0; row < GridRows; row++)
        {
            for (int column = 0; column < GridColumns; column++)
            {
                SetCell(layer, row, column, EmptyCell);
            }
        }
    }


    public void FillLayer(
        int layer,
        int definitionIndex)
    {
        EnsureCellCapacity();

        for (int row = 0; row < GridRows; row++)
        {
            for (int column = 0; column < GridColumns; column++)
            {
                SetCell(layer, row, column, definitionIndex);
            }
        }
    }


    /// <summary>
    /// Centered pyramid paint karta hai.
    ///
    /// Har upar wali row donon taraf se ek ek object chhoti hoti hai
    /// (yani do kam), kyunke objects ek fixed lattice par baithte hain.
    /// Sirf ek kam karne se row lattice par center nahi ho sakti aur
    /// pyramid ek taraf jhuki hui lagti hai.
    /// </summary>
    public void ApplyPyramidPreset(
        int layer,
        int definitionIndex)
    {
        ClearLayer(layer);


        for (int row = 0; row < GridRows; row++)
        {
            int width = GridColumns - row * 2;

            if (width <= 0)
            {
                break;
            }


            int start = (GridColumns - width) / 2;

            for (int column = start; column < start + width; column++)
            {
                SetCell(layer, row, column, definitionIndex);
            }
        }
    }


    /// <summary>
    /// Ek layer ka design doosri layer par copy karta hai. Do layer
    /// wale tower jaldi banane ke liye.
    /// </summary>
    public void CopyLayer(
        int sourceLayer,
        int targetLayer)
    {
        if (sourceLayer == targetLayer)
        {
            return;
        }


        EnsureCellCapacity();


        for (int row = 0; row < GridRows; row++)
        {
            for (int column = 0; column < GridColumns; column++)
            {
                SetCell(
                    targetLayer,
                    row,
                    column,
                    GetCell(sourceLayer, row, column)
                );
            }
        }
    }
}
