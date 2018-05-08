using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class AttachMicrophone : MonoBehaviour {

	public AudioSource audioSource {
		get { return _audioSource ?? (_audioSource = GetComponent<AudioSource>()); }
	}
	private AudioSource _audioSource = null;

	private const int samplingFrequency = 44100;
	
	void Awake () {
		audioSource.clip = Microphone.Start(null, true, 1, samplingFrequency);
		while (!(Microphone.GetPosition(null) > 0));
		audioSource.Play();
	}
}
