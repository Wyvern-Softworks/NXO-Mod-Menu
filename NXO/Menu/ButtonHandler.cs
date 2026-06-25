using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NXO.Mods;
using NXO.Mods.Categories;
using UnityEngine;
using UnityEngine.Networking;

namespace NXO.Menu;

public class ButtonHandler
{
	public class Button
	{
		private bool _enabled;

		public string buttonText { get; set; }

		public bool isToggle { get; set; }

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					Main.MarkActiveTicksDirty();
				}
			}
		}

		public Action onEnable { get; set; }

		public Action onDisable { get; set; }

		public bool incremental { get; set; }

		public Action up { get; set; }

		public Action down { get; set; }

		public Category Page { get; set; }

		public bool showGear { get; set; }

		public string tooltip { get; set; }

		public bool isCategory { get; set; }

		public Button(string label, Category page, bool isToggle, bool isActive, Action onClick, Action onDisable = null, bool incremental = false, Action incrementUp = null, Action incrementDown = null)
		{
			buttonText = label;
			this.isToggle = isToggle;
			onEnable = onClick;
			Page = page;
			this.onDisable = onDisable;
			Enabled = isActive;
			this.incremental = incremental;
			up = incrementUp;
			down = incrementDown;
		}

		public void SetText(string newText)
		{
			buttonText = newText;
		}
	}

	public class BtnCollider : MonoBehaviour
	{
		public Button clickedButton;

		public readonly List<Transform> animBody = new List<Transform>(6);

		public readonly List<Vector3> animBodyBase = new List<Vector3>(6);

		public readonly List<Transform> animUniform = new List<Transform>(2);

		public readonly List<Vector3> animUniformBase = new List<Vector3>(2);

		private Coroutine _pressRoutine;

		private float _factor = 1f;

		public const float ToggleScale = 0.92f;

		private const float PressScale = 0.85f;

		private const float ShrinkDuration = 0.05f;

		private const float ReleaseDuration = 0.12f;

		public void OnTriggerEnter(Collider collider)
		{
			if (collider == null)
			{
				if ((UnityEngine.Object)null == (UnityEngine.Object)null)
				{
					return;
				}
			}
			else if ((UnityEngine.Object)(object)((Component)collider).gameObject == (UnityEngine.Object)null)
			{
				return;
			}
			if (((UnityEngine.Object)((Component)collider).gameObject).name != "buttonclicker" || Time.time - _lastClickTime < 0.25f)
			{
				return;
			}
			_lastClickTime = Time.time;
			GorillaTagger taggerInstance = Variables.taggerInstance;
			if (taggerInstance != null)
			{
				taggerInstance.StartVibration(Variables.rightHandedMenu, Variables.taggerInstance.tagHapticStrength / 2f, Variables.taggerInstance.tagHapticDuration / 2f);
				PlayClickSound();
				InputHandler.LGrip();
			}
			else
			{
				PlayClickSound();
				if (!InputHandler.LGrip())
				{
					if (!Main.MenuAnimations || animBody.Count == 0)
					{
						Toggle(clickedButton);
					}
					else if (_pressRoutine != null)
					{
						((MonoBehaviour)this).StopCoroutine(_pressRoutine);
						_pressRoutine = ((MonoBehaviour)this).StartCoroutine(PressRoutine());
					}
					else
					{
						_pressRoutine = ((MonoBehaviour)this).StartCoroutine(PressRoutine());
					}
					return;
				}
			}
			TryAddModToFavorites(clickedButton);
			Toggle(clickedButton);
		}

		public void ApplyFactor(float f)
		{
			_factor = f;
			for (int num = 0; num < animBody.Count; num++)
			{
				Transform val = animBody[num];
				if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
				{
					Vector3 val2 = animBodyBase[num];
					val.localScale = new Vector3(val2.x, val2.y * f, val2.z * f);
				}
			}
			for (int num2 = 0; num2 < animUniform.Count; num2++)
			{
				Transform val3 = animUniform[num2];
				if (!((UnityEngine.Object)(object)val3 == (UnityEngine.Object)null))
				{
					val3.localScale = animUniformBase[num2] * f;
				}
			}
		}

		public void RegisterBody(Transform t)
		{
			if (!((UnityEngine.Object)(object)t == (UnityEngine.Object)null))
			{
				animBody.Add(t);
				animBodyBase.Add(t.localScale);
			}
		}

		private IEnumerator ScaleTo(float target, float duration, bool overshoot = false)
		{
			float from = _factor;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				float t = elapsed / duration;
				float k = overshoot ? EaseOutBack(t) : (1f - (1f - t) * (1f - t) * (1f - t));
				ApplyFactor(Mathf.LerpUnclamped(from, target, k));
				elapsed += Time.unscaledDeltaTime;
				yield return null;
			}
			ApplyFactor(target);
		}

		private IEnumerator PressRoutine()
		{
			Button btn = clickedButton;
			if (btn != null && btn.isToggle)
			{
				yield return ScaleTo(btn.Enabled ? 1f : 0.92f, 0.05f);
			}
			else
			{
				yield return ScaleTo(0.85f, 0.05f);
			}
			_pressRoutine = null;
			Toggle(btn);
		}

		private static float EaseOutBack(float t)
		{
			float num = t - 1f;
			return 1f + 4f * num * num * num + 3f * num * num;
		}

		public static void TryAddModToFavorites(Button button)
		{
			if (button == null || string.IsNullOrEmpty(button.buttonText) || button.isCategory || (!button.isToggle && button.onEnable == null))
			{
				return;
			}
			if (FavoriteMods.Contains(button))
			{
				FavoriteMods.Remove(button);
				favoriteMods.Remove(button.buttonText);
				GorillaTagger taggerInstance = Variables.taggerInstance;
				if (taggerInstance != null)
				{
					VRRig offlineVRRig = taggerInstance.offlineVRRig;
					if (offlineVRRig != null)
					{
						offlineVRRig.PlayHandTapLocal(28, !Variables.rightHandedMenu, 1f);
						NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, "Unfavorited `" + button.buttonText + "`");
						SaveFavoriteMods();
						Main.RefreshMenu();
						return;
					}
				}
				NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, "Unfavorited `" + button.buttonText + "`");
				SaveFavoriteMods();
				Main.RefreshMenu();
				return;
			}
			FavoriteMods.Add(button);
			favoriteMods.Add(button.buttonText);
			GorillaTagger taggerInstance2 = Variables.taggerInstance;
			if (taggerInstance2 != null)
			{
				VRRig offlineVRRig2 = taggerInstance2.offlineVRRig;
				if (offlineVRRig2 != null)
				{
					offlineVRRig2.PlayHandTapLocal(28, !Variables.rightHandedMenu, 1f);
					NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Favorited `" + button.buttonText + "`");
					SaveFavoriteMods();
					Main.RefreshMenu();
				}
			}
			NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Favorited `" + button.buttonText + "`");
			SaveFavoriteMods();
			Main.RefreshMenu();
		}

		public void RegisterUniform(Transform t)
		{
			if (!((UnityEngine.Object)(object)t == (UnityEngine.Object)null))
			{
				animUniform.Add(t);
				animUniformBase.Add(t.localScale);
			}
		}
	}

	public enum SoundType
	{
		AssetBundle,
		HandTap,
		EmbeddedWav,
		CustomFile
	}

	public class SoundEntry
	{
		public SoundType Type;

		public string AssetPath;

		public string ClipName;

		public string Description;

		public int HandTapIndex;

		public SoundEntry(SoundType type, string resourcePath, string clipName, string description)
		{
			Type = type;
			AssetPath = resourcePath;
			ClipName = clipName;
			Description = description;
		}

		public SoundEntry(SoundType type, int index, string desc)
		{
			Type = type;
			HandTapIndex = index;
			Description = desc;
		}
	}

	private static float _lastClickTime;

	private static Coroutine _loadingCoroutine;

	private const float CLICK_COOLDOWN = 0.25f;

	public static List<string> favoriteMods = new List<string>();

	public static List<Button> FavoriteMods = new List<Button>();

	public static readonly List<SoundEntry> SoundSequence = new List<SoundEntry>
	{
		new SoundEntry(SoundType.AssetBundle, "NXO.Resources.thocky", "thocky", "Thocky"),
		new SoundEntry(SoundType.AssetBundle, "NXO.Resources.click1", "click", "Pop"),
		new SoundEntry(SoundType.AssetBundle, "NXO.Resources.minecraftclick", "minecraftclick", "Minecraft"),
		new SoundEntry(SoundType.HandTap, 67, "OG Button"),
		new SoundEntry(SoundType.HandTap, 66, "Key Switch")
	};

	private static readonly Dictionary<string, AudioClip> _customClickClips = new Dictionary<string, AudioClip>();

	private static string _currentOpenPreset;

	private static string _currentViewingPreset;

	private static readonly Dictionary<string, List<Button>> _presetCategoryButtons = new Dictionary<string, List<Button>>();

	public static Button _gearTargetButton;

	private static string AutoSavePrefKey = "NXO_AutoSave";

	public static bool OverlapCustomClickSounds = false;

	private static readonly Dictionary<string, AudioClip> _preloadedClips = new Dictionary<string, AudioClip>();

	private static readonly List<AssetBundle> _loadedBundles = new List<AssetBundle>();

	private static string CustomClickSoundsFolderPath => Path.Combine(Variables.folderName, "Custom Click Sounds");

	private static string AutoSaveDirectoryPath => Path.Combine(Variables.folderName, "Auto Save");

	private static string FavoriteModsFilePath => Path.Combine(Variables.folderName, "FavoriteMods.txt");

	private static string PresetsDirectoryPath => Path.Combine(Variables.folderName, "Presets");

	public static void LoadCustomClickSounds()
	{
		foreach (KeyValuePair<string, AudioClip> current in _customClickClips)
		{
			if ((UnityEngine.Object)(object)current.Value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current.Value);
			}
		}
		_customClickClips.Clear();
		SoundSequence.RemoveAll((SoundEntry s) => s.Type == SoundType.CustomFile);
		string[] array;
		if (!Directory.Exists(CustomClickSoundsFolderPath))
		{
			Directory.CreateDirectory(CustomClickSoundsFolderPath);
			CreateCustomClickInstructions();
		}
		array = (from f in Directory.GetFiles(CustomClickSoundsFolderPath, "*.*")
			where f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
			select f).ToArray();
		if (array.Length == 0)
		{
			SoundSequence.Add(new SoundEntry(SoundType.CustomFile, "", "", "Custom Placeholder"));
			return;
		}
		foreach (string text in array)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
			SoundSequence.Add(new SoundEntry(SoundType.CustomFile, text, "", fileNameWithoutExtension));
		}
	}

	private static void SafeInvoke(Action action, string label)
	{
		try
		{
			action?.Invoke();
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError((object)("[NXO] Button action failed for '" + label + "': " + ex));
		}
	}

	private static Button FindButtonInCurrentContext(string targetName)
	{
		if (Variables.currentPage == Category.Element_Settings)
		{
			List<Button> source = Settings.GenerateElementButtons();
			Button button = source.FirstOrDefault((Button b) => b != null && b.buttonText == targetName);
			if (button != null)
			{
				return button;
			}
			return ModButtons.buttons.FirstOrDefault((Button b) => b != null && b.buttonText == targetName);
		}
		return ModButtons.buttons.FirstOrDefault((Button b) => b != null && b.buttonText == targetName);
	}

	public static void LoadAutoSavePreference()
	{
		Variables.AutoSave = PlayerPrefs.GetInt("NXO_AutoSave", 1) == 1;
	}

	public static void OpenFolder(string path)
	{
		Directory.CreateDirectory(path);
		NotificationLib.SendNotification(NotificationLib.NotificationType.Info, "Folder ready: " + path);
	}

	public static void PreloadAllClickSounds()
	{
		foreach (SoundEntry current in SoundSequence)
		{
			if (current.Type != SoundType.AssetBundle || _preloadedClips.ContainsKey(current.ClipName))
			{
				continue;
			}
			AssetBundle val = AssetHandler.LoadAssetBundle(current.AssetPath);
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				UnityEngine.Debug.LogError((object)("[NXO] Failed to preload bundle: '" + current.AssetPath + "'"));
				continue;
			}
			AudioClip val2 = val.LoadAsset<AudioClip>(current.ClipName);
			if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null)
			{
				_preloadedClips[current.ClipName] = val2;
				_loadedBundles.Add(val);
			}
			else
			{
				UnityEngine.Debug.LogError((object)("[NXO] Failed to preload clip '" + current.ClipName + "'"));
				val.Unload(true);
			}
		}
	}

	private static void ViewPresetSettings(string presetName)
	{
		_currentViewingPreset = presetName + "_Settings";
		string path = Path.Combine(PresetsDirectoryPath, presetName, "Settings.txt");
		List<Button> list = new List<Button>
		{
			new Button("Return", Category.Saved_Presets, isToggle: false, isActive: false, () =>
			{
				_currentViewingPreset = null;
				GeneratePresetCategoryButtons(presetName);
				Main._lastPageDrawn = Category.Home;
				Main._lastDrawModVersion = -1;
				Main.RedrawButtonList();
			})
			{
				isCategory = true
			}
		};
		if (File.Exists(path))
		{
			string[] array = File.ReadAllLines(path);
			foreach (string label in array)
			{
				list.Add(new Button(label, Category.Saved_Presets, isToggle: false, isActive: false, null));
			}
		}
		_presetCategoryButtons[_currentViewingPreset] = list;
		Variables.currentPage = Category.Saved_Presets;
		Main._lastPageDrawn = Category.Home;
		Main._lastDrawModVersion = -1;
		Main.RedrawButtonList();
	}

	private static void BeginRenamePreset(string presetName)
	{
		SearchAndKeyboard.OpenTypingKeyboard(presetName, "Enter preset name...");
		SearchAndKeyboard.onTypingComplete = (string newName) =>
		{
			string text = newName.Trim();
			newName = text;
			if (!string.IsNullOrEmpty(newName) && !(newName == presetName))
			{
				string text2 = Path.Combine(PresetsDirectoryPath, presetName);
				string text3 = Path.Combine(PresetsDirectoryPath, newName);
				if (Directory.Exists(text2))
				{
					if (Directory.Exists(text3))
					{
						NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Preset Name Already Exists");
					}
					else
					{
						Directory.Move(text2, text3);
						_presetCategoryButtons.Remove(presetName);
						_presetCategoryButtons.Remove(newName);
						_currentOpenPreset = null;
						_currentViewingPreset = null;
						GeneratePresetButtons();
						NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Renamed " + presetName + " → " + newName);
						Main.RedrawButtonList();
					}
				}
			}
		};
	}

	private static void OpenPresetCategory(string presetName)
	{
		_currentOpenPreset = presetName;
		_currentViewingPreset = null;
		if (!_presetCategoryButtons.ContainsKey(presetName))
		{
			GeneratePresetCategoryButtons(presetName);
			Variables.currentPage = Category.Saved_Presets;
			Main._lastPageDrawn = Category.Home;
			Main._lastDrawModVersion = -1;
			Main.RedrawButtonList();
		}
		else
		{
			Variables.currentPage = Category.Saved_Presets;
			Main._lastPageDrawn = Category.Home;
			Main._lastDrawModVersion = -1;
			Main.RedrawButtonList();
		}
	}

	public static void NavigatePage(bool forward)
	{
		int num;
		if (!SearchAndKeyboard.isSearching || string.IsNullOrWhiteSpace(SearchAndKeyboard.inputText))
		{
			List<Button> currentButtons = GetCurrentButtons();
			num = currentButtons.Count((Button b) => b != null && Settings.ShouldShowButton(b.buttonText));
			if (num == 0)
			{
				return;
			}
		}
		else
		{
			List<Button> currentButtons = ModButtons.buttons.Where((Button b) => b?.buttonText.Contains(SearchAndKeyboard.inputText, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
			num = currentButtons.Count((Button b) => b != null && Settings.ShouldShowButton(b.buttonText));
			if (num == 0)
			{
				return;
			}
		}
		int num2 = Mathf.CeilToInt((float)num / (float)Variables.ButtonsPerPage);
		if (!forward)
		{
			Variables.currentCategoryPage = (Variables.currentCategoryPage - 1 + num2) % num2;
			Main.RedrawButtonList(-1);
			return;
		}
		Variables.currentCategoryPage = (Variables.currentCategoryPage + 1) % num2;
		Main.RedrawButtonList(1);
	}

	public static void PlayClickSound()
	{
		if (Settings._currentSoundIndex < 0 || Settings._currentSoundIndex >= SoundSequence.Count)
		{
			return;
		}

		SoundEntry soundEntry = SoundSequence[Settings._currentSoundIndex];
		if (soundEntry.Type == SoundType.AssetBundle)
		{
			if (_preloadedClips.TryGetValue(soundEntry.ClipName, out AudioClip value))
			{
				AssetHandler.PlaySound(RigManager.GetHandObject, value, 0.625f);
			}
			else
			{
				UnityEngine.Debug.LogError((object)("[NXO] Clip not preloaded: '" + soundEntry.ClipName + "'"));
			}
			return;
		}

		if (soundEntry.Type == SoundType.CustomFile)
		{
			if (string.IsNullOrEmpty(soundEntry.AssetPath))
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Custom Sound Is Empty. To Add One Go To `Gorilla Tag > NXO Mod Menu > Custom Click Sounds`, Paste Your Sound There And Restart Your Game.");
				return;
			}

			if (OverlapCustomClickSounds)
			{
				GorillaTagger taggerInstance = Variables.taggerInstance;
				if ((UnityEngine.Object)(object)taggerInstance == (UnityEngine.Object)null)
				{
					return;
				}

				((MonoBehaviour)taggerInstance).StartCoroutine(LoadCustomClickClipCoroutine(soundEntry.AssetPath, delegate(AudioClip clip)
				{
					if (!((UnityEngine.Object)(object)clip == (UnityEngine.Object)null) && !((UnityEngine.Object)(object)RigManager.GetHandObject == (UnityEngine.Object)null))
					{
						GameObject val = new GameObject("CustomClickTemp");
						val.transform.SetParent(RigManager.GetHandObject.transform);
						val.transform.localPosition = Vector3.zero;
						AudioSource val2 = val.AddComponent<AudioSource>();
						val2.clip = clip;
						val2.volume = 0.45f;
						val2.spatialBlend = 1f;
						val2.Play();
						((MonoBehaviour)Variables.taggerInstance).StartCoroutine(CoroutineHelper.DestroyAfter(val, clip.length + 0.1f));
					}
				}));
				return;
			}

			GorillaTagger taggerInstance2 = Variables.taggerInstance;
			if ((UnityEngine.Object)(object)taggerInstance2 == (UnityEngine.Object)null)
			{
				return;
			}

			if (_loadingCoroutine != null)
			{
				((MonoBehaviour)taggerInstance2).StopCoroutine(_loadingCoroutine);
			}

			_loadingCoroutine = ((MonoBehaviour)taggerInstance2).StartCoroutine(LoadCustomClickClipCoroutine(soundEntry.AssetPath, delegate(AudioClip clip)
			{
				_loadingCoroutine = null;
				if ((UnityEngine.Object)(object)clip != (UnityEngine.Object)null)
				{
					AssetHandler.PlaySound(RigManager.GetHandObject, clip, 0.625f);
				}
			}));
			return;
		}

		if (soundEntry.Type == SoundType.EmbeddedWav)
		{
			GorillaTagger taggerInstance3 = Variables.taggerInstance;
			if ((UnityEngine.Object)(object)taggerInstance3 == (UnityEngine.Object)null)
			{
				return;
			}

			if (_loadingCoroutine != null)
			{
				((MonoBehaviour)taggerInstance3).StopCoroutine(_loadingCoroutine);
			}

			_loadingCoroutine = ((MonoBehaviour)taggerInstance3).StartCoroutine(AssetHandler.LoadEmbeddedAudioClip(soundEntry.AssetPath, delegate(AudioClip clip)
			{
				_loadingCoroutine = null;
				if ((UnityEngine.Object)(object)clip != (UnityEngine.Object)null)
				{
					AssetHandler.PlaySound(RigManager.GetHandObject, clip);
				}
			}));
			return;
		}

		if ((UnityEngine.Object)(object)Variables.taggerInstance?.offlineVRRig != (UnityEngine.Object)null)
		{
			Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(soundEntry.HandTapIndex, !Variables.rightHandedMenu, 0.625f);
		}
	}

	public static bool IsCategoryButton(Button button)
	{
		return button.isCategory;
	}

	private static void LoadSpecificPreset(string presetName)
	{
		string path = Path.Combine(PresetsDirectoryPath, presetName);
		string path2 = Path.Combine(path, "Mods.txt");
		string path3 = Path.Combine(path, "Settings.txt");

		if (File.Exists(path2))
		{
			HashSet<string> hashSet = File.ReadAllLines(path2).ToHashSet();
			foreach (Button current in ModButtons.buttons.Where((Button b) => b != null))
			{
				bool shouldBeEnabled = current.isToggle && hashSet.Contains(current.buttonText);
				if (current.Enabled == shouldBeEnabled)
				{
					continue;
				}

				current.Enabled = shouldBeEnabled;
				if (shouldBeEnabled)
				{
					SafeInvoke(current.onEnable, current.buttonText);
					NXOUI.AddMod(current.buttonText);
				}
				else
				{
					SafeInvoke(current.onDisable, current.buttonText);
					NXOUI.RemoveMod(current.buttonText);
				}
			}
		}

		if (File.Exists(path3))
		{
			Settings.LoadSettings(path3);
		}

		NotificationLib.SendNotification(NotificationLib.NotificationType.Loaded, "Preset `" + presetName + "`");
	}

	public static void LoadAutosavedStuff()
	{
		if (!Variables.AutoSave)
		{
			return;
		}
		string path = Path.Combine(AutoSaveDirectoryPath, "Mods.txt");
		string path2 = Path.Combine(AutoSaveDirectoryPath, "Settings.txt");

		if (File.Exists(path2))
		{
			Settings.LoadSettings(path2);
		}
		if (File.Exists(path))
		{
			File.WriteAllLines(path, Array.Empty<string>());
		}
	}

	public static void OpenGear_Menu(Button button)
	{
		_gearTargetButton = button;
		ChangePage(Category.Gear_Menu);
	}

	private static void CreateCustomClickInstructions()
	{
		string path = Path.Combine(CustomClickSoundsFolderPath, "Instructions.txt");
		if (!File.Exists(path))
		{
			File.WriteAllText(path, "=== HOW TO ADD CUSTOM CLICK SOUNDS ===\r\n\r\n1. Find an audio file (.mp3 or .wav)\r\n   - You can download sounds from YouTube using online converters\r\n   - Make sure it's one of these file types: MP3 or WAV\r\n\r\n2. Copy your audio file into this folder (Custom Click Sounds)\r\n   - The file name will be shown as the sound name in the menu\r\n   - Example: \"thock.wav\" will show as \"thock\" in the menu\r\n\r\n3. In-game, go to Settings > Click Sound and cycle to your custom sound\r\n   - It will appear at the end of the sound list\r\n\r\nTIPS:\r\n- Keep file names short and simple\r\n- Short sounds work best as click sounds\r\n- Don't use special characters in file names\r\n- You can add as many sounds as you want, each file becomes its own option\r\n\r\nThat's it! Have fun!");
		}
	}

	public static void SaveAutosavedStuff()
	{
		Directory.CreateDirectory(AutoSaveDirectoryPath);
		List<string> contents = (from b in ModButtons.buttons
			where b != null && b.isToggle && b.Enabled
			select b.buttonText).ToList();
		File.WriteAllLines(Path.Combine(AutoSaveDirectoryPath, "Mods.txt"), contents);
		Settings.SaveSettings(Path.Combine(AutoSaveDirectoryPath, "Settings.txt"));
	}

	public static void SetAutoSave(bool on)
	{
		Variables.AutoSave = on;
		PlayerPrefs.SetInt("NXO_AutoSave", on ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static void SetMenuAnimations(bool on)
	{
		Main.MenuAnimations = on;
	}

	public static void GeneratePresetButtons()
	{
		List<Button> list = ModButtons.buttons.Where((Button b) => b.Page != Category.Saved_Presets).ToList();
		list.Add(new Button("Return", Category.Saved_Presets, isToggle: false, isActive: false, delegate
		{
			ChangePage(Category.Presets);
		})
		{
			isCategory = true
		});
		if (Directory.Exists(PresetsDirectoryPath))
		{
			string[] directories = Directory.GetDirectories(PresetsDirectoryPath);
			foreach (string path in directories)
			{
				string presetName = Path.GetFileName(path);
				list.Add(new Button(presetName, Category.Saved_Presets, isToggle: false, isActive: false, () =>
				{
					OpenPresetCategory(presetName);
				}));
			}
			ModButtons.buttons = list.ToArray();
		}
		else
		{
			ModButtons.buttons = list.ToArray();
		}
	}

	private static void ReturnToMainPage()
	{
		Variables.currentPage = Category.Home;
		Variables.currentCategoryPage = 0;
		PlayersActionList.ClearPlayerCam(clearAll: true);
		Main.RedrawButtonList(-1);
	}

	private static void GeneratePresetCategoryButtons(string presetName)
	{
		_presetCategoryButtons[presetName] = new List<Button>
		{
			new Button("Return", Category.Saved_Presets, isToggle: false, isActive: false, delegate
			{
				_currentOpenPreset = null;
				_currentViewingPreset = null;
				Main._lastPageDrawn = Category.Home;
				Main._lastDrawModVersion = -1;
				Main.RedrawButtonList();
			})
			{
				isCategory = true
			},
			new Button("Load This Preset", Category.Saved_Presets, isToggle: false, isActive: false, () =>
			{
				LoadSpecificPreset(presetName);
			}),
			new Button("Rename Preset", Category.Saved_Presets, isToggle: false, isActive: false, () =>
			{
				BeginRenamePreset(presetName);
			}),
			new Button("Delete This Preset", Category.Saved_Presets, isToggle: false, isActive: false, () =>
			{
				DeleteSpecificPreset(presetName);
			}),
			new Button("View Saved Mods", Category.Saved_Presets, isToggle: false, isActive: false, () =>
			{
				ViewPresetMods(presetName);
			}),
			new Button("View Saved Settings", Category.Saved_Presets, isToggle: false, isActive: false, () =>
			{
				ViewPresetSettings(presetName);
			})
		};
	}

	private static void DeleteSpecificPreset(string presetName)
	{
		string path = Path.Combine(PresetsDirectoryPath, presetName);
		if (Directory.Exists(path))
		{
			Directory.Delete(path, recursive: true);
			_presetCategoryButtons.Remove(presetName);
			_currentOpenPreset = null;
			_currentViewingPreset = null;
			GeneratePresetButtons();
			NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, "Preset `" + presetName + "`");
			Main.RedrawButtonList();
		}
	}

	public static List<Button> GetButtonInfoByPage(Category page)
	{
		if (page == Category.Element_Settings)
		{
			return Settings.GenerateElementButtons();
		}
		if (page == Category.Saved_Presets)
		{
			string text = _currentViewingPreset ?? _currentOpenPreset;
			if (text != null)
			{
				if (!_presetCategoryButtons.TryGetValue(text, out List<Button> value))
				{
					return new List<Button>();
				}
				return value;
			}
		}
		if (page == Category.Recorded_Macros && Macros.currentOpenMacro != null)
		{
			if (!Macros.macroCategoryButtons.TryGetValue(Macros.currentOpenMacro, out List<Button> value2))
			{
				return new List<Button>();
			}
			return value2;
		}
		return page switch
		{
			Category.Enabled => ModButtons.buttons.Where((Button b) => b?.Enabled ?? false).ToList(), 
			Category.Favorited => GetFavoritedButtons(), 
			_ => ModButtons.buttons.Where((Button b) => b != null && b.Page == page).ToList(), 
		};
	}

	public static void LoadFavoritedMods()
	{
		if (!File.Exists(FavoriteModsFilePath))
		{
			return;
		}
		favoriteMods = File.ReadAllLines(FavoriteModsFilePath).ToList();
		foreach (string modName in favoriteMods)
		{
			Button button = ModButtons.buttons.FirstOrDefault((Button b) => b != null && b.buttonText == modName);
			if (button != null && !FavoriteMods.Contains(button))
			{
				FavoriteMods.Add(button);
			}
		}
	}

	public static List<Button> GetCurrentButtons()
	{
		if (SearchAndKeyboard.isSearching && !string.IsNullOrWhiteSpace(SearchAndKeyboard.inputText))
		{
			return ModButtons.buttons.Where((Button b) => b != null && b.Page != Category.Home && b.buttonText.Contains(SearchAndKeyboard.inputText, StringComparison.OrdinalIgnoreCase)).ToList();
		}
		return GetButtonInfoByPage(Variables.currentPage);
	}

	public static void SavePreset()
	{
		Directory.CreateDirectory(PresetsDirectoryPath);
		string text = $"Preset_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
		string text2 = Path.Combine(PresetsDirectoryPath, text);
		Directory.CreateDirectory(text2);
		List<string> contents = (from b in ModButtons.buttons
			where b != null && b.isToggle && b.Enabled
			select b.buttonText).ToList();
		File.WriteAllLines(Path.Combine(text2, "Mods.txt"), contents);
		Settings.SaveSettings(Path.Combine(text2, "Settings.txt"));
		NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Preset `" + text + "`");
		GeneratePresetButtons();
	}

	public static void DisableAllMods()
	{
		foreach (Button current in ModButtons.buttons.Where((Button b) => b != null && b.isToggle && b.Enabled))
		{
			current.Enabled = false;
			Action onDisable = current.onDisable;
			if (onDisable != null)
			{
				onDisable();
			}
			NXOUI.RemoveMod(current.buttonText);
		}
		Main.RefreshMenu();
	}

	public static void ToggleButton(Button button)
	{
		if (button == null)
		{
			return;
		}
		if (!button.isToggle)
		{
			SafeInvoke(button.onEnable, button.buttonText);
			if (button.Page != Variables.currentPage)
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Info, button.buttonText);
			}
			else
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Enabled, button.buttonText);
			}
			return;
		}
		button.Enabled = !button.Enabled;
		if (button.Enabled)
		{
			SafeInvoke(button.onEnable, button.buttonText);
			NotificationLib.SendNotification(NotificationLib.NotificationType.Enabled, button.buttonText);
			NXOUI.AddMod(button.buttonText);
			Main.RedrawButtonList();
		}
		else
		{
			SafeInvoke(button.onDisable, button.buttonText);
			NotificationLib.SendNotification(NotificationLib.NotificationType.Disabled, button.buttonText);
			NXOUI.RemoveMod(button.buttonText);
			Main.RedrawButtonList();
		}
	}

	public static void ChangePage(Category newPage)
	{
		Variables.currentPage = newPage;
		Variables.currentCategoryPage = 0;
		Main._lastPageDrawn = Category.Home;
		Main._lastDrawModVersion = -1;
		switch (newPage)
		{
		case Category.Players:
			PlayersActionList.GeneratePlayerButtons();
			break;
		case Category.Player_Action:
			if (PlayersActionList.selectedPlayer != null)
			{
				PlayersActionList.GeneratePlayer_ActionButtons();
				PlayersActionList.AddPlayerCam();
				Main.RedrawButtonList(1);
				return;
			}
			break;
		}
		Main.RedrawButtonList(1);
	}

	public static void CleanupCustomClickSounds()
	{
		foreach (KeyValuePair<string, AudioClip> current in _customClickClips)
		{
			if ((UnityEngine.Object)(object)current.Value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current.Value);
			}
		}
		_customClickClips.Clear();
	}

	private static List<Button> GetFavoritedButtons()
	{
		List<Button> list = new List<Button>
		{
			new Button("<color=blue>LG On Press To Favorite</color>", Category.Favorited, isToggle: false, isActive: false, null)
		};
		list.AddRange(FavoriteMods.Where((Button b) => b != null));
		return list;
	}

	private static void ViewPresetMods(string presetName)
	{
		_currentViewingPreset = presetName + "_Mods";
		string path = Path.Combine(PresetsDirectoryPath, presetName, "Mods.txt");
		List<Button> list = new List<Button>
		{
			new Button("Return", Category.Saved_Presets, isToggle: false, isActive: false, () =>
			{
				_currentViewingPreset = null;
				GeneratePresetCategoryButtons(presetName);
				Main._lastPageDrawn = Category.Home;
				Main._lastDrawModVersion = -1;
				Main.RedrawButtonList();
			})
			{
				isCategory = true
			}
		};
		if (File.Exists(path))
		{
			string[] array = File.ReadAllLines(path);
			foreach (string label in array)
			{
				list.Add(new Button(label, Category.Saved_Presets, isToggle: false, isActive: false, null));
			}
		}
		_presetCategoryButtons[_currentViewingPreset] = list;
		Variables.currentPage = Category.Saved_Presets;
		Main._lastPageDrawn = Category.Home;
		Main._lastDrawModVersion = -1;
		Main.RedrawButtonList();
	}

	public static void Toggle(Button button)
	{
		if (button == null)
		{
			return;
		}
		if (button.buttonText.EndsWith("_DOWN"))
		{
			string text = button.buttonText.Substring(0, button.buttonText.Length - 5);
			Button button2 = FindButtonInCurrentContext(text);
			if (button2 == null)
			{
				if (null == null)
				{
					return;
				}
			}
			else if (button2.down == null)
			{
				return;
			}
			SafeInvoke(button2.down, text);
			Main.RedrawButtonList();
			return;
		}
		if (button.buttonText.EndsWith("_UP"))
		{
			string text2 = button.buttonText.Substring(0, button.buttonText.Length - 3);
			Button button3 = FindButtonInCurrentContext(text2);
			if (button3 == null)
			{
				if (null == null)
				{
					return;
				}
			}
			else if (button3.up == null)
			{
				return;
			}
			SafeInvoke(button3.up, text2);
			Main.RedrawButtonList();
			return;
		}
		switch (button.buttonText)
		{
		case "<":
			NavigatePage(forward: false);
			break;
		case ">":
			NavigatePage(forward: true);
			break;
		case "ReturnButton":
			ReturnToMainPage();
			break;
		case "Toggle Search Button":
			SearchAndKeyboard.ToggleKeyboard();
			break;
		default:
			ToggleButton(button);
			break;
		}
	}

	public static IEnumerator LoadCustomClickClipCoroutine(string filePath, Action<AudioClip> callback)
	{
		if (_customClickClips.TryGetValue(filePath, out AudioClip cached))
		{
			callback?.Invoke(cached);
			yield break;
		}
		string ext = Path.GetExtension(filePath).ToLowerInvariant();
		AudioType audioType = (AudioType)((ext == ".wav") ? 20 : 13);
		using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(new Uri(filePath).AbsoluteUri, audioType);
		yield return www.SendWebRequest();
		if ((int)www.result != 1)
		{
			yield break;
		}
		AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
		if ((UnityEngine.Object)(object)clip == (UnityEngine.Object)null)
		{
			yield break;
		}
		((UnityEngine.Object)clip).name = Path.GetFileNameWithoutExtension(filePath);
		_customClickClips[filePath] = clip;
		callback?.Invoke(clip);
	}

	public static void SaveFavoriteMods()
	{
		Directory.CreateDirectory(Variables.folderName);
		File.WriteAllLines(FavoriteModsFilePath, favoriteMods);
		NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Favorite");
	}
}
