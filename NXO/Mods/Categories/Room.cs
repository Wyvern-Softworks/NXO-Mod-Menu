using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaNetworking;
using GorillaTagScripts;
using NXO.Menu;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using InternalState = NetworkSystemPUN.InternalState;

namespace NXO.Mods.Categories;

public class Room : MonoBehaviourPunCallbacks
{
	public static string roomCode;

	public static bool instantCreate;

	public static string RandomString(int length = 4)
	{
		char[] array = new char[length];
		int num = 0;
		if (num < length)
		{
			do
			{
				array[num] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[UnityEngine.Random.Range(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Length)];
				num++;
			}
			while (num < length);
		}
		return new string(array);
	}

	public static void SetQuitBoxActive(bool isActive)
	{
		GameObject val = GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox");
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			val.SetActive(isActive);
		}
	}

	public static void DumpAllRPCs()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("=== ALL RPCS IN GORILLA TAG ===");
		stringBuilder.AppendLine($"Generated: {DateTime.Now}\n");
		int num = 0;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type[] types = assembly.GetTypes();
			int num2 = 0;
			while (num2 < types.Length)
			{
				Type type = types[num2];
				IEnumerable<MethodInfo> enumerable = from m in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
					where ((MemberInfo)m).GetCustomAttribute<PunRPC>() != null
					select m;
			if (enumerable.Any())
			{
				stringBuilder.AppendLine("\n━━━ " + type.FullName + " ━━━");
				foreach (MethodInfo current in enumerable)
				{
					ParameterInfo[] parameters = current.GetParameters();
					string text = ((parameters.Length == 0) ? "no parameters" : string.Join(", ", parameters.Select((ParameterInfo p) => p.ParameterType.Name + " " + p.Name)));
					stringBuilder.AppendLine("  [RPC] " + current.Name + "(" + text + ")");
					num++;
				}
				num2++;
			}
				else
				{
					num2++;
				}
			}
		}
		stringBuilder.AppendLine($"\n\nTotal RPCs Found: {num}");
		string text2 = Path.Combine(Variables.folderName, $"RPC_Dump_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
		File.WriteAllText(text2, stringBuilder.ToString());
		NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "RPC dump saved");
	}

	public static void Reconnect()
	{
		if (string.IsNullOrEmpty(roomCode))
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "No Photon.Realtime.Room Code Saved");
			return;
		}
		if (PhotonNetwork.InRoom)
		{
			NetworkSystem.Instance.ReturnToSinglePlayer();
			CoroutineHelper.InvokeAfterDelay(1.5f, Reconnect);
			return;
		}
		((PhotonNetworkController)PhotonNetworkController.Instance).AttemptToJoinSpecificRoomWithCallback(roomCode, (GorillaNetworking.JoinType)0, (Action<NetJoinResult>)delegate(NetJoinResult result)
		{
			if ((int)result == 2)
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Photon.Realtime.Room Is Full");
			}
			else if ((int)result == 4)
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Unknown");
			}
		});
	}

	public static void CreateRoom(string roomName, bool isPublic, byte roomSize = 0, GorillaNetworking.JoinType roomJoinType = (GorillaNetworking.JoinType)0)
	{
		if (roomSize > 10 && !roomName.StartsWith("@"))
		{
			roomName = "@" + roomName;
		}
		GorillaNetworkJoinTrigger val = ((PhotonNetworkController)PhotonNetworkController.Instance).currentJoinTrigger ?? ((GorillaComputer)GorillaComputer.instance).GetJoinTriggerForZone("forest");
		RoomConfig val4 = new RoomConfig
		{
			createIfMissing = true,
			isJoinable = true,
			isPublic = isPublic,
			MaxPlayers = ((roomSize == 0) ? RoomSystem.GetRoomSizeForCreate(val.zone, Enum.Parse<GameModeType>(((GorillaComputer)GorillaComputer.instance).currentGameMode.Value, true), !isPublic, SubscriptionManager.IsLocalSubscribed()) : roomSize)
		};
		ExitGames.Client.Photon.Hashtable val2 = new ExitGames.Client.Photon.Hashtable();
		((Dictionary<object, object>)val2).Add((object)"platform", (object)((PhotonNetworkController)PhotonNetworkController.Instance).platformTag);
		((Dictionary<object, object>)val2).Add((object)"gameMode", (object)val.GetFullDesiredGameModeString());
		((Dictionary<object, object>)val2).Add((object)"language", (object)((object)LocalisationManager.CurrentLanguage).ToString());
		((Dictionary<object, object>)val2).Add((object)"fan_club", (object)(SubscriptionManager.IsLocalSubscribed() ? "true" : "false"));
		((Dictionary<object, object>)val2).Add((object)"queueName", (object)((GorillaComputer)GorillaComputer.instance).currentQueue);
		val4.CustomProps = val2;
		((PhotonNetworkController)PhotonNetworkController.Instance).currentJoinType = roomJoinType;
		if ((int)roomJoinType == 2 || (int)roomJoinType == 4)
		{
			Task.Run((Func<Task?>)((PhotonNetworkController)PhotonNetworkController.Instance).SendPartyFollowCommands);
		}
		if ((int)roomJoinType == 2)
		{
			val4.SetFriendIDs(FriendshipGroupDetection.Instance.PartyMemberIDs.ToList());
		}
		else
		{
			val4.SetFriendIDs(((PhotonNetworkController)PhotonNetworkController.Instance).FriendIDList);
		}
		if (instantCreate)
		{
			NetworkSystem instance = NetworkSystem.Instance;
			((NetworkSystemPUN)((instance is NetworkSystemPUN) ? instance : null)).internalState = (InternalState)16;
			ForceCreateRoom(roomName, val4);
		}
		else
		{
			NetworkSystem.Instance.ConnectToRoom(roomName, val4, -1);
		}
	}

	public static void JoinRoom()
	{
		SearchAndKeyboard.OpenTypingKeyboard("", "Enter room code...");
		SearchAndKeyboard.onTypingComplete = delegate(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "No Photon.Realtime.Room Code Entered");
			}
			else
			{
				((PhotonNetworkController)PhotonNetworkController.Instance).AttemptToJoinSpecificRoomWithCallback(code, (GorillaNetworking.JoinType)0, (Action<NetJoinResult>)delegate(NetJoinResult result)
				{
					if ((int)result == 2)
					{
						NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Photon.Realtime.Room Is Full");
					}
					else if ((int)result == 4)
					{
						NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Unknown");
					}
				});
			}
		};
	}

	public static async Task ForceCreateRoom(string name, RoomConfig options)
	{
		if (NetworkSystem.Instance.InRoom)
		{
			await NetworkSystem.Instance.ReturnToSinglePlayer();
		}
		NetworkSystem instance = NetworkSystem.Instance;
		await ((NetworkSystemPUN)((instance is NetworkSystemPUN) ? instance : null)).TryCreateRoom(name, options);
	}

	public static void SetNetworkTriggersActive(bool isActive)
	{
		GameObject val = GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab");
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			val.SetActive(isActive);
		}
	}

	public static void JoinRandomPublic()
	{
		if (PhotonNetwork.InRoom)
		{
			NetworkSystem.Instance.ReturnToSinglePlayer();
			CoroutineHelper.InvokeAfterDelay(1.5f, JoinRandomPublic);
			return;
		}
		GorillaNetworkJoinTrigger val = ((PhotonNetworkController)PhotonNetworkController.Instance).currentJoinTrigger ?? ((GorillaComputer)GorillaComputer.instance).GetJoinTriggerForZone("forest");
		if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "No Join Trigger Found");
		}
		else
		{
			((PhotonNetworkController)PhotonNetworkController.Instance).AttemptToJoinPublicRoom(val, (GorillaNetworking.JoinType)0, null, false);
		}
	}

	public static void Disconnect()
	{
		if (!PhotonNetwork.InRoom)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Not In A Room");
		}
		else
		{
			NetworkSystem.Instance.ReturnToSinglePlayer();
		}
	}

	public static void GrabSelfID()
	{
		string value = "SELF ID GRABBED FROM NXO";
		StringBuilder stringBuilder = new StringBuilder(value);
		stringBuilder.AppendLine("NAME: " + PhotonNetwork.LocalPlayer.NickName + " ID: " + PhotonNetwork.LocalPlayer.UserId);
		string text = Path.Combine(Variables.folderName, "Self_ID_By_NXO.txt");
		File.WriteAllText(text, stringBuilder.ToString());
		NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Self ID saved");
	}

	public static void GrabAllIDs()
	{
		if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.PlayerList == null || PhotonNetwork.PlayerList.Length == 0)
		{
			UnityEngine.Debug.LogError((object)"Failed to grab IDs: No room or players found.");
			return;
		}
		string value = "IDS GRABBED FROM NXO \nIDS GRABBED FROM ROOM: " + PhotonNetwork.CurrentRoom.Name + "\n\n";
		StringBuilder stringBuilder = new StringBuilder(value);
		Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Photon.Realtime.Player val in playerList)
		{
			stringBuilder.AppendLine("NAME: " + val.NickName + " ID: " + val.UserId);
		}
		string text = Path.Combine(Variables.folderName, "Grabbed_IDs_By_NXO.txt");
		File.WriteAllText(text, stringBuilder.ToString());
		NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "IDs saved");
	}

	public static string RandomRoomName()
	{
		string text;
		do
		{
			text = RandomString();
		}
		while (((GorillaComputer)GorillaComputer.instance).CheckAutoBanListForName(text));
		return text;
	}
}
