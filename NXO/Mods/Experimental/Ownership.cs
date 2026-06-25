using System;
using Photon.Pun;
using UnityEngine;

namespace NXO.Mods.Experimental;

internal class Ownership
{
	public static void TakeOwnershipOverObj(GameObject Obj)
	{
		RequestableOwnershipGuard val = default(RequestableOwnershipGuard);
		Obj.TryGetComponent<RequestableOwnershipGuard>(out val);
		if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
		{
			val.ownershipRequestNonce = Guid.NewGuid().ToString();
			val.currentState = (NetworkingState)3;
			val.netViews[0].SendRPC("OwnershipRequested", val.actualOwner, new object[1] { val.ownershipRequestNonce });
			val.netViews[0].SendRPC("TransferOwnershipFromToRPC", (RpcTarget)1, new object[2]
			{
				PhotonNetwork.LocalPlayer,
				val.ownershipRequestNonce
			});
			val.netViews[0].SendRPC("SetOwnershipFromMasterClient", (RpcTarget)1, new object[1] { PhotonNetwork.LocalPlayer });
			val.SetOwnership(VRRig.LocalRig.Creator, false, false);
		}
	}
}
