#include <iostream>
#include <algorithm>
#include <list>
#include <map>

#include <random>
#include <sstream>
#include <iomanip>

#include <thread>
#include <mutex>

#include <process.h>
#include <winsock2.h>
#include <windows.h>
using namespace std;



#define BUF_SIZE 1024
#define PINGTHREAD_WAIT_TIME 1000
#define MAX_DISCONNECT_COUNT 3
#define CRIT_TIME_OUT 5



//송수신용 구조체
typedef struct PACKET
{
	OVERLAPPED overlapped;
	SOCKADDR_IN clntAddr;
	int clntAddr_Size;
	WSABUF wsaBuf;
	char buf[BUF_SIZE];
	bool isReceving;
} *LP_PACKET;

//패킷의 용도를 구분하기 위한 열거형
enum PacketType
{
	ACCEPT = 1,
	INIT = 2,
	PING = 3,
	ECHO = 4,
	CRIT = 5,
	HOST = 6
};

struct Player
{
	SOCKADDR_IN addr;				//플레이어의  IP/Port
	string name;					//플레이어 이름
	bool isHost = false;			//플레이어의 호스트 유무
	int ping;						//4자리 숫자 (서버 ping과 플레이어의 ping이 일치하는지 확인하기 위한 용도)
	int disconnectCnt = 0;			//연결 해제 카운트 (일정량 이상이 되면 연결해제)
};
bool operator==(const Player& a, const Player& b)
{
	if ((a.addr.sin_addr.s_addr == b.addr.sin_addr.s_addr && a.addr.sin_port == b.addr.sin_port) || a.name == b.name)
		return true;
	return false;	
}

struct CritStruct
{
	SOCKET servSock;
	string packet;
	PacketType packetType;
};
bool operator==(const SOCKADDR_IN& a, const SOCKADDR_IN& b)
{
	if (a.sin_addr.s_addr == b.sin_addr.s_addr && a.sin_port == b.sin_port && a.sin_family == b.sin_family)
		return true;

	return false;
}

list<Player> playerList;
map<string, list<SOCKADDR_IN>> critMap;

int critCnt = 0;
mutex critMap_mtx;
mutex critCnt_Mtx;


unsigned WINAPI IOCPThread(void* arg);
void CritThread(void* arg);
void PingThread(void* arg);

void StartCritThread(list<SOCKADDR_IN> critList, SOCKET servSock, string packetBuffer, PacketType packetType);
string GetCritCnt();
string SetPacketType(PacketType _packetType);
void ErrorHandling(const char* message);


