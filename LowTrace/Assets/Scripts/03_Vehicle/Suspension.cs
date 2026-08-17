using UnityEngine;

public class Suspension : MonoBehaviour
{
    [System.Serializable]
    public class Rueda
    {
        public Transform pivote;
        public float radio = 0.28f;
        public bool activa = true;

        [HideInInspector] public float compresion;
    }

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Rueda[] ruedas;

    [Header("Suspension")]
    [Tooltip("Altura en reposo entre el pivote de la rueda y el suelo")]
    [SerializeField] private float alturaDescanso = 0.5f;
    [Tooltip("Fuerza del muelle: N por metro de compresion")]
    [SerializeField] private float rigidez = 1500f;
    [Tooltip("Frena la velocidad vertical del punto: N por m/s")]
    [SerializeField] private float amortiguacion = 250f;
    [Tooltip("Capas con que choca el raycast")]
    [SerializeField] private LayerMask mascaraSuelo = ~0;

    public Rigidbody Cuerpo => rb;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb == null || ruedas == null) return;

        foreach (Rueda r in ruedas)
        {
            if (r == null || !r.activa || r.pivote == null) continue;

            r.compresion = 0f;
            Vector3 punto = r.pivote.position;
            float maxDist = alturaDescanso + r.radio;

            if (!Physics.Raycast(punto, Vector3.down, out RaycastHit hit, maxDist, mascaraSuelo, QueryTriggerInteraction.Ignore))
                continue;

            r.compresion = Mathf.Clamp01((alturaDescanso - hit.distance) / alturaDescanso);

            float velPunto = Vector3.Dot(rb.GetPointVelocity(punto), Vector3.up);
            float fuerza = rigidez * Mathf.Max(0f, alturaDescanso - hit.distance)
                           - amortiguacion * velPunto;
            if (fuerza <= 0f) continue;

            rb.AddForceAtPosition(Vector3.up * fuerza, punto, ForceMode.Force);
        }
    }
}