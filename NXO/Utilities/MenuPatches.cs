using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaNetworking.Store;
using HarmonyLib;
using NXO.Mods.Categories;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using UnityEngine;
using MaterialData = GorillaLocomotion.GTPlayer.MaterialData;

namespace NXO.Utilities;

[HarmonyPatch]
public class MenuPatches
{
	[HarmonyPatch(typeof(VRRig), "PostTick")]
	public class RigAnimationPatch
	{
		public static Action OnPostTick;

		[HarmonyPostfix]
		private static void Postfix(VRRig __instance)
		{
			if (__instance.isLocal)
			{
				OnPostTick?.Invoke();
			}
		}
	}

	[HarmonyPatch(typeof(Slingshot), "GetLaunchVelocity")]
	public class SlingshotAimbot
	{
		public static bool enabled;

		[HarmonyPostfix]
		public static void Postfix(Slingshot __instance, ref Vector3 __result)
		{
			if (!enabled)
			{
				return;
			}
			if (!((TransferrableObject)__instance).InLeftHand())
			{
				if (InputHandler.RTrigger())
				{
					return;
				}
			}
			else if (InputHandler.LTrigger())
			{
				return;
			}
			List<NetPlayer> excludedPlayers = new List<NetPlayer>();
			VRRig val3;
			if (PhotonNetwork.InRoom && (UnityEngine.Object)(object)GorillaGameManager.instance != (UnityEngine.Object)null)
			{
				GameModeType val = GorillaGameManager.instance.GameType();
				int num = (int)(val - 1);
				num = num - (num - 11) * (((uint)num > 10u) ? 1 : 0) + 29;
				int num2 = num;
				if (num2 != 30)
				{
					GorillaTagManager val2 = (GorillaTagManager)GorillaGameManager.instance;
					if (val2.isCurrentlyTag)
					{
						excludedPlayers.Add(val2.currentIt);
					}
					else
					{
						excludedPlayers.AddRange(val2.currentInfected);
					}
					Transform head = ((Component)Variables.taggerInstance.headCollider).transform;
					val3 = (from rig in VRRigCache.ActiveRigs
						where !rig.isLocal && (UnityEngine.Object)(object)rig != (UnityEngine.Object)null
						where !excludedPlayers.Contains(RigManager.GetNetPlayerFromVRRig(rig))
						select rig).OrderBy(delegate(VRRig rig)
					{
						Vector3 val8 = ((Component)rig).transform.position - head.position;
						Vector3 normalized = val8.normalized;
						return Vector3.Angle(head.forward, normalized) + Vector3.Distance(head.position, ((Component)rig).transform.position) * 0.1f;
					}).FirstOrDefault();
					if ((UnityEngine.Object)(object)val3 == (UnityEngine.Object)null)
					{
						return;
					}
				}
				else
				{
					Transform head = ((Component)Variables.taggerInstance.headCollider).transform;
					val3 = (from rig in VRRigCache.ActiveRigs
						where !rig.isLocal && (UnityEngine.Object)(object)rig != (UnityEngine.Object)null
						where !excludedPlayers.Contains(RigManager.GetNetPlayerFromVRRig(rig))
						select rig).OrderBy(delegate(VRRig rig)
					{
						Vector3 val8 = ((Component)rig).transform.position - head.position;
						Vector3 normalized = val8.normalized;
						return Vector3.Angle(head.forward, normalized) + Vector3.Distance(head.position, ((Component)rig).transform.position) * 0.1f;
					}).FirstOrDefault();
					if ((UnityEngine.Object)(object)val3 == (UnityEngine.Object)null)
					{
						return;
					}
				}
			}
			else
			{
				Transform head = ((Component)Variables.taggerInstance.headCollider).transform;
				val3 = (from rig in VRRigCache.ActiveRigs
					where !rig.isLocal && (UnityEngine.Object)(object)rig != (UnityEngine.Object)null
					where !excludedPlayers.Contains(RigManager.GetNetPlayerFromVRRig(rig))
					select rig).OrderBy(delegate(VRRig rig)
				{
					Vector3 val8 = ((Component)rig).transform.position - head.position;
					Vector3 normalized = val8.normalized;
					return Vector3.Angle(head.forward, normalized) + Vector3.Distance(head.position, ((Component)rig).transform.position) * 0.1f;
				}).FirstOrDefault();
				if ((UnityEngine.Object)(object)val3 == (UnityEngine.Object)null)
				{
					return;
				}
			}
			Vector3 position = val3.headMesh.transform.position;
			Vector3 val4 = val3.LatestVelocity();
			val4.y *= 0.33f;
			Vector3 position2 = ((Component)__instance.center).transform.position;
			float num3 = Vector3.Distance(position2, position) / 20f;
			Vector3 val5 = position + val4 * num3;
			Vector3 val6 = val5 - position2;
			Vector3 val7 = default(Vector3);
			val7 = new Vector3(val6.x, 0f, val6.z);
			float magnitude = val7.magnitude;
			float y = val6.y;
			float num4 = 0f - Physics.gravity.y;
			float num5 = Mathf.Sqrt(num4 * (y + Mathf.Sqrt(magnitude * magnitude + y * y)));
			float num6 = num5 * 2.5f;
			float num7 = num6 * num6;
			float num8 = num7 * num7 - num4 * (num4 * magnitude * magnitude + 2f * y * num7);
			if (num8 <= 0f)
			{
				__result = val6.normalized * num6;
				return;
			}
			float num9 = Mathf.Atan((num7 - Mathf.Sqrt(num8)) / (num4 * magnitude));
			__result = val7.normalized * Mathf.Cos(num9) * num6 + Vector3.up * Mathf.Sin(num9) * num6;
		}
	}

