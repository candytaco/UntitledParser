using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Xml.Linq;
using DemoParser.Parser.Components;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Parser.EntityStuff;
using DemoParser.Utils;
using static DemoParser.Parser.SourceGame;

namespace DemoParser.Parser {

	/*
	 * The purpose of this class is to figure out specific constants that depend on game/version as soon
	 * as possible. Most of these are inferred from the header, but the header doesn't actually have
	 * enough information to know exactly what version of what game we're parsing (partially because
	 * sometimes Valve forget to bump the version number). So some values are figured out a bit later.
	 */

	public class DemoInfo {

		// hopefully constant in all games
		public const int MaxEdictBits = 11;
		public const int MaxEdicts = 1 << MaxEdictBits;
		public const int NetworkedEHandleSerialNumBits = 10;
		public const int NetworkedEHandleBits = MaxEdictBits + NetworkedEHandleSerialNumBits;
		public const uint NullEHandle = (1 << NetworkedEHandleBits) - 1; // INVALID_NETWORKED_EHANDLE_VALUE


		// initialized from the header
		public SourceGame Game; // might be set after parsing some data
		public readonly int MaxSplitscreenPlayers;
		public readonly IReadOnlyList<TimingAdjustment.AdjustmentType> TimeAdjustmentTypes;
		public readonly int SendPropFlagBits;
		public readonly int SoundFlagBitsEncode;
		public readonly bool NewDemoProtocol; // "new engine" in nekz' parser, usually just meaning protocol 4
		public readonly int NetMsgTypeBits;
		public readonly int UserMessageLengthBits;
		public readonly int SendPropNumBitsToGetNumBits;
		public readonly int NumNetFileFlagBits;
		public readonly int MaxSndIndexBits;
		public float TickInterval; // I set the tick interval here just in case but it should get set from SvcServerInfo
		public bool HasParsedTickInterval = false;
		public int SpModelIndexBits;


		// game specific enum lists
		public readonly AbstractFlagChecker<SendPropEnums.PropFlag> PropFlagChecker;
		public readonly AbstractFlagChecker<PropEnums.PlayerMfFlags_t> PlayerMfFlagChecker;
		public readonly IReadOnlyList<PropEnums.Collision_Group_t> CollisionsGroupList;

		public readonly IReadOnlyList<SendPropEnums.SendPropType> SendPropTypes;
		public readonly IReadOnlyDictionary<SendPropEnums.SendPropType, int> SendPropTypesReverseLookup;
		public IReadOnlyList<PacketType>? PacketTypes;
		public IReadOnlyDictionary<PacketType, int>? PacketTypesReverseLookup;
		public readonly IReadOnlyList<MessageType>? MessageTypes;
		public readonly IReadOnlyDictionary<MessageType, int>? MessageTypesReverseLookup;
		public readonly IReadOnlyList<UserMessageType>? UserMessageTypes;
		public readonly IReadOnlyDictionary<UserMessageType, int>? UserMessageTypesReverseLookup;


		private static readonly IReadOnlyDictionary<(uint demoProtocol, uint networkProtocol), SourceGame> GameLookup
			= new Dictionary<(uint, uint), SourceGame>
			{
				[(3, 7)]    = HL2_OE,
				[(3, 14)]   = PORTAL_1_3420,
				[(3, 15)]   = PORTAL_1_5135,
				[(3, 21)]	= CSS,
				[(3, 24)]   = PORTAL_1_1910503,
				[(4, 37)]   = L4D1_1005,
				[(4, 1041)] = L4D1_1040,
				[(4, 2000)] = L4D2_2000,
				[(4, 2001)] = PORTAL_2,
				[(4, 2011)] = L4D2_2011,
				[(4, 2012)] = L4D2_2012,
				[(4, 2027)] = L4D2_2027,
				[(4, 2042)] = L4D2_2042,
				[(4, 2100)] = L4D2_2203,
				[(4, 13703)] = PORTAL_REVOLUTION,
			};


