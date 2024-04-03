using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private float _health_Point;       //체력
    [SerializeField] private float _attack_Damage;      //공격력
    [SerializeField] private float _defensive_Power;    //방어력
    [SerializeField] private float _movement_Speed;     //이동속도

    public float health_Point
    {
        get { return _health_Point; }

        set { _health_Point = value;

            //체력이 0이하로 떨어진 경우 죽음 처리
            if (_health_Point <= 0f)
                OnDeath();        
        }
    }

    public float attack_Damage
    { get { return _attack_Damage; } }


    public float movement_Speed
    { get { return _movement_Speed; } }


    protected abstract void OnDeath();
}
