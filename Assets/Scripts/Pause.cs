using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    /*
    에디터 내에서 스레드가 정상적으로 종료되지 않아 임시로 만든 스크립트임
    */

    public void PauseExit()
    {
        ClientSystem.clientSystem.StopReceive();
        SceneManager.LoadScene(0);
    }
}
