public class Upgrade_Speed : Upgrade
{
    public override void Activate()
    {
        Player.player.movement_Speed += 0.02f;
        upgrade_Level++;
    }
}
