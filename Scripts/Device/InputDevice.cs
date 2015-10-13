using System;
using UnityEngine;
using System.Collections.Generic;

namespace InControl {
public class InputDevice {
	public delegate void OnInputChanged(int player, InputDevice inputDevice, InputControl inputControl);
	public static event OnInputChanged onInputChanged = delegate {};

	public static readonly InputDevice Null = new InputDevice( "NullInputDevice" );

	public int player = -1;
	public int SortOrder = int.MaxValue;

	public string Name { get; protected set; }
	public string Meta { get; protected set; }

	public ulong LastChangeTick { get; protected set; }

	public List<InputControl> Controls { get; protected set; }


	public InputDevice( string name ) {
		Name = name;
		Meta = "";

		LastChangeTick = 0;

		Controls = new List<InputControl>();
	}


	public InputControl GetControl( Enum inputControlType, int player = -1 ) {
		InputControlType controlType = (InputControlType)inputControlType;
		var control = Controls.Find(c => c.Target == controlType && c.player == player);
		return control ?? InputControl.Null;
	}


	// Warning: this is not efficient. Don't use it unless you have to, m'kay?
	public static InputControlType GetInputControlTypeByName( string inputControlName ) {
		return (InputControlType) Enum.Parse( typeof(InputControlType), inputControlName );
	}


	// Warning: this is not efficient. Don't use it unless you have to, m'kay?
	public InputControl GetControlByName( string inputControlName, int player = -1 ) {
		var inputControlType = GetInputControlTypeByName( inputControlName );
		return GetControl( inputControlType, player );
	}


	public InputControl AddControl( InputControlType target, string handle, int player = -1 ) {
		var inputControl = new InputControl( handle, target, player );
		Controls.Add(inputControl);
		return inputControl;
	}


	public void UpdateWithState( InputControlType inputControlType, bool state, ulong updateTick, int player = -1 ) {
		GetControl( inputControlType, player ).UpdateWithState( state, updateTick );
	}


	public void UpdateWithValue( InputControlType inputControlType, float value, ulong updateTick, int player = -1 ) {
		GetControl( inputControlType, player ).UpdateWithValue( value, updateTick );
	}


	public void PreUpdate( ulong updateTick, float deltaTime ) {
		for (int i = 0; i < Controls.Count; i++) {
			var control = Controls[i];
			if (control != null) {
				control.PreUpdate( updateTick );
			}
		}
	}


	public virtual void Update( ulong updateTick, float deltaTime ) {
		// Implemented by subclasses.
	}


	public void PostUpdate( ulong updateTick, float deltaTime ) {
		for (int i = 0; i < Controls.Count; i++) {
			var control = Controls[i];
			if (control != null) {
				int p = (control.player < 0) ? player : control.player;
				// This only really applies to analog controls.
				if (control.RawValue.HasValue) {
					control.UpdateWithValue( control.RawValue.Value, updateTick );
				}
				else if (control.PreValue.HasValue) {
					control.UpdateWithValue( ProcessAnalogControlValue( control, deltaTime ), updateTick );
				}

				control.PostUpdate( updateTick );

				if (control.HasChanged) {
					onInputChanged(p, this, control);
					LastChangeTick = updateTick;
				}
				else if (control.Obverse != null) {
					onInputChanged(p, this, control);
				}

			}
		}
	}


	float ProcessAnalogControlValue( InputControl control, float deltaTime ) {
		var analogValue = control.PreValue.Value;

		var obverseTarget = control.Obverse;
		if (obverseTarget.HasValue) {
			var obverseControl = GetControl( obverseTarget, control.player );
			analogValue = ApplyCircularDeadZone( analogValue, obverseControl.PreValue.Value, control.LowerDeadZone, control.UpperDeadZone );
		}
		else {
			analogValue = ApplyDeadZone( analogValue, control.LowerDeadZone, control.UpperDeadZone );
		}

		return ApplySmoothing( analogValue, control.LastValue, deltaTime, control.Sensitivity );
	}


	float ApplyDeadZone( float value, float lowerDeadZone, float upperDeadZone ) {
		return Mathf.InverseLerp( lowerDeadZone, upperDeadZone, Mathf.Abs( value ) ) * Mathf.Sign( value );
	}


	float ApplyCircularDeadZone( float axisValue1, float axisValue2, float lowerDeadZone, float upperDeadZone ) {
		var axisVector = new Vector2( axisValue1, axisValue2 );
		var magnitude = Mathf.InverseLerp( lowerDeadZone, upperDeadZone, axisVector.magnitude );
		return (axisVector.normalized * magnitude).x;
	}


