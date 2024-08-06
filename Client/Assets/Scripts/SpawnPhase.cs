using UnityEngine;

public class SpawnPhase : MonoBehaviour
{
    [SerializeField] private float _time;             //스폰 시작 시간
    [SerializeField] private string _type;            //스폰할 몬스터 종류의 이름
    [SerializeField] private int _number;             //스폰할 개수
    [SerializeField] private float _interval;         //스폰 사이의 간격 (한번의 여러 마리의 스폰을 금지)

    public float time { get { return _time; } }
    public string type { get { return _type; } }
    public int number { get { return _number; } }
    public float interval { get { return _interval; } }
}
