using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
	public static ParticlesManager m_Instance = null;

	private int m_rangedIndex;

	public float m_rangedSpeed = 0.1f;


	public List<ParticleSystem> m_rangedPool;

	public List<GameObject> m_damagePool;

	public List<GameObject> m_deathPool;

	public List<ParticleSystem> m_activeRangedParticle;

	public List<Vector3> m_endPosition;

	private Vector3 m_unitPos = Vector3.zero;

	private GameObject shootFrom;

	private float travelTime;
	
	void Awake()
	{
		m_Instance = this;

		for(int i = 0; i < m_rangedPool.Count; ++i)
		{
			m_rangedPool[i].Stop();
		}

		m_activeRangedParticle = new List<ParticleSystem>();
		m_endPosition = new List<Vector3>();
	}

	/// <summary>
	/// Move ranged particle to desired destination
	/// </summary>

	public void RangedParticle(Vector3 unitPos, Vector3 endPos)
	{
		m_rangedPool[m_rangedIndex].transform.position = unitPos;
		m_rangedPool[m_rangedIndex].Play();
		m_activeRangedParticle.Add(m_rangedPool[m_rangedIndex]);
		m_endPosition.Add(endPos);
		print(Vector3.Distance(unitPos, endPos));
		++m_rangedIndex;
		//activeParticle[0].transform.position = Vector3.MoveTowards(m_unitPos, m_endPosition[0], 5.0f);
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
				m_activeRangedParticle[i].transform.position = Vector3.MoveTowards(m_activeRangedParticle[i].transform.position, m_endPosition[i], m_rangedSpeed);

				travelTime += Time.deltaTime;
				//Checks if the particle has reached its destination
				//Removes the particle from active and the endposition for the particle
				//Removes one from the ranged index
				if(m_activeRangedParticle[i].transform.position == m_endPosition[i])
				{
					m_activeRangedParticle[i].time = m_activeRangedParticle[i].main.duration;

					m_activeRangedParticle.Remove(m_activeRangedParticle[i]);
					m_endPosition.Remove(m_endPosition[i]);

					if(m_rangedIndex < 0)
					{
						m_rangedIndex = 0;
					}
					else
					{
						--m_rangedIndex;
					}

					print(travelTime);
					travelTime = 0.0f;
				}
			}
		}
		
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit raycastHit;
		if (Physics.Raycast(ray, out raycastHit))
		{
			if(Input.GetMouseButtonDown(0))
			{
				if (m_unitPos == Vector3.zero)
				{
					m_unitPos = raycastHit.collider.gameObject.transform.position;
				}
				else
				{
					RangedParticle(m_unitPos, raycastHit.collider.gameObject.transform.position);
					m_unitPos = Vector3.zero;
				}
			}
		}
	}


}
