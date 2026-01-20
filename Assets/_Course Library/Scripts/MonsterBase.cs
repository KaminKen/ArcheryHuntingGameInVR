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
    
    [Tooltip("Attack range - distance at which monster stops and attacks")]
    public float attackRange = 1.5f;
    
    [Tooltip("Time between attacks")]
    public float attackCooldown = 2f;
    
    [Header("Animation Settings")]
    [Tooltip("Reference to the Animator component")]
    public Animator animator;
    
    [Tooltip("Duration of spawn animation (monster won't move during this time)")]
    public float spawnDuration = 1f;

    // Animation parameter names
    protected const string ANIM_SPAWN = "Spawn";
    protected const string ANIM_WALK = "Walk";
    protected const string ANIM_ATTACK = "Attack";
    protected const string ANIM_DIE = "Die";

    protected bool isAlive = true;
    protected bool isSpawning = true;
    protected bool isAttacking = false;
    protected float spawnTimer = 0f;
    protected float attackTimer = 0f;

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
        if (!isAlive || target == null) return;

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

        // Handle attack behavior
        if (distanceToTarget <= attackRange)
        {
            HandleAttack();
        }
        else
        {
            // Move towards target
            isAttacking = false;
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
    /// Handle attack behavior when near target
    /// </summary>
    protected virtual void HandleAttack()
    {
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Attack cooldown
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown)
        {
            PerformAttack();
            attackTimer = 0f;
        }
    }

    /// <summary>
    /// Execute attack
    /// </summary>
    protected virtual void PerformAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(ANIM_ATTACK);
        }

        // Deal damage to camp
        DealDamageToCamp();
    }

    /// <summary>
    /// [WIP] Deal damage to the camp
    /// </summary>
    protected virtual void DealDamageToCamp()
    {
        return;
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
        // Visualize attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Visualize target line
        if (target != null && isAlive)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}