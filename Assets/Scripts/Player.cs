using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : Character
{
    [SerializeField] private Image EXP_UI;                      //경험치 UI
    [SerializeField] private UpgradeSystem upgradeSystem;       //업그레이드 시스템
    [SerializeField] private int _player_Level = 1;             //플레이어 레벨
    [SerializeField] private float _current_XP = 0;             //현재 경험치
    [SerializeField] private float _required_XP = 5;            //요구 경험치
    [SerializeField] private float knockbackForce = 30f;        //넉백 거리
    [SerializeField] private float knockbackDuration = 0.1f;    //넉백 지속 시간
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private int blinkCount = 5;

    private Rigidbody2D rb;                                     //리지드 바디
    private Material material;                                  //쉐이더 적용용 메테리얼

    private bool isHit = false;                                 //타격된 유무 (= 현재 무적인 상태)
    private PlayerHP hpBar;                                     //인게임 HP 표시 UI

    //플레이어 싱글톤 구현
    public static Player player;

    private void Awake()
    {
        if(player == null)
        {
            player = this;

            if (ClientSystem.clientSystem != null)
            {
                player.name = ClientSystem.clientSystem.playerName;
                player.GetComponentInChildren<SpriteRenderer>().sprite = ClientSystem.clientSystem.playerSprite;
                player.GetComponentInChildren<TMP_Text>().text = player.name;
            }
        }
        else
            Destroy(player);

        rb = GetComponent<Rigidbody2D>();
        material = GetComponentInChildren<SpriteRenderer>().material;
        isDead = false;
    }

    private void Start()
    {
        hpBar = Instantiate(Resources.Load<GameObject>("HP_Bar")).GetComponent<PlayerHP>();
        hpBar.SetTarget(this);
    }


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
        //죽음 처리 (=몬스터 어그로 해제)
        isDead = true;

        //모든 스킬 삭제 (=모든 공격 취소)
        GameSystem.gameSystem.DestroyAllSkills();

        //sprite의 투명도 조절
        material = Resources.Load<Material>("TransparentMaterial");
        material.SetFloat("_Transparency", 0.25f);
        GetComponentInChildren<SpriteRenderer>().material = material;

        //벽 통과 허용
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        collider.excludeLayers = 1 << LayerMask.NameToLayer("Obstacle");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //살아있는 경우에만
        if (!isDead)
        {
            //몬스터와 겹친 경우
            if (LayerMask.NameToLayer("Monster") == collision.gameObject.layer)
            {
                //무적이 아닌 경우
                if (!isHit)
                {
                    //고정 5데미지 (임시)
                    int tempDamage = 5;

                    //1. 서버에게 적용된 데미지를 전송 
                    if (ClientSystem.clientSystem != null)
                    {
                        ClientSystem.clientSystem.SendToServer(this.name + "~" + tempDamage.ToString(), ClientSystem.EchoType.ATTACK, true);
                    }

                    //2. 플레이어의 HP 감소
                    current_Health_Point -= tempDamage;

                    //3. 물리 적용
                    OnHit(collision.transform);
                }
            }
        }
    }

    private void OnHit(Transform target)
    {
        isHit = true;

        //반짝거리는 효과 및 그 시간 동안 무적 부여
        StartCoroutine(Blink());
        
        //넉백 적용
        StartCoroutine(StartKnockback(target));
    }

    private IEnumerator StartKnockback(Transform target)
    {
        Vector2 initVelocity = rb.velocity;
        Vector3 direction = transform.position - target.position;

        float timer = 0f;

        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

        while(timer < knockbackDuration)
        {
            timer += Time.deltaTime; 

            yield return null;
        }

        rb.velocity = initVelocity;
    }

    private IEnumerator Blink()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            material.SetFloat("_Blink", 1);
            yield return new WaitForSeconds(blinkDuration);
            material.SetFloat("_Blink", 0);
            yield return new WaitForSeconds(blinkDuration);
        }

        isHit = false;
    }
}
