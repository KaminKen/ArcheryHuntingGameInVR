using UnityEngine;

public class HealthSphereUI : MonoBehaviour
{
    [SerializeField] private MonsterBase monster;   // drag the parent monster here
    [SerializeField] private float minScale = 0.05f; // so it never becomes invisible
    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;

        if (monster == null)
            monster = GetComponentInParent<MonsterBase>();
    }

    void LateUpdate()
    {
        if (monster == null) return;

        float hp01 = Mathf.Clamp01(monster.CurrentHealth / monster.maxHealth); // needs 1 small change below
        float s = Mathf.Lerp(minScale, 1f, hp01);

        transform.localScale = baseScale * s;
    }
}
