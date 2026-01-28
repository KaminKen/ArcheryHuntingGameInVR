using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimpleAdventureManager : MonoBehaviour
{
    [Header("오브젝트 설정")]
    public GameObject player;
    public GameObject deer;
    public GameObject tent;
    public AudioSource soundEffect;

    [Header("애니메이션 설정")]
    [Range(1f, 10f)] public float textHeight = 2.5f;
    [Range(1f, 20f)] public float turnSpeed = 5.0f; // 회전 속도 조절
    public bool startAnimation = false;

    [Header("UI 크기 및 폰트 커스텀")]
    public Vector2 worldBubbleSize = new Vector2(600, 200);
    public Vector2 screenBubbleSize = new Vector2(900, 150);
    [Range(10, 100)] public int worldFontSize = 40;
    [Range(10, 100)] public int screenFontSize = 24;

    [Header("대사 시간 설정")]
    public float timePerChar = 0.1f;
    public float minDisplayTime = 1.5f;
    public float maxDisplayTime = 5.0f;

    private GameObject blackScreen;
    private GameObject worldCanvas;
    private GameObject screenCanvas;
    private Text worldText;
    private Text screenText;
    private bool isRunning = false;
    private bool isFunction1Active = true;

    // 회전 제어 변수
    private Transform currentLookTarget;

    void OnValidate()
    {
        if (worldCanvas != null) worldCanvas.GetComponent<RectTransform>().sizeDelta = worldBubbleSize;
        if (screenCanvas != null) screenCanvas.GetComponent<RectTransform>().sizeDelta = screenBubbleSize;
        if (worldText != null) worldText.fontSize = worldFontSize;
        if (screenText != null) screenText.fontSize = screenFontSize;

        if (startAnimation && Application.isPlaying && !isRunning)
        {
            TriggerArrowEvent();
        }
    }

    void Awake()
    {
        CreateUIElements();
    }

    void Update()
    {
        // 타겟이 설정되어 있을 경우 부드럽게 회전
        if (currentLookTarget != null && deer != null && deer.activeSelf)
        {
            Vector3 direction = currentLookTarget.position - deer.transform.position;
            direction.y = 0; // 수평 회전 유지

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                deer.transform.rotation = Quaternion.Slerp(deer.transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            }
        }
    }

    void CreateUIElements()
    {
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject canvasObj = new GameObject("AdventureCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        blackScreen = new GameObject("BlackFade");
        blackScreen.transform.SetParent(canvasObj.transform, false);
        Image blackImg = blackScreen.AddComponent<Image>();
        blackImg.color = new Color(0, 0, 0, 0);
        blackImg.raycastTarget = false;
        SetStretch(blackScreen.GetComponent<RectTransform>());

        screenCanvas = new GameObject("PlayerSubtitle");
        screenCanvas.transform.SetParent(canvasObj.transform, false);
        Image subBg = screenCanvas.AddComponent<Image>();
        subBg.color = new Color(0, 0, 0, 0.85f);
        screenText = CreateTextComponent(screenCanvas.transform, defaultFont, screenFontSize);
        RectTransform subRect = screenCanvas.GetComponent<RectTransform>();
        subRect.sizeDelta = screenBubbleSize;
        subRect.anchoredPosition = new Vector2(0, 80);
        subRect.anchorMin = new Vector2(0.5f, 0); subRect.anchorMax = new Vector2(0.5f, 0);
        screenCanvas.SetActive(false);

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
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(20, 15); rt.offsetMax = new Vector2(-20, -15);
        return t;
    }

    void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    public void TriggerArrowEvent()
    {
        if (isFunction1Active && !isRunning) StartCoroutine(MainSequence());
    }

    IEnumerator MainSequence()
    {
        isRunning = true;
        isFunction1Active = false;
        startAnimation = false;

        if (soundEffect) soundEffect.Play();
        yield return StartCoroutine(FadeEffect(1f));

        if (deer != null)
        {
            deer.transform.position = player.transform.position + player.transform.forward * 3.0f;
            currentLookTarget = player.transform; // 주인공을 부드럽게 바라보기 시작
        }

        yield return StartCoroutine(FadeEffect(0f));

        yield return ShowTalk(deer, "Deer", "So… why did you shoot me?");
        yield return ShowTalk(null, "Player", "Uh… because it’s a game?");
        yield return ShowTalk(deer, "Deer", "Must be nice, living that kind of life.");
        yield return ShowTalk(deer, "Deer", "You stick an arrow in an innocent creature’s butt and that’s what you say?");
        yield return ShowTalk(deer, "Deer", "“Well, it’s a game.”");
        yield return ShowTalk(null, "Player", "Sorry…");
        yield return ShowTalk(deer, "Deer", "I want to call the cops, honestly.\nBut we’re in the middle of the forest.");

        // [행동] 텐트 쪽으로 부드럽게 시선 전환
        currentLookTarget = tent.transform;

        yield return ShowTalk(deer, "Deer", "Oh. Nice camp.\nIf I rest here for one night, I’ll be totally fine.");
        yield return ShowTalk(null, "Player", "That’s my camp though?");

        // 이동 시작 시 시선 추적 중단 (이동 방향을 보기 위해)
        currentLookTarget = null;
        yield return StartCoroutine(MoveDeerToTent());
        if (deer != null) deer.SetActive(false);

        yield return ShowTalk(tent, "Deer", "Oh, is this a sleeping bag? Perfect.");
        yield return ShowTalk(tent, "Deer", "Oh right. While I’m resting, skeletons are gonna swarm in, so… block them for me.");
        yield return ShowTalk(null, "Player", "What? Does that even make sense?");
        yield return ShowTalk(tent, "Deer", "A deer talking makes sense, but that doesn’t?");
        yield return ShowTalk(tent, "Deer", "Actually, tomorrow’s a holiday, so the skeletons won’t show up.");
        yield return ShowTalk(tent, "Deer", "Hang in there for one day.\nBye.");

        isRunning = false;
    }

    IEnumerator ShowTalk(GameObject target, string speaker, string content)
    {
        string textLine = $"<b>{speaker}: {content}</b>";
        float displayTime = Mathf.Clamp(content.Length * timePerChar, minDisplayTime, maxDisplayTime);

        if (target == null)
        {
            screenCanvas.SetActive(true);
            screenText.text = textLine;
            yield return new WaitForSeconds(displayTime);
            screenCanvas.SetActive(false);
        }
        else
        {
            worldCanvas.SetActive(true);
            worldText.text = textLine;
            float timer = 0;
            while (timer < displayTime)
            {
                if (target != null && Camera.main != null)
                {
                    worldCanvas.transform.position = target.transform.position + Vector3.up * textHeight;
                    worldCanvas.transform.rotation = Quaternion.LookRotation(worldCanvas.transform.position - Camera.main.transform.position);
                }
                timer += Time.deltaTime;
                yield return null;
            }
            worldCanvas.SetActive(false);
        }
    }

    IEnumerator MoveDeerToTent()
    {
        while (deer != null && Vector3.Distance(deer.transform.position, tent.transform.position) > 0.6f)
        {
            // 이동 방향을 부드럽게 바라보며 이동
            Vector3 moveDir = tent.transform.position - deer.transform.position;
            moveDir.y = 0;
            if (moveDir.sqrMagnitude > 0.01f)
            {
                deer.transform.rotation = Quaternion.Slerp(deer.transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * turnSpeed);
            }

            deer.transform.position = Vector3.MoveTowards(deer.transform.position, tent.transform.position, Time.deltaTime * 3.5f);
            yield return null;
        }
    }

    IEnumerator FadeEffect(float targetAlpha)
    {
        Image img = blackScreen.GetComponent<Image>();
        float startAlpha = img.color.a;
        float elapsed = 0;
        while (elapsed < 1.2f)
        {
            elapsed += Time.deltaTime;
            img.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, targetAlpha, elapsed / 1.2f));
            yield return null;
        }
    }
}