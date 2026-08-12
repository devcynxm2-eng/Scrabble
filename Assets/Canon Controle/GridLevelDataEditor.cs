#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridLevelData))]
public sealed class GridLevelDataEditor : Editor
{
    private const string LevelDatabasePath =
        GridLevelAddressablesEditorUtility.DatabasePath;

    private const string LivePreviewSessionKey =
        "RoyalSmash.LiveWorkingLevelPreview";

    private static bool livePreviewRefreshQueued;

    private int paintLayerZ;
    private int paintTierY;
    private Color paintColor = Color.white;
    private int paintDefinitionIndex;
    private bool eraseMode;

    private PhysicsTowerObject prefabShapeToAdd;

    private int paintSpanX = 1;
    private int paintSpanY = 1;
    private int paintSpanZ = 1;
    private PieceOrientation paintOrientation = PieceOrientation.UprightY;
    private float paintCustomZRotation;
    private bool manualFootprintOverride;

    /// <summary>
    /// ON hone par piece apne grid slot ke center par nahi, do adjacent
    /// cells ke BEECH ki seam par baithti hai — brick-offset stacking
    /// (jaise real bricks) ke liye. X/Z dono directions mein alag se
    /// on/off ho sakta hai.
    /// </summary>
    private bool seamOffsetX;
    private bool seamOffsetZ;

    /// <summary>
    /// Live-measured footprint per shape, in-memory only (never
    /// serialized). Nothing to configure and nothing to go stale —
    /// re-measured automatically whenever a shape wasn't measured yet
    /// or this level's Cell Size changed since the last measurement.
    /// </summary>
    private readonly Dictionary<PhysicsObjectDefinition, Vector3Int>
        footprintCache =
            new Dictionary<PhysicsObjectDefinition, Vector3Int>();

    private Vector3 footprintCacheCellSize;

    private int presetBottomRowCount = 8;
    private int presetRowCount = 6;
    private float presetRingRadius = 4f;

    private int levelNumberToEdit = 1;

    private int autoDesignSeed = 12345;
    private int autoDesignDifficulty = 4;
    private int autoDesignGridWidth = 16;
    private int autoDesignRows = 7;
    private int autoDesignBaseWidth = 10;
    private int autoDesignLayers = 2;
    private float autoDesignLayerGap = 0.15f;
    private bool autoDesignUseSeams = true;
    private bool autoDesignAllowTwoTables = true;
    private float autoDesignTableGap = 0.65f;
    private float autoDesignTwoTableForwardOffset = 0.8f;
    private int autoBatchStartLevel = 11;
    private int autoBatchLevelCount = 20;
    private bool autoBatchProgressDifficulty = true;

