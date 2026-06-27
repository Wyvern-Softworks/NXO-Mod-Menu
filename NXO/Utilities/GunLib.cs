using System;
using NXO.Mods.Categories;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NXO.Utilities;

public class GunLib
{
	public static bool BigGunPointer;

	public static bool GunLineEnabled;

	public static bool LeftHandGun;

	public static bool GriplessGuns;

	public static bool TriggerlessGuns;

	public static VRRig lockedTargetRig;

	public static VRRig potentialTargetRig;

	public static GameObject lineFollow;

	public static GameObject gunPointer = null;

	public static GameObject[] lineCylinders = null;

	public static RaycastHit raycastHit;

	public static Material lineMaterial = null;

	private static Renderer gunPointerRenderer;

	private static Material gunPointerMaterial = null;

	private static Renderer[] cylinderRenderers;

	private static bool raycastHasHit;

	private static Camera _thirdPersonCameraCache = null;

	private static readonly Vector3 _scaleSmall = new Vector3(0.1f, 0.1f, 0.1f);

	private static readonly Vector3 _scaleBig = new Vector3(0.2f, 0.2f, 0.2f);

	public static bool GunGrips => GriplessGuns || (Mouse.current != null && Mouse.current.rightButton.isPressed) || (LeftHandGun ? InputHandler.LGrip() : InputHandler.RGrip());

	public static bool GunTriggers => TriggerlessGuns || (Mouse.current != null && Mouse.current.leftButton.isPressed) || (LeftHandGun ? InputHandler.LTrigger() : InputHandler.RTrigger());

	public static Color32 CurrentGunColor => ((UnityEngine.Object)(object)lockedTargetRig != (UnityEngine.Object)null) ? Settings.GunLockColor : (GunTriggers ? Settings.GunFireColor : Settings.GunIdleColor);

	private static Transform GunControllerTransform => LeftHandGun ? Variables.playerInstance.LeftHand.controllerTransform : Variables.playerInstance.RightHand.controllerTransform;

	private static Transform GunVisualHandTransform => LeftHandGun ? Variables.taggerInstance.leftHandTransform : Variables.taggerInstance.rightHandTransform;

	public static bool SetupGun()
	{
		if (!GunGrips)
		{
			CancelGunUse();
			return false;
		}
		SetupRaycast();
		return GunTriggers;
	}

	public static void SetGunVisibility(bool isVisible)
	{
		if ((UnityEngine.Object)(object)gunPointer != (UnityEngine.Object)null)
		{
			gunPointer.SetActive(isVisible);
			if (!((UnityEngine.Object)(object)lineFollow != (UnityEngine.Object)null))
			{
				return;
			}
		}
		else if (!((UnityEngine.Object)(object)lineFollow != (UnityEngine.Object)null))
		{
			return;
		}
		lineFollow.SetActive(isVisible && GunLineEnabled);
	}

	public static void SetupRaycast()
	{
		bool useMouseRay = Mouse.current != null && (Mouse.current.rightButton.isPressed || Mouse.current.leftButton.isPressed);
		Transform controllerTransform = GunControllerTransform;
		if ((UnityEngine.Object)(object)_thirdPersonCameraCache == (UnityEngine.Object)null && (UnityEngine.Object)(object)Variables.thirdPersonCamera != (UnityEngine.Object)null)
		{
			_thirdPersonCameraCache = Variables.thirdPersonCamera.GetComponent<Camera>();
		}
		Ray val = (useMouseRay && (UnityEngine.Object)(object)_thirdPersonCameraCache != (UnityEngine.Object)null) ? _thirdPersonCameraCache.ScreenPointToRay((Vector2)(((InputControl<Vector2>)(object)((Pointer)Mouse.current).position).ReadValue())) : new Ray(controllerTransform.position - controllerTransform.up, -controllerTransform.up);
		raycastHasHit = Physics.Raycast(val, out raycastHit, 100f, Variables.NoInvisLayers());
		Vector3 position;
		if ((UnityEngine.Object)(object)lockedTargetRig != (UnityEngine.Object)null)
		{
			position = ((Component)lockedTargetRig).transform.position;
		}
		else
		{
			position = raycastHasHit ? raycastHit.point : val.origin + val.direction * 100f;
		}
		SetupGunObjectPositions(position);
	}

