using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instancia;

    public static SceneLoader Instancia
    {
        get
        {
            if (_instancia == null)
            {
                _instancia = FindFirstObjectByType<SceneLoader>();
                if (_instancia == null)
                {
                    GameObject objetoCargador = new GameObject("SceneLoader");
                    _instancia = objetoCargador.AddComponent<SceneLoader>();
                }
            }
            return _instancia;
        }
        private set { _instancia = value; }
    }

    // Escenas de UI donde NO debe haber skybox
    private static readonly string[] escenasUI = { "Menu", "Ajustes", "Ranking", "Creditos", "Seleccion-Mapa" };

    private void Awake()
    {
        if (_instancia != null && _instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        _instancia = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnEscenaCargada;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnEscenaCargada;
    }

    private void OnEscenaCargada(Scene escena, LoadSceneMode modo)
    {
        // Si es escena de UI, limpiar skybox
        foreach (string ui in escenasUI)
        {
            if (escena.name == ui)
            {
                RenderSettings.skybox = null;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientSkyColor = new Color(0.15f, 0.15f, 0.2f);
                RenderSettings.fog = false;
                return;
            }
        }
    }

    public static SceneLoader ObtenerInstancia()
    {
        return Instancia;
    }

    public void CargarEscena(string nombreEscena)
    {
        if (!string.IsNullOrEmpty(nombreEscena))
        {
            SceneManager.LoadScene(nombreEscena);
        }
    }

    public void CargarEscenaActual()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
