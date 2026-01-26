using System.Collections.Generic;
using UnityEngine;

public class SemicircleTreeGenerator : MonoBehaviour
{
    [Header("Semicircle Settings")]
    public float maxRadius = 50f;
    public float holeRadius = 5f;
    [Range(0, 360)] public float rotationAngle = 0f; // 반원의 중심 방향 (0: 정면/Z축, 90: 오른쪽/X축)

    [Header("Tree Settings")]
    public GameObject[] treePrefabs;
    public float minDistance = 3f;
    [Range(10, 50)] public int rejectionSamples = 30;

    private GameObject treeGroup;

    [ContextMenu("Generate Rotated Semicircle")]
    public void GenerateMap()
    {
        ClearMap();
        treeGroup = new GameObject("GeneratedTrees_Semicircle");
        treeGroup.transform.SetParent(this.transform);
        treeGroup.transform.localPosition = Vector3.zero;

        List<Vector2> points = GeneratePointsData();

        foreach (Vector2 point in points)
        {
            if (treePrefabs == null || treePrefabs.Length == 0) break;

            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
            Vector3 spawnPos = new Vector3(point.x, 0, point.y);

            GameObject tree = Instantiate(prefab, transform.position + spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0), treeGroup.transform);
            tree.transform.localScale = Vector3.one * Random.Range(0.8f, 1.3f);
        }
    }

    public void ClearMap()
    {
        Transform existingGroup = transform.Find("GeneratedTrees_Semicircle");
        if (existingGroup != null) DestroyImmediate(existingGroup.gameObject);
    }

    List<Vector2> GeneratePointsData()
    {
        float cellSize = minDistance / Mathf.Sqrt(2);

        // 어떤 방향으로든 회전할 수 있으므로, 전체 원을 포함하는 정사각형 그리드를 만듭니다.
        int gridSize = Mathf.CeilToInt((maxRadius * 2) / cellSize);
        int[,] grid = new int[gridSize, gridSize];

        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnQueue = new List<Vector2>();

        // 시작점: 반원 방향 벡터의 절반 지점 (안전하게 내부에서 시작)
        Vector2 dir = GetDirectionVector();
        Vector2 startPoint = dir * (maxRadius / 2f);

        spawnQueue.Add(startPoint);

        while (spawnQueue.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnQueue.Count);
            Vector2 spawnCenter = spawnQueue[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < rejectionSamples; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                float dist = Random.Range(minDistance, 2 * minDistance);
                Vector2 candidate = spawnCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

                if (IsValid(candidate, gridSize, cellSize, points, grid))
                {
                    points.Add(candidate);
                    spawnQueue.Add(candidate);

                    int gx = (int)((candidate.x + maxRadius) / cellSize);
                    int gy = (int)((candidate.y + maxRadius) / cellSize);
                    grid[gx, gy] = points.Count;

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
        // 1. 기본 거리 및 중심 구멍 체크
        float magnitude = candidate.magnitude;
        if (magnitude > maxRadius || magnitude < holeRadius) return false;

        // 2. 방향 체크 (내적 사용)
        // 후보 지점의 방향 벡터와 반원 설정 방향 벡터 사이의 각도가 90도(PI/2) 이내여야 함
        Vector2 centerDir = GetDirectionVector();
        if (Vector2.Dot(candidate.normalized, centerDir) < 0) return false;

        // 3. 그리드 범위 및 거리 체크
        int gx = (int)((candidate.x + maxRadius) / cellSize);
        int gy = (int)((candidate.y + maxRadius) / cellSize);
        if (gx < 0 || gx >= gridSize || gy < 0 || gy >= gridSize) return false;

        for (int x = Mathf.Max(0, gx - 2); x <= Mathf.Min(gx + 2, gridSize - 1); x++)
        {
            for (int y = Mathf.Max(0, gy - 2); y <= Mathf.Min(gy + 2, gridSize - 1); y++)
            {
                int pointIndex = grid[x, y] - 1;
                if (pointIndex != -1 && Vector2.Distance(candidate, points[pointIndex]) < minDistance) return false;
            }
        }
        return true;
    }

    Vector2 GetDirectionVector()
    {
        // rotationAngle을 라디안으로 변환하여 방향 벡터 반환 (유니티 좌표계 기준 Z축이 0도)
        float rad = (90f - rotationAngle) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Vector2 dir = GetDirectionVector();

        Gizmos.color = Color.cyan;
        // 반원 가이드 그리기
        int segments = 30;
        float startAngle = (90f - rotationAngle - 90f) * Mathf.Deg2Rad;
        Vector3 lastPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + (i / (float)segments) * Mathf.PI;
            Vector3 pos = new Vector3(Mathf.Cos(currentAngle), 0, Mathf.Sin(currentAngle)) * maxRadius;
            if (i > 0) Gizmos.DrawLine(lastPoint, pos);
            else Gizmos.DrawLine(Vector3.zero, pos); // 시작 선
            lastPoint = pos;
        }
        Gizmos.DrawLine(lastPoint, Vector3.zero); // 마지막 선
    }
}