using UnityEngine;

public class ThunderProjectile : Projectile
{
    private Animator animator;

    private void Awake()
    {
        base.Init();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        //�ִϸ��̼� ���� �� Destroy
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            Destroy(gameObject);
    }

    public override void Shoot(Vector2 target, Vector2 shooter, int direction)
    {
        transform.position = target;
    }
}
