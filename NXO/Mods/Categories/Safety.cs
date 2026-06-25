using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaNetworking;
using NXO.Menu;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;

namespace NXO.Mods.Categories;

public class Safety : MonoBehaviourPunCallbacks
{
	private static bool isOn = false;

	private static bool wasButtonPressed = false;

	private static float lagcooldown = 0f;

	private static float lagcooldownduration = 0.5f;

	private static GameObject radiusSphere = null;

	private static readonly List<ButtonHandler.Button> _panicSnapshot = new List<ButtonHandler.Button>();

	private static bool _panicked;

	private static bool _panicPressedLast;

	private static string PanicButtonText = "Panic (X)";

	private static float lastVol;

	private static float startSilenceTime = -1f;

	private static bool reloaded;

	public static readonly string[] namePrefix = new string[11]
	{
		"EPIC", "REAL", "NOT", "SILLY", "LITTLE", "BIG", "MAYBE", "SUB2", "OG", "FR",
		"NOT"
	};

	public static readonly string[] nameSuffix = new string[12]
	{
		"GT", "VR", "LOL", "FAN", "XD", "LOL", "MONKE", "YT", "NOT", "FR",
		"LMAO", "GTAG"
	};

	public static readonly string[] names = new string[66]
	{
		"0", "PBBV", "J3VU", "BEES", "NEMO", "LEMMING", "BILLY", "TIMMY", "MINIGAMES", "JMANCURLY",
		"VMT", "ELLIOT", "DAISY09", "MONK", "MONKE", "MONKI", "MONKEY", "MONKIY", "GORILL", "GOORILA",
		"GORILLA", "TTT", "TTTPIG", "PPPTIG", "K9", "BANANA", "PEANUTBUTTER", "GHOSTMONKE", "STATUE", "NOVA",
		"LUNAR", "MOON", "SUN", "RANDOM", "UNKNOWN", "GLITCH", "BUG", "ERROR", "CODE", "HACKER",
		"MODDER", "INVIS", "INVISIBLE", "TAGGER", "UNTAGGED", "BLUE", "RED", "GREEN", "PURPLE", "YELLOW",
		"BLACK", "WHITE", "BROWN", "CYAN", "GRAY", "GREY", "BANNED", "LEMON", "PLUSHIE", "CHEETO",
		"TIKTOK", "YOUTUBE", "TWITCH", "DISCORD", "MODDER", "HACKER"
	};

	private static bool previouslyInLobby;

	private static readonly List<VRRig> nameSpoofRigs = new List<VRRig>();

	private static readonly List<VRRig> colorSpoofRigs = new List<VRRig>();

	public static int targetElo = 4000;

	public static int targetBadge = 7;

	public static void SpoofBadge(bool enable)
	{
		MenuPatches.RankedPatch.enabled = enable;
		if (MenuPatches.RankedPatch.enabled && (!Mathf.Approximately(VRRig.LocalRig.currentRankedELO, (float)targetElo) || VRRig.LocalRig.currentRankedSubTierQuest != targetBadge || VRRig.LocalRig.currentRankedSubTierPC != targetBadge))
		{
			VRRig.LocalRig.SetRankedInfo((float)targetElo, targetBadge, targetBadge, true);
		}
	}

