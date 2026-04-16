using System.Collections;
using UnityEngine;

public class SeedDebris : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Rigidbody2D rb;
    
    private float lifetime = 3.0f;
   
    void Start()
    {
        //파편마다 살짝 다르게 튀도록 랜덤 범위 설정
        float randomX = Random.Range(-3f, 3f); 
        float randomY = Random.Range(3f, 6f);  
        rb.linearVelocity = new Vector2(randomX, randomY);

        //회전효과 부여
        rb.AddTorque(Random.Range(-10f, 10f), ForceMode2D.Impulse);

        //깜빡이다 사라지는 코루틴 실행
        StartCoroutine(BlinkAndDie());
    }

    private IEnumerator BlinkAndDie()
    {
        //깜빡이기 위한 변수, 주기 설정
        float elapsed = 0f;
        float blinkInterval = 0.1f; 

        while (elapsed < lifetime)
        {
            sr.enabled = !sr.enabled; // 스프라이트 껐다 켰다 하기
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        //시간이 지나면 완전 삭제
        Destroy(gameObject);
    }
}