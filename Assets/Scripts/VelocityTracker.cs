using UnityEngine;

public class VelocityTracker : MonoBehaviour
{
    public Vector3 CurrentVelocity { get; private set; }

    private Vector3 _previousPosition;

    private void OnEnable()
    {
        _previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        CurrentVelocity = (transform.position - _previousPosition) / Time.fixedDeltaTime;
        _previousPosition = transform.position;
    }
}