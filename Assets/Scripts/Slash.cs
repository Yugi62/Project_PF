using UnityEngine;

public class Slash : Skill
{
    private void Start()
    {
        projectile = Resources.Load<GameObject>("SlashProjectile");
    }

    private void FixedUpdate()
    {
        //쿨타임마다 스킬 발동
        if (timerTime >= cool_Time)
        {
            EnableSkill();
            timerTime = 0f;
        }
        timerTime += Time.deltaTime;
    }

    protected override void EnableSkill()
    {
        GameObject newSlash = CreateProjectile();

        //생성 위치 조정
        newSlash.transform.position = this.transform.position;

        //플레이어가 반전된 상태인 경우 똑같이 반전
        Vector3 newscale = Player.player.transform.localScale;
        newSlash.transform.localScale = newscale;
    }
}
