using System.Text;
using NXO.Menu;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace NXO.Mods.Categories;

public class QoL : MonoBehaviour
{
	public static bool StatsOverlayEnabled;

	public static bool ShowFps = true;

	public static bool ShowPing = true;

	public static bool ShowRoom = true;

	public static bool WristWatchEnabled;

	private static QoL Instance;

	private readonly StringBuilder _statsBuilder = new StringBuilder(256);

	private GameObject _watchRoot;

	private Text _watchText;

	private float _smoothFps = 72f;

	private int _displayFps;

	private float _nextTextRefresh;

	private string _cachedStats = "";

	private static readonly Color32 OverlayBackground = new Color32((byte)0, (byte)0, (byte)0, (byte)160);

	private static readonly Color32 WatchBackground = new Color32((byte)8, (byte)8, (byte)12, (byte)210);

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		float delta = Time.unscaledDeltaTime;
		if (delta > 0f)
		{
			_smoothFps = Mathf.Lerp(_smoothFps, 1f / delta, delta * 4f);
		}
		if (Time.unscaledTime >= _nextTextRefresh)
		{
			_displayFps = Mathf.RoundToInt(_smoothFps);
			_cachedStats = BuildStatsText(multiline: true);
			_nextTextRefresh = Time.unscaledTime + 0.25f;
		}
		UpdateWatch();
	}

	private void OnGUI()
	{
		if (!StatsOverlayEnabled)
		{
			return;
		}
		string text = string.IsNullOrEmpty(_cachedStats) ? BuildStatsText(multiline: true) : _cachedStats;
		int lineCount = Mathf.Max(1, text.Split('\n').Length);
		Rect rect = new Rect(12f, 42f, 245f, 28f + lineCount * 20f);
		Color oldColor = GUI.color;
		GUI.color = OverlayBackground;
		GUI.Box(rect, GUIContent.none);
		GUI.color = Color.white;
		GUI.Label(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, rect.height - 12f), "NXO Utilities\n" + text);
		GUI.color = oldColor;
	}

	public static void SetStatsOverlay(bool enabled)
	{
		StatsOverlayEnabled = enabled;
	}

	public static void SetShowFps(bool enabled)
	{
		ShowFps = enabled;
	}

	public static void SetShowPing(bool enabled)
	{
		ShowPing = enabled;
	}

	public static void SetShowRoom(bool enabled)
	{
		ShowRoom = enabled;
	}

	public static void SetWristWatch(bool enabled)
	{
		WristWatchEnabled = enabled;
		if (!enabled)
		{
			Instance?.DestroyWatch();
		}
	}

	public static void SetPerformanceBoost(bool enabled)
	{
		Visuals.FPSBoost(enabled);
	}

	public static void SetUncapFps(bool enabled)
	{
		Visuals.UncapFPS(enabled);
	}

	private string BuildStatsText(bool multiline)
	{
		_statsBuilder.Clear();
		AppendStat("FPS", _displayFps <= 0 ? Mathf.RoundToInt(_smoothFps).ToString() : _displayFps.ToString(), multiline);
		if (ShowPing)
		{
			AppendStat("Ping", PhotonNetwork.GetPing() + " ms", multiline);
		}
		if (ShowRoom)
		{
			AppendStat("Room", CurrentRoomText(), multiline);
			AppendStat("Players", PlayerCountText(), multiline);
		}
		if (_statsBuilder.Length == 0)
		{
			_statsBuilder.Append("No stats selected");
		}
		return _statsBuilder.ToString();
	}

	private void AppendStat(string label, string value, bool multiline)
	{
		if (label == "FPS" && !ShowFps)
		{
			return;
		}
		if (_statsBuilder.Length > 0)
		{
			_statsBuilder.Append(multiline ? '\n' : " | ");
		}
		_statsBuilder.Append(label);
		_statsBuilder.Append(": ");
		_statsBuilder.Append(value);
	}

	private static string CurrentRoomText()
	{
		if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
		{
			return "Not In Room";
		}
		return PhotonNetwork.CurrentRoom.Name;
	}

	private static string PlayerCountText()
	{
		if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
		{
			return "0";
		}
		return PhotonNetwork.PlayerList.Length + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
	}

	private void UpdateWatch()
	{
		if (!WristWatchEnabled)
		{
			return;
		}
		Transform leftHand = Variables.playerInstance?.LeftHand.controllerTransform;
		if ((UnityEngine.Object)(object)leftHand == (UnityEngine.Object)null)
		{
			return;
		}
		EnsureWatch(leftHand);
		_watchText.text = "NXO\n" + (string.IsNullOrEmpty(_cachedStats) ? BuildStatsText(multiline: true) : _cachedStats);
	}

	private void EnsureWatch(Transform parent)
	{
		if ((UnityEngine.Object)(object)_watchRoot != (UnityEngine.Object)null)
		{
			if (_watchRoot.transform.parent != parent)
			{
				_watchRoot.transform.SetParent(parent, false);
			}
			return;
		}
		_watchRoot = new GameObject("NXO Wrist Stats");
		_watchRoot.transform.SetParent(parent, false);
		_watchRoot.transform.localPosition = new Vector3(0f, 0.045f, 0.075f);
		_watchRoot.transform.localRotation = Quaternion.Euler(65f, 0f, 180f);
		_watchRoot.transform.localScale = Vector3.one * 0.0015f;
		Canvas canvas = _watchRoot.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		RectTransform rootRect = _watchRoot.GetComponent<RectTransform>();
		rootRect.sizeDelta = new Vector2(220f, 115f);
		GameObject bg = new GameObject("Background");
		bg.transform.SetParent(_watchRoot.transform, false);
		Image image = bg.AddComponent<Image>();
		image.color = WatchBackground;
		RectTransform bgRect = bg.GetComponent<RectTransform>();
		bgRect.anchorMin = Vector2.zero;
		bgRect.anchorMax = Vector2.one;
		bgRect.offsetMin = Vector2.zero;
		bgRect.offsetMax = Vector2.zero;
		GameObject textObj = new GameObject("Text");
		textObj.transform.SetParent(_watchRoot.transform, false);
		_watchText = textObj.AddComponent<Text>();
		_watchText.font = Main.CurrentFont;
		_watchText.alignment = TextAnchor.MiddleCenter;
		_watchText.resizeTextForBestFit = true;
		_watchText.resizeTextMinSize = 6;
		_watchText.resizeTextMaxSize = 16;
		_watchText.color = Color.white;
		RectTransform textRect = textObj.GetComponent<RectTransform>();
		textRect.anchorMin = Vector2.zero;
		textRect.anchorMax = Vector2.one;
		textRect.offsetMin = new Vector2(8f, 6f);
		textRect.offsetMax = new Vector2(-8f, -6f);
	}

	private void DestroyWatch()
	{
		if ((UnityEngine.Object)(object)_watchRoot != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_watchRoot);
		}
		_watchRoot = null;
		_watchText = null;
	}

	private void OnDestroy()
	{
		DestroyWatch();
		if (Instance == this)
		{
			Instance = null;
		}
	}
}
