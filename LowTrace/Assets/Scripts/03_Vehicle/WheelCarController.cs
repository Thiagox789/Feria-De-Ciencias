using UnityEngine;

/// <summary>
/// Controlador de auto con WheelColliders.
/// Aceleracin, freno y direccin con fsica realista.
/// Sin marchas, sin luces, sin sonido.
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

    [Header("Acceleration")]
    [SerializeField] private float motorTorque = 1500f;
    [SerializeField] private float maxForwardSpeed = 120f;
    [SerializeField] private float maxReverseSpeed = 40f;

    [Header("Braking")]
    [SerializeField] private float brakeTorque = 3000f;
    [SerializeField] private float naturalDeceleration = 8f;

    [Header("Steering")]
    [SerializeField] private float maxSteerAngle = 35f;
    [SerializeField] private float steerSpeed = 8f;

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
    }

    private void Update()
    {
        GetInput();

        if (!carreraIniciada && (accelInput != 0 || Mathf.Abs(steerInput) > 0.05f))
        {
            carreraIniciada = true;
            if (GameManager.Instancia != null)
                GameManager.Instancia.IniciarCarrera();
        }
    }

    private void FixedUpdate()
    {
        CalculateSpeed();
        Acceleration();
        Braking();
        Steering();
        Deceleration();
        SyncWheels();
    }

    private void GetInput()
    {
        accelInput = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) accelInput = 1f;

        brakeInput = 0;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) brakeInput = 1f;

        float rawSteer = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) rawSteer = 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) rawSteer = -1f;
        steerInput = Mathf.Lerp(steerInput, rawSteer, steerSpeed * Time.fixedDeltaTime);
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

    private void Braking()
    {
        if (brakeInput > 0f)
        {
            frontLeftWheel.brakeTorque = brakeInput * brakeTorque;
            frontRightWheel.brakeTorque = brakeInput * brakeTorque;
            rearLeftWheel.brakeTorque = brakeInput * brakeTorque;
            rearRightWheel.brakeTorque = brakeInput * brakeTorque;
        }
        else
        {
            frontLeftWheel.brakeTorque = 0f;
            frontRightWheel.brakeTorque = 0f;
            rearLeftWheel.brakeTorque = 0f;
            rearRightWheel.brakeTorque = 0f;
        }
    }

    private void Steering()
    {
        float speedReduction = 1f - Mathf.Clamp01(speedFactor);
        float targetAngle = steerInput * maxSteerAngle * speedReduction;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, steerSpeed * Time.fixedDeltaTime);

        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    private void Deceleration()
    {
        if (accelInput == 0f && brakeInput == 0f)
        {
            if (Mathf.Abs(currentSpeed) > 0.5f)
            {
                Vector3 decelForce = -rb.transform.forward * Mathf.Sign(currentSpeed) * naturalDeceleration;
                rb.AddForce(decelForce, ForceMode.Acceleration);
            }
        }
    }

    private void SyncWheels()
    {
        UpdateSingleWheel(frontLeftWheel, frontLeftMesh);
        UpdateSingleWheel(frontRightWheel, frontRightMesh);
        UpdateSingleWheel(rearLeftWheel, rearLeftMesh);
        UpdateSingleWheel(rearRightWheel, rearRightMesh);
    }

    private void UpdateSingleWheel(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.SetPositionAndRotation(pos, rot);
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