using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    /*
    ������ ������ �����尡 ���������� ������� �ʾ� �ӽ÷� ���� ��ũ��Ʈ��
    */

    public void PauseExit()
    {
        ClientSystem.clientSystem.StopReceive();
        SceneManager.LoadScene(0);
    }
}
