using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitToMainMenu : MonoBehaviour
{
	public void QuitToMenu()
	{
		SceneManager.LoadScene("Menu_Famine");
	}
}