// System ranking UI controller with pagination support
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
    [SerializeField] private TextMeshProUGUI textoTitulo;

    [Header("Botones")]
    [SerializeField] private Button botonVolver;
    [SerializeField] private Button botonLimpiar;

    [Header("Colores")]
    [SerializeField] private Color colorPrimero = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color colorSegundo = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color colorTercero = new Color(0.8f, 0.5f, 0.2f);
    [SerializeField] private Color[] coloresFilas;

    [Header("Búsqueda y Filtro")]
    [SerializeField] private TMP_InputField inputBuscarNombre;
    [SerializeField] private TMP_Dropdown dropdownMapa;

    [Header("Paginación")]
    [SerializeField] private Button botonPaginaAnterior;
    [SerializeField] private Button botonPaginaSiguiente;
    [SerializeField] private TextMeshProUGUI textoPaginaInfo;

    [Header("Filas Estáticas (Opcional)")]
    [SerializeField] private TextMeshProUGUI[] textosNumeros;
    [SerializeField] private TextMeshProUGUI[] textosJugadores;
    [SerializeField] private TextMeshProUGUI[] textosTiempos;

    private List<GameObject> filasGeneradas = new List<GameObject>();
    private int paginaActual = 0;
    private const int FILAS_POR_PAGINA = 10;

    private void Awake()
    {
        AutoBuscarReferencias();

        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverAlMenu);

        if (botonLimpiar != null)
            botonLimpiar.onClick.AddListener(LimpiarRanking);

        if (inputBuscarNombre != null)
        {
            inputBuscarNombre.characterLimit = 14;
            inputBuscarNombre.onValueChanged.AddListener(OnFiltroCambiado);
        }

        if (dropdownMapa != null)
            dropdownMapa.onValueChanged.AddListener(OnFiltroCambiado);

        if (botonPaginaAnterior != null)
            botonPaginaAnterior.onClick.AddListener(PaginaAnterior);

        if (botonPaginaSiguiente != null)
            botonPaginaSiguiente.onClick.AddListener(PaginaSiguiente);

        InicializarDropdownMapa();

        if (templateFila != null)
            templateFila.SetActive(false);
    }

    public void PaginaAnterior()
    {
        if (paginaActual > 0)
        {
            paginaActual--;
            CargarRanking();
        }
    }

    public void PaginaSiguiente()
    {
        paginaActual++;
        CargarRanking();
    }

    private void InicializarDropdownMapa()
    {
        if (dropdownMapa == null) return;
        
        dropdownMapa.ClearOptions();
        List<string> opciones = new List<string>();

        if (DataManager.Instancia != null)
        {
            List<RecordsData.EntradaRanking> ranking = DataManager.Instancia.ObtenerRanking();
            foreach (var r in ranking)
            {
                if (!string.IsNullOrEmpty(r.mapa) && !opciones.Contains(r.mapa))
                {
                    opciones.Add(r.mapa);
                }
            }
        }

        opciones.Sort();
        opciones.Insert(0, "Todos");

        dropdownMapa.AddOptions(opciones);
        dropdownMapa.value = 0;
        dropdownMapa.RefreshShownValue();
    }

    private void OnFiltroCambiado(string texto)
    {
        paginaActual = 0;
        CargarRanking();
    }

    private void OnFiltroCambiado(int indice)
    {
        paginaActual = 0;
        CargarRanking();
    }

    public void AutoBuscarReferencias()
    {
        if (botonVolver == null)
        {
            var btnSalirObj = GameObject.Find("Boton-Salir");
            if (btnSalirObj != null)
                botonVolver = btnSalirObj.GetComponent<Button>();
        }

        if (inputBuscarNombre == null)
        {
            var inputObj = GameObject.Find("Canvas/Ranking/Panel-Jugador/Nombre-Input");
            if (inputObj != null)
                inputBuscarNombre = inputObj.GetComponent<TMP_InputField>();
        }

        if (dropdownMapa == null)
        {
            var dropObj = GameObject.Find("Canvas/Ranking/Panel-Jugador/Dropdown");
            if (dropObj != null)
                dropdownMapa = dropObj.GetComponent<TMP_Dropdown>();
        }

        if (textoTitulo == null)
        {
            var titleObj = GameObject.Find("Canvas/Panel-Titulo/Titulo-Ajustes");
            if (titleObj != null)
                textoTitulo = titleObj.GetComponent<TextMeshProUGUI>();
        }

        if (botonPaginaAnterior == null)
        {
            var btnAnt = GameObject.Find("Boton-Anterior");
            if (btnAnt != null) botonPaginaAnterior = btnAnt.GetComponent<Button>();
        }

        if (botonPaginaSiguiente == null)
        {
            var btnSig = GameObject.Find("Boton-Siguiente");
            if (btnSig != null) botonPaginaSiguiente = btnSig.GetComponent<Button>();
        }

        if (textoPaginaInfo == null)
        {
            var txtPag = GameObject.Find("Texto-Pagina");
            if (txtPag != null) textoPaginaInfo = txtPag.GetComponent<TextMeshProUGUI>();
        }

        bool necesitaBuscar = (textosJugadores == null || textosJugadores.Length == 0 || (textosJugadores.Length > 0 && textosJugadores[0] == null));

        if (necesitaBuscar && templateFila == null)
        {
            var fj = GameObject.Find("Canvas/Ranking/Fila_Jugadores");
            var ft = GameObject.Find("Canvas/Ranking/Fila_Tiempo");
            var fn = GameObject.Find("Canvas/Ranking/Fila_Numeros");

            if (fj != null) textosJugadores = fj.GetComponentsInChildren<TextMeshProUGUI>();
            if (ft != null) textosTiempos = ft.GetComponentsInChildren<TextMeshProUGUI>();
            if (fn != null) textosNumeros = fn.GetComponentsInChildren<TextMeshProUGUI>();
        }
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

        string filtroNombre = inputBuscarNombre != null ? inputBuscarNombre.text : "";
        string filtroMapa = (dropdownMapa != null && dropdownMapa.options.Count > 0) ? dropdownMapa.options[dropdownMapa.value].text : "";

        if (textoTitulo != null)
        {
            if (string.IsNullOrEmpty(filtroMapa) || filtroMapa.Equals("Todos", System.StringComparison.OrdinalIgnoreCase))
                textoTitulo.text = "RANKING GLOBAL";
            else
                textoTitulo.text = "RANKING " + filtroMapa.ToUpper();
        }

        List<DataManager.EntradaRankingConPosicion> ranking = DataManager.Instancia.ObtenerRankingFiltradoConPosicion(filtroNombre, filtroMapa);

        int totalPaginas = Mathf.Max(1, Mathf.CeilToInt((float)ranking.Count / FILAS_POR_PAGINA));
        paginaActual = Mathf.Clamp(paginaActual, 0, totalPaginas - 1);

        if (textoPaginaInfo != null)
        {
            textoPaginaInfo.text = (paginaActual + 1).ToString();
        }

        if (botonPaginaAnterior != null)
            botonPaginaAnterior.interactable = (paginaActual > 0);

        if (botonPaginaSiguiente != null)
            botonPaginaSiguiente.interactable = (paginaActual < totalPaginas - 1);

        if (ranking.Count == 0)
        {
            MostrarSinResultados(true);
            ActualizarFilasEstaticas(ranking);
            return;
        }

        MostrarSinResultados(false);

        if (templateFila != null && contenedorFilas != null)
        {
            GenerarFilas(ranking);
        }
        else
        {
            ActualizarFilasEstaticas(ranking);
        }
    }

    private void ActualizarFilasEstaticas(List<DataManager.EntradaRankingConPosicion> ranking)
    {
        if (textosJugadores == null || textosTiempos == null) return;

        int offset = paginaActual * FILAS_POR_PAGINA;
        int totalSlots = Mathf.Min(textosJugadores.Length, textosTiempos.Length);

        for (int i = 0; i < totalSlots; i++)
        {
            int indexRanking = offset + i;
            if (indexRanking < ranking.Count)
            {
                int posReal = ranking[indexRanking].posicionGlobal;

                if (textosNumeros != null && i < textosNumeros.Length)
                {
                    textosNumeros[i].gameObject.SetActive(true);
                    textosNumeros[i].text = posReal.ToString();
                }

                textosJugadores[i].gameObject.SetActive(true);
                textosJugadores[i].text = ranking[indexRanking].entrada.nombreJugador;

                textosTiempos[i].gameObject.SetActive(true);
                textosTiempos[i].text = FormatearTiempo(ranking[indexRanking].entrada.tiempo);
            }
            else
            {
                if (textosNumeros != null && i < textosNumeros.Length)
                    textosNumeros[i].text = "-";

                textosJugadores[i].text = "---";
                textosTiempos[i].text = "--:--.---";
            }
        }
    }

    private void GenerarFilas(List<DataManager.EntradaRankingConPosicion> ranking)
    {
        for (int i = 0; i < ranking.Count; i++)
        {
            GameObject fila = Instantiate(templateFila, contenedorFilas);
            fila.SetActive(true);
            filasGeneradas.Add(fila);

            TextMeshProUGUI[] textos = fila.GetComponentsInChildren<TextMeshProUGUI>();

            if (textos.Length >= 3)
            {
                int posReal = ranking[i].posicionGlobal;
                textos[0].text = posReal.ToString();
                textos[1].text = ranking[i].entrada.nombreJugador;
                textos[2].text = FormatearTiempo(ranking[i].entrada.tiempo);
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
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
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
