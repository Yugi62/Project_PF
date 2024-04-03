using Unity.VisualScripting;
using UnityEngine;

public class Monster : Character
{
    [SerializeField] private float _drop_XP;        //��� ����ġ

    private Player shooter;                         //��ų ������

    protected override void OnDeath()
    {
        //���� �� ��ų �������� ����ġ ����
        Destroy(gameObject);
        shooter.current_XP += _drop_XP;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerMask.NameToLayer("Skill") == collision.gameObject.layer)
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();

            shooter = projectile.shooter;

            this.health_Point -= projectile.projectile_Damage;            
        }
    }
}
