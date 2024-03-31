using UnityEngine;

public class Player : Character
{
    [SerializeField] private int _player_Level;     //플레이어 레벨
    [SerializeField] private float _current_XP;     //현재 경험치
    [SerializeField] private float _required_XP;    //요구 경험치
}
