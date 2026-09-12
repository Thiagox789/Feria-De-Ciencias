using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Este script controla la interfaz de usuario durante la carrera (HUD).
// Muestra el cronómetro en pantalla, los checkpoints superados, la vuelta actual,
// la pantalla de victoria al cruzar la meta y permite guardar el récord.
public class UIManager : MonoBehaviour
{
    [Header("Cronómetro y Datos de Carrera (HUD)")]
    [SerializeField] private TextMeshProUGUI textoTiempoCronometro;
    [SerializeField] private TextMeshProUGUI textoDiferenciaConRecord;
    [SerializeField] private TextMeshProUGUI textoRecordHUD;
    [SerializeField] private TextMeshProUGUI textoCheckpoints;
    [SerializeField] private TextMeshProUGUI textoNumeroVuelta;

    [Header("Estado de la Carrera")]
    [SerializeField] private TextMeshProUGUI textoEstadoMensaje;

    [Header("Pantalla de Victoria (Al cruzar la meta)")]
    [SerializeField] private GameObject[] elementosAOcultarAlGanar;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private TextMeshProUGUI textoTiempoFinal;
    [SerializeField] private TextMeshProUGUI textoMejorRecord;
    [SerializeField] private TextMeshProUGUI textoDiferenciaFinal;

    [Header("Guardar Récord en Ranking")]
    [SerializeField] private TMP_InputField inputNombreJugador;
    [SerializeField] private Button botonGuardarRanking;
    [SerializeField] private TextMeshProUGUI textoPosicionRanking;
    [SerializeField] private GameObject panelRankingGuardado;

    private float tiempoFinalCarrera;

    private void Awake()
    {
        // Nos suscribimos a los eventos del GameManager
        GameManager.OnEstadoCambio += MostrarMensajeEstado;
        GameManager.OnCarreraTerminada += MostrarPantallaVictoria;

        if (botonGuardarRanking != null)
        {
            botonGuardarRanking.onClick.AddListener(GuardarTiempoEnRanking);
        }
    }

    private void OnDestroy()
    {
        GameManager.OnEstadoCambio -= MostrarMensajeEstado;
        GameManager.OnCarreraTerminada -= MostrarPantallaVictoria;
    }

    private void Start()
    {
        ActualizarTextoRecord();
    }

    private void Update()
    {
        if (GameManager.Instancia == null) return;

        // Actualizamos el tiempo en pantalla cada segundo
        if (textoTiempoCronometro != null)
        {
            textoTiempoCronometro.text = FormatearTiempo(GameManager.Instancia.TiempoCarrera);
        }

        ActualizarDiferenciaConRecord();
        ActualizarTextoCheckpoints();
        ActualizarTextoVuelta();
        ProcesarTeclasAccesoRapido();
    }

    private string ObtenerNombreMapaActual()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    private void ActualizarTextoCheckpoints()
    {
        if (textoCheckpoints == null || GameManager.Instancia == null) return;

        textoCheckpoints.text = GameManager.Instancia.CheckpointsCompletados + "/" +
                                GameManager.Instancia.CheckpointsTotales;
    }

    private void ActualizarTextoVuelta()
    {
        if (textoNumeroVuelta == null || GameManager.Instancia == null) return;

        textoNumeroVuelta.text = GameManager.Instancia.VueltaActual + "/" +
                                 GameManager.Instancia.VueltasTotales;
    }

