using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using FSM;

public class Player : MonoBehaviour, PlayerStateController
{
    //각종 컴포넌트
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D collider;
    [SerializeField] GameObject collectedPrefab;
    [SerializeField] SpriteRenderer playerRenderer;
    [SerializeField] Material defaultRenderer;
    [SerializeField] Material invincibleRenderer;
    
    //파티클처리용
    [SerializeField] ParticleSystem runDust;
    [SerializeField] ParticleSystem dashDust;
    [SerializeField] ParticleSystem jumpDust;
    [SerializeField] ParticleSystem landingDust;
    [SerializeField] ParticleSystem wallSlipDust;
    [SerializeField] ParticleSystem wallJumpDust;
    [SerializeField] ParticleSystem invincibleDust;
    
    //이동 속도, 점프 강도, 벽에서 미끄러지는 강도
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float wallSlip;
    
    public static event Action<Vector2> OnPlayerDeath; //플레이어 죽음처리용 이벤트
    public static event Action OnItemCollected; //아이템 획득카운팅용 이벤트
    
    //============ 상태패턴용 ============
    //현재 상태 및 상태머신
    private IState currentState;
    private StateMachine stateMachine;
    
    //플레이어가 가질 수 있는 모든 상태
    public PlayerIdleState IdleState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerWallSlipState WallSlipState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }
    public PlayerDoubleJumpState DoubleJumpState { get; private set; }
    public PlayerHitState HitState { get; private set; }
    
    //상태패턴 개편용 새로운 변수들
    private bool isGrounded;
    private bool canDoubleJump;
    private bool canJump;
    private float xInput;
    private float yInput;
    private bool jumpInput;
    private bool dashInput;
    private Vector2 dashDirection;
    private int playerDirection;
    private Vector2 hitDirection;
    private bool playerDead;
    
    //애니메이션 프레임 카운팅용
    private readonly int frameCountPropertyId = Shader.PropertyToID("_FrameCount");

    //무적 상태변수
    //추후 별도 스크립트로 분리 가능
    private bool isInvincible = false;
    private float invincibleTime = 5.0f;
    //몬스터에서 플레이어 무적 상태 판단 가능하도록 프로퍼티
    public bool IsInvincible => isInvincible;
    //마지막 스프라이트 기억해두는 변수
    private Sprite lastSprite;
    
    //대시 제한용 변수
    private float dashCoolTime = 1.0f;
    private float dashTimer = 0f;
    
    //플레이어 컴포넌트 전달용
    public Animator Animator => animator;
    public Rigidbody2D Rb => rb;
    public BoxCollider2D Collider => collider;
    public SpriteRenderer PlayerRenderer => playerRenderer;
    public ParticleSystem DashDust => dashDust;
    public ParticleSystem LandingDust => landingDust;
    public ParticleSystem JumpDust => jumpDust;
    public ParticleSystem WallJumpDust => wallJumpDust;
    
    //상태 체크용 변수
    //다른 스크립트에서 코드를 작성하다가 실수로 사용하지 못하게 컨트롤러 통해서 관리
    bool PlayerStateController.IsGrounded => isGrounded; //땅 체크
    bool PlayerStateController.IsWall => IsWall(); //벽 체크
    bool PlayerStateController.CanJump { get => canJump; set => canJump = value; } //점프 가능 상태인지
    bool PlayerStateController.CanDoubleJump { get => canDoubleJump; set => canDoubleJump = value; } //더블 점프 가능 상태인지
    int PlayerStateController.PlayerDirection { get => playerDirection; set => playerDirection = value; } //플레이어 보는 방향
    float PlayerStateController.XInput => xInput; //이동(왼쪽 오른쪽) 입력값
    float PlayerStateController.YInput => yInput;
    bool PlayerStateController.JumpInput => jumpInput;
    bool PlayerStateController.DashInput => dashInput;
    bool PlayerStateController.CanDash => dashTimer <= 0;
    float PlayerStateController.JumpForce => jumpForce; //점프력
    float PlayerStateController.WallSlip => wallSlip; //벽 미끄러짐 정도
    Vector2 PlayerStateController.HitDirection => hitDirection; //무언가에 맞았을 때, 튕겨나갈 방향
    
    private void Awake()
    {
        //이벤트 구독처리
        //외부에서 이벤트 호출용 메소드를 호출하면 사망이벤트 진행
        OnPlayerDeath += HandleDeath;
        
        //상태 머신, 상태 초기화
        stateMachine = new StateMachine();
        
        IdleState = new PlayerIdleState(this, stateMachine);
        RunState = new PlayerRunState(this, stateMachine);
        DashState = new PlayerDashState(this, stateMachine);
        FallState = new PlayerFallState(this, stateMachine);
        JumpState = new PlayerJumpState(this, stateMachine);
        DoubleJumpState = new PlayerDoubleJumpState(this, stateMachine);
        WallJumpState = new PlayerWallJumpState(this, stateMachine);
        WallSlipState = new PlayerWallSlipState(this, stateMachine);
        HitState = new PlayerHitState(this, stateMachine);
    }

    private void Start()
    {
        stateMachine.ChangeState(IdleState);
    }

    private void Update()
    {
        //만약 죽은 상태면 무시
        if(stateMachine.CurrentState == HitState) return;
        
        //플레이어의 이동 입력 실시간으로 받기
        PlayerInput();
        
        //대시 쿨타임 처리
        if( dashTimer > 0 ) dashTimer -= Time.deltaTime;
        
        stateMachine.Update();
    }

    private void PlayerInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        jumpInput = Input.GetButtonDown("Jump");
        dashInput = Input.GetKeyDown(KeyCode.LeftShift);
    }
    
    private void FixedUpdate()
    {
        //땅 체크
        CheckGrounded();
        
        stateMachine.FixedUpdate();
    }
    
    //LateUpdate를 통해 스프라이트 변경 체크 후, 프레임 갱신
    //무적 상태 프레임 카운팅용
    private void LateUpdate()
    {
        //만약 죽은 상태면 무시
        if(stateMachine.CurrentState == HitState) return;
        
        // 무적 상태일 때만 체크합니다.
        if (isInvincible)
        {
            // 방금 전까지 기억하던 스프라이트와 현재 스프라이트가 다르다면?
            // (즉, 애니메이션이 바뀌었거나 프레임이 넘어갔다면)
            if (playerRenderer.sprite != lastSprite)
            {
                // 쉐이더 프레임을 갱신하고
                UpdateShaderFrameCount();
            
                // 현재 스프라이트를 '이전 스프라이트'로 덮어씌워 기억해 둡니다.
                lastSprite = playerRenderer.sprite;
            }
        }
    }
    
    //컨트롤러를 통해 실행될 메소드
    //이동처리
    void PlayerStateController.ExecuteMove(float inputDirection)
    {
        //입력받은 방향을 기준으로 이동
        rb.linearVelocity = new Vector2(inputDirection * moveSpeed, rb.linearVelocity.y);
        
        //이동 할 때 스프라이트 전환 처리 필요
        if (Mathf.Abs(inputDirection) > 0.1f)
        {
            FlipSprite(inputDirection); 
        }
    }
    
    //컨트롤러를 통해 실행될 메소드
    //일반, 더블점프 로직
    void PlayerStateController.ExecuteJump(float inputJumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, inputJumpForce);
    }

    //컨트롤러를 통해 실행될 메소드
    //대시 쿨타임 지정
    void PlayerStateController.ExecuteResetDashCooltime()
    {
        dashTimer = dashCoolTime;
    }

    //이미지 방향 전환
    private void FlipSprite(float direction)
    {        
        if (direction > 0f)
        {
            playerDirection = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (direction < 0f)
        {
            playerDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void CheckGrounded()
    {
        //만약 올라가는 도중이라면 착지할 수가 없음
        //플랫폼을 지날 때 추가로 점프가 되는걸 방지해줌
        if (rb.linearVelocity.y > 0.01f) 
        {
            isGrounded = false; 
            return; 
        }
        
        //박스캐스트 상자 크기
        Vector2 boxSize = new Vector2(collider.bounds.size.x * 0.95f, 0.05f);
        
        //캐스팅 시작 위치 및 거리
        Vector2 origin = new Vector2(collider.bounds.center.x, collider.bounds.min.y + 0.05f);
        float castDistance = 0.15f;

        //땅 확인
        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0f, Vector2.down, castDistance, LayerMask.GetMask("Ground", "Platforms"));

        //땅이 확인됐다면 true
        if (hit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    //벽 판정용 메소드
    private bool IsWall()
    {
        //보는 방향 바로앞에 그라운드 레이어 벽이 있다면 벽 판정
        if (Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.right * playerDirection, 0.03f,
                LayerMask.GetMask("Ground")))
        {
            return true;
        }

        return false;
    }

    //아이템 획득처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //만약 죽은 상태면 무시
        if(stateMachine.CurrentState == HitState) return;
        
        if (collision.gameObject.tag == "Item")
        {
            //Diamond로 시작하는지 확인, 추후 아이템 종류가 늘어나면 enum으로 종류 분리
            if (collision.gameObject.name.StartsWith("Diamond"))
            {
                ActiveInvincible(invincibleTime);
            }
            
            //추후 이펙트 오브젝트 자체를 없애줘야 함
            //스크립트 분리하면 그때 게임 매니저 등에서 처리
            GameObject effect = Instantiate(collectedPrefab, collision.transform.position, Quaternion.identity);
            
            //아이템 획득 관련 이벤트 호출
            OnItemCollected?.Invoke();

            //획득한 아이템 삭제
            Destroy(collision.gameObject);
            
            StartCoroutine(DestroyEffect(effect));
        }
    }

    private void ActiveInvincible(float time)
    {
        //무적 아이템을 연속으로 먹는 경우를 위해 stop 후 start
        StopCoroutine("InvincibilityRoutine");
        StartCoroutine("InvincibilityRoutine", time);
    }
    
    private IEnumerator InvincibilityRoutine(float time)
    {
        //무적 상태 활성화
        isInvincible = true;
        
        //머티리얼 교체 및 프레임 계산
        playerRenderer.material = invincibleRenderer; 
        UpdateShaderFrameCount();
        invincibleDust.Play();

        //지정된 시간만큼 대기
        yield return new WaitForSeconds(time);

        //무적상태 비활성화
        isInvincible = false;

        //머티리얼 복구
        playerRenderer.material = defaultRenderer;
        invincibleDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    
    //각종 상태패턴에서 애니메이션 변경될 때 마다 호출하는 메소드
    private void UpdateShaderFrameCount()
    {
        //만약 무적 상태가 아니라면
        if (!isInvincible) return;

        //원본 텍스처 가로 길이 / 현재 프레임 1장의 가로 길이 = 총 가로 프레임 개수
        float currentFrameCount = playerRenderer.sprite.texture.width / playerRenderer.sprite.rect.width;

        //적용된 무적 쉐이더의 _FrameCount 속성에 값을 밀어넣음
        playerRenderer.material.SetFloat(frameCountPropertyId, currentFrameCount);
    }

    private IEnumerator DestroyEffect(GameObject effect)
    {
        //약 1초 대기
        yield return new WaitForSeconds(1f);

        Destroy(effect);
    }
    
    //함정, 몬스터에서 이벤트를 호출하기 위해 사용하는 통로용 메소드
    public void CallDeathEvent(Vector2 bounceDir)
    {
        if (!isInvincible)
        {
            //무언가에 맞았는데 플레이어가 무적이 아니라면 이벤트 호출
            OnPlayerDeath?.Invoke(bounceDir);
        }
    }

    //사망처리 시작용 메소드
    private void HandleDeath(Vector2 bounceDir)
    {
        if (playerDead) return;
        if (isInvincible) return; //무적 상태라면 무시

        playerDead = true;
        hitDirection = bounceDir;
        
        //피격 상태는 메인 로직에서 직접 실행
        stateMachine.ChangeState(HitState);
    }
    
    //파티클 - 달리는 상황
    //애니메이션 이벤트라 코드 이전 못하는중
    //애니메이션 이벤트로 두지 말까?
    private void RunDust()
    {
        runDust.Emit(Random.Range(1,3));
    }
    
    //파티클 - 벽 슬라이딩
    private void WallSlipDust()
    {
        wallSlipDust.Play();
    }

    //몬스터 밟은 이후 더블점프 가능하게 변경
    public void KillMonster(float bounce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); //기존 낙하 속도 초기화
        rb.AddForce(Vector2.up * bounce, ForceMode2D.Impulse); //플레이어 위로 점프
        canDoubleJump = true;
        stateMachine.ChangeState(JumpState); //점프상태로 변경
    }

    //파괴될 때 이벤트 해제
    private void OnDestroy()
    {
        OnPlayerDeath -= HandleDeath;
    }
}