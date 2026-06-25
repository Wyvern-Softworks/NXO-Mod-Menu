using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NXO.Menu;
using NXO.Mods;
using UnityEngine;
using UnityEngine.UI;

namespace NXO.Utilities;

internal class NotificationLib : MonoBehaviour
{
	public enum NotificationType
	{
		Enabled,
		Disabled,
		Saved,
		Loaded,
		Deleted,
		Room,
		Error,
		Alert,
		Info
	}

	private static readonly Dictionary<string, float> _notificationTimestamps = new Dictionary<string, float>();

	private GameObject _hudObj;

	private GameObject _hudObj2;

	private GameObject _mainCamera;

	private Text _notificationText;

	private Text _arrayListText;

	private Material _notificationMaterial;

	private Material _arrayListMaterial;

	private readonly List<GameObject> _trackedObjects = new List<GameObject>();

	private bool _hasInitialized;

	private float _fadeAlpha = 1f;

	private bool _isFading;

	private const float NOTIFICATION_DELAY = 3f;

	private const float FADE_DURATION = 0.5f;

	private int _arrayListCacheVersion = -1;

	private string _arrayListCache = "";

	private readonly StringBuilder _arrayListSB = new StringBuilder(512);

	private readonly List<string> _enabledModsBuffer = new List<string>(32);

	public static bool inRoom;

	public static bool RoomNotifications = true;

	public static bool ArrayListEnabled;

	private static readonly Dictionary<NotificationType, string> _typeColors = new Dictionary<NotificationType, string>
	{
		{
			NotificationType.Enabled,
			"#00FF00"
		},
		{
			NotificationType.Disabled,
			"#FF4040"
		},
		{
			NotificationType.Saved,
			"#00AAFF"
		},
		{
			NotificationType.Loaded,
			"#00FFFF"
		},
		{
			NotificationType.Deleted,
			"#FF8C00"
		},
		{
			NotificationType.Room,
			"#C040FF"
		},
		{
			NotificationType.Error,
			"#FF0000"
		},
		{
			NotificationType.Alert,
			"#FFD700"
		},
		{
			NotificationType.Info,
			"#B0B0B0"
		}
	};

	public static string PreviousNotification { get; private set; }

	public static bool IsEnabled { get; set; } = true;

	public static NotificationLib Instance { get; private set; }

	public void UpdateNotifications()
	{
		if (!_hasInitialized)
		{
			Init();
		}
		if ((UnityEngine.Object)(object)_hudObj2 != (UnityEngine.Object)null && (UnityEngine.Object)(object)_mainCamera != (UnityEngine.Object)null)
		{
			_hudObj2.transform.SetPositionAndRotation(_mainCamera.transform.position, _mainCamera.transform.rotation);
		}
		ProcessExpiredNotifications();
	}

	public void UpdateTextAlpha()
	{
		if ((UnityEngine.Object)(object)_notificationText != (UnityEngine.Object)null)
		{
			Color color = ((Graphic)_notificationText).color;
			color.a = _fadeAlpha;
			((Graphic)_notificationText).color = color;
		}
	}

	private Text CreateTextElement(string name, GameObject parent, Vector3 position, Vector2 size, int fontSize)
	{
		GameObject val = new GameObject(name);
		val.transform.parent = parent.transform;
		Text val2 = val.AddComponent<Text>();
		val2.fontSize = fontSize;
		val2.alignment = (TextAnchor)4;
		((Graphic)val2).rectTransform.sizeDelta = size;
		((Transform)((Graphic)val2).rectTransform).localScale = new Vector3(0.01f, 0.01f, 1f);
		((Transform)((Graphic)val2).rectTransform).localPosition = position;
		_trackedObjects.Add(val);
		return val2;
	}

	public static void ClearAllNotifications()
	{
		_notificationTimestamps.Clear();
		if ((UnityEngine.Object)(object)Instance._notificationText != (UnityEngine.Object)null)
		{
			Instance.UpdateNotificationText();
		}
	}

	public void UpdateArrayList()
	{
		if (!_hasInitialized)
		{
			Init();
		}
		if ((UnityEngine.Object)(object)_arrayListText == (UnityEngine.Object)null)
		{
			return;
		}
		if (!ArrayListEnabled)
		{
			_arrayListText.text = "";
			return;
		}
		if (_arrayListCacheVersion == NXOUI._modListVersion)
		{
			_arrayListText.text = _arrayListCache;
			return;
		}
		_enabledModsBuffer.Clear();
		foreach (ButtonHandler.Button button in ModButtons.buttons)
		{
			if (button != null && button.Enabled)
			{
				_enabledModsBuffer.Add(button.buttonText);
			}
		}
		_enabledModsBuffer.Sort((a, b) => b.Length.CompareTo(a.Length));
		_arrayListSB.Clear();
		for (int i = 0; i < _enabledModsBuffer.Count; i++)
		{
			if (i > 0)
			{
				_arrayListSB.Append('\n');
			}
			_arrayListSB.Append("<color=blue>></color> ");
			_arrayListSB.Append(_enabledModsBuffer[i]);
		}
		_arrayListCache = _arrayListSB.ToString();
		_arrayListCacheVersion = NXOUI._modListVersion;
		_arrayListText.text = _arrayListCache;
	}

