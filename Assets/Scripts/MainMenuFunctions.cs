﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuFunctions : MonoBehaviour
{
    /// <summary>
    /// The camera's animator.
    /// </summary>
    private Animator m_Anim = null;

    /// <summary>
    /// Canvas group for fading to black.
    /// </summary>
    public CanvasGroup m_BlackScreen = null;

    /// <summary>
    /// On startup.
    /// </summary>
    void Awake()
    {
        m_Anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Fade to black and start the game.
    /// </summary>
    public void StartGame()
    {
        StartCoroutine(Ghost.Fade.FadeCanvasGroup(m_BlackScreen, 1.0f, 0.0f, 1.0f, SceneManager.LoadScene, "Famine_Split"));
    }

    /// <summary>
    /// Fade to black and quit the game.
    /// </summary>
    public void QuitGame()
    {
        StartCoroutine(Ghost.Fade.FadeCanvasGroup(m_BlackScreen, 1.0f, 0.0f, 1.0f, Application.Quit));
    }

    /// <summary>
    /// Get animator to move to credits.
    /// </summary>
    public void ViewCredits()
    {
        m_Anim.SetBool("isCredits", true);
    }

    /// <summary>
    /// Get animator to leave credits.
    /// </summary>
    public void LeaveCredits()
    {
        m_Anim.SetBool("isCredits", false);
    }
}