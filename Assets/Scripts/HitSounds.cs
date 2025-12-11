using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

public class HitSounds : MonoBehaviour
{


    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clipHappy;
    [SerializeField] private AudioClip clipAngry;
    [SerializeField] private AudioClip clipAngrier;
    [SerializeField] private AudioClip clipAngriest;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

    }
    public void PlayHitAudio(float happyLevel)
    {
        if (audioSource.isPlaying)
            return;

        switch (happyLevel)
        {
            case float n when n == 0f:
                audioSource.clip = clipAngriest;
                break;
            case float n when n < 40f:
                audioSource.clip = clipAngrier;
                break;
            case float n when n < 60f:
                audioSource.clip = clipAngry;
                break;
            case float n when n >= 60f:
                audioSource.clip = clipHappy;
                break;
        }

        audioSource.Play();
    }

}
