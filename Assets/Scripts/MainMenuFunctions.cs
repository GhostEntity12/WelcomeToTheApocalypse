using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuFunctions : MonoBehaviour
{
	/// <summary>
	/// The camera's animator.
	/// </summary>
	private Animator m_Anim = null;

	/// <summary>
	/// Canvas group for fading to black.
	/// </summary>
	public CanvasGroup m_BlackScreen = null;

	public CanvasGroup m_Prompt = null;

	public List<Renderer> m_ArtistPosters = new List<Renderer>();
	public List<Renderer> m_DesignerPosters = new List<Renderer>();
	public List<Renderer> m_ProgrammerPosters = new List<Renderer>();

	List<Renderer> m_AllPosters = new List<Renderer>();

	public Collider m_ArtistCollider = null;
	public Collider m_DesignerCollider = null;
	public Collider m_ProgrammingCollider = null;

	/// <summary>
	/// On startup.
	/// </summary>
	void Awake()
	{
		m_Anim = Camera.main.GetComponent<Animator>();

		m_AllPosters = m_ArtistPosters.Concat(m_DesignerPosters).Concat(m_ProgrammerPosters).ToList();

		foreach (Renderer renderer in m_AllPosters)
		{
			renderer.material = new Material(renderer.material);
		}
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			FromSubsection();
		}
	}

	/// <summary>
	/// Fade to black and start the game.
	/// </summary>
	public void StartGame()
	{
		m_Anim.SetTrigger("isMainGame");
		LeanTween.alphaCanvas(m_BlackScreen, 1.0f, 1.5f);
		LeanTween.delayedCall(1.5f, LoadMainScene);
	}

	void LoadMainScene() => SceneManager.LoadScene("Famine_Split");

	/// <summary>
	/// Fade to black and quit the game.
	/// </summary>
	public void QuitGame()
	{
		LeanTween.alphaCanvas(m_BlackScreen, 1.0f, 1.0f);
		LeanTween.delayedCall(1.0f, Application.Quit);
	}

	/// <summary>
	/// Get animator to move to credits.
	/// </summary>
	public void ViewCredits()
	{
		m_Anim.SetBool("isCredits", true);
	}

	/// <summary>
	/// Get animator to leave credits.
	/// </summary>
	public void LeaveCredits()
	{
		m_Anim.SetBool("isCredits", false);
	}

	public void ToggleGlow(TextMeshPro text)
	{
		text.fontMaterial.SetFloat("_GlowPower", text.fontMaterial.GetFloat("_GlowPower") == 0 ? 1 : 0);
	}

	public void ToggleGlow(string disciplinePosters)
	{
		List<Renderer> postersToUpdate = new List<Renderer>();
		switch (disciplinePosters.ToLower())
		{
			case "art":
				postersToUpdate = m_ArtistPosters;
				break;
			case "design":
				postersToUpdate = m_DesignerPosters;
				break;
			case "programming":
				postersToUpdate = m_ProgrammerPosters;
				break;
			default:
				break;
		}
		foreach (Renderer renderer in postersToUpdate)
		{
			renderer.material.SetFloat("_DoOutline", renderer.material.GetFloat("_DoOutline") == 1 ? 0 : 1);
		}
	}

	public void ToSubsection(string boolName)
	{
		m_Anim.SetBool(boolName, true);
		m_ArtistCollider.enabled = false;
		m_DesignerCollider.enabled = false;
		m_ProgrammingCollider.enabled = false;
		LeanTween.alphaCanvas(m_Prompt, 1, 0.5f);
	}

	public void FromSubsection()
	{
		m_ArtistCollider.enabled = true;
		m_DesignerCollider.enabled = true;
		m_ProgrammingCollider.enabled = true;
		m_Anim.SetBool("isArtists", false);
		m_Anim.SetBool("isDesigners", false);
		m_Anim.SetBool("isProgrammers", false);
		LeanTween.alphaCanvas(m_Prompt, 0, 0.5f);
	}
}