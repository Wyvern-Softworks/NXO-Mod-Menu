using System;
using System.Collections;
using System.Collections.Generic;
using GorillaNetworking;
using NXO.Menu;
using NXO.Mods.Categories;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NXO.Utilities;

public class CustomBoards : MonoBehaviour
{
	private struct JoinScreens
	{
		public JoinTriggerUITemplate Template;

		public Material A;

		public Material B;

		public Material C;

		public Material D;

		public Material E;

		public Material F;

		public Material G;

		public Material H;
	}

	private struct BoardConfig(string p, Vector3 pos, Vector3 rot, Vector3 s)
	{
		public readonly string Path = p;

		public readonly Vector3 Pos = pos;

		public readonly Vector3 Rot = rot;

		public readonly Vector3 Scale = s;
	}

	private const int TreeRoomBoardIndex = 3;

	private const int ForestBoardIndex = 6;

	private Material _mat;

	private Renderer _monitor;

	private readonly List<TextMeshPro> _texts = new List<TextMeshPro>(32);

	private readonly Dictionary<string, GameObject> _scenePlanes = new Dictionary<string, GameObject>();

	private readonly Dictionary<Renderer, Material> _originalMats = new Dictionary<Renderer, Material>();

	private readonly List<Renderer> _pinned = new List<Renderer>(8);

	private string _message = "Loading...";

	private bool _boardsDone;

	private float _nextFindBoardsAttempt;

	private bool _findBoardsErrorLogged;

	private bool _matApplied;

	public static bool boardsEnabled;

	private Material _cachedBoardPinwheelMat;

	private readonly List<JoinScreens> _joinScreens = new List<JoinScreens>();

	private bool _joinCached;

	private Texture2D _featuredTex;

	private SpriteRenderer _featuredSr;

	private Sprite _featuredSprite;