	float ApplySmoothing( float thisValue, float lastValue, float deltaTime, float sensitivity ) {
		// 1.0f and above is instant (no smoothing).
		if (Mathf.Approximately( sensitivity, 1.0f )) {
			return thisValue;
		}

		// Apply sensitivity (how quickly the value adapts to changes).
		var maxDelta = deltaTime * sensitivity * 100.0f;

		// Snap to zero when changing direction quickly.
		if (Mathf.Sign( lastValue ) != Mathf.Sign( thisValue )) {
			lastValue = 0.0f;
		}

		return Mathf.MoveTowards( lastValue, thisValue, maxDelta );
	}


	public bool LastChangedAfter( InputDevice otherDevice ) {
		return LastChangeTick > otherDevice.LastChangeTick;
	}


	public virtual void Vibrate( float leftMotor, float rightMotor ) {
	}


	public void Vibrate( float intensity ) {
		Vibrate( intensity, intensity );
	}


	public virtual bool IsSupportedOnThisPlatform {
		get { return true; }
	}


	public virtual bool IsKnown {
		get { return true; }
	}


	public bool MenuWasPressed {
		get {
			return GetControl( InputControlType.Back ).WasPressed ||
				GetControl( InputControlType.Start ).WasPressed ||
				GetControl( InputControlType.Select ).WasPressed ||
				GetControl( InputControlType.System ).WasPressed ||
				GetControl( InputControlType.Pause ).WasPressed ||
				GetControl( InputControlType.Menu ).WasPressed;
		}
	}


	public InputControl LeftStickX (int player = -1) {
		return GetControl( InputControlType.LeftStickX, player );
	}


	public InputControl LeftStickY (int player = -1) {
		return GetControl( InputControlType.LeftStickY, player );
	}


	public InputControl LeftStickButton (int player = -1) {
		return GetControl( InputControlType.LeftStickButton, player );
	}

	public InputControl RightStickX (int player = -1) {
		return GetControl( InputControlType.RightStickX, player );
	}


	public InputControl RightStickY (int player = -1) {
		return GetControl( InputControlType.RightStickY, player );
	}


	public InputControl RightStickButton (int player = -1) {
		return GetControl( InputControlType.RightStickButton, player );
	}

	public InputControl DPadUp (int player = -1) {
		return GetControl( InputControlType.DPadUp, player );
	}


	public InputControl DPadDown (int player = -1) {
		return GetControl( InputControlType.DPadDown, player );
	}


	public InputControl DPadLeft (int player = -1) {
		return GetControl( InputControlType.DPadLeft, player );
	}


	public InputControl DPadRight (int player = -1) {
		return GetControl( InputControlType.DPadRight, player );
	}

	public InputControl Action1 (int player = -1) {
		return GetControl( InputControlType.Action1, player );
	}


	public InputControl Action2 (int player = -1) {
		return GetControl( InputControlType.Action2, player );
	}


	public InputControl Action3 (int player = -1) {
		return GetControl( InputControlType.Action3, player );
	}


	public InputControl Action4 (int player = -1) {
		return GetControl( InputControlType.Action4, player );
	}

	public InputControl LeftTrigger (int player = -1) {
		return GetControl( InputControlType.LeftTrigger, player );
	}


	public InputControl RightTrigger (int player = -1) {
		return GetControl( InputControlType.RightTrigger, player );
	}

	public InputControl LeftBumper (int player = -1) {
		return GetControl( InputControlType.LeftBumper, player );
	}


	public InputControl RightBumper (int player = -1) {
		return GetControl( InputControlType.RightBumper, player );
	}


	public Vector2 LeftStickVector (int player = -1) {
		return new Vector2( LeftStickX(player).Value, LeftStickY(player).Value );
	}


	public Vector2 RightStickVector (int player = -1) {
		return new Vector2( RightStickX(player).Value, RightStickY(player).Value );
	}


	public float DPadX (int player = -1) {
		return DPadLeft(player).State ? -DPadLeft(player).Value : DPadRight(player).Value;
	}


	public float DPadY (int player = -1) {
		var y = DPadUp(player).State ? DPadUp(player).Value : -DPadDown(player).Value; 
		return InputManager.InvertYAxis ? -y : y;
	}


	public Vector2 DPadVector (int player = -1) {
		return new Vector2( DPadX(player), DPadY(player) ).normalized;
	}


	public Vector2 Direction (int player = -1) {
		var dpad = DPadVector(player);
		var zero = Mathf.Approximately( dpad.x, 0.0f ) && Mathf.Approximately( dpad.y, 0.0f );
		return zero ? LeftStickVector(player) : dpad;
	}
}
}
