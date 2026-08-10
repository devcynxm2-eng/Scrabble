#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridLevelData))]
public sealed class GridLevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        GridLevelData levelData =
            (GridLevelData)target;


        EditorGUILayout.Space(15f);

        EditorGUILayout.LabelField(
            "Image → 3D Grid Baker",
            EditorStyles.boldLabel
        );


        if (levelData.SourceImage == null)
        {
            EditorGUILayout.HelpBox(
                "Source Image assign karein.",
                MessageType.Warning
            );
        }


        using (
            new EditorGUI.DisabledScope(
                levelData.SourceImage == null
            ))
        {
            if (GUILayout.Button(
                    "BAKE 3D GRID FROM IMAGE",
                    GUILayout.Height(38f)))
            {
                BakeGrid(
                    levelData
                );
            }
        }


        EditorGUILayout.Space(8f);


        if (levelData.HasValidBakedGrid)
        {
            EditorGUILayout.HelpBox(
                $"Baked Grid: " +
                $"{levelData.GridWidth} x " +
                $"{levelData.GridHeight} x " +
                $"{levelData.GridDepth}\n" +
                $"Occupied Boxes: " +
                $"{levelData.BakedOccupiedCellCount}",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Grid abhi bake nahi hui. " +
                "Source Image set karke Bake button press karein.",
                MessageType.Warning
            );
        }
    }


    private static void BakeGrid(
        GridLevelData levelData)
    {
        Texture2D texture =
            levelData.SourceImage;


        if (texture == null)
        {
            return;
        }


        string assetPath =
            AssetDatabase.GetAssetPath(
                texture
            );


        TextureImporter importer =
            AssetImporter.GetAtPath(
                assetPath
            ) as TextureImporter;


        bool restoreReadability =
            false;


        try
        {
            /*
             * User ko manually Read/Write ON
             * karne ki zaroorat nahi.
             */
            if (importer != null &&
                !importer.isReadable)
            {
                restoreReadability =
                    true;


                importer.isReadable =
                    true;


                importer.SaveAndReimport();
            }


            Undo.RecordObject(
                levelData,
                "Bake Grid Level From Image"
            );


            levelData
                .EditorBakeFromReadableSourceImage();


            EditorUtility.SetDirty(
                levelData
            );


            AssetDatabase.SaveAssets();


            SceneView.RepaintAll();
        }
        catch (System.Exception exception)
        {
            Debug.LogException(
                exception,
                levelData
            );
        }
        finally
        {
            /*
             * Texture ki original import setting
             * restore kar dete hain.
             */
            if (importer != null &&
                restoreReadability)
            {
                importer.isReadable =
                    false;


                importer.SaveAndReimport();
            }
        }
    }
}

#endif