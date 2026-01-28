using UnityEngine;

public class HealthBar3D : MonoBehaviour
{
    [SerializeField] private MonsterBase monster;
    //[SerializeField] private float minFill = 0.05f;

    private Vector3 fullScale;
    private Vector3 fullLocalPos;

    void Start()
    {
        fullScale = transform.localScale;
        fullLocalPos = transform.localPosition;

        if (monster == null)
            monster = GetComponentInParent<MonsterBase>();
    }

    void LateUpdate()
    {
        if (monster == null) return;

        float hp01 = Mathf.Clamp01(monster.CurrentHealth / monster.maxHealth);

        // Hide the bar completely at 0 HP
        if (hp01 <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        float x = fullScale.x * hp01;
        transform.localScale = new Vector3(x, fullScale.y, fullScale.z);

        // keep left side anchored
        float offset = (fullScale.x - x) * 0.5f;
        transform.localPosition = fullLocalPos - new Vector3(offset, 0f, 0f);
    }

}
