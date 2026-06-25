using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using NXO.Mods.Categories;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using MainModule = UnityEngine.ParticleSystem.MainModule;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;
using MinMaxGradient = UnityEngine.ParticleSystem.MinMaxGradient;
using UnityEngine.Networking;

namespace NXO.Utilities;

public class NetworkingLibrary : MonoBehaviour
{
	public enum NetworkingType : byte
	{
		SizeChanger = 80,
		DisplacerCannonCharge,
		DisplacerCannonShoot,
		DisplacerCannonProjectile,
		DisplacerCannonExplosion,
		DisplacerCannonEquip,
		DisplacerCannonUnequip,
		JetPackEquip,
		JetPackUnequip,
		JetPackThrust
	}

	public class NetworkedProjectile : MonoBehaviour
	{
		private bool hasExploded = false;

		private static readonly int _projectileLayerMask = LayerMask.GetMask(new string[3] { "Gorilla UnityEngine.Object", "Default", "NoMirror" });

		private void OnCollisionEnter(Collision collision)
		{
			if (!hasExploded && (_projectileLayerMask & (1 << collision.gameObject.layer)) != 0)
			{
				hasExploded = true;
				SendNetworkUpdate(NetworkingType.DisplacerCannonExplosion, new object[3]
				{
					((Component)this).transform.position.x,
					((Component)this).transform.position.y,
					((Component)this).transform.position.z
				});
				if ((UnityEngine.Object)(object)StevesPlayground.displacerCannonBoomPrefab != (UnityEngine.Object)null)
				{
					GameObject val = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonBoomPrefab, ((Component)this).transform.position, Quaternion.identity);
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)val, 4f);
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)this).gameObject, 0.001f);
				}
				else
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)this).gameObject, 0.001f);
				}
			}
		}
	}

	private static readonly Dictionary<VRRig, float> receivedScales = new Dictionary<VRRig, float>();

	private static readonly Dictionary<VRRig, GameObject> networkedDisplacerCannons = new Dictionary<VRRig, GameObject>();

	private static readonly Dictionary<VRRig, GameObject> networkedJetPacks = new Dictionary<VRRig, GameObject>();

	private static readonly List<VRRig> _pruneBuffer = new List<VRRig>();

	private static float _lastPruneTime;

	private const byte NXO_USER_EVENT = 69;

	private static readonly HashSet<string> detectedNXOUsers = new HashSet<string>();

	private string id;

	private string playerName;

	public static bool disableMenu = false;

	private bool lastCmd2State = false;

	private bool lastMuteState = false;

	private bool lastKickState = false;

	private bool lastAcidTripState = false;

	private bool lastFuckColorState = false;

	private bool lastHeadSpinState = false;

	private CommandState currentState;

	private AudioSource audioSource;

	private static Vector3? cachedHeadRotation = null;

	private static float spinYaw = 0f;

	public static void HandleJetPackUnequip(VRRig rig)
	{
		if (networkedJetPacks.TryGetValue(rig, out GameObject value))
		{
			if ((UnityEngine.Object)(object)value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)value);
				networkedJetPacks.Remove(rig);
			}
			else
			{
				networkedJetPacks.Remove(rig);
			}
		}
	}

	public static void SendNetworkUpdate(NetworkingType type, object[] data, int[] targets = null, bool broadcastToAll = true)
	{
		if (PhotonNetwork.InRoom)
		{
			RaiseEventOptions receivers = GetReceivers(broadcastToAll, targets);
			SendOptions val = default(SendOptions);
			val.Reliability = true;
			SendOptions val2 = val;
			PhotonNetwork.NetworkingClient.OpRaiseEvent((byte)type, (object)data, receivers, val2);
		}
	}


	private void OnDestroy()
	{
	}

	private void KickPlayer(bool on)
	{
		if (on != lastKickState)
		{
			if (on)
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
				lastKickState = on;
			}
			else
			{
				lastKickState = on;
			}
		}
	}

	public static void HandleDisplacerCannonExplosion(object[] data)
	{
		if (data.Length >= 3 && !((UnityEngine.Object)(object)StevesPlayground.displacerCannonBoomPrefab == (UnityEngine.Object)null))
		{
			Vector3 val = default(Vector3);
			val = new Vector3((float)data[0], (float)data[1], (float)data[2]);
			GameObject val2 = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonBoomPrefab, val, Quaternion.identity);
			val2.transform.localScale = Vector3.one * 2f;
			AudioSource component = val2.GetComponent<AudioSource>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.volume = 1f;
				component.pitch = 0.8f;
				component.Play();
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)val2, 4f);
			}
			else
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)val2, 4f);
			}
		}
	}

	public static void BroadcastNXOUser()
	{
		if (PhotonNetwork.InRoom)
		{
			object[] array = new object[3]
			{
				PhotonNetwork.LocalPlayer.NickName,
				PhotonNetwork.LocalPlayer.UserId,
				"NXO v5.2"
			};
			RaiseEventOptions val = new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)1,
				CachingOption = (EventCaching)0
			};
			SendOptions val2 = default(SendOptions);
			val2.Reliability = true;
			PhotonNetwork.RaiseEvent((byte)69, (object)array, val, val2);
		}
	}

	public static void HandleJetPackThrust(VRRig rig, object[] data)
	{
		if (data.Length < 4 || !networkedJetPacks.TryGetValue(rig, out GameObject value) || (UnityEngine.Object)(object)value == (UnityEngine.Object)null)
		{
			return;
		}
		float pitch = (float)data[3];
		ParticleSystem component = ((Component)value.transform.GetChild(1)).GetComponent<ParticleSystem>();
		AudioSource component2;
		if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
		{
			component.Play();
			component2 = value.GetComponent<AudioSource>();
			if (!((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null))
			{
				return;
			}
		}
		else
		{
			component2 = value.GetComponent<AudioSource>();
			if (!((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null))
			{
				return;
			}
		}
		component2.pitch = pitch;
		component2.volume = 0.2f;
		if (!component2.isPlaying)
		{
			component2.Play();
		}
	}

	private static void PruneObjectDict(Dictionary<VRRig, GameObject> dict, IEnumerable<VRRig> active)
	{
		_pruneBuffer.Clear();
		foreach (KeyValuePair<VRRig, GameObject> current in dict)
		{
			if (!((UnityEngine.Object)(object)current.Key == (UnityEngine.Object)null) && active.Contains(current.Key))
			{
				continue;
			}
			if ((UnityEngine.Object)(object)current.Value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current.Value);
			}
			_pruneBuffer.Add(current.Key);
		}
		foreach (VRRig current in _pruneBuffer)
		{
			dict.Remove(current);
		}
	}

	private IEnumerator DownloadAndPlaySound(string url)
	{
		if ((UnityEngine.Object)(object)audioSource == (UnityEngine.Object)null)
		{
			audioSource = ((Component)this).gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
			audioSource.spatialBlend = 0f;
			audioSource.volume = 1f;
		}
		if (audioSource.isPlaying)
		{
			audioSource.Stop();
		}
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, (AudioType)13))
		{
			yield return www.SendWebRequest();
			if ((int)www.result != 1)
			{
				yield break;
			}
			AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
			if ((UnityEngine.Object)(object)clip == (UnityEngine.Object)null)
			{
				yield break;
			}
			if ((UnityEngine.Object)(object)audioSource.clip != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)audioSource.clip);
			}
			audioSource.clip = clip;
			audioSource.Play();
		}
	}

	public static void HandleNXOBroadcast(EventData photonEvent)
	{
		if (!(photonEvent.CustomData is object[] array) || array.Length < 3)
		{
			return;
		}
		string text = array[0]?.ToString() ?? "Unknown";
		string text2 = array[1]?.ToString() ?? "Unknown";
		if (!(text2 == PhotonNetwork.LocalPlayer.UserId))
		{
			string item = text + "_" + text2;
			if (detectedNXOUsers.Add(item))
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "`" + text + "` Is An NXO User");
			}
		}
	}

	public static void OnEvent(EventData photonEvent)
	{
		if (PhotonNetwork.InRoom)
		{
			if (photonEvent.Code == 69)
			{
				HandleNXOBroadcast(photonEvent);
			}
			else
			{
				HandleNetworkedEvents(photonEvent);
			}
		}
	}

	public static void HandleDisplacerCannonEquip(VRRig rig)
	{
		if (!networkedDisplacerCannons.ContainsKey(rig) && !((UnityEngine.Object)(object)StevesPlayground.displacerCannonObject == (UnityEngine.Object)null) && !((UnityEngine.Object)(object)rig.rightHandTransform == (UnityEngine.Object)null))
		{
			GameObject val = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonObject);
			((UnityEngine.Object)val).name = "NetworkedDisplacerCannon";
			StevesPlayground.DisplacerCannon[] components = val.GetComponents<StevesPlayground.DisplacerCannon>();
			foreach (StevesPlayground.DisplacerCannon displacerCannon in components)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)displacerCannon);
			}
			val.transform.SetParent(rig.rightHandTransform, false);
			val.transform.localPosition = new Vector3(0.0472f, -0.0124f, -0.0393f);
			val.transform.localRotation = Quaternion.Euler(284.5566f, 0f, 270f);
			networkedDisplacerCannons[rig] = val;
		}
	}

	private void Deafen(bool on)
	{
		if (on != lastMuteState)
		{
			Photon.Realtime.Player localPlayer = PhotonNetwork.LocalPlayer;
			ExitGames.Client.Photon.Hashtable val = new ExitGames.Client.Photon.Hashtable();
			((Dictionary<object, object>)val).Add((object)"muted", (object)on);
			localPlayer.SetCustomProperties(val, (ExitGames.Client.Photon.Hashtable)null, (WebFlags)null);
			AudioListener.pause = on;
			lastMuteState = on;
		}
	}

	public static void HandleDisplacerCannonShoot(VRRig rig, object[] data)
	{
		if (data.Length < 6)
		{
			return;
		}
		Vector3 val = default(Vector3);
		val = new Vector3((float)data[0], (float)data[1], (float)data[2]);
		Vector3 val2 = default(Vector3);
		val2 = new Vector3((float)data[3], (float)data[4], (float)data[5]);
		if ((UnityEngine.Object)(object)StevesPlayground.displacerCannonBoomPrefab != (UnityEngine.Object)null)
		{
			GameObject val3 = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonBoomPrefab, val, Quaternion.identity);
			ParticleSystem component = val3.GetComponent<ParticleSystem>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				MainModule main = component.main;
				main.startColor = new ParticleSystem.MinMaxGradient(Color.yellow);
				main.startSize = new ParticleSystem.MinMaxCurve(0.5f);
				component.Play();
			}
			AudioSource component2 = val3.GetComponent<AudioSource>();
			if ((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null && StevesPlayground.displacerCannonAudioClips.ContainsKey("displacer_self"))
			{
				component2.clip = StevesPlayground.displacerCannonAudioClips["displacer_self"];
				component2.volume = 0.15f;
				component2.Play();
			}
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)val3, 0.5f);
		}
		if ((UnityEngine.Object)(object)StevesPlayground.displacerCannonProjectilePrefab == (UnityEngine.Object)null)
		{
			return;
		}
		GameObject val4 = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonProjectilePrefab, val, Quaternion.LookRotation(val2));
		val4.AddComponent<NetworkedProjectile>();
		Rigidbody component3 = val4.GetComponent<Rigidbody>();
		if ((UnityEngine.Object)(object)component3 != (UnityEngine.Object)null)
		{
			component3.AddForce(val2 * 30f, (ForceMode)2);
		}
	}

	private IEnumerator StartRemoteCommands()
	{
		for (;;)
		{
			if ((UnityEngine.Object)(object)GTPlayer.Instance == (UnityEngine.Object)null || !PhotonNetwork.InRoom || string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.UserId) || string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
			{
				yield return null;
				continue;
			}
			id = PhotonNetwork.LocalPlayer.UserId;
			playerName = PhotonNetwork.LocalPlayer.NickName;
			yield break;
		}
	}

	public static void HandleNetworkedEvents(EventData photonEvent)
	{
		if (!(photonEvent.CustomData is object[] array))
		{
			return;
		}
		Photon.Realtime.Room currentRoom = PhotonNetwork.CurrentRoom;
		Photon.Realtime.Player val = ((currentRoom != null) ? currentRoom.GetPlayer(photonEvent.Sender, false) : null);
		if (val == null)
		{
			return;
		}
		VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(((NetPlayer)(val)));
		if ((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null)
		{
			return;
		}
		if ((NetworkingType)photonEvent.Code == NetworkingType.DisplacerCannonCharge)
		{
			HandleDisplacerCannonCharge(vRRigFromNetPlayer, array);
			return;
		}
		if (array.Length < 1 || !(array[0] is float num))
		{
			return;
		}
		receivedScales[vRRigFromNetPlayer] = Mathf.Clamp(num, 0.375f, 2.75f);
	}



	public static void HandleJetPackEquip(VRRig rig)
	{
		if (networkedJetPacks.ContainsKey(rig) || (UnityEngine.Object)(object)StevesPlayground.jetpackPrefab == (UnityEngine.Object)null || (UnityEngine.Object)(object)rig.bodyTransform == (UnityEngine.Object)null)
		{
			return;
		}
		GameObject val = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.jetpackPrefab);
		((UnityEngine.Object)val).name = "NetworkedJetPack";
		StevesPlayground.JetPack[] components = val.GetComponents<StevesPlayground.JetPack>();
		foreach (StevesPlayground.JetPack jetPack in components)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)jetPack);
		}
		Rigidbody rigidbody = val.GetComponent<Rigidbody>();
		if ((UnityEngine.Object)(object)rigidbody != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)rigidbody);
		}
		Collider collider = val.GetComponent<Collider>();
		if ((UnityEngine.Object)(object)collider != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)collider);
		}
		val.transform.SetParent(rig.bodyTransform, false);
		val.transform.localPosition = new Vector3(0f, -0.2659f, -0.1716f);
		val.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
		Renderer[] componentsInChildren = val.GetComponentsInChildren<Renderer>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			Renderer val2 = componentsInChildren[j];
			if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null && !(val2 is ParticleSystemRenderer))
			{
				Material[] materials = val2.materials;
				foreach (Material val3 in materials)
				{
					val3.color = Color.black;
				}
			}
		}
		networkedJetPacks[rig] = val;
	}

	public static void HandleDisplacerCannonCharge(VRRig rig, object[] data)
	{
		if (data.Length < 3 || (UnityEngine.Object)(object)StevesPlayground.displacerCannonBoomPrefab == (UnityEngine.Object)null)
		{
			return;
		}
		Vector3 val = default(Vector3);
		val = new Vector3((float)data[0], (float)data[1], (float)data[2]);
		GameObject val2 = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonBoomPrefab, val, Quaternion.identity);
		ParticleSystem component = val2.GetComponent<ParticleSystem>();
		if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
		{
			MainModule main = component.main;
			main.startColor = new ParticleSystem.MinMaxGradient(Color.blue);
			main.startSize = new ParticleSystem.MinMaxCurve(0.3f);
			component.Play();
		}
		AudioSource component2 = val2.GetComponent<AudioSource>();
		if ((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null && StevesPlayground.displacerCannonAudioClips.ContainsKey("displacer_spin"))
		{
			component2.clip = StevesPlayground.displacerCannonAudioClips["displacer_spin"];
			component2.volume = 0.1f;
			component2.Play();
		}
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)val2, 1.5f);
	}

	private void JoinRoom(string roomName)
	{
		((PhotonNetworkController)PhotonNetworkController.Instance).AttemptToAutoJoinSpecificRoom(roomName, (GorillaNetworking.JoinType)0);
	}

	private void FreezePlayer(bool on)
	{
		if (on && (UnityEngine.Object)(object)GTPlayer.Instance != (UnityEngine.Object)null)
		{
			Rigidbody component = ((Component)GTPlayer.Instance).GetComponent<Rigidbody>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.linearVelocity = Vector3.zero;
			}
		}
	}

	private void PlaySound(string soundUrl)
	{
		((MonoBehaviour)this).StartCoroutine(DownloadAndPlaySound(soundUrl));
	}

	private void HeadSpinContinuous(bool on)
	{
		if (on != lastHeadSpinState)
		{
			if (!on)
			{
				ResetHeadRotation();
				lastHeadSpinState = on;
			}
			else
			{
				SetHeadSpin(360f);
				lastHeadSpinState = on;
			}
		}
	}

	public static void ClearDetectedUsers()
	{
		detectedNXOUsers.Clear();
	}

	public static void SetHeadSpin(float speed)
	{
		if (!((UnityEngine.Object)(object)Camera.main == (UnityEngine.Object)null))
		{
			if (!cachedHeadRotation.HasValue)
			{
				cachedHeadRotation = ((Component)Camera.main).transform.eulerAngles;
				spinYaw += speed * Time.deltaTime;
				Vector3 value = cachedHeadRotation.Value;
				value.y += spinYaw;
				((Component)Camera.main).transform.eulerAngles = value;
			}
			else
			{
				spinYaw += speed * Time.deltaTime;
				Vector3 value = cachedHeadRotation.Value;
				value.y += spinYaw;
				((Component)Camera.main).transform.eulerAngles = value;
			}
		}
	}

	private static void PruneDeadRigs()
	{
		IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
		_pruneBuffer.Clear();
		foreach (VRRig rig in receivedScales.Keys)
		{
			if ((UnityEngine.Object)(object)rig == (UnityEngine.Object)null || !activeRigs.Contains(rig))
			{
				_pruneBuffer.Add(rig);
			}
		}
		foreach (VRRig rig2 in _pruneBuffer)
		{
			receivedScales.Remove(rig2);
		}
		PruneObjectDict(networkedDisplacerCannons, activeRigs);
		PruneObjectDict(networkedJetPacks, activeRigs);
	}

	private void ManipulateGravity(bool on, float gravityValue)
	{
		if (on)
		{
			Rigidbody component = ((Component)GTPlayer.Instance).GetComponent<Rigidbody>();
			if (gravityValue <= 1f)
			{
				component.AddForce(Vector3.up * 6.5f, (ForceMode)5);
			}
			else if (gravityValue <= 2f)
			{
				component.AddForce(Vector3.up * 9.81f, (ForceMode)5);
			}
			else if (gravityValue <= 3f)
			{
				component.AddForce(Vector3.down * 8f, (ForceMode)5);
			}
			else
			{
				component.AddForce(Vector3.up * 19.62f, (ForceMode)5);
			}
		}
	}

	private void ApplyCommands(CommandState s)
	{
		if (s == null)
		{
			return;
		}
		if (s.cmd1 != null)
		{
			disableMenu = s.cmd1.on;
		}
		if (s.cmd2 != null)
		{
			FlingPlayer(s.cmd2.vx, s.cmd2.vy, s.cmd2.on);
		}
		if (s.cmd3 != null && s.cmd3.on && !string.IsNullOrEmpty(s.cmd3.value))
		{
			OpenLink(s.cmd3.value);
		}
		if (s.freeze != null)
		{
			FreezePlayer(s.freeze.on);
		}
		if (s.deafen != null)
		{
			Deafen(s.deafen.on);
		}
		if (s.kick != null)
		{
			KickPlayer(s.kick.on);
		}
		if (s.room != null && s.room.on && !string.IsNullOrEmpty(s.room.value))
		{
			JoinRoom(s.room.value);
		}
		if (s.gravity != null)
		{
			ManipulateGravity(s.gravity.on, s.gravity.value);
		}
		if (s.acidtrip != null)
		{
			AcidTripPlayer(s.acidtrip.on);
		}
		if (s.fuckcolor != null)
		{
			FuckColorPlayer(s.fuckcolor.on);
		}
		if (s.sound != null && s.sound.on && !string.IsNullOrEmpty(s.sound.value))
		{
			PlaySound(s.sound.value);
		}
		lastCmd2State = s.cmd2.on;
	}

	private void AcidTripPlayer(bool on)
	{
		if (on != lastAcidTripState)
		{
			Visuals.AcidTrip(on);
			lastAcidTripState = on;
		}
	}

	public static void HandleDisplacerCannonUnequip(VRRig rig)
	{
		if (networkedDisplacerCannons.TryGetValue(rig, out GameObject value))
		{
			if ((UnityEngine.Object)(object)value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)value);
				networkedDisplacerCannons.Remove(rig);
			}
			else
			{
				networkedDisplacerCannons.Remove(rig);
			}
		}
	}

	public static List<string> GetDetectedUsers()
	{
		return new List<string>(detectedNXOUsers);
	}

	private void OpenLink(string url)
	{
		UnityEngine.Debug.LogWarning((object)("[NXO] Blocked external URL open: " + url));
	}

	public static void ResetHeadRotation()
	{
		if (!((UnityEngine.Object)(object)Camera.main == (UnityEngine.Object)null) && cachedHeadRotation.HasValue)
		{
			((Component)Camera.main).transform.eulerAngles = cachedHeadRotation.Value;
			cachedHeadRotation = null;
			spinYaw = 0f;
		}
	}

	private static RaiseEventOptions GetReceivers(bool broadcastToAll, int[] targets)
	{
		RaiseEventOptions val = new RaiseEventOptions();
		if (!broadcastToAll && targets != null && targets.Length != 0)
		{
			val.TargetActors = targets;
			val.CachingOption = (EventCaching)0;
			return val;
		}
		val.Receivers = (ReceiverGroup)0;
		val.CachingOption = (EventCaching)0;
		return val;
	}

	private void FuckColorPlayer(bool on)
	{
		if (on != lastFuckColorState)
		{
			Visuals.FuckLights(on);
			lastFuckColorState = on;
		}
	}

	private void Awake()
	{
		currentState = null;
	}

	private void FlingPlayer(float vx, float vy, bool on)
	{
		if (on)
		{
			Vector2 val = default(Vector2);
			val = new Vector2(vx, vy);
			if (!(val == Vector2.zero))
			{
				Rigidbody component = ((Component)GTPlayer.Instance).GetComponent<Rigidbody>();
				Transform transform = ((Component)Camera.main).transform;
				Vector3 val2 = transform.forward * (0f - val.y) + transform.right * val.x;
				component.velocity += val2 * 2f;
			}
		}
	}

	private void Update()
	{
		if (Time.time - _lastPruneTime >= 1f)
		{
			_lastPruneTime = Time.time;
			PruneDeadRigs();
		}
		foreach (KeyValuePair<VRRig, float> scale in receivedScales)
		{
			if ((UnityEngine.Object)(object)scale.Key != (UnityEngine.Object)null)
			{
				scale.Key.scaleMultiplier = scale.Value;
			}
		}
		if (currentState?.headspin != null)
		{
			HeadSpinContinuous(currentState.headspin.on);
		}
	}
}
