using Unity.VisualScripting;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [SerializeField] private float _skill_Coefficient;          //��ų ���
    [SerializeField] private float _skill_Speed;                //��ų �ӵ� (=����ü �߻� �ӵ��� ���� ��� 0f)
    [SerializeField] private float _cool_Time;                  //��Ÿ��

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

        //��ų �ӵ� �ʱ�ȭ
        newProjectile.GetComponent<Projectile>().projectile_Speed = _skill_Speed;

        return newProjectile;
    }
}
