using Unity.VisualScripting;
using UnityEngine;

public class Monster : Character
{
    [SerializeField] private float _drop_XP;        //드롭 경험치

    private Player shooter;                         //스킬 시전자

    protected override void OnDeath()
    {
        //삭제 및 스킬 시전자의 경험치 증가
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
