using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PureValueBenchmark : MonoBehaviour
{
    private int testCount = 10000; // 만 개 체급
    private float dummyValue;

    void Start()
    {
        // 시동 프레임 드랍 방지를 위해 풀 크기 사전 확보
        DOTween.SetTweensCapacity(10500, 100);
        UnityEngine.Debug.Log("준비 완료: 키보드 '1'은 코루틴 만 개, '2'는 DOTween 만 개를 구동합니다.");
    }

    void Update()
    {
        // 1번 누르면 코루틴 만 개 실행
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UnityEngine.Debug.Log("▶ 코루틴 만 개 매 프레임 루프 시작");
            for (int i = 0; i < testCount; i++)
            {
                StartCoroutine(FloatCoroutine());
            }
        }

        // 2번 누르면 DOTween 만 개 실행
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UnityEngine.Debug.Log("▶ DOTween 만 개 매 프레임 루프 시작");
            for (int i = 0; i < testCount; i++)
            {
                // 변수나 컴포넌트 없이 오직 내부 메모리 값만 2초간 변경
                DOTween.To(() => dummyValue, x => dummyValue = x, 100f, 2f);
            }
        }
    }

    private IEnumerator FloatCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            dummyValue = Mathf.Lerp(0f, 100f, elapsed / 2f);
            yield return null; // 매 프레임 유니티 네이티브 브릿지를 통과함
        }
    }
}