using UnityEngine;

public class Increase_Defensive : Upgrade
{
    public override void Activate()
    {
        Player.player.defensive_Power += 1f;
        upgrade_Level++;
    }
}
