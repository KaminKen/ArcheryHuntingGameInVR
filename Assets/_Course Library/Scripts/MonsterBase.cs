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
    
    [Header("Combat Settings")]
    [Tooltip("Monster health points")]
    public float health = 100f;
    
    [Tooltip("Damage dealt to camp when reaching it")]
    public float attackDamage = 10f;
    
    [Header("Animation Settings")]
    [Tooltip("Reference to the Animator component")]
    public Animator animator;
    
    [Tooltip("Duration of spawn animation (monster won't move during this time)")]
    public float spawnDuration = 1f;
    
    [Tooltip("Time to wait before destroying monster after attack (for death animation)")]
    public float destroyDelay = 5f;

    // Animation parameter names
    protected const string ANIM_SPAWN = "Spawn";
    protected const string ANIM_WALK = "Walk";
    protected const string ANIM_ATTACK = "Attack";
    protected const string ANIM_DIE = "Die";

    protected bool isAlive = true;
    protected bool isSpawning = true;
    protected bool hasAttacked = false;
    protected float spawnTimer = 0f;
    protected float safetyRadius = 2f; // Will be set by spawner

    protected virtual void Start()
    {
        // Get Animator component if not assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning($"No Animator found on {gameObject.name}!");
        }

        // Play spawn animation
        PlaySpawnAnimation();
    }

    protected virtual void Update()
    {
        if (!isAlive || target == null || hasAttacked) return;

        // Handle spawn phase
        if (isSpawning)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnDuration)
            {
                isSpawning = false;
                OnSpawnComplete();
            }
            return; // Don't move or attack while spawning
        }

        // Check distance to target
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Check if monster entered safety radius
        if (distanceToTarget <= safetyRadius)
        {
            PerformFinalAttack();
        }
        else
        {
            // Move towards target
            MoveTowardsTarget();
        }
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
    /// Set the safety radius from MonsterSpawner
    /// Called by MonsterSpawner
    /// </summary>
    public virtual void SetSafetyRadius(float radius)
    {
        safetyRadius = radius;
    }

    /// <summary>
    /// Play spawn animation
    /// </summary>
    protected virtual void PlaySpawnAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(ANIM_SPAWN);
        }
    }

    /// <summary>
    /// Called when spawn animation completes
    /// </summary>
    protected virtual void OnSpawnComplete()
    {
        // Start walking
        if (animator != null)
        {
            animator.SetBool(ANIM_WALK, true);
        }
    }

    /// <summary>
    /// Basic movement towards target
    /// Override this for custom movement behaviors
    /// </summary>
    protected virtual void MoveTowardsTarget()
    {
        if (target == null) return;

        // Make sure walk animation is playing
        if (animator != null && !animator.GetBool(ANIM_WALK))
        {
            animator.SetBool(ANIM_WALK, true);
        }

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
    /// Perform final attack when entering safety radius
    /// </summary>
    protected virtual void PerformFinalAttack()
    {
        if (hasAttacked) return;
        
        hasAttacked = true;

        // Stop walking
        if (animator != null)
        {
            animator.SetBool(ANIM_WALK, false);
        }

        // Face the target
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        directionToTarget.y = 0;
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = targetRotation;
        }

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger(ANIM_ATTACK);
        }

        // Deal damage to camp
        DealDamageToCamp();

        // Destroy monster after delay (for animation to complete)
        Destroy(gameObject, destroyDelay);
    }

    /// <summary>
    /// [WIP] Deal damage to the camp
    /// </summary>
    protected virtual void DealDamageToCamp()
    {
        // TODO: Implement camp damage logic
        Debug.Log($"{gameObject.name} dealt {attackDamage} damage to camp!");
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
        
        // Stop all movement
        if (animator != null)
        {
            animator.SetBool(ANIM_WALK, false);
            animator.SetTrigger(ANIM_DIE);
        }

        // Disable collider if exists
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Destroy after death animation (adjust timing as needed)
        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmos()
    {
        // Visualize safety radius (in red)
        if (target != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(target.position, safetyRadius);
        }

        // Visualize target line
        if (target != null && isAlive)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}