using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private float alpha = 0f;          // 화면 투명도 (0: 투명, 1: 검정)
    private Texture2D blackTexture;    // 화면을 덮을 검은색 텍스처
    public float fadeDuration = 1.5f;  // 암전 시간

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬이 넘어가도 암전 효과를 유지하거나 관리하기 위해 파괴되지 않게 설정
            DontDestroyOnLoad(gameObject);

            // 검은색 단색 텍스처 생성
            blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, Color.black);
            blackTexture.Apply();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 사슴이 호출하는 함수
    public void OnDeerHit()
    {
        StartCoroutine(FadeOutAndChangeScene());
    }

    private IEnumerator FadeOutAndChangeScene()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // 시간에 따라 alpha 값을 0에서 1로 증가
            alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        // 암전 완료 후 씬 전환
        SceneManager.LoadScene("mainscene");

        // (선택 사항) 전환 후 다시 밝아지게 하고 싶다면 아래 코루틴 실행
        // StartCoroutine(FadeIn());
    }

    // 유니티의 레거시 GUI 시스템을 이용해 화면에 직접 그리기
    private void OnGUI()
    {
        if (alpha > 0)
        {
            // GUI 색상을 검은색 + 현재 alpha 값으로 설정
            GUI.color = new Color(0, 0, 0, alpha);
            // 전체 화면 크기로 텍스처 그리기
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
        }
    }
}