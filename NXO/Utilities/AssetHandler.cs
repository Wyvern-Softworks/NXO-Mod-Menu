using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace NXO.Utilities;

public static class AssetHandler
{
	public static readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();

	private static readonly Dictionary<string, Material> _cachedMaterials = new Dictionary<string, Material>();

	private static readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

	public static Texture2D LoadEmbeddedTexture(string fileName)
	{
		if (_textureCache.TryGetValue(fileName, out Texture2D value))
		{
			return value;
		}
		Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName);
		if (manifestResourceStream == null)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		manifestResourceStream.CopyTo(memoryStream);
		byte[] array = memoryStream.ToArray();
		Texture2D val = new Texture2D(2, 2, (TextureFormat)4, false);
		if (!ImageConversion.LoadImage(val, array))
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)val);
			return null;
		}
		_textureCache[fileName] = val;
		return val;
	}

	public static Material LoadMaterial(string bundleName, string materialName)
	{
		string key = bundleName + "_" + materialName;
		if (_cachedMaterials.TryGetValue(key, out Material value) && (UnityEngine.Object)(object)value != (UnityEngine.Object)null)
		{
			return value;
		}
		AssetBundle val = LoadAssetBundle(bundleName);
		if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
		{
			return null;
		}
		Material val2 = null;
		string[] allAssetNames = val.GetAllAssetNames();
		int num = 0;
		while (num < allAssetNames.Length)
		{
			string text = allAssetNames[num];
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
			if (string.Equals(fileNameWithoutExtension, materialName, StringComparison.OrdinalIgnoreCase))
			{
				UnityEngine.Object val3 = val.LoadAsset(text);
				Material val4 = (Material)(object)((val3 is Material) ? val3 : null);
				if (val4 != null)
				{
					val2 = val4;
					break;
				}
				GameObject val5 = (GameObject)(object)((val3 is GameObject) ? val3 : null);
				if (val5 != null)
				{
					Renderer component = val5.GetComponent<Renderer>();
					if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null && (UnityEngine.Object)(object)component.sharedMaterial != (UnityEngine.Object)null)
					{
						val2 = component.sharedMaterial;
						break;
					}
					num++;
				}
				else
				{
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		if ((UnityEngine.Object)(object)val2 == (UnityEngine.Object)null)
		{
			string[] allAssetNames2 = val.GetAllAssetNames();
			foreach (string text2 in allAssetNames2)
			{
				UnityEngine.Object val6 = val.LoadAsset(text2);
				Material val7 = (Material)(object)((val6 is Material) ? val6 : null);
				if (val7 != null)
				{
					val2 = val7;
					break;
				}
				Shader val8 = (Shader)(object)((val6 is Shader) ? val6 : null);
				if (val8 != null)
				{
					val2 = new Material(val8);
					break;
				}
			}
		}
		if ((UnityEngine.Object)(object)val2 == (UnityEngine.Object)null)
		{
			UnityEngine.Debug.LogWarning((object)("[NXO] No usable material for '" + materialName + "' in bundle '" + bundleName + "'"));
			return null;
		}
		_cachedMaterials[key] = val2;
		return val2;
	}

	private static void TryDeleteTemp(string path)
	{
		if (!string.IsNullOrEmpty(path) && File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public static AssetBundle LoadAssetBundle(string resourceName)
	{
		if (_loadedBundles.TryGetValue(resourceName, out AssetBundle value))
		{
			return value;
		}
		Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
		if (manifestResourceStream == null)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		manifestResourceStream.CopyTo(memoryStream);
		byte[] array = memoryStream.ToArray();
		AssetBundle val = AssetBundle.LoadFromMemory(array);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			_loadedBundles[resourceName] = val;
			return val;
		}
		return val;
	}

	public static void CleanupAssets()
	{
		foreach (KeyValuePair<string, Texture2D> current in _textureCache)
		{
			if ((UnityEngine.Object)(object)current.Value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current.Value);
			}
		}
		_textureCache.Clear();
		foreach (KeyValuePair<string, Material> current2 in _cachedMaterials)
		{
			if ((UnityEngine.Object)(object)current2.Value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)current2.Value);
			}
		}
		_cachedMaterials.Clear();
		foreach (KeyValuePair<string, AssetBundle> current3 in _loadedBundles)
		{
			if ((UnityEngine.Object)(object)current3.Value != (UnityEngine.Object)null)
			{
				current3.Value.Unload(true);
			}
		}
		_loadedBundles.Clear();
	}

	public static void SetProperty(Material mat, string property, object value)
	{
		if ((UnityEngine.Object)(object)mat == (UnityEngine.Object)null || !mat.HasProperty(property))
		{
			return;
		}
		if (!(value is float num))
		{
			if (!(value is Color val))
			{
				if (!(value is int num2))
				{
					Texture val2 = (Texture)((value is Texture) ? value : null);
					if (val2 != null)
					{
						mat.SetTexture(property, val2);
					}
				}
				else
				{
					mat.SetInt(property, num2);
				}
			}
			else
			{
				mat.SetColor(property, val);
			}
		}
		else
		{
			mat.SetFloat(property, num);
		}
	}

	public static IEnumerator LoadEmbeddedAudioClip(string resourceName, Action<AudioClip> callback)
	{
		string ext = Path.GetExtension(resourceName)?.ToLowerInvariant();
		AudioType audioType = ext switch
		{
			".wav" => (AudioType)20,
			".mp3" => (AudioType)13,
			_ => (AudioType)0
		};
		if ((int)audioType == 0)
		{
			UnityEngine.Debug.LogWarning((object)("[NXO] Unsupported audio format: " + ext));
			yield break;
		}
		string tempPath = Path.Combine(Application.temporaryCachePath, $"{Guid.NewGuid()}_{Path.GetFileName(resourceName)}");
		bool writeSuccess = false;
		try
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			if (stream == null)
			{
				UnityEngine.Debug.LogWarning((object)("[NXO] Could not find embedded resource: " + resourceName));
			}
			else
			{
				using FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
				stream.CopyTo(fs);
				writeSuccess = true;
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogWarning((object)("[NXO] Failed writing embedded audio temp file: " + ex));
		}
		if (!writeSuccess)
		{
			TryDeleteTemp(tempPath);
			yield break;
		}
		UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(new Uri(tempPath).AbsoluteUri, audioType);
		try
		{
			((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;
			yield return www.SendWebRequest();
			if ((int)www.result == 1)
			{
				AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
				if ((UnityEngine.Object)(object)clip != (UnityEngine.Object)null && (int)clip.loadState == 2)
				{
					((UnityEngine.Object)clip).name = Path.GetFileNameWithoutExtension(resourceName);
					callback?.Invoke(clip);
				}
				else
				{
					UnityEngine.Debug.LogWarning((object)("[NXO] Clip loaded but data not ready: " + resourceName));
					if ((UnityEngine.Object)(object)clip != (UnityEngine.Object)null)
					{
						UnityEngine.Object.Destroy((UnityEngine.Object)(object)clip);
					}
				}
			}
			else
			{
				UnityEngine.Debug.LogWarning((object)("[NXO] Failed to load audio clip '" + resourceName + "': " + www.error));
			}
		}
		finally
		{
			www.Dispose();
			TryDeleteTemp(tempPath);
		}
	}

	public static IEnumerator LoadTextureFromUrl(GameObject target, string url)
	{
		if ((UnityEngine.Object)(object)target == (UnityEngine.Object)null || string.IsNullOrEmpty(url))
		{
			yield break;
		}
		using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
		yield return uwr.SendWebRequest();
		if ((int)uwr.result != 1 || (UnityEngine.Object)(object)target == (UnityEngine.Object)null)
		{
			yield break;
		}
		Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
		Renderer renderer = target.GetComponent<Renderer>();
		if ((UnityEngine.Object)(object)renderer == (UnityEngine.Object)null)
		{
			yield break;
		}
		if ((UnityEngine.Object)(object)renderer.material != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)renderer.material);
		}
		renderer.material = new Material(Variables.uiShader)
		{
			mainTexture = (Texture)(object)texture
		};
	}

	public static void PlaySound(GameObject target, AudioClip clip, float volume = 1f)
	{
		if (!((UnityEngine.Object)(object)target == (UnityEngine.Object)null) && !((UnityEngine.Object)(object)clip == (UnityEngine.Object)null))
		{
			AudioSource component = target.GetComponent<AudioSource>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				component = target.AddComponent<AudioSource>();
				((UnityEngine.Object)component).hideFlags = (HideFlags)61;
				component.clip = clip;
				component.volume = volume;
				component.loop = false;
				component.Play();
			}
			else
			{
				component.clip = clip;
				component.volume = volume;
				component.loop = false;
				component.Play();
			}
		}
	}
}
