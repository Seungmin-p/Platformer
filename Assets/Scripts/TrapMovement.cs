using UnityEngine;
using System;
using System.Collections.Generic;

public class TrapMovement : MonoBehaviour
{
    //함정이 이동할 포인트들
    [SerializeField] List<Transform> waypoints;
    [SerializeField] float moveSpeed = 3f;
    private int currentTargetIndex = 0;
    private int direction = 1;
    private float pauseTimer = 0f;
    
    public event Action OnWaypointReached;

    //외부에서 현재 이동 방향을 읽을 수 있는 프로퍼티
    public Vector3 MoveDirection { get; private set; }
    
    //외부에서 호출 가능한 움직임을 멈추게하는 메소드
    public void Pause(float time)
    {
        pauseTimer = time;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Count < 2) return;
        
        //pauseTimer이 남아있는 경우 움직임 대신 시간만 감소
        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return; 
        }
        
        MoveToTarget();
    }
    private void MoveToTarget()
    {
        //현재 및 목표 위치
        Vector3 currentPos = transform.position;
        Vector3 targetPos = waypoints[currentTargetIndex].position;

        //현재 이동 방향 저장하기
        if (Vector3.Distance(currentPos, targetPos) > 0.001f)
        {
            MoveDirection = (targetPos - currentPos).normalized;
        }

        //목표 포인트까지 moveSpeed * Time.deltaTime만큼 이동
        transform.position = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

        //목표 도달 시
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            //목표 도달 이벤트 호출
            OnWaypointReached?.Invoke();
            
            //pauseTimer이 남아있다면 움직임 멈춤
            if (pauseTimer > 0) MoveDirection = Vector3.zero;
            
            //타겟 인덱스 변경
            currentTargetIndex = (currentTargetIndex + 1) % waypoints.Count;
        }
    }
}