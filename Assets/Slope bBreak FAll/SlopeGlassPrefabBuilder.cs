#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Scene ke maujooda breakable jar se self-contained BreakableGlass
/// prefabs banata hai, taake level editor ke Object Palette mein
/// alag alag jars use kiye ja sakein.
///
/// Palette ek ScriptableObject asset par hai, aur ScriptableObject
/// kisi scene object ka reference save nahi kar sakta. Is liye
/// palette ko asli prefabs chahiye — yeh utility wohi banata hai.
///
/// Scene wale jar ki broken parts uska sibling hai, child nahi. Seedha
/// prefab banane par wo reference toot jata. Yahan broken parts ko
/// pehle jar ke neeche laaya jata hai (wahi offset jo runtime par
/// GlassTowerController nikalta hai), phir prefab save hota hai — is
/// tarah prefab mein tootne wale tukre bhi sath aate hain.
/// </summary>
public static class SlopeGlassPrefabBuilder
{
    private const string OutputFolder =
        "Assets/Slope bBreak FAll/Glass Prefabs";

    private const string MaterialFolder =
        "Assets/Canon Controle/Material";

    /*
     * Tootne wale shard patle aur mure hue hain, magar unka BoxCollider
     * poore mur ke gird ek dabba hai — naapa to box asli shishe se 6.6
     * guna bara nikla, aur har box 65-88% khali. Do jars ke shard ek
     * doosre ke khali dabbon mein ghus kar paida hote hain, aur physics
     * unhein zor se alag dhakelti hai. Wohi "blast" hai.
     *
     * Do cheezein iska ilaj karti hain:
     *   1. shard ko convex MeshCollider, jo asli shape ke qareeb hai
     *   2. shard ko apni layer, jis ki aapas mein takkar band hai —
     *      tootay hue tukre ek doosre ko dhakelte hi nahi
     *
     * BreakableGlass.cs ko is mein bilkul haath nahi lagta.
     */
    private const string DebrisLayerName = "GlassDebris";


    /*
     * Support tolerance — poora jar ek hi hit mein khatam hone ka ilaaj.
     *
     * Jar ke lower body ke teen wedges (Cube, Cube (1), Cube (3)) barabar
     * oonchai par saath saath khare hain; unke centre sirf 4-5 mm alag
     * hain. Default 0.12 par BreakableGlass ka support map unhen ek doosre
     * ke OOPAR khara samajh leta hai, aur ek zanjeer ban jati hai:
     *
     *   Cube  <-  Cube (3)  <-  Cube (1)  <-  Cube (4)  <-  gardan  <-  kinara
     *
     * Ball jar ke jism par kahin bhi lage to sabse pehle Cube nikalta hai,
     * aur poori zanjeer bereham hisaab se gir jati hai — 7 mein se 6 tukre,
     * yani poora jar. (Napa gaya: y = -1.02 se -0.03 tak har hit par 6/7.)
     *
     * 0.45 par teenon wedges ka support neeche wali disc ban jati hai, is
     * liye ek wedge nikalne par baqi jar khara rehta hai — jar mein sirf
     * ek soorakh hota hai. 0.55 se ooper jane par gardan ka support bhi
     * neeche ke wedges ban jate hain aur upper body girne ke baad gardan
     * hawa mein latak jati hai, is liye value beech mein rakhi gayi hai.
     *
     * Ye BreakableGlass ka apna serialized field hai — script ko haath
     * nahi lagta, sirf in prefabs ki tuning badalti hai. Scene ka asli
     * template waise ka waisa hai.
     */
    private const float SupportLevelTolerance = 0.45f;


    /*
     * Sirf wohi colours jin ki material asset mojood hai. Geometry
     * aur fragments wahi rehte hain jo scene mein pehle se chal rahe
     * hain, is liye tootna bilkul waise hi hota hai jaise abhi hota
     * hai — sirf rang badalta hai.
     */
    private static readonly string[] ColourNames =
    {
        "Red",
        "Blue",
        "Orange",
        "Pink",
        "Purple",
        "Yellow"
    };


