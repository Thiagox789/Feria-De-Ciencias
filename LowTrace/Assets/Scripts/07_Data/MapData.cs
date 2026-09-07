using UnityEngine;

public enum TamanioPista
{
    S,
    M,
    L,
    XL
}

public enum DificultadPista
{
    Facil,
    Medio,
    Dificil
}

[CreateAssetMenu(fileName = "NuevoMapa", menuName = "LowTrace/MapData")]
public class MapData : ScriptableObject
{
    [Header("Información General")]
    public string nombre;
    public string escena;
    public Sprite miniatura;

    [Header("Configuración de Pista")]
    public TamanioPista tipoPista = TamanioPista.M;
    public DificultadPista dificultad = DificultadPista.Medio;
    public float tiempoEstimado = 60f; // En segundos

    [Header("Cielos Disponibles")]
    [Tooltip("Lista de skyboxes que se pueden elegir para este mapa")]
    public Material[] skyboxes;
    [Tooltip("Nombres para mostrar en la UI (debe coincidir con el array de skyboxes)")]
    public string[] nombresSkyboxes;
    [Tooltip("Índice del cielo por defecto (0 = primer elemento)")]
    public int skyboxDefault = 0;
}
