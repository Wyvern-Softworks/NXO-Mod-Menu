using System.Collections.Generic;
using ExitGames.Client.Photon;
using NXO.Menu;
using Photon.Pun;
using UnityEngine;

namespace NXO.Mods.Categories;

internal class PropHunt
{
	private static int _propIndex;

	private static string[] _propIds;

	public static ButtonHandler.Button CyclePropButton;

	public static void RemoveBlindFold()
	{
		GameObject ph_blindfold_forCamera_1p = GetPropHunt()._ph_blindfold_forCamera_1p;
		if (ph_blindfold_forCamera_1p != null)
		{
			ph_blindfold_forCamera_1p.SetActive(false);
			GameObject ph_blindfold_forCamera_3p = GetPropHunt()._ph_blindfold_forCamera_3p;
			if (ph_blindfold_forCamera_3p != null)
			{
				ph_blindfold_forCamera_3p.SetActive(false);
			}
		}
		else
		{
			GameObject ph_blindfold_forCamera_3p2 = GetPropHunt()._ph_blindfold_forCamera_3p;
			if (ph_blindfold_forCamera_3p2 != null)
			{
				ph_blindfold_forCamera_3p2.SetActive(false);
			}
		}
	}

	public static GorillaPropHuntGameManager GetPropHunt()
	{
		return (GorillaPropHuntGameManager)GorillaGameManager.instance;
	}

