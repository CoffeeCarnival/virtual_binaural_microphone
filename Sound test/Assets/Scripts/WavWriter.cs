using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

public class WavWriter : BinaryWriter {
	public ushort channels { get; set; }
	public uint sampleRate { get; set; }
	public ushort bit { get; set; }

	private string riffID { get { return "RIFF"; } }
	private uint size { get { return (uint)(OutStream.Length - (sizeof(uint) + sizeof(byte) * 4 * 4));} }
	private string wavID { get { return "WAVE"; } }
	private string fmtID { get { return "fmt "; } }
	private uint fmtSize { get { return (uint)(sizeof(uint) * 2 + sizeof(ushort) * 4); } }
	private ushort format { get { return 0x0001; } }
	private uint bytePerSec { get { return sampleRate * blockSize; } }
	private ushort blockSize { get { return (ushort)(channels * (bit >> 3)); } }
	private string dataID { get { return "data"; } }
	private uint dataSize { get { return (uint)(OutStream.Length - wavHeaderSize); } }

	private int wavHeaderSize {
		get {
			return sizeof(uint) * 5 +
				sizeof(ushort) * 4 +
				sizeof(byte) * 4 * 4;
		}
	}
	private int wavDataSize {
		get {
			return (int)(OutStream.Length - wavHeaderSize);
		}
	}

	public WavWriter(Stream stream) {
		OutStream = stream;
		WriteHeader();
		SeekWaveDataFirst();
	}

	public long SeekWaveDataFirst() {
		return Seek(wavHeaderSize, SeekOrigin.Begin);
	}
	public long SeekWaveDataEnd() {
		return Seek(0, SeekOrigin.End);
	}

	protected override void Dispose(bool disposing) {
		WriteHeader();
		base.Dispose(disposing);
	}

	private void WriteHeader() {
		Seek(0, SeekOrigin.Begin);
		Write(Encoding.ASCII.GetBytes(riffID));
		Write(BitConverter.GetBytes(size));
		Write(Encoding.ASCII.GetBytes(wavID));
		Write(Encoding.ASCII.GetBytes(fmtID));
		Write(BitConverter.GetBytes(fmtSize));
		Write(BitConverter.GetBytes(format));
		Write(BitConverter.GetBytes(channels));
		Write(BitConverter.GetBytes(sampleRate));
		Write(BitConverter.GetBytes(bytePerSec));
		Write(BitConverter.GetBytes(blockSize));
		Write(BitConverter.GetBytes(bit));
		Write(Encoding.ASCII.GetBytes(dataID));
		Write(BitConverter.GetBytes(dataSize));
	}
}

public static partial class StructEx {
	/// <summary>
	/// 構造体のサイズを取得する
	/// </summary>
	/// <returns>型のサイズ</returns>
	public static int Sizeof<T>(this T obj) where T : struct {
		return Marshal.SizeOf(obj);
	}
	/// <summary>
	/// 構造体配列のサイズを取得する
	/// </summary>
	/// <returns>型のサイズ</returns>
	public static int Sizeof<T>(this T[] obj) where T : struct {
		return obj.Count() != 0 ?
			obj.Sum(e => e.Sizeof()) :
			0;
	}
}