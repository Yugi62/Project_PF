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
        //애니메이션 종료 시 Destroy
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            Destroy(gameObject);
    }

    public override void Shoot(Vector2 target, Vector2 shooter, int direction)
    {
        //생성 위치 조정
        transform.position = target;

        //플레이어가 반전된 상태인 경우 똑같이 반전
        Vector3 newscale = new Vector3(direction, 1, 1);
        transform.localScale = newscale;
    }
}
