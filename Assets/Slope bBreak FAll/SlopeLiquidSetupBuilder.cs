#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SlopeLiquidSetupBuilder
{
    private const string GroundLayerName = "SlopeGround";

    [MenuItem("Tools/Slope Ball/Build Liquid Spill Setup")]
    public static void BuildLiquidSetup()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Slope bBreak FAll/Shaders/SlopeClearWater.mat");
        if (material == null)
        {
            Debug.LogError("The Clear Water material is missing.");
            return;
        }
        int groundLayer = EnsureGroundLayer();
        AssignGroundLayerToSlopeSurfaces(groundLayer);
        GameObject root = new GameObject("LiquidSpill");
        try
        {
            var spiller = root.AddComponent<SlopeLiquidSpiller>();
            var serialized = new SerializedObject(spiller);
            serialized.FindProperty("waterMaterial").objectReferenceValue = material;
            serialized.FindProperty("groundMask").intValue = 1 << groundLayer;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Slope bBreak FAll/LiquidSpill.prefab");
            AssetDatabase.SaveAssets();
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
        Debug.Log("Clear Water spill prefab rebuilt.");
    }
    private static int EnsureGroundLayer()
    {
        int existing = LayerMask.NameToLayer(GroundLayerName);

        if (existing >= 0)
        {
            return existing;
        }


        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath(
                "ProjectSettings/TagManager.asset"
            )[0]
        );

        SerializedProperty layers = tagManager.FindProperty("layers");

        // 0-7 Unity ki apni hain, user layers 8 se shuru hoti hain
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);

            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = GroundLayerName;
                tagManager.ApplyModifiedProperties();

                Debug.Log($"Layer {i} = {GroundLayerName} bana di gayi.");

                return i;
            }
        }


        Debug.LogWarning(
            "Koi khali layer nahi mili — droplets har cheez se takrayenge."
        );

        return 0;
    }


    /// <summary>
    /// Slope ke ground surfaces ko us layer par le aata hai.
    ///
    /// Ground wo hain jo SlopeBallGame ke neeche hain, collider rakhte
    /// hain, aur na khud glass hain na kisi glass ka hissa.
    /// </summary>
    private static void AssignGroundLayerToSlopeSurfaces(int groundLayer)
    {
        GameObject slopeRoot = null;

        foreach (GameObject root in
                 UnityEditor.SceneManagement.EditorSceneManager
                     .GetActiveScene().GetRootGameObjects())
        {
            if (root.name == "SlopeBallGame")
            {
                slopeRoot = root;
            }
        }

        if (slopeRoot == null)
        {
            return;
        }


        int moved = 0;

        foreach (Transform child in slopeRoot.transform)
        {
            if (!child.name.StartsWith("Plane"))
            {
                continue;
            }

            if (child.GetComponent<Collider>() == null ||
                child.GetComponentInParent<BreakableGlass>() != null)
            {
                continue;
            }

            if (child.gameObject.layer != groundLayer)
            {
                child.gameObject.layer = groundLayer;
                moved++;
            }
        }


        if (moved > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log($"{moved} ground surfaces '{GroundLayerName}' layer par aa gayin.");
        }
    }


}
#endif
