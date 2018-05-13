using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

	private static T _Instance = null;
	public static T Instance {
		get {
			return _Instance ?? (_Instance = FindObjectOfType<T>());
		}
	}

	virtual protected void Awake() {
		CheckInstance();
	}
	protected bool CheckInstance() {
		if (_Instance == null) {
			_Instance = this as T;
			return true;
		}else if (_Instance == this) {
			return true;
		}
		Destroy(this);
		return false;
	}
}
