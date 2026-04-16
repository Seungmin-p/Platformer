using System.Collections;
using UnityEngine;

public class FallingPlatforms : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    
    private float idleAmplitude = 0.2f; // 둥둥 뜨는 높이
    private float idleSpeed = 3f;      // 둥둥 뜨는 속도
    private float fallDelay = 1.0f;    // 밟은 후 떨어질 때 까지의 시간
    private float shakeIntensity = 0.1f; // 밟았을 때 흔들림 강도
    
    private float timeOffset;      //시작 위치에 랜덤성을 부여하기 위한 변수
    private float individualSpeed; //떠있는 속도에 랜덤성을 살짝 부여하기 위한 변수

    private Vector3 originPos;
    private bool isStepped = false;

    void Start() {
        //설정상의 위치값을 저장함
        originPos = transform.position;

        //시작 지점 랜덤화
        timeOffset = Random.Range(0f, 2f * Mathf.PI);

        //속도 랜덤화
        individualSpeed = idleSpeed * Random.Range(0.8f, 1.2f);
    }

    void Update() {
        //아직 멈추지 않았다면
        if (!isStepped) {
            //현재 위치값을 Sin을 이용해 계산
            //지금까지 지난 시간을 랜덤속도값에 곱해주고, 시작지점의 위치를 더해줌
            float currentPos = (Time.time * individualSpeed) + timeOffset;
            
            //계산한 값에 sin파 적용 후, 떠다니는 높이를 곱해준 이후 초기 y좌표에 더해서 새 좌표를 만들어줌
            float newY = originPos.y + Mathf.Sin(currentPos) * idleAmplitude;
            
            //y좌표를 sin을 적용해서 새로 만든 y좌표로 변경
            transform.position = new Vector3(originPos.x, newY, originPos.z);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        //플레이어와 닿았으며, 아직 멈추지 않은 경우
        if (collision.gameObject.CompareTag("Player") && !isStepped) {
            
            //밟힌 방향을 가져옴
            Vector2 normal = collision.GetContact(0).normal;
            
            //만약 밟힌 방향이 -0.5f보다 작다면, 플레이어가 위에서 밟았다는 의미
            //플레이어 기준으로 공중 플랫폼을 밟은 방향의 위치가 메인이기 때문에 y는 +가 아니라 -인게 맞음
            if (normal.y < -0.5f)
            {
                //애니메이션 실행, 중복 밟힘 방지, 떨어지기 시작
                animator.SetTrigger("doStop");
                isStepped = true;
                StartCoroutine(FallRoutine());
            }
        } else if( collision.gameObject.CompareTag("Ground") )
        {
            //떨어져서 땅에 닿은 경우 객체 삭제
            Destroy(gameObject);
        }
    }

    //플레이어에게 밟혀 시작되는 추락 메소드
    private IEnumerator FallRoutine() {
        //떨어지는 시간체크용 변수 및 현재 위치 저장하는 변수
        float elapsed = 0f;
        Vector3 currentPos = transform.position;

        //밟은 순간부터 fallDelay시간 까지 강하게 위아래로 흔들림
        while (elapsed < fallDelay) {
            float shakeOffset = Random.Range(-shakeIntensity, shakeIntensity);
            //y값에 한해서 흔들림 효과 랜덤으로 계속 부여
            transform.position = new Vector3(currentPos.x, currentPos.y + shakeOffset, currentPos.z);
            
            //반복문 종료를 위한 시간 누적
            elapsed += Time.deltaTime;
            
            //다음 프레임까지 대기
            yield return null;
        }

        //반복문이 종료되면 추락 시작
        //rigidbody 속성을 dynamin으로 변경해서 중력 영향을 받게함
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f; //빠르게 떨어질 수 있게 중력값 변경
    }
}
