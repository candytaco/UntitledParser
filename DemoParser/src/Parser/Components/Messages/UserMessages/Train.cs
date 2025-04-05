using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class Train : UserMessage {

		public byte Pos;


		public Train(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Pos = bsr.ReadByte();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"pos (byte): {Pos}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("Train");
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
