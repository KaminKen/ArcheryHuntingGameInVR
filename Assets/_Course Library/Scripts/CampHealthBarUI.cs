using UnityEngine;

public class CampHealthBarUI : MonoBehaviour
{
    private Vector3 fullScale;
    private Vector3 fullLocalPos;

    void Start()
    {
        fullScale = transform.localScale;
        fullLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        float hp01 = Mathf.Clamp01(gm.GetHealthPercent());

        // if it was disabled earlier, re-enable when hp > 0
        if (!gameObject.activeSelf && hp01 > 0f)
            gameObject.SetActive(true);

        if (hp01 <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        float x = fullScale.x * hp01;
        transform.localScale = new Vector3(x, fullScale.y, fullScale.z);

        float offset = (fullScale.x - x) * 0.5f;
        transform.localPosition = fullLocalPos - new Vector3(offset, 0f, 0f);
    }
}
