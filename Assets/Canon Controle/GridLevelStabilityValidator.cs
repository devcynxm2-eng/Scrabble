using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static grid validation used before a generated level is saved. It catches
/// missing definitions, out-of-bounds spans, empty table halves, and pieces
/// whose bottom footprint has no meaningful support on the row below.
/// </summary>
public static class GridLevelStabilityValidator
{
    private const float MinimumSupportRatio = 0.34f;


    public static bool Validate(
        GridLevelData level,
        out string summary)
    {
        List<string> errors = new List<string>();

        if (level == null)
        {
            summary = "Level data null hai.";
            return false;
        }

        if (!level.HasValidGrid)
        {
            summary = "Grid allocation invalid hai.";
            return false;
        }

        int anchorCount = 0;
        int unsupportedCount = 0;
        bool firstTableHasPiece = false;
        bool secondTableHasPiece = false;

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

                    anchorCount++;

                    if (x < level.SecondTableSplitColumn)
                    {
                        firstTableHasPiece = true;
                    }
                    else
                    {
                        secondTableHasPiece = true;
                    }

                    PhysicsObjectDefinition definition =
                        level.GetPaletteEntry(cell.DefinitionIndex);

                    if (definition == null || definition.Prefab == null)
                    {
                        AddError(
                            errors,
                            $"({x},{y},{z}) ki prefab definition missing hai."
                        );
                    }

                    if (x + cell.SpanX > level.GridWidth ||
                        y + cell.SpanY > level.GridHeight ||
                        z + cell.SpanZ > level.GridDepth)
                    {
                        AddError(
                            errors,
                            $"({x},{y},{z}) ka span grid se bahar ja raha hai."
                        );
                        continue;
                    }

                    if (y <= 0)
                    {
                        continue;
                    }

                    bool sufficientlySupported = HasSufficientSupport(
                        level,
                        x,
                        y,
                        z,
                        cell,
                        out float supportRatio
                    );

                    if (!sufficientlySupported)
                    {
                        unsupportedCount++;
                        AddError(
                            errors,
                            $"Floating block ({x},{y},{z}); support " +
                            $"{supportRatio:P0}."
                        );
                    }
                }
            }
        }

        if (anchorCount == 0)
        {
            AddError(errors, "Level mein koi playable block nahi hai.");
        }

        if (level.UseSecondTable &&
            (!firstTableHasPiece || !secondTableHasPiece))
        {
            AddError(
                errors,
                "Two-table level mein ek table ki grid half empty hai."
            );
        }

        if (errors.Count > 0)
        {
            summary =
                $"Validation failed: {errors.Count} issue(s), " +
                $"{unsupportedCount} unsupported. " +
                string.Join(" ", errors);
            return false;
        }

        summary =
            $"Valid structure: {anchorCount} pieces, no floating blocks" +
            (level.UseSecondTable ? ", both tables populated." : ".");
        return true;
    }


    private static bool HasSufficientSupport(
        GridLevelData level,
        int anchorX,
        int anchorY,
        int anchorZ,
        GridCellData cell,
        out float supportRatio)
    {
        float minimumX =
            anchorX - 0.5f + cell.LocalOffset.x;
        float maximumX =
            minimumX + Mathf.Max(1, cell.SpanX);
        float minimumZ =
            anchorZ - 0.5f + cell.LocalOffset.z;
        float maximumZ =
            minimumZ + Mathf.Max(1, cell.SpanZ);

        float footprintArea =
            Mathf.Max(0.0001f,
                (maximumX - minimumX) *
                (maximumZ - minimumZ));

        float supportedArea = 0f;
        float minimumSupportCenterX = float.PositiveInfinity;
        float maximumSupportCenterX = float.NegativeInfinity;
        float minimumSupportCenterZ = float.PositiveInfinity;
        float maximumSupportCenterZ = float.NegativeInfinity;
        int firstSupportX = Mathf.FloorToInt(minimumX - 0.5f);
        int lastSupportX = Mathf.CeilToInt(maximumX + 0.5f);
        int firstSupportZ = Mathf.FloorToInt(minimumZ - 0.5f);
        int lastSupportZ = Mathf.CeilToInt(maximumZ + 0.5f);

        for (int z = firstSupportZ; z <= lastSupportZ; z++)
        {
            for (int x = firstSupportX; x <= lastSupportX; x++)
            {
                GridCellData support =
                    level.GetCell(x, anchorY - 1, z);

                if (support == null || !support.Occupied)
                {
                    continue;
                }

                float overlapX = Mathf.Max(
                    0f,
                    Mathf.Min(maximumX, x + 0.5f) -
                    Mathf.Max(minimumX, x - 0.5f)
                );
                float overlapZ = Mathf.Max(
                    0f,
                    Mathf.Min(maximumZ, z + 0.5f) -
                    Mathf.Max(minimumZ, z - 0.5f)
                );

                supportedArea += overlapX * overlapZ;

                if (overlapX * overlapZ > 0.0001f)
                {
                    minimumSupportCenterX =
                        Mathf.Min(minimumSupportCenterX, x);
                    maximumSupportCenterX =
                        Mathf.Max(maximumSupportCenterX, x);
                    minimumSupportCenterZ =
                        Mathf.Min(minimumSupportCenterZ, z);
                    maximumSupportCenterZ =
                        Mathf.Max(maximumSupportCenterZ, z);
                }
            }
        }

        supportRatio = Mathf.Clamp01(supportedArea / footprintArea);

        float footprintCenterX = (minimumX + maximumX) * 0.5f;
        float footprintCenterZ = (minimumZ + maximumZ) * 0.5f;

        bool bridgedAcrossX =
            cell.SpanX > 1 &&
            minimumSupportCenterX < footprintCenterX &&
            maximumSupportCenterX > footprintCenterX;

        bool bridgedAcrossZ =
            cell.SpanZ > 1 &&
            minimumSupportCenterZ < footprintCenterZ &&
            maximumSupportCenterZ > footprintCenterZ;

        return supportRatio + 0.0001f >= MinimumSupportRatio ||
               bridgedAcrossX ||
               bridgedAcrossZ;
    }


    private static void AddError(
        List<string> errors,
        string error)
    {
        // Keep the inspector/log readable on a badly malformed level.
        if (errors.Count < 12)
        {
            errors.Add(error);
        }
    }
}
