using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameSystem : MonoBehaviour
{
    public static GameSystem gameSystem;

    [SerializeField] private Timer timer;
    [SerializeField] private UpgradeSystem upgradeSystem;

    [SerializeField] private GameObject playerCount;
    [SerializeField] private GameObject disableStart;

    [SerializeField] private Transform startPoint;

    [SerializeField] private GameObject lobby;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap ground;
    [SerializeField] private Tilemap obstacle;

    private Vector3Int[,] cells;
    private BoundsInt bounds;

    public bool isGameStarted = false;

    public List<Transform> players;
    public List<GameObject> skills;

    private void Awake()
    {
        //게임 시스템 싱글톤
        if (gameSystem == null)
            gameSystem = this;

        else
            Destroy(gameSystem);

        players = new List<Transform>();

        //실제 사용하는 타일맵만 로드
        ground.CompressBounds();
        obstacle.CompressBounds();
        bounds = ground.cellBounds;

        //타일맵을 Vector3[,]로 변환 (각 타일의 중심지를 기준으로 x,y 값을 넣고 z로 장애물인지 아닌지 구별)
        CreateGrid();

        //isGameStarted에 따라 시작되는 코루틴 시작
        StartCoroutine(StartGame());
    }

    private void Start()
    {
        players.Add(Player.player.transform);
    }

    public void StartGameByButton()
    {
        //서버에게 게임 시작을 알림
        if (ClientSystem.clientSystem != null && ClientSystem.clientSystem.isHost)
            ClientSystem.clientSystem.SendToServer("", ClientSystem.EchoType.START, true);

        //코루틴 실행
        isGameStarted = true;
    }

    private IEnumerator StartGame()
    {
        while (!isGameStarted)
            yield return null;

        //사용 완료한 UI 비활성화
        playerCount.SetActive(false);
        disableStart.SetActive(false);

        //타이머 시작 (타이머의 시간을 기준으로 몬스터를 생성)
        timer.isStarted = true;

        //업그레이드 카드 뽑기
        upgradeSystem.FirstDraw();

        //맵 변경
        lobby.SetActive(false);
        grid.gameObject.SetActive(true);
    }

    private void CreateGrid()
    {
        cells = new Vector3Int[bounds.size.x, bounds.size.y];

        for (int x = bounds.xMin, i = 0; i < (bounds.size.x); x++, i++)
        {
            for (int y = bounds.yMin, j = 0; j < (bounds.size.y); y++, j++)
            {
                //장애물이 존재하는 경우 z = 1
                if (obstacle.HasTile(new Vector3Int(x, y, 0)))                
                    cells[i, j] = new Vector3Int(x, y, 1);
                
                //장애물이 존재하지 않고 땅이 존재하는 경우 z = 0
                else if (ground.HasTile(new Vector3Int(x, y, 0)))                
                    cells[i, j] = new Vector3Int(x, y, 0);

                //둘 다 아닌 경우 밟을 수 없는 땅으로 취급하여 z = 1
                else                
                    cells[i, j] = new Vector3Int(x, y, 1);                
            }
        }
    }

    public Vector3Int[,] GetGrid()
    {
        return cells;
    }

    public Vector2Int WorldToGrid(Vector3 position)
    {
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        Vector3 vec3Pos = grid.CellToWorld(grid.WorldToCell(position));

        Vector2Int vec2Pos = new Vector2Int();
        vec2Pos.x = (int)vec3Pos.x;
        vec2Pos.y = (int)vec3Pos.y;

        return vec2Pos;
    }

    public void DestroyAllSkills()
    {
        foreach(GameObject skill in skills)
            Destroy(skill);
        
        skills.Clear();
    }

    public void CheckPlayerIsAllDead()
    {
        bool isAllDead = true;
        foreach(Transform p in players)
        {
            if (!p.GetComponent<Character>().isDead)
                isAllDead = false;
        }

        if (isAllDead)
            ClientSystem.clientSystem.ResetGame();        
    }
}
