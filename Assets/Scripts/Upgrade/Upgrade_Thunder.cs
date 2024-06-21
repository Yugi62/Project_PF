using UnityEngine;

public class Upgrade_Thunder : Upgrade
{
    GameObject thunder;
    public override void Activate()
    {
        if(upgrade_Level == 1)
        {
            thunder = Instantiate(Resources.Load<GameObject>("Thunder"));
            GameSystem.gameSystem.skills.Add(thunder);
            thunder.name = "Thunder";
            thunder.transform.position = Player.player.transform.position;         
        }
        else
        {
            thunder.GetComponent<Skill>().skill_Coefficient += 1;
        }
        upgrade_Level++;
    }
}
