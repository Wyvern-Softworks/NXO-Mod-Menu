using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Controller = OVRInput.Controller;

namespace NXO.Mods.Categories;

public static class StevesPlayground
{
	public class DisplacerCannon : MonoBehaviour
	{
		public class Projectile : MonoBehaviour
		{
			private bool hasExploded = false;

			public void OnCollisionEnter(Collision collision)
			{
				if (hasExploded)
				{
					return;
				}
				int mask = LayerMask.GetMask(new string[3] { "Gorilla UnityEngine.Object", "Default", "NoMirror" });
				if ((mask & (1 << collision.gameObject.layer)) != 0)
				{
					hasExploded = true;
					NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonExplosion, new object[3]
					{
						((Component)this).transform.position.x,
						((Component)this).transform.position.y,
						((Component)this).transform.position.z
					});
					if ((UnityEngine.Object)(object)displacerCannonBoomPrefab != (UnityEngine.Object)null)
					{
						GameObject val = UnityEngine.Object.Instantiate<GameObject>(displacerCannonBoomPrefab, ((Component)this).transform.position, Quaternion.identity);
						UnityEngine.Object.Destroy((UnityEngine.Object)(object)val, 4f);
						UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)this).gameObject, 0.001f);
					}
					else
					{
						UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)this).gameObject, 0.001f);
					}
				}
			}
		}

		public static bool isActive;

		public AudioSource audioSource;

		public AudioClip shootSound;

		public AudioClip chargeSound;

		public Animator animator;

		public ParticleSystem shoot;

		public ParticleSystem charge;

		public bool isCharging = false;

		public IEnumerator Charge()
		{
			if (isCharging)
			{
				yield break;
			}
			isCharging = true;
			Transform chargeBone = FindBone("Charge", ((Component)this).transform);
			NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonCharge, new object[3]
			{
				chargeBone.position.x,
				chargeBone.position.y,
				chargeBone.position.z
			});
			float duration = 1f;
			float elapsed = 0f;
			charge.Play();
			if ((UnityEngine.Object)(object)animator != (UnityEngine.Object)null)
			{
				animator.speed = 0f;
			}
			if ((UnityEngine.Object)(object)audioSource != (UnityEngine.Object)null && (UnityEngine.Object)(object)chargeSound != (UnityEngine.Object)null)
			{
				audioSource.volume = 0.1f;
				audioSource.PlayOneShot(chargeSound);
			}
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				if ((UnityEngine.Object)(object)animator != (UnityEngine.Object)null)
				{
					animator.speed = Mathf.Lerp(0f, 1f, elapsed / duration);
				}
				yield return null;
			}
			if ((UnityEngine.Object)(object)animator != (UnityEngine.Object)null)
			{
				animator.speed = 1f;
			}
			charge.Stop();
			Shoot();
			yield return new WaitForSeconds(0.2f);
			isCharging = false;
		}

		public void Update()
		{
			if (!isActive)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)this).gameObject);
			}
			else
			{
				if (isCharging)
				{
					return;
				}
				if (!InputHandler.RTrigger())
				{
					if (!((ButtonControl)Keyboard.current.enterKey).isPressed)
					{
						return;
					}
				}
				else if (1 == 0)
				{
					return;
				}
				((MonoBehaviour)this).StartCoroutine(Charge());
			}
		}

		public void Shoot()
		{
			Transform val = FindBone("Launch", ((Component)this).transform);
			NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonShoot, new object[6]
			{
				val.position.x,
				val.position.y,
				val.position.z,
				(-val.up).x,
				(-val.up).y,
				(-val.up).z
			});
			if ((UnityEngine.Object)(object)audioSource != (UnityEngine.Object)null && (UnityEngine.Object)(object)shootSound != (UnityEngine.Object)null)
			{
				audioSource.volume = 0.15f;
				audioSource.PlayOneShot(shootSound);
			}
			if ((UnityEngine.Object)(object)shoot != (UnityEngine.Object)null)
			{
				shoot.Play();
			}
			if ((UnityEngine.Object)(object)displacerCannonProjectilePrefab == (UnityEngine.Object)null)
			{
				return;
			}
			GameObject val2 = UnityEngine.Object.Instantiate<GameObject>(displacerCannonProjectilePrefab, val.position, val.rotation);
			val2.AddComponent<Projectile>();
			Rigidbody component = val2.GetComponent<Rigidbody>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.AddForce(-val.up * 30f, (ForceMode)2);
			}
		}
	}

	public class JetPack : MonoBehaviour
	{
		public static bool isActive;

		public ParticleSystem thrusterEffect;

		public AudioSource source;

		public void Update()
		{
			if (!isActive)
			{
				currentJetPack.SetActive(false);
				return;
			}
			currentJetPack.SetActive(true);
			if ((InputHandler.LTrigger() && InputHandler.RTrigger()) || ((ButtonControl)Keyboard.current.spaceKey).isPressed)
			{
				Rigidbody rigidbody = Variables.taggerInstance.rigidbody;
				if (!((UnityEngine.Object)(object)rigidbody != (UnityEngine.Object)null))
				{
					return;
				}
				source.pitch = 0.8f + Variables.taggerInstance.rigidbody.velocity.y / 20f;
				rigidbody.AddForce(Vector3.up * 1500f, (ForceMode)0);
				thrusterEffect.Play();
				Vector3 position = ((Component)thrusterEffect).transform.position;
				NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.JetPackThrust, new object[4] { position.x, position.y, position.z, source.pitch });
				Vector2 joystickAxis = InputHandler.GetJoystickAxis(left: false);
				if (joystickAxis.magnitude <= 0.1f && Keyboard.current != null)
				{
					if (((ButtonControl)Keyboard.current.upArrowKey).isPressed)
					{
						joystickAxis.y += 1f;
					}
					if (((ButtonControl)Keyboard.current.downArrowKey).isPressed)
					{
						joystickAxis.y -= 1f;
					}
					if (((ButtonControl)Keyboard.current.leftArrowKey).isPressed)
					{
						joystickAxis.x -= 1f;
					}
					if (((ButtonControl)Keyboard.current.rightArrowKey).isPressed)
					{
						joystickAxis.x += 1f;
					}
				}
				if (GTPlayer.Instance.IsGroundedButt)
				{
					return;
				}
				if (joystickAxis.magnitude > 0.1f && (UnityEngine.Object)(object)rigidbody != (UnityEngine.Object)null)
				{
					Vector3 forward = Variables.taggerInstance.offlineVRRig.bodyTransform.forward;
					Vector3 right = Variables.taggerInstance.offlineVRRig.bodyTransform.right;
					Vector3 val = forward * joystickAxis.y + right * joystickAxis.x;
					Vector3 normalized = val.normalized;
					UnityEngine.Debug.Log((object)$"JetPack Move Direction: {normalized}");
					rigidbody.AddForce(normalized * 1500f, (ForceMode)0);
				}
			}
			else
			{
				thrusterEffect.Stop();
				source.pitch = 0f;
			}
		}
	}

	public class WebSling : MonoBehaviour
	{
		public static bool isActive;

		public LineRenderer webLine;

		public SpringJoint? joint;

		public bool isLeftHand;

		private Vector3 hitPoint;

		private bool isExtending;

		private bool isRecoiling;

		private float currentLength;

		private float targetLength;

		private float recoilTime;

		public void DrawCurvedWeb(Vector3 start, Vector3 end)
		{
			int positionCount = webLine.positionCount;
			int num = 0;
			if (num < positionCount)
			{
				do
				{
					float num2 = (float)num / (float)(positionCount - 1);
					float num3 = Mathf.Sin(num2 * MathF.PI) * 0.3f * Vector3.Distance(start, end);
					Vector3 val = Vector3.down * num3;
					Vector3 val2 = Vector3.Lerp(start, end, num2) + val;
					webLine.SetPosition(num, val2);
					num++;
				}
				while (num < positionCount);
			}
		}

		public void Update()
		{
			if (!isActive)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)this).gameObject);
				return;
			}
			if ((UnityEngine.Object)(object)webLine == (UnityEngine.Object)null)
			{
				UnityEngine.Debug.LogError((object)"WebSling: webLine is null! Make sure the prefab has a LineRenderer.");
				return;
			}
			bool flag = isLeftHand ? InputHandler.LTrigger() : InputHandler.RTrigger();
			Transform val = isLeftHand ? Variables.taggerInstance.offlineVRRig.leftHandTransform : Variables.taggerInstance.offlineVRRig.rightHandTransform;
			if (flag)
			{
				if ((UnityEngine.Object)(object)joint == (UnityEngine.Object)null)
				{
					RaycastHit val2 = default(RaycastHit);
					hitPoint = Physics.Raycast(val.position, val.forward, out val2, 50f) ? val2.point : val.position + val.forward * 16f;
					joint = ((Component)Variables.taggerInstance).gameObject.AddComponent<SpringJoint>();
					((Joint)joint).autoConfigureConnectedAnchor = false;
					((Joint)joint).connectedAnchor = hitPoint;
					float num = Vector3.Distance(Variables.taggerInstance.rigidbody.position, hitPoint);
					joint.maxDistance = num * 0.8f;
					joint.minDistance = num * 0.25f;
					joint.spring = 5000f;
					joint.damper = 4000f;
					((Joint)joint).massScale = 5f;
					webLine.positionCount = 20;
					currentLength = 0f;
					targetLength = num;
					isExtending = true;
					isRecoiling = false;
					recoilTime = 0f;
				}
				else
				{
					UpdateWebLine();
					Vector3 localControllerVelocity = OVRInput.GetLocalControllerVelocity(isLeftHand ? (Controller)1 : (Controller)2);
					if (localControllerVelocity.magnitude >= 2.5f)
					{
						if ((UnityEngine.Object)(object)joint == (UnityEngine.Object)null)
						{
							UnityEngine.Debug.LogWarning((object)"WebSling Joint is null during pull attempt.");
							return;
						}
						Vector3 val3 = ((Joint)joint).connectedAnchor - ((Component)Variables.taggerInstance).transform.position;
						Vector3 val4 = val3.normalized * 4.5f;
						Variables.taggerInstance.rigidbody.AddForce(val4, (ForceMode)2);
						UnityEngine.Debug.Log((object)"WebSling Pulling Photon.Realtime.Player Towards Web Point!");
					}
				}
				if (isExtending)
				{
					AnimateWebExtend(val);
				}
				if (isRecoiling)
				{
					AnimateRecoil(val);
				}
			}
			else
			{
				if ((UnityEngine.Object)(object)joint != (UnityEngine.Object)null)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)joint);
					joint = null;
				}
				isExtending = false;
				isRecoiling = false;
				currentLength = 0f;
				targetLength = 0f;
				webLine.positionCount = 0;
			}
		}

		public void UpdateWebLine()
		{
			if (!((UnityEngine.Object)(object)joint == (UnityEngine.Object)null) && !((UnityEngine.Object)(object)webLine == (UnityEngine.Object)null) && !((UnityEngine.Object)(object)Variables.taggerInstance == (UnityEngine.Object)null) && !((UnityEngine.Object)(object)Variables.taggerInstance.offlineVRRig == (UnityEngine.Object)null))
			{
				if (!isLeftHand)
				{
					Transform rightHandTransform = Variables.taggerInstance.offlineVRRig.rightHandTransform;
					DrawCurvedWeb(rightHandTransform.position, ((Joint)joint).connectedAnchor);
				}
				else
				{
					Transform rightHandTransform = Variables.taggerInstance.offlineVRRig.leftHandTransform;
					DrawCurvedWeb(rightHandTransform.position, ((Joint)joint).connectedAnchor);
				}
			}
		}

		public void AnimateRecoil(Transform hand)
		{
			recoilTime += Time.deltaTime * 10f;
			float num = Mathf.Sin(recoilTime) * 0.2f * (1f - Mathf.Clamp01(recoilTime / 2f));
			Vector3 val = hitPoint - hand.position;
			Vector3 normalized = val.normalized;
			Vector3 end = hitPoint + normalized * num;
			DrawCurvedWeb(hand.position, end);
			if (recoilTime > MathF.PI)
			{
				isRecoiling = false;
				DrawCurvedWeb(hand.position, hitPoint);
			}
		}

		public void AnimateWebExtend(Transform hand)
		{
			currentLength = Mathf.MoveTowards(currentLength, targetLength, Time.deltaTime * 70f);
			Vector3 val = hitPoint - hand.position;
			Vector3 normalized = val.normalized;
			Vector3 end = hand.position + normalized * currentLength;
			DrawCurvedWeb(hand.position, end);
			if (Mathf.Abs(currentLength - targetLength) < 0.05f)
			{
				isExtending = false;
				isRecoiling = true;
				recoilTime = 0f;
			}
		}
	}

	public enum FuncType
	{
		DisplacerCannon,
		Jetpack
	}

	public static GameObject displacerCannonObject;

	public static GameObject displacerCannonProjectilePrefab;

	public static GameObject displacerCannonBoomPrefab;

	public static GameObject jetpackPrefab;

	public static GameObject currentJetPack;

	public static GameObject webSlingPrefab;

	public static GameObject leftWebSling;

	public static GameObject rightWebSling;

	public static Dictionary<string, AudioClip> displacerCannonAudioClips = new Dictionary<string, AudioClip>();

	public static AssetBundle AssetBundle;

	private static List<Transform> boneSearchList = new List<Transform>(32);

	public static Transform FindBone(string name, Transform targ)
	{
		boneSearchList.Clear();
		boneSearchList.Add(targ);
		if (boneSearchList.Count > 0)
		{
			do
			{
				Transform val = boneSearchList[boneSearchList.Count - 1];
				boneSearchList.RemoveAt(boneSearchList.Count - 1);
				if (((UnityEngine.Object)val).name == name)
				{
					return val;
				}
				int num = 0;
				int childCount = val.childCount;
				if (num < childCount)
				{
					do
					{
						boneSearchList.Add(val.GetChild(num));
						num++;
					}
					while (num < childCount);
				}
			}
			while (boneSearchList.Count > 0);
		}
		return null;
	}

	public static void Manager(FuncType type, bool add)
	{
		if (add)
		{
			Runnable(type);
		}
		else
		{
			Removeable(type);
		}
	}

	public static void Removeable(FuncType type)
	{
		switch (type)
		{
		case FuncType.DisplacerCannon:
			if (DisplacerCannon.isActive)
			{
				DisplacerCannon.isActive = false;
				NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonUnequip, new object[0]);
				UnityEngine.Debug.Log((object)"Displacer Cannon Deactivated!");
			}
			break;
		case FuncType.Jetpack:
			if (JetPack.isActive)
			{
				JetPack.isActive = false;
				NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.JetPackUnequip, new object[0]);
				UnityEngine.Debug.Log((object)"JetPack Deactivated!");
			}
			break;
		}
	}

	public static void Runnable(FuncType type)
	{
		if ((UnityEngine.Object)(object)AssetBundle == (UnityEngine.Object)null)
		{
			AssetBundle = AssetHandler.LoadAssetBundle("NXO.Resources.nxostevestuff");
			displacerCannonObject = AssetBundle.LoadAsset<GameObject>("Displacer_P");
			displacerCannonProjectilePrefab = AssetBundle.LoadAsset<GameObject>("DisplacerShot_P");
			displacerCannonBoomPrefab = AssetBundle.LoadAsset<GameObject>("DisplacerCumshot_P");
			jetpackPrefab = AssetBundle.LoadAsset<GameObject>("JetPack_P");
			currentJetPack = UnityEngine.Object.Instantiate<GameObject>(jetpackPrefab);
			((UnityEngine.Object)currentJetPack).name = "JetPack";
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)currentJetPack.GetComponent<Rigidbody>());
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)currentJetPack.GetComponent<Collider>());
			currentJetPack.transform.SetParent(((Component)Variables.taggerInstance.offlineVRRig.bodyRenderer).transform, true);
			currentJetPack.transform.localPosition = new Vector3(0f, -0.2659f, -0.1716f);
			currentJetPack.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
			currentJetPack.SetActive(false);
			currentJetPack.AddComponent<JetPack>().thrusterEffect = ((Component)currentJetPack.transform.GetChild(1)).gameObject.GetComponent<ParticleSystem>();
			currentJetPack.GetComponent<JetPack>().source = currentJetPack.GetComponent<AudioSource>();
			displacerCannonAudioClips = new Dictionary<string, AudioClip>();
			AudioClip[] array = AssetBundle.LoadAllAssets<AudioClip>();
			int num = 0;
			while (num < array.Length)
			{
				AudioClip val = array[num];
				if (((UnityEngine.Object)val).name.StartsWith("displacer_"))
				{
					displacerCannonAudioClips[((UnityEngine.Object)val).name] = val;
					UnityEngine.Debug.Log((object)("Loaded Displacer Cannon Audio Clip: " + ((UnityEngine.Object)val).name));
					num++;
				}
				else
				{
					num++;
				}
			}
			webSlingPrefab = AssetBundle.LoadAsset<GameObject>("Websling_P");
		}
		if (type == FuncType.DisplacerCannon)
		{
			if (!DisplacerCannon.isActive)
			{
				DisplacerCannon.isActive = true;
				UnityEngine.Debug.Log((object)"Displacer Cannon Activated!");
				NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonEquip, new object[0]);
				GameObject val2 = UnityEngine.Object.Instantiate<GameObject>(displacerCannonObject);
				DisplacerCannon displacerCannon = val2.AddComponent<DisplacerCannon>();
				displacerCannon.shootSound = displacerCannonAudioClips["displacer_self"];
				displacerCannon.chargeSound = displacerCannonAudioClips["displacer_spin"];
				displacerCannon.audioSource = val2.AddComponent<AudioSource>();
				displacerCannon.animator = val2.GetComponentInChildren<Animator>();
				displacerCannon.shoot = ((Component)FindBone("Launch", val2.transform)).GetComponent<ParticleSystem>();
				displacerCannon.charge = ((Component)FindBone("Charge", val2.transform)).GetComponent<ParticleSystem>();
				((Component)displacerCannon).transform.SetParent(Variables.taggerInstance.offlineVRRig.rightHandTransform, true);
				((Component)displacerCannon).transform.localPosition = new Vector3(0.0472f, -0.0124f, -0.0393f);
				((Component)displacerCannon).transform.localRotation = Quaternion.Euler(284.5566f, 0f, 270f);
			}
			return;
		}
		if (type != FuncType.Jetpack || JetPack.isActive)
		{
			return;
		}
		JetPack.isActive = true;
		NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.JetPackEquip, new object[0]);
		currentJetPack.SetActive(true);
		Renderer[] componentsInChildren = currentJetPack.GetComponentsInChildren<Renderer>();
		int num2 = 0;
		while (num2 < componentsInChildren.Length)
		{
			Renderer val3 = componentsInChildren[num2];
			if ((UnityEngine.Object)(object)val3 != (UnityEngine.Object)null && !(val3 is ParticleSystemRenderer))
			{
				Material[] materials = val3.materials;
				foreach (Material val4 in materials)
				{
					val4.color = Color.black;
				}
				num2++;
			}
			else
			{
				num2++;
			}
		}
		UnityEngine.Debug.Log((object)"JetPack Activated!");
	}
}
