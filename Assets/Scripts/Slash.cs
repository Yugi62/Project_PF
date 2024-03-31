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
        //애니메이션 종료 시 sprite 비활성화
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            spriteRenderer.enabled = false;

        //쿨타임마다 스킬 발동
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