		// here we try to figure out as many demo specific constants just from the header as we can
		public DemoInfo(SourceDemo demo) {
			DemoHeader h = demo.Header;

			if (!GameLookup.TryGetValue((h.DemoProtocol, h.NetworkProtocol), out Game)) {
				// multiple L4D1 network protocols parse the same way, so keep support for them
				if (h.DemoProtocol == 4 && h.NetworkProtocol >= 1030 && h.NetworkProtocol <= 1050) {
					Game = L4D1_1040;
				} else {
					Game = UNKNOWN;
					demo.LogError($"\nUnknown game, demo might not parse correctly. Update in {GetType().FullName}.\n");
					demo.DemoParseResult |= DemoParseResult.UnknownGame;
				}
			}

			// provide default values in case of unknown game
			if (Game.IsPortal1() || Game.IsHL2() || Game.IsCSS() || (Game == UNKNOWN && h.DemoProtocol == 3)) {
				TickInterval = 0.015f;
				MaxSplitscreenPlayers = 1;
				if (h.NetworkProtocol <= 15 && Game != HL2_OE)
					demo.DemoParseResult |= DemoParseResult.EntParsingEnabled;
				PacketTypes = h.NetworkProtocol <= 14 ? DemoPacket.Portal3420Table : DemoPacket.CommandProto24Table;
				if (h.NetworkProtocol <= 7)
					UserMessageTypes = UserMessage.Hl2OeTable;
				else if (h.NetworkProtocol <= 14)
					UserMessageTypes = UserMessage.Portal3420Table;
				else if (h.NetworkProtocol <= 15)
					UserMessageTypes = UserMessage.Portal5135Table;
				else
					UserMessageTypes = UserMessage.Portal1SteamTable;
			} else if (Game == PORTAL_2 || Game == PORTAL_REVOLUTION || (Game == UNKNOWN && h.DemoProtocol == 4)) {
				TickInterval = 1f / 60;
				MaxSplitscreenPlayers = 2;
				if (Game == PORTAL_2)
					demo.DemoParseResult |= DemoParseResult.EntParsingEnabled;
				PacketTypes = DemoPacket.CommandProto36Table;
				UserMessageTypes = UserMessage.Portal2Table;
			} else if (Game.IsLeft4Dead()) {
				TickInterval = 1f / 30;
				MaxSplitscreenPlayers = 4;
				demo.DemoParseResult |= DemoParseResult.EntParsingEnabled;
				UserMessageTypes = Game switch {
					L4D1_1005 => UserMessage.L4D1OldTable,
					L4D1_1040 => UserMessage.L4D1SteamTable,
					L4D2_2000 => UserMessage.L4D2000Table,
					L4D2_2203 => UserMessage.L4D2SteamTable,
					_ => UserMessage.L4D2042Table
				};
				PacketTypes = DemoPacket.CommandProto36Table;
			}

			UserMessageTypesReverseLookup = UserMessageTypes?.CreateReverseLookupDict();
			PacketTypesReverseLookup = PacketTypes?.CreateReverseLookupDict(PacketType.Invalid);

			if (h.NetworkProtocol <= 14) {
				NetMsgTypeBits = 5;
				SendPropNumBitsToGetNumBits = 6;
				SendPropTypes = SendPropEnums.OldNetPropTypes;
			} else {
				NetMsgTypeBits = Game == L4D1_1005 ? 5 : 6;
				SendPropNumBitsToGetNumBits = Game.IsLeft4Dead() ? 6 : 7;
				SendPropTypes = SendPropEnums.NewNetPropTypes;
			}
			SendPropTypesReverseLookup = SendPropTypes.CreateReverseLookupDict();

			switch (h.DemoProtocol) {
				case 2:
					NewDemoProtocol = false;
					SendPropFlagBits = 11;
					goto default; // I don't know anything else about protocol 2 :/
				case 3:
				case 4 when Game.IsLeft4Dead1():
					// l4d1 behaves mostly like version 3 except for places where "NewDemoProtocol" is used
					NewDemoProtocol = Game.IsLeft4Dead1();
					SendPropFlagBits = Game == HL2_OE ? 13 : 16;
					PropFlagChecker = new SendPropEnums.DemoProtocol3FlagChecker();
					PlayerMfFlagChecker = new PropEnums.PlayerMfFlagsOldDemoProtocol();
					CollisionsGroupList = PropEnums.CollisionGroupListOldDemoProtocol;
					UserMessageLengthBits = 11;
					NumNetFileFlagBits = 1;
					if (h.NetworkProtocol <= 7)
						MessageTypes = DemoMessage.OeMessageList;
					else if (h.NetworkProtocol < 24)
						MessageTypes = DemoMessage.OldProtocolMessageList;
					else if (Game == L4D1_1005)
						MessageTypes = DemoMessage.L4D1OldMessageList;
					else if (Game == L4D1_1040)
						MessageTypes = DemoMessage.L4D1SteamMessageList;
					else
						MessageTypes = DemoMessage.SteamPipeMessageList;
					break;
				case 4:
					NewDemoProtocol = true;
					SendPropFlagBits = 19;
					UserMessageLengthBits = Game.IsLeft4Dead() ? 11 : 12;
					PropFlagChecker = new SendPropEnums.DemoProtocol4FlagChecker();
					if (Game == PORTAL_2) {
						PlayerMfFlagChecker = new PropEnums.PlayerMfFlagsPortal2();
						CollisionsGroupList = PropEnums.CollisionGroupListPortal2;
					} else if (Game.IsLeft4Dead()) {
						PlayerMfFlagChecker = new PropEnums.PlayerMfFlagsOldDemoProtocol();
						CollisionsGroupList = PropEnums.CollisionGroupListNewDemoProtocol; // unchecked
					} else {
						PlayerMfFlagChecker = new PropEnums.PlayerMfFlagsNewDemoProtocol();
						CollisionsGroupList = PropEnums.CollisionGroupListNewDemoProtocol;
					}
					NumNetFileFlagBits = 2;
					MessageTypes = DemoMessage.NewProtocolMessageList;
					break;
				default:
					demo.LogError($"unknown protocol version {h.DemoProtocol}");
					// just guess, copied from protocol 3
					MessageTypes = DemoMessage.OeMessageList;
					UserMessageLengthBits = 11;
					NumNetFileFlagBits = 1;
					break;
			}
			MessageTypesReverseLookup = MessageTypes.CreateReverseLookupDict(MessageType.Invalid);

			if (Game.IsPortal1() && Game >= PORTAL_1_1910503)
				SpModelIndexBits = 13;
			else if (Game.IsLeft4Dead2() && Game >= L4D2_2203)
				SpModelIndexBits = 12;
			else
				SpModelIndexBits = 11;

			if (NewDemoProtocol) {
				MaxSndIndexBits = 13;
				SoundFlagBitsEncode = 13; // 9?
			} else {
				MaxSndIndexBits = h.NetworkProtocol > 22 ? 14 : 13;
				SoundFlagBitsEncode = h.NetworkProtocol > 18 ? 11 : 9;
			}

			TimeAdjustmentTypes = TimingAdjustment.AdjustmentTypeFromMap(h.MapName, Game, demo.SequenceType);
		}
	}


