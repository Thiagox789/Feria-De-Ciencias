using UnityEngine;

[CreateAssetMenu(fileName = "NuevosAjustes", menuName = "Datos/Ajustes")]
public class SettingsData : ScriptableObject
{
    [Header("Volumen del Juego")]
    [Range(0f, 1f)] public float volumenMusica = 1f;
    [Range(0f, 1f)] public float volumenSFX = 1f;

    [Header("Pantalla")]
    public int indiceMonitorPrincipal;
    public int indiceMonitorRanking = 1;
    public int indiceResolucion;
    public bool pantallaCompleta = true;

    [Header("Puerto Serial (Volante)")]
    public string puertoSerial = "";
}
