using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Este script controla la pantalla de Ranking / Tabla de Posiciones.
// Muestra los mejores tiempos ordenados de menor a mayor, con soporte para paginación y búsqueda por nombre o mapa.
public class RankingUI : MonoBehaviour
{
    [Header("Contenedor de Filas")]
    [SerializeField] private Transform contenedorFilas;

    [Header("Plantilla de Fila (Template)")]
    [SerializeField] private GameObject plantillaFila;

    [Header("Textos de Información")]
    [SerializeField] private TextMeshProUGUI textoSinResultados;
    [SerializeField] private TextMeshProUGUI textoTituloRanking;

    [Header("Botones de Navegación")]
    [SerializeField] private Button botonVolver;
    [SerializeField] private Button botonBorrarTodo;

    [Header("Colores del Podio")]
    [SerializeField] private Color colorPrimerLugar = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color colorSegundoLugar = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color colorTercerLugar = new Color(0.8f, 0.5f, 0.2f);

    [Header("Búsqueda y Filtro de Circuito")]
    [SerializeField] private TMP_InputField inputBuscadorNombre;
    [SerializeField] private TMP_Dropdown dropdownFiltroMapa;

    [Header("Controles de Paginación")]
    [SerializeField] private Button botonPaginaAnterior;
    [SerializeField] private Button botonPaginaSiguiente;
    [SerializeField] private TextMeshProUGUI textoNumeroPagina;

    [Header("Filas Estáticas (Opcionales)")]
    [SerializeField] private TextMeshProUGUI[] textosNumerosPosicion;
    [SerializeField] private TextMeshProUGUI[] textosNombresJugadores;
    [SerializeField] private TextMeshProUGUI[] textosTiemposJugadores;

    private List<GameObject> filasGeneradas = new List<GameObject>();
    private int paginaActual = 0;
    private const int REGISTROS_POR_PAGINA = 10;

    private void Awake()
    {
        BuscarReferenciasEnEscena();

        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverAlMenu);

        if (botonBorrarTodo != null)
            botonBorrarTodo.onClick.AddListener(LimpiarTablaRanking);

        if (inputBuscadorNombre != null)
        {
            inputBuscadorNombre.characterLimit = 14;
            inputBuscadorNombre.onValueChanged.AddListener(AlCambiarFiltroTexto);
        }

        if (dropdownFiltroMapa != null)
            dropdownFiltroMapa.onValueChanged.AddListener(AlCambiarFiltroDropdown);

        if (botonPaginaAnterior != null)
            botonPaginaAnterior.onClick.AddListener(IrAPaginaAnterior);

        if (botonPaginaSiguiente != null)
            botonPaginaSiguiente.onClick.AddListener(IrAPaginaSiguiente);

        CargarOpcionesDelDropdown();

