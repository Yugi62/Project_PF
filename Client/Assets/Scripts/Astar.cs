using System.Collections.Generic;
using UnityEngine;

public class Astar
{
    Node[,] nodes;

    public List<Node> CreatePath(Vector3Int[,] grid, Vector2Int vStart, Vector2Int vEnd)
    {
        Node nStart = null;
        Node nEnd = null;

        //행의 최대 인덱스 + 1
        int gridY = grid.GetUpperBound(0) + 1;
        //열의 최대 인덱스 + 1
        int gridX = grid.GetUpperBound(1) + 1;

        //노드 정보 초기화
        nodes = new Node[gridY, gridX];
        for (int i = 0; i < gridY; i++)
        {
            for (int j = 0; j < gridX; j++)
            {
                //특정 인덱스에 해당하는 grid의 정보를 노드에 초기화
                nodes[i, j] = new Node(grid[i, j].x, grid[i, j].y, grid[i, j].z < 1 ? true : false);
            }
        }

        for (int i = 0; i < gridY; i++)
        {
            for (int j = 0; j < gridX; j++)
            {
                //노드의 인접노드 정보를 추가
                nodes[i, j].AddAdjacents(nodes, i, j);

                //벡터에 해당하는 시작과 끝 노드를 탐색
                if (nodes[i, j].x == vStart.x && nodes[i, j].y == vStart.y)
                    nStart = nodes[i, j];
                else if (nodes[i, j].x == vEnd.x && nodes[i, j].y == vEnd.y)
                    nEnd = nodes[i, j];
            }
        }

        //시작과 끝이 존재하는지 검사
        if (nStart == null || nEnd == null || !nEnd.isAccessible)
            return null;

        List<Node> openList = new List<Node>();
        List<Node> closeList = new List<Node>();
        openList.Add(nStart);

        while (openList.Count > 0)
        {
            /*
            A* 알고리즘
            1. openList에서 가장 비용이 작은 노드를 선택 (이후 이 노드를 선택 노드라고 칭한다)
            2. 선택 노드가 도착 지점인 경우 경로 제작 후 반환
            3. 아닌 경우 선택 노드를 clostList에 추가 후 openList에서 선택노드를 삭제 
            4. 선택 노드의 인접 노드의 f,g,h를 계산
            5. 인접 노드가 closeList에 없는 경우 openList에 추가
            6. 2번 조건 혹은 openList의 값이 없을 때까지 1~5를 반복
            */

            //1. openList에서 가장 비용이 작은 노드를 선택 (비용이 같은 경우 h가 더 작은 값을 선택)
            int selectedIndex = 0;
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].f < openList[selectedIndex].f)
                    selectedIndex = i;
                else if (openList[i].f == openList[selectedIndex].f)
                {
                    if (openList[i].h < openList[selectedIndex].h)
                        selectedIndex = i;
                }
            }

            //2. 선택 노드가 도착 지점인 경우 경로 제작 후 반환
            var currentNode = openList[selectedIndex];
            if (currentNode == nEnd)
            {
                List<Node> path = new List<Node>();

                var tempNode = currentNode;

                path.Add(tempNode);

                while (tempNode.parentNode != null)
                {
                    path.Add(tempNode.parentNode);
                    tempNode = tempNode.parentNode;
                }

                path.Reverse();
                return path;
            }

            //3. 아닌 경우 선택 노드를 clostList에 추가 후 openList에서 선택노드를 삭제 
            closeList.Add(currentNode);
            openList.Remove(currentNode);

            var adjacentList = currentNode.adjacentNodes;
            for (int i = 0; i < adjacentList.Count; i++)
            {
                var selectedAdjacent = adjacentList[i];

                //선택 노드에 접근이 가능한지 체크
                if (!closeList.Contains(selectedAdjacent) && selectedAdjacent.isAccessible)
                {
                    if (openList.Contains(selectedAdjacent))
                        continue;

                    else
                    {
                        selectedAdjacent.g = 10;
                        selectedAdjacent.h = Heuristic(selectedAdjacent, nEnd);
                        selectedAdjacent.f = selectedAdjacent.g + selectedAdjacent.h;
                        selectedAdjacent.parentNode = currentNode;
                        openList.Add(selectedAdjacent);
                    }
                }
            }
        }

        return null;
    }

    private int Heuristic(Node a, Node b)
    {
        int x = (int)Mathf.Abs(a.x - b.x);
        int y = (int)Mathf.Abs(a.y - b.y);

        return x + y;
    }

}
public class Node
{
    public float x { get; private set; }
    public float y { get; private set; }

    public int h;
    public int g;
    public int f;

    public bool isAccessible { get; private set; }

    public List<Node> adjacentNodes;
    public Node parentNode = null;

    public Node(float _x, float _y, bool _isAccessible)
    {
        this.x = _x;
        this.y = _y;
        this.h = 0;
        this.g = 0;
        this.f = 0;
        this.isAccessible = _isAccessible;
        adjacentNodes = new List<Node>();
    }
    public void AddAdjacents(Node[,] nodes, int x, int y)
    {
        /*
        AddAdjacents
        1. 상하좌우에 존재하는 인접노드를 리스트에 넣는다
        2. 범위 밖인 경우에는 넣지 않는다
        */

        if (x < nodes.GetUpperBound(0))
            adjacentNodes.Add(nodes[x + 1, y]);

        if (x > 0)
            adjacentNodes.Add(nodes[x - 1, y]);

        if (y < nodes.GetUpperBound(1))
            adjacentNodes.Add(nodes[x, y + 1]);

        if (y > 0)
            adjacentNodes.Add(nodes[x, y - 1]);
    }
}