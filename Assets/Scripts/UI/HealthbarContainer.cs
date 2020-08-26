using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthbarContainer : MonoBehaviour
{
    public Image m_HealthbarImage = null;

    public TextMeshProUGUI m_HealthChangeIndicator = null;

    public void SetChildrenActive(bool active)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}
