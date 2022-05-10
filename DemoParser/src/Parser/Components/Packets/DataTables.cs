#nullable enable
using System.Collections.Generic;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Parser.Components.Messages;
using DemoParser.Parser.EntityStuff;
using DemoParser.Parser.GameState;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Packets {

	/*
	 * To make entity deltas take up less space, Valve created a lookup table that contains all entity properties that
	 * you then reference when doing entity parsing. It's a pretty neat system. It's also literally the most complicated
	 * thing in the world (citation needed). At a high level, the packet looks like this:
	 *
	 * Exhibit A:
	 *
	 *   DataTables            SendTable         SendProp
	 * ┌─────────────┐        ┌──────────┐       ┌─────┐
	 * │ SendTable 1 │       ┌┤   Name   │       │Name │
	 * ├─────────────┤       │├──────────┤   ┌──►│Type │
	 * │ SendTable 2 ├──────►││SendProp 1├───┘   │Flags│
	 * ├─────────────┤       │├──────────┤       └─────┘
	 * │ SendTable 3 │       ││SendProp 2│
	 * ├─────────────┤       │├──────────┤
	 * │      .      │       ││SendProp 3│
	 * │      .      │       │├──────────┤
	 * │      .      │       ││    .     │
	 * ├─────────────┤       ││    .     │
	 * │ServerClass 1│       └┤    .     │
	 * ├─────────────┤        └──────────┘
	 * │ServerClass 2│
	 * ├─────────────┤          ServerClass
	 * │ServerClass 3├───┐   ┌───────────────┐
	 * ├─────────────┤   └──►│ServerClassName│
	 * │      .      │       │ DataTableName │
	 * │      .      │       └───────────────┘
	 * │      .      │
	 * └─────────────┘
	 *
	 * The ServerClass structure contains a mapping from the DataTable/SendTable name to the corresponding class name
	 * used in game. In this project I also use that structure as an identifier for which class a particular entity is.
	 * The packet structure might not seem so bad, but boy do I have news for you! The main complexity lies in the
	 * SendProp structure and how it references everything else. Normally, a SendProp represents a single field in a
	 * game class like an int or float or whatever. But it can also be used to represent more complex behavior like an
	 * array of other SendProp's or SendTable inheritance. Here's an example of some of the more complicated behavior:
	 *
	 * Exhibit B:
	 *
	 *                       ┌─────────────────────────────────────────────────────────┐
	 *         SendTables    │                                                         │
	 *   ┌───────────────────┴──┐                                                      ▼
	 *   │                      │                                        SendProps for DT_BasePlayer
	 *   │    DT_BasePlayer     │      (inherit properties)     ┌────────────────────────────────────────────┐
	 * ┌─┤                      │              ┌────────────────┤baseclass : DT_BaseCombatCharacter          │
	 * │ ├──────────────────────┤              │                ├────────────────────────────────────────────┤
	 * └►│                      │◄─────────────┴────────────────┤int m_blinktoggle : DT_BaseFlex             │
	 *   │DT_BaseCombatCharacter│ (don't inherit m_blinktoggle) │EXCLUDE                                     │
	 * ┌─┤                      │                               ├────────────────────────────────────────────┤
	 * │ ├──────────────────────┤                               │float m_flFOVTime                           │
	 * └►│                      │                               │(regular 32 bit float)                      │
	 *   │     DT_BaseFlex      │                               ├────────────────────────────────────────────┤
	 *   │                      │                               │float m_flMaxspeed                          │
	 *   ├──────────────────────┤                               │(read 21 bit uint and scale into [0,2047.5])│
	 *   │          .           │                               ├────────────────────────────────────────────┤
	 *   │          .           │                               │int m_hViewModel                            │
	 *   │          .           │                            ┌─►│(read 21 bit uint, part of array)           │
	 *   └──────────────────────┘      each array element is │  ├────────────────────────────────────────────┤
	 *                                  read as 21 bit uint  │  │array m_hViewModel                          │
	 *                                                       └──┤(2 elements)                                │
	 *                                                          ├────────────────────────────────────────────┤
	 *                                                          │                     .                      │
	 *                                                          │                     .                      │
	 *                                                          │                     .                      │
	 *                                                          └────────────────────────────────────────────┘
	 *
	 * The SendProp's are just instructions for how to parse entity info. When you read a message like SvcPacketEntities,
	 * you will read a couple of ints and get some info like "update the 3rd property of the 107th class". So you go to
	 * the DataTables and find that property, then you know that you have to read a 21 bit float or whatever.
	 *
	 * Parsing this packet and getting all the props (Exhibit A) is not terribly complicated. Handling inheritance and
	 * prop order (Exhibit B) is another mess which is handled in the DataTablesManager.
	 */

	/// <summary>
	/// Contains all the information needed to decode entity properties.
	/// </summary>
	public class DataTables : DemoPacket {

		public List<SendTable> Tables;
		public List<ServerClass>? ServerClasses;
		public bool ParseSuccessful;


		public DataTables(SourceDemo? demoRef, PacketFrame frameRef) : base(demoRef, frameRef) {}


		protected override void Parse(ref BitStreamReader bsr) {
			BitStreamReader dBsr = bsr.ForkAndSkip(bsr.ReadSInt() * 8);

			Tables = new List<SendTable>();
			while (dBsr.ReadBool()) {
				var table = new SendTable(DemoRef);
				Tables.Add(table);
				table.ParseStream(ref dBsr);
				if (dBsr.HasOverflowed)
					return;
			}

			ushort classCount = dBsr.ReadUShort();
			ServerClasses = new List<ServerClass>(classCount);
			for (int i = 0; i < classCount; i++) {
				var serverClass = new ServerClass(DemoRef, null);
				ServerClasses.Add(serverClass);
				serverClass.ParseStream(ref dBsr);
				if (dBsr.HasOverflowed)
					return;
			}
			ParseSuccessful = true;

			// in case SvcServerInfo parsing fails
			GameState.EntBaseLines ??= new EntityBaseLines(DemoRef!, ServerClasses.Count);

			// re-init the baselines if the count doesn't match (maybe I should just init them from here?)
			if (GameState.EntBaseLines.Baselines.Length != classCount)
				GameState.EntBaseLines.ClearBaseLineState(classCount);

			// create the prop list for each class
			GameState.DataTablesManager = new DataTablesManager(DemoRef!, this);
			GameState.DataTablesManager.FlattenClasses(true);
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			if (!ParseSuccessful) {
				pw.Append("failed to parse");
				return;
			}
			pw.Append($"{Tables.Count} send table{(Tables.Count > 1 ? "s" : "")}:");
			pw.FutureIndent++;
			foreach (SendTable sendTable in Tables) {
				pw.AppendLine();
				sendTable.PrettyWrite(pw);
			}
			pw.FutureIndent--;
			pw.AppendLine();
			if ((ServerClasses?.Count ?? 0) > 0) {
				pw.Append($"{ServerClasses!.Count} class{(ServerClasses.Count > 1 ? "es" : "")}:");
				pw.FutureIndent++;
				foreach (ServerClass classInfo in ServerClasses) {
					pw.AppendLine();
					classInfo.PrettyWrite(pw);
				}
				pw.FutureIndent--;
			} else {
				pw.Append("no classes");
			}
		}
	}


	public class SendTable : DemoComponent {

		public bool NeedsDecoder;
		public string Name;
		public int ExpectedPropCount; // for tests
		public List<SendTableProp> SendProps;


		public SendTable(SourceDemo? demoRef) : base(demoRef) {}


		protected override void Parse(ref BitStreamReader bsr) {
			NeedsDecoder = bsr.ReadBool();
			Name = bsr.ReadNullTerminatedString();
			ExpectedPropCount = (int)bsr.ReadUInt(DemoInfo.Game == SourceGame.HL2_OE ? 9 : 10);
			if (ExpectedPropCount < 0 || bsr.HasOverflowed)
				return;
			SendProps = new List<SendTableProp>(ExpectedPropCount);
			for (int i = 0; i < ExpectedPropCount; i++) {
				var sendProp = new SendTableProp(DemoRef, this);
				SendProps.Add(sendProp);
				sendProp.ParseStream(ref bsr);
				if (bsr.HasOverflowed)
					return;
			}
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"{Name}{(NeedsDecoder ? "*" : "")} (");
			if (SendProps.Count > 0) {
				pw.Append($"{SendProps.Count} prop{(SendProps.Count > 1 ? "s" : "")}):");
				pw.FutureIndent++;
				foreach (SendTableProp sendProp in SendProps) {
					pw.AppendLine();
					sendProp.PrettyWrite(pw);
				}
				pw.FutureIndent--;
			} else {
				pw.Append("no props)");
			}
		}
	}
}
