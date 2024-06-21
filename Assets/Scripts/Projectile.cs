using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;

    [SerializeField] protected float _projectile_Speed;     //투사체 속도
    protected float _projectile_Damage;                     //투사체 공격력
    protected bool _isPlayer = false;                       //발사자가 현 플레이어인지


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

    public bool isPlayer
    {
        get { return _isPlayer; }
        set { _isPlayer = value; }
    }

    protected void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public abstract void Shoot(Vector2 target, Vector2 shooter, int direction);
}
