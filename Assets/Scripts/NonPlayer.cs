using UnityEngine;

public class NonPlayer : Character
{
    private PlayerHP hpBar;

    private void Start()
    {
        hpBar = Instantiate(Resources.Load<GameObject>("HP_Bar")).GetComponent<PlayerHP>();
        hpBar.SetTarget(this);
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                isMoving = false;
        }
    }

    protected override void OnDeath()
    {
        //���� ó�� (=���� ��׷� ����)
        isDead = true;

        //sprite�� ���� ����
        Material material = Resources.Load<Material>("TransparentMaterial");
        material.SetFloat("_Transparency", 0.25f);
        GetComponentInChildren<SpriteRenderer>().material = material;

        //�� ��� ���
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        collider.excludeLayers = 1 << LayerMask.NameToLayer("Obstacle");

        //�÷��̾ ��� �׾����� Ȯ��
        GameSystem.gameSystem.CheckPlayerIsAllDead();
    }
}
