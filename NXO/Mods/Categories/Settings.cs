using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NXO.Menu;
using UnityEngine;

namespace NXO.Mods.Categories;

public class Settings
{
	public enum ColorMode
	{
		Solid,
		Lerp,
		Rainbow,
		Strobe,
		Pinwheel
	}

	public enum ColorElement
	{
		Pinwheel,
		Outline,
		Background,
		Button,
		Title,
		AccentStrip,
		Boards
	}

	public enum AccentStripType
	{
		Off,
		Top,
		Both
	}

	public delegate bool ControllerInput();

	public static readonly string[] ColorModeNames = new string[5] { "Solid", "Lerp", "Rainbow", "Strobe", "Pinwheel" };

	public static ColorMode BackgroundMode = ColorMode.Solid;

	public static ColorMode ButtonMode = ColorMode.Solid;

	public static ColorMode EnabledButtonMode = ColorMode.Solid;

	public static ColorMode TitleMode = ColorMode.Pinwheel;

	public static ColorMode OutlineColorMode = ColorMode.Pinwheel;

	public static ColorMode AccentStripMode = ColorMode.Solid;

	public static ButtonHandler.Button backgroundModeButton;

	public static ButtonHandler.Button buttonModeButton;

	public static ButtonHandler.Button enabledModeButton;

	public static ButtonHandler.Button titleModeButton;

	public static ButtonHandler.Button outlineColorModeButton;

	public static ColorElement _currentColorElement;

