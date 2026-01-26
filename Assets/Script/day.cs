using UnityEngine;

public class DayNightCyclePlus : MonoBehaviour
{
    [Header("? 시간 설정")]
    [Tooltip("하루 주기 (초)")]
    public float dayDuration = 10f;
    [Range(0, 1)]
    public float currentTime = 0.25f;

    [Header("?? 현재 시간 표시 (Read Only)")]
    public string timeDisplay;

    [Header("?? 광원 및 비주얼")]
    public Light sunLight;
    public MeshRenderer sunVisual; // 태양 구체 (Sphere)
    public Light moonLight;
    public MeshRenderer moonVisual; // 달 구체 (Sphere)

    [Tooltip("태양과 달이 배치될 거리")]
    public float orbitDistance = 100f;
    [Tooltip("태양과 달의 크기")]
    public float bodySize = 5f;

    [Header("?? 환경 설정")]
    public Material skyboxMaterial;
    public bool useFog = true;

    [Header("?? 자동 그라데이션")]
    public Gradient sunColor;
    public Gradient moonColor;
    public Gradient skyboxTint;
    public Gradient fogColor;

    void Start()
    {
        SetupGradients();
        if (useFog) RenderSettings.fog = true;

        // 비주얼 오브젝트 초기화 (크기 조절 등)
        if (sunVisual != null) sunVisual.transform.localScale = Vector3.one * bodySize;
        if (moonVisual != null) moonVisual.transform.localScale = Vector3.one * bodySize;
    }

    void Update()
    {
        // 1. 시간 흐름 업데이트
        currentTime += Time.deltaTime / dayDuration;
        if (currentTime >= 1f) currentTime = 0f;

        // 2. 인스펙터에 시간 표시
        UpdateInspectorTime();

        // 3. 태양과 달의 회전 및 위치 업데이트
        UpdateCelestialBodies();

        // 4. 강도 및 색상 조절
        UpdateLighting();
    }

    void UpdateCelestialBodies()
    {
        float sunAngle = currentTime * 360f;

        // 빛의 회전
        sunLight.transform.rotation = Quaternion.Euler(sunAngle - 90f, -90f, 0f);
        moonLight.transform.rotation = Quaternion.Euler(sunAngle + 90f, -90f, 0f);

        // 비주얼(구체)의 위치 설정: 빛이 비추는 반대 방향 하늘에 배치
        if (sunVisual != null)
        {
            sunVisual.transform.position = transform.position - (sunLight.transform.forward * orbitDistance);
            sunVisual.transform.LookAt(transform.position); // 항상 중심을 바라보게
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
    }

    void UpdateLighting()
    {
        float sunIntensity = Mathf.Clamp01(Vector3.Dot(sunLight.transform.forward, Vector3.down));
        float moonIntensity = Mathf.Clamp01(Vector3.Dot(moonLight.transform.forward, Vector3.down));

        // 태양/달 색상 및 강도
        Color currentSunColor = sunColor.Evaluate(currentTime);
        Color currentMoonColor = moonColor.Evaluate(currentTime);

        sunLight.color = currentSunColor;
        sunLight.intensity = sunIntensity * 1.2f;

        moonLight.color = currentMoonColor;
        moonLight.intensity = moonIntensity * 0.5f;

        // 추가: 구체(Visual) 자체의 색상도 빛과 맞춤 (Emission 활용)
        if (sunVisual != null) sunVisual.material.SetColor("_EmissionColor", currentSunColor * 2f);
        if (moonVisual != null) moonVisual.material.SetColor("_EmissionColor", currentMoonColor * 2f);

        // 스카이박스 및 안개
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetColor("_Tint", skyboxTint.Evaluate(currentTime));
            skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(0.2f, 1.0f, sunIntensity));
        }

        if (useFog)
        {
            RenderSettings.fogColor = fogColor.Evaluate(currentTime);
            RenderSettings.fogDensity = Mathf.Lerp(0.05f, 0.02f, sunIntensity);
        }

        RenderSettings.ambientLight = fogColor.Evaluate(currentTime) * 0.5f;
    }

    // (SetupGradients 및 CreateGradient 함수는 기존과 동일하므로 생략 가능하나 유지를 위해 포함)
    void SetupGradients()
    {
        sunColor = CreateGradient(new Color(1f, 0.4f, 0.2f), Color.white, new Color(1f, 0.3f, 0.1f), new Color(0.1f, 0.1f, 0.2f));
        moonColor = CreateGradient(Color.black, Color.black, Color.black, new Color(0.7f, 0.8f, 1f));
        skyboxTint = CreateGradient(new Color(0.3f, 0.2f, 0.4f), new Color(0.5f, 0.7f, 1f), new Color(0.8f, 0.4f, 0.2f), new Color(0.02f, 0.02f, 0.1f));
        fogColor = CreateGradient(new Color(0.5f, 0.3f, 0.2f), new Color(0.7f, 0.8f, 1f), new Color(0.6f, 0.3f, 0.2f), new Color(0.01f, 0.02f, 0.05f));
    }

    Gradient CreateGradient(Color dawn, Color day, Color dusk, Color night)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(dawn, 0.2f),
                new GradientColorKey(day, 0.5f),
                new GradientColorKey(dusk, 0.75f),
                new GradientColorKey(night, 0.9f),
                new GradientColorKey(dawn, 1.0f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }
}