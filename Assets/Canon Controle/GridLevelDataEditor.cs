#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridLevelData))]
public sealed class GridLevelDataEditor : Editor
{
    private const string LevelDatabasePath =
        GridLevelAddressablesEditorUtility.DatabasePath;

    private const string LivePreviewSessionKey =
        "RoyalSmash.LiveWorkingLevelPreview";

    private const string LivePreviewLaunchSessionKey =
        "RoyalSmash.LiveWorkingLevelPreviewLaunch";

    private static bool livePreviewRefreshQueued;

    private int paintLayerZ;
    private int paintTableIndex;
    private int paintTierY;
    private Color paintColor = Color.white;
    private int paintDefinitionIndex;
    private bool eraseMode;
    private bool editPlacedBlockMode;
    private Vector3Int selectedCellCoordinate =
        new Vector3Int(-1, -1, -1);
    private int selectedBlockTableIndex;
    private string blockMoveFeedback = string.Empty;

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

    private const float CellButtonSize = 18f;


    public override void OnInspectorGUI()
    {
        GridLevelData levelData =
            (GridLevelData)target;

        if (levelData.EditorMigrateLegacyTable())
        {
            serializedObject.Update();
            EditorUtility.SetDirty(levelData);
        }

        if (levelData.EditorMigrateSharedGridTableAssignments())
        {
            serializedObject.Update();
            EditorUtility.SetDirty(levelData);
        }

        EditorGUI.BeginChangeCheck();

        DrawLiveGamePreviewSection(levelData);

        DrawLevelCreatorSection(levelData);

        DrawDefaultInspector();

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

                SessionState.SetBool(
                    LivePreviewLaunchSessionKey,
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

                    SessionState.SetBool(
                        LivePreviewLaunchSessionKey,
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


    private static void NotifyLevelDesignChanged(
        GridLevelData levelData)
    {
        if (levelData == null)
        {
            return;
        }

        EditorUtility.SetDirty(levelData);

        if (Application.isPlaying &&
            SessionState.GetBool(
                LivePreviewSessionKey,
                false
            ))
        {
            QueueLiveGamePreviewRefresh(levelData);
        }
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

        DrawTableTargetPicker(levelData);

        if (GUILayout.Button(
                $"Allocate / Resize ALL TABLE GRIDS To " +
                $"{levelData.GridWidth} x " +
                $"{levelData.GridHeight} x " +
                $"{levelData.GridDepth}"))
        {
            Undo.RecordObject(
                levelData,
                "Resize Grid Level"
            );

            int tableGridCount =
                Mathf.Max(1, levelData.TableCount);

            for (int tableIndex = 0;
                 tableIndex < tableGridCount;
                 tableIndex++)
            {
                levelData.EditorEnsureGridAllocated(
                    tableIndex,
                    true
                );
            }

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

        if (!levelData.IsGridAllocatedForTable(
                paintTableIndex))
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
            levelData.IsGridAllocatedForTable(
                paintTableIndex) &&
            GUILayout.Button(
                $"COPY LAYER {paintLayerZ + 1} TO ALL LAYERS"))
        {
            Undo.RecordObject(
                levelData,
                "Copy Grid Layer To All Layers"
            );

            levelData.EditorCopyDepthLayerToAll(
                paintLayerZ,
                paintTableIndex
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


    private void DrawTableTargetPicker(
        GridLevelData levelData)
    {
        int tableCount =
            Mathf.Max(1, levelData.TableCount);

        string[] tableLabels =
            new string[tableCount];

        for (int index = 0;
             index < tableLabels.Length;
             index++)
        {
            LevelTable table =
                levelData.GetTablePrefab(index);

            tableLabels[index] =
                $"Table {index + 1}: " +
                $"{(table != null ? table.name : "Missing Prefab")}" +
                (index == 0 ? " (PRIMARY)" : string.Empty);
        }

        paintTableIndex =
            Mathf.Clamp(
                paintTableIndex,
                0,
                tableLabels.Length - 1
            );

        int requestedTableIndex =
            EditorGUILayout.Popup(
                new GUIContent(
                    "Current Block Table",
                    "Ab jo blocks paint honge woh selected table ki top surface par spawn honge."
                ),
                paintTableIndex,
                tableLabels
            );

        if (requestedTableIndex != paintTableIndex)
        {
            paintTableIndex = requestedTableIndex;
            selectedCellCoordinate =
                new Vector3Int(-1, -1, -1);
            blockMoveFeedback = string.Empty;
        }

        if (!levelData.IsGridAllocatedForTable(
                paintTableIndex))
        {
            Undo.RecordObject(
                levelData,
                $"Create Table {paintTableIndex + 1} Grid"
            );

            levelData.EditorEnsureGridAllocated(
                paintTableIndex,
                false
            );

            EditorUtility.SetDirty(levelData);
        }

        Vector3 tablePosition =
            levelData.GetTablePositionOffset(
                paintTableIndex
            );

        Vector3 tableRotation =
            levelData.GetTableRotationEuler(
                paintTableIndex
            );

        string runtimeRotation =
            levelData.IsTableRuntimeRotationEnabled(
                paintTableIndex)
                ? $"ON ({levelData.GetTableRuntimeRotationSpeed(paintTableIndex):0.###} deg/s around " +
                  $"{levelData.GetTableRuntimeRotationAxis(paintTableIndex)})"
                : "OFF";

        string runtimeMovement =
            levelData.IsTableRuntimeHorizontalMovementEnabled(
                paintTableIndex)
                ? $"ON ({levelData.GetTableRuntimeMovementDistance(paintTableIndex):0.###} distance, " +
                  $"{levelData.GetTableRuntimeMovementSpeed(paintTableIndex):0.###} cycles/s along " +
                  $"{levelData.GetTableRuntimeMovementAxis(paintTableIndex)})"
                : "OFF";

        string physicsRelease =
            levelData.ShouldReleaseTableBlocksAfterMovementCycles(
                paintTableIndex)
                ? $"ON (random " +
                  $"{levelData.GetTableMinimumMovementCyclesBeforeRelease(paintTableIndex)}-" +
                  $"{levelData.GetTableMaximumMovementCyclesBeforeRelease(paintTableIndex)} cycles)"
                : "OFF";

        EditorGUILayout.HelpBox(
            $"TABLE {paintTableIndex + 1} ka independent LOCAL grid active hai. " +
            $"Table Position: {tablePosition}, Rotation: {tableRotation}. " +
            $"Runtime Rotation: {runtimeRotation}. " +
            $"Left/Right Movement: {runtimeMovement}. " +
            $"Physics Release: {physicsRelease}. " +
            "Grid center runtime mein isi table ki top surface follow karega.",
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

        EditorGUILayout.LabelField(
            "Grid Tool",
            EditorStyles.boldLabel
        );

        int activeTool =
            editPlacedBlockMode
                ? 1
                : eraseMode ? 2 : 0;

        activeTool = GUILayout.Toolbar(
            activeTool,
            new[]
            {
                "PAINT",
                "EDIT BLOCK",
                "ERASE"
            }
        );

        editPlacedBlockMode = activeTool == 1;
        eraseMode = activeTool == 2;
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

            levelData.EditorClearAllCells(
                paintTableIndex
            );

            NotifyLevelDesignChanged(levelData);
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
                paintDefinitionIndex,
                true,
                paintTableIndex
            );

            if (levelData.MirrorPaintAcrossLayers)
            {
                levelData.EditorCopyDepthLayerToAll(
                    paintLayerZ,
                    paintTableIndex
                );
            }

            NotifyLevelDesignChanged(levelData);
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
                paintDefinitionIndex,
                paintTableIndex
            );

            if (levelData.MirrorPaintAcrossLayers)
            {
                levelData.EditorCopyDepthLayerToAll(
                    paintLayerZ,
                    paintTableIndex
                );
            }

            NotifyLevelDesignChanged(levelData);
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
                paintDefinitionIndex,
                false,
                1f,
                paintTableIndex
            );

            NotifyLevelDesignChanged(levelData);
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
            editPlacedBlockMode
                ? $"TABLE {paintTableIndex + 1} Grid Edit (select a block)"
                : $"TABLE {paintTableIndex + 1} Grid Paint (independent grid)",
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

        DrawSelectedBlockEditor(levelData);

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
                    levelData.GetCell(
                        x,
                        y,
                        paintLayerZ,
                        paintTableIndex
                    );

                bool isOccupied =
                    cell != null && cell.Occupied;

                Vector3Int displayedAnchor =
                    isOccupied && cell.IsCovered
                        ? cell.AnchorCoordinate
                        : new Vector3Int(
                            x,
                            y,
                            paintLayerZ
                        );

                bool isSelected =
                    editPlacedBlockMode &&
                    isOccupied &&
                    selectedBlockTableIndex == paintTableIndex &&
                    displayedAnchor == selectedCellCoordinate;

                Color previousBackground =
                    GUI.backgroundColor;

                GUI.backgroundColor =
                    isSelected
                        ? new Color(1f, 0.82f, 0.18f, 1f)
                        : isOccupied
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

                GUIContent cellContent =
                    new GUIContent(
                        cellLabel,
                        isOccupied
                            ? $"Table {paintTableIndex + 1}"
                            : editPlacedBlockMode
                                ? "Selected block ko yahan move karein"
                                : $"Table {paintTableIndex + 1} par paint karein"
                    );

                if (GUILayout.Button(
                        cellContent,
                        GUILayout.Width(CellButtonSize),
                        GUILayout.Height(CellButtonSize)))
                {
                    if (editPlacedBlockMode)
                    {
                        Vector3Int clickedCoordinate =
                            new Vector3Int(
                                x,
                                y,
                                paintLayerZ
                            );

                        if (!isOccupied &&
                            TryGetSelectedBlock(
                                levelData,
                                out _,
                                out _))
                        {
                            TryMoveSelectedBlock(
                                levelData,
                                clickedCoordinate
                            );
                        }
                        else
                        {
                            selectedCellCoordinate =
                                isOccupied && cell.IsCovered
                                    ? cell.AnchorCoordinate
                                    : isOccupied
                                        ? clickedCoordinate
                                        : new Vector3Int(-1, -1, -1);

                            blockMoveFeedback = string.Empty;

                            if (isOccupied)
                            {
                                selectedBlockTableIndex =
                                    paintTableIndex;
                            }
                        }

                        Repaint();
                        GUIUtility.ExitGUI();
                    }

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
                                targetLayer,
                                paintTableIndex
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
                                paintCustomZRotation,
                                paintTableIndex
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
                                paintDefinitionIndex,
                                paintTableIndex
                            );
                        }
                    }

                    levelData.RecalculateGridMetadata();
                    NotifyLevelDesignChanged(levelData);
                }

                GUI.backgroundColor =
                    previousBackground;
            }

            EditorGUILayout.EndHorizontal();
        }
    }


    private void DrawSelectedBlockEditor(
        GridLevelData levelData)
    {
        if (!editPlacedBlockMode)
        {
            return;
        }

        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        if (!TryGetSelectedBlock(
                levelData,
                out Vector3Int anchorCoordinate,
                out GridCellData cell))
        {
            EditorGUILayout.HelpBox(
                "Edit karne ke liye grid mein kisi occupied block par click karein.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
            return;
        }

        PhysicsObjectDefinition definition =
            levelData.GetPaletteEntry(
                cell.DefinitionIndex
            );

        EditorGUILayout.LabelField(
            $"Selected Block ({anchorCoordinate.x}, " +
            $"{anchorCoordinate.y}, {anchorCoordinate.z})",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            definition != null
                ? definition.DisplayName
                : $"Palette Index {cell.DefinitionIndex}",
            EditorStyles.miniLabel
        );

        int tableCount =
            Mathf.Max(1, levelData.TableCount);

        string[] tableLabels =
            new string[tableCount];

        for (int index = 0;
             index < tableLabels.Length;
             index++)
        {
            LevelTable table =
                levelData.GetTablePrefab(index);

            tableLabels[index] =
                $"Table {index + 1}: " +
                (table != null ? table.name : "Missing Prefab");
        }

        int currentTableIndex =
            Mathf.Clamp(
                selectedBlockTableIndex,
                0,
                tableLabels.Length - 1
            );

        int requestedTableIndex =
            EditorGUILayout.Popup(
                "Block Table",
                currentTableIndex,
                tableLabels
            );

        if (requestedTableIndex != currentTableIndex)
        {
            Undo.RecordObject(
                levelData,
                "Change Placed Block Table"
            );

            if (levelData.EditorTryTransferBlockToTable(
                selectedBlockTableIndex,
                anchorCoordinate,
                requestedTableIndex,
                out string transferFailureReason))
            {
                selectedBlockTableIndex = requestedTableIndex;
                paintTableIndex = requestedTableIndex;
                blockMoveFeedback = string.Empty;
                NotifyLevelDesignChanged(levelData);
                GUIUtility.ExitGUI();
            }

            blockMoveFeedback = transferFailureReason;
        }

        EditorGUILayout.HelpBox(
            "Move without values: grid ke kisi EMPTY cell par click karein, " +
            "ya neeche one-cell move buttons use karein.",
            MessageType.None
        );

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(
                "UP +Y",
                GUILayout.Width(90f)) &&
            TryMoveSelectedBlock(
                levelData,
                anchorCoordinate + Vector3Int.up))
        {
            GUIUtility.ExitGUI();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("LEFT -X") &&
            TryMoveSelectedBlock(
                levelData,
                anchorCoordinate + Vector3Int.left))
        {
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("DOWN -Y") &&
            TryMoveSelectedBlock(
                levelData,
                anchorCoordinate + Vector3Int.down))
        {
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("RIGHT +X") &&
            TryMoveSelectedBlock(
                levelData,
                anchorCoordinate + Vector3Int.right))
        {
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.EndHorizontal();

        if (levelData.GridDepth > 1)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("DEPTH -Z") &&
                TryMoveSelectedBlock(
                    levelData,
                    anchorCoordinate +
                    new Vector3Int(0, 0, -1)))
            {
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("DEPTH +Z") &&
                TryMoveSelectedBlock(
                    levelData,
                    anchorCoordinate +
                    new Vector3Int(0, 0, 1)))
            {
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        if (!string.IsNullOrEmpty(blockMoveFeedback))
        {
            EditorGUILayout.HelpBox(
                blockMoveFeedback,
                MessageType.Warning
            );
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(
            "Rest Between Cells",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("HALF LEFT") &&
            SetSelectedBlockSeamOffset(
                levelData,
                cell,
                new Vector3(
                    -0.5f,
                    cell.LocalOffset.y,
                    cell.LocalOffset.z
                )))
        {
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("CENTER X") &&
            SetSelectedBlockSeamOffset(
                levelData,
                cell,
                new Vector3(
                    0f,
                    cell.LocalOffset.y,
                    cell.LocalOffset.z
                )))
        {
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("HALF RIGHT") &&
            SetSelectedBlockSeamOffset(
                levelData,
                cell,
                new Vector3(
                    0.5f,
                    cell.LocalOffset.y,
                    cell.LocalOffset.z
                )))
        {
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("HALF BACK") &&
            SetSelectedBlockSeamOffset(
                levelData,
                cell,
                new Vector3(
                    cell.LocalOffset.x,
                    cell.LocalOffset.y,
                    -0.5f
                )))
        {
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("CENTER Z") &&
            SetSelectedBlockSeamOffset(
                levelData,
                cell,
                new Vector3(
                    cell.LocalOffset.x,
                    cell.LocalOffset.y,
                    0f
                )))
        {
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("HALF FRONT") &&
            SetSelectedBlockSeamOffset(
                levelData,
                cell,
                new Vector3(
                    cell.LocalOffset.x,
                    cell.LocalOffset.y,
                    0.5f
                )))
        {
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "HALF button block ko do cells ke beech rakhta hai. X aur Z " +
            "HALF buttons combine karke block ko four-cell corner par bhi rakh sakte hain.",
            MessageType.None
        );

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(
            "Optional Fine Transform",
            EditorStyles.boldLabel
        );

        EditorGUI.BeginChangeCheck();

        Vector3 requestedPosition =
            EditorGUILayout.Vector3Field(
                new GUIContent(
                    "Position Offset (Cells)",
                    "Decimal values allowed hain. X/Y/Z value grid cell spacing ke mutabiq block ko freely move karti hai."
                ),
                cell.LocalOffset
            );

        PieceOrientation requestedOrientation =
            (PieceOrientation)EditorGUILayout.EnumPopup(
                "Base Orientation",
                cell.Orientation
            );

        float requestedCustomZ =
            cell.CustomZRotation;

        if (requestedOrientation == PieceOrientation.CustomZ)
        {
            requestedCustomZ =
                EditorGUILayout.FloatField(
                    "Base Custom Z Angle",
                    requestedCustomZ
                );
        }

        Vector3 requestedRotation =
            EditorGUILayout.Vector3Field(
                new GUIContent(
                    "Extra XYZ Rotation",
                    "Base Orientation ke upar degrees mein free XYZ rotation."
                ),
                cell.RotationEulerOffset
            );

        Vector3 requestedScale =
            EditorGUILayout.Vector3Field(
                new GUIContent(
                    "Size Multiplier",
                    "(1, 1, 1) auto-fit size hai. Har axis separately resize ho sakti hai."
                ),
                cell.ScaleMultiplier
            );

        requestedScale =
            new Vector3(
                Mathf.Max(0.01f, requestedScale.x),
                Mathf.Max(0.01f, requestedScale.y),
                Mathf.Max(0.01f, requestedScale.z)
            );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(
                levelData,
                "Edit Placed Grid Block Transform"
            );

            cell.SetLocalOffset(requestedPosition);
            cell.SetOrientation(requestedOrientation);
            cell.SetCustomZRotation(requestedCustomZ);
            cell.SetRotationEulerOffset(requestedRotation);
            cell.SetScaleMultiplier(requestedScale);

            NotifyLevelDesignChanged(levelData);
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(
            "Breakable",
            EditorStyles.boldLabel
        );

        EditorGUI.BeginChangeCheck();

        bool requestedBreakable =
            EditorGUILayout.Toggle(
                "Break On Cannon Hit",
                cell.Breakable
            );

        int requestedHitsToBreak =
            cell.HitsToBreak;

        if (requestedBreakable)
        {
            requestedHitsToBreak =
                EditorGUILayout.IntSlider(
                    "Hits To Break",
                    requestedHitsToBreak,
                    1,
                    10
                );
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(
                levelData,
                "Change Placed Block Breakable Settings"
            );

            cell.SetBreakable(
                requestedBreakable,
                requestedHitsToBreak
            );

            NotifyLevelDesignChanged(levelData);
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("RESET TRANSFORM"))
        {
            Undo.RecordObject(
                levelData,
                "Reset Placed Grid Block Transform"
            );

            cell.SetLocalOffset(Vector3.zero);
            cell.SetOrientation(PieceOrientation.UprightY);
            cell.SetCustomZRotation(0f);
            cell.SetRotationEulerOffset(Vector3.zero);
            cell.SetScaleMultiplier(Vector3.one);

            NotifyLevelDesignChanged(levelData);
        }

        if (GUILayout.Button("DELETE BLOCK"))
        {
            Undo.RecordObject(
                levelData,
                "Delete Selected Grid Block"
            );

            levelData.EditorClearSpanGroupAt(
                anchorCoordinate.x,
                anchorCoordinate.y,
                anchorCoordinate.z,
                selectedBlockTableIndex
            );

            levelData.RecalculateGridMetadata();
            selectedCellCoordinate =
                new Vector3Int(-1, -1, -1);

            NotifyLevelDesignChanged(levelData);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUIUtility.ExitGUI();
            return;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(6f);
    }


    private bool TryGetSelectedBlock(
        GridLevelData levelData,
        out Vector3Int anchorCoordinate,
        out GridCellData anchorCell)
    {
        anchorCoordinate = selectedCellCoordinate;
        anchorCell = levelData.GetCell(
            selectedCellCoordinate.x,
            selectedCellCoordinate.y,
            selectedCellCoordinate.z,
            selectedBlockTableIndex
        );

        if (anchorCell == null ||
            !anchorCell.Occupied)
        {
            anchorCell = null;
            return false;
        }

        if (anchorCell.IsCovered)
        {
            anchorCoordinate =
                anchorCell.AnchorCoordinate;

            anchorCell = levelData.GetCell(
                anchorCoordinate.x,
                anchorCoordinate.y,
                anchorCoordinate.z,
                selectedBlockTableIndex
            );
        }

        return anchorCell != null &&
               anchorCell.Occupied &&
               !anchorCell.IsCovered;
    }


    private bool TryMoveSelectedBlock(
        GridLevelData levelData,
        Vector3Int destinationCoordinate)
    {
        Undo.RecordObject(
            levelData,
            "Move Placed Grid Block"
        );

        if (!levelData.EditorTryMoveBlock(
                selectedCellCoordinate,
                destinationCoordinate,
                out string failureReason,
                selectedBlockTableIndex))
        {
            blockMoveFeedback = failureReason;
            Repaint();
            return false;
        }

        selectedCellCoordinate = destinationCoordinate;
        paintLayerZ = destinationCoordinate.z;
        blockMoveFeedback = string.Empty;

        NotifyLevelDesignChanged(levelData);
        Repaint();
        return true;
    }


    private bool SetSelectedBlockSeamOffset(
        GridLevelData levelData,
        GridCellData cell,
        Vector3 offset)
    {
        if (cell == null ||
            cell.LocalOffset == offset)
        {
            return false;
        }

        Undo.RecordObject(
            levelData,
            "Place Grid Block Between Cells"
        );

        cell.SetLocalOffset(offset);
        NotifyLevelDesignChanged(levelData);
        Repaint();
        return true;
    }




}

#endif
