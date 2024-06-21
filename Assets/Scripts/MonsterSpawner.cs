using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    /*
    1. 호스트인 경우에만 스폰
    2. 몬스터 이름이 절대 겹치지 않게 규칙을 생성
    */

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Timer timer;

    private SpawnPhase[] spawnPhases;
    private int currentPhase = 0;

    private GameObject ork;
    private GameObject ghost;

    private int monsterCount = 0;

    private void Awake()
    {
        ork = Resources.Load<GameObject>("Ork");
        ghost = Resources.Load<GameObject>("Ghost");

        spawnPhases = new SpawnPhase[transform.childCount];
        for (int i = 0; i < spawnPhases.Length; i++)
            spawnPhases[i] = transform.GetChild(i).gameObject.GetComponent<SpawnPhase>();
    }

    private int invokeCnt = 0;
    private bool isInvoking = false;

    private void FixedUpdate()
    {
        if (spawnPhases.Length > currentPhase)
        {
            if (timer.GetTimerTime() <= spawnPhases[currentPhase].time && !isInvoking)
            {
                InvokeRepeating("SpawnMonster", 0f, spawnPhases[currentPhase].interval);
                isInvoking = true;
            }
        }
    }

    private void SpawnMonster()
    {
        invokeCnt++;

        SpawnPhase spawnPhase = spawnPhases[currentPhase];

        if (invokeCnt > spawnPhase.number)
        {
            CancelInvoke("SpawnMonster");
            currentPhase++;
            invokeCnt = 0;
            isInvoking = false;

            return;
        }

        GameObject spawnType = StrToType(spawnPhase.type);
        GameObject newObject = Instantiate(spawnType, spawnPoint.position, transform.rotation);
        newObject.name = "Monster_" + monsterCount.ToString();
        monsterCount++;

        if (monsterCount >= int.MaxValue)
            monsterCount = 0;
    }

    private GameObject StrToType(string type)
    {
        switch(type)
        {
            case "Ork":
                return ork;

            case "Ghost":
                return ghost;

            default:
                return null;
        }
    }
}
