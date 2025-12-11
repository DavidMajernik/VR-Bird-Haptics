using UnityEngine;

public class BirdAngerMeter : MonoBehaviour
{
    [Header("Mood Settings")]
    [SerializeField] private float currentHappyLevel = 100f;
    [Tooltip("How much mood decreases per hit")]
    [SerializeField] private float moodDecrease = 20f;
    [Tooltip("How much mood increases per pet")]
    [SerializeField] private float moodIncrease = 10f;

    [Header("Particle Effects")]
    [Tooltip("Particle system for emotional display")]
    [SerializeField] private ParticleSystem emotionParticles;
    [Tooltip("Color when bird is punched")]
    [SerializeField] private Color punchColor = Color.red;
    [Tooltip("Color when bird is petted")]
    [SerializeField] private Color petColor = Color.green;

    private ParticleSystem.MainModule particleMain;
    private const int particleBurstCount = 30;

    public float HappyLevel => currentHappyLevel;
    public bool IsHappy => currentHappyLevel > 60f;
    public bool IsAngry => currentHappyLevel < 40f;
    public System.Action OnJumpScareTriggered;

    private void Awake()
    {
        if (emotionParticles != null)
        {
            particleMain = emotionParticles.main;
        }
    }

    public void OnPunch()
    {
        currentHappyLevel -= moodDecrease;
        currentHappyLevel = Mathf.Clamp(currentHappyLevel, 0f, 100f);

        Debug.Log($"Bird was punched! Happy level: {currentHappyLevel:F1}");

        EmitParticles(punchColor);
    }

    public void OnPet()
    {
        currentHappyLevel += moodIncrease;
        currentHappyLevel = Mathf.Clamp(currentHappyLevel, 0f, 100f);

        Debug.Log($"Bird was petted! Happy level: {currentHappyLevel:F1}");

        EmitParticles(petColor);
    }

    private void EmitParticles(Color color)
    {
        if (emotionParticles == null) return;

        // Set particle color
        particleMain.startColor = color;

        // Emit burst of particles
        emotionParticles.Emit(particleBurstCount);
    }

    public void SetHappyLevel(float level)
    {
        currentHappyLevel = Mathf.Clamp(level, 0f, 100f);
    }

    public void ResetMood()
    {
        currentHappyLevel = 100f;
    }

    public float GetHappyLevel()
    {
        return currentHappyLevel;
    }
}