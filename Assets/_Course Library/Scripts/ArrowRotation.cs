using UnityEngine;

public class ArrowRotation : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float rotationSpeed = 15f;

    private void FixedUpdate()
    {
        if (rb.linearVelocity.sqrMagnitude < 0.001f) return;

        Vector3 direction = rb.linearVelocity.normalized;
        transform.forward = Vector3.Slerp(
            transform.forward,
            direction,
            rotationSpeed * Time.fixedDeltaTime
        );
    }
}
