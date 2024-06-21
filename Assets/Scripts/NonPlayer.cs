using UnityEngine;

public class NonPlayer : Character
{
    private PlayerHP hpBar;

    private void Start()
    {
        hpBar = Instantiate(Resources.Load<GameObject>("HP_Bar")).GetComponent<PlayerHP>();
        hpBar.SetTarget(this);
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
    }
}
