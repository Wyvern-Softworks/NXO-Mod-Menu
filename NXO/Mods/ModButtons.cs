using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GorillaLocomotion;
using GorillaNetworking;
using NXO.Menu;
using NXO.Mods.Categories;
using Photon.Pun;
using UnityEngine;

namespace NXO.Mods;

public class ModButtons
{
	private static ButtonHandler.Button[] _buttons = new ButtonHandler.Button[453]
	{
		new ButtonHandler.Button("Settings", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Settings);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Presets", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Presets);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Favorites", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Favorited);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Enabled", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Enabled);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Players List", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Players);
			PlayersActionList.GeneratePlayerButtons();
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Safety", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Safety);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Room", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Room);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("VRRig", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Player);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Movement", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Movement);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Visuals", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Visuals);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Audio", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Audio);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Gamemode", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Gamemode);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Projectiles", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Projectiles);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Fun", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Fun);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Macros", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Macros);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Overpowered", Category.Home, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Overpowered);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Open NXO Folder", Category.Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.OpenFolder(Variables.folderName);
		}),
		new ButtonHandler.Button("Default Settings", Category.Settings, isToggle: false, isActive: false, delegate
		{
			Settings.ResetToDefaultSettings();
		}),
		new ButtonHandler.Button("Auto Save", Category.Settings, isToggle: true, isActive: true, delegate
		{
			ButtonHandler.SetAutoSave(on: true);
		}, delegate
		{
			ButtonHandler.SetAutoSave(on: false);
		}),
		new ButtonHandler.Button("Panic (X)", Category.Settings, isToggle: true, isActive: false, delegate
		{
			Safety.Panic();
		}, delegate
		{
			Safety.PanicReset();
		}),
		new ButtonHandler.Button("Disable All Mods", Category.Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.DisableAllMods();
		}),
		new ButtonHandler.Button("Menu", Category.Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Menu_Settings);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Return", Category.Menu_Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Settings);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Right Hand Menu", Category.Menu_Settings, isToggle: true, isActive: false, delegate
		{
			Settings.SwitchHands(setActive: true);
		}, delegate
		{
			Settings.SwitchHands(setActive: false);
		}),
		new ButtonHandler.Button("Mods Array List", Category.Menu_Settings, isToggle: true, isActive: false, delegate
		{
			Settings.ToggleArrayList(setActive: true);
		}, delegate
		{
			Settings.ToggleArrayList(setActive: false);
		}),
		new ButtonHandler.Button("Open & Close Sounds", Category.Menu_Settings, isToggle: true, isActive: true, delegate
		{
			Settings.MenuOpenAndCloseSounds(setActive: true);
		}, delegate
		{
			Settings.MenuOpenAndCloseSounds(setActive: false);
		}),
		new ButtonHandler.Button("Dynamic Animations", Category.Menu_Settings, isToggle: true, isActive: true, delegate
		{
			Main.MenuAnimations = true;
		}, delegate
		{
			Main.MenuAnimations = false;
		}),
		new ButtonHandler.Button("Menu Follows When Searching", Category.Menu_Settings, isToggle: true, isActive: true, delegate
		{
			Variables.SearchFollow = true;
		}, delegate
		{
			Variables.SearchFollow = false;
		}),
		new ButtonHandler.Button("Overlap Custom Click Sounds", Category.Menu_Settings, isToggle: true, isActive: false, delegate
		{
			ButtonHandler.OverlapCustomClickSounds = true;
		}, delegate
		{
			ButtonHandler.OverlapCustomClickSounds = false;
		}),
		new ButtonHandler.Button("Custom Board Colors", Category.Menu_Settings, isToggle: true, isActive: false, delegate
		{
			CustomBoards.SetBoardsEnabled(enabled: true);
		}, delegate
		{
			CustomBoards.SetBoardsEnabled(enabled: false);
		}),
		(Settings.CycleMenuFontButton = new ButtonHandler.Button("Menu Font : " + Settings.CurrentFontDescription, Category.Menu_Settings, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.CycleFont(forward: true);
		}, delegate
		{
			Settings.CycleFont(forward: false);
		})),
		(Settings.ChangeSoundButton = new ButtonHandler.Button("Click Sound : " + Settings.ClickSoundDescription, Category.Menu_Settings, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.ChangeClickSound(forward: true);
		}, delegate
		{
			Settings.ChangeClickSound(forward: false);
		})),
		(Settings.outlineModeButton = new ButtonHandler.Button("Outline : " + Settings.OutlineModes[Settings.OutlineMode], Category.Menu_Settings, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.CycleOutlineMode(forward: true);
		}, delegate
		{
			Settings.CycleOutlineMode(forward: false);
		})),
		(Settings.adjustMenuSizeButton = new ButtonHandler.Button("Menu Size : " + Settings.MenuSizeDescription, Category.Menu_Settings, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustMenuSize(forward: true);
		}, delegate
		{
			Settings.AdjustMenuSize(forward: false);
		})),
		(Settings.adjustRoundnessButton = new ButtonHandler.Button("Roundness : " + Settings.RoundnessDescription, Category.Menu_Settings, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustRoundness(forward: true);
		}, delegate
		{
			Settings.AdjustRoundness(forward: false);
		})),
		(Settings.adjustOpacityButton = new ButtonHandler.Button("Opacity : " + Settings.OpacityDescription, Category.Menu_Settings, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustOpacity(forward: true);
		}, delegate
		{
			Settings.AdjustOpacity(forward: false);
		})),
		(Settings.adjustAccentStripTypeButton = new ButtonHandler.Button("Accent Strip : " + Settings.AccentStripTypeDescription, Category.Menu_Settings, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustAccentStripType(forward: true);
		}, delegate
		{
			Settings.AdjustAccentStripType(forward: false);
		})),
		new ButtonHandler.Button("Colors", Category.Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Color_Settings);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Return", Category.Color_Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Settings);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Pinwheel", Category.Color_Settings, isToggle: false, isActive: false, delegate
		{
			Settings.OpenElementSettings(Settings.ColorElement.Pinwheel);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Outline", Category.Color_Settings, isToggle: false, isActive: false, delegate
		{
			Settings.OpenElementSettings(Settings.ColorElement.Outline);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Background", Category.Color_Settings, isToggle: false, isActive: false, delegate
		{
			Settings.OpenElementSettings(Settings.ColorElement.Background);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Buttons", Category.Color_Settings, isToggle: false, isActive: false, delegate
		{
			Settings.OpenElementSettings(Settings.ColorElement.Button);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Title", Category.Color_Settings, isToggle: false, isActive: false, delegate
		{
			Settings.OpenElementSettings(Settings.ColorElement.Title);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Boards", Category.Color_Settings, isToggle: false, isActive: false, delegate
		{
			Settings.OpenElementSettings(Settings.ColorElement.Boards);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Accent Strip", Category.Color_Settings, isToggle: false, isActive: false, delegate
		{
			Settings.OpenElementSettings(Settings.ColorElement.AccentStrip);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Notifications", Category.Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Notification_Settings);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Return", Category.Notification_Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Settings);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Toggle Notifications", Category.Notification_Settings, isToggle: true, isActive: true, delegate
		{
			Settings.ToggleNotifications(setActive: true);
		}, delegate
		{
			Settings.ToggleNotifications(setActive: false);
		}),
		new ButtonHandler.Button("Photon.Realtime.Room Notifications", Category.Notification_Settings, isToggle: true, isActive: true, delegate
		{
			Settings.RoomNotifications(setActive: true);
		}, delegate
		{
			Settings.RoomNotifications(setActive: false);
		}),
		new ButtonHandler.Button("Photon.Realtime.Room Notifications Sound", Category.Notification_Settings, isToggle: true, isActive: true, delegate
		{
			Settings.SoundRoomNotifications(setActive: true);
		}, delegate
		{
			Settings.SoundRoomNotifications(setActive: false);
		}),
		new ButtonHandler.Button("Clear Notifications", Category.Notification_Settings, isToggle: false, isActive: false, delegate
		{
			Settings.ClearNotifications();
		}),
		new ButtonHandler.Button("Gun", Category.Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Gun_Settings);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Return", Category.Gun_Settings, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Settings);
		})
		{
			isCategory = true
		},
		(Settings.cycleGunAnimationButton = new ButtonHandler.Button("Gun Animation : " + Settings.GunAnimationType, Category.Gun_Settings, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.CycleGunAnimation(forward: true);
		}, delegate
		{
			Settings.CycleGunAnimation(forward: false);
		})),
		new ButtonHandler.Button("Big Gun Pointer", Category.Gun_Settings, isToggle: true, isActive: false, delegate
		{
			Settings.BigPointer(setActive: true);
		}, delegate
		{
			Settings.BigPointer(setActive: false);
		}),
		new ButtonHandler.Button("Gun Line", Category.Gun_Settings, isToggle: true, isActive: true, delegate
		{
			Settings.GunLine(setActive: true);
		}, delegate
		{
			Settings.GunLine(setActive: false);
		}),
		new ButtonHandler.Button("Open Presets Folder", Category.Presets, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.OpenFolder(Path.Combine(Variables.folderName, "Presets"));
		}),
		new ButtonHandler.Button("Save Preset", Category.Presets, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.SavePreset();
		}),
		new ButtonHandler.Button("Saved Presets", Category.Presets, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Saved_Presets);
		})
		{
			isCategory = true
		},
		(Settings.adjustAntiReportRadiusButton = new ButtonHandler.Button("Anti Report Radius : " + Settings.AntiReportRadiusDescription, Category.Safety, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustAntiReportRadius(forward: true);
		}, delegate
		{
			Settings.AdjustAntiReportRadius(forward: false);
		})),
		new ButtonHandler.Button("Visualize Anti Report", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.VisualizeAntiReport();
		}, delegate
		{
			Safety.DisableVisualizeAntiReport();
		}),
		new ButtonHandler.Button("Anti Report", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.AntiReport(autoQueue: false, reconnect: false);
		}),
		new ButtonHandler.Button("Join Random Anti Report", Category.Safety, isToggle: true, isActive: true, delegate
		{
			Safety.AntiReport(autoQueue: true, reconnect: false);
		}),
		new ButtonHandler.Button("Reconnect Anti Report", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.AntiReport(autoQueue: false, reconnect: true);
		}),
		new ButtonHandler.Button("No Finger Movement", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Player.NoFingerMovement();
		}),
		new ButtonHandler.Button("Change Identity", Category.Safety, isToggle: false, isActive: false, delegate
		{
			Safety.ChangeIdentity();
		}),
		new ButtonHandler.Button("Change Identity On Disconnect", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.ChangeIdentityOnDisconnect(Safety.ChangeIdentity);
		}),
		new ButtonHandler.Button("Name Spoof", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.NameSpoof();
		}),
		new ButtonHandler.Button("Color Spoof", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.ColorSpoof();
		}),
		new ButtonHandler.Button("Ranked Platform Spoof", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.SpoofPlatform(enabled: true);
		}, delegate
		{
			Safety.SpoofPlatform(enabled: false);
		}),
		new ButtonHandler.Button("Ranked Badge Spoof", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.SpoofBadge(enable: true);
		}, delegate
		{
			Safety.SpoofBadge(enable: false);
		}),
		new ButtonHandler.Button("Bypass Mod Checkers", Category.Safety, isToggle: true, isActive: false, delegate
		{
			MenuPatches.propertiesEnabled = true;
			Safety.BypassModCheckers();
		}, delegate
		{
			MenuPatches.propertiesEnabled = false;
		}),
		new ButtonHandler.Button("Bypass Automod", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.BypassAutomod();
		}),
		new ButtonHandler.Button("Anti Moderator", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.AntiModerator();
		}),
		new ButtonHandler.Button("Anti Cheat Notifications", Category.Safety, isToggle: true, isActive: false, delegate
		{
			MenuPatches.AntiCheatNotifications = true;
		}, delegate
		{
			MenuPatches.AntiCheatNotifications = false;
		}),
		new ButtonHandler.Button("Accept TOS", Category.Safety, isToggle: true, isActive: false, delegate
		{
			MenuPatches.tosEnabled = true;
		}, delegate
		{
			MenuPatches.tosEnabled = false;
		}),
		new ButtonHandler.Button("Bypass K-ID", Category.Safety, isToggle: true, isActive: false, delegate
		{
			MenuPatches.kidEnabled = true;
		}, delegate
		{
			MenuPatches.kidEnabled = false;
		}),
		new ButtonHandler.Button("Fake Lag (A)", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Safety.FakeLag();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Disable Quit Box", Category.Safety, isToggle: true, isActive: false, delegate
		{
			Room.SetQuitBoxActive(isActive: false);
		}, delegate
		{
			Room.SetQuitBoxActive(isActive: true);
		}),
		new ButtonHandler.Button("Quit Game", Category.Room, false, false, delegate
		{
			NotificationLib.SendNotification(NotificationLib.NotificationType.Info, "Quit Game is disabled");
		}, (Action)null, false, (Action)null, (Action)null),
		new ButtonHandler.Button("Disconnect", Category.Room, isToggle: false, isActive: false, delegate
		{
			Room.Disconnect();
		}),
		new ButtonHandler.Button("Reconnect", Category.Room, isToggle: false, isActive: false, delegate
		{
			Room.Reconnect();
		}),
		new ButtonHandler.Button("Join Random Public", Category.Room, isToggle: false, isActive: false, delegate
		{
			Room.JoinRandomPublic();
		}),
		new ButtonHandler.Button("Create Public", Category.Room, isToggle: false, isActive: false, delegate
		{
			Room.CreateRoom(Room.RandomRoomName(), isPublic: true, 0, (GorillaNetworking.JoinType)0);
		}),
		new ButtonHandler.Button("Create Private", Category.Room, isToggle: false, isActive: false, delegate
		{
			Room.CreateRoom(Room.RandomRoomName(), isPublic: false, 0, (GorillaNetworking.JoinType)0);
		}),
		new ButtonHandler.Button("Join Specific Room", Category.Room, isToggle: false, isActive: false, delegate
		{
			Room.JoinRoom();
		}),
		new ButtonHandler.Button("Disable Network Triggers", Category.Room, isToggle: true, isActive: false, delegate
		{
			Room.SetNetworkTriggersActive(isActive: false);
		}, delegate
		{
			Room.SetNetworkTriggersActive(isActive: true);
		}),
		new ButtonHandler.Button("Grab All IDs", Category.Room, isToggle: false, isActive: false, delegate
		{
			Room.GrabAllIDs();
		}),
		new ButtonHandler.Button("Grab Own ID", Category.Room, isToggle: false, isActive: false, delegate
		{
			Room.GrabSelfID();
		}),
		new ButtonHandler.Button("Anti AFK Kick", Category.Room, isToggle: true, isActive: false, delegate
		{
			((PhotonNetworkController)PhotonNetworkController.Instance).disableAFKKick = true;
		}, delegate
		{
			((PhotonNetworkController)PhotonNetworkController.Instance).disableAFKKick = false;
		}),
		new ButtonHandler.Button("US Servers", Category.Room, isToggle: false, isActive: false, delegate
		{
			PhotonNetwork.ConnectToRegion("us");
		}),
		new ButtonHandler.Button("USW Servers", Category.Room, isToggle: false, isActive: false, delegate
		{
			PhotonNetwork.ConnectToRegion("usw");
		}),
		new ButtonHandler.Button("EU Servers", Category.Room, isToggle: false, isActive: false, delegate
		{
			PhotonNetwork.ConnectToRegion("eu");
		}),
		new ButtonHandler.Button("Queue to Default", Category.Room, isToggle: false, isActive: false, delegate
		{
			((GorillaComputer)GorillaComputer.instance).currentQueue = "DEFAULT";
		}),
		new ButtonHandler.Button("Queue to Minigames", Category.Room, isToggle: false, isActive: false, delegate
		{
			((GorillaComputer)GorillaComputer.instance).currentQueue = "MINIGAMES";
		}),
		new ButtonHandler.Button("Queue to Competitive", Category.Room, isToggle: false, isActive: false, delegate
		{
			((GorillaComputer)GorillaComputer.instance).currentQueue = "COMPETITIVE";
		}),
		new ButtonHandler.Button("Ghost Monke (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.GhostMonke();
		}),
		new ButtonHandler.Button("Invisible Monke (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.InvisibleMonke();
		}),
		new ButtonHandler.Button("Ghost & Invisibility (A/B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.GhostInvisibleMonke();
		}),
		(Settings.adjustArmLengthButton = new ButtonHandler.Button("Long Arms Length : " + Settings.ArmLengthDescription, Category.Player, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustArmLength(forward: true);
		}, delegate
		{
			Settings.AdjustArmLength(forward: false);
		})),
		new ButtonHandler.Button("Long Arms", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.LongArms(setActive: true);
		}, delegate
		{
			Player.LongArms(setActive: false);
		}),
		new ButtonHandler.Button("Size Change (LT/RT/CS)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.SizeChanger();
		}),
		new ButtonHandler.Button("Networked Size Change (LT/RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Networked.SizeChanger.NetworkedSizeChanger(enable: true);
		}, delegate
		{
			Networked.SizeChanger.NetworkedSizeChanger(enable: false);
		}),
		new ButtonHandler.Button("Spin Head", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.SetHeadRotation(null, spin: true);
		}, delegate
		{
			Player.ResetHeadRotation();
		}),
		new ButtonHandler.Button("Upside Head", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.SetHeadRotation((Vector3?)new Vector3(180f, 0f, 0f), false);
		}, delegate
		{
			Player.ResetHeadRotation();
		}),
		new ButtonHandler.Button("Backwards Head", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.SetHeadRotation((Vector3?)new Vector3(0f, 180f, 0f), false);
		}, delegate
		{
			Player.ResetHeadRotation();
		}),
		new ButtonHandler.Button("Snap Neck (Left)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.SetHeadRotation((Vector3?)new Vector3(0f, 0f, 90f), false);
		}, delegate
		{
			Player.ResetHeadRotation();
		}),
		new ButtonHandler.Button("Snap Neck (Right)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.SetHeadRotation((Vector3?)new Vector3(0f, 0f, -90f), false);
		}, delegate
		{
			Player.ResetHeadRotation();
		}),
		new ButtonHandler.Button("Freeze Rig (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.FreezeRig();
		}),
		new ButtonHandler.Button("Grab Rig (RG/LG)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.GrabRig();
		}),
		new ButtonHandler.Button("Fake Body Tracking", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.FakeBodyTracking();
		}),
		new ButtonHandler.Button("Ragdoll Monke (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.RagdollMonke();
		}, delegate
		{
			Player.DisableRagdollMonke();
		}),
		new ButtonHandler.Button("T-Pose (RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.TPose();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Lay On Back (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.LayOnBack();
		}),
		new ButtonHandler.Button("Lay On Stomach (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.LayOnStomach();
		}),
		new ButtonHandler.Button("Upside Down (RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.UpsideDown();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Backflip (RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.Backflip();
		}, delegate
		{
			Player.DisableBackflip();
		}),
		new ButtonHandler.Button("Frontflip (RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.Frontflip();
		}, delegate
		{
			Player.DisableFrontflip();
		}),
		new ButtonHandler.Button("Cartwheel (RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.Cartwheel();
		}, delegate
		{
			Player.DisableCartwheel();
		}),
		new ButtonHandler.Button("Griddy (RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.GriddyMonke();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Dance Monke (RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.DanceMonke();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Spaz Monke (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.SpazMonke();
		}, delegate
		{
			Player.ResetHeadRotation();
		}),
		new ButtonHandler.Button("Spider Monke (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.SpiderMonke();
		}, delegate
		{
			Player.DisableSpiderMonke();
		}),
		new ButtonHandler.Button("Glitch Monke (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.GlitchMonke();
		}, delegate
		{
			Player.DisableGlitchMonke();
		}),
		new ButtonHandler.Button("Wobbly Monke (B)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.WobblyMonke();
		}, delegate
		{
			Player.DisableWobblyMonke();
		}),
		new ButtonHandler.Button("Helicopter Monke (RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.HelicopterMonke();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Ascend Monke (RT)", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.AscendingMonke();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		(Settings.teleportMapButton = new ButtonHandler.Button("TP To : " + Settings.MapDescription, Category.Player, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.CycleMap(forward: true);
		}, delegate
		{
			Settings.CycleMap(forward: false);
		})),
		new ButtonHandler.Button("Teleport To Map", Category.Player, isToggle: false, isActive: false, delegate
		{
			Player.TeleportToMap();
		}),
		new ButtonHandler.Button("Teleport Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.TeleportGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Rig Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.RigGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Chase Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.ChasePlayerGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Chase All", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.ChaseAll();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Orbit Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.OrbitPlayerGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Orbit All", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.OrbitAll();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Jumpscare Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.JumpscarePlayerGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Jumpscare All", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.JumpscareAll();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Copy Photon.Realtime.Player Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.CopyPlayerGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Copy All", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.CopyAll();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Mirror Photon.Realtime.Player Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.MirrorPlayerGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Mirror All", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.MirrorAll();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Backshot Photon.Realtime.Player Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.BackshotPlayerGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Backshot All", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.BackshotAll();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Gawk Gawk Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.GawkGawkGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Gawk Gawk All", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.GawkGawkAll();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Piggyback Photon.Realtime.Player Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.PiggybackPlayerGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Piggyback All", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.PiggybackAll();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Only Visible To Photon.Realtime.Player Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.OnlyVisibleToPlayerGun();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			GunLib.lockedTargetRig = null;
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Only Invisible To Photon.Realtime.Player Gun", Category.Player, isToggle: true, isActive: false, delegate
		{
			Player.OnlyInvisibleToPlayerGun();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			GunLib.lockedTargetRig = null;
			GunLib.SetGunVisibility(isVisible: false);
		}),
		(Settings.adjustFlySpeedButton = new ButtonHandler.Button("Fly Speed : " + Settings.FlySpeedDescription, Category.Movement, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustFlySpeed(forward: true);
		}, delegate
		{
			Settings.AdjustFlySpeed(forward: false);
		})),
		new ButtonHandler.Button("Fly (A)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.FlyMonke(useVelocity: false);
		}),
		new ButtonHandler.Button("Fly + Noclip (A)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.FlyNoclip();
		}),
		(Settings.cyclePlatformTypeButton = new ButtonHandler.Button("Platform Type : " + Settings.PlatformType, Category.Movement, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.CyclePlatformType(forward: true);
		}, delegate
		{
			Settings.CyclePlatformType(forward: false);
		})),
		new ButtonHandler.Button("Use Triggers For Platforms", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Settings.TriggerPlatforms(setActive: true);
		}, delegate
		{
			Settings.TriggerPlatforms(setActive: false);
		}),
		new ButtonHandler.Button("Platforms (RG/LG)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.TogglePlatforms();
		}),
		new ButtonHandler.Button("Frozone (RG/LG)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.Frozone();
		}),
		(Settings.adjustSpeedBoostButton = new ButtonHandler.Button("Boost Speed : " + Settings.SpeedDescription, Category.Movement, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustSpeedBoost(forward: true);
		}, delegate
		{
			Settings.AdjustSpeedBoost(forward: false);
		})),
		new ButtonHandler.Button("Use Grip For Speedboost", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Settings.GripSpeedBoost(setActive: true);
		}, delegate
		{
			Settings.GripSpeedBoost(setActive: false);
		}),
		new ButtonHandler.Button("Speed Boost", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.SpeedBoost();
		}),
		new ButtonHandler.Button("Noclip (RT)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.Noclip();
		}),
		(Settings.adjustWalkWalkStrengthButton = new ButtonHandler.Button("Wall Walk Strength : " + Settings.WalkWalkStrengthDescription, Category.Movement, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustWallWalkStrength(forward: true);
		}, delegate
		{
			Settings.AdjustWallWalkStrength(forward: false);
		})),
		new ButtonHandler.Button("Wall Walk (RG/LG)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.WallWalk();
		}),
		new ButtonHandler.Button("Pull Mod", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.PullMod();
		}),
		new ButtonHandler.Button("Iron Monke (RG/LG)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.IronMonke(10);
		}),
		new ButtonHandler.Button("Accel Fly (A)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.FlyMonke(useVelocity: true);
		}),
		new ButtonHandler.Button("Up And Down (LT/RT)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.UpAndDown();
		}),
		new ButtonHandler.Button("Low Gravity", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.LowGravity();
		}),
		new ButtonHandler.Button("Zero Gravity", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.AntiGravity();
		}),
		new ButtonHandler.Button("High Gravity", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.HighGravity();
		}),
		new ButtonHandler.Button("Reverse Gravity", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.ReverseGravity();
		}),
		new ButtonHandler.Button("WASD Movement", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.WASDMovement();
		}),
		new ButtonHandler.Button("Joystick Fly", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.JoystickFly();
		}),
		new ButtonHandler.Button("Uncap Max Velocity", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Variables.playerInstance.maxJumpSpeed = 9999f;
		}),
		new ButtonHandler.Button("Fast Swim Speed", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.FastSwim();
		}),
		new ButtonHandler.Button("Dash Monke (A)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.DashAndAirJump(isDashEnabled: true, isAirJumpEnabled: false);
		}),
		new ButtonHandler.Button("Checkpoint (RG/RT)", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.PlaceCheckPoint(setActive: true);
		}, delegate
		{
			Movement.PlaceCheckPoint(setActive: false);
		}),
		new ButtonHandler.Button("Slide Control", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.SlideControl(setActive: true);
		}, delegate
		{
			Movement.SlideControl(setActive: false);
		}),
		new ButtonHandler.Button("Anti Slip", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.GrippyHands(setActive: true);
		}, delegate
		{
			Movement.GrippyHands(setActive: false);
		}),
		new ButtonHandler.Button("Slippy Hands", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Movement.SlippyHands(setActive: true);
		}, delegate
		{
			Movement.SlippyHands(setActive: false);
		}),
		new ButtonHandler.Button("No Tag Freeze", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Variables.playerInstance.disableMovement = false;
		}),
		new ButtonHandler.Button("Force Tag Freeze", Category.Movement, isToggle: true, isActive: false, delegate
		{
			Variables.playerInstance.disableMovement = true;
		}, delegate
		{
			Variables.playerInstance.disableMovement = false;
		}),
		(Settings.cycleNametagTypeButton = new ButtonHandler.Button("Nametag Type : " + Settings.NametagType, Category.Visuals, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.CycleNametagType(forward: true);
		}, delegate
		{
			Settings.CycleNametagType(forward: false);
		})),
		new ButtonHandler.Button("Name Tags", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.Nametags(enable: true);
		}, delegate
		{
			Visuals.Nametags(enable: false);
		}),
		new ButtonHandler.Button("Team Checked ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Settings.IsTeamChecked(setActive: true);
		}, delegate
		{
			Settings.IsTeamChecked(setActive: false);
		}),
		new ButtonHandler.Button("Unfilled Box ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.ESP3D(enable: true);
		}, delegate
		{
			Visuals.ESP3D(enable: false);
		}),
		new ButtonHandler.Button("Unfilled Box ESP 2D", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.ESP2D(enable: true);
		}, delegate
		{
			Visuals.ESP2D(enable: false);
		}),
		new ButtonHandler.Button("Filled Box ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.FilledBoxESP(enable: true);
		}, delegate
		{
			Visuals.FilledBoxESP(enable: false);
		}),
		new ButtonHandler.Button("Filled Box ESP 2D", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.FilledBoxESP2D(enable: true);
		}, delegate
		{
			Visuals.FilledBoxESP2D(enable: false);
		}),
		(Settings.tracerPositionButton = new ButtonHandler.Button("Tracer Position : " + Settings.TracerPosition, Category.Visuals, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustTracerPosition(forward: true);
		}, delegate
		{
			Settings.AdjustTracerPosition(forward: false);
		})),
		new ButtonHandler.Button("Tracers ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.TracersESP(enable: true);
		}, delegate
		{
			Visuals.TracersESP(enable: false);
		}),
		new ButtonHandler.Button("Prediction ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.PredictionESP(enable: true);
		}, delegate
		{
			Visuals.PredictionESP(enable: false);
		}),
		new ButtonHandler.Button("Bone ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.BoneESP(enable: true);
		}, delegate
		{
			Visuals.BoneESP(enable: false);
		}),
		new ButtonHandler.Button("Skeleton ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.SkeletonESP(enable: true);
		}, delegate
		{
			Visuals.SkeletonESP(enable: false);
		}),
		new ButtonHandler.Button("Chams ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.ChamsESP(enable: true);
		}, delegate
		{
			Visuals.ChamsESP(enable: false);
		}),
		new ButtonHandler.Button("Trails ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.TrailsESP(enable: true);
		}, delegate
		{
			Visuals.TrailsESP(enable: false);
		}),
		new ButtonHandler.Button("Beacons ESP", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.BeaconsESP(enable: true);
		}, delegate
		{
			Visuals.BeaconsESP(enable: false);
		}),
		(Settings.adjustFOVButton = new ButtonHandler.Button("FPC FOV : " + Settings.FOVDescription, Category.Visuals, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustFOV(forward: true);
		}, delegate
		{
			Settings.AdjustFOV(forward: false);
		})),
		new ButtonHandler.Button("First Person Cam (PC)", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.FPC(enable: true);
		}, delegate
		{
			Visuals.FPC(enable: false);
		}),
		new ButtonHandler.Button("Free Cam (X)", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.Freecam();
		}, delegate
		{
			Visuals.DisableNXOCamera();
		}),
		new ButtonHandler.Button("VR 3rd Person (X)", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.VrThirdPerson();
		}, delegate
		{
			Visuals.DisableNXOCamera();
		}),
		new ButtonHandler.Button("Drunk Cam (X)", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.DrunkCam();
		}, delegate
		{
			Visuals.DisableNXOCamera();
		}),
		new ButtonHandler.Button("Spectate Gun", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.SpectateGun();
		}, delegate
		{
			Visuals.DisableNXOCamera();
		}),
		new ButtonHandler.Button("Orbit Cam (X)", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.OrbitCam();
		}, delegate
		{
			Visuals.DisableNXOCamera();
		}),
		new ButtonHandler.Button("VR 3rd Person In Front (X)", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.VrThirdPersonInFront();
		}, delegate
		{
			Visuals.DisableNXOCamera();
		}),
		new ButtonHandler.Button("Upside Down Cam (X)", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.UpsideDownCam();
		}, delegate
		{
			Visuals.DisableNXOCamera();
		}),
		new ButtonHandler.Button("Monke Sense", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.MonkeSense(enable: true);
		}, delegate
		{
			Visuals.MonkeSense(enable: false);
		}),
		new ButtonHandler.Button("Mute Monke Sense", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.muteMonkeSense = true;
		}, delegate
		{
			Visuals.muteMonkeSense = false;
		}),
		new ButtonHandler.Button("FPS Boost", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.FPSBoost(enable: true);
		}, delegate
		{
			Visuals.FPSBoost(enable: false);
		}),
		new ButtonHandler.Button("X-Ray", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.ToggleXray(enabled: true);
		}, delegate
		{
			Visuals.ToggleXray(enabled: false);
		}),
		new ButtonHandler.Button("Toggle Fog", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.ToggleFog(enable: true);
		}, delegate
		{
			Visuals.ToggleFog(enable: false);
		}),
		new ButtonHandler.Button("Fuck Colors", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.FuckLights(enable: true);
		}, delegate
		{
			Visuals.FuckLights(enable: false);
		}),
		new ButtonHandler.Button("Acid Trip", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.AcidTrip(enable: true);
		}, delegate
		{
			Visuals.AcidTrip(enable: false);
		}),
		new ButtonHandler.Button("Trippy Monkes", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.TrippyMonkes(enable: true);
		}, delegate
		{
			Visuals.TrippyMonkes(enable: false);
		}),
		new ButtonHandler.Button("Shiny Monkes", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.ShinyMonkes(enable: true);
		}, delegate
		{
			Visuals.ShinyMonkes(enable: false);
		}),
		new ButtonHandler.Button("Shiny Self", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.ShinySelf(enable: true);
		}, delegate
		{
			Visuals.ShinySelf(enable: false);
		}),
		new ButtonHandler.Button("Ghost Rig", Category.Visuals, isToggle: true, isActive: true, delegate
		{
			Settings.ToggleGhostRig(setActive: true);
		}, delegate
		{
			Settings.ToggleGhostRig(setActive: false);
		}),
		new ButtonHandler.Button("90 FPS", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.CapFPS(90);
		}),
		new ButtonHandler.Button("72 FPS", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.CapFPS(72);
		}),
		new ButtonHandler.Button("60 FPS", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.CapFPS(60);
		}),
		new ButtonHandler.Button("45 FPS", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.CapFPS(45);
		}),
		new ButtonHandler.Button("30 FPS", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.CapFPS(30);
		}),
		new ButtonHandler.Button("15 FPS", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.CapFPS(15);
		}),
		new ButtonHandler.Button("Uncap FPS", Category.Visuals, isToggle: true, isActive: false, delegate
		{
			Visuals.UncapFPS(enabled: true);
		}, delegate
		{
			Visuals.UncapFPS(enabled: false);
		}),
		new ButtonHandler.Button("Enter Soundboard", Category.Audio, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Soundboard);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Return", Category.Soundboard, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Audio);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Load Soundboard", Category.Soundboard, isToggle: false, isActive: false, delegate
		{
			Soundboard.InitializeSounds();
		}),
		new ButtonHandler.Button("Disable All Sounds", Category.Soundboard, isToggle: false, isActive: false, delegate
		{
			Soundboard.SoundboardSoundsActive.Keys.ToList().ForEach(Soundboard.StopSound);
		}),
		new ButtonHandler.Button("Loop Sounds", Category.Soundboard, isToggle: true, isActive: false, delegate
		{
			Soundboard.EnableLoop();
		}, delegate
		{
			Soundboard.DisableLoop();
		}),
		(Settings.cycleControllerBindButton = new ButtonHandler.Button("Sound Input : " + Settings.inputName, Category.Soundboard, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.CycleControllerBind();
		}, delegate
		{
			Settings.CycleControllerBind(forward: false);
		})),
		new ButtonHandler.Button("Return", Category.SFX, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Soundboard);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Return", Category.Trolling, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Soundboard);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Return", Category.Songs, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Soundboard);
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Reload Microphone", Category.Audio, isToggle: false, isActive: false, delegate
		{
			Sound.ReloadMic();
		}),
		new ButtonHandler.Button("Hear Self", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Variables.taggerInstance.myRecorder.DebugEchoMode = true;
		}, delegate
		{
			Variables.taggerInstance.myRecorder.DebugEchoMode = false;
		}),
		new ButtonHandler.Button("High Quality Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SetMicQuality(65000, 48000);
		}, delegate
		{
			Sound.SetMicQuality(20000, 16000);
		}),
		new ButtonHandler.Button("Low Quality Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SetMicQuality(6000, 8000);
		}, delegate
		{
			Sound.SetMicQuality(20000, 16000);
		}),
		new ButtonHandler.Button("Reverb Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.ReverbMic(enable: true);
		}, delegate
		{
			Sound.ReverbMic(enable: false);
		}),
		new ButtonHandler.Button("Loud Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.LoudMic(enable: true);
		}, delegate
		{
			Sound.LoudMic(enable: false);
		}),
		new ButtonHandler.Button("Mute Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.MuteMic(mute: true);
		}, delegate
		{
			Sound.MuteMic(mute: false);
		}),
		new ButtonHandler.Button("High Pitch Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SetMicPitch(1.2f);
		}, delegate
		{
			Sound.SetMicPitch(1f);
		}),
		new ButtonHandler.Button("Low Pitch Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SetMicPitch(0.75f);
		}, delegate
		{
			Sound.SetMicPitch(1f);
		}),
		new ButtonHandler.Button("Echo Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.EchoVoice(enable: true);
		}, delegate
		{
			Sound.EchoVoice(enable: false);
		}),
		new ButtonHandler.Button("Static Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.RadioVoice(enable: true);
		}, delegate
		{
			Sound.RadioVoice(enable: false);
		}),
		new ButtonHandler.Button("Muffled Microphone", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.UnderwaterVoice(enable: true);
		}, delegate
		{
			Sound.UnderwaterVoice(enable: false);
		}),
		new ButtonHandler.Button("Random Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(UnityEngine.Random.Range(0, 228));
		}),
		new ButtonHandler.Button("Annoying Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(248);
		}),
		new ButtonHandler.Button("Boop Player", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.OnTouchPlayerSoundSpam(84);
		}),
		new ButtonHandler.Button("Gong Player", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.OnTouchPlayerSoundSpam(248);
		}),
		new ButtonHandler.Button("Slap Player", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.OnTouchPlayerSoundSpam(338);
		}),
		new ButtonHandler.Button("Jman HELLO! Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(337);
		}),
		new ButtonHandler.Button("Jman Slap Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(338);
		}),
		new ButtonHandler.Button("Jman Okay Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(336);
		}),
		new ButtonHandler.Button("Glass Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(28);
		}),
		new ButtonHandler.Button("Metal Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(18);
		}),
		new ButtonHandler.Button("Pop Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(84);
		}),
		new ButtonHandler.Button("Squeaky Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(75);
		}),
		new ButtonHandler.Button("Crystal Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(213);
		}),
		new ButtonHandler.Button("Turkey Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(83);
		}),
		new ButtonHandler.Button("Frog Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(91);
		}),
		new ButtonHandler.Button("AK-47 Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(203);
		}),
		new ButtonHandler.Button("Wolf Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(195);
		}),
		new ButtonHandler.Button("Cat Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(236);
		}),
		new ButtonHandler.Button("Bee Sound Spam (RT)", Category.Audio, isToggle: true, isActive: false, delegate
		{
			Sound.SpecificSoundSpam(191);
		}),
		new ButtonHandler.Button("Tag All (RT)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.TagAll();
		}),
		new ButtonHandler.Button("Tag Gun", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.TagGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Flick Tag (RT)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.FlickTag();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Tag Aura", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.TagAura();
		}),
		new ButtonHandler.Button("Tag Self", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.TagSelf();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Try Un-Tag Self", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.UntagSelf();
		}),
		new ButtonHandler.Button("Anti Tag", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.AntiTag();
		}),
		new ButtonHandler.Button("No Tag On Join", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.NoTagOnJoin(setActive: true);
		}, delegate
		{
			Gamemode.NoTagOnJoin(setActive: false);
		}),
		new ButtonHandler.Button("No Tag Limit", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Variables.taggerInstance.maxTagDistance = float.MaxValue;
		}, delegate
		{
			Variables.taggerInstance.maxTagDistance = 1.2f;
		}),
		new ButtonHandler.Button("Untag All (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.UntagAll();
		}),
		new ButtonHandler.Button("Untag Gun (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.UntagGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Tag Lag (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Overpowered.TagLag(enable: true);
		}, delegate
		{
			Overpowered.TagLag(enable: false);
		}),
		new ButtonHandler.Button("Auto Guardian", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.AlwaysGuardian();
		}),
		new ButtonHandler.Button("Guardian Grab All (RG)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.GuardianGrabAll();
		}),
		new ButtonHandler.Button("Guardian Release All (RT)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.GuardianReleaseAll();
		}),
		new ButtonHandler.Button("Guardian Orbit All (RT)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.GuardianOrbitSelfAll();
		}),
		new ButtonHandler.Button("Guardian Fling All (RT)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.GuardianFlingAll();
		}),
		new ButtonHandler.Button("Guardian Fling Gun", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.GuardianFlingGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Guardian Bring All To Pointer", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.GuardianBringAllToPointer();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Guardian Self (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.GuardianSelf();
		}),
		new ButtonHandler.Button("Guardian All (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.GuardianAll();
		}),
		new ButtonHandler.Button("Guardian Gun (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.GuardianGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Un-Guardian Self (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.UnGuardianSelf();
		}),
		new ButtonHandler.Button("Un-Guardian All (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.UnGuardianAll();
		}),
		new ButtonHandler.Button("Un-Guardian Gun (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.UnGuardianGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Paintbrawl Aimbot", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			MenuPatches.SlingshotAimbot.enabled = true;
		}, delegate
		{
			MenuPatches.SlingshotAimbot.enabled = false;
		}),
		new ButtonHandler.Button("Paintbrawl Kill All", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlKillAll();
		}),
		new ButtonHandler.Button("Paintbrawl Kill Gun", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlKillGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Paintbrawl Kill Self", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlKillSelf();
		}),
		new ButtonHandler.Button("Paintbrawl Godmode (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlGodmode();
		}),
		new ButtonHandler.Button("Paintbrawl No Delay (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.NoPaintBrawlDelay(enable: true);
		}, delegate
		{
			Gamemode.NoPaintBrawlDelay(enable: false);
		}),
		new ButtonHandler.Button("Paintbrawl Mat All (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlMatAll();
		}),
		new ButtonHandler.Button("Paintbrawl Mat Gun (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlMatGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Paintbrawl End Game (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.EndPaintbrawlGame();
		}),
		new ButtonHandler.Button("Paintbrawl Start Game (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.StartPaintbrawlGame();
		}),
		new ButtonHandler.Button("Paintbrawl Restart Game (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.RestartPaintbrawlGame();
		}),
		new ButtonHandler.Button("Paintbrawl Spam Balloon All (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlSpamAllBalloons();
		}),
		new ButtonHandler.Button("Paintbrawl Spam Balloon Gun (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlBalloonSpamGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Paintbrawl Spam Balloon Self (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlBalloonSpamSelf();
		}),
		new ButtonHandler.Button("Paintbrawl Revive All (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.PaintbrawlReviveAll();
		}),
		new ButtonHandler.Button("Paintbrawl Revive Gun (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			Gamemode.PaintbrawlReviveGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Paintbrawl Revive Self (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			Gamemode.PaintbrawlReviveSelf();
		}),
		new ButtonHandler.Button("Float Gun (Blaster)", Category.Gamemode, isToggle: true, isActive: false, NXO.Mods.Categories.SuperInfection.GunFlingTest),
		new ButtonHandler.Button("Complete Quests", Category.Gamemode, isToggle: false, isActive: false, NXO.Mods.Categories.SuperInfection.CompleteAllQuests),
		new ButtonHandler.Button("Complete & Claim Quests", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			NXO.Mods.Categories.SuperInfection.CompleteAllQuestsAndClaimPoints(SIPlayer.LocalPlayer);
		}),
		new ButtonHandler.Button("Add TechPoints", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			NXO.Mods.Categories.SuperInfection.AddResources((SIResource.ResourceType)0, 10);
		}),
		new ButtonHandler.Button("Give All Resources", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			NXO.Mods.Categories.SuperInfection.GiveAllResourcesToMax(SIPlayer.LocalPlayer);
		}),
		new ButtonHandler.Button("Unlock All Gadgets", Category.Gamemode, isToggle: false, isActive: false, NXO.Mods.Categories.SuperInfection.UnlockAllGadgets),
		new ButtonHandler.Button("Unlock Full Tree", Category.Gamemode, isToggle: false, isActive: false, NXO.Mods.Categories.SuperInfection.UnlockFullTechTree),
		new ButtonHandler.Button("No Blaster Cooldown", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			IEnumerator<SIGadgetBlaster> enumerator = (from x in NXO.Mods.Categories.SuperInfection.GetActiveLocalPlayerGadgets()
				select ((Component)x).GetComponent<SIGadgetBlaster>() into x
				where (UnityEngine.Object)(object)x != (UnityEngine.Object)null
				select x).GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					SIGadgetBlaster current = enumerator.Current;
					NXO.Mods.Categories.SuperInfection.RemoveBlasterCooldown(current);
				}
				while (enumerator.MoveNext());
			}
		}),
		new ButtonHandler.Button("Max Charge", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			IEnumerator<SIGadgetChargeBlaster> enumerator = (from x in NXO.Mods.Categories.SuperInfection.GetActiveLocalPlayerGadgets()
				select ((Component)x).GetComponent<SIGadgetChargeBlaster>() into x
				where (UnityEngine.Object)(object)x != (UnityEngine.Object)null
				select x).GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					SIGadgetChargeBlaster current = enumerator.Current;
					NXO.Mods.Categories.SuperInfection.MaxChargeAlways(current);
				}
				while (enumerator.MoveNext());
			}
		}),
		new ButtonHandler.Button("Fast Charge", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			IEnumerator<SIGadgetChargeBlaster> enumerator = (from x in NXO.Mods.Categories.SuperInfection.GetActiveLocalPlayerGadgets()
				select ((Component)x).GetComponent<SIGadgetChargeBlaster>() into x
				where (UnityEngine.Object)(object)x != (UnityEngine.Object)null
				select x).GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					SIGadgetChargeBlaster current = enumerator.Current;
					NXO.Mods.Categories.SuperInfection.FastCharge(current);
				}
				while (enumerator.MoveNext());
			}
		}),
		new ButtonHandler.Button("No Cooldown (Dash)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			IEnumerator<SIGadgetDashYoyo> enumerator = (from x in NXO.Mods.Categories.SuperInfection.GetActiveLocalPlayerGadgets()
				select ((Component)x).GetComponent<SIGadgetDashYoyo>() into x
				where (UnityEngine.Object)(object)x != (UnityEngine.Object)null
				select x).GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					SIGadgetDashYoyo current = enumerator.Current;
					NXO.Mods.Categories.SuperInfection.RemoveDashCooldown(current);
				}
				while (enumerator.MoveNext());
			}
		}),
		new ButtonHandler.Button("Clear Exclusion Zones", Category.Gamemode, isToggle: false, isActive: false, NXO.Mods.Categories.SuperInfection.ClearAllExclusionZones),
		(PropHunt.CyclePropButton = new ButtonHandler.Button("Prop : " + PropHunt.CurrentPropName(), Category.Gamemode, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			PropHunt.CycleProp(forward: true);
		}, delegate
		{
			PropHunt.CycleProp(forward: false);
		})),
		new ButtonHandler.Button("Become Prop", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			PropHunt.BecomeProp();
		}),
		new ButtonHandler.Button("Skip Seeker Blindfold", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			PropHunt.RemoveBlindFold();
		}),
		new ButtonHandler.Button("Hiders ESP", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			PropHunt.RevealHiddenProps(enable: true);
		}, delegate
		{
			PropHunt.RevealHiddenProps(enable: false);
		}),
		new ButtonHandler.Button("Seekers ESP", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			PropHunt.SeekerESP(enable: true);
		}, delegate
		{
			PropHunt.SeekerESP(enable: false);
		}),
		new ButtonHandler.Button("Prop Tag All", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			PropHunt.PropTagAll();
		}),
		new ButtonHandler.Button("Force Round Start (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			PropHunt.ForceRoundStart();
		}),
		new ButtonHandler.Button("Force Round End (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			PropHunt.ForceRoundEnd();
		}),
		new ButtonHandler.Button("Spam Gamemode (<color=red>M</color>)", Category.Gamemode, isToggle: true, isActive: false, delegate
		{
			PropHunt.SpamGamemode();
		}),
		new ButtonHandler.Button("Become Seeker (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			PropHunt.BecomeSeeker();
		}),
		new ButtonHandler.Button("Become Hider (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			PropHunt.BecomeHider();
		}),
		new ButtonHandler.Button("Collapse Boundary (<color=red>M</color>)", Category.Gamemode, isToggle: false, isActive: false, delegate
		{
			PropHunt.CollapseBoundary();
		}),
		new ButtonHandler.Button("Snowball Effect Gun", Category.Projectiles, isToggle: true, isActive: false, delegate
		{
			Projectile.SnowballEffectPlayerGun();
		}),
		new ButtonHandler.Button("Touch to Snowball Effect", Category.Projectiles, isToggle: true, isActive: false, delegate
		{
			Projectile.TouchToSnowballEffect();
		}),
		new ButtonHandler.Button("Get Touched to Snowball Effect", Category.Projectiles, isToggle: true, isActive: false, delegate
		{
			Projectile.GetTouchedToSnowballEffect();
		}),
		new ButtonHandler.Button("Anti Knockback", Category.Projectiles, isToggle: true, isActive: false, delegate
		{
			MenuPatches.knockbackEnabled = true;
		}, delegate
		{
			MenuPatches.knockbackEnabled = false;
		}),
		new ButtonHandler.Button("Always Big Growing Snowballs", Category.Projectiles, isToggle: true, isActive: false, delegate
		{
			MenuPatches.autoBig = true;
		}, delegate
		{
			MenuPatches.autoBig = false;
		}),
		new ButtonHandler.Button("Jet Pack (Networked)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			StevesPlayground.Manager(StevesPlayground.FuncType.Jetpack, add: true);
		}, delegate
		{
			StevesPlayground.Manager(StevesPlayground.FuncType.Jetpack, add: false);
		}),
		new ButtonHandler.Button("Grappling Hook (RT/LT)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.GrapplingHook(active: true);
		}, delegate
		{
			Fun.GrapplingHook(active: false);
		}),
		new ButtonHandler.Button("Displacer Cannon (Networked)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			StevesPlayground.Manager(StevesPlayground.FuncType.DisplacerCannon, add: true);
		}, delegate
		{
			StevesPlayground.Manager(StevesPlayground.FuncType.DisplacerCannon, add: false);
		}),
		new ButtonHandler.Button("Web Shooters (RT/LT)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.Webshooters(active: true);
		}, delegate
		{
			Fun.Webshooters(active: false);
		}),
		new ButtonHandler.Button("Place Bomb (RG/RT)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.PlaceBomb(active: true);
		}, delegate
		{
			Fun.PlaceBomb(active: false);
		}),
		new ButtonHandler.Button("Punch Mod", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.PunchMod();
		}),
		new ButtonHandler.Button("Draw Mod (RG/LG)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.DrawMod(active: true);
		}, delegate
		{
			Fun.DrawMod(active: false);
		}),
		new ButtonHandler.Button("Disable Rain", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.RainyWeather(setActive: false);
		}, delegate
		{
			Fun.RainyWeather(setActive: true);
		}),
		new ButtonHandler.Button("Rain Mode", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.RainyWeather(setActive: true);
		}, delegate
		{
			Fun.RainyWeather(setActive: false);
		}),
		new ButtonHandler.Button("Toggle Snow", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Visuals.ToggleSnow(enable: true);
		}, delegate
		{
			Visuals.ToggleSnow(enable: false);
		}),
		(Settings.adjustTimeOfDayButton = new ButtonHandler.Button("Time Of Day : " + Settings.TimeOfDayDescription, Category.Fun, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustTimeOfDay(forward: true);
		}, delegate
		{
			Settings.AdjustTimeOfDay(forward: false);
		})),
		new ButtonHandler.Button("Disable Wind Barriers", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.DisableWindBarriers(enable: true);
		}, delegate
		{
			Fun.DisableWindBarriers(enable: false);
		}),
		new ButtonHandler.Button("Max Quest Score", Category.Fun, isToggle: false, isActive: false, delegate
		{
			Variables.taggerInstance.offlineVRRig.SetQuestScore(int.MaxValue);
		}),
		new ButtonHandler.Button("Add Barrel To Cart", Category.Fun, isToggle: false, isActive: false, delegate
		{
			Fun.AddBarrelToCart();
		}),
		new ButtonHandler.Button("Spam Bracelet (G)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.BraceletSpam();
		}),
		new ButtonHandler.Button("Remove Bracelets", Category.Fun, isToggle: false, isActive: false, delegate
		{
			Fun.RemoveBracelet();
		}),
		new ButtonHandler.Button("Try-On Cosmetics Anywhere", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.TryCosmeticAnywhere(enable: true);
		}, delegate
		{
			Fun.TryCosmeticAnywhere(enable: false);
		}),
		new ButtonHandler.Button("Unlock All Cosmetics", Category.Fun, isToggle: false, isActive: false, delegate
		{
			Fun.UnlockAllCosmetics();
		}),
		new ButtonHandler.Button("Loud Hand Taps", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.LoudHandTaps();
		}, delegate
		{
			Fun.FixHandTaps();
		}),
		new ButtonHandler.Button("Silent Hand Taps", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.SilentHandTaps();
		}, delegate
		{
			Fun.FixHandTaps();
		}),
		new ButtonHandler.Button("Instant Hand Taps", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Variables.taggerInstance.tapCoolDown = 0f;
		}, delegate
		{
			Variables.taggerInstance.tapCoolDown = 0.33f;
		}),
		new ButtonHandler.Button("Mute Elevator", Category.Fun, isToggle: true, isActive: true, delegate
		{
			Fun.ElevatorBuzz(enable: false);
		}, delegate
		{
			Fun.ElevatorBuzz(enable: true);
		}),
		new ButtonHandler.Button("Solid Water", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.ModifyWater(solid: true, transparent: false);
		}, delegate
		{
			Fun.ModifyWater(solid: false, transparent: false);
		}),
		new ButtonHandler.Button("Disable Water", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.ModifyWater(solid: false, transparent: true);
		}, delegate
		{
			Fun.ModifyWater(solid: false, transparent: false);
		}),
		new ButtonHandler.Button("Air Swim", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.AirSwim(active: true);
		}, delegate
		{
			Fun.AirSwim(active: false);
		}),
		new ButtonHandler.Button("Water Bender (RG/LG)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.WaterBender();
		}),
		new ButtonHandler.Button("Splash Gun", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.SplashGun();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Water Barrage (RT)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.WaterBarrage();
		}),
		new ButtonHandler.Button("Splash Aura (RT)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.SplashAura();
		}),
		new ButtonHandler.Button("Splash Self (RT)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.SplashSelf();
		}),
		new ButtonHandler.Button("Grab Gliders (RG)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.GrabGliders();
		}),
		new ButtonHandler.Button("Orbit Gliders (RT)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.OrbitGliders();
		}),
		new ButtonHandler.Button("Spaz Gliders (RT)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.SpazGliders();
		}),
		new ButtonHandler.Button("Glider Gun", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.GliderGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Glider Aura", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.GliderAura();
		}),
		new ButtonHandler.Button("Fast Gliders", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.GliderSpeed(0.4f, 0.5f);
		}, delegate
		{
			Fun.GliderSpeed(0.1f, 0.2f);
		}),
		new ButtonHandler.Button("Slowmo Gliders", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.GliderSpeed(0.04f, 0.05f);
		}, delegate
		{
			Fun.GliderSpeed(0.1f, 0.2f);
		}),
		new ButtonHandler.Button("Destroy Gliders", Category.Fun, isToggle: false, isActive: false, delegate
		{
			Fun.DestroyGliders();
		}),
		new ButtonHandler.Button("Respawn Gliders", Category.Fun, isToggle: false, isActive: false, delegate
		{
			Fun.RespawnGliders();
		}),
		new ButtonHandler.Button("Spawn Hoverboard", Category.Fun, isToggle: false, isActive: false, delegate
		{
			Fun.SpawnHoverboard();
		}),
		new ButtonHandler.Button("Shoot Hoverboards (RG)", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.ShootHoverboards();
		}),
		new ButtonHandler.Button("Orbit Hoverboards", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.OrbitHoverboards();
		}),
		new ButtonHandler.Button("Hoverboard Aura", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.HoverboardAura();
		}),
		new ButtonHandler.Button("Hoverboard Gun", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.HoverboardGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Spaz Hoverboard", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.SpazHoverboard();
		}),
		new ButtonHandler.Button("Rainbow Hoverboard", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.RainbowHoverboard();
		}),
		new ButtonHandler.Button("Strobe Hoverboard", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.StrobeHoverboard();
		}),
		new ButtonHandler.Button("Become Hoverboard", Category.Fun, isToggle: true, isActive: false, delegate
		{
			Fun.BecomeHoverboard();
		}, delegate
		{
			GTPlayer.Instance.SetHoverActive(false);
			VRRig.LocalRig.hoverboardVisual.SetNotHeld();
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Open Macros Folder", Category.Macros, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.OpenFolder(Path.Combine(Variables.folderName, "Macros"));
		}),
		new ButtonHandler.Button("Record Macro (A)", Category.Macros, isToggle: true, isActive: false, delegate
		{
			Macros.MacroController();
		}, delegate
		{
			Macros.CleanupMacro();
		}),
		new ButtonHandler.Button("Recorded Macros", Category.Macros, isToggle: false, isActive: false, delegate
		{
			ButtonHandler.ChangePage(Category.Recorded_Macros);
		})
		{
			isCategory = true
		},
		(Settings.adjustLagTypeButton = new ButtonHandler.Button("Lag Type : " + Settings.LagTypeDescription, Category.Overpowered, isToggle: false, isActive: false, null, null, incremental: true, delegate
		{
			Settings.AdjustLagType(forward: true);
		}, delegate
		{
			Settings.AdjustLagType(forward: false);
		})),
		new ButtonHandler.Button("Lag All (RT)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.LagAll();
		}),
		new ButtonHandler.Button("Lag Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.LagGun();
		}),
		new ButtonHandler.Button("Touch To Lag", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.TouchToLag();
		}),
		new ButtonHandler.Button("Get Touched To Lag", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.GetTouchedToLag();
		}),
		new ButtonHandler.Button("Lag Aura", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.LagAura();
		}),
		new ButtonHandler.Button("Stump Kick To Specific Room", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			if (!SearchAndKeyboard.isSearching)
			{
				Overpowered.SetKickRoom();
			}
		}, delegate
		{
			Overpowered.ClearKickRoom();
		}),
		new ButtonHandler.Button("Fast Stump Kick", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Room.instantCreate = true;
		}, delegate
		{
			Room.instantCreate = false;
		}),
		new ButtonHandler.Button("Stump Kick Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.StumpKickGun();
		}),
		new ButtonHandler.Button("Stump Kick All", Category.Overpowered, isToggle: false, isActive: false, delegate
		{
			Overpowered.StumpKickAll();
		}),
		new ButtonHandler.Button("Destroy Photon.Realtime.Player Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.DestroyGun();
		}),
		new ButtonHandler.Button("Destroy All", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.DestroyAll();
		}),
		new ButtonHandler.Button("Deafen Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.DeafenGun();
		}),
		new ButtonHandler.Button("Deafen All", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.DeafenAll();
		}),
		new ButtonHandler.Button("Touch To Deafen", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.TouchToDeafen();
		}),
		new ButtonHandler.Button("Get Touched To Deafen", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.GetTouchedToDeafen();
		}),
		new ButtonHandler.Button("Earrape Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.EarrapeGun();
		}, delegate
		{
			Sound.Earrape(enable: false);
			Overpowered._earrapeGunActive = false;
			MenuPatches.SerializationPatch.Override = null;
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Earrape All", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.EarrapeAll();
		}, delegate
		{
			Sound.Earrape(enable: false);
			MenuPatches.SerializationPatch.Override = null;
		}),
		new ButtonHandler.Button("Barrel Fling Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.BarrelFlingGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Touch To Barrel Fling", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.TouchToBarrelFling();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Get Touched To Barrel Fling", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.GetTouchedToBarrelFling();
		}, delegate
		{
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Force Grab (RG)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.ForceGrab();
		}),
		new ButtonHandler.Button("Force Grab Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.ForceGrabGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Fling On Grab", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.FlingOnGrab();
		}),
		new ButtonHandler.Button("Metro Crash On Grab", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.MetroMapCrashOnGrab();
		}),
		new ButtonHandler.Button("Fling To Point On Grab", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.FlingTowardsPointOnGrab(enable: true);
		}, delegate
		{
			Overpowered.FlingTowardsPointOnGrab(enable: false);
		}),
		new ButtonHandler.Button("Try Crash Modders On Grab", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.CrashModdersOnGrab();
		}),
		new ButtonHandler.Button("Splash Annoy Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.SplashAnnoyGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Splash Annoy All (RT)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.SplashAnnoyAll();
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
		}),
		new ButtonHandler.Button("Seizure Screen Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			if (!(Time.time % 0.2f > 0.1f))
			{
				Fun.HoverboardScreenGun(Color.cyan);
			}
			else
			{
				Fun.HoverboardScreenGun(Color.red);
			}
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			GunLib.SetGunVisibility(isVisible: false);
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Seizure Screen All (RT)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			if (!(Time.time % 0.2f > 0.1f))
			{
				Fun.HoverboardScreenAll(Color.cyan);
			}
			else
			{
				Fun.HoverboardScreenAll(Color.red);
			}
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Strobe Screen Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.HoverboardScreenGun(Variables.RandomColor());
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			GunLib.SetGunVisibility(isVisible: false);
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Strobe Screen All (RT)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.HoverboardScreenAll(Variables.RandomColor());
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Flash Screen Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			if (!(Time.time % 0.2f > 0.1f))
			{
				Fun.HoverboardScreenGun(Color.black);
			}
			else
			{
				Fun.HoverboardScreenGun(Color.white);
			}
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			GunLib.SetGunVisibility(isVisible: false);
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Flash Screen All (RT)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			if (!(Time.time % 0.2f > 0.1f))
			{
				Fun.HoverboardScreenAll(Color.black);
			}
			else
			{
				Fun.HoverboardScreenAll(Color.white);
			}
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Black Screen Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.HoverboardScreenGun(Color.black);
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			GunLib.SetGunVisibility(isVisible: false);
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Black Screen All (RT)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.HoverboardScreenAll(Color.black);
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("White Screen Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.HoverboardScreenGun(Color.white);
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			GunLib.SetGunVisibility(isVisible: false);
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("White Screen All (RT)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.HoverboardScreenAll(Color.white);
		}, delegate
		{
			MenuPatches.SerializationPatch.Override = null;
			Player.SetRigStatus(rigStatus: true);
		}),
		new ButtonHandler.Button("Glider Annoy All (RT)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.GliderBlindAll();
		}),
		new ButtonHandler.Button("Glider Annoy Gun", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Fun.GliderBlindGun();
		}, delegate
		{
			GunLib.SetGunVisibility(isVisible: false);
		}),
		new ButtonHandler.Button("Lock Room", Category.Overpowered, isToggle: false, isActive: false, delegate
		{
			Overpowered.SetRoomStatus(status: false);
		}),
		new ButtonHandler.Button("Unlock Room", Category.Overpowered, isToggle: false, isActive: false, delegate
		{
			Overpowered.SetRoomStatus(status: true);
		}),
		new ButtonHandler.Button("Enable Event (<color=red>M</color>)", Category.Overpowered, isToggle: false, isActive: false, delegate
		{
			if (Variables.IsMaster())
			{
				((GreyZoneManager)GreyZoneManager.Instance).ActivateGreyZoneAuthority();
			}
		}),
		new ButtonHandler.Button("Disable Event (<color=red>M</color>)", Category.Overpowered, isToggle: false, isActive: false, delegate
		{
			if (Variables.IsMaster())
			{
				((GreyZoneManager)GreyZoneManager.Instance).DeactivateGreyZoneAuthority();
			}
		}),
		new ButtonHandler.Button("Spam Event (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.SpamEvent();
		}),
		new ButtonHandler.Button("Infinite Event (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			if (Variables.IsMaster())
			{
				((GreyZoneManager)GreyZoneManager.Instance).greyZoneActiveDuration = float.MaxValue;
			}
		}, delegate
		{
			if (Variables.IsMaster())
			{
				((GreyZoneManager)GreyZoneManager.Instance).greyZoneActiveDuration = 90f;
			}
		}),
		new ButtonHandler.Button("Slow All (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.SlowAll();
		}),
		new ButtonHandler.Button("Slow Gun (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.SlowGun();
		}),
		new ButtonHandler.Button("Slow Aura (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.SlowAura();
		}),
		new ButtonHandler.Button("Slow On Touch (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.SlowOnTouch();
		}),
		new ButtonHandler.Button("Vibrate All (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.VibrateAll();
		}),
		new ButtonHandler.Button("Vibrate Gun (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.VibrateGun();
		}),
		new ButtonHandler.Button("Vibrate Aura (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.VibrateAura();
		}),
		new ButtonHandler.Button("Vibrate On Touch (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.VibrateOnTouch();
		}),
		new ButtonHandler.Button("Mat Spam All (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.MatSpamAll();
		}),
		new ButtonHandler.Button("Mat Spam Gun (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.MatSpamGun();
		}),
		new ButtonHandler.Button("Touch To Mat Spam (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.TouchToMatSpam();
		}),
		new ButtonHandler.Button("Get Touched To Mat Spam (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.GetTouchedToMatSpam();
		}),
		new ButtonHandler.Button("Spaz All Targets (<color=red>M</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.SpazAllTargets();
		}),
		new ButtonHandler.Button("Tagged Sound (<color=red>M/RT</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.RoomSoundSpam(0);
		}),
		new ButtonHandler.Button("Round End Sound (<color=red>M/RT</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.RoomSoundSpam(2);
		}),
		new ButtonHandler.Button("Bonk Sound (<color=red>M/RT</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.RoomSoundSpam(4);
		}),
		new ButtonHandler.Button("Count Sound (<color=red>M/RT</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.RoomSoundSpam(1);
		}),
		new ButtonHandler.Button("Brawl Sound (<color=red>M/RT</color>)", Category.Overpowered, isToggle: true, isActive: false, delegate
		{
			Overpowered.RoomSoundSpam(7);
		}),
		new ButtonHandler.Button("Return", Category.Gear_Menu, isToggle: false, isActive: false, delegate
		{
			if (ButtonHandler._gearTargetButton != null)
			{
				ButtonHandler.ChangePage(ButtonHandler._gearTargetButton.Page);
			}
			else
			{
				ButtonHandler.ChangePage(Category.Home);
			}
		})
		{
			isCategory = true
		},
		new ButtonHandler.Button("Tooltip", Category.Gear_Menu, isToggle: false, isActive: false, delegate
		{
			if (ButtonHandler._gearTargetButton?.tooltip != null)
			{
				NotificationLib.SendNotification(NotificationLib.NotificationType.Info, ButtonHandler._gearTargetButton.tooltip);
			}
		})
	};

	public static ButtonHandler.Button[] buttons
	{
		get
		{
			return _buttons;
		}
		set
		{
			_buttons = value;
			Main.MarkActiveTicksDirty();
		}
	}
}
