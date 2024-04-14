using UnityEngine;

public class Increase_Damage : Upgrade
{
    public override void Activate()
    {
        Player.player.attack_Damage += 1.5f;
        upgrade_Level++;        
    }
}
