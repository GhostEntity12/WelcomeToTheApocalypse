using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRangedParticle : MonoBehaviour
{
	[Header("Parts")]
	public GameObject m_OuterOrb;
	public GameObject m_InnerOrb;
	public ParticleObject m_Crackle;
	public ParticleObject m_Swirl;

	private Material m_MatOuterOrb;
	private Material m_MatInnerOrb;

	private void Awake()
	{
		Renderer outerOrbRenderer = m_OuterOrb.GetComponent<Renderer>();
		outerOrbRenderer.material = new Material(outerOrbRenderer.material);
		m_MatOuterOrb = outerOrbRenderer.material;

		Renderer innerOrbRenderer = m_InnerOrb.GetComponent<Renderer>();
		innerOrbRenderer.material = new Material(innerOrbRenderer.material);
		m_MatInnerOrb = innerOrbRenderer.material;

		m_Crackle.Setup();
		m_Swirl.Setup();
	}

	private void Start()
	{
		SetColor(ParticlesManager.m_Instance.m_EnemyRanged);
	}

	public void SetColor(RangedColor colors)
	{
		m_MatOuterOrb.SetColor("_Color", colors.m_OuterOrbColor);
		m_MatInnerOrb.SetColor("_FringeColor", colors.m_InnerOrbColor);

		m_Crackle.SetColor(colors.m_ParticleColor);
		m_Swirl.SetColor(colors.m_ParticleColor);
	}
}
