using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace NXO.Mods.Experimental;

internal class Room
{
	public static void SetMasterClient(Photon.Realtime.Player p)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.SetMasterClient(p);
			RaiseEventOptions val = new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)1
			};
			ExitGames.Client.Photon.Hashtable val2 = new ExitGames.Client.Photon.Hashtable();
			val2.Add((byte)0, (object)p.ActorNumber);
			ExitGames.Client.Photon.Hashtable val3 = val2;
			PhotonNetwork.RaiseEvent((byte)150, (object)val3, val, SendOptions.SendReliable);
		}
		else
		{
			RaiseEventOptions val = new RaiseEventOptions
			{
				Receivers = (ReceiverGroup)1
			};
			ExitGames.Client.Photon.Hashtable val4 = new ExitGames.Client.Photon.Hashtable();
			val4.Add((byte)0, (object)p.ActorNumber);
			ExitGames.Client.Photon.Hashtable val3 = val4;
			PhotonNetwork.RaiseEvent((byte)150, (object)val3, val, SendOptions.SendReliable);
		}
	}
}