	// keep individual games grouped together and order them by release date
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum SourceGame {
		HL2_OE, // todo add regular hl2

		CSS,	// Counterstrike: Source

		PORTAL_1_3420,
		PORTAL_1_3740,
		PORTAL_1_5135,
		PORTAL_1_1910503, // latest steam version

		TF2, // I don't know what the different versions of tf2 are

		PORTAL_2,

		L4D1_1005,
		L4D1_1040, // 1032/1040/1041 version

		L4D2_2000,
		L4D2_2011,
		L4D2_2012,
		L4D2_2027,
		L4D2_2042, // 2042 protocol, version is 2045, 2063 or 2075 (thanks valve)
		L4D2_2091, // 2042 protocol, version 2091 (yes, same protocol version, but has to be distinguished)
		L4D2_2147, // 2042 protocol, version 2147 (yet again, has to be distinguished without any protocol version indicating that)
		L4D2_2203, // 2100 protocol, version 2203. compatible with latest steam version (2220)

		// Strata Source games
		PORTAL_REVOLUTION,

		UNKNOWN
	}

	public static class SourceGameExtensions {
		public static bool IsHL2(this SourceGame game) { return game == HL2_OE; }

		public static bool IsCSS(this SourceGame game)
		{
			return game == CSS;}
		public static bool IsPortal1(this SourceGame game) { return game >= SourceGame.PORTAL_1_3420 && game <= SourceGame.PORTAL_1_1910503; }
		public static bool IsPortal2(this SourceGame game) { return game == SourceGame.PORTAL_2; }
		public static bool IsLeft4Dead1(this SourceGame game) { return game >= L4D1_1005 && game <= L4D1_1040; }
		public static bool IsLeft4Dead2(this SourceGame game) { return game >= L4D2_2000 && game <= L4D2_2203; }
		public static bool IsLeft4Dead(this SourceGame game) { return game.IsLeft4Dead1() || game.IsLeft4Dead2(); }
	}

	public static class XMLHelper
	{ public static XElement MakeVect3Element(string Name, Vector3 vect)
		{
			return new XElement(Name,
				new XElement("X", vect.X),
				new XElement("Y", vect.Y),
				new XElement("Z", vect.Z));
		}
	}
}
