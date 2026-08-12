using UnityEngine;

public enum GridLevelArchetypeKind
{
    SteppedPyramid = 0,
    TwinTowers = 1,
    CastleGate = 2,
    Colonnade = 3,
    RoyalCrown = 4,
    AlternatingFort = 5,
    TripleSpire = 6,
    Skyline = 7,
    Hourglass = 8,
    TerracedTemple = 9
}


public enum GridLevelColorPattern
{
    Random,
    HorizontalBands,
    Alternating
}


[CreateAssetMenu(
    fileName = "LevelArchetype_",
    menuName = "Royal Smash/Level Archetype Template")]
public sealed class GridLevelArchetypeTemplate : ScriptableObject
{
    [Header("Playtested Source")]

    [SerializeField]
    private string displayName = "Winning Archetype";

    [Tooltip("Phase 1 ka tested manual level jis se defaults capture hue.")]
    [SerializeField]
    private GridLevelData sourceLevel;

    [SerializeField]
    private GridLevelArchetypeKind archetypeKind;

    [SerializeField]
    private bool approvedForGeneration;


    [Header("Difficulty Eligibility")]

    [SerializeField, Range(1, 10)]
    private int minimumDifficulty = 1;

    [SerializeField, Range(1, 10)]
    private int maximumDifficulty = 10;

    [SerializeField, Min(1f)]
    private float selectionWeight = 1f;


    [Header("Safe Variation Range")]

    [SerializeField]
    private Vector2Int rowRange = new Vector2Int(4, 8);

    [SerializeField]
    private Vector2Int baseWidthRange = new Vector2Int(5, 10);

    [SerializeField]
    private Vector2Int shapeTypeRange = new Vector2Int(1, 3);

    [SerializeField]
    private GridLevelColorPattern colorPattern =
        GridLevelColorPattern.HorizontalBands;

    [SerializeField]
    private Vector2 horizontalGapRange = new Vector2(0.01f, 0.04f);

    [SerializeField]
    private Vector2 verticalGapRange = Vector2.zero;

    [SerializeField, Range(1, 3)]
    private int maximumDepthLayers = 2;

    [SerializeField, Range(0f, 1f)]
    private float seamChanceAtHighDifficulty = 0.35f;

    [SerializeField, Range(0f, 1f)]
    private float twoTableChanceAtHighDifficulty = 0.1f;

    [SerializeField, Min(1)]
    private int twoTableMinimumLevel = 60;

    [SerializeField, TextArea(2, 5)]
    private string designerNotes = string.Empty;


    public string DisplayName =>
        string.IsNullOrWhiteSpace(displayName) ? name : displayName;

    public GridLevelData SourceLevel => sourceLevel;

    public GridLevelArchetypeKind ArchetypeKind => archetypeKind;

    public bool ApprovedForGeneration => approvedForGeneration;

    public int MinimumDifficulty =>
        Mathf.Min(minimumDifficulty, maximumDifficulty);

    public int MaximumDifficulty =>
        Mathf.Max(minimumDifficulty, maximumDifficulty);

    public float SelectionWeight => Mathf.Max(0.01f, selectionWeight);

    public Vector2Int RowRange => NormalizeRange(rowRange, 3, 16);

    public Vector2Int BaseWidthRange =>
        NormalizeRange(baseWidthRange, 2, 24);

    public Vector2Int ShapeTypeRange =>
        NormalizeRange(shapeTypeRange, 1, 8);

    public GridLevelColorPattern ColorPattern => colorPattern;

    public Vector2 HorizontalGapRange =>
        NormalizeFloatRange(horizontalGapRange, 0f, 0.25f);

    public Vector2 VerticalGapRange =>
        NormalizeFloatRange(verticalGapRange, 0f, 0.25f);

    public int MaximumDepthLayers =>
        Mathf.Clamp(maximumDepthLayers, 1, 3);

    public float SeamChanceAtHighDifficulty =>
        Mathf.Clamp01(seamChanceAtHighDifficulty);

    public float TwoTableChanceAtHighDifficulty =>
        Mathf.Clamp01(twoTableChanceAtHighDifficulty);

    public int TwoTableMinimumLevel =>
        Mathf.Max(1, twoTableMinimumLevel);

    public string DesignerNotes => designerNotes;


#if UNITY_EDITOR
    public void EditorCapture(
        GridLevelData source,
        GridLevelArchetypeKind kind)
    {
        sourceLevel = source;
        archetypeKind = kind;
        displayName = source != null &&
                      !string.IsNullOrWhiteSpace(source.ArchetypeTag)
            ? source.ArchetypeTag
            : kind.ToString();

        if (source != null &&
            source.TryGetOccupiedBounds(
                out Vector3Int minimum,
                out Vector3Int maximum))
        {
            int rows = maximum.y - minimum.y + 1;
            int width = maximum.x - minimum.x + 1;

            rowRange = new Vector2Int(
                Mathf.Max(3, rows - 1),
                Mathf.Min(16, rows + 2)
            );

            baseWidthRange = new Vector2Int(
                Mathf.Max(2, width - 1),
                Mathf.Min(24, width + 2)
            );

            maximumDepthLayers =
                Mathf.Clamp(source.GridDepth, 1, 3);

            int availableShapes = Mathf.Max(1, source.BlockPalette.Count);
            shapeTypeRange = new Vector2Int(
                1,
                Mathf.Min(availableShapes, 4)
            );

            horizontalGapRange = new Vector2(
                Mathf.Max(0f, source.HorizontalGap - 0.01f),
                source.HorizontalGap + 0.01f
            );

            verticalGapRange = new Vector2(
                Mathf.Max(0f, source.VerticalGap - 0.005f),
                source.VerticalGap + 0.005f
            );
        }
    }
#endif


    private static Vector2Int NormalizeRange(
        Vector2Int range,
        int absoluteMinimum,
        int absoluteMaximum)
    {
        int minimum = Mathf.Clamp(
            Mathf.Min(range.x, range.y),
            absoluteMinimum,
            absoluteMaximum
        );
        int maximum = Mathf.Clamp(
            Mathf.Max(range.x, range.y),
            minimum,
            absoluteMaximum
        );

        return new Vector2Int(minimum, maximum);
    }


    private static Vector2 NormalizeFloatRange(
        Vector2 range,
        float absoluteMinimum,
        float absoluteMaximum)
    {
        float minimum = Mathf.Clamp(
            Mathf.Min(range.x, range.y),
            absoluteMinimum,
            absoluteMaximum
        );
        float maximum = Mathf.Clamp(
            Mathf.Max(range.x, range.y),
            minimum,
            absoluteMaximum
        );

        return new Vector2(minimum, maximum);
    }


    private void OnValidate()
    {
        minimumDifficulty = Mathf.Clamp(minimumDifficulty, 1, 10);
        maximumDifficulty = Mathf.Clamp(maximumDifficulty, 1, 10);
        selectionWeight = Mathf.Max(0.01f, selectionWeight);
        rowRange = NormalizeRange(rowRange, 3, 16);
        baseWidthRange = NormalizeRange(baseWidthRange, 2, 24);
        shapeTypeRange = NormalizeRange(shapeTypeRange, 1, 8);
        horizontalGapRange =
            NormalizeFloatRange(horizontalGapRange, 0f, 0.25f);
        verticalGapRange =
            NormalizeFloatRange(verticalGapRange, 0f, 0.25f);
        maximumDepthLayers = Mathf.Clamp(maximumDepthLayers, 1, 3);
        twoTableMinimumLevel = Mathf.Max(1, twoTableMinimumLevel);
    }
}
