using BepInEx;
using UnityEngine;
using Valve.VR;

namespace NXO.Mods.Categories;

public class Movement
{
	public static bool useGripForSpeedBoost = false;

	public static bool noclipEnabled;

	public static bool pressedPrimary = false;

	public static bool GrippySurfaces = false;

	public static float CurrentCarSpeed = 0f;

	public static bool SlipperySurfaces = false;

	public static Vector3 OldMousePosition;

	public static float Sensitivity = 0.3f;

	public static bool didPress = false;

	public static bool isTriggerPlatforms = false;

	private static bool longJumpPressed = false;

	private static Vector3 wallContactPoint;

	private static Vector3 wallContactNormal;

	private static GameObject checkpoint;

	private static GameObject leftPlatform;

	private static GameObject rightPlatform;

	private static GameObject leftIce;

	private static GameObject rightIce;

	private static GameObject[] leftSides;

	private static GameObject[] rightSides;

	private static Material _platformMat;

	private static readonly Vector3 _platformScale = new Vector3(0.28f, 0.015f, 0.28f);

	private static readonly Vector3 _sideX = new Vector3(0.0075f, 0.15f, 0.15f);

	private static readonly Vector3 _sideY = new Vector3(0.15f, 0.0075f, 0.15f);

	private static readonly Vector3 _sideZ = new Vector3(0.15f, 0.15f, 0.0075f);

	private static bool TouchLeft = false;

	private static bool TouchRight = false;

	private static void InitializeMaterial()
	{
		if ((UnityEngine.Object)(object)_platformMat == (UnityEngine.Object)null)
		{
			_platformMat = new Material(Variables.uiShader);
			_platformMat.color = Color.black;
		}
	}

	public static void AntiGravity()
	{
		Variables.taggerInstance.rigidbody.AddForce(Vector3.up * 9.81f, (ForceMode)5);
	}

	public static void WASDMovement()
	{
		Transform transform = ((Component)Variables.taggerInstance.headCollider).transform;
		float num = UnityInput.Current.GetKey((KeyCode)304) ? (Settings.FlySpeed + 3f) : Settings.FlySpeed;
		((Component)Variables.playerInstance).GetComponent<Rigidbody>().linearVelocity = new Vector3(0f, 0.065f, 0f);
		Vector3 val = Vector3.zero;
		if (UnityInput.Current.GetKey((KeyCode)119))
		{
			val += transform.forward;
		}
		if (UnityInput.Current.GetKey((KeyCode)115))
		{
			val -= transform.forward;
		}
		if (UnityInput.Current.GetKey((KeyCode)97))
		{
			val -= transform.right;
		}
		if (UnityInput.Current.GetKey((KeyCode)100))
		{
			val += transform.right;
		}
		if (UnityInput.Current.GetKey((KeyCode)32))
		{
			val += transform.up;
		}
		if (UnityInput.Current.GetKey((KeyCode)306))
		{
			val -= transform.up;
		}
		Transform transform2 = ((Component)Variables.taggerInstance.headCollider).transform;
		transform2.position += val.normalized * num * Time.deltaTime;
		if (UnityInput.Current.GetMouseButton(1))
		{
			Vector3 val2 = UnityInput.Current.mousePosition - OldMousePosition;
			float num2 = 0.1f;
			Transform transform3 = Variables.taggerInstance.mainCamera.transform;
			transform3.localEulerAngles += new Vector3((0f - val2.y) * num2, val2.x * num2, 0f);
		}
		OldMousePosition = UnityInput.Current.mousePosition;
	}

	public static void TriggerFly()
	{
		if (InputHandler.RTrigger())
		{
			Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
			Transform transform = ((Component)Variables.playerInstance.headCollider).transform;
			Transform transform2 = ((Component)Variables.playerInstance).transform;
			transform2.position += transform.forward * Time.deltaTime * Settings.FlySpeed;
			component.linearVelocity = Vector3.zero;
		}
	}

