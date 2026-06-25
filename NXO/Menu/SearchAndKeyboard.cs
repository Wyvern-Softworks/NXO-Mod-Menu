using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using NXO.Mods.Categories;
using UnityEngine;
using UnityEngine.UI;

namespace NXO.Menu;

public class SearchAndKeyboard : MonoBehaviour
{
	public class KeyCollider : MonoBehaviour
	{
		public string key;

		public Vector3 baseScale;

		public GameObject roundedHolder;

		private Coroutine _bounceRoutine;

		private static float _kcpsCooldown;

		private static float ElasticOut(float t)
		{
			if (t <= 0f)
			{
				return 0f;
			}
			if (t >= 1f)
			{
				return 1f;
			}
			return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.1f) * (MathF.PI * 2f) / 0.4f) + 1f;
		}

		private IEnumerator BounceRoutine()
		{
			Transform target = (((UnityEngine.Object)(object)roundedHolder != (UnityEngine.Object)null) ? roundedHolder.transform : ((Component)this).transform);
			Vector3 targetBase = (((UnityEngine.Object)(object)roundedHolder != (UnityEngine.Object)null) ? Vector3.one : baseScale);
			Vector3 punched = new Vector3(targetBase.x * 1.25f, targetBase.y * 1.25f, targetBase.z * 0.7f);
			float halfDur = 0.063f;
			float elapsed = 0f;
			while (elapsed < halfDur)
			{
				if ((UnityEngine.Object)(object)this == (UnityEngine.Object)null)
				{
					yield break;
				}
				float t = elapsed / halfDur;
				Vector3 scale = Vector3.LerpUnclamped(targetBase, punched, 1f - Mathf.Pow(1f - t, 3f));
				target.localScale = scale;
				if ((UnityEngine.Object)(object)roundedHolder != (UnityEngine.Object)null)
				{
					((Component)this).transform.localScale = new Vector3(scale.x * baseScale.x, scale.y * baseScale.y, scale.z * baseScale.z);
				}
				elapsed += Time.deltaTime;
				yield return null;
			}
			elapsed = 0f;
			float restDur = 0.18f - halfDur;
			while (elapsed < restDur)
			{
				if ((UnityEngine.Object)(object)this == (UnityEngine.Object)null)
				{
					yield break;
				}
				float t2 = elapsed / restDur;
				Vector3 scale2 = Vector3.LerpUnclamped(punched, targetBase, ElasticOut(t2));
				target.localScale = scale2;
				if ((UnityEngine.Object)(object)roundedHolder != (UnityEngine.Object)null)
				{
					((Component)this).transform.localScale = new Vector3(scale2.x * baseScale.x, scale2.y * baseScale.y, scale2.z * baseScale.z);
				}
				elapsed += Time.deltaTime;
				yield return null;
			}
			if ((UnityEngine.Object)(object)this != (UnityEngine.Object)null)
			{
				target.localScale = targetBase;
				((Component)this).transform.localScale = baseScale;
			}
			_bounceRoutine = null;
		}

		private void OnTriggerEnter(Collider collider)
		{
			if (!(((UnityEngine.Object)((Component)collider).gameObject).name != "keyclicker") && !((float)Time.frameCount < _kcpsCooldown + 12.5f))
			{
				_kcpsCooldown = Time.frameCount;
				HandleKeyPress(key);
			}
		}

		public void TriggerBounce()
		{
			if (((Component)this).gameObject.activeInHierarchy)
			{
				if (_bounceRoutine != null)
				{
					((MonoBehaviour)this).StopCoroutine(_bounceRoutine);
					_bounceRoutine = ((MonoBehaviour)this).StartCoroutine(BounceRoutine());
				}
				else
				{
					_bounceRoutine = ((MonoBehaviour)this).StartCoroutine(BounceRoutine());
				}
			}
		}
	}

	public static Text searchText;

	public static string inputText = "";

	public static GameObject keyboardObj;

	public static bool isSearching;

	public static bool shouldShowSearchText;

	public static bool isTyping;

	private static Material _keyboardBgMaterial;

	private static Material _outlineMaterial;

	private static readonly List<Material> _keyboardMaterials = new List<Material>();

	private static readonly List<GameObject> _keyboardObjects = new List<GameObject>();

	private static readonly List<Mesh> _keyboardMeshes = new List<Mesh>();

	public static List<KeyCode> lastPressedKeys = new List<KeyCode>();

	private static readonly Dictionary<string, KeyCollider> _keyColliders = new Dictionary<string, KeyCollider>();

	private static readonly List<(Main.ColorRole role, GameObject obj, List<GameObject> parts)> _keyboardColorGroups = new List<(Main.ColorRole, GameObject, List<GameObject>)>();

	private const float KeyBouncePunch = 1.25f;

	private const float KeyBounceDuration = 0.18f;

	private static readonly KeyCode[] _allowedKeys;

	public static string placeholderText;

	public static float _preSarchMenuScale;

	private static float _blinkTimer;

	private static bool _cursorVisible;

	public static Action<string> onTypingComplete;

	public static Action onTypingCancelled;

	private static Material _clickerMaterial1;

	private static Material _clickerMaterial2;

	public static KeyCode[] allowedKeys => _allowedKeys;

	private static GameObject CreateKeyboardPrimitive(Transform parent)
	{
		GameObject val = GameObject.CreatePrimitive((PrimitiveType)3);
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)val.GetComponent<Rigidbody>());
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)val.GetComponent<BoxCollider>());
		val.layer = LayerMask.NameToLayer("UI");
		val.transform.SetParent(parent, false);
		val.transform.localRotation = Quaternion.identity;
		_keyboardObjects.Add(val);
		return val;
	}

	private static void CreateKeyClicker(ref GameObject keyclickerObj, Transform parentTransform, ref Material clickerMaterial)
	{
		if ((UnityEngine.Object)(object)keyclickerObj != (UnityEngine.Object)null)
		{
			return;
		}
		keyclickerObj = new GameObject("keyclicker");
		((Collider)keyclickerObj.AddComponent<BoxCollider>()).isTrigger = true;
		keyclickerObj.layer = LayerMask.NameToLayer("UI");
		keyclickerObj.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
		clickerMaterial = new Material(Variables.uberShader)
		{
			color = Color.white
		};
		((Renderer)keyclickerObj.AddComponent<MeshRenderer>()).material = clickerMaterial;
		if ((UnityEngine.Object)(object)parentTransform != (UnityEngine.Object)null)
		{
			keyclickerObj.transform.SetParent(parentTransform);
			keyclickerObj.transform.localScale = new Vector3(0.0035f, 0.0035f, 0.0035f);
			keyclickerObj.transform.localPosition = new Vector3(0f, -0.1f, 0f);
		}
	}

	public static void CloseKeyboard()
	{
		isSearching = false;
		isTyping = false;
		onTypingComplete = null;
		onTypingCancelled = null;
		shouldShowSearchText = false;
		Main._lastKeyboardYRotation = float.MinValue;
		Main._lastKeyboardSnapPosition = Vector3.zero;
		if (_preSarchMenuScale > 0f)
		{
			Main.menuScale = _preSarchMenuScale;
			_preSarchMenuScale = -1f;
		}
		CleanupKeyboard();
		Main.RefreshMenu();
	}

	private static Material CreateButtonMaterial()
	{
		Material val;
		if (Settings.ButtonMode == Settings.ColorMode.Pinwheel)
		{
			Material pinwheelMaterial = Main.GetPinwheelMaterial();
			if ((UnityEngine.Object)(object)pinwheelMaterial != (UnityEngine.Object)null)
			{
				_keyboardMaterials.Add(pinwheelMaterial);
				return pinwheelMaterial;
			}
			val = CreateMaterial((Color32)(Settings.ButtonColor), 2460);
			_keyboardMaterials.Add(val);
			return val;
		}
		val = CreateMaterial((Color32)(Settings.ButtonColor), 2460);
		_keyboardMaterials.Add(val);
		return val;
	}

	static SearchAndKeyboard()
	{
		_allowedKeys = new KeyCode[]
		{
			KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
			KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
			KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,
			KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
			KeyCode.Space, KeyCode.Backspace, KeyCode.Delete
		};
		placeholderText = "Type to search...";
		_preSarchMenuScale = -1f;
		_cursorVisible = true;
	}

	public static void HandleKeyboardInput()
	{
		if (!Variables.InPcCondition || (!isSearching && !isTyping))
		{
			return;
		}
		lastPressedKeys.Clear();
		for (int i = 0; i < allowedKeys.Length; i++)
		{
			KeyCode keyCode = allowedKeys[i];
			if (UnityInput.Current.GetKeyDown(keyCode))
			{
				HandleKeyPress(KeyCodeToString(keyCode));
				lastPressedKeys.Add(keyCode);
			}
		}
	}

	public static void KeyClicker1(Transform parentTransform)
	{
		CreateKeyClicker(ref Variables.keyclickerObj1, parentTransform, ref _clickerMaterial1);
	}

	public static int FuzzyScore(string text, string query)
	{
		if (string.IsNullOrEmpty(query))
		{
			return 0;
		}
		if (string.IsNullOrEmpty(text))
		{
			return int.MinValue;
		}
		string text2 = text.ToLowerInvariant();
		string text3 = query.ToLowerInvariant();
		int num = text2.IndexOf(text3);
		if (num == 0)
		{
			return 10000;
		}
		if (num > 0)
		{
			return 5000 - num;
		}
		int score = 0;
		int queryIndex = 0;
		int lastMatch = -1;
		int consecutive = 0;
		int textIndex = 0;
		while (textIndex < text2.Length && queryIndex < text3.Length)
		{
			if (text2[textIndex] == text3[queryIndex])
			{
				if (lastMatch == textIndex - 1)
				{
					consecutive++;
				}
				else
				{
					consecutive = 0;
				}
				score += 10 + consecutive * 8;
				if (textIndex == 0 || text2[textIndex - 1] == ' ')
				{
					score += 15;
				}
				lastMatch = textIndex;
				queryIndex++;
			}
			textIndex++;
		}
		if (queryIndex < text3.Length)
		{
			return int.MinValue;
		}
		return score - (text2.Length - text3.Length);
	}

	public static void KeyClicker2(Transform parentTransform)
	{
		CreateKeyClicker(ref Variables.keyclickerObj2, parentTransform, ref _clickerMaterial2);
	}

	public static void CleanupKeyboard()
	{
		if ((UnityEngine.Object)(object)keyboardObj != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)keyboardObj);
			keyboardObj = null;
		}
		if ((UnityEngine.Object)(object)Variables.keyclickerObj1 != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)Variables.keyclickerObj1);
			Variables.keyclickerObj1 = null;
		}
		if ((UnityEngine.Object)(object)Variables.keyclickerObj2 != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)Variables.keyclickerObj2);
			Variables.keyclickerObj2 = null;
		}
		foreach (Material material in _keyboardMaterials)
		{
			if ((UnityEngine.Object)(object)material != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)material);
			}
		}
		_keyboardMaterials.Clear();
		foreach (GameObject keyboardObject in _keyboardObjects)
		{
			if ((UnityEngine.Object)(object)keyboardObject != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)keyboardObject);
			}
		}
		_keyboardObjects.Clear();
		foreach (Mesh keyboardMesh in _keyboardMeshes)
		{
			if ((UnityEngine.Object)(object)keyboardMesh != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)keyboardMesh);
			}
		}
		_keyboardMeshes.Clear();
		if ((UnityEngine.Object)(object)_clickerMaterial1 != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_clickerMaterial1);
			_clickerMaterial1 = null;
		}
		if ((UnityEngine.Object)(object)_clickerMaterial2 != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_clickerMaterial2);
			_clickerMaterial2 = null;
		}
		_keyboardBgMaterial = null;
		_outlineMaterial = null;
		_keyboardColorGroups.Clear();
		_keyColliders.Clear();
		inputText = "";
	}

	public static void DrawKeyboard()
	{
		if ((UnityEngine.Object)(object)keyboardObj != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)keyboardObj);
			keyboardObj = null;
		}
		foreach (Material material in _keyboardMaterials)
		{
			if ((UnityEngine.Object)(object)material != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)material);
			}
		}
		_keyboardMaterials.Clear();
		foreach (GameObject keyboardObject in _keyboardObjects)
		{
			if ((UnityEngine.Object)(object)keyboardObject != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)keyboardObject);
			}
		}
		_keyboardObjects.Clear();
		foreach (Mesh keyboardMesh in _keyboardMeshes)
		{
			if ((UnityEngine.Object)(object)keyboardMesh != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)keyboardMesh);
			}
		}
		_keyboardMeshes.Clear();
		_keyboardColorGroups.Clear();
		_keyColliders.Clear();
		keyboardObj = new GameObject("Keyboard");
		if (Settings.OutlineMode > 0)
		{
			GameObject val = CreateKeyboardPrimitive(keyboardObj.transform);
			((UnityEngine.Object)val).name = "KeyboardOutline";
			val.transform.localScale = new Vector3(0.5f, 0.255f, 0.004f);
			val.transform.localPosition = new Vector3(0f, 0.002f, 0.006f);
			_outlineMaterial = CreateOutlineMaterial();
			_keyboardMaterials.Add(_outlineMaterial);
			val.GetComponent<Renderer>().sharedMaterial = _outlineMaterial;
			List<GameObject> parts = null;
			if (Settings.MenuRoundness > 0f)
			{
				parts = RoundKeyboard(val, 0.015f * Settings.MenuRoundness);
			}
			RegisterKeyboardColorGroup(Main.ColorRole.Outline, val, parts);
		}
		GameObject val2 = CreateKeyboardPrimitive(keyboardObj.transform);
		((UnityEngine.Object)val2).name = "KeyboardBackground";
		val2.transform.localScale = new Vector3(0.485f, 0.24f, 0.005f);
		val2.transform.localPosition = new Vector3(0f, 0.002f, 0.005f);
		_keyboardBgMaterial = CreateBackgroundMaterial();
		val2.GetComponent<Renderer>().sharedMaterial = _keyboardBgMaterial;
		List<GameObject> parts2 = null;
		if (Settings.MenuRoundness > 0f)
		{
			parts2 = RoundKeyboard(val2, 0.015f * Settings.MenuRoundness);
		}
		RegisterKeyboardColorGroup(Main.ColorRole.Background, val2, parts2);
		string[] array = new string[4] { "1234567890", "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
		float num = 0.04f;
		float num2 = 0.005f;
		float num3 = 0.092f;
		foreach (string text in array)
		{
			float num4 = (float)text.Length * num + (float)(text.Length - 1) * num2;
			float num5 = 0f - num4 / 2f + num / 2f;
			foreach (char c in text)
			{
				CreateKey(c.ToString(), new Vector3(num5, num3, 0f), num);
				num5 += num + num2;
			}
			num3 -= num + num2;
		}
		float num7 = num3;
		float num8 = num * 5f;
		float num9 = num * 2.1f;
		float num10 = num * 2.1f;
		float num11 = num8 + num2 + num9 + num2 + num10;
		float num12 = 0f - num11 / 2f;
		CreateKey("SPACE", new Vector3(num12 + num8 / 2f, num7, 0f), num8);
		CreateKey("BACK", new Vector3(num12 + num8 + num2 + num9 / 2f, num7, 0f), num9);
		CreateKey("ENTER", new Vector3(num12 + num8 + num2 + num9 + num2 + num10 / 2f, num7, 0f), num10);
	}

	private static Material CreateOutlineMaterial()
	{
		if (Settings.OutlineColorMode == Settings.ColorMode.Pinwheel)
		{
			Material pinwheelMaterial = Main.GetPinwheelMaterial();
			if ((UnityEngine.Object)(object)pinwheelMaterial != (UnityEngine.Object)null)
			{
				_keyboardMaterials.Add(pinwheelMaterial);
				return pinwheelMaterial;
			}
			return CreateMaterial((Color32)(Settings.OutlineColor), 2455);
		}
		return CreateMaterial((Color32)(Settings.OutlineColor), 2455);
	}

	private static void CreateKey(string key, Vector3 position, float width, float height = 0.04f)
	{
		GameObject val = GameObject.CreatePrimitive((PrimitiveType)3);
		BoxCollider component = val.GetComponent<BoxCollider>();
		((Collider)component).isTrigger = true;
		val.layer = LayerMask.NameToLayer("UI");
		val.transform.SetParent(keyboardObj.transform, false);
		val.transform.localScale = new Vector3(width - 0.0025f, height - 0.0025f, 0.012f);
		val.transform.localPosition = position;
		val.transform.localRotation = Quaternion.identity;
		_keyboardObjects.Add(val);
		Rigidbody val2 = val.AddComponent<Rigidbody>();
		val2.isKinematic = true;
		val2.useGravity = false;
		Material sharedMaterial = CreateButtonMaterial();
		val.GetComponent<Renderer>().sharedMaterial = sharedMaterial;
		GameObject val3 = new GameObject("KeyCanvas");
		val3.transform.SetParent(val.transform, false);
		val3.transform.localPosition = new Vector3(0f, 0f, -0.52f);
		val3.transform.localRotation = Quaternion.identity;
		float num = width - 0.0025f;
		float num2 = height - 0.0025f;
		float num3 = 0.01f;
		val3.transform.localScale = new Vector3(num3 / num * num2, num3, num3);
		val3.layer = LayerMask.NameToLayer("UI");
		_keyboardObjects.Add(val3);
		Canvas val4 = val3.AddComponent<Canvas>();
		val4.renderMode = (RenderMode)2;
		val4.sortingOrder = 10;
		Text val5 = new GameObject("Label").AddComponent<Text>();
		((Component)val5).transform.SetParent(val3.transform, false);
		val5.font = Main.CurrentFont;
		val5.text = key;
		val5.fontSize = 50;
		((Graphic)val5).color = Color.white;
		val5.alignment = (TextAnchor)4;
		val5.resizeTextForBestFit = true;
		val5.resizeTextMinSize = 10;
		val5.resizeTextMaxSize = 50;
		RectTransform component2 = ((Component)val5).GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(200f, 200f);
		((Transform)component2).localPosition = Vector3.zero;
		((Transform)component2).localRotation = Quaternion.identity;
		List<GameObject> list = null;
		if (Settings.OutlineMode >= 2)
		{
			GameObject val6 = CreateKeyboardPrimitive(keyboardObj.transform);
			((UnityEngine.Object)val6).name = "KeyOutline";
			val6.transform.localScale = new Vector3(width - 0.0025f + 0.004f, height - 0.0025f + 0.004f, 0.011f);
			val6.transform.localPosition = new Vector3(position.x, position.y, position.z + 0.0005f);
			Material val7 = CreateOutlineMaterial();
			_keyboardMaterials.Add(val7);
			val6.GetComponent<Renderer>().sharedMaterial = val7;
			List<GameObject> parts = null;
			if (Settings.MenuRoundness > 0f)
			{
				parts = RoundKeyboard(val6, 0.007f * Settings.MenuRoundness);
			}
			RegisterKeyboardColorGroup(Main.ColorRole.Outline, val6, parts);
		}
		if (Settings.MenuRoundness > 0f)
		{
			list = RoundKeyboard(val, 0.007f * Settings.MenuRoundness);
		}
		RegisterKeyboardColorGroup(Main.ColorRole.Button, val, list);
		KeyCollider keyCollider = val.AddComponent<KeyCollider>();
		keyCollider.key = key;
		keyCollider.baseScale = val.transform.localScale;
		if (list != null && list.Count > 0)
		{
			keyCollider.roundedHolder = list[0];
		}
		_keyColliders[key] = keyCollider;
	}

	public unsafe static string KeyCodeToString(KeyCode keyCode)
	{
		KeyCode val = keyCode;
		if ((int)val != 8)
		{
			if ((int)val != 13)
			{
				if ((int)val == 32)
				{
					return "SPACE";
				}
				if ((int)val >= 48 && (int)val <= 57)
				{
					return (val - 48).ToString();
				}
				return ((object)(*(KeyCode*)(&keyCode))).ToString();
			}
			return "ENTER";
		}
		return "BACK";
	}

	public static void ReRegisterKeyboardColorGroups()
	{
		foreach (var (role, val, extraParts) in _keyboardColorGroups)
		{
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				Main.RegisterColorGroup(role, val, extraParts);
			}
		}
	}

	private static List<GameObject> RoundKeyboard(GameObject panel, float roundness)
	{
		List<GameObject> list = new List<GameObject>(1);
		if ((UnityEngine.Object)(object)panel == (UnityEngine.Object)null)
		{
			return list;
		}
		Renderer component = panel.GetComponent<Renderer>();
		if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
		{
			return list;
		}
		Transform val = panel.transform.parent ?? keyboardObj.transform;
		GameObject val2 = new GameObject("rounded_" + ((UnityEngine.Object)panel).name);
		val2.transform.SetParent(val, false);
		val2.transform.localPosition = panel.transform.localPosition;
		val2.transform.localRotation = panel.transform.localRotation;
		val2.transform.localScale = Vector3.one;
		val2.layer = panel.layer;
		Mesh val3 = KeyboardRoundObj(panel.transform.localScale, roundness);
		_keyboardMeshes.Add(val3);
		val2.AddComponent<MeshFilter>().mesh = val3;
		Material val4 = new Material(component.sharedMaterial);
		val4.SetInt("_Cull", 0);
		((Renderer)val2.AddComponent<MeshRenderer>()).sharedMaterial = val4;
		_keyboardMaterials.Add(val4);
		_keyboardObjects.Add(val2);
		component.enabled = false;
		list.Add(val2);
		return list;
	}

	private static Material CreateBackgroundMaterial()
	{
		Material val;
		if (Settings.BackgroundMode == Settings.ColorMode.Pinwheel)
		{
			Material pinwheelMaterial = Main.GetPinwheelMaterial();
			if ((UnityEngine.Object)(object)pinwheelMaterial != (UnityEngine.Object)null)
			{
				_keyboardMaterials.Add(pinwheelMaterial);
				return pinwheelMaterial;
			}
			val = CreateMaterial((Color32)(Settings.BackgroundColor), 2450);
			_keyboardMaterials.Add(val);
			return val;
		}
		val = CreateMaterial((Color32)(Settings.BackgroundColor), 2450);
		_keyboardMaterials.Add(val);
		return val;
	}

	private static Mesh KeyboardRoundObj(Vector3 localScale, float r, int seg = 5)
	{
		float num = localScale.x * 0.5f;
		float num2 = localScale.y * 0.5f;
		float num3 = localScale.z * 0.5f;
		float num4 = Mathf.Clamp(r, 0.0001f, Mathf.Min(num, num2) * 0.9f);
		r = num4;
		float num5 = num - r;
		float num6 = num2 - r;
		int capacity = (seg + 1) * 4;
		List<Vector2> list = new List<Vector2>(capacity);
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
		int num7 = 0;
		if (num7 < 4)
		{
			do
			{
				float num8 = (float)num7 * MathF.PI * 0.5f;
				float num9 = array[num7];
				float num10 = array2[num7];
				int num11 = 0;
				if (num11 <= seg)
				{
					do
					{
						float num12 = Mathf.Lerp(num8, num8 + MathF.PI / 2f, (float)num11 / (float)seg);
						list.Add(new Vector2(num9 + Mathf.Cos(num12) * r, num10 + Mathf.Sin(num12) * r));
						num11++;
					}
					while (num11 <= seg);
				}
				num7++;
			}
			while (num7 < 4);
		}
		int count = list.Count;
		List<Vector3> list2 = new List<Vector3>(count * 4 + 4);
		List<Vector3> list3 = new List<Vector3>(count * 4 + 4);
		List<int> list4 = new List<int>(count * 12);
		int count2 = list2.Count;
		list2.Add(new Vector3(0f, 0f, num3));
		list3.Add(Vector3.forward);
		int count3 = list2.Count;
		int num13 = 0;
		if (num13 < count)
		{
			do
			{
				list2.Add(new Vector3(list[num13].x, list[num13].y, num3));
				list3.Add(Vector3.forward);
				num13++;
			}
			while (num13 < count);
		}
		int num14 = 0;
		if (num14 < count)
		{
			do
			{
				list4.Add(count2);
				list4.Add(count3 + num14);
				list4.Add(count3 + (num14 + 1) % count);
				num14++;
			}
			while (num14 < count);
		}
		int count4 = list2.Count;
		list2.Add(new Vector3(0f, 0f, 0f - num3));
		list3.Add(Vector3.back);
		int count5 = list2.Count;
		int num15 = 0;
		if (num15 < count)
		{
			do
			{
				list2.Add(new Vector3(list[num15].x, list[num15].y, 0f - num3));
				list3.Add(Vector3.back);
				num15++;
			}
			while (num15 < count);
		}
		int num16 = 0;
		if (num16 < count)
		{
			do
			{
				list4.Add(count4);
				list4.Add(count5 + (num16 + 1) % count);
				list4.Add(count5 + num16);
				num16++;
			}
			while (num16 < count);
		}
		int count6 = list2.Count;
		int num17 = 0;
		if (num17 < count)
		{
			do
			{
				Vector3 val = new Vector3(list[num17].x, list[num17].y, 0f);
				Vector3 normalized = val.normalized;
				list2.Add(new Vector3(list[num17].x, list[num17].y, num3));
				list3.Add(normalized);
				list2.Add(new Vector3(list[num17].x, list[num17].y, 0f - num3));
				list3.Add(normalized);
				num17++;
			}
			while (num17 < count);
		}
		int num18 = 0;
		if (num18 < count)
		{
			do
			{
				int num19 = (num18 + 1) % count;
				int num20 = count6 + num18 * 2;
				int item = num20 + 1;
				int num21 = count6 + num19 * 2;
				int item2 = num21 + 1;
				list4.Add(num20);
				list4.Add(item);
				list4.Add(num21);
				list4.Add(num21);
				list4.Add(item);
				list4.Add(item2);
				num18++;
			}
			while (num18 < count);
		}
		int count7 = list4.Count;
		int num22 = 0;
		if (num22 < count7)
		{
			do
			{
				list4.Add(list4[num22]);
				list4.Add(list4[num22 + 2]);
				list4.Add(list4[num22 + 1]);
				num22 += 3;
			}
			while (num22 < count7);
		}
		Mesh val2 = new Mesh
		{
			name = "KeyboardRoundedMesh"
		};
		val2.SetVertices(list2);
		val2.SetNormals(list3);
		val2.SetTriangles(list4, 0);
		val2.RecalculateBounds();
		return val2;
	}

	public static void CloseTypingKeyboard(bool cancelled = false)
	{
		string obj = inputText.Trim();
		isSearching = false;
		isTyping = false;
		shouldShowSearchText = false;
		Main._lastKeyboardYRotation = float.MinValue;
		Main._lastKeyboardSnapPosition = Vector3.zero;
		if (_preSarchMenuScale > 0f)
		{
			Main.menuScale = _preSarchMenuScale;
			_preSarchMenuScale = -1f;
		}
		Action<string> action = onTypingComplete;
		Action action2 = onTypingCancelled;
		onTypingComplete = null;
		onTypingCancelled = null;
		CleanupKeyboard();
		Main.RefreshMenu();
		if (cancelled)
		{
			action2?.Invoke();
		}
		else
		{
			action?.Invoke(obj);
		}
	}

	public static void HandleKeyPress(string key)
	{
		if ((UnityEngine.Object)(object)searchText == (UnityEngine.Object)null)
		{
			return;
		}
		if (_keyColliders.TryGetValue(key, out KeyCollider value) && (UnityEngine.Object)(object)value != (UnityEngine.Object)null)
		{
			value.TriggerBounce();
		}
		if (key == "SPACE")
		{
			inputText += " ";
		}
		else if (key == "BACK")
		{
			if (inputText.Length > 0)
			{
				string text = inputText;
				inputText = text.Substring(0, text.Length - 1);
			}
		}
		else if (key == "ENTER")
		{
			if (isTyping)
			{
				CloseTypingKeyboard();
			}
			else
			{
				CloseKeyboard();
			}
			return;
		}
		else
		{
			inputText += key;
		}
		Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(66, true, 0.625f);
		Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(66, false, 0.625f);
		searchText.text = inputText;
		Variables.currentCategoryPage = 0;
		if (isSearching && !isTyping)
		{
			Main.RedrawButtonList();
		}
	}

	private static Material CreateMaterial(Color color, int renderQueue)
	{
		return Main.CreateColorMaterial(color, renderQueue);
	}

	public static void OpenTypingKeyboard(string prefill = "", string placeholder = "Type here...")
	{
		onTypingComplete = null;
		onTypingCancelled = null;
		isSearching = true;
		isTyping = true;
		shouldShowSearchText = true;
		inputText = prefill;
		placeholderText = placeholder;
		if (Variables.InMenuCondition && Main.menuScale != 0.75f)
		{
			_preSarchMenuScale = Main.menuScale;
			Main.menuScale = 0.75f;
		}
		if (!Variables.InPcCondition)
		{
			DrawKeyboard();
			KeyClicker1(Variables.playerInstance.RightHand.controllerTransform);
			KeyClicker2(Variables.playerInstance.LeftHand.controllerTransform);
		}
		Main.RefreshMenu();
	}

	private static void RegisterKeyboardColorGroup(Main.ColorRole role, GameObject obj, List<GameObject> parts)
	{
		_keyboardColorGroups.Add((role, obj, parts));
		Main.RegisterColorGroup(role, obj, parts);
	}

	public static void UpdateSearchBlink()
	{
		if (!shouldShowSearchText || (UnityEngine.Object)(object)searchText == (UnityEngine.Object)null)
		{
			return;
		}
		_blinkTimer += Time.deltaTime;
		if (!(_blinkTimer < 0.5f))
		{
			_blinkTimer = 0f;
			_cursorVisible = !_cursorVisible;
			if (!string.IsNullOrEmpty(inputText))
			{
				string text = inputText;
				searchText.text = text + (_cursorVisible ? "|" : "");
			}
			else
			{
				string text = placeholderText;
				searchText.text = text + (_cursorVisible ? "|" : "");
			}
		}
	}

	public static void ToggleKeyboard()
	{
		if (isSearching)
		{
			CloseKeyboard();
		}
		else
		{
			OpenKeyboard(typingMode: false);
		}
	}

	public static void OpenKeyboard(bool typingMode, string prefill = "", string placeholder = "Type to search...")
	{
		isSearching = true;
		isTyping = typingMode;
		shouldShowSearchText = true;
		inputText = prefill;
		placeholderText = placeholder;
		if (Variables.InMenuCondition && Main.menuScale != 0.75f)
		{
			_preSarchMenuScale = Main.menuScale;
			Main.menuScale = 0.75f;
		}
		if (!Variables.InPcCondition)
		{
			DrawKeyboard();
			KeyClicker1(Variables.playerInstance.RightHand.controllerTransform);
			KeyClicker2(Variables.playerInstance.LeftHand.controllerTransform);
		}
		Main.RefreshMenu();
	}
}
