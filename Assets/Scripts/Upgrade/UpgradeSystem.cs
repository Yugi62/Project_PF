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

    //업그레이드 카드 3장 뽑기
    public void Draw()
    {
        //업그레이드 카드 셔플 후 UI에 적용
        Shuffle();
        image_00.sprite = upgrade_List[0].icon;
        image_01.sprite = upgrade_List[1].icon;
        image_02.sprite = upgrade_List[2].icon;

        //UI 활성화
        upgrade_UI.SetActive(true);
    }

    //0번 카드 선택 (UI 기준 왼쪽)
    public void Select00()
    {
        upgrade_List[0].Activate();
        upgrade_UI.SetActive(false);
    }

    //1번 카드 선택 (UI 기준 중앙)
    public void Select01()
    {
        upgrade_List[1].Activate();
        upgrade_UI.SetActive(false);
    }

    //2번 카드 선택 (UI 기준 오른쪽)
    public void Select02()
    {
        upgrade_List[2].Activate();
        upgrade_UI.SetActive(false);
    }
}
