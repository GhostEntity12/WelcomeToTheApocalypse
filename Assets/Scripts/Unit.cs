using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using static Ghost.BFS;

public enum Allegiance
{
    /// <summary>
    /// Character is on the player's side.
    /// </summary>
    Player,
    /// <summary>
    /// Character is an enemy to the player.
    /// </summary>
    Enemy,
    /// <summary>
    /// Character doesn't have an allegiance (just to be safe).
    /// </summary>
    None
}

public class Unit : MonoBehaviour
{
    /// <summary>
    /// The starting health of the character.
    /// </summary>
    public int m_StartingHealth = 6;

    /// <summary>
    /// The current health of the character.
    /// </summary>
    private int m_CurrentHealth = 0;

    /// <summary>
    /// The starting movement of the character.
    /// </summary>
    public int m_StartingMovement = 5;

    /// <summary>
    /// The current movement of the character.
    /// </summary>
    private int m_CurrentMovement = 0;

    /// <summary>
    /// How fast the unit moves.
    /// </summary>
    public float m_MoveSpeed = 3.0f;

    /// <summary>
    /// The skills avaliable to the unit.
    /// </summary>
    public List<BaseSkill> m_Skills = new List<BaseSkill>();

    /// <summary>
    /// The passive effect of the character.
    /// </summary>
    public StatusEffect m_PassiveEffect = null;

    /// <summary>
    /// The status debuffs on the character.
    /// </summary>
    private List<InflictableStatus> m_StatusEffects = new List<InflictableStatus>();

    /// <summary>
    /// The allegiance of the character.
    /// </summary>
    public Allegiance m_Allegiance = Allegiance.None;

    /// <summary>
    /// Is the character alive?
    /// </summary>
    private bool m_Alive = true;

    /// <summary>
    /// Is the character moving?
    /// </summary>
    private bool m_Moving = false;

    /// <summary>
    /// The path for the character to take to get to their destination.
    /// </summary>
    private Stack<Node> m_MovementPath = new Stack<Node>();

    /// <summary>
    /// The node the unit is targeting for their movement.
    /// </summary>
    private Node m_CurrentTargetNode = null;

    /// <summary>
    /// The node's the unit can move to.
    /// </summary>
    public List<Node> m_MovableNodes = new List<Node>();

    /// <summary>
    /// The image representing the unit's health.
    /// </summary>
    public Image m_HealthBar = null;

    /// <summary>
    /// The text indicating a change to a unit's health.
    /// </summary>
    public GameObject m_HealthChangeIndicator = null;

    private TextMeshProUGUI m_HealthChangeIndicatorText = null;

    private HealthChangeIndicator m_HealthChangeIndicatorScript = null;

    // On startup.
    void Awake()
    {
        m_CurrentHealth = m_StartingHealth;

        m_CurrentMovement = m_StartingMovement;

        if (m_HealthChangeIndicator != null)
        {
        m_HealthChangeIndicatorText = m_HealthChangeIndicator.GetComponent<TextMeshProUGUI>();
        m_HealthChangeIndicatorScript = m_HealthChangeIndicator.GetComponent<HealthChangeIndicator>();
        }
    }

    void Start()
    {
        Grid.m_Instance.SetUnit(gameObject);
        m_CurrentTargetNode = Grid.m_Instance.GetNode(transform.position);
    }

