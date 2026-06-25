using System;
using System.Collections.Generic;
using GorillaLocomotion;
using NXO.Menu;
using NXO.Mods;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace NXO.Utilities;

public class Variables
{
	public static GameObject menuObj;

	public static GameObject background;

	public static GameObject canvasObj;

	public static GameObject clickerObj;

	public static GameObject BackToStartButton;

	public static GameObject keyclickerObj1;

	public static GameObject keyclickerObj2;

	public static GameObject searchButton;

	public static GameObject TopMenuButton;

	public static Text title;

	private static Category _currentPage;

	public static int currentCategoryPage = 0;

	public static int ButtonsPerPage = 7;

	public static float ButtonSpacing = 0.09f;

	public static bool toggledisconnectButton = true;

	public static bool SearchFollow = true;

	public static bool rightHandedMenu = false;

	public static bool toggleNotifications = true;

	public static bool AutoSave;

	public static bool notificationSent = false;

	public static bool teamCheckedESP = false;

	public static bool SearchEnabled = false;

	public static bool PCMenuOpen = false;

	public static KeyCode PCMenuKey = (KeyCode)308;

	public static bool openMenu;

	public static bool menuOpen;

	public static bool InMenuCondition;

	public static bool InPcCondition;

	public static bool OpenAndCloseMenuSounds = true;

	public static bool NotificationSounds = true;

	public static AudioClip menuOpenSound;

	public static AudioClip menuCloseSound;

	public static AudioClip notificationSound;

	public static string folderName;

	public static GTPlayer playerInstance;

	public static GorillaTagger taggerInstance;

	public static GameObject thirdPersonCamera;

	public static GameObject shoulderCamera;

	public static GameObject TransformCam;

	public static GameObject cm;

	public static bool didThirdPerson = false;

	public static float rpcCooldown = 0f;

	public static float lastClearTime = 0f;

	public static int fps;

	public static float lastFPSTime = 0f;

	public static string nxouri = "https://nxo.lol/";

	private static readonly Dictionary<Type, Array> cache = new Dictionary<Type, Array>();

	public static readonly Dictionary<string, GameObject> cachedObjects = new Dictionary<string, GameObject>();

	public static Shader guiShader = Shader.Find("GUI/Text Shader");

	public static Shader uberShader = Shader.Find("GorillaTag/UberShader");

	public static Shader uiShader = Shader.Find("UI/Default");

	public static Shader shinyShader = Shader.Find("Universal Render Pipeline/Lit");

	private static int? noInvisLayers;

	public static Category currentPage
	{
		get
		{
			return _currentPage;
		}
		set
		{
			_currentPage = value;
			Main._lastPageDrawn = Category.Home;
			Main._lastDrawModVersion = -1;
		}
	}

	public static Color GetRainbowColor(float hue)
	{
		return Color.HSVToRGB(hue % 1f, 1f, 1f);
	}

	public static int NoInvisLayers()
	{
		int valueOrDefault = noInvisLayers.GetValueOrDefault();
		if (!noInvisLayers.HasValue)
		{
			valueOrDefault = ~((1 << LayerMask.NameToLayer("TransparentFX")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Zone")) | (1 << LayerMask.NameToLayer("Gorilla Trigger")) | (1 << LayerMask.NameToLayer("Gorilla Boundary")) | (1 << LayerMask.NameToLayer("GorillaCosmetics")) | (1 << LayerMask.NameToLayer("GorillaParticle")));
			noInvisLayers = valueOrDefault;
			return valueOrDefault;
		}
		return valueOrDefault;
	}

	public static bool GetGamemode(string gamemodeName)
	{
		return GorillaGameManager.instance.GameModeName().IndexOf(gamemodeName, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public static GameObject FindObject(string find)
	{
		if (cachedObjects.TryGetValue(find, out GameObject value) && (UnityEngine.Object)(object)value != (UnityEngine.Object)null)
		{
			return value;
		}
		cachedObjects.Remove(find);
		GameObject val = GameObject.Find(find);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			cachedObjects[find] = val;
			return val;
		}
		return val;
	}

	public static Color RandomColor()
	{
		return (Color32)(new Color32((byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), byte.MaxValue));
	}

	public static bool IsMaster()
	{
		return PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;
	}

	public static T[] GetAllType<T>(bool refresh = false) where T : UnityEngine.Object
	{
		int num = 6;
		int num2 = 6;
		int[] array = new int[12];
		array[6] = 1;
		array[7] = 2;
		array[8] = 3;
		array[10] = 4;
		array[9] = 5;
		array[11] = 6;
		Array value = default(Array);
		T[] array3 = default(T[]);
		T[] result = default(T[]);
		while (num2 != 0)
		{
			switch (array[num + 0])
			{
			default:
			{
				int num3 = (refresh ? 1 : 0) * 1 + 7;
				num = num3;
				continue;
			}
			case 2:
			{
				int num3 = ((!cache.TryGetValue(typeof(T), out value)) ? 1 : 0) * -1 + 10;
				num = num3;
				continue;
			}
			case 3:
			{
				int num3 = 1 * -1 + 10;
				num = num3;
				continue;
			}
			case 4:
				array3 = (T[])value;
				num = 11;
				continue;
			case 5:
			{
				cache.Remove(typeof(T));
				T[] array4 = UnityEngine.Object.FindObjectsOfType<T>(true);
				cache[typeof(T)] = array4;
				array3 = array4;
				num = 11;
				continue;
			}
			case 6:
			{
				T[] array2 = array3;
				num2 = 0;
				result = array2;
				continue;
			}
			case 0:
				break;
			}
			break;
		}
		return result;
	}

	public static string GetHTMode()
	{
		Photon.Realtime.Room currentRoom;
		do
		{
			currentRoom = PhotonNetwork.CurrentRoom;
		}
		while (currentRoom == null);
		if (((RoomInfo)currentRoom).CustomProperties == null || !((Dictionary<object, object>)(object)((RoomInfo)currentRoom).CustomProperties).ContainsKey((object)"gameMode"))
		{
			return "ERROR";
		}
		return ((RoomInfo)currentRoom).CustomProperties[(object)"gameMode"].ToString();
	}

	public static Vector3 RandomVector3(float range = 1f)
	{
		return UnityEngine.Random.insideUnitSphere * range;
	}

	public static Quaternion RandomQuaternion()
	{
		return UnityEngine.Random.rotationUniform;
	}
}
