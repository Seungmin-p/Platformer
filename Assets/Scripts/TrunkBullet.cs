using UnityEngine;

public class TrunkBullet : MonoBehaviour
{
    //파편 프리팹
    [SerializeField] GameObject[] debrisPrefabs;
    [SerializeField] Rigidbody2D rb;
    
    //투사체 속도 및 방향 기억용 변수
    private float speed = 14f; 
    private int currentDirection;

    //공격 메소드
    public void Fire(int direction)
    {
        //입력받은 방향 저장
        currentDirection = direction;
        
        //입력받은 값에 속도를 곱해서 발사함
        rb.linearVelocity = new Vector2(direction * speed, 0);
        
        //발사된 방향에 따라서 이미지 방향 처리
        Vector3 scale = transform.localScale;
        scale.x = (direction == 1) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //닿은게 몬스터, 함정, 아이템인 경우는 무시
        if (collision.CompareTag("Monster") || collision.CompareTag("Trap") || collision.CompareTag("Item")) return;
        
        //플레이어에 닿은 경우 플레이어 사망처리
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                Vector2 bounceDir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                player.CallDeathEvent(bounceDir * 1.5f); 
            }
        }

        //어딘가에 부딪혔다면, 파편화
        if (debrisPrefabs != null && debrisPrefabs.Length > 0)
        {
            Vector2 safeSpawnPos = (Vector2)transform.position - new Vector2(currentDirection * 0.2f, 0);
            
            foreach (GameObject debrisPrefab in debrisPrefabs)
            {
                if (debrisPrefab != null)
                {
                    //각 파편을 내 현재 위치에서 사방으로 살짝 랜덤하게 흩뿌려 생성합니다.
                    Vector2 spawnOffset = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                    Instantiate(debrisPrefab, safeSpawnPos + spawnOffset, Quaternion.identity);
                }
            }
        }

        //이후 객체 삭제
        Destroy(gameObject);
    }
}