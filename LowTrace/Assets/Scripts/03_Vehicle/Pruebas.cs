using UnityEngine;

public class Pruebas : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform accelerationPoint;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness = 30000f;
    [SerializeField] private float restLength = 1f;
    [SerializeField] private float springTravel = 0.5f;
    [SerializeField] private float wheelRadius = 0.33f;
    [SerializeField] private float damperStiffness = 3000f;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 22f;
    [SerializeField] private float deceleration = 8f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float maxSpeedReverse = 12f;
    [SerializeField] private float steerStrength = 10f;
    [SerializeField] private float maxYawVelocity = 0.7f;  // Velocidad angular máxima en Y (evita giro inestable)
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 7f;

    [Header("Stability")]
    [SerializeField] private float antiRollStrength = 5000f;
    [SerializeField] private bool autoResetIfFlipped = true;
    [SerializeField] private float flipTimeout = 3f;

    [Header("Inputs (debug)")]
    [SerializeField] private float moveInput;
    [SerializeField] private float steerInput;

    // Estado del coche
    private int[] wheelIsGrounded = new int[4];
    private bool isGrounded;
    private Vector3 currentCarLocalVelocity;
    private float carVelocityRatio;
    private bool carreraIniciada;
    private float flippedTimer;

    public float GiroEntrada => steerInput;
    public float VelocidadNormalizada => carVelocityRatio;

    private void Start()
    {
        if (carRB == null)
            carRB = GetComponent<Rigidbody>();

        // Curva de giro por defecto: maximo giro cuando el auto está parado,
        // reduciendo progresivamente a altas velocidades para estabilidad.
        if (turningCurve == null || turningCurve.length == 0)
        {
            turningCurve = new AnimationCurve(
                new Keyframe(0f,   1.0f),
                new Keyframe(0.3f, 0.9f),
                new Keyframe(0.7f, 0.6f),
                new Keyframe(1.0f, 0.35f)
            );
        }
    }

    private void Update()
    {
        GetPlayerInput();
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
        AntiRoll();
        CheckFlipped();
    }

    #region Input Handling
    private void GetPlayerInput()
    {
        moveInput = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))   moveInput =  1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveInput = -1;

        steerInput = 0;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) steerInput =  1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  steerInput = -1;

        if (!carreraIniciada && (moveInput != 0 || steerInput != 0))
        {
            carreraIniciada = true;
            if (GameManager.Instancia != null)
                GameManager.Instancia.IniciarCarrera();
        }
    }
    #endregion

    #region Car Status Check
    private void GroundCheck()
    {
        int tempGroundedWheels = 0;
        for (int i = 0; i < wheelIsGrounded.Length; i++)
            tempGroundedWheels += wheelIsGrounded[i];

        isGrounded = tempGroundedWheels > 1;
    }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.linearVelocity);
        carVelocityRatio = Mathf.Abs(currentCarLocalVelocity.z) / maxSpeed;
    }
    #endregion

    #region Movement
    private void Movement()
    {
        if (!isGrounded) return;

        Acceleration();
        Deceleration();
        Turn();
        SidewaysDrag();
    }

    private void Acceleration()
    {
        // Límite de velocidad diferenciado adelante / reversa
        float currentForwardSpeed = currentCarLocalVelocity.z;
        bool atMaxForward = moveInput > 0 && currentForwardSpeed >=  maxSpeed;
        bool atMaxReverse = moveInput < 0 && currentForwardSpeed <= -maxSpeedReverse;
        if (atMaxForward || atMaxReverse) return;

        carRB.AddForce(acceleration * moveInput * transform.forward, ForceMode.Acceleration);
    }

    private void Deceleration()
    {
        if (moveInput == 0 && Mathf.Abs(currentCarLocalVelocity.z) > 0.1f)
        {
            Vector3 forwardDirection = Mathf.Sign(currentCarLocalVelocity.z) * transform.forward;
            carRB.AddForce(deceleration * -forwardDirection, ForceMode.Acceleration);
        }
    }

    private void Turn()
    {
        float curveValue = turningCurve.Evaluate(carVelocityRatio);

        // Invertir giro al ir en reversa (comportamiento natural de un auto real)
        float directionSign = Mathf.Abs(currentCarLocalVelocity.z) > 0.5f
            ? Mathf.Sign(currentCarLocalVelocity.z)
            : 1f;

        // Evitar que el auto gire en el lugar (tank turn) cuando está quieto
        // Necesita moverse al menos un poco para poder aplicar torque de giro
        float speedFactor = Mathf.Clamp01(Mathf.Abs(currentCarLocalVelocity.z) / 1.5f);

        float turnAmount = steerStrength * steerInput * curveValue * directionSign * speedFactor;
        carRB.AddTorque(transform.up * turnAmount, ForceMode.Acceleration);

        // --- Clamp de velocidad angular en Y ---
        // Evita que a alta velocidad el auto acumule demasiada rotación y se dispare
        Vector3 localAngVel = transform.InverseTransformDirection(carRB.angularVelocity);
        localAngVel.y = Mathf.Clamp(localAngVel.y, -maxYawVelocity, maxYawVelocity);
        carRB.angularVelocity = transform.TransformDirection(localAngVel);
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        // Drag lateral cuadrático: a más velocidad lateral, más fuerza de corrección
        // Signo preservado con Abs para que la dirección sea siempre correcta
        float dragForceMagnitude = -Mathf.Sign(currentSidewaysSpeed)
            * dragCoefficient
            * Mathf.Abs(currentSidewaysSpeed);

        Vector3 dragForce = transform.right * dragForceMagnitude;
        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }
    #endregion

    #region Suspension
    private void Suspension()
    {
        float maxLength = restLength + springTravel;

        for (int i = 0; i < rayPoints.Length; i++)
        {
            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out RaycastHit hit, maxLength + wheelRadius, drivable))
            {
                wheelIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;
                float springForce = springCompression * springStiffness;

                Vector3 pointVelocity = carRB.GetPointVelocity(rayPoints[i].position);
                float springVelocity = Vector3.Dot(rayPoints[i].up, pointVelocity);
                float dampForce = springVelocity * damperStiffness;

                float netForce = springForce - dampForce;
                carRB.AddForceAtPosition(rayPoints[i].up * netForce, rayPoints[i].position);
            }
            else
            {
                wheelIsGrounded[i] = 0;
            }
        }
    }
    #endregion

    #region Stability
    /// <summary>
    /// Torque correctivo que evita que el auto se vuelque en curvas o terrenos
    /// irregulares. Solo actúa cuando hay inclinación lateral notable.
    /// </summary>
    private void AntiRoll()
    {
        if (!isGrounded) return;

        Vector3 upDir = transform.up;
        float rollAngle = Vector3.Dot(upDir, Vector3.up); // 1 = derecho, 0 = volcado

        if (rollAngle < 0.98f)
        {
            Vector3 rollCorrection = Vector3.Cross(upDir, Vector3.up);
            carRB.AddTorque(rollCorrection * antiRollStrength * (1f - rollAngle), ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// Detecta si el auto quedó volcado y lo endereza automáticamente tras flipTimeout segundos.
    /// </summary>
    private void CheckFlipped()
    {
        if (!autoResetIfFlipped) return;

        bool flipped = Vector3.Dot(transform.up, Vector3.up) < 0.1f;

        if (flipped)
        {
            flippedTimer += Time.fixedDeltaTime;
            if (flippedTimer >= flipTimeout)
            {
                Vector3 pos = transform.position + Vector3.up * 1.5f;
                transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
                carRB.linearVelocity = Vector3.zero;
                carRB.angularVelocity = Vector3.zero;
                flippedTimer = 0f;
            }
        }
        else
        {
            flippedTimer = 0f;
        }
    }
    #endregion
}