    private static readonly int[][] ProgressionArchetypes =
    {
        new[] { 0, 7 },
        new[] { 0, 1, 6, 7 },
        new[] { 0, 2, 3, 6, 9 },
        new[] { 2, 3, 4, 5, 8, 9 },
        new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }
    };
    private string autoDesignSummary;

    private const float CellButtonSize = 18f;


    public override void OnInspectorGUI()
    {
        GridLevelData levelData =
            (GridLevelData)target;

        EditorGUI.BeginChangeCheck();

        DrawLiveGamePreviewSection(levelData);

        DrawLevelCreatorSection(levelData);

        DrawDefaultInspector();

        DrawAutomaticLevelDesigner(levelData);

        DrawManualPainterSection(levelData);
        DrawSaveLevelSection(levelData);

        bool inspectorChanged =
            EditorGUI.EndChangeCheck();

        if (inspectorChanged &&
            Application.isPlaying &&
            SessionState.GetBool(
                LivePreviewSessionKey,
                false
            ))
        {
            QueueLiveGamePreviewRefresh(levelData);
        }
    }


    private void DrawAutomaticLevelDesigner(
        GridLevelData levelData)
    {
        EditorGUILayout.Space(16f);

        EditorGUILayout.LabelField(
            "Automatic Level Designer (Test)",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Controlled-random, support-valid tower current single " +
            "working grid par generate hota hai. Generator ten structural " +
            "families use karta hai, including pyramid, gates, pillars, " +
            "crown, forts, spires, skyline, hourglass aur temple. Same Seed " +
            "same design banata hai. Generate working design replace karega; " +
            "saved Addressable level tab tak change nahi hoga jab tak " +
            "SAVE button use na karein.",
            MessageType.Info
        );

        autoDesignSeed =
            EditorGUILayout.IntField(
                "Seed",
                autoDesignSeed
            );

        autoDesignDifficulty =
            EditorGUILayout.IntSlider(
                "Difficulty",
                autoDesignDifficulty,
                1,
                10
            );

        autoDesignGridWidth =
            EditorGUILayout.IntSlider(
                "Grid Width",
                autoDesignGridWidth,
                6,
                24
            );

        autoDesignRows =
            EditorGUILayout.IntSlider(
                "Tower Rows",
                autoDesignRows,
                3,
                16
            );

        autoDesignBaseWidth =
            EditorGUILayout.IntSlider(
                "Base Width",
                autoDesignBaseWidth,
                4,
                Mathf.Max(4, autoDesignGridWidth - 2)
            );

        autoDesignLayers =
            EditorGUILayout.IntSlider(
                "Depth Layers",
                autoDesignLayers,
                1,
                3
            );

        using (
            new EditorGUI.DisabledScope(
                autoDesignLayers <= 1))
        {
            autoDesignLayerGap =
                Mathf.Max(
                    0f,
                    EditorGUILayout.FloatField(
                        "Layer Gap",
                        autoDesignLayerGap
                    )
                );
        }

        autoDesignUseSeams =
            EditorGUILayout.Toggle(
                "Use Seam Brick Rows",
                autoDesignUseSeams
            );

        autoDesignAllowTwoTables =
            EditorGUILayout.Toggle(
                "Allow Two-Table Levels",
                autoDesignAllowTwoTables
            );

        using (
            new EditorGUI.DisabledScope(
                !autoDesignAllowTwoTables))
        {
            autoDesignTableGap =
                Mathf.Max(
                    0f,
                    EditorGUILayout.FloatField(
                        "Gap Between Tables",
                        autoDesignTableGap
                    )
                );

            autoDesignTwoTableForwardOffset =
                Mathf.Max(
                    0f,
                    EditorGUILayout.FloatField(
                        "Two Tables Front Shift",
                        autoDesignTwoTableForwardOffset
                    )
                );
        }

        bool hasValidShape = false;

        if (levelData.BlockPalette != null)
        {
            foreach (PhysicsObjectDefinition definition
                     in levelData.BlockPalette)
            {
                if (definition != null && definition.Prefab != null)
                {
                    hasValidShape = true;
                    break;
                }
            }
        }

        if (!hasValidShape)
        {
            EditorGUILayout.HelpBox(
                "Generator ke liye Block Palette mein kam az kam ek " +
                "valid prefab shape required hai.",
                MessageType.Warning
            );
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("NEW RANDOM SEED"))
        {
            autoDesignSeed =
                unchecked(
                    (int)System.DateTime.UtcNow.Ticks
                );
        }

        using (new EditorGUI.DisabledScope(!hasValidShape))
        {
            if (GUILayout.Button(
                    "GENERATE TEST DESIGN",
                    GUILayout.Height(38f)))
            {
                GenerateAutomaticLevel(
                    levelData,
                    false
                );
            }
        }

        EditorGUILayout.EndHorizontal();

        using (new EditorGUI.DisabledScope(!hasValidShape))
        {
            if (GUILayout.Button(
                    "GENERATE + SAVE AS CURRENT LEVEL",
                    GUILayout.Height(34f)))
            {
                GenerateAutomaticLevel(
                    levelData,
                    true
                );
            }
        }

        EditorGUILayout.Space(8f);

        EditorGUILayout.LabelField(
            "Production Batch Workflow",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Approved manual archetype templates, automatic stability " +
            "validation aur test-batch gate ke liye production pipeline " +
            "use karein. Purana direct batch intentionally locked hai.",
            MessageType.Info
        );

        if (GUILayout.Button(
                "OPEN LEVEL PRODUCTION PIPELINE",
                GUILayout.Height(38f)))
        {
            LevelProductionPipelineWindow.Open();
        }

        using (new EditorGUI.DisabledScope(true))
        {

        autoBatchStartLevel =
            Mathf.Max(
                1,
                EditorGUILayout.IntField(
                    "Start Level Number",
                    autoBatchStartLevel
                )
            );

        autoBatchLevelCount =
            EditorGUILayout.IntSlider(
                "Number Of Levels",
                autoBatchLevelCount,
                20,
                30
            );

        autoBatchProgressDifficulty =
            EditorGUILayout.Toggle(
                "Progress Difficulty",
                autoBatchProgressDifficulty
            );

        int batchEndLevel =
            autoBatchStartLevel +
            autoBatchLevelCount - 1;

        EditorGUILayout.HelpBox(
            $"Level {autoBatchStartLevel} se Level {batchEndLevel} tak " +
            "separate ScriptableObjects generate/save honge. Existing " +
            "levels in this range update/overwrite ho jayenge. Har level " +
            "ka layout uniqueness-check se verify hoga. Progress Difficulty " +
            "ON ho to Levels 1-10 simple one-table structures se start " +
            "honge; gates, seams, depth aur two-table challenges gradually " +
            "late phases mein unlock honge.",
            MessageType.Warning
        );

        using (new EditorGUI.DisabledScope(!hasValidShape))
        {
            if (GUILayout.Button(
                    $"GENERATE + SAVE {autoBatchLevelCount} LEVELS",
                    GUILayout.Height(42f)))
            {
                bool approved =
                    EditorUtility.DisplayDialog(
                        "Generate Batch Levels?",
                        $"Level {autoBatchStartLevel}–{batchEndLevel} " +
                        "generate honge. Is range ke existing saved " +
                        "levels overwrite ho sakte hain.",
                        "Generate Levels",
                        "Cancel"
                    );

                if (approved)
                {
                    GenerateAutomaticLevelBatch(levelData);
                    GUIUtility.ExitGUI();
                }
            }
        }

        }

        if (!string.IsNullOrEmpty(autoDesignSummary))
        {
            EditorGUILayout.HelpBox(
                autoDesignSummary,
                MessageType.None
            );
        }
    }


    private void GenerateAutomaticLevel(
        GridLevelData levelData,
        bool saveAfterGeneration)
    {
        serializedObject.ApplyModifiedProperties();

        Undo.RecordObject(
            levelData,
            "Generate Automatic Grid Level"
        );

        GridLevelProceduralGenerator.Settings settings =
            new GridLevelProceduralGenerator.Settings(
                autoDesignSeed,
                autoDesignDifficulty,
                autoDesignGridWidth,
                autoDesignRows,
                autoDesignBaseWidth,
                autoDesignLayers,
                autoDesignLayerGap,
                autoDesignUseSeams,
                -1,
                autoDesignAllowTwoTables &&
                PositiveModulo(autoDesignSeed, 10) == 1,
                autoDesignTableGap,
                autoDesignTwoTableForwardOffset
            );

        bool generated =
            GridLevelProceduralGenerator.Generate(
                levelData,
                settings,
                out autoDesignSummary
            );

        if (!generated)
        {
            Debug.LogWarning(autoDesignSummary, levelData);
            return;
        }

        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssetIfDirty(levelData);

        if (saveAfterGeneration)
        {
            SaveWorkingLevel(
                levelData,
                GetOrCreateLevelDatabase()
            );
        }

        if (Application.isPlaying &&
            SessionState.GetBool(
                LivePreviewSessionKey,
                false
            ))
        {
            QueueLiveGamePreviewRefresh(levelData);
        }

        serializedObject.Update();

        Debug.Log(autoDesignSummary, levelData);
    }


    private void GenerateAutomaticLevelBatch(
        GridLevelData levelData)
    {
        serializedObject.ApplyModifiedProperties();

        Undo.RecordObject(
            levelData,
            "Generate Automatic Level Batch"
        );

        GridLevelDatabase database =
            GetOrCreateLevelDatabase();

        int requestedCount =
            Mathf.Clamp(autoBatchLevelCount, 20, 30);

        int generatedCount = 0;
        int firstLevel = Mathf.Max(1, autoBatchStartLevel);
        string lastGenerationSummary = string.Empty;
        HashSet<string> structuralSignatures =
            new HashSet<string>();

        int requestedLastLevel =
            firstLevel + requestedCount - 1;

        /*
         * Partial continuation (e.g. start at Level 24) bhi earlier
         * saved levels ke layouts repeat nahi karegi. Target overwrite
         * range ko intentionally preload nahi karte.
         */
        for (int databaseIndex = 0;
             databaseIndex < database.Count;
             databaseIndex++)
        {
            int savedLevelNumber =
                database.GetLevelNumber(databaseIndex);

            if (savedLevelNumber >= firstLevel &&
                savedLevelNumber <= requestedLastLevel)
            {
                continue;
            }

            GridLevelData savedLevel =
                GridLevelAddressablesEditorUtility.GetLevelAsset(
                    database,
                    savedLevelNumber
                );

            if (savedLevel != null)
            {
                structuralSignatures.Add(
                    BuildStructuralSignature(savedLevel)
                );
            }
        }

        try
        {
            for (int i = 0; i < requestedCount; i++)
            {
                int levelNumber = firstLevel + i;

                EditorUtility.DisplayProgressBar(
                    "Automatic Level Designer",
                    $"Generating Level {levelNumber} " +
                    $"({i + 1}/{requestedCount})",
                    (float)i / requestedCount
                );

                int levelSeed =
                    unchecked(
                        autoDesignSeed +
                        i * 104729
                    );

                int levelDifficulty =
                    autoBatchProgressDifficulty
                        ? GetProgressionDifficulty(levelNumber)
                        : autoDesignDifficulty;

                int levelRows =
                    autoBatchProgressDifficulty
                        ? GetProgressionRows(levelNumber, levelSeed)
                        : Mathf.Clamp(
                            autoDesignRows +
                            PositiveModulo(levelSeed, 5) - 2,
                            4,
                            10
                        );

                int levelBaseWidth =
                    autoBatchProgressDifficulty
                        ? GetProgressionBaseWidth(
                            levelNumber,
                            levelSeed,
                            autoDesignGridWidth
                        )
                        : Mathf.Clamp(
                            autoDesignBaseWidth +
                            PositiveModulo(levelSeed / 7, 5) - 2,
                            4,
                            autoDesignGridWidth - 2
                        );

                int levelLayers =
                    autoBatchProgressDifficulty
                        ? GetProgressionLayers(
                            levelNumber,
                            autoDesignLayers
                        )
                        : autoDesignLayers;

                bool levelUsesSeams =
                    autoDesignUseSeams &&
                    (!autoBatchProgressDifficulty || levelNumber >= 21);

                bool levelUsesTwoTables =
                    autoDesignAllowTwoTables &&
                    (!autoBatchProgressDifficulty
                        ? PositiveModulo(levelNumber, 7) == 2
                        : IsProgressionTwoTableLevel(levelNumber));

                levelData.EditorSetLevelNumber(levelNumber);

                bool generated = false;

                const int maximumUniqueAttempts = 120;

                for (int attempt = 0;
                     attempt < maximumUniqueAttempts;
                     attempt++)
                {
                    int retryGroup =
                        attempt / Mathf.Max(
                            1,
                            GetProgressionArchetypeCount(levelNumber)
                        );

                    int retryRows =
                        Mathf.Clamp(
                            levelRows +
                            GetBalancedRetryVariation(
                                retryGroup * 3
                            ),
                            4,
                            10
                        );

                    int retryBaseWidth =
                        Mathf.Clamp(
                            levelBaseWidth +
                            GetBalancedRetryVariation(
                                retryGroup
                            ),
                            4,
                            autoDesignGridWidth - 2
                        );

                    GridLevelProceduralGenerator.Settings settings =
                        new GridLevelProceduralGenerator.Settings(
                            unchecked(
                                levelSeed + attempt * 48611
                            ),
                            levelDifficulty,
                            autoDesignGridWidth,
                            retryRows,
                            retryBaseWidth,
                            levelLayers,
                            autoDesignLayerGap,
                            levelUsesSeams,
                            autoBatchProgressDifficulty
                                ? GetProgressionArchetypeIndex(
                                    levelNumber,
                                    attempt
                                )
                                : levelNumber - 1 + attempt,
                            levelUsesTwoTables,
                            autoDesignTableGap,
                            autoDesignTwoTableForwardOffset
                        );

                    generated =
                        GridLevelProceduralGenerator.Generate(
                            levelData,
                            settings,
                            out lastGenerationSummary
                        );

                    if (!generated)
                    {
                        break;
                    }

                    string signature =
                        BuildStructuralSignature(levelData);

                    if (structuralSignatures.Add(signature))
                    {
                        break;
                    }

                    generated = false;
                    lastGenerationSummary =
                        "Duplicate structure detected; trying another variant.";
                }

                if (!generated)
                {
                    Debug.LogError(
                        $"Batch stopped at Level {levelNumber}: " +
                        lastGenerationSummary +
                        " 120 cross-pattern variants try kiye gaye.",
                        levelData
                    );
                    break;
                }

                EditorUtility.SetDirty(levelData);

                SaveWorkingLevel(
                    levelData,
                    database
                );

                generatedCount++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(levelData);
            AssetDatabase.SaveAssets();
        }

        int lastSavedLevel =
            generatedCount > 0
                ? firstLevel + generatedCount - 1
                : firstLevel;

        autoDesignSummary =
            generatedCount == requestedCount
                ? $"Batch complete: Level {firstLevel}–{lastSavedLevel} " +
                  $"({generatedCount} levels) generate, save aur " +
                  "Addressable register ho gaye. Working canvas par " +
                  $"Level {lastSavedLevel} open hai."
                : $"Batch incomplete: {generatedCount}/{requestedCount} " +
                  "levels saved. " + lastGenerationSummary;

        serializedObject.Update();
        Debug.Log(autoDesignSummary, levelData);

        if (Application.isPlaying &&
            SessionState.GetBool(
                LivePreviewSessionKey,
                false
            ))
        {
            QueueLiveGamePreviewRefresh(levelData);
        }
    }


    private static int PositiveModulo(
        int value,
        int modulus)
    {
        int result = value % modulus;
        return result < 0 ? result + modulus : result;
    }


    private static int GetBalancedRetryVariation(int index)
    {
        if (index <= 0)
        {
            return 0;
        }

        int magnitude = (index + 1) / 2;

        return (index & 1) == 1
            ? -magnitude
            : magnitude;
    }


    private static int GetProgressionPhase(int levelNumber)
    {
        if (levelNumber <= 10)
        {
            return 0;
        }

        if (levelNumber <= 25)
        {
            return 1;
        }

        if (levelNumber <= 45)
        {
            return 2;
        }

        if (levelNumber <= 70)
        {
            return 3;
        }

        return 4;
    }


    private static int GetProgressionDifficulty(int levelNumber)
    {
        return Mathf.Clamp(
            1 + Mathf.FloorToInt(
                Mathf.Clamp01((levelNumber - 1) / 99f) * 9f
            ),
            1,
            10
        );
    }


    private static int GetProgressionRows(
        int levelNumber,
        int seed)
    {
        int phase = GetProgressionPhase(levelNumber);
        int[] minimumRows = { 4, 5, 6, 7, 8 };
        int[] maximumRows = { 5, 6, 7, 9, 10 };

        return minimumRows[phase] +
               PositiveModulo(
                   seed / 11,
                   maximumRows[phase] - minimumRows[phase] + 1
               );
    }


    private static int GetProgressionBaseWidth(
        int levelNumber,
        int seed,
        int gridWidth)
    {
        int phase = GetProgressionPhase(levelNumber);
        int[] minimumWidth = { 5, 6, 7, 8, 9 };
        int[] maximumWidth = { 7, 8, 10, 12, 14 };
        int safeMaximum = Mathf.Min(
            maximumWidth[phase],
            gridWidth - 2
        );
        int safeMinimum = Mathf.Min(
            minimumWidth[phase],
            safeMaximum
        );

        return safeMinimum +
               PositiveModulo(
                   seed / 17,
                   safeMaximum - safeMinimum + 1
               );
    }


    private static int GetProgressionLayers(
        int levelNumber,
        int maximumLayers)
    {
        if (levelNumber <= 15)
        {
            return 1;
        }

        if (levelNumber <= 35)
        {
            return Mathf.Min(maximumLayers, 2);
        }

        return Mathf.Clamp(maximumLayers, 1, 3);
    }


    private static bool IsProgressionTwoTableLevel(
        int levelNumber)
    {
        // Multi-table is a late, occasional challenge rather than filler.
        return levelNumber >= 60 &&
               (levelNumber == 60 ||
                levelNumber == 70 ||
                levelNumber == 80 ||
                levelNumber == 90 ||
                levelNumber == 100);
    }


    private static int GetProgressionArchetypeCount(
        int levelNumber)
    {
        return ProgressionArchetypes[
            GetProgressionPhase(levelNumber)
        ].Length;
    }


    private static int GetProgressionArchetypeIndex(
        int levelNumber,
        int attempt)
    {
        int[] allowed = ProgressionArchetypes[
            GetProgressionPhase(levelNumber)
        ];
        int sequence = Mathf.Max(0, levelNumber - 1) + attempt;
        int family = allowed[PositiveModulo(sequence, allowed.Length)];
        int variant = sequence / allowed.Length;

        return family +
               variant *
               GridLevelProceduralGenerator.StructuralPatternCount;
    }


    private static string BuildStructuralSignature(
        GridLevelData levelData)
    {
        StringBuilder signature = new StringBuilder(2048);

        signature.Append(levelData.UseSecondTable ? "T2|" : "T1|");
        signature.Append(levelData.GridWidth).Append('x');
        signature.Append(levelData.GridHeight).Append('x');
        signature.Append(levelData.GridDepth).Append('|');

        for (int z = 0; z < levelData.GridDepth; z++)
        {
            for (int y = 0; y < levelData.GridHeight; y++)
            {
                for (int x = 0; x < levelData.GridWidth; x++)
                {
                    GridCellData cell = levelData.GetCell(x, y, z);

                    if (cell == null ||
                        !cell.Occupied ||
                        cell.IsCovered)
                    {
                        continue;
                    }

                    signature
                        .Append(x).Append(',')
                        .Append(y).Append(',')
                        .Append(z).Append(':')
                        .Append(cell.SpanX).Append(',')
                        .Append(cell.SpanY).Append(',')
                        .Append(cell.SpanZ).Append(':')
                        .Append(
                            Mathf.RoundToInt(
                                cell.LocalOffset.x * 100f
                            )
                        ).Append(',')
                        .Append(
                            Mathf.RoundToInt(
                                cell.LocalOffset.z * 100f
                            )
                        ).Append(';');
                }
            }
        }

        return signature.ToString();
    }


    private void DrawLiveGamePreviewSection(
        GridLevelData levelData)
    {
        EditorGUILayout.LabelField(
            "Live Game Preview",
            EditorStyles.boldLabel
        );

        bool previewEnabled =
            SessionState.GetBool(
                LivePreviewSessionKey,
                false
            );

        EditorGUILayout.HelpBox(
            Application.isPlaying && previewEnabled
                ? "LIVE PREVIEW ACTIVE: Inspector mein design change " +
                  "karein; Game View automatically refresh hogi."
                : "Current working grid ko exact Game View mein dekhne " +
                  "aur Play Mode mein live edit karne ke liye preview start karein.",
            Application.isPlaying && previewEnabled
                ? MessageType.Info
                : MessageType.None
        );

        if (!Application.isPlaying)
        {
            if (GUILayout.Button(
                    "START LIVE GAME PREVIEW",
                    GUILayout.Height(42f)))
            {
                SessionState.SetBool(
                    LivePreviewSessionKey,
                    true
                );

                /*
                 * Working canvas disk par sync hota hai taa-ke Play Mode
                 * domain reload exact current design read kare. Central
                 * saved-level database sirf SAVE LEVEL se update hoti hai.
                 */
                EditorUtility.SetDirty(levelData);
                AssetDatabase.SaveAssetIfDirty(levelData);

                EditorApplication.EnterPlaymode();
                GUIUtility.ExitGUI();
            }
        }
        else
        {
            if (!previewEnabled)
            {
                if (GUILayout.Button(
                        "ENABLE CURRENT DESIGN PREVIEW",
                        GUILayout.Height(38f)))
                {
                    SessionState.SetBool(
                        LivePreviewSessionKey,
                        true
                    );

                    QueueLiveGamePreviewRefresh(levelData);
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("REFRESH GAME VIEW"))
                {
                    QueueLiveGamePreviewRefresh(levelData);
                }

                if (GUILayout.Button("STOP LIVE PREVIEW"))
                {
                    SessionState.SetBool(
                        LivePreviewSessionKey,
                        false
                    );

                    GridLevelDatabase database =
                        GetOrCreateLevelDatabase();

                    LevelRuntimeController controller =
                        FindFirstObjectByType<LevelRuntimeController>();

                    if (controller != null &&
                        database.Count > 0)
                    {
                        controller.LoadLevelByNumber(
                            database.GetLevelNumber(0)
                        );
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space(10f);
    }


    private static void QueueLiveGamePreviewRefresh(
        GridLevelData levelData)
    {
        if (livePreviewRefreshQueued ||
            levelData == null)
        {
            return;
        }

        livePreviewRefreshQueued = true;

        EditorApplication.delayCall += () =>
        {
            livePreviewRefreshQueued = false;

            if (!Application.isPlaying ||
                !SessionState.GetBool(
                    LivePreviewSessionKey,
                    false
                ))
            {
                return;
            }

            LevelRuntimeController controller =
                FindFirstObjectByType<LevelRuntimeController>();

            if (controller != null)
            {
                controller.LoadLevel(levelData);
            }
        };
    }


    private void DrawLevelCreatorSection(
        GridLevelData levelData)
    {
        GridLevelDatabase database =
            GetOrCreateLevelDatabase();

        GridLevelAddressablesEditorUtility
            .EnsurePerLevelAddressableAssets(database);

        EditorGUILayout.LabelField(
            "Level Creator",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            $"Saved Levels: {database.Count}\n" +
            "Yeh GridLevelData single working canvas hai. SAVE LEVEL " +
            "separate ScriptableObject create/update karke usay " +
            "Addressable banata hai.",
            MessageType.Info
        );

        levelNumberToEdit =
            Mathf.Max(
                1,
                EditorGUILayout.IntField(
                    "Level Number To Edit",
                    levelNumberToEdit
                )
            );

        using (
            new EditorGUI.DisabledScope(
                database.FindIndexByLevelNumber(
                    levelNumberToEdit
                ) < 0))
        {
            if (GUILayout.Button(
                    "LOAD SAVED LEVEL FOR EDIT"))
            {
                LoadSavedLevelForEdit(
                    levelData,
                    database,
                    levelNumberToEdit
                );

                serializedObject.Update();
                GUIUtility.ExitGUI();
            }
        }

        if (GUILayout.Button(
                "SAVE CURRENT + START NEXT LEVEL DESIGN",
                GUILayout.Height(42f)))
        {
            serializedObject.ApplyModifiedProperties();

            SaveWorkingLevel(
                levelData,
                database
            );

            StartNextLevelDesign(
                levelData,
                database
            );

            serializedObject.Update();
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.Space(10f);
    }


    private static void SaveWorkingLevel(
        GridLevelData workingLevel,
        GridLevelDatabase database)
    {
        if (workingLevel == null ||
            database == null)
        {
            return;
        }

        int levelNumber =
            Mathf.Max(1, workingLevel.LevelNumber);

        GridLevelData savedLevel =
            GridLevelAddressablesEditorUtility.SaveWorkingLevel(
                workingLevel,
                database,
                out bool isNewLevelAsset
            );

        if (savedLevel == null)
        {
            Debug.LogError(
                $"Level {levelNumber} save nahi ho saka.",
                workingLevel
            );
            return;
        }

        Debug.Log(
            isNewLevelAsset
                ? $"Level {levelNumber} separate Addressable asset mein saved."
                : $"Level {levelNumber} Addressable asset update ho gaya.",
            workingLevel
        );
    }


    private static void LoadSavedLevelForEdit(
        GridLevelData workingLevel,
        GridLevelDatabase database,
        int levelNumber)
    {
        GridLevelData savedLevel =
            GridLevelAddressablesEditorUtility.GetLevelAsset(
                database,
                levelNumber
            );

        if (savedLevel == null)
        {
            Debug.LogWarning(
                $"Saved Level {levelNumber} asset nahi mila.",
                database
            );

            return;
        }

        Undo.RecordObject(
            workingLevel,
            $"Load Level {levelNumber} For Edit"
        );

        string workingAssetName = workingLevel.name;

        EditorUtility.CopySerialized(
            savedLevel,
            workingLevel
        );

        workingLevel.name = workingAssetName;
        workingLevel.hideFlags = HideFlags.None;

        EditorUtility.SetDirty(workingLevel);

        Debug.Log(
            $"Level {levelNumber} loaded in single working grid.",
            workingLevel
        );
    }


    private static void StartNextLevelDesign(
        GridLevelData workingLevel,
        GridLevelDatabase database)
    {
        int nextLevelNumber = 1;

        for (int i = 0; i < database.Count; i++)
        {
            int savedLevelNumber = database.GetLevelNumber(i);

            if (savedLevelNumber > 0)
            {
                nextLevelNumber = Mathf.Max(
                    nextLevelNumber,
                    savedLevelNumber + 1
                );
            }
        }

        Undo.RecordObject(
            workingLevel,
            "Start Next Grid Level Design"
        );

        workingLevel.EditorSetLevelNumber(nextLevelNumber);
        workingLevel.EditorClearAllCells();
        EditorUtility.SetDirty(workingLevel);

        Debug.Log(
            $"Blank Level {nextLevelNumber} working grid par ready hai.",
            workingLevel
        );
    }


    private static GridLevelDatabase
        GetOrCreateLevelDatabase()
    {
        GridLevelDatabase database =
            AssetDatabase.LoadAssetAtPath<GridLevelDatabase>(
                LevelDatabasePath
            );

        if (database != null)
        {
            return database;
        }

        database =
            CreateInstance<GridLevelDatabase>();

        AssetDatabase.CreateAsset(
            database,
            LevelDatabasePath
        );

        AssetDatabase.SaveAssets();

        return database;
    }


    private void DrawSaveLevelSection(
        GridLevelData levelData)
    {
        EditorGUILayout.Space(16f);

        EditorGUILayout.LabelField(
            "Level Save",
            EditorStyles.boldLabel
        );

        string assetPath =
            AssetDatabase.GetAssetPath(levelData);

        EditorGUILayout.HelpBox(
            string.IsNullOrEmpty(assetPath)
                ? "Single working GridLevelData asset missing hai."
                : $"Single working grid:\n{assetPath}\n" +
                  $"SAVE LEVEL separate Addressable Level " +
                  $"{levelData.LevelNumber} asset create/update karega.",
            string.IsNullOrEmpty(assetPath)
                ? MessageType.Warning
                : MessageType.None
        );

        using (
            new EditorGUI.DisabledScope(
                string.IsNullOrEmpty(assetPath)))
        {
            if (GUILayout.Button(
                    "SAVE LEVEL",
                    GUILayout.Height(38f)))
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(levelData);

                SaveWorkingLevel(
                    levelData,
                    GetOrCreateLevelDatabase()
                );

                Debug.Log(
                    $"Level {levelData.LevelNumber} saved successfully.",
                    levelData
                );
            }
        }
    }




    private void DrawManualPainterSection(
        GridLevelData levelData)
    {
        EditorGUILayout.Space(20f);

        EditorGUILayout.LabelField(
            "Manual Level Painter",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Royal Smash jaisi hand-designed block/tier towers banane ke liye. " +
            "Grid cells ko directly paint karke level design karein.",
            MessageType.None
        );


        DrawDepthLayerSetup(levelData);

        if (GUILayout.Button(
                $"Allocate / Resize Grid To " +
                $"{levelData.GridWidth} x " +
                $"{levelData.GridHeight} x " +
                $"{levelData.GridDepth}"))
        {
            Undo.RecordObject(
                levelData,
                "Resize Grid Level"
            );

            levelData.EditorEnsureGridAllocated(true);

            EditorUtility.SetDirty(levelData);
        }

        DrawQuickShapeSetup(levelData);


        if (levelData.BlockPalette == null ||
            levelData.BlockPalette.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Pehle upar 'Block Palette' mein kam az kam ek shape " +
                "(Physics Object Definition) add karein — jaise Cube ya " +
                "Cylinder — phir paint kar sakenge.",
                MessageType.Warning
            );

            return;
        }

        if (!levelData.IsGridAllocated)
        {
            EditorGUILayout.HelpBox(
                "Pehle grid allocate karein taake paint kar sakein.",
                MessageType.Info
            );

            return;
        }


        EditorGUILayout.Space(6f);

        DrawShapePicker(levelData);

        EditorGUILayout.Space(6f);

        DrawSpanPicker(levelData);

        EditorGUILayout.Space(6f);

        DrawColorPicker(levelData);

        EditorGUILayout.Space(6f);

        DrawPresetTools(levelData);

        EditorGUILayout.Space(10f);

        DrawGridPainter(levelData);
    }


    private void DrawDepthLayerSetup(
        GridLevelData levelData)
    {
        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField(
            "Depth Layers",
            EditorStyles.boldLabel
        );

        int requestedLayerCount =
            EditorGUILayout.IntField(
                "Layer Count",
                levelData.GridDepth
            );

        requestedLayerCount =
            Mathf.Clamp(requestedLayerCount, 1, 16);

        if (requestedLayerCount != levelData.GridDepth)
        {
            Undo.RecordObject(
                levelData,
                "Change Grid Layer Count"
            );

            levelData.EditorSetDepthLayerCount(
                requestedLayerCount
            );

            paintLayerZ =
                Mathf.Clamp(
                    paintLayerZ,
                    0,
                    requestedLayerCount - 1
                );

            EditorUtility.SetDirty(levelData);
        }

        bool requestedMirror =
            EditorGUILayout.Toggle(
                new GUIContent(
                    "Mirror Paint To All Layers",
                    "ON: paint ya erase sab depth layers par same jagah apply hoga."
                ),
                levelData.MirrorPaintAcrossLayers
            );

        if (requestedMirror !=
            levelData.MirrorPaintAcrossLayers)
        {
            Undo.RecordObject(
                levelData,
                "Change Layer Paint Mirroring"
            );

            levelData.EditorSetMirrorPaintAcrossLayers(
                requestedMirror
            );

            EditorUtility.SetDirty(levelData);
        }

        float requestedLayerGap =
            EditorGUILayout.FloatField(
                new GUIContent(
                    "Layer Gap",
                    "Adjacent depth layers ke blocks ke darmiyan world-space gap."
                ),
                levelData.DepthGap
            );

        requestedLayerGap =
            Mathf.Max(0f, requestedLayerGap);

        if (!Mathf.Approximately(
                requestedLayerGap,
                levelData.DepthGap))
        {
            Undo.RecordObject(
                levelData,
                "Change Grid Layer Gap"
            );

            levelData.EditorSetDepthGap(
                requestedLayerGap
            );

            EditorUtility.SetDirty(levelData);
        }

        if (levelData.GridDepth > 1 &&
            levelData.IsGridAllocated &&
            GUILayout.Button(
                $"COPY LAYER {paintLayerZ + 1} TO ALL LAYERS"))
        {
            Undo.RecordObject(
                levelData,
                "Copy Grid Layer To All Layers"
            );

            levelData.EditorCopyDepthLayerToAll(
                paintLayerZ
            );

            EditorUtility.SetDirty(levelData);
        }

        EditorGUILayout.HelpBox(
            levelData.MirrorPaintAcrossLayers
                ? $"Mirror ON: ek layer par banaya design sab layers par show hoga. Current layer gap: {levelData.DepthGap:0.###}."
                : $"Mirror OFF: Layer buttons se har layer separately edit hogi. Current layer gap: {levelData.DepthGap:0.###}.",
            MessageType.Info
        );
    }


    private void DrawQuickShapeSetup(
        GridLevelData levelData)
    {
        EditorGUILayout.Space(10f);

        EditorGUILayout.LabelField(
            "Quick Add Prefab Shape",
            EditorStyles.boldLabel
        );

        prefabShapeToAdd =
            (PhysicsTowerObject)EditorGUILayout.ObjectField(
                "Shape Prefab",
                prefabShapeToAdd,
                typeof(PhysicsTowerObject),
                false
            );

        EditorGUILayout.HelpBox(
            "Drag a prefab whose root has PhysicsTowerObject, Rigidbody " +
            "and a non-trigger Collider. Its Renderer material and texture " +
            "are preserved automatically.",
            MessageType.None
        );

        if (prefabShapeToAdd != null)
        {
            Renderer[] renderers =
                prefabShapeToAdd.GetComponentsInChildren<Renderer>(true);

            int materialCount = 0;
            int texturedMaterialCount = 0;

            foreach (Renderer targetRenderer in renderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                foreach (Material material in targetRenderer.sharedMaterials)
                {
                    if (material == null)
                    {
                        continue;
                    }

                    materialCount++;

                    if (material.mainTexture != null)
                    {
                        texturedMaterialCount++;
                    }
                }
            }

            MessageType materialMessageType =
                texturedMaterialCount > 0
                    ? MessageType.Info
                    : MessageType.Warning;

            EditorGUILayout.HelpBox(
                $"Detected Renderers: {renderers.Length} | " +
                $"Materials: {materialCount} | " +
                $"Materials With Texture: {texturedMaterialCount}\n" +
                (texturedMaterialCount > 0
                    ? "Prefab ka existing material/texture automatically use hoga."
                    : "Prefab material par texture nahi mila. Texture chahiye to prefab Renderer ke material par assign karein."),
                materialMessageType
            );
        }

        using (new EditorGUI.DisabledScope(prefabShapeToAdd == null))
        {
            if (GUILayout.Button(
                    "CREATE DEFINITION + ADD TO BLOCK PALETTE",
                    GUILayout.Height(30f)))
            {
                CreateAndAddShapeDefinition(
                    levelData,
                    prefabShapeToAdd
                );
            }
        }
    }


    private void CreateAndAddShapeDefinition(
        GridLevelData levelData,
        PhysicsTowerObject shapePrefab)
    {
        if (levelData == null ||
            shapePrefab == null)
        {
            return;
        }

        Rigidbody body =
            shapePrefab.GetComponent<Rigidbody>();

        Collider[] colliders =
            shapePrefab.GetComponentsInChildren<Collider>(true);

        bool hasNonTriggerCollider =
            false;

        foreach (Collider targetCollider in colliders)
        {
            if (targetCollider != null &&
                targetCollider.enabled &&
                !targetCollider.isTrigger)
            {
                hasNonTriggerCollider = true;
                break;
            }
        }

        if (body == null ||
            !hasNonTriggerCollider)
        {
            EditorUtility.DisplayDialog(
                "Invalid Physics Shape Prefab",
                "The prefab root needs a Rigidbody and at least one " +
                "enabled non-trigger Collider on the root or a child.",
                "OK"
            );

            return;
        }

        string levelAssetPath =
            AssetDatabase.GetAssetPath(levelData);

        string folderPath =
            Path.GetDirectoryName(levelAssetPath);

        if (string.IsNullOrEmpty(folderPath))
        {
            folderPath = "Assets";
        }

        folderPath =
            folderPath.Replace('\\', '/');

        string definitionPath =
            AssetDatabase.GenerateUniqueAssetPath(
                $"{folderPath}/{shapePrefab.name}Definition.asset"
            );

        PhysicsObjectDefinition definition =
            CreateInstance<PhysicsObjectDefinition>();

        definition.EditorConfigure(
            shapePrefab,
            shapePrefab.name
        );

        AssetDatabase.CreateAsset(
            definition,
            definitionPath
        );

        Undo.RecordObject(
            levelData,
            "Add Prefab Shape To Block Palette"
        );

        levelData.EditorAddPaletteEntry(
            definition
        );

        EditorUtility.SetDirty(levelData);
        EditorUtility.SetDirty(definition);

        AssetDatabase.SaveAssets();

        paintDefinitionIndex =
            levelData.BlockPalette.Count - 1;

        footprintCache.Remove(definition);
        prefabShapeToAdd = null;

        Selection.activeObject = definition;

        Debug.Log(
            $"Shape definition created and added: {definitionPath}",
            levelData
        );
    }


    private void DrawShapePicker(
        GridLevelData levelData)
    {
        EditorGUILayout.LabelField(
            "Paint Shape",
            EditorStyles.boldLabel
        );

        paintDefinitionIndex =
            Mathf.Clamp(
                paintDefinitionIndex,
                0,
                levelData.BlockPalette.Count - 1
            );

        EditorGUILayout.BeginHorizontal();

        for (int i = 0;
             i < levelData.BlockPalette.Count;
             i++)
        {
            PhysicsObjectDefinition entry =
                levelData.BlockPalette[i];

            string label =
                entry != null
                    ? entry.DisplayName
                    : $"[{i}] missing";

            bool isSelected =
                i == paintDefinitionIndex;

            GUI.backgroundColor =
                isSelected
                    ? new Color(0.4f, 0.75f, 1f)
                    : Color.white;

            if (GUILayout.Toggle(
                    isSelected,
                    label,
                    EditorStyles.miniButton))
            {
                paintDefinitionIndex = i;
            }
        }

        GUI.backgroundColor =
            Color.white;

        EditorGUILayout.EndHorizontal();

        PhysicsObjectDefinition selectedDefinition =
            levelData.BlockPalette[paintDefinitionIndex];

        if (selectedDefinition != null)
        {
            SerializedObject definitionObject =
                new SerializedObject(selectedDefinition);

            definitionObject.Update();

            SerializedProperty tintProperty =
                definitionObject.FindProperty(
                    "tintWithPaintColor"
                );

            EditorGUILayout.PropertyField(
                tintProperty,
                new GUIContent(
                    "Tint Prefab With Paint Color",
                    "OFF keeps the prefab material and texture exactly."
                )
            );

            if (definitionObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(
                    selectedDefinition
                );
            }
        }
    }


    /// <summary>
    /// Footprint is measured live from the selected shape by default —
    /// pick the shape, it already knows how many cells it needs, no
    /// setup step required. Orientation buttons re-derive that footprint
    /// (and the spawn rotation) for a tipped-over piece with one click,
    /// so a tall piece can be made to lie flat without hand-typing spans.
    /// </summary>
    private void DrawSpanPicker(
        GridLevelData levelData)
    {
        EditorGUILayout.LabelField(
            "Footprint & Orientation",
            EditorStyles.boldLabel
        );

        PhysicsObjectDefinition selectedDefinition =
            paintDefinitionIndex >= 0 &&
            paintDefinitionIndex < levelData.BlockPalette.Count
                ? levelData.BlockPalette[paintDefinitionIndex]
                : null;

        Vector3Int naturalFootprint =
            selectedDefinition != null
                ? GetOrMeasureFootprint(selectedDefinition, levelData.CellSize)
                : Vector3Int.one;

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            $"'{(selectedDefinition != null ? selectedDefinition.DisplayName : "?")}' " +
            $"measured size (upright): " +
            $"{naturalFootprint.x}x{naturalFootprint.y}x{naturalFootprint.z} cells",
            EditorStyles.miniLabel
        );

        if (GUILayout.Button(
                "Re-measure",
                GUILayout.Width(90f)))
        {
            /*
             * Sirf edge case ke liye — agar prefab ka mesh/collider isi
             * Editor session mein change hua ho. Normal flow mein
             * kabhi press karne ki zaroorat nahi.
             */
            footprintCache.Clear();
        }

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();

        DrawOrientationButton("Upright", PieceOrientation.UprightY);
        DrawOrientationButton("Lying (X)", PieceOrientation.LyingX);
        DrawOrientationButton("X 90", PieceOrientation.LyingZ);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        DrawOrientationButton("Y 90", PieceOrientation.RotatedY90);
        DrawOrientationButton("Y 180", PieceOrientation.RotatedY180);
        DrawOrientationButton("Y 270", PieceOrientation.RotatedY270);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        DrawOrientationButton("Z 90", PieceOrientation.RotatedZ90);
        DrawOrientationButton("Z 180", PieceOrientation.RotatedZ180);
        DrawOrientationButton("Z 270", PieceOrientation.RotatedZ270);

        EditorGUILayout.EndHorizontal();

        DrawOrientationButton(
            "CUSTOM Z",
            PieceOrientation.CustomZ
        );

        EditorGUILayout.BeginHorizontal();

        using (
            new EditorGUI.DisabledScope(
                paintOrientation != PieceOrientation.CustomZ))
        {
            paintCustomZRotation =
                EditorGUILayout.FloatField(
                    new GUIContent(
                        "Custom Z Angle",
                        "Sirf CUSTOM Z orientation ke liye apni degree value."
                    ),
                    paintCustomZRotation
                );
        }

        if (GUILayout.Button(
                "Reset",
                GUILayout.Width(65f)))
        {
            paintCustomZRotation = 0f;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "Upright = as authored (tall pieces stand up). " +
            "Lying (X) lays a tall piece along X. " +
            "X 90 rotates the piece 90 degrees around its X axis.",
            MessageType.None
        );

        EditorGUILayout.HelpBox(
            "Y 90/180/270 aur Z 90/180/270 selected piece ko " +
            "respective local axis par rotate karte hain. Apna angle " +
            "sirf CUSTOM Z select karne ke baad effective hoga.",
            MessageType.None
        );


        EditorGUILayout.LabelField(
            "Seam Offset (brick-style stacking)",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        seamOffsetX =
            EditorGUILayout.ToggleLeft(
                "Center On Seam (X)",
                seamOffsetX,
                GUILayout.Width(160f)
            );

        seamOffsetZ =
            EditorGUILayout.ToggleLeft(
                "Center On Seam (Z)",
                seamOffsetZ,
                GUILayout.Width(160f)
            );

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "ON: the next click lands half a cell to the side instead " +
            "of dead-center — so a piece can sit straddling the seam " +
            "between two cells below it (e.g. one block resting on top " +
            "of two, half on each) instead of snapping to a single " +
            "cell's exact center.",
            MessageType.None
        );


        Vector3Int effectiveFootprint =
            PieceOrientationUtility.ApplyToFootprint(
                naturalFootprint,
                paintOrientation
            );

        manualFootprintOverride =
            EditorGUILayout.ToggleLeft(
                "Manual Footprint Override",
                manualFootprintOverride
            );

        if (manualFootprintOverride)
        {
            paintSpanX =
                EditorGUILayout.IntSlider(
                    "Span X",
                    paintSpanX,
                    1,
                    Mathf.Max(1, levelData.GridWidth)
                );

            paintSpanY =
                EditorGUILayout.IntSlider(
                    "Span Y",
                    paintSpanY,
                    1,
                    Mathf.Max(1, levelData.GridHeight)
                );

            if (levelData.GridDepth > 1)
            {
                paintSpanZ =
                    EditorGUILayout.IntSlider(
                        "Span Z",
                        paintSpanZ,
                        1,
                        Mathf.Max(1, levelData.GridDepth)
                    );
            }
            else
            {
                paintSpanZ = 1;
            }
        }
        else
        {
            paintSpanX =
                Mathf.Max(1, effectiveFootprint.x);

            paintSpanY =
                Mathf.Max(1, effectiveFootprint.y);

            paintSpanZ =
                levelData.GridDepth > 1
                    ? Mathf.Max(1, effectiveFootprint.z)
                    : 1;

            EditorGUILayout.LabelField(
                $"Next click paints: {paintSpanX}x{paintSpanY}x{paintSpanZ} cells"
            );
        }
    }


    /// <summary>
    /// Returns how many grid cells this shape naturally needs, measuring
    /// it the first time it's asked about (or after Cell Size changes)
    /// and reusing that afterward. Nothing here is authored or stored on
    /// the definition — there is no field to set and nothing that can go
    /// stale from a forgotten step; picking a shape always yields the
    /// right footprint.
    /// </summary>
    private Vector3Int GetOrMeasureFootprint(
        PhysicsObjectDefinition definition,
        Vector3 cellSize)
    {
        if (footprintCacheCellSize != cellSize)
        {
            footprintCache.Clear();
            footprintCacheCellSize = cellSize;
        }

        if (footprintCache.TryGetValue(
                definition,
                out Vector3Int cachedFootprint))
        {
            return cachedFootprint;
        }

        Vector3Int measuredFootprint =
            MeasurePrefabFootprint(definition, cellSize);

        footprintCache[definition] =
            measuredFootprint;

        return measuredFootprint;
    }


    /// <summary>
    /// Temporarily instantiates the definition's prefab (editor-only,
    /// destroyed immediately after) and measures its actual physics
    /// bounds at its authored scale, then divides by this level's
    /// CellSize and rounds to the nearest whole number of cells per
    /// axis.
    /// </summary>
    private static Vector3Int MeasurePrefabFootprint(
        PhysicsObjectDefinition definition,
        Vector3 cellSize)
    {
        if (definition == null ||
            definition.Prefab == null)
        {
            return Vector3Int.one;
        }

        GameObject tempInstance =
            (GameObject)PrefabUtility.InstantiatePrefab(
                definition.Prefab.gameObject
            );

        tempInstance.transform.SetPositionAndRotation(
            Vector3.zero,
            Quaternion.identity
        );

        Vector3Int result =
            Vector3Int.one;

        try
        {
            PhysicsTowerObject towerObject =
                tempInstance.GetComponent<PhysicsTowerObject>();

            Physics.SyncTransforms();

            if (towerObject != null &&
                towerObject.TryGetPhysicsBounds(out Bounds bounds))
            {
                /*
                 * bounds.size prefab ki AUTHORED (as-is) world size hai —
                 * ye seedha cellSize se compare hoti hai. Runtime fit-scale
                 * calculation (LevelRuntimeController.TryGetFitData) ke
                 * ulat: yahan authored scale ko normalize/divide NAHI
                 * karte, kyunke hume yahi jaanna hai ke shape ABHI, jaisi
                 * authored hai, kitne cells cover karti hai.
                 */
                result =
                    new Vector3Int(
                        Mathf.Max(1, Mathf.RoundToInt(bounds.size.x / Mathf.Max(0.0001f, cellSize.x))),
                        Mathf.Max(1, Mathf.RoundToInt(bounds.size.y / Mathf.Max(0.0001f, cellSize.y))),
                        Mathf.Max(1, Mathf.RoundToInt(bounds.size.z / Mathf.Max(0.0001f, cellSize.z)))
                    );
            }
        }
        finally
        {
            Object.DestroyImmediate(tempInstance);
        }

        return result;
    }


    private void DrawOrientationButton(
        string label,
        PieceOrientation value)
    {
        bool isSelected =
            paintOrientation == value;

        GUI.backgroundColor =
            isSelected
                ? new Color(0.4f, 0.9f, 0.55f)
                : Color.white;

        if (GUILayout.Toggle(
                isSelected,
                label,
                EditorStyles.miniButton))
        {
            paintOrientation = value;
        }

        GUI.backgroundColor =
            Color.white;
    }


    private void DrawColorPicker(
        GridLevelData levelData)
    {
        EditorGUILayout.LabelField(
            "Paint Color",
            EditorStyles.boldLabel
        );

        SerializedProperty paletteProperty =
            serializedObject.FindProperty("colorPalette");

        if (paletteProperty != null &&
            paletteProperty.arraySize > 0)
        {
            EditorGUILayout.BeginHorizontal();

            for (int i = 0;
                 i < paletteProperty.arraySize;
                 i++)
            {
                Color swatchColor =
                    paletteProperty
                        .GetArrayElementAtIndex(i)
                        .colorValue;

                Color previousBackground =
                    GUI.backgroundColor;

                GUI.backgroundColor =
                    swatchColor;

                if (GUILayout.Button(
                        "",
                        GUILayout.Width(28f),
                        GUILayout.Height(20f)))
                {
                    paintColor = swatchColor;
                }

                GUI.backgroundColor =
                    previousBackground;
            }

            EditorGUILayout.EndHorizontal();
        }

        paintColor =
            EditorGUILayout.ColorField(
                "Custom Color",
                paintColor
            );

        eraseMode =
            EditorGUILayout.ToggleLeft(
                "Erase Mode (click cells to clear)",
                eraseMode
            );
    }


    private void DrawPresetTools(
        GridLevelData levelData)
    {
        EditorGUILayout.LabelField(
            "Shape Presets",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear All"))
        {
            Undo.RecordObject(
                levelData,
                "Clear Grid Level"
            );

            levelData.EditorClearAllCells();

            EditorUtility.SetDirty(levelData);
        }

        if (GUILayout.Button("Fill Current Layer"))
        {
            Undo.RecordObject(
                levelData,
                "Fill Grid Layer"
            );

            levelData.EditorFillRectangle(
                paintLayerZ,
                0,
                0,
                levelData.GridWidth - 1,
                levelData.GridHeight - 1,
                paintColor,
                paintDefinitionIndex
            );

            if (levelData.MirrorPaintAcrossLayers)
            {
                levelData.EditorCopyDepthLayerToAll(
                    paintLayerZ
                );
            }

            EditorUtility.SetDirty(levelData);
        }

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField(
            "Pyramid (bottom-heavy stack)"
        );

        presetBottomRowCount =
            EditorGUILayout.IntSlider(
                "Bottom Row Count",
                presetBottomRowCount,
                1,
                levelData.GridWidth
            );

        presetRowCount =
            EditorGUILayout.IntSlider(
                "Row Count",
                presetRowCount,
                1,
                presetBottomRowCount
            );

        if (GUILayout.Button("Apply Pyramid Preset"))
        {
            Undo.RecordObject(
                levelData,
                "Apply Pyramid Preset"
            );

            levelData.EditorApplyPyramidPreset(
                paintLayerZ,
                presetBottomRowCount,
                presetRowCount,
                ReadPalette(),
                paintDefinitionIndex
            );

            if (levelData.MirrorPaintAcrossLayers)
            {
                levelData.EditorCopyDepthLayerToAll(
                    paintLayerZ
                );
            }

            EditorUtility.SetDirty(levelData);
        }


        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField(
            "Round Tier (cake-style layer, needs Grid Depth > 1)"
        );

        paintTierY =
            EditorGUILayout.IntSlider(
                "Tier Height (Y)",
                paintTierY,
                0,
                Mathf.Max(0, levelData.GridHeight - 1)
            );

        presetRingRadius =
            EditorGUILayout.Slider(
                "Radius",
                presetRingRadius,
                0.5f,
                Mathf.Max(
                    levelData.GridWidth,
                    levelData.GridDepth
                )
            );

        if (GUILayout.Button("Apply Round Tier At This Height"))
        {
            Undo.RecordObject(
                levelData,
                "Apply Round Tier"
            );

            levelData.EditorApplyRingLayer(
                paintTierY,
                presetRingRadius,
                paintColor,
                paintDefinitionIndex
            );

            EditorUtility.SetDirty(levelData);
        }
    }


    private List<Color> ReadPalette()
    {
        List<Color> palette =
            new List<Color>();

        SerializedProperty paletteProperty =
            serializedObject.FindProperty("colorPalette");

        if (paletteProperty == null)
        {
            return palette;
        }

        for (int i = 0;
             i < paletteProperty.arraySize;
             i++)
        {
            palette.Add(
                paletteProperty
                    .GetArrayElementAtIndex(i)
                    .colorValue
            );
        }

        return palette;
    }


    private void DrawGridPainter(
        GridLevelData levelData)
    {
        EditorGUILayout.LabelField(
            "Grid Paint (click cells to toggle)",
            EditorStyles.boldLabel
        );

        if (levelData.GridDepth > 1)
        {
            string[] layerLabels =
                new string[levelData.GridDepth];

            for (int z = 0; z < layerLabels.Length; z++)
            {
                layerLabels[z] =
                    $"Layer {z + 1}";
            }

            EditorGUILayout.LabelField(
                "Depth Layer (Z)"
            );

            paintLayerZ = GUILayout.Toolbar(
                Mathf.Clamp(
                    paintLayerZ,
                    0,
                    levelData.GridDepth - 1
                ),
                layerLabels
            );
        }
        else
        {
            paintLayerZ = 0;
        }

        string legend =
            "Legend: ";

        for (int i = 0;
             i < levelData.BlockPalette.Count;
             i++)
        {
            PhysicsObjectDefinition entry =
                levelData.BlockPalette[i];

            legend +=
                $"{i}={(entry != null ? entry.DisplayName : "missing")}  ";
        }

        EditorGUILayout.LabelField(
            legend,
            EditorStyles.miniLabel
        );

        EditorGUILayout.Space(4f);

        /*
         * Top row drawn first so the tower reads bottom-to-top
         * the same way it will spawn at runtime.
         */
        for (int y = levelData.GridHeight - 1;
             y >= 0;
             y--)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0;
                 x < levelData.GridWidth;
                 x++)
            {
                GridCellData cell =
                    levelData.GetCell(x, y, paintLayerZ);

                bool isOccupied =
                    cell != null && cell.Occupied;

                Color previousBackground =
                    GUI.backgroundColor;

                GUI.backgroundColor =
                    isOccupied
                        ? cell.Color
                        : new Color(1f, 1f, 1f, 0.12f);

                /*
                 * Shape ka short hint (palette index) taake mixed
                 * shapes wale cells button ke text se pehchane ja sakein.
                 * Covered cells (bare footprint ka hissa, khud spawn
                 * nahi hoti) ek dot se dikhai jaati hain.
                 */
                string cellLabel =
                    isOccupied
                        ? (
                            cell.IsCovered
                                ? "•"
                                : cell.DefinitionIndex.ToString()
                          )
                        : "";

                if (GUILayout.Button(
                        cellLabel,
                        GUILayout.Width(CellButtonSize),
                        GUILayout.Height(CellButtonSize)))
                {
                    int effectiveSpanZ =
                        levelData.MirrorPaintAcrossLayers
                            ? 1
                            : paintSpanZ;

                    bool paintingSpan =
                        paintSpanX > 1 ||
                        paintSpanY > 1 ||
                        effectiveSpanZ > 1 ||
                        paintOrientation != PieceOrientation.UprightY ||
                        seamOffsetX ||
                        seamOffsetZ;

                    Vector3 seamOffset =
                        new Vector3(
                            seamOffsetX ? 0.5f : 0f,
                            0f,
                            seamOffsetZ ? 0.5f : 0f
                        );

                    Undo.RecordObject(
                        levelData,
                        eraseMode
                            ? "Erase Grid Cell"
                            : "Paint Grid Cell"
                    );

                    int firstTargetLayer =
                        levelData.MirrorPaintAcrossLayers
                            ? 0
                            : paintLayerZ;

                    int lastTargetLayer =
                        levelData.MirrorPaintAcrossLayers
                            ? levelData.GridDepth - 1
                            : paintLayerZ;

                    for (int targetLayer = firstTargetLayer;
                         targetLayer <= lastTargetLayer;
                         targetLayer++)
                    {
                        if (eraseMode)
                        {
                            levelData.EditorClearSpanGroupAt(
                                x,
                                y,
                                targetLayer
                            );
                        }
                        else if (paintingSpan)
                        {
                            levelData.EditorPaintSpan(
                                x,
                                y,
                                targetLayer,
                                paintSpanX,
                                paintSpanY,
                                effectiveSpanZ,
                                paintColor,
                                paintDefinitionIndex,
                                paintOrientation,
                                seamOffset,
                                paintCustomZRotation
                            );
                        }
                        else
                        {
                            levelData.EditorSetCell(
                                x,
                                y,
                                targetLayer,
                                true,
                                paintColor,
                                paintDefinitionIndex
                            );
                        }
                    }

                    levelData.RecalculateGridMetadata();
                    EditorUtility.SetDirty(levelData);
                }

                GUI.backgroundColor =
                    previousBackground;
            }

            EditorGUILayout.EndHorizontal();
        }
    }


}

#endif
