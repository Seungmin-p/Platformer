using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public static MainCamera Instance { get; private set; }
    
    [SerializeField] float smoothSpeed = 1f; //카메라가 따라가는 부드러운 정도 (낮을수록 부드러움)
    [SerializeField] Vector3 offset = new Vector3(0, 0, -10);//플레이어와의 거리 유지

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void LateUpdate()
    {
        if (Player.Instance == null) return;

        //플레이어의 현재 위치에 오프셋을 더한 목표 좌표 계산
        Vector3 targetPosition = Player.Instance.transform.position + offset;

        //Lerp를 사용하여 현재 카메라 위치에서 목표 위치로 부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        //카메라의 위치 갱신
        transform.position = smoothedPosition;
    }
}
