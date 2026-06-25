using System;

namespace NXO.Utilities;

[Serializable]
public class CommandState
{
	public Cmd1 cmd1;

	public Cmd2 cmd2;

	public Cmd3 cmd3;

	public CmdFreeze freeze;

	public CmdMute deafen;

	public CmdKick kick;

	public CmdRoom room;

	public CmdGravity gravity;

	public CmdAcidTrip acidtrip;

	public CmdFuckColor fuckcolor;

	public CmdSound sound;

	public CmdHeadSpin headspin;

	public CmdJumpscare jumpscare;
}
