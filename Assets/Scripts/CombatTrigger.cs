using System.Collections.Generic;
using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    public List<Unit> m_TriggerEnemyEncounter = new List<Unit>();

    public TextAsset scene;

    private void OnTriggerEnter(Collider other)
    {
        // Give enemies in m_TriggerEnemyEncounter to the game manager for them to take turns.

        if (scene)
        {
            DialogueManager.instance.TriggerDialogue(scene);
        }
    }
}
