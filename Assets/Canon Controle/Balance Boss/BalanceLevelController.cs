using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Balance boss level ka orchestrator.
///
/// Ye jaan boojh kar LevelRuntimeController se BILKUL alag hai. Balance
/// boss apni scene mein chalta hai jahan LevelRuntimeController maujood
/// NAHI hota — kyunke CannonController ke Shoot() aur AddExtraShots()
/// dono us par guard karte hain, aur uske na hone se wo guards khud
/// pass ho jate hain.
///
/// Is tarah maujooda 100 levels ka koi bhi code chhue baghair ye feature
/// kaam karta hai.
/// </summary>
[DisallowMultipleComponent]
public sealed class BalanceLevelController : MonoBehaviour
{
    public enum BalanceSide
    {
        Left = 0,
        Right = 1
    }


    [Header("Data")]

    [SerializeField]
    private BalanceLevelData levelData;


    [Header("Scene References")]

    [SerializeField]
    private BalanceBeam beam;

    [SerializeField]
    private CannonController cannonController;

    [Tooltip(
        "Spawned blocks ka parent. Khali ho to is GameObject ke neeche " +
        "ek root khud ban jayega."
    )]
    [SerializeField]
    private Transform towerRoot;


    [Header("Debug")]

    [SerializeField]
    private bool showDebugLogs = false;


    /* ---------- Events (UI in par bind kare) ---------- */

    /// <summary>Naya turn shuru. (playerIndex, turnNumber, ballsThisTurn)</summary>
    public event Action<int, int, int> TurnStarted;

    /// <summary>Har frame tilt update. (signedTilt, isWarning)</summary>
    public event Action<float, bool> TiltChanged;

    /// <summary>Score badla. (leftCleared, rightCleared)</summary>
    public event Action<int, int> ScoreChanged;

    /// <summary>Per-player score badla. (player0Score, player1Score)</summary>
    public event Action<int, int> PlayerScoreChanged;

    /// <summary>Balance gir gaya — level fail.</summary>
    public event Action BalanceLost;

    /// <summary>Level jeeta. (winnerPlayerIndex, -1 = tie / solo)</summary>
    public event Action<int> BalanceLevelWon;


    private readonly List<PhysicsTowerObject> leftBlocks =
        new List<PhysicsTowerObject>();

    private readonly List<PhysicsTowerObject> rightBlocks =
        new List<PhysicsTowerObject>();

    private readonly Dictionary<PhysicsTowerObject, BalanceSide> sideByBlock =
        new Dictionary<PhysicsTowerObject, BalanceSide>();

    private readonly int[] playerScores = new int[2];

    private int currentPlayerIndex;
    private int currentTurnNumber = 1;
    private int leftCleared;
    private int rightCleared;
    private float overTiltSeconds;
    private bool levelFinished;
    private bool levelStarted;


    public bool IsFinished => levelFinished;

    public int CurrentPlayerIndex => currentPlayerIndex;

    public int CurrentTurnNumber => currentTurnNumber;

    public float CurrentTilt =>
        beam != null ? beam.TiltAngle : 0f;

    public int LeftCleared => leftCleared;

    public int RightCleared => rightCleared;


    private void Awake()
    {
        ResolveReferences();
    }


    private void OnEnable()
    {
        if (cannonController != null)
        {
            cannonController.OutOfMoves -= HandleOutOfMoves;
            cannonController.OutOfMoves += HandleOutOfMoves;
        }
    }


    private void OnDisable()
    {
        if (cannonController != null)
        {
            cannonController.OutOfMoves -= HandleOutOfMoves;
        }
    }


    private void Start()
    {
        StartLevel();
    }


    private void OnDestroy()
    {
        UnsubscribeFromBlocks();
    }


