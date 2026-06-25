using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace NXO.Utilities;

public static class ControllerEmulator
{
	public static bool EmulatorEnabled = false;

	private static bool _virtualRTrigger = false;

	private static bool _virtualLTrigger = false;

	private static bool _virtualRGrip = false;

	private static bool _virtualLGrip = false;

	private static bool _virtualRPrimary = false;

	private static bool _virtualLPrimary = false;

	private static bool _virtualRSecondary = false;

	private static bool _virtualLSecondary = false;

	private static Texture2D _hudBackgroundTexture;

	private static GUIStyle _backgroundStyle;

	private static GUIStyle _headerStyle;

	private static GUIStyle _tableHeaderStyle;

	private static GUIStyle _rowStyle;

	private static GUIStyle _activeStyle;

	private static GUIStyle _keyCenterStyle;

	private static bool _stylesInitialized = false;

	private const float HUD_WIDTH = 280f;

	private const float HUD_HEIGHT = 210f;

	private const float ROW_HEIGHT = 20f;

	private static readonly string[] ControllerNames = new string[8] { "Right Trigger", "Left Trigger", "Right Bumper", "Left Bumper", "A Button", "B Button", "X Button", "Y Button" };

	private static readonly string[] KeyNames = new string[8] { "T", "R", "G", "F", "Y", "U", "E", "Q" };

	public static bool GetRGrip()
	{
		return _virtualRGrip;
	}

	private static void InitializeStyles()
	{
		_hudBackgroundTexture = CreateOptimizedHUDTexture();
		GUIStyle val = new GUIStyle();
		val.normal.background = _hudBackgroundTexture;
		_backgroundStyle = val;
		GUIStyle val2 = new GUIStyle
		{
			fontSize = 18,
			fontStyle = (FontStyle)1
		};
		val2.normal.textColor = Color.white;
		val2.alignment = (TextAnchor)4;
		_headerStyle = val2;
		GUIStyle val3 = new GUIStyle
		{
			fontSize = 14,
			fontStyle = (FontStyle)1
		};
		val3.normal.textColor = Color.white;
		val3.alignment = (TextAnchor)3;
		_tableHeaderStyle = val3;
		GUIStyle val4 = new GUIStyle
		{
			fontSize = 12
		};
		val4.normal.textColor = (Color32)(new Color32((byte)180, (byte)180, (byte)180, byte.MaxValue));
		val4.alignment = (TextAnchor)3;
		_rowStyle = val4;
		GUIStyle val5 = new GUIStyle
		{
			fontSize = 12,
			fontStyle = (FontStyle)1
		};
		val5.normal.textColor = Color.white;
		val5.alignment = (TextAnchor)3;
		_activeStyle = val5;
		GUIStyle val6 = new GUIStyle
		{
			fontSize = 12
		};
		val6.normal.textColor = (Color32)(new Color32((byte)180, (byte)180, (byte)180, byte.MaxValue));
		val6.alignment = (TextAnchor)4;
		_keyCenterStyle = val6;
		_stylesInitialized = true;
	}

	private static Texture2D CreateOptimizedHUDTexture()
	{
		int num = 280;
		int num2 = 210;
		int num3 = 12;
		Texture2D val = new Texture2D(num, num2, (TextureFormat)4, false)
		{
			filterMode = (FilterMode)1
		};
		Color32 val2 = default(Color32);
		val2 = new Color32((byte)0, (byte)0, (byte)0, (byte)180);
		Color32 val3 = default(Color32);
		val3 = new Color32((byte)0, (byte)0, (byte)0, (byte)0);
		Color32[] array = (Color32[])(object)new Color32[num * num2];
		for (int num4 = 0; num4 < num2; num4++)
		{
			for (int num5 = 0; num5 < num; num5++)
			{
				int num6 = num4 * num + num5;
				int num7 = num5;
				int num8 = num4;
				bool roundedCorner = false;
				if (num5 < num3 && num4 >= num2 - num3)
				{
					num7 = num3;
					num8 = num2 - num3;
					roundedCorner = true;
				}
				else if (num5 >= num - num3 && num4 >= num2 - num3)
				{
					num7 = num - num3 - 1;
					num8 = num2 - num3;
					roundedCorner = true;
				}
				else if (num5 < num3 && num4 < num3)
				{
					num7 = num3;
					num8 = num3;
					roundedCorner = true;
				}
				else if (num5 >= num - num3 && num4 < num3)
				{
					num7 = num - num3 - 1;
					num8 = num3;
					roundedCorner = true;
				}
				if (roundedCorner)
				{
					float num9 = Mathf.Sqrt((float)((num5 - num7) * (num5 - num7) + (num4 - num8) * (num4 - num8)));
					array[num6] = ((num9 > (float)num3) ? val3 : val2);
				}
				else
				{
					array[num6] = val2;
				}
			}
		}
		val.SetPixels32(array);
		val.Apply(false);
		return val;
	}

	public static bool GetRSecondary()
	{
		return _virtualRSecondary;
	}

	public static bool GetLPrimary()
	{
		return _virtualLPrimary;
	}

	public static bool GetRTrigger()
	{
		return _virtualRTrigger;
	}

	public static bool GetLSecondary()
	{
		return _virtualLSecondary;
	}

