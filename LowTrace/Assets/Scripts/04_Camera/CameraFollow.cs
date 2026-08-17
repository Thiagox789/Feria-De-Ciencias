using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -6f);
    [SerializeField] private float suavizado = 5f;

    private void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 destino = objetivo.position + offset;
        transform.position = Vector3.Lerp(transform.position, destino, suavizado * Time.deltaTime);
        transform.LookAt(objetivo);
    }
}