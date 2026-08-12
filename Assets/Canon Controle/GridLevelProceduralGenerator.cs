#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Editor-only controlled procedural designer. It combines multiple
/// structural archetypes instead of recoloring one repeated pyramid.
/// </summary>
public static class GridLevelProceduralGenerator
{
    private enum Archetype
    {
        SteppedPyramid,
        TwinTowers,
        CastleGate,
        Colonnade,
        RoyalCrown,
        AlternatingFort,
        TripleSpire,
        Skyline,
        Hourglass,
        TerracedTemple
    }

    public const int StructuralPatternCount = 10;


    public readonly struct Settings
    {
        public readonly int Seed;
        public readonly int Difficulty;
        public readonly int GridWidth;
        public readonly int Rows;
        public readonly int BaseWidth;
        public readonly int Layers;
        public readonly float LayerGap;
        public readonly bool UseSeamRows;
        public readonly int ArchetypeIndex;
        public readonly bool UseTwoTables;
        public readonly float TableGap;
        public readonly float TwoTableForwardOffset;
        public readonly int MaximumShapeTypes;
        public readonly GridLevelColorPattern ColorPattern;

        public Settings(
            int seed,
            int difficulty,
            int gridWidth,
            int rows,
            int baseWidth,
            int layers,
            float layerGap,
            bool useSeamRows,
            int archetypeIndex = -1,
            bool useTwoTables = false,
            float tableGap = 0.65f,
            float twoTableForwardOffset = 0.8f,
            int maximumShapeTypes = 0,
            GridLevelColorPattern colorPattern =
                GridLevelColorPattern.Random)
        {
            Seed = seed;
            Difficulty = difficulty;
            GridWidth = gridWidth;
            Rows = rows;
            BaseWidth = baseWidth;
            Layers = layers;
            LayerGap = layerGap;
            UseSeamRows = useSeamRows;
            ArchetypeIndex = archetypeIndex;
            UseTwoTables = useTwoTables;
            TableGap = tableGap;
            TwoTableForwardOffset = twoTableForwardOffset;
            MaximumShapeTypes = maximumShapeTypes;
            ColorPattern = colorPattern;
        }
    }


    private sealed class GenerationContext
    {
        public GridLevelData Level;
        public IReadOnlyList<PhysicsObjectDefinition> Definitions;
        public IReadOnlyList<Color> Colors;
        public System.Random Random;
        public int Difficulty;
        public int GridWidth;
        public int Rows;
        public int BaseWidth;
        public int MaximumShapeTypes;
        public GridLevelColorPattern ColorPattern;
        public int Pieces;
        public int SeamPieces;
        public int LongBeams;
    }


