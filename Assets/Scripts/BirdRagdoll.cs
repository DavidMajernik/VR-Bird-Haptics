using UnityEngine;
using System.Collections.Generic;

public class BirdRagdoll : MonoBehaviour
{
    private List<Rigidbody> _ragdollRigidbodies = new List<Rigidbody>();
    private Rigidbody _mainRigidBody;
    public Animator _animator;

    [Tooltip("Rigidbodies that should NEVER be set to kinematic")]
    [SerializeField] private List<Rigidbody> _exceptions;

    private void Awake()
    {
        _mainRigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>(); // Try to find on self
        if (_animator == null) _animator = GetComponentInParent<Animator>(); // Try parent

        Rigidbody[] allRbs = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRbs)
        {
            if (rb == _mainRigidBody) continue;
            if (_exceptions != null && _exceptions.Contains(rb)) continue;
            _ragdollRigidbodies.Add(rb);
        }

        DisableRagdoll();
    }

    public void EnableRagdoll(Vector3 impactForce = default(Vector3))
    {
        // Disable animator FIRST so physics can take over cleanly
        if (_animator != null) _animator.enabled = false;

        _mainRigidBody.isKinematic = false;
        _mainRigidBody.useGravity = true;

        if (impactForce != Vector3.zero)
        {
            _mainRigidBody.AddForce(impactForce, ForceMode.Impulse);
        }

        foreach (var rb in _ragdollRigidbodies)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.useGravity = true;

            // Reset velocities to prevent "explosions" when enabling
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (impactForce != Vector3.zero)
            {
                rb.AddForce(impactForce, ForceMode.Impulse);
            }
        }
    }

    public void DisableRagdoll()
    {
        // Disable ragdoll physics first
        foreach (var rb in _ragdollRigidbodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        _mainRigidBody.isKinematic = true;
        _mainRigidBody.useGravity = false;
        _mainRigidBody.linearVelocity = Vector3.zero;
        _mainRigidBody.angularVelocity = Vector3.zero;

        // Re-enable animator AFTER physics is disabled to snap bones back to animated pose
        if (_animator != null)
        {
            _animator.enabled = true;
        }
    }
}