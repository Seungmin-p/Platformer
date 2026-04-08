using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D collider;
    [SerializeField] GameObject collectedPrefab;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float wallSlip;

    private float horizontalInput;
    private bool isDoubleJump;
    private bool canJump;
    private int playerDirection;
    private bool playerDead;

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && canJump)
        {
            Vector2 jumpPos = new Vector2(rb.linearVelocity.x, jumpForce);

            if (!IsGrounded())
            {
                if (IsWall())
                {
                    //이때 벽에서 밀어주면서 점프해야함
                    //이후 더블점프도 가능
                    isDoubleJump = false;
                    canJump = true;

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
        if (Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, 0.1f,
                LayerMask.GetMask("Ground")))
        {
            canJump = true;
            return true;
        }

        return false;
    }

    private bool IsWall()
    {
        if (Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.right * playerDirection, 0.03f,
                LayerMask.GetMask("Ground")))
        {
            return true;
        }

        return false;
    }

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Trap")
        {
            //Trap과 충돌한 정확한 위치 가져오기
            //0은 가장 먼저 부딪힌 첫번째 포인트를 의미함
            //.point를 통해 부딪힌 위치의 정확한 월드좌표 X,Y를 가져옴
            Vector2 contactPoint = collision.GetContact(0).point;

            //내 캐릭터의 중심 위치 - 부딪힌 정확한 포인트의 벡터연산으로 부딪힌 좌표에서 캐릭터를 향하는 방향을 구함
            //.normalized를 통해 정규화 진행
            Vector2 knockbackDir = ((Vector2)transform.position - contactPoint).normalized;

            //구한 knockbackDir 값을 전달
            GameOver(knockbackDir);
        }
    }

    private void GameOver(Vector2 bounceDir)
    {
        //플레이어 사망처리, 블록 통과를 위한 트리거 활성화, 애니메이션 처리를 위한 트리거 설정
        playerDead = true;
        collider.isTrigger = true;
        animator.SetTrigger("onHit");
        
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
}