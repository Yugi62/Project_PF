using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

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
        //1. ����ü ����
        GameObject newSlash = CreateProjectile();

        //2. �������� ����
        if (ClientSystem.clientSystem != null)
            ClientSystem.clientSystem.SendToServer(
            "SlashProjectile" + "~" +
                transform.position.x.ToString("F2") + "~" +
                transform.position.y.ToString("F2") + "~" +
                transform.position.x.ToString("F2") + "~" +
                transform.position.y.ToString("F2") + "~" +
                 ((int)Player.player.transform.localScale.x).ToString(),
                ClientSystem.EchoType.PROJECTILE, false);

        //3. ����ü �߻�
        int dir = (int)Player.player.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x;
        newSlash.GetComponent<Projectile>().Shoot(transform.position, transform.position, dir);
    }
}
