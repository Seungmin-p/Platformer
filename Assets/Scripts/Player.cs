using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D collider;
    [SerializeField] GameObject collectedPrefab;
    [SerializeField] SpriteRenderer playerRenderer;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float wallSlip;

    private float horizontalInput;
    private bool isDoubleJump;
    private bool canJump;
    private int playerDirection;
    private bool playerDead;
    
    public event Action<Vector2> OnPlayerDeath;
    
    private void Awake()
    {
        //이벤트 구독처리
        //외부에서 이벤트 호출용 메소드를 호출하면 사망이벤트 진행
        OnPlayerDeath += HandleDeath;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && (canJump || IsWall()) )
        {
            Vector2 jumpPos = new Vector2(rb.linearVelocity.x, jumpForce);

            if (!IsGrounded())
            {
                if (IsWall())
                {
                    //이때 벽에서 밀어주면서 점프해야함
                    //이후 더블점프도 가능
                    CanDoubleJump();

                    //붙은 벽의 반대방향으로 밀려야함
                    jumpPos = new Vector2(jumpForce * (playerDirection * -1), jumpForce);
                }
                else
                {
                    isDoubleJump = true;
                    canJump = false;
                }
            }

            rb.linearVelocity = jumpPos;
        }

        FlipSprite();
        UpdateAnimationState();
    }

    //더블점프 초기화용 메소드
    public void CanDoubleJump()
    {
        isDoubleJump = false;
        canJump = true;
    }

    private void FixedUpdate()
    {
        if( playerDead ) return;
        
        //벽점프 밀어내기를 위한 예외처리
        if (!IsWall())
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        if (IsWall() && !IsJumping() )
        {
            WallSlip();
        }
    }

    //이미지 방향 전환
    private void FlipSprite()
    {
        if (horizontalInput > 0f)
        {
            playerDirection = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalInput < 0f)
        {
            playerDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    //애니메이션 전환용 메소드
    private void UpdateAnimationState()
    {
        bool isRunning = Mathf.Abs(horizontalInput) > 0.1f;
        animator.SetBool("isRunning", isRunning);

        animator.SetBool("isGrounded", IsGrounded());

        animator.SetBool("isJumping", IsJumping());

        animator.SetBool("isFalling", IsFalling());

        animator.SetBool("DoubleJump", isDoubleJump);

        animator.SetBool("isWall", IsWall());
        
        animator.SetBool("isDead", playerDead);
    }

    private bool IsJumping()
    {
        return rb.linearVelocity.y > 0;
    }

    private bool IsFalling()
    {
        if (!IsJumping())
        {
            //애니메이션 해제용
            isDoubleJump = false;
            return true;
        }

        return false;
    }

    private bool IsGrounded()
    {
        //만약 올라가는 도중이라면 착지할 수가 없음
        //플랫폼을 지날 때 추가로 점프가 되는걸 방지해줌
        if (rb.linearVelocity.y > 0.01f) return false; 
        
        //박스의 크기를 넓고 얇게 정해줌
        Vector2 boxSize = new Vector2(collider.bounds.size.x * 0.8f, 0.1f);

        //박스의 위치를 플레이어의 가장 아래로 정해줌
        Vector2 startPos = new Vector2(collider.bounds.center.x, collider.bounds.min.y);

        //발바닥에서 아래로 박스캐스팅 진행
        //부딪힌 무언가의 레이어가 그라운드, 플랫폼인 경우 착지판정
        if (Physics2D.BoxCast(startPos, boxSize, 0f, Vector2.down, 0.1f, LayerMask.GetMask("Ground", "Platforms")))
        {
            canJump = true;
            return true;
        }

        //아니라면 착지하지 않은 상태
        return false;
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

    //벽에서 미끄러지는 메소드
    private void WallSlip()
    {
        rb.linearVelocity = new Vector2(0f, wallSlip * -1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            //추후 이펙트 오브젝트 자체를 없애줘야 함
            //스크립트 분리하면 그때 게임 매니저 등에서 처리
            Instantiate(collectedPrefab, collision.transform.position, Quaternion.identity);

            collision.gameObject.SetActive(false);
        }
    }
    
    //사망처리 시작용 메소드
    private void HandleDeath(Vector2 bounceDir)
    {
        if (playerDead) return;
        
        //기존에 만들어둔 사망 로직 실행
        GameOver(bounceDir);
    }

    //함정, 몬스터에서 이벤트를 호출하기 위해 사용하는 통로용 메소드
    public void CallDeathEvent(Vector2 bounceDir)
    {
        //이벤트 호출
        OnPlayerDeath?.Invoke(bounceDir);
    }

    //사망처리용 메소드
    private void GameOver(Vector2 bounceDir)
    {
        //플레이어 사망처리, 블록 통과를 위한 트리거 활성화, 애니메이션 처리를 위한 트리거 설정
        playerDead = true;
        collider.isTrigger = true;
        animator.SetTrigger("onHit");
        
        //디졸브를 위한 코루틴 실행
        StartCoroutine(DieRoutine());
        
        //기존 속도 제거, z축 잠금 해제
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.None;
        
        //위로 튕겨져 나가는 힘
        bounceDir.y = Mathf.Abs(bounceDir.y) + 2f;

        //충돌 방향의 반대로 튕겨져 나가는 힘
        rb.AddForce(bounceDir * 10f, ForceMode2D.Impulse);
        
        //캐릭터 회전처리
        rb.AddTorque(bounceDir.x * 5f, ForceMode2D.Impulse);
    }
    
    //디졸브 처리용 메소드
    private IEnumerator DieRoutine()
    {
        float duration = 1.0f; //사라지는데 걸리는 총 시간
        float elapsedTime = 0f;

        //플레이어에 적용된 머티리얼을 가져옴
        Material mat = playerRenderer.material;

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
        Destroy(gameObject);
    }
}