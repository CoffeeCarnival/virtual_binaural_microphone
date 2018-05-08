using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RecordingSound))]
[RequireComponent(typeof(AudioSource))]
public class PlayRecordedSound : MonoBehaviour, ISetText
{

	private RecordingSound recordingSound { get { return _recordingSound ?? (_recordingSound = GetComponent<RecordingSound>()); } }
	private RecordingSound _recordingSound;
	private AudioSource audioSource { get { return _audioSource ?? (_audioSource = GetComponent<AudioSource>()); } }
	private AudioSource _audioSource;

	[SerializeField]
	private AttachMicrophone _microphone;

	private bool isSetupPlay { get; set; }

	public void OnClickPlayButton()
	{
		if (audioSource.isPlaying)
		{
			OnStopRecordedSound();
		}
		else
		{
			OnPlayRecordedSound();
		}
	}

	public void OnPlayRecordedSound()
	{
		if (audioSource.isPlaying || !recordingSound.isStandBy)
		{
			return;
		}
		isSetupPlay = false;
		_microphone.audioSource.mute = true;
		var filePath = recordingSound.outputPath + "\\" + recordingSound.fileName + ".wav";
		StartCoroutine(StreamPlayAudioFile(filePath));
	}
	public void OnStopRecordedSound()
	{
		if (audioSource.isPlaying)
		{
			audioSource.Stop();
			audioSource.clip = null;
			_microphone.audioSource.mute = false;
		}
	}
	private IEnumerator StreamPlayAudioFile(string filePath)
	{
		using (WWW www = new WWW("file:///" + filePath))
		{
			yield return www;
			audioSource.clip = www.GetAudioClip(false, true);
			audioSource.loop = false;

			audioSource.Play();
			isSetupPlay = true;
		}
	}

	public void SetText(Text text)
	{
		StartCoroutine(SetTextAfterSetup(text));
	}
	private IEnumerator SetTextAfterSetup(Text text)
	{
		yield return new WaitWhile(() => !isSetupPlay);
		text.text = audioSource.isPlaying ? "PLAY" : string.Empty;	
	}
}
