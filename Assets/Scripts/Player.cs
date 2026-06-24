using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using FSM;

public class Player : MonoBehaviour, PlayerStateController
{
    public static Player Instance { get; private set; }
    
    //각종 컴포넌트
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D collider;
    [SerializeField] GameObject collectedPrefab;
    [SerializeField] SpriteRenderer playerRenderer;
    [SerializeField] Material defaultRenderer;
    [SerializeField] Material invincibleRenderer;
    [SerializeField] FSMGraph.FSMRuntimeGraph fsmGraphAsset; //상태머신 그래프
    [SerializeField] GameObject bombPrefab; //폭탄 프리팹
    
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
    private StateMachine<Player> stateMachine;
    
    //플레이어가 가질 수 있는 모든 상태
    public PlayerHitState HitState { get; private set; }
    
    //상태패턴 개편용 새로운 변수들
    private bool isGrounded;
    private bool canDoubleJump;
    private bool canJump;
    private float xInput;
    private float yInput;
    private bool isFall;
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
    
    //FSM 그래프를 위한 임시 프로퍼티
    public float MoveSpeed => moveSpeed;
    public bool IsGrounded => isGrounded; //땅 체크
    public bool IsWall => WallCheck(); //벽 체크
    public bool CanJump { get => canJump; set => canJump = value; } //점프 가능 상태인지
    public bool CanDoubleJump { get => canDoubleJump; set => canDoubleJump = value; } //더블 점프 가능 상태인지
    public int PlayerDirection { get => playerDirection; set => playerDirection = value; } //플레이어 보는 방향
    public float XInput => xInput; //이동(왼쪽 오른쪽) 입력값
    public float YInput => yInput; //위 아래 입력값
    public bool IsFall => Rb.linearVelocity.y <= -0.1f;
    public bool JumpInput => jumpInput;
    public bool DashInput => dashInput;
    public bool CanDash => dashTimer <= 0;
    public float JumpForce => jumpForce; //점프력
    public float WallSlip => wallSlip; //벽 미끄러짐 정도
    public Vector2 HitDirection => hitDirection; //무언가에 맞았을 때, 튕겨나갈 방향
    public bool IsFirstLanded { get; set; } = true;
    public bool IsDashFinished { get; set; }
    public bool IsEnemyStepped { get; set; }
    
    //벽의 반대방향 입력이 들어와도 일단 추락판정
    //왼쪽(-1)이나 오른쪽(1)을 볼 때 반대 입력이 들어왔다면, 두 입력을 곱하면 항상 음수가 된다.
    public bool IsOppositionMove => Mathf.Abs(xInput) > 0.1f && (xInput * playerDirection < 0);
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 이미 존재하므로, 새로 생성된 중복 오브젝트는 즉시 파괴
            Destroy(gameObject);
            return;
        }

        // 최초 생성된 인스턴스를 static 변수에 저장
        Instance = this;

        // 씬이 전환되어도 이 오브젝트(GameManager)를 파괴하지 않고 유지
        DontDestroyOnLoad(gameObject);
        
        //이벤트 구독처리
        //외부에서 이벤트 호출용 메소드를 호출하면 사망이벤트 진행
        OnPlayerDeath += HandleDeath;
    }

    private void Start()
    {
        // stateMachine.ChangeState(IdleState);
        
        if (fsmGraphAsset != null)
        {
            // 그래프 에셋(설계도)에게 나(this)를 주면, 내부의 CreateStateMachine 함수가 
            // 노드들을 읽어 완성된 상태 머신을 반환해 줍니다.
            stateMachine = fsmGraphAsset.CreateStateMachine(this);
        }
    }

    private void Update()
    {
        //만약 죽은 상태면 무시
        if(stateMachine.CurrentState == HitState) return;

        //땅 체크
        CheckGrounded();
        
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
        
        //F 입력 시 폭탄 소환
        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnBomb();
        }
    }
    
    private void FixedUpdate()
    {
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
    private bool WallCheck()
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

            //청크 매니저가 확인된다면
            if (ChunkManager.Instance != null)
            {
                //아이템의 위치를 기반으로 'D' 마킹
                ChunkManager.Instance.MarkObjectAsDestroyed(collision.transform.position);
            }
            
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
        
        var emission = invincibleDust.emission;
        emission.enabled = true;
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

    //폭탄을 생성하는 메소드
    private void SpawnBomb()
    {
        if (bombPrefab == null) return;

        Vector2 bombBoxSize = new Vector2(0.8f, 0.8f);
        float bombHalfSize = 0.4f;
        float maxSpawnDistance = 1.2f;
        float targetDistance;

        RaycastHit2D hit = Physics2D.BoxCast(collider.bounds.center, bombBoxSize, 0f, Vector2.right * playerDirection, maxSpawnDistance + bombHalfSize, LayerMask.GetMask("Ground"));

        //벽이 있는 경우
        if (hit.collider != null)
        {
            //벽의 거리가 폭탄의 절반 크기보다 작은 경우
            if (hit.distance < bombHalfSize)
            {
                //플레이어 위치에서 스폰
                targetDistance = 0f;
            }
            //벽의 거리가 폭탄의 절반 크기보다 큰 경우
            else
            {
                //벽까지의 거리
                targetDistance = hit.distance;
            }
        } 
        //벽이 없는 경우
        else
        {
            //최대 제한 거리
            targetDistance = maxSpawnDistance;
        }
        
        //플레이어 위치, 시선방향, 거리
        Vector3 finalSpawnPos = transform.position + new Vector3(playerDirection * targetDistance, 0, 0);
        Instantiate(bombPrefab, finalSpawnPos, Quaternion.identity);
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
        stateMachine.ChangeState(new PlayerHitState(this, stateMachine));
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
        IsEnemyStepped = true; //점프상태로 변경용 변수
    }

    //파괴될 때 이벤트 해제
    private void OnDestroy()
    {
        OnPlayerDeath -= HandleDeath;
    }
}