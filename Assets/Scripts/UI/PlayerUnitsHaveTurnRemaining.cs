using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUnitsHaveTurnRemaining : MonoBehaviour
{
    public TextMeshProUGUI m_UnitsStillWithTurnsText = null;

    public void ResetText()
    {
        m_UnitsStillWithTurnsText.text = "";
    }

    public void DismissPrompt()
    {
        ResetText();
        gameObject.SetActive(false);
    }
}