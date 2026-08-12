#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public sealed class LevelProductionPipelineWindow : EditorWindow
{
    private const string ArchetypeFolder =
        "Assets/Canon Controle/Archetypes";

    private GridLevelDatabase database;
    private GridLevelData selectedFoundationLevel;
    private GridLevelArchetypeKind newTemplateKind;
    private int testBatchStartLevel = 11;
    private int testBatchCount = 20;
    private int generationSeed = 12345;
    private bool testBatchPlaytestedAndApproved;
    private int bulkStartLevel = 31;
    private int bulkEndLevel = 1000;
    private Vector2 scrollPosition;
    private readonly List<string> qualityControlResults =
        new List<string>();


    [MenuItem("Tools/Royal Smash/Level Production Pipeline")]
    public static void Open()
    {
        GetWindow<LevelProductionPipelineWindow>(
            "Level Pipeline"
        );
    }


    private void OnEnable()
    {
        database = AssetDatabase.LoadAssetAtPath<GridLevelDatabase>(
            GridLevelAddressablesEditorUtility.DatabasePath
        );

        if (Selection.activeObject is GridLevelData selected)
        {
            selectedFoundationLevel = selected;
        }
    }


    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField(
            "Royal Smash Level Production Pipeline",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Manual playtesting first, approved archetypes second, " +
            "small generated batch third. Bulk generation stays locked " +
            "until the test batch is explicitly approved.",
            MessageType.Info
        );

        database = (GridLevelDatabase)EditorGUILayout.ObjectField(
            "Level Database",
            database,
            typeof(GridLevelDatabase),
            false
        );

        DrawFoundationPhase();
        DrawArchetypePhase();
        DrawTestBatchPhase();
        DrawBulkPhase();
        DrawQualityControlPhase();

        EditorGUILayout.EndScrollView();
    }


    private void DrawFoundationPhase()
    {
        EditorGUILayout.Space(14f);
        EditorGUILayout.LabelField(
            "Phase 1 - Manual Foundation Levels",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Manual workflow: blank working grid par Level 1 design karein, " +
            "SAVE LEVEL karein, phir NEW BLANK LEVEL se agla number banayein. " +
            "Level 1-15 tak isi tarah hand-design aur playtest karein.",
            MessageType.Info
        );

        if (GUILayout.Button("RESET ALL SAVED LEVELS + BLANK LEVEL 1"))
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete All Saved Levels?",
                "All Assets/Canon Controle/Levels/Level_XXX assets, " +
                "database entries and their Addressables entries delete " +
                "hon gi. Working grid blank Level 1 ban jayegi. This cannot " +
                "be undone through the Unity Undo system.",
                "Delete Saved Levels",
                "Cancel"
            );

            if (confirmed)
            {
                GridLevelAddressablesEditorUtility.DeleteAllSavedLevels(
                    database,
                    true
                );
                selectedFoundationLevel =
                    AssetDatabase.LoadAssetAtPath<GridLevelData>(
                        GridLevelAddressablesEditorUtility.WorkingLevelPath
                    );
                Repaint();
            }
        }

        selectedFoundationLevel =
            (GridLevelData)EditorGUILayout.ObjectField(
                "Foundation Level",
                selectedFoundationLevel,
                typeof(GridLevelData),
                false
            );

        if (database == null)
        {
            EditorGUILayout.HelpBox(
                "GridLevelDatabase assign karein.",
                MessageType.Warning
            );
            return;
        }

        int found = 0;
        int playtested = 0;
        int approved = 0;

        for (int levelNumber = 1; levelNumber <= 15; levelNumber++)
        {
            GridLevelData level =
                GridLevelAddressablesEditorUtility.GetLevelAsset(
                    database,
                    levelNumber
                );

            if (level == null)
            {
                continue;
            }

            found++;

            if (level.PlaytestAttempts > 0 ||
                level.ReviewStatus != LevelReviewStatus.Untested)
            {
                playtested++;
            }

            if (level.ReviewStatus ==
                LevelReviewStatus.ApprovedAsArchetype)
            {
                approved++;
            }
        }

        EditorGUILayout.HelpBox(
            $"Foundation assets: {found}/15 | Playtested: {playtested} | " +
            $"Archetype winners: {approved}. Ratings aur notes selected " +
            "level ke normal Inspector mein Design QA section se fill karein.",
            approved >= 3 ? MessageType.Info : MessageType.Warning
        );

        using (new EditorGUI.DisabledScope(selectedFoundationLevel == null))
        {
            if (GUILayout.Button("VALIDATE SELECTED FOUNDATION LEVEL"))
            {
                bool valid = GridLevelStabilityValidator.Validate(
                    selectedFoundationLevel,
                    out string summary
                );

                if (valid)
                {
                    Debug.Log(summary, selectedFoundationLevel);
                }
                else
                {
                    Debug.LogError(summary, selectedFoundationLevel);
                }

                EditorUtility.DisplayDialog(
                    valid ? "Structure Valid" : "Structure Invalid",
                    summary,
                    "OK"
                );
            }
        }
    }


    private void DrawArchetypePhase()
    {
        EditorGUILayout.Space(14f);
        EditorGUILayout.LabelField(
            "Phase 2 - Winning Archetype Templates",
            EditorStyles.boldLabel
        );

        newTemplateKind =
            (GridLevelArchetypeKind)EditorGUILayout.EnumPopup(
                "Template Family",
                newTemplateKind
            );

        bool sourceApproved =
            selectedFoundationLevel != null &&
            selectedFoundationLevel.ReviewStatus ==
                LevelReviewStatus.ApprovedAsArchetype;

        using (new EditorGUI.DisabledScope(!sourceApproved))
        {
            if (GUILayout.Button("CREATE TEMPLATE FROM APPROVED LEVEL"))
            {
                CreateTemplateFromSelectedLevel();
            }
        }

        if (selectedFoundationLevel != null && !sourceApproved)
        {
            EditorGUILayout.HelpBox(
                "Template banane se pehle selected level ka Review Status " +
                "Approved As Archetype set karein.",
                MessageType.Warning
            );
        }

        List<GridLevelArchetypeTemplate> templates = FindTemplates(false);
        int approvedCount = 0;

        foreach (GridLevelArchetypeTemplate template in templates)
        {
            bool ready = IsTemplateReady(template);

            if (ready)
            {
                approvedCount++;
            }

            EditorGUILayout.ObjectField(
                template.DisplayName +
                (ready ? " [READY]" : " [DRAFT]"),
                template,
                typeof(GridLevelArchetypeTemplate),
                false
            );
        }

        EditorGUILayout.HelpBox(
            $"Templates: {templates.Count} | Approved for generation: " +
            $"{approvedCount}. Recommended: 3-5 proven templates.",
            approvedCount >= 3 ? MessageType.Info : MessageType.Warning
        );
    }


    private void DrawTestBatchPhase()
    {
        EditorGUILayout.Space(14f);
        EditorGUILayout.LabelField(
            "Phases 3-5 - Validated Test Batch",
            EditorStyles.boldLabel
        );

        generationSeed = EditorGUILayout.IntField(
            "Master Seed",
            generationSeed
        );
        testBatchStartLevel = Mathf.Max(
            11,
            EditorGUILayout.IntField(
                "Start Level",
                testBatchStartLevel
            )
        );
        testBatchCount = EditorGUILayout.IntSlider(
            "Test Level Count",
            testBatchCount,
            20,
            30
        );

        List<GridLevelArchetypeTemplate> approvedTemplates =
            FindTemplates(true);
        bool canGenerate =
            database != null && approvedTemplates.Count >= 3;

        using (new EditorGUI.DisabledScope(!canGenerate))
        {
            if (GUILayout.Button(
                    $"GENERATE + VALIDATE {testBatchCount} TEST LEVELS",
                    GUILayout.Height(38f)))
            {
                int lastLevel =
                    testBatchStartLevel + testBatchCount - 1;

                if (EditorUtility.DisplayDialog(
                        "Generate Test Batch?",
                        $"Levels {testBatchStartLevel}-{lastLevel} overwrite " +
                        "ho sakte hain. Har level stability aur duplicate " +
                        "validation pass karne ke baad hi save hoga.",
                        "Generate",
                        "Cancel"))
                {
                    GenerateBatch(
                        testBatchStartLevel,
                        testBatchCount,
                        approvedTemplates,
                        "Test Batch"
                    );
                }
            }
        }

        if (approvedTemplates.Count < 3)
        {
            EditorGUILayout.HelpBox(
                "Small batch se pehle kam az kam 3 winning templates ka " +
                "Approved For Generation ON hona required hai.",
                MessageType.Warning
            );
        }
    }


    private void DrawBulkPhase()
    {
        EditorGUILayout.Space(14f);
        EditorGUILayout.LabelField(
            "Phase 6 - Gated Bulk Generation",
            EditorStyles.boldLabel
        );

        testBatchPlaytestedAndApproved = EditorGUILayout.ToggleLeft(
            "I manually played the 20-30 test levels and approved the results",
            testBatchPlaytestedAndApproved
        );

        bulkStartLevel = Mathf.Max(
            11,
            EditorGUILayout.IntField("Bulk Start Level", bulkStartLevel)
        );
        bulkEndLevel = Mathf.Clamp(
            EditorGUILayout.IntField("Bulk End Level", bulkEndLevel),
            bulkStartLevel,
            1000
        );

        List<GridLevelArchetypeTemplate> approvedTemplates =
            FindTemplates(true);
        bool canGenerateBulk =
            testBatchPlaytestedAndApproved &&
            database != null &&
            approvedTemplates.Count >= 3;

        using (new EditorGUI.DisabledScope(!canGenerateBulk))
        {
            if (GUILayout.Button(
                    "GENERATE APPROVED BULK RANGE",
                    GUILayout.Height(38f)))
            {
                int count = bulkEndLevel - bulkStartLevel + 1;

                if (EditorUtility.DisplayDialog(
                        "Generate Bulk Levels?",
                        $"{count} levels ({bulkStartLevel}-{bulkEndLevel}) " +
                        "generate/save honge. Existing assets overwrite ho " +
                        "sakte hain. Continue only after test-batch approval.",
                        "Generate Bulk",
                        "Cancel"))
                {
                    GenerateBatch(
                        bulkStartLevel,
                        count,
                        approvedTemplates,
                        "Bulk Generation"
                    );
                }
            }
        }
    }


    private void DrawQualityControlPhase()
    {
        EditorGUILayout.Space(14f);
        EditorGUILayout.LabelField(
            "Phase 7 - Every 50th Level Spot Check",
            EditorStyles.boldLabel
        );

        using (new EditorGUI.DisabledScope(database == null))
        {
            if (GUILayout.Button("SCAN LEVELS 50, 100, 150 ... 1000"))
            {
                RunQualityControlScan();
            }
        }

        foreach (string result in qualityControlResults)
        {
            EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel);
        }
    }


    private void CreateTemplateFromSelectedLevel()
    {
        EnsureArchetypeFolderExists();

        GridLevelArchetypeTemplate template =
            CreateInstance<GridLevelArchetypeTemplate>();
        template.EditorCapture(selectedFoundationLevel, newTemplateKind);

        string path = AssetDatabase.GenerateUniqueAssetPath(
            $"{ArchetypeFolder}/" +
            $"Archetype_{newTemplateKind}.asset"
        );

        AssetDatabase.CreateAsset(template, path);
        EditorUtility.SetDirty(template);
        AssetDatabase.SaveAssets();
        Selection.activeObject = template;
        EditorGUIUtility.PingObject(template);
    }


    private void GenerateBatch(
        int startLevel,
        int count,
        List<GridLevelArchetypeTemplate> templates,
        string operationName)
    {
        HashSet<string> signatures =
            LoadExistingSignatures(startLevel, startLevel + count - 1);
        int generatedCount = 0;
        string lastFailure = string.Empty;

        try
        {
            GridLevelAddressablesEditorUtility
                .EnsurePerLevelAddressableAssets(database);

            for (int offset = 0; offset < count; offset++)
            {
                int levelNumber = startLevel + offset;

                if (EditorUtility.DisplayCancelableProgressBar(
                        operationName,
                        $"Level {levelNumber} ({offset + 1}/{count})",
                        (float)offset / Mathf.Max(1, count)))
                {
                    lastFailure = "Generation user ne cancel ki.";
                    break;
                }

                if (!TryGenerateUniqueLevel(
                        levelNumber,
                        templates,
                        signatures,
                        out GridLevelData generated,
                        out lastFailure))
                {
                    Debug.LogError(
                        $"{operationName} Level {levelNumber} par ruki: " +
                        lastFailure
                    );
                    break;
                }

                GridLevelAddressablesEditorUtility.SaveWorkingLevelInBatch(
                    generated,
                    database,
                    out bool created
                );

                DestroyImmediate(generated);
                generatedCount++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        string summary = generatedCount == count
            ? $"{operationName} complete: {generatedCount} validated " +
              "Addressable levels saved."
            : $"{operationName} incomplete: {generatedCount}/{count}. " +
              lastFailure;

        Debug.Log(summary, database);
        EditorUtility.DisplayDialog(operationName, summary, "OK");
    }


    private bool TryGenerateUniqueLevel(
        int levelNumber,
        List<GridLevelArchetypeTemplate> templates,
        HashSet<string> signatures,
        out GridLevelData generated,
        out string failure)
    {
        generated = null;
        failure = string.Empty;
        int difficulty = GetDifficulty(levelNumber);

        const int maximumAttempts = 80;

        for (int attempt = 0; attempt < maximumAttempts; attempt++)
        {
            int attemptSeed = unchecked(
                generationSeed +
                levelNumber * 104729 +
                attempt * 48611
            );

            GridLevelArchetypeTemplate template =
                SelectTemplate(
                    templates,
                    difficulty,
                    attemptSeed,
                    attempt
                );

            if (template == null || template.SourceLevel == null)
            {
                failure =
                    "Eligible template ya uska Source Level missing hai.";
                return false;
            }

            GridLevelData candidate = CreateInstance<GridLevelData>();
            EditorUtility.CopySerialized(template.SourceLevel, candidate);
            candidate.EditorSetLevelNumber(levelNumber);

            System.Random random = new System.Random(attemptSeed);
            float difficulty01 = (difficulty - 1) / 9f;
            Vector2Int rowRange = template.RowRange;
            Vector2Int widthRange = template.BaseWidthRange;
            int rows = PickProgressiveValue(
                rowRange,
                difficulty01,
                random
            );
            int baseWidth = PickProgressiveValue(
                widthRange,
                difficulty01,
                random
            );
            int maximumShapeTypes = PickProgressiveValue(
                template.ShapeTypeRange,
                difficulty01,
                random
            );
            float horizontalGap = PickFloatRange(
                template.HorizontalGapRange,
                random
            );
            float verticalGap = PickFloatRange(
                template.VerticalGapRange,
                random
            );

            candidate.EditorSetPhysicalGaps(
                horizontalGap,
                verticalGap
            );
            int layers = Mathf.Clamp(
                1 + Mathf.FloorToInt(
                    difficulty01 * template.MaximumDepthLayers
                ),
                1,
                template.MaximumDepthLayers
            );
            bool useSeams =
                random.NextDouble() <
                template.SeamChanceAtHighDifficulty * difficulty01;
            bool useTwoTables =
                levelNumber >= template.TwoTableMinimumLevel &&
                random.NextDouble() <
                template.TwoTableChanceAtHighDifficulty * difficulty01;

            int variant =
                Mathf.Max(0, levelNumber - 1 + attempt) /
                Mathf.Max(1, templates.Count);
            int archetypeIndex =
                (int)template.ArchetypeKind +
                variant *
                GridLevelProceduralGenerator.StructuralPatternCount;

            GridLevelProceduralGenerator.Settings settings =
                new GridLevelProceduralGenerator.Settings(
                    attemptSeed,
                    difficulty,
                    template.SourceLevel.GridWidth,
                    rows,
                    Mathf.Clamp(
                        baseWidth,
                        2,
                        template.SourceLevel.GridWidth - 2
                    ),
                    layers,
                    template.SourceLevel.DepthGap,
                    useSeams,
                    archetypeIndex,
                    useTwoTables,
                    template.SourceLevel.SecondTableGap,
                    template.SourceLevel.TwoTableForwardOffset,
                    maximumShapeTypes,
                    template.ColorPattern
                );

            bool built = GridLevelProceduralGenerator.Generate(
                candidate,
                settings,
                out string generationSummary
            );

            if (!built)
            {
                failure = generationSummary;
                DestroyImmediate(candidate);
                continue;
            }

            if (!GridLevelStabilityValidator.Validate(
                    candidate,
                    out string validationSummary))
            {
                failure = validationSummary;
                DestroyImmediate(candidate);
                continue;
            }

            string signature = BuildStructuralSignature(candidate);

            if (!signatures.Add(signature))
            {
                failure = "Duplicate structure; another seed tried.";
                DestroyImmediate(candidate);
                continue;
            }

            candidate.EditorMarkProcedural(
                template.DisplayName,
                attemptSeed,
                difficulty
            );
            generated = candidate;
            return true;
        }

        failure =
            $"{maximumAttempts} attempts ke baad unique stable layout " +
            "nahi mila. Template variation ranges tune karein.";
        return false;
    }


    private HashSet<string> LoadExistingSignatures(
        int overwriteStart,
        int overwriteEnd)
    {
        HashSet<string> signatures = new HashSet<string>();

        if (database == null)
        {
            return signatures;
        }

        for (int i = 0; i < database.Count; i++)
        {
            int levelNumber = database.GetLevelNumber(i);

            if (levelNumber >= overwriteStart &&
                levelNumber <= overwriteEnd)
            {
                continue;
            }

            GridLevelData level =
                GridLevelAddressablesEditorUtility.GetLevelAsset(
                    database,
                    levelNumber
                );

            if (level != null)
            {
                signatures.Add(BuildStructuralSignature(level));
            }
        }

        return signatures;
    }


    private static GridLevelArchetypeTemplate SelectTemplate(
        List<GridLevelArchetypeTemplate> templates,
        int difficulty,
        int seed,
        int attempt)
    {
        List<GridLevelArchetypeTemplate> eligible =
            new List<GridLevelArchetypeTemplate>();
        float totalWeight = 0f;

        foreach (GridLevelArchetypeTemplate template in templates)
        {
            if (template == null ||
                template.SourceLevel == null ||
                difficulty < template.MinimumDifficulty ||
                difficulty > template.MaximumDifficulty)
            {
                continue;
            }

            eligible.Add(template);
            totalWeight += template.SelectionWeight;
        }

        if (eligible.Count == 0)
        {
            return null;
        }

        // Attempt offset prevents one high-weight template monopolizing retries.
        System.Random random = new System.Random(
            unchecked(seed + attempt * 8191)
        );
        float pick = (float)random.NextDouble() * totalWeight;

        foreach (GridLevelArchetypeTemplate template in eligible)
        {
            pick -= template.SelectionWeight;

            if (pick <= 0f)
            {
                return template;
            }
        }

        return eligible[eligible.Count - 1];
    }


    private static int PickProgressiveValue(
        Vector2Int range,
        float difficulty01,
        System.Random random)
    {
        int center = Mathf.RoundToInt(
            Mathf.Lerp(range.x, range.y, difficulty01)
        );
        int variation = random.Next(-1, 2);
        return Mathf.Clamp(center + variation, range.x, range.y);
    }


    private static float PickFloatRange(
        Vector2 range,
        System.Random random)
    {
        return Mathf.Lerp(
            range.x,
            range.y,
            (float)random.NextDouble()
        );
    }


    private static int GetDifficulty(int levelNumber)
    {
        float progress = Mathf.Clamp01((levelNumber - 1) / 999f);
        float eased = Mathf.Pow(progress, 0.65f);
        return Mathf.Clamp(
            1 + Mathf.FloorToInt(eased * 9f),
            1,
            10
        );
    }


    private void RunQualityControlScan()
    {
        qualityControlResults.Clear();

        for (int levelNumber = 50;
             levelNumber <= 1000;
             levelNumber += 50)
        {
            GridLevelData level =
                GridLevelAddressablesEditorUtility.GetLevelAsset(
                    database,
                    levelNumber
                );

            if (level == null)
            {
                qualityControlResults.Add(
                    $"Level {levelNumber}: MISSING"
                );
                continue;
            }

            bool valid = GridLevelStabilityValidator.Validate(
                level,
                out string validation
            );

            qualityControlResults.Add(
                $"Level {levelNumber}: " +
                $"{(valid ? "VALID" : "INVALID")} | " +
                $"Review: {level.ReviewStatus} | " +
                $"Template: {level.GeneratedFromTemplate} | " +
                validation
            );
        }

        Repaint();
    }


    private static List<GridLevelArchetypeTemplate> FindTemplates(
        bool approvedOnly)
    {
        List<GridLevelArchetypeTemplate> templates =
            new List<GridLevelArchetypeTemplate>();
        string[] guids = AssetDatabase.FindAssets(
            "t:GridLevelArchetypeTemplate"
        );

        foreach (string guid in guids)
        {
            GridLevelArchetypeTemplate template =
                AssetDatabase.LoadAssetAtPath<GridLevelArchetypeTemplate>(
                    AssetDatabase.GUIDToAssetPath(guid)
                );

            if (template != null &&
                (!approvedOnly || IsTemplateReady(template)))
            {
                templates.Add(template);
            }
        }

        templates.Sort((left, right) =>
            string.Compare(
                left.DisplayName,
                right.DisplayName,
                StringComparison.OrdinalIgnoreCase
            )
        );
        return templates;
    }


    private static bool IsTemplateReady(
        GridLevelArchetypeTemplate template)
    {
        if (template == null ||
            !template.ApprovedForGeneration ||
            template.SourceLevel == null)
        {
            return false;
        }

        LevelReviewStatus sourceStatus =
            template.SourceLevel.ReviewStatus;

        return sourceStatus == LevelReviewStatus.ApprovedAsArchetype ||
               sourceStatus == LevelReviewStatus.ProductionApproved;
    }


    private static string BuildStructuralSignature(
        GridLevelData level)
    {
        StringBuilder signature = new StringBuilder(2048);
        signature.Append(level.UseSecondTable ? "T2|" : "T1|");
        signature.Append(level.GridWidth).Append('x');
        signature.Append(level.GridHeight).Append('x');
        signature.Append(level.GridDepth).Append('|');

        for (int z = 0; z < level.GridDepth; z++)
        {
            for (int y = 0; y < level.GridHeight; y++)
            {
                for (int x = 0; x < level.GridWidth; x++)
                {
                    GridCellData cell = level.GetCell(x, y, z);

                    if (cell == null ||
                        !cell.Occupied ||
                        cell.IsCovered)
                    {
                        continue;
                    }

                    signature.Append(x).Append(',');
                    signature.Append(y).Append(',');
                    signature.Append(z).Append(':');
                    signature.Append(cell.SpanX).Append(',');
                    signature.Append(cell.SpanY).Append(',');
                    signature.Append(cell.SpanZ).Append(':');
                    signature.Append(
                        Mathf.RoundToInt(cell.LocalOffset.x * 100f)
                    ).Append(',');
                    signature.Append(
                        Mathf.RoundToInt(cell.LocalOffset.z * 100f)
                    ).Append(';');
                }
            }
        }

        return signature.ToString();
    }


    private static void EnsureArchetypeFolderExists()
    {
        if (!AssetDatabase.IsValidFolder(ArchetypeFolder))
        {
            AssetDatabase.CreateFolder(
                "Assets/Canon Controle",
                "Archetypes"
            );
        }
    }
}

#endif
