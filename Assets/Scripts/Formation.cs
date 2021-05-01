using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation : MonoBehaviour
{
	[SerializeField]
	private Transform m_DeathPosition = null;

	[SerializeField]
	private Transform m_PestilencePosition = null;

	[SerializeField]
	private Transform m_FaminePosition = null;

	[SerializeField]
	private Transform m_WarPosition = null;

	public Transform GetDeathPosition()
	{
		return m_DeathPosition;
	}
	public Transform GetPestilencePosition()
	{
		return m_PestilencePosition;
	}
	public Transform GetFaminePosition()
	{
		return m_FaminePosition;
	}
	public Transform GetWarPosition()
	{
		return m_WarPosition;
	}
}
