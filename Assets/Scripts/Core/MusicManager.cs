using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField]
    private AudioClip backgroundMusic;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            audioSource.Play();
        }
    }

    public void FadeOut(float duration = 0.5f)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    public void FadeIn(float duration = 0.5f)
    {
        StartCoroutine(FadeInCoroutine(duration));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = audioSource.volume;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);

            yield return null;
        }

        audioSource.volume = 0f;

        audioSource.Pause();
    }

    private IEnumerator FadeInCoroutine(float duration)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.UnPause();
        }

        float targetVolume = 0.5f;

        audioSource.volume = 0f;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);

            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}
