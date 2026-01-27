using UnityEngine;

public class DayNightCyclePlus : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Reference to GameManager for survival time")]
    public GameManager gameManager;
    
    [Tooltip("Current normalized time (0-1) across all phases")]
    [Range(0, 1)]
    public float currentTime = 0f;
    
    [Header("Phase Transition Settings")]
    [Tooltip("Time in seconds for transitioning between phases")]
    public float phaseTransitionTime = 30f;

    [Header("Current Time Display (Read Only)")]
    public string timeDisplay;
    public string phaseDisplay;

    [Header("Light Sources and Visuals")]
    public Light sunLight;
    public MeshRenderer sunVisual; // Sun sphere
    public Light moonLight;
    public MeshRenderer moonVisual; // Moon sphere

    [Tooltip("Distance at which sun and moon are positioned")]
    public float orbitDistance = 100f;
    [Tooltip("Size of sun and moon spheres")]
    public float bodySize = 5f;

    [Header("Environment Settings")]
    public Material skyboxMaterial;
    public bool useFog = true;

    [Header("Dawn Fog Settings")]
    [Tooltip("Enable volumetric fog effect during dawn phase")]
    public bool useDawnFog = true;
    
    [Tooltip("Maximum fog density during dawn (blocks distant view)")]
    [Range(0f, 1f)]
    public float dawnFogDensity = 0.15f;
    
    [Tooltip("Color of the dawn fog")]
    public Color dawnFogColor = new Color(0.7f, 0.7f, 0.8f, 1f);

    [Header("Automatic Gradients")]
    public Gradient sunColor;
    public Gradient moonColor;
    public Gradient skyboxTint;
    public Gradient fogColor;

    // Phase-specific celestial angles (fixed positions for each phase)
    private float dayAngle = 180f;      // Sun at zenith (overhead)
    private float nightAngle = 270f;   // Moon at zenith, sun below horizon
    private float dawnAngle = 0f;      // Horizon position

    // Current angle and target angle for smooth transitions
    private float currentAngle = 90f;
    private float targetAngle = 90f;
    
    // Transition state tracking
    private bool isTransitioning = false;
    private float transitionStartAngle = 0f;
    private float transitionProgress = 0f;
    
    // Phase time tracking
    private float dayStartTime = 0f;
    private float dayEndTime = 0.4f;
    private float nightStartTime = 0.4f;
    private float nightEndTime = 0.7f;
    private float dawnStartTime = 0.7f;
    private float dawnEndTime = 1f;
    
    private TimePhase lastPhase = TimePhase.Day;

    void Start()
    {
        // Auto-find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        SetupGradients();
        if (useFog) RenderSettings.fog = true;

        // Initialize visual objects (scale adjustment, etc.)
        if (sunVisual != null) sunVisual.transform.localScale = Vector3.one * bodySize;
        if (moonVisual != null) moonVisual.transform.localScale = Vector3.one * bodySize;
        
        // Calculate phase time ranges based on GameManager settings
        if (gameManager != null)
        {
            UpdatePhaseTimeRanges();
        }
        
        // Initialize at day position
        currentAngle = dayAngle;
        targetAngle = dayAngle;
    }

    void Update()
    {
        if (gameManager == null || !gameManager.IsGameActive()) return;

        // 1. Update time progression based on GameManager
        UpdateTimeProgression();

        // 2. Display time in Inspector
        UpdateInspectorTime();

        // 3. Handle phase transitions and celestial movement
        UpdateCelestialTransition();

        // 4. Update rotation and position of sun and moon
        UpdateCelestialBodies();

        // 5. Adjust intensity and color
        UpdateLighting();
        
        // 6. Update dawn fog if enabled
        UpdateDawnFog();
    }

    /// <summary>
    /// Calculate phase time ranges based on GameManager phase percentages
    /// </summary>
    void UpdatePhaseTimeRanges()
    {
        if (gameManager == null) return;

        dayStartTime = 0f;
        dayEndTime = gameManager.dayPhasePercent;
        
        nightStartTime = dayEndTime;
        nightEndTime = nightStartTime + gameManager.nightPhasePercent;
        
        dawnStartTime = nightEndTime;
        dawnEndTime = 1f;
    }

    /// <summary>
    /// Update time progression based on GameManager's survival timer
    /// </summary>
    void UpdateTimeProgression()
    {
        if (gameManager == null) return;

        // Get normalized time from GameManager (0-1)
        currentTime = gameManager.GetNormalizedTime();
        
        // Clamp to prevent overflow
        currentTime = Mathf.Clamp01(currentTime);
    }

    /// <summary>
    /// Handle phase transitions and calculate when to move celestial bodies
    /// </summary>
    void UpdateCelestialTransition()
    {
        if (gameManager == null) return;

        TimePhase currentPhase = gameManager.GetCurrentPhase();
        float phaseDuration = gameManager.GetPhaseDuration(currentPhase);
        float phaseTime = gameManager.currentPhaseTime;
        
        // Determine target angle based on current phase
        float newTargetAngle = GetPhaseTargetAngle(currentPhase);
        
        // Check if phase changed
        if (currentPhase != lastPhase)
        {
            // Phase just changed, start transition
            StartTransition(currentAngle, newTargetAngle);
            lastPhase = currentPhase;
        }
        
        // Calculate when transition should start
        float effectiveTransitionTime = Mathf.Min(phaseTransitionTime, phaseDuration);
        float transitionStartTime = phaseDuration - effectiveTransitionTime;
        
        // Check if we should be transitioning
        if (phaseTime >= transitionStartTime && !isTransitioning)
        {
            // Start transition to next phase position
            TimePhase nextPhase = GetNextPhase(currentPhase);
            float nextTargetAngle = GetPhaseTargetAngle(nextPhase);
            StartTransition(currentAngle, nextTargetAngle);
        }
        
        // Update transition progress
        if (isTransitioning)
        {
            float timeIntoTransition = phaseTime - transitionStartTime;
            float effectiveDuration = Mathf.Max(0.1f, effectiveTransitionTime); // Prevent division by zero
            transitionProgress = Mathf.Clamp01(timeIntoTransition / effectiveDuration);
            
            // Smooth interpolation
            float smoothProgress = Mathf.SmoothStep(0f, 1f, transitionProgress);
            currentAngle = Mathf.LerpAngle(transitionStartAngle, targetAngle, smoothProgress);
            
            Debug.Log($"Transitioning: {transitionProgress:F2} - Angle: {currentAngle:F1}° (from {transitionStartAngle:F1}° to {targetAngle:F1}°)");
            
            // End transition when complete
            if (transitionProgress >= 1f)
            {
                isTransitioning = false;
                currentAngle = targetAngle;
            }
        }
    }

    /// <summary>
    /// Start a new transition
    /// </summary>
    void StartTransition(float fromAngle, float toAngle)
    {
        isTransitioning = true;
        transitionStartAngle = fromAngle;
        targetAngle = toAngle;
        transitionProgress = 0f;
        
        Debug.Log($"Starting transition from {fromAngle:F1}° to {toAngle:F1}°");
    }

    /// <summary>
    /// Get the target angle for a specific phase
    /// </summary>
    float GetPhaseTargetAngle(TimePhase phase)
    {
        switch (phase)
        {
            case TimePhase.Day:
                return dayAngle;
            case TimePhase.Night:
                return nightAngle;
            case TimePhase.Dawn:
                return dawnAngle;
            default:
                return dayAngle;
        }
    }

    /// <summary>
    /// Get the next phase in sequence
    /// </summary>
    TimePhase GetNextPhase(TimePhase currentPhase)
    {
        switch (currentPhase)
        {
            case TimePhase.Day:
                return TimePhase.Night;
            case TimePhase.Night:
                return TimePhase.Dawn;
            case TimePhase.Dawn:
                return TimePhase.Day; // Loop back to day
            default:
                return TimePhase.Day;
        }
    }

    void UpdateCelestialBodies()
    {
        // Use current angle for celestial rotation
        float sunAngle = currentAngle;

        // Rotation of lights
        sunLight.transform.rotation = Quaternion.Euler(sunAngle - 90f, -90f, 0f);
        moonLight.transform.rotation = Quaternion.Euler(sunAngle + 90f, -90f, 0f);

        // Position visuals (spheres): place in sky opposite to light direction
        if (sunVisual != null)
        {
            sunVisual.transform.position = transform.position - (sunLight.transform.forward * orbitDistance);
            sunVisual.transform.LookAt(transform.position); // Always face the center
        }

        if (moonVisual != null)
        {
            moonVisual.transform.position = transform.position - (moonLight.transform.forward * orbitDistance);
            moonVisual.transform.LookAt(transform.position);
        }
    }

    void UpdateInspectorTime()
    {
        float hours = currentTime * 24f;
        int hh = Mathf.FloorToInt(hours);
        int mm = Mathf.FloorToInt((hours - hh) * 60f);
        timeDisplay = string.Format("{0:00}:{1:00}", hh, mm);
        
        // Display current phase
        if (gameManager != null)
        {
            TimePhase phase = gameManager.GetCurrentPhase();
            phaseDisplay = phase.ToString();
            
            if (isTransitioning)
            {
                phaseDisplay += $" (Transitioning {transitionProgress * 100f:F0}%)";
            }
        }
    }

    void UpdateLighting()
    {
        float sunIntensity = Mathf.Clamp01(Vector3.Dot(sunLight.transform.forward, Vector3.down));
        float moonIntensity = Mathf.Clamp01(Vector3.Dot(moonLight.transform.forward, Vector3.down));

        // Sun/moon color and intensity - use smooth interpolation during transitions
        Color currentSunColor = sunColor.Evaluate(currentTime);
        Color currentMoonColor = moonColor.Evaluate(currentTime);

        sunLight.color = currentSunColor;
        sunLight.intensity = sunIntensity * 1.2f;

        moonLight.color = currentMoonColor;
        moonLight.intensity = moonIntensity * 0.5f;

        // Additional: set sphere (Visual) emission color to match lights (using Emission)
        if (sunVisual != null) sunVisual.material.SetColor("_EmissionColor", currentSunColor * 2f);
        if (moonVisual != null) moonVisual.material.SetColor("_EmissionColor", currentMoonColor * 2f);

        // Skybox and fog
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetColor("_Tint", skyboxTint.Evaluate(currentTime));
            skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(0.2f, 1.0f, sunIntensity));
        }

        if (useFog)
        {
            // Use dawn fog color during dawn phase, otherwise use gradient
            Color targetFogColor = (gameManager != null && gameManager.GetCurrentPhase() == TimePhase.Dawn && useDawnFog) 
                ? dawnFogColor 
                : fogColor.Evaluate(currentTime);
            
            RenderSettings.fogColor = targetFogColor;
            
            // Normal fog density (will be overridden by dawn fog if active)
            if (!useDawnFog || gameManager == null || gameManager.GetCurrentPhase() != TimePhase.Dawn)
            {
                RenderSettings.fogDensity = Mathf.Lerp(0.05f, 0.02f, sunIntensity);
            }
        }

        RenderSettings.ambientLight = fogColor.Evaluate(currentTime) * 0.5f;
    }

    /// <summary>
    /// Update special fog effects during dawn phase to obscure distant view
    /// </summary>
    void UpdateDawnFog()
    {
        if (!useFog || !useDawnFog || gameManager == null) return;

        TimePhase currentPhase = gameManager.GetCurrentPhase();
        
        // Always use ExponentialSquared fog mode for smooth, natural fog
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        
        if (currentPhase == TimePhase.Dawn)
        {
            // Set higher fog density during dawn
            float dawnProgress = gameManager.GetNormalizedPhaseTime();
            
            // Fade fog in at start of dawn, fade out at end
            float fogIntensity;
            if (dawnProgress < 0.2f)
            {
                // Fade in during first 20% of dawn
                fogIntensity = Mathf.Lerp(0f, 1f, dawnProgress / 0.2f);
            }
            else if (dawnProgress > 0.8f)
            {
                // Fade out during last 20% of dawn
                fogIntensity = Mathf.Lerp(1f, 0f, (dawnProgress - 0.8f) / 0.2f);
            }
            else
            {
                // Full fog during middle 60% of dawn
                fogIntensity = 1f;
            }
            
            // Apply fog density - higher values = denser fog
            RenderSettings.fogDensity = dawnFogDensity * fogIntensity;
        }
        else
        {
            // For other phases, use lighter fog density
            // This will be further adjusted in UpdateLighting()
            RenderSettings.fogDensity = 0.02f;
        }
    }

    // (SetupGradients and CreateGradient functions are identical to the original, omitted or kept for maintenance)
    void SetupGradients()
    {
        // Adjusted gradients to better match the three distinct phases
        // Day phase (0-0.4): bright daylight
        // Night phase (0.4-0.7): dark night with moon
        // Dawn phase (0.7-1.0): foggy transition
        
        sunColor = CreateGradient(
            new Color(1f, 1f, 0.9f),      // Day: bright white-yellow
            new Color(1f, 0.4f, 0.2f),    // Night: dim orange (sunset)
            new Color(1f, 0.6f, 0.4f),    // Dawn: warm orange
            new Color(1f, 1f, 0.9f)       // Back to day
        );
        
        moonColor = CreateGradient(
            Color.black,                   // Day: moon not visible
            new Color(0.7f, 0.8f, 1f),    // Night: bright moon
            new Color(0.3f, 0.4f, 0.5f),  // Dawn: fading moon
            Color.black                    // Back to day
        );
        
        skyboxTint = CreateGradient(
            new Color(0.5f, 0.7f, 1f),    // Day: bright blue sky
            new Color(0.02f, 0.02f, 0.1f), // Night: dark blue-black
            new Color(0.6f, 0.5f, 0.5f),  // Dawn: grey-brown (foggy)
            new Color(0.5f, 0.7f, 1f)     // Back to day
        );
        
        fogColor = CreateGradient(
            new Color(0.7f, 0.8f, 1f),    // Day: light blue
            new Color(0.01f, 0.02f, 0.05f), // Night: very dark
            new Color(0.7f, 0.7f, 0.8f),  // Dawn: grey fog color
            new Color(0.7f, 0.8f, 1f)     // Back to day
        );
    }

    Gradient CreateGradient(Color phase1, Color phase2, Color phase3, Color phase4)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(phase1, 0.0f),   // Start of Day
                new GradientColorKey(phase1, 0.4f),   // End of Day
                new GradientColorKey(phase2, 0.55f),  // Middle of Night
                new GradientColorKey(phase2, 0.7f),   // End of Night
                new GradientColorKey(phase3, 0.85f),  // Middle of Dawn
                new GradientColorKey(phase4, 1.0f)    // End of Dawn (loop to Day)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f), 
                new GradientAlphaKey(1f, 1f) 
            }
        );
        return g;
    }
}