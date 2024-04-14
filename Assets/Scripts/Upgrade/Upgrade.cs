using UnityEngine;

public abstract class Upgrade : MonoBehaviour
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _description;
    [SerializeField] private int _upgrade_Level = 1;    

    public Sprite icon{ get { return _icon; } }

    protected int upgrade_Level 
    { 
        get { return _upgrade_Level; }
        set { _upgrade_Level = value; } 
    }

    public abstract void Activate();
}