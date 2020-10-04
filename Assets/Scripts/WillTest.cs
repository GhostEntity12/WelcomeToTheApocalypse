using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WillTest : MonoBehaviour
{
    public Transform m_Destination;
    public float m_Speed = 10;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.version.ToLower().Contains("Gold") && Application.version.ToLower().Contains("1.0"))
        {
            Destroy(m_Destination.gameObject);
            Destroy(gameObject);
        }

        if (Random.Range(0f, 1f) > 0.3f)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, m_Destination.position, m_Speed * Time.deltaTime) ;
    }
}
