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
        ACCEPT = 1,                                             //���� �õ�
        INIT = 2,                                               //���� ���� �� �ʱ�ȭ
        PING = 3,                                               //������ ������ �Ǿ����� ���������� Ȯ��
        ECHO = 4,                                               //�ڽ��� ������ �ٸ� Ŭ���̾�Ʈ���� ����
        CRIT = 5,                                               //���ڿ� ���������� ���� �� �������� ����
        HOST = 6                                                //ȣ��Ʈ ���� Ȥ�� ����
    }
    public enum EchoType
    {
        START = 0,                                              //���� ����
        MESSAGE = 1,                                            //�޽���        (Ex. ä��)
        MOVE = 2,                                               //�̵� ����     (Ex. �÷��̾� �̵�, ���� ��ġ ����ȭ)
        ATTACK = 3,                                             //�ǰ� ����     (Ex. HP ����ȭ, ��� ó��)
        SPAWN = 4,                                              //���� ����     (Ex. �÷��̾� ����, ���� ����)
        PROJECTILE = 5,                                         //����ü ����   (Ex. ����ü ����)
        DISCONNECT = 6                                          //���� ����
    }


    //Ŭ���̾�Ʈ �ý��� �̱���
    public static ClientSystem clientSystem;

    private const int BUF_SIZE = 1024;                          //���� ������
    private const float MAX_CONNECT_WAIT_TIME = 5;              //���� ���� �� �ִ� ��� �ð� (�ð� ���� �� ���� ����)

    [SerializeField] private TMP_InputField IP_InputField;      //
    [SerializeField] private TMP_InputField Port_InputField;    //
    [SerializeField] private TMP_InputField Name_InputField;    //
    [SerializeField] private TMP_Text error_Text;               //���� ���� ���� ��¿� UI
    public TMP_Text chat_Text;
    
    private Socket clntSock;                                    //����
    private EndPoint servEP;                                    //������ endpoint (=�ּ�)
    private byte[] buffer = new byte[BUF_SIZE];                 //����

    private bool isConnecting = false;                          //������ �������� ����
    public bool isConnected { get; private set; }               //������ ����� ����

    [SerializeField] private bool _isHost;
    public bool isHost { get { return _isHost; } }              //ȣ��Ʈ�� ����

    private Thread receiveThread;                               //���� ������
    private bool isReceving = false;                            //���� �������� �ݺ��� ����� bool

    private int critCnt = -1;

    private float timerTime = 0f;                               //Ÿ�̸ӿ� ����
    public string playerName {  get; private set; }             //�÷��̾��� �̸�
    public Sprite playerSprite;                                 //�÷��̾��� sprite
    public string prefabName;                                   //�÷��̾��� sprite ������ ���� prefab�� �̸�

    private Vector2 playerPosition;                             //�� �÷��̾��� ��ġ
    private float playerDirection;                              //�� �÷��̾��� ����


    private Queue<string> spawnQueue = new Queue<string>();
    private Queue<string> moveQueue = new Queue<string>();
    private Queue<string> attackQueue = new Queue<string>();
    private Queue<string> projectileQueue = new Queue<string>();
    private Queue<string> disconnectQueue = new Queue<string>();

    private void Awake()
    {
        //Ŭ���̾�Ʈ �ý��� �̱���
        if (clientSystem == null)
        {
            clientSystem = this;
            DontDestroyOnLoad(clientSystem);
        }
        else
            Destroy(clientSystem);

        //IOCP �ּ� ������ �� ����
        ThreadPool.SetMinThreads(24, 24);

        //���� ���� (UDP)
        clntSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //������Ƽ �ʱ�ȭ
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

            //�÷��̾��� ��ġ�� �ٲ� ������ �ǽð����� �������� ����
            if (Player.player != null)
            {
                if (playerPosition != (Vector2)Player.player.transform.position)
                {
                    playerPosition = Player.player.transform.position;
                    playerDirection = Player.player.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x;
                    SendToServer(playerName + "~" + playerPosition.x.ToString("F2") + "~" + playerPosition.y.ToString("F2") + "~" + playerDirection.ToString(), EchoType.MOVE, false);
                }
            } 

            //GameScene�� �� �����κ��� ���� ��Ŷ ó��
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {   
                //���� ť (�̸�~�����ո�~position.x~position.y~����)
                if (spawnQueue.Count != 0)
                {
                    string buffer = spawnQueue.Peek();
                    spawnQueue.Dequeue();
                    string[] splitedBuffer = buffer.Split("~");

                    GameObject gameObject = Resources.Load<GameObject>(splitedBuffer[1]);

                    GameObject newObject = Instantiate(gameObject);
                    newObject.name = splitedBuffer[0];
                    newObject.transform.position = new Vector2(float.Parse(splitedBuffer[2]), float.Parse(splitedBuffer[3]));

                    //���� �ʱ�ȭ ����� �ȵǴ� ��
                    //newObject.transform.localScale = new Vector3(float.Parse(splitedBuffer[4]), 1, 1);

                    //������ ���� ������Ʈ�� �÷��̾��� ���
                    if (newObject.CompareTag("Player"))
                    {
                        //�÷��̾� ��Ͽ� �߰�
                        GameSystem.gameSystem.players.Add(newObject.transform);
                        //�ΰ��� UI�� �ؽ�Ʈ�� �̸����� �ʱ�ȭ
                        newObject.GetComponentInChildren<TMP_Text>().text = newObject.name;
                    }
                }

                //�̵� ť (�̸�~position.x~position.y~����)
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

                //���� ť (�̸�~������)
                if(attackQueue.Count != 0)
                {
                    string buffer = attackQueue.Peek();
                    attackQueue.Dequeue();
                    string[] splitedBuffer = buffer.Split("~");

                    GameObject gameObject = GameObject.Find(splitedBuffer[0]);

                    if (gameObject != null)
                        gameObject.GetComponent<Character>().current_Health_Point -= float.Parse(splitedBuffer[1]);
                }

                //����ü ť (������~target.x~taget.y~shooter.x~shooter.y)
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

                //�������� ť (�̸�)
                if(disconnectQueue.Count != 0)
                {
                    string buffer = disconnectQueue.Peek();
                    disconnectQueue.Dequeue();
                    string[] splitedBuffer = buffer.Split("~");

                    GameObject disconnectedPlayer = GameObject.Find(splitedBuffer[0]);

                    //�ΰ��ӿ� �����ϴ� �÷��̾� ����
                    if (disconnectedPlayer != null)
                        Destroy(disconnectedPlayer);

                    //�÷��̾� ����� ��ȸ�ϸ鼭 ������ �̸��� ����� ����
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
        StartReceive : receiveThread ���� (������ ���� �ӽ������� 1���� ����)
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
        StopReceive : receiveThread ���� (Join���� �����ϹǷ� ������ ���� ���ɼ� ����)
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

                //�ϴ� ������ �������� �ӽ÷�
                string temp = "0" + Name_InputField.text;

                byte[] bufferToSend = Encoding.UTF8.GetBytes(SetPacketType(temp, PacketType.ACCEPT));
                sendEventArgs.SetBuffer(bufferToSend, 0, bufferToSend.Length);

                //1. �������� ��Ŷ�� �����Ͽ� ������ �����ϰڴٰ� �˸�
                clntSock.SendToAsync(sendEventArgs);

                isReceving = true;
                StartReceive();

                //2. Ÿ�̸� ���� �� �����ϴ� ��� ���� ���� ó��
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
            if(packetType == PacketType.ACCEPT)
            {
                //�� �� �ڸ��� �ڿ� �ټ� �ڸ��� ������ ��Ŷ�� �����ϱ� ���� �ִ� ���̹Ƿ� ������� �ʴ´�
                string name = packetBuffer.Substring(1);
                name = name.Substring(0, name.Length - 5);

                if (name == Name_InputField.text)
                {
                    playerName = Name_InputField.text;

                    isConnecting = false;
                    timerTime = 0f;
                    isConnected = true;

                    //�������� ������ �Ϸ�Ǿ����� �˸�
                    SendToServer("1" + packetBuffer.Substring(1), PacketType.ACCEPT);
                    //�������� ���� ��û (���� ���� ��)
                    SendToServer("", PacketType.INIT);
                    //�ٸ� Ŭ���̾�Ʈ���� �� �÷��̾ �����϶�� ����
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





        //������ ����� ���
        else if(isConnected)
        {
            switch (packetType)
            {
                case PacketType.HOST:
                    _isHost = true;
                    SendToServer(packetBuffer, PacketType.HOST);
                    break;





                //���� ���� �� �ٸ� �÷��̾��� ������ �����κ��� ��û�ϰų� ����
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





                //Ping�� ���� ��� ping�� 1�� ���� �� �������� ����
                case PacketType.PING:
                    int currentPing = int.Parse(packetBuffer);
                    currentPing++;
                    SendToServer(currentPing.ToString(), PacketType.PING); 
                    break;





                //�ٸ� Ŭ���̾�Ʈ���� ���� ��Ŷ ����
                case PacketType.ECHO:                    
                    EchoType echoType = (EchoType)int.Parse(packetBuffer.Substring(0, 2));
                    packetBuffer = packetBuffer.Substring(2);

                    //EchoType�� ���� �۾��� �и�
                    switch (echoType)
                    {                  
                        //�̰� UI ����� �۵��ǰ� ������ �̵� �� ����� (UI�� �ȳ�����)
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





                //�ٸ� Ŭ���̾�Ʈ�κ��� ���� ��Ŷ ���� + �������� �����Ͽ� �޾����� �˸�
                case PacketType.CRIT:

                    EchoType critType = (EchoType)int.Parse(packetBuffer.Substring(0, 2));
                    string subBuffer = packetBuffer.Substring(2);

                    //������ȣ�� �ߺ����� ����
                    int cnt = int.Parse(subBuffer.Substring(subBuffer.Length - 4, 4));
                    if (critCnt >= cnt)                    
                        break;                    
                    else
                        critCnt = cnt;

                    //EchoType�� ���� �۾��� �и�
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

                    //1�� �տ� �ٿ� �������� ����� �޾����� �˸�
                    packetBuffer = "1" + packetBuffer;
                    SendToServer(packetBuffer, PacketType.CRIT);
                    break;
            }
        }     
    }
    public void SendToServer(string str, EchoType echoType, bool isCrit)
    {
        /*
        SendToServer : �������� ������ ����

        */
        if (isConnected)
        {
            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.RemoteEndPoint = servEP;

            string fixed_gameObjectType = ((int)echoType).ToString();
            fixed_gameObjectType = fixed_gameObjectType.PadLeft(2, '0');
            str = fixed_gameObjectType + str;

            byte[] bufferToSend;

            //�������� �ʿ��� ��Ŷ�� ������ ��� "0"�� packetType�� str ���̿� ���� ("0" : �������� ������ ���ڸ� ��Ź, "1" : �������� ����� �޾����� �˸�) 
            if (isCrit)
                bufferToSend = Encoding.UTF8.GetBytes(SetPacketType(str, PacketType.CRIT).Insert(2, "0"));
            //�������� �ʿ���� ���
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