	public void Init()
	{
		if (!_hasInitialized)
		{
			_mainCamera = GameObject.Find("Main Camera");
			if (!((UnityEngine.Object)(object)_mainCamera == (UnityEngine.Object)null))
			{
				_hudObj2 = CreateAndTrackHUDObject("HUD_Notification_Parent");
				_hudObj2.transform.position = _mainCamera.transform.position + new Vector3(-1.5f, 0f, -4.5f);
				_hudObj = CreateAndTrackHUDObject("HUD_Notification", _hudObj2.transform);
				Canvas val = _hudObj.AddComponent<Canvas>();
				val.renderMode = (RenderMode)2;
				val.worldCamera = _mainCamera.GetComponent<Camera>();
				CanvasScaler val2 = _hudObj.AddComponent<CanvasScaler>();
				val2.dynamicPixelsPerUnit = 10f;
				_hudObj.AddComponent<GraphicRaycaster>();
				RectTransform component = _hudObj.GetComponent<RectTransform>();
				component.sizeDelta = new Vector2(5f, 5f);
				((Transform)component).localScale = Vector3.one;
				((Transform)component).localPosition = new Vector3(0f, 0f, 1.6f);
				((Transform)component).rotation = Quaternion.Euler(0f, -250f, 0f);
				_notificationText = CreateTextElement("NotificationText", _hudObj, new Vector3(-1.2f, -0.75f, 0f), new Vector2(300f, 70f), 7);
				_notificationText.font = Main.CurrentFont;
				_notificationText.fontStyle = (FontStyle)1;
				_notificationText.alignment = (TextAnchor)6;
				_notificationMaterial = new Material(Shader.Find("GUI/Text Shader"));
				((Graphic)_notificationText).material = _notificationMaterial;
				_arrayListText = CreateTextElement("ArrayListText", _hudObj, new Vector3(-1.2f, -0.5f, 0f), new Vector2(300f, 400f), 7);
				_arrayListText.font = Main.CurrentFont;
				_arrayListText.alignment = (TextAnchor)0;
				_arrayListMaterial = new Material(Shader.Find("GUI/Text Shader"));
				((Graphic)_arrayListText).material = _arrayListMaterial;
				_hasInitialized = true;
			}
		}
	}

	private void Awake()
	{
		if ((UnityEngine.Object)(object)Instance != (UnityEngine.Object)null && (UnityEngine.Object)(object)Instance != (UnityEngine.Object)(object)this)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)this);
			return;
		}
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)((Component)this).gameObject);
	}

	public void UpdateNotificationText()
	{
		if ((UnityEngine.Object)(object)_notificationText != (UnityEngine.Object)null)
		{
			_notificationText.text = string.Join(Environment.NewLine, _notificationTimestamps.Keys);
		}
	}

	private GameObject CreateAndTrackHUDObject(string name, Transform parent = null)
	{
		GameObject val = new GameObject(name);
		if ((UnityEngine.Object)(object)parent != (UnityEngine.Object)null)
		{
			val.transform.parent = parent;
			_trackedObjects.Add(val);
			return val;
		}
		_trackedObjects.Add(val);
		return val;
	}

	private void Update()
	{
		UpdateNotifications();
		UpdateArrayList();
	}

	private IEnumerator FadeOutNotification()
	{
		float elapsed = 0f;
		while (elapsed < 0.5f)
		{
			elapsed += Time.deltaTime;
			_fadeAlpha = Mathf.Lerp(1f, 0f, elapsed / 0.5f);
			UpdateTextAlpha();
			yield return null;
		}
		_fadeAlpha = 0f;
		UpdateTextAlpha();
		string oldestKey = null;
		float oldestVal = float.MaxValue;
		foreach (KeyValuePair<string, float> kvp in _notificationTimestamps)
		{
			if (kvp.Value < oldestVal)
			{
				oldestVal = kvp.Value;
				oldestKey = kvp.Key;
			}
		}
		if (oldestKey != null)
		{
			_notificationTimestamps.Remove(oldestKey);
		}
		UpdateNotificationText();
		_isFading = false;
		_fadeAlpha = 1f;
	}

	public void ProcessExpiredNotifications()
	{
		if (_notificationTimestamps.Count == 0)
		{
			return;
		}
		float time = Time.time;
		float num = float.MaxValue;
		foreach (float current in _notificationTimestamps.Values)
		{
			if (current < num)
			{
				num = current;
			}
		}
		float num2 = time - num;
		if (num2 >= 3f)
		{
			if (!_isFading)
			{
				_isFading = true;
				((MonoBehaviour)this).StartCoroutine(FadeOutNotification());
			}
		}
		else
		{
			_fadeAlpha = 1f;
			_isFading = false;
			UpdateTextAlpha();
		}
	}

	public static void SendNotification(NotificationType type, string content)
	{
		if (!Variables.toggleNotifications || !IsEnabled || string.IsNullOrEmpty(content) || (UnityEngine.Object)(object)Instance?._notificationText == (UnityEngine.Object)null)
		{
			return;
		}
		string text;
		if (!_typeColors.TryGetValue(type, out string value))
		{
			string arg = "#FFFFFF";
			text = $"<color={arg}>{type}</color> : {content}";
			if (text == PreviousNotification)
			{
				return;
			}
		}
		else
		{
			string arg = value;
			text = $"<color={arg}>{type}</color> : {content}";
			if (text == PreviousNotification)
			{
				return;
			}
		}
		_notificationTimestamps[text] = Time.time;
		PreviousNotification = text;
		Instance.UpdateNotificationText();
		((MonoBehaviour)Instance).StartCoroutine(Instance.FadeInNotification());
	}

	private IEnumerator FadeInNotification()
	{
		if (_isFading)
		{
			yield break;
		}
		float elapsed = 0f;
		while (elapsed < 0.5f)
		{
			elapsed += Time.deltaTime;
			_fadeAlpha = Mathf.Lerp(0f, 1f, elapsed / 0.5f);
			UpdateTextAlpha();
			yield return null;
		}
		_fadeAlpha = 1f;
		UpdateTextAlpha();
	}
}
