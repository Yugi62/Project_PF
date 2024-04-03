using UnityEngine;

public class SlashProjectile : Projectile
{
    private void Start()
    {
        base.Init();
    }

    private void FixedUpdate()
    {
        //애니메이션 종료 시 Destroy
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            Destroy(gameObject);
    }
}
