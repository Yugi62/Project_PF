using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSystem : MonoBehaviour
{
    public enum PacketType
    {
        ACCEPT = 1,                                             //접속 시도
        INIT = 2,                                               //최초 접속 후 초기화
        PING = 3,                                               //서버와 연결이 되었는지 지속적으로 확인
        ECHO = 4,                                               //자신을 제외한 다른 클라이언트에게 전송
        CRIT = 5,                                               //에코와 동일하지만 누락 시 재전송을 보장
        HOST = 6                                                //호스트 설정 혹은 변경
    }
    public enum EchoType
    {
        START = 0,                                              //게임 시작
        MESSAGE = 1,                                            //메시지        (Ex. 채팅)
        MOVE = 2,                                               //이동 관련     (Ex. 플레이어 이동, 몬스터 위치 동기화)
        ATTACK = 3,                                             //피격 관련     (Ex. HP 동기화, 사망 처리)
        SPAWN = 4,                                              //생성 관련     (Ex. 플레이어 생성, 몬스터 생성)
        PROJECTILE = 5,                                         //투사체 관련   (Ex. 투사체 생성)
        DISCONNECT = 6                                          //연결 해제
    }


    //클라이언트 시스템 싱글톤
    public static ClientSystem clientSystem;

    private const int BUF_SIZE = 1024;                          //버퍼 사이즈
    private const float MAX_CONNECT_WAIT_TIME = 5;              //서버 연결 시 최대 대기 시간 (시간 오버 시 연결 실패)

    [SerializeField] private TMP_InputField IP_InputField;      //
    [SerializeField] private TMP_InputField Port_InputField;    //
    [SerializeField] private TMP_InputField Name_InputField;    //
    [SerializeField] private TMP_Text error_Text;               //연결 실패 원인 출력용 UI
    public TMP_Text chat_Text;
    
    private Socket clntSock;                                    //소켓
    private EndPoint servEP;                                    //서버의 endpoint (=주소)
    private byte[] buffer = new byte[BUF_SIZE];                 //버퍼

    private bool isConnecting = false;                          //서버와 연결중인 유무
    public bool isConnected { get; private set; }               //서버와 연결된 유무

    [SerializeField] private bool _isHost;
    public bool isHost { get { return _isHost; } }              //호스트인 유무

    private Thread receiveThread;                               //수신 스레드
    private bool isReceving = false;                            //수신 스레드의 반복문 제어용 bool

    private int critCnt = -1;

    private float timerTime = 0f;                               //타이머용 변수
    public string playerName {  get; private set; }             //플레이어의 이름
    public Sprite playerSprite;                                 //플레이어의 sprite
    public string prefabName;                                   //플레이어의 sprite 정보만 담은 prefab의 이름

    private Vector2 playerPosition;                             //현 플레이어의 위치
    private float playerDirection;                              //현 플레이어의 방향


    private Queue<string> spawnQueue = new Queue<string>();
    private Queue<string> moveQueue = new Queue<string>();
    private Queue<string> attackQueue = new Queue<string>();
    private Queue<string> projectileQueue = new Queue<string>();
    private Queue<string> disconnectQueue = new Queue<string>();

    private void Awake()
    {
        //클라이언트 시스템 싱글톤
        if (clientSystem == null)
        {
            clientSystem = this;
            DontDestroyOnLoad(clientSystem);
        }
        else
            Destroy(clientSystem);

        //IOCP 최소 스레드 수 설정
        ThreadPool.SetMinThreads(24, 24);

        //소켓 생성 (UDP)
        clntSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //프로퍼티 초기화
        isConnected = false;
        _isHost = false;
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
                error_Text.text = "Failed to connect to server";
            }
        }

        if(isConnected)
        {
            if(SceneManager.GetActiveScene().buildIndex == 0)            
                SceneManager.LoadScene(1);            

            //플레이어의 위치가 바뀔 때마다 실시간으로 서버에게 전송
            if (Player.player != null)
            {
                if (playerPosition != (Vector2)Player.player.transform.position)
                {
                    playerPosition = Player.player.transform.position;
                    playerDirection = Player.player.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x;
                    SendToServer(playerName + "~" + playerPosition.x.ToString("F2") + "~" + playerPosition.y.ToString("F2") + "~" + playerDirection.ToString(), EchoType.MOVE, false);
                }
            } 

            //GameScene일 때 서버로부터 받은 패킷 처리
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {   
                //생성 큐 (이름~프리팹명~position.x~position.y~방향)
                if (spawnQueue.Count != 0)
                {
                    string buffer = spawnQueue.Peek();
                    spawnQueue.Dequeue();
                    string[] splitedBuffer = buffer.Split("~");

                    GameObject gameObject = Resources.Load<GameObject>(splitedBuffer[1]);

                    GameObject newObject = Instantiate(gameObject);
                    newObject.name = splitedBuffer[0];
                    newObject.transform.position = new Vector2(float.Parse(splitedBuffer[2]), float.Parse(splitedBuffer[3]));

                    //방향 초기화 제대로 안되는 것
                    //newObject.transform.localScale = new Vector3(float.Parse(splitedBuffer[4]), 1, 1);

                    //생성한 게임 오브젝트가 플레이어인 경우
                    if (newObject.CompareTag("Player"))
                    {
                        //플레이어 목록에 추가
                        GameSystem.gameSystem.players.Add(newObject.transform);
                        //인게임 UI의 텍스트를 이름으로 초기화
                        newObject.GetComponentInChildren<TMP_Text>().text = newObject.name;
                    }
                }

                //이동 큐 (이름~position.x~position.y~방향)
                if (moveQueue.Count != 0)
                {
                    string buffer = moveQueue.Peek();
                    moveQueue.Dequeue();
                    string[] splitedBuffer = buffer.Split("~");

                    GameObject gameObject = GameObject.Find(splitedBuffer[0]);

                    if(gameObject != null)
                    {
                        gameObject.transform.position = new Vector2(float.Parse(splitedBuffer[1]), float.Parse(splitedBuffer[2]));
                        gameObject.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale = new Vector3(float.Parse(splitedBuffer[3]), 1, 1);

                        Monster monster = gameObject.GetComponent<Monster>();
                        if (monster != null)
                            monster.CreatePath();
                    }
                }

                //공격 큐 (이름~데미지)
                if(attackQueue.Count != 0)
                {
                    string buffer = attackQueue.Peek();
                    attackQueue.Dequeue();
                    string[] splitedBuffer = buffer.Split("~");

                    GameObject gameObject = GameObject.Find(splitedBuffer[0]);

                    if (gameObject != null)
                        gameObject.GetComponent<Character>().current_Health_Point -= float.Parse(splitedBuffer[1]);
                }

                //투사체 큐 (프리팹~target.x~taget.y~shooter.x~shooter.y)
                if(projectileQueue.Count != 0)
                {
                    string buffer = projectileQueue.Peek();
                    projectileQueue.Dequeue();
                    string[] splitedBuffer = buffer.Split("~");

                    Vector2 target = new Vector2(float.Parse(splitedBuffer[1]), float.Parse(splitedBuffer[2]));
                    Vector2 shooter = new Vector2(float.Parse(splitedBuffer[3]), float.Parse(splitedBuffer[4]));
                    int direction = int.Parse(splitedBuffer[5]);

                    GameObject newObject = Instantiate(Resources.Load<GameObject>(splitedBuffer[0]));
                    newObject.GetComponent<Projectile>().Shoot(target, shooter, direction);
                }

                //연결해제 큐 (이름)
                if(disconnectQueue.Count != 0)
                {
                    string buffer = disconnectQueue.Peek();
                    disconnectQueue.Dequeue();
                    string[] splitedBuffer = buffer.Split("~");

                    GameObject disconnectedPlayer = GameObject.Find(splitedBuffer[0]);

                    //인게임에 존재하는 플레이어 제거
                    if (disconnectedPlayer != null)
                        Destroy(disconnectedPlayer);

                    //플레이어 목록을 순회하면서 동일한 이름인 경우의 제거
                    for (int i = 0; i < GameSystem.gameSystem.players.Count; i++)
                    {
                        if (GameSystem.gameSystem.players[i].name == splitedBuffer[0])
                            GameSystem.gameSystem.players.RemoveAt(i);
                    }
                }

            }
        }
    }

    private void StartReceive()
    {
        /*
        StartReceive : receiveThread 시작 (스레드 수는 임시적으로 1개로 제한)
        */

        if(receiveThread == null)
        {
            receiveThread = new Thread(ReceiveFromServer);
            receiveThread.Start();
        }
    }
    public void StopReceive()
    {
        /*
        StopReceive : receiveThread 종료 (Join으로 종료하므로 게임이 멈출 가능성 존재)
        */

        if (isReceving)
        {
            isReceving = false;
            receiveThread.Join();
            receiveThread = null;
        }
    }
    public void ConnectToServer()
    {
        error_Text.text = "Connecting...";

        if (!isConnecting)
        {
            if (IP_InputField.text != string.Empty && Port_InputField.text != string.Empty && Name_InputField.text != string.Empty)
            {
                servEP = new IPEndPoint(IPAddress.Parse(IP_InputField.text), int.Parse(Port_InputField.text));
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.RemoteEndPoint = servEP;

                //일단 모양새는 안좋지만 임시로
                string temp = "0" + Name_InputField.text;

                byte[] bufferToSend = Encoding.UTF8.GetBytes(SetPacketType(temp, PacketType.ACCEPT));
                sendEventArgs.SetBuffer(bufferToSend, 0, bufferToSend.Length);

                //1. 서버에게 패킷을 전송하여 연결을 시작하겠다고 알림
                clntSock.SendToAsync(sendEventArgs);

                isReceving = true;
                StartReceive();

                //2. 타이머 설정 후 오버하는 경우 연결 실패 처리
                isConnecting = true;
            }

            else            
                error_Text.text = "Invalid value";            
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
            if(packetType == PacketType.ACCEPT)
            {
                //앞 한 자리와 뒤에 다섯 자리는 서버가 패킷을 구분하기 위해 있는 것이므로 사용하지 않는다
                string name = packetBuffer.Substring(1);
                name = name.Substring(0, name.Length - 5);

                if (name == Name_InputField.text)
                {
                    playerName = Name_InputField.text;

                    isConnecting = false;
                    timerTime = 0f;
                    isConnected = true;

                    //서버에게 접속이 완료되었음을 알림
                    SendToServer("1" + packetBuffer.Substring(1), PacketType.ACCEPT);
                    //서버에게 정보 요청 (최초 접속 시)
                    SendToServer("", PacketType.INIT);
                    //다른 클라이언트에게 현 플레이어를 생성하라고 에코
                    SendToServer(playerName + "~" + prefabName + "~" + playerPosition.x.ToString("F2") + "~" + playerPosition.y.ToString("F2") + "~" + playerDirection.ToString(), EchoType.SPAWN, true);
                }
                else
                {
                    StopReceive();
                    timerTime = 0f;
                    isConnecting = false;
                    error_Text.text = "This player already exists";
                }
            }
        }





        //서버와 연결된 경우
        else if(isConnected)
        {
            switch (packetType)
            {
                case PacketType.HOST:
                    _isHost = true;
                    SendToServer(packetBuffer, PacketType.HOST);
                    break;





                //최초 접속 시 다른 플레이어의 정보를 서버로부터 요청하거나 전달
                case PacketType.INIT:

                    if (packetBuffer.Contains('~'))
                    {
                        spawnQueue.Enqueue(packetBuffer);

                        packetBuffer = packetBuffer + "~1";

                        SendToServer(packetBuffer, PacketType.INIT);
                    }
                    else
                    {
                        packetBuffer += "~" + playerName + "~" + prefabName + "~" + playerPosition.x.ToString("F2") + "~" + playerPosition.y.ToString("F2") + "~" + playerDirection.ToString() + "~0";
                        SendToServer(packetBuffer, PacketType.INIT);
                        Debug.Log("1");
                    }
                    break;





                //Ping을 받은 경우 ping에 1을 더한 후 서버에게 전송
                case PacketType.PING:
                    int currentPing = int.Parse(packetBuffer);
                    currentPing++;
                    SendToServer(currentPing.ToString(), PacketType.PING); 
                    break;





                //다른 클라이언트에게 받은 패킷 적용
                case PacketType.ECHO:                    
                    EchoType echoType = (EchoType)int.Parse(packetBuffer.Substring(0, 2));
                    packetBuffer = packetBuffer.Substring(2);

                    //EchoType에 따라 작업을 분리
                    switch (echoType)
                    {                  
                        //이거 UI 제대로 작동되게 수정해 이동 다 만들고 (UI가 안내려감)
                        case EchoType.MESSAGE:                            
                            chat_Text.text += packetBuffer;
                            break;

                        case EchoType.MOVE:       
                            moveQueue.Enqueue(packetBuffer);
                            break;

                        case EchoType.PROJECTILE:
                            projectileQueue.Enqueue(packetBuffer);
                            break;

                        default:                            
                            break;
                    }

                    break;





                //다른 클라이언트로부터 받은 패킷 적용 + 서버에게 전송하여 받았음을 알림
                case PacketType.CRIT:

                    EchoType critType = (EchoType)int.Parse(packetBuffer.Substring(0, 2));
                    string subBuffer = packetBuffer.Substring(2);

                    //고유번호로 중복적용 방지
                    int cnt = int.Parse(subBuffer.Substring(subBuffer.Length - 4, 4));
                    if (critCnt >= cnt)                    
                        break;                    
                    else
                        critCnt = cnt;

                    //EchoType에 따라 작업을 분리
                    switch (critType)
                    {
                        case EchoType.START:
                            GameSystem.gameSystem.isGameStarted = true;
                            break;

                        case EchoType.ATTACK:
                            attackQueue.Enqueue(subBuffer);
                            break;

                        case EchoType.SPAWN:
                            spawnQueue.Enqueue(subBuffer);
                            break;

                        case EchoType.DISCONNECT:
                            disconnectQueue.Enqueue(subBuffer);
                            break;

                        default:
                            break;
                    }

                    //1을 앞에 붙여 서버에게 제대로 받았음을 알림
                    packetBuffer = "1" + packetBuffer;
                    SendToServer(packetBuffer, PacketType.CRIT);
                    break;
            }
        }     
    }
    public void SendToServer(string str, EchoType echoType, bool isCrit)
    {
        /*
        SendToServer : 서버에게 데이터 전송

        */
        if (isConnected)
        {
            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.RemoteEndPoint = servEP;

            string fixed_gameObjectType = ((int)echoType).ToString();
            fixed_gameObjectType = fixed_gameObjectType.PadLeft(2, '0');
            str = fixed_gameObjectType + str;

            byte[] bufferToSend;

            //재전송이 필요한 패킷을 보내는 경우 "0"을 packetType과 str 사이에 삽입 ("0" : 재전송을 보장한 에코를 부탁, "1" : 서버에게 제대로 받았음을 알림) 
            if (isCrit)
                bufferToSend = Encoding.UTF8.GetBytes(SetPacketType(str, PacketType.CRIT).Insert(2, "0"));
            //재전송이 필요없는 경우
            else
                bufferToSend = Encoding.UTF8.GetBytes(SetPacketType(str, PacketType.ECHO));

            sendEventArgs.SetBuffer(bufferToSend, 0, bufferToSend.Length);

            clntSock.SendToAsync(sendEventArgs);
        }
    }
    public void SendToServer(string str, PacketType packetType)
    {
        if(isConnected)
        {
            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.RemoteEndPoint = servEP;
            byte[] bufferToSend = Encoding.UTF8.GetBytes(SetPacketType(str, packetType));
            sendEventArgs.SetBuffer(bufferToSend, 0, bufferToSend.Length);
            clntSock.SendToAsync(sendEventArgs);
        }
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
