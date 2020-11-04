using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanner : MonoBehaviour
{
	public float m_PanSpeed = 0.3f;

	private int m_CurrentPanel = 0;
	private int m_PanelCount;

	private RectTransform m_RectTransform;

	private void Start()
	{
		m_PanelCount = transform.childCount;
		transform.GetChild(0).GetComponent<TutorialPanel>().m_LeftButton.SetActive(false);
		transform.GetChild(m_PanelCount - 1).GetComponent<TutorialPanel>().m_RightButton.SetActive(false);
		m_RectTransform = GetComponent<RectTransform>();
	}

	public void PanLeft()
	{
		m_CurrentPanel++;
		LeanTween.move(m_RectTransform, new Vector2(m_CurrentPanel * 1335f, 0), m_PanSpeed).setEaseInOutCubic();
	}

	public void PanRight()
	{
		m_CurrentPanel--;
		LeanTween.move(m_RectTransform, new Vector2(m_CurrentPanel * 1335f, 0), m_PanSpeed).setEaseInOutCubic();
	}
}
