//각 상태별로 로직 중복을 막기 위한 컨트롤러
//호출을 담당하며, 실제 로직은 플레이어 내부에 존재

using UnityEngine;

namespace FSM
{
    public interface PlayerStateController
    {
        //상태 판단용 정보들
        //읽기 전용
        bool IsGrounded { get; } //땅 판정
        bool IsWall { get; } //벽 판정
        float XInput { get; } //좌 우 이동값
        float JumpForce { get; } //점프력
        float WallSlip { get; } //벽 미끄러짐 정도
        Vector2 HitDirection { get; } //무언가에 맞았을 때, 튕겨나갈 방향
    
        //상태 변경 가능
        bool CanJump { get; set; }
        bool CanDoubleJump { get; set; }
        int PlayerDirection { get; set; }
    
        //실행할 동작
        void ExecuteMove(float moveInput);
        void ExecuteJump(float jumpPower);
    }
}