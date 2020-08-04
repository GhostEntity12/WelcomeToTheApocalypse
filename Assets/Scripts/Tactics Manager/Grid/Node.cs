using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{

	public int[] m_neighbours = new int[8];
	public int[] m_costs = new int[8];
	public int x, z;
	public int fScore, gScore, hScore;

	public bool isWalkable;

	public Vector3 worldPosition;

	public GridObject obstacle;

	public GameObject unit;

	public GameObject m_tile;

	public Node m_previousNode;

	[Header("Breadth First Search")]
	public bool visited = false;
	public Node parentNode = null;
	public int distance = 0;

	public List<Node> adjacentNodes = new List<Node>();

	public void Reset()
	{
		m_tile.SetActive(false);
		visited = false;
		parentNode = null;
		distance = 0;
	}

	public void CalculateFScore()
	{
		fScore = gScore + hScore;
	}
}