int main(void)
{
	WSADATA wsaData;
	SOCKET servSock;
	SOCKADDR_IN servAddr;

	WSAEVENT readEvent;
	HANDLE iocp;

	//포트 입력
	cout << "Press Port number to start server : ";
	int port;
	cin >> port;

	//Version 2.2
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
		ErrorHandling("WSAStartup() error");

	//UDP 소켓 생성 (Overlapped IO + Non-blocking)
	servSock = WSASocket(PF_INET, SOCK_DGRAM, 0, NULL, 0, WSA_FLAG_OVERLAPPED);
	if (servSock == INVALID_SOCKET)
		ErrorHandling("socket() error");

	//소켓 바인드
	memset(&servAddr, 0, sizeof(servAddr));
	servAddr.sin_family = AF_INET;
	servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	servAddr.sin_port = htons(port);
	if (bind(servSock, (const SOCKADDR*)&servAddr, sizeof(servAddr)) == SOCKET_ERROR)
		ErrorHandling("bind() error");

	//수신 이벤트 초기화 (Non-signaled + Auto Reset)
	readEvent = CreateEvent(NULL, false, false, NULL);
	if (WSAEventSelect(servSock, readEvent, FD_READ) == SOCKET_ERROR)
		ErrorHandling("WSAEventSelect() error");

	//IOCP 초기화 (IO 완료 시 IOCPThread에서 처리)
	iocp = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, 0, 0);
	if (CreateIoCompletionPort((HANDLE)servSock, iocp, (ULONG_PTR)servSock, 0) == NULL)
		ErrorHandling("CreateIoCompletionPort() error");
	
	//IOCPThread 시작
	if (_beginthreadex(NULL, 0, IOCPThread, (void*)iocp, 0, NULL) == 0)
		ErrorHandling("Failed to begin IOCPThread");

	//PingThread 시작
	thread pingThread(PingThread, (void*)servSock);
	pingThread.detach();

	//서버 시작
	cout << endl << "---Server sucessfully started---" << endl << endl;
	while (true)
	{
		//소켓 버퍼에 데이터가 들어올 때까지 블럭
		WaitForSingleObject(readEvent, INFINITE);

		//수신용 패킷 동적할당 (이후 IOCPThread에서 할당해제)
		LP_PACKET packet = new PACKET;
		memset(&(packet->overlapped), 0, sizeof(packet->overlapped));
		packet->clntAddr_Size = sizeof(packet->clntAddr);
		packet->wsaBuf.buf = packet->buf;
		packet->wsaBuf.len = sizeof(packet->buf);
		packet->isReceving = true;

		DWORD flags = 0;

		WSARecvFrom(
			servSock,
			&(packet->wsaBuf),
			1,
			NULL,
			&flags,
			(SOCKADDR*)&(packet->clntAddr),
			&(packet->clntAddr_Size),
			&(packet->overlapped),
			NULL);
	}

	WSACloseEvent(readEvent);;
	closesocket(servSock);
	WSACleanup();

	return 0;
}

