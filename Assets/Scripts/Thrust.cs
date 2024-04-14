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
        
        //2. ��ġ �ʱ�ȭ
        thrustProjectile.transform.position = transform.position;

        //3. ���� �ʱ�ȭ
        Vector2 direction = target.position - thrustProjectile.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90);
        thrustProjectile.transform.rotation = rotation;        

        //4. ����ü �߻�
        thrustProjectile.GetComponent<ThrustProjectile>().Shoot(direction.normalized);
    }
}
