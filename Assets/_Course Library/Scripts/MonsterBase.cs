using UnityEngine;

/// <summary>
/// Base class for all monster types
/// Inherit from this class to create specific monster behaviors
/// </summary>
public class MonsterBase : MonoBehaviour
{
    [Header("Target Settings")]
    protected Transform target; // The camp center or target position
    
    [Header("Movement Settings")]
    [Tooltip("Monster movement speed")]
    public float moveSpeed = 2f;
    
    [Tooltip("Rotation speed when turning towards target")]
    public float rotationSpeed = 5f;
    
    [Header("Monster Stats")]
    [Tooltip("Monster health points")]
    public float health = 100f;
    
    [Tooltip("Damage dealt to camp when reaching it")]
    public float attackDamage = 10f;

    protected bool isAlive = true;

    protected virtual void Start()
    {
        // Override this in child classes for custom initialization
    }

    protected virtual void Update()
    {
        if (!isAlive || target == null) return;

        // Default behavior: move towards target
        MoveTowardsTarget();
    }

    /// <summary>
    /// Set the target that this monster should move towards
    /// Called by MonsterSpawner
    /// </summary>
    public virtual void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Basic movement towards target
    /// Override this for custom movement behaviors
    /// </summary>
    protected virtual void MoveTowardsTarget()
    {
        if (target == null) return;

        // Calculate direction to target
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Keep movement on horizontal plane

        // Rotate towards target
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move forward
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Take damage
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (!isAlive) return;

        health -= damage;
        
        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Called when monster dies
    /// Override for custom death behavior
    /// </summary>
    protected virtual void Die()
    {
        isAlive = false;
        // Add death animation, effects, etc. here
        Destroy(gameObject, 0.1f); // Small delay for any death effects
    }

    /// <summary>
    /// Called when monster reaches the camp
    /// Override for custom attack behavior
    /// </summary>
    protected virtual void OnReachCamp()
    {
        // Notify camp of damage (you'll need to implement CampHealth script)
        // CampHealth campHealth = target.GetComponent<CampHealth>();
        // if (campHealth != null)
        // {
        //     campHealth.TakeDamage(attackDamage);
        // }
        
        Die(); // Monster dies after attacking
    }

    /// <summary>
    /// Check if monster has reached the camp
    /// </summary>
    protected virtual bool HasReachedTarget()
    {
        if (target == null) return false;
        
        float distance = Vector3.Distance(transform.position, target.position);
        return distance < 1f; // Adjust this threshold as needed
    }

    private void OnDrawGizmos()
    {
        // Visualize target in editor
        if (target != null && isAlive)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}