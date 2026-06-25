using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NXO.Menu;
using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Networking;
using InputSourceType = Photon.Voice.Unity.Recorder.InputSourceType;

namespace NXO.Mods.Categories;

public static class Soundboard
{
	private static readonly Dictionary<string, bool> soundboardSoundsActive = new Dictionary<string, bool>();

	private static readonly Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();

	private static readonly Dictionary<string, AudioSource> loadedSounds = new Dictionary<string, AudioSource>();

	private static readonly Dictionary<string, string> soundboardUrls = new Dictionary<string, string>
	{
		{ "SFX", "https://raw.githubusercontent.com/Wyvern-Softworks/NXO-Mod-Menu/refs/heads/main/Resources/SoundEffects/SoundEffectsDownloadPath.txt" },
		{ "Trolling", "https://raw.githubusercontent.com/Wyvern-Softworks/NXO-Mod-Menu/refs/heads/main/Resources/Trolling/TrollingDownloadPath.txt" },
		{ "Songs", "https://raw.githubusercontent.com/Wyvern-Softworks/NXO-Mod-Menu/refs/heads/main/Resources/Songs/SongsDownloadPath.txt" }
	};

	public static Dictionary<string, string> SFX = new Dictionary<string, string>();

	public static Dictionary<string, string> Trolling = new Dictionary<string, string>();

	public static Dictionary<string, string> Songs = new Dictionary<string, string>();

	public static Dictionary<string, string> Custom = new Dictionary<string, string>();

	private static bool loop;

	private static AudioSource currentAudioSource;

	private static string currentlyPlayingSound;

	private static bool isPlaying;

	public static Dictionary<string, bool> SoundboardSoundsActive => soundboardSoundsActive;

	public static bool SoundsInitialized { get; private set; }

	private static string Custom_SoundsFolderPath => Path.Combine(Variables.folderName, "Custom_Sounds");

	private static void CreateInstructionsFile()
	{
		string path = Path.Combine(Custom_SoundsFolderPath, "Instructions.txt");
		if (!Directory.Exists(Custom_SoundsFolderPath))
		{
			Directory.CreateDirectory(Custom_SoundsFolderPath);
			if (File.Exists(path))
			{
				return;
			}
		}
		else if (File.Exists(path))
		{
			return;
		}
		string contents = "=== HOW TO ADD CUSTOM SOUNDS ===\r\n\r\n                1. Find an audio file (.mp3, .wav, or .ogg)\r\n                   - You can download sounds from YouTube using online converters\r\n                   - Make sure it's one of these file types: MP3, WAV, or OGG\r\n\r\n                2. Copy your audio file into this folder (Custom_Sounds)\r\n                   - The file name will be the button name in the menu\r\n                   - Example: \"explosion.mp3\" will show as \"explosion\" in the menu\r\n\r\n                3. In-game, go to Soundboard > Custom Sounds > Reload Custom Sounds\r\n                   - This will refresh the list and add your new sounds\r\n\r\n                4. Click any sound button to play it in-game\r\n                   - Other players will hear it through your mic\r\n                   - Press the button again to stop the sound\r\n\r\n                TIPS:\r\n                - Keep file names short and simple\r\n                - Don't use special characters in file names\r\n                - Sounds can be any length\r\n                - You can add as many sounds as you want\r\n\r\n                That's it! Have fun!";
		File.WriteAllText(path, contents);
	}

