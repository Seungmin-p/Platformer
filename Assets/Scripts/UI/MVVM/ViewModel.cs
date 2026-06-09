using UnityEngine;
using System;

namespace MVVM
{
    public class ViewModel
    {
        private readonly Model _model;

        //View에서 바인딩할 이벤트들
        public event Action<int> OnHpViewChanged;
        public event Action<int> OnLevelViewChanged;
        public event Action<float> OnExpViewChanged;
        public event Action OnPlayerDead;

        public int CurrentHp => _model._hp;
        public string HpPercentText => $"{_model._hp} / 100";
        public int CurrentLevel => _model._level;
        public float CurrentExp => _model._exp;

        public ViewModel(Model model)
        {
            _model = model;
            
            //모델의 데이터가 직접적으로 변경됐을 경우 통지하는 이벤트 추가 필요
            _model.OnHpChanged += HandleModelHpChanged;
            _model.OnLevelChanged += HandleModelLevelChanged;
            _model.OnExpChanged += HandleModelExpChanged;
        }

        public void TakeDamage(int damage)
        {
            _model.DecreaseHp(damage);
            _model.GainExp(5);
        }

        private void HandleModelHpChanged(int hp)
        {
            OnHpViewChanged?.Invoke(hp);
            
            if(hp <= 0)
                OnPlayerDead?.Invoke();
        }

        private void HandleModelLevelChanged(int level)
        {
            OnLevelViewChanged?.Invoke(level);
        }

        private void HandleModelExpChanged(int exp)
        {
            OnExpViewChanged?.Invoke(exp);
        }
        
        //TODO : 언바인드
        public void UnBind()
        {
            _model.OnHpChanged -= HandleModelHpChanged;
            _model.OnLevelChanged -= HandleModelLevelChanged;
            _model.OnExpChanged -= HandleModelExpChanged;
        }
    }
}
