using UnityEngine;

/// <summary>
/// Ek SlopeLevelData asset ko scene ke GlassTowerController aur
/// BallDropController par apply karta hai.
///
/// Ye kaam Awake mein hota hai. GlassTowerController aur
/// BallDropController dono sirf Start() use karte hain, aur Unity
/// har Awake ko har Start se pehle chalata hai, is liye tower banne
/// se pehle level ki values lag chuki hoti hain.
/// </summary>
[DisallowMultipleComponent]
public sealed class SlopeLevelLoader : MonoBehaviour
{
    [Header("Level")]

    [Tooltip("Jo level load karna hai. Khali chhorne par kuch nahi hota.")]
    [SerializeField]
    private SlopeLevelData level;
    [SerializeField] private SlopeLevelData[] levels;
    [SerializeField] private int currentLevelIndex;
    private float completionTime = -1f;

    private void Update()
    {
        if (towerController == null || !towerController.isActiveAndEnabled ||
            level == null || levels == null || towerController.GeneratedGlasses.Count == 0)
            return;

        foreach (var glass in towerController.GeneratedGlasses)
        {
            // A toppled glass counts even when it lands intact below break speed.
            if (glass != null && glass.IsSupporting)
            {
                completionTime = -1f;
                return;
            }
        }
        if (completionTime < 0f)
            completionTime = Time.time + 0.75f;
        if (Time.time < completionTime)
            return;

        int index = System.Array.IndexOf(levels, level);
        if (index < 0)
            return;
        for (int i = index + 1; i < levels.Length; i++)
        {
            if (levels[i] == null)
                continue;
            currentLevelIndex = i;
            if (ballController != null) ballController.ClearReleasedBalls();
            RebuildWithLevel(levels[i]);
            return;
        }
    }

    private void OnDisable() => completionTime = -1f;


    [Header("Scene References")]

    [SerializeField]
    private GlassTowerController towerController;


    [Tooltip(
        "Ball ke movement / power settings isi par lagti hain. " +
        "Optional hai."
    )]
    [SerializeField]
    private BallDropController ballController;


    public SlopeLevelData Level => level;


    private void Awake()
    {
        ApplyLevel();
    }


    /// <summary>
    /// Assigned level ki values scene ke controllers par likh deta hai.
    /// </summary>
    [ContextMenu("Apply Level")]
    public void ApplyLevel()
    {
        completionTime = -1f;
        if (level == null)
        {
            Debug.LogWarning(
                "SlopeLevelLoader: level not assigned.",
                this
            );

            return;
        }


        ApplyTowerSettings();


        ApplyBallSettings();
    }


    /// <summary>
    /// Runtime par level badalne ke liye. Tower dobara banane ke
    /// liye GlassTowerController.BuildTower() alag se call karna hoga,
    /// ya seedha RebuildNow() use karo.
    /// </summary>
    public void SetLevel(
        SlopeLevelData newLevel)
    {
        level = newLevel;


        ApplyLevel();
    }


    /// <summary>
    /// Level apply karke tower ko foran dobara banata hai.
    ///
    /// Editor ka Live Preview isi ko call karta hai, taake design
    /// change karte hi Game View mein naya tower nazar aa jaye.
    /// Sirf Play Mode mein kaam karta hai kyunke GlassTowerController
    /// runtime par hi tower banata hai.
    /// </summary>
    public void RebuildNow()
    {
        ApplyLevel();


        if (towerController != null)
        {
            towerController.BuildTower();
        }
    }


    /// <summary>
    /// Live preview ke liye level set karke foran rebuild.
    /// </summary>
    public void RebuildWithLevel(
        SlopeLevelData newLevel)
    {
        level = newLevel;


        RebuildNow();
    }


    private void ApplyTowerSettings()
    {
        if (towerController == null)
        {
            return;
        }


        /*
         * Painted grid seedha cell mask ban jata hai. Iske lagne ke
         * baad GlassTowerController rows / pyramidShape ko ignore
         * karta hai aur exactly wahi shape banata hai jo editor
         * mein paint hui thi.
         */
        towerController.SetCellMask(
            level.GridColumns,
            level.GridRows,
            level.GridLayers,
            level.LayerSpacing,
            level.Cells,
            level.ObjectPalette
        );

        towerController.maskAutoRowSpacing = level.AutoRowSpacing;
        towerController.maskRestOnGround = level.RestOnGround;

        towerController.glassScale = level.GlassScale;
        towerController.spacing = level.Spacing;
        towerController.localOrigin = level.LocalOrigin;

        towerController.collapseStepDelay =
            level.CollapseStepDelay;

        towerController.usePhysicalSupportCheck =
            level.UsePhysicalSupportCheck;

        towerController.physicalSupportDistance =
            level.PhysicalSupportDistance;

        towerController.physicalSupportProbeRadius =
            level.PhysicalSupportProbeRadius;
    }


    private void ApplyBallSettings()
    {
        if (ballController == null)
        {
            return;
        }


        ballController.ballLifetime = level.BallLifetime;
        ballController.minXPosition = level.MinXPosition;
        ballController.maxXPosition = level.MaxXPosition;
        ballController.minimumFallSpeed = level.MinimumFallSpeed;
        ballController.maximumFallSpeed = level.MaximumFallSpeed;

        ballController.extraDownwardAcceleration =
            level.ExtraDownwardAcceleration;

        ballController.powerChargeSpeed = level.PowerChargeSpeed;
        ballController.minimumPower = level.MinimumPower;
        ballController.maximumPower = level.MaximumPower;
    }
}