	private static readonly Color32[] CustomColors = (Color32[])(object)new Color32[90]
	{
		new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32((byte)230, (byte)230, (byte)230, byte.MaxValue),
		new Color32((byte)180, (byte)180, (byte)180, byte.MaxValue),
		new Color32((byte)128, (byte)128, (byte)128, byte.MaxValue),
		new Color32((byte)80, (byte)80, (byte)80, byte.MaxValue),
		new Color32((byte)40, (byte)40, (byte)40, byte.MaxValue),
		new Color32((byte)15, (byte)15, (byte)15, byte.MaxValue),
		new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue),
		new Color32((byte)220, (byte)20, (byte)60, byte.MaxValue),
		new Color32((byte)178, (byte)34, (byte)34, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)99, (byte)99, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)105, (byte)180, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)20, (byte)147, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)182, (byte)193, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)192, (byte)203, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)0, (byte)127, byte.MaxValue),
		new Color32((byte)231, (byte)84, (byte)128, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)127, (byte)80, byte.MaxValue),
		new Color32((byte)250, (byte)128, (byte)114, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)140, (byte)0, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)69, (byte)0, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)215, (byte)80, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)99, (byte)71, byte.MaxValue),
		new Color32((byte)210, (byte)105, (byte)30, byte.MaxValue),
		new Color32((byte)139, (byte)69, (byte)19, byte.MaxValue),
		new Color32((byte)160, (byte)82, (byte)45, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, (byte)0, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)220, (byte)80, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)215, (byte)0, byte.MaxValue),
		new Color32((byte)240, (byte)230, (byte)140, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)250, (byte)205, byte.MaxValue),
		new Color32((byte)0, byte.MaxValue, (byte)0, byte.MaxValue),
		new Color32((byte)50, (byte)205, (byte)50, byte.MaxValue),
		new Color32((byte)34, (byte)139, (byte)34, byte.MaxValue),
		new Color32((byte)0, (byte)128, (byte)0, byte.MaxValue),
		new Color32((byte)124, (byte)252, (byte)0, byte.MaxValue),
		new Color32((byte)173, byte.MaxValue, (byte)47, byte.MaxValue),
		new Color32((byte)152, (byte)251, (byte)152, byte.MaxValue),
		new Color32((byte)60, (byte)179, (byte)113, byte.MaxValue),
		new Color32((byte)46, (byte)204, (byte)113, byte.MaxValue),
		new Color32((byte)85, (byte)107, (byte)47, byte.MaxValue),
		new Color32((byte)0, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32((byte)64, (byte)224, (byte)208, byte.MaxValue),
		new Color32((byte)72, (byte)209, (byte)204, byte.MaxValue),
		new Color32((byte)0, (byte)206, (byte)209, byte.MaxValue),
		new Color32((byte)127, byte.MaxValue, (byte)212, byte.MaxValue),
		new Color32((byte)0, (byte)128, (byte)128, byte.MaxValue),
		new Color32((byte)176, (byte)224, (byte)230, byte.MaxValue),
		new Color32((byte)0, (byte)0, byte.MaxValue, byte.MaxValue),
		new Color32((byte)30, (byte)144, byte.MaxValue, byte.MaxValue),
		new Color32((byte)0, (byte)191, byte.MaxValue, byte.MaxValue),
		new Color32((byte)135, (byte)206, (byte)235, byte.MaxValue),
		new Color32((byte)70, (byte)130, (byte)180, byte.MaxValue),
		new Color32((byte)100, (byte)149, (byte)237, byte.MaxValue),
		new Color32((byte)25, (byte)25, (byte)112, byte.MaxValue),
		new Color32((byte)65, (byte)105, (byte)225, byte.MaxValue),
		new Color32((byte)180, (byte)220, byte.MaxValue, byte.MaxValue),
		new Color32((byte)128, (byte)0, (byte)128, byte.MaxValue),
		new Color32((byte)186, (byte)85, (byte)211, byte.MaxValue),
		new Color32((byte)218, (byte)112, (byte)214, byte.MaxValue),
		new Color32((byte)221, (byte)160, (byte)221, byte.MaxValue),
		new Color32((byte)147, (byte)112, (byte)219, byte.MaxValue),
		new Color32((byte)138, (byte)43, (byte)226, byte.MaxValue),
		new Color32((byte)75, (byte)0, (byte)130, byte.MaxValue),
		new Color32((byte)139, (byte)0, (byte)139, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)0, byte.MaxValue, byte.MaxValue),
		new Color32((byte)199, (byte)21, (byte)133, byte.MaxValue),
		new Color32((byte)72, (byte)61, (byte)139, byte.MaxValue),
		new Color32((byte)57, byte.MaxValue, (byte)20, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)0, (byte)234, byte.MaxValue),
		new Color32((byte)13, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)244, (byte)0, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)92, (byte)0, byte.MaxValue),
		new Color32((byte)191, (byte)0, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)20, (byte)100, byte.MaxValue),
		new Color32((byte)0, (byte)100, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)209, (byte)220, byte.MaxValue),
		new Color32((byte)200, (byte)245, (byte)220, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)253, (byte)184, byte.MaxValue),
		new Color32((byte)196, (byte)197, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)228, (byte)196, byte.MaxValue),
		new Color32((byte)200, (byte)235, (byte)245, byte.MaxValue),
		new Color32((byte)245, (byte)240, (byte)230, byte.MaxValue),
		new Color32((byte)192, (byte)192, (byte)192, byte.MaxValue),
		new Color32((byte)212, (byte)175, (byte)55, byte.MaxValue),
		new Color32((byte)176, (byte)141, (byte)87, byte.MaxValue),
		new Color32((byte)184, (byte)115, (byte)51, byte.MaxValue),
		new Color32((byte)229, (byte)228, (byte)226, byte.MaxValue)
	};

	private static readonly string[] CustomColorNames = new string[90]
	{
		"White", "Off-White", "Light Gray", "Gray", "Medium Gray", "Dark Gray", "Near Black", "Black", "Red", "Crimson",
		"Firebrick", "Salmon", "Hot Pink", "Deep Pink", "Light Pink", "Pink", "Bubblegum", "Rose", "Coral", "Salmon Pink",
		"Orange", "Dark Orange", "Orange Red", "Tangerine", "Tomato", "Chocolate", "Saddle Brown", "Sienna", "Yellow", "Sunny Yellow",
		"Gold", "Khaki", "Cream", "Green", "Lime Green", "Forest Green", "Dark Green", "Lawn Green", "Green Yellow", "Pale Green",
		"Medium Sea Green", "Emerald", "Dark Olive", "Cyan", "Turquoise", "Medium Turquoise", "Dark Turquoise", "Aquamarine", "Teal", "Powder Blue",
		"Blue", "Dodger Blue", "Deep Sky Blue", "Sky Blue", "Steel Blue", "Cornflower", "Midnight Blue", "Royal Blue", "Pastel Blue", "Purple",
		"Medium Orchid", "Orchid", "Plum", "Medium Purple", "Blue Violet", "Indigo", "Dark Magenta", "Magenta", "Medium Violet Red", "Dark Slate Blue",
		"Neon Green", "Neon Pink", "Neon Cyan", "Neon Yellow", "Neon Orange", "Neon Purple", "Neon Red", "Neon Blue", "Pastel Pink", "Pastel Mint",
		"Pastel Yellow", "Pastel Lavender", "Pastel Peach", "Pastel Sky", "Pastel Cream", "Silver", "Antique Gold", "Bronze", "Copper", "Platinum"
	};

	public static ButtonHandler.Button pinwheelColor1Button;

	private static int pinwheelColor1Index = 77;

	public static Color32 PinwheelColor1 = new Color32((byte)0, (byte)100, byte.MaxValue, byte.MaxValue);

	public static string PinwheelColor1Name = "Neon Blue";

	public static ButtonHandler.Button pinwheelColor2Button;

	private static int pinwheelColor2Index = 1;

	public static Color32 PinwheelColor2 = new Color32((byte)230, (byte)230, (byte)230, byte.MaxValue);

	public static string PinwheelColor2Name = "Off-White";

	public static ButtonHandler.Button pinwheelSpeedButton;

	private static readonly (float speed, string desc)[] PinwheelSpeedOptions = new(float, string)[5]
	{
		(0.25f, "Slow"),
		(0.5f, "Normal"),
		(1f, "Fast"),
		(2f, "Very Fast"),
		(4f, "Strobe")
	};

	private static int pinwheelSpeedIndex = 1;

	public static string PinwheelSpeedDescription = "Normal";

	public static ButtonHandler.Button adjustAccentStripAnimSpeedButton;

	private static int accentStripAnimSpeedIndex = 2;

	public static float AccentStripAnimSpeed = 1f;

	public static string AccentStripAnimSpeedDescription = "Normal";

	public static ButtonHandler.Button accentStripModeButton;

	public static ButtonHandler.Button adjustAccentStripColorButton;

	public static ButtonHandler.Button adjustAccentStripColor2Button;

	private static int accentStripColorIndex = 6;

	private static int accentStripColor2Index = 1;

	public static Color32 AccentStripColor = new Color32((byte)15, (byte)15, (byte)15, byte.MaxValue);

	public static Color32 AccentStripColor2 = new Color32((byte)230, (byte)230, (byte)230, byte.MaxValue);

	public static string AccentStripColorDescription = "Near Black";

	public static string AccentStripColor2Description = "Off-White";

	private static readonly (float speed, string desc)[] AnimationSpeedOptions = new(float, string)[5]
	{
		(0.25f, "Very Slow"),
		(0.5f, "Slow"),
		(1f, "Normal"),
		(2f, "Fast"),
		(4f, "Very Fast")
	};

	public static ButtonHandler.Button adjustOutlineAnimSpeedButton;

	private static int outlineAnimSpeedIndex = 2;

	public static float OutlineAnimSpeed = 1f;

	public static string OutlineAnimSpeedDescription = "Normal";

	public static ButtonHandler.Button adjustBackgroundAnimSpeedButton;

	private static int backgroundAnimSpeedIndex = 2;

	public static float BackgroundAnimSpeed = 1f;

	public static string BackgroundAnimSpeedDescription = "Normal";

	public static ButtonHandler.Button adjustButtonAnimSpeedButton;

	private static int buttonAnimSpeedIndex = 2;

	public static float ButtonAnimSpeed = 1f;

	public static string ButtonAnimSpeedDescription = "Normal";

	public static ButtonHandler.Button adjustEnabledButtonAnimSpeedButton;

	private static int enabledButtonAnimSpeedIndex = 2;

	public static float EnabledButtonAnimSpeed = 1f;

	public static string EnabledButtonAnimSpeedDescription = "Normal";

	public static ButtonHandler.Button adjustTitleAnimSpeedButton;

	private static int titleAnimSpeedIndex = 2;

	public static float TitleAnimSpeed = 1f;

	public static string TitleAnimSpeedDescription = "Normal";

	public static ButtonHandler.Button adjustOutlineColorButton;

	public static ButtonHandler.Button adjustOutlineColor2Button;

	private static int outlineColorIndex = 7;

	private static int outlineColor2Index = 1;

	public static Color32 OutlineColor = new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue);

	public static Color32 OutlineColor2 = new Color32((byte)230, (byte)230, (byte)230, byte.MaxValue);

	public static string OutlineColorDescription = "Black";

	public static string OutlineColor2Description = "Off-White";

	public static int OutlineMode = 1;

	public static ButtonHandler.Button outlineModeButton;

	public static readonly string[] OutlineModes = new string[3] { "None", "Minimal", "Everything" };

	public static ButtonHandler.Button adjustMenuSizeButton;

	private static readonly (float scale, string desc)[] MenuSizeOptions = new(float, string)[5]
	{
		(0.45f, "Tiny"),
		(0.6f, "Small"),
		(0.75f, "Medium"),
		(0.9f, "Large"),
		(1.1f, "Massive")
	};

	private static int menuSizeIndex = 1;

	public static string MenuSizeDescription = "Small";

	public static ButtonHandler.Button adjustRoundnessButton;

	private static readonly (float value, string desc)[] RoundnessOptions = new(float, string)[5]
	{
		(0f, "None"),
		(0.3f, "Low"),
		(0.5f, "Medium"),
		(0.75f, "High"),
		(1f, "Roundest")
	};

	private static int roundnessIndex = 3;

	public static float MenuRoundness = 0.75f;

	public static string RoundnessDescription = "High";

	public static ButtonHandler.Button adjustBoardsAnimSpeedButton;

	private static int boardsAnimSpeedIndex = 2;

	public static float BoardsAnimSpeed = 1f;

	public static string BoardsAnimSpeedDescription = "Normal";

	public static ColorMode BoardsMode = ColorMode.Solid;

	public static ButtonHandler.Button boardsModeButton;

	public static ButtonHandler.Button adjustBoardsColorButton;

	public static ButtonHandler.Button adjustBoardsColor2Button;

	private static int boardsColorIndex = 77;

	private static int boardsColor2Index = 1;

	public static Color32 BoardsColor = new Color32((byte)0, (byte)100, byte.MaxValue, byte.MaxValue);

	public static Color32 BoardsColor2 = new Color32((byte)230, (byte)230, (byte)230, byte.MaxValue);

	public static string BoardsColorDescription = "Neon Blue";

	public static string BoardsColor2Description = "Off-White";

	public static ButtonHandler.Button adjustAccentStripTypeButton;

	public static AccentStripType CurrentAccentStripType = AccentStripType.Both;

	private static readonly string[] AccentStripTypeNames = new string[3] { "Off", "Top", "Both" };

	private static int accentStripTypeIndex = 2;

	public static string AccentStripTypeDescription = "Both";

	public static ButtonHandler.Button adjustBackgroundColorButton;

	public static ButtonHandler.Button adjustBackgroundColor2Button;

	private static int backgroundColorIndex = 7;

	private static int backgroundColor2Index = 5;

	public static Color32 BackgroundColor = new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue);

	public static Color32 BackgroundColor2 = new Color32((byte)40, (byte)40, (byte)40, byte.MaxValue);

	public static string BackgroundColorDescription = "Black";

	public static string BackgroundColor2Description = "Dark Gray";

	public static ButtonHandler.Button adjustButtonColorButton;

	public static ButtonHandler.Button adjustButtonColor2Button;

	private static int buttonColorIndex = 6;

	private static int buttonColor2Index = 4;

	public static Color32 ButtonColor = new Color32((byte)15, (byte)15, (byte)15, byte.MaxValue);

	public static Color32 ButtonColor2 = new Color32((byte)80, (byte)80, (byte)80, byte.MaxValue);

	public static string ButtonColorDescription = "Near Black";

	public static string ButtonColor2Description = "Medium Gray";

	public static ButtonHandler.Button adjustEnabledButtonColorButton;

	public static ButtonHandler.Button adjustEnabledButtonColor2Button;

	private static int enabledButtonColorIndex = 4;

	private static int enabledButtonColor2Index = 3;

	public static Color32 EnabledButtonColor = new Color32((byte)80, (byte)80, (byte)80, byte.MaxValue);

	public static Color32 EnabledButtonColor2 = new Color32((byte)128, (byte)128, (byte)128, byte.MaxValue);

	public static string EnabledButtonColorDescription = "Medium Gray";

	public static string EnabledButtonColor2Description = "Gray";

	public static ButtonHandler.Button titleColorButton;

	public static ButtonHandler.Button titleColor2Button;

	private static int titleColorIndex = 0;

	private static int titleColor2Index = 2;

	public static Color32 TitleColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color32 TitleColor2 = new Color32((byte)180, (byte)180, (byte)180, byte.MaxValue);

	public static string TitleColorDescription = "White";

	public static string TitleColor2Description = "Light Gray";

	public static ButtonHandler.Button tracerPositionButton;

	private static readonly string[] TracerPositions = new string[4] { "Right Hand", "Left Hand", "Head", "Button" };

	public static int tracerPositionIndex = 0;

	public static string TracerPosition = TracerPositions[0];

	public static ButtonHandler.Button adjustSpeedBoostButton;

	private static readonly (float speed, string desc)[] SpeedOptions = new(float, string)[6]
	{
		(10f, "Medium"),
		(14f, "Fast"),
		(18f, "Very Fast"),
		(7.5f, "Mosa"),
		(7f, "Comp"),
		(6f, "Slow")
	};

	private static int currentSpeedIndex = 0;

	public static float SpeedboostSpeed = 10f;

	public static float SpeedboostMultiplier = 2f;

	public static string SpeedDescription = "Medium";

	public static ButtonHandler.Button adjustFlySpeedButton;

	private static readonly (float speed, string desc)[] FlySpeedOptions = new(float, string)[5]
	{
		(12f, "Medium"),
		(16f, "Fast"),
		(20f, "Very Fast"),
		(4f, "Very Slow"),
		(8f, "Slow")
	};

	private static int flySpeedIndex = 0;

	public static float FlySpeed = 12f;

	public static string FlySpeedDescription = "Medium";

	public static ButtonHandler.Button adjustFOVButton;

	private static readonly (float value, string desc)[] FOVOptions = new(float, string)[5]
	{
		(100f, "100"),
		(110f, "110"),
		(120f, "120"),
		(80f, "80"),
		(90f, "90")
	};

	private static int FOVIndex = 0;

	public static float FOV = 100f;

	public static string FOVDescription = "100";

	public static ButtonHandler.Button adjustWalkWalkStrengthButton;

	private static readonly (float strength, string desc)[] WallWalkOptions = new(float, string)[3]
	{
		(-7.5f, "Medium"),
		(-12.5f, "Strong"),
		(-3f, "Weak")
	};

	private static int WallWalkStrengthIndex = 0;

	public static float WallWalkStrength = -7.5f;

	public static string WalkWalkStrengthDescription = "Medium";

	public static ButtonHandler.Button adjustTimeOfDayButton;

	private static int timeOfDayIndex = 0;

	private static readonly string[] TimeOfDayNames = new string[10] { "Dawn", "Morning", "Late Morning", "Midday", "Afternoon", "Late Afternoon", "Dusk", "Evening", "Night", "Late Night" };

	public static string TimeOfDayDescription = "Dawn";

	public static ButtonHandler.Button adjustArmLengthButton;

	private static readonly (Vector3 length, string desc)[] ArmLengthOptions = new(Vector3, string)[3]
	{
		(new Vector3(1.2f, 1.2f, 1.2f), "Slightly Long"),
		(new Vector3(1.3f, 1.3f, 1.3f), "Long"),
		(new Vector3(1.5f, 1.5f, 1.5f), "Very Long")
	};

	private static int ArmLengthIndex = 0;

	public static Vector3 ArmLength = new Vector3(1.2f, 1.2f, 1.2f);

	public static string ArmLengthDescription = "Slightly Long";

	public static ButtonHandler.Button cyclePlatformTypeButton;

	private static readonly string[] PlatformTypes = new string[4] { "Normal", "Sticky", "Invisible", "Invisible Sticky" };

	private static int PlatformTypeIndex = 0;

	public static string PlatformType = "Normal";

	public static int currentIndex = 0;

	public static ControllerInput currentControllerInput = InputHandler.None;

	public static string inputName = "None";

	public static ControllerInput[] controllerInputs = new ControllerInput[9]
	{
		InputHandler.None,
		InputHandler.RPrimary,
		InputHandler.LPrimary,
		InputHandler.RSecondary,
		InputHandler.LSecondary,
		InputHandler.RTrigger,
		InputHandler.LTrigger,
		InputHandler.RGrip,
		InputHandler.LGrip
	};

	public static ButtonHandler.Button cycleControllerBindButton;

	public static ButtonHandler.Button adjustAntiReportRadiusButton;

	private static readonly (float radius, string desc)[] AntiReportRadiusOptions = new(float, string)[4]
	{
		(0.55f, "Default"),
		(0.65f, "Large"),
		(0.75f, "Very Large"),
		(0.45f, "Small")
	};

	private static int antiReportRadiusIndex = 0;

	public static float AntiReportRadius = 0.55f;

	public static string AntiReportRadiusDescription = "Default";

	public static ButtonHandler.Button adjustProjectileSpeedButton;

	public static ButtonHandler.Button adjustProjectileSizeButton;

	private static readonly (float speed, string desc)[] ProjectileSpeedOptions = new(float, string)[5]
	{
		(10f, "Slow"),
		(25f, "Medium"),
		(50f, "Fast"),
		(75f, "Very Fast"),
		(150f, "Extreme")
	};

	private static readonly (int scale, string desc)[] ProjectileSizeOptions = new(int, string)[3]
	{
		(0, "Regular"),
		(3, "Large"),
		(5, "Max")
	};

	private static int ProjectileSpeedIndex = 1;

	private static int ProjectileSizeIndex = 2;

	public static float ProjectileSpeed = 25f;

	public static int snowballScale = 5;

	public static string ProjectileSpeedDescription = "Medium";

	public static string ProjectileSizeDescription = "Max";

	public static ButtonHandler.Button CycleMenuFontButton;

	public static ButtonHandler.Button adjustLagTypeButton;

	private static readonly (int packets, float cooldown, string desc)[] LagTypeOptions = new(int, float, string)[4]
	{
		(450, 1f, "Regular"),
		(1350, 3f, "Heavy"),
		(3600, 8f, "Extreme"),
		(225, 0.5f, "Light")
	};

	private static int lagTypeIndex = 0;

	public static int LagPackets = 450;

	public static float LagCooldown = 1f;

	public static string LagTypeDescription = "Regular";

	public static ButtonHandler.Button cycleNametagTypeButton;

	private static readonly string[] NametagTypes = new string[3] { "Full", "Medium", "Minimal" };

	public static int nametagTypeIndex = 0;

	public static string NametagType = "Full";

	public static ButtonHandler.Button ChangeSoundButton;

	public static int _currentSoundIndex;

	public static int _selectedMapIndex = 0;

	public static ButtonHandler.Button teleportMapButton;

	public static ButtonHandler.Button cycleGunAnimationButton;

	public static ButtonHandler.Button leftHandGunButton;

	public static ButtonHandler.Button griplessGunsButton;

	public static ButtonHandler.Button triggerlessGunsButton;

	private static readonly string[] GunAnimations = new string[2] { "None", "Wiggly" };

	private static int gunAnimationIndex = 0;

	public static string GunAnimationType = "None";

	public static ButtonHandler.Button adjustOpacityButton;

	private static readonly (float value, string desc)[] OpacityOptions = new(float, string)[5]
	{
		(1f, "Solid"),
		(0.25f, "Glass"),
		(0.35f, "Low"),
		(0.45f, "Medium"),
		(0.55f, "High")
	};

	private static int opacityIndex = 0;

	public static float MenuOpacity = 1f;

	public static string OpacityDescription = "Solid";

	private static string[] _defaultSettings;

	public static int CurrentFontIndex { get; private set; }

	public static string CurrentFontDescription => (Main._fonts.Count > 0 && CurrentFontIndex >= 0 && CurrentFontIndex < Main._fonts.Count) ? Main._fonts[CurrentFontIndex].Description : "Arial";

	public static string ClickSoundDescription => ButtonHandler.SoundSequence[_currentSoundIndex].Description;

	public static string MapDescription => Player._maps[_selectedMapIndex].name;

	public static string SettingsFilePath => Path.Combine(Variables.folderName, "Settings.txt");

	public static void CycleNametagType(bool forward)
	{
		if (!forward)
		{
			nametagTypeIndex = (nametagTypeIndex - 1 + NametagTypes.Length) % NametagTypes.Length;
			NametagType = NametagTypes[nametagTypeIndex];
			cycleNametagTypeButton?.SetText("Nametag Type : " + NametagType);
		}
		else
		{
			nametagTypeIndex = (nametagTypeIndex + 1) % NametagTypes.Length;
			NametagType = NametagTypes[nametagTypeIndex];
			cycleNametagTypeButton?.SetText("Nametag Type : " + NametagType);
		}
	}

	private static int ModeIndexByName(string name)
	{
		int num = Array.IndexOf(ColorModeNames, name);
		if (num < 0)
		{
			return -1;
		}
		return num;
	}

	private static bool IsOnValue(string value)
	{
		return value.Equals("On", StringComparison.OrdinalIgnoreCase) || value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1";
	}

	public static void AdjustArmLength(bool forward)
	{
		if (!forward)
		{
			ArmLengthIndex = (ArmLengthIndex - 1 + ArmLengthOptions.Length) % ArmLengthOptions.Length;
			(ArmLength, ArmLengthDescription) = ArmLengthOptions[ArmLengthIndex];
			adjustArmLengthButton?.SetText("Long Arms Length : " + ArmLengthDescription);
		}
		else
		{
			ArmLengthIndex = (ArmLengthIndex + 1) % ArmLengthOptions.Length;
			(ArmLength, ArmLengthDescription) = ArmLengthOptions[ArmLengthIndex];
			adjustArmLengthButton?.SetText("Long Arms Length : " + ArmLengthDescription);
		}
	}

	public static void CycleGunAnimation(bool forward)
	{
		if (!forward)
		{
			gunAnimationIndex = (gunAnimationIndex - 1 + GunAnimations.Length) % GunAnimations.Length;
			GunAnimationType = GunAnimations[gunAnimationIndex];
			cycleGunAnimationButton?.SetText("Gun Animation : " + GunAnimationType);
		}
		else
		{
			gunAnimationIndex = (gunAnimationIndex + 1) % GunAnimations.Length;
			GunAnimationType = GunAnimations[gunAnimationIndex];
			cycleGunAnimationButton?.SetText("Gun Animation : " + GunAnimationType);
		}
	}

	public static Color GetAnimatedColor(ColorMode mode, Color c1, Color c2, float speed = 1f, int seed = 0)
	{
		float num = Time.time * speed;
		int num2 = (int)mode;
		num2 = num2 - (num2 - 5) * (((uint)num2 > 4u) ? 1 : 0) + 24;
		int num3 = num2;
		return (num3 == 25) ? Color.Lerp(c1, c2, (Mathf.Sin(num + (float)seed * 0.5f) + 1f) * 0.5f) : c1;
	}

	public static void TriggerPlatforms(bool setActive)
	{
		Movement.isTriggerPlatforms = setActive;
	}

	public static void AdjustBoardsColor(bool forward)
	{
		CycleColor(ref boardsColorIndex, ref BoardsColor, ref BoardsColorDescription, adjustBoardsColorButton, "Boards Color", forward);
	}

	public static void AdjustButtonAnimSpeed(bool forward)
	{
		CycleAnimSpeed(ref buttonAnimSpeedIndex, ref ButtonAnimSpeed, ref ButtonAnimSpeedDescription, adjustButtonAnimSpeedButton, forward);
	}

	public static void AdjustAccentStripColor2(bool forward)
	{
		CycleColor(ref accentStripColor2Index, ref AccentStripColor2, ref AccentStripColor2Description, adjustAccentStripColor2Button, "Accent Color 2", forward);
	}

	public static void BigPointer(bool setActive)
	{
		GunLib.BigGunPointer = setActive;
	}

	public static void LeftHandGun(bool setActive)
	{
		GunLib.LeftHandGun = setActive;
		GunLib.CancelGunUse();
	}

	public static void GriplessGuns(bool setActive)
	{
		GunLib.GriplessGuns = setActive;
		GunLib.CancelGunUse();
	}

	public static void TriggerlessGuns(bool setActive)
	{
		GunLib.TriggerlessGuns = setActive;
		GunLib.CancelGunUse();
	}

	public static void CycleControllerBind(bool forward = true)
	{
		currentIndex = Array.IndexOf(controllerInputs, currentControllerInput);
		if (currentIndex == -1)
		{
			UnityEngine.Debug.LogError((object)"Error: Current input is not found in the array!");
		}
		else if (!forward)
		{
			currentIndex = (currentIndex - 1 + controllerInputs.Length) % controllerInputs.Length;
			currentControllerInput = controllerInputs[currentIndex];
			inputName = currentControllerInput.Method.Name;
			cycleControllerBindButton?.SetText("Sound Input : " + inputName);
		}
		else
		{
			currentIndex = (currentIndex + 1) % controllerInputs.Length;
			currentControllerInput = controllerInputs[currentIndex];
			inputName = currentControllerInput.Method.Name;
			cycleControllerBindButton?.SetText("Sound Input : " + inputName);
		}
	}

	private static int ColorIndexByName(string name)
	{
		int num = Array.IndexOf(CustomColorNames, name);
		if (num < 0)
		{
			return -1;
		}
		return num;
	}

	public static bool ShouldShowButton(string buttonText)
	{
		if (string.IsNullOrEmpty(buttonText))
		{
			return true;
		}
		if (buttonText.StartsWith("Background Color 2"))
		{
			if (BackgroundMode != ColorMode.Lerp)
			{
				return BackgroundMode == ColorMode.Strobe;
			}
			return true;
		}
		if (buttonText.StartsWith("Button Color 2"))
		{
			if (ButtonMode != ColorMode.Lerp)
			{
				return ButtonMode == ColorMode.Strobe;
			}
			return true;
		}
		if (buttonText.StartsWith("Enabled Button Color 2"))
		{
			if (EnabledButtonMode != ColorMode.Lerp)
			{
				return EnabledButtonMode == ColorMode.Strobe;
			}
			return true;
		}
		if (buttonText.StartsWith("Title Color 2"))
		{
			if (TitleMode != ColorMode.Lerp)
			{
				return TitleMode == ColorMode.Strobe;
			}
			return true;
		}
		if (buttonText.StartsWith("Outline Color 2"))
		{
			if (OutlineColorMode != ColorMode.Lerp)
			{
				return OutlineColorMode == ColorMode.Strobe;
			}
			return true;
		}
		if (buttonText.StartsWith("Accent Color 2"))
		{
			if (AccentStripMode != ColorMode.Lerp)
			{
				return AccentStripMode == ColorMode.Strobe;
			}
			return true;
		}
		if (buttonText.StartsWith("Boards Color 2"))
		{
			if (BoardsMode != ColorMode.Lerp)
			{
				return BoardsMode == ColorMode.Strobe;
			}
			return true;
		}
		if (buttonText.StartsWith("Outline Color "))
		{
			if (OutlineColorMode != ColorMode.Rainbow)
			{
				return OutlineColorMode != ColorMode.Pinwheel;
			}
			return false;
		}
		if (buttonText.StartsWith("Background Color "))
		{
			if (BackgroundMode != ColorMode.Rainbow)
			{
				return BackgroundMode != ColorMode.Pinwheel;
			}
			return false;
		}
		if (buttonText.StartsWith("Button Color "))
		{
			if (ButtonMode != ColorMode.Rainbow)
			{
				return ButtonMode != ColorMode.Pinwheel;
			}
			return false;
		}
		if (buttonText.StartsWith("Enabled Button Color "))
		{
			if (EnabledButtonMode != ColorMode.Rainbow)
			{
				return EnabledButtonMode != ColorMode.Pinwheel;
			}
			return false;
		}
		if (buttonText.StartsWith("Title Color "))
		{
			if (TitleMode != ColorMode.Rainbow)
			{
				return TitleMode != ColorMode.Pinwheel;
			}
			return false;
		}
		if (buttonText.StartsWith("Accent Color "))
		{
			if (AccentStripMode != ColorMode.Rainbow)
			{
				return AccentStripMode != ColorMode.Pinwheel;
			}
			return false;
		}
		if (buttonText.StartsWith("Boards Color "))
		{
			if (BoardsMode != ColorMode.Rainbow)
			{
				return BoardsMode != ColorMode.Pinwheel;
			}
			return false;
		}
		if (buttonText.StartsWith("Pinwheel Speed"))
		{
			return OutlineMode != 0;
		}
		return true;
	}

	public static void SaveSettings(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		File.WriteAllLines(path, BuildSettingsLines());
	}

	public static void AdjustAccentStripType(bool forward)
	{
		if (!forward)
		{
			accentStripTypeIndex = (accentStripTypeIndex - 1 + AccentStripTypeNames.Length) % AccentStripTypeNames.Length;
			CurrentAccentStripType = (AccentStripType)accentStripTypeIndex;
			AccentStripTypeDescription = AccentStripTypeNames[accentStripTypeIndex];
			ButtonHandler.Button button = adjustAccentStripTypeButton;
			if (button != null)
			{
				button.SetText("Accent Strip : " + AccentStripTypeDescription);
				Main.RefreshMenu();
			}
		}
		else
		{
			accentStripTypeIndex = (accentStripTypeIndex + 1) % AccentStripTypeNames.Length;
			CurrentAccentStripType = (AccentStripType)accentStripTypeIndex;
			AccentStripTypeDescription = AccentStripTypeNames[accentStripTypeIndex];
			ButtonHandler.Button button2 = adjustAccentStripTypeButton;
			if (button2 != null)
			{
				button2.SetText("Accent Strip : " + AccentStripTypeDescription);
				Main.RefreshMenu();
			}
		}
		Main.RefreshMenu();
	}

	public static void AdjustTitleColor(bool forward)
	{
		CycleColor(ref titleColorIndex, ref TitleColor, ref TitleColorDescription, titleColorButton, "Title Color", forward);
	}

	public static void CycleOutlineColorMode(bool forward)
	{
		CycleMode(ref OutlineColorMode, delegate(string v)
		{
			outlineColorModeButton?.SetText("Outline Mode : " + v);
		}, forward);
	}

	public static void AdjustProjectileSpeed(bool forward)
	{
		if (!forward)
		{
			ProjectileSpeedIndex = (ProjectileSpeedIndex - 1 + ProjectileSpeedOptions.Length) % ProjectileSpeedOptions.Length;
			(ProjectileSpeed, ProjectileSpeedDescription) = ProjectileSpeedOptions[ProjectileSpeedIndex];
			adjustProjectileSpeedButton?.SetText("Projectile Speed : " + ProjectileSpeedDescription);
		}
		else
		{
			ProjectileSpeedIndex = (ProjectileSpeedIndex + 1) % ProjectileSpeedOptions.Length;
			(ProjectileSpeed, ProjectileSpeedDescription) = ProjectileSpeedOptions[ProjectileSpeedIndex];
			adjustProjectileSpeedButton?.SetText("Projectile Speed : " + ProjectileSpeedDescription);
		}
	}

	public static void ResetToDefaultSettings()
	{
		if (_defaultSettings == null)
		{
			return;
		}
		string[] defaultSettings = _defaultSettings;
		int num = 0;
		while (num < defaultSettings.Length)
		{
			string text = defaultSettings[num];
			string[] array = text.Split(':');
			if (array.Length >= 2)
			{
				ApplyKeyValue(array[0].Trim(), string.Join(":", array.Skip(1)).Trim());
				num++;
			}
			else
			{
				num++;
			}
		}
		SaveSettings();
		Main.RefreshMenu();
		NotificationLib.SendNotification(NotificationLib.NotificationType.Loaded, "Default Settings");
	}

	public static void AdjustOutlineColor2(bool forward)
	{
		CycleColor(ref outlineColor2Index, ref OutlineColor2, ref OutlineColor2Description, adjustOutlineColor2Button, "Outline Color 2", forward);
	}

	public static void AdjustTracerPosition(bool forward)
	{
		if (!forward)
		{
			tracerPositionIndex = (tracerPositionIndex - 1 + TracerPositions.Length) % TracerPositions.Length;
			TracerPosition = TracerPositions[tracerPositionIndex];
			tracerPositionButton?.SetText("Tracer Position : " + TracerPosition);
		}
		else
		{
			tracerPositionIndex = (tracerPositionIndex + 1) % TracerPositions.Length;
			TracerPosition = TracerPositions[tracerPositionIndex];
			tracerPositionButton?.SetText("Tracer Position : " + TracerPosition);
		}
	}

	public static void CycleButtonMode(bool forward)
	{
		CycleMode(ref ButtonMode, delegate(string v)
		{
			buttonModeButton?.SetText("Button Mode : " + v);
		}, forward);
	}

	public static void AdjustWallWalkStrength(bool forward)
	{
		if (!forward)
		{
			WallWalkStrengthIndex = (WallWalkStrengthIndex - 1 + WallWalkOptions.Length) % WallWalkOptions.Length;
			(WallWalkStrength, WalkWalkStrengthDescription) = WallWalkOptions[WallWalkStrengthIndex];
			adjustWalkWalkStrengthButton?.SetText("Wall Walk Strength : " + WalkWalkStrengthDescription);
		}
		else
		{
			WallWalkStrengthIndex = (WallWalkStrengthIndex + 1) % WallWalkOptions.Length;
			(WallWalkStrength, WalkWalkStrengthDescription) = WallWalkOptions[WallWalkStrengthIndex];
			adjustWalkWalkStrengthButton?.SetText("Wall Walk Strength : " + WalkWalkStrengthDescription);
		}
	}

	public static void CaptureDefaults()
	{
		if (_defaultSettings == null)
		{
			_defaultSettings = BuildSettingsLines();
		}
	}

	public static List<ButtonHandler.Button> GenerateElementButtons()
	{
		List<ButtonHandler.Button> list = new List<ButtonHandler.Button>
		{
			new ButtonHandler.Button("Return", Category.Element_Settings, isToggle: false, isActive: false, delegate
			{
				ButtonHandler.ChangePage(Category.Color_Settings);
			})
			{
				isCategory = true
			}
		};
		int currentColorElement = (int)_currentColorElement;
		currentColorElement = currentColorElement - (currentColorElement - 7) * (((uint)currentColorElement > 6u) ? 1 : 0) + 86;
		int num = currentColorElement;
		if (num != 87)
		{
			pinwheelColor1Button = Incremental("Pinwheel Color 1 : " + PinwheelColor1Name, delegate
			{
				AdjustPinwheelColor1(forward: true);
			}, delegate
			{
				AdjustPinwheelColor1(forward: false);
			});
			pinwheelColor2Button = Incremental("Pinwheel Color 2 : " + PinwheelColor2Name, delegate
			{
				AdjustPinwheelColor2(forward: true);
			}, delegate
			{
				AdjustPinwheelColor2(forward: false);
			});
			pinwheelSpeedButton = Incremental("Pinwheel Speed : " + PinwheelSpeedDescription, delegate
			{
				AdjustPinwheelSpeed(forward: true);
			}, delegate
			{
				AdjustPinwheelSpeed(forward: false);
			});
			list.Add(pinwheelColor1Button);
			list.Add(pinwheelColor2Button);
			list.Add(pinwheelSpeedButton);
		}
		else
		{
			outlineColorModeButton = Incremental("Outline Mode : " + ColorModeNames[(int)OutlineColorMode], delegate
			{
				CycleOutlineColorMode(forward: true);
			}, delegate
			{
				CycleOutlineColorMode(forward: false);
			});
			adjustOutlineColorButton = Incremental("Outline Color : " + OutlineColorDescription, delegate
			{
				AdjustOutlineColor(forward: true);
			}, delegate
			{
				AdjustOutlineColor(forward: false);
			});
			adjustOutlineColor2Button = Incremental("Outline Color 2 : " + OutlineColor2Description, delegate
			{
				AdjustOutlineColor2(forward: true);
			}, delegate
			{
				AdjustOutlineColor2(forward: false);
			});
			list.Add(outlineColorModeButton);
			if (OutlineColorMode == ColorMode.Lerp || OutlineColorMode == ColorMode.Rainbow || OutlineColorMode == ColorMode.Strobe)
			{
				adjustOutlineAnimSpeedButton = Incremental("Animation Speed : " + OutlineAnimSpeedDescription, delegate
				{
					AdjustOutlineAnimSpeed(forward: true);
				}, delegate
				{
					AdjustOutlineAnimSpeed(forward: false);
				});
				list.Add(adjustOutlineAnimSpeedButton);
				list.Add(adjustOutlineColorButton);
				list.Add(adjustOutlineColor2Button);
			}
			else
			{
				list.Add(adjustOutlineColorButton);
				list.Add(adjustOutlineColor2Button);
			}
		}
		return list;
	}

	public static void LoadSettings(string path)
	{
		if (!File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		int num = 0;
		while (num < array.Length)
		{
			string text = array[num];
			string[] array2 = text.Split(':');
			if (array2.Length >= 2)
			{
				ApplyKeyValue(array2[0].Trim(), string.Join(":", array2.Skip(1)).Trim());
				num++;
			}
			else
			{
				num++;
			}
		}
		NotificationLib.RefreshFonts();
		NXOUI.RequestRebuild();
		if ((UnityEngine.Object)(object)Variables.menuObj != (UnityEngine.Object)null)
		{
			Main.RefreshMenu();
		}
	}

	public static void SaveSettings()
	{
		SaveSettings(SettingsFilePath);
	}

	public static void AdjustButtonColor(bool forward)
	{
		CycleColor(ref buttonColorIndex, ref ButtonColor, ref ButtonColorDescription, adjustButtonColorButton, "Button Color", forward);
	}

	public static void ToggleGhostRig(bool setActive)
	{
		RigManager.ghostRigEnabled = setActive;
	}

	public static void AdjustFlySpeed(bool forward)
	{
		if (!forward)
		{
			flySpeedIndex = (flySpeedIndex - 1 + FlySpeedOptions.Length) % FlySpeedOptions.Length;
			(FlySpeed, FlySpeedDescription) = FlySpeedOptions[flySpeedIndex];
			adjustFlySpeedButton?.SetText("Fly Speed : " + FlySpeedDescription);
		}
		else
		{
			flySpeedIndex = (flySpeedIndex + 1) % FlySpeedOptions.Length;
			(FlySpeed, FlySpeedDescription) = FlySpeedOptions[flySpeedIndex];
			adjustFlySpeedButton?.SetText("Fly Speed : " + FlySpeedDescription);
		}
	}

	public static void CycleTitleMode(bool forward)
	{
		CycleMode(ref TitleMode, delegate(string v)
		{
			titleModeButton?.SetText("Title Mode : " + v);
		}, forward);
	}

	public static void AdjustTitleColor2(bool forward)
	{
		CycleColor(ref titleColor2Index, ref TitleColor2, ref TitleColor2Description, titleColor2Button, "Title Color 2", forward);
	}

	public static void AdjustAccentStripColor(bool forward)
	{
		CycleColor(ref accentStripColorIndex, ref AccentStripColor, ref AccentStripColorDescription, adjustAccentStripColorButton, "Accent Color", forward);
	}

	public static void AdjustBackgroundColor(bool forward)
	{
		CycleColor(ref backgroundColorIndex, ref BackgroundColor, ref BackgroundColorDescription, adjustBackgroundColorButton, "Background Color", forward);
	}

	public static void AdjustOpacity(bool forward)
	{
		if (!forward)
		{
			opacityIndex = (opacityIndex - 1 + OpacityOptions.Length) % OpacityOptions.Length;
			(float value, string desc) tuple = OpacityOptions[opacityIndex];
			MenuOpacity = tuple.value;
			OpacityDescription = tuple.desc;
			ButtonHandler.Button button = adjustOpacityButton;
			if (button != null)
			{
				button.SetText("Opacity : " + OpacityDescription);
				Main.RefreshMenu();
			}
		}
		else
		{
			opacityIndex = (opacityIndex + 1) % OpacityOptions.Length;
			(float value, string desc) tuple2 = OpacityOptions[opacityIndex];
			MenuOpacity = tuple2.value;
			OpacityDescription = tuple2.desc;
			ButtonHandler.Button button2 = adjustOpacityButton;
			if (button2 != null)
			{
				button2.SetText("Opacity : " + OpacityDescription);
				Main.RefreshMenu();
			}
		}
		Main.RefreshMenu();
	}

	public static void AdjustPinwheelColor2(bool forward)
	{
		CycleColor(ref pinwheelColor2Index, ref PinwheelColor2, ref PinwheelColor2Name, pinwheelColor2Button, "Pinwheel Color 2", forward);
		Main.pinwheelColor2 = (Color32)(PinwheelColor2);
	}

	public static void AdjustMenuSize(bool forward)
	{
		if (!forward)
		{
			menuSizeIndex = (menuSizeIndex - 1 + MenuSizeOptions.Length) % MenuSizeOptions.Length;
			(float scale, string desc) tuple = MenuSizeOptions[menuSizeIndex];
			Main.menuScale = tuple.scale;
			MenuSizeDescription = tuple.desc;
			ButtonHandler.Button button = adjustMenuSizeButton;
			if (button != null)
			{
				button.SetText("Menu Size : " + MenuSizeDescription);
				Main.RefreshMenu();
			}
		}
		else
		{
			menuSizeIndex = (menuSizeIndex + 1) % MenuSizeOptions.Length;
			(float scale, string desc) tuple2 = MenuSizeOptions[menuSizeIndex];
			Main.menuScale = tuple2.scale;
			MenuSizeDescription = tuple2.desc;
			ButtonHandler.Button button2 = adjustMenuSizeButton;
			if (button2 != null)
			{
				button2.SetText("Menu Size : " + MenuSizeDescription);
				Main.RefreshMenu();
			}
		}
		Main.RefreshMenu();
	}

	public static void CycleBoardsMode(bool forward)
	{
		CycleMode(ref BoardsMode, delegate(string v)
		{
			boardsModeButton?.SetText("Boards Mode : " + v);
		}, forward);
	}

	public static void CycleAccentStripMode(bool forward)
	{
		CycleMode(ref AccentStripMode, delegate(string v)
		{
			accentStripModeButton?.SetText("Accent Mode : " + v);
		}, forward);
	}

	public static void CycleFont(bool forward)
	{
		if (Main._fonts.Count != 0)
		{
			if (!forward)
			{
				CurrentFontIndex = (CurrentFontIndex - 1 + Main._fonts.Count) % Main._fonts.Count;
				CycleMenuFontButton?.SetText("Menu Font : " + CurrentFontDescription);
			}
			else
			{
				CurrentFontIndex = (CurrentFontIndex + 1) % Main._fonts.Count;
				CycleMenuFontButton?.SetText("Menu Font : " + CurrentFontDescription);
			}
			NotificationLib.RefreshFonts();
			NXOUI.RequestRebuild();
			Main.RefreshMenu();
		}
	}

	public static void AdjustEnabledButtonColor(bool forward)
	{
		CycleColor(ref enabledButtonColorIndex, ref EnabledButtonColor, ref EnabledButtonColorDescription, adjustEnabledButtonColorButton, "Enabled Button Color", forward);
	}

	public static void AdjustAntiReportRadius(bool forward)
	{
		if (!forward)
		{
			antiReportRadiusIndex = (antiReportRadiusIndex - 1 + AntiReportRadiusOptions.Length) % AntiReportRadiusOptions.Length;
			(AntiReportRadius, AntiReportRadiusDescription) = AntiReportRadiusOptions[antiReportRadiusIndex];
			adjustAntiReportRadiusButton?.SetText("Anti Report Radius : " + AntiReportRadiusDescription);
		}
		else
		{
			antiReportRadiusIndex = (antiReportRadiusIndex + 1) % AntiReportRadiusOptions.Length;
			(AntiReportRadius, AntiReportRadiusDescription) = AntiReportRadiusOptions[antiReportRadiusIndex];
			adjustAntiReportRadiusButton?.SetText("Anti Report Radius : " + AntiReportRadiusDescription);
		}
	}

	public static void AdjustButtonColor2(bool forward)
	{
		CycleColor(ref buttonColor2Index, ref ButtonColor2, ref ButtonColor2Description, adjustButtonColor2Button, "Button Color 2", forward);
	}

	public static void AdjustRoundness(bool forward)
	{
		if (!forward)
		{
			roundnessIndex = (roundnessIndex - 1 + RoundnessOptions.Length) % RoundnessOptions.Length;
			(float value, string desc) tuple = RoundnessOptions[roundnessIndex];
			MenuRoundness = tuple.value;
			RoundnessDescription = tuple.desc;
			ButtonHandler.Button button = adjustRoundnessButton;
			if (button != null)
			{
				button.SetText("Roundness : " + RoundnessDescription);
				Main.RefreshMenu();
			}
		}
		else
		{
			roundnessIndex = (roundnessIndex + 1) % RoundnessOptions.Length;
			(float value, string desc) tuple2 = RoundnessOptions[roundnessIndex];
			MenuRoundness = tuple2.value;
			RoundnessDescription = tuple2.desc;
			ButtonHandler.Button button2 = adjustRoundnessButton;
			if (button2 != null)
			{
				button2.SetText("Roundness : " + RoundnessDescription);
				Main.RefreshMenu();
			}
		}
		Main.RefreshMenu();
	}

	public static void OpenElementSettings(ColorElement element)
	{
		_currentColorElement = element;
		ButtonHandler.ChangePage(Category.Element_Settings);
	}

	public static void ClearNotifications()
	{
		NotificationLib.ClearAllNotifications();
	}

	private static ButtonHandler.Button Incremental(string text, Action up, Action down)
	{
		return new ButtonHandler.Button(text, Category.Element_Settings, isToggle: false, isActive: false, null, null, incremental: true, up, down);
	}

	public static void MenuOpenAndCloseSounds(bool setActive)
	{
		Variables.OpenAndCloseMenuSounds = setActive;
	}

	public static void AdjustOutlineAnimSpeed(bool forward)
	{
		CycleAnimSpeed(ref outlineAnimSpeedIndex, ref OutlineAnimSpeed, ref OutlineAnimSpeedDescription, adjustOutlineAnimSpeedButton, forward);
	}

	public static void AdjustAccentStripAnimSpeed(bool forward)
	{
		CycleAnimSpeed(ref accentStripAnimSpeedIndex, ref AccentStripAnimSpeed, ref AccentStripAnimSpeedDescription, adjustAccentStripAnimSpeedButton, forward);
	}

	public static void AdjustBoardsAnimSpeed(bool forward)
	{
		CycleAnimSpeed(ref boardsAnimSpeedIndex, ref BoardsAnimSpeed, ref BoardsAnimSpeedDescription, adjustBoardsAnimSpeedButton, forward);
	}

	public static void RoomNotifications(bool setActive)
	{
		NotificationLib.RoomNotifications = setActive;
	}

	public static void ToggleNotifications(bool setActive)
	{
		Variables.toggleNotifications = setActive;
	}

	public static void AdjustBackgroundAnimSpeed(bool forward)
	{
		CycleAnimSpeed(ref backgroundAnimSpeedIndex, ref BackgroundAnimSpeed, ref BackgroundAnimSpeedDescription, adjustBackgroundAnimSpeedButton, forward);
	}

	public static void CycleBackgroundMode(bool forward)
	{
		CycleMode(ref BackgroundMode, delegate(string v)
		{
			backgroundModeButton?.SetText("Background Mode : " + v);
		}, forward);
	}

	public static void AdjustEnabledButtonAnimSpeed(bool forward)
	{
		CycleAnimSpeed(ref enabledButtonAnimSpeedIndex, ref EnabledButtonAnimSpeed, ref EnabledButtonAnimSpeedDescription, adjustEnabledButtonAnimSpeedButton, forward);
	}

	public static void SoundRoomNotifications(bool setActive)
	{
		Variables.NotificationSounds = setActive;
	}

	private static string[] BuildSettingsLines()
	{
		return new string[57]
		{
			"Pinwheel Speed : " + PinwheelSpeedDescription,
			"Tracer Position : " + TracerPosition,
			"Boost Speed : " + SpeedDescription,
			"Fly Speed : " + FlySpeedDescription,
			"FPC FOV : " + FOVDescription,
			"Wall Walk Strength : " + WalkWalkStrengthDescription,
			"Long Arms Length : " + ArmLengthDescription,
			"Platform Type : " + PlatformType,
			"Sound Input : " + inputName,
			"Anti Report Radius : " + AntiReportRadiusDescription,
			"Projectile Speed : " + ProjectileSpeedDescription,
			"Projectile Size : " + ProjectileSizeDescription,
			"Menu Font : " + CurrentFontDescription,
			"Click Sound : " + ClickSoundDescription,
			"Lag Type : " + LagTypeDescription,
			"Nametag Type : " + NametagType,
			"Gun Animation : " + GunAnimationType,
			"Left Hand Gun : " + (GunLib.LeftHandGun ? "On" : "Off"),
			"Gripless Guns : " + (GunLib.GriplessGuns ? "On" : "Off"),
			"Triggerless Guns : " + (GunLib.TriggerlessGuns ? "On" : "Off"),
			"Background Color : " + BackgroundColorDescription,
			"Background Color 2 : " + BackgroundColor2Description,
			"Pinwheel Color 1 : " + PinwheelColor1Name,
			"Pinwheel Color 2 : " + PinwheelColor2Name,
			"Button Color : " + ButtonColorDescription,
			"Button Color 2 : " + ButtonColor2Description,
			"Enabled Button Color : " + EnabledButtonColorDescription,
			"Enabled Button Color 2 : " + EnabledButtonColor2Description,
			"Outline : " + OutlineModes[OutlineMode],
			"Title Color : " + TitleColorDescription,
			"Title Color 2 : " + TitleColor2Description,
			"Background Mode : " + ColorModeNames[(int)BackgroundMode],
			"Button Mode : " + ColorModeNames[(int)ButtonMode],
			"Enabled Mode : " + ColorModeNames[(int)EnabledButtonMode],
			"Title Mode : " + ColorModeNames[(int)TitleMode],
			"Outline Color : " + OutlineColorDescription,
			"Outline Color 2 : " + OutlineColor2Description,
			"Outline Mode : " + ColorModeNames[(int)OutlineColorMode],
			"Outline Anim Speed : " + OutlineAnimSpeedDescription,
			"Background Anim Speed : " + BackgroundAnimSpeedDescription,
			"Button Anim Speed : " + ButtonAnimSpeedDescription,
			"Enabled Button Anim Speed : " + EnabledButtonAnimSpeedDescription,
			"Title Anim Speed : " + TitleAnimSpeedDescription,
			"Accent Color : " + AccentStripColorDescription,
			"Accent Color 2 : " + AccentStripColor2Description,
			"Accent Mode : " + ColorModeNames[(int)AccentStripMode],
			"Accent Anim Speed : " + AccentStripAnimSpeedDescription,
			"Menu Size : " + MenuSizeDescription,
			"Roundness : " + RoundnessDescription,
			"Accent Strip Type : " + AccentStripTypeDescription,
			"Time Of Day : " + TimeOfDayDescription,
			"Opacity : " + OpacityDescription,
			"Boards Color : " + BoardsColorDescription,
			"Boards Color 2 : " + BoardsColor2Description,
			"Boards Mode : " + ColorModeNames[(int)BoardsMode],
			"Boards Anim Speed : " + BoardsAnimSpeedDescription,
			"TP Map : " + MapDescription
		};
	}

	public static void AdjustFOV(bool forward)
	{
		if (!forward)
		{
			FOVIndex = (FOVIndex - 1 + FOVOptions.Length) % FOVOptions.Length;
			(FOV, FOVDescription) = FOVOptions[FOVIndex];
			adjustFOVButton?.SetText("FPC FOV : " + FOVDescription);
		}
		else
		{
			FOVIndex = (FOVIndex + 1) % FOVOptions.Length;
			(FOV, FOVDescription) = FOVOptions[FOVIndex];
			adjustFOVButton?.SetText("FPC FOV : " + FOVDescription);
		}
	}

	public static void CycleEnabledMode(bool forward)
	{
		CycleMode(ref EnabledButtonMode, delegate(string v)
		{
			enabledModeButton?.SetText("Enabled Mode : " + v);
		}, forward);
	}

	public static void AdjustEnabledButtonColor2(bool forward)
	{
		CycleColor(ref enabledButtonColor2Index, ref EnabledButtonColor2, ref EnabledButtonColor2Description, adjustEnabledButtonColor2Button, "Enabled Button Color 2", forward);
	}

	public static void LoadSettings()
	{
		LoadSettings(SettingsFilePath);
	}

	public static void CyclePlatformType(bool forward)
	{
		if (!forward)
		{
			PlatformTypeIndex = (PlatformTypeIndex - 1 + PlatformTypes.Length) % PlatformTypes.Length;
			PlatformType = PlatformTypes[PlatformTypeIndex];
			cyclePlatformTypeButton?.SetText("Platform Type : " + PlatformType);
		}
		else
		{
			PlatformTypeIndex = (PlatformTypeIndex + 1) % PlatformTypes.Length;
			PlatformType = PlatformTypes[PlatformTypeIndex];
			cyclePlatformTypeButton?.SetText("Platform Type : " + PlatformType);
		}
	}

	public static void AdjustBoardsColor2(bool forward)
	{
		CycleColor(ref boardsColor2Index, ref BoardsColor2, ref BoardsColor2Description, adjustBoardsColor2Button, "Boards Color 2", forward);
	}

	public static void AdjustProjectileSize(bool forward)
	{
		if (!forward)
		{
			ProjectileSizeIndex = (ProjectileSizeIndex - 1 + ProjectileSizeOptions.Length) % ProjectileSizeOptions.Length;
			(snowballScale, ProjectileSizeDescription) = ProjectileSizeOptions[ProjectileSizeIndex];
			adjustProjectileSizeButton?.SetText("Projectile Size : " + ProjectileSizeDescription);
		}
		else
		{
			ProjectileSizeIndex = (ProjectileSizeIndex + 1) % ProjectileSizeOptions.Length;
			(snowballScale, ProjectileSizeDescription) = ProjectileSizeOptions[ProjectileSizeIndex];
			adjustProjectileSizeButton?.SetText("Projectile Size : " + ProjectileSizeDescription);
		}
	}

	public static void CycleMap(bool forward)
	{
		if (!forward)
		{
			_selectedMapIndex = (_selectedMapIndex - 1 + Player._maps.Length) % Player._maps.Length;
			teleportMapButton?.SetText("TP To : " + Player._maps[_selectedMapIndex].name);
		}
		else
		{
			_selectedMapIndex = (_selectedMapIndex + 1) % Player._maps.Length;
			teleportMapButton?.SetText("TP To : " + Player._maps[_selectedMapIndex].name);
		}
	}

	public static void AdjustTimeOfDay(bool forward)
	{
		if (!forward)
		{
			timeOfDayIndex = (timeOfDayIndex - 1 + TimeOfDayNames.Length) % TimeOfDayNames.Length;
			TimeOfDayDescription = TimeOfDayNames[timeOfDayIndex];
			((BetterDayNightManager)BetterDayNightManager.instance).SetTimeOfDay(timeOfDayIndex);
			adjustTimeOfDayButton?.SetText("Time Of Day : " + TimeOfDayDescription);
		}
		else
		{
			timeOfDayIndex = (timeOfDayIndex + 1) % TimeOfDayNames.Length;
			TimeOfDayDescription = TimeOfDayNames[timeOfDayIndex];
			((BetterDayNightManager)BetterDayNightManager.instance).SetTimeOfDay(timeOfDayIndex);
			adjustTimeOfDayButton?.SetText("Time Of Day : " + TimeOfDayDescription);
		}
	}

	public static void AdjustTitleAnimSpeed(bool forward)
	{
		CycleAnimSpeed(ref titleAnimSpeedIndex, ref TitleAnimSpeed, ref TitleAnimSpeedDescription, adjustTitleAnimSpeedButton, forward);
	}

	public static void AdjustLagType(bool forward)
	{
		if (!forward)
		{
			lagTypeIndex = (lagTypeIndex - 1 + LagTypeOptions.Length) % LagTypeOptions.Length;
			(LagPackets, LagCooldown, LagTypeDescription) = LagTypeOptions[lagTypeIndex];
			adjustLagTypeButton?.SetText("Lag Type : " + LagTypeDescription);
		}
		else
		{
			lagTypeIndex = (lagTypeIndex + 1) % LagTypeOptions.Length;
			(LagPackets, LagCooldown, LagTypeDescription) = LagTypeOptions[lagTypeIndex];
			adjustLagTypeButton?.SetText("Lag Type : " + LagTypeDescription);
		}
	}

	public static void AdjustPinwheelSpeed(bool forward)
	{
		if (!forward)
		{
			pinwheelSpeedIndex = (pinwheelSpeedIndex - 1 + PinwheelSpeedOptions.Length) % PinwheelSpeedOptions.Length;
			(Main.pinwheelSpeed, PinwheelSpeedDescription) = PinwheelSpeedOptions[pinwheelSpeedIndex];
			pinwheelSpeedButton?.SetText("Pinwheel Speed : " + PinwheelSpeedDescription);
		}
		else
		{
			pinwheelSpeedIndex = (pinwheelSpeedIndex + 1) % PinwheelSpeedOptions.Length;
			(Main.pinwheelSpeed, PinwheelSpeedDescription) = PinwheelSpeedOptions[pinwheelSpeedIndex];
			pinwheelSpeedButton?.SetText("Pinwheel Speed : " + PinwheelSpeedDescription);
		}
	}

	public static void ToggleArrayList(bool setActive)
	{
		NotificationLib.ArrayListEnabled = setActive;
	}

	private static void CycleMode(ref ColorMode mode, Action<string> setText, bool forward = true)
	{
		int num = ColorModeNames.Length;
		int num2 = (int)mode;
		if (!forward)
		{
			num2 = (int)(mode = (ColorMode)((num2 - 1 + num) % num));
			setText(ColorModeNames[num2]);
		}
		else
		{
			num2 = (int)(mode = (ColorMode)((num2 + 1) % num));
			setText(ColorModeNames[num2]);
		}
	}

	public static void GunLine(bool setActive)
	{
		GunLib.GunLineEnabled = setActive;
	}

	public static void GripSpeedBoost(bool setActive)
	{
		Movement.useGripForSpeedBoost = setActive;
	}

	public static void AdjustPinwheelColor1(bool forward)
	{
		CycleColor(ref pinwheelColor1Index, ref PinwheelColor1, ref PinwheelColor1Name, pinwheelColor1Button, "Pinwheel Color 1", forward);
		Main.pinwheelColor1 = (Color32)(PinwheelColor1);
	}

	public static void AdjustOutlineColor(bool forward)
	{
		CycleColor(ref outlineColorIndex, ref OutlineColor, ref OutlineColorDescription, adjustOutlineColorButton, "Outline Color", forward);
	}

	public static void ChangeClickSound(bool forward)
	{
		if (!forward)
		{
			_currentSoundIndex = (_currentSoundIndex - 1 + ButtonHandler.SoundSequence.Count) % ButtonHandler.SoundSequence.Count;
			ChangeSoundButton?.SetText("Click Sound : " + ClickSoundDescription);
		}
		else
		{
			_currentSoundIndex = (_currentSoundIndex + 1) % ButtonHandler.SoundSequence.Count;
			ChangeSoundButton?.SetText("Click Sound : " + ClickSoundDescription);
		}
	}

	public static void CycleOutlineMode(bool forward)
	{
		if (!forward)
		{
			OutlineMode = (OutlineMode - 1 + OutlineModes.Length) % OutlineModes.Length;
			ButtonHandler.Button button = outlineModeButton;
			if (button != null)
			{
				button.SetText("Outline : " + OutlineModes[OutlineMode]);
				Main.RefreshMenu();
			}
		}
		else
		{
			OutlineMode = (OutlineMode + 1) % OutlineModes.Length;
			ButtonHandler.Button button2 = outlineModeButton;
			if (button2 != null)
			{
				button2.SetText("Outline : " + OutlineModes[OutlineMode]);
				Main.RefreshMenu();
			}
		}
		Main.RefreshMenu();
	}

	public static void AdjustSpeedBoost(bool forward)
	{
		if (!forward)
		{
			currentSpeedIndex = (currentSpeedIndex - 1 + SpeedOptions.Length) % SpeedOptions.Length;
			(float speed, string desc) tuple = SpeedOptions[currentSpeedIndex];
			SpeedboostSpeed = tuple.speed;
			SpeedDescription = tuple.desc;
			SpeedboostMultiplier = SpeedboostSpeed / 5f;
			adjustSpeedBoostButton?.SetText("Boost Speed : " + SpeedDescription);
		}
		else
		{
			currentSpeedIndex = (currentSpeedIndex + 1) % SpeedOptions.Length;
			(float speed, string desc) tuple2 = SpeedOptions[currentSpeedIndex];
			SpeedboostSpeed = tuple2.speed;
			SpeedDescription = tuple2.desc;
			SpeedboostMultiplier = SpeedboostSpeed / 5f;
			adjustSpeedBoostButton?.SetText("Boost Speed : " + SpeedDescription);
		}
	}

	private static void CycleColor(ref int index, ref Color32 color, ref string desc, ButtonHandler.Button btn, string prefix, bool forward)
	{
		index = (forward ? ((index + 1) % CustomColors.Length) : ((index - 1 + CustomColors.Length) % CustomColors.Length));
		color = CustomColors[index];
		desc = CustomColorNames[index];
		btn?.SetText(prefix + " : " + desc);
	}

	public static void SwitchHands(bool setActive)
	{
		Variables.rightHandedMenu = setActive;
	}

	public static void AdjustBackgroundColor2(bool forward)
	{
		CycleColor(ref backgroundColor2Index, ref BackgroundColor2, ref BackgroundColor2Description, adjustBackgroundColor2Button, "Background Color 2", forward);
	}

	private static void ApplyKeyValue(string key, string value)
	{
		switch (key)
		{
		case "Pinwheel Speed":
		{
			int num26 = 0;
			if (num26 >= PinwheelSpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (PinwheelSpeedOptions[num26].desc == value)
				{
					pinwheelSpeedIndex = num26;
					(Main.pinwheelSpeed, PinwheelSpeedDescription) = PinwheelSpeedOptions[num26];
					pinwheelSpeedButton?.SetText("Pinwheel Speed : " + PinwheelSpeedDescription);
					break;
				}
				num26++;
			}
			while (num26 < PinwheelSpeedOptions.Length);
			break;
		}
		case "Tracer Position":
			tracerPositionIndex = Array.IndexOf(TracerPositions, value);
			if (tracerPositionIndex >= 0)
			{
				TracerPosition = TracerPositions[tracerPositionIndex];
				tracerPositionButton?.SetText("Tracer Position : " + TracerPosition);
			}
			break;
		case "Boost Speed":
		{
			int num14 = 0;
			if (num14 >= SpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (SpeedOptions[num14].desc == value)
				{
					currentSpeedIndex = num14;
					(float speed, string desc) tuple5 = SpeedOptions[num14];
					SpeedboostSpeed = tuple5.speed;
					SpeedDescription = tuple5.desc;
					SpeedboostMultiplier = SpeedboostSpeed / 5f;
					adjustSpeedBoostButton?.SetText("Boost Speed : " + SpeedDescription);
					break;
				}
				num14++;
			}
			while (num14 < SpeedOptions.Length);
			break;
		}
		case "Fly Speed":
		{
			int num30 = 0;
			if (num30 >= FlySpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (FlySpeedOptions[num30].desc == value)
				{
					flySpeedIndex = num30;
					(FlySpeed, FlySpeedDescription) = FlySpeedOptions[num30];
					adjustFlySpeedButton?.SetText("Fly Speed : " + FlySpeedDescription);
					break;
				}
				num30++;
			}
			while (num30 < FlySpeedOptions.Length);
			break;
		}
		case "FPC FOV":
		{
			int num24 = 0;
			if (num24 >= FOVOptions.Length)
			{
				break;
			}
			do
			{
				if (FOVOptions[num24].desc == value)
				{
					FOVIndex = num24;
					(FOV, FOVDescription) = FOVOptions[num24];
					adjustFOVButton?.SetText("FPC FOV : " + FOVDescription);
					break;
				}
				num24++;
			}
			while (num24 < FOVOptions.Length);
			break;
		}
		case "Wall Walk Strength":
		{
			int num3 = 0;
			if (num3 >= WallWalkOptions.Length)
			{
				break;
			}
			do
			{
				if (WallWalkOptions[num3].desc == value)
				{
					WallWalkStrengthIndex = num3;
					(WallWalkStrength, WalkWalkStrengthDescription) = WallWalkOptions[num3];
					adjustWalkWalkStrengthButton?.SetText("Wall Walk Strength : " + WalkWalkStrengthDescription);
					break;
				}
				num3++;
			}
			while (num3 < WallWalkOptions.Length);
			break;
		}
		case "Long Arms Length":
		{
			int num36 = 0;
			if (num36 >= ArmLengthOptions.Length)
			{
				break;
			}
			do
			{
				if (ArmLengthOptions[num36].desc == value)
				{
					ArmLengthIndex = num36;
					(ArmLength, ArmLengthDescription) = ArmLengthOptions[num36];
					adjustArmLengthButton?.SetText("Long Arms Length : " + ArmLengthDescription);
					break;
				}
				num36++;
			}
			while (num36 < ArmLengthOptions.Length);
			break;
		}
		case "Platform Type":
			PlatformTypeIndex = Array.IndexOf(PlatformTypes, value);
			if (PlatformTypeIndex >= 0)
			{
				PlatformType = PlatformTypes[PlatformTypeIndex];
				cyclePlatformTypeButton?.SetText("Platform Type : " + PlatformType);
			}
			break;
		case "Sound Input":
		{
			int num19 = 0;
			if (num19 >= controllerInputs.Length)
			{
				break;
			}
			do
			{
				if (controllerInputs[num19].Method.Name == value)
				{
					currentIndex = num19;
					currentControllerInput = controllerInputs[num19];
					inputName = value;
					cycleControllerBindButton?.SetText("Sound Input : " + inputName);
					break;
				}
				num19++;
			}
			while (num19 < controllerInputs.Length);
			break;
		}
		case "Anti Report Radius":
		{
			int num9 = 0;
			if (num9 >= AntiReportRadiusOptions.Length)
			{
				break;
			}
			do
			{
				if (AntiReportRadiusOptions[num9].desc == value)
				{
					antiReportRadiusIndex = num9;
					(AntiReportRadius, AntiReportRadiusDescription) = AntiReportRadiusOptions[num9];
					adjustAntiReportRadiusButton?.SetText("Anti Report Radius : " + AntiReportRadiusDescription);
					break;
				}
				num9++;
			}
			while (num9 < AntiReportRadiusOptions.Length);
			break;
		}
		case "Projectile Speed":
		{
			int num2 = 0;
			if (num2 >= ProjectileSpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (ProjectileSpeedOptions[num2].desc == value)
				{
					ProjectileSpeedIndex = num2;
					(ProjectileSpeed, ProjectileSpeedDescription) = ProjectileSpeedOptions[num2];
					adjustProjectileSpeedButton?.SetText("Projectile Speed : " + ProjectileSpeedDescription);
					break;
				}
				num2++;
			}
			while (num2 < ProjectileSpeedOptions.Length);
			break;
		}
		case "Projectile Size":
		{
			int num41 = 0;
			if (num41 >= ProjectileSizeOptions.Length)
			{
				break;
			}
			do
			{
				if (ProjectileSizeOptions[num41].desc == value)
				{
					ProjectileSizeIndex = num41;
					(snowballScale, ProjectileSizeDescription) = ProjectileSizeOptions[num41];
					adjustProjectileSizeButton?.SetText("Projectile Size : " + ProjectileSizeDescription);
					break;
				}
				num41++;
			}
			while (num41 < ProjectileSizeOptions.Length);
			break;
		}
		case "Menu Font":
		{
			int num34 = 0;
			if (num34 >= Main._fonts.Count)
			{
				break;
			}
			do
			{
				if (Main._fonts[num34].Description == value)
				{
					CurrentFontIndex = num34;
					CycleMenuFontButton?.SetText("Menu Font : " + CurrentFontDescription);
					break;
				}
				num34++;
			}
			while (num34 < Main._fonts.Count);
			break;
		}
		case "Click Sound":
		{
			bool flag = false;
			int num20 = 0;
			if (num20 < ButtonHandler.SoundSequence.Count)
			{
				do
				{
					if (ButtonHandler.SoundSequence[num20].Description == value)
					{
						_currentSoundIndex = num20;
						ButtonHandler.Button changeSoundButton = ChangeSoundButton;
						if (changeSoundButton != null)
						{
							changeSoundButton.SetText("Click Sound : " + ClickSoundDescription);
							flag = true;
						}
						else
						{
							flag = true;
						}
						break;
					}
					num20++;
				}
				while (num20 < ButtonHandler.SoundSequence.Count);
			}
			if (!flag)
			{
				_currentSoundIndex = 0;
				ChangeSoundButton?.SetText("Click Sound : " + ClickSoundDescription);
			}
			break;
		}
		case "Lag Type":
		{
			int num17 = 0;
			if (num17 >= LagTypeOptions.Length)
			{
				break;
			}
			do
			{
				if (LagTypeOptions[num17].desc == value)
				{
					lagTypeIndex = num17;
					(LagPackets, LagCooldown, LagTypeDescription) = LagTypeOptions[num17];
					adjustLagTypeButton?.SetText("Lag Type : " + LagTypeDescription);
					break;
				}
				num17++;
			}
			while (num17 < LagTypeOptions.Length);
			break;
		}
		case "Nametag Type":
			nametagTypeIndex = Array.IndexOf(NametagTypes, value);
			if (nametagTypeIndex >= 0)
			{
				NametagType = NametagTypes[nametagTypeIndex];
				cycleNametagTypeButton?.SetText("Nametag Type : " + NametagType);
			}
			break;
		case "Gun Animation":
			gunAnimationIndex = Array.IndexOf(GunAnimations, value);
			if (gunAnimationIndex >= 0)
			{
				GunAnimationType = GunAnimations[gunAnimationIndex];
				cycleGunAnimationButton?.SetText("Gun Animation : " + GunAnimationType);
			}
			break;
		case "Left Hand Gun":
			GunLib.LeftHandGun = IsOnValue(value);
			if (leftHandGunButton != null)
			{
				leftHandGunButton.Enabled = GunLib.LeftHandGun;
			}
			break;
		case "Gripless Guns":
			GunLib.GriplessGuns = IsOnValue(value);
			if (griplessGunsButton != null)
			{
				griplessGunsButton.Enabled = GunLib.GriplessGuns;
			}
			break;
		case "Triggerless Guns":
			GunLib.TriggerlessGuns = IsOnValue(value);
			if (triggerlessGunsButton != null)
			{
				triggerlessGunsButton.Enabled = GunLib.TriggerlessGuns;
			}
			break;
		case "Background Color":
		{
			int num39 = ColorIndexByName(value);
			if (num39 >= 0)
			{
				backgroundColorIndex = num39;
				BackgroundColor = CustomColors[num39];
				BackgroundColorDescription = CustomColorNames[num39];
				adjustBackgroundColorButton?.SetText("Background Color : " + BackgroundColorDescription);
			}
			break;
		}
		case "Background Color 2":
		{
			int num31 = ColorIndexByName(value);
			if (num31 >= 0)
			{
				backgroundColor2Index = num31;
				BackgroundColor2 = CustomColors[num31];
				BackgroundColor2Description = CustomColorNames[num31];
				adjustBackgroundColor2Button?.SetText("Background Color 2 : " + BackgroundColor2Description);
			}
			break;
		}
		case "Pinwheel Color 1":
		{
			int num21 = ColorIndexByName(value);
			if (num21 >= 0)
			{
				pinwheelColor1Index = num21;
				PinwheelColor1 = CustomColors[num21];
				PinwheelColor1Name = CustomColorNames[num21];
				ButtonHandler.Button button2 = pinwheelColor1Button;
				if (button2 != null)
				{
					button2.SetText("Pinwheel Color 1 : " + PinwheelColor1Name);
					Main.pinwheelColor1 = (Color32)(PinwheelColor1);
				}
				else
				{
					Main.pinwheelColor1 = (Color32)(PinwheelColor1);
				}
			}
			break;
		}
		case "Pinwheel Color 2":
		{
			int num18 = ColorIndexByName(value);
			if (num18 >= 0)
			{
				pinwheelColor2Index = num18;
				PinwheelColor2 = CustomColors[num18];
				PinwheelColor2Name = CustomColorNames[num18];
				ButtonHandler.Button button = pinwheelColor2Button;
				if (button != null)
				{
					button.SetText("Pinwheel Color 2 : " + PinwheelColor2Name);
					Main.pinwheelColor2 = (Color32)(PinwheelColor2);
				}
				else
				{
					Main.pinwheelColor2 = (Color32)(PinwheelColor2);
				}
			}
			break;
		}
		case "Button Color":
		{
			int num12 = ColorIndexByName(value);
			if (num12 >= 0)
			{
				buttonColorIndex = num12;
				ButtonColor = CustomColors[num12];
				ButtonColorDescription = CustomColorNames[num12];
				adjustButtonColorButton?.SetText("Button Color : " + ButtonColorDescription);
			}
			break;
		}
		case "Button Color 2":
		{
			int num5 = ColorIndexByName(value);
			if (num5 >= 0)
			{
				buttonColor2Index = num5;
				ButtonColor2 = CustomColors[num5];
				ButtonColor2Description = CustomColorNames[num5];
				adjustButtonColor2Button?.SetText("Button Color 2 : " + ButtonColor2Description);
			}
			break;
		}
		case "Enabled Button Color":
		{
			int num47 = ColorIndexByName(value);
			if (num47 >= 0)
			{
				enabledButtonColorIndex = num47;
				EnabledButtonColor = CustomColors[num47];
				EnabledButtonColorDescription = CustomColorNames[num47];
				adjustEnabledButtonColorButton?.SetText("Enabled Button Color : " + EnabledButtonColorDescription);
			}
			break;
		}
		case "Enabled Button Color 2":
		{
			int num43 = ColorIndexByName(value);
			if (num43 >= 0)
			{
				enabledButtonColor2Index = num43;
				EnabledButtonColor2 = CustomColors[num43];
				EnabledButtonColor2Description = CustomColorNames[num43];
				adjustEnabledButtonColor2Button?.SetText("Enabled Button Color 2 : " + EnabledButtonColor2Description);
			}
			break;
		}
		case "Title Color":
		{
			int num37 = ColorIndexByName(value);
			if (num37 >= 0)
			{
				titleColorIndex = num37;
				TitleColor = CustomColors[num37];
				TitleColorDescription = CustomColorNames[num37];
				titleColorButton?.SetText("Title Color : " + TitleColorDescription);
			}
			break;
		}
		case "Title Color 2":
		{
			int num33 = ColorIndexByName(value);
			if (num33 >= 0)
			{
				titleColor2Index = num33;
				TitleColor2 = CustomColors[num33];
				TitleColor2Description = CustomColorNames[num33];
				titleColor2Button?.SetText("Title Color 2 : " + TitleColor2Description);
			}
			break;
		}
		case "Outline Color":
		{
			int num28 = ColorIndexByName(value);
			if (num28 >= 0)
			{
				outlineColorIndex = num28;
				OutlineColor = CustomColors[num28];
				OutlineColorDescription = CustomColorNames[num28];
				adjustOutlineColorButton?.SetText("Outline Color : " + OutlineColorDescription);
			}
			break;
		}
		case "Outline Color 2":
		{
			int num22 = ColorIndexByName(value);
			if (num22 >= 0)
			{
				outlineColor2Index = num22;
				OutlineColor2 = CustomColors[num22];
				OutlineColor2Description = CustomColorNames[num22];
				adjustOutlineColor2Button?.SetText("Outline Color 2 : " + OutlineColor2Description);
			}
			break;
		}
		case "Outline":
			OutlineMode = Array.IndexOf(OutlineModes, value);
			if (OutlineMode < 0)
			{
				OutlineMode = 1;
				outlineModeButton?.SetText("Outline : " + OutlineModes[OutlineMode]);
			}
			else
			{
				outlineModeButton?.SetText("Outline : " + OutlineModes[OutlineMode]);
			}
			break;
		case "Background Mode":
		{
			int num15 = ModeIndexByName(value);
			if (num15 >= 0)
			{
				BackgroundMode = (ColorMode)num15;
				backgroundModeButton?.SetText("Background Mode : " + ColorModeNames[num15]);
			}
			break;
		}
		case "Button Mode":
		{
			int num11 = ModeIndexByName(value);
			if (num11 >= 0)
			{
				ButtonMode = (ColorMode)num11;
				buttonModeButton?.SetText("Button Mode : " + ColorModeNames[num11]);
			}
			break;
		}
		case "Enabled Mode":
		{
			int num7 = ModeIndexByName(value);
			if (num7 >= 0)
			{
				EnabledButtonMode = (ColorMode)num7;
				enabledModeButton?.SetText("Enabled Mode : " + ColorModeNames[num7]);
			}
			break;
		}
		case "Title Mode":
		{
			int num4 = ModeIndexByName(value);
			if (num4 >= 0)
			{
				TitleMode = (ColorMode)num4;
				titleModeButton?.SetText("Title Mode : " + ColorModeNames[num4]);
			}
			break;
		}
		case "Outline Mode":
		{
			int num46 = ModeIndexByName(value);
			if (num46 >= 0)
			{
				OutlineColorMode = (ColorMode)num46;
				outlineColorModeButton?.SetText("Outline Mode : " + ColorModeNames[num46]);
			}
			break;
		}
		case "Outline Anim Speed":
		{
			int num45 = 0;
			if (num45 >= AnimationSpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (AnimationSpeedOptions[num45].desc == value)
				{
					outlineAnimSpeedIndex = num45;
					(OutlineAnimSpeed, OutlineAnimSpeedDescription) = AnimationSpeedOptions[num45];
					adjustOutlineAnimSpeedButton?.SetText("Animation Speed : " + OutlineAnimSpeedDescription);
					break;
				}
				num45++;
			}
			while (num45 < AnimationSpeedOptions.Length);
			break;
		}
		case "Background Anim Speed":
		{
			int num44 = 0;
			if (num44 >= AnimationSpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (AnimationSpeedOptions[num44].desc == value)
				{
					backgroundAnimSpeedIndex = num44;
					(BackgroundAnimSpeed, BackgroundAnimSpeedDescription) = AnimationSpeedOptions[num44];
					adjustBackgroundAnimSpeedButton?.SetText("Animation Speed : " + BackgroundAnimSpeedDescription);
					break;
				}
				num44++;
			}
			while (num44 < AnimationSpeedOptions.Length);
			break;
		}
		case "Button Anim Speed":
		{
			int num42 = 0;
			if (num42 >= AnimationSpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (AnimationSpeedOptions[num42].desc == value)
				{
					buttonAnimSpeedIndex = num42;
					(ButtonAnimSpeed, ButtonAnimSpeedDescription) = AnimationSpeedOptions[num42];
					adjustButtonAnimSpeedButton?.SetText("Animation Speed : " + ButtonAnimSpeedDescription);
					break;
				}
				num42++;
			}
			while (num42 < AnimationSpeedOptions.Length);
			break;
		}
		case "Enabled Button Anim Speed":
		{
			int num40 = 0;
			if (num40 >= AnimationSpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (AnimationSpeedOptions[num40].desc == value)
				{
					enabledButtonAnimSpeedIndex = num40;
					(EnabledButtonAnimSpeed, EnabledButtonAnimSpeedDescription) = AnimationSpeedOptions[num40];
					adjustEnabledButtonAnimSpeedButton?.SetText("Animation Speed : " + EnabledButtonAnimSpeedDescription);
					break;
				}
				num40++;
			}
			while (num40 < AnimationSpeedOptions.Length);
			break;
		}
		case "Title Anim Speed":
		{
			int num38 = 0;
			if (num38 >= AnimationSpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (AnimationSpeedOptions[num38].desc == value)
				{
					titleAnimSpeedIndex = num38;
					(TitleAnimSpeed, TitleAnimSpeedDescription) = AnimationSpeedOptions[num38];
					adjustTitleAnimSpeedButton?.SetText("Animation Speed : " + TitleAnimSpeedDescription);
					break;
				}
				num38++;
			}
			while (num38 < AnimationSpeedOptions.Length);
			break;
		}
		case "Accent Color":
		{
			int num35 = ColorIndexByName(value);
			if (num35 >= 0)
			{
				accentStripColorIndex = num35;
				AccentStripColor = CustomColors[num35];
				AccentStripColorDescription = CustomColorNames[num35];
				adjustAccentStripColorButton?.SetText("Accent Color : " + AccentStripColorDescription);
			}
			break;
		}
		case "Accent Color 2":
		{
			int num32 = ColorIndexByName(value);
			if (num32 >= 0)
			{
				accentStripColor2Index = num32;
				AccentStripColor2 = CustomColors[num32];
				AccentStripColor2Description = CustomColorNames[num32];
				adjustAccentStripColor2Button?.SetText("Accent Color 2 : " + AccentStripColor2Description);
			}
			break;
		}
		case "Accent Mode":
		{
			int num29 = ModeIndexByName(value);
			if (num29 >= 0)
			{
				AccentStripMode = (ColorMode)num29;
				accentStripModeButton?.SetText("Accent Mode : " + ColorModeNames[num29]);
			}
			break;
		}
		case "Accent Anim Speed":
		{
			int num27 = 0;
			if (num27 >= AnimationSpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (AnimationSpeedOptions[num27].desc == value)
				{
					accentStripAnimSpeedIndex = num27;
					(AccentStripAnimSpeed, AccentStripAnimSpeedDescription) = AnimationSpeedOptions[num27];
					adjustAccentStripAnimSpeedButton?.SetText("Animation Speed : " + AccentStripAnimSpeedDescription);
					break;
				}
				num27++;
			}
			while (num27 < AnimationSpeedOptions.Length);
			break;
		}
		case "Menu Size":
		{
			int num25 = 0;
			if (num25 >= MenuSizeOptions.Length)
			{
				break;
			}
			do
			{
				if (MenuSizeOptions[num25].desc == value)
				{
					menuSizeIndex = num25;
					(Main.menuScale, MenuSizeDescription) = MenuSizeOptions[num25];
					adjustMenuSizeButton?.SetText("Menu Size : " + MenuSizeDescription);
					break;
				}
				num25++;
			}
			while (num25 < MenuSizeOptions.Length);
			break;
		}
		case "Roundness":
		{
			int num23 = 0;
			if (num23 >= RoundnessOptions.Length)
			{
				break;
			}
			do
			{
				if (RoundnessOptions[num23].desc == value)
				{
					roundnessIndex = num23;
					(MenuRoundness, RoundnessDescription) = RoundnessOptions[num23];
					adjustRoundnessButton?.SetText("Roundness : " + RoundnessDescription);
					break;
				}
				num23++;
			}
			while (num23 < RoundnessOptions.Length);
			break;
		}
		case "Accent Strip Type":
			accentStripTypeIndex = Array.IndexOf(AccentStripTypeNames, value);
			if (accentStripTypeIndex >= 0)
			{
				CurrentAccentStripType = (AccentStripType)accentStripTypeIndex;
				AccentStripTypeDescription = AccentStripTypeNames[accentStripTypeIndex];
				adjustAccentStripTypeButton?.SetText("Accent Strip : " + AccentStripTypeDescription);
			}
			break;
		case "Time Of Day":
			timeOfDayIndex = Array.IndexOf(TimeOfDayNames, value);
			if (timeOfDayIndex >= 0)
			{
				TimeOfDayDescription = TimeOfDayNames[timeOfDayIndex];
				BetterDayNightManager instance = BetterDayNightManager.instance;
				if (instance != null)
				{
					((BetterDayNightManager)instance).SetTimeOfDay(timeOfDayIndex);
					adjustTimeOfDayButton?.SetText("Time Of Day : " + TimeOfDayDescription);
				}
				else
				{
					adjustTimeOfDayButton?.SetText("Time Of Day : " + TimeOfDayDescription);
				}
			}
			break;
		case "Opacity":
		{
			int num16 = 0;
			if (num16 >= OpacityOptions.Length)
			{
				break;
			}
			do
			{
				if (OpacityOptions[num16].desc == value)
				{
					opacityIndex = num16;
					(MenuOpacity, OpacityDescription) = OpacityOptions[num16];
					adjustOpacityButton?.SetText("Opacity : " + OpacityDescription);
					break;
				}
				num16++;
			}
			while (num16 < OpacityOptions.Length);
			break;
		}
		case "Boards Color":
		{
			int num13 = ColorIndexByName(value);
			if (num13 >= 0)
			{
				boardsColorIndex = num13;
				BoardsColor = CustomColors[num13];
				BoardsColorDescription = CustomColorNames[num13];
				adjustBoardsColorButton?.SetText("Boards Color : " + BoardsColorDescription);
			}
			break;
		}
		case "Boards Color 2":
		{
			int num10 = ColorIndexByName(value);
			if (num10 >= 0)
			{
				boardsColor2Index = num10;
				BoardsColor2 = CustomColors[num10];
				BoardsColor2Description = CustomColorNames[num10];
				adjustBoardsColor2Button?.SetText("Boards Color 2 : " + BoardsColor2Description);
			}
			break;
		}
		case "Boards Mode":
		{
			int num8 = ModeIndexByName(value);
			if (num8 >= 0)
			{
				BoardsMode = (ColorMode)num8;
				boardsModeButton?.SetText("Boards Mode : " + ColorModeNames[num8]);
			}
			break;
		}
		case "Boards Anim Speed":
		{
			int num6 = 0;
			if (num6 >= AnimationSpeedOptions.Length)
			{
				break;
			}
			do
			{
				if (AnimationSpeedOptions[num6].desc == value)
				{
					boardsAnimSpeedIndex = num6;
					(BoardsAnimSpeed, BoardsAnimSpeedDescription) = AnimationSpeedOptions[num6];
					adjustBoardsAnimSpeedButton?.SetText("Animation Speed : " + BoardsAnimSpeedDescription);
					break;
				}
				num6++;
			}
			while (num6 < AnimationSpeedOptions.Length);
			break;
		}
		case "TP Map":
		{
			int num = Array.FindIndex(Player._maps, ((string name, string zone, string pos) m) => m.name == value);
			if (num >= 0)
			{
				_selectedMapIndex = num;
				teleportMapButton?.SetText("TP To : " + Player._maps[_selectedMapIndex].name);
			}
			else
			{
				teleportMapButton?.SetText("TP To : " + Player._maps[_selectedMapIndex].name);
			}
			break;
		}
		}
	}

	public static void IsTeamChecked(bool setActive)
	{
		Variables.teamCheckedESP = setActive;
	}

	private static void CycleAnimSpeed(ref int index, ref float speed, ref string desc, ButtonHandler.Button btn, bool forward)
	{
		index = (forward ? ((index + 1) % AnimationSpeedOptions.Length) : ((index - 1 + AnimationSpeedOptions.Length) % AnimationSpeedOptions.Length));
		(speed, desc) = AnimationSpeedOptions[index];
		btn?.SetText("Animation Speed : " + desc);
	}
}
