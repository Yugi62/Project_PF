using UnityEngine;
using UnityEngine.UI;

public class Player : Character
{
    //플레이어 싱글톤 구현
    public static Player player;

    private void Awake()
    {
        if(player == null)
        {
            player = this;
            DontDestroyOnLoad(player);
        }
        else
            Destroy(player);
    }

    [SerializeField] private Image EXP_UI;                      //경험치 UI
    [SerializeField] private UpgradeSystem upgradeSystem;       //업그레이드 시스템
    [SerializeField] private int _player_Level;                 //플레이어 레벨
    [SerializeField] private float _current_XP;                 //현재 경험치
    [SerializeField] private float _required_XP;                //요구 경험치

    public float current_XP
    {
        get { return _current_XP; }

        set { _current_XP = value;

            //경험치 UI 갱신
            EXP_UI.fillAmount = _current_XP / _required_XP;

            //요구 경험치에 도달 시 레벨업
            if(_current_XP >= _required_XP)            
                LevelUp();                 
        }
    }

    private void LevelUp()
    {
        //레벨업 시 업그레이드 UI 출력
        upgradeSystem.Draw();

        //요구 경험치 증가 및 현재 경험치를 0으로 초기화
        current_XP = 0;
        _required_XP *= 1.5f;
    }

    protected override void OnDeath()
    {
        
    }
}
