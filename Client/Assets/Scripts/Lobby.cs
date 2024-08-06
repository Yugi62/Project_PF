using UnityEngine;

public class Lobby : MonoBehaviour
{
    private GameObject[] lobbies;

    private void Awake()
    {
        lobbies = new GameObject[transform.childCount];
        for(int i =0; i<lobbies.Length; i++)        
            lobbies[i] = transform.GetChild(i).gameObject;
    }

    public void OpenLobby(int index)
    {
        foreach (GameObject lobby in lobbies)        
            lobby.SetActive(false);

        lobbies[index].SetActive(true);
    }
}
