public class Upgrade_HP : Upgrade
{
    public override void Activate()
    {
        Player.player.current_Health_Point += 5f;
        upgrade_Level++;
    }
}
