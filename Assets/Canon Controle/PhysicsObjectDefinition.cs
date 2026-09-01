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

    [Header("Chain Reaction")]

    /*
     * Special "chain reaction" block.
     *
     * Ye poori settings SIRF un definitions par ON hoti hain jo special
     * block ke liye banayi gayi hon. Purani definitions mein ye fields
     * default (OFF) rehti hain, is liye maujooda levels ka behaviour
     * bilkul nahi badalta.
     */
    [Tooltip(
        "ON: is block par cannonball lagte hi aas paas ke blocks par " +
        "blast hoga. Poora tower nahi girta — sirf radius ke andar ke " +
        "chand blocks urte hain."
    )]
    [SerializeField]
    private bool chainReactionEnabled = false;

    [Tooltip("Blast ka radius world units mein.")]
    [SerializeField, Min(0f)]
    private float chainReactionRadius = 1.2f;

    [Tooltip(
        "Ek blast mein zyada se zyada kitne blocks affect honge. " +
        "Yehi cap poore tower ko girne se rokta hai — qareeb tareen " +
        "blocks ko tarjeeh milti hai."
    )]
    [SerializeField, Min(1)]
    private int chainReactionMaxBlocks = 6;

    [Tooltip("Har affected block ko milne wala impulse.")]
    [SerializeField, Min(0f)]
    private float chainReactionImpulse = 6f;

    [Tooltip(
        "Blast ko thora upar ki taraf dhakelta hai taake blocks " +
        "sirf side mein khisakne ke bajaye uchhal kar giren."
    )]
    [SerializeField, Range(0f, 1f)]
    private float chainReactionUpwardBias = 0.35f;

    [Tooltip(
        "Electric / explosion particle prefab. Blast ki jagah par " +
        "spawn hoga. Khali chhorna bhi theek hai."
    )]
    [SerializeField]
    private GameObject chainReactionVfxPrefab;

    [Tooltip("VFX kitne seconds baad khud destroy ho jayega.")]
    [SerializeField, Min(0.1f)]
    private float chainReactionVfxLifetime = 2f;

    [Tooltip(
        "ON: agar blast ki zad mein koi doosra special block aaye to " +
        "wo bhi phatega. Is se multi-stage chain banti hai. Har block " +
        "sirf ek baar phat sakta hai, is liye infinite loop nahi banta."
    )]
    [SerializeField]
    private bool chainReactionPropagates = true;

    [Tooltip(
        "Agla special block kitni der baad phatega. YEHI wo cheez hai jo " +
        "isay 'chain reaction' banati hai — 0 rakhne par poori chain ek " +
        "hi frame mein chal jati hai aur ek bara dhamaka lagta hai.\n\n" +
        "0.10 - 0.20 ke darmiyan achha lagta hai."
    )]
    [SerializeField, Min(0f)]
    private float chainReactionPropagationDelay = 0.12f;


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

    public bool ChainReactionEnabled =>
        chainReactionEnabled;

    public float ChainReactionRadius =>
        chainReactionRadius;

    public int ChainReactionMaxBlocks =>
        chainReactionMaxBlocks;

    public float ChainReactionImpulse =>
        chainReactionImpulse;

    public float ChainReactionUpwardBias =>
        chainReactionUpwardBias;

    public GameObject ChainReactionVfxPrefab =>
        chainReactionVfxPrefab;

    public float ChainReactionVfxLifetime =>
        chainReactionVfxLifetime;

    public bool ChainReactionPropagates =>
        chainReactionPropagates;

    public float ChainReactionPropagationDelay =>
        chainReactionPropagationDelay;

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
