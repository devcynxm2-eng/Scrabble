using UnityEngine;
using UnityEngine.Serialization;

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

    [Header("Break Behaviour")]

    [Tooltip(
        "Optional prefab containing the already-broken pieces. Each piece " +
        "should have its own Rigidbody and Collider. Empty uses the block " +
        "prefab's break visual or generated fallback shards."
    )]
    [SerializeField]
    private GameObject brokenPiecesPrefab;

    [Tooltip(
        "Optional individual piece prefabs. Assign Piece 1, Piece 2, etc. " +
        "The runtime combines them automatically and adds missing Rigidbody " +
        "or Collider components. This list takes priority over the combined " +
        "Broken Pieces Prefab above."
    )]
    [SerializeField]
    private GameObject[] brokenPiecePrefabs;

    [Tooltip(
        "ON: this object breaks when it receives a direct cannon-ball hit. " +
        "Useful for glass/ice boxes."
    )]
    [SerializeField]
    private bool breakOnCannonBallHit;

    [Tooltip(
        "ON: after this object has fallen from the tower, it breaks when " +
        "it reaches the lower ground."
    )]
    [SerializeField]
    private bool breakOnGroundImpact;

    [Tooltip(
        "Minimum collision speed required before a lower-ground impact " +
        "can break the object."
    )]
    [SerializeField, Min(0f)]
    private float minimumGroundBreakSpeed = 0.6f;

    [Tooltip("Outward force applied to the broken pieces.")]
    [SerializeField, Min(0f)]
    private float brokenPiecesImpulse = 1.4f;

    [Tooltip("Broken pieces are removed after this many seconds.")]
    [SerializeField, Min(0.1f)]
    private float brokenPiecesLifetime = 2.5f;

    [Tooltip(
        "Extra scale correction for the broken prefab. Leave at (1,1,1) " +
        "when its root matches the intact object."
    )]
    [SerializeField]
    private Vector3 brokenPiecesScaleMultiplier = Vector3.one;

    [Header("Glass Shatter Style")]

    [Tooltip(
        "ON creates several small randomized copies from the supplied piece " +
        "prefabs, producing a sharp glass-like shatter instead of a few large chunks."
    )]
    [SerializeField]
    private bool useGlassShatterStyle;

    [Tooltip("How many small shards to create from every supplied piece prefab.")]
    [SerializeField, Range(1, 5)]
    private int glassCopiesPerPiece = 3;

    [Tooltip("Random size range used for glass shards.")]
    [SerializeField]
    private Vector2 glassPieceScaleRange = new Vector2(0.22f, 0.38f);

    [Tooltip("Small starting spread around the exact break position.")]
    [SerializeField, Min(0f)]
    private float glassSpawnSpread = 0.16f;

    [Tooltip("Extra upward kick added to the glass burst.")]
    [SerializeField, Min(0f)]
    private float glassUpwardImpulse = 0.35f;

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
    [FormerlySerializedAs("chainReactionVfxPrefab")]
    [SerializeField]
    private ParticleSystem chainReactionParticlePrefab;

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

    public GameObject BrokenPiecesPrefab =>
        brokenPiecesPrefab;

    public GameObject[] BrokenPiecePrefabs =>
        brokenPiecePrefabs;

    public bool BreakOnCannonBallHit =>
        breakOnCannonBallHit;

    public bool BreakOnGroundImpact =>
        breakOnGroundImpact;

    public float MinimumGroundBreakSpeed =>
        minimumGroundBreakSpeed;

    public float BrokenPiecesImpulse =>
        brokenPiecesImpulse;

    public float BrokenPiecesLifetime =>
        brokenPiecesLifetime;

    public Vector3 BrokenPiecesScaleMultiplier =>
        brokenPiecesScaleMultiplier;

    public bool UseGlassShatterStyle =>
        useGlassShatterStyle;

    public int GlassCopiesPerPiece =>
        glassCopiesPerPiece;

    public Vector2 GlassPieceScaleRange =>
        glassPieceScaleRange;

    public float GlassSpawnSpread =>
        glassSpawnSpread;

    public float GlassUpwardImpulse =>
        glassUpwardImpulse;

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

    public ParticleSystem ChainReactionParticlePrefab =>
        chainReactionParticlePrefab;

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
