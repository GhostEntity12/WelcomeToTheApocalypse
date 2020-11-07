using UnityEngine;

namespace Ghost
{
	public class AnimationDebugger : MonoBehaviour
	{
		Animator m_Anim;

		public bool m_Reset;

		[Header("TriggerAnimations")]
		public bool m_Attack;
		public bool m_Damage;
		public bool m_Death;

		int m_DefaultHash;

		private void Start()
		{
			m_Anim = GetComponent<Animator>();
			m_DefaultHash = m_Anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
		}

		private void Update()
		{
			if (m_Reset)
			{
				m_Anim.Play(m_DefaultHash, 0);
				m_Reset = false;
			}
			if (m_Attack)
			{
				m_Anim.SetTrigger("TriggerSkill");
				m_Attack = false;
			}
			if (m_Damage)
			{
				m_Anim.SetTrigger("TriggerDamage");
				m_Damage = false;
			}
			if (m_Death)
			{
				m_Anim.SetTrigger("TriggerDeath");
				m_Death = false;
			}
		}
	}
}