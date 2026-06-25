using Photon.Pun;
using UnityEngine;

namespace NXO.Mods.Categories;

public static class Networked
{
	public static class SizeChanger
	{
		private static float sizeScale = 1f;

		private static float lastSentScale = 1f;

		public static void NetworkedSizeChanger(bool enable)
		{
			if (!PhotonNetwork.InRoom)
			{
				return;
			}
			if (!enable)
			{
				NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.SizeChanger, new object[1] { 1f });
				return;
			}
			if (InputHandler.LSecondary())
			{
				sizeScale = 1f;
			}
			if (InputHandler.LTrigger())
			{
				sizeScale -= 0.05f;
			}
			if (InputHandler.RTrigger())
			{
				sizeScale += 0.05f;
			}
			sizeScale = Mathf.Clamp(sizeScale, 0.375f, 2.75f);
			Variables.playerInstance.nativeScale = sizeScale;
			if (sizeScale != lastSentScale)
			{
				lastSentScale = sizeScale;
				NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.SizeChanger, new object[1] { sizeScale });
			}
		}
	}
}
