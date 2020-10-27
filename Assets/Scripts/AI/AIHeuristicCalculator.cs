using System.Collections.Generic;
using UnityEngine;

public enum AIHeuristics
{
    Move,
    Attack,
    Heal
}

[CreateAssetMenu(menuName = "AI Heuristic Calculator")]
public class AIHeuristicCalculator : ScriptableObject
{
    public List<AIHeuristics> m_AIActionHeuristics = new List<AIHeuristics>();

    public float m_KillPoints = 10;
}