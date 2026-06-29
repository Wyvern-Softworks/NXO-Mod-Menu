using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using GorillaLocomotion;
using GorillaTag;
using NXO.Mods;
using NXO.Mods.Categories;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using HandState = GorillaLocomotion.GTPlayer.HandState;

namespace NXO.Menu;

public class Main : MonoBehaviour
{
	private struct FadeItem
	{
		public Renderer r;

		public Text txt;

		public float origA;
	}

	public enum ColorRole
	{
		None,
		Background,
		Button,
		EnabledButton,
		Outline,
		AccentStrip
	}

	private class TrackedColorGroup
	{
		public ColorRole role;

		public List<Renderer> renderers = new List<Renderer>(8);
	}

	public class FontEntry
	{
		public string Description;

		public Font Font;
	}

	public static readonly Vector3 MenuSize = new Vector3(0.75f, 0.2875f, 0.45f);

	public static readonly Vector3 BackgroundScale = new Vector3(0.01f, 0.875f, 0.9f);

	public static readonly Vector3 BackgroundPos = new Vector3(0.05f, 0f, 0.025f);

	public const float FaceX = 0.07525f;

	public const float IconX = 0.07225f;

	public static readonly Vector3 SearchBarScale = new Vector3(0.01f, 0.8625f, 0.0625f);

	public static readonly Vector3 SearchBarPos = new Vector3(0.0667f, 0f, 0.56f);

	public static readonly Vector3 SearchTextPos = new Vector3(0.054f, 0f, 0.2525f);

	public static readonly Vector3 HeaderIconScale = new Vector3(0.039375f, 0.0590625f, 0f);

	public const float DisconnectIconY = -0.25f;

	public const float SearchIconY = -0.35f;

	public const float HeaderIconZ = 0.45f;

	public static readonly Vector2 TitleSize = new Vector2(0.11375f, 0.02625f);

	public static readonly Vector3 AccentStripScale = new Vector3(0.001f, 0.775f, 0.004f);

	public static readonly Vector3 AccentStripPos = new Vector3(0.0715f, 0f, 0.4f);

	private static int? _uiLayerMask;

	public static Camera _pcCamera;

	private static string _lastSearchInput;

	public static Category _lastPageDrawn;

	private static int _lastPageIndex;

	public static int _lastDrawModVersion = -1;

	private static readonly List<ButtonHandler.Button> _pageCache = new List<ButtonHandler.Button>(7);

	private static bool _lastWasSearching;

	public const float ButtonFullWidth = 0.82f;

	public const float ButtonWidthWithGear = 0.695f;

	public const float ButtonWidthIncremental = 0.555f;

	public const float ButtonHeight = 0.08f;

	public const float ButtonDepth = 0.0075f;

	public const float ButtonTextX = 0.05975f;

	public const float ButtonYWithGear = 0.0625f;

	public const float ButtonBaseZ = 0.335f;

	public const float SubButtonWidth = 0.12f;

	public const float IncrementUpY = -0.3525f;

	public const float IncrementDownY = 0.3525f;

	public const float GearY = -0.3525f;

	public const float NavRowZ = -0.33f;

	public const float ReturnHeight = 0.2925f;

	public const float PageHeight = 0.25f;

	public const float PageLeftY = 0.285f;

	public const float PageRightY = -0.285f;

	public static readonly Vector2 ModTextSize = new Vector2(0.22f, 1f / 64f);

	public static readonly Vector2 IncrementalTextSize = new Vector2(0.15f, 0.0155f);

	public static readonly Vector2 SubTextSize = new Vector2(0.1f, 0.015f);

	public static readonly Vector2 PageTextSize = new Vector2(0.15f, 0.0225f);

	public static readonly Vector2 GearIconSize = new Vector2(0.1f, 0.02f);

	public static readonly Vector3 HomeIconScale = new Vector3(0.015f, 0.015f, 0f);

	public const float OutlineThickness = 0.0125f;

	public const float ButtonOutlineThickness = 0.0065f;

	public const float ClickerSize = 0.0035f;

	public static readonly Vector3 ClickerLocalPos = new Vector3(0f, -0.1f, 0f);

	public static List<Material> _dynamicMaterials = new List<Material>();

	public static readonly List<GameObject> _trackedObjects = new List<GameObject>();

	private static readonly List<GameObject> _pageButtonObjects = new List<GameObject>();

	private static readonly List<Material> _pageButtonMaterials = new List<Material>();

	private static readonly List<Mesh> _pageButtonMeshes = new List<Mesh>();

	private static bool _drawingPageButtons;

	private static readonly List<FadeItem> _fadeItems = new List<FadeItem>(96);

	private static readonly HashSet<Material> _fadeSeen = new HashSet<Material>();

	private static Coroutine _pageFadeRoutine;

	private const float PageFadeDuration = 0.16f;

	private static readonly List<TrackedColorGroup> _colorGroups = new List<TrackedColorGroup>(32);

	private static readonly List<ButtonHandler.Button> _newCacheScratch = new List<ButtonHandler.Button>(7);

	private static readonly List<(ButtonHandler.Button b, int score)> _scoredScratch = new List<(ButtonHandler.Button, int)>(32);

	private static bool _wasInMenuCondition;

	private static bool _wasInPcCondition;

	public static float menuScale = 0.6f;

	public static bool MenuAnimations = true;

	public static GameObject disconnectButton;

	private static GameObject _disconnectButtonBackground;

	private static List<GameObject> _disconnectButtonRoundedParts;

	private static bool? _disconnectButtonWasInRoom;

	private static readonly Color32 DisconnectButtonColor = new Color32(byte.MaxValue, (byte)55, (byte)55, byte.MaxValue);

	private static readonly Color32 JoinRandomButtonColor = new Color32((byte)55, (byte)220, (byte)85, byte.MaxValue);

	public static Color pinwheelColor1 = (Color32)(new Color32((byte)0, (byte)100, byte.MaxValue, byte.MaxValue));

	public static Color pinwheelColor2 = (Color32)(new Color32((byte)230, (byte)230, (byte)230, byte.MaxValue));

	public static float pinwheelSpeed = 0.5f;

	public static Material _cachedPinwheelBase;

	private static Material _clickerMaterial;

	private static float _lastAutoSaveTime;

	private const float AUTO_SAVE_INTERVAL = 60f;

	private static float _lastTitleUpdateTime;

	private const float TITLE_UPDATE_INTERVAL = 0.1f;

	private static readonly StringBuilder _titleBuilder = new StringBuilder(256);

	public static readonly List<FontEntry> _fonts = new List<FontEntry>(8);

	private static readonly List<Mesh> _dynamicMeshes = new List<Mesh>();

	private static readonly List<Material> _dynamicPinwheelMaterials = new List<Material>();

	private static Material _activePinwheelMaterial;

	public static readonly List<Font> _dynamicFonts = new List<Font>(4);

	private static readonly List<ButtonHandler.Button> _activeTicks = new List<ButtonHandler.Button>(32);

	private static bool _activeTicksDirty = true;

	public static float _lastKeyboardYRotation = float.MinValue;

	public static Vector3 _lastKeyboardSnapPosition = Vector3.zero;

	private static bool _colorsDirty = true;

	private static bool _lateUpdateLogged;

	private static int UILayerMask
	{
		get
		{
			int valueOrDefault = _uiLayerMask.GetValueOrDefault();
			int result;
			if (!_uiLayerMask.HasValue)
			{
				valueOrDefault = 1 << LayerMask.NameToLayer("UI");
				_uiLayerMask = valueOrDefault;
				result = valueOrDefault;
			}
			else
			{
				result = valueOrDefault;
			}
			return result;
		}
	}

	public static float TextOffset => (Settings.CurrentFontDescription == "Comic Sans") ? 0.1525f : 0.15f;

	public static float IncrementTextOffset => Settings.CurrentFontDescription switch
	{
		"Minecraft" => 0.15f, 
		"Arial" => 0.148f, 
		"Comic Sans" => 0.152f, 
		_ => 0.155f, 
	};

	public static float PageZOffset
	{
		get
		{
			string currentFontDescription = Settings.CurrentFontDescription;
			if (!(currentFontDescription == "Minecraft"))
			{
				if (currentFontDescription == "Arial")
				{
					return -0.14875f;
				}
				return -23f / 160f;
			}
			return -0.14875f;
		}
	}

	public static (float y, float z) TitleOffset
	{
		get
		{
			string currentFontDescription = Settings.CurrentFontDescription;
			if (!(currentFontDescription == "Comic Sans"))
			{
				if (currentFontDescription == "Minecraft")
				{
					return (y: 0.075f, z: 0.2025f);
				}
				return (y: 0.07f, z: 0.2025f);
			}
			return (y: 0.062f, z: 0.2075f);
		}
	}

	private static List<GameObject> ObjectSink => _drawingPageButtons ? _pageButtonObjects : _trackedObjects;

	private static List<Material> MaterialSink => _drawingPageButtons ? _pageButtonMaterials : _dynamicMaterials;

	private static List<Mesh> MeshSink => _drawingPageButtons ? _pageButtonMeshes : _dynamicMeshes;

	public static Font CurrentFont => (_fonts.Count > 0 && Settings.CurrentFontIndex >= 0 && Settings.CurrentFontIndex < _fonts.Count) ? _fonts[Settings.CurrentFontIndex].Font : Resources.GetBuiltinResource<Font>("Arial.ttf");

	public static void AddModButtons(float offset, ButtonHandler.Button button)
	{
		if (button == null)
		{
			return;
		}
		if (button.incremental)
		{
			CreateIncrementalButton("DOWN : " + button.buttonText, button, offset, 0.3525f, "<", "_DOWN");
			CreateIncrementalButton("UP : " + button.buttonText, button, offset, -0.3525f, ">", "_UP");
		}
		float width = button.incremental ? 0.555f : (button.showGear ? 0.695f : 0.82f);
		float yOffset = (button.showGear && !button.incremental) ? 0.0625f : 0f;
		GameObject val = CreateCube("Button: " + button.buttonText, null, keepCollider: true);
		val.transform.localScale = new Vector3(0.0075f, width, 0.08f);
		val.transform.localPosition = new Vector3(0.07525f, yOffset, 0.335f - offset);
		bool isActionable = !button.incremental && (button.isToggle || button.onEnable != null || ButtonHandler.IsCategoryButton(button));
		ButtonHandler.BtnCollider btnCollider = null;
		if (isActionable)
		{
			btnCollider = val.AddComponent<ButtonHandler.BtnCollider>();
			btnCollider.clickedButton = button;
		}
		ApplyColor(val, (Color32)(button.Enabled ? Settings.EnabledButtonColor : Settings.ButtonColor));
		List<GameObject> outline = ApplyOutline(val, 2, 0.0065f);
		List<GameObject> list = null;
		if (Settings.MenuRoundness > 0f)
		{
			list = RoundObj(val, null, null, Settings.MenuRoundness);
		}
		RegisterColorGroup(button.Enabled ? ColorRole.EnabledButton : ColorRole.Button, val, list);
		if (button.showGear && !button.incremental)
		{
			CreateQuickActionButton(button, offset);
		}
		Text val2 = CreateCanvasText();
		((UnityEngine.Object)val2).name = "Button: Text - " + button.buttonText;
		val2.text = ButtonHandler.FavoriteMods.Contains(button) ? ("<color=blue>★</color> " + button.buttonText) : button.buttonText;
		float num = 1.2f;
		((Component)val2).gameObject.transform.localScale = new Vector3(num, num, 1f);
		RectTransform component = ((Component)val2).GetComponent<RectTransform>();
		((Transform)component).localPosition = new Vector3(0.05975f, button.showGear ? 0.015f : 0f, TextOffset - offset / 2.225f);
		((Transform)component).localRotation = Quaternion.Euler(180f, 90f, 90f);
		Vector2 textSize = button.incremental ? IncrementalTextSize : ModTextSize;
		component.sizeDelta = new Vector2(textSize.x / num, textSize.y);
		Transform categoryArrow = null;
		if (ButtonHandler.IsCategoryButton(button))
		{
			Text val3 = CreateCanvasText();
			((UnityEngine.Object)val3).name = "Button: Arrow - " + button.buttonText;
			val3.text = (button.buttonText == "Return") ? "<color=red><</color>" : ("<color=#" + ColorUtility.ToHtmlStringRGB((Color32)(Settings.EnabledButtonColor)) + ">></color>");
			val3.alignment = (TextAnchor)3;
			((Component)val3).gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
			RectTransform component2 = ((Component)val3).GetComponent<RectTransform>();
			((Transform)component2).localPosition = new Vector3(0.05975f, button.showGear ? 0.012f : (-0.01125f), TextOffset - offset / 2.225f);
			((Transform)component2).localRotation = Quaternion.Euler(180f, 90f, 90f);
			component2.sizeDelta = button.incremental ? IncrementalTextSize : ModTextSize;
			categoryArrow = ((Component)val3).transform;
		}
		if (btnCollider != null)
		{
			RegisterClickAnim(btnCollider, val, list, outline, ((Component)val2).transform, categoryArrow);
		}
		if (btnCollider != null && MenuAnimations && button.isToggle && button.Enabled)
		{
			btnCollider.ApplyFactor(0.92f);
		}
	}

