#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Liquid spill ka poora setup banata hai: material, particle system,
/// aur scene mein spiller.
///
/// Sab kuch mobile ko dhyan mein rakh kar tune kiya gaya hai — particle
/// count kam, shadows band, aur ek hi material taake sab kuch ek batch
/// mein jaye.
/// </summary>
public static class SlopeLiquidSetupBuilder
{
    private const string ShaderName = "Slope Ball/Liquid";

    private const string WaterShaderName = "Slope Ball/Water Puddle";

    private const string FallShaderName = "Slope Ball/Water Fall";

    private const string DropletMaterialPath =
        "Assets/Slope bBreak FAll/Shaders/SlopeLiquid.mat";

    private const string PuddleMaterialPath =
        "Assets/Slope bBreak FAll/Shaders/SlopeLiquidPuddle.mat";

    private const string FallMaterialPath =
        "Assets/Slope bBreak FAll/Shaders/SlopeWaterFall.mat";

    private const string PrefabPath =
        "Assets/Slope bBreak FAll/LiquidSpill.prefab";

    /*
     * Droplets sirf zameen/slope se takrate hain, jars se nahi.
     *
     * Pehle wo har cheez se takrate thay. Jar tootte hi droplet uske
     * apne tukron se takra jata tha, jahan lifetimeLoss uski zindagi
     * kha jata aur minKillSpeed usay maar deta — zameen tak pohanchta
     * hi nahi tha. Aur splat bhi pehle takrao par banta tha, yani jar
     * par, zameen par nahi.
     */
    private const string GroundLayerName = "SlopeGround";


