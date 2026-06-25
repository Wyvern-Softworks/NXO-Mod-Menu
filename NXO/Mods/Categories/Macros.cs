using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NXO.Menu;
using UnityEngine;

namespace NXO.Mods.Categories;

public class Macros
{
	public struct MacroFrame
	{
		public Vector3 rigPosition;

		public Quaternion rigRotation;

		public Vector3 headPosition;

		public Quaternion headRotation;

		public Vector3 leftHandPosition;

		public Quaternion leftHandRotation;

		public Vector3 rightHandPosition;

		public Quaternion rightHandRotation;

		public float leftIndexT;

		public float leftMiddleT;

		public float leftThumbT;

		public float rightIndexT;

		public float rightMiddleT;

		public float rightThumbT;
	}

	public static bool isRecordingMacro = false;

	public static bool isPlayingMacro = false;

	private static bool loopMacro = false;

	private static int macroPlaybackIndex = 0;

	private static List<MacroFrame> currentRecording = new List<MacroFrame>();

	private static List<MacroFrame> currentPlayback = null;

	public static string currentOpenMacro = null;

	public static Dictionary<string, List<ButtonHandler.Button>> macroCategoryButtons = new Dictionary<string, List<ButtonHandler.Button>>();

	private const int MAX_FRAMES = 10800;

	private static bool lastPrimaryState = false;

	private static string macroSavePath => Path.Combine(Variables.folderName, "Macros");

	private static void StopMacroPlayback()
	{
		isPlayingMacro = false;
		loopMacro = false;
		if (currentPlayback != null && currentPlayback.Count > 0 && macroPlaybackIndex > 0)
		{
			Player.TeleportTo(currentPlayback[macroPlaybackIndex - 1].rigPosition);
		}
		currentPlayback = null;
		macroPlaybackIndex = 0;
		if ((UnityEngine.Object)(object)Variables.taggerInstance?.offlineVRRig != (UnityEngine.Object)null)
		{
			((Behaviour)Variables.taggerInstance.offlineVRRig).enabled = true;
		}
		NotificationLib.SendNotification(NotificationLib.NotificationType.Disabled, "Macro Stopped");
	}

	private static void OpenMacroOptions(string macroName)
	{
		currentOpenMacro = macroName;
		macroCategoryButtons[macroName] = new List<ButtonHandler.Button>
		{
			new ButtonHandler.Button("Return", Category.Recorded_Macros, isToggle: false, isActive: false, delegate
			{
				currentOpenMacro = null;
				Main.RefreshMenu();
			})
			{
				isCategory = true
			},
			new ButtonHandler.Button("Play Macro", Category.Recorded_Macros, isToggle: true, isActive: false, () =>
			{
				StartMacroPlayback(macroName);
			}, delegate
			{
				StopMacroPlayback();
			}),
			new ButtonHandler.Button("Loop Macro", Category.Recorded_Macros, isToggle: true, isActive: false, () =>
			{
				loopMacro = true;
				StartMacroPlayback(macroName);
			}, delegate
			{
				loopMacro = false;
				if (isPlayingMacro)
				{
					StopMacroPlayback();
				}
			}),
			new ButtonHandler.Button("Rename Macro", Category.Recorded_Macros, isToggle: false, isActive: false, () =>
			{
				BeginRenameMacro(macroName);
			}),
			new ButtonHandler.Button("Delete Macro", Category.Recorded_Macros, isToggle: false, isActive: false, () =>
			{
				DeleteMacro(macroName);
			})
		};
		Main._lastPageDrawn = Category.Home;
		Main._lastDrawModVersion = -1;
		Main.RedrawButtonList();
	}

	public static void MacroPlaybackTick()
	{
		if (isPlayingMacro)
		{
			PlaybackMacroFrame();
		}
	}

	public static void CleanupMacro()
	{
		if (isPlayingMacro)
		{
			StopMacroPlayback();
			if (!isRecordingMacro)
			{
				return;
			}
		}
		else if (!isRecordingMacro)
		{
			return;
		}
		isRecordingMacro = false;
		currentRecording.Clear();
	}

