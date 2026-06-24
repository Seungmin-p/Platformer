using UnityEngine;

public class UIToolkitModel : MonoBehaviour
{
    public int _hp;
    public int _level;
    public int _exp;
    private const int MaxExp = 100;

    public void DecreaseHp(int amount)
    {
        _hp -= amount;
    }

    public void GainExp(int amount)
    {
        if(_hp <= 0) return;
        
        int newExp = _exp + amount;

        while (newExp >= MaxExp)
        {
            newExp -= MaxExp;
            _level++;
            _hp = 100;
        }
        
        _exp = newExp;
    }
}
