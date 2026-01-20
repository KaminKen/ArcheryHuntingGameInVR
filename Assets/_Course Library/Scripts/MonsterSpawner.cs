using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Camp Settings")]
    [Tooltip("The camp position that monsters will move towards")]
    public Transform campCenter;

    [Header("Spawn Area Settings")]
    [Tooltip("Outer radius of the spawn area")]
    public float spawnRadius = 10f;
    
    [Tooltip("Inner radius - monsters won't spawn closer than this distance")]
    public float minSpawnRadius = 3f;
    
    [Tooltip("Angle range for semicircle (180 = half circle, 360 = full circle)")]
    [Range(0f, 360f)]
    public float spawnAngleRange = 180f;
    
    [Tooltip("Rotation offset for the semicircle direction (0 = front, 90 = right, etc.)")]
    public float spawnAngleOffset = 0f;

    [Header("Spawn Timing")]
    [Tooltip("Time between each spawn in seconds")]
    public float spawnInterval = 3f;
    
    [Tooltip("Random variation added to spawn interval (±)")]
    public float spawnIntervalVariation = 0.5f;
    
    [Tooltip("Start spawning automatically on game start")]
    public bool autoStart = true;

    [Header("Monster Settings")]
    [Tooltip("List of monster prefabs that can be spawned")]
    public GameObject[] monsterPrefabs;
    
    [Tooltip("Spawn weights for each monster type (higher = more likely)")]
    public float[] spawnWeights;
    
    [Tooltip("Parent transform for spawned monsters (optional, for organization)")]
    public Transform monstersParent;

    [Header("Optional Limits")]
    [Tooltip("Maximum number of monsters alive at once (0 = unlimited)")]
    public int maxActiveMonsters = 0;
    
    [Tooltip("Maximum total monsters to spawn (0 = unlimited)")]
    public int maxTotalSpawns = 0;

    // Private variables
    private bool isSpawning = false;
    private int currentActiveMonsters = 0;
    private int totalSpawned = 0;
    private List<GameObject> activeMonsters = new List<GameObject>();

    void Start()
    {
        // Validate settings
        if (campCenter == null)
        {
            Debug.LogError("Camp Center is not assigned! Using spawner position as camp center.");
            campCenter = transform;
        }

        // Initialize spawn weights if not set
        if (spawnWeights == null || spawnWeights.Length != monsterPrefabs.Length)
        {
            spawnWeights = new float[monsterPrefabs.Length];
            for (int i = 0; i < spawnWeights.Length; i++)
            {
                spawnWeights[i] = 1f; // Equal weight for all
            }
        }

        // Start spawning if auto-start is enabled
        if (autoStart)
        {
            StartSpawning();
        }
    }

    /// <summary>
    /// Start the spawning process
    /// </summary>
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    /// <summary>
    /// Stop the spawning process
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Main spawn coroutine
    /// </summary>
    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            // Check if we've reached max total spawns
            if (maxTotalSpawns > 0 && totalSpawned >= maxTotalSpawns)
            {
                Debug.Log("Maximum total spawns reached. Stopping spawner.");
                StopSpawning();
                yield break;
            }

            // Check if we've reached max active monsters
            if (maxActiveMonsters > 0 && currentActiveMonsters >= maxActiveMonsters)
            {
                yield return new WaitForSeconds(0.5f); // Wait and check again
                continue;
            }

            // Spawn a monster
            SpawnMonster();

            // Wait for next spawn with variation
            float waitTime = spawnInterval + Random.Range(-spawnIntervalVariation, spawnIntervalVariation);
            waitTime = Mathf.Max(0.1f, waitTime); // Ensure minimum wait time
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Spawn a single monster
    /// </summary>
    private void SpawnMonster()
    {
        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
        {
            Debug.LogError("No monster prefabs assigned!");
            return;
        }

        // Select random monster based on weights
        GameObject selectedPrefab = SelectRandomMonster();
        if (selectedPrefab == null)
        {
            Debug.LogError("Selected monster prefab is null!");
            return;
        }

        // Calculate spawn position
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Calculate rotation to face the camp center
        Vector3 directionToCamp = (campCenter.position - spawnPosition).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(directionToCamp);

        // Instantiate monster
        GameObject monster = Instantiate(selectedPrefab, spawnPosition, spawnRotation);

        // Set parent if specified
        if (monstersParent != null)
        {
            monster.transform.SetParent(monstersParent);
        }

        // Try to pass camp center to monster (if it has a compatible script)
        MonsterBase monsterScript = monster.GetComponent<MonsterBase>();
        if (monsterScript != null)
        {
            monsterScript.SetTarget(campCenter);
        }

        // Track the monster
        activeMonsters.Add(monster);
        currentActiveMonsters++;
        totalSpawned++;

        // Listen for monster destruction
        MonsterTracker tracker = monster.AddComponent<MonsterTracker>();
        tracker.spawner = this;

        Debug.Log($"Spawned {selectedPrefab.name} at {spawnPosition}. Total spawned: {totalSpawned}");
    }

    /// <summary>
    /// Select a random monster based on spawn weights
    /// </summary>
    private GameObject SelectRandomMonster()
    {
        float totalWeight = 0f;
        foreach (float weight in spawnWeights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < monsterPrefabs.Length; i++)
        {
            currentWeight += spawnWeights[i];
            if (randomValue <= currentWeight)
            {
                return monsterPrefabs[i];
            }
        }

        return monsterPrefabs[0]; // Fallback
    }

    /// <summary>
    /// Calculate a random spawn position within the semicircle (between min and max radius)
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        // Random distance between minSpawnRadius and spawnRadius
        // Using squared distribution for better spread
        float minRadiusNormalized = minSpawnRadius / spawnRadius;
        float randomNormalized = Mathf.Sqrt(Random.Range(minRadiusNormalized * minRadiusNormalized, 1f));
        float randomDistance = randomNormalized * spawnRadius;
        
        // Random angle within the specified range
        float randomAngle = Random.Range(0f, spawnAngleRange) + spawnAngleOffset;
        
        // Convert to radians
        float angleInRadians = randomAngle * Mathf.Deg2Rad;
        
        // Calculate offset from camp center
        Vector3 offset = new Vector3(
            Mathf.Cos(angleInRadians) * randomDistance,
            0f,
            Mathf.Sin(angleInRadians) * randomDistance
        );
        
        // Return final position
        return campCenter.position + offset;
    }

    /// <summary>
    /// Called when a monster is destroyed
    /// </summary>
    public void OnMonsterDestroyed(GameObject monster)
    {
        if (activeMonsters.Contains(monster))
        {
            activeMonsters.Remove(monster);
            currentActiveMonsters--;
        }
    }

    /// <summary>
    /// Visualize spawn area in editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (campCenter == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(campCenter.position, 0.5f); // Camp center marker

        // Draw outer circle arc
        Gizmos.color = Color.red;
        int segments = 50;
        float angleStep = spawnAngleRange / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (angleStep * i + spawnAngleOffset) * Mathf.Deg2Rad;
            float angle2 = (angleStep * (i + 1) + spawnAngleOffset) * Mathf.Deg2Rad;
            
            Vector3 point1 = campCenter.position + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * spawnRadius;
            Vector3 point2 = campCenter.position + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * spawnRadius;
            
            Gizmos.DrawLine(point1, point2);
        }
        
        // Draw inner circle arc (exclusion zone)
        Gizmos.color = Color.blue;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (angleStep * i + spawnAngleOffset) * Mathf.Deg2Rad;
            float angle2 = (angleStep * (i + 1) + spawnAngleOffset) * Mathf.Deg2Rad;
            
            Vector3 point1 = campCenter.position + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * minSpawnRadius;
            Vector3 point2 = campCenter.position + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * minSpawnRadius;
            
            Gizmos.DrawLine(point1, point2);
        }
        
        // Draw radius lines
        float startAngle = spawnAngleOffset * Mathf.Deg2Rad;
        float endAngle = (spawnAngleOffset + spawnAngleRange) * Mathf.Deg2Rad;
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(campCenter.position, campCenter.position + new Vector3(Mathf.Cos(startAngle), 0, Mathf.Sin(startAngle)) * spawnRadius);
        Gizmos.DrawLine(campCenter.position, campCenter.position + new Vector3(Mathf.Cos(endAngle), 0, Mathf.Sin(endAngle)) * spawnRadius);
        
        // Draw connecting lines between inner and outer arcs
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            campCenter.position + new Vector3(Mathf.Cos(startAngle), 0, Mathf.Sin(startAngle)) * minSpawnRadius,
            campCenter.position + new Vector3(Mathf.Cos(startAngle), 0, Mathf.Sin(startAngle)) * spawnRadius
        );
        Gizmos.DrawLine(
            campCenter.position + new Vector3(Mathf.Cos(endAngle), 0, Mathf.Sin(endAngle)) * minSpawnRadius,
            campCenter.position + new Vector3(Mathf.Cos(endAngle), 0, Mathf.Sin(endAngle)) * spawnRadius
        );
    }
}

/// <summary>
/// Helper component to track when monsters are destroyed
/// </summary>
public class MonsterTracker : MonoBehaviour
{
    public MonsterSpawner spawner;

    private void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnMonsterDestroyed(gameObject);
        }
    }
}