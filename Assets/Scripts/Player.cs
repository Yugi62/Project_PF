using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : Character
{
    [SerializeField] private Image EXP_UI;                      //����ġ UI
    [SerializeField] private UpgradeSystem upgradeSystem;       //���׷��̵� �ý���
    [SerializeField] private int _player_Level = 1;             //�÷��̾� ����
    [SerializeField] private float _current_XP = 0;             //���� ����ġ
    [SerializeField] private float _required_XP = 5;            //�䱸 ����ġ
    [SerializeField] private float knockbackForce = 30f;        //�˹� �Ÿ�
    [SerializeField] private float knockbackDuration = 0.1f;    //�˹� ���� �ð�
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private int blinkCount = 5;

    private Rigidbody2D rb;                                     //������ �ٵ�
    private Material material;                                  //���̴� ����� ���׸���

    private bool isHit = false;                                 //Ÿ�ݵ� ���� (= ���� ������ ����)
    private PlayerHP hpBar;                                     //�ΰ��� HP ǥ�� UI

    //�÷��̾� �̱��� ����
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
        //���� ó�� (=���� ��׷� ����)
        isDead = true;

        //��� ��ų ���� (=��� ���� ���)
        GameSystem.gameSystem.DestroyAllSkills();

        //sprite�� ���� ����
        material = Resources.Load<Material>("TransparentMaterial");
        material.SetFloat("_Transparency", 0.25f);
        GetComponentInChildren<SpriteRenderer>().material = material;

        //�� ��� ���
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        collider.excludeLayers = 1 << LayerMask.NameToLayer("Obstacle");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //����ִ� ��쿡��
        if (!isDead)
        {
            //���Ϳ� ��ģ ���
            if (LayerMask.NameToLayer("Monster") == collision.gameObject.layer)
            {
                //������ �ƴ� ���
                if (!isHit)
                {
                    //���� 5������ (�ӽ�)
                    int tempDamage = 5;

                    //1. �������� ����� �������� ���� 
                    if (ClientSystem.clientSystem != null)
                    {
                        ClientSystem.clientSystem.SendToServer(this.name + "~" + tempDamage.ToString(), ClientSystem.EchoType.ATTACK, true);
                    }

                    //2. �÷��̾��� HP ����
                    current_Health_Point -= tempDamage;

                    //3. ���� ����
                    OnHit(collision.transform);
                }
            }
        }
    }

    private void OnHit(Transform target)
    {
        isHit = true;

        //��¦�Ÿ��� ȿ�� �� �� �ð� ���� ���� �ο�
        StartCoroutine(Blink());
        
        //�˹� ����
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
