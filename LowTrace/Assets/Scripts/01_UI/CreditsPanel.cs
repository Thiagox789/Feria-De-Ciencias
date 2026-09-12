using UnityEngine;

// Este script controla la ventana de Créditos.
// Muestra el panel con los nombres de los creadores y oculta los otros botones.
public class CreditsPanel : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private GameObject panelCreditos;
    [SerializeField] private GameObject[] objetosAOcultar;

    // Muestra la ventana de créditos
    public void Abrir()
    {
        if (panelCreditos != null) panelCreditos.SetActive(true);

        if (objetosAOcultar != null)
        {
            foreach (GameObject objeto in objetosAOcultar)
            {
                if (objeto != null) objeto.SetActive(false);
            }
        }
    }

    // Cierra la ventana de créditos y vuelve a mostrar el menú
    public void Cerrar()
    {
        if (panelCreditos != null) panelCreditos.SetActive(false);

        if (objetosAOcultar != null)
        {
            foreach (GameObject objeto in objetosAOcultar)
            {
                if (objeto != null) objeto.SetActive(true);
            }
        }
    }

    // Botón para salir y volver al Menú Principal
    public void VolverAlMenu()
    {
        if (SceneLoader.Instancia != null)
        {
            SceneLoader.Instancia.VolverAlMenu();
        }
    }
}
