using UnityEngine;

public class Trap : MonoBehaviour
{
    //각 객체별로 튕겨져나가는 힘을 조절하기 위한 변수
    [SerializeField] protected float bounceForce = 1f;

    //플레이어의 죽음 이벤트 호출을 위한 메소드
    protected void TriggerDeath(GameObject playerObj, Vector2 contactPoint)
    {
        Player player = playerObj.GetComponent<Player>();
        if (player != null)
        {
            //튕겨져나갈 방향 계산
            Vector2 dir = ((Vector2)player.transform.position - contactPoint).normalized;
            //플레이어 이벤트 호출
            player.CallDeathEvent(dir * bounceForce);
        }
    }
}
