﻿using UnityEngine;

public class UnitHealthBarCanvas : MonoBehaviour
{
    /// <summary>
    /// The healthbar to be put above all unit's heads
    /// </summary>
    public GameObject m_HealthbarTemplate;

    private void Start()
    {
        foreach (Unit u in UnitsManager.m_Instance.m_AllUnits)
        {
            GameObject healthbar = Instantiate(m_HealthbarTemplate, transform);
            healthbar.name = $"{u.name} Healthbar";
            HealthbarContainer container = healthbar.GetComponent<HealthbarContainer>();
            container.SetUnit(u);
            container.UnitSetHealthbar();
            container.m_HealthChangeIndicator.GetComponent<HealthChangeIndicator>().Create();
        }
    }
}
