using UnityEngine;

// Este script se coloca en los objetos coleccionables de la pista (Monedas y Turbos).
// Hace girar el objeto en el aire y aplica el beneficio cuando el auto los toca.
[RequireComponent(typeof(Collider))]
public class TrackItem : MonoBehaviour
{
    public enum TipoItem
    {
        Moneda,
        Turbo
    }

    [Header("Configuración del Item")]
    [SerializeField] private TipoItem tipoItem = TipoItem.Moneda;
    [SerializeField] private string etiquetaJugador = "Player";

    [Header("Valores")]
    [SerializeField] private int puntosMoneda = 10;
    [SerializeField] private float fuerzaTurbo = 15f;

    [Header("Efectos Visuales y Sonido")]
    [SerializeField] private GameObject efectoVisualAlRecolectar;
    [SerializeField] private AudioClip sonidoAlRecolectar;
    [SerializeField] private float velocidadRotacion = 90f; // Grados por segundo que gira en el aire

    private bool yaRecolectado = false;

    private void Update()
    {
        // Hace girar el objeto continuamente para llamar la atención en la pista
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider otroObjeto)
    {
        if (yaRecolectado) return;
        if (!otroObjeto.CompareTag(etiquetaJugador)) return;

        WheelCarController auto = otroObjeto.GetComponent<WheelCarController>();
        if (auto == null) return;

        yaRecolectado = true;

        // Aplicamos el efecto de la moneda o del turbo
        AplicarEfecto(auto);

        // Reproducimos el sonido o destello de partículas
        ReproducirEfectos();

        // Ocultamos el objeto de la pista
        gameObject.SetActive(false);
    }

    private void AplicarEfecto(WheelCarController auto)
    {
        switch (tipoItem)
        {
            case TipoItem.Moneda:
                Debug.Log($"Moneda recogida: +{puntosMoneda} puntos");
                break;

            case TipoItem.Turbo:
                auto.ApplyBoost(fuerzaTurbo);
                Debug.Log("¡Turbo de velocidad activado!");
                break;
        }
    }

    private void ReproducirEfectos()
    {
        if (efectoVisualAlRecolectar != null)
        {
            Instantiate(efectoVisualAlRecolectar, transform.position, Quaternion.identity);
        }

        if (sonidoAlRecolectar != null)
        {
            AudioSource.PlayClipAtPoint(sonidoAlRecolectar, transform.position);
        }
    }
}
