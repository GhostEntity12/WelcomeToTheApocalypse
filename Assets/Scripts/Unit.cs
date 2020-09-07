using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public List<BaseSkill> m_LearnedSkills = new List<BaseSkill>();

    /// <summary>
    /// The skills avaliable to the unit.
    /// </summary>
    private List<BaseSkill> m_Skills = new List<BaseSkill>();

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
    private bool m_IsAlive = true;

    /// <summary>
    /// Is the character moving?
    /// </summary>
    private bool m_IsMoving = false;

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
    private HealthbarContainer m_Healthbar = null;

    /// <summary>
    /// The script for the health change indicator.
    /// </summary>
    private HealthChangeIndicator m_HealthChangeIndicatorScript = null;

    /// <summary>
    /// The starting action points of the unit.
    /// </summary>
    public int m_StartingActionPoints = 1;

    /// <summary>
    /// The currect action points of the unit.
    /// </summary>
    private int m_CurrentActionPoints = 0;

    /// <summary>
    /// Position of the unit's healthbar above their head.
    /// </summary>
    public Transform m_HealthbarPosition = null;

    public Action<Unit> m_ActionOnFinishPath;

    public AIHeuristicCalculator m_AIHeuristicCalculator = null;

    // On startup.
    void Awake()
    {
        m_CurrentHealth = m_StartingHealth;

        m_CurrentMovement = m_StartingMovement;

        m_CurrentActionPoints = m_StartingActionPoints;

        m_Skills = m_LearnedSkills.Select(s => Instantiate(s)).ToList();
    }

    void Start()
    {
        Grid.m_Instance.SetUnit(gameObject);
       m_CurrentTargetNode = Grid.m_Instance.GetNode(transform.position);
    }

    void Update()
    {
        // If have a target that the unit hasn't arrived at yet, move towards the target position.
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position,m_CurrentTargetNode.worldPosition, m_MoveSpeed * Time.deltaTime);
            // If have arrived at position (0.1 units close to target is close enough).
            if ((transform.position -m_CurrentTargetNode.worldPosition).magnitude < 0.1f)
            {
                // Set the actual position to the target
                transform.position =m_CurrentTargetNode.worldPosition; // Just putting this here so it sets the position exactly. - James L

                // Target the next node.
                if (m_MovementPath.Count > 0)
                {
                    SetTargetNodePosition(m_MovementPath.Pop());
                }
                // Have arrived at the final node in the path, stop moving.
                else
                {
                    m_IsMoving = false;
                    Grid.m_Instance.SetUnit(gameObject);
                    m_ActionOnFinishPath?.Invoke(this);
                    m_ActionOnFinishPath = null;
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

        if (m_Healthbar != null)
        {
            m_Healthbar.gameObject.SetActive(true);
            m_Healthbar.transform.position = Camera.main.WorldToScreenPoint(m_HealthbarPosition.position);
            m_Healthbar.m_HealthbarImage.fillAmount = (float)m_CurrentHealth / m_StartingHealth;
            m_Healthbar.SetChildrenActive(true);
            m_HealthChangeIndicatorScript.SetStartPositionToCurrent();
            m_HealthChangeIndicatorScript.Reset();
            m_Healthbar.Reset();
        }
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
        SetCurrentHealth(m_CurrentHealth + increase);

        if (m_Healthbar != null)
        {
            m_Healthbar.m_HealthChangeIndicator.text = "+" + increase;
            m_HealthChangeIndicatorScript.HealthIncreased();
        }
    }

    /// <summary>
    /// Decrease the unit's current health.
    /// </summary>
    /// <param name="decrease"> The amount to decrease the unit's health by. </param>
    public void DecreaseCurrentHealth(int decrease)
    {
        SetCurrentHealth(m_CurrentHealth - decrease);

        if (m_Healthbar != null)
        {
            m_Healthbar.m_HealthChangeIndicator.text = "-" + decrease;
            m_HealthChangeIndicatorScript.HealthDecrease();
        }
    }

    /// <summary>
    /// Reset the unit's current health.
    /// </summary>
    public void ResetCurrentHealth() { m_CurrentHealth = m_StartingHealth; }

    /// <summary>
    /// Check if the unit's health is above 0.
    /// If equal to or below, the unit is not alive.
    /// </summary>
    private void CheckAlive()
    {
        if (m_CurrentHealth <= 0)
        {
            Debug.Log($"{name} died");
            m_IsAlive = false;

            // Check if the unit has the "DefeatEnemyWinCondition" script on it.
            // If it does, the player has won the level by defeating the boss.
            GetComponent<DefeatEnemyWinCondition>()?.EnemyDefeated();

            // If this is a player unit, check if the player has any units remaining.
            if (m_Allegiance == Allegiance.Player)
            {
                GameManager.m_Instance.CheckPlayerUnitsAlive();
                UnitsManager.m_Instance.m_DeadPlayerUnits.Add(this);
                UnitsManager.m_Instance.m_PlayerUnits.Remove(this);
            }

            Node currentNode = Grid.m_Instance.GetNode(transform.position);
            currentNode.unit = null;
            currentNode.m_isBlocked = false;
            // TODO: replace with something to actually remove the unit
            gameObject.SetActive(false);
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
        m_IsMoving = true;
        SetTargetNodePosition(m_MovementPath.Pop());
        print(string.Join(", ", path.Select(n=>n.m_NodeHighlight.name)));
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
    /// Get if the unit is alive.
    /// </summary>
    /// <returns>If the unit is alive.</returns>
    public bool GetAlive() { return m_IsAlive; }

    /// <summary>
    /// Get the unit's action points.
    /// </summary>
    /// <returns>The current amount of action points the unit has.</returns>
    public int GetActionPoints() { return m_CurrentActionPoints; }

    /// <summary>
    /// Decrease the amount of action points the unit has.
    /// </summary>
    /// <param name="decrease">The amount to decrease the unit's action points by.</param>
    public void DecreaseActionPoints(int decrease)
    {
        m_CurrentActionPoints -= decrease;
    }

    /// <summary>
    /// Reset the unit's action points.
    /// </summary>
    public void ResetActionPoints()
    {
        m_CurrentActionPoints = m_StartingActionPoints;
    }

    /// <summary>
    /// Add a status effect to the unit.
    /// </summary>
    /// <param name="effect"> The status effect to add to the unit. </param>
    public void AddStatusEffect(InflictableStatus effect) { m_StatusEffects.Add(effect); }

    /// <summary>
    /// Set the healthbar of the unit.
    /// </summary>
    /// <param name="healthbar">The healthbar game object.</param>
    public void SetHealthbar(HealthbarContainer healthbar)
    {
        m_Healthbar = healthbar.GetComponent<HealthbarContainer>();        
        m_HealthChangeIndicatorScript = healthbar.m_HealthChangeIndicator.GetComponent<HealthChangeIndicator>();
    }

    /// <summary>
    /// Get the heuristic calculator on the unit.
    /// </summary>
    /// <returns>The unit's heuristic calculator.</returns>
    public AIHeuristicCalculator GetHeuristicCalculator() { return m_AIHeuristicCalculator; }

    /// <summary>
    /// Gets the nodes the unit can move to, stores them and highlights them.
    /// </summary>
    /// <param name="startingNode"> The node to search from, can find it's own position if it can't be provided. </param>
    public void HighlightMovableNodes(Node startingNode = null)
    {
        m_MovableNodes = Grid.m_Instance.GetNodesWithinRadius(GetCurrentMovement(), startingNode ?? Grid.m_Instance.GetNode(transform.position)); // Returns the current node by default, but can be overridden
        foreach (Node node in m_MovableNodes)
        {
            node.m_NodeHighlight.ChangeHighlight(TileState.MovementRange);
        }
    }

    /// <summary>
    /// Activate one of the unit's skills.
    /// </summary>
    /// <param name="skill"> The skill to activate. </param>
    public void ActivateSkill(BaseSkill skill, Node castLocation)
    {
        // Doing my own search cause List.Find is gross.
        for (int i = 0; i < m_Skills.Count; ++i)
        {
            if (m_Skills[i] == skill)
            {
                m_Skills[i].affectedNodes = Grid.m_Instance.GetNodesWithinRadius(m_Skills[i].m_AffectedRange, castLocation, true);
                m_Skills[i].CastSkill();
                return;
            }
        }

        Debug.LogError("Skill " + skill.name + " couldn't be found in " + gameObject.name + ".");
    }
}