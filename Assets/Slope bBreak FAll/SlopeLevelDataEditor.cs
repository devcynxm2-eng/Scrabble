#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SlopeLevelData ka custom inspector — slope ball ka level designer.
///
/// Workflow Canon wale GridLevelDataEditor jaisa hi hai: ek working
/// canvas asset par design karo, phir SAVE se wo alag numbered level
/// asset mein chala jata hai aur database mein register ho jata hai.
/// Live Preview bhi wahi tareeqa use karta hai.
///
/// Farq sirf mechanics ka hai. Canon 3D grid mein blocks paint karta
/// hai; yahan glass tower ki shape paint hoti hai (columns x rows x
/// layers), jise GlassTowerController cell mask ke taur par parhta hai.
///
/// Ye Canon ke editor se koi cheez share nahi karta — na base class,
/// na helper — is liye Canon bilkul affect nahi hota.
/// </summary>
[CustomEditor(typeof(SlopeLevelData))]
public sealed class SlopeLevelDataEditor : Editor
{
    private const string LevelsFolder =
        "Assets/Slope bBreak FAll/Levels";

    private const string DatabasePath =
        "Assets/Slope bBreak FAll/SlopeLevelDatabase.asset";

    private const string LivePreviewSessionKey =
        SlopeLivePreviewLauncher.EnabledKey;

    private const float CellSize = 26f;


    private static bool livePreviewRefreshQueued;


    private static readonly Color EmptyColor =
        new Color(1f, 1f, 1f, 0.12f);

    private static readonly Color UnsupportedColor =
        new Color(1f, 0.55f, 0.2f, 1f);


    private SerializedProperty levelNumber;
    private SerializedProperty gridColumns;
    private SerializedProperty gridRows;
    private SerializedProperty gridLayers;
    private SerializedProperty layerSpacing;
    private SerializedProperty objectPalette;

    private SerializedProperty glassScale;
    private SerializedProperty spacing;
    private SerializedProperty localOrigin;
    private SerializedProperty autoRowSpacing;
    private SerializedProperty restOnGround;

    private SerializedProperty collapseStepDelay;
    private SerializedProperty usePhysicalSupportCheck;
    private SerializedProperty physicalSupportDistance;
    private SerializedProperty physicalSupportProbeRadius;

    private SerializedProperty ballLifetime;
    private SerializedProperty minXPosition;
    private SerializedProperty maxXPosition;
    private SerializedProperty minimumFallSpeed;
    private SerializedProperty maximumFallSpeed;
    private SerializedProperty extraDownwardAcceleration;

    private SerializedProperty powerChargeSpeed;
    private SerializedProperty minimumPower;
    private SerializedProperty maximumPower;


    /*
     * Har palette object ka asli rang, uske material se parha hua.
     * Material lookup har repaint par karna mehnga hai, is liye cache.
     */
    private Color[] paletteColorCache;
    private bool paletteColorsReady;

    private int levelNumberToEdit = 1;
    private int paintLayer;
    private int brushIndex;
    private bool eraseMode;
    private bool showCollapse;
    private bool showBall;


