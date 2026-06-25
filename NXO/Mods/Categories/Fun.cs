using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using GorillaNetworking;
using HarmonyLib;
using NXO.Menu;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using CosmeticItem = GorillaNetworking.CosmeticsController.CosmeticItem;
using CosmeticSet = GorillaNetworking.CosmeticsController.CosmeticSet;
using Controller = OVRInput.Controller;
using WeatherType = BetterDayNightManager.WeatherType;

namespace NXO.Mods.Categories;

public class Fun
{
	private static readonly Vector3[] lastRight = (Vector3[])(object)new Vector3[10];

	private static readonly Vector3[] lastLeft = (Vector3[])(object)new Vector3[10];

	private static GameObject rightPointer;

	private static GameObject leftPointer;

	private static readonly HashSet<GameObject> drawings = new HashSet<GameObject>();

	private static bool colorChangerCooldown = false;

	private static int currentColor = 0;

	private static Color drawColor = Color.white;

	private static GameObject bomb;

	private static float orbitSpeed = 100f;

	private static float orbitRadius = 1.5f;

	private static float orbitAngle = 0f;

	private static bool becomingEntity;

	private static bool togglePressed;

	private static float hoverboardDelay;

	private static Coroutine hoverboardCoroutine;

	public static Coroutine BugCoroutine;

	private static bool lastBecomeWasNull;

	private static float hoverboardAuraDelay;

	private static bool strobeState;

	private static float strobeDelay;

	private static WeatherType[] originalWeatherCycle;

	private static GameObject[] elevatorAmbientObjects;

	private static float splashCooldown = 0.25f;

	private static float rpcCooldown;

	private static WaterVolume[] cachedWater;

	public static GameObject airSwimPart;

	private static int[] archiveCosmetics;

	private static int[] cachedLmajuCosmetics;

	private static bool cosmeticsEnabled;

	private static float braceletSpamDelay;

	public static bool unlockedCosmetics;

	private static GameObject rightaimer;

	private static GameObject leftaimer;

	private static LineRenderer lr;

	private static LineRenderer leftlr;

	private static SpringJoint joint;

	private static SpringJoint leftjoint;

	private static bool cangrapple = true;

	private static bool canleftgrapple = true;

	private static bool wackstart = false;

	private const float maxDistance = 100f;

	private const float Spring = 5000f;

	private const float Damper = 4000f;

	private const float MassScale = 6f;

	private const float pullspeed = 3f;

	private const float speedtopull = 2.5f;

	private static GameObject rightGrappleAimer;

	private static GameObject leftGrappleAimer;

	private static LineRenderer grappleLR;

	private static LineRenderer leftGrappleLR;

	private static Vector3 rightGrapplePoint;

	private static Vector3 leftGrapplePoint;

	private static bool rightGrappleAttached;

	private static bool leftGrappleAttached;

	private static bool grappleStart;

	public static void AddBarrelToCart()
	{
		((CosmeticsController)CosmeticsController.instance).currentCart.Insert(0, ((CosmeticsController)CosmeticsController.instance).GetItemFromDict("LMAPE."));
	}

	public static void GliderGun()
	{
		if (!GunLib.SetupGun())
		{
			return;
		}
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		int num = 0;
		while (num < allType.Length)
		{
			GliderHoldable val = allType[num];
			if (((NetworkView)val).GetView.Owner == PhotonNetwork.LocalPlayer)
			{
				((Component)val).gameObject.transform.position = GunLib.raycastHit.point + Vector3.up;
				num++;
			}
			else
			{
				((NetworkHoldableObject)val).OnHover((InteractionPoint)null, (GameObject)null);
				num++;
			}
		}
	}

