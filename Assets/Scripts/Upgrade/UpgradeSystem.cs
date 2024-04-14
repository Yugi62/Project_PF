using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSystem : MonoBehaviour
{
    [SerializeField] private GameObject upgrade_UI;
    [SerializeField] private Image image_00;
    [SerializeField] private Image image_01;
    [SerializeField] private Image image_02;
    
    private List<Upgrade> upgrade_List;

    private void Awake()
    {
        //�ڽĵ��� Upgrade ������Ʈ�� �޾� ����Ʈ�� ����
        upgrade_List = new List<Upgrade>();
        Component[] components = gameObject.GetComponentsInChildren<Component>();
        foreach (Component component in components)
        {
            if(component is Upgrade)            
               upgrade_List.Add(component as Upgrade);            
        }
    }

    //���׷��̵� ī�� ����
    private void Shuffle()
    {
        System.Random rand = new System.Random();

        for(int i = 0; i <upgrade_List.Count; i++)
        {
            int randIndex = rand.Next(i + 1);            
            Upgrade temp = upgrade_List[randIndex];
            upgrade_List[randIndex] = upgrade_List[i];
            upgrade_List[i] = temp;
        }
        
    }

    //���׷��̵� ī�� 3�� �̱�
    public void Draw()
    {
        //���׷��̵� ī�� ���� �� UI�� ����
        Shuffle();
        image_00.sprite = upgrade_List[0].icon;
        image_01.sprite = upgrade_List[1].icon;
        image_02.sprite = upgrade_List[2].icon;

        //UI Ȱ��ȭ
        upgrade_UI.SetActive(true);
    }

    //0�� ī�� ���� (UI ���� ����)
    public void Select00()
    {
        upgrade_List[0].Activate();
        upgrade_UI.SetActive(false);
    }

    //1�� ī�� ���� (UI ���� �߾�)
    public void Select01()
    {
        upgrade_List[1].Activate();
        upgrade_UI.SetActive(false);
    }

    //2�� ī�� ���� (UI ���� ������)
    public void Select02()
    {
        upgrade_List[2].Activate();
        upgrade_UI.SetActive(false);
    }
}
