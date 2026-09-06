using UnityEngine;

/// <summary>
/// Cámara de seguimiento en tercera persona para juegos de carreras arcade.
/// Suaviza posición y rotación, y aumenta el FOV según la velocidad del auto
/// para reforzar la sensación de velocidad.
/// </summary>
public class ArcadeCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform del auto a seguir.")]
    [SerializeField] private Transform target;
    [Tooltip("Referencia al controlador del auto para leer su velocidad (evita GetComponent en Update).")]
    [SerializeField] private WheelCarController targetCarController;

    [Header("Posicionamiento")]
    [Tooltip("Offset local respecto al auto (detrás y arriba).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 3.5f, -7f);
    [SerializeField] private float positionSmoothTime = 0.15f;

    [Header("Rotación")]
    [SerializeField] private float rotationSmoothSpeed = 8f;
    [Tooltip("Punto por encima del auto al que mira la cámara (para no mirar al piso).")]
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);

    [Header("FOV dinámico")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float maxFOV = 75f;
    [SerializeField] private float fovSmoothSpeed = 4f;

    private Vector3 velocityRef = Vector3.zero;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }
    }

    private void OnEnable()
    {
        if (target != null)
        {
            transform.position = target.TransformPoint(offset);
            transform.LookAt(target.position + lookAtOffset);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        FollowPosition();
        FollowRotation();
        UpdateFOV();
    }

    private void FollowPosition()
    {
        Vector3 desiredPosition = target.TransformPoint(offset);
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocityRef,
            positionSmoothTime
        );
    }

    private void FollowRotation()
    {
        Vector3 lookTarget = target.position + lookAtOffset;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }

    private void UpdateFOV()
    {
        if (targetCamera == null || targetCarController == null) return;

        float speedRatio = targetCarController.SpeedRatio01;
        float desiredFOV = Mathf.Lerp(baseFOV, maxFOV, speedRatio);

        targetCamera.fieldOfView = Mathf.Lerp(
            targetCamera.fieldOfView,
            desiredFOV,
            fovSmoothSpeed * Time.deltaTime
        );
    }
}
