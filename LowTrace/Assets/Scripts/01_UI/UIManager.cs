using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("Cronometro (HUD)")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoDiferencia;
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

    private void Awake()
    {
        GameManager.OnEstadoCambio += MostrarEstado;
        GameManager.OnCarreraTerminada += MostrarPantallaVictoria;
    }

    private void OnDestroy()
    {
        GameManager.OnEstadoCambio -= MostrarEstado;
        GameManager.OnCarreraTerminada -= MostrarPantallaVictoria;
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

    private void ActualizarDiferencia()
    {
        if (textoDiferencia == null || GameManager.Instancia == null) return;

        float tiempoActual = GameManager.Instancia.TiempoCarrera;
        float record = GameManager.Instancia.MejorTiempo;

        if (record <= 0f)
        {
            textoDiferencia.text = "0";
            return;
        }

        float diferencia = Mathf.Abs(record - tiempoActual);
        textoDiferencia.text = Formatear(diferencia);
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

        if (textoRecord != null && GameManager.Instancia != null)
        {
            textoRecord.text = GameManager.Instancia.MejorTiempo > 0f
                ? Formatear(GameManager.Instancia.MejorTiempo)
                : "0";
        }

        if (textoDiferenciaFinal != null && GameManager.Instancia != null)
        {
            float diferencia = Mathf.Abs(GameManager.Instancia.MejorTiempo - tiempoFinal);
            textoDiferenciaFinal.text = diferencia > 0f ? Formatear(diferencia) : "0";
        }
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