using System.Collections.Generic;
using UnityEngine;

public class DecorationGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapSize = 50;           // 전체 맵 크기
    public float holeRadius = 20f;     // 이 반지름 "안쪽"에만 데코 생성

    [Header("Decoration Settings")]
    public GameObject[] decoPrefabs;
    public float minDistance = 1f;     // 데코레이션 간 최소 거리
    [Range(10, 50)] public int rejectionSamples = 30;

    private GameObject decoGroup;

    [ContextMenu("Generate Decorations")]
    public void GenerateDecorations()
    {
        ClearDecorations();

        decoGroup = new GameObject("GeneratedDecorations");
        decoGroup.transform.SetParent(this.transform);
        decoGroup.transform.localPosition = Vector3.zero;

        List<Vector2> points = GeneratePointsData();

        foreach (Vector2 point in points)
        {
            if (decoPrefabs == null || decoPrefabs.Length == 0) break;

            GameObject prefab = decoPrefabs[Random.Range(0, decoPrefabs.Length)];

            // 좌표 계산: 맵 중앙을 (0,0,0)으로 맞춤
            Vector3 spawnPos = new Vector3(point.x - mapSize / 2f, 0, point.y - mapSize / 2f);

            GameObject deco = Instantiate(prefab, transform.position + spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0), decoGroup.transform);

            // 자연스러움을 위한 랜덤 크기
            float randomScale = Random.Range(0.6f, 1.1f);
            deco.transform.localScale = Vector3.one * randomScale;
        }

        Debug.Log($"{points.Count}개의 데코레이션이 안쪽 영역에 생성되었습니다.");
    }

    [ContextMenu("Clear Decorations")]
    public void ClearDecorations()
    {
        Transform existingGroup = transform.Find("GeneratedDecorations");
        if (existingGroup != null) DestroyImmediate(existingGroup.gameObject);
    }

    List<Vector2> GeneratePointsData()
    {
        float cellSize = minDistance / Mathf.Sqrt(2);
        int gridSize = Mathf.Max(1, Mathf.CeilToInt(mapSize / cellSize));
        int[,] grid = new int[gridSize, gridSize];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnQueue = new List<Vector2>();

        // 시작점을 맵의 정중앙으로 설정
        Vector2 centerPoint = new Vector2(mapSize / 2f, mapSize / 2f);
        spawnQueue.Add(centerPoint);

        while (spawnQueue.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnQueue.Count);
            Vector2 spawnCenter = spawnQueue[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < rejectionSamples; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                float dir = Random.Range(minDistance, 2 * minDistance);
                Vector2 candidate = spawnCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dir;

                if (IsValid(candidate, gridSize, cellSize, points, grid))
                {
                    points.Add(candidate);
                    spawnQueue.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted) spawnQueue.RemoveAt(spawnIndex);
        }
        return points;
    }

    bool IsValid(Vector2 candidate, int gridSize, float cellSize, List<Vector2> points, int[,] grid)
    {
        // 1. 맵 경계 체크
        if (candidate.x < 0 || candidate.x >= mapSize || candidate.y < 0 || candidate.y >= mapSize) return false;

        // 2. 중요: 지정된 반지름(holeRadius) "안쪽"에 있는지 체크
        Vector2 center = new Vector2(mapSize / 2f, mapSize / 2f);
        if (Vector2.Distance(candidate, center) > holeRadius) return false;

        // 3. 데코 간 최소 거리 체크
        int cellX = (int)(candidate.x / cellSize);
        int cellY = (int)(candidate.y / cellSize);

        for (int x = Mathf.Max(0, cellX - 2); x <= Mathf.Min(cellX + 2, gridSize - 1); x++)
        {
            for (int y = Mathf.Max(0, cellY - 2); y <= Mathf.Min(cellY + 2, gridSize - 1); y++)
            {
                int pointIndex = grid[x, y] - 1;
                if (pointIndex != -1)
                {
                    if (Vector2.Distance(candidate, points[pointIndex]) < minDistance) return false;
                }
            }
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        // 데코레이션 생성 영역 시각화 (녹색 원)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Vector3.zero, holeRadius);

        // 맵 전체 경계 시각화 (회색 사각형)
        Gizmos.color = new Color(1, 1, 1, 0.3f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapSize, 0, mapSize));
    }
}