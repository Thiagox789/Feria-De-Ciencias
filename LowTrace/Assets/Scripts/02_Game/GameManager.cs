using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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

    public static event System.Action<EstadoJuego> OnEstadoCambio;
    public static event System.Action<float> OnCarreraTerminada;
    public static event System.Action<int> OnVueltaCompletada;

    [Header("Referencias")]
    [SerializeField] private Transform auto;
    [SerializeField] private float umbralCaida = -3f;
    [SerializeField] private Checkpoint[] checkpoints;

    private HashSet<int> checkpointsTocados = new HashSet<int>();

    public int CheckpointsCompletados => checkpointsTocados.Count;
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
        if (indice < 0) return;

        if (checkpointsTocados.Contains(indice)) return;

        checkpointsTocados.Add(indice);
        Debug.Log($"[GameManager] Checkpoint {indice} registrado. Total: {checkpointsTocados.Count}/{checkpoints.Length}");

        if (checkpointsTocados.Count >= checkpoints.Length)
        {
            VueltaCompleta = true;
            Debug.Log("[GameManager] ¡Todos los checkpoints completados! VueltaCompleta = true");
            OnVueltaCompletada?.Invoke(VueltaActual);
        }
    }

    public void CruzarMeta()
    {
        Debug.Log($"[GameManager] CruzarMeta llamado. Estado={Estado}, VueltaCompleta={VueltaCompleta}");
        if (Estado != EstadoJuego.Carrera) return;

        if (!VueltaCompleta) return;

        VueltaCompleta = false;

        VueltaActual++;

        if (VueltaActual >= VueltasTotales)
        {
            Debug.Log("[GameManager] Terminando carrera...");
            TerminarCarrera();
        }
    }

    private void Update()
    {
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
        checkpointsTocados.Clear();
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
        if (SceneLoader.Instancia != null)
            SceneLoader.Instancia.CargarEscenaActual();
    }

    private void CambiarEstado(EstadoJuego nuevo)
    {
        Estado = nuevo;
        OnEstadoCambio?.Invoke(nuevo);
    }
}
