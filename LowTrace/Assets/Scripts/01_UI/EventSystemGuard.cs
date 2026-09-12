using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// Este script es un guardián del sistema de eventos de la interfaz (EventSystem).
// Asegura que siempre haya un EventSystem en cada escena para que los botones y la pantalla táctil o mouse respondan.
public class EventSystemGuard : MonoBehaviour
{
    private static EventSystemGuard instancia;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void AlCargarEscena(UnityEngine.SceneManagement.Scene escena, UnityEngine.SceneManagement.LoadSceneMode modo)
    {
        AsegurarEventSystem();
    }

    // Comprueba si existe un EventSystem y si no, lo crea automáticamente
    private void AsegurarEventSystem()
    {
        EventSystem existente = FindFirstObjectByType<EventSystem>();
        if (existente == null)
        {
            GameObject objetoEventSystem = new GameObject("EventSystem");
            objetoEventSystem.AddComponent<EventSystem>();
            objetoEventSystem.AddComponent<InputSystemUIInputModule>();
        }
        else if (existente.GetComponent<InputSystemUIInputModule>() == null)
        {
            existente.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }
}
