using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class GridLevelAddressEntry
{
    [SerializeField, Min(1)]
    private int levelNumber = 1;

    [SerializeField]
    private string address;

    public int LevelNumber => levelNumber;

    public string Address => address;

#if UNITY_EDITOR
    public GridLevelAddressEntry(
        int newLevelNumber,
        string newAddress)
    {
        levelNumber = Mathf.Max(1, newLevelNumber);
        address = newAddress;
    }

    public void EditorSetAddress(string newAddress)
    {
        address = newAddress;
    }
#endif
}


[CreateAssetMenu(
    fileName = "GridLevelDatabase",
    menuName = "Royal Smash/Grid Level Database")]
public sealed class GridLevelDatabase : ScriptableObject
{
    [Tooltip(
        "Runtime par load hone wale per-level Addressable assets ki " +
        "ordered catalog. Full level data is asset mein store nahi hota."
    )]
    [SerializeField]
    private List<GridLevelAddressEntry> addressableLevels =
        new List<GridLevelAddressEntry>();

    /*
     * Purane central-database snapshots sirf one-time editor migration
     * ke liye rakhe hain. Runtime kabhi is list se level load nahi karta.
     * Field name intentionally 'levels' hai taa-ke existing serialized
     * data loss ke baghair deserialize ho sake.
     */
    [SerializeField, HideInInspector]
    private List<GridLevelData> levels =
        new List<GridLevelData>();

    public IReadOnlyList<GridLevelAddressEntry> AddressableLevels =>
        addressableLevels;

    public int Count =>
        addressableLevels != null
            ? addressableLevels.Count
            : 0;

    public int MaximumPlayableLevelNumber
    {
        get
        {
            int maximumLevelNumber = 0;

            if (addressableLevels == null)
            {
                return maximumLevelNumber;
            }

            foreach (GridLevelAddressEntry entry in addressableLevels)
            {
                if (entry != null)
                {
                    maximumLevelNumber = Mathf.Max(
                        maximumLevelNumber,
                        entry.LevelNumber
                    );
                }
            }

            return maximumLevelNumber;
        }
    }


#if UNITY_EDITOR

    /// <summary>
    /// Ek level ko poori tarah delete karta hai: Addressables group se
    /// uski entry, disk se .asset file, aur is database se catalog
    /// reference — teeno ek sath, taake koi dangling/orphaned reference
    /// na bache.
    /// </summary>
    public bool EditorDeleteLevel(int levelNumber)
    {
        int index =
            FindIndexByLevelNumber(levelNumber);

        if (index < 0)
        {
            Debug.LogWarning(
                $"EditorDeleteLevel: Level {levelNumber} database mein " +
                "nahi mila.",
                this
            );

            return false;
        }

        GridLevelAddressEntry entry =
            addressableLevels[index];

        string address =
            entry.Address;

        if (!string.IsNullOrWhiteSpace(address))
        {
            RemoveAddressableEntryAndAsset(address);
        }

        addressableLevels.RemoveAt(index);

        UnityEditor.EditorUtility.SetDirty(this);

        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log(
            $"Level {levelNumber} delete ho gaya (database + " +
            "Addressables + asset file).",
            this
        );

        return true;
    }


    private static void RemoveAddressableEntryAndAsset(
        string address)
    {
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings
            settings =
                UnityEditor.AddressableAssets.
                    AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            Debug.LogWarning(
                "Addressable Asset Settings nahi mile — sirf database " +
                "entry remove ho rahi hai, .asset file manually delete " +
                "karni hogi."
            );

            return;
        }

        UnityEditor.AddressableAssets.Settings.AddressableAssetEntry
            targetEntry =
                null;

        foreach (UnityEditor.AddressableAssets.Settings.
                     AddressableAssetGroup group in settings.groups)
        {
            if (group == null)
            {
                continue;
            }

            foreach (UnityEditor.AddressableAssets.Settings.
                         AddressableAssetEntry candidate in
                     group.entries)
            {
                if (candidate != null &&
                    candidate.address == address)
                {
                    targetEntry = candidate;
                    break;
                }
            }

            if (targetEntry != null)
            {
                break;
            }
        }

        if (targetEntry == null)
        {
            Debug.LogWarning(
                $"Addressables mein address '{address}' wali entry " +
                "nahi mili — sirf database entry remove ho rahi hai."
            );

            return;
        }

