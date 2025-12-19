using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStar : MonoBehaviour
{
    [SerializeField] Image star;
    [SerializeField] ParticlePlayer starFX;
    [SerializeField] float delay = 0.5f;
    [SerializeField] bool activated = false;


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

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayStarSound();
        }

        yield return new WaitForSeconds(delay);

        SetActive(true);
    }
}
