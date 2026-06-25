using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GorillaExtensions;
using POpusCodec.Enums;
using Photon.Pun;
using Photon.Voice;
using Photon.Voice.Unity;
using Photon.Voice.Unity.UtilityScripts;
using UnityEngine;
using InputSourceType = Photon.Voice.Unity.Recorder.InputSourceType;

namespace NXO.Mods.Categories;

public class Sound
{
	public class MicPitchShifter : VoiceComponent
	{
		public class PitchProcessor : IProcessor<float>, IDisposable
		{
			private readonly float pitch;

			public float[] Process(float[] buf)
			{
				int num = buf.Length;
				float[] array = new float[num];
				float num2 = 0f;
				int num3 = 0;
				if (num3 < num)
				{
					do
					{
						int num4 = Mathf.FloorToInt(num2);
						int num5 = Mathf.Min(num4 + 1, num - 1);
						float num6 = num2 - (float)num4;
						float num7 = Mathf.Lerp(buf[num4], buf[num5], num6);
						array[num3] = num7;
						num2 += pitch;
						if (num2 >= (float)(num - 1))
						{
							break;
						}
						num3++;
					}
					while (num3 < num);
				}
				return array;
			}

			public void Dispose()
			{
			}

			public PitchProcessor(float pitchFactor)
			{
				pitch = Mathf.Clamp(pitchFactor, 0.5f, 2f);
			}
		}

		public float PitchFactor = 1.5f;

		public PitchProcessor floatProcessor;

		public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
		{
			LocalVoice voice = p.Voice;
			LocalVoiceAudioFloat val = (LocalVoiceAudioFloat)(object)((voice is LocalVoiceAudioFloat) ? voice : null);
			if (val != null)
			{
				floatProcessor = new PitchProcessor(PitchFactor);
				((LocalVoiceFramed<float>)(object)val).AddPostProcessor(new IProcessor<float>[1] { floatProcessor });
			}
		}
	}

	public class EchoEffect : VoiceComponent
	{
		public class EchoProcessor : IProcessor<float>, IDisposable
		{
			private readonly float decay;

			private readonly float[] buffer;

			private int bufferIndex = 0;

			private const int sampleRate = 48000;

			public EchoProcessor(float delaySeconds, float decayAmount)
			{
				decay = Mathf.Clamp01(decayAmount);
				buffer = new float[Mathf.CeilToInt(48000f * delaySeconds)];
			}

			public void Dispose()
			{
			}

			public float[] Process(float[] buf)
			{
				float[] array = new float[buf.Length];
				int num = 0;
				if (num < buf.Length)
				{
					do
					{
						float num2 = buffer[bufferIndex];
						float num3 = buf[num] + num2 * decay;
						array[num] = Mathf.Clamp(num3, -1f, 1f);
						buffer[bufferIndex] = num3;
						bufferIndex = (bufferIndex + 1) % buffer.Length;
						num++;
					}
					while (num < buf.Length);
				}
				return array;
			}
		}

		public float echoDelay = 0.3f;

		public float echoDecay = 0.6f;

		private EchoProcessor processor;

		public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
		{
			LocalVoice voice = p.Voice;
			LocalVoiceAudioFloat val = (LocalVoiceAudioFloat)(object)((voice is LocalVoiceAudioFloat) ? voice : null);
			if (val != null)
			{
				processor = new EchoProcessor(echoDelay, echoDecay);
				((LocalVoiceFramed<float>)(object)val).AddPostProcessor(new IProcessor<float>[1] { processor });
			}
		}
	}

	public class RadioEffect : VoiceComponent
	{
		public class RadioProcessor : IProcessor<float>, IDisposable
		{
			private float phase = 0f;

			private const int sampleRate = 48000;

			public float[] Process(float[] buf)
			{
				float[] array = new float[buf.Length];
				for (int num = 0; num < buf.Length; num++)
				{
					float num2 = buf[num];
					if (num > 0)
					{
						num2 -= buf[num - 1] * 0.9f;
					}
					float num3 = (UnityEngine.Random.value - 0.5f) * 0.05f;
					array[num] = Mathf.Clamp(num2 + num3, -1f, 1f);
					phase += 2.0833333E-05f;
				}
				return array;
			}

