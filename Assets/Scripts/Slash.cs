using UnityEngine;

public class Slash : Skill
{
    private void Start()
    {
        base.Init();
        projectile = Resources.Load<GameObject>("SlashProjectile");
    }

    private void FixedUpdate()
    {
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
        GameObject newSlash = CreateProjectile();

        //���� ��ġ ����
        newSlash.transform.position = this.transform.position;

        //�÷��̾ ������ ������ ��� �Ȱ��� ����
        Vector3 newscale = player.transform.localScale;
        newSlash.transform.localScale = newscale;
    }
}
