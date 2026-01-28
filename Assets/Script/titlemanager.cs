using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleSceneGenerator : MonoBehaviour
{
    [Header("Title Settings")]
    public string gameTitle = "MY AWESOME VR GAME";
    public string openingSceneName = "OpeningScene";

    [Header("Transform Settings")]
    public Vector3 panelPosition = new Vector3(0, 1.5f, 3.0f);
    public Vector3 panelRotation = new Vector3(0, 0, 0);

    [Header("Visual Customization")]
    public Color panelColor = new Color(0, 0, 0, 0.85f);
    public Color buttonColor = Color.white;
    public Vector2 buttonSize = new Vector2(300, 70);

    // 에디터에서 버튼을 만들기 위한 함수
    [ContextMenu("Generate Title UI")] // 인스펙터 컴포넌트 우클릭 메뉴에서 실행 가능
    public void GenerateTitleUI()
    {
        // 1. 기존에 생성된 UI가 있다면 삭제 (중복 방지)
        GameObject existingCanvas = GameObject.Find("TitleCanvas_Permanent");
        if (existingCanvas != null)
        {
            DestroyImmediate(existingCanvas);
        }

        // 2. Canvas 생성
        GameObject canvasObj = new GameObject("TitleCanvas_Permanent");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        canvasRT.position = panelPosition;
        canvasRT.rotation = Quaternion.Euler(panelRotation);
        canvasRT.sizeDelta = new Vector2(600, 800);
        canvasRT.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // 3. 배경 Panel 생성
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = panelColor;

        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;

        // 4. 레이아웃 설정
        VerticalLayoutGroup layout = panelObj.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 50;
        layout.childControlHeight = false;
        layout.childControlWidth = false;

        // 5. 제목 생성
        CreateText(panelObj.transform, gameTitle, 70);

        // 6. 시작 버튼 생성
        CreateButton(panelObj.transform, "START GAME", openingSceneName, true);

        // 7. 나가기 버튼 생성
        CreateButton(panelObj.transform, "QUIT", "", false);

        Debug.Log("Title UI가 성공적으로 생성되었습니다. 이제 씬을 저장하세요!");
    }

    void CreateText(Transform parent, string content, int fontSize)
    {
        GameObject textObj = new GameObject("TitleText");
        textObj.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(500, 150);
    }

    void CreateButton(Transform parent, string label, string sceneName, bool isStartButton)
    {
        GameObject btnObj = new GameObject(label + "_Button");
        btnObj.transform.SetParent(parent, false);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = buttonColor;

        Button btn = btnObj.AddComponent<Button>();

        // 버튼 텍스트
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnTxt = txtObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = label;
        btnTxt.color = Color.black;
        btnTxt.fontSize = 32;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.rectTransform.anchorMin = Vector2.zero;
        btnTxt.rectTransform.anchorMax = Vector2.one;
        btnTxt.rectTransform.sizeDelta = Vector2.zero;

        btnObj.GetComponent<RectTransform>().sizeDelta = buttonSize;

        // 중요: 씬에 영구 보관되는 버튼에 이벤트를 할당하려면 
        // 별도의 헬퍼 스크립트를 붙이거나 가벼운 컴포넌트를 연결하는 것이 좋습니다.
        TitleButtonHandler handler = btnObj.AddComponent<TitleButtonHandler>();
        if (isStartButton)
        {
            handler.targetScene = sceneName;
            btn.onClick.AddListener(handler.LoadScene);
        }
        else
        {
            btn.onClick.AddListener(handler.QuitGame);
        }
    }
}

// 버튼 클릭 이벤트를 저장하기 위한 작은 클래스
public class TitleButtonHandler : MonoBehaviour
{
    public string targetScene;
    public void LoadScene() => SceneManager.LoadScene(targetScene);
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

// 인스펙터에 실제 버튼을 표시해주는 에디터 스크립트
#if UNITY_EDITOR
[CustomEditor(typeof(TitleSceneGenerator))]
public class TitleGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 기존 변수들 표시

        TitleSceneGenerator generator = (TitleSceneGenerator)target;

        GUILayout.Space(20);
        if (GUILayout.Button("Generate Title UI", GUILayout.Height(40)))
        {
            generator.GenerateTitleUI();
        }
        
        if (GUILayout.Button("Clear Generated UI"))
        {
            GameObject obj = GameObject.Find("TitleCanvas_Permanent");
            if (obj != null) DestroyImmediate(obj);
        }
    }
}
#endif