using UnityEngine;

/// <summary>
/// Controlador de auto estilo arcade (tipo Hotshot Racing / Horizon Chase).
/// No usa WheelColliders: se basa en un Rigidbody simple + fuerzas manuales
/// para lograr un manejo rápido, directo y "gamey".
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Punto desde donde se calcula el centro de masa (opcional). Si es null, se usa el propio transform del auto.")]
    [SerializeField] private Transform centerOfMass;

    [Header("Aceleración / Velocidad")]
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private float reverseMaxSpeed = 15f;
    [SerializeField] private float brakeForce = 40f;
    [Tooltip("Desaceleración natural cuando no se presiona ni acelerar ni frenar.")]
    [SerializeField] private float naturalDeceleration = 10f;

    [Header("Dirección (Steering)")]
    [SerializeField] private float turnSpeed = 180f;
    [Tooltip("Curva que reduce la capacidad de giro a altas velocidades para dar sensación de control.")]
    [SerializeField] private AnimationCurve turnSpeedBySpeed = AnimationCurve.Linear(0f, 1f, 1f, 0.5f);

    [Header("Drift (Derrape)")]
    [SerializeField] private KeyCode driftKey = KeyCode.Space;
    [Tooltip("Grip lateral normal (0 = sin agarre, 1 = agarre total).")]
    [Range(0f, 1f)] [SerializeField] private float normalGrip = 0.95f;
    [Tooltip("Grip lateral durante el derrape. Cuanto más bajo, más patina el auto.")]
    [Range(0f, 1f)] [SerializeField] private float driftGrip = 0.35f;
    [Tooltip("Multiplicador extra de giro mientras se derrapa (para que el drift se sienta más brusco).")]
    [SerializeField] private float driftTurnMultiplier = 1.3f;

    [Header("Estabilidad")]
    [Tooltip("Empuja el auto hacia el suelo para que no salte en baches o rampas.")]
    [SerializeField] private float extraGravity = 10f;

    private Rigidbody rb;
    private float verticalInput;
    private float horizontalInput;
    private bool isDrifting;

    public float CurrentForwardSpeed { get; private set; }
    public float SpeedRatio01 { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (centerOfMass != null)
        {
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }
    }

    private void Update()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        isDrifting = Input.GetKey(driftKey);
    }

    private void FixedUpdate()
    {
        ApplyAcceleration();
        ApplySteering();
        ApplyLateralGrip();
        ApplyExtraGravity();
        UpdateSpeedInfo();
    }

    private void ApplyAcceleration()
    {
        Vector3 forward = transform.forward;
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, forward);

        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            bool isAccelerating = verticalInput > 0f;
            float limit = isAccelerating ? maxSpeed : reverseMaxSpeed;

            bool isBraking = (forwardSpeed > 0.5f && verticalInput < 0f) || (forwardSpeed < -0.5f && verticalInput > 0f);

            if (isBraking)
            {
                rb.AddForce(-forward * Mathf.Sign(forwardSpeed) * brakeForce, ForceMode.Acceleration);
            }
            else if (Mathf.Abs(forwardSpeed) < limit)
            {
                rb.AddForce(forward * verticalInput * acceleration, ForceMode.Acceleration);
            }
        }
        else
        {
            Vector3 decelForce = -forward * forwardSpeed * naturalDeceleration * Time.fixedDeltaTime;
            rb.AddForce(decelForce, ForceMode.VelocityChange);
        }
    }

    private void ApplySteering()
    {
        float speedFactor = Mathf.Clamp01(Mathf.Abs(CurrentForwardSpeed) / maxSpeed);
        float turnModifier = turnSpeedBySpeed.Evaluate(speedFactor);

        float movementFactor = Mathf.Clamp01(Mathf.Abs(CurrentForwardSpeed) / 2f);

        float driftMultiplier = isDrifting ? driftTurnMultiplier : 1f;

        float turnAmount = horizontalInput * turnSpeed * turnModifier * movementFactor * driftMultiplier;

        if (CurrentForwardSpeed < -0.5f)
        {
            turnAmount *= -1f;
        }

        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void ApplyLateralGrip()
    {
        Vector3 right = transform.right;
        Vector3 localVelocity = rb.linearVelocity;

        float lateralSpeed = Vector3.Dot(localVelocity, right);
        float currentGrip = isDrifting ? driftGrip : normalGrip;

        Vector3 lateralCorrection = -right * lateralSpeed * currentGrip;
        rb.AddForce(lateralCorrection, ForceMode.VelocityChange);
    }

    private void ApplyExtraGravity()
    {
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    private void UpdateSpeedInfo()
    {
        CurrentForwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        SpeedRatio01 = Mathf.Clamp01(Mathf.Abs(CurrentForwardSpeed) / maxSpeed);
    }

    public void ApplyBoost(float boostForce)
    {
        rb.AddForce(transform.forward * boostForce, ForceMode.VelocityChange);
    }
}
