using UnityEngine;
using System.Collections;

namespace StartFramework.GamePlay.Astar
{
    public class Node
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX; //二维数组的坐标
        public int gridY; //二维数组的坐标	

        /// <summary>
        /// 左上角，此节点与起点的距离。受当前节点影响，如果有较小的代价则会更新。
        /// </summary>
        public int gCost;
        /// <summary>
        /// 右上角，此节点与终点的距离
        /// </summary>
        public int hCost;
        public Node parent;

        public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
        {
            walkable = _walkable;
            worldPosition = _worldPos;
            gridX = _gridX;
            gridY = _gridY;
        }

        /// <summary>
        /// 代价合计
        /// </summary>
        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }
    }
}