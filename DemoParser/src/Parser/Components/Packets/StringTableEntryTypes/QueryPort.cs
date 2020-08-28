using System;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Packets.StringTableEntryTypes {
	
	public class QueryPort : StringTableEntryData {
		
		internal override bool InlineToString => true;
		public uint Port;


		public QueryPort(SourceDemo demoRef, BitStreamReader reader) : base(demoRef, reader) {}
		

		internal override void ParseStream(BitStreamReader bsr) {
			Port = bsr.ReadUInt();
		}


		internal override void WriteToStreamWriter(BitStreamWriter bsw) {
			throw new NotImplementedException();
		}


		public override void AppendToWriter(IndentedWriter iw) {
			iw.Append(Port.ToString());
		}
	}
}