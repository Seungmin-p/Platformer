using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D collider;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float wallSlip;
    
    private float horizontalInput;
    private bool isDoubleJump;
    private bool canJump;
    private int playerDirection;
    
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && canJump)
        {
            Vector2 jumpPos = new Vector2(rb.linearVelocity.x, jumpForce);
            
            if (! IsGrounded() )
            {
                if (IsWall())
                {
                    //이때 벽에서 밀어주면서 점프해야함
                    //이후 더블점프도 가능
                    isDoubleJump = false;
                    canJump = true;
                    
                    //붙은 벽의 반대방향으로 밀려야함
                    jumpPos = new Vector2(jumpForce * ( playerDirection * -1 ), jumpForce);
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
        //벽점프 밀어내기를 위한 예외처리
        if ( ! IsWall() )
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
            
        }

        if (IsWall() && ! IsJumping())
        {
            WallSlip();
        }
    }

    private void FlipSprite()
    {
        if (horizontalInput > 0f)
        {
            playerDirection = 1;
            transform.localScale = new Vector3(1,1,1);
        }
        else if (horizontalInput < 0f)
        {
            playerDirection = -1;
            transform.localScale = new Vector3(-1,1,1);
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
    }

    private bool IsJumping()
    {
        return rb.linearVelocity.y > 0;
    }
    
    private bool IsFalling()
    {
        if ( ! IsJumping() )
        {
            //애니메이션 해제용
            isDoubleJump = false;
            return true;
        }
        
        return false;
    }

    private bool IsGrounded()
    {
        if (Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, 0.1f, LayerMask.GetMask("Ground")))
        {
            canJump = true;
            return true;
        }
        
        return false;
    }

    private bool IsWall()
    {
        if (Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.right * playerDirection, 0.03f, LayerMask.GetMask("Ground")))
        {
            return true;
        }
        
        return false;
    }

    private void WallSlip()
    {
        rb.linearVelocity = new Vector2(0f, wallSlip * -1);
    }
}