	public static void HandFly()
	{
		Vector3 direction = Vector3.zero;
		if (InputHandler.LGrip())
		{
			direction += -Variables.playerInstance.LeftHand.controllerTransform.up;
		}
		if (InputHandler.RGrip())
		{
			direction += -Variables.playerInstance.RightHand.controllerTransform.up;
		}
		if (direction == Vector3.zero)
		{
			return;
		}
		Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
		Transform transform = ((Component)Variables.playerInstance).transform;
		transform.position += direction.normalized * Time.deltaTime * Settings.FlySpeed;
		component.linearVelocity = Vector3.zero;
	}

	public static void AutoWalk()
	{
		Vector3 forward = ((Component)Variables.playerInstance.headCollider).transform.forward;
		forward.y = 0f;
		if (forward.sqrMagnitude < 0.0001f)
		{
			return;
		}
		Transform transform = ((Component)Variables.playerInstance).transform;
		transform.position += forward.normalized * Time.deltaTime * Mathf.Max(2.5f, Settings.SpeedboostSpeed * 0.35f);
	}

	public static void SpeedBoost()
	{
		if (!useGripForSpeedBoost || InputHandler.RGrip())
		{
			Variables.playerInstance.maxJumpSpeed = Settings.SpeedboostSpeed;
			Variables.playerInstance.jumpMultiplier = Settings.SpeedboostMultiplier;
		}
	}

