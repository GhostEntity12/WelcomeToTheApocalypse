using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int m_StartingHealth = 10;
    private int m_CurrentHealth;

    public int m_StartingMovement = 6;
    private int m_CurrentMovement;

    public List<Skill> m_Skills = new List<Skill>();

    public StatusEffect m_PassiveEffect = null;

    private List<InflictableStatus> m_StatusDebuffs = new List<InflictableStatus>();

    void Awake()
    {
        m_CurrentHealth = m_StartingHealth;

        m_CurrentMovement = m_StartingMovement;
    }

    void Update()
    {
        
    }
}
