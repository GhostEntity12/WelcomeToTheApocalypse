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

	public Material[] materials;

	public bool m_isTargetable;
	
	public bool m_isAffected;

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
		m_Renderer.material = materials[(int)state];
	}

	private void Update()
	{
		if (m_isAffected)
		{
			ChangeHighlight(TileState.EffectRange);
		}
		else if (m_isTargetable)
		{
			ChangeHighlight(TileState.TargetRange);
		}
	}
}
