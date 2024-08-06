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
        //���� �ý��� �̱���
        if (gameSystem == null)
            gameSystem = this;

        else
            Destroy(gameSystem);

        players = new List<Transform>();

        //���� ����ϴ� Ÿ�ϸʸ� �ε�
        ground.CompressBounds();
        obstacle.CompressBounds();
        bounds = ground.cellBounds;

        //Ÿ�ϸ��� Vector3[,]�� ��ȯ (�� Ÿ���� �߽����� �������� x,y ���� �ְ� z�� ��ֹ����� �ƴ��� ����)
        CreateGrid();

        //isGameStarted�� ���� ���۵Ǵ� �ڷ�ƾ ����
        StartCoroutine(StartGame());
    }

    private void Start()
    {
        players.Add(Player.player.transform);
    }

    public void StartGameByButton()
    {
        //�������� ���� ������ �˸�
        if (ClientSystem.clientSystem != null && ClientSystem.clientSystem.isHost)
            ClientSystem.clientSystem.SendToServer("", ClientSystem.EchoType.START, true);

        //�ڷ�ƾ ����
        isGameStarted = true;
    }

    private IEnumerator StartGame()
    {
        while (!isGameStarted)
            yield return null;

        //��� �Ϸ��� UI ��Ȱ��ȭ
        playerCount.SetActive(false);
        disableStart.SetActive(false);

        //Ÿ�̸� ���� (Ÿ�̸��� �ð��� �������� ���͸� ����)
        timer.isStarted = true;

        //���׷��̵� ī�� �̱�
        upgradeSystem.FirstDraw();

        //�� ����
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
                //��ֹ��� �����ϴ� ��� z = 1
                if (obstacle.HasTile(new Vector3Int(x, y, 0)))                
                    cells[i, j] = new Vector3Int(x, y, 1);
                
                //��ֹ��� �������� �ʰ� ���� �����ϴ� ��� z = 0
                else if (ground.HasTile(new Vector3Int(x, y, 0)))                
                    cells[i, j] = new Vector3Int(x, y, 0);

                //�� �� �ƴ� ��� ���� �� ���� ������ ����Ͽ� z = 1
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
