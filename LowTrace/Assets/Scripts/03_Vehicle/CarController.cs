using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        Vector3 dir = Vector3.zero;
        if (kb.wKey.isPressed) dir += Vector3.forward;
        if (kb.sKey.isPressed) dir += Vector3.back;
        if (kb.aKey.isPressed) dir += Vector3.left;
        if (kb.dKey.isPressed) dir += Vector3.right;

        if (dir != Vector3.zero && GameManager.Instancia.Estado == GameManager.EstadoJuego.Espera)
        {
            GameManager.Instancia.IniciarCarrera();
        }

        transform.position += dir.normalized * velocidad * Time.deltaTime;
    }
}