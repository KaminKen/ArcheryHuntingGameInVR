using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject player;
    public GameObject deer;
    public AudioSource backgroundMusic;

    [Header("Animation Settings")]
    [Range(1f, 10f)] public float textHeight = 2.5f;
    [Range(1f, 20f)] public float turnSpeed = 5.0f;

    [Header("UI Size and Font Customization")]
    public Vector2 worldBubbleSize = new Vector2(600, 200);
    public Vector2 screenBubbleSize = new Vector2(900, 150);
    [Range(10, 100)] public int worldFontSize = 40;
    [Range(10, 100)] public int screenFontSize = 24;

    [Header("Display Time Settings")]
    public float timePerChar = 0.1f;
    public float minDisplayTime = 2.0f;
    public float maxDisplayTime = 5.0f;

    [Header("Auto-Return Settings")]
    [Tooltip("Time in seconds before automatically returning to opening scene")]
    [Range(1f, 30f)] public float autoReturnDelay = 5.0f;

    [Header("VR Canvas Distance Settings")]
    [Tooltip("Distance from camera for UI elements in meters")]
    [Range(1f, 5f)] public float uiDistance = 2.5f;
    [Tooltip("Distance from camera for black fade screen in meters")]
    [Range(0.1f, 2f)] public float fadeScreenDistance = 0.5f;

    [Header("Testing Controls (Inspector Only)")]
    [Tooltip("Check this box in Inspector to test SUCCESS ending")]
    public bool testSuccessEnding = false;
    [Tooltip("Check this box in Inspector to test FAILURE ending")]
    public bool testFailureEnding = false;
    [Tooltip("Enable this to override the static GameSuccess value with test settings")]
    public bool useTestMode = false;

    // UI Components
    private GameObject blackScreen;
    private GameObject worldCanvas;
    private GameObject screenCanvas;
    private GameObject countdownCanvas;
    private Text worldText;
    private Text screenText;
    private Text youDiedText;
    private Text countdownText;
    
    // State management
    private bool isRunning = false;
    private bool isSuccess = false;
    
    // Rotation target
    private Transform currentLookTarget;

    // Static variable to receive ending data from previous scene
    public static bool GameSuccess { get; set; } = false;

    void Awake()
    {
        CreateUIElements();
        
        // Use test mode if enabled, otherwise use static GameSuccess value
        if (useTestMode)
        {
            if (testSuccessEnding)
            {
                isSuccess = true;
                Debug.Log("[EndingManager] TEST MODE: Playing SUCCESS ending");
            }
            else if (testFailureEnding)
            {
                isSuccess = false;
                Debug.Log("[EndingManager] TEST MODE: Playing FAILURE ending");
            }
            else
            {
                Debug.LogWarning("[EndingManager] TEST MODE enabled but no ending selected. Defaulting to SUCCESS.");
                isSuccess = true;
            }
        }
        else
        {
            isSuccess = GameSuccess;
            Debug.Log($"[EndingManager] Playing {(isSuccess ? "SUCCESS" : "FAILURE")} ending");
        }
    }

    void OnValidate()
    {
        // Ensure only one test option is selected at a time
        if (testSuccessEnding && testFailureEnding)
        {
            testFailureEnding = false;
        }

        // Auto-enable test mode when either test option is checked
        if (testSuccessEnding || testFailureEnding)
        {
            useTestMode = true;
        }
    }

    void Start()
    {
        // Automatically start the ending cutscene
        StartCoroutine(PlayEndingSequence());
    }

    void Update()
    {
        // Update UI positions to always face camera
        UpdateUIPositions();

        // Smoothly rotate deer to look at target
        if (currentLookTarget != null && deer != null && deer.activeSelf)
        {
            Vector3 direction = currentLookTarget.position - deer.transform.position;
            direction.y = 0; // Only horizontal rotation

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                deer.transform.rotation = Quaternion.Slerp(
                    deer.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * turnSpeed
                );
            }
        }
    }

    void UpdateUIPositions()
    {
        // Update all screen-space UI elements to face the camera and maintain proper distance
        if (Camera.main == null) return;

        Transform camTransform = Camera.main.transform;
        
        // Update black screen position (very close to camera)
        if (blackScreen != null && blackScreen.activeSelf)
        {
            blackScreen.transform.position = camTransform.position + camTransform.forward * fadeScreenDistance;
            blackScreen.transform.rotation = Quaternion.LookRotation(blackScreen.transform.position - camTransform.position);
        }

        // Update "YOU DIED" text position
        if (countdownCanvas != null && countdownCanvas.activeSelf)
        {
            countdownCanvas.transform.position = camTransform.position + camTransform.forward * uiDistance;
            countdownCanvas.transform.rotation = Quaternion.LookRotation(countdownCanvas.transform.position - camTransform.position);
        }

        // Update screen subtitle position (player dialogue)
        if (screenCanvas != null && screenCanvas.activeSelf)
        {
            screenCanvas.transform.position = camTransform.position + camTransform.forward * uiDistance;
            screenCanvas.transform.rotation = Quaternion.LookRotation(screenCanvas.transform.position - camTransform.position);
        }
    }

    void CreateUIElements()
    {
        // Get default font
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Create black screen for fade effects (WorldSpace Canvas for VR)
        CreateBlackScreen(defaultFont);

        // Create countdown canvas with "YOU DIED" or countdown text (WorldSpace for VR)
        CreateCountdownCanvas(defaultFont);

        // Create screen subtitle for player dialogue (WorldSpace for VR)
        CreateScreenSubtitleCanvas(defaultFont);

        // Create world space subtitle for deer dialogue
        CreateWorldSubtitleCanvas(defaultFont);
    }

    void CreateBlackScreen(Font font)
    {
        // Create black screen as WorldSpace canvas for VR compatibility
        blackScreen = new GameObject("BlackFadeCanvas");
        Canvas canvas = blackScreen.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Set canvas size and scale for VR
        RectTransform canvasRect = blackScreen.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 100); // Large enough to cover view
        blackScreen.transform.localScale = Vector3.one * 0.02f; // Scale to appropriate size in world space

        // Create black image
        GameObject blackObj = new GameObject("BlackImage");
        blackObj.transform.SetParent(blackScreen.transform, false);
        Image blackImg = blackObj.AddComponent<Image>();
        blackImg.color = new Color(0, 0, 0, 1); // Start with black
        blackImg.raycastTarget = false;
        SetStretch(blackObj.GetComponent<RectTransform>());

        Debug.Log("[EndingManager] Created WorldSpace black screen for VR");
    }

    void CreateCountdownCanvas(Font font)
    {
        // Create countdown canvas with "YOU DIED" text and countdown (WorldSpace for VR)
        countdownCanvas = new GameObject("CountdownCanvas");
        Canvas canvas = countdownCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        
        // Set canvas size and scale
        RectTransform canvasRect = countdownCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(800, 300);
        countdownCanvas.transform.localScale = Vector3.one * 0.002f; // Smaller scale for text readability

        // Create "YOU DIED" text
        GameObject youDiedObj = new GameObject("YouDiedText");
        youDiedObj.transform.SetParent(countdownCanvas.transform, false);
        youDiedText = youDiedObj.AddComponent<Text>();
        youDiedText.font = font;
        youDiedText.text = "YOU DIED";
        youDiedText.fontSize = 80;
        youDiedText.alignment = TextAnchor.MiddleCenter;
        youDiedText.color = new Color(0.8f, 0.1f, 0.1f, 1f); // Dark red
        youDiedText.fontStyle = FontStyle.Bold;
        
        RectTransform youDiedRect = youDiedText.GetComponent<RectTransform>();
        youDiedRect.anchorMin = new Vector2(0.5f, 0.5f);
        youDiedRect.anchorMax = new Vector2(0.5f, 0.5f);
        youDiedRect.sizeDelta = new Vector2(800, 150);
        youDiedRect.anchoredPosition = new Vector2(0, 30); // Slightly above center

        // Create countdown text
        GameObject countdownObj = new GameObject("CountdownText");
        countdownObj.transform.SetParent(countdownCanvas.transform, false);
        countdownText = countdownObj.AddComponent<Text>();
        countdownText.font = font;
        countdownText.text = "Returning in 5...";
        countdownText.fontSize = 32;
        countdownText.alignment = TextAnchor.MiddleCenter;
        countdownText.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray
        countdownText.fontStyle = FontStyle.Normal;
        
        RectTransform countdownRect = countdownText.GetComponent<RectTransform>();
        countdownRect.anchorMin = new Vector2(0.5f, 0.5f);
        countdownRect.anchorMax = new Vector2(0.5f, 0.5f);
        countdownRect.sizeDelta = new Vector2(600, 80);
        countdownRect.anchoredPosition = new Vector2(0, -80); // Below "YOU DIED"
        
        countdownCanvas.SetActive(false);

        Debug.Log("[EndingManager] Created WorldSpace countdown canvas for VR");
    }

    void CreateScreenSubtitleCanvas(Font font)
    {
        // Create screen subtitle as WorldSpace canvas for VR (player dialogue)
        screenCanvas = new GameObject("PlayerSubtitleCanvas");
        Canvas canvas = screenCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50;
        
        // Set canvas size and scale
        RectTransform canvasRect = screenCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = screenBubbleSize;
        screenCanvas.transform.localScale = Vector3.one * 0.002f;

        // Create background
        Image subBg = screenCanvas.AddComponent<Image>();
        subBg.color = new Color(0, 0, 0, 0.85f);
        
        // Create text
        screenText = CreateTextComponent(screenCanvas.transform, font, screenFontSize);
        
        screenCanvas.SetActive(false);

        Debug.Log("[EndingManager] Created WorldSpace screen subtitle canvas for VR");
    }

    void CreateWorldSubtitleCanvas(Font font)
    {
        // Create world space subtitle for deer dialogue
        worldCanvas = new GameObject("WorldSubtitleCanvas");
        Canvas wCanvas = worldCanvas.AddComponent<Canvas>();
        wCanvas.renderMode = RenderMode.WorldSpace;
        wCanvas.sortingOrder = 50;
        
        RectTransform wRect = worldCanvas.GetComponent<RectTransform>();
        wRect.sizeDelta = worldBubbleSize;
        worldCanvas.transform.localScale = Vector3.one * 0.01f;
        
        Image wBg = worldCanvas.AddComponent<Image>();
        wBg.color = new Color(0, 0, 0, 0.85f);
        worldText = CreateTextComponent(worldCanvas.transform, font, worldFontSize);
        worldCanvas.SetActive(false);

        Debug.Log("[EndingManager] Created WorldSpace world subtitle canvas for VR");
    }

    Text CreateTextComponent(Transform parent, Font font, int fontSize)
    {
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(parent, false);
        Text t = textObj.AddComponent<Text>();
        t.font = font;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = true;
        RectTransform rt = t.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(20, 15);
        rt.offsetMax = new Vector2(-20, -15);
        return t;
    }

    void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    IEnumerator PlayEndingSequence()
    {
        isRunning = true;

        if (isSuccess)
        {
            // Success ending sequence
            yield return StartCoroutine(PlaySuccessEnding());
        }
        else
        {
            // Failure ending sequence
            yield return StartCoroutine(PlayFailureEnding());
        }

        // Start countdown timer after ending
        yield return StartCoroutine(CountdownTimer());

        // Return to opening scene
        ReturnToOpening();
    }

    IEnumerator PlaySuccessEnding()
    {
        // Fade in (screen is already black from start)
        yield return new WaitForSeconds(1.0f);

        // Spawn deer in front of player
        if (deer != null && player != null)
        {
            deer.SetActive(true);
            deer.transform.position = player.transform.position + player.transform.forward * 3.0f;
            currentLookTarget = player.transform; // Deer looks at player
        }

        // Play background music if assigned
        if (backgroundMusic != null) backgroundMusic.Play();

        // Fade out black screen
        yield return StartCoroutine(FadeEffect(0f));

        // Wait a moment before dialogue
        yield return new WaitForSeconds(0.5f);

        // Show deer's dialogue
        yield return ShowTalk(deer, "Deer", "Impressive, hope you can have this luck next time");

        // Wait before showing countdown
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator PlayFailureEnding()
    {
        // Screen is already black
        yield return new WaitForSeconds(0.5f);

        // Show countdown canvas with "YOU DIED" text
        countdownCanvas.SetActive(true);
        youDiedText.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(false); // Hide countdown text initially
        
        // Fade in "YOU DIED" text
        float elapsed = 0f;
        float fadeDuration = 1.5f;
        Color startColor = new Color(0.8f, 0.1f, 0.1f, 0f);
        Color endColor = new Color(0.8f, 0.1f, 0.1f, 1f);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            youDiedText.color = Color.Lerp(startColor, endColor, elapsed / fadeDuration);
            yield return null;
        }

        // Display "YOU DIED" for a while
        yield return new WaitForSeconds(2.0f);

        // Fade out "YOU DIED" slightly (but keep visible)
        elapsed = 0f;
        Color midColor = new Color(0.8f, 0.1f, 0.1f, 0.7f);
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            youDiedText.color = Color.Lerp(endColor, midColor, elapsed / 0.5f);
            yield return null;
        }

        // Wait before showing countdown
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator CountdownTimer()
    {
        // Show countdown text
        countdownText.gameObject.SetActive(true);
        countdownCanvas.SetActive(true);

        // For success ending, show the countdown canvas
        if (isSuccess && youDiedText != null)
        {
            youDiedText.gameObject.SetActive(false); // Hide "YOU DIED" for success ending
        }

        float timeRemaining = autoReturnDelay;
        
        while (timeRemaining > 0)
        {
            // Update countdown text
            int seconds = Mathf.CeilToInt(timeRemaining);
            countdownText.text = $"Returning to opening in {seconds}...";
            
            yield return new WaitForSeconds(1.0f);
            timeRemaining -= 1.0f;
        }

        countdownText.text = "Returning...";
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator ShowTalk(GameObject target, string speaker, string content)
    {
        string textLine = $"<b>{speaker}: {content}</b>";
        float displayTime = Mathf.Clamp(
            content.Length * timePerChar, 
            minDisplayTime, 
            maxDisplayTime
        );

        if (target == null)
        {
            // Show as screen subtitle (for player dialogue) - now in world space facing camera
            screenCanvas.SetActive(true);
            screenText.text = textLine;
            yield return new WaitForSeconds(displayTime);
            screenCanvas.SetActive(false);
        }
        else
        {
            // Show as world space subtitle (for deer dialogue)
            worldCanvas.SetActive(true);
            worldText.text = textLine;
            float timer = 0;
            
            while (timer < displayTime)
            {
                if (target != null && Camera.main != null)
                {
                    // Position text above target
                    worldCanvas.transform.position = target.transform.position + Vector3.up * textHeight;
                    // Make text face camera
                    worldCanvas.transform.rotation = Quaternion.LookRotation(
                        worldCanvas.transform.position - Camera.main.transform.position
                    );
                }
                timer += Time.deltaTime;
                yield return null;
            }
            
            worldCanvas.SetActive(false);
        }
    }

    IEnumerator FadeEffect(float targetAlpha)
    {
        // Find the Image component in the black screen's child
        Image img = blackScreen.GetComponentInChildren<Image>();
        if (img == null)
        {
            Debug.LogError("[EndingManager] Black screen Image component not found!");
            yield break;
        }

        float startAlpha = img.color.a;
        float elapsed = 0;
        float fadeDuration = 1.2f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            img.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration));
            yield return null;
        }
    }

    void ReturnToOpening()
    {
        // Reset the game success flag
        GameSuccess = false;
        
        Debug.Log("[EndingManager] Returning to opening scene");
        
        // Load opening scene
        SceneManager.LoadScene("opening");
    }

    // Method to be called from previous scene's manager
    public static void SetEndingResult(bool success)
    {
        GameSuccess = success;
        Debug.Log($"[EndingManager] SetEndingResult called! Parameter: {success}, GameSuccess is now: {GameSuccess}");
    }
}