using System.Linq;
using UnityEngine;

public class NodeDataReader : MonoBehaviour
{
	/// <summary>
	/// Reads the data of the node at the transform
	/// </summary>
	[ContextMenu("Read from position")]
	void ReadData()
	{
		Grid g = FindObjectOfType<Grid>();

		if (g)
		{
			Node n = g.GetNode(transform.position);

			Debug.Log(
				$"Position: {n.x}/{n.z}\n" +
				$"Unit: {n.unit}\n" +
				$"Is Walkable: {n.isWalkable}\n" +
				$"World Position: {n.worldPosition}\n" +
				$"Node Highlight: {n.m_NodeHighlight.name}\n" +
				$"Neighbors: {string.Join(", ", n.adjacentNodes.Select(a => a.m_NodeHighlight))}");
		}
	}
}
