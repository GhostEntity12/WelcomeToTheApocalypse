using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
	
	Node[,,] m_grid;
	[SerializeField]
	float xzScale = 1f;
	[SerializeField]
	float yScale = 1f;
	[SerializeField]
	bool tileActive = false;

	bool searched;

	Vector3 minPosition;

	public Vector3 extends = new Vector3(1f, 1f, 1f);

	int maxX, maxZ, maxY;
	int posX, posY, posZ;

	List<Vector3> nodeViz = new List<Vector3>();

	[Tooltip("Used to show the node position with something like a really small plane or something")]
	public GameObject node;
	[Tooltip("Used to store the objects for the nodes")]
	public GameObject nodeArray;

	//Will be deleted being used for testing
	public List<GameObject> unit;
	void Start()
	{
		ReadLevel();
	}

	void ReadLevel()
	{
		//Finds GridPosition script on objects
		GridPosition[] gp = FindObjectsOfType<GridPosition>();

		//Defaults the 
		float minX = float.MaxValue;
		float maxX = float.MinValue;

		#region Multiple floors
		float minY = minX;
		float maxY = maxX;
		#endregion

		float minZ = minX;
		float maxZ = maxX;

		for (int i = 0; i < gp.Length; i++)
		{
			Transform t = gp[i].transform;
			//Sets the min position if the position of the gridPosition is less than the MinX which is set to the highest value
			if(t.position.x < minX)
			{
				minX = t.position.x;
			}
			//Sets the max position if the position of the gridPosition is less than the MaxX which is set to the lowest value
			if (t.position.x > maxX)
			{
				maxX = t.position.x;
			}
			if (t.position.z < minZ)
			{
				minZ = t.position.z;
			}
			if (t.position.z > maxZ)
			{
				maxZ = t.position.z;
			}

			#region Multiple floors
			if (t.position.y < minY)
			{
				minY = t.position.y;
			}
			if (t.position.y > maxY)
			{
				maxY = t.position.y;
			}
			#endregion
		}

		posX = Mathf.FloorToInt((maxX - minX) / xzScale);
		posZ = Mathf.FloorToInt((maxZ - minZ) / xzScale);

		#region Multiple floors
		posY = Mathf.FloorToInt((maxY - minY) / yScale);
		#endregion

		#region Multiple floors
		if(posY == 0)
		{
			posY = 1;
		}
		#endregion

		minPosition = Vector3.zero;
		minPosition.x = minX;
		minPosition.z = minZ;

		#region Multiple floors
		minPosition.y = minY;
		#endregion

		CreateGrid(posX, posZ, posY);
	}

	//Creates the grid
	void CreateGrid(int a_posX, int a_posZ, int a_posY)
	{

		m_grid = new Node[a_posX, a_posY, a_posZ];

		#region Multiple floors
		for (int y = 0; y < a_posY; y++)
		{
			#endregion
			for (int x = 0; x < a_posX; ++x)
			{

				for (int z = 0; z < a_posZ; ++z)
				{
					//A new node and sets it's X and Z
					Node n = new Node();
					n.x = x;
					n.z = z;

					#region Multiple floors
					n.y = y;
					#endregion

					Vector3 tp = minPosition;

					tp.x += x * xzScale + 0.5f;
					tp.z += z * xzScale + 0.5f;

					#region Multiple floors
					tp.y += y * yScale;// + 0.5f;
					#endregion

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
						node.transform.position = new Vector3(n.worldPosition.x, n.worldPosition.y + 0.01f, n.worldPosition.z);
						n.tile = Instantiate(node, node.transform.position, node.transform.rotation, nodeArray.transform);
						n.tile.SetActive(tileActive);
						RaycastHit hit;
						Vector3 origin = n.worldPosition;
						origin.y += yScale - 0.1f;
						if (Physics.Raycast(origin, Vector3.down, out hit, yScale - .1f))
						{
							n.worldPosition = hit.point;
						}
					}
					if (n.obstacle != null)
					{
						nodeViz.Add(n.worldPosition);
					}
					m_grid[x, y, z] = n;

				}

			}

		}
		node.SetActive(false);
		SetNodeNeighbours();
	}

	void SetNodeNeighbours()
	{
		for (int x = 0; x < 60; ++x)
		{
			for (int z = 0; z < 60; ++z)
			{
				if (z > 0)
				{
					print("set 0");
					m_grid[x, 0, z].adjacentNodes.Add(m_grid[x, 0, z - 1]);
				}
				//1 West
				if (x > 0)
				{
					print("set 1");
					m_grid[x, 0, z].adjacentNodes.Add(m_grid[x - 1, 0, z]);
				}
				//2 North
				if (z < 60 - 1)
				{
					print("set 2");
					m_grid[x, 0, z].adjacentNodes.Add(m_grid[x, 0, z + 1]);
				}
				//3 East
				if (x < 60 - 1)
				{
					print("set 3");
					m_grid[x, 0, z].adjacentNodes.Add(m_grid[x + 1, 0, z]);
				}
				////4 South West
				//if (z > 0 && x > 0)
				//{
				//	m_aapNodeList[x][z]->m_apNeighbours[4] = m_aapNodeList[x - 1][z - 1];
				//}
				////5 North West
				//if (x > 0 && z < m_nHeight - 1)
				//{
				//	m_aapNodeList[x][z]->m_apNeighbours[5] = m_aapNodeList[x - 1][z + 1];
				//}
				////6 North East
				//if (z < m_nHeight - 1 && x < m_nWidth - 1)
				//{
				//	m_aapNodeList[x][z]->m_apNeighbours[6] = m_aapNodeList[x + 1][z + 1];
				//}
				////7 South East
				//if (z > 0 && x < m_nWidth - 1)
				//{
				//	m_aapNodeList[x][z]->m_apNeighbours[7] = m_aapNodeList[x + 1][z - 1];
				//}

				//m_aapNodeList[x][z]->m_anCosts[0] = 10;
				//m_aapNodeList[x][z]->m_anCosts[1] = 10;
				//m_aapNodeList[x][z]->m_anCosts[2] = 10;
				//m_aapNodeList[x][z]->m_anCosts[3] = 10;
				////m_aapNodeList[x][y]->m_anCosts[4] = 14;
				////m_aapNodeList[x][y]->m_anCosts[5] = 14;
				////m_aapNodeList[x][y]->m_anCosts[6] = 14;
				////m_aapNodeList[x][y]->m_anCosts[7] = 14;
				//if (m_aapNodeList[x][z]->m_bFruit == true)
				//{
				//	m_v2Fruit = m_aapNodeList[x][z]->m_v2Position;
				//}
			}
		}
	}

	public Node GetNode(Vector3 wp)
	{
		Vector3 p = wp - minPosition;
		int x = Mathf.FloorToInt(p.x / xzScale);
		print("X: " + x);
		#region Multiple floors
		int y = Mathf.FloorToInt(p.y / yScale);
		print("Y: " + y);
		#endregion
		int z = Mathf.FloorToInt(p.z / xzScale);
		print("Z: " + z);

		return GetNode(x, y, z);
	}

	public Node GetNode(int a_x, int a_y, int a_z)
	{
		//Checks if the object is out of bounds it returns null
		if(a_x < 0 || a_x > posX - 1 ||
			a_y < 0 || a_y > posY - 1 ||
			a_z < 0 || a_z > posZ - 1 )
		{
			return null;
		}

		print("Unit Node Pos: " + m_grid[a_x - 1, a_y, a_z].worldPosition);
		return m_grid[a_x, a_y, a_z];
	}
	
	public void SetUnit(GameObject unit)
	{
		Node n = GetNode(unit.transform.position);
		n.unit = unit;
	}

	public void GetArea(int radius, GameObject gameObject)
	{

		foreach (Node node in Ghost.BFS.GetNodesWithinRadius(radius, GetNode(gameObject.transform.position)))
		{
			node.tile.SetActive(true);
		}
		GetNode(gameObject.transform.position).tile.SetActive(false);
	}
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		for(int i = 0; i < nodeViz.Count; ++i)
		{
			Gizmos.DrawWireCube(nodeViz[i], extends);
		}
	}

	void Update()
	{
		if (!searched)
		{
			foreach (GameObject go in unit)
			{
				GetArea(4, go);
			}
			searched = true;
		}
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