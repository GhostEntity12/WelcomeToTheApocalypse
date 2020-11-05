using System.Collections.Generic;

public class AIUnit : Unit
{
	public enum State
	{
		Attacking,
		Fleeing,
		Moving,
		Idle
	}

	State m_currentState;

	public int m_currentHealth;
	public int m_startingMovement;
	public int m_currentMovement;
	public int m_movementCost;

	public bool m_isAlive;
	public bool m_isMoving;

	public Stack<Node> m_movementPath;
	public Node m_currentTargetNode;
	public List<Node> m_moveableNodes;

	public List<Unit> playerUnits;

	public Unit firstTarget;
	public Unit secondTarget;

	private void Awake()
	{
		m_isAlive = true;
		m_isMoving = false;

		m_Allegiance = Allegiance.Enemy;
		m_currentState = State.Idle;

		m_movementCost = 0;

		m_startingMovement = 5;
		m_currentMovement = m_startingMovement;

		m_StartingHealth = 6;
		m_currentHealth = m_StartingHealth;

		m_MoveSpeed = 3.0f;

		m_MovableNodes = new List<Node>();
		m_currentTargetNode = null;
		m_movementPath = new Stack<Node>();
	}

	private void Start()
	{
		Grid.m_Instance.SetUnit(gameObject);
		m_currentTargetNode = Grid.m_Instance.GetNode(transform.position);
	}

	private void Update()
	{
		if (GameManager.m_Instance.GetCurrentTurn() == Allegiance.Enemy)
		{
			foreach (Unit unit in UnitsManager.m_Instance.m_PlayerUnits)
			{
				if (unit.GetAllegiance() == Allegiance.Player)
				{
					Grid.m_Instance.FindPath(transform.position, unit.transform.position, out m_movementPath, out m_movementCost);

					m_currentState = State.Moving;

					if (m_currentState == State.Moving)
						SetMovementPath(m_movementPath);

					for (int i = 0; i < 4; ++i)
					{
						if (Grid.m_Instance.GetNode(unit.transform.position).adjacentNodes[i].unit.m_Allegiance != m_Allegiance)
						{
							m_currentState = State.Attacking;

							if (m_currentState == State.Attacking)
							{

							}
						}
					}
				}
			}
		}
	}
}
