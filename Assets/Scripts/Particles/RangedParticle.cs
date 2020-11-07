using UnityEngine;

public class RangedParticle : MonoBehaviour
{
	[HideInInspector]
	public GameObject m_caster;

	public Unit m_Target;

	void OnTriggerEnter(Collider other)
	{
		Unit hitUnit = other.GetComponent<Unit>();
		//Check if the collider is not its self
		if (!hitUnit)
		{
			if (hitUnit == m_Target)
			{
				ParticlesManager.m_Instance.TakeSkillEffects();
			}
		}
	}
}
