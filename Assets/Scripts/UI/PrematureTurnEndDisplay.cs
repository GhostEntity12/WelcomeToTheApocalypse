using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrematureTurnEndDisplay : MonoBehaviour
{
	public TextMeshProUGUI m_UnitsStillWithTurnsText = null;

	private readonly string m_DefaultString = "You still have units who can act!\n" +
												"Are you sure you want to end your turn?\n\n" +
												"Units with actions left:<size=32.5>\n";

	public bool m_Active = false;

	private void Start()
	{
		LeanTween.scale(gameObject, Vector2.zero, 0f);
	}

	public void UpdateText()
	{
		string unitsWithActions = string.Empty;

		foreach (Unit unit in UnitsManager.m_Instance.m_PlayerUnits.Where(u => u.GetActionPoints() > 0 || u.GetCurrentMovement() > 0))
		{
			unitsWithActions += unit.name + "\n";
		}

		m_UnitsStillWithTurnsText.text = m_DefaultString + unitsWithActions;
	}

	public void DisplayPrematureEndScreen(bool display)
	{
		if (display)
		{
			UIManager.m_Instance.m_ActiveUI = true;
			m_Active = true;
			UpdateText();
			LeanTween.scale(gameObject, Vector2.one, 0.03f);
		}
		else
		{
			UIManager.m_Instance.m_ActiveUI = false;
			m_Active = false;
			UIManager.m_Instance.EndTurnBlocker().SetMouseOverUIElement(false);
			LeanTween.scale(gameObject, Vector2.zero, 0.03f);
		}

		// Need this to force the Content Fitters to behave, forcing decisions of undefined behaviour
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
	}
}