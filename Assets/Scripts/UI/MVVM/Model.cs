using UnityEngine;
using System;

namespace MVVM
{
    public class Model : MonoBehaviour
    {
        public event Action<int> OnHpChanged;
        public event Action<int> OnLevelChanged;
        public event Action<int> OnExpChanged;
        
        public int _hp;
        public int _exp;
        public int _level;

        public Model(int hp, int exp, int level)
        {
            _hp = hp;
            _exp = exp;
            _level = level;
        }
        
        public void DecreaseHp(int hp)
        {
            if(_hp <= 0) return;
            
            _hp -= hp;
            OnHpChanged?.Invoke(_hp);
        }

        public void GainExp(int exp)
        {
            if(_hp <= 0) return;
            
            int newExp = _exp + exp;

            while (newExp >= 100)
            {
                newExp -= 100;
                _level++;
                _hp = 100;
                
                OnLevelChanged?.Invoke(_level);
                OnHpChanged?.Invoke(_hp);
            }
            
            _exp = newExp;
            OnExpChanged?.Invoke(_exp);
        }
    }
}