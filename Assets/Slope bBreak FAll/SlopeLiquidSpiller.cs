//using UnityEngine;

//[DisallowMultipleComponent]
//public sealed class SlopeLiquidSpiller : MonoBehaviour
//{
//    private static SlopeLiquidSpiller instance;

//    [SerializeField] private Material waterMaterial;
//    [SerializeField] private LayerMask groundMask = ~0;
//    private Material runtimeMaterial;

//    private void Awake()
//    {
//        foreach (var system in GetComponentsInChildren<ParticleSystem>(true))
//        {
//            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
//            system.gameObject.SetActive(false);
//        }
//    }

//    private void OnEnable() => instance = this;

//    private void OnDisable()
//    {
//        if (instance == this) instance = null;
//        foreach (var burst in GetComponentsInChildren<SlopeWaterBurst>(true))
//            Destroy(burst.gameObject);
//    }

//    private void OnDestroy()
//    {
//        if (instance == this) instance = null;
//        if (runtimeMaterial != null) Destroy(runtimeMaterial);
//    }

//    public static void SpillAt(Vector3 position, Color liquidColor, float amount, float sizeScale)
//    {
//        // Recover after a script reload, and support scenes without the spill prefab.
//        if (instance == null)
//            instance = FindFirstObjectByType<SlopeLiquidSpiller>();
//        if (instance == null)
//            instance = new GameObject("Slope Water Effects").AddComponent<SlopeLiquidSpiller>();

//        instance.Spill(position, liquidColor, amount, sizeScale);
//    }

//    public void Spill(Vector3 position, Color liquidColor, float amount, float sizeScale)
//    {
//        if (waterMaterial == null)
//        {
//            Shader shader = Shader.Find("Slope Ball/Clear Water");
//            if (shader == null)
//            {
//                Debug.LogError("Water splash could not start: Slope Ball/Clear Water shader is missing.", this);
//                return;
//            }
//            runtimeMaterial = new Material(shader);
//            waterMaterial = runtimeMaterial;
//        }
//        SlopeWaterBurst.Spawn(position, liquidColor, amount, sizeScale,
//            groundMask, waterMaterial, transform);
//    }
//}









using UnityEngine;

[DisallowMultipleComponent]
public sealed class SlopeLiquidSpiller : MonoBehaviour
{
    private static SlopeLiquidSpiller instance;

    [Header("Water Surface")]
    [SerializeField] private Material waterMaterial;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Splash Particle")]
    [SerializeField] private ParticleSystem spillParticlePrefab;

    private Material runtimeMaterial;

    private void OnEnable()
    {
        instance = this;
    }

    private void OnDisable()
    {
        if (instance == this)
            instance = null;

        foreach (var burst in GetComponentsInChildren<SlopeWaterBurst>(true))
        {
            Destroy(burst.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }

    public static void SpillAt(
        Vector3 position,
        Color liquidColor,
        float amount,
        float sizeScale)
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<SlopeLiquidSpiller>();
        }

        if (instance == null)
        {
            GameObject go = new GameObject("Slope Water Effects");
            instance = go.AddComponent<SlopeLiquidSpiller>();
        }

        instance.Spill(
            position,
            liquidColor,
            amount,
            sizeScale
        );
    }

    public void Spill(
        Vector3 position,
        Color liquidColor,
        float amount,
        float sizeScale)
    {
        SpawnParticle(
            position,
            liquidColor,
            amount,
            sizeScale
        );

        if (waterMaterial == null)
        {
            Shader shader = Shader.Find("Slope Ball/Clear Water");

            if (shader == null)
            {
                Debug.LogError(
                    "Water splash could not start: Slope Ball/Clear Water shader is missing.",
                    this
                );
                return;
            }

            runtimeMaterial = new Material(shader);
            waterMaterial = runtimeMaterial;
        }

        SlopeWaterBurst.Spawn(
            position,
            liquidColor,
            amount,
            sizeScale,
            groundMask,
            waterMaterial,
            transform
        );
    }

    private void SpawnParticle(
        Vector3 position,
        Color liquidColor,
        float amount,
        float sizeScale)
    {
        if (spillParticlePrefab == null)
        {
            Debug.LogWarning(
                "Spill Particle Prefab is not assigned.",
                this
            );
            return;
        }

        ParticleSystem particle = Instantiate(
            spillParticlePrefab,
            position,
            Quaternion.identity
        );

        particle.gameObject.SetActive(true);

        particle.transform.localScale =
            Vector3.one * Mathf.Max(0.01f, sizeScale);

        ParticleSystem.MainModule main = particle.main;

        main.startColor = liquidColor;

        particle.Clear(true);
        particle.Play(true);

        int extraParticles = Mathf.Clamp(
            Mathf.RoundToInt(amount * 10f),
            0,
            50
        );

        if (extraParticles > 0)
        {
            particle.Emit(extraParticles);
        }

        float destroyDelay =
            main.duration +
            main.startLifetime.constantMax +
            1f;

        Destroy(
            particle.gameObject,
            destroyDelay
        );
    }
}






