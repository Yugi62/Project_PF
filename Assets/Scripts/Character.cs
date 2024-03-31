using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private int _health_Point;         //체력
    [SerializeField] private float _attack_Damage;      //공격력
    [SerializeField] private float _defensive_Power;    //방어력
    [SerializeField] private float _movement_Speed;     //이동속도

    public float attack_Damage
    { get { return _attack_Damage; } }

    public float movement_Speed
    { get { return _movement_Speed; } }
}
