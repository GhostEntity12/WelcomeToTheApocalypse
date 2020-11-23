using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
	public Unit m_EnemyToActivate;

	public TextAsset m_Scene;

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Camera>())
		{
			if (m_EnemyToActivate)
			{
				AIManager.m_Instance.EnableUnits(new Unit[] { m_EnemyToActivate });
			}
			if (m_Scene)
			{
				UIManager.m_Instance.SwapToDialogue(m_Scene, 0.4f);
			}
		}
	}
}
