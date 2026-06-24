using System.Collections;
using UnityEngine;

namespace MapGeneration
{
    public class Bomb : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Rigidbody2D rb;
        [SerializeField] GameObject explosionEffectPrefab;//폭탄 이펙트
        [SerializeField] LayerMask damageLayer;
        
        
        [SerializeField] float countdownTime = 2.0f; //폭탄이 터지기까지 걸리는 시간
        [SerializeField] int explosionRange = 3;     //폭발 범위
        [SerializeField] float pushSpeed = 9f;           //밀리는 속도 (플레이어 속도와 동일)
        [SerializeField] float horizontalFriction = 2f; //밀리지 않을 때의 이동 저항력
        
        private bool isBeingPushed = false; //밀리는 중인지 판단하는 플래그
        private Collider2D[] myColliders; //콜라이더들을 담아둘 변수

        private void Awake()
        {
            //콜라이더들 두종류 등록
            myColliders = GetComponents<Collider2D>();
        }
        
        private void Start()
        {
            //플레이어 인스턴스가 확실하게 존재한다면
            if (Player.Instance != null && Player.Instance.Collider != null)
            {
                //폭탄의 콜라이더 전체를 확인
                foreach (Collider2D col in myColliders)
                {
                    //트리거가 꺼져있는 콜라이더를 대상으로
                    if (!col.isTrigger)
                    {
                        //플레이어와의 물리 충돌을 무시하여, 제자리 스폰 문제없게 변경
                        Physics2D.IgnoreCollision(col, Player.Instance.Collider, true);
                    }
                }
            }
            
            //폭탄 카운트다운 진행
            StartCoroutine(CountdownRoutine());
        }
        
        private void FixedUpdate()
        {
            //플레이어에게 실시간으로 밀리는 중이 아니라면 저항력 적용
            if (rb != null && !isBeingPushed)
            {
                //현재 속도에 저항력 적용
                Vector2 velocity = rb.linearVelocity;
                //velocity.x ~ 0까지 시간과 horizontalFriction 값을 곱해서 보간처리
                velocity.x = Mathf.Lerp(velocity.x, 0f, Time.fixedDeltaTime * horizontalFriction);
                rb.linearVelocity = velocity;
            }
    
            //밀리는 상태 매 물리 프레임마다 초기화
            //물리 프레임 순서상 FixedUpdate -> OnTriggerStay2D 순서로 동작
            //따라서 밀리고 있다면 OnTriggerStay2D에서 다시 true로 변경됨
            isBeingPushed = false;
        }
        
        //트리거가 켜져있는 큰 콜라이더용 플레이어가 폭탄을 미는지 체크
        private void OnTriggerStay2D(Collider2D collision)
        {
            //부딪힌게 플레이어라면
            if (collision.CompareTag("Player"))
            {
                //플레이어의 좌우 이동 입력값 가져오기 (-1, 0, 1)
                float playerInput = Player.Instance.XInput; 

                //방향키를 누르고 있을 때만 연산
                if (Mathf.Abs(playerInput) > 0.1f)
                {
                    //폭탄과 플레이어의 상대 위치 (폭탄이 오른쪽에 있으면 양수, 왼쪽에 있으면 음수)
                    float dirToBomb = transform.position.x - collision.transform.position.x;

                    //플레이어가 폭탄과 충돌한 상태에서 폭탄이 있는 방향으로 폭탄을 미는지 확인
                    //dirToBomb이 양수라면 폭탄이 플레이어보다 오른쪽인데, 이때 playerInput 또한 양수라면 오른쪽으로 밀고있다는걸 의미
                    if (Mathf.Sign(dirToBomb) == Mathf.Sign(playerInput))
                    {
                        //마찰력 로직을 패스하기 위해 미는 상태를 켜줌
                        isBeingPushed = true; 
                        //폭탄의 속도를 밀기 속도로 고정
                        rb.linearVelocity = new Vector2(playerInput * pushSpeed, rb.linearVelocity.y);
                    }
                }
            }
        }
        
        //제자리 스폰을 위한 로직
        //플레이어가 폭탄 밖으로 나가는 순간 작동
        private void OnTriggerExit2D(Collider2D collision)
        {
            //트리거에서 빠져나간 대상이 플레이어라면
            if (collision.CompareTag("Player"))
            {
                //콜라이더 목록을 확인해서
                foreach (Collider2D col in myColliders)
                {
                    //그중 트리거가 꺼져있는 물리판정용 콜라이더에게
                    if (!col.isTrigger) 
                    {
                        //플레이어와의 충돌을 더 이상 무시하지 않음
                        //이로써 폭탄 제자리 스폰이후 빠져나간 다음 다시 플레이어와 충돌을 가능하게 함
                        Physics2D.IgnoreCollision(col, collision, false);
                    }
                }
            }
        }

