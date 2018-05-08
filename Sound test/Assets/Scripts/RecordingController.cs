using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class RecordingController : MonoBehaviour {

	[SerializeField]
	public RecordingSound recordingSound;
	[SerializeField]
	public VIVEControllerInput viveController;
	[SerializeField]
	[Range(0.0f,0.9f)]
	public float inputAxisLengthThreshold;
	[SerializeField]
	public Color basicColor;
	[SerializeField]
	public Color selectColor;
	[SerializeField]
	public Transform centerObj;
	[SerializeField]
	public List<Button> buttons;
	

	private void Awake()
	{
		centerObj = centerObj ?? transform;		
	}
	private void OnEnable()
	{
		viveController.OnTouchDownTrackPadAxis += OnTouchStart;
		viveController.OnTouchTrackPadAxis += OnTouch;
		viveController.OnTouchUpTrackPadAxis += OnTouchEnd;
		viveController.OnClickTrackPadAxis += OnClick;
		SetButtonActive(false);
	}
	private void OnDisable()
	{
		viveController.OnTouchDownTrackPadAxis -= OnTouchStart;
		viveController.OnTouchTrackPadAxis -= OnTouch;
		viveController.OnTouchUpTrackPadAxis -= OnTouchEnd;
		viveController.OnClickTrackPadAxis -= OnClick;
	}
	private void SetButtonActive(bool active)
	{
		foreach (var button in buttons)
		{
			button.gameObject.SetActive(active);
		}
	}
	private int GetSelectButtonIndex(Vector2 axis)
	{
		if (axis.sqrMagnitude < (inputAxisLengthThreshold * inputAxisLengthThreshold))
		{
			return -1;
		}
		var centerPos = centerObj.position;
		var centerRotate = centerObj.rotation;
		var inputAxisVec = centerRotate * axis;
		var buttonsDot = buttons
			.Select((button, i) => new {
				dot = Vector3.Dot(((button.transform.position - centerPos).normalized), inputAxisVec),
				index = i,
			})
			.ToArray();
		var selectButton = buttonsDot
			.OrderByDescending(dot => dot.dot)
			.First();
		return selectButton.index;
	}

	private void OnTouchStart(Vector2 axis)
	{
		SetButtonActive(true);
	}
	private void OnTouch(Vector2 axis)
	{
		int buttonIndex = GetSelectButtonIndex(axis);

		foreach(int i in Enumerable.Range(0, buttons.Count))
		{
			var localPos = buttons[i].transform.localPosition;
			buttons[i].image.color = i == buttonIndex ? selectColor : basicColor;
		}		
	}
	private void OnTouchEnd(Vector2 axis)
	{
		SetButtonActive(false);
	}
	private void OnClick(Vector2 axis)
	{
		int buttonIndex = GetSelectButtonIndex(axis);
		if (buttonIndex >= 0)
		{
			buttons[buttonIndex].onClick.Invoke();
		}
	}

}
