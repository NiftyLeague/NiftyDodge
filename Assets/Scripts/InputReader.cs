using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputReader
{
	public enum Device
	{
		Keyboard, Gamepad1, Gamepad2, Gamepad3, Gamepad4, Any, AnyFree
	}

	static KeyCode kbLeft = KeyCode.LeftArrow;
	static KeyCode kbRight = KeyCode.RightArrow;
	static KeyCode kbUp = KeyCode.UpArrow;
	static KeyCode kbDown = KeyCode.DownArrow;
	static KeyCode kbA = KeyCode.Space;
	static KeyCode kbB = KeyCode.Z;
	static KeyCode kbX = KeyCode.X;
	static KeyCode kbY = KeyCode.C;
	static KeyCode kbStart = KeyCode.Return;
	static KeyCode kbLeftShoulder = KeyCode.Tab;

	static HashSet<Device> gamepadsInUse = new HashSet<Device>();
	static float deadZone = 0.5f;
	static bool haveInitialized;

	public static void GetFrameInput(Device device, InputState inputState)
	{
		if (inputState.updateFrame != Time.frameCount)
		{
			GetInput(device, inputState);
		}
	}

	public static void GetInput(InputState inputState)
	{
		GetInput(Device.Any, inputState);
	}


	public static void GetInput(Device device, InputState inputState)
	{
		inputState.updateFrame = Time.frameCount;
		CacheLastInput(inputState);
		switch (device)
		{
		case Device.Keyboard:
			GetKeyboardInput(inputState);
			break;
		case Device.Gamepad1:
			if (GamepadHasBeenAssigned(Device.Gamepad1))
			{
				GetGamepadInput(Gamepad.all[0], inputState);
			}
			else
			{
				ClearInputState(inputState);
			}
			break;
		case Device.Gamepad2:
			if (GamepadHasBeenAssigned(Device.Gamepad2))
			{
				GetGamepadInput(Gamepad.all[1], inputState);
			}
			else
			{
				ClearInputState(inputState);
			}
			break;
		case Device.Gamepad3:
			if (GamepadHasBeenAssigned(Device.Gamepad3))
			{
				GetGamepadInput(Gamepad.all[2], inputState);
			}
			else
			{
				ClearInputState(inputState);
			}
			break;
		case Device.Gamepad4:
			if (GamepadHasBeenAssigned(Device.Gamepad4))
			{
				GetGamepadInput(Gamepad.all[3], inputState);
			}
			else
			{
				ClearInputState(inputState);
			}
			break;
		case Device.Any:
			GetAnyInput(inputState, false);
			break;
		case Device.AnyFree:
			GetAnyInput(inputState, true);
			break;
		}
	}

	public static void CacheLastInput(InputState inputState)
	{
		inputState.wasAButton = inputState.aButton;
		inputState.wasBButton = inputState.bButton;
		inputState.wasXButton = inputState.xButton;
		inputState.wasYButton = inputState.yButton;

		inputState.wasLeft = inputState.left;
		inputState.wasRight = inputState.right;
		inputState.wasUp = inputState.up;
		inputState.wasDown = inputState.down;
		inputState.wasStart = inputState.start;
		inputState.wasOptions = inputState.options;
		inputState.wasLeftShoulder = inputState.leftShoulder;
	}

	public static Device? GetGamepadDevice(Gamepad device)
	{
		for (int i = 0; i < Gamepad.all.Count; i++)
		{
			if (Gamepad.all[i] == device)
			{
				switch (i)
				{
				case 0:
					return Device.Gamepad1;
				case 1:
					return Device.Gamepad2;
				case 2:
					return Device.Gamepad3;
				case 3:
					return Device.Gamepad4;
				}
			}
		}
		return null;
	}

	public static Gamepad GetDeviceGamepad(Device? device)
	{
		if (device == null || !GamepadHasBeenAssigned((Device)device))
		{
			return null;
		}
		switch (device)
		{
		case Device.Gamepad1:
			return Gamepad.all[0];
		case Device.Gamepad2:
			return Gamepad.all[1];
		case Device.Gamepad3:
			return Gamepad.all[2];
		case Device.Gamepad4:
			return Gamepad.all[3];
		}
		return null;
	}

	public static void Initialize()
	{
		gamepadsInUse.Clear();
		InputSystem.onDeviceChange += OnDeviceChange;
	}

	private static void OnDeviceChange(InputDevice device, InputDeviceChange change)
	{
		if (!(device is Gamepad))
		{
			return;
		}
		switch (change)
		{
		case InputDeviceChange.Removed:
		case InputDeviceChange.Disconnected:
		case InputDeviceChange.Disabled:
		case InputDeviceChange.Destroyed:
			var gamepad = GetGamepadDevice(device as Gamepad);
			if (gamepad.HasValue)
			{
				gamepadsInUse.Remove(gamepad.Value);
			}
			break;
		}
	}

	static void GetAnyInput(InputState inputState, bool freeOnly)
	{
		bool usingKbAxis = false;

		if (Input.GetKey(kbLeft))
		{
			inputState.xAxis -= 1f;
			usingKbAxis = true;
		}
		if (Input.GetKey(kbRight))
		{
			inputState.xAxis += 1f;
			usingKbAxis = true;
		}
		if (Input.GetKey(kbUp))
		{
			inputState.yAxis += 1f;
			usingKbAxis = true;
		}
		if (Input.GetKey(kbDown))
		{
			inputState.yAxis -= 1f;
			usingKbAxis = true;
		}

		inputState.up = Input.GetKey(kbUp);
		inputState.down = Input.GetKey(kbDown);
		inputState.left = Input.GetKey(kbLeft);
		inputState.right = Input.GetKey(kbRight);

		inputState.yButton = Input.GetKey(kbY) || Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Delete);
		inputState.xButton = Input.GetKey(kbX);
		inputState.aButton = Input.GetKey(kbA);
		inputState.bButton = Input.GetKey(kbB);
		inputState.start = Input.GetKey(kbStart);
		inputState.options = false;
		inputState.leftShoulder = Input.GetKey(kbLeftShoulder);

		foreach (Gamepad device in Gamepad.all)
		{
			var gamepad = GetGamepadDevice(device).Value;
			if (freeOnly && gamepadsInUse.Contains(gamepad))
			{
				continue;
			}
			Vector2 leftStick = device.leftStick.ReadValue();
			inputState.right = inputState.right || leftStick.x > deadZone || device.dpad.right.isPressed;
			inputState.left = inputState.left || leftStick.x < -deadZone || device.dpad.left.isPressed;
			inputState.up = inputState.up || leftStick.y > deadZone || device.dpad.up.isPressed;
			inputState.down = inputState.down || leftStick.y < -deadZone || device.dpad.down.isPressed;

			inputState.aButton = inputState.aButton || device.aButton.isPressed;
			inputState.bButton = inputState.bButton || device.bButton.isPressed;
			inputState.xButton = inputState.xButton || device.xButton.isPressed;
			inputState.yButton = inputState.yButton || device.yButton.isPressed;

			inputState.start = inputState.start || device.startButton.isPressed;
			inputState.options = inputState.options || device.selectButton.isPressed;
			inputState.leftShoulder = inputState.leftShoulder || device.leftShoulder.isPressed;

			if (!usingKbAxis)
			{
				inputState.xAxis = leftStick.x;
				inputState.yAxis = leftStick.y;
			}
		}
	}

	static void GetKeyboardInput(InputState inputState)
	{
		inputState.xAxis = inputState.yAxis = inputState.leftTrigger = inputState.rightTrigger = 0f;
		if (Input.GetKey(kbLeft))
		{
			inputState.xAxis -= 1f;
		}
		if (Input.GetKey(kbRight))
		{
			inputState.xAxis += 1f;
		}
		if (Input.GetKey(kbUp))
		{
			inputState.yAxis += 1f;
		}
		if (Input.GetKey(kbDown))
		{
			inputState.yAxis -= 1f;
		}

		inputState.up = Input.GetKey(kbUp);
		inputState.down = Input.GetKey(kbDown);
		inputState.left = Input.GetKey(kbLeft);
		inputState.right = Input.GetKey(kbRight);

		inputState.yButton = Input.GetKey(kbY) || Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Delete);
		inputState.xButton = Input.GetKey(kbX);
		inputState.aButton = Input.GetKey(kbA);
		inputState.bButton = Input.GetKey(kbB);
		inputState.start = Input.GetKey(kbStart);
		inputState.options = false;
		inputState.leftShoulder = Input.GetKey(kbLeftShoulder);
	}


	static void GetGamepadInput(Gamepad device, InputState inputState)
	{
		if (device == null)
			return;

		Vector2 leftStick = device.leftStick.ReadValue();
		inputState.right = (leftStick.x > deadZone) || device.dpad.right.isPressed;
		inputState.left = (leftStick.x < -deadZone) || device.dpad.left.isPressed;
		inputState.up = (leftStick.y > deadZone) || device.dpad.up.isPressed;
		inputState.down = (leftStick.y < -deadZone) || device.dpad.down.isPressed;

		inputState.aButton = device.aButton.isPressed;
		inputState.bButton = device.bButton.isPressed;
		inputState.xButton = device.xButton.isPressed;
		inputState.yButton = device.yButton.isPressed;

		inputState.leftTrigger = device.leftTrigger.ReadValue();
		inputState.rightTrigger = device.rightTrigger.ReadValue();
		inputState.start = device.startButton.isPressed;
		inputState.options = device.selectButton.isPressed;
		inputState.leftShoulder = device.leftShoulder.isPressed;

		inputState.xAxis = leftStick.x;
		inputState.yAxis = leftStick.y;
	}


	public static void ClearInputState(InputState inputState)
	{
		inputState.rightTrigger = inputState.leftTrigger = inputState.xAxis = inputState.yAxis = 0f;
		inputState.left = inputState.right = inputState.up = inputState.down = inputState.aButton = inputState.bButton = inputState.xButton = inputState.yButton = false;
		inputState.options = inputState.leftShoulder = false;
	}


	static bool GamepadHasBeenAssigned(Device device)
	{
		int index = 0;

		switch (device)
		{
		case Device.Gamepad1:
			index = 0;
			break;
		case Device.Gamepad2:
			index = 1;
			break;
		case Device.Gamepad3:
			index = 2;
			break;
		case Device.Gamepad4:
			index = 3;
			break;
		default:
			break;
		}

		return Gamepad.all.Count > index;
		//            return inControlDevices[index] != null;
		//            return false;
	}

	public static void SetGamepadInUse(Device gamepad, bool inUse)
	{
		if (inUse)
		{
			gamepadsInUse.Add(gamepad);
		}
		else if (gamepadsInUse.Contains(gamepad))
		{
			gamepadsInUse.Remove(gamepad);
		}
	}

	public static bool AnyGamepadsConnected()
	{
		bool hasGamepad = false;
		foreach (Gamepad g in Gamepad.all)
		{
			hasGamepad = hasGamepad || g.enabled;
		}
		return hasGamepad;
	}

	public static bool IsGamepadDevice(Device device)
	{
		return device == Device.Gamepad1 || device == Device.Gamepad2 || device == Device.Gamepad3 || device == Device.Gamepad4;
	}

	public static bool AnyGamepadFree()
	{
		foreach (Gamepad g in Gamepad.all)
		{
			var device = GetGamepadDevice(g);
			if (device.HasValue && !gamepadsInUse.Contains(device.Value))
			{
				return true;
			}
		}
		return false;
	}
}
