using Unity.VisualScripting;
using UnityEngine;

public class Monster : Character
{
    [SerializeField] private float _drop_XP;        //��� ����ġ

    protected override void OnDeath()
    {
        //���� �� ��ų �������� ����ġ ����
        Destroy(gameObject);

        //04.08 ���� ��Ÿ ģ ����� ����ġ�� �÷��ְ� �����ؾ� �Ѵ� (���� ���� �� ���� ��)
        Player.player.current_XP += _drop_XP;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerMask.NameToLayer("Skill") == collision.gameObject.layer)
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();

            this.health_Point -= projectile.projectile_Damage;            
        }
    }
}
