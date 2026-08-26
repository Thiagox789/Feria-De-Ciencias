using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecordsData", menuName = "LowTrace/Records Data")]
public class RecordsData : ScriptableObject
{
    [System.Serializable]
    public class RegistroTiempo
    {
        public string nombreJugador;
        public float tiempo;
        public string fecha;
        public string mapa;
    }

    public List<RegistroTiempo> registros = new List<RegistroTiempo>();

    public void AgregarRegistro(string nombre, float tiempo, string mapa)
    {
        RegistroTiempo nuevo = new RegistroTiempo
        {
            nombreJugador = nombre,
            tiempo = tiempo,
            fecha = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            mapa = mapa
        };
        registros.Add(nuevo);
        registros.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));
    }

    public float ObtenerMejorTiempo(string mapa)
    {
        foreach (var reg in registros)
        {
            if (reg.mapa == mapa)
                return reg.tiempo;
        }
        return 0f;
    }

    public List<RegistroTiempo> ObtenerRanking(string mapa, int limite = 10)
    {
        List<RegistroTiempo> ranking = new List<RegistroTiempo>();
        foreach (var reg in registros)
        {
            if (reg.mapa == mapa)
                ranking.Add(reg);
        }
        ranking.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));
        return ranking.GetRange(0, Mathf.Min(limite, ranking.Count));
    }
}