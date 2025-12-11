using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    [SerializeField] ParticleSystem[] allParticles;
    [SerializeField] float lifetime = 1f;
    [SerializeField] bool destroyImmediately = true;
    // Start is called before the first frame update
    void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        if (destroyImmediately)
        {
            Destroy(gameObject, lifetime);
        }
    }

    public void Play()
    {
        foreach (ParticleSystem ps in allParticles)
        {
            ps.Stop();
            ps.Play();
        }

        Destroy(gameObject, lifetime);
    }
}
