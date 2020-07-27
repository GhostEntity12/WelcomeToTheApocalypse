using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// VARIABLES AND FUNCTIONS THAT ARE COMMENTED ARE FOR SYSTEMS THAT HAVE YET TO BE CREATED, JUST TEMPORARY FOR GETTING IDEAS OUT.

public enum Allegiance
{
    // Character is on the player's side.
    Player,
    // Character is an enemy to the player.
    Enemy,
    // Character doesn't have an allegiance (just to be safe).
    None
}

public class Unit : MonoBehaviour
{
    // The starting health of the character.
    public int m_StartingHealth = 10;

    // The current health of the character.
    private int m_CurrentHealth = 0;

    // The starting movement of the character.
    public int m_StartingMovement = 6;

    // The current movement of the character.
    private int m_CurrentMovement = 0;

    // The skills avaliable to the character.
    //public List<Skill> m_Skills = new List<Skill>();

    // The passive effect of the character.
    public StatusEffect m_PassiveEffect = null;

    // The status debuffs on the character.
    private List<InflictableStatus> m_StatusDebuffs = new List<InflictableStatus>();

    // What "team" the character is on.
    public Allegiance m_Allegiance = Allegiance.None;

    // Is the character alive?
    private bool m_Alive = true;

    // Is the character moving?
    private bool m_Moving = false;

    // The path for the character to take to get to their destination.
    //private Queue<GridNode> m_MovementPath = new Queue<GridNode>();

    // On startup.
    void Awake()
    {
        m_CurrentHealth = m_StartingHealth;

        m_CurrentMovement = m_StartingMovement;
    }

    // Set the character's health to something.
    public void SetCurrentHealth(int health)
    {
        m_CurrentHealth = health;
        CheckAlive();
    }

    // Get the character's currnet health.
    public int GetCurrentHealth() { return m_CurrentHealth; }

    // Increase the character's current health.
    public void IncreaseCurrentHealth(int increase) { m_CurrentHealth += increase; }

    // Decrease the character's current health.
    public void DecreaseCurrentHealth(int decrease)
    {
        m_CurrentHealth -= decrease;
        CheckAlive();
    }    

    // Reset the character's current health.
    public void ResetCurrentHealth() { m_CurrentHealth = m_StartingHealth; }

    // Check if the character's health is above 0.
    // If equal to or below, the character is not alive.
    private void CheckAlive()
    {
        if (m_CurrentHealth <= 0)
        {
            m_Alive = false;
        }
    }

    // Get the current amount of movement of the character.
    public int GetCurrentMovement() { return m_CurrentMovement; }

    // Decrement the character's current amount of movement.
    public void DecrementCurrentMovement() { --m_CurrentMovement; }

    // Get the list of skills of the character.
    //public List<Skill> GetSkills() { return m_Skills; }

    // Set the movement path of the character.
    // public void SetMovementPath(Queue<GridNode> path)
    //{
    //    m_MovementPath = path;
    //    m_Moving = true
    //}
}