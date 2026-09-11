using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MapSelectionManager : MonoBehaviour
{
    public static MapSelectionManager Instancia { get; private set; }

    [Header("Lista de Mapas (ScriptableObjects)")]
    [SerializeField] private List<MapData> mapas = new List<MapData>();

    [Header("Opciones de Inicio")]
    [SerializeField] private bool empezarEnPrimerMapa = true;

    [Header("Referencias de UI (Arrastrar en Inspector)")]
    [SerializeField] private Image imagenMapa;
    [SerializeField] private RawImage rawImagenMapa;
    [SerializeField] private SpriteRenderer spriteRendererMapa;
    [SerializeField] private TextMeshProUGUI textoNombreMapa;
    [SerializeField] private TextMeshProUGUI textoTipoPista;
    [SerializeField] private TextMeshProUGUI textoDificultad;
    [SerializeField] private TextMeshProUGUI textoTiempoEstimado;

    [Header("Botones de Navegación (Arrastrar en Inspector)")]
    [SerializeField] private Button botonAdelante;
    [SerializeField] private Button botonAtras;

    [Header("Selector de Cielo")]
    [SerializeField] private TextMeshProUGUI textoNombreCielo;
    [SerializeField] private Button botonCieloAdelante;
    [SerializeField] private Button botonCieloAtras;

    private int indiceSeleccionado = 0;
    private int indiceCieloActual = 0;
    private int framesRestantesApply = 0;

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

    private void OnEscenaCargada(Scene escena, LoadSceneMode modo)
    {
        framesRestantesApply = 10;
    }

    private void Update()
    {
        if (framesRestantesApply > 0)
        {
            AplicarSkybox();
            framesRestantesApply--;
        }
    }

    private System.Collections.IEnumerator AplicarSkyboxDespuesDeFrame()
    {
        yield return null;
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
            string key = "SkyboxIndex_" + mapa.escena;
            int defaultIdx = Mathf.Clamp(mapa.skyboxDefault, 0, mapa.skyboxes.Length - 1);
            indiceCieloActual = PlayerPrefs.GetInt(key, defaultIdx);
            indiceCieloActual = Mathf.Clamp(indiceCieloActual, 0, mapa.skyboxes.Length - 1);
        }
    }

    public void CopiarReferencias(MapSelectionManager nuevo)
    {
        if (nuevo.mapas != null && nuevo.mapas.Count > 0)
            this.mapas = nuevo.mapas;

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
            this.indiceSeleccionado = 0;

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
        if (!EsMapaDesbloqueado(indice)) return;

        indiceSeleccionado = indice;
        
        MapData mapa = MapaActual;
        if (mapa != null && mapa.skyboxes != null && mapa.skyboxes.Length > 0)
        {
            string key = "SkyboxIndex_" + mapa.escena;
            int defaultIdx = Mathf.Clamp(mapa.skyboxDefault, 0, mapa.skyboxes.Length - 1);
            indiceCieloActual = PlayerPrefs.GetInt(key, defaultIdx);
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

    public bool EsMapaDesbloqueado(int indice)
    {
        if (indice < 0 || (mapas != null && indice >= mapas.Count)) return false;
        return true;
    }

    public void ActualizarUI()
    {
        MapData mapa = MapaActual;
        if (mapa == null) return;

        if (mapa.miniatura != null)
        {
            if (imagenMapa != null)
                imagenMapa.sprite = mapa.miniatura;

            if (rawImagenMapa != null)
                rawImagenMapa.texture = mapa.miniatura.texture;

            if (spriteRendererMapa != null)
                spriteRendererMapa.sprite = mapa.miniatura;
        }

        if (textoNombreMapa != null)
            textoNombreMapa.text = mapa.nombre;

        if (textoTipoPista != null)
            textoTipoPista.text = mapa.tipoPista.ToString();

        if (textoDificultad != null)
            textoDificultad.text = mapa.dificultad.ToString();

        if (textoTiempoEstimado != null)
            textoTiempoEstimado.text = mapa.tiempoEstimado.ToString("F0") + "s";

        ActualizarUICielo();
    }

    public void CargarMapaSeleccionado()
    {
        if (mapas == null || mapas.Count == 0) return;
        
        // Aplicar skybox antes de cargar la escena
        AplicarSkybox();
        
        string escena = mapas[indiceSeleccionado].escena;
        if (!string.IsNullOrEmpty(escena))
            SceneManager.LoadScene(escena);
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
            indiceSeleccionado = PlayerPrefs.GetInt("MapSelection");

        if (indiceSeleccionado < 0 || (mapas != null && indiceSeleccionado >= mapas.Count))
            indiceSeleccionado = 0;
    }

    // ==========================================
    // SELECTOR DE CIELO
    // ==========================================
    
    public void SeleccionarCieloSiguiente()
    {
        MapData mapa = MapaActual;
        if (mapa == null || mapa.skyboxes == null || mapa.skyboxes.Length <= 1) return;

        indiceCieloActual = (indiceCieloActual + 1) % mapa.skyboxes.Length;
        PlayerPrefs.SetInt("SkyboxIndex_" + mapa.escena, indiceCieloActual);
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
        PlayerPrefs.SetInt("SkyboxIndex_" + mapa.escena, indiceCieloActual);
        PlayerPrefs.Save();
        ActualizarUICielo();
        AplicarSkybox();
    }

    private void ActualizarUICielo()
    {
        MapData mapa = MapaActual;
        if (mapa == null || mapa.skyboxes == null || mapa.skyboxes.Length == 0)
        {
            if (textoNombreCielo != null)
                textoNombreCielo.text = "Sin cielo";
            return;
        }

        // Asegurar que el índice sea válido
        if (indiceCieloActual < 0 || indiceCieloActual >= mapa.skyboxes.Length)
            indiceCieloActual = 0;

        // Mostrar nombre del cielo
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
        if (mapa == null || mapa.skyboxes == null || mapa.skyboxes.Length == 0)
            return null;

        if (indiceCieloActual < 0 || indiceCieloActual >= mapa.skyboxes.Length)
            return null;

        return mapa.skyboxes[indiceCieloActual];
    }

    public void AplicarSkybox()
    {
        Material skybox = ObtenerSkyboxSeleccionado();
        if (skybox != null)
        {
            RenderSettings.skybox = skybox;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = skybox.HasProperty("_Tint") ? skybox.GetColor("_Tint") : new Color(0.5f, 0.5f, 0.5f);
            DynamicGI.UpdateEnvironment();
        }
    }
}