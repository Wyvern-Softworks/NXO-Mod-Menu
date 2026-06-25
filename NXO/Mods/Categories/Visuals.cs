using System;
using System.Collections.Generic;
using System.Linq;
using GorillaLocomotion;
using GorillaTag.Rendering;
using NXO.Menu;
using UnityEngine;

namespace NXO.Mods.Categories;

public class Visuals
{
	private class ESPData
	{
		public Dictionary<VRRig, GameObject> singleObjects = new Dictionary<VRRig, GameObject>();

		public Dictionary<VRRig, List<GameObject>> multiObjects = new Dictionary<VRRig, List<GameObject>>();

		public Material material;

		public void Clear()
		{
			foreach (GameObject current in singleObjects.Values)
			{
				if ((UnityEngine.Object)(object)current != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)current);
				}
			}
			singleObjects.Clear();
			foreach (List<GameObject> current2 in multiObjects.Values)
			{
				foreach (GameObject current3 in current2)
				{
					if ((UnityEngine.Object)(object)current3 != (UnityEngine.Object)null)
					{
						UnityEngine.Object.Destroy((UnityEngine.Object)(object)current3);
					}
				}
			}
			multiObjects.Clear();
			if ((UnityEngine.Object)(object)material != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)material);
				material = null;
			}
		}
	}

	private static readonly List<VRRig> fixedMeshRigs = new List<VRRig>();

	private static ESPData tracerData = new ESPData();

	private static ESPData wireframeData = new ESPData();

	private static ESPData twoDBoxData = new ESPData();

	private static ESPData boneData = new ESPData();

	private static ESPData beaconData = new ESPData();

	private static ESPData trailData = new ESPData();

	private static Dictionary<VRRig, GameObject> nametags = new Dictionary<VRRig, GameObject>();

	private static readonly int[][] edgeIndices = new int[12][]
	{
		new int[2] { 0, 1 },
		new int[2] { 1, 2 },
		new int[2] { 2, 3 },
		new int[2] { 3, 0 },
		new int[2] { 4, 5 },
		new int[2] { 5, 6 },
		new int[2] { 6, 7 },
		new int[2] { 7, 4 },
		new int[2] { 0, 4 },
		new int[2] { 1, 5 },
		new int[2] { 2, 6 },
		new int[2] { 3, 7 }
	};

	public static int[] bones = new int[38]
	{
		4, 3, 5, 4, 19, 18, 20, 19, 3, 18,
		21, 20, 22, 21, 25, 21, 29, 21, 31, 29,
		27, 25, 24, 22, 6, 5, 7, 6, 10, 6,
		14, 6, 16, 14, 12, 10, 9, 7
	};

	public static int TimeType = 0;

	public static bool lastState = false;

	private static bool acidApplied = false;

	private static readonly Dictionary<Renderer, Material> _acidOriginalMaterials = new Dictionary<Renderer, Material>();

	private static ESPData predictionData = new ESPData();

	private static Dictionary<VRRig, Vector3> lastPlayerPositions = new Dictionary<VRRig, Vector3>();

	private static Dictionary<VRRig, Vector3> smoothedVelocities = new Dictionary<VRRig, Vector3>();

	private static ESPData filledBoxData = new ESPData();

	private static ESPData filledBox2DData = new ESPData();

	private static Dictionary<VRRig, Material> originalRigMaterials = new Dictionary<VRRig, Material>();

	private static bool trippyMonkesApplied = false;

	public static GameObject nxoCamera;

	private static Vector3 CameraVelocity;

	private static bool _camLast = false;

	private static float lastFrame;

	private static readonly List<Renderer> hiddenRenderers = new List<Renderer>();

	private static List<GameObject> monkeSenseVignette = new List<GameObject>();

	private static AudioSource monkeSenseAudio = null;

	private static float monkeSenseBeepCooldown = 0f;

	private const float SENSE_FAR = 15f;

	private const float SENSE_CLOSE = 8f;

	private const float SENSE_DANGER = 3f;

	public static bool muteMonkeSense = false;

	public static void TrippyMonkes(bool enable)
	{
		if (!enable)
		{
			if (!trippyMonkesApplied)
			{
				return;
			}
			foreach (KeyValuePair<VRRig, Material> current in originalRigMaterials)
			{
				VRRig key = current.Key;
				Material value = current.Value;
				if ((UnityEngine.Object)(object)key != (UnityEngine.Object)null && (UnityEngine.Object)(object)key.mainSkin != (UnityEngine.Object)null && (UnityEngine.Object)(object)value != (UnityEngine.Object)null)
				{
					((Renderer)key.mainSkin).material = value;
				}
			}
			originalRigMaterials.Clear();
			trippyMonkesApplied = false;
		}
		else
		{
			if (trippyMonkesApplied)
			{
				return;
			}
			Material val = AssetHandler.LoadMaterial("NXO.Resources.acidtrip", "AcidTrip");
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				return;
			}
			foreach (VRRig current2 in VRRigCache.ActiveRigs)
			{
				if ((UnityEngine.Object)(object)current2 == (UnityEngine.Object)null || (UnityEngine.Object)(object)current2.mainSkin == (UnityEngine.Object)null || (UnityEngine.Object)(object)((Renderer)current2.mainSkin).material == (UnityEngine.Object)null || current2.isOfflineVRRig || (UnityEngine.Object)(object)current2 == (UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig)
				{
					continue;
				}
				if (!originalRigMaterials.ContainsKey(current2))
				{
					originalRigMaterials[current2] = ((Renderer)current2.mainSkin).material;
				}
				((Renderer)current2.mainSkin).material = val;
			}
			trippyMonkesApplied = true;
		}
	}

	public static void BeaconsESP(bool enable)
	{
		if (!enable)
		{
			beaconData.Clear();
			return;
		}
		beaconData.material = EnsureMaterial(ref beaconData.material, Variables.guiShader);
		List<VRRig> list = VRRigCache.ActiveRigs.ToList();
		CleanupInactiveRigs(beaconData, list);
		foreach (VRRig current in list)
		{
			if (current.isOfflineVRRig || current.isMyPlayer)
			{
				continue;
			}
			LineRenderer component;
			if (!beaconData.singleObjects.TryGetValue(current, out GameObject value) || (UnityEngine.Object)(object)value == (UnityEngine.Object)null)
			{
				value = new GameObject("Beacon");
				LineRenderer val = value.AddComponent<LineRenderer>();
				val.positionCount = 2;
				float startWidth = (val.endWidth = 0.15f);
				val.startWidth = startWidth;
				val.useWorldSpace = true;
				((Renderer)val).material = beaconData.material;
				beaconData.singleObjects[current] = value;
			}
			component = value.GetComponent<LineRenderer>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				continue;
			}
			Vector3 position = ((Component)current).transform.position;
			Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current);
			LineRenderer obj = component;
			Color startColor = (component.endColor = (Color32)(teamColor));
			obj.startColor = startColor;
			component.SetPositions((Vector3[])(object)new Vector3[2]
			{
				position + Vector3.down * 50f,
				position + Vector3.up * 50f
			});
		}
	}

	public static void TrailsESP(bool enable)
	{
		if (!enable)
		{
			trailData.Clear();
			return;
		}
		trailData.material = EnsureMaterial(ref trailData.material, Variables.guiShader);
		List<VRRig> list = VRRigCache.ActiveRigs.ToList();
		CleanupInactiveRigs(trailData, list);
		foreach (VRRig current in list)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig)
			{
				continue;
			}
			if (!trailData.singleObjects.TryGetValue(current, out GameObject value))
			{
				Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current);
				value = new GameObject("Trail");
				value.transform.position = ((Component)current).transform.position;
				value.transform.SetParent(((Component)current).transform);
				TrailRenderer val = value.AddComponent<TrailRenderer>();
				val.time = 2f;
				val.startWidth = 0.2f;
				val.endWidth = 0f;
				((Renderer)val).material = trailData.material;
				Color startColor = (val.endColor = (Color32)(teamColor));
				val.startColor = startColor;
				trailData.singleObjects[current] = value;
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)value, val.time);
			}
			else
			{
				value.transform.position = ((Component)current).transform.position;
			}
		}
	}

	public static void AcidTrip(bool enable)
	{
		if (!enable)
		{
			if (!acidApplied)
			{
				return;
			}
			foreach (KeyValuePair<Renderer, Material> current in _acidOriginalMaterials)
			{
				if ((UnityEngine.Object)(object)current.Key != (UnityEngine.Object)null)
				{
					current.Key.sharedMaterial = current.Value;
				}
			}
			_acidOriginalMaterials.Clear();
			acidApplied = false;
		}
		else
		{
			if (acidApplied)
			{
				return;
			}
			Material val = AssetHandler.LoadMaterial("NXO.Resources.acidtrip", "AcidTrip");
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				return;
			}
			Renderer[] array = UnityEngine.Object.FindObjectsOfType<Renderer>();
			int num = 0;
			while (num < array.Length)
			{
				Renderer val2 = array[num];
				if (!((UnityEngine.Object)(object)val2 == (UnityEngine.Object)null) && val2.enabled && ((Component)val2).gameObject.activeSelf)
				{
					_acidOriginalMaterials[val2] = val2.sharedMaterial;
					val2.sharedMaterial = val;
					num++;
				}
				else
				{
					num++;
				}
			}
			acidApplied = true;
		}
	}

	public static void Freecam()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.LPrimary, ref InputHandler.isOn, ref _camLast);
		Vector3 val2 = default(Vector3);
		if (!InputHandler.isOn)
		{
			DisableNXOCamera();
		}
		else if ((UnityEngine.Object)(object)nxoCamera == (UnityEngine.Object)null)
		{
			nxoCamera = new GameObject("NXO_Cam");
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			Vector2 joystickAxis = InputHandler.GetJoystickAxis(left: true);
			Vector2 joystickAxis2 = InputHandler.GetJoystickAxis(left: false);
			val2 = new Vector3(joystickAxis.x, joystickAxis2.y, joystickAxis.y);
			Vector3 val3 = GTVector3Extensions.X_Z(((Component)GTPlayer.Instance.bodyCollider).transform.forward);
			Vector3 val4 = GTVector3Extensions.X_Z(((Component)GTPlayer.Instance.bodyCollider).transform.right);
			Vector3 val5 = val2.x * val4 + val2.y * Vector3.up + val2.z * val3;
			Vector3 val6 = val5;
			val5 = val6 * Settings.FlySpeed;
			val6 = val5;
			CameraVelocity = Vector3.Lerp(CameraVelocity, val6, 0.12875f);
			Transform transform = nxoCamera.transform;
			transform.position += CameraVelocity * Time.unscaledDeltaTime;
			nxoCamera.transform.rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation;
		}
		else
		{
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			Vector2 joystickAxis = InputHandler.GetJoystickAxis(left: true);
			Vector2 joystickAxis2 = InputHandler.GetJoystickAxis(left: false);
			val2 = new Vector3(joystickAxis.x, joystickAxis2.y, joystickAxis.y);
			Vector3 val3 = GTVector3Extensions.X_Z(((Component)GTPlayer.Instance.bodyCollider).transform.forward);
			Vector3 val4 = GTVector3Extensions.X_Z(((Component)GTPlayer.Instance.bodyCollider).transform.right);
			Vector3 val7 = val2.x * val4 + val2.y * Vector3.up + val2.z * val3;
			Vector3 val6 = val7;
			val7 = val6 * Settings.FlySpeed;
			val6 = val7;
			CameraVelocity = Vector3.Lerp(CameraVelocity, val6, 0.12875f);
			Transform transform2 = nxoCamera.transform;
			transform2.position += CameraVelocity * Time.unscaledDeltaTime;
			nxoCamera.transform.rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation;
		}
	}

	public static void ShinySelf(bool enable)
	{
		Material material = ((Renderer)Variables.taggerInstance.offlineVRRig.mainSkin).material;
		if (enable)
		{
			material.shader = Variables.shinyShader;
			material.SetFloat("_Smoothness", 0.95f);
			material.SetFloat("_Metallic", 0.85f);
		}
		else
		{
			material.shader = Variables.uberShader;
		}
	}

	public static void FuckLights(bool enable)
	{
		((BetterDayNightManager)BetterDayNightManager.instance).AnimateLightFlash(2, (float)((!enable) ? 2 : 0), (float)((!enable) ? 2 : 0), 2f);
	}

	public static void ESP2D(bool enable)
	{
		if (!enable)
		{
			twoDBoxData.Clear();
			return;
		}
		twoDBoxData.material = EnsureMaterial(ref twoDBoxData.material, Variables.guiShader);
		List<VRRig> list = VRRigCache.ActiveRigs.ToList();
		CleanupInactiveRigs(twoDBoxData, list);
		foreach (VRRig current in list)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			if (!twoDBoxData.multiObjects.TryGetValue(current, out List<GameObject> value))
			{
				value = new List<GameObject>();
				int num = 0;
				if (num < 4)
				{
					do
					{
						GameObject val = new GameObject("2DBoxLine");
						LineRenderer val2 = val.AddComponent<LineRenderer>();
						val2.positionCount = 2;
						float startWidth = (val2.endWidth = 0.07f);
						val2.startWidth = startWidth;
						val2.useWorldSpace = true;
						((Renderer)val2).material = twoDBoxData.material;
						value.Add(val);
						num++;
					}
					while (num < 4);
				}
				twoDBoxData.multiObjects[current] = value;
			}
			Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current);
			Vector3 position = ((Component)current).transform.position;
			Quaternion val3 = Quaternion.LookRotation(position - ((Component)Camera.main).transform.position);
			float num3 = 0.03f;
			Vector3[] array = (Vector3[])(object)new Vector3[4]
			{
				position + val3 * new Vector3(-0.5f - num3, 0.5f + num3, 0f),
				position + val3 * new Vector3(0.5f + num3, 0.5f + num3, 0f),
				position + val3 * new Vector3(0.5f + num3, -0.5f - num3, 0f),
				position + val3 * new Vector3(-0.5f - num3, -0.5f - num3, 0f)
			};
			for (int num4 = 0; num4 < value.Count; num4++)
			{
				LineRenderer component = value[num4].GetComponent<LineRenderer>();
				if (!((UnityEngine.Object)(object)component == (UnityEngine.Object)null))
				{
					Color startColor = (component.endColor = (Color32)(teamColor));
					component.startColor = startColor;
					Vector3 val5 = array[(num4 + 1) % 4] - array[num4];
					Vector3 normalized = val5.normalized;
					component.SetPositions((Vector3[])(object)new Vector3[2]
					{
						array[num4] - normalized * num3,
						array[(num4 + 1) % 4] + normalized * num3
					});
				}
			}
		}
	}

	public static void ShinyMonkes(bool enable)
	{
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			SkinnedMeshRenderer mainSkin = current.mainSkin;
			if ((UnityEngine.Object)(object)((mainSkin != null) ? ((Renderer)mainSkin).material : null) == (UnityEngine.Object)null)
			{
				continue;
			}
			Material material = ((Renderer)current.mainSkin).material;
			if (enable)
			{
				material.shader = Variables.shinyShader;
				material.SetFloat("_Smoothness", 0.95f);
				material.SetFloat("_Metallic", 0.85f);
			}
			else
			{
				material.shader = Variables.uberShader;
			}
		}
	}

	public static void CapFPS(int fps)
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = Mathf.Clamp(fps, 15, 360);
		lastFrame = Time.realtimeSinceStartup;
	}

	public static void PredictionESP(bool enable)
	{
		if (!enable)
		{
			predictionData.Clear();
			lastPlayerPositions.Clear();
			smoothedVelocities.Clear();
			return;
		}
		predictionData.material = EnsureMaterial(ref predictionData.material, Variables.guiShader);
		List<VRRig> activeRigs = VRRigCache.ActiveRigs.ToList();
		CleanupInactiveRigs(predictionData, activeRigs);
		List<VRRig> list = lastPlayerPositions.Keys.Where((VRRig rig) => (UnityEngine.Object)(object)rig == (UnityEngine.Object)null || !activeRigs.Contains(rig)).ToList();
		foreach (VRRig current in list)
		{
			lastPlayerPositions.Remove(current);
			smoothedVelocities.Remove(current);
		}
		foreach (VRRig current2 in activeRigs)
		{
			if (current2.isMyPlayer || current2.isOfflineVRRig)
			{
				continue;
			}
			Vector3 position = ((Component)current2).transform.position;
			Vector3 val = Vector3.zero;
			if (lastPlayerPositions.TryGetValue(current2, out var value))
			{
				Vector3 val2 = (position - value) / Time.deltaTime;
				if (val2.magnitude > 30f)
				{
					val2 = val2.normalized * 30f;
				}
				if (smoothedVelocities.TryGetValue(current2, out Vector3 value2))
				{
					val = Vector3.Lerp(value2, val2, 0.3f);
				}
				else
				{
					val = val2;
				}
				smoothedVelocities[current2] = val;
			}
			lastPlayerPositions[current2] = position;
			if (val.magnitude < 0.3f)
			{
				continue;
			}
			if (!predictionData.singleObjects.TryGetValue(current2, out GameObject value3) || (UnityEngine.Object)(object)value3 == (UnityEngine.Object)null)
			{
				value3 = new GameObject("PredictionLine");
				LineRenderer val3 = value3.AddComponent<LineRenderer>();
				val3.positionCount = 2;
				val3.startWidth = 0.02f;
				val3.endWidth = 0.08f;
				val3.useWorldSpace = true;
				((Renderer)val3).material = predictionData.material;
				predictionData.singleObjects[current2] = value3;
			}
			LineRenderer component = value3.GetComponent<LineRenderer>();
			Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current2), Variables.teamCheckedESP, current2);
			component.startColor = new Color((float)(int)teamColor.r / 255f, (float)(int)teamColor.g / 255f, (float)(int)teamColor.b / 255f, 0.3f);
			component.endColor = new Color((float)(int)teamColor.r / 255f, (float)(int)teamColor.g / 255f, (float)(int)teamColor.b / 255f, 0.8f);
			float num = 0.25f;
			Vector3 val4 = position + val * num;
			if (val.y > 0f)
			{
				val4 += Physics.gravity * (num * num * 0.5f);
			}
			component.SetPosition(0, position);
			component.SetPosition(1, val4);
		}
	}

	public static void ESP3D(bool enable)
	{
		if (!enable)
		{
			wireframeData.Clear();
			return;
		}
		wireframeData.material = EnsureMaterial(ref wireframeData.material, Variables.guiShader);
		List<VRRig> list = VRRigCache.ActiveRigs.ToList();
		CleanupInactiveRigs(wireframeData, list);
		foreach (VRRig current in list)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			if (!wireframeData.multiObjects.TryGetValue(current, out List<GameObject> value))
			{
				value = new List<GameObject>();
				int[][] array = edgeIndices;
				for (int i = 0; i < array.Length; i++)
				{
					_ = array[i];
					GameObject val = new GameObject("WireframeLine");
					LineRenderer val2 = val.AddComponent<LineRenderer>();
					val2.positionCount = 2;
					float startWidth = (val2.endWidth = 0.06f);
					val2.startWidth = startWidth;
					val2.useWorldSpace = true;
					((Renderer)val2).material = wireframeData.material;
					value.Add(val);
				}
				wireframeData.multiObjects[current] = value;
			}
			Vector3 halfSize = new Vector3(((Component)current).transform.localScale.x * 0.375f, ((Component)current).transform.localScale.y * 0.525f, ((Component)current).transform.localScale.z * 0.375f);
			Vector3 center = ((Component)current).transform.position - new Vector3(0f, 0.075f, 0f);
			Vector3[] array2 = CalculateCorners(center, halfSize, ((Component)current).transform.rotation);
			Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current);
			for (int num2 = 0; num2 < value.Count; num2++)
			{
				LineRenderer component = value[num2].GetComponent<LineRenderer>();
				if (!((UnityEngine.Object)(object)component == (UnityEngine.Object)null))
				{
					Color startColor = (component.endColor = (Color32)(teamColor));
					component.startColor = startColor;
					component.SetPositions((Vector3[])(object)new Vector3[2]
					{
						array2[edgeIndices[num2][0]],
						array2[edgeIndices[num2][1]]
					});
				}
			}
		}
	}

	public static void MonkeSense(bool enable)
	{
		bool flag = RigManager.RigIsInfected(VRRig.LocalRig);
		if (!enable)
		{
			foreach (GameObject current in monkeSenseVignette)
			{
				if ((UnityEngine.Object)(object)current != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)current);
				}
			}
			monkeSenseVignette.Clear();
			if ((UnityEngine.Object)(object)monkeSenseAudio != (UnityEngine.Object)null)
			{
				monkeSenseAudio.Stop();
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)monkeSenseAudio).gameObject);
				monkeSenseAudio = null;
			}
			return;
		}
		List<VRRig> list = VRRigCache.ActiveRigs.ToList();
		List<(VRRig, float)> list2 = new List<(VRRig, float)>();
		float num = float.MaxValue;
		foreach (VRRig current2 in list)
		{
			if (current2.isOfflineVRRig)
			{
				continue;
			}
			bool flag2 = RigManager.RigIsInfected(current2);
			if ((!flag && !flag2) || (flag && flag2))
			{
				continue;
			}
			float num2 = Vector3.Distance(((Component)Variables.taggerInstance.bodyCollider).transform.position, ((Component)current2).transform.position);
			if (num2 <= 15f)
			{
				list2.Add((current2, num2));
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		int num3 = 8;
		Transform transform = ((Component)Camera.main).transform;
		if (monkeSenseVignette.Count < num3)
		{
			do
			{
				GameObject val = new GameObject("MonkeSenseArrow");
				LineRenderer val2 = val.AddComponent<LineRenderer>();
				val2.positionCount = 2;
				val2.useWorldSpace = true;
				((Renderer)val2).material = new Material(Variables.guiShader);
				monkeSenseVignette.Add(val);
			}
			while (monkeSenseVignette.Count < num3);
		}
		float[] array = new float[num3];
		foreach ((VRRig, float) current3 in list2)
		{
			VRRig item = current3.Item1;
			float item2 = current3.Item2;
			Vector3 val3 = ((Component)item).transform.position - transform.position;
			val3.y = 0f;
			Vector3 forward = transform.forward;
			forward.y = 0f;
			float num4 = 0f - Vector3.SignedAngle(forward.normalized, val3.normalized, Vector3.up);
			float num5 = 1f - Mathf.Clamp01((item2 - 3f) / 12f);
			float num6 = Mathf.Lerp(0.15f, 1f, num5);
			for (int num7 = 0; num7 < num3; num7++)
			{
				float num8 = 360f / (float)num3 * (float)num7;
				float num9 = Mathf.Abs(Mathf.DeltaAngle(num8, num4));
				if (num9 < 360f / (float)num3)
				{
					float num10 = Mathf.Lerp(num6, 0f, num9 / (360f / (float)num3));
					if (num10 > array[num7])
					{
						array[num7] = num10;
					}
				}
			}
		}
		Color val4 = flag ? Color.green : Color.red;
		for (int num11 = 0; num11 < num3; num11++)
		{
			float num12 = (360f / (float)num3 * (float)num11 + 90f) * (MathF.PI / 180f);
			Vector3 val5 = transform.right * Mathf.Cos(num12) + transform.up * Mathf.Sin(num12);
			Vector3 val6 = transform.right * Mathf.Cos(num12 + MathF.PI / 2f) + transform.up * Mathf.Sin(num12 + MathF.PI / 2f);
			LineRenderer component = monkeSenseVignette[num11].GetComponent<LineRenderer>();
			bool flag3 = array[num11] > 0f;
			int num13 = (component.positionCount = 20);
			float num15 = 6f;
			float num16 = flag3 ? Mathf.Lerp(0.01f, 0.025f, array[num11]) : 0.008f;
			float num17 = flag3 ? 4f : 1.5f;
			for (int num18 = 0; num18 < num13; num18++)
			{
				float num19 = (float)num18 / (float)(num13 - 1);
				Vector3 val7 = transform.position + transform.forward * 0.6f + val5 * Mathf.Lerp(0.25f, 0.55f, num19);
				float num20 = Mathf.Sin(num19 * num15 * MathF.PI + Time.time * num17) * num16 * Mathf.Sin(num19 * MathF.PI);
				component.SetPosition(num18, val7 + val6 * num20);
			}
			if (!flag3)
			{
				component.startColor = new Color(val4.r, val4.g, val4.b, array[num11]);
				component.endColor = new Color(val4.r, val4.g, val4.b, 0f);
			}
			else
			{
				component.startColor = new Color(1f, 1f, 1f, 0.4f);
				component.endColor = new Color(1f, 1f, 1f, 0f);
			}
			monkeSenseVignette[num11].SetActive(true);
		}
		if (list2.Count == 0)
		{
			foreach (GameObject current4 in monkeSenseVignette)
			{
				if ((UnityEngine.Object)(object)current4 != (UnityEngine.Object)null)
				{
					current4.SetActive(false);
				}
			}
			if ((UnityEngine.Object)(object)monkeSenseAudio != (UnityEngine.Object)null)
			{
				monkeSenseAudio.Stop();
			}
			return;
		}
		float num21;
		if (num > 3f)
		{
			if (num > 8f)
			{
				num21 = 1.2f;
				if (muteMonkeSense)
				{
					return;
				}
			}
			else
			{
				num21 = 0.4f;
				if (muteMonkeSense)
				{
					return;
				}
			}
		}
		else
		{
			num21 = 0.15f;
			if (muteMonkeSense)
			{
				return;
			}
		}
		if (!(Time.time >= monkeSenseBeepCooldown))
		{
			return;
		}
		monkeSenseBeepCooldown = Time.time + num21;
		if ((UnityEngine.Object)(object)monkeSenseAudio == (UnityEngine.Object)null)
		{
			GameObject val8 = new GameObject("MonkeSenseAudio");
			monkeSenseAudio = val8.AddComponent<AudioSource>();
			monkeSenseAudio.spatialBlend = 0f;
			monkeSenseAudio.volume = 0.6f;
		}
		int num22 = 44100;
		int num23 = num22 / 10;
		float num24 = (num <= 3f) ? 880f : ((num > 8f) ? 440f : 660f);
		AudioClip val9 = AudioClip.Create("SenseBeep", num23, 1, num22, false);
		float[] array2 = new float[num23];
		for (int num25 = 0; num25 < num23; num25++)
		{
			float num26 = (float)num25 / (float)num22;
			float num27 = 1f - (float)num25 / (float)num23;
			array2[num25] = Mathf.Sin(MathF.PI * 2f * num24 * num26) * num27 * 0.5f;
		}
		val9.SetData(array2, 0);
		monkeSenseAudio.PlayOneShot(val9);
	}

	public static void TracersESP(bool enable)
	{
		if (!enable)
		{
			tracerData.Clear();
			return;
		}
		tracerData.material = EnsureMaterial(ref tracerData.material, Variables.guiShader);
		List<VRRig> list = VRRigCache.ActiveRigs.ToList();
		CleanupInactiveRigs(tracerData, list);
		foreach (VRRig current in list)
		{
			if (current.isMyPlayer || current.isOfflineVRRig)
			{
				continue;
			}
			if (!tracerData.singleObjects.TryGetValue(current, out GameObject value) || (UnityEngine.Object)(object)value == (UnityEngine.Object)null)
			{
				value = new GameObject("Tracer");
				LineRenderer val = value.AddComponent<LineRenderer>();
				val.positionCount = 2;
				float startWidth = (val.endWidth = 0.015f);
				val.startWidth = startWidth;
				val.useWorldSpace = true;
				((Renderer)val).material = tracerData.material;
				tracerData.singleObjects[current] = value;
			}
			LineRenderer component = value.GetComponent<LineRenderer>();
			Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current);
			LineRenderer obj = component;
			Color startColor = (component.endColor = (Color32)(teamColor));
			obj.startColor = startColor;
			int num2 = (int)(Settings.tracerPositionIndex - 1);
			num2 = num2 - (num2 - 3) * (((uint)num2 > 2u) ? 1 : 0) + 110;
			int num3 = num2;
			Vector3 val4 = ((num3 == 111) ? (((Component)Variables.taggerInstance.headCollider).transform.position + new Vector3(0f, 0.4f, 0f)) : Variables.taggerInstance.leftHandTransform.position);
			Vector3 val5 = val4;
			component.SetPosition(0, val5);
			component.SetPosition(1, ((Component)current).transform.position);
		}
	}

	public static void ToggleXray(bool enabled)
	{
		if (enabled)
		{
			if (hiddenRenderers.Count != 0)
			{
				return;
			}
			Renderer[] allType = Variables.GetAllType<Renderer>(false);
			int num = 0;
			while (num < allType.Length)
			{
				Renderer val = allType[num];
				if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null) && !(val is SkinnedMeshRenderer) && val.enabled && ((Component)val).gameObject.activeSelf)
				{
					val.enabled = false;
					hiddenRenderers.Add(val);
					num++;
				}
				else
				{
					num++;
				}
			}
			return;
		}
		foreach (Renderer current in hiddenRenderers)
		{
			if ((UnityEngine.Object)(object)current != (UnityEngine.Object)null)
			{
				current.enabled = true;
			}
		}
		hiddenRenderers.Clear();
	}

	public static void DrunkCam()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.LPrimary, ref InputHandler.isOn, ref _camLast);
		if (!InputHandler.isOn)
		{
			DisableNXOCamera();
		}
		else if ((UnityEngine.Object)(object)nxoCamera == (UnityEngine.Object)null)
		{
			nxoCamera = new GameObject("NXO_Cam");
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			float num = 15f;
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			nxoCamera.transform.rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation * Quaternion.Euler(Mathf.Sin(Time.time) * num, Mathf.Cos(Time.time * 0.7f) * num, Mathf.Sin(Time.time * 1.3f) * num);
		}
		else
		{
			float num = 15f;
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			nxoCamera.transform.rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation * Quaternion.Euler(Mathf.Sin(Time.time) * num, Mathf.Cos(Time.time * 0.7f) * num, Mathf.Sin(Time.time * 1.3f) * num);
		}
	}

	public static void OrbitCam()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.LPrimary, ref InputHandler.isOn, ref _camLast);
		if (!InputHandler.isOn)
		{
			DisableNXOCamera();
		}
		else if ((UnityEngine.Object)(object)nxoCamera == (UnityEngine.Object)null)
		{
			nxoCamera = new GameObject("NXO_Cam");
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			float num = Time.time * 45f;
			Vector3 val2 = Quaternion.Euler(0f, num, 0f) * new Vector3(2f, 0.5f, 0f);
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + val2;
			nxoCamera.transform.LookAt(((Component)Variables.taggerInstance.headCollider).transform.position);
		}
		else
		{
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			float num = Time.time * 45f;
			Vector3 val2 = Quaternion.Euler(0f, num, 0f) * new Vector3(2f, 0.5f, 0f);
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.position + val2;
			nxoCamera.transform.LookAt(((Component)Variables.taggerInstance.headCollider).transform.position);
		}
	}

	public static void BoneESP(bool enable)
	{
		if (!enable)
		{
			boneData.Clear();
			return;
		}
		boneData.material = EnsureMaterial(ref boneData.material, Variables.guiShader);
		List<VRRig> list = VRRigCache.ActiveRigs.ToList();
		CleanupInactiveRigs(boneData, list);
		foreach (VRRig current in list)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			if (!boneData.multiObjects.TryGetValue(current, out List<GameObject> value))
			{
				value = new List<GameObject>();
				int num = 0;
				if (num <= bones.Length / 2)
				{
					do
					{
						GameObject val = new GameObject("BoneLine");
						LineRenderer val2 = val.AddComponent<LineRenderer>();
						val2.positionCount = 2;
						float startWidth = (val2.endWidth = 0.04f);
						val2.startWidth = startWidth;
						((Renderer)val2).material = boneData.material;
						value.Add(val);
						num++;
					}
					while (num <= bones.Length / 2);
				}
				boneData.multiObjects[current] = value;
			}
			Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current);
			LineRenderer component = value[0].GetComponent<LineRenderer>();
			LineRenderer obj = component;
			Color startColor = (component.endColor = (Color32)(teamColor));
			obj.startColor = startColor;
			component.SetPositions((Vector3[])(object)new Vector3[2]
			{
				((Component)current.head.rigTarget).transform.position + new Vector3(0f, 0.16f, 0f),
				((Component)current.head.rigTarget).transform.position - new Vector3(0f, 0.4f, 0f)
			});
			for (int num3 = 0; num3 < bones.Length; num3 += 2)
			{
				LineRenderer component2 = value[1 + num3 / 2].GetComponent<LineRenderer>();
				startColor = (component2.endColor = (Color32)(teamColor));
				component2.startColor = startColor;
				component2.SetPositions((Vector3[])(object)new Vector3[2]
				{
					current.mainSkin.bones[bones[num3]].position,
					current.mainSkin.bones[bones[num3 + 1]].position
				});
			}
		}
	}

	private static Vector3[] CalculateCorners(Vector3 center, Vector3 halfSize, Quaternion rotation)
	{
		Vector3[] array = (Vector3[])(object)new Vector3[8]
		{
			new Vector3(0f - halfSize.x, 0f - halfSize.y, 0f - halfSize.z),
			new Vector3(halfSize.x, 0f - halfSize.y, 0f - halfSize.z),
			new Vector3(halfSize.x, 0f - halfSize.y, halfSize.z),
			new Vector3(0f - halfSize.x, 0f - halfSize.y, halfSize.z),
			new Vector3(0f - halfSize.x, halfSize.y, 0f - halfSize.z),
			new Vector3(halfSize.x, halfSize.y, 0f - halfSize.z),
			new Vector3(halfSize.x, halfSize.y, halfSize.z),
			new Vector3(0f - halfSize.x, halfSize.y, halfSize.z)
		};
		Vector3[] array2 = (Vector3[])(object)new Vector3[8];
		int num = 0;
		if (num < 8)
		{
			do
			{
				array2[num] = center + rotation * array[num];
				num++;
			}
			while (num < 8);
		}
		return array2;
	}

	public static void FilledBoxESP2D(bool enable)
	{
		if (!enable)
		{
			filledBox2DData.Clear();
			return;
		}
		filledBox2DData.material = EnsureMaterial(ref filledBox2DData.material, Variables.guiShader);
		List<VRRig> list = VRRigCache.ActiveRigs.ToList();
		CleanupInactiveRigs(filledBox2DData, list);
		foreach (VRRig current in list)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			if (!filledBox2DData.singleObjects.TryGetValue(current, out GameObject value) || (UnityEngine.Object)(object)value == (UnityEngine.Object)null)
			{
				value = GameObject.CreatePrimitive((PrimitiveType)5);
				((UnityEngine.Object)value).name = "FilledBox2D";
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)value.GetComponent<Collider>());
				Material material = new Material(Variables.guiShader);
				value.GetComponent<Renderer>().material = material;
				filledBox2DData.singleObjects[current] = value;
			}
			Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current);
			value.GetComponent<Renderer>().material.color = (Color32)(new Color32(teamColor.r, teamColor.g, teamColor.b, (byte)80));
			Vector3 position = ((Component)current).transform.position;
			value.transform.position = position;
			value.transform.rotation = Quaternion.LookRotation(position - ((Component)Camera.main).transform.position);
			value.transform.localScale = new Vector3(1.06f, 1.06f, 1f);
		}
	}

	public static void ToggleFog(bool enable)
	{
		if (enable)
		{
			ZoneShaderSettings.activeInstance.SetGroundFogValue(new Color(0.9569f, 0.6941f, 0.502f, 0.1216f), 40f, 10f, 40f);
		}
		else
		{
			ZoneShaderSettings.activeInstance.SetGroundFogValue(Color.clear, 0f, 0f, 0f);
		}
	}

	public static void FPSBoost(bool enable)
	{
		if (enable != lastState)
		{
			lastState = enable;
			if (enable)
			{
				Screen.SetResolution(NXOUI.originalWidth / 6, NXOUI.originalHeight / 6, true);
				QualitySettings.SetQualityLevel(0, true);
				QualitySettings.pixelLightCount = 0;
				QualitySettings.shadows = (ShadowQuality)0;
				QualitySettings.realtimeReflectionProbes = false;
				QualitySettings.softParticles = false;
				QualitySettings.lodBias = 0.5f;
				QualitySettings.antiAliasing = 0;
				QualitySettings.vSyncCount = 0;
				QualitySettings.anisotropicFiltering = (AnisotropicFiltering)0;
				QualitySettings.globalTextureMipmapLimit = 2;
				QualitySettings.skinWeights = (SkinWeights)1;
			}
			else
			{
				Screen.SetResolution(NXOUI.originalWidth, NXOUI.originalHeight, true);
				QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
				QualitySettings.pixelLightCount = 4;
				QualitySettings.shadows = (ShadowQuality)2;
				QualitySettings.realtimeReflectionProbes = true;
				QualitySettings.softParticles = true;
				QualitySettings.lodBias = 1.5f;
				QualitySettings.antiAliasing = 2;
				QualitySettings.vSyncCount = 1;
				QualitySettings.anisotropicFiltering = (AnisotropicFiltering)2;
				QualitySettings.globalTextureMipmapLimit = 0;
				QualitySettings.skinWeights = (SkinWeights)4;
			}
		}
	}

	public static void SpectateGun()
	{
		if (GunLib.SetupLockOnGun())
		{
			if ((UnityEngine.Object)(object)nxoCamera == (UnityEngine.Object)null)
			{
				nxoCamera = new GameObject("NXO_Cam");
				nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
				Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
				val.nearClipPlane = 0.01f;
				val.cameraType = (CameraType)1;
				nxoCamera.transform.position = GunLib.lockedTargetRig.bodyTransform.TransformPoint(new Vector3(0f, 0.8f, -1.5f));
				nxoCamera.transform.rotation = GunLib.lockedTargetRig.headMesh.transform.rotation;
			}
			else
			{
				Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
				val.nearClipPlane = 0.01f;
				val.cameraType = (CameraType)1;
				nxoCamera.transform.position = GunLib.lockedTargetRig.bodyTransform.TransformPoint(new Vector3(0f, 0.8f, -1.5f));
				nxoCamera.transform.rotation = GunLib.lockedTargetRig.headMesh.transform.rotation;
			}
		}
		else
		{
			DisableNXOCamera();
		}
	}

	public static void DisableNXOCamera()
	{
		InputHandler.isOn = false;
		if ((UnityEngine.Object)(object)nxoCamera != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)nxoCamera.GetComponent<Camera>());
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)nxoCamera);
			nxoCamera = null;
		}
	}

	public static void SkeletonESP(bool enable)
	{
		if (enable)
		{
			foreach (VRRig current in VRRigCache.ActiveRigs)
			{
				if ((UnityEngine.Object)(object)current == (UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig)
				{
					continue;
				}
				Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current);
				((SyncToPlayerColor)current.skeleton).UpdateColor(current.playerColor);
				((Renderer)current.skeleton.renderer).sharedMaterial.shader = Shader.Find("GUI/Text Shader");
				((Renderer)current.skeleton.renderer).sharedMaterial.color = (Color32)(teamColor);
				((Behaviour)current.skeleton).enabled = true;
				((Renderer)current.skeleton.renderer).enabled = true;
			}
			return;
		}
		foreach (VRRig current2 in VRRigCache.ActiveRigs)
		{
			if ((UnityEngine.Object)(object)current2 == (UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig)
			{
				continue;
			}
			((Behaviour)current2.skeleton).enabled = false;
			((Renderer)current2.skeleton.renderer).enabled = false;
		}
	}

	public static void VrThirdPersonInFront()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.LPrimary, ref InputHandler.isOn, ref _camLast);
		if (!InputHandler.isOn)
		{
			DisableNXOCamera();
		}
		else if ((UnityEngine.Object)(object)nxoCamera == (UnityEngine.Object)null)
		{
			nxoCamera = new GameObject("NXO_Cam");
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position + ((Component)Variables.taggerInstance.headCollider).transform.forward * 0.8f;
			nxoCamera.transform.LookAt(((Component)Variables.taggerInstance.headCollider).transform.position);
		}
		else
		{
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position + ((Component)Variables.taggerInstance.headCollider).transform.forward * 0.8f;
			nxoCamera.transform.LookAt(((Component)Variables.taggerInstance.headCollider).transform.position);
		}
	}

	public static void UncapFPS(bool enabled)
	{
		if (enabled)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = int.MaxValue;
		}
		else
		{
			Application.targetFrameRate = 144;
		}
	}

	public static void FilledBoxESP(bool enable)
	{
		if (!enable)
		{
			filledBoxData.Clear();
			return;
		}
		List<VRRig> list = VRRigCache.ActiveRigs.ToList();
		CleanupInactiveRigs(filledBoxData, list);
		foreach (VRRig current in list)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			if (!filledBoxData.singleObjects.TryGetValue(current, out GameObject value) || (UnityEngine.Object)(object)value == (UnityEngine.Object)null)
			{
				value = GameObject.CreatePrimitive((PrimitiveType)3);
				((UnityEngine.Object)value).name = "FilledBox";
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)value.GetComponent<Collider>());
				Material material = new Material(Variables.guiShader);
				value.GetComponent<Renderer>().material = material;
				filledBoxData.singleObjects[current] = value;
			}
			Color32 teamColor = GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current);
			value.GetComponent<Renderer>().material.color = (Color32)(new Color32(teamColor.r, teamColor.g, teamColor.b, (byte)80));
			value.transform.position = ((Component)current).transform.position - new Vector3(0f, 0.075f, 0f);
			value.transform.rotation = ((Component)current).transform.rotation;
			value.transform.localScale = new Vector3(((Component)current).transform.localScale.x * 0.75f, ((Component)current).transform.localScale.y * 1.05f, ((Component)current).transform.localScale.z * 0.75f);
		}
	}

	public static void FPC(bool enable)
	{
		if ((UnityEngine.Object)(object)Variables.shoulderCamera == (UnityEngine.Object)null)
		{
			Variables.shoulderCamera = GameObject.Find("Shoulder Camera");
		}
		if ((UnityEngine.Object)(object)Variables.thirdPersonCamera == (UnityEngine.Object)null)
		{
			Variables.thirdPersonCamera = GameObject.Find("Third Person Camera");
		}
		if (enable)
		{
			if (!Variables.didThirdPerson)
			{
				Variables.didThirdPerson = true;
				Variables.TransformCam = GameObject.Find("CM vcam1");
			}
			if ((UnityEngine.Object)(object)Variables.TransformCam != (UnityEngine.Object)null)
			{
				Variables.TransformCam.SetActive(false);
			}
			if ((UnityEngine.Object)(object)Variables.shoulderCamera == (UnityEngine.Object)null)
			{
				return;
			}
			Variables.shoulderCamera.transform.SetParent(((Component)Camera.main).transform);
			Variables.shoulderCamera.transform.localPosition = Vector3.zero;
			Variables.shoulderCamera.transform.localRotation = Quaternion.identity;
			Variables.shoulderCamera.GetComponent<Camera>().fieldOfView = Settings.FOV;
			return;
		}
		if ((UnityEngine.Object)(object)Variables.TransformCam != (UnityEngine.Object)null)
		{
			Variables.TransformCam.SetActive(true);
		}
		if ((UnityEngine.Object)(object)Variables.shoulderCamera == (UnityEngine.Object)null)
		{
			return;
		}
		if ((UnityEngine.Object)(object)Variables.thirdPersonCamera != (UnityEngine.Object)null)
		{
			Variables.shoulderCamera.transform.SetParent(Variables.thirdPersonCamera.transform);
			Variables.shoulderCamera.transform.localPosition = Vector3.zero;
			Variables.shoulderCamera.transform.localRotation = Quaternion.identity;
		}
	}

	public static void FixRigColors(VRRig rig)
	{
		if ((UnityEngine.Object)(object)rig == (UnityEngine.Object)null || (UnityEngine.Object)(object)rig.mainSkin == (UnityEngine.Object)null || (UnityEngine.Object)(object)rig.mainSkin.sharedMesh == (UnityEngine.Object)null || fixedMeshRigs.Contains(rig))
		{
			return;
		}
		fixedMeshRigs.Add(rig);
		if (rig.mainSkin.sharedMesh.colors32 != null && rig.mainSkin.sharedMesh.colors32.Length != 0)
		{
			Color32[] array = (Color32[])(object)new Color32[rig.mainSkin.sharedMesh.colors32.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (Color32)(Color.white);
			}
			rig.mainSkin.sharedMesh.colors32 = array;
			if (rig.mainSkin.sharedMesh.colors == null)
			{
				return;
			}
		}
		else if (rig.mainSkin.sharedMesh.colors == null)
		{
			return;
		}
		if (rig.mainSkin.sharedMesh.colors.Length != 0)
		{
			Color[] array2 = (Color[])(object)new Color[rig.mainSkin.sharedMesh.colors.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = Color.white;
			}
			rig.mainSkin.sharedMesh.colors = array2;
		}
	}

	public static void ChamsESP(bool enable)
	{
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if (current.isOfflineVRRig)
			{
				continue;
			}
			SkinnedMeshRenderer mainSkin = current.mainSkin;
			if ((UnityEngine.Object)(object)((mainSkin != null) ? ((Renderer)mainSkin).material : null) == (UnityEngine.Object)null)
			{
				continue;
			}
			Material material = ((Renderer)current.mainSkin).material;
			if (enable)
			{
				FixRigColors(current);
				material.shader = Variables.guiShader;
				material.color = (Color32)(GetTeamColor(RigManager.RigIsInfected(current), Variables.teamCheckedESP, current));
			}
			else if ((UnityEngine.Object)(object)material.shader == (UnityEngine.Object)(object)Variables.guiShader)
			{
				material.shader = Variables.uberShader;
			}
		}
	}

	private static void CleanupInactiveRigs(ESPData data, List<VRRig> activeRigs)
	{
		List<VRRig> list = new List<VRRig>();
		foreach (VRRig current in data.singleObjects.Keys)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || !activeRigs.Contains(current) || current.isOfflineVRRig)
			{
				list.Add(current);
			}
		}
		foreach (VRRig current2 in data.multiObjects.Keys)
		{
			if ((UnityEngine.Object)(object)current2 == (UnityEngine.Object)null || !activeRigs.Contains(current2) || current2.isOfflineVRRig)
			{
				list.Add(current2);
			}
		}
		foreach (VRRig current3 in list)
		{
			if (data.singleObjects.TryGetValue(current3, out GameObject value))
			{
				if ((UnityEngine.Object)(object)value != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)value);
				}
				data.singleObjects.Remove(current3);
			}
			if (data.multiObjects.TryGetValue(current3, out List<GameObject> value2))
			{
				foreach (GameObject current4 in value2)
				{
					if ((UnityEngine.Object)(object)current4 != (UnityEngine.Object)null)
					{
						UnityEngine.Object.Destroy((UnityEngine.Object)(object)current4);
					}
				}
				data.multiObjects.Remove(current3);
			}
		}
	}

	public static void ToggleSnow(bool enable)
	{
		GameObject obj = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Environment/WeatherDayNight");
		object obj2;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			Transform transform = obj.transform;
			if (transform == null)
			{
				obj2 = null;
			}
			else
			{
				Transform obj3 = transform.Find("snow");
				obj2 = ((obj3 != null) ? ((Component)obj3).gameObject : null);
			}
		}
		GameObject val = (GameObject)obj2;
		if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
		{
			val.SetActive(enable);
			((Behaviour)val.GetComponent<TimeOfDayDependentAudio>()).enabled = !enable;
			Transform val2 = val.transform.Find("snow partic");
			if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null)
			{
				((Component)val2).gameObject.SetActive(enable);
			}
		}
	}

	private static Color32 GetTeamColor(bool isInfected, bool teamChecked, VRRig rig)
	{
		if (teamChecked)
		{
			if (!isInfected)
			{
				return new Color32((byte)0, byte.MaxValue, (byte)0, (byte)155);
			}
			return new Color32(byte.MaxValue, (byte)0, (byte)0, (byte)155);
		}
		if (isInfected)
		{
			return new Color32(byte.MaxValue, (byte)0, (byte)0, (byte)155);
		}
		Color playerColor = rig.playerColor;
		return new Color32((byte)(playerColor.r * 255f), (byte)(playerColor.g * 255f), (byte)(playerColor.b * 255f), (byte)155);
	}

	public static void UpsideDownCam()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.LPrimary, ref InputHandler.isOn, ref _camLast);
		if (!InputHandler.isOn)
		{
			DisableNXOCamera();
		}
		else if ((UnityEngine.Object)(object)nxoCamera == (UnityEngine.Object)null)
		{
			nxoCamera = new GameObject("NXO_Cam");
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			nxoCamera.transform.rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation * Quaternion.Euler(0f, 0f, 180f);
		}
		else
		{
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			nxoCamera.transform.rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation * Quaternion.Euler(0f, 0f, 180f);
		}
	}

	public static void VrThirdPerson()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.LPrimary, ref InputHandler.isOn, ref _camLast);
		if (!InputHandler.isOn)
		{
			DisableNXOCamera();
		}
		else if ((UnityEngine.Object)(object)nxoCamera == (UnityEngine.Object)null)
		{
			nxoCamera = new GameObject("NXO_Cam");
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.TransformPoint(new Vector3(0f, 0.8f, -1.5f));
			nxoCamera.transform.rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation;
		}
		else
		{
			Camera val = nxoCamera.GetComponent<Camera>() ?? nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			nxoCamera.transform.position = ((Component)Variables.taggerInstance.bodyCollider).transform.TransformPoint(new Vector3(0f, 0.8f, -1.5f));
			nxoCamera.transform.rotation = ((Component)Variables.taggerInstance.headCollider).transform.rotation;
		}
	}

	private static Material EnsureMaterial(ref Material material, Shader shader)
	{
		if ((UnityEngine.Object)(object)material == (UnityEngine.Object)null || (UnityEngine.Object)(object)material.shader != (UnityEngine.Object)(object)shader)
		{
			if ((UnityEngine.Object)(object)material != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)material);
				material = new Material(shader);
				return material;
			}
			material = new Material(shader);
			return material;
		}
		return material;
	}

	public static string GetPlatform(VRRig rig)
	{
		if (rig.HasCosmetic("S. FIRST LOGIN"))
		{
			return "<color=#001b96>STEAM</color>";
		}
		if (rig.HasCosmetic("FIRST LOGIN"))
		{
			return "<color=#4CFF4C>PC</color>";
		}
		return "<color=#FFA500>QUEST</color>";
	}

	public static void Nametags(bool enable)
	{
		if (!enable)
		{
			foreach (GameObject current in nametags.Values)
			{
				if ((UnityEngine.Object)(object)current != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)current);
				}
			}
			nametags.Clear();
			return;
		}
		List<VRRig> activeRigs = VRRigCache.ActiveRigs.ToList();
		List<VRRig> list = nametags.Keys.Where((VRRig r) => (UnityEngine.Object)(object)r == (UnityEngine.Object)null || !activeRigs.Contains(r) || r.isOfflineVRRig).ToList();
		foreach (VRRig current2 in list)
		{
			if (nametags.TryGetValue(current2, out GameObject value))
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)value);
			}
			nametags.Remove(current2);
		}
		foreach (VRRig current3 in activeRigs)
		{
			if ((UnityEngine.Object)(object)current3 == (UnityEngine.Object)null || current3.isMyPlayer || current3.isOfflineVRRig || current3.Creator == null)
			{
				continue;
			}
			if (!nametags.TryGetValue(current3, out GameObject value2) || (UnityEngine.Object)(object)value2 == (UnityEngine.Object)null)
			{
				value2 = new GameObject("Nametag");
				GameObject val = new GameObject("Shadow");
				val.transform.SetParent(value2.transform);
				val.transform.localPosition = new Vector3(0.003f, -0.003f, 0.001f);
				val.transform.localScale = Vector3.one;
				TextMesh val2 = val.AddComponent<TextMesh>();
				val2.fontSize = 280;
				val2.fontStyle = (FontStyle)1;
				val2.font = Main.CurrentFont;
				val2.color = new Color(0f, 0f, 0f, 0.7f);
				val2.anchor = (TextAnchor)4;
				val2.alignment = (TextAlignment)1;
				((Component)val2).GetComponent<Renderer>().material = val2.font.material;
				TextMesh val3 = value2.AddComponent<TextMesh>();
				val3.fontSize = 280;
				val3.fontStyle = (FontStyle)1;
				val3.font = Main.CurrentFont;
				val3.anchor = (TextAnchor)4;
				val3.alignment = (TextAlignment)1;
				((Component)val3).GetComponent<Renderer>().material = val3.font.material;
				nametags[current3] = value2;
			}
			TextMesh component = value2.GetComponent<TextMesh>();
			Transform child = value2.transform.GetChild(0);
			TextMesh val4 = ((child != null) ? ((Component)child).GetComponent<TextMesh>() : null);
			Color playerColor = current3.playerColor;
			string text = ColorUtility.ToHtmlStringRGB(playerColor);
			string platform = GetPlatform(current3);
			string text2 = $"({(int)(playerColor.r * 9f)}, {(int)(playerColor.g * 9f)}, {(int)(playerColor.b * 9f)})";
			int fps = current3.fps;
			string text3 = "00FF00";
			if (fps < 45)
			{
				text3 = "FF0000";
			}
			else if (fps < 60)
			{
				text3 = "FFA500";
			}
			else if (fps < 72)
			{
				text3 = "FFFF00";
			}
			else if (fps < 90)
			{
				text3 = "7FFF00";
			}
			string text4 = $"<color=#{text3}>{fps}</color>";
			int nametagTypeIndex = Settings.nametagTypeIndex;
			int num = nametagTypeIndex;
			num = num - (num - 3) * (((uint)num > 2u) ? 1 : 0) + 507;
			int num2 = num;
			string text5 = ((num2 == 508) ? ("<size=320><color=#" + text + ">" + current3.Creator.NickName + "</color></size>\n<size=220>" + platform + " | " + text4 + "</size>") : ("<size=320><color=#" + text + ">" + current3.Creator.NickName + "</color></size>\n<size=220>" + platform + " | " + text2 + " | " + text4 + "</size>\n<size=200><color=#AAAAAA>" + current3.Creator.UserId + "</color></size>"));
			string text6 = (component.text = text5);
			if ((UnityEngine.Object)(object)val4 != (UnityEngine.Object)null)
			{
				val4.text = text6.Replace("<color=#" + text + ">", "<color=#000000>").Replace("<color=#AAAAAA>", "<color=#000000>").Replace("<color=#" + text3 + ">", "<color=#000000>");
			}
			float num3 = Vector3.Distance(((Component)Camera.main).transform.position, ((Component)current3).transform.position);
			float num4 = Mathf.Min(num3, 20f);
			float num5 = 0.0035f + num4 * 0.00025f;
			value2.transform.localScale = Vector3.one * num5;
			value2.transform.position = current3.headMesh.transform.position + Vector3.up * 0.55f;
			value2.transform.LookAt(((Component)Camera.main).transform);
			value2.transform.Rotate(0f, 180f, 0f);
		}
	}
}
