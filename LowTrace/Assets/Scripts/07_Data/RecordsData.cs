using UnityEngine;

// Esto permite que podamos crear este "archivo de datos" desde el menú de Unity
// (Click derecho -> Create -> Datos -> Records)
[CreateAssetMenu(fileName = "NuevosRecords", menuName = "Datos/Records")]
public class RecordsData : ScriptableObject
{
    [Header("Récord de la pista")]
    // Aquí guardaremos el mejor tiempo en segundos. 
    // Le ponemos un número muy alto por defecto (9999) para que sea muy fácil 
    // de superar en la primera carrera.
    public float mejorTiempo = 9999f;
}
