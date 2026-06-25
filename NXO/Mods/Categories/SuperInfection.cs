using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using AuthorityToClientRPC = SuperInfectionManager.AuthorityToClientRPC;
using ClientToClientRPC = SuperInfectionManager.ClientToClientRPC;
using EState = SIGadgetDashYoyo.EState;
using ProgressionData = SIPlayer.ProgressionData;
using ResourceType = SIResource.ResourceType;
using SIQuestsList = SIProgression.SIQuestsList;

namespace NXO.Mods.Categories;

public class SuperInfection
{
	public enum GadgetTypes
	{
		Thrusters,
		LongArms,
		Dash,
		Platforms,
		Blasters
	}

	private static float timer;

	public static SIPlayer siPlayer;

	public static Transform StoredPos;

	public const int WeirdGear = 1573124711;

	public const int StrangeWood = -894667703;

	public const int BouncySand = -1111610567;

	public const int FloppyMetal = -1409076879;

	public const int VibratingSpring = 1618940484;

	public const int MonkeyIdol = 1880272606;

	public const int WristJetJet = 1551901997;

	public const int WristJetPropellor = -1912435955;

	public const int StiltFixed = 1447779317;

	public const int StiltTurkey = 686793174;

	public const int StiltFixedShort = -827046453;

	public const int StiltMotorized2 = 1428761418;

	public const int StiltMotorized3 = 1996041101;

	public const int StiltFixedLong = -1906115882;

	public const int StiltExtendo = 683567723;

	public const int DashYoyo = 1799386883;

	public const int PlatformDeployer = -1236344563;

	public const int PlatformDeployerBouncy = 1657474495;

	public const int TentacleArm = 621310034;

	public const int TentacleArmCrawler = 1814413281;

	public const int TentacleArmStrider = 2060634971;

	public const int AirJuke = -1196783306;

	public const int AirGrab = -2029993207;

	public const int LaserZipline = -1581486942;

	public const int WeakBlaster = 1312505709;

	public const int ChargeBlaster = 1469243263;

	public const int MegaChargeBlaster = -1529067748;

	public const int BlastLobber = -122499862;

	public const int LongBlaster = -108912318;

	private static SuperInfectionManager SIM => SuperInfectionManager.activeSuperInfectionManager;

