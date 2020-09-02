using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AIHeuristics
{
    Move,
    Attack,
    Heal
}

[CreateAssetMenu(fileName = "AI Heuristic Calculator", menuName = "AI Heuristic Calculator", order = 2)]
public class AIHeuristicCalculator : ScriptableObject
{
    public List<AIHeuristics> m_AIActionHeuristics = new List<AIHeuristics>();

    public float m_KillPoints = 10;
}