	[HarmonyPatch(typeof(PhotonNetworkController), "OnJoinedRoom")]
	public class JoinedRoomPatch
	{
		public static bool enabled;

		[HarmonyPrefix]
		private static void Prefix()
		{
			if (enabled)
			{
				((PhotonNetworkController)PhotonNetworkController.Instance).currentJoinType = (GorillaNetworking.JoinType)6;
			}
		}
	}

	[HarmonyPatch(typeof(PhotonNetworkController), "AttemptToJoinRankedPublicRoom")]
	public class RankedPatch
	{
		public static bool enabled;

		public static string targetPlatform;

		public static string targetTier;

		[HarmonyPrefix]
		public static bool Prefix(GorillaNetworkJoinTrigger triggeredTrigger, GorillaNetworking.JoinType roomJoinType = (GorillaNetworking.JoinType)0)
		{
			if (enabled)
			{
				((PhotonNetworkController)PhotonNetworkController.Instance).AttemptToJoinRankedPublicRoomAsync(triggeredTrigger, targetTier ?? ((object)RankedProgressionManager.Instance.GetRankedMatchmakingTier()).ToString(), targetPlatform ?? "PC", roomJoinType);
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(GorillaWrappedSerializer), "FailedToSpawn")]
	public class FailedToSpawn
	{
		public static bool enabled;

		[HarmonyPrefix]
		public static bool Prefix(object __instance)
		{
			if (!enabled)
			{
				return true;
			}
			((Component)__instance).gameObject.SetActive(false);
			return false;
		}
	}

