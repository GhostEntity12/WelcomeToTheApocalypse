using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrematureTurnEndDisplay : MonoBehaviour
{
    public TextMeshProUGUI m_UnitsStillWithTurnsText = null;

    private readonly string m_DefaultString =   "You still have units who can act!\n" +
                                                "Are you sure you want to end your turn?\n\n" +
                                                "Units with actions left: \n<size=32.5>";

    public void UpdateText()
    {
        string unitsWithActions = string.Empty;

        foreach (Unit unit in UnitsManager.m_Instance.m_PlayerUnits)
        {
            unitsWithActions += unit.name + "\n";
        }

        m_UnitsStillWithTurnsText.text = m_DefaultString + unitsWithActions;
    }
}