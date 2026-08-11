#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class GridLevelAddressablesEditorUtility
{
    public const string DatabasePath =
        "Assets/Canon Controle/GridLevelDatabase.asset";

    public const string LevelsFolder =
        "Assets/Canon Controle/Levels";

    public const string AddressablesGroupName =
        "Grid Levels";

    public const string AddressablesLabel =
        "level";

    private static bool migrationQueued;


    [InitializeOnLoadMethod]
    private static void QueueAutomaticMigration()
    {
        if (migrationQueued)
        {
            return;
        }

        migrationQueued = true;
        EditorApplication.delayCall += RunAutomaticMigration;
    }


    [MenuItem("Tools/Royal Smash/Migrate Levels to Addressables")]
    private static void MigrateLevelsFromMenu()
    {
        GridLevelDatabase database =
            AssetDatabase.LoadAssetAtPath<GridLevelDatabase>(
                DatabasePath
            );

        if (database == null)
        {
            Debug.LogError(
                $"GridLevelDatabase missing: {DatabasePath}"
            );
            return;
        }

        EnsurePerLevelAddressableAssets(database);
        Selection.activeObject = database;
    }


    private static void RunAutomaticMigration()
    {
        migrationQueued = false;

        if (EditorApplication.isCompiling ||
            EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            QueueAutomaticMigration();
            return;
        }

        GridLevelDatabase database =
            AssetDatabase.LoadAssetAtPath<GridLevelDatabase>(
                DatabasePath
            );

        if (database != null)
        {
            EnsurePerLevelAddressableAssets(database);
        }
    }


    public static GridLevelData GetLevelAsset(
        GridLevelDatabase database,
        int levelNumber)
    {
        if (database == null)
        {
            return null;
        }

        int index =
            database.FindIndexByLevelNumber(levelNumber);

        if (index < 0)
        {
            return null;
        }

        string assetPath =
            GetAssetPath(levelNumber);

        GridLevelData level =
            AssetDatabase.LoadAssetAtPath<GridLevelData>(
                assetPath
            );

        if (level != null)
        {
            return level;
        }

        string guid =
            AssetDatabase.AssetPathToGUID(assetPath);

        return string.IsNullOrEmpty(guid)
            ? null
            : AssetDatabase.LoadAssetAtPath<GridLevelData>(
                AssetDatabase.GUIDToAssetPath(guid)
            );
    }


    public static GridLevelData SaveWorkingLevel(
        GridLevelData workingLevel,
        GridLevelDatabase database,
        out bool created)
    {
        created = false;

        if (workingLevel == null ||
            database == null)
        {
            return null;
        }

        EnsurePerLevelAddressableAssets(database);
        EnsureLevelsFolderExists();

        int levelNumber =
            Mathf.Max(1, workingLevel.LevelNumber);

        string assetPath =
            GetAssetPath(levelNumber);

        GridLevelData savedLevel =
            AssetDatabase.LoadAssetAtPath<GridLevelData>(
                assetPath
            );

        if (savedLevel == null)
        {
            savedLevel = ScriptableObject.CreateInstance<GridLevelData>();
            AssetDatabase.CreateAsset(savedLevel, assetPath);
            created = true;
        }

        EditorUtility.CopySerialized(
            workingLevel,
            savedLevel
        );

        savedLevel.name =
            $"Level_{levelNumber:000}";

        savedLevel.hideFlags = HideFlags.None;
        savedLevel.EditorSetLevelNumber(levelNumber);

        string address =
            GetAddress(levelNumber);

        MakeAddressable(assetPath, address);
        database.EditorRegisterAddress(levelNumber, address);

        EditorUtility.SetDirty(savedLevel);
        EditorUtility.SetDirty(database);
        EditorUtility.SetDirty(workingLevel);
        AssetDatabase.SaveAssets();

        return savedLevel;
    }


    public static void EnsurePerLevelAddressableAssets(
        GridLevelDatabase database)
    {
        if (database == null)
        {
            return;
        }

        EnsureLevelsFolderExists();

        List<GridLevelData> legacyLevels =
            new List<GridLevelData>();

        IReadOnlyList<GridLevelData> serializedLegacyLevels =
            database.EditorLegacyLevels;

        if (serializedLegacyLevels != null)
        {
            for (int i = 0;
                 i < serializedLegacyLevels.Count;
                 i++)
            {
                GridLevelData legacyLevel =
                    serializedLegacyLevels[i];

                if (legacyLevel != null)
                {
                    legacyLevels.Add(legacyLevel);
                }
            }
        }

        /*
         * Purane database mein kuch orphan subassets list se detach ho
         * sakte hain. Unko bhi discover karna zaroori hai warna valid
         * designed level migration mein miss ho sakta hai.
         */
        Object[] databaseAssets =
            AssetDatabase.LoadAllAssetsAtPath(DatabasePath);

        foreach (Object databaseAsset in databaseAssets)
        {
            if (databaseAsset is GridLevelData embeddedLevel &&
                !legacyLevels.Contains(embeddedLevel))
            {
                legacyLevels.Add(embeddedLevel);
            }
        }

        bool changed = false;

        foreach (GridLevelData legacyLevel in legacyLevels)
        {
            int levelNumber =
                Mathf.Max(1, legacyLevel.LevelNumber);

            string assetPath =
                GetAssetPath(levelNumber);

            GridLevelData separateAsset =
                AssetDatabase.LoadAssetAtPath<GridLevelData>(
                    assetPath
                );

            if (separateAsset == null)
            {
                separateAsset =
                    ScriptableObject.CreateInstance<GridLevelData>();

                EditorUtility.CopySerialized(
                    legacyLevel,
                    separateAsset
                );

                separateAsset.name =
                    $"Level_{levelNumber:000}";

                separateAsset.hideFlags = HideFlags.None;
                separateAsset.EditorSetLevelNumber(levelNumber);
                AssetDatabase.CreateAsset(separateAsset, assetPath);
                EditorUtility.SetDirty(separateAsset);
                changed = true;
            }

            string address = GetAddress(levelNumber);
            MakeAddressable(assetPath, address);
            database.EditorRegisterAddress(levelNumber, address);
            changed = true;
        }

        /*
         * Existing catalog entries ko bhi repair/verify karte hain. Isse
         * manually moved Addressables group ya removed label next editor
         * reload par automatically theek ho jata hai.
         */
        for (int i = 0; i < database.Count; i++)
        {
            int levelNumber = database.GetLevelNumber(i);
            string assetPath = GetAssetPath(levelNumber);

            if (AssetDatabase.LoadAssetAtPath<GridLevelData>(assetPath) ==
                null)
            {
                continue;
            }

            string address = GetAddress(levelNumber);
            MakeAddressable(assetPath, address);
            database.EditorRegisterAddress(levelNumber, address);
        }

        if (legacyLevels.Count > 0)
        {
            database.EditorClearLegacyLevels();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            foreach (GridLevelData legacyLevel in legacyLevels)
            {
                if (legacyLevel != null &&
                    AssetDatabase.GetAssetPath(legacyLevel) == DatabasePath)
                {
                    Object.DestroyImmediate(legacyLevel, true);
                }
            }

            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"{database.Count} levels separate ScriptableObjects " +
                "mein migrate ho kar Addressable ban gaye hain.",
                database
            );
        }
    }


    private static string GetAssetPath(int levelNumber)
    {
        return
            $"{LevelsFolder}/Level_{Mathf.Max(1, levelNumber):000}.asset";
    }


    private static string GetAddress(int levelNumber)
    {
        return
            $"levels/{Mathf.Max(1, levelNumber):000}";
    }


    private static void EnsureLevelsFolderExists()
    {
        if (AssetDatabase.IsValidFolder(LevelsFolder))
        {
            return;
        }

        string parent = Path.GetDirectoryName(LevelsFolder)
            ?.Replace('\\', '/');

        string folderName = Path.GetFileName(LevelsFolder);

        AssetDatabase.CreateFolder(parent, folderName);
    }


    private static void MakeAddressable(
        string assetPath,
        string address)
    {
        string guid =
            AssetDatabase.AssetPathToGUID(assetPath);

        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError(
                $"Addressable level asset GUID missing: {assetPath}"
            );
            return;
        }

        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.GetSettings(true);

        AddressableAssetGroup group =
            settings.FindGroup(AddressablesGroupName);

        if (group == null)
        {
            group = settings.CreateGroup(
                AddressablesGroupName,
                false,
                false,
                false,
                null,
                typeof(ContentUpdateGroupSchema),
                typeof(BundledAssetGroupSchema)
            );
        }

        AddressableAssetEntry entry =
            settings.FindAssetEntry(guid);

        if (entry == null ||
            entry.parentGroup != group)
        {
            entry = settings.CreateOrMoveEntry(
                guid,
                group,
                false,
                true
            );
        }

        bool changed = false;

        if (entry.address != address)
        {
            entry.address = address;
            changed = true;
        }

        if (!entry.labels.Contains(AddressablesLabel))
        {
            entry.SetLabel(AddressablesLabel, true, true, true);
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(settings);
        }
    }
}

#endif
