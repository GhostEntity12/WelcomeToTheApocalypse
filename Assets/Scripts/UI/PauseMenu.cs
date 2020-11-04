using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	public PopupCheck m_Popup;

	public void PopupRestart()
	{
		m_Popup.OpenPopup(
			() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex),
			"Restart",
			"Are you sure you want to restart?\n" +
			"Your progress won't be saved.");
	}

	public void PopupQuit()
	{
		m_Popup.OpenPopup(
			() => SceneManager.LoadScene(0),
			"Quit",
			"Are you sure you want to quit?\n" +
			"Your progress won't be saved.");
	}
}