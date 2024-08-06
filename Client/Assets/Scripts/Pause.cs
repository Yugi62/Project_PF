using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] GameObject menu;

    public void OnPauseButton()
    {
        menu.SetActive(true);
    }

    public void OnCheckButton()
    {
        if (ClientSystem.clientSystem != null)
        {
            ClientSystem.clientSystem.StopReceive();
            Destroy(ClientSystem.clientSystem.gameObject);
        }

        if (SceneManager.GetActiveScene().buildIndex == 1)
            SceneManager.LoadScene(0);
    }

    public void OnCrossButton()
    {
        menu.SetActive(false);
    }
}
