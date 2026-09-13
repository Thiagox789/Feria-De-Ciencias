using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO.Ports;

// Este script controla la pantalla o panel de Ajustes del juego.
// Permite modificar el volumen, monitores, resolución, pantalla completa y puerto serial.
public class SettingsPanelUI : MonoBehaviour
{
    [Header("Barritas de Volumen (Sliders)")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderEfectosSFX;

    [Header("Pantalla")]
    [SerializeField] private TMP_Dropdown dropdownMonitor;
    [SerializeField] private TMP_Dropdown dropdownMonitorRanking;
    [SerializeField] private TMP_Dropdown dropdownResolucion;
    [SerializeField] private Toggle togglePantallaCompleta;
    [SerializeField] private TMP_Dropdown dropdownCalidadGraficos;

    [Header("Puerto Serial (Volante)")]
    [SerializeField] private TMP_Dropdown dropdownPuertoSerial;
    [SerializeField] private TMP_Text textoEstadoSerial;

    [Header("Botones")]
    [SerializeField] private Button botonCerrarPanel;

    private Resolution[] resolucionesDisponibles;
    private string[] puertosDisponibles;

    protected virtual void Start()
    {
        LlenarDropdowns();
        CargarValoresGuardados();
        ConectarEventosDeUI();
    }

    // ==========================================
    // LLENADO DE DROPDOWNS
    // ==========================================
    private void LlenarDropdowns()
    {
        LlenarDropdownMonitores();
        LlenarDropdownResoluciones();
        LlenarDropdownPuertosSeriales();
    }

    private void LlenarDropdownMonitores()
    {
        var opciones = new System.Collections.Generic.List<string>();

        // Aseguramos que siempre existan al menos las opciones Monitor 1 y Monitor 2 en el desplegable
        int cantidadMonitores = Mathf.Max(2, Display.displays.Length);

        for (int i = 0; i < cantidadMonitores; i++)
        {
            opciones.Add("Monitor " + (i + 1));
        }

        if (dropdownMonitor != null)
        {
            dropdownMonitor.ClearOptions();
            dropdownMonitor.AddOptions(opciones);
        }

        if (dropdownMonitorRanking != null)
        {
            dropdownMonitorRanking.ClearOptions();
            dropdownMonitorRanking.AddOptions(opciones);
        }
    }

    private void LlenarDropdownResoluciones()
    {
        if (dropdownResolucion == null) return;

        dropdownResolucion.ClearOptions();
        resolucionesDisponibles = Screen.resolutions;
        var opciones = new System.Collections.Generic.List<string>();

        for (int i = 0; i < resolucionesDisponibles.Length; i++)
        {
            Resolution r = resolucionesDisponibles[i];
            opciones.Add(r.width + " x " + r.height + " @ " + r.refreshRateRatio.value + "Hz");
        }

        dropdownResolucion.AddOptions(opciones);
    }

    private void LlenarDropdownPuertosSeriales()
    {
        if (dropdownPuertoSerial == null) return;

        dropdownPuertoSerial.ClearOptions();
        puertosDisponibles = SerialPort.GetPortNames();
        var opciones = new System.Collections.Generic.List<string>();

        opciones.Add("Ninguno");
        for (int i = 0; i < puertosDisponibles.Length; i++)
        {
            opciones.Add(puertosDisponibles[i]);
        }

        dropdownPuertoSerial.AddOptions(opciones);
    }

    // ==========================================
    // CARGA DE VALORES GUARDADOS
    // ==========================================
    public void CargarValoresGuardados()
    {
        if (DataManager.Instancia == null || DataManager.Instancia.ajustes == null) return;

        var ajustes = DataManager.Instancia.ajustes;

        if (sliderMusica != null) sliderMusica.value = ajustes.volumenMusica;
        if (sliderEfectosSFX != null) sliderEfectosSFX.value = ajustes.volumenSFX;

        if (dropdownMonitor != null)
        {
            int idx = ajustes.indiceMonitorRanking;
            if (idx >= 0 && idx < dropdownMonitor.options.Count)
            {
                dropdownMonitor.value = idx;
            }
        }

        if (dropdownResolucion != null && dropdownResolucion.options.Count > ajustes.indiceResolucion)
        {
            dropdownResolucion.value = ajustes.indiceResolucion;
        }

        if (togglePantallaCompleta != null)
        {
            togglePantallaCompleta.isOn = ajustes.pantallaCompleta;
        }

        if (dropdownCalidadGraficos != null)
        {
            dropdownCalidadGraficos.value = QualitySettings.GetQualityLevel();
        }

        if (dropdownPuertoSerial != null)
        {
            int indicePuerto = 0;
            if (!string.IsNullOrEmpty(ajustes.puertoSerial))
            {
                for (int i = 0; i < puertosDisponibles.Length; i++)
                {
                    if (puertosDisponibles[i] == ajustes.puertoSerial)
                    {
                        indicePuerto = i + 1;
                        break;
                    }
                }
            }
            dropdownPuertoSerial.value = indicePuerto;
        }

        ActualizarEstadoSerial();
    }

    // ==========================================
    // CONEXIÓN DE EVENTOS
    // ==========================================
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

        if (dropdownMonitor != null)
        {
            dropdownMonitor.onValueChanged.RemoveListener(AlCambiarMonitor);
            dropdownMonitor.onValueChanged.AddListener(AlCambiarMonitor);
        }

        if (dropdownMonitorRanking != null)
        {
            dropdownMonitorRanking.onValueChanged.RemoveListener(AlCambiarMonitor);
            dropdownMonitorRanking.onValueChanged.AddListener(AlCambiarMonitor);
        }

        if (dropdownResolucion != null)
        {
            dropdownResolucion.onValueChanged.RemoveListener(AlCambiarResolucion);
            dropdownResolucion.onValueChanged.AddListener(AlCambiarResolucion);
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

        if (dropdownPuertoSerial != null)
        {
            dropdownPuertoSerial.onValueChanged.RemoveListener(AlCambiarPuertoSerial);
            dropdownPuertoSerial.onValueChanged.AddListener(AlCambiarPuertoSerial);
        }

        if (botonCerrarPanel != null)
        {
            botonCerrarPanel.onClick.RemoveListener(CerrarPanel);
            botonCerrarPanel.onClick.AddListener(CerrarPanel);
        }
    }

    // ==========================================
    // CALLBACKS DE UI
    // ==========================================
    public void AlCambiarVolumenMusica(float nuevoVolumen)
    {
        if (SoundManager.Instancia != null)
            SoundManager.Instancia.SetVolumenMusica(nuevoVolumen);

        GuardarAjusteEnDisco();
    }

    public void AlCambiarVolumenSFX(float nuevoVolumen)
    {
        if (SoundManager.Instancia != null)
            SoundManager.Instancia.SetVolumenSFX(nuevoVolumen);

        GuardarAjusteEnDisco();
    }

    public void AlCambiarMonitor(int indice)
    {
        if (indice < 0 || indice >= Display.displays.Length) return;

        // Activar el display seleccionado
        for (int i = 0; i < Display.displays.Length; i++)
        {
            if (i == indice)
            {
                Display.displays[i].Activate();
            }
        }

        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            DataManager.Instancia.ajustes.indiceMonitorRanking = indice;
        }

        GuardarAjusteEnDisco();
    }

