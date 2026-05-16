using UnityEngine;

namespace FSM
{
    public class MonsterChickenChaseState : MonsterState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int runAnimHash = Animator.StringToHash("Run");
        private static readonly int idleAnimHash = Animator.StringToHash("Idle");
        private Chicken chicken; //Chicken 본체를 담는 변수
        
        private bool isRunning;

        public MonsterChickenChaseState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
            //owner를 Chicken으로 형변환 해서 저장
            chicken = (Chicken)owner;
        }

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            chicken.Animator.Play(runAnimHash);
            isRunning = true;
        }
        public override void OnUpdate()
        {
            //플레이어 방향 실시간 추적
            chicken.ChickenChase();
            
            //벽이나 낭떠러지가 앞에 있는 경우
            if (controller.IsWall || controller.IsEdge)
            {
                //기존에 뛰고있었다면
                if (isRunning)
                {
                    //추적 상태는 유지하되 제자리에서 멈춤
                    controller.ExecuteStop();
                    chicken.Animator.Play(idleAnimHash); 
                    isRunning = false;
                }
            }
            else
            {
                //눈 앞에 방해되는게 없다면
                
                //플레이어 위치에 따른 방향은 조절되니까, 단순하게 방향대로 쭉 뛰어감
                controller.ExecuteMove(controller.MonsterDirection);
                
                //기존에 뛰지 않고 있었다면
                if (!isRunning)
                {
                    chicken.Animator.Play(runAnimHash);
                    isRunning = true;
                }
            }
        }
        public override void OnFixedUpdate()
        {
        }
        public override void OnExit()
        {
        }
    }
}