unsigned WINAPI IOCPThread(void* arg)
{
	HANDLE iocp = (HANDLE)arg;
	SOCKET servSock;
	LP_PACKET packet;
	DWORD bytesTransferred;

	while (true)
	{
		//입출력이 완료될 때까지 블록
		GetQueuedCompletionStatus(iocp, &bytesTransferred, (PULONG_PTR)&servSock, (LPOVERLAPPED*)&packet, INFINITE);

		//수신받는 패킷에 한해서만 IOCP 적용
		if (!packet->isReceving)
			continue;		

		//수신받은 패킷에서 쓰레기 값 처리
		string packetBuffer = packet->buf;
		packetBuffer = packetBuffer.substr(0, packetBuffer.find(-51));

		//패킷의 용도와 실제 데이터를 분리
		int packetType = strtol(packetBuffer.substr(0, 2).c_str(), NULL, 16);

		packetBuffer = packetBuffer.substr(2);

		//송신용 패킷 초기화
		PACKET packetForSending;
		memset(&packetForSending.overlapped, 0, sizeof(packetForSending.overlapped));
		packetForSending.wsaBuf.buf = packetForSending.buf;
		packetForSending.wsaBuf.len = sizeof(packetForSending.buf);
		packetForSending.isReceving = false;

		//플레이어가 서버에 존재하는지 탐색
		Player playerForFind; 
		playerForFind.addr = packet->clntAddr;

		auto currentPlayer = find(playerList.begin(), playerList.end(), playerForFind);


		//용도에 따라 작업을 분리
		switch (packetType)
		{

		case PacketType::ACCEPT:
			/*			
			ACCEPT 과정)

			1. 클라이언트 -> 서버 (접속 요청)
				1번 실패 시 클라이언트에서 에러 출력

			2. 서버 -> 클라이언트 (접속 확인)

			3. 클라이언트 -> 서버 (접속 확인)
				3번 실패 시 서버에서 2번을 CRIT_TIME_OUT일 때까지 재시도			

			*/

			//1. 클라이언트 -> 서버 (접속 요청)
			if (packetBuffer[0] == '0')
			{
				//플레이어가 리스트에 존재하는지 확인
				if (currentPlayer == playerList.end())
				{
					//리스트에 넣을 플레이어의 이름 초기화
					playerForFind.name = packetBuffer.substr(1);

					//리스트에 플레이어 저장
					playerList.push_back(playerForFind);

					//critMap에 사용할 4자리 고유번호를 가져와 packetBuffet 뒤에 붙인다
					string cnt = GetCritCnt();
					packetBuffer += '~' + cnt;

					//critList에 패킷을 전송할 대상들을 push_back
					list<SOCKADDR_IN> critList;
					critList.push_back(playerForFind.addr);

					//클라이언트에게 전송 (재전송을 보장)
					StartCritThread(critList, servSock, packetBuffer, PacketType::ACCEPT);
				}
				else
				{
					//동일한 이름의 플레이어가 있는 경우 접속을 허용하지 말 것
				}
			}

			//3. 클라이언트 -> 서버 (접속 확인)
			else if (packetBuffer[0] == '1')
			{
				cout << (*currentPlayer).name << "(" << inet_ntoa(packet->clntAddr.sin_addr) << ")" << " has accessed to server" << endl;

				packetBuffer[0] = '0';			
				auto it = find(critMap[packetBuffer].begin(), critMap[packetBuffer].end(), packet->clntAddr);
				if (it != critMap[packetBuffer].end())
				{
					if (critMap_mtx.try_lock())
					{
						critMap[packetBuffer].erase(it);
						critMap_mtx.unlock();
					}
				}

				//최초로 들어온 플레이어인 경우 호스트로 지정
				if (playerList.size() == 1)
				{
					//critMap에 사용할 4자리 고유번호를 가져온다
					string cnt = GetCritCnt();

					list<SOCKADDR_IN> critList;
					critList.push_back(packet->clntAddr);

					StartCritThread(critList, servSock, cnt, PacketType::HOST);
				}
			}
			break;





		case PacketType::HOST:

			if (1)
			{
				auto it = find(critMap[packetBuffer].begin(), critMap[packetBuffer].end(), packet->clntAddr);
				if (it != critMap[packetBuffer].end())
				{
					if (critMap_mtx.try_lock())
					{
						critMap[packetBuffer].erase(it);
						critMap_mtx.unlock();
					}
				}

				currentPlayer->isHost = true;
				cout << currentPlayer->name << "(" << inet_ntoa(currentPlayer->addr.sin_addr) << ")" << " is now set as host" << endl;
			}
			break;





		case PacketType::INIT:

			if (currentPlayer != playerList.end())
			{
				//클라이언트가 다른 클라이언트들의 정보를 요청한 경우
				if (packetBuffer.empty())
				{
					packetBuffer = currentPlayer->name;

					string cnt = GetCritCnt();
					packetBuffer += cnt;

					list<SOCKADDR_IN> critList;

					//보낸 클라이언트를 제외한 나머지 클라이언트를 리스트에 push
					for (auto it = playerList.begin(); it != playerList.end(); it++)
					{
						if (it == currentPlayer)
							continue;

						critList.push_back(it->addr);
					}

					StartCritThread(critList, servSock, packetBuffer, PacketType::INIT);
				}

				//다른 클라이언트들로부터 요청한 실시간 정보를 받은 경우
				else
				{
					int pos = packetBuffer.find('~');

					string critStr = packetBuffer.substr(0, pos);

					critMap_mtx.lock();

					auto exist = critMap.find(critStr);
					if (exist != critMap.end())
					{
						auto iterator = find(critMap[critStr].begin(), critMap[critStr].end(), currentPlayer->addr);

						if (iterator != critMap[critStr].end())
							critMap[critStr].erase(iterator);
					}

					critMap_mtx.unlock();

					//이름으로 playerList에서 플레이어를 탐색
					Player initPlayer;
					initPlayer.name = packetBuffer.substr(0, pos - 4);
					auto it = find(playerList.begin(), playerList.end(), initPlayer);

					if (critMap.find(packetBuffer) == critMap.end())
					{
						packetBuffer = packetBuffer.substr(pos + 1);

						//플레이어가 존재하는 경우 데이터를 전송
						if (it != playerList.end())
						{
							string cnt = GetCritCnt();
							packetBuffer += '~' + cnt;

							list<SOCKADDR_IN> critList;
							critList.push_back(it->addr);

							StartCritThread(critList, servSock, packetBuffer, PacketType::INIT);
						}
					}
					else
					{
						critMap_mtx.lock();

						if (critMap.find(packetBuffer) != critMap.end())
						{
							auto it = find(critMap[packetBuffer].begin(), critMap[packetBuffer].end(), currentPlayer->addr);

							if (it != critMap[packetBuffer].end())
								critMap[packetBuffer].erase(it);
						}

						critMap_mtx.unlock();
					}
				}
			}
			break;





		case PacketType::PING:

			if (currentPlayer != playerList.end())			
				(*currentPlayer).ping = stoi(packetBuffer);			
			break;






		case PacketType::ECHO:

			if (currentPlayer != playerList.end())
			{
				packetBuffer = SetPacketType(PacketType::ECHO) + packetBuffer;
				strcpy(packetForSending.buf, packetBuffer.c_str());

				for (auto it = playerList.begin(); it != playerList.end(); it++)
				{
					if (it == currentPlayer)
						continue;

					WSASendTo(
						servSock,
						&packetForSending.wsaBuf,
						1,
						NULL,
						0,
						(const SOCKADDR*)&it->addr,
						sizeof(it->addr),
						&packetForSending.overlapped,
						NULL
					);
				}
			}
			break;




		case PacketType::CRIT:

			if (currentPlayer != playerList.end())
			{
				string critType = packetBuffer.substr(0,1);
				packetBuffer = packetBuffer.substr(1);

				//클라이언트로부터 재전송을 보장한 패킷을 받은 경우
				if (critType == "0")
				{
					list<SOCKADDR_IN> critList;

					//critMap의 키가 겹치지 않기 위해 뒤에 4자리의 숫자를 붙인다
					string cnt = GetCritCnt();

					packetBuffer += "~" + cnt;

					//보낸 클라이언트를 제외한 나머지 클라이언트를 리스트에 push
					for (auto it = playerList.begin(); it != playerList.end(); it++)
					{
						if (it == currentPlayer)
							continue;

						critList.push_back(it->addr);
					}

					StartCritThread(critList, servSock, packetBuffer, PacketType::CRIT);
				}
				//클라이언트로부터 패킷이 제대로 도착한 신호를 받은 경우
				else if (critType == "1")
				{
					auto it = find(critMap[packetBuffer].begin(), critMap[packetBuffer].end(), packet->clntAddr);

					if (it != critMap[packetBuffer].end())
					{
						if (critMap_mtx.try_lock())
						{
							critMap[packetBuffer].erase(it);
							critMap_mtx.unlock();
						}
					}
				}				
			}			
			break;
		}



		//사용 완료한 패킷의 동적할당 해제
		delete packet;
	}

	return 0;
}

