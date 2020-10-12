using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandAnimStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().SetFloat("Speed", Random.Range(0.7f, 1.5f));
        GetComponent<Animator>().Play("Paper1", 0, Random.Range(0, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length));
    }
}
