using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamManager : MonoBehaviour
{
	public static RoamManager m_Instance;

	private PlayerManager m_PlayerManager;

    // Start is called before the first frame update
    void Awake()
    {
        m_Instance = this;
    }

	void Start()
	{
		m_PlayerManager = PlayerManager.m_Instance;
	}

    // Update is called once per frame
    public void RoamUpdate()
    {
        m_PlayerManager.PlayerRoamUpdate();
    }
}
