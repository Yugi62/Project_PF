using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Monster : Character
{
    [SerializeField] private float _drop_XP;        //드롭 경험치
    [SerializeField] private bool isAstar;          //Astar 방식으로 움직이기 (false인 경우 장애물을 무시하고 일직선으로 플레이어를 쫓아간다)
    [SerializeField] private float updatePathTime;  //동선 갱신 시간

    private Projectile currentHitProjectile;        //최근에 타격된 투사체    
    private Vector3 target;                         //타겟 (=몬스터가 따라가는 대상)
    private bool hasTarget = false;                 //타겟의 존재 유무 (=월드의 살아있는 플레이어의 존재 유무)

    private Astar astar;
    private List<Node> path;
    private int currentIndex;

    private Material material;
    private GameObject damagePrefab;


    private bool isParalyzed = false;    

    private void FixedUpdate()
    {
        //경직이 해제된 경우에만 움직인다
        if (!isParalyzed)
        {
            //타겟이 존재하면 path대로 움직인다
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

                //타겟과의 거리가 0.1 이하인 경우 목표에 도착한 것으로 간주한다
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
        //호스트인 경우 몬스터의 위치를 특정 시간마다 동기화
        if (ClientSystem.clientSystem != null && ClientSystem.clientSystem.isHost)
            InvokeRepeating("SyncPosition", 0f, 1.0f);

       if (updatePathTime > 0f)
            InvokeRepeating("CreatePath", 0f, updatePathTime);
    }

    public void CreatePath()
    {
        //가장 가까운 플레이어 탐색
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

            //0은 현 위치이므로 1로 설정
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
        //삭제 및 스킬 시전자의 경험치 증가
        Destroy(gameObject);

        //마무리를 한 사람이 현 플레이어인 경우에만 경험치 증가
        if (currentHitProjectile != null && currentHitProjectile.isPlayer)
            Player.player.current_XP += _drop_XP;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        //스킬에 맞은 경우 HP 감소
        if (LayerMask.NameToLayer("Skill") == collision.gameObject.layer)
        {
            StartCoroutine(Paralyze());

            currentHitProjectile = collision.gameObject.GetComponent<Projectile>();

            this.current_Health_Point -= currentHitProjectile.projectile_Damage;

            //데미지가 0인 경우 인게임에서 데미지를 표시하지 않음
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
        //월드에서 가장 가까운 플레이어를 탐색
        float minDistance = float.MaxValue;

        foreach(Transform t in GameSystem.gameSystem.players)
        {
            //플레이어가 살아있는 경우에만 transform을 가져온다
            if (!t.GetComponent<Character>().isDead)
            {
                //플레이어가 최소 1명이라도 살아있는 경우 true 처리
                hasTarget = true;

                //플레이어의 position을 순회하면서 가장 가까운 것을 초기화
                float distance = Vector2.Distance(transform.position, t.position);                
                if (minDistance > distance)
                {
                    minDistance = distance;
                    target = t.position;
                }
            }
        }

        //월드에 살아있는 플레이어가 없을 때 몬스터의 움직임을 막기 위해 false 처리
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
