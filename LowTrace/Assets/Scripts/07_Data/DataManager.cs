using UnityEngine;
using System.IO;
using System.Collections.Generic;

// Este es el Gestor de Datos (DataManager).
// Guarda y carga la información del juego en archivos formato JSON en el disco rígido
// (volumen, mejores tiempos y tabla de posiciones de jugadores).
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
                    GameObject objetoGestor = new GameObject("DataManager");
                    _instancia = objetoGestor.AddComponent<DataManager>();
                }
            }
            return _instancia;
        }
        private set { _instancia = value; }
    }

    [Header("Objetos de Datos")]
    public RecordsData records;
    public SettingsData ajustes;

    private string rutaArchivoRecords;
    private string rutaArchivoAjustes;

    private const int MAXIMO_ENTRADAS_RANKING = 100;

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

        // Rutas donde se guardan los archivos JSON en la computadora del jugador
        rutaArchivoRecords = Application.persistentDataPath + "/records_jugador.json";
        rutaArchivoAjustes = Application.persistentDataPath + "/ajustes_juego.json";

        CargarDatos();
        CargarAjustes();
    }

    // ==========================================
    // RÉCORDS Y RANKING EN JSON
    // ==========================================
    public void GuardarDatos()
    {
        // Convertimos el objeto C# a texto formato JSON
        string textoJson = JsonUtility.ToJson(records);
        File.WriteAllText(rutaArchivoRecords, textoJson);
    }

    public void CargarDatos()
    {
        if (string.IsNullOrEmpty(rutaArchivoRecords))
            rutaArchivoRecords = Application.persistentDataPath + "/records_jugador.json";

        if (records == null)
            records = ScriptableObject.CreateInstance<RecordsData>();

        if (File.Exists(rutaArchivoRecords))
        {
            // Leemos el texto del archivo JSON y lo cargamos en el objeto
            string textoJson = File.ReadAllText(rutaArchivoRecords);
            JsonUtility.FromJsonOverwrite(textoJson, records);
        }
        else
        {
            records.rankingGlobal.Clear();
            records.mejorTiempo = 9999f;
            GuardarDatos();
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

        if (records.rankingGlobal.Count > MAXIMO_ENTRADAS_RANKING)
        {
            records.rankingGlobal.RemoveRange(MAXIMO_ENTRADAS_RANKING, records.rankingGlobal.Count - MAXIMO_ENTRADAS_RANKING);
        }

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

        listaPorMapa.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));

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

    public float ObtenerMejorTiempoPorMapa(string mapa)
    {
        var rankingMapa = ObtenerRankingPorMapa(mapa);
        if (rankingMapa != null && rankingMapa.Count > 0)
        {
            return rankingMapa[0].tiempo;
        }
        return (records != null && records.mejorTiempo < 9999f) ? records.mejorTiempo : 0f;
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
        records.mejorTiempo = 9999f;
        GuardarDatos();
    }

    // ==========================================
    // CONFIGURACIÓN DE AJUSTES EN JSON
    // ==========================================
    public void GuardarAjustes()
    {
        string textoJson = JsonUtility.ToJson(ajustes);
        File.WriteAllText(rutaArchivoAjustes, textoJson);
    }

    public void CargarAjustes()
    {
        if (File.Exists(rutaArchivoAjustes))
        {
            string textoJson = File.ReadAllText(rutaArchivoAjustes);
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
