using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip bgm;

    public AudioClip boxPickup;
    public AudioClip boxRotate;
    public AudioClip boxDrop;
    public AudioClip boxInvalid;

    public AudioClip conveyor;

    public AudioClip truckArrive;
    public AudioClip truckLeave;

    public AudioClip buttonSelect;
    public AudioClip ticking;
    public AudioClip timeOut;
    public AudioClip scoreIncrease;
    public AudioClip payout;


    void Awake()
    {
        // Singleton pattern
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

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }

    public void PlaySFXDelayed(AudioClip clip, float delay)
    {
        StartCoroutine(PlaySFXDelayedRoutine(clip, delay));
    }

    private IEnumerator PlaySFXDelayedRoutine(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);

        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }
}