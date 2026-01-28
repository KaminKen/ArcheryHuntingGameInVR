using UnityEngine;

public class DeerCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        // Check if the colliding object has the "Arrow" tag (Make sure to set the Tag on the arrow prefab)
        if (collision.CompareTag("Arrow"))
        {
            Debug.Log("Deer was hit by an arrow!");

            // Destroy the arrow (optional feature)
            Destroy(collision.gameObject);

            // Request scene transition from the manager
            openmanager.Instance.OnDeerHit();
        }
    }
}