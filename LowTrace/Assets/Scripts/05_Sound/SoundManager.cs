using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instancia { get; private set; }

    [Header("Música")]
    [SerializeField] private AudioSource musicaSource;
    [SerializeField] private AudioClip[] musicaClips;
    [SerializeField] private float volumenMusica = 0.5f;

    [Header("Efectos de Sonido")]
    [SerializeField] private AudioSource[] sfxSources;
    [SerializeField] private float volumenSFX = 0.7f;

    private int musicaActual = -1;

    public float VolumenMusica
    {
        get => volumenMusica;
        set
        {
            volumenMusica = Mathf.Clamp01(value);
            if (musicaSource != null)
                musicaSource.volume = volumenMusica;
        }
    }

    public float VolumenSFX
    {
        get => volumenSFX;
        set => volumenSFX = Mathf.Clamp01(value);
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        if (musicaSource != null)
            musicaSource.volume = volumenMusica;
    }

    public void PlayMusic(int indice)
    {
        if (musicaClips == null || indice < 0 || indice >= musicaClips.Length) return;
        if (musicaActual == indice && musicaSource.isPlaying) return;

        musicaActual = indice;
        musicaSource.clip = musicaClips[indice];
        musicaSource.volume = volumenMusica;
        musicaSource.loop = true;
        musicaSource.Play();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicaSource.clip = clip;
        musicaSource.volume = volumenMusica;
        musicaSource.loop = true;
        musicaSource.Play();
    }

    public void StopMusic()
    {
        musicaSource.Stop();
        musicaActual = -1;
    }

    public void PauseMusic()
    {
        musicaSource.Pause();
    }

    public void ResumeMusic()
    {
        musicaSource.UnPause();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        AudioSource fuente = ObtenerFuenteSFXDisponible();
        if (fuente != null)
        {
            fuente.clip = clip;
            fuente.volume = volumenSFX;
            fuente.Play();
        }
    }

    public void PlaySFX(AudioClip clip, float volumen)
    {
        if (clip == null) return;
        AudioSource fuente = ObtenerFuenteSFXDisponible();
        if (fuente != null)
        {
            fuente.clip = clip;
            fuente.volume = volumen * volumenSFX;
            fuente.Play();
        }
    }

    public void StopAllSFX()
    {
        foreach (var fuente in sfxSources)
        {
            if (fuente != null && fuente.isPlaying)
                fuente.Stop();
        }
    }

    private AudioSource ObtenerFuenteSFXDisponible()
    {
        foreach (var fuente in sfxSources)
        {
            if (fuente != null && !fuente.isPlaying)
                return fuente;
        }
        return sfxSources.Length > 0 ? sfxSources[0] : null;
    }
}