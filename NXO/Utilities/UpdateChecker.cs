using System;
using System.Collections;
using System.Text;
using NXO.Initialization;
using UnityEngine;
using UnityEngine.Networking;

namespace NXO.Utilities;

internal class UpdateChecker : MonoBehaviour
{
	private const string VersionFileUrl = "https://raw.githubusercontent.com/Wyvern-Softworks/NXO-Mod-Menu/refs/heads/main/Resources/NXO-Version.txt";

	private const string CacheLastCheckKey = "NXO_UpdateChecker_VersionFile_LastCheckUtc";

	private const string CacheLatestVersionKey = "NXO_UpdateChecker_VersionFile_LatestVersion";

	private const long CheckIntervalSeconds = 300;

	private static bool _started;

	private void Start()
	{
		if (_started)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)this);
			return;
		}
		_started = true;
		((MonoBehaviour)this).StartCoroutine(CheckForUpdates());
	}

	private IEnumerator CheckForUpdates()
	{
		yield return null;

		long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		if (TryUseCachedVersion(now))
		{
			yield break;
		}

		using UnityWebRequest req = UnityWebRequest.Get(VersionFileUrl);
		req.timeout = 10;
		req.SetRequestHeader("User-Agent", "NXO-Mod-Menu/" + PluginInfo.menuVersion);
		yield return req.SendWebRequest();

		if ((int)req.result != 1)
		{
			UnityEngine.Debug.LogWarning((object)("[NXO] Update check failed: " + req.error));
			yield break;
		}

		string latestVersion = ParseVersionFile(req.downloadHandler.text);
		if (string.IsNullOrWhiteSpace(latestVersion))
		{
			UnityEngine.Debug.LogWarning((object)"[NXO] Update check returned an empty version file.");
			yield break;
		}

		CacheVersion(now, latestVersion);
		HandleVersion(latestVersion);
	}

	private static bool TryUseCachedVersion(long now)
	{
		string lastCheckText = PlayerPrefs.GetString(CacheLastCheckKey, string.Empty);
		if (!long.TryParse(lastCheckText, out long lastCheck) || now - lastCheck >= CheckIntervalSeconds)
		{
			return false;
		}

		string latestVersion = PlayerPrefs.GetString(CacheLatestVersionKey, string.Empty);
		if (string.IsNullOrWhiteSpace(latestVersion))
		{
			return false;
		}

		HandleVersion(latestVersion);
		return true;
	}

	private static void CacheVersion(long now, string latestVersion)
	{
		PlayerPrefs.SetString(CacheLastCheckKey, now.ToString());
		PlayerPrefs.SetString(CacheLatestVersionKey, latestVersion ?? string.Empty);
		PlayerPrefs.Save();
	}

	private static string ParseVersionFile(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}

		string[] lines = text.Replace("\r", string.Empty).Split('\n');
		foreach (string rawLine in lines)
		{
			string line = rawLine.Trim();
			if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
			{
				continue;
			}

			int equalsIndex = line.IndexOf('=');
			return (equalsIndex >= 0 ? line.Substring(equalsIndex + 1) : line).Trim();
		}

		return string.Empty;
	}

	private static void HandleVersion(string latestVersion)
	{
		if (!IsNewerVersion(latestVersion, PluginInfo.menuVersion))
		{
			UnityEngine.Debug.Log((object)("[NXO] Update check: current version is up to date (" + PluginInfo.menuVersion + ")."));
			return;
		}

		string message = "Update available: " + latestVersion + " (current " + PluginInfo.menuVersion + ")";
		UnityEngine.Debug.Log((object)("[NXO] " + message + " - https://github.com/Wyvern-Softworks/NXO-Mod-Menu"));
		if ((UnityEngine.Object)(object)NotificationLib.Instance != (UnityEngine.Object)null)
		{
			((MonoBehaviour)NotificationLib.Instance).StartCoroutine(NotifyWhenReady(message));
		}
	}

	private static IEnumerator NotifyWhenReady(string message)
	{
		for (int i = 0; i < 10; i++)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, message);
			if (!string.IsNullOrEmpty(NotificationLib.PreviousNotification) && NotificationLib.PreviousNotification.Contains(message))
			{
				yield break;
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private static bool IsNewerVersion(string latestVersion, string currentVersion)
	{
		Version latest = ParseVersion(latestVersion);
		Version current = ParseVersion(currentVersion);
		if (latest != null && current != null)
		{
			return latest.CompareTo(current) > 0;
		}

		return !string.Equals(NormalizeVersion(latestVersion), NormalizeVersion(currentVersion), StringComparison.OrdinalIgnoreCase);
	}

	private static Version ParseVersion(string text)
	{
		string normalized = NormalizeVersion(text);
		if (string.IsNullOrEmpty(normalized))
		{
			return null;
		}

		StringBuilder builder = new StringBuilder(normalized.Length);
		bool started = false;
		foreach (char c in normalized)
		{
			if (char.IsDigit(c))
			{
				started = true;
				builder.Append(c);
			}
			else if (started && c == '.')
			{
				builder.Append(c);
			}
			else if (started)
			{
				break;
			}
		}

		string versionText = builder.ToString().Trim('.');
		return Version.TryParse(versionText, out Version version) ? version : null;
	}

	private static string NormalizeVersion(string text)
	{
		return (text ?? string.Empty).Trim().TrimStart('v', 'V');
	}
}
