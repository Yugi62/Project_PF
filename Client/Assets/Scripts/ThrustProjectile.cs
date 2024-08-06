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

    public override void Shoot(Vector2 target, Vector2 shooter, int direction)
    {
        //��ġ �ʱ�ȭ
        transform.position = shooter;

        //���� �ʱ�ȭ
        Vector2 dir = (Vector3)target - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90);
        transform.rotation = rotation;

        //����ü �߻�
        rb2D.velocity = dir * _projectile_Speed;
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
