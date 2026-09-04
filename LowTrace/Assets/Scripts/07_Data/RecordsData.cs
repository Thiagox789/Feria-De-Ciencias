using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NuevosRecords", menuName = "Datos/Records")]
public class RecordsData : ScriptableObject
{
    [Header("Récord personal (mejor tiempo)")]
    public float mejorTiempo = 9999f;

    [Header("Ranking por mapa")]
    public List<EntradaRanking> rankingGlobal = new List<EntradaRanking>();

    [System.Serializable]
    public class EntradaRanking
    {
        public string nombreJugador;
        public float tiempo;
        public string mapa;
        public string fecha;
    }

    public List<EntradaRanking> ObttenerRankingPorMapa(string mapa)
    {
        List<EntradaRanking> resultado = new List<EntradaRanking>();
        foreach (var entrada in rankingGlobal)
        {
            if (entrada.mapa == mapa)
                resultado.Add(entrada);
        }
        resultado.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));
        return resultado;
    }
}
