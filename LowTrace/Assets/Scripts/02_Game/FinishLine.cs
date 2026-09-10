using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[FinishLine] OnTriggerEnter: {other.name}, tag={other.tag}");
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.CruzarMeta();
        }
    }
}