    public static bool Generate(
        GridLevelData level,
        Settings settings,
        out string summary)
    {
        summary = string.Empty;

        if (!ValidateLevel(level, out summary))
        {
            return false;
        }

        int difficulty = Mathf.Clamp(settings.Difficulty, 1, 10);
        int gridWidth = Mathf.Clamp(settings.GridWidth, 6, 32);
        int rowCount = Mathf.Clamp(settings.Rows, 3, 24);
        int gridHeight = Mathf.Max(rowCount + 3, 8);

        int tableCellCapacity =
            GetSafeTableCellCapacity(level);

        bool useTwoTables =
            settings.UseTwoTables && gridWidth >= 8;

        int maximumBaseWidth =
            useTwoTables
                ? Mathf.Min(gridWidth - 2, tableCellCapacity * 2)
                : Mathf.Min(gridWidth - 2, tableCellCapacity);

        int baseWidth = Mathf.Clamp(
            settings.BaseWidth,
            4,
            Mathf.Max(4, maximumBaseWidth)
        );
        CalculateSafeDepthLayout(
            level,
            settings.Layers,
            settings.LayerGap,
            out int layerCount,
            out float safeLayerGap
        );

        level.EditorSetGridSize(gridWidth, gridHeight);
        level.EditorSetDepthLayerCount(layerCount);
        level.EditorSetDepthGap(safeLayerGap);
        level.EditorSetMirrorPaintAcrossLayers(layerCount > 1);
        level.EditorSetGridOffset(Vector3.zero);
        level.EditorConfigureTables(
            useTwoTables,
            gridWidth / 2,
            settings.TableGap,
            settings.TwoTableForwardOffset
        );
        level.EditorClearAllCells();

        GenerationContext context =
            new GenerationContext
            {
                Level = level,
                Definitions = level.BlockPalette,
                Colors = level.ColorPalette,
                Random = new System.Random(settings.Seed),
                Difficulty = difficulty,
                GridWidth = gridWidth,
                Rows = rowCount,
                BaseWidth = baseWidth,
                MaximumShapeTypes = settings.MaximumShapeTypes,
                ColorPattern = settings.ColorPattern
            };

        int archetypeCount = StructuralPatternCount;

        int rawArchetypeIndex =
            settings.ArchetypeIndex >= 0
                ? settings.ArchetypeIndex
                : settings.Seed;

        Archetype archetype =
            (Archetype)PositiveModulo(
                rawArchetypeIndex,
                archetypeCount
            );

        int variant =
            PositiveModulo(
                rawArchetypeIndex / archetypeCount,
                16
            );

        /*
         * Geometry is identical across depth layers so linked-layer
         * physics stays valid. Shape, color and Y rotation still vary.
         */
        for (int z = 0; z < layerCount; z++)
        {
            if (useTwoTables)
            {
                GenerateDualArenas(
                    context,
                    variant + (int)archetype,
                    z
                );
            }
            else
            {
                GenerateArchetype(
                    context,
                    archetype,
                    variant,
                    z,
                    settings.UseSeamRows
                );
            }
        }

        level.EditorSetAvailableBalls(30);
        level.RecalculateGridMetadata();

        summary =
            $"Pattern: {(useTwoTables ? "Dual Arenas" : GetArchetypeDisplayName(archetype))} | " +
            $"Pieces: {context.Pieces}, long beams: {context.LongBeams}, " +
            $"seam pieces: {context.SeamPieces}, rows: {rowCount}, " +
            $"layers: {layerCount}, tables: {(useTwoTables ? 2 : 1)}, " +
            $"safe table capacity: {tableCellCapacity} cells each, " +
            $"seed: {settings.Seed}.";

        return context.Pieces > 0;
    }


    private static bool ValidateLevel(
        GridLevelData level,
        out string error)
    {
        error = string.Empty;

        if (level == null)
        {
            error = "Working GridLevelData missing hai.";
            return false;
        }

        IReadOnlyList<PhysicsObjectDefinition> palette =
            level.BlockPalette;

        if (palette == null || palette.Count == 0)
        {
            error =
                "Block Palette empty hai. Pehle kam az kam ek prefab shape add karein.";
            return false;
        }

        for (int i = 0; i < palette.Count; i++)
        {
            if (palette[i] != null && palette[i].Prefab != null)
            {
                return true;
            }
        }

        error = "Block Palette mein koi valid prefab shape nahi hai.";
        return false;
    }


    private static void GenerateArchetype(
        GenerationContext context,
        Archetype archetype,
        int variant,
        int z,
        bool useSeams)
    {
        switch (archetype)
        {
            case Archetype.TwinTowers:
                GenerateTwinTowers(context, variant, z);
                break;

            case Archetype.CastleGate:
                GenerateCastleGate(context, variant, z);
                break;

            case Archetype.Colonnade:
                GenerateColonnade(context, variant, z);
                break;

            case Archetype.RoyalCrown:
                GenerateRoyalCrown(context, variant, z);
                break;

            case Archetype.AlternatingFort:
                GenerateAlternatingFort(context, variant, z);
                break;

            case Archetype.TripleSpire:
                GenerateTripleSpire(context, variant, z);
                break;

            case Archetype.Skyline:
                GenerateSkyline(context, variant, z);
                break;

            case Archetype.Hourglass:
                GenerateHourglass(context, variant, z);
                break;

            case Archetype.TerracedTemple:
                GenerateTerracedTemple(context, variant, z);
                break;

            default:
                GenerateSteppedPyramid(
                    context,
                    variant,
                    z,
                    useSeams
                );
                break;
        }
    }


