using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private float _max_Heath_Point;        //최대 체력 
    [SerializeField] private float _current_Health_Point;   //현재 체력 

    [SerializeField] private float _attack_Damage;          //공격력
    [SerializeField] private float _defensive_Power;        //방어력
    [SerializeField] private float _movement_Speed;         //이동속도

    public Vector2 targetPosition;                          //이동할 위치
    public bool isMoving = false;
    public float speed = 0.25f;



    /*
    프로퍼티)
    */

    public float max_heath_Point
    {
        get { return _max_Heath_Point; }
    }

    public float current_Health_Point
    {
        get { return _current_Health_Point; }

        set {
            //1. 변경된 체력 적용
            _current_Health_Point = value;

            //2. 체력이 0이하로 떨어진 경우 죽음 처리
            if (_current_Health_Point <= 0f)
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
        get { return _defensive_Power; } 
        set { _defensive_Power = value;}
    }

    public bool isDead { get; protected set; }                   //플레이어 사망 유무


    /*
    추상 메소드)
    */
    protected abstract void OnDeath();

    /*
    메소드)
    */

    public void MoveSmooth()
    {

    }



}