    private void ProcesarTeclasAccesoRapido()
    {
        if (Keyboard.current == null) return;

        // Si se presiona R o Espacio se reinicia la carrera
        if (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ReintentarCarrera();
        }

        // Si terminó la carrera y se presiona Escape, vuelve al Menú
        if (GameManager.Instancia.Estado == GameManager.EstadoJuego.Terminado)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                VolverAlMenu();
            }
        }
    }

    private void ActualizarTextoRecord()
    {
        string mapa = ObtenerNombreMapaActual();
        float mejorTiempo = 0f;

        if (DataManager.Instancia != null)
        {
            mejorTiempo = DataManager.Instancia.ObtenerMejorTiempoPorMapa(mapa);
        }
        if (mejorTiempo <= 0f && GameManager.Instancia != null)
        {
            mejorTiempo = GameManager.Instancia.MejorTiempo;
        }

        string recordFormateado = mejorTiempo > 0f ? FormatearTiempo(mejorTiempo) : "--:--.---";

        if (textoMejorRecord != null)
        {
            textoMejorRecord.text = recordFormateado;
        }

        if (textoRecordHUD != null)
        {
            textoRecordHUD.text = recordFormateado;
        }
    }

    private void ActualizarDiferenciaConRecord()
    {
        if (GameManager.Instancia == null) return;

        ActualizarTextoRecord();

        float tiempoActual = GameManager.Instancia.TiempoCarrera;
        string mapa = ObtenerNombreMapaActual();

        float mejorTiempo = 0f;
        if (DataManager.Instancia != null)
        {
            mejorTiempo = DataManager.Instancia.ObtenerMejorTiempoPorMapa(mapa);
        }
        if (mejorTiempo <= 0f && GameManager.Instancia != null)
        {
            mejorTiempo = GameManager.Instancia.MejorTiempo;
        }

        if (textoDiferenciaConRecord == null) return;

        if (mejorTiempo <= 0f)
        {
            textoDiferenciaConRecord.text = "--:--.---";
            return;
        }

        float diferencia = tiempoActual - mejorTiempo;
        if (diferencia > 0)
        {
            textoDiferenciaConRecord.text = "+" + FormatearTiempo(diferencia);
        }
        else if (diferencia < 0)
        {
            textoDiferenciaConRecord.text = "-" + FormatearTiempo(Mathf.Abs(diferencia));
        }
        else
        {
            textoDiferenciaConRecord.text = "00:00.000";
        }
    }

    private void MostrarMensajeEstado(GameManager.EstadoJuego estado)
    {
        if (textoEstadoMensaje == null) return;

        switch (estado)
        {
            case GameManager.EstadoJuego.Espera:
                textoEstadoMensaje.text = "¡PREPARATE!";
                break;
            case GameManager.EstadoJuego.Carrera:
                textoEstadoMensaje.text = "¡CORRIENDO!";
                break;
            case GameManager.EstadoJuego.Terminado:
                textoEstadoMensaje.text = "¡META!";
                break;
        }
    }

    private void MostrarPantallaVictoria(float tiempoFinal)
    {
        tiempoFinalCarrera = tiempoFinal;

        if (elementosAOcultarAlGanar != null)
        {
            foreach (GameObject objeto in elementosAOcultarAlGanar)
            {
                if (objeto != null) objeto.SetActive(false);
            }
        }

        if (panelVictoria != null) panelVictoria.SetActive(true);

        if (textoTiempoFinal != null)
        {
            textoTiempoFinal.text = FormatearTiempo(tiempoFinal);
        }

        string mapa = MapSelectionManager.Instancia != null 
            ? MapSelectionManager.Instancia.ObtenerNombreEscenaActual() 
            : "Mapa1";

        float mejorTiempo = 0f;
        if (DataManager.Instancia != null)
        {
            mejorTiempo = DataManager.Instancia.ObtenerMejorTiempoPorMapa(mapa);
        }

        if (textoMejorRecord != null)
        {
            textoMejorRecord.text = mejorTiempo > 0f ? FormatearTiempo(mejorTiempo) : FormatearTiempo(tiempoFinal);
        }

        if (textoDiferenciaFinal != null)
        {
            if (mejorTiempo <= 0f)
            {
                textoDiferenciaFinal.text = "¡Nuevo Récord!";
            }
            else
            {
                float diferencia = tiempoFinal - mejorTiempo;
                if (diferencia > 0)
                    textoDiferenciaFinal.text = "+" + FormatearTiempo(diferencia);
                else if (diferencia < 0)
                    textoDiferenciaFinal.text = "-" + FormatearTiempo(Mathf.Abs(diferencia));
                else
                    textoDiferenciaFinal.text = "00:00.000";
            }
        }

        if (panelRankingGuardado != null)
        {
            panelRankingGuardado.SetActive(false);
        }
    }

    private void GuardarTiempoEnRanking()
    {
        if (DataManager.Instancia == null) return;

        string nombreJugador = "Jugador";
        if (inputNombreJugador != null && !string.IsNullOrEmpty(inputNombreJugador.text))
        {
            nombreJugador = inputNombreJugador.text.Trim();
        }

        string mapa = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        DataManager.Instancia.AgregarAlRanking(nombreJugador, tiempoFinalCarrera, mapa);
        DataManager.Instancia.IntentarNuevoRecord(tiempoFinalCarrera);

        int posicion = DataManager.Instancia.ObtenerPosicionEnRanking(tiempoFinalCarrera, mapa);

        if (panelRankingGuardado != null)
        {
            panelRankingGuardado.SetActive(true);
        }

        if (textoPosicionRanking != null)
        {
            if (posicion <= 3)
                textoPosicionRanking.text = "¡TOP " + posicion + "!";
            else
                textoPosicionRanking.text = "Posición #" + posicion;
        }

        if (botonGuardarRanking != null)
        {
            botonGuardarRanking.interactable = false;
        }
    }

    public void ReintentarCarrera()
    {
        if (SceneLoader.Instancia != null)
        {
            SceneLoader.Instancia.CargarEscenaActual();
        }
    }

    public void VolverAlMenu()
    {
        if (SceneLoader.Instancia != null)
        {
            SceneLoader.Instancia.VolverAlMenu();
        }
    }

    // Convierte segundos numéricos a un formato legible de minutos:segundos.milisegundos (01:23.456)
    public static string FormatearTiempo(float tiempoSegundos)
    {
        int minutos = (int)(tiempoSegundos / 60f);
        int segundos = (int)(tiempoSegundos % 60f);
        int milisegundos = (int)((tiempoSegundos - Mathf.Floor(tiempoSegundos)) * 1000f);
        return string.Format("{0:00}:{1:00}.{2:000}", minutos, segundos, milisegundos);
    }
}