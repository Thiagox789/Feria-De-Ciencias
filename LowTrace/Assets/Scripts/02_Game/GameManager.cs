using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum EstadoJuego { Espera, Carrera, Terminado }

    public static GameManager Instancia { get; private set; }

    public EstadoJuego Estado { get; private set; } = EstadoJuego.Espera;
    public float TiempoCarrera { get; private set; }
    public float MejorTiempo { get; private set; }
    public int VueltaActual { get; private set; }
    public int VueltasTotales { get; private set; } = 1;

    [Header("Vueltas")]
    [SerializeField] private int vueltasTotales = 1;
    public bool VueltaCompleta { get; private set; }
    private int siguienteCheckpoint;

    public static event System.Action<EstadoJuego> OnEstadoCambio;
    public static event System.Action<float> OnCarreraTerminada;
    public static event System.Action<int> OnVueltaCompletada;

    [Header("Referencias")]
    [SerializeField] private Transform auto;
    [SerializeField] private float umbralCaida = -3f;
    [SerializeField] private Checkpoint[] checkpoints;

    private int checkpointsPisados;

    public int CheckpointsCompletados => checkpointsPisados;
    public int CheckpointsTotales => checkpoints.Length;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        VueltasTotales = vueltasTotales;
    }

    public void RegistrarCheckpoint(Checkpoint checkpoint)
    {
        if (Estado != EstadoJuego.Carrera) return;
        if (checkpoint == null) return;

        int indice = System.Array.IndexOf(checkpoints, checkpoint);

        if (indice < 0 || indice != siguienteCheckpoint) return;

        siguienteCheckpoint++;
        checkpointsPisados++;

        if (siguienteCheckpoint >= checkpoints.Length)
        {
            VueltaCompleta = true;
            OnVueltaCompletada?.Invoke(VueltaActual);
        }
    }

    public void CruzarMeta()
    {
        if (Estado != EstadoJuego.Carrera) return;

        if (!VueltaCompleta) return;

        VueltaCompleta = false;

        VueltaActual++;

        if (VueltaActual >= VueltasTotales)
        {
            TerminarCarrera();
        }
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.rKey.wasPressedThisFrame)
            {
                Reintentar();
                return;
            }
            if (kb.escapeKey.wasPressedThisFrame)
            {
                VolverAlMenu();
                return;
            }
        }

        if (Estado == EstadoJuego.Carrera)
        {
            TiempoCarrera += Time.deltaTime;

            if (auto != null && auto.position.y < umbralCaida)
            {
                Castigo();
            }
        }
    }

    public void IniciarCarrera()
    {
        TiempoCarrera = 0f;
        CambiarEstado(EstadoJuego.Carrera);
    }

    public void TerminarCarrera()
    {
        if (Estado != EstadoJuego.Carrera) return;

        if (MejorTiempo == 0f || TiempoCarrera < MejorTiempo)
        {
            MejorTiempo = TiempoCarrera;
        }

        CambiarEstado(EstadoJuego.Terminado);
        OnCarreraTerminada?.Invoke(TiempoCarrera);
    }

    public void Castigo()
    {
        if (Estado != EstadoJuego.Carrera) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Reintentar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    private void CambiarEstado(EstadoJuego nuevo)
    {
        Estado = nuevo;
        OnEstadoCambio?.Invoke(nuevo);
    }
}