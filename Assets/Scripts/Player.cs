using UnityEngine;

public class Player : Character
{
    [SerializeField] private int _player_Level;     //플레이어 레벨
    [SerializeField] private float _current_XP;     //현재 경험치
    [SerializeField] private float _required_XP;    //요구 경험치

    public float current_XP
    {
        get { return _current_XP; }

        set { _current_XP = value;       
            
            //요구 경험치에 도달 시 레벨업
            if(_current_XP >= _required_XP)            
                LevelUp();                 
        }
    }

    private void LevelUp()
    {

    }

    protected override void OnDeath()
    {
        
    }
}
