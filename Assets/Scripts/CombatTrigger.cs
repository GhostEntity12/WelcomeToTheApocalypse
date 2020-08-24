using System.Collections.Generic;
using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    public List<Unit> m_TriggerEnemyEncounter = new List<Unit>();

    public TextAsset scene;

    private void OnTriggerEnter(Collider other)
    {
        if (other?.GetComponent<Unit>()?.GetAllegiance() == Allegiance.Player)
        {
            // Give enemies in m_TriggerEnemyEncounter to the game manager for them to take turns.

            if (scene)
            {
                UIManager.m_Instance.SwapToDialogue(scene);
            }

            gameObject.SetActive(false);
        }
    }
}