void CritThread(void* arg)
{
	/*
	CritThread : 재전송이 필요한 패킷을 받은 경우 작동
	1. 재전송 목록에 있는 클라이언트들에게 패킷을 전송
	2. 대기 (이 사이에 IOCPThread에서 수신을 받으면 재전송 목록에서 삭제)
	3. 대기 후에도 재전송 목록에 클라이언트가 남아있다면 다시 1부터 반복
	*/

	//스레드 변수 초기
	CritStruct* critStruct = (CritStruct*)arg;
	SOCKET servSock = critStruct->servSock;
	string packet = critStruct->packet;
	PacketType packetType = critStruct->packetType;

	//동적할당 해제 (CritThread 시작 직전에 할당)
	delete critStruct;

	//송신용 구조체 초기화
	PACKET packetForSending;
	memset(packetForSending.buf, 0, sizeof(packetForSending.buf));
	memset(&packetForSending.overlapped, 0, sizeof(packetForSending.overlapped));
	packetForSending.wsaBuf.buf = packetForSending.buf;
	packetForSending.wsaBuf.len = sizeof(packetForSending.buf);
	packetForSending.isReceving = false;
	strcpy(packetForSending.buf, (SetPacketType(packetType) + packet).c_str());

	int waitTime = 0;

	//1. 보낸 클라이언트를 제외한 나머지 클라이언트들에게 에코 (3. 대기 후에도 재전송할 클라이언트 목록이 남아있다면 다시 1부터 반복)
	while (!critMap[packet].empty())
	{
		critMap_mtx.lock();
		for (auto it : critMap[packet])
		{
			WSASendTo(
				servSock,
				&packetForSending.wsaBuf,
				1,
				NULL,
				0,
				(const SOCKADDR*)&it,
				sizeof(it),
				&packetForSending.overlapped,
				NULL
			);
		}
		critMap_mtx.unlock();

		//2. 대기
		this_thread::sleep_for(chrono::milliseconds(1000));
		
		//3. 특정 시간 내로 스레드를 탈출 못하면 break
		waitTime += 1000;
		if (waitTime >= 5000)
			break;		
	}
	 
	//스레드 종료 (=재전송 완료)
	critMap_mtx.lock();
	critMap.erase(packet);
	critMap_mtx.unlock();
}

