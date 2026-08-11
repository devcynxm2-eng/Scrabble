using UnityEngine;

/// <summary>
/// Quick-rotate options for a painted piece. UprightY is how the
/// prefab was authored (its long axis, if any, pointing up). LyingX/
/// LyingZ tip it 90 degrees so that same long axis points sideways
/// instead — used to turn a "vertical" piece "horizontal" from the
/// grid painter without hand-authoring a second rotated prefab.
/// </summary>
public enum PieceOrientation
{
    // Explicit values preserve orientations already serialized in
    // existing GridLevelData assets.
    UprightY = 0,
    LyingX = 1,
    LyingZ = 2,
    RotatedY90 = 3,
    RotatedY180 = 4,
    RotatedY270 = 5,
    RotatedZ90 = 6,
    RotatedZ180 = 7,
    RotatedZ270 = 8,
    CustomZ = 9
}

public static class PieceOrientationUtility
{
    /// <summary>
    /// Extra local rotation (on top of the table's surface rotation)
    /// needed to visually tip a piece from upright into this orientation.
    /// </summary>
    public static Quaternion GetRotation(
        PieceOrientation orientation,
        float customZRotation = 0f)
    {
        switch (orientation)
        {
            case PieceOrientation.LyingX:
                return Quaternion.Euler(0f, 0f, 90f);

            case PieceOrientation.LyingZ:
                return Quaternion.Euler(90f, 0f, 0f);

            case PieceOrientation.RotatedY90:
                return Quaternion.Euler(0f, 90f, 0f);

            case PieceOrientation.RotatedY180:
                return Quaternion.Euler(0f, 180f, 0f);

            case PieceOrientation.RotatedY270:
                return Quaternion.Euler(0f, 270f, 0f);

            case PieceOrientation.RotatedZ90:
                return Quaternion.Euler(0f, 0f, 90f);

            case PieceOrientation.RotatedZ180:
                return Quaternion.Euler(0f, 0f, 180f);

            case PieceOrientation.RotatedZ270:
                return Quaternion.Euler(0f, 0f, 270f);

            case PieceOrientation.CustomZ:
                return Quaternion.Euler(
                    0f,
                    0f,
                    customZRotation
                );

            default:
                return Quaternion.identity;
        }
    }

    /// <summary>
    /// Re-expresses an upright natural footprint (in grid cells) as it
    /// should occupy the grid once tipped into this orientation — e.g.
    /// a naturally (1,2,1) tall piece becomes (2,1,1) wide when laid
    /// LyingX.
    /// </summary>
    public static Vector3Int ApplyToFootprint(
        Vector3Int uprightFootprint,
        PieceOrientation orientation)
    {
        switch (orientation)
        {
            case PieceOrientation.LyingX:
            case PieceOrientation.RotatedZ90:
            case PieceOrientation.RotatedZ270:
                return new Vector3Int(
                    uprightFootprint.y,
                    uprightFootprint.x,
                    uprightFootprint.z
                );

            case PieceOrientation.LyingZ:
                return new Vector3Int(
                    uprightFootprint.x,
                    uprightFootprint.z,
                    uprightFootprint.y
                );

            case PieceOrientation.RotatedY90:
            case PieceOrientation.RotatedY270:
                return new Vector3Int(
                    uprightFootprint.z,
                    uprightFootprint.y,
                    uprightFootprint.x
                );

            default:
                return uprightFootprint;
        }
    }

    /// <summary>
    /// Grid-aligned target world size (from cellSize * span) needs to be
    /// re-mapped onto the prefab's own UNROTATED local axes before
    /// computing its fit scale — otherwise a tipped piece would be
    /// stretched on the wrong axis. This is ApplyToFootprint's inverse:
    /// it tells us which local axis (x/y/z) is responsible for filling
    /// which world-space target extent once GetRotation() is applied.
    /// </summary>
    public static Vector3 GetLocalAxisTargetSize(
        Vector3 gridAlignedTargetSize,
        PieceOrientation orientation)
    {
        switch (orientation)
        {
            case PieceOrientation.LyingX:
            case PieceOrientation.RotatedZ90:
            case PieceOrientation.RotatedZ270:
                return new Vector3(
                    gridAlignedTargetSize.y,
                    gridAlignedTargetSize.x,
                    gridAlignedTargetSize.z
                );

            case PieceOrientation.LyingZ:
                return new Vector3(
                    gridAlignedTargetSize.x,
                    gridAlignedTargetSize.z,
                    gridAlignedTargetSize.y
                );

            case PieceOrientation.RotatedY90:
            case PieceOrientation.RotatedY270:
                return new Vector3(
                    gridAlignedTargetSize.z,
                    gridAlignedTargetSize.y,
                    gridAlignedTargetSize.x
                );

            default:
                return gridAlignedTargetSize;
        }
    }
}
