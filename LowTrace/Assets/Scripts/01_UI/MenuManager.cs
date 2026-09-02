using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Botones con efecto hover")]
    [SerializeField] private Button[] botones;

    [Header("Configuración hover")]
    [SerializeField] private float escalaObjetivo = 1.1f;
    [SerializeField] private float velocidad = 10f;
    [SerializeField] private Color colorHover = new Color(1f, 0.72f, 0.3f);
    [SerializeField] private bool pulsoActivo = false;

    private void Awake()
    {
        if (botones == null) return;

        foreach (Button boton in botones)
        {
            if (boton == null) continue;

            ButtonHoverScale hover = boton.gameObject.GetComponent<ButtonHoverScale>();
            if (hover == null)
                hover = boton.gameObject.AddComponent<ButtonHoverScale>();

            hover.SetConfig(escalaObjetivo, velocidad, colorHover, pulsoActivo);
        }
    }

    public void Jugar()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.CargarEscena("IA");
    }

    public void IrAMapa()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.CargarEscena("Mapa");
    }

    public void VolverAlMenu()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.VolverAlMenu();
    }

    public void IrAAjustes()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.CargarEscena("Ajustes");
    }

    public void IrARanking()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.CargarEscena("Ranking");
    }

    public void IrACreditos()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.CargarEscena("Creditos");
    }

    public void Salir()
    {
        Application.Quit();
    }
}
