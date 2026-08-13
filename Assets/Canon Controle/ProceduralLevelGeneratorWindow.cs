#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Bulk-generates GridLevelData Addressable level assets (e.g. levels
/// 1-1000) using the same Editor* painting API GridLevelDataEditor's
/// manual painter uses, so every generated level stays fully hand
/// -editable afterward and LevelRuntimeController needs no changes.
/// Grid size, shape variety and ball count scale with level progress.
/// Tables cycle every "Levels Per World" levels so the environment
/// visibly changes as the player advances, matching the reference
/// game's rotating kingdoms - add more table/shape assets later and
/// re-run to pick them up automatically.
/// </summary>
public sealed class ProceduralLevelGeneratorWindow : EditorWindow
{
    [MenuItem("Tools/Royal Smash/Procedural Level Generator")]
    private static void Open()
    {
        GetWindow<ProceduralLevelGeneratorWindow>("Level Generator");
    }

    private static readonly Color[] DefaultColors =
    {
        new Color(1.00f, 0.20f, 0.30f, 1f),
        new Color(0.10f, 0.58f, 1.00f, 1f),
        new Color(1.00f, 0.78f, 0.10f, 1f),
        new Color(0.55f, 0.25f, 0.95f, 1f),
        new Color(0.05f, 0.80f, 0.52f, 1f),
        new Color(1.00f, 0.42f, 0.10f, 1f)
    };

    [SerializeField] private int startLevel = 1;
    [SerializeField] private int endLevel = 1000;
    [SerializeField] private int randomSeed = 12345;

    [SerializeField] private List<LevelTable> tablePool = new List<LevelTable>();
    [SerializeField, Min(1)] private int levelsPerWorld = 100;

    [SerializeField] private List<PhysicsObjectDefinition> shapePool = new List<PhysicsObjectDefinition>();

