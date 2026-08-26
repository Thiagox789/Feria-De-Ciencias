using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instancia { get; private set; }

    private SettingsData settings;

    public float VolumenMusica
    {
        get => settings != null ? settings.volumenMusica : 0.5f;
        set
        {
            if (settings == null) return;
            settings.volumenMusica = Mathf.Clamp01(value);
            if (SoundManager.Instancia != null)
                SoundManager.Instancia.VolumenMusica = settings.volumenMusica;
        }
    }

    public float VolumenSFX
    {
        get => settings != null ? settings.volumenSFX : 0.7f;
        set
        {
            if (settings == null) return;
            settings.volumenSFX = Mathf.Clamp01(value);
            if (SoundManager.Instancia != null)
                SoundManager.Instancia.VolumenSFX = settings.volumenSFX;
        }
    }

    public int CalidadGrafica
    {
        get => settings != null ? settings.calidadGrafica : 2;
        set
        {
            if (settings == null) return;
            settings.calidadGrafica = Mathf.Clamp(value, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(settings.calidadGrafica);
        }
    }

    public bool PantallaCompleta
    {
        get => settings != null ? settings.pantallaCompleta : false;
        set
        {
            if (settings == null) return;
            settings.pantallaCompleta = value;
            Screen.fullScreen = value;
        }
    }

    public float SensibilidadVolante
    {
        get => settings != null ? settings.sensibilidadVolante : 1f;
        set
        {
            if (settings == null) return;
            settings.sensibilidadVolante = Mathf.Clamp(value, 0.1f, 3f);
        }
    }

    public string NombreJugador
    {
        get => settings != null ? settings.nombreJugador : "Jugador";
        set
        {
            if (settings == null) return;
            settings.nombreJugador = value;
        }
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

    private void Start()
    {
        if (DataManager.Instancia != null)
            settings = DataManager.Instancia.Settings;

        AplicarConfiguracion();
    }

    public void AplicarConfiguracion()
    {
        if (settings == null) return;

        if (SoundManager.Instancia != null)
        {
            SoundManager.Instancia.VolumenMusica = settings.volumenMusica;
            SoundManager.Instancia.VolumenSFX = settings.volumenSFX;
        }

        QualitySettings.SetQualityLevel(settings.calidadGrafica);
        Screen.fullScreen = settings.pantallaCompleta;
    }

    public void Guardar()
    {
        if (DataManager.Instancia != null)
            DataManager.Instancia.GuardarSettings();
    }
}