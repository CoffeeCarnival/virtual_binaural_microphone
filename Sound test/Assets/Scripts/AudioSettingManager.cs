using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utf8Json;
using System.IO;

public class AudioSettingManager : SingletonMonoBehaviour<AudioSettingManager> {

	[SerializeField]
	private string settingFilePath = string.Empty;

	public dynamic setting { get; private set; }

	private new void Awake() {
		LoadSetting();
	}
	public void LoadSetting() {
		string jsonText = string.Empty;
		using (FileStream fs = new FileStream(settingFilePath, FileMode.Open))
		using (BinaryReader br = new BinaryReader(fs)) {
			jsonText = br.ReadString();
		}
		setting = JsonSerializer.Deserialize<dynamic>(jsonText);
	}
}
