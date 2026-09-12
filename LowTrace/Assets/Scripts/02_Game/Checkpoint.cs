using UnityEngine;

// Este script se coloca en los Checkpoints (puntos de control) de la pista de carreras.
// Cuando el auto del jugador atraviesa la puerta del checkpoint, cambia de color,
// activa un efecto de partículas y le avisa al GameManager para guardar el progreso de la vuelta.
public class Checkpoint : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer mallaVisual;
    [SerializeField] private Color colorOriginal = Color.white;
    [SerializeField] private Color colorActivado = Color.green;

    [Header("Efecto de Partículas")]
    [SerializeField] private ParticleSystem particulas;

    private Material materialInstancia;
    private bool estaActivado;

    private void Awake()
    {
        // Guardamos el material para cambiarle el color al tocarlo
        if (mallaVisual != null)
        {
            materialInstancia = mallaVisual.material;
            materialInstancia.color = colorOriginal;
        }

        if (particulas != null)
        {
            particulas.Stop();
        }
    }

    // Se activa cuando un objeto físico entra en el área transparente (Trigger) del checkpoint
    private void OnTriggerEnter(Collider otroObjeto)
    {
        // Solo reaccionamos si el objeto que cruzó es el jugador ("Player")
        if (!otroObjeto.CompareTag("Player")) return;
        if (estaActivado) return; // Si ya fue cruzado en esta vuelta, no hace nada repetido

        estaActivado = true;
        Debug.Log($"[Checkpoint] {name} cruzado por el Jugador");

        // Avisamos al gestor del juego que completamos un checkpoint más
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.RegistrarCheckpoint(this);
        }

        // Cambiamos el color de la puerta a verde
        if (materialInstancia != null)
        {
            materialInstancia.color = colorActivado;
        }

        // Encendemos el efecto de partículas
        if (particulas != null)
        {
            particulas.Play();
        }
    }

    // Permite reiniciar el checkpoint para la siguiente vuelta
    public void ReiniciarCheckpoint()
    {
        estaActivado = false;
        if (materialInstancia != null)
        {
            materialInstancia.color = colorOriginal;
        }
    }
}