	public static void SetupGunObjectPositions(Vector3 pos)
	{
		if ((UnityEngine.Object)(object)gunPointer == (UnityEngine.Object)null)
		{
			gunPointer = GameObject.CreatePrimitive((PrimitiveType)0);
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)gunPointer.GetComponent<Rigidbody>());
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)gunPointer.GetComponent<SphereCollider>());
			gunPointerRenderer = gunPointer.GetComponent<Renderer>();
			gunPointerMaterial = new Material(Variables.guiShader);
			gunPointerRenderer.sharedMaterial = gunPointerMaterial;
			gunPointer.transform.localScale = (BigGunPointer ? _scaleBig : _scaleSmall);
		}
		else
		{
			gunPointer.transform.localScale = (BigGunPointer ? _scaleBig : _scaleSmall);
		}
		Color32 val = CurrentGunColor;
		gunPointer.transform.position = pos;
		gunPointerMaterial.color = (Color32)(val);
		if (!GunLineEnabled)
		{
			if ((UnityEngine.Object)(object)lineFollow != (UnityEngine.Object)null && lineFollow.activeSelf)
			{
				lineFollow.SetActive(false);
			}
			SetGunVisibility(isVisible: true);
			return;
		}
		if ((UnityEngine.Object)(object)lineMaterial == (UnityEngine.Object)null)
		{
			lineMaterial = new Material(Variables.guiShader);
		}
		if ((UnityEngine.Object)(object)lineFollow == (UnityEngine.Object)null)
		{
			lineFollow = new GameObject("LineParent");
		}
		if (lineCylinders == null)
		{
			lineCylinders = (GameObject[])(object)new GameObject[50];
			cylinderRenderers = (Renderer[])(object)new Renderer[50];
			for (int i = 0; i < 50; i++)
			{
				lineCylinders[i] = GameObject.CreatePrimitive((PrimitiveType)2);
				lineCylinders[i].transform.SetParent(lineFollow.transform);
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)lineCylinders[i].GetComponent<Collider>());
				cylinderRenderers[i] = lineCylinders[i].GetComponent<Renderer>();
				cylinderRenderers[i].sharedMaterial = lineMaterial;
			}
		}
		Transform gunVisualHandTransform = GunVisualHandTransform;
		Vector3 position = gunVisualHandTransform.position;
		lineMaterial.color = (Color32)(val);
		if (Settings.GunAnimationType == "Wiggly")
		{
			Vector3 right = gunVisualHandTransform.right;
			float time = Time.time;
			for (int i = 0; i < 49; i++)
			{
				if (!lineCylinders[i].activeSelf)
				{
					lineCylinders[i].SetActive(true);
				}
				float startT = (float)i / 49f;
				float endT = (float)(i + 1) / 49f;
				Vector3 start = Vector3.Lerp(position, pos, startT) + right * (Mathf.Sin(time * 7.5f + startT * MathF.PI * 2f) * 0.05f);
				Vector3 end = Vector3.Lerp(position, pos, endT) + right * (Mathf.Sin(time * 7.5f + endT * MathF.PI * 2f) * 0.05f);
				float distance = Vector3.Distance(start, end);
				lineCylinders[i].transform.position = (start + end) * 0.5f;
				lineCylinders[i].transform.rotation = Quaternion.LookRotation(end - start) * Quaternion.Euler(90f, 0f, 0f);
				lineCylinders[i].transform.localScale = new Vector3(0.015f, distance * 0.5f, 0.015f);
			}
			if (lineCylinders[49].activeSelf)
			{
				lineCylinders[49].SetActive(false);
			}
		}
		else
		{
			if (!lineCylinders[0].activeSelf)
			{
				lineCylinders[0].SetActive(true);
			}
			float distance = Vector3.Distance(position, pos);
			lineCylinders[0].transform.position = (position + pos) * 0.5f;
			lineCylinders[0].transform.rotation = Quaternion.LookRotation(pos - position) * Quaternion.Euler(90f, 0f, 0f);
			lineCylinders[0].transform.localScale = new Vector3(0.015f, distance * 0.5f, 0.015f);
			for (int i = 1; i < 50; i++)
			{
				if (lineCylinders[i].activeSelf)
				{
					lineCylinders[i].SetActive(false);
				}
			}
		}
		SetGunVisibility(isVisible: true);
	}

	public static bool SetupLockOnGun()
	{
		if (!GunGrips)
		{
			CancelGunUse();
			return false;
		}
		if ((UnityEngine.Object)(object)lockedTargetRig != (UnityEngine.Object)null && lockedTargetRig.Creator == null)
		{
			lockedTargetRig = null;
		}
		SetupRaycast();
		if (!GunTriggers)
		{
			lockedTargetRig = null;
			return false;
		}
		if ((UnityEngine.Object)(object)lockedTargetRig == (UnityEngine.Object)null && raycastHasHit)
		{
			Collider collider = raycastHit.collider;
			VRRig val = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null && (UnityEngine.Object)(object)val != (UnityEngine.Object)(object)VRRig.LocalRig)
			{
				lockedTargetRig = val;
			}
		}
		return (UnityEngine.Object)(object)lockedTargetRig != (UnityEngine.Object)null;
	}

	public static void CancelGunUse()
	{
		lockedTargetRig = null;
		potentialTargetRig = null;
		SetGunVisibility(isVisible: false);
	}
}
