using UnityEngine;

public class Addbones : MonoBehaviour
{
    [ContextMenu("Add Rigidbodies to All Bones")]
    void AddRigidbodies()
    {
        Transform[] bones = GetComponentsInChildren<Transform>();

        foreach (Transform bone in bones)
        {
            // Skip if already has Rigidbody
            if (bone.GetComponent<Rigidbody>() != null) continue;

            // Skip root object
            if (bone == transform) continue;

            // Add Rigidbody
            Rigidbody rb = bone.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.mass = 0.2f;

            // Add Capsule Collider
            CapsuleCollider col = bone.gameObject.AddComponent<CapsuleCollider>();
            col.radius = 0.01f;
            col.height = 0.01f;

            Debug.Log($"Added Rigidbody to {bone.name}");
        }
    }
}