    private void OnEnable()
    {
        levelNumber = serializedObject.FindProperty("levelNumber");
        gridColumns = serializedObject.FindProperty("gridColumns");
        gridRows = serializedObject.FindProperty("gridRows");
        gridLayers = serializedObject.FindProperty("gridLayers");
        layerSpacing = serializedObject.FindProperty("layerSpacing");
        objectPalette = serializedObject.FindProperty("objectPalette");

        glassScale = serializedObject.FindProperty("glassScale");
        spacing = serializedObject.FindProperty("spacing");
        localOrigin = serializedObject.FindProperty("localOrigin");
        autoRowSpacing = serializedObject.FindProperty("autoRowSpacing");
        restOnGround = serializedObject.FindProperty("restOnGround");

        collapseStepDelay =
            serializedObject.FindProperty("collapseStepDelay");
        usePhysicalSupportCheck =
            serializedObject.FindProperty("usePhysicalSupportCheck");
        physicalSupportDistance =
            serializedObject.FindProperty("physicalSupportDistance");
        physicalSupportProbeRadius =
            serializedObject.FindProperty("physicalSupportProbeRadius");

        ballLifetime = serializedObject.FindProperty("ballLifetime");
        minXPosition = serializedObject.FindProperty("minXPosition");
        maxXPosition = serializedObject.FindProperty("maxXPosition");
        minimumFallSpeed =
            serializedObject.FindProperty("minimumFallSpeed");
        maximumFallSpeed =
            serializedObject.FindProperty("maximumFallSpeed");
        extraDownwardAcceleration =
            serializedObject.FindProperty("extraDownwardAcceleration");

        powerChargeSpeed =
            serializedObject.FindProperty("powerChargeSpeed");
        minimumPower = serializedObject.FindProperty("minimumPower");
        maximumPower = serializedObject.FindProperty("maximumPower");

        levelNumberToEdit =
            Mathf.Max(1, levelNumber.intValue);
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SlopeLevelData levelData =
            (SlopeLevelData)target;

        levelData.EnsureCellCapacity();

        EnsurePaletteColorCache(levelData);


        DrawLiveGamePreviewSection(levelData);

        DrawLevelCreatorSection(levelData);

        DrawGridSetup(levelData);

        DrawObjectPalette(levelData);

        DrawPresetTools(levelData);

        DrawGridPainter(levelData);

        DrawSaveLevelSection(levelData);

        EditorGUILayout.Space(8f);

        DrawGlassSettings();

        DrawCollapseSettings();

        DrawBallSettings();


        serializedObject.ApplyModifiedProperties();
    }


    // ==========================
    // LIVE PREVIEW
    // ==========================

    private void DrawLiveGamePreviewSection(
        SlopeLevelData levelData)
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
                ? "LIVE PREVIEW ACTIVE: yahan design change karein, " +
                  "Game View mein tower foran dobara ban jayega."
                : "Play Mode mein current design ko asli game mein " +
                  "dekhne ke liye preview start karein.",
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
                 * Launch flag aur level dono SessionState mein jate
                 * hain kyunke Play Mode ka domain reload editor ka
                 * baaki sara state udaa deta hai. Inhi ki madad se
                 * SlopeLivePreviewLauncher Play Mode shuru hote hi
                 * pehla tower bana deta hai.
                 */
                SessionState.SetBool(
                    SlopeLivePreviewLauncher.LaunchKey,
                    true
                );

                SlopeLivePreviewLauncher.RememberLevel(levelData);

                /*
                 * Working canvas disk par sync hota hai taake Play Mode
                 * domain reload ke baad exact current design parhe.
                 */
                EditorUtility.SetDirty(levelData);
                AssetDatabase.SaveAssetIfDirty(levelData);

