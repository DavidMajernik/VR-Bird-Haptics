using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BirdFlying : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _flightSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;
    [Tooltip("How high the bird arcs between points")]
    [SerializeField] private float _arcHeight = 2.0f;

    [Header("Perch Settings")]
    [Tooltip("Transform on the Left Controller where the bird should land")]
    public Transform playerPerchTarget;
    [Tooltip("How far ahead to place the control point based on current direction")]
    [SerializeField] private float _perchTransitionDistance = 2f;
    [Tooltip("Duration of the landing animation in seconds")]
    [SerializeField] private float _landingDuration = 0.5f;
    [Tooltip("Distance at which the bird starts slowing down")]
    [SerializeField] private float _slowdownDistance = 2f;
    [Tooltip("Minimum speed multiplier when approaching (0.1 = 10% of normal speed)")]
    [SerializeField] private float _minSpeedMultiplier = 0.3f;
    [Tooltip("How quickly the bird adjusts to moving perch target")]
    [SerializeField] private float _perchTrackingSpeed = 2f;

    private List<Transform> _allWaypoints;
    private Vector3 _p0, _p1, _p2;
    private float _t; //time from 0 to 1
    private bool _isPerching = false;
    private bool _isLanding = false;
    private Transform _lastWaypoint;

    // Store current tangent for smooth transitions
    private Vector3 _currentTangent;

    // Landing animation variables
    private float _landingTimer = 0f;
    private Vector3 _landingStartPos;
    private Quaternion _landingStartRot;

    public System.Action OnArrivedAtPerch;

    private void Awake()
    {
        // Find all objects tagged "Waypoint"
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Waypoint");
        _allWaypoints = new List<Transform>();
        foreach (var o in objs) _allWaypoints.Add(o.transform);

        if (_allWaypoints.Count == 0) Debug.LogWarning("No objects with tag 'Waypoint' found!");
    }

    private void OnEnable()
    {
        _isPerching = false;
        _isLanding = false;
        PickNewPath();
    }

    void Update()
    {
        // Handle landing animation separately
        if (_isLanding)
        {
            UpdateLandingAnimation();
            return;
        }

        // 1. Increment 't' based on speed and distance
        float distance = Vector3.Distance(_p0, _p2);
        if (distance <= 0.1f) distance = 0.1f;

        // Apply slowdown when approaching the perch
        float speedMultiplier = 1f;
        if (_isPerching)
        {
            float distanceToTarget = Vector3.Distance(transform.position, playerPerchTarget.position);
            if (distanceToTarget < _slowdownDistance)
            {
                // Smoothly interpolate speed from normal to minimum as we get closer
                float slowdownFactor = distanceToTarget / _slowdownDistance;
                speedMultiplier = Mathf.Lerp(_minSpeedMultiplier, 1f, slowdownFactor);
            }
        }

        _t += (Time.deltaTime * _flightSpeed * speedMultiplier) / distance;

        // 2. Smoothly adjust target if perching (for moving hands)
        if (_isPerching && playerPerchTarget != null)
        {
            // Smoothly update the target position instead of snapping
            _p2 = Vector3.Lerp(_p2, playerPerchTarget.position, _perchTrackingSpeed * Time.deltaTime);
        }

        // 3. Check for arrival
        if (_t >= 1.0f)
        {
            _t = 1.0f;

            if (_isPerching)
            {
                // Start landing animation
                StartLandingAnimation();
                return;
            }
            else
            {
                PickNewPath();
            }
        }

        // 4. Calculate Bezier Position
        Vector3 newPos = CalculateQuadraticBezier(_p0, _p1, _p2, _t);

        // 5. Calculate current tangent (for smooth transitions)
        _currentTangent = CalculateBezierTangent(_p0, _p1, _p2, _t);

        // 6. Update Rotation to look forward along the curve
        if (_currentTangent != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(_currentTangent);

            // If perching, rotate instantly toward perch target
            if (_isPerching && playerPerchTarget != null)
            {
                Vector3 directionToPerch = (playerPerchTarget.position - transform.position).normalized;
                if (directionToPerch != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(directionToPerch);
                }
            }
            else
            {
                // Normal smooth rotation for waypoint flying
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
            }
        }

        // 7. Apply Position
        transform.position = newPos;
    }

    private void StartLandingAnimation()
    {
        _isLanding = true;
        _landingTimer = 0f;
        _landingStartPos = transform.position;
        _landingStartRot = transform.rotation;
    }

    private void UpdateLandingAnimation()
    {
        _landingTimer += Time.deltaTime;
        float normalizedTime = Mathf.Clamp01(_landingTimer / _landingDuration);

        // Use ease-out curve for smooth deceleration
        float easedTime = 1f - Mathf.Pow(1f - normalizedTime, 3f);

        // Smoothly move to perch position
        transform.position = Vector3.Lerp(_landingStartPos, playerPerchTarget.position, easedTime);

        // Smoothly rotate to match perch rotation
        transform.rotation = Quaternion.Slerp(_landingStartRot, playerPerchTarget.rotation, easedTime);

        // Check if landing animation is complete
        if (normalizedTime >= 1f)
        {
            _isLanding = false;
            OnArrivedAtPerch?.Invoke();
            enabled = false;
        }
    }

    public void FlyToPerch()
    {
        if (_isPerching) return;

        _isPerching = true;

        // Setup path to Hand
        _p0 = transform.position;
        _p2 = playerPerchTarget.position;

        // This makes the bird continue in its current direction before curving toward the perch
        Vector3 currentDirection = _currentTangent.normalized;

        // If we don't have a valid tangent yet, use forward
        if (currentDirection == Vector3.zero)
        {
            currentDirection = transform.forward;
        }

        // Place control point ahead of bird in its current direction then blend it
        Vector3 directionBasedPoint = _p0 + currentDirection * _perchTransitionDistance;

        // Blend between direction-based point and a point between start/end
        Vector3 midPoint = (_p0 + _p2) / 2f + Vector3.up * _arcHeight;

        // Weight more toward current direction
        _p1 = Vector3.Lerp(midPoint, directionBasedPoint, 0.6f);

        _t = 0f; // Reset path progress
        _lastWaypoint = null; //clear last waypoint
    }

    private void PickNewPath()
    {
        if (_allWaypoints.Count == 0) return;

        //current pos
        _p0 = transform.position;

        //new target
        Transform targetWp;

        if (_allWaypoints.Count > 1)
        {
            do
            {
                targetWp = _allWaypoints[Random.Range(0, _allWaypoints.Count)];
            }
            while (targetWp == _lastWaypoint);
        }
        else
        {
            targetWp = _allWaypoints[0];
        }

        _lastWaypoint = targetWp;
        _p2 = targetWp.position;

        // For regular waypoint transitions, also use current tangent for smoothness
        Vector3 currentDirection = _currentTangent.normalized;
        if (currentDirection == Vector3.zero)
        {
            currentDirection = transform.forward;
        }

        Vector3 mathematicalCenter = (_p0 + _p2) / 2;
        Vector3 baseArc = mathematicalCenter + (Vector3.up * _arcHeight);

        // Blend current direction with random variation
        Vector3 directionInfluence = _p0 + currentDirection * 2f;
        _p1 = Vector3.Lerp(baseArc + (Random.insideUnitSphere * 5f), directionInfluence, 0.3f);

        _t = 0f;
    }

    // Formula: (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
    private Vector3 CalculateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return p;
    }

    // Calculates the direction the bird is facing at point 't'
    private Vector3 CalculateBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        return 2 * u * (p1 - p0) + 2 * t * (p2 - p1);
    }
}