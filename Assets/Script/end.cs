using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("설정")]
    public GameObject[] prefabs;
    public Transform targetObject;
    public float yOffset = 2.0f;

    void Start()
    {
        SpawnAndSetup();
    }

    void SpawnAndSetup()
    {
        if (prefabs.Length == 0 || targetObject == null) return;

        int randomIndex = Random.Range(0, prefabs.Length);
        Vector3 spawnPosition = targetObject.position + Vector3.up * yOffset;

        // 1. 프리팹 생성 후 변수에 담기
        GameObject spawnedObj = Instantiate(prefabs[randomIndex], spawnPosition, Quaternion.identity);

        // 2. 리지드바디(Rigidbody) 추가 및 설정
        if (!spawnedObj.GetComponent<Rigidbody>()) // 이미 있다면 중복 추가 방지
        {
            Rigidbody rb = spawnedObj.AddComponent<Rigidbody>();
            rb.mass = 1.0f; // 무게 설정 (선택 사항)
            // rb.useGravity = true; // 중력 사용 여부
        }

        // 3. 콜라이더(BoxCollider) 추가
        // 오브젝트 형태에 따라 MeshCollider나 SphereCollider로 변경 가능합니다.
        if (!spawnedObj.GetComponent<Collider>())
        {
            spawnedObj.AddComponent<BoxCollider>();
        }

        Debug.Log($"{spawnedObj.name}에 물리 컴포넌트가 추가되었습니다.");
    }
}