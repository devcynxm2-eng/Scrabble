using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "GridLevelDatabase",
    menuName = "Royal Smash/Grid Level Database")]
public sealed class GridLevelDatabase : ScriptableObject
{
    [Tooltip(
        "Level Creator se banne wale tamam levels ki central ordered list."
    )]
    [SerializeField]
    private List<GridLevelData> levels =
        new List<GridLevelData>();

    public IReadOnlyList<GridLevelData> Levels => levels;

    public int Count =>
        levels != null
            ? levels.Count
            : 0;


    public GridLevelData GetLevel(
        int index)
    {
        if (levels == null ||
            index < 0 ||
            index >= levels.Count)
        {
            return null;
        }

        return levels[index];
    }


    public int IndexOf(
        GridLevelData level)
    {
        return
            levels != null
                ? levels.IndexOf(level)
                : -1;
    }


    public GridLevelData GetLevelByNumber(
        int levelNumber)
    {
        if (levels == null)
        {
            return null;
        }

        foreach (GridLevelData level in levels)
        {
            if (level != null &&
                level.LevelNumber == levelNumber)
            {
                return level;
            }
        }

        return null;
    }


#if UNITY_EDITOR

    public void EditorRegisterLevel(
        GridLevelData level)
    {
        if (level == null)
        {
            return;
        }

        if (levels == null)
        {
            levels = new List<GridLevelData>();
        }

        levels.RemoveAll(entry => entry == null);

        if (!levels.Contains(level))
        {
            levels.Add(level);
        }

        levels.Sort(CompareLevels);
    }


    public void EditorReplaceLevel(
        GridLevelData oldLevel,
        GridLevelData replacement)
    {
        if (replacement == null)
        {
            return;
        }

        if (levels == null)
        {
            levels = new List<GridLevelData>();
        }

        int index = levels.IndexOf(oldLevel);

        if (index >= 0)
        {
            levels[index] = replacement;
        }
        else if (!levels.Contains(replacement))
        {
            levels.Add(replacement);
        }

        levels.RemoveAll(entry => entry == null);
        levels.Sort(CompareLevels);
    }


    private static int CompareLevels(
        GridLevelData left,
        GridLevelData right)
    {
        if (left == right)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int numberComparison =
            left.LevelNumber.CompareTo(
                right.LevelNumber
            );

        return
            numberComparison != 0
                ? numberComparison
                : string.CompareOrdinal(
                    left.name,
                    right.name
                );
    }

#endif
}
