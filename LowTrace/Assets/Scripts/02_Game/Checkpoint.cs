using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private Color colorOriginal = Color.white;
    [SerializeField] private Color colorActivado = Color.green;

    [Header("Particulas")]
    [SerializeField] private ParticleSystem particulas;

    private Material materialInstancia;
    private bool activado;

    private void Awake()
    {
        if (meshRenderer != null)
        {
            materialInstancia = meshRenderer.material;
            materialInstancia.color = colorOriginal;
        }

        if (particulas != null)
            particulas.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Checkpoint] {name} detectó: {other.name}, tag={other.tag}");

        if (!other.CompareTag("Player")) return;
        if (activado) return;

        activado = true;
        Debug.Log($"[Checkpoint] {name} activado por Player");

        if (GameManager.Instancia != null)
            GameManager.Instancia.RegistrarCheckpoint(this);

        if (materialInstancia != null)
            materialInstancia.color = colorActivado;

        if (particulas != null)
            particulas.Play();
    }
}
