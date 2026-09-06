using UnityEngine;

public class Pruebas : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRB;

    [Tooltip("Puntos FIJOS de anclaje de suspensión (hardpoints), hijos del chasis. NO son el mesh visual.")]
    [SerializeField] private Transform[] rayPoints;

    [Tooltip("Mallas visuales de cada rueda, hijas de cada rayPoint correspondiente (mismo orden).")]
    [SerializeField] private Transform[] wheelMeshes;

    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform accelerationPoint;

    [Header("Suspension Settings")]
    [Tooltip("Rigidez del resorte (N/m). Valores típicos: 20000-50000 para auto arcade.")]
    [SerializeField] private float springStiffness = 35000f;
    [Tooltip("Longitud en reposo del resorte (distancia hardpoint→suelo en reposo). Típico: 0.3-0.5m.")]
    [SerializeField] private float restLength = 0.4f;
    [Tooltip("Recorrido máximo de compresión del resorte. Típico: 0.2-0.4m.")]
    [SerializeField] private float springTravel = 0.3f;
    [Tooltip("Radio de la rueda (para cálculo visual y offset del raycast).")]
    [SerializeField] private float wheelRadius = 0.33f;
    [Tooltip("Amortiguación (N·s/m). Debe ser ~5-15% de springStiffness.")]
    [SerializeField] private float damperStiffness = 4500f;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 22f;
    [SerializeField] private float deceleration = 8f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float maxSpeedReverse = 12f;
    [SerializeField] private float steerStrength = 7f;
    [SerializeField] private float maxYawVelocity = 0.9f;
    [SerializeField] private AnimationCurve turningCurve;
    [Tooltip("Ya no se usa directamente; el grip ahora cancela velocidad lateral vía VelocityChange.")]
    [SerializeField] private float dragCoefficient = 7f;

    [Header("Stability")]
    [SerializeField] private float antiRollStrength = 5000f;
    [SerializeField] private bool autoResetIfFlipped = true;
    [SerializeField] private float flipTimeout = 3f;

    [Header("Wheel Visual Rotation (opcional)")]
    [Tooltip("Si querés que las ruedas visualmente giren al andar, además de subir/bajar.")]
    [SerializeField] private bool spinWheelMeshes = true;
    private float[] wheelSpinAngle;

    [Header("Inputs (debug)")]
    [SerializeField] private float moveInput;
    [SerializeField] private float steerInput;

    [Tooltip("Velocidad de interpolación del steer con teclado (más alto = más inmediato).")]
    [SerializeField] private float steerLerpSpeed = 8f;
    private float steerInputRaw;

    private int[] wheelIsGrounded = new int[4];
    private bool isGrounded;
    private Vector3 currentCarLocalVelocity;
    private float carVelocityRatio;
    
    [Header("Drift")]
    [SerializeField] private KeyCode driftKey = KeyCode.LeftShift;
    [SerializeField] private float normalGrip = 0.88f;
    [SerializeField] private float driftGrip = 0.3f;
    [SerializeField] private float driftTurnMultiplier = 1.4f;
    private bool isDrifting;
