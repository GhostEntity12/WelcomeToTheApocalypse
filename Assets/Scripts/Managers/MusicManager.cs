using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public enum Horseman
{
    Death,
    Pestilence,
    Famine,
    War
}

public class MusicManager : MonoBehaviour
{
    [Range(0, 3)]
    public int DebugAdd;
    public static MusicManager m_Instance;
    [Range(0, 15)]
    public int m_HorsemanMask = 0b_0000;

    [EventRef]
    public string m_MusicEvent = "";
    public EventInstance m_MusicState;
    PARAMETER_ID m_MusicParameterID;

    private void Start()
    {
        if (m_Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            m_Instance = this;
            DontDestroyOnLoad(this);
            m_MusicState = RuntimeManager.CreateInstance(m_MusicEvent);
            m_MusicState.start();

            m_MusicState.getDescription(out EventDescription musicEventDescription);
            musicEventDescription.getParameterDescriptionByName("HorsemenNum", out PARAMETER_DESCRIPTION musicParameterDescription);
            m_MusicParameterID = musicParameterDescription.id;
        }
    }

    public void AddHorseman(Horseman horseman)
    {
        m_HorsemanMask |= 1 << (int)horseman;
        m_MusicState.setParameterByID(m_MusicParameterID, m_HorsemanMask);
    }

    public void RemoveHorseman(Horseman horseman)
    {
        m_HorsemanMask &= ~(1 << (int)horseman);
        m_MusicState.setParameterByID(m_MusicParameterID, m_HorsemanMask);
    }

    public void SetHorsemen(int horsemenMask)
    {
        m_HorsemanMask = horsemenMask;
        m_MusicState.setParameterByID(m_MusicParameterID, m_HorsemanMask);
    }
}