using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeDataReader : MonoBehaviour
{
	[ContextMenu("Read")]
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
