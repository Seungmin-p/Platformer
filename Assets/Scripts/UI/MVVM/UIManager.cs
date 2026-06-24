using UnityEngine;
using MVVM;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private UIToolkitView playerView;
    [SerializeField] private UIToolkitModel _playerModel;

    private UIToolkitViewModel _playerViewModel;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // 2. 데이터를 관리할 ViewModel 생성 (View가 없어도 미리 생성 가능!)
        _playerViewModel = new UIToolkitViewModel(_playerModel);
        
        //기존의 이벤트 바인딩 기반은 옵저버 패턴과 동일
    }

    private void Start()
    {
        // 3. 씬이 시작될 때 View에게 ViewModel을 던져주며 "연결해라" 지시 (단 한 번만!)
        if (playerView != null)
        {
            playerView.Bind(_playerViewModel);
        }
    }

    private void OnDestroy()
    {
        // 4. 게임 종료/씬 전환 등 UIManager가 파괴될 때
        // 비로소 ViewModel과 Model의 깊은 연결을 최종적으로 끊어줌 (메모리 누수 방지)
        if (_playerViewModel != null)
        {
            //TODO: 객체 파괴
        }
    }
}
