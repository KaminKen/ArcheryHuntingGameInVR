// using System.Runtime.Serialization;
// using Microsoft.CSharp.RuntimeBinder;
// using System.Reflection;
// using System.Threading.Tasks.Dataflow;
// using UnityEngine;

// public class StickingArrowToSurfaces : MonoBehaviour
// {
//     [SerializeField]
//     private Rigidbody rb;
//     [SerializeField]
//     private SphereCollider myCollider;
//     [SerializeField]
//     private GameObject stickingArrow;

//     private void OnCollisionEnter(Collision collision)
//     {
//         rb.isKinematic = true;
//         myCollider.isTrigger = true;

//         GameObject arrow = Instantiate(stickingArrow);
//         arrow.transform.position = transform.position;
//         arrow.transform.forward = transform.forward; //rotation of the object to be forward

//         if(collision.collider.attachedRigidbod != null)
//         {
//             arrow.transform.parent = collision.collider.attachedRigidbody.transform;
//         }
//         collision.collider.GetComponent<IHittable>()?.GetHit();
//         Destroy(gameObject);
//     }
// }

using UnityEngine;

public class StickingArrowToSurfaces : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider myCollider;          // SphereCollider is fine, Collider is more flexible
    [SerializeField] private GameObject stickingArrowPrefab;

    private bool hasStuck = false;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasStuck) return;
        hasStuck = true;

        // Stop this flying arrow
        rb.isKinematic = true;
        rb.detectCollisions = false;
        myCollider.enabled = false;

        // Spawn stuck arrow at the impact point
        ContactPoint hit = collision.GetContact(0);

        // Keep same orientation as your flying arrow (since you use forward)
        Quaternion rot = transform.rotation;

        GameObject stuckArrow = Instantiate(stickingArrowPrefab, hit.point, rot);

        // Parent to the rigidbody hit so it moves with it (Unity 6-safe)
        if (collision.rigidbody != null)
        {
            stuckArrow.transform.SetParent(collision.rigidbody.transform, true);
        }

        // Call hit logic if present
        collision.collider.GetComponent<IHittable>()?.GetHit();

        Destroy(gameObject);
    }
}
