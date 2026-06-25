using System;
using System.Collections.Generic;
using BepInEx;
using GorillaNetworking;
using NXO.Mods;
using NXO.Mods.Categories;
using UnityEngine;
using UnityEngine.InputSystem;
using WindowFunction = UnityEngine.GUI.WindowFunction;

namespace NXO.Menu;

public class NXOUI : MonoBehaviour
{
	public static bool IsGuiVisible;

	public static readonly List<string> ModNotifications = new List<string>();

	public static bool freecamEnabled;

	public static bool firstPersonEnabled;

	public static bool buttonClickerEnabled;

	public static float GlobalHeight = 315f;

	public static float GlobalWidth = 192.4f;

	public static int _modListVersion;

	private static bool _resCaptured;

	private static Vector3? _oldLocalPos;

	private static readonly Dictionary<string, float> _modEntryTimes = new Dictionary<string, float>();

	private static readonly Dictionary<string, float> _modRemoveTimes = new Dictionary<string, float>();

	private static readonly List<string> _removeBuffer = new List<string>();

	private static readonly HashSet<string> _modNotificationSet = new HashSet<string>();

	private static readonly List<(string mod, float w)> _sortedMods = new List<(string, float)>(32);

	private readonly Dictionary<int, Texture2D> _modBarTexCache = new Dictionary<int, Texture2D>();

	private readonly Dictionary<int, float> _modBarTexUsed = new Dictionary<int, float>();

	private const int ModBarCacheCap = 48;

	private int _lastWatermarkFps = -1;

	private string _cachedWatermarkText;

	private float _cachedWatermarkW;

	private Texture2D _watermarkTex;

	private Texture2D _bgTex;

	private Rect _win;

	private string _inputUpper = "";

	private int _lastW;

	private int _lastH;

	private bool _dirty = true;

	private Font _font;

	private int _btnIndex;

	private readonly Dictionary<string, Texture2D> _tex = new Dictionary<string, Texture2D>(16);

	private Texture2D _pixel;

	private float _smoothFps;

	private float _fpsUpdateTimer;

	private int _displayedFps;

	private GUIStyle _sField;

	private GUIStyle _sBtn;

	private GUIStyle _sBtnOn;

	private GUIStyle _sMod;

	private GUIStyle _sWatermark;

	private readonly GUIContent _gc = new GUIContent();

	private const float WatermarkH = 26f;

	private const float Pad = 9f;

	private const float BtnH = 22f;

	private const float BtnGap = 2f;

	private float _animT = 1f;

	private bool _animatingIn;

	private bool _animatingOut;

	private bool _wasVisible = true;

	public static int originalWidth { get; private set; }

	public static int originalHeight { get; private set; }

	private void BuildStyles()
	{
		GUIStyle val = new GUIStyle
		{
			fontSize = 13,
			font = _font,
			fontStyle = (FontStyle)1,
			alignment = (TextAnchor)3,
			wordWrap = false
		};
		val.normal.textColor = Color.white;
		_sMod = val;
		_sField = new GUIStyle(GUI.skin.textField)
		{
			fontSize = 10,
			font = _font,
			fontStyle = (FontStyle)1,
			alignment = (TextAnchor)4
		};
		SetAllStates(_sField, Tex("field"));
		_sBtn = new GUIStyle(GUI.skin.button)
		{
			fontSize = 10,
			font = _font,
			fontStyle = (FontStyle)1,
			alignment = (TextAnchor)4,
			richText = true
		};
		_sBtn.normal.background = Tex("btn");
		_sBtn.hover.background = Tex("btnH");
		_sBtn.active.background = Tex("btnP");
		_sBtn.focused.background = Tex("btn");
		SetAllTextColor(_sBtn, Color.white);
		_sBtnOn = new GUIStyle(_sBtn);
		_sBtnOn.normal.background = Tex("btnOn");
		_sBtnOn.hover.background = Tex("btnOnH");
		_sBtnOn.active.background = Tex("btnOnP");
		_sBtnOn.focused.background = Tex("btnOn");
		GUIStyle val2 = new GUIStyle
		{
			fontSize = 16,
			font = _font,
			alignment = (TextAnchor)3,
			richText = true
		};
		val2.normal.textColor = Color.white;
		_sWatermark = val2;
	}

