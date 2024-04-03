using Unity.VisualScripting;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [SerializeField] private float _skill_Coefficient;      //��ų���
    [SerializeField] private float _cool_Time;              //��Ÿ��

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
        //projectile ����
        GameObject newProjectile = Instantiate(projectile);

        //����� ���� projectile ���ݷ� �ʱ�ȭ
        newProjectile.GetComponent<Projectile>().projectile_Damage = player.attack_Damage * _skill_Coefficient;

        //��ų ������ �ʱ�ȭ
        newProjectile.GetComponent<Projectile>().shooter = player;

        return newProjectile;
    }
}
