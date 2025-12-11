using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStar : MonoBehaviour
{
    public Image star;
    public ParticlePlayer starFX;
    public float delay = 0.5f;
    public AudioClip starSound;
    public bool activated = false;


    private void Start()
    {
        SetActive(false);
    }
    void SetActive(bool state)
    {
        if (star != null)
        {
            star.gameObject.SetActive(state);
        }
    }
    public void Activate()
    {
        if (activated)
        {
            return;
        }

        StartCoroutine(ActivateRoutine());
    }
    IEnumerator ActivateRoutine()
    {
        activated = true;

        if (starFX != null)
        {
            starFX.Play();
        }

        if (SoundManager.instance != null && starSound != null)
        {
            SoundManager.instance.PlayClipAtPoint(starSound, Vector3.zero, SoundManager.instance.fxVolume);
        }

        yield return new WaitForSeconds(delay);

        SetActive(true);
    }
}
