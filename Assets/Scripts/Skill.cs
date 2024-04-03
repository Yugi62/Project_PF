using Unity.VisualScripting;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [SerializeField] private float _skill_Coefficient;      //스킬계수
    [SerializeField] private float _cool_Time;              //쿨타임

    protected float cool_Time { get { return _cool_Time; } }

    protected Player player;
    protected GameObject projectile;
    protected float current_Time;

    protected void Init()
    {
        player = GetComponentInParent<Player>();
        current_Time = 0f;
    }

    protected abstract void EnableSkill();

    protected GameObject CreateProjectile()
    {
        //projectile 생성
        GameObject newProjectile = Instantiate(projectile);

        //계수에 따른 projectile 공격력 초기화
        newProjectile.GetComponent<Projectile>().projectile_Damage = player.attack_Damage * _skill_Coefficient;

        //스킬 시전자 초기화
        newProjectile.GetComponent<Projectile>().shooter = player;

        return newProjectile;
    }
}
