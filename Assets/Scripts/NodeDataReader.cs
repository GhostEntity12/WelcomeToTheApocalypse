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
				$"World Position: {n.worldPosition}");
		}
	}
}
