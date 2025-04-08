using UnityEngine;
using UnityEditor;
using System.IO;

public class SetSpritePPU : EditorWindow
{
    private int pixelsPerUnit = 100;

    [MenuItem("Tools/Set PPU for Sprites Folder")]
    public static void ShowWindow()
    {
        GetWindow<SetSpritePPU>("Set Sprite PPU");
    }

    void OnGUI()
    {
        GUILayout.Label("Set Pixels Per Unit for Sprites", EditorStyles.boldLabel);
        pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", pixelsPerUnit);

        if (GUILayout.Button("Apply to Assets/Sprites/"))
        {
            ApplyPPUToSpritesFolder(pixelsPerUnit);
        }
    }

    private static void ApplyPPUToSpritesFolder(int ppu)
    {
        string spritesFolderPath = "Assets/Sprites";
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { spritesFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null && importer.textureType == TextureImporterType.Sprite)
            {
                importer.spritePixelsPerUnit = ppu;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                Debug.Log($"Updated: {path}");
            }
        }

        Debug.Log($"✅ All sprites in '{spritesFolderPath}' updated to {ppu} PPU.");
    }
}
