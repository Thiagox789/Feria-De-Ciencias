using UnityEngine;
using System.Collections.Generic;

// Este es el Gestor Principal de la Carrera (GameManager).
// Controla el estado del juego (En Espera, En Carrera, Terminado),
// mide el tiempo del cronómetro, valida los checkpoints y cuenta las vueltas.
public class GameManager : MonoBehaviour
{
    // Estados posibles del juego
    public enum EstadoJuego 
    { 
        Espera,     // Esperando a que el auto acelere
        Carrera,    // Carrera en curso (cronómetro activo)
        Terminado   // El auto cruzó la meta final
    }

    // Singleton para acceder a GameManager.Instancia desde cualquier script
    public static GameManager Instancia { get; private set; }

    public EstadoJuego Estado { get; private set; } = EstadoJuego.Espera;
    public float TiempoCarrera { get; private set; }
    public float MejorTiempo { get; private set; }
    public int VueltaActual { get; private set; }
    public int VueltasTotales { get; private set; } = 1;

    [Header("Configuración de Vueltas")]
    [SerializeField] private int vueltasTotales = 1;
    public bool VueltaCompleta { get; private set; }

    // Eventos para notificar a la UI cuando cambia el estado del juego
    public static event System.Action<EstadoJuego> OnEstadoCambio;
    public static event System.Action<float> OnCarreraTerminada;
    public static event System.Action<int> OnVueltaCompletada;

    [Header("Referencias en la Escena")]
    [SerializeField] private Transform autoJugador;
    [SerializeField] private float limiteCaidaVacio = -3f; // Si el auto cae al vacío reabre la escena
    [SerializeField] private Checkpoint[] listaCheckpoints;

    private HashSet<int> checkpointsTocados = new HashSet<int>();

    public int CheckpointsCompletados => checkpointsTocados.Count;
    public int CheckpointsTotales => listaCheckpoints != null ? listaCheckpoints.Length : 0;

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

    private void Update()
    {
        if (Estado == EstadoJuego.Carrera)
        {
            TiempoCarrera += Time.deltaTime;

            // Si el auto se cae de la pista al vacío, reiniciamos la carrera
            if (autoJugador != null && autoJugador.position.y < limiteCaidaVacio)
            {
                ReiniciarPorCaida();
            }
        }
    }

    // Inicia el cronómetro cuando el jugador presiona las teclas de acelerar por primera vez
    public void IniciarCarrera()
    {
        TiempoCarrera = 0f;
        checkpointsTocados.Clear();
        CambiarEstado(EstadoJuego.Carrera);
    }

    // Registra un checkpoint cuando el auto pasa a través de él
    public void RegistrarCheckpoint(Checkpoint checkpoint)
    {
        if (Estado != EstadoJuego.Carrera) return;
        if (checkpoint == null || listaCheckpoints == null) return;

        int indice = System.Array.IndexOf(listaCheckpoints, checkpoint);
        if (indice < 0) return;

        if (checkpointsTocados.Contains(indice)) return;

        checkpointsTocados.Add(indice);
        Debug.Log($"[GameManager] Checkpoint {indice} superado. Progreso: {checkpointsTocados.Count}/{listaCheckpoints.Length}");

        // Si ya pasó por todos los checkpoints de la pista, habilita poder cruzar la meta
        if (checkpointsTocados.Count >= listaCheckpoints.Length)
        {
            VueltaCompleta = true;
            OnVueltaCompletada?.Invoke(VueltaActual);
        }
    }

    // Se llama cuando el auto toca la línea de meta
    public void CruzarMeta()
    {
        if (Estado != EstadoJuego.Carrera) return;

        // Si no completó todos los checkpoints previamente, no le cuenta la meta (evita trampas)
        if (!VueltaCompleta) return;

        VueltaCompleta = false;
        VueltaActual++;

        if (VueltaActual >= VueltasTotales)
        {
            TerminarCarrera();
        }
    }

    // Finaliza la carrera y detiene el tiempo
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

    // Si el auto cae al vacío reinicia la escena
    public void ReiniciarPorCaida()
    {
        if (Estado != EstadoJuego.Carrera) return;
        if (SceneLoader.Instancia != null)
        {
            SceneLoader.Instancia.CargarEscenaActual();
        }
    }

    public void Castigo() => ReiniciarPorCaida();

    private void CambiarEstado(EstadoJuego nuevoEstado)
    {
        Estado = nuevoEstado;
        OnEstadoCambio?.Invoke(nuevoEstado);
    }
}
