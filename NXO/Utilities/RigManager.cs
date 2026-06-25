using GorillaNetworking;
using NXO.Mods.Categories;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace NXO.Utilities;

public static class RigManager
{
	private static VRRig ghostRig;

	private static Material ghostRigMaterial;

	public static bool ghostRigEnabled = true;

	public static GameObject GetHandObject => Variables.rightHandedMenu ? ((Component)Variables.taggerInstance.offlineVRRig.rightHandPlayer).gameObject : ((Component)Variables.taggerInstance.offlineVRRig.leftHandPlayer).gameObject;

	public static Photon.Realtime.Player NetPlayerToPhotonPlayer(NetPlayer p)
	{
		return p.GetPlayerRef();
	}

	public static PhotonView GetPhotonViewFromVRRig(VRRig vrrig)
	{
		NetworkView networkViewFromVRRig = GetNetworkViewFromVRRig(vrrig);
		return (networkViewFromVRRig != null) ? networkViewFromVRRig.GetView : null;
	}

	public static void HandleGhostRig()
	{
		if (!ghostRigEnabled)
		{
			if ((UnityEngine.Object)(object)ghostRig != (UnityEngine.Object)null)
			{
				((Component)ghostRig).gameObject.SetActive(false);
			}
			return;
		}
		if (!((Behaviour)Variables.taggerInstance.offlineVRRig).enabled)
		{
			if ((UnityEngine.Object)(object)ghostRig == (UnityEngine.Object)null)
			{
				ghostRig = UnityEngine.Object.Instantiate<VRRig>(Variables.taggerInstance.offlineVRRig);
				ghostRig.headBodyOffset = Vector3.zero;
				((Component)ghostRig).transform.SetParent(((Component)Variables.taggerInstance.offlineVRRig).transform.parent);
				Transform leftSlide = ((Component)ghostRig).transform.Find("VR Constraints/LeftArm/Left Arm IK/SlideAudio");
				if (leftSlide != null)
				{
					((Component)leftSlide).gameObject.SetActive(false);
				}
				Transform rightSlide = ((Component)ghostRig).transform.Find("VR Constraints/RightArm/Right Arm IK/SlideAudio");
				if (rightSlide != null)
				{
					((Component)rightSlide).gameObject.SetActive(false);
				}
				Transform bodySlide = ((Component)ghostRig).transform.Find("GorillaPlayerNetworkedRigAnchor/rig/bodySlideAudio");
				if (bodySlide != null)
				{
					((Component)bodySlide).gameObject.SetActive(false);
				}
				Visuals.FixRigColors(ghostRig);
			}
			if ((UnityEngine.Object)(object)ghostRigMaterial == (UnityEngine.Object)null)
			{
				ghostRigMaterial = new Material(Variables.guiShader);
				ghostRigMaterial.color = (Color32)(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)50));
			}
			if (!((Component)ghostRig).gameObject.activeSelf)
			{
				((Component)ghostRig).gameObject.SetActive(true);
			}
			if (!((Behaviour)ghostRig).enabled)
			{
				((Behaviour)ghostRig).enabled = true;
			}
			((Renderer)ghostRig.mainSkin).material = ghostRigMaterial;
			((Component)ghostRig.headConstraint).transform.SetPositionAndRotation(((Component)Variables.playerInstance.headCollider).transform.position, ((Component)Variables.playerInstance.headCollider).transform.rotation);
			ghostRig.leftHandTransform.SetPositionAndRotation(Variables.playerInstance.LeftHand.controllerTransform.position, Variables.playerInstance.LeftHand.controllerTransform.rotation);
			ghostRig.rightHandTransform.SetPositionAndRotation(Variables.playerInstance.RightHand.controllerTransform.position, Variables.playerInstance.RightHand.controllerTransform.rotation);
			return;
		}
		if ((UnityEngine.Object)(object)ghostRig != (UnityEngine.Object)null && ((Component)ghostRig).gameObject.activeSelf)
		{
			((Component)ghostRig).gameObject.SetActive(false);
		}
	}

	public static VRRig GetVRRigFromNetPlayer(NetPlayer netPlayer)
	{
		return (netPlayer == null) ? null : GorillaGameManager.StaticFindRigForPlayer(netPlayer);
	}

	public static bool IsRigLocal(this VRRig rig)
	{
		return (UnityEngine.Object)(object)rig != (UnityEngine.Object)null && (rig.isLocal || ((UnityEngine.Object)(object)ghostRig != (UnityEngine.Object)null && (UnityEngine.Object)(object)rig == (UnityEngine.Object)(object)ghostRig));
	}

	public static NetworkView GetNetworkViewFromVRRig(VRRig vrrig)
	{
		return ((UnityEngine.Object)(object)vrrig == (UnityEngine.Object)null) ? null : vrrig.netView;
	}

	public static NetPlayer GetNetPlayerFromVRRig(VRRig vrrig)
	{
		return vrrig.Creator ?? vrrig.OwningNetPlayer ?? NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(((Component)vrrig.rigSerializer).gameObject));
	}

	public static bool IsOtherPlayer(VRRig rig)
	{
		if ((UnityEngine.Object)(object)rig != (UnityEngine.Object)null && !rig.isOfflineVRRig)
		{
			return (UnityEngine.Object)(object)rig != (UnityEngine.Object)(object)VRRig.LocalRig;
		}
		return false;
	}

	public static void ChangeColor(Color color, object target = null)
	{
		PlayerPrefs.SetFloat("redValue", Mathf.Clamp(color.r, 0f, 1f));
		PlayerPrefs.SetFloat("greenValue", Mathf.Clamp(color.g, 0f, 1f));
		PlayerPrefs.SetFloat("blueValue", Mathf.Clamp(color.b, 0f, 1f));
		GorillaTagger.Instance.UpdateColor(color.r, color.g, color.b);
		PlayerPrefs.Save();
		if (target != null)
		{
			NetPlayer val = (NetPlayer)((target is NetPlayer) ? target : null);
			if (val == null)
			{
				if (target is RpcTarget val2)
				{
					GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", val2, new object[3] { color.r, color.g, color.b });
				}
			}
			else
			{
				GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", val, new object[3] { color.r, color.g, color.b });
			}
		}
		else
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", (RpcTarget)0, new object[3] { color.r, color.g, color.b });
		}
		Safety.RPCShield();
	}

	public static void ChangeName(string PlayerName, bool noColor = false)
	{
		((GorillaComputer)GorillaComputer.instance).currentName = PlayerName;
		((GorillaComputer)GorillaComputer.instance).SetLocalNameTagText(((GorillaComputer)GorillaComputer.instance).currentName);
		((GorillaComputer)GorillaComputer.instance).savedName = ((GorillaComputer)GorillaComputer.instance).currentName;
		PlayerPrefs.SetString("playerName", ((GorillaComputer)GorillaComputer.instance).currentName);
		PlayerPrefs.Save();
		PhotonNetwork.LocalPlayer.NickName = PlayerName;
		if (!noColor && (((GorillaComputer)GorillaComputer.instance).friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) || CosmeticWardrobeProximityDetector.IsUserNearWardrobe(PhotonNetwork.LocalPlayer.ActorNumber)))
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", (RpcTarget)0, new object[3]
			{
				VRRig.LocalRig.playerColor.r,
				VRRig.LocalRig.playerColor.g,
				VRRig.LocalRig.playerColor.b
			});
			Safety.RPCShield();
		}
	}

	public static bool RigIsInfected(VRRig rig)
	{
		bool result;
		if ((UnityEngine.Object)(object)rig == (UnityEngine.Object)null || !PhotonNetwork.InRoom || (UnityEngine.Object)(object)GorillaGameManager.instance == (UnityEngine.Object)null)
		{
			result = false;
		}
		else
		{
			NetPlayer netPlayerFromVRRig = GetNetPlayerFromVRRig(rig);
			if (netPlayerFromVRRig == null)
			{
				result = false;
			}
			else
			{
				int num = (int)((int)GorillaGameManager.instance.GameType());
				num = num - (num - 12) * (((uint)num > 11u) ? 1 : 0) + 57;
				int num2 = num;
				if (num2 != 58)
				{
					result = false;
				}
				else
				{
					GorillaGameManager instance = GorillaGameManager.instance;
					GorillaTagManager val = (GorillaTagManager)(object)((instance is GorillaTagManager) ? instance : null);
					if (val != null)
					{
						result = (val.isCurrentlyTag ? (val.currentIt == netPlayerFromVRRig) : (val.currentInfected?.Contains(netPlayerFromVRRig) ?? false));
					}
					else
					{
						SkinnedMeshRenderer mainSkin = rig.mainSkin;
						if ((UnityEngine.Object)(object)((mainSkin != null) ? ((Renderer)mainSkin).material : null) == (UnityEngine.Object)null)
						{
							result = false;
						}
						else
						{
							string name = ((UnityEngine.Object)((Renderer)rig.mainSkin).material).name;
							result = name.Contains("fected") || name.Contains("It");
						}
					}
				}
			}
		}
		return result;
	}

	public static bool HandsTouchTargets(Transform[] hands, Transform[] targets)
	{
		if (hands == null || targets == null)
		{
			return false;
		}
		int num = 0;
		while (num < hands.Length)
		{
			Transform val = hands[num];
			if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
			{
				foreach (Transform val2 in targets)
				{
					if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null && Vector3.Distance(val.position, val2.position) < 0.25f)
					{
						return true;
					}
				}
				num++;
			}
			else
			{
				num++;
			}
		}
		return false;
	}
}
