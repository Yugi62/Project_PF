using UnityEditor.Connect;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    protected Player _shooter;
    protected float _projectile_Damage;

    public Player shooter
    {
        get { return _shooter; }
        set { _shooter = value; }
    }

    public float projectile_Damage
    {
        get { return _projectile_Damage; }
        set { _projectile_Damage = value; }
    }

    protected void Init()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
