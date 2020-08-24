using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public bool dialogueActive;
    bool isDisplayingText;
    IEnumerator displayDialogueCoroutine;

    [Header("UI")]
    CanvasGroup canvasGroup;
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

    [Header("Text Display Options")]
    [Tooltip("The length of time to wait between displaying characters")]
    public float delay = 0.05f;

    [Header("File")]
    [Tooltip("The scene to load")]
    public TextAsset sceneName;
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

    private enum Side { Left, Right }

    private void Awake()
    {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>(); // Gets the canvas group to deal with fading opacity
        foreach (CharacterPortraitContainer characterPortraits in Resources.LoadAll<CharacterPortraitContainer>("Characters")) // Creates the dictionary
        {
            characterDictionary.Add(characterPortraits.name, characterPortraits);
        }
        ClearDialogueBox(); // Clears the dialogue box, just in case
        namePos = nameHolder.anchoredPosition;
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
            UIManager.m_Instance.SwapDialogue(UIManager.m_Instance.GetUIStyle(currentCharacter.name));
        }

    }
    public void StartDisplaying()
    {
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
        try { return (Sprite)character.GetType().GetField(expression).GetValue(character); }
        catch (KeyNotFoundException) { return defaultCharacterSprite; }
    }

    /// <summary>
    /// Displays a given string letter by letter
    /// </summary>
    /// <param name="text">The text to display</param>
    /// <returns></returns>
    IEnumerator DisplayDialogue(string text)
    {
        isDisplayingText = true; // Marks the system as typing out letters. Used to determine what happens when pressing enter

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
        if (Input.GetKeyDown(KeyCode.Return) && canvasGroup.interactable) // If enter is pressed and the textboxes are visible
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
                if (clearAfterScene) // Clears the scene if told to
                {
                    sceneName = null;
                }

                UIManager.m_Instance.SwapFromDialogue();
                dialogueActive = false;
                UIManager.m_Instance.SlideElement(UIManager.m_Instance.m_LeftSpeaker, UIManager.ScreenState.Offscreen, ClearDialogueBox);
                UIManager.m_Instance.SlideElement(UIManager.m_Instance.m_RightSpeaker, UIManager.ScreenState.Offscreen);
                leftCharacter = null;
                rightCharacter = null;
                sceneName = null;
                return;
            }
            else
            {
                LoadNewLine(); // Loads the next line
            }
        }
    }

    // DEBUG
    [ContextMenu("Trigger Dialogue")]
    void TriggerDialogue() => TriggerDialogue(sceneName);
    public void TriggerDialogue(TextAsset _sceneName)
    {
        dialogueActive = true;
        ClearDialogueBox();
        sceneName = _sceneName;

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

        canvasGroup.interactable = canvasGroup.blocksRaycasts = canvasGroup.alpha == 1;

        LoadNewLine();
        // Fade the canvas in
        //StartCoroutine(FadeCanvasGroup(canvasGroup, uiFadeInSpeed, canvasGroup.alpha, 1, PostFade));
    }

    ///// <summary>
    ///// Run via callback. Cleans up after fading.
    ///// </summary>
    //void PostFade()
    //{
    //    canvasGroup.interactable = canvasGroup.blocksRaycasts = canvasGroup.alpha == 1;
    //}
}