    private void ResolveReferences()
    {
        if (beam == null)
        {
            beam = FindFirstObjectByType<BalanceBeam>(
                FindObjectsInactive.Include
            );
        }

        if (cannonController == null)
        {
            cannonController = FindFirstObjectByType<CannonController>(
                FindObjectsInactive.Include
            );
        }

        if (towerRoot == null)
        {
            GameObject rootObject =
                new GameObject("Balance Tower Root");

            rootObject.transform.SetParent(transform, false);
            towerRoot = rootObject.transform;
        }
    }


    /// <summary>
    /// Poora level shuru se set karta hai. Restart ke liye bhi yehi call
    /// karein.
    /// </summary>
    public void StartLevel()
    {
        if (levelData == null)
        {
            Debug.LogError(
                "BalanceLevelController: BalanceLevelData assign nahi hai.",
                this
            );

            return;
        }

        if (beam == null)
        {
            Debug.LogError(
                "BalanceLevelController: BalanceBeam scene mein nahi mila.",
                this
            );

            return;
        }

        ClearLevel();

        beam.ResetBeam();

        BuildTower(BalanceSide.Left, beam.LeftPlatform, leftBlocks);
        BuildTower(BalanceSide.Right, beam.RightPlatform, rightBlocks);

        levelFinished = false;
        levelStarted = true;
        overTiltSeconds = 0f;
        currentPlayerIndex = 0;
        currentTurnNumber = 1;
        leftCleared = 0;
        rightCleared = 0;
        playerScores[0] = 0;
        playerScores[1] = 0;

        RaiseScoreEvents();

        BeginTurn();
    }


    /// <summary>
    /// Spawned blocks hata deta hai. Restart se pehle chalta hai.
    /// </summary>
    public void ClearLevel()
    {
        UnsubscribeFromBlocks();

        DestroyBlocks(leftBlocks);
        DestroyBlocks(rightBlocks);

        sideByBlock.Clear();
        levelStarted = false;
    }


    private void DestroyBlocks(
        List<PhysicsTowerObject> blocks)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            PhysicsTowerObject block = blocks[i];

