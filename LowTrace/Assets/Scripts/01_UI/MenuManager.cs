using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("IA");
    }

    public void IrAMapa()
    {
        SceneManager.LoadScene("Mapa");
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void IrAAjustes()
    {
        SceneManager.LoadScene("Ajustes");
    }

    public void IrARanking()
    {
        SceneManager.LoadScene("Ranking");
    }

    public void IrACreditos()
    {
        SceneManager.LoadScene("Creditos");
    }

    public void Salir()
    {
        Application.Quit();
    }
}
