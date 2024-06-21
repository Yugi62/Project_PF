using Unity.VisualScripting;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [SerializeField] private float _skill_Coefficient;          //스킬 계수
    [SerializeField] private float _cool_Time;                  //쿨타임

    public float skill_Coefficient { get { return _skill_Coefficient; } set { _skill_Coefficient = value; } }

    protected float cool_Time { get { return _cool_Time; } }

    protected GameObject projectile;                            //투사체 오브젝트
    protected float timerTime = 0f;                             //쿨타임 계산용 float

    protected abstract void EnableSkill();

    protected GameObject CreateProjectile()
    {
        //projectile 생성
        GameObject newProjectile = Instantiate(projectile);

        //계수에 따른 projectile 공격력 초기화
        newProjectile.GetComponent<Projectile>().projectile_Damage = Player.player.attack_Damage * _skill_Coefficient;

        newProjectile.GetComponent<Projectile>().isPlayer = true;

        return newProjectile;
    }

    protected void FollowPlayer()
    {
        gameObject.transform.position = Player.player.transform.position;
    }
}
