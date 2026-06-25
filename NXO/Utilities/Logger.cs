using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine;

namespace NXO.Utilities;

public static class Logger
{
	public static bool Enabled = false;

	public static HashSet<byte> EventBlacklist = new HashSet<byte> { 3, 200, 201, 203, 206 };

	public static HashSet<string> RPCBlacklist = new HashSet<string> { "OnHandTapRPCShared", "TransferOwnershipFromToRPC", "PieceDestroyedRPC", "TriggerAttractAnim" };

	public static string FormatData(object data)
	{
		if (data == null)
		{
			return "null";
		}
		if (data is Dictionary<byte, object> source)
		{
			IEnumerable<string> values = source.Select((KeyValuePair<byte, object> kvp) => $"[{kvp.Key}]={FormatData(kvp.Value)}");
			return "{" + string.Join(", ", values) + "}";
		}
		if (data is object[] array)
		{
			IEnumerable<string> values2 = from x in array.Take(10)
				select FormatData(x);
			string text = "[" + string.Join(", ", values2) + "]";
			if (array.Length > 10)
			{
				return text + $" +{array.Length - 10}";
			}
			return text;
		}
		if (data is ExitGames.Client.Photon.Hashtable hash)
		{
			IEnumerable<string> values3 = from object k in ((Dictionary<object, object>)(object)hash).Keys
				select $"{k}={FormatData(hash[k])}";
			return "{" + string.Join(", ", values3) + "}";
		}
		if (data is byte[] array2)
		{
			return $"byte[{array2.Length}]";
		}
		return data.ToString();
	}

	public static void EnableAllLogging(bool enabled)
	{
		if (Enabled != enabled)
		{
			Enabled = enabled;
			UnityEngine.Debug.Log((object)("[UltimateLogger] Logging " + (enabled ? "ENABLED" : "DISABLED")));
		}
	}
}
