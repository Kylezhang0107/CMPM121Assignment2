using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("SFX Source")]
    public AudioSource sfxSource;

    [Header("Sound Effects")]

    public AudioClip spellFire;
    public AudioClip spellHit;
    public AudioClip enemyKilled;
    public AudioClip buttonClick;
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("Player")]
    public AudioClip playerHurt;
    public AudioClip footsteps;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        if (sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySpellFire()
    {
        PlaySFX(spellFire);
    }

    public void PlaySpellHit()
    {
        PlaySFX(spellHit);
    }

    public void PlayEnemyKilled()
    {
        PlaySFX(enemyKilled);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayPlayerHurt()
    {
        PlaySFX(playerHurt);
    }

    public void PlayWin()
    {
        StartCoroutine(PlayPrioritySound(winSound));
    }

    public void PlayLose()
    {
        StartCoroutine(PlayPrioritySound(loseSound));
    }

    private IEnumerator PlayPrioritySound(AudioClip clip)
    {
        if (clip == null)
        {
            yield break;
        }

        MusicManager.Instance.FadeOut(0.5f);

        yield return new WaitForSeconds(0.5f);
        
        sfxSource.PlayOneShot(clip);

        yield return new WaitForSeconds(clip.length);

        MusicManager.Instance.FadeIn(0.5f);
    }
}
