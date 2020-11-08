using System.Collections.Generic;
using UnityEngine;

public class NodeDistanceEfficiencyCheck : MonoBehaviour
{

	[ContextMenu("Test1")]
	float Test1()
	{
		float startTime = Time.realtimeSinceStartup;
		foreach (Unit aiUnit in UnitsManager.m_Instance.m_ActiveEnemyUnits)
		{
			Node startNode = Grid.m_Instance.GetNode(aiUnit.transform.position);
			foreach (Node node in Grid.m_Instance.GetNodesWithinRadius(aiUnit.GetCurrentMovement(), startNode))
			{
				int distance = Mathf.Abs(startNode.x - node.x) + Mathf.Abs(startNode.z - node.z);
			}
		}
		float duration = Time.realtimeSinceStartup - startTime;
		return duration;
	}

	[ContextMenu("Test2")]
	float Test2()
	{
		float startTime = Time.realtimeSinceStartup;
		foreach (Unit aiUnit in UnitsManager.m_Instance.m_ActiveEnemyUnits)
		{
			Node startNode = Grid.m_Instance.GetNode(aiUnit.transform.position);
			foreach (Node node in Grid.m_Instance.GetNodesWithinRadius(aiUnit.GetCurrentMovement(), startNode))
			{
				if (Grid.m_Instance.FindPath(startNode, node, out Stack<Node> path, out int cost, allowBlocked: true))
				{
					int distance = path.Count;
				}
				else
				{
					Debug.LogWarning("Couldn't find a path for a character");
				}
			}
		}

		float duration = Time.realtimeSinceStartup - startTime;
		return duration;
	}

	[ContextMenu("Compare")]
	void Compare()
	{
		float totalTime = 0;
		float total1 = 0;
		float total2 = 0;
		for (int i = 0; i < 100; i++)
		{
			float t1 = Test1();
			float t2 = Test2();

			total1 += t1;
			total2 += t2;
			totalTime += t1 / t2 * 100;
		}
		Debug.Log($"Pure maths (average: {total1 / 100}) took {totalTime / 100}% of the time of pathfinding ((average: {total2 / 100})for {UnitsManager.m_Instance.m_ActiveEnemyUnits.Count} units");
	}
}
