using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;


public abstract class Monster : MonoBehaviour
{
    //몬스터들의 코드에서 사용할 속성들
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Collider2D col;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected ParticleSystem runDust;
    
    [SerializeField] protected float moveSpeed = 4f;
    [SerializeField] protected float bounceForce = 1f;
    
    //몬스터가 가질 수 있는 공통 상태
    public enum State { Idle, Patrol, Chase, Attack, Dead }
    [SerializeField] protected State currentState = State.Idle;
    
    public static event Action OnMonsterDefeated; //몬스터 킬 카운팅용 이벤트

    protected virtual void Start() { }

    protected virtual void Update()
    {
        //죽은 상태면 return
        if (currentState == State.Dead) return;

        Think(); //상황 판단
        Act();   //행동
    }

    //몬스터들별로 별도의 로직을 갖게 해줌
    protected virtual void Think() { }
    protected virtual void Act() { }

    //플레이어와 부딪혔을 때의 로직
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //플레이어와 부딪힌 방향을 가져옴
            Vector2 normal = collision.GetContact(0).normal;
            
            //만약 밟힌 방향이 -0.5f보다 작다면, 플레이어가 위에서 밟았다는 의미
            //플레이어 기준으로 공중 플랫폼을 밟은 방향의 위치가 메인이기 때문에 y는 +가 아니라 -인게 맞음
            if (normal.y < -0.5f)
            {
                //몬스터가 날아갈 방향 계산 (플레이어 위치 -> 몬스터 위치)
                Vector2 bounceDir = ((Vector2)(transform.position - collision.transform.position)).normalized;

                //플레이어를 위로 살짝 튕겨주기
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                Player player = collision.gameObject.GetComponent<Player>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0); //기존 낙하 속도 초기화
                    playerRb.AddForce(Vector2.up * 20f, ForceMode2D.Impulse); //플레이어 위로 점프
                    player.JumpDust(false); //점프 파티클 호출
                    player.CanDoubleJump(); //밟은 후 추가 점프 가능하도록
                }
                
                //몬스터 사망 관련 이벤트 호출
                OnMonsterDefeated.Invoke();

                //몬스터 사망 함수 호출
                Die(bounceDir);
                return;
            }
            
            //위에서 밟히지 않은 경우
            TriggerDeath(collision.gameObject, collision.GetContact(0).point);
        }
    }
    
    //몬스터 사망은 플레이어랑 같은 방식
    protected virtual void Die(Vector2 bounceDir)
    {
        if (currentState == State.Dead) return; // 중복 실행 방지

        //각종 상태 변경
        currentState = State.Dead;
        col.isTrigger = true; 
        animator.SetTrigger("doHit");
        
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