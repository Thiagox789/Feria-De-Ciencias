using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instancia;

    [Header("Archivos de Datos (ScriptableObjects)")]
    public RecordsData records;
    public SettingsData ajustes;

    private string rutaRecords;
    private string rutaAjustes;

    private const int MAXIMO_RANKING = 10;

    private void Awake()
    {
        if (Instancia != null)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

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
    public void AgregarAlRanking(string nombre, float tiempo)
    {
        RecordsData.EntradaRanking nuevaEntrada = new RecordsData.EntradaRanking
        {
            nombreJugador = nombre,
            tiempo = tiempo,
            fecha = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm")
        };

        records.ranking.Add(nuevaEntrada);
        records.ranking.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));

        if (records.ranking.Count > MAXIMO_RANKING)
            records.ranking.RemoveRange(MAXIMO_RANKING, records.ranking.Count - MAXIMO_RANKING);

        GuardarDatos();
    }

    public List<RecordsData.EntradaRanking> ObtenerRanking()
    {
        return new List<RecordsData.EntradaRanking>(records.ranking);
    }

    public int ObtenerPosicionEnRanking(float tiempo)
    {
        for (int i = 0; i < records.ranking.Count; i++)
        {
            if (tiempo <= records.ranking[i].tiempo)
                return i + 1;
        }
        return records.ranking.Count + 1;
    }

    public void LimpiarRanking()
    {
        records.ranking.Clear();
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
