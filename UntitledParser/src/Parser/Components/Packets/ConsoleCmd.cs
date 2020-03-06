using UntitledParser.Parser.Components.Abstract;
using UntitledParser.Utils;

namespace UntitledParser.Parser.Components.Packets {
	
	public class ConsoleCmd : DemoPacket {
		
		public string Command;
		
		
		public ConsoleCmd(SourceDemo demoRef, BitStreamReader reader, int tick) : base(demoRef, reader, tick) {}


		internal override void ParseStream(BitStreamReader bsr) {
			bsr.SkipBytes(4); // size of packet
			Command = bsr.ReadNullTerminatedString();
			SetLocalStreamEnd(bsr);
		}
		

		internal override void WriteToStreamWriter(BitStreamWriter bsw) {
			throw new System.NotImplementedException();
		}


		internal override void AppendToWriter(IndentedWriter iw) {
			iw += Command;
		}
	}
}