			public void Dispose()
			{
			}
		}

		private RadioProcessor processor;

		public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
		{
			LocalVoice voice = p.Voice;
			LocalVoiceAudioFloat val = (LocalVoiceAudioFloat)(object)((voice is LocalVoiceAudioFloat) ? voice : null);
			if (val != null)
			{
				processor = new RadioProcessor();
				((LocalVoiceFramed<float>)(object)val).AddPostProcessor(new IProcessor<float>[1] { processor });
			}
		}
	}

	public class UnderwaterEffect : VoiceComponent
	{
		public class UnderwaterProcessor : IProcessor<float>, IDisposable
		{
			private float[] lowPassBuffer = new float[5];

			private int bufferIndex = 0;

			public void Dispose()
			{
			}

			public float[] Process(float[] buf)
			{
				float[] array = new float[buf.Length];
				int num = 0;
				if (num < buf.Length)
				{
					do
					{
						lowPassBuffer[bufferIndex] = buf[num];
						float num2 = 0f;
						int num3 = 0;
						if (num3 < lowPassBuffer.Length)
						{
							do
							{
								num2 += lowPassBuffer[num3];
								num3++;
							}
							while (num3 < lowPassBuffer.Length);
						}
						array[num] = num2 / (float)lowPassBuffer.Length * 0.7f;
						bufferIndex = (bufferIndex + 1) % lowPassBuffer.Length;
						num++;
					}
					while (num < buf.Length);
				}
				return array;
			}
		}

		private UnderwaterProcessor processor;

		public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
		{
			LocalVoice voice = p.Voice;
			LocalVoiceAudioFloat val = (LocalVoiceAudioFloat)(object)((voice is LocalVoiceAudioFloat) ? voice : null);
			if (val != null)
			{
				processor = new UnderwaterProcessor();
				((LocalVoiceFramed<float>)(object)val).AddPostProcessor(new IProcessor<float>[1] { processor });
			}
		}
	}

	public class ReverbEffect : VoiceComponent
	{
		public class ReverbProcessor : IProcessor<float>, IDisposable
		{
			private readonly float amount;

			private float[] buffer1;

			private float[] buffer2;

			private float[] buffer3;

			private int index1 = 0;

			private int index2 = 0;

			private int index3 = 0;

			public void Dispose()
			{
			}

			public float[] Process(float[] buf)
			{
				float[] array = new float[buf.Length];
				int num = 0;
				if (num < buf.Length)
				{
					do
					{
						float num2 = buffer1[index1];
						float num3 = buffer2[index2];
						float num4 = buffer3[index3];
						float num5 = (num2 + num3 + num4) / 3f * amount;
						array[num] = buf[num] + num5 * 0.4f;
						buffer1[index1] = buf[num] + num2 * 0.5f;
						buffer2[index2] = buf[num] + num3 * 0.6f;
						buffer3[index3] = buf[num] + num4 * 0.7f;
						index1 = (index1 + 1) % buffer1.Length;
						index2 = (index2 + 1) % buffer2.Length;
						index3 = (index3 + 1) % buffer3.Length;
						num++;
					}
					while (num < buf.Length);
				}
				return array;
			}

			public ReverbProcessor(float reverbAmount)
			{
				amount = Mathf.Clamp01(reverbAmount);
				buffer1 = new float[1789];
				buffer2 = new float[2357];
				buffer3 = new float[3137];
			}
		}

		public float reverbAmount = 0.5f;

		private ReverbProcessor processor;

		public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
		{
			LocalVoice voice = p.Voice;
			LocalVoiceAudioFloat val = (LocalVoiceAudioFloat)(object)((voice is LocalVoiceAudioFloat) ? voice : null);
			if (val != null)
			{
				processor = new ReverbProcessor(reverbAmount);
				((LocalVoiceFramed<float>)(object)val).AddPostProcessor(new IProcessor<float>[1] { processor });
			}
		}
	}

	public class SquareWaveEffect : VoiceComponent
	{
		public class SquareProcessor : IProcessor<float>, IDisposable
		{
			private float phase = 0f;

			private const float SAMPLE_RATE = 48000f;

