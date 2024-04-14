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
        //���� �ð� ��� �Ŀ��� �浹���� �ʴ� ��� ����
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
        //����ü �浹 �� ���� (������ ����� Monster�� ����)
        if (LayerMask.NameToLayer("Monster") == collision.gameObject.layer)
        {
            Destroy(gameObject);
        }
    }
}
