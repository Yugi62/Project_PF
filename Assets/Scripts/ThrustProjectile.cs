using UnityEngine;

public class ThrustProjectile : Projectile
{
    [SerializeField] private float projectileDestroyTime;
    private float timerTime = 0f;

    private Rigidbody2D rb2D;

    private void Awake()
    {
        base.Init();
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        //일정 시간 경과 후에도 충돌하지 않는 경우 제거
        if(timerTime >= projectileDestroyTime)
        {
            Destroy(gameObject);
        }
        timerTime += Time.deltaTime;
    }

    public void Shoot(Vector2 direction)
    {
        rb2D.velocity = direction * _projectile_Speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //투사체 충돌 시 제거 (데미지 계산은 Monster가 수행)
        if (LayerMask.NameToLayer("Monster") == collision.gameObject.layer)
        {
            Destroy(gameObject);
        }
    }
}
