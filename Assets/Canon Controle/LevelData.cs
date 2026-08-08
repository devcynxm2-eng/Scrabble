//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public enum TowerColorPattern
//{
//    Alternating,
//    ByRow,
//    Randomized
//}

//[CreateAssetMenu(
//    fileName = "Level_001",
//    menuName = "Royal Smash/Level Data")]
//public sealed class LevelData : ScriptableObject
//{
//    [Header("Level")]

//    [SerializeField, Min(1)]
//    private int levelNumber = 1;

//    [SerializeField, Min(1)]
//    private int availableBalls = 10;


//    [Header("Table")]

//    [SerializeField]
//    private LevelTable tablePrefab;

//    [SerializeField]
//    private Vector3 tablePositionOffset =
//        Vector3.zero;

//    [SerializeField]
//    private Vector3 tableRotationEuler =
//        Vector3.zero;


//    [Header("Tower")]

//    [SerializeField]
//    private Vector3 towerOffset =
//        Vector3.zero;

//    [SerializeField, Min(0f)]
//    private float horizontalGap = 0f;

//    [SerializeField, Min(0f)]
//    private float verticalGap = 0f;


//    [Header("Tower Colors")]

//    [Tooltip(
//        "ON hone par tower objects automatically colorful honge."
//    )]
//    [SerializeField]
//    private bool colorizeTower = true;

//    [Tooltip(
//        "Alternating recommended hai. " +
//        "Is se adjacent boxes different colors mein aate hain."
//    )]
//    [SerializeField]
//    private TowerColorPattern colorPattern =
//        TowerColorPattern.Alternating;

//    [Tooltip(
//        "Bright mobile-game palette. " +
//        "Aap Inspector se colors change/add kar sakte hain."
//    )]
//    [SerializeField]
//    private List<Color> colorPalette =
//        new List<Color>
//        {
//            new Color(1.00f, 0.23f, 0.33f, 1f), // Red / Pink
//            new Color(0.10f, 0.58f, 1.00f, 1f), // Blue
//            new Color(1.00f, 0.78f, 0.12f, 1f), // Yellow
//            new Color(0.52f, 0.28f, 0.95f, 1f), // Purple
//            new Color(0.05f, 0.78f, 0.55f, 1f), // Green
//            new Color(1.00f, 0.45f, 0.12f, 1f)  // Orange
//        };

//    [Tooltip(
//        "Randomized pattern ke liye deterministic seed."
//    )]
//    [SerializeField]
//    private int colorSeed = 12345;


//    [Header("Rows - Bottom To Top")]

//    [Tooltip(
//        "Element 0 bottom row hai. " +
//        "Last element top row hoga."
//    )]
//    [SerializeField]
//    private List<LevelRowData> rows =
//        new List<LevelRowData>();


//    public int LevelNumber =>
//        levelNumber;

//    public int AvailableBalls =>
//        availableBalls;

//    public LevelTable TablePrefab =>
//        tablePrefab;

//    public Vector3 TablePositionOffset =>
//        tablePositionOffset;

//    public Vector3 TableRotationEuler =>
//        tableRotationEuler;

//    public Vector3 TowerOffset =>
//        towerOffset;

//    public float HorizontalGap =>
//        horizontalGap;

//    public float VerticalGap =>
//        verticalGap;

//    public IReadOnlyList<LevelRowData> Rows =>
//        rows;

//    public bool ColorizeTower =>
//        colorizeTower;


//    public Color GetObjectColor(
//        int rowIndex,
//        int objectIndex)
//    {
//        if (!colorizeTower ||
//            colorPalette == null ||
//            colorPalette.Count == 0)
//        {
//            return Color.white;
//        }

//        int colorIndex;

//        switch (colorPattern)
//        {
//            case TowerColorPattern.ByRow:

//                colorIndex =
//                    PositiveModulo(
//                        rowIndex,
//                        colorPalette.Count
//                    );

//                break;


//            case TowerColorPattern.Randomized:

//                colorIndex =
//                    GetDeterministicColorIndex(
//                        rowIndex,
//                        objectIndex
//                    );

//                break;


//            default:

//                /*
//                 * Recommended:
//                 *
//                 * Adjacent boxes aur next rows
//                 * automatically different colors.
//                 */
//                colorIndex =
//                    PositiveModulo(
//                        rowIndex + objectIndex,
//                        colorPalette.Count
//                    );

//                break;
//        }

//        return colorPalette[
//            colorIndex
//        ];
//    }


//    private int GetDeterministicColorIndex(
//        int rowIndex,
//        int objectIndex)
//    {
//        unchecked
//        {
//            int hash =
//                colorSeed;

//            hash =
//                (hash * 397) ^
//                levelNumber;

//            hash =
//                (hash * 397) ^
//                rowIndex;

//            hash =
//                (hash * 397) ^
//                objectIndex;

//            if (hash == int.MinValue)
//            {
//                hash = 0;
//            }

//            hash =
//                Mathf.Abs(hash);

//            return hash %
//                   colorPalette.Count;
//        }
//    }


//    private static int PositiveModulo(
//        int value,
//        int modulo)
//    {
//        if (modulo <= 0)
//        {
//            return 0;
//        }

//        int result =
//            value % modulo;

//        if (result < 0)
//        {
//            result += modulo;
//        }

//        return result;
//    }
//}


//[Serializable]
//public sealed class LevelRowData
//{
//    [Header("Object")]

//    [SerializeField]
//    private PhysicsObjectDefinition objectDefinition;

//    [SerializeField, Min(1)]
//    private int count = 1;


//    [Header("Row Offset")]

//    [SerializeField]
//    private float centerOffsetX = 0f;