    void Update()
    {
        // If have a target that the unit hasn't arrived at yet, move towards the target position.
        if (m_Moving)
        {
            //Debug.Log((transform.position - m_TargetNode.worldPosition).magnitude);
            transform.position = Vector3.MoveTowards(transform.position, m_CurrentTargetNode.worldPosition, m_MoveSpeed * Time.deltaTime);
            // If have arrived at position (0.1 units close to target is close enough).
            if ((transform.position - m_CurrentTargetNode.worldPosition).magnitude < 0.1f)
            {
                transform.position = m_CurrentTargetNode.worldPosition; // Just putting this here so it sets the position exactly. - James L

                // Target the next node.
                if (m_MovementPath.Count > 0)
                {
                    SetTargetNodePosition(m_MovementPath.Pop());
                }
                // Have arrived at the final node in the path, stop moving.
                else
                {
                    m_Moving = false;
                    Grid.m_Instance.SetUnit(gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Set the health of the unit.
    /// </summary>
    /// <param name="health"> What to set the unit's health to. </param>
    public void SetCurrentHealth(int health)
    {
        m_CurrentHealth = health;
        CheckAlive();

        // Don't go over that max starting health.
        if (m_CurrentHealth > m_StartingHealth)
            m_CurrentHealth = m_StartingHealth;

        m_HealthBar.fillAmount = m_CurrentHealth / m_StartingHealth;
        m_HealthChangeIndicatorScript.Reset();
    }

    /// <summary>
    /// Get the current health of the unit.
    /// </summary>
    /// <returns> The current health of the unit. </returns>
    public int GetCurrentHealth() { return m_CurrentHealth; }

    /// <summary>
    /// Increase the unit's current health.
    /// </summary>
    /// <param name="increase"> The amount to increase the unit's health by. </param>
    public void IncreaseCurrentHealth(int increase)
    {
        m_HealthChangeIndicatorText.text = "+" + increase;
        SetCurrentHealth(m_CurrentHealth + increase);
        m_HealthChangeIndicatorScript.HealthIncreased();
    }

    /// <summary>
    /// Decrease the unit's current health.
    /// </summary>
    /// <param name="decrease"> The amount to decrease the unit's health by. </param>
    public void DecreaseCurrentHealth(int decrease)
    {
        m_HealthChangeIndicatorText.text = "-" + decrease;
        SetCurrentHealth(m_CurrentHealth - decrease);
        m_HealthChangeIndicatorScript.HealthDecrease();
    }

    /// <summary>
    /// Reset the unit's current health.
    /// </summary>
    public void ResetCurrentHealth() { m_CurrentHealth = m_StartingHealth; }

    /// <summary>
    /// Check if the character's health is above 0.
    /// If equal to or below, the character is not alive.
    /// </summary>
    private void CheckAlive()
    {
        if (m_CurrentHealth <= 0)
        {
            m_Alive = false;
        }
    }

    /// <summary>
    /// Get the current amount of movement of the character.
    /// </summary>
    /// <returns> The unit's current movement. </summary>
    public int GetCurrentMovement() { return m_CurrentMovement; }

    /// <summary>
    /// Decrease the character's current amount of movement.
    /// </summary>
    /// <param name="decrease"> The amount to decrease the unit's movement pool by. </param>
    public void DecreaseCurrentMovement(int decrease) { m_CurrentMovement -= decrease; }

    /// <summary>
    /// Reset the unit's current movement.
    /// </summary>
    public void ResetCurrentMovement() { m_CurrentMovement = m_StartingMovement; }

    /// <summary>
    /// Get the list of skills of the unit.
    /// </summary>
    /// <returns> List of skills the unit has. </returns>
    public List<BaseSkill> GetSkills() { return m_Skills; }

    /// <summary>
    /// Get a specific skill.
    /// </summary>
    /// <param name="skillIndex"> The index of the skill to get. </param>
    public BaseSkill GetSkill(int skillIndex) { return m_Skills[skillIndex]; }

    // Set the movement path of the character.
    public void SetMovementPath(Stack<Node> path)
    {
        m_MovementPath = path;
        m_Moving = true;
        SetTargetNodePosition(m_MovementPath.Pop());
        DecreaseCurrentMovement(path.Count);
    }

    /// <summary>
    /// Set the target node of the unit.
    /// </summary>
    /// <param name="target"> The node to set as the target. </param>
    public void SetTargetNodePosition(Node target)
    {
        // Unassign the unit on the current node.
        // Before setting the new target node.
        Grid.m_Instance.RemoveUnit(m_CurrentTargetNode);
        m_CurrentTargetNode = target;
    }

    /// <summary>
    /// Get the unit's path.
    /// </summary>
    /// <returns> Stack of the unit's movement path. </returns>
    public Stack<Node> GetMovementPath() { return m_MovementPath; }

    /// <summary>
    /// Get the unit's allegiance.
    /// </summary>
    /// <returns> The allegiance of the unit. </returns>
    public Allegiance GetAllegiance() { return m_Allegiance; }

    /// <summary>
    /// Add a status effect to the unit.
    /// </summary>
    /// <param name="effect"> The status effect to add to the unit. </param>
    public void AddStatusEffect(InflictableStatus effect) { m_StatusEffects.Add(effect); }

    /// <summary>
    /// Gets the nodes the unit can move to, stores them and highlights them.
    /// </summary>
    /// <param name="startingNode"> The node to search from, can find it's own position if it can't be provided. </param>
    public void HighlightMovableNodes(Node startingNode = null)
    {
        m_MovableNodes = GetNodesWithinRadius(GetCurrentMovement(), startingNode ?? Grid.m_Instance.GetNode(transform.position)); // Returns the current node by default, but can be overridden
        foreach (Node node in m_MovableNodes)
        {
            node.m_NodeHighlight.ChangeHighlight(TileState.MovementRange);
        }
    }

    /// <summary>
    /// Activate one of the unit's skills.
    /// </summary>
    /// <param name="skill"> The skill to activate. </param>
    public void ActivateSkill(BaseSkill skill)
    {
        // Doing my own search cause List.Find is gross.
        for (int i = 0; i < m_Skills.Count; ++i)
        {
            if (m_Skills[i] == skill)
            {
                m_Skills[i].CastSkill();
                return;
            }
        }

        Debug.LogError("Skill " + skill.name + " couldn't be found in " + gameObject.name + ".");
    }
}