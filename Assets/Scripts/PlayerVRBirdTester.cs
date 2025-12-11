using UnityEngine;

/// <summary>
/// Tracks VR hand velocity, provides a perch point,
/// and exposes helper functions for bird interaction.
/// Works with the Unity XR Template.
/// </summary>
public class PlayerVRBirdTester : MonoBehaviour
{
    [Header("XR Rig References")]
    public Transform head;           // Main camera
    public Transform leftHand;       // Left controller
    public Transform rightHand;      // Right controller

    [Header("Bird Perching")]
    public Transform armPerch;       // Transform on player's arm
    public float armPerchRadius = 0.15f;

    [Header("Hit Detection Settings")]
    public float minHitVelocity = 1.0f;
    public float velocitySmoothing = 10f;  // Higher = smoother

    private Vector3 leftHandLastPos;
    private Vector3 rightHandLastPos;

    private Vector3 _leftHandVel;
    private Vector3 _rightHandVel;

    public Vector3 leftHandVelocity => _leftHandVel;
    public Vector3 rightHandVelocity => _rightHandVel;

    void Awake()
    {
        AutoAssignXRObjects();
    }

    void Start()
    {
        // Initialize last positions
        if (leftHand) leftHandLastPos = leftHand.position;
        if (rightHand) rightHandLastPos = rightHand.position;
    }

    void Update()
    {
        CalculateHandVelocities();
    }

    // -----------------------------------------------------------
    // Hand Velocity Calculation
    // -----------------------------------------------------------
    private void CalculateHandVelocities()
    {
        if (leftHand)
        {
            Vector3 raw = (leftHand.position - leftHandLastPos) / Time.deltaTime;
            _leftHandVel = Vector3.Lerp(_leftHandVel, raw, Time.deltaTime * velocitySmoothing);
            leftHandLastPos = leftHand.position;
        }

        if (rightHand)
        {
            Vector3 raw = (rightHand.position - rightHandLastPos) / Time.deltaTime;
            _rightHandVel = Vector3.Lerp(_rightHandVel, raw, Time.deltaTime * velocitySmoothing);
            rightHandLastPos = rightHand.position;
        }
    }

    // -----------------------------------------------------------
    // Used by BirdHitDetector
    // -----------------------------------------------------------
    public bool HandIsHitting(Collider col, out Vector3 velocity)
    {
        // Compare transforms directly
        if (col.transform == leftHand)
        {
            velocity = leftHandVelocity;
            return leftHandVelocity.magnitude >= minHitVelocity;
        }

        if (col.transform == rightHand)
        {
            velocity = rightHandVelocity;
            return rightHandVelocity.magnitude >= minHitVelocity;
        }

        velocity = Vector3.zero;
        return false;
    }

    // -----------------------------------------------------------
    // Gizmos for debugging
    // -----------------------------------------------------------
    void OnDrawGizmos()
    {
        if (armPerch)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(armPerch.position, armPerchRadius);
        }

        if (leftHand)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(leftHand.position, leftHandVelocity * 0.1f);
        }

        if (rightHand)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(rightHand.position, rightHandVelocity * 0.1f);
        }
    }

    // -----------------------------------------------------------
    // Auto-assign XR references if the user forgot
    // -----------------------------------------------------------
    private void AutoAssignXRObjects()
    {
        if (!head)
        {
            var cam = Camera.main;
            if (cam) head = cam.transform;
        }

        if (!leftHand || !rightHand)
        {
            // Use FindObjectsByType instead of deprecated FindObjectsOfType
            var hands = FindObjectsByType<Transform>(FindObjectsSortMode.None);

            foreach (var t in hands)
            {
                if (t.name.ToLower().Contains("left") && t.name.ToLower().Contains("hand"))
                    leftHand = t;

                if (t.name.ToLower().Contains("right") && t.name.ToLower().Contains("hand"))
                    rightHand = t;
            }
        }
    }
}
