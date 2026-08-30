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
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float steerStrength = 30f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 1f;

    [Header("Inputs")]
    [SerializeField] private float moveInput;
    [SerializeField] private float steerInput;

    // Estado del coche
    private int[] wheelIsGrounded = new int[4];
    private bool isGrounded;
    private Vector3 currentCarLocalVelocity;
    private float carVelocityRatio;

    public float GiroEntrada => steerInput;
    public float VelocidadNormalizada => carVelocityRatio;

    private void Start()
    {
        if (carRB == null)
        {
            carRB = GetComponent<Rigidbody>();
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
    }

    #region Input Handling
    private void GetPlayerInput()
    {
        moveInput = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveInput = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveInput = -1;

        steerInput = 0;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) steerInput = 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) steerInput = -1;
    }
    #endregion

    #region Car Status Check
    private void GroundCheck()
    {
        int tempGroundedWheels = 0;
        for (int i = 0; i < wheelIsGrounded.Length; i++)
        {
            tempGroundedWheels += wheelIsGrounded[i];
        }

        // Si más de 1 rueda toca el suelo, el auto está grounded
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
        if (isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
        }
    }

    private void Acceleration()
    {
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
        float speedFactor = Mathf.Abs(currentCarLocalVelocity.z) > 0.5f ? Mathf.Sign(currentCarLocalVelocity.z) : 1f;
        float turnAmount = steerStrength * steerInput * curveValue * speedFactor;
        carRB.AddTorque(transform.up * turnAmount, ForceMode.Acceleration);
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;
        float dragForceMagnitude = -currentSidewaysSpeed * dragCoefficient;
        Vector3 dragForce = transform.right * dragForceMagnitude;

        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }
    #endregion

    #region Suspension
    private void Suspension()
    {
        RaycastHit hit;
        float maxLength = restLength + springTravel;

        for (int i = 0; i < rayPoints.Length; i++)
        {
            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, drivable))
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

                // Debug.DrawRay(rayPoints[i].position, -rayPoints[i].up * hit.distance, Color.red);
            }
            else
            {
                wheelIsGrounded[i] = 0;
                // Debug.DrawRay(rayPoints[i].position, -rayPoints[i].up * (maxLength + wheelRadius), Color.green);
            }
        }
    }
    #endregion
}