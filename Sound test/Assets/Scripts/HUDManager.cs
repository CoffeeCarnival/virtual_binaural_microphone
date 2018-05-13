using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class HUDManager : MonoBehaviour {

	[SerializeField]
	private Image _textPanel = null;
	[SerializeField]
	private Text _text = null;
	

	void Start () {
		if (_text != null) {
			_text
				.ObserveEveryValueChanged(t => t.text)
				.Subscribe(t => _textPanel.gameObject.SetActive(!string.IsNullOrEmpty(t)));
			_text.text = string.Empty;
		}
	}
	
}
