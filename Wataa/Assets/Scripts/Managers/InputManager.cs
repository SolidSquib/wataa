using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	// Singleton instance
	private static InputManager mInputMan;

	private static bool bEnableInput = true;

	public delegate void Delegate_Vector3Param(Vector3 Position);
	public delegate void Delegate_TwoVector3Params(Vector3 Position, Vector3 Delta);
	public delegate void Delegate_Float(float Value);
	public delegate void Delegate_Empty();
	public delegate void Delegate_Touch(Touch touch);

	// Mouse
	public event Delegate_TwoVector3Params OnMouseMoveEvent;
	public event Delegate_Vector3Param OnLeftMouseButtonDown;
	public event Delegate_Vector3Param OnLeftMouseButtonUp;
	public event Delegate_Vector3Param OnRightMouseButtonDown;
	public event Delegate_Vector3Param OnRightMouseButtonUp;

	// Touch
	public event Delegate_Touch OnTouchDragEvent;
	public event Delegate_Touch OnTouchStartEvent;
	public event Delegate_Touch OnTouchEndEvent;

	// Raw Key input
	private Dictionary<KeyCode, System.Action> KeyDownCallbacks = new Dictionary<KeyCode, System.Action>();
	private Dictionary<KeyCode, System.Action> KeyUpCallbacks = new Dictionary<KeyCode, System.Action>();
	private Dictionary<KeyCode, bool> KeyEventStates = new Dictionary<KeyCode, bool>();

	// Editor bound input
	private Dictionary<string, System.Action> ButtonDownCallbacks = new Dictionary<string, System.Action>();
	private Dictionary<string, System.Action> ButtonUpCallbacks = new Dictionary<string, System.Action>();
	private Dictionary<string, bool> ButtonEventStates = new Dictionary<string, bool>();

	// Axis input
	private Dictionary<string, System.Action<float>> AxisCallbacks = new Dictionary<string, System.Action<float>>();
	private Dictionary<string, float> AxisValues = new Dictionary<string, float>();

	// Mouse variables
	bool _LeftMouseButtonDown = false;
	bool _RightMouseButtonDown = false;

	#region Property Accessors 
	public static InputManager Singleton
	{
		get { return mInputMan; }
	}
	#endregion

	void Awake()
	{
		if (!mInputMan)
		{
			mInputMan = this;
		}
		else if (mInputMan != this)
		{
			Destroy(gameObject);
		}
	}

	void Update()
	{
		if (bEnableInput)
		{
			// Mouse event
			CheckMouseEvents();
			CheckKeyboardEvents();
			CheckButtonEvents();
			DistributeAxisEvents();
		}
	}

	public void EnableInput()
	{
		bEnableInput = true;
	}

	public void DisableInput()
	{
		bEnableInput = false;
	}

	void CheckTouchEvents()
	{
		// Account for five fingers max.
		for (int i = 0; i < 5; ++i)
		{
			Touch touch = Input.GetTouch(i);
			switch(touch.phase)
			{
				case TouchPhase.Began:
					OnTouchStartEvent(touch);
					break;

				case TouchPhase.Ended:
					OnTouchEndEvent(touch);
					break;

				case TouchPhase.Moved:
					OnTouchDragEvent(touch);
					break;
			}
		}
	}

	void CheckMouseEvents()
	{
        // Check and update mouse position
        Vector3 MousePosition = Input.mousePosition;
        Vector3 MouseMovement = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        OnMouseMoveEvent?.Invoke(MousePosition, MouseMovement);

        // Check and update left mouse button
        bool LeftMouseButtonDown = Input.GetMouseButton(0);
		if (OnLeftMouseButtonDown != null && LeftMouseButtonDown && !_LeftMouseButtonDown)
		{
			OnLeftMouseButtonDown(MousePosition);
		}
		else if (OnLeftMouseButtonUp != null && !LeftMouseButtonDown && _LeftMouseButtonDown)
		{
			OnLeftMouseButtonUp(MousePosition);
		}
		_LeftMouseButtonDown = LeftMouseButtonDown;

		// Check and update right mouse button
		bool RightMouseButtonDown = Input.GetMouseButton(1);
		if (OnRightMouseButtonDown != null && RightMouseButtonDown && !_RightMouseButtonDown)
		{
			OnRightMouseButtonDown(MousePosition);
		}
		else if (OnRightMouseButtonUp != null && !RightMouseButtonDown && _RightMouseButtonDown)
		{
			OnRightMouseButtonUp(MousePosition);
		}
		_RightMouseButtonDown = RightMouseButtonDown;
    }

	void CheckKeyboardEvents()
	{
		foreach (var Pair in KeyEventStates)
		{
			bool KeyDown = Input.GetKeyDown(Pair.Key);
			if (KeyEventStates.ContainsKey(Pair.Key) && KeyDown != KeyEventStates[Pair.Key])
			{
				if (KeyDown && KeyDownCallbacks.ContainsKey(Pair.Key))
				{
					KeyDownCallbacks[Pair.Key]();
				}
				else if (!KeyDown && KeyUpCallbacks.ContainsKey(Pair.Key))
				{
					KeyUpCallbacks[Pair.Key]();
				}

				KeyEventStates[Pair.Key] = KeyDown;
			}
		}
	}

	void CheckButtonEvents()
	{
		foreach (var Pair in ButtonEventStates)
		{
			bool ButtonDown = Input.GetKeyDown(Pair.Key);
			if (ButtonEventStates.ContainsKey(Pair.Key) && ButtonDown != ButtonEventStates[Pair.Key])
			{
				if (ButtonDown && ButtonDownCallbacks.ContainsKey(Pair.Key))
				{
					ButtonDownCallbacks[Pair.Key]();
				}
				else if (!ButtonDown && ButtonUpCallbacks.ContainsKey(Pair.Key))
				{
					ButtonUpCallbacks[Pair.Key]();
				}

				ButtonEventStates[Pair.Key] = ButtonDown;
			}
		}
	}

	void DistributeAxisEvents()
	{
		foreach (var Pair in AxisCallbacks)
		{
			AxisValues[Pair.Key] = Input.GetAxis(Pair.Key);
			Pair.Value(AxisValues[Pair.Key]);
		}
	}

	#region Register and Unregister input events
	public void RegisterKeyDownEvent(KeyCode Key, System.Action Callback)
	{
		if (KeyDownCallbacks.ContainsKey(Key))
		{
			KeyDownCallbacks[Key] += Callback;
		}
		else
		{
			KeyDownCallbacks.Add(Key, Callback);
			if (!KeyEventStates.ContainsKey(Key))
			{
				KeyEventStates.Add(Key, false);
			}
		}
	}

	public void RegisterKeyUpEvent(KeyCode Key, System.Action Callback)
	{
		if (KeyUpCallbacks.ContainsKey(Key))
		{
			KeyUpCallbacks[Key] += Callback;
		}
		else
		{
			KeyUpCallbacks.Add(Key, Callback);
			if (!KeyEventStates.ContainsKey(Key))
			{
				KeyEventStates.Add(Key, false);
			}
		}
	}

	public void UnregisterKeyDownEvent(KeyCode Key, System.Action Callback)
	{
		if (KeyDownCallbacks.ContainsKey(Key))
		{
			KeyDownCallbacks[Key] -= Callback;
			if (KeyDownCallbacks[Key] == null)
			{
				KeyDownCallbacks.Remove(Key);
				if (!KeyUpCallbacks.ContainsKey(Key))
				{
					KeyEventStates.Remove(Key);
				}
			}
		}
	}

	public void UnregisterKeyUpEvent(KeyCode Key, System.Action Callback)
	{
		if (KeyUpCallbacks.ContainsKey(Key))
		{
			KeyUpCallbacks[Key] -= Callback;
			if (KeyUpCallbacks[Key] == null)
			{
				KeyUpCallbacks.Remove(Key);
				if (!KeyDownCallbacks.ContainsKey(Key))
				{
					KeyEventStates.Remove(Key);
				}
			}
		}
	}

	public void RegisterButtonDownEvent(string Key, System.Action Callback)
	{
		if (ButtonDownCallbacks.ContainsKey(Key))
		{
			ButtonDownCallbacks[Key] += Callback;
		}
		else
		{
			ButtonDownCallbacks.Add(Key, Callback);
			if (!ButtonEventStates.ContainsKey(Key))
			{
				ButtonEventStates.Add(Key, false);
			}
		}
	}

	public void RegisterButtonUpEvent(string Key, System.Action Callback)
	{
		if (ButtonUpCallbacks.ContainsKey(Key))
		{
			ButtonUpCallbacks[Key] += Callback;
		}
		else
		{
			ButtonUpCallbacks.Add(Key, Callback);
			if (!ButtonEventStates.ContainsKey(Key))
			{
				ButtonEventStates.Add(Key, false);
			}
		}
	}

	public void UnregisterButtonDownEvent(string Key, System.Action Callback)
	{
		if (ButtonDownCallbacks.ContainsKey(Key))
		{
			ButtonDownCallbacks[Key] -= Callback;
			if (ButtonDownCallbacks[Key] == null)
			{
				ButtonDownCallbacks.Remove(Key);
				if (!ButtonUpCallbacks.ContainsKey(Key))
				{
					ButtonUpCallbacks.Remove(Key);
				}
			}
		}
	}

	public void UnregisterButtonUpEvent(string Key, System.Action Callback)
	{
		if (ButtonUpCallbacks.ContainsKey(Key))
		{
			ButtonUpCallbacks[Key] -= Callback;
			if (ButtonUpCallbacks[Key] == null)
			{
				ButtonUpCallbacks.Remove(Key);
				if (!ButtonDownCallbacks.ContainsKey(Key))
				{
					ButtonDownCallbacks.Remove(Key);
				}
			}
		}
	}

	public void RegisterAxisEvent(string Key, System.Action<float> Callback)
	{
		if (AxisCallbacks.ContainsKey(Key))
		{
			AxisCallbacks[Key] += Callback;
		}
		else
		{
			AxisCallbacks.Add(Key, Callback);
			AxisValues.Add(Key, 0);
		}
	}

	public void UnregisterAxisEvent(string Key, System.Action<float> Callback)
	{
		if (AxisCallbacks.ContainsKey(Key))
		{
			AxisCallbacks[Key] -= Callback;
			if (AxisCallbacks[Key] == null)
			{
				AxisCallbacks.Remove(Key);
				AxisValues.Remove(Key);
			}
		}
	}
	#endregion
}

