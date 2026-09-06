using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapMenuUI : MonoBehaviour
{
    [Header("Botones Principales")]
    [SerializeField] private Button botonJugar;
    [SerializeField] private Button botonVolver;
    [SerializeField] private Button botonAjustes;

    [Header("Panel Ajustes")]
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private Slider sliderVolumenMusica;
    [SerializeField] private Slider sliderVolumenSFX;
    [SerializeField] private Toggle togglePantallaCompleta;
    [SerializeField] private TMP_Dropdown dropdownCalidad;
    [SerializeField] private Button botonCerrarAjustes;

    private void Awake()
    {
        AutoBuscarReferencias();
        ConectarListeners();
    }

    private void AutoBuscarReferencias()
    {
        if (botonJugar == null)
        {
            var obj = GameObject.Find("Canvas/Botones/Boton-Jugar");
            if (obj != null) botonJugar = obj.GetComponent<Button>();
        }

        if (botonVolver == null)
        {
            var obj = GameObject.Find("Canvas/Botones/Boton-Volver");
            if (obj != null) botonVolver = obj.GetComponent<Button>();
        }

        if (botonCerrarAjustes == null && panelAjustes != null)
        {
            var obj = GameObject.Find("Canvas/Panel-Ajustes/Boton-Cerrar");
            if (obj != null) botonCerrarAjustes = obj.GetComponent<Button>();
        }
    }

    private void ConectarListeners()
    {
        if (botonJugar != null)
            botonJugar.onClick.AddListener(Jugar);

        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverAlMenu);

        if (botonAjustes != null)
            botonAjustes.onClick.AddListener(AbrirAjustes);

        if (botonCerrarAjustes != null)
            botonCerrarAjustes.onClick.AddListener(CerrarAjustes);

        if (sliderVolumenMusica != null)
            sliderVolumenMusica.onValueChanged.AddListener(CambiarVolumenMusica);

        if (sliderVolumenSFX != null)
            sliderVolumenSFX.onValueChanged.AddListener(CambiarVolumenSFX);

        if (togglePantallaCompleta != null)
            togglePantallaCompleta.onValueChanged.AddListener(CambiarPantallaCompleta);

        if (dropdownCalidad != null)
            dropdownCalidad.onValueChanged.AddListener(CambiarCalidad);
    }

    private void Start()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(false);
    }

    private void Jugar()
    {
        if (MapSelectionManager.Instancia != null)
            MapSelectionManager.Instancia.CargarMapaSeleccionado();
    }

    private void VolverAlMenu()
    {
        if (SceneLoader.Instancia != null)
            SceneLoader.Instancia.VolverAlMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    private void AbrirAjustes()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(true);
    }

    private void CerrarAjustes()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(false);
    }

    private void CambiarVolumenMusica(float valor)
    {
        if (SoundManager.Instancia != null)
            SoundManager.Instancia.SetVolumenMusica(valor);

        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            DataManager.Instancia.ajustes.volumenMusica = valor;
            DataManager.Instancia.GuardarAjustes();
        }
    }

    private void CambiarVolumenSFX(float valor)
    {
        if (SoundManager.Instancia != null)
            SoundManager.Instancia.SetVolumenSFX(valor);

        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            DataManager.Instancia.ajustes.volumenSFX = valor;
            DataManager.Instancia.GuardarAjustes();
        }
    }

    private void CambiarPantallaCompleta(bool valor)
    {
        Screen.fullScreen = valor;
    }

    private void CambiarCalidad(int indice)
    {
        QualitySettings.SetQualityLevel(indice);
    }
}