using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapSize = 50;         // 50 * 50 맵
    public float holeRadius = 20f;   // 나무가 생성되지 않을 중앙 원의 반지름

    [Header("Tree Settings")]
    public GameObject[] treePrefabs;
    public float minDistance = 2f;
    [Range(10, 50)] public int rejectionSamples = 30;

    private GameObject treeGroup;

    [ContextMenu("Generate Tree Map")]
    public void GenerateMap()
    {
        ClearMap();

        treeGroup = new GameObject("GeneratedTrees");
        treeGroup.transform.SetParent(this.transform);
        treeGroup.transform.localPosition = Vector3.zero;

        List<Vector2> points = GeneratePointsData();

        foreach (Vector2 point in points)
        {
            if (treePrefabs == null || treePrefabs.Length == 0) break;

            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
            Vector3 spawnPos = new Vector3(point.x - mapSize / 2f, 0, point.y - mapSize / 2f);

            GameObject tree = Instantiate(prefab, transform.position + spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0), treeGroup.transform);

            // 자연스러움을 위해 크기 랜덤 조절 추가
            float randomScale = Random.Range(0.8f, 1.3f);
            tree.transform.localScale = Vector3.one * randomScale;
        }

        Debug.Log($"{points.Count}개의 나무가 외곽에 생성되었습니다.");
    }

    [ContextMenu("Clear Tree Map")]
    public void ClearMap()
    {
        Transform existingGroup = transform.Find("GeneratedTrees");
        if (existingGroup != null) DestroyImmediate(existingGroup.gameObject);
    }

    List<Vector2> GeneratePointsData()
    {
        float cellSize = minDistance / Mathf.Sqrt(2);
        int gridSize = Mathf.Max(1, Mathf.CeilToInt(mapSize / cellSize));
        int[,] grid = new int[gridSize, gridSize];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnQueue = new List<Vector2>();

        // 시작점을 맵의 모서리 부근으로 설정 (중앙은 비워야 하므로)
        Vector2 startPoint = new Vector2(1f, 1f);
        spawnQueue.Add(startPoint);

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
        // 1. 맵 사각형 경계 체크 (0 ~ mapSize)
        if (candidate.x < 0 || candidate.x >= mapSize || candidate.y < 0 || candidate.y >= mapSize) return false;

        // 2. 핵심 수정: 중앙 원 안에 있으면 거부 (반지름보다 작으면 false)
        Vector2 center = new Vector2(mapSize / 2f, mapSize / 2f);
        if (Vector2.Distance(candidate, center) < holeRadius) return false;

        // 3. 나무 간 거리 체크
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

        // 나무가 생성되지 않는 중앙 금지 구역 (빨간색 원)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, holeRadius);

        // 맵 전체 경계 (흰색 사각형)
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapSize, 0, mapSize));
    }
}