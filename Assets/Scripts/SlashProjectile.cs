using UnityEngine;

public class SlashProjectile : Projectile
{
    private void Start()
    {
        base.Init();
    }

    private void FixedUpdate()
    {
        //�ִϸ��̼� ���� �� Destroy
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            Destroy(gameObject);
    }
}
