using UnityEngine;

public class Upgrade_Slash : Upgrade
{
    GameObject slash;
    public override void Activate()
    {
        if (upgrade_Level == 1)
        {
            slash = Instantiate(Resources.Load<GameObject>("Slash"), Player.player.GetComponentInChildren<SpriteRenderer>().gameObject.transform);
            GameSystem.gameSystem.skills.Add(slash);
            slash.name = "Slash";
        }
        else
        {
            slash.GetComponent<Skill>().skill_Coefficient += 1;
        }
        upgrade_Level++;
    }
}
