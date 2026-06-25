using System;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NXO.Utilities;

public static class InputHandler
{
	public static bool isOn;

	public static bool wasButtonPressed;

	public static bool isOn2;

	public static bool wasButtonPressed2;

	public static bool RGrip()
	{
		return ControllerEmulator.EmulatorEnabled ? ControllerEmulator.GetRGrip() : ((ControllerInputPoller)ControllerInputPoller.instance).rightGrab;
	}

	public static bool None()
	{
		return true;
	}

	public static void ToggleOnButtonPress(Func<bool> buttonCheck, ref bool toggle, ref bool lastState)
	{
		bool flag = buttonCheck();
		if (!lastState && flag)
		{
			toggle = !toggle;
			lastState = flag;
		}
		else
		{
			lastState = flag;
		}
	}

	public static bool LSecondary()
	{
		return ControllerEmulator.EmulatorEnabled ? ControllerEmulator.GetLSecondary() : ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerSecondaryButton;
	}

	public static bool LTrigger()
	{
		return ControllerEmulator.EmulatorEnabled ? ControllerEmulator.GetLTrigger() : (((ControllerInputPoller)ControllerInputPoller.instance).leftControllerIndexFloat > 0.1f);
	}

	public static bool LPrimary()
	{
		return ControllerEmulator.EmulatorEnabled ? ControllerEmulator.GetLPrimary() : ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerPrimaryButton;
	}

	public static bool RTrigger()
	{
		return ControllerEmulator.EmulatorEnabled ? ControllerEmulator.GetRTrigger() : (((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat > 0.1f);
	}

	public static Vector2 GetJoystickAxis(bool left)
	{
		if (SteamVR.active)
		{
			if (!left)
			{
				return SteamVR_Actions.gorillaTag_RightJoystick2DAxis.GetAxis((SteamVR_Input_Sources)2);
			}
			return SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis((SteamVR_Input_Sources)1);
		}
		Vector2 zero = Vector2.zero;
		InputDevice val;
		if (!left)
		{
			val = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerDevice;
			val.TryGetFeatureValue(CommonUsages.primary2DAxis, out zero);
			return zero;
		}
		val = ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerDevice;
		val.TryGetFeatureValue(CommonUsages.primary2DAxis, out zero);
		return zero;
	}

	public static bool RSecondary()
	{
		return ControllerEmulator.EmulatorEnabled ? ControllerEmulator.GetRSecondary() : ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerSecondaryButton;
	}

	public static bool LJoystickClick()
	{
		if (ControllerEmulator.EmulatorEnabled)
		{
			return false;
		}
		bool result = false;
		InputDevice leftControllerDevice = ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerDevice;
		leftControllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out result);
		return result;
	}

	public static bool RJoystickClick()
	{
		if (ControllerEmulator.EmulatorEnabled)
		{
			return false;
		}
		bool result = false;
		InputDevice rightControllerDevice = ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerDevice;
		rightControllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out result);
		return result;
	}

	public static bool LGrip()
	{
		return ControllerEmulator.EmulatorEnabled ? ControllerEmulator.GetLGrip() : ((ControllerInputPoller)ControllerInputPoller.instance).leftGrab;
	}

	public static bool RPrimary()
	{
		return ControllerEmulator.EmulatorEnabled ? ControllerEmulator.GetRPrimary() : ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerPrimaryButton;
	}
}
