using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointCounter : MonoBehaviour
{
    /// <summary>
    /// List of objects that will serve as counters for the unit's action points.
    /// </summary>
    private List<Image> m_Counters = new List<Image>();

    public Color m_ActiveColor;
    public Color m_InactiveColor;

    void Awake()
    {
        // Get all the children of this object.
        // These are the counters for the unit's action points.
        for(int i = 0; i < transform.childCount; ++i)
        {
            Image child = transform.GetChild(i).GetComponent<Image>();

            // Make sure the child is valid.
            if (child)
                m_Counters.Add(child);
        }
    }

    /// <summary>
    /// Update the action points counter to show as many action points as the unit has avaliable.
    /// </summary>
    public void UpdateActionPointCounter()
    {
        for (int i = m_Counters.Count; i > GameManager.m_Instance.GetSelectedUnit().GetActionPoints(); --i)
        {
            m_Counters[i - 1].color = m_InactiveColor;
        }
    }

    /// <summary>
    /// Reset the action point counter.
    /// </summary>
    public void ResetActionPointCounter()
    {
        for(int i = 0; i < m_Counters.Count; ++i)
        {
            m_Counters[i].gameObject.SetActive(i < GameManager.m_Instance.GetSelectedUnit().m_StartingActionPoints);
            m_Counters[i].color = m_ActiveColor;
        }
    }
}