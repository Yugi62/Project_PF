using UnityEngine;

public class SlashProjectile : Projectile
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
}
