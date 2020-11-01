using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
	/// <summary>
	/// Which team's turn is it currently.
	/// </summary>
	private Allegiance m_CurrentTeamTurn = Allegiance.None;

	/// <summary>
	/// The position of the button when it is onscreen.
	/// </summary>
	public Transform m_OnScreenPosition = null;

	/// <summary>
	/// The position of the button when it is offscreen.
	/// </summary>
	private Transform m_OffScreenPosition = null;

	/// <summary>
	/// The time to tween between on and off screen.
	/// </summary>
	public float m_TweenTime = 0.1f;

	void Awake()
	{
		m_OffScreenPosition = transform;
	}

	/// <summary>
	/// Update the end turn button on who's turn it currently is.
	/// </summary>
	/// <param name="newTeamTurn">Which team is starting their turn now.</param>
	public void UpdateCurrentTeamTurn(Allegiance newTeamTurn)
	{
		m_CurrentTeamTurn = newTeamTurn;

		// Player's turn, move end turn button onto the screen.
		if (m_CurrentTeamTurn == Allegiance.Player)
		{
			LeanTween.move(gameObject, m_OnScreenPosition, m_TweenTime);
		}
		// Enemy's turn, move end turn button off screen.
		else if (m_CurrentTeamTurn == Allegiance.Enemy)
		{
			LeanTween.move(gameObject, m_OffScreenPosition, m_TweenTime);
		}
	}
}