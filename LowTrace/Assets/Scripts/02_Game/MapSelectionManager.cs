using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MapSelectionManager : MonoBehaviour
{
    public static MapSelectionManager Instancia { get; private set; }

    [System.Serializable]
    public class MapaInfo
    {
        public string nombre;
        public string escena;
        public Sprite miniatura;
        public float tiempoRequerido;
        public string descripcion;
    }

    [Header("Mapas")]
    [SerializeField] private List<MapaInfo> mapas = new List<MapaInfo>();

    private int indiceSeleccionado = 0;

    public int IndiceSeleccionado => indiceSeleccionado;
    public MapaInfo MapaActual => mapas.Count > 0 ? mapas[indiceSeleccionado] : null;
    public List<MapaInfo> Mapas => mapas;

    public event System.Action<int> OnMapaCambiado;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        CargarSeleccion();
    }

    public void SeleccionarMapa(int indice)
    {
        if (indice < 0 || indice >= mapas.Count) return;

        if (!EsMapaDesbloqueado(indice)) return;

        indiceSeleccionado = indice;
        GuardarSeleccion();
        OnMapaCambiado?.Invoke(indice);
    }

    public void SeleccionarSiguiente()
    {
        int siguiente = indiceSeleccionado + 1;
        if (siguiente >= mapas.Count) siguiente = 0;

        while (siguiente != indiceSeleccionado && !EsMapaDesbloqueado(siguiente))
        {
            siguiente++;
            if (siguiente >= mapas.Count) siguiente = 0;
        }

        if (siguiente != indiceSeleccionado)
            SeleccionarMapa(siguiente);
    }

    public void SeleccionarAnterior()
    {
        int anterior = indiceSeleccionado - 1;
        if (anterior < 0) anterior = mapas.Count - 1;

        while (anterior != indiceSeleccionado && !EsMapaDesbloqueado(anterior))
        {
            anterior--;
            if (anterior < 0) anterior = mapas.Count - 1;
        }

        if (anterior != indiceSeleccionado)
            SeleccionarMapa(anterior);
    }

    public bool EsMapaDesbloqueado(int indice)
    {
        if (indice < 0 || indice >= mapas.Count) return false;
        return true;
    }

    public void CargarMapaSeleccionado()
    {
        if (mapas.Count == 0) return;
        string escena = mapas[indiceSeleccionado].escena;
        if (!string.IsNullOrEmpty(escena))
            SceneManager.LoadScene(escena);
    }

    public string ObtenerNombreEscenaActual()
    {
        if (mapas.Count == 0) return "Game";
        return mapas[indiceSeleccionado].escena;
    }

    private void GuardarSeleccion()
    {
        PlayerPrefs.SetInt("MapSelection", indiceSeleccionado);
        PlayerPrefs.Save();
    }

    private void CargarSeleccion()
    {
        if (PlayerPrefs.HasKey("MapSelection"))
            indiceSeleccionado = PlayerPrefs.GetInt("MapSelection");

        if (indiceSeleccionado < 0 || indiceSeleccionado >= mapas.Count)
            indiceSeleccionado = 0;
    }
}