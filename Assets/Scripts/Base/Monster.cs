using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using FSM;


public abstract class Monster : MonoBehaviour, MonsterStateController
{
    //몬스터들의 코드에서 사용할 속성들
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Collider2D col;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected ParticleSystem runDust;
    
    [SerializeField] protected float moveSpeed = 4f;
    [SerializeField] protected float bounceForce = 1f;
    
    //벽 및 바닥으로 인식할 레이어
    [SerializeField] LayerMask[] groundLayers;
    
    //현재 상태 및 상태머신
    protected IState currentState;
    protected StateMachine stateMachine;
    
    //몬스터가 공통적으로 갖는 상태
    public MonsterIdleState IdleState { get; private set; }
    public MonsterRunState RunState { get; private set; }
    public MonsterDeadState DeadState { get; private set; }
    
    //몬스터 컴포넌트 전달용
    public Animator Animator => animator;
    public Rigidbody2D Rb => rb;
    public Collider2D Col => col;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    
    protected int direction; //몬스터가 바라보고 이동하는 방향
    protected int combinedLayerMask; //합쳐진 레이어값들을 저장해두는 변수
    
    //상태 체크용 변수
    //다른 스크립트에서 코드를 작성하다가 실수로 사용하지 못하게 컨트롤러 통해서 관리
    bool MonsterStateController.IsWall => IsWall(); //벽 체크
    bool MonsterStateController.IsEdge => IsEdge(); //낭떠러지 체크
    int MonsterStateController.MonsterDirection { get => direction; set => direction = value; } //몬스터가 보는 방향
    
    public static event Action OnMonsterDefeated; //몬스터 킬 카운팅용 이벤트
    
    protected virtual void Awake()
    {
        //상태 머신, 상태 초기화
        stateMachine = new StateMachine();
        
        IdleState = new MonsterIdleState(this, stateMachine);
        RunState = new MonsterRunState(this, stateMachine);
        DeadState = new MonsterDeadState(this, stateMachine);
        
        //레이어 배열을 하나의 비트마스크로 통합
        combinedLayerMask = 0;
        foreach (LayerMask mask in groundLayers)
        {
            //OR 논리합
            combinedLayerMask |= mask.value;
        }
    }

    protected virtual void Start() { }

    protected virtual void Update()
    {
        stateMachine.Update();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    //플레이어와 부딪혔을 때의 로직
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //플레이어와 부딪힌 방향 및 플레이어 스크립트를 가져옴
            Vector2 normal = collision.GetContact(0).normal;
            Player player = collision.gameObject.GetComponent<Player>();
            
            //몬스터가 날아갈 방향 계산 (플레이어 위치 -> 몬스터 위치)
            Vector2 bounceDir = ((Vector2)(transform.position - collision.transform.position)).normalized;
            
            //만약 밟힌 방향이 -0.5f보다 작다면, 플레이어가 위에서 밟았다는 의미
            //플레이어 기준으로 공중 플랫폼을 밟은 방향의 위치가 메인이기 때문에 y는 +가 아니라 -인게 맞음
            if (normal.y < -0.5f)
            {
                //플레이어의 몬스터 처치 메소드 실행
                if (player != null)
                {
                    player.KillMonster(20f);
                }

                //Hit 판정 발생
                TakeHit(bounceDir);
                return;
            }
            
            //위에서 밟히지 않은 경우 플레이어 무적상태 확인
            if (player.IsInvincible)
            {
                TakeHit(bounceDir);
            }
            else
            {
                //플레이어가 무적이 아니라면 사망 트리거 호출
                TriggerDeath(collision.gameObject, collision.GetContact(0).point);
            }
        }
    }
    
    protected virtual void TakeHit(Vector2 bounceDir)
    {
        //몬스터 사망 관련 이벤트 세팅 후 호출
        DeadState.Setup(bounceDir);
        OnMonsterDefeated?.Invoke();

        //몬스터 사망 상태 변경
        stateMachine.ChangeState(DeadState);
    }
    
    //컨트롤러를 통해 실행될 메소드
    //몬스터 이동
    void MonsterStateController.ExecuteMove(float inputDirection)
    {
        rb.linearVelocity = new Vector2(inputDirection * moveSpeed, rb.linearVelocity.y);
    }
    
    //이동멈춤
    void MonsterStateController.ExecuteStop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    //방향전환
    void MonsterStateController.ExecuteTurn()
    {
        //방향 뒤집기
        direction *= -1;

        //보는 방향도 뒤집어줌
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    //방향전환
    void MonsterStateController.ExecuteDie(Vector2 bounceDir)
    {
        //트리거 및 애니메이션 변경
        col.isTrigger = true; 
        animator.Play("Hit");
        
        //디졸브 효과 적용
        StartCoroutine(DieRoutine());
        
        //몬스터 물리 효과
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.None;
        
        //몬스터는 플레이어 대비 옆보단 위쪽으로 튕겨지도록 연출
        bounceDir.y = Mathf.Abs(bounceDir.y) + 4f;
        rb.AddForce(bounceDir * 5f, ForceMode2D.Impulse); 
        rb.AddTorque(bounceDir.x * 5f, ForceMode2D.Impulse);
    }
    
    //디졸브 연출
    protected IEnumerator DieRoutine()
    {
        float duration = 1.0f;
        float elapsedTime = 0f;

        //몬스터에 적용된 머티리얼 가져오기
        Material mat = spriteRenderer.material;

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

        Destroy(gameObject);
    }
    
    //벽 판정 메소드
    protected bool IsWall()
    {
        //레이캐스트 레이저 시작점 및 길이 정하기
        Vector2 origin = new Vector2(
            col.bounds.center.x + (direction * (col.bounds.size.x / 2f)),
            col.bounds.center.y - 0.2f
        );
        float distance = 0.3f;

        //디버그용 선
        // Debug.DrawRay(origin, Vector2.right * direction * distance, Color.red);

        //레이어 마스크에 맞게 무언가 닿았으면 true 아니면 fasle 리턴
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, distance, combinedLayerMask);
        return hit.collider != null;
    }

    //낭떠러지 판정 메소드
    protected bool IsEdge()
    {
        //내 몸통의 앞쪽 맨 끝 좌표의 발 밑 구하기
        Vector2 origin = new Vector2(
            col.bounds.center.x + (direction * (col.bounds.size.x / 2f)),
            col.bounds.min.y + 0.1f
        );

        //바닥을 향해 쏠 레이저 길이
        float distance = 0.3f;

        //디버그용 선
        // Debug.DrawRay(origin, Vector2.down * distance, Color.blue);

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, combinedLayerMask);

        //벽이나 바닥이 아니라 아무것도 확인을 못했다면 낭떠러지라는 뜻
        return hit.collider == null;
    }

    //플레이어와 옆으로 부딪혀 플레이어를 죽이는 상황
    protected void TriggerDeath(GameObject playerObj, Vector2 contactPoint)
    {
        Player player = playerObj.GetComponent<Player>();
        if (player != null)
        {
            Vector2 dir = ((Vector2)player.transform.position - contactPoint).normalized;
            player.CallDeathEvent(dir * bounceForce);
        }
    }
    
    //달리는 파티클 출력용 메소드
    private void RunDust()
    {
        runDust.Emit(Random.Range(1,3));
    }
}