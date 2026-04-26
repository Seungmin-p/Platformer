using UnityEngine;

public class Rockhead : Trap
{
    [SerializeField] Animator anim;
    [SerializeField] TrapMovement trapMovement;
    [SerializeField] ParticleSystem crashDust;
    private float crushThreshold = 0.9f;
    private float hitWaitTime = 1f; //벽에 부딪힌 후 대기시간
    private string currentState = "Idle";
    
    private void Awake()
    {
        //포인트 도착 시 호출을 위한 이벤트 등록
        if (trapMovement != null)
        {
            trapMovement.OnWaypointReached += HandleWaypointReached;
        }
    }
    
    private void Update()
    {
        //이동 방향이 있고, 멈춰있는 상태가 아닐 때
        if (trapMovement.MoveDirection != Vector3.zero)
        {
            UpdateAnimationState("Rockhead_Blink");
        }
    }
    
    //벽 도착 시 이벤트에 의해 실행되는 메소드
    private void HandleWaypointReached()
    {
        //방향 기억해두기
        Vector3 hitDir = trapMovement.MoveDirection;
        
        //멈춤처리
        trapMovement.Pause(hitWaitTime);

        //기존 이동 방향으로 애니메이션 처리
        PlayAnimation(hitDir);
    }

    private void PlayAnimation(Vector3 dir)
    {
        //어느 방향으로 이동중인지 체크
        //부딪히는 방향에 따라서 파티클 각도처리 및 애니메이션 실행
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0.1f)
            {
                crashDust.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                UpdateAnimationState("Rockhead_RightHit");
            }
            else if (dir.x < -0.1f)
            {
                crashDust.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
                UpdateAnimationState("Rockhead_LeftHit");
            }
            
        }
        else
        {
            if (dir.y > 0.1f)
            {
                crashDust.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                UpdateAnimationState("Rockhead_TopHit");
            }
            else if (dir.y < -0.1f)
            {
                crashDust.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                UpdateAnimationState("Rockhead_BottomHit");
            }
        }
        
        crashDust.Play();
    }

    private void UpdateAnimationState(string newState)
    {
        if (currentState == newState) return; 
        anim.Play(newState);
        currentState = newState;
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //멈춰있는 경우는 아무렇지 않아야함
            if (trapMovement.MoveDirection == Vector3.zero) return;
            
            //블록 이동 방향
            Vector2 pushDir = trapMovement.MoveDirection.normalized;
            
            //플레이어 중심 좌표
            Vector2 playerPos = collision.transform.position;
            
            //레이저 길이
            float checkDist = 0.7f;
            Debug.DrawRay(playerPos, pushDir * checkDist, Color.red);

            //레이저를 벽에만 반응하도록 처리
            RaycastHit2D hit = Physics2D.Raycast(playerPos, pushDir, checkDist, LayerMask.GetMask("Ground"));
            
            //무언가 감지 됐다면
            if (hit.collider != null)
            {
                //압사처리
                TriggerDeath(collision.gameObject, hit.point);
            }
        }
    }
    
    private void OnDestroy()
    {
        if (trapMovement != null)
        {
            trapMovement.OnWaypointReached -= HandleWaypointReached;
        }
    }
}
