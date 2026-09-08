using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slope Ball ke saare saved levels ki fehrist.
///
/// Canon wale GridLevelDatabase se alag hai — yahan Addressables ki
/// zaroorat nahi kyunke slope game usi scene mein chalta hai, is liye
/// levels seedhe reference se load hote hain.
/// </summary>
[CreateAssetMenu(
    fileName = "SlopeLevelDatabase",
    menuName = "Slope Ball/Slope Level Database",
    order = 1
)]
public sealed class SlopeLevelDatabase : ScriptableObject
{
    [Tooltip(
        "Level number ke hisaab se sorted saved levels. Editor ka " +
        "SAVE button khud isay maintain karta hai."
    )]
    [SerializeField]
    private List<SlopeLevelData> savedLevels =
        new List<SlopeLevelData>();


    public int Count => savedLevels.Count;

    public IReadOnlyList<SlopeLevelData> SavedLevels => savedLevels;


    public SlopeLevelData GetLevel(
        int index)
    {
        if (index < 0 ||
            index >= savedLevels.Count)
        {
            return null;
        }


        return savedLevels[index];
    }


    public int FindIndexByLevelNumber(
        int levelNumber)
    {
        for (int i = 0; i < savedLevels.Count; i++)
        {
            if (savedLevels[i] != null &&
                savedLevels[i].LevelNumber == levelNumber)
            {
                return i;
            }
        }


        return -1;
    }


    public SlopeLevelData FindByLevelNumber(
        int levelNumber)
    {
        int index = FindIndexByLevelNumber(levelNumber);

        return index < 0 ? null : savedLevels[index];
    }


    /// <summary>
    /// Sab se bara saved level number. Khali database par 0.
    /// </summary>
    public int GetHighestLevelNumber()
    {
        int highest = 0;

        for (int i = 0; i < savedLevels.Count; i++)
        {
            if (savedLevels[i] != null &&
                savedLevels[i].LevelNumber > highest)
            {
                highest = savedLevels[i].LevelNumber;
            }
        }


        return highest;
    }


#if UNITY_EDITOR
    /// <summary>
    /// Level ko list mein add ya replace karta hai aur level number
    /// ke hisaab se sort rakhta hai. Sirf editor ke liye.
    /// </summary>
    public void EditorRegister(
        SlopeLevelData level)
    {
        if (level == null)
        {
            return;
        }


        savedLevels.RemoveAll(
            entry => entry == null
        );


        int existing = FindIndexByLevelNumber(
            level.LevelNumber
        );

        if (existing >= 0)
        {
            savedLevels[existing] = level;
        }
        else
        {
            savedLevels.Add(level);
        }


        savedLevels.Sort(
            (a, b) => a.LevelNumber.CompareTo(b.LevelNumber)
        );


        UnityEditor.EditorUtility.SetDirty(this);
    }


    public void EditorRemove(
        int levelNumber)
    {
        int index = FindIndexByLevelNumber(levelNumber);

        if (index >= 0)
        {
            savedLevels.RemoveAt(index);
        }


        EditorPruneMissing();

        UnityEditor.EditorUtility.SetDirty(this);
    }


    /// <summary>
    /// Un entries ko hata deta hai jinka asset delete ho chuka hai.
    ///
    /// Designer kisi level asset ko Project window se seedha delete
    /// kar de to list mein khali (null) slot reh jata hai. Editor
    /// khulte waqt yeh khud saaf ho jata hai.
    /// </summary>
    public void EditorPruneMissing()
    {
        int removed = savedLevels.RemoveAll(
            entry => entry == null
        );

        if (removed > 0)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
