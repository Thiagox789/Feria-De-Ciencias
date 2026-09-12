using UnityEngine;

public class AplicarSkyboxEscena : MonoBehaviour
{
    [Header("Datos del Mapa Actual")]
    [SerializeField] private MapData mapaActual;

    private Material materialSkybox;
    private int framesParaAplicar = 30;

    private void Start()
    {
        materialSkybox = ObtenerSkyboxCorrecto();
        if (materialSkybox != null)
        {
            Debug.Log("[Skybox] Aplicando: " + materialSkybox.name);
        }
    }

    private void Update()
    {
        if (framesParaAplicar > 0 && materialSkybox != null)
        {
            AplicarSkybox();
            framesParaAplicar--;
        }
    }

    private void AplicarSkybox()
    {
        // 1. Skybox material
        RenderSettings.skybox = materialSkybox;

        // 2. Ambient: sacar colores del skybox
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1f;

        // 3. Aplicar preset de iluminacion segun el cielo
        AplicarPresetIluminacion(materialSkybox.name);

        // 4. GI
        DynamicGI.UpdateEnvironment();

        // 5. Camaras
        Camera[] camaras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in camaras)
        {
            if (cam.gameObject.layer != LayerMask.NameToLayer("UI"))
                cam.clearFlags = CameraClearFlags.Skybox;
        }
    }

    private void AplicarPresetIluminacion(string nombreSkybox)
    {
        string nombre = nombreSkybox.ToLower();

        if (nombre.Contains("night") || nombre.Contains("noche"))
        {
            // Noche: azul oscuro, frio
            RenderSettings.ambientSkyColor = new Color(0.05f, 0.05f, 0.15f);
            RenderSettings.ambientEquatorColor = new Color(0.03f, 0.03f, 0.08f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.05f);
            SetSunColor(new Color(0.4f, 0.45f, 0.6f), 0.3f);
            SetFogColor(new Color(0.05f, 0.05f, 0.12f));
        }
        else if (nombre.Contains("alien"))
        {
            // Alien: verde/morado extraterrestre
            RenderSettings.ambientSkyColor = new Color(0.15f, 0.08f, 0.2f);
            RenderSettings.ambientEquatorColor = new Color(0.1f, 0.05f, 0.15f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.02f, 0.08f);
            SetSunColor(new Color(0.6f, 0.4f, 0.8f), 0.6f);
            SetFogColor(new Color(0.1f, 0.05f, 0.15f));
        }
        else if (nombre.Contains("space") || nombre.Contains("espacio"))
        {
            // Espacio: negro profundo, estrellas
            RenderSettings.ambientSkyColor = new Color(0.01f, 0.01f, 0.03f);
            RenderSettings.ambientEquatorColor = new Color(0.005f, 0.005f, 0.02f);
            RenderSettings.ambientGroundColor = new Color(0.002f, 0.002f, 0.01f);
            SetSunColor(new Color(0.8f, 0.8f, 1f), 0.2f);
            SetFogColor(new Color(0.01f, 0.01f, 0.03f));
        }
        else if (nombre.Contains("morning") || nombre.Contains("amanecer"))
        {
            // Amanecer: naranja/rosa suave
            RenderSettings.ambientSkyColor = new Color(0.6f, 0.4f, 0.3f);
            RenderSettings.ambientEquatorColor = new Color(0.5f, 0.3f, 0.25f);
            RenderSettings.ambientGroundColor = new Color(0.3f, 0.2f, 0.15f);
            SetSunColor(new Color(1f, 0.7f, 0.4f), 0.8f);
            SetFogColor(new Color(0.7f, 0.5f, 0.35f));
        }
        else if (nombre.Contains("sunset") || nombre.Contains("atardecer"))
        {
            // Atardecer: naranja fuerte, sombras largas
            RenderSettings.ambientSkyColor = new Color(0.7f, 0.35f, 0.2f);
            RenderSettings.ambientEquatorColor = new Color(0.5f, 0.25f, 0.15f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.12f, 0.08f);
            SetSunColor(new Color(1f, 0.5f, 0.2f), 1f);
            SetFogColor(new Color(0.8f, 0.4f, 0.2f));
        }
        else
        {
            // Dia (default): azul claro, brillante
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.6f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.45f, 0.5f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.22f, 0.25f);
            SetSunColor(new Color(1f, 0.95f, 0.85f), 1f);
            SetFogColor(new Color(0.6f, 0.7f, 0.8f));
        }
    }

    private void SetSunColor(Color color, float intensidad)
    {
        Light sol = FindFirstObjectByType<Light>();
        if (sol != null && sol.type == LightType.Directional)
        {
            sol.color = color;
            sol.intensity = intensidad;
        }
    }

    private void SetFogColor(Color color)
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = color;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 50f;
        RenderSettings.fogEndDistance = 300f;
    }

    private Material ObtenerSkyboxCorrecto()
    {
        if (mapaActual != null && mapaActual.skyboxes != null && mapaActual.skyboxes.Length > 0)
        {
            int indice = PlayerPrefs.GetInt("SkyboxGlobal", mapaActual.skyboxDefault);
            indice = Mathf.Clamp(indice, 0, mapaActual.skyboxes.Length - 1);
            return mapaActual.skyboxes[indice];
        }
        return null;
    }
}
