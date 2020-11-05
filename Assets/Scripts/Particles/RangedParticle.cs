using UnityEngine;

public class RangedParticle : MonoBehaviour
{
	[HideInInspector]
	public GameObject m_caster;

	void OnTriggerEnter(Collider other)
	{
		//Check if the collider is not its self
		if (other.gameObject != m_caster)
		{
			other.gameObject.GetComponent<Unit>().m_animator.SetTrigger("TriggerDamage");
		}
	}
}