private bool carreraIniciada;
    private float flippedTimer;

    public float GiroEntrada => steerInput;
    public float VelocidadNormalizada => carVelocityRatio;

    private void Start()
    {
        if (carRB == null)
            carRB = GetComponent<Rigidbody>();

        if (turningCurve == null || turningCurve.length == 0)
        {
            // Curva agresiva: a alta velocidad el auto apenas puede girar (más controlable)
            turningCurve = new AnimationCurve(
                new Keyframe(0f,   1.0f),
                new Keyframe(0.25f, 0.7f),
                new Keyframe(0.55f, 0.35f),
                new Keyframe(1.0f, 0.15f)
            );
        }

        wheelSpinAngle = new float[rayPoints.Length];
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

    // La posición/rotación visual de las ruedas se actualiza después de que
    // toda la física del frame terminó de resolverse (evita jitter).
    private void LateUpdate()
    {
        UpdateWheelVisuals();
    }

    #region Input Handling
    private void GetPlayerInput()
    {
        moveInput = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))   moveInput =  1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveInput = -1;

        isDrifting = Input.GetKey(driftKey);

        // Leer el steer crudo (teclado: -1, 0 o 1)
        steerInputRaw = 0;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) steerInputRaw =  1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  steerInputRaw = -1;

        // Interpolar suavemente para evitar el giro todo-o-nada con teclado
        steerInput = Mathf.Lerp(steerInput, steerInputRaw, steerLerpSpeed * Time.deltaTime);

        if (!carreraIniciada && (moveInput != 0 || Mathf.Abs(steerInput) > 0.05f))
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

        float directionSign = Mathf.Abs(currentCarLocalVelocity.z) > 0.5f
            ? Mathf.Sign(currentCarLocalVelocity.z)
            : 1f;

        // Giro máximo recién a ~6 m/s; a velocidades bajas el auto gira con menos fuerza
        float speedFactor = Mathf.Clamp01(Mathf.Abs(currentCarLocalVelocity.z) / 6f);

        float driftMult = isDrifting ? driftTurnMultiplier : 1f;
        float turnAmount = steerStrength * steerInput * curveValue * directionSign * speedFactor * driftMult;
        carRB.AddTorque(transform.up * turnAmount, ForceMode.Acceleration);

        Vector3 localAngVel = transform.InverseTransformDirection(carRB.angularVelocity);
        localAngVel.y = Mathf.Clamp(localAngVel.y, -maxYawVelocity, maxYawVelocity);
        carRB.angularVelocity = transform.TransformDirection(localAngVel);
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;
        float currentGrip = isDrifting ? driftGrip : normalGrip;

        // Cancelar directamente la fracción de velocidad lateral (VelocityChange = m/s instantáneos)
        // Esto da grip real en lugar de solo "empujar" en contra de la velocidad lateral.
        Vector3 lateralVelocity = transform.right * currentSidewaysSpeed;
        carRB.AddForce(-lateralVelocity * currentGrip, ForceMode.VelocityChange);
    }
    #endregion

    #region Suspension

    // Guardamos, por rueda, cuánto se comprimió el resorte este frame (0 = extendido, 1 = tope).
    // Lo usamos después en UpdateWheelVisuals() para posicionar el mesh sin recalcular el raycast.
    private float[] lastCompressionRatio;
    private RaycastHit[] lastHit;
    private bool[] lastHitValid;

    private void Suspension()
    {
        // Inicialización perezosa de los arrays de cache (una sola vez)
        if (lastCompressionRatio == null || lastCompressionRatio.Length != rayPoints.Length)
        {
            lastCompressionRatio = new float[rayPoints.Length];
            lastHit = new RaycastHit[rayPoints.Length];
            lastHitValid = new bool[rayPoints.Length];
        }

        float maxLength = restLength + springTravel;
        Vector3 rayDir = Vector3.down; // Siempre hacia abajo en mundo (no rota con el auto)

        for (int i = 0; i < rayPoints.Length; i++)
        {
            // Offset: iniciar el raycast desde arriba del hardpoint para evitar self-intersection
            Vector3 rayOrigin = rayPoints[i].position + rayDir * wheelRadius;

            if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, maxLength + wheelRadius, drivable))
            {
                wheelIsGrounded[i] = 1;
                lastHitValid[i] = true;
                lastHit[i] = hit;

                float currentSpringLength = hit.distance;
                float springCompression = Mathf.Clamp01((restLength - currentSpringLength) / springTravel);
                lastCompressionRatio[i] = springCompression;

                float springForce = springCompression * springStiffness;

                // Velocidad relativa del punto de contacto en dirección de la normal del suelo
                Vector3 pointVelocity = carRB.GetPointVelocity(hit.point);
                float springVelocity = Vector3.Dot(hit.normal, pointVelocity);
                float dampForce = springVelocity * damperStiffness;

                float netForce = springForce - dampForce;
                // IMPORTANTE: Aplicar fuerza EN EL PUNTO DE CONTACTO (hit.point), no en el hardpoint
                carRB.AddForceAtPosition(hit.normal * netForce, hit.point);
            }
            else
            {
                wheelIsGrounded[i] = 0;
                lastHitValid[i] = false;
                lastCompressionRatio[i] = 0f; // sin contacto: rueda totalmente extendida
            }
        }
    }

    /// <summary>
    /// Posiciona cada mesh de rueda en su punto real de contacto (o totalmente
    /// extendido si no hay contacto), en vez de dejarlo fijo respecto al chasis.
    /// Esto es lo que resuelve el "hundimiento" visual en rampas y desniveles.
    /// </summary>
    private void UpdateWheelVisuals()
    {
        if (wheelMeshes == null || rayPoints == null) return;

        float maxLength = restLength + springTravel;

        for (int i = 0; i < rayPoints.Length && i < wheelMeshes.Length; i++)
        {
            if (wheelMeshes[i] == null) continue;

            float distanceDown;

            if (lastHitValid != null && i < lastHitValid.Length && lastHitValid[i])
            {
                // Con contacto: la rueda se posiciona justo tocando el punto de impacto
                distanceDown = lastHit[i].distance + wheelRadius;
            }
            else
            {
                // Sin contacto: la rueda "cuelga" hasta el límite de su recorrido
                distanceDown = maxLength + wheelRadius;
            }

            // Usar Vector3.down (mundo) porque el raycast ahora siempre va hacia abajo en mundo
            wheelMeshes[i].position = rayPoints[i].position - Vector3.up * distanceDown;
            wheelMeshes[i].rotation = rayPoints[i].rotation;

            // Rotación de giro de la rueda sobre su propio eje (efecto visual, no físico)
            if (spinWheelMeshes && wheelIsGrounded[i] == 1)
            {
                float wheelCircumference = 2f * Mathf.PI * wheelRadius;
                float forwardSpeed = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].forward);
                float spinDelta = (forwardSpeed / wheelCircumference) * 360f * Time.deltaTime;

                wheelSpinAngle[i] += spinDelta;
                wheelMeshes[i].Rotate(Vector3.right, spinDelta, Space.Self);
            }
        }
    }
    #endregion

    #region Stability
    /// <summary>
    /// Corrige SOLO el roll lateral (volcado de costado), ignorando el pitch
    /// natural de subir/bajar rampas. Antes esto usaba un Dot global que
    /// confundía "estar en una rampa" con "estar por volcar".
    /// </summary>
    private void AntiRoll()
    {
        if (!isGrounded) return;

        Vector3 localUp = transform.InverseTransformDirection(transform.up);
        float rollAngle = Mathf.Atan2(localUp.x, localUp.y) * Mathf.Rad2Deg;

        if (Mathf.Abs(rollAngle) > 2f)
        {
            float correctionTorque = -rollAngle * antiRollStrength * Mathf.Deg2Rad;
            carRB.AddTorque(transform.forward * correctionTorque, ForceMode.Acceleration);
        }
    }

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

    private void OnDrawGizmos()
    {
        if (rayPoints == null) return;

        float maxLength = restLength + springTravel;

        for (int i = 0; i < rayPoints.Length; i++)
        {
            if (rayPoints[i] == null) continue;

            bool grounded = Application.isPlaying && wheelIsGrounded != null && i < wheelIsGrounded.Length && wheelIsGrounded[i] == 1;
            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawLine(rayPoints[i].position, rayPoints[i].position - Vector3.up * (maxLength + wheelRadius));
            Gizmos.DrawWireSphere(rayPoints[i].position - Vector3.up * (maxLength + wheelRadius), 0.05f);
        }
    }
}
