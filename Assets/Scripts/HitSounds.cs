using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

public class HitSounds : MonoBehaviour
{


    [Header("Bird & Player Audio")]
    [SerializeField] private AudioSource audioSourceBird;
    [SerializeField] private AudioSource audioSourcePlayer;

    [Header("Hit Reaction Clips")]
    [SerializeField] private AudioClip clipHappy;
    [SerializeField] private AudioClip clipAngry;
    [SerializeField] private AudioClip clipAngrier;
    [SerializeField] private AudioClip clipPunch;

    [Header("Ambient Noise")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private bool loopAmbient = true;

    void Awake()
    {
        audioSourceBird = GetComponent<AudioSource>();

    }

    void Start()
    {
        PlayAmbient();
    }

    void Update()
    {
        if (audioSourceBird != null)
            audioSourceBird.transform.position = transform.position;
    }

    public void PlayAmbient()
    {
        if (ambientSource == null || ambientClip == null)
            return;

        ambientSource.clip = ambientClip;
        ambientSource.loop = loopAmbient;
        ambientSource.Play();
    }
    public void PlayHitAudio(float happyLevel)
    {
        if (audioSourceBird.isPlaying)
            return;

        switch (happyLevel)
        {
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
        audioSourcePlayer.clip = clipPunch;
        audioSourcePlayer.Play();
    }

}