	public static bool GetLGrip()
	{
		return _virtualLGrip;
	}

	public static bool GetRPrimary()
	{
		return _virtualRPrimary;
	}

	private static void DrawControllerRow(float x, float y, string controller, string key, bool isActive)
	{
		GUI.Label(new Rect(x, y, 140f, 19f), controller, isActive ? _activeStyle : _rowStyle);
		GUI.Label(new Rect(x + 117.5f, y, 40f, 19f), key, _keyCenterStyle);
		GUI.Label(new Rect(x + 190f, y, 60f, 19f), isActive ? "ON" : "OFF", isActive ? _activeStyle : _rowStyle);
	}

	public static void Update()
	{
		if (EmulatorEnabled && Keyboard.current != null)
		{
			_virtualRTrigger = ((ButtonControl)Keyboard.current[(Key)34]).isPressed;
			_virtualLTrigger = ((ButtonControl)Keyboard.current[(Key)32]).isPressed;
			_virtualRGrip = ((ButtonControl)Keyboard.current[(Key)21]).isPressed;
			_virtualLGrip = ((ButtonControl)Keyboard.current[(Key)20]).isPressed;
			_virtualRPrimary = ((ButtonControl)Keyboard.current[(Key)39]).isPressed;
			_virtualRSecondary = ((ButtonControl)Keyboard.current[(Key)35]).isPressed;
			_virtualLPrimary = ((ButtonControl)Keyboard.current[(Key)19]).isPressed;
			_virtualLSecondary = ((ButtonControl)Keyboard.current[(Key)31]).isPressed;
		}
	}

	public static void Cleanup()
	{
		if ((UnityEngine.Object)(object)_hudBackgroundTexture != (UnityEngine.Object)null)
		{
			UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)_hudBackgroundTexture);
			_hudBackgroundTexture = null;
			_backgroundStyle = null;
			_headerStyle = null;
			_tableHeaderStyle = null;
			_rowStyle = null;
			_activeStyle = null;
			_keyCenterStyle = null;
			_stylesInitialized = false;
		}
		else
		{
			_backgroundStyle = null;
			_headerStyle = null;
			_tableHeaderStyle = null;
			_rowStyle = null;
			_activeStyle = null;
			_keyCenterStyle = null;
			_stylesInitialized = false;
		}
	}

	public static bool GetLTrigger()
	{
		return _virtualLTrigger;
	}

	public static void ShowControllerHUD()
	{
		if (!EmulatorEnabled)
		{
			return;
		}
		Rect val = default(Rect);
		bool[] array;
		int num3;
		float num2;
		if (!_stylesInitialized)
		{
			InitializeStyles();
			val = new Rect(15f, (float)Screen.height - 210f - 15f, 280f, 210f);
			GUI.Box(val, "", _backgroundStyle);
			float num = val.y + 15f;
			num2 = num;
			GUI.Label(new Rect(val.x + 20f, num2, 140f, 20f), "Controller", _tableHeaderStyle);
			GUI.Label(new Rect(val.x + 145f, num2, 40f, 20f), "Key", _tableHeaderStyle);
			GUI.Label(new Rect(val.x + 210f, num2, 60f, 20f), "Status", _tableHeaderStyle);
			num = num2 + 22f;
			num2 = num;
			GUI.color = new Color(1f, 1f, 1f, 0.1f);
			GUI.DrawTexture(new Rect(val.x + 20f, num2, val.width - 40f, 1f), (Texture)(object)Texture2D.whiteTexture);
			GUI.color = Color.white;
			num = num2 + 6f;
			num2 = num;
			array = new bool[8] { _virtualRTrigger, _virtualLTrigger, _virtualRGrip, _virtualLGrip, _virtualRPrimary, _virtualRSecondary, _virtualLPrimary, _virtualLSecondary };
			num3 = 0;
		}
		else
		{
			val = new Rect(15f, (float)Screen.height - 210f - 15f, 280f, 210f);
			GUI.Box(val, "", _backgroundStyle);
			float num4 = val.y + 15f;
			num2 = num4;
			GUI.Label(new Rect(val.x + 20f, num2, 140f, 20f), "Controller", _tableHeaderStyle);
			GUI.Label(new Rect(val.x + 145f, num2, 40f, 20f), "Key", _tableHeaderStyle);
			GUI.Label(new Rect(val.x + 210f, num2, 60f, 20f), "Status", _tableHeaderStyle);
			num4 = num2 + 22f;
			num2 = num4;
			GUI.color = new Color(1f, 1f, 1f, 0.1f);
			GUI.DrawTexture(new Rect(val.x + 20f, num2, val.width - 40f, 1f), (Texture)(object)Texture2D.whiteTexture);
			GUI.color = Color.white;
			num4 = num2 + 6f;
			num2 = num4;
			array = new bool[8] { _virtualRTrigger, _virtualLTrigger, _virtualRGrip, _virtualLGrip, _virtualRPrimary, _virtualRSecondary, _virtualLPrimary, _virtualLSecondary };
			num3 = 0;
		}
		if (num3 < 8)
		{
			do
			{
				DrawControllerRow(val.x + 20f, num2, ControllerNames[num3], KeyNames[num3], array[num3]);
				num2 += 20f;
				num3++;
			}
			while (num3 < 8);
		}
	}
}
