using System.Collections.Generic;
using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    public List<Unit> m_EnemiesToActivate = new List<Unit>();

    public TextAsset scene;


    private void Awake()
    {
        // Remove empty slots
        m_EnemiesToActivate.RemoveAll(m => m == null);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Unit>()?.GetAllegiance() == Allegiance.Player)
        {
            AIManager.m_Instance.EnableUnits(m_EnemiesToActivate);

            if (scene)
            {
                if (DialogueManager.instance.dialogueActive)
                {
                    Debug.LogError($"Dialogue manager is already displaying {DialogueManager.instance.sceneName}, can't start {scene}!");
                    return;
                }
                UIManager.m_Instance.SwapToDialogue(scene);
                Debug.Log(other.name, this);
            }

            gameObject.SetActive(false);
        }
    }
}
