using UnityEngine;

//땅에 떨어진 후 버섯로직을 그대로 쓰기 위해 상속처리
public class Radish : Mushroom 
{

    [SerializeField] GameObject[] propellerDebrisPrefabs; //잎사귀 프리팹
    [SerializeField] float floatAmplitudeX = 3.0f; //좌우 이동 폭
    [SerializeField] float floatSpeedX = 1.0f; //좌우 이동 속도
    [SerializeField] float floatAmplitudeY = 1.5f; //상하 이동 폭
    [SerializeField] float floatSpeedY = 2.0f; //상하 이동 속도
    [SerializeField] ParticleSystem flyDust;
    
    
    private bool isFlying = true; //비행 상태
    private Vector3 startPos; //시작 위치
    
    //비행 동작에 필요한 랜덤처리 변수
    private float randomSeedX;
    private float randomSeedY;

    protected override void Start()
    {
        base.Start(); 
        
        //시작 위치 기억
        startPos = transform.position;
        
        //랜덤처리
        randomSeedX = Random.Range(0f, 100f);
        randomSeedY = Random.Range(0f, 100f);
        
        if (isFlying) 
        {
            //비행중 상태 설정
            rb.bodyType = RigidbodyType2D.Kinematic;
            animator.SetBool("isFlying", true); 
        }
    }

    protected override void Think()
    {
        //땅에 떨어진 상태에서만 부모 클래스의 Think 로직 실행
        if (!isFlying) 
        {
            base.Think();
        }
    }

    protected override void Act()
    {
        if (currentState == State.Dead) return;

        //날고있는 상태일 때
        if (isFlying)
        {
            //부드러운 랜덤(PerlinNoise) 사용
            //0.0 ~ 1.0 사이의 값을 반환하므로, -0.5를 한 뒤 2를 곱해 -1.0 ~ 1.0으로 만들어줌
            float noiseX = (Mathf.PerlinNoise(Time.time * floatSpeedX, randomSeedX) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(randomSeedY, Time.time * floatSpeedY) - 0.5f) * 2f;

            //랜덤값을 각 이동 폭에 곱해 최종 좌표 확보
            float newX = startPos.x + noiseX * floatAmplitudeX;
            float newY = startPos.y + noiseY * floatAmplitudeY;
            
            //이동 방향에 맞춰 보는 방향도 돌림
            float deltaX = newX - transform.position.x;
            if (Mathf.Abs(deltaX) > 0.01f) 
            {
                Vector3 scale = transform.localScale;
                scale.x = (deltaX > 0) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            //위치 확정
            transform.position = new Vector3(newX, newY, transform.position.z);
        }
        else
        {
            //땅에 떨어졌을 때는 부모클래스의 Act 로직 실행
            base.Act();
        }
    }

    //부모 클래스의 Die 메소드를 오버라이딩해서 새 동작으로 바꿈
    protected override void Die(Vector2 bounceDir)
    {
        if (isFlying)
        {
            //날고있는 상태에서 밟히면 추락처리
            HandleFallDown();
        }
        else
        {
            //날지 않는 상태에서 밟히면 실제 사망
            base.Die(bounceDir);
        }
    }

    //추락 처리 메소드
    private void HandleFallDown()
    {
        //비행 상태 변경
        isFlying = false; 

        //맞는 모션 재생
        animator.SetTrigger("doHit");

        //애니메이션 전환
        animator.SetBool("isFlying", false);
        
        //비행 파티클 중단
        flyDust.Stop();
        
        //중력 적용
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 8f; 
        
        //보는 방향과 실제 방향 동기화
        direction = transform.localScale.x < 0 ? 1 : -1;

        //추락 시 같이 떨어질 파편 생성
        if (propellerDebrisPrefabs != null && propellerDebrisPrefabs.Length > 0)
        {
            //몬스터 콜라이더의 가장 윗부분(max.y)을 기준으로 파편 생성
            Vector3 headPosition = new Vector3(transform.position.x, col.bounds.max.y, transform.position.z);

            foreach (GameObject debrisPrefab in propellerDebrisPrefabs)
            {
                if (debrisPrefab != null)
                {
                    //파편이 겹치지 않도록 랜덤값 부여
                    Vector3 spawnOffset = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0f, 0.1f), 0);
                    Instantiate(debrisPrefab, headPosition + spawnOffset, Quaternion.identity);
                }
            }
        }
    }
}