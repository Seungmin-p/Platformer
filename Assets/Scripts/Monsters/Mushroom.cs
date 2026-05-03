using UnityEngine;

public class Mushroom : Monster
{
    protected override void Start()
    {
        //나중에 추가될 로직을 위해 미리 연결
        base.Start();
        
        //초기 방향 랜덤 설정 (50% 확률로 1 또는 -1)
        direction = Random.value > 0.5f ? 1 : -1;
        
        //초기 방향에 따른 바라보는 방향 설정
        Vector3 scale = transform.localScale;
        scale.x = (direction == 1) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        //버섯 몬스터의 기본 상태 지정
        stateMachine.ChangeState(RunState);
    }
}