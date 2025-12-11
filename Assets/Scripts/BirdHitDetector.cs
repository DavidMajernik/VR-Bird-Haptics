using UnityEngine;

public class BirdHitDetector : MonoBehaviour
{
    BirdController controller;
    BirdAngerMeter angerMeter;

    [Header("Hit Settings")]
    [SerializeField] private float _minHitSpeed = 5f;
    [SerializeField] private float _forceMultiplier = 1f;

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

        // Punch
        if (speed >= _minHitSpeed)
        {
            Vector3 impactForce = (tracker != null)
                ? tracker.CurrentVelocity * _forceMultiplier
                : (transform.position - other.transform.position).normalized * 10f;

            angerMeter?.OnPunch();
            controller?.SetState(BirdState.Ragdoll, impactForce);
        }
        else
        {
            // Pet
            angerMeter?.OnPet();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Reset reaction so next hand entry can be detected
        if (other.CompareTag("PlayerHand"))
        {
            hasReacted = false;
        }
    }
}
