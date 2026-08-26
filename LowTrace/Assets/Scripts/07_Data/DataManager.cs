using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public static DataManager Instancia { get; private set; }

    [Header("ScriptableObjects")]
    [SerializeField] private RecordsData recordsData;
    [SerializeField] private SettingsData settingsData;
    [SerializeField] private UnlocksData unlocksData;

    private string carpetaGuardado;
    private string rutaRecords;
    private string rutaSettings;
    private string rutaUnlocks;

    public RecordsData Records => recordsData;
    public SettingsData Settings => settingsData;
    public UnlocksData Unlocks => unlocksData;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        carpetaGuardado = Application.persistentDataPath + "/Guardado";
        rutaRecords = carpetaGuardado + "/records.json";
        rutaSettings = carpetaGuardado + "/settings.json";
        rutaUnlocks = carpetaGuardado + "/unlocks.json";

        if (!Directory.Exists(carpetaGuardado))
            Directory.CreateDirectory(carpetaGuardado);

        CargarTodos();
    }

    public void GuardarTodos()
    {
        GuardarRecords();
        GuardarSettings();
        GuardarUnlocks();
    }

    public void CargarTodos()
    {
        CargarRecords();
        CargarSettings();
        CargarUnlocks();
    }

    public void GuardarRecords()
    {
        if (recordsData == null) return;
        string json = JsonUtility.ToJson(recordsData, true);
        File.WriteAllText(rutaRecords, json);
    }

    public void CargarRecords()
    {
        if (recordsData == null) return;
        if (File.Exists(rutaRecords))
        {
            string json = File.ReadAllText(rutaRecords);
            JsonUtility.FromJsonOverwrite(json, recordsData);
        }
    }

    public void GuardarSettings()
    {
        if (settingsData == null) return;
        string json = JsonUtility.ToJson(settingsData, true);
        File.WriteAllText(rutaSettings, json);
    }

    public void CargarSettings()
    {
        if (settingsData == null) return;
        if (File.Exists(rutaSettings))
        {
            string json = File.ReadAllText(rutaSettings);
            JsonUtility.FromJsonOverwrite(json, settingsData);
        }
    }

    public void GuardarUnlocks()
    {
        if (unlocksData == null) return;
        string json = JsonUtility.ToJson(unlocksData, true);
        File.WriteAllText(rutaUnlocks, json);
    }

    public void CargarUnlocks()
    {
        if (unlocksData == null) return;
        if (File.Exists(rutaUnlocks))
        {
            string json = File.ReadAllText(rutaUnlocks);
            JsonUtility.FromJsonOverwrite(json, unlocksData);
        }
    }

    public void BorrarDatos()
    {
        if (File.Exists(rutaRecords)) File.Delete(rutaRecords);
        if (File.Exists(rutaSettings)) File.Delete(rutaSettings);
        if (File.Exists(rutaUnlocks)) File.Delete(rutaUnlocks);
    }
}