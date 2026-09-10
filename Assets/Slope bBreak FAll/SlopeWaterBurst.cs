using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>A short, finite volume of water: ballistic drops and a settling wet surface.</summary>
public sealed class SlopeWaterBurst : MonoBehaviour
{
    private struct Drop
    {
        public Vector3 position, velocity;
        public float radius;
        public bool landed, splashed;
    }

    private const int Segments = 12;
    private const int Latitudes = 6;
    private const int PuddleSegments = 48;
    private readonly List<Vector3> vertices = new List<Vector3>(2400);
    private readonly List<Vector3> normals = new List<Vector3>(2400);
    private readonly List<Vector2> uvs = new List<Vector2>(2400);
    private readonly List<Color> colors = new List<Color>(2400);
    private readonly List<int> triangles = new List<int>(12000);
    private readonly RaycastHit[] surfaceHits = new RaycastHit[32];
    private static readonly Queue<SlopeWaterBurst> active = new Queue<SlopeWaterBurst>();
    private Drop[] drops;
    private Mesh mesh;
    private LayerMask groundMask;
    private Color tint;
    private float age, landingAge = -1f, radius, seed;
    private Vector3 groundPoint, groundNormal, tangent, bitangent;
    private bool hasGround;

    public static void Spawn(Vector3 position, Color color, float amount,
        float scale, LayerMask mask, Material material, Transform owner)
    {
        if (material == null || amount <= 0f) return;
        // Bound transparent geometry when many glasses break together.
        while (active.Count > 0 && active.Peek() == null) active.Dequeue();
        while (active.Count >= 16)
        {
            var oldest = active.Dequeue();
            if (oldest != null) Destroy(oldest.gameObject);
        }
        var go = new GameObject("Water splash");
        go.transform.SetParent(owner, true);
        go.transform.position = position;
        var effect = go.AddComponent<SlopeWaterBurst>();
        effect.Initialize(color, amount, scale, mask, material);
        active.Enqueue(effect);
    }

    private void Initialize(Color color, float amount, float scale, LayerMask mask, Material material)
    {
        groundMask = mask;
        // Clear water with only a hint of the jar's colour.
        tint = Color.Lerp(Color.white, color, 0.12f);
        tint.a = 1f;
        seed = Random.value * 20f;
        radius = Mathf.Lerp(0.32f, 0.55f, Mathf.Clamp01(amount));
        mesh = new Mesh { name = "Animated water volume" };
        mesh.MarkDynamic();
        gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        int count = Mathf.Clamp(Mathf.RoundToInt(14 * amount), 8, 20);
        drops = new Drop[count];
        float size = Mathf.Clamp(scale, 0.6f, 1.2f);
        for (int i = 0; i < count; i++)
        {
            Vector2 spread = Random.insideUnitCircle;
            drops[i] = new Drop
            {
                position = Random.insideUnitSphere * 0.055f,
                velocity = new Vector3(spread.x * 1.35f, Random.Range(0.3f, 1.5f), spread.y * 1.35f),
                radius = Random.Range(0.025f, 0.065f) * size
            };
        }
        RebuildMesh();
    }

