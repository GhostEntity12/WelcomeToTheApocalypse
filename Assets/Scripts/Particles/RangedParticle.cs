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
}

public class RangedParticle : MonoBehaviour
{
	[HideInInspector]
	public GameObject m_caster;

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

		Setup(m_Crackle);
		Setup(m_Swirl);
		Setup(m_LightTrail);
		Setup(m_DarkTrail);
		Setup(m_Head);
		Setup(m_Sparks);
	}

	private void Start()
	{
		SetColor(ParticlesManager.m_Instance.m_FamineRanged);
	}

	public void Play()
	{
		m_Crackle.m_ParticleSystem.Play();
		m_Swirl.m_ParticleSystem.Play();
		m_LightTrail.m_ParticleSystem.Play();
		m_DarkTrail.m_ParticleSystem.Play();
		m_Head.m_ParticleSystem.Play();
		m_Sparks.m_ParticleSystem.Play();
	}

	public void SetColor(RangedColor colors)
	{
		m_MatOuterOrb.SetColor("_Color", colors.m_OuterOrbColor);
		m_MatInnerOrb.SetColor("_FringeColor", colors.m_InnerOrbColor);

		SetMaterialColor(m_Crackle, colors.m_ParticleColor);
		SetMaterialColor(m_Swirl, colors.m_ParticleColor);
		SetMaterialColor(m_LightTrail, colors.m_ParticleColor);
		SetMaterialColor(m_DarkTrail, colors.m_ParticleColor);
		SetMaterialColor(m_Head, colors.m_ParticleColor);
		SetMaterialColor(m_Sparks, colors.m_ParticleColor);
	}

	void Setup(ParticleObject particle)
	{
		ParticleSystemRenderer particleRenderer = particle.m_Particle.GetComponent<ParticleSystemRenderer>();
		particleRenderer.material = new Material(particleRenderer.material);
		particleRenderer.trailMaterial = new Material(particleRenderer.trailMaterial);
		particle.m_BaseMaterial = particleRenderer.material;
		particle.m_TrailMaterial = particleRenderer.trailMaterial;

		particle.m_ParticleSystem = particle.m_Particle.GetComponent<ParticleSystem>();
	}

	void SetMaterialColor(ParticleObject particle, Color color)
	{
		particle.m_BaseMaterial.SetColor("_EmissionColor", color);
		particle.m_TrailMaterial.SetColor("_EmissionColor", color);
	}

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
