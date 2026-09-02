using UnityEngine;

[CreateAssetMenu(fileName = "NuevosAjustes", menuName = "Datos/Ajustes")]
public class SettingsData : ScriptableObject
{
    [Header("Volumen del Juego")]
    // [Range] hace que en Unity aparezca una barrita para elegir entre 0.0 (muteado) y 1.0 (máximo)
    [Range(0f, 1f)] public float volumenMusica = 1f;
    [Range(0f, 1f)] public float volumenSFX = 1f;
}