            if (block == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(block.gameObject);
            }
            else
            {
                DestroyImmediate(block.gameObject);
            }
        }

        blocks.Clear();
    }


    private void UnsubscribeFromBlocks()
    {
        foreach (KeyValuePair<PhysicsTowerObject, BalanceSide> entry
                 in sideByBlock)
        {
            if (entry.Key != null)
            {
                entry.Key.Cleared -= HandleBlockCleared;
            }
        }
    }


    /// <summary>
    /// Ek side ka tower banata hai. Blocks foran DYNAMIC hote hain taake
    /// unka wazan beam par pare — yehi is mechanic ki jaan hai.
    /// </summary>
    private void BuildTower(
        BalanceSide side,
        Transform platform,
        List<PhysicsTowerObject> target)
    {
        if (platform == null)
        {
            Debug.LogError(
                $"BalanceLevelController: {side} platform assign nahi hai.",
                this
            );

            return;
        }

        int width = levelData.TowerWidth;
        int height = levelData.TowerHeight;
        Vector3 cell = levelData.CellSize;

        /* Tower ko platform ke markaz par centre karte hain. */
        float startX =
            -((width - 1) * cell.x) * 0.5f;

        for (int row = 0; row < height; row++)
        {
            PhysicsObjectDefinition definition =
                levelData.GetRowDefinition(row);

            if (definition == null ||
                definition.Prefab == null)
            {
                Debug.LogError(
                    $"BalanceLevelController: row {row} ki definition ya " +
                    "uska prefab missing hai.",
                    this
                );

                continue;
            }

            for (int column = 0; column < width; column++)
            {
                Vector3 localPosition =
                    new Vector3(
                        startX + column * cell.x,
                        cell.y * (row + 0.5f),
                        0f
                    );

                PhysicsTowerObject block = Instantiate(
                    definition.Prefab,
                    platform
                );

                block.transform.localPosition = localPosition;
                block.transform.localRotation = Quaternion.identity;

                block.name =
                    $"{side} Block r{row} c{column}";

                block.PrepareForSpawn();

                if (definition.AutoFitToCell)
                {
                    FitBlockToCell(block, definition);
                }

                if (definition.TintWithPaintColor)
                {
                    block.RestorePrefabVisual();
                }

                /*
                 * Grid levels mein blocks locked (kinematic) spawn hote
                 * hain aur hit par unlock hote hain. Balance boss mein
                 * yeh nahi chalta: block ka wazan shuru se hi beam par
                 * hona chahiye, warna see-saw jhukega hi nahi.
                 */
                block.ActivatePhysics();

                block.Cleared += HandleBlockCleared;

                sideByBlock[block] = side;
                target.Add(block);

                /*
                 * Spawn ke baad block ko towerRoot ke neeche NAHI le
                 * jaate — usay platform ka child rehna hai taake beam
                 * jhukne par tower bhi uske saath jhuke.
                 */
            }
        }

        if (showDebugLogs)
        {
            Debug.Log(
                $"BalanceLevelController: {side} tower mein " +
                $"{target.Count} blocks bane.",
                this
            );
        }
    }


    /// <summary>
    /// Block ko cell size ke mutabiq scale karta hai, taake mukhtalif
    /// shapes (cube / cylinder) ek hi tower mein clean stack hon.
    ///
    /// Grid levels mein ye kaam LevelRuntimeController ka fit-cache karta
    /// hai. Balance boss us system se independent hai, is liye yahan
    /// seedha prefab ke physics bounds se scale nikalte hain.
    /// </summary>
    private void FitBlockToCell(
        PhysicsTowerObject block,
        PhysicsObjectDefinition definition)
    {
        if (!block.TryGetPhysicsBounds(out Bounds bounds))
        {
            return;
        }

        Vector3 size = bounds.size;

        if (size.x <= 0.0001f ||
            size.y <= 0.0001f ||
            size.z <= 0.0001f)
        {
            return;
        }

        Vector3 cell = levelData.CellSize;

        Vector3 currentScale = block.transform.localScale;

        Vector3 fitted = new Vector3(
            currentScale.x * (cell.x / size.x),
            currentScale.y * (cell.y / size.y),
            currentScale.z * (cell.z / size.z)
        );

        Vector3 multiplier = definition.ManualScaleMultiplier;

        block.transform.localScale = new Vector3(
            fitted.x * multiplier.x,
            fitted.y * multiplier.y,
            fitted.z * multiplier.z
        );
    }


    private void BeginTurn()
    {
        if (levelFinished ||
            cannonController == null)
        {
            return;
        }

        int ballsThisTurn = levelData.BallsPerTurn;

        /*
         * CannonController ke paas balls SET karne ka public API nahi,
         * sirf AddExtraShots() hai. Is liye kami poori karte hain.
         *
         * Pehle turn par remainingBalls scene ke 'Fallback Ball Limit'
         * se aati hain (kyunke is scene mein LevelRuntimeController nahi
         * hai), is liye wahan bhi yehi value rakhni chahiye.
         */
        int deficit =
            ballsThisTurn - cannonController.RemainingBalls;

        if (deficit > 0)
        {
            cannonController.AddExtraShots(deficit);
        }
        else if (deficit < 0)
        {
            Debug.LogWarning(
                "BalanceLevelController: cannon ke paas is turn ke liye " +
                $"zaroorat se {-deficit} zyada balls hain. Scene ki " +
                "CannonController par 'Fallback Ball Limit' ko " +
                $"{ballsThisTurn} kar dein.",
                this
            );
        }

        cannonController.SetGameplayActive(true);

        TurnStarted?.Invoke(
            currentPlayerIndex,
            currentTurnNumber,
            ballsThisTurn
        );

        if (showDebugLogs)
        {
            Debug.Log(
                $"Balance turn {currentTurnNumber} — " +
                $"Player {currentPlayerIndex + 1}",
                this
            );
        }
    }


    private void HandleOutOfMoves()
    {
        if (levelFinished ||
            !levelStarted)
        {
            return;
        }

        AdvanceTurn();
    }


    private void AdvanceTurn()
    {
        int playerCount = levelData.PlayerCount;

        currentPlayerIndex =
            (currentPlayerIndex + 1) % playerCount;

        /*
         * Har player ke ek ek turn ke baad hi turn number barhta hai.
         */
        if (currentPlayerIndex == 0)
        {
            currentTurnNumber++;
        }

        if (currentTurnNumber > levelData.TurnsPerPlayer)
        {
            /*
             * Saare turns khatam. Balance abhi tak qaim hai, is liye
             * player jeet gaya.
             */
            FinishLevelAsWin();

            return;
        }

        BeginTurn();
    }


    private void HandleBlockCleared(
        PhysicsTowerObject block)
    {
        if (block == null)
        {
            return;
        }

        block.Cleared -= HandleBlockCleared;

        if (!sideByBlock.TryGetValue(block, out BalanceSide side))
        {
            return;
        }

        sideByBlock.Remove(block);

        if (side == BalanceSide.Left)
        {
            leftCleared++;
            leftBlocks.Remove(block);
        }
        else
        {
            rightCleared++;
            rightBlocks.Remove(block);
        }

        /*
         * Block girane ka credit us player ko jata hai jiska turn chal
         * raha hai.
         */
        playerScores[currentPlayerIndex]++;

        RaiseScoreEvents();

        if (block.gameObject != null)
        {
            block.gameObject.SetActive(false);
        }

        if (leftBlocks.Count == 0 &&
            rightBlocks.Count == 0)
        {
            FinishLevelAsWin();
        }
    }


    private void RaiseScoreEvents()
    {
        ScoreChanged?.Invoke(leftCleared, rightCleared);

        PlayerScoreChanged?.Invoke(
            playerScores[0],
            playerScores[1]
        );
    }


    private void FixedUpdate()
    {
        if (levelFinished ||
            !levelStarted ||
            beam == null ||
            levelData == null)
        {
            return;
        }

        float tilt = beam.TiltAngle;
        float absoluteTilt = Mathf.Abs(tilt);

        TiltChanged?.Invoke(
            tilt,
            absoluteTilt >= levelData.WarningTiltAngle
        );

        if (absoluteTilt >= levelData.MaxTiltAngle)
        {
            overTiltSeconds += Time.fixedDeltaTime;

            if (overTiltSeconds >= levelData.TiltGraceSeconds)
            {
                FinishLevelAsLoss();
            }

            return;
        }

        /*
         * Player ne beam ko wapas limit ke andar le aaya — grace timer
         * reset ho jata hai.
         */
        overTiltSeconds = 0f;
    }


    private void FinishLevelAsLoss()
    {
        if (levelFinished)
        {
            return;
        }

        levelFinished = true;

        if (cannonController != null)
        {
            cannonController.SetGameplayActive(false);
        }

        if (showDebugLogs)
        {
            Debug.Log("Balance lost.", this);
        }

        BalanceLost?.Invoke();
    }


    private void FinishLevelAsWin()
    {
        if (levelFinished)
        {
            return;
        }

        levelFinished = true;

        if (cannonController != null)
        {
            cannonController.SetGameplayActive(false);
        }

        int winner = -1;

        if (levelData.PlayerCount > 1)
        {
            if (playerScores[0] > playerScores[1])
            {
                winner = 0;
            }
            else if (playerScores[1] > playerScores[0])
            {
                winner = 1;
            }
        }

        if (showDebugLogs)
        {
            Debug.Log(
                $"Balance level won. Winner = {winner}",
                this
            );
        }

        BalanceLevelWon?.Invoke(winner);
    }


    /// <summary>UI ke restart button ke liye.</summary>
    public void RestartLevel()
    {
        StartLevel();
    }
}
