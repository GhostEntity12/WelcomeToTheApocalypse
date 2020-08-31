using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GridObject))]
public class Grid : MonoBehaviour
{
	public static Grid m_Instance = null;
	
	Node[,] m_Grid;
	[SerializeField]
	float xzScale = 1f;

	Vector3 minPosition;

	[Tooltip("The extents of the ovelapbox when searching for walkable areas")]
	public Vector3 m_Extents = new Vector3(0.9f, 1f, 0.9f);

	int posX, posZ;

	[Tooltip("Used to show the node position with something like a really small plane or something")]
	public GameObject m_Tile;
	[Tooltip("Used to store the objects for the nodes")]
	private GameObject m_NodeArray;

	void Awake()
	{
		m_Instance = this;
		GetComponent<GridObject>().isWalkable = true;
		if (FindObjectsOfType<GridArea>().Length == 0)
		{
			Debug.LogError("Map incomplete, no areas declared walkable", this);
		}
		ReadLevel();
	}

	[ContextMenu("Generate")]
	void ReadLevel()
	{
		DestroyImmediate(GameObject.Find("Nodes"));
		m_NodeArray = new GameObject("Nodes");
		m_NodeArray.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);

		Bounds bounds = GetComponent<Collider>().bounds;

		posX = Mathf.FloorToInt(bounds.size.x / xzScale);
		posZ = Mathf.FloorToInt(bounds.size.z / xzScale);

