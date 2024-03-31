using UnityEngine;

public class Slash : Skill
{
    private BoxCollider2D boxCollider;

    private void Start()
    {
        base.Init();

        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        //�ִϸ��̼� ���� �� sprite ��Ȱ��ȭ
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            spriteRenderer.enabled = false;

        //��Ÿ�Ӹ��� ��ų �ߵ�
        if (current_Time >= cool_Time)
        {
            EnableSkill();
            current_Time = 0f;
        }
        current_Time += Time.deltaTime;
    }

    protected override void EnableSkill()
    {
        spriteRenderer.enabled = true;
        animator.SetTrigger("Slash");
    }

}
