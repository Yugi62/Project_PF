using UnityEngine;

public class Increase_HP : Upgrade
{
    public override void Activate()
    {
        Player.player.health_Point += 5f;
        upgrade_Level++;
    }
}
