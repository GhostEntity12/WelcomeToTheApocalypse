using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{

	public int[] m_neighbours = new int[8];
	public int x;
	public int y;
	public int z;
	public float fCost;
	public float gCost;

	public bool isWalkable;

	public Vector3 worldPosition;

	public GridObject obstacle;

	public GameObject unit;

	public GameObject tile;


	[Header("Breadth First Search")]
	public bool visited = false;
	public Node parentNode = null;
	public int distance = 0;

	public List<Node> adjacentNodes = new List<Node>();

	public void Reset()
	{
		visited = false;
		parentNode = null;
		distance = 0;
	}
}
