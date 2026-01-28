using UnityEngine;

/// <summary>
/// Base class for all monster types
/// Inherit from this class to create specific monster behaviors
/// </summary>
public class MonsterBase : MonoBehaviour, IHittable
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
    public float maxHealth = 100f;
    
    [Tooltip("Current health")]
    protected float health;
    
    [Tooltip("Damage dealt to camp when reaching it")]
    public float attackDamage = 10f;
    
    [Tooltip("Damage taken from each arrow hit")]
    public float arrowDamage = 25f;
    
    [Header("Physics Settings")]
    [Tooltip("Rigidbody component for arrow attachment")]
    private Rigidbody rb;
    
    [Tooltip("Should monster fall when killed (like MovingTarget)")]
    public bool fallWhenKilled = true;
    
    [Header("Animation Settings")]
    [Tooltip("Reference to the Animator component")]
    public Animator animator;
    
    [Tooltip("Duration of spawn animation (monster won't move during this time)")]
    public float spawnDuration = 1f;
    
    [Tooltip("Time to wait before destroying monster after attack (for death animation)")]
    public float destroyDelay = 5f;
    
    [Tooltip("Time to wait before destroying monster after death (for death animation)")]
    public float deathDestroyDelay = 3f;

    [Header("Audio Settings")]
    [Tooltip("Audio source for hit sounds")]
    public AudioSource hitAudioSource;

    public float CurrentHealth => health;

    // Animation parameter names
    protected const string ANIM_SPAWN = "Spawn";
    protected const string ANIM_WALK = "Walk";
    protected const string ANIM_ATTACK = "Attack";
    protected const string ANIM_HIT = "Hit";
    protected const string ANIM_DIE = "Die";
    protected const string ANIM_IS_ALIVE = "IsAlive";
    protected const string ANIM_HEALTH_PERCENT = "HealthPercent"; // Float parameter for health percentage

    protected bool isAlive = true;
    protected bool isSpawning = true;
    protected bool hasAttacked = false;
    protected float spawnTimer = 0f;
    protected float safetyRadius = 2f; // Will be set by spawner

    private float speedMultiplier = 1f;

    public void SetSpeedMultiplier(float mul)
    {
        speedMultiplier = Mathf.Clamp(mul, 0.1f, 5f);
    }



    protected virtual void Awake()
    {
        // Get Rigidbody component
        rb = GetComponent<Rigidbody>();
        
        // If no rigidbody exists, add one for arrow attachment
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Set rigidbody to kinematic initially (monster controls its own movement)
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Initialize health
        health = maxHealth;
    }

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
        
        // Get AudioSource if not assigned
        if (hitAudioSource == null)
        {
            hitAudioSource = GetComponent<AudioSource>();
        }

        // Set initial animator states
        if (animator != null)
        {
            animator.SetBool(ANIM_IS_ALIVE, true);
            animator.SetFloat(ANIM_HEALTH_PERCENT, 1f);
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * speedMultiplier * Time.deltaTime);
        }

        // Move forward (using transform for kinematic movement)
        transform.position += direction * moveSpeed * speedMultiplier * Time.deltaTime;

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
    /// Deal damage to the camp
    /// </summary>
    protected virtual void DealDamageToCamp()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} dealt {attackDamage} damage to camp!");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} tried to attack camp but GameManager.Instance is null!");
        }
    }

    /// <summary>
    /// Implementation of IHittable interface - called when hit by arrow
    /// </summary>
    // public virtual void GetHit()
    // {
    //     if (!isAlive) return;
        
    //     TakeDamage(arrowDamage);
    // }
    public virtual void GetHit(Collider hitCollider)
    {
        if (!isAlive) return;

        float damage = arrowDamage;

        if (hitCollider.CompareTag("Head"))
        {
            damage *= 2f;
        }

        TakeDamage(damage);
    }


    /// <summary>
    /// Take damage from any source
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (!isAlive) return;

        health -= damage;
        
        // Update health percentage in animator
        float healthPercent = Mathf.Clamp01(health / maxHealth);
        if (animator != null)
        {
            animator.SetFloat(ANIM_HEALTH_PERCENT, healthPercent);
            
        }
        
        // Play hit sound
        if (hitAudioSource != null)
        {
            hitAudioSource.Play();
        }
        
        if (health <= 0)
        {
            Die();
            Debug.LogWarning($"========die=======!");
        }
        else
        {
            // Play hit animation if still alive
            PlayHitAnimation();
        }
    }

    /// <summary>
    /// Play hit animation when damaged but not killed
    /// </summary>
    protected virtual void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(ANIM_HIT);
        }
    }

    /// <summary>
    /// Called when monster dies
    /// Override for custom death behavior
    /// </summary>
    protected virtual void Die()
    {
        isAlive = false;
        
        // Update animator
        if (animator != null)
        {
            animator.SetBool(ANIM_IS_ALIVE, false);
            animator.SetBool(ANIM_WALK, false);
            animator.SetTrigger(ANIM_DIE);
            
        }

        // If fallWhenKilled is enabled, switch rigidbody to dynamic mode
        // This allows the monster (with attached arrows) to fall naturally
        if (fallWhenKilled && rb != null)
        {
            rb.isKinematic = false;
            // Optional: add a small upward force for dramatic effect
            // rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }

        // Disable collider to prevent further interactions
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Destroy after death animation
        Destroy(gameObject, deathDestroyDelay);
    }

    /// <summary>
    /// Handle collision with arrows and other objects
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // Only play collision sound if not kinematic (i.e., after death and falling)
        // and not colliding with arrows (arrows have their own impact sounds)
        if (!rb.isKinematic && !collision.gameObject.CompareTag("Arrow"))
        {
            if (hitAudioSource != null)
            {
                hitAudioSource.Play();
            }
        }
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
        
        // Visualize health bar above monster
        if (Application.isPlaying && isAlive)
        {
            Vector3 healthBarPos = transform.position + Vector3.up * 2f;
            float healthPercent = health / maxHealth;
            
            // Background (red)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f, healthBarPos + Vector3.right * 0.5f);
            
            // Foreground (green)
            Gizmos.color = Color.green;
            Vector3 healthBarEnd = healthBarPos - Vector3.right * 0.5f + Vector3.right * healthPercent;
            Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f, healthBarEnd);
        }
    }
}