    [MenuItem("Tools/Slope Ball/Build Liquid Spill Setup")]
    public static void BuildLiquidSetup()
    {
        int groundLayer = EnsureGroundLayer();
        AssignGroundLayerToSlopeSurfaces(groundLayer);

        Material dropletMaterial = GetOrCreateMaterial(
            DropletMaterialPath,
            isPuddle: false
        );

        Material puddleMaterial = GetOrCreateWaterMaterial();
        Material fallMaterial = GetOrCreateFallMaterial();

        if (dropletMaterial == null || puddleMaterial == null)
        {
            EditorUtility.DisplayDialog(
                "Slope Ball",
                $"Shader '{ShaderName}' nahi mila.",
                "OK"
            );

            return;
        }


        GameObject root = new GameObject("LiquidSpill");

        ParticleSystem splat = BuildSplatSystem(puddleMaterial);
        splat.transform.SetParent(root.transform, false);

        ParticleSystem fall = BuildFallSystem(fallMaterial);
        fall.transform.SetParent(root.transform, false);

        ParticleSystem droplets = BuildDropletSystem(dropletMaterial, groundLayer);
        droplets.transform.SetParent(root.transform, false);
        droplets.transform.SetAsFirstSibling();

        SlopeLiquidSpiller spiller =
            root.AddComponent<SlopeLiquidSpiller>();

        SerializedObject so = new SerializedObject(spiller);
        so.FindProperty("dropletSystem").objectReferenceValue = droplets;
        so.FindProperty("puddleSystem").objectReferenceValue = splat;
        so.FindProperty("fallSystem").objectReferenceValue = fall;
        so.FindProperty("groundMask").intValue = 1 << groundLayer;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"Slope Ball: liquid spill setup ban gaya ({PrefabPath}). " +
            "Isay SlopeBallGame ke neeche scene mein rakhein."
        );
    }


    /// <summary>
    /// Zameen ke liye alag layer banata hai (agar pehle se na ho).
    /// Project ki har raycast mask ~0 (sab layers) hai, is liye nayi
    /// layer se kisi maujooda cheez par asar nahi parta.
    /// </summary>
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


    /// <summary>
    /// Droplet aur puddle, dono ek hi shader use karte hain magar
    /// alag tuning ke sath — girta hua qatra chhota aur chamakdaar,
    /// zameen par phaila paani zyada shaffaf aur be-tarteeb kinare wala.
    /// </summary>
    /// <summary>
    /// Zameen par behte hue paani ka material. Ye alag shader use karta
    /// hai jis mein satah harkat karti hai — yehi cheez isay "paani"
    /// banati hai, warna wo ek rangeen dhabba hi rehta hai.
    /// </summary>
    /// <summary>
    /// Jar se zameen tak girti hui dhaar. Ye ek juri hui satah hai —
    /// isi ki kami thi, jis se paani "girta hua" nahi lagta tha.
    /// </summary>
    private static ParticleSystem BuildFallSystem(Material material)
    {
        GameObject go = new GameObject("WaterFall");
        ParticleSystem system = go.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = system.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startSpeed = 0f;
        main.startLifetime = 0.8f;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 60;

        // Chaudai aur lambai alag alag set hoti hain, is liye 3D size
        main.startSize3D = true;
        main.startSizeX = 0.35f;
        main.startSizeY = 1f;
        main.startSizeZ = 1f;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = false;

        ParticleSystemRenderer renderer = ConfigureRenderer(
            system,
            material,
            ParticleSystemRenderMode.VerticalBillboard
        );

        /*
         * Particle ki umar shader tak TEXCOORD0.z mein pohanchti hai.
         * Wahi dhaar ke neeche girne ko chalati hai — koi script har
         * frame material ko haath nahi lagati, is liye saari dhaarein
         * ek hi draw call mein rehti hain.
         */
        var streams = new System.Collections.Generic.List<
            ParticleSystemVertexStream>();

        renderer.GetActiveVertexStreams(streams);

        if (!streams.Contains(ParticleSystemVertexStream.AgePercent))
        {
            streams.Add(ParticleSystemVertexStream.AgePercent);
        }

        renderer.SetActiveVertexStreams(streams);

        return system;
    }


    private static Material GetOrCreateFallMaterial()
    {
        Shader shader = Shader.Find(FallShaderName);

        if (shader == null)
        {
            return null;
        }


        Material material =
            AssetDatabase.LoadAssetAtPath<Material>(FallMaterialPath);

        if (material == null)
        {
            material = new Material(shader);

            Directory.CreateDirectory(
                Path.GetDirectoryName(FallMaterialPath)
            );

            AssetDatabase.CreateAsset(material, FallMaterialPath);
        }
        else
        {
            material.shader = shader;
        }


        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_Width", 0.92f);
        material.SetFloat("_Neck", 0.22f);
        material.SetFloat("_HeadBulge", 0.40f);
        material.SetFloat("_EdgeSoft", 0.06f);
        material.SetFloat("_FlowSpeed", 2.4f);
        material.SetFloat("_WaveStrength", 0.30f);
        material.SetFloat("_WaveScale", 7f);
        material.SetFloat("_FrontSpeed", 1.25f);
        material.SetFloat("_RefractionStrength", 0.018f);
        material.SetFloat("_Tinting", 0.80f);
        material.SetFloat("_BodyAlpha", 0.95f);
        material.SetFloat("_RimBoost", 1.3f);
        material.SetFloat("_Glint", 0.4f);

        EditorUtility.SetDirty(material);

        return material;
    }


    private static Material GetOrCreateWaterMaterial()
    {
        Shader shader = Shader.Find(WaterShaderName);

        if (shader == null)
        {
            return null;
        }


        Material material =
            AssetDatabase.LoadAssetAtPath<Material>(PuddleMaterialPath);

        if (material == null)
        {
            material = new Material(shader);

            Directory.CreateDirectory(
                Path.GetDirectoryName(PuddleMaterialPath)
            );

            AssetDatabase.CreateAsset(material, PuddleMaterialPath);
        }
        else
        {
            material.shader = shader;
        }


        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_Softness", 0.22f);
        material.SetFloat("_EdgeWobble", 0.10f);
        material.SetFloat("_WobbleFrequency", 9f);
        material.SetFloat("_FlowSpeed", 1.4f);
        material.SetFloat("_RippleStrength", 0.55f);
        material.SetFloat("_RippleScale", 17f);
        material.SetFloat("_RefractionStrength", 0.022f);
        material.SetFloat("_Tinting", 0.55f);
        material.SetFloat("_CoreAlpha", 0.92f);
        material.SetFloat("_RimBoost", 1.5f);
        material.SetFloat("_RimTightness", 4.5f);
        material.SetFloat("_Glint", 0.45f);

        EditorUtility.SetDirty(material);

        return material;
    }


    private static Material GetOrCreateMaterial(
        string path,
        bool isPuddle)
    {
        Shader shader = Shader.Find(ShaderName);

        if (shader == null)
        {
            return null;
        }


        Material material =
            AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            material = new Material(shader);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }


        material.SetColor("_BaseColor", Color.white);

        if (isPuddle)
        {
            // Zameen par phaila paani: be-tarteeb kinara, zyada shaffaf
            material.SetFloat("_Softness", 0.26f);
            material.SetFloat("_EdgeWobble", 0.09f);
            material.SetFloat("_WobbleFrequency", 11f);
            material.SetFloat("_CoreAlpha", 0.62f);
            material.SetFloat("_RimBoost", 1.60f);
            material.SetFloat("_RimTightness", 5.0f);
            material.SetFloat("_Glint", 0.25f);
            material.SetFloat("_Depth", 0.30f);
        }
        else
        {
            // Girta hua qatra: kasa hua kinara, tez chamak
            material.SetFloat("_Softness", 0.18f);
            material.SetFloat("_EdgeWobble", 0.03f);
            material.SetFloat("_WobbleFrequency", 18f);
            material.SetFloat("_CoreAlpha", 0.70f);
            material.SetFloat("_RimBoost", 1.70f);
            material.SetFloat("_RimTightness", 6.0f);
            material.SetFloat("_Glint", 0.60f);
            material.SetFloat("_Depth", 0.40f);
        }

        EditorUtility.SetDirty(material);

        return material;
    }


    /// <summary>
    /// Girne wale droplets. Speed, size aur lifetime spiller runtime par
    /// deta hai, is liye yahan sirf woh cheezein set hain jo har droplet
    /// par ek jaisi hain.
    /// </summary>
    private static ParticleSystem BuildDropletSystem(
        Material material,
        int groundLayer)
    {
        GameObject go = new GameObject("Droplets");
        ParticleSystem system = go.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = system.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startSpeed = 0f;                    // velocity Emit se aati hai
        main.startLifetime = 3f;                 // zameen tak pohanchne ka waqt
        main.startSize = 0.15f;
        main.gravityModifier = 1.3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 220;                 // mobile budget

        // Emission sirf manual Emit se hoti hai
        ParticleSystem.EmissionModule emission = system.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = false;

        /*
         * Ground par lagna zaroori hai warna paani zameen ke andar
         * chala jayega aur splat kabhi nahi banega.
         */
        ParticleSystem.CollisionModule collision = system.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.quality = ParticleSystemCollisionQuality.High;

        // Sirf zameen/slope. Jars aur unke tukre nazar-andaz.
        collision.collidesWith = 1 << groundLayer;

        /*
         * Takrane par paani marna nahi chahiye — usay satah par
         * behna chahiye. Is liye lifetime bilkul nahi katti, dheema
         * droplet bhi zinda rehta hai, aur bounce na ke barabar hai
         * taake wo satah se chipak kar dhalaan par behta rahe.
         */
        collision.dampen = 0.25f;
        collision.bounce = 0.05f;
        collision.lifetimeLoss = 0f;
        collision.minKillSpeed = 0f;
        collision.radiusScale = 0.6f;
        collision.sendCollisionMessages = false;

        /*
         * Pehle har takrao par ek sub-emitter splat banata tha, is liye
         * zameen par paani ki jagah bikhre hue nuqte ban jate thay. Ab
         * zameen wala paani spiller khud ek hi baar banata hai.
         */

        /*
         * Sab se bari cheez jo isay paani jaisa banati hai: qatra apni
         * raftaar ki simt mein khinch jata hai. Gol billboard hamesha
         * bubble lagta hai, chahe shader kitna hi acha ho. Iski koi
         * alag cost nahi — wahi ek quad hai, bas khincha hua.
         */
        ParticleSystemRenderer dropletRenderer = ConfigureRenderer(
            system,
            material,
            ParticleSystemRenderMode.Stretch
        );

        dropletRenderer.velocityScale = 0.22f;
        dropletRenderer.lengthScale = 1.6f;
        dropletRenderer.cameraVelocityScale = 0f;

        return system;
    }


    /// <summary>
    /// Zameen par bana hua paani ka nishan. Yehi dhire dhire ghayab
    /// hota hai.
    /// </summary>
    private static ParticleSystem BuildSplatSystem(
        Material material)
    {
        GameObject go = new GameObject("GroundSplat");
        ParticleSystem system = go.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = system.main;

        // Manual Emit se chalta hai, is liye system khud "chalta" rehna chahiye
        main.loop = true;
        main.playOnAwake = true;
        main.startSpeed = 0f;
        main.startLifetime = 2.6f;               // "slowly disappear"
        main.startSize = 0.55f;                  // jar ~0.8 lambi hai
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 160;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[0]);

        /*
         * Splat theek us jagah banta hai jahan droplet zameen se
         * takraya — yani bilkul satah par. Wahan wo ground ke saath
         * z-fight karta hai aur aksar bilkul gayab lagta hai. Shape
         * ko zara sa upar utha kar ye masla khatam ho jata hai.
         */
        ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.01f;
        shape.position = new Vector3(0f, 0.035f, 0f);

        /*
         * Nishan pehle phailta hai — jaise paani zameen par bikhar
         * raha ho — phir apne size par ruk jata hai.
         */
        ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve grow = new AnimationCurve(
            new Keyframe(0f, 0.35f),
            new Keyframe(0.35f, 1f),
            new Keyframe(1f, 1f)
        );
        size.size = new ParticleSystem.MinMaxCurve(1f, grow);

        /*
         * Yehi wo hissa hai jo "ground par girne ke baad slowly
         * disappear" karta hai. Alpha shader ke vertex colour mein
         * jata hai, is liye fade ki koi alag cost nahi.
         */
        ParticleSystem.ColorOverLifetimeModule colour =
            system.colorOverLifetime;
        colour.enabled = true;

        Gradient fade = new Gradient();
        fade.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.12f),
                new GradientAlphaKey(1f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colour.color = new ParticleSystem.MinMaxGradient(fade);

        // Zameen par chapta pada rahe, camera ki taraf na ghoome
        ConfigureRenderer(
            system,
            material,
            ParticleSystemRenderMode.HorizontalBillboard
        );

        return system;
    }


    private static ParticleSystemRenderer ConfigureRenderer(
        ParticleSystem system,
        Material material,
        ParticleSystemRenderMode renderMode)
    {
        ParticleSystemRenderer renderer =
            system.GetComponent<ParticleSystemRenderer>();

        renderer.renderMode = renderMode;
        renderer.sharedMaterial = material;

        // Mobile: transparent particles par shadows ka koi faida nahi
        renderer.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage =
            UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage =
            UnityEngine.Rendering.ReflectionProbeUsage.Off;
        renderer.alignment = ParticleSystemRenderSpace.World;

        return renderer;
    }
}
#endif