        if (plantillaFila != null)
            plantillaFila.SetActive(false);
    }

    private void Start()
    {
        CargarRankingEnPantalla();
    }

    public void IrAPaginaAnterior()
    {
        if (paginaActual > 0)
        {
            paginaActual--;
            CargarRankingEnPantalla();
        }
    }

    public void IrAPaginaSiguiente()
    {
        paginaActual++;
        CargarRankingEnPantalla();
    }

    private void CargarOpcionesDelDropdown()
    {
        if (dropdownFiltroMapa == null) return;
        
        dropdownFiltroMapa.ClearOptions();
        List<string> listaCircuitos = new List<string>();

        if (DataManager.Instancia != null)
        {
            List<RecordsData.EntradaRanking> listaRanking = DataManager.Instancia.ObtenerRanking();
            foreach (var registro in listaRanking)
            {
                if (!string.IsNullOrEmpty(registro.mapa) && !listaCircuitos.Contains(registro.mapa))
                {
                    listaCircuitos.Add(registro.mapa);
                }
            }
        }

        listaCircuitos.Sort();
        listaCircuitos.Insert(0, "Todos");

        dropdownFiltroMapa.AddOptions(listaCircuitos);
        dropdownFiltroMapa.value = 0;
        dropdownFiltroMapa.RefreshShownValue();
    }

    private void AlCambiarFiltroTexto(string texto)
    {
        paginaActual = 0;
        CargarRankingEnPantalla();
    }

    private void AlCambiarFiltroDropdown(int indice)
    {
        paginaActual = 0;
        CargarRankingEnPantalla();
    }

    private void BuscarReferenciasEnEscena()
    {
        if (botonVolver == null)
        {
            var btn = GameObject.Find("Boton-Salir");
            if (btn != null) botonVolver = btn.GetComponent<Button>();
        }

        if (inputBuscadorNombre == null)
        {
            var inp = GameObject.Find("Canvas/Ranking/Panel-Jugador/Nombre-Input");
            if (inp != null) inputBuscadorNombre = inp.GetComponent<TMP_InputField>();
        }

        if (dropdownFiltroMapa == null)
        {
            var drop = GameObject.Find("Canvas/Ranking/Panel-Jugador/Dropdown");
            if (drop != null) dropdownFiltroMapa = drop.GetComponent<TMP_Dropdown>();
        }
    }

    public void CargarRankingEnPantalla()
    {
        LimpiarFilasPrevias();

        if (DataManager.Instancia == null) return;

        string filtroNombre = inputBuscadorNombre != null ? inputBuscadorNombre.text : "";
        string filtroMapa = (dropdownFiltroMapa != null && dropdownFiltroMapa.options.Count > 0) ? dropdownFiltroMapa.options[dropdownFiltroMapa.value].text : "";

        if (textoTituloRanking != null)
        {
            if (string.IsNullOrEmpty(filtroMapa) || filtroMapa.Equals("Todos", System.StringComparison.OrdinalIgnoreCase))
                textoTituloRanking.text = "RANKING GLOBAL";
            else
                textoTituloRanking.text = "RANKING " + filtroMapa.ToUpper();
        }

        List<DataManager.EntradaRankingConPosicion> rankingFiltrado = DataManager.Instancia.ObtenerRankingFiltradoConPosicion(filtroNombre, filtroMapa);

        int totalPaginas = Mathf.Max(1, Mathf.CeilToInt((float)rankingFiltrado.Count / REGISTROS_POR_PAGINA));
        paginaActual = Mathf.Clamp(paginaActual, 0, totalPaginas - 1);

        if (textoNumeroPagina != null)
        {
            textoNumeroPagina.text = (paginaActual + 1).ToString();
        }

        if (botonPaginaAnterior != null)
            botonPaginaAnterior.interactable = (paginaActual > 0);

        if (botonPaginaSiguiente != null)
            botonPaginaSiguiente.interactable = (paginaActual < totalPaginas - 1);

        if (rankingFiltrado.Count == 0)
        {
            MostrarMensajeSinResultados(true);
            ActualizarFilasEstaticas(rankingFiltrado);
            return;
        }

        MostrarMensajeSinResultados(false);

        if (plantillaFila != null && contenedorFilas != null)
        {
            GenerarFilasDinamicas(rankingFiltrado);
        }
        else
        {
            ActualizarFilasEstaticas(rankingFiltrado);
        }
    }

    private void ActualizarFilasEstaticas(List<DataManager.EntradaRankingConPosicion> ranking)
    {
        if (textosNombresJugadores == null || textosTiemposJugadores == null) return;

        int desplazamiento = paginaActual * REGISTROS_POR_PAGINA;
        int espacioTotal = Mathf.Min(textosNombresJugadores.Length, textosTiemposJugadores.Length);

        for (int i = 0; i < espacioTotal; i++)
        {
            int indiceReal = desplazamiento + i;
            if (indiceReal < ranking.Count)
            {
                int posicion = ranking[indiceReal].posicionGlobal;

                if (textosNumerosPosicion != null && i < textosNumerosPosicion.Length)
                {
                    textosNumerosPosicion[i].gameObject.SetActive(true);
                    textosNumerosPosicion[i].text = posicion.ToString();
                }

                textosNombresJugadores[i].gameObject.SetActive(true);
                textosNombresJugadores[i].text = ranking[indiceReal].entrada.nombreJugador;

                textosTiemposJugadores[i].gameObject.SetActive(true);
                textosTiemposJugadores[i].text = FormatearTiempo(ranking[indiceReal].entrada.tiempo);
            }
            else
            {
                if (textosNumerosPosicion != null && i < textosNumerosPosicion.Length)
                    textosNumerosPosicion[i].text = "-";

                textosNombresJugadores[i].text = "---";
                textosTiemposJugadores[i].text = "--:--.---";
            }
        }
    }

    private void GenerarFilasDinamicas(List<DataManager.EntradaRankingConPosicion> ranking)
    {
        for (int i = 0; i < ranking.Count; i++)
        {
            GameObject nuevaFila = Instantiate(plantillaFila, contenedorFilas);
            nuevaFila.SetActive(true);
            filasGeneradas.Add(nuevaFila);

            TextMeshProUGUI[] componentesTexto = nuevaFila.GetComponentsInChildren<TextMeshProUGUI>();

            if (componentesTexto.Length >= 3)
            {
                int posicion = ranking[i].posicionGlobal;
                componentesTexto[0].text = posicion.ToString();
                componentesTexto[1].text = ranking[i].entrada.nombreJugador;
                componentesTexto[2].text = FormatearTiempo(ranking[i].entrada.tiempo);
            }
        }
    }

    private void LimpiarFilasPrevias()
    {
        foreach (GameObject fila in filasGeneradas)
        {
            if (fila != null) Destroy(fila);
        }
        filasGeneradas.Clear();
    }

    private void MostrarMensajeSinResultados(bool mostrar)
    {
        if (textoSinResultados != null)
        {
            textoSinResultados.gameObject.SetActive(mostrar);
        }
    }

    private string FormatearTiempo(float tiempoSegundos)
    {
        int min = (int)(tiempoSegundos / 60f);
        int seg = (int)(tiempoSegundos % 60f);
        int mili = (int)((tiempoSegundos - Mathf.Floor(tiempoSegundos)) * 1000f);
        return string.Format("{0:00}:{1:00}.{2:000}", min, seg, mili);
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

    private void LimpiarTablaRanking()
    {
        if (DataManager.Instancia != null)
        {
            DataManager.Instancia.LimpiarRanking();
            CargarRankingEnPantalla();
        }
    }
}
