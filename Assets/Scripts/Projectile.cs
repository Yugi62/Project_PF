using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;            
    protected float _projectile_Damage;                 //투사체 공격력
    protected float _projectile_Speed;                  //투사체 속도 (없는 경우 0f)

    public float projectile_Damage
    {
        get { return _projectile_Damage; }
        set { _projectile_Damage = value; }
    }

    public float projectile_Speed
    {
        get { return _projectile_Speed; }
        set { _projectile_Speed = value; }
    }

    protected void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
