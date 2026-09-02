using UnityEngine;
using System.IO; 

public class DataManager : MonoBehaviour
{
    public static DataManager Instancia;

    [Header("Archivos de Datos (ScriptableObjects)")]
    public RecordsData records;
    public SettingsData ajustes; // ¡NUEVO! Agregamos los ajustes de volumen

    private string rutaRecords;
    private string rutaAjustes;  // ¡NUEVO! Ruta para guardar los ajustes

    private void Awake()
    {
        if (Instancia != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject); 

        // Definimos las rutas de los archivos en la computadora
        rutaRecords = Application.persistentDataPath + "/records_jugador.json";
        rutaAjustes = Application.persistentDataPath + "/ajustes_juego.json";

        // Apenas empieza el juego, cargamos TODO
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
    // AJUSTES DE VOLUMEN (¡NUEVO!)
    // ==========================================
    
    // Convierte el volumen a JSON y lo guarda en la compu
    public void GuardarAjustes()
    {
        string textoJson = JsonUtility.ToJson(ajustes);
        File.WriteAllText(rutaAjustes, textoJson);
        Debug.Log("¡Ajustes de sonido guardados en JSON: " + rutaAjustes + "!");
    }

    // Lee el JSON de la compu y actualiza el juego
    public void CargarAjustes()
    {
        if (File.Exists(rutaAjustes))
        {
            string textoJson = File.ReadAllText(rutaAjustes);
            JsonUtility.FromJsonOverwrite(textoJson, ajustes);
        }
        else 
        {
            // Si es la primera vez, aseguramos que el volumen esté al máximo por defecto
            if (ajustes != null)
            {
                ajustes.volumenMusica = 1f;
                ajustes.volumenSFX = 1f;
            }
        }
    }
}
