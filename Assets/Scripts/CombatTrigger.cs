using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
	public GameObject m_EnemyGroupToActivate;
	private Unit[] m_EnemiesToActivate = new Unit[] { };

	public TextAsset scene;


	private void Awake()
	{
		// Remove empty slots
		if (m_EnemyGroupToActivate)
		{
			m_EnemiesToActivate = m_EnemyGroupToActivate.GetComponentsInChildren<Unit>();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Unit>()?.GetAllegiance() == Allegiance.Player)
		{
			if (m_EnemyGroupToActivate)
			{
				AIManager.m_Instance.EnableUnits(m_EnemiesToActivate);
			}

			if (scene)
			{
				if (scene.name.Contains("Start"))
				{
					UIManager.m_Instance.SwapToDialogue(scene, () => UIManager.m_Instance.m_Tutorial.OpenTutorial());
				}
				else
				{
					UIManager.m_Instance.SwapToDialogue(scene);
				}
			}

			gameObject.SetActive(false);
		}
	}
}
