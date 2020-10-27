using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        m_Anim = Camera.main.GetComponent<Animator>();
    }

    /// <summary>
    /// Fade to black and start the game.
    /// </summary>
    public void StartGame()
    {
        m_Anim.SetTrigger("isMainGame");
        LeanTween.alphaCanvas(m_BlackScreen, 1.0f, 1.5f);
        LeanTween.delayedCall(1.5f, () => SceneManager.LoadScene("Famine_Split"));
    }

    /// <summary>
    /// Fade to black and quit the game.
    /// </summary>
    public void QuitGame()
    {
        LeanTween.alphaCanvas(m_BlackScreen, 1.0f, 1.0f);
        LeanTween.delayedCall(1.0f, Application.Quit);
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

    public void ToggleGlow(TextMeshPro text)
    {
        text.fontMaterial.SetFloat("_GlowPower", text.fontMaterial.GetFloat("_GlowPower") == 0 ? 1 : 0);
    }
}