			private const float FREQUENCY = 3500f;

			public void Dispose()
			{
			}

			public float[] Process(float[] buf)
			{
				float[] array = new float[buf.Length];
				for (int num = 0; num < buf.Length; num++)
				{
					phase += 7f / 96f;
					if (phase >= 1f)
					{
						phase -= 1f;
					}
					array[num] = ((phase < 0.5f) ? 1f : (-1f)) * 0.5f;
				}
				return array;
			}
		}

		private SquareProcessor processor;

		public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
		{
			LocalVoice voice = p.Voice;
			LocalVoiceAudioFloat val = (LocalVoiceAudioFloat)(object)((voice is LocalVoiceAudioFloat) ? voice : null);
			if (val != null)
			{
				processor = new SquareProcessor();
				((LocalVoiceFramed<float>)(object)val).AddPostProcessor(new IProcessor<float>[1] { processor });
			}
		}
	}

	public class AutotuneEffect : VoiceComponent
	{
		public class AutotuneProcessor : IProcessor<float>, IDisposable
		{
			private const int SAMPLE_RATE = 48000;

			private const float MIN_FREQ = 80f;

			private const float MAX_FREQ = 1100f;

			private readonly float[] pitchBuffer = new float[2048];

			private static readonly float[] Notes = GenerateNotes();

			private float SnapToNote(float freq)
			{
				float result = Notes[0];
				float num = Mathf.Abs(freq - Notes[0]);
				float[] notes = Notes;
				int num2 = 0;
				while (num2 < notes.Length)
				{
					float num3 = notes[num2];
					float num4 = Mathf.Abs(freq - num3);
					if (num4 < num)
					{
						num = num4;
						result = num3;
						num2++;
					}
					else
					{
						num2++;
					}
				}
				return result;
			}

			public float[] Process(float[] buf)
			{
				float num = DetectPitch(buf);
				if (num < 0f)
				{
					return buf;
				}
				float num2 = SnapToNote(num);
				float num3 = num2 / num;
				float num4 = num3;
				num3 = Mathf.Clamp(num4, 0.5f, 2f);
				num4 = num3;
				float[] array = new float[buf.Length];
				for (int num5 = 0; num5 < buf.Length; num5++)
				{
					float num6 = (float)num5 * num4;
					int num7 = Mathf.FloorToInt(num6);
					int num8 = Mathf.Min(num7 + 1, buf.Length - 1);
					if (num7 >= buf.Length)
					{
						array[num5] = buf[^1];
					}
					else
					{
						array[num5] = Mathf.Lerp(buf[num7], buf[num8], num6 - (float)num7);
					}
				}
				return array;
			}

			public void Dispose()
			{
			}

			private static float[] GenerateNotes()
			{
				float[] array = new float[128];
				int num = 0;
				if (num < 128)
				{
					do
					{
						array[num] = 440f * Mathf.Pow(2f, (float)(num - 69) / 12f);
						num++;
					}
					while (num < 128);
				}
				return array;
			}

			private float DetectPitch(float[] buf)
			{
				int num = 43;
				int num2 = 600;
				int num3 = num2;
				num2 = Mathf.Min(num3, buf.Length / 2);
				num3 = num2;
				float num4 = -1f;
				int num5 = num;
				for (int num6 = num; num6 < num3; num6++)
				{
					float num7 = 0f;
					float num8 = 0f;
					int num9 = 0;
					if (num9 < buf.Length - num6)
					{
						do
						{
							num7 += buf[num9] * buf[num9 + num6];
							num8 += buf[num9] * buf[num9] + buf[num9 + num6] * buf[num9 + num6];
							num9++;
					}
					while (num9 < buf.Length - num6);
					}
					if (num8 > 0f)
					{
						num7 = 2f * num7 / num8;
					}
					if (num7 > num4)
					{
						num4 = num7;
						num5 = num6;
					}
				}
				if (!(num4 > 0.4f))
				{
					return -1f;
				}
				return 48000f / (float)num5;
			}
		}

		private AutotuneProcessor processor;

