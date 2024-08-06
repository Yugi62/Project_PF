using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private float _max_Heath_Point;        //�ִ� ü�� 
    [SerializeField] private float _current_Health_Point;   //���� ü�� 

    [SerializeField] private float _attack_Damage;          //���ݷ�
    [SerializeField] private float _defensive_Power;        //����
    [SerializeField] private float _movement_Speed;         //�̵��ӵ�

    public Vector2 targetPosition;                          //�̵��� ��ġ
    public bool isMoving = false;
    public float speed = 0.25f;



    /*
    ������Ƽ)
    */

    public float max_heath_Point
    {
        get { return _max_Heath_Point; }
    }

    public float current_Health_Point
    {
        get { return _current_Health_Point; }

        set {
            //1. ����� ü�� ����
            _current_Health_Point = value;

            //2. ü���� 0���Ϸ� ������ ��� ���� ó��
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

    public bool isDead { get; protected set; }                   //�÷��̾� ��� ����


    /*
    �߻� �޼ҵ�)
    */
    protected abstract void OnDeath();

    /*
    �޼ҵ�)
    */

    public void MoveSmooth()
    {

    }



}
