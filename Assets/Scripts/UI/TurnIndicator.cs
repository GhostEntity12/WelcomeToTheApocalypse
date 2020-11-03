using TMPro;
using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
    /// <summary>
	/// Which team's turn is it currently.
	/// </summary>
    private Allegiance m_CurrentTeamTurn = Allegiance.None;

    /// <summary>
    /// The text shown onscreen.
    /// </summary>
    private TextMeshProUGUI m_TMPText = null;

    void Awake()
    {
        m_TMPText = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateTurnIndicator(Allegiance newTeamTurn)
    {
        m_CurrentTeamTurn = newTeamTurn;

        // Update text to tell player who's turn it currently is.

        if (m_CurrentTeamTurn == Allegiance.Player)
        {
            Debug.Log("============Player turn============");
            m_TMPText.text = "Player turn";
        }
        else if (m_CurrentTeamTurn == Allegiance.Enemy)
        {
            Debug.Log("============Enemy turn============");
            m_TMPText.text = "Enemy turn";
        }
    }
}
