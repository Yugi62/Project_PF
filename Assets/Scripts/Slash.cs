using UnityEngine;

public class Slash : Skill
{
    private void Start()
    {
        base.Init();
        projectile = Resources.Load<GameObject>("SlashProjectile");
    }

    private void FixedUpdate()
    {
        //쿨타임마다 스킬 발동
        if (current_Time >= cool_Time)
        {
            EnableSkill();
            current_Time = 0f;
        }
        current_Time += Time.deltaTime;
    }

    protected override void EnableSkill()
    {
        GameObject newSlash = CreateProjectile();

        //생성 위치 조정
        newSlash.transform.position = this.transform.position;

        //플레이어가 반전된 상태인 경우 똑같이 반전
        Vector3 newscale = player.transform.localScale;
        newSlash.transform.localScale = newscale;
    }
}
