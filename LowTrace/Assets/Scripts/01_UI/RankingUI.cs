using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RankingUI : MonoBehaviour
{
    [Header("Panel Principal")]
    [SerializeField] private Transform contenedorFilas;

    [Header("Template")]
    [SerializeField] private GameObject templateFila;

    [Header("Información")]
    [SerializeField] private TextMeshProUGUI textoSinResultados;

    [Header("Botones")]
    [SerializeField] private Button botonVolver;
    [SerializeField] private Button botonLimpiar;

    [Header("Colores")]
    [SerializeField] private Color colorPrimero = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color colorSegundo = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color colorTercero = new Color(0.8f, 0.5f, 0.2f);
    [SerializeField] private Color[] coloresFilas;

    private List<GameObject> filasGeneradas = new List<GameObject>();

    private void Awake()
    {
        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverAlMenu);

        if (botonLimpiar != null)
            botonLimpiar.onClick.AddListener(LimpiarRanking);

        if (templateFila != null)
            templateFila.SetActive(false);
    }

    private void Start()
    {
        CargarRanking();
    }

    public void CargarRanking()
    {
        LimpiarFilas();

        if (DataManager.Instancia == null)
        {
            Debug.LogWarning("DataManager no encontrado");
            return;
        }

        List<RecordsData.EntradaRanking> ranking = DataManager.Instancia.ObtenerRanking();

        if (ranking.Count == 0)
        {
            MostrarSinResultados(true);
            return;
        }

        MostrarSinResultados(false);
        GenerarFilas(ranking);
    }

    private void GenerarFilas(List<RecordsData.EntradaRanking> ranking)
    {
        for (int i = 0; i < ranking.Count; i++)
        {
            GameObject fila = Instantiate(templateFila, contenedorFilas);
            fila.SetActive(true);
            filasGeneradas.Add(fila);

            TextMeshProUGUI[] textos = fila.GetComponentsInChildren<TextMeshProUGUI>();

            if (textos.Length >= 3)
            {
                textos[0].text = (i + 1).ToString();
                textos[0].color = ObtenerColorPosicion(i);

                textos[1].text = ranking[i].nombreJugador;

                textos[2].text = FormatearTiempo(ranking[i].tiempo);
            }

            Image bg = fila.GetComponent<Image>();
            if (bg != null && coloresFilas != null && i < coloresFilas.Length)
            {
                bg.color = coloresFilas[i];
            }
        }
    }

    private void LimpiarFilas()
    {
        foreach (GameObject fila in filasGeneradas)
        {
            if (fila != null)
                Destroy(fila);
        }
        filasGeneradas.Clear();
    }

    private void MostrarSinResultados(bool mostrar)
    {
        if (textoSinResultados != null)
            textoSinResultados.gameObject.SetActive(mostrar);
    }

    private Color ObtenerColorPosicion(int posicion)
    {
        switch (posicion)
        {
            case 0: return colorPrimero;
            case 1: return colorSegundo;
            case 2: return colorTercero;
            default: return Color.white;
        }
    }

    private string FormatearTiempo(float tiempo)
    {
        int min = (int)(tiempo / 60f);
        int seg = (int)(tiempo % 60f);
        int mili = (int)((tiempo - Mathf.Floor(tiempo)) * 1000f);
        return string.Format("{0:00}:{1:00}.{2:000}", min, seg, mili);
    }

    private void VolverAlMenu()
    {
        if (SceneLoader.Instancia != null)
            SceneLoader.Instancia.VolverAlMenu();
    }

    private void LimpiarRanking()
    {
        if (DataManager.Instancia != null)
        {
            DataManager.Instancia.LimpiarRanking();
            CargarRanking();
        }
    }
}
