using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using RaiseEventBatch = Photon.Pun.PhotonNetwork.RaiseEventBatch;
using SerializeViewBatch = Photon.Pun.PhotonNetwork.SerializeViewBatch;

namespace NXO.Utilities;

internal class PhotonSerializer
{
	public static void ForceSerialization()
	{
		PhotonNetwork.RunViewUpdate();
	}

	public static void BroadcastViews(bool exclude = false, PhotonView[] viewFilter = null)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		List<int> list = (viewFilter ?? Array.Empty<PhotonView>()).Select((PhotonView v) => v.ViewID).ToList();
		foreach (PhotonView current in PhotonNetwork.PhotonViewCollection)
		{
			if (!current.IsMine || (int)current.Synchronization == 0 || !((Behaviour)current).isActiveAndEnabled || PhotonNetwork.blockedSendingGroups.Contains(current.Group))
			{
				continue;
			}

			bool inFilter = list.Contains(current.ViewID);
			if ((!exclude && inFilter) || (exclude && !inFilter))
			{
				SendSerialize(current);
			}
		}
	}

	public static void SendSerialize(PhotonView pv, RaiseEventOptions options = null)
	{
		if (!PhotonNetwork.InRoom || (UnityEngine.Object)(object)pv == (UnityEngine.Object)null)
		{
			return;
		}
		List<object> list = PhotonNetwork.OnSerializeWrite(pv);
		if (list == null)
		{
			return;
		}
		RaiseEventBatch val = new RaiseEventBatch
		{
			Reliable = ((int)pv.Synchronization == 1 || pv.mixedModeIsReliable),
			Group = pv.Group
		};
		IDictionary serializeViewBatches = PhotonNetwork.serializeViewBatches;
		SerializeViewBatch val2 = new SerializeViewBatch(val, 2);
		if (!serializeViewBatches.Contains(val))
		{
			serializeViewBatches[val] = val2;
		}
		val2.Add(list);
		if (options != null)
		{
			RaiseEventOptions serializeRaiseEvOptions = PhotonNetwork.serializeRaiseEvOptions;
			_ = new RaiseEventOptions
			{
				CachingOption = serializeRaiseEvOptions.CachingOption,
				Flags = serializeRaiseEvOptions.Flags,
				InterestGroup = serializeRaiseEvOptions.InterestGroup,
				TargetActors = options.TargetActors,
				Receivers = options.Receivers
			};
		}
		_ = val2.Batch.Reliable;
		List<object> objectUpdates = val2.ObjectUpdates;
		objectUpdates[0] = PhotonNetwork.ServerTimestamp;
		objectUpdates[1] = ((PhotonNetwork.currentLevelPrefix != 0) ? ((object)PhotonNetwork.currentLevelPrefix) : null);
		_ = PhotonNetwork.NetworkingClient;
	}
}
