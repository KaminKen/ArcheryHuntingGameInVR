using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("설정")]
    public GameObject[] prefabs;
    public Transform targetObject;
    public float yOffset = 2.0f;

    void Start()
    {
        SpawnRandomObject();
    }

    void SpawnRandomObject()
    {
        if (prefabs.Length == 0 || targetObject == null) return;

        int randomIndex = Random.Range(0, prefabs.Length);
        Vector3 spawnPosition = targetObject.position + new Vector3(0, yOffset, 0);

        // 1. 객체를 생성하고 변수에 할당합니다.
        GameObject spawnedObject = Instantiate(prefabs[randomIndex], spawnPosition, Quaternion.identity);

        // 2. 리지드바디가 이미 있는지 확인하고, 없으면 추가합니다.
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = spawnedObject.AddComponent<Rigidbody>();
            Debug.Log($"{spawnedObject.name}에 Rigidbody를 새로 추가했습니다.");
        }

        // 3. (선택 사항) 추가적인 물리 설정을 여기서 할 수 있습니다.
        // rb.mass = 2.0f;
        // rb.useGravity = true;
    }
}