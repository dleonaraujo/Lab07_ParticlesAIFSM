using UnityEngine;
using System.Collections.Generic;

public class AStarPathfinder : MonoBehaviour
{
    public PathGrid grid;

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node current = GetLowestFNode(openList);

            if (current == targetNode)
                return BuildPath(startNode, targetNode);

            openList.Remove(current);
            closedList.Add(current);

            foreach (Node neighbor in grid.GetNeighbors(current))
            {
                if (!neighbor.walkable || closedList.Contains(neighbor))
                    continue;

                int newCostToNeighbor = current.gCost + GetDistance(current, neighbor);

                if (newCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = current;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
        return null;
    }

    Node GetLowestFNode(List<Node> list)
    {
        Node lowest = list[0];

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].fCost < lowest.fCost ||
                (list[i].fCost == lowest.fCost && list[i].hCost < lowest.hCost))
                lowest = list[i];
        }
        return lowest;
    }

    int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);
        return dstX + dstY;
    }

    List<Vector3> BuildPath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node current = endNode;

        while (current != startNode)
        {
            path.Add(current.worldPosition);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }
}