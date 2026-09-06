using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("Cronometro (HUD)")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoDiferencia;
    [SerializeField] private TextMeshProUGUI textoRecordHUD;
    [SerializeField] private TextMeshProUGUI textoCheckpoints;
    [SerializeField] private TextMeshProUGUI textoVuelta;

    [Header("Estado")]
    [SerializeField] private TextMeshProUGUI textoEstado;

    [Header("Pantalla de victoria")]
    [SerializeField] private GameObject[] objetosAOcultar;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private TextMeshProUGUI textoTiempoFinal;
    [SerializeField] private TextMeshProUGUI textoRecord;
    [SerializeField] private TextMeshProUGUI textoDiferenciaFinal;

    [Header("Ranking")]
    [SerializeField] private TMP_InputField inputNombreJugador;
    [SerializeField] private Button botonGuardarRanking;
    [SerializeField] private TextMeshProUGUI textoPosicionRanking;
    [SerializeField] private GameObject panelRankingGuardado;

    private float tiempoFinalCarrera;

    private void Awake()
    {
        GameManager.OnEstadoCambio += MostrarEstado;
        GameManager.OnCarreraTerminada += MostrarPantallaVictoria;

        if (botonGuardarRanking != null)
            botonGuardarRanking.onClick.AddListener(GuardarEnRanking);
    }

    private void OnDestroy()
    {
        GameManager.OnEstadoCambio -= MostrarEstado;
        GameManager.OnCarreraTerminada -= MostrarPantallaVictoria;
    }

    private void Start()
    {
        ActualizarTextoRecord();
    }

    private void Update()
    {
        if (GameManager.Instancia == null) return;

        if (textoTiempo != null)
        {
            textoTiempo.text = Formatear(GameManager.Instancia.TiempoCarrera);
        }

        ActualizarDiferencia();
        ActualizarCheckpoints();
        ActualizarVuelta();
        ProcesarTeclas();
    }

    private string ObtenerNombreMapaActual()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    private void ActualizarCheckpoints()
    {
        if (textoCheckpoints == null || GameManager.Instancia == null) return;

        textoCheckpoints.text = GameManager.Instancia.CheckpointsCompletados + "/" +
                                GameManager.Instancia.CheckpointsTotales;
    }

    private void ActualizarVuelta()
    {
        if (textoVuelta == null || GameManager.Instancia == null) return;

        textoVuelta.text = GameManager.Instancia.VueltaActual + "/" +
                           GameManager.Instancia.VueltasTotales;
    }

    private void ProcesarTeclas()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Reintentar();
        }

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

        string recordString = mejorTiempo > 0f ? Formatear(mejorTiempo) : "--:--.---";

        if (textoRecord != null)
        {
            textoRecord.text = recordString;
        }

        if (textoRecordHUD != null)
        {
            textoRecordHUD.text = recordString;
        }
    }

    private void ActualizarDiferencia()
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

        if (textoDiferencia == null) return;

        if (mejorTiempo <= 0f)
        {
            textoDiferencia.text = "--:--.---";
            return;
        }

        float diff = tiempoActual - mejorTiempo;
        if (diff > 0)
        {
            textoDiferencia.text = "+" + Formatear(diff);
        }
        else if (diff < 0)
        {
            textoDiferencia.text = "-" + Formatear(Mathf.Abs(diff));
        }
        else
        {
            textoDiferencia.text = "00:00.000";
        }
    }

    private void MostrarEstado(GameManager.EstadoJuego estado)
    {
        if (textoEstado == null) return;

        switch (estado)
        {
            case GameManager.EstadoJuego.Espera:
                textoEstado.text = "PREPARATE";
                break;
            case GameManager.EstadoJuego.Carrera:
                textoEstado.text = "CORRIENDO";
                break;
            case GameManager.EstadoJuego.Terminado:
                textoEstado.text = "META";
                break;
        }
    }

    private void MostrarPantallaVictoria(float tiempoFinal)
    {
        tiempoFinalCarrera = tiempoFinal;

        if (objetosAOcultar != null)
        {
            foreach (GameObject obj in objetosAOcultar)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        if (panelVictoria != null) panelVictoria.SetActive(true);

        if (textoTiempoFinal != null)
        {
            textoTiempoFinal.text = Formatear(tiempoFinal);
        }

        string mapa = MapSelectionManager.Instancia != null 
            ? MapSelectionManager.Instancia.ObtenerNombreEscenaActual() 
            : "Mapa1";

        float mejorTiempo = 0f;
        if (DataManager.Instancia != null)
        {
            mejorTiempo = DataManager.Instancia.ObtenerMejorTiempoPorMapa(mapa);
        }

        if (textoRecord != null)
        {
            textoRecord.text = mejorTiempo > 0f ? Formatear(mejorTiempo) : Formatear(tiempoFinal);
        }

        if (textoDiferenciaFinal != null)
        {
            if (mejorTiempo <= 0f)
            {
                textoDiferenciaFinal.text = "¡Nuevo Récord!";
            }
            else
            {
                float diff = tiempoFinal - mejorTiempo;
                if (diff > 0)
                    textoDiferenciaFinal.text = "+" + Formatear(diff);
                else if (diff < 0)
                    textoDiferenciaFinal.text = "-" + Formatear(Mathf.Abs(diff));
                else
                    textoDiferenciaFinal.text = "00:00.000";
            }
        }

        if (panelRankingGuardado != null)
            panelRankingGuardado.SetActive(false);
    }

    private void GuardarEnRanking()
    {
        if (DataManager.Instancia == null) return;

        string nombre = "Jugador";
        if (inputNombreJugador != null && !string.IsNullOrEmpty(inputNombreJugador.text))
        {
            nombre = inputNombreJugador.text.Trim();
        }

        string mapa = "Mapa1";
        if (MapSelectionManager.Instancia != null)
            mapa = MapSelectionManager.Instancia.ObtenerNombreEscenaActual();

        DataManager.Instancia.AgregarAlRanking(nombre, tiempoFinalCarrera, mapa);
        DataManager.Instancia.IntentarNuevoRecord(tiempoFinalCarrera);

        int posicion = DataManager.Instancia.ObtenerPosicionEnRanking(tiempoFinalCarrera, mapa);

        if (panelRankingGuardado != null)
            panelRankingGuardado.SetActive(true);

        if (textoPosicionRanking != null)
        {
            if (posicion <= 3)
                textoPosicionRanking.text = "TOP " + posicion + "!";
            else
                textoPosicionRanking.text = "Posicion #" + posicion;
        }

        if (botonGuardarRanking != null)
            botonGuardarRanking.interactable = false;
    }

    public void Reintentar()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.CargarEscenaActual();
    }

    public void VolverAlMenu()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.VolverAlMenu();
    }

    public static string Formatear(float t)
    {
        int min = (int)(t / 60f);
        int seg = (int)(t % 60f);
        int mili = (int)((t - Mathf.Floor(t)) * 1000f);
        return string.Format("{0:00}:{1:00}.{2:000}", min, seg, mili);
    }
}