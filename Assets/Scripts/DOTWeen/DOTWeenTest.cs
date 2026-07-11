using UnityEngine;
using DG.Tweening;

public class DOTWeenTest : MonoBehaviour
{
    private void Start()
    {
        // 2. 오브젝트를 2초 동안 X 좌표 5의 위치로 부드럽게 이동
        transform.DOMoveX(5f, 2f).SetLoops(-1, LoopType.Yoyo);
    }
}
