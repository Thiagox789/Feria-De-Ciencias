using UnityEngine;

public class Wheel : MonoBehaviour
{
    [Tooltip("Auto con el Rigidbody (se usa para leer la velocidad)")]
    [SerializeField] private Rigidbody rb;
    [Tooltip("Radio de la rueda en metros")]
    [SerializeField] private float radio = 0.3f;
    [Tooltip("Eje local de rotacion de la rueda (eje de la llanta)")]
    [SerializeField] private Vector3 ejeGiro = new Vector3(0f, 0f, 1f);

    private void Awake()
    {
        if (rb == null) rb = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        Transform raiz = transform.parent != null ? transform.parent : transform;
        float avance = Vector3.Dot(rb.linearVelocity, raiz.forward);

        float velocidadAngular = avance / radio;
        transform.Rotate(ejeGiro * (velocidadAngular * Mathf.Rad2Deg * Time.fixedDeltaTime), Space.Self);
    }
}