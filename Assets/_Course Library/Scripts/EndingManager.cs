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

        // Create main canvas
        GameObject canvasObj = new GameObject("EndingCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster for VR pointer interaction
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

        // Create black screen for fade effects
        blackScreen = new GameObject("BlackFade");
        blackScreen.transform.SetParent(canvasObj.transform, false);
        Image blackImg = blackScreen.AddComponent<Image>();
        blackImg.color = new Color(0, 0, 0, 1); // Start with black
        blackImg.raycastTarget = false;
        SetStretch(blackScreen.GetComponent<RectTransform>());

        // Create "YOU DIED" text (for failure ending)
        GameObject youDiedObj = new GameObject("YouDiedText");
        youDiedObj.transform.SetParent(canvasObj.transform, false);
        youDiedText = youDiedObj.AddComponent<Text>();
        youDiedText.font = defaultFont;
        youDiedText.text = "YOU DIED";
        youDiedText.fontSize = 80;
        youDiedText.alignment = TextAnchor.MiddleCenter;
        youDiedText.color = new Color(0.8f, 0.1f, 0.1f, 1f); // Dark red
        youDiedText.fontStyle = FontStyle.Bold;
        RectTransform youDiedRect = youDiedText.GetComponent<RectTransform>();
        youDiedRect.anchorMin = new Vector2(0.5f, 0.5f);
        youDiedRect.anchorMax = new Vector2(0.5f, 0.5f);
        youDiedRect.sizeDelta = new Vector2(800, 150);
        youDiedRect.anchoredPosition = Vector2.zero;
        youDiedObj.SetActive(false);

        // Create screen subtitle for player dialogue
        screenCanvas = new GameObject("PlayerSubtitle");
        screenCanvas.transform.SetParent(canvasObj.transform, false);
        Image subBg = screenCanvas.AddComponent<Image>();
        subBg.color = new Color(0, 0, 0, 0.85f);
        screenText = CreateTextComponent(screenCanvas.transform, defaultFont, screenFontSize);
        RectTransform subRect = screenCanvas.GetComponent<RectTransform>();
        subRect.sizeDelta = screenBubbleSize;
        subRect.anchoredPosition = new Vector2(0, 80);
        subRect.anchorMin = new Vector2(0.5f, 0);
        subRect.anchorMax = new Vector2(0.5f, 0);
        screenCanvas.SetActive(false);

        // Create world space subtitle for deer dialogue
        worldCanvas = new GameObject("WorldSubtitle");
        Canvas wCanvas = worldCanvas.AddComponent<Canvas>();
        wCanvas.renderMode = RenderMode.WorldSpace;
        RectTransform wRect = worldCanvas.GetComponent<RectTransform>();
        wRect.sizeDelta = worldBubbleSize;
        worldCanvas.transform.localScale = Vector3.one * 0.01f;
        Image wBg = worldCanvas.AddComponent<Image>();
        wBg.color = new Color(0, 0, 0, 0.85f);
        worldText = CreateTextComponent(worldCanvas.transform, defaultFont, worldFontSize);
        worldCanvas.SetActive(false);

        // Create button canvas for return to title button
        buttonCanvas = new GameObject("ButtonCanvas");
        buttonCanvas.transform.SetParent(canvasObj.transform, false);
        CreateReturnButton(buttonCanvas.transform, defaultFont);
        buttonCanvas.SetActive(false);
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
        
        // Set button position and size (larger for VR interaction)
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = buttonSize;
        buttonRect.anchoredPosition = new Vector2(0, -200);

        // Add button component
        returnButton = buttonObj.AddComponent<Button>();
        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = buttonColor;
        buttonImg.raycastTarget = true; // Ensure button can receive VR pointer events

        // Add outline for better visibility
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

        // Add click event (for UI raycast interaction, if it works)
        returnButton.onClick.AddListener(ReturnToOpening);
        
        Debug.Log("[EndingManager] Return button created - Press A/B/X/Y on VR controller to return");
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
        youDiedText.gameObject.SetActive(true);
        
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

        youDiedText.gameObject.SetActive(false);

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
            // Show as screen subtitle (for player dialogue)
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
        Image img = blackScreen.GetComponent<Image>();
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