    [SerializeField] private Vector3 cellSize = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField, Min(2)] private int minGridWidth = 5;
    [SerializeField, Min(2)] private int maxGridWidth = 26;
    [SerializeField, Min(2)] private int minGridHeight = 5;
    [SerializeField, Min(2)] private int maxGridHeight = 16;
    [SerializeField, Range(1, 8)] private int maxDepthLayers = 2;

    [SerializeField, Min(1)] private int minBalls = 10;
    [SerializeField] private float ballsPerBlockRatioStart = 0.6f;
    [SerializeField] private float ballsPerBlockRatioEnd = 0.25f;

    private SerializedObject serializedSelf;
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        serializedSelf = new SerializedObject(this);

        if (tablePool.Count == 0)
        {
            AutoFillTablePool();
        }

        if (shapePool.Count == 0)
        {
            AutoFillShapePool();
        }
    }

    private void OnGUI()
    {
        serializedSelf.Update();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.HelpBox(
            "Generates GridLevelData Addressable level assets in bulk, " +
            "using the same painting API as the manual Level Creator " +
            "painter. Existing Level_XXX assets whose number falls " +
            "inside the range below will be overwritten. Not undoable " +
            "- commit or back up first.",
            MessageType.Info
        );

        EditorGUILayout.LabelField("Range", EditorStyles.boldLabel);
        DrawPropertyField("startLevel", "Start Level");
        DrawPropertyField("endLevel", "End Level");
        DrawPropertyField("randomSeed", "Random Seed");

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Table Pool", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            tablePool.Count == 0
                ? "No tables assigned - add at least one LevelTable prefab."
                : $"{tablePool.Count} table(s). A new table is picked " +
                  $"every {levelsPerWorld} levels, cycling through the " +
                  "list, so the environment changes as the player " +
                  "advances. Add more LevelTable prefabs later and " +
                  "click Auto-Fill again to pick them up.",
            tablePool.Count == 0 ? MessageType.Warning : MessageType.None
        );

        DrawPropertyField("tablePool", "Tables");
        DrawPropertyField("levelsPerWorld", "Levels Per World");

        if (GUILayout.Button("Auto-Fill Tables From Project"))
        {
            AutoFillTablePool();
        }

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Shape Pool", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            shapePool.Count == 0
                ? "No shapes assigned - add at least one Physics Object " +
                  "Definition."
                : $"{shapePool.Count} shape(s). Early levels use fewer " +
                  "shapes, more unlock as the level number increases. " +
                  "Add more Physics Object Definitions later (new " +
                  "cylinders/blocks) and click Auto-Fill again.",
            shapePool.Count == 0 ? MessageType.Warning : MessageType.None
        );

        DrawPropertyField("shapePool", "Shapes");

        if (GUILayout.Button("Auto-Fill Shapes From Project"))
        {
            AutoFillShapePool();
        }

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Grid Size", EditorStyles.boldLabel);
        DrawPropertyField("cellSize", "Cell Size");
        DrawPropertyField("minGridWidth", "Min Grid Width");
        DrawPropertyField("maxGridWidth", "Max Grid Width");
        DrawPropertyField("minGridHeight", "Min Grid Height");
        DrawPropertyField("maxGridHeight", "Max Grid Height");
        DrawPropertyField("maxDepthLayers", "Max Depth Layers");

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Balls", EditorStyles.boldLabel);
        DrawPropertyField("minBalls", "Minimum Balls");
        DrawPropertyField("ballsPerBlockRatioStart", "Balls Per Block (Level 1)");
        DrawPropertyField("ballsPerBlockRatioEnd", "Balls Per Block (Last Level)");

        serializedSelf.ApplyModifiedProperties();

        EditorGUILayout.Space(16f);

        bool canGenerate = CanGenerate(out string blockReason);

        using (new EditorGUI.DisabledScope(!canGenerate))
        {
            if (GUILayout.Button(
                    $"GENERATE LEVELS {startLevel} - {endLevel}",
                    GUILayout.Height(42f)))
            {
                GenerateLevels();
            }
        }

        if (!canGenerate)
        {
            EditorGUILayout.HelpBox(blockReason, MessageType.Warning);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawPropertyField(string propertyName, string label)
    {
        SerializedProperty property = serializedSelf.FindProperty(propertyName);

        if (property != null)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }
    }

    private bool CanGenerate(out string reason)
    {
        if (startLevel < 1 || endLevel < startLevel)
        {
            reason = "Start Level must be >= 1 and End Level must be >= Start Level.";
            return false;
        }

        if (tablePool.Count == 0)
        {
            reason = "Assign at least one table.";
            return false;
        }

        if (shapePool.Count == 0)
        {
            reason = "Assign at least one shape.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private void AutoFillTablePool()
    {
        tablePool.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            LevelTable table = prefab != null ? prefab.GetComponent<LevelTable>() : null;

            if (table != null && !tablePool.Contains(table))
            {
                tablePool.Add(table);
            }
        }

        Repaint();
    }

    private void AutoFillShapePool()
    {
        shapePool.Clear();

        string[] guids = AssetDatabase.FindAssets("t:PhysicsObjectDefinition");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            PhysicsObjectDefinition definition =
                AssetDatabase.LoadAssetAtPath<PhysicsObjectDefinition>(path);

            if (definition != null &&
                definition.Prefab != null &&
                !shapePool.Contains(definition))
            {
                shapePool.Add(definition);
            }
        }

        Repaint();
    }

    private void GenerateLevels()
    {
        if (!CanGenerate(out string reason))
        {
            EditorUtility.DisplayDialog("Cannot Generate", reason, "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Generate Levels",
                $"Generate levels {startLevel}-{endLevel}? Any existing " +
                "Level_XXX asset in that range will be overwritten. " +
                "This cannot be undone.",
                "Generate",
                "Cancel"))
        {
            return;
        }

        GenerateRangeInternal(startLevel, endLevel, startLevel, endLevel, showProgressUI: true);

        Debug.Log($"Procedural level generation finished ({startLevel}-{endLevel}).");
    }

    /// <summary>
    /// Headless entry point with no confirmation dialog or progress bar
    /// - for automation (build scripts, CI, or a quick verification
    /// run) rather than the interactive window. Uses this window
    /// type's own default settings, including the project auto-fill
    /// for tables/shapes performed in OnEnable.
    /// </summary>
    public static void GenerateRangeHeadless(int rangeStartLevel, int rangeEndLevel)
    {
        GenerateRangeHeadless(rangeStartLevel, rangeEndLevel, rangeStartLevel, rangeEndLevel);
    }

    /// <summary>
    /// Same as <see cref="GenerateRangeHeadless(int, int)"/>, but lets
    /// the levels actually written (rangeStartLevel..rangeEndLevel) be
    /// a sub-batch of a larger difficulty curve
    /// (progressRangeStart..progressRangeEnd) - e.g. writing just
    /// 201-400 while still scaling difficulty as if it were the middle
    /// of a full 1-1000 run, so batching a big generation into several
    /// calls still produces one continuous curve instead of the curve
    /// restarting every batch.
    /// </summary>
    public static void GenerateRangeHeadless(
        int rangeStartLevel,
        int rangeEndLevel,
        int progressRangeStart,
        int progressRangeEnd)
    {
        ProceduralLevelGeneratorWindow window =
            CreateInstance<ProceduralLevelGeneratorWindow>();

        try
        {
            window.startLevel = rangeStartLevel;
            window.endLevel = rangeEndLevel;

            if (!window.CanGenerate(out string reason))
            {
                Debug.LogError($"GenerateRangeHeadless: {reason}");
                return;
            }

            window.GenerateRangeInternal(
                rangeStartLevel,
                rangeEndLevel,
                progressRangeStart,
                progressRangeEnd,
                showProgressUI: false
            );

            Debug.Log($"Procedural level generation finished ({rangeStartLevel}-{rangeEndLevel}).");
        }
        finally
        {
            DestroyImmediate(window);
        }
    }

    private void GenerateRangeInternal(
        int rangeStartLevel,
        int rangeEndLevel,
        int progressRangeStart,
        int progressRangeEnd,
        bool showProgressUI)
    {
        GridLevelDatabase database = GetOrCreateLevelDatabase();

        int progressTotal = Mathf.Max(1, progressRangeEnd - progressRangeStart);
        int batchTotal = Mathf.Max(1, rangeEndLevel - rangeStartLevel);

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int levelNumber = rangeStartLevel; levelNumber <= rangeEndLevel; levelNumber++)
            {
                float progress = (float)(levelNumber - progressRangeStart) / progressTotal;

                if (showProgressUI &&
                    EditorUtility.DisplayCancelableProgressBar(
                        "Generating Levels",
                        $"Level {levelNumber} / {rangeEndLevel}",
                        (float)(levelNumber - rangeStartLevel) / batchTotal))
                {
                    break;
                }

                GenerateSingleLevel(levelNumber, progress, database);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();

            if (showProgressUI)
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
        }
    }

    private void GenerateSingleLevel(
        int levelNumber,
        float progress,
        GridLevelDatabase database)
    {
        System.Random random = new System.Random(MixSeed(randomSeed, levelNumber));

        /*
         * DIFFICULTY MODEL
         *
         * Grid width/height are NOT difficulty - they are just the
         * canvas. A bigger grid holding the same topple-in-one-shot
         * tower is no harder, it is only more blocks to render.
         * Difficulty here comes from properties that change how many
         * accurate shots the structure actually costs:
         *
         *  1. Cluster count - separate structures must each be brought
         *     down, so shots cannot be shared between them.
         *  2. Squatness - a tall narrow tower topples from one good hit
         *     (easy); a wide low one has to be chipped apart (hard).
         *  3. Ball budget tightness relative to the estimated shots.
         *  4. Depth layers - a second rank must be cleared too.
         */
        int clusterCount = ChooseClusterCount(progress, random);

        /*
         * Early: narrow and tall (topples readily).
         * Late: wide and squat (stable, must be dismantled).
         */
        int clusterWidth =
            Mathf.RoundToInt(Mathf.Lerp(4f, 7f, progress)) + random.Next(0, 2);

        int height =
            Mathf.RoundToInt(Mathf.Lerp(13f, 9f, progress)) + random.Next(0, 3);

        height = Mathf.Clamp(height, minGridHeight, maxGridHeight);

        int clusterGap = random.Next(1, 3);

        int worldIndexForTable = (levelNumber - 1) / Mathf.Max(1, levelsPerWorld);

        LevelTable plannedTable =
            tablePool[worldIndexForTable % tablePool.Count];

        /*
         * The tower has to physically stand on the table. Cell counts
         * are therefore capped by the table's own surface, measured
         * from its collider rather than assumed - otherwise edge blocks
         * spawn over thin air and drop the moment physics starts.
         * Measuring (instead of hard-coding) means any table added
         * later automatically sets its own limits.
         */
        GetTableCapacity(
            plannedTable,
            out int capacityX,
            out int capacityZ
        );

        /*
         * Drop clusters until the row fits, then shrink the clusters
         * themselves; a level with fewer, well-formed structures beats
         * one with structures sliced off at the table edge.
         */
        while (clusterCount > 1 &&
               clusterCount * 3 + (clusterCount - 1) * clusterGap > capacityX)
        {
            clusterCount--;
        }

        int widthAvailableForClusters =
            capacityX - (clusterCount - 1) * clusterGap;

        clusterWidth = Mathf.Clamp(
            Mathf.Min(clusterWidth, widthAvailableForClusters / Mathf.Max(1, clusterCount)),
            3,
            Mathf.Max(3, capacityX)
        );

        int width = Mathf.Clamp(
            clusterCount * clusterWidth + (clusterCount - 1) * clusterGap,
            minGridWidth,
            Mathf.Min(maxGridWidth, capacityX)
        );

        /*
         * Graduated rather than binary. The previous form picked either
         * 1 or maxDepthLayers, so raising the setting to 3 produced 1
         * and 3 but never 2 - the middle option was unreachable.
         */
        int deepestAllowed = Mathf.Min(maxDepthLayers, capacityZ);

        int depthLayers = ChooseDepthLayers(deepestAllowed, progress, random);

        int shapesAvailable = Mathf.Clamp(
            1 + Mathf.FloorToInt(progress * (shapePool.Count - 1)),
            1,
            shapePool.Count
        );

        LevelTable table = plannedTable;

        int minimumBlocks = Mathf.Max(
            10,
            Mathf.RoundToInt(clusterCount * clusterWidth * height * 0.10f)
        );

        List<List<StructurePiece>> layerPieces =
            new List<List<StructurePiece>>();

        for (int z = 0; z < depthLayers; z++)
        {
            layerPieces.Add(
                BuildClusteredLayer(
                    clusterCount,
                    clusterWidth,
                    clusterGap,
                    width,
                    height,
                    levelNumber,
                    z,
                    minimumBlocks
                )
            );
        }

        /*
         * Every cell of the grid is serialized whether or not it holds
         * anything, so an oversized canvas is pure file weight - which
         * matters directly on mobile. Shrink-wrapping the grid to the
         * content's bounding box keeps the asset as small as the level
         * actually needs.
         */
        TrimToContent(layerPieces, ref width, ref height);

        GridLevelData level = ScriptableObject.CreateInstance<GridLevelData>();

        SerializedObject levelSerialized = new SerializedObject(level);
        levelSerialized.FindProperty("gridWidth").intValue = width;
        levelSerialized.FindProperty("gridHeight").intValue = height;
        levelSerialized.FindProperty("cellSize").vector3Value = cellSize;
        levelSerialized.FindProperty("tablePrefab").objectReferenceValue = table;
        levelSerialized.ApplyModifiedPropertiesWithoutUndo();

        level.EditorSetLevelNumber(levelNumber);
        level.EditorSetDepthLayerCount(depthLayers);

        /*
         * Mirroring is deliberately off: layers now hold genuinely
         * different structures, so copying one across all of them
         * would overwrite that variety.
         */
        level.EditorSetMirrorPaintAcrossLayers(false);

        for (int i = 0; i < shapesAvailable; i++)
        {
            level.EditorAddPaletteEntry(shapePool[i]);
        }

        PaintLayers(level, layerPieces, shapesAvailable, random);

        int balls = ComputeBallBudget(
            level.OccupiedCellCount,
            clusterCount,
            depthLayers,
            progress
        );

        levelSerialized.Update();
        levelSerialized.FindProperty("availableBalls").intValue = balls;
        levelSerialized.ApplyModifiedPropertiesWithoutUndo();

        GridLevelAddressablesEditorUtility.SaveWorkingLevel(level, database, out _);

        Object.DestroyImmediate(level);
    }

    /// <summary>
    /// Avalanche-mixes seed and level number. A plain seed ^ (level * k)
    /// leaves consecutive levels highly correlated in System.Random's
    /// first few draws, which visibly clumps the archetype choice
    /// (long runs of the same structure type across nearby levels).
    /// </summary>
    private static int MixSeed(int seed, int levelNumber)
    {
        unchecked
        {
            uint hash = (uint)seed * 2654435761u;

            hash ^= (uint)levelNumber * 2246822519u;
            hash = (hash ^ (hash >> 15)) * 2246822519u;
            hash = (hash ^ (hash >> 13)) * 3266489917u;
            hash ^= hash >> 16;

            return (int)(hash & 0x7FFFFFFF);
        }
    }

    /// <summary>
    /// How many grid cells fit on a table's playing surface along X
    /// (width) and Z (depth), measured from its actual tower-surface
    /// collider at the level's cell size.
    /// </summary>
    private void GetTableCapacity(
        LevelTable table,
        out int capacityX,
        out int capacityZ)
    {
        capacityX = maxGridWidth;
        capacityZ = maxDepthLayers;

        if (table == null ||
            table.TowerSurfaceCollider == null)
        {
            return;
        }

        BoxCollider surface = table.TowerSurfaceCollider;
        Vector3 lossyScale = surface.transform.lossyScale;

        float worldX = surface.size.x * Mathf.Abs(lossyScale.x);
        float worldZ = surface.size.z * Mathf.Abs(lossyScale.z);

        float stepX = Mathf.Max(0.0001f, cellSize.x);
        float stepZ = Mathf.Max(0.0001f, cellSize.z);

        capacityX = Mathf.Max(3, Mathf.FloorToInt(worldX / stepX));
        capacityZ = Mathf.Max(1, Mathf.FloorToInt(worldZ / stepZ));
    }

    /// <summary>
    /// Picks a depth between 1 and the deepest the table allows,
    /// weighted so early levels stay shallow and later ones use the
    /// full available depth. Every value in between is reachable.
    /// </summary>
    private static int ChooseDepthLayers(
        int deepestAllowed,
        float progress,
        System.Random random)
    {
        deepestAllowed = Mathf.Max(1, deepestAllowed);

        if (deepestAllowed == 1)
        {
            return 1;
        }

        float roll = (float)random.NextDouble();

        /*
         * Chance of going beyond a single layer at all, rising with
         * progress; the extra depth beyond 2 is reserved for later
         * levels so the back ranks arrive gradually.
         */
        float multiLayerChance = Mathf.Lerp(0.3f, 0.9f, progress);

        if (roll > multiLayerChance)
        {
            return 1;
        }

        int maximumForProgress = Mathf.Clamp(
            2 + Mathf.FloorToInt(progress * (deepestAllowed - 1)),
            2,
            deepestAllowed
        );

        return random.Next(2, maximumForProgress + 1);
    }

    /// <summary>
    /// Number of separate structures the player has to bring down.
    /// Splitting the same block budget into two towers is meaningfully
    /// harder than one tall one, because shots and chain reactions
    /// cannot carry across the gap.
    /// </summary>
    private static int ChooseClusterCount(float progress, System.Random random)
    {
        float roll = (float)random.NextDouble();

        if (progress < 0.25f)
        {
            return roll < 0.85f ? 1 : 2;
        }

        if (progress < 0.6f)
        {
            return roll < 0.45f ? 1 : 2;
        }

        if (roll < 0.25f)
        {
            return 1;
        }

        return roll < 0.75f ? 2 : 3;
    }

    /// <summary>
    /// Lays the level's clusters side by side across the canvas, each
    /// its own independently generated (and independently sound)
    /// structure with its own archetype.
    /// </summary>
    private List<StructurePiece> BuildClusteredLayer(
        int clusterCount,
        int clusterWidth,
        int clusterGap,
        int totalWidth,
        int height,
        int levelNumber,
        int layerIndex,
        int minimumBlocks)
    {
        List<StructurePiece> combined = new List<StructurePiece>();

        int perClusterMinimum =
            Mathf.Max(6, minimumBlocks / Mathf.Max(1, clusterCount));

        int cursorX = 0;

        for (int cluster = 0; cluster < clusterCount; cluster++)
        {
            if (cursorX >= totalWidth)
            {
                break;
            }

            int availableWidth = Mathf.Min(clusterWidth, totalWidth - cursorX);

            if (availableWidth < 3)
            {
                break;
            }

            /*
             * Distinct archetype per cluster and per layer, so a
             * multi-cluster level reads as several different buildings
             * rather than the same one repeated across the table.
             */
            int clusterArchetype =
                ChooseArchetype(levelNumber * 31 + cluster * 7 + layerIndex * 3);

            List<StructurePiece> pieces =
                BuildSoundStructure(
                    availableWidth,
                    height,
                    clusterArchetype,
                    levelNumber * 101 + cluster * 17,
                    layerIndex,
                    perClusterMinimum
                );

            foreach (StructurePiece piece in pieces)
            {
                combined.Add(new StructurePiece(
                    piece.X + cursorX,
                    piece.Y,
                    piece.SpanX,
                    piece.SpanY,
                    piece.Role
                ));
            }

            cursorX += availableWidth + clusterGap;
        }

        return combined;
    }

    /// <summary>
    /// Shrink-wraps the grid to the union bounding box of all layers,
    /// shifting pieces so the content starts at the origin. Empty rows
    /// and columns still cost full serialized cells, so trimming them
    /// is a direct saving on the shipped asset.
    /// </summary>
    private static void TrimToContent(
        List<List<StructurePiece>> layerPieces,
        ref int width,
        ref int height)
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (List<StructurePiece> layer in layerPieces)
        {
            foreach (StructurePiece piece in layer)
            {
                minX = Mathf.Min(minX, piece.X);
                minY = Mathf.Min(minY, piece.Y);
                maxX = Mathf.Max(maxX, piece.X + Mathf.Max(1, piece.SpanX) - 1);
                maxY = Mathf.Max(maxY, piece.Y + Mathf.Max(1, piece.SpanY) - 1);
            }
        }

        if (minX > maxX || minY > maxY)
        {
            return;
        }

        for (int z = 0; z < layerPieces.Count; z++)
        {
            List<StructurePiece> shifted =
                new List<StructurePiece>(layerPieces[z].Count);

            foreach (StructurePiece piece in layerPieces[z])
            {
                shifted.Add(new StructurePiece(
                    piece.X - minX,
                    piece.Y - minY,
                    piece.SpanX,
                    piece.SpanY,
                    piece.Role
                ));
            }

            layerPieces[z] = shifted;
        }

        width = maxX - minX + 1;
        height = maxY - minY + 1;
    }

    /// <summary>
    /// Estimates how many accurate shots the level actually costs, then
    /// grants a budget on top of that. The old formula was a flat
    /// blocks * ratio, which ignored that a chain-collapsing tower and
    /// three separate stumps with the same block count need completely
    /// different numbers of shots.
    /// </summary>
    private int ComputeBallBudget(
        int occupiedCellCount,
        int clusterCount,
        int depthLayers,
        float progress)
    {
        /*
         * Each cluster costs a couple of aimed shots to destabilize,
         * plus a share of chip-away shots for what does not collapse
         * on its own. A second rank adds its own clean-up cost.
         */
        float chipRate = Mathf.Lerp(0.16f, 0.09f, progress);

        float estimatedShots =
            clusterCount * 2f +
            occupiedCellCount * chipRate +
            (depthLayers - 1) * 2f;

        /*
         * Generosity narrows with progress: early levels hand out far
         * more balls than needed, late levels leave little slack.
         */
        float generosity = Mathf.Lerp(2.0f, 1.15f, progress);

        int budget = Mathf.CeilToInt(estimatedShots * generosity);

        return Mathf.Max(minBalls, budget);
    }

    /// <summary>
    /// One placed piece of a structure. spanX/spanY > 1 produce a single
    /// merged block covering several cells - required for lintels, planks
    /// and bridges, because a 1x1 cell floating over a gap has nothing
    /// directly beneath it and LevelRuntimeController's support chain
    /// would immediately drop it.
    /// </summary>
    private struct StructurePiece
    {
        public int X;
        public int Y;
        public int SpanX;
        public int SpanY;
        public int Role;

        public StructurePiece(int x, int y, int spanX, int spanY, int role)
        {
            X = x;
            Y = y;
            SpanX = spanX;
            SpanY = spanY;
            Role = role;
        }
    }

    private const int RoleWall = 0;
    private const int RolePillar = 1;
    private const int RoleBeam = 2;
    private const int RoleCrown = 3;

    private const int ArchetypeCount = 14;

    /// <summary>
    /// Every block of 10 consecutive levels gets a freshly shuffled deck
    /// of all 10 archetypes, so each one appears exactly once per block.
    /// A plain per-level random pick instead produced long runs of the
    /// same structure (measured runs of 6+), which is what makes a
    /// generated set feel repetitive to play through.
    /// </summary>
    private int ChooseArchetype(int levelNumber)
    {
        int blockIndex = (levelNumber - 1) / ArchetypeCount;
        int indexInBlock = (levelNumber - 1) % ArchetypeCount;

        int[] deck = new int[ArchetypeCount];

        for (int i = 0; i < ArchetypeCount; i++)
        {
            deck[i] = i;
        }

        System.Random deckRandom =
            new System.Random(MixSeed(randomSeed + 7919, blockIndex));

        for (int i = ArchetypeCount - 1; i > 0; i--)
        {
            int swapIndex = deckRandom.Next(i + 1);
            int temporary = deck[i];
            deck[i] = deck[swapIndex];
            deck[swapIndex] = temporary;
        }

        return deck[indexInBlock];
    }

    /// <summary>
    /// Builds one castle-style archetype. All archetypes are available
    /// from level 1 - progression is carried by size, depth, shape
    /// variety and ball budget rather than by withholding structure
    /// types, which would leave the early game (the part most players
    /// actually see) looking samey.
    /// </summary>
    private List<StructurePiece> BuildStructure(
        int width,
        int height,
        int archetypeIndex,
        System.Random random)
    {
        switch (archetypeIndex)
        {
            case 0:
                return BuildSteppedPyramid(width, height, random);

            case 1:
                return BuildCrenellatedKeep(width, height, random);

            case 2:
                return BuildSkylineCluster(width, height, random);

            case 3:
                return BuildPlankShelfTower(width, height, random);

            case 4:
                return BuildGateTower(width, height, random);

            case 5:
                return BuildTwinTowerBridge(width, height, random);

            case 6:
                return BuildFortressWall(width, height, random);

            case 7:
                return BuildTableStack(width, height, random);

            case 8:
                return BuildHollowKeep(width, height, random);

            case 9:
                return BuildLayeredCake(width, height, random);

            case 10:
                return BuildStaircase(width, height, random);

            case 11:
                return BuildArchRow(width, height, random);

            case 12:
                return BuildSpireTower(width, height, random);

            default:
                return BuildWingedCastle(width, height, random);
        }
    }

    private static List<StructurePiece> BuildStaircase(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int stepWidth = random.Next(3, Mathf.Max(4, width / 4) + 1);
        int stepRise = random.Next(2, 4);
        int steps = Mathf.Min(width / Mathf.Max(1, stepWidth), height / Mathf.Max(1, stepRise));

        /*
         * Ascending open bays: each step is a taller frame carrying a
         * deck, so the staircase is climbable-looking but hollow.
         */
        for (int step = 0; step < steps; step++)
        {
            int bayHeight = Mathf.Min(height - 1, (step + 1) * stepRise);
            int startX = step * stepWidth;

            if (startX + stepWidth > width || bayHeight < 1)
            {
                break;
            }

            AddFrameBay(pieces, startX, 0, stepWidth, bayHeight, width, height);
        }

        return pieces;
    }

    private static List<StructurePiece> BuildArchRow(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int archWidth = random.Next(3, Mathf.Max(4, width / 2) + 1);
        int archHeight = random.Next(2, Mathf.Max(3, height / 2) + 1);
        int archCount = Mathf.Max(1, width / archWidth);
        int stories = Mathf.Max(1, random.Next(1, 3));

        for (int story = 0; story < stories; story++)
        {
            int baseY = story * (archHeight + 1);

            if (baseY + archHeight >= height)
            {
                break;
            }

            for (int arch = 0; arch < archCount; arch++)
            {
                int startX = arch * archWidth;

                if (startX + archWidth > width)
                {
                    break;
                }

                for (int y = baseY; y < baseY + archHeight; y++)
                {
                    pieces.Add(new StructurePiece(startX, y, 1, 1, RolePillar));

                    pieces.Add(new StructurePiece(
                        startX + archWidth - 1, y, 1, 1, RolePillar
                    ));
                }

                pieces.Add(new StructurePiece(
                    startX, baseY + archHeight, archWidth, 1, RoleBeam
                ));
            }
        }

        return pieces;
    }

    private static List<StructurePiece> BuildSpireTower(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int baseWidth = random.Next(Mathf.Max(4, width / 2), width + 1);
        int baseHeight = random.Next(2, Mathf.Max(3, height / 3) + 1);
        int startX = (width - baseWidth) / 2;

        /*
         * Open hall at the base carrying a narrow spire, instead of a
         * solid plinth with a solid mast on top.
         */
        AddFrameBay(pieces, startX, 0, baseWidth, baseHeight, width, height);

        if (baseWidth >= 7)
        {
            int midX = startX + baseWidth / 2;

            for (int y = 0; y < baseHeight && y < height; y++)
            {
                pieces.Add(new StructurePiece(midX, y, 1, 1, RolePillar));
            }
        }

        int spireWidth = Mathf.Max(2, baseWidth / 3);
        int spireStart = (width - spireWidth) / 2;
        int spireHeight = random.Next(Mathf.Max(2, height / 3), height - baseHeight);
        int spireY = baseHeight + 1;
        int storyHeight = random.Next(2, 4);

        while (spireY + storyHeight < spireY + spireHeight && spireY + storyHeight < height)
        {
            AddFrameBay(
                pieces, spireStart, spireY, spireWidth, storyHeight, width, height
            );

            spireY += storyHeight + 1;
        }

        return pieces;
    }

    private static List<StructurePiece> BuildWingedCastle(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int keepWidth = Mathf.Max(3, width / 3);
        int keepStart = (width - keepWidth) / 2;
        int keepHeight = random.Next(Mathf.Max(4, height / 2), height);
        int storyHeight = random.Next(2, 4);

        /*
         * Central keep as stacked open rooms, with lower open wings to
         * either side - a silhouette with interior voids rather than a
         * solid block of cubes.
         */
        int currentY = 0;

        while (currentY + storyHeight < keepHeight && currentY + storyHeight < height)
        {
            AddFrameBay(
                pieces, keepStart, currentY, keepWidth, storyHeight, width, height
            );

            currentY += storyHeight + 1;
        }

        int wingSpan = Mathf.Max(2, (width - keepWidth) / 2);
        int wingHeight = Mathf.Max(2, keepHeight / 3);

        int leftStart = Mathf.Max(0, keepStart - wingSpan);
        int leftWidth = keepStart - leftStart;

        if (leftWidth >= 2)
        {
            AddFrameBay(pieces, leftStart, 0, leftWidth, wingHeight, width, height);
        }

        int rightStart = keepStart + keepWidth;
        int rightWidth = Mathf.Min(wingSpan, width - rightStart);

        if (rightWidth >= 2)
        {
            AddFrameBay(pieces, rightStart, 0, rightWidth, wingHeight, width, height);
        }

        if (currentY < height)
        {
            for (int x = keepStart; x < keepStart + keepWidth && x < width; x += 2)
            {
                pieces.Add(new StructurePiece(x, currentY, 1, 1, RoleCrown));
            }
        }

        return pieces;
    }

    /// <summary>
    /// Wraps a base archetype with optional composition and decoration.
    /// With only ~14 archetypes across 1000 levels each would otherwise
    /// recur ~70 times; splitting the field between two different
    /// archetypes and adding optional buttresses/crowns multiplies the
    /// distinct outcomes by roughly two orders of magnitude.
    /// </summary>
    private List<StructurePiece> BuildComposedStructure(
        int width,
        int height,
        int archetypeIndex,
        System.Random random)
    {
        List<StructurePiece> pieces;

        bool canCompose = width >= 10;
        int compositionRoll = random.Next(100);

        if (canCompose && compositionRoll < 30)
        {
            int splitX = random.Next(width / 3, width * 2 / 3 + 1);

            int secondArchetype =
                (archetypeIndex + 1 + random.Next(ArchetypeCount - 1)) % ArchetypeCount;

            pieces = BuildStructure(splitX, height, archetypeIndex, random);

            List<StructurePiece> rightPieces =
                BuildStructure(width - splitX, height, secondArchetype, random);

            foreach (StructurePiece piece in rightPieces)
            {
                pieces.Add(new StructurePiece(
                    piece.X + splitX,
                    piece.Y,
                    piece.SpanX,
                    piece.SpanY,
                    piece.Role
                ));
            }
        }
        else
        {
            pieces = BuildStructure(width, height, archetypeIndex, random);
        }

        if (random.Next(100) < 35)
        {
            AddButtresses(pieces, width, height, random);
        }

        return pieces;
    }

    /// <summary>
    /// Adds short grounded columns hugging the outer edges - visually
    /// breaks up the silhouette and gives the player low-value edge
    /// targets distinct from the main mass.
    /// </summary>
    private static void AddButtresses(
        List<StructurePiece> pieces,
        int width,
        int height,
        System.Random random)
    {
        int buttressHeight = random.Next(1, Mathf.Max(2, height / 3) + 1);

        for (int y = 0; y < buttressHeight && y < height; y++)
        {
            pieces.Add(new StructurePiece(0, y, 1, 1, RolePillar));
            pieces.Add(new StructurePiece(width - 1, y, 1, 1, RolePillar));
        }
    }

    /// <summary>
    /// Stacks hollow post-and-lintel bays: two vertical posts carrying a
    /// horizontal beam, with the interior left OPEN. This is the load
    /// -bearing frame real cannon physics puzzles are built from - shoot
    /// out a post and everything above drops. Solid cell-by-cell fills
    /// are deliberately avoided; they read as a brick wall and absorb
    /// impacts instead of collapsing.
    /// </summary>
    private static void AddFrameBay(
        List<StructurePiece> pieces,
        int originX,
        int originY,
        int bayWidth,
        int bayHeight,
        int gridWidth,
        int gridHeight)
    {
        if (bayWidth < 1 || bayHeight < 1)
        {
            return;
        }

        /*
         * A 2-wide bay would place pillars on two ADJACENT columns,
         * leaving no interior void - i.e. a solid block, the exact
         * "cubes stacked on cubes" look this whole archetype set
         * exists to avoid. Narrow bays collapse to a single post so
         * there is always daylight between supports.
         */
        bool tooNarrowForVoid = bayWidth < 3;

        for (int y = originY; y < originY + bayHeight && y < gridHeight; y++)
        {
            if (originX >= 0 && originX < gridWidth)
            {
                pieces.Add(new StructurePiece(originX, y, 1, 1, RolePillar));
            }

            if (tooNarrowForVoid)
            {
                continue;
            }

            int rightX = originX + bayWidth - 1;

            if (rightX >= 0 && rightX < gridWidth && rightX != originX)
            {
                pieces.Add(new StructurePiece(rightX, y, 1, 1, RolePillar));
            }
        }

        int beamY = originY + bayHeight;

        if (beamY < gridHeight && originX >= 0 && originX + bayWidth <= gridWidth)
        {
            pieces.Add(new StructurePiece(originX, beamY, bayWidth, 1, RoleBeam));
        }
    }

    /// <summary>
    /// Hollow rectangular room - side walls plus a spanning roof beam,
    /// open inside.
    /// </summary>
    private static void AddHollowRoom(
        List<StructurePiece> pieces,
        int originX,
        int originY,
        int roomWidth,
        int roomHeight,
        int gridWidth,
        int gridHeight)
    {
        AddFrameBay(
            pieces, originX, originY, roomWidth, roomHeight, gridWidth, gridHeight
        );
    }

    private static List<StructurePiece> BuildSteppedPyramid(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int baseWidth = random.Next(Mathf.Max(4, width * 2 / 3), width + 1);
        int storyHeight = random.Next(2, 4);
        int step = random.Next(1, 3);

        int currentWidth = baseWidth;
        int currentY = 0;

        /*
         * Each tier is an open frame rather than a filled slab, so the
         * pyramid is a stack of rooms that can pancake downward.
         */
        while (currentWidth >= 2 && currentY + storyHeight < height)
        {
            int startX = (width - currentWidth) / 2;

            AddFrameBay(
                pieces, startX, currentY, currentWidth, storyHeight, width, height
            );

            if (currentWidth >= 6)
            {
                int midX = startX + currentWidth / 2;

                for (int y = currentY; y < currentY + storyHeight && y < height; y++)
                {
                    pieces.Add(new StructurePiece(midX, y, 1, 1, RolePillar));
                }
            }

            currentY += storyHeight + 1;
            currentWidth -= step * 2;
        }

        return pieces;
    }

    private static List<StructurePiece> BuildCrenellatedKeep(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int keepWidth = random.Next(Mathf.Max(4, width / 2), width + 1);
        int startX = (width - keepWidth) / 2;
        int storyHeight = random.Next(2, 4);
        int stories = Mathf.Max(1, random.Next(2, 4));

        int currentY = 0;

        for (int story = 0; story < stories; story++)
        {
            if (currentY + storyHeight >= height)
            {
                break;
            }

            AddHollowRoom(
                pieces, startX, currentY, keepWidth, storyHeight, width, height
            );

            /*
             * Interior post every few columns - keeps a wide floor from
             * being a single unsupported span without filling the room.
             */
            if (keepWidth >= 7)
            {
                int midX = startX + keepWidth / 2;

                for (int y = currentY; y < currentY + storyHeight && y < height; y++)
                {
                    pieces.Add(new StructurePiece(midX, y, 1, 1, RolePillar));
                }
            }

            currentY += storyHeight + 1;
        }

        for (int x = startX; x < startX + keepWidth && x < width; x += 2)
        {
            if (currentY < height)
            {
                pieces.Add(new StructurePiece(x, currentY, 1, 1, RoleCrown));
            }
        }

        return pieces;
    }

    private static List<StructurePiece> BuildSkylineCluster(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int x = 0;

        while (x < width)
        {
            int towerWidth = random.Next(3, Mathf.Max(4, width / 3) + 1);
            towerWidth = Mathf.Min(towerWidth, width - x);

            if (towerWidth < 3)
            {
                break;
            }

            int towerHeight = random.Next(Mathf.Max(3, height / 3), height);
            int storyHeight = random.Next(2, 4);

            /*
             * Hollow shaft: walls and floors only, open core.
             */
            int currentY = 0;

            while (currentY + storyHeight < towerHeight && currentY + storyHeight < height)
            {
                AddFrameBay(
                    pieces, x, currentY, towerWidth, storyHeight, width, height
                );

                currentY += storyHeight + 1;
            }

            x += towerWidth + random.Next(1, 3);
        }

        return pieces;
    }

    /// <summary>
    /// Pillars carrying full-width horizontal planks - the signature
    /// "knock the legs out and the floor drops" structure of cannon
    /// physics puzzles.
    /// </summary>
    private static List<StructurePiece> BuildPlankShelfTower(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int shelfWidth = random.Next(Mathf.Max(3, width / 2), width + 1);
        int startX = (width - shelfWidth) / 2;
        int pillarHeight = random.Next(1, 3);
        int storyHeight = pillarHeight + 1;
        int stories = Mathf.Max(1, (height - 1) / storyHeight);

        for (int story = 0; story < stories; story++)
        {
            int baseY = story * storyHeight;

            int leftPillarX = startX;
            int rightPillarX = startX + shelfWidth - 1;
            int middlePillarX = startX + shelfWidth / 2;

            bool useMiddlePillar =
                shelfWidth >= 5 && random.Next(2) == 0;

            for (int y = baseY; y < baseY + pillarHeight; y++)
            {
                pieces.Add(new StructurePiece(leftPillarX, y, 1, 1, RolePillar));
                pieces.Add(new StructurePiece(rightPillarX, y, 1, 1, RolePillar));

                if (useMiddlePillar)
                {
                    pieces.Add(new StructurePiece(middlePillarX, y, 1, 1, RolePillar));
                }
            }

            int plankY = baseY + pillarHeight;

            if (plankY >= height)
            {
                break;
            }

            pieces.Add(new StructurePiece(startX, plankY, shelfWidth, 1, RoleBeam));
        }

        return pieces;
    }

    private static List<StructurePiece> BuildGateTower(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int gateWidth = random.Next(3, Mathf.Max(4, width) + 1);
        gateWidth = Mathf.Min(gateWidth, width);

        int startX = (width - gateWidth) / 2;
        int legWidth = Mathf.Max(1, gateWidth / 4);
        int gateHeight = random.Next(2, Mathf.Max(3, height / 2) + 1);

        for (int y = 0; y < gateHeight; y++)
        {
            for (int i = 0; i < legWidth; i++)
            {
                pieces.Add(new StructurePiece(startX + i, y, 1, 1, RolePillar));

                pieces.Add(new StructurePiece(
                    startX + gateWidth - 1 - i, y, 1, 1, RolePillar
                ));
            }
        }

        if (gateHeight < height)
        {
            pieces.Add(new StructurePiece(startX, gateHeight, gateWidth, 1, RoleBeam));
        }

        int upperHeight = random.Next(1, Mathf.Max(2, height - gateHeight - 1) + 1);

        for (int y = gateHeight + 1; y < gateHeight + 1 + upperHeight && y < height; y++)
        {
            for (int x = startX; x < startX + gateWidth && x < width; x++)
            {
                pieces.Add(new StructurePiece(x, y, 1, 1, RoleWall));
            }
        }

        return pieces;
    }

    private static List<StructurePiece> BuildTwinTowerBridge(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int towerWidth = Mathf.Max(1, width / 4);
        int towerHeight = random.Next(Mathf.Max(3, height / 2), height);
        int leftStart = 0;
        int rightStart = width - towerWidth;

        for (int y = 0; y < towerHeight; y++)
        {
            for (int i = 0; i < towerWidth; i++)
            {
                pieces.Add(new StructurePiece(leftStart + i, y, 1, 1, RolePillar));
                pieces.Add(new StructurePiece(rightStart + i, y, 1, 1, RolePillar));
            }
        }

        int bridgeY = random.Next(Mathf.Max(1, towerHeight / 2), towerHeight);

        pieces.Add(new StructurePiece(0, bridgeY, width, 1, RoleBeam));

        if (towerHeight < height)
        {
            pieces.Add(new StructurePiece(0, towerHeight, width, 1, RoleCrown));
        }

        return pieces;
    }

    private static List<StructurePiece> BuildFortressWall(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int wallHeight = random.Next(2, Mathf.Max(3, height / 2) + 1);
        int towerWidth = Mathf.Max(1, width / 5);
        int towerHeight = Mathf.Min(height, wallHeight + random.Next(2, 4));

        /*
         * Curtain wall as a row of open gate bays rather than a solid
         * slab - the wall still reads as a barrier but has voids the
         * player can collapse into.
         */
        int bayWidth = Mathf.Max(2, width / random.Next(2, 4));
        int bayX = 0;

        while (bayX + bayWidth <= width)
        {
            AddFrameBay(pieces, bayX, 0, bayWidth, wallHeight, width, height);
            bayX += bayWidth;
        }

        for (int y = wallHeight + 1; y < towerHeight && y < height; y++)
        {
            for (int i = 0; i < towerWidth; i++)
            {
                pieces.Add(new StructurePiece(i, y, 1, 1, RolePillar));
                pieces.Add(new StructurePiece(width - 1 - i, y, 1, 1, RolePillar));
            }
        }

        for (int x = 0; x < width; x += 2)
        {
            if (wallHeight < height)
            {
                pieces.Add(new StructurePiece(x, wallHeight, 1, 1, RoleCrown));
            }
        }

        return pieces;
    }

    private static List<StructurePiece> BuildTableStack(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int tableWidth = random.Next(Mathf.Max(3, width / 2), width + 1);
        int startX = (width - tableWidth) / 2;
        int legHeight = random.Next(1, 3);
        int storyHeight = legHeight + 1;
        int stories = Mathf.Max(1, (height - 1) / storyHeight);

        int currentWidth = tableWidth;
        int currentStart = startX;

        for (int story = 0; story < stories; story++)
        {
            int baseY = story * storyHeight;

            if (currentWidth < 2)
            {
                break;
            }

            for (int y = baseY; y < baseY + legHeight; y++)
            {
                pieces.Add(new StructurePiece(currentStart, y, 1, 1, RolePillar));

                pieces.Add(new StructurePiece(
                    currentStart + currentWidth - 1, y, 1, 1, RolePillar
                ));
            }

            int topY = baseY + legHeight;

            if (topY >= height)
            {
                break;
            }

            pieces.Add(new StructurePiece(currentStart, topY, currentWidth, 1, RoleBeam));

            int shrink = random.Next(0, 2) * 2;
            currentWidth -= shrink;
            currentStart += shrink / 2;
        }

        return pieces;
    }

    private static List<StructurePiece> BuildHollowKeep(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int keepWidth = random.Next(Mathf.Max(4, width / 2), width + 1);
        int keepHeight = random.Next(Mathf.Max(3, height / 2), height);
        int startX = (width - keepWidth) / 2;

        /*
         * Capped at 2: a wall 3+ cubes thick is a solid slab that eats
         * cannonballs rather than toppling.
         */
        int wallThickness = Mathf.Clamp(keepWidth / 5, 1, 2);

        for (int y = 0; y < keepHeight; y++)
        {
            for (int i = 0; i < wallThickness; i++)
            {
                pieces.Add(new StructurePiece(startX + i, y, 1, 1, RoleWall));

                pieces.Add(new StructurePiece(
                    startX + keepWidth - 1 - i, y, 1, 1, RoleWall
                ));
            }
        }

        if (keepHeight < height)
        {
            pieces.Add(new StructurePiece(startX, keepHeight, keepWidth, 1, RoleBeam));
        }

        int roofY = keepHeight + 1;

        if (roofY < height)
        {
            int roofWidth = Mathf.Max(1, keepWidth - 2);
            int roofStart = startX + 1;

            for (int x = roofStart; x < roofStart + roofWidth && x < width; x += 2)
            {
                pieces.Add(new StructurePiece(x, roofY, 1, 1, RoleCrown));
            }
        }

        return pieces;
    }

    private static List<StructurePiece> BuildLayeredCake(
        int width,
        int height,
        System.Random random)
    {
        List<StructurePiece> pieces = new List<StructurePiece>();

        int currentWidth = random.Next(Mathf.Max(4, width * 2 / 3), width + 1);
        int currentStart = (width - currentWidth) / 2;
        int y = 0;

        while (y < height && currentWidth >= 2)
        {
            pieces.Add(new StructurePiece(currentStart, y, currentWidth, 1, RoleBeam));
            y++;

            if (y >= height)
            {
                break;
            }

            int pillarCount = Mathf.Max(2, currentWidth / 3);
            int pillarHeight = random.Next(1, 3);

            for (int p = 0; p < pillarCount; p++)
            {
                int pillarX = currentStart +
                    Mathf.RoundToInt(
                        (float)p / Mathf.Max(1, pillarCount - 1) * (currentWidth - 1)
                    );

                for (int py = y; py < y + pillarHeight && py < height; py++)
                {
                    pieces.Add(new StructurePiece(pillarX, py, 1, 1, RolePillar));
                }
            }

            y += pillarHeight;

            int shrink = random.Next(0, 2) * 2;
            currentWidth -= shrink;
            currentStart += shrink / 2;
        }

        return pieces;
    }

    private static int CountCells(List<StructurePiece> pieces)
    {
        int total = 0;

        foreach (StructurePiece piece in pieces)
        {
            total += Mathf.Max(1, piece.SpanX) * Mathf.Max(1, piece.SpanY);
        }

        return total;
    }

    /// <summary>
    /// Flips a structure left-to-right so asymmetric archetypes don't
    /// always lean the same way across the whole level set.
    /// </summary>
    private static List<StructurePiece> MirrorHorizontally(
        List<StructurePiece> pieces,
        int width)
    {
        List<StructurePiece> mirrored = new List<StructurePiece>(pieces.Count);

        foreach (StructurePiece piece in pieces)
        {
            mirrored.Add(new StructurePiece(
                width - piece.X - piece.SpanX,
                piece.Y,
                piece.SpanX,
                piece.SpanY,
                piece.Role
            ));
        }

        return mirrored;
    }

    /// <summary>
    /// Drops pieces that overlap an already-claimed cell, then repeatedly
    /// drops any piece with nothing beneath its footprint until the whole
    /// structure is stable. Without this a generated level could spawn
    /// already-collapsing, which reads as broken rather than designed.
    /// </summary>
    private static List<StructurePiece> MakeStructurallySound(
        List<StructurePiece> pieces,
        int width,
        int height)
    {
        List<StructurePiece> kept = new List<StructurePiece>();
        bool[,] occupied = new bool[width, height];

        foreach (StructurePiece piece in pieces)
        {
            if (piece.X < 0 ||
                piece.Y < 0 ||
                piece.X + piece.SpanX > width ||
                piece.Y + piece.SpanY > height)
            {
                continue;
            }

            bool overlaps = false;

            for (int y = piece.Y; y < piece.Y + piece.SpanY && !overlaps; y++)
            {
                for (int x = piece.X; x < piece.X + piece.SpanX; x++)
                {
                    if (occupied[x, y])
                    {
                        overlaps = true;
                        break;
                    }
                }
            }

            if (overlaps)
            {
                continue;
            }

            for (int y = piece.Y; y < piece.Y + piece.SpanY; y++)
            {
                for (int x = piece.X; x < piece.X + piece.SpanX; x++)
                {
                    occupied[x, y] = true;
                }
            }

            kept.Add(piece);
        }

        bool removedAny = true;

        while (removedAny)
        {
            removedAny = false;

            for (int i = kept.Count - 1; i >= 0; i--)
            {
                StructurePiece piece = kept[i];

                if (piece.Y == 0)
                {
                    continue;
                }

                bool hasSupport = false;

                for (int x = piece.X; x < piece.X + piece.SpanX; x++)
                {
                    if (occupied[x, piece.Y - 1])
                    {
                        hasSupport = true;
                        break;
                    }
                }

                if (hasSupport)
                {
                    continue;
                }

                for (int y = piece.Y; y < piece.Y + piece.SpanY; y++)
                {
                    for (int x = piece.X; x < piece.X + piece.SpanX; x++)
                    {
                        occupied[x, y] = false;
                    }
                }

                kept.RemoveAt(i);
                removedAny = true;
            }
        }

        return kept;
    }

    /// <summary>
    /// Builds one structurally sound structure for a single depth layer,
    /// re-rolling parameters until it clears the minimum block floor.
    /// Soundness pruning can otherwise reduce a candidate to a handful
    /// of blocks, which plays as an already-solved level.
    /// </summary>
    private List<StructurePiece> BuildSoundStructure(
        int width,
        int height,
        int archetypeIndex,
        int levelNumber,
        int layerIndex,
        int minimumBlocks)
    {
        List<StructurePiece> best = null;
        int bestCellCount = -1;

        for (int attempt = 0; attempt < 12; attempt++)
        {
            System.Random attemptRandom =
                new System.Random(
                    MixSeed(
                        randomSeed + attempt * 104729 + layerIndex * 15485863,
                        levelNumber
                    )
                );

            List<StructurePiece> candidate =
                BuildComposedStructure(width, height, archetypeIndex, attemptRandom);

            if (attemptRandom.Next(2) == 0)
            {
                candidate = MirrorHorizontally(candidate, width);
            }

            candidate = MakeStructurallySound(candidate, width, height);

            int cellCount = CountCells(candidate);

            if (cellCount > bestCellCount)
            {
                bestCellCount = cellCount;
                best = candidate;
            }

            if (cellCount >= minimumBlocks)
            {
                break;
            }
        }

        return best ?? new List<StructurePiece>();
    }

    /// <summary>
    /// Paints every depth layer's own structure. Colors are chosen by
    /// structural role rather than per cell, so a tower reads as
    /// deliberately built (pillars one color, beams another) instead of
    /// as random confetti.
    /// </summary>
    private void PaintLayers(
        GridLevelData level,
        List<List<StructurePiece>> layerPieces,
        int shapesAvailable,
        System.Random random)
    {
        Color[] roleColors = new Color[4];
        int firstColor = random.Next(DefaultColors.Length);

        for (int i = 0; i < roleColors.Length; i++)
        {
            roleColors[i] = DefaultColors[(firstColor + i) % DefaultColors.Length];
        }

        int beamShape = random.Next(shapesAvailable);
        int pillarShape = random.Next(shapesAvailable);

        for (int z = 0; z < layerPieces.Count; z++)
        {
            foreach (StructurePiece piece in layerPieces[z])
            {
                Color color =
                    roleColors[Mathf.Clamp(piece.Role, 0, roleColors.Length - 1)];

                int shapeIndex =
                    piece.Role == RoleBeam
                        ? beamShape
                        : piece.Role == RolePillar
                            ? pillarShape
                            : random.Next(shapesAvailable);

                if (piece.SpanX > 1 || piece.SpanY > 1)
                {
                    level.EditorPaintSpan(
                        piece.X,
                        piece.Y,
                        z,
                        piece.SpanX,
                        piece.SpanY,
                        1,
                        color,
                        shapeIndex
                    );
                }
                else
                {
                    level.EditorSetCell(piece.X, piece.Y, z, true, color, shapeIndex);
                }
            }
        }

        level.RecalculateGridMetadata();
    }

    private static GridLevelDatabase GetOrCreateLevelDatabase()
    {
        GridLevelDatabase database =
            AssetDatabase.LoadAssetAtPath<GridLevelDatabase>(
                GridLevelAddressablesEditorUtility.DatabasePath
            );

        if (database != null)
        {
            return database;
        }

        database = ScriptableObject.CreateInstance<GridLevelDatabase>();

        AssetDatabase.CreateAsset(database, GridLevelAddressablesEditorUtility.DatabasePath);
        AssetDatabase.SaveAssets();

        return database;
    }
}

#endif
