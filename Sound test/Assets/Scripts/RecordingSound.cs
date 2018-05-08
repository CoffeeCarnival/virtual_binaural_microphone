using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text;

public class RecordingSound : MonoBehaviour, ISetText
{

	private static string defaultPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); } }

	[System.Serializable]
	public enum SPECTRUM
	{
		low = 64,
		lowMiddle = 128,
		lowHigh = 256,
		middleLow = 512,
		middle = 1024,
		middleHigh = 2048,
		highLow = 4096,
		high = 8192,
	}
	[SerializeField]
	private SPECTRUM _spectrum = SPECTRUM.lowHigh;
	private SPECTRUM spectrum { get { return _spectrum; } }
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
	private int channel { get; set; }
	

	public void SetText(Text text)
	{
		text.text = isRecording ? "REC" : string.Empty;
	}

	private void Awake()
	{
		isStandBy = true;
	}
	private void Update()
	{
		
	}
	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (isRecording)
		{
			this.channel = channels;
			PushAudioDataQueue(data, channels);
			foreach (int i in Enumerable.Range(0, data.Count()))
			{
				data[i] = 0f;
			}
		}
	}
	private void PushAudioDataQueue(float[] data, int channels)
	{
		float[] enqueueData = new float[data.Count()];
		Array.Copy(data, enqueueData, data.Count());
		audioDataQueue.Enqueue(enqueueData);
	}
	private void WavCreateTask()
	{
		string filePath = outputPath + "\\" + fileName + ".wav";
		using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
		using (BinaryWriter binWriter = new BinaryWriter(fileStream))
		{
			var data = new DataChunk();
			while (isRecording || audioDataQueue.Count() > 0)
			{
				if (audioDataQueue.Count() > 0)
				{
					var soundData = audioDataQueue.Dequeue();
					data.AddAudioData(soundData);
				}
				else
				{
					Thread.Sleep(1);
				}
			}
			var fmt = new FmtChunk(
				channel: (ushort)this.channel);

			var wavData = new WavData(
				fmtChunk: fmt,
				dataChunk: data);
			binWriter.Write(wavData.GetBytes());
		}
		isStandBy = true;
	}
	public void OnClickRecordButton()
	{
		isRecording = !isRecording;
		if (isRecording && !isStandBy)
		{
			isRecording = false;
		}
		if (isRecording)
		{
			isStandBy = false;
			this.wavCreateThread = new Thread(WavCreateTask);
			this.wavCreateThread.IsBackground = true;
			this.wavCreateThread.Priority = System.Threading.ThreadPriority.Lowest;
			
			this.wavCreateThread.Start();
		}
	}

	public class WavData : IByteData
	{
		private string riffHeader;
		private string waveHeader;
		private FmtChunk fmtChunk;
		private DataChunk dataChunk;

		public WavData(
			string riffHeader = "RIFF",
			string waveHeader = "WAVE",
			FmtChunk fmtChunk = null,
			DataChunk dataChunk = null)
		{
			this.riffHeader = riffHeader;
			this.waveHeader = waveHeader;
			this.fmtChunk = fmtChunk ?? new FmtChunk();
			this.dataChunk = dataChunk ?? new DataChunk();
		}
		public byte[] GetBytes()
		{
			var fmtByte = this.fmtChunk.GetBytes();
			var dataByte = this.dataChunk.GetBytes();
			UInt32 fileSize = (UInt32)(this.waveHeader.Count() + fmtByte.Count() + dataByte.Count());
			var wavData = new[]
			{
			new { dataByte = Encoding.ASCII.GetBytes(this.riffHeader), size = this.riffHeader.Count(), },
			new { dataByte = BitConverter.GetBytes(fileSize), size = sizeof(UInt32), },
			new { dataByte = Encoding.ASCII.GetBytes(this.waveHeader), size = this.waveHeader.Count(), },
			new { dataByte = fmtByte, size = fmtByte.Count(), },
			new { dataByte = dataByte, size = dataByte.Count(), },
		};
			int size = wavData
				.Select(d => d.size)
				.Sum();
			Byte[] data = new Byte[size];
			int index = 0;
			foreach (var wData in wavData)
			{
				Array.Copy(wData.dataByte, 0, data, index, wData.size);
				index += wData.size;
			}
			return data;
		}
	}
	public interface IByteData
	{
		Byte[] GetBytes();
	}
	public interface IWavChunk : IByteData
	{
		string GetChunkName();
		UInt32 GetChunkSize();
	}
	public class FmtChunk : IWavChunk
	{
		private string formatChunkTag;
		private UInt16 formatId;
		private UInt16 channel;
		private UInt32 samplesPerSec;
		private UInt32 avgBytesPerSec;
		private UInt16 blockAlign;
		private UInt16 bitsPerSample;
		private UInt16 extendsSize;
		private Byte[] extendsData;

		public FmtChunk(
			string formatChunkTag = "fmt ",
			UInt16 formatId = 0x0001,
			UInt16 channel = 1,
			UInt32 samplesPerSec = 44100,
			UInt16 bitsPerSample = 16,
			Byte[] extendsData = null)
		{
			this.formatChunkTag = formatChunkTag;
			this.formatId = formatId;
			this.channel = channel;
			this.samplesPerSec = samplesPerSec;
			this.bitsPerSample = bitsPerSample;
			this.blockAlign = (UInt16)(this.channel * (this.bitsPerSample >> 3));
			this.avgBytesPerSec = this.samplesPerSec * this.channel * this.blockAlign;
			this.extendsData = extendsData ?? new Byte[0];
			this.extendsSize = (UInt16)this.extendsData.Count();
		}
		public Byte[] GetBytes()
		{
			var fmtChunk = new[]
			{
			new { byteData = Encoding.ASCII.GetBytes(GetChunkName()), size = GetChunkName().Count() },
			new { byteData = BitConverter.GetBytes(GetChunkSize()), size = sizeof(UInt32) },
			new { byteData = BitConverter.GetBytes(this.formatId), size = sizeof(UInt16) },
			new { byteData = BitConverter.GetBytes(this.channel), size = sizeof(UInt16) },
			new { byteData = BitConverter.GetBytes(this.samplesPerSec), size = sizeof(UInt32) },
			new { byteData = BitConverter.GetBytes(this.avgBytesPerSec), size = sizeof(UInt32) },
			new { byteData = BitConverter.GetBytes(this.blockAlign), size = sizeof(UInt16) },
			new { byteData = BitConverter.GetBytes(this.bitsPerSample), size = sizeof(UInt16) },
		};
			if (this.extendsSize > 0)
			{
				var fmtChunkExt = new[]
				{
				new { byteData = BitConverter.GetBytes(this.extendsSize), size = sizeof(UInt16) },
				new { byteData = this.extendsData, size = (int)this.extendsSize },
			};
				fmtChunk = fmtChunk
					.Concat(fmtChunkExt)
					.ToArray();
			}
			int size = fmtChunk
				.Select(d => d.size)
				.Sum();
			Byte[] data = new Byte[size];
			int index = 0;
			foreach (var chunk in fmtChunk)
			{
				Array.Copy(chunk.byteData, 0, data, index, chunk.size);
				index += chunk.size;
			}
			return data;
		}
		public string GetChunkName()
		{
			return this.formatChunkTag;
		}
		public UInt32 GetChunkSize()
		{
			return
				sizeof(UInt16) * 4 +
				sizeof(UInt32) * 2 +
				(uint)(this.extendsSize == 0 ? 0 : this.extendsSize + sizeof(UInt16));
		}
	}
	public class DataChunk : IWavChunk
	{
		private string dataChunkTag;
		private List<Byte> waveData;

		public DataChunk(
			string dataChunkTag = "data",
			List<Byte> waveData = null)
		{
			this.dataChunkTag = dataChunkTag;
			this.waveData = waveData ?? new List<Byte>();
		}
		public static IEnumerable<byte> Convert(float[] buff)
		{
			return buff
					.Select(d => BitConverter.GetBytes((short)(short.MaxValue * d)))
					.Aggregate(new List<byte>(), (list, d) =>
					{
						list.AddRange(d);
						return list;
					});
		}
		public void AddAudioData(float[] data)
		{
			waveData.AddRange(Convert(data));
		}
		public Byte[] GetBytes()
		{
			var dataChunk = new[]
			{
			new{ dataByte = Encoding.ASCII.GetBytes(GetChunkName()) , size = GetChunkName().Count(), },
			new{ dataByte = BitConverter.GetBytes(GetChunkSize()) , size = sizeof(UInt32), },
			new{ dataByte = this.waveData.ToArray() , size = this.waveData.Count(), },
		};
			int size = dataChunk
				.Select(d => d.size)
				.Sum();
			Byte[] data = new Byte[size];
			int index = 0;
			foreach (var chunk in dataChunk)
			{
				Array.Copy(chunk.dataByte, 0, data, index, chunk.size);
				index += chunk.size;
			}
			return data;
		}
		public string GetChunkName()
		{
			return this.dataChunkTag;
		}
		public UInt32 GetChunkSize()
		{
			return (uint)this.waveData.Count();
		}
	}
}
