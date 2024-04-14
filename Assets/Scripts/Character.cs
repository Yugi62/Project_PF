using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private float _health_Point;       //ü��
    [SerializeField] private float _attack_Damage;      //���ݷ�
    [SerializeField] private float _defensive_Power;    //����
    [SerializeField] private float _movement_Speed;     //�̵��ӵ�


    /*
    ������Ƽ)
    */
    public float health_Point
    {
        get { return _health_Point; }

        set { _health_Point = value;

            //ü���� 0���Ϸ� ������ ��� ���� ó��
            if (_health_Point <= 0f)
                OnDeath();        
        }
    }
    public float attack_Damage
    { 
        get { return _attack_Damage; } 
        set { _attack_Damage = value;}  
    }
    public float movement_Speed
    { 
        get { return _movement_Speed; }
        set { _movement_Speed = value;}
    }
    public float defensive_Power
    { 
        get { return defensive_Power; } 
        set { defensive_Power = value;}
    }

    /*
    �߻� �޼ҵ�)
    */
    protected abstract void OnDeath();
}
