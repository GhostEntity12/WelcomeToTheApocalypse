using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillWithTargets
{
	public List<Unit> m_Targets;
	public BaseSkill m_Skill;
	public int m_Responses;

	public SkillWithTargets(List<Unit> targets, BaseSkill skill)
	{
		m_Targets = targets;
		m_Skill = skill;
		m_Responses = 0;
	}
}

[System.Serializable]
public class RangedColor
{
	[ColorUsage(true, true)]
	public Color m_InnerOrbColor = Color.black;
	public Color m_OuterOrbColor = Color.black;
	[ColorUsage(true, true)]
	public Color m_ParticleColor = Color.white;
}


public delegate void Notification();

public class ParticlesManager : MonoBehaviour
{
	public event Notification m_ListEmptied;

	public static ParticlesManager m_Instance = null;

	public SkillWithTargets m_ActiveSkill 
	{ 
		get
		{
			//Debug.Log($"Current value of m_ActiveSkill is {(_m_ActiveSkill == null ? "null" : _m_ActiveSkill.ToString())}, {(_m_ActiveSkill == null ? null : _m_ActiveSkill.m_Skill)}");

			/* 
			 * Some serious black magic is going on here. Grant, if you're reading this it's the
			 * weird null behaviour where setting the class as null set it as a new (null, null).
			 * Using a property to intercept the (null, null) class and just return the expected
			 * null instead.
			 */

			if (_m_ActiveSkill != null && _m_ActiveSkill.m_Skill == null && _m_ActiveSkill.m_Targets == null)
			{
				return null;
			}
			return _m_ActiveSkill;
		}
		set => _m_ActiveSkill = value;
	}

	private SkillWithTargets _m_ActiveSkill = new SkillWithTargets(null, null);

	//Zeroed
	[Header("Melee Particle")]
	//public int m_numberOfMelee;

	//public GameObject m_meleeParent;

	//public GameObject m_meleeParticle;

	public ParticleSystem m_melee;


	[Header("Heal Particle")]
	//public int m_numberOfHeal;

	//public GameObject m_healParent;

	//public GameObject m_healParticle;

	public ParticleSystem m_heal;


	[Header("Ranged Particle")]
	public RangedColor m_DefaultRanged;
	public RangedColor m_PestilenceRanged;
	public RangedColor m_FamineRanged;
	public RangedColor m_EnemyRanged;

	public float m_ZDistanceSpawn = 0.2f;

	public int m_RangedPoolSize = 2;

	public float m_rangedSpeed = 0.1f;

	public GameObject m_rangedParent;

	public GameObject m_rangedParticle;

	//instantiate, slerp
	private List<RangedParticle> m_rangedPool = new List<RangedParticle>();

	private List<RangedParticle> m_activeRangedParticle = new List<RangedParticle>();
	private int m_rangedIndex;



	/// <summary>
	/// Used to store the death particles
	/// </summary>
	//zeroed out
	[Header("Death Particle")]
	public int m_numberOfDeath;

	public GameObject m_deathParent;

	public GameObject m_deathParticle;

	private List<ParticleSystem> m_deathPool = new List<ParticleSystem>();

	private List<ParticleSystem> m_activeDeathParticle = new List<ParticleSystem>();

	private int m_deathIndex;
	/// <summary>
	/// Used to check which unit is killed and which particle is its
	/// </summary>
	[Tooltip("Keeps track of the units to check which one is killed. Add the units in the same order as m_unitParticles so they have the same index.")]
	private List<GameObject> m_units;

	/// <summary>
	/// The particles that are active while the unit is alive
	/// </summary>
	public List<ParticleSystem> m_unitParticles = new List<ParticleSystem>();


	private List<Vector3> m_endPosition = new List<Vector3>();

	private Vector3 m_unitPos = Vector3.zero;

	private GameObject shootFrom;

	private float travelTime;

	void Awake()
	{
		m_Instance = this;

		for (int i = 0; i < m_RangedPoolSize; ++i)
		{
			m_rangedPool.Add(Instantiate(m_rangedParticle, m_rangedParent.transform).GetComponent<RangedParticle>());
		}

		for (int i = 0; i < m_numberOfDeath; ++i)
		{
			m_deathPool.Add(Instantiate(m_deathParticle, m_deathParent.transform).GetComponent<ParticleSystem>());
		}

		//for (int i = 0; i < m_numberOfMelee; ++i)
		//{
		//	m_meleePool.Add(Instantiate(m_meleeParticle, m_meleeParent.transform).GetComponent<ParticleSystem>());
		//}

		//for (int i = 0; i < m_numberOfHeal; ++i)
		//{
		//	m_healPool.Add(Instantiate(m_healParticle, m_healParent.transform).GetComponent<ParticleSystem>());
		//}
	}

	private void Start()
	{ 
		m_ActiveSkill = null;
	}

	/// <summary>
	/// Move ranged particle to desired destination
	/// </summary>

