using UnityEngine;

public class Thrust : Skill
{
    private int layerMask;
    private BoxCollider2D boxCollider;
    private Transform target = null;

    private void Start()
    {
        projectile = Resources.Load<GameObject>("ThrustProjectile");
        layerMask = 1 << LayerMask.NameToLayer("Monster");
        boxCollider = GetComponent<BoxCollider2D>();
    }
    private void FixedUpdate()
    {
        FollowPlayer();

        //�ֺ��� ���� �ִ��� Ž��
        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, 0f, layerMask);

        //���� �����ϸ鼭 ��Ÿ���� ������ �� ��ų �ߵ�
        if (colliders.Length > 0)
        {
            if (timerTime >= cool_Time)
            {
                target = colliders[0].GetComponent<Transform>();
                EnableSkill();
                timerTime = 0f;
            }
        }

        timerTime += Time.deltaTime;
    }

    protected override void EnableSkill()
    {
        //1. ����ü ����
        GameObject thrustProjectile = CreateProjectile();

        //2. �������� ����
        if (ClientSystem.clientSystem != null)
            ClientSystem.clientSystem.SendToServer(
                "ThrustProjectile" + "~" +
                target.position.x.ToString("F2") + "~" +
                target.position.y.ToString("F2") + "~" +
                transform.position.x.ToString("F2") + "~" +
                transform.position.y.ToString("F2") + "~" +
                 ((int)transform.localScale.x).ToString(),
                ClientSystem.EchoType.PROJECTILE, false);

        //3. ����ü �߻�
        thrustProjectile.GetComponent<ThrustProjectile>().Shoot(target.position, transform.position, (int)transform.localScale.x);  
    }
}
