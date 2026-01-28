using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

    [Header("Button Settings")]
    public Vector2 buttonSize = new Vector2(300, 80);
    public int buttonFontSize = 28;
    public Color buttonColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    public Color buttonHoverColor = new Color(0.3f, 0.7f, 1f, 1f);

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
    private GameObject buttonCanvas;
    private GameObject youDiedCanvas;
    private Text worldText;
    private Text screenText;
    private Text youDiedText;
    private Button returnButton;
    
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
            // If both are checked, uncheck the other one based on which was just checked
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

        // VR Controller button input to return to opening scene
        // This works when button canvas is active (ending sequence finished)
        if (buttonCanvas != null && buttonCanvas.activeSelf)
        {
            CheckVRControllerInput();
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
        if (youDiedCanvas != null && youDiedCanvas.activeSelf)
        {
            youDiedCanvas.transform.position = camTransform.position + camTransform.forward * uiDistance;
            youDiedCanvas.transform.rotation = Quaternion.LookRotation(youDiedCanvas.transform.position - camTransform.position);
        }

        // Update screen subtitle position (player dialogue)
        if (screenCanvas != null && screenCanvas.activeSelf)
        {
            screenCanvas.transform.position = camTransform.position + camTransform.forward * uiDistance;
            screenCanvas.transform.rotation = Quaternion.LookRotation(screenCanvas.transform.position - camTransform.position);
        }

        // Update button canvas position
        if (buttonCanvas != null && buttonCanvas.activeSelf)
        {
            buttonCanvas.transform.position = camTransform.position + camTransform.forward * uiDistance;
            buttonCanvas.transform.rotation = Quaternion.LookRotation(buttonCanvas.transform.position - camTransform.position);
        }
    }

    void CheckVRControllerInput()
    {
        // Using new Input System for VR controller buttons
        bool returnPressed = false;

        // Get current gamepad (VR controllers are detected as gamepads)
        Gamepad gamepad = Gamepad.current;
        
        if (gamepad != null)
        {
            // Check face buttons (A, B, X, Y)
            if (gamepad.buttonSouth.wasPressedThisFrame ||  // A button
                gamepad.buttonEast.wasPressedThisFrame ||   // B button
                gamepad.buttonWest.wasPressedThisFrame ||   // X button
                gamepad.buttonNorth.wasPressedThisFrame)    // Y button
            {
                returnPressed = true;
                Debug.Log("[EndingManager] VR controller face button pressed (A/B/X/Y)");
            }

            // Check triggers and shoulders as fallback
            if (gamepad.leftTrigger.wasPressedThisFrame ||
                gamepad.rightTrigger.wasPressedThisFrame ||
                gamepad.leftShoulder.wasPressedThisFrame ||
                gamepad.rightShoulder.wasPressedThisFrame)
            {
                returnPressed = true;
                Debug.Log("[EndingManager] VR controller trigger/shoulder pressed");
            }
        }

        // Fallback: Check keyboard for testing in editor
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.anyKey.wasPressedThisFrame)
            {
                returnPressed = true;
                Debug.Log("[EndingManager] Keyboard pressed (editor testing)");
            }
        }

        // Return to opening scene if any button was pressed
        if (returnPressed)
        {
            ReturnToOpening();
        }
    }

    void CreateUIElements()
    {
        // Ensure there's an EventSystem for button interaction (VR compatible)
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            UnityEngine.EventSystems.StandaloneInputModule inputModule = eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            Debug.Log("[EndingManager] Created EventSystem for UI interaction");
        }

        // Get default font
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Create black screen for fade effects (WorldSpace Canvas for VR)
        CreateBlackScreen(defaultFont);

        // Create "YOU DIED" text canvas (WorldSpace for VR)
        CreateYouDiedCanvas(defaultFont);

        // Create screen subtitle for player dialogue (WorldSpace for VR)
        CreateScreenSubtitleCanvas(defaultFont);

        // Create world space subtitle for deer dialogue (already WorldSpace)
        CreateWorldSubtitleCanvas(defaultFont);

        // Create button canvas for return to title button (WorldSpace for VR)
        CreateButtonCanvas(defaultFont);
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
        
        // Add GraphicRaycaster (not needed for black screen but good practice)
        GraphicRaycaster raycaster = blackScreen.AddComponent<GraphicRaycaster>();
        raycaster.ignoreReversedGraphics = true;

        // Create black image
        GameObject blackObj = new GameObject("BlackImage");
        blackObj.transform.SetParent(blackScreen.transform, false);
        Image blackImg = blackObj.AddComponent<Image>();
        blackImg.color = new Color(0, 0, 0, 1); // Start with black
        blackImg.raycastTarget = false;
        SetStretch(blackObj.GetComponent<RectTransform>());

        Debug.Log("[EndingManager] Created WorldSpace black screen for VR");
    }

    void CreateYouDiedCanvas(Font font)
    {
        // Create "YOU DIED" text as WorldSpace canvas for VR
        youDiedCanvas = new GameObject("YouDiedCanvas");
        Canvas canvas = youDiedCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        
        // Set canvas size and scale
        RectTransform canvasRect = youDiedCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(800, 200);
        youDiedCanvas.transform.localScale = Vector3.one * 0.002f; // Smaller scale for text readability
        
        // Add GraphicRaycaster
        GraphicRaycaster raycaster = youDiedCanvas.AddComponent<GraphicRaycaster>();

        // Create "YOU DIED" text
        GameObject textObj = new GameObject("YouDiedText");
        textObj.transform.SetParent(youDiedCanvas.transform, false);
        youDiedText = textObj.AddComponent<Text>();
        youDiedText.font = font;
        youDiedText.text = "YOU DIED";
        youDiedText.fontSize = 80;
        youDiedText.alignment = TextAnchor.MiddleCenter;
        youDiedText.color = new Color(0.8f, 0.1f, 0.1f, 1f); // Dark red
        youDiedText.fontStyle = FontStyle.Bold;
        
        RectTransform textRect = youDiedText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(800, 150);
        textRect.anchoredPosition = Vector2.zero;
        
        youDiedCanvas.SetActive(false);

        Debug.Log("[EndingManager] Created WorldSpace 'YOU DIED' canvas for VR");
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
        
        // Add GraphicRaycaster
        GraphicRaycaster raycaster = screenCanvas.AddComponent<GraphicRaycaster>();

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
        // Create world space subtitle for deer dialogue (already WorldSpace, just keep it)
        worldCanvas = new GameObject("WorldSubtitleCanvas");
        Canvas wCanvas = worldCanvas.AddComponent<Canvas>();
        wCanvas.renderMode = RenderMode.WorldSpace;
        wCanvas.sortingOrder = 50;
        
        RectTransform wRect = worldCanvas.GetComponent<RectTransform>();
        wRect.sizeDelta = worldBubbleSize;
        worldCanvas.transform.localScale = Vector3.one * 0.01f;
        
        // Add GraphicRaycaster
        GraphicRaycaster raycaster = worldCanvas.AddComponent<GraphicRaycaster>();
        
        Image wBg = worldCanvas.AddComponent<Image>();
        wBg.color = new Color(0, 0, 0, 0.85f);
        worldText = CreateTextComponent(worldCanvas.transform, font, worldFontSize);
        worldCanvas.SetActive(false);

        Debug.Log("[EndingManager] Created WorldSpace world subtitle canvas for VR");
    }

    void CreateButtonCanvas(Font font)
    {
        // Create button canvas as WorldSpace for VR compatibility
        buttonCanvas = new GameObject("ButtonCanvas");
        Canvas canvas = buttonCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        
        // Set canvas size and scale
        RectTransform canvasRect = buttonCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 300);
        buttonCanvas.transform.localScale = Vector3.one * 0.002f;
        
        // Add GraphicRaycaster for VR pointer interaction
        GraphicRaycaster raycaster = buttonCanvas.AddComponent<GraphicRaycaster>();
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

        // Create return button
        CreateReturnButton(buttonCanvas.transform, font);
        
        buttonCanvas.SetActive(false);

        Debug.Log("[EndingManager] Created WorldSpace button canvas for VR");
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

    void CreateReturnButton(Transform parent, Font font)
    {
        GameObject buttonObj = new GameObject("ReturnButton");
        buttonObj.transform.SetParent(parent, false);
        
        // Set button position and size (centered for VR interaction)
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = buttonSize;
        buttonRect.anchoredPosition = Vector2.zero; // Center in canvas

        // Add button component
        returnButton = buttonObj.AddComponent<Button>();
        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = buttonColor;
        buttonImg.raycastTarget = true; // Ensure button can receive VR pointer events

        // Add outline for better visibility in VR
        Outline outline = buttonObj.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2, 2);

        // Create button text
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.font = font;
        buttonText.text = "Return to Opening\n<size=18>(Press A/B/X/Y on controller)</size>";
        buttonText.fontSize = buttonFontSize;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.raycastTarget = false; // Text shouldn't block button clicks
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Enhanced button colors for better VR feedback
        ColorBlock colors = returnButton.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = new Color(0.1f, 0.4f, 0.7f, 1f);
        colors.selectedColor = buttonHoverColor;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        returnButton.colors = colors;

        // Add navigation for VR controller support
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None; // Only one button, no navigation needed
        returnButton.navigation = nav;

        // Add click event (works with VR pointer/raycast interaction)
        returnButton.onClick.AddListener(ReturnToOpening);
        
        Debug.Log("[EndingManager] Return button created - Press A/B/X/Y on VR controller or point and click");
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

        // Show return button after ending
        buttonCanvas.SetActive(true);
        isRunning = false;
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

        // Wait before showing button
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator PlayFailureEnding()
    {
        // Screen is already black
        yield return new WaitForSeconds(0.5f);

        // Show "YOU DIED" text
        youDiedCanvas.SetActive(true);
        
        // Fade in "YOU DIED" text
        Text youDiedComponent = youDiedText;
        float elapsed = 0f;
        float fadeDuration = 1.5f;
        Color startColor = new Color(0.8f, 0.1f, 0.1f, 0f);
        Color endColor = new Color(0.8f, 0.1f, 0.1f, 1f);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            youDiedComponent.color = Color.Lerp(startColor, endColor, elapsed / fadeDuration);
            yield return null;
        }

        // Display "YOU DIED" for a while
        yield return new WaitForSeconds(2.0f);

        // Fade out "YOU DIED"
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            youDiedComponent.color = Color.Lerp(endColor, startColor, elapsed / fadeDuration);
            yield return null;
        }

        youDiedCanvas.SetActive(false);

        // Wait before showing button
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

    // Legacy method name for UI button compatibility (if needed)
    void ReturnToTitle()
    {
        ReturnToOpening();
    }

    // Method to be called from previous scene's manager
    public static void SetEndingResult(bool success)
    {
        GameSuccess = success;
        Debug.Log($"[EndingManager] SetEndingResult called! Parameter: {success}, GameSuccess is now: {GameSuccess}");
    }
}