    public void AlCambiarResolucion(int indice)
    {
        if (resolucionesDisponibles == null || indice < 0 || indice >= resolucionesDisponibles.Length) return;

        Resolution r = resolucionesDisponibles[indice];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);

        GuardarAjusteEnDisco();
    }

    public void AlCambiarPantallaCompleta(bool esPantallaCompleta)
    {
        Screen.fullScreen = esPantallaCompleta;
        GuardarAjusteEnDisco();
    }

    public void AlCambiarCalidad(int indiceCalidad)
    {
        QualitySettings.SetQualityLevel(indiceCalidad);
    }

    public void AlCambiarPuertoSerial(int indice)
    {
        string puerto = "";

        if (indice > 0 && puertosDisponibles != null && indice - 1 < puertosDisponibles.Length)
        {
            puerto = puertosDisponibles[indice - 1];
        }

        ActualizarEstadoSerial();
        GuardarAjusteEnDisco();
    }

    // ==========================================
    // UTILIDADES
    // ==========================================
    private void GuardarAjusteEnDisco()
    {
        if (DataManager.Instancia == null || DataManager.Instancia.ajustes == null) return;

        var ajustes = DataManager.Instancia.ajustes;

        ajustes.volumenMusica = sliderMusica != null ? sliderMusica.value : ajustes.volumenMusica;
        ajustes.volumenSFX = sliderEfectosSFX != null ? sliderEfectosSFX.value : ajustes.volumenSFX;
        ajustes.indiceMonitorRanking = dropdownMonitor != null ? dropdownMonitor.value : ajustes.indiceMonitorRanking;
        ajustes.indiceMonitorPrincipal = 0; // El juego principal siempre se ejecuta en Monitor 1
        ajustes.indiceResolucion = dropdownResolucion != null ? dropdownResolucion.value : ajustes.indiceResolucion;
        ajustes.pantallaCompleta = togglePantallaCompleta != null ? togglePantallaCompleta.isOn : ajustes.pantallaCompleta;

        if (dropdownPuertoSerial != null && puertosDisponibles != null)
        {
            int idx = dropdownPuertoSerial.value - 1;
            ajustes.puertoSerial = (idx >= 0 && idx < puertosDisponibles.Length) ? puertosDisponibles[idx] : "";
        }

        DataManager.Instancia.GuardarAjustes();
    }

    private void ActualizarEstadoSerial()
    {
        if (textoEstadoSerial == null) return;

        if (dropdownPuertoSerial == null || dropdownPuertoSerial.value == 0)
        {
            textoEstadoSerial.text = "Sin conexión";
        }
        else
        {
            textoEstadoSerial.text = "Conectado: " + dropdownPuertoSerial.options[dropdownPuertoSerial.value].text;
        }
    }

    public void AbrirPanel()
    {
        LlenarDropdowns();
        CargarValoresGuardados();
        gameObject.SetActive(true);
    }

    public void CerrarPanel()
    {
        gameObject.SetActive(false);
    }
}
