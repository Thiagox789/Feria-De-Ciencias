using UnityEngine;

/// <summary>
/// Controlador de auto arcade con WheelColliders.
/// Aceleración, freno y dirección pensados para sentirse tipo
/// juego low-poly de carreras (giro rápido, responsivo, exagerado).
/// Sin marchas, sin luces.
/// </summary>
public class WheelCarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private WheelCollider frontLeftWheel;
    [SerializeField] private WheelCollider frontRightWheel;
    [SerializeField] private WheelCollider rearLeftWheel;
    [SerializeField] private WheelCollider rearRightWheel;
    [SerializeField] private Transform frontLeftMesh;
    [SerializeField] private Transform frontRightMesh;
    [SerializeField] private Transform rearLeftMesh;
    [SerializeField] private Transform rearRightMesh;

    private Quaternion[] meshInitialRotations = new Quaternion[4];

    [Header("Acceleration")]
    [SerializeField] private float motorTorque = 4000f;
    [SerializeField] private float maxForwardSpeed = 250f;
    [SerializeField] private float maxReverseSpeed = 60f;

    [Header("Braking")]
    [SerializeField] private float brakeTorque = 4000f;
    [Tooltip("Freno de motor al soltar el acelerador. Evita que el auto siga deslizando sin control.")]
    [SerializeField] private float engineBrakeTorque = 800f;

    [Header("Steering (Arcade)")]
    [Tooltip("Ángulo máximo de las ruedas a velocidad baja/cero.")]
    [SerializeField] private float maxSteerAngle = 35f;
    [Tooltip("Ángulo mínimo de las ruedas a velocidad máxima.")]
    [SerializeField] private float minSteerAngleAtHighSpeed = 18f;
    [Tooltip("Velocidad del lerp del steer en FixedUpdate (0.05=lento, 0.3=rápido).")]
    [SerializeField] [Range(0.05f, 0.4f)] private float steerLerpSpeed = 0.15f;

    [Header("Arcade Turn Assist")]
    [Tooltip("Torque extra de rotación. Mantenerlo BAJO (5-15) para no dominar los WheelColliders.")]
    [SerializeField] private float turnAssistTorque = 10f;
    [Tooltip("Velocidad mínima (km/h) para que el turn assist empiece a actuar.")]
    [SerializeField] private float minSpeedForAssist = 15f;
    [Tooltip("Velocidad angular máxima permitida (rad/s). Techo duro contra el spin infinito.")]
    [SerializeField] private float maxAngularVelocityY = 1.8f;
    [Tooltip("Damping angular cuando no se gira (frena el spin residual rápidamente).")]
    [SerializeField] private float angularDampingIdle = 10f;
    [Tooltip("Damping angular mientras se gira (más suave para no cortar el giro).")]
    [SerializeField] private float angularDampingSteer = 5f;

    [Header("Grip / Drift")]
    [Tooltip("Rigidez de agarre lateral trasero.")]
    [SerializeField] private float rearGripStiffness = 1.2f;
    [SerializeField] private float frontGripStiffness = 1.6f;

    [Header("Lateral Damping")]
    [Tooltip("Amortigua la velocidad lateral en cada FixedUpdate. 1.0 = sin amortiguación, 0.85 = mucha. Evita el deslizamiento descontrolado.")]
    [SerializeField] [Range(0.8f, 1f)] private float lateralDamping = 0.92f;

    [Header("Physics")]
    [SerializeField] private Transform centerOfMass;

    [Header("Debug")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private float speedFactor;
    [SerializeField] private float currentSteerAngle;
    [SerializeField] private float steerInput;
    [SerializeField] private float accelInput;
    [SerializeField] private float brakeInput;

    private bool carreraIniciada;

    // Propiedades pblicas para ArcadeCameraController y TrackItem
    public float SpeedRatio01 => speedFactor;
    public float CurrentForwardSpeed => currentSpeed / 3.6f;
    public float CurrentSpeedKMH => Mathf.Abs(currentSpeed);

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (centerOfMass != null)
            rb.centerOfMass = centerOfMass.localPosition;

        // Limitar la velocidad angular máxima para evitar el spin descontrolado
        rb.maxAngularVelocity = maxAngularVelocityY;

        meshInitialRotations[0] = frontLeftMesh != null ? frontLeftMesh.localRotation : Quaternion.identity;
        meshInitialRotations[1] = frontRightMesh != null ? frontRightMesh.localRotation : Quaternion.identity;
        meshInitialRotations[2] = rearLeftMesh != null ? rearLeftMesh.localRotation : Quaternion.identity;
        meshInitialRotations[3] = rearRightMesh != null ? rearRightMesh.localRotation : Quaternion.identity;

        ApplyGripSettings();
    }

    private void ApplyGripSettings()
    {
        SetSidewaysStiffness(frontLeftWheel, frontGripStiffness);
        SetSidewaysStiffness(frontRightWheel, frontGripStiffness);
        SetSidewaysStiffness(rearLeftWheel, rearGripStiffness);
        SetSidewaysStiffness(rearRightWheel, rearGripStiffness);
    }

    private void SetSidewaysStiffness(WheelCollider col, float stiffness)
    {
        if (col == null) return;
        WheelFrictionCurve curve = col.sidewaysFriction;
        curve.stiffness = stiffness;
        col.sidewaysFriction = curve;
    }

    private void Update()
    {
        // Solo lectura cruda de teclas (sin lerp aquí, el lerp va en FixedUpdate)
        accelInput = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1f : 0f;
        brakeInput  = (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? 1f : 0f;

        if (!carreraIniciada && (accelInput != 0 || Mathf.Abs(steerInput) > 0.05f))
        {
            carreraIniciada = true;
            if (GameManager.Instancia != null)
                GameManager.Instancia.IniciarCarrera();
        }
    }

    private void FixedUpdate()
    {
        SmoothSteerInput();   // Lerp del steer aquí (frame-rate correcto)
        CalculateSpeed();
        Acceleration();
        Braking();
        EngineBraking();
        Steering();
        TurnAssist();
        LateralDamping();
        SyncWheels();
    }

    /// <summary>
    /// Suaviza el steer en FixedUpdate con Time.fixedDeltaTime.
    /// Antes estaba en Update con Time.fixedDeltaTime → bug de frame-rate.
    /// </summary>
    private void SmoothSteerInput()
    {
        float rawSteer = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) rawSteer =  1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  rawSteer = -1f;
        steerInput = Mathf.Lerp(steerInput, rawSteer, steerLerpSpeed);
    }

    private void CalculateSpeed()
    {
        currentSpeed = Vector3.Dot(rb.transform.forward, rb.linearVelocity) * 3.6f;
        speedFactor = Mathf.InverseLerp(0f, maxForwardSpeed, Mathf.Abs(currentSpeed));
    }

    private void Acceleration()
    {
        float motorForce = Mathf.Lerp(motorTorque, 0f, speedFactor);

        if (accelInput > 0f)
        {
            if (Mathf.Abs(currentSpeed) < maxForwardSpeed)
            {
                rearLeftWheel.motorTorque = motorForce * accelInput;
                rearRightWheel.motorTorque = motorForce * accelInput;
            }
            else
            {
                rearLeftWheel.motorTorque = 0f;
                rearRightWheel.motorTorque = 0f;
            }
        }
        else
        {
            rearLeftWheel.motorTorque = 0f;
            rearRightWheel.motorTorque = 0f;
        }
    }

    /// <summary>
    /// Frenado real: usa brakeTorque en las 4 ruedas cuando va hacia adelante.
    /// Cuando ya está casi parado, permite ir en reversa suavemente.
    /// ANTES: aplicaba torque negativo al motor → desestabilizaba el trasero.
    /// </summary>
    private void Braking()
    {
        if (brakeInput > 0f)
        {
            if (currentSpeed > 1f)
            {
                // Frenando hacia adelante: brakeTorque en las 4 ruedas
                frontLeftWheel.brakeTorque  = brakeTorque * brakeInput;
                frontRightWheel.brakeTorque = brakeTorque * brakeInput;
                rearLeftWheel.brakeTorque   = brakeTorque * brakeInput;
                rearRightWheel.brakeTorque  = brakeTorque * brakeInput;
            }
            else
            {
                // Ya parado o yendo hacia atrás: reversa suave
                ClearBrakeTorques();
                float reverseForce = motorTorque * 0.4f;
                rearLeftWheel.motorTorque  = -brakeInput * reverseForce;
                rearRightWheel.motorTorque = -brakeInput * reverseForce;
            }
        }
        else
        {
            ClearBrakeTorques();
        }
    }

    /// <summary>
    /// Freno de motor: al soltar el acelerador el auto desacelera naturalmente.
    /// Sin esto los WheelColliders no tienen fricción de transmisión → el auto flota.
    /// </summary>
    private void EngineBraking()
    {
        if (accelInput == 0f && brakeInput == 0f && Mathf.Abs(currentSpeed) > 0.5f)
        {
            rearLeftWheel.brakeTorque  = engineBrakeTorque;
            rearRightWheel.brakeTorque = engineBrakeTorque;
        }
    }

    private void ClearBrakeTorques()
    {
        frontLeftWheel.brakeTorque  = 0f;
        frontRightWheel.brakeTorque = 0f;
        rearLeftWheel.brakeTorque   = 0f;
        rearRightWheel.brakeTorque  = 0f;
    }

    private void UpdateSingleWheel(WheelCollider col, Transform mesh, int index)
    {
        if (mesh == null) return;
        col.GetWorldPose(out Vector3 worldPos, out Quaternion worldRot);
        Transform parent = mesh.parent;
        if (parent != null)
        {
            mesh.localPosition = parent.InverseTransformPoint(worldPos);
            Quaternion parentRot = Quaternion.Inverse(parent.rotation);
            Quaternion deltaRot = worldRot * Quaternion.Inverse(col.transform.rotation);
            mesh.localRotation = meshInitialRotations[index] * (parentRot * deltaRot * parent.rotation);
        }
        else
        {
            mesh.SetPositionAndRotation(worldPos, worldRot);
        }
    }

    private void Steering()
    {
        // Sin doble-lerp: steerInput ya viene suavizado desde SmoothSteerInput()
        float currentMaxAngle = Mathf.Lerp(maxSteerAngle, minSteerAngleAtHighSpeed, speedFactor);
        currentSteerAngle = steerInput * currentMaxAngle;

        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    /// <summary>
    /// Turn assist suave: complementa la física de los WheelColliders sin dominarla.
    /// Con turnAssistTorque bajo (10) y maxAngularVelocity limitado no puede causar spin.
    /// El damping angular corre SIEMPRE para frenar la rotación residual.
    /// </summary>
    private void TurnAssist()
    {
        // --- Damping angular: corre SIEMPRE, independiente de la velocidad ---
        bool isSteering = Mathf.Abs(steerInput) > 0.05f;
        float damp = isSteering ? angularDampingSteer : angularDampingIdle;
        Vector3 av = rb.angularVelocity;
        av.y = Mathf.Lerp(av.y, 0f, damp * Time.fixedDeltaTime);
        rb.angularVelocity = av;

        // --- Torque assist: solo si hay suficiente velocidad ---
        float speedKmh = Mathf.Abs(currentSpeed);
        if (speedKmh < minSpeedForAssist) return;

        // Curva sin() natural: 0 en vel baja, pico en vel media, baja en vel alta
        float normalizedSpeed = Mathf.Clamp01(speedKmh / maxForwardSpeed);
        float assistMult = Mathf.Sin(normalizedSpeed * Mathf.PI);
        float dirSign = currentSpeed < 0f ? -1f : 1f;

        rb.AddTorque(Vector3.up * steerInput * turnAssistTorque * assistMult * dirSign,
                     ForceMode.Acceleration);
    }

    /// <summary>
    /// Amortigua la velocidad lateral del auto en cada FixedUpdate.
    /// Es lo que evita que el auto se vaya de costado como en hielo.
    /// lateralDamping = 0.92 → pierde ~8% de vel lateral por tick de física.
    /// </summary>
    private void LateralDamping()
    {
        if (InAir()) return;
        Vector3 localVel = rb.transform.InverseTransformDirection(rb.linearVelocity);
        localVel.x *= lateralDamping;
        rb.linearVelocity = rb.transform.TransformDirection(localVel);
    }

    private void SyncWheels()
    {
        UpdateSingleWheel(frontLeftWheel, frontLeftMesh, 0);
        UpdateSingleWheel(frontRightWheel, frontRightMesh, 1);
        UpdateSingleWheel(rearLeftWheel, rearLeftMesh, 2);
        UpdateSingleWheel(rearRightWheel, rearRightMesh, 3);
    }

    public bool InAir()
    {
        if (!frontLeftWheel.GetGroundHit(out _)) return true;
        if (!frontRightWheel.GetGroundHit(out _)) return true;
        if (!rearLeftWheel.GetGroundHit(out _)) return true;
        if (!rearRightWheel.GetGroundHit(out _)) return true;
        return false;
    }

    public void ApplyBoost(float boostForce)
    {
        rb.AddForce(rb.transform.forward * boostForce, ForceMode.VelocityChange);
    }
}