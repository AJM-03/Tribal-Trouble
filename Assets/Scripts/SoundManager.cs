using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource musicSource, effectSource;
    public Animator musicAnim;

    public float musicChangeTime;
    public float musicPlayTime;

    public AudioClip[] music;
    private AudioClip newMusic;
    public bool cutsceneWait;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        StartCoroutine(ChangeMusic());
    }



    public void PlaySound(AudioClip clip, float volume)
    {
        effectSource.volume = volume;
        effectSource.PlayOneShot(clip);
    }



    public void StopSound()
    {
        effectSource.Stop();
    }



    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
    }



    IEnumerator ChangeMusic()
    {
        musicAnim.SetTrigger("FadeOut");
        yield return new WaitForSeconds(musicChangeTime);
        musicAnim.SetTrigger("FadeOut");

        do
        {
            newMusic = music[Random.Range(0, music.Length)];
        } while (newMusic == musicSource.clip);

        musicSource.clip = newMusic;
        musicSource.Play();
        yield return new WaitForSeconds(musicPlayTime);

        if (cutsceneWait)
            yield return new WaitForSeconds(musicPlayTime);

        if (musicAnim.GetCurrentAnimatorStateInfo(0).IsName("FadeOut"))
        {
            musicAnim.SetTrigger("FadeOut");
            yield return new WaitForSeconds(2);
        }

        StartCoroutine(ChangeMusic());
    }
}
