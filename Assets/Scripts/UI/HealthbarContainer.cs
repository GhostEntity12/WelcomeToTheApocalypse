using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthbarContainer : MonoBehaviour
{
    public Image m_HealthbarImage = null;

    public TextMeshProUGUI m_HealthChangeIndicator = null;

    public bool m_isMagnetic = false;

    public Unit u;

    Camera m_MainCam;

    private void Awake()
    {
        m_MainCam = Camera.main;
    }

    public void SetChildrenActive(bool active)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }
    }

    private void Update()
    {
        if (m_isMagnetic)
        {
            transform.position = m_MainCam.WorldToScreenPoint(u.m_HealthbarPosition.position);
        }
    }
}
