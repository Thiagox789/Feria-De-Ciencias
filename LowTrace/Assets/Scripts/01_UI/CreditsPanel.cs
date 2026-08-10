using UnityEngine;

public class CreditsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject[] objetosAOcultar;

    public void Abrir()
    {
        if (panel != null) panel.SetActive(true);
        if (objetosAOcultar != null)
        {
            foreach (GameObject obj in objetosAOcultar)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }

    public void Cerrar()
    {
        if (panel != null) panel.SetActive(false);
        if (objetosAOcultar != null)
        {
            foreach (GameObject obj in objetosAOcultar)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }
}
