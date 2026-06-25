using System;
using System.Collections.Generic;
using System.Text;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace NXO.Utilities;

[HarmonyPatch]
public static class LoggingPatches
{
	[HarmonyPatch(typeof(PhotonNetwork), "RaiseEvent")]
	[HarmonyPrefix]
	public static void RaiseEvent(byte eventCode, object eventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
	{
		if (!Logger.Enabled || Logger.EventBlacklist.Contains(eventCode))
		{
			return;
		}
		Photon.Realtime.Player localPlayer = PhotonNetwork.LocalPlayer;
		string arg = ((localPlayer != null) ? localPlayer.NickName : null) ?? "Local";
		Photon.Realtime.Player localPlayer2 = PhotonNetwork.LocalPlayer;
		int num = ((localPlayer2 != null) ? localPlayer2.ActorNumber : (-1));
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
		stringBuilder.AppendLine("[EVENT] PhotonNetwork OUT");
		stringBuilder.AppendLine($"  ├─ Code: {eventCode}");
		stringBuilder.AppendLine($"  ├─ Sender: {arg} (Actor #{num})");
		if (eventContent != null)
		{
			stringBuilder.AppendLine("  ├─ Data: " + Logger.FormatData(eventContent));
		}
		if (raiseEventOptions?.TargetActors != null)
		{
			stringBuilder.AppendLine("  ├─ Targets: [" + string.Join(", ", raiseEventOptions.TargetActors) + "]");
		}
		stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
		stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
		UnityEngine.Debug.Log((object)stringBuilder.ToString());
	}

	[HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCall", new Type[]
	{
		typeof(PhotonMessageInfo),
		typeof(string)
	})]
	[HarmonyPrefix]
	public static void MonkeAgent_IncrementRPCCall(PhotonMessageInfo info, string callingMethod)
	{
		if (Logger.Enabled && !Logger.RPCBlacklist.Contains(callingMethod))
		{
			Photon.Realtime.Player sender = info.Sender;
			_ = ((sender != null) ? sender.NickName : null) ?? "Unknown";
			Photon.Realtime.Player sender2 = info.Sender;
			if (sender2 != null)
			{
				_ = sender2.ActorNumber;
			}
			Photon.Realtime.Player sender3 = info.Sender;
			if (sender3 != null)
			{
				_ = sender3.IsMasterClient;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
			stringBuilder.AppendLine("[RPC] MonkeAgent");
			stringBuilder.AppendLine("  ├─ Method: " + callingMethod);
		}
	}

	[HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCallLocal", new Type[]
	{
		typeof(PhotonMessageInfoWrapped),
		typeof(string)
	})]
	[HarmonyPrefix]
	public static void MonkeAgent_IncrementRPCCallLocal(MonkeAgent __instance, PhotonMessageInfoWrapped infoWrapped, string rpcFunction)
	{
		if (Logger.Enabled && !Logger.RPCBlacklist.Contains(rpcFunction))
		{
			string arg = "Unknown";
			int num = -1;
			PhotonView component = ((Component)__instance).GetComponent<PhotonView>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null && component.Owner != null)
			{
				arg = component.Owner.NickName ?? "Unknown";
				num = component.Owner.ActorNumber;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
				stringBuilder.AppendLine("[RPC] MonkeAgent.Local");
				stringBuilder.AppendLine("  ├─ Method: " + rpcFunction);
				stringBuilder.AppendLine($"  ├─ Sender: {arg} (Actor #{num})");
				stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
				stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
				UnityEngine.Debug.Log((object)stringBuilder.ToString());
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
				stringBuilder.AppendLine("[RPC] MonkeAgent.Local");
				stringBuilder.AppendLine("  ├─ Method: " + rpcFunction);
				stringBuilder.AppendLine($"  ├─ Sender: {arg} (Actor #{num})");
				stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
				stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
				UnityEngine.Debug.Log((object)stringBuilder.ToString());
			}
		}
	}

	[HarmonyPatch(typeof(VRRig), "IncrementRPC", new Type[]
	{
		typeof(PhotonMessageInfo),
		typeof(string)
	})]
	[HarmonyPrefix]
	public static void VRRig_IncrementRPC_Info(VRRig __instance, PhotonMessageInfo info, string sourceCall)
	{
		if (Logger.Enabled && !Logger.RPCBlacklist.Contains(sourceCall))
		{
			Photon.Realtime.Player sender = info.Sender;
			string arg = ((sender != null) ? sender.NickName : null) ?? "Unknown";
			Photon.Realtime.Player sender2 = info.Sender;
			int num = ((sender2 != null) ? sender2.ActorNumber : (-1));
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
			stringBuilder.AppendLine("[RPC] VRRig");
			stringBuilder.AppendLine("  ├─ Method: " + sourceCall);
			stringBuilder.AppendLine($"  ├─ Sender: {arg} (Actor #{num})");
			stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
			stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
			UnityEngine.Debug.Log((object)stringBuilder.ToString());
		}
	}

	[HarmonyPatch(typeof(LoadBalancingClient), "OpRaiseEvent")]
	[HarmonyPrefix]
	public static void LogOpRaiseEvent(byte eventCode, object customEventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
	{
		if (!Logger.Enabled || Logger.EventBlacklist.Contains(eventCode))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
		stringBuilder.AppendLine("[OP] OpRaiseEvent");
		stringBuilder.AppendLine($"  ├─ Code: {eventCode}");
		stringBuilder.AppendLine("  ├─ Data: " + Logger.FormatData(customEventContent));
		if (raiseEventOptions?.TargetActors != null)
		{
			stringBuilder.AppendLine("  ├─ Targets: [" + string.Join(", ", raiseEventOptions.TargetActors) + "]");
		}
		stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
		stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
		UnityEngine.Debug.Log((object)stringBuilder.ToString());
	}

	[HarmonyPatch(typeof(PhotonView), "TransferOwnership", new Type[] { typeof(Player) })]
	[HarmonyPrefix]
	public static void TransferOwnership(PhotonView __instance, Photon.Realtime.Player newOwner)
	{
		if (Logger.Enabled)
		{
			Photon.Realtime.Player owner = __instance.Owner;
			string text = ((owner != null) ? owner.NickName : null) ?? "None";
			if (newOwner == null)
			{
				string text2 = null ?? "None";
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
				stringBuilder.AppendLine("[OWNERSHIP] Transfer");
				stringBuilder.AppendLine($"  ├─ ViewID: {__instance.ViewID}");
				stringBuilder.AppendLine("  ├─ From: " + text);
				stringBuilder.AppendLine("  ├─ To: " + text2);
				stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
				stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
				UnityEngine.Debug.Log((object)stringBuilder.ToString());
			}
			else
			{
				string text2 = newOwner.NickName ?? "None";
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
				stringBuilder.AppendLine("[OWNERSHIP] Transfer");
				stringBuilder.AppendLine($"  ├─ ViewID: {__instance.ViewID}");
				stringBuilder.AppendLine("  ├─ From: " + text);
				stringBuilder.AppendLine("  ├─ To: " + text2);
				stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
				stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
				UnityEngine.Debug.Log((object)stringBuilder.ToString());
			}
		}
	}

	[HarmonyPatch(typeof(VRRig), "IncrementRPC", new Type[]
	{
		typeof(PhotonMessageInfoWrapped),
		typeof(string)
	})]
	[HarmonyPrefix]
	public static void VRRig_IncrementRPC_Wrapped(VRRig __instance, PhotonMessageInfoWrapped info, string sourceCall)
	{
		if (!Logger.Enabled || Logger.RPCBlacklist.Contains(sourceCall))
		{
			return;
		}
		string arg = "Unknown";
		int num = -1;
		PhotonView value = Traverse.Create((object)__instance).Field("netView").GetValue<PhotonView>();
		if ((UnityEngine.Object)(object)value != (UnityEngine.Object)null && value.Owner != null)
		{
			arg = value.Owner.NickName ?? "Unknown";
			num = value.Owner.ActorNumber;
		}
		else if (__instance != null && __instance.Creator != null)
		{
			arg = __instance.Creator.NickName ?? "Unknown";
			num = __instance.Creator.ActorNumber;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
		stringBuilder.AppendLine("[RPC] VRRig");
		stringBuilder.AppendLine("  ├─ Method: " + sourceCall);
		stringBuilder.AppendLine($"  ├─ Sender: {arg} (Actor #{num})");
		stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
		stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
		UnityEngine.Debug.Log((object)stringBuilder.ToString());
	}

	[HarmonyPatch(typeof(PhotonPeer), "SendOperation", new Type[]
	{
		typeof(byte),
		typeof(Dictionary<byte, object>),
		typeof(SendOptions)
	})]
	[HarmonyPrefix]
	public static void SendOperation(byte operationCode, Dictionary<byte, object> operationParameters, SendOptions sendOptions)
	{
		if (!Logger.Enabled)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
		stringBuilder.AppendLine("[OPERATION] SendOperation");
		stringBuilder.AppendLine($"  ├─ OpCode: {operationCode}");
		if (operationParameters != null)
		{
			Dictionary<byte, object>.Enumerator enumerator = operationParameters.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					KeyValuePair<byte, object> current = enumerator.Current;
					stringBuilder.AppendLine($"  ├─ [{current.Key}] = {Logger.FormatData(current.Value)}");
				}
				while (enumerator.MoveNext());
			}
			stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
			stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
			UnityEngine.Debug.Log((object)stringBuilder.ToString());
		}
		else
		{
			stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
			stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
			UnityEngine.Debug.Log((object)stringBuilder.ToString());
		}
	}

	[HarmonyPatch(typeof(LoadBalancingClient), "OnEvent")]
	[HarmonyPrefix]
	public static void OnEvent(EventData photonEvent)
	{
		if (!Logger.Enabled || Logger.EventBlacklist.Contains(photonEvent.Code))
		{
			return;
		}
		int sender = photonEvent.Sender;
		Photon.Realtime.Room currentRoom = PhotonNetwork.CurrentRoom;
		Photon.Realtime.Player val = ((currentRoom != null) ? currentRoom.GetPlayer(sender, false) : null);
		bool isMasterClient = val != null && val.IsMasterClient;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
		stringBuilder.AppendLine("[EVENT] Photon IN");
		stringBuilder.AppendLine($"  ├─ Code: {photonEvent.Code}");
		if (!isMasterClient)
		{
			stringBuilder.AppendLine("  ├─ CustomData: " + Logger.FormatData(photonEvent.CustomData));
		}
		if (photonEvent.Parameters != null)
		{
			foreach (KeyValuePair<byte, object> current in photonEvent.Parameters)
			{
				stringBuilder.AppendLine($"  ├─ [{current.Key}] = {Logger.FormatData(current.Value)}");
			}
		}
		stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
		stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
		UnityEngine.Debug.Log((object)stringBuilder.ToString());
	}

	[HarmonyPatch(typeof(PhotonView), "RequestOwnership")]
	[HarmonyPrefix]
	public static void RequestOwnership(PhotonView __instance)
	{
		if (Logger.Enabled)
		{
			Photon.Realtime.Player owner = __instance.Owner;
			string text = ((owner != null) ? owner.NickName : null) ?? "None";
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
			stringBuilder.AppendLine("[OWNERSHIP] Request");
			stringBuilder.AppendLine($"  ├─ ViewID: {__instance.ViewID}");
			stringBuilder.AppendLine("  ├─ CurrentOwner: " + text);
			stringBuilder.AppendLine($"  └─ Time: {DateTime.Now:HH:mm:ss.fff}");
			stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
			UnityEngine.Debug.Log((object)stringBuilder.ToString());
		}
	}
}
