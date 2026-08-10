using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System.IO;

public static class LowRaceFontImporter
{
    private const string FontsFolder = "Assets/TextMesh Pro/Resources/Fonts & Materials";

    [MenuItem("Tools/LowRace/Importar Fuentes TMP")]
    public static void ImportarFuentes()
    {
        if (!Directory.Exists(FontsFolder))
        {
            Debug.LogError("No se encontro la carpeta: " + FontsFolder);
            return;
        }

        string[] ttfPaths = Directory.GetFiles(FontsFolder, "*.ttf", SearchOption.TopDirectoryOnly);
        if (ttfPaths.Length == 0)
        {
            Debug.LogWarning("No hay archivos .ttf en " + FontsFolder);
            return;
        }

        foreach (string ttfPath in ttfPaths)
        {
            Font fuente = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (fuente == null)
            {
                Debug.LogWarning("No se pudo cargar la fuente: " + ttfPath);
                continue;
            }

            string assetName = Path.GetFileNameWithoutExtension(ttfPath) + " SDF";
            string assetPath = Path.Combine(FontsFolder, assetName + ".asset");

            TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (asset != null)
            {
                Debug.Log("Ya existe el Font Asset: " + assetPath);
                continue;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(fuente, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
            AssetDatabase.CreateAsset(fontAsset, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Creado Font Asset: " + assetPath);
        }

        AssetDatabase.Refresh();
    }
}
