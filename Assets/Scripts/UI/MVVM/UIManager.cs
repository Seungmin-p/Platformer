using UnityEngine;
using MVVM;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private View playerView;

    private ViewModel _playerViewModel;
    private Model _playerModel;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // 1. 장기 보존될 데이터(Model) 생성
        _playerModel = new Model(100, 0, 1);

        // 2. 데이터를 관리할 ViewModel 생성 (View가 없어도 미리 생성 가능!)
        _playerViewModel = new MVVM.ViewModel(_playerModel);
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
            _playerViewModel.UnBind();
        }
    }
}
