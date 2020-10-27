using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PrematureTurnEndDisplay : MonoBehaviour
{
    public TextMeshProUGUI m_UnitsStillWithTurnsText = null;

    private readonly string m_DefaultString =   "You still have units who can act!\n" +
                                                "Are you sure you want to end your turn?\n\n" +
                                                "Units with actions left:<size=32.5>\n";

    public void UpdateText()
    {
        string unitsWithActions = string.Empty;

        foreach (Unit unit in UnitsManager.m_Instance.m_PlayerUnits.Where(u => u.GetActionPoints() > 0 || u.GetCurrentMovement() > 0))
        {
            unitsWithActions += unit.name + "\n";
        }

        m_UnitsStillWithTurnsText.text = m_DefaultString + unitsWithActions;
    }
}