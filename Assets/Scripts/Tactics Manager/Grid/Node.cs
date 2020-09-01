using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Node
{
	private float m_movement;
	private float m_damage;
	private float m_kill;
	private float m_healing;

	public int[] m_costs = new int[8];
	public int x, z;
	public int fScore, gScore, hScore;

	public bool m_isOnMap;

	public bool m_isBlocked;

	public Vector3 worldPosition;

	public GridObject obstacle;

	public Unit unit;

	public GameObject m_tile;

	public Node m_previousNode;

	[Header("Breadth First Search")]
	public bool visited = false;
	public Node parentNode = null;
	public int distance = 0;

	public List<Node> adjacentNodes = new List<Node>();

	public NodeHighlight m_NodeHighlight;

	public void Reset()
	{
		m_NodeHighlight.ChangeHighlight(TileState.None);
		visited = false;
		parentNode = null;
		distance = 0;
	}

	public void CalculateFScore()
	{
		fScore = gScore + hScore;
	}

	public float GetMinMax()
	{
		return m_movement + m_damage + m_kill + m_healing;
	}


	public void SetDamage(float damage)
	{
		m_damage = damage;
	}

	public float GetDamage()
	{
		return m_damage;
	}


	public void SetKill(float kill)
	{
		m_kill = kill;
	}

	public float GetKill()
	{
		return m_kill;
	}


	public void SetHealing(float healing)
	{
		m_healing = healing;
	}

	public float GetHealing()
	{
		return m_healing;
	}


	public void SetMovement(float movement)
	{
		m_movement = movement;
	}

	public float GetMovement()
	{
		return m_movement;
	}
}
