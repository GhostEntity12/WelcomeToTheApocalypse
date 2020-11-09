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
}
