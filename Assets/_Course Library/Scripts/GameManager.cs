using UnityEngine;

/// <summary>
/// Manages the camp health and win/lose conditions
/// Place this script on the camp center object in your scene
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Camp Health Settings")]
    [Tooltip("Maximum health of the camp")]
    public float maxHealth = 100f;
    
    [Tooltip("Current health of the camp")]
    private float currentHealth;
    
    [Header("Win/Lose Conditions")]
    [Tooltip("Time in seconds the player needs to survive to win")]
    public float survivalTime = 300f; // 5 minutes default
    
    [Tooltip("Current survival timer")]
    private float currentSurvivalTime = 0f;
    
    [Header("Game State")]
    [Tooltip("Is the game currently active")]
    private bool isGameActive = true;
    
    [Header("End Game Events")]
    [Tooltip("Called when player wins (survives the duration)")]
    public UnityEngine.Events.UnityEvent onWin;
    
    [Tooltip("Called when player loses (camp destroyed)")]
    public UnityEngine.Events.UnityEvent onLose;

    // Singleton instance for easy access from monsters
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Setup singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Initialize health
        currentHealth = maxHealth;
    }

    private void Start()
    {
        Debug.Log($"Camp initialized with {maxHealth} health. Survive for {survivalTime} seconds to win!");
    }

    private void Update()
    {
        if (!isGameActive) return;

        // Update survival timer
        currentSurvivalTime += Time.deltaTime;

        // Check win condition
        if (currentSurvivalTime >= survivalTime)
        {
            Win();
        }
    }

    /// <summary>
    /// Called by monsters when they attack the camp
    /// </summary>
    /// <param name="damage">Amount of damage to deal to the camp</param>
    public void TakeDamage(float damage)
    {
        if (!isGameActive) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"Camp took {damage} damage! Current health: {currentHealth}/{maxHealth}");

        // Check lose condition
        if (currentHealth <= 0)
        {
            Lose();
        }
    }

    /// <summary>
    /// Called when player successfully survives the required time
    /// </summary>
    private void Win()
    {
        if (!isGameActive) return;

        isGameActive = false;
        Debug.Log("Victory! Camp survived!");

        // Invoke win event
        onWin?.Invoke();
    }

    /// <summary>
    /// Called when camp health reaches zero
    /// </summary>
    private void Lose()
    {
        if (!isGameActive) return;

        isGameActive = false;
        Debug.Log("Defeat! Camp was destroyed!");

        // Invoke lose event
        onLose?.Invoke();
    }

    /// <summary>
    /// Get current health of the camp
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Get health percentage (0-1)
    /// </summary>
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// Get remaining survival time
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0, survivalTime - currentSurvivalTime);
    }

    /// <summary>
    /// Get current survival time
    /// </summary>
    public float GetCurrentSurvivalTime()
    {
        return currentSurvivalTime;
    }

    /// <summary>
    /// Check if game is still active
    /// </summary>
    public bool IsGameActive()
    {
        return isGameActive;
    }

    private void OnDrawGizmos()
    {
        // Visualize camp center
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw health indicator
        if (Application.isPlaying)
        {
            float healthPercent = currentHealth / maxHealth;
            Gizmos.color = Color.Lerp(Color.red, Color.green, healthPercent);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}