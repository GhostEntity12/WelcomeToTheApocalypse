using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GridObject))]
public class Grid : MonoBehaviour
{
	public static Grid m_Instance = null;
	
	Node[,] m_grid;
	[SerializeField]
	float xzScale = 1f;
	[SerializeField]
	bool tileActive = false;

	bool searched;

	Vector3 minPosition;

	public Vector3 extends = new Vector3(1f, 1f, 1f);

	int maxX, maxZ;
	int posX, posZ;

	List<Vector3> nodeViz = new List<Vector3>();

	[Tooltip("Used to show the node position with something like a really small plane or something")]
	public GameObject node;
	[Tooltip("Used to store the objects for the nodes")]
	public GameObject nodeArray;

	//Will be deleted being used for testing
	public List<GameObject> unit;

	// DEBUG
	List<Node> nodething = new List<Node>();

	void Awake()
	{
		m_Instance = this;
		ReadLevel();
	}

	void Start()
	{
		
	}

	void ReadLevel()
	{
		////Finds GridPosition script on objects
		//GridPosition[] gp = FindObjectsOfType<GridPosition>();

		////Defaults the 
		//float minX = float.MaxValue;
		//float maxX = float.MinValue;

		//float minZ = minX;
		//float maxZ = maxX;

		//for (int i = 0; i < gp.Length; i++)
		//{
		//	Transform t = gp[i].transform;
		//	//Sets the min position if the position of the gridPosition is less than the MinX which is set to the highest value
		//	if (t.position.x < minX)
		//	{
		//		minX = t.position.x;
		//	}
		//	//Sets the max position if the position of the gridPosition is less than the MaxX which is set to the lowest value
		//	if (t.position.x > maxX)
		//	{
		//		maxX = t.position.x;
		//	}
		//	if (t.position.z < minZ)
		//	{
		//		minZ = t.position.z;
		//	}
		//	if (t.position.z > maxZ)
		//	{
		//		maxZ = t.position.z;
		//	}
		//}

		//posX = Mathf.FloorToInt((maxX - minX) / xzScale);
		//posZ = Mathf.FloorToInt((maxZ - minZ) / xzScale);

		//minPosition = Vector3.zero;
		//minPosition.x = minX;
		//minPosition.z = minZ;

		//CreateGrid(posX, posZ);

		Bounds bounds = GetComponent<Collider>().bounds;

		posX = Mathf.FloorToInt(bounds.size.x / xzScale);
		posZ = Mathf.FloorToInt(bounds.size.z / xzScale);

		minPosition = new Vector3(bounds.min.x, 0, bounds.min.z);
		CreateGrid(posX, posZ);
	}

