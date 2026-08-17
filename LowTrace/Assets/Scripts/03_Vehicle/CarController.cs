using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [SerializeField] private float velocidadMax = 35f;
    [SerializeField] private float aceleracion = 20f;
    [SerializeField] private float frenado = 25f;
    [SerializeField] private float retroceso = 6f;
    [SerializeField] private float velocidadGiro = 130f;

    [Header("Manejo")]
    [Tooltip("Rango de la velocidad de giro: 1 = maximo a baja velocidad, el segundo = a maxima velocidad")]
    [SerializeField] private Vector2 factorGiroVelocidad = new Vector2(1f, 0.55f);
    [Tooltip("Factor de alineacion de la direccion con el auto. Menor = mas derrape")]
    [SerializeField] private float alineacionNormal = 16f;
    [Tooltip("Factor de alineacion cuando se derrapa (Shift). Menor = mas desliz")]
    [SerializeField] private float alineacionDerrape = 3f;

    private Rigidbody rb;
    private float velocidadActual;
    private float yawActual;
    private Vector3 direccionMovimiento;

    public float VelocidadNormalizada => Mathf.InverseLerp(0f, velocidadMax, Mathf.Abs(velocidadActual));
    public float GiroEntrada { get; private set; }
    public bool Derrapando { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        yawActual = transform.eulerAngles.y;
        direccionMovimiento = transform.forward;

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        float adelante = 0f;
        if (kb.wKey.isPressed) adelante = 1f;
        if (kb.sKey.isPressed) adelante = -1f;

        float giro = 0f;
        if (kb.aKey.isPressed) giro = -1f;
        if (kb.dKey.isPressed) giro = 1f;
        GiroEntrada = giro;

        if (adelante != 0f && GameManager.Instancia.Estado == GameManager.EstadoJuego.Espera)
        {
            GameManager.Instancia.IniciarCarrera();
        }

        if (adelante > 0f)
            velocidadActual = Mathf.MoveTowards(velocidadActual, velocidadMax, aceleracion * Time.deltaTime);
        else if (adelante < 0f)
            velocidadActual = Mathf.MoveTowards(velocidadActual, -retroceso, frenado * Time.deltaTime);
        else
            velocidadActual = Mathf.MoveTowards(velocidadActual, 0f, frenado * Time.deltaTime);

        float sentido = velocidadActual >= 0f ? 1f : -1f;
        float velNorm = VelocidadNormalizada;

        float factorGiro = Mathf.Lerp(factorGiroVelocidad.x, factorGiroVelocidad.y, velNorm);
        yawActual += giro * velocidadGiro * factorGiro * sentido * Time.deltaTime;

        Derrapando = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
        float factorAlineacion = Derrapando ? alineacionDerrape : alineacionNormal;
        Quaternion rotAuto = Quaternion.Euler(0f, yawActual, 0f);
        direccionMovimiento = Vector3.Slerp(direccionMovimiento, rotAuto * Vector3.forward, factorAlineacion * Time.deltaTime).normalized;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 eulerActual = rb.rotation.eulerAngles;
        rb.MoveRotation(Quaternion.Euler(eulerActual.x, yawActual, eulerActual.z));

        Vector3 dirHorizontal = direccionMovimiento;
        dirHorizontal.y = 0f;
        dirHorizontal.Normalize();

        Vector3 velocidad = dirHorizontal * velocidadActual;
        velocidad.y = rb.linearVelocity.y;
        rb.linearVelocity = velocidad;
    }
}