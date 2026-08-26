using UnityEngine;

[CreateAssetMenu(fileName = "SettingsData", menuName = "LowTrace/Settings Data")]
public class SettingsData : ScriptableObject
{
    [Header("Audio")]
    [Range(0f, 1f)] public float volumenMusica = 0.5f;
    [Range(0f, 1f)] public float volumenSFX = 0.7f;

    [Header("Gráficos")]
    public int calidadGrafica = 2;
    public bool pantallaCompleta = false;

    [Header("Controles")]
    [Range(0.1f, 3f)] public float sensibilidadVolante = 1f;
    public bool invertirEjeY = false;

    [Header("General")]
    public string nombreJugador = "Jugador";
    public string idioma = "es";

    public void CopiarDesde(SettingsData otro)
    {
        volumenMusica = otro.volumenMusica;
        volumenSFX = otro.volumenSFX;
        calidadGrafica = otro.calidadGrafica;
        pantallaCompleta = otro.pantallaCompleta;
        sensibilidadVolante = otro.sensibilidadVolante;
        invertirEjeY = otro.invertirEjeY;
        nombreJugador = otro.nombreJugador;
        idioma = otro.idioma;
    }
}