void PingThread(void* arg)
{
	/*
	PingThread : 서버 시작 직후 호출되며 다음과 같이 작동한다
	 1. 플레이어 리스트에 저장되어 있는 모든 IP에게 지속적으로 SYN을 보낸다 (SYN은 1000~9998 사이의 난수)
	 2. PINGTHREAD_WAIT_TIME만큼 기다리며 그 사이의 클라이언트로부터 ACK를 받아 ACK == SYN +1인지 확인한다
	 3. ACK 값이 다른 경우 카운트를 1 올리며 카운트가 MAX_DISCONNECT_COUNT가 된 경우 리스트에서 IP/Port를 제거한다 (=연결해제)
	 4. 연결 해제 시 다른 모든 플레이어에게 연결이 해제된 플레이어의 정보를 전송한다 (4.의 경우 패킷의 재전송을 보장한다)

	*/

	SOCKET servSock = (SOCKET)arg;

	//난수 생성기 초기화 (1000 ~ 9998 사이의 난수) 
	random_device rd;
	mt19937 gen(rd());
	uniform_int_distribution<> dist(1000, 9998);

	//전송용 구조체 초기화
	PACKET packet;
	memset(packet.buf, 0, sizeof(packet.buf));
	memset(&packet.overlapped, 0, sizeof(packet.overlapped));
	packet.wsaBuf.buf = packet.buf;
	packet.wsaBuf.len = sizeof(packet.buf);
	packet.isReceving = false;

	while (true)
	{
		//난수 생성
		int currentPing = dist(gen);

		//전송용 구조체에 string 복사
		strcpy(packet.buf, (SetPacketType(PacketType::PING) + to_string(currentPing)).c_str());

		//모든 플레이어에게 핑 전송 (Ex. 031111 형태)
		for(auto player : playerList)
		{
			WSASendTo(
				servSock,
				&packet.wsaBuf,
				1,
				NULL,
				0,
				(const SOCKADDR*)&player.addr,
				sizeof(player.addr),
				&packet.overlapped,
				NULL
			);
		}


		//일정 시간 대기
		this_thread::sleep_for(chrono::milliseconds(PINGTHREAD_WAIT_TIME));
		

		//모든 플레이어를 순회하면서 핑이 정상적으로 도착했는지 체크
		for (auto it = playerList.begin(); it != playerList.end(); it++)
		{
			//핑 == 전송한 핑 + 1이 아닌 경우 카운트를 1 올린다
			if ((*it).ping != currentPing + 1)
			{
				(*it).disconnectCnt++;

				//카운트가 MAX_DISCONNECT_COUNT를 초과한 경우 플레이어 연결 해제
				if ((*it).disconnectCnt >= MAX_DISCONNECT_COUNT)
				{
					cout << (*it).name << "(" << inet_ntoa((*it).addr.sin_addr) << ")" << " has disconnected" << endl;
					string playerName = (*it).name;

					auto temp = it;
					it++;
	
					//연결 해제시킨 플레이어가 호스트인 경우 다음 플레이어에게 호스트를 이관
					if (temp->isHost && it != playerList.end())
					{
						it->isHost = true;

						strcpy(packet.buf, (SetPacketType(PacketType::HOST).c_str()));

						WSASendTo(
							servSock,
							&packet.wsaBuf,
							1,
							NULL,
							0,
							(const SOCKADDR*)&it->addr,
							sizeof(it->addr),
							&packet.overlapped,
							NULL
						);

						cout << it->name << "(" << inet_ntoa(it->addr.sin_addr) << ")" << " is now set as host" << endl;
					}					

					//플레이어 리스트에서 플레이어 삭제
					playerList.erase(temp);

					//보낸 클라이언트를 제외한 나머지 클라이언트를 리스트에 push
					list<SOCKADDR_IN> critList;					
					for (auto it = playerList.begin(); it != playerList.end(); it++)
					{
						critList.push_back(it->addr);
					}

					string cnt = GetCritCnt();
					cnt = "06" + playerName + "~" + cnt;
					StartCritThread(critList, servSock, cnt, PacketType::CRIT);

					//앞에서 it++를 하는데 이때 이미 끝에 도달한 경우 break
					if (it == playerList.end())
						break;
				}
			}
			else
				(*it).disconnectCnt = 0;				
		}		
	}

}

