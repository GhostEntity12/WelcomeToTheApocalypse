using System;
using TMPro;
using UnityEngine;

public class PopupCheck : MonoBehaviour
{
	public Action m_ConfirmAction;

	public TextMeshProUGUI m_BodyText;
	public TextMeshProUGUI m_Confirmation;

	public void OpenPopup(Action action, string actionName, string bodyText)
	{
		LeanTween.scale(gameObject, Vector2.one, 0.03f).setEaseOutCubic();
		m_ConfirmAction = action;
		m_Confirmation.text = actionName;
		m_BodyText.text = bodyText;
	}

	public void ClosePopup() => LeanTween.scale(gameObject, Vector2.zero, 0.03f).setEaseInCubic();

	public void DoAction() => m_ConfirmAction.Invoke();
}
