using UnityEngine;
using UnityEngine.UI; // Necesario para usar Sliders e interactuar con la Interfaz (UI)

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Sliders (Deslizadores) de UI")]
    // Aquí debes arrastrar los Sliders que creaste en el Canvas de Unity
    public Slider sliderMusica;
    public Slider sliderSFX;

    private void Start()
    {
        // 1. Al abrir el menú de ajustes, acomodamos las barritas de los sliders en su posición correcta 
        // leyendo los valores que están guardados en el DataManager (los que vinieron del JSON)
        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            if (sliderMusica != null) sliderMusica.value = DataManager.Instancia.ajustes.volumenMusica;
            if (sliderSFX != null) sliderSFX.value = DataManager.Instancia.ajustes.volumenSFX;
        }

        // 2. Le decimos a los sliders que, cada vez que el jugador los mueva, 
        // ejecuten las funciones de abajo automáticamente.
        if (sliderMusica != null) sliderMusica.onValueChanged.AddListener(AlMoverSliderMusica);
        if (sliderSFX != null) sliderSFX.onValueChanged.AddListener(AlMoverSliderSFX);
    }

    // Esta función se ejecuta sola cada vez que mueves la barra de Música
    private void AlMoverSliderMusica(float nuevoVolumen)
    {
        // A) Cambiamos el volumen real en el SoundManager para que escuches el cambio al instante
        if (SoundManager.Instancia != null)
        {
            SoundManager.Instancia.SetVolumenMusica(nuevoVolumen);
        }

        // B) Modificamos el valor en los datos y lo Guardamos en el JSON en el disco duro
        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            DataManager.Instancia.ajustes.volumenMusica = nuevoVolumen;
            DataManager.Instancia.GuardarAjustes();
        }
    }

    // Esta función se ejecuta sola cada vez que mueves la barra de Efectos Especiales (SFX)
    private void AlMoverSliderSFX(float nuevoVolumen)
    {
        if (SoundManager.Instancia != null)
        {
            SoundManager.Instancia.SetVolumenSFX(nuevoVolumen);
        }

        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            DataManager.Instancia.ajustes.volumenSFX = nuevoVolumen;
            DataManager.Instancia.GuardarAjustes();
        }
    }
}
