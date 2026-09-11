using UnityEngine;

public class AplicarSkyboxEscena : MonoBehaviour
{
    [Header("MapData del mapa actual")]
    [SerializeField] private MapData mapaActual;

    private int framesRestantes = 15;

    private void Start()
    {
        Aplicar();
    }

    private void Update()
    {
        if (framesRestantes > 0)
        {
            Aplicar();
            framesRestantes--;
        }
    }

    private void Aplicar()
    {
        Material skybox = null;

        if (MapSelectionManager.Instancia != null)
        {
            skybox = MapSelectionManager.Instancia.ObtenerSkyboxSeleccionado();
        }

        if (skybox == null && mapaActual != null && mapaActual.skyboxes != null && mapaActual.skyboxes.Length > 0)
        {
            string key = "SkyboxIndex_" + mapaActual.escena;
            int idx = PlayerPrefs.GetInt(key, mapaActual.skyboxDefault);
            idx = Mathf.Clamp(idx, 0, mapaActual.skyboxes.Length - 1);
            skybox = mapaActual.skyboxes[idx];
        }

        if (skybox != null)
        {
            RenderSettings.skybox = skybox;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = skybox.HasProperty("_Tint") ? skybox.GetColor("_Tint") : new Color(0.5f, 0.5f, 0.5f);
            DynamicGI.UpdateEnvironment();
        }
    }
}
