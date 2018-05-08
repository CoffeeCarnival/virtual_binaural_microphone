using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class VIVEControllerInput : MonoBehaviour {

	private SteamVR_TrackedObject trackedObject { get { return _trackedObject ?? (_trackedObject = GetComponent<SteamVR_TrackedObject>()); } }
	private SteamVR_TrackedObject _trackedObject;
	private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObject.index); } }

	
	public UnityAction<Vector2> OnTouchTrackPadAxis = (v) => { };
	public UnityAction<Vector2> OnTouchDownTrackPadAxis = (v) => { };
	public UnityAction<Vector2> OnTouchUpTrackPadAxis = (v) => { };
	public UnityAction<Vector2> OnClickTrackPadAxis = (v) => { };
	public UnityAction OnDownHairTrigger = () => { };
	public UnityAction OnUpHairTrigger = () => { };
	public UnityAction OnDownGrip = () => { };
	public UnityAction OnUpGrip = () => { };

	void Update () {

		ControllerInput();

	}

	void ControllerInput()
	{
		var axis = controller.GetAxis();
		if (controller.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
		{
			OnTouchDownTrackPadAxis.Invoke(axis);
		}
		if (controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
		{
			OnTouchTrackPadAxis.Invoke(axis);
		}
		if (controller.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
		{
			OnTouchUpTrackPadAxis.Invoke(axis);
		}
		if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
		{
			OnClickTrackPadAxis.Invoke(axis);
		}
		if (controller.GetHairTriggerDown())
		{
			OnDownHairTrigger.Invoke();
		}
		if (controller.GetHairTriggerUp())
		{
			OnUpHairTrigger.Invoke();
		}
		if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
		{
			OnDownGrip.Invoke();
		}
		if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
		{
			OnUpGrip.Invoke();
		}
	}

}
