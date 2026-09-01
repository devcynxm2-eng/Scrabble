using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Balance boss level ka data.
///
/// Ye jaan boojh kar GridLevelData se BILKUL alag rakha gaya hai. Balance
/// boss ka model mukhtalif hai (static table par grid ke bajaye ek jhukta
/// hua see-saw), aur alag asset rakhne se maujooda 100 levels aur
/// LevelRuntimeController ko chhune ki zaroorat nahi parti.
/// </summary>
[CreateAssetMenu(
    fileName = "BalanceBossLevel",
    menuName = "Royal Smash/Balance Boss Level")]
public sealed class BalanceLevelData : ScriptableObject
{
    [Header("Level")]

    [SerializeField, Min(1)]
    private int levelNumber = 1;

    [SerializeField]
    private string displayName = "Balance Boss";


    [Header("Players")]

    [Tooltip(
        "1 = solo practice, 2 = bari bari ek hi phone par (pass and play)."
    )]
    [SerializeField, Range(1, 2)]
    private int playerCount = 2;

    [Tooltip(
        "Har turn mein player ko kitni balls milti hain. IMPORTANT: scene " +
        "ki CannonController par 'Fallback Ball Limit' bhi yehi value " +
        "rakhein, taake pehla turn bhi theek shuru ho."
    )]
    [SerializeField, Min(1)]
    private int ballsPerTurn = 5;

    [Tooltip("Har player ko kitne turns milenge.")]
    [SerializeField, Min(1)]
    private int turnsPerPlayer = 3;


    [Header("Balance Rules")]

    [Tooltip(
        "Beam is angle se zyada jhuk gaya (aur grace time guzar gaya) to " +
        "level FAIL. Degrees mein."
    )]
    [SerializeField, Range(1f, 60f)]
    private float maxTiltAngle = 22f;

    [Tooltip(
        "Is angle ke baad UI warning dikhani chahiye (abhi fail nahi). " +
        "Max Tilt Angle se kam rakhein."
    )]
    [SerializeField, Range(1f, 60f)]
    private float warningTiltAngle = 14f;

    [Tooltip(
        "Max angle cross karne ke baad player ko sambhalne ke liye kitna " +
        "waqt milega. 0 = foran fail."
    )]
    [SerializeField, Min(0f)]
    private float tiltGraceSeconds = 0.75f;


    [Header("Towers")]

    [Tooltip("Har side ke tower mein kitne blocks chore mein.")]
    [SerializeField, Min(1)]
    private int towerWidth = 2;

    [Tooltip("Har side ke tower mein kitne blocks ooper.")]
    [SerializeField, Min(1)]
    private int towerHeight = 5;

    [Tooltip("Ek block ka footprint. Blocks isi spacing par stack honge.")]
    [SerializeField]
    private Vector3 cellSize =
        new Vector3(0.35f, 0.35f, 0.35f);

    [Tooltip(
        "Row ke hisaab se shapes. Row 0 sab se neeche. List khatam ho " +
        "jaye to wapas shuru se cycle hoti hai, is liye ek entry bhi " +
        "kaafi hai."
    )]
    [SerializeField]
    private List<PhysicsObjectDefinition> rowDefinitions =
        new List<PhysicsObjectDefinition>();


    public int LevelNumber => levelNumber;

    public string DisplayName =>
        string.IsNullOrEmpty(displayName)
            ? name
            : displayName;

    public int PlayerCount => Mathf.Clamp(playerCount, 1, 2);

    public int BallsPerTurn => Mathf.Max(1, ballsPerTurn);

    public int TurnsPerPlayer => Mathf.Max(1, turnsPerPlayer);

    public float MaxTiltAngle => maxTiltAngle;

    public float WarningTiltAngle =>
        Mathf.Min(warningTiltAngle, maxTiltAngle);

    public float TiltGraceSeconds => Mathf.Max(0f, tiltGraceSeconds);

    public int TowerWidth => Mathf.Max(1, towerWidth);

    public int TowerHeight => Mathf.Max(1, towerHeight);

    public Vector3 CellSize => cellSize;

    public IReadOnlyList<PhysicsObjectDefinition> RowDefinitions =>
        rowDefinitions;

    /// <summary>
    /// Diye gaye row ke liye shape. List chhoti ho to cycle ho jati hai.
    /// </summary>
    public PhysicsObjectDefinition GetRowDefinition(int row)
    {
        if (rowDefinitions == null ||
            rowDefinitions.Count == 0)
        {
            return null;
        }

        int index =
            Mathf.Abs(row) % rowDefinitions.Count;

        return rowDefinitions[index];
    }


    /// <summary>
    /// Dono side ke blocks ki mushtarak tadaad — score UI ke liye.
    /// </summary>
    public int BlocksPerSide =>
        TowerWidth * TowerHeight;
}
