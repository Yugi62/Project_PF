using Unity.VisualScripting;
using UnityEngine;

public class Monster : Character
{
    [SerializeField] private float _drop_XP;        //드롭 경험치

    protected override void OnDeath()
    {
        //삭제 및 스킬 시전자의 경험치 증가
        Destroy(gameObject);

        //04.08 이후 막타 친 사람의 경험치만 올려주게 수정해야 한다 (서버 구현 후 만질 것)
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
