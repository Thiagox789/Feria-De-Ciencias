using UnityEngine;

// Este script se coloca en cada rueda visual del auto (sin collider).
// Hace girar la rueda visualmente según qué tan rápido se mueve el chasis del auto.
public class Wheel : MonoBehaviour
{
    [Tooltip("El Rigidbody del auto (cuerpo físico principal)")]
    [SerializeField] private Rigidbody cuerpoAuto;
    [Tooltip("Radio de la rueda en metros")]
    [SerializeField] private float radioRueda = 0.3f;
    [Tooltip("Eje local de rotación de la llanta")]
    [SerializeField] private Vector3 ejeRotacion = new Vector3(0f, 0f, 1f);

    private void Awake()
    {
        // Si no se asignó en el Inspector, busca el Rigidbody en el auto padre
        if (cuerpoAuto == null)
        {
            cuerpoAuto = GetComponentInParent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        if (cuerpoAuto == null) return;

        // Calculamos cuánto avanza el auto hacia adelante
        Transform padre = transform.parent != null ? transform.parent : transform;
        float velocidadAvance = Vector3.Dot(cuerpoAuto.linearVelocity, padre.forward);

        // Calculamos la velocidad angular (cuántos radianes gira por segundo)
        float velocidadAngular = velocidadAvance / radioRueda;

        // Hacemos girar la rueda en su eje local
        transform.Rotate(ejeRotacion * (velocidadAngular * Mathf.Rad2Deg * Time.fixedDeltaTime), Space.Self);
    }
}