//using UnityEngine;

//[CreateAssetMenu(
//    fileName = "PhysicsObjectDefinition",
//    menuName = "Royal Smash/Physics Object Definition")]
//public sealed class PhysicsObjectDefinition : ScriptableObject
//{
//    [Header("Prefab")]
//    [SerializeField]
//    private PhysicsTowerObject prefab;

//    [Header("Placement Size")]
//    [Tooltip("Object collider ka approximate X/Y/Z size.")]
//    [SerializeField]
//    private Vector3 placementSize =
//        new Vector3(0.38f, 0.55f, 0.38f);

//    [Header("Pool")]
//    [SerializeField, Min(0)]
//    private int minimumPrewarmCount = 16;

//    public PhysicsTowerObject Prefab => prefab;
//    public Vector3 PlacementSize => placementSize;
//    public int MinimumPrewarmCount => minimumPrewarmCount;
//}



using UnityEngine;

[CreateAssetMenu(
    fileName = "PhysicsObjectDefinition",
    menuName = "Royal Smash/Physics Object Definition")]
public sealed class PhysicsObjectDefinition : ScriptableObject
{
    [Header("Prefab")]

    [SerializeField]
    private PhysicsTowerObject prefab;

    [Header("Pool")]

    [Tooltip(
        "Level start par minimum kitne objects pool mein ready rakhein."
    )]
    [SerializeField, Min(0)]
    private int minimumPrewarmCount = 16;

    public PhysicsTowerObject Prefab =>
        prefab;

    public int MinimumPrewarmCount =>
        minimumPrewarmCount;
}




