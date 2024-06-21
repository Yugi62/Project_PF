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
        //죽음 처리 (=몬스터 어그로 해제)
        isDead = true;

        //sprite의 투명도 조절
        Material material = Resources.Load<Material>("TransparentMaterial");
        material.SetFloat("_Transparency", 0.25f);
        GetComponentInChildren<SpriteRenderer>().material = material;

        //벽 통과 허용
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        collider.excludeLayers = 1 << LayerMask.NameToLayer("Obstacle");
    }
}
