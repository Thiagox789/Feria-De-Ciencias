using UnityEngine;
using UnityEditor;

public class CrearSkyboxesKenny : EditorWindow
{
    [MenuItem("LowTrace/Crear Skyboxes de Kenny")]
    static void Crear()
    {
        string carpetaTexturas = "Assets/Assets-Importados/Kenny/kenney_skyboxes/Skyboxes";
        string carpetaMateriales = "Assets/Assets-Importados/Kenny/kenney_skyboxes/Materials";

        if (!AssetDatabase.IsValidFolder(carpetaMateriales))
        {
            AssetDatabase.CreateFolder("Assets/Assets-Importados/Kenny/kenney_skyboxes", "Materials");
        }

        string[] texturasNombres = { "skybox-day", "skybox-night", "skybox-alien", "skybox-space", "skybox-morning" };

        foreach (string nombre in texturasNombres)
        {
            string rutaTextura = carpetaTexturas + "/" + nombre + ".png";
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(rutaTextura);

            if (tex == null)
            {
                Debug.LogWarning("No se encontró: " + rutaTextura);
                continue;
            }

            Material mat = new Material(Shader.Find("Skybox/Panoramic"));
            mat.name = "Skybox-" + nombre;
            mat.mainTexture = tex;

            string rutaMat = carpetaMateriales + "/Skybox-" + nombre + ".mat";
            AssetDatabase.CreateAsset(mat, rutaMat);
            Debug.Log("Creado: " + rutaMat);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Skyboxes de Kenny creados. Asignalos en los MapData.");
    }
}
