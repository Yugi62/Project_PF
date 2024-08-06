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

    public override void Shoot(Vector2 target, Vector2 shooter, int direction)
    {
        //���� ��ġ ����
        transform.position = target;

        //�÷��̾ ������ ������ ��� �Ȱ��� ����
        Vector3 newscale = new Vector3(direction, 1, 1);
        transform.localScale = newscale;
    }
}
