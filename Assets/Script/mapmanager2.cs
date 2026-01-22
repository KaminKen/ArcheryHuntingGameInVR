using UnityEngine;
using System.Collections.Generic;

public class MapBoundaryGenerator : MonoBehaviour
{
    [Header("Boundary Settings")]
    public float outerSize = 60f;    // 전체 외곽 크기 (60x60)
    public float innerSize = 50f;    // 내부 빈 공간 크기 (50x50)

    [Header("Rock Settings")]
    public GameObject[] rockPrefabs;
    public float rockSpacing = 1.5f;   // 돌 사이의 간격 (값이 작을수록 촘촘함)
    public float positionJitter = 0.4f; // 배치 랜덤 오차 (격자 무늬 방지)

    [Header("Size Controls")]
    public float minScale = 0.5f;      // 돌의 최소 크기 (수정 가능)
    public float maxScale = 1.2f;      // 돌의 최대 크기 (수정 가능)

    private GameObject rockGroup;

    [ContextMenu("Generate Rock Boundary")]
    public void GenerateBoundary()
    {
        ClearBoundary();

        rockGroup = new GameObject("GeneratedBoundaryRocks");
        rockGroup.transform.SetParent(this.transform);
        rockGroup.transform.localPosition = Vector3.zero;

        float halfOuter = outerSize / 2f;
        float halfInner = innerSize / 2f;

        // X, Z 축으로 그리드를 돌며 배치
        for (float x = -halfOuter; x <= halfOuter; x += rockSpacing)
        {
            for (float z = -halfOuter; z <= halfOuter; z += rockSpacing)
            {
                // 60x60 안쪽이면서 50x50 바깥쪽인 영역 검사
                if (Mathf.Abs(x) > halfInner || Mathf.Abs(z) > halfInner)
                {
                    SpawnRock(new Vector3(x, 0, z));
                }
            }
        }
        Debug.Log("외곽 돌 벽 생성 완료! 돌 크기 범위: " + minScale + " ~ " + maxScale);
    }

    void SpawnRock(Vector3 position)
    {
        if (rockPrefabs == null || rockPrefabs.Length == 0) return;

        // 위치 미세 조정
        Vector3 finalPos = position + new Vector3(
            Random.Range(-positionJitter, positionJitter),
            0,
            Random.Range(-positionJitter, positionJitter)
        );

        GameObject prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];

        // 회전: Y축은 완전 랜덤, X/Z는 살짝만 기울임 (자연스러움)
        Quaternion randomRot = Quaternion.Euler(
            Random.Range(-5f, 5f),
            Random.Range(0, 360f),
            Random.Range(-5f, 5f)
        );

        GameObject rock = Instantiate(prefab, transform.position + finalPos, randomRot, rockGroup.transform);

        // 크기 설정 (인스펙터에서 입력받은 값 사용)
        float randomScale = Random.Range(minScale, maxScale);
        rock.transform.localScale = Vector3.one * randomScale;
    }

    [ContextMenu("Clear Boundary")]
    public void ClearBoundary()
    {
        Transform existingGroup = transform.Find("GeneratedBoundaryRocks");
        if (existingGroup != null)
        {
            DestroyImmediate(existingGroup.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(outerSize, 0, outerSize));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(innerSize, 0, innerSize));
    }
}