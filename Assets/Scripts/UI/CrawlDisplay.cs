using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum Outcome { Win, Loss }

public class CrawlDisplay : MonoBehaviour
{
	public TextAsset m_FailScript;
	public TextAsset m_WinScript;
	[SerializeField]
	float m_TimeBetweenLines = 0.5f;
	[SerializeField]
	float m_LineFadeTime = 1f;

	[Space(20)]
	public TextMeshProUGUI m_Display;

	int m_CurrentScreen = 0;
	bool m_AcceptingInput = false;
	string[] m_ScriptLines;
	List<List<string>> m_LinesByScreen = new List<List<string>>();

	public Action m_OnEndCrawlEvent;

	public RectTransform m_CrawlButtons;

	public void LoadCrawl(Outcome outcome)
	{

		UIManager.m_Instance.m_ActiveUI = true;
		m_Display.text = string.Empty;
		m_ScriptLines = (outcome == Outcome.Win ? m_WinScript : m_FailScript).text.Split(
			new[] { "\r\n", "\r", "\n", Environment.NewLine },
			StringSplitOptions.None
			);

		int screen = 0;
		m_LinesByScreen.Add(new List<string>());

		foreach (string line in m_ScriptLines)
		{
			if (line == "<br>")
			{
				screen++;
				m_LinesByScreen.Add(new List<string>());
			}
			else
			{
				m_LinesByScreen[screen].Add(line);
			}
		}
		LeanTween.alphaCanvas(UIManager.m_Instance.m_BlackScreen, 1, 2).setOnComplete(StartDisplay);
	}

	public IEnumerator DisplayScreen(int screen)
	{
		string oldString = string.Empty;

		// Fade old text out (if it exists)
		if (!string.IsNullOrEmpty(m_Display.text))
		{
			oldString = m_Display.text;

			int alpha = 255;
			while (alpha > 0)
			{
				m_Display.text = $"<alpha=#{alpha:X2}>{oldString}\n"; // Display existing text,
				alpha = Math.Max(0, alpha - Mathf.CeilToInt(256 / (m_LineFadeTime / Time.fixedDeltaTime)));
				yield return new WaitForFixedUpdate();
			}

			// Clear the strings
			oldString = string.Empty;
			m_Display.text = oldString;
		}
		// Load in lines sequentially
		foreach (string line in m_LinesByScreen[screen])
		{
			if (line == "<sprite=\"ProgressArrow\" index=0>")
			{
				oldString += line + "\n";
				m_Display.text = oldString; // Update the textbox
			}
			int alpha = 0;

			// Fade to 100% alpha
			while (alpha < 256)
			{
				m_Display.text = oldString + $"<alpha=#{alpha:X2}>{line}\n"; // Display existing text, with tag and new text 
				alpha = Math.Min(256, alpha + Mathf.CeilToInt(256 / (m_LineFadeTime / Time.fixedDeltaTime))); // Increment alpha
				yield return new WaitForFixedUpdate();
			}
			oldString += line + "\n"; // Update the "existing" text. This also scrubs the tag.
			m_Display.text = oldString; // Update the textbox
			yield return new WaitForSeconds(m_TimeBetweenLines);
		}
		m_AcceptingInput = true;
		m_CurrentScreen++;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return) && m_AcceptingInput)
		{
			if (m_CurrentScreen < m_LinesByScreen.Count)
			{
				m_AcceptingInput = false;
				StartDisplay();
			}
			else
			{
				// TODO: Load next scene I guess
				m_OnEndCrawlEvent?.Invoke();
				m_OnEndCrawlEvent = null;
				Debug.Log($"Finished crawl");
			}
		}
	}

	void StartDisplay()
	{
		StartCoroutine(DisplayScreen(m_CurrentScreen));
	}
}
