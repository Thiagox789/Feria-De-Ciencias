using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Este script controla la pantalla de Selección de Mapa (Jugar, Volver, Abrir Ajustes).
public class MapMenuUI : MonoBehaviour
{
    [Header("Botones Principales")]
    [SerializeField] private Button botonJugar;
    [SerializeField] private Button botonVolver;
    [SerializeField] private Button botonAjustes;

    [Header("Panel Ajustes (Reutiliza SettingsPanelUI)")]
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private SettingsPanelUI settingsPanel;

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

        if (panelAjustes == null)
        {
            panelAjustes = GameObject.Find("Canvas/Panel-Ajustes");
        }

        if (settingsPanel == null && panelAjustes != null)
        {
            settingsPanel = panelAjustes.GetComponent<SettingsPanelUI>();
            if (settingsPanel == null)
                settingsPanel = panelAjustes.AddComponent<SettingsPanelUI>();
        }
    }

    private void ConectarListeners()
    {
        if (botonJugar != null)
        {
            botonJugar.onClick.RemoveListener(Jugar);
            botonJugar.onClick.AddListener(Jugar);
        }

        if (botonVolver != null)
        {
            botonVolver.onClick.RemoveListener(VolverAlMenu);
            botonVolver.onClick.AddListener(VolverAlMenu);
        }

        if (botonAjustes != null)
        {
            botonAjustes.onClick.RemoveListener(AbrirAjustes);
            botonAjustes.onClick.AddListener(AbrirAjustes);
        }
    }

    private void Start()
    {
        if (panelAjustes != null)
        {
            panelAjustes.SetActive(false);
        }
    }

    private void Jugar()
    {
        if (MapSelectionManager.Instancia != null)
        {
            MapSelectionManager.Instancia.CargarMapaSeleccionado();
        }
        else
        {
            // Fallback por si la escena no tiene MapSelectionManager
            if (SceneLoader.Instancia != null)
                SceneLoader.Instancia.CargarEscena("Circuito1");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("Circuito1");
        }
    }

    private void VolverAlMenu()
    {
        if (SceneLoader.Instancia != null)
        {
            SceneLoader.Instancia.VolverAlMenu();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }

    private void AbrirAjustes()
    {
        if (settingsPanel != null)
        {
            settingsPanel.AbrirPanel();
        }
        else if (panelAjustes != null)
        {
            panelAjustes.SetActive(true);
        }
    }

    private void CerrarAjustes()
    {
        if (settingsPanel != null)
        {
            settingsPanel.CerrarPanel();
        }
        else if (panelAjustes != null)
        {
            panelAjustes.SetActive(false);
        }
    }
}