	private static List<MacroFrame> LoadMacro(string name)
	{
		string path = Path.Combine(macroSavePath, name + ".macro");
		if (!File.Exists(path))
		{
			return null;
		}
		List<MacroFrame> list = new List<MacroFrame>();
		BinaryReader binaryReader = new BinaryReader(File.Open(path, FileMode.Open));
		int num = binaryReader.ReadInt32();
		int num2 = 0;
		if (num2 < num)
		{
			do
			{
				list.Add(new MacroFrame
				{
					rigPosition = ReadVector3(binaryReader),
					rigRotation = ReadQuaternion(binaryReader),
					headPosition = ReadVector3(binaryReader),
					headRotation = ReadQuaternion(binaryReader),
					leftHandPosition = ReadVector3(binaryReader),
					leftHandRotation = ReadQuaternion(binaryReader),
					rightHandPosition = ReadVector3(binaryReader),
					rightHandRotation = ReadQuaternion(binaryReader),
					leftIndexT = binaryReader.ReadSingle(),
					leftMiddleT = binaryReader.ReadSingle(),
					leftThumbT = binaryReader.ReadSingle(),
					rightIndexT = binaryReader.ReadSingle(),
					rightMiddleT = binaryReader.ReadSingle(),
					rightThumbT = binaryReader.ReadSingle()
				});
				num2++;
			}
			while (num2 < num);
		}
		return list;
	}

	private static void BeginRenameMacro(string macroName)
	{
		SearchAndKeyboard.OpenTypingKeyboard(macroName, "Enter macro name...");
		SearchAndKeyboard.onTypingComplete = (string newName) =>
		{
			string text = newName.Trim();
			newName = text;
			if (!string.IsNullOrEmpty(newName) && !(newName == macroName))
			{
				string text2 = Path.Combine(macroSavePath, macroName + ".macro");
				string text3 = Path.Combine(macroSavePath, newName + ".macro");
				if (File.Exists(text2))
				{
					if (File.Exists(text3))
					{
						NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Macro Name Already Exists");
					}
					else
					{
						File.Move(text2, text3);
						if (macroCategoryButtons.ContainsKey(macroName))
						{
							macroCategoryButtons[newName] = macroCategoryButtons[macroName];
							macroCategoryButtons.Remove(macroName);
							currentOpenMacro = null;
							GenerateMacroButtons();
							NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Renamed " + macroName + " → " + newName);
							Main.RedrawButtonList();
						}
						else
						{
							currentOpenMacro = null;
							GenerateMacroButtons();
							NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Renamed " + macroName + " → " + newName);
							Main.RedrawButtonList();
						}
					}
				}
			}
		};
	}

	private static void StartRecording()
	{
		currentRecording.Clear();
		isRecordingMacro = true;
		NotificationLib.SendNotification(NotificationLib.NotificationType.Enabled, "Macro Recording...");
	}

	private static void WriteVector3(BinaryWriter w, Vector3 v)
	{
		w.Write(v.x);
		w.Write(v.y);
		w.Write(v.z);
	}

	private static void RecordFrame()
	{
		if (currentRecording.Count >= 10800)
		{
			StopRecording();
			return;
		}
		VRRig offlineVRRig = Variables.taggerInstance.offlineVRRig;
		if (!((UnityEngine.Object)(object)offlineVRRig == (UnityEngine.Object)null))
		{
			currentRecording.Add(new MacroFrame
			{
				rigPosition = ((Component)offlineVRRig).transform.position,
				rigRotation = ((Component)offlineVRRig).transform.rotation,
				headPosition = offlineVRRig.head.rigTarget.position,
				headRotation = offlineVRRig.head.rigTarget.rotation,
				leftHandPosition = offlineVRRig.leftHand.rigTarget.position,
				leftHandRotation = offlineVRRig.leftHand.rigTarget.rotation,
				rightHandPosition = offlineVRRig.rightHand.rigTarget.position,
				rightHandRotation = offlineVRRig.rightHand.rigTarget.rotation,
				leftIndexT = ((offlineVRRig.leftIndex != null) ? ((VRMap)offlineVRRig.leftIndex).calcT : 0f),
				leftMiddleT = ((offlineVRRig.leftMiddle != null) ? ((VRMap)offlineVRRig.leftMiddle).calcT : 0f),
				leftThumbT = ((offlineVRRig.leftThumb != null) ? ((VRMap)offlineVRRig.leftThumb).calcT : 0f),
				rightIndexT = ((offlineVRRig.rightIndex != null) ? ((VRMap)offlineVRRig.rightIndex).calcT : 0f),
				rightMiddleT = ((offlineVRRig.rightMiddle != null) ? ((VRMap)offlineVRRig.rightMiddle).calcT : 0f),
				rightThumbT = ((offlineVRRig.rightThumb != null) ? ((VRMap)offlineVRRig.rightThumb).calcT : 0f)
			});
		}
	}

