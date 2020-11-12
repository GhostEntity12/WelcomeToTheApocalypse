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
	public List<Unit> m_DeadPlayerUnits = new List<Unit>();

	private void Awake()
	{
		m_Instance = this;

		m_AllUnits = FindObjectsOfType<Unit>();

		m_PlayerUnits = m_AllUnits.Where(u => u.GetAllegiance() == Allegiance.Player).ToList();
		m_EnemyUnits = m_AllUnits.Where(u => u.GetAllegiance() == Allegiance.Enemy).ToList();
	}

	private void Start()
	{
		foreach (Unit playerUnit in m_PlayerUnits)
		{
			Grid.m_Instance.SetUnitInitial(playerUnit);
			if (MusicManager.m_Instance)
			{
				switch (playerUnit.m_CharacterName)
				{
					case "Death":
						MusicManager.m_Instance.AddHorseman(Horseman.Death);
						break;
					case "Pestilence":
						MusicManager.m_Instance.AddHorseman(Horseman.Pestilence);
						break;
					case "Famine":
						MusicManager.m_Instance.AddHorseman(Horseman.Pestilence);
						break;
					case "War":
						MusicManager.m_Instance.AddHorseman(Horseman.War);
						break;
					default:
						break;
				}
			}
		}
	}
}
