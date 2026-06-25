using HarmonyLib;
using UnityEngine;

namespace NXO.Initialization;

public class Loader
{
	public static void Load()
	{
		new Harmony("com.nxo.nxomodmenu.org").PatchAll();
		UnityEngine.Debug.Log((object)$"NXO v{PluginInfo.menuVersion} initialized.");
	}
}