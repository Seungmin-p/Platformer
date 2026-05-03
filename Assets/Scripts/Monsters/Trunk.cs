using System.Collections;
using UnityEngine;
using FSM;

public class Trunk : Monster
{
    [SerializeField] LayerMask playerLayer; //플레이어의 레이어
    [SerializeField] GameObject projectilePrefab; //투사체 프리팹
    [SerializeField] Transform firePoint; //투사체 발사 위치, 좌표계산으로도 가능하지만 빈 오브젝트로 위치 잡아보기
    private float sightRange = 30f;  //몬스터 시야 거리
    [SerializeField] float attackCooldown = 1.1f; //공격 딜레이
    private float attackTimer;
    
    public MonsterTrunkAttackState TrunkAttackState { get; private set; }


    protected override void Awake()
    {
        base.Awake();
        
        TrunkAttackState = new MonsterTrunkAttackState(this, stateMachine);
    }
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

        //나무 몬스터의 기본 상태 지정
        stateMachine.ChangeState(RunState);
    }

    protected override void Update()
    {
        base.Update();
        
        //이미 죽은 경우면 되돌아감
        if(stateMachine.CurrentState == DeadState) return;

        if (attackTimer > 0.0) attackTimer -= Time.deltaTime;

        //공격 상태인데, 공격 쿨타임이 지났다면
        if (attackTimer <= 0.0)
        {
            //플레이어를 찾은 경우 재공격
            if (FoundPlayer())
            {
                //타이머 초기화 후, 공격 상태 변환
                attackTimer = attackCooldown;
                stateMachine.ChangeState(TrunkAttackState);
            }
            else
            {
                //만약 기존에 공격상태였다면
                if (stateMachine.CurrentState == TrunkAttackState)
                {
                    //플레이어를 못찾았다면 다시 움직이기
                    stateMachine.ChangeState(RunState);
                }
            }
        }       
    }

    //플레이어 감지용 메소드
    private bool FoundPlayer()
    {
        //몸통 중앙에서 앞쪽으로 시야만큼 레이저 발사
        Vector2 origin = col.bounds.center;
        
        //시야 범위 확인용 선
        // Debug.DrawRay(origin, Vector2.right * direction * sightRange, Color.yellow);

        //플레이어 레이어가 확인됐다면 true 반환
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, sightRange, playerLayer);
        return hit.collider != null;
    }

    //공격 상태에서 호출할 공격 패턴
    public void TrunkAttack()
    {
        //투사체 생성
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject seed = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            
            TrunkBullet BulletScript = seed.GetComponent<TrunkBullet>();
            
            //스크립트를 성공적으로 찾았다면, 나무의 현재 방향(direction)을 전달해 발사처리
            if (BulletScript != null)
            {
                BulletScript.Fire(direction); 
            }
        }
    }
}