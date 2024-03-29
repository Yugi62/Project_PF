using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private int _health_Point;      //ü��
    [SerializeField] private int _attack_Damage;     //���ݷ�
    [SerializeField] private int _defensive_Power;   //����
    [SerializeField] private float _movement_Speed;  //�̵��ӵ�

    public float movement_Speed
    { get { return _movement_Speed; } }
}