	private static void HandleWebshooter(ref SpringJoint jnt, ref LineRenderer line, ref GameObject aim, ref bool canGrap, Transform hand, float trigger, Controller ctrl, bool isRight)
	{
		RaycastHit val = default(RaycastHit);
		bool hasHit = Physics.Raycast(hand.position, hand.forward, out val, 100f);
		if (hasHit)
		{
			if ((UnityEngine.Object)(object)aim == (UnityEngine.Object)null)
			{
				aim = GameObject.CreatePrimitive((PrimitiveType)0);
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)aim.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)aim.GetComponent<SphereCollider>());
				aim.transform.localScale = Vector3.one * 0.2f;
				aim.GetComponent<Renderer>().material.color = Color.green;
				aim.SetActive(true);
				aim.transform.position = val.point;
			}
			else
			{
				aim.SetActive(true);
				aim.transform.position = val.point;
			}
		}
		if (trigger <= 0.1f)
		{
			if ((UnityEngine.Object)(object)jnt != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)jnt);
				jnt = null;
			}
			line.positionCount = 0;
			canGrap = true;
			return;
		}
		if (canGrap && hasHit)
		{
			jnt = ((Component)Variables.playerInstance).gameObject.AddComponent<SpringJoint>();
			((Joint)jnt).autoConfigureConnectedAnchor = false;
			((Joint)jnt).connectedAnchor = val.point;
			float num = Vector3.Distance(((Component)Variables.playerInstance).transform.position, val.point);
			jnt.maxDistance = num * 0.8f;
			jnt.minDistance = num * 0.25f;
			jnt.spring = 5000f;
			jnt.damper = 4000f;
			((Joint)jnt).massScale = 6f;
			line.positionCount = 2;
			line.SetPosition(1, val.point);
			Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(82, isRight, 1f);
			canGrap = false;
			if (!((UnityEngine.Object)(object)jnt != (UnityEngine.Object)null))
			{
				return;
			}
		}
		else if (!((UnityEngine.Object)(object)jnt != (UnityEngine.Object)null))
		{
			return;
		}
		line.positionCount = 2;
		line.SetPosition(0, hand.position);
		line.SetPosition(1, ((Joint)jnt).connectedAnchor);
		Vector3 val2;
		if ((UnityEngine.Object)(object)aim != (UnityEngine.Object)null)
		{
			aim.transform.position = ((Joint)jnt).connectedAnchor;
			val2 = OVRInput.GetLocalControllerVelocity(ctrl);
			if (!(val2.magnitude >= 2.5f))
			{
				return;
			}
		}
		else
		{
			val2 = OVRInput.GetLocalControllerVelocity(ctrl);
			if (!(val2.magnitude >= 2.5f))
			{
				return;
			}
		}
		Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
		val2 = ((Joint)jnt).connectedAnchor - ((Component)Variables.playerInstance).transform.position;
		component.AddForce(val2.normalized * 3f, (ForceMode)2);
	}

	public static void SilentHandTaps()
	{
		MenuPatches.handtapEnabled = true;
		MenuPatches.tapsEnabled = false;
		MenuPatches.doOverride = false;
		MenuPatches.overrideVolume = 0f;
		MenuPatches.tapMultiplier = 0;
		Variables.taggerInstance.handTapVolume = 0f;
	}

	public static void BraceletSpam()
	{
		if (Time.time > braceletSpamDelay)
		{
			GetBracelet(Time.frameCount % 2 == 0);
			braceletSpamDelay = Time.time + 0.1f;
		}
	}

	public static void OrbitGliders()
	{
		if (!InputHandler.RTrigger())
		{
			return;
		}
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		int num = 0;
		GliderHoldable[] array = allType;
		int num2 = 0;
		while (num2 < array.Length)
		{
			GliderHoldable val = array[num2];
			if (((NetworkView)val).GetView.Owner == PhotonNetwork.LocalPlayer)
			{
				float num3 = 360f / (float)allType.Length * (float)num;
				((Component)val).gameObject.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position + new Vector3(MathF.Cos(num3 + (float)Time.frameCount / 30f) * 5f, 2f, MathF.Sin(num3 + (float)Time.frameCount / 30f) * 5f);
				num++;
				num2++;
			}
			else
			{
				((NetworkHoldableObject)val).OnHover((InteractionPoint)null, (GameObject)null);
				num++;
				num2++;
			}
		}
	}

	private static void CleanupGrapple()
	{
		if ((UnityEngine.Object)(object)grappleLR != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Renderer)grappleLR).material);
		}
		if ((UnityEngine.Object)(object)leftGrappleLR != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Renderer)leftGrappleLR).material);
		}
		Main.DestroyAndNullify<GameObject>(ref rightGrappleAimer, 0f);
		Main.DestroyAndNullify<GameObject>(ref leftGrappleAimer, 0f);
		Main.DestroyAndNullify<LineRenderer>(ref grappleLR, 0f);
		Main.DestroyAndNullify<LineRenderer>(ref leftGrappleLR, 0f);
		rightGrappleAttached = (leftGrappleAttached = (grappleStart = false));
	}

	public static void WaterBarrage()
	{
		if (InputHandler.RTrigger() && !(Time.time < rpcCooldown))
		{
			rpcCooldown = Time.time + splashCooldown;
			Safety.RPCShield();
			Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
			Vector3 val = insideUnitSphere.normalized * UnityEngine.Random.Range(1.5f, 1.6f);
			Vector3 val2 = ((Component)Variables.taggerInstance.offlineVRRig).transform.position + val;
			PlaySplashEffect(val2, Quaternion.LookRotation(((Component)Variables.taggerInstance.offlineVRRig).transform.position - val2), 125f);
		}
	}

	public static void FixHandTaps()
	{
		MenuPatches.handtapEnabled = false;
		MenuPatches.tapsEnabled = true;
		MenuPatches.doOverride = false;
		MenuPatches.overrideVolume = 0.1f;
		MenuPatches.tapMultiplier = 1;
		Variables.taggerInstance.handTapVolume = 0.1f;
	}

	public static void BecomeEntity(string name)
	{
		GameObject val = GameObject.Find(name);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			Player.SetRigStatus(rigStatus: false);
			((Component)VRRig.LocalRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position - Vector3.up * 99999f;
			val.transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position;
			val.transform.rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation;
		}
		else if (lastBecomeWasNull)
		{
			Player.SetRigStatus(rigStatus: true);
			lastBecomeWasNull = (UnityEngine.Object)(object)val != (UnityEngine.Object)null;
			return;
		}
		lastBecomeWasNull = (UnityEngine.Object)(object)val != (UnityEngine.Object)null;
	}

	public static void LoudHandTaps()
	{
		MenuPatches.handtapEnabled = true;
		MenuPatches.tapsEnabled = true;
		MenuPatches.doOverride = true;
		MenuPatches.overrideVolume = 99999f;
		MenuPatches.tapMultiplier = 10;
		Variables.taggerInstance.handTapVolume = 99999f;
	}

	private static IEnumerator ResetHoverboard()
	{
		yield return (object)new WaitForSeconds(0.3f);
		GTPlayer.Instance.SetHoverActive(false);
		VRRig.LocalRig.hoverboardVisual.SetNotHeld();
	}

	public static void GliderSpeed(float pull, float drag)
	{
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		foreach (GliderHoldable val in allType)
		{
			val.pullUpLiftBonus = pull;
			val.dragVsSpeedDragFactor = drag;
		}
	}

	public static void WaterBender()
	{
		if (Time.time < rpcCooldown)
		{
			return;
		}
		rpcCooldown = Time.time + splashCooldown;
		Safety.RPCShield();
		bool rightGrip = InputHandler.RGrip();
		bool leftGrip = InputHandler.LGrip();
		if (rightGrip && leftGrip)
		{
			Vector3 pos = (Variables.playerInstance.RightHand.controllerTransform.position + Variables.playerInstance.LeftHand.controllerTransform.position) / 2f;
			float num = Vector3.Distance(Variables.playerInstance.RightHand.controllerTransform.position, Variables.playerInstance.LeftHand.controllerTransform.position);
			PlaySplashEffect(pos, Quaternion.Lerp(Variables.playerInstance.RightHand.controllerTransform.rotation, Variables.playerInstance.LeftHand.controllerTransform.rotation, 0.5f), Mathf.Clamp(num * 100f, 75f, 500f));
		}
		else
		{
			if (rightGrip)
			{
				PlaySplashEffect(Variables.playerInstance.RightHand.controllerTransform.position, ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation, 125f);
			}
			if (leftGrip)
			{
				PlaySplashEffect(Variables.playerInstance.LeftHand.controllerTransform.position, ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation, 125f);
			}
		}
		if (InputHandler.RPrimary())
		{
			PlaySplashEffect(Variables.taggerInstance.offlineVRRig.headMesh.transform.position + Vector3.up * 0.5f, Quaternion.Euler(90f, 0f, 0f), 125f);
		}
	}

	public static int[] PackedCosmetics(string[] cosmetics)
	{
		return new CosmeticSet(cosmetics, CosmeticsController.instance).ToPackedIDArray();
	}

	public static void GetBracelet(bool state)
	{
		bool leftGrip = InputHandler.LGrip();
		bool rightGrip = InputHandler.RGrip();
		if (leftGrip)
		{
			SetBraceletState(enable: false, left: false);
			SetBraceletState(state, left: true);
		}
		if (rightGrip)
		{
		SetBraceletState(state, left: false);
		SetBraceletState(enable: false, left: true);
		}
		if (leftGrip || rightGrip)
		{
			Safety.RPCShield();
		}
	}

	public static void GrabObject(string name)
	{
		if (InputHandler.RGrip())
		{
			Variables.FindObject(name).transform.position = Variables.taggerInstance.rightHandTransform.position;
		}
	}

	private static void CleanupWebshooters()
	{
		if ((UnityEngine.Object)(object)lr != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Renderer)lr).material);
		}
		if ((UnityEngine.Object)(object)leftlr != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Renderer)leftlr).material);
		}
		Main.DestroyAndNullify<GameObject>(ref rightaimer, 0f);
		Main.DestroyAndNullify<GameObject>(ref leftaimer, 0f);
		Main.DestroyAndNullify<LineRenderer>(ref lr, 0f);
		Main.DestroyAndNullify<LineRenderer>(ref leftlr, 0f);
		Main.DestroyAndNullify<SpringJoint>(ref joint, 0f);
		Main.DestroyAndNullify<SpringJoint>(ref leftjoint, 0f);
		wackstart = false;
		cangrapple = true;
		canleftgrapple = true;
	}

	public static void GliderBlindGun()
	{
		if (!GunLib.SetupLockOnGun())
		{
			return;
		}
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		int num = 0;
		while (num < allType.Length)
		{
			GliderHoldable val = allType[num];
			if (((NetworkView)val).GetView.Owner == PhotonNetwork.LocalPlayer)
			{
				((Component)val).gameObject.transform.position = GunLib.lockedTargetRig.headMesh.transform.position;
				((Component)val).gameObject.transform.rotation = Variables.RandomQuaternion();
				num++;
			}
			else
			{
				((NetworkHoldableObject)val).OnHover((InteractionPoint)null, (GameObject)null);
				num++;
			}
		}
	}

	private static void SetBraceletState(bool enable, bool left)
	{
		Variables.taggerInstance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", (RpcTarget)0, new object[2] { enable, left });
	}

	public static void DropHoverboard(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel, Color color)
	{
		if (Vector3.Distance(((Component)Variables.taggerInstance.bodyCollider).transform.position, pos) > 5f)
		{
			((Behaviour)VRRig.LocalRig).enabled = false;
			((Component)VRRig.LocalRig).transform.position = pos + Vector3.down * 4f;
			if (hoverboardCoroutine != null)
			{
				((MonoBehaviour)CoroutineHelper.Instance).StopCoroutine(hoverboardCoroutine);
				hoverboardCoroutine = ((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(EnableRig());
				FreeHoverboardManager.instance.SendDropBoardRPC(pos, rot, vel, angVel, color);
				Safety.RPCShield();
			}
			else
			{
				hoverboardCoroutine = ((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(EnableRig());
				FreeHoverboardManager.instance.SendDropBoardRPC(pos, rot, vel, angVel, color);
				Safety.RPCShield();
			}
		}
		else
		{
			FreeHoverboardManager.instance.SendDropBoardRPC(pos, rot, vel, angVel, color);
			Safety.RPCShield();
		}
	}

	public static void RainyWeather(bool setActive)
	{
		if (setActive)
		{
			originalWeatherCycle = (WeatherType[])((BetterDayNightManager)BetterDayNightManager.instance).weatherCycle.Clone();
			int num = 1;
			if (num < ((BetterDayNightManager)BetterDayNightManager.instance).weatherCycle.Length)
			{
				do
				{
					((BetterDayNightManager)BetterDayNightManager.instance).weatherCycle[num] = (WeatherType)1;
					num++;
				}
				while (num < ((BetterDayNightManager)BetterDayNightManager.instance).weatherCycle.Length);
			}
		}
		else if (originalWeatherCycle != null)
		{
			((BetterDayNightManager)BetterDayNightManager.instance).weatherCycle = originalWeatherCycle;
			originalWeatherCycle = null;
		}
		else
		{
			((BetterDayNightManager)BetterDayNightManager.instance).GenerateWeatherEventTimes();
			originalWeatherCycle = null;
		}
	}

	public static void GrabEntity(string name)
	{
		if (InputHandler.RGrip())
		{
			GameObject val = GameObject.Find(name);
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				val.transform.position = Variables.taggerInstance.rightHandTransform.position;
				val.transform.rotation = Variables.taggerInstance.rightHandTransform.rotation;
			}
		}
	}

	public static void SplashSelf()
	{
		if (InputHandler.RTrigger() && !(Time.time < rpcCooldown))
		{
			rpcCooldown = Time.time + splashCooldown;
			Safety.RPCShield();
			PlaySplashEffect(((Component)Variables.taggerInstance.offlineVRRig).transform.position, ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation, 125f);
		}
	}

	public static void EntityGun(string name)
	{
		if (GunLib.SetupGun())
		{
			GameObject val = GameObject.Find(name);
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				val.transform.position = GunLib.raycastHit.point + Vector3.up;
			}
		}
	}

	public static void GliderBlindAll()
	{
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		int num = 0;
		if (!InputHandler.RTrigger())
		{
			return;
		}
		foreach (VRRig current in VRRigCache.ActiveRigs.Where((VRRig r) => !r.isLocal))
		{
			if (num >= allType.Length)
			{
				break;
			}
			GliderHoldable val = allType[num];
			if (((NetworkView)val).GetView.Owner == PhotonNetwork.LocalPlayer)
			{
				((Component)val).gameObject.transform.position = current.headMesh.transform.position;
				((Component)val).gameObject.transform.rotation = Variables.RandomQuaternion();
			}
			else
			{
				((NetworkHoldableObject)val).OnHover((InteractionPoint)null, (GameObject)null);
			}
			num++;
		}
	}

	public static void OrbitHoverboards()
	{
		if (Time.time < hoverboardDelay)
		{
			return;
		}
		hoverboardDelay = Time.time + 0.25f;
		Vector3 position = ((Component)Variables.taggerInstance.headCollider).transform.position;
		float num = (float)Time.frameCount / 30f;
		int num2 = 0;
		if (num2 < 2)
		{
			Vector3 val = default(Vector3);
			Vector3 val2 = default(Vector3);
			do
			{
				float num3 = (float)num2 * 180f;
				float num4 = num3 - 25f;
				val = new Vector3(MathF.Cos(num3 + num) * 2f, 1f, MathF.Sin(num3 + num) * 2f);
				val2 = new Vector3(MathF.Cos(num4 + num) * 2f, 1f, MathF.Sin(num4 + num) * 2f);
				Vector3 pos = position + val;
				Vector3 val3 = position - val;
				Vector3 val4 = val3;
				Quaternion rot = Quaternion.Euler(val4.normalized);
				val3 = val2 - val;
				val4 = val3;
				DropHoverboard(pos, rot, val4.normalized * 6.5f, new Vector3(0f, 360f, 0f), Variables.RandomColor());
				num2++;
			}
			while (num2 < 2);
		}
	}

	public static void BecomeObject(string name)
	{
		bool primary = InputHandler.RPrimary();
		if (primary && !togglePressed)
		{
			becomingEntity = !becomingEntity;
			if (becomingEntity)
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = new Vector3(999f, 999f, 999f);
				togglePressed = true;
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
				togglePressed = true;
			}
		}
		else if (!primary)
		{
			togglePressed = false;
		}
		if (becomingEntity)
		{
			Transform transform = ((Component)Variables.taggerInstance.headCollider).transform;
			Variables.FindObject(name).transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
		}
	}

	public static void ObjectOrbit(string name)
	{
		orbitAngle += orbitSpeed * Time.deltaTime;
		float num = orbitAngle * (MathF.PI / 180f);
		Variables.FindObject(name).transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position + new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num)) * orbitRadius;
	}

	private static void DestroyAllDrawings()
	{
		foreach (GameObject current in drawings)
		{
			if ((UnityEngine.Object)(object)current != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current);
			}
		}
		drawings.Clear();
	}

	public static void PunchMod()
	{
		int num = 0;
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig)
			{
				continue;
			}
			Rigidbody component = ((Component)Variables.playerInstance).GetComponent<Rigidbody>();
			Vector3 position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position;
			Vector3 position2 = current.rightHandTransform.position;
			if (Vector3.Distance(position2, position) < 0.25f)
			{
				component.AddForce(Vector3.Normalize(position2 - lastRight[num]) * 9f + Vector3.up * 2f, (ForceMode)2);
			}
			lastRight[num] = position2;
			Vector3 position3 = current.leftHandTransform.position;
			if (Vector3.Distance(position3, position) < 0.25f)
			{
				component.AddForce(Vector3.Normalize(position3 - lastLeft[num]) * 9f + Vector3.up * 2f, (ForceMode)2);
			}
			lastLeft[num] = position3;
			num++;
		}
	}

	public static void GrapplingHook(bool active)
	{
		Color startColor;
		float startWidth;
		if (!active)
		{
			CleanupGrapple();
			return;
		}
		if (!grappleStart)
		{
			if ((UnityEngine.Object)(object)grappleLR == (UnityEngine.Object)null)
			{
				GameObject val = new GameObject("RGrappleLine");
				grappleLR = val.AddComponent<LineRenderer>();
				((Renderer)grappleLR).material = new Material(Shader.Find("Sprites/Default"));
				LineRenderer obj = grappleLR;
				startColor = (grappleLR.endColor = Color.white);
				obj.startColor = startColor;
				LineRenderer obj2 = grappleLR;
				startWidth = (grappleLR.endWidth = 0.02f);
				obj2.startWidth = startWidth;
				grappleLR.positionCount = 0;
				val.transform.SetParent(((Component)Variables.playerInstance).transform);
			}
			if ((UnityEngine.Object)(object)leftGrappleLR == (UnityEngine.Object)null)
			{
				GameObject val2 = new GameObject("LGrappleLine");
				leftGrappleLR = val2.AddComponent<LineRenderer>();
				((Renderer)leftGrappleLR).material = new Material(Shader.Find("Sprites/Default"));
				LineRenderer obj3 = leftGrappleLR;
				startColor = (leftGrappleLR.endColor = Color.white);
				obj3.startColor = startColor;
				LineRenderer obj4 = leftGrappleLR;
				startWidth = (leftGrappleLR.endWidth = 0.02f);
				obj4.startWidth = startWidth;
				leftGrappleLR.positionCount = 0;
				val2.transform.SetParent(((Component)Variables.playerInstance).transform);
			}
			grappleStart = true;
		}
		HandleGrapple(ref rightGrappleAimer, ref grappleLR, ref rightGrapplePoint, ref rightGrappleAttached, Variables.playerInstance.RightHand.controllerTransform, ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat, isRight: false);
		HandleGrapple(ref leftGrappleAimer, ref leftGrappleLR, ref leftGrapplePoint, ref leftGrappleAttached, Variables.playerInstance.LeftHand.controllerTransform, ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerIndexFloat, isRight: true);
	}

	public static void RideEntity(string name)
	{
		GameObject val = GameObject.Find(name);
		if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
		{
			Player.TeleportTo(val.transform.position);
			((Component)Variables.taggerInstance).GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
		}
	}

	public static void HoverboardScreenAll(Color color)
	{
		if (InputHandler.RTrigger())
		{
			MenuPatches.SerializationPatch.Override = () =>
			{
				if (!PhotonNetwork.InRoom)
				{
					return true;
				}
				PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { Variables.taggerInstance.myVRRig.GetView });
				Vector3 position = ((Component)VRRig.LocalRig).transform.position;
				NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
				int num = 0;
				while (num < playerListOthers.Length)
				{
					NetPlayer val = playerListOthers[num];
					VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
					if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
					{
						HoverboardScreenTarget(vRRigFromNetPlayer, color);
						PhotonView getView = Variables.taggerInstance.myVRRig.GetView;
						RaiseEventOptions val2 = new RaiseEventOptions();
						val2.TargetActors = new int[1] { val.ActorNumber };
						PhotonSerializer.SendSerialize(getView, val2);
						num++;
					}
					else
					{
						num++;
					}
				}
				Safety.RPCShield();
				((Behaviour)VRRig.LocalRig).enabled = true;
				((Component)VRRig.LocalRig).transform.position = position;
				return false;
			};
		}
		else
		{
			MenuPatches.SerializationPatch.Override = null;
		}
	}

	public static void SplashAnnoyGun()
	{
		if (GunLib.SetupLockOnGun())
		{
			if (!(Time.time < rpcCooldown))
			{
				rpcCooldown = Time.time + splashCooldown;
				((Behaviour)VRRig.LocalRig).enabled = false;
				((Component)VRRig.LocalRig).transform.position = ((Component)GunLib.lockedTargetRig).transform.position;
				Vector3 pos = GunLib.lockedTargetRig.headMesh.transform.position + GunLib.lockedTargetRig.headMesh.transform.forward * 0.2f;
				PlaySplashEffect(pos, GunLib.lockedTargetRig.headMesh.transform.rotation, 999f);
				((Behaviour)VRRig.LocalRig).enabled = true;
				Safety.RPCShield();
			}
		}
		else
		{
			((Behaviour)VRRig.LocalRig).enabled = true;
		}
	}

	public static void SpazEntity(string name)
	{
		GameObject val = GameObject.Find(name);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			val.transform.rotation = Variables.RandomQuaternion();
		}
	}

	public static void BecomeHoverboard()
	{
		Vector3 position = ((Component)Variables.taggerInstance.bodyCollider).transform.position;
		Quaternion rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation;
		((Behaviour)VRRig.LocalRig).enabled = false;
		((Component)VRRig.LocalRig).transform.position = position - Vector3.up;
		GTPlayer.Instance.SetHoverAllowed(true, false);
		GTPlayer.Instance.SetHoverActive(true);
		HoverboardVisual hoverboardVisual = VRRig.LocalRig.hoverboardVisual;
		Transform value = Traverse.Create((object)hoverboardVisual).Property("NominalParentTransform", (object[])null).GetValue<Transform>();
		hoverboardVisual.SetIsHeld(true, value.InverseTransformPoint(position), GTExt.InverseTransformRotation(value, rotation), VRRig.LocalRig.playerColor);
		Traverse.Create((object)hoverboardVisual).Field("interpolatedLocalPosition").SetValue((object)hoverboardVisual.NominalLocalPosition);
		Traverse.Create((object)hoverboardVisual).Field("interpolatedLocalRotation").SetValue((object)hoverboardVisual.NominalLocalRotation);
		GTPlayer.Instance.SetHoverboardPosRot(position, rotation);
	}

	public static void ObjectGun(string name)
	{
		if (!GunLib.GunGrips)
		{
			GunLib.SetGunVisibility(isVisible: false);
			return;
		}
		GunLib.SetupRaycast();
		if (GunLib.GunTriggers)
		{
			Variables.FindObject(name).transform.position = GunLib.raycastHit.point;
		}
	}

	public static void DisableWindBarriers(bool enable)
	{
		MenuPatches.forceEnabled = enable;
		Variables.FindObject("Environment Objects/LocalObjects_Prefab/Forest/Environment/Forest_ForceVolumes/").SetActive(!enable);
		Variables.FindObject("Environment Objects/LocalObjects_Prefab/ForestToHoverboard/TurnOnInForestAndHoverboard/ForestDome_CollisionOnly").SetActive(!enable);
	}

	public static void EntityOrbit(string name, float offset = 0f)
	{
		GameObject val = GameObject.Find(name);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			val.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position + new Vector3(MathF.Cos(offset + (float)Time.frameCount / 30f), 1f, MathF.Sin(offset + (float)Time.frameCount / 30f));
		}
	}

	public static void AirSwim(bool active)
	{
		if (active)
		{
			if ((UnityEngine.Object)(object)airSwimPart == (UnityEngine.Object)null)
			{
				GameObject val = Variables.FindObject("Environment Objects/LocalObjects_Prefab/ForestToBeach/ForestToBeach_Prefab_V4/ForestToBeach_Geo/CaveWaterVolume");
				if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
				{
					airSwimPart = UnityEngine.Object.Instantiate<GameObject>(val);
					airSwimPart.transform.localScale = new Vector3(5f, 5f, 5f);
					airSwimPart.GetComponent<Renderer>().enabled = false;
					Variables.playerInstance.audioManager.UnsetMixerSnapshot(0.1f);
					airSwimPart.transform.position = ((Component)GorillaTagger.Instance.headCollider).transform.position + new Vector3(0f, 2.5f, 0f);
				}
			}
			else
			{
				Variables.playerInstance.audioManager.UnsetMixerSnapshot(0.1f);
				airSwimPart.transform.position = ((Component)GorillaTagger.Instance.headCollider).transform.position + new Vector3(0f, 2.5f, 0f);
			}
		}
		else if ((UnityEngine.Object)(object)airSwimPart != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)airSwimPart);
			airSwimPart = null;
		}
	}

	public static void HoverboardGun()
	{
		if (!GunLib.GunGrips)
		{
			GunLib.SetGunVisibility(isVisible: false);
			return;
		}
		GunLib.SetupRaycast();
		if (GunLib.GunTriggers && Time.time > hoverboardDelay)
		{
			hoverboardDelay = Time.time + 0.25f;
			DropHoverboard(GunLib.raycastHit.point + Vector3.up, Variables.RandomQuaternion(), Vector3.zero, Vector3.zero, Variables.RandomColor());
		}
	}

	public static void RespawnGliders()
	{
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		int num = 0;
		while (num < allType.Length)
		{
			GliderHoldable val = allType[num];
			if (((NetworkView)val).GetView.Owner == PhotonNetwork.LocalPlayer)
			{
				val.Respawn();
				num++;
			}
			else
			{
				((NetworkHoldableObject)val).OnHover((InteractionPoint)null, (GameObject)null);
				num++;
			}
		}
	}

	public static void StrobeHoverboard()
	{
		if (!((UnityEngine.Object)(object)VRRig.LocalRig.hoverboardVisual == (UnityEngine.Object)null) && VRRig.LocalRig.hoverboardVisual.IsHeld)
		{
			if (Time.time > strobeDelay)
			{
				strobeDelay = Time.time + 0.1f;
				strobeState = !strobeState;
				VRRig.LocalRig.hoverboardVisual.SetIsHeld(VRRig.LocalRig.hoverboardVisual.IsLeftHanded, VRRig.LocalRig.hoverboardVisual.NominalLocalPosition, VRRig.LocalRig.hoverboardVisual.NominalLocalRotation, strobeState ? Color.white : Color.black);
			}
			else
			{
				VRRig.LocalRig.hoverboardVisual.SetIsHeld(VRRig.LocalRig.hoverboardVisual.IsLeftHanded, VRRig.LocalRig.hoverboardVisual.NominalLocalPosition, VRRig.LocalRig.hoverboardVisual.NominalLocalRotation, strobeState ? Color.white : Color.black);
			}
		}
	}

	public static void GliderAura()
	{
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		int num = 0;
		while (num < allType.Length)
		{
			GliderHoldable val = allType[num];
			if (((NetworkView)val).GetView.Owner == PhotonNetwork.LocalPlayer)
			{
				((Component)val).gameObject.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position + Variables.RandomVector3();
				((Component)val).gameObject.transform.rotation = Variables.RandomQuaternion();
				num++;
			}
			else
			{
				((NetworkHoldableObject)val).OnHover((InteractionPoint)null, (GameObject)null);
				num++;
			}
		}
	}

	public static void ShootHoverboards()
	{
		if (InputHandler.RGrip() && !(Time.time < hoverboardDelay))
		{
			hoverboardDelay = Time.time + 0.5f;
			Transform rightHandTransform = Variables.taggerInstance.rightHandTransform;
			DropHoverboard(rightHandTransform.position, rightHandTransform.rotation, rightHandTransform.forward * 10f, Vector3.zero, Variables.RandomColor());
		}
	}

	public static void HoverboardScreenGun(Color color)
	{
		if (GunLib.SetupLockOnGun())
		{
			MenuPatches.SerializationPatch.Override = () =>
			{
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
				{
					return false;
				}
				HoverboardScreenTarget(GunLib.lockedTargetRig, color);
				PhotonSerializer.SendSerialize(Variables.taggerInstance.myVRRig.GetView);
				return false;
			};
		}
		else
		{
			MenuPatches.SerializationPatch.Override = null;
			((Behaviour)VRRig.LocalRig).enabled = true;
		}
	}

	public static void ModifyWater(bool solid, bool transparent)
	{
		if (cachedWater == null)
		{
			cachedWater = UnityEngine.Object.FindObjectsOfType<WaterVolume>();
		}
		string text;
		if (solid)
		{
			text = "Default";
		}
		else if (transparent)
		{
			text = "TransparentFX";
		}
		else
		{
			text = "Water";
		}
		int num = LayerMask.NameToLayer(text);
		if (num == -1)
		{
			return;
		}
		WaterVolume[] array = cachedWater;
		int num2 = 0;
		while (num2 < array.Length)
		{
			WaterVolume val = array[num2];
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				((Component)val).gameObject.layer = num;
			}
			num2++;
		}
	}

	public static void SpazGliders()
	{
		if (!InputHandler.RTrigger())
		{
			return;
		}
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		int num = 0;
		while (num < allType.Length)
		{
			GliderHoldable val = allType[num];
			if (((NetworkView)val).GetView.Owner == PhotonNetwork.LocalPlayer)
			{
				((Component)val).gameObject.transform.rotation = Variables.RandomQuaternion();
				num++;
			}
			else
			{
				((NetworkHoldableObject)val).OnHover((InteractionPoint)null, (GameObject)null);
				num++;
			}
		}
	}

	public static IEnumerator ReturnRig()
	{
		yield return (object)new WaitForSeconds(0.2f);
		((Behaviour)VRRig.LocalRig).enabled = true;
		BugCoroutine = null;
	}

	public static void SplashAura()
	{
		if (!InputHandler.RTrigger() || Time.time < rpcCooldown)
		{
			return;
		}
		rpcCooldown = Time.time + splashCooldown;
		Safety.RPCShield();
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current != (UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig && (UnityEngine.Object)(object)current != (UnityEngine.Object)(object)Variables.taggerInstance.myVRRig)
			{
				PlaySplashEffect(((Component)current).transform.position, ((Component)current).transform.rotation, 125f);
			}
		}
	}

	public static void SetName(string name)
	{
		((GorillaComputer)GorillaComputer.instance).currentName = name;
		PhotonNetwork.LocalPlayer.NickName = name;
		((GorillaComputer)GorillaComputer.instance).SetLocalNameTagText(name);
		((GorillaComputer)GorillaComputer.instance).savedName = name;
		PlayerPrefs.SetString("playerName", name);
		PlayerPrefs.Save();
	}

	public static void TryCosmeticAnywhere(bool enable)
	{
		if (enable == cosmeticsEnabled)
		{
			return;
		}
		cosmeticsEnabled = enable;
		int[] array;
		if (enable)
		{
			archiveCosmetics = ((CosmeticsController)CosmeticsController.instance).currentWornSet.ToPackedIDArray();
			array = cachedLmajuCosmetics ?? (cachedLmajuCosmetics = PackedCosmetics(Enumerable.Repeat("LMAJU.", 16).ToArray()));
		}
		else
		{
			array = archiveCosmetics;
		}
		CosmeticSet val = new CosmeticSet(array, CosmeticsController.instance);
		((CosmeticsController)CosmeticsController.instance).currentWornSet = val;
		Variables.taggerInstance.offlineVRRig.cosmeticSet = val;
		Variables.taggerInstance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", (RpcTarget)0, new object[3]
		{
			array,
			((CosmeticsController)CosmeticsController.instance).tryOnSet.ToPackedIDArray(),
			false
		});
		Safety.RPCShield();
	}

	public static void PlaceBomb(bool active)
	{
		if (active)
		{
			if (InputHandler.RGrip())
			{
				if ((UnityEngine.Object)(object)bomb == (UnityEngine.Object)null)
				{
					bomb = GameObject.CreatePrimitive((PrimitiveType)3);
					bomb.transform.localScale = Vector3.one * 0.2f;
					bomb.GetComponent<Renderer>().material.color = Color.red;
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)bomb.GetComponent<BoxCollider>());
					bomb.transform.position = Variables.playerInstance.RightHand.controllerTransform.position;
				}
				else
				{
					bomb.transform.position = Variables.playerInstance.RightHand.controllerTransform.position;
				}
			}
			else if (InputHandler.RTrigger() && (UnityEngine.Object)(object)bomb != (UnityEngine.Object)null)
			{
				((Collider)Variables.playerInstance.bodyCollider).attachedRigidbody.AddExplosionForce(50000f, bomb.transform.position, 5f);
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)bomb);
				bomb = null;
			}
		}
		else if ((UnityEngine.Object)(object)bomb != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)bomb);
			bomb = null;
		}
	}

	private static void HandleGrapple(ref GameObject aim, ref LineRenderer line, ref Vector3 grapPt, ref bool attached, Transform hand, float trigger, bool isRight)
	{
		RaycastHit val = default(RaycastHit);
		bool hasHit = Physics.Raycast(hand.position, hand.forward, out val, 100f);
		if (hasHit)
		{
			if ((UnityEngine.Object)(object)aim == (UnityEngine.Object)null)
			{
				aim = GameObject.CreatePrimitive((PrimitiveType)0);
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)aim.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)aim.GetComponent<SphereCollider>());
				aim.transform.localScale = Vector3.one * 0.2f;
				aim.GetComponent<Renderer>().material.color = Color.green;
				aim.SetActive(true);
				aim.transform.position = val.point;
			}
			else
			{
				aim.SetActive(true);
				aim.transform.position = val.point;
			}
		}
		if (trigger <= 0.1f)
		{
			attached = false;
			line.positionCount = 0;
			return;
		}
		if (!attached && hasHit)
		{
			grapPt = val.point;
			attached = true;
			line.positionCount = 2;
			line.SetPosition(1, grapPt);
			Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(82, isRight, 1f);
			if (!attached)
			{
				return;
			}
		}
		else if (!attached)
		{
			return;
		}
		line.positionCount = 2;
		line.SetPosition(0, hand.position);
		line.SetPosition(1, grapPt);
		if ((UnityEngine.Object)(object)aim != (UnityEngine.Object)null)
		{
			aim.transform.position = grapPt;
		}
		Vector3 val2 = grapPt - ((Component)Variables.playerInstance.bodyCollider).transform.position;
		Vector3 normalized = val2.normalized;
		float num = Vector3.Distance(((Component)Variables.playerInstance.bodyCollider).transform.position, grapPt);
		if (num > 1f)
		{
			((Component)Variables.playerInstance).GetComponent<Rigidbody>().velocity = Vector3.Lerp(((Component)Variables.playerInstance).GetComponent<Rigidbody>().velocity, normalized * 15f, Time.deltaTime * 10f);
		}
	}

	public static void UnlockAllCosmetics()
	{
		MenuPatches.cosmeticsEnabled = true;
		if (MenuPatches.cosmeticsInitialized && !unlockedCosmetics)
		{
			unlockedCosmetics = true;
			IEnumerator<CosmeticItem> enumerator = ((CosmeticsController)CosmeticsController.instance).allCosmetics.Where((CosmeticItem i) => !((CosmeticsController)CosmeticsController.instance).concatStringCosmeticsAllowed.Contains(i.itemName)).GetEnumerator();
			while (enumerator.MoveNext())
			{
				CosmeticItem current = enumerator.Current;
				((CosmeticsController)CosmeticsController.instance).UnlockItem(current.itemName, false);
			}
		}
	}

	public static void HoverboardScreenTarget(VRRig rig, Color color)
	{
		if (hoverboardCoroutine != null)
		{
			((MonoBehaviour)CoroutineHelper.Instance).StopCoroutine(hoverboardCoroutine);
			hoverboardCoroutine = ((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(ResetHoverboard());
			Vector3 angularVelocity = GTExt.GetOrAddComponent<GorillaVelocityEstimator>(rig.headMesh).angularVelocity;
			Vector3 val = rig.headMesh.transform.TransformPoint(-0.3f, 0.1f, 0.3725f) + rig.LatestVelocity() * 0.5f;
			Quaternion val2 = rig.headMesh.transform.rotation * Quaternion.Euler(angularVelocity * (18f / MathF.PI)) * Quaternion.Euler(0f, 90f, 270f);
			((Behaviour)VRRig.LocalRig).enabled = false;
			((Component)VRRig.LocalRig).transform.position = val - Vector3.up * 0.5f;
			GTPlayer.Instance.SetHoverAllowed(true, false);
			HoverboardVisual hoverboardVisual = VRRig.LocalRig.hoverboardVisual;
			Transform value = Traverse.Create((object)hoverboardVisual).Property("NominalParentTransform", (object[])null).GetValue<Transform>();
			hoverboardVisual.SetIsHeld(true, value.InverseTransformPoint(val), GTExt.InverseTransformRotation(value, val2), color);
			GTPlayer.Instance.SetHoverActive(false);
			Traverse.Create((object)hoverboardVisual).Field("interpolatedLocalPosition").SetValue((object)hoverboardVisual.NominalLocalPosition);
			Traverse.Create((object)hoverboardVisual).Field("interpolatedLocalRotation").SetValue((object)hoverboardVisual.NominalLocalRotation);
			GTPlayer.Instance.SetHoverboardPosRot(val, val2);
		}
		else
		{
			hoverboardCoroutine = ((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(ResetHoverboard());
			Vector3 angularVelocity = GTExt.GetOrAddComponent<GorillaVelocityEstimator>(rig.headMesh).angularVelocity;
			Vector3 val = rig.headMesh.transform.TransformPoint(-0.3f, 0.1f, 0.3725f) + rig.LatestVelocity() * 0.5f;
			Quaternion val2 = rig.headMesh.transform.rotation * Quaternion.Euler(angularVelocity * (18f / MathF.PI)) * Quaternion.Euler(0f, 90f, 270f);
			((Behaviour)VRRig.LocalRig).enabled = false;
			((Component)VRRig.LocalRig).transform.position = val - Vector3.up * 0.5f;
			GTPlayer.Instance.SetHoverAllowed(true, false);
			HoverboardVisual hoverboardVisual = VRRig.LocalRig.hoverboardVisual;
			Transform value = Traverse.Create((object)hoverboardVisual).Property("NominalParentTransform", (object[])null).GetValue<Transform>();
			hoverboardVisual.SetIsHeld(true, value.InverseTransformPoint(val), GTExt.InverseTransformRotation(value, val2), color);
			GTPlayer.Instance.SetHoverActive(false);
			Traverse.Create((object)hoverboardVisual).Field("interpolatedLocalPosition").SetValue((object)hoverboardVisual.NominalLocalPosition);
			Traverse.Create((object)hoverboardVisual).Field("interpolatedLocalRotation").SetValue((object)hoverboardVisual.NominalLocalRotation);
			GTPlayer.Instance.SetHoverboardPosRot(val, val2);
		}
	}

	public static void SplashGun()
	{
		if (GunLib.SetupGun())
		{
			if (!(Time.time < rpcCooldown))
			{
				rpcCooldown = Time.time + splashCooldown;
				((Behaviour)VRRig.LocalRig).enabled = false;
				((Component)VRRig.LocalRig).transform.position = GunLib.raycastHit.point - Vector3.up * 2f;
				PlaySplashEffect(GunLib.raycastHit.point, (GunLib.raycastHit.normal != Vector3.zero) ? Quaternion.LookRotation(GunLib.raycastHit.normal) : Quaternion.identity, 125f);
				((Behaviour)VRRig.LocalRig).enabled = true;
				Safety.RPCShield();
			}
		}
		else
		{
			((Behaviour)VRRig.LocalRig).enabled = true;
		}
	}

	public static void FreezeEntity(string name)
	{
		GameObject val = GameObject.Find(name);
		if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
		{
			ThrowableBug component = val.GetComponent<ThrowableBug>();
			if (!((UnityEngine.Object)(object)component == (UnityEngine.Object)null))
			{
				component.bugRotationalVelocity = Quaternion.identity;
				component.targetVelocity = Vector3.zero;
				component.thrownVeloicity = Vector3.zero;
				component.thrownYVelocity = 0f;
				component.reliableState.travelingDirection = Vector3.zero;
			}
		}
	}

	public static void SetEntitySpeed(string name, float speed)
	{
		GameObject val = GameObject.Find(name);
		if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
		{
			ThrowableBug component = val.GetComponent<ThrowableBug>();
			if (!((UnityEngine.Object)(object)component == (UnityEngine.Object)null))
			{
				component.maxNaturalSpeed = speed;
			}
		}
	}

	public static void DestroyEntity(string name)
	{
		GameObject val = GameObject.Find(name);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			val.transform.position = new Vector3(99999f, 99999f, 99999f);
		}
	}

	public static void DrawMod(bool active)
	{
		if (active)
		{
			if ((UnityEngine.Object)(object)rightPointer == (UnityEngine.Object)null)
			{
				rightPointer = GameObject.CreatePrimitive((PrimitiveType)0);
				rightPointer.transform.localScale = Vector3.one * 0.1f;
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)rightPointer.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)rightPointer.GetComponent<SphereCollider>());
			}
			if ((UnityEngine.Object)(object)leftPointer == (UnityEngine.Object)null)
			{
				leftPointer = GameObject.CreatePrimitive((PrimitiveType)0);
				leftPointer.transform.localScale = Vector3.one * 0.1f;
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)leftPointer.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)leftPointer.GetComponent<SphereCollider>());
			}
			rightPointer.transform.position = Variables.playerInstance.RightHand.controllerTransform.position;
			leftPointer.transform.position = Variables.playerInstance.LeftHand.controllerTransform.position;
			rightPointer.GetComponent<Renderer>().material.color = drawColor;
			leftPointer.GetComponent<Renderer>().material.color = drawColor;
			if (InputHandler.RGrip())
			{
				GameObject val = GameObject.CreatePrimitive((PrimitiveType)0);
				val.transform.localScale = Vector3.one * 0.1f;
				val.transform.position = Variables.playerInstance.RightHand.controllerTransform.position;
				val.GetComponent<Renderer>().material.color = drawColor;
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)val.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)val.GetComponent<SphereCollider>());
				drawings.Add(val);
			}
			if (InputHandler.LGrip())
			{
				GameObject val2 = GameObject.CreatePrimitive((PrimitiveType)0);
				val2.transform.localScale = Vector3.one * 0.1f;
				val2.transform.position = Variables.playerInstance.LeftHand.controllerTransform.position;
				val2.GetComponent<Renderer>().material.color = drawColor;
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)val2.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)val2.GetComponent<SphereCollider>());
				drawings.Add(val2);
			}
			if (InputHandler.RPrimary())
			{
				if (!colorChangerCooldown)
				{
					currentColor = (currentColor + 1) % 13;
					drawColor = (currentColor == 1) ? Color.blue : Color.white;
					colorChangerCooldown = true;
				}
				return;
			}
			colorChangerCooldown = false;
			return;
		}
		if ((UnityEngine.Object)(object)rightPointer != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)rightPointer);
			rightPointer = null;
		}
		if ((UnityEngine.Object)(object)leftPointer != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)leftPointer);
			leftPointer = null;
		}
		DestroyAllDrawings();
	}

	public static void HoverboardAura()
	{
		if (Time.time < hoverboardAuraDelay)
		{
			return;
		}
		hoverboardAuraDelay = Time.time + 0.25f;
		int num = 0;
		if (num < 2)
		{
			do
			{
				DropHoverboard(((Component)Variables.taggerInstance.headCollider).transform.position + Variables.RandomVector3(), Variables.RandomQuaternion(), Variables.RandomVector3(20f), Variables.RandomVector3(20f), Variables.RandomColor());
				num++;
			}
			while (num < 2);
		}
	}

	public static void PlaySplashEffect(Vector3 pos, Quaternion rot, float size)
	{
		Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlaySplashEffect", (RpcTarget)0, new object[6] { pos, rot, size, size, true, true });
	}

	public static void DestroyGliders()
	{
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		int num = 0;
		while (num < allType.Length)
		{
			GliderHoldable val = allType[num];
			if (((NetworkView)val).GetView.Owner == PhotonNetwork.LocalPlayer)
			{
				((Component)val).gameObject.transform.position = new Vector3(99999f, 99999f, 99999f);
				num++;
			}
			else
			{
				((NetworkHoldableObject)val).OnHover((InteractionPoint)null, (GameObject)null);
				num++;
			}
		}
	}

	private static IEnumerator EnableRig()
	{
		yield return (object)new WaitForSeconds(0.3f);
		((Behaviour)VRRig.LocalRig).enabled = true;
	}

	public static void SpazHoverboard()
	{
		if ((UnityEngine.Object)(object)VRRig.LocalRig.hoverboardVisual != (UnityEngine.Object)null && VRRig.LocalRig.hoverboardVisual.IsHeld)
		{
			VRRig.LocalRig.hoverboardVisual.SetIsHeld(VRRig.LocalRig.hoverboardVisual.IsLeftHanded, VRRig.LocalRig.hoverboardVisual.NominalLocalPosition, Variables.RandomQuaternion(), Variables.RandomColor());
		}
	}

	public static void GrabGliders()
	{
		if (!InputHandler.RGrip())
		{
			return;
		}
		GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>(false);
		int num = 0;
		while (num < allType.Length)
		{
			GliderHoldable val = allType[num];
			if (((NetworkView)val).GetView.Owner == PhotonNetwork.LocalPlayer)
			{
				((Component)val).gameObject.transform.position = Variables.taggerInstance.rightHandTransform.position;
				((Component)val).gameObject.transform.rotation = Variables.taggerInstance.rightHandTransform.rotation;
				num++;
			}
			else
			{
				((NetworkHoldableObject)val).OnHover((InteractionPoint)null, (GameObject)null);
				num++;
			}
		}
	}

	public static void SpawnHoverboard()
	{
		DropHoverboard(((Component)VRRig.LocalRig).transform.position, ((Component)VRRig.LocalRig).transform.rotation, Vector3.zero, Vector3.zero, Variables.RandomColor());
		GTPlayer.Instance.SetHoverAllowed(true, false);
	}

	public static void SplashAnnoyAll()
	{
		if (InputHandler.RTrigger())
		{
			MenuPatches.SerializationPatch.Override = delegate
			{
				if (!PhotonNetwork.InRoom)
				{
					return true;
				}
				if (Time.time < rpcCooldown)
				{
					return false;
				}
				rpcCooldown = Time.time + splashCooldown;
				PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { Variables.taggerInstance.myVRRig.GetView });
				Vector3 position = ((Component)VRRig.LocalRig).transform.position;
				NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
				int num = 0;
				while (num < playerListOthers.Length)
				{
					NetPlayer val = playerListOthers[num];
					VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
					if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
					{
						((Component)VRRig.LocalRig).transform.position = ((Component)vRRigFromNetPlayer).transform.position;
						PhotonView getView = Variables.taggerInstance.myVRRig.GetView;
						RaiseEventOptions val2 = new RaiseEventOptions();
						val2.TargetActors = new int[1] { val.ActorNumber };
						PhotonSerializer.SendSerialize(getView, val2);
						Variables.taggerInstance.myVRRig.SendRPC("RPC_PlaySplashEffect", val, new object[6]
						{
							vRRigFromNetPlayer.headMesh.transform.position + vRRigFromNetPlayer.headMesh.transform.forward * 0.2f,
							vRRigFromNetPlayer.headMesh.transform.rotation,
							999f,
							999f,
							true,
							true
						});
						num++;
					}
					else
					{
						num++;
					}
				}
				((Component)VRRig.LocalRig).transform.position = position;
				Safety.RPCShield();
				return false;
			};
		}
		else
		{
			MenuPatches.SerializationPatch.Override = null;
		}
	}

	public static void RainbowHoverboard()
	{
		if (!((UnityEngine.Object)(object)VRRig.LocalRig.hoverboardVisual == (UnityEngine.Object)null) && VRRig.LocalRig.hoverboardVisual.IsHeld)
		{
			VRRig.LocalRig.hoverboardVisual.SetIsHeld(VRRig.LocalRig.hoverboardVisual.IsLeftHanded, VRRig.LocalRig.hoverboardVisual.NominalLocalPosition, VRRig.LocalRig.hoverboardVisual.NominalLocalRotation, Color.HSVToRGB((float)Time.frameCount / 180f % 1f, 1f, 1f));
		}
	}

	public static void ElevatorBuzz(bool enable)
	{
		GameObject[] array;
		int num;
		if (elevatorAmbientObjects == null)
		{
			elevatorAmbientObjects = (GameObject[])(object)new GameObject[3]
			{
				GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/GhostReactorElevator/HasVisualComponents/Audio/Ambient"),
				GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/CityElevator/HasVisualComponents/Audio/Ambient"),
				GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/StumpElevator/HasVisualComponents/Audio/Ambient")
			};
			array = elevatorAmbientObjects;
			num = 0;
		}
		else
		{
			array = elevatorAmbientObjects;
			num = 0;
		}
		while (num < array.Length)
		{
			GameObject val = array[num];
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				val.SetActive(enable);
				num++;
			}
			else
			{
				num++;
			}
		}
	}

	public static void RemoveBracelet()
	{
		SetBraceletState(enable: false, left: true);
		SetBraceletState(enable: false, left: false);
		Safety.RPCShield();
	}

	public static void Webshooters(bool active)
	{
		Color startColor;
		float startWidth;
		if (!active)
		{
			CleanupWebshooters();
			return;
		}
		if (!wackstart)
		{
			if ((UnityEngine.Object)(object)lr == (UnityEngine.Object)null)
			{
				GameObject val = new GameObject("RightWebLine");
				lr = val.AddComponent<LineRenderer>();
				((Renderer)lr).material = new Material(Shader.Find("Sprites/Default"));
				LineRenderer obj = lr;
				startColor = (lr.endColor = Color.white);
				obj.startColor = startColor;
				LineRenderer obj2 = lr;
				startWidth = (lr.endWidth = 0.02f);
				obj2.startWidth = startWidth;
				lr.positionCount = 0;
				val.transform.SetParent(((Component)Variables.playerInstance).transform);
			}
			if ((UnityEngine.Object)(object)leftlr == (UnityEngine.Object)null)
			{
				GameObject val2 = new GameObject("LeftWebLine");
				leftlr = val2.AddComponent<LineRenderer>();
				((Renderer)leftlr).material = new Material(Shader.Find("Sprites/Default"));
				LineRenderer obj3 = leftlr;
				startColor = (leftlr.endColor = Color.white);
				obj3.startColor = startColor;
				LineRenderer obj4 = leftlr;
				startWidth = (leftlr.endWidth = 0.02f);
				obj4.startWidth = startWidth;
				leftlr.positionCount = 0;
				val2.transform.SetParent(((Component)Variables.playerInstance).transform);
			}
			wackstart = true;
		}
		HandleWebshooter(ref joint, ref lr, ref rightaimer, ref cangrapple, Variables.playerInstance.RightHand.controllerTransform, ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat, (Controller)2, isRight: false);
		HandleWebshooter(ref leftjoint, ref leftlr, ref leftaimer, ref canleftgrapple, Variables.playerInstance.LeftHand.controllerTransform, ((ControllerInputPoller)ControllerInputPoller.instance).leftControllerIndexFloat, (Controller)1, isRight: true);
	}
}
