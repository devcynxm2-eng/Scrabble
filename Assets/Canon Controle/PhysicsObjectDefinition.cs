using UnityEngine;

[CreateAssetMenu(
    fileName = "PhysicsObjectDefinition",
    menuName = "Royal Smash/Physics Object Definition")]
public sealed class PhysicsObjectDefinition : ScriptableObject
{
    [Header("Prefab")]

    [SerializeField]
    private PhysicsTowerObject prefab;

    [Tooltip(
        "Palette/UI mein dikhane ke liye. Empty ho to asset name use hoga."
    )]
    [SerializeField]
    private string displayName;

    [Header("Visual")]

    [Tooltip(
        "ON applies the level paint color as a tint. Leave OFF to " +
        "preserve the prefab Renderer material and texture exactly."
    )]
    [SerializeField]
    private bool tintWithPaintColor = false;

    [Header("Pool")]

    [Tooltip(
        "Level start par minimum kitne objects pool mein ready rakhein."
    )]
    [SerializeField, Min(0)]
    private int minimumPrewarmCount = 16;

    [Header("Grid Fit")]

    [Tooltip(
        "ON hone par is shape ko GridLevelData ki Cell Size ke andar " +
        "automatically scale kiya jayega (mixed shapes ek uniform grid " +
        "mein clean fit hote hain). OFF ho to prefab ki authored scale " +
        "as-is use hogi (decoration pieces jo cell se chota/bara honi " +
        "chahiye, jaise corner toppers)."
    )]
    [SerializeField]
    private bool autoFitToCell = true;

    [Tooltip(
        "Fit hone ke baad extra multiplier. Default (1,1,1)."
    )]
    [SerializeField]
    private Vector3 manualScaleMultiplier =
        Vector3.one;

    public PhysicsTowerObject Prefab =>
        prefab;

    public string DisplayName =>
        string.IsNullOrEmpty(displayName)
            ? name
            : displayName;

    public int MinimumPrewarmCount =>
        minimumPrewarmCount;

    public bool TintWithPaintColor =>
        tintWithPaintColor;

    public bool AutoFitToCell =>
        autoFitToCell;

    public Vector3 ManualScaleMultiplier =>
        manualScaleMultiplier;

#if UNITY_EDITOR
    /// <summary>
    /// Used by GridLevelDataEditor when a prefab is added through the
    /// quick shape setup UI. Runtime code still reads this asset normally.
    /// </summary>
    public void EditorConfigure(
        PhysicsTowerObject prefabValue,
        string displayNameValue)
    {
        prefab = prefabValue;
        displayName = displayNameValue;
    }
#endif
}
