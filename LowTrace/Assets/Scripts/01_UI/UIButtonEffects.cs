using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Este script se coloca en un Botón de la UI.
// Hace que el botón se agrande suavemente cuando pasamos el cursor por encima
// y reproduzca un sonido cuando hacemos clic.
[RequireComponent(typeof(Button))]
public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Efecto al pasar el cursor (Hover)")]
    [SerializeField] private float escalaObjetivo = 1.1f; // Cuánto se agranda el botón (1.1 = 10% más grande)
    [SerializeField] private float velocidadAnimacion = 10f; // Qué tan rápido se agranda o achica
    [SerializeField] private bool cambiarColor = true;
    [SerializeField] private Color colorHover = new Color(1f, 0.92f, 0.6f);

    [Header("Efecto de pulso continuo (Opcional)")]
    [SerializeField] private bool pulsoActivo = false;
    [SerializeField] private float amplitudPulso = 0.03f;
    [SerializeField] private float velocidadPulso = 2f;

    [Header("Sonido")]
    [SerializeField] private bool reproducirSonido = true;

    private RectTransform rectTransform;
    private Vector3 escalaBaseOriginal;
    private Image imagenBoton;
    private bool cursorEncima;
    private Button botonUI;

    // Permite cambiar la configuración desde otros scripts si es necesario
    public void Configurar(float escala, float velocidad, Color color, bool usarPulso, bool usarSonido = true)
    {
        escalaObjetivo = escala;
        velocidadAnimacion = velocidad;
        colorHover = color;
        pulsoActivo = usarPulso;
        reproducirSonido = usarSonido;
    }

    public void SetConfig(float escala, float velocidad, Color color, bool usarPulso, bool usarSonido = true)
    {
        Configurar(escala, velocidad, color, usarPulso, usarSonido);
    }

    protected virtual void Awake()
    {
        // Guardamos las referencias necesarias al iniciar el objeto
        rectTransform = GetComponent<RectTransform>();
        escalaBaseOriginal = rectTransform != null ? rectTransform.localScale : Vector3.one;
        imagenBoton = GetComponent<Image>();
        botonUI = GetComponent<Button>();

        // Escuchamos cuando el jugador hace clic en el botón
        if (botonUI != null)
        {
            botonUI.onClick.AddListener(AlHacerClic);
        }
    }

    // Se ejecuta al hacer clic en el botón
    private void AlHacerClic()
    {
        if (reproducirSonido && SoundManager.Instancia != null)
        {
            SoundManager.Instancia.PlaySFXBoton();
        }
    }

    private void Update()
    {
        if (rectTransform == null) return;

        // Si tiene efecto de pulso activo y el mouse no está encima, hace un latido suave
        float multiplicadorPulso = (pulsoActivo && !cursorEncima) ? 1f + Mathf.Sin(Time.time * velocidadPulso) * amplitudPulso : 1f;

        // Calculamos la escala destino (agrandado o normal)
        Vector3 escalaDestino = escalaBaseOriginal * (cursorEncima ? escalaObjetivo : 1f) * multiplicadorPulso;

        // Animamos suavemente la escala actual hacia la escala destino usando Lerp
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, escalaDestino, velocidadAnimacion * Time.deltaTime);
    }

    // Se ejecuta automáticamente cuando el cursor entra en el botón
    public void OnPointerEnter(PointerEventData datosEvento)
    {
        cursorEncima = true;
        if (cambiarColor && imagenBoton != null)
        {
            imagenBoton.color = colorHover;
        }
    }

    // Se ejecuta automáticamente cuando el cursor sale del botón
    public void OnPointerExit(PointerEventData datosEvento)
    {
        cursorEncima = false;
        if (cambiarColor && imagenBoton != null)
        {
            imagenBoton.color = Color.white;
        }
    }
}
