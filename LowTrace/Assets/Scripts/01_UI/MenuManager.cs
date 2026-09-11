using UnityEngine;
using UnityEngine.UI;

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
                efectos = boton.gameObject.AddComponent<UIButtonEffects>();

            efectos.SetConfig(escalaObjetivo, velocidad, colorHover, pulsoActivo, reproducirSonido);
        }
    }

    public void Jugar()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.CargarEscena("Seleccion-Mapa");
    }

    public void IrAMapa()
    {
        if (SceneLoader.Instancia != null) SceneLoader.Instancia.CargarEscena("Seleccion-Mapa");
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
