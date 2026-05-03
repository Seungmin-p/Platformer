using FSM;
using UnityEngine;

public class Chicken : Monster
{
    [SerializeField] private LayerMask playerLayer; //플레이어 레이어
    [SerializeField] private LayerMask groundLayer; //그라운드 레이어
    
    [SerializeField] private float viewRadius = 15f; //몬스터의 시야 반경

    private float chaseSpeed = 9f; //돌격 속도
    private Transform targetPlayer; //플레이어 위치
    
    public MonsterChickenStopState ChickenStopState { get; private set; }
    public MonsterChickenChaseState ChickenChaseState { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        
        //이동 속도 업데이트
        moveSpeed = chaseSpeed;
        ChickenStopState = new MonsterChickenStopState(this, stateMachine);
        ChickenChaseState = new MonsterChickenChaseState(this, stateMachine);
    }

    protected override void Start()
    {
        base.Start();
        
        //현재 이미지가 바라보는 방향을 기준으로 초기 방향 설정
        //x가 0보다 높으면 왼쪽을 보고있다는 의미인데, 그럼 왼쪽으로 가야하니 -1
        direction = transform.localScale.x > 0 ? -1 : 1;
        
        //닭 몬스터의 기본 상태 지정
        stateMachine.ChangeState(ChickenStopState);
    }

    protected override void Update()
    {
        base.Update();
        
        //만약 플레이어를 찾았다면
        if (IsPlayerVisible())
        {
            if (stateMachine.CurrentState != ChickenChaseState)
            {
                //현재 추적상태가 아니라면 추적상태로 변경
                stateMachine.ChangeState(ChickenChaseState);
            }
        }
        else
        {
            //플레이어가 시야에서 벗어났다면
            if (stateMachine.CurrentState != ChickenStopState)
            {
                //현재 멈춰있는 상태라 아니라면 멈춤 상태로 변경
                stateMachine.ChangeState(ChickenStopState);
            }
        }
    }

    public void ChickenChase()
    {
        //플레이어와 몬스터의 x축 거리 차이 계산
        float xDiff = targetPlayer.position.x - transform.position.x;
        
        //방향을 바꾸기 위해 필요한 거리
        float offset = 2f;

        if (xDiff > offset)
        {
            //플레이어가 오른쪽에 있는 경우
            SetDirection(1);
        }
        else if (xDiff < -offset)
        {
            //플레이어가 왼쪽에 있는 경우
            SetDirection(-1);
        }
    }
    
    //방향 전환용 메소드
    private void SetDirection(int newDir)
    {
        if (direction != newDir)
        {
            direction = newDir;
            Vector3 scale = transform.localScale;
            scale.x = (direction == 1) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    //시야각 로직
    private bool IsPlayerVisible()
    {
        //몬스터 기준 시야 범위 내에 플레이어가 들어왔는지 체크
        Collider2D hit = Physics2D.OverlapCircle(transform.position, viewRadius, playerLayer);

        //범위 내에 들어왔다면
        if (hit != null)
        {
            //이때, 플레이어와 몬스터 사이에 벽이 있는지 체크해야함
            RaycastHit2D wallHit = Physics2D.Linecast(transform.position, hit.transform.position, groundLayer);
                
            //걸리는 벽이 없다면 보인다는 의미
            if (wallHit.collider == null)
            {
                //플레이어 추적 시작할 수 있도록 true 반환
                targetPlayer = hit.transform;
                return true; 
            }
        }
        
        //보이지 않는 경우 false 반환
        return false;
    }
}