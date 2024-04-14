using UnityEngine;

public class Slash : Skill
{
    private void Start()
    {
        projectile = Resources.Load<GameObject>("SlashProjectile");
    }

    private void FixedUpdate()
    {
        //��Ÿ�Ӹ��� ��ų �ߵ�
        if (timerTime >= cool_Time)
        {
            EnableSkill();
            timerTime = 0f;
        }
        timerTime += Time.deltaTime;
    }

    protected override void EnableSkill()
    {
        GameObject newSlash = CreateProjectile();

        //���� ��ġ ����
        newSlash.transform.position = this.transform.position;

        //�÷��̾ ������ ������ ��� �Ȱ��� ����
        Vector3 newscale = Player.player.transform.localScale;
        newSlash.transform.localScale = newscale;
    }
}
