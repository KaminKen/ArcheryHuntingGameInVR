using UnityEngine;



public class DeerCollision : MonoBehaviour

{

    private void OnTriggerEnter2D(Collider2D collision)

    {

        // 충돌한 물체의 태그가 "Arrow"라면 (화살 프리팹에 Tag를 설정하세요)

        if (collision.CompareTag("Arrow"))

        {

            Debug.Log("사슴이 화살에 맞았습니다!");



            // 화살 제거 (선택 사항)

            Destroy(collision.gameObject);



            // 매니저에게 암전 및 씬 전환 요청

            opencontroller.Instance.OnDeerHit();

        }

    }

}