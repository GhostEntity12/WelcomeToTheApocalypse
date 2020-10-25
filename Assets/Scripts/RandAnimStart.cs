using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandAnimStart : MonoBehaviour
{
    public bool m_RandomiseSpeed;

    // Start is called before the first frame update
    void Start()
    {
        if (m_RandomiseSpeed)
        {
            GetComponent<Animator>().SetFloat("Speed", Random.Range(0.7f, 1.5f));
        }
        GetComponent<Animator>().Play(0, 0, Random.Range(0, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length));
    }
}
