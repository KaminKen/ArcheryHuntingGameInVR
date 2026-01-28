using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Time phase enumeration for the three stages of gameplay
/// </summary>
public enum TimePhase
{
    Day,
    Night,
    Dawn
}

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
    
    [Header("Time Phase Durations")]
    [Tooltip("Percentage of total time for Day phase (0-1)")]
    [Range(0f, 1f)]
    public float dayPhasePercent = 0.4f;
    
    [Tooltip("Percentage of total time for Night phase (0-1)")]
    [Range(0f, 1f)]
    public float nightPhasePercent = 0.3f;
    
    [Tooltip("Percentage of total time for Dawn phase (0-1)")]
    [Range(0f, 1f)]
    public float dawnPhasePercent = 0.3f;
    
    [Header("Current Phase Info (Read Only)")]
    [Tooltip("Current time phase")]
    public TimePhase currentPhase = TimePhase.Day;
    
    [Tooltip("Time elapsed in current phase")]
    public float currentPhaseTime = 0f;
    
    [Header("Game State")]
    [Tooltip("Is the game currently active")]
    private bool isGameActive = true;
    
    [Header("End Game Events")]
    [Tooltip("Called when player wins (survives the duration)")]
    public UnityEngine.Events.UnityEvent onWin;
    
    [Tooltip("Called when player loses (camp destroyed)")]
    public UnityEngine.Events.UnityEvent onLose;

    [SerializeField] private MonsterSpawner spawner;

    [Header("Spawner Difficulty By Phase")]
    [SerializeField] private float daySpawnInterval = 3f;
    [SerializeField] private float nightSpawnInterval = 2f;
    [SerializeField] private float dawnSpawnInterval = 1.2f;

    [SerializeField] private float daySpawnAngle = 180f;
    [SerializeField] private float nightSpawnAngle = 180f;
    [SerializeField] private float dawnSpawnAngle = 360f;

    [SerializeField] private int dayMaxActive = 0;    // 0 = unlimited (keep as-is)
    [SerializeField] private int nightMaxActive = 0;
    [SerializeField] private int dawnMaxActive = 0;

    [Header("Skeleton Rush (Night/Dawn)")]
    [SerializeField] private bool enableSkeletonRushAtNight = true;
    [SerializeField] private bool enableSkeletonRushAtDawn = true;

    // speed multiplier range for “some skeleton is rushing”
    [SerializeField] private Vector2 nightSkeletonSpeedMulRange = new Vector2(1.0f, 1.8f);
    [SerializeField] private Vector2 dawnSkeletonSpeedMulRange  = new Vector2(1.2f, 2.2f);



    // Singleton instance for easy access from monsters
    public static GameManager Instance { get; private set; }
    
    // Phase change tracking
    private TimePhase lastPhase;

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
        
        // Normalize phase percentages
        NormalizePhasePercentages();
    }

    private void Start()
    {
        Debug.Log($"Camp initialized with {maxHealth} health. Survive for {survivalTime} seconds to win!");
        Debug.Log($"Phase durations - Day: {GetPhaseDuration(TimePhase.Day)}s, Night: {GetPhaseDuration(TimePhase.Night)}s, Dawn: {GetPhaseDuration(TimePhase.Dawn)}s");
        
        lastPhase = currentPhase;
        OnPhaseChanged(currentPhase);
    }

    private void Update()
    {
        if (!isGameActive) return;

        // Update survival timer
        currentSurvivalTime += Time.deltaTime;

        // Update current phase
        UpdateCurrentPhase();
        
        // Check for phase change
        if (currentPhase != lastPhase)
        {
            OnPhaseChanged(currentPhase);
            lastPhase = currentPhase;
        }

        // Check win condition
        if (currentSurvivalTime >= survivalTime)
        {
            Win();
        }
    }

    /// <summary>
    /// Normalize phase percentages to ensure they sum to 1.0
    /// </summary>
    private void NormalizePhasePercentages()
    {
        float total = dayPhasePercent + nightPhasePercent + dawnPhasePercent;
        if (total > 0)
        {
            dayPhasePercent /= total;
            nightPhasePercent /= total;
            dawnPhasePercent /= total;
        }
        else
        {
            // Default values if all are zero
            dayPhasePercent = 0.4f;
            nightPhasePercent = 0.3f;
            dawnPhasePercent = 0.3f;
        }
    }

    /// <summary>
    /// Update the current time phase based on elapsed time
    /// </summary>
    private void UpdateCurrentPhase()
    {
        float dayDuration = survivalTime * dayPhasePercent;
        float nightDuration = survivalTime * nightPhasePercent;
        float dawnDuration = survivalTime * dawnPhasePercent;
        
        if (currentSurvivalTime < dayDuration)
        {
            currentPhase = TimePhase.Day;
            currentPhaseTime = currentSurvivalTime;
        }
        else if (currentSurvivalTime < dayDuration + nightDuration)
        {
            currentPhase = TimePhase.Night;
            currentPhaseTime = currentSurvivalTime - dayDuration;
        }
        else
        {
            currentPhase = TimePhase.Dawn;
            currentPhaseTime = currentSurvivalTime - dayDuration - nightDuration;
        }
    }

    /// <summary>
    /// Called when the time phase changes
    /// </summary>
    private void OnPhaseChanged(TimePhase newPhase)
    {
        Debug.Log($"Time phase changed to: {newPhase}");
        
        // Interface for monster spawning system
        switch (newPhase)
        {
            case TimePhase.Day:
                OnDayPhaseStarted();
                break;
            case TimePhase.Night:
                OnNightPhaseStarted();
                break;
            case TimePhase.Dawn:
                OnDawnPhaseStarted();
                break;
        }
    }

    private void OnDayPhaseStarted()
    {
        if (spawner == null) return;

        spawner.spawnInterval = daySpawnInterval;
        spawner.spawnAngleRange = daySpawnAngle;
        spawner.maxActiveMonsters = dayMaxActive;

        spawner.enableSkeletonRush = false; // keep as-is
    }

    private void OnNightPhaseStarted()
    {
        if (spawner == null) return;

        spawner.spawnInterval = nightSpawnInterval;
        spawner.spawnAngleRange = nightSpawnAngle;
        spawner.maxActiveMonsters = nightMaxActive;

        spawner.enableSkeletonRush = enableSkeletonRushAtNight;
        spawner.skeletonSpeedMulMin = nightSkeletonSpeedMulRange.x;
        spawner.skeletonSpeedMulMax = nightSkeletonSpeedMulRange.y;
    }

    private void OnDawnPhaseStarted()
    {
        if (spawner == null) return;

        spawner.spawnInterval = dawnSpawnInterval;
        spawner.spawnAngleRange = dawnSpawnAngle; // 360° final phase
        spawner.maxActiveMonsters = dawnMaxActive;

        spawner.enableSkeletonRush = enableSkeletonRushAtDawn;
        spawner.skeletonSpeedMulMin = dawnSkeletonSpeedMulRange.x;
        spawner.skeletonSpeedMulMax = dawnSkeletonSpeedMulRange.y;
    }


    /// <summary>
    /// Get the duration of a specific phase in seconds
    /// </summary>
    public float GetPhaseDuration(TimePhase phase)
    {
        switch (phase)
        {
            case TimePhase.Day:
                return survivalTime * dayPhasePercent;
            case TimePhase.Night:
                return survivalTime * nightPhasePercent;
            case TimePhase.Dawn:
                return survivalTime * dawnPhasePercent;
            default:
                return 0f;
        }
    }

    /// <summary>
    /// Get the current time phase
    /// </summary>
    public TimePhase GetCurrentPhase()
    {
        return currentPhase;
    }

    /// <summary>
    /// Get normalized time progress (0-1) within the entire survival duration
    /// </summary>
    public float GetNormalizedTime()
    {
        return Mathf.Clamp01(currentSurvivalTime / survivalTime);
    }

    /// <summary>
    /// Get normalized time progress (0-1) within the current phase
    /// </summary>
    public float GetNormalizedPhaseTime()
    {
        float phaseDuration = GetPhaseDuration(currentPhase);
        if (phaseDuration <= 0) return 0f;
        return Mathf.Clamp01(currentPhaseTime / phaseDuration);
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

        // Set ending result and load ending scene
        EndingManager.SetEndingResult(true);
        SceneManager.LoadScene("ending");
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

        // Set ending result and load ending scene
        EndingManager.SetEndingResult(false);
        SceneManager.LoadScene("ending");
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