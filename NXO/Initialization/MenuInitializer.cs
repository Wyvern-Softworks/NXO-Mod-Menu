using GorillaLocomotion;
using HarmonyLib;
using NXO.Menu;
using NXO.Utilities;
using UnityEngine;

namespace NXO.Initialization;

[HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
internal class MenuInitializer
{
	private static GameObject obj;

	[HarmonyPostfix]
	private static void Postfix()
	{
		if (!((UnityEngine.Object)(object)obj != (UnityEngine.Object)null))
		{
			obj = new GameObject("NXO");
			obj.AddComponent<Main>();
			obj.AddComponent<CoroutineHelper>();
			obj.AddComponent<NotificationLib>();
			obj.AddComponent<NXOUI>();
			obj.AddComponent<CustomBoards>();
			obj.AddComponent<NetworkingLibrary>();
			UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)obj);
			UnityEngine.Debug.Log((object)"NXO initialized.");
		}
	}
}
