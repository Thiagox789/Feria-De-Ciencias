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
            if (SonMapasEquivalentes(entrada.mapa, mapa))
                resultado.Add(entrada);
        }
        resultado.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));
        return resultado;
    }

    private bool SonMapasEquivalentes(string m1, string m2)
    {
        if (string.IsNullOrEmpty(m1) || string.IsNullOrEmpty(m2)) return true;
        if (string.Equals(m1, m2, System.StringComparison.OrdinalIgnoreCase)) return true;

        string n1 = m1.Replace(" ", "").ToLower();
        string n2 = m2.Replace(" ", "").ToLower();
        if (n1 == n2) return true;

        if ((n1.Contains("ia") || n1.Contains("mapa1")) && (n2.Contains("ia") || n2.Contains("mapa1")))
            return true;

        return false;
    }
}
