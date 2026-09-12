using UnityEngine;
using UnityEngine.SceneManagement;

// Este script es el Gestor de Sonido (SoundManager).
// Controla la música de fondo de las pantallas y los efectos de sonido (clic de botones y motor del auto).
public class SoundManager : MonoBehaviour
{
    // Singleton para acceder a SoundManager.Instancia desde cualquier script
    public static SoundManager Instancia;

    [Header("Reproductores de Audio")]
    [SerializeField] private AudioSource reproductorMusica;
    [SerializeField] private AudioSource reproductorEfectosSFX;
    [SerializeField] private AudioSource reproductorMotorSFX;

    [Header("Canciones de Fondo")]
    [SerializeField] private AudioClip[] canciones;

    [Header("Sonidos de la Interfaz")]
    [SerializeField] private AudioClip sonidoBoton;

    [Header("Sonido del Motor")]
    [SerializeField] private AudioClip sonidoMotor;
    [SerializeField] private float tonoMotorMinimo = 0.8f;
    [SerializeField] private float tonoMotorMaximo = 2.0f;

    private WheelCarController autoEnEscena;

    private void Awake()
    {
        if (Instancia != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        if (reproductorMusica != null)
        {
            reproductorMusica.playOnAwake = false;
            reproductorMusica.loop = true;
        }

        if (reproductorEfectosSFX != null)
        {
            reproductorEfectosSFX.playOnAwake = false;
        }
        
        if (reproductorMotorSFX != null) 
        {
            reproductorMotorSFX.playOnAwake = false;
            reproductorMotorSFX.loop = true; 
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    // Cambia automáticamente la canción de fondo al cambiar de pantalla
    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        autoEnEscena = null;

        if (reproductorMotorSFX != null && reproductorMotorSFX.isPlaying)
        {
            reproductorMotorSFX.Stop();
        }

        int indiceCancion = 0; // Canción del Menú por defecto

        if (escena.name == "IA" || escena.name == "Game" || escena.name == "Mapa" || escena.name.StartsWith("Circuito")) 
        {
            indiceCancion = 1; // Canción de la Carrera
        }
        else if (escena.name == "Creditos")
        {
            indiceCancion = 2; // Canción de Créditos
        }

        PlayMusic(indiceCancion);
    }

    private void Start()
    {
        // Carga los volúmenes guardados en los datos del juego
        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            SetVolumenMusica(DataManager.Instancia.ajustes.volumenMusica);
            SetVolumenSFX(DataManager.Instancia.ajustes.volumenSFX);
        }
    }

    public void SetVolumenMusica(float volumen)
    {
        if (reproductorMusica != null)
        {
            reproductorMusica.volume = volumen;
        }
    }

    public void SetVolumenSFX(float volumen)
    {
        if (reproductorEfectosSFX != null) reproductorEfectosSFX.volume = volumen;
        if (reproductorMotorSFX != null) reproductorMotorSFX.volume = volumen;
    }

    public void PlayMusic(int indice)
    {
        if (reproductorMusica != null && canciones != null && indice < canciones.Length && canciones[indice] != null)
        {
            reproductorMusica.loop = true;
            // Solo cambia la canción si es distinta para no reiniciarla desde cero
            if (reproductorMusica.clip != canciones[indice])
            {
                reproductorMusica.clip = canciones[indice]; 
                reproductorMusica.Play();                
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (reproductorEfectosSFX != null && clip != null)
        {
            reproductorEfectosSFX.PlayOneShot(clip);
        }
    }

    public void PlaySFXBoton()
    {
        if (reproductorEfectosSFX != null && sonidoBoton != null)
        {
            reproductorEfectosSFX.PlayOneShot(sonidoBoton);
        }
    }

    private void Update()
    {
        if (autoEnEscena == null)
        {
            autoEnEscena = FindObjectOfType<WheelCarController>();
        }

        if (autoEnEscena == null || reproductorMotorSFX == null) return;

        bool carreraActiva = GameManager.Instancia != null &&
                             GameManager.Instancia.Estado == GameManager.EstadoJuego.Carrera;

        if (carreraActiva && sonidoMotor != null)
        {
            if (!reproductorMotorSFX.isPlaying)
            {
                reproductorMotorSFX.clip = sonidoMotor;
                reproductorMotorSFX.loop = true;
                reproductorMotorSFX.Play();
            }

            // Cambia el tono del motor según la velocidad del auto (más agudo a mayor velocidad)
            float tono = Mathf.Lerp(tonoMotorMinimo, tonoMotorMaximo, autoEnEscena.SpeedRatio01);
            reproductorMotorSFX.pitch = tono;
        }
        else if (!carreraActiva && reproductorMotorSFX.isPlaying)
        {
            reproductorMotorSFX.Stop();
        }
    }
}
