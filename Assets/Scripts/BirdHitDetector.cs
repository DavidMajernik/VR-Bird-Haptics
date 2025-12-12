using Oculus.Haptics;
using UnityEngine;

public class BirdHitDetector : MonoBehaviour
{
    BirdController controller;
    BirdAngerMeter angerMeter;
    [SerializeField] private HitSounds hitSounds;

    [Header("Hit Settings")]
    [SerializeField] private float _minHitSpeed = 5f;
    [SerializeField] private float _forceMultiplier = 1f;

    [Header("Haptics Clips (.haptic files)")]
    [SerializeField] private HapticClip punchClip;
    [SerializeField] private HapticClip petClip;

    private HapticClipPlayer leftPunchPlayer;
    private HapticClipPlayer rightPunchPlayer;
    private HapticClipPlayer leftPetPlayer;
    private HapticClipPlayer rightPetPlayer;

    private bool hasReacted = false;

    void Awake()
    {
        controller = GetComponentInParent<BirdController>();
        angerMeter = GetComponent<BirdAngerMeter>();
        if (angerMeter == null)
        {
            Debug.LogWarning("No BirdAngerMeter found.");
        }
    }

    void Start()
    {
        // Initialize haptic clip players
        if (punchClip != null)
        {
            leftPunchPlayer = new HapticClipPlayer(punchClip);
            rightPunchPlayer = new HapticClipPlayer(punchClip);
        }

        if (petClip != null)
        {
            leftPetPlayer = new HapticClipPlayer(petClip);
            rightPetPlayer = new HapticClipPlayer(petClip);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasReacted) return;
        if (!other.CompareTag("PlayerHand"))
            return;

        hasReacted = true;

        VelocityTracker tracker = other.GetComponent<VelocityTracker>();
        if (tracker == null)
            tracker = other.GetComponentInParent<VelocityTracker>();

        float speed = (tracker != null) ? tracker.CurrentVelocity.magnitude : 0f;

        // Determine which hand hit the bird
        Controller handController = DetermineHandController(other.gameObject);

        // Punch
        if (speed >= _minHitSpeed)
        {
            Vector3 impactForce = (tracker != null)
                ? tracker.CurrentVelocity * _forceMultiplier
                : (transform.position - other.transform.position).normalized * 10f;

            angerMeter?.OnPunch();
            controller?.SetState(BirdState.Ragdoll, impactForce);
            hitSounds.PlayPunchAudio();

            // Play punch haptic on the hand that hit
            PlayPunchHaptic(handController);
        }
        else
        {
            angerMeter?.OnPet();

            //lol there is no pet haptic but if you wanted you could!
            PlayPetHaptic(handController);
        }

        hitSounds.PlayHitAudio(angerMeter.HappyLevel);
    }

    void OnTriggerExit(Collider other)
    {
        // Reset reaction so next hand entry can be detected
        if (other.CompareTag("PlayerHand"))
        {
            hasReacted = false;
        }
    }

    private Controller DetermineHandController(GameObject handObject)
    {
        // Try to identify the hand by name or tag
        string objName = handObject.name.ToLower();

        if (objName.Contains("left"))
        {
            return Controller.Left;
        }
        else if (objName.Contains("right"))
        {
            return Controller.Right;
        }

        // Check parent objects
        Transform parent = handObject.transform.parent;
        while (parent != null)
        {
            string parentName = parent.name.ToLower();
            if (parentName.Contains("left"))
            {
                return Controller.Left;
            }
            else if (parentName.Contains("right"))
            {
                return Controller.Right;
            }
            parent = parent.parent;
        }

        // Default to right if unable to determine
        Debug.LogWarning($"Could not determine hand controller for {handObject.name}, defaulting to Right");
        return Controller.Right;
    }

    private void PlayPunchHaptic(Controller hand)
    {
        if (punchClip == null)
            return;

        if (hand == Controller.Left && leftPunchPlayer != null)
        {
            leftPunchPlayer.Play(Controller.Left);
        }
        else if (hand == Controller.Right && rightPunchPlayer != null)
        {
            rightPunchPlayer.Play(Controller.Right);
        }
    }

    private void PlayPetHaptic(Controller hand)
    {
        if (petClip == null)
            return;

        if (hand == Controller.Left && leftPetPlayer != null)
        {
            leftPetPlayer.Play(Controller.Left);
        }
        else if (hand == Controller.Right && rightPetPlayer != null)
        {
            rightPetPlayer.Play(Controller.Right);
        }
    }

    void OnDestroy()
    {
        // Clean up haptic players
        leftPunchPlayer?.Dispose();
        rightPunchPlayer?.Dispose();
        leftPetPlayer?.Dispose();
        rightPetPlayer?.Dispose();
    }

    void OnApplicationQuit()
    {
        Haptics.Instance.Dispose();
    }
}