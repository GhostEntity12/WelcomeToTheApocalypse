using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    //The costs of movements along a path, these ints make the AI favor diagonal movements instead of jittering up, right movement.
    private const int moveDiagonalCost = 14;
    private const int moveStraightCost = 10;

    //Reference to a grid.
    //Create a list of nodes to track.
    private Grid grid;
    private List<Node> openList;
    private List<Node> closedList;

    //Movement costs.
    public int gCost; //Cost of cheapest path from start to 'n'.
    public int fCost; //Current best guess of how short the path can be.
    public int hCost; //Heuristic cost.

    //NEEDED IN NODE CLASS.
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    //private List<Node> FindPath(int startX, int startY, int endX, int endY)
    //{
    //    Node startNode = new Node();
    //    Node endNode = new Node();

    //    openList = new List<Node> { startNode };
    //    closedList = new List<Node>();

    //    //for (int x = 0; x < /*gridWidth*/; x++)
    //    //{
    //    //    for (int y = 0; y < /*gridHeight*/; y++)
    //    //    {
    //    //        Node pathNode = new Node();
    //    //        pathNode.gCost = int.MaxValue;
    //    //        //pathNode.CalculateFCost();
    //    //        pathNode.parentNode = null;
    //    //    }
    //    //}

    //    startNode.gCost = 0;
    //    //startNode.hCost = CalculateDistanceCost(startNode, endNode);
    //    //startNode.CalculateFCost();

    //    while (openList.Count > 0)
    //    {
    //        Node currentNode = GetLowestFCostNode(openList);

    //        if (currentNode == endNode)
    //        {
    //            return CalculatePath(endNode);
    //        }

    //        openList.Remove(currentNode);
    //        closedList.Add(currentNode);
    //    }
    //}

    //private List<Node> GetNeighbourList(Node currentNode)
    //{
    //    List<Node> neighbourList = new List<Node>();

    //    if (currentNode.x - 1 >= 0)
    //    { 
    //        neighbourList.Add(GetN)
    //    }
    //}

    //Does nothing for now.
    private List<Node> CalculatePath(Node endNode)
    {
        return null;
    }

    //Calculate the distance cost betweent two nodes.
    private int CalculateDistanceCost(Node a, Node b)
    {
        //Grab the bounds of x and y axis of nodes.
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);

        //Distance of padding between nodes.
        int remaining = Mathf.Abs(xDistance - yDistance);

        //Return cost.
        return moveDiagonalCost * Mathf.Min(xDistance, yDistance) + moveStraightCost * remaining;
    }

    //Returns the lowest possible path of nodes.
    private Node GetLowestFCostNode(List<Node> pathNodeList)
    {
        Node lowestFCostNode = pathNodeList[0];

        for (int i = 1; i < pathNodeList.Count; i++)
        {
            //if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            //{
            //    lowestFCostNode = pathNodeList[i];
            //}
        }

        return lowestFCostNode;
    }
}
