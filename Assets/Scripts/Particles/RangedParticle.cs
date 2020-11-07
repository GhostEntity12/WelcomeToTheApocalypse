using UnityEngine;

[System.Serializable]
public class ParticleObject
{
	public GameObject m_Particle;
	[HideInInspector]
	public ParticleSystem m_ParticleSystem;
	[HideInInspector]
	public Material m_BaseMaterial;
	[HideInInspector]
	public Material m_TrailMaterial;

	public void Setup()
	{
		ParticleSystemRenderer particleRenderer = m_Particle.GetComponent<ParticleSystemRenderer>();
		particleRenderer.material = new Material(particleRenderer.material);
		particleRenderer.trailMaterial = new Material(particleRenderer.trailMaterial);
		m_BaseMaterial = particleRenderer.material;
		m_TrailMaterial = particleRenderer.trailMaterial;

		m_ParticleSystem = m_Particle.GetComponent<ParticleSystem>();
	}

	public void SetColor(Color color)
	{
		m_BaseMaterial.SetColor("_EmissionColor", color);
		m_TrailMaterial.SetColor("_EmissionColor", color);
	}
}

public class RangedParticle : MonoBehaviour
{
	public Unit m_Target;

	[Header("Parts")]
	public GameObject m_OuterOrb;
	public GameObject m_InnerOrb;
	public ParticleObject m_Crackle;
	public ParticleObject m_Swirl;
	public ParticleObject m_LightTrail;
	public ParticleObject m_DarkTrail;
	public ParticleObject m_Head;
	public ParticleObject m_Sparks;

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
		m_DarkTrail.Setup();
		m_LightTrail.Setup();
		m_Head.Setup();
		m_Sparks.Setup();
	}

	public void Play()
	{
		m_InnerOrb.SetActive(true);
		m_OuterOrb.SetActive(true);
		m_Crackle.m_ParticleSystem.Play();
		m_Swirl.m_ParticleSystem.Play();
		m_LightTrail.m_ParticleSystem.Play();
		m_DarkTrail.m_ParticleSystem.Play();
		m_Head.m_ParticleSystem.Play();
		m_Sparks.m_ParticleSystem.Play();
	}
	public void Stop()
	{
		m_InnerOrb.SetActive(false);
		m_OuterOrb.SetActive(false);
		m_Crackle.m_ParticleSystem.Stop();
		m_Swirl.m_ParticleSystem.Stop();
		m_LightTrail.m_ParticleSystem.Stop();
		m_DarkTrail.m_ParticleSystem.Stop();
		m_Head.m_ParticleSystem.Stop();
		m_Sparks.m_ParticleSystem.Stop();
	}

	public void SetColor(RangedColor colors)
	{
		m_MatOuterOrb.SetColor("_Color", colors.m_OuterOrbColor);
		m_MatInnerOrb.SetColor("_FringeColor", colors.m_InnerOrbColor);

		m_Crackle.SetColor(colors.m_ParticleColor);
		m_Swirl.SetColor(colors.m_ParticleColor);
		m_LightTrail.SetColor(colors.m_ParticleColor);
		m_DarkTrail.SetColor(colors.m_ParticleColor);
		m_Head.SetColor(colors.m_ParticleColor);
		m_Sparks.SetColor(colors.m_ParticleColor);
	}

	void OnTriggerEnter(Collider other)
	{
		Unit hitUnit = other.GetComponent<Unit>();
		//Check if the collider is not its self
		if (hitUnit)
		{
			if (hitUnit == m_Target)
			{
				ParticlesManager.m_Instance.TakeSkillEffects();
			}
		}
	}
}
