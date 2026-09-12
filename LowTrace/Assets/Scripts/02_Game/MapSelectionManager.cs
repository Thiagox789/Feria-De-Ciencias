using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// Este es el Gestor de Selección de Mapas y Cielos (MapSelectionManager).
// Permite al jugador elegir qué circuito correr y qué clima/cielo (skybox) usar.
public class MapSelectionManager : MonoBehaviour
{
    public static MapSelectionManager Instancia { get; private set; }

    [Header("Lista de Circuitos Disponibles")]
    [SerializeField] private List<MapData> mapas = new List<MapData>();

    [Header("Opciones de Inicio")]
    [SerializeField] private bool empezarEnPrimerMapa = true;

    [Header("Elementos de la Interfaz (UI)")]
    [SerializeField] private Image imagenMapa;
    [SerializeField] private RawImage rawImagenMapa;
    [SerializeField] private SpriteRenderer spriteRendererMapa;
    [SerializeField] private TextMeshProUGUI textoNombreMapa;
    [SerializeField] private TextMeshProUGUI textoTipoPista;
    [SerializeField] private TextMeshProUGUI textoDificultad;
    [SerializeField] private TextMeshProUGUI textoTiempoEstimado;

    [Header("Botones para Cambiar de Mapa")]
    [SerializeField] private Button botonAdelante;
    [SerializeField] private Button botonAtras;

    [Header("Botones para Cambiar de Cielo (Skybox)")]
    [SerializeField] private TextMeshProUGUI textoNombreCielo;
    [SerializeField] private Button botonCieloAdelante;
    [SerializeField] private Button botonCieloAtras;

    private int indiceSeleccionado = 0;
    private int indiceCieloActual = 0;

    public int IndiceSeleccionado => indiceSeleccionado;
    public MapData MapaActual => (mapas != null && mapas.Count > 0 && indiceSeleccionado >= 0 && indiceSeleccionado < mapas.Count) ? mapas[indiceSeleccionado] : null;
    public List<MapData> Mapas => mapas;