    [MenuItem("Tools/Slope Ball/Build Glass Jar Prefabs")]
    public static void BuildGlassJarPrefabs()
    {
        BreakableGlass source = FindSourceGlass();

        if (source == null)
        {
            EditorUtility.DisplayDialog(
                "Slope Ball",
                "Scene mein koi BreakableGlass nahi mila.\n\n" +
                "Canon_Controller scene kholein (jis mein " +
                "SlopeBallGame root hai) aur dobara chalayein.",
                "OK"
            );

            return;
        }


        if (source.brokenParts == null)
        {
            EditorUtility.DisplayDialog(
                "Slope Ball",
                $"'{source.name}' par Broken Parts assigned nahi hai, " +
                "is liye tootne wale tukron ke baghair prefab banana " +
                "theek nahi hoga.",
                "OK"
            );

            return;
        }


        EnsureOutputFolder();

        int debrisLayer = EnsureDebrisLayer();


        /*
         * Broken parts ka jar ke relative offset — bilkul wahi hisaab
         * jo GlassTowerController.CopyExternalBrokenPartsIfNeeded
         * runtime par karta hai.
         */
        Vector3 brokenLocalPosition =
            source.transform.InverseTransformPoint(
                source.brokenParts.transform.position
            );

        Quaternion brokenLocalRotation =
            Quaternion.Inverse(source.transform.rotation) *
            source.brokenParts.transform.rotation;

        Vector3 brokenLocalScale = GetRelativeScale(
            source.brokenParts.transform.lossyScale,
            source.transform.lossyScale
        );


        List<string> created = new List<string>();


        bool allColoursReady = true;

        for (int colourIndex = 0;
             colourIndex < ColourNames.Length;
             colourIndex++)
        {
            string colour = ColourNames[colourIndex];

            Material material = LoadMaterial(colour);

            if (material == null)
            {
                Debug.LogWarning(
                    $"Material JamJar_{colour} nahi mila — skip."
                );

                continue;
            }


            string path =
                $"{OutputFolder}/BreakableJar_{colour}.prefab";

            BuildOne(
                source,
                material,
                colourIndex,
                debrisLayer,
                $"BreakableJar_{colour}",
                path,
                brokenLocalPosition,
                brokenLocalRotation,
                brokenLocalScale,
                ref allColoursReady
            );

            created.Add(path);
        }


        if (!allColoursReady)
        {
            Debug.LogWarning(
                "Kuch material previews abhi ban rahe thay, is liye un " +
                "jars ka liquid colour fallback par hai. Thori der baad " +
                "ye menu dobara chalayein taake asli rang bake ho jaye."
            );
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


        Debug.Log(
            $"Slope Ball: {created.Count} breakable jar prefabs " +
            $"ban gaye ({OutputFolder}). Inhein level ke Object " +
            "Palette mein daal kar alag alag jars place kar sakte hain."
        );
    }


    private static void BuildOne(
        BreakableGlass source,
        Material material,
        int colourIndex,
        int debrisLayer,
        string prefabName,
        string path,
        Vector3 brokenLocalPosition,
        Quaternion brokenLocalRotation,
        Vector3 brokenLocalScale,
        ref bool allColoursReady)
    {
        GameObject root =
            Object.Instantiate(source.gameObject);

        root.name = prefabName;
        root.transform.SetPositionAndRotation(
            Vector3.zero,
            Quaternion.identity
        );
        root.transform.localScale =
            source.transform.localScale;
        root.SetActive(true);


        GameObject broken =
            Object.Instantiate(source.brokenParts);

        broken.name = "BrokenParts";
        broken.transform.SetParent(root.transform, false);
        broken.transform.localPosition = brokenLocalPosition;
        broken.transform.localRotation = brokenLocalRotation;
        broken.transform.localScale = brokenLocalScale;
        broken.SetActive(true);


        BreakableGlass glass =
            root.GetComponent<BreakableGlass>();

        /*
         * Dono references ab prefab ke andar hi point karte hain,
         * scene par nahi — isi se prefab self-contained banta hai.
         */
        SerializedObject so = new SerializedObject(glass);
        so.FindProperty("intactObject").objectReferenceValue = root;
        so.FindProperty("brokenParts").objectReferenceValue = broken;
        so.FindProperty("supportLevelTolerance").floatValue =
            SupportLevelTolerance;
        so.ApplyModifiedPropertiesWithoutUndo();


        ApplyMaterial(root, material);

        ApplyJarCollider(root);

        ApplyShardColliders(root, glass, debrisLayer);

        BakeLiquidColour(root, glass, colourIndex, ref allColoursReady);


        PrefabUtility.SaveAsPrefabAsset(root, path);

        Object.DestroyImmediate(root);
    }


    /// <summary>
    /// Jar ke andar ke liquid ka rang prefab mein likh deta hai.
    ///
    /// Runtime par texture parh kar rang nikalna mobile par faltu
    /// kharcha hai, is liye yeh kaam yahin editor mein ho jata hai.
    /// Wahi utility level editor ka grid bhi use karta hai, to grid ka
    /// rang aur girne wale paani ka rang hamesha match karte hain.
    /// </summary>
    private static void BakeLiquidColour(
        GameObject root,
        BreakableGlass glass,
        int colourIndex,
        ref bool allColoursReady)
    {
        SlopeGlassLiquid liquid =
            root.GetComponent<SlopeGlassLiquid>();

        if (liquid == null)
        {
            liquid = root.AddComponent<SlopeGlassLiquid>();
        }


        Color colour = SlopeGlassColorUtility.GetObjectColor(
            glass,
            colourIndex,
            out bool ready
        );

        if (!ready)
        {
            allColoursReady = false;
        }


        SerializedObject so = new SerializedObject(liquid);
        so.FindProperty("liquidColor").colorValue = colour;
        so.ApplyModifiedPropertiesWithoutUndo();
    }


    /// <summary>
    /// Sabit jar ko uski apni gol shape ka convex MeshCollider deta hai.
    ///
    /// Jar dekhne mein bela (cylinder) hai — uska radius 0.490 hai. Magar
    /// collider ek CHAKOR box tha, jiska corner markaz se 0.693 door
    /// jata hai: yani shishe se 0.203 meter (41%) bahar, hawa mein.
    ///
    /// Yehi wajah thi ke girti hui jar deewar se "taka laga kar" ruk jati
    /// thi. Ruk wo shishe par nahi rahi thi — wo apne ghaib box ke nukeele
    /// kone par tiki hoti thi. Box ka chapta pehlu khari deewar se poora
    /// chipak jata hai aur neeche ka kona neeche wale tukre mein dhans
    /// jata hai, to takra bilkul mazboot ban jata hai. Asli gol jar aise
    /// nahi ruk sakti — wo phisal ya lurhak kar gir jati.
    ///
    /// Ab collider jar ki apni shakal ka hai, is liye jo nazar aata hai
    /// wohi takrata hai. AABB dono ka bilkul barabar (0.981 x 1.982 x
    /// 0.981) hai, is liye tower ki chunai aur qatarein waise ki waise
    /// rehti hain — sirf takkar ki shakal durust hoti hai.
    ///
    /// convex isi liye ke girti hui poori jar par runtime par Rigidbody
    /// lagti hai; BreakableGlass ka SetWholeGlassGravity khud bhi sabit
    /// jar ke MeshCollider ko convex karta hai, yani ye wahi surat hai
    /// jiski wo tawaqqo rakhta hai.
    /// </summary>
    private static void ApplyJarCollider(GameObject root)
    {
        MeshFilter filter = root.GetComponent<MeshFilter>();

        if (filter == null || filter.sharedMesh == null)
        {
            return;
        }


        BoxCollider[] boxes = root.GetComponents<BoxCollider>();

        for (int i = 0; i < boxes.Length; i++)
        {
            Object.DestroyImmediate(boxes[i]);
        }


        MeshCollider collider = root.GetComponent<MeshCollider>();

        if (collider == null)
        {
            collider = root.AddComponent<MeshCollider>();
        }

        collider.sharedMesh = filter.sharedMesh;
        collider.convex = true;
    }


    /// <summary>
    /// Har shard ko uski apni shape ka convex MeshCollider deta hai aur
    /// usay debris layer par le aata hai.
    ///
    /// Sabit jar ka collider chhua nahi jata — wo scene ke original
    /// jaisa hi rehta hai.
    /// </summary>
    private static void ApplyShardColliders(
        GameObject root,
        BreakableGlass glass,
        int debrisLayer)
    {
        if (glass == null || glass.brokenParts == null)
        {
            return;
        }


        foreach (Transform shard in glass.brokenParts.transform)
        {
            MeshFilter filter = shard.GetComponent<MeshFilter>();

            if (filter == null || filter.sharedMesh == null)
            {
                continue;
            }


            BoxCollider[] boxes = shard.GetComponents<BoxCollider>();

            for (int i = 0; i < boxes.Length; i++)
            {
                Object.DestroyImmediate(boxes[i]);
            }


            MeshCollider collider = shard.GetComponent<MeshCollider>();

            if (collider == null)
            {
                collider = shard.gameObject.AddComponent<MeshCollider>();
            }


            collider.sharedMesh = filter.sharedMesh;

            // Rigidbody ke sath non-convex mesh collider chalta nahi
            collider.convex = true;

            shard.gameObject.layer = debrisLayer;


            /*
             * Gira hua tukra kuch second baad khud ghayab ho jata hai.
             * Zameen par pare rehne se na gameplay ko kuch milta hai na
             * nazar ko — sirf rigidbody aur collider ka bojh barhta hai.
             */
            if (shard.GetComponent<SlopeShardCleanup>() == null)
            {
                shard.gameObject.AddComponent<SlopeShardCleanup>();
            }
        }
    }


    /// <summary>
    /// Shards ke liye alag layer, jis ki apne aap se takkar band ho.
    ///
    /// Yehi wo cheez hai jo blast ko jar se khatam karti hai: tukre
    /// zameen aur jars se to takrate hain, magar ek doosre se nahi.
    /// Project ki har raycast mask ~0 hai, is liye nayi layer se kisi
    /// maujooda cheez par asar nahi parta.
    /// </summary>
    private static int EnsureDebrisLayer()
    {
        int layer = LayerMask.NameToLayer(DebrisLayerName);

        if (layer < 0)
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath(
                    "ProjectSettings/TagManager.asset"
                )[0]
            );

            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty entry = layers.GetArrayElementAtIndex(i);

                if (string.IsNullOrEmpty(entry.stringValue))
                {
                    entry.stringValue = DebrisLayerName;
                    tagManager.ApplyModifiedProperties();
                    layer = i;

                    Debug.Log($"Layer {i} = {DebrisLayerName} bana di gayi.");

                    break;
                }
            }
        }


        if (layer < 0)
        {
            Debug.LogWarning("Koi khali layer nahi mili.");

            return 0;
        }


        DisableSelfCollision(layer);

        return layer;
    }


    /// <summary>
    /// Debris layer ka collision matrix set karta hai: har cheez se
    /// takkar on, sirf apne aap se off.
    ///
    /// Nayi layer ka matrix row 0 aata hai — wo KISI cheez se nahi
    /// takrati, zameen se bhi nahi. Is liye poora row set karna parta
    /// hai. DynamicsManager.asset ko SerializedObject se likhna kaam
    /// nahi karta, magar Physics.IgnoreLayerCollision editor mein
    /// seedha project settings mein likh deta hai.
    /// </summary>
    private static void DisableSelfCollision(int layer)
    {
        for (int other = 0; other < 32; other++)
        {
            bool ignore = other == layer;

            if (Physics.GetIgnoreLayerCollision(layer, other) != ignore)
            {
                Physics.IgnoreLayerCollision(layer, other, ignore);
            }
        }


        Debug.Log(
            $"Physics matrix: layer {layer} har cheez se takrayegi, " +
            "sirf apne aap se nahi."
        );
    }


    /// <summary>
    /// Jar aur uske tootne wale tukron, dono par ek hi rang lagta hai
    /// warna tootne par colour badal jata.
    /// </summary>
    private static void ApplyMaterial(
        GameObject root,
        Material material)
    {
        foreach (MeshRenderer renderer in
                 root.GetComponentsInChildren<MeshRenderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }


    /// <summary>
    /// Template hamesha SCENE ka jar hona chahiye.
    ///
    /// FindObjectsByType un prefab assets ko bhi laut sakta hai jo
    /// pehle se memory mein load hain — yani pehle se banaya hua
    /// BreakableJar prefab. Us se template ke taur par prefab chun
    /// liya jata tha, jis mein BrokenParts pehle se child hoti hai,
    /// aur nayi prefab mein do BrokenParts chali jati theen.
    /// </summary>
    private static BreakableGlass FindSourceGlass()
    {
        BreakableGlass[] candidates =
            Object.FindObjectsByType<BreakableGlass>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        BreakableGlass fallback = null;

        for (int i = 0; i < candidates.Length; i++)
        {
            BreakableGlass candidate = candidates[i];

            if (candidate == null ||
                EditorUtility.IsPersistent(candidate) ||
                !candidate.gameObject.scene.IsValid())
            {
                continue;
            }


            /*
             * Template wo hai jiski broken parts alag padi hain. Agar
             * broken parts pehle se child hai to wo hamara apna banaya
             * hua prefab ka instance hai, template nahi.
             */
            if (candidate.brokenParts != null &&
                !candidate.brokenParts.transform.IsChildOf(candidate.transform))
            {
                return candidate;
            }


            if (fallback == null)
            {
                fallback = candidate;
            }
        }


        return fallback;
    }


    private static Material LoadMaterial(
        string colour)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(
            $"{MaterialFolder}/JamJar_{colour}.mat"
        );
    }


    private static void EnsureOutputFolder()
    {
        if (AssetDatabase.IsValidFolder(OutputFolder))
        {
            return;
        }


        Directory.CreateDirectory(OutputFolder);
        AssetDatabase.Refresh();
    }


    private static Vector3 GetRelativeScale(
        Vector3 childScale,
        Vector3 parentScale)
    {
        return new Vector3(
            SafeDivide(childScale.x, parentScale.x),
            SafeDivide(childScale.y, parentScale.y),
            SafeDivide(childScale.z, parentScale.z)
        );
    }


    private static float SafeDivide(
        float value,
        float divisor)
    {
        return Mathf.Abs(divisor) > 0.0001f
            ? value / divisor
            : value;
    }
}
#endif