	public static void ColorSpoof()
	{
		List<VRRig> list = new List<VRRig>();
		foreach (VRRig colorSpoofRig in colorSpoofRigs)
		{
			if (!VRRigCache.ActiveRigs.Contains(colorSpoofRig))
			{
				list.Add(colorSpoofRig);
			}
		}
		foreach (VRRig item in list)
		{
			colorSpoofRigs.Remove(item);
		}
		list.Clear();
		foreach (VRRig item2 in from rig in VRRigCache.ActiveRigs
			where !rig.isLocal
			where !colorSpoofRigs.Contains(rig)
			select rig)
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RigManager.GetNetPlayerFromVRRig(item2), new object[3]
			{
				UnityEngine.Random.Range(0f, 1f),
				UnityEngine.Random.Range(0f, 1f),
				UnityEngine.Random.Range(0f, 1f)
			});
			colorSpoofRigs.Add(item2);
		}
	}

	public static void SpoofPlatform(bool enabled, string target = null)
	{
		MenuPatches.RankedPatch.enabled = enabled;
		MenuPatches.RankedPatch.targetPlatform = target;
	}

	private static void PanicDisable()
	{
		NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "Panic Enabled, Features Disabled.");
		_panicSnapshot.Clear();
		ButtonHandler.Button[] buttons = ModButtons.buttons;
		foreach (ButtonHandler.Button button in buttons)
		{
			if (button == null || !button.Enabled || button.buttonText == PanicButtonText)
			{
				continue;
			}
			_panicSnapshot.Add(button);
			button.Enabled = false;
			Action onDisable = button.onDisable;
			if (onDisable != null)
			{
				onDisable();
			}
			NXOUI.RemoveMod(button.buttonText);
		}
		_panicked = true;
		Main.RefreshMenu();
	}

	public static void DisableVisualizeAntiReport()
	{
		if ((UnityEngine.Object)(object)radiusSphere != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)radiusSphere);
			radiusSphere = null;
		}
	}

	public static void AntiReport(bool autoQueue, bool reconnect)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		foreach (GorillaPlayerScoreboardLine current in GorillaScoreboardTotalUpdater.allScoreboardLines)
		{
			if (current.linePlayer != NetworkSystem.Instance.LocalPlayer || (UnityEngine.Object)(object)current.reportButton == (UnityEngine.Object)null)
			{
				continue;
			}
			Transform transform = ((Component)current.reportButton).gameObject.transform;
			float antiReportRadius = Settings.AntiReportRadius;
			foreach (VRRig current2 in VRRigCache.ActiveRigs)
			{
				if ((UnityEngine.Object)(object)current2 == (UnityEngine.Object)null || (UnityEngine.Object)(object)current2 == (UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig)
				{
					continue;
				}
				float num = Vector3.Distance(current2.rightHandTransform.position, transform.position);
				float num2 = Vector3.Distance(current2.leftHandTransform.position, transform.position);
				if (num < antiReportRadius || num2 < antiReportRadius)
				{
					NetworkSystem.Instance.ReturnToSinglePlayer();
					NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "`" + current2.Creator.NickName + "` Attempted to Report You!");
					if (autoQueue)
					{
						CoroutineHelper.InvokeAfterDelay(2f, Room.JoinRandomPublic);
					}
					else if (reconnect)
					{
						CoroutineHelper.InvokeAfterDelay(2f, Room.Reconnect);
					}
					return;
				}
			}
		}
	}

	public static void FakeLag()
	{
		bool flag = InputHandler.RPrimary();
		if (!wasButtonPressed && flag)
		{
			isOn = !isOn;
		}
		wasButtonPressed = flag;
		if (isOn)
		{
			if (Time.time > lagcooldown)
			{
				lagcooldown = Time.time + lagcooldownduration;
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = !((Behaviour)Variables.taggerInstance.offlineVRRig).enabled;
				lagcooldownduration = UnityEngine.Random.Range(0.1f, 0.4f);
			}
			return;
		}
		if ((UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig != (UnityEngine.Object)null)
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
		}
	}

	public static void RPCShield()
	{
		if (PhotonNetwork.InRoom)
		{
			((MonkeAgent)MonkeAgent.instance).rpcErrorMax = int.MaxValue;
			((MonkeAgent)MonkeAgent.instance).rpcCallLimit = int.MaxValue;
			((MonkeAgent)MonkeAgent.instance).logErrorMax = int.MaxValue;
			((MonkeAgent)MonkeAgent.instance).userRPCCalls.Clear();
			PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
			PhotonNetwork.QuickResends = int.MaxValue;
			PhotonNetwork.SendAllOutgoingCommands();
		}
	}

	public static void NameSpoof()
	{
		List<VRRig> list = new List<VRRig>();
		foreach (VRRig nameSpoofRig in nameSpoofRigs)
		{
			if (!VRRigCache.ActiveRigs.Contains(nameSpoofRig))
			{
				list.Add(nameSpoofRig);
			}
		}
		foreach (VRRig item in list)
		{
			nameSpoofRigs.Remove(item);
		}
		list.Clear();
		string nickName = PhotonNetwork.NickName;
		foreach (VRRig current3 in VRRigCache.ActiveRigs)
		{
			if (current3.isLocal || nameSpoofRigs.Contains(current3))
			{
				continue;
			}
			string text = (UnityEngine.Random.Range(0, 3) == 0) ? namePrefix[UnityEngine.Random.Range(0, namePrefix.Length)] : "";
			string text2 = (UnityEngine.Random.Range(0, 3) == 0) ? nameSuffix[UnityEngine.Random.Range(0, nameSuffix.Length)] : "";
			string text3 = text + names[UnityEngine.Random.Range(0, names.Length)] + text2;
			RigManager.ChangeName((text3.Length <= 12) ? text3 : text3.Substring(0, 12), noColor: true);
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RigManager.GetNetPlayerFromVRRig(current3), new object[3]
			{
				UnityEngine.Random.Range(0f, 1f),
				UnityEngine.Random.Range(0f, 1f),
				UnityEngine.Random.Range(0f, 1f)
			});
			nameSpoofRigs.Add(current3);
		}
		if (PhotonNetwork.NickName != nickName)
		{
			PhotonNetwork.NickName = nickName;
		}
	}

	public static void ChangeIdentity()
	{
		string text = "gorilla";
		int num = 0;
		if (num < 4)
		{
			do
			{
				text += UnityEngine.Random.Range(0, 9);
				num++;
			}
			while (num < 4);
		}
		RigManager.ChangeName(text);
		byte b = (byte)UnityEngine.Random.Range(0, 255);
		byte b2 = (byte)UnityEngine.Random.Range(0, 255);
		byte b3 = (byte)UnityEngine.Random.Range(0, 255);
		RigManager.ChangeColor((Color32)(new Color32(b, b2, b3, byte.MaxValue)));
	}

	public static void ChangeIdentityOnDisconnect(Action identityType)
	{
		if (!PhotonNetwork.InRoom && previouslyInLobby && identityType != null)
		{
			identityType();
			previouslyInLobby = PhotonNetwork.InRoom;
		}
		else
		{
			previouslyInLobby = PhotonNetwork.InRoom;
		}
	}

	public static void PanicReset()
	{
		if (_panicked)
		{
			PanicRestore();
		}
	}

	public static void BypassAutomod()
	{
		GorillaTagger.moderationMutedTime = -1f;
		if (((GorillaComputer)GorillaComputer.instance).autoMuteType != "OFF")
		{
			((GorillaComputer)GorillaComputer.instance).autoMuteType = "OFF";
			PlayerPrefs.SetInt("autoMute", 0);
			PlayerPrefs.Save();
		}
		Recorder primaryRecorder = NetworkSystem.Instance.VoiceConnection.PrimaryRecorder;
		if ((UnityEngine.Object)(object)primaryRecorder == (UnityEngine.Object)null)
		{
			return;
		}
		if ((int)primaryRecorder.SourceType == 1)
		{
			return;
		}
		float num = 0f;
		GorillaSpeakerLoudness component = ((Component)VRRig.LocalRig).GetComponent<GorillaSpeakerLoudness>();
		if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
		{
			num = component.Loudness;
		}
		if (num != 0f)
		{
			startSilenceTime = -1f;
			reloaded = false;
			lastVol = num;
			return;
		}
		if (lastVol != 0f)
		{
			startSilenceTime = Time.time;
			reloaded = false;
		}
		if (startSilenceTime > 0f && !reloaded && Time.time - startSilenceTime >= 0.25f)
		{
			primaryRecorder.RestartRecording(true);
			reloaded = true;
		}
		lastVol = num;
	}

	public static void VisualizeAntiReport()
	{
		if (!PhotonNetwork.InRoom)
		{
			if ((UnityEngine.Object)(object)radiusSphere != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)radiusSphere);
				radiusSphere = null;
			}
			return;
		}
		List<GorillaPlayerScoreboardLine>.Enumerator enumerator = GorillaScoreboardTotalUpdater.allScoreboardLines.GetEnumerator();
		while (enumerator.MoveNext())
		{
			GorillaPlayerScoreboardLine current = enumerator.Current;
			if (current.linePlayer != NetworkSystem.Instance.LocalPlayer || (UnityEngine.Object)(object)current.reportButton == (UnityEngine.Object)null)
			{
				continue;
			}
			Transform transform = ((Component)current.reportButton).gameObject.transform;
			if ((UnityEngine.Object)(object)radiusSphere == (UnityEngine.Object)null)
			{
				radiusSphere = GameObject.CreatePrimitive((PrimitiveType)0);
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)radiusSphere.GetComponent<Collider>());
				Renderer component = radiusSphere.GetComponent<Renderer>();
				component.material.shader = Shader.Find("GUI/Text Shader");
				component.material.color = new Color(1f, 0f, 0f, 0.3f);
				radiusSphere.transform.position = transform.position;
				radiusSphere.transform.localScale = Vector3.one * Settings.AntiReportRadius;
			}
			else
			{
				radiusSphere.transform.position = transform.position;
				radiusSphere.transform.localScale = Vector3.one * Settings.AntiReportRadius;
			}
			break;
		}
	}

	public static void BypassModCheckers()
	{
		Photon.Realtime.Player localPlayer = PhotonNetwork.LocalPlayer;
		if (localPlayer == null || localPlayer.CustomProperties == null || ((Dictionary<object, object>)(object)localPlayer.CustomProperties).Count == 0)
		{
			return;
		}
		ExitGames.Client.Photon.Hashtable val = new ExitGames.Client.Photon.Hashtable();
		IEnumerator<string> enumerator = (from keyObj in ((Dictionary<object, object>)(object)localPlayer.CustomProperties).Keys.ToList()
			select keyObj?.ToString() into key
			where key != null
			where !key.Equals("didTutorial")
			select key).GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				string current = enumerator.Current;
				val[(object)current] = null;
			}
			while (enumerator.MoveNext());
		}
		if (((Dictionary<object, object>)(object)val).Count > 0)
		{
			localPlayer.SetCustomProperties(val, (ExitGames.Client.Photon.Hashtable)null, (WebFlags)null);
		}
	}

	public static void AntiModerator()
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			if (current.HasCosmetic("LBAAK"))
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
				NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "`" + current.Creator.NickName + "` Is A Moderator");
				CoroutineHelper.InvokeAfterDelay(1.5f, Room.JoinRandomPublic);
				return;
			}
		}
	}

	private static void PanicRestore()
	{
		foreach (ButtonHandler.Button current in _panicSnapshot)
		{
			if (current == null)
			{
				continue;
			}
			current.Enabled = true;
			Action onEnable = current.onEnable;
			if (onEnable != null)
			{
				onEnable();
			}
			NXOUI.AddMod(current.buttonText);
		}
		_panicSnapshot.Clear();
		_panicked = false;
		NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "Panic Disabled, Features Restored.");
		Main.RefreshMenu();
	}

	public static void Panic()
	{
		bool panicPressedLast = default(bool);
		if (InputHandler.LPrimary() && !_panicPressedLast)
		{
			if (_panicked)
			{
				PanicRestore();
				_panicPressedLast = panicPressedLast;
			}
			else
			{
				PanicDisable();
				_panicPressedLast = panicPressedLast;
			}
		}
		else
		{
			_panicPressedLast = panicPressedLast;
		}
	}
}