        string assetPath =
            targetEntry.AssetPath;

        settings.RemoveAssetEntry(
            targetEntry.guid
        );

        if (!string.IsNullOrWhiteSpace(assetPath))
        {
            UnityEditor.AssetDatabase.DeleteAsset(assetPath);
        }
    }

    /// <summary>
    /// Deletes every level currently in the database catalog (each
    /// one's Addressables entry + .asset file + database entry). Same
    /// per-level cleanup as EditorDeleteLevel(), just looped over
    /// everything.
    /// </summary>
    public int EditorDeleteAllLevels()
    {
        if (addressableLevels == null ||
            addressableLevels.Count == 0)
        {
            return 0;
        }

        /*
         * Snapshot level numbers first — deleting mutates
         * addressableLevels while we iterate.
         */
        List<int> levelNumbers =
            new List<int>(addressableLevels.Count);

        foreach (GridLevelAddressEntry entry in addressableLevels)
        {
            if (entry != null)
            {
                levelNumbers.Add(entry.LevelNumber);
            }
        }

        int deletedCount = 0;

        foreach (int levelNumber in levelNumbers)
        {
            if (EditorDeleteLevel(levelNumber))
            {
                deletedCount++;
            }
        }

        return deletedCount;
    }


    /// <summary>
    /// Deletes every level whose number falls within
    /// [rangeStart, rangeEnd] inclusive.
    /// </summary>
    public int EditorDeleteLevelRange(
        int rangeStart,
        int rangeEnd)
    {
        if (addressableLevels == null ||
            addressableLevels.Count == 0)
        {
            return 0;
        }

        List<int> levelNumbers =
            new List<int>();

        foreach (GridLevelAddressEntry entry in addressableLevels)
        {
            if (entry != null &&
                entry.LevelNumber >= rangeStart &&
                entry.LevelNumber <= rangeEnd)
            {
                levelNumbers.Add(entry.LevelNumber);
            }
        }

        int deletedCount = 0;

        foreach (int levelNumber in levelNumbers)
        {
            if (EditorDeleteLevel(levelNumber))
            {
                deletedCount++;
            }
        }

        return deletedCount;
    }

#endif


    public GridLevelAddressEntry GetEntry(int index)
    {
        if (addressableLevels == null ||
            index < 0 ||
            index >= addressableLevels.Count)
        {
            return null;
        }

        return addressableLevels[index];
    }


    public string GetAddress(int index)
    {
        GridLevelAddressEntry entry = GetEntry(index);

        return entry != null
            ? entry.Address
            : null;
    }


    public int GetLevelNumber(int index)
    {
        GridLevelAddressEntry entry = GetEntry(index);

        return entry != null
            ? entry.LevelNumber
            : -1;
    }


    public int FindIndexByLevelNumber(int levelNumber)
    {
        if (addressableLevels == null)
        {
            return -1;
        }

        for (int i = 0; i < addressableLevels.Count; i++)
        {
            GridLevelAddressEntry entry = addressableLevels[i];

            if (entry != null &&
                entry.LevelNumber == levelNumber)
            {
                return i;
            }
        }

        return -1;
    }


#if UNITY_EDITOR

    public IReadOnlyList<GridLevelData> EditorLegacyLevels => levels;

    public void EditorRegisterAddress(
        int levelNumber,
        string address)
    {
        if (addressableLevels == null)
        {
            addressableLevels =
                new List<GridLevelAddressEntry>();
        }

        int existingIndex =
            FindIndexByLevelNumber(levelNumber);

        if (existingIndex >= 0)
        {
            addressableLevels[existingIndex]
                .EditorSetAddress(address);
        }
        else
        {
            addressableLevels.Add(
                new GridLevelAddressEntry(
                    levelNumber,
                    address
                )
            );
        }

        addressableLevels.RemoveAll(entry => entry == null);
        addressableLevels.Sort(
            (left, right) =>
                left.LevelNumber.CompareTo(right.LevelNumber)
        );
    }


    public void EditorClearLegacyLevels()
    {
        if (levels == null)
        {
            levels = new List<GridLevelData>();
            return;
        }

        levels.Clear();
    }

#endif
}
