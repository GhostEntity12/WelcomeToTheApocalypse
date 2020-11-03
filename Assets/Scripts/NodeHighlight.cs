using System;
using System.Reflection;
using UnityEngine;

public enum TileState
{
	MovementRange,
	TargetRange,
	EffectRange,
	None
}

public class NodeHighlight : MonoBehaviour
{
	/// <summary>
	/// The renderer of the highlight mesh
	/// </summary>
	private Renderer m_Renderer;

	/// <summary>
	/// The materials that represent movement, targeting and affected area
	/// </summary>
	public Material[] m_Highlights;

	/// <summary>
	/// Whether the node is targetable
	/// </summary>
	public bool m_IsTargetable;

	/// <summary>
	/// Whether the node is in the selected skill's area of effect
	/// </summary>
	public bool m_IsAffected;

	/// <summary>
	/// Whether the node is in the possible targeting space
	/// </summary>
	public bool m_IsInTargetArea;

	private void Awake()
	{
		m_Renderer = GetComponent<Renderer>();
	}

	/// <summary>
	/// Changes the material of the highlight
	/// </summary>
	/// <param name="state">What state the highlight should be set to</param>
	public void ChangeHighlight(TileState state)
	{
		if (!m_Renderer)
		{
			m_Renderer = GetComponent<Renderer>();
		}

		if (state == TileState.None)
		{
			m_Renderer.enabled = false;
			return;
		}
		m_Renderer.enabled = true;
		m_Renderer.material = m_Highlights[(int)state];
	}

	private void Update()
	{
		if (m_IsInTargetArea)
		{
			if (m_IsAffected)
			{
				ChangeHighlight(TileState.EffectRange);
			}
			else if (m_IsTargetable)
			{
				ChangeHighlight(TileState.TargetRange);
			}
			else
			{
				ChangeHighlight(TileState.None);
			}
		}
		else if (m_IsAffected || m_IsTargetable)
		{
			m_IsAffected = false;
			m_IsTargetable = false;
		}
	}

	[ContextMenu("Read Node Data")]
	void ReadData()
	{
		Grid g = FindObjectOfType<Grid>();

		if (g)
		{
			Node n = g.GetNode(transform.position);

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

			Debug.Log(output, n.m_NodeHighlight);
		}
	}

	[ContextMenu("Print heuristic value")]
	void phv()
	{
		if (GameManager.m_Instance.GetCurrentTurn() == Allegiance.Enemy)
		{
			if (GameManager.m_Instance.GetSelectedUnit() == null)
			{
				Debug.LogError("You need to select a unit in order to get this tile's heuristics!");
			}
			else
			{
				Debug.Log(AIManager.m_Instance.FindHeuristic(Grid.m_Instance.GetNode(transform.position), GameManager.m_Instance.GetSelectedUnit()).SumHeuristics());
			}
		}
		else
		{
			Debug.LogWarning("Can't get the heuristic value when it's the player turn!");
		}
	}
}