	private static IEnumerator ScaleOverTime(GameObject obj, Vector3 from, Vector3 to, float duration)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			if ((UnityEngine.Object)(object)obj == (UnityEngine.Object)null)
			{
				yield break;
			}
			float t = elapsed / duration;
			obj.transform.localScale = Vector3.Lerp(from, to, 1f - Mathf.Pow(1f - t, 3f));
			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}
		if ((UnityEngine.Object)(object)obj != (UnityEngine.Object)null)
		{
			obj.transform.localScale = to;
		}
	}

	private static Text CreateCanvasText(int fontSize = 3)
	{
		GameObject val = new GameObject();
		val.transform.SetParent(Variables.canvasObj.transform, false);
		ObjectSink.Add(val);
		Text val2 = val.AddComponent<Text>();
		val2.font = CurrentFont;
		val2.fontStyle = (FontStyle)0;
		((Graphic)val2).color = (Color32)((Color32)(Color.white));
		val2.fontSize = fontSize;
		val2.alignment = (TextAnchor)4;
		val2.resizeTextForBestFit = true;
		val2.resizeTextMinSize = 0;
		return val2;
	}

	private static void ApplyTexture(GameObject obj, string resourcePath, Color? tint = null)
	{
		Texture2D val = AssetHandler.LoadEmbeddedTexture(resourcePath);
		if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
		{
			Material val2 = new Material(Variables.uiShader)
			{
				mainTexture = (Texture)(object)val
			};
			if (tint.HasValue)
			{
				val2.color = tint.Value;
				obj.GetComponent<Renderer>().sharedMaterial = TrackMat(val2);
			}
			else
			{
				obj.GetComponent<Renderer>().sharedMaterial = TrackMat(val2);
			}
		}
	}

	private static void RegisterClickAnim(ButtonHandler.BtnCollider col, GameObject body, List<GameObject> rounded, List<GameObject> outline, params Transform[] uniform)
	{
		if ((UnityEngine.Object)(object)col == (UnityEngine.Object)null)
		{
			return;
		}
		if ((UnityEngine.Object)(object)body != (UnityEngine.Object)null)
		{
			col.RegisterBody(body.transform);
		}
		if (rounded != null)
		{
			for (int i = 0; i < rounded.Count; i++)
			{
				if ((UnityEngine.Object)(object)rounded[i] != (UnityEngine.Object)null)
				{
					col.RegisterBody(rounded[i].transform);
				}
			}
		}
		if (outline != null)
		{
			for (int j = 0; j < outline.Count; j++)
			{
				if ((UnityEngine.Object)(object)outline[j] != (UnityEngine.Object)null)
				{
					col.RegisterBody(outline[j].transform);
				}
			}
		}
		if (uniform == null)
		{
			return;
		}
		for (int k = 0; k < uniform.Length; k++)
		{
			if ((UnityEngine.Object)(object)uniform[k] != (UnityEngine.Object)null)
			{
				col.RegisterUniform(uniform[k]);
			}
		}
	}

	private static GameObject CreateQuad(string name = null, Transform parent = null)
	{
		GameObject val = CreatePrimitive((PrimitiveType)5, name, parent);
		((Collider)val.AddComponent<BoxCollider>()).isTrigger = true;
		return val;
	}

	private static string BuildAnimatedTitle()
	{
		_titleBuilder.Clear();
		_titleBuilder.Append("<b>");
		string text = $"NXO v{NXO.Initialization.PluginInfo.menuVersion}";
		for (int i = 0; i < text.Length; i++)
		{
			if (i == 3)
			{
				_titleBuilder.Append("</b>");
			}
			float t = (Mathf.Sin(Time.time * pinwheelSpeed * 2f - (float)i * 0.5f) + 1f) / 2f;
			Color val = Color.Lerp(pinwheelColor1, pinwheelColor2, t);
			_titleBuilder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(val)).Append('>')
				.Append(text[i])
				.Append("</color>");
		}
		return _titleBuilder.ToString();
	}

	public static void LoadFonts()
	{
		CleanupFonts();
		RegisterFont("Arial", Resources.GetBuiltinResource<Font>("Arial.ttf"));
		Font val = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 22);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			_dynamicFonts.Add(val);
			RegisterFont("Comic Sans", val);
			RegisterFontFromBundle("Minecraft", "NXO.Resources.minecraftfont", "Minecraftia-Regular");
		}
		else
		{
			RegisterFontFromBundle("Minecraft", "NXO.Resources.minecraftfont", "Minecraftia-Regular");
		}
	}

	public static void NotifyRoomEvent(string message)
	{
		if (NotificationLib.RoomNotifications)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Room, message);
			if ((UnityEngine.Object)(object)Variables.notificationSound != (UnityEngine.Object)null && Variables.NotificationSounds)
			{
				AssetHandler.PlaySound(Variables.thirdPersonCamera, Variables.notificationSound);
			}
		}
	}

	public static void OnLeaveRoom()
	{
		if (NotificationLib.inRoom)
		{
			NotificationLib.inRoom = false;
			MenuPatches.SerializationPatch.Override = null;
			NetworkingLibrary.ClearDetectedUsers();
			NotifyRoomEvent("Left Code `" + Room.roomCode + "`");
			PlayersActionList.GeneratePlayerButtons();
			Sound.ResetMic();
		}
	}

	private static List<GameObject> ApplyOutline(GameObject target, int requiredMode, float zGrow = 0.0125f)
	{
		if (Settings.OutlineMode < requiredMode)
		{
			return null;
		}
		GameObject val = CreateOutline(target, Variables.menuObj.transform, zGrow);
		List<GameObject> list = null;
		if (Settings.MenuRoundness > 0f)
		{
			list = RoundObj(val, null, null, Settings.MenuRoundness, 2455);
		}
		RegisterColorGroup(ColorRole.Outline, val, list);
		List<GameObject> list2 = new List<GameObject>(2) { val };
		if (list != null)
		{
			list2.AddRange(list);
		}
		return list2;
	}

	public static void RefreshMenu()
	{
		CleanupMenu();
		Draw();
		SearchAndKeyboard.ReRegisterKeyboardColorGroups();
	}

	public static IEnumerator BounceAnim(GameObject obj, Vector3 target, bool cleanupAfter = false)
	{
		if ((UnityEngine.Object)(object)obj == (UnityEngine.Object)null)
		{
			yield break;
		}
		Vector3 start = obj.transform.localScale;
		if (cleanupAfter)
		{
			if (MenuAnimations)
			{
				yield return ScaleOverTime(obj, start, target, 0.2f);
			}
			CleanupMenu();
			_wasInMenuCondition = false;
			_wasInPcCondition = false;
			Variables.InMenuCondition = false;
			Variables.InPcCondition = false;
			yield break;
		}
		if (!MenuAnimations)
		{
			obj.transform.localScale = target;
			yield break;
		}
		float elapsed = 0f;
		while (elapsed < 0.8f)
		{
			if ((UnityEngine.Object)(object)obj == (UnityEngine.Object)null)
			{
				yield break;
			}
			float t = elapsed / 0.8f;
			obj.transform.localScale = Vector3.LerpUnclamped(start, target, ElasticEaseOut(t));
			elapsed += Time.deltaTime;
			yield return null;
		}
		if ((UnityEngine.Object)(object)obj != (UnityEngine.Object)null)
		{
			obj.transform.localScale = target;
		}
	}

	private static void PositionMenuOnKeyboard()
	{
		if ((UnityEngine.Object)(object)SearchAndKeyboard.keyboardObj != (UnityEngine.Object)null && (UnityEngine.Object)(object)Variables.playerInstance != (UnityEngine.Object)null)
		{
			float y = ((Component)Variables.playerInstance.headCollider).transform.eulerAngles.y;
			Vector3 position = ((Component)Variables.playerInstance.headCollider).transform.position;
			if (_lastKeyboardYRotation == float.MinValue)
			{
				_lastKeyboardYRotation = y;
				_lastKeyboardSnapPosition = position;
			}
			if (Variables.SearchFollow)
			{
				float angle = Mathf.Abs(Mathf.DeltaAngle(_lastKeyboardYRotation, y));
				float distance = Vector3.Distance(position, _lastKeyboardSnapPosition);
				if (!(angle < 45f) || distance >= 1.5f)
				{
					_lastKeyboardYRotation = y;
					_lastKeyboardSnapPosition = position;
				}
				SearchAndKeyboard.keyboardObj.transform.position = position;
				SearchAndKeyboard.keyboardObj.transform.rotation = Quaternion.Euler(0f, _lastKeyboardYRotation, 0f);
				Transform transform3 = SearchAndKeyboard.keyboardObj.transform;
				transform3.position += SearchAndKeyboard.keyboardObj.transform.forward * 0.35f - Vector3.up * 0.35f;
				SearchAndKeyboard.keyboardObj.transform.Rotate(Vector3.right, 70f);
			}
			else
			{
				SearchAndKeyboard.keyboardObj.transform.position = _lastKeyboardSnapPosition;
				SearchAndKeyboard.keyboardObj.transform.rotation = Quaternion.Euler(0f, _lastKeyboardYRotation, 0f);
				Transform transform = SearchAndKeyboard.keyboardObj.transform;
				transform.position += SearchAndKeyboard.keyboardObj.transform.forward * 0.35f - Vector3.up * 0.35f;
				SearchAndKeyboard.keyboardObj.transform.Rotate(Vector3.right, 70f);
			}
		}
		Transform transform2 = SearchAndKeyboard.keyboardObj.transform;
		Variables.menuObj.transform.position = transform2.position - transform2.forward * 0.17f + transform2.up * 0.2f;
		Variables.menuObj.transform.rotation = transform2.rotation * Quaternion.Euler(-75f, 0f, 0f);
		Variables.menuObj.transform.Rotate(Vector3.up, 90f);
		Variables.menuObj.transform.Rotate(Vector3.right, -90f);
	}

	public static void AddSearchBar()
	{
		if (SearchAndKeyboard.isSearching)
		{
			Variables.TopMenuButton = CreateCube(null, null, keepCollider: true);
			Variables.TopMenuButton.transform.localScale = SearchBarScale;
			Variables.TopMenuButton.transform.localPosition = SearchBarPos;
			ApplyColor(Variables.TopMenuButton, (Color32)(Settings.BackgroundColor));
			ApplyOutline(Variables.TopMenuButton, 1);
			List<GameObject> extraParts = null;
			if (Settings.MenuRoundness > 0f)
			{
				extraParts = RoundObj(Variables.TopMenuButton, null, null, Settings.MenuRoundness);
				RegisterColorGroup(ColorRole.Background, Variables.TopMenuButton, extraParts);
				SearchAndKeyboard.searchText = CreateCanvasText();
				SearchAndKeyboard.searchText.alignment = (TextAnchor)4;
				RectTransform component = ((Component)SearchAndKeyboard.searchText).GetComponent<RectTransform>();
				component.sizeDelta = new Vector2(0.2f, 0.02f);
				((Transform)component).localScale = new Vector3(0.9f, 0.9f, 0.9f);
				((Transform)component).localRotation = Quaternion.Euler(180f, 90f, 90f);
				((Transform)component).localPosition = SearchTextPos;
				SearchAndKeyboard.searchText.text = ((!string.IsNullOrEmpty(SearchAndKeyboard.inputText)) ? SearchAndKeyboard.inputText : SearchAndKeyboard.placeholderText);
			}
			else
			{
				RegisterColorGroup(ColorRole.Background, Variables.TopMenuButton, extraParts);
				SearchAndKeyboard.searchText = CreateCanvasText();
				SearchAndKeyboard.searchText.alignment = (TextAnchor)4;
				RectTransform component = ((Component)SearchAndKeyboard.searchText).GetComponent<RectTransform>();
				component.sizeDelta = new Vector2(0.2f, 0.02f);
				((Transform)component).localScale = new Vector3(0.9f, 0.9f, 0.9f);
				((Transform)component).localRotation = Quaternion.Euler(180f, 90f, 90f);
				((Transform)component).localPosition = SearchTextPos;
				SearchAndKeyboard.searchText.text = ((!string.IsNullOrEmpty(SearchAndKeyboard.inputText)) ? SearchAndKeyboard.inputText : SearchAndKeyboard.placeholderText);
			}
		}
	}

	private static void CreateIncrementalButton(string name, ButtonHandler.Button parent, float offset, float yPos, string symbol, string suffix)
	{
		GameObject val = CreateCube(name, null, keepCollider: true);
		val.transform.localScale = new Vector3(0.0075f, 0.12f, 0.08f);
		val.transform.localPosition = new Vector3(0.07525f, yPos, 0.335f - offset);
		ButtonHandler.BtnCollider btnCollider = val.AddComponent<ButtonHandler.BtnCollider>();
		btnCollider.clickedButton = new ButtonHandler.Button(parent.buttonText + suffix, parent.Page, isToggle: false, isActive: false, null);
		ApplyColor(val, (Color32)(Settings.ButtonColor));
		List<GameObject> outline = ApplyOutline(val, 2, 0.0065f);
		List<GameObject> list = null;
		if (Settings.MenuRoundness > 0f)
		{
			list = RoundObj(val, null, null, Settings.MenuRoundness);
		}
		RegisterColorGroup(ColorRole.Button, val, list);
		Text val2 = CreateCanvasText();
		val2.text = symbol;
		((Component)val2).gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
		RectTransform component = ((Component)val2).GetComponent<RectTransform>();
		float incrementTextOffset = IncrementTextOffset;
		float num = (yPos < 0f) ? -0.1025f : 0.1015f;
		((Transform)component).localPosition = new Vector3(0.05975f, num, incrementTextOffset - offset / 2.225f);
		component.sizeDelta = SubTextSize;
		((Transform)component).localRotation = Quaternion.Euler(180f, 90f, 90f);
		RegisterClickAnim(btnCollider, val, list, outline, ((Component)val2).transform);
	}

	private static Material TrackMat(Material mat)
	{
		if ((UnityEngine.Object)(object)mat != (UnityEngine.Object)null)
		{
			MaterialSink.Add(mat);
			return mat;
		}
		return mat;
	}

	public static void RedrawButtonList(int animate = 0)
	{
		if ((UnityEngine.Object)(object)Variables.menuObj == (UnityEngine.Object)null)
		{
			return;
		}
		if (_pageFadeRoutine != null && (UnityEngine.Object)(object)Variables.playerInstance != (UnityEngine.Object)null)
		{
			((MonoBehaviour)Variables.playerInstance).StopCoroutine(_pageFadeRoutine);
			_pageFadeRoutine = null;
		}
		_fadeItems.Clear();
		for (int i = 0; i < _pageButtonObjects.Count; i++)
		{
			if ((UnityEngine.Object)(object)_pageButtonObjects[i] != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)_pageButtonObjects[i]);
			}
		}
		_pageButtonObjects.Clear();
		for (int j = 0; j < _pageButtonMaterials.Count; j++)
		{
			if ((UnityEngine.Object)(object)_pageButtonMaterials[j] != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)_pageButtonMaterials[j]);
			}
		}
		_pageButtonMaterials.Clear();
		for (int k = 0; k < _pageButtonMeshes.Count; k++)
		{
			if ((UnityEngine.Object)(object)_pageButtonMeshes[k] != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)_pageButtonMeshes[k]);
			}
		}
		_pageButtonMeshes.Clear();
		_colorGroups.RemoveAll((TrackedColorGroup g) => g.renderers.Count == 0 || g.renderers.All((Renderer r) => (UnityEngine.Object)(object)r == (UnityEngine.Object)null));
		_lastDrawModVersion = -1;
		_lastWasSearching = false;
		_lastSearchInput = null;
		DrawCurrentPageButtons();
		SetUILayer(Variables.menuObj);
		if (animate != 0 && MenuAnimations)
		{
			StartPageFade();
		}
	}

	public static Material GetPinwheelMaterial()
	{
		if ((UnityEngine.Object)(object)_cachedPinwheelBase == (UnityEngine.Object)null)
		{
			_cachedPinwheelBase = AssetHandler.LoadMaterial("NXO.Resources.pinwheelshader", "outline2");
			if ((UnityEngine.Object)(object)_cachedPinwheelBase == (UnityEngine.Object)null)
			{
				UnityEngine.Debug.LogError((object)"Failed to load pinwheel material");
				return null;
			}
		}
		Material val = new Material(_cachedPinwheelBase);
		AssetHandler.SetProperty(val, "_Speed", 0f - pinwheelSpeed);
		AssetHandler.SetProperty(val, "_COLOR1", pinwheelColor1);
		AssetHandler.SetProperty(val, "_COLOR2", pinwheelColor2);
		return val;
	}

	public static void OnPlayerJoin(NetPlayer player)
	{
		NetworkingLibrary.BroadcastNXOUser();
		if (player != NetworkSystem.Instance.LocalPlayer)
		{
			NotifyRoomEvent("`" + player.NickName + "` Joined");
			if (Variables.currentPage != Category.Players)
			{
				return;
			}
		}
		else if (Variables.currentPage != Category.Players)
		{
			return;
		}
		PlayersActionList.GeneratePlayerButtons();
	}

	private static float ElasticEaseOut(float t)
	{
		if (t <= 0f)
		{
			return 0f;
		}
		if (t >= 1f)
		{
			return 1f;
		}
		return 1.05f * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.075f) * (MathF.PI * 2f) / 0.3f) + 1f;
	}

	private static GameObject CreatePrimitive(PrimitiveType type, string name = null, Transform parent = null, bool keepCollider = false, bool triggerCollider = true)
	{
		GameObject val = GameObject.CreatePrimitive(type);
		if (name != null)
		{
			((UnityEngine.Object)val).name = name;
		}
		Rigidbody component = val.GetComponent<Rigidbody>();
		if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)component);
		}
		Collider component2 = val.GetComponent<Collider>();
		if ((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null)
		{
			if (!keepCollider)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)component2);
			}
			else if (component2 is BoxCollider val2)
			{
				((Collider)val2).isTrigger = triggerCollider;
			}
		}
		Transform transform = val.transform;
		GameObject menuObj = Variables.menuObj;
		transform.SetParent(parent ?? ((menuObj != null) ? menuObj.transform : null), true);
		val.transform.localRotation = Quaternion.identity;
		ObjectSink.Add(val);
		return val;
	}

	private void Start()
	{
		try
		{
			UnityEngine.Debug.Log((object)"[NXO] Main.Start begin");
			_dynamicMaterials.Clear();
			_trackedObjects.Clear();
			_colorGroups.Clear();
			Variables.folderName = Path.Combine(Directory.GetCurrentDirectory(), "NXO Mod Menu");
			Directory.CreateDirectory(Variables.folderName);
			Variables.taggerInstance = GorillaTagger.Instance;
			Variables.playerInstance = GTPlayer.Instance;
			Variables.thirdPersonCamera = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
			LoadFonts();
			ButtonHandler.LoadCustomClickSounds();
			ButtonHandler.PreloadAllClickSounds();
			Settings.CaptureDefaults();
			ButtonHandler.LoadAutoSavePreference();
			Settings.LoadSettings();
			ButtonHandler.LoadAutosavedStuff();
			ButtonHandler.LoadFavoritedMods();
			Variables.cm = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1");
			_lastAutoSaveTime = Time.time;
			UnityEngine.Debug.Log((object)"[NXO] Main.Start end");
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError((object)("[NXO] Main.Start failed: " + ex));
		}
	}

	private static void UpdateAnimatedColors()
	{
		if ((UnityEngine.Object)(object)Variables.menuObj == (UnityEngine.Object)null && !SearchAndKeyboard.isSearching)
		{
			return;
		}
		bool backgroundPinwheel = Settings.BackgroundMode == Settings.ColorMode.Pinwheel;
		bool buttonPinwheel = Settings.ButtonMode == Settings.ColorMode.Pinwheel;
		bool enabledPinwheel = Settings.EnabledButtonMode == Settings.ColorMode.Pinwheel;
		bool outlinePinwheel = Settings.OutlineColorMode == Settings.ColorMode.Pinwheel;
		bool accentPinwheel = Settings.AccentStripMode == Settings.ColorMode.Pinwheel;
		bool anyPinwheel = backgroundPinwheel || buttonPinwheel || enabledPinwheel || outlinePinwheel || accentPinwheel;
		bool backgroundAnimated = Settings.BackgroundMode != Settings.ColorMode.Solid;
		bool buttonAnimated = Settings.ButtonMode != Settings.ColorMode.Solid;
		bool enabledAnimated = Settings.EnabledButtonMode != Settings.ColorMode.Solid;
		bool outlineAnimated = Settings.OutlineColorMode != Settings.ColorMode.Solid;
		bool accentAnimated = Settings.AccentStripMode != Settings.ColorMode.Solid;
		bool titleAnimated = Settings.TitleMode != Settings.ColorMode.Solid;
		if (!(backgroundAnimated || buttonAnimated || enabledAnimated || outlineAnimated || accentAnimated || titleAnimated) && !_colorsDirty)
		{
			return;
		}
		if (anyPinwheel)
		{
			if ((UnityEngine.Object)(object)_activePinwheelMaterial == (UnityEngine.Object)null)
			{
				_activePinwheelMaterial = GetPinwheelMaterial();
				_dynamicPinwheelMaterials.Add(_activePinwheelMaterial);
			}
			else if (_colorsDirty)
			{
				AssetHandler.SetProperty(_activePinwheelMaterial, "_Speed", 0f - pinwheelSpeed);
				AssetHandler.SetProperty(_activePinwheelMaterial, "_COLOR1", pinwheelColor1);
				AssetHandler.SetProperty(_activePinwheelMaterial, "_COLOR2", pinwheelColor2);
			}
		}
		Color backgroundColor = backgroundPinwheel ? Color.white : Settings.GetAnimatedColor(Settings.BackgroundMode, (Color32)(Settings.BackgroundColor), (Color32)(Settings.BackgroundColor2), Settings.BackgroundAnimSpeed);
		Color buttonColor = buttonPinwheel ? Color.white : Settings.GetAnimatedColor(Settings.ButtonMode, (Color32)(Settings.ButtonColor), (Color32)(Settings.ButtonColor2), Settings.ButtonAnimSpeed, 1);
		Color enabledColor = enabledPinwheel ? Color.white : Settings.GetAnimatedColor(Settings.EnabledButtonMode, (Color32)(Settings.EnabledButtonColor), (Color32)(Settings.EnabledButtonColor2), Settings.EnabledButtonAnimSpeed, 2);
		Color outlineColor = outlinePinwheel ? Color.white : Settings.GetAnimatedColor(Settings.OutlineColorMode, (Color32)(Settings.OutlineColor), (Color32)(Settings.OutlineColor2), Settings.OutlineAnimSpeed, 4);
		Color accentColor = accentPinwheel ? Color.white : Settings.GetAnimatedColor(Settings.AccentStripMode, (Color32)(Settings.AccentStripColor), (Color32)(Settings.AccentStripColor2), Settings.AccentStripAnimSpeed, 5);
		for (int i = 0; i < _colorGroups.Count; i++)
		{
			TrackedColorGroup trackedColorGroup = _colorGroups[i];
			Color roleColor;
			bool rolePinwheel;
			bool roleAnimated;
			switch (trackedColorGroup.role)
			{
			case ColorRole.Background:
				roleColor = backgroundColor;
				rolePinwheel = backgroundPinwheel;
				roleAnimated = backgroundAnimated;
				break;
			case ColorRole.Button:
				roleColor = buttonColor;
				rolePinwheel = buttonPinwheel;
				roleAnimated = buttonAnimated;
				break;
			case ColorRole.EnabledButton:
				roleColor = enabledColor;
				rolePinwheel = enabledPinwheel;
				roleAnimated = enabledAnimated;
				break;
			case ColorRole.Outline:
				roleColor = outlineColor;
				rolePinwheel = outlinePinwheel;
				roleAnimated = outlineAnimated;
				break;
			case ColorRole.AccentStrip:
				roleColor = accentColor;
				rolePinwheel = accentPinwheel;
				roleAnimated = accentAnimated;
				break;
			default:
				continue;
			}
			if (!(roleAnimated || _colorsDirty) || (rolePinwheel && !_colorsDirty))
			{
				continue;
			}
			for (int j = 0; j < trackedColorGroup.renderers.Count; j++)
			{
				Renderer val = trackedColorGroup.renderers[j];
				if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
				{
					continue;
				}
				if (rolePinwheel)
				{
					if ((UnityEngine.Object)(object)_activePinwheelMaterial != (UnityEngine.Object)null)
					{
						val.sharedMaterial = _activePinwheelMaterial;
					}
					continue;
				}
				Color color = roleColor;
				color.a = Settings.MenuOpacity;
				if ((UnityEngine.Object)(object)val.sharedMaterial == (UnityEngine.Object)(object)_activePinwheelMaterial || (UnityEngine.Object)(object)val.sharedMaterial == (UnityEngine.Object)null)
				{
					Material val2 = CreateColorMaterial(color);
					_dynamicMaterials.Add(val2);
					val.sharedMaterial = val2;
				}
				else
				{
					val.sharedMaterial.color = color;
				}
			}
		}
		if ((UnityEngine.Object)(object)Variables.title != (UnityEngine.Object)null && (titleAnimated || _colorsDirty))
		{
			Color animatedColor = Settings.GetAnimatedColor(Settings.TitleMode, (Color32)(Settings.TitleColor), (Color32)(Settings.TitleColor2), (Settings.TitleMode == Settings.ColorMode.Pinwheel) ? pinwheelSpeed : Settings.TitleAnimSpeed, 3);
			((Graphic)Variables.title).color = animatedColor;
		}
		_colorsDirty = false;
	}

	public static void SetUILayer(GameObject obj)
	{
		if ((UnityEngine.Object)(object)obj == (UnityEngine.Object)null)
		{
			return;
		}
		obj.layer = LayerMask.NameToLayer("UI");
		foreach (Transform child in obj.transform)
		{
			if ((UnityEngine.Object)(object)child != (UnityEngine.Object)null)
			{
				SetUILayer(((Component)child).gameObject);
			}
		}
	}

	public static void AddReturnButton()
	{
		Variables.BackToStartButton = CreateCube(null, null, keepCollider: true);
		Variables.BackToStartButton.transform.localScale = new Vector3(0.0075f, 0.2925f, 0.08f);
		Variables.BackToStartButton.transform.localPosition = new Vector3(0.07525f, 0f, -0.33f);
		ButtonHandler.BtnCollider btnCollider = Variables.BackToStartButton.AddComponent<ButtonHandler.BtnCollider>();
		btnCollider.clickedButton = new ButtonHandler.Button("ReturnButton", Category.Home, isToggle: false, isActive: false, null);
		ApplyColor(Variables.BackToStartButton, (Color32)(Settings.ButtonColor));
		List<GameObject> outline = ApplyOutline(Variables.BackToStartButton, 2, 0.0065f);
		List<GameObject> list = null;
		if (Settings.MenuRoundness > 0f)
		{
			list = RoundObj(Variables.BackToStartButton, null, null, Settings.MenuRoundness);
			RegisterColorGroup(ColorRole.Button, Variables.BackToStartButton, list);
			GameObject val = CreateQuad(null, Variables.canvasObj.transform);
			val.transform.localScale = HomeIconScale;
			val.transform.localPosition = new Vector3(0.05975f, 0f, -0.14875f);
			val.transform.localRotation = Quaternion.Euler(0f, -90f, -90f);
			ApplyTexture(val, "NXO.Resources.homeicon.png");
			RegisterClickAnim(btnCollider, Variables.BackToStartButton, list, outline, val.transform);
		}
		else
		{
			RegisterColorGroup(ColorRole.Button, Variables.BackToStartButton, list);
			GameObject val = CreateQuad(null, Variables.canvasObj.transform);
			val.transform.localScale = HomeIconScale;
			val.transform.localPosition = new Vector3(0.05975f, 0f, -0.14875f);
			val.transform.localRotation = Quaternion.Euler(0f, -90f, -90f);
			ApplyTexture(val, "NXO.Resources.homeicon.png");
			RegisterClickAnim(btnCollider, Variables.BackToStartButton, list, outline, val.transform);
		}
	}

	private static GameObject CreateCube(string name = null, Transform parent = null, bool keepCollider = false)
	{
		return CreatePrimitive((PrimitiveType)3, name, parent, keepCollider);
	}

	public static void CleanupFonts()
	{
		_fonts.Clear();
		foreach (Font current in _dynamicFonts)
		{
			if ((UnityEngine.Object)(object)current != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current);
			}
		}
		_dynamicFonts.Clear();
	}

	public static void OnPlayerLeave(NetPlayer player)
	{
		if (player != NetworkSystem.Instance.LocalPlayer)
		{
			NotifyRoomEvent("`" + player.NickName + "` Left");
			if (Variables.currentPage != Category.Players)
			{
				return;
			}
		}
		else if (Variables.currentPage != Category.Players)
		{
			return;
		}
		PlayersActionList.GeneratePlayerButtons();
	}

	public static Material CreateColorMaterial(Color color, int renderQueue = 2460)
	{
		if (Settings.MenuOpacity >= 1f)
		{
			return new Material(Variables.uberShader)
			{
				color = color
			};
		}
		Material val = new Material(Variables.uiShader);
		Color color2 = color;
		color2.a = Settings.MenuOpacity;
		val.color = color2;
		val.renderQueue = renderQueue;
		return val;
	}

	private static void CreateQuickActionButton(ButtonHandler.Button parent, float offset)
	{
		GameObject val = CreateCube("Gear: " + parent.buttonText, null, keepCollider: true);
		val.transform.localScale = new Vector3(0.0075f, 0.12f, 0.08f);
		val.transform.localPosition = new Vector3(0.07525f, -0.3525f, 0.335f - offset);
		ButtonHandler.BtnCollider btnCollider = val.AddComponent<ButtonHandler.BtnCollider>();
		btnCollider.clickedButton = new ButtonHandler.Button("_GEAR_" + parent.buttonText, parent.Page, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.OpenGear_Menu(parent);
		});
		ApplyColor(val, (Color32)(Settings.ButtonColor));
		List<GameObject> outline = ApplyOutline(val, 2, 0.0065f);
		List<GameObject> list = null;
		if (Settings.MenuRoundness > 0f)
		{
			list = RoundObj(val, null, null, Settings.MenuRoundness);
			RegisterColorGroup(ColorRole.Button, val, list);
			Text val2 = CreateCanvasText();
			val2.text = "⚙";
			((Component)val2).gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
			RectTransform component = ((Component)val2).GetComponent<RectTransform>();
			((Transform)component).localPosition = new Vector3(0.05975f, -0.1055f, TextOffset - offset / 2.225f);
			((Transform)component).localRotation = Quaternion.Euler(180f, 90f, 90f);
			component.sizeDelta = GearIconSize;
			RegisterClickAnim(btnCollider, val, list, outline, ((Component)val2).transform);
		}
		else
		{
			RegisterColorGroup(ColorRole.Button, val, list);
			Text val2 = CreateCanvasText();
			val2.text = "⚙";
			((Component)val2).gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
			RectTransform component = ((Component)val2).GetComponent<RectTransform>();
			((Transform)component).localPosition = new Vector3(0.05975f, -0.1055f, TextOffset - offset / 2.225f);
			((Transform)component).localRotation = Quaternion.Euler(180f, 90f, 90f);
			component.sizeDelta = GearIconSize;
			RegisterClickAnim(btnCollider, val, list, outline, ((Component)val2).transform);
		}
	}

	private static void CreateMenuCanvasAndTitle()
	{
		Variables.canvasObj = new GameObject("canvas");
		Variables.canvasObj.transform.parent = Variables.menuObj.transform;
		_trackedObjects.Add(Variables.canvasObj);
		Canvas val = Variables.canvasObj.AddComponent<Canvas>();
		val.renderMode = (RenderMode)2;
		val.sortingOrder = 100;
		Variables.canvasObj.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2750f;
		Variables.canvasObj.AddComponent<GraphicRaycaster>();
		Variables.title = CreateCanvasText(4);
		Variables.title.alignment = (TextAnchor)5;
		Variables.title.supportRichText = true;
		((Graphic)Variables.title).color = (Color32)(Settings.TitleColor);
		(float, float) titleOffset = TitleOffset;
		RectTransform component = ((Component)Variables.title).GetComponent<RectTransform>();
		((Transform)component).localPosition = Vector3.zero;
		((Transform)component).position = new Vector3(0.054f, titleOffset.Item1, titleOffset.Item2);
		((Transform)component).localRotation = Quaternion.Euler(180f, 90f, 90f);
		component.sizeDelta = TitleSize;
	}

	private static void TickActiveMods()
	{
		if (_activeTicksDirty)
		{
			_activeTicks.Clear();
			ButtonHandler.Button[] buttons = ModButtons.buttons;
			for (int i = 0; i < buttons.Length; i++)
			{
				ButtonHandler.Button button = buttons[i];
				if (button != null && button.isToggle && button.Enabled && button.onEnable != null)
				{
					_activeTicks.Add(button);
				}
			}
			_activeTicksDirty = false;
		}
		for (int j = 0; j < _activeTicks.Count; j++)
		{
			ButtonHandler.Button button2 = _activeTicks[j];
			if (button2 != null && button2.isToggle && button2.Enabled && button2.onEnable != null)
			{
				try
				{
					button2.onEnable();
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError((object)("[NXO] Active mod failed for '" + button2.buttonText + "': " + ex));
					button2.Enabled = false;
					NXOUI.RemoveMod(button2.buttonText);
				}
			}
		}
	}

	public static void Draw(bool openCall = false)
	{
		CreateBaseMenuObject();
		CreateMenuBackground();
		CreateMenuCanvasAndTitle();
		CreateAccentStrip();
		ToggleSearchButton();
		AddSearchBar();
		AddDisconnectButton();
		AddReturnButton();
		AddPageButton("<", 0.285f);
		AddPageButton(">", -0.285f);
		if (PlayersActionList.selectedPlayer != null && Variables.currentPage == Category.Player_Action)
		{
			PlayersActionList.AddPlayerCam();
		}
		DrawCurrentPageButtons();
		Transform transform = Variables.menuObj.transform;
		transform.localScale *= menuScale;
		if (openCall)
		{
			Vector3 localScale = Variables.menuObj.transform.localScale;
			Variables.menuObj.transform.localScale = Vector3.zero;
			((MonoBehaviour)Variables.playerInstance).StartCoroutine(BounceAnim(Variables.menuObj, localScale));
		}
		SetUILayer(Variables.menuObj);
	}

	private static void AddPageButton(string direction, float yPos)
	{
		GameObject val = CreateCube(null, null, keepCollider: true);
		val.transform.localScale = new Vector3(0.0075f, 0.25f, 0.08f);
		val.transform.localPosition = new Vector3(0.07525f, yPos, -0.33f);
		ButtonHandler.BtnCollider btnCollider = val.AddComponent<ButtonHandler.BtnCollider>();
		btnCollider.clickedButton = new ButtonHandler.Button(direction, Category.Home, isToggle: false, isActive: false, null);
		ApplyColor(val, (Color32)(Settings.ButtonColor));
		List<GameObject> outline = ApplyOutline(val, 2, 0.0065f);
		List<GameObject> list = null;
		if (Settings.MenuRoundness > 0f)
		{
			list = RoundObj(val, null, null, Settings.MenuRoundness);
			RegisterColorGroup(ColorRole.Button, val, list);
			Text val2 = CreateCanvasText();
			val2.text = direction;
			((Component)val2).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			RectTransform component = ((Component)val2).GetComponent<RectTransform>();
			((Transform)component).localPosition = Vector3.zero;
			component.sizeDelta = PageTextSize;
			((Transform)component).localRotation = Quaternion.Euler(180f, 90f, 90f);
			((Transform)component).localPosition = new Vector3(0.05975f, (direction == "<") ? 0.083f : (-0.083f), PageZOffset);
			RegisterClickAnim(btnCollider, val, list, outline, ((Component)val2).transform);
		}
		else
		{
			RegisterColorGroup(ColorRole.Button, val, list);
			Text val2 = CreateCanvasText();
			val2.text = direction;
			((Component)val2).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			RectTransform component = ((Component)val2).GetComponent<RectTransform>();
			((Transform)component).localPosition = Vector3.zero;
			component.sizeDelta = PageTextSize;
			((Transform)component).localRotation = Quaternion.Euler(180f, 90f, 90f);
			((Transform)component).localPosition = new Vector3(0.05975f, (direction == "<") ? 0.083f : (-0.083f), PageZOffset);
			RegisterClickAnim(btnCollider, val, list, outline, ((Component)val2).transform);
		}
	}

	public static void RegisterColorGroup(ColorRole role, GameObject baseObj, List<GameObject> extraParts)
	{
		if (role == ColorRole.None || (UnityEngine.Object)(object)baseObj == (UnityEngine.Object)null)
		{
			return;
		}
		TrackedColorGroup trackedColorGroup = new TrackedColorGroup
		{
			role = role
		};
		Renderer component = baseObj.GetComponent<Renderer>();
		if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
		{
			trackedColorGroup.renderers.Add(component);
		}
		if (extraParts != null)
		{
			for (int i = 0; i < extraParts.Count; i++)
			{
				if ((UnityEngine.Object)(object)extraParts[i] != (UnityEngine.Object)null)
				{
					Renderer component2 = extraParts[i].GetComponent<Renderer>();
					if ((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null)
					{
						trackedColorGroup.renderers.Add(component2);
					}
				}
			}
		}
		_colorGroups.Add(trackedColorGroup);
		_colorsDirty = true;
	}

	private static void DrawCurrentPageButtons()
	{
		bool flag = SearchAndKeyboard.isSearching && !SearchAndKeyboard.isTyping && !string.IsNullOrEmpty(SearchAndKeyboard.inputText);
		bool flag2 = flag != _lastWasSearching;
		bool flag3 = flag && (SearchAndKeyboard.inputText != _lastSearchInput || Variables.currentCategoryPage != _lastPageIndex);
		bool flag5 = !flag && Variables.currentPage != _lastPageDrawn;
		bool flag6 = !flag && Variables.currentCategoryPage != _lastPageIndex;
		bool flag4 = !flag && NXOUI._modListVersion != _lastDrawModVersion;
		if (!(flag2 || flag3 || flag5 || flag6 || flag4))
		{
			_drawingPageButtons = true;
			for (int i = 0; i < _pageCache.Count; i++)
			{
				AddModButtons((float)i * Variables.ButtonSpacing, _pageCache[i]);
			}
			_drawingPageButtons = false;
			return;
		}
		List<ButtonHandler.Button> newCacheScratch = _newCacheScratch;
		newCacheScratch.Clear();
		if (flag)
		{
			List<(ButtonHandler.Button, int)> scoredScratch = _scoredScratch;
			scoredScratch.Clear();
			ButtonHandler.Button[] buttons = ModButtons.buttons;
			for (int j = 0; j < buttons.Length; j++)
			{
				ButtonHandler.Button button = buttons[j];
				if (button != null && button.Page != Category.Home)
				{
					int num2 = SearchAndKeyboard.FuzzyScore(button.buttonText, SearchAndKeyboard.inputText);
					if (num2 > 0)
					{
						scoredScratch.Add((button, num2));
					}
				}
			}
			scoredScratch.Sort(((ButtonHandler.Button b, int score) a, (ButtonHandler.Button b, int score) z) => z.score.CompareTo(a.score));
			int num3 = Variables.currentCategoryPage * Variables.ButtonsPerPage;
			int num4 = Mathf.Min(num3 + Variables.ButtonsPerPage, scoredScratch.Count);
			for (int k = num3; k < num4; k++)
			{
				newCacheScratch.Add(scoredScratch[k].Item1);
			}
		}
		else
		{
			List<ButtonHandler.Button> buttonInfoByPage = ButtonHandler.GetButtonInfoByPage(Variables.currentPage);
			int num5 = 0;
			int num6 = Variables.currentCategoryPage * Variables.ButtonsPerPage;
			foreach (ButtonHandler.Button current in buttonInfoByPage)
			{
				if (current != null && Settings.ShouldShowButton(current.buttonText))
				{
					if (num5 >= num6 && newCacheScratch.Count < Variables.ButtonsPerPage)
					{
						newCacheScratch.Add(current);
					}
					num5++;
				}
			}
		}
		if (newCacheScratch.Count > 0 || !flag)
		{
			_pageCache.Clear();
			_pageCache.AddRange(newCacheScratch);
			_lastSearchInput = SearchAndKeyboard.inputText;
			_lastPageDrawn = Variables.currentPage;
			_lastPageIndex = Variables.currentCategoryPage;
			_lastDrawModVersion = NXOUI._modListVersion;
		}
		_lastWasSearching = flag;
		_drawingPageButtons = true;
		for (int l = 0; l < _pageCache.Count; l++)
		{
			AddModButtons((float)l * Variables.ButtonSpacing, _pageCache[l]);
		}
		_drawingPageButtons = false;
	}

	private static void HandleVrMenuState()
	{
		Variables.openMenu = Settings.MenuOpenButtonPressed();
		bool shouldOpenMenu = Variables.openMenu || SearchAndKeyboard.isSearching;
		if (shouldOpenMenu && !Variables.InPcCondition && !_wasInMenuCondition)
		{
			Variables.InMenuCondition = true;
			_wasInMenuCondition = true;
			if (!((UnityEngine.Object)(object)Variables.menuObj != (UnityEngine.Object)null))
			{
				Draw(openCall: true);
				if (Variables.OpenAndCloseMenuSounds)
				{
					AssetHandler.PlaySound(RigManager.GetHandObject, Variables.menuOpenSound, 0.625f);
				}
			}
			if (!((UnityEngine.Object)(object)Variables.clickerObj != (UnityEngine.Object)null))
			{
				AddButtonClicker(Variables.rightHandedMenu ? Variables.playerInstance.LeftHand.controllerTransform : Variables.playerInstance.RightHand.controllerTransform);
			}
			return;
		}
		if (!shouldOpenMenu && _wasInMenuCondition)
		{
			Variables.InMenuCondition = false;
			_wasInMenuCondition = false;
			if ((UnityEngine.Object)(object)Variables.menuObj != (UnityEngine.Object)null)
			{
				((MonoBehaviour)Variables.playerInstance).StartCoroutine(BounceAnim(Variables.menuObj, Vector3.zero, cleanupAfter: true));
				DestroyClicker();
				if (Variables.OpenAndCloseMenuSounds)
				{
					AssetHandler.PlaySound(RigManager.GetHandObject, Variables.menuCloseSound, 0.625f);
				}
			}
			return;
		}
		if (!Variables.InMenuCondition || !((UnityEngine.Object)(object)Variables.menuObj != (UnityEngine.Object)null))
		{
			return;
		}
		if (!((UnityEngine.Object)(object)Variables.clickerObj != (UnityEngine.Object)null))
		{
			AddButtonClicker(Variables.rightHandedMenu ? Variables.playerInstance.LeftHand.controllerTransform : Variables.playerInstance.RightHand.controllerTransform);
		}
		if (SearchAndKeyboard.isSearching)
		{
			if ((UnityEngine.Object)(object)SearchAndKeyboard.keyboardObj != (UnityEngine.Object)null)
			{
				PositionMenuOnKeyboard();
				AddTitleAndFPSCounter();
				return;
			}
		}
		PositionMenuForHand();
		AddTitleAndFPSCounter();
	}

	public static void RegisterFontFromBundle(string description, string bundlePath, string fontName)
	{
		AssetBundle val = AssetHandler.LoadAssetBundle(bundlePath);
		if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
		{
			Font val2 = val.LoadAsset<Font>(fontName);
			if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null)
			{
				RegisterFont(description, val2);
			}
		}
	}

	private static void DestroyClicker()
	{
		if ((UnityEngine.Object)(object)Variables.clickerObj != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)Variables.clickerObj);
			Variables.clickerObj = null;
			if (!((UnityEngine.Object)(object)_clickerMaterial != (UnityEngine.Object)null))
			{
				return;
			}
		}
		else if (!((UnityEngine.Object)(object)_clickerMaterial != (UnityEngine.Object)null))
		{
			return;
		}
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)_clickerMaterial);
		_clickerMaterial = null;
	}

	private static void StartPageFade()
	{
		if ((UnityEngine.Object)(object)Variables.menuObj == (UnityEngine.Object)null || (UnityEngine.Object)(object)Variables.playerInstance == (UnityEngine.Object)null)
		{
			return;
		}
		_fadeItems.Clear();
		_fadeSeen.Clear();
		for (int i = 0; i < _pageButtonObjects.Count; i++)
		{
			GameObject val = _pageButtonObjects[i];
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				continue;
			}
			Text component = val.GetComponent<Text>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				_fadeItems.Add(new FadeItem
				{
					txt = component,
					origA = ((Graphic)component).color.a
				});
				SetFadeAlpha(_fadeItems.Count - 1, 0f);
				continue;
			}
			Renderer component2 = val.GetComponent<Renderer>();
			Material val2 = ((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null) ? component2.sharedMaterial : null;
			if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null && _fadeSeen.Add(val2))
			{
				_fadeItems.Add(new FadeItem
				{
					r = component2,
					origA = val2.color.a
				});
				SetFadeAlpha(_fadeItems.Count - 1, 0f);
			}
		}
		if (_fadeItems.Count > 0)
		{
			_pageFadeRoutine = ((MonoBehaviour)Variables.playerInstance).StartCoroutine(PageFadeRoutine());
		}
	}

	public static void MarkActiveTicksDirty()
	{
		_activeTicksDirty = true;
	}

	private static IEnumerator PageFadeRoutine()
	{
		float elapsed = 0f;
		while (elapsed < 0.16f)
		{
			float factor = elapsed / 0.16f;
			for (int i = 0; i < _fadeItems.Count; i++)
			{
				SetFadeAlpha(i, factor);
			}
			elapsed += Time.deltaTime;
			yield return null;
		}
		for (int i = 0; i < _fadeItems.Count; i++)
		{
			SetFadeAlpha(i, 1f);
		}
		_pageFadeRoutine = null;
	}

	public static void ToggleSearchButton()
	{
		if ((UnityEngine.Object)(object)Variables.searchButton != (UnityEngine.Object)null)
		{
			return;
		}
		GameObject val = CreateCube("SearchBg", null, keepCollider: true);
		val.transform.localScale = new Vector3(0.0075f, HeaderIconScale.x * 2.504f, HeaderIconScale.x * 1.6f);
		val.transform.localPosition = new Vector3(0.07525f, -0.35f, 0.45f);
		val.AddComponent<ButtonHandler.BtnCollider>().clickedButton = new ButtonHandler.Button("Toggle Search Button", Category.Home, isToggle: true, isActive: false, null);
		ApplyColor(val, (Color32)(SearchAndKeyboard.isSearching ? Settings.EnabledButtonColor : Settings.ButtonColor));
		ApplyOutline(val, 2, 0.0065f);
		List<GameObject> extraParts = null;
		if (Settings.MenuRoundness > 0f)
		{
			extraParts = RoundObj(val, null, null, Settings.MenuRoundness);
		}
		RegisterColorGroup(SearchAndKeyboard.isSearching ? ColorRole.EnabledButton : ColorRole.Button, val, extraParts);
		Variables.searchButton = CreateQuad();
		Variables.searchButton.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
		Variables.searchButton.transform.localScale = HeaderIconScale;
		Variables.searchButton.transform.localPosition = new Vector3(0.079535715f, -0.35f, 0.45f);
		ApplyTexture(Variables.searchButton, "NXO.Resources.searchicon.png");
	}

	private static void CreateAccentStrip()
	{
		if (Settings.CurrentAccentStripType != Settings.AccentStripType.Off)
		{
			GameObject val = CreateCube();
			val.transform.localScale = AccentStripScale;
			val.transform.localPosition = AccentStripPos;
			ApplyStripColor(val, (Color32)(Settings.AccentStripColor));
			RegisterColorGroup(ColorRole.AccentStrip, val, null);
			if (Settings.CurrentAccentStripType == Settings.AccentStripType.Both)
			{
				GameObject val2 = CreateCube();
				val2.transform.localScale = AccentStripScale;
				val2.transform.localPosition = new Vector3(AccentStripPos.x, AccentStripPos.y, -0.27f);
				ApplyStripColor(val2, (Color32)(Settings.AccentStripColor2));
				RegisterColorGroup(ColorRole.AccentStrip, val2, null);
			}
		}
	}

	public static List<GameObject> RoundObj(GameObject toRound, Transform parent = null, List<Material> materialList = null, float roundness = 0.5f, int transparentQueue = 2460)
	{
		List<GameObject> list = new List<GameObject>(1);
		if ((UnityEngine.Object)(object)toRound == (UnityEngine.Object)null)
		{
			return list;
		}
		Renderer component = toRound.GetComponent<Renderer>();
		if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
		{
			return list;
		}
		if ((UnityEngine.Object)(object)parent == (UnityEngine.Object)null)
		{
			parent = Variables.menuObj.transform;
		}
		if (materialList == null)
		{
			materialList = MaterialSink;
		}
		Vector3 localScale = toRound.transform.localScale;
		Vector3 lossyScale = parent.lossyScale;
		float num = localScale.x * 0.5f;
		float num2 = localScale.y * 0.5f * lossyScale.y;
		float num3 = localScale.z * 0.5f * lossyScale.z;
		float num4 = Mathf.Min(0.0175f * roundness, Mathf.Min(num2, num3) * 0.9f);
		float num5 = (num2 - num4) / lossyScale.y;
		float num6 = (num3 - num4) / lossyScale.z;
		float num7 = num4 / lossyScale.y;
		float num8 = num4 / lossyScale.z;
		int capacity = 24;
		List<Vector2> list2 = new List<Vector2>(capacity);
		float[] array = new float[4]
		{
			num5,
			0f - num5,
			0f - num5,
			num5
		};
		float[] array2 = new float[4]
		{
			num6,
			num6,
			0f - num6,
			0f - num6
		};
		for (int num9 = 0; num9 < 4; num9++)
		{
			float num10 = (float)num9 * MathF.PI * 0.5f;
			float num11 = array[num9];
			float num12 = array2[num9];
			for (int num13 = 0; num13 <= 5; num13++)
			{
				float num14 = Mathf.Lerp(num10, num10 + MathF.PI / 2f, (float)num13 / 5f);
				list2.Add(new Vector2(num11 + Mathf.Cos(num14) * num7, num12 + Mathf.Sin(num14) * num8));
			}
		}
		int count = list2.Count;
		List<Vector3> list3 = new List<Vector3>(count * 4 + 4);
		List<Vector3> list4 = new List<Vector3>(count * 4 + 4);
		List<int> list5 = new List<int>(count * 12);
		int count2 = list3.Count;
		list3.Add(new Vector3(num, 0f, 0f));
		list4.Add(Vector3.right);
		int count3 = list3.Count;
		for (int num15 = 0; num15 < count; num15++)
		{
			list3.Add(new Vector3(num, list2[num15].x, list2[num15].y));
			list4.Add(Vector3.right);
		}
		for (int num16 = 0; num16 < count; num16++)
		{
			list5.Add(count2);
			list5.Add(count3 + (num16 + 1) % count);
			list5.Add(count3 + num16);
		}
		int count4 = list3.Count;
		list3.Add(new Vector3(0f - num, 0f, 0f));
		list4.Add(Vector3.left);
		int count5 = list3.Count;
		for (int num17 = 0; num17 < count; num17++)
		{
			list3.Add(new Vector3(0f - num, list2[num17].x, list2[num17].y));
			list4.Add(Vector3.left);
		}
		for (int num18 = 0; num18 < count; num18++)
		{
			list5.Add(count4);
			list5.Add(count5 + num18);
			list5.Add(count5 + (num18 + 1) % count);
		}
		int count6 = list3.Count;
		for (int num19 = 0; num19 < count; num19++)
		{
			Vector3 val = new Vector3(0f, list2[num19].x, list2[num19].y);
			Vector3 normalized = val.normalized;
			list3.Add(new Vector3(num, list2[num19].x, list2[num19].y));
			list4.Add(normalized);
			list3.Add(new Vector3(0f - num, list2[num19].x, list2[num19].y));
			list4.Add(normalized);
		}
		for (int num20 = 0; num20 < count; num20++)
		{
			int num21 = (num20 + 1) % count;
			int num22 = count6 + num20 * 2;
			int item = num22 + 1;
			int num23 = count6 + num21 * 2;
			int item2 = num23 + 1;
			list5.Add(num22);
			list5.Add(num23);
			list5.Add(item);
			list5.Add(num23);
			list5.Add(item2);
			list5.Add(item);
		}
		int count7 = list5.Count;
		for (int num24 = 0; num24 < count7; num24 += 3)
		{
			list5.Add(list5[num24]);
			list5.Add(list5[num24 + 2]);
			list5.Add(list5[num24 + 1]);
		}
		Mesh val2 = new Mesh
		{
			name = "RoundedMesh"
		};
		val2.SetVertices(list3);
		val2.SetNormals(list4);
		val2.SetTriangles(list5, 0);
		val2.RecalculateBounds();
		MeshSink.Add(val2);
		GameObject val3 = new GameObject("rounded_" + ((UnityEngine.Object)toRound).name);
		val3.transform.SetParent(parent, false);
		val3.transform.localPosition = toRound.transform.localPosition;
		val3.transform.localRotation = toRound.transform.localRotation;
		val3.transform.localScale = Vector3.one;
		val3.AddComponent<MeshFilter>().mesh = val2;
		Material val4;
		if (Settings.MenuOpacity >= 1f)
		{
			val4 = new Material(Variables.uberShader)
			{
				color = component.sharedMaterial.color
			};
		}
		else
		{
			val4 = new Material(Variables.uiShader);
			Color color = component.sharedMaterial.color;
			color.a = Settings.MenuOpacity;
			val4.color = color;
			val4.renderQueue = transparentQueue;
			val4.SetInt("_Cull", 0);
		}
		((Renderer)val3.AddComponent<MeshRenderer>()).sharedMaterial = val4;
		materialList.Add(val4);
		ObjectSink.Add(val3);
		component.enabled = false;
		list.Add(val3);
		return list;
	}

	public static void DestroyAndNullify<T>(ref T obj, float delay = 0f) where T : UnityEngine.Object
	{
		int num = 6;
		int num2 = 6;
		int[] array = new int[16];
		array[6] = 1;
		array[7] = 2;
		array[9] = 3;
		array[8] = 4;
		array[10] = 5;
		array[11] = 6;
		array[12] = 7;
		array[13] = 8;
		array[15] = 9;
		Component val = default(Component);
		while (num2 != 0)
		{
			switch (array[num + 0])
			{
			default:
			{
				int num3 = ((!((UnityEngine.Object)(object)obj == (UnityEngine.Object)null)) ? 1 : 0) * 2 + 7;
				num = num3;
				break;
			}
			case 2:
				num = 15;
				break;
			case 3:
			{
				object obj2 = obj;
				val = (Component)((obj2 is Component) ? obj2 : null);
				int num3 = ((val == null) ? 1 : 0) * 2 + 8;
				num = num3;
				break;
			}
			case 4:
			{
				int num3 = ((!((UnityEngine.Object)(object)val != (UnityEngine.Object)null)) ? 1 : 0) * 1 + 11;
				num = num3;
				break;
			}
			case 5:
			{
				int num3 = 1 * 1 + 11;
				num = num3;
				break;
			}
			case 6:
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)val.gameObject, delay);
				num = 13;
				break;
			case 7:
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)obj, delay);
				obj = default(T);
				num2 = 0;
				break;
			case 8:
				obj = default(T);
				num2 = 0;
				break;
			case 9:
				num2 = 0;
				break;
			case 0:
				return;
			}
		}
	}

	public static void AddTitleAndFPSCounter()
	{
		if (!((UnityEngine.Object)(object)Variables.title == (UnityEngine.Object)null) && !(Time.time - _lastTitleUpdateTime < 0.1f))
		{
			_lastTitleUpdateTime = Time.time;
			Variables.title.text = ((Settings.TitleMode == Settings.ColorMode.Pinwheel) ? BuildAnimatedTitle() : $"<b>NXO</b> v{NXO.Initialization.PluginInfo.menuVersion}");
		}
	}

	private static void ApplyColor(GameObject obj, Color color)
	{
		Renderer component = obj.GetComponent<Renderer>();
		if (!((UnityEngine.Object)(object)component == (UnityEngine.Object)null))
		{
			component.sharedMaterial = TrackMat(CreateColorMaterial(color));
		}
	}

	public static void RegisterFont(string description, Font font)
	{
		if ((UnityEngine.Object)(object)font != (UnityEngine.Object)null)
		{
			_fonts.Add(new FontEntry
			{
				Description = description,
				Font = font
			});
		}
	}

	public static void AddDisconnectButton()
	{
		GameObject val = CreateCube("DisconnectBg", null, keepCollider: true);
		_disconnectButtonBackground = val;
		val.transform.localScale = new Vector3(0.0075f, HeaderIconScale.x * 2.504f, HeaderIconScale.x * 1.6f);
		val.transform.localPosition = new Vector3(0.07525f, -0.2375f, 0.45f);
		val.AddComponent<ButtonHandler.BtnCollider>().clickedButton = new ButtonHandler.Button("Disconnect Button", Category.Home, isToggle: false, isActive: false, delegate
		{
			HandleDisconnectJoinButton();
		});
		ApplyColor(val, CurrentDisconnectJoinButtonColor());
		ApplyOutline(val, 2, 0.0065f);
		List<GameObject> extraParts = null;
		if (Settings.MenuRoundness > 0f)
		{
			extraParts = RoundObj(val, null, null, Settings.MenuRoundness);
			_disconnectButtonRoundedParts = extraParts;
			disconnectButton = CreateQuad();
			disconnectButton.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
			disconnectButton.transform.localScale = HeaderIconScale;
			disconnectButton.transform.localPosition = new Vector3(0.079535715f, -0.2375f, 0.45f);
			ApplyTexture(disconnectButton, "NXO.Resources.disconnecticon.png", Color.white);
		}
		else
		{
			_disconnectButtonRoundedParts = null;
			disconnectButton = CreateQuad();
			disconnectButton.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
			disconnectButton.transform.localScale = HeaderIconScale;
			disconnectButton.transform.localPosition = new Vector3(0.079535715f, -0.2375f, 0.45f);
			ApplyTexture(disconnectButton, "NXO.Resources.disconnecticon.png", Color.white);
		}
		_disconnectButtonWasInRoom = null;
		UpdateDisconnectJoinButton(force: true);
	}

	private static void HandleDisconnectJoinButton()
	{
		if (PhotonNetwork.InRoom)
		{
			Room.Disconnect();
		}
		else
		{
			Room.JoinRandomPublic();
		}
		UpdateDisconnectJoinButton(force: true);
	}

	private static Color32 CurrentDisconnectJoinButtonColor()
	{
		return PhotonNetwork.InRoom ? DisconnectButtonColor : JoinRandomButtonColor;
	}

	private static void UpdateDisconnectJoinButton(bool force = false)
	{
		if ((UnityEngine.Object)(object)_disconnectButtonBackground == (UnityEngine.Object)null)
		{
			return;
		}
		bool inRoom = PhotonNetwork.InRoom;
		if (!force && _disconnectButtonWasInRoom.HasValue && _disconnectButtonWasInRoom.Value == inRoom)
		{
			return;
		}
		_disconnectButtonWasInRoom = inRoom;
		Color32 color = CurrentDisconnectJoinButtonColor();
		SetRendererColor(_disconnectButtonBackground, color);
		if (_disconnectButtonRoundedParts != null)
		{
			for (int i = 0; i < _disconnectButtonRoundedParts.Count; i++)
			{
				SetRendererColor(_disconnectButtonRoundedParts[i], color);
			}
		}
	}

	private static void SetRendererColor(GameObject obj, Color color)
	{
		if ((UnityEngine.Object)(object)obj == (UnityEngine.Object)null)
		{
			return;
		}
		Renderer component = obj.GetComponent<Renderer>();
		if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null && (UnityEngine.Object)(object)component.sharedMaterial != (UnityEngine.Object)null)
		{
			Color val = color;
			val.a = component.sharedMaterial.color.a;
			component.sharedMaterial.color = val;
		}
	}

	private void LateUpdate()
	{
		if (!_lateUpdateLogged)
		{
			_lateUpdateLogged = true;
		}
		if ((UnityEngine.Object)(object)Variables.taggerInstance == (UnityEngine.Object)null)
		{
			Variables.taggerInstance = GorillaTagger.Instance;
		}
		if ((UnityEngine.Object)(object)Variables.playerInstance == (UnityEngine.Object)null)
		{
			Variables.playerInstance = GTPlayer.Instance;
		}
		if ((UnityEngine.Object)(object)Variables.thirdPersonCamera == (UnityEngine.Object)null)
		{
			Variables.thirdPersonCamera = GameObject.Find("Photon.Realtime.Player Objects/Third Person Camera/Shoulder Camera");
		}
		if (NetworkingLibrary.disableMenu || (UnityEngine.Object)(object)Variables.playerInstance == (UnityEngine.Object)null || (UnityEngine.Object)(object)Variables.taggerInstance == (UnityEngine.Object)null)
		{
			return;
		}
		if (Time.time - _lastAutoSaveTime >= 60f)
		{
			_lastAutoSaveTime = Time.time;
			if (Variables.AutoSave)
			{
				ButtonHandler.SaveAutosavedStuff();
			}
			else
			{
				Settings.SaveSettings();
			}
		}
		TickActiveMods();
		if (Keyboard.current != null && ((ButtonControl)Keyboard.current.rightAltKey).wasPressedThisFrame)
		{
			NXOUI.IsGuiVisible = !NXOUI.IsGuiVisible;
		}
		if (UnityInput.Current.GetKeyDown(Variables.PCMenuKey))
		{
			Variables.PCMenuOpen = !Variables.PCMenuOpen;
		}
		ControllerEmulator.Update();
		Macros.MacroPlaybackTick();
		RigManager.HandleGhostRig();
		HandlePlayerActionCamera();
		HandlePcMenuState();
		HandleVrMenuState();
		UpdateDisconnectJoinButton();
		SearchAndKeyboard.HandleKeyboardInput();
		UpdateAnimatedColors();
		SearchAndKeyboard.UpdateSearchBlink();
	}

	private static void HandlePcMenuState()
	{
		if (Variables.PCMenuOpen && !Variables.InMenuCondition && !InputHandler.LPrimary() && !InputHandler.RPrimary() && !Variables.menuOpen && !_wasInPcCondition)
		{
			Variables.InPcCondition = true;
			_wasInPcCondition = true;
			if (!((UnityEngine.Object)(object)Variables.menuObj != (UnityEngine.Object)null))
			{
				Draw(openCall: true);
				if (Variables.OpenAndCloseMenuSounds)
				{
					AssetHandler.PlaySound(GetPcSoundTarget(), Variables.menuOpenSound, 1.25f);
				}
			}
			return;
		}
		bool shouldClosePcMenu = !Variables.PCMenuOpen || Variables.InMenuCondition || InputHandler.LPrimary() || InputHandler.RPrimary() || Variables.menuOpen;
		if (shouldClosePcMenu && _wasInPcCondition)
		{
			Variables.InPcCondition = false;
			_wasInPcCondition = false;
			if (!((UnityEngine.Object)(object)Variables.menuObj != (UnityEngine.Object)null))
			{
				return;
			}
			if (Variables.OpenAndCloseMenuSounds)
			{
				AssetHandler.PlaySound(GetPcSoundTarget(), Variables.menuCloseSound, 1.25f);
				((MonoBehaviour)Variables.playerInstance).StartCoroutine(BounceAnim(Variables.menuObj, Vector3.zero, cleanupAfter: true));
				DestroyClicker();
				if (!SearchAndKeyboard.isSearching)
				{
					return;
				}
			}
			else
			{
				((MonoBehaviour)Variables.playerInstance).StartCoroutine(BounceAnim(Variables.menuObj, Vector3.zero, cleanupAfter: true));
				DestroyClicker();
				if (!SearchAndKeyboard.isSearching)
				{
					return;
				}
			}
			SearchAndKeyboard.CloseKeyboard();
			return;
		}
		if (!Variables.InPcCondition || !((UnityEngine.Object)(object)Variables.menuObj != (UnityEngine.Object)null))
		{
			return;
		}
		Transform anchor = GetPcMenuAnchor();
		AddButtonClicker(anchor);
		AddTitleAndFPSCounter();
		if ((UnityEngine.Object)(object)anchor == (UnityEngine.Object)null)
		{
			return;
		}
		Variables.menuObj.transform.SetParent(anchor, true);
		Variables.menuObj.transform.position = anchor.position + anchor.rotation * Vector3.forward * 0.5f + Vector3.down * 0.025f;
		Variables.menuObj.transform.rotation = Quaternion.LookRotation(-anchor.forward, Vector3.up);
		Variables.menuObj.transform.Rotate(Vector3.up, -90f);
		Variables.menuObj.transform.Rotate(Vector3.right, -90f);
		if (Mouse.current != null && Mouse.current.leftButton.isPressed)
		{
			if ((UnityEngine.Object)(object)Variables.clickerObj != (UnityEngine.Object)null)
			{
				MeshRenderer component = Variables.clickerObj.GetComponent<MeshRenderer>();
				if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null && !((Renderer)component).enabled)
				{
					((Renderer)component).enabled = true;
				}
			}
			Camera pcCamera = GetPcCamera();
			if ((UnityEngine.Object)(object)pcCamera == (UnityEngine.Object)null)
			{
				return;
			}
			Ray val = pcCamera.ScreenPointToRay((Vector2)(((InputControl<Vector2>)(object)((Pointer)Mouse.current).position).ReadValue()));
			RaycastHit[] array = Physics.RaycastAll(val, 100f, UILayerMask);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit val2 = array[i];
				Collider collider = val2.collider;
				ButtonHandler.BtnCollider btnCollider = ((collider != null) ? ((Component)collider).GetComponent<ButtonHandler.BtnCollider>() : null);
				if ((UnityEngine.Object)(object)btnCollider != (UnityEngine.Object)null && (UnityEngine.Object)(object)Variables.clickerObj != (UnityEngine.Object)null)
				{
					btnCollider.OnTriggerEnter(Variables.clickerObj.GetComponent<Collider>());
					break;
				}
			}
			return;
		}
		if ((UnityEngine.Object)(object)Variables.clickerObj != (UnityEngine.Object)null && Mouse.current != null && !Mouse.current.leftButton.isPressed)
		{
			MeshRenderer component2 = Variables.clickerObj.GetComponent<MeshRenderer>();
			if ((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null && ((Renderer)component2).enabled)
			{
				((Renderer)component2).enabled = false;
			}
		}
	}

	private static Camera GetPcCamera()
	{
		if ((UnityEngine.Object)(object)_pcCamera != (UnityEngine.Object)null)
		{
			return _pcCamera;
		}
		if ((UnityEngine.Object)(object)Variables.thirdPersonCamera != (UnityEngine.Object)null)
		{
			_pcCamera = Variables.thirdPersonCamera.GetComponent<Camera>();
			if ((UnityEngine.Object)(object)_pcCamera != (UnityEngine.Object)null)
			{
				return _pcCamera;
			}
		}
		_pcCamera = Camera.main;
		return _pcCamera;
	}

	private static Transform GetPcMenuAnchor()
	{
		Camera camera = GetPcCamera();
		if ((UnityEngine.Object)(object)camera != (UnityEngine.Object)null)
		{
			return ((Component)camera).transform;
		}
		if ((UnityEngine.Object)(object)Variables.thirdPersonCamera != (UnityEngine.Object)null)
		{
			return Variables.thirdPersonCamera.transform;
		}
		if ((UnityEngine.Object)(object)Variables.taggerInstance?.mainCamera != (UnityEngine.Object)null)
		{
			return Variables.taggerInstance.mainCamera.transform;
		}
		if ((UnityEngine.Object)(object)Variables.playerInstance?.headCollider != (UnityEngine.Object)null)
		{
			return ((Component)Variables.playerInstance.headCollider).transform;
		}
		return null;
	}

	private static GameObject GetPcSoundTarget()
	{
		if ((UnityEngine.Object)(object)Variables.thirdPersonCamera != (UnityEngine.Object)null)
		{
			return Variables.thirdPersonCamera;
		}
		Camera camera = GetPcCamera();
		if ((UnityEngine.Object)(object)camera != (UnityEngine.Object)null)
		{
			return ((Component)camera).gameObject;
		}
		return null;
	}

	private static void PositionMenuForHand()
	{
		if ((UnityEngine.Object)(object)Variables.menuObj == (UnityEngine.Object)null || (UnityEngine.Object)(object)Variables.playerInstance == (UnityEngine.Object)null)
		{
			return;
		}
		HandState hand = Variables.rightHandedMenu ? Variables.playerInstance.RightHand : Variables.playerInstance.LeftHand;
		Transform controllerTransform = hand.controllerTransform;
		Variables.menuObj.transform.position = controllerTransform.position - controllerTransform.up * 0.035f + controllerTransform.right * 0.006f;
		Vector3 eulerAngles = controllerTransform.rotation.eulerAngles;
		if (Variables.rightHandedMenu)
		{
			eulerAngles += new Vector3(0f, 0f, 180f);
		}
		Variables.menuObj.transform.rotation = Quaternion.Euler(eulerAngles);
	}

	private static void ApplyStripColor(GameObject obj, Color color)
	{
		Renderer component = obj.GetComponent<Renderer>();
		if (!((UnityEngine.Object)(object)component == (UnityEngine.Object)null))
		{
			Material mat;
			if (Settings.MenuOpacity >= 1f)
			{
				mat = new Material(Variables.uberShader)
				{
					color = color
				};
				component.sharedMaterial = TrackMat(mat);
				return;
			}
			mat = new Material(Variables.uiShader);
			Color color2 = color;
			color2.a = Settings.MenuOpacity;
			mat.color = color2;
			mat.renderQueue = 2460;
			component.sharedMaterial = TrackMat(mat);
		}
	}

	private static void HandlePlayerActionCamera()
	{
		if (PhotonNetwork.InRoom)
		{
			if (Variables.currentPage == Category.Player_Action && PlayersActionList.selectedPlayer != null)
			{
				PlayersActionList.UpdatePlayerCam();
			}
		}
		else
		{
			Variables.notificationSent = false;
			PlayersActionList.ClearPlayerCam();
		}
	}

	public static void AddButtonClicker(Transform parentTransform)
	{
		if ((UnityEngine.Object)(object)Variables.clickerObj != (UnityEngine.Object)null)
		{
			return;
		}
		Variables.clickerObj = new GameObject("buttonclicker");
		((Collider)Variables.clickerObj.AddComponent<BoxCollider>()).isTrigger = true;
		Variables.clickerObj.layer = LayerMask.NameToLayer("UI");
		Variables.clickerObj.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
		if ((UnityEngine.Object)(object)_clickerMaterial == (UnityEngine.Object)null)
		{
			_clickerMaterial = new Material(Variables.uberShader)
			{
				color = Color.white
			};
			((Renderer)Variables.clickerObj.AddComponent<MeshRenderer>()).sharedMaterial = _clickerMaterial;
			if ((UnityEngine.Object)(object)parentTransform == (UnityEngine.Object)null)
			{
				return;
			}
		}
		else
		{
			((Renderer)Variables.clickerObj.AddComponent<MeshRenderer>()).sharedMaterial = _clickerMaterial;
			if ((UnityEngine.Object)(object)parentTransform == (UnityEngine.Object)null)
			{
				return;
			}
		}
		Variables.clickerObj.transform.SetParent(parentTransform);
		Variables.clickerObj.transform.localScale = Vector3.one * 0.0035f;
		Variables.clickerObj.transform.localPosition = ClickerLocalPos;
	}

	public static void CleanupMenu()
	{
		PlayersActionList.ClearPlayerCam();
		_colorGroups.Clear();
		_colorsDirty = true;
		foreach (GameObject current in _pageButtonObjects)
		{
			if ((UnityEngine.Object)(object)current != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current);
			}
		}
		_pageButtonObjects.Clear();
		foreach (GameObject current2 in _trackedObjects)
		{
			if ((UnityEngine.Object)(object)current2 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current2);
			}
		}
		_trackedObjects.Clear();
		Main.DestroyAndNullify<GameObject>(ref Variables.menuObj, 0f);
		DestroyClicker();
		foreach (Material current3 in _pageButtonMaterials)
		{
			if ((UnityEngine.Object)(object)current3 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current3);
			}
		}
		_pageButtonMaterials.Clear();
		foreach (Material current4 in _dynamicMaterials)
		{
			if ((UnityEngine.Object)(object)current4 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current4);
			}
		}
		_dynamicMaterials.Clear();
		foreach (Material current5 in _dynamicPinwheelMaterials)
		{
			if ((UnityEngine.Object)(object)current5 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current5);
			}
		}
		_dynamicPinwheelMaterials.Clear();
		_activePinwheelMaterial = null;
		foreach (Mesh current6 in _pageButtonMeshes)
		{
			if ((UnityEngine.Object)(object)current6 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current6);
			}
		}
		_pageButtonMeshes.Clear();
		foreach (Mesh current7 in _dynamicMeshes)
		{
			if ((UnityEngine.Object)(object)current7 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current7);
			}
		}
		_dynamicMeshes.Clear();
		Variables.background = null;
		Variables.TopMenuButton = null;
		Variables.searchButton = null;
		disconnectButton = null;
		_disconnectButtonBackground = null;
		_disconnectButtonRoundedParts = null;
		_disconnectButtonWasInRoom = null;
		Variables.canvasObj = null;
		Variables.BackToStartButton = null;
		Variables.title = null;
		SearchAndKeyboard.searchText = null;
		_pageCache.Clear();
		_lastDrawModVersion = -1;
		_lastWasSearching = false;
		_lastSearchInput = null;
		_drawingPageButtons = false;
	}

	private static void CreateMenuBackground()
	{
		Variables.background = CreateCube("menucolor");
		Variables.background.transform.localScale = BackgroundScale;
		Variables.background.transform.position = BackgroundPos;
		ApplyColor(Variables.background, (Color32)(Settings.BackgroundColor));
		ApplyOutline(Variables.background, 1);
		List<GameObject> extraParts = null;
		if (Settings.MenuRoundness > 0f)
		{
			extraParts = RoundObj(Variables.background, null, null, Settings.MenuRoundness, 2450);
			RegisterColorGroup(ColorRole.Background, Variables.background, extraParts);
		}
		else
		{
			RegisterColorGroup(ColorRole.Background, Variables.background, extraParts);
		}
	}

	public static void OnJoinRoom()
	{
		if (!NotificationLib.inRoom)
		{
			NotificationLib.inRoom = true;
			NetworkingLibrary.BroadcastNXOUser();
			Room.roomCode = PhotonNetwork.CurrentRoom.Name;
			NotifyRoomEvent("Joined Code `" + Room.roomCode + "`");
			((MonoBehaviour)Variables.playerInstance).StartCoroutine(PlayersActionList.DelayedPlayerButtonGeneration());
		}
	}

	private static void CreateBaseMenuObject()
	{
		Variables.menuObj = GameObject.CreatePrimitive((PrimitiveType)3);
		((UnityEngine.Object)Variables.menuObj).name = "menu";
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)Variables.menuObj.GetComponent<Rigidbody>());
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)Variables.menuObj.GetComponent<BoxCollider>());
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)Variables.menuObj.GetComponent<Renderer>());
		Variables.menuObj.transform.localScale = MenuSize;
	}

	private static void SetFadeAlpha(int i, float factor)
	{
		FadeItem fadeItem = _fadeItems[i];
		float a = fadeItem.origA * factor;
		if ((UnityEngine.Object)(object)fadeItem.txt != (UnityEngine.Object)null)
		{
			Color color = ((Graphic)fadeItem.txt).color;
			color.a = a;
			((Graphic)fadeItem.txt).color = color;
		}
		else if ((UnityEngine.Object)(object)fadeItem.r != (UnityEngine.Object)null && (UnityEngine.Object)(object)fadeItem.r.sharedMaterial != (UnityEngine.Object)null)
		{
			Color color2 = fadeItem.r.sharedMaterial.color;
			color2.a = a;
			fadeItem.r.sharedMaterial.color = color2;
		}
	}

	private static GameObject CreateOutline(GameObject target, Transform parent, float zGrow = 0.0125f)
	{
		GameObject val = CreateCube(null, parent);
		val.transform.position = target.transform.position;
		val.transform.rotation = target.transform.rotation;
		Vector3 localScale = target.transform.localScale;
		Vector3 lossyScale = parent.lossyScale;
		float num = zGrow * (lossyScale.z / lossyScale.y);
		val.transform.localScale = new Vector3(localScale.x - 0.0025f, localScale.y + num, localScale.z + zGrow);
		ApplyColor(val, (Color32)(Settings.OutlineColor));
		return val;
	}
}
