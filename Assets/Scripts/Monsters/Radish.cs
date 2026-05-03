using UnityEngine;
using FSM;

public class Radish : Monster 
{

    [SerializeField] GameObject[] propellerDebrisPrefabs; //잎사귀 프리팹
    [SerializeField] float floatAmplitudeX = 3.0f; //좌우 이동 폭
    [SerializeField] float floatSpeedX = 1.0f; //좌우 이동 속도
    [SerializeField] float floatAmplitudeY = 1.5f; //상하 이동 폭
    [SerializeField] float floatSpeedY = 2.0f; //상하 이동 속도
    [SerializeField] ParticleSystem flyDust;
    
    public MonsterRadishFlyState RadishFlyState { get; private set; }
    public MonsterRadishHitState RadishHitState { get; private set; }
    
    private Vector3 startPos; //시작 위치
    
    //비행 동작에 필요한 랜덤처리 변수
    private float randomSeedX;
    private float randomSeedY;

    protected override void Awake()
    {
        base.Awake(); 
        
        RadishFlyState = new MonsterRadishFlyState(this, stateMachine);
        RadishHitState = new MonsterRadishHitState(this, stateMachine);
    }
    
    protected override void Start()
    {
        base.Start(); 
        
        //시작 위치 기억
        startPos = transform.position;
        
        //랜덤처리
        randomSeedX = Random.Range(0f, 100f);
        randomSeedY = Random.Range(0f, 100f);
        
        //무 몬스터의 기본 상태 지정
        stateMachine.ChangeState(RadishFlyState);
    }

    protected override void TakeHit(Vector2 bounceDir)
    {
        if (stateMachine.CurrentState == RadishFlyState)
        {
            //날고 있었다면 Hit 상태
            stateMachine.ChangeState(RadishHitState);
        }
        else
        {
            //날고있지 않았다면 사망처리
            base.TakeHit(bounceDir);
        }
    }

    public void RadishFlying()
    {
        //rb 타입 설정
        rb.bodyType = RigidbodyType2D.Kinematic;
        
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

    //추락 처리 메소드
    public void RadishFallDown()
    {
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
    
    //땅 판정, Hit 후 Idle 전환용
    public bool IsGrounded()
    {
        //정 중앙에서 아래로 짧게 캐스트
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);
        float distance = 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, combinedLayerMask);
        return hit.collider != null;
    }
}