	private static void PlayAudioSource(AudioSource source, string pathOrUrl)
	{
		if ((UnityEngine.Object)(object)source == (UnityEngine.Object)null)
		{
			UnityEngine.Debug.LogError((object)"AudioSource is null.");
			return;
		}
		source.Play();
		((Component)source).transform.parent = ((Component)Variables.playerInstance).transform;
		SetupRecorder(source);
		if (loop)
		{
			Coroutine value = ((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(PlaySoundCoroutine(source, pathOrUrl));
			activeCoroutines[pathOrUrl] = value;
		}
	}

	public static void LoadCustom_Sounds()
	{
		if (!Directory.Exists(Custom_SoundsFolderPath))
		{
			Directory.CreateDirectory(Custom_SoundsFolderPath);
			UnityEngine.Debug.Log((object)("Created Custom_Sounds folder at: " + Custom_SoundsFolderPath));
			NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Custom Sounds Folder Created");
			return;
		}
		string[] array = (from file in Directory.GetFiles(Custom_SoundsFolderPath, "*.*")
			where file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)
			select file).ToArray();
		int num = 0;
		string[] array2 = array;
		int num2 = 0;
		while (num2 < array2.Length)
		{
			string text = array2[num2];
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
			if (!string.IsNullOrEmpty(fileNameWithoutExtension))
			{
				if (!Custom.ContainsKey(fileNameWithoutExtension))
				{
					Custom[fileNameWithoutExtension] = text;
					num++;
					num2++;
					continue;
				}
				UnityEngine.Debug.LogWarning((object)("Duplicate custom sound: " + fileNameWithoutExtension));
			}
			num2++;
		}
		if (num > 0)
		{
			UnityEngine.Debug.Log((object)$"Loaded {num} custom sounds from folder");
		}
	}

	private static void AddSoundButtons(List<ButtonHandler.Button> buttonList, Dictionary<string, string> sounds, Category category)
	{
		foreach (KeyValuePair<string, string> current in sounds)
		{
			string pathOrUrl = current.Value;
			buttonList.Add(new ButtonHandler.Button(current.Key, category, isToggle: true, isActive: false, delegate
			{
				PlaySound(pathOrUrl);
			}, delegate
			{
				StopSound(pathOrUrl);
			}));
		}
	}

	public static void InitializeSounds()
	{
		if (!SoundsInitialized)
		{
			if ((UnityEngine.Object)(object)CoroutineHelper.Instance == (UnityEngine.Object)null)
			{
				UnityEngine.Debug.LogError((object)"CoroutineHelper.Instance is null. Make sure CoroutineHelper is initialized in the scene.");
				NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Coroutine Helper Not Initialized");
				return;
			}
			SoundsInitialized = true;
			UnityEngine.Debug.Log((object)"Sounds Initialized");
			((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(GetSoundsToButtons());
			CoroutineHelper.InvokeAfterDelay(2f, Main.RefreshMenu);
			CreateInstructionsFile();
		}
	}

	private static void ResetPlayState(string pathOrUrl)
	{
		currentlyPlayingSound = null;
		soundboardSoundsActive[pathOrUrl] = false;
		isPlaying = false;
	}

	public static IEnumerator GetSoundsToButtons()
	{
		List<ButtonHandler.Button> buttonList = new List<ButtonHandler.Button>(ModButtons.buttons);
		yield return LoadAllSoundsCoroutine();
		AddSoundButtons(buttonList, Custom, Category.Custom_Sounds);
		AddSoundButtons(buttonList, SFX, Category.SFX);
		AddSoundButtons(buttonList, Songs, Category.Songs);
		AddSoundButtons(buttonList, Trolling, Category.Trolling);
		buttonList.Add(new ButtonHandler.Button("Custom Sounds", Category.Soundboard, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Custom_Sounds);
		})
		{
			isCategory = true
		});
		buttonList.Add(new ButtonHandler.Button("Sound Effects", Category.Soundboard, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.SFX);
		})
		{
			isCategory = true
		});
		buttonList.Add(new ButtonHandler.Button("Trolling Related", Category.Soundboard, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Trolling);
		})
		{
			isCategory = true
		});
		buttonList.Add(new ButtonHandler.Button("Songs", Category.Soundboard, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Songs);
		})
		{
			isCategory = true
		});
		buttonList.Add(new ButtonHandler.Button("Return", Category.Custom_Sounds, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Soundboard);
		})
		{
			isCategory = true
		});
		buttonList.Add(new ButtonHandler.Button("Open Custom Sounds Folder", Category.Custom_Sounds, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.OpenFolder(Path.Combine(Variables.folderName, "Custom_Sounds"));
		}));
		buttonList.Add(new ButtonHandler.Button("Reload Custom Sounds", Category.Custom_Sounds, isToggle: false, isActive: false, ReloadCustom_Sounds));
		ModButtons.buttons = buttonList.ToArray();
	}

	public static void DisableLoop()
	{
		loop = false;
	}

	public static IEnumerator DownloadAndPlaySound(string pathOrUrl)
	{
		bool isLocalFile = File.Exists(pathOrUrl);
		AudioType audioType = GetAudioType(pathOrUrl);
		string requestPath = isLocalFile ? ("file://" + pathOrUrl) : pathOrUrl;
		using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(requestPath, audioType))
		{
			yield return request.SendWebRequest();
			if ((int)request.result != 1)
			{
				UnityEngine.Debug.LogError((object)("Failed to load audio: " + request.error));
				ResetPlayState(pathOrUrl);
				yield break;
			}
			AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
			if ((UnityEngine.Object)(object)audioClip == (UnityEngine.Object)null)
			{
				UnityEngine.Debug.LogError((object)"Audio clip is null");
				ResetPlayState(pathOrUrl);
				yield break;
			}
			if (loadedSounds.TryGetValue(pathOrUrl, out AudioSource stale) && (UnityEngine.Object)(object)stale != (UnityEngine.Object)null)
			{
				stale.Stop();
				if ((UnityEngine.Object)(object)stale.clip != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)stale.clip);
				}
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)stale).gameObject);
			}
			GameObject soundObject = new GameObject("SoundFromFile");
			AudioSource audioSource = soundObject.AddComponent<AudioSource>();
			audioSource.clip = audioClip;
			audioSource.volume = 1f;
			audioSource.loop = loop;
			loadedSounds[pathOrUrl] = audioSource;
			currentAudioSource = audioSource;
			PlayAudioSource(audioSource, pathOrUrl);
		}
	}

	private static void SetupRecorder(AudioSource source)
	{
		Variables.taggerInstance.myRecorder.SourceType = (Photon.Voice.Unity.Recorder.InputSourceType)1;
		Variables.taggerInstance.myRecorder.AudioClip = source.clip;
		Variables.taggerInstance.myRecorder.RestartRecording(true);
		Variables.taggerInstance.myRecorder.LoopAudioClip = loop;
	}

	public static IEnumerator LoadAllSoundsCoroutine()
	{
		Task task = LoadAllSoundboardSounds();
		while (!task.IsCompleted)
		{
			yield return null;
		}
	}

	public static void EnableLoop()
	{
		loop = true;
	}

	public static void FixRecorder()
	{
		Variables.taggerInstance.myRecorder.SourceType = (Photon.Voice.Unity.Recorder.InputSourceType)0;
		Variables.taggerInstance.myRecorder.AudioClip = null;
		Variables.taggerInstance.myRecorder.RestartRecording(true);
		Variables.taggerInstance.myRecorder.LoopAudioClip = false;
		currentlyPlayingSound = null;
		isPlaying = false;
	}

	public static async Task<string> GetRawFrom(string url)
	{
		using (UnityWebRequest request = UnityWebRequest.Get(url))
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			request.SendWebRequest().completed += delegate
			{
				tcs.SetResult(result: true);
			};
			await tcs.Task;
			return ((int)request.result == 1) ? request.downloadHandler.text : null;
		}
	}

	public static void ReloadCustom_Sounds()
	{
		Custom.Clear();
		LoadCustom_Sounds();
		List<ButtonHandler.Button> list = new List<ButtonHandler.Button>();
		ButtonHandler.Button[] buttons = ModButtons.buttons;
		int num = 0;
		while (num < buttons.Length)
		{
			ButtonHandler.Button button = buttons[num];
			if (button.Page != Category.Custom_Sounds || !button.isToggle)
			{
				list.Add(button);
				num++;
			}
			else
			{
				num++;
			}
		}
		AddSoundButtons(list, Custom, Category.Custom_Sounds);
		ModButtons.buttons = list.ToArray();
		Main.RefreshMenu();
		int count = Custom.Count;
		NotificationLib.SendNotification(NotificationLib.NotificationType.Loaded, string.Format("Custom Sounds Reloaded `{0}` `{1}`", count, (count != 1) ? "s" : ""));
	}

	public static async Task LoadAllSoundboardSounds()
	{
		foreach (KeyValuePair<string, string> entry in soundboardUrls)
		{
			await LoadSoundboardSounds(entry.Value, entry.Key);
		}
		LoadCustom_Sounds();
	}

	public static void CleanupAllSounds()
	{
		foreach (string current in soundboardSoundsActive.Keys.ToList())
		{
			if (soundboardSoundsActive[current])
			{
				StopSound(current);
			}
		}
		foreach (KeyValuePair<string, AudioSource> current2 in loadedSounds.ToList())
		{
			if ((UnityEngine.Object)(object)current2.Value != (UnityEngine.Object)null)
			{
				current2.Value.Stop();
				AudioClip clip = current2.Value.clip;
				current2.Value.clip = null;
				if ((UnityEngine.Object)(object)((Component)current2.Value).gameObject != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)current2.Value).gameObject);
				}
				if ((UnityEngine.Object)(object)clip != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)clip);
				}
			}
		}
		loadedSounds.Clear();
		soundboardSoundsActive.Clear();
		activeCoroutines.Clear();
		currentAudioSource = null;
		currentlyPlayingSound = null;
		isPlaying = false;
	}

	public static void StopSound(string pathOrUrl)
	{
		if (!soundboardSoundsActive.ContainsKey(pathOrUrl) || !soundboardSoundsActive[pathOrUrl])
		{
			return;
		}
		soundboardSoundsActive[pathOrUrl] = false;
		if (activeCoroutines.TryGetValue(pathOrUrl, out Coroutine value))
		{
			((MonoBehaviour)CoroutineHelper.Instance).StopCoroutine(value);
			activeCoroutines.Remove(pathOrUrl);
		}
		if (loadedSounds.TryGetValue(pathOrUrl, out AudioSource value2))
		{
			if ((UnityEngine.Object)(object)value2 != (UnityEngine.Object)null)
			{
				value2.Stop();
				AudioClip clip = value2.clip;
				value2.clip = null;
				GameObject gameObject = ((Component)value2).gameObject;
				if ((UnityEngine.Object)(object)gameObject != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)gameObject);
				}
				if ((UnityEngine.Object)(object)clip != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)clip);
				}
			}
			loadedSounds.Remove(pathOrUrl);
		}
		if ((UnityEngine.Object)(object)currentAudioSource != (UnityEngine.Object)null && currentlyPlayingSound == pathOrUrl)
		{
			currentAudioSource = null;
		}
		FixRecorder();
	}

	public static IEnumerator PlaySoundCoroutine(AudioSource source, string pathOrUrl)
	{
		if (!source.isPlaying)
		{
			source.Play();
		}
		while (loop && soundboardSoundsActive.ContainsKey(pathOrUrl) && soundboardSoundsActive[pathOrUrl])
		{
			yield return (object)new WaitForSeconds(source.clip.length);
			if (loop)
			{
				source.Play();
			}
		}
		StopSound(pathOrUrl);
	}

	public static void PlaySound(string pathOrUrl)
	{
		if (!Settings.currentControllerInput())
		{
			isPlaying = false;
			return;
		}
		if (isPlaying)
		{
			return;
		}
		if (!PhotonNetwork.InRoom)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Must Be In A Room");
			return;
		}
		isPlaying = true;
		if (currentlyPlayingSound != null && currentlyPlayingSound != pathOrUrl)
		{
			StopSound(currentlyPlayingSound);
		}
		currentlyPlayingSound = pathOrUrl;
		soundboardSoundsActive[pathOrUrl] = true;
		if (loadedSounds.TryGetValue(pathOrUrl, out AudioSource value))
		{
			currentAudioSource = value;
			PlayAudioSource(value, pathOrUrl);
			return;
		}
		((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DownloadAndPlaySound(pathOrUrl));
	}

	private static AudioType GetAudioType(string path)
	{
		return (AudioType)(Path.GetExtension(path).ToLower() switch
		{
			".mp3" => 13,
			".wav" => 20,
			".ogg" => 14,
			_ => 13,
		});
	}

	public static async Task LoadSoundboardSounds(string url, string category)
	{
		string rawData = await GetRawFrom(url);
		if (string.IsNullOrEmpty(rawData))
		{
			UnityEngine.Debug.LogError((object)("Failed to fetch sounds for " + category));
			return;
		}
		Dictionary<string, string> targetDict = category switch
		{
			"SFX" => SFX,
			"Trolling" => Trolling,
			"Songs" => Songs,
			_ => null,
		};
		if (targetDict == null)
		{
			UnityEngine.Debug.LogError((object)("Invalid category: " + category));
			return;
		}
		string[] lines = rawData.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string line in lines)
		{
			string[] parts = line.Split(';');
			if (parts.Length == 2)
			{
				string name = parts[0].Trim();
				string soundUrl = parts[1].Trim();
				if (!targetDict.ContainsKey(name))
				{
					targetDict[name] = soundUrl;
				}
			}
		}
	}
}
