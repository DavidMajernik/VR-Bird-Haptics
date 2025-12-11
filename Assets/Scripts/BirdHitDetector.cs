using UnityEngine;

public class BirdHitDetector : MonoBehaviour
{
    BirdController controller;

    [Header("Hit Settings")]
    [Tooltip("The minimum speed the hand must be moving to trigger the ragdoll (in meters/sec).")]
    [SerializeField] private float _minHitSpeed = 4f;

    [Tooltip("Multiplies the hand velocity to create the impact force. Higher = bird flies further.")]
    [SerializeField] private float _forceMultiplier = 1f;

    void Awake()
    {
        controller = GetComponentInParent<BirdController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (controller == null)
        {
            Debug.LogWarning("No BirdController found on BirdHitDetector!");
            return;
        }
        else
        {
            Debug.Log("BirdHitDetector detected a collision.");
        }

        if (other.CompareTag("PlayerHand"))
        {
            VelocityTracker tracker = other.GetComponent<VelocityTracker>();

            if (tracker == null) tracker = other.GetComponentInParent<VelocityTracker>();

            if (tracker != null)
            {
                float speed = tracker.CurrentVelocity.magnitude;

                if (speed < _minHitSpeed)
                {
                    Debug.Log($"Hit too weak: {speed} m/s (Needed: {_minHitSpeed})");
                    return;
                }

                Vector3 impactForce = tracker.CurrentVelocity * _forceMultiplier;

                Debug.Log($"Bird SMACKED! Speed: {speed}, Force: {impactForce}");
                controller.SetState(BirdState.Ragdoll, impactForce);
            }
            else
            {
                Debug.LogWarning("No VelocityTracker found on PlayerHand! Using default force.");
                Vector3 direction = (transform.position - other.transform.position).normalized;
                controller.SetState(BirdState.Ragdoll, direction * 10f);
            }
        }
    }
}