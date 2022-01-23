using UnityEngine;
using System.Collections.Generic;

namespace StartFramework.GamePlay.Astar
{
    public class Pathfinding : MonoBehaviour
    {
        public Transform seeker, target;
        IGrid grid;

        //点击寻路
        private int currentPathIndex;

        void Awake()
        {
            grid = GetComponent<IGrid>();
        }

        private void Start()
        {
            //FindPath(seeker.position, target.position);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                //还原
                currentPathIndex = 0;

                FindPath(seeker.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }

            if (grid.path != null && grid.path.Count > 0)
            {
                Vector3 targetPosition = grid.path[currentPathIndex].worldPosition;
                if (Vector3.Distance(seeker.transform.position, targetPosition) > 0.1f)
                {
                    Vector3 moveDir = (targetPosition - seeker.transform.position).normalized;
                    seeker.transform.position = seeker.transform.position + moveDir * 6f * Time.deltaTime;
                }
                else
                {
                    currentPathIndex++;
                    if (currentPathIndex >= grid.path.Count)
                    {
                        //校准和最后一个点的位置
                        seeker.transform.position = grid.path[currentPathIndex - 1].worldPosition;
                        //还原
                        grid.path.Clear();
                        grid.path = null;
                        currentPathIndex = 0;
                    }
                }
            }
        }

        void FindPath(Vector3 startPos, Vector3 targetPos)
        {
            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node targetNode = grid.NodeFromWorldPoint(targetPos);

            List<Node> openSet = new List<Node>(); //open列表：等待评估
                                                   //哈希集
                                                   //优点：提供高性能的set操作。
                                                   //特点：不包含重复元素，无特定顺序，无索引。
                                                   //ps：如果顺序或元素复制比应用程序的性能更重要，请考虑结合使用 List<T> 类和 Sort 方法。
            HashSet<Node> closedSet = new HashSet<Node>(); //close列表：评估完毕
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                //open列表中的成员进行比对，找出一个最佳节点
                Node node = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost)
                    {
                        if (openSet[i].hCost < node.hCost)
                            node = openSet[i];
                    }
                }
                openSet.Remove(node);
                closedSet.Add(node);

                //最终...找到终点
                if (node == targetNode)
                {
                    RetracePath(startNode, targetNode);
                    return;
                }

                //找邻居 → 根据当前节点重新计算邻居的代价 → 将所有邻居放入open列表
                foreach (Node neighbour in grid.GetNeighbours(node))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    //***根据当前节点刷新邻居的g代价
                    //只有新的代价小于之前的代价 才刷新邻居的g代价值
                    int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                    if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = node;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }

        //回溯:根据终点向回找父节点
        //如何得出的这个最佳路径：
        //--找邻居→评估代价→更新当前节点→找邻居刷新值→循环，直到找到了终点，然后依据终点的父节点回溯出路径。
        void RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Reverse();

            grid.path = path;
        }

        int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}