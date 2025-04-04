using System;
using System.Xml.Linq;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Messages {

	public class SvcCreateStringTable : DemoMessage {

		public string TableName;
		public short MaxEntries;
		public int NumEntries;
		public bool UserDataFixedSize;
		public int UserDataSize;
		public int UserDataSizeBits;
		public StringTableFlags? Flags;
		public StringTableUpdates? TableUpdates;


		public SvcCreateStringTable(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			TableName = bsr.ReadNullTerminatedString();
			MaxEntries = (short)bsr.ReadUShort();
			NumEntries = (int)bsr.ReadUInt(ParserUtils.HighestBitIndex((uint)MaxEntries) + 1);
			uint dataLen = bsr.ReadUInt(DemoInfo.Game.IsLeft4Dead2() ? 21 : 20);
			UserDataFixedSize = bsr.ReadBool();
			UserDataSize = (int)(UserDataFixedSize ? bsr.ReadUInt(12) : 0);
			UserDataSizeBits = (int)(UserDataFixedSize ? bsr.ReadUInt(4) : 0);
			if (DemoRef.Header.NetworkProtocol >= 15)
				Flags = (StringTableFlags)bsr.ReadUInt(DemoInfo.NewDemoProtocol ? 2 : 1);

			GameState.StringTablesManager.AddNewTableInfo(this);
			TableUpdates = new StringTableUpdates(DemoRef, TableName, NumEntries, true);
			TableUpdates.ParseStream(bsr.ForkAndSkip((int)dataLen));
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"name: {TableName}");
			pw.AppendLine($"max entries: {MaxEntries}");
			pw.AppendLine($"number of entries: {NumEntries}");
			pw.AppendLine($"user data fixed size: {UserDataFixedSize}");
			pw.AppendLine($"user data size: {UserDataSize}");
			pw.AppendLine($"user data size in bits: {UserDataSizeBits}");
			if (DemoRef.Header.NetworkProtocol >= 15)
				pw.AppendLine($"flags: {Flags}");
			pw.Append("table update:");
			pw.FutureIndent++;
			pw.AppendLine();
			if (TableUpdates == null)
				pw.Append("table update could not be parsed");
			else
				TableUpdates.PrettyWrite(pw);
			pw.FutureIndent--;
		}

		public override void XMLWrite(XElement parent)
		{
			parent.Add(new XElement("SvcCreateStringTable"));
			// TODO: this
		}
	}


	[Flags]
	public enum StringTableFlags : uint { // hl2sdk-portal2  public/networkstringtabledefs.h   line 60
		None                   = 0,
		DataCompressed         = 1,
		DictionaryMaybeEnabled = 1 << 1, // merely a suggestion

		Fake                   = 1 << 30 // I created this SvcCreateStringTable object myself and it's fake
	}
}
