using UnityEngine;
using UnityEngine.Audio;

//Script Untuk Mengatur Audio
public class AudioManager : MonoBehaviour
{
    // Fungsi Instance untuk pemanggilan
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip[] musicClips;
    public AudioClip[] sfxClips;

    // Menghilangkan Gameobject
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Memainkan Musik sesuai Index
    public void PlayMusic(int index)
    {
        if (index < 0 || index >= musicClips.Length)
        {
            Debug.LogWarning("Invalid music index!");
            return;
        }

        if (musicSource.clip != musicClips[index] || !musicSource.isPlaying)
        {
            musicSource.clip = musicClips[index];
            musicSource.Play();
        }
    }

    // Memainkan SFX sesuai Index
    public void PlaySFX(int index)
    {
        if (index < 0 || index >= sfxClips.Length)
        {
            Debug.LogWarning("Invalid SFX index!");
            return;
        }

        sfxSource.PlayOneShot(sfxClips[index]);
    }

    // Menghentikan Musik
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Menghentikan SFX
    public void StopSFX()
    {
        sfxSource.Stop();
    }
}
