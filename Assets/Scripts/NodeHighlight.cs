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
	private Renderer m_Renderer;

	public Material[] m_Highlights;

	public bool m_IsTargetable;
	
	public bool m_IsAffected;

	public bool m_IsInTargetArea;

	private void Awake()
	{
		m_Renderer = GetComponent<Renderer>();
	}

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
