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

        // En la compilación ejecutable, activar inmediatamente el Display 2 si hay 2 o más monitores conectados
        if (Display.displays.Length > 1)
        {
            int ancho = Display.displays[1].systemWidth;
            int alto = Display.displays[1].systemHeight;
            if (ancho <= 0) ancho = 1920;
            if (alto <= 0) alto = 1080;
            Display.displays[1].Activate(ancho, alto, 60);
        }

        SceneManager.sceneLoaded += OnEscenaCargada;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnEscenaCargada;
    }

    private void Start()
    {
        AsegurarRankingEnSegundoMonitor();
    }

    private void OnEscenaCargada(Scene escena, LoadSceneMode modo)
    {
        // Si no estamos cargando la escena de Ranking, asegurar que esté cargada en segundo plano para el Monitor 2
        if (escena.name != "Ranking")
        {
            AsegurarRankingEnSegundoMonitor();

            Canvas[] todosCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in todosCanvases)
            {
                if (c.gameObject.scene.name != "Ranking")
                {
                    c.targetDisplay = 0;
                }
            }

            Camera[] todasCamaras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in todasCamaras)
            {
                if (cam.gameObject.scene.name != "Ranking")
                {
                    cam.targetDisplay = 0;
                }
            }
        }

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

    private void AsegurarRankingEnSegundoMonitor()
    {
        bool esMultiMonitor = Display.displays.Length > 1;

#if UNITY_EDITOR
        // En el Editor de Unity, permitimos la carga aditiva para poder visualizar Display 1 y Display 2 en pestañas paralelas
        esMultiMonitor = true;
#endif

        if (esMultiMonitor && !EsEscenaRankingCargada())
        {
            SceneManager.LoadSceneAsync("Ranking", LoadSceneMode.Additive);
        }
    }

    private bool EsEscenaRankingCargada()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name == "Ranking")
            {
                return true;
            }
        }
        return false;
    }
}
