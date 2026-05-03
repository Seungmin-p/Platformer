//각 상태별로 로직 중복을 막기 위한 컨트롤러
//호출을 담당하며, 실제 로직은 몬스터 내부에 존재

using UnityEngine;

namespace FSM
{
    public interface MonsterStateController
    {
        //상태 판단용 정보들
        //읽기 전용
        bool IsWall { get; } //벽 판정
        bool IsEdge { get; } //낭떠러지 판정
        
        //상태 변경 가능
        int MonsterDirection { get; set; }
        
        //실행할 동작
        void ExecuteMove(float moveInput);
        void ExecuteStop();
        void ExecuteTurn();
        void ExecuteDie(Vector2 direction);
    }
}