using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    private static DataManager _instancia;
    public static DataManager Instancia
    {
        get
        {
            if (_instancia == null)
            {
                _instancia = FindObjectOfType<DataManager>();
                if (_instancia == null)
                {
                    GameObject go = new GameObject("DataManager");
                    _instancia = go.AddComponent<DataManager>();
                }
            }
            return _instancia;
        }
        private set { _instancia = value; }
    }

    [Header("Archivos de Datos (ScriptableObjects)")]
    public RecordsData records;
    public SettingsData ajustes;

    private string rutaRecords;
    private string rutaAjustes;

    private const int MAXIMO_RANKING = 100;

    private void Awake()
    {
        if (_instancia != null && _instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        _instancia = this;
        DontDestroyOnLoad(gameObject);

        if (records == null)
            records = ScriptableObject.CreateInstance<RecordsData>();
        if (ajustes == null)
            ajustes = ScriptableObject.CreateInstance<SettingsData>();

        rutaRecords = Application.persistentDataPath + "/records_jugador.json";
        rutaAjustes = Application.persistentDataPath + "/ajustes_juego.json";

        CargarDatos();
        CargarAjustes();
    }

    // ==========================================
    // RÉCORDS
    // ==========================================
    public void GuardarDatos()
    {
        string textoJson = JsonUtility.ToJson(records);
        File.WriteAllText(rutaRecords, textoJson);
    }

    public void CargarDatos()
    {
        if (string.IsNullOrEmpty(rutaRecords))
            rutaRecords = Application.persistentDataPath + "/records_jugador.json";

        if (records == null)
            records = ScriptableObject.CreateInstance<RecordsData>();

        if (File.Exists(rutaRecords))
        {
            string textoJson = File.ReadAllText(rutaRecords);
            JsonUtility.FromJsonOverwrite(textoJson, records);
        }
    }

    public void IntentarNuevoRecord(float nuevoTiempo)
    {
        if (nuevoTiempo < records.mejorTiempo)
        {
            records.mejorTiempo = nuevoTiempo;
            GuardarDatos();
        }
    }

    // ==========================================
    // RANKING
    // ==========================================
    public void AgregarAlRanking(string nombre, float tiempo, string mapa)
    {
        if (!string.IsNullOrEmpty(nombre) && nombre.Length > 14)
        {
            nombre = nombre.Substring(0, 14);
        }

        RecordsData.EntradaRanking nuevaEntrada = new RecordsData.EntradaRanking
        {
            nombreJugador = nombre,
            tiempo = tiempo,
            mapa = mapa,
            fecha = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm")
        };

        records.rankingGlobal.Add(nuevaEntrada);
        records.rankingGlobal.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));

        if (records.rankingGlobal.Count > MAXIMO_RANKING)
            records.rankingGlobal.RemoveRange(MAXIMO_RANKING, records.rankingGlobal.Count - MAXIMO_RANKING);

        GuardarDatos();
    }

    public struct EntradaRankingConPosicion
    {
        public int posicionGlobal;
        public RecordsData.EntradaRanking entrada;
    }

    public List<RecordsData.EntradaRanking> ObtenerRanking()
    {
        if (records == null)
            records = ScriptableObject.CreateInstance<RecordsData>();

        if (records.rankingGlobal == null)
            records.rankingGlobal = new List<RecordsData.EntradaRanking>();

        return new List<RecordsData.EntradaRanking>(records.rankingGlobal);
    }

    public List<EntradaRankingConPosicion> ObtenerRankingFiltradoConPosicion(string filtroNombre = "", string filtroMapa = "")
    {
        if (records == null)
            records = ScriptableObject.CreateInstance<RecordsData>();

        if (records.rankingGlobal == null)
            records.rankingGlobal = new List<RecordsData.EntradaRanking>();

        // 1. Filtrar primero por mapa (o incluir todos)
        List<RecordsData.EntradaRanking> listaPorMapa = new List<RecordsData.EntradaRanking>();
        bool esTodos = string.IsNullOrEmpty(filtroMapa) || 
                       filtroMapa.Equals("Todos", System.StringComparison.OrdinalIgnoreCase) ||
                       filtroMapa.StartsWith("Option", System.StringComparison.OrdinalIgnoreCase);

        foreach (var entrada in records.rankingGlobal)
        {
            if (esTodos || (entrada.mapa != null && entrada.mapa.Equals(filtroMapa, System.StringComparison.OrdinalIgnoreCase)))
            {
                listaPorMapa.Add(entrada);
            }
        }

        // 2. Ordenar por tiempo (menor tiempo = mejor posición en este mapa)
        listaPorMapa.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));

        // 3. Asignar posición en este mapa y filtrar por nombre si hay texto en el buscador
        List<EntradaRankingConPosicion> resultado = new List<EntradaRankingConPosicion>();

        for (int i = 0; i < listaPorMapa.Count; i++)
        {
            var entrada = listaPorMapa[i];

            bool coincideNombre = string.IsNullOrEmpty(filtroNombre) || 
                                  (entrada.nombreJugador != null && entrada.nombreJugador.IndexOf(filtroNombre, System.StringComparison.OrdinalIgnoreCase) >= 0);

            if (coincideNombre)
            {
                resultado.Add(new EntradaRankingConPosicion
                {
                    posicionGlobal = i + 1,
                    entrada = entrada
                });
            }
        }

        return resultado;
    }

    public List<RecordsData.EntradaRanking> ObtenerRankingPorMapa(string mapa)
    {
        return records.ObttenerRankingPorMapa(mapa);
    }

    public int ObtenerPosicionEnRanking(float tiempo, string mapa)
    {
        var rankingMapa = records.ObttenerRankingPorMapa(mapa);
        for (int i = 0; i < rankingMapa.Count; i++)
        {
            if (tiempo <= rankingMapa[i].tiempo)
                return i + 1;
        }
        return rankingMapa.Count + 1;
    }

    public void LimpiarRanking()
    {
        records.rankingGlobal.Clear();
        GuardarDatos();
    }

    // ==========================================
    // AJUSTES DE VOLUMEN
    // ==========================================
    public void GuardarAjustes()
    {
        string textoJson = JsonUtility.ToJson(ajustes);
        File.WriteAllText(rutaAjustes, textoJson);
    }

    public void CargarAjustes()
    {
        if (File.Exists(rutaAjustes))
        {
            string textoJson = File.ReadAllText(rutaAjustes);
            JsonUtility.FromJsonOverwrite(textoJson, ajustes);
        }
        else
        {
            if (ajustes != null)
            {
                ajustes.volumenMusica = 1f;
                ajustes.volumenSFX = 1f;
            }
        }
    }
}
