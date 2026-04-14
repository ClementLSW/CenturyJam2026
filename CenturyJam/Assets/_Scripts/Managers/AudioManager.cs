using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 10;
    private List<AudioSource> sfxSources = new List<AudioSource>();

    [Header("Clips")]
    public AudioClip bgm;

    public AudioClip boxPickup;
    public AudioClip boxRotate;
    public AudioClip boxDrop;
    public AudioClip boxInvalid;

    public AudioClip conveyor;

    public AudioClip vanArrive;
    public AudioClip vanLeave;

    public AudioClip buttonSelect;
    public AudioClip ticking;
    public AudioClip timeOut;
    public AudioClip scoreIncrease;
    public AudioClip payout;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxSources.Add(source);
        }
    }

    void Start()
    {
        PlayMusic(bgm);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, bool randomPitch = true)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSource();

        source.clip = clip;
        source.volume = volume;

        source.pitch = randomPitch ? UnityEngine.Random.Range(0.95f, 1.05f) : 1f;

        source.Play();
    }

    public void PlaySFXDelayed(AudioClip clip, float delay, float volume = 1f)
    {
        StartCoroutine(PlaySFXDelayedRoutine(clip, delay, volume));
    }

    private IEnumerator PlaySFXDelayedRoutine(AudioClip clip, float delay, float volume)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(clip, volume);
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
                return source;
        }

        // fallback (reuse first)
        return sfxSources[0];
    }
}