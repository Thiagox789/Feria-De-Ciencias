using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// Este script permite cambiar entre diferentes cámaras virtuales de Cinemachine
// durante la carrera presionando la tecla C para obtener tomas cinematográficas.
public class CinemachineCameraController : MonoBehaviour
{
    public static CinemachineCameraController Instancia { get; private set; }

    [Header("Lista de Cámaras de la Escena (Cinemachine)")]
    [Tooltip("Arrastra aquí las distintas Virtual Cameras (Ej: Trasera, Capó, Lateral Derrape, Frontal)")]
    [SerializeField] private List<GameObject> camarasCinemachine = new List<GameObject>();

    [Header("Tecla para cambiar de Cámara")]
    [SerializeField] private KeyCode teclaCambioCamara = KeyCode.C;

    private int indiceCamaraActual = 0;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    private void Start()
    {
        AutoBuscarCamaras();
        ActivarCamaraPorIndice(indiceCamaraActual);
    }

    private void AutoBuscarCamaras()
    {
        if (camarasCinemachine != null && camarasCinemachine.Count > 0) return;

        camarasCinemachine = new List<GameObject>();

        // Buscar objetos en la escena cuyo nombre contenga "vcam", "virtual", "camera" o "camara"
        foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            string nombre = go.name.ToLower();
            if (nombre.Contains("vcam") || nombre.Contains("cinemachine") || nombre.Contains("virtualcam"))
            {
                if (!camarasCinemachine.Contains(go))
                {
                    camarasCinemachine.Add(go);
                }
            }
        }
    }

    private void Update()
    {
        // Detectar si presiona la tecla C (compatible con Input heredado y New Input System)
        bool presionoC = Input.GetKeyDown(teclaCambioCamara) || 
                         (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame);

        if (presionoC)
        {
            CambiarASiguienteCamara();
        }
    }

    public void CambiarASiguienteCamara()
    {
        if (camarasCinemachine == null || camarasCinemachine.Count == 0)
        {
            AutoBuscarCamaras();
        }

        if (camarasCinemachine == null || camarasCinemachine.Count <= 1) return;

        indiceCamaraActual = (indiceCamaraActual + 1) % camarasCinemachine.Count;
        ActivarCamaraPorIndice(indiceCamaraActual);
    }

    public void ActivarCamaraPorIndice(int indice)
    {
        if (camarasCinemachine == null || camarasCinemachine.Count == 0) return;
        if (indice < 0 || indice >= camarasCinemachine.Count) return;

        for (int i = 0; i < camarasCinemachine.Count; i++)
        {
            if (camarasCinemachine[i] != null)
            {
                bool esActiva = (i == indice);
                camarasCinemachine[i].SetActive(esActiva);
            }
        }

        Debug.Log($"[Cinemachine] Cámara cambiada a: {camarasCinemachine[indice].name}");
    }
}