    public event System.Action<int> OnMapaCambiado;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Instancia.CopiarReferencias(this);
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        InicializarSeleccion();
    }

    private void Start()
    {
        ConectarBotones();
        ActualizarUI();
        SceneManager.sceneLoaded += OnEscenaCargada;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnEscenaCargada;
    }

    // Cuando carga una nueva escena, esperamos un momento antes de aplicar el skybox.
    // Esto es necesario porque Unity puede sobreescribir el skybox durante la carga.
    private static readonly string[] escenasUI = { "Menu", "Ajustes", "Ranking", "Creditos", "Seleccion-Mapa" };

    private void OnEscenaCargada(Scene escena, LoadSceneMode modo)
    {
        foreach (string ui in escenasUI)
        {
            if (escena.name == ui)
            {
                RenderSettings.skybox = null;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientSkyColor = new Color(0.15f, 0.15f, 0.2f);
                RenderSettings.fog = false;
                return;
            }
        }
        // En escenas de juego (Circuito1, Circuito2), AplicarSkyboxEscena se encarga
    }

    // Esperamos hasta que la escena esté completamente lista y luego ponemos el skybox
    private IEnumerator AplicarSkyboxAlCargar()
    {
        // Esperamos 3 frames para que Unity termine de inicializar la escena
        yield return null;
        yield return null;
        yield return null;
        // Esperamos también al final del frame de render
        yield return new WaitForEndOfFrame();
        AplicarSkybox();
        // Aplicamos una vez más por las dudas (por si URP hace override en el primer render)
        yield return new WaitForEndOfFrame();
        AplicarSkybox();
    }

    private void InicializarSeleccion()
    {
        if (empezarEnPrimerMapa)
        {
            indiceSeleccionado = 0;
        }
        else
        {
            CargarSeleccion();
        }

        MapData mapa = MapaActual;
        if (mapa != null && mapa.skyboxes != null && mapa.skyboxes.Length > 0)
        {
            int defecto = Mathf.Clamp(mapa.skyboxDefault, 0, mapa.skyboxes.Length - 1);
            indiceCieloActual = PlayerPrefs.GetInt("SkyboxGlobal", defecto);
            indiceCieloActual = Mathf.Clamp(indiceCieloActual, 0, mapa.skyboxes.Length - 1);
        }
    }

    public void CopiarReferencias(MapSelectionManager nuevo)
    {
        if (nuevo.mapas != null && nuevo.mapas.Count > 0)
        {
            this.mapas = nuevo.mapas;
        }

        this.imagenMapa = nuevo.imagenMapa;
        this.rawImagenMapa = nuevo.rawImagenMapa;
        this.spriteRendererMapa = nuevo.spriteRendererMapa;
        this.textoNombreMapa = nuevo.textoNombreMapa;
        this.textoTipoPista = nuevo.textoTipoPista;
        this.textoDificultad = nuevo.textoDificultad;
        this.textoTiempoEstimado = nuevo.textoTiempoEstimado;
        this.botonAdelante = nuevo.botonAdelante;
        this.botonAtras = nuevo.botonAtras;

        this.textoNombreCielo = nuevo.textoNombreCielo;
        this.botonCieloAdelante = nuevo.botonCieloAdelante;
        this.botonCieloAtras = nuevo.botonCieloAtras;

        if (nuevo.empezarEnPrimerMapa)
        {
            this.indiceSeleccionado = 0;
        }

        ConectarBotones();
        ActualizarUI();
    }

    private void ConectarBotones()
    {
        if (botonAdelante != null)
        {
            botonAdelante.onClick.RemoveListener(SeleccionarSiguiente);
            botonAdelante.onClick.AddListener(SeleccionarSiguiente);
        }

        if (botonAtras != null)
        {
            botonAtras.onClick.RemoveListener(SeleccionarAnterior);
            botonAtras.onClick.AddListener(SeleccionarAnterior);
        }

        if (botonCieloAdelante != null)
        {
            botonCieloAdelante.onClick.RemoveListener(SeleccionarCieloSiguiente);
            botonCieloAdelante.onClick.AddListener(SeleccionarCieloSiguiente);
        }

        if (botonCieloAtras != null)
        {
            botonCieloAtras.onClick.RemoveListener(SeleccionarCieloAnterior);
            botonCieloAtras.onClick.AddListener(SeleccionarCieloAnterior);
        }
    }

    public void SeleccionarMapa(int indice)
    {
        if (mapas == null || mapas.Count == 0) return;
        if (indice < 0 || indice >= mapas.Count) return;

        indiceSeleccionado = indice;
        
        MapData mapa = MapaActual;
        if (mapa != null && mapa.skyboxes != null && mapa.skyboxes.Length > 0)
        {
            int defecto = Mathf.Clamp(mapa.skyboxDefault, 0, mapa.skyboxes.Length - 1);
            indiceCieloActual = PlayerPrefs.GetInt("SkyboxGlobal", defecto);
            indiceCieloActual = Mathf.Clamp(indiceCieloActual, 0, mapa.skyboxes.Length - 1);
        }
        else
        {
            indiceCieloActual = 0;
        }
        
        GuardarSeleccion();
        ActualizarUI();
        AplicarSkybox();
        OnMapaCambiado?.Invoke(indice);
    }

    public void SeleccionarSiguiente()
    {
        if (mapas == null || mapas.Count == 0) return;
        int siguiente = (indiceSeleccionado + 1) % mapas.Count;
        SeleccionarMapa(siguiente);
    }

    public void SeleccionarAnterior()
    {
        if (mapas == null || mapas.Count == 0) return;
        int anterior = indiceSeleccionado - 1;
        if (anterior < 0) anterior = mapas.Count - 1;
        SeleccionarMapa(anterior);
    }

    public void ActualizarUI()
    {
        MapData mapa = MapaActual;
        if (mapa == null) return;

        if (mapa.miniatura != null)
        {
            if (imagenMapa != null) imagenMapa.sprite = mapa.miniatura;
            if (rawImagenMapa != null) rawImagenMapa.texture = mapa.miniatura.texture;
            if (spriteRendererMapa != null) spriteRendererMapa.sprite = mapa.miniatura;
        }

        if (textoNombreMapa != null) textoNombreMapa.text = mapa.nombre;
        if (textoTipoPista != null) textoTipoPista.text = mapa.tipoPista.ToString();
        if (textoDificultad != null) textoDificultad.text = mapa.dificultad.ToString();
        if (textoTiempoEstimado != null) textoTiempoEstimado.text = mapa.tiempoEstimado.ToString("F0") + "s";

        ActualizarUICielo();
    }

    public void CargarMapaSeleccionado()
    {
        if (mapas != null && mapas.Count > 0 && indiceSeleccionado >= 0 && indiceSeleccionado < mapas.Count)
        {
            string escena = mapas[indiceSeleccionado].escena;
            if (!string.IsNullOrEmpty(escena))
            {
                // Guardamos qué skybox usar antes de cargar la nueva escena
                // El skybox se aplicará automáticamente cuando la escena cargue (OnEscenaCargada)
                SceneManager.LoadScene(escena);
                return;
            }
        }

        // Fallback por defecto si no hay lista de mapas configurada en el inspector
        SceneManager.LoadScene("Circuito1");
    }

    public string ObtenerNombreEscenaActual()
    {
        if (mapas == null || mapas.Count == 0) return "Game";
        return mapas[indiceSeleccionado].escena;
    }

    private void GuardarSeleccion()
    {
        PlayerPrefs.SetInt("MapSelection", indiceSeleccionado);
        PlayerPrefs.Save();
    }

    private void CargarSeleccion()
    {
        if (PlayerPrefs.HasKey("MapSelection"))
        {
            indiceSeleccionado = PlayerPrefs.GetInt("MapSelection");
        }

        if (indiceSeleccionado < 0 || (mapas != null && indiceSeleccionado >= mapas.Count))
        {
            indiceSeleccionado = 0;
        }
    }

    // ==========================================
    // SELECCIÓN Y CAMBIO DE CIELO (SKYBOX)
    // ==========================================
    
    public void SeleccionarCieloSiguiente()
    {
        MapData mapa = MapaActual;
        if (mapa == null || mapa.skyboxes == null || mapa.skyboxes.Length <= 1) return;

        indiceCieloActual = (indiceCieloActual + 1) % mapa.skyboxes.Length;
        PlayerPrefs.SetInt("SkyboxGlobal", indiceCieloActual);
        PlayerPrefs.Save();
        ActualizarUICielo();
        AplicarSkybox();
    }

    public void SeleccionarCieloAnterior()
    {
        MapData mapa = MapaActual;
        if (mapa == null || mapa.skyboxes == null || mapa.skyboxes.Length <= 1) return;

        indiceCieloActual--;
        if (indiceCieloActual < 0) indiceCieloActual = mapa.skyboxes.Length - 1;
        PlayerPrefs.SetInt("SkyboxGlobal", indiceCieloActual);
        PlayerPrefs.Save();
        ActualizarUICielo();
        AplicarSkybox();
    }

    private void ActualizarUICielo()
    {
        MapData mapa = MapaActual;
        if (mapa == null || mapa.skyboxes == null || mapa.skyboxes.Length == 0)
        {
            if (textoNombreCielo != null) textoNombreCielo.text = "Sin cielo";
            return;
        }

        if (indiceCieloActual < 0 || indiceCieloActual >= mapa.skyboxes.Length)
        {
            indiceCieloActual = 0;
        }

        if (textoNombreCielo != null)
        {
            if (mapa.nombresSkyboxes != null && indiceCieloActual < mapa.nombresSkyboxes.Length)
                textoNombreCielo.text = mapa.nombresSkyboxes[indiceCieloActual];
            else
                textoNombreCielo.text = "Cielo " + (indiceCieloActual + 1);
        }
    }

    public Material ObtenerSkyboxSeleccionado()
    {
        MapData mapa = MapaActual;
        if (mapa == null || mapa.skyboxes == null || mapa.skyboxes.Length == 0) return null;
        if (indiceCieloActual < 0 || indiceCieloActual >= mapa.skyboxes.Length) return null;

        return mapa.skyboxes[indiceCieloActual];
    }

    public void AplicarSkybox()
    {
        Material skybox = ObtenerSkyboxSeleccionado();
        if (skybox == null) return;

        // Ponemos el material del cielo en los ajustes del mundo
        RenderSettings.skybox = skybox;
        DynamicGI.UpdateEnvironment();

        // Buscamos TODAS las camaras y les decimos que muestren el cielo
        Camera[] todasLasCamaras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in todasLasCamaras)
        {
            // Solo configuramos las camaras que no son de la interfaz (UI)
            if (cam.gameObject.layer != LayerMask.NameToLayer("UI"))
            {
                cam.clearFlags = CameraClearFlags.Skybox;
            }
        }

        // Nos aseguramos tambien con Camera.main por si acaso
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
    }
}