//    [SerializeField]
//    private float depthOffset = 0f;


//    [Header("Rotation")]

//    [SerializeField]
//    private Vector3 rotationEuler =
//        Vector3.zero;


//    public PhysicsObjectDefinition ObjectDefinition =>
//        objectDefinition;

//    public int Count =>
//        count;

//    public float CenterOffsetX =>
//        centerOffsetX;

//    public float DepthOffset =>
//        depthOffset;

//    public Vector3 RotationEuler =>
//        rotationEuler;
//}





using System;
using System.Collections.Generic;
using UnityEngine;

public enum TowerColorPattern
{
    Alternating,
    ByRow,
    Randomized
}

[CreateAssetMenu(
    fileName = "Level_001",
    menuName = "Royal Smash/Level Data")]
public sealed class LevelData : ScriptableObject
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


    [Header("Tower")]

    [SerializeField]
    private Vector3 towerOffset =
        Vector3.zero;

    [SerializeField, Min(0f)]
    private float horizontalGap = 0f;

    [SerializeField, Min(0f)]
    private float verticalGap = 0f;


    [Header("Tower Colors")]

    [Tooltip(
        "ON hone par tower objects automatically colorful honge."
    )]
    [SerializeField]
    private bool colorizeTower = true;

    [Tooltip(
        "Alternating recommended hai. " +
        "Is se adjacent boxes different colors mein aate hain."
    )]
    [SerializeField]
    private TowerColorPattern colorPattern =
        TowerColorPattern.Alternating;

    [Tooltip(
        "Bright mobile-game palette. " +
        "Aap Inspector se colors change/add kar sakte hain."
    )]
    [SerializeField]
    private List<Color> colorPalette =
        new List<Color>
        {
            new Color(1.00f, 0.23f, 0.33f, 1f), // Red / Pink
            new Color(0.10f, 0.58f, 1.00f, 1f), // Blue
            new Color(1.00f, 0.78f, 0.12f, 1f), // Yellow
            new Color(0.52f, 0.28f, 0.95f, 1f), // Purple
            new Color(0.05f, 0.78f, 0.55f, 1f), // Green
            new Color(1.00f, 0.45f, 0.12f, 1f)  // Orange
        };

    [Tooltip(
        "Randomized pattern ke liye deterministic seed."
    )]
    [SerializeField]
    private int colorSeed = 12345;


    [Header("Rows - Bottom To Top")]

    [Tooltip(
        "Element 0 bottom row hai. " +
        "Last element top row hoga."
    )]
    [SerializeField]
    private List<LevelRowData> rows =
        new List<LevelRowData>();


    public int LevelNumber =>
        levelNumber;

    public int AvailableBalls =>
        availableBalls;

    public LevelTable TablePrefab =>
        tablePrefab;

    public Vector3 TablePositionOffset =>
        tablePositionOffset;

    public Vector3 TableRotationEuler =>
        tableRotationEuler;

    public Vector3 TowerOffset =>
        towerOffset;

    public float HorizontalGap =>
        horizontalGap;

    public float VerticalGap =>
        verticalGap;

    public IReadOnlyList<LevelRowData> Rows =>
        rows;

    public bool ColorizeTower =>
        colorizeTower;


    public Color GetObjectColor(
        int rowIndex,
        int objectIndex)
    {
        if (!colorizeTower ||
            colorPalette == null ||
            colorPalette.Count == 0)
        {
            return Color.white;
        }

        int colorIndex;

        switch (colorPattern)
        {
            case TowerColorPattern.ByRow:

                colorIndex =
                    PositiveModulo(
                        rowIndex,
                        colorPalette.Count
                    );

                break;


            case TowerColorPattern.Randomized:

                colorIndex =
                    GetDeterministicColorIndex(
                        rowIndex,
                        objectIndex
                    );

                break;


            default:

                /*
                 * Recommended:
                 *
                 * Adjacent boxes aur next rows
                 * automatically different colors.
                 */
                colorIndex =
                    PositiveModulo(
                        rowIndex + objectIndex,
                        colorPalette.Count
                    );

                break;
        }

        return colorPalette[
            colorIndex
        ];
    }


    private int GetDeterministicColorIndex(
        int rowIndex,
        int objectIndex)
    {
        unchecked
        {
            int hash =
                colorSeed;

            hash =
                (hash * 397) ^
                levelNumber;

            hash =
                (hash * 397) ^
                rowIndex;

            hash =
                (hash * 397) ^
                objectIndex;

            if (hash == int.MinValue)
            {
                hash = 0;
            }

            hash =
                Mathf.Abs(hash);

            return hash %
                   colorPalette.Count;
        }
    }


    private static int PositiveModulo(
        int value,
        int modulo)
    {
        if (modulo <= 0)
        {
            return 0;
        }

        int result =
            value % modulo;

        if (result < 0)
        {
            result += modulo;
        }

        return result;
    }
}


[Serializable]
public sealed class LevelRowData
{
    [Header("Object")]

    [SerializeField]
    private PhysicsObjectDefinition objectDefinition;

    [SerializeField, Min(1)]
    private int count = 1;


    [Header("Row Offset")]

    [SerializeField]
    private float centerOffsetX = 0f;

    [SerializeField]
    private float depthOffset = 0f;


    [Header("Rotation")]

    [SerializeField]
    private Vector3 rotationEuler =
        Vector3.zero;


    public PhysicsObjectDefinition ObjectDefinition =>
        objectDefinition;

    public int Count =>
        count;

    public float CenterOffsetX =>
        centerOffsetX;

    public float DepthOffset =>
        depthOffset;

    public Vector3 RotationEuler =>
        rotationEuler;
}