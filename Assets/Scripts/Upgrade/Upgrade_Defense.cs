public class Upgrade_Defense : Upgrade
{
    public override void Activate()
    {
        Player.player.defensive_Power += 1f;
        upgrade_Level++;
    }
}