	public void OnRanged(Unit caster, Vector3 targetPos, Unit targetUnit)
	{
		RangedParticle systemToUse = m_rangedPool[m_rangedIndex];
		switch (caster.m_CharacterName)
		{
			case "Famine":
				systemToUse.SetColor(m_FamineRanged);
				break;
			case "Pestilence":
				systemToUse.SetColor(m_PestilenceRanged);
				break;
			case "Ranged Enemy":
				systemToUse.SetColor(m_EnemyRanged);
				break;
			default:
				break;
		}
		systemToUse.m_Target = targetUnit;
		systemToUse.transform.position = caster.transform.position + Vector3.up + (caster.transform.forward * m_ZDistanceSpawn);
		systemToUse.transform.LookAt(targetPos);
		systemToUse.Play();
		m_activeRangedParticle.Add(systemToUse);
		m_endPosition.Add(targetPos);
		++m_rangedIndex;
		//activeParticle[0].transform.position = Vector3.MoveTowards(m_unitPos, m_endPosition[0], 5.0f);
	}

	public void OnMelee(Vector3 unitPos)
	{
		m_melee.transform.position = unitPos;
		m_melee.Play();
	}

	public void OnHeal(Vector3 targetPos)
	{
		m_heal.transform.position = targetPos;
		m_heal.Play();
	}

	public void OnDeath(Vector3 killedUnitPos)
	{
		if (m_deathIndex < m_deathPool.Count)
		{
			m_deathPool[m_deathIndex].transform.position = killedUnitPos;
			m_deathPool[m_deathIndex].Play();
			m_activeDeathParticle.Add(m_deathPool[m_deathIndex]);
			m_activeDeathParticle.Reverse();
			++m_deathIndex;
			//print("Pre: " + m_deathIndex);
		}
		for (int i = 0; i < m_unitParticles.Count; ++i)
		{
			if (m_unitParticles[i].transform.position == killedUnitPos)
			{
				m_unitParticles[i].Stop();
				//print("Unit: " + killedUnitPos);
				//print("Particle: " + m_unitParticles[i].transform.position);
				break;
			}
		}
	}

	void Update()
	{
		//Checks if there is an active ranged particle to start moving it
		//Stops all code within if statement from running when not needed
		if (m_activeRangedParticle.Count > 0)
		{

			//Goes through the active ranged partcles to move from an unit to another
			for (int i = 0; i < m_activeRangedParticle.Count; ++i)
			{
				RangedParticle particleToCheck = m_activeRangedParticle[i];
				//particleToCheck = Vector3.MoveTowards(particleToCheck, m_endPosition[i], m_rangedSpeed);
				particleToCheck.transform.position = Vector3.MoveTowards(particleToCheck.transform.position, m_endPosition[i], m_rangedSpeed);
				travelTime += Time.deltaTime;
				//Checks if the particle has reached its destination
				//Removes the particle from active and the endposition for the particle
				//Removes one from the ranged index
				if (particleToCheck.transform.position == m_endPosition[i])
				{
					//m_activeRangedParticle[i].time = m_activeRangedParticle[i].main.duration;

					m_activeRangedParticle.Remove(particleToCheck);
					m_endPosition.Remove(m_endPosition[i]);

					particleToCheck.Stop();

					particleToCheck.transform.position = m_rangedParent.transform.position;

					if (m_rangedIndex <= 0)
					{
						m_rangedIndex = 0;
					}
					else
					{
						--m_rangedIndex;
					}
					travelTime = 0.0f;
				}
			}
		}

		//Checks if it still emitting particles before removing and resetting the index
		for (int i = 0; i < m_activeDeathParticle.Count; ++i)
		{
			if (!m_activeDeathParticle[i].isEmitting)
			{
				m_activeDeathParticle.Remove(m_activeDeathParticle[i]);
				if (m_deathIndex >= 5)
				{
					m_deathIndex = 0;
				}
				//print("Post: " + m_deathIndex);
			}
		}

		////Used to send particles around between units for testing
		//Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		//RaycastHit raycastHit;
		//if (Physics.Raycast(ray, out raycastHit))
		//{
		//	if(Input.GetMouseButtonDown(0))
		//	{
		//		OnDeath(raycastHit.collider.gameObject.transform.position);
		//		//if (m_unitPos == Vector3.zero)
		//		//{
		//		//	m_unitPos = raycastHit.collider.gameObject.transform.position;
		//		//}
		//		//else
		//		//{
		//		//	RangedAttack(m_unitPos, raycastHit.collider.gameObject.transform.position);
		//		//	m_unitPos = Vector3.zero;
		//		//}
		//	}
		//}
	}

	public void TakeSkillEffects()
	{
		foreach (Unit affectedUnit in m_ActiveSkill.m_Targets)
		{
			affectedUnit.IncomingSkill(m_ActiveSkill.m_Skill);
		}
	}

	public void RemoveUnitFromTarget()
	{
		m_ActiveSkill.m_Responses++;
		if (m_ActiveSkill.m_Targets.Count == m_ActiveSkill.m_Responses)
		{
			if (m_ActiveSkill.m_Skill is DamageSkill)
			{
				(m_ActiveSkill.m_Skill as DamageSkill).m_ExtraDamage = 0;
			}
			m_ActiveSkill = null;
			ListEmptied();
		}
	}

	protected virtual void ListEmptied() => m_ListEmptied?.Invoke();

}
