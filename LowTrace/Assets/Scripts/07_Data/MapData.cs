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
    [TextArea(2, 5)]
    public string descripcion;

    [Header("Configuración de Pista")]
    public TamanioPista tipoPista = TamanioPista.M;
    public DificultadPista dificultad = DificultadPista.Medio;
    public float tiempoEstimado = 60f; // En segundos
}
