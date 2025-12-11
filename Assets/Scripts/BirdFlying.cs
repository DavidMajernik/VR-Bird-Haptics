using UnityEngine;
using System.Collections.Generic;

public class BirdFlying : MonoBehaviour
{
    [Header("Flight Settings")]
    public float flightSpeed = 5f;
    public float rotationSpeed = 8f;

    [Header("Bezier Arc Settings")]
    [Tooltip("How high the arc goes based on waypoint distance.")]
    public float arcHeightFactor = 0.25f;

    [Tooltip("Random variance only applied to vertical arc.")]
    public float arcRandomness = 0.5f;

    [Header("Perch Settings")]
    public Transform playerPerchTarget;
    public float landingDuration = 0.5f;
    public float slowdownDistance = 2f;
    public float minSpeedMultiplier = 0.3f;
    public float perchTrackingSpeed = 3f;

    public System.Action OnArrivedAtPerch;
    private BirdAngerMeter angerMeter;

    private bool _isPerching = false;
    private bool _isLanding = false;

    private Vector3 p0, p1, p2, p3;
    private float t = 0f;

    private Vector3 exitVelocity;

    private float landingTimer = 0f;
    private Vector3 landingStartPos;
    private Quaternion landingStartRot;

    private List<Transform> friendlyWaypoints = new List<Transform>();
    private List<Transform> neutralWaypoints = new List<Transform>();
    private List<Transform> angryWaypoints = new List<Transform>();
    private Transform lastWaypoint;

    void Awake()
    {
        BuildWaypointList();
        HideWaypointMeshes();
        angerMeter = GetComponentInChildren<BirdAngerMeter>();
    }

    void OnEnable()
    {
        _isPerching = false;
        _isLanding = false;
        exitVelocity = transform.forward * flightSpeed;

        PickNewPath();
    }

    void Update()
    {
        if (_isLanding)
        {
            UpdateLandingAnimation();
            return;
        }

        // Track perch target if moving
        if (_isPerching && playerPerchTarget != null)
        {
            p3 = Vector3.Lerp(p3, playerPerchTarget.position, perchTrackingSpeed * Time.deltaTime);
        }

        // Recalculate path length AFTER perch tracking
        float pathLength = EstimateBezierLength(p0, p1, p2, p3);

        float speedMultiplier = 1f;
        if (_isPerching)
        {
            float dist = Vector3.Distance(transform.position, playerPerchTarget.position);
            if (dist < slowdownDistance)
            {
                float s = dist / slowdownDistance;
                speedMultiplier = Mathf.Lerp(minSpeedMultiplier, 1f, s);
            }
        }

        t += (Time.deltaTime * flightSpeed * speedMultiplier) / pathLength;

        // Reached end of curve
        if (t >= 1f)
        {
            t = 1f;

            exitVelocity = CalculateBezierTangent(p0, p1, p2, p3, 1f).normalized * flightSpeed;

            if (_isPerching)
            {
                StartLanding();
                return;
            }
            else
            {
                PickNewPath();
            }
        }

        // Move along curve
        Vector3 pos = CalculateBezier(p0, p1, p2, p3, t);
        Vector3 tangent = CalculateBezierTangent(p0, p1, p2, p3, t);

        transform.position = pos;

        if (tangent != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(tangent, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    void StartLanding()
    {
        _isLanding = true;
        landingTimer = 0f;
        landingStartPos = transform.position;
        landingStartRot = transform.rotation;
    }

    void UpdateLandingAnimation()
    {
        landingTimer += Time.deltaTime;
        float tNorm = Mathf.Clamp01(landingTimer / landingDuration);

        float ease = 1f - Mathf.Pow(1f - tNorm, 3f);

        transform.position = Vector3.Lerp(landingStartPos, playerPerchTarget.position, ease);
        transform.rotation = Quaternion.Slerp(landingStartRot, playerPerchTarget.rotation, ease);

        if (tNorm >= 1f)
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

        p0 = transform.position;
        p3 = playerPerchTarget.position;

        Vector3 dir = (p3 - p0).normalized;
        float dist = Vector3.Distance(p0, p3);
        Vector3 exitDir = exitVelocity.normalized;

        p1 = p0 + exitDir * Mathf.Min(dist * 0.33f, 6f);
        p2 = p3 - dir * Mathf.Min(dist * 0.33f, 6f);

        // Vertical arc only
        float arc = dist * arcHeightFactor + Random.Range(-arcRandomness, arcRandomness);
        p1 += Vector3.up * arc;
        p2 += Vector3.up * arc * 0.5f;

        t = 0f;
        lastWaypoint = null;
    }

    private void PickNewPath()
    {
        List<Transform> currentWaypoints;
        float happyLevel = angerMeter.GetHappyLevel();

        // Choose waypoint list based on anger level
        if (happyLevel > 60f)
        {
            currentWaypoints = friendlyWaypoints;
        }
        else if (happyLevel < 40f)
        {
            currentWaypoints = angryWaypoints;
        }
        else
        {
            currentWaypoints = neutralWaypoints;
        }

        if (currentWaypoints.Count == 0) return;

        Transform target;
        do
        {
            target = currentWaypoints[Random.Range(0, currentWaypoints.Count)];
        }
        while (target == lastWaypoint && currentWaypoints.Count > 1);

        lastWaypoint = target;

        p0 = transform.position;
        p3 = target.position;

        float dist = Vector3.Distance(p0, p3);
        Vector3 dir = (p3 - p0).normalized;
        Vector3 exitDir = exitVelocity.normalized;

        p1 = p0 + exitDir * Mathf.Min(dist * 0.33f, 6f);
        p2 = p3 - dir * Mathf.Min(dist * 0.33f, 6f);

        float arc = dist * arcHeightFactor + Random.Range(-arcRandomness, arcRandomness);
        p1 += Vector3.up * arc;
        p2 += Vector3.up * arc * 0.5f;

        t = 0f;
    }

    Vector3 CalculateBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1 - t;
        return
            u * u * u * a +
            3f * u * u * t * b +
            3f * u * t * t * c +
            t * t * t * d;
    }

    Vector3 CalculateBezierTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1 - t;
        return
            3f * u * u * (b - a) +
            6f * u * t * (c - b) +
            3f * t * t * (d - c);
    }

    float EstimateBezierLength(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int segments = 20)
    {
        float len = 0f;
        Vector3 prev = a;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 p = CalculateBezier(a, b, c, d, t);
            len += Vector3.Distance(prev, p);
            prev = p;
        }
        return len;
    }

    void BuildWaypointList()
    {
        AddWaypoints("FriendlyWaypoint", friendlyWaypoints);
        AddWaypoints("NeutralWaypoint", neutralWaypoints);
        AddWaypoints("AngryWaypoint", angryWaypoints);
    }

    void AddWaypoints(string tag, List<Transform> temp)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
            temp.Add(obj.transform);
    }

    void HideWaypointMeshes()
    {
        foreach (var t in friendlyWaypoints)
            if (t.TryGetComponent(out Renderer r)) r.enabled = false;
        foreach (var t in neutralWaypoints)
            if (t.TryGetComponent(out Renderer r)) r.enabled = false;
        foreach (var t in angryWaypoints)
            if (t.TryGetComponent(out Renderer r)) r.enabled = false;
    }
}
