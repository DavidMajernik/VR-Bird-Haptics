using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

public class HitSounds : MonoBehaviour
{


    [SerializeField] private AudioSource audioSourceBird;
    [SerializeField] private AudioSource audioSourcePlayer;
    [SerializeField] private AudioClip clipHappy;
    [SerializeField] private AudioClip clipAngry;
    [SerializeField] private AudioClip clipAngrier;
    [SerializeField] private AudioClip clipAngriest;
    [SerializeField] private AudioClip clipPunch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        audioSourceBird = GetComponent<AudioSource>();

    }
    public void PlayHitAudio(float happyLevel)
    {
        if (audioSourceBird.isPlaying)
            return;

        switch (happyLevel)
        {
            case float n when n == 0f:
                audioSourceBird.clip = clipAngriest;
                break;
            case float n when n < 40f:
                audioSourceBird.clip = clipAngrier;
                break;
            case float n when n < 60f:
                audioSourceBird.clip = clipAngry;
                break;
            case float n when n >= 60f:
                audioSourceBird.clip = clipHappy;
                break;
        }

        audioSourceBird.Play();
    }

    public void PlayPunchAudio()
    {
        if (audioSourcePlayer.isPlaying)
            return;
        audioSourcePlayer.clip = clipPunch;
        audioSourcePlayer.Play();
    }

}