    private bool FindSurface(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
    {
        hit = default;
        float nearest = float.PositiveInfinity;
        int count = Physics.RaycastNonAlloc(origin, direction, surfaceHits, distance,
            groundMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            var candidate = surfaceHits[i];
            if (candidate.distance >= nearest || candidate.normal.y < 0.5f ||
                candidate.collider.GetComponentInParent<BreakableGlass>() != null ||
                candidate.collider.GetComponentInParent<BreakableGlassPiece>() != null ||
                BreakableGlass.IsBall(candidate.collider)) continue;
            hit = candidate;
            nearest = candidate.distance;
        }
        return nearest < float.PositiveInfinity;
    }

    private void Update()
    {
        // Small substeps prevent tunnelling and unstable motion after a slow frame.
        float remaining = Mathf.Min(Time.deltaTime, 0.1f);
        while (remaining > 0f)
        {
            float dt = Mathf.Min(remaining, 1f / 60f);
            Simulate(dt);
            remaining -= dt;
        }
        RebuildMesh();
        if (age > 5f) Destroy(gameObject);
    }

    private void Simulate(float dt)
    {
        age += dt;
        for (int i = 0; i < drops.Length; i++)
        {
            var drop = drops[i];
            if (drop.landed) continue;
            drop.velocity += Physics.gravity * dt;
            Vector3 step = drop.velocity * dt;
            if (FindSurface(transform.position + drop.position, step.normalized,
                step.magnitude + drop.radius, out var hit))
            {
                if (landingAge < 0f)
                {
                    landingAge = age;
                    hasGround = true;
                    groundPoint = hit.point - transform.position + hit.normal * 0.005f;
                    groundNormal = hit.normal;
                    tangent = Vector3.Cross(groundNormal, Vector3.forward).normalized;
                    if (tangent.sqrMagnitude < 0.1f) tangent = Vector3.right;
                    bitangent = Vector3.Cross(groundNormal, tangent);
                }
                if (!drop.splashed)
                {
                    drop.splashed = true;
                    drop.radius *= 0.65f;
                    drop.position = hit.point - transform.position + hit.normal * (drop.radius + 0.01f);
                    drop.velocity = Vector3.ProjectOnPlane(drop.velocity, hit.normal) * 0.35f + hit.normal * 0.65f;
                }
                else drop.landed = true;
            }
            else drop.position += step;
            if (age > 2.5f) drop.landed = true;
            drops[i] = drop;
        }
    }

    private void AddVertex(Vector3 p, Vector3 n, float thickness, float alpha)
    {
        vertices.Add(p);
        normals.Add(n);
        uvs.Add(new Vector2(thickness, 0f));
        colors.Add(new Color(tint.r, tint.g, tint.b, alpha));
    }

    private void RebuildMesh()
    {
        vertices.Clear(); normals.Clear(); uvs.Clear(); colors.Clear(); triangles.Clear();
        foreach (var drop in drops)
        {
            if (drop.landed) continue;
            // True curved geometry, stretched by velocity; no camera-facing discs.
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, drop.velocity.normalized);
            float stretch = Mathf.Clamp(1f + drop.velocity.magnitude * 0.2f, 1f, 2.3f);
            Vector3 shape = new Vector3(drop.radius / Mathf.Sqrt(stretch), drop.radius * stretch, drop.radius / Mathf.Sqrt(stretch));
            int start = vertices.Count;
            for (int y = 0; y <= Latitudes; y++)
            {
                float phi = Mathf.PI * y / Latitudes;
                for (int x = 0; x <= Segments; x++)
                {
                    float theta = 2 * Mathf.PI * x / Segments;
                    Vector3 n = new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta));
                    AddVertex(drop.position + rotation * Vector3.Scale(n, shape),
                        rotation * new Vector3(n.x / shape.x, n.y / shape.y, n.z / shape.z).normalized,
                        1f, Mathf.Clamp01((2.5f - age) * 3f));
                    if (y == Latitudes || x == Segments) continue;
                    int a = start + y * (Segments + 1) + x, b = a + Segments + 1;
                    triangles.Add(a); triangles.Add(a + 1); triangles.Add(b);
                    triangles.Add(a + 1); triangles.Add(b + 1); triangles.Add(b);
                }
            }
        }
        if (hasGround && landingAge >= 0f)
        {
            AddPuddle();
            if (age - landingAge < 0.3f) AddSplashCrown();
        }
        mesh.Clear();
        mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs);
        mesh.SetColors(colors); mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
    }

    private void AddPuddle()
    {
        float t = age - landingAge;
        float spread = radius * Mathf.Lerp(0.15f, 1f, 1f - Mathf.Exp(-t * 5f));
        float fade = Mathf.Clamp01((5f - age) / 1.2f) * Mathf.Clamp01(t * 12f);
        int start = vertices.Count;
        const int rings = 6;
        for (int ring = 0; ring <= rings; ring++)
        {
            float r = (float)ring / rings;
            for (int j = 0; j <= PuddleSegments; j++)
            {
                float angle = j * 2f * Mathf.PI / PuddleSegments;
                float boundary = 1f + 0.10f * Mathf.Sin(angle * 3 + seed) + 0.06f * Mathf.Sin(angle * 5 - seed);
                Vector3 radial = tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle);
                float wavePhase = r * 22f - t * 13f;
                float damping = Mathf.Exp(-t * 2.2f);
                float height = 0.009f * Mathf.Sin(wavePhase) * damping * (1f - r);
                float slope = (0.009f * damping / Mathf.Max(spread, 0.05f)) *
                    (22f * Mathf.Cos(wavePhase) * (1f - r) - Mathf.Sin(wavePhase));
                float meniscus = Mathf.SmoothStep(0f, 0.38f, Mathf.InverseLerp(0.65f, 1f, r));
                Vector3 normal = (groundNormal - radial * (slope + meniscus)).normalized;
                AddVertex(groundPoint + radial * (r * spread * boundary) + groundNormal * (0.01f + height),
                    normal, 0.25f, fade * (ring == rings ? 0f : 0.92f));
                if (ring == rings || j == PuddleSegments) continue;
                int a = start + ring * (PuddleSegments + 1) + j, b = a + PuddleSegments + 1;
                triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
                triangles.Add(a + 1); triangles.Add(b); triangles.Add(b + 1);
            }
        }
    }

    private void OnDestroy()
    {
        if (mesh == null) return;
        if (Application.isPlaying) Destroy(mesh);
        else DestroyImmediate(mesh);
    }

    private void AddSplashCrown()
    {
        float progress = Mathf.Clamp01((age - landingAge) / 0.3f);
        float crownRadius = Mathf.Lerp(0.06f, radius * 0.8f, progress);
        float height = Mathf.Sin(progress * Mathf.PI) * 0.11f;
        int start = vertices.Count;
        for (int ring = 0; ring <= 3; ring++)
        for (int j = 0; j <= PuddleSegments; j++)
        {
            float angle = j * 2 * Mathf.PI / PuddleSegments;
            Vector3 radial = tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle);
            float r = ring / 3f;
            float lobes = 0.75f + 0.25f * Mathf.Sin(angle * 9 + seed);
            Vector3 point = groundPoint + radial * crownRadius * (0.65f + r * 0.35f)
                + groundNormal * (Mathf.Sin(r * Mathf.PI) * height * lobes + 0.01f);
            Vector3 normal = (groundNormal - radial * Mathf.Cos(r * Mathf.PI) * 1.5f).normalized;
            AddVertex(point, normal, 0.6f, (1f - progress) * (ring == 0 || ring == 3 ? 0f : 0.8f));
            if (ring == 3 || j == PuddleSegments) continue;
            int a = start + ring * (PuddleSegments + 1) + j, b = a + PuddleSegments + 1;
            triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
            triangles.Add(a + 1); triangles.Add(b); triangles.Add(b + 1);
        }
    }
}
