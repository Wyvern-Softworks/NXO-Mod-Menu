using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaNetworking;
using NXO.Menu;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using StatusEffects = RoomSystem.StatusEffects;
using DropPositions = BodyDockPositions.DropPositions;
using PositionState = TransferrableObject.PositionState;

namespace NXO.Mods.Categories;

public static class Overpowered
{

	private static float _lagCooldown = 0f;

	private static float grabDelay = 0f;

	private static VRRig lockedGrabRig;

	private static float lockedGrabUntil;

	private static GameObject point;

	private static readonly Dictionary<VRRig, int> materialState = new Dictionary<VRRig, int>();

	public static float materialDelay;

	private static float slowDelay;

	private static float vibrateDelay;

	public static Coroutine disablebarrelCoroutine;

	private const int barrelIndex = 618;

	private static float lastToggleTime = 0f;

	public static float roomSoundDelay;

	public static bool _earrapeGunActive = false;

	public static string specificRoom;

	private static float kickDelay;

	private static bool _kickRoomSet;

	public static void SetRoomStatus(bool status)
	{
		if (PhotonNetwork.InRoom)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			ExitGames.Client.Photon.Hashtable val = new ExitGames.Client.Photon.Hashtable();
			val.Add((byte)253, (object)status);
			val.Add((byte)254, (object)status);
			val.Add(byte.MaxValue, (object)(status ? PhotonNetwork.CurrentRoom.MaxPlayers : 0));
			dictionary.Add(251, (object)val);
			dictionary.Add(250, true);
			dictionary.Add(231, null);
			Dictionary<byte, object> dictionary2 = dictionary;
			((PhotonPeer)PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer).SendOperation((byte)252, dictionary2, SendOptions.SendReliable);
			GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
		}
	}

	public static void MatSpamGun()
	{
		if (Variables.IsMaster() && GunLib.SetupLockOnGun() && Time.time > materialDelay)
		{
			materialDelay = Time.time + 0.1f;
			MaterialTarget(GunLib.lockedTargetRig);
		}
	}

	public static void BarrelFlingGun()
	{
		if (GunLib.SetupLockOnGun())
		{
			Vector3 position = ((Component)GunLib.lockedTargetRig).transform.position;
			Vector3 vel = new Vector3(0f, 8000f, 0f);
			Quaternion identity = Quaternion.identity;
			RaiseEventOptions val = new RaiseEventOptions();
			val.TargetActors = new int[1] { RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig).ActorNumber };
			SendBarrelProjectile(position, vel, identity, val);
		}
	}

	public static void SpazAllTargets()
	{
		if (Variables.IsMaster())
		{
			HitTargetNetworkState[] allType = Variables.GetAllType<HitTargetNetworkState>(false);
			foreach (HitTargetNetworkState val in allType)
			{
				val.hitCooldownTime = 0;
				val.TargetHit(Vector3.zero, Vector3.zero);
			}
		}
	}

	public static bool CanCallNow(this CallLimiter limiter, float? time = null)
	{
		if (limiter == null)
		{
			return false;
		}
		if (limiter.callTimeHistory == null || limiter.callHistoryLength <= 0)
		{
			return true;
		}
		float num2 = time ?? Time.time;
		int num3 = limiter.oldTimeIndex;
		if (num3 < 0)
		{
			num3 = 0;
		}
		else if (num3 >= limiter.callHistoryLength)
		{
			num3 = limiter.callHistoryLength - 1;
		}
		float num4 = limiter.callTimeHistory[num3];
		return num4 == float.MinValue || num4 <= num2;
	}

	public static void LagTarget(int[] targetActors)
	{
		if (!PhotonNetwork.InRoom || Time.time < _lagCooldown)
		{
			return;
		}
		_lagCooldown = Time.time + Settings.LagCooldown;
		RaiseEventOptions val = new RaiseEventOptions
		{
			TargetActors = targetActors,
			CachingOption = (EventCaching)2
		};
		SendOptions val2 = default(SendOptions);
		val2.Reliability = false;
		val2.DeliveryMode = (DeliveryMode)0;
		SendOptions val3 = val2;
		int num = 0;
		if (num < Settings.LagPackets)
		{
			do
			{
				PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent((byte)186, (object)new object[1] { float.NaN }, val, val3);
				num++;
			}
			while (num < Settings.LagPackets);
		}
		Safety.RPCShield();
	}

	public static void TagLag(bool enable)
	{
		if (Variables.IsMaster())
		{
			GorillaTagManager val = (GorillaTagManager)GorillaGameManager.instance;
			val.tagCoolDown = (enable ? float.MaxValue : 5f);
		}
	}

	public static void ForceGrabGun()
	{
		if (GunLib.SetupLockOnGun())
		{
			ForceGrabPlayer(GunLib.lockedTargetRig, ((Component)VRRig.LocalRig).transform.position);
		}
		else
		{
			((Behaviour)VRRig.LocalRig).enabled = true;
		}
	}

	public static void SpamEvent()
	{
		if (Variables.IsMaster() && Time.time - lastToggleTime >= 0.1f)
		{
			lastToggleTime = Time.time;
			if (((GreyZoneManager)GreyZoneManager.Instance).greyZoneActive)
			{
				((GreyZoneManager)GreyZoneManager.Instance).DeactivateGreyZoneAuthority();
			}
			else
			{
				((GreyZoneManager)GreyZoneManager.Instance).ActivateGreyZoneAuthority();
			}
		}
	}

	public static void DeafenAll()
	{
		int num = 0;
		if (num < 2)
		{
			do
			{
				DeafenPlayer((object)(ReceiverGroup)0);
				num++;
			}
			while (num < 2);
		}
	}

	public static void GetTouchedToMatSpam()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
		Transform[] targets = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.offlineVRRig.headMesh.transform,
			Variables.taggerInstance.offlineVRRig.bodyTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isMyPlayer)
			{
				continue;
			}
			if (RigManager.HandsTouchTargets((Transform[])(object)new Transform[2] { current.leftHandTransform, current.rightHandTransform }, targets))
			{
				if (Time.time > materialDelay)
				{
					materialDelay = Time.time + 0.1f;
					MaterialTarget(current);
				}
				return;
			}
		}
	}

	public static bool ForceGrabPlayer(VRRig rig, Vector3 targetPosition, bool returnOnGrab = false)
	{
		if ((UnityEngine.Object)(object)rig == (UnityEngine.Object)null || rig.isLocal)
		{
			return false;
		}
		if (!rig.leftHandLink.CanBeGrabbed() && !rig.rightHandLink.CanBeGrabbed())
		{
			return false;
		}
		bool useLeft = rig.leftHandLink.CanBeGrabbed();
		TakeMyHand_HandLink val = useLeft ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink;
		TakeMyHand_HandLink val2 = useLeft ? rig.leftHandLink : rig.rightHandLink;
		if (val2.grabbedPlayer == NetworkSystem.Instance.LocalPlayer)
		{
			return returnOnGrab;
		}
		if (grabDelay == 0f)
		{
			grabDelay = (val2.rejectGrabsUntilTimestamp > Time.time) ? val2.rejectGrabsUntilTimestamp : (Time.time + 1f);
			return false;
		}
		if (Time.time <= grabDelay)
		{
			return false;
		}
		((Behaviour)VRRig.LocalRig).enabled = false;
		((Component)VRRig.LocalRig).transform.position = rig.syncPos;
		val.TentacleTryCreateLink(val2);
		((Behaviour)VRRig.LocalRig).enabled = true;
		grabDelay = (val2.rejectGrabsUntilTimestamp > Time.time) ? val2.rejectGrabsUntilTimestamp : (Time.time + 1f);
		return false;
	}

	public static void FlingOnGrab()
	{
		if (IsBeingGrabbed())
		{
			((Behaviour)VRRig.LocalRig).enabled = false;
			Transform transform = ((Component)VRRig.LocalRig).transform;
			transform.position += Vector3.up * 2000f;
			((Behaviour)VRRig.LocalRig).enabled = true;
		}
	}

	public static void RoomSoundSpam(int id)
	{
		if (Variables.IsMaster() && InputHandler.RTrigger() && Time.time > roomSoundDelay)
		{
			object[] array = new object[3] { id, 99999f, false };
			object[] array2 = new object[3]
			{
				PhotonNetwork.ServerTimestamp,
				(byte)3,
				array
			};
			try
			{
				PhotonNetwork.RaiseEvent((byte)3, (object)array2, new RaiseEventOptions
				{
					Receivers = (ReceiverGroup)1
				}, SendOptions.SendUnreliable);
			}
			catch (Exception)
			{
			}
			Safety.RPCShield();
			roomSoundDelay = Time.time + 0.1f;
		}
	}

	public static void TouchToBarrelFling()
	{
		Transform[] hands = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.leftHandTransform,
			Variables.taggerInstance.rightHandTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal)
			{
				continue;
			}
			Transform[] targets = (Transform[])(object)new Transform[2]
			{
				current.headMesh.transform,
				current.bodyTransform
			};
			if (RigManager.HandsTouchTargets(hands, targets))
			{
				Vector3 position = ((Component)current).transform.position;
				Vector3 vel = new Vector3(0f, 8000f, 0f);
				Quaternion identity = Quaternion.identity;
				RaiseEventOptions val = new RaiseEventOptions();
				val.TargetActors = new int[1] { RigManager.GetNetPlayerFromVRRig(current).ActorNumber };
				SendBarrelProjectile(position, vel, identity, val);
				return;
			}
		}
	}

	public static void MaterialTarget(VRRig rig)
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You're Not Master Client");
			return;
		}
		NetPlayer netPlayerFromVRRig = RigManager.GetNetPlayerFromVRRig(rig);
		materialState.TryGetValue(rig, out var value);
		int num = value + 1;
		value = num;
		num = value % 4;
		value = num;
		if ((int)GorillaGameManager.instance.GameType() == 0)
		{
			value = 4;
			materialState[rig] = value;
			AddInfected(netPlayerFromVRRig);
			return;
		}
		materialState[rig] = value;
		if (value == 1)
		{
			RemoveInfected(netPlayerFromVRRig);
			return;
		}
		AddInfected(netPlayerFromVRRig);
	}

	public static void SetKickRoom()
	{
		if (SearchAndKeyboard.isSearching || _kickRoomSet)
		{
			return;
		}
		SearchAndKeyboard.OpenTypingKeyboard(specificRoom ?? "", "Enter room code...");
		SearchAndKeyboard.onTypingComplete = delegate(string code)
		{
			_kickRoomSet = true;
			ButtonHandler.Button button = ModButtons.buttons.FirstOrDefault((ButtonHandler.Button b) => b != null && b.buttonText?.StartsWith("Kick To Specific Room") == true);
			if (string.IsNullOrEmpty(code))
			{
				specificRoom = null;
				if (button != null)
				{
					button.SetText("Kick To Specific Room");
					NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, "Kick Photon.Realtime.Room Cleared");
				}
				else
				{
					NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, "Kick Photon.Realtime.Room Cleared");
				}
			}
			else
			{
				specificRoom = code;
				if (button != null)
				{
					button.SetText("Kick To Specific Room : " + code);
					NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Kick Photon.Realtime.Room Set To `" + code + "`");
				}
				else
				{
					NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Kick Photon.Realtime.Room Set To `" + code + "`");
				}
			}
		};
		SearchAndKeyboard.onTypingCancelled = delegate
		{
			ButtonHandler.Button button = ModButtons.buttons.FirstOrDefault((ButtonHandler.Button b) => b != null && b.buttonText?.StartsWith("Kick To Specific Room") == true);
			if (button != null)
			{
				button.Enabled = false;
				specificRoom = null;
				_kickRoomSet = false;
				if (button == null)
				{
					return;
				}
			}
			else
			{
				specificRoom = null;
				_kickRoomSet = false;
				if (button == null)
				{
					return;
				}
			}
			button.SetText("Kick To Specific Room");
		};
	}

	public static PhotonView GetPhotonView(this VRRig rig)
	{
		return rig.netView.GetView;
	}

	public static void FlingTowardsPointOnGrab(bool enable)
	{
		if (enable)
		{
			if (GunLib.SetupGun())
			{
				if ((UnityEngine.Object)(object)point == (UnityEngine.Object)null)
				{
					point = GameObject.CreatePrimitive((PrimitiveType)0);
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)point.GetComponent<Collider>());
					point.transform.localScale = Vector3.one * 0.25f;
					point.GetComponent<Renderer>().material.color = Color.blue;
					point.transform.position = GunLib.raycastHit.point + Vector3.up * 0.25f;
					if (!IsBeingGrabbed())
					{
						return;
					}
				}
				else
				{
					point.transform.position = GunLib.raycastHit.point + Vector3.up * 0.25f;
					if (!IsBeingGrabbed())
					{
						return;
					}
				}
			}
			else if (!IsBeingGrabbed())
			{
				return;
			}
			if (!((UnityEngine.Object)(object)point == (UnityEngine.Object)null))
			{
				((Behaviour)VRRig.LocalRig).enabled = false;
				Transform transform = ((Component)VRRig.LocalRig).transform;
				Vector3 position = transform.position;
				Vector3 val = point.transform.position - ((Component)VRRig.LocalRig).transform.position;
				transform.position = position + val.normalized * 2000f;
				((Behaviour)VRRig.LocalRig).enabled = true;
			}
		}
		else if ((UnityEngine.Object)(object)point != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)point);
			point = null;
		}
	}

	public static void AddRock(NetPlayer plr)
	{
		if (!PhotonNetwork.InRoom || (UnityEngine.Object)(object)GorillaGameManager.instance == (UnityEngine.Object)null || plr == null)
		{
			return;
		}
		GameModeType val = GorillaGameManager.instance.GameType();
		int num = (int)(val - 1);
		num = num - (num - 11) * (((uint)num > 10u) ? 1 : 0) + 363;
		int num2 = num;
		if (num2 != 364)
		{
			GorillaGameManager instance = GorillaGameManager.instance;
			GorillaTagManager val2 = (GorillaTagManager)(object)((instance is GorillaTagManager) ? instance : null);
			if (val2 != null)
			{
				val2.ChangeCurrentIt(plr, true);
			}
		}
	}

	public static void Serialize(this PhotonView view, RaiseEventOptions options = null)
	{
		PhotonSerializer.SendSerialize(view, options);
	}

	public static void RemoveRock(NetPlayer plr)
	{
		if (!PhotonNetwork.InRoom || (UnityEngine.Object)(object)GorillaGameManager.instance == (UnityEngine.Object)null || plr == null)
		{
			return;
		}
		GameModeType val = GorillaGameManager.instance.GameType();
		int num = (int)(val - 1);
		num = num - (num - 11) * (((uint)num > 10u) ? 1 : 0) + 398;
		int num2 = num;
		if (num2 != 399)
		{
			GorillaGameManager instance = GorillaGameManager.instance;
			GorillaTagManager val2 = (GorillaTagManager)(object)((instance is GorillaTagManager) ? instance : null);
			if (val2 != null && val2.currentIt == plr)
			{
				val2.ChangeCurrentIt((NetPlayer)null, true);
			}
		}
	}

	public static void DestroyAll()
	{
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal)
			{
				PhotonNetwork.OpRemoveCompleteCacheOfPlayer(current.Creator.ActorNumber);
			}
		}
	}

	public static void StumpKickGun()
	{
		if (!GunLib.GunGrips)
		{
			GunLib.SetGunVisibility(isVisible: false);
			GunLib.lockedTargetRig = null;
			return;
		}
		GunLib.SetupRaycast();
		Collider collider = GunLib.raycastHit.collider;
		GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
		if (GunLib.GunTriggers && Time.time > kickDelay)
		{
			if ((UnityEngine.Object)(object)GunLib.lockedTargetRig == (UnityEngine.Object)null)
			{
				GunLib.lockedTargetRig = GunLib.potentialTargetRig;
				return;
			}
			NetPlayer player = RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig);
			kickDelay = Time.time + 0.5f;
			if (!((GorillaComputer)GorillaComputer.instance).friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId))
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Photon.Realtime.Player Must Be In Stump");
				return;
			}
			if (!NetworkSystem.Instance.SessionIsPrivate)
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You Must Be In A Private Room");
				return;
			}
			((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(StumpKickDelay(delegate
			{
				PhotonNetworkController instance = PhotonNetworkController.Instance;
				int num = UnityEngine.Random.Range(0, 99);
				int num2 = num;
				string text = num2.ToString().PadLeft(2, '0');
				num = UnityEngine.Random.Range(0, 99999999);
				num2 = num;
				((PhotonNetworkController)instance).shuffler = text + num2.ToString().PadLeft(8, '0');
				PhotonNetworkController instance2 = PhotonNetworkController.Instance;
				num = UnityEngine.Random.Range(0, 99999999);
				num2 = num;
				((PhotonNetworkController)instance2).keyStr = num2.ToString().PadLeft(8, '0');
				BetaNearbyFollowCommand(((GorillaComputer)GorillaComputer.instance).friendJoinCollider, RigManager.NetPlayerToPhotonPlayer(player));
				Safety.RPCShield();
			}, delegate
			{
				Room.CreateRoom(specificRoom ?? Room.RandomString(), isPublic: false, 0, (GorillaNetworking.JoinType)1);
			}));
		}
		else if (!GunLib.GunTriggers)
		{
			GunLib.lockedTargetRig = null;
		}
	}

	public static void GetTouchedToDeafen()
	{
		Transform[] targets = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.offlineVRRig.headMesh.transform,
			Variables.taggerInstance.offlineVRRig.bodyTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isMyPlayer)
			{
				continue;
			}
			Transform[] hands = (Transform[])(object)new Transform[2] { current.leftHandTransform, current.rightHandTransform };
			if (RigManager.HandsTouchTargets(hands, targets))
			{
				DeafenPlayer(new int[1] { current.Creator.ActorNumber });
				return;
			}
		}
	}

	public static void EarrapeAll()
	{
		Sound.Earrape(enable: true);
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

	public static void VibrateGun()
	{
		if (Variables.IsMaster() && GunLib.SetupLockOnGun() && Time.time > vibrateDelay)
		{
			vibrateDelay = Time.time + 0.5f;
			RaiseEventOptions val = new RaiseEventOptions();
			val.TargetActors = new int[1] { GunLib.lockedTargetRig.Creator.ActorNumber };
			SendStatusEffect((StatusEffects)1, val);
		}
	}

	public static void DeafenGun()
	{
		if (!GunLib.SetupLockOnGun())
		{
			return;
		}
		int num = 0;
		if (num < 2)
		{
			do
			{
				DeafenPlayer(new int[1] { GunLib.lockedTargetRig.Creator.ActorNumber });
				num++;
			}
			while (num < 2);
		}
	}

	public static void MatSpamAll()
	{
		if (!Variables.IsMaster() || !(Time.time > materialDelay))
		{
			return;
		}
		materialDelay = Time.time + 0.1f;
		IEnumerator<VRRig> enumerator = VRRigCache.ActiveRigs.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				VRRig current = enumerator.Current;
				MaterialTarget(current);
			}
			while (enumerator.MoveNext());
		}
	}

	public static void GetTouchedToBarrelFling()
	{
		Transform[] targets = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.offlineVRRig.headMesh.transform,
			Variables.taggerInstance.offlineVRRig.bodyTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isMyPlayer)
			{
				continue;
			}
			Transform[] hands = (Transform[])(object)new Transform[2] { current.leftHandTransform, current.rightHandTransform };
			if (RigManager.HandsTouchTargets(hands, targets))
			{
				Vector3 position = ((Component)current).transform.position;
				Vector3 vel = new Vector3(0f, 8000f, 0f);
				Quaternion identity = Quaternion.identity;
				RaiseEventOptions val = new RaiseEventOptions();
				val.TargetActors = new int[1] { RigManager.GetNetPlayerFromVRRig(current).ActorNumber };
				SendBarrelProjectile(position, vel, identity, val);
				return;
			}
		}
	}

	public static void LagGun()
	{
		if (GunLib.SetupLockOnGun())
		{
			LagTarget(new int[1] { GunLib.lockedTargetRig.Creator.ActorNumber });
		}
	}

	public static void DeafenPlayer(object player)
	{
		RaiseEventOptions val = new RaiseEventOptions();
		if (player is ReceiverGroup receivers)
		{
			val.Receivers = receivers;
		}
		else
		{
			if (!(player is int[] targetActors))
			{
				return;
			}
			val.TargetActors = targetActors;
		}
		SendOptions val2 = default(SendOptions);
		val2.Reliability = true;
		val2.Channel = 0;
		SendOptions val3 = val2;
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>
		{
			{ 1, 255 },
			{ 2, 48000 },
			{ 3, 2 },
			{ 4, 20000 },
			{ 5, 30000 },
			{ 10, null },
			{
				11,
				(byte)0
			},
			{
				12,
				(byte)11
			}
		};
		object[] array = new object[3]
		{
			(byte)0,
			(byte)1,
			new object[1] { dictionary }
		};
		((LoadBalancingClient)((VoiceConnection)PhotonVoiceNetwork.Instance).Client).OpRaiseEvent((byte)202, (object)array, val, val3);
	}

	public static void TouchToLag()
	{
		Transform[] hands = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.leftHandTransform,
			Variables.taggerInstance.rightHandTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || (UnityEngine.Object)(object)current == (UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig)
			{
				continue;
			}
			Transform[] targets = (Transform[])(object)new Transform[2]
			{
				current.headMesh.transform,
				current.bodyTransform
			};
			if (RigManager.HandsTouchTargets(hands, targets))
			{
				LagTarget(new int[1] { current.Creator.ActorNumber });
				return;
			}
		}
	}

	public static IEnumerator DisableSendBarrel()
	{
		yield return (object)new WaitForSeconds(0.3f);
		MenuPatches.bypassPositionCheck = false;
		((Behaviour)VRRig.LocalRig).enabled = true;
		TransferrableObject bar = VRRig.LocalRig.myBodyDockPositions.allObjects[618];
		((Component)bar).gameObject.SetActive(true);
		bar.storedZone = (DropPositions)2;
		bar.currentState = (PositionState)2;
	}

	public static void EarrapeGun()
	{
		if (!GunLib.SetupLockOnGun())
		{
			return;
		}
		if (!_earrapeGunActive)
		{
			Sound.Earrape(enable: true);
			_earrapeGunActive = true;
		}
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal || current.Creator.ActorNumber == GunLib.lockedTargetRig.Creator.ActorNumber)
			{
				continue;
			}
			DeafenPlayer(new int[1] { current.Creator.ActorNumber });
		}
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
			((Component)VRRig.LocalRig).transform.SetPositionAndRotation(((Component)GunLib.lockedTargetRig).transform.position, ((Component)GunLib.lockedTargetRig).transform.rotation);
			VRRig.LocalRig.head.rigTarget.SetPositionAndRotation(GunLib.lockedTargetRig.head.rigTarget.position, GunLib.lockedTargetRig.head.rigTarget.rotation);
			VRRig.LocalRig.leftHand.rigTarget.SetPositionAndRotation(GunLib.lockedTargetRig.leftHand.rigTarget.position, GunLib.lockedTargetRig.leftHand.rigTarget.rotation);
			VRRig.LocalRig.rightHand.rigTarget.SetPositionAndRotation(GunLib.lockedTargetRig.rightHand.rigTarget.position, GunLib.lockedTargetRig.rightHand.rigTarget.rotation);
			RaiseEventOptions val = new RaiseEventOptions();
			val.TargetActors = new int[1] { GunLib.lockedTargetRig.Creator.ActorNumber };
			PhotonSerializer.SendSerialize(component, val);
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

	public static void SlowGun()
	{
		if (Variables.IsMaster() && GunLib.SetupLockOnGun() && Time.time > slowDelay)
		{
			RaiseEventOptions val = new RaiseEventOptions();
			val.TargetActors = new int[1] { GunLib.lockedTargetRig.Creator.ActorNumber };
			SendStatusEffect((StatusEffects)0, val);
			slowDelay = Time.time + 1f;
		}
	}

	public static IEnumerator StumpKickDelay(Action action, Action action2, float extraDelay = 0f, bool changeQueue = false)
	{
		((PhotonNetworkController)PhotonNetworkController.Instance).FriendIDList.Clear();
		if (extraDelay > 0f)
		{
			yield return (object)new WaitForSeconds(extraDelay);
		}
		bool joinedRoomPatchEnabled = MenuPatches.JoinedRoomPatch.enabled;
		string queueArchive = ((GorillaComputer)GorillaComputer.instance).currentQueue;
		try
		{
			if (changeQueue)
			{
				((GorillaComputer)GorillaComputer.instance).currentQueue = Room.RandomString();
			}
			action?.Invoke();
			yield return (object)new WaitForSeconds(0.3f);
			action2?.Invoke();
		}
		finally
		{
			MenuPatches.JoinedRoomPatch.enabled = joinedRoomPatchEnabled;
			if (changeQueue)
			{
				((GorillaComputer)GorillaComputer.instance).currentQueue = queueArchive;
			}
		}
	}

	public static void SlowAll()
	{
		if (Variables.IsMaster() && InputHandler.RTrigger() && Time.time > slowDelay)
		{
			SendStatusEffect((StatusEffects)0, new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)0
			});
			slowDelay = Time.time + 1f;
		}
	}

	public static void LagAll()
	{
		if (!InputHandler.RTrigger())
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal)
			{
				continue;
			}
			list.Add(current.Creator.ActorNumber);
		}
		if (list.Count > 0)
		{
			LagTarget(list.ToArray());
		}
	}

	private static bool IsBeingGrabbed()
	{
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isLocal)
			{
				continue;
			}
			if (VRRig.LocalRig.leftHandLink.IsLinkActive() || VRRig.LocalRig.rightHandLink.IsLinkActive())
			{
				return true;
			}
		}
		return false;
	}

	public static void ClearKickRoom()
	{
		specificRoom = null;
		_kickRoomSet = false;
		if (SearchAndKeyboard.isSearching)
		{
			SearchAndKeyboard.CloseTypingKeyboard(cancelled: true);
		}
		ButtonHandler.Button button = ModButtons.buttons.FirstOrDefault((ButtonHandler.Button b) => b != null && b.buttonText?.StartsWith("Kick To Specific Room") == true);
		if (button != null)
		{
			button.SetText("Kick To Specific Room");
		}
		NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, "Kick Photon.Realtime.Room Cleared");
	}

	public static void VibrateAura()
	{
		if (!Variables.IsMaster() || !PhotonNetwork.InRoom || Time.time < vibrateDelay)
		{
			return;
		}
		Vector3 position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position;
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal)
			{
				continue;
			}
			if (Vector3.Distance(position, ((Component)current).transform.position) < 4f)
			{
				vibrateDelay = Time.time + 0.5f;
				RaiseEventOptions val = new RaiseEventOptions();
				val.TargetActors = new int[1] { current.Creator.ActorNumber };
				SendStatusEffect((StatusEffects)1, val);
				return;
			}
		}
	}

	public static void SlowAura()
	{
		if (!Variables.IsMaster() || !PhotonNetwork.InRoom || Time.time < slowDelay)
		{
			return;
		}
		Vector3 position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position;
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal)
			{
				continue;
			}
			if (Vector3.Distance(position, ((Component)current).transform.position) < 4f)
			{
				slowDelay = Time.time + 1f;
				RaiseEventOptions val = new RaiseEventOptions();
				val.TargetActors = new int[1] { current.Creator.ActorNumber };
				SendStatusEffect((StatusEffects)0, val);
				return;
			}
		}
	}

	public static void SlowOnTouch()
	{
		if (!Variables.IsMaster() || !PhotonNetwork.InRoom || Time.time < slowDelay)
		{
			return;
		}
		Transform[] hands = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.leftHandTransform,
			Variables.taggerInstance.rightHandTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal)
			{
				continue;
			}
			if (RigManager.HandsTouchTargets(hands, (Transform[])(object)new Transform[2]
			{
				current.headMesh.transform,
				current.bodyTransform
			}))
			{
				slowDelay = Time.time + 1f;
				RaiseEventOptions val = new RaiseEventOptions();
				val.TargetActors = new int[1] { current.Creator.ActorNumber };
				SendStatusEffect((StatusEffects)0, val);
				return;
			}
		}
	}

	public static void GetTouchedToLag()
	{
		Transform[] targets = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.offlineVRRig.headMesh.transform,
			Variables.taggerInstance.offlineVRRig.bodyTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isMyPlayer)
			{
				continue;
			}
			Transform[] hands = (Transform[])(object)new Transform[2] { current.leftHandTransform, current.rightHandTransform };
			if (RigManager.HandsTouchTargets(hands, targets))
			{
				LagTarget(new int[1] { current.Creator.ActorNumber });
				return;
			}
		}
	}

	public static void DestroyGun()
	{
		if (GunLib.SetupLockOnGun())
		{
			PhotonNetwork.OpRemoveCompleteCacheOfPlayer(GunLib.lockedTargetRig.Creator.ActorNumber);
		}
	}

	private static void SendStatusEffect(RoomSystem.StatusEffects statusEffect, RaiseEventOptions options)
	{
		if (Variables.IsMaster())
		{
			object[] array = new object[1] { (int)statusEffect };
			PhotonNetwork.RaiseEvent((byte)3, (object)new object[3]
			{
				NetworkSystem.Instance.ServerTimestamp,
				(byte)2,
				array
			}, options, SendOptions.SendUnreliable);
			Safety.RPCShield();
		}
	}

	public static void RemoveInfected(NetPlayer plr)
	{
		if (!PhotonNetwork.InRoom || (UnityEngine.Object)(object)GorillaGameManager.instance == (UnityEngine.Object)null || plr == null)
		{
			return;
		}
		GameModeType gameType = GorillaGameManager.instance.GameType();
		if ((int)(gameType - 1) == 1)
		{
			return;
		}
		GorillaGameManager instance = GorillaGameManager.instance;
		GorillaTagManager val2 = (GorillaTagManager)(object)((instance is GorillaTagManager) ? instance : null);
		if (val2 == null)
		{
			return;
		}
		if (val2.isCurrentlyTag)
		{
			if (val2.currentIt == plr)
			{
				val2.currentIt = null;
			}
		}
		else if (val2.currentInfected != null && val2.currentInfected.Contains(plr))
		{
			val2.currentInfected.Remove(plr);
		}
	}

	public static void MetroMapCrashOnGrab()
	{
		if (IsBeingGrabbed())
		{
			((Behaviour)VRRig.LocalRig).enabled = false;
			Vector3 val = new Vector3(137.0344f, 66.6208f, -42.8385f) - ((Component)VRRig.LocalRig).transform.position;
			Vector3 normalized = val.normalized;
			Transform transform = ((Component)VRRig.LocalRig).transform;
			transform.position += Vector3.up * 200f + normalized * 400f;
			((Behaviour)VRRig.LocalRig).enabled = true;
		}
	}

	public static void CrashModdersOnGrab()
	{
		if (IsBeingGrabbed())
		{
			((Behaviour)VRRig.LocalRig).enabled = false;
			Transform transform = ((Component)VRRig.LocalRig).transform;
			transform.position += Vector3.down * 2000f;
			((Behaviour)VRRig.LocalRig).enabled = true;
		}
	}

	public static void SendBarrelProjectile(Vector3 pos, Vector3 vel, Quaternion rot, RaiseEventOptions options = null)
	{
		if (options == null)
		{
			options = new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)1
			};
		}
		MenuPatches.bypassPositionCheck = true;
		if (disablebarrelCoroutine != null)
		{
			((MonoBehaviour)CoroutineHelper.Instance).StopCoroutine(disablebarrelCoroutine);
		}
		disablebarrelCoroutine = ((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DisableSendBarrel());
		TransferrableObject val2 = VRRig.LocalRig.myBodyDockPositions.allObjects[618];
		if (!((Component)val2).gameObject.activeSelf)
		{
			VRRig.LocalRig.SetActiveTransferrableObjectIndex(1, 618);
			((Component)val2).gameObject.SetActive(true);
		}
		val2.storedZone = (DropPositions)2;
		val2.currentState = (PositionState)8;
		if (!((Component)val2).gameObject.activeSelf)
		{
			return;
		}
		DeployableObject component = ((Component)val2).GetComponent<DeployableObject>();
		if (component.m_spamChecker.CanCallNow())
		{
			object[] array = new object[5]
			{
				((PhotonSignal)component._deploySignal)._signalID,
				PhotonNetwork.ServerTimestamp,
				BitPackUtils.PackWorldPosForNetwork(pos),
				BitPackUtils.PackQuaternionForNetwork(rot),
				BitPackUtils.PackWorldPosForNetwork(vel)
			};
			((Component)VRRig.LocalRig).transform.position = pos;
			VRRig.LocalRig.GetPhotonView().Serialize();
			RaiseEventOptions val3 = options;
			SendOptions val4 = default(SendOptions);
			val4.Reliability = false;
			val4.DeliveryMode = (DeliveryMode)3;
			PhotonNetwork.RaiseEvent((byte)177, (object)array, (RaiseEventOptions)val3, val4);
			component._child.Deploy(component, pos, rot, vel, false);
			component.DeployChild();
			Safety.RPCShield();
		}
	}

	public static void BetaNearbyFollowCommand(GorillaFriendCollider friendCollider, Photon.Realtime.Player player)
	{
		((PhotonNetworkController)PhotonNetworkController.Instance).FriendIDList.Add(player.UserId);
		object[] array = new object[2]
		{
			((PhotonNetworkController)PhotonNetworkController.Instance).shuffler,
			((PhotonNetworkController)PhotonNetworkController.Instance).keyStr
		};
		NetEventOptions val = new NetEventOptions();
		val.TargetActors = new int[1] { player.ActorNumber };
		NetEventOptions val2 = val;
		if (friendCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && friendCollider.playerIDsCurrentlyTouching.Contains(player.UserId) && player != PhotonNetwork.LocalPlayer)
		{
			RoomSystem.SendEvent((byte)4, array, ref val2, false);
		}
		else if (!friendCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You Must Be In Stump");
		}
	}

	public static void VibrateOnTouch()
	{
		if (!Variables.IsMaster() || !PhotonNetwork.InRoom || Time.time < vibrateDelay)
		{
			return;
		}
		Transform[] hands = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.leftHandTransform,
			Variables.taggerInstance.rightHandTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal)
			{
				continue;
			}
			if (RigManager.HandsTouchTargets(hands, (Transform[])(object)new Transform[2]
			{
				current.headMesh.transform,
				current.bodyTransform
			}))
			{
				vibrateDelay = Time.time + 0.5f;
				RaiseEventOptions val = new RaiseEventOptions();
				val.TargetActors = new int[1] { current.Creator.ActorNumber };
				SendStatusEffect((StatusEffects)1, val);
				return;
			}
		}
	}

	public static void ForceGrab()
	{
		if (!InputHandler.RGrip())
		{
			lockedGrabRig = null;
			lockedGrabUntil = 0f;
			return;
		}
		if ((UnityEngine.Object)(object)lockedGrabRig == (UnityEngine.Object)null || Time.time > lockedGrabUntil)
		{
			lockedGrabRig = null;
			IEnumerator<VRRig> enumerator = VRRigCache.ActiveRigs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				VRRig current = enumerator.Current;
				if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isLocal || current.isOfflineVRRig || (!current.leftHandLink.CanBeGrabbed() && !current.rightHandLink.CanBeGrabbed()))
				{
					continue;
				}
				lockedGrabRig = current;
				lockedGrabUntil = Time.time + 5f;
				break;
			}
			if (!((UnityEngine.Object)(object)lockedGrabRig != (UnityEngine.Object)null))
			{
				return;
			}
		}
		else if (!((UnityEngine.Object)(object)lockedGrabRig != (UnityEngine.Object)null))
		{
			return;
		}
		ForceGrabPlayer(lockedGrabRig, ((Component)VRRig.LocalRig).transform.position);
	}

	public static void VibrateAll()
	{
		if (Variables.IsMaster() && InputHandler.RTrigger() && Time.time > vibrateDelay)
		{
			SendStatusEffect((StatusEffects)1, new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)0
			});
			vibrateDelay = Time.time + 0.5f;
		}
	}

	public static void AddInfected(NetPlayer plr)
	{
		if (!PhotonNetwork.InRoom || (UnityEngine.Object)(object)GorillaGameManager.instance == (UnityEngine.Object)null || plr == null)
		{
			return;
		}
		GameModeType val = GorillaGameManager.instance.GameType();
		int num = (int)(val - 1);
		num = num - (num - 11) * (((uint)num > 10u) ? 1 : 0) + 283;
		int num2 = num;
		if (num2 == 284)
		{
			return;
		}
		GorillaGameManager instance = GorillaGameManager.instance;
		GorillaTagManager val2 = (GorillaTagManager)(object)((instance is GorillaTagManager) ? instance : null);
		if (val2 != null)
		{
			if (val2.isCurrentlyTag)
			{
				val2.ChangeCurrentIt(plr, true);
			}
			else if (val2.currentInfected != null && !val2.currentInfected.Contains(plr))
			{
				val2.AddInfectedPlayer(plr, true);
			}
		}
	}

	public static void LagAura()
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		List<int> list = new List<int>();
		Vector3 position = ((Component)Variables.taggerInstance.offlineVRRig).transform.position;
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal)
			{
				continue;
			}
			if (Vector3.Distance(position, ((Component)current).transform.position) < 4f)
			{
				list.Add(current.Creator.ActorNumber);
			}
		}
		if (list.Count > 0)
		{
			LagTarget(list.ToArray());
		}
	}

	public static void StumpKickAll()
	{
		if (PhotonNetwork.InRoom)
		{
			if (!NetworkSystem.Instance.SessionIsPrivate)
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You Must Be In A Private Room");
				return;
			}
			((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(StumpKickDelay(delegate
			{
				PhotonNetworkController instance = PhotonNetworkController.Instance;
				int num = UnityEngine.Random.Range(0, 99);
				int num2 = num;
				string text = num2.ToString().PadLeft(2, '0');
				num = UnityEngine.Random.Range(0, 99999999);
				num2 = num;
				((PhotonNetworkController)instance).shuffler = text + num2.ToString().PadLeft(8, '0');
				PhotonNetworkController instance2 = PhotonNetworkController.Instance;
				num = UnityEngine.Random.Range(0, 99999999);
				num2 = num;
				((PhotonNetworkController)instance2).keyStr = num2.ToString().PadLeft(8, '0');
				IEnumerator<VRRig> enumerator = VRRigCache.ActiveRigs.Where((VRRig rig) => !rig.IsRigLocal() && ((GorillaComputer)GorillaComputer.instance).friendJoinCollider.playerIDsCurrentlyTouching.Contains(rig.netView.Owner.UserId)).GetEnumerator();
				if (enumerator.MoveNext())
				{
					do
					{
						VRRig current = enumerator.Current;
						BetaNearbyFollowCommand(((GorillaComputer)GorillaComputer.instance).friendJoinCollider, RigManager.NetPlayerToPhotonPlayer(RigManager.GetNetPlayerFromVRRig(current)));
					}
					while (enumerator.MoveNext());
				}
				Safety.RPCShield();
			}, delegate
			{
				Room.CreateRoom(specificRoom ?? Room.RandomString(), isPublic: false, 0, (GorillaNetworking.JoinType)1);
			}));
		}
		else
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Not In A Room");
		}
	}

	public static void TouchToDeafen()
	{
		Transform[] hands = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.leftHandTransform,
			Variables.taggerInstance.rightHandTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || (UnityEngine.Object)(object)current == (UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig)
			{
				continue;
			}
			Transform[] targets = (Transform[])(object)new Transform[2]
			{
				current.headMesh.transform,
				current.bodyTransform
			};
			if (RigManager.HandsTouchTargets(hands, targets))
			{
				DeafenPlayer(new int[1] { current.Creator.ActorNumber });
				return;
			}
		}
	}

	public static void TouchToMatSpam()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
		Transform[] hands = (Transform[])(object)new Transform[2]
		{
			Variables.taggerInstance.leftHandTransform,
			Variables.taggerInstance.rightHandTransform
		};
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || current.isOfflineVRRig || current.isLocal)
			{
				continue;
			}
			if (RigManager.HandsTouchTargets(hands, (Transform[])(object)new Transform[2]
			{
				current.headMesh.transform,
				current.bodyTransform
			}))
			{
				if (Time.time > materialDelay)
				{
					materialDelay = Time.time + 0.1f;
					MaterialTarget(current);
				}
				return;
			}
		}
	}
}
