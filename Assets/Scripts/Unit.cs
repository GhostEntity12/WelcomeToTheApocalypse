using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Ghost.BFS;

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
    public int m_StartingHealth = 6;

    // The current health of the character.
    private int m_CurrentHealth = 0;

    // The starting movement of the character.
    public int m_StartingMovement = 5;

    // The current movement of the character.
    private int m_CurrentMovement = 0;

    // How fast the unit moves.
    public float m_MoveSpeed = 3.0f;

    // The skills avaliable to the character.
    public List<BaseSkill> m_Skills = new List<BaseSkill>();

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
    private Queue<Node> m_MovementPath = new Queue<Node>();

    private Vector3 m_TargetPosition = Vector3.zero;

    // Blame James L for this
    public List<Node> m_MovableNodes = new List<Node>();

    // On startup.
    void Awake()
    {
        m_CurrentHealth = m_StartingHealth;

        m_CurrentMovement = m_StartingMovement;
    }

    void Update()
    {
        // If have a target that the unit hasn't arrived at yet, move towards the target position.
        // Would be refactored to move along path rather than towards a target position.
        if (m_Moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, m_MoveSpeed * Time.deltaTime);
            // If have arrived at position (0.01 units close to target is close enough).
            if ((transform.position - m_TargetPosition).magnitude < 0.01f)
            {
                m_Moving = false;
                transform.position = m_TargetPosition; // Just putting this here so it sets the position exactly. - James L
            }
        }
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
    public void IncreaseCurrentHealth(int increase)
    {
        m_CurrentHealth += increase;

        // Don't go over that max starting health.
        if (m_CurrentHealth > m_StartingHealth)
            m_CurrentHealth = m_StartingHealth;
    }

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

    // Decrease the character's current amount of movement.
    public void DecreaseCurrentMovement(int decrease) { m_CurrentMovement -= decrease; }

    // Get the list of skills of the character.
    public List<BaseSkill> GetSkills() { return m_Skills; }

    // Get a specific skill.
    public BaseSkill GetSkill(int skillIndex) { return m_Skills[skillIndex]; }

    // Set the movement path of the character.
    public void SetMovementPath(Queue<Node> path)
    {
        m_MovementPath = path;
        m_Moving = true;
    }

    public void SetTargetPosition(Vector3 target)
    {
        m_TargetPosition = target;
        m_Moving = true;
    }

    // Gets the nodes the unit can move to, stores them and highlights them
    public void HighlightMovableNodes(Node startingNode = null)
    {
        m_MovableNodes = GetNodesWithinRadius(GetCurrentMovement(), startingNode ?? Grid.m_Instance.GetNode(transform.position)); // Returns the current node by default, but can be overridden
        Grid.m_Instance.HighlightNodes(m_MovableNodes);
    }

    public void ActivateSkill(int skillInput)
    {
        m_Skills[skillInput - 1].CastSkill();
    }
}