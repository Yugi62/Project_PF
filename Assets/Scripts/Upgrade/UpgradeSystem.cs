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
        //자식들의 Upgrade 컴포넌트를 받아 리스트에 저장
        upgrade_List = new List<Upgrade>();
        Component[] components = gameObject.GetComponentsInChildren<Component>();
        foreach (Component component in components)
        {
            if(component is Upgrade)            
               upgrade_List.Add(component as Upgrade);            
        }
    }

    //업그레이드 카드 셔플
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

    //최초로 카드를 뽑을 때는 패시브는 제외 (스킬 카드만 나오게 설정)
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

        //UI 활성화
        upgrade_UI.SetActive(true);
    }

    //업그레이드 카드 3장 뽑기
    public void Draw()
    {
        //업그레이드 카드 셔플 후 UI에 적용
        Shuffle();

        for(int i = 0; i < 3; i++)
        {
            image[i].sprite = upgrade_List[i].icon;
            text[i].text = upgrade_List[i].description;
            selectedIndex[i] = i;
        }

        //UI 활성화
        upgrade_UI.SetActive(true);
    }

    //0번 카드 선택 (UI 기준 왼쪽)
    public void Select00()
    {
        upgrade_List[selectedIndex[0]].Activate();
        upgrade_UI.SetActive(false);
    }

    //1번 카드 선택 (UI 기준 중앙)
    public void Select01()
    {
        upgrade_List[selectedIndex[1]].Activate();
        upgrade_UI.SetActive(false);
    }

    //2번 카드 선택 (UI 기준 오른쪽)
    public void Select02()
    {
        upgrade_List[selectedIndex[2]].Activate();
        upgrade_UI.SetActive(false);
    }
}
