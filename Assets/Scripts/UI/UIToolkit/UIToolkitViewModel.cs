using System;
using Unity.Properties;
using UnityEngine;

public class UIToolkitViewModel
{
    private UIToolkitModel _model;

    [CreateProperty] public string HpPercentText => $"{_model._hp} / 100";
    [CreateProperty] public float HpSliderValue => _model._hp;
    [CreateProperty] public string LevelText => $"LV : {_model._level}";
    [CreateProperty] public float ExpSliderValue => _model._exp;
    
    public UIToolkitViewModel(UIToolkitModel model)
    {
        _model = model;
    }

    public void ExecuteTakeDamage(int damage)
    {
        _model.DecreaseHp(damage);
        _model.GainExp(5);
    }
}
