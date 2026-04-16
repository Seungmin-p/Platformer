using System.Collections;
using UnityEngine;

//공격을 제외한 로직들이 버섯과 동일하기에 상속처리
public class Trunk : Mushroom 
{
    [SerializeField] LayerMask playerLayer; //플레이어의 레이어
    [SerializeField] GameObject projectilePrefab; //투사체 프리팹
    [SerializeField] Transform firePoint; //투사체 발사 위치, 좌표계산으로도 가능하지만 빈 오브젝트로 위치 잡아보기
    private float sightRange = 30f;  //몬스터 시야 거리
    [SerializeField] float attackCooldown = 0.8f; //공격 딜레이

    private bool isAttacking = false;

    protected override void Think()
    {
        //공격중엔 추가적인 생각이 필요없음
        if (isAttacking) return;

        //시야 내에 플레이어가 있는지 체크
        if (CanSeePlayer())
        {
            //플레이어 확인 시 공격 시작
            StartCoroutine(AttackRoutine());
            return; 
        }

        //플레이어 확인이 안됐다면 버섯과 동일하게 순찰 진행
        base.Think();
    }

    // 🏃 실제 행동: 버섯의 다리를 덮어씁니다.
    protected override void Act()
    {
        if (currentState == State.Attack)
        {
            // 공격 중일 때는 제자리에 딱 멈춰서기
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
        }
        else
        {
            // 공격 중이 아니면 부모(버섯)처럼 열심히 걸어 다니기!
            base.Act();
        }
    }

    //플레이어 감지용 메소드
    private bool CanSeePlayer()
    {
        //몸통 중앙에서 앞쪽으로 시야만큼 레이저 발사
        Vector2 origin = col.bounds.center;
        
        //시야 범위 확인용 선
        Debug.DrawRay(origin, Vector2.right * direction * sightRange, Color.yellow);

        //플레이어 레이어가 확인됐다면 true 반환
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, sightRange, playerLayer);
        return hit.collider != null;
    }

    //공격용 메소드
    private IEnumerator AttackRoutine()
    {
        //상태 업데이트
        isAttacking = true;
        currentState = State.Attack;

        //공격 애니메이션
        animator.SetTrigger("doAttack");

        //애니메이션 상 공격 딜레이만큼 잠깐 대기
        yield return new WaitForSeconds(0.3f);

        //투사체 생성
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject seed = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            
            TrunkBullet BulletScript = seed.GetComponent<TrunkBullet>();
            
            // 3. 스크립트를 성공적으로 찾았다면, 나무의 현재 방향(direction)을 전달해 발사!
            if (BulletScript != null)
            {
                BulletScript.Fire(direction); 
            }
        }

        //공격 쿨타임 대기
        yield return new WaitForSeconds(attackCooldown);

        //공격이 끝나면 다시 순찰 시작
        currentState = State.Patrol;
        isAttacking = false;
    }
}