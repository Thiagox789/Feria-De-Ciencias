using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.RegistrarCheckpoint(this);
        }
    }
}