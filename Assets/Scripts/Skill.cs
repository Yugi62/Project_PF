using Unity.VisualScripting;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [SerializeField] private float _skill_Coefficient;          //스킬 계수
    [SerializeField] private float _skill_Speed;                //스킬 속도 (=투사체 발사 속도로 없는 경우 0f)
    [SerializeField] private float _cool_Time;                  //쿨타임

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

        //스킬 속도 초기화
        newProjectile.GetComponent<Projectile>().projectile_Speed = _skill_Speed;

        return newProjectile;
    }
}
