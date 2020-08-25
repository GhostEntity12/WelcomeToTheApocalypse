using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHealthBarCanvas : MonoBehaviour
{
    /// <summary>
    /// The healthbar to be put above all unit's heads
    /// </summary>
    public GameObject m_HealthbarTemplate = null;

    /// <summary>
    /// List of units that need a healthbar to be placed above them.
    /// </summary>
    public List<Unit> m_UnitsForHealthbars = new List<Unit>();

    private void Awake()
    {
        foreach(Unit u in m_UnitsForHealthbars)
        {
            u.SetHealthbar(Instantiate(m_HealthbarTemplate, transform));
        }
    }

    public void ShowHealthbar(Unit unitHealth)
    {
        foreach(Unit u in m_UnitsForHealthbars)
        {
            if (u == unitHealth)
            {
                u.SetHealthbarActive();
            }
        }
    }
}
