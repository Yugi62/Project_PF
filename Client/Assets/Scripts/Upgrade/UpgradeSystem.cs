using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSystem : MonoBehaviour
{
    [SerializeField] private GameObject upgrade_UI;
    [SerializeField] private Image[] image;
    [SerializeField] private TMP_Text[] text;
    
    private List<Upgrade> upgrade_List;
    private int[] selectedIndex = new int[3];

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

    //���ʷ� ī�带 ���� ���� �нú�� ���� (��ų ī�常 ������ ����)
    public void FirstDraw()
    {
        Shuffle();

        int i = 0;
        int cnt = 0;

        while(cnt < 3)
        {
            if (!upgrade_List[i].isPasive)
            {
                image[cnt].sprite = upgrade_List[i].icon;
                text[cnt].text = upgrade_List[i].description;

                selectedIndex[cnt] = i;
                cnt++;

            }
            i++;
        }

        //UI Ȱ��ȭ
        upgrade_UI.SetActive(true);
    }

    //���׷��̵� ī�� 3�� �̱�
    public void Draw()
    {
        //���׷��̵� ī�� ���� �� UI�� ����
        Shuffle();

        for(int i = 0; i < 3; i++)
        {
            image[i].sprite = upgrade_List[i].icon;
            text[i].text = upgrade_List[i].description;
            selectedIndex[i] = i;
        }

        //UI Ȱ��ȭ
        upgrade_UI.SetActive(true);
    }

    //0�� ī�� ���� (UI ���� ����)
    public void Select00()
    {
        upgrade_List[selectedIndex[0]].Activate();
        upgrade_UI.SetActive(false);
    }

    //1�� ī�� ���� (UI ���� �߾�)
    public void Select01()
    {
        upgrade_List[selectedIndex[1]].Activate();
        upgrade_UI.SetActive(false);
    }

    //2�� ī�� ���� (UI ���� ������)
    public void Select02()
    {
        upgrade_List[selectedIndex[2]].Activate();
        upgrade_UI.SetActive(false);
    }
}
