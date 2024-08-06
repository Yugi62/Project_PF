using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Monster : Character
{
    [SerializeField] private float _drop_XP;        //��� ����ġ
    [SerializeField] private bool isAstar;          //Astar ������� �����̱� (false�� ��� ��ֹ��� �����ϰ� ���������� �÷��̾ �Ѿư���)
    [SerializeField] private float updatePathTime;  //���� ���� �ð�

    private Projectile currentHitProjectile;        //�ֱٿ� Ÿ�ݵ� ����ü    
    private Vector3 target;                         //Ÿ�� (=���Ͱ� ���󰡴� ���)
    private bool hasTarget = false;                 //Ÿ���� ���� ���� (=������ ����ִ� �÷��̾��� ���� ����)

    private Astar astar;
    private List<Node> path;
    private int currentIndex;

    private Material material;
    private GameObject damagePrefab;


    private bool isParalyzed = false;    

    private void FixedUpdate()
    {
        //������ ������ ��쿡�� �����δ�
        if (!isParalyzed)
        {
            //Ÿ���� �����ϸ� path��� �����δ�
            if (hasTarget && path != null)
            {
                target = new Vector3(path[currentIndex].x, path[currentIndex].y, 1);                
                
                Vector3 direction = (target - transform.position).normalized;

                  Transform spriteTransform = GetComponentInChildren<SpriteRenderer>().gameObject.transform;
                if (direction.x < 0)
                    spriteTransform.localScale = new Vector3(-1, 1, 1);
                else
                    spriteTransform.localScale = new Vector3(1, 1, 1);

                transform.position += direction * movement_Speed;

                //Ÿ�ٰ��� �Ÿ��� 0.1 ������ ��� ��ǥ�� ������ ������ �����Ѵ�
                if (Vector3.Distance(transform.position, target) < 0.1f)
                {
                    if (path.Count - 1 > currentIndex)
                        currentIndex++;
                }
            }
        }
    }

    private void Awake()
    {
        material = GetComponentInChildren<SpriteRenderer>().material;
        damagePrefab = Resources.Load<GameObject>("Damage");

        astar = new Astar();
    }

    private void Start()
    {
        //ȣ��Ʈ�� ��� ������ ��ġ�� Ư�� �ð����� ����ȭ
        if (ClientSystem.clientSystem != null && ClientSystem.clientSystem.isHost)
            InvokeRepeating("SyncPosition", 0f, 1.0f);

       if (updatePathTime > 0f)
            InvokeRepeating("CreatePath", 0f, updatePathTime);
    }

    public void CreatePath()
    {
        //���� ����� �÷��̾� Ž��
        FindClosestPlayer();

        if (target != null)
        {
            if (isAstar)
                path = astar.CreatePath(GameSystem.gameSystem.GetGrid(), GameSystem.gameSystem.WorldToGrid(transform.position), GameSystem.gameSystem.WorldToGrid(target));

            else
            {
                path = new List<Node>();
                path.Add(new Node(transform.position.x, transform.position.y, true));
                path.Add(new Node(target.x, target.y, true));
            }

            //0�� �� ��ġ�̹Ƿ� 1�� ����
            currentIndex = 1;
        }
    }

    private void SyncPosition()
    {
        if (ClientSystem.clientSystem != null)
            ClientSystem.clientSystem.SendToServer(
                gameObject.name + "~" +
                transform.position.x.ToString("F2") + "~" +
                transform.position.y.ToString("F2") + "~" +
                transform.localScale.x.ToString(),
                ClientSystem.EchoType.MOVE, false);
    }

    protected override void OnDeath()
    {
        //���� �� ��ų �������� ����ġ ����
        Destroy(gameObject);

        //�������� �� ����� �� �÷��̾��� ��쿡�� ����ġ ����
        if (currentHitProjectile != null && currentHitProjectile.isPlayer)
            Player.player.current_XP += _drop_XP;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        //��ų�� ���� ��� HP ����
        if (LayerMask.NameToLayer("Skill") == collision.gameObject.layer)
        {
            StartCoroutine(Paralyze());

            currentHitProjectile = collision.gameObject.GetComponent<Projectile>();

            this.current_Health_Point -= currentHitProjectile.projectile_Damage;

            //�������� 0�� ��� �ΰ��ӿ��� �������� ǥ������ ����
            if (currentHitProjectile.projectile_Damage > 0)
            {
                GameObject newDamage = Instantiate<GameObject>(damagePrefab);
                newDamage.GetComponent<TMP_Text>().text = currentHitProjectile.projectile_Damage.ToString();
                newDamage.transform.position = new Vector3(transform.position.x, transform.position.y + 0.25f, -5f);
            }

            if (ClientSystem.clientSystem != null)
                ClientSystem.clientSystem.SendToServer(this.name + "~" + currentHitProjectile.projectile_Damage.ToString(), ClientSystem.EchoType.ATTACK, true);
        }
    }

    private void FindClosestPlayer()
    {
        //���忡�� ���� ����� �÷��̾ Ž��
        float minDistance = float.MaxValue;

        foreach(Transform t in GameSystem.gameSystem.players)
        {
            //�÷��̾ ����ִ� ��쿡�� transform�� �����´�
            if (!t.GetComponent<Character>().isDead)
            {
                //�÷��̾ �ּ� 1���̶� ����ִ� ��� true ó��
                hasTarget = true;

                //�÷��̾��� position�� ��ȸ�ϸ鼭 ���� ����� ���� �ʱ�ȭ
                float distance = Vector2.Distance(transform.position, t.position);                
                if (minDistance > distance)
                {
                    minDistance = distance;
                    target = t.position;
                }
            }
        }

        //���忡 ����ִ� �÷��̾ ���� �� ������ �������� ���� ���� false ó��
        if(minDistance == float.MaxValue)        
            hasTarget = false;     
    }

    private IEnumerator Paralyze()
    {
        isParalyzed = true;
        material.SetFloat("_GrayAmount", 1f);

        yield return new WaitForSeconds(0.5f);

        isParalyzed = false;
        material.SetFloat("_GrayAmount", 0f);
    }
}
