using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("시간 설정")]
    public float dayDuration = 120f;
    [Range(0, 1)]
    public float currentTime = 0.25f;

    [Header("참조 및 자동 설정 색상")]
    public Light sunLight;
    public Gradient sunColor;
    public Gradient ambientColor;

    private void Reset()
    {
        // 1. 태양 빛 색상 프리셋 (일출-정오-일몰-밤)
        sunColor = new Gradient();
        GradientColorKey[] sunKeys = new GradientColorKey[5];
        sunKeys[0] = new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 0.2f);  // 새벽
        sunKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0.2f), 0.25f);    // 일출 (주황)
        sunKeys[2] = new GradientColorKey(Color.white, 0.5f);                   // 정오 (흰색)
        sunKeys[3] = new GradientColorKey(new Color(1f, 0.4f, 0.2f), 0.75f);    // 일몰 (진한 주황)
        sunKeys[4] = new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 0.8f);  // 밤

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);
        sunColor.SetKeys(sunKeys, alphaKeys);

        // 2. 주변광(Ambient) 색상 프리셋 (에러 수정된 부분)
        ambientColor = new Gradient();
        GradientColorKey[] ambKeys = new GradientColorKey[5];
        Color nightColor = new Color(0.15f, 0.15f, 0.25f); // 밤의 푸르스름한 어둠
        Color dayColor = new Color(0.7f, 0.75f, 0.8f);     // 낮의 밝은 하늘색

        ambKeys[0] = new GradientColorKey(nightColor, 0.2f);
        ambKeys[1] = new GradientColorKey(dayColor, 0.35f);
        ambKeys[2] = new GradientColorKey(dayColor, 0.65f);
        ambKeys[3] = new GradientColorKey(nightColor, 0.8f);
        ambKeys[4] = new GradientColorKey(nightColor, 1f);

        ambientColor.SetKeys(ambKeys, alphaKeys);

        if (sunLight == null) sunLight = GetComponent<Light>();
    }

    void Update()
    {
        currentTime += Time.deltaTime / dayDuration;
        if (currentTime >= 1f) currentTime = 0f;

        // 태양 회전
        float sunRotation = (currentTime * 360f) - 90f;
        sunLight.transform.localRotation = Quaternion.Euler(sunRotation, 170f, 0f);

        // 태양 색상 및 강도
        sunLight.color = sunColor.Evaluate(currentTime);
        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        sunLight.intensity = Mathf.Lerp(0, 1.2f, Mathf.Max(0, dotProduct));

        // 주변광 적용
        RenderSettings.ambientLight = ambientColor.Evaluate(currentTime);
    }
}