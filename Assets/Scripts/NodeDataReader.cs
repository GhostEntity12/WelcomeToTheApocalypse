using System;
using System.Linq;
using System.Reflection;
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

			//Debug.LogWarning(
			//	$"Position: {n.x}/{n.z}\n" +
			//	$"Unit: {n.unit}\n" +
			//	$"Is Walkable: {n.isOnMap}\n" +
			//	$"Is Blocked: {n.isBlocked}\n" +
			//	$"World Position: {n.worldPosition}\n" +
			//	$"Node Highlight: {n.m_NodeHighlight.name}");

			FieldInfo[] fieldInfos = typeof(Node).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			string output = "======" + n.m_NodeHighlight.name + "======\n";

			foreach (var item in fieldInfos)
			{
				try
				{
					output += $"{item.Name}: {item.GetValue(n)}\n";
				}
				catch (ArgumentException)
				{
					output += $"{item.Name}: unobtainable\n";
				}

			}

			Debug.Log(output);
		}
	}
}
