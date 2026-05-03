using UnityEngine;
using System.Collections;

namespace FSM
{
    public class PlayerHitState : PlayerState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Hit");
        
        public PlayerHitState(Player owner, StateMachine stateMachine) : base(owner, stateMachine) {}

        public override void OnEnter()
        {
            //사망 판정 메소드 실행
            PlayerHit();           
        }
        
        public override void OnUpdate()
        {
        }
        public override void OnFixedUpdate()
        {
        }
        public override void OnExit()
        {
        }

        private void PlayerHit()
        {
            //블록 통과를 위한 트리거 활성화
            owner.Collider.isTrigger = true;
            owner.Animator.Play(animHash);
        
            //디졸브를 위한 코루틴 실행
            //MonoBehaviour가 없기 때문에 owner를 통해서 실행해야함
            owner.StartCoroutine(DieRoutine());
        
            //기존 속도 제거, z축 잠금 해제
            owner.Rb.linearVelocity = Vector2.zero;
            owner.Rb.constraints = RigidbodyConstraints2D.None;
        
            //위로 튕겨져 나가는 힘
            Vector2 bounceDir = controller.HitDirection;
            bounceDir.y = Mathf.Abs(controller.HitDirection.y) + 2f;

            //충돌 방향의 반대로 튕겨져 나가는 힘
            owner.Rb.AddForce(bounceDir * 10f, ForceMode2D.Impulse);
        
            //캐릭터 회전처리
            owner.Rb.AddTorque(bounceDir.x * 5f, ForceMode2D.Impulse);
        }
        
        //디졸브 처리용 메소드
        private IEnumerator DieRoutine()
        {
            float duration = 1.0f; //사라지는데 걸리는 총 시간
            float elapsedTime = 0f;

            //플레이어에 적용된 머티리얼을 가져옴
            Material mat = owner.PlayerRenderer.material;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
            
                //시간에 따라 0에서 1로 부드럽게 변하는 값 계산 (Lerp)
                float dissolveValue = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            
                //쉐이더의 Dissolve 변수값 변경
                mat.SetFloat("_Dissolve", dissolveValue);
            
                //다음 프레임까지 대기
                yield return null;
            }

            //설정한 시간에 도달하면 오브젝트 파괴
            Object.Destroy(owner.gameObject);
        }
    }
}