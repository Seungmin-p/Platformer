using System.Collections;
using UnityEngine;

public class Mushroom : Monster
{
    //낭떠러지, 벽을 만났을 때 기다리는 시간
    [SerializeField] float waitTime = 1.0f; 
    
    //벽 및 바닥으로 인식할 레이어
    [SerializeField] LayerMask[] groundLayers; 

    protected int direction; //처음 이동할 방향용 변수
    private bool isWaiting = false; //현재 쉬고 있는 상태인지 체크하는 변수
    private int combinedLayerMask; //합쳐진 레이어값들을 저장해두는 변수

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

        //레이어 배열을 하나의 비트마스크로 통합
        combinedLayerMask = 0;
        foreach (LayerMask mask in groundLayers)
        {
            //OR 논리합
            combinedLayerMask |= mask.value;
        }
        
        //버섯 몬스터의 기본 상태를 순찰로 변경
        currentState = State.Patrol; 
    }

    //상황 판단
    protected override void Think()
    {
        //쉬는 중이면 생각할 필요 없음
        if (isWaiting) return;

        //앞이 벽 혹은 낭떠러지인지 체크
        if (IsHittingWall() || IsNearEdge())
        {
            //멈추고 방향을 바꾸는 코루틴 실행
            StartCoroutine(WaitAndTurn());
        }
    }

    //행동
    protected override void Act()
    {
        //현재 쉬지 않고 순찰중이라면
        if (currentState == State.Patrol && !isWaiting)
        {
            //계속 걸어가기, 어차피 벽이나 낭떠러지 만나면 쉬는 상태로 변함
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            animator.SetBool("isRunning", true); 
        }
        else
        {
            //순찰 상태가 멈춘 경우
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
        }
    }

    //벽 판정 메소드
    private bool IsHittingWall()
    {
        //레이캐스트 레이저 시작점 및 길이 정하기
        Vector2 origin = new Vector2(
            col.bounds.center.x + (direction * (col.bounds.size.x / 2f)),
            col.bounds.center.y
        );
        float distance = 0.3f;

        //디버그용 선
        Debug.DrawRay(origin, Vector2.right * direction * distance, Color.red);

        //레이어 마스크에 맞게 무언가 닿았으면 true 아니면 fasle 리턴
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, distance, combinedLayerMask);
        return hit.collider != null;
    }

    //낭떠러지 판정 메소드
    private bool IsNearEdge()
    {
        //내 몸통의 앞쪽 맨 끝 좌표의 발 밑 구하기
        Vector2 origin = new Vector2(
            col.bounds.center.x + (direction * (col.bounds.size.x / 2f)),
            col.bounds.min.y + 0.1f 
        );
        
        //바닥을 향해 쏠 레이저 길이
        float distance = 0.3f;

        //디버그용 선
        Debug.DrawRay(origin, Vector2.down * distance, Color.blue);

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, combinedLayerMask);
        
        //벽이나 바닥이 아니라 아무것도 확인을 못했다면 낭떠러지라는 뜻
        return hit.collider == null; 
    }

    //멈췄다가 반대로 향하는 메소드
    private IEnumerator WaitAndTurn()
    {
        isWaiting = true;
        currentState = State.Idle; // 대기 상태로 변경 (Act()에서 속도 0으로 깎임)

        //설정한 시간만큼 대기
        yield return new WaitForSeconds(waitTime);

        //방향 뒤집기
        direction *= -1;

        //보는 방향도 뒤집어줌
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        //다시 움직이기 시작
        currentState = State.Patrol;
        isWaiting = false;
    }
}