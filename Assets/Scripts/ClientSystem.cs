using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSystem : MonoBehaviour
{
    //클라이언트 시스템 싱글톤 구현
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


    private const int BUF_SIZE = 1024;                          //버퍼 사이즈
    private const float MAX_CONNECT_WAIT_TIME = 5;              //서버 연결 시 최대 대기 시간 (시간 오버 시 연결 실패)

    [SerializeField] private TMP_InputField IP_InputField;
    [SerializeField] private TMP_InputField Port_InputField;
    [SerializeField] private TMP_InputField Name_InputField;

    [SerializeField] private TMP_Text info_Text;                //연결 실패 원인 출력용 UI
    [SerializeField] private TMP_Text chat_Text;
    
    private Socket clntSock;                                    //소켓
    private EndPoint servEP;                                    //서버의 endpoint (=주소)
    private byte[] buffer = new byte[BUF_SIZE];                 //버퍼
    private bool isConnecting = false;                          //서버와 연결중인지
    private bool isConnected = false;                           //서버와 연결되어 있는 유무

    private Thread receiveThread;

    private bool isReceving = false;

    private float timerTime = 0f;
    public string playerName {  get; private set; }


    private void Awake()
    {
        //클라이언트 시스템 싱글톤
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
        //소켓 생성
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

                //1. 서버에게 패킷을 전송하여 연결을 시작하겠다고 알림
                clntSock.SendToAsync(sendEventArgs);

                isReceving = true;
                StartReceive();

                //2. 타이머 설정 후 오버하는 경우 연결 실패 처리
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

            //소켓 버퍼에 패킷이 있는 경우 비동기 수신 시작
            if (clntSock.Available > 0)            
                clntSock.ReceiveFromAsync(receiveEventArgs);
        }
    }
    private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        string buffer = Encoding.UTF8.GetString(e.Buffer);
        string packetBuffer = FixBuffer(buffer);
        PacketType packetType = GetPacketType(buffer);

        //서버와 연결되어 있지 않은 경우
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
        //서버와 연결된 경우
        else if(isConnected)
        {
            switch (packetType)
            {
                //Ping을 받은 경우 ping에 1을 더한 후 서버에게 전송
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
        SendToServer : 서버에게 데이터 전송 (본인을 제외한 모든 클라이언트에게 ECHO 된다)

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
        FixBuffer : buffer에서 사용되는 부분만 잘라서 반환 (PacketType 부분도 같이 자른다)

        */

        buffer = buffer.Substring(0, buffer.IndexOf('\0'));
        buffer = buffer.Substring(2);
        return buffer;
    }

    private PacketType GetPacketType(string buffer)
    {
        /*
        GetPacketType : 서버로부터 받은 버퍼의 앞 두 글자(패킷의 용도)를 받아 PacketType으로 반환 

        */

        buffer = buffer.Substring(0, 2);
        return (PacketType)System.Convert.ToInt32(buffer);
    }

    private string SetPacketType(string original, PacketType type)
    {
        /*
        SetPacketType : 패킷 앞에 어떤 용도로 사용되었는지 16진수로 명시하는 작업을 실시 (용도의 종류는 PacketType 참조)
         1. type를 16진수 변환 후 문자열로 typeString에 저장
         2. typeString + 원본 데이터 순서로 문자열 재구축 후 반환 (Ex. 011452 = 핑으로 사용되는 네자리 난수)
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
