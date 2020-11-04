using System.Collections.Generic;
using UnityEngine;

public class AudioSync : MonoBehaviour
{
	public AudioSource masterTrack;
	public List<AudioSource> slaveTracks;

	public float slaveVol = 0.6f;

	private void Awake()
	{
		//slaveTracks.volume = 0;
	}

	void FixedUpdate()
	{
		slaveTracks.ForEach(t => t.timeSamples = masterTrack.timeSamples);
	}
}
