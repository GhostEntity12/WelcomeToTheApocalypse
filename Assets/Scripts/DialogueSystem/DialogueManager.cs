using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
	public static DialogueManager instance;
	public KeyCode[] ProgressionKeys = new KeyCode[1] { KeyCode.Return };

	public bool dialogueActive;
	bool isDisplayingText;
	IEnumerator displayDialogueCoroutine;

	[Header("UI")]
	[Tooltip("Fow quickly the UI fades in")]
	public float uiFadeInSpeed = 0.4f;
	[Space(15)]
	[Tooltip("The object which holds characters' names")]
	public TextMeshProUGUI nameBox;
	[Tooltip("The object which holds characters' dialogue")]
	public TextMeshProUGUI dialogueBox;
	[Tooltip("The container for the object which holds a character's name")]
	public RectTransform nameHolder;
	private Vector2 namePos;
	[Tooltip("The left-side object which holds characters' image")]
	public Image bustL;
	[Tooltip("The right-side object which holds characters' image")]
	public Image bustR;
	public GameObject skipDialogueDisplay;
	public CanvasGroup darkenedBackground;

	[Header("Text Display Options")]
	[Tooltip("The length of time to wait between displaying characters")]
	public float delay = 0.05f;

	[Header("File")]
	[Tooltip("The scene to load")]
	public TextAsset sceneName;
	public Queue<TextAsset> sceneQueue = new Queue<TextAsset>();
	public Queue<Action> onFinishDialogueActions = new Queue<Action>();
	[Tooltip("Whether to clear the scene after it has run")]
	public bool clearAfterScene;
	private string[] parsedText;

	[Header("Characters")]
	public Sprite defaultCharacterSprite;
	readonly Dictionary<string, CharacterPortraitContainer> characterDictionary = new Dictionary<string, CharacterPortraitContainer>();

	CharacterPortraitContainer leftCharacter;
	CharacterPortraitContainer rightCharacter;

	string[] fileLines;
	int currentLine;
	string characterName, characterDialogue, characterExpression;
	CharacterPortraitContainer currentCharacter;
	Vector2 defaultPortraitSize;

	private enum Side { Left, Right }

	private void Awake()
	{
		instance = this;
		foreach (CharacterPortraitContainer characterPortraits in Resources.LoadAll<CharacterPortraitContainer>("Characters")) // Creates the dictionary
		{
			characterDictionary.Add(characterPortraits.name, characterPortraits);
		}
		ClearDialogueBox(); // Clears the dialogue box, just in case
		namePos = nameHolder.anchoredPosition;
		defaultPortraitSize = bustL.rectTransform.sizeDelta;
	}

	/// <summary>
	/// Clears the dialogue box's name, dialogue and image
	/// </summary>
	void ClearDialogueBox()
	{
		bustL.sprite = null;
		bustR.sprite = null;
		nameBox.text = string.Empty;
		dialogueBox.text = string.Empty;
		currentCharacter = null;
	}

	/// <summary>
	/// Loads the next line from the fileLines array
	/// </summary>
	void LoadNewLine()
	{
		// Split the line into its components and store them
		parsedText = fileLines[currentLine].Split('|');
		currentLine++;

		bool sameChar = false;
		// Check if it's the same character
		if (currentCharacter)
		{
			sameChar = currentCharacter.name == parsedText[0];
		}

		// Set the variables
		currentCharacter = characterDictionary[parsedText[0]];
		characterName = currentCharacter.name;
		characterExpression = parsedText[1].ToLower();
		characterDialogue = parsedText[2];

		if (sameChar)
		{
			StartDisplaying();
		}
		else
		{
			UIManager.m_Instance.SwapDialogue(currentCharacter.m_UiData);
		}

	}
	public void StartDisplaying()
	{
		isDisplayingText = true; // Marks the system as typing out letters. Used to determine what happens when pressing enter
								 // Set the portrait
		try
		{
			switch (parsedText[3].ToLower()[1])
			{
				case 'l':
					ManageDialoguePortrait(Side.Left);
					break;
				case 'r':
					ManageDialoguePortrait(Side.Right);
					break;
				default:
					throw new IndexOutOfRangeException();
			}
		}
		catch (IndexOutOfRangeException)
		{
			if (leftCharacter == currentCharacter || leftCharacter == null)
			{
				ManageDialoguePortrait(Side.Left);
			}
			else
			{
				ManageDialoguePortrait(Side.Right);
			}
		}

		if (leftCharacter == rightCharacter)
		{
			Debug.LogError($"{characterName} is taking up both sides!");
		}

		// Clears the dialogue box
		dialogueBox.text = string.Empty;

		// Sets the name box
		nameBox.text = characterName;

		// Declare and then start the coroutine/IEnumerator so it can be stopped later
		displayDialogueCoroutine = DisplayDialogue(characterDialogue);
		StartCoroutine(displayDialogueCoroutine);
	}

	private void ManageDialoguePortrait(Side side)
	{
		// Set references
		CharacterPortraitContainer character;
		Image bust;
		UIManager.TweenedElement speaker;
		CharacterPortraitContainer otherCharacter;
		Image otherBust;
		UIManager.TweenedElement otherSpeaker;

		if (side == Side.Left)
		{
			character = leftCharacter;
			bust = bustL;
			speaker = UIManager.m_Instance.m_LeftSpeaker;
			leftCharacter = currentCharacter;
			otherCharacter = rightCharacter;
			otherBust = bustR;
			otherSpeaker = UIManager.m_Instance.m_RightSpeaker;
			nameHolder.anchoredPosition = namePos;
		}
		else
		{
			character = rightCharacter;
			bust = bustR;
			speaker = UIManager.m_Instance.m_RightSpeaker;
			rightCharacter = currentCharacter;
			otherCharacter = leftCharacter;
			otherBust = bustL;
			otherSpeaker = UIManager.m_Instance.m_LeftSpeaker;
			nameHolder.anchoredPosition = new Vector2(-namePos.x, namePos.y);
		}

		// Grey out the other character
		LeanTween.color(otherBust.rectTransform, Color.gray, 0.1f);
		LeanTween.color(bust.rectTransform, Color.white, 0.1f);

		// Swap portraits
		if (character == currentCharacter)
		{
			bust.sprite = GetCharacterPortrait(currentCharacter, characterExpression);
			// Grow the active speaker
			LeanTween.size(otherBust.rectTransform, defaultPortraitSize, 0.1f);
			LeanTween.size(bust.rectTransform, defaultPortraitSize * 1.1f, 0.1f);
		}
		else
		{
			LeanTween.color(otherBust.rectTransform, Color.gray, 0.1f);
			LeanTween.color(bust.rectTransform, Color.white, 0.1f);
			character = currentCharacter;
			UIManager.m_Instance.SlideElement(speaker, UIManager.ScreenState.Offscreen, () =>
			{
				bust.sprite = GetCharacterPortrait(currentCharacter, characterExpression);
				UIManager.m_Instance.SlideElement(speaker, UIManager.ScreenState.Onscreen);
				// Grow the active speaker
				LeanTween.size(otherBust.rectTransform, defaultPortraitSize, 0.1f);
				LeanTween.size(bust.rectTransform, defaultPortraitSize * 1.1f, 0.1f);
			});
		}
	}

	/// <summary>
	/// Converts the expression string into the associated Sprite variable in the given character. 
	/// Returns the unknown character sprite if no associated character is found
	/// </summary>
	/// <returns></returns>
	Sprite GetCharacterPortrait(CharacterPortraitContainer character, string expression)
	{
		try { return (Sprite)typeof(CharacterPortraitContainer).GetField(expression).GetValue(character); }
		catch (Exception exception)
		{
			if (exception is KeyNotFoundException) // Character not found - return default character sprite.
			{
				return defaultCharacterSprite;
			}
			else if (exception is NullReferenceException) // Expression not found - return neutral.
			{
				return (Sprite)typeof(CharacterPortraitContainer).GetField("neutral").GetValue(character);
			}
			else throw exception;

		}
	}

	/// <summary>
	/// Displays a given string letter by letter
	/// </summary>
	/// <param name="text">The text to display</param>
	/// <returns></returns>
	IEnumerator DisplayDialogue(string text)
	{
		for (int i = 0; i < text.Length; i++) // Adds a letter to the textbox then waits the delay time
		{
			if (text[i] == '<') // If the letter is an opening tag character, autofill the rest of the tag
			{
				int indexOfClose = text.Substring(i).IndexOf('>');
				if (indexOfClose == -1)
				{
					dialogueBox.text += text[i];
					yield return new WaitForSeconds(delay);
					continue;
				}
				dialogueBox.text += text.Substring(i, indexOfClose);
				i += indexOfClose - 1;
				continue;
			}

			dialogueBox.text += text[i];
			yield return new WaitForSeconds(delay);
		}

		isDisplayingText = false; // Marks the system as no longer typing out
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && dialogueActive)
		{
			skipDialogueDisplay.SetActive(!skipDialogueDisplay.activeInHierarchy);
			return;
		}
		if (skipDialogueDisplay.activeInHierarchy) return;
		if (GetAnyKeyDown(ProgressionKeys) && dialogueActive) // If enter is pressed and the textboxes are visible
		{
			if (isDisplayingText) // If the system is currently typing out, finish and return
			{
				StopCoroutine(displayDialogueCoroutine); // Stops the typing out
				dialogueBox.text = characterDialogue; // Fills the textbox with the entirety of the character's line
				isDisplayingText = false; // Marks the system as no longer typing out
				return;
			}
			else if (currentLine >= fileLines.Length) // If there are no more lines
			{
				EndScene();
			}
			else
			{
				LoadNewLine(); // Loads the next line
			}
		}
	}

	public void EndScene()
	{
		Debug.Log($"<color=#5cd3e0>[Dialogue]</color> Finished dialogue {sceneName.name}");
		LeanTween.alphaCanvas(darkenedBackground, 0.0f, 0.2f);
		StopCoroutine(displayDialogueCoroutine); // Stops the typing out
		dialogueBox.text = characterDialogue; // Fills the textbox with the entirety of the character's line
		isDisplayingText = false; // Marks the system as no longer typing out

		currentLine = fileLines.Length;

		if (clearAfterScene) // Clears the scene if told to
		{
			sceneName = null;
		}
		UIManager.m_Instance.SwapFromDialogue();
		leftCharacter = null;
		rightCharacter = null;
		UIManager.m_Instance.ShowTurnIndicator();
		UIManager.m_Instance.m_ActiveUI = false;
		dialogueActive = false;
		LeanTween.size(bustL.rectTransform, defaultPortraitSize, 0.1f);
		LeanTween.size(bustR.rectTransform, defaultPortraitSize, 0.1f);
		UIManager.m_Instance.SlideElement(UIManager.m_Instance.m_LeftSpeaker, UIManager.ScreenState.Offscreen, ClearDialogueBox);
		UIManager.m_Instance.SlideElement(UIManager.m_Instance.m_RightSpeaker, UIManager.ScreenState.Offscreen);

		onFinishDialogueActions.Dequeue()?.Invoke();
		if (sceneQueue.Count > 0)
		{
			TriggerDialogue(sceneQueue.Dequeue());
		}
	}

	public void QueueDialogue(TextAsset _sceneName, Action onEndAction = null)
	{
		Debug.Log($"<color=#5cd3e0>[Dialogue]</color> Queuing dialogue {_sceneName.name}");
		sceneQueue.Enqueue(_sceneName);
		onFinishDialogueActions.Enqueue(onEndAction);
		if (!dialogueActive)
		{
			TriggerDialogue(sceneQueue.Dequeue());
		}
	}

	public void TriggerDialogue(TextAsset _sceneName)
	{
		UIManager.m_Instance.m_ActiveUI = true;
		LeanTween.alphaCanvas(darkenedBackground, 0.9f, 0.4f);
		UIManager.m_Instance.HideTurnIndicator();
		dialogueActive = true;
		ClearDialogueBox();
		sceneName = _sceneName;
		Debug.Log($"<color=#5cd3e0>[Dialogue]</color> Starting dialogue {sceneName.name}");

		// Loads the file into memory
		TextAsset file = sceneName;

		// Throws error no matching file exists
		if (file == null)
		{
			Debug.LogError($"Dialogue file not found!");
			return;
		}
		if (!file.name.StartsWith("DIA_"))
		{
			Debug.LogError($"\"{file.name}\" isn't a dialogue file!");
			sceneName = null;
			return;
		}

		// Splits the input on its new lines
		fileLines = file.text.Split(
			new[] { "\r\n", "\r", "\n", Environment.NewLine },
			StringSplitOptions.None
			);
		currentLine = 0;
		LoadNewLine();
	}

	bool GetAnyKeyDown(params KeyCode[] aKeys)
	{
		foreach (var key in aKeys)
			if (Input.GetKeyDown(key))
				return true;
		return false;
	}
}