	private static readonly Dictionary<string, BoardConfig> SceneBoards = new Dictionary<string, BoardConfig>
	{
		["Canyon2"] = new BoardConfig("Canyon/CanyonScoreboardAnchor/GorillaScoreBoard", new Vector3(-24.5019f, -28.7746f, 0.1f), new Vector3(270f, 0f, 0f), new Vector3(21.5946f, 1f, 22.1782f)),
		["Skyjungle"] = new BoardConfig("skyjungle/UI/Scoreboard/GorillaScoreBoard", new Vector3(-21.2764f, -32.1928f, 0f), new Vector3(270.2987f, 0.2f, 359.9f), new Vector3(21.6f, 0.1f, 20.4909f)),
		["Mountain"] = new BoardConfig("Mountain/MountainScoreboardAnchor/GorillaScoreBoard", Vector3.zero, Vector3.zero, Vector3.one),
		["Metropolis"] = new BoardConfig("MetroMain/ComputerArea/Scoreboard/GorillaScoreBoard", new Vector3(-25.1f, -31f, 0.1502f), new Vector3(270.1958f, 0.2086f, 0f), new Vector3(21f, 102.9727f, 21.4f)),
		["Bayou"] = new BoardConfig("BayouMain/ComputerArea/GorillaScoreBoardPhysical", new Vector3(-28.3419f, -26.851f, 0.3f), new Vector3(270f, 0f, 0f), new Vector3(21.3636f, 38f, 21f)),
		["Beach"] = new BoardConfig("BeachScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -33.7126f, 0.1f), new Vector3(270.056f, 0f, 0f), new Vector3(21.2f, 2f, 21.6f)),
		["Cave"] = new BoardConfig("Cave_Main_Prefab/CrystalCaveScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -33.7126f, 0.1f), new Vector3(270.056f, 0f, 0f), new Vector3(21.2f, 2f, 21.6f)),
		["Rotating"] = new BoardConfig("RotatingPermanentEntrance/UI (1)/RotatingScoreboard/RotatingScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -33.7126f, 0.1f), new Vector3(270.056f, 0f, 0f), new Vector3(21.2f, 2f, 21.6f)),
		["MonkeBlocks"] = new BoardConfig("Environment Objects/MonkeBlocksRoomPersistent/AtticScoreBoard/AtticScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -24.5091f, 0.57f), new Vector3(270.1856f, 0.1f, 0f), new Vector3(21.6f, 1.2f, 20.8f)),
		["Basement"] = new BoardConfig("Basement/BasementScoreboardAnchor/GorillaScoreBoard/", new Vector3(-22.1964f, -24.5091f, 0.57f), new Vector3(270.1856f, 0.1f, 0f), new Vector3(21.6f, 1.2f, 20.8f)),
		["City"] = new BoardConfig("City_Pretty/CosmeticsScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -34.9f, 0.57f), new Vector3(270f, 0f, 0f), new Vector3(21.6f, 2.4f, 22f))
	};

	public static CustomBoards Instance { get; private set; }

	private IEnumerator CustomFeaturedMap()
	{
		GameObject obj = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/ModIOFeaturedMapsDisplay/FeaturedMapImage");
		_featuredSr = ((obj != null) ? obj.GetComponent<SpriteRenderer>() : null);
		if ((UnityEngine.Object)(object)_featuredSr == (UnityEngine.Object)null)
		{
			yield break;
		}
		Texture2D newTex;
		using (UnityWebRequest req = UnityWebRequestTexture.GetTexture("https://raw.githubusercontent.com/Wyvern-Softworks/NXO-Mod-Menu/refs/heads/main/Resources/NXO%20is%20sooooo%20awesome.png"))
		{
			yield return req.SendWebRequest();
			if ((int)req.result != 1)
			{
				yield break;
			}
			newTex = DownloadHandlerTexture.GetContent(req);
		}
		Sprite newSprite = Sprite.Create(newTex, new Rect(0f, 0f, (float)((Texture)newTex).width, (float)((Texture)newTex).height), new Vector2(0.5f, 0.5f), 100f);
		Texture2D oldTex = _featuredTex;
		Sprite oldSprite = _featuredSprite;
		_featuredTex = newTex;
		_featuredSprite = newSprite;
		if ((UnityEngine.Object)(object)oldSprite != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)oldSprite);
		}
		if ((UnityEngine.Object)(object)oldTex != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)oldTex);
		}
		_featuredSr.sprite = _featuredSprite;
		((Component)_featuredSr).gameObject.SetActive(true);
		GameObject loading = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/LoadingText");
		if ((UnityEngine.Object)(object)loading != (UnityEngine.Object)null)
		{
			loading.SetActive(false);
		}
		GameObject displayObject = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/ModIOFeaturedMapsDisplay");
		NewMapsDisplay display = ((displayObject != null) ? displayObject.GetComponent<NewMapsDisplay>() : null);
		if ((UnityEngine.Object)(object)display != (UnityEngine.Object)null && (UnityEngine.Object)(object)display.mapInfoTMP != (UnityEngine.Object)null)
		{
			((Component)display.mapInfoTMP).gameObject.SetActive(true);
			display.mapInfoTMP.text = "NXO ON TOP!";
		}
	}

	private void FindBoards()
	{
		GameObject treeRoom = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom");
		if ((UnityEngine.Object)(object)treeRoom == (UnityEngine.Object)null)
		{
			_nextFindBoardsAttempt = Time.time + 5f;
			return;
		}
		ApplyToNthTempFile(treeRoom.transform, 3);
		GameObject forest = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest");
		if ((UnityEngine.Object)(object)forest != (UnityEngine.Object)null)
		{
			ApplyToNthTempFile(forest.transform, 6);
		}
		PhotonNetworkController controller = PhotonNetworkController.Instance;
		if ((UnityEngine.Object)(object)controller != (UnityEngine.Object)null && ((PhotonNetworkController)controller).allJoinTriggers != null)
		{
			foreach (GorillaNetworkJoinTrigger trigger in ((PhotonNetworkController)controller).allJoinTriggers)
			{
				if (trigger == null || trigger.ui == null)
				{
					continue;
				}
				JoinTriggerUITemplate template = trigger.ui.template;
				if ((UnityEngine.Object)(object)template != (UnityEngine.Object)null && !_joinCached)
				{
					_joinScreens.Add(new JoinScreens
					{
						Template = template,
						A = template.ScreenBG_AbandonPartyAndSoloJoin,
						B = template.ScreenBG_AlreadyInRoom,
						C = template.ScreenBG_ChangingGameModeSoloJoin,
						D = template.ScreenBG_Error,
						E = template.ScreenBG_InPrivateRoom,
						F = template.ScreenBG_LeaveRoomAndGroupJoin,
						G = template.ScreenBG_LeaveRoomAndSoloJoin,
						H = template.ScreenBG_NotConnectedSoloJoin
					});
				}
				CacheText(trigger.ui.screenText);
			}
			_joinCached = true;
		}
		CacheText("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText");
		CacheText("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData");
		CacheText("Environment Objects/LocalObjects_Prefab/TreeRoom/Data");
		CacheText("Environment Objects/LocalObjects_Prefab/TreeRoom/FunctionSelect");
		GameObject forestBoard = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/ForestScoreboardAnchor/GorillaScoreBoard");
		Transform forestBoardTransform = ((forestBoard != null) ? forestBoard.transform : null);
		if ((UnityEngine.Object)(object)forestBoardTransform != (UnityEngine.Object)null)
		{
			for (int i = 0; i < forestBoardTransform.childCount; i++)
			{
				GameObject gameObject = ((Component)forestBoardTransform.GetChild(i)).gameObject;
				if (gameObject.activeSelf)
				{
					string childName = ((UnityEngine.Object)gameObject).name;
					if (childName.Contains("Board Text") || childName.Contains("Scoreboard_OfflineText"))
					{
						CacheText(gameObject.GetComponent<TextMeshPro>());
					}
				}
			}
		}
		GameObject monitor = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen");
		_monitor = ((monitor != null) ? monitor.GetComponent<Renderer>() : null);
		CacheMat(_monitor);
		_boardsDone = true;
		_findBoardsErrorLogged = false;
	}

	private void CacheMat(Renderer r)
	{
		if (!((UnityEngine.Object)(object)r == (UnityEngine.Object)null) && !_originalMats.ContainsKey(r))
		{
			_originalMats[r] = r.sharedMaterial;
			_pinned.Add(r);
		}
	}

	private void Update()
	{
		if (!boardsEnabled)
		{
			if (_matApplied)
			{
				foreach (KeyValuePair<Renderer, Material> originalMat in _originalMats)
				{
					if ((UnityEngine.Object)(object)originalMat.Key != (UnityEngine.Object)null)
					{
						originalMat.Key.sharedMaterial = originalMat.Value;
					}
				}
				RevertJoinScreens();
				foreach (GameObject plane in _scenePlanes.Values)
				{
					if ((UnityEngine.Object)(object)plane != (UnityEngine.Object)null)
					{
						plane.SetActive(false);
					}
				}
				_matApplied = false;
			}
			return;
		}
		if (!_boardsDone)
		{
			if (Time.time < _nextFindBoardsAttempt)
			{
				return;
			}
			try
			{
				FindBoards();
			}
			catch (Exception ex)
			{
				if (!_findBoardsErrorLogged)
				{
					_findBoardsErrorLogged = true;
					UnityEngine.Debug.LogWarning((object)("[NXO] CustomBoards.FindBoards failed; retrying later: " + ex));
				}
				_nextFindBoardsAttempt = Time.time + 5f;
				return;
			}
			if (!_boardsDone)
			{
				return;
			}
		}
		Material val;
		if (Settings.BoardsMode == Settings.ColorMode.Pinwheel)
		{
			if ((UnityEngine.Object)(object)_cachedBoardPinwheelMat == (UnityEngine.Object)null)
			{
				_cachedBoardPinwheelMat = Main.GetPinwheelMaterial();
			}
			else
			{
				AssetHandler.SetProperty(_cachedBoardPinwheelMat, "_Speed", 0f - Main.pinwheelSpeed);
				AssetHandler.SetProperty(_cachedBoardPinwheelMat, "_COLOR1", Main.pinwheelColor1);
				AssetHandler.SetProperty(_cachedBoardPinwheelMat, "_COLOR2", Main.pinwheelColor2);
			}
			val = _cachedBoardPinwheelMat;
		}
		else
		{
			if ((UnityEngine.Object)(object)_cachedBoardPinwheelMat != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)_cachedBoardPinwheelMat);
				_cachedBoardPinwheelMat = null;
			}
			Color animatedColor = Settings.GetAnimatedColor(Settings.BoardsMode, (Color32)(Settings.BoardsColor), (Color32)(Settings.BoardsColor2), Settings.BoardsAnimSpeed, 6);
			_mat.color = new Color(animatedColor.r * 0.75f, animatedColor.g * 0.75f, animatedColor.b * 0.75f, animatedColor.a);
			val = _mat;
		}
		for (int i = 0; i < _pinned.Count; i++)
		{
			Renderer renderer = _pinned[i];
			if ((UnityEngine.Object)(object)renderer != (UnityEngine.Object)null && (UnityEngine.Object)(object)renderer.sharedMaterial != (UnityEngine.Object)(object)val)
			{
				renderer.sharedMaterial = val;
			}
		}
		foreach (GameObject plane in _scenePlanes.Values)
		{
			if ((UnityEngine.Object)(object)plane != (UnityEngine.Object)null)
			{
				Renderer component = plane.GetComponent<Renderer>();
				if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null && (UnityEngine.Object)(object)component.sharedMaterial != (UnityEngine.Object)(object)val)
				{
					component.sharedMaterial = val;
				}
			}
		}
		foreach (JoinScreens joinScreen in _joinScreens)
		{
			if ((UnityEngine.Object)(object)joinScreen.Template != (UnityEngine.Object)null && (UnityEngine.Object)(object)joinScreen.Template.ScreenBG_AlreadyInRoom != (UnityEngine.Object)(object)val)
			{
				joinScreen.Template.ScreenBG_AbandonPartyAndSoloJoin = val;
				joinScreen.Template.ScreenBG_AlreadyInRoom = val;
				joinScreen.Template.ScreenBG_ChangingGameModeSoloJoin = val;
				joinScreen.Template.ScreenBG_Error = val;
				joinScreen.Template.ScreenBG_InPrivateRoom = val;
				joinScreen.Template.ScreenBG_LeaveRoomAndGroupJoin = val;
				joinScreen.Template.ScreenBG_LeaveRoomAndSoloJoin = val;
				joinScreen.Template.ScreenBG_NotConnectedSoloJoin = val;
				PhotonNetworkController instance = PhotonNetworkController.Instance;
				if (instance != null)
				{
					((PhotonNetworkController)instance).UpdateTriggerScreens();
				}
			}
		}
		if (!_matApplied)
		{
			foreach (GameObject plane2 in _scenePlanes.Values)
			{
				if ((UnityEngine.Object)(object)plane2 != (UnityEngine.Object)null)
				{
					plane2.SetActive(true);
				}
			}
			_matApplied = true;
		}
	}

	public static void SetBoardsEnabled(bool enabled)
	{
		boardsEnabled = enabled;
	}

	public static IEnumerator GetPlayersCoroutine(Action<int> callback)
	{
		callback?.Invoke(-1);
		yield break;
	}

	private void CacheText(string path)
	{
		GameObject obj = GameObject.Find(path);
		CacheText((obj != null) ? obj.GetComponent<TextMeshPro>() : null);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (!SceneBoards.TryGetValue(scene.name, out var value))
		{
			return;
		}
		GameObject val;
		if (_scenePlanes.TryGetValue(scene.name, out GameObject value2) && (UnityEngine.Object)(object)value2 != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)value2);
			val = GameObject.Find(value.Path);
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				return;
			}
		}
		else
		{
			val = GameObject.Find(value.Path);
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				return;
			}
		}
		GameObject val2 = GameObject.CreatePrimitive((PrimitiveType)4);
		val2.transform.SetParent(val.transform, false);
		val2.transform.localPosition = value.Pos;
		val2.transform.localRotation = Quaternion.Euler(value.Rot);
		val2.transform.localScale = value.Scale;
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)val2.GetComponent<Collider>());
		if (Settings.BoardsMode != Settings.ColorMode.Pinwheel || !((UnityEngine.Object)(object)_cachedBoardPinwheelMat != (UnityEngine.Object)null))
		{
			Material mat = _mat;
			val2.GetComponent<Renderer>().sharedMaterial = mat;
			val2.SetActive(boardsEnabled);
			_scenePlanes[scene.name] = val2;
		}
		else
		{
			Material mat = _cachedBoardPinwheelMat;
			val2.GetComponent<Renderer>().sharedMaterial = mat;
			val2.SetActive(boardsEnabled);
			_scenePlanes[scene.name] = val2;
		}
	}

	private void RevertJoinScreens()
	{
		if (_joinScreens.Count == 0)
		{
			return;
		}
		foreach (JoinScreens current in _joinScreens)
		{
			if ((UnityEngine.Object)(object)current.Template == (UnityEngine.Object)null)
			{
				continue;
			}
			current.Template.ScreenBG_AbandonPartyAndSoloJoin = current.A;
			current.Template.ScreenBG_AlreadyInRoom = current.B;
			current.Template.ScreenBG_ChangingGameModeSoloJoin = current.C;
			current.Template.ScreenBG_Error = current.D;
			current.Template.ScreenBG_InPrivateRoom = current.E;
			current.Template.ScreenBG_LeaveRoomAndGroupJoin = current.F;
			current.Template.ScreenBG_LeaveRoomAndSoloJoin = current.G;
			current.Template.ScreenBG_NotConnectedSoloJoin = current.H;
		}
		PhotonNetworkController instance = PhotonNetworkController.Instance;
		if (instance != null)
		{
			((PhotonNetworkController)instance).UpdateTriggerScreens();
		}
	}

	private IEnumerator FetchMotd()
	{
		for (;;)
		{
			using (UnityWebRequest req = UnityWebRequest.Get("https://raw.githubusercontent.com/Wyvern-Softworks/NXO-Mod-Menu/refs/heads/main/Resources/NXO-Menu-Status.txt"))
			{
				req.timeout = 5;
				yield return req.SendWebRequest();
				_message = (((int)req.result == 1) ? req.downloadHandler.text.Trim() : "THIS VERSION IS OUTDATED! CHECK https://github.com/Wyvern-Softworks/NXO-Mod-Menu FOR UPDATES.");
			}
			yield return new WaitForSeconds(2f);
		}
	}

	private void SetupCoCText()
	{
		GameObject obj = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText");
		TextMeshPro val = ((obj != null) ? obj.GetComponent<TextMeshPro>() : null);
		TextMeshPro body;
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			((TMP_Text)val).text = "NXO PAID 5.2";
			((Graphic)val).color = Color.white;
			((TMP_Text)val).richText = true;
			GameObject obj2 = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData");
			body = ((obj2 != null) ? obj2.GetComponent<TextMeshPro>() : null);
			if ((UnityEngine.Object)(object)body == (UnityEngine.Object)null)
			{
				return;
			}
		}
		else
		{
			GameObject obj3 = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData");
			body = ((obj3 != null) ? obj3.GetComponent<TextMeshPro>() : null);
			if ((UnityEngine.Object)(object)body == (UnityEngine.Object)null)
			{
				return;
			}
		}
		((Graphic)body).color = Color.white;
		((TMP_Text)body).richText = true;
		void UpdateBody(int playerCount)
		{
			if ((UnityEngine.Object)(object)body == (UnityEngine.Object)null)
			{
				return;
			}
			string text = (playerCount < 0) ? "" : $"NXO Users Online: {playerCount}\n\n";
			((TMP_Text)body).text = "THANK YOU FOR BUYING NXO PAID!\n\nNXO IS NOT RESPONSIBLE FOR ANY ACTIONS\nTAKEN AGAINST YOUR ACCOUNT.\n\n" + text + _message + "\n\nCREATED BY: NUGGET\nDEVELOPERS: CATLICKER AND LIEX\nhttps://github.com/Wyvern-Softworks/NXO-Mod-Menu";
		}
		PlayFabTitleDataTextDisplay component = ((Component)body).GetComponent<PlayFabTitleDataTextDisplay>();
		if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
		{
			((Behaviour)component).enabled = false;
			((MonoBehaviour)this).StartCoroutine(GetPlayersCoroutine(UpdateBody));
			((MonoBehaviour)this).StartCoroutine(CustomFeaturedMap());
			return;
		}
		((MonoBehaviour)this).StartCoroutine(GetPlayersCoroutine(UpdateBody));
		((MonoBehaviour)this).StartCoroutine(CustomFeaturedMap());
	}

	public static IEnumerator PingRoutine(Action<int> callback)
	{
		for (;;)
		{
			yield return GetPlayersCoroutine(callback);
			yield return new WaitForSeconds(60f);
		}
	}

	private void OnDestroy()
	{
		if ((UnityEngine.Object)(object)_mat != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_mat);
		}
		if ((UnityEngine.Object)(object)_cachedBoardPinwheelMat != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_cachedBoardPinwheelMat);
		}
		if ((UnityEngine.Object)(object)_featuredSprite != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_featuredSprite);
		}
		if ((UnityEngine.Object)(object)_featuredTex != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)_featuredTex);
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
		_mat = new Material(Shader.Find("GorillaTag/UberShader"));
	}

	private void ApplyJoinScreens()
	{
		if (_joinScreens.Count == 0)
		{
			return;
		}
		Material val = (Settings.BoardsMode == Settings.ColorMode.Pinwheel && (UnityEngine.Object)(object)_cachedBoardPinwheelMat != (UnityEngine.Object)null) ? _cachedBoardPinwheelMat : _mat;
		foreach (JoinScreens current in _joinScreens)
		{
			if ((UnityEngine.Object)(object)current.Template == (UnityEngine.Object)null)
			{
				continue;
			}
			current.Template.ScreenBG_AbandonPartyAndSoloJoin = val;
			current.Template.ScreenBG_AlreadyInRoom = val;
			current.Template.ScreenBG_ChangingGameModeSoloJoin = val;
			current.Template.ScreenBG_Error = val;
			current.Template.ScreenBG_InPrivateRoom = val;
			current.Template.ScreenBG_LeaveRoomAndGroupJoin = val;
			current.Template.ScreenBG_LeaveRoomAndSoloJoin = val;
			current.Template.ScreenBG_NotConnectedSoloJoin = val;
		}
		PhotonNetworkController instance = PhotonNetworkController.Instance;
		if (instance != null)
		{
			((PhotonNetworkController)instance).UpdateTriggerScreens();
		}
	}

	private void ApplyToNthTempFile(Transform parent, int n)
	{
		int num = 0;
		for (int i = 0; i < parent.childCount; i++)
		{
			if (!((UnityEngine.Object)parent.GetChild(i)).name.Contains("UnityTempFile"))
			{
				continue;
			}
			if (num++ == n)
			{
				Renderer component = ((Component)parent.GetChild(i)).GetComponent<Renderer>();
				if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
				{
					CacheMat(component);
				}
				break;
			}
		}
	}

	private void CacheText(TextMeshPro tmp)
	{
		if ((UnityEngine.Object)(object)tmp != (UnityEngine.Object)null && !_texts.Contains(tmp))
		{
			_texts.Add(tmp);
		}
	}

	private void OnSceneUnloaded(Scene scene)
	{
		if (_scenePlanes.TryGetValue(scene.name, out GameObject value))
		{
			if ((UnityEngine.Object)(object)value != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)value);
				_scenePlanes.Remove(scene.name);
			}
			else
			{
				_scenePlanes.Remove(scene.name);
			}
		}
	}
}
