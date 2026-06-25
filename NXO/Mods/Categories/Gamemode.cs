using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaGameModes;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NXO.Mods.Categories;

public class Gamemode
{
	public static float autoGuardianDelay;

	private static float guardianGrabDelay;

	private static float guardianReleaseDelay;

	public static float balloonSpamDelay;

	public static int paintbrawlKillIndex;

	public static readonly Dictionary<int, float> paintbrawlKillDelays = new Dictionary<int, float>();

	public static void AntiTag()
	{
		if (RigManager.RigIsInfected(VRRig.LocalRig))
		{
			return;
		}
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if (RigManager.RigIsInfected(current))
			{
				if (Vector3.Distance(((Component)Variables.taggerInstance.offlineVRRig).transform.position, ((Component)current).transform.position) < 3f)
				{
					((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
					((Component)Variables.taggerInstance.offlineVRRig).transform.position = new Vector3(999f, 999f, 999f);
				}
				else
				{
					((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
				}
			}
		}
	}

	public static void FlickTag()
	{
		if (!Mouse.current.rightButton.isPressed && !Mouse.current.leftButton.isPressed)
		{
			Ray val = new Ray(Variables.playerInstance.RightHand.controllerTransform.position, Variables.playerInstance.RightHand.controllerTransform.forward);
			Physics.Raycast(val, out GunLib.raycastHit, 100f);
			GunLib.SetupGunObjectPositions(GunLib.raycastHit.point);
			Collider collider = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider != null) ? ((Component)collider).GetComponentInParent<VRRig>() : null);
			if (!InputHandler.RTrigger())
			{
				return;
			}
		}
		else
		{
			Ray val = Variables.thirdPersonCamera.GetComponent<Camera>().ScreenPointToRay((Vector2)(((InputControl<Vector2>)(object)((Pointer)Mouse.current).position).ReadValue()));
			Physics.Raycast(val, out GunLib.raycastHit, 100f);
			GunLib.SetupGunObjectPositions(GunLib.raycastHit.point);
			Collider collider2 = GunLib.raycastHit.collider;
			GunLib.potentialTargetRig = ((collider2 != null) ? ((Component)collider2).GetComponentInParent<VRRig>() : null);
			if (!InputHandler.RTrigger())
			{
				return;
			}
		}
		if (RigManager.RigIsInfected(VRRig.LocalRig))
		{
			Variables.playerInstance.RightHand.controllerTransform.position = GunLib.raycastHit.point;
			PhotonSerializer.ForceSerialization();
			PunExtensions.GetPhotonView(GameObject.Find("Photon.Realtime.Player Objects/RigCache/Network Parent/GameMode(Clone)")).RPC("RPC_ReportTag", (RpcTarget)0, new object[1] { GunLib.potentialTargetRig.Creator.ActorNumber });
			PhotonNetwork.SendAllOutgoingCommands();
			((PhotonPeer)PhotonNetwork.NetworkingClient.LoadBalancingPeer).SendAcksOnly();
		}
	}

	public static void UntagSelf()
	{
		if (!PhotonNetwork.InRoom)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Not In A Room");
			return;
		}
		if (!RigManager.RigIsInfected(VRRig.LocalRig))
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You're Not Tagged");
			return;
		}
		Room.Reconnect();
		NoTagOnJoin(setActive: true);
	}

	public static void UntagAll()
	{
		if (Variables.IsMaster())
		{
			Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
			foreach (Photon.Realtime.Player val in playerList)
			{
				RemoveInfected(((NetPlayer)(val)));
			}
		}
	}

	public static void PaintbrawlReviveGun()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
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
					PaintbrawlRevivePlayer(GunLib.lockedTargetRig.Creator.ActorNumber);
				}
			}
			else
			{
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			GunLib.lockedTargetRig = null;
			GunLib.SetGunVisibility(isVisible: false);
		}
	}

	public static void GuardianFlingAll()
	{
		if (InputHandler.RTrigger() && !(Time.time <= guardianReleaseDelay))
		{
			guardianReleaseDelay = Time.time + 0.1f;
			SetAllPlayersVelocity((RpcTarget)1, new Vector3(0f, 100000000f, 0f));
			Safety.RPCShield();
		}
	}

	public static void UnGuardianGun()
	{
		if (!Variables.IsMaster() || !GunLib.SetupLockOnGun())
		{
			return;
		}
		IEnumerator<GorillaGuardianZoneManager> enumerator = (from gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers
			where ((Behaviour)gorillaGuardianZoneManager).enabled && gorillaGuardianZoneManager.IsZoneValid()
			where gorillaGuardianZoneManager.CurrentGuardian == RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig)
			select gorillaGuardianZoneManager).GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				GorillaGuardianZoneManager current = enumerator.Current;
				current.SetGuardian((NetPlayer)null);
			}
			while (enumerator.MoveNext());
		}
	}

	public static void UntagGun()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
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
					RemoveInfected(RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig));
				}
			}
			else
			{
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			GunLib.lockedTargetRig = null;
			GunLib.SetGunVisibility(isVisible: false);
		}
	}

	public static void TagAll()
	{
		if (!InputHandler.RTrigger())
		{
			return;
		}
		IEnumerator<VRRig> enumerator = VRRigCache.ActiveRigs.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				VRRig current = enumerator.Current;
				TagPlayer(current);
			}
			while (enumerator.MoveNext());
		}
	}

	public static void NoTagOnJoin(bool setActive)
	{
		ExitGames.Client.Photon.Hashtable val = new ExitGames.Client.Photon.Hashtable();
		((Dictionary<object, object>)(object)val).Add((object)"didTutorial", (object)(!setActive));
		PhotonNetwork.LocalPlayer.SetCustomProperties(val, (ExitGames.Client.Photon.Hashtable)null, (WebFlags)null);
		PlayerPrefs.SetString("didTutorial", setActive ? "" : "done");
		PlayerPrefs.Save();
	}

	public static void TagAura()
	{
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if (RigManager.RigIsInfected(VRRig.LocalRig) && !RigManager.RigIsInfected(current) && Vector3.Distance(current.bodyTransform.position, ((Component)Variables.taggerInstance.bodyCollider).transform.position) < 4f)
			{
				Variables.playerInstance.RightHand.controllerTransform.position = current.headMesh.transform.position;
				PhotonSerializer.ForceSerialization();
				PunExtensions.GetPhotonView(GameObject.Find("Photon.Realtime.Player Objects/RigCache/Network Parent/GameMode(Clone)")).RPC("RPC_ReportTag", (RpcTarget)0, new object[1] { current.Creator.ActorNumber });
				PhotonNetwork.SendAllOutgoingCommands();
				((PhotonPeer)PhotonNetwork.NetworkingClient.LoadBalancingPeer).SendAcksOnly();
			}
		}
	}

	public static void NoPaintBrawlDelay(bool enable)
	{
		if (Variables.IsMaster())
		{
			GorillaPaintbrawlManager val = (GorillaPaintbrawlManager)GorillaGameManager.instance;
			val.hitCooldown = (enable ? 0f : 3f);
			val.tagCoolDown = (enable ? 0f : 5f);
			val.stunGracePeriod = (enable ? 0f : 2f);
		}
	}

	public static void TagGun()
	{
		if (GunLib.SetupLockOnGun())
		{
			TagPlayer(GunLib.lockedTargetRig);
		}
	}

	public static void PaintbrawlBalloonSpamSelf()
	{
		if (Variables.IsMaster() && !(Time.time < balloonSpamDelay))
		{
			balloonSpamDelay = Time.time + 0.1f;
			PaintbrawlSpamBalloon(PhotonNetwork.LocalPlayer.ActorNumber);
		}
	}

	public static void StartPaintbrawlGame()
	{
		if (Variables.IsMaster())
		{
			((GorillaPaintbrawlManager)GorillaGameManager.instance).StartBattle();
		}
	}

	public static void PaintbrawlBalloonSpamGun()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
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
				else if (Time.time >= balloonSpamDelay)
				{
					balloonSpamDelay = Time.time + 0.1f;
					PaintbrawlSpamBalloon(GunLib.lockedTargetRig.Creator.ActorNumber);
				}
			}
			else
			{
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			GunLib.lockedTargetRig = null;
			GunLib.SetGunVisibility(isVisible: false);
		}
	}

	public static void GuardianSelf()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
		GorillaGuardianManager val = (GorillaGuardianManager)GorillaGameManager.instance;
		if (val.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
		{
			return;
		}
		TappableGuardianIdol[] allType = Variables.GetAllType<TappableGuardianIdol>(false);
		int num = 0;
		while (num < allType.Length)
		{
			TappableGuardianIdol val2 = allType[num];
			NetworkSceneObject manager = ((Tappable)val2).manager;
			if ((UnityEngine.Object)(object)manager != (UnityEngine.Object)null && (UnityEngine.Object)(object)manager.photonView != (UnityEngine.Object)null && !val2.isChangingPositions)
			{
				GorillaGuardianZoneManager zoneManager = val2.zoneManager;
				if (zoneManager.IsZoneValid() && (UnityEngine.Object)(object)manager != (UnityEngine.Object)null && zoneManager.CurrentGuardian == null)
				{
					zoneManager.SetGuardian(NetworkSystem.Instance.LocalPlayer);
					break;
				}
				num++;
			}
			else
			{
				num++;
			}
		}
	}

	public static void PaintbrawlReviveAll()
	{
		if (Variables.IsMaster())
		{
			Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
			foreach (Photon.Realtime.Player val in playerList)
			{
				PaintbrawlRevivePlayer(val.ActorNumber);
			}
		}
	}

	public static void GuardianFlingGun()
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
					SetPlayerVelocity(RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig), new Vector3(0f, 99999f, 0f));
				}
			}
			else
			{
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			GunLib.lockedTargetRig = null;
			GunLib.SetGunVisibility(isVisible: false);
		}
	}

	public static void PaintbrawlKillGun()
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
					PaintbrawlKillPlayer(GunLib.lockedTargetRig.Creator.ActorNumber, GunLib.lockedTargetRig);
				}
			}
			else
			{
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			GunLib.lockedTargetRig = null;
			GunLib.SetGunVisibility(isVisible: false);
		}
	}

	public static void GuardianBringAllToPointer()
	{
		if (GunLib.GunGrips)
		{
			GunLib.SetupRaycast();
			if (GunLib.GunTriggers)
			{
				if (!(Time.time > guardianReleaseDelay))
				{
					return;
				}
				guardianReleaseDelay = Time.time + 0.1f;
				IEnumerator<VRRig> enumerator = VRRigCache.ActiveRigs.Where((VRRig plr) => !plr.isLocal).GetEnumerator();
				if (enumerator.MoveNext())
				{
					do
					{
						VRRig current = enumerator.Current;
						NetPlayer netPlayerFromVRRig = RigManager.GetNetPlayerFromVRRig(current);
						Vector3 val = GunLib.raycastHit.point - ((Component)current).transform.position;
						SetPlayerVelocity(netPlayerFromVRRig, val.normalized * 20f);
						Safety.RPCShield();
					}
					while (enumerator.MoveNext());
				}
			}
			else
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			}
		}
		else
		{
			GunLib.SetGunVisibility(isVisible: false);
		}
	}

	public static void PaintbrawlRevivePlayer(int actorNumber)
	{
		if (Variables.IsMaster())
		{
			GorillaPaintbrawlManager val = (GorillaPaintbrawlManager)GorillaGameManager.instance;
			val.playerLives[actorNumber] = 4;
		}
	}

	public static void PaintbrawlReviveSelf()
	{
		PaintbrawlRevivePlayer(PhotonNetwork.LocalPlayer.ActorNumber);
	}

	public static void SetAllPlayersVelocity(RpcTarget target, Vector3 velocity)
	{
		if (velocity.sqrMagnitude > 400f)
		{
			velocity = velocity.normalized * 20f;
		}
		GorillaGuardianManager val2 = (GorillaGuardianManager)GorillaGameManager.instance;
		if (!val2.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Must Be Guardian");
			return;
		}
		IEnumerable<VRRig> source = ((target == (RpcTarget)1) ? VRRigCache.ActiveRigs.Where((VRRig r) => !r.isLocal) : VRRigCache.ActiveRigs);
		foreach (VRRig current in source.Where((VRRig r) => (UnityEngine.Object)(object)r != (UnityEngine.Object)null))
		{
			NetPlayer netPlayerFromVRRig = RigManager.GetNetPlayerFromVRRig(current);
			if (netPlayerFromVRRig == null)
			{
				continue;
			}
			RigManager.GetNetworkViewFromVRRig(current).SendRPC("GrabbedByPlayer", netPlayerFromVRRig, new object[3] { true, false, false });
			RigManager.GetNetworkViewFromVRRig(current).SendRPC("DroppedByPlayer", netPlayerFromVRRig, new object[1] { velocity });
		}
		Safety.RPCShield();
	}

	public static void PaintbrawlSpamAllBalloons()
	{
		if (Variables.IsMaster() && !(Time.time < balloonSpamDelay))
		{
			balloonSpamDelay = Time.time + 0.1f;
			Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
			foreach (Photon.Realtime.Player val in playerList)
			{
				PaintbrawlSpamBalloon(val.ActorNumber);
			}
		}
	}

	public static void RemoveInfected(NetPlayer plr)
	{
		if (!PhotonNetwork.InRoom || (UnityEngine.Object)(object)GorillaGameManager.instance == (UnityEngine.Object)null)
		{
			return;
		}
		GameModeType gameType = GorillaGameManager.instance.GameType();
		if ((int)(gameType - 1) == 1)
		{
			return;
		}
		GorillaTagManager val = (GorillaTagManager)GorillaGameManager.instance;
		if (val.isCurrentlyTag && val.currentIt == plr)
		{
			val.currentIt = null;
		}
		else if (!val.isCurrentlyTag && val.currentInfected != null && val.currentInfected.Contains(plr))
		{
			val.currentInfected.Remove(plr);
		}
	}

	public static void GuardianOrbitSelfAll()
	{
		if (InputHandler.RTrigger() && !(Time.time <= guardianReleaseDelay))
		{
			guardianReleaseDelay = Time.time + 0.2f;
			VRRig[] array = VRRigCache.ActiveRigs.Where((VRRig r) => !r.isLocal).ToArray();
			Vector3 val = default(Vector3);
			for (int num = 0; num < array.Length; num++)
			{
				float num2 = (360f / (float)array.Length * (float)num + Time.time) * (MathF.PI / 180f);
				val = new Vector3(Mathf.Cos(num2) * 5f, 2f, Mathf.Sin(num2) * 5f);
				Vector3 val2 = ((Component)Variables.taggerInstance.headCollider).transform.position + val;
				SetPlayerVelocity(RigManager.GetNetPlayerFromVRRig(array[num]), val2 - ((Component)array[num]).transform.position);
			}
			Safety.RPCShield();
		}
	}

	public static void PaintbrawlSpamBalloon(int actorNumber)
	{
		if (Variables.IsMaster())
		{
			GorillaPaintbrawlManager val = (GorillaPaintbrawlManager)GorillaGameManager.instance;
			val.playerLives[actorNumber] = UnityEngine.Random.Range(0, 4);
		}
	}

	public static void GuardianGrabAll()
	{
		if (!InputHandler.RGrip() || Time.time <= guardianGrabDelay)
		{
			return;
		}
		guardianGrabDelay = Time.time + 0.1f;
		IEnumerator<VRRig> enumerator = VRRigCache.ActiveRigs.Where((VRRig r) => !r.isLocal).GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				VRRig current = enumerator.Current;
				NetPlayer netPlayerFromVRRig = RigManager.GetNetPlayerFromVRRig(current);
				Vector3 val = ((Component)Variables.taggerInstance.bodyCollider).transform.position - ((Component)current).transform.position;
				SetPlayerVelocity(netPlayerFromVRRig, val.normalized * 20f);
			}
			while (enumerator.MoveNext());
		}
	}

	public static void GuardianReleaseAll()
	{
		if (!InputHandler.RTrigger() || Time.time <= guardianReleaseDelay)
		{
			return;
		}
		guardianReleaseDelay = Time.time + 0.1f;
		GorillaGuardianManager val = (GorillaGuardianManager)GorillaGameManager.instance;
		if (!val.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Must Be Guardian");
			return;
		}
		IEnumerator<VRRig> enumerator = VRRigCache.ActiveRigs.Where((VRRig r) => !r.isLocal).GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				VRRig current = enumerator.Current;
				RigManager.GetNetworkViewFromVRRig(current).SendRPC("DroppedByPlayer", (RpcTarget)1, new object[1] { Vector3.zero });
			}
			while (enumerator.MoveNext());
		}
		Safety.RPCShield();
	}

	public static void RestartPaintbrawlGame()
	{
		if (Variables.IsMaster())
		{
			GorillaPaintbrawlManager val = (GorillaPaintbrawlManager)GorillaGameManager.instance;
			val.BattleEnd();
			val.StartBattle();
		}
	}

	public static void UnGuardianSelf()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
		IEnumerator<GorillaGuardianZoneManager> enumerator = (from gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers
			where ((Behaviour)gorillaGuardianZoneManager).enabled && gorillaGuardianZoneManager.IsZoneValid()
			where gorillaGuardianZoneManager.CurrentGuardian == NetworkSystem.Instance.LocalPlayer
			select gorillaGuardianZoneManager).GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				GorillaGuardianZoneManager current = enumerator.Current;
				current.SetGuardian((NetPlayer)null);
			}
			while (enumerator.MoveNext());
		}
	}

	public static void UnGuardianAll()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
		IEnumerator<GorillaGuardianZoneManager> enumerator = GorillaGuardianZoneManager.zoneManagers.Where((GorillaGuardianZoneManager gorillaGuardianZoneManager) => ((Behaviour)gorillaGuardianZoneManager).enabled && gorillaGuardianZoneManager.IsZoneValid()).GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				GorillaGuardianZoneManager current = enumerator.Current;
				current.SetGuardian((NetPlayer)null);
			}
			while (enumerator.MoveNext());
		}
	}

	public static void TagSelf()
	{
		if (!RigManager.RigIsInfected(VRRig.LocalRig))
		{
			foreach (VRRig current in VRRigCache.ActiveRigs)
			{
				if (RigManager.RigIsInfected(current))
				{
					((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
					((Component)Variables.taggerInstance.offlineVRRig).transform.position = current.rightHandTransform.position;
					Vector3 position = ((Component)current).transform.position;
					Vector3 position2 = Variables.taggerInstance.offlineVRRig.head.rigTarget.position;
					float num = Vector3.Distance(position, position2);
					if (num < 1.667f)
					{
						Variables.playerInstance.LeftHand.controllerTransform.position = position;
					}
				}
			}
		}
		else
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
		}
	}

	public static void PaintbrawlMatAll()
	{
		if (Variables.IsMaster())
		{
			Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
			foreach (Photon.Realtime.Player val in playerList)
			{
				GorillaPaintbrawlManager val2 = (GorillaPaintbrawlManager)GorillaGameManager.instance;
				val2.playerLives[val.ActorNumber] = 0;
				val2.playerLives[val.ActorNumber] = 4;
			}
		}
	}

	public static void PaintbrawlMatGun()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
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
				GorillaPaintbrawlManager val = (GorillaPaintbrawlManager)GorillaGameManager.instance;
				val.playerLives[GunLib.lockedTargetRig.Creator.ActorNumber] = 0;
				val.playerLives[GunLib.lockedTargetRig.Creator.ActorNumber] = 4;
			}
			else
			{
				GunLib.lockedTargetRig = null;
			}
		}
		else
		{
			GunLib.lockedTargetRig = null;
			GunLib.SetGunVisibility(isVisible: false);
		}
	}

	public static void PaintbrawlGodmode()
	{
		if (Variables.IsMaster())
		{
			GorillaPaintbrawlManager val = (GorillaPaintbrawlManager)GorillaGameManager.instance;
			val.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 4;
			Variables.playerInstance.disableMovement = false;
		}
	}

	public static void PaintbrawlKillAll()
	{
		if (Variables.IsMaster())
		{
			Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
			int num = 0;
			while (num < playerList.Length)
			{
				Photon.Realtime.Player val = playerList[num];
				if (val.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
				{
					PaintbrawlKillPlayer(val.ActorNumber, null);
					num++;
				}
				else
				{
					num++;
				}
			}
		}
		else
		{
			VRRig val2 = (from r in VRRigCache.ActiveRigs
				where !r.isLocal
				orderby UnityEngine.Random.value
				select r).FirstOrDefault();
			if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null)
			{
				PaintbrawlKillPlayer(val2.Creator.ActorNumber, val2);
			}
		}
	}

	public static void AlwaysGuardian()
	{
		if (!PhotonNetwork.InRoom || (int)GorillaGameManager.instance.GameType() != 8)
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			return;
		}
		GorillaGuardianManager val = (GorillaGuardianManager)GorillaGameManager.instance;
		if (val.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			return;
		}
		TappableGuardianIdol[] allType = Variables.GetAllType<TappableGuardianIdol>(false);
		foreach (TappableGuardianIdol val2 in allType)
		{
			NetworkSceneObject manager = ((Tappable)val2).manager;
			if (!((UnityEngine.Object)(object)manager?.photonView != (UnityEngine.Object)null) || val2.isChangingPositions)
			{
				continue;
			}
			GorillaGuardianZoneManager zoneManager = val2.zoneManager;
			if (zoneManager.IsZoneValid())
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
				((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)val2).transform.position + UnityEngine.Random.insideUnitSphere * 0.1f;
				((Component)Variables.taggerInstance.offlineVRRig.leftHand.rigTarget).transform.position = ((Component)val2).transform.position;
				((Component)Variables.taggerInstance.offlineVRRig.rightHand.rigTarget).transform.position = ((Component)val2).transform.position;
				if (Time.time > autoGuardianDelay)
				{
					float currentActivationTime = zoneManager._currentActivationTime;
					float requiredActivationTime = zoneManager.requiredActivationTime;
					autoGuardianDelay = Time.time + ((currentActivationTime >= requiredActivationTime - 1f) ? 0f : 0.2f);
					((Tappable)val2).OnTap(UnityEngine.Random.Range(0f, 1f));
					Safety.RPCShield();
				}
				return;
			}
		}
		((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
	}

	public static void PaintbrawlKillPlayer(int actorNumber, VRRig rig)
	{
		if (Variables.IsMaster())
		{
			GorillaPaintbrawlManager val = (GorillaPaintbrawlManager)GorillaGameManager.instance;
			val.playerLives[actorNumber] = 0;
			return;
		}
		Photon.Realtime.Player val2;
		int num;
		if (paintbrawlKillDelays.TryGetValue(actorNumber, out var value))
		{
			if (Time.time < value)
			{
				return;
			}
			paintbrawlKillDelays[actorNumber] = Time.time + 3.1f;
			val2 = PhotonNetwork.PlayerList.First((Photon.Realtime.Player p) => p.ActorNumber == actorNumber);
			num = 0;
		}
		else
		{
			paintbrawlKillDelays[actorNumber] = Time.time + 3.1f;
			val2 = PhotonNetwork.PlayerList.First((Photon.Realtime.Player p) => p.ActorNumber == actorNumber);
			num = 0;
		}
		if (num < 10)
		{
			do
			{
				((GorillaWrappedSerializer)GameMode.ActiveNetworkHandler).SendRPC("RPC_ReportSlingshotHit", false, new object[3]
				{
					val2,
					((Component)rig).transform.position,
					paintbrawlKillIndex
				});
				Safety.RPCShield();
				paintbrawlKillIndex++;
				num++;
			}
			while (num < 10);
		}
	}

	public static void PaintbrawlKillSelf()
	{
		PaintbrawlKillPlayer(PhotonNetwork.LocalPlayer.ActorNumber, Variables.taggerInstance.offlineVRRig);
	}

	public static void GuardianGun()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
		GorillaGuardianManager val = (GorillaGuardianManager)GorillaGameManager.instance;
		if (val.IsPlayerGuardian(RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig)))
		{
			return;
		}
		TappableGuardianIdol[] allType = Variables.GetAllType<TappableGuardianIdol>(false);
		int num = 0;
		while (num < allType.Length)
		{
			TappableGuardianIdol val2 = allType[num];
			NetworkSceneObject manager = ((Tappable)val2).manager;
			if ((UnityEngine.Object)(object)manager != (UnityEngine.Object)null && (UnityEngine.Object)(object)manager.photonView != (UnityEngine.Object)null && !val2.isChangingPositions)
			{
				GorillaGuardianZoneManager zoneManager = val2.zoneManager;
				if (zoneManager.IsZoneValid() && (UnityEngine.Object)(object)manager != (UnityEngine.Object)null && zoneManager.CurrentGuardian == null)
				{
					zoneManager.SetGuardian(RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig));
					break;
				}
				num++;
			}
			else
			{
				num++;
			}
		}
	}

	public static void SetPlayerVelocity(NetPlayer victim, Vector3 velocity)
	{
		if (victim == null)
		{
			return;
		}
		if (velocity.sqrMagnitude > 400f)
		{
			velocity = velocity.normalized * 20f;
		}
		GorillaGuardianManager val2 = (GorillaGuardianManager)GorillaGameManager.instance;
		if (!val2.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Must Be Guardian");
			return;
		}
		VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(victim);
		if (!((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null))
		{
			RigManager.GetNetworkViewFromVRRig(vRRigFromNetPlayer).SendRPC("GrabbedByPlayer", victim, new object[3] { true, false, false });
			RigManager.GetNetworkViewFromVRRig(vRRigFromNetPlayer).SendRPC("DroppedByPlayer", victim, new object[1] { velocity });
			Safety.RPCShield();
		}
	}

	public static void GuardianAll()
	{
		if (!Variables.IsMaster())
		{
			return;
		}
		int num = 0;
		IEnumerator<GorillaGuardianZoneManager> enumerator = GorillaGuardianZoneManager.zoneManagers.Where((GorillaGuardianZoneManager gorillaGuardianZoneManager) => ((Behaviour)gorillaGuardianZoneManager).enabled && gorillaGuardianZoneManager.IsZoneValid()).GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				GorillaGuardianZoneManager current = enumerator.Current;
				current.SetGuardian(((NetPlayer)(PhotonNetwork.PlayerList[num])));
				num++;
			}
			while (enumerator.MoveNext());
		}
	}

	public static void TagPlayer(VRRig rig)
	{
		if (!((UnityEngine.Object)(object)Variables.taggerInstance == (UnityEngine.Object)null))
		{
			if ((UnityEngine.Object)(object)rig == (UnityEngine.Object)null || (UnityEngine.Object)(object)rig == (UnityEngine.Object)(object)VRRig.LocalRig || RigManager.RigIsInfected(rig) || !RigManager.RigIsInfected(VRRig.LocalRig))
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
				return;
			}
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
			((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)rig).transform.position;
			PhotonSerializer.ForceSerialization();
			PunExtensions.GetPhotonView(GameObject.Find("Photon.Realtime.Player Objects/RigCache/Network Parent/GameMode(Clone)")).RPC("RPC_ReportTag", (RpcTarget)0, new object[1] { rig.Creator.ActorNumber });
			PhotonNetwork.SendAllOutgoingCommands();
			((PhotonPeer)PhotonNetwork.NetworkingClient.LoadBalancingPeer).SendAcksOnly();
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			Safety.RPCShield();
		}
	}

	public static void EndPaintbrawlGame()
	{
		if (Variables.IsMaster())
		{
			((GorillaPaintbrawlManager)GorillaGameManager.instance).BattleEnd();
		}
	}
}
