using UnityEngine;
using UnityEngine.UI;

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

    public void PlayWin()
    {
        PlaySFX(winSound);
    }

    public void PlayLose()
    {
        PlaySFX(loseSound);
    }
}
