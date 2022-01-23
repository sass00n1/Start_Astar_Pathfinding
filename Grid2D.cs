using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace StartFramework.GamePlay.Astar
{
    public class Grid2D : MonoBehaviour, IGrid
    {
        public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        public float nodeRadius;

        //2D
        public Tilemap tilemap;
        public bool debug;

        Node[,] grid;
        float nodeDiameter; //节点直径
        int gridSizeX, gridSizeY; //寻路网格的尺寸

        void Awake()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter); //30
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter); //30
            CreateGrid();
        }

        //创建寻路网格 → 烘焙node
        void CreateGrid()
        {
            grid = new Node[gridSizeX, gridSizeY];

            //寻路网格的左下角世界坐标
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    //此node的世界坐标
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                    //HACK:烘焙寻路网格时，障碍的识别用了这种硬编码方式
                    bool walkable = true;
                    TileBase tileBase = tilemap.GetTile(tilemap.WorldToCell(worldPoint));
                    if (tileBase != null)
                    {
                        string tileBaseName = tileBase.name.Substring(0, 4);
                        if (tileBaseName == "Wall")
                        {
                            walkable = false;
                        }
                    }
                    grid[x, y] = new Node(walkable, worldPoint, x, y);
                }
            }
        }

        /// <summary>
        /// 找邻居
        /// </summary>
        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    if (x < 0 && y < 0) continue;
                    if (x > 0 && y < 0) continue;
                    if (x < 0 && y > 0) continue;
                    if (x > 0 && y > 0) continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        neighbours.Add(grid[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }


        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = (int)((gridSizeX) * percentX);
            int y = (int)((gridSizeY) * percentY);

            //鼠标点击地图外后，坐标将变为最大，这会越界。
            if (x == gridSizeX)
            {
                x--;
            }
            if (y == gridSizeY)
            {
                y--;
            }

            return grid[x, y];
        }

        public List<Node> path { get; set; }
        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

            if (grid != null)
            {
                foreach (Node n in grid)
                {
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    if (path != null)
                        if (path.Contains(n))
                            Gizmos.color = Color.green;

                    if (debug == true)
                    {
                        Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                    }
                }
            }
        }
    }
}