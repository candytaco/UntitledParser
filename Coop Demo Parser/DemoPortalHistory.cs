using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using DemoParser.Parser.Components.Messages.UserMessages;

namespace Coop_Demo_Parser;

/**
 * Because multiple portal effect messages can happen on the same tick
 * these classes take these messages and turn information into more of a
 * timeseries for the state of the portals across the entire thing
 */
public class DemoPortalHistory
{
	public List<DemoFramePortals> frames;

	public DemoPortalHistory()
	{
		frames = new List<DemoFramePortals>();
	}

	/**
	 * Add a new message, assumes that portal closed messages will be given before portal opened messages
	 */
	public void AddPortalEffectMessage(PortalFxSurface message, int tick)
	{
		PropagateState(tick);
		DemoFramePortals changedFrame;
		if (tick == frames.Last().Tick)	// edit last frame in-place
			changedFrame = frames.Last();
		else
		{
			changedFrame = frames.Last().Copy();
			frames.Add(changedFrame);
		}

		int player = message.OwnerEnt;
		int portal = message.PortalNum;

		switch (message.Effect)
		{
			case PortalFizzleType.PortalFizzleSuccess:
				changedFrame.Player(player).Portal(portal).Position = message.Origin;
				changedFrame.Player(player).Portal(portal).Orientation = message.Angles;
				break;
			case PortalFizzleType.PortalFizzleCantFit:
				break;
			case PortalFizzleType.PortalFizzleOverlappedLinked:
				break;
			case PortalFizzleType.PortalFizzleBadVolume:
				break;
			case PortalFizzleType.PortalFizzleBadSurface:
				break;
			case PortalFizzleType.PortalFizzleKilled:
				changedFrame.Player(player).Portal(portal).Position *= float.NaN;
				changedFrame.Player(player).Portal(portal).Orientation *= float.NaN;
				break;
			case PortalFizzleType.PortalFizzleCleanser:
				changedFrame.Player(player).Portal(portal).Position *= float.NaN;
				changedFrame.Player(player).Portal(portal).Orientation *= float.NaN;
				break;
			case PortalFizzleType.PortalFizzleClose:
				changedFrame.Player(player).Portal(portal).Position *= float.NaN;
				changedFrame.Player(player).Portal(portal).Orientation *= float.NaN;
				break;
			case PortalFizzleType.PortalFizzleNearBlue:
				break;
			case PortalFizzleType.PortalFizzleNearRed:
				break;
			case PortalFizzleType.PortalFizzleNone:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	/**
	 * Propagate previous portal info to current state
	 */
	public void PropagateState(int tick)
	{
		if (frames.Count < 1)
		{
			frames.Add(new DemoFramePortals(0));
		}

		while (frames.Last().Tick < tick - 1)
		{
			frames.Add(new DemoFramePortals(frames.Last()));
		}

	}
}

public struct DemoFramePortals
{
	public PlayerPortalInfo Player1 = new PlayerPortalInfo();
	public PlayerPortalInfo Player2 = new PlayerPortalInfo();

	public int Tick { get; private set; }

	public DemoFramePortals(int tick)
	{
		Tick = tick;
	}

	public DemoFramePortals(DemoFramePortals last)
	{
		this.Tick = last.Tick + 1;
		this.Player1 = last.Player1;
		this.Player2 = last.Player2;
	}

	public DemoFramePortals Copy()
	{
		DemoFramePortals outVal = new DemoFramePortals(Tick + 1);
		outVal.Player1 = this.Player1.Copy();
		outVal.Player2 = this.Player2.Copy();
		return outVal;
	}

	public void Write(StreamWriter writer)
	{
		writer.Write(Tick); writer.Write(",");
		Player1.Write(writer);
		Player2.Write(writer);
	}

	public PlayerPortalInfo Player(int player)
	{
		if (player < 2)
			return this.Player1;
		return this.Player2;
	}
}

public class PlayerPortalInfo
{
	public PortalInfo Portal1 = new PortalInfo();
	public PortalInfo Portal2 = new PortalInfo();
	public PlayerPortalInfo()
	{
	}

	public PlayerPortalInfo(PortalInfo portal1, PortalInfo portal2)
	{
		Portal1 = portal1;
		Portal2 = portal2;
	}

	public PlayerPortalInfo Copy()
	{
		return new PlayerPortalInfo(Portal1.Copy(), Portal2.Copy());
	}

	public void Write(StreamWriter writer)
	{
		Portal1.Write(writer);
		Portal2.Write(writer);
	}

	public PortalInfo Portal(int portalIndex)
	{
		if (portalIndex < 2)
			return Portal1;
		return Portal2;
	}
}

public class PortalInfo
{
	public Vector3 Position = new Vector3(Single.NaN);
	public Vector3 Orientation = new Vector3(Single.NaN);

	public PortalInfo()
	{
	}

	public PortalInfo(Vector3 position, Vector3 orientation)
	{
		Position = position;
		Orientation = orientation;
	}

	public PortalInfo(PortalInfo other)
	{
		this.Position = other.Position;
		this.Orientation = other.Orientation;
	}

	public void Write(StreamWriter writer)
	{
		writer.Write("{0},{1},{2},", Position.X, Position.Y, Position.Z);
		writer.Write("{0},{1},{2},", Orientation.X, Orientation.Y, Orientation.Z);
	}

	public PortalInfo Copy()
	{
		return new PortalInfo(new Vector3(this.Position.X, this.Position.Y, this.Position.Z),
			new Vector3(this.Orientation.X, this.Orientation.Y, this.Orientation.Z));
	}
}