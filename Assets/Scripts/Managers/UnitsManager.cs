using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitsManager : MonoBehaviour
{
	public static UnitsManager m_Instance;

	public Unit[] m_AllUnits;
	public List<Unit> m_PlayerUnits = new List<Unit>();
	public List<Unit> m_EnemyUnits = new List<Unit>();
	public List<Unit> m_ActiveEnemyUnits = new List<Unit>();

	private void Awake()
	{
		m_Instance = this;

		m_AllUnits = FindObjectsOfType<Unit>();

		m_PlayerUnits = m_AllUnits.Where(u => u.GetAllegiance() == Allegiance.Player).ToList();
		m_EnemyUnits = m_AllUnits.Where(u => u.GetAllegiance() == Allegiance.Enemy).ToList();
	}
}
