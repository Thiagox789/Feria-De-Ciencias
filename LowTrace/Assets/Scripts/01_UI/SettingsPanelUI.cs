using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelUI : MonoBehaviour
{
    [Header("Sliders de UI")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    [Header("Opciones Adicionales (Opcionales)")]
    [SerializeField] private Toggle togglePantallaCompleta;
    [SerializeField] private TMP_Dropdown dropdownCalidad;
    [SerializeField] private Button botonCerrar;

    protected virtual void Start()
    {
        CargarValoresIniciales();
        ConectarListeners();
    }

    public void CargarValoresIniciales()
    {
        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            if (sliderMusica != null) sliderMusica.value = DataManager.Instancia.ajustes.volumenMusica;
            if (sliderSFX != null) sliderSFX.value = DataManager.Instancia.ajustes.volumenSFX;
        }

        if (togglePantallaCompleta != null)
            togglePantallaCompleta.isOn = Screen.fullScreen;

        if (dropdownCalidad != null)
            dropdownCalidad.value = QualitySettings.GetQualityLevel();
    }

    private void ConectarListeners()
    {
        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.RemoveListener(OnMusicaChanged);
            sliderMusica.onValueChanged.AddListener(OnMusicaChanged);
        }

        if (sliderSFX != null)
        {
            sliderSFX.onValueChanged.RemoveListener(OnSFXChanged);
            sliderSFX.onValueChanged.AddListener(OnSFXChanged);
        }

        if (togglePantallaCompleta != null)
        {
            togglePantallaCompleta.onValueChanged.RemoveListener(OnPantallaCompletaChanged);
            togglePantallaCompleta.onValueChanged.AddListener(OnPantallaCompletaChanged);
        }

        if (dropdownCalidad != null)
        {
            dropdownCalidad.onValueChanged.RemoveListener(OnCalidadChanged);
            dropdownCalidad.onValueChanged.AddListener(OnCalidadChanged);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveListener(CerrarPanel);
            botonCerrar.onClick.AddListener(CerrarPanel);
        }
    }

    public void OnMusicaChanged(float nuevoVolumen)
    {
        if (SoundManager.Instancia != null)
            SoundManager.Instancia.SetVolumenMusica(nuevoVolumen);

        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            DataManager.Instancia.ajustes.volumenMusica = nuevoVolumen;
            DataManager.Instancia.GuardarAjustes();
        }
    }

    public void OnSFXChanged(float nuevoVolumen)
    {
        if (SoundManager.Instancia != null)
            SoundManager.Instancia.SetVolumenSFX(nuevoVolumen);

        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            DataManager.Instancia.ajustes.volumenSFX = nuevoVolumen;
            DataManager.Instancia.GuardarAjustes();
        }
    }

    public void OnPantallaCompletaChanged(bool esPantallaCompleta)
    {
        Screen.fullScreen = esPantallaCompleta;
    }

    public void OnCalidadChanged(int indiceCalidad)
    {
        QualitySettings.SetQualityLevel(indiceCalidad);
    }

    public void AbrirPanel()
    {
        gameObject.SetActive(true);
        CargarValoresIniciales();
    }

    public void CerrarPanel()
    {
        gameObject.SetActive(false);
    }
}