		minPosition = new Vector3(bounds.min.x, 0, bounds.min.z);
		CreateGrid(posX, posZ);
	}

	//Creates the grid
	void CreateGrid(int a_posX, int a_posZ)
	{
		m_Grid = new Node[a_posX, a_posZ];

		for (int x = 0; x < a_posX; ++x)
		{

			for (int z = 0; z < a_posZ; ++z)
			{

				Vector3 tilePosition = minPosition;

				tilePosition.x += x * xzScale + 0.5f;
				tilePosition.z += z * xzScale + 0.5f;

				//A new node and sets it's X and Z
				Node n = new Node
				{
					x = x,
					z = z,
					worldPosition = tilePosition
				};

				Collider[] overlapNode = Physics.OverlapBox(tilePosition, m_Extents / 2, Quaternion.identity);

				if (overlapNode.Length > 0)
				{
					bool isWalkable = overlapNode.Select(o => o.GetComponent<GridArea>()).Where(g => g != null).Count() > 0;

					if (isWalkable)
					{
						for (int i = 0; i < overlapNode.Length; ++i)
						{

							GridObject obj = overlapNode[i].transform.GetComponentInChildren<GridObject>();

							if (obj != null)
							{
								if (obj.isWalkable && n.obstacle == null)
								{
									isWalkable = true;
								}
								else
								{
									isWalkable = false;
									n.obstacle = obj;
								}

							}

						}
					}

					n.m_isOnMap = isWalkable;
				}

				if (n.m_isOnMap)
				{
					n.m_tile = Instantiate(m_Tile, new Vector3(n.worldPosition.x, n.worldPosition.y + 0.01f, n.worldPosition.z), Quaternion.identity, m_NodeArray.transform);
					n.m_NodeHighlight = n.m_tile.GetComponent<NodeHighlight>();
					n.m_NodeHighlight.name = $"Node {n.x}/{n.z}";
					n.m_NodeHighlight.ChangeHighlight(TileState.None);
				}
				m_Grid[x, z] = n;

			}

		}
		SetNodeNeighbours();
	}
	/* |5|2|6|
	   |1| |3|
	   |4|0|7| */


	void SetNodeNeighbours()
	{
		for (int x = 0; x < posX; ++x)
		{
			for (int z = 0; z < posZ; ++z)
			{
				//0 South
				if (z > 0)
				{
					m_Grid[x, z].adjacentNodes.Add(m_Grid[x, z - 1]);
				}
				else
				{
					m_Grid[x, z].adjacentNodes.Add(null);
				}

				//1 West
				if (x > 0)
				{
					m_Grid[x, z].adjacentNodes.Add(m_Grid[x - 1, z]);
				}
				else
				{
					m_Grid[x, z].adjacentNodes.Add(null);
				}

				//2 North
				if (z < posZ - 1)
				{
					m_Grid[x, z].adjacentNodes.Add(m_Grid[x, z + 1]);
				}
				else
				{
					m_Grid[x, z].adjacentNodes.Add(null);
				}

				//3 East
				if (x < posX - 1)
				{
					m_Grid[x, z].adjacentNodes.Add(m_Grid[x + 1, z]);
				}
				else
				{
					m_Grid[x, z].adjacentNodes.Add(null);
				}

				//4 South West
				if (z > 0 && x > 0)
				{
					m_Grid[x, z].adjacentNodes.Add(m_Grid[x - 1, z - 1]);
				}
				else
				{
					m_Grid[x, z].adjacentNodes.Add(null);
				}

				//5 North West
				if (x > 0 && z < posZ - 1)
				{
					m_Grid[x, z].adjacentNodes.Add(m_Grid[x - 1, z + 1]);
				}
				else
				{
					m_Grid[x, z].adjacentNodes.Add(null);
				}

				//6 North East
				if (z < posZ - 1 && x < posX - 1)
				{
					m_Grid[x, z].adjacentNodes.Add(m_Grid[x + 1, z + 1]);
				}
				else
				{
					m_Grid[x, z].adjacentNodes.Add(null);
				}

				//7 South East
				if (z > 0 && x < posX - 1)
				{
					m_Grid[x, z].adjacentNodes.Add(m_Grid[x + 1, z - 1]);
				}
				else
				{
					m_Grid[x, z].adjacentNodes.Add(null);
				}

				m_Grid[x, z].m_costs[0] = 10;
				m_Grid[x, z].m_costs[1] = 10;
				m_Grid[x, z].m_costs[2] = 10;
				m_Grid[x, z].m_costs[3] = 10;
				m_Grid[x, z].m_costs[4] = 19;
				m_Grid[x, z].m_costs[5] = 19;
				m_Grid[x, z].m_costs[6] = 19;
				m_Grid[x, z].m_costs[7] = 19;
			}
		}
	}

	public Node GetNode(Vector3 wp)
	{
		Vector3 p = wp - minPosition;
		int x = Mathf.FloorToInt(p.x / xzScale);
		int z = Mathf.FloorToInt(p.z / xzScale);

		return GetNode(x, z);
	}

	public Node GetNode(int a_x, int a_z)
	{
		//Checks if the object is out of bounds it returns null
		if(a_x < 0 || a_x > posX - 1 ||
			a_z < 0 || a_z > posZ - 1 )
		{
			return null;
		}

		return m_Grid[a_x, a_z];
	}
	
	public void SetUnit(GameObject unit)
	{
		Node n = GetNode(unit.transform.position);
		n.unit = unit.GetComponent<Unit>();
		n.m_isBlocked = true;
	}

	public Unit GetUnit(Vector3 mousePos)
	{
		return GetNode(mousePos).unit;
	}

	public void RemoveUnit(Node unitNode)
	{
		unitNode.unit = null;
		unitNode.m_isBlocked = false;
	}

	public bool FindPath(Vector3 startPos, Vector3 endPos, ref Stack<Node> path, out int cost)
	{
		// Assigned just for the leaving before looking for a path.
		cost = 0;

		Node m_startNode = GetNode(startPos);
		Node m_endNode = GetNode(endPos);

		// Make sure the start and end nodes are valid.
		if (m_startNode == null || m_endNode == null)
		{
			return false;
		}

		if (m_startNode == m_endNode)
		{
			return false;
		}

		if(m_endNode.m_isOnMap == false)
		{
			return false;
		}

		// Empty the path stack.
		path.Clear();

		// Nodes not finished being used.
		Queue<Node> m_openList = new Queue<Node>();

		// Nodes to no longer be used.
		List<Node> m_closedList = new List<Node>();

		// If a path has been found.
		bool m_foundPath = false;

		m_startNode.m_previousNode = null;
		
		// Calculate the variables for the starting node.
		m_startNode.gScore = 0;
		m_startNode.hScore = CalculateHeristic(m_startNode, m_endNode);
		m_startNode.CalculateFScore();

		m_openList.Enqueue(m_startNode);

		// While there is a node in the open list, look for a path.
		while (m_openList.Count > 0)
		{
			// Start searching from the most recent node in the open list.
			Node currentNode = m_openList.Dequeue();

			// Don't search from this node anymore.
			m_closedList.Add(currentNode);

			// If the node being checked is the end node, the path is complete, and we can stop searching.
			if(currentNode == m_endNode)
			{
				m_foundPath = true;
				break;
			}

			// Look through all the node's neighbours for the node leading to the end node.
			for (int i = 0; i < currentNode.adjacentNodes.Count; ++i)
			{
				Node neighbourNode = currentNode.adjacentNodes[i];

				// Make sure the neghbour node is valid.
				if (neighbourNode == null)
				{
					continue;
				}

				if(neighbourNode.m_isOnMap == false)
				{
					continue;
				}

				if(m_closedList.Contains(neighbourNode) == true)
				{
					continue;
				}

				if (neighbourNode.m_isBlocked == true)
				{
					continue;
				}

				// If the neighbour node isn't in the open list, calculate it's variables and add it.
				if (m_openList.Contains(neighbourNode) == false)
				{
					neighbourNode.m_previousNode = currentNode;
					neighbourNode.gScore = currentNode.gScore + currentNode.m_costs[i];
					neighbourNode.hScore = CalculateHeristic(neighbourNode, m_endNode);
					neighbourNode.CalculateFScore();
					m_openList.Enqueue(neighbourNode);
				}
				// Neighbour node is in the open list, check if it's valid for the path.
				else
				{
					int costs = currentNode.fScore + currentNode.m_costs[i];
					if(costs < neighbourNode.fScore)
					{
						neighbourNode.gScore = currentNode.gScore + currentNode.m_costs[i];
						neighbourNode.fScore = neighbourNode.gScore + neighbourNode.hScore;
						neighbourNode.m_previousNode = currentNode;
					}
				}
			}
		}

		// If a path has been found, assign it to the path argument that was passed in.
		if (m_foundPath == true)
		{
			Node current = m_endNode;
			cost = Mathf.FloorToInt(current.gScore / 19) * 2 + (current.gScore % 19 / 10);
			while (current.m_previousNode != null)
			{
				path.Push(current);
				current = current.m_previousNode;
			}
			return true;
		}

		return false;
	}

	public bool FindAIPath(Vector3 startPos, Vector3 playerPos, ref Stack<Node> path, out int cost)
	{
		Node n;
		float previous = float.MaxValue;
		Vector3 endPos = Vector3.zero;
		for(int i = 0; i < 4; ++i)
		{
			n = GetNode(playerPos).adjacentNodes[i];
			float current = Vector3.Distance(n.worldPosition, startPos);
			if (current < previous)
			{
				endPos = n.worldPosition;
			}
			previous = current;
		}

		// The below is the same as FindPath(), so why not just call it?
		// Might have f'ed something up, but it works ¯\_(._.)_/¯
		// - James L

		bool result = FindPath(startPos, endPos, ref path, out cost);

		return result;
	}

	/// <summary>
	/// Get nodes within a given radius.
	/// </summary>
	/// <param name="radius">The radius to get nodes within.</param>
	/// <param name="startNode">The node in the centre of the radius.</param>
	/// <param name="allowBlockedNodes">If blocked nodes can be selected in the radius.</param>
	/// <returns>List of nodes within the given radius.</returns>
	public List<Node> GetNodesWithinRadius(int radius, Node startNode, bool allowBlockedNodes = false)
	{
		List<Node> nodesInRadius = new List<Node>();

		Queue<Node> openList = new Queue<Node>();
		List<Node> closedList = new List<Node>();

		startNode.gScore = 0;

		openList.Enqueue(startNode);

		// While there are nodes to search.
		while (openList.Count() > 0)
		{
			// Get the next node to search.
			Node currentNode = openList.Dequeue();

			if (currentNode.m_isOnMap == false)
				continue;
			if (currentNode != startNode && allowBlockedNodes == false && currentNode.m_isBlocked == true)
				continue;

			nodesInRadius.Add(currentNode);

			if (currentNode.gScore < radius)
			{
				// Go through and add the neighbours.
				for (int i = 0; i < 4; ++i)
				{
					Node neighbourNode = currentNode.adjacentNodes[i];
	
					if (neighbourNode == null)
						continue;
					if (closedList.Contains(neighbourNode) == true)
						continue;
					if (neighbourNode.m_isOnMap == false)
						continue;
	
					neighbourNode.gScore = currentNode.gScore + 1;
					openList.Enqueue(neighbourNode);
				}
				closedList.Add(currentNode);
			}
		}

		return nodesInRadius;
	}

	int CalculateHeristic(Node node, Node endNode)
	{
		int dx = Mathf.Abs(node.x - endNode.x);
		int dz = Mathf.Abs(node.z - endNode.z);

		return 19 * Mathf.Max(dx, dz) + 10 * Mathf.Abs(dx - dz);
	}
}