	[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnDisconnected")]
	public class RateLimitPatch
	{
		[HarmonyPostfix]
		private static void Postfix(DisconnectCause cause)
		{
			SerializationPatch.Override = null;
			if ((UnityEngine.Object)(object)VRRig.LocalRig != (UnityEngine.Object)null && !((Behaviour)VRRig.LocalRig).enabled)
			{
				((Behaviour)VRRig.LocalRig).enabled = true;
			}
			if ((UnityEngine.Object)(object)Variables.taggerInstance?.offlineVRRig != (UnityEngine.Object)null && !((Behaviour)Variables.taggerInstance.offlineVRRig).enabled)
			{
				((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
			}
			if ((int)cause == 16 || (int)cause == 17 || (int)cause == 7)
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "RPC Kicked");
			}
		}
	}

	[HarmonyPatch(typeof(NewMapsDisplay), "UpdateSlideshow")]
	public class NewMapsDisplayPatch
	{
		[HarmonyPrefix]
		private static bool Prefix()
		{
			return false;
		}
	}

	[HarmonyPatch(typeof(PhotonNetwork), "RunViewUpdate")]
	public class SerializationPatch
	{
		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action m_OnSerialization;

		public static Func<bool> Override;

		public static event Action OnSerialization
		{
			[CompilerGenerated]
			add
			{
				Action onSerialization = SerializationPatch.m_OnSerialization;
				Action action = onSerialization;
				Action action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				onSerialization = Interlocked.CompareExchange(ref SerializationPatch.m_OnSerialization, value2, action2);
				action = onSerialization;
				if ((object)action != action2)
				{
					do
					{
						action2 = action;
						value2 = (Action)Delegate.Combine(action2, value);
						action = Interlocked.CompareExchange(ref SerializationPatch.m_OnSerialization, value2, action2);
					}
					while ((object)action != action2);
				}
			}
			[CompilerGenerated]
			remove
			{
				Action onSerialization = SerializationPatch.m_OnSerialization;
				Action action = onSerialization;
				Action action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				onSerialization = Interlocked.CompareExchange(ref SerializationPatch.m_OnSerialization, value2, action2);
				action = onSerialization;
				if ((object)action != action2)
				{
					do
					{
						action2 = action;
						value2 = (Action)Delegate.Remove(action2, value);
						action = Interlocked.CompareExchange(ref SerializationPatch.m_OnSerialization, value2, action2);
					}
					while ((object)action != action2);
				}
			}
		}

		[HarmonyPrefix]
		private static bool Prefix()
		{
			bool result;
			if (!PhotonNetwork.InRoom)
			{
				result = true;
			}
			else
			{
				int num = 9;
				Action onSerialization = SerializationPatch.m_OnSerialization;
				if (onSerialization == null)
				{
					num = 10;
				}
				else
				{
					onSerialization();
				}
				if (num != 10)
				{
				}
				result = Override == null || Override();
			}
			return result;
		}
	}

	public static bool handtapEnabled;

	public static bool tapsEnabled = true;

	public static bool doOverride;

	public static float overrideVolume = 99999f;

	public static float overrideSpeed = 99999f;

	public static int tapMultiplier = 1;

	public static bool forceEnabled;

	public static bool bypassPositionCheck;

	public static bool AntiCheatNotifications = false;

	public static bool kidEnabled;

	public static bool knockbackEnabled;

	public static bool micEnabled;

	public static bool snowballEnabled;

	public static bool autoBig;

	public static bool propertiesEnabled;

	public static bool tosEnabled;

	public static bool cosmeticsEnabled;

	public static bool cosmeticsInitialized;

	public static string cosmeticsOwned;

	[HarmonyPatch(typeof(LegalAgreements), "Update")]
	[HarmonyPrefix]
	private static bool LegalAgreementsUpdate(LegalAgreements __instance)
	{
		if (!tosEnabled)
		{
			return true;
		}
		((ControllerInputPoller)ControllerInputPoller.instance).leftControllerPrimary2DAxis.y = -1f;
		__instance.scrollSpeed = 10f;
		__instance._maxScrollSpeed = 10f;
		return false;
	}

	[HarmonyPatch(typeof(GorillaNetworkPublicTestsJoin), "GracePeriod")]
	[HarmonyPrefix]
	private static bool GracePeriod1Prefix()
	{
		return false;
	}

    [HarmonyPatch(typeof(MonkeAgent), "QuitDelay")]
    [HarmonyPrefix]
	private static bool QuitDelayPrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(BundleManager), "CheckIfBundlesOwned")]
	[HarmonyPostfix]
	private static void CheckIfBundlesOwnedPostfix()
	{
		cosmeticsInitialized = true;
		cosmeticsOwned = ((CosmeticsController)CosmeticsController.instance).concatStringCosmeticsAllowed;
	}

	[HarmonyPatch(typeof(ModIOTermsOfUse_v1), "PostUpdate")]
	[HarmonyPrefix]
	private static bool ModIOTermsPostUpdate(ModIOTermsOfUse_v1 __instance)
	{
		if (!tosEnabled)
		{
			return true;
		}
		__instance.TurnPage(999);
		((ControllerInputPoller)ControllerInputPoller.instance).leftControllerPrimary2DAxis.y = -1f;
		__instance.holdTime = 0.1f;
		return false;
	}

