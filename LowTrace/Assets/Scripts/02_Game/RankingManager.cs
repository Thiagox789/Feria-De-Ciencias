using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instancia { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelRanking;
    [SerializeField] private TextMeshProUGUI[] textosPosiciones;
    [SerializeField] private TextMeshProUGUI[] textosTiempos;
    [SerializeField] private TextMeshProUGUI textoMapaActual;

    [Header("Configuración")]
    [SerializeField] private int maximoEntradas = 10;
    [SerializeField] private string mapaFiltro = "";

    private List<RegistroTiempo> rankingActual = new List<RegistroTiempo>();

    public event System.Action OnRankingActualizado;

    [System.Serializable]
    public class RegistroTiempo
    {
        public string nombreJugador;
        public float tiempo;
        public string mapa;
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ActualizarRanking()
    {
        ActualizarUI();
        OnRankingActualizado?.Invoke();
    }

    public void ActualizarRanking(string mapa)
    {
        mapaFiltro = mapa;
        ActualizarRanking();
    }

    public void AgregarTiempo(string nombre, float tiempo, string mapa)
    {
        RegistroTiempo registro = new RegistroTiempo
        {
            nombreJugador = nombre,
            tiempo = tiempo,
            mapa = mapa
        };
        rankingActual.Add(registro);
        rankingActual.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));

        if (rankingActual.Count > maximoEntradas)
            rankingActual.RemoveRange(maximoEntradas, rankingActual.Count - maximoEntradas);

        ActualizarUI();
    }

    public List<RegistroTiempo> ObtenerRanking()
    {
        return new List<RegistroTiempo>(rankingActual);
    }

    public float ObtenerMejorTiempo()
    {
        if (rankingActual.Count == 0) return 0f;
        return rankingActual[0].tiempo;
    }

    private void ActualizarUI()
    {
        if (textoMapaActual != null && !string.IsNullOrEmpty(mapaFiltro))
            textoMapaActual.text = mapaFiltro;

        if (textosPosiciones == null || textosTiempos == null) return;

        for (int i = 0; i < textosPosiciones.Length; i++)
        {
            if (i < rankingActual.Count)
            {
                textosPosiciones[i].gameObject.SetActive(true);
                textosTiempos[i].gameObject.SetActive(true);

                textosPosiciones[i].text = (i + 1) + ". " + rankingActual[i].nombreJugador;
                textosTiempos[i].text = FormatearTiempo(rankingActual[i].tiempo);
            }
            else
            {
                textosPosiciones[i].gameObject.SetActive(false);
                textosTiempos[i].gameObject.SetActive(false);
            }
        }
    }

    public void MostrarRanking()
    {
        if (panelRanking != null)
            panelRanking.SetActive(true);
    }

    public void OcultarRanking()
    {
        if (panelRanking != null)
            panelRanking.SetActive(false);
    }

    public void LimpiarRanking()
    {
        rankingActual.Clear();
        ActualizarUI();
    }

    private string FormatearTiempo(float tiempo)
    {
        int min = (int)(tiempo / 60f);
        int seg = (int)(tiempo % 60f);
        int mili = (int)((tiempo - Mathf.Floor(tiempo)) * 1000f);
        return string.Format("{0:00}:{1:00}.{2:000}", min, seg, mili);
    }
}
