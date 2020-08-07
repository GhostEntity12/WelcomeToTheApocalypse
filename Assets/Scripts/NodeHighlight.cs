using System.Collections;
using System.Collections.Generic;
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
}