	[HarmonyPatch(typeof(PlayFabClientInstanceAPI), "ReportDeviceInfo")]
	[HarmonyPrefix]
	private static bool ReportDeviceInfoInstancePrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(MonkeAgent), "LogErrorCount")]
	[HarmonyPrefix]
	private static bool LogErrorCountPatch(string logString, string stackTrace, LogType type)
	{
		return false;
	}

	[HarmonyPatch(typeof(MonkeAgent), "DispatchReport")]
	[HarmonyPrefix]
	private static bool DispatchReportPatch()
	{
		return false;
	}

	[HarmonyPatch(typeof(Photon.Realtime.Player), "SetCustomProperties")]
	[HarmonyPrefix]
	public static bool SetCustomProperties(Photon.Realtime.Player __instance, ref ExitGames.Client.Photon.Hashtable propertiesToSet)
	{
		if (__instance.IsLocal && propertiesEnabled && ((IEnumerable<KeyValuePair<object, object>>)propertiesToSet).Any((KeyValuePair<object, object> prop) => prop.Key.ToString() != "didTutorial"))
		{
			return false;
		}
		return true;
	}
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.OnDisable))]
    public class OnDisable
    {
        public static bool Prefix(VRRig __instance) =>
            !__instance.isLocal;
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.Awake))]
    public class Awake
    {
        public static bool Prefix(VRRig __instance) =>
            __instance.gameObject.name != "Local Gorilla Player(Clone)";
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
    public class PostTick
    {
        public static bool Prefix(VRRig __instance) =>
            !__instance.isLocal || __instance.enabled;
    }


    [HarmonyPatch(typeof(GorillaNetworkPublicTestJoin2), "GracePeriod")]
	[HarmonyPrefix]
	private static bool GracePeriod2Prefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(Photon.Realtime.Player), "set_CustomProperties")]
	[HarmonyPrefix]
	public static bool set_CustomProperties(Photon.Realtime.Player __instance, ref ExitGames.Client.Photon.Hashtable value)
	{
		if (__instance.IsLocal && propertiesEnabled && ((IEnumerable<KeyValuePair<object, object>>)value).Any((KeyValuePair<object, object> prop) => prop.Key.ToString() != "didTutorial"))
		{
			return false;
		}
		return true;
	}

    [HarmonyPatch(typeof(PlayFabClientInstanceAPI), "ReportPlayer")]
    [HarmonyPrefix]
	private static bool ReportPlayerInstancePrefix(ReportPlayerClientRequest request, Action<ReportPlayerClientResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		return false;
	}

	[HarmonyPatch(typeof(GorillaSpeakerLoudness), "UpdateLoudness")]
	[HarmonyPrefix]
	private static bool MicPrefix(GorillaSpeakerLoudness __instance, ref bool ___isMicEnabled, ref bool ___isSpeaking, ref float ___loudness)
	{
		return !micEnabled || ((UnityEngine.Object)((Component)__instance).gameObject).name != "Local Gorilla Player";
	}

	[HarmonyPatch(typeof(GorillaGameManager), "ForceStopGame_DisconnectAndDestroy")]
	[HarmonyPrefix]
	private static bool ForceStopGamePrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(ForceVolume), "OnTriggerStay")]
	[HarmonyPrefix]
	public static bool OnTriggerStayPatch()
	{
		return !forceEnabled;
	}

	[HarmonyPatch(typeof(VRRig), "IsPositionInRange")]
	[HarmonyPostfix]
	public static void Postfix(VRRig __instance, ref bool __result, Vector3 position, float range)
	{
		NetPlayer val = RigManager.GetNetPlayerFromVRRig(__instance) ?? null;
		if (!bypassPositionCheck || !__instance.isLocal)
		{
			if (val != null)
			{
				if (val != NetworkSystem.Instance.LocalPlayer)
				{
					return;
				}
			}
			else if (0 == 0)
			{
				return;
			}
		}
		__result = true;
	}

	[HarmonyPatch(typeof(VRRig), "SetHandEffectData")]
	[HarmonyPrefix]
	private static bool HandDataEffect(VRRig __instance, object effectContext, int audioClipIndex, bool isDownTap, bool isLeftHand, StiltID stiltID, float handTapVolume, float handTapSpeed, Vector3 dirFromHitToHand)
	{
		if (!handtapEnabled || !__instance.isLocal)
		{
			return true;
		}
		if (doOverride)
		{
			MaterialData handSurfaceData = VRRig.LocalRig.GetHandSurfaceData(audioClipIndex);
			Type contextType = effectContext.GetType();
			contextType.GetField("soundFX")?.SetValue(effectContext, handSurfaceData.audio);
			contextType.GetField("speed")?.SetValue(effectContext, overrideSpeed);
			contextType.GetField("soundVolume")?.SetValue(effectContext, overrideVolume);
			if (PhotonNetwork.InRoom && tapMultiplier > 1)
			{
				int num = 0;
				if (num < tapMultiplier)
				{
					do
					{
						Variables.taggerInstance.myVRRig.SendRPC("RPC_PlayHandTap", (RpcTarget)0, new object[3] { audioClipIndex, isLeftHand, handTapSpeed });
						num++;
					}
					while (num < tapMultiplier);
				}
				Safety.RPCShield();
				return false;
			}
			return false;
		}
		if (!tapsEnabled)
		{
			Type contextType = effectContext.GetType();
			contextType.GetField("speed")?.SetValue(effectContext, 0f);
			contextType.GetField("soundVolume")?.SetValue(effectContext, 0f);
			Variables.taggerInstance.handTapVolume = 0f;
			Variables.taggerInstance.handTapSpeed = 0f;
			Variables.taggerInstance.audioClipIndex = -1;
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(GTPlayer), "ApplyKnockback")]
	[HarmonyPrefix]
	public static bool ApplyKnockback(Vector3 direction, float speed)
	{
		return !knockbackEnabled;
	}

	[HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCallLocal")]
	[HarmonyPrefix]
	private static bool IncrementRPCCallLocalPrefix(PhotonMessageInfoWrapped infoWrapped, string rpcFunction)
	{
		return false;
	}

    [HarmonyPatch(typeof(PlayFabClientAPI), "ReportPlayer")]
    [HarmonyPrefix]
	private static bool ReportPlayerPrefix(ReportPlayerClientRequest request, Action<ReportPlayerClientResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		return false;
	}

	[HarmonyPatch(typeof(KIDManager), "HasPermissionToUseFeature")]
	[HarmonyPostfix]
	public static void HasPermissionToUseFeature(ref bool __result)
	{
		if (kidEnabled)
		{
			__result = true;
		}
	}

	[HarmonyPatch(typeof(PrivateUIRoom), "StartOverlay")]
	[HarmonyPrefix]
	private static bool PrivateUIRoomStartOverlay()
	{
		return !tosEnabled;
	}

	[HarmonyPatch(typeof(ForceVolume), "OnTriggerExit")]
	[HarmonyPrefix]
	public static bool OnTriggerExitPatch()
	{
		return !forceEnabled;
	}

    [HarmonyPatch(typeof(GameObject), "CreatePrimitive")]
    [HarmonyPostfix]
	private static void CreatePrimitivePostfix(GameObject __result)
	{
		Renderer component = __result.GetComponent<Renderer>();
		if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
		{
			Material material = component.material;
			material.shader = Variables.uberShader;
			material.color = Color.blue;
		}
	}

	[HarmonyPatch(typeof(MonkeAgent), "SendReport")]
	[HarmonyPrefix]
	private static bool SendReportPatch(string susReason, string susId, string susNick)
	{
		if (AntiCheatNotifications && susId == PhotonNetwork.LocalPlayer.UserId)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "Anti-Cheat Reported For " + susReason);
			return false;
		}
		return false;
	}

	[HarmonyPatch(typeof(KIDManager), "UseKID")]
	[HarmonyPrefix]
	private static bool KIDManagerUseKID(ref Task<bool> __result)
	{
		if (!tosEnabled)
		{
			return true;
		}
		__result = Task.FromResult(result: false);
		return false;
	}

	[HarmonyPatch(typeof(PlayFabHttp), "InitializeScreenTimeTracker")]
	[HarmonyPrefix]
	private static bool InitializeScreenTimeTrackerPrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(VRRig), "PostTick")]
	[HarmonyPrefix]
	private static bool PostTickPrefix(VRRig __instance)
	{
		return !__instance.isLocal || ((Behaviour)__instance).enabled;
	}

	[HarmonyPatch(typeof(GorillaTelemetry), "IsConnected")]
	[HarmonyPrefix]
	private static bool IsConnectedPrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(ForceVolume), "OnTriggerEnter")]
	[HarmonyPrefix]
	public static bool OnTriggerEnterPatch()
	{
		return !forceEnabled;
	}

	[HarmonyPatch(typeof(PlayFabDeviceUtil), "SendDeviceInfoToPlayFab")]
	[HarmonyPrefix]
	private static bool SendDeviceInfoPrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(GrowingSnowballThrowable), "OnEnable")]
	[HarmonyPostfix]
	public static void SnowballPostfix(GrowingSnowballThrowable __instance)
	{
		if (autoBig)
		{
			__instance.IncreaseSize(5);
		}
		else if (snowballEnabled)
		{
			__instance.IncreaseSize(Settings.snowballScale);
		}
	}

	[HarmonyPatch(typeof(MonkeAgent), "ShouldDisconnectFromRoom")]
	[HarmonyPrefix]
	private static bool ShouldDisconnectFromRoomPrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(MonkeAgent), "CheckReports")]
	[HarmonyPrefix]
	private static bool CheckReportsPatch()
	{
		return false;
	}

	[HarmonyPatch(typeof(PlayFabClientAPI), "AttributeInstall")]
	[HarmonyPrefix]
	private static bool AttributeInstallPrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCall", new Type[]
	{
		typeof(PhotonMessageInfo),
		typeof(string)
	})]
	[HarmonyPrefix]
	private static bool IncrementRPCCallPrefix(PhotonMessageInfo info, string callingMethod = "")
	{
		return false;
	}

	[HarmonyPatch(typeof(VRRig), "IsItemAllowed")]
	[HarmonyPostfix]
	private static void IsItemAllowedPostfix(ref bool __result)
	{
		if (cosmeticsEnabled)
		{
			__result = true;
		}
	}

	[HarmonyPatch(typeof(GTPlayer), "GetSlidePercentage")]
	[HarmonyPostfix]
	private static void GetSlidePercentagePostfix(ref float __result)
	{
		if (Movement.GrippySurfaces)
		{
			__result = 0f;
		}
		else if (Movement.SlipperySurfaces)
		{
			__result = 1f;
		}
	}

	[HarmonyPatch(typeof(VRRig), "OnDisable")]
	[HarmonyPrefix]
	private static bool OnDisablePrefix(VRRig __instance)
	{
		return !__instance.isLocal;
	}

	[HarmonyPatch(typeof(PlayFabClientAPI), "ReportDeviceInfo")]
	[HarmonyPrefix]
	private static bool ReportDeviceInfoPrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(MonkeAgent), "GetRPCCallTracker")]
	[HarmonyPrefix]
	private static bool GetRPCCallTrackerPrefix()
	{
		return false;
	}

    [HarmonyPatch(typeof(GTPlayer), "AntiTeleportTechnology", 0)]
    [HarmonyPrefix]
	private static bool AntiTeleportTechnologyPrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(PlayFabDeviceUtil), "GetAdvertIdFromUnity")]
	[HarmonyPrefix]
	private static bool GetAdvertIdPrefix()
	{
		return false;
	}

	[HarmonyPatch(typeof(VRRig), "IncrementRPC", new Type[]
	{
		typeof(PhotonMessageInfoWrapped),
		typeof(string)
	})]
	[HarmonyPrefix]
	private static bool IncrementRPCPrefix(PhotonMessageInfoWrapped info, string sourceCall)
	{
		return false;
	}

	[HarmonyPatch(typeof(AgeSlider), "PostUpdate")]
	[HarmonyPrefix]
	private static bool AgeSliderPostUpdate(AgeSlider __instance)
	{
		if (!tosEnabled)
		{
			return true;
		}
		__instance._currentAge = 21;
		__instance.holdTime = 0.1f;
		return false;
	}
}
