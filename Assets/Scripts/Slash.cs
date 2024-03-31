using UnityEngine;

public class Slash : Skill
{
    private void Start()
    {
        player = GetComponentInParent<Player>();
    }
}
