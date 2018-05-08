using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class HUDManager : MonoBehaviour {

	[SerializeField]
	private Image _textPanel;
	[SerializeField]
	private Text _text;
	

	void Start () {
		_text
			.ObserveEveryValueChanged(t => t.text)
			.Subscribe(t => _textPanel.gameObject.SetActive(!string.IsNullOrEmpty(t)));
		_text.text = string.Empty;
	}
	
}
