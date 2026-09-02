using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapMenuUI : MonoBehaviour
{
    [Header("Panel Mapa")]
    [SerializeField] private GameObject panelMapa;
    [SerializeField] private Image imagenMapa;
    [SerializeField] private TextMeshProUGUI textoNombreMapa;
    [SerializeField] private TextMeshProUGUI textoDescripcion;
    [SerializeField] private TextMeshProUGUI textoTiempoRecord;

    [Header("Navegación")]
    [SerializeField] private Button botonIzquierda;
    [SerializeField] private Button botonDerecha;

    [Header("Botones Principales")]
    [SerializeField] private Button botonJugar;
    [SerializeField] private Button botonVolver;
    [SerializeField] private Button botonAjustes;

    [Header("Panel Ajustes")]
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private Slider sliderVolumenMusica;
    [SerializeField] private Slider sliderVolumenSFX;
    [SerializeField] private Toggle togglePantallaCompleta;
    [SerializeField] private TMP_Dropdown dropdownCalidad;
    [SerializeField] private Button botonCerrarAjustes;

    private int indiceActual = 0;

    private void Awake()
    {
        if (botonIzquierda != null)
            botonIzquierda.onClick.AddListener(NavegarIzquierda);

        if (botonDerecha != null)
            botonDerecha.onClick.AddListener(NavegarDerecha);

        if (botonJugar != null)
            botonJugar.onClick.AddListener(Jugar);

        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverAlMenu);

        if (botonAjustes != null)
            botonAjustes.onClick.AddListener(AbrirAjustes);

        if (botonCerrarAjustes != null)
            botonCerrarAjustes.onClick.AddListener(CerrarAjustes);

        if (sliderVolumenMusica != null)
            sliderVolumenMusica.onValueChanged.AddListener(CambiarVolumenMusica);

        if (sliderVolumenSFX != null)
            sliderVolumenSFX.onValueChanged.AddListener(CambiarVolumenSFX);

        if (togglePantallaCompleta != null)
            togglePantallaCompleta.onValueChanged.AddListener(CambiarPantallaCompleta);

        if (dropdownCalidad != null)
            dropdownCalidad.onValueChanged.AddListener(CambiarCalidad);
    }

    private void Start()
    {
        if (MapSelectionManager.Instancia != null)
        {
            MapSelectionManager.Instancia.OnMapaCambiado += ActualizarUI;
            indiceActual = MapSelectionManager.Instancia.IndiceSeleccionado;
        }

        ActualizarUI(indiceActual);

        if (panelAjustes != null)
            panelAjustes.SetActive(false);
    }

    private void OnDestroy()
    {
        if (MapSelectionManager.Instancia != null)
            MapSelectionManager.Instancia.OnMapaCambiado -= ActualizarUI;
    }

    private void NavegarIzquierda()
    {
        if (MapSelectionManager.Instancia == null) return;
        MapSelectionManager.Instancia.SeleccionarAnterior();
        indiceActual = MapSelectionManager.Instancia.IndiceSeleccionado;
    }

    private void NavegarDerecha()
    {
        if (MapSelectionManager.Instancia == null) return;
        MapSelectionManager.Instancia.SeleccionarSiguiente();
        indiceActual = MapSelectionManager.Instancia.IndiceSeleccionado;
    }

    private void ActualizarUI(int indice)
    {
        if (MapSelectionManager.Instancia == null) return;

        var mapa = MapSelectionManager.Instancia.MapaActual;
        if (mapa == null) return;

        if (imagenMapa != null && mapa.miniatura != null)
            imagenMapa.sprite = mapa.miniatura;

        if (textoNombreMapa != null)
            textoNombreMapa.text = mapa.nombre;

        if (textoDescripcion != null)
            textoDescripcion.text = mapa.descripcion;

        if (textoTiempoRecord != null)
            textoTiempoRecord.text = "Sin récord";
    }

    private void Jugar()
    {
        if (MapSelectionManager.Instancia != null)
            MapSelectionManager.Instancia.CargarMapaSeleccionado();
    }

    private void VolverAlMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    private void AbrirAjustes()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(true);
    }

    private void CerrarAjustes()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(false);
    }

    private void CambiarVolumenMusica(float valor)
    {
        AudioListener.volume = valor;
    }

    private void CambiarVolumenSFX(float valor)
    {
    }

    private void CambiarPantallaCompleta(bool valor)
    {
        Screen.fullScreen = valor;
    }

    private void CambiarCalidad(int indice)
    {
        QualitySettings.SetQualityLevel(indice);
    }
}
