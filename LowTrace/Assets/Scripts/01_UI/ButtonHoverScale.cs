using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Escala")]
    [SerializeField] private float escalaObjetivo = 1.1f;
    [SerializeField] private float velocidad = 10f;

    [Header("Color")]
    [SerializeField] private bool cambiarColor = true;
    [SerializeField] private Color colorHover = new Color(1f, 0.92f, 0.6f);
    [SerializeField] private Image imagen;

    [Header("Pulso permanente (boton Jugar)")]
    [SerializeField] private bool pulsoActivo = false;
    [SerializeField] private float amplitudPulso = 0.03f;
    [SerializeField] private float velocidadPulso = 2f;

    private RectTransform rect;
    private Vector3 escalaBase;
    private Coroutine corrutinaEscala;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        escalaBase = rect.localScale;
        if (imagen == null) imagen = GetComponent<Image>();
    }

    private void Update()
    {
        if (pulsoActivo)
        {
            float oscilacion = 1f + Mathf.Sin(Time.time * velocidadPulso) * amplitudPulso;
            rect.localScale = escalaBase * oscilacion;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        DetenerEscala();
        corrutinaEscala = StartCoroutine(AnimarEscala(escalaObjetivo));
        if (cambiarColor && imagen != null)
        {
            imagen.color = colorHover;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DetenerEscala();
        corrutinaEscala = StartCoroutine(AnimarEscala(1f));
        if (cambiarColor && imagen != null)
        {
            imagen.color = Color.white;
        }
    }

    private void DetenerEscala()
    {
        if (corrutinaEscala != null)
        {
            StopCoroutine(corrutinaEscala);
        }
    }

    private IEnumerator AnimarEscala(float objetivo)
    {
        Vector3 destino = escalaBase * objetivo;
        while (Vector3.Distance(rect.localScale, destino) > 0.001f)
        {
            rect.localScale = Vector3.Lerp(rect.localScale, destino, velocidad * Time.deltaTime);
            yield return null;
        }
        rect.localScale = destino;
    }
}
