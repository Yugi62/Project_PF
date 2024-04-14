using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSystem : MonoBehaviour
{
    //Ŭ���̾�Ʈ �ý��� �̱��� ����
    public static ClientSystem clientSystem;

    public enum PacketType
    {
        ACCEPT = 1,
        PING = 2,
        ECHO = 3
    }

    public enum GameObjectType
    {
        MESSAGE = 1
    }


    private const int BUF_SIZE = 1024;                          //���� ������
    private const float MAX_CONNECT_WAIT_TIME = 5;              //���� ���� �� �ִ� ��� �ð� (�ð� ���� �� ���� ����)

    [SerializeField] private TMP_InputField IP_InputField;
    [SerializeField] private TMP_InputField Port_InputField;
    [SerializeField] private TMP_InputField Name_InputField;

    [SerializeField] private TMP_Text info_Text;                //���� ���� ���� ��¿� UI
    [SerializeField] private TMP_Text chat_Text;
    
    private Socket clntSock;                                    //����
    private EndPoint servEP;                                    //������ endpoint (=�ּ�)
    private byte[] buffer = new byte[BUF_SIZE];                 //����
    private bool isConnecting = false;                          //������ ����������
    private bool isConnected = false;                           //������ ����Ǿ� �ִ� ����

    private Thread receiveThread;

    private bool isReceving = false;

    private float timerTime = 0f;
    public string playerName {  get; private set; }


    private void Awake()
    {
        //Ŭ���̾�Ʈ �ý��� �̱���
        if(clientSystem == null)
        {
            clientSystem = this;
            DontDestroyOnLoad(clientSystem);
        }
        else
            Destroy(clientSystem);
    }
    private void Start()
    {
        //���� ����
        clntSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }
    private void Update()
    {
        if(isConnecting)
        {
            timerTime += Time.deltaTime;
            if(timerTime >= MAX_CONNECT_WAIT_TIME)
            {
                StopReceive();


                timerTime = 0f;
                isConnecting = false;
                info_Text.text = "Failed to connect to server";
            }
        }

        if(isConnected)
        {
            if(SceneManager.GetActiveScene().buildIndex == 0)
            {
                SceneManager.LoadScene(1);
            }
        }
    }

    private void StartReceive()
    {
        if(receiveThread == null)
        {
            receiveThread = new Thread(ReceiveFromServer);
            receiveThread.Start();
        }
    }

    private void StopReceive()
    {
        isReceving = false;
        receiveThread.Join();
        receiveThread = null;
    }

    public void ConnectToServer()
    {
        info_Text.text = "Connecting...";

        if (!isConnecting)
        {
            if (IP_InputField.text != string.Empty && Port_InputField.text != string.Empty && Name_InputField.text != string.Empty)
            {
                servEP = new IPEndPoint(IPAddress.Parse(IP_InputField.text), int.Parse(Port_InputField.text));
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.Completed += SendCompleted;
                sendEventArgs.RemoteEndPoint = servEP;

                byte[] bufferToSend = Encoding.UTF8.GetBytes(SetPacketType(Name_InputField.text, PacketType.ACCEPT));
                sendEventArgs.SetBuffer(bufferToSend, 0, bufferToSend.Length);

                //1. �������� ��Ŷ�� �����Ͽ� ������ �����ϰڴٰ� �˸�
                clntSock.SendToAsync(sendEventArgs);

                isReceving = true;
                StartReceive();

                //2. Ÿ�̸� ���� �� �����ϴ� ��� ���� ���� ó��
                isConnecting = true;
            }

            else            
                info_Text.text = "Invalid value";            
        }
    }
    private void ReceiveFromServer()
    {
        while (isReceving)
        {
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
            receiveEventArgs.Completed += ReceiveCompleted;
            receiveEventArgs.RemoteEndPoint = remoteEP;
            receiveEventArgs.SetBuffer(buffer, 0, BUF_SIZE);

            //���� ���ۿ� ��Ŷ�� �ִ� ��� �񵿱� ���� ����
            if (clntSock.Available > 0)            
                clntSock.ReceiveFromAsync(receiveEventArgs);
        }
    }
    private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        string buffer = Encoding.UTF8.GetString(e.Buffer);
        string packetBuffer = FixBuffer(buffer);
        PacketType packetType = GetPacketType(buffer);

        //������ ����Ǿ� ���� ���� ���
        if (isConnecting && !isConnected)
        {
            if(packetType == PacketType.ACCEPT && packetBuffer == Name_InputField.text)
            {
                playerName = Name_InputField.text;

                isConnecting = false;
                timerTime = 0f;

                isConnected = true;
            }
        }
        //������ ����� ���
        else if(isConnected)
        {
            switch (packetType)
            {
                //Ping�� ���� ��� ping�� 1�� ���� �� �������� ����
                case PacketType.PING:
                    int currentPing = int.Parse(packetBuffer);
                    currentPing++;
                    SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                    sendEventArgs.Completed += SendCompleted;
                    sendEventArgs.RemoteEndPoint = servEP;
                    byte[] bufferToSend = Encoding.UTF8.GetBytes(SetPacketType(currentPing.ToString(), PacketType.PING));
                    sendEventArgs.SetBuffer(bufferToSend, 0, bufferToSend.Length);
                    clntSock.SendToAsync(sendEventArgs);
                    break;

                case PacketType.ECHO:
                    packetBuffer = packetBuffer.Substring(2);
                    chat_Text.text += packetBuffer;
                    break;
            }
        }
    }
    public void SendToServer(string str, GameObjectType gameObjectType)
    {
        /*
        SendToServer : �������� ������ ���� (������ ������ ��� Ŭ���̾�Ʈ���� ECHO �ȴ�)

        */
        if (isConnected)
        {
            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.Completed += SendCompleted;
            sendEventArgs.RemoteEndPoint = servEP;

            string fixed_gameObjectType = ((int)gameObjectType).ToString();
            fixed_gameObjectType = fixed_gameObjectType.PadLeft(2, '0');
            str = fixed_gameObjectType + str;

            byte[] bufferToSend = Encoding.UTF8.GetBytes(SetPacketType(str, PacketType.ECHO));
            sendEventArgs.SetBuffer(bufferToSend, 0, bufferToSend.Length);

            clntSock.SendToAsync(sendEventArgs);
        }
    }
    private void SendCompleted(object sender, SocketAsyncEventArgs e)
    {

    }
    private string FixBuffer(string buffer)
    {
        /*
        FixBuffer : buffer���� ���Ǵ� �κи� �߶� ��ȯ (PacketType �κе� ���� �ڸ���)

        */

        buffer = buffer.Substring(0, buffer.IndexOf('\0'));
        buffer = buffer.Substring(2);
        return buffer;
    }

    private PacketType GetPacketType(string buffer)
    {
        /*
        GetPacketType : �����κ��� ���� ������ �� �� ����(��Ŷ�� �뵵)�� �޾� PacketType���� ��ȯ 

        */

        buffer = buffer.Substring(0, 2);
        return (PacketType)System.Convert.ToInt32(buffer);
    }

    private string SetPacketType(string original, PacketType type)
    {
        /*
        SetPacketType : ��Ŷ �տ� � �뵵�� ���Ǿ����� 16������ ����ϴ� �۾��� �ǽ� (�뵵�� ������ PacketType ����)
         1. type�� 16���� ��ȯ �� ���ڿ��� typeString�� ����
         2. typeString + ���� ������ ������ ���ڿ� �籸�� �� ��ȯ (Ex. 011452 = ������ ���Ǵ� ���ڸ� ����)
        */

        string typeString = System.Convert.ToString((int)type, 16);
        typeString = typeString.PadLeft(2, '0');
        original = typeString + original;
        return original;
    }


    private void OnApplicationQuit()
    {
        if(receiveThread != null)
        {
            StopReceive();
        }
    }
}
