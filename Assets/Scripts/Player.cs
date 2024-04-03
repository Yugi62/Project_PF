using UnityEngine;

public class Player : Character
{
    [SerializeField] private int _player_Level;     //�÷��̾� ����
    [SerializeField] private float _current_XP;     //���� ����ġ
    [SerializeField] private float _required_XP;    //�䱸 ����ġ

    public float current_XP
    {
        get { return _current_XP; }

        set { _current_XP = value;       
            
            //�䱸 ����ġ�� ���� �� ������
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