	private static void WriteQuaternion(BinaryWriter w, Quaternion q)
	{
		w.Write(q.x);
		w.Write(q.y);
		w.Write(q.z);
		w.Write(q.w);
	}

	public static void MacroController()
	{
		if (isPlayingMacro)
		{
			return;
		}
		if (InputHandler.RPrimary())
		{
			lastPrimaryState = false;
			if (!isRecordingMacro)
			{
				StartRecording();
			}
			else
			{
				StopRecording();
			}
		}
		else
		{
			lastPrimaryState = false;
		}
		if (!isRecordingMacro)
		{
			return;
		}
		RecordFrame();
	}

	private static void StopRecording()
	{
		isRecordingMacro = false;
		if (currentRecording.Count == 0)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "No Frames Recorded");
			return;
		}
		string text = $"Macro_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
		SaveMacro(text, currentRecording);
		GenerateMacroButtons();
		NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, $"{text} ({currentRecording.Count} frames)");
		currentRecording.Clear();
		Main.RefreshMenu();
	}

	public static void GenerateMacroButtons()
	{
		List<ButtonHandler.Button> list = ModButtons.buttons.ToList();
		list.RemoveAll((ButtonHandler.Button b) => b.Page == Category.Recorded_Macros);
		list.Add(new ButtonHandler.Button("Return", Category.Recorded_Macros, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Macros);
		})
		{
			isCategory = true
		});
		if (Directory.Exists(macroSavePath))
		{
			string[] files = Directory.GetFiles(macroSavePath, "*.macro");
			foreach (string path in files)
			{
				string macroName = Path.GetFileNameWithoutExtension(path);
				list.Add(new ButtonHandler.Button(macroName, Category.Recorded_Macros, isToggle: false, isActive: false, () =>
				{
					OpenMacroOptions(macroName);
				}));
			}
			ModButtons.buttons = list.ToArray();
		}
		else
		{
			ModButtons.buttons = list.ToArray();
		}
	}

	private static Vector3 ReadVector3(BinaryReader r)
	{
		return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
	}

	private static void PlaybackMacroFrame()
	{
		if (currentPlayback == null || macroPlaybackIndex >= currentPlayback.Count)
		{
			if (loopMacro && currentPlayback != null)
			{
				macroPlaybackIndex = 0;
			}
			else
			{
				StopMacroPlayback();
			}
			return;
		}
		VRRig offlineVRRig = Variables.taggerInstance.offlineVRRig;
		((Behaviour)offlineVRRig).enabled = false;
		MacroFrame macroFrame = currentPlayback[macroPlaybackIndex];
		((Component)offlineVRRig).transform.SetPositionAndRotation(macroFrame.rigPosition, macroFrame.rigRotation);
		offlineVRRig.head.rigTarget.SetPositionAndRotation(macroFrame.headPosition, macroFrame.headRotation);
		offlineVRRig.leftHand.rigTarget.SetPositionAndRotation(macroFrame.leftHandPosition, macroFrame.leftHandRotation);
		offlineVRRig.rightHand.rigTarget.SetPositionAndRotation(macroFrame.rightHandPosition, macroFrame.rightHandRotation);
		if (offlineVRRig.leftIndex != null)
		{
			((VRMap)offlineVRRig.leftIndex).calcT = macroFrame.leftIndexT;
			((VRMap)offlineVRRig.leftIndex).LerpFinger(1f, false);
		}
		if (offlineVRRig.leftMiddle != null)
		{
			((VRMap)offlineVRRig.leftMiddle).calcT = macroFrame.leftMiddleT;
			((VRMap)offlineVRRig.leftMiddle).LerpFinger(1f, false);
		}
		if (offlineVRRig.leftThumb != null)
		{
			((VRMap)offlineVRRig.leftThumb).calcT = macroFrame.leftThumbT;
			((VRMap)offlineVRRig.leftThumb).LerpFinger(1f, false);
		}
		if (offlineVRRig.rightIndex != null)
		{
			((VRMap)offlineVRRig.rightIndex).calcT = macroFrame.rightIndexT;
			((VRMap)offlineVRRig.rightIndex).LerpFinger(1f, false);
		}
		if (offlineVRRig.rightMiddle != null)
		{
			((VRMap)offlineVRRig.rightMiddle).calcT = macroFrame.rightMiddleT;
			((VRMap)offlineVRRig.rightMiddle).LerpFinger(1f, false);
		}
		if (offlineVRRig.rightThumb != null)
		{
			((VRMap)offlineVRRig.rightThumb).calcT = macroFrame.rightThumbT;
			((VRMap)offlineVRRig.rightThumb).LerpFinger(1f, false);
		}
		macroPlaybackIndex++;
	}

	private static void DeleteMacro(string macroName)
	{
		string path = Path.Combine(macroSavePath, macroName + ".macro");
		if (File.Exists(path))
		{
			File.Delete(path);
			macroCategoryButtons.Remove(macroName);
			currentOpenMacro = null;
			GenerateMacroButtons();
			NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, macroName);
			Main.RefreshMenu();
		}
		else
		{
			macroCategoryButtons.Remove(macroName);
			currentOpenMacro = null;
			GenerateMacroButtons();
			NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, macroName);
			Main.RefreshMenu();
		}
	}

	public static void StartMacroPlayback(string macroName)
	{
		List<MacroFrame> list = LoadMacro(macroName);
		if (list == null || list.Count == 0)
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Failed To Load Macro");
			return;
		}
		currentPlayback = list;
		macroPlaybackIndex = 0;
		isPlayingMacro = true;
		NotificationLib.SendNotification(NotificationLib.NotificationType.Loaded, macroName);
	}

	private static Quaternion ReadQuaternion(BinaryReader r)
	{
		return new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
	}

	private static void SaveMacro(string name, List<MacroFrame> frames)
	{
		BinaryWriter binaryWriter;
		if (!Directory.Exists(macroSavePath))
		{
			Directory.CreateDirectory(macroSavePath);
			string path = Path.Combine(macroSavePath, name + ".macro");
			binaryWriter = new BinaryWriter(File.Open(path, FileMode.Create));
		}
		else
		{
			string path = Path.Combine(macroSavePath, name + ".macro");
			binaryWriter = new BinaryWriter(File.Open(path, FileMode.Create));
		}
		binaryWriter.Write(frames.Count);
		List<MacroFrame>.Enumerator enumerator = frames.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				MacroFrame current = enumerator.Current;
				WriteVector3(binaryWriter, current.rigPosition);
				WriteQuaternion(binaryWriter, current.rigRotation);
				WriteVector3(binaryWriter, current.headPosition);
				WriteQuaternion(binaryWriter, current.headRotation);
				WriteVector3(binaryWriter, current.leftHandPosition);
				WriteQuaternion(binaryWriter, current.leftHandRotation);
				WriteVector3(binaryWriter, current.rightHandPosition);
				WriteQuaternion(binaryWriter, current.rightHandRotation);
				binaryWriter.Write(current.leftIndexT);
				binaryWriter.Write(current.leftMiddleT);
				binaryWriter.Write(current.leftThumbT);
				binaryWriter.Write(current.rightIndexT);
				binaryWriter.Write(current.rightMiddleT);
				binaryWriter.Write(current.rightThumbT);
			}
			while (enumerator.MoveNext());
		}
	}
}
