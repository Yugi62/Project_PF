using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private int _health_Point;         //ü��
    [SerializeField] private float _attack_Damage;      //���ݷ�
    [SerializeField] private float _defensive_Power;    //����
    [SerializeField] private float _movement_Speed;     //�̵��ӵ�

    public float attack_Damage
    { get { return _attack_Damage; } }

    public float movement_Speed
    { get { return _movement_Speed; } }
}
