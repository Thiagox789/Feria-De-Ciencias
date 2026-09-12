using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Este script controla la pantalla o panel de Ajustes del juego.
// Permite modificar el volumen de la música, el volumen de los efectos de sonido,
// cambiar la pantalla completa y ajustar la calidad gráfica.
public class SettingsPanelUI : MonoBehaviour
{
    [Header("Barritas de Volumen (Sliders)")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderEfectosSFX;

    [Header("Opciones Adicionales")]
    [SerializeField] private Toggle togglePantallaCompleta;
    [SerializeField] private TMP_Dropdown dropdownCalidadGraficos;
    [SerializeField] private Button botonCerrarPanel;

    protected virtual void Start()
    {
        CargarValoresGuardados();
        ConectarEventosDeUI();
    }

    // Carga los datos de volumen guardados previamente en el disco duro (JSON)
    public void CargarValoresGuardados()
    {
        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            if (sliderMusica != null) sliderMusica.value = DataManager.Instancia.ajustes.volumenMusica;
            if (sliderEfectosSFX != null) sliderEfectosSFX.value = DataManager.Instancia.ajustes.volumenSFX;
        }

        if (togglePantallaCompleta != null)
        {
            togglePantallaCompleta.isOn = Screen.fullScreen;
        }

        if (dropdownCalidadGraficos != null)
        {
            dropdownCalidadGraficos.value = QualitySettings.GetQualityLevel();
        }
    }

    // Conecta las funciones con los elementos de la interfaz automáticamente
    private void ConectarEventosDeUI()
    {
        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.RemoveListener(AlCambiarVolumenMusica);
            sliderMusica.onValueChanged.AddListener(AlCambiarVolumenMusica);
        }

        if (sliderEfectosSFX != null)
        {
            sliderEfectosSFX.onValueChanged.RemoveListener(AlCambiarVolumenSFX);
            sliderEfectosSFX.onValueChanged.AddListener(AlCambiarVolumenSFX);
        }

        if (togglePantallaCompleta != null)
        {
            togglePantallaCompleta.onValueChanged.RemoveListener(AlCambiarPantallaCompleta);
            togglePantallaCompleta.onValueChanged.AddListener(AlCambiarPantallaCompleta);
        }

        if (dropdownCalidadGraficos != null)
        {
            dropdownCalidadGraficos.onValueChanged.RemoveListener(AlCambiarCalidad);
            dropdownCalidadGraficos.onValueChanged.AddListener(AlCambiarCalidad);
        }

        if (botonCerrarPanel != null)
        {
            botonCerrarPanel.onClick.RemoveListener(CerrarPanel);
            botonCerrarPanel.onClick.AddListener(CerrarPanel);
        }
    }

    // Se ejecuta al mover la barra de Música
    public void AlCambiarVolumenMusica(float nuevoVolumen)
    {
        // 1. Cambiamos el volumen en el mezclador de audio para escucharlo en vivo
        if (SoundManager.Instancia != null)
        {
            SoundManager.Instancia.SetVolumenMusica(nuevoVolumen);
        }

        // 2. Guardamos el nuevo valor en el archivo JSON
        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            DataManager.Instancia.ajustes.volumenMusica = nuevoVolumen;
            DataManager.Instancia.GuardarAjustes();
        }
    }

    // Se ejecuta al mover la barra de Efectos de Sonido (SFX)
    public void AlCambiarVolumenSFX(float nuevoVolumen)
    {
        if (SoundManager.Instancia != null)
        {
            SoundManager.Instancia.SetVolumenSFX(nuevoVolumen);
        }

        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            DataManager.Instancia.ajustes.volumenSFX = nuevoVolumen;
            DataManager.Instancia.GuardarAjustes();
        }
    }

    // Se ejecuta al activar o desactivar la casilla de Pantalla Completa
    public void AlCambiarPantallaCompleta(bool esPantallaCompleta)
    {
        Screen.fullScreen = esPantallaCompleta;
    }

    // Se ejecuta al cambiar la opción de calidad gráfica en la lista desplegable
    public void AlCambiarCalidad(int indiceCalidad)
    {
        QualitySettings.SetQualityLevel(indiceCalidad);
    }

    // Muestra el panel y refresca los datos
    public void AbrirPanel()
    {
        gameObject.SetActive(true);
        CargarValoresGuardados();
    }

    // Oculta el panel de ajustes
    public void CerrarPanel()
    {
        gameObject.SetActive(false);
    }
}