		public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
		{
			LocalVoice voice = p.Voice;
			LocalVoiceAudioFloat val = (LocalVoiceAudioFloat)(object)((voice is LocalVoiceAudioFloat) ? voice : null);
			if (val != null)
			{
				processor = new AutotuneProcessor();
				((LocalVoiceFramed<float>)(object)val).AddPostProcessor(new IProcessor<float>[1] { processor });
			}
		}
	}

	public static bool haschangedint = false;

	public static bool haschangedint2 = false;

	public static int sound = 0;

	private static bool lastLeftTouch = false;

	private static bool lastRightTouch = false;

	private static float soundCooldown = 0.175f;

	public static void SetMicPitch(float pitch)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (!Mathf.Approximately(pitch, 1f))
		{
			MicPitchShifter component = ((Component)myRecorder).gameObject.GetComponent<MicPitchShifter>();
			if (!((UnityEngine.Object)(object)component != (UnityEngine.Object)null) || !Mathf.Approximately(component.PitchFactor, pitch))
			{
				MicPitchShifter orAddComponent = GTExt.GetOrAddComponent<MicPitchShifter>(((Component)myRecorder).gameObject);
				orAddComponent.PitchFactor = pitch;
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
		}
		else if (((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<MicPitchShifter>() != (UnityEngine.Object)null))
		{
			MicPitchShifter component2 = ((Component)myRecorder).gameObject.GetComponent<MicPitchShifter>();
			((Behaviour)component2).enabled = false;
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<MicPitchShifter>());
			((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
		}
	}

	public static void MuteMic(bool mute)
	{
		if (PhotonNetwork.InRoom)
		{
			Recorder myRecorder = GorillaTagger.Instance.myRecorder;
			if (myRecorder.IsRecording == mute)
			{
				myRecorder.IsRecording = !mute;
			}
		}
	}

	public static void OnTouchPlayerSoundSpam(int SoundIndex)
	{
		if (Time.time < Variables.rpcCooldown)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		float num = 0.325f;
		foreach (VRRig current in VRRigCache.ActiveRigs)
		{
			if (!RigManager.IsOtherPlayer(current))
			{
				continue;
			}
			if (!flag)
			{
				flag = Vector3.Distance(Variables.taggerInstance.leftHandTransform.position, current.headMesh.transform.position) < num;
			}
			if (!flag2)
			{
				flag2 = Vector3.Distance(Variables.taggerInstance.rightHandTransform.position, current.headMesh.transform.position) < num;
			}
			if (flag && flag2)
			{
				break;
			}
		}
		if (flag && !lastLeftTouch)
		{
			Variables.rpcCooldown = Time.time + soundCooldown;
			Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlayHandTap", (RpcTarget)0, new object[3] { SoundIndex, true, 99999f });
			Safety.RPCShield();
		}
		if (flag2 && !lastRightTouch)
		{
			Variables.rpcCooldown = Time.time + soundCooldown;
			Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlayHandTap", (RpcTarget)0, new object[3] { SoundIndex, false, 99999f });
			Safety.RPCShield();
		}
		lastLeftTouch = flag;
		lastRightTouch = flag2;
	}

	public static void RadioVoice(bool enable)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (enable)
		{
			if (!((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<RadioEffect>() != (UnityEngine.Object)null))
			{
				GTExt.GetOrAddComponent<RadioEffect>(((Component)myRecorder).gameObject);
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
		}
		else if (((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<RadioEffect>() != (UnityEngine.Object)null))
		{
			RadioEffect component = ((Component)myRecorder).gameObject.GetComponent<RadioEffect>();
			((Behaviour)component).enabled = false;
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<RadioEffect>());
			((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
		}
	}

	public static void SetMicQuality(int bitrate, int samplingRate)
	{
		if (PhotonNetwork.InRoom)
		{
			Recorder myRecorder = GorillaTagger.Instance.myRecorder;
			if ((int)myRecorder.SamplingRate != samplingRate || myRecorder.Bitrate != bitrate)
			{
				myRecorder.SamplingRate = (SamplingRate)samplingRate;
				myRecorder.Bitrate = bitrate;
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
		}
	}

	public static void ResetMic()
	{
		GorillaTagger taggerInstance = Variables.taggerInstance;
		Recorder val = ((taggerInstance != null) ? taggerInstance.myRecorder : null);
		if (!((UnityEngine.Object)(object)val == (UnityEngine.Object)null))
		{
			Soundboard.CleanupAllSounds();
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)val).gameObject.GetComponent<MicPitchShifter>());
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)val).gameObject.GetComponent<EchoEffect>());
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)val).gameObject.GetComponent<ReverbEffect>());
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)val).gameObject.GetComponent<RadioEffect>());
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)val).gameObject.GetComponent<UnderwaterEffect>());
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)val).gameObject.GetComponent<SquareWaveEffect>());
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)val).gameObject.GetComponent<MicAmplifier>());
			val.SourceType = (Photon.Voice.Unity.Recorder.InputSourceType)0;
			val.AudioClip = null;
			val.LoopAudioClip = false;
			val.IsRecording = true;
			val.VoiceDetection = true;
			val.VoiceDetectionThreshold = 0.02f;
			val.TransmitEnabled = true;
			val.Bitrate = 20000;
			val.SamplingRate = (SamplingRate)16000;
			val.RestartRecording(true);
		}
	}

	public static void ReverbMic(bool enable)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if ((UnityEngine.Object)(object)myRecorder == (UnityEngine.Object)null)
		{
			return;
		}
		if (enable)
		{
			if (!((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<ReverbEffect>() != (UnityEngine.Object)null))
			{
				GTExt.GetOrAddComponent<ReverbEffect>(((Component)myRecorder).gameObject);
				myRecorder.VoiceDetection = false;
				myRecorder.VoiceDetectionThreshold = 0f;
				myRecorder.TransmitEnabled = true;
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
			return;
		}
		ReverbEffect component = ((Component)myRecorder).gameObject.GetComponent<ReverbEffect>();
		if (!((UnityEngine.Object)(object)component == (UnityEngine.Object)null))
		{
			((Behaviour)component).enabled = false;
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)component);
			myRecorder.VoiceDetection = true;
			myRecorder.VoiceDetectionThreshold = 0.02f;
			((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
		}
	}

	private static IEnumerator DelayReloadMic()
	{
		yield return (object)new WaitForSeconds(0.25f);
		ReloadMic();
	}

	public static void Earrape(bool enable)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (enable)
		{
			if (!((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<SquareWaveEffect>() != (UnityEngine.Object)null))
			{
				myRecorder.TransmitEnabled = true;
				myRecorder.VoiceDetection = false;
				myRecorder.VoiceDetectionThreshold = 0f;
				GTExt.GetOrAddComponent<SquareWaveEffect>(((Component)myRecorder).gameObject);
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
		}
		else if (((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<SquareWaveEffect>() != (UnityEngine.Object)null))
		{
			myRecorder.VoiceDetection = true;
			myRecorder.VoiceDetectionThreshold = 0.01f;
			SquareWaveEffect component = ((Component)myRecorder).gameObject.GetComponent<SquareWaveEffect>();
			((Behaviour)component).enabled = false;
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<SquareWaveEffect>());
			((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
		}
	}

	public static void ReloadMic()
	{
		GorillaTagger instance = GorillaTagger.Instance;
		if ((UnityEngine.Object)(object)((instance != null) ? instance.myRecorder : null) != (UnityEngine.Object)null)
		{
			GorillaTagger.Instance.myRecorder.RestartRecording(true);
		}
	}

	public static void UnderwaterVoice(bool enable)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (enable)
		{
			if (!((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<UnderwaterEffect>() != (UnityEngine.Object)null))
			{
				GTExt.GetOrAddComponent<UnderwaterEffect>(((Component)myRecorder).gameObject);
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
		}
		else if (((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<UnderwaterEffect>() != (UnityEngine.Object)null))
		{
			UnderwaterEffect component = ((Component)myRecorder).gameObject.GetComponent<UnderwaterEffect>();
			((Behaviour)component).enabled = false;
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<UnderwaterEffect>());
			((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
		}
	}

	public static string GetSoundName(int index)
	{
		int num = index;
		num = num - (num - 229) * (((uint)num > 228u) ? 1 : 0) + 233;
		int num2 = num;
		return (num2 == 234) ? "Rock Wall" : "Default";
	}

	public static void SpecificSoundSpam(int SoundIndex)
	{
		if (Time.time < Variables.rpcCooldown)
		{
			return;
		}
		Variables.rpcCooldown = Time.time + soundCooldown;
		Safety.RPCShield();
		if (InputHandler.RTrigger())
		{
			if (PhotonNetwork.InRoom)
			{
				Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlayHandTap", (RpcTarget)0, new object[3]
				{
					SoundIndex,
					Variables.rightHandedMenu,
					99999f
				});
			}
			else
			{
				Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(SoundIndex, Variables.rightHandedMenu, 1f);
			}
		}
	}

	public static void Custom_Soundspam()
	{
		if (Time.time < Variables.rpcCooldown)
		{
			return;
		}
		Variables.rpcCooldown = Time.time + soundCooldown;
		Safety.RPCShield();
		if (InputHandler.RTrigger())
		{
			if (PhotonNetwork.InRoom)
			{
				Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlayHandTap", (RpcTarget)0, new object[3]
				{
					sound,
					Variables.rightHandedMenu,
					99999f
				});
			}
			else
			{
				Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(sound, Variables.rightHandedMenu, 99999f);
			}
		}
		if (InputHandler.RPrimary())
		{
			if (!haschangedint)
			{
				sound++;
				string soundName = GetSoundName(sound);
				NotificationLib.ClearAllNotifications();
				NotificationLib.SendNotification(NotificationLib.NotificationType.Info, "Set Index To: " + soundName + " [" + sound + "]");
				haschangedint = true;
			}
		}
		else
		{
			haschangedint = false;
		}
		if (InputHandler.RSecondary())
		{
			if (!haschangedint2)
			{
				sound--;
				string soundName2 = GetSoundName(sound);
				NotificationLib.ClearAllNotifications();
				NotificationLib.SendNotification(NotificationLib.NotificationType.Info, "Set Index To: " + soundName2 + " [" + sound + "]");
				haschangedint2 = true;
			}
		}
		else
		{
			haschangedint2 = false;
		}
	}

	public static void EchoVoice(bool enable)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if ((UnityEngine.Object)(object)myRecorder == (UnityEngine.Object)null)
		{
			return;
		}
		if (enable)
		{
			if (!((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<EchoEffect>() != (UnityEngine.Object)null))
			{
				EchoEffect orAddComponent = GTExt.GetOrAddComponent<EchoEffect>(((Component)myRecorder).gameObject);
				orAddComponent.echoDelay = 0.3f;
				orAddComponent.echoDecay = 0.6f;
				myRecorder.VoiceDetection = false;
				myRecorder.VoiceDetectionThreshold = 0f;
				myRecorder.TransmitEnabled = true;
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
		}
		else
		{
			EchoEffect component = ((Component)myRecorder).gameObject.GetComponent<EchoEffect>();
			if (!((UnityEngine.Object)(object)component == (UnityEngine.Object)null))
			{
				((Behaviour)component).enabled = false;
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)component);
				myRecorder.VoiceDetection = true;
				myRecorder.VoiceDetectionThreshold = 0.02f;
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
		}
	}

	public static void LoudMic(bool enable)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		Recorder myRecorder = GorillaTagger.Instance.myRecorder;
		if (enable)
		{
			if (!((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<MicAmplifier>() != (UnityEngine.Object)null))
			{
				MicAmplifier orAddComponent = GTExt.GetOrAddComponent<MicAmplifier>(((Component)myRecorder).gameObject);
				orAddComponent.AmplificationFactor = 13f;
				orAddComponent.AmplificationFactor = 13f;
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
		}
		else if (((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<MicAmplifier>() != (UnityEngine.Object)null))
		{
			if (!((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<MicAmplifier>() == (UnityEngine.Object)null))
			{
				MicAmplifier component = ((Component)myRecorder).gameObject.GetComponent<MicAmplifier>();
				((Behaviour)component).enabled = false;
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)myRecorder).gameObject.GetComponent<MicAmplifier>());
				((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
			}
		}
		else
		{
			((MonoBehaviour)CoroutineHelper.Instance).StartCoroutine(DelayReloadMic());
		}
	}
}
