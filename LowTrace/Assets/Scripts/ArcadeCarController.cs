using UnityEngine;
using UnityEngine.InputSystem;

public class ArcadeCarController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody carRB;
    public Transform[] rayPoints;
    public Transform accelerationPoint;

    [Header("Suspension")]
    public float springStiffness = 30000f;
    public float damperStiffness = 3000f;
    public float restLength = 1.0f;
    public float springTravel = 0.5f;
    public float wheelRadius = 0.33f;

    [Header("Movement")]
    public float acceleration = 25f;
    public float deceleration = 10f;
    public float maxSpeed = 100f;
    public float steerStrength = 30f;
    public AnimationCurve turnCurve;

    [Header("Drag")]
    public float dragCoefficient = 1.0f;

    [Header("Ground")]
    public LayerMask drivableMask;
    public bool isGrounded;

    private float currentSpeed;

    private void Start()
    {
        if (carRB == null)
            carRB = GetComponent<Rigidbody>();

        if (turnCurve == null || turnCurve.length == 0)
        {
            turnCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.5f, 0.8f),
                new Keyframe(1f, 0.4f)
            );
        }
    }

    private void FixedUpdate()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        float vertical = 0f;
        float horizontal = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed) vertical -= 1f;
            if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed) horizontal += 1f;

            if (Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            if (Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
        }

        // --- Ground check ---
        isGrounded = false;
        foreach (Transform point in rayPoints)
        {
            if (point == null) continue;

            float maxLength = restLength + springTravel + wheelRadius;
            RaycastHit hit;

            if (Physics.Raycast(point.position, -point.up, out hit, maxLength, drivableMask))
            {
                isGrounded = true;

                // --- Suspension force (Hooke + Damping) ---
                float currentLength = hit.distance - wheelRadius;
                float compression = Mathf.Clamp01((restLength - currentLength) / springTravel);
                float springForce = compression * springStiffness;

                float velocity = Vector3.Dot(carRB.GetPointVelocity(point.position), -point.up);
                float damperForce = velocity * damperStiffness;

                float totalForce = springForce - damperForce;
                carRB.AddForceAtPosition(point.up * totalForce, point.position);
            }
        }

        if (!isGrounded) return;

        // --- Acceleration / Deceleration ---
        currentSpeed = carRB.linearVelocity.magnitude;

        if (Mathf.Abs(vertical) > 0.01f)
        {
            Vector3 forwardForce = transform.forward * (vertical * acceleration * carRB.mass);
            carRB.AddForceAtPosition(forwardForce, accelerationPoint != null ? accelerationPoint.position : transform.position);
        }
        else
        {
            Vector3 vel = carRB.linearVelocity;
            Vector3 horizontalVel = new Vector3(vel.x, 0f, vel.z);
            if (horizontalVel.magnitude > 0.1f)
            {
                Vector3 brakeForce = -horizontalVel.normalized * (deceleration * carRB.mass);
                carRB.AddForce(brakeForce);
            }
        }

        // --- Clamp max speed (horizontal only) ---
        Vector3 hVel = new Vector3(carRB.linearVelocity.x, 0f, carRB.linearVelocity.z);
        if (hVel.magnitude > maxSpeed)
        {
            Vector3 clamped = hVel.normalized * maxSpeed;
            carRB.linearVelocity = new Vector3(clamped.x, carRB.linearVelocity.y, clamped.z);
        }

        // --- Steering ---
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            float speedFactor = hVel.magnitude / maxSpeed;
            float turnAmount = turnCurve.Evaluate(speedFactor) * horizontal * steerStrength;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount * Time.fixedDeltaTime, 0f);
            carRB.MoveRotation(carRB.rotation * turnRotation);
        }

        // --- Sideways drag (anti-slip) ---
        Vector3 right = transform.right;
        float sidewaysSpeed = Vector3.Dot(carRB.linearVelocity, right);
        Vector3 sidewaysDrag = -right * (sidewaysSpeed * dragCoefficient);
        carRB.AddForce(sidewaysDrag, ForceMode.Acceleration);
    }
}
