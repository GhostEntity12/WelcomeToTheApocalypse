using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBlockingUI : MonoBehaviour
{
	/// <summary>
	/// If the player's cursor is over the UI element.
	/// </summary>
	private bool m_MouseOverUIElement = false;
    
	/// <summary>
	/// Set if the player's mouse is over the UI element.
	/// </summary>
	/// <param name="overButton">If the player's cursor is over the UI element.</param>
	public void SetMouseOverUIElement(bool overUI)
	{
		m_MouseOverUIElement = overUI;
	}

	/// <summary>
	/// Check if the player's mouse is over the UI element.
	/// </summary>
	/// <returns>If the player's cursor is over the UI element.</returns>
	public bool GetMouseOverUIElement() { return m_MouseOverUIElement; }
}