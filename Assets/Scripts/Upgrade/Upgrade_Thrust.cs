using UnityEngine;

public class Upgrade_Thrust : Upgrade
{
    GameObject thrust;
    public override void Activate()
    {
        if (upgrade_Level == 1)
        {
            thrust = Instantiate(Resources.Load<GameObject>("Thrust"));
            GameSystem.gameSystem.skills.Add(thrust);
            thrust.name = "Thrust";
            thrust.transform.position = Player.player.transform.position;
        }
        else
        {
            thrust.GetComponent<Skill>().skill_Coefficient += 1;
        }
        upgrade_Level++;
    }
}
