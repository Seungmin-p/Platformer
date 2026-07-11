using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.Profiling;
using System.Diagnostics;

public class UI_BenchmarkRunner : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private GameObject testPrefab;
    [SerializeField] private Transform canvasTransform; 
    [SerializeField] private int objectCount = 10000; // 💡 만 개(10,000)로 체급 상향
    [SerializeField] private float moveDuration = 2f; 
    [SerializeField] private float moveDistance = 200f; 

    private GameObject[] _pool;
    private Vector3[] _startPositions;

    void Start()
    {
        // 만 개 이상의 트윈을 처리할 공간을 프로그램 시작 시점에 미리 확보합니다.
        DOTween.SetTweensCapacity(10500, 100);
        
        _pool = new GameObject[objectCount];
        _startPositions = new Vector3[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            // 💡 10,000개가 화면에 골고루 배치되도록 격자 배치 조절 (100 x 100 구조)
            Vector3 spawnPos = new Vector3((i % 100) * 10f - 500f, (i / 100) * 10f - 500f, 0f);
        
            _pool[i] = Instantiate(testPrefab, canvasTransform); 
            _pool[i].transform.localPosition = spawnPos;
            _startPositions[i] = spawnPos;
        }

        UnityEngine.Debug.Log($"[{objectCount}개 UI 준비 완료] 'Space' 키를 누르면 벤치마크가 시작됩니다.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(CoreBenchmarkSequence());
        }
    }

    private IEnumerator CoreBenchmarkSequence()
    {
        UnityEngine.Debug.Log("▶ [START] 성능 테스트 시작");

        // --------------------------------------------------
        // TEST A : 순수 유니티 코루틴 만 개 가동
        // --------------------------------------------------
        UnityEngine.Debug.Log("1. 순수 코루틴 만 개 시동...");
        
        Stopwatch sw = Stopwatch.StartNew();
        Profiler.BeginSample("★_MASS_COROUTINE_RUN"); 

        for (int i = 0; i < objectCount; i++)
        {
            StartCoroutine(MoveXInfiniteYoyoCoroutine(_pool[i].transform, _startPositions[i], _startPositions[i].x + moveDistance, moveDuration));
        }

        Profiler.EndSample();
        sw.Stop();
        UnityEngine.Debug.Log($"▶ [결과] 코루틴 만 개 시동 연산 소요 시간 : {sw.ElapsedMilliseconds} ms");

        // 💡 딜레이 단축: 왕복 연출 시간(4초)이 끝나자마자 칼같이 다음 단계로 이동
        yield return new WaitForSeconds(moveDuration * 2f); 

        // --------------------------------------------------
        // 환경 리셋 단계
        // --------------------------------------------------
        ResetAllPositions();
        yield return new WaitForSeconds(1f); // 리셋 처리가 물리적으로 반영될 최소한의 찰나만 대기


        // --------------------------------------------------
        // TEST B : DOTween 라이브러리 만 개 가동
        // --------------------------------------------------
        UnityEngine.Debug.Log("2. DOTween 만 개 시동...");

        sw.Restart();
        Profiler.BeginSample("★_MASS_DOTWEEN_RUN"); 

        for (int i = 0; i < objectCount; i++)
        {
            _pool[i].transform.DOLocalMoveX(_startPositions[i].x + moveDistance, moveDuration)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo);
        }

        Profiler.EndSample();
        sw.Stop();
        UnityEngine.Debug.Log($"▶ [결과] DOTween 만 개 시동 연산 소요 시간 : {sw.ElapsedMilliseconds} ms");

        // 💡 딜레이 단축: 왕복 연출 시간(4초) 대기 후 종료
        yield return new WaitForSeconds(moveDuration * 2f); 

        UnityEngine.Debug.Log("■ [END] 성능 테스트 종료");
    }

    private void ResetAllPositions()
    {
        for (int i = 0; i < objectCount; i++)
        {
            _pool[i].transform.DOKill();
            _pool[i].transform.localPosition = _startPositions[i];
        }
    }

    private IEnumerator MoveXInfiniteYoyoCoroutine(Transform target, Vector3 originPos, float targetX, float duration)
    {
        Vector3 destinationPosition = new Vector3(targetX, originPos.y, originPos.z);

        // 1. 순방향 이동 (Ease Out)
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeOutT = t * (2f - t); 

            float currentX = Mathf.Lerp(originPos.x, destinationPosition.x, easeOutT);
            target.localPosition = new Vector3(currentX, originPos.y, originPos.z);
            yield return null;
        }
        target.localPosition = destinationPosition;

        // 2. 역방향 이동 (Ease In)
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeInT = t * t; 

            float currentX = Mathf.Lerp(destinationPosition.x, originPos.x, easeInT);
            target.localPosition = new Vector3(currentX, originPos.y, originPos.z);
            yield return null;
        }
        target.localPosition = originPos;
    }
}