	public static void UnlockAllGadgets()
	{
		bool[][] unlockedTechTreeData = SIProgression.Instance.unlockedTechTreeData;
		foreach (bool[] array in unlockedTechTreeData)
		{
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = true;
			}
		}
	}

	public static void StealGadget(SIPlayer target)
	{
		if ((UnityEngine.Object)(object)target == (UnityEngine.Object)null || target.activePlayerGadgets.Count == 0)
		{
			return;
		}
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		GameEntity gameEntityFromNetId = gameEntityManager.GetGameEntityFromNetId(target.activePlayerGadgets[0]);
		if (!((UnityEngine.Object)(object)gameEntityFromNetId == (UnityEngine.Object)null))
		{
			SIGadget component = ((Component)gameEntityFromNetId).GetComponent<SIGadget>();
			bool flag = false;
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.FindAttachedHand(out flag);
				gameEntityManager.RequestGrabEntity(gameEntityFromNetId.id, flag, ((Component)gameEntityFromNetId).transform.localPosition, ((Component)gameEntityFromNetId).transform.localRotation);
			}
			else
			{
				gameEntityManager.RequestGrabEntity(gameEntityFromNetId.id, flag, ((Component)gameEntityFromNetId).transform.localPosition, ((Component)gameEntityFromNetId).transform.localRotation);
			}
		}
	}

	public static void MaxStashedBonusPoints()
	{
		SIProgression.Instance.stashedBonusPoints = 255;
		SIPlayer.SetAndBroadcastProgression();
	}

	public static void KnockbackAllPlayers(float strength)
	{
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			SIPlayer val = SIPlayer.Get(current);
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null || (UnityEngine.Object)(object)val == (UnityEngine.Object)(object)SIPlayer.LocalPlayer)
			{
				continue;
			}
			Vector3 val2 = ((Component)current).transform.position - ((Component)VRRig.LocalRig).transform.position;
			val.PlayerKnockback(val2.normalized * strength, true, false);
		}
	}

	public static Vector3 PosToVector3(Vector3 currentPos, Vector3 targetPos, float speed)
	{
		Vector3 val = targetPos - currentPos;
		return val.normalized * speed;
	}

	public static void ForceDropAllGadgets(SIPlayer player)
	{
		if ((UnityEngine.Object)(object)player == (UnityEngine.Object)null || (UnityEngine.Object)(object)player.gamePlayer?.rig == (UnityEngine.Object)null)
		{
			return;
		}
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		if (!gameEntityManager.IsAuthority())
		{
			return;
		}
		List<int> list = new List<int>(player.activePlayerGadgets);
		foreach (int current in list)
		{
			GameEntity gameEntityFromNetId = gameEntityManager.GetGameEntityFromNetId(current);
			if ((UnityEngine.Object)(object)gameEntityFromNetId == (UnityEngine.Object)null || !gameEntityManager.IsValidNetId(current))
			{
				continue;
			}
			bool flag = player.gamePlayer.IsHoldingEntity(gameEntityFromNetId.id, true);
			Vector3 val = new Vector3(UnityEngine.Random.Range(-1f, 1f), 2.5f, UnityEngine.Random.Range(-1f, 1f));
			Vector3 val2 = val.normalized * 3f;
			gameEntityManager.photonView.RPC("ThrowEntityRPC", (RpcTarget)0, new object[8]
			{
				current,
				flag,
				((Component)gameEntityFromNetId).transform.position,
				((Component)gameEntityFromNetId).transform.rotation,
				val2,
				Vector3.zero,
				PhotonNetwork.LocalPlayer,
				PhotonNetwork.Time
			});
		}
	}

	public static void SpawnAllUnlockedGadgets(SIPlayer player)
	{
		if ((UnityEngine.Object)(object)player == (UnityEngine.Object)null)
		{
			return;
		}
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		SITechTreeSO progressionSO = SIPlayer.progressionSO;
		for (int num = 0; num < player.CurrentProgression.techTreeData.Length; num++)
		{
			for (int num2 = 0; num2 < player.CurrentProgression.techTreeData[num].Length; num2++)
			{
				if (player.CurrentProgression.techTreeData[num][num2] && progressionSO.IsValidNode(num, num2))
				{
					SITechTreeNode treeNode = progressionSO.GetTreeNode(num, num2);
					if (treeNode != null && treeNode.IsDispensableGadget)
					{
						int staticHash = StaticHashExt.GetStaticHash(((UnityEngine.Object)((Component)treeNode.unlockedGadgetPrefab).gameObject).name);
						if (gameEntityManager.FactoryHasEntity(staticHash))
						{
							gameEntityManager.RequestCreateItem(staticHash, player.gamePlayer.rig.rightHandTransform.position + UnityEngine.Random.insideUnitSphere * 0.3f, Quaternion.identity, (long)player.ActorNr);
						}
					}
				}
			}
		}
	}

	public static void SetBonusProgress(int amount)
	{
		SIProgression.Instance.bonusProgress = amount;
		SIPlayer.SetAndBroadcastProgression();
	}

	public static void ForceCreateItemForPlayer(SIPlayer player, int entityTypeId, Vector3 spawnPosition, bool shoot = false)
	{
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		int num = gameEntityManager.CreateNetId(1 + gameEntityManager.FactoryGetBuiltInEntityCountById(entityTypeId));
		gameEntityManager.photonView.RPC("CreateItemRPC", (RpcTarget)1, new object[6]
		{
			new int[1] { num },
			new int[1] { entityTypeId },
			new long[1] { BitPackUtils.PackWorldPosForNetwork(spawnPosition) },
			new int[1] { BitPackUtils.PackQuaternionForNetwork(Quaternion.identity) },
			new long[1],
			new int[1] { -1 }
		});
		if (shoot)
		{
			gameEntityManager.photonView.RPC("ThrowEntityRPC", (RpcTarget)0, new object[8]
			{
				num,
				false,
				player.gamePlayer.rig.rightHandTransform.position,
				player.gamePlayer.rig.rightHandTransform.rotation,
				player.gamePlayer.rig.rightHandTransform.forward * 20f,
				Vector3.zero,
				player.gamePlayer.rig.OwningNetPlayer.GetPlayerRef(),
				PhotonNetwork.Time
			});
		}
		else
		{
			gameEntityManager.photonView.RPC("GrabEntityRPC", (RpcTarget)0, new object[4]
			{
				num,
				false,
				BitPackUtils.PackHandPosRotForNetwork(Vector3.zero, Quaternion.identity),
				player.gamePlayer.rig.OwningNetPlayer.GetPlayerRef()
			});
		}
	}

	public static void GiveTechPoints(SIPlayer player, int amount)
	{
		if (!((UnityEngine.Object)(object)player == (UnityEngine.Object)null) && amount > 0)
		{
			ProgressionData currentProgression = player.CurrentProgression;
			int[] array = new int[currentProgression.currentQuestProgresses.Length + 1];
			Array.Copy(currentProgression.currentQuestProgresses, array, currentProgression.currentQuestProgresses.Length);
			array[^1] = amount;
			player.UpdateProgression(currentProgression.resourceArray, currentProgression.limitedDepositTimeArray, currentProgression.techTreeData, currentProgression.stashedQuests, currentProgression.stashedBonusPoints, currentProgression.bonusProgress + amount, currentProgression.currentQuestIds, array);
			SIPlayer.SetAndBroadcastProgression();
		}
	}

	public static void DispenseGadget(string gadgetNodeName)
	{
		SICombinedTerminal val = UnityEngine.Object.FindObjectOfType<SICombinedTerminal>();
		if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
		{
			return;
		}
		int num = -1;
		int num2 = 0;
		if (num2 < val.dispenser.CurrentPage.AllNodes.Count)
		{
			do
			{
				if (val.dispenser.CurrentPage.AllNodes[num2].Value.nickName == gadgetNodeName)
				{
					num = num2;
					break;
				}
				num2++;
			}
			while (num2 < val.dispenser.CurrentPage.AllNodes.Count);
		}
		if (num != -1)
		{
			val.dispenser._currentNode = num;
			val.dispenser.gadgetDispensePosition.position = Variables.taggerInstance.rightHandTransform.position;
			val.dispenser.AuthorityDispenseGadgetForPlayer(SIPlayer.LocalPlayer);
		}
	}

	public static void RainbowStilts(SIGadgetStilt instance_)
	{
		Color color = Color.HSVToRGB(Time.time * 0.2f % 1f, 1f, 1f);
		Renderer val = default(Renderer);
		Renderer val2 = default(Renderer);
		if (instance_.tip.TryGetComponent<Renderer>(out val))
		{
			val.material.color = color;
			if (!instance_.midpoint.TryGetComponent<Renderer>(out val2))
			{
				return;
			}
		}
		else if (!instance_.midpoint.TryGetComponent<Renderer>(out val2))
		{
			return;
		}
		val2.material.color = color;
	}

	public static void LongExtendoStilts(SIGadgetStilt instance_)
	{
		if (instance_.TriggerToExtend)
		{
			instance_.maxLength = 5f;
			instance_.targetLength = 5f;
		}
	}

	public static GameEntity SpawnGadgetForPlayer(SIPlayer player, string gadgetName)
	{
		if ((UnityEngine.Object)(object)player == (UnityEngine.Object)null)
		{
			return null;
		}
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		int staticHash = StaticHashExt.GetStaticHash(gadgetName);
		if (!gameEntityManager.FactoryHasEntity(staticHash))
		{
			return null;
		}
		GameEntityId val = gameEntityManager.RequestCreateItem(staticHash, player.gamePlayer.rig.rightHandTransform.position, Quaternion.identity, (long)SIPlayer.LocalPlayer.ActorNr);
		return gameEntityManager.GetGameEntity(val);
	}

	public static List<SIGadget> GetAllActiveGadgets()
	{
		List<SIGadget> list = new List<SIGadget>();
		foreach (GameEntity current in SIM.gameEntityManager.GetGameEntities())
		{
			SIGadget component = ((Component)current).GetComponent<SIGadget>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				list.Add(component);
			}
		}
		return list;
	}

	public static void GunFlingTest()
	{
		if (GunLib.SetupLockOnGun())
		{
			siPlayer = ((Component)GunLib.lockedTargetRig).gameObject.GetComponent<SIPlayer>();
			if (!((UnityEngine.Object)(object)siPlayer != (UnityEngine.Object)null))
			{
				return;
			}
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = false;
			((Component)Variables.taggerInstance.offlineVRRig).transform.position = ((Component)GunLib.lockedTargetRig).transform.position + new Vector3(0f, 0.2f, 0f);
			Variables.taggerInstance.rightHandTransform.position = GunLib.lockedTargetRig.bodyTransform.position;
			Variables.taggerInstance.leftHandTransform.position = GunLib.lockedTargetRig.bodyTransform.position;
			Variables.taggerInstance.leftHandTransform.rotation = Quaternion.Euler(-90f, 0f, 0f);
			Variables.taggerInstance.rightHandTransform.rotation = Quaternion.Euler(-90f, 0f, 0f);
			IEnumerator<SIGadgetBlaster> enumerator = (from x in GetActiveLocalPlayerGadgets()
				select ((Component)x).GetComponent<SIGadgetBlaster>() into x
				where (UnityEngine.Object)(object)x != (UnityEngine.Object)null
				select x).GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					SIGadgetBlaster current = enumerator.Current;
					RemoveBlasterCooldown(current);
					TargetPlayer(current, siPlayer, PosToVector3(Variables.taggerInstance.rightHandTransform.position, ((Component)siPlayer).transform.position, 2f), Float: true);
				}
				while (enumerator.MoveNext());
			}
		}
		else
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
		}
	}

	public static void ForceMaxCharge(SIGadgetChargeBlaster instance)
	{
		if (!((UnityEngine.Object)(object)instance == (UnityEngine.Object)null) && instance.chargeLevels != null && instance.chargeLevels.Length != 0)
		{
			instance.currentCharge = instance.chargeLevels[^1].chargeThreshold + 1f;
		}
	}

	public static void FastCharge(SIGadgetChargeBlaster instance_)
	{
		instance_.maxChargeDiff = 9999f;
		((Component)instance_).SendMessage("OnUpdateAuthority", (object)Time.deltaTime, (SendMessageOptions)1);
	}

	public static void RemoveDashCooldown(SIGadgetDashYoyo instance_)
	{
		if ((int)instance_._state == 1)
		{
			instance_._state = (EState)0;
		}
	}

	public static void CompleteAllQuestsAndClaimPoints(SIPlayer player)
	{
		if ((UnityEngine.Object)(object)player == (UnityEngine.Object)null)
		{
			return;
		}
		ProgressionData currentProgression = player.CurrentProgression;
		SIQuestsList questSourceList = SIProgression.Instance.questSourceList;
		for (int num = 0; num < currentProgression.currentQuestIds.Length; num++)
		{
			RotatingQuest questById = questSourceList.GetQuestById(currentProgression.currentQuestIds[num]);
			if (questById == null)
			{
				continue;
			}
			currentProgression.currentQuestProgresses[num] = questById.requiredOccurenceCount;
			currentProgression.bonusProgress += 100;
			GorillaQuestManager questManager = questById.questManager;
			if (questManager != null)
			{
				questManager.HandleQuestCompleted(questById.questID);
				break;
			}
		}
		player.UpdateProgression(currentProgression.resourceArray, currentProgression.limitedDepositTimeArray, currentProgression.techTreeData, currentProgression.stashedQuests, currentProgression.stashedBonusPoints, currentProgression.bonusProgress, currentProgression.currentQuestIds, currentProgression.currentQuestProgresses);
		SIPlayer.SetAndBroadcastProgression();
	}

	public static List<SIGadget> GetActiveLocalPlayerGadgets()
	{
		List<SIGadget> list = new List<SIGadget>();
		if ((UnityEngine.Object)(object)SIPlayer.LocalPlayer == (UnityEngine.Object)null)
		{
			return list;
		}
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		foreach (int current in SIPlayer.LocalPlayer.activePlayerGadgets)
		{
			GameEntity gameEntity = gameEntityManager.GetGameEntity(gameEntityManager.GetEntityIdFromNetId(current));
			SIGadget val = ((gameEntity != null) ? ((Component)gameEntity).GetComponent<SIGadget>() : null);
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				list.Add(val);
			}
		}
		return list;
	}

	public static void SpawnGadgetAtHand(string gadgetName, bool leftHand = false)
	{
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		int staticHash = StaticHashExt.GetStaticHash(gadgetName);
		if (gameEntityManager.FactoryHasEntity(staticHash))
		{
			if (!leftHand)
			{
				Vector3 position = Variables.taggerInstance.rightHandTransform.position;
				gameEntityManager.RequestCreateItem(staticHash, position, Quaternion.identity, (long)SIPlayer.LocalPlayer.ActorNr);
			}
			else
			{
				Vector3 position = Variables.taggerInstance.leftHandTransform.position;
				gameEntityManager.RequestCreateItem(staticHash, position, Quaternion.identity, (long)SIPlayer.LocalPlayer.ActorNr);
			}
		}
	}

	public static void ExplodeGadgetsOutward(float force)
	{
		Vector3 position = ((Component)VRRig.LocalRig).transform.position;
		List<GameEntity>.Enumerator enumerator = SIM.gameEntityManager.GetGameEntities().GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				GameEntity current = enumerator.Current;
				Rigidbody component = ((Component)current).GetComponent<Rigidbody>();
				component.isKinematic = false;
				Vector3 val = ((Component)current).transform.position - position;
				component.linearVelocity = val.normalized * force;
			}
			while (enumerator.MoveNext());
		}
	}

	public static void KnockbackPlayer(SIPlayer player, Vector3 direction, float strength)
	{
		if (!((UnityEngine.Object)(object)player == (UnityEngine.Object)null))
		{
			player.PlayerKnockback(direction.normalized * strength, true, false);
		}
	}

	public static void ClearAllProjectiles(SIGadgetBlaster blaster)
	{
		foreach (SIGadgetBlasterProjectile current in blaster.activeProjectiles)
		{
			if (!((UnityEngine.Object)(object)current != (UnityEngine.Object)null))
			{
				continue;
			}
			blaster.DespawnProjectile(current);
		}
		blaster.activeProjectiles.Clear();
	}

	public static void ResetPlayerGadgets(SIPlayer player)
	{
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		int num = 0;
		if (num < player.activePlayerGadgets.Count)
		{
			do
			{
				gameEntityManager.RequestDestroyItem(gameEntityManager.GetEntityIdFromNetId(player.activePlayerGadgets[num]));
				num++;
			}
			while (num < player.activePlayerGadgets.Count);
		}
		player.activePlayerGadgets.Clear();
	}

	public static void DeleteAllGadgets()
	{
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			SIPlayer val = SIPlayer.Get(current);
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				SIM.ClearPlayerGadgets(val);
			}
		}
	}

	public static void CompleteAllQuests()
	{
		int[] activeQuestIds = SIProgression.Instance.ActiveQuestIds;
		foreach (int num in activeQuestIds)
		{
			RotatingQuest questById = SIProgression.Instance.questSourceList.GetQuestById(num);
			questById.SetProgress(questById.requiredOccurenceCount);
		}
		SIProgression.Instance.SaveQuestProgress();
	}

	public static void MaxChargeAlways(SIGadgetChargeBlaster instance_)
	{
		instance_.maxChargeDiff = 9999f;
		instance_.currentCharge = instance_.chargeLevels[^1].chargeThreshold + 1f;
	}

	public static void TargetPlayer(SIGadgetBlaster instance_, SIPlayer target, Vector3 velocity, bool Float = false)
	{
		if (instance_.activeProjectiles.Count <= 0)
		{
			return;
		}
		foreach (SIGadgetBlasterProjectile current in instance_.activeProjectiles)
		{
			if ((UnityEngine.Object)(object)current.parentBlaster != (UnityEngine.Object)(object)instance_ || (UnityEngine.Object)(object)current.firedByPlayer != (UnityEngine.Object)(object)SIPlayer.LocalPlayer)
			{
				continue;
			}
			Transform bodyTransform = ((Component)target).gameObject.GetComponent<VRRig>().bodyTransform;
			instance_.firingPosition.position = bodyTransform.position - (Vector3)(Float ? new Vector3(0f, 0.3f, 0f) : Vector3.zero);
			instance_.firingPosition.rotation = Quaternion.LookRotation(bodyTransform.forward, bodyTransform.up);
			current.rb.velocity = velocity;
		}
	}

	public static void TeleportAllGadgetsToPlayer(SIPlayer player)
	{
		if ((UnityEngine.Object)(object)player == (UnityEngine.Object)null)
		{
			return;
		}
		Vector3 position = ((Component)player.gamePlayer.rig).transform.position;
		foreach (GameEntity current in SIM.gameEntityManager.GetGameEntities())
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null || (UnityEngine.Object)(object)((Component)current).GetComponent<SIGadget>() == (UnityEngine.Object)null)
			{
				continue;
			}
			((Component)current).transform.position = position + UnityEngine.Random.insideUnitSphere * 0.5f;
		}
	}

	public static void SpawnGadgetOnAllPlayers(string gadgetName)
	{
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		int staticHash = StaticHashExt.GetStaticHash(gadgetName);
		if (!gameEntityManager.FactoryHasEntity(staticHash))
		{
			return;
		}
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			SIPlayer val = SIPlayer.Get(current);
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				continue;
			}
			gameEntityManager.RequestCreateItem(staticHash, current.rightHandTransform.position, Quaternion.identity, (long)val.ActorNr);
		}
	}

	public static void ClearExclusionZones(SIPlayer player)
	{
		player.exclusionZoneCount = 0;
	}

	public static void UnlockFullTechTree()
	{
		bool[][] unlockedTechTreeData = SIProgression.Instance.unlockedTechTreeData;
		for (int i = 0; i < unlockedTechTreeData.Length; i++)
		{
			int num = 0;
			if (num < unlockedTechTreeData[i].Length)
			{
				do
				{
					unlockedTechTreeData[i][num] = true;
					num++;
				}
				while (num < unlockedTechTreeData[i].Length);
			}
		}
		SIPlayer.SetAndBroadcastProgression();
	}

	public static void UnlockTechNode(SIPlayer player, int tierIndex, int nodeIndex)
	{
		if (!((UnityEngine.Object)(object)player == (UnityEngine.Object)null))
		{
			ProgressionData currentProgression = player.CurrentProgression;
			if (!currentProgression.techTreeData[tierIndex][nodeIndex])
			{
				currentProgression.techTreeData[tierIndex][nodeIndex] = true;
				player.UpdateProgression(currentProgression.resourceArray, currentProgression.limitedDepositTimeArray, currentProgression.techTreeData, currentProgression.stashedQuests, currentProgression.stashedBonusPoints, currentProgression.bonusProgress, currentProgression.currentQuestIds, currentProgression.currentQuestProgresses);
				SIPlayer.SetAndBroadcastProgression();
			}
		}
	}

	public static void AddResources(ResourceType resourceType, int amount)
	{
		if (SIProgression.Instance.resourceDict.ContainsKey(resourceType))
		{
			if ((int)resourceType == 0)
			{
				SIProgression.Instance.resourceDict[resourceType] += amount;
				SIProgression.Instance.AttemptIncrementResource(resourceType);
			}
			else
			{
				SIProgression.Instance.resourceDict[resourceType] += Math.Min(amount, SIProgression.Instance.GetResourceMaxCap(resourceType) - SIProgression.Instance.resourceDict[resourceType]);
				SIProgression.Instance.AttemptIncrementResource(resourceType);
			}
		}
	}

	public static void LaunchAllPlayersUp(float strength)
	{
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			SIPlayer val = SIPlayer.Get(current);
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				val.PlayerKnockback(Vector3.up * strength, true, false);
			}
		}
	}

	public static void ForceBlasterSpamAtTarget(SIPlayer shooter, SIPlayer target, int blastersPerType = 3)
	{
		int[] array = new int[5] { 1312505709, 1469243263, -1529067748, -122499862, -108912318 };
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		Vector3 position = target.gamePlayer.rig.headMesh.transform.position;
		int[] array2 = array;
		foreach (int num in array2)
		{
			int num2 = 0;
			if (num2 < blastersPerType)
			{
				do
				{
					Vector3 val = position + new Vector3(UnityEngine.Random.Range(-5f, 5f), 6f, UnityEngine.Random.Range(-5f, 5f));
					Vector3 val2 = position - val;
					Vector3 normalized = val2.normalized;
					int num3 = gameEntityManager.CreateNetId(1 + gameEntityManager.FactoryGetBuiltInEntityCountById(num));
					gameEntityManager.photonView.RPC("CreateItemRPC", (RpcTarget)0, new object[6]
					{
						new int[1] { num3 },
						new int[1] { num },
						new long[1] { BitPackUtils.PackWorldPosForNetwork(val) },
						new int[1] { BitPackUtils.PackQuaternionForNetwork(Quaternion.LookRotation(normalized)) },
						new long[1],
						new int[1] { -1 }
					});
					gameEntityManager.photonView.RPC("ThrowEntityRPC", (RpcTarget)0, new object[8]
					{
						num3,
						false,
						val,
						Quaternion.LookRotation(normalized),
						normalized * 20f,
						Vector3.zero,
						shooter.gamePlayer.rig.OwningNetPlayer.GetPlayerRef(),
						PhotonNetwork.Time
					});
					num2++;
				}
				while (num2 < blastersPerType);
			}
		}
		PhotonNetwork.SendAllOutgoingCommands();
	}

	public static void ClearAllExclusionZones()
	{
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			SIPlayer val = SIPlayer.Get(current);
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				val.exclusionZoneCount = 0;
			}
		}
		SIPlayer.LocalPlayer.exclusionZoneCount = 0;
	}

	public static void BroadcastFakeProgression(int[] fakeResources, bool[][] fakeTechTree)
	{
		ProgressionData currentProgression = SIPlayer.LocalPlayer.CurrentProgression;
		SIM.CallRPC((ClientToClientRPC)0, new object[8]
		{
			fakeResources,
			new int[2],
			fakeTechTree,
			255,
			255,
			255,
			currentProgression.currentQuestIds,
			currentProgression.currentQuestProgresses
		});
	}

	public static void DeleteAllGadgetsForPlayer(SIPlayer player)
	{
		if (!((UnityEngine.Object)(object)player == (UnityEngine.Object)null))
		{
			SIM.ClearPlayerGadgets(player);
		}
	}

	public static void SetProjectileVelocity(SIGadgetBlaster blaster, Vector3 velocity)
	{
		foreach (SIGadgetBlasterProjectile current in blaster.activeProjectiles)
		{
			if (!((UnityEngine.Object)(object)current != (UnityEngine.Object)null))
			{
				continue;
			}
			current.rb.linearVelocity = velocity;
		}
	}

	private static void RemoveBlasterCooldownInner(SIGadgetBlaster instance_)
	{
		if ((UnityEngine.Object)(object)instance_ == (UnityEngine.Object)null || !instance_.LocalEquippedOrActivated || (!InputHandler.RTrigger() && !InputHandler.LTrigger()))
		{
			return;
		}
		instance_.lastFired = -1f;
		instance_.projectileCount = 0;
		timer += Time.deltaTime;
		if (timer >= 4E-07f)
		{
			do
			{
				timer -= 4E-07f;
				((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat = (InputHandler.RTrigger() ? 0f : 0.6f);
				((ControllerInputPoller)ControllerInputPoller.instance).leftControllerIndexFloat = (InputHandler.LTrigger() ? 0f : 0.6f);
			}
			while (timer >= 4E-07f);
		}
	}

	public static void FreezeAllGadgets()
	{
		foreach (GameEntity current in SIM.gameEntityManager.GetGameEntities())
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null)
			{
				continue;
			}
			Rigidbody component = ((Component)current).GetComponent<Rigidbody>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.isKinematic = true;
			}
		}
	}

	public static GameEntity SpawnGadgetAtPosition(GameObject gadgetPrefab, Vector3 position)
	{
		GameEntityManager activeManager = GameEntityManager.activeManager;
		int staticHash = StaticHashExt.GetStaticHash(((UnityEngine.Object)gadgetPrefab).name);
		GameEntityId val = activeManager.RequestCreateItem(staticHash, position, Quaternion.identity, 0L);
		return activeManager.GetGameEntity(val);
	}

	public static void GiveAllResourcesToMax(SIPlayer player)
	{
		if ((UnityEngine.Object)(object)player == (UnityEngine.Object)null)
		{
			return;
		}
		foreach (ResourceType val in Enum.GetValues(typeof(ResourceType)))
		{
			if (!SIProgression.Instance.resourceDict.ContainsKey(val))
			{
				continue;
			}
			SIProgression.Instance.resourceDict[val] = SIProgression.Instance.GetResourceMaxCap(val);
		}
		SIPlayer.SetAndBroadcastProgression();
	}

	public static void SendFakeIdolCelebration(Vector3 position)
	{
		SIM.CallRPC((AuthorityToClientRPC)5, new object[1] { position });
	}

	public static void RedirectProjectilesToTarget(SIGadgetBlaster blaster, Transform target)
	{
		foreach (SIGadgetBlasterProjectile current in blaster.activeProjectiles)
		{
			if ((UnityEngine.Object)(object)current == (UnityEngine.Object)null)
			{
				continue;
			}
			Rigidbody rb = current.rb;
			Vector3 val = target.position - ((Component)current).transform.position;
			Vector3 val2 = val;
			Vector3 normalized = val2.normalized;
			val = current.rb.linearVelocity;
			val2 = val;
			rb.linearVelocity = normalized * val2.magnitude;
		}
	}

	public static void InfCharges(SIGadgetPlatformDeployer instance_)
	{
		instance_.remainingRechargeTime = 0f;
	}

	public static void MaxStashedQuests()
	{
		SIProgression.Instance.stashedQuests = 255;
		SIPlayer.SetAndBroadcastProgression();
	}

	public static void UnfreezeAllGadgets()
	{
		foreach (GameEntity current in SIM.gameEntityManager.GetGameEntities())
		{
			Rigidbody component = ((Component)current).GetComponent<Rigidbody>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.isKinematic = false;
			}
		}
	}

	public static void ForceFireBlasterAtTarget(SIPlayer shooter, SIPlayer target)
	{
		GameEntityManager gameEntityManager = SIM.gameEntityManager;
		foreach (int current in shooter.activePlayerGadgets)
		{
			GameEntity gameEntityFromNetId = gameEntityManager.GetGameEntityFromNetId(current);
			if ((UnityEngine.Object)(object)gameEntityFromNetId == (UnityEngine.Object)null)
			{
				continue;
			}
			SIGadgetBlaster component = ((Component)gameEntityFromNetId).GetComponent<SIGadgetBlaster>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				continue;
			}
			Vector3 val = target.gamePlayer.rig.headMesh.transform.position - component.firingPosition.position;
			Vector3 normalized = val.normalized;
			SIM.CallRPC((ClientToClientRPC)3, new object[3]
			{
				current,
				0,
				new object[3]
				{
					component.NextFireId(),
					component.firingPosition.position,
					normalized * 25f
				}
			});
		}
	}

	public static void RemoveBlasterCooldown(SIGadgetBlaster instance_)
	{
		if ((UnityEngine.Object)(object)instance_ == (UnityEngine.Object)null || !instance_.LocalEquippedOrActivated || (!InputHandler.RTrigger() && !InputHandler.LTrigger()))
		{
			return;
		}
		instance_.lastFired = -1f;
		instance_.projectileCount = 0;
		timer += Time.deltaTime;
		if (timer >= 3E-07f)
		{
			do
			{
				timer -= 3E-07f;
				((ControllerInputPoller)ControllerInputPoller.instance).rightControllerIndexFloat = (InputHandler.RTrigger() ? 0f : 0.6f);
				((ControllerInputPoller)ControllerInputPoller.instance).leftControllerIndexFloat = (InputHandler.LTrigger() ? 0f : 0.6f);
			}
			while (timer >= 3E-07f);
		}
		RemoveBlasterCooldownInner(instance_);
		RemoveBlasterCooldownInner(instance_);
	}

	public static void RequestEntityStateSpam(SIGadget gadget, long state, int times)
	{
		if ((UnityEngine.Object)(object)gadget == (UnityEngine.Object)null)
		{
			return;
		}
		int num = 0;
		if (num < times)
		{
			do
			{
				gadget.gameEntity.RequestState(gadget.gameEntity.id, state);
				num++;
			}
			while (num < times);
		}
	}
}