	//Creates the grid
	void CreateGrid(int a_posX, int a_posZ)
	{

		m_grid = new Node[a_posX, a_posZ];

		for (int x = 0; x < a_posX; ++x)
		{

			for (int z = 0; z < a_posZ; ++z)
			{
				//A new node and sets it's X and Z
				Node n = new Node();
				n.x = x;
				n.z = z;

				Vector3 tp = minPosition;

				tp.x += x * xzScale + 0.5f;
				tp.z += z * xzScale + 0.5f;

				n.worldPosition = tp;

				Collider[] overlapNode = Physics.OverlapBox(tp, extends / 2, Quaternion.identity);

				if (overlapNode.Length > 0)
				{

					bool isWalkable = false;

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

					n.isWalkable = isWalkable;
				}

				if (n.isWalkable)
				{
					n.m_tile = Instantiate(node, new Vector3(n.worldPosition.x, n.worldPosition.y + 0.01f, n.worldPosition.z), Quaternion.identity, nodeArray.transform);
					n.m_NodeHighlight = n.m_tile.GetComponent<NodeHighlight>();
					n.m_NodeHighlight.ChangeHighlight(TileState.None);
				}
				if (n.obstacle != null)
				{
					nodeViz.Add(n.worldPosition);
				}
				m_grid[x, z] = n;

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
					//print("set 0");
					m_grid[x, z].adjacentNodes.Add(m_grid[x, z - 1]);
				}
				else
				{
					m_grid[x, z].adjacentNodes.Add(null);
				}

				//1 West
				if (x > 0)
				{
					//print("set 1");
					m_grid[x, z].adjacentNodes.Add(m_grid[x - 1, z]);
				}
				else
				{
					m_grid[x, z].adjacentNodes.Add(null);
				}

				//2 North
				if (z < posZ - 1)
				{
					//print("set 2");
					m_grid[x, z].adjacentNodes.Add(m_grid[x, z + 1]);
				}
				else
				{
					m_grid[x, z].adjacentNodes.Add(null);
				}

				//3 East
				if (x < posX - 1)
				{
					//print("set 3");
					m_grid[x, z].adjacentNodes.Add(m_grid[x + 1, z]);
				}
				else
				{
					m_grid[x, z].adjacentNodes.Add(null);
				}

				//4 South West
				if (z > 0 && x > 0)
				{
					m_grid[x, z].adjacentNodes.Add(m_grid[x - 1, z - 1]);
				}
				else
				{
					m_grid[x, z].adjacentNodes.Add(null);
				}

				//5 North West
				if (x > 0 && z < posZ - 1)
				{
					m_grid[x, z].adjacentNodes.Add(m_grid[x - 1, z + 1]);
				}
				else
				{
					m_grid[x, z].adjacentNodes.Add(null);
				}

				//6 North East
				if (z < posZ - 1 && x < posX - 1)
				{
					m_grid[x, z].adjacentNodes.Add(m_grid[x + 1, z + 1]);
				}
				else
				{
					m_grid[x, z].adjacentNodes.Add(null);
				}

				//7 South East
				if (z > 0 && x < posX - 1)
				{
					m_grid[x, z].adjacentNodes.Add(m_grid[x + 1, z - 1]);
				}
				else
				{
					m_grid[x, z].adjacentNodes.Add(null);
				}

				m_grid[x, z].m_costs[0] = 10;
				m_grid[x, z].m_costs[1] = 10;
				m_grid[x, z].m_costs[2] = 10;
				m_grid[x, z].m_costs[3] = 10;
				m_grid[x, z].m_costs[4] = 19;
				m_grid[x, z].m_costs[5] = 19;
				m_grid[x, z].m_costs[6] = 19;
				m_grid[x, z].m_costs[7] = 19;
			}
		}
	}

	public Node GetNode(Vector3 wp)
	{
		Vector3 p = wp - minPosition;
		int x = Mathf.FloorToInt(p.x / xzScale);
		//print("X: " + x);
		int z = Mathf.FloorToInt(p.z / xzScale);
		//print("Z: " + z);

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

		//print("Unit Node Pos: " + m_grid[a_x - 1, a_z].worldPosition);
		return m_grid[a_x, a_z];
	}
	
	public void SetUnit(GameObject unit)
	{
		Node n = GetNode(unit.transform.position);
		n.unit = unit.GetComponent<Unit>();
	}

	public Unit GetUnit(Vector3 mousePos)
	{
		return GetNode(mousePos).unit;
	}

	public void RemoveUnit(Node unitNode)
	{
		unitNode.unit = null;
	}

	public void ClearNode()
	{
		foreach (Node node in nodething)
		{
			node.m_NodeHighlight.ChangeHighlight(TileState.None);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		for(int i = 0; i < nodeViz.Count; ++i)
		{
			Gizmos.DrawWireCube(nodeViz[i], extends);
		}
	}

	public bool FindPath(Vector3 startPos, Vector3 endPos, ref Stack<Node> path, out int cost)
	{

		Node m_startNode = GetNode(startPos);
		Node m_endNode = GetNode(endPos);

		if (m_startNode == null || m_endNode == null)
		{
			return false;
		}

		if (m_startNode == m_endNode)
		{
			return false;
		}

		if(m_startNode.isWalkable == false || m_endNode.isWalkable == false)
		{
			return false;
		}

		path.Clear();
		Queue<Node> m_openList = new Queue<Node>();
		List<Node> m_closedList = new List<Node>();

		bool m_foundPath = false;

		m_startNode.m_previousNode = null;
		m_startNode.gScore = 0;
		m_startNode.hScore = CalculateHeristic(m_startNode, m_endNode);
		m_startNode.CalculateFScore();

		m_openList.Enqueue(m_startNode);

		while (m_openList.Count > 0)
		{
			Node currentNode = m_openList.Dequeue();

			m_closedList.Add(currentNode);

			if(currentNode == m_endNode)
			{
				m_foundPath = true;
				break;
			}

			for (int i = 0; i < currentNode.adjacentNodes.Count; ++i)
			{
				Node neighbourNode = currentNode.adjacentNodes[i];

				if (neighbourNode == null)
				{
					continue;
				}

				if(neighbourNode.isWalkable == false)
				{
					continue;
				}

				if(m_closedList.Contains(neighbourNode) == true)
				{
					continue;
				}

				if(m_openList.Contains(neighbourNode) == false)
				{
					neighbourNode.m_previousNode = currentNode;
					neighbourNode.gScore = currentNode.gScore + currentNode.m_costs[i];
					neighbourNode.hScore = CalculateHeristic(neighbourNode, m_endNode);
					neighbourNode.CalculateFScore();
					m_openList.Enqueue(neighbourNode);
				}
				else
				{
					int cost = currentNode.fScore + currentNode.m_costs[i];
					if(cost < neighbourNode.fScore)
					{
						neighbourNode.gScore = currentNode.gScore + currentNode.m_costs[i];
						neighbourNode.fScore = neighbourNode.gScore + neighbourNode.hScore;
						neighbourNode.m_previousNode = currentNode;
					}
				}
			}
		}


		if (m_foundPath == true)
		{
			Node current = m_endNode;
			while (current != null)
			{
				path.Push(current);
				current = current.m_previousNode;
			}
		}

		m_foundPath = false;

		//Costs
		m_startNode = GetNode(startPos);
		m_endNode = GetNode(endPos);

		m_openList.Clear();
		m_closedList.Clear();

		m_startNode.m_previousNode = null;
		m_startNode.gScore = 0;
		m_startNode.hScore = CalculateHeristic(m_startNode, m_endNode);
		m_startNode.CalculateFScore();

		m_openList.Enqueue(m_startNode);

		//Get Costs
		while (m_openList.Count > 0)
		{
			Node currentNode = m_openList.Dequeue();

			m_closedList.Add(currentNode);

			if (currentNode == m_endNode)
			{
				m_foundPath = true;
				break;
			}

			for (int i = 0; i < 4; ++i)
			{
				Node neighbourNode = currentNode.adjacentNodes[i];

				if (neighbourNode == null)
				{
					continue;
				}

				if (neighbourNode.isWalkable == false)
				{
					continue;
				}

				if (m_closedList.Contains(neighbourNode) == true)
				{
					continue;
				}

				if (m_openList.Contains(neighbourNode) == false)
				{
					neighbourNode.m_previousNode = currentNode;
					neighbourNode.gScore = currentNode.gScore + currentNode.m_costs[i];
					neighbourNode.hScore = CalculateHeristic(neighbourNode, m_endNode);
					neighbourNode.CalculateFScore();
					m_openList.Enqueue(neighbourNode);
				}
				else
				{
					int cost = currentNode.fScore + currentNode.m_costs[i];
					if (cost < neighbourNode.fScore)
					{
						neighbourNode.gScore = currentNode.gScore + currentNode.m_costs[i];
						neighbourNode.fScore = neighbourNode.gScore + neighbourNode.hScore;
						neighbourNode.m_previousNode = currentNode;
					}
				}
			}
		}

		if (m_foundPath == true)
		{
			Node current = m_endNode;
			int pathLength = 0;
			while (current != null)
			{
				current = current.m_previousNode;
				++costs;
			}
			cost = costs;
			return true;
		}

		return false;
	}

	int CalculateHeristic(Node node, Node endNode)
	{
		int dx = Mathf.Abs(node.x - endNode.x);
		int dz = Mathf.Abs(node.z - endNode.z);

		if (dx < dz)
		{
			return ((19 * dz) + 10 * (dx - dz));
		}
		else
		{
			return ((19 * dx) + 10 * (dz - dx));
		}
	}

	void Update()
	{
		//if (!searched)
		//{
		//	foreach (GameObject go in unit)
		//	{
		//		GetArea(4, go);
		//	}
		//	searched = true;
		//}
	}

	//Will be Deleted
	[ContextMenu("Test")]
	void Test()
	{
		foreach (GameObject go in unit)
		{
			print(go.name + " Node Pos: " + GetNode(go.transform.position).worldPosition);
			print(go.name + " Pos: " + go.transform.position);
		}
	}
	/*Multple Floor stuff will be deleted at a later date*/
}