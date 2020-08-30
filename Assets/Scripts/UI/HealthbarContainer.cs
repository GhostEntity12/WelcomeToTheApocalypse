using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthbarContainer : MonoBehaviour
{
    public Image m_HealthbarImage = null;

    public TextMeshProUGUI m_HealthChangeIndicator = null;

    public bool m_IsMagnetic = false;

    public Unit u;

    Transform m_Transform;

    Camera m_MainCam;

    private void Start()
    {
        m_MainCam = Camera.main;
        m_Transform = u.m_HealthbarPosition;
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
        if (m_IsMagnetic)
        {
            transform.position = m_MainCam.WorldToScreenPoint(m_Transform.position);
        }
    }
}