        //폭탄 스폰과 함께 실행되는 코루틴
        private IEnumerator CountdownRoutine()
        {
            float startTime = Time.time; //폭탄이 켜진 시간 기록
            Color originalColor = Color.white;
            Color warningColor = Color.red;

            //현재 시간에서, 시작 시간을 뺀 값이 터질때 까지의 시간(2초)보다 작다면 계속 반복
            while (Time.time - startTime < countdownTime)
            {
                //진행도(진행된 시간 / 터질때까지 걸리는 총 시간)
                float progress = (Time.time - startTime) / countdownTime;
                
                //진행도에 따라서 깜빡이는 속도 빠르게 증가
                float blinkSpeed = Mathf.Lerp(0.3f, 0.05f, progress); 

                //실행될 때 마다 현재 색상 비교 후, 다른 색상으로 변경
                spriteRenderer.color = (spriteRenderer.color == originalColor) ? warningColor : originalColor;

                //깜빡이는 속도만큼 대기
                yield return new WaitForSeconds(blinkSpeed);
            }

            Explode();
        }

        //폭발 메소드, 객체 폭발과 타일 폭발로 나뉨
        private void Explode()
        {
            //씬에 청크매니저 인스턴스가 있다면
            if (ChunkManager.Instance != null)
            {
                //폭탄 범위를 넉넉하게 잡아줌
                float calculatedRadius = explosionRange + 1.5f; 
                
                //폭탄이 터진 위치와, 그 반경을 기준으로 범위에 포함된 모든 객체를 가져옴
                Collider2D[] objectsToDamage = Physics2D.OverlapCircleAll(transform.position, calculatedRadius, damageLayer);
                
                //폭탄이 터진 위치를 격자좌표로 전환
                Vector3Int bombCell = ChunkManager.Instance.WorldToCell(transform.position);

                //폭발 범위 내 객체들을 확인
                foreach (Collider2D obj in objectsToDamage)
                {
                    //격자 좌표 전환
                    Vector3Int targetCell = ChunkManager.Instance.WorldToCell(obj.transform.position);
                    
                    //맨해튼 거리 체크
                    int gridDistance = Mathf.Abs(targetCell.x - bombCell.x) + Mathf.Abs(targetCell.y - bombCell.y);

                    //범위 밖이라면 패스
                    if (gridDistance > explosionRange) continue;

                    //범위 안에들어온 객체에 따른 데미지, 삭제처리
                    if (obj.CompareTag("Player"))
                    {
                        Player.Instance.CallDeathEvent(Vector2.up); 
                    }
                    else if(obj.CompareTag("Trap"))
                    {
                        //청크 매니저가 확인된다면
                        if (ChunkManager.Instance != null)
                        {
                            //파괴된 오브젝트의 위치를 기반으로 D 마킹
                            ChunkManager.Instance.MarkObjectAsDestroyed(obj.transform.position);
                        }
                        Destroy(obj.gameObject);
                    }
                    else if (obj.CompareTag("Monster"))
                    {
                        Monster monster = obj.GetComponent<Monster>();
                        if (monster != null)
                        {
                            //몬스터가 내부적으로 D 마크를 찍을 수 있도록 히트처리
                            monster.HitByExplosion(); 
                        }
                        else
                        {
                            Destroy(obj.gameObject);
                        }
                    }
                }

                //청크 매니저의 기능을 통해 폭발 범위 타일 처리
                ChunkManager.Instance.ExplodeTilesInWorld(transform.position, explosionRange, explosionEffectPrefab);
            }
            //청크 매니저 방식과 거의 유사
            else if (TilemapCreator.Instance != null)
            {
                UnityEngine.Tilemaps.Tilemap tilemap = TilemapCreator.Instance.MainTilemap;
                if (tilemap == null) return;

                float calculatedRadius = explosionRange + 1.5f; 
                Collider2D[] objectsToDamage = Physics2D.OverlapCircleAll(transform.position, calculatedRadius, damageLayer);
                Vector3Int bombCell = tilemap.WorldToCell(transform.position);

                foreach (Collider2D obj in objectsToDamage)
                {
                    Vector3Int targetCell = tilemap.WorldToCell(obj.transform.position);
                    int gridDistance = Mathf.Abs(targetCell.x - bombCell.x) + Mathf.Abs(targetCell.y - bombCell.y);

                    if (gridDistance > explosionRange) continue;

                    if (obj.CompareTag("Player"))
                    {
                        Player.Instance.CallDeathEvent(Vector2.up); 
                    }
                    else
                    {
                        Destroy(obj.gameObject);
                    }
                }

                //TilemapCreator의 기능을 통해 폭발 범위 타일 처리
                TilemapCreator.Instance.ExplodeTiles(transform.position, explosionRange, explosionEffectPrefab);
            }

            //폭탄 오브젝트 삭제처리
            Destroy(gameObject);
        }
    }
}