    private static void GenerateSteppedPyramid(
        GenerationContext context,
        int variant,
        int z,
        bool useSeams)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int previousStart,
            out int previousEnd
        );

        for (int y = 0; y < context.Rows; y++)
        {
            int shrinkRate = variant % 2 == 0 ? 2 : 3;
            int shrink = y / shrinkRate;
            int width = Mathf.Max(2, context.BaseWidth - shrink * 2);

            GetCenteredRange(
                context.GridWidth,
                width,
                out int start,
                out int end
            );

            bool seamRow = useSeams && y > 0 && (y & 1) == 1;

            if (seamRow)
            {
                start = Mathf.Max(start, previousStart);
                end = Mathf.Min(end, previousEnd - 1);
            }
            else if (y > 0)
            {
                start = Mathf.Max(start, previousStart);
                end = Mathf.Min(end, previousEnd);
            }

            if (end < start)
            {
                break;
            }

            bool battlementTop =
                y == context.Rows - 1 && context.Difficulty >= 5;

            for (int x = start; x <= end; x++)
            {
                if (battlementTop && ((x - start) & 1) == 1)
                {
                    continue;
                }

                PaintCell(context, x, y, z, seamRow);
            }

            previousStart = start;
            previousEnd = end;
        }
    }


    private static void GenerateTwinTowers(
        GenerationContext context,
        int variant,
        int z)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int start,
            out int end
        );

        int maximumGap = Mathf.Max(1, context.BaseWidth - 4);
        int gapWidth = Mathf.Clamp(
            1 + variant % 3,
            1,
            maximumGap
        );
        int usableWidth = context.BaseWidth - gapWidth;
        int leftWidth = Mathf.Max(2, usableWidth / 2);
        int rightWidth = Mathf.Max(2, usableWidth - leftWidth);
        int leftEnd = start + leftWidth - 1;
        int rightStart = end - rightWidth + 1;

        for (int y = 0; y < context.Rows; y++)
        {
            int shrink = y / (variant % 2 == 0 ? 4 : 3);

            int currentLeftStart = Mathf.Min(leftEnd, start + shrink);
            int currentLeftEnd = Mathf.Max(currentLeftStart, leftEnd - shrink);
            int currentRightStart = Mathf.Min(end, rightStart + shrink);
            int currentRightEnd = Mathf.Max(currentRightStart, end - shrink);

            FillCells(
                context,
                currentLeftStart,
                currentLeftEnd,
                y,
                z
            );

            FillCells(
                context,
                currentRightStart,
                currentRightEnd,
                y,
                z
            );
        }
    }


    private static void GenerateCastleGate(
        GenerationContext context,
        int variant,
        int z)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int start,
            out int end
        );

        int maximumPillarWidth =
            Mathf.Max(1, (context.BaseWidth - 2) / 2);

        int pillarWidth = Mathf.Clamp(
            1 + variant % 2,
            1,
            maximumPillarWidth
        );
        int gateHeight = Mathf.Clamp(context.Rows - 3, 2, context.Rows - 1);

        for (int y = 0; y < gateHeight; y++)
        {
            FillCells(
                context,
                start,
                start + pillarWidth - 1,
                y,
                z
            );

            FillCells(
                context,
                end - pillarWidth + 1,
                end,
                y,
                z
            );
        }

        PaintBeam(
            context,
            start,
            end,
            gateHeight,
            z
        );

        for (int y = gateHeight + 1; y < context.Rows; y++)
        {
            FillCells(
                context,
                start,
                start + pillarWidth - 1,
                y,
                z
            );

            FillCells(
                context,
                end - pillarWidth + 1,
                end,
                y,
                z
            );
        }
    }


    private static void GenerateColonnade(
        GenerationContext context,
        int variant,
        int z)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int start,
            out int end
        );

        PaintBeam(context, start, end, 0, z);

        int roofY = Mathf.Max(2, context.Rows - 2);
        int spacing = variant % 2 == 0 ? 2 : 3;

        for (int x = start; x <= end; x += spacing)
        {
            for (int y = 1; y < roofY; y++)
            {
                PaintCell(context, x, y, z, false);
            }
        }

        if ((end - start) % spacing != 0)
        {
            for (int y = 1; y < roofY; y++)
            {
                PaintCell(context, end, y, z, false);
            }
        }

        PaintBeam(context, start, end, roofY, z);

        for (int x = start; x <= end; x += 2)
        {
            PaintCell(context, x, roofY + 1, z, false);
        }
    }


    private static void GenerateRoyalCrown(
        GenerationContext context,
        int variant,
        int z)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int start,
            out int end
        );

        int stemWidth = Mathf.Clamp(1 + variant % 3, 1, 3);
        int stemStart = (context.GridWidth - stemWidth) / 2;
        int stemEnd = stemStart + stemWidth - 1;
        int stemHeight = Mathf.Clamp(context.Rows / 2, 2, context.Rows - 2);

        for (int y = 0; y < stemHeight; y++)
        {
            FillCells(context, stemStart, stemEnd, y, z);
        }

        PaintBeam(context, start, end, stemHeight, z);

        int upperRows = context.Rows - stemHeight - 1;

        for (int row = 0; row < upperRows; row++)
        {
            int inset = row;
            int rowStart = Mathf.Min(end, start + inset);
            int rowEnd = Mathf.Max(rowStart, end - inset);
            int y = stemHeight + 1 + row;

            if (row == upperRows - 1 && context.Difficulty >= 4)
            {
                PaintCell(context, rowStart, y, z, false);

                if (rowEnd > rowStart)
                {
                    PaintCell(context, rowEnd, y, z, false);
                }

                int center = (rowStart + rowEnd) / 2;

                if (center != rowStart && center != rowEnd)
                {
                    PaintCell(context, center, y, z, false);
                }
            }
            else
            {
                FillCells(context, rowStart, rowEnd, y, z);
            }
        }
    }


    private static void GenerateAlternatingFort(
        GenerationContext context,
        int variant,
        int z)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int originalStart,
            out int originalEnd
        );

        int start = originalStart;
        int end = originalEnd;

        PaintBeam(context, start, end, 0, z);

        for (int y = 1; y < context.Rows; y++)
        {
            if ((y & 1) == 0)
            {
                if (y % 4 == 0 && end - start >= 5)
                {
                    start++;
                    end--;
                }

                PaintBeam(context, start, end, y, z);
                continue;
            }

            PaintCell(context, start, y, z, false);
            PaintCell(context, end, y, z, false);

            int center = (start + end) / 2;
            int shift = variant % 2 == 0 ? 0 : (y % 4 == 1 ? -1 : 1);
            int shiftedCenter = Mathf.Clamp(center + shift, start, end);

            if (shiftedCenter != start && shiftedCenter != end)
            {
                PaintCell(context, shiftedCenter, y, z, false);
            }
        }
    }


    private static void GenerateTripleSpire(
        GenerationContext context,
        int variant,
        int z)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int start,
            out int end
        );

        PaintBeam(context, start, end, 0, z);

        int[] centers =
        {
            start + 1,
            (start + end) / 2,
            end - 1
        };

        for (int tower = 0; tower < centers.Length; tower++)
        {
            int heightReduction =
                PositiveModulo(variant + tower * 2, 4);

            int height = Mathf.Max(
                3,
                context.Rows - heightReduction
            );

            int width =
                context.BaseWidth >= 8 &&
                PositiveModulo(variant + tower, 3) == 0
                    ? 2
                    : 1;

            int towerStart =
                Mathf.Clamp(
                    centers[tower] - (width - 1) / 2,
                    start,
                    end
                );

            int towerEnd = Mathf.Min(end, towerStart + width - 1);

            for (int y = 1; y < height; y++)
            {
                FillCells(context, towerStart, towerEnd, y, z);
            }
        }
    }


    private static void GenerateSkyline(
        GenerationContext context,
        int variant,
        int z)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int start,
            out int end
        );

        for (int x = start; x <= end; x++)
        {
            int local = x - start;
            int wave = PositiveModulo(
                local * 3 + variant * 5 + local * local,
                Mathf.Max(3, context.Rows - 2)
            );

            int height = Mathf.Clamp(
                3 + wave,
                3,
                context.Rows
            );

            for (int y = 0; y < height; y++)
            {
                PaintCell(context, x, y, z, false);
            }
        }
    }


    private static void GenerateHourglass(
        GenerationContext context,
        int variant,
        int z)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int start,
            out int end
        );

        int waistY = Mathf.Clamp(
            context.Rows / 2 + PositiveModulo(variant, 3) - 1,
            2,
            context.Rows - 2
        );

        for (int y = 0; y < waistY; y++)
        {
            int inset = Mathf.Min(y, context.BaseWidth / 3);
            FillCells(context, start + inset, end - inset, y, z);
        }

        /* One long physical beam safely supports the widening upper cup. */
        PaintBeam(context, start, end, waistY, z);

        int upperRows = context.Rows - waistY - 1;

        for (int row = 0; row < upperRows; row++)
        {
            int inset = Mathf.Max(0, upperRows - row - 1);
            inset = Mathf.Min(inset, context.BaseWidth / 3);
            FillCells(
                context,
                start + inset,
                end - inset,
                waistY + 1 + row,
                z
            );
        }
    }


    private static void GenerateTerracedTemple(
        GenerationContext context,
        int variant,
        int z)
    {
        GetCenteredRange(
            context.GridWidth,
            context.BaseWidth,
            out int start,
            out int end
        );

        int y = 0;
        int terrace = 0;

        while (y < context.Rows && start <= end)
        {
            PaintBeam(context, start, end, y, z);
            y++;

            if (y >= context.Rows)
            {
                break;
            }

            int pillarInset = PositiveModulo(variant + terrace, 2);
            int leftPillar = Mathf.Min(end, start + pillarInset);
            int rightPillar = Mathf.Max(start, end - pillarInset);

            PaintCell(context, leftPillar, y, z, false);

            if (rightPillar != leftPillar)
            {
                PaintCell(context, rightPillar, y, z, false);
            }

            y++;
            start++;
            end--;
            terrace++;
        }
    }


    private static void GenerateDualArenas(
        GenerationContext context,
        int variant,
        int z)
    {
        int split = context.GridWidth / 2;
        int perTableWidth = Mathf.Clamp(
            context.BaseWidth / 2,
            2,
            Mathf.Max(2, split - 1)
        );

        GetCenteredRangeInRegion(
            0,
            split - 1,
            perTableWidth,
            out int leftStart,
            out int leftEnd
        );

        GetCenteredRangeInRegion(
            split,
            context.GridWidth - 1,
            perTableWidth,
            out int rightStart,
            out int rightEnd
        );

        GenerateArenaSide(
            context,
            leftStart,
            leftEnd,
            Mathf.Clamp(
                context.Rows - PositiveModulo(variant, 3),
                4,
                context.Rows
            ),
            z,
            variant
        );

        GenerateArenaSide(
            context,
            rightStart,
            rightEnd,
            Mathf.Clamp(
                context.Rows - PositiveModulo(variant + 2, 4),
                4,
                context.Rows
            ),
            z,
            variant + 5
        );
    }


    private static void GenerateArenaSide(
        GenerationContext context,
        int start,
        int end,
        int height,
        int z,
        int style)
    {
        int styleIndex = PositiveModulo(style, 6);

        if (styleIndex == 0)
        {
            for (int y = 0; y < height; y++)
            {
                int inset = Mathf.Min(y / 2, (end - start) / 2);
                FillCells(context, start + inset, end - inset, y, z);
            }

            return;
        }

        if (styleIndex == 1)
        {
            PaintBeam(context, start, end, 0, z);

            for (int y = 1; y < height - 1; y++)
            {
                PaintCell(context, start, y, z, false);
                PaintCell(context, end, y, z, false);
            }

            PaintBeam(context, start, end, height - 1, z);
            return;
        }

        if (styleIndex == 2)
        {
            for (int x = start; x <= end; x++)
            {
                int columnHeight = Mathf.Clamp(
                    3 + PositiveModulo(x * 5 + style, height),
                    3,
                    height
                );

                for (int y = 0; y < columnHeight; y++)
                {
                    PaintCell(context, x, y, z, false);
                }
            }

            return;
        }

        int width = end - start + 1;
        int center = (start + end) / 2;

        if (styleIndex == 3)
        {
            int towerWidth = Mathf.Max(1, width / 3);

            for (int y = 0; y < height; y++)
            {
                int taper = y >= height - 2 ? 1 : 0;
                FillCells(
                    context,
                    start + taper,
                    start + towerWidth - 1,
                    y,
                    z
                );
                FillCells(
                    context,
                    end - towerWidth + 1,
                    end - taper,
                    y,
                    z
                );
            }

            return;
        }

        if (styleIndex == 4)
        {
            for (int y = 0; y < height; y++)
            {
                int inset = Mathf.Min(
                    y / 3,
                    Mathf.Max(0, (width - 2) / 2)
                );
                FillCells(context, start + inset, end - inset, y, z);
            }

            PaintCell(context, center, height, z, false);
            return;
        }

        for (int y = 0; y < height; y++)
        {
            bool wideBand = y < 2 || y >= height - 2;
            int bandInset = wideBand
                ? 0
                : Mathf.Max(1, width / 3);

            FillCells(
                context,
                start + bandInset,
                end - bandInset,
                y,
                z
            );
        }
    }


    private static void GetCenteredRangeInRegion(
        int regionStart,
        int regionEnd,
        int desiredWidth,
        out int start,
        out int end)
    {
        int regionWidth = regionEnd - regionStart + 1;
        desiredWidth = Mathf.Clamp(desiredWidth, 1, regionWidth);
        start = regionStart + (regionWidth - desiredWidth) / 2;
        end = start + desiredWidth - 1;
    }


    private static void FillCells(
        GenerationContext context,
        int start,
        int end,
        int y,
        int z)
    {
        for (int x = start; x <= end; x++)
        {
            PaintCell(context, x, y, z, false);
        }
    }


    private static void PaintCell(
        GenerationContext context,
        int x,
        int y,
        int z,
        bool seam)
    {
        if (x < 0 || x >= context.GridWidth || y < 0)
        {
            return;
        }

        context.Level.EditorPaintSpan(
            x,
            y,
            z,
            1,
            1,
            1,
            PickColor(
                context.Colors,
                context.Random,
                x,
                y,
                z,
                context.ColorPattern
            ),
            PickValidDefinitionIndex(
                context.Definitions,
                context.Random,
                context.Difficulty,
                context.MaximumShapeTypes
            ),
            PickOrientation(context.Random, context.Difficulty),
            seam
                ? new Vector3(0.5f, 0f, 0f)
                : Vector3.zero
        );

        context.Pieces++;

        if (seam)
        {
            context.SeamPieces++;
        }
    }


    private static void PaintBeam(
        GenerationContext context,
        int start,
        int end,
        int y,
        int z)
    {
        start = Mathf.Clamp(start, 0, context.GridWidth - 1);
        end = Mathf.Clamp(end, start, context.GridWidth - 1);

        context.Level.EditorPaintSpan(
            start,
            y,
            z,
            end - start + 1,
            1,
            1,
            PickColor(
                context.Colors,
                context.Random,
                start,
                y,
                z,
                context.ColorPattern
            ),
            PickValidDefinitionIndex(
                context.Definitions,
                context.Random,
                context.Difficulty,
                context.MaximumShapeTypes
            ),
            PieceOrientation.RotatedY90,
            Vector3.zero
        );

        context.Pieces++;
        context.LongBeams++;
    }


    private static void GetCenteredRange(
        int gridWidth,
        int desiredWidth,
        out int start,
        out int end)
    {
        desiredWidth = Mathf.Clamp(desiredWidth, 1, gridWidth);
        start = (gridWidth - desiredWidth) / 2;
        end = start + desiredWidth - 1;
    }


    private static int PickValidDefinitionIndex(
        IReadOnlyList<PhysicsObjectDefinition> palette,
        System.Random random,
        int difficulty,
        int maximumShapeTypes)
    {
        List<int> validIndices = new List<int>();

        for (int i = 0; i < palette.Count; i++)
        {
            if (palette[i] != null && palette[i].Prefab != null)
            {
                validIndices.Add(i);
            }
        }

        if (validIndices.Count == 1 || difficulty <= 2)
        {
            return validIndices[0];
        }

        int usableCount = Mathf.Clamp(
            1 + difficulty / 3,
            1,
            validIndices.Count
        );

        if (maximumShapeTypes > 0)
        {
            usableCount = Mathf.Min(usableCount, maximumShapeTypes);
        }

        return validIndices[random.Next(usableCount)];
    }


    private static Color PickColor(
        IReadOnlyList<Color> colors,
        System.Random random,
        int x,
        int y,
        int z,
        GridLevelColorPattern pattern)
    {
        if (colors == null || colors.Count == 0)
        {
            return Color.white;
        }

        int index;

        switch (pattern)
        {
            case GridLevelColorPattern.HorizontalBands:
                index = PositiveModulo(y + z, colors.Count);
                break;

            case GridLevelColorPattern.Alternating:
                index = PositiveModulo(x + y + z, colors.Count);
                break;

            default:
                index = random.Next(colors.Count);
                break;
        }

        return colors[index];
    }


    private static PieceOrientation PickOrientation(
        System.Random random,
        int difficulty)
    {
        if (difficulty <= 2 || random.NextDouble() > 0.42)
        {
            return PieceOrientation.UprightY;
        }

        PieceOrientation[] safeRotations =
        {
            PieceOrientation.UprightY,
            PieceOrientation.RotatedY90,
            PieceOrientation.RotatedY180,
            PieceOrientation.RotatedY270
        };

        return safeRotations[random.Next(safeRotations.Length)];
    }


    private static int PositiveModulo(int value, int modulus)
    {
        int result = value % modulus;
        return result < 0 ? result + modulus : result;
    }


    private static int GetSafeTableCellCapacity(
        GridLevelData level)
    {
        if (level.TablePrefab == null ||
            !level.TablePrefab.TryGetTowerSurfaceSize(
                out Vector2 tableSize))
        {
            return Mathf.Max(4, level.GridWidth - 2);
        }

        float stepX =
            level.CellSize.x + level.HorizontalGap;

        if (stepX <= 0.001f)
        {
            return Mathf.Max(4, level.GridWidth - 2);
        }

        /* 90% leaves collider-edge safety space on both sides. */
        return Mathf.Max(
            4,
            Mathf.FloorToInt(
                (tableSize.x * 0.9f + level.HorizontalGap) /
                stepX
            )
        );
    }


    private static void CalculateSafeDepthLayout(
        GridLevelData level,
        int requestedLayers,
        float requestedGap,
        out int safeLayers,
        out float safeGap)
    {
        safeLayers = Mathf.Clamp(requestedLayers, 1, 3);
        safeGap = Mathf.Max(0f, requestedGap);

        if (level.TablePrefab == null ||
            !level.TablePrefab.TryGetTowerSurfaceSize(
                out Vector2 tableSize))
        {
            return;
        }

        float safeDepth = tableSize.y * 0.95f;
        float cellDepth = Mathf.Max(0.001f, level.CellSize.z);

        while (safeLayers > 1 &&
               cellDepth * safeLayers > safeDepth)
        {
            safeLayers--;
        }

        if (safeLayers <= 1)
        {
            safeLayers = 1;
            safeGap = 0f;
            return;
        }

        float maximumGap =
            (safeDepth - cellDepth * safeLayers) /
            (safeLayers - 1);

        safeGap = Mathf.Clamp(
            safeGap,
            0f,
            Mathf.Max(0f, maximumGap)
        );
    }


    private static string GetArchetypeDisplayName(
        Archetype archetype)
    {
        switch (archetype)
        {
            case Archetype.TwinTowers:
                return "Twin Towers";
            case Archetype.CastleGate:
                return "Castle Gate";
            case Archetype.Colonnade:
                return "Pillars And Roof";
            case Archetype.RoyalCrown:
                return "Top-Heavy Crown";
            case Archetype.AlternatingFort:
                return "Alternating Fort";
            case Archetype.TripleSpire:
                return "Triple Spire";
            case Archetype.Skyline:
                return "Royal Skyline";
            case Archetype.Hourglass:
                return "Hourglass Tower";
            case Archetype.TerracedTemple:
                return "Terraced Temple";
            default:
                return "Stepped Pyramid";
        }
    }
}

#endif
