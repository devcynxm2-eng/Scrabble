using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GlassTowerController : MonoBehaviour
{
    [Header("Glass Prefab")]
    [Tooltip("Use a prefab with BreakableGlass on its root. Broken parts should be children of that prefab.")]
    public BreakableGlass glassPrefab;

    [Tooltip("Generated glasses are parented here. This transform is used when left empty.")]
    public Transform towerRoot;

    [Tooltip("Disable a scene object used as the template after its copies are built. Prefab assets are unaffected.")]
    public bool hideSceneTemplate = true;

    [Header("Tower Shape")]
    [Min(1)]
    public int rows = 5;

    [Min(1)]
    public int baseColumns = 5;

    [Tooltip("Each row above contains one fewer glass, producing a centered pyramid.")]
    public bool pyramidShape = true;

    [Tooltip("Uniform scale applied to every generated glass.")]
    [Range(0.1f, 2f)]
    public float glassScale = 0.65f;

    [Tooltip("Local distance between glass centres.")]
    public Vector2 spacing = new Vector2(0.635f, 1.285f);

    [Tooltip("Offset of the tower base from Tower Root.")]
    public Vector3 localOrigin = Vector3.zero;

    [Header("Painted Level (optional)")]
    [Tooltip("Build the tower from a painted cell mask instead of rows/pyramid.")]
    public bool useCellMask;

    [Tooltip("Width of the painted grid. Every row uses this same width.")]
    [Min(1)]
    public int maskColumns = 5;

    [Min(1)]
    public int maskRows = 5;

    [Tooltip("Depth of the painted grid. 2 builds a tower two objects thick.")]
    [Min(1)]
    public int maskLayers = 1;

    [Tooltip("Z distance between painted layers.")]
    public float maskLayerSpacing = 0.6f;

    [Tooltip("Objects a painted cell can reference. Empty falls back to Glass Prefab.")]
    public List<BreakableGlass> maskPalette = new List<BreakableGlass>();

    [Tooltip("Palette index per cell, -1 for empty. Ordered layer, then row, then column.")]
    public List<int> cellMask = new List<int>();

    [Header("Runtime")]
    public bool buildOnStart = true;

    [Tooltip("Delay between support loss and the next unsupported row shattering.")]
    [Min(0f)]
    public float collapseStepDelay = 0.08f;

    [Header("Physical Support Check")]
    [Tooltip("Check for a real collider directly below each upper glass.")]
    public bool usePhysicalSupportCheck = true;

    [Min(0.01f)]
    public float physicalSupportDistance = 0.18f;

    [Min(0.005f)]
    public float physicalSupportProbeRadius = 0.04f;

    public LayerMask physicalSupportMask = ~0;

    private readonly List<BreakableGlass> generatedGlasses =
        new List<BreakableGlass>();

    private readonly Dictionary<BreakableGlass, List<BreakableGlass>> supports =
        new Dictionary<BreakableGlass, List<BreakableGlass>>();

    private Coroutine collapseRoutine;
    private bool clearingTower;

    public IReadOnlyList<BreakableGlass> GeneratedGlasses => generatedGlasses;

    private void Start()
    {
        if (buildOnStart)
        {
            BuildTower();
        }
    }

    private void FixedUpdate()
    {
        if (collapseRoutine != null || supports.Count == 0)
        {
            return;
        }

        // Keep auditing support during physics simulation. This also catches a
        // support Rigidbody that fell or was destroyed without sending an event.
        if (FindUnsupportedGlasses().Count > 0)
        {
            collapseRoutine = StartCoroutine(CollapseUnsupportedGlasses());
        }
    }

    [ContextMenu("Build Tower (Play Mode)")]
    public void BuildTower()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Glass towers are generated at runtime. Enter Play Mode to build the tower.",
                this);
            return;
        }

        ClearTower();

        if (glassPrefab == null)
        {
            Debug.LogWarning("Glass Tower needs a Glass Prefab.", this);
            return;
        }

        Transform parent = towerRoot != null ? towerRoot : transform;

        // A painted level takes over. Without a mask the original
        // rows/pyramid path below runs exactly as before.
        if (UsesCellMask())
        {
            BuildMaskedTower(parent);
            HideMaskTemplatesIfNeeded();
            return;
        }

        int rowCount = Mathf.Max(1, rows);
        int bottomColumnCount = Mathf.Max(1, baseColumns);
        List<List<BreakableGlass>> towerRows =
            new List<List<BreakableGlass>>(rowCount);

        for (int row = 0; row < rowCount; row++)
        {
            int columnCount = pyramidShape
                ? Mathf.Max(1, bottomColumnCount - row)
                : bottomColumnCount;

            List<BreakableGlass> currentRow =
                new List<BreakableGlass>(columnCount);
            float rowWidth = (columnCount - 1) * spacing.x;

            for (int column = 0; column < columnCount; column++)
            {
                BreakableGlass glass = Instantiate(glassPrefab, parent);
                glass.name = $"Glass_R{row}_C{column}";
                glass.transform.localPosition = localOrigin + new Vector3(
                    column * spacing.x - rowWidth * 0.5f,
                    row * spacing.y,
                    0f);
                glass.transform.localRotation = Quaternion.identity;
                glass.transform.localScale =
                    glassPrefab.transform.localScale * glassScale;
                CopyExternalBrokenPartsIfNeeded(glass, parent);
                glass.ConfigureTower(this, row, column);

                currentRow.Add(glass);
                generatedGlasses.Add(glass);
                supports.Add(glass, new List<BreakableGlass>(2));
            }

            towerRows.Add(currentRow);
        }

        AssignSupports(towerRows);
        HideSceneTemplateIfNeeded();
    }

    public const int EmptyMaskCell = -1;

    public bool UsesCellMask()
    {
        return useCellMask &&
               cellMask != null &&
               cellMask.Count > 0 &&
               maskColumns > 0 &&
               maskRows > 0 &&
               maskLayers > 0;
    }

    /// <summary>
    /// Palette index at a painted cell, or EmptyMaskCell when empty.
    /// </summary>
    public int GetMaskCell(int layer, int row, int column)
    {
        if (!UsesCellMask() ||
            layer < 0 || row < 0 || column < 0 ||
            layer >= maskLayers ||
            row >= maskRows ||
            column >= maskColumns)
        {
            return EmptyMaskCell;
        }

        int index = (layer * maskRows + row) * maskColumns + column;
        return index < cellMask.Count ? cellMask[index] : EmptyMaskCell;
    }

    /// <summary>
    /// The object a painted cell spawns. Falls back to Glass Prefab when the
    /// palette is empty or the index does not resolve, so a level that never
    /// set up a palette still builds.
    /// </summary>
    public BreakableGlass GetMaskPrefab(int definitionIndex)
    {
        if (maskPalette == null ||
            definitionIndex < 0 ||
            definitionIndex >= maskPalette.Count ||
            maskPalette[definitionIndex] == null)
        {
            return glassPrefab;
        }

        return maskPalette[definitionIndex];
    }

    /// <summary>
    /// Replaces the painted level. Call before BuildTower.
    /// </summary>
    public void SetCellMask(
        int columns,
        int rows,
        int layers,
        float layerSpacing,
        IReadOnlyList<int> mask,
        IReadOnlyList<BreakableGlass> palette)
    {
        maskColumns = Mathf.Max(1, columns);
        maskRows = Mathf.Max(1, rows);
        maskLayers = Mathf.Max(1, layers);
        maskLayerSpacing = layerSpacing;

        cellMask.Clear();
        maskPalette.Clear();

        if (palette != null)
        {
            for (int i = 0; i < palette.Count; i++)
            {
                maskPalette.Add(palette[i]);
            }
        }

        if (mask == null)
        {
            useCellMask = false;
            return;
        }

        for (int i = 0; i < mask.Count; i++)
        {
            cellMask.Add(mask[i]);
        }

        useCellMask = cellMask.Count > 0;
    }

    /// <summary>
    /// Builds a painted tower. Every column sits on a fixed lattice, so an
    /// object is held up by the one directly beneath it in the same layer.
    /// Painting an object with nothing below leaves it unsupported, and it
    /// drops on the first support audit exactly like one whose support was
    /// shattered.
    /// </summary>
    private void BuildMaskedTower(Transform parent)
    {
        float rowWidth = (maskColumns - 1) * spacing.x;
        float depthWidth = (maskLayers - 1) * maskLayerSpacing;

        for (int layer = 0; layer < maskLayers; layer++)
        {
            List<BreakableGlass[]> grid =
                new List<BreakableGlass[]>(maskRows);

            for (int row = 0; row < maskRows; row++)
            {
                BreakableGlass[] currentRow =
                    new BreakableGlass[maskColumns];

                for (int column = 0; column < maskColumns; column++)
                {
                    int definitionIndex =
                        GetMaskCell(layer, row, column);

                    if (definitionIndex == EmptyMaskCell)
                    {
                        continue;
                    }

                    BreakableGlass prefab =
                        GetMaskPrefab(definitionIndex);

                    if (prefab == null)
                    {
                        continue;
                    }

                    BreakableGlass glass = Instantiate(prefab, parent);
                    glass.name = $"Glass_L{layer}_R{row}_C{column}";
                    glass.transform.localPosition = localOrigin + new Vector3(
                        column * spacing.x - rowWidth * 0.5f,
                        row * spacing.y,
                        layer * maskLayerSpacing - depthWidth * 0.5f);
                    glass.transform.localRotation = Quaternion.identity;
                    glass.transform.localScale =
                        prefab.transform.localScale * glassScale;
                    CopyExternalBrokenPartsIfNeeded(glass, parent, prefab);
                    glass.ConfigureTower(this, row, column);

                    currentRow[column] = glass;
                    generatedGlasses.Add(glass);
                    supports.Add(glass, new List<BreakableGlass>(1));
                }

                grid.Add(currentRow);
            }

            AssignMaskedSupports(grid);
        }
    }

    private void AssignMaskedSupports(List<BreakableGlass[]> grid)
    {
        for (int row = 1; row < grid.Count; row++)
        {
            BreakableGlass[] lowerRow = grid[row - 1];
            BreakableGlass[] currentRow = grid[row];

            for (int column = 0; column < currentRow.Length; column++)
            {
                BreakableGlass glass = currentRow[column];

                if (glass == null || lowerRow[column] == null)
                {
                    continue;
                }

                supports[glass].Add(lowerRow[column]);
            }
        }
    }

    /// <summary>
    /// Hides every scene object used as a painted template, plus the
    /// default Glass Prefab template.
    /// </summary>
    private void HideMaskTemplatesIfNeeded()
    {
        HideSceneTemplateIfNeeded();

        if (!hideSceneTemplate || maskPalette == null)
        {
            return;
        }

        for (int i = 0; i < maskPalette.Count; i++)
        {
            BreakableGlass entry = maskPalette[i];

            if (entry == null ||
                !entry.gameObject.scene.IsValid())
            {
                continue;
            }

            GameObject externalBrokenParts = entry.brokenParts;
            entry.gameObject.SetActive(false);

            if (externalBrokenParts != null &&
                externalBrokenParts != entry.gameObject &&
                !externalBrokenParts.transform.IsChildOf(entry.transform))
            {
                externalBrokenParts.SetActive(false);
            }
        }
    }

    private void CopyExternalBrokenPartsIfNeeded(
        BreakableGlass glass,
        Transform towerParent)
    {
        CopyExternalBrokenPartsIfNeeded(glass, towerParent, glassPrefab);
    }

    // A painted cell can spawn any palette object, so the broken parts must
    // be read from the prefab that was actually instantiated.
    private void CopyExternalBrokenPartsIfNeeded(
        BreakableGlass glass,
        Transform towerParent,
        BreakableGlass sourcePrefab)
    {
        if (sourcePrefab == null)
        {
            return;
        }

        GameObject brokenTemplate = sourcePrefab.brokenParts;

        if (brokenTemplate == null ||
            brokenTemplate == sourcePrefab.gameObject ||
            brokenTemplate.transform.IsChildOf(sourcePrefab.transform))
        {
            return;
        }

        GameObject brokenCopy = Instantiate(brokenTemplate, towerParent);
        brokenCopy.name = $"{glass.name}_BrokenParts";
        brokenCopy.transform.SetParent(glass.transform, false);
        brokenCopy.transform.localPosition =
            sourcePrefab.transform.InverseTransformPoint(brokenTemplate.transform.position);
        brokenCopy.transform.localRotation =
            Quaternion.Inverse(sourcePrefab.transform.rotation) *
            brokenTemplate.transform.rotation;
        brokenCopy.transform.localScale = GetRelativeScale(
            brokenTemplate.transform.lossyScale,
            sourcePrefab.transform.lossyScale);
        glass.brokenParts = brokenCopy;
    }

    private static Vector3 GetRelativeScale(Vector3 childScale, Vector3 parentScale)
    {
        return new Vector3(
            SafeDivide(childScale.x, parentScale.x),
            SafeDivide(childScale.y, parentScale.y),
            SafeDivide(childScale.z, parentScale.z));
    }

    private static float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) > 0.0001f ? value / divisor : value;
    }

    private void HideSceneTemplateIfNeeded()
    {
        if (!hideSceneTemplate ||
            glassPrefab == null ||
            !glassPrefab.gameObject.scene.IsValid())
        {
            return;
        }

        GameObject externalBrokenParts = glassPrefab.brokenParts;
        glassPrefab.gameObject.SetActive(false);

        if (externalBrokenParts != null &&
            externalBrokenParts != glassPrefab.gameObject &&
            !externalBrokenParts.transform.IsChildOf(glassPrefab.transform))
        {
            externalBrokenParts.SetActive(false);
        }
    }

    private void AssignSupports(List<List<BreakableGlass>> towerRows)
    {
        for (int row = 1; row < towerRows.Count; row++)
        {
            List<BreakableGlass> lowerRow = towerRows[row - 1];
            List<BreakableGlass> currentRow = towerRows[row];

            for (int column = 0; column < currentRow.Count; column++)
            {
                List<BreakableGlass> glassSupports = supports[currentRow[column]];

                if (lowerRow.Count == currentRow.Count + 1)
                {
                    glassSupports.Add(lowerRow[column]);
                    glassSupports.Add(lowerRow[column + 1]);
                }
                else
                {
                    glassSupports.Add(lowerRow[Mathf.Min(column, lowerRow.Count - 1)]);
                }
            }
        }
    }

    public void NotifyGlassShattered(BreakableGlass glass)
    {
        if (clearingTower || glass == null || !supports.ContainsKey(glass))
        {
            return;
        }

        if (collapseRoutine == null)
        {
            collapseRoutine = StartCoroutine(CollapseUnsupportedGlasses());
        }
    }

    private IEnumerator CollapseUnsupportedGlasses()
    {
        if (collapseStepDelay > 0f)
        {
            yield return new WaitForSeconds(collapseStepDelay);
        }
        else
        {
            yield return null;
        }

        while (true)
        {
            List<BreakableGlass> unsupported = FindUnsupportedGlasses();

            if (unsupported.Count == 0)
            {
                break;
            }

            for (int i = 0; i < unsupported.Count; i++)
            {
                BreakableGlass glass = unsupported[i];

                if (glass != null && glass.IsSupporting)
                {
                    glass.DropFromLostSupport();
                }
            }

            if (collapseStepDelay > 0f)
            {
                yield return new WaitForSeconds(collapseStepDelay);
            }
            else
            {
                yield return null;
            }
        }

        collapseRoutine = null;
    }

    private List<BreakableGlass> FindUnsupportedGlasses()
    {
        List<BreakableGlass> result = new List<BreakableGlass>();

        foreach (KeyValuePair<BreakableGlass, List<BreakableGlass>> entry in supports)
        {
            BreakableGlass glass = entry.Key;

            if (glass == null || !glass.IsSupporting || glass.TowerRow <= 0)
            {
                continue;
            }

            List<BreakableGlass> glassSupports = entry.Value;

            if (usePhysicalSupportCheck &&
                !glass.HasPhysicalSupport(
                    physicalSupportDistance,
                    physicalSupportProbeRadius,
                    physicalSupportMask))
            {
                result.Add(glass);
                continue;
            }

            bool allSupportsIntact = glassSupports.Count > 0;

            for (int i = 0; i < glassSupports.Count; i++)
            {
                BreakableGlass support = glassSupports[i];

                if (support == null || !support.IsSupporting)
                {
                    allSupportsIntact = false;
                    break;
                }
            }

            if (!allSupportsIntact)
            {
                result.Add(glass);
            }
        }

        return result;
    }

    [ContextMenu("Clear Tower (Play Mode)")]
    public void ClearTower()
    {
        if (collapseRoutine != null)
        {
            StopCoroutine(collapseRoutine);
            collapseRoutine = null;
        }

        clearingTower = true;
        supports.Clear();

        for (int i = generatedGlasses.Count - 1; i >= 0; i--)
        {
            BreakableGlass glass = generatedGlasses[i];

            if (glass != null)
            {
                Destroy(glass.gameObject);
            }
        }

        generatedGlasses.Clear();
        clearingTower = false;
    }
}