	public static void PlaceCheckPoint(bool setActive)
	{
		if (!setActive)
		{
			if ((UnityEngine.Object)(object)checkpoint != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)checkpoint);
				checkpoint = null;
			}
			return;
		}
		if ((UnityEngine.Object)(object)checkpoint == (UnityEngine.Object)null)
		{
			checkpoint = GameObject.CreatePrimitive((PrimitiveType)0);
			checkpoint.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
			Renderer component = checkpoint.GetComponent<Renderer>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.material.shader = Shader.Find("GUI/Text Shader");
				component.material.color = Color.red;
			}
		}
		checkpoint.GetComponent<Collider>().enabled = false;
		if (InputHandler.RGrip())
		{
			checkpoint.transform.position = Variables.playerInstance.RightHand.controllerTransform.position;
			checkpoint.GetComponent<Renderer>().material.color = Color.red;
			return;
		}
		if (InputHandler.RTrigger())
		{
			if ((UnityEngine.Object)(object)checkpoint != (UnityEngine.Object)null)
			{
				Renderer component2 = checkpoint.GetComponent<Renderer>();
				if ((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null)
				{
					component2.material.color = Color.green;
					((Component)Variables.playerInstance).GetComponent<Rigidbody>().velocity = Vector3.zero;
					((Component)Variables.playerInstance).transform.position = checkpoint.transform.position;
				}
				else
				{
					((Component)Variables.playerInstance).GetComponent<Rigidbody>().velocity = Vector3.zero;
					((Component)Variables.playerInstance).transform.position = checkpoint.transform.position;
				}
			}
		}
		else
		{
			Renderer component3 = checkpoint.GetComponent<Renderer>();
			if ((UnityEngine.Object)(object)component3 != (UnityEngine.Object)null)
			{
				component3.material.color = Color.red;
			}
		}
	}

	public static void NoTagFreeze()
	{
		Variables.playerInstance.disableMovement = false;
	}

	private static void FrozonePlatforms(ref GameObject ice, bool grabbing, Transform hand)
	{
		if (grabbing)
		{
			ice = GameObject.CreatePrimitive((PrimitiveType)3);
			ice.transform.localScale = _platformScale;
			ice.transform.position = hand.position + new Vector3(0f, -0.06f, 0f);
			ice.transform.rotation = hand.rotation * Quaternion.Euler(0f, 0f, -90f);
			ice.GetComponent<Renderer>().material.color = new Color(0.525f, 0.839f, 0.847f);
			ice.AddComponent<GorillaSurfaceOverride>().overrideIndex = 59;
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)ice, 0.75f);
		}
	}

	public static void DashAndAirJump(bool isDashEnabled, bool isAirJumpEnabled)
	{
		if (InputHandler.RPrimary())
		{
			if (!pressedPrimary)
			{
				pressedPrimary = true;
				if (isDashEnabled)
				{
					((Component)Variables.playerInstance).GetComponent<Rigidbody>().velocity = ((Component)Variables.playerInstance.headCollider).transform.forward * 9f;
				}
				if (isAirJumpEnabled)
				{
					Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
					component.velocity += Vector3.up * 7f;
				}
			}
			return;
		}
		pressedPrimary = false;
	}

	public static void PullMod()
	{
		bool flag = Variables.playerInstance.IsHandTouching(true);
		bool flag2 = Variables.playerInstance.IsHandTouching(false);
		if ((!flag && TouchLeft) || (!flag2 && TouchRight))
		{
			Vector3 velocity = Variables.taggerInstance.rigidbody.velocity;
			Transform transform = ((Component)Variables.playerInstance).transform;
			transform.position += new Vector3(velocity.x * 0.05f, 0f, velocity.z * 0.05f);
		}
		TouchLeft = flag;
		TouchRight = flag2;
	}

	public static void JoystickFly()
	{
		Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
		Transform transform = ((Component)Variables.playerInstance.headCollider).transform;
		component.useGravity = false;
		component.linearVelocity = Vector3.zero;
		Vector2 axis = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.axis;
		if (axis.magnitude > 0.1f)
		{
			Vector3 val = transform.forward * axis.y + transform.right * axis.x;
			Transform transform2 = ((Component)Variables.playerInstance).transform;
			transform2.position += val * Time.deltaTime * Settings.FlySpeed;
		}
	}

	public static void SlippyHands(bool setActive)
	{
		SlipperySurfaces = setActive;
	}

	public static void GrippyHands(bool setActive)
	{
		GrippySurfaces = setActive;
	}

	public static void Noclip()
	{
		SetNoclipActive(InputHandler.RTrigger());
	}

	public static void GripNoclip()
	{
		SetNoclipActive(InputHandler.LGrip() || InputHandler.RGrip());
	}

	public static void ConstantNoclip()
	{
		SetNoclipActive(active: true);
	}

	public static void DisableNoclip()
	{
		SetNoclipActive(active: false);
	}

	private static void SetNoclipActive(bool active)
	{
		if (active != noclipEnabled)
		{
			noclipEnabled = active;
			MeshCollider[] array = UnityEngine.Object.FindObjectsOfType<MeshCollider>();
			foreach (MeshCollider val in array)
			{
				((Collider)val).enabled = !noclipEnabled;
			}
		}
	}

	public static void IronMonke(int flySpeed)
	{
		if (InputHandler.LGrip())
		{
			((Collider)Variables.playerInstance.bodyCollider).attachedRigidbody.AddForce((float)flySpeed * -Variables.taggerInstance.leftHandTransform.right, (ForceMode)5);
			if (!InputHandler.RGrip())
			{
				return;
			}
		}
		else if (!InputHandler.RGrip())
		{
			return;
		}
		((Collider)Variables.playerInstance.bodyCollider).attachedRigidbody.AddForce((float)flySpeed * Variables.taggerInstance.rightHandTransform.right, (ForceMode)5);
	}

	public static void UpAndDown()
	{
		Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
		Transform transform = ((Component)Variables.playerInstance.bodyCollider).transform;
		if (InputHandler.RTrigger())
		{
			component.velocity += transform.up * 20f * Time.deltaTime;
			if (!InputHandler.LTrigger())
			{
				return;
			}
		}
		else if (!InputHandler.LTrigger())
		{
			return;
		}
		component.velocity -= transform.up * 20f * Time.deltaTime;
	}

	public static void ReverseGravity()
	{
		Variables.taggerInstance.rigidbody.AddForce(Vector3.up * 19.62f, (ForceMode)5);
	}

	public static void LowGravity()
	{
		Variables.taggerInstance.rigidbody.AddForce(Vector3.up * 6.5f, (ForceMode)5);
	}

	private static GameObject CreatePlatform(Vector3 scale, Vector3 pos, Quaternion rot, Material mat, bool invisible)
	{
		GameObject val = GameObject.CreatePrimitive((PrimitiveType)3);
		val.transform.localScale = scale;
		val.transform.position = pos;
		val.transform.rotation = rot;
		Renderer component = val.GetComponent<Renderer>();
		component.material = mat;
		component.enabled = !invisible;
		return val;
	}

	public static void WallWalk()
	{
		if (Variables.playerInstance.LeftHand.wasColliding || Variables.playerInstance.RightHand.wasColliding)
		{
			RaycastHit lastHitInfoHand = Variables.playerInstance.lastHitInfoHand;
			wallContactPoint = lastHitInfoHand.point;
			wallContactNormal = lastHitInfoHand.normal;
			if (!(wallContactPoint != Vector3.zero))
			{
				return;
			}
		}
		else if (!(wallContactPoint != Vector3.zero))
		{
			return;
		}
		if (!InputHandler.RGrip())
		{
			if (!InputHandler.LGrip())
			{
				return;
			}
		}
		else if (1 == 0)
		{
			return;
		}
		((Collider)Variables.playerInstance.bodyCollider).attachedRigidbody.AddForce(wallContactNormal * Settings.WallWalkStrength, (ForceMode)5);
	}

	public static void TogglePlatforms()
	{
		Transform controllerTransform = Variables.playerInstance.LeftHand.controllerTransform;
		Transform controllerTransform2 = Variables.playerInstance.RightHand.controllerTransform;
		bool grabbing = PlatformInput(rightHand: false);
		bool grabbing2 = PlatformInput(rightHand: true);
		switch (Settings.PlatformType.ToLower())
		{
		case "normal":
			Platforms(ref leftPlatform, grabbing, controllerTransform, invisible: false);
			Platforms(ref rightPlatform, grabbing2, controllerTransform2, invisible: false);
			break;
		case "sticky":
			StickyPlatforms(ref leftSides, grabbing, controllerTransform, invisible: false);
			StickyPlatforms(ref rightSides, grabbing2, controllerTransform2, invisible: false);
			break;
		case "invisible":
			Platforms(ref leftPlatform, grabbing, controllerTransform, invisible: true);
			Platforms(ref rightPlatform, grabbing2, controllerTransform2, invisible: true);
			break;
		case "invisible sticky":
			StickyPlatforms(ref leftSides, grabbing, controllerTransform, invisible: true);
			StickyPlatforms(ref rightSides, grabbing2, controllerTransform2, invisible: true);
			break;
		}
	}

	private static void StickyPlatforms(ref GameObject[] sides, bool grabbing, Transform hand, bool invisible)
	{
		if (grabbing)
		{
			if (sides != null)
			{
				return;
			}
			InitializeMaterial();
			sides = (GameObject[])(object)new GameObject[6];
			Vector3[] array = (Vector3[])(object)new Vector3[6]
			{
				hand.right * -0.075f,
				hand.right * 0.075f,
				hand.up * 0.075f,
				hand.up * -0.075f,
				hand.forward * 0.075f,
				hand.forward * -0.075f
			};
			Vector3[] array2 = (Vector3[])(object)new Vector3[6] { _sideX, _sideX, _sideY, _sideY, _sideZ, _sideZ };
			int num = 0;
			if (num < 6)
			{
				do
				{
					sides[num] = CreatePlatform(array2[num], hand.position + array[num], hand.rotation, _platformMat, invisible);
					num++;
				}
				while (num < 6);
			}
		}
		else
		{
			if (sides == null)
			{
				return;
			}
			GameObject[] array3 = sides;
			int num2 = 0;
			while (num2 < array3.Length)
			{
				GameObject val = array3[num2];
				if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)val);
					num2++;
				}
				else
				{
					num2++;
				}
			}
			sides = null;
		}
	}

	private static void Platforms(ref GameObject platform, bool grabbing, Transform hand, bool invisible)
	{
		if (grabbing)
		{
			if ((UnityEngine.Object)(object)platform == (UnityEngine.Object)null)
			{
				InitializeMaterial();
				platform = CreatePlatform(_platformScale, hand.position + new Vector3(0f, -0.02f, 0f), hand.rotation * Quaternion.Euler(0f, 0f, -90f), _platformMat, invisible);
			}
		}
		else if ((UnityEngine.Object)(object)platform != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)platform);
			platform = null;
		}
	}

	public static void FastSwim()
	{
		if (Variables.playerInstance.InWater)
		{
			Rigidbody component = ((Component)Variables.playerInstance).gameObject.GetComponent<Rigidbody>();
			component.velocity *= 1.03f;
		}
	}

	private static bool PlatformInput(bool rightHand)
	{
		return (!isTriggerPlatforms) ? (rightHand ? InputHandler.RGrip() : InputHandler.LGrip()) : (rightHand ? InputHandler.RTrigger() : InputHandler.LTrigger());
	}

	public static void FlyNoclip()
	{
		bool active = InputHandler.RPrimary();
		if (active)
		{
			Transform transform = ((Component)Variables.playerInstance).transform;
			transform.position += ((Component)Variables.playerInstance.headCollider).transform.forward * Time.deltaTime * Settings.FlySpeed;
			((Component)Variables.playerInstance).GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
		}
		SetNoclipActive(active);
	}

	public static void FeatherFalling()
	{
		Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
		Vector3 linearVelocity = component.linearVelocity;
		if (linearVelocity.y < -2f)
		{
			linearVelocity.y = -2f;
			component.linearVelocity = linearVelocity;
		}
	}

	public static void LongJump()
	{
		if (InputHandler.RPrimary())
		{
			if (longJumpPressed)
			{
				return;
			}
			longJumpPressed = true;
			Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
			Vector3 forward = ((Component)Variables.playerInstance.headCollider).transform.forward;
			forward.y = 0f;
			if (forward.sqrMagnitude < 0.0001f)
			{
				forward = ((Component)Variables.playerInstance).transform.forward;
			}
			component.linearVelocity = forward.normalized * 8f + Vector3.up * 5f;
			return;
		}
		longJumpPressed = false;
	}

	public static void HighGravity()
	{
		Variables.taggerInstance.rigidbody.AddForce(Vector3.down * 8f, (ForceMode)5);
	}

	public static void FlyMonke(bool useVelocity, float velocityMultiplier = 1.15f)
	{
		Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
		if (InputHandler.RPrimary())
		{
			if (useVelocity)
			{
				component.angularVelocity += ((Component)Variables.playerInstance.headCollider).transform.forward * Settings.FlySpeed * velocityMultiplier * Time.deltaTime;
				return;
			}
			Transform transform = ((Component)Variables.playerInstance).transform;
			transform.position += ((Component)Variables.playerInstance.headCollider).transform.forward * Time.deltaTime * Settings.FlySpeed;
			component.angularVelocity = Vector3.zero;
		}
	}

	public static void Frozone()
	{
		FrozonePlatforms(ref leftIce, PlatformInput(rightHand: false), Variables.playerInstance.LeftHand.controllerTransform);
		FrozonePlatforms(ref rightIce, PlatformInput(rightHand: true), Variables.playerInstance.RightHand.controllerTransform);
	}

	public static void SlideControl(bool setActive)
	{
		Variables.playerInstance.slideControl = (setActive ? 0.04f : 0.00425f);
	}
}
