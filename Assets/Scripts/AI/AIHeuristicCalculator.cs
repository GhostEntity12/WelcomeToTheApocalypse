using UnityEngine;

public enum MovementType
{
	Chase,
	Guard,
	Group,
	Mixed
}

[CreateAssetMenu(menuName = "AI Heuristic Calculator")]
public class AIHeuristicCalculator : ScriptableObject
{
	public float m_KillPoints = 10;

	public MovementType m_MovementType;

	public float m_DamageWeighting = 1;
	public float m_HealWeighting = 1;
	public float m_StatusWeighting = 1;

}