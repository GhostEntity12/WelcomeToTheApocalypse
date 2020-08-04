using Ghost;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemInput : MonoBehaviour
{
    // Start is called before the first frame update

    Camera c;

    Unit u;
    void Start()
    {
        c = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Physics.Raycast(c.ScreenPointToRay(Input.mousePosition), out RaycastHit hit0))
        //{
        //    Debug.LogWarning(hit0.transform.name);
        //}
        if (Input.GetMouseButton(0) && Physics.Raycast(c.ScreenPointToRay(Input.mousePosition),out RaycastHit hit1, Mathf.Infinity, 1<< 11))
        {
            if (u)
            {

                foreach (var item in u.m_MovableNodes)
                {
                    item.m_tile.SetActive(false);
                }

            }
            u = hit1.transform.GetComponent<Unit>();


            u.m_MovableNodes = BFS.GetNodesWithinRadius(u.GetCurrentMovement(), Grid.m_Instance.GetNode(u.transform.position));
            Grid.m_Instance.GetArea(u.GetCurrentMovement(), u.gameObject);
        }
        else if (Input.GetMouseButton(0) && Physics.Raycast(c.ScreenPointToRay(Input.mousePosition),out RaycastHit hit2, Mathf.Infinity, 1<< 10))
        {
            foreach (var item in u.m_MovableNodes)
            {
                item.m_tile.SetActive(false);
            }
            u.SetTargetPosition(hit2.transform.position);
        }
    }
}
