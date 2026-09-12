using UnityEngine;
using UnityEngine.UI;

// Este script controla la navegación y los botones del Menú Principal del juego.
public class MenuManager : MonoBehaviour
{
    [Header("Botones con efecto hover y sonido")]
    [SerializeField] private Button[] botones;

    [Header("Configuración visual")]
    [SerializeField] private float escalaObjetivo = 1.1f;
    [SerializeField] private float velocidad = 10f;
    [SerializeField] private Color colorHover = new Color(1f, 0.72f, 0.3f);
    [SerializeField] private bool pulsoActivo = false;
    [SerializeField] private bool reproducirSonido = true;

    private void Awake()
    {
        if (botones == null) return;

        foreach (Button boton in botones)
        {
            if (boton == null) continue;

            UIButtonEffects efectos = boton.gameObject.GetComponent<UIButtonEffects>();
            if (efectos == null)
            {
                efectos = boton.gameObject.AddComponent<UIButtonEffects>();
            }

            efectos.SetConfig(escalaObjetivo, velocidad, colorHover, pulsoActivo, reproducirSonido);
        }
    }

    // Botón Jugar -> Carga la pantalla de selección de mapas
    public void Jugar()
    {
        CargarEscenaSegura("Seleccion-Mapa");
    }

    public void IrAMapa()
    {
        CargarEscenaSegura("Seleccion-Mapa");
    }

    public void VolverAlMenu()
    {
        CargarEscenaSegura("Menu");
    }

    public void IrAAjustes()
    {
        CargarEscenaSegura("Ajustes");
    }

    public void IrARanking()
    {
        CargarEscenaSegura("Ranking");
    }

    public void IrACreditos()
    {
        CargarEscenaSegura("Creditos");
    }

    public void Salir()
    {
        Application.Quit();
    }

    // Método seguro que garantiza cargar la escena sin importar si el singleton fue instanciado o no
    private void CargarEscenaSegura(string nombreEscena)
    {
        if (SceneLoader.Instancia != null)
        {
            SceneLoader.Instancia.CargarEscena(nombreEscena);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscena);
        }
    }
}
