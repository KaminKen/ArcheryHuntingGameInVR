using UnityEngine;

/// <summary>
/// Deer monster behavior that inherits from MonsterBase
/// Calls openmanager when hit by an arrow
/// </summary>
public class DeerCollider : MonsterBase
{
    [Header("Deer Specific Settings")]
    [Tooltip("Whether this deer has already triggered the OnDeerHit event")]
    private bool hasTriggeredHitEvent = false;

    /// <summary>
    /// Override GetHit to add deer-specific behavior
    /// </summary>
    public override void GetHit(Collider hitCollider)
    {
        if (!isAlive) return;

        // Call OpenManager.OnDeerHit() when deer is hit (only once per deer)
        if (!hasTriggeredHitEvent)
        {
            hasTriggeredHitEvent = true;
            
            if (openmanager.Instance != null)
            {
                openmanager.Instance.OnDeerHit();
                Debug.Log($"{gameObject.name} was hit! Called openmanager.OnDeerHit()");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} tried to call openmanager.OnDeerHit() but openmanager.Instance is null!");
            }
        }

        // Call base class GetHit to handle damage and other logic
        base.GetHit(hitCollider);
    }

    /// <summary>
    /// Optional: Override TakeDamage if you want to call OnDeerHit for any damage source
    /// Currently using GetHit override which is specifically for arrow hits
    /// </summary>
    // public override void TakeDamage(float damage)
    // {
    //     if (!isAlive) return;
    //     
    //     if (!hasTriggeredHitEvent)
    //     {
    //         hasTriggeredHitEvent = true;
    //         
    //         if (openmanager.Instance != null)
    //         {
    //             openmanager.Instance.OnDeerHit();
    //         }
    //     }
    //     
    //     base.TakeDamage(damage);
    // }
}