using System.Collections.Generic;
using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    public List<Unit> m_EnemiesToActivate = new List<Unit>();

    public TextAsset scene;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Unit>()?.GetAllegiance() == Allegiance.Player)
        {
            AIManager.m_Instance.EnableUnits(m_EnemiesToActivate);

            if (scene)
            {
                UIManager.m_Instance.SwapToDialogue(scene);
            }

            gameObject.SetActive(false);
        }
    }
}
