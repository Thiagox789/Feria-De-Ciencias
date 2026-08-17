using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [Tooltip("Offset relativo al auto (local). Z: atras/adelante, Y: altura, X: lateral")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -6f);
    [SerializeField] private float suavizadoPos = 5f;
    [SerializeField] private float suavizadoRot = 5f;
    [Tooltip("FOV base y maximo segun velocidad (sensacion de velocidad tipo arcade)")]
    [SerializeField] private float fovBase = 60f;
    [SerializeField] private float fovMax = 100f;
    [Header("Mirada en curvas")]
    [Tooltip("Que tan adelante del auto mira la camara")]
    [SerializeField] private float adelantoPunto = 4f;
    [Tooltip("Desplazamiento lateral maximo del punto de mira al girar (deja ver hacia la curva)")]
    [SerializeField] private float adelantoLateral = 2.5f;
    [SerializeField] private float suavizadoGiro = 4f;

    private Camera camara;
    private float giroSuavizado;

    private void Awake()
    {
        camara = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 destino = objetivo.position + objetivo.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, destino, suavizadoPos * Time.deltaTime);

        CarController cc = objetivo.GetComponent<CarController>();
        if (cc != null)
        {
            giroSuavizado = Mathf.Lerp(giroSuavizado, cc.GiroEntrada, suavizadoGiro * Time.deltaTime);
        }
        else
        {
            giroSuavizado = Mathf.Lerp(giroSuavizado, 0f, suavizadoGiro * Time.deltaTime);
        }

        Vector3 puntoMira = objetivo.position
                            + objetivo.forward * adelantoPunto
                            + objetivo.right * (giroSuavizado * adelantoLateral);

        Quaternion rotDeseada = Quaternion.LookRotation(puntoMira - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotDeseada, suavizadoRot * Time.deltaTime);

        if (camara != null && cc != null)
            camara.fieldOfView = Mathf.Lerp(fovBase, fovMax, cc.VelocidadNormalizada);
    }
}