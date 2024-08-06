using UnityEngine;

public class SpawnPhase : MonoBehaviour
{
    [SerializeField] private float _time;             //���� ���� �ð�
    [SerializeField] private string _type;            //������ ���� ������ �̸�
    [SerializeField] private int _number;             //������ ����
    [SerializeField] private float _interval;         //���� ������ ���� (�ѹ��� ���� ������ ������ ����)

    public float time { get { return _time; } }
    public string type { get { return _type; } }
    public int number { get { return _number; } }
    public float interval { get { return _interval; } }
}
