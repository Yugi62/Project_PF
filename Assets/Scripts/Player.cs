using UnityEngine;
using UnityEngine.UI;

public class Player : Character
{
    //�÷��̾� �̱��� ����
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

    [SerializeField] private Image EXP_UI;                      //����ġ UI
    [SerializeField] private UpgradeSystem upgradeSystem;       //���׷��̵� �ý���
    [SerializeField] private int _player_Level;                 //�÷��̾� ����
    [SerializeField] private float _current_XP;                 //���� ����ġ
    [SerializeField] private float _required_XP;                //�䱸 ����ġ

    public float current_XP
    {
        get { return _current_XP; }

        set { _current_XP = value;

            //����ġ UI ����
            EXP_UI.fillAmount = _current_XP / _required_XP;

            //�䱸 ����ġ�� ���� �� ������
            if(_current_XP >= _required_XP)            
                LevelUp();                 
        }
    }

    private void LevelUp()
    {
        //������ �� ���׷��̵� UI ���
        upgradeSystem.Draw();

        //�䱸 ����ġ ���� �� ���� ����ġ�� 0���� �ʱ�ȭ
        current_XP = 0;
        _required_XP *= 1.5f;
    }

    protected override void OnDeath()
    {
        
    }
}
