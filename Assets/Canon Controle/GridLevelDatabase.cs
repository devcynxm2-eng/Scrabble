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
