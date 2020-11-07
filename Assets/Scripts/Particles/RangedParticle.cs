using UnityEngine;

[System.Serializable]
public class ParticleObject
{
	public GameObject m_Particle;
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

		MaterialSetup(m_Crackle);
		MaterialSetup(m_Swirl);
		MaterialSetup(m_LightTrail);
		MaterialSetup(m_DarkTrail);
		MaterialSetup(m_Head);
		MaterialSetup(m_Sparks);
	}

	private void Start()
	{
		SetColor(ParticlesManager.m_Instance.m_FamineRanged);
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

	void MaterialSetup(ParticleObject particle)
	{
		ParticleSystemRenderer particleRenderer = particle.m_Particle.GetComponent<ParticleSystemRenderer>();
		particleRenderer.material = new Material(particleRenderer.material);
		particleRenderer.trailMaterial = new Material(particleRenderer.trailMaterial);
		particle.m_BaseMaterial = particleRenderer.material;
		particle.m_TrailMaterial = particleRenderer.trailMaterial;
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
