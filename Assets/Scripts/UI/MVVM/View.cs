using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVM
{
    public class View : MonoBehaviour
    {
        private ViewModel _viewModel;
        
        private VisualElement _mainContainer;
        private Button _openButton;
        private Button _closeButton;
        private Label _hpText;
        private Label _levelText;
        private ProgressBar _expBar;
        private Button _damageButton;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            
            _openButton = root.Q<Button>("OpenButton");
            _closeButton = root.Q<Button>("CloseButton");
            _mainContainer = root.Q<VisualElement>("MainContainer");
            _hpText = root.Q<Label>("HpText");
            _levelText = root.Q<Label>("LevelText");
            _expBar = root.Q<ProgressBar>("ExpBar");
            _damageButton = root.Q<Button>("DamageButton");
            
            //시작할 때 메인 UI는 숨겨주기
            _mainContainer.style.display = DisplayStyle.None;

            //UI 버튼 이벤트
            _openButton.clicked += OpenWindow;
            _closeButton.clicked += CloseWindow;
            _damageButton.clicked += OnClickDamage;
        }

        //뷰 - 뷰모델 -> 바인드
        //뷰모델 - 모딜 -> 
        public void Bind(ViewModel viewModel)
        {
            _viewModel = viewModel;
            
            //데이터 바인딩
            _viewModel.OnHpViewChanged += OnHpChanged;
            _viewModel.OnLevelViewChanged += OnLevelChanged; 
            _viewModel.OnExpViewChanged += OnExpChanged;
            _viewModel.OnPlayerDead += OnPlayerDead;
            
            //초기값 세팅
            OnHpChanged(_viewModel.CurrentHp);
            OnLevelChanged(_viewModel.CurrentLevel);
            OnExpChanged(_viewModel.CurrentExp);
        }
        
        private void OpenWindow()
        {
            //메인 UI 활성화
            _mainContainer.style.display = DisplayStyle.Flex;
        }

        private void CloseWindow()
        {
            //메인 UI 비활성화
            _mainContainer.style.display = DisplayStyle.None;
        }
        
        private void OnClickDamage()
        {
            _viewModel.TakeDamage(10);
        }

        private void OnHpChanged(int hp)
        {
            _hpText.text = _viewModel.HpPercentText;
        }
        
        private void OnLevelChanged(int level)
        {
            _levelText.text = $"LV : {level}";
        }
        
        private void OnExpChanged(float exp)
        {
            _expBar.value = exp;
            _expBar.title = $"{exp} / 100";
        }

        private void OnPlayerDead()
        {
            Debug.Log("플레이어 사망");
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.OnHpViewChanged -= OnHpChanged;
                _viewModel.OnLevelViewChanged -= OnLevelChanged;
                _viewModel.OnExpViewChanged -= OnExpChanged;
                _viewModel.OnPlayerDead -= OnPlayerDead;
            }
            
            if (_openButton != null) _openButton.clicked -= OpenWindow;
            if (_closeButton != null) _closeButton.clicked -= CloseWindow;
            if (_damageButton != null) _damageButton.clicked -= OnClickDamage;
        }
    }
}