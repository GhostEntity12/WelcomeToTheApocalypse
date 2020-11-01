using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    public GameObject m_EnemyGroupToActivate;
    private Unit[] m_EnemiesToActivate = new Unit[] { };

    public TextAsset scene;


    private void Awake()
    {
        // Remove empty slots
        m_EnemiesToActivate = m_EnemyGroupToActivate.GetComponentsInChildren<Unit>();
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
            }

            gameObject.SetActive(false);
        }
    }
}