                EditorApplication.EnterPlaymode();
                GUIUtility.ExitGUI();
            }
        }
        else if (!previewEnabled)
        {
            if (GUILayout.Button(
                    "ENABLE CURRENT DESIGN PREVIEW",
                    GUILayout.Height(38f)))
            {
                SessionState.SetBool(
                    LivePreviewSessionKey,
                    true
                );

                SlopeLivePreviewLauncher.RememberLevel(levelData);

                QueueLivePreviewRefresh(levelData);
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("REFRESH GAME VIEW"))
            {
                QueueLivePreviewRefresh(levelData);
            }

            if (GUILayout.Button("STOP LIVE PREVIEW"))
            {
                SessionState.SetBool(
                    LivePreviewSessionKey,
                    false
                );
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(8f);
    }


    private static void QueueLivePreviewRefresh(
        SlopeLevelData levelData)
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


            SlopeLivePreviewLauncher.RefreshNow(levelData);
        };
    }


    /// <summary>
    /// Painter aur presets SlopeLevelData ko seedha badalte hain,
    /// SerializedProperty ke zariye nahi.
    ///
    /// Us soorat mein serializedObject ka snapshot purana reh jata
    /// hai, aur frame ke aakhir mein ApplyModifiedProperties() usi
    /// purane snapshot ko object par wapis likh sakta hai — yani
    /// abhi ki gayi painting gayab. Update() se snapshot ko nayi
    /// halat par le aate hain, phir Repaint() se grid foran naya
    /// nazar aata hai.
    /// </summary>
    private void MarkDesignChanged(
        SlopeLevelData levelData)
    {
        NotifyLevelDesignChanged(levelData);

        serializedObject.Update();

        Repaint();
    }


    /// <summary>
    /// Design change hone par asset dirty karta hai, aur live preview
    /// on ho to Game View bhi refresh kar deta hai.
    /// </summary>
    private static void NotifyLevelDesignChanged(
        SlopeLevelData levelData)
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
            QueueLivePreviewRefresh(levelData);
        }
    }


    // ==========================
    // LEVEL CREATOR
    // ==========================

    private void DrawLevelCreatorSection(
        SlopeLevelData levelData)
    {
        SlopeLevelDatabase database =
            GetOrCreateLevelDatabase();

        EditorGUILayout.LabelField(
            "Level Creator",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            $"Saved Levels: {database.Count}\n" +
            "Yeh SlopeLevelData ek working canvas hai. SAVE LEVEL " +
            "isay alag numbered asset mein save karke database " +
            "mein register karta hai.",
            MessageType.Info
        );

        /*
         * User ke liye saaf rehna chahiye ke wo canvas par hai ya
         * kisi saved level par, warna LOAD ka button dabana bemaani
         * lagta hai.
         */
        bool isSavedLevelAsset =
            database.FindIndexByLevelNumber(
                levelData.LevelNumber
            ) >= 0 &&
            database.FindByLevelNumber(
                levelData.LevelNumber
            ) == levelData;

        EditorGUILayout.LabelField(
            "Editing",
            isSavedLevelAsset
                ? $"{levelData.name}  (saved level — changes seedhe " +
                  "isi level par lag rahe hain)"
                : $"{levelData.name}  (working canvas)"
        );

        EditorGUILayout.PropertyField(levelNumber);

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

        EditorGUILayout.Space(6f);
    }


    // ==========================
    // SAVE
    // ==========================

    private void DrawSaveLevelSection(
        SlopeLevelData levelData)
    {
        SlopeLevelDatabase database =
            GetOrCreateLevelDatabase();

        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField(
            "Save",
            EditorStyles.boldLabel
        );

        if (levelData.GetTotalGlassCount() == 0)
        {
            EditorGUILayout.HelpBox(
                "Grid khali hai — save karne par level mein koi " +
                "object nahi hoga.",
                MessageType.Warning
            );
        }

        if (GUILayout.Button(
                "SAVE LEVEL",
                GUILayout.Height(38f)))
        {
            serializedObject.ApplyModifiedProperties();

            SaveWorkingLevel(levelData, database);

            serializedObject.Update();
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button(
                "SAVE CURRENT + START NEXT LEVEL DESIGN",
                GUILayout.Height(28f)))
        {
            serializedObject.ApplyModifiedProperties();

            SaveWorkingLevel(levelData, database);
            StartNextLevelDesign(levelData, database);

            serializedObject.Update();
            GUIUtility.ExitGUI();
        }
    }


    private static void SaveWorkingLevel(
        SlopeLevelData workingLevel,
        SlopeLevelDatabase database)
    {
        if (workingLevel == null ||
            database == null)
        {
            return;
        }


        int number = Mathf.Max(1, workingLevel.LevelNumber);

        string assetPath =
            $"{LevelsFolder}/SlopeLevel_{number:000}.asset";

        EnsureLevelsFolderExists();


        SlopeLevelData savedLevel =
            AssetDatabase.LoadAssetAtPath<SlopeLevelData>(assetPath);

        bool isNewAsset = savedLevel == null;


        if (isNewAsset)
        {
            savedLevel =
                ScriptableObject.CreateInstance<SlopeLevelData>();

            AssetDatabase.CreateAsset(savedLevel, assetPath);
        }


        /*
         * Working canvas khud hi ek saved asset ho sakta hai. Us
         * soorat mein copy karne ki zaroorat nahi, warna
         * CopySerialized apne hi upar chalega.
         */
        if (savedLevel != workingLevel)
        {
            EditorUtility.CopySerialized(workingLevel, savedLevel);

            // CopySerialized asset ka naam bhi le aata hai
            savedLevel.name =
                Path.GetFileNameWithoutExtension(assetPath);
        }


        EditorUtility.SetDirty(savedLevel);

        database.EditorRegister(savedLevel);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


        Debug.Log(
            isNewAsset
                ? $"Slope level {number} naye asset mein save ho gaya: {assetPath}"
                : $"Slope level {number} update ho gaya: {assetPath}",
            savedLevel
        );
    }


    private static void LoadSavedLevelForEdit(
        SlopeLevelData workingLevel,
        SlopeLevelDatabase database,
        int number)
    {
        SlopeLevelData saved =
            database.FindByLevelNumber(number);

        if (saved == null)
        {
            Debug.LogWarning(
                $"Slope level {number} database mein nahi mila.",
                workingLevel
            );

            return;
        }


        /*
         * Agar user ne saved level asset ko hi khola hua hai to load
         * karne ko kuch nahi — wo pehle se wahi level edit kar raha
         * hai. Pehle yahan khamoshi se return ho jata tha, jis se
         * lagta tha ke button kaam hi nahi kar raha.
         */
        if (saved == workingLevel)
        {
            Debug.Log(
                $"Ye asset khud level {number} hai — aap pehle se " +
                "isay hi edit kar rahe hain. Doosra level laane ke " +
                "liye SlopeLevelCanvas kholein.",
                workingLevel
            );

            return;
        }


        string workingName = workingLevel.name;

        EditorUtility.CopySerialized(saved, workingLevel);

        workingLevel.name = workingName;

        EditorUtility.SetDirty(workingLevel);
        AssetDatabase.SaveAssets();

        NotifyLevelDesignChanged(workingLevel);


        Debug.Log(
            $"Slope level {number} edit ke liye load ho gaya.",
            workingLevel
        );
    }


    private static void StartNextLevelDesign(
        SlopeLevelData workingLevel,
        SlopeLevelDatabase database)
    {
        int next = database.GetHighestLevelNumber() + 1;

        SerializedObject so =
            new SerializedObject(workingLevel);

        so.FindProperty("levelNumber").intValue = next;
        so.ApplyModifiedPropertiesWithoutUndo();

        workingLevel.ClearAll();

        EditorUtility.SetDirty(workingLevel);
        AssetDatabase.SaveAssets();


        Debug.Log(
            $"Ab level {next} design karne ke liye canvas khali hai.",
            workingLevel
        );
    }


    private static void EnsureLevelsFolderExists()
    {
        if (AssetDatabase.IsValidFolder(LevelsFolder))
        {
            return;
        }


        Directory.CreateDirectory(LevelsFolder);
        AssetDatabase.Refresh();
    }


    private static SlopeLevelDatabase GetOrCreateLevelDatabase()
    {
        SlopeLevelDatabase database =
            AssetDatabase.LoadAssetAtPath<SlopeLevelDatabase>(
                DatabasePath
            );

        if (database != null)
        {
            // Hath se delete kiye gaye levels ki khali entries saaf karo
            database.EditorPruneMissing();

            return database;
        }


        database =
            ScriptableObject.CreateInstance<SlopeLevelDatabase>();

        AssetDatabase.CreateAsset(database, DatabasePath);
        AssetDatabase.SaveAssets();

        return database;
    }


    // ==========================
    // GRID SETUP
    // ==========================

    private void DrawGridSetup(
        SlopeLevelData levelData)
    {
        EditorGUILayout.LabelField(
            "Grid",
            EditorStyles.boldLabel
        );

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(gridColumns);
        EditorGUILayout.PropertyField(gridRows);
        EditorGUILayout.PropertyField(
            gridLayers,
            new GUIContent("Layers (depth)")
        );

        using (new EditorGUI.DisabledScope(gridLayers.intValue < 2))
        {
            EditorGUILayout.PropertyField(layerSpacing);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            levelData.EnsureCellCapacity();
            MarkDesignChanged(levelData);
        }
    }


    // ==========================
    // OBJECT PALETTE
    // ==========================

    private void DrawObjectPalette(
        SlopeLevelData levelData)
    {
        EditorGUILayout.LabelField(
            "Objects",
            EditorStyles.boldLabel
        );

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(
            objectPalette,
            new GUIContent("Object Palette"),
            true
        );

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            // Naye objects ke rang dobara parhne parenge
            paletteColorCache = null;
            paletteColorsReady = false;

            NotifyLevelDesignChanged(levelData);
            serializedObject.Update();
        }


        if (objectPalette.arraySize == 0)
        {
            EditorGUILayout.HelpBox(
                "Palette khali hai. Aise mein tower controller ka " +
                "default Glass Prefab lagta hai. Alag alag objects " +
                "place karne ke liye yahan unhein add karein.",
                MessageType.Info
            );
        }


        DrawBrushPicker();
    }


    /// <summary>
    /// Konsa object paint hoga wo yahan select hota hai. Erase
    /// select karne par click cell khali kar deta hai.
    /// </summary>
    private void DrawBrushPicker()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            "Brush",
            GUILayout.Width(EditorGUIUtility.labelWidth - 2f)
        );


        Color previousBackground = GUI.backgroundColor;

        GUI.backgroundColor =
            eraseMode ? Color.white : EmptyColor;

        if (GUILayout.Button(
                "Erase",
                GUILayout.Height(22f)))
        {
            eraseMode = true;
        }


        int paletteCount =
            Mathf.Max(1, objectPalette.arraySize);

        for (int i = 0; i < paletteCount; i++)
        {
            GUI.backgroundColor =
                !eraseMode && brushIndex == i
                    ? GetPaletteColor(i)
                    : GetPaletteColor(i) * 0.6f;

            string label =
                objectPalette.arraySize == 0
                    ? "Default"
                    : GetPaletteName(i);

            if (GUILayout.Button(
                    $"{i}  {label}",
                    GUILayout.Height(22f)))
            {
                eraseMode = false;
                brushIndex = i;
            }
        }

        GUI.backgroundColor = previousBackground;

        EditorGUILayout.EndHorizontal();
    }


    private string GetPaletteName(
        int index)
    {
        if (index < 0 ||
            index >= objectPalette.arraySize)
        {
            return "missing";
        }


        Object entry =
            objectPalette
                .GetArrayElementAtIndex(index)
                .objectReferenceValue;

        return entry == null ? "missing" : entry.name;
    }


    /// <summary>
    /// Grid ka cell wahi rang dikhata hai jo asli jar ka hai, taake
    /// design karte waqt tower ka look andaza ho jaye. Material na
    /// mile to ek fallback rang.
    /// </summary>
    private Color GetPaletteColor(
        int index)
    {
        if (index < 0)
        {
            return EmptyColor;
        }


        if (paletteColorCache != null &&
            index < paletteColorCache.Length)
        {
            return paletteColorCache[index];
        }


        return FallbackColor(index);
    }


    private static Color FallbackColor(
        int index)
    {
        return SlopeGlassColorUtility.FallbackColor(index);
    }



    /// <summary>
    /// Palette ke rang cache karta hai.
    ///
    /// Asset previews Unity background mein banata hai, is liye pehli
    /// dafa wo tayyar nahi hote. Jab tak sab tayyar na hon, cache ko
    /// adhoora maan kar agle repaint par dobara banate hain.
    /// </summary>
    private void EnsurePaletteColorCache(
        SlopeLevelData levelData)
    {
        int count = Mathf.Max(1, levelData.ObjectPalette.Count);

        if (paletteColorCache != null &&
            paletteColorCache.Length == count &&
            paletteColorsReady)
        {
            return;
        }


        paletteColorCache = new Color[count];
        paletteColorsReady = true;

        for (int i = 0; i < count; i++)
        {
            paletteColorCache[i] = ReadObjectColor(
                i < levelData.ObjectPalette.Count
                    ? levelData.ObjectPalette[i]
                    : null,
                i,
                out bool ready
            );

            if (!ready)
            {
                paletteColorsReady = false;
            }
        }


        if (!paletteColorsReady)
        {
            Repaint();
        }
    }


    private static Color ReadObjectColor(
        BreakableGlass prefab,
        int fallbackIndex,
        out bool ready)
    {
        return SlopeGlassColorUtility.GetObjectColor(
            prefab,
            fallbackIndex,
            out ready
        );
    }


    private static Color GhostColor(
        Color source)
    {
        return new Color(
            source.r,
            source.g,
            source.b,
            0.22f
        );
    }



    // ==========================
    // PRESETS
    // ==========================

    private void DrawPresetTools(
        SlopeLevelData levelData)
    {
        int brush = eraseMode ? 0 : brushIndex;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Layer"))
        {
            Undo.RecordObject(levelData, "Clear Slope Layer");
            levelData.ClearLayer(paintLayer);
            MarkDesignChanged(levelData);
        }

        if (GUILayout.Button("Fill Layer"))
        {
            Undo.RecordObject(levelData, "Fill Slope Layer");
            levelData.FillLayer(paintLayer, brush);
            MarkDesignChanged(levelData);
        }

        if (GUILayout.Button("Pyramid"))
        {
            Undo.RecordObject(levelData, "Pyramid Slope Layer");
            levelData.ApplyPyramidPreset(paintLayer, brush);
            MarkDesignChanged(levelData);
        }

        if (GUILayout.Button("Clear All"))
        {
            Undo.RecordObject(levelData, "Clear Slope Level");
            levelData.ClearAll();
            MarkDesignChanged(levelData);
        }

        EditorGUILayout.EndHorizontal();


        if (levelData.GridLayers > 1)
        {
            EditorGUILayout.BeginHorizontal();

            for (int other = 0; other < levelData.GridLayers; other++)
            {
                if (other == paintLayer)
                {
                    continue;
                }


                if (GUILayout.Button(
                        $"COPY LAYER {paintLayer + 1} -> {other + 1}"))
                {
                    Undo.RecordObject(levelData, "Copy Slope Layer");
                    levelData.CopyLayer(paintLayer, other);
                    MarkDesignChanged(levelData);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }


    // ==========================
    // GRID PAINTER
    // ==========================

    private void DrawGridPainter(
        SlopeLevelData levelData)
    {
        EditorGUILayout.Space(4f);

        if (levelData.GridLayers > 1)
        {
            string[] layerLabels =
                new string[levelData.GridLayers];

            for (int i = 0; i < layerLabels.Length; i++)
            {
                /*
                 * Har layer ka count sath likha hai taake pata chale
                 * kaunsi layer khali hai bina us par switch kiye.
                 */
                int layerTotal = 0;

                for (int row = 0; row < levelData.GridRows; row++)
                {
                    layerTotal += levelData.GetRowCount(i, row);
                }

                layerLabels[i] = $"Layer {i + 1}  ({layerTotal})";
            }

            EditorGUILayout.LabelField("Depth Layer");

            paintLayer = GUILayout.Toolbar(
                Mathf.Clamp(
                    paintLayer,
                    0,
                    levelData.GridLayers - 1
                ),
                layerLabels
            );
        }
        else
        {
            paintLayer = 0;
        }


        EditorGUILayout.LabelField(
            $"Layer {paintLayer + 1} Paint  —  row 0 sab se neeche",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            levelData.GridLayers > 1
                ? "Cell ka number palette index hai. Narangi = neeche " +
                  "kuch nahi, wo girega. Halka · = doosri layer " +
                  "par yahan object hai."
                : "Cell ka number palette index hai. Narangi = neeche " +
                  "kuch nahi, wo girega.",
            EditorStyles.miniLabel
        );


        /*
         * Top row pehle draw hoti hai taake grid bilkul waise
         * padhi jaye jaise tower runtime par khara hota hai.
         */
        for (int row = levelData.GridRows - 1; row >= 0; row--)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                row.ToString(),
                EditorStyles.miniLabel,
                GUILayout.Width(18f)
            );

            for (int column = 0;
                 column < levelData.GridColumns;
                 column++)
            {
                int definitionIndex =
                    levelData.GetCell(paintLayer, row, column);

                bool occupied =
                    definitionIndex != SlopeLevelData.EmptyCell;

                bool floating =
                    occupied &&
                    row > 0 &&
                    !levelData.IsOccupied(paintLayer, row - 1, column);

                /*
                 * Doosri layers par is cell par kuch hai ya nahi.
                 * Khali cell ko halka sa dikha dete hain taake layers
                 * ek doosre ke sath align ki ja sakein.
                 */
                int ghostIndex = SlopeLevelData.EmptyCell;

                if (!occupied &&
                    levelData.GridLayers > 1)
                {
                    for (int other = 0;
                         other < levelData.GridLayers;
                         other++)
                    {
                        if (other == paintLayer)
                        {
                            continue;
                        }


                        int otherIndex =
                            levelData.GetCell(other, row, column);

                        if (otherIndex != SlopeLevelData.EmptyCell)
                        {
                            ghostIndex = otherIndex;
                            break;
                        }
                    }
                }

                Color previousBackground =
                    GUI.backgroundColor;

                GUI.backgroundColor =
                    floating
                        ? UnsupportedColor
                        : occupied
                            ? GetPaletteColor(definitionIndex)
                            : ghostIndex != SlopeLevelData.EmptyCell
                                ? GhostColor(GetPaletteColor(ghostIndex))
                                : EmptyColor;

                GUIContent cellContent =
                    new GUIContent(
                        occupied
                            ? definitionIndex.ToString()
                            : ghostIndex != SlopeLevelData.EmptyCell
                                ? "·"
                                : "",
                        floating
                            ? $"L{paintLayer + 1} R{row} C{column} — neeche support nahi"
                            : ghostIndex != SlopeLevelData.EmptyCell
                                ? $"L{paintLayer + 1} R{row} C{column} — khali, magar doosri layer par object hai"
                                : $"L{paintLayer + 1} R{row} C{column}"
                    );

                if (GUILayout.Button(
                        cellContent,
                        GUILayout.Width(CellSize),
                        GUILayout.Height(CellSize)))
                {
                    Undo.RecordObject(
                        levelData,
                        "Paint Slope Level"
                    );

                    levelData.SetCell(
                        paintLayer,
                        row,
                        column,
                        eraseMode
                            ? SlopeLevelData.EmptyCell
                            : brushIndex
                    );

                    MarkDesignChanged(levelData);
                }

                GUI.backgroundColor = previousBackground;
            }

            GUILayout.Label(
                levelData.GetRowCount(paintLayer, row).ToString(),
                EditorStyles.miniLabel,
                GUILayout.Width(20f)
            );

            EditorGUILayout.EndHorizontal();
        }


        EditorGUILayout.LabelField(
            "Total objects (saari layers)",
            levelData.GetTotalGlassCount().ToString()
        );
    }


    // ==========================
    // SETTINGS
    // ==========================

    private void DrawGlassSettings()
    {
        EditorGUILayout.LabelField(
            "Glass",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(glassScale);
        EditorGUILayout.PropertyField(localOrigin);

        EditorGUILayout.PropertyField(restOnGround);
        EditorGUILayout.PropertyField(autoRowSpacing);

        /*
         * Spacing.y sirf tab lagti hai jab auto stacking band ho.
         * Auto on ho to rows object ki asli height se stack hoti hain.
         */
        using (new EditorGUI.DisabledScope(autoRowSpacing.boolValue))
        {
            EditorGUILayout.PropertyField(spacing);
        }

        if (autoRowSpacing.boolValue)
        {
            EditorGUILayout.HelpBox(
                "Rows objects ki asli height par stack ho rahi hain, " +
                "is liye Spacing Y ka Y hissa ignore hai. Sirf Spacing X " +
                "(columns ka faasla) chal raha hai.",
                MessageType.None
            );
        }
    }


    private void DrawCollapseSettings()
    {
        showCollapse = EditorGUILayout.Foldout(
            showCollapse,
            "Collapse",
            true
        );

        if (!showCollapse)
        {
            return;
        }


        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(collapseStepDelay);
        EditorGUILayout.PropertyField(usePhysicalSupportCheck);

        using (new EditorGUI.DisabledScope(
                   !usePhysicalSupportCheck.boolValue))
        {
            EditorGUILayout.PropertyField(physicalSupportDistance);
            EditorGUILayout.PropertyField(physicalSupportProbeRadius);
        }

        EditorGUI.indentLevel--;
    }


    private void DrawBallSettings()
    {
        showBall = EditorGUILayout.Foldout(
            showBall,
            "Ball",
            true
        );

        if (!showBall)
        {
            return;
        }


        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(ballLifetime);
        EditorGUILayout.PropertyField(minXPosition);
        EditorGUILayout.PropertyField(maxXPosition);
        EditorGUILayout.PropertyField(minimumFallSpeed);
        EditorGUILayout.PropertyField(maximumFallSpeed);
        EditorGUILayout.PropertyField(extraDownwardAcceleration);

        EditorGUILayout.Space(4f);

        EditorGUILayout.LabelField(
            "Power Meter",
            EditorStyles.miniBoldLabel
        );

        EditorGUILayout.PropertyField(powerChargeSpeed);
        EditorGUILayout.PropertyField(minimumPower);
        EditorGUILayout.PropertyField(maximumPower);

        EditorGUI.indentLevel--;


        if (maximumFallSpeed.floatValue < minimumFallSpeed.floatValue)
        {
            EditorGUILayout.HelpBox(
                "Maximum Fall Speed, Minimum Fall Speed se kam hai.",
                MessageType.Warning
            );
        }

        if (maxXPosition.floatValue <= minXPosition.floatValue)
        {
            EditorGUILayout.HelpBox(
                "Max X Position, Min X Position se bara hona chahiye " +
                "warna ball hil nahi payegi.",
                MessageType.Warning
            );
        }
    }
}
#endif
