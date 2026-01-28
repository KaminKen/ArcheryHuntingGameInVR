using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
// VR 레이캐스트 지원을 위해 필수
using UnityEngine.XR.Interaction.Toolkit.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events; // 에디터에서 이벤트를 영구 기록하기 위함
#endif

public class titlemanager : MonoBehaviour
{
    [Header("Title Settings")]
    public string gameTitle = "MY VR ADVENTURE";
    public string openingSceneName = "OpeningScene";

    [Header("Transform Settings")]
    public Vector3 panelPosition = new Vector3(0, 1.5f, 3.0f);
    public Vector3 panelRotation = new Vector3(0, 0, 0);

    [Header("Visual Customization")]
    public Color panelColor = new Color(0, 0, 0, 0.9f);
    public Vector2 buttonSize = new Vector2(350, 80);

    [ContextMenu("Generate Title UI")]
    public void GenerateTitleUI()
    {
        // 1. 기존 UI 제거
        GameObject oldCanvas = GameObject.Find("TitleCanvas_VR_Permanent");
        if (oldCanvas != null) DestroyImmediate(oldCanvas);

        // 2. Canvas 생성 및 VR 설정
        GameObject canvasObj = new GameObject("TitleCanvas_VR_Permanent");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // 

        // VR에서 컨트롤러 광선을 인식하게 하는 핵심 컴포넌트
        canvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();
        canvasObj.AddComponent<CanvasScaler>();

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        canvasRT.position = panelPosition;
        canvasRT.rotation = Quaternion.Euler(panelRotation);
        canvasRT.sizeDelta = new Vector2(600, 800);
        canvasRT.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // VR 클릭을 돕기 위한 박스 콜라이더 추가 (선택 사항이나 권장)
        BoxCollider collider = canvasObj.AddComponent<BoxCollider>();
        collider.size = new Vector3(600, 800, 1);

        // 3. 패널 및 레이아웃
        GameObject panelObj = new GameObject("Background");
        panelObj.transform.SetParent(canvasObj.transform, false);
        panelObj.AddComponent<Image>().color = panelColor;

        VerticalLayoutGroup layout = panelObj.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 60;
        layout.childControlHeight = false;
        layout.childControlWidth = false;

        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one; panelRT.sizeDelta = Vector2.zero;

        // 4. 제목 생성
        CreateText(panelObj.transform, gameTitle, 80);

        // 5. 버튼 생성 및 이벤트 영구 연결
        CreateVRButton(panelObj.transform, "START GAME", "LoadOpeningScene");
        CreateVRButton(panelObj.transform, "QUIT GAME", "QuitApplication");

        Debug.Log("VR 전용 Title UI 생성 완료! EventSystem에 XR UI Input Module이 있는지 확인하세요.");
    }

    void CreateText(Transform parent, string content, int fontSize)
    {
        GameObject textObj = new GameObject("TitleText");
        textObj.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(550, 200);
    }

    void CreateVRButton(Transform parent, string label, string methodName)
    {
        GameObject btnObj = new GameObject(label + "_Btn");
        btnObj.transform.SetParent(parent, false);
        btnObj.AddComponent<Image>();
        Button btn = btnObj.AddComponent<Button>();
        btnObj.GetComponent<RectTransform>().sizeDelta = buttonSize;

        // 텍스트 생성
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnTxt = txtObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = label;
        btnTxt.color = Color.black;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.rectTransform.anchorMin = Vector2.zero; btnTxt.rectTransform.anchorMax = Vector2.one; btnTxt.rectTransform.sizeDelta = Vector2.zero;

        // 이벤트 처리기 부착
        TitleActionHandler handler = btnObj.AddComponent<TitleActionHandler>();
        handler.targetScene = openingSceneName;

#if UNITY_EDITOR
        // 에디터에서 이벤트를 '영구적(Persistent)'으로 기록하여 씬 저장 시 유지되게 함
        UnityEventTools.AddPersistentListener(btn.onClick, (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), handler, methodName));
#endif
    }
}

// 런타임 작동을 보장하는 별도 핸들러
public class TitleActionHandler : MonoBehaviour
{
    public string targetScene;

    public void LoadOpeningScene()
    {
        Debug.Log("씬 전환 시작: " + targetScene);
        SceneManager.LoadScene(targetScene);
    }

    public void QuitApplication()
    {
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(titlemanager))]
public class TitleGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        titlemanager gen = (titlemanager)target;
        GUILayout.Space(20);
        if (GUILayout.Button("Generate VR Title UI", GUILayout.Height(40))) gen.GenerateTitleUI();
    }
}
#endif