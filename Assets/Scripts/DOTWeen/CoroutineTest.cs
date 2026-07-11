using UnityEngine;
using System.Collections;

public class CoroutineTest : MonoBehaviour
{
    private void Start()
    {
        //2초 동안 X 좌표 5f의 위치로 이동하는 코루틴 시작
        StartCoroutine(MoveXEaseOutCoroutine(5f, 2f));
    }

    private IEnumerator MoveXEaseOutCoroutine(float targetX, float duration)
    {
        // 원래 시작했던 원본 위치와 목표 위치를 미리 계산하여 저장합니다.
        Vector3 originPosition = transform.position;
        Vector3 destinationPosition = new Vector3(targetX, originPosition.y, originPosition.z);

        while (true)
        {
            // ==========================================
            // 1. 순방향 이동 (원래 위치 -> 목표 위치)
            // ==========================================
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float easeOutT = t * (2f - t); 

                float currentX = Mathf.Lerp(originPosition.x, destinationPosition.x, easeOutT);
                transform.position = new Vector3(currentX, originPosition.y, originPosition.z);

                yield return null;
            }
            transform.position = destinationPosition; // 순방향 목적지 강제 고정


            // ==========================================
            // 2. 역방향 이동 (목표 위치 -> 원래 위치) - LoopType.Yoyo 효과
            // ==========================================
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
            
                // 💡 보정 핵심: 정점에서 부드럽게 출발하도록 Ease In 공식을 적용합니다.
                float easeInT = t * t; // 초반 감속 ➔ 후반 가속

                // 정점(destination)에서 출발하여 원점(origin)으로 부드럽게 가속하며 복귀
                float currentX = Mathf.Lerp(destinationPosition.x, originPosition.x, easeInT);
                transform.position = new Vector3(currentX, originPosition.y, originPosition.z);

                yield return null;
            }
            transform.position = originPosition; // 역방향 목적지(원래 위치) 강제 고정
        }
    }
}