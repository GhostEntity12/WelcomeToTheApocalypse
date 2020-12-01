﻿using UnityEngine;

public class Cinematics : MonoBehaviour
{
    public Transform m_TrailerConversationPosition;

    [Header("Death")]
    public Unit m_Death;
    public Unit m_DeathTarget;
    public float m_DeathDelay = 0.3645f;
    public Transform m_DeathPosition;
    Animator m_DeathEnemyAnim;
    private int m_DefaultHash;

    public Transform m_DeathDialoguePosition;

    [Header("Pestilence")]
    public Animator m_Holder;
    public Unit m_Pestilence;
    public Transform m_PestilenceTarget;
    public float m_PestilenceDelay;
    public Unit[] m_PestilenceUnits;

    public Transform m_PestilenceDialoguePosition;

    [Header("Famine")]
    public Unit m_Famine;
    public float m_FamineDelay = 0.8f;

    public Transform m_FamineDialoguePosition;

    bool m_Prepped = false;

    Animator m_Anim;

    // Start is called before the first frame update
    private void Start()
    {
        m_Anim = GetComponent<Animator>();
        m_Death.gameObject.SetActive(false);
        m_Pestilence.gameObject.SetActive(false);
    }

    void Prep()
    {
        m_Death.gameObject.SetActive(true);
        m_Pestilence.gameObject.SetActive(true);
        AIManager.m_Instance.EnableUnits(new Unit[] { m_DeathTarget });
        AIManager.m_Instance.EnableUnits(m_PestilenceUnits);
        AIManager.m_Instance.EnableUnits(new Unit[] { m_Famine });

        m_DeathEnemyAnim = m_DeathTarget.GetComponent<Animator>();
        m_DefaultHash = m_Anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        m_Prepped = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad9)) 
        {
            if (!m_Prepped) Prep();
            transform.parent = null;
            GameManager.m_Instance.m_SelectedUnit = m_Famine;

            m_Anim.SetTrigger("Famine");
            LeanTween.delayedCall(m_FamineDelay, () => m_Famine.ActivateSkill(m_Famine.GetSkill(2), Grid.m_Instance.GetNode(m_Famine.transform.position)));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            if (!m_Prepped) Prep();
            transform.parent = m_Holder.transform;
            GameManager.m_Instance.m_SelectedUnit = m_Pestilence;

            Grid.m_Instance.RemoveUnit(Grid.m_Instance.GetNode(m_Death.transform.position));
            m_Death.transform.position = m_PestilenceTarget.position;
            m_Death.transform.LookAt(m_Pestilence.transform);
            Grid.m_Instance.SetUnit(m_Death);

            m_Anim.SetTrigger("Pestilence");
            m_Holder.SetTrigger("Trigger");
            LeanTween.delayedCall(m_PestilenceDelay, () => m_Pestilence.ActivateSkill(m_Pestilence.GetSkill(1), Grid.m_Instance.GetNode(m_PestilenceTarget.position)));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            if (!m_Prepped) Prep();
            transform.parent = null;
            GameManager.m_Instance.m_SelectedUnit = m_Death;

            Grid.m_Instance.RemoveUnit(Grid.m_Instance.GetNode(m_Death.transform.position));
            m_Death.transform.position = m_DeathPosition.position;
            m_Death.transform.LookAt(m_DeathTarget.transform);
            Grid.m_Instance.SetUnit(m_Death);
            m_DeathTarget.gameObject.SetActive(true);
            m_DeathTarget.SetCurrentHealth(m_DeathTarget.GetStartingHealth());
            m_DeathEnemyAnim.Play(m_DefaultHash, 0);
            Grid.m_Instance.SetUnit(m_DeathTarget);

            m_Anim.SetTrigger("Death");
            LeanTween.delayedCall(m_DeathDelay, () => m_Death.ActivateSkill(m_Death.GetSkill(1), Grid.m_Instance.GetNode(m_DeathTarget.transform.position)));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            if (!m_Prepped) Prep();
            transform.parent = m_FamineDialoguePosition;
            transform.position = m_FamineDialoguePosition.transform.position;
            transform.rotation = m_FamineDialoguePosition.transform.rotation;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            if (!m_Prepped) Prep();
            transform.parent = m_PestilenceDialoguePosition;
            transform.position = m_PestilenceDialoguePosition.transform.position;
            transform.rotation = m_PestilenceDialoguePosition.transform.rotation;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            if (!m_Prepped) Prep();
            transform.parent = m_DeathDialoguePosition;
            transform.position = m_DeathDialoguePosition.transform.position;
            transform.rotation = m_DeathDialoguePosition.transform.rotation;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            transform.parent = m_TrailerConversationPosition;
            transform.position = m_TrailerConversationPosition.transform.position;
            transform.rotation = m_TrailerConversationPosition.transform.rotation;
            LeanTween.delayedCall(2f, () => m_TrailerConversationPosition.GetComponent<Collider>().enabled = true);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            if (!m_Prepped) Prep();
            transform.parent = null;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            transform.parent = null;
            m_Anim.SetTrigger("Establishing0");
        }
        else if (Input.GetKeyDown(KeyCode.KeypadDivide))
        {
            transform.parent = null;
            m_Anim.SetTrigger("Establishing1");
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            transform.parent = null;
            m_Anim.SetTrigger("Establishing2");
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            transform.parent = null;
            m_Anim.SetTrigger("Establishing3");
        }
    }
}