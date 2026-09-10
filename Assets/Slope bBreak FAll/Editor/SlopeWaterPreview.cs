using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class SlopeWaterPreview
{
    [MenuItem("Tools/Slope Ball/Render Clear Water Preview")]
    public static void Render()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene previous = SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(previous.path))
        {
            if (Application.isBatchMode)
                EditorSceneManager.SaveScene(previous, "Assets/WaterPreviewEmpty.unity");
            else
            {
                Debug.LogWarning("Open a saved scene before rendering the water preview.");
                return;
            }
        }
        Scene preview = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        RenderTexture target = null;
        Material floorMaterial = null;
        Material tileMaterial = null;
        try
        {
            var center = new Vector3(10000, 0, 10000);
            floorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            floorMaterial.color = new Color(0.42f, 0.48f, 0.52f);
            tileMaterial = new Material(floorMaterial) { color = new Color(0.66f, 0.70f, 0.72f) };
            for (int x = -10; x <= 10; x++)
            for (int z = -10; z <= 10; z++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.transform.position = center + new Vector3(x * 0.4f, -0.05f, z * 0.4f);
                tile.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
                tile.GetComponent<Renderer>().sharedMaterial = (x + z) % 2 == 0 ? floorMaterial : tileMaterial;
            }
            var light = new GameObject("Preview light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(45, -35, 0);
            var camera = new GameObject("Preview camera").AddComponent<Camera>();
            camera.enabled = false;
            camera.transform.position = center + new Vector3(1.6f, 1.6f, -2.6f);
            camera.transform.LookAt(center + Vector3.up * 0.4f);
            camera.fieldOfView = 36;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 15;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.18f, 0.23f, 0.3f);
            var data = camera.GetUniversalAdditionalCameraData();
            data.requiresColorTexture = true;
            data.renderPostProcessing = false;
            target = new RenderTexture(960, 720, 24);
            target.Create();
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Slope bBreak FAll/Shaders/SlopeClearWater.mat");
            Physics.SyncTransforms();
            Random.State random = Random.state;
            Random.InitState(1234);
            SlopeWaterBurst.Spawn(center + Vector3.up * 0.8f, Color.white, 1f, 1f, ~0, material, null);
            Random.state = random;
            SlopeWaterBurst effect = null;
            foreach (var root in preview.GetRootGameObjects())
                if (root.TryGetComponent<SlopeWaterBurst>(out var candidate)) effect = candidate;
            var simulate = typeof(SlopeWaterBurst).GetMethod("Simulate", BindingFlags.Instance | BindingFlags.NonPublic);
            var rebuild = typeof(SlopeWaterBurst).GetMethod("RebuildMesh", BindingFlags.Instance | BindingFlags.NonPublic);
            Directory.CreateDirectory("WaterPreview");
            for (int frame = 0; frame <= 110; frame++)
            {
                simulate.Invoke(effect, new object[] { 1f / 60f });
                if (frame != 12 && frame != 35 && frame != 65 && frame != 110) continue;
                rebuild.Invoke(effect, null);
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                var oldTarget = RenderTexture.active;
                RenderTexture.active = target;
                var image = new Texture2D(960, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 960, 720), 0, 0);
                image.Apply();
                File.WriteAllBytes($"WaterPreview/water-{frame}.png", image.EncodeToPNG());
                Object.DestroyImmediate(image);
                RenderTexture.active = oldTarget;
            }
            var messages = ShaderUtil.GetShaderMessages(material.shader);
            File.WriteAllText("WaterPreview/result.txt", "Render complete. Shader messages: " + messages.Length);
            foreach (var message in messages) Debug.Log(message.message);
            Debug.Log("Clear water preview rendered to WaterPreview.");
        }
        finally
        {
            EditorSceneManager.CloseScene(preview, true);
            SceneManager.SetActiveScene(previous);
            if (target != null) Object.DestroyImmediate(target);
            if (floorMaterial != null) Object.DestroyImmediate(floorMaterial);
            if (tileMaterial != null) Object.DestroyImmediate(tileMaterial);
        }
    }
}
