using UnityEngine;
using UnityEngine.SceneManagement; // NUEVO: Necesario para saber en qué escena estamos

// MonoBehaviour permite que este script se pueda "pegar" a un objeto dentro de Unity
public class SoundManager : MonoBehaviour
{
    // Patrón "Singleton"
    public static SoundManager Instancia;

    [Header("Reproductores de Audio")]
    [SerializeField] private AudioSource musica;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioSource motorSFX;

    [Header("Lista de Canciones")]
    [SerializeField] private AudioClip[] musicas;

    private void Awake()
    {
        if (Instancia != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        if (musica != null) musica.playOnAwake = false;
        if (sfx != null) sfx.playOnAwake = false;
        
        if (motorSFX != null) 
        {
            motorSFX.playOnAwake = false;
            motorSFX.loop = true; 
        }
    }

    // ======= ¡NUEVO! CAMBIO AUTOMÁTICO DE CANCIÓN =======
    // Le avisamos a Unity que queremos ejecutar nuestra función AlCargarEscena cada vez que se cambie de pantalla
    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        int indiceCancion = 0; // Por defecto la 0 (para Menú, Ajustes y Ranking)

        // Verificamos qué pantalla acaba de cargar
        if (escena.name == "IA" || escena.name == "Game" || escena.name == "Mapa") 
        {
            indiceCancion = 1; // La canción de jugar
        }
        else if (escena.name == "Creditos")
        {
            indiceCancion = 2; // La canción de créditos
        }

        // Llamamos al método PlayMusic para reproducirla
        PlayMusic(indiceCancion);
    }
    // ===================================================

    // ======= ¡NUEVO! CONTROL DE VOLUMEN =======
    
    // La función Start() se ejecuta justo después de Awake().
    // Aquí el DataManager ya cargó el JSON, así que le pedimos el volumen guardado.
    private void Start()
    {
        if (DataManager.Instancia != null && DataManager.Instancia.ajustes != null)
        {
            SetVolumenMusica(DataManager.Instancia.ajustes.volumenMusica);
            SetVolumenSFX(DataManager.Instancia.ajustes.volumenSFX);
        }
    }

    // Cambia el volumen de la música (recibe un valor de 0 a 1)
    public void SetVolumenMusica(float volumen)
    {
        if (musica != null)
        {
            musica.volume = volumen; // .volume es una propiedad nativa de Unity
        }
    }

    // Cambia el volumen de los efectos (y del motor también)
    public void SetVolumenSFX(float volumen)
    {
        if (sfx != null) sfx.volume = volumen;
        if (motorSFX != null) motorSFX.volume = volumen;
    }
    // =========================================

    public void PlayMusic(int index)
    {
        if (index < musicas.Length && musicas[index] != null)
        {
            // ¡IMPORTANTE! Solo cambiamos la canción si es diferente a la que ya está sonando.
            // Así evitamos que la música se reinicie desde cero al pasar de Menú a Ajustes.
            if (musica.clip != musicas[index])
            {
                musica.clip = musicas[index]; 
                musica.Play();                
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfx != null && clip != null)
        {
            sfx.PlayOneShot(clip);
        }
    }

    public void IniciarMotor(AudioClip clipMotor)
    {
        if (motorSFX != null && clipMotor != null)
        {
            motorSFX.clip = clipMotor;
            motorSFX.Play();
        }
    }

    public void DetenerMotor()
    {
        if (motorSFX != null) motorSFX.Stop();
    }

    public void CambiarTonoMotor(float pitch)
    {
        if (motorSFX != null) motorSFX.pitch = pitch;
    }

    public void StopMusic()
    {
        if (musica != null) musica.Stop();
    }
}
