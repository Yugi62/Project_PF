using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    GameObject[] characters;        //ĳ���͵��� �̹����� ������ ���� ������Ʈ
    int currentIndex = 0;           //���� �ε���

    private void Awake()
    {
        characters = new GameObject[transform.childCount];

        for(int i =0; i < characters.Length; i++)        
            characters[i] = transform.GetChild(i).gameObject;        
    }

    public void LeftButton()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = characters.Length - 1;

        for(int i =  0; i < characters.Length; i++)
            characters[i].SetActive(false);

        characters[currentIndex].SetActive(true);
    }

    public void RightButton()
    {
        currentIndex++;

        if (currentIndex > characters.Length - 1)
            currentIndex = 0;

        for (int i = 0; i < characters.Length; i++)
            characters[i].SetActive(false);

        characters[currentIndex].SetActive(true);
    }

    public void SelectButton()
    {
        ClientSystem.clientSystem.playerSprite = characters[currentIndex].GetComponent<Image>().sprite;
        ClientSystem.clientSystem.prefabName = "Nonplayer_" + currentIndex.ToString();
    }
}
