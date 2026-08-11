#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridLevelData))]
public sealed class GridLevelDataEditor : Editor
{
    private const string LevelDatabasePath =
        "Assets/Canon Controle/GridLevelDatabase.asset";

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

    private const float CellButtonSize = 18f;


    public override void OnInspectorGUI()
    {
        GridLevelData levelData =
            (GridLevelData)target;

        DrawLevelCreatorSection(levelData);

        DrawDefaultInspector();

        DrawImageBakeSection(levelData);
        DrawManualPainterSection(levelData);
        DrawSaveLevelSection(levelData);
    }


    private void DrawLevelCreatorSection(
        GridLevelData levelData)
    {
        GridLevelDatabase database =
            GetOrCreateLevelDatabase();

        EnsureDatabaseUsesSnapshots(database);

        EditorGUILayout.LabelField(
            "Level Creator",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            $"Saved Levels: {database.Count}\n" +
            "Yeh GridLevelData single working canvas hai. SAVE LEVEL " +
            "database mein snapshot create/update karta hai.",
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
                database.GetLevelByNumber(
                    levelNumberToEdit
                ) == null))
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

        EnsureDatabaseUsesSnapshots(database);

        int levelNumber =
            Mathf.Max(1, workingLevel.LevelNumber);

        GridLevelData savedLevel =
            database.GetLevelByNumber(levelNumber);

        bool isNewSnapshot = savedLevel == null;

        if (isNewSnapshot)
        {
            savedLevel =
                CreateInstance<GridLevelData>();

            AssetDatabase.AddObjectToAsset(
                savedLevel,
                database
            );
        }

        EditorUtility.CopySerialized(
            workingLevel,
            savedLevel
        );

        savedLevel.name =
            $"SavedLevel_{levelNumber:000}";

        savedLevel.hideFlags =
            HideFlags.HideInHierarchy;

        savedLevel.EditorSetLevelNumber(levelNumber);

        Undo.RecordObject(
            database,
            "Save Grid Level Snapshot"
        );

        database.EditorRegisterLevel(savedLevel);

        EditorUtility.SetDirty(savedLevel);
        EditorUtility.SetDirty(database);
        EditorUtility.SetDirty(workingLevel);
        AssetDatabase.SaveAssets();

        Debug.Log(
            isNewSnapshot
                ? $"Level {levelNumber} saved in central database."
                : $"Level {levelNumber} updated in central database.",
            workingLevel
        );
    }


    private static void LoadSavedLevelForEdit(
        GridLevelData workingLevel,
        GridLevelDatabase database,
        int levelNumber)
    {
        EnsureDatabaseUsesSnapshots(database);

        GridLevelData savedLevel =
            database.GetLevelByNumber(levelNumber);

        if (savedLevel == null)
        {
            Debug.LogWarning(
                $"Saved Level {levelNumber} database mein nahi mila.",
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
            GridLevelData savedLevel = database.GetLevel(i);

            if (savedLevel != null)
            {
                nextLevelNumber = Mathf.Max(
                    nextLevelNumber,
                    savedLevel.LevelNumber + 1
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


    private static void EnsureDatabaseUsesSnapshots(
        GridLevelDatabase database)
    {
        if (database == null)
        {
            return;
        }

        bool changed = false;

        for (int i = 0; i < database.Count; i++)
        {
            GridLevelData existingLevel =
                database.GetLevel(i);

            if (existingLevel == null ||
                AssetDatabase.GetAssetPath(existingLevel) ==
                LevelDatabasePath)
            {
                continue;
            }

            GridLevelData snapshot =
                CreateInstance<GridLevelData>();

            EditorUtility.CopySerialized(
                existingLevel,
                snapshot
            );

            snapshot.name =
                $"SavedLevel_{existingLevel.LevelNumber:000}";

            snapshot.hideFlags =
                HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(
                snapshot,
                database
            );

            database.EditorReplaceLevel(
                existingLevel,
                snapshot
            );

            EditorUtility.SetDirty(snapshot);
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }
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
                  $"SAVE LEVEL database mein Level " +
                  $"{levelData.LevelNumber} create/update karega.",
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


    private void DrawImageBakeSection(
        GridLevelData levelData)
    {
        EditorGUILayout.Space(15f);

        EditorGUILayout.LabelField(
            "Image → 3D Grid Baker",
            EditorStyles.boldLabel
        );


        if (levelData.SourceImage == null)
        {
            EditorGUILayout.HelpBox(
                "Source Image assign karein.",
                MessageType.Warning
            );
        }


        using (
            new EditorGUI.DisabledScope(
                levelData.SourceImage == null))
        {
            if (GUILayout.Button(
                    "BAKE 3D GRID FROM IMAGE",
                    GUILayout.Height(38f)))
            {
                BakeGrid(levelData);
            }
        }


        EditorGUILayout.Space(8f);

        if (levelData.HasValidBakedGrid)
        {
            EditorGUILayout.HelpBox(
                $"Baked Grid: " +
                $"{levelData.GridWidth} x " +
                $"{levelData.GridHeight} x " +
                $"{levelData.GridDepth}\n" +
                $"Occupied Boxes: " +
                $"{levelData.BakedOccupiedCellCount}",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Grid abhi bake nahi hui. " +
                "Source Image se bake karein, ya neeche " +
                "Manual Level Painter se hath se design karein.",
                MessageType.Warning
            );
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
            "Image ki zaroorat nahi — grid ko directly paint karein.",
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

        EditorGUILayout.HelpBox(
            "Upright = as authored (tall pieces stand up). " +
            "Lying (X) lays a tall piece along X. " +
            "X 90 rotates the piece 90 degrees around its X axis.",
            MessageType.None
        );

        EditorGUILayout.HelpBox(
            "Y 90/180/270 rotates an upright piece around its Y axis.",
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
                                seamOffset
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

                    levelData.RecalculateBakedMetadata();
                    EditorUtility.SetDirty(levelData);
                }

                GUI.backgroundColor =
                    previousBackground;
            }

            EditorGUILayout.EndHorizontal();
        }
    }


    private static void BakeGrid(
        GridLevelData levelData)
    {
        Texture2D texture =
            levelData.SourceImage;


        if (texture == null)
        {
            return;
        }


        string assetPath =
            AssetDatabase.GetAssetPath(
                texture
            );


        TextureImporter importer =
            AssetImporter.GetAtPath(
                assetPath
            ) as TextureImporter;


        bool restoreReadability =
            false;


        try
        {
            /*
             * User ko manually Read/Write ON
             * karne ki zaroorat nahi.
             */
            if (importer != null &&
                !importer.isReadable)
            {
                restoreReadability =
                    true;


                importer.isReadable =
                    true;


                importer.SaveAndReimport();
            }


            Undo.RecordObject(
                levelData,
                "Bake Grid Level From Image"
            );


            levelData
                .EditorBakeFromReadableSourceImage();


            EditorUtility.SetDirty(
                levelData
            );


            AssetDatabase.SaveAssets();


            SceneView.RepaintAll();
        }
        catch (System.Exception exception)
        {
            Debug.LogException(
                exception,
                levelData
            );
        }
        finally
        {
            /*
             * Texture ki original import setting
             * restore kar dete hain.
             */
            if (importer != null &&
                restoreReadability)
            {
                importer.isReadable =
                    false;


                importer.SaveAndReimport();
            }
        }
    }
}

#endif
