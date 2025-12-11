using UnityEngine;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

public class HitSounds : MonoBehaviour
{
    BirdAngerMeter angerMeter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        angerMeter = GetComponent<BirdAngerMeter>();

        if (angerMeter == null)
        {
            Debug.LogWarning("No BirdAngerMeter found.");
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