	public static void SpamGamemode()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			GorillaPropHuntGameManager propHunt = GetPropHunt();
			propHunt.PH_OnRoundEnd();
			propHunt.InfectionRoundEndCheck();
			propHunt.InfectionRoundStartCheck();
		}
	}

	public static void PropTagAll()
	{
		GorillaPropHuntGameManager propHunt = GetPropHunt();
		if ((int)propHunt._ph_gameState != 6)
		{
			return;
		}
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)(object)VRRig.LocalRig || ((GorillaTagManager)propHunt).IsInfected(current.Creator))
			{
				continue;
			}
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
			((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)current).transform.position;
			PhotonSerializer.ForceSerialization();
			PunExtensions.GetPhotonView(GameObject.Find("Photon.Realtime.Player Objects/RigCache/Network Parent/GameMode(Clone)")).RPC("RPC_ReportTag", (RpcTarget)0, new object[1] { current.Creator.ActorNumber });
			PhotonNetwork.SendAllOutgoingCommands();
			((PhotonPeer)PhotonNetwork.NetworkingClient.LoadBalancingPeer).SendAcksOnly();
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
		}
	}

	public static string CurrentPropName()
	{
		if (_propIds == null || _propIds.Length == 0)
		{
			return "None";
		}
		return GetPropDisplayName(_propIds[_propIndex]);
	}

	public static void BecomeSeeker()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			((GorillaTagManager)GetPropHunt()).AddInfectedPlayer(NetworkSystem.Instance.LocalPlayer, true);
		}
	}

	public static void BecomeProp()
	{
		if (((GorillaTagManager)GetPropHunt()).IsInfected(NetworkSystem.Instance.LocalPlayer))
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You're A Seeker");
		}
		else if (_propIds == null || _propIds.Length == 0)
		{
			_propIds = PropHuntPools.AllPropCosmeticIds;
			TurnIntoProp(_propIds[_propIndex]);
		}
		else
		{
			TurnIntoProp(_propIds[_propIndex]);
		}
	}

	public static void ForceRoundStart()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			GetPropHunt().InfectionRoundStartCheck();
		}
	}

	public static void SeekerESP(bool enable)
	{
		GorillaPropHuntGameManager propHunt = GetPropHunt();
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			bool flag = ((GorillaTagManager)propHunt).IsInfected(current.Creator);
			if (enable)
			{
				if (!flag)
				{
					((Behaviour)current.skeleton).enabled = false;
					((Renderer)current.skeleton.renderer).enabled = false;
					continue;
				}
				((Renderer)current.mainSkin).material.shader = Variables.guiShader;
				((Renderer)current.mainSkin).material.color = new Color(1f, 0.15f, 0.15f, 0.85f);
				((Renderer)current.mainSkin).material.renderQueue = 4000;
				((Behaviour)current.skeleton).enabled = false;
				((Renderer)current.skeleton.renderer).enabled = false;
			}
			else
			{
				if (!flag)
				{
					continue;
				}
				((Renderer)current.mainSkin).material.shader = Variables.uberShader;
			}
		}
	}

	public static string GetPropDisplayName(string cosmeticId)
	{
		if (PropHuntPools.propCosmeticId_to_cosmeticSO.TryGetValue(cosmeticId, out var value))
		{
			return value.info.displayName;
		}
		return cosmeticId;
	}

	public static void CollapseBoundary()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			GetPropHunt()._ph_playBoundary.radiusScale = 0.001f;
		}
	}

	public static void ForceRoundEnd()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			GetPropHunt().PH_OnRoundEnd();
			GetPropHunt().InfectionRoundEndCheck();
		}
	}

	public static void RevealHiddenProps(bool enable)
	{
		GorillaPropHuntGameManager propHunt = GetPropHunt();
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			if (enable)
			{
				if (((GorillaTagManager)propHunt).IsInfected(current.Creator))
				{
					continue;
				}
				current.SetInvisibleToLocalPlayer(false);
				current.bodyRenderer.SetSkeletonBodyActive(true);
				((Renderer)current.skeleton.renderer).sharedMaterial.shader = Variables.guiShader;
				((Renderer)current.skeleton.renderer).sharedMaterial.color = new Color(0.4f, 1f, 0.6f, 0.35f);
				((Renderer)current.skeleton.renderer).sharedMaterial.renderQueue = 4000;
				((Behaviour)current.skeleton).enabled = true;
				((Renderer)current.skeleton.renderer).enabled = true;
			}
			else
			{
				((Behaviour)current.skeleton).enabled = false;
				((Renderer)current.skeleton.renderer).enabled = false;
				((Renderer)current.skeleton.renderer).sharedMaterial.shader = Variables.uberShader;
			}
		}
	}

	public static void CycleProp(bool forward)
	{
		if (_propIds == null || _propIds.Length == 0)
		{
			_propIds = PropHuntPools.AllPropCosmeticIds;
		}
		if (forward)
		{
			_propIndex = (_propIndex + 1) % _propIds.Length;
		}
		else
		{
			_propIndex = (_propIndex - 1 + _propIds.Length) % _propIds.Length;
		}
		CyclePropButton?.SetText("Prop : " + CurrentPropName());
	}

	public static void BecomeHider()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		GorillaPropHuntGameManager propHunt = GetPropHunt();
		if (((GorillaTagManager)propHunt).currentInfected.Contains(NetworkSystem.Instance.LocalPlayer))
		{
			do
			{
				((GorillaTagManager)propHunt).currentInfected.Remove(NetworkSystem.Instance.LocalPlayer);
			}
			while (((GorillaTagManager)propHunt).currentInfected.Contains(NetworkSystem.Instance.LocalPlayer));
		}
		((GorillaTagManager)propHunt).UpdateInfectionState();
	}

	public static void TurnIntoProp(string playfabId)
	{
		if (!PropHuntPools.IsReady || string.IsNullOrEmpty(playfabId))
		{
			return;
		}
		PropHuntGrabbableProp val = default(PropHuntGrabbableProp);
		PropHuntPools.TryGetGrabbableProp(playfabId, out val);
		PropHuntTaggableProp taggableProp = default(PropHuntTaggableProp);
		PropHuntPools.TryGetTaggableProp(playfabId, out taggableProp);
		if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
		{
			return;
		}
		PropHuntHandFollower propHuntHandFollower = VRRig.LocalRig.propHuntHandFollower;
		propHuntHandFollower.DestroyProp();
		propHuntHandFollower._grabbableProp = val;
		propHuntHandFollower._taggableProp = taggableProp;
		propHuntHandFollower._prop = ((Component)val).gameObject;
		propHuntHandFollower._propOffset = val.offset;
		propHuntHandFollower._hasProp = true;
		val.handFollower = propHuntHandFollower;
		((Component)val).gameObject.SetActive(true);
		int num = 0;
		if (num < val.interactionPoints.Count)
		{
			do
			{
				val.interactionPoints[num].OnSpawn(VRRig.LocalRig);
				num++;
			}
			while (num < val.interactionPoints.Count);
		}
	}
}
