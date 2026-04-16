using UnityEngine;

public class SwingMace : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("체크하면 한 방향으로 무한 회전, 해제하면 그네처럼 왕복 운동합니다.")]
    [SerializeField] bool isFullRotation = false; 
    
    [Tooltip("회전 속도 (숫자가 클수록 빠름)")]
    [SerializeField] float speed = 2f;
    
    [Tooltip("그네 모드일 때 좌우로 꺾일 최대 각도")]
    [SerializeField] float swingAngle = 80f; 

    private float startAngle;

    private void Start()
    {
        //시작할 때 에디터에 배치된 Z축 각도를 기준점으로 삼습니다.
        startAngle = transform.localEulerAngles.z;
    }

    private void Update()
    {
        if (isFullRotation)
        {
            //한 방향으로 계속 돌도록 처리
            transform.Rotate(0, 0, speed * 100f * Time.deltaTime);
        }
        else
        {
            //Sin값을 이용하여 계속 왔다갔다 할 수 있도록 처리
            float angleOffset = Mathf.Sin(Time.time * speed) * swingAngle;
            transform.localRotation = Quaternion.Euler(0, 0, startAngle + angleOffset);
        }
    }
}