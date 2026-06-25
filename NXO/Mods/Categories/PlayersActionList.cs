using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NXO.Menu;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace NXO.Mods.Categories;

public class PlayersActionList
{
	public static NetPlayer selectedPlayer;

	public static Camera PlayerCam;

	public static GameObject PlayerCamScreen;

	public static RenderTexture renderTexture;

	public static bool isCameraActive;

	public static GameObject PlayerCamOutline;

	public static void GeneratePlayer_ActionButtons()
	{
		List<ButtonHandler.Button> list = ModButtons.buttons.ToList();
		list.RemoveAll((ButtonHandler.Button b) => b.Page == Category.Player_Action);
		VRRig targetRig = RigManager.GetVRRigFromNetPlayer(selectedPlayer);
		list.Add(new ButtonHandler.Button("Return", Category.Player_Action, isToggle: false, isActive: false, delegate
		{
			ClearPlayerCam(clearAll: true);
			ButtonHandler.ChangePage(Category.Players);
		})
		{
			isCategory = true
		});
		list.Add(new ButtonHandler.Button("Name : " + selectedPlayer.NickName, Category.Player_Action, isToggle: false, isActive: false, null));
		list.Add(new ButtonHandler.Button("Platform : " + Visuals.GetPlatform(targetRig), Category.Player_Action, isToggle: false, isActive: false, null));
		list.Add(new ButtonHandler.Button("ID : " + selectedPlayer.UserId.ToUpper(), Category.Player_Action, isToggle: false, isActive: false, null));
		string[] obj = new string[6] { "Color : ", null, null, null, null, null };
		float num = ((Renderer)targetRig.mainSkin).material.color.r * 9f;
		float num2 = num;
		obj[1] = num2.ToString();
		obj[2] = ", ";
		num = ((Renderer)targetRig.mainSkin).material.color.g * 9f;
		num2 = num;
		obj[3] = num2.ToString();
		obj[4] = ", ";
		num = ((Renderer)targetRig.mainSkin).material.color.b * 9f;
		num2 = num;
		obj[5] = num2.ToString();
		list.Add(new ButtonHandler.Button(string.Concat(obj), Category.Player_Action, isToggle: false, isActive: false, null));
		list.Add(new ButtonHandler.Button("Tag Player", Category.Player_Action, isToggle: true, isActive: false, () =>
		{
			if ((UnityEngine.Object)(object)targetRig != (UnityEngine.Object)null)
			{
				if (!RigManager.RigIsInfected(VRRig.LocalRig))
				{
					NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Must Be Tagged");
				}
				else if (RigManager.RigIsInfected(targetRig))
				{
					NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "`" + targetRig.Creator.NickName + "` Is Already Tagged");
				}
				else
				{
					Gamemode.TagPlayer(targetRig);
				}
			}
		}));
		list.Add(new ButtonHandler.Button("Lag Player", Category.Player_Action, isToggle: true, isActive: false, () =>
		{
			Overpowered.LagTarget(new int[1] { targetRig.Creator.ActorNumber });
		}));
		list.Add(new ButtonHandler.Button("Deafen Player", Category.Player_Action, isToggle: true, isActive: false, () =>
		{
			Overpowered.DeafenPlayer(targetRig);
		}));
		list.Add(new ButtonHandler.Button("Copy Player", Category.Player_Action, isToggle: true, isActive: false, () =>
		{
			Player.CopyPlayer(targetRig);
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}));
		list.Add(new ButtonHandler.Button("Piggyback Player", Category.Player_Action, isToggle: true, isActive: false, () =>
		{
			if ((UnityEngine.Object)(object)targetRig != (UnityEngine.Object)null)
			{
				Vector3 position = ((Component)targetRig).transform.position + ((Component)targetRig).transform.up * 0.3f - ((Component)targetRig).transform.forward * 0.4f;
				Player.TeleportTo(position);
			}
		}));
		list.Add(new ButtonHandler.Button("Spectate Player (X)", Category.Player_Action, isToggle: true, isActive: false, delegate
		{
			SpectatePlayer();
		}, delegate
		{
			Visuals.DisableNXOCamera();
		}));
		list.Add(new ButtonHandler.Button("Teleport To Player", Category.Player_Action, isToggle: false, isActive: false, () =>
		{
			Player.TeleportTo(((Component)targetRig).transform.position);
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}));
		ModButtons.buttons = list.ToArray();
	}

	public static void GeneratePlayerButtons()
	{
		List<ButtonHandler.Button> list = ModButtons.buttons.ToList();
		list.RemoveAll((ButtonHandler.Button b) => b.Page == Category.Players);
		Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Photon.Realtime.Player val in playerList)
		{
			NetPlayer capturedPlayer = ((NetPlayer)(val));
			VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(((NetPlayer)(val)));
			string text = val.NickName;
			if ((UnityEngine.Object)(object)vRRigFromNetPlayer != (UnityEngine.Object)null)
			{
				SkinnedMeshRenderer mainSkin = vRRigFromNetPlayer.mainSkin;
				if ((UnityEngine.Object)(object)((mainSkin != null) ? ((Renderer)mainSkin).material : null) != (UnityEngine.Object)null)
				{
					Color color = ((Renderer)vRRigFromNetPlayer.mainSkin).material.color;
					string text2 = ColorUtility.ToHtmlStringRGB(color);
					text = "<color=#" + text2 + ">" + val.NickName + "</color>";
				}
			}
			if (RigManager.RigIsInfected(RigManager.GetVRRigFromNetPlayer(capturedPlayer)))
			{
				text += " : <color=red>Tagged</color>";
			}
			list.Add(new ButtonHandler.Button(text, Category.Players, isToggle: false, isActive: false, () =>
			{
				selectedPlayer = capturedPlayer;
				GeneratePlayer_ActionButtons();
				ButtonHandler.ChangePage(Category.Player_Action);
			})
			{
				isCategory = true
			});
		}
		ModButtons.buttons = list.ToArray();
		Main.RefreshMenu();
	}

	public static void ClearPlayerCam(bool clearAll = false)
	{
		if (clearAll)
		{
			if ((UnityEngine.Object)(object)PlayerCam != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)PlayerCam).gameObject);
				PlayerCam = null;
			}
			if ((UnityEngine.Object)(object)renderTexture != (UnityEngine.Object)null)
			{
				if (renderTexture.IsCreated())
				{
					renderTexture.Release();
				}
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)renderTexture);
				renderTexture = null;
			}
			selectedPlayer = null;
			isCameraActive = false;
		}
		if ((UnityEngine.Object)(object)PlayerCamScreen != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)PlayerCamScreen);
			PlayerCamScreen = null;
		}
		if ((UnityEngine.Object)(object)PlayerCamOutline != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)PlayerCamOutline);
			PlayerCamOutline = null;
		}
	}

	public static void AddPlayerCam()
	{
		if (selectedPlayer == null || (UnityEngine.Object)(object)Variables.menuObj == (UnityEngine.Object)null)
		{
			return;
		}
		VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(selectedPlayer);
		if ((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Players Rig Is Null, Please Refresh");
			return;
		}
		if ((UnityEngine.Object)(object)PlayerCamScreen != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)PlayerCamScreen);
			PlayerCamScreen = null;
		}
		if ((UnityEngine.Object)(object)PlayerCam == (UnityEngine.Object)null)
		{
			GameObject val2 = new GameObject("NXO Photon.Realtime.Player Camera");
			PlayerCam = val2.AddComponent<Camera>();
			renderTexture = new RenderTexture(512, 512, 16, (RenderTextureFormat)0);
			((Texture)renderTexture).filterMode = (FilterMode)1;
			((Texture)renderTexture).wrapMode = (TextureWrapMode)1;
			renderTexture.Create();
			PlayerCam.targetTexture = renderTexture;
			PlayerCam.fieldOfView = 90f;
			PlayerCam.nearClipPlane = 0.01f;
			PlayerCam.farClipPlane = 1000f;
			PlayerCam.clearFlags = (CameraClearFlags)1;
			((Behaviour)PlayerCam).enabled = true;
			PlayerCam.cullingMask = -1;
		}
		PlayerCamScreen = GameObject.CreatePrimitive((PrimitiveType)3);
		((UnityEngine.Object)PlayerCamScreen).name = "NXO Photon.Realtime.Player Camera Screen";
		PlayerCamScreen.transform.SetParent(Variables.menuObj.transform, false);
		PlayerCamScreen.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
		PlayerCamScreen.transform.localScale = new Vector3(0.01f, 0.5f, 0.75f);
		PlayerCamScreen.transform.localPosition = new Vector3(0.064f, 0.85f, 0.05f);
		Renderer component = PlayerCamScreen.GetComponent<Renderer>();
		if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
		{
			Material val = new Material(Shader.Find("Unlit/Texture"));
			val.mainTexture = (Texture)(object)renderTexture;
			component.material = val;
			Main._dynamicMaterials.Add(val);
		}
		Collider component2 = PlayerCamScreen.GetComponent<Collider>();
		if ((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)component2);
		}
		Material pinwheelMaterial = Main.GetPinwheelMaterial();
		if ((UnityEngine.Object)(object)pinwheelMaterial != (UnityEngine.Object)null)
		{
			PlayerCamOutline = GameObject.CreatePrimitive((PrimitiveType)3);
			Rigidbody component4 = PlayerCamOutline.GetComponent<Rigidbody>();
			if ((UnityEngine.Object)(object)component4 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)component4);
			}
			BoxCollider component3 = PlayerCamOutline.GetComponent<BoxCollider>();
			if ((UnityEngine.Object)(object)component3 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)component3);
			}
			PlayerCamOutline.transform.SetParent(Variables.menuObj.transform, false);
			PlayerCamOutline.transform.localPosition = PlayerCamScreen.transform.localPosition;
			PlayerCamOutline.transform.localRotation = PlayerCamScreen.transform.localRotation;
			Vector3 localScale = PlayerCamScreen.transform.localScale;
			PlayerCamOutline.transform.localScale = new Vector3(localScale.x - 0.0025f, localScale.y + 0.015f, localScale.z + 0.015f);
			PlayerCamOutline.GetComponent<Renderer>().material = pinwheelMaterial;
			Main._dynamicMaterials.Add(pinwheelMaterial);
		}
		isCameraActive = true;
	}

	public static void SpectatePlayer()
	{
		InputHandler.ToggleOnButtonPress(InputHandler.LPrimary, ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
		if (!InputHandler.isOn)
		{
			Visuals.DisableNXOCamera();
			return;
		}
		VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(selectedPlayer);
		if ((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null)
		{
			Visuals.DisableNXOCamera();
		}
		else if ((UnityEngine.Object)(object)Visuals.nxoCamera == (UnityEngine.Object)null)
		{
			Visuals.nxoCamera = new GameObject("NXO_Cam");
			Visuals.nxoCamera.transform.position = ((Component)Variables.taggerInstance.headCollider).transform.position;
			Camera val = Visuals.nxoCamera.GetComponent<Camera>() ?? Visuals.nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			Visuals.nxoCamera.transform.position = vRRigFromNetPlayer.bodyTransform.TransformPoint(new Vector3(0f, 0.8f, -1.5f));
			Visuals.nxoCamera.transform.rotation = vRRigFromNetPlayer.headMesh.transform.rotation;
		}
		else
		{
			Camera val = Visuals.nxoCamera.GetComponent<Camera>() ?? Visuals.nxoCamera.AddComponent<Camera>();
			val.nearClipPlane = 0.01f;
			val.cameraType = (CameraType)1;
			Visuals.nxoCamera.transform.position = vRRigFromNetPlayer.bodyTransform.TransformPoint(new Vector3(0f, 0.8f, -1.5f));
			Visuals.nxoCamera.transform.rotation = vRRigFromNetPlayer.headMesh.transform.rotation;
		}
	}

	public static void UpdatePlayerCam()
	{
		if (isCameraActive && !((UnityEngine.Object)(object)PlayerCam == (UnityEngine.Object)null) && selectedPlayer != null && Variables.currentPage == Category.Player_Action)
		{
			VRRig vRRigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(selectedPlayer);
			if ((UnityEngine.Object)(object)vRRigFromNetPlayer == (UnityEngine.Object)null)
			{
				ClearPlayerCam(clearAll: true);
			}
			else if (!((UnityEngine.Object)(object)vRRigFromNetPlayer.head?.rigTarget == (UnityEngine.Object)null))
			{
				Transform rigTarget = vRRigFromNetPlayer.head.rigTarget;
				Vector3 position = rigTarget.position - rigTarget.forward * 1f + Vector3.up * 0.25f;
				((Component)PlayerCam).transform.position = position;
				((Component)PlayerCam).transform.rotation = Quaternion.LookRotation(rigTarget.forward, Vector3.up);
			}
		}
	}

	public static IEnumerator DelayedPlayerButtonGeneration()
	{
		yield return new WaitForSeconds(0.5f);
		if (Variables.currentPage == Category.Players)
		{
			GeneratePlayerButtons();
		}
	}
}
