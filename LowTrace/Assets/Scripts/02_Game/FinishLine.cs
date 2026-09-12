using UnityEngine;

// Este script se coloca en la Línea de Meta del circuito.
// Detecta cuando el auto del jugador cruza la meta para terminar la carrera o contar una vuelta.
public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider otroObjeto)
    {
        // Solo reaccionamos si el objeto que cruza es el auto del jugador
        if (!otroObjeto.CompareTag("Player")) return;

        Debug.Log($"[Meta] El auto cruzó la línea de meta: {otroObjeto.name}");

        // Le avisamos al gestor del juego que cruzamos la meta
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.CruzarMeta();
        }
    }
}
