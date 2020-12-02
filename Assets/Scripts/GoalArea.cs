using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class GoalArea : MonoBehaviour
{
	public CanvasGroup m_BlackScreen;
	public TextAsset m_Scene;
	public int m_SceneToLoad;

	private void OnTriggerEnter(Collider other)
	{
		Unit u = other.GetComponent<Unit>();
		if (u && u.GetAllegiance() == Allegiance.Player)
		{
			if (m_Scene)
			{
				UIManager.m_Instance.SwapToDialogue(m_Scene, onDialogueEndAction: LoadScene);
			}
		}
	}

	void LoadScene()
	{
		LeanTween.alphaCanvas(m_BlackScreen, 1, 0.5f).setOnComplete(() => SceneManager.LoadScene(m_SceneToLoad));
	}
}
