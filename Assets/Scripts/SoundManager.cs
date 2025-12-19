using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] AudioSource[] musicSource;

    [SerializeField] AudioSource[] winSound;

    [SerializeField] AudioSource[] loseSound;

    [SerializeField] AudioSource[] bonusSound;

    [SerializeField] AudioSource clearPieceSound;
    [SerializeField] AudioSource starSound;

    [SerializeField] float musicMinDb = -80f;
    float currentMusicMaxDb = -20f; // sẽ auto cập nhật theo clip


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        PlayRandomMusic();
    }

    public void PlayRandomSound(AudioSource[] audioSource)
    {
        if(audioSource.Length == 0) return;
        int index = Random.Range(0, audioSource.Length);
        audioSource[index].Play();
    }

    public void PlayRandomMusic()
    {
        PlayRandomSound(musicSource);
    }


    public void PlayWinSound()
    {
        PlayRandomSound(winSound);
    }

    public void PlayLoseSound()
    {
        PlayRandomSound(loseSound);
    }

    public void PlayBonusSound()
    {
        PlayRandomSound(bonusSound);
    }

    public void PlayClearPieceSound()
    {
        clearPieceSound.Play();
    }
    public void PlayStarSound()
    {
        starSound.Play();
    }

}
