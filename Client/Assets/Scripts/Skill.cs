using Unity.VisualScripting;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [SerializeField] private float _skill_Coefficient;          //��ų ���
    [SerializeField] private float _cool_Time;                  //��Ÿ��

    public float skill_Coefficient { get { return _skill_Coefficient; } set { _skill_Coefficient = value; } }

    protected float cool_Time { get { return _cool_Time; } }

    protected GameObject projectile;                            //����ü ������Ʈ
    protected float timerTime = 0f;                             //��Ÿ�� ���� float

    protected abstract void EnableSkill();

    protected GameObject CreateProjectile()
    {
        //projectile ����
        GameObject newProjectile = Instantiate(projectile);

        //����� ���� projectile ���ݷ� �ʱ�ȭ
        newProjectile.GetComponent<Projectile>().projectile_Damage = Player.player.attack_Damage * _skill_Coefficient;

        newProjectile.GetComponent<Projectile>().isPlayer = true;

        return newProjectile;
    }

    protected void FollowPlayer()
    {
        gameObject.transform.position = Player.player.transform.position;
    }
}
