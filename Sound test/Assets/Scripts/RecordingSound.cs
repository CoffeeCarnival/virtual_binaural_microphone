using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text;
using UnityEngine.Diagnostics;

public class RecordingSound : MonoBehaviour, ISetText {

	private static string defaultPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); } }
	
	[SerializeField]
	private string _outputPath = string.Empty;
	public string outputPath { get { return string.IsNullOrEmpty(_outputPath) ? defaultPath : _outputPath; } }
	[SerializeField]
	private string _fileName = string.Empty;
	public string fileName { get { return _fileName; } }


	public bool isStandBy { get; private set; } = false;

	private bool isRecording { get; set; } = false;
	private Queue<float[]> audioDataQueue { get; set; } = new Queue<float[]>();
	private Thread wavCreateThread { get; set; } = null;

	private int channels { get; set; }
	private int bit { get; set; }
	private int samplerate { get; set; }


	public void SetText(Text text) {
		text.text = isRecording ? "REC" : string.Empty;
	}

	private void Awake() {
		isStandBy = true;
		this.channels = channels;
		this.bit = 16;
		this.samplerate = AudioSettings.outputSampleRate;
	}
	private void Update() {

	}
	private void OnAudioFilterRead(float[] data, int channels) {
		if (isRecording) {
			this.channels = channels;
			PushAudioDataQueue(data, channels);
			foreach (int i in Enumerable.Range(0, data.Count())) {
				data[i] = 0f;
			}
		}
	}
	private void PushAudioDataQueue(float[] data, int channels) {
		float[] enqueueData = new float[data.Count()];
		Array.Copy(data, enqueueData, data.Count());
		audioDataQueue.Enqueue(enqueueData);
	}
	private void WavCreateTask() {
		string filePath = outputPath + "\\" + fileName + ".wav";
		using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
		using (WavWriter wavWriter = new WavWriter(fileStream)) {
			while (isRecording || audioDataQueue.Count() > 0) {
				if (audioDataQueue.Count() > 0) {
					var soundData = audioDataQueue.Dequeue();
					foreach(var data in soundData) {
						wavWriter.Write(FloatToShortWave(data));
					}
				} else {
					Thread.Sleep(1);
				}
			}
			wavWriter.channels = (ushort)this.channels;
			wavWriter.bit = (ushort)this.bit;
			wavWriter.sampleRate = (uint)this.samplerate;
		}
		isStandBy = true;
	}
	private short FloatToShortWave(float waveData) {
		return (short)(waveData * short.MaxValue);
	}
	public void OnClickRecordButton() {
		isRecording = !isRecording;
		if (isRecording && !isStandBy) {
			isRecording = false;
		}
		if (isRecording) {
			isStandBy = false;
			this.wavCreateThread = new Thread(WavCreateTask);
			this.wavCreateThread.IsBackground = true;
			this.wavCreateThread.Priority = System.Threading.ThreadPriority.Lowest;

			this.wavCreateThread.Start();
		}
	}
}
