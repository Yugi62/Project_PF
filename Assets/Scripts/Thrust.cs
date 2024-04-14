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
        //주변에 적이 있는지 탐색
        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, 0f, layerMask);

        //적이 존재하면서 쿨타임이 돌았을 때 스킬 발동
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
        //1. 투사체 생성
        GameObject thrustProjectile = CreateProjectile();
        
        //2. 위치 초기화
        thrustProjectile.transform.position = transform.position;

        //3. 방향 초기화
        Vector2 direction = target.position - thrustProjectile.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90);
        thrustProjectile.transform.rotation = rotation;        

        //4. 투사체 발사
        thrustProjectile.GetComponent<ThrustProjectile>().Shoot(direction.normalized);
    }
}
