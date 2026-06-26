using System;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace NXO.Mods.Categories;

public class Player
{
	private static float orbitSpeed = 100f;

	private static float orbitDistance = 2f;

	private static bool hasTeleported = false;

	private static Vector3? cachedHeadRotation = null;

	public static float sizeScale = 1f;

	public static float lastSentScale = -1f;

	private static bool wasRigDisabled = false;

	private static float _headSpinOffset = 0f;

	public static (string name, string zone, string pos)[] _maps = new(string, string, string)[16]
	{
		("Forest", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/TreeRoomSpawnForestZone", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Forest, Tree Exit"),
		("City", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToCity", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City Front"),
		("Canyons", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestCanyonTransition", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Canyon"),
		("Clouds", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToSkyJungle", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Clouds From Computer"),
		("Caves", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToCave", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Cave"),
		("Beach", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/BeachToForest", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Beach for Computer"),
		("Mountains", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToMountain", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Mountain"),
		("Basement", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToBasement", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Basement For Computer"),
		("Metropolis", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/MetropolisOnly", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Metropolis from Computer"),
		("Arcade", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToArcade", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City frm Arcade"),
		("Critters", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityCrittersTransition", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City from Critters"),
		("Skate Park", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToHoverboard", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Hoverboard from Forest"),
		("Monke Blocks", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/MonkeBlocksElevatorExit", "Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/MonkeBlocksElevator/Triggers/JoinRoomTrigger"),
		("Rotating", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToRotating", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Rotating Map"),
		("Bayou", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/BayouOnly", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - BayouComputer2"),
		("Lava Forest", "Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/VIMForestLavaElevator/Triggers/VIMExp1_SetZoneTrigger", "Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/VIMForestLavaElevator/Triggers/JoinRoomTrigger")
	};

	private static Vector3 _layOnBackPos;

	private static Vector3 _layOnStomachPos;

	private static float _backflipAngle;

	private static float _frontflipAngle;

	private static float _cartWheelAngle;

	private static float _spider;

	private static float _spiderIdle;

	private static Vector3 _spiderLastPos;

	private static bool _spiderInit;

	private static float _glitchOffset;

	private static float _glitchTimer;

	private static Vector3 _glitchPos;

	private static float _wobbleOffset;

	private static GameObject _ragdollObj;

	private static Rigidbody _ragdollBody;

	private static BoxCollider _ragdollCollider;

	private static float HeadY
	{
		get
		{
			Quaternion rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation;
			return rotation.eulerAngles.y;
		}
	}

	public static void ChaseAll()
	{
		MenuPatches.SerializationPatch.Override = delegate
		{
			PhotonView component = ((Component)Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				return false;
			}
			PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { component });
			Vector3 position = ((Component)VRRig.LocalRig).transform.position;
			Quaternion rotation = ((Component)VRRig.LocalRig).transform.rotation;
			Vector3 position2 = VRRig.LocalRig.leftHand.rigTarget.position;
			Quaternion rotation2 = VRRig.LocalRig.leftHand.rigTarget.rotation;
			Vector3 position3 = VRRig.LocalRig.rightHand.rigTarget.position;
			Quaternion rotation3 = VRRig.LocalRig.rightHand.rigTarget.rotation;
			Quaternion rotation4 = ((Component)VRRig.LocalRig.head.rigTarget).transform.rotation;
			NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
			int num = 0;
			while (num < playerListOthers.Length)
			{
				NetPlayer val = playerListOthers[num];
				VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
				if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
				{
					((Component)VRRig.LocalRig).transform.position = Vector3.MoveTowards(((Component)VRRig.LocalRig).transform.position, ((Component)vRRigFromNetPlayer).transform.position, Time.deltaTime * 10f);
					((Component)VRRig.LocalRig).transform.LookAt(((Component)vRRigFromNetPlayer).transform);
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.position = ((Component)VRRig.LocalRig).transform.position + ((Component)VRRig.LocalRig).transform.right * -1.5f;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.position = ((Component)VRRig.LocalRig).transform.position + ((Component)VRRig.LocalRig).transform.right * 1.5f;
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
					((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = Quaternion.Euler((float)UnityEngine.Random.Range(0, 360), (float)UnityEngine.Random.Range(0, 360), (float)UnityEngine.Random.Range(0, 360));
					RaiseEventOptions val2 = new RaiseEventOptions();
					val2.TargetActors = new int[1] { val.ActorNumber };
					PhotonSerializer.SendSerialize(component, val2);
					num++;
				}
				else
				{
					num++;
				}
			}
			Safety.RPCShield();
			((Component)VRRig.LocalRig).transform.position = position;
			((Component)VRRig.LocalRig).transform.rotation = rotation;
			VRRig.LocalRig.leftHand.rigTarget.position = position2;
			VRRig.LocalRig.leftHand.rigTarget.rotation = rotation2;
			VRRig.LocalRig.rightHand.rigTarget.position = position3;
			VRRig.LocalRig.rightHand.rigTarget.rotation = rotation3;
			((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = rotation4;
			return false;
		};
	}

	public static void DisableFrontflip()
	{
		_frontflipAngle = 0f;
		SetRigStatus(rigStatus: true);
	}

	private static void SetCrawlHand(Transform hand, float phase, float side)
	{
		Transform transform = ((Component)Variables.taggerInstance.offlineVRRig).transform;
		if (phase < 0.3f)
		{
			float num = phase / 0.3f;
			float num2 = Mathf.Lerp(-0.5f, 0.7f, num);
			float num3 = Mathf.Sin(num * MathF.PI) * 0.25f;
			((Component)hand).transform.position = transform.position + transform.forward * num2 + transform.right * side + transform.up * (-0.4f + num3);
		}
		else
		{
			float num4 = (phase - 0.3f) / 0.7f;
			float num2 = Mathf.Lerp(0.7f, -0.5f, num4);
			float num3 = 0f;
			((Component)hand).transform.position = transform.position + transform.forward * num2 + transform.right * side + transform.up * (-0.4f + num3);
		}
	}

	public static void GriddyMonke()
	{
		if (!InputHandler.RTrigger())
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation;
		Vector3 val = ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -0.25f + ((Component)Variables.taggerInstance.offlineVRRig).transform.forward * 0.5f * Mathf.Cos((float)Time.frameCount / 10f) + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * -0.3f * Mathf.Abs(Mathf.Sin((float)Time.frameCount / 7f));
		Vector3 val2 = ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 0.25f + ((Component)Variables.taggerInstance.offlineVRRig).transform.forward * 0.5f * Mathf.Cos((float)Time.frameCount / 10f) + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * -0.3f * Mathf.Abs(Mathf.Sin((float)Time.frameCount / 7f));
		SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + val, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + val2);
	}

	public static void ChasePlayerGun()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			Collider collider = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if (GunLib.GunTriggers)
			{
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
				{
					GunLib.lockedTargetRig = GunLib.potentialTargetRig;
					return;
				}
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = Vector3.MoveTowards(((Component)Variables.taggerInstance.offlineVRRig).transform.position, ((Component)GunLib.lockedTargetRig).transform.position, Time.deltaTime * 10f);
				((Component)Variables.taggerInstance.offlineVRRig).transform.LookAt(((Component)GunLib.lockedTargetRig).transform);
				((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1.5f;
				((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1.5f;
				((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
				((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
				Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = new Vector3((float)UnityEngine.Random.Range(0, 360), (float)UnityEngine.Random.Range(0, 180), (float)UnityEngine.Random.Range(0, 180));
				Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = new Vector3((float)UnityEngine.Random.Range(0, 360), (float)UnityEngine.Random.Range(0, 360), (float)UnityEngine.Random.Range(0, 360));
				((Component)Variables.taggerInstance).GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			SetRigStatus(rigStatus: true);
			GunLib.CancelGunUse();
		}
	}

	public static void LongArms(bool setActive)
	{
		((Component)Variables.playerInstance).transform.localScale = (setActive ? Settings.ArmLength : Vector3.one);
	}

	public static void DisableSpiderMonke()
	{
		_spider = 0f;
		_spiderIdle = 0f;
		_spiderInit = false;
		SetRigStatus(rigStatus: true);
	}

	public static void PiggybackAll()
	{
		MenuPatches.SerializationPatch.Override = delegate
		{
			PhotonView component = ((Component)Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				return false;
			}
			PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { component });
			Vector3 position = ((Component)VRRig.LocalRig).transform.position;
			Quaternion rotation = ((Component)VRRig.LocalRig).transform.rotation;
			Vector3 position2 = VRRig.LocalRig.leftHand.rigTarget.position;
			Quaternion rotation2 = VRRig.LocalRig.leftHand.rigTarget.rotation;
			Vector3 position3 = VRRig.LocalRig.rightHand.rigTarget.position;
			Quaternion rotation3 = VRRig.LocalRig.rightHand.rigTarget.rotation;
			Quaternion rotation4 = ((Component)VRRig.LocalRig.head.rigTarget).transform.rotation;
			NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
			int num = 0;
			while (num < playerListOthers.Length)
			{
				NetPlayer val = playerListOthers[num];
				VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
				if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
				{
					((Component)VRRig.LocalRig).transform.position = ((Component)vRRigFromNetPlayer).transform.position + ((Component)vRRigFromNetPlayer).transform.up * 0.3f - ((Component)vRRigFromNetPlayer).transform.forward * 0.4f;
					((Component)VRRig.LocalRig).transform.rotation = ((Component)vRRigFromNetPlayer).transform.rotation;
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.position = ((Component)VRRig.LocalRig).transform.position + ((Component)vRRigFromNetPlayer).transform.right * -0.3f + ((Component)vRRigFromNetPlayer).transform.up * 0.1f;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.position = ((Component)VRRig.LocalRig).transform.position + ((Component)vRRigFromNetPlayer).transform.right * 0.3f + ((Component)vRRigFromNetPlayer).transform.up * 0.1f;
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.rotation = ((Component)vRRigFromNetPlayer).transform.rotation;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.rotation = ((Component)vRRigFromNetPlayer).transform.rotation;
					((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = ((Component)vRRigFromNetPlayer).transform.rotation;
					RaiseEventOptions val2 = new RaiseEventOptions();
					val2.TargetActors = new int[1] { val.ActorNumber };
					PhotonSerializer.SendSerialize(component, val2);
					num++;
				}
				else
				{
					num++;
				}
			}
			Safety.RPCShield();
			((Component)VRRig.LocalRig).transform.position = position;
			((Component)VRRig.LocalRig).transform.rotation = rotation;
			VRRig.LocalRig.leftHand.rigTarget.position = position2;
			VRRig.LocalRig.leftHand.rigTarget.rotation = rotation2;
			VRRig.LocalRig.rightHand.rigTarget.position = position3;
			VRRig.LocalRig.rightHand.rigTarget.rotation = rotation3;
			((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = rotation4;
			return false;
		};
	}

	public static void RigGun()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			if (GunLib.GunTriggers)
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = GunLib.raycastHit.point + new Vector3(0f, 1f, 0f);
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			}
		}
		else
		{
			SetRigStatus(rigStatus: true);
			GunLib.CancelGunUse();
		}
	}

	public static void SetRigStatus(bool rigStatus)
	{
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = rigStatus;
		if (rigStatus)
		{
			ResetHeadRotation();
		}
	}

	public static void DisableGlitchMonke()
	{
		_glitchOffset = 0f;
		_glitchTimer = 0f;
		_glitchPos = Vector3.zero;
		SetRigStatus(rigStatus: true);
	}

	public static void BackshotAll()
	{
		MenuPatches.SerializationPatch.Override = delegate
		{
			PhotonView component = ((Component)Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				return false;
			}
			PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { component });
			Vector3 position = ((Component)VRRig.LocalRig).transform.position;
			Quaternion rotation = ((Component)VRRig.LocalRig).transform.rotation;
			Vector3 position2 = VRRig.LocalRig.leftHand.rigTarget.position;
			Quaternion rotation2 = VRRig.LocalRig.leftHand.rigTarget.rotation;
			Vector3 position3 = VRRig.LocalRig.rightHand.rigTarget.position;
			Quaternion rotation3 = VRRig.LocalRig.rightHand.rigTarget.rotation;
			Quaternion rotation4 = ((Component)VRRig.LocalRig.head.rigTarget).transform.rotation;
			NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
			int num = 0;
			while (num < playerListOthers.Length)
			{
				NetPlayer val = playerListOthers[num];
				VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
				if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
				{
					((Component)VRRig.LocalRig).transform.position = ((Component)vRRigFromNetPlayer).transform.position + ((Component)vRRigFromNetPlayer).transform.forward * (0f - (0.2f + Mathf.Sin((float)Time.frameCount / 8f) * 0.1f));
					((Component)VRRig.LocalRig).transform.rotation = ((Component)vRRigFromNetPlayer).transform.rotation;
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.position = ((Component)vRRigFromNetPlayer).transform.position + ((Component)vRRigFromNetPlayer).transform.right * -0.2f + ((Component)vRRigFromNetPlayer).transform.up * -0.4f;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.position = ((Component)vRRigFromNetPlayer).transform.position + ((Component)vRRigFromNetPlayer).transform.right * 0.2f + ((Component)vRRigFromNetPlayer).transform.up * -0.4f;
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.rotation = ((Component)vRRigFromNetPlayer).transform.rotation;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.rotation = ((Component)vRRigFromNetPlayer).transform.rotation;
					((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = ((Component)vRRigFromNetPlayer).transform.rotation;
					RaiseEventOptions val2 = new RaiseEventOptions();
					val2.TargetActors = new int[1] { val.ActorNumber };
					PhotonSerializer.SendSerialize(component, val2);
					num++;
				}
				else
				{
					num++;
				}
			}
			Safety.RPCShield();
			((Component)VRRig.LocalRig).transform.position = position;
			((Component)VRRig.LocalRig).transform.rotation = rotation;
			VRRig.LocalRig.leftHand.rigTarget.position = position2;
			VRRig.LocalRig.leftHand.rigTarget.rotation = rotation2;
			VRRig.LocalRig.rightHand.rigTarget.position = position3;
			VRRig.LocalRig.rightHand.rigTarget.rotation = rotation3;
			((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = rotation4;
			return false;
		};
	}

	public static void PiggybackPlayerGun()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			Collider collider = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if (GunLib.GunTriggers)
			{
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
				{
					GunLib.lockedTargetRig = GunLib.potentialTargetRig;
					return;
				}
				Vector3 position = ((Component)GunLib.lockedTargetRig).transform.position + ((Component)GunLib.lockedTargetRig).transform.up * 0.3f - ((Component)GunLib.lockedTargetRig).transform.forward * 0.4f;
				TeleportTo(position);
			}
			else
			{
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			SetRigStatus(rigStatus: true);
			GunLib.CancelGunUse();
		}
	}

	public static void Cartwheel()
	{
		if (!InputHandler.RTrigger())
		{
			_cartWheelAngle = 0f;
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, 0.15f, 0f);
		_cartWheelAngle = (_cartWheelAngle + 270f * Time.deltaTime) % 360f;
		SetRigRotation(0f, HeadY, _cartWheelAngle);
		SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1.2f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1.2f);
	}

	public static void DisableBackflip()
	{
		_backflipAngle = 0f;
		SetRigStatus(rigStatus: true);
	}

	public static void SpazMonke()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (!InputHandler.isOn)
		{
			ResetHeadRotation();
			return;
		}
		Vector3 val = default(Vector3);
		val = new Vector3(UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f));
		SetHeadRotation(val);
		Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.eulerAngles = val;
		Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.eulerAngles = val;
	}

	public static void GrabRig()
	{
		Transform val = null;
		if (InputHandler.RGrip() && !InputHandler.LGrip())
		{
			val = Variables.taggerInstance.rightHandTransform;
		}
		else if (InputHandler.LGrip() && !InputHandler.RGrip())
		{
			val = Variables.taggerInstance.leftHandTransform;
		}
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
			((Component)Variables.taggerInstance.offlineVRRig).transform.SetPositionAndRotation(val.position, val.rotation);
			((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = val.rotation;
			SetHands(val.position + val.right * -0.3f, val.position + val.right * 0.3f, val.rotation);
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
	}

	public static void RagdollMonke()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (!InputHandler.isOn)
		{
			if ((UnityEngine.Object)(object)_ragdollBody != (UnityEngine.Object)null)
			{
				DisableRagdollMonke();
			}
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		if ((UnityEngine.Object)(object)_ragdollBody == (UnityEngine.Object)null)
		{
			_ragdollObj = new GameObject("RagdollPhysics");
			_ragdollCollider = _ragdollObj.AddComponent<BoxCollider>();
			_ragdollCollider.size = new Vector3(0.4f, 0.9f, 0.4f);
			_ragdollCollider.center = new Vector3(0f, -0.3f, 0f);
			_ragdollBody = _ragdollObj.AddComponent<Rigidbody>();
			_ragdollBody.isKinematic = false;
			_ragdollBody.mass = 1f;
			_ragdollBody.drag = 0.5f;
			_ragdollBody.angularDrag = 0.5f;
			_ragdollBody.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position;
			_ragdollObj.layer = 8;
			((Component)Variables.taggerInstance.offlineVRRig).transform.position = _ragdollObj.transform.position;
			((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = _ragdollObj.transform.rotation;
		}
		else
		{
			((Component)Variables.taggerInstance.offlineVRRig).transform.position = _ragdollObj.transform.position;
			((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = _ragdollObj.transform.rotation;
		}
	}

	public static void CopyPlayer(VRRig target)
	{
		if ((UnityEngine.Object)(object)target == (UnityEngine.Object)null || (UnityEngine.Object)(object)Variables.taggerInstance?.offlineVRRig == (UnityEngine.Object)null)
		{
			return;
		}
		VRRig offlineVRRig = Variables.taggerInstance.offlineVRRig;
		((Behaviour)offlineVRRig).enabled = false;
		((Component)offlineVRRig).transform.SetPositionAndRotation(((Component)target).transform.position, ((Component)target).transform.rotation);
		Transform[] array = (Transform[])(object)new Transform[3]
		{
			offlineVRRig.head.rigTarget,
			offlineVRRig.leftHand.rigTarget,
			offlineVRRig.rightHand.rigTarget
		};
		Transform[] array2 = (Transform[])(object)new Transform[3]
		{
			target.head.rigTarget,
			target.leftHand.rigTarget,
			target.rightHand.rigTarget
		};
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPositionAndRotation(array2[i].position, array2[i].rotation);
		}
		if (target.leftIndex != null)
		{
			((VRMap)offlineVRRig.leftIndex).calcT = ((VRMap)target.leftIndex).calcT;
			((VRMap)offlineVRRig.leftIndex).LerpFinger(1f, false);
		}
		if (target.leftMiddle != null)
		{
			((VRMap)offlineVRRig.leftMiddle).calcT = ((VRMap)target.leftMiddle).calcT;
			((VRMap)offlineVRRig.leftMiddle).LerpFinger(1f, false);
		}
		if (target.leftThumb != null)
		{
			((VRMap)offlineVRRig.leftThumb).calcT = ((VRMap)target.leftThumb).calcT;
			((VRMap)offlineVRRig.leftThumb).LerpFinger(1f, false);
		}
		if (target.rightIndex != null)
		{
			((VRMap)offlineVRRig.rightIndex).calcT = ((VRMap)target.rightIndex).calcT;
			((VRMap)offlineVRRig.rightIndex).LerpFinger(1f, false);
		}
		if (target.rightMiddle != null)
		{
			((VRMap)offlineVRRig.rightMiddle).calcT = ((VRMap)target.rightMiddle).calcT;
			((VRMap)offlineVRRig.rightMiddle).LerpFinger(1f, false);
		}
		if (target.rightThumb != null)
		{
			((VRMap)offlineVRRig.rightThumb).calcT = ((VRMap)target.rightThumb).calcT;
			((VRMap)offlineVRRig.rightThumb).LerpFinger(1f, false);
		}
	}

	public static void CopyAll()
	{
		MenuPatches.SerializationPatch.Override = delegate
		{
			PhotonView component = ((Component)Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				return false;
			}
			PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { component });
			Vector3 position = ((Component)VRRig.LocalRig).transform.position;
			Quaternion rotation = ((Component)VRRig.LocalRig).transform.rotation;
			Vector3 position2 = VRRig.LocalRig.leftHand.rigTarget.position;
			Quaternion rotation2 = VRRig.LocalRig.leftHand.rigTarget.rotation;
			Vector3 position3 = VRRig.LocalRig.rightHand.rigTarget.position;
			Quaternion rotation3 = VRRig.LocalRig.rightHand.rigTarget.rotation;
			Quaternion rotation4 = ((Component)VRRig.LocalRig.head.rigTarget).transform.rotation;
			NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
			int num = 0;
			while (num < playerListOthers.Length)
			{
				NetPlayer val = playerListOthers[num];
				VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
				if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
				{
					((Component)VRRig.LocalRig).transform.SetPositionAndRotation(((Component)vRRigFromNetPlayer).transform.position, ((Component)vRRigFromNetPlayer).transform.rotation);
					VRRig.LocalRig.head.rigTarget.SetPositionAndRotation(vRRigFromNetPlayer.head.rigTarget.position, vRRigFromNetPlayer.head.rigTarget.rotation);
					VRRig.LocalRig.leftHand.rigTarget.SetPositionAndRotation(vRRigFromNetPlayer.leftHand.rigTarget.position, vRRigFromNetPlayer.leftHand.rigTarget.rotation);
					VRRig.LocalRig.rightHand.rigTarget.SetPositionAndRotation(vRRigFromNetPlayer.rightHand.rigTarget.position, vRRigFromNetPlayer.rightHand.rigTarget.rotation);
					RaiseEventOptions val2 = new RaiseEventOptions();
					val2.TargetActors = new int[1] { val.ActorNumber };
					PhotonSerializer.SendSerialize(component, val2);
					num++;
				}
				else
				{
					num++;
				}
			}
			Safety.RPCShield();
			((Component)VRRig.LocalRig).transform.position = position;
			((Component)VRRig.LocalRig).transform.rotation = rotation;
			VRRig.LocalRig.leftHand.rigTarget.position = position2;
			VRRig.LocalRig.leftHand.rigTarget.rotation = rotation2;
			VRRig.LocalRig.rightHand.rigTarget.position = position3;
			VRRig.LocalRig.rightHand.rigTarget.rotation = rotation3;
			((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = rotation4;
			return false;
		};
	}

	public static void Backflip()
	{
		if (!InputHandler.RTrigger())
		{
			_backflipAngle = 0f;
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, 0.15f, 0f);
		_backflipAngle = (_backflipAngle - 360f * Time.deltaTime + 360f) % 360f;
		SetRigRotation(_backflipAngle, HeadY, 0f);
		SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -0.6f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.2f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 0.6f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.2f);
	}

	public static void DisableCartwheel()
	{
		_cartWheelAngle = 0f;
		SetRigStatus(rigStatus: true);
	}

	public static void FreezeRig()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (InputHandler.isOn)
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
			((Component)Variables.taggerInstance.offlineVRRig).transform.SetPositionAndRotation(((Component)Variables.taggerInstance.headCollider).transform.position, ((Component)Variables.taggerInstance.headCollider).transform.rotation);
		}
		else
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
		}
	}

	public static void SizeChanger()
	{
		if (InputHandler.LSecondary())
		{
			sizeScale = 1f;
		}
		if (InputHandler.LTrigger())
		{
			sizeScale -= 0.05f;
		}
		if (InputHandler.RTrigger())
		{
			sizeScale += 0.05f;
		}
		Variables.playerInstance.nativeScale = sizeScale;
	}

	public static void MirrorAll()
	{
		MenuPatches.SerializationPatch.Override = delegate
		{
			PhotonView component = ((Component)Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				return false;
			}
			PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { component });
			Vector3 position = ((Component)VRRig.LocalRig).transform.position;
			Quaternion rotation = ((Component)VRRig.LocalRig).transform.rotation;
			Vector3 position2 = VRRig.LocalRig.leftHand.rigTarget.position;
			Quaternion rotation2 = VRRig.LocalRig.leftHand.rigTarget.rotation;
			Vector3 position3 = VRRig.LocalRig.rightHand.rigTarget.position;
			Quaternion rotation3 = VRRig.LocalRig.rightHand.rigTarget.rotation;
			Quaternion rotation4 = ((Component)VRRig.LocalRig.head.rigTarget).transform.rotation;
			NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
			int num = 0;
			while (num < playerListOthers.Length)
			{
				NetPlayer val = playerListOthers[num];
				VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
				if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
				{
					((Component)VRRig.LocalRig).transform.position = ((Component)vRRigFromNetPlayer).transform.position + ((Component)vRRigFromNetPlayer).transform.forward * 1.5f;
					((Component)VRRig.LocalRig).transform.rotation = Quaternion.Euler(0f, ((Component)vRRigFromNetPlayer).transform.eulerAngles.y + 180f, 0f);
					Vector3 val2 = ((Component)vRRigFromNetPlayer).transform.InverseTransformPoint(vRRigFromNetPlayer.leftHand.rigTarget.position);
					Vector3 val3 = ((Component)vRRigFromNetPlayer).transform.InverseTransformPoint(vRRigFromNetPlayer.rightHand.rigTarget.position);
					val2.x *= -1f;
					val3.x *= -1f;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.position = ((Component)VRRig.LocalRig).transform.TransformPoint(val2);
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.position = ((Component)VRRig.LocalRig).transform.TransformPoint(val3);
					Quaternion rotation5 = vRRigFromNetPlayer.leftHand.rigTarget.rotation;
					Quaternion rotation6 = vRRigFromNetPlayer.rightHand.rigTarget.rotation;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.rotation = new Quaternion(0f - rotation5.x, rotation5.y, rotation5.z, 0f - rotation5.w);
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.rotation = new Quaternion(0f - rotation6.x, rotation6.y, rotation6.z, 0f - rotation6.w);
					VRRig.LocalRig.head.rigTarget.rotation = Quaternion.Euler(vRRigFromNetPlayer.head.rigTarget.eulerAngles.x, vRRigFromNetPlayer.head.rigTarget.eulerAngles.y + 180f, 0f - vRRigFromNetPlayer.head.rigTarget.eulerAngles.z);
					RaiseEventOptions val4 = new RaiseEventOptions();
					val4.TargetActors = new int[1] { val.ActorNumber };
					PhotonSerializer.SendSerialize(component, val4);
					num++;
				}
				else
				{
					num++;
				}
			}
			Safety.RPCShield();
			((Component)VRRig.LocalRig).transform.position = position;
			((Component)VRRig.LocalRig).transform.rotation = rotation;
			VRRig.LocalRig.leftHand.rigTarget.position = position2;
			VRRig.LocalRig.leftHand.rigTarget.rotation = rotation2;
			VRRig.LocalRig.rightHand.rigTarget.position = position3;
			VRRig.LocalRig.rightHand.rigTarget.rotation = rotation3;
			((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = rotation4;
			return false;
		};
	}

	public static void OnlyInvisibleToPlayerGun()
	{
		if (GunLib.SetupLockOnGun())
		{
			MenuPatches.SerializationPatch.Override = delegate
			{
				NetPlayer target = RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig);
				PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { VRRig.LocalRig.GetPhotonView() });
				Vector3 position = ((Component)VRRig.LocalRig).transform.position;
				PhotonSerializer.SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions
				{
					TargetActors = (from plr in PhotonNetwork.PlayerList
						where plr.ActorNumber != target.ActorNumber
						select plr.ActorNumber).ToArray()
				});
				((Component)VRRig.LocalRig).transform.position = new Vector3(UnityEngine.Random.Range(-99999f, 99999f), 99999f, UnityEngine.Random.Range(-99999f, 99999f));
				PhotonView photonView = VRRig.LocalRig.GetPhotonView();
				RaiseEventOptions val = new RaiseEventOptions();
				val.TargetActors = new int[1] { target.ActorNumber };
				PhotonSerializer.SendSerialize(photonView, val);
				Safety.RPCShield();
				((Component)VRRig.LocalRig).transform.position = position;
				return false;
			};
		}
		else
		{
			MenuPatches.SerializationPatch.Override = null;
		}
	}

	public static void SpiderMonke()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (!InputHandler.isOn)
		{
			DisableSpiderMonke();
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		Vector3 position = ((Component)Variables.taggerInstance.bodyCollider).transform.position;
		if (!_spiderInit)
		{
			_spiderLastPos = position;
			_spiderInit = true;
		}
		Vector3 val = position - _spiderLastPos;
		float num = val.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
		_spiderLastPos = position;
		_spider += num * Time.deltaTime * 1.6f;
		_spiderIdle += Time.deltaTime;
		((Component)Variables.taggerInstance.offlineVRRig).transform.position = position + new Vector3(0f, 0.02f, 0f);
		SetRigRotation(80f, HeadY, Mathf.Sin(_spiderIdle * 4f) * 5f);
		if (num > 0.4f)
		{
			((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = Quaternion.Euler(-25f, HeadY + Mathf.Sin(_spider * 5f) * 10f, Mathf.Sin(_spider * 6f) * 8f);
		}
		else
		{
			((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = Quaternion.Euler(-20f, HeadY + Mathf.Sin(_spiderIdle * 0.8f) * 35f, Mathf.Sin(_spiderIdle * 0.6f) * 10f);
		}
		float phase = _spider % 1f;
		float phase2 = (_spider + 0.5f) % 1f;
		SetCrawlHand(Variables.taggerInstance.offlineVRRig.leftHand.rigTarget, phase, -0.45f);
		SetCrawlHand(Variables.taggerInstance.offlineVRRig.rightHand.rigTarget, phase2, 0.45f);
		((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.rotation = ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation;
		((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.rotation = ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation;
	}

	public static void GawkGawkAll()
	{
		MenuPatches.SerializationPatch.Override = delegate
		{
			PhotonView component = ((Component)Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				return false;
			}
			PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { component });
			Vector3 position = ((Component)VRRig.LocalRig).transform.position;
			Quaternion rotation = ((Component)VRRig.LocalRig).transform.rotation;
			Vector3 position2 = VRRig.LocalRig.leftHand.rigTarget.position;
			Quaternion rotation2 = VRRig.LocalRig.leftHand.rigTarget.rotation;
			Vector3 position3 = VRRig.LocalRig.rightHand.rigTarget.position;
			Quaternion rotation3 = VRRig.LocalRig.rightHand.rigTarget.rotation;
			Quaternion rotation4 = ((Component)VRRig.LocalRig.head.rigTarget).transform.rotation;
			NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
			int num = 0;
			while (num < playerListOthers.Length)
			{
				NetPlayer val = playerListOthers[num];
				VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
				if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
				{
					((Component)VRRig.LocalRig).transform.position = ((Component)vRRigFromNetPlayer).transform.position + ((Component)vRRigFromNetPlayer).transform.forward * (0.2f + Mathf.Sin((float)Time.frameCount / 8f) * 0.1f) + ((Component)vRRigFromNetPlayer).transform.up * -0.4f;
					Transform transform = ((Component)VRRig.LocalRig).transform;
					Quaternion rotation5 = ((Component)vRRigFromNetPlayer).transform.rotation;
					Quaternion val2 = rotation5;
					transform.rotation = Quaternion.Euler(val2.eulerAngles + new Vector3(0f, 180f, 0f));
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.position = ((Component)vRRigFromNetPlayer).transform.position + ((Component)vRRigFromNetPlayer).transform.right * 0.2f + ((Component)vRRigFromNetPlayer).transform.up * -0.4f;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.position = ((Component)vRRigFromNetPlayer).transform.position + ((Component)vRRigFromNetPlayer).transform.right * -0.2f + ((Component)vRRigFromNetPlayer).transform.up * -0.4f;
					Transform transform2 = ((Component)VRRig.LocalRig.leftHand.rigTarget).transform;
					rotation5 = ((Component)vRRigFromNetPlayer).transform.rotation;
					val2 = rotation5;
					transform2.rotation = Quaternion.Euler(val2.eulerAngles + new Vector3(0f, 180f, 0f));
					Transform transform3 = ((Component)VRRig.LocalRig.rightHand.rigTarget).transform;
					rotation5 = ((Component)vRRigFromNetPlayer).transform.rotation;
					val2 = rotation5;
					transform3.rotation = Quaternion.Euler(val2.eulerAngles + new Vector3(0f, 180f, 0f));
					Transform transform4 = ((Component)VRRig.LocalRig.head.rigTarget).transform;
					rotation5 = ((Component)vRRigFromNetPlayer).transform.rotation;
					val2 = rotation5;
					transform4.rotation = Quaternion.Euler(val2.eulerAngles + new Vector3(0f, 180f, 0f));
					RaiseEventOptions val3 = new RaiseEventOptions();
					val3.TargetActors = new int[1] { val.ActorNumber };
					PhotonSerializer.SendSerialize(component, val3);
					num++;
				}
				else
				{
					num++;
				}
			}
			Safety.RPCShield();
			((Component)VRRig.LocalRig).transform.position = position;
			((Component)VRRig.LocalRig).transform.rotation = rotation;
			VRRig.LocalRig.leftHand.rigTarget.position = position2;
			VRRig.LocalRig.leftHand.rigTarget.rotation = rotation2;
			VRRig.LocalRig.rightHand.rigTarget.position = position3;
			VRRig.LocalRig.rightHand.rigTarget.rotation = rotation3;
			((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = rotation4;
			return false;
		};
	}

	public static void GlitchMonke()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (!InputHandler.isOn)
		{
			DisableGlitchMonke();
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		_glitchOffset += Time.deltaTime;
		_glitchTimer += Time.deltaTime;
		if (_glitchTimer > 0.05f)
		{
			_glitchTimer = 0f;
			_glitchPos = new Vector3(UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f));
			((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, 0.15f, 0f) + _glitchPos;
			SetRigRotation(UnityEngine.Random.Range(-5f, 5f), HeadY + UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f));
			SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -0.4f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.1f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 0.4f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.1f);
		}
		else
		{
			((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, 0.15f, 0f) + _glitchPos;
			SetRigRotation(UnityEngine.Random.Range(-5f, 5f), HeadY + UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f));
			SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -0.4f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.1f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 0.4f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.1f);
		}
	}

	public static void FakeBodyTracking()
	{
		((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = ((Component)Camera.main).transform.rotation;
		((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = ((Component)Variables.playerInstance.LeftHand.handFollower).transform.position;
		((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = ((Component)Variables.playerInstance.RightHand.handFollower).transform.position;
	}

	public static void LayOnStomach()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (InputHandler.isOn)
		{
			if (!wasRigDisabled)
			{
				_layOnStomachPos = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, -0.15f, 0f);
				wasRigDisabled = true;
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = _layOnStomachPos;
				((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
				((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = Quaternion.Euler(0f, 90f, 90f);
				SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.25f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * -0.5f);
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = _layOnStomachPos;
				((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
				((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = Quaternion.Euler(0f, 90f, 90f);
				SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.25f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * -0.5f);
			}
		}
		else
		{
			wasRigDisabled = false;
			SetRigStatus(rigStatus: true);
			ResetHeadRotation();
		}
	}

	public static void OnlyVisibleToPlayerGun()
	{
		if (GunLib.SetupLockOnGun())
		{
			MenuPatches.SerializationPatch.Override = delegate
			{
				NetPlayer target = RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig);
				PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { VRRig.LocalRig.GetPhotonView() });
				Vector3 position = ((Component)VRRig.LocalRig).transform.position;
				PhotonView photonView = VRRig.LocalRig.GetPhotonView();
				RaiseEventOptions val = new RaiseEventOptions();
				val.TargetActors = new int[1] { target.ActorNumber };
				PhotonSerializer.SendSerialize(photonView, val);
				((Component)VRRig.LocalRig).transform.position = new Vector3(UnityEngine.Random.Range(-99999f, 99999f), 99999f, UnityEngine.Random.Range(-99999f, 99999f));
				PhotonSerializer.SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions
				{
					TargetActors = (from plr in PhotonNetwork.PlayerList
						where plr.ActorNumber != target.ActorNumber
						select plr.ActorNumber).ToArray()
				});
				Safety.RPCShield();
				((Component)VRRig.LocalRig).transform.position = position;
				return false;
			};
		}
		else
		{
			MenuPatches.SerializationPatch.Override = null;
		}
	}

	private static void SetRigRotation(float x, float y, float z)
	{
		Transform transform = ((Component)Variables.taggerInstance.offlineVRRig).transform;
		Quaternion rotation = (((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = Quaternion.Euler(x, y, z));
		transform.rotation = rotation;
	}

	public static void BackshotPlayerGun()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			Collider collider = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if (GunLib.GunTriggers)
			{
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
				{
					GunLib.lockedTargetRig = GunLib.potentialTargetRig;
					return;
				}
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)GunLib.lockedTargetRig).transform.position + ((Component)GunLib.lockedTargetRig).transform.forward * (0f - (0.2f + Mathf.Sin((float)Time.frameCount / 8f) * 0.1f));
				((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = ((Component)GunLib.lockedTargetRig).transform.rotation;
				((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = ((Component)GunLib.lockedTargetRig).transform.position + ((Component)GunLib.lockedTargetRig).transform.right * -0.2f + ((Component)GunLib.lockedTargetRig).transform.up * -0.4f;
				((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = ((Component)GunLib.lockedTargetRig).transform.position + ((Component)GunLib.lockedTargetRig).transform.right * 0.2f + ((Component)GunLib.lockedTargetRig).transform.up * -0.4f;
				((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.rotation = ((Component)GunLib.lockedTargetRig).transform.rotation;
				((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.rotation = ((Component)GunLib.lockedTargetRig).transform.rotation;
				((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = ((Component)GunLib.lockedTargetRig).transform.rotation;
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			SetRigStatus(rigStatus: true);
			GunLib.CancelGunUse();
		}
	}

	public static void GawkGawkGun()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			Collider collider = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if (GunLib.GunTriggers)
			{
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
				{
					GunLib.lockedTargetRig = GunLib.potentialTargetRig;
					return;
				}
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)GunLib.lockedTargetRig).transform.position + ((Component)GunLib.lockedTargetRig).transform.forward * (0.2f + Mathf.Sin((float)Time.frameCount / 8f) * 0.1f) + ((Component)GunLib.lockedTargetRig).transform.up * -0.4f;
				Transform transform = ((Component)Variables.taggerInstance.offlineVRRig).transform;
				Quaternion rotation = ((Component)GunLib.lockedTargetRig).transform.rotation;
				Quaternion val = rotation;
				transform.rotation = Quaternion.Euler(val.eulerAngles + new Vector3(0f, 180f, 0f));
				((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = ((Component)GunLib.lockedTargetRig).transform.position + ((Component)GunLib.lockedTargetRig).transform.right * 0.2f + ((Component)GunLib.lockedTargetRig).transform.up * -0.4f;
				((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = ((Component)GunLib.lockedTargetRig).transform.position + ((Component)GunLib.lockedTargetRig).transform.right * -0.2f + ((Component)GunLib.lockedTargetRig).transform.up * -0.4f;
				Transform transform2 = ((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform;
				rotation = ((Component)GunLib.lockedTargetRig).transform.rotation;
				val = rotation;
				transform2.rotation = Quaternion.Euler(val.eulerAngles + new Vector3(0f, 180f, 0f));
				Transform transform3 = ((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform;
				rotation = ((Component)GunLib.lockedTargetRig).transform.rotation;
				val = rotation;
				transform3.rotation = Quaternion.Euler(val.eulerAngles + new Vector3(0f, 180f, 0f));
				Transform transform4 = ((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform;
				rotation = ((Component)GunLib.lockedTargetRig).transform.rotation;
				val = rotation;
				transform4.rotation = Quaternion.Euler(val.eulerAngles + new Vector3(0f, 180f, 0f));
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			SetRigStatus(rigStatus: true);
			GunLib.CancelGunUse();
		}
	}

	public static void OrbitPlayerGun()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			Collider collider = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if (GunLib.GunTriggers)
			{
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
				{
					GunLib.lockedTargetRig = GunLib.potentialTargetRig;
					return;
				}
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.LookAt(((Component)GunLib.lockedTargetRig).transform.position);
				((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1.5f;
				((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1.5f;
				((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
				((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
				((Component)Variables.taggerInstance.offlineVRRig).transform.RotateAround(((Component)GunLib.lockedTargetRig).transform.position, Vector3.up, orbitSpeed * Time.deltaTime);
				Vector3 val = ((Component)Variables.taggerInstance.offlineVRRig).transform.position - ((Component)GunLib.lockedTargetRig).transform.position;
				Vector3 position = val.normalized * orbitDistance + ((Component)GunLib.lockedTargetRig).transform.position;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = position;
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			SetRigStatus(rigStatus: true);
			GunLib.CancelGunUse();
		}
	}

	private static void SetHands(Vector3 left, Vector3 right, Quaternion? rot = null)
	{
		Quaternion? val = rot;
		if (!val.HasValue)
		{
			Quaternion rotation = ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation;
			((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = left;
			((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = right;
			((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.rotation = rotation;
			((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.rotation = rotation;
		}
		else
		{
			Quaternion rotation = val.GetValueOrDefault();
			((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = left;
			((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = right;
			((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.rotation = rotation;
			((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.rotation = rotation;
		}
	}

	public static void UpsideDown()
	{
		if (!InputHandler.RTrigger())
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position;
		((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(0f, HeadY, 180f);
		Transform transform = ((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform;
		Quaternion rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation;
		Quaternion val = rotation;
		float num = 0f - val.eulerAngles.x;
		rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation;
		val = rotation;
		transform.rotation = Quaternion.Euler(num, val.eulerAngles.y, 180f);
		Vector3 position = Variables.playerInstance.LeftHand.controllerTransform.position;
		Vector3 position2 = Variables.playerInstance.RightHand.controllerTransform.position;
		Vector3 position3 = ((Component)Variables.taggerInstance.offlineVRRig).transform.position;
		Vector3 position4 = position3 + (position - position3) * -1f;
		Vector3 position5 = position3 + (position2 - position3) * -1f;
		((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = position4;
		((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = position5;
		Quaternion rotation2 = Variables.playerInstance.LeftHand.controllerTransform.rotation;
		Quaternion rotation3 = Variables.playerInstance.RightHand.controllerTransform.rotation;
		((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.rotation = new Quaternion(0f - rotation2.x, rotation2.y, rotation2.z, 0f - rotation2.w);
		((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.rotation = new Quaternion(0f - rotation3.x, rotation3.y, rotation3.z, 0f - rotation3.w);
	}

	public static void CopyPlayerGun()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			Collider collider = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if (GunLib.GunTriggers)
			{
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
				{
					GunLib.lockedTargetRig = GunLib.potentialTargetRig;
				}
				else
				{
					CopyPlayer(GunLib.lockedTargetRig);
				}
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			SetRigStatus(rigStatus: true);
			GunLib.CancelGunUse();
		}
	}

	public static void TeleportTo(Vector3 position)
	{
		Vector3 val = position - ((Component)Variables.taggerInstance.bodyCollider).transform.position + ((Component)Variables.taggerInstance).transform.position;
		Variables.playerInstance.TeleportTo(val, ((Component)Variables.playerInstance).transform.rotation, false, false);
	}

	public static void GhostInvisibleMonke()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RPrimary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn2, ref InputHandler.wasButtonPressed2);
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = !InputHandler.isOn && !InputHandler.isOn2;
		if (InputHandler.isOn2)
		{
			((Component)Variables.taggerInstance.offlineVRRig).transform.position = new Vector3(999f, 999f, 999f);
		}
	}

	public static void JumpscareAll()
	{
		MenuPatches.SerializationPatch.Override = delegate
		{
			PhotonView component = ((Component)Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				return false;
			}
			PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { component });
			Vector3 position = ((Component)VRRig.LocalRig).transform.position;
			Quaternion rotation = ((Component)VRRig.LocalRig).transform.rotation;
			Vector3 position2 = VRRig.LocalRig.leftHand.rigTarget.position;
			Quaternion rotation2 = VRRig.LocalRig.leftHand.rigTarget.rotation;
			Vector3 position3 = VRRig.LocalRig.rightHand.rigTarget.position;
			Quaternion rotation3 = VRRig.LocalRig.rightHand.rigTarget.rotation;
			Quaternion rotation4 = ((Component)VRRig.LocalRig.head.rigTarget).transform.rotation;
			NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
			int num = 0;
			while (num < playerListOthers.Length)
			{
				NetPlayer val = playerListOthers[num];
				VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
				if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
				{
					((Component)VRRig.LocalRig).transform.position = vRRigFromNetPlayer.headMesh.transform.position + vRRigFromNetPlayer.headMesh.transform.forward * UnityEngine.Random.Range(0.1f, 0.5f);
					((Component)VRRig.LocalRig.head.rigTarget).transform.LookAt(vRRigFromNetPlayer.headMesh.transform.position);
					Quaternion rotation5 = ((Component)VRRig.LocalRig.head.rigTarget).transform.rotation;
					((Component)VRRig.LocalRig).transform.rotation = rotation5;
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.position = vRRigFromNetPlayer.headMesh.transform.position + vRRigFromNetPlayer.headMesh.transform.right * 0.2f;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.position = vRRigFromNetPlayer.headMesh.transform.position + vRRigFromNetPlayer.headMesh.transform.right * -0.2f;
					Transform transform = ((Component)VRRig.LocalRig.leftHand.rigTarget).transform;
					Quaternion rotation6 = ((Component)VRRig.LocalRig).transform.rotation;
					Quaternion val2 = rotation6;
					transform.rotation = Quaternion.Euler(val2.eulerAngles + new Vector3(0f, 180f, 0f));
					Transform transform2 = ((Component)VRRig.LocalRig.rightHand.rigTarget).transform;
					rotation6 = ((Component)VRRig.LocalRig).transform.rotation;
					val2 = rotation6;
					transform2.rotation = Quaternion.Euler(val2.eulerAngles + new Vector3(0f, 180f, 0f));
					((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = rotation5;
					RaiseEventOptions val3 = new RaiseEventOptions();
					val3.TargetActors = new int[1] { val.ActorNumber };
					PhotonSerializer.SendSerialize(component, val3);
					num++;
				}
				else
				{
					num++;
				}
			}
			Safety.RPCShield();
			((Component)VRRig.LocalRig).transform.position = position;
			((Component)VRRig.LocalRig).transform.rotation = rotation;
			VRRig.LocalRig.leftHand.rigTarget.position = position2;
			VRRig.LocalRig.leftHand.rigTarget.rotation = rotation2;
			VRRig.LocalRig.rightHand.rigTarget.position = position3;
			VRRig.LocalRig.rightHand.rigTarget.rotation = rotation3;
			((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = rotation4;
			return false;
		};
	}

	public static void AscendingMonke()
	{
		if (InputHandler.RTrigger())
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
			Transform transform = ((Component)Variables.taggerInstance.offlineVRRig).transform;
			transform.position += new Vector3(0f, 0.01f, 0f);
			((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation;
			((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1f;
			((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1f;
			SetHeadRotation((Vector3?)new Vector3(180f, 0f, 0f), false);
		}
		else
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
		}
	}

	public static void JumpscarePlayerGun()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			Collider collider = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if (GunLib.GunTriggers)
			{
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
				{
					GunLib.lockedTargetRig = GunLib.potentialTargetRig;
					return;
				}
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = GunLib.lockedTargetRig.headMesh.transform.position + GunLib.lockedTargetRig.headMesh.transform.forward * UnityEngine.Random.Range(0.1f, 0.5f);
				((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.LookAt(GunLib.lockedTargetRig.headMesh.transform.position);
				((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = ((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation;
				((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = GunLib.lockedTargetRig.headMesh.transform.position + GunLib.lockedTargetRig.headMesh.transform.right * 0.2f;
				((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = GunLib.lockedTargetRig.headMesh.transform.position + GunLib.lockedTargetRig.headMesh.transform.right * -0.2f;
				((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = ((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation;
				Transform transform = ((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform;
				Quaternion rotation = ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation;
				Quaternion val = rotation;
				transform.rotation = Quaternion.Euler(val.eulerAngles + new Vector3(0f, 180f, 0f));
				Transform transform2 = ((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform;
				rotation = ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation;
				val = rotation;
				transform2.rotation = Quaternion.Euler(val.eulerAngles + new Vector3(0f, 180f, 0f));
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			SetRigStatus(rigStatus: true);
			GunLib.CancelGunUse();
		}
	}

	public static void DisableRagdollMonke()
	{
		if ((UnityEngine.Object)(object)_ragdollObj != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_ragdollObj);
			_ragdollObj = null;
			_ragdollBody = null;
			_ragdollCollider = null;
			SetRigStatus(rigStatus: true);
		}
		else
		{
			SetRigStatus(rigStatus: true);
		}
	}

	public static void TeleportToMap()
	{
		(string, string, string) tuple = _maps[Settings._selectedMapIndex];
		GameObject obj = Variables.FindObject(tuple.Item2);
		if (obj != null)
		{
			GorillaSetZoneTrigger component = obj.GetComponent<GorillaSetZoneTrigger>();
			if (component != null)
			{
				((GorillaTriggerBox)component).OnBoxTriggered();
				GameObject obj2 = Variables.FindObject(tuple.Item3);
				TeleportTo((obj2 != null) ? obj2.transform.position : ((Component)VRRig.LocalRig).transform.position);
			}
		}
		GameObject obj3 = Variables.FindObject(tuple.Item3);
		TeleportTo((obj3 != null) ? obj3.transform.position : ((Component)VRRig.LocalRig).transform.position);
	}

	public static void MirrorPlayerGun()
	{
		VRRig offlineVRRig;
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			Collider collider = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if (GunLib.GunTriggers)
			{
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
				{
					GunLib.lockedTargetRig = GunLib.potentialTargetRig;
					return;
				}
				if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null || (UnityEngine.Object)(object)Variables.taggerInstance?.offlineVRRig == (UnityEngine.Object)null)
				{
					return;
				}
				offlineVRRig = Variables.taggerInstance.offlineVRRig;
				((Behaviour)offlineVRRig).enabled = false;
				Vector3 position = ((Component)GunLib.lockedTargetRig).transform.position + ((Component)GunLib.lockedTargetRig).transform.forward * 1.5f;
				((Component)offlineVRRig).transform.position = position;
				((Component)offlineVRRig).transform.rotation = Quaternion.Euler(0f, ((Component)GunLib.lockedTargetRig).transform.eulerAngles.y + 180f, 0f);
				Vector3 val = ((Component)GunLib.lockedTargetRig).transform.InverseTransformPoint(GunLib.lockedTargetRig.leftHand.rigTarget.position);
				Vector3 val2 = ((Component)GunLib.lockedTargetRig).transform.InverseTransformPoint(GunLib.lockedTargetRig.rightHand.rigTarget.position);
				val.x *= -1f;
				val2.x *= -1f;
				((Component)offlineVRRig.rightHand.rigTarget).transform.position = ((Component)offlineVRRig).transform.TransformPoint(val);
				((Component)offlineVRRig.leftHand.rigTarget).transform.position = ((Component)offlineVRRig).transform.TransformPoint(val2);
				Quaternion rotation = GunLib.lockedTargetRig.leftHand.rigTarget.rotation;
				Quaternion rotation2 = GunLib.lockedTargetRig.rightHand.rigTarget.rotation;
				((Component)offlineVRRig.rightHand.rigTarget).transform.rotation = new Quaternion(0f - rotation.x, rotation.y, rotation.z, 0f - rotation.w);
				((Component)offlineVRRig.leftHand.rigTarget).transform.rotation = new Quaternion(0f - rotation2.x, rotation2.y, rotation2.z, 0f - rotation2.w);
				offlineVRRig.head.rigTarget.rotation = Quaternion.Euler(GunLib.lockedTargetRig.head.rigTarget.eulerAngles.x, GunLib.lockedTargetRig.head.rigTarget.eulerAngles.y + 180f, 0f - GunLib.lockedTargetRig.head.rigTarget.eulerAngles.z);
				if (GunLib.lockedTargetRig.leftIndex != null)
				{
					((VRMap)offlineVRRig.rightIndex).calcT = ((VRMap)GunLib.lockedTargetRig.leftIndex).calcT;
					((VRMap)offlineVRRig.rightIndex).LerpFinger(1f, false);
				}
				if (GunLib.lockedTargetRig.leftMiddle != null)
				{
					((VRMap)offlineVRRig.rightMiddle).calcT = ((VRMap)GunLib.lockedTargetRig.leftMiddle).calcT;
					((VRMap)offlineVRRig.rightMiddle).LerpFinger(1f, false);
				}
				if (GunLib.lockedTargetRig.leftThumb != null)
				{
					((VRMap)offlineVRRig.rightThumb).calcT = ((VRMap)GunLib.lockedTargetRig.leftThumb).calcT;
					((VRMap)offlineVRRig.rightThumb).LerpFinger(1f, false);
				}
				if (GunLib.lockedTargetRig.rightIndex != null)
				{
					((VRMap)offlineVRRig.leftIndex).calcT = ((VRMap)GunLib.lockedTargetRig.rightIndex).calcT;
					((VRMap)offlineVRRig.leftIndex).LerpFinger(1f, false);
				}
				if (GunLib.lockedTargetRig.rightMiddle != null)
				{
					((VRMap)offlineVRRig.leftMiddle).calcT = ((VRMap)GunLib.lockedTargetRig.rightMiddle).calcT;
					((VRMap)offlineVRRig.leftMiddle).LerpFinger(1f, false);
				}
				if (GunLib.lockedTargetRig.rightThumb != null)
				{
					((VRMap)offlineVRRig.leftThumb).calcT = ((VRMap)GunLib.lockedTargetRig.rightThumb).calcT;
					((VRMap)offlineVRRig.leftThumb).LerpFinger(1f, false);
				}
				return;
			}
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			GunLib.lockedTargetRig = null;
			return;
		}
		SetRigStatus(rigStatus: true);
		GunLib.CancelGunUse();
	}

	public static void DanceMonke()
	{
		if (!InputHandler.RTrigger())
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			ResetHeadRotation();
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		float num = Time.time * 2f;
		float num2 = Time.time * 4f;
		((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(0f, HeadY + Mathf.Sin(num) * 30f, 0f);
		((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, 0.15f - Mathf.Abs(Mathf.Sin(num * 2f)) * 0.3f, 0f);
		SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -0.8f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * (0.3f + Mathf.Sin(num2) * 0.4f) + ((Component)Variables.taggerInstance.offlineVRRig).transform.forward * Mathf.Cos(num2) * 0.3f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 0.8f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * (0.3f + Mathf.Sin(num2 + MathF.PI) * 0.4f) + ((Component)Variables.taggerInstance.offlineVRRig).transform.forward * Mathf.Cos(num2 + MathF.PI) * 0.3f);
		((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.rotation = Quaternion.Euler(90f + Mathf.Sin(num2) * 45f, 0f, Mathf.Cos(num2) * 30f);
		((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.rotation = Quaternion.Euler(90f + Mathf.Sin(num2 + MathF.PI) * 45f, 0f, Mathf.Cos(num2 + MathF.PI) * 30f);
		SetHeadRotation((Vector3?)new Vector3(Mathf.Sin(num) * 15f, HeadY, Mathf.Cos(num2) * 10f), false);
	}

	public static void DisableWobblyMonke()
	{
		_wobbleOffset = 0f;
		SetRigStatus(rigStatus: true);
	}

	public static void HelicopterMonke()
	{
		if (InputHandler.RTrigger())
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
			Transform transform = ((Component)Variables.taggerInstance.offlineVRRig).transform;
			transform.position += new Vector3(0f, 0.075f, 0f);
			Transform transform2 = ((Component)Variables.taggerInstance.offlineVRRig).transform;
			Quaternion rotation = ((Component)Variables.taggerInstance.offlineVRRig).transform.rotation;
			transform2.rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0f, 10f, 0f));
			((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1f;
			((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1f;
		}
		else
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
		}
	}

	public static void Frontflip()
	{
		if (!InputHandler.RTrigger())
		{
			_frontflipAngle = 0f;
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, 0.15f, 0f);
		_frontflipAngle = (_frontflipAngle + 360f * Time.deltaTime) % 360f;
		SetRigRotation(_frontflipAngle, HeadY, 0f);
		SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -0.6f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.2f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 0.6f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.2f);
	}

	public static void GhostMonke()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = !InputHandler.isOn;
	}

	public static void InvisibleMonke()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (InputHandler.isOn)
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
			((Component)Variables.taggerInstance.offlineVRRig).transform.position = new Vector3(999f, 999f, 999f);
		}
		else
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
		}
	}

	public static void LayOnBack()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (InputHandler.isOn)
		{
			if (!wasRigDisabled)
			{
				_layOnBackPos = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, -0.15f, 0f);
				wasRigDisabled = true;
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = _layOnBackPos;
				((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
				((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = Quaternion.Euler(0f, 90f, -90f);
				SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.25f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * -0.5f);
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = _layOnBackPos;
				((Component)Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
				((Component)Variables.taggerInstance.offlineVRRig.head.rigTarget).transform.rotation = Quaternion.Euler(0f, 90f, -90f);
				SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * 0.25f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * -0.5f);
			}
		}
		else
		{
			wasRigDisabled = false;
			SetRigStatus(rigStatus: true);
			ResetHeadRotation();
		}
	}

	public static void ResetHeadRotation()
	{
		if (!((UnityEngine.Object)(object)Variables.taggerInstance?.offlineVRRig?.head?.rigTarget == (UnityEngine.Object)null) && cachedHeadRotation.HasValue)
		{
			Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = cachedHeadRotation.Value;
			cachedHeadRotation = null;
		}
	}

	public static void TPose()
	{
		if (!InputHandler.RTrigger())
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, 0.15f, 0f);
		SetRigRotation(0f, HeadY, 0f);
		SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -1.2f, ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 1.2f);
	}

	public static void WobblyMonke()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.RSecondary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (!InputHandler.isOn)
		{
			DisableWobblyMonke();
			return;
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
		_wobbleOffset += Time.deltaTime;
		((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0f, 0.15f + Mathf.Sin(_wobbleOffset * 3f) * 0.05f, 0f);
		SetRigRotation(Mathf.Sin(_wobbleOffset * 2.5f) * 15f, HeadY, Mathf.Cos(_wobbleOffset * 2f) * 15f);
		float num = Mathf.Sin(_wobbleOffset * 3f) * 0.15f;
		float num2 = Mathf.Cos(_wobbleOffset * 3f) * 0.15f;
		SetHands(((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * -0.5f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * (0.2f + num), ((Component)Variables.taggerInstance.offlineVRRig).transform.position + ((Component)Variables.taggerInstance.offlineVRRig).transform.right * 0.5f + ((Component)Variables.taggerInstance.offlineVRRig).transform.up * (0.2f + num2));
	}

	public static void SetHeadRotation(Vector3? headRotation = null, bool spin = false)
	{
		if (!cachedHeadRotation.HasValue)
		{
			cachedHeadRotation = Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles;
		}
		if (spin)
		{
			_headSpinOffset += 5f;
			Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = new Vector3(0f, _headSpinOffset, 0f);
			return;
		}
		_headSpinOffset = 0f;
		if (headRotation.HasValue)
		{
			Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = headRotation.Value;
		}
	}

	public static void OrbitAll()
	{
		MenuPatches.SerializationPatch.Override = delegate
		{
			PhotonView component = ((Component)Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				return false;
			}
			PhotonSerializer.BroadcastViews(exclude: true, (PhotonView[])(object)new PhotonView[1] { component });
			Vector3 position = ((Component)VRRig.LocalRig).transform.position;
			Quaternion rotation = ((Component)VRRig.LocalRig).transform.rotation;
			Vector3 position2 = VRRig.LocalRig.leftHand.rigTarget.position;
			Quaternion rotation2 = VRRig.LocalRig.leftHand.rigTarget.rotation;
			Vector3 position3 = VRRig.LocalRig.rightHand.rigTarget.position;
			Quaternion rotation3 = VRRig.LocalRig.rightHand.rigTarget.rotation;
			Quaternion rotation4 = ((Component)VRRig.LocalRig.head.rigTarget).transform.rotation;
			NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
			int num = 0;
			while (num < playerListOthers.Length)
			{
				NetPlayer val = playerListOthers[num];
				VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(val);
				if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
				{
					((Component)VRRig.LocalRig).transform.LookAt(((Component)vRRigFromNetPlayer).transform.position);
					((Component)VRRig.LocalRig).transform.RotateAround(((Component)vRRigFromNetPlayer).transform.position, Vector3.up, orbitSpeed * Time.deltaTime);
					Vector3 val2 = ((Component)VRRig.LocalRig).transform.position - ((Component)vRRigFromNetPlayer).transform.position;
					Vector3 position4 = val2.normalized * orbitDistance + ((Component)vRRigFromNetPlayer).transform.position;
					((Component)VRRig.LocalRig).transform.position = position4;
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.position = ((Component)VRRig.LocalRig).transform.position + ((Component)VRRig.LocalRig).transform.right * -1.5f;
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.position = ((Component)VRRig.LocalRig).transform.position + ((Component)VRRig.LocalRig).transform.right * 1.5f;
					((Component)VRRig.LocalRig.leftHand.rigTarget).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
					((Component)VRRig.LocalRig.rightHand.rigTarget).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
					RaiseEventOptions val3 = new RaiseEventOptions();
					val3.TargetActors = new int[1] { val.ActorNumber };
					PhotonSerializer.SendSerialize(component, val3);
					num++;
				}
				else
				{
					num++;
				}
			}
			Safety.RPCShield();
			((Component)VRRig.LocalRig).transform.position = position;
			((Component)VRRig.LocalRig).transform.rotation = rotation;
			VRRig.LocalRig.leftHand.rigTarget.position = position2;
			VRRig.LocalRig.leftHand.rigTarget.rotation = rotation2;
			VRRig.LocalRig.rightHand.rigTarget.position = position3;
			VRRig.LocalRig.rightHand.rigTarget.rotation = rotation3;
			((Component)VRRig.LocalRig.head.rigTarget).transform.rotation = rotation4;
			return false;
		};
	}

	public static void TeleportGun()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			if (GunLib.GunTriggers)
			{
				if (!hasTeleported && (UnityEngine.Object)(object)GunLib.raycastHit.collider != (UnityEngine.Object)null)
				{
					TeleportTo(GunLib.raycastHit.point + new Vector3(0f, 1f, -0.5f));
					Variables.taggerInstance.rigidbody.velocity = Vector3.zero;
					hasTeleported = true;
				}
			}
			else
			{
				hasTeleported = false;
			}
		}
		else
		{
			hasTeleported = false;
			GunLib.CancelGunUse();
		}
	}

	public static void NoFingerMovement()
	{
		VRRig offlineVRRig = Variables.taggerInstance.offlineVRRig;
		((VRMap)offlineVRRig.leftIndex).calcT = 0f;
		((VRMap)offlineVRRig.leftIndex).LerpFinger(1f, false);
		((VRMap)offlineVRRig.leftMiddle).calcT = 0f;
		((VRMap)offlineVRRig.leftMiddle).LerpFinger(1f, false);
		((VRMap)offlineVRRig.leftThumb).calcT = 0f;
		((VRMap)offlineVRRig.leftThumb).LerpFinger(1f, false);
		((VRMap)offlineVRRig.rightIndex).calcT = 0f;
		((VRMap)offlineVRRig.rightIndex).LerpFinger(1f, false);
		((VRMap)offlineVRRig.rightMiddle).calcT = 0f;
		((VRMap)offlineVRRig.rightMiddle).LerpFinger(1f, false);
		((VRMap)offlineVRRig.rightThumb).calcT = 0f;
		((VRMap)offlineVRRig.rightThumb).LerpFinger(1f, false);
	}
}