void StartCritThread(list<SOCKADDR_IN> critList, SOCKET servSock, string packetBuffer, PacketType packetType)
{
	/*
	StartCritThread : 사전작업으로 critMap 초기화 및 동적할당 후 critThread 시작 

	*/

	//critMap에 critList 대입
	critMap[packetBuffer] = critList;

	//스레드에서 사용할 구조체 동적할당 (할당해제는 critThread에서 실시)
	CritStruct* critStruct = new CritStruct;
	critStruct->servSock = servSock;
	critStruct->packet = packetBuffer;
	critStruct->packetType = packetType;

	//CritThread 시작
	thread critThread(CritThread, (void*)critStruct);
	critThread.detach();
}

string GetCritCnt()
{
	/*
	GetCritCnt : critMap의 키가 겹치지 않기 위해 4자리의 숫자를 string으로 만든다 (critCnt는 전역변수이므로 다른 스레드와 겹치지 않게 동기화 작업)

	*/

	critCnt_Mtx.lock();

	string cnt = to_string(critCnt);
	critCnt++;
	if (critCnt >= 9999)
		critCnt = 0;

	critCnt_Mtx.unlock();

	//4자리를 유지하기 위해 부족한만큼 앞에 0을 체워준다
	while (cnt.length() < 4)
		cnt = "0" + cnt;

	return cnt;
}

string SetPacketType(PacketType _packetType)
{
	/*
	SetPacketType : enum인 PacketType를 받아 16진수로 변환 후 십의 자리가 비었다면 '0'을 채운 후 string으로 반환한다

	*/

	ostringstream os;
	os << setfill('0') << setw(2) << hex << _packetType;
	return os.str();
}

void ErrorHandling(const char* message)
{
	/*
	ErrorHandling : 에러 발생 시 발생 시 에러 출력 후 프로그램 종료

	*/

	fputs(message, stderr);
	fputc('\n', stderr);
	exit(1);
}