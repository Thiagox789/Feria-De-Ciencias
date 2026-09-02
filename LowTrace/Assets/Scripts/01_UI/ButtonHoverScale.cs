using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float escalaObjetivo = 1.1f;
    [SerializeField] private float velocidad = 10f;
    [SerializeField] private bool cambiarColor = true;
    [SerializeField] private Color colorHover = new Color(1f, 0.92f, 0.6f);
    [SerializeField] private Image imagen;
    [SerializeField] private bool pulsoActivo = false;
    [SerializeField] private float amplitudPulso = 0.03f;
    [SerializeField] private float velocidadPulso = 2f;

    private RectTransform rect;
    private Vector3 escalaBase;
    private bool sobreElemento;

    public void SetConfig(float escala, float vel, Color color, bool pulso)
    {
        escalaObjetivo = escala;
        velocidad = vel;
        colorHover = color;
        pulsoActivo = pulso;
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        escalaBase = rect.localScale;
        if (imagen == null) imagen = GetComponent<Image>();
    }

    private void Update()
    {
        float pulso = pulsoActivo && !sobreElemento ? 1f + Mathf.Sin(Time.time * velocidadPulso) * amplitudPulso : 1f;
        Vector3 destino = escalaBase * (sobreElemento ? escalaObjetivo : 1f) * pulso;
        rect.localScale = Vector3.Lerp(rect.localScale, destino, velocidad * Time.deltaTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        sobreElemento = true;
        if (cambiarColor && imagen != null) imagen.color = colorHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        sobreElemento = false;
        if (cambiarColor && imagen != null) imagen.color = Color.white;
    }
}