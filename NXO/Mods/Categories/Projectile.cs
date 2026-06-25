using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using RecieverTarget = NetEventOptions.RecieverTarget;

namespace NXO.Mods.Categories;

public class Projectile
{
	private static float snowballEffectDelay;

	public static void GetTouchedToSnowballEffect()
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
				if (Time.time > snowballEffectDelay)
				{
					SendSnowballEffect(current.OwningNetPlayer.ActorNumber);
					snowballEffectDelay = Time.time + 0.1f;
				}
				return;
			}
		}
	}

	private static void SendSnowballEffect(int actorNumber)
	{
		NetworkSystemRaiseEvent.RaiseEvent((byte)3, (object)new object[3]
		{
			NetworkSystem.Instance.ServerTimestamp,
			6,
			new object[2]
			{
				actorNumber,
				(object)(PlayerEffect)0
			}
		}, new NetEventOptions
		{
			Reciever = (RecieverTarget)1
		}, false);
	}

	public static void TouchToSnowballEffect()
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
				if (Time.time > snowballEffectDelay)
				{
					SendSnowballEffect(current.OwningNetPlayer.ActorNumber);
					snowballEffectDelay = Time.time + 0.1f;
				}
				return;
			}
		}
	}

	public static void SnowballEffectPlayerGun()
	{
		if (GunLib.SetupLockOnGun() && !(Time.time <= snowballEffectDelay) && PhotonNetwork.InRoom && (UnityEngine.Object)(object)GunLib.lockedTargetRig != (UnityEngine.Object)null && (UnityEngine.Object)(object)GunLib.lockedTargetRig != (UnityEngine.Object)(object)RigManager.GetVRRigFromNetPlayer(((NetPlayer)(PhotonNetwork.LocalPlayer))))
		{
			SendSnowballEffect(GunLib.lockedTargetRig.OwningNetPlayer.ActorNumber);
			snowballEffectDelay = Time.time + 0.1f;
		}
	}
}
