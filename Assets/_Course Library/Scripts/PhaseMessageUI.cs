using System.Collections;
using TMPro;
using UnityEngine;

public class PhaseMessageUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeIn = 0.25f;
    [SerializeField] private float hold = 2.5f;
    [SerializeField] private float fadeOut = 0.35f;

    [Header("Messages")]
    [TextArea] public string dayMsg   = "Day begins...";
    [TextArea] public string nightMsg = "Night falls...";
    [TextArea] public string dawnMsg  = "Dawn... stay sharp.";
    [TextArea] public string dawnWarningExtra = "Deer: Hey! It's coming from behind!";

    private TimePhase lastPhase;
    private Coroutine routine;

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Start()
    {
        if (gameManager == null) gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("[PhaseMessageUI] GameManager not found in scene.");
            return;
        }

        if (messageText == null)
        {
            Debug.LogError("[PhaseMessageUI] messageText not assigned.");
            return;
        }

        lastPhase = gameManager.GetCurrentPhase();
        ShowForPhase(lastPhase);
    }

    void Update()
    {
        if (gameManager == null) return;

        var phase = gameManager.GetCurrentPhase();
        if (phase != lastPhase)
        {
            lastPhase = phase;
            ShowForPhase(phase);
        }
    }

    void ShowForPhase(TimePhase phase)
    {
        string msg = phase switch
        {
            TimePhase.Day   => dayMsg,
            TimePhase.Night => nightMsg,
            TimePhase.Dawn  => dawnMsg + "\n" + dawnWarningExtra,
            _ => ""
        };

        Show(msg);
    }

    public void Show(string msg)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(msg));
    }

    IEnumerator ShowRoutine(string msg)
    {
        messageText.text = msg;

        // Fade in
        yield return FadeTo(1f, fadeIn);

        // Hold
        yield return new WaitForSeconds(hold);

        // Fade out
        yield return FadeTo(0f, fadeOut);

        routine = null;
    }

    IEnumerator FadeTo(float target, float duration)
    {
        float start = canvasGroup.alpha;
        if (duration <= 0f)
        {
            canvasGroup.alpha = target;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            // smooth
            k = k * k * (3f - 2f * k);
            canvasGroup.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }

        canvasGroup.alpha = target;
    }
}
