using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum Outcome { Win, Loss }

public class CrawlDisplay : MonoBehaviour
{
	[SerializeField]
	float m_TimeBetweenLines = 0.5f;
	[SerializeField]
	float m_LineFadeTime = 1f;

	[Space(20)]
	public TextMeshProUGUI m_Display;
	public CanvasGroup m_Prompt;

	int m_CurrentScreen = 0;
	bool m_AcceptingInput = false;
	string[] m_ScriptLines;
	private List<List<string>> m_LinesByScreen = new List<List<string>>();

	public Action m_OnEndCrawlEvent;

	public void LoadCrawl(TextAsset script)
	{
		m_Display.text = string.Empty;
		m_ScriptLines = script.text.Split(
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
		if (UIManager.m_Instance)
		{
			UIManager.m_Instance.m_ActiveUI = true;
			LeanTween.alphaCanvas(UIManager.m_Instance.m_BlackScreen, 1, 2).setOnComplete(StartDisplay);
		}
		else StartDisplay();
	}

	public IEnumerator DisplayScreen(int screen)
	{
		LeanTween.alphaCanvas(m_Prompt, 0, 0.2f);
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
				continue;
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
		LeanTween.alphaCanvas(m_Prompt, 1, 0.5f);
	}

	private void Update()
	{
		if (GetAnyKeyDown(KeyCode.Return, KeyCode.Space, KeyCode.Mouse0) && m_AcceptingInput)
		{
			if (m_CurrentScreen < m_LinesByScreen.Count)
			{
				m_AcceptingInput = false;
				StartDisplay();
			}
			else
			{
				m_OnEndCrawlEvent?.Invoke();
				m_OnEndCrawlEvent = null;
				Debug.Log($"Finished crawl");
			}
		}
	}

	void StartDisplay() => StartCoroutine(DisplayScreen(m_CurrentScreen));

	bool GetAnyKeyDown(params KeyCode[] aKeys)
	{
		foreach (var key in aKeys)
			if (Input.GetKeyDown(key))
				return true;
		return false;
	}
}
