using UnityEngine;

public class ChickenMonster : Monster
{
    [SerializeField] private LayerMask playerLayer; //플레이어 레이어
    [SerializeField] private LayerMask groundLayer; //그라운드 레이어
    [SerializeField] private LayerMask platformLayer; //플랫폼 레이어
    
    [SerializeField] private float viewRadius = 15f; //몬스터의 시야 반경

    private float chaseSpeed = 9f; //돌격 속도
    private int combinedFloorMask; //그라운드, 플랫폼 레이어 통합용 변수
    private bool hasAggro = false; //플레이어 포착 여부
    private Transform targetPlayer; //플레이어 위치
    private int direction; //현재 방향

    protected override void Start()
    {
        base.Start();
        
        //닭은 기본적으로 대기상태
        currentState = State.Idle; 

        //현재 이미지가 바라보는 방향을 기준으로 초기 방향 설정
        //x가 0보다 높으면 왼쪽을 보고있다는 의미인데, 그럼 왼쪽으로 가야하니 -1
        direction = transform.localScale.x > 0 ? -1 : 1;

        //벽, 낭떠러지 센서용으로 레이어 통합해주기
        combinedFloorMask = groundLayer.value | platformLayer.value;
    }

    protected override void Think()
    {
        if (currentState == State.Dead) return;

        if (IsPlayerVisible())
        {
            //플레이어가 보인다면 추적 시작
            hasAggro = true;
            currentState = State.Chase;
            
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
        else
        {
            //시야에서 사라졌거나 벽 뒤로 숨으면 추적 포기
            hasAggro = false;
            currentState = State.Idle;
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

    protected override void Act()
    {
        if (currentState == State.Dead) return;

        //추적 상태라면
        if (hasAggro)
        {
            //달리는 도중 벽, 낭떠러지 체크
            if (!IsHittingWall() && !IsNearEdge())
            {
                //문제 없다면 계속 달림
                rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
                animator.SetBool("isRunning", true);
            }
            else
            {
                //벽이나 낭떠러지가 있다면 멈춤
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isRunning", false);
            }
        }
        else
        {
            //추적 상태가 아니라면 멈춤
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
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

    //벽 감지
    private bool IsHittingWall()
    {
        Vector2 origin = new Vector2(col.bounds.center.x + (direction * (col.bounds.size.x / 2f)), col.bounds.center.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, 0.3f, groundLayer);
        return hit.collider != null;
    }

    //낭떠러지 감지
    private bool IsNearEdge()
    {
        Vector2 origin = new Vector2(col.bounds.center.x + (direction * (col.bounds.size.x / 2f)), col.bounds.min.y + 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.3f, combinedFloorMask);
        return hit.collider == null; 
    }
}