	private static Color FlowColor(float phaseOffset = 0f)
	{
		return Color.Lerp(Main.pinwheelColor1, Main.pinwheelColor2, Mathf.PingPong(Time.unscaledTime * 1.5f - phaseOffset, 1f));
	}

	private void OnDestroy()
	{
		foreach (KeyValuePair<string, Texture2D> item in _tex)
		{
			if ((UnityEngine.Object)(object)item.Value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)item.Value);
			}
		}
		_tex.Clear();
		foreach (KeyValuePair<int, Texture2D> item2 in _modBarTexCache)
		{
			if ((UnityEngine.Object)(object)item2.Value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)item2.Value);
			}
		}
		_modBarTexCache.Clear();
		_modBarTexUsed.Clear();
		if ((UnityEngine.Object)(object)_pixel != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_pixel);
			_pixel = null;
		}
		if ((UnityEngine.Object)(object)_bgTex != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_bgTex);
			_bgTex = null;
		}
		if ((UnityEngine.Object)(object)_watermarkTex != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_watermarkTex);
			_watermarkTex = null;
		}
	}

	public static void HandleActiveMods()
	{
		if (freecamEnabled)
		{
			AddMod("Freecam");
			Transform headTransform = ((Component)Variables.taggerInstance.headCollider).transform;
			float speed = Settings.FlySpeed / 2f;
			if (UnityInput.Current.GetKey((KeyCode)304))
			{
				speed += 3f;
			}
			((Component)Variables.playerInstance).GetComponent<Rigidbody>().linearVelocity = new Vector3(0f, 0.065f, 0f);
			Vector3 movement = Vector3.zero;
			if (UnityInput.Current.GetKey((KeyCode)119))
			{
				movement += headTransform.forward;
			}
			if (UnityInput.Current.GetKey((KeyCode)115))
			{
				movement -= headTransform.forward;
			}
			if (UnityInput.Current.GetKey((KeyCode)97))
			{
				movement -= headTransform.right;
			}
			if (UnityInput.Current.GetKey((KeyCode)100))
			{
				movement += headTransform.right;
			}
			if (UnityInput.Current.GetKey((KeyCode)32))
			{
				movement += headTransform.up;
			}
			if (UnityInput.Current.GetKey((KeyCode)306))
			{
				movement -= headTransform.up;
			}
			headTransform.position += movement.normalized * speed * Time.deltaTime;
			if (UnityInput.Current.GetMouseButton(1))
			{
				Vector3 mouseDelta = UnityInput.Current.mousePosition - Movement.OldMousePosition;
				Transform cameraTransform = Variables.taggerInstance.mainCamera.transform;
				cameraTransform.localEulerAngles += new Vector3((0f - mouseDelta.y) * 0.1f, mouseDelta.x * 0.1f, 0f);
			}
			Movement.OldMousePosition = UnityInput.Current.mousePosition;
		}
		if (buttonClickerEnabled)
		{
			AddMod("Button Clicker");
			if (Mouse.current.leftButton.isPressed)
			{
				Ray ray;
				if ((UnityEngine.Object)(object)Main._pcCamera != (UnityEngine.Object)null)
				{
					ray = Main._pcCamera.ScreenPointToRay((Vector2)(((InputControl<Vector2>)(object)((Pointer)Mouse.current).position).ReadValue()));
				}
				else
				{
					ray = (Main._pcCamera = Variables.thirdPersonCamera.GetComponent<Camera>()).ScreenPointToRay((Vector2)(((InputControl<Vector2>)(object)((Pointer)Mouse.current).position).ReadValue()));
				}
				Physics.Raycast(ray, out RaycastHit hit, 512f, Variables.NoInvisLayers());
				if (!_oldLocalPos.HasValue)
				{
					_oldLocalPos = Variables.taggerInstance.rightHandTriggerCollider.transform.localPosition;
				}
				Variables.taggerInstance.rightHandTriggerCollider.transform.position = hit.point;
				return;
			}
		}
		RestoreHand();
	}

	private static Texture2D MakeRect(int w, int h, Color32 fill, Color32 bord, int bw, int r)
	{
		w = Mathf.Max(1, w);
		h = Mathf.Max(1, h);
		Texture2D val = new Texture2D(w, h, (TextureFormat)4, false)
		{
			filterMode = (FilterMode)1
		};
		Color32[] array = (Color32[])(object)new Color32[w * h];
		Color32 transparent = new Color32((byte)0, (byte)0, (byte)0, (byte)0);
		for (int y = 0; y < h; y++)
		{
			for (int x = 0; x < w; x++)
			{
				int index = y * w + x;
				int cornerX = x;
				int cornerY = y;
				bool rounded = false;
				if (x < r && y >= h - r)
				{
					cornerX = r;
					cornerY = h - r;
					rounded = true;
				}
				else if (x >= w - r && y >= h - r)
				{
					cornerX = w - r - 1;
					cornerY = h - r;
					rounded = true;
				}
				else if (x < r && y < r)
				{
					cornerX = r;
					cornerY = r;
					rounded = true;
				}
				else if (x >= w - r && y < r)
				{
					cornerX = w - r - 1;
					cornerY = r;
					rounded = true;
				}
				if (rounded)
				{
					float distance = Mathf.Sqrt((float)((x - cornerX) * (x - cornerX) + (y - cornerY) * (y - cornerY)));
					array[index] = ((distance > (float)r) ? transparent : ((distance > (float)(r - bw) && bw > 0) ? bord : fill));
				}
				else
				{
					array[index] = ((bw > 0 && (x < bw || x >= w - bw || y < bw || y >= h - bw)) ? bord : fill);
				}
			}
		}
		val.SetPixels32(array);
		val.Apply(false, true);
		return val;
	}

	private void EnsureBgTexture()
	{
		int num = Mathf.Max(1, (int)GlobalWidth);
		int num2 = Mathf.Max(1, (int)GlobalHeight);
		if ((UnityEngine.Object)(object)_bgTex != (UnityEngine.Object)null && ((Texture)_bgTex).width == num && ((Texture)_bgTex).height == num2 && _tex.ContainsKey("bg"))
		{
			return;
		}
		if ((UnityEngine.Object)(object)_bgTex != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_bgTex);
		}
		_bgTex = new Texture2D(num, num2, (TextureFormat)4, false)
		{
			filterMode = (FilterMode)1
		};
		_tex["bg"] = _bgTex;
		Color32[] pixels = (Color32[])(object)new Color32[num * num2];
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = new Color32((byte)0, (byte)0, (byte)0, (byte)160);
		}
		_bgTex.SetPixels32(pixels);
		_bgTex.Apply(false);
	}

	public static void AddMod(string t)
	{
		if (!_modNotificationSet.Contains(t))
		{
			ModNotifications.Add(t);
			_modNotificationSet.Add(t);
			_modEntryTimes[t] = Time.unscaledTime;
			if (ModNotifications.Count > 100)
			{
				ModNotifications.RemoveAt(0);
				_modListVersion++;
			}
			else
			{
				_modListVersion++;
			}
		}
	}

	private void OnGUI()
	{
		if (!IsGuiVisible && !_animatingOut)
		{
			return;
		}
		int width = Screen.width;
		int height = Screen.height;
		if (width != _lastW || height != _lastH)
		{
			_lastW = width;
			_lastH = height;
			_dirty = true;
		}
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * ((float)width / (float)originalWidth * 2f));
		if (_dirty)
		{
			Rebuild();
			_dirty = false;
		}
		HandleActiveMods();
		if (IsGuiVisible && !_wasVisible)
		{
			_animatingIn = true;
			_animatingOut = false;
			_animT = 0f;
		}
		else if (!IsGuiVisible && _wasVisible)
		{
			_animatingOut = true;
			_animatingIn = false;
			_animT = 1f;
		}
		_wasVisible = IsGuiVisible;
		if (_animatingIn)
		{
			_animT = Mathf.MoveTowards(_animT, 1f, Time.unscaledDeltaTime * 5f);
			if (_animT >= 1f)
			{
				_animT = 1f;
				_animatingIn = false;
			}
		}
		if (_animatingOut)
		{
			_animT = Mathf.MoveTowards(_animT, 0f, Time.unscaledDeltaTime * 5f);
			if (_animT <= 0f)
			{
				_animT = 0f;
				_animatingOut = false;
			}
		}
		if (!IsGuiVisible && !_animatingOut)
		{
			return;
		}
		float num = 1f - Mathf.Pow(1f - _animT, 3f);
		float num2 = Mathf.Lerp(-400f, 0f, num);
		float num3 = Mathf.Lerp(400f, 0f, num);
		GUI.color = new Color(1f, 1f, 1f, num);
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = matrix * Matrix4x4.Translate(new Vector3(num2, 0f, 0f));
		if (ModNotifications.Count > 0)
		{
			DrawModList();
		}
		float num4 = Mathf.Lerp(-40f, 0f, num);
		GUI.matrix = matrix * Matrix4x4.Translate(new Vector3(0f, num4, 0f));
		DrawWatermark();
		EnsureBgTexture();
		GUI.matrix = matrix * Matrix4x4.Translate(new Vector3(num3, 0f, 0f));
		if (_win.width < 1f)
		{
		_win = new Rect((float)originalWidth / 2f - GlobalWidth - 8f, 8f, GlobalWidth, GlobalHeight);
		}
		_win = GUI.Window(1, _win, new WindowFunction(DrawWin), "", GUIStyle.none);
		GUI.matrix = matrix;
		GUI.color = Color.white;
		ControllerEmulator.ShowControllerHUD();
	}

	private Texture2D GetModBarTex(int barW)
	{
		if (_modBarTexCache.TryGetValue(barW, out Texture2D value) && (UnityEngine.Object)(object)value != (UnityEngine.Object)null)
		{
			_modBarTexUsed[barW] = Time.unscaledTime;
			return value;
		}
		if (_modBarTexCache.Count >= 48)
		{
			int key = 0;
			float num = float.MaxValue;
			foreach (KeyValuePair<int, float> current in _modBarTexUsed)
			{
				if (current.Value < num)
				{
					num = current.Value;
					key = current.Key;
				}
			}
			if (_modBarTexCache.TryGetValue(key, out Texture2D value2) && (UnityEngine.Object)(object)value2 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)value2);
			}
			_modBarTexCache.Remove(key);
			_modBarTexUsed.Remove(key);
		}
		value = MakeRect(barW, 20, new Color32((byte)0, (byte)0, (byte)0, (byte)180), default(Color32), 0, 6);
		_modBarTexCache[barW] = value;
		_modBarTexUsed[barW] = Time.unscaledTime;
		return value;
	}

	private static void SetAllTextColor(GUIStyle s, Color c)
	{
		GUIStyleState normal = s.normal;
		GUIStyleState active = s.active;
		GUIStyleState hover = s.hover;
		Color val = (s.focused.textColor = c);
		Color val3 = (hover.textColor = val);
		Color textColor = (active.textColor = val3);
		normal.textColor = textColor;
	}

	private void Awake()
	{
		_font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		_pixel = new Texture2D(1, 1, (TextureFormat)4, false)
		{
			filterMode = (FilterMode)0
		};
		_pixel.SetPixels32((Color32[])(object)new Color32[1]
		{
			new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
		});
		_pixel.Apply(false, true);
		if (!_resCaptured)
		{
			Resolution currentResolution = Screen.currentResolution;
			Resolution val = currentResolution;
			originalWidth = val.width * 2;
			currentResolution = Screen.currentResolution;
			val = currentResolution;
			originalHeight = val.height * 2;
			_resCaptured = true;
		}
		foreach (ButtonHandler.Button button in ModButtons.buttons)
		{
			if (button.Enabled && button.isToggle && !_modNotificationSet.Contains(button.buttonText))
			{
				AddMod(button.buttonText);
			}
		}
		_win = new Rect((float)originalWidth / 2f - GlobalWidth - 8f, 8f, GlobalWidth, GlobalHeight);
	}

	private void Btn(ref float y, float w, string label, Action click)
	{
		SetAllTextColor(_sBtn, FlowColor((float)_btnIndex * 0.15f));
		GUILayout.BeginArea(new Rect(9f, y, w, 24f));
		if (GUILayout.Button(label, _sBtn, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(22f) }))
		{
			click();
		}
		GUILayout.EndArea();
		y += 24f;
		_btnIndex++;
	}

	private static void SetAllStates(GUIStyle s, Texture2D t)
	{
		GUIStyleState normal = s.normal;
		GUIStyleState active = s.active;
		GUIStyleState hover = s.hover;
		Texture2D val = (s.focused.background = t);
		Texture2D val3 = (hover.background = val);
		Texture2D background = (active.background = val3);
		normal.background = background;
		GUIStyleState normal2 = s.normal;
		GUIStyleState active2 = s.active;
		GUIStyleState hover2 = s.hover;
		Color val6 = (s.focused.textColor = Color.white);
		Color val7 = (hover2.textColor = val6);
		Color textColor = (active2.textColor = val7);
		normal2.textColor = textColor;
	}

	private void Rebuild()
	{
		foreach (KeyValuePair<string, Texture2D> current in _tex)
		{
			if ((UnityEngine.Object)(object)current.Value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current.Value);
			}
		}
		_tex.Clear();
		_bgTex = null;
		foreach (KeyValuePair<int, Texture2D> current2 in _modBarTexCache)
		{
			if ((UnityEngine.Object)(object)current2.Value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current2.Value);
			}
		}
		_modBarTexCache.Clear();
		_modBarTexUsed.Clear();
		if ((UnityEngine.Object)(object)_watermarkTex != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_watermarkTex);
			_watermarkTex = null;
		}
		_lastWatermarkFps = -1;
		int w = (int)(GlobalWidth - 18f);
		Color32 fill = new Color32((byte)0, (byte)0, (byte)0, (byte)180);
		Color32 fill2 = new Color32((byte)60, (byte)60, (byte)60, (byte)180);
		Put("field", MakeRect(w, 22, new Color32((byte)0, (byte)0, (byte)0, (byte)180), default(Color32), 0, 6));
		Put("btn", MakeRect(w, 22, fill, default(Color32), 0, 6));
		Put("btnH", MakeRect(w, 22, new Color32((byte)25, (byte)25, (byte)25, (byte)200), default(Color32), 0, 6));
		Put("btnP", MakeRect(w, 22, new Color32((byte)10, (byte)10, (byte)10, (byte)160), default(Color32), 0, 6));
		Put("btnOn", MakeRect(w, 22, fill2, default(Color32), 0, 6));
		Put("btnOnH", MakeRect(w, 22, new Color32((byte)75, (byte)75, (byte)75, (byte)210), default(Color32), 0, 6));
		Put("btnOnP", MakeRect(w, 22, new Color32((byte)50, (byte)50, (byte)50, (byte)200), default(Color32), 0, 6));
		BuildStyles();
	}

	private void Tog(ref float y, float w, string label, bool on, Action click)
	{
		GUIStyle style = on ? _sBtnOn : _sBtn;
		SetAllTextColor(style, FlowColor((float)_btnIndex * 0.15f));
		GUILayout.BeginArea(new Rect(9f, y, w, 24f));
		if (GUILayout.Button(label, style, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(22f) }))
		{
			ButtonHandler.PlayClickSound();
			click();
		}
		GUILayout.EndArea();
		y += 24f;
		_btnIndex++;
	}

	private void DrawModList()
	{
		float unscaledTime = Time.unscaledTime;
		float num = 8f;
		_removeBuffer.Clear();
		foreach (KeyValuePair<string, float> current in _modRemoveTimes)
		{
			if (unscaledTime - current.Value >= 0.4f)
			{
				_removeBuffer.Add(current.Key);
			}
		}
		for (int num2 = 0; num2 < _removeBuffer.Count; num2++)
		{
			ModNotifications.Remove(_removeBuffer[num2]);
			_modNotificationSet.Remove(_removeBuffer[num2]);
			_modEntryTimes.Remove(_removeBuffer[num2]);
			_modRemoveTimes.Remove(_removeBuffer[num2]);
			_modListVersion++;
		}
		int count = ModNotifications.Count;
		if (count == 0)
		{
			return;
		}
		_sortedMods.Clear();
		for (int num3 = 0; num3 < count; num3++)
		{
			string text = ModNotifications[num3];
			_gc.text = text;
			_sortedMods.Add((text, _sMod.CalcSize(_gc).x));
		}
		_sortedMods.Sort(((string mod, float w) a, (string mod, float w) b) => b.w.CompareTo(a.w));
		for (int num4 = 0; num4 < _sortedMods.Count; num4++)
		{
			string item = _sortedMods[num4].mod;
			float item2 = _sortedMods[num4].w;
			float entryAge = _modEntryTimes.TryGetValue(item, out var value) ? (unscaledTime - value) : 999f;
			bool removing = _modRemoveTimes.TryGetValue(item, out var value2);
			float removeAge = removing ? (unscaledTime - value2) : 999f;
			float offset = 0f;
			float scale = 1f;
			float alpha = 1f;
			if (removing && removeAge < 0.4f)
			{
				float t = removeAge / 0.4f;
				offset = t * 30f;
				alpha = 1f - t;
			}
			else if (entryAge < 0.4f)
			{
				float t2 = entryAge / 0.4f;
				float bounce = Mathf.Sin(t2 * MathF.PI * 2.5f) * (1f - t2);
				offset = bounce * 8f;
				scale = 1f + bounce * 0.15f;
			}
			float x = 6f + offset;
			float y = num + (float)num4 * 24f;
			float width2 = item2 + 18f;
			Matrix4x4 matrix = GUI.matrix;
			GUIUtility.ScaleAroundPivot(new Vector2(scale, 1f), new Vector2(x, y + 10f));
			Texture2D modBarTex = GetModBarTex((int)width2);
			Color val = FlowColor((float)num4 * 0.15f);
			GUI.color = new Color(1f, 1f, 1f, alpha);
			GUI.DrawTexture(new Rect(x, y, width2, 20f), (Texture)(object)modBarTex);
			_sMod.normal.textColor = new Color(val.r, val.g, val.b, alpha);
			GUI.Label(new Rect(x + 10f, y, item2 + 10f, 20f), item, _sMod);
			GUI.matrix = matrix;
			GUI.color = Color.white;
		}
		_sMod.normal.textColor = Color.white;
	}

	private void DrawWatermark()
	{
		_smoothFps = Mathf.Lerp(_smoothFps, 1f / Time.unscaledDeltaTime, Time.unscaledDeltaTime * 4f);
		_fpsUpdateTimer -= Time.unscaledDeltaTime;
		if (_fpsUpdateTimer <= 0f || _displayedFps != _lastWatermarkFps)
		{
			if (_fpsUpdateTimer <= 0f)
			{
				_displayedFps = Mathf.RoundToInt(_smoothFps);
				_fpsUpdateTimer = 0.5f;
			}
			_lastWatermarkFps = _displayedFps;
			_cachedWatermarkText = string.Format("{0} | v{1} | FPS: {2} | (L-Alt) Menu (R-Alt) GUI", "NXO", $"{NXO.Initialization.PluginInfo.menuVersion}", _displayedFps);
			_gc.text = _cachedWatermarkText;
			_cachedWatermarkW = _sWatermark.CalcSize(_gc).x + 20f;
		}
		float cachedWatermarkW = _cachedWatermarkW;
		float num = (float)originalWidth / 4f - cachedWatermarkW / 2f;
		if ((UnityEngine.Object)(object)_watermarkTex == (UnityEngine.Object)null || ((Texture)_watermarkTex).width != (int)cachedWatermarkW)
		{
			if ((UnityEngine.Object)(object)_watermarkTex != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)_watermarkTex);
			}
			_watermarkTex = MakeRect((int)cachedWatermarkW, 26, new Color32((byte)0, (byte)0, (byte)0, (byte)140), default(Color32), 0, 8);
		}
		GUI.color = Color.white;
		GUI.DrawTexture(new Rect(num, 8f, cachedWatermarkW, 26f), (Texture)(object)_watermarkTex);
		_sWatermark.normal.textColor = FlowColor();
		GUI.Label(new Rect(num + 10f, 8f, cachedWatermarkW, 26f), _cachedWatermarkText, _sWatermark);
		_sWatermark.normal.textColor = Color.white;
	}

	private void Put(string k, Texture2D t)
	{
		if (_tex.TryGetValue(k, out Texture2D value) && (UnityEngine.Object)(object)value != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)value);
			_tex[k] = t;
		}
		else
		{
			_tex[k] = t;
		}
	}

	private static void RestoreHand()
	{
		if (_oldLocalPos.HasValue)
		{
			Variables.taggerInstance.rightHandTriggerCollider.transform.position = _oldLocalPos.Value;
			_oldLocalPos = null;
		}
	}

	public static void RemoveMod(string t)
	{
		if (_modNotificationSet.Contains(t))
		{
			_modRemoveTimes[t] = Time.unscaledTime;
			_modListVersion++;
		}
	}

	private void DrawWin(int id)
	{
		float num = _win.width - 18f;
		GUI.DrawTexture(new Rect(0f, 0f, _win.width, _win.height), (Texture)(object)Tex("bg"));
		float num2 = 9f;
		float y = num2;
		_btnIndex = 0;
		GUILayout.BeginArea(new Rect(9f, y, num, 24f));
		string text = GUILayout.TextField(_inputUpper, _sField, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(22f) });
		GUILayout.EndArea();
		if (text != _inputUpper)
		{
			_inputUpper = text.ToUpper();
			GorillaTagger taggerInstance = Variables.taggerInstance;
			if (taggerInstance != null)
			{
				VRRig offlineVRRig = taggerInstance.offlineVRRig;
				if (offlineVRRig != null)
				{
					offlineVRRig.PlayHandTapLocal(66, false, 1f);
					y += 24f;
					Btn(ref y, num, "Join Specific", delegate
					{
						ButtonHandler.PlayClickSound();
						((PhotonNetworkController)PhotonNetworkController.Instance).AttemptToJoinSpecificRoom(_inputUpper, (GorillaNetworking.JoinType)0);
					});
					Btn(ref y, num, "Join Random", delegate
					{
						ButtonHandler.PlayClickSound();
						Room.JoinRandomPublic();
					});
					Btn(ref y, num, "Change Name", delegate
					{
						ButtonHandler.PlayClickSound();
						RigManager.ChangeName(_inputUpper);
					});
					Btn(ref y, num, "Disconnect", delegate
					{
						ButtonHandler.PlayClickSound();
						NetworkSystem.Instance.ReturnToSinglePlayer();
					});
					Btn(ref y, num, "Reconnect", delegate
					{
						ButtonHandler.PlayClickSound();
						Room.Reconnect();
					});
					Tog(ref y, num, "Freecam", freecamEnabled, delegate
					{
						freecamEnabled = !freecamEnabled;
						if (!freecamEnabled)
						{
							RemoveMod("Freecam");
						}
					});
					Tog(ref y, num, "Noclip", Movement.noclipEnabled, delegate
					{
						Movement.noclipEnabled = !Movement.noclipEnabled;
						MeshCollider[] array = UnityEngine.Object.FindObjectsOfType<MeshCollider>();
						foreach (MeshCollider val in array)
						{
							((Collider)val).enabled = !Movement.noclipEnabled;
						}
						if (Movement.noclipEnabled)
						{
							AddMod("Noclip");
						}
						else
						{
							RemoveMod("Noclip");
						}
					});
					Tog(ref y, num, "First Person", firstPersonEnabled, delegate
					{
						firstPersonEnabled = !firstPersonEnabled;
						if (firstPersonEnabled)
						{
							AddMod("First Person");
							Visuals.FPC(enable: true);
						}
						else
						{
							RemoveMod("First Person");
							Visuals.FPC(enable: false);
						}
					});
					Tog(ref y, num, "Button Clicker", buttonClickerEnabled, delegate
					{
						buttonClickerEnabled = !buttonClickerEnabled;
						if (!buttonClickerEnabled)
						{
							RemoveMod("Button Clicker");
						}
					});
					Tog(ref y, num, "Controller Emulator", ControllerEmulator.EmulatorEnabled, delegate
					{
						ControllerEmulator.EmulatorEnabled = !ControllerEmulator.EmulatorEnabled;
					});
					GlobalHeight = y + 9f;
					_win.height = GlobalHeight;
				}
			}
			y += 24f;
			Btn(ref y, num, "Join Specific", delegate
			{
				ButtonHandler.PlayClickSound();
				((PhotonNetworkController)PhotonNetworkController.Instance).AttemptToJoinSpecificRoom(_inputUpper, (GorillaNetworking.JoinType)0);
			});
			Btn(ref y, num, "Join Random", delegate
			{
				ButtonHandler.PlayClickSound();
				Room.JoinRandomPublic();
			});
			Btn(ref y, num, "Change Name", delegate
			{
				ButtonHandler.PlayClickSound();
				RigManager.ChangeName(_inputUpper);
			});
			Btn(ref y, num, "Disconnect", delegate
			{
				ButtonHandler.PlayClickSound();
				NetworkSystem.Instance.ReturnToSinglePlayer();
			});
			Btn(ref y, num, "Reconnect", delegate
			{
				ButtonHandler.PlayClickSound();
				Room.Reconnect();
			});
			Tog(ref y, num, "Freecam", freecamEnabled, delegate
			{
				freecamEnabled = !freecamEnabled;
				if (!freecamEnabled)
				{
					RemoveMod("Freecam");
				}
			});
			Tog(ref y, num, "Noclip", Movement.noclipEnabled, delegate
			{
				Movement.noclipEnabled = !Movement.noclipEnabled;
				MeshCollider[] array = UnityEngine.Object.FindObjectsOfType<MeshCollider>();
				foreach (MeshCollider val in array)
				{
					((Collider)val).enabled = !Movement.noclipEnabled;
				}
				if (Movement.noclipEnabled)
				{
					AddMod("Noclip");
				}
				else
				{
					RemoveMod("Noclip");
				}
			});
			Tog(ref y, num, "First Person", firstPersonEnabled, delegate
			{
				firstPersonEnabled = !firstPersonEnabled;
				if (firstPersonEnabled)
				{
					AddMod("First Person");
					Visuals.FPC(enable: true);
				}
				else
				{
					RemoveMod("First Person");
					Visuals.FPC(enable: false);
				}
			});
			Tog(ref y, num, "Button Clicker", buttonClickerEnabled, delegate
			{
				buttonClickerEnabled = !buttonClickerEnabled;
				if (!buttonClickerEnabled)
				{
					RemoveMod("Button Clicker");
				}
			});
			Tog(ref y, num, "Controller Emulator", ControllerEmulator.EmulatorEnabled, delegate
			{
				ControllerEmulator.EmulatorEnabled = !ControllerEmulator.EmulatorEnabled;
			});
			GlobalHeight = y + 9f;
			_win.height = GlobalHeight;
			return;
		}
		y += 24f;
		Btn(ref y, num, "Join Specific", delegate
		{
			ButtonHandler.PlayClickSound();
			((PhotonNetworkController)PhotonNetworkController.Instance).AttemptToJoinSpecificRoom(_inputUpper, (GorillaNetworking.JoinType)0);
		});
		Btn(ref y, num, "Join Random", delegate
		{
			ButtonHandler.PlayClickSound();
			Room.JoinRandomPublic();
		});
		Btn(ref y, num, "Change Name", delegate
		{
			ButtonHandler.PlayClickSound();
			RigManager.ChangeName(_inputUpper);
		});
		Btn(ref y, num, "Disconnect", delegate
		{
			ButtonHandler.PlayClickSound();
			NetworkSystem.Instance.ReturnToSinglePlayer();
		});
		Btn(ref y, num, "Reconnect", delegate
		{
			ButtonHandler.PlayClickSound();
			Room.Reconnect();
		});
		Tog(ref y, num, "Freecam", freecamEnabled, delegate
		{
			freecamEnabled = !freecamEnabled;
			if (!freecamEnabled)
			{
				RemoveMod("Freecam");
			}
		});
		Tog(ref y, num, "Noclip", Movement.noclipEnabled, delegate
		{
			Movement.noclipEnabled = !Movement.noclipEnabled;
			MeshCollider[] array = UnityEngine.Object.FindObjectsOfType<MeshCollider>();
			foreach (MeshCollider val in array)
			{
				((Collider)val).enabled = !Movement.noclipEnabled;
			}
			if (Movement.noclipEnabled)
			{
				AddMod("Noclip");
			}
			else
			{
				RemoveMod("Noclip");
			}
		});
		Tog(ref y, num, "First Person", firstPersonEnabled, delegate
		{
			firstPersonEnabled = !firstPersonEnabled;
			if (firstPersonEnabled)
			{
				AddMod("First Person");
				Visuals.FPC(enable: true);
			}
			else
			{
				RemoveMod("First Person");
				Visuals.FPC(enable: false);
			}
		});
		Tog(ref y, num, "Button Clicker", buttonClickerEnabled, delegate
		{
			buttonClickerEnabled = !buttonClickerEnabled;
			if (!buttonClickerEnabled)
			{
				RemoveMod("Button Clicker");
			}
		});
		Tog(ref y, num, "Controller Emulator", ControllerEmulator.EmulatorEnabled, delegate
		{
			ControllerEmulator.EmulatorEnabled = !ControllerEmulator.EmulatorEnabled;
		});
		GlobalHeight = y + 9f;
		_win.height = GlobalHeight;
	}

	private Texture2D Tex(string k)
	{
		Texture2D value;
		return (Texture2D)(_tex.TryGetValue(k, out value) ? ((object